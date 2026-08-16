// Additive contract vectors for the open ink-wash character single-image profile.
// This test performs no provider call, image read, record write, or evaluation execution.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const gm = join(testDir, "..");
const docs = {
  visual: join(gm, "GeneratedMediaVisualPromptAuthoringGuide.md"),
  registry: join(gm, "GeneratedMediaAuthoringProfileRegistryGuide.md"),
  contract: join(gm, "GeneratedMediaImageGenOnlyContractGuide.md"),
  record: join(gm, "GeneratedMediaRecordGuide.md"),
  evaluation: join(gm, "GeneratedMediaCharacterExpressionEvaluationGuide.md"),
  pipeline: join(gm, "ImageGenCharacterImagePipelineGuide.md"),
  planning: join(gm, "..", "..", "character", "data-structures", "CharacterPlanningDataGuide.md"),
  planningPrompt: join(gm, "..", "..", "..", "task-prompts", "character", "ActCharacterPlanningPrompts.md"),
  authoringPrompt: join(gm, "..", "..", "..", "task-prompts", "content", "generated-media", "ImageGenCharacterImagePromptAuthoringPrompt.md"),
  generationPrompt: join(gm, "..", "..", "..", "task-prompts", "content", "generated-media", "ImageGenCharacterImageGenerationPrompt.md"),
  evaluationPrompt: join(gm, "..", "..", "..", "task-prompts", "content", "generated-media", "GeneratedMediaCharacterExpressionEvaluationPrompt.md"),
};
const text = Object.fromEntries(Object.entries(docs).map(([name, path]) =>
  [name, readFileSync(path, "utf8").replaceAll("\r\n", "\n")]));

function canonicalJson(value) {
  if (Array.isArray(value)) return "[" + value.map(canonicalJson).join(",") + "]";
  if (value !== null && typeof value === "object") {
    return "{" + Object.keys(value).sort().map((key) =>
      JSON.stringify(key) + ":" + canonicalJson(value[key])).join(",") + "}";
  }
  return JSON.stringify(value);
}

function hashObject(value) {
  return createHash("sha256").update(Buffer.from(canonicalJson(value), "utf8")).digest("hex");
}

function profileAfterHeading(source, heading) {
  const start = source.indexOf(heading);
  assert.notEqual(start, -1);
  const fence = String.fromCharCode(96).repeat(3);
  const jsonStart = source.indexOf(fence + "json\n", start);
  assert.notEqual(jsonStart, -1);
  const bodyStart = jsonStart + fence.length + 5;
  const bodyEnd = source.indexOf("\n" + fence, bodyStart);
  assert.notEqual(bodyEnd, -1);
  return JSON.parse(source.slice(bodyStart, bodyEnd));
}

const key = "projectbs_character_open_ink_wash_dynamic_contour@1.0.0";
const expectedHash = "37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd";
const referenceHash = "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf";
const profile = profileAfterHeading(text.visual,
  "### Open ink-wash dynamic-contour character profile");

assert.equal(profile.expressionProfileKey, key);
assert.equal(hashObject(profile), expectedHash);
assert.deepEqual(Object.keys(profile), [
  "expressionProfileKey", "applicability", "proportionAndAgeContract",
  "contourOmissionBudget", "mokSeonContract", "pigmentApplicationContract",
  "paletteRoleContract", "negativeSpaceContract", "backgroundContract",
  "identityAnchorContract", "acceptedStyleReferenceContract",
  "authoringProjectionContract", "negativeStyleLock", "positiveStyleLock",
]);
assert.deepEqual(profile.applicability.structureProfiles, ["character_single_image_v2"]);
assert.equal(profile.applicability.characterAnimationInheritance, "prohibited");
assert.deepEqual(profile.proportionAndAgeContract.fullBodyHeadCount,
  { minimum: 4, maximum: 5, target: 4.25 });
assert.equal(profile.proportionAndAgeContract.presentation, "young_adult");
assert.equal(profile.proportionAndAgeContract.minorOrChildCoding, "prohibited");
assert.deepEqual(
  [profile.contourOmissionBudget.minimum, profile.contourOmissionBudget.maximum,
    profile.contourOmissionBudget.target], [35, 55, 45]);
assert.deepEqual(profile.mokSeonContract.requiredStrokePhases,
  ["brush_start", "directional_drag", "dry_end"]);
assert.equal(profile.mokSeonContract.directionalWeight, "required");
assert.deepEqual(profile.pigmentApplicationContract.media,
  ["rough_watercolor", "rough_pastel"]);
assert.equal(profile.pigmentApplicationContract.controlledBleedBeyondOutline, "required");
assert.equal(profile.pigmentApplicationContract.controlledMisalignmentBeyondOutline, "required");
assert.deepEqual(profile.paletteRoleContract.roles.map((role) => role.colorFamily),
  ["faded_blue_gray_or_indigo", "dusty_gray_brown", "muted_ochre"]);
assert.equal(profile.negativeSpaceContract.minimumAchromaticOrUnpaintedPercent, 70);
assert.deepEqual(profile.negativeSpaceContract.scopes, ["figure_interior", "full_canvas"]);
assert.deepEqual(profile.backgroundContract.generationBackground,
  { mode: "removable_solid", colorFamily: "warm_ivory" });
for (const field of ["halo", "vignette", "scene", "shadow"])
  assert.equal(profile.backgroundContract[field], "prohibited");
assert.equal(profile.acceptedStyleReferenceContract.sha256, referenceHash);
assert.equal(profile.acceptedStyleReferenceContract.status,
  "audit_only_unbound_without_durable_project_relative_copy");
assert.match(profile.acceptedStyleReferenceContract.canonicalReferenceBinding,
  /^prohibited_until_reviewed_durable_project_relative_copy/);
assert.equal(profile.negativeStyleLock.length, 7);
assert.equal(profile.positiveStyleLock.length, 7);

const sparse = profileAfterHeading(text.visual, "### Sparse ink pastel motion profile");
const boldV2 = profileAfterHeading(text.visual, "### Bold-outline accepted-result alignment profile");
assert.equal(hashObject(sparse),
  "b5ce18d11e249598ad6da13d59340cf7cede3d2896259dcc5e02dbbf98e80443");
assert.equal(hashObject(boldV2),
  "5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3");

function validatePlanningSelection(selection, payloadHash, structureProfile) {
  if (selection !== key || payloadHash !== expectedHash)
    throw new Error("character_style_profile_conflict");
  if (structureProfile !== "character_single_image_v2")
    throw new Error("character_style_profile_conflict");
  return { expressionProfileKey: selection, expressionProfilePayloadHash: payloadHash };
}
assert.deepEqual(validatePlanningSelection(key, expectedHash, "character_single_image_v2"),
  { expressionProfileKey: key, expressionProfilePayloadHash: expectedHash });
assert.throws(() => validatePlanningSelection(key, expectedHash, "character_animation_v2"),
  /character_style_profile_conflict/);
assert.throws(() => validatePlanningSelection(key, "0".repeat(64), "character_single_image_v2"),
  /character_style_profile_conflict/);

const policyMembers = Object.keys(profile).filter((member) =>
  !["expressionProfileKey", "negativeStyleLock", "positiveStyleLock"].includes(member));
function validatePromptProjection(payload, evidenceMembers, promptMembers, referenceBindings) {
  if (hashObject(payload) !== expectedHash)
    throw new Error("open_ink_wash_profile_projection_mismatch");
  if (policyMembers.some((member) => !Object.hasOwn(payload, member)))
    throw new Error("missing_open_ink_wash_profile_projection");
  if (policyMembers.some((member) => !evidenceMembers.includes(member)))
    throw new Error("open_ink_wash_profile_evidence_incomplete");
  if (policyMembers.some((member) => !promptMembers.includes(member)))
    throw new Error("provider_prompt_open_ink_wash_projection_missing");
  if (referenceBindings.length !== 0)
    throw new Error("open_ink_wash_reference_role_invalid");
  return true;
}
assert.equal(validatePromptProjection(profile, policyMembers, policyMembers, []), true);
const drifted = structuredClone(profile);
drifted.contourOmissionBudget.maximum = 56;
assert.throws(() => validatePromptProjection(drifted, policyMembers, policyMembers, []),
  /open_ink_wash_profile_projection_mismatch/);
assert.throws(() => validatePromptProjection(profile, policyMembers.slice(1), policyMembers, []),
  /open_ink_wash_profile_evidence_incomplete/);
assert.throws(() => validatePromptProjection(profile, policyMembers, policyMembers.slice(0, -1), []),
  /provider_prompt_open_ink_wash_projection_missing/);
assert.throws(() => validatePromptProjection(profile, policyMembers, policyMembers,
  [{ role: "character_reference", projectRelativePath: "invented.png", sha256: referenceHash }]),
  /open_ink_wash_reference_role_invalid/);

const passingObservation = {
  headCount: 4.25, youngAdult: true, childCoded: false, omission: 45,
  brushStart: true, directionalDrag: true, dryEnd: true, pressureVariable: true,
  directionalWeight: true, stickerClean: false, uniformOutline: false,
  vectorClean: false, broadRoughPigment: true, controlledBleed: true,
  controlledMisalignment: true, cleanCelFill: false, decorativeSmallSplashes: false,
  separatePaletteRoles: true, figureNegativeSpace: 70, canvasNegativeSpace: 70,
  removableWarmIvory: true, halo: false, vignette: false, scene: false,
  shadow: false, identityEquipmentStable: true, referenceRoleValid: true,
};

function semanticGate(value, stage) {
  const prefix = "character_" + stage + "_open_ink_wash_";
  if (value.headCount < 4 || value.headCount > 5 || !value.youngAdult || value.childCoded)
    throw new Error(prefix + "proportion_age_gate_failed");
  if (value.omission < 35 || value.omission > 55 || !value.brushStart ||
      !value.directionalDrag || !value.dryEnd || !value.pressureVariable ||
      !value.directionalWeight || value.stickerClean || value.uniformOutline ||
      value.vectorClean)
    throw new Error(prefix + "contour_mok_seon_gate_failed");
  if (!value.broadRoughPigment || !value.controlledBleed ||
      !value.controlledMisalignment || value.cleanCelFill ||
      value.decorativeSmallSplashes || !value.separatePaletteRoles ||
      value.figureNegativeSpace < 70 || value.canvasNegativeSpace < 70)
    throw new Error(prefix + "pigment_negative_space_gate_failed");
  if (!value.removableWarmIvory || value.halo || value.vignette || value.scene || value.shadow)
    throw new Error(prefix + "background_gate_failed");
  if (!value.identityEquipmentStable)
    throw new Error(prefix + "identity_equipment_gate_failed");
  if (!value.referenceRoleValid)
    throw new Error(prefix + "reference_role_gate_failed");
  return true;
}

assert.equal(semanticGate(passingObservation, "generation"), true);
assert.equal(semanticGate(passingObservation, "evaluation"), true);
for (const [change, token] of [
  [{ childCoded: true }, "proportion_age_gate_failed"],
  [{ omission: 56 }, "contour_mok_seon_gate_failed"],
  [{ dryEnd: false }, "contour_mok_seon_gate_failed"],
  [{ cleanCelFill: true }, "pigment_negative_space_gate_failed"],
  [{ canvasNegativeSpace: 69 }, "pigment_negative_space_gate_failed"],
  [{ halo: true }, "background_gate_failed"],
  [{ identityEquipmentStable: false }, "identity_equipment_gate_failed"],
  [{ referenceRoleValid: false }, "reference_role_gate_failed"],
]) {
  assert.throws(() => semanticGate({ ...passingObservation, ...change }, "generation"),
    new RegExp("character_generation_open_ink_wash_" + token));
  assert.throws(() => semanticGate({ ...passingObservation, ...change }, "evaluation"),
    new RegExp("character_evaluation_open_ink_wash_" + token));
}

for (const name of ["registry", "planning", "planningPrompt", "record", "evaluation",
  "pipeline", "authoringPrompt", "generationPrompt", "evaluationPrompt"]) {
  assert.ok(text[name].includes(key), name + " missing profile key");
}
for (const name of ["visual", "registry", "planning", "record"]) {
  assert.ok(text[name].includes(expectedHash), name + " missing payload hash");
}
for (const token of [
  "missing_open_ink_wash_profile_projection",
  "character_generation_open_ink_wash_contour_mok_seon_gate_failed",
  "character_evaluation_open_ink_wash_reference_role_gate_failed",
]) assert.ok(text.contract.includes(token));
assert.ok(text.visual.includes("referenceBindings") &&
  text.visual.includes("empty for this audit evidence"));
assert.ok(!text.registry.includes("baseExpressionProfileKey=" + key));

console.log({
  expressionProfileKey: key,
  expressionProfilePayloadHash: expectedHash,
  acceptedStyleReferenceSha256: referenceHash,
  providerCalled: false,
  submitCount: 0,
  cost: 0,
});
console.log("generated media open ink-wash profile contract vectors: PASS");
