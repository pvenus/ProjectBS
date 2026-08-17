// Current character package-mode evaluation adapter vectors.
// No media inspection, evaluation write, promotion, provider, or Unity work occurs.

import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const generatedMediaGuideRoot = join(testDir, "..");
const contentGuideRoot = join(generatedMediaGuideRoot, "..");
const repoDocsRoot = join(contentGuideRoot, "..", "..");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");

const pipelineGuide = read(join(contentGuideRoot,
  "GeneratedImageEvaluationPipelineGuide.md"));
const packageGuide = read(join(generatedMediaGuideRoot,
  "GeneratedMediaEvaluationPackageGuide.md"));
const animationGuide = read(join(repoDocsRoot, "planning-guides", "character",
  "EvaluationAnimationGuide.md"));
const evaluationPrompt = read(join(repoDocsRoot, "task-prompts", "content",
  "GeneratedImageEvaluationPrompt.md"));

const currentRegistry = new Map([
  ["character_single_image.character", {
    structureProfile: "character_single_image_v2",
    adapterId: "current_character_single_image_adapter",
  }],
  ["animation.character", {
    structureProfile: "animation_gif_frame_set_v2",
    adapterId: "character_animation_gif_frame_set_v2",
  }],
]);

function resolveCurrentAdapter(input) {
  if (input.artifactType !== undefined)
    throw new Error("legacy_current_identity_conflict");
  const row = currentRegistry.get(`${input.assetType}.${input.domainType}`);
  if (!row) throw new Error("missing_domain_evaluation_adapter");
  if (input.structureProfile !== row.structureProfile)
    throw new Error("artifact_identity_mismatch");
  if (input.packageSchema !== "generated_media_evaluation_package_v2"
      || input.packageSealed !== true || !input.evaluationPackageId)
    throw new Error("evaluation_package_not_sealed");
  return row;
}

const attackPackage = {
  packageSchema: "generated_media_evaluation_package_v2",
  packageSealed: true,
  evaluationPackageId:
    "evalpkg2.animation.character.seojin.1.character.seojin.1.attack.draw_slash.one_shot.v2.7f01498051e70e85",
  assetType: "animation",
  domainType: "character",
  structureProfile: "animation_gif_frame_set_v2",
  provenanceBranch: "accepted_result_capture",
};
assert.deepEqual(resolveCurrentAdapter(attackPackage), {
  structureProfile: "animation_gif_frame_set_v2",
  adapterId: "character_animation_gif_frame_set_v2",
});
assert.deepEqual(resolveCurrentAdapter({ ...attackPackage,
  evaluationPackageId: "evalpkg2.character_single_image.character.seojin.1.example",
  assetType: "character_single_image",
  structureProfile: "character_single_image_v2",
  provenanceBranch: "strict_generation" }), {
  structureProfile: "character_single_image_v2",
  adapterId: "current_character_single_image_adapter",
});
assert.throws(() => resolveCurrentAdapter({ ...attackPackage,
  artifactType: "character_animation" }), /legacy_current_identity_conflict/);
assert.throws(() => resolveCurrentAdapter({ ...attackPackage,
  structureProfile: "ordered_frame_set" }), /artifact_identity_mismatch/);
assert.throws(() => resolveCurrentAdapter({ ...attackPackage,
  packageSealed: false }), /evaluation_package_not_sealed/);

const categoryMaximums = [30, 25, 20, 15, 10];
assert.equal(categoryMaximums.reduce((sum, value) => sum + value, 0), 100);
function verdict({ scores, hardFail = false, completeEvidence = true }) {
  if (!completeEvidence) return "not_evaluated";
  const total = scores.reduce((sum, value) => sum + value, 0);
  if (hardFail || total < 80) return "FAIL";
  if (total < 90) return "CONDITIONAL_PASS";
  return "PASS";
}
assert.equal(verdict({ scores: [27, 23, 18, 13, 9] }), "PASS");
assert.equal(verdict({ scores: [27, 22, 18, 13, 9] }), "CONDITIONAL_PASS");
assert.equal(verdict({ scores: [30, 25, 20, 15, 10], hardFail: true }), "FAIL");
assert.equal(verdict({ scores: [], completeEvidence: false }), "not_evaluated");

assert.match(pipelineGuide,
  /animation \+ domainType=character \| character \| animation_gif_frame_set_v2[\s\S]*ready/);
assert.match(pipelineGuide,
  /character_single_image \+ domainType=character \| character \| character_single_image_v2[\s\S]*ready/);
assert.match(pipelineGuide, /### 8\.5\.1 animation_gif_frame_set_v2/);
assert.match(packageGuide, /### animation_gif_frame_set_v2/);
assert.match(packageGuide, /coherent master[\s\S]*completed GIF[\s\S]*contiguous PNG/i);

for (const field of ["adapterId", "assetType", "domainType", "evaluationDomain",
  "structureProfile", "canonicalContentSourceRule", "artifactUsageRule",
  "planningEvidenceRule", "stagingSourceRule", "projectTargetRule",
  "requiredEvidence", "domainFatalGates", "scoreCategories", "passThreshold",
  "categoryMinimums", "domainNativeResults", "resultNormalization",
  "domainSpecificNotes fields", "mediaEvidenceRule", "reEvaluationRule"]) {
  assert.match(animationGuide, new RegExp(field));
}
for (const criterionId of ["anim.frame_continuity_body_integrity",
  "anim.identity_equipment_weapon", "anim.direction_spatial_stability",
  "anim.action_readability", "anim.timing_loop_ending"])
  assert.match(animationGuide, new RegExp(criterionId.replaceAll(".", "\\.")));
for (const surface of [pipelineGuide, animationGuide, evaluationPrompt]) {
  assert.match(surface, /animation_gif_frame_set_v2/);
  assert.match(surface, /accepted-result|accepted_result|accepted-result branch/);
  assert.match(surface, /fake prompt|fake[\s\S]*generation|fake[\s\S]*download/i);
  assert.match(surface, /legacy[\s\S]*character_animation/i);
}
assert.match(evaluationPrompt,
  /animation\+domainType=character\+animation_gif_frame_set_v2/);
assert.match(evaluationPrompt,
  /character_single_image\+character\+character_single_image_v2 route는 그대로 ready/);

console.log({ evaluationPackageId: attackPackage.evaluationPackageId,
  adapterId: "character_animation_gif_frame_set_v2",
  preservedCurrentIdentity: true,
  legacyAliasUsed: false,
  providerCalled: false,
  evaluationExecuted: false,
  projectCopyCalled: false });
console.log("generated media current character evaluation adapter vectors: PASS");
