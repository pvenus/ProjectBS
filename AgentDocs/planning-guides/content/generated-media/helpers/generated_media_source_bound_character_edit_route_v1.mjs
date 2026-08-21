import { createHash } from "node:crypto";
import { canonicalJson } from "./generated_media_canonical_serializers_v1.mjs";

export const ROUTE_CONTRACT_KEY =
  "projectbs_generated_media_source_bound_character_edit_route@1.0.0";
export const ROUTE_CONTRACT_PAYLOAD_SHA256 =
  "77b4b9d4d9d5db7a2c2fb1cdb5ccb1812faffe535559fdb57400515f48e05359";

const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const jcsSha = (value) => sha(Buffer.from(canonicalJson(value), "utf8"));
const jcsFile = (value) => Buffer.from(`${canonicalJson(value)}\n`, "utf8");

export function validateRouteContract(contract, editProfile) {
  if (contract.routeContractKey !== ROUTE_CONTRACT_KEY
    || jcsSha(contract) !== ROUTE_CONTRACT_PAYLOAD_SHA256)
    throw new Error("source_bound_edit_route_contract_hash_mismatch");
  if (editProfile.profileKey !== contract.profileBinding.profileKey
    || jcsSha(editProfile) !== contract.profileBinding.profilePayloadSha256)
    throw new Error("source_bound_edit_profile_binding_mismatch");
  const lines = editProfile.editContract.providerPromptLines;
  if (lines.length !== 6 || jcsSha(lines)
    !== contract.promptSerialization.providerPromptLinesJcsSha256)
    throw new Error("source_bound_edit_prompt_lines_mismatch");
  const promptBytes = Buffer.from(lines.join("\n"), "utf8");
  if (sha(promptBytes) !== contract.promptSerialization.providerPromptSha256
    || promptBytes.at(-1) === 10)
    throw new Error("source_bound_edit_prompt_serialization_mismatch");
  return promptBytes;
}

export function buildSourceBoundEditRoute({ contract, editProfile,
  authorityMainSha }) {
  const promptBytes = validateRouteContract(contract, editProfile);
  if (!/^[0-9a-f]{40}$/.test(authorityMainSha))
    throw new Error("source_bound_edit_authority_main_invalid");
  const source = contract.sourceBinding;
  const approvalEvidenceSha256 = jcsSha(contract.approvalEvidence);
  const referenceProjection = {
    mode: "referenced_image_paths",
    paths: [source.sourcePathEvidence],
    sourceSha256: source.sourceSha256,
  };
  const executionScope = {
    schemaVersion: contract.executionScopeContract.schemaVersion,
    authorityMainSha,
    requestId: source.requestId,
    assetType: "character_single_image",
    domainType: "character",
    contentId: source.contentId,
    structureProfile: "character_single_image_v2",
    sourceSha256: source.sourceSha256,
    generationReceiptSha256: source.generationReceiptSha256,
    generationHandoffSha256: source.generationHandoffSha256,
    profileKey: contract.profileBinding.profileKey,
    profilePayloadSha256: contract.profileBinding.profilePayloadSha256,
    routeContractKey: contract.routeContractKey,
    routeContractPayloadSha256: ROUTE_CONTRACT_PAYLOAD_SHA256,
    callableSchemaSha256: source.callableSchemaSha256,
    providerPromptSha256: sha(promptBytes),
    referenceProjection,
    approvalEvidence: contract.approvalEvidence,
    approvalEvidenceSha256,
    submitCountMaximum: contract.approvalEvidence.submitCountMaximum,
    retryCountMaximum: contract.approvalEvidence.retryCountMaximum,
    outputContract: editProfile.outputContract,
  };
  const executionScopeHash = jcsSha(executionScope);
  const idempotencyKey = `gmedit1.${executionScopeHash.slice(0, 20)}`;
  const routePayload = {
    schemaVersion: contract.routeIdentityContract.routePayloadSchemaVersion,
    authorityMainSha,
    requestId: source.requestId,
    assetType: "character_single_image",
    domainType: "character",
    contentId: source.contentId,
    structureProfile: "character_single_image_v2",
    sourcePathEvidence: source.sourcePathEvidence,
    sourceSha256: source.sourceSha256,
    generationReceiptPathEvidence: source.generationReceiptPathEvidence,
    generationReceiptSha256: source.generationReceiptSha256,
    generationHandoffSha256: source.generationHandoffSha256,
    profileKey: contract.profileBinding.profileKey,
    profilePayloadSha256: contract.profileBinding.profilePayloadSha256,
    routeContractKey: contract.routeContractKey,
    routeContractPayloadSha256: ROUTE_CONTRACT_PAYLOAD_SHA256,
    callableSchemaSha256: source.callableSchemaSha256,
    providerPromptLines: editProfile.editContract.providerPromptLines,
    promptSerialization: contract.promptSerialization,
    approvalEvidence: contract.approvalEvidence,
    approvalEvidenceSha256,
    executionScope,
    executionScopeHash,
    idempotencyKey,
    submitCountMaximum: 1,
    retryCountMaximum: 0,
    outputContract: editProfile.outputContract,
    state: contract.initialState,
    createdAt: contract.createdAtAuthority.value,
  };
  const routePayloadSha256 = jcsSha(routePayload);
  const routeId = `gmeditroute1.character_single_image.character.seojin.3.${routePayloadSha256.slice(0, 20)}`;
  const routePath = `AgentDocs/planning-data/generated-media-source-edits/v1/character_single_image/character.seojin.3/${routeId}.json`;
  const routeRecord = { schemaVersion: contract.routeIdentityContract.routeRecordSchemaVersion,
    routeId, routePayloadSha256, routePayload };
  const routeRecordBytes = jcsFile(routeRecord);
  const routeRecordSha256 = sha(routeRecordBytes);
  const executionHandoff = {
    schemaVersion: contract.executionHandoffContract.schemaVersion,
    routeId, routePath, routeRecordSha256, routePayloadSha256,
    authorityMainSha, executionScopeHash, idempotencyKey,
    providerPromptSha256: sha(promptBytes), referenceProjection,
    callableSchemaSha256: source.callableSchemaSha256,
    submitCountMaximum: 1, retryCountMaximum: 0,
    state: contract.initialState,
  };
  const executionHandoffSha256 = jcsSha(executionHandoff);
  const indexEntry = {
    schemaVersion: contract.indexContract.entrySchemaVersion,
    routeId, routePath, routeRecordSha256, routePayloadSha256,
    executionHandoffSha256, executionScopeHash, idempotencyKey,
    state: contract.initialState, createdAt: contract.createdAtAuthority.value,
  };
  return { promptBytes, approvalEvidenceSha256, executionScope,
    executionScopeHash, idempotencyKey, routePayload, routePayloadSha256,
    routeId, routePath, routeRecord, routeRecordBytes, routeRecordSha256,
    executionHandoff, executionHandoffSha256, indexEntry };
}

export function buildRouteIndex({ contract, entries }) {
  const sorted = [...entries].sort((left, right) =>
    left.routeId < right.routeId ? -1 : left.routeId > right.routeId ? 1 : 0);
  const seen = new Set();
  const entryMembers = ["schemaVersion", "routeId", "routePath",
    "routeRecordSha256", "routePayloadSha256", "executionHandoffSha256",
    "executionScopeHash", "idempotencyKey", "state", "createdAt"].sort();
  for (const entry of sorted) {
    if (entry.schemaVersion !== contract.indexContract.entrySchemaVersion
      || canonicalJson(Object.keys(entry).sort()) !== canonicalJson(entryMembers))
      throw new Error("source_bound_edit_route_index_entry_invalid");
    if (seen.has(entry.routeId)) throw new Error("source_bound_edit_route_index_duplicate");
    seen.add(entry.routeId);
  }
  const indexPayload = { schemaVersion: contract.indexContract.payloadSchemaVersion,
    entries: sorted };
  const indexPayloadSha256 = jcsSha(indexPayload);
  const index = { schemaVersion: contract.indexContract.indexSchemaVersion,
    indexPayloadSha256, entries: sorted };
  const indexBytes = jcsFile(index);
  return { indexPayload, indexPayloadSha256, index, indexBytes,
    indexSha256: sha(indexBytes), indexPath: contract.indexContract.indexPath };
}
