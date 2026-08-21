import { createHash } from "node:crypto";
import { readFileSync, writeFileSync, existsSync } from "node:fs";
import { inflateSync } from "node:zlib";
import { fileURLToPath } from "node:url";
import {
  canonicalJson,
  serializePngRgba8,
  sha256Hex,
} from "./generated_media_canonical_serializers_v1.mjs";

export const PROFILE_KEY =
  "projectbs_character_open_ink_source_bound_green_carrier_uncomposite@1.0.0";
export const PROFILE_PAYLOAD_SHA256 =
  "b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746";
export const ALGORITHM_KEY = "generated_media_border_calibrated_green_uncomposite_v1";
export const ALGORITHM_VERSION = "1.0.0";

const PNG_SIGNATURE = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const maskSha = (mask) => sha(Buffer.from(mask));
const clamp8 = (value) => Math.max(0, Math.min(255, value));

const crcTable = Array.from({ length: 256 }, (_, value) => {
  let crc = value;
  for (let bit = 0; bit < 8; bit += 1)
    crc = (crc & 1) ? (0xedb88320 ^ (crc >>> 1)) : (crc >>> 1);
  return crc >>> 0;
});
const crc32 = (bytes) => {
  let crc = 0xffffffff;
  for (const byte of bytes) crc = crcTable[(crc ^ byte) & 0xff] ^ (crc >>> 8);
  return (crc ^ 0xffffffff) >>> 0;
};

const paeth = (left, above, upperLeft) => {
  const estimate = left + above - upperLeft;
  const leftDistance = Math.abs(estimate - left);
  const aboveDistance = Math.abs(estimate - above);
  const upperLeftDistance = Math.abs(estimate - upperLeft);
  if (leftDistance <= aboveDistance && leftDistance <= upperLeftDistance) return left;
  if (aboveDistance <= upperLeftDistance) return above;
  return upperLeft;
};

export function decodePngRgb8(bytes) {
  const png = Buffer.from(bytes);
  if (!png.subarray(0, 8).equals(PNG_SIGNATURE)) throw new Error("source_png_signature_invalid");
  let offset = 8;
  let width;
  let height;
  const idat = [];
  const chunkTypes = [];
  while (offset < png.length) {
    if (offset + 12 > png.length) throw new Error("source_png_chunk_truncated");
    const length = png.readUInt32BE(offset);
    const typeBytes = png.subarray(offset + 4, offset + 8);
    const type = typeBytes.toString("ascii");
    const data = png.subarray(offset + 8, offset + 8 + length);
    if (offset + 12 + length > png.length) throw new Error("source_png_chunk_truncated");
    if (png.readUInt32BE(offset + 8 + length) !== crc32(Buffer.concat([typeBytes, data])))
      throw new Error("source_png_crc_mismatch");
    chunkTypes.push(type);
    if (type === "IHDR") {
      if (length !== 13) throw new Error("source_png_ihdr_invalid");
      width = data.readUInt32BE(0);
      height = data.readUInt32BE(4);
      if (data[8] !== 8 || data[9] !== 2 || data[10] !== 0 || data[11] !== 0
        || data[12] !== 0) throw new Error("source_png_rgb8_noninterlaced_required");
    } else if (type === "IDAT") idat.push(data);
    else if (type === "IEND") break;
    offset += 12 + length;
  }
  if (!width || !height || idat.length === 0 || chunkTypes.at(-1) !== "IEND")
    throw new Error("source_png_member_missing");
  const scanlines = inflateSync(Buffer.concat(idat));
  const stride = width * 3;
  if (scanlines.length !== (stride + 1) * height)
    throw new Error("source_png_scanline_length_mismatch");
  const rgb = Buffer.alloc(width * height * 3);
  let inputOffset = 0;
  for (let y = 0; y < height; y += 1) {
    const filter = scanlines[inputOffset];
    inputOffset += 1;
    for (let x = 0; x < stride; x += 1) {
      const raw = scanlines[inputOffset + x];
      const outputOffset = y * stride + x;
      const left = x >= 3 ? rgb[outputOffset - 3] : 0;
      const above = y > 0 ? rgb[outputOffset - stride] : 0;
      const upperLeft = y > 0 && x >= 3 ? rgb[outputOffset - stride - 3] : 0;
      if (filter === 0) rgb[outputOffset] = raw;
      else if (filter === 1) rgb[outputOffset] = (raw + left) & 0xff;
      else if (filter === 2) rgb[outputOffset] = (raw + above) & 0xff;
      else if (filter === 3) rgb[outputOffset] = (raw + Math.floor((left + above) / 2)) & 0xff;
      else if (filter === 4) rgb[outputOffset] = (raw + paeth(left, above, upperLeft)) & 0xff;
      else throw new Error("source_png_filter_invalid");
    }
    inputOffset += stride;
  }
  return { width, height, rgb, sourceChunkTypes: chunkTypes };
}

const neighbors = (index, width, height, visitor) => {
  const x = index % width;
  if (index >= width) visitor(index - width);
  if (index < width * (height - 1)) visitor(index + width);
  if (x > 0) visitor(index - 1);
  if (x < width - 1) visitor(index + 1);
};

const percentileNearest = (sorted, fraction) =>
  sorted[Math.round((sorted.length - 1) * fraction)];

export function deriveSourceEvidence({ width, height, rgb }) {
  const pixels = width * height;
  if (rgb.length !== pixels * 3) throw new Error("source_rgb_length_mismatch");
  const excess = new Int16Array(pixels);
  for (let i = 0; i < pixels; i += 1) {
    const offset = i * 3;
    excess[i] = rgb[offset + 1] - Math.max(rgb[offset], rgb[offset + 2]);
  }
  const borderIndexes = [];
  for (let x = 0; x < width; x += 1) borderIndexes.push(x, (height - 1) * width + x);
  for (let y = 1; y < height - 1; y += 1)
    borderIndexes.push(y * width, y * width + width - 1);
  const borderValues = borderIndexes.map((index) => excess[index]).sort((a, b) => a - b);
  if (borderValues[0] <= 0) throw new Error("source_chroma_outer_perimeter_not_green_dominant");
  const floor = borderValues[0];
  const candidate = new Uint8Array(pixels);
  for (let i = 0; i < pixels; i += 1) candidate[i] = excess[i] >= floor ? 1 : 0;

  const edgeCarrier = new Uint8Array(pixels);
  const queue = new Int32Array(pixels);
  let head = 0;
  let tail = 0;
  for (const index of borderIndexes) {
    if (!candidate[index] || edgeCarrier[index]) continue;
    edgeCarrier[index] = 1;
    queue[tail++] = index;
  }
  while (head < tail) {
    const index = queue[head++];
    neighbors(index, width, height, (next) => {
      if (candidate[next] && !edgeCarrier[next]) {
        edgeCarrier[next] = 1;
        queue[tail++] = next;
      }
    });
  }
  const edgeCarrierPixelCount = tail;
  if (!borderIndexes.every((index) => edgeCarrier[index] === 1))
    throw new Error("source_chroma_outer_perimeter_carrier_disconnected");

  const enclosedCarrier = new Uint8Array(pixels);
  const componentSizes = [];
  for (let seed = 0; seed < pixels; seed += 1) {
    if (!candidate[seed] || edgeCarrier[seed] || enclosedCarrier[seed]) continue;
    head = 0;
    tail = 0;
    enclosedCarrier[seed] = 1;
    queue[tail++] = seed;
    while (head < tail) {
      const index = queue[head++];
      neighbors(index, width, height, (next) => {
        if (candidate[next] && !edgeCarrier[next] && !enclosedCarrier[next]) {
          enclosedCarrier[next] = 1;
          queue[tail++] = next;
        }
      });
    }
    componentSizes.push(tail);
  }

  const combinedCarrier = new Uint8Array(pixels);
  const oneRing = new Uint8Array(pixels);
  for (let i = 0; i < pixels; i += 1)
    combinedCarrier[i] = edgeCarrier[i] || enclosedCarrier[i] ? 1 : 0;
  for (let index = 0; index < pixels; index += 1) {
    if (!combinedCarrier[index]) continue;
    neighbors(index, width, height, (next) => {
      if (!combinedCarrier[next] && excess[next] > 0) oneRing[next] = 1;
    });
  }
  const ringValues = [];
  let enclosedCarrierPixelCount = 0;
  let oneRingPixelCount = 0;
  let xMin = width;
  let yMin = height;
  let xMax = -1;
  let yMax = -1;
  for (let index = 0; index < pixels; index += 1) {
    if (enclosedCarrier[index]) enclosedCarrierPixelCount += 1;
    if (oneRing[index]) {
      oneRingPixelCount += 1;
      ringValues.push(excess[index]);
    }
    if (!combinedCarrier[index]) {
      const x = index % width;
      const y = Math.floor(index / width);
      xMin = Math.min(xMin, x);
      yMin = Math.min(yMin, y);
      xMax = Math.max(xMax, x);
      yMax = Math.max(yMax, y);
    }
  }
  ringValues.sort((a, b) => a - b);
  return {
    floor,
    excess,
    edgeCarrier,
    enclosedCarrier,
    combinedCarrier,
    oneRing,
    measured: {
      borderEvidence: {
        outerPerimeterPixelCount: borderValues.length,
        outerPerimeterAllGreenDominant: true,
        greenExcessMin: floor,
        greenExcessMedian: percentileNearest(borderValues, 0.5),
        greenExcessMax: borderValues.at(-1),
        sourceCalibratedFloor: floor,
      },
      topologyEvidence: {
        edgeCarrierPixelCount,
        edgeCarrierRatioPpm: Math.round(edgeCarrierPixelCount * 1_000_000 / pixels),
        edgeCarrierMaskSha256: maskSha(edgeCarrier),
        enclosedCarrierComponentCount: componentSizes.length,
        enclosedCarrierPixelCount,
        enclosedComponentSizesSha256: sha(Buffer.from(JSON.stringify(componentSizes))),
        enclosedCarrierMaskSha256: maskSha(enclosedCarrier),
        candidateMaskSha256: maskSha(combinedCarrier),
        oneRingPixelCount,
        oneRingGreenExcessMin: ringValues[0],
        oneRingGreenExcessMedian: percentileNearest(ringValues, 0.5),
        oneRingGreenExcessP95: percentileNearest(ringValues, 0.95),
        oneRingGreenExcessMax: ringValues.at(-1),
        oneRingMaskSha256: maskSha(oneRing),
      },
      foregroundEvidence: {
        bbox: { xMin, yMin, xMax, yMax },
        margins: { left: xMin, top: yMin, right: width - 1 - xMax,
          bottom: height - 1 - yMax },
      },
    },
  };
}

const roundSigned = (numerator, denominator) => numerator >= 0
  ? Math.floor((numerator + Math.floor(denominator / 2)) / denominator)
  : -Math.floor((-numerator + Math.floor(denominator / 2)) / denominator);

export function recoverRgba({ width, height, rgb, evidence }) {
  const pixels = width * height;
  const rgba = Buffer.alloc(pixels * 4);
  let transparentPixelCount = 0;
  let partialAlphaPixelCount = 0;
  let nonTransformedRgbMismatchCount = 0;
  let greenFringeCount = 0;
  let newCyanFringeCount = 0;
  let newMagentaFringeCount = 0;
  let rawModelRecompositionErrorMax = 0;
  for (let index = 0; index < pixels; index += 1) {
    const sourceOffset = index * 3;
    const outputOffset = index * 4;
    const source = [rgb[sourceOffset], rgb[sourceOffset + 1], rgb[sourceOffset + 2]];
    if (evidence.combinedCarrier[index]) {
      rgba[outputOffset] = 0;
      rgba[outputOffset + 1] = 0;
      rgba[outputOffset + 2] = 0;
      rgba[outputOffset + 3] = 0;
      transparentPixelCount += 1;
      continue;
    }
    if (!evidence.oneRing[index]) {
      rgba[outputOffset] = source[0];
      rgba[outputOffset + 1] = source[1];
      rgba[outputOffset + 2] = source[2];
      rgba[outputOffset + 3] = 255;
      if (rgba[outputOffset] !== source[0] || rgba[outputOffset + 1] !== source[1]
        || rgba[outputOffset + 2] !== source[2]) nonTransformedRgbMismatchCount += 1;
      continue;
    }
    const excess = evidence.excess[index];
    const alpha = Math.max(1, Math.min(254,
      Math.floor((255 * (evidence.floor - excess) + Math.floor(evidence.floor / 2))
        / evidence.floor)));
    let backgroundIndex = -1;
    let backgroundExcess = -32768;
    neighbors(index, width, height, (next) => {
      if (evidence.combinedCarrier[next]
        && (evidence.excess[next] > backgroundExcess
          || (evidence.excess[next] === backgroundExcess
            && (backgroundIndex < 0 || next < backgroundIndex)))) {
        backgroundIndex = next;
        backgroundExcess = evidence.excess[next];
      }
    });
    if (backgroundIndex < 0) throw new Error("source_chroma_one_ring_model_invalid");
    const backgroundOffset = backgroundIndex * 3;
    const background = [rgb[backgroundOffset], rgb[backgroundOffset + 1],
      rgb[backgroundOffset + 2]];
    const raw = source.map((channel, channelIndex) => clamp8(roundSigned(
      255 * channel - (255 - alpha) * background[channelIndex], alpha)));
    for (let channel = 0; channel < 3; channel += 1) {
      const recomposed = Math.round((alpha * raw[channel]
        + (255 - alpha) * background[channel]) / 255);
      rawModelRecompositionErrorMax = Math.max(rawModelRecompositionErrorMax,
        Math.abs(recomposed - source[channel]));
    }
    const output = [...raw];
    output[1] = Math.max(Math.min(output[0], output[2]),
      Math.min(Math.max(output[0], output[2]), output[1]));
    const sourceCyan = Math.max(0, Math.min(source[1], source[2]) - source[0]);
    const outputCyan = Math.max(0, Math.min(output[1], output[2]) - output[0]);
    if (outputCyan > sourceCyan) output[0] = clamp8(output[0] + outputCyan - sourceCyan);
    output[1] = Math.max(Math.min(output[0], output[2]),
      Math.min(Math.max(output[0], output[2]), output[1]));
    rgba[outputOffset] = output[0];
    rgba[outputOffset + 1] = output[1];
    rgba[outputOffset + 2] = output[2];
    rgba[outputOffset + 3] = alpha;
    partialAlphaPixelCount += 1;
    if (output[1] > Math.max(output[0], output[2])) greenFringeCount += 1;
    if (Math.max(0, Math.min(output[1], output[2]) - output[0]) > sourceCyan)
      newCyanFringeCount += 1;
    const sourceMagenta = Math.max(0, Math.min(source[0], source[2]) - source[1]);
    if (Math.max(0, Math.min(output[0], output[2]) - output[1]) > sourceMagenta)
      newMagentaFringeCount += 1;
  }
  return { rgba, metrics: { transparentPixelCount, partialAlphaPixelCount,
    nonTransformedRgbMismatchCount, greenFringeCount, newCyanFringeCount,
    newMagentaFringeCount, rawModelRecompositionErrorMax } };
}

export function validateFixture(profile, sourceBytes, receiptBytes) {
  if (sha(Buffer.from(canonicalJson(profile))) !== PROFILE_PAYLOAD_SHA256
    || profile.profileKey !== PROFILE_KEY) throw new Error("source_chroma_profile_hash_mismatch");
  const sourceSha256 = sha(sourceBytes);
  const generationReceiptSha256 = sha(receiptBytes);
  const fixture = profile.sourceBindings.find((entry) =>
    entry.sourceSha256 === sourceSha256
      && entry.generationReceiptSha256 === generationReceiptSha256);
  if (!fixture) throw new Error("source_chroma_binding_not_registered");
  if (fixture.byteLength !== sourceBytes.length) throw new Error("source_chroma_source_fixture_mismatch");
  const parsed = JSON.parse(receiptBytes.toString("utf8"));
  if (parsed.providerOutputSha256 !== sourceSha256
    || parsed.requestId !== fixture.requestId
    || parsed.generationHandoffSha256 !== fixture.generationHandoffSha256
    || parsed.idempotencyKey !== fixture.idempotencyKey
    || parsed.submitCount !== 1 || parsed.retryCount !== 0
    || parsed.state !== "output_nonconformant_no_retry")
    throw new Error("source_chroma_generation_receipt_mismatch");
  return fixture;
}

export function validateMeasuredEvidence(fixture, measured) {
  for (const member of ["borderEvidence", "topologyEvidence", "foregroundEvidence"]) {
    if (canonicalJson(fixture[member]) !== canonicalJson(measured[member]))
      throw new Error("source_chroma_calibration_evidence_mismatch");
  }
  return true;
}

export function executeRecovery({ profile, sourceBytes, receiptBytes }) {
  const fixture = validateFixture(profile, sourceBytes, receiptBytes);
  const decoded = decodePngRgb8(sourceBytes);
  if (decoded.width !== fixture.image.width || decoded.height !== fixture.image.height)
    throw new Error("source_chroma_source_fixture_mismatch");
  const evidence = deriveSourceEvidence(decoded);
  validateMeasuredEvidence(fixture, evidence.measured);
  const recovered = recoverRgba({ ...decoded, evidence });
  const { metrics } = recovered;
  if (metrics.transparentPixelCount <= 0 || metrics.partialAlphaPixelCount <= 0
    || metrics.nonTransformedRgbMismatchCount !== 0 || metrics.greenFringeCount !== 0
    || metrics.newCyanFringeCount !== 0 || metrics.newMagentaFringeCount !== 0
    || metrics.rawModelRecompositionErrorMax > 1)
    throw new Error("source_chroma_output_validation_failed");
  const outputBytes = serializePngRgba8({ width: decoded.width, height: decoded.height,
    rgba: recovered.rgba });
  const algorithmSettings = { profileKey: PROFILE_KEY,
    profilePayloadSha256: PROFILE_PAYLOAD_SHA256, algorithmKey: ALGORITHM_KEY,
    algorithmVersion: ALGORITHM_VERSION, sourceCalibratedFloor: evidence.floor,
    connectivity: 4, oneRingPolicy: "exact_positive_four_neighbor_ring_v1",
    transparentRgb: [0, 0, 0], serializerKey: "generated_media_png_rgba8_store_v1",
    serializerVersion: "1.0.0" };
  const receiptPayload = {
    schemaVersion: "generated_media_source_bound_chroma_uncomposite_receipt_v1",
    state: "recovered",
    contentId: fixture.contentId,
    requestId: fixture.requestId,
    sourceSha256: fixture.sourceSha256,
    generationReceiptSha256: fixture.generationReceiptSha256,
    profileKey: PROFILE_KEY,
    profilePayloadSha256: PROFILE_PAYLOAD_SHA256,
    algorithmSettings,
    algorithmSettingsSha256: sha(Buffer.from(canonicalJson(algorithmSettings))),
    sourceEvidence: evidence.measured,
    sourceEvidenceSha256: sha(Buffer.from(canonicalJson(evidence.measured))),
    outputSha256: sha256Hex(outputBytes),
    outputByteLength: outputBytes.length,
    width: decoded.width,
    height: decoded.height,
    colorMode: "RGBA",
    outputValidation: metrics,
    providerCalled: false,
    submitCount: 0,
    retryCount: 0,
    evaluationStatus: "not_evaluated",
    projectCopyEligible: false,
    nextStep: "preservation_then_independent_evaluation",
  };
  return { outputBytes, receipt: { ...receiptPayload,
    receiptPayloadSha256: sha(Buffer.from(canonicalJson(receiptPayload))) } };
}

function main(argv) {
  if (argv.length !== 5) throw new Error(
    "usage: node generated_media_source_bound_chroma_uncomposite_v1.mjs profile.json source.png generation-receipt.json output.png output.receipt.json");
  const [profilePath, sourcePath, receiptPath, outputPath, outputReceiptPath] = argv;
  if (existsSync(outputPath) || existsSync(outputReceiptPath))
    throw new Error("source_chroma_output_collision");
  const result = executeRecovery({
    profile: JSON.parse(readFileSync(profilePath, "utf8")),
    sourceBytes: readFileSync(sourcePath),
    receiptBytes: readFileSync(receiptPath),
  });
  writeFileSync(outputPath, result.outputBytes, { flag: "wx" });
  writeFileSync(outputReceiptPath, `${canonicalJson(result.receipt)}\n`,
    { encoding: "utf8", flag: "wx" });
  process.stdout.write(`${canonicalJson({ state: "recovered", outputSha256:
    result.receipt.outputSha256, receiptPayloadSha256:
    result.receipt.receiptPayloadSha256, providerCalled: false, submitCount: 0 })}\n`);
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  try { main(process.argv.slice(2)); }
  catch (error) { process.stderr.write(`${error.message}\n`); process.exitCode = 1; }
}
