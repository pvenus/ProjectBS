// Terminal evaluation-to-project-promotion orchestration vectors.
// No role dispatch, project copy, provider call, record, or media mutation occurs.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const generatedMediaGuideRoot = join(testDir, "..");
const contentGuideRoot = join(generatedMediaGuideRoot, "..");
const promptRoot = join(contentGuideRoot, "..", "..", "task-prompts", "content");
const generatedMediaPromptRoot = join(promptRoot, "generated-media");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");

const routingGuide = read(join(generatedMediaGuideRoot,
  "GeneratedMediaRequestRoutingGuide.md"));
const orchestrationPrompt = read(join(generatedMediaPromptRoot,
  "GeneratedMediaPipelineOrchestrationPrompt.md"));
const promotionGuide = read(join(contentGuideRoot,
  "GeneratedImageProjectPromotionGuide.md"));
const promotionPrompt = read(join(promptRoot,
  "GeneratedImageProjectPromotionPrompt.md"));

const relayFields = ["requestId", "evaluationPackageId", "assetType",
  "domainType", "contentId", "evaluationRecordId", "replaceExisting",
  "replacementApprovalRef"];
const terminalStatuses = new Set(["promoted", "blocked", "not_promoted",
  "copy_failed"]);

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") return `{${Object.keys(value)
    .sort().map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
}

function dispatchKey(relay) {
  return createHash("sha256").update(Buffer.from(canonicalJson(relay), "utf8"))
    .digest("hex");
}

function validateRelay(relay) {
  assert.deepEqual(Object.keys(relay), relayFields);
  for (const key of relayFields.slice(0, 6)) {
    assert.equal(typeof relay[key], "string");
    assert.ok(relay[key].length > 0);
    assert.doesNotMatch(relay[key], /^(?:[A-Za-z]:[\\/]|[/\\]{2}|\/)/);
  }
  assert.equal(typeof relay.replaceExisting, "boolean");
  if (relay.replaceExisting) {
    assert.equal(typeof relay.replacementApprovalRef, "string");
    assert.ok(relay.replacementApprovalRef.length > 0);
    assert.doesNotMatch(relay.replacementApprovalRef,
      /^(?:[A-Za-z]:[\\/]|[/\\]{2}|\/)/);
  } else assert.equal(relay.replacementApprovalRef, null);
  return true;
}

function eligible(input) {
  return input.packagePresent === true && input.packageSealed === true
    && input.packageSchema === "generated_media_evaluation_package_v2"
    && input.evaluationSchema === "generated_image_evaluation_v1"
    && input.evaluationStatus === "completed" && input.result === "PASS"
    && input.passForProjectCopy === true
    && input.promotionStatus === "not_promoted"
    && input.previewOnly !== true && input.notEvaluated !== true
    && input.packageRouteRegistered === true;
}

const base = {
  packagePresent: true,
  packageSealed: true,
  packageSchema: "generated_media_evaluation_package_v2",
  evaluationSchema: "generated_image_evaluation_v1",
  evaluationStatus: "completed",
  result: "PASS",
  passForProjectCopy: true,
  promotionStatus: "not_promoted",
  previewOnly: false,
  notEvaluated: false,
  packageRouteRegistered: true,
};
assert.equal(eligible(base), true);
for (const invalid of [
  { packagePresent: false }, { packageSealed: false },
  { evaluationStatus: "incomplete" }, { result: "Conditional Pass" },
  { result: "Fail" }, { passForProjectCopy: false },
  { promotionStatus: "promoted" }, { previewOnly: true },
  { notEvaluated: true }, { packageRouteRegistered: false },
]) assert.equal(eligible({ ...base, ...invalid }), false);

const characterRelay = {
  requestId: "gmplan2.character_single_image.character.seojin.1.example",
  evaluationPackageId: "evalpkg2.character_single_image.character.seojin.1.example",
  assetType: "character_single_image",
  domainType: "character",
  contentId: "character.seojin.1",
  evaluationRecordId: "eval.character.character.seojin.1.example",
  replaceExisting: false,
  replacementApprovalRef: null,
};
assert.equal(validateRelay(characterRelay), true);
const animationRelay = { ...characterRelay,
  requestId: "gmplan2.animation.character.seojin.1.example",
  evaluationPackageId: "evalpkg2.animation.character.seojin.1.example",
  assetType: "animation",
  evaluationRecordId: "eval.animation.character.seojin.1.example" };
assert.equal(validateRelay(animationRelay), true);
assert.throws(() => validateRelay({ ...characterRelay,
  sourcePath: "C:/forbidden/source.png" }));
assert.throws(() => validateRelay({ ...characterRelay,
  replacementApprovalRef: "C:/approval.txt", replaceExisting: true }));

const activeDispatches = new Set();
function dispatchOnce(relay) {
  validateRelay(relay);
  const key = dispatchKey(relay);
  if (activeDispatches.has(key)) return "reused_terminal_no_dispatch";
  activeDispatches.add(key);
  return "dispatched_once";
}
assert.equal(dispatchOnce(characterRelay), "dispatched_once");
assert.equal(dispatchOnce(characterRelay), "reused_terminal_no_dispatch");
for (const status of terminalStatuses) assert.ok(terminalStatuses.has(status));

for (const surface of [routingGuide, orchestrationPrompt]) {
  assert.match(surface, /01a01094-7d22-7a51-b92e-bf6154769017/);
  assert.match(surface, /evaluationStatus[=:` ]+completed/);
  assert.match(surface, /passForProjectCopy[=:` ]+true/);
  assert.match(surface, /promotionStatus[=:` ]+not_promoted/);
  assert.match(surface, /preview/);
  assert.match(surface, /Conditional\s+Pass/);
  assert.match(surface, /promoted[\s\S]*blocked[\s\S]*not_promoted[\s\S]*copy_failed/);
  for (const field of relayFields) assert.match(surface, new RegExp(field));
}
for (const surface of [promotionGuide, promotionPrompt]) {
  assert.match(surface, /character_single_image[\s\S]*domainType=character/);
  assert.match(surface, /character_single_image_v2/);
  assert.match(surface, /animation[\s\S]*domainType=character/);
  assert.match(surface, /animation_gif_frame_set_v2/);
  assert.match(surface, /Assets\/ImagesGenerated\/Character\/portrait/);
  assert.match(surface, /Assets\/ImagesGenerated\/Character\/animation/);
  assert.match(surface, /legacy[\s\S]*character_animation/i);
}
assert.match(routingGuide, /no route returns to routing, generation,[\s\S]*evaluation/i);
assert.match(orchestrationPrompt, /되돌리지 않는다/);

console.log({ projectPromotionThreadId:
  "01a01094-7d22-7a51-b92e-bf6154769017",
relayFields, terminalStatuses: [...terminalStatuses],
characterDispatchKey: dispatchKey(characterRelay),
providerCalled: false, projectCopyCalled: false });
console.log("generated media project-promotion dispatch vectors: PASS");
