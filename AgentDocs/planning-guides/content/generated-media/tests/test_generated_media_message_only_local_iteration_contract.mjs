import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const generatedMedia = path.resolve(here, "..");
const taskPrompts = path.resolve(generatedMedia, "..", "..", "..", "task-prompts", "content", "generated-media");
const read = (file) => fs.readFileSync(file, "utf8");

const guide = read(path.join(generatedMedia,
  "GeneratedMediaMessageOnlyLocalIterationGuide.md"));
const orchestration = read(path.join(taskPrompts,
  "GeneratedMediaPipelineOrchestrationPrompt.md"));
const routing = read(path.join(taskPrompts,
  "GeneratedMediaRequestRoutingPrompt.md"));

for (const surface of [guide, orchestration, routing]) {
  assert.match(surface, /generated_media_message_only_local_iteration_v1/);
  assert.match(surface, /self-contained/i);
  assert.match(surface, /message forwarding only/i);
  assert.match(surface, /output image path|outputImagePath/i);
  assert.match(surface, /chat-only [`]?PASS\s*\|\s*FAIL/i);
  assert.match(surface, /local_unpublished/);
  assert.match(surface, /separate|별도/);
}

for (const forbiddenArtifact of ["planning handoff", "routing record", "prompt record",
  "evaluation record", "receipt", "manifest", "package", "index", "snapshot",
  "JCS", "raw Git BLOB", "full suite"]) {
  assert.match(guide, new RegExp(forbiddenArtifact.replaceAll(" ", "\\s+"), "i"));
}

assert.match(guide, /Only actual media outputs produced by generation are persisted/);
assert.match(guide, /does not alter or delete historical/i);
assert.match(guide, /never silently[\s\S]*publishes, preserves, promotes, copies, or imports/);
assert.doesNotMatch(guide, /schemaVersion\s*:/);

console.log({
  policyKey: "generated_media_message_only_local_iteration_v1",
  workflow: ["planning_message", "generation", "chat_evaluation"],
  persistedArtifacts: ["actual_media_outputs"],
  providerCalled: false,
});
console.log("generated media message-only local iteration text contract: PASS");
