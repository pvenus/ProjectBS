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

assert.match(contract, /Existing immutable animation records/);
assert.match(contract, /New animation writes with that\s+legacy extraction mode are forbidden/);
assert.match(planning, /historical[\s\S]*read-only evidence/);

console.log("provider-native animated GIF contract vectors passed");
