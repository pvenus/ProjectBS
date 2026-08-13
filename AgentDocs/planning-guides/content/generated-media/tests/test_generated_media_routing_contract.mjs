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

const firstPayload = fixture();
const retryPayload = fixture();
assert.equal(routingId(firstPayload), routingId(retryPayload));
const firstBytes = recordBytes(firstPayload);
const retryBytes = requireByteIdenticalReuse(
  routingId(retryPayload), recordBytes(retryPayload), routingId(firstPayload), firstBytes,
);
assert.strictEqual(retryBytes, firstBytes);

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
console.log("generated media routing v2 contract vectors: PASS");
