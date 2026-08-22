import { createHash } from "node:crypto";

export const ADAPTER_KEY =
  "generated_media_trusted_local_main_evaluation_record_adapter@1.0.0";
export const ADAPTER_PAYLOAD_SHA256 =
  "c76b11ee51f641da78b54048c670658628e379ce9f74f8b9cb878c1c9742953e";
export const INPUT_SCHEMA =
  "generated_media_trusted_local_main_evaluation_projection_input_v1";
export const RECORD_SCHEMA =
  "generated_media_trusted_local_main_evaluation_record_v1";
export const INDEX_SCHEMA =
  "generated_media_trusted_local_main_evaluation_index_v1";

export function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

export function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

export function jsonFileBytes(value) {
  return Buffer.from(`${canonicalJson(value)}\n`, "utf8");
}

function assertClosedKeys(value, keys, token) {
  if (value === null || typeof value !== "object" || Array.isArray(value)
      || canonicalJson(Object.keys(value).sort()) !== canonicalJson([...keys].sort()))
    throw new Error(token);
}

function assertSha(value, token) {
  if (!/^[0-9a-f]{64}$/.test(value)) throw new Error(token);
}

function parseCanonicalEvidence(bytes, expectedSha, token) {
  if (!Buffer.isBuffer(bytes) || bytes.length === 0) throw new Error(token);
  if (sha256(bytes) !== expectedSha) throw new Error(token);
  if (bytes.includes(0x0d) || bytes.at(-1) !== 0x0a
      || bytes.subarray(0, 3).equals(Buffer.from([0xef, 0xbb, 0xbf])))
    throw new Error(token);
  let value;
  try { value = JSON.parse(bytes.subarray(0, -1).toString("utf8")); }
  catch { throw new Error(token); }
  if (!jsonFileBytes(value).equals(bytes)) throw new Error(token);
  return value;
}

function containsNumericScore(value) {
  if (Array.isArray(value)) return value.some(containsNumericScore);
  if (value !== null && typeof value === "object") return Object.entries(value)
    .some(([key, child]) => /score/i.test(key) || containsNumericScore(child));
  return false;
}

function resolvePointer(value, pointer) {
  if (!["/outputSha256", "/mediaSha256"].includes(pointer))
    throw new Error("trusted_local_evaluation_input_mismatch");
  return value[pointer.slice(1)];
}

const inputKeys = ["schemaVersion", "adapterKey", "adapterPayloadSha256",
  "referencePolicyKey", "contentId", "assetType", "domainType",
  "structureProfile", "mediaPath", "mediaSha256", "mediaByteLength",
  "profileKey", "profilePayloadSha256", "evaluationEvidencePath",
  "evaluationEvidenceSha256", "sourceBoundReceiptId",
  "sourceBoundReceiptPath", "sourceBoundReceiptSha256",
  "sourceBoundContentShaPointer", "publicationState", "indexBeforeSha256"];
const evidenceKeys = ["schemaVersion", "evaluationTaskId", "contentId",
  "mediaSha256", "decision", "facts"];
const factKeys = ["factId", "outcome", "evidenceRef"];

export function emptyIndex(contentId) {
  return { schemaVersion: INDEX_SCHEMA, contentId, entries: {} };
}

export function projectTrustedLocalMainEvaluation({ input,
  evaluationEvidenceBytes, sourceBoundReceiptBytes, indexBeforeBytes }) {
  assertClosedKeys(input, inputKeys, "trusted_local_evaluation_input_mismatch");
  if (input.schemaVersion !== INPUT_SCHEMA || input.adapterKey !== ADAPTER_KEY
      || input.adapterPayloadSha256 !== ADAPTER_PAYLOAD_SHA256
      || !["generated_media_trusted_local_evaluated_main_reference@1.0.0",
        "generated_media_trusted_evaluated_prior_grade_main_reference@1.0.0"]
        .includes(input.referencePolicyKey)
      || input.assetType !== "character_single_image"
      || input.domainType !== "character"
      || input.structureProfile !== "character_single_image_v2"
      || input.publicationState !== "local_unpublished"
      || typeof input.contentId !== "string" || input.contentId.length === 0
      || typeof input.mediaPath !== "string" || input.mediaPath.length === 0
      || !Number.isSafeInteger(input.mediaByteLength) || input.mediaByteLength <= 0
      || typeof input.profileKey !== "string" || input.profileKey.length === 0)
    throw new Error("trusted_local_evaluation_input_mismatch");
  for (const value of [input.mediaSha256, input.profilePayloadSha256,
    input.evaluationEvidenceSha256, input.sourceBoundReceiptSha256,
    input.indexBeforeSha256]) assertSha(value, "trusted_local_evaluation_input_mismatch");
  const expectedReceiptId =
    `gmsourcereceipt1.${input.sourceBoundReceiptSha256.slice(0, 20)}`;
  if (input.sourceBoundReceiptId !== expectedReceiptId)
    throw new Error("trusted_local_evaluation_input_mismatch");

  const evidence = parseCanonicalEvidence(evaluationEvidenceBytes,
    input.evaluationEvidenceSha256,
    "trusted_local_evaluation_evidence_hash_mismatch");
  if (containsNumericScore(evidence))
    throw new Error("trusted_local_evaluation_numeric_score_forbidden");
  assertClosedKeys(evidence, evidenceKeys,
    "trusted_local_evaluation_evidence_unavailable");
  if (evidence.schemaVersion !== "generated_media_independent_evaluation_evidence_v1"
      || !["PASS", "FAIL", "unavailable"].includes(evidence.decision)
      || evidence.contentId !== input.contentId
      || evidence.mediaSha256 !== input.mediaSha256
      || typeof evidence.evaluationTaskId !== "string"
      || evidence.evaluationTaskId.length === 0
      || !Array.isArray(evidence.facts) || evidence.facts.length === 0)
    throw new Error("trusted_local_evaluation_content_mismatch");
  for (const fact of evidence.facts) {
    assertClosedKeys(fact, factKeys, "trusted_local_evaluation_evidence_unavailable");
    if (typeof fact.factId !== "string" || fact.factId.length === 0
        || !["PASS", "FAIL", "unavailable"].includes(fact.outcome)
        || typeof fact.evidenceRef !== "string" || fact.evidenceRef.length === 0)
      throw new Error("trusted_local_evaluation_evidence_unavailable");
  }

  const receipt = parseCanonicalEvidence(sourceBoundReceiptBytes,
    input.sourceBoundReceiptSha256,
    "trusted_local_evaluation_source_receipt_hash_mismatch");
  if (resolvePointer(receipt, input.sourceBoundContentShaPointer)
      !== input.mediaSha256)
    throw new Error("trusted_local_evaluation_source_receipt_content_mismatch");

  const payload = {
    schemaVersion: "generated_media_trusted_local_main_evaluation_hash_payload_v1",
    adapterKey: input.adapterKey,
    adapterPayloadSha256: input.adapterPayloadSha256,
    referencePolicyKey: input.referencePolicyKey,
    contentId: input.contentId,
    assetType: input.assetType,
    domainType: input.domainType,
    structureProfile: input.structureProfile,
    mediaPath: input.mediaPath,
    mediaSha256: input.mediaSha256,
    mediaByteLength: input.mediaByteLength,
    profileKey: input.profileKey,
    profilePayloadSha256: input.profilePayloadSha256,
    independentEvaluationEvidence: {
      evaluationTaskId: evidence.evaluationTaskId,
      path: input.evaluationEvidencePath,
      sha256: input.evaluationEvidenceSha256,
      factsSha256: sha256(Buffer.from(canonicalJson(evidence.facts), "utf8")),
    },
    sourceBoundReceipt: {
      id: input.sourceBoundReceiptId,
      path: input.sourceBoundReceiptPath,
      sha256: input.sourceBoundReceiptSha256,
      contentShaPointer: input.sourceBoundContentShaPointer,
      contentSha256: input.mediaSha256,
    },
    evaluationStatus: "completed",
    result: evidence.decision,
    scorePolicy: "not_scored",
    providerReceiptPolicy: "not_required_not_claimed",
    publicationState: input.publicationState,
  };
  const evaluationPayloadSha256 =
    sha256(Buffer.from(canonicalJson(payload), "utf8"));
  const evaluationRecordId =
    `gmtrusteval1.${input.contentId}.${evaluationPayloadSha256.slice(0, 20)}`;
  const record = { ...payload, schemaVersion: RECORD_SCHEMA,
    evaluationRecordId, evaluationPayloadSha256 };
  const recordBytes = jsonFileBytes(record);
  const evaluationRecordSha256 = sha256(recordBytes);
  const evaluationRecordPath =
    `AgentDocs/planning-data/generated-media-evaluations/v1/trusted_local_main/${input.contentId}/${evaluationRecordId}.json`;

  const indexBefore = parseCanonicalEvidence(indexBeforeBytes,
    input.indexBeforeSha256, "trusted_local_evaluation_index_cas_mismatch");
  assertClosedKeys(indexBefore, ["schemaVersion", "contentId", "entries"],
    "trusted_local_evaluation_index_cas_mismatch");
  if (indexBefore.schemaVersion !== INDEX_SCHEMA
      || indexBefore.contentId !== input.contentId
      || indexBefore.entries === null || typeof indexBefore.entries !== "object"
      || Array.isArray(indexBefore.entries))
    throw new Error("trusted_local_evaluation_index_cas_mismatch");
  const entry = { evaluationRecordId, evaluationRecordPath,
    evaluationPayloadSha256, evaluationRecordSha256,
    mediaSha256: input.mediaSha256, result: evidence.decision };
  const existing = indexBefore.entries[evaluationRecordId];
  if (existing && canonicalJson(existing) !== canonicalJson(entry))
    throw new Error("trusted_local_evaluation_index_collision");
  const entries = { ...indexBefore.entries, [evaluationRecordId]: entry };
  const sortedEntries = Object.fromEntries(Object.entries(entries)
    .sort(([left], [right]) => Buffer.from(left).compare(Buffer.from(right))));
  const indexAfter = { schemaVersion: INDEX_SCHEMA,
    contentId: input.contentId, entries: sortedEntries };
  const indexAfterBytes = jsonFileBytes(indexAfter);

  return { payload, evaluationRecordId, evaluationPayloadSha256, record,
    recordBytes, evaluationRecordPath, evaluationRecordSha256, entry,
    indexAfter, indexAfterBytes, indexAfterSha256: sha256(indexAfterBytes),
    reuseStatus: existing ? "reused_identical" : "created" };
}
