// Closed sequential prior-grade identity authority vectors. No provider,
// planning, routing, media, postprocess, evaluation, or promotion is executed.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(here, "..");
const repo = join(guideRoot, "..", "..", "..", "..");
const profilePath = join(guideRoot, "helpers",
  "generated_media_sequential_grade_identity_execution_profile_v1.json");

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") return `{${Object.keys(value)
    .sort().map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
}
const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const assertClosed = (value, keys) => assert.deepEqual(Object.keys(value).sort(),
  [...keys].sort());

const rawProfile = readFileSync(profilePath);
assert.equal(rawProfile[0], 0x7b);
assert.equal(rawProfile.at(-1), 0x0a);
assert.equal(rawProfile.includes(Buffer.from("\r")), false);
const profile = JSON.parse(rawProfile.toString("utf8"));
const profileHash = sha256(Buffer.from(canonicalJson(profile), "utf8"));
assert.equal(profileHash,
  "73a48f8c8013e3a79ac04e0c161075a14ce6b1194527c48585fd33edb009ea04");
assert.equal(profile.executionProfileKey,
  "projectbs_character_open_ink_opaque_chroma_sequential_grade_identity_anchored@1.0.0");
assert.equal(profile.applicability.expressionProfilePayloadHash,
  "b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a");
assert.equal(profile.fixtureBinding.sha256,
  "ff512be1ac75ba0924eab316679dcab4ee171f4a0703014791f2f295e8a6d327");
assert.equal(profile.fixtureBinding.byteLength, 6293540);
assert.ok(profile.rejectedTargetMedia.sha256.includes(
  "1af18044008dc72c749ade2232f61838aad1d037541f8a57cf439031a1714f2e"));

const selectionKeys = ["schemaVersion", "executionProfileKey",
  "executionProfilePayloadSha256", "role", "targetContentId", "targetGrade",
  "authorityContentId", "authorityGrade", "localPath", "pathPolicy", "sha256",
  "byteLength", "trustedEvidencePolicyKey", "evaluationRecordId",
  "evaluationRecordPath", "evaluationRecordSha256", "sourceBoundReceiptId",
  "sourceBoundReceiptPath", "sourceBoundReceiptSha256"];
const selection = {
  schemaVersion: "generated_media_sequential_grade_identity_authority_selection_v1",
  executionProfileKey: profile.executionProfileKey,
  executionProfilePayloadSha256: profileHash,
  role: "identity_equipment_proportion_orientation_authority_only",
  targetContentId: "character.seojin.3",
  targetGrade: 3,
  authorityContentId: "character.seojin.2",
  authorityGrade: 2,
  localPath: profile.fixtureBinding.localPath,
  pathPolicy: profile.fixtureBinding.pathPolicy,
  sha256: profile.fixtureBinding.sha256,
  byteLength: profile.fixtureBinding.byteLength,
  trustedEvidencePolicyKey:
    "generated_media_trusted_evaluated_prior_grade_main_reference@1.0.0",
  evaluationRecordId: "eval.character.character.seojin.2.completed-pass",
  evaluationRecordPath: "evaluation/character.seojin.2/completed-pass.json",
  evaluationRecordSha256: "1".repeat(64),
  sourceBoundReceiptId: "gmsourcebound.character.seojin.2.receipt",
  sourceBoundReceiptPath: "receipts/character.seojin.2/source-bound.json",
  sourceBoundReceiptSha256: "2".repeat(64),
};

function validateSelection(value) {
  assertClosed(value, selectionKeys);
  if (canonicalJson(value) !== canonicalJson(selection))
    throw new Error("sequential_identity_projection_mismatch");
  if (value.targetGrade !== value.authorityGrade + 1)
    throw new Error("sequential_identity_reference_mismatch");
  if (value.role !== "identity_equipment_proportion_orientation_authority_only")
    throw new Error("sequential_identity_reference_mismatch");
  if (value.evaluationRecordSha256.length !== 64
      || value.sourceBoundReceiptSha256.length !== 64)
    throw new Error("sequential_identity_evidence_mismatch");
  return true;
}
assert.equal(validateSelection(selection), true);
assert.throws(() => validateSelection({ ...selection, targetGrade: 4 }),
  /sequential_identity_projection_mismatch|sequential_identity_reference_mismatch/);
assert.throws(() => validateSelection({ ...selection, providerReceiptSha256: "3".repeat(64) }),
  /deep-equal|keys/i);
assert.throws(() => validateSelection({ ...selection,
  sha256: profile.rejectedTargetMedia.sha256[0] }), /sequential_identity_projection_mismatch/);

const projectionNames = ["planning", "routing", "normalizedRequest",
  "authoringHandoff", "visualBrief", "promptHashPayload", "promptRecord",
  "promptIndexEntry", "generationHandoff", "generationPreflight",
  "executionScope", "generationReceipt"];
const projections = Object.fromEntries(projectionNames.map((name) =>
  [name, structuredClone(selection)]));
for (const value of Object.values(projections))
  assert.equal(canonicalJson(value), canonicalJson(selection));
projections.routing.sha256 = "0".repeat(64);
assert.notEqual(canonicalJson(projections.routing), canonicalJson(selection));

function validateEvidence(evaluation, receipt) {
  if (evaluation.evaluationStatus !== "completed" || evaluation.result !== "PASS"
      || evaluation.contentId !== selection.authorityContentId
      || evaluation.mediaSha256 !== selection.sha256)
    throw new Error("sequential_identity_evidence_mismatch");
  if (receipt.contentId !== selection.authorityContentId
      || receipt.outputSha256 !== selection.sha256 || receipt.state !== "completed")
    throw new Error("sequential_identity_evidence_mismatch");
  return true;
}
assert.equal(validateEvidence({ evaluationStatus: "completed", result: "PASS",
  contentId: selection.authorityContentId, mediaSha256: selection.sha256 },
{ state: "completed", contentId: selection.authorityContentId,
  outputSha256: selection.sha256 }), true);
assert.throws(() => validateEvidence({ evaluationStatus: "completed", result: "FAIL",
  contentId: selection.authorityContentId, mediaSha256: selection.sha256 },
{ state: "completed", contentId: selection.authorityContentId,
  outputSha256: selection.sha256 }), /sequential_identity_evidence_mismatch/);

function validateCall(call, observedSha, state = "fresh") {
  assertClosed(call, ["prompt", "referenced_image_paths"]);
  if (!call.prompt || call.referenced_image_paths.length !== 1)
    throw new Error("sequential_identity_reference_count_invalid");
  if (call.referenced_image_paths[0] !== selection.localPath
      || observedSha !== selection.sha256)
    throw new Error("sequential_identity_reference_mismatch");
  if (["active", "completed", "ambiguous"].includes(state))
    throw new Error("duplicate_provider_call_risk");
  return true;
}
assert.equal(validateCall({ prompt: "closed next-grade prompt",
  referenced_image_paths: [selection.localPath] }, selection.sha256), true);
assert.throws(() => validateCall({ prompt: "x", referenced_image_paths: [] },
  selection.sha256), /sequential_identity_reference_count_invalid/);
assert.throws(() => validateCall({ prompt: "x",
  referenced_image_paths: [selection.localPath], num_last_images_to_include: 1 },
selection.sha256), /deep-equal|keys/i);
assert.throws(() => validateCall({ prompt: "x",
  referenced_image_paths: [selection.localPath] }, selection.sha256, "completed"),
/duplicate_provider_call_risk/);

for (const lock of ["face_geometry", "hairline",
  "low_topknot_and_short_controlled_hair", "compact_body_proportions",
  "right_handed_sword_and_scabbard", "recognizable_pouch_and_shoulder_equipment",
  "body_and_equipment_orientation"])
  assert.ok(profile.identityLocks.requiredUnchanged.includes(lock));
assert.deepEqual(profile.identityLocks.allowedDelta, [
  "next_grade_clothing_facts_explicitly_approved_by_target_planning",
  "next_grade_authority_facts_explicitly_approved_by_target_planning"]);
assert.equal(profile.callableContract.submitCountMaximum, 1);
assert.equal(profile.callableContract.retryCountMaximum, 0);

const surfaces = [
  "AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md",
  "AgentDocs/task-prompts/character/ActCharacterPlanningPrompts.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md",
];
for (const relative of surfaces) {
  const text = readFileSync(join(repo, relative), "utf8").replaceAll("\r\n", "\n");
  assert.match(text, /projectbs_character_open_ink_opaque_chroma_sequential_grade_identity_anchored@1\.0\.0/);
  assert.match(text, /generated_media_sequential_grade_identity_authority_selection_v1/);
  assert.match(text, /73a48f8c8013e3a79ac04e0c161075a14ce6b1194527c48585fd33edb009ea04/);
}

console.log({ executionProfileKey: profile.executionProfileKey,
  executionProfilePayloadSha256: profileHash,
  selectionSchemaVersion: selection.schemaVersion,
  fixtureSha256: selection.sha256,
  providerCalled: false, submitCount: 0, cost: 0 });
console.log("generated media sequential grade identity authority vectors: PASS");
