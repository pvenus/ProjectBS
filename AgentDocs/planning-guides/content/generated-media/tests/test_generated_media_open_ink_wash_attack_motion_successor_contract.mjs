import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const generatedMedia = path.resolve(here, "..");
const repo = path.resolve(generatedMedia, "../../../..");
const read = (relative) => fs.readFileSync(path.join(repo, relative), "utf8");
const successorGuide = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashAttackMotionSuccessorGuide.md");
const visual = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md");
const registry = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md");
const central = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md");
const planning = read("AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md");
const handoff = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md");
const router = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md");
const routingPrompt = read("AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md");
const authoring = read("AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md");
const generation = read("AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md");
const evaluation = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md");
const evaluationPrompt = read("AgentDocs/task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md");

const key = "projectbs_character_open_ink_wash_attack_motion@1.0.0";
const hash = "07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c";
const baseKey = "projectbs_character_open_ink_wash_dynamic_contour@2.0.0";
const baseHash = "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5";
const sparseKey = "projectbs_character_sparse_ink_pastel_motion@1.0.0";
const sparseHash = "b5ce18d11e249598ad6da13d59340cf7cede3d2896259dcc5e02dbbf98e80443";
const alphaKey = "generated_media_true_alpha_foreground@1.0.0";
const alphaHash = "2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108";

function canonical(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonical).join(",")}]`;
  return `{${Object.keys(value).sort().map((k) => `${JSON.stringify(k)}:${canonical(value[k])}`).join(",")}}`;
}
const sha = (value) => crypto.createHash("sha256").update(canonical(value)).digest("hex");
function parseFirstJson(source) {
  const match = source.replaceAll("\r\n", "\n").match(/```json\s*([\s\S]*?)\s*```/);
  assert.ok(match);
  return JSON.parse(match[1]);
}
function parseSectionJson(source, marker) {
  const section = source.replaceAll("\r\n", "\n").slice(source.indexOf(marker));
  return parseFirstJson(section);
}

const profile = parseFirstJson(successorGuide);
assert.deepEqual(Object.keys(profile), ["expressionProfileKey", "baseProfileBinding",
  "animationApplicability", "motionProjectionContract", "trueAlphaProjectionBinding",
  "authoringProjectionContract", "negativeAnimationLock", "positiveAnimationLock"]);
assert.equal(profile.expressionProfileKey, key);
assert.equal(sha(profile), hash);
assert.equal(profile.baseProfileBinding.expressionProfileKey, baseKey);
assert.equal(profile.baseProfileBinding.expressionProfilePayloadHash, baseHash);
assert.equal(profile.animationApplicability.motionClass, "attack");
assert.deepEqual(profile.animationApplicability.structureProfiles, ["character_animation_v2"]);
assert.equal(profile.animationApplicability.singleImageSelection, "prohibited");
assert.equal(profile.motionProjectionContract.orderedFrameCount, 6);
assert.deepEqual(profile.motionProjectionContract.requiredPlanningBindings,
  ["motionDirection", "swordArc", "torsoRotation", "keyPoseOrder",
    "frameContinuityAnchors", "dynamicPigment"]);
assert.equal(profile.trueAlphaProjectionBinding.projectionKey, alphaKey);
assert.equal(profile.trueAlphaProjectionBinding.projectionPayloadHash, alphaHash);
assert.equal(profile.trueAlphaProjectionBinding.requirements.length, 12);
assert.equal(profile.negativeAnimationLock.length, 4);
assert.equal(profile.positiveAnimationLock.length, 4);

const sparse = parseSectionJson(visual, "### Sparse ink pastel motion profile");
const openV2 = parseSectionJson(visual, "### Open ink-wash output-conformance successor profile");
assert.equal(sha(sparse), sparseHash);
assert.equal(sha(openV2), baseHash);
assert.deepEqual(sparse.identityAnchors.proportion.fullBodyHeadCount,
  { minimum: 3.75, maximum: 4.25 });
assert.deepEqual(openV2.proportionAndAgeContract.fullBodyHeadCount,
  { minimum: 4, maximum: 5, target: 4.25 });
assert.equal(sparse.accentPalette.allowed.length, 2);
assert.equal(openV2.paletteRoleContract.roles.length, 3);
assert.equal(openV2.negativeSpaceContract.minimumAchromaticOrUnpaintedPercent, 70);

for (const surface of [registry, central, planning, handoff, router, routingPrompt,
  authoring, generation, evaluation, evaluationPrompt]) {
  assert.match(surface, new RegExp(key.replaceAll(".", "\\.")));
  assert.match(surface, new RegExp(hash));
}
assert.match(registry, new RegExp(`expressionProfileKey=${sparseKey.replaceAll(".", "\\.")}`));
assert.match(registry, new RegExp(`expressionProfilePayloadHash=${sparseHash}`));
assert.match(registry, new RegExp(`baseExpressionProfileKey=${baseKey.replaceAll(".", "\\.")}`));
assert.match(registry, new RegExp(`requiredTransparentForegroundProjectionKey=${alphaKey.replaceAll(".", "\\.")}`));

const routingTokens = ["open_ink_attack_successor_reference_mismatch",
  "open_ink_attack_motion_not_attack", "missing_open_ink_attack_motion_bindings",
  "open_ink_attack_true_alpha_binding_mismatch"];
const authoringTokens = ["open_ink_attack_base_projection_mismatch",
  "open_ink_attack_evidence_omission", "provider_prompt_open_ink_attack_projection_missing"];
const generationTokens = ["character_generation_open_ink_attack_style_gate_failed",
  "character_generation_open_ink_attack_motion_continuity_gate_failed",
  "character_generation_open_ink_attack_true_alpha_gate_failed"];
const evaluationTokens = ["character_evaluation_open_ink_attack_style_gate_failed",
  "character_evaluation_open_ink_attack_motion_continuity_gate_failed",
  "character_evaluation_open_ink_attack_true_alpha_gate_failed"];
for (const token of routingTokens) assert.match(router + routingPrompt, new RegExp(token));
for (const token of authoringTokens) assert.match(authoring, new RegExp(token));
for (const token of generationTokens) assert.match(generation, new RegExp(token));
for (const token of evaluationTokens) assert.match(evaluation + evaluationPrompt, new RegExp(token));

const bindings = profile.motionProjectionContract.requiredPlanningBindings;
function route({ requestedKey = key, referenceKey = baseKey, referenceHash = baseHash, attack = true,
  motionBindings = bindings, alphaProjectionKey = alphaKey,
  alphaProjectionHash = alphaHash, frameCount = 6 } = {}) {
  if (requestedKey !== key) throw new Error("character_style_profile_conflict");
  if (referenceKey !== baseKey || referenceHash !== baseHash)
    throw new Error("open_ink_attack_successor_reference_mismatch");
  if (!attack) throw new Error("open_ink_attack_motion_not_attack");
  if (!Array.isArray(motionBindings) || motionBindings.length !== 6
      || motionBindings.some((v, i) => v !== bindings[i]))
    throw new Error("missing_open_ink_attack_motion_bindings");
  if (alphaProjectionKey !== alphaKey || alphaProjectionHash !== alphaHash || frameCount !== 6)
    throw new Error("open_ink_attack_true_alpha_binding_mismatch");
  return { successorKey: key, successorHash: hash };
}
assert.deepEqual(route(), { successorKey: key, successorHash: hash });
assert.throws(() => route({ requestedKey: sparseKey }), /character_style_profile_conflict/);
assert.throws(() => route({ referenceHash: sparseHash }), /open_ink_attack_successor_reference_mismatch/);
assert.throws(() => route({ attack: false }), /open_ink_attack_motion_not_attack/);
assert.throws(() => route({ motionBindings: bindings.slice(0, 5) }), /missing_open_ink_attack_motion_bindings/);
assert.throws(() => route({ alphaProjectionHash: "0".repeat(64) }), /open_ink_attack_true_alpha_binding_mismatch/);
assert.throws(() => route({ frameCount: 5 }), /open_ink_attack_true_alpha_binding_mismatch/);

console.log({ expressionProfileKey: key, expressionProfilePayloadHash: hash,
  baseKey, baseHash, alphaKey, alphaHash });
console.log("generated media open-ink attack motion successor contract: PASS");
