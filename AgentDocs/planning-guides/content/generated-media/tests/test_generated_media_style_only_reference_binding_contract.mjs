// Durable style-only reference review/binding vectors.
// No provider call, image generation, planning write, or evaluation occurs.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const repo = join(testDir, "..", "..", "..", "..", "..");

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}
function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}
function hashObject(value) {
  return sha256(Buffer.from(canonicalJson(value), "utf8"));
}

const assetType = "character_single_image";
const styleReferenceId = "open_ink_wash_dynamic_contour";
const assetSha256 = "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf";
const assetPath = `AgentDocs/reference-assets/generated-media/style-only/${assetType}/${styleReferenceId}/${assetSha256}.png`;
const reviewDir = `AgentDocs/planning-data/style-reference-reviews/v1/${assetType}/${styleReferenceId}`;

const reviewHashPayload = {
  schemaVersion: "generated_media_style_reference_review_v1",
  assetType,
  styleReferenceId,
  purpose: "style_only",
  asset: {
    projectRelativePath: assetPath,
    sha256: assetSha256,
    mediaType: "image/png",
    byteLength: 1883943,
    pixelDimensions: { width: 1024, height: 1536 },
  },
  profileBindings: [{
    expressionProfileKey: "projectbs_character_open_ink_wash_dynamic_contour@1.0.0",
    expressionProfilePayloadHash: "37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd",
  }, {
    expressionProfileKey: "projectbs_character_open_ink_wash_dynamic_contour@2.0.0",
    expressionProfilePayloadHash: "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5",
  }],
  allowedObservationCategories: ["contour_openness", "pressure_variable_mok_seon",
    "broad_rough_pigment", "palette_role_distribution", "negative_space_balance",
    "non_semantic_composition_density"],
  prohibitedSemanticTransfers: ["person", "person_identity", "canonical_character_identity",
    "pose", "action", "clothing", "equipment", "edit_target"],
  providerReferencePolicy: {
    authorizedRole: "style_only", capabilitySupportRequired: true,
    identityReferenceRole: "prohibited", editReferenceRole: "prohibited",
    promptSubjectDescriptionFromReference: "prohibited",
  },
  reviewAuthority: {
    type: "authenticated_user_selected_best_cut",
    sourceThreadId: "01a00a7b-94b3-7423-b79b-ac41cefcca59",
    reviewedAssetSha256: assetSha256,
  },
  status: "approved",
};
const reviewPayloadSha256 = hashObject(reviewHashPayload);
const reviewRecordId = `gmstyleref1.${assetType}.${styleReferenceId}.${reviewPayloadSha256.slice(0, 20)}`;
const reviewRecordPath = `${reviewDir}/${reviewRecordId}.json`;
const reviewRecord = {
  ...reviewHashPayload,
  reviewRecordId,
  reviewPayloadSha256,
  validation: { schema: "valid", assetBytes: "valid",
    purposeAndTransferBoundary: "valid", profileBindings: "valid" },
};
const reviewRecordBytes = Buffer.from(`${canonicalJson(reviewRecord)}\n`, "utf8");
const reviewRecordSha256 = sha256(reviewRecordBytes);

function assertClosedKeys(value, expected, token = "style_reference_review_payload_mismatch") {
  assert.deepEqual(Object.keys(value).sort(), [...expected].sort(), token);
}

const binding = {
  role: "style_only",
  projectRelativePath: assetPath,
  sha256: assetSha256,
  reviewRecordId,
  reviewRecordPath,
  reviewRecordSha256,
};

function validateBinding(candidate, { profileKey, profileHash } = {
  profileKey: reviewRecord.profileBindings[1].expressionProfileKey,
  profileHash: reviewRecord.profileBindings[1].expressionProfilePayloadHash,
}) {
  const keys = ["role", "projectRelativePath", "sha256", "reviewRecordId",
    "reviewRecordPath", "reviewRecordSha256"];
  if (Object.keys(candidate).length !== keys.length || keys.some((key) => !Object.hasOwn(candidate, key))) {
    throw new Error("style_reference_binding_incomplete");
  }
  if (candidate.role !== "style_only" || /^[A-Za-z]:[\\/]/.test(candidate.projectRelativePath) ||
      !candidate.projectRelativePath.startsWith("AgentDocs/reference-assets/generated-media/style-only/")) {
    throw new Error("style_reference_role_invalid");
  }
  if (candidate.projectRelativePath !== assetPath || candidate.sha256 !== assetSha256) {
    throw new Error("style_reference_asset_hash_mismatch");
  }
  if (candidate.reviewRecordId !== reviewRecordId || candidate.reviewRecordPath !== reviewRecordPath ||
      candidate.reviewRecordSha256 !== reviewRecordSha256) {
    throw new Error("style_reference_review_record_hash_mismatch");
  }
  if (!reviewRecord.profileBindings.some((profile) =>
    profile.expressionProfileKey === profileKey && profile.expressionProfilePayloadHash === profileHash)) {
    throw new Error("style_reference_binding_scope_mismatch");
  }
  if (reviewRecord.purpose !== "style_only" || reviewRecord.status !== "approved" ||
      !["person", "person_identity", "canonical_character_identity", "pose", "action",
        "clothing", "equipment", "edit_target"].every((item) =>
        reviewRecord.prohibitedSemanticTransfers.includes(item))) {
    throw new Error("style_reference_semantic_transfer_forbidden");
  }
  return true;
}

const assetBytes = readFileSync(join(repo, assetPath));
assert.equal(sha256(assetBytes), assetSha256);
assert.equal(assetBytes.length, 1883943);
assert.deepEqual([...assetBytes.subarray(0, 8)], [137, 80, 78, 71, 13, 10, 26, 10]);
assert.equal(assetBytes.readUInt32BE(16), 1024);
assert.equal(assetBytes.readUInt32BE(20), 1536);

const storedReviewBytes = readFileSync(join(repo, reviewRecordPath));
assert.ok(storedReviewBytes.equals(reviewRecordBytes));
assert.equal(sha256(storedReviewBytes), "51630e6c2c4ec80caae9bf5c995f7673e2b8fddf83870c5a28452971fa2be4c2");
assert.deepEqual(JSON.parse(storedReviewBytes.toString("utf8")), reviewRecord);
assert.equal(storedReviewBytes.at(-1), 0x0a);
assert.notEqual(storedReviewBytes.at(-2), 0x0d);

const indexPath = `${reviewDir}/review_index.json`;
const indexBytes = readFileSync(join(repo, indexPath));
const index = JSON.parse(indexBytes.toString("utf8"));
assert.ok(indexBytes.equals(Buffer.from(`${canonicalJson(index)}\n`, "utf8")));
assertClosedKeys(index, ["schemaVersion", "assetType", "styleReferenceId", "entries"]);
assert.equal(index.schemaVersion, "generated_media_style_reference_review_index_v1");
assert.equal(index.assetType, assetType);
assert.equal(index.styleReferenceId, styleReferenceId);
assert.deepEqual(Object.keys(index.entries), [reviewRecordId]);
assert.deepEqual(index.entries[reviewRecordId], {
  reviewRecordId, recordPath: reviewRecordPath, recordSha256: reviewRecordSha256,
  reviewPayloadSha256, assetPath, assetSha256, purpose: "style_only", status: "approved",
});

assert.equal(validateBinding(binding), true);
assert.throws(() => validateBinding({ role: "style_only", projectRelativePath: assetPath,
  sha256: assetSha256 }), /style_reference_binding_incomplete/);
assert.throws(() => validateBinding({ ...binding,
  projectRelativePath: "C:\\temp\\best-cut.png" }), /style_reference_role_invalid/);
assert.throws(() => validateBinding({ ...binding, sha256: "0".repeat(64) }),
  /style_reference_asset_hash_mismatch/);
assert.throws(() => validateBinding({ ...binding, reviewRecordSha256: "0".repeat(64) }),
  /style_reference_review_record_hash_mismatch/);
assert.throws(() => validateBinding(binding, { profileKey: reviewRecord.profileBindings[1].expressionProfileKey,
  profileHash: "0".repeat(64) }), /style_reference_binding_scope_mismatch/);
assert.throws(() => validateBinding({ ...binding, role: "identity_reference" }),
  /style_reference_role_invalid/);

const guide = readFileSync(join(repo,
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md"), "utf8");
for (const required of ["generated_media_style_reference_review_v1",
  "style_reference_binding_incomplete", "style_reference_semantic_transfer_forbidden",
  "person_identity", "edit_target"]) assert.ok(guide.includes(required), required);

const visualGuide = readFileSync(join(repo,
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md"), "utf8");
assert.ok(visualGuide.includes("projectbs_character_open_ink_wash_dynamic_contour@1.0.0"));
assert.ok(visualGuide.includes("37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd"));
assert.ok(visualGuide.includes("projectbs_character_open_ink_wash_dynamic_contour@2.0.0"));
assert.ok(visualGuide.includes("b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5"));

console.log({ reviewRecordId, reviewPayloadSha256, reviewRecordSha256,
  assetSha256, assetByteLength: assetBytes.length });
console.log("generated media durable style-only reference binding vectors: PASS");
