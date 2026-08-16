// Additive output-conformance vectors for open ink-wash v2.
// No provider call, image read, evaluation execution, or record write occurs.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const gm = join(testDir, "..");
const paths = {
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
const text = Object.fromEntries(Object.entries(paths).map(([name, path]) =>
  [name, readFileSync(path, "utf8").replaceAll("\r\n", "\n")]));

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}
function hashObject(value) {
  return createHash("sha256").update(Buffer.from(canonicalJson(value), "utf8")).digest("hex");
}
function profileAfterHeading(source, heading) {
  const start = source.indexOf(heading);
  assert.notEqual(start, -1, `missing heading ${heading}`);
  const fence = "```";
  const jsonStart = source.indexOf(`${fence}json\n`, start);
  assert.notEqual(jsonStart, -1);
  const bodyStart = jsonStart + 8;
  const bodyEnd = source.indexOf(`\n${fence}`, bodyStart);
  assert.notEqual(bodyEnd, -1);
  return JSON.parse(source.slice(bodyStart, bodyEnd));
}

const keyV1 = "projectbs_character_open_ink_wash_dynamic_contour@1.0.0";
const hashV1 = "37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd";
const keyV2 = "projectbs_character_open_ink_wash_dynamic_contour@2.0.0";
const hashV2 = "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5";
const profileV1 = profileAfterHeading(text.visual,
  "### Open ink-wash dynamic-contour character profile");
const profileV2 = profileAfterHeading(text.visual,
  "### Open ink-wash output-conformance successor profile");

assert.equal(profileV1.expressionProfileKey, keyV1);
assert.equal(hashObject(profileV1), hashV1, "published v1 bytes/meaning drifted");
assert.equal(profileV2.expressionProfileKey, keyV2);
assert.equal(hashObject(profileV2), hashV2);
assert.equal(profileV2.predecessorBinding.expressionProfileKey, keyV1);
assert.equal(profileV2.predecessorBinding.expressionProfilePayloadHash, hashV1);
assert.equal(Object.keys(profileV2).length, 19);
assert.equal(profileV2.negativeStyleLock.length, 9);
assert.equal(profileV2.positiveStyleLock.length, 9);
assert.deepEqual(profileV2.proportionMeasurementContract.observableAcceptance,
  { minimum: 4, maximum: 5 });
assert.equal(profileV2.surfaceDetailContract.individualArmorPlateEnumeration, "prohibited");
assert.equal(profileV2.surfaceDetailContract.rivetsLacingAndFastenerEnumeration, "prohibited");
assert.equal(profileV2.backgroundContract.generationBackground.color, "#F2EFE6");
assert.equal(profileV2.backgroundContract.radialGradient, "prohibited");
assert.deepEqual(profileV2.providerOutputConformanceContract.gateOrder,
  ["proportion_age", "contour_mok_seon", "surface_detail",
    "pigment_palette_negative_space", "background", "identity_equipment", "reference_role"]);

const policyMembers = Object.keys(profileV2).filter((key) =>
  !["expressionProfileKey", "negativeStyleLock", "positiveStyleLock"].includes(key));
function validateAuthoring(payload, evidenceMembers, promptMembers) {
  if (hashObject(payload) !== hashV2) throw new Error("open_ink_wash_v2_profile_projection_mismatch");
  if (policyMembers.some((member) => !evidenceMembers.includes(member)))
    throw new Error("open_ink_wash_v2_profile_evidence_incomplete");
  if (policyMembers.some((member) => !promptMembers.includes(member)))
    throw new Error("provider_prompt_open_ink_wash_v2_projection_missing");
  return true;
}
assert.equal(validateAuthoring(profileV2, policyMembers, policyMembers), true);
assert.throws(() => validateAuthoring(profileV2, policyMembers.slice(1), policyMembers),
  /open_ink_wash_v2_profile_evidence_incomplete/);

const gateOrder = profileV2.providerOutputConformanceContract.gateOrder;
const failureTokens = {
  proportion_age: "character_preview_open_ink_wash_v2_proportion_age_nonconformant",
  contour_mok_seon: "character_preview_open_ink_wash_v2_contour_mok_seon_nonconformant",
  surface_detail: "character_preview_open_ink_wash_v2_surface_detail_nonconformant",
  pigment_palette_negative_space: "character_preview_open_ink_wash_v2_pigment_negative_space_nonconformant",
  background: "character_preview_open_ink_wash_v2_background_nonconformant",
  identity_equipment: "character_preview_open_ink_wash_v2_identity_equipment_nonconformant",
  reference_role: "character_preview_open_ink_wash_v2_reference_role_nonconformant",
};
function classifyOutput(results) {
  const firstFail = gateOrder.find((gateId) => results[gateId] === "fail");
  const firstInsufficient = gateOrder.find((gateId) => results[gateId] === "evidence_insufficient");
  if (firstFail) return {
    profileConformanceStatus: "preview_profile_nonconformant",
    failureType: failureTokens[firstFail], nextStep: "stop_no_retry_not_final",
  };
  if (firstInsufficient) return {
    profileConformanceStatus: "preview_profile_conformance_blocked",
    failureType: "character_preview_open_ink_wash_v2_evidence_insufficient",
    nextStep: "stop_no_retry_not_final",
  };
  return { profileConformanceStatus: "preview_conformant_no_downstream",
    failureType: "none", nextStep: "no_downstream" };
}
const pass = Object.fromEntries(gateOrder.map((gate) => [gate, "pass"]));
assert.deepEqual(classifyOutput(pass), {
  profileConformanceStatus: "preview_conformant_no_downstream",
  failureType: "none", nextStep: "no_downstream",
});
for (const [gateId, failureType] of Object.entries(failureTokens)) {
  const result = classifyOutput({ ...pass, [gateId]: "fail" });
  assert.equal(result.failureType, failureType);
  assert.equal(result.nextStep, "stop_no_retry_not_final");
}
const observedV9Like = classifyOutput({ ...pass, proportion_age: "fail",
  surface_detail: "fail", background: "fail" });
assert.deepEqual(observedV9Like, {
  profileConformanceStatus: "preview_profile_nonconformant",
  failureType: "character_preview_open_ink_wash_v2_proportion_age_nonconformant",
  nextStep: "stop_no_retry_not_final",
});

const receiptPayload = {
  schemaVersion: "generated_media_profile_conformance_receipt_v1",
  requestId: "gmplan2.character_single_image.character.seojin.1.fixture",
  planningSnapshotHash: "1".repeat(64), promptRecordId: "gmprompt3.fixture",
  promptRecordSha256: "2".repeat(64), expressionProfileKey: keyV2,
  expressionProfilePayloadHash: hashV2, observableOutputSha256: "3".repeat(64),
  ...observedV9Like,
  gateResults: gateOrder.map((gateId) => ({ gateId,
    result: ["proportion_age", "surface_detail", "background"].includes(gateId) ? "fail" : "pass" })),
  providerCalled: true, submitCount: 1, retryCount: 0,
};
const receipt = { ...receiptPayload, receiptPayloadSha256: hashObject(receiptPayload) };
assert.equal(receipt.submitCount, 1);
assert.equal(receipt.retryCount, 0);
assert.equal(receipt.gateResults.length, 7);
for (const forbidden of ["expressionProfilePayload", "scenePromptOriginal",
  "sourcePlanningFiles", "routingRecordId", "registryRowId", "indexPath", "casKey"])
  assert.equal(Object.hasOwn(receipt, forbidden), false);

for (const name of Object.keys(paths)) assert.ok(text[name].includes(keyV2), `${name} missing v2 key`);
for (const name of ["visual", "registry", "planning", "record"])
  assert.ok(text[name].includes(hashV2), `${name} missing v2 hash`);
for (const token of ["open_ink_wash_v2_profile_projection_mismatch",
  "character_generation_open_ink_wash_v2_surface_detail_gate_failed",
  "character_preview_open_ink_wash_v2_background_nonconformant",
  "character_preview_open_ink_wash_v2_evidence_insufficient",
  "character_evaluation_open_ink_wash_v2_surface_detail_gate_failed"])
  assert.ok(text.contract.includes(token), `contract missing ${token}`);
assert.ok(text.record.includes("creates no file, index row, or CAS projection"));

console.log({ expressionProfileKey: keyV2, expressionProfilePayloadHash: hashV2,
  predecessorPayloadHash: hashV1, providerCalled: false, submitCount: 0, cost: 0 });
console.log("generated media open ink-wash profile v2 output-conformance vectors: PASS");
