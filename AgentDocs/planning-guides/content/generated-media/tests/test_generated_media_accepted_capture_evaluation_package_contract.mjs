// Closed branch vectors for accepted-result capture -> preservation/evaluation package.
// No record, media, provider, evaluation, or promotion action occurs.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const repo = join(guideRoot, "..", "..", "..", "..");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");
const preservation = read(join(guideRoot, "GeneratedMediaPreservationPackagingGuide.md"));
const evaluationPackage = read(join(guideRoot, "GeneratedMediaEvaluationPackageGuide.md"));
const preservationPrompt = read(join(repo, "AgentDocs", "task-prompts", "content",
  "generated-media", "GeneratedMediaPreservationPackagingPrompt.md"));
const evaluationPrompt = read(join(repo, "AgentDocs", "task-prompts", "content",
  "GeneratedImageEvaluationPrompt.md"));
const expressionPrompt = read(join(repo, "AgentDocs", "task-prompts", "content",
  "generated-media", "GeneratedMediaCharacterExpressionEvaluationPrompt.md"));

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") return `{${Object.keys(value)
    .sort().map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
}
const hashObject = (value) => createHash("sha256")
  .update(Buffer.from(canonicalJson(value), "utf8")).digest("hex");

const commonKeys = ["requestId", "assetType", "domainType", "contentId",
  "planningSnapshotHash", "routingRecordId",
  "preservationRecordId", "preservationPayloadHash", "provider",
  "structureProfile", "profileExtension", "members", "projectTarget"];
const strictKeys = ["promptRecordId", "promptRecordSha256",
  "generationRecordId", "generationRecordSha256"];
const acceptedKeys = ["acceptedPromptEvidence", "acceptedResultCaptureRecordId",
  "acceptedResultCaptureRecordPath", "acceptedResultCaptureRecordSha256",
  "acceptedResultCaptureReceiptSha256"];

function validatePreservationInput(input) {
  const strict = ["promptRecordId", "generationRecordId", "generationRecordSha256",
    "providerResultRefs", "approvalCostProjection"];
  const acceptedBranch = ["acceptedPromptEvidence", "acceptedResultCaptureRecordId",
    "acceptedResultCaptureRecordPath", "acceptedResultCaptureRecordSha256",
    "acceptedResultCaptureReceipt", "acceptedResultCaptureReceiptSha256"];
  const hasStrict = strict.some((key) => Object.hasOwn(input, key));
  const hasAccepted = acceptedBranch.some((key) => Object.hasOwn(input, key));
  if (hasStrict && hasAccepted) throw new Error("preservation_input_branch_conflict");
  const branch = hasAccepted ? acceptedBranch : strict;
  if (branch.some((key) => !Object.hasOwn(input, key)))
    throw new Error("preservation_input_branch_incomplete");
  const common = new Set(["requestId", "routingRecordId", ...branch]);
  if (Object.keys(input).some((key) => !common.has(key)))
    throw new Error("preservation_input_unknown_field");
  return true;
}

function validateManifest(payload) {
  const keys = Object.keys(payload);
  const hasStrict = strictKeys.some((key) => Object.hasOwn(payload, key));
  const hasAccepted = acceptedKeys.some((key) => Object.hasOwn(payload, key));
  if (hasStrict && hasAccepted) throw new Error("evaluation_package_input_branch_conflict");
  const branchKeys = hasAccepted ? acceptedKeys : strictKeys;
  if (branchKeys.some((key) => !Object.hasOwn(payload, key)))
    throw new Error("evaluation_package_input_branch_incomplete");
  const allowed = new Set([...commonKeys, ...branchKeys,
    ...(payload.assetType === "animation" ? ["animationRequestId"] : [])]);
  if (keys.some((key) => !allowed.has(key)))
    throw new Error("evaluation_package_unknown_branch_field");
  if (hasAccepted) {
    const roles = payload.members.map((member) => member.role);
    if (payload.assetType === "animation") {
      assert.deepEqual(payload.acceptedPromptEvidence, {
        source: "accepted_result_capture",
        providerPromptPayloadHash:
          "278b55136cfba5ea0977f7db37c893452aca6b494592c311c3569f02ee0eb658",
        promptFileSha256:
          "ad91da8019c4ed46134500bea15654f04d8f341f4056b1186f5825e9bac3ffb8",
      });
      assert.equal(roles.filter((x) => x === "accepted_provider_prompt").length, 1);
    } else {
      assert.deepEqual(payload.acceptedPromptEvidence, {
        source: "accepted_result_capture", status: "unavailable_observed",
        claim: "not_claimed",
      });
      assert.equal(roles.includes("accepted_provider_prompt"), false);
      assert.equal(roles.filter((x) => x === "accepted_project_candidate_png").length, 1);
    }
    for (const role of ["accepted_capture_record", "accepted_capture_receipt"])
      assert.equal(roles.filter((x) => x === role).length, 1);
    assert.equal(roles.includes("generation_record"), false);
  }
  return hashObject(payload);
}

const common = {
  requestId: "gmplan2.animation.character.seojin.1.05fc416f22862fc0ec5f",
  assetType: "animation", domainType: "character", contentId: "character.seojin.1",
  animationRequestId: "character.seojin.1.attack.draw_slash.one_shot.v2",
  planningSnapshotHash:
    "05fc416f22862fc0ec5fd5bf8be2072971f8ab94d769205d3bf2c67a0a9d2c14",
  routingRecordId:
    "gmroute2.animation.character.seojin.1.character.seojin.1.attack.draw_slash.one_shot.v2.f8c243408349bfe2d7b4",
  preservationRecordId: "gmpreserve2.animation.contract-vector",
  preservationPayloadHash: "1".repeat(64), provider: "imagegen",
  structureProfile: "animation_gif_frame_set_v2", profileExtension: {},
  projectTarget: { status: "informational_only" },
};
const member = (memberId, role, sha256) => ({ memberId, role,
  relativePath: `accepted-capture/${memberId}.json`, sha256,
  mediaType: "application/json", width: 0, height: 0, order: 0,
  profileData: {} });

const accepted = {
  ...common,
  acceptedPromptEvidence: {
    source: "accepted_result_capture",
    providerPromptPayloadHash:
      "278b55136cfba5ea0977f7db37c893452aca6b494592c311c3569f02ee0eb658",
    promptFileSha256:
      "ad91da8019c4ed46134500bea15654f04d8f341f4056b1186f5825e9bac3ffb8",
  },
  acceptedResultCaptureRecordId:
    "gmaccept1.animation.character.seojin.1.character.seojin.1.attack.draw_slash.one_shot.v2.7437c844ae7c5fa9c7d9",
  acceptedResultCaptureRecordPath:
    "AgentDocs/planning-data/generated-media-accepted-result-capture/v1/animation/character.seojin.1/character.seojin.1.attack.draw_slash.one_shot.v2/gmaccept1.animation.character.seojin.1.character.seojin.1.attack.draw_slash.one_shot.v2.7437c844ae7c5fa9c7d9.json",
  acceptedResultCaptureRecordSha256:
    "f36f6f3c9594f83999dc6af9c760a95275a295be86a194b0a06674b6d5e69faf",
  acceptedResultCaptureReceiptSha256: "2".repeat(64),
  members: [
    member("provider-prompt", "accepted_provider_prompt",
      "ad91da8019c4ed46134500bea15654f04d8f341f4056b1186f5825e9bac3ffb8"),
    member("capture-record", "accepted_capture_record",
      "f36f6f3c9594f83999dc6af9c760a95275a295be86a194b0a06674b6d5e69faf"),
    member("capture-receipt", "accepted_capture_receipt", "2".repeat(64)),
    { ...member("accepted-gif", "completed_gif",
      "8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621"),
      relativePath: "source/accepted.gif", mediaType: "image/gif", width: 640,
      height: 512, order: 1 },
  ],
};

const strict = { ...common,
  promptRecordId: "gmprompt3.strict", promptRecordSha256: "3".repeat(64),
  generationRecordId: "gmgen2.strict", generationRecordSha256: "4".repeat(64),
  members: [member("generation-record", "generation_record", "4".repeat(64))] };

const acceptedStill = {
  requestId:
    "gmplan2.character_single_image.character.seojin.1.e5537d6487d06b88f452",
  assetType: "character_single_image", domainType: "character",
  contentId: "character.seojin.1",
  planningSnapshotHash: "e5537d6487d06b88f452".padEnd(64, "0"),
  routingRecordId: "gmroute2.character_single_image.character.seojin.1.vector",
  preservationRecordId: "gmpreserve2.character_single_image.contract-vector",
  preservationPayloadHash: "5".repeat(64), provider: "imagegen",
  structureProfile: "character_single_image_v2", profileExtension: {},
  projectTarget: { status: "informational_only" },
  acceptedPromptEvidence: { source: "accepted_result_capture",
    status: "unavailable_observed", claim: "not_claimed" },
  acceptedResultCaptureRecordId:
    "gmaccept1.character_single_image.character.seojin.1.vector",
  acceptedResultCaptureRecordPath:
    "AgentDocs/planning-data/generated-media-accepted-result-capture/v1/character_single_image/character.seojin.1/gmaccept1.character_single_image.character.seojin.1.vector.json",
  acceptedResultCaptureRecordSha256: "6".repeat(64),
  acceptedResultCaptureReceiptSha256: "7".repeat(64),
  members: [
    member("capture-record", "accepted_capture_record", "6".repeat(64)),
    member("capture-receipt", "accepted_capture_receipt", "7".repeat(64)),
    { ...member("accepted-image", "accepted_project_candidate_png",
      "0fbc5702a04683e2fe483ba230d10f92d31ea88984330c7314f14590313815b0"),
      relativePath: "source/accepted.png", mediaType: "image/png", width: 1,
      height: 1, order: 1 },
  ],
};

const acceptedPreservationInput = {
  requestId: accepted.requestId, routingRecordId: accepted.routingRecordId,
  acceptedPromptEvidence: accepted.acceptedPromptEvidence,
  acceptedResultCaptureRecordId: accepted.acceptedResultCaptureRecordId,
  acceptedResultCaptureRecordPath: accepted.acceptedResultCaptureRecordPath,
  acceptedResultCaptureRecordSha256: accepted.acceptedResultCaptureRecordSha256,
  acceptedResultCaptureReceipt: { schemaVersion:
    "generated_media_accepted_result_capture_receipt_v1", state: "captured" },
  acceptedResultCaptureReceiptSha256: accepted.acceptedResultCaptureReceiptSha256,
};
const strictPreservationInput = {
  requestId: strict.requestId, routingRecordId: strict.routingRecordId,
  promptRecordId: strict.promptRecordId, generationRecordId: strict.generationRecordId,
  generationRecordSha256: strict.generationRecordSha256,
  providerResultRefs: ["provider-result"], approvalCostProjection: {},
};

assert.match(validateManifest(accepted), /^[0-9a-f]{64}$/);
assert.match(validateManifest(acceptedStill), /^[0-9a-f]{64}$/);
assert.match(validateManifest(strict), /^[0-9a-f]{64}$/);
assert.equal(validatePreservationInput(acceptedPreservationInput), true);
assert.equal(validatePreservationInput(strictPreservationInput), true);
assert.throws(() => validatePreservationInput({ ...acceptedPreservationInput,
  generationRecordId: "gmgen2.mixed" }), /preservation_input_branch_conflict/);
const partialPreservation = structuredClone(acceptedPreservationInput);
delete partialPreservation.acceptedResultCaptureRecordPath;
assert.throws(() => validatePreservationInput(partialPreservation),
  /preservation_input_branch_incomplete/);
assert.throws(() => validatePreservationInput({ ...acceptedPreservationInput,
  inventedPromptRecordId: "fake" }), /preservation_input_unknown_field/);
assert.throws(() => validateManifest({ ...accepted,
  generationRecordId: "gmgen2.mixed" }), /evaluation_package_input_branch_conflict/);
const partial = structuredClone(accepted);
delete partial.acceptedResultCaptureReceiptSha256;
assert.throws(() => validateManifest(partial), /evaluation_package_input_branch_incomplete/);
assert.throws(() => validateManifest({ ...accepted, inventedPromptRecordId: "fake" }),
  /evaluation_package_unknown_branch_field/);
assert.throws(() => validateManifest({ ...acceptedStill,
  animationRequestId: "forbidden-for-still" }),
  /evaluation_package_unknown_branch_field/);
assert.throws(() => validateManifest({ ...acceptedStill,
  members: [...acceptedStill.members,
    member("provider-prompt", "accepted_provider_prompt", "8".repeat(64))] }));

for (const surface of [preservation, evaluationPackage, preservationPrompt,
  evaluationPrompt, expressionPrompt]) {
  assert.match(surface, /acceptedPromptEvidence/);
  assert.match(surface, /providerPromptPayloadHash/);
  assert.match(surface, /promptFileSha256/);
  assert.match(surface, /fake[\s\S]{0,40}prompt|MUST NOT invent|만들거나 요구하지/i);
}
assert.match(evaluationPackage, /generation\/ # strict branch only/);
assert.match(evaluationPackage, /accepted-capture\/ # accepted-result branch only/);
for (const token of ["evaluation_package_input_branch_conflict",
  "evaluation_package_input_branch_incomplete",
  "evaluation_package_unknown_branch_field"]) {
  assert.match(evaluationPackage, new RegExp(token));
  assert.match(evaluationPrompt, new RegExp(token));
}
for (const token of ["preservation_input_branch_conflict",
  "preservation_input_branch_incomplete", "preservation_input_unknown_field"]) {
  assert.match(preservation, new RegExp(token));
  assert.match(preservationPrompt, new RegExp(token));
}

console.log({ acceptedManifestPayloadHash: validateManifest(accepted),
  acceptedStillManifestPayloadHash: validateManifest(acceptedStill),
  strictManifestPayloadHash: validateManifest(strict), providerCalled: false,
  submitCount: 0 });
console.log("accepted capture evaluation-package branch vectors: PASS");
