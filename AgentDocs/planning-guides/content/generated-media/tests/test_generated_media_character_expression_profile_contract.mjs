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

console.log({
  expressionProfileKey: profile.expressionProfileKey,
  expressionProfilePayloadHash: profileHash,
  negativeLockCount: profile.negativeStyleLock.length,
  positiveLockCount: profile.positiveStyleLock.length,
});
console.log("generated media animation-ready character expression profile vectors: PASS");
