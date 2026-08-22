// Pure trusted-local MAIN evaluation record projection vectors. This test does
// not make an evaluation decision or write workflow artifacts.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { ADAPTER_KEY, ADAPTER_PAYLOAD_SHA256, INPUT_SCHEMA, INDEX_SCHEMA,
  canonicalJson, emptyIndex, jsonFileBytes, projectTrustedLocalMainEvaluation,
  sha256 } from "../helpers/generated_media_trusted_local_main_evaluation_projector_v1.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(here, "..");
const contentRoot = join(guideRoot, "..");
const repoDocsRoot = join(contentRoot, "..", "..");
const adapterPath = join(guideRoot, "helpers",
  "generated_media_trusted_local_main_evaluation_adapter_v1.json");
const adapterBytes = readFileSync(adapterPath);
assert.equal(adapterBytes.includes(0x0d), false);
assert.equal(adapterBytes.at(-1), 0x0a);
const adapter = JSON.parse(adapterBytes.toString("utf8"));
assert.equal(sha256(Buffer.from(canonicalJson(adapter), "utf8")),
  ADAPTER_PAYLOAD_SHA256);
assert.equal(adapter.adapterKey, ADAPTER_KEY);
assert.equal(adapter.fixtureBinding.mediaSha256,
  "ff512be1ac75ba0924eab316679dcab4ee171f4a0703014791f2f295e8a6d327");
assert.equal(adapter.fixtureBinding.sourceBoundReceiptSha256,
  "27031cd5d53f04233091b5811665eb68ff940ba7c97c0bd106a5edd79e45ee7e");

const mediaSha256 = adapter.fixtureBinding.mediaSha256;
const receipt = { schemaVersion: "test_source_bound_receipt_v1",
  outputSha256: mediaSha256, state: "complete" };
const receiptBytes = jsonFileBytes(receipt);
const receiptSha256 = sha256(receiptBytes);
const evidence = { schemaVersion: "generated_media_independent_evaluation_evidence_v1",
  evaluationTaskId: "independent-evaluation-task", contentId: "character.seojin.2",
  mediaSha256, decision: "PASS", facts: [
    { factId: "identity", outcome: "PASS", evidenceRef: "fact:identity" },
    { factId: "technical", outcome: "PASS", evidenceRef: "fact:technical" },
  ] };
const evidenceBytes = jsonFileBytes(evidence);
const indexBytes = jsonFileBytes(emptyIndex("character.seojin.2"));
const input = {
  schemaVersion: INPUT_SCHEMA,
  adapterKey: ADAPTER_KEY,
  adapterPayloadSha256: ADAPTER_PAYLOAD_SHA256,
  referencePolicyKey: "generated_media_trusted_evaluated_prior_grade_main_reference@1.0.0",
  contentId: "character.seojin.2",
  assetType: "character_single_image",
  domainType: "character",
  structureProfile: "character_single_image_v2",
  mediaPath: adapter.fixtureBinding.mediaPath,
  mediaSha256,
  mediaByteLength: adapter.fixtureBinding.mediaByteLength,
  profileKey: "projectbs_character_open_ink_identity_anchored_source_bound_green_carrier_fit@1.0.0",
  profilePayloadSha256: "1a669eed96cda8a2add59445cbf3c1e174fe359b1c03bf42ed707477d3cdc138",
  evaluationEvidencePath: "evaluation/evidence/character.seojin.2.json",
  evaluationEvidenceSha256: sha256(evidenceBytes),
  sourceBoundReceiptId: `gmsourcereceipt1.${receiptSha256.slice(0, 20)}`,
  sourceBoundReceiptPath: "receipts/character.seojin.2.json",
  sourceBoundReceiptSha256: receiptSha256,
  sourceBoundContentShaPointer: "/outputSha256",
  publicationState: "local_unpublished",
  indexBeforeSha256: sha256(indexBytes),
};

const projected = projectTrustedLocalMainEvaluation({ input,
  evaluationEvidenceBytes: evidenceBytes, sourceBoundReceiptBytes: receiptBytes,
  indexBeforeBytes: indexBytes });
const rerun = projectTrustedLocalMainEvaluation({ input,
  evaluationEvidenceBytes: evidenceBytes, sourceBoundReceiptBytes: receiptBytes,
  indexBeforeBytes: indexBytes });
assert.equal(projected.evaluationRecordId, rerun.evaluationRecordId);
assert.equal(projected.recordBytes.equals(rerun.recordBytes), true);
assert.equal(projected.indexAfterBytes.equals(rerun.indexAfterBytes), true);
assert.equal(projected.record.result, "PASS");
assert.equal(projected.record.scorePolicy, "not_scored");
assert.equal(projected.record.providerReceiptPolicy, "not_required_not_claimed");
assert.equal(projected.record.publicationState, "local_unpublished");
assert.match(projected.evaluationRecordId,
  /^gmtrusteval1\.character\.seojin\.2\.[0-9a-f]{20}$/);
assert.equal(projected.evaluationRecordPath,
  `AgentDocs/planning-data/generated-media-evaluations/v1/trusted_local_main/character.seojin.2/${projected.evaluationRecordId}.json`);

for (const decision of ["PASS", "FAIL", "unavailable"]) {
  const nextEvidence = { ...evidence, decision };
  const nextBytes = jsonFileBytes(nextEvidence);
  const result = projectTrustedLocalMainEvaluation({ input: { ...input,
    evaluationEvidenceSha256: sha256(nextBytes) },
  evaluationEvidenceBytes: nextBytes, sourceBoundReceiptBytes: receiptBytes,
  indexBeforeBytes: indexBytes });
  assert.equal(result.record.result, decision);
  assert.equal(result.record.scorePolicy, "not_scored");
}

assert.throws(() => projectTrustedLocalMainEvaluation({ input,
  evaluationEvidenceBytes: Buffer.alloc(0), sourceBoundReceiptBytes: receiptBytes,
  indexBeforeBytes: indexBytes }), /trusted_local_evaluation_evidence_hash_mismatch/);
assert.throws(() => projectTrustedLocalMainEvaluation({ input,
  evaluationEvidenceBytes: evidenceBytes, sourceBoundReceiptBytes: Buffer.from("chat-only\n"),
  indexBeforeBytes: indexBytes }), /trusted_local_evaluation_source_receipt_hash_mismatch/);
const mismatchReceiptBytes = jsonFileBytes({ ...receipt, outputSha256: "0".repeat(64) });
assert.throws(() => projectTrustedLocalMainEvaluation({ input: { ...input,
  sourceBoundReceiptSha256: sha256(mismatchReceiptBytes),
  sourceBoundReceiptId: `gmsourcereceipt1.${sha256(mismatchReceiptBytes).slice(0, 20)}` },
evaluationEvidenceBytes: evidenceBytes, sourceBoundReceiptBytes: mismatchReceiptBytes,
indexBeforeBytes: indexBytes }), /trusted_local_evaluation_source_receipt_content_mismatch/);
const scoredEvidenceBytes = jsonFileBytes({ ...evidence, numericScore: 100 });
assert.throws(() => projectTrustedLocalMainEvaluation({ input: { ...input,
  evaluationEvidenceSha256: sha256(scoredEvidenceBytes) },
evaluationEvidenceBytes: scoredEvidenceBytes, sourceBoundReceiptBytes: receiptBytes,
indexBeforeBytes: indexBytes }), /trusted_local_evaluation_numeric_score_forbidden/);
assert.throws(() => projectTrustedLocalMainEvaluation({ input: { ...input,
  indexBeforeSha256: "0".repeat(64) }, evaluationEvidenceBytes: evidenceBytes,
sourceBoundReceiptBytes: receiptBytes, indexBeforeBytes: indexBytes }),
/trusted_local_evaluation_index_cas_mismatch/);

const occupied = { schemaVersion: INDEX_SCHEMA, contentId: input.contentId,
  entries: { [projected.evaluationRecordId]: { ...projected.entry,
    evaluationRecordSha256: "f".repeat(64) } } };
const occupiedBytes = jsonFileBytes(occupied);
assert.throws(() => projectTrustedLocalMainEvaluation({ input: { ...input,
  indexBeforeSha256: sha256(occupiedBytes) }, evaluationEvidenceBytes: evidenceBytes,
sourceBoundReceiptBytes: receiptBytes, indexBeforeBytes: occupiedBytes }),
/trusted_local_evaluation_index_collision/);

const surfaces = [
  join(guideRoot, "GeneratedMediaTrustedLocalMainEvaluationRecordGuide.md"),
  join(guideRoot, "GeneratedMediaEvaluationPackageGuide.md"),
  join(guideRoot, "GeneratedMediaRecordGuide.md"),
  join(guideRoot, "GeneratedMediaSequentialGradeIdentityAuthorityGuide.md"),
  join(contentRoot, "GeneratedImageEvaluationPipelineGuide.md"),
  join(repoDocsRoot, "task-prompts", "content", "GeneratedImageEvaluationPrompt.md"),
  join(repoDocsRoot, "task-prompts", "content", "generated-media",
    "GeneratedMediaCharacterExpressionEvaluationPrompt.md"),
  join(repoDocsRoot, "task-prompts", "content", "generated-media",
    "GeneratedMediaTrustedLocalMainEvaluationRecordPrompt.md"),
];
for (const surface of surfaces) {
  const text = readFileSync(surface, "utf8").replaceAll("\r\n", "\n");
  assert.match(text, /generated_media_trusted_local_main_evaluation_record_adapter@1\.0\.0/);
  assert.match(text, /c76b11ee51f641da78b54048c670658628e379ce9f74f8b9cb878c1c9742953e/);
  assert.match(text, /generated_media_trusted_local_main_evaluation_record_v1/);
  assert.match(text, /PASS[| ]+FAIL[| ]+unavailable/);
  assert.match(text, /not_scored|no-score|numeric score/i);
}

console.log({ adapterKey: ADAPTER_KEY,
  adapterPayloadSha256: ADAPTER_PAYLOAD_SHA256,
  recordSchemaVersion: projected.record.schemaVersion,
  deterministicRecordId: projected.evaluationRecordId,
  providerCalled: false, evaluationDecisionMade: false, cost: 0 });
console.log("generated media trusted-local MAIN evaluation adapter vectors: PASS");
