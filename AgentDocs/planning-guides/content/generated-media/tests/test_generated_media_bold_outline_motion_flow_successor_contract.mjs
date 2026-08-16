import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const generatedMedia = path.resolve(here, "..");
const repo = path.resolve(generatedMedia, "../../../..");
const read = (relative) => fs.readFileSync(path.join(repo, relative), "utf8");
const visual = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md");
const registry = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md");
const central = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md");
const router = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md");
const authoring = read("AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md");
const generation = read("AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md");
const evaluation = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md");

const key = "projectbs_character_bold_outline_attack_motion_flow@1.0.0";
const hash = "1c828ef73b1de41453197f0d2fef80eebb069e42767d3f017ccb8dab0b947c8c";
const baseKey = "projectbs_character_bold_outline_compressed_detail@2.0.0";
const baseHash = "5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3";
const marker = "### Bold-outline attack motion-flow successor profile";
const section = visual.slice(visual.indexOf(marker));
const start = section.indexOf("{");
const end = section.indexOf("\n}\n", start) + 2;
const profile = JSON.parse(section.slice(start, end));

function canonicalJson(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  return `{${Object.keys(value).sort().map((k) => `${JSON.stringify(k)}:${canonicalJson(value[k])}`).join(",")}}`;
}

assert.deepEqual(Object.keys(profile), [
  "expressionProfileKey", "baseProfileBinding", "animationApplicability",
  "motionFlowContract", "frameContinuityContract", "authoringProjectionContract",
  "negativeAnimationLock", "positiveAnimationLock"
]);
assert.equal(profile.expressionProfileKey, key);
assert.equal(crypto.createHash("sha256").update(canonicalJson(profile)).digest("hex"), hash);
assert.equal(profile.baseProfileBinding.expressionProfileKey, baseKey);
assert.equal(profile.baseProfileBinding.expressionProfilePayloadHash, baseHash);
assert.equal(profile.baseProfileBinding.requiredExternalOutlineSourcePx, 18);
assert.equal(profile.baseProfileBinding.requiredInternalLineSourcePx, 8);
assert.deepEqual([
  profile.baseProfileBinding.requiredMaximumTotalVisibleMarks,
  profile.baseProfileBinding.requiredMaximumInternalLineMarks,
  profile.baseProfileBinding.requiredMaximumSecondaryFoldMarksPerGarmentRegion
], [64, 56, 5]);
assert.equal(profile.animationApplicability.motionClass, "attack");
assert.equal(profile.animationApplicability.singleImageSelection, "prohibited");
assert.deepEqual(profile.motionFlowContract.fadedIndigoSwordTorsoBrushFlow.markCountPerActiveFrame,
  { minimum: 3, maximum: 5 });
assert.deepEqual(profile.authoringProjectionContract.requiredApprovedMotionBindings,
  ["motionDirection", "swordArc", "torsoRotation", "shoulderInertia", "hemInertia",
    "darkNeutralInkTrajectory", "keyPoseOrder", "frameContinuityAnchors"]);
assert.equal(profile.negativeAnimationLock.length, 4);
assert.equal(profile.positiveAnimationLock.length, 4);

assert.match(registry, new RegExp(`expressionProfileKey=${key}`));
assert.match(registry, new RegExp(`expressionProfilePayloadHash=${hash}`));
assert.match(registry, /appliesTo=character_animation_v2/);
assert.match(registry, /Both\s+bold-outline compressed-detail versions remain intentionally single-image-only/);

const tokens = [
  "bold_outline_motion_successor_reference_mismatch",
  "bold_outline_motion_flow_not_attack",
  "missing_bold_outline_motion_flow_planning_bindings",
  "bold_outline_motion_flow_base_projection_mismatch",
  "bold_outline_motion_flow_evidence_omission",
  "provider_prompt_bold_outline_motion_flow_projection_missing",
  "character_generation_bold_outline_motion_flow_gate_failed",
  "character_generation_bold_outline_motion_continuity_gate_failed",
  "character_generation_bold_outline_motion_identity_equipment_gate_failed",
  "character_evaluation_bold_outline_motion_flow_gate_failed",
  "character_evaluation_bold_outline_motion_continuity_gate_failed",
  "character_evaluation_bold_outline_motion_identity_equipment_gate_failed"
];
for (const token of tokens) assert.match(central, new RegExp(token));
for (const token of tokens.slice(0, 3)) assert.match(router, new RegExp(token));
for (const token of tokens.slice(3, 6)) assert.match(authoring, new RegExp(token));
for (const token of tokens.slice(6, 9)) assert.match(generation, new RegExp(token));
for (const token of tokens.slice(9)) assert.match(evaluation, new RegExp(token));

function route({ referenceKey = baseKey, referenceHash = baseHash, attack = true,
  bindings = profile.authoringProjectionContract.requiredApprovedMotionBindings } = {}) {
  if (referenceKey !== baseKey || referenceHash !== baseHash) {
    throw new Error("bold_outline_motion_successor_reference_mismatch");
  }
  if (!attack) throw new Error("bold_outline_motion_flow_not_attack");
  if (bindings.length !== 8) throw new Error("missing_bold_outline_motion_flow_planning_bindings");
  return { animationRequestId: "one.attack.animation", successorKey: key };
}
assert.deepEqual(route(), { animationRequestId: "one.attack.animation", successorKey: key });
assert.throws(() => route({ referenceHash: "0".repeat(64) }), /bold_outline_motion_successor_reference_mismatch/);
assert.throws(() => route({ attack: false }), /bold_outline_motion_flow_not_attack/);
assert.throws(() => route({ bindings: ["motionDirection"] }), /missing_bold_outline_motion_flow_planning_bindings/);

function author({ baseProjection = true, evidence = true, providerProjection = true } = {}) {
  if (!baseProjection) throw new Error("bold_outline_motion_flow_base_projection_mismatch");
  if (!evidence) throw new Error("bold_outline_motion_flow_evidence_omission");
  if (!providerProjection) throw new Error("provider_prompt_bold_outline_motion_flow_projection_missing");
  return "ready_for_generation";
}
assert.equal(author(), "ready_for_generation");
assert.throws(() => author({ baseProjection: false }), /bold_outline_motion_flow_base_projection_mismatch/);
assert.throws(() => author({ evidence: false }), /bold_outline_motion_flow_evidence_omission/);
assert.throws(() => author({ providerProjection: false }), /provider_prompt_bold_outline_motion_flow_projection_missing/);

function generate({ motionFlow = true, continuity = true, anchors = true } = {}) {
  if (!motionFlow) throw new Error("character_generation_bold_outline_motion_flow_gate_failed");
  if (!continuity) throw new Error("character_generation_bold_outline_motion_continuity_gate_failed");
  if (!anchors) throw new Error("character_generation_bold_outline_motion_identity_equipment_gate_failed");
  return { providerCalled: true };
}
assert.throws(() => generate({ motionFlow: false }), /character_generation_bold_outline_motion_flow_gate_failed/);
assert.throws(() => generate({ continuity: false }), /character_generation_bold_outline_motion_continuity_gate_failed/);
assert.throws(() => generate({ anchors: false }), /character_generation_bold_outline_motion_identity_equipment_gate_failed/);

function evaluate({ motionFlow = true, continuity = true, anchors = true } = {}) {
  if (!motionFlow) throw new Error("character_evaluation_bold_outline_motion_flow_gate_failed");
  if (!continuity) throw new Error("character_evaluation_bold_outline_motion_continuity_gate_failed");
  if (!anchors) throw new Error("character_evaluation_bold_outline_motion_identity_equipment_gate_failed");
  return "PASS";
}
assert.equal(evaluate(), "PASS");
assert.throws(() => evaluate({ motionFlow: false }), /character_evaluation_bold_outline_motion_flow_gate_failed/);
assert.throws(() => evaluate({ continuity: false }), /character_evaluation_bold_outline_motion_continuity_gate_failed/);
assert.throws(() => evaluate({ anchors: false }), /character_evaluation_bold_outline_motion_identity_equipment_gate_failed/);

console.log({ expressionProfileKey: key, expressionProfilePayloadHash: hash, baseKey, baseHash });
console.log("generated media bold-outline attack motion-flow successor contract: PASS");
