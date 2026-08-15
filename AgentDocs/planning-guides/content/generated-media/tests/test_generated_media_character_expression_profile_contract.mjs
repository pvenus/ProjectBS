import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

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

function assertClosedKeys(value, required) {
  assert.deepEqual(Object.keys(value).sort(), [...required].sort());
}

function exactProfileFromGuide(guide) {
  const marker = "The following payload is canonical and immutable for\n`projectbs_character_animation_ready_minimal_ink_line@1.0.0`:";
  const start = guide.indexOf(marker);
  assert.notEqual(start, -1);
  const match = guide.slice(start).match(/```json\r?\n([\s\S]*?)\r?\n```/);
  assert.ok(match);
  return JSON.parse(match[1]);
}

const testDir = dirname(fileURLToPath(import.meta.url));
const guidePath = join(testDir, "..", "GeneratedMediaVisualPromptAuthoringGuide.md");
const registryPath = join(testDir, "..", "GeneratedMediaAuthoringProfileRegistryGuide.md");
const planningGuidePath = join(testDir, "..", "..", "..", "character", "data-structures",
  "CharacterPlanningDataGuide.md");
const guide = readFileSync(guidePath, "utf8").replace(/\r\n/g, "\n");
const registry = readFileSync(registryPath, "utf8").replace(/\r\n/g, "\n");
const planningGuide = readFileSync(planningGuidePath, "utf8").replace(/\r\n/g, "\n");
const profile = exactProfileFromGuide(guide);
const profileHash = hashObject(profile);

assertClosedKeys(profile, [
  "expressionProfileKey", "proportionProjection", "detailDensityBudget",
  "colorValueBudget", "authoringProjectionContract", "negativeStyleLock",
  "positiveStyleLock",
]);
assert.equal(profile.expressionProfileKey,
  "projectbs_character_animation_ready_minimal_ink_line@1.0.0");
assertClosedKeys(profile.proportionProjection, [
  "fullBodyHeadCount", "headToFullHeightPercent", "limbPolicy",
  "rejectAboveHeadCount", "rejectNaturalisticAdultHeadCountRange",
]);
assert.deepEqual(profile.proportionProjection.fullBodyHeadCount,
  { minimum: 3.75, maximum: 4.25 });
assert.deepEqual(profile.proportionProjection.headToFullHeightPercent,
  { minimum: 24, maximum: 27 });
assert.equal(profile.proportionProjection.limbPolicy, "shortened_simplified");
assert.equal(profile.proportionProjection.rejectAboveHeadCount, 4.25);
assert.deepEqual(profile.proportionProjection.rejectNaturalisticAdultHeadCountRange,
  { minimum: 7, maximum: 8 });
assertClosedKeys(profile.detailDensityBudget, [
  "level", "silhouettePriority", "contourPolicy", "identityStrokeGroups",
  "identityEncoding", "flatValueMasses", "frameToFrameReproducibility", "forbidden",
]);
assert.deepEqual(profile.detailDensityBudget.identityStrokeGroups,
  ["face", "garment", "armor", "weapon"]);
assertClosedKeys(profile.colorValueBudget, [
  "accentHueCount", "accentHuePolicy", "valueMassPolicy", "gradients",
  "modeledShading", "cinematicOrPhysicalLighting", "realisticMaterialRendering",
]);
assert.deepEqual(profile.colorValueBudget.accentHueCount, { minimum: 1, maximum: 2 });
assertClosedKeys(profile.authoringProjectionContract, [
  "planningSelection", "planningValuePolicy", "requiredProjectionIds",
  "evidencePolicy", "promptInclusion", "conflictPolicy",
]);
for (const lock of [...profile.negativeStyleLock, ...profile.positiveStyleLock]) {
  assertClosedKeys(lock, ["constraintId", "statement", "authorityRef"]);
}
assert.equal(new Set([...profile.negativeStyleLock, ...profile.positiveStyleLock]
  .map(({ constraintId }) => constraintId)).size,
profile.negativeStyleLock.length + profile.positiveStyleLock.length);
assert.match(registry, new RegExp(`expressionProfilePayloadHash=${profileHash}`));
const fence = String.fromCharCode(96).repeat(3);
assert.ok(guide.includes(`exactly:\n\n${fence}text\n${profileHash}\n${fence}`));
assert.match(planningGuide,
  /expressionProfileKey: optional exact registered character expression-profile key/);
assert.match(planningGuide,
  /must be projected unchanged into the handoff snapshot `approvedFacts`/);

function resolvePlanningSelection(selection) {
  if (selection === undefined) return "projectbs_character_restrained_ink_line@1.0.0";
  if (selection === profile.expressionProfileKey) return selection;
  throw new Error("character_style_profile_conflict");
}

assert.equal(resolvePlanningSelection(undefined),
  "projectbs_character_restrained_ink_line@1.0.0");
assert.equal(resolvePlanningSelection(profile.expressionProfileKey), profile.expressionProfileKey);
assert.throws(() => resolvePlanningSelection("unknown@1.0.0"),
  /character_style_profile_conflict/);

function authoringProjection(overrides = {}) {
  const projectionIds = profile.authoringProjectionContract.requiredProjectionIds;
  const lockIds = [...profile.negativeStyleLock, ...profile.positiveStyleLock]
    .map(({ constraintId }) => constraintId);
  return {
    selection: profile.expressionProfileKey,
    fullBodyHeadCount: { minimum: 3.75, maximum: 4.25 },
    headToFullHeightPercent: { minimum: 24, maximum: 27 },
    limbPolicy: "shortened_simplified",
    detailDensityBudget: structuredClone(profile.detailDensityBudget),
    colorValueBudget: structuredClone(profile.colorValueBudget),
    evidenceIds: [...projectionIds, ...lockIds],
    ...overrides,
  };
}

function validateAuthoringProjection(value) {
  if (value.selection !== profile.expressionProfileKey) {
    throw new Error("character_style_profile_conflict");
  }
  if (!value.fullBodyHeadCount || !value.headToFullHeightPercent || !value.limbPolicy) {
    throw new Error("missing_character_proportion_projection");
  }
  const heads = value.fullBodyHeadCount;
  const percent = value.headToFullHeightPercent;
  if (heads.minimum < 3.75 || heads.maximum > 4.25 || heads.minimum > heads.maximum
    || percent.minimum < 24 || percent.maximum > 27 || percent.minimum > percent.maximum
    || value.limbPolicy !== "shortened_simplified") {
    throw new Error("character_proportion_out_of_range");
  }
  if (canonicalJson(value.detailDensityBudget) !== canonicalJson(profile.detailDensityBudget)) {
    throw new Error("missing_animation_safe_detail_budget");
  }
  if (canonicalJson(value.colorValueBudget) !== canonicalJson(profile.colorValueBudget)) {
    throw new Error("missing_character_color_value_budget");
  }
  const requiredEvidence = [
    ...profile.authoringProjectionContract.requiredProjectionIds,
    ...profile.negativeStyleLock.map(({ constraintId }) => constraintId),
    ...profile.positiveStyleLock.map(({ constraintId }) => constraintId),
  ];
  if (requiredEvidence.some((id) => !value.evidenceIds.includes(id))) {
    throw new Error("character_profile_evidence_omission");
  }
  return true;
}

assert.equal(validateAuthoringProjection(authoringProjection()), true);
assert.throws(() => validateAuthoringProjection(authoringProjection({
  fullBodyHeadCount: { minimum: 3.75, maximum: 4.26 },
})), /character_proportion_out_of_range/);
assert.throws(() => validateAuthoringProjection(authoringProjection({
  detailDensityBudget: { level: "dense" },
})), /missing_animation_safe_detail_budget/);
assert.throws(() => validateAuthoringProjection(authoringProjection({
  colorValueBudget: { accentHueCount: { minimum: 1, maximum: 3 } },
})), /missing_character_color_value_budget/);
assert.throws(() => validateAuthoringProjection(authoringProjection({ evidenceIds: [] })),
  /character_profile_evidence_omission/);

const scenePromptOriginal = [...profile.negativeStyleLock, ...profile.positiveStyleLock]
  .map(({ statement }) => statement).join("\n");
for (const lock of [...profile.negativeStyleLock, ...profile.positiveStyleLock]) {
  assert.ok(scenePromptOriginal.includes(lock.statement));
}

function semanticGate(observed, stage) {
  const prefix = stage === "generation" ? "character_generation" : "character_evaluation";
  if (observed.fullBodyHeadCount < 3.75 || observed.fullBodyHeadCount > 4.25
    || observed.headToFullHeightPercent < 24 || observed.headToFullHeightPercent > 27
    || observed.naturalisticSevenToEightHeads || observed.heroicTall) {
    throw new Error(`${prefix}_proportion_gate_failed`);
  }
  if (observed.denseRealisticDetail || observed.scalesOrRivets || observed.denseFolds
    || observed.hatching || observed.microtexture || observed.modeledShading) {
    throw new Error(`${prefix}_detail_density_gate_failed`);
  }
  if (observed.accentHueCount < 1 || observed.accentHueCount > 2
    || observed.gradients || observed.cinematicOrPhysicalLighting
    || observed.realisticMaterialRendering || observed.nonminimalValueMasses) {
    throw new Error(`${prefix}_color_value_gate_failed`);
  }
  return true;
}

const compliant = {
  fullBodyHeadCount: 4, headToFullHeightPercent: 25, naturalisticSevenToEightHeads: false,
  heroicTall: false, denseRealisticDetail: false, scalesOrRivets: false,
  denseFolds: false, hatching: false, microtexture: false, modeledShading: false,
  accentHueCount: 2, gradients: false, cinematicOrPhysicalLighting: false,
  realisticMaterialRendering: false, nonminimalValueMasses: false,
};
assert.equal(semanticGate(compliant, "generation"), true);
assert.equal(semanticGate(compliant, "evaluation"), true);
assert.throws(() => semanticGate({ ...compliant, fullBodyHeadCount: 4.5 }, "generation"),
  /character_generation_proportion_gate_failed/);
assert.throws(() => semanticGate({ ...compliant, scalesOrRivets: true }, "generation"),
  /character_generation_detail_density_gate_failed/);
assert.throws(() => semanticGate({ ...compliant, accentHueCount: 3 }, "generation"),
  /character_generation_color_value_gate_failed/);
assert.throws(() => semanticGate({ ...compliant, heroicTall: true }, "evaluation"),
  /character_evaluation_proportion_gate_failed/);
assert.throws(() => semanticGate({ ...compliant, hatching: true }, "evaluation"),
  /character_evaluation_detail_density_gate_failed/);
assert.throws(() => semanticGate({ ...compliant, gradients: true }, "evaluation"),
  /character_evaluation_color_value_gate_failed/);

function sparseProfileFromGuide(source) {
  const marker = "The following payload is canonical and immutable for\n`projectbs_character_sparse_ink_pastel_motion@1.0.0`";
  const start = source.indexOf(marker);
  assert.notEqual(start, -1);
  const match = source.slice(start).match(/```json\r?\n([\s\S]*?)\r?\n```/);
  assert.ok(match);
  return JSON.parse(match[1]);
}

const sparse = sparseProfileFromGuide(guide);
const sparseHash = hashObject(sparse);
assert.equal(sparseHash, "b5ce18d11e249598ad6da13d59340cf7cede3d2896259dcc5e02dbbf98e80443");
assertClosedKeys(sparse, ["expressionProfileKey", "contourOmissionBudget", "lineHierarchy",
  "negativeSpacePolicy", "pigmentBudget", "accentPalette", "pigmentApplication",
  "motionLinePolicy", "identityAnchors"]);
assert.equal(sparse.expressionProfileKey,
  "projectbs_character_sparse_ink_pastel_motion@1.0.0");
assert.equal(Object.hasOwn(sparse, "positiveStyleLock"), false);
assert.equal(Object.hasOwn(sparse, "negativeStyleLock"), false);
assert.deepEqual(sparse.contourOmissionBudget.main, { minimum: 35, maximum: 45 });
assert.deepEqual(sparse.contourOmissionBudget.animationFrame, { minimum: 35, maximum: 50 });
assert.equal(sparse.pigmentBudget.mainMaximumPigmentedArea, 18);
assert.deepEqual(sparse.pigmentBudget.mainAccentCount, { minimum: 4, maximum: 7 });
assert.deepEqual(sparse.pigmentBudget.animationFrameAccentCount, { minimum: 3, maximum: 6 });
assert.deepEqual(sparse.accentPalette.allowed,
  ["faded_indigo_navy", "dusty_ochre_gray_brown"]);
assert.deepEqual(sparse.identityAnchors.proportion.fullBodyHeadCount,
  { minimum: 3.75, maximum: 4.25 });
assert.match(registry, new RegExp(`${sparse.expressionProfileKey.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}.*${sparseHash}`, "s"));

function validateSparseAuthoringProjection(payload, evidenceMembers, promptMembers) {
  const expected = ["contourOmissionBudget", "lineHierarchy", "negativeSpacePolicy",
    "pigmentBudget", "accentPalette", "pigmentApplication", "motionLinePolicy",
    "identityAnchors"];
  if (expected.some((key) => !Object.hasOwn(payload, key)))
    throw new Error("missing_sparse_profile_projection");
  if (hashObject(payload) !== sparseHash)
    throw new Error("sparse_profile_projection_mismatch");
  if (expected.some((key) => !evidenceMembers.includes(key)))
    throw new Error("sparse_profile_evidence_incomplete");
  if (expected.some((key) => !promptMembers.includes(key)))
    throw new Error("provider_prompt_sparse_projection_missing");
  return true;
}
const sparseMembers = Object.keys(sparse).filter((key) => key !== "expressionProfileKey");
assert.equal(validateSparseAuthoringProjection(sparse, sparseMembers, sparseMembers), true);
const missingSparseMember = structuredClone(sparse);
delete missingSparseMember.motionLinePolicy;
assert.throws(() => validateSparseAuthoringProjection(missingSparseMember,
  sparseMembers, sparseMembers), /missing_sparse_profile_projection/);
const mismatchedSparse = structuredClone(sparse);
mismatchedSparse.pigmentBudget.mainMaximumPigmentedArea = 19;
assert.throws(() => validateSparseAuthoringProjection(mismatchedSparse,
  sparseMembers, sparseMembers), /sparse_profile_projection_mismatch/);
assert.throws(() => validateSparseAuthoringProjection(sparse,
  sparseMembers.slice(1), sparseMembers), /sparse_profile_evidence_incomplete/);
assert.throws(() => validateSparseAuthoringProjection(sparse,
  sparseMembers, sparseMembers.slice(0, -1)), /provider_prompt_sparse_projection_missing/);

function sparseMainGate(v) {
  if (v.headCount < 3.75 || v.headCount > 4.25 || v.naturalisticSevenToEightHeads)
    throw new Error("character_evaluation_proportion_gate_failed");
  if (v.closedContour || v.fullyInkedSilhouette)
    throw new Error("character_evaluation_sparse_contour_gate_failed");
  if (v.omission < 35 || v.omission > 45)
    throw new Error("character_evaluation_sparse_omission_budget_gate_failed");
  if (v.celFill || v.pigmentArea > 18 || v.offPalette || v.accents < 4 || v.accents > 7)
    throw new Error("character_evaluation_sparse_pigment_budget_gate_failed");
  if (!v.identityAnchorsStable)
    throw new Error("character_evaluation_identity_anchor_gate_failed");
  return true;
}

const mainPass = { headCount: 4, naturalisticSevenToEightHeads: false,
  closedContour: false, fullyInkedSilhouette: false, celFill: false,
  pigmentArea: 17, offPalette: false, omission: 40, accents: 5,
  identityAnchorsStable: true };
assert.equal(sparseMainGate(mainPass), true);
assert.throws(() => sparseMainGate({ ...mainPass, headCount: 7 }), /proportion_gate_failed/);
assert.throws(() => sparseMainGate({ ...mainPass, closedContour: true }), /contour_gate_failed/);
assert.throws(() => sparseMainGate({ ...mainPass, omission: 34 }), /omission_budget_gate_failed/);
assert.throws(() => sparseMainGate({ ...mainPass, accents: 8 }), /pigment_budget_gate_failed/);
assert.throws(() => sparseMainGate({ ...mainPass, celFill: true }), /pigment_budget_gate_failed/);
assert.throws(() => sparseMainGate({ ...mainPass, pigmentArea: 19 }), /pigment_budget_gate_failed/);
assert.throws(() => sparseMainGate({ ...mainPass, offPalette: true }), /pigment_budget_gate_failed/);

function sparseAnimationGate(frames, approvedFinalFrameCount) {
  assert.ok(Number.isInteger(approvedFinalFrameCount) && approvedFinalFrameCount > 0);
  assert.equal(frames.length, approvedFinalFrameCount);
  if (frames.every((frame) => frame.actionSignature === frames[0].actionSignature)
    || frames.some((frame) => !frame.lineMotionCue || !frame.pigmentMotionCue))
    throw new Error("character_evaluation_sparse_motion_gate_failed");
  if (frames.some((frame) => frame.omission < 35 || frame.omission > 50))
    throw new Error("character_evaluation_sparse_omission_budget_gate_failed");
  if (frames.some((frame) => frame.accents < 3 || frame.accents > 6
    || frame.offPalette || frame.celFill))
    throw new Error("character_evaluation_sparse_pigment_budget_gate_failed");
  if (frames.some((frame) => !frame.identityAnchorsStable))
    throw new Error("character_evaluation_identity_anchor_gate_failed");
  return true;
}
const sixFramePass = Array.from({ length: 6 }, (_, index) => ({
  actionSignature: `attack_${index}`, lineMotionCue: true, pigmentMotionCue: true,
  omission: 40, accents: 4, offPalette: false, celFill: false,
  identityAnchorsStable: true,
}));
assert.equal(sparseAnimationGate(sixFramePass, 6), true);
assert.equal(sparseAnimationGate(sixFramePass.slice(0, 4), 4), true);
assert.throws(() => sparseAnimationGate(sixFramePass.map((frame) => ({
  ...frame, actionSignature: "static" })), 6), /motion_gate_failed/);
assert.throws(() => sparseAnimationGate(sixFramePass.map((frame, index) => ({
  ...frame, pigmentMotionCue: index !== 2 })), 6), /motion_gate_failed/);
assert.throws(() => sparseAnimationGate(sixFramePass.map((frame, index) => ({
  ...frame, identityAnchorsStable: index !== 3 })), 6), /identity_anchor_gate_failed/);
assert.throws(() => sparseAnimationGate(sixFramePass.map((frame, index) => ({
  ...frame, accents: index === 4 ? 7 : frame.accents })), 6), /pigment_budget_gate_failed/);
assert.throws(() => sparseAnimationGate(sixFramePass.map((frame, index) => ({
  ...frame, omission: index === 1 ? 51 : frame.omission })), 6),
/omission_budget_gate_failed/);

function sparseGenerationBudgetGate({ artifactType, omission, accents, pigmentArea = 0,
  offPalette = false, celFill = false }) {
  const omissionRange = artifactType === "main" ? [35, 45] : [35, 50];
  const accentRange = artifactType === "main" ? [4, 7] : [3, 6];
  if (omission < omissionRange[0] || omission > omissionRange[1])
    throw new Error("character_generation_sparse_omission_budget_gate_failed");
  if (accents < accentRange[0] || accents > accentRange[1] || offPalette || celFill
    || (artifactType === "main" && pigmentArea > 18))
    throw new Error("character_generation_sparse_pigment_budget_gate_failed");
  return true;
}
assert.equal(sparseGenerationBudgetGate({ artifactType: "main", omission: 40,
  accents: 5, pigmentArea: 18 }), true);
assert.equal(sparseGenerationBudgetGate({ artifactType: "animation", omission: 50,
  accents: 3 }), true);
assert.throws(() => sparseGenerationBudgetGate({ artifactType: "main", omission: 46,
  accents: 5 }), /character_generation_sparse_omission_budget_gate_failed/);
assert.throws(() => sparseGenerationBudgetGate({ artifactType: "animation", omission: 40,
  accents: 7 }), /character_generation_sparse_pigment_budget_gate_failed/);

console.log({
  expressionProfileKey: profile.expressionProfileKey,
  expressionProfilePayloadHash: profileHash,
  negativeLockCount: profile.negativeStyleLock.length,
  positiveLockCount: profile.positiveStyleLock.length,
});
console.log({ sparseExpressionProfileKey: sparse.expressionProfileKey,
  sparseExpressionProfilePayloadHash: sparseHash });
console.log("generated media animation-ready character expression profile vectors: PASS");
