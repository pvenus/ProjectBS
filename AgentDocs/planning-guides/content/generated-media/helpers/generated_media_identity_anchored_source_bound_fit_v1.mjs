import { createHash } from "node:crypto";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { canonicalJson, serializePngRgba8 } from
  "./generated_media_canonical_serializers_v1.mjs";
import { decodePngRgb8, deriveSourceEvidence } from
  "./generated_media_source_bound_chroma_uncomposite_v1.mjs";
import { deriveCarrierModels, recoverExpandedFringe,
  removeTargetResidualCarrier } from
  "./generated_media_source_bound_chroma_fit_v2.mjs";
import { placeOnCanvas, resizePremultipliedBox } from
  "./generated_media_source_bound_chroma_fit_v1.mjs";
import { connectedComponents } from
  "./generated_media_source_bound_chroma_fit_final_v3.mjs";

export const PROFILE_KEY =
  "projectbs_character_open_ink_identity_anchored_source_bound_green_carrier_fit@1.0.0";
export const PROFILE_PAYLOAD_SHA256 =
  "1a669eed96cda8a2add59445cbf3c1e174fe359b1c03bf42ed707477d3cdc138";
export const ALGORITHM_KEY =
  "generated_media_identity_anchored_source_bound_green_carrier_fit_v1";
export const ALGORITHM_VERSION = "1.0.0";

const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const maskSha = (mask) => sha(Buffer.from(mask.buffer, mask.byteOffset,
  mask.byteLength));
const roundHalfUp = (numerator, denominator) =>
  Math.floor((2 * numerator + denominator) / (2 * denominator));

const neighbors4 = (index, width, height) => {
  const x = index % width; const y = Math.floor(index / width); const result = [];
  if (y > 0) result.push(index - width);
  if (x > 0) result.push(index - 1);
  if (x + 1 < width) result.push(index + 1);
  if (y + 1 < height) result.push(index + width);
  return result;
};
const dominance = (red, green, blue) => ({
  green: green > Math.max(red, blue),
  cyan: Math.min(green, blue) > red,
  magenta: Math.min(red, blue) > green,
});
const hasDominance = (red, green, blue) =>
  Object.values(dominance(red, green, blue)).some(Boolean);
const alphaMask = (rgba) => {
  const mask = new Uint8Array(rgba.length / 4);
  for (let index = 0; index < mask.length; index += 1)
    if (rgba[index * 4 + 3] > 0) mask[index] = 1;
  return mask;
};
const strongGreenMask = (rgba) => {
  const mask = new Uint8Array(rgba.length / 4);
  for (let index = 0; index < mask.length; index += 1) {
    const offset = index * 4;
    if (rgba[offset + 3] === 255
      && rgba[offset + 1] - Math.max(rgba[offset], rgba[offset + 2]) >= 24
      && rgba[offset + 1] >= 96) mask[index] = 1;
  }
  return mask;
};

export function validateProfile(profile) {
  if (profile.profileKey !== PROFILE_KEY
    || sha(Buffer.from(canonicalJson(profile), "utf8")) !== PROFILE_PAYLOAD_SHA256)
    throw new Error("identity_source_bound_fit_profile_hash_mismatch");
  return profile;
}

function validateBinding(profile, sourceBytes, receiptBytes) {
  const binding = profile.sourceBinding;
  if (sha(sourceBytes) !== binding.sourceSha256
    || sourceBytes.length !== binding.byteLength
    || sha(receiptBytes) !== binding.generationReceiptSha256
    || receiptBytes.length !== binding.generationReceiptByteLength)
    throw new Error("identity_source_bound_fit_binding_not_registered");
  const receipt = JSON.parse(receiptBytes.toString("utf8"));
  if (receipt.providerOutputSha256 !== binding.sourceSha256
    || receipt.executionScopeHash !== binding.generationScopeSha256
    || receipt.generationHandoffSha256 !== binding.generationHandoffSha256
    || receipt.providerCalled !== true || receipt.submitCount !== 1
    || receipt.retryCount !== 0 || receipt.state !== "output_nonconformant_no_retry")
    throw new Error("identity_source_bound_fit_receipt_projection_mismatch");
  const required = receipt.identityEquipmentConformance.requiredGateResults;
  const prohibited = receipt.identityEquipmentConformance.prohibitedGateResults;
  if (required.filter((gate) => gate.gate !==
      "compact full-body proportion and safe fit").some((gate) => gate.result !== "pass")
    || prohibited.some((gate) => gate.result !== "pass"))
    throw new Error("identity_source_bound_fit_identity_gate_not_accepted");
}

function areaModelAt({ source, evidence, models, fit, dx, dy }) {
  const box = fit.sourceForegroundBbox;
  const sourceWidth = box.xMax - box.xMin + 1;
  const sourceHeight = box.yMax - box.yMin + 1;
  const targetWidth = fit.targetSize.width; const targetHeight = fit.targetSize.height;
  const xStart = dx * sourceWidth; const xEnd = (dx + 1) * sourceWidth;
  const yStart = dy * sourceHeight; const yEnd = (dy + 1) * sourceHeight;
  const composite = [0, 0, 0]; const background = [0, 0, 0];
  let totalWeight = 0; let fringeWeight = 0; let positiveWeight = 0;
  for (let sy = Math.floor(yStart / targetHeight);
    sy < Math.ceil(yEnd / targetHeight); sy += 1) {
    const overlapY = Math.min(yEnd, (sy + 1) * targetHeight)
      - Math.max(yStart, sy * targetHeight);
    for (let sx = Math.floor(xStart / targetWidth);
      sx < Math.ceil(xEnd / targetWidth); sx += 1) {
      const overlapX = Math.min(xEnd, (sx + 1) * targetWidth)
        - Math.max(xStart, sx * targetWidth);
      const weight = overlapX * overlapY;
      const index = (box.yMin + sy) * source.width + box.xMin + sx;
      const sourceOffset = index * 3; const backgroundOffset = models.fullRoot[index] * 3;
      totalWeight += weight;
      if (models.fringeMask[index]) fringeWeight += weight;
      if (evidence.excess[index] > 0) positiveWeight += weight;
      for (let channel = 0; channel < 3; channel += 1) {
        composite[channel] += source.rgb[sourceOffset + channel] * weight;
        background[channel] += source.rgb[backgroundOffset + channel] * weight;
      }
    }
  }
  return { composite: composite.map((value) => roundHalfUp(value, totalWeight)),
    background: background.map((value) => roundHalfUp(value, totalWeight)),
    totalWeight, fringeWeight, positiveWeight };
}

function inverseCompositeSolution(model, initialAlpha) {
  const alphas = Array.from({ length: 254 }, (_, position) => position + 1)
    .sort((left, right) => Math.abs(left - initialAlpha)
      - Math.abs(right - initialAlpha) || left - right);
  for (const alpha of alphas) {
    const raw = model.composite.map((value, channel) =>
      (255 * value - (255 - alpha) * model.background[channel]) / alpha);
    if (raw.some((value) => value < 0 || value > 255)) continue;
    const foreground = raw.map((value) => Math.round(value));
    const recomposed = foreground.map((value, channel) => Math.round(
      (alpha * value + (255 - alpha) * model.background[channel]) / 255));
    if (!hasDominance(...foreground)
      && Math.max(...recomposed.map((value, channel) =>
        Math.abs(value - model.composite[channel]))) <= 1)
      return { alpha, foreground };
  }
  return undefined;
}

function applySolution(output, index, solution, clearedMask) {
  const offset = index * 4;
  if (!solution) {
    output.fill(0, offset, offset + 4); clearedMask[index] = 1; return false;
  }
  output.set(solution.foreground, offset); output[offset + 3] = solution.alpha;
  return true;
}

function cleanEdges({ rgba, source, evidence, models, fit, contract }) {
  const output = Buffer.from(rgba); const width = 1024; const height = 1536;
  const candidateMask = new Uint8Array(width * height);
  const clearedMask = new Uint8Array(width * height);
  const counts = { green: 0, cyan: 0, magenta: 0 };
  let visitCount = 0; let solvedCount = 0; let clearedCount = 0; let roundCount = 0;
  for (let round = 1; round <= contract.maximumRounds; round += 1) {
    const snapshot = Buffer.from(output); let roundCandidates = 0;
    for (let y = fit.placement.y; y < fit.placement.y + fit.targetSize.height; y += 1) {
      for (let x = fit.placement.x; x < fit.placement.x + fit.targetSize.width; x += 1) {
        const index = y * width + x; const offset = index * 4;
        if (snapshot[offset + 3] <= 0 || snapshot[offset + 3] >= 255
          || !neighbors4(index, width, height).some((next) =>
            snapshot[next * 4 + 3] === 0)
          || !hasDominance(snapshot[offset], snapshot[offset + 1], snapshot[offset + 2]))
          continue;
        const model = areaModelAt({ source, evidence, models, fit,
          dx: x - fit.placement.x, dy: y - fit.placement.y });
        if (model.fringeWeight === 0) continue;
        candidateMask[index] = 1; roundCandidates += 1; visitCount += 1;
        const classes = dominance(snapshot[offset], snapshot[offset + 1],
          snapshot[offset + 2]);
        for (const key of Object.keys(counts)) if (classes[key]) counts[key] += 1;
        if (applySolution(output, index,
          inverseCompositeSolution(model, snapshot[offset + 3]), clearedMask)) solvedCount += 1;
        else clearedCount += 1;
      }
    }
    roundCount = round;
    if (roundCandidates === 0) break;
    if (round === contract.maximumRounds)
      throw new Error("identity_source_bound_fit_edge_cleanup_not_converged");
  }
  if (visitCount !== contract.candidateVisitCount
    || canonicalJson(counts) !== canonicalJson(contract.candidateCountByDominance)
    || maskSha(candidateMask) !== contract.candidateMaskSha256
    || solvedCount !== contract.solvedVisitCount
    || clearedCount !== contract.clearedVisitCount
    || maskSha(clearedMask) !== contract.clearedMaskSha256
    || roundCount !== contract.convergenceRoundCountIncludingTerminalZero)
    throw new Error(`identity_source_bound_fit_edge_evidence_mismatch:${canonicalJson({
      visitCount, counts, candidateMaskSha256: maskSha(candidateMask), solvedCount,
      clearedCount, clearedMaskSha256: maskSha(clearedMask), roundCount })}`);
  return { rgba: output, evidence: { candidateVisitCount: visitCount,
    candidateMaskSha256: maskSha(candidateMask), solvedVisitCount: solvedCount,
    clearedVisitCount: clearedCount, clearedMaskSha256: maskSha(clearedMask),
    convergenceRoundCountIncludingTerminalZero: roundCount } };
}

function cleanOpaquePositive({ rgba, source, evidence, models, fit, contract }) {
  const output = Buffer.from(rgba); const candidateMask = strongGreenMask(output);
  const components = connectedComponents(candidateMask, 1024, 1536);
  const clearedMask = new Uint8Array(candidateMask.length);
  let solved = 0; let cleared = 0;
  for (const component of components) for (const index of component.pixels) {
    const x = index % 1024; const y = Math.floor(index / 1024);
    const model = areaModelAt({ source, evidence, models, fit,
      dx: x - fit.placement.x, dy: y - fit.placement.y });
    if (model.positiveWeight !== model.totalWeight)
      throw new Error("identity_source_bound_fit_opaque_candidate_not_source_supported");
    if (applySolution(output, index, inverseCompositeSolution(model, 254), clearedMask))
      solved += 1;
    else cleared += 1;
  }
  if (components.length !== contract.candidateComponentCount
    || candidateMask.reduce((sum, value) => sum + value, 0) !== contract.candidatePixelCount
    || maskSha(candidateMask) !== contract.candidateMaskSha256
    || solved !== contract.solvedPixelCount || cleared !== contract.clearedPixelCount
    || maskSha(clearedMask) !== contract.clearedMaskSha256)
    throw new Error("identity_source_bound_fit_opaque_evidence_mismatch");
  return { rgba: output, evidence: { candidatePixelCount: contract.candidatePixelCount,
    candidateMaskSha256: maskSha(candidateMask), solvedPixelCount: solved,
    clearedPixelCount: cleared, clearedMaskSha256: maskSha(clearedMask) } };
}

function cleanDetached({ rgba, source, evidence, models, fit, contract }) {
  const output = Buffer.from(rgba); const components = connectedComponents(alphaMask(output),
    1024, 1536); const mask = new Uint8Array(1024 * 1536); let pixels = 0;
  for (const component of components.slice(1)) for (const index of component.pixels) {
    const x = index % 1024; const y = Math.floor(index / 1024);
    const model = areaModelAt({ source, evidence, models, fit,
      dx: x - fit.placement.x, dy: y - fit.placement.y });
    if (model.fringeWeight <= 0)
      throw new Error("identity_source_bound_fit_fragment_not_source_supported");
    mask[index] = 1; pixels += 1; output.fill(0, index * 4, index * 4 + 4);
  }
  if (components.length - 1 !== contract.componentCount || pixels !== contract.pixelCount
    || maskSha(mask) !== contract.maskSha256)
    throw new Error("identity_source_bound_fit_fragment_evidence_mismatch");
  return { rgba: output, evidence: { componentCount: components.length - 1,
    pixelCount: pixels, maskSha256: maskSha(mask) } };
}

function validateOutput(rgba, contract) {
  let xMin = 1024; let yMin = 1536; let xMax = -1; let yMax = -1;
  let transparentRgb = 0; const edgeDominance = { green: 0, cyan: 0, magenta: 0 };
  for (let index = 0; index < 1024 * 1536; index += 1) {
    const offset = index * 4; const alpha = rgba[offset + 3];
    if (alpha === 0) {
      if (rgba[offset] || rgba[offset + 1] || rgba[offset + 2]) transparentRgb += 1;
      continue;
    }
    const x = index % 1024; const y = Math.floor(index / 1024);
    xMin = Math.min(xMin, x); xMax = Math.max(xMax, x);
    yMin = Math.min(yMin, y); yMax = Math.max(yMax, y);
    if (alpha < 255 && neighbors4(index, 1024, 1536)
      .some((next) => rgba[next * 4 + 3] === 0)) {
      const classes = dominance(rgba[offset], rgba[offset + 1], rgba[offset + 2]);
      for (const key of Object.keys(edgeDominance)) if (classes[key]) edgeDominance[key] += 1;
    }
  }
  const components = connectedComponents(alphaMask(rgba), 1024, 1536);
  if (transparentRgb !== 0 || components.length !== contract.alphaComponentCount
    || components[0].area !== contract.foregroundPixelCount
    || canonicalJson({ xMin, yMin, xMax, yMax }) !== canonicalJson(contract.foregroundBbox)
    || canonicalJson(edgeDominance)
      !== canonicalJson(contract.silhouetteEdgeDominanceCounts)
    || strongGreenMask(rgba).some(Boolean))
    throw new Error("identity_source_bound_fit_output_validation_failed");
  return { foregroundBbox: { xMin, yMin, xMax, yMax },
    foregroundPixelCount: components[0].area, alphaComponentCount: components.length,
    transparentRgbNonzeroCount: transparentRgb,
    silhouetteEdgeDominanceCounts: edgeDominance };
}

export function executeIdentityAnchoredSourceBoundFit({ profile, sourceBytes,
  receiptBytes }) {
  validateProfile(profile); validateBinding(profile, sourceBytes, receiptBytes);
  const source = decodePngRgb8(sourceBytes); const evidence = deriveSourceEvidence(source);
  const models = deriveCarrierModels({ ...source, evidence });
  const carrier = profile.algorithmContract.carrierModel;
  if (evidence.measured.borderEvidence.sourceCalibratedFloor !== carrier.sourceCalibratedFloor
    || evidence.measured.topologyEvidence.edgeCarrierMaskSha256
      !== carrier.edgeCarrierMaskSha256
    || evidence.measured.topologyEvidence.candidateMaskSha256
      !== carrier.candidateMaskSha256
    || models.measured.sourceFringeMaskSha256 !== carrier.sourceFringeMaskSha256
    || models.measured.fullNearestCarrierRootMapSha256
      !== carrier.fullNearestCarrierRootMapSha256)
    throw new Error("identity_source_bound_fit_carrier_evidence_mismatch");
  const recovered = recoverExpandedFringe({ ...source, evidence, models });
  if (recovered.protectedRgbMismatchCount !== 0)
    throw new Error("identity_source_bound_fit_protected_source_mismatch");
  const fit = profile.algorithmContract.fit;
  const resized = resizePremultipliedBox({ rgba: recovered.rgba,
    sourceWidth: source.width, sourceHeight: source.height,
    sourceBbox: fit.sourceForegroundBbox, targetWidth: fit.targetSize.width,
    targetHeight: fit.targetSize.height });
  const residual = removeTargetResidualCarrier({ resizedRgba: resized,
    rgb: source.rgb, fullRoot: models.fullRoot, sourceWidth: source.width,
    sourceBbox: fit.sourceForegroundBbox, targetWidth: fit.targetSize.width,
    targetHeight: fit.targetSize.height });
  const residualContract = profile.algorithmContract.residualCarrierCleanup;
  if (canonicalJson({ pixelCount: residual.measured.residualPixelCount,
    solvedPixelCount: residual.measured.solvedPixelCount,
    clearedPixelCount: residual.measured.clearedPixelCount,
    maskSha256: residual.measured.residualTargetMaskSha256,
    clearedMaskSha256: residual.measured.residualClearedTargetMaskSha256 })
    !== canonicalJson(residualContract))
    throw new Error("identity_source_bound_fit_residual_evidence_mismatch");
  const canvas = placeOnCanvas({ resizedRgba: residual.rgba,
    targetWidth: fit.targetSize.width, targetHeight: fit.targetSize.height,
    canvasWidth: fit.canvas.width, canvasHeight: fit.canvas.height,
    x: fit.placement.x, y: fit.placement.y });
  const edge = cleanEdges({ rgba: canvas, source, evidence, models, fit,
    contract: profile.algorithmContract.edgeFringeCleanup });
  const opaque = cleanOpaquePositive({ rgba: edge.rgba, source, evidence, models, fit,
    contract: profile.algorithmContract.opaquePositiveGreenCleanup });
  const fragments = cleanDetached({ rgba: opaque.rgba, source, evidence, models, fit,
    contract: profile.algorithmContract.detachedFragmentCleanup });
  const validation = validateOutput(fragments.rgba, profile.outputContract);
  const outputBytes = serializePngRgba8({ width: 1024, height: 1536,
    rgba: fragments.rgba });
  if (sha(fragments.rgba) !== profile.outputContract.outputRgbaSha256
    || sha(outputBytes) !== profile.outputContract.outputPngSha256
    || outputBytes.length !== profile.outputContract.outputPngByteLength)
    throw new Error("identity_source_bound_fit_output_identity_mismatch");
  return { outputBytes, canvasRgba: fragments.rgba, outputValidation: validation,
    cleanupEvidence: { edge: edge.evidence, opaque: opaque.evidence,
      detached: fragments.evidence }, profileKey: PROFILE_KEY,
    profilePayloadSha256: PROFILE_PAYLOAD_SHA256, providerCalled: false,
    submitCount: 0, retryCount: 0 };
}

function main(argv) {
  if (argv.length !== 5) throw new Error("usage: node generated_media_identity_anchored_source_bound_fit_v1.mjs profile.json source.png generation-receipt.json output.png output.receipt.json");
  const [profilePath, sourcePath, receiptPath, outputPath, outputReceiptPath] = argv;
  if (existsSync(outputPath) || existsSync(outputReceiptPath))
    throw new Error("identity_source_bound_fit_output_collision");
  const sourceBytes = readFileSync(sourcePath); const receiptBytes = readFileSync(receiptPath);
  const result = executeIdentityAnchoredSourceBoundFit({
    profile: JSON.parse(readFileSync(profilePath, "utf8")), sourceBytes, receiptBytes });
  writeFileSync(outputPath, result.outputBytes, { flag: "wx" });
  const receipt = { schemaVersion:
    "generated_media_identity_anchored_source_bound_fit_receipt_v1",
    state: "fit_complete", profileKey: result.profileKey,
    profilePayloadSha256: result.profilePayloadSha256,
    sourceSha256: sha(sourceBytes), generationReceiptSha256: sha(receiptBytes),
    outputSha256: sha(result.outputBytes), cleanupEvidence: result.cleanupEvidence,
    outputValidation: result.outputValidation, providerCalled: false, submitCount: 0,
    retryCount: 0, evaluationStatus: "not_evaluated", projectCopyEligible: false };
  writeFileSync(outputReceiptPath, `${canonicalJson(receipt)}\n`, { flag: "wx" });
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  try { main(process.argv.slice(2)); }
  catch (error) { process.stderr.write(`${error.message}\n`); process.exitCode = 1; }
}
