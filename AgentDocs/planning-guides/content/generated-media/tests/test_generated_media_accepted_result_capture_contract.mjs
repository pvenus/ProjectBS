// Closed vectors for generated_media_accepted_result_capture_v1.
// This test performs no provider call and writes no workflow artifact.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const promptRoot = join(guideRoot, "..", "..", "..", "task-prompts",
  "content", "generated-media");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");
const contract = read(join(guideRoot, "GeneratedMediaImageGenOnlyContractGuide.md"));
const recordGuide = read(join(guideRoot, "GeneratedMediaRecordGuide.md"));
const preservation = read(join(guideRoot, "GeneratedMediaPreservationPackagingGuide.md"));
const capturePrompt = read(join(promptRoot, "GeneratedMediaAcceptedResultCapturePrompt.md"));
const preservationPrompt = read(join(promptRoot,
  "GeneratedMediaPreservationPackagingPrompt.md"));

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}
const hashObject = (value) => createHash("sha256")
  .update(Buffer.from(canonicalJson(value), "utf8")).digest("hex");

const recordKeys = ["schemaVersion", "captureRecordId",
  "capturePayloadSha256", "requestId", "assetType", "domainType",
  "contentId", "animationRequestId", "planningSnapshotHash",
  "routingRecordId", "routingRecordSha256", "sourceExecutionEvidence",
  "userAcceptance", "promptEvidence", "settingsEvidence",
  "referenceEvidence", "resultEvidence", "capabilityEvidenceStatus",
  "costEvidenceStatus", "preSubmitGateAttestation", "captureAction",
  "downstreamAuthorization", "createdAt", "validation"];

function payloadOf(record) {
  const payload = structuredClone(record);
  delete payload.captureRecordId;
  delete payload.capturePayloadSha256;
  delete payload.validation;
  return payload;
}

function validateRecord(record) {
  assert.deepEqual(Object.keys(record).sort(), [...recordKeys].sort());
  assert.equal(record.schemaVersion,
    "generated_media_accepted_result_capture_v1");
  if (record.sourceExecutionEvidence.historicalSubmitCount !== 1
      || record.sourceExecutionEvidence.historicalRetryCount !== 0
      || !record.sourceExecutionEvidence.taskId
      || !record.sourceExecutionEvidence.toolCallId)
    throw new Error("accepted_capture_execution_evidence_missing");
  if (record.userAcceptance.authorityType !== "authenticated_user_acceptance"
      || record.userAcceptance.acceptedArtifactSha256
        !== record.resultEvidence.completedGif.sha256
      || record.createdAt !== record.userAcceptance.acceptedAt)
    throw new Error("accepted_capture_acceptance_missing");
  if (record.capabilityEvidenceStatus !== "unavailable_observed"
      || record.costEvidenceStatus !== "unavailable_observed"
      || record.preSubmitGateAttestation
        !== "not_claimed_post_result_capture")
    throw new Error("accepted_capture_false_attestation");
  assert.deepEqual(record.captureAction,
    { providerCalled: false, submitCount: 0, retryCount: 0 });
  assert.deepEqual(record.downstreamAuthorization, {
    preservationAuthorized: true, evaluationAuthorized: true,
    promotionAuthorized: false,
    promotionPrerequisites:
      "strict_evaluation_pass_and_explicit_project_mapping",
  });
  const { frames, completedGif, providerMaster } = record.resultEvidence;
  if (!["image", "animated_gif"].includes(providerMaster.mediaType)
      || frames.length === 0
      || frames.length !== completedGif.frameCount
      || frames.some((frame, index) => frame.frameIndex !== index)
      || record.referenceEvidence.length === 0)
    throw new Error("accepted_capture_incomplete_member_set");
  const payloadHash = hashObject(payloadOf(record));
  assert.equal(record.capturePayloadSha256, payloadHash);
  assert.equal(record.captureRecordId,
    `gmaccept1.animation.${record.contentId}.${record.animationRequestId}.${payloadHash.slice(0, 20)}`);
  return true;
}

function makeRecord() {
  const record = {
    schemaVersion: "generated_media_accepted_result_capture_v1",
    captureRecordId: "pending", capturePayloadSha256: "pending",
    requestId: "gmplan2.animation.character.seojin.1.05fc416f22862fc0ec5f",
    assetType: "animation", domainType: "character",
    contentId: "character.seojin.1",
    animationRequestId: "character.seojin.1.attack.draw_slash.one_shot.v2",
    planningSnapshotHash:
      "05fc416f22862fc0ec5fd5bf8be2072971f8ab94d769205d3bf2c67a0a9d2c14",
    routingRecordId:
      "gmroute2.animation.character.seojin.1.character.seojin.1.attack.draw_slash.one_shot.v2.f8c243408349bfe2d7b4",
    routingRecordSha256:
      "aa94d1ff18f1444b0110b6366c3dee7ac2bd38e416ccf0565da4ed042fa72bda",
    sourceExecutionEvidence: {
      taskId: "01a00f8f-0a51-7f73-b2d0-449f2d72b3cc",
      toolCallId: "exact-observed-tool-call-id", provider: "imagegen",
      providerTool: "built-in_imagegen", historicalSubmitCount: 1,
      historicalRetryCount: 0,
    },
    userAcceptance: {
      authorityType: "authenticated_user_acceptance",
      acceptanceMessageId: "exact-authenticated-message-id",
      acceptedAt: "2026-08-17T12:00:00+09:00",
      acceptedArtifactSha256:
        "8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621",
    },
    promptEvidence: {
      path: "C:/Users/parkv/Documents/Codex/2026-08-17/projectbs-seojin-accepted-evidence-capture/outputs/authoring-handoff-recovered-provider-prompt-verbatim.txt",
      fileSha256:
        "ad91da8019c4ed46134500bea15654f04d8f341f4056b1186f5825e9bac3ffb8",
      providerPromptPayloadHash:
        "278b55136cfba5ea0977f7db37c893452aca6b494592c311c3569f02ee0eb658",
    },
    settingsEvidence: {
      path: "C:/Users/parkv/Documents/Codex/2026-08-17/projectbs-seojin-accepted-evidence-capture/outputs/recovered-provider-settings.json",
      fileSha256:
        "5b556f319018b9da3a64bd31e233ccdc91c92b7e7435ea000e1c650e8bcce2ba",
    },
    referenceEvidence: [{ role: "visual_reference_only_not_identity_or_edit_target",
      path: "C:/exact/observed/submitted-reference.png",
      sha256: "0fbc5702a04683e2fe483ba230d10f92d31ea88984330c7314f14590313815b0" }],
    resultEvidence: {
      providerMaster: {
        path: "C:/Users/parkv/.codex/generated_images/01a00f8f-0a51-7f73-b2d0-449f2d72b3cc/exec-ba4d8e58-5060-4124-bb8a-3fa8de2e4634.png",
        sha256: "e1e5cba53ed5d8b34f111d57c5c3652638859565dd2280ee644c14a015c5ec00",
        mediaType: "image",
      },
      completedGif: { path: "C:/exact/accepted.gif",
        sha256: "8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621",
        width: 640, height: 512, frameCount: 6 },
      frames: Array.from({ length: 6 }, (_, frameIndex) => ({ frameIndex,
        path: `C:/exact/frame-${frameIndex}.png`,
        sha256: String(frameIndex + 1).repeat(64) })),
    },
    capabilityEvidenceStatus: "unavailable_observed",
    costEvidenceStatus: "unavailable_observed",
    preSubmitGateAttestation: "not_claimed_post_result_capture",
    captureAction: { providerCalled: false, submitCount: 0, retryCount: 0 },
    downstreamAuthorization: {
      preservationAuthorized: true, evaluationAuthorized: true,
      promotionAuthorized: false,
      promotionPrerequisites:
        "strict_evaluation_pass_and_explicit_project_mapping",
    },
    createdAt: "2026-08-17T12:00:00+09:00",
    validation: { status: "valid", acceptance: "valid",
      executionEvidence: "valid", identities: "valid", rawHashes: "valid",
      memberClosure: "valid" },
  };
  record.capturePayloadSha256 = hashObject(payloadOf(record));
  record.captureRecordId = `gmaccept1.animation.${record.contentId}.${record.animationRequestId}.${record.capturePayloadSha256.slice(0, 20)}`;
  return record;
}

for (const surface of [contract, recordGuide, preservation, capturePrompt,
  preservationPrompt]) {
  assert.match(surface, /generated_media_accepted_result_capture_v1/);
  assert.match(surface, /unavailable_observed/);
  assert.match(surface, /not_claimed_post_result_capture/);
}
for (const surface of [contract, recordGuide, preservation, capturePrompt]) {
  assert.match(surface, /strict evaluation[^\n]*PASS/i);
  assert.match(surface, /explicit\s+project mapping/i);
}

// The additive bridge does not redefine strict or provider-native modes.
assert.match(contract, /generated_media_generation_v2/);
assert.match(contract, /providerCalled=false/);
assert.match(preservation, /provider_native_animated_gif/);
assert.match(preservation, /provider returned one coherent six-cell master\s+IMAGE, not a GIF/i);

const valid = makeRecord();
assert.equal(validateRecord(valid), true);
assert.equal(validateRecord(structuredClone(valid)), true); // idempotent projection
assert.throws(() => validateRecord({ ...valid,
  capabilityEvidenceStatus: "supported" }), /accepted_capture_false_attestation/);
assert.throws(() => validateRecord({ ...valid,
  preSubmitGateAttestation: "passed" }), /accepted_capture_false_attestation/);
assert.throws(() => validateRecord({ ...valid, sourceExecutionEvidence: {
  ...valid.sourceExecutionEvidence, historicalSubmitCount: 2 } }),
  /accepted_capture_execution_evidence_missing/);
assert.throws(() => validateRecord({ ...valid, userAcceptance: {
  ...valid.userAcceptance, acceptedArtifactSha256: "f".repeat(64) } }),
  /accepted_capture_acceptance_missing/);
assert.throws(() => validateRecord({ ...valid, resultEvidence: {
  ...valid.resultEvidence, frames: valid.resultEvidence.frames.slice(0, 5) } }),
  /accepted_capture_incomplete_member_set/);
assert.throws(() => validateRecord({ ...valid, downstreamAuthorization: {
  ...valid.downstreamAuthorization, promotionAuthorized: true } }));

console.log({ schemaVersion: valid.schemaVersion,
  captureRecordId: valid.captureRecordId, testsProviderCalled: false,
  testsSubmitCount: 0 });
console.log("generated media accepted result capture contract vectors: PASS");
