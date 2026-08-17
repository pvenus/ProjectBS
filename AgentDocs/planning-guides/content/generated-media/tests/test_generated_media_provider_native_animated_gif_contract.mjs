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
  assert.match(surface, /generated_media_attack_gif_final_validation_receipt_v1/);
  assert.match(surface, /pelvis/i);
  assert.match(surface, /baseline/i);
  assert.match(surface, /shared[^\n]*width basis/i);
  assert.match(surface, /neighboring/i);
  assert.match(surface, /global palette/i);
  assert.match(surface, /fully opaque\s+background/i);
  assert.match(surface, /no[^\n]*clipping/i);
}

const receiptKeys = ["schemaVersion", "animationRequestId",
  "originalAnimatedGifSha256", "width", "height", "frameCount",
  "sharedWidthBasis", "pelvisDriftMaxPx", "baselineDriftMaxPx",
  "scaleUniform", "timingUniform", "globalPaletteUniform",
  "backgroundFullyOpaque", "clippingDetected",
  "neighboringFragmentsDetected", "status"];

function validateAcceptedAttackReceipt(receipt, expected) {
  assert.deepEqual(Object.keys(receipt).sort(), [...receiptKeys].sort());
  assert.equal(receipt.schemaVersion,
    "generated_media_attack_gif_final_validation_receipt_v1");
  assert.equal(receipt.originalAnimatedGifSha256, expected.sha256);
  assert.equal(receipt.width, expected.width);
  assert.equal(receipt.height, expected.height);
  assert.equal(receipt.frameCount, expected.frameCount);
  const valid = receipt.pelvisDriftMaxPx === 0
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
  schemaVersion: "generated_media_attack_gif_final_validation_receipt_v1",
  animationRequestId: "character.seojin.1.attack.draw_slash.one_shot.v14",
  originalAnimatedGifSha256: "8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621",
  width: 640, height: 512, frameCount: 6,
  sharedWidthBasis: "longest_clean_left_right_margin",
  pelvisDriftMaxPx: 0, baselineDriftMaxPx: 0,
  scaleUniform: true, timingUniform: true, globalPaletteUniform: true,
  backgroundFullyOpaque: true, clippingDetected: false,
  neighboringFragmentsDetected: false, status: "valid",
};
const expected = { sha256: accepted.originalAnimatedGifSha256,
  width: 640, height: 512, frameCount: 6 };
assert.equal(validateAcceptedAttackReceipt(accepted, expected), true);
for (const drift of [{ pelvisDriftMaxPx: 1 }, { baselineDriftMaxPx: 1 },
  { clippingDetected: true }, { neighboringFragmentsDetected: true },
  { scaleUniform: false }, { timingUniform: false },
  { globalPaletteUniform: false }, { backgroundFullyOpaque: false }]) {
  assert.equal(validateAcceptedAttackReceipt({ ...accepted, ...drift,
    status: "blocked" }, expected), false);
}

assert.match(contract, /Existing immutable animation records/);
assert.match(contract, /New animation writes with that\s+legacy extraction mode are forbidden/);
assert.match(planning, /historical[\s\S]*read-only evidence/);

console.log("provider-native animated GIF contract vectors passed");
