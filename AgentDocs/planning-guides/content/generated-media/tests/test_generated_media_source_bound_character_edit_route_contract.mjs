// Deterministic source-bound character edit route projection vectors.
// No route artifacts are written and no provider/media operation is performed.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { canonicalJson } from
  "../helpers/generated_media_canonical_serializers_v1.mjs";
import {
  ROUTE_CONTRACT_KEY,
  ROUTE_CONTRACT_PAYLOAD_SHA256,
  buildRouteIndex,
  buildSourceBoundEditRoute,
  validateRouteContract,
} from "../helpers/generated_media_source_bound_character_edit_route_v1.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, "..");
const repo = join(root, "..", "..", "..", "..");
const load = (name) => JSON.parse(readFileSync(join(root, "helpers", name), "utf8"));
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const jcsSha = (value) => sha(Buffer.from(canonicalJson(value), "utf8"));

const contract = load("generated_media_source_bound_character_edit_route_contract_v1.json");
const profile = load("generated_media_source_bound_character_edit_profile_v1.json");
assert.equal(contract.routeContractKey, ROUTE_CONTRACT_KEY);
assert.equal(jcsSha(contract), ROUTE_CONTRACT_PAYLOAD_SHA256);
assert.equal(ROUTE_CONTRACT_PAYLOAD_SHA256,
  "77b4b9d4d9d5db7a2c2fb1cdb5ccb1812faffe535559fdb57400515f48e05359");
assert.equal(jcsSha(profile),
  "aa65434f5fb9c22cb42db199c936ee414648b933f4b83c159065341f4e704011");
assert.equal(validateRouteContract(contract, profile).toString("utf8"),
  profile.editContract.providerPromptLines.join("\n"));
assert.equal(sha(Buffer.from(profile.editContract.providerPromptLines.join("\n"))),
  "3a90b405e4303362dc912a19117f0501fa4b0575ee188aaf8a6f8353fb7d255d");
assert.equal(jcsSha(profile.editContract.providerPromptLines),
  "0b932ce168ba6ad422ad678b575dcb0ac04b0f08a3494aa644cead3e3ed2044c");
assert.notEqual(contract.promptSerialization.providerPromptSha256,
  contract.promptSerialization.providerPromptLinesJcsSha256);
assert.equal(contract.promptSerialization.terminalLf, false);

assert.deepEqual(contract.approvalEvidence, {
  approvedAction: "one_source_bound_character_edit_submit",
  approvedSourceSha256: "d435d0a6e5a7de4e7c50cd4e2552145eaa1eb8310d8874b37ed1e1a5a4c82c3d",
  authorityKind: "authenticated_user_request",
  authorityThreadId: "01a0235c-ba97-7330-ae2b-0e381a965afc",
  retryCountMaximum: 0,
  schemaVersion: "generated_media_source_bound_character_edit_approval_evidence_v1",
  submitCountMaximum: 1,
});
assert.equal(jcsSha(contract.approvalEvidence),
  "35a6f34e7e0f5e1f078121cb43fd113e4d8d596c43fe31d0e8d2bc54a1dc531a");
assert.equal(contract.initialState, "ready_for_generation");
assert.equal(contract.createdAtAuthority.value, "2026-08-22T05:50:05+09:00");

const authorityMainSha = "7423915a7b10d13e75eae895f06566dd8efa2591";
const projected = buildSourceBoundEditRoute({ contract, editProfile: profile,
  authorityMainSha });
assert.equal(projected.approvalEvidenceSha256,
  "35a6f34e7e0f5e1f078121cb43fd113e4d8d596c43fe31d0e8d2bc54a1dc531a");
assert.equal(projected.executionScopeHash,
  "fdc984220a1d539a4efca2d87a5594647ccb54e3127734897451e97afa38a5b3");
assert.equal(projected.idempotencyKey, "gmedit1.fdc984220a1d539a4efc");
assert.equal(projected.routePayloadSha256,
  "a05a9b9b1506299298222a3870ef486ebcd74d60dce6a4be31c945127fb115f1");
assert.equal(projected.routeId,
  "gmeditroute1.character_single_image.character.seojin.3.a05a9b9b150629929822");
assert.equal(projected.routeRecordSha256,
  "d385de66c4957bd724527223515a370d2dc74bab60f8370ac57d2e976a54fc2d");
assert.equal(projected.executionHandoffSha256,
  "2c55d801f40dfd9fd38ebc8ce97c3a624b74c280d61a80d34fb41db6bf8b8b3a");
assert.equal(projected.routeRecordBytes.at(-1), 10);
assert.equal(projected.routeRecordBytes.includes(Buffer.from("\r")), false);
assert.equal(projected.executionScope.authorityMainSha, authorityMainSha);
assert.deepEqual(projected.executionScope.referenceProjection, {
  mode: "referenced_image_paths",
  paths: [contract.sourceBinding.sourcePathEvidence],
  sourceSha256: contract.sourceBinding.sourceSha256,
});

assert.deepEqual(Object.keys(projected.executionHandoff).sort(), [
  "authorityMainSha", "callableSchemaSha256", "executionScopeHash",
  "idempotencyKey", "providerPromptSha256", "referenceProjection",
  "retryCountMaximum", "routeId", "routePath", "routePayloadSha256",
  "routeRecordSha256", "schemaVersion", "state", "submitCountMaximum",
].sort());
const index = buildRouteIndex({ contract, entries: [projected.indexEntry] });
assert.equal(index.indexPayloadSha256,
  "f74bc90d3518e335d7977cf96890edff999d103292ccf0e155765728f1a65f06");
assert.equal(index.indexSha256,
  "0f8a5c0feabae4409316e2c3a58ee561079c5d034771004a67f3278175624113");
assert.equal(index.indexBytes.at(-1), 10);
assert.throws(() => buildRouteIndex({ contract,
  entries: [projected.indexEntry, projected.indexEntry] }),
  /source_bound_edit_route_index_duplicate/);
const invalidEntry = { ...projected.indexEntry, unknown: true };
assert.throws(() => buildRouteIndex({ contract, entries: [invalidEntry] }),
  /source_bound_edit_route_index_entry_invalid|source_bound_edit_route_index/);

const driftedContract = structuredClone(contract);
driftedContract.initialState = "active";
assert.throws(() => validateRouteContract(driftedContract, profile),
  /source_bound_edit_route_contract_hash_mismatch/);
const driftedProfile = structuredClone(profile);
driftedProfile.editContract.providerPromptLines[0] += " ";
assert.throws(() => validateRouteContract(contract, driftedProfile),
  /source_bound_edit_profile_binding_mismatch/);
assert.throws(() => buildSourceBoundEditRoute({ contract, editProfile: profile,
  authorityMainSha: "not-a-commit" }), /source_bound_edit_authority_main_invalid/);

const surfaces = [
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaSourceBoundMainCompletionGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterSourceBoundEditGenerationPrompt.md",
];
for (const surface of surfaces) {
  const text = readFileSync(join(repo, surface), "utf8").replaceAll("\r\n", "\n");
  assert.ok(text.includes(ROUTE_CONTRACT_KEY), `${surface}: contract key`);
  assert.ok(text.includes(ROUTE_CONTRACT_PAYLOAD_SHA256), `${surface}: contract hash`);
}

console.log({ routeContractKey: ROUTE_CONTRACT_KEY,
  routeContractPayloadSha256: ROUTE_CONTRACT_PAYLOAD_SHA256,
  promptSha256: contract.promptSerialization.providerPromptSha256,
  vectorExecutionScopeHash: projected.executionScopeHash,
  vectorRoutePayloadSha256: projected.routePayloadSha256,
  providerCalled: false, submitCount: 0 });
console.log("generated media source-bound character edit route contract: PASS");
