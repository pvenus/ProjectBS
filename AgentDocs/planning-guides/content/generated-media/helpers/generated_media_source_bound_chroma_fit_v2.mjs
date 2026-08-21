import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { canonicalJson, serializePngRgba8 } from
  "./generated_media_canonical_serializers_v1.mjs";
import { decodePngRgb8, deriveSourceEvidence } from
  "./generated_media_source_bound_chroma_uncomposite_v1.mjs";
import { placeOnCanvas, resizePremultipliedBox } from
  "./generated_media_source_bound_chroma_fit_v1.mjs";

export const PROFILE_KEY =
  "projectbs_character_open_ink_source_bound_green_carrier_fit@2.0.0";
export const PROFILE_PAYLOAD_SHA256 =
  "84db44afba6bce328a51f078f2147055846f282de71b2c56b9d7876264f9bccf";
export const G3_EDIT_PROFILE_KEY =
  "projectbs_character_open_ink_source_bound_green_carrier_fit_g3_edit@1.0.0";
export const G3_EDIT_PROFILE_PAYLOAD_SHA256 =
  "f1b9563f271334c5addbf780bec1bca886f540d1a804e93684f56774c516a086";
const REGISTERED_PROFILE_HASHES = new Map([
  [PROFILE_KEY, PROFILE_PAYLOAD_SHA256],
  [G3_EDIT_PROFILE_KEY, G3_EDIT_PROFILE_PAYLOAD_SHA256],
]);

const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const maskSha = (mask) => sha(Buffer.from(mask));
const roundHalfUp = (numerator, denominator) =>
  Math.floor((numerator + Math.floor(denominator / 2)) / denominator);
const roundSigned = (numerator, denominator) => numerator >= 0
  ? roundHalfUp(numerator, denominator)
  : -roundHalfUp(-numerator, denominator);
const clamp8 = (value) => Math.max(0, Math.min(255, value));

const visitNeighbors = (index, width, height, visitor) => {
  const x = index % width;
  if (index >= width) visitor(index - width);
  if (index < width * (height - 1)) visitor(index + width);
  if (x > 0) visitor(index - 1);
  if (x < width - 1) visitor(index + 1);
};

function rootMapSha(rootMap) {
  const bytes = Buffer.alloc(rootMap.length * 4);
  for (let index = 0; index < rootMap.length; index += 1)
    bytes.writeInt32LE(rootMap[index], index * 4);
  return sha(bytes);
}

export function deriveCarrierModels({ width, height, evidence }) {
  const pixels = width * height;
  const queue = new Int32Array(pixels);
  const fringeDistance = new Int32Array(pixels);
  const fringeRoot = new Int32Array(pixels);
  fringeDistance.fill(-1); fringeRoot.fill(-1);
  let head = 0; let tail = 0;
  for (let index = 0; index < pixels; index += 1) {
    if (!evidence.combinedCarrier[index]) continue;
    fringeDistance[index] = 0; fringeRoot[index] = index; queue[tail++] = index;
  }
  while (head < tail) {
    const index = queue[head++];
    visitNeighbors(index, width, height, (next) => {
      if (fringeDistance[next] < 0 && evidence.excess[next] > 0) {
        fringeDistance[next] = fringeDistance[index] + 1;
        fringeRoot[next] = fringeRoot[index];
        queue[tail++] = next;
      }
    });
  }
  const fringeMask = new Uint8Array(pixels);
  let fringePixelCount = 0; let maxGraphDistance = 0;
  for (let index = 0; index < pixels; index += 1) {
    if (fringeDistance[index] <= 0) continue;
    fringeMask[index] = 1; fringePixelCount += 1;
    maxGraphDistance = Math.max(maxGraphDistance, fringeDistance[index]);
  }

  const fullDistance = new Int32Array(pixels);
  const fullRoot = new Int32Array(pixels);
  fullDistance.fill(-1); fullRoot.fill(-1); head = 0; tail = 0;
  for (let index = 0; index < pixels; index += 1) {
    if (!evidence.combinedCarrier[index]) continue;
    fullDistance[index] = 0; fullRoot[index] = index; queue[tail++] = index;
  }
  while (head < tail) {
    const index = queue[head++];
    visitNeighbors(index, width, height, (next) => {
      if (fullDistance[next] < 0) {
        fullDistance[next] = fullDistance[index] + 1;
        fullRoot[next] = fullRoot[index];
        queue[tail++] = next;
      }
    });
  }
  return { fringeDistance, fringeRoot, fringeMask, fullRoot,
    measured: { fringePixelCount, maxGraphDistance,
      sourceFringeMaskSha256: maskSha(fringeMask),
      nearestCarrierRootMapSha256: rootMapSha(fringeRoot),
      fullNearestCarrierRootMapSha256: rootMapSha(fullRoot) } };
}

export function recoverExpandedFringe({ width, height, rgb, evidence, models }) {
  const pixels = width * height;
  const rgba = Buffer.alloc(pixels * 4);
  let protectedRgbMismatchCount = 0;
  for (let index = 0; index < pixels; index += 1) {
    const sourceOffset = index * 3;
    const outputOffset = index * 4;
    if (evidence.combinedCarrier[index]) continue;
    if (models.fringeDistance[index] > 0) {
      const alpha = Math.max(1, Math.min(254, roundHalfUp(
        255 * (evidence.floor - evidence.excess[index]), evidence.floor)));
      const backgroundOffset = models.fringeRoot[index] * 3;
      for (let channel = 0; channel < 3; channel += 1)
        rgba[outputOffset + channel] = clamp8(roundSigned(
          255 * rgb[sourceOffset + channel]
          - (255 - alpha) * rgb[backgroundOffset + channel], alpha));
      rgba[outputOffset + 3] = alpha;
    } else {
      rgba[outputOffset] = rgb[sourceOffset];
      rgba[outputOffset + 1] = rgb[sourceOffset + 1];
      rgba[outputOffset + 2] = rgb[sourceOffset + 2];
      rgba[outputOffset + 3] = 255;
      if (rgba[outputOffset] !== rgb[sourceOffset]
        || rgba[outputOffset + 1] !== rgb[sourceOffset + 1]
        || rgba[outputOffset + 2] !== rgb[sourceOffset + 2])
        protectedRgbMismatchCount += 1;
    }
  }
  return { rgba, protectedRgbMismatchCount };
}

function areaModelAt({ rgb, fullRoot, sourceWidth, sourceBbox,
  targetWidth, targetHeight, dx, dy }) {
  const sourceBoxWidth = sourceBbox.xMax - sourceBbox.xMin + 1;
  const sourceBoxHeight = sourceBbox.yMax - sourceBbox.yMin + 1;
  const xStart = dx * sourceBoxWidth; const xEnd = (dx + 1) * sourceBoxWidth;
  const yStart = dy * sourceBoxHeight; const yEnd = (dy + 1) * sourceBoxHeight;
  const composite = [0, 0, 0]; const background = [0, 0, 0];
  let totalWeight = 0;
  for (let sy = Math.floor(yStart / targetHeight);
    sy < Math.ceil(yEnd / targetHeight); sy += 1) {
    const overlapY = Math.min(yEnd, (sy + 1) * targetHeight)
      - Math.max(yStart, sy * targetHeight);
    for (let sx = Math.floor(xStart / targetWidth);
      sx < Math.ceil(xEnd / targetWidth); sx += 1) {
      const overlapX = Math.min(xEnd, (sx + 1) * targetWidth)
        - Math.max(xStart, sx * targetWidth);
      const weight = overlapX * overlapY;
      const index = (sourceBbox.yMin + sy) * sourceWidth + sourceBbox.xMin + sx;
      const sourceOffset = index * 3;
      const backgroundOffset = fullRoot[index] * 3;
      totalWeight += weight;
      for (let channel = 0; channel < 3; channel += 1) {
        composite[channel] += rgb[sourceOffset + channel] * weight;
        background[channel] += rgb[backgroundOffset + channel] * weight;
      }
    }
  }
  return { composite: composite.map((value) => roundHalfUp(value, totalWeight)),
    background: background.map((value) => roundHalfUp(value, totalWeight)) };
}

export function removeTargetResidualCarrier({ resizedRgba, rgb, fullRoot,
  sourceWidth, sourceBbox, targetWidth, targetHeight }) {
  const output = Buffer.from(resizedRgba);
  const residualMask = new Uint8Array(targetWidth * targetHeight);
  const clearedMask = new Uint8Array(targetWidth * targetHeight);
  let residualPixelCount = 0; let solvedPixelCount = 0; let clearedPixelCount = 0;
  let postResidualPartialAlphaPositiveGreenCount = 0;
  for (let y = 0; y < targetHeight; y += 1) {
    for (let x = 0; x < targetWidth; x += 1) {
      const pixel = y * targetWidth + x;
      const offset = pixel * 4;
      const initialAlpha = output[offset + 3];
      if (initialAlpha <= 0 || initialAlpha >= 255
        || output[offset + 1] <= Math.max(output[offset], output[offset + 2]))
        continue;
      residualMask[pixel] = 1; residualPixelCount += 1;
      const model = areaModelAt({ rgb, fullRoot, sourceWidth, sourceBbox,
        targetWidth, targetHeight, dx: x, dy: y });
      let solution;
      for (let alpha = initialAlpha; alpha >= 1; alpha -= 1) {
        const raw = model.composite.map((value, channel) =>
          (255 * value - (255 - alpha) * model.background[channel]) / alpha);
        if (raw.some((value) => value < 0 || value > 255)) continue;
        const foreground = raw.map((value) => Math.round(value));
        const recomposed = foreground.map((value, channel) => Math.round(
          (alpha * value + (255 - alpha) * model.background[channel]) / 255));
        const error = Math.max(...recomposed.map((value, channel) =>
          Math.abs(value - model.composite[channel])));
        if (foreground[1] <= Math.max(foreground[0], foreground[2]) && error <= 1) {
          solution = { alpha, foreground };
          break;
        }
      }
      if (solution) {
        output[offset] = solution.foreground[0];
        output[offset + 1] = solution.foreground[1];
        output[offset + 2] = solution.foreground[2];
        output[offset + 3] = solution.alpha;
        solvedPixelCount += 1;
      } else {
        output.fill(0, offset, offset + 4);
        clearedMask[pixel] = 1; clearedPixelCount += 1;
      }
    }
  }
  for (let offset = 0; offset < output.length; offset += 4) {
    if (output[offset + 3] === 0) output.fill(0, offset, offset + 3);
    else if (output[offset + 3] < 255
      && output[offset + 1] > Math.max(output[offset], output[offset + 2]))
      postResidualPartialAlphaPositiveGreenCount += 1;
  }
  return { rgba: output, measured: { residualPixelCount, solvedPixelCount,
    clearedPixelCount, residualTargetMaskSha256: maskSha(residualMask),
    residualClearedTargetMaskSha256: maskSha(clearedMask),
    postResidualPartialAlphaPositiveGreenCount } };
}

export function validateProfile(profile) {
  const expectedHash = REGISTERED_PROFILE_HASHES.get(profile.profileKey);
  if (!expectedHash || sha(Buffer.from(canonicalJson(profile), "utf8")) !== expectedHash)
    throw new Error("source_chroma_fit_v2_profile_hash_mismatch");
  return profile;
}

export function validateOutputContract({ rgba, width, height, outputContract }) {
  let xMin = width; let yMin = height; let xMax = -1; let yMax = -1;
  let alphaMin = 255; let alphaMax = 0;
  let partialAlphaPositiveGreenExcessCount = 0;
  for (let y = 0; y < height; y += 1) {
    for (let x = 0; x < width; x += 1) {
      const offset = (y * width + x) * 4;
      const alpha = rgba[offset + 3];
      alphaMin = Math.min(alphaMin, alpha); alphaMax = Math.max(alphaMax, alpha);
      if (alpha === 0) {
        if (rgba[offset] !== 0 || rgba[offset + 1] !== 0 || rgba[offset + 2] !== 0)
          throw new Error("source_chroma_fit_v2_transparent_rgb_nonzero");
      } else {
        xMin = Math.min(xMin, x); yMin = Math.min(yMin, y);
        xMax = Math.max(xMax, x); yMax = Math.max(yMax, y);
        if (alpha < 255 && rgba[offset + 1] > Math.max(rgba[offset], rgba[offset + 2]))
          partialAlphaPositiveGreenExcessCount += 1;
      }
      if ((x === 0 || y === 0 || x === width - 1 || y === height - 1) && alpha !== 0)
        throw new Error("source_chroma_fit_v2_border_alpha_nonzero");
    }
  }
  const foregroundBbox = { xMin, yMin, xMax, yMax };
  if (width !== outputContract.canvas.width || height !== outputContract.canvas.height
    || alphaMin !== outputContract.alphaMin || alphaMax !== outputContract.alphaMax
    || canonicalJson(foregroundBbox) !== canonicalJson(outputContract.foregroundBbox))
    throw new Error("source_chroma_fit_v2_output_geometry_mismatch");
  if (partialAlphaPositiveGreenExcessCount
    !== outputContract.partialAlphaPositiveGreenExcessCount)
    throw new Error("source_chroma_fit_v2_output_fringe_mismatch");
  return { alphaMin, alphaMax, foregroundBbox,
    partialAlphaPositiveGreenExcessCount, transparentPixelRgb: [0, 0, 0],
    cornersAndFullBorderAlpha: 0, noClipping: true };
}

export function executeRecoveryFitV2({ profile, sourceBytes, receiptBytes }) {
  validateProfile(profile);
  const binding = profile.sourceBinding;
  const receiptSha256 = binding.generationReceiptSha256
    ?? binding.editExecutionReceiptSha256;
  if (sha(sourceBytes) !== binding.sourceSha256 || sourceBytes.length !== binding.byteLength
    || sha(receiptBytes) !== receiptSha256)
    throw new Error("source_chroma_fit_v2_binding_not_registered");
  const receipt = JSON.parse(receiptBytes.toString("utf8"));
  const generationReceiptValid = binding.generationReceiptSha256
    && receipt.generationHandoffSha256 === binding.generationHandoffSha256;
  const editReceiptValid = binding.editExecutionReceiptSha256
    && receipt.executionHandoffSha256 === binding.editExecutionHandoffSha256
    && receipt.executionScopeHash === binding.editExecutionScopeHash
    && receipt.routeId === binding.routeId
    && receipt.routeRecordSha256 === binding.routeRecordSha256;
  if (receipt.providerOutputSha256 !== binding.sourceSha256
    || receipt.requestId !== binding.requestId || (!generationReceiptValid && !editReceiptValid)
    || receipt.idempotencyKey !== binding.idempotencyKey
    || receipt.submitCount !== 1 || receipt.retryCount !== 0
    || receipt.state !== "output_nonconformant_no_retry")
    throw new Error("source_chroma_fit_v2_generation_receipt_mismatch");
  const decoded = decodePngRgb8(sourceBytes);
  const evidence = deriveSourceEvidence(decoded);
  const models = deriveCarrierModels({ ...decoded, evidence });
  const sourceExpected = profile.algorithmContract.sourceFringe;
  const sourceEvidenceExpected = {
    fringePixelCount: sourceExpected.pixelCount,
    maxGraphDistance: sourceExpected.maxGraphDistance,
    sourceFringeMaskSha256: sourceExpected.sourceFringeMaskSha256,
    nearestCarrierRootMapSha256: sourceExpected.nearestCarrierRootMapSha256,
    fullNearestCarrierRootMapSha256:
      profile.algorithmContract.targetResidual.fullNearestCarrierRootMapSha256,
  };
  for (const member of Object.keys(sourceEvidenceExpected))
    if (models.measured[member] !== sourceEvidenceExpected[member])
      throw new Error("source_chroma_fit_v2_fringe_evidence_mismatch");
  const recovered = recoverExpandedFringe({ ...decoded, evidence, models });
  if (recovered.protectedRgbMismatchCount !== 0)
    throw new Error("source_chroma_fit_v2_protected_information_mismatch");
  const fit = profile.algorithmContract.fit;
  const resized = resizePremultipliedBox({ rgba: recovered.rgba,
    sourceWidth: decoded.width, sourceHeight: decoded.height,
    sourceBbox: fit.sourceForegroundBbox, targetWidth: fit.targetSize.width,
    targetHeight: fit.targetSize.height });
  const residual = removeTargetResidualCarrier({ resizedRgba: resized,
    rgb: decoded.rgb, fullRoot: models.fullRoot, sourceFringeMask: models.fringeMask,
    sourceWidth: decoded.width,
    sourceBbox: fit.sourceForegroundBbox, targetWidth: fit.targetSize.width,
    targetHeight: fit.targetSize.height });
  const residualExpected = profile.algorithmContract.targetResidual;
  for (const [measured, expected] of [["residualPixelCount",
    "preResidualPartialAlphaPositiveGreenCount"], ["solvedPixelCount",
    "residualSolvedPixelCount"], ["clearedPixelCount", "residualClearedPixelCount"],
  ["residualTargetMaskSha256", "residualTargetMaskSha256"],
  ["residualClearedTargetMaskSha256", "residualClearedTargetMaskSha256"],
  ["postResidualPartialAlphaPositiveGreenCount",
    "postResidualPartialAlphaPositiveGreenCount"]])
    if (residual.measured[measured] !== residualExpected[expected])
      throw new Error("source_chroma_fit_v2_residual_evidence_mismatch");
  const canvasRgba = placeOnCanvas({ resizedRgba: residual.rgba,
    targetWidth: fit.targetSize.width, targetHeight: fit.targetSize.height,
    canvasWidth: fit.canvas.width, canvasHeight: fit.canvas.height,
    x: fit.placement.x, y: fit.placement.y });
  const outputValidation = validateOutputContract({ rgba: canvasRgba,
    width: fit.canvas.width, height: fit.canvas.height,
    outputContract: profile.outputContract });
  const outputBytes = serializePngRgba8({ width: fit.canvas.width,
    height: fit.canvas.height, rgba: canvasRgba });
  if ((profile.outputContract.outputPngSha256
      && sha(outputBytes) !== profile.outputContract.outputPngSha256)
    || (profile.outputContract.outputPngByteLength
      && outputBytes.length !== profile.outputContract.outputPngByteLength)
    || (profile.outputContract.outputRgbaSha256
      && sha(canvasRgba) !== profile.outputContract.outputRgbaSha256))
    throw new Error("source_chroma_fit_v2_output_identity_mismatch");
  return { outputBytes, canvasRgba, evidence: { source: models.measured,
    residual: residual.measured, outputValidation }, profileKey: profile.profileKey,
    profilePayloadSha256: REGISTERED_PROFILE_HASHES.get(profile.profileKey),
    providerCalled: false, submitCount: 0,
    retryCount: 0 };
}

function main(argv) {
  if (argv.length !== 5) throw new Error("usage: node generated_media_source_bound_chroma_fit_v2.mjs profile.json source.png generation-receipt.json output.png output.receipt.json");
  const [profilePath, sourcePath, receiptPath, outputPath, outputReceiptPath] = argv;
  if (existsSync(outputPath) || existsSync(outputReceiptPath))
    throw new Error("source_chroma_fit_v2_output_collision");
  const result = executeRecoveryFitV2({ profile: JSON.parse(readFileSync(profilePath, "utf8")),
    sourceBytes: readFileSync(sourcePath), receiptBytes: readFileSync(receiptPath) });
  writeFileSync(outputPath, result.outputBytes, { flag: "wx" });
  const receipt = { schemaVersion: "generated_media_source_bound_chroma_fit_receipt_v2",
    state: "fit_complete", profileKey: result.profileKey,
    profilePayloadSha256: result.profilePayloadSha256,
    sourceSha256: sha(readFileSync(sourcePath)), outputSha256: sha(result.outputBytes),
    evidence: result.evidence, providerCalled: false, submitCount: 0, retryCount: 0,
    evaluationStatus: "not_evaluated", projectCopyEligible: false };
  writeFileSync(outputReceiptPath, `${canonicalJson(receipt)}\n`, { flag: "wx" });
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  try { main(process.argv.slice(2)); }
  catch (error) { process.stderr.write(`${error.message}\n`); process.exitCode = 1; }
}
