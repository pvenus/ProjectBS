import fs from "node:fs";
import path from "node:path";
import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";

const repo = String.raw`C:\Users\parkv\.codex\worktrees\178d\ProjectBS-agent`;
const git = String.raw`C:\Users\parkv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\git\cmd\git.exe`;
const authorityMain = "59327a7213f34934b3f6843cc0a23af7ec12d131";
const handoffRel = "AgentDocs/planning-data/character/generated-media-handoffs/v2/character.seojin.2/gmplan2.character_single_image.character.seojin.2.12c3acc879fd35a0878b.character_single_image.json";
const handoffShaExpected = "ad845ed70ed8b3860014dcb54ad6b5514a1a3557168f98b74433658f6ef63f94";
const planningReceiptRel = "AgentDocs/planning-data/character/generated-media-handoffs/v2/character.seojin.2/gmplan2.character_single_image.character.seojin.2.12c3acc879fd35a0878b.planning-receipt.json";
const planningReceiptShaExpected = "e944475dfa7adb3ade329d6be4dc2b9da07b54dc625f99db942fa565435da07a";
const receiptRoot = String.raw`C:\github\ProjectBS-agent\output\generated-media-routing-receipts\character.seojin.2`;

function sha256Bytes(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function canonicalBytes(value) {
  return Buffer.from(`${canonicalJson(value)}\n`, "utf8");
}

function gitBytes(revision, rel) {
  return execFileSync(git, ["-c", `safe.directory=${repo}`, "-C", repo, "show", `${revision}:${rel}`], {
    encoding: null,
    maxBuffer: 64 * 1024 * 1024,
  });
}

function gitText(revision, rel) {
  return gitBytes(revision, rel).toString("utf8");
}

function readLocal(rel) {
  return fs.readFileSync(path.join(repo, ...rel.split("/")));
}

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

function assertCanonicalFile(bytes, object, label) {
  assert(bytes.equals(canonicalBytes(object)), `${label}_not_canonical_jcs_lf`);
}

function atomicNoClobber(filePath, bytes) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  if (fs.existsSync(filePath)) {
    const existing = fs.readFileSync(filePath);
    if (!existing.equals(bytes)) throw new Error(`no_clobber_collision:${filePath}`);
    return "reused_identical";
  }
  const temp = `${filePath}.tmp.${process.pid}.${Date.now()}`;
  let fd;
  try {
    fd = fs.openSync(temp, "wx", 0o644);
    fs.writeFileSync(fd, bytes);
    fs.fsyncSync(fd);
    fs.closeSync(fd);
    fd = undefined;
    fs.linkSync(temp, filePath);
  } finally {
    if (fd !== undefined) fs.closeSync(fd);
    if (fs.existsSync(temp)) fs.unlinkSync(temp);
  }
  return "created";
}

function sortAnchors(items) {
  return [...items].sort((left, right) =>
    Buffer.from(left.path).compare(Buffer.from(right.path)) ||
    Buffer.from(left.role).compare(Buffer.from(right.role)));
}

function rawMainAnchor(role, rel) {
  return { role, path: rel, sha256: sha256Bytes(gitBytes(authorityMain, rel)) };
}

// Live authority and immutable input checks.
const liveMain = execFileSync(git, ["-c", `safe.directory=${repo}`, "-C", repo, "rev-parse", "origin/main"], { encoding: "utf8" }).trim();
assert(liveMain === authorityMain, `authority_main_drift:${liveMain}`);

const handoffBytes = readLocal(handoffRel);
assert(sha256Bytes(handoffBytes) === handoffShaExpected, "planning_handoff_sha_mismatch");
const handoff = JSON.parse(handoffBytes.toString("utf8"));
assertCanonicalFile(handoffBytes, handoff, "planning_handoff");
assert(handoff.schemaVersion === "generated_media_planning_handoff_v2", "unsupported_handoff_schema");
assert(handoff.requestId === "gmplan2.character_single_image.character.seojin.2.12c3acc879fd35a0878b", "request_id_mismatch");
assert(handoff.assetType === "character_single_image" && handoff.domainType === "character" && handoff.contentId === "character.seojin.2", "routing_scope_mismatch");
assert(handoff.planningSnapshot.snapshotHash === "12c3acc879fd35a0878b7a88949f829186afa260c563a44193798e30e5464bd0", "planning_snapshot_mismatch");
assert(handoff.requestId === `gmplan2.${handoff.assetType}.${handoff.contentId}.${handoff.planningSnapshot.snapshotHash.slice(0, 20)}`, "request_derivation_mismatch");

const planningReceiptBytes = readLocal(planningReceiptRel);
assert(sha256Bytes(planningReceiptBytes) === planningReceiptShaExpected, "planning_receipt_sha_mismatch");
const planningReceipt = JSON.parse(planningReceiptBytes.toString("utf8"));
assertCanonicalFile(planningReceiptBytes, planningReceipt, "planning_receipt");
assert(planningReceipt.requestId === handoff.requestId && planningReceipt.snapshotHash === handoff.planningSnapshot.snapshotHash, "planning_receipt_identity_mismatch");
assert(planningReceipt.artifacts.handoff.sha256 === handoffShaExpected, "planning_receipt_handoff_binding_mismatch");

const snapshotPayload = {
  schemaVersion: "generated_media_planning_snapshot_hash_payload_v2",
  sourcePlanningFiles: structuredClone(handoff.sourcePlanningFiles),
  approvedFacts: structuredClone(handoff.planningSnapshot.approvedFacts),
};
assert(sha256Bytes(Buffer.from(canonicalJson(snapshotPayload), "utf8")) === handoff.planningSnapshot.snapshotHash, "planning_snapshot_hash_mismatch");

for (const source of handoff.sourcePlanningFiles) {
  const bytes = source.revision ? gitBytes(source.revision, source.path) : readLocal(source.path);
  assert(sha256Bytes(bytes) === source.sha256, `source_hash_mismatch:${source.path}`);
}

const identityAsset = handoff.identityConsistencyLock.identityAuthority;
const identityBytes = readLocal(identityAsset.path);
assert(sha256Bytes(identityBytes) === identityAsset.sha256, "identity_authority_sha_mismatch");

assert(handoff.expressionProfileKey === "projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0", "character_style_profile_conflict");
assert(handoff.expressionProfilePayloadHash === "b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a", "character_style_profile_conflict");
assert(handoff.baseExpressionProfileKey === "projectbs_character_open_ink_wash_dynamic_contour@2.0.0", "base_profile_mismatch");
assert(handoff.baseExpressionProfilePayloadHash === "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5", "base_profile_mismatch");
assert(handoff.singleImageSpecification.generationBackground.mode === "removable_solid" && handoff.singleImageSpecification.generationBackground.color === "#00FF00", "open_ink_chroma_master_contract_mismatch");
assert(!Object.hasOwn(handoff, "transparentForegroundSelection"), "open_ink_chroma_direct_alpha_conflict");
assert(handoff.opaqueChromaProviderMasterContract.backgroundFullyOpaque === true && handoff.opaqueChromaProviderMasterContract.providerTransparency === "prohibited", "open_ink_chroma_master_contract_mismatch");

const routingGuideRel = "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md";
const recordGuideRel = "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md";
const registryRel = "AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md";
const legacyRegistryRel = "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRoutingLegacyCompatibilityRegistry.json";
const opaqueGuideRel = "AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashOpaqueChromaSuccessorGuide.md";
const authoringPromptRel = "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md";
const generationPromptRel = "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md";
const routingPromptRel = "AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md";

const registryText = gitText(authorityMain, registryRel);
assert(registryText.includes("projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0") && registryText.includes(handoff.expressionProfilePayloadHash), "character_style_profile_conflict");
assert(registryText.includes("character_single_image_v2") && registryText.includes("character_single_image@2.0.0"), "unsupported_current_route");
const opaqueText = gitText(authorityMain, opaqueGuideRel);
assert(opaqueText.includes(handoff.expressionProfileKey) && opaqueText.includes(handoff.expressionProfilePayloadHash), "opaque_chroma_profile_authority_mismatch");
gitBytes(authorityMain, authoringPromptRel);
gitBytes(authorityMain, generationPromptRel);

// The published compatibility registry is closed/canonical and applies only to its exact G3 records.
const legacyBytes = gitBytes(authorityMain, legacyRegistryRel);
const legacy = JSON.parse(legacyBytes.toString("utf8"));
assertCanonicalFile(legacyBytes, legacy, "legacy_compatibility_registry");
assert(legacy.schemaVersion === "generated_media_routing_legacy_compatibility_registry_v1", "legacy_compatibility_registry_invalid");
for (const entry of Object.values(legacy.entries)) {
  assert(entry.compatibilityRule === "allow_exact_created_at_omission_only", "legacy_compatibility_registry_invalid");
  const indexBytes = gitBytes(authorityMain, entry.routingIndexPath);
  assert(sha256Bytes(indexBytes) === entry.initialIndexSha256, "legacy_initial_index_sha_mismatch");
  const index = JSON.parse(indexBytes.toString("utf8"));
  assertCanonicalFile(indexBytes, index, "legacy_routing_index");
  for (const legacyRecord of entry.legacyRecords) {
    const bytes = gitBytes(authorityMain, legacyRecord.recordPath);
    assert(sha256Bytes(bytes) === legacyRecord.recordSha256, "legacy_record_sha_mismatch");
    const record = JSON.parse(bytes.toString("utf8"));
    assertCanonicalFile(bytes, record, "legacy_routing_record");
    assert(!Object.hasOwn(record, "createdAt"), "legacy_created_at_presence_invalid");
    assert(record.routingRecordId === legacyRecord.routingRecordId && record.routingPayloadSha256 === legacyRecord.routingPayloadSha256, "legacy_record_identity_mismatch");
    const indexed = index.entries[legacyRecord.routingRecordId];
    assert(indexed && indexed.recordPath === legacyRecord.recordPath && indexed.recordSha256 === legacyRecord.recordSha256 && indexed.routingPayloadSha256 === legacyRecord.routingPayloadSha256, "legacy_index_projection_mismatch");
  }
}

const planningSnapshotHash = handoff.planningSnapshot.snapshotHash;
const typeSpecification = {
  identityConsistencyLock: structuredClone(handoff.identityConsistencyLock),
  singleImageSpecification: structuredClone(handoff.singleImageSpecification),
};
const normalizedRequest = {
  requestId: handoff.requestId,
  contentId: handoff.contentId,
  assetType: handoff.assetType,
  domainType: handoff.domainType,
  contentUsage: handoff.contentUsage,
  planningSnapshotHash,
  requiredElements: structuredClone(handoff.requiredElements),
  prohibitedElements: structuredClone(handoff.prohibitedElements),
  typeSpecification: structuredClone(typeSpecification),
};
const routeConstants = {
  registryVersion: "generated_media_authoring_profile_registry_v2",
  registryRowId: "character_single_image_v2",
  profileKey: "character_single_image@2.0.0",
  selectedPipeline: "imagegen_character_single_image",
  selectedAuthoringPrompt: authoringPromptRel,
  selectedGenerationPrompt: generationPromptRel,
  provider: "imagegen",
  structureProfile: "character_single_image_v2",
};
const prebindingHandoff = {
  planningHandoffPath: handoffRel,
  requestId: handoff.requestId,
  assetType: handoff.assetType,
  domainType: handoff.domainType,
  contentId: handoff.contentId,
  planningSnapshotHash,
  sourcePlanningFiles: structuredClone(handoff.sourcePlanningFiles),
  requiredElements: structuredClone(handoff.requiredElements),
  prohibitedElements: structuredClone(handoff.prohibitedElements),
  typeSpecification: structuredClone(typeSpecification),
  normalizedRequest: structuredClone(normalizedRequest),
  ...routeConstants,
};
const routingPayload = {
  schemaVersion: "generated_media_routing_hash_payload_v2",
  routerVersion: "generated_media_router_v2",
  ...routeConstants,
  requestId: handoff.requestId,
  assetType: handoff.assetType,
  domainType: handoff.domainType,
  contentId: handoff.contentId,
  planningHandoffPath: handoffRel,
  planningSnapshotHash,
  sourcePlanningFiles: structuredClone(handoff.sourcePlanningFiles),
  requiredElements: structuredClone(handoff.requiredElements),
  prohibitedElements: structuredClone(handoff.prohibitedElements),
  typeSpecification: structuredClone(typeSpecification),
  normalizedRequest: structuredClone(normalizedRequest),
  routingReason: {
    code: "exact_registry_row_match",
    registryRowId: routeConstants.registryRowId,
    profileKey: routeConstants.profileKey,
    matchedFields: {
      assetType: handoff.assetType,
      domainType: handoff.domainType,
      profileKey: routeConstants.profileKey,
    },
  },
  authoringHandoff: prebindingHandoff,
};

const routingPayloadSha256 = sha256Bytes(Buffer.from(canonicalJson(routingPayload), "utf8"));
const routingRecordId = `gmroute2.${handoff.assetType}.${handoff.contentId}.${routingPayloadSha256.slice(0, 20)}`;
const scopeRel = `AgentDocs/planning-data/generated-media-routing/v2/${handoff.assetType}/${handoff.contentId}`;
const routingRecordRel = `${scopeRel}/${routingRecordId}.json`;
const indexRel = `${scopeRel}/routing_index.json`;
const routingRecordAbs = path.join(repo, ...routingRecordRel.split("/"));
const indexAbs = path.join(repo, ...indexRel.split("/"));

const routingRecord = structuredClone(routingPayload);
routingRecord.schemaVersion = "generated_media_routing_v2";
routingRecord.routingRecordId = routingRecordId;
routingRecord.routingPayloadSha256 = routingPayloadSha256;
routingRecord.createdAt = handoff.planningSnapshot.capturedAt;
routingRecord.validation = {
  status: "valid",
  planningHandoff: "valid",
  sourceHashes: "valid",
  planningSnapshot: "valid",
  typeSpecification: "valid",
  registryMatchCount: 1,
  recordIdentity: "valid",
};
Object.assign(routingRecord.authoringHandoff, {
  routingRecordId,
  routingRecordPath: routingRecordRel,
  routingPayloadSha256,
  indexPath: indexRel,
});
const routingRecordBytes = canonicalBytes(routingRecord);
const routingRecordSha256 = sha256Bytes(routingRecordBytes);

// Reprojection check before mutation.
const reprojected = structuredClone(routingRecord);
delete reprojected.routingRecordId;
delete reprojected.routingPayloadSha256;
delete reprojected.createdAt;
delete reprojected.validation;
reprojected.schemaVersion = "generated_media_routing_hash_payload_v2";
for (const key of ["routingRecordId", "routingRecordPath", "routingPayloadSha256", "indexPath"]) delete reprojected.authoringHandoff[key];
assert(canonicalJson(reprojected) === canonicalJson(routingPayload), "routing_payload_reprojection_mismatch");

const indexEntry = {
  routingRecordId,
  recordSchemaVersion: "generated_media_routing_v2",
  recordPath: routingRecordRel,
  recordSha256: routingRecordSha256,
  routingPayloadSha256,
  requestId: handoff.requestId,
  assetType: handoff.assetType,
  domainType: handoff.domainType,
  contentId: handoff.contentId,
  planningSnapshotHash,
  registryVersion: routeConstants.registryVersion,
  registryRowId: routeConstants.registryRowId,
  profileKey: routeConstants.profileKey,
};

let existingIndexBytes = null;
let index;
if (fs.existsSync(indexAbs)) {
  existingIndexBytes = fs.readFileSync(indexAbs);
  index = JSON.parse(existingIndexBytes.toString("utf8"));
  assertCanonicalFile(existingIndexBytes, index, "routing_index");
  assert(index.schemaVersion === "generated_media_routing_index_v2" && index.assetType === handoff.assetType && index.contentId === handoff.contentId, "routing_index_scope_mismatch");
  for (const [key, value] of Object.entries(index.entries)) {
    assert(key === value.routingRecordId, "routing_index_entry_key_mismatch");
    const recordBytes = readLocal(value.recordPath);
    assert(sha256Bytes(recordBytes) === value.recordSha256, "routing_index_existing_record_sha_mismatch");
  }
  if (Object.hasOwn(index.entries, routingRecordId)) assert(canonicalJson(index.entries[routingRecordId]) === canonicalJson(indexEntry), "routing_index_write_failed");
} else {
  index = { schemaVersion: "generated_media_routing_index_v2", assetType: handoff.assetType, contentId: handoff.contentId, entries: {} };
}
index.entries[routingRecordId] = indexEntry;
const indexBytes = canonicalBytes(index);

// Record-first publication.
const recordWriteStatus = atomicNoClobber(routingRecordAbs, routingRecordBytes);
assert(fs.readFileSync(routingRecordAbs).equals(routingRecordBytes), "routing_record_reread_mismatch");

// Same-scope exclusive lock plus exact preimage CAS for the complete index.
fs.mkdirSync(path.dirname(indexAbs), { recursive: true });
const lockPath = `${indexAbs}.lock`;
let lockFd;
try {
  lockFd = fs.openSync(lockPath, "wx", 0o644);
  const currentExists = fs.existsSync(indexAbs);
  if (existingIndexBytes === null) assert(!currentExists, "routing_index_write_failed:cas_preimage_changed");
  else assert(currentExists && fs.readFileSync(indexAbs).equals(existingIndexBytes), "routing_index_write_failed:cas_preimage_changed");
  if (existingIndexBytes === null) {
    atomicNoClobber(indexAbs, indexBytes);
  } else if (!existingIndexBytes.equals(indexBytes)) {
    const temp = `${indexAbs}.tmp.${process.pid}.${Date.now()}`;
    const fd = fs.openSync(temp, "wx", 0o644);
    fs.writeFileSync(fd, indexBytes);
    fs.fsyncSync(fd);
    fs.closeSync(fd);
    assert(fs.readFileSync(indexAbs).equals(existingIndexBytes), "routing_index_write_failed:cas_preimage_changed");
    fs.renameSync(temp, indexAbs);
  }
} finally {
  if (lockFd !== undefined) fs.closeSync(lockFd);
  if (fs.existsSync(lockPath)) fs.unlinkSync(lockPath);
}
assert(fs.readFileSync(indexAbs).equals(indexBytes), "routing_index_reread_mismatch");
const indexSha256 = sha256Bytes(indexBytes);

// Detached control-plane receipt. It is outside the canonical routing scope.
const contractAuthorityAnchors = sortAnchors([
  rawMainAnchor("routing_contract", routingGuideRel),
  rawMainAnchor("routing_record_contract", recordGuideRel),
  rawMainAnchor("routing_prompt_contract", routingPromptRel),
  rawMainAnchor("routing_legacy_compatibility_registry", legacyRegistryRel),
  rawMainAnchor("character_image_authoring_prompt_contract", authoringPromptRel),
  rawMainAnchor("character_image_generation_prompt_contract", generationPromptRel),
]);
const profileAuthorityAnchors = sortAnchors([
  rawMainAnchor("character_profile_registry", registryRel),
  rawMainAnchor("opaque_chroma_successor_profile", opaqueGuideRel),
]);
const immutableArtifactAnchors = sortAnchors([
  { role: "planning_handoff", path: handoffRel, sha256: handoffShaExpected },
  { role: "planning_receipt", path: planningReceiptRel, sha256: planningReceiptShaExpected },
  ...handoff.sourcePlanningFiles.map((source) => ({ role: source.role, path: source.path, sha256: source.sha256 })),
  { role: "identity_authority", path: identityAsset.path, sha256: identityAsset.sha256 },
]);
const authorityPayload = {
  schemaVersion: "generated_media_authority_bundle_hash_payload_v1",
  authoritativeMainSha: authorityMain,
  requestedStageScope: ["routing", "authoring"],
  immutableArtifactAnchors,
  contractAuthorityAnchors,
  profileAuthorityAnchors,
};
const authorityBundleSha256 = sha256Bytes(Buffer.from(canonicalJson(authorityPayload), "utf8"));
const authorityBundleId = `gmauthbundle1.${authorityBundleSha256.slice(0, 20)}`;
const stagePayload = {
  schemaVersion: "generated_media_stage_delta_hash_payload_v1",
  authorityBundleId,
  authorityBundleSha256,
  fromStage: "routing",
  toStage: "authoring",
  unitIdentity: { requestId: handoff.requestId, assetType: handoff.assetType, domainType: handoff.domainType, contentId: handoff.contentId },
  newArtifacts: sortAnchors([
    { role: "routing_record", path: routingRecordRel, sha256: routingRecordSha256 },
    { role: "routing_index", path: indexRel, sha256: indexSha256 },
  ]),
  priorValidationReceiptRefs: [{ stage: "planning", receiptId: `${handoff.requestId}.planning-receipt`, receiptSha256: planningReceiptShaExpected }],
  publicationState: "local_unpublished",
  nextStep: "git_publication",
  providerState: { state: "not_called", providerCalled: false, submitCount: 0 },
  relayPolicy: "child_final_once_parent_next_role_once",
  observerPolicy: "compact_terminal_receipt_only",
};
const stageDeltaEnvelopeSha256 = sha256Bytes(Buffer.from(canonicalJson(stagePayload), "utf8"));
const stageDeltaEnvelopeId = `gmdelta1.routing.authoring.${stageDeltaEnvelopeSha256.slice(0, 20)}`;
const chainPayload = {
  schemaVersion: "generated_media_pipeline_receipt_chain_hash_payload_v1",
  authorityBundleId,
  authorityBundleSha256,
  unitIdentity: structuredClone(stagePayload.unitIdentity),
  stageEnvelopeRefs: [{ stageDeltaEnvelopeId, stageDeltaEnvelopeSha256 }],
};
const pipelineReceiptChainSha256 = sha256Bytes(Buffer.from(canonicalJson(chainPayload), "utf8"));
const pipelineReceiptChainId = `gmpipechain1.${pipelineReceiptChainSha256.slice(0, 20)}`;
const compactReceipt = {
  schemaVersion: "generated_media_routing_receipt_v1",
  status: "routed",
  reuseStatus: recordWriteStatus,
  validatedAuthorityRevision: authorityMain,
  routingRecordId,
  routingRecordPath: routingRecordRel,
  routingPayloadSha256,
  routingRecordSha256,
  indexPath: indexRel,
  indexSha256,
  authorityBundleId,
  authorityBundleSha256,
  stageDeltaEnvelopeId,
  stageDeltaEnvelopeSha256,
  pipelineReceiptChainId,
  pipelineReceiptChainSha256,
  authoringHandoffPointer: "/authoringHandoff",
  publicationState: "local_unpublished",
  nextStep: "git_publication",
  providerCalled: false,
};
const receiptBytes = canonicalBytes(compactReceipt);
const receiptAbs = path.join(receiptRoot, `${routingRecordId}.routing-receipt.json`);
const receiptWriteStatus = atomicNoClobber(receiptAbs, receiptBytes);
const receiptSha256 = sha256Bytes(receiptBytes);

// Immediate byte-identical reuse audit without touching canonical bytes.
assert(fs.readFileSync(routingRecordAbs).equals(routingRecordBytes), "routing_record_reuse_failed");
assert(fs.readFileSync(indexAbs).equals(indexBytes), "routing_index_reuse_failed");
assert(fs.readFileSync(receiptAbs).equals(receiptBytes), "routing_receipt_reuse_failed");

console.log(JSON.stringify({
  status: "routed_local_unpublished",
  canonicalTaskId: routingRecordId,
  requestId: handoff.requestId,
  routingRecordId,
  routingRecordPath: routingRecordAbs,
  routingRecordSha256,
  routingPayloadSha256,
  indexPath: indexAbs,
  indexSha256,
  authoringHandoffPointer: "/authoringHandoff",
  detachedReceiptPath: receiptAbs,
  detachedReceiptSha256: receiptSha256,
  recordWriteStatus,
  receiptWriteStatus,
  immediateReuseAudit: "reused_identical",
  validatedAuthorityRevision: authorityMain,
  planningHandoffSha256: handoffShaExpected,
  planningReceiptSha256: planningReceiptShaExpected,
  legacyCompatibilityRegistrySha256: sha256Bytes(legacyBytes),
  providerCalled: false,
  downstreamDispatched: false,
}, null, 2));
