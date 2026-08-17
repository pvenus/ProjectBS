// Closed documentation vectors for provider-native animated GIF animation.
// This test performs no provider call and writes no workflow artifact.

import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const taskPromptRoot = join(guideRoot, "..", "..", "..", "task-prompts",
  "content", "generated-media");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");

const contract = read(join(guideRoot, "GeneratedMediaImageGenOnlyContractGuide.md"));
const planning = read(join(guideRoot, "GeneratedMediaPlanningHandoffGuide.md"));
const routing = read(join(guideRoot, "GeneratedMediaRequestRoutingGuide.md"));
const pipeline = read(join(guideRoot, "ImageGenAnimationPipelineGuide.md"));
const preservation = read(join(guideRoot, "GeneratedMediaPreservationPackagingGuide.md"));
const routingPrompt = read(join(taskPromptRoot, "GeneratedMediaRequestRoutingPrompt.md"));
const authoringPrompt = read(join(taskPromptRoot, "ImageGenAnimationPromptAuthoringPrompt.md"));
const generationPrompt = read(join(taskPromptRoot, "ImageGenAnimationGenerationPrompt.md"));
const preservationPrompt = read(join(taskPromptRoot,
  "GeneratedMediaPreservationPackagingPrompt.md"));

const allCurrentSurfaces = [contract, planning, routing, pipeline, routingPrompt,
  authoringPrompt, generationPrompt, preservation, preservationPrompt];

for (const surface of allCurrentSurfaces) {
  assert.match(surface, /provider_native_animated_gif/);
}

for (const surface of [contract, planning, routing, pipeline, routingPrompt,
  authoringPrompt, generationPrompt]) {
  assert.match(surface, /gif_timeline_exact/);
}

assert.match(contract, /configured_animated_gif_capability/);
assert.match(pipeline, /configured_animated_gif_capability/);
assert.match(generationPrompt, /configured_animated_gif_capability/);

for (const surface of [contract, pipeline, generationPrompt]) {
  assert.match(surface, /animated_provider_capability_unavailable/);
  assert.match(surface, /providerCalled=false/);
  assert.match(surface, /submitCount=0/);
}

for (const surface of [contract, pipeline, authoringPrompt, generationPrompt,
  preservation, preservationPrompt]) {
  assert.match(surface, /contact sheet/i);
  assert.match(surface, /sprite sheet/i);
}

assert.match(contract, /preserve the exact original GIF bytes and hash/);
assert.match(contract, /fixedCellCanvas[\s\S]*not a multi-cell grid or sheet geometry/);
assert.match(preservation, /preserve exact provider-native animated GIF original and hash/);
assert.match(preservationPrompt, /playable animated GIF 원본과 hash를 먼저 보존/);
assert.match(generationPrompt, /정확히 한 번 제출하고 retry=0/);

for (const surface of [pipeline, generationPrompt, preservation,
  preservationPrompt]) {
  assert.match(surface,
    /generated_media_attack_coherent_master_to_gif_validation_receipt_v2/);
  assert.match(surface, /six-cell master\s+IMAGE/i);
  assert.match(surface, /providerDidReturnGif=false/);
  assert.match(surface, /close[^\n]*reopen/i);
  assert.match(surface, /six PNG/i);
}

// The provider-native mode remains present, while the accepted v2 section
// explicitly denies that the provider returned a GIF.
assert.match(pipeline, /provider_native_animated_gif/);
const acceptedSection = pipeline.split(
  "### Accepted-result coherent-master-to-GIF guidance")[1]
  .split("## Input, Output, State, and Validation")[0];
assert.match(acceptedSection,
  /provider returns\s+one coherent six-cell master IMAGE and did not return a GIF/i);
assert.match(acceptedSection,
  /animationSourceMode=generation_role_coherent_six_cell_master_to_gif/);
assert.match(acceptedSection,
  /extractionMode=generation_completed_gif_timeline_exact/);

const receiptKeys = ["schemaVersion", "animationRequestId",
  "providerDidReturnGif", "providerMasterImageSha256",
  "providerMasterCellCount", "completedGifSha256", "width", "height",
  "frameCount", "extractedPngFrameSha256s", "sharedWidthBasis",
  "pelvisDriftMaxPx", "baselineDriftMaxPx", "scaleUniform",
  "timingUniform", "globalPaletteUniform", "backgroundFullyOpaque",
  "clippingDetected", "neighboringFragmentsDetected",
  "gifClosedAndReopened", "pngsExtractedFromReopenedGif", "status"];

function validateCoherentMasterReceipt(receipt, expected) {
  assert.deepEqual(Object.keys(receipt).sort(), [...receiptKeys].sort());
  assert.equal(receipt.schemaVersion,
    "generated_media_attack_coherent_master_to_gif_validation_receipt_v2");
  if (receipt.providerDidReturnGif !== false)
    throw new Error("invalid_animation_source_mode");
  assert.equal(receipt.width, expected.width);
  assert.equal(receipt.height, expected.height);
  assert.equal(receipt.frameCount, expected.frameCount);
  const valid = receipt.providerMasterCellCount === 6
    && receipt.extractedPngFrameSha256s.length === 6
    && receipt.gifClosedAndReopened === true
    && receipt.pngsExtractedFromReopenedGif === true
    && receipt.pelvisDriftMaxPx === 0
    && receipt.baselineDriftMaxPx === 0
    && receipt.scaleUniform === true
    && receipt.timingUniform === true
    && receipt.globalPaletteUniform === true
    && receipt.backgroundFullyOpaque === true
    && receipt.clippingDetected === false
    && receipt.neighboringFragmentsDetected === false;
  assert.equal(receipt.status, valid ? "valid" : "blocked");
  return valid;
}

const accepted = {
  schemaVersion:
    "generated_media_attack_coherent_master_to_gif_validation_receipt_v2",
  animationRequestId: "character.seojin.1.attack.draw_slash.one_shot.v14",
  providerDidReturnGif: false,
  providerMasterImageSha256: "1".repeat(64),
  providerMasterCellCount: 6,
  completedGifSha256:
    "8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621",
  width: 640, height: 512, frameCount: 6,
  extractedPngFrameSha256s: Array.from({ length: 6 }, (_, i) =>
    i.toString(16).repeat(64)),
  sharedWidthBasis: "longest_clean_left_right_margin",
  pelvisDriftMaxPx: 0, baselineDriftMaxPx: 0,
  scaleUniform: true, timingUniform: true, globalPaletteUniform: true,
  backgroundFullyOpaque: true, clippingDetected: false,
  neighboringFragmentsDetected: false, gifClosedAndReopened: true,
  pngsExtractedFromReopenedGif: true, status: "valid",
};
const expected = { width: 640, height: 512, frameCount: 6 };
assert.equal(validateCoherentMasterReceipt(accepted, expected), true);
assert.throws(() => validateCoherentMasterReceipt({ ...accepted,
  providerDidReturnGif: true }, expected), /invalid_animation_source_mode/);
for (const failure of [{ providerMasterCellCount: 5 },
  { extractedPngFrameSha256s: accepted.extractedPngFrameSha256s.slice(0, 5) },
  { gifClosedAndReopened: false }, { pngsExtractedFromReopenedGif: false },
  { pelvisDriftMaxPx: 1 }, { baselineDriftMaxPx: 1 },
  { clippingDetected: true }, { neighboringFragmentsDetected: true },
  { scaleUniform: false }, { timingUniform: false },
  { globalPaletteUniform: false }, { backgroundFullyOpaque: false }]) {
  assert.equal(validateCoherentMasterReceipt({ ...accepted, ...failure,
    status: "blocked" }, expected), false);
}

assert.match(contract, /Existing immutable animation records/);
assert.match(contract, /New animation writes with that\s+legacy extraction mode are forbidden/);
assert.match(planning, /historical[\s\S]*read-only evidence/);

console.log("provider-native animated GIF contract vectors passed");
