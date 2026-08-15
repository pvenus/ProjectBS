import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function hashObject(value) {
  return createHash("sha256").update(canonicalJson(value), "utf8").digest("hex");
}

function assertClosedKeys(value, keys) {
  assert.deepEqual(Object.keys(value).sort(), [...keys].sort());
}

function profileFromGuide(guide, key) {
  const start = guide.indexOf(key);
  assert.notEqual(start, -1, `missing profile marker ${key}`);
  const match = guide.slice(start).match(/```json\r?\n([\s\S]*?)\r?\n```/);
  assert.ok(match, `missing profile payload ${key}`);
  return JSON.parse(match[1]);
}

const root = join(dirname(fileURLToPath(import.meta.url)), "..", "..", "..", "..");
const generatedMedia = join(root, "planning-guides", "content", "generated-media");
const prompts = join(root, "task-prompts", "content", "generated-media");
const guide = readFileSync(join(generatedMedia, "GeneratedMediaVisualPromptAuthoringGuide.md"), "utf8")
  .replace(/\r\n/g, "\n");
const registry = readFileSync(join(generatedMedia, "GeneratedMediaAuthoringProfileRegistryGuide.md"), "utf8")
  .replace(/\r\n/g, "\n");
const contract = readFileSync(join(generatedMedia, "GeneratedMediaImageGenOnlyContractGuide.md"), "utf8")
  .replace(/\r\n/g, "\n");
const recordGuide = readFileSync(join(generatedMedia, "GeneratedMediaRecordGuide.md"), "utf8")
  .replace(/\r\n/g, "\n");
const pipeline = readFileSync(join(generatedMedia, "ImageGenCharacterImagePipelineGuide.md"), "utf8")
  .replace(/\r\n/g, "\n");
const evaluation = readFileSync(join(generatedMedia, "GeneratedMediaCharacterExpressionEvaluationGuide.md"), "utf8")
  .replace(/\r\n/g, "\n");
const authoringPrompt = readFileSync(join(prompts, "ImageGenCharacterImagePromptAuthoringPrompt.md"), "utf8")
  .replace(/\r\n/g, "\n");
const generationPrompt = readFileSync(join(prompts, "ImageGenCharacterImageGenerationPrompt.md"), "utf8")
  .replace(/\r\n/g, "\n");
const evaluationPrompt = readFileSync(join(prompts, "GeneratedMediaCharacterExpressionEvaluationPrompt.md"), "utf8")
  .replace(/\r\n/g, "\n");
const planningGuide = readFileSync(join(root, "planning-guides", "character", "data-structures",
  "CharacterPlanningDataGuide.md"), "utf8").replace(/\r\n/g, "\n");

const key = "projectbs_character_bold_outline_compressed_detail@1.0.0";
const expectedHash = "dc5db9990f26dd1ed0ebc25c6c2b46a10b68cb4ca3248e69f7c27b28e1568b33";
const profile = profileFromGuide(guide, key);
assert.equal(hashObject(profile), expectedHash);
assertClosedKeys(profile, [
  "expressionProfileKey", "proportionProjection", "outlineHierarchy",
  "facialSimplificationBudget", "compressedDetailBudget", "colorSignatureContract",
  "inkTreatment", "authoringProjectionContract", "negativeStyleLock", "positiveStyleLock",
]);
assert.equal(profile.expressionProfileKey, key);
assert.deepEqual(profile.proportionProjection.fullBodyHeadCount,
  { minimum: 4, maximum: 5, target: 4.5 });
assert.deepEqual(profile.proportionProjection.headToFullHeightPercent,
  { minimum: 20, maximum: 25 });
assert.deepEqual(profile.proportionProjection.rejectNaturalisticAdultHeadCountRange,
  { minimum: 6.5, maximum: 8 });
assert.deepEqual(profile.outlineHierarchy.externalOutlineSourcePx,
  { minimum: 16, maximum: 22 });
assert.deepEqual(profile.outlineHierarchy.externalOutlineTargetPx,
  { minimum: 1.5, maximum: 2.0625 });
assert.equal(profile.outlineHierarchy.minimumExternalToInternalThicknessRatio, 2);
assert.equal(profile.facialSimplificationBudget.maximumTotalMarks, 9);
assert.deepEqual(profile.facialSimplificationBudget.componentMaximums,
  { browsAndEyes: 4, nose: 1, mouth: 1, jawAndFaceShape: 3 });
assert.equal(profile.compressedDetailBudget.maximumSecondaryFoldMarksPerGarmentRegion, 3);
assert.deepEqual(profile.colorSignatureContract.maximumCharacterCoveragePercent,
  { minimum: 1, maximum: 35 });
assert.deepEqual(profile.colorSignatureContract.maximumColorMasses,
  { minimum: 1, maximum: 4 });
assert.equal(profile.inkTreatment.silhouetteErosion, "prohibited");
for (const lock of [...profile.negativeStyleLock, ...profile.positiveStyleLock]) {
  assertClosedKeys(lock, ["constraintId", "statement", "authorityRef"]);
}
assert.equal(new Set([...profile.negativeStyleLock, ...profile.positiveStyleLock]
  .map(({ constraintId }) => constraintId)).size,
profile.negativeStyleLock.length + profile.positiveStyleLock.length);
assert.match(registry, new RegExp(`${key.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}.*${expectedHash}`, "s"));
assert.match(registry, /appliesTo=character_single_image_v2\nselection=explicit_approved_planning_fact_and_projection_required/);
assert.match(planningGuide, /expressionProfileProjection: # required only for projectbs_character_bold_outline_compressed_detail@1\.0\.0/);

function validProjection(overrides = {}) {
  const base = {
    expressionProfileKey: key,
    fullBodyHeadCount: 4.5,
    externalOutlineSourcePx: 18,
    internalLineSourcePx: 8,
    facialMarkBudget: {
      countingUnit: profile.facialSimplificationBudget.countingUnit,
      maximumTotalMarks: 9,
      componentMaximums: { browsAndEyes: 4, nose: 1, mouth: 1, jawAndFaceShape: 3 },
    },
    compressedDetailBudget: structuredClone(profile.compressedDetailBudget),
    primaryHue: "faded_indigo",
    primaryAnchorElements: ["outer_robe_trim", "waist_sash"],
    maximumCharacterCoveragePercent: 30,
    maximumColorMasses: 3,
    neutralOutlineColor: "ink_black",
    neutralWeaponColor: "charcoal_steel",
    evidenceIds: [
      ...profile.authoringProjectionContract.requiredPlanningBindings,
      ...profile.negativeStyleLock.map(({ constraintId }) => constraintId),
      ...profile.positiveStyleLock.map(({ constraintId }) => constraintId),
    ],
    promptLockIds: [...profile.negativeStyleLock, ...profile.positiveStyleLock]
      .map(({ constraintId }) => constraintId),
  };
  return { ...base, ...overrides };
}

function validateAuthoring(value) {
  if (value.expressionProfileKey !== key) throw new Error("character_style_profile_conflict");
  if (value.fullBodyHeadCount === undefined) throw new Error("missing_bold_outline_proportion_projection");
  if (value.fullBodyHeadCount < 4 || value.fullBodyHeadCount > 5)
    throw new Error("bold_outline_proportion_out_of_range");
  if (value.externalOutlineSourcePx === undefined || value.internalLineSourcePx === undefined)
    throw new Error("missing_bold_outline_hierarchy_projection");
  if (!Number.isInteger(value.externalOutlineSourcePx)
    || value.externalOutlineSourcePx < 16 || value.externalOutlineSourcePx > 22
    || !(value.internalLineSourcePx > 0)
    || value.externalOutlineSourcePx / value.internalLineSourcePx < 2) {
    throw new Error("bold_outline_hierarchy_out_of_range");
  }
  const face = value.facialMarkBudget;
  if (!face || face.countingUnit !== profile.facialSimplificationBudget.countingUnit
    || !face.componentMaximums) throw new Error("missing_bold_outline_facial_mark_budget");
  const limits = profile.facialSimplificationBudget.componentMaximums;
  const componentKeys = Object.keys(limits);
  if (!Number.isInteger(face.maximumTotalMarks) || face.maximumTotalMarks < 1
    || face.maximumTotalMarks > 9
    || componentKeys.some((name) => !Number.isInteger(face.componentMaximums[name])
      || face.componentMaximums[name] < 0 || face.componentMaximums[name] > limits[name])
    || componentKeys.reduce((sum, name) => sum + face.componentMaximums[name], 0)
      > face.maximumTotalMarks) throw new Error("bold_outline_facial_mark_budget_exceeded");
  if (!value.compressedDetailBudget)
    throw new Error("missing_bold_outline_compressed_detail_budget");
  if (canonicalJson(value.compressedDetailBudget) !== canonicalJson(profile.compressedDetailBudget))
    throw new Error("bold_outline_detail_budget_conflict");
  const secondaryHuePresent = Object.hasOwn(value, "secondaryHue");
  const secondaryAnchorsPresent = Object.hasOwn(value, "secondaryAnchorElements");
  if (typeof value.primaryHue !== "string" || value.primaryHue.length === 0
    || !Array.isArray(value.primaryAnchorElements) || value.primaryAnchorElements.length === 0
    || secondaryHuePresent !== secondaryAnchorsPresent
    || typeof value.neutralOutlineColor !== "string" || value.neutralOutlineColor.length === 0
    || typeof value.neutralWeaponColor !== "string" || value.neutralWeaponColor.length === 0) {
    throw new Error("missing_character_color_signature");
  }
  const lists = [value.primaryAnchorElements,
    ...(secondaryAnchorsPresent ? [value.secondaryAnchorElements] : [])];
  if (lists.some((list) => !Array.isArray(list) || list.length === 0
    || new Set(list).size !== list.length)
    || !Number.isInteger(value.maximumCharacterCoveragePercent)
    || value.maximumCharacterCoveragePercent < 1
    || value.maximumCharacterCoveragePercent > 35
    || !Number.isInteger(value.maximumColorMasses)
    || value.maximumColorMasses < 1 || value.maximumColorMasses > 4) {
    throw new Error("character_color_signature_invalid");
  }
  const evidence = [
    ...profile.authoringProjectionContract.requiredPlanningBindings,
    ...profile.negativeStyleLock.map(({ constraintId }) => constraintId),
    ...profile.positiveStyleLock.map(({ constraintId }) => constraintId),
  ];
  if (evidence.some((id) => !value.evidenceIds.includes(id)))
    throw new Error("bold_outline_profile_evidence_omission");
  const locks = [...profile.negativeStyleLock, ...profile.positiveStyleLock]
    .map(({ constraintId }) => constraintId);
  if (locks.some((id) => !value.promptLockIds.includes(id)))
    throw new Error("provider_prompt_bold_outline_projection_missing");
  return true;
}

assert.equal(validateAuthoring(validProjection()), true);
assert.equal(validateAuthoring(validProjection({
  secondaryHue: "dusty_ochre", secondaryAnchorElements: ["sash_knot"],
})), true);
assert.throws(() => validateAuthoring(validProjection({ fullBodyHeadCount: undefined })),
  /missing_bold_outline_proportion_projection/);
assert.throws(() => validateAuthoring(validProjection({ fullBodyHeadCount: 5.1 })),
  /bold_outline_proportion_out_of_range/);
assert.throws(() => validateAuthoring(validProjection({ externalOutlineSourcePx: undefined })),
  /missing_bold_outline_hierarchy_projection/);
assert.throws(() => validateAuthoring(validProjection({ internalLineSourcePx: 10 })),
  /bold_outline_hierarchy_out_of_range/);
assert.throws(() => validateAuthoring(validProjection({ facialMarkBudget: undefined })),
  /missing_bold_outline_facial_mark_budget/);
assert.throws(() => validateAuthoring(validProjection({
  facialMarkBudget: { countingUnit: profile.facialSimplificationBudget.countingUnit,
    maximumTotalMarks: 10, componentMaximums: { browsAndEyes: 4, nose: 1, mouth: 1, jawAndFaceShape: 3 } },
})), /bold_outline_facial_mark_budget_exceeded/);
assert.throws(() => validateAuthoring(validProjection({ compressedDetailBudget: { priority: "surface_first" } })),
  /bold_outline_detail_budget_conflict/);
assert.throws(() => validateAuthoring(validProjection({ compressedDetailBudget: undefined })),
  /missing_bold_outline_compressed_detail_budget/);
assert.throws(() => validateAuthoring(validProjection({ primaryHue: undefined })),
  /missing_character_color_signature/);
assert.throws(() => validateAuthoring(validProjection({ secondaryHue: "dusty_ochre" })),
  /missing_character_color_signature/);
assert.throws(() => validateAuthoring(validProjection({ maximumCharacterCoveragePercent: 36 })),
  /character_color_signature_invalid/);
assert.throws(() => validateAuthoring(validProjection({
  primaryAnchorElements: ["waist_sash", "waist_sash"],
})), /character_color_signature_invalid/);
assert.throws(() => validateAuthoring(validProjection({ evidenceIds: [] })),
  /bold_outline_profile_evidence_omission/);
assert.throws(() => validateAuthoring(validProjection({ promptLockIds: [] })),
  /provider_prompt_bold_outline_projection_missing/);

function semanticGate(value, stage) {
  const prefix = stage === "generation" ? "character_generation" : "character_evaluation";
  if (value.headCount < 4 || value.headCount > 5 || value.naturalisticTall || value.longLimbs)
    throw new Error(`${prefix}_bold_outline_proportion_gate_failed`);
  if (value.externalSourcePx < 16 || value.externalSourcePx > 22
    || value.externalSourcePx / value.internalSourcePx < 2 || !value.boldSilhouetteVisible)
    throw new Error(`${prefix}_bold_outline_hierarchy_gate_failed`);
  if (value.facialMarks > 9 || value.realisticFace)
    throw new Error(`${prefix}_bold_outline_facial_mark_budget_gate_failed`);
  if (value.denseDetail || value.scalesOrRivets || value.hatching || value.modeledShading)
    throw new Error(`${prefix}_bold_outline_detail_budget_gate_failed`);
  if (!value.colorAnchorsValid || value.colorCoverage > 35 || value.colorMasses > 4
    || value.fullGarmentFill || !value.neutralColorsValid)
    throw new Error(`${prefix}_bold_outline_color_signature_gate_failed`);
  return true;
}
const observedPass = {
  headCount: 4.5, naturalisticTall: false, longLimbs: false,
  externalSourcePx: 18, internalSourcePx: 8, boldSilhouetteVisible: true,
  facialMarks: 9, realisticFace: false, denseDetail: false, scalesOrRivets: false,
  hatching: false, modeledShading: false, colorAnchorsValid: true,
  colorCoverage: 30, colorMasses: 3, fullGarmentFill: false, neutralColorsValid: true,
};
assert.equal(semanticGate(observedPass, "generation"), true);
assert.equal(semanticGate(observedPass, "evaluation"), true);
for (const [change, token] of [
  [{ headCount: 6.5 }, "proportion"],
  [{ internalSourcePx: 10 }, "hierarchy"],
  [{ facialMarks: 10 }, "facial_mark_budget"],
  [{ scalesOrRivets: true }, "detail_budget"],
  [{ colorMasses: 5 }, "color_signature"],
]) {
  assert.throws(() => semanticGate({ ...observedPass, ...change }, "generation"),
    new RegExp(`character_generation_bold_outline_${token}_gate_failed`));
  assert.throws(() => semanticGate({ ...observedPass, ...change }, "evaluation"),
    new RegExp(`character_evaluation_bold_outline_${token}_gate_failed`));
}

for (const [oldKey, oldHash] of [
  ["projectbs_character_restrained_ink_line@1.0.0", "bda082ffe297c29cdc6b933a6c219ae67b11ae38bc784c198e4603c1741199cf"],
  ["projectbs_character_animation_ready_minimal_ink_line@1.0.0", "de3339457f05c3dfd6fb6f854c102079c5c14f54d908a474cca093943afc7e06"],
  ["projectbs_character_sparse_ink_pastel_motion@1.0.0", "b5ce18d11e249598ad6da13d59340cf7cede3d2896259dcc5e02dbbf98e80443"],
]) assert.equal(hashObject(profileFromGuide(guide, oldKey)), oldHash);

function resolveProfile(selection, assetType) {
  if (selection === undefined) return "projectbs_character_restrained_ink_line@1.0.0";
  const registered = new Set([
    "projectbs_character_animation_ready_minimal_ink_line@1.0.0",
    "projectbs_character_sparse_ink_pastel_motion@1.0.0",
    key,
  ]);
  if (!registered.has(selection) || (selection === key && assetType !== "character_single_image"))
    throw new Error("character_style_profile_conflict");
  return selection;
}
assert.equal(resolveProfile(key, "character_single_image"), key);
assert.throws(() => resolveProfile(key, "animation"), /character_style_profile_conflict/);
assert.throws(() => resolveProfile("unknown@1.0.0", "character_single_image"),
  /character_style_profile_conflict/);

const requiredDocs = [contract, recordGuide, pipeline, evaluation, authoringPrompt,
  generationPrompt, evaluationPrompt];
for (const document of requiredDocs) assert.ok(document.includes(key));
for (const token of [
  "missing_bold_outline_proportion_projection",
  "bold_outline_hierarchy_out_of_range",
  "bold_outline_facial_mark_budget_exceeded",
  "bold_outline_detail_budget_conflict",
  "character_color_signature_invalid",
  "character_generation_bold_outline_color_signature_gate_failed",
  "character_evaluation_bold_outline_color_signature_gate_failed",
]) assert.ok(contract.includes(token), `missing central token ${token}`);

assert.match(contract, /generated_media_hosted_preview_auto_approval_policy_v1/);
assert.match(contract, /submitCountMaximumPerScope: 1/);
assert.match(contract, /retryCountMaximumPerScope: 0/);

console.log({ expressionProfileKey: key, expressionProfilePayloadHash: expectedHash,
  negativeLockCount: profile.negativeStyleLock.length,
  positiveLockCount: profile.positiveStyleLock.length });
console.log("generated media bold-outline compressed-detail profile contract: PASS");
