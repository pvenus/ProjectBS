import assert from "node:assert/strict";
import { readFileSync } from "node:fs";

const guide = readFileSync(new URL("../GeneratedMediaCharacterExpressionEvaluationGuide.md", import.meta.url), "utf8");
const rolePrompt = readFileSync(new URL("../../../../task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md", import.meta.url), "utf8");
const commonPrompt = readFileSync(new URL("../../../../task-prompts/content/GeneratedImageEvaluationPrompt.md", import.meta.url), "utf8");

function classifySurfaceDetail(observed) {
  const allowedMainVariance = observed.assetType === "character_main_image"
    && observed.profile === "projectbs_character_open_ink_wash_dynamic_contour@2.0.0"
    && observed.lowContrast && observed.lowDensity && observed.flat
    && (observed.insideOneBroadShoulderMass || observed.limitedLegWrapBands)
    && !observed.enumerableConstruction && !observed.densePlatesOrScales
    && !observed.rivetsLacingFasteners && !observed.microtexture
    && !observed.modeledLightOrMaterial && !observed.hardGateConflict;
  if (allowedMainVariance) return {
    fatal: false,
    findingType: "minor_expressive_surface_variance",
    regenerationRequired: false,
    passCompatible: true,
  };
  if (observed.enumerableConstruction || observed.densePlatesOrScales
    || observed.rivetsLacingFasteners || observed.microtexture
    || observed.modeledLightOrMaterial || observed.hardGateConflict) {
    return { fatal: true,
      failureType: "character_evaluation_open_ink_wash_v2_surface_detail_gate_failed" };
  }
  return { fatal: false, findingType: null, regenerationRequired: false, passCompatible: true };
}

const bounded = {
  assetType: "character_main_image",
  profile: "projectbs_character_open_ink_wash_dynamic_contour@2.0.0",
  lowContrast: true,
  lowDensity: true,
  flat: true,
  insideOneBroadShoulderMass: true,
  limitedLegWrapBands: true,
  enumerableConstruction: false,
  densePlatesOrScales: false,
  rivetsLacingFasteners: false,
  microtexture: false,
  modeledLightOrMaterial: false,
  hardGateConflict: false,
};
assert.deepEqual(classifySurfaceDetail(bounded), {
  fatal: false,
  findingType: "minor_expressive_surface_variance",
  regenerationRequired: false,
  passCompatible: true,
});

for (const mutation of [
  { densePlatesOrScales: true },
  { rivetsLacingFasteners: true },
  { microtexture: true },
  { modeledLightOrMaterial: true },
  { hardGateConflict: true },
]) assert.equal(classifySurfaceDetail({ ...bounded, ...mutation }).fatal, true);

assert.equal(classifySurfaceDetail({ ...bounded, assetType: "character_animation",
  densePlatesOrScales: true }).fatal, true);
assert.equal(classifySurfaceDetail({ ...bounded, profile: "another_profile@1.0.0",
  densePlatesOrScales: true }).fatal, true);
assert.notEqual(classifySurfaceDetail({ ...bounded, lowDensity: false }).findingType,
  "minor_expressive_surface_variance");

for (const text of [guide, rolePrompt, commonPrompt]) {
  for (const token of ["minor_expressive_surface_variance", "regenerationRequired=false",
    "character_evaluation_open_ink_wash_v2_surface_detail_gate_failed"])
    assert.ok(text.includes(token), `missing ${token}`);
  assert.ok(text.includes("animation"));
}
for (const hardBoundary of ["identity", "equipment", "proportion", "silhouette",
  "background", "clipping"]) assert.ok(guide.toLowerCase().includes(hardBoundary));

console.log("generated media open-ink-wash v2 evaluation tolerance: PASS");

function verdict({ totalScore, hardFail }) {
  assert.ok(Number.isFinite(totalScore) && totalScore >= 0 && totalScore <= 100);
  return hardFail || totalScore < 80
    ? { result: "FAIL", passForProjectCopy: false }
    : { result: "PASS", passForProjectCopy: true };
}
assert.deepEqual(verdict({ totalScore: 80, hardFail: false }),
  { result: "PASS", passForProjectCopy: true });
assert.deepEqual(verdict({ totalScore: 79.99, hardFail: false }),
  { result: "FAIL", passForProjectCopy: false });
assert.deepEqual(verdict({ totalScore: 100, hardFail: true }),
  { result: "FAIL", passForProjectCopy: false });

const packageGuide = readFileSync(new URL("../GeneratedMediaEvaluationPackageGuide.md", import.meta.url), "utf8");
const commonGuide = readFileSync(new URL("../../GeneratedImageEvaluationPipelineGuide.md", import.meta.url), "utf8");
for (const text of [guide, packageGuide, commonGuide, rolePrompt, commonPrompt]) {
  assert.ok(text.includes("100"));
  assert.ok(text.includes("80"));
  assert.ok(text.includes("passForProjectCopy=true"));
  assert.ok(text.includes("CONDITIONAL_PASS"));
}
for (const soft of ["brush", "low-density", "wrap", "pigment", "negative-space",
  "minor proportion", "polish", "readability"])
  assert.ok(guide.toLowerCase().includes(soft), `missing soft boundary ${soft}`);
for (const hard of ["wrong person", "child", "weapon", "handedness", "corrupt",
  "severe clipping", "alpha/background", "watermark"])
  assert.ok(guide.toLowerCase().includes(hard), `missing hard boundary ${hard}`);
