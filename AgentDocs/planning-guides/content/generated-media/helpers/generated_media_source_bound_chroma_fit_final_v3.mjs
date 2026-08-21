import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { canonicalJson, serializePngRgba8 } from
  "./generated_media_canonical_serializers_v1.mjs";
import { decodePngRgb8, deriveSourceEvidence } from
  "./generated_media_source_bound_chroma_uncomposite_v1.mjs";
import { deriveCarrierModels, executeRecoveryFitV2 } from
  "./generated_media_source_bound_chroma_fit_v2.mjs";

export const G2_FINAL_PROFILE_KEY =
  "projectbs_character_open_ink_source_bound_green_carrier_fit@3.0.0";
export const G2_FINAL_PROFILE_PAYLOAD_SHA256 =
  "5188d2bd92fdf22dded70fe8e3ab60f1fee1aa79ac6072845883072d99a875c2";
export const G3_FINAL_PROFILE_KEY =
  "projectbs_character_open_ink_source_bound_green_carrier_fit_g3_edit@2.0.0";
export const G3_FINAL_PROFILE_PAYLOAD_SHA256 =
  "40cf8dcfbdc9043d1cdadeca64ee34ef8a11566140aa1e0ac8cc0d3b5baae425";
const PROFILE_HASHES = new Map([
  [G2_FINAL_PROFILE_KEY, G2_FINAL_PROFILE_PAYLOAD_SHA256],
  [G3_FINAL_PROFILE_KEY, G3_FINAL_PROFILE_PAYLOAD_SHA256],
]);
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const maskSha = (mask) => sha(Buffer.from(mask.buffer, mask.byteOffset, mask.byteLength));
const roundHalfUp = (numerator, denominator) =>
  Math.floor((2 * numerator + denominator) / (2 * denominator));

export function validateFinalProfile(profile) {
  const expected = PROFILE_HASHES.get(profile.profileKey);
  if (!expected || sha(Buffer.from(canonicalJson(profile), "utf8")) !== expected)
    throw new Error("source_chroma_fit_final_profile_hash_mismatch");
  return profile;
}

const neighbors4 = (index, width, height) => {
  const x = index % width; const y = Math.floor(index / width); const result = [];
  if (y > 0) result.push(index - width);
  if (x > 0) result.push(index - 1);
  if (x + 1 < width) result.push(index + 1);
  if (y + 1 < height) result.push(index + width);
  return result;
};

export function connectedComponents(mask, width, height) {
  const seen = new Uint8Array(mask.length); const queue = new Int32Array(mask.length);
  const result = [];
  for (let seed = 0; seed < mask.length; seed += 1) {
    if (!mask[seed] || seen[seed]) continue;
    let head = 0; let tail = 0; queue[tail++] = seed; seen[seed] = 1;
    const pixels = []; let xMin = width; let yMin = height; let xMax = -1; let yMax = -1;
    while (head < tail) {
      const index = queue[head++]; pixels.push(index);
      const x = index % width; const y = Math.floor(index / width);
      xMin = Math.min(xMin, x); xMax = Math.max(xMax, x);
      yMin = Math.min(yMin, y); yMax = Math.max(yMax, y);
      for (const next of neighbors4(index, width, height))
        if (mask[next] && !seen[next]) { seen[next] = 1; queue[tail++] = next; }
    }
    result.push({ pixels, area: pixels.length, bbox: { xMin, yMin, xMax, yMax } });
  }
  return result.sort((left, right) => right.area - left.area
    || left.bbox.yMin - right.bbox.yMin || left.bbox.xMin - right.bbox.xMin);
}

const opaqueStrongGreenMask = (rgba) => {
  const mask = new Uint8Array(rgba.length / 4);
  for (let index = 0; index < mask.length; index += 1) {
    const offset = index * 4;
    if (rgba[offset + 3] === 255
      && rgba[offset + 1] - Math.max(rgba[offset], rgba[offset + 2]) >= 24
      && rgba[offset + 1] >= 96) mask[index] = 1;
  }
  return mask;
};

const alphaMask = (rgba) => {
  const mask = new Uint8Array(rgba.length / 4);
  for (let index = 0; index < mask.length; index += 1)
    if (rgba[index * 4 + 3] > 0) mask[index] = 1;
  return mask;
};

export function cleanExactOpaqueCarrierAndIsolatedAlpha({ rgba, width, height,
  contract }) {
  const output = Buffer.from(rgba);
  const strongMask = opaqueStrongGreenMask(output);
  const components = connectedComponents(strongMask, width, height)
    .map(({ area, bbox }) => ({ area, bbox }));
  if (maskSha(strongMask) !== contract.opaqueCarrierCleanup.maskSha256
    || canonicalJson(components) !== canonicalJson(contract.opaqueCarrierCleanup.components)
    || components.reduce((sum, component) => sum + component.area, 0)
      !== contract.opaqueCarrierCleanup.pixelCount)
    throw new Error("source_chroma_fit_final_opaque_carrier_evidence_mismatch");
  for (let index = 0; index < strongMask.length; index += 1)
    if (strongMask[index]) output.fill(0, index * 4, index * 4 + 4);

  const isolatedMask = new Uint8Array(width * height);
  const isolated = connectedComponents(alphaMask(rgba), width, height)
    .filter(({ area }) => area === contract.isolatedAlphaCleanup.eligibleAreaExact);
  for (const component of isolated) for (const index of component.pixels)
    isolatedMask[index] = 1;
  if (isolated.length !== contract.isolatedAlphaCleanup.eligibleComponentCount
    || maskSha(isolatedMask) !== contract.isolatedAlphaCleanup.eligibleMaskSha256)
    throw new Error("source_chroma_fit_final_isolated_alpha_evidence_mismatch");
  for (let index = 0; index < isolatedMask.length; index += 1)
    if (isolatedMask[index]) output.fill(0, index * 4, index * 4 + 4);
  return { rgba: output, evidence: { opaqueCarrierPixelCount:
    contract.opaqueCarrierCleanup.pixelCount, opaqueCarrierMaskSha256: maskSha(strongMask),
    isolatedAlphaPixelCount: isolatedMask.reduce((sum, value) => sum + value, 0),
    isolatedAlphaMaskSha256: maskSha(isolatedMask) } };
}

const dominance = (red, green, blue) => ({
  green: green > Math.max(red, blue),
  cyan: Math.min(green, blue) > red,
  magenta: Math.min(red, blue) > green,
});
const hasDominance = (red, green, blue) =>
  Object.values(dominance(red, green, blue)).some(Boolean);

const edgeDistanceOneMask = (rgba, width, height) => {
  const mask = new Uint8Array(width * height);
  for (let index = 0; index < mask.length; index += 1) {
    const alpha = rgba[index * 4 + 3];
    if (alpha <= 0 || alpha >= 255) continue;
    if (neighbors4(index, width, height).some((next) => rgba[next * 4 + 3] === 0))
      mask[index] = 1;
  }
  return mask;
};

function areaModelAt({ rgb, fullRoot, sourceFringeMask, sourceWidth, sourceBbox,
  targetWidth, targetHeight, dx, dy }) {
  const sourceBoxWidth = sourceBbox.xMax - sourceBbox.xMin + 1;
  const sourceBoxHeight = sourceBbox.yMax - sourceBbox.yMin + 1;
  const xStart = dx * sourceBoxWidth; const xEnd = (dx + 1) * sourceBoxWidth;
  const yStart = dy * sourceBoxHeight; const yEnd = (dy + 1) * sourceBoxHeight;
  const composite = [0, 0, 0]; const background = [0, 0, 0];
  let totalWeight = 0; let fringeWeight = 0;
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
      const sourceOffset = index * 3; const backgroundOffset = fullRoot[index] * 3;
      totalWeight += weight;
      if (sourceFringeMask[index]) fringeWeight += weight;
      for (let channel = 0; channel < 3; channel += 1) {
        composite[channel] += rgb[sourceOffset + channel] * weight;
        background[channel] += rgb[backgroundOffset + channel] * weight;
      }
    }
  }
  return { composite: composite.map((value) => roundHalfUp(value, totalWeight)),
    background: background.map((value) => roundHalfUp(value, totalWeight)),
    fringeWeight };
}

export function cleanExactPartialEdgeFringe({ rgba, width, height, source,
  models, fit, contract }) {
  const output = Buffer.from(rgba); const edgeMask = edgeDistanceOneMask(rgba, width, height);
  const candidateMask = new Uint8Array(width * height);
  const clearedMask = new Uint8Array(width * height);
  const counts = { green: 0, cyan: 0, magenta: 0 };
  let candidateCount = 0; let solvedCount = 0; let clearedCount = 0;
  for (let y = fit.placement.y; y < fit.placement.y + fit.targetSize.height; y += 1) {
    for (let x = fit.placement.x; x < fit.placement.x + fit.targetSize.width; x += 1) {
      const index = y * width + x; const offset = index * 4;
      if (!edgeMask[index] || !hasDominance(output[offset], output[offset + 1],
        output[offset + 2])) continue;
      const model = areaModelAt({ rgb: source.rgb, fullRoot: models.fullRoot,
        sourceFringeMask: models.fringeMask, sourceWidth: source.width,
        sourceBbox: fit.sourceForegroundBbox, targetWidth: fit.targetSize.width,
        targetHeight: fit.targetSize.height, dx: x - fit.placement.x,
        dy: y - fit.placement.y });
      if (model.fringeWeight === 0) continue;
      candidateMask[index] = 1; candidateCount += 1;
      const classes = dominance(output[offset], output[offset + 1], output[offset + 2]);
      for (const key of Object.keys(counts)) if (classes[key]) counts[key] += 1;
      const initialAlpha = output[offset + 3];
      const alphas = Array.from({ length: 254 }, (_, position) => position + 1)
        .sort((left, right) => Math.abs(left - initialAlpha) - Math.abs(right - initialAlpha)
          || left - right);
      let solution;
      for (const alpha of alphas) {
        const raw = model.composite.map((value, channel) =>
          (255 * value - (255 - alpha) * model.background[channel]) / alpha);
        if (raw.some((value) => value < 0 || value > 255)) continue;
        const foreground = raw.map((value) => Math.round(value));
        const recomposed = foreground.map((value, channel) => Math.round(
          (alpha * value + (255 - alpha) * model.background[channel]) / 255));
        const error = Math.max(...recomposed.map((value, channel) =>
          Math.abs(value - model.composite[channel])));
        if (!hasDominance(...foreground) && error <= 1) {
          solution = { alpha, foreground }; break;
        }
      }
      if (solution) {
        output.set(solution.foreground, offset); output[offset + 3] = solution.alpha;
        solvedCount += 1;
      } else {
        output.fill(0, offset, offset + 4); clearedMask[index] = 1; clearedCount += 1;
      }
    }
  }
  if (candidateCount !== contract.candidateCount
    || canonicalJson(counts) !== canonicalJson(contract.candidateCountByDominance)
    || maskSha(candidateMask) !== contract.candidateMaskSha256
    || solvedCount !== contract.solvedCount || clearedCount !== contract.clearedCount
    || maskSha(clearedMask) !== contract.clearedMaskSha256)
    throw new Error("source_chroma_fit_final_edge_fringe_evidence_mismatch");
  const after = { green: 0, cyan: 0, magenta: 0 };
  for (let index = 0; index < candidateMask.length; index += 1) {
    if (!candidateMask[index] || output[index * 4 + 3] === 0) continue;
    const classes = dominance(output[index * 4], output[index * 4 + 1],
      output[index * 4 + 2]);
    for (const key of Object.keys(after)) if (classes[key]) after[key] += 1;
  }
  if (canonicalJson(after) !== canonicalJson(contract.postCleanupCandidateDominanceCount))
    throw new Error("source_chroma_fit_final_edge_fringe_residual");
  return { rgba: output, evidence: { candidateCount, candidateCountByDominance: counts,
    candidateMaskSha256: maskSha(candidateMask), solvedCount, clearedCount,
    clearedMaskSha256: maskSha(clearedMask), postCleanupCandidateDominanceCount: after } };
}

function validateOutput(rgba, width, height, contract) {
  let xMin = width; let yMin = height; let xMax = -1; let yMax = -1;
  let transparentRgbNonzeroCount = 0;
  for (let index = 0; index < width * height; index += 1) {
    const offset = index * 4; const alpha = rgba[offset + 3];
    if (alpha === 0) {
      if (rgba[offset] || rgba[offset + 1] || rgba[offset + 2])
        transparentRgbNonzeroCount += 1;
    } else {
      const x = index % width; const y = Math.floor(index / width);
      xMin = Math.min(xMin, x); xMax = Math.max(xMax, x);
      yMin = Math.min(yMin, y); yMax = Math.max(yMax, y);
    }
  }
  const foregroundBbox = { xMin, yMin, xMax, yMax };
  if (width !== contract.canvas.width || height !== contract.canvas.height
    || transparentRgbNonzeroCount !== 0
    || canonicalJson(foregroundBbox) !== canonicalJson(contract.foregroundBbox))
    throw new Error("source_chroma_fit_final_output_validation_failed");
  if (contract.opaqueStrongGreenPixelCount === 0
    && opaqueStrongGreenMask(rgba).some((value) => value))
    throw new Error("source_chroma_fit_final_opaque_carrier_residual");
  if (contract.isolatedOnePixelAlphaComponentCount === 0
    && connectedComponents(alphaMask(rgba), width, height).some(({ area }) => area === 1))
    throw new Error("source_chroma_fit_final_isolated_alpha_residual");
  return { foregroundBbox, transparentRgbNonzeroCount };
}

export function executeFinalFit({ profile, predecessorProfile, sourceBytes,
  sourceReceiptBytes }) {
  validateFinalProfile(profile);
  if (sha(sourceBytes) !== profile.sourceBinding.sourceSha256
    || sourceBytes.length !== profile.sourceBinding.byteLength
    || sha(sourceReceiptBytes) !== (profile.sourceBinding.generationReceiptSha256
      ?? profile.sourceBinding.editExecutionReceiptSha256))
    throw new Error("source_chroma_fit_final_binding_not_registered");
  if (predecessorProfile.profileKey !== profile.predecessorBinding.profileKey
    || sha(Buffer.from(canonicalJson(predecessorProfile), "utf8"))
      !== profile.predecessorBinding.profilePayloadSha256)
    throw new Error("source_chroma_fit_final_predecessor_mismatch");
  const predecessor = executeRecoveryFitV2({ profile: predecessorProfile,
    sourceBytes, receiptBytes: sourceReceiptBytes });
  if (sha(predecessor.outputBytes) !== profile.rejectedPredecessorEvidence.outputPngSha256)
    throw new Error("source_chroma_fit_final_rejected_intermediate_mismatch");
  let cleaned;
  if (profile.profileKey === G2_FINAL_PROFILE_KEY) {
    cleaned = cleanExactOpaqueCarrierAndIsolatedAlpha({ rgba: predecessor.canvasRgba,
      width: profile.outputContract.canvas.width, height: profile.outputContract.canvas.height,
      contract: profile.algorithmContract });
  } else {
    const source = decodePngRgb8(sourceBytes); const sourceEvidence = deriveSourceEvidence(source);
    const models = deriveCarrierModels({ ...source, evidence: sourceEvidence });
    cleaned = cleanExactPartialEdgeFringe({ rgba: predecessor.canvasRgba,
      width: profile.outputContract.canvas.width, height: profile.outputContract.canvas.height,
      source, models, fit: predecessorProfile.algorithmContract.fit,
      contract: profile.algorithmContract.edgeFringeCleanup });
  }
  const outputValidation = validateOutput(cleaned.rgba, profile.outputContract.canvas.width,
    profile.outputContract.canvas.height, profile.outputContract);
  const outputBytes = serializePngRgba8({ width: profile.outputContract.canvas.width,
    height: profile.outputContract.canvas.height, rgba: cleaned.rgba });
  if (sha(cleaned.rgba) !== profile.outputContract.outputRgbaSha256
    || sha(outputBytes) !== profile.outputContract.outputPngSha256
    || outputBytes.length !== profile.outputContract.outputPngByteLength)
    throw new Error("source_chroma_fit_final_output_identity_mismatch");
  return { outputBytes, canvasRgba: cleaned.rgba, cleanupEvidence: cleaned.evidence,
    outputValidation, profileKey: profile.profileKey,
    profilePayloadSha256: PROFILE_HASHES.get(profile.profileKey),
    providerCalled: false, submitCount: 0, retryCount: 0 };
}

function main(argv) {
  if (argv.length !== 6) throw new Error("usage: node generated_media_source_bound_chroma_fit_final_v3.mjs successor-profile.json predecessor-profile.json source.png source-receipt.json output.png output.receipt.json");
  const [profilePath, predecessorPath, sourcePath, sourceReceiptPath,
    outputPath, receiptPath] = argv;
  if (existsSync(outputPath) || existsSync(receiptPath))
    throw new Error("source_chroma_fit_final_output_collision");
  const result = executeFinalFit({ profile: JSON.parse(readFileSync(profilePath, "utf8")),
    predecessorProfile: JSON.parse(readFileSync(predecessorPath, "utf8")),
    sourceBytes: readFileSync(sourcePath), sourceReceiptBytes: readFileSync(sourceReceiptPath) });
  writeFileSync(outputPath, result.outputBytes, { flag: "wx" });
  const receipt = { schemaVersion: "generated_media_source_bound_chroma_fit_receipt_v3",
    state: "fit_complete", profileKey: result.profileKey,
    profilePayloadSha256: result.profilePayloadSha256,
    sourceSha256: sha(readFileSync(sourcePath)), outputSha256: sha(result.outputBytes),
    cleanupEvidence: result.cleanupEvidence, outputValidation: result.outputValidation,
    providerCalled: false, submitCount: 0, retryCount: 0,
    evaluationStatus: "not_evaluated", projectCopyEligible: false };
  writeFileSync(receiptPath, `${canonicalJson(receipt)}\n`, { flag: "wx" });
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  try { main(process.argv.slice(2)); }
  catch (error) { process.stderr.write(`${error.message}\n`); process.exitCode = 1; }
}
