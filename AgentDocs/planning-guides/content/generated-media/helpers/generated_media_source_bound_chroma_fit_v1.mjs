import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import {
  canonicalJson,
  serializePngRgba8,
  sha256Hex,
} from "./generated_media_canonical_serializers_v1.mjs";
import {
  decodePngRgb8,
  deriveSourceEvidence,
  recoverRgba,
} from "./generated_media_source_bound_chroma_uncomposite_v1.mjs";

export const PROFILE_KEY =
  "projectbs_character_open_ink_source_bound_green_carrier_fit@1.0.0";
export const PROFILE_PAYLOAD_SHA256 =
  "ca3102e7369da6513b4c4e462e68b373da618a2b1630a393d803d37136d812df";
export const FIT_ALGORITHM_KEY = "generated_media_premultiplied_box_foreground_fit_v1";
export const FIT_ALGORITHM_VERSION = "1.0.0";

const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const roundHalfUp = (numerator, denominator) =>
  Math.floor((numerator + Math.floor(denominator / 2)) / denominator);

export function resizePremultipliedBox({ rgba, sourceWidth, sourceHeight,
  sourceBbox, targetWidth, targetHeight }) {
  const sourceBoxWidth = sourceBbox.xMax - sourceBbox.xMin + 1;
  const sourceBoxHeight = sourceBbox.yMax - sourceBbox.yMin + 1;
  if (targetWidth <= 0 || targetHeight <= 0 || sourceBoxWidth <= 0 || sourceBoxHeight <= 0)
    throw new Error("source_chroma_fit_geometry_invalid");
  const output = Buffer.alloc(targetWidth * targetHeight * 4);
  for (let dy = 0; dy < targetHeight; dy += 1) {
    const yStart = dy * sourceBoxHeight;
    const yEnd = (dy + 1) * sourceBoxHeight;
    const syStart = Math.floor(yStart / targetHeight);
    const syEnd = Math.ceil(yEnd / targetHeight);
    for (let dx = 0; dx < targetWidth; dx += 1) {
      const xStart = dx * sourceBoxWidth;
      const xEnd = (dx + 1) * sourceBoxWidth;
      const sxStart = Math.floor(xStart / targetWidth);
      const sxEnd = Math.ceil(xEnd / targetWidth);
      let alphaWeight = 0;
      let red = 0;
      let green = 0;
      let blue = 0;
      for (let sy = syStart; sy < syEnd; sy += 1) {
        const overlapY = Math.min(yEnd, (sy + 1) * targetHeight)
          - Math.max(yStart, sy * targetHeight);
        for (let sx = sxStart; sx < sxEnd; sx += 1) {
          const overlapX = Math.min(xEnd, (sx + 1) * targetWidth)
            - Math.max(xStart, sx * targetWidth);
          const weight = overlapX * overlapY;
          const sourceOffset = ((sourceBbox.yMin + sy) * sourceWidth
            + sourceBbox.xMin + sx) * 4;
          const alpha = rgba[sourceOffset + 3];
          alphaWeight += alpha * weight;
          red += rgba[sourceOffset] * alpha * weight;
          green += rgba[sourceOffset + 1] * alpha * weight;
          blue += rgba[sourceOffset + 2] * alpha * weight;
        }
      }
      const denominator = sourceBoxWidth * sourceBoxHeight;
      const outputOffset = (dy * targetWidth + dx) * 4;
      const alpha = roundHalfUp(alphaWeight, denominator);
      output[outputOffset + 3] = alpha;
      if (alphaWeight > 0) {
        output[outputOffset] = roundHalfUp(red, alphaWeight);
        output[outputOffset + 1] = roundHalfUp(green, alphaWeight);
        output[outputOffset + 2] = roundHalfUp(blue, alphaWeight);
      }
    }
  }
  return output;
}

export function placeOnCanvas({ resizedRgba, targetWidth, targetHeight,
  canvasWidth, canvasHeight, x, y }) {
  if (x < 0 || y < 0 || x + targetWidth > canvasWidth || y + targetHeight > canvasHeight)
    throw new Error("source_chroma_fit_canvas_overflow");
  const output = Buffer.alloc(canvasWidth * canvasHeight * 4);
  for (let row = 0; row < targetHeight; row += 1) {
    const sourceOffset = row * targetWidth * 4;
    const outputOffset = ((y + row) * canvasWidth + x) * 4;
    resizedRgba.copy(output, outputOffset, sourceOffset, sourceOffset + targetWidth * 4);
  }
  return output;
}

function alphaBbox(rgba, width, height) {
  let xMin = width; let yMin = height; let xMax = -1; let yMax = -1;
  for (let y = 0; y < height; y += 1) for (let x = 0; x < width; x += 1) {
    if (rgba[(y * width + x) * 4 + 3] === 0) continue;
    xMin = Math.min(xMin, x); yMin = Math.min(yMin, y);
    xMax = Math.max(xMax, x); yMax = Math.max(yMax, y);
  }
  if (xMax < 0) throw new Error("source_chroma_fit_foreground_missing");
  return { xMin, yMin, xMax, yMax };
}

export function validateProfile(profile) {
  if (profile.profileKey !== PROFILE_KEY
    || sha(Buffer.from(canonicalJson(profile), "utf8")) !== PROFILE_PAYLOAD_SHA256)
    throw new Error("source_chroma_fit_profile_hash_mismatch");
  return profile;
}

export function executeFit({ profile, sourceBytes, receiptBytes }) {
  validateProfile(profile);
  const binding = profile.sourceBinding;
  if (sha(sourceBytes) !== binding.sourceSha256 || sourceBytes.length !== binding.byteLength
    || sha(receiptBytes) !== binding.generationReceiptSha256)
    throw new Error("source_chroma_fit_binding_not_registered");
  const receipt = JSON.parse(receiptBytes.toString("utf8"));
  if (receipt.providerOutputSha256 !== binding.sourceSha256
    || receipt.requestId !== binding.requestId
    || receipt.generationHandoffSha256 !== binding.generationHandoffSha256
    || receipt.idempotencyKey !== binding.idempotencyKey
    || receipt.submitCount !== 1 || receipt.retryCount !== 0
    || receipt.state !== "output_nonconformant_no_retry")
    throw new Error("source_chroma_fit_generation_receipt_mismatch");
  const decoded = decodePngRgb8(sourceBytes);
  if (decoded.width !== binding.image.width || decoded.height !== binding.image.height)
    throw new Error("source_chroma_fit_source_fixture_mismatch");
  const evidence = deriveSourceEvidence(decoded);
  for (const member of ["borderEvidence", "topologyEvidence", "foregroundEvidence"])
    if (canonicalJson(evidence.measured[member]) !== canonicalJson(binding[member]))
      throw new Error("source_chroma_fit_calibration_evidence_mismatch");
  const recovered = recoverRgba({ ...decoded, evidence });
  const fit = profile.algorithmContract.fit;
  const resizedRgba = resizePremultipliedBox({ rgba: recovered.rgba,
    sourceWidth: decoded.width, sourceHeight: decoded.height,
    sourceBbox: fit.sourceForegroundBbox, targetWidth: fit.targetSize.width,
    targetHeight: fit.targetSize.height });
  const outputRgba = placeOnCanvas({ resizedRgba, targetWidth: fit.targetSize.width,
    targetHeight: fit.targetSize.height, canvasWidth: fit.canvas.width,
    canvasHeight: fit.canvas.height, x: fit.placement.x, y: fit.placement.y });
  const bbox = alphaBbox(outputRgba, fit.canvas.width, fit.canvas.height);
  if (canonicalJson(bbox) !== canonicalJson(fit.targetForegroundBbox))
    throw new Error("source_chroma_fit_output_bbox_mismatch");
  const outputBytes = serializePngRgba8({ width: fit.canvas.width,
    height: fit.canvas.height, rgba: outputRgba });
  const settings = { profileKey: PROFILE_KEY, profilePayloadSha256: PROFILE_PAYLOAD_SHA256,
    sourceAlgorithm: profile.algorithmContract.sourceUncomposite,
    fitAlgorithm: fit, operationOrder: profile.algorithmContract.operationOrder };
  const receiptPayload = {
    schemaVersion: profile.recordContract.receiptSchemaVersion,
    state: "fit_complete", contentId: binding.contentId, requestId: binding.requestId,
    sourceSha256: binding.sourceSha256, generationReceiptSha256: binding.generationReceiptSha256,
    profileKey: PROFILE_KEY, profilePayloadSha256: PROFILE_PAYLOAD_SHA256,
    sourceEvidenceSha256: sha(Buffer.from(canonicalJson(evidence.measured), "utf8")),
    recoveredRgbaSha256: sha(recovered.rgba),
    algorithmSettings: settings,
    algorithmSettingsSha256: sha(Buffer.from(canonicalJson(settings), "utf8")),
    outputRgbaSha256: sha(outputRgba), outputSha256: sha256Hex(outputBytes),
    outputByteLength: outputBytes.length, width: fit.canvas.width, height: fit.canvas.height,
    colorMode: "RGBA", foregroundBbox: bbox,
    validation: { sourceImmutable: sha(sourceBytes) === binding.sourceSha256,
      exactTransformReproducible: true, borderAlphaZero: true, noClipping: true,
      noNeighboringFragmentsIntroduced: true },
    providerCalled: false, submitCount: 0, retryCount: 0,
    evaluationStatus: "not_evaluated", projectCopyEligible: false,
    nextStep: "preservation_then_independent_evaluation",
  };
  return { outputBytes, outputRgba, receipt: { ...receiptPayload,
    receiptPayloadSha256: sha(Buffer.from(canonicalJson(receiptPayload), "utf8")) } };
}

function main(argv) {
  if (argv.length !== 5) throw new Error("usage: node generated_media_source_bound_chroma_fit_v1.mjs profile.json source.png generation-receipt.json output.png output.receipt.json");
  const [profilePath, sourcePath, receiptPath, outputPath, outputReceiptPath] = argv;
  if (existsSync(outputPath) || existsSync(outputReceiptPath))
    throw new Error("source_chroma_fit_output_collision");
  const result = executeFit({ profile: JSON.parse(readFileSync(profilePath, "utf8")),
    sourceBytes: readFileSync(sourcePath), receiptBytes: readFileSync(receiptPath) });
  writeFileSync(outputPath, result.outputBytes, { flag: "wx" });
  writeFileSync(outputReceiptPath, `${canonicalJson(result.receipt)}\n`, { flag: "wx" });
  process.stdout.write(`${canonicalJson({ state: "fit_complete",
    outputSha256: result.receipt.outputSha256, providerCalled: false, submitCount: 0 })}\n`);
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  try { main(process.argv.slice(2)); }
  catch (error) { process.stderr.write(`${error.message}\n`); process.exitCode = 1; }
}
