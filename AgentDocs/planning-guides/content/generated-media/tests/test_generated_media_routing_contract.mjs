// Executable contract vectors for Generated Media routing v2.
// The fixture uses only JSON values whose JSON.stringify form is RFC 8785 JCS.
// This is a test vector, not a general JCS implementation or routing writer.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function payloadHash(payload) {
  return createHash("sha256").update(canonicalJson(payload), "utf8").digest("hex");
}

function routingId(payload) {
  const prefix = payloadHash(payload).slice(0, 20);
  return payload.assetType === "animation"
    ? `gmroute2.animation.${payload.contentId}.${payload.animationRequestId}.${prefix}`
    : `gmroute2.${payload.assetType}.${payload.contentId}.${prefix}`;
}

function fixture() {
  const payload = {
    schemaVersion: "generated_media_routing_hash_payload_v2",
    routerVersion: "generated_media_router_v2",
    registryVersion: "generated_media_authoring_profile_registry_v2",
    registryRowId: "character_single_image_v2",
    profileKey: "character_single_image@2.0.0",
    requestId: "req.contract.vector.001",
    assetType: "character_single_image",
    domainType: "character",
    contentId: "character.contract_vector",
    planningHandoffPath: "AgentDocs/planning-data/generated-media-planning/v2/character.contract_vector.json",
    planningSnapshotHash: "0".repeat(64),
    sourcePlanningFiles: [{
      path: "AgentDocs/planning-data/characters/contract_vector.json",
      role: "approved_character_plan",
      sha256: "1".repeat(64),
    }],
    requiredElements: ["one full character"],
    prohibitedElements: ["text"],
    typeSpecification: {
      identityConsistencyLock: {
        identityId: "character.contract_vector",
        referenceFacts: ["approved identity"],
      },
      singleImageSpecification: {
        viewpoint: "front",
        pose: "neutral",
        framing: "full_body",
        canvas: { width: 1024, height: 1024 },
        targetDisplaySize: { width: 256, height: 256 },
        safeArea: "all subject pixels inside 16px inset",
        finalBackgroundPolicy: "transparent",
        generationBackground: { mode: "removable_solid", color: "#00ff00" },
        noShadow: true,
        outline: { enabled: false },
        anchor: {
          type: "pelvis_root_ground_axis",
          pelvisOrRootPoint: "canvas_center_x",
          groundContactAxis: "bottom_safe_area",
        },
      },
    },
    normalizedRequest: {
      requestId: "req.contract.vector.001",
      contentId: "character.contract_vector",
      assetType: "character_single_image",
      domainType: "character",
      contentUsage: "contract_test_only",
      planningSnapshotHash: "0".repeat(64),
      requiredElements: ["one full character"],
      prohibitedElements: ["text"],
      typeSpecification: null,
    },
    selectedPipeline: "imagegen_character_single_image",
    selectedAuthoringPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md",
    selectedGenerationPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md",
    provider: "imagegen",
    structureProfile: "character_single_image_v2",
    routingReason: {
      code: "exact_registry_row_match",
      registryRowId: "character_single_image_v2",
      profileKey: "character_single_image@2.0.0",
      matchedFields: {
        assetType: "character_single_image",
        domainType: "character",
        profileKey: "character_single_image@2.0.0",
      },
    },
    authoringHandoff: {
      planningHandoffPath: "AgentDocs/planning-data/generated-media-planning/v2/character.contract_vector.json",
      requestId: "req.contract.vector.001",
      assetType: "character_single_image",
      domainType: "character",
      contentId: "character.contract_vector",
      planningSnapshotHash: "0".repeat(64),
      sourcePlanningFiles: null,
      requiredElements: ["one full character"],
      prohibitedElements: ["text"],
      typeSpecification: null,
      normalizedRequest: null,
      registryVersion: "generated_media_authoring_profile_registry_v2",
      registryRowId: "character_single_image_v2",
      profileKey: "character_single_image@2.0.0",
      selectedPipeline: "imagegen_character_single_image",
      selectedAuthoringPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md",
      selectedGenerationPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md",
      provider: "imagegen",
      structureProfile: "character_single_image_v2",
    },
  };
  payload.normalizedRequest.typeSpecification = structuredClone(payload.typeSpecification);
  payload.authoringHandoff.sourcePlanningFiles = structuredClone(payload.sourcePlanningFiles);
  payload.authoringHandoff.typeSpecification = structuredClone(payload.typeSpecification);
  payload.authoringHandoff.normalizedRequest = structuredClone(payload.normalizedRequest);
  return payload;
}

function recordBytes(payload) {
  const digest = payloadHash(payload);
  const id = routingId(payload);
  const record = structuredClone(payload);
  record.schemaVersion = "generated_media_routing_v2";
  record.routingRecordId = id;
  record.routingPayloadSha256 = digest;
  record.createdAt = "2026-08-13T00:00:00Z";
  record.validation = { status: "valid" };
  Object.assign(record.authoringHandoff, {
    routingRecordId: id,
    routingRecordPath: `AgentDocs/planning-data/generated-media-routing/v2/character_single_image/character.contract_vector/${id}.json`,
    routingPayloadSha256: digest,
    indexPath: "AgentDocs/planning-data/generated-media-routing/v2/character_single_image/character.contract_vector/routing_index.json",
  });
  return Buffer.from(`${canonicalJson(record)}\n`, "utf8");
}

function requireByteIdenticalReuse(expectedId, expectedBytes, occupiedId, occupiedBytes) {
  if (occupiedId !== expectedId || !occupiedBytes.equals(expectedBytes)) {
    throw new Error("routing_record_collision");
  }
  return occupiedBytes;
}

function sha256Canonical(value) {
  return createHash("sha256").update(canonicalJson(value), "utf8").digest("hex");
}

function validateAndProjectStyleBinding(payload) {
  const bindings = payload.styleReferenceBindings;
  if (!Array.isArray(bindings) || bindings.length !== 1) {
    throw new Error("style_reference_binding_incomplete");
  }
  const keys = ["role", "projectRelativePath", "sha256", "reviewRecordId",
    "reviewRecordPath", "reviewRecordSha256"];
  if (Object.keys(bindings[0]).length !== keys.length ||
      keys.some((key) => !Object.hasOwn(bindings[0], key))) {
    throw new Error("style_reference_binding_incomplete");
  }
  if (bindings[0].role !== "style_only") throw new Error("style_reference_role_invalid");
  for (const consumer of [payload.normalizedRequest, payload.authoringHandoff]) {
    if (canonicalJson(consumer.styleReferenceBindings) !== canonicalJson(bindings)) {
      throw new Error("style_reference_binding_scope_mismatch");
    }
  }
  return true;
}

function authorityBundle(anchorSha = "1".repeat(64)) {
  const payload = {
    schemaVersion: "generated_media_authority_bundle_hash_payload_v1",
    authoritativeMainSha: "a".repeat(40),
    requestedStageScope: ["routing", "authoring"],
    immutableArtifactAnchors: [{
      role: "planning_handoff",
      path: "AgentDocs/planning-data/generated-media-planning/v2/character.contract_vector.json",
      sha256: anchorSha,
    }],
    contractAuthorityAnchors: [{
      role: "routing_contract",
      path: "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md",
      sha256: "2".repeat(64),
    }],
    profileAuthorityAnchors: [{
      role: "character_profile_registry",
      path: "AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md",
      sha256: "3".repeat(64),
    }],
  };
  const authorityBundleSha256 = sha256Canonical(payload);
  return {
    ...payload,
    schemaVersion: "generated_media_authority_bundle_receipt_v1",
    authorityBundleId: `gmauthbundle1.${authorityBundleSha256.slice(0, 20)}`,
    authorityBundleSha256,
  };
}

function canReuseAuthorityBundle(previous, current) {
  return previous.authorityBundleId === current.authorityBundleId &&
    previous.authorityBundleSha256 === current.authorityBundleSha256 &&
    canonicalJson(previous) === canonicalJson(current);
}

function validateAuthorityBundle(receipt) {
  const payload = structuredClone(receipt);
  delete payload.authorityBundleId;
  delete payload.authorityBundleSha256;
  payload.schemaVersion = "generated_media_authority_bundle_hash_payload_v1";
  const expectedSha256 = sha256Canonical(payload);
  assert.equal(receipt.authorityBundleSha256, expectedSha256);
  assert.equal(receipt.authorityBundleId,
    `gmauthbundle1.${expectedSha256.slice(0, 20)}`);
}

const transitions = new Set([
  "planning->routing", "routing->authoring", "authoring->generation",
  "generation->preservation", "generation->preview_terminal",
  "preservation->evaluation_package", "evaluation_package->terminal",
]);
const forbiddenBulkKeys = new Set([
  "normalizedRequest", "sourcePlanningFiles", "requiredElements",
  "prohibitedElements", "typeSpecification", "approvedFacts",
  "expressionProfilePayload", "negativeStyleLock", "positiveStyleLock",
  "authoringHandoff", "generationHandoff", "preservationHandoff",
]);

function containsForbiddenBulk(value) {
  if (Array.isArray(value)) return value.some(containsForbiddenBulk);
  if (value === null || typeof value !== "object") return false;
  return Object.entries(value).some(([key, nested]) =>
    forbiddenBulkKeys.has(key) || containsForbiddenBulk(nested));
}

function stageDelta(bundle, overrides = {}) {
  const payload = {
    schemaVersion: "generated_media_stage_delta_hash_payload_v1",
    authorityBundleId: bundle.authorityBundleId,
    authorityBundleSha256: bundle.authorityBundleSha256,
    fromStage: "routing",
    toStage: "authoring",
    unitIdentity: {
      requestId: "req.contract.vector.001",
      assetType: "character_single_image",
      domainType: "character",
      contentId: "character.contract_vector",
    },
    newArtifacts: [{
      role: "routing_record",
      path: "AgentDocs/planning-data/generated-media-routing/v2/character_single_image/character.contract_vector/record.json",
      sha256: "4".repeat(64),
    }],
    priorValidationReceiptRefs: [{
      stage: "planning",
      receiptId: "planning.receipt.contract_vector",
      receiptSha256: "5".repeat(64),
    }],
    publicationState: "local_unpublished",
    nextStep: "git_publication",
    providerState: { state: "not_called", providerCalled: false, submitCount: 0 },
    relayPolicy: "child_final_once_parent_next_role_once",
    observerPolicy: "compact_terminal_receipt_only",
    ...overrides,
  };
  const stageDeltaEnvelopeSha256 = sha256Canonical(payload);
  return {
    ...payload,
    schemaVersion: "generated_media_stage_delta_envelope_v1",
    stageDeltaEnvelopeId: `gmdelta1.${payload.fromStage}.${payload.toStage}.${stageDeltaEnvelopeSha256.slice(0, 20)}`,
    stageDeltaEnvelopeSha256,
  };
}

function validateStageDelta(envelope) {
  if (containsForbiddenBulk(envelope)) throw new Error("forbidden_bulk_field");
  if (!transitions.has(`${envelope.fromStage}->${envelope.toStage}`))
    throw new Error("invalid_stage_transition");
  if ((envelope.publicationState === "local_unpublished" && envelope.nextStep !== "git_publication") ||
      (envelope.publicationState === "authoritative_git_blob" && envelope.nextStep !== envelope.toStage))
    throw new Error("invalid_publication_transition_pair");
  if (envelope.providerState.state === "not_called" &&
      (envelope.providerState.providerCalled !== false || envelope.providerState.submitCount !== 0))
    throw new Error("invalid_provider_state");
  if (envelope.providerState.state !== "not_called" &&
      (envelope.providerState.providerCalled !== true || envelope.providerState.submitCount < 1))
    throw new Error("invalid_provider_state");
  const payload = structuredClone(envelope);
  delete payload.stageDeltaEnvelopeId;
  delete payload.stageDeltaEnvelopeSha256;
  payload.schemaVersion = "generated_media_stage_delta_hash_payload_v1";
  const expectedSha256 = sha256Canonical(payload);
  assert.equal(expectedSha256, envelope.stageDeltaEnvelopeSha256);
  assert.equal(envelope.stageDeltaEnvelopeId,
    `gmdelta1.${envelope.fromStage}.${envelope.toStage}.${expectedSha256.slice(0, 20)}`);
}

function pipelineChain(bundle, unitIdentity, stageEnvelopes) {
  const payload = {
    schemaVersion: "generated_media_pipeline_receipt_chain_hash_payload_v1",
    authorityBundleId: bundle.authorityBundleId,
    authorityBundleSha256: bundle.authorityBundleSha256,
    unitIdentity: structuredClone(unitIdentity),
    stageEnvelopeRefs: stageEnvelopes.map((envelope) => ({
      stageDeltaEnvelopeId: envelope.stageDeltaEnvelopeId,
      stageDeltaEnvelopeSha256: envelope.stageDeltaEnvelopeSha256,
    })),
  };
  assert.equal(new Set(payload.stageEnvelopeRefs.map((ref) => ref.stageDeltaEnvelopeId)).size,
    payload.stageEnvelopeRefs.length);
  const pipelineReceiptChainSha256 = sha256Canonical(payload);
  return {
    ...payload,
    schemaVersion: "generated_media_pipeline_receipt_chain_v1",
    pipelineReceiptChainId: `gmpipechain1.${pipelineReceiptChainSha256.slice(0, 20)}`,
    pipelineReceiptChainSha256,
  };
}

function compactStatus(chain, stageEnvelope, state = "routed") {
  const payload = {
    schemaVersion: "generated_media_compact_status_hash_payload_v1",
    pipelineReceiptChainId: chain.pipelineReceiptChainId,
    pipelineReceiptChainSha256: chain.pipelineReceiptChainSha256,
    stage: stageEnvelope.fromStage,
    state,
    stageReceiptId: stageEnvelope.stageDeltaEnvelopeId,
    stageReceiptSha256: stageEnvelope.stageDeltaEnvelopeSha256,
    publicationState: stageEnvelope.publicationState,
    providerState: structuredClone(stageEnvelope.providerState),
  };
  const statusReceiptSha256 = sha256Canonical(payload);
  return {
    ...payload,
    schemaVersion: "generated_media_compact_status_v1",
    statusReceiptId: `gmstatus1.${payload.stage}.${statusReceiptSha256.slice(0, 20)}`,
    statusReceiptSha256,
  };
}

function relayOnce(receiptId, delivered) {
  if (delivered.has(receiptId)) throw new Error("duplicate_control_plane_relay");
  delivered.add(receiptId);
}

function compactReceipt(payload, bundle, delta, chain, reuseStatus = "created") {
  const digest = payloadHash(payload);
  const id = routingId(payload);
  const routingRecordPath = `AgentDocs/planning-data/generated-media-routing/v2/${payload.assetType}/${payload.contentId}/${id}.json`;
  const routingRecordBytes = recordBytes(payload);
  return {
    schemaVersion: "generated_media_routing_receipt_v1",
    status: "routed",
    reuseStatus,
    validatedAuthorityRevision: "a".repeat(40),
    routingRecordId: id,
    routingRecordPath,
    routingPayloadSha256: digest,
    routingRecordSha256: createHash("sha256").update(routingRecordBytes).digest("hex"),
    indexPath: `AgentDocs/planning-data/generated-media-routing/v2/${payload.assetType}/${payload.contentId}/routing_index.json`,
    indexSha256: "b".repeat(64),
    authorityBundleId: bundle.authorityBundleId,
    authorityBundleSha256: bundle.authorityBundleSha256,
    stageDeltaEnvelopeId: delta.stageDeltaEnvelopeId,
    stageDeltaEnvelopeSha256: delta.stageDeltaEnvelopeSha256,
    pipelineReceiptChainId: chain.pipelineReceiptChainId,
    pipelineReceiptChainSha256: chain.pipelineReceiptChainSha256,
    authoringHandoffPointer: "/authoringHandoff",
    publicationState: "local_unpublished",
    nextStep: "git_publication",
    providerCalled: false,
  };
}

function validateCompactReceipt(receipt) {
  const keys = [
    "authorityBundleId", "authorityBundleSha256", "authoringHandoffPointer",
    "indexPath", "indexSha256", "nextStep", "pipelineReceiptChainId",
    "pipelineReceiptChainSha256", "providerCalled", "publicationState",
    "reuseStatus", "routingPayloadSha256", "routingRecordId",
    "routingRecordPath", "routingRecordSha256", "schemaVersion",
    "stageDeltaEnvelopeId", "stageDeltaEnvelopeSha256", "status",
    "validatedAuthorityRevision",
  ];
  assert.deepEqual(Object.keys(receipt).sort(), keys.sort());
  assert.match(receipt.validatedAuthorityRevision, /^[0-9a-f]{40}$/);
  for (const key of [
    "routingPayloadSha256", "routingRecordSha256", "indexSha256",
    "authorityBundleSha256", "stageDeltaEnvelopeSha256",
    "pipelineReceiptChainSha256",
  ])
    assert.match(receipt[key], /^[0-9a-f]{64}$/);
  if ((receipt.publicationState === "local_unpublished" && receipt.nextStep !== "git_publication") ||
      (receipt.publicationState === "authoritative_git_blob" && receipt.nextStep !== "authoring"))
    throw new Error("compact_receipt_state_mismatch");
  assert.equal(receipt.authoringHandoffPointer, "/authoringHandoff");
  assert.equal(receipt.providerCalled, false);
  assert.equal(containsForbiddenBulk(receipt), false);
}

const firstPayload = fixture();
const retryPayload = fixture();
assert.equal(routingId(firstPayload), routingId(retryPayload));
const firstBytes = recordBytes(firstPayload);
const retryBytes = requireByteIdenticalReuse(
  routingId(retryPayload), recordBytes(retryPayload), routingId(firstPayload), firstBytes,
);
assert.strictEqual(retryBytes, firstBytes);

const durablePayload = fixture();
durablePayload.styleReferenceBindings = [{
  role: "style_only",
  projectRelativePath: "AgentDocs/reference-assets/generated-media/style-only/character_single_image/open_ink_wash_dynamic_contour/b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf.png",
  sha256: "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf",
  reviewRecordId: "gmstyleref1.character_single_image.open_ink_wash_dynamic_contour.d6dae45a8f8f6591b5cb",
  reviewRecordPath: "AgentDocs/planning-data/style-reference-reviews/v1/character_single_image/open_ink_wash_dynamic_contour/gmstyleref1.character_single_image.open_ink_wash_dynamic_contour.d6dae45a8f8f6591b5cb.json",
  reviewRecordSha256: "51630e6c2c4ec80caae9bf5c995f7673e2b8fddf83870c5a28452971fa2be4c2",
}];
durablePayload.normalizedRequest.styleReferenceBindings =
  structuredClone(durablePayload.styleReferenceBindings);
durablePayload.authoringHandoff.styleReferenceBindings =
  structuredClone(durablePayload.styleReferenceBindings);
assert.equal(validateAndProjectStyleBinding(durablePayload), true);
const droppedBinding = structuredClone(durablePayload);
delete droppedBinding.authoringHandoff.styleReferenceBindings;
assert.throws(() => validateAndProjectStyleBinding(droppedBinding),
  /style_reference_binding_scope_mismatch/);
const incompleteBinding = structuredClone(durablePayload);
delete incompleteBinding.styleReferenceBindings[0].reviewRecordSha256;
assert.throws(() => validateAndProjectStyleBinding(incompleteBinding),
  /style_reference_binding_incomplete/);

const changed = structuredClone(firstPayload);
changed.requiredElements.push("visible hands");
assert.notEqual(payloadHash(firstPayload), payloadHash(changed));
assert.notEqual(routingId(firstPayload), routingId(changed));

assert.throws(
  () => requireByteIdenticalReuse(
    routingId(firstPayload), firstBytes, routingId(firstPayload), Buffer.concat([firstBytes, Buffer.from(" ")]),
  ),
  /routing_record_collision/,
);

assert.equal(payloadHash(firstPayload), "64d413124b79a587833cd316cf18ae7f373ebfe53d605de3d84ce0498b4c0788");
assert.equal(routingId(firstPayload), "gmroute2.character_single_image.character.contract_vector.64d413124b79a587833c");

const firstBundle = authorityBundle();
const retryBundle = authorityBundle();
validateAuthorityBundle(firstBundle);
validateAuthorityBundle(retryBundle);
assert.equal(canonicalJson(firstBundle), canonicalJson(retryBundle));
assert.equal(canReuseAuthorityBundle(firstBundle, retryBundle), true);

const changedBundle = authorityBundle("9".repeat(64));
validateAuthorityBundle(changedBundle);
assert.notEqual(changedBundle.authorityBundleSha256, firstBundle.authorityBundleSha256);
assert.notEqual(changedBundle.authorityBundleId, firstBundle.authorityBundleId);
assert.equal(canReuseAuthorityBundle(firstBundle, changedBundle), false);

const firstDelta = stageDelta(firstBundle);
const retryDelta = stageDelta(retryBundle);
validateStageDelta(firstDelta);
validateStageDelta(retryDelta);
assert.equal(canonicalJson(firstDelta), canonicalJson(retryDelta));

const forbiddenDelta = structuredClone(firstDelta);
forbiddenDelta.normalizedRequest = { requestId: firstPayload.requestId };
assert.throws(() => validateStageDelta(forbiddenDelta), /forbidden_bulk_field/);
const skippedStage = stageDelta(firstBundle, { toStage: "generation" });
assert.throws(() => validateStageDelta(skippedStage), /invalid_stage_transition/);
const unpublishedAuthoring = stageDelta(firstBundle, { nextStep: "authoring" });
assert.throws(() => validateStageDelta(unpublishedAuthoring), /invalid_publication_transition_pair/);
const contradictoryProviderState = stageDelta(firstBundle, {
  providerState: { state: "not_called", providerCalled: true, submitCount: 1 },
});
assert.throws(() => validateStageDelta(contradictoryProviderState), /invalid_provider_state/);

const firstChain = pipelineChain(firstBundle, firstDelta.unitIdentity, [firstDelta]);
const retryChain = pipelineChain(retryBundle, retryDelta.unitIdentity, [retryDelta]);
assert.equal(canonicalJson(firstChain), canonicalJson(retryChain));
for (const forbiddenPathKey of ["path", "recordPath", "indexPath", "orchestrationRecordPath"])
  assert.equal(canonicalJson(firstChain).includes(`\"${forbiddenPathKey}\"`), false);

const deliveredEnvelopes = new Set();
relayOnce(firstDelta.stageDeltaEnvelopeId, deliveredEnvelopes);
assert.throws(() => relayOnce(firstDelta.stageDeltaEnvelopeId, deliveredEnvelopes),
  /duplicate_control_plane_relay/);
const firstStatus = compactStatus(firstChain, firstDelta);
const emittedStatuses = new Set();
relayOnce(firstStatus.statusReceiptId, emittedStatuses);
assert.throws(() => relayOnce(firstStatus.statusReceiptId, emittedStatuses),
  /duplicate_control_plane_relay/);

const receipt = compactReceipt(firstPayload, firstBundle, firstDelta, firstChain);
validateCompactReceipt(receipt);
assert.equal(receipt.publicationState, "local_unpublished");
assert.equal(receipt.nextStep, "git_publication");
assert.ok(Buffer.byteLength(canonicalJson(receipt), "utf8") <
  Buffer.byteLength(canonicalJson(firstPayload.authoringHandoff), "utf8"));
const invalidReceipt = structuredClone(receipt);
invalidReceipt.nextStep = "authoring";
assert.throws(() => validateCompactReceipt(invalidReceipt), /compact_receipt_state_mismatch/);
console.log("generated media routing v2 contract vectors: PASS");
