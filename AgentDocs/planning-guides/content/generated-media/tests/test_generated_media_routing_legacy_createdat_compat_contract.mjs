import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

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

function assertClosedKeys(value, required) {
  assert.deepEqual(Object.keys(value).sort(), [...required].sort());
}

const registryBytes = readFileSync(new URL(
  "../GeneratedMediaRoutingLegacyCompatibilityRegistry.json", import.meta.url));
const registry = JSON.parse(registryBytes);
assert.equal(registryBytes.equals(Buffer.from(`${canonicalJson(registry)}\n`, "utf8")), true);
assertClosedKeys(registry, ["schemaVersion", "entries"]);
assert.equal(registry.schemaVersion,
  "generated_media_routing_legacy_compatibility_registry_v1");

const compatibility = registry.entries[
  "character_single_image.character.seojin.3.pre_created_at.v1"];
assertClosedKeys(compatibility, [
  "assetType", "compatibilityId", "compatibilityRule", "contentId",
  "decisionBaseMainSha", "initialIndexSha256", "legacyRecords",
  "omittedRequiredMember", "recordSchemaVersion", "routingIndexPath",
]);
assert.equal(compatibility.decisionBaseMainSha,
  "9afb4131e837c88b44ec769705fa075912025c68");
assert.equal(compatibility.initialIndexSha256,
  "189b518eb5ccc209f45c7b6c98c80fcaa467754f61799163dc156537d7c6cf2b");
assert.equal(compatibility.omittedRequiredMember, "createdAt");
assert.equal(compatibility.compatibilityRule,
  "allow_exact_created_at_omission_only");
assert.equal(compatibility.legacyRecords.length, 4);
assert.deepEqual(compatibility.legacyRecords.map((record) => record.recordSha256), [
  "97c89bd59b51445206c22fbc5255a3a24fc54d1cc7e4bc282bdee5ffe7f7e78e",
  "94a7a2778882e57a7e98e6642fc7e59cbfc43db859b03254aa40fe18a1302863",
  "96777e7111cf853ae46a07ba5632d0ab19afd1d1478eb13c351647a52a2f843d",
  "65994f23f69305ba86910c017d0f9523f0ea8d7affd9030080491dd14c20055c",
]);
for (const record of compatibility.legacyRecords) {
  assertClosedKeys(record, [
    "recordPath", "recordSha256", "routingPayloadSha256", "routingRecordId",
  ]);
}

const currentRequiredKeys = [
  "assetType", "authoringHandoff", "contentId", "createdAt", "domainType",
  "normalizedRequest", "planningHandoffPath", "planningSnapshotHash", "profileKey",
  "prohibitedElements", "provider", "registryRowId", "registryVersion", "requestId",
  "requiredElements", "routerVersion", "routingPayloadSha256", "routingReason",
  "routingRecordId", "schemaVersion", "selectedAuthoringPrompt",
  "selectedGenerationPrompt", "selectedPipeline", "sourcePlanningFiles",
  "structureProfile", "typeSpecification", "validation",
];

function currentRecord(overrides = {}) {
  return {
    assetType: "character_single_image",
    authoringHandoff: {},
    contentId: "character.seojin.3",
    createdAt: "2026-08-22T00:00:00+09:00",
    domainType: "character",
    normalizedRequest: {},
    planningHandoffPath: "AgentDocs/planning-data/example.json",
    planningSnapshotHash: "0".repeat(64),
    profileKey: "character_single_image@2.0.0",
    prohibitedElements: ["text"],
    provider: "imagegen",
    registryRowId: "character_single_image_v2",
    registryVersion: "generated_media_authoring_profile_registry_v2",
    requestId: "request.current",
    requiredElements: ["character"],
    routerVersion: "generated_media_router_v2",
    routingPayloadSha256: "1".repeat(64),
    routingReason: {},
    routingRecordId: "gmroute2.character_single_image.character.seojin.3.current",
    schemaVersion: "generated_media_routing_v2",
    selectedAuthoringPrompt: "authoring.md",
    selectedGenerationPrompt: "generation.md",
    selectedPipeline: "imagegen_character_single_image",
    sourcePlanningFiles: [],
    structureProfile: "character_single_image_v2",
    typeSpecification: {},
    validation: {},
    ...overrides,
  };
}

function validateCurrent(record) {
  assertClosedKeys(record, currentRequiredKeys);
  assert.match(record.createdAt,
    /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:Z|[+-]\d{2}:\d{2})$/);
}

function validateLegacy(observed) {
  const matches = compatibility.legacyRecords.filter((candidate) =>
    observed.record.schemaVersion === compatibility.recordSchemaVersion &&
    observed.record.assetType === compatibility.assetType &&
    observed.record.contentId === compatibility.contentId &&
    observed.routingIndexPath === compatibility.routingIndexPath &&
    observed.record.routingRecordId === candidate.routingRecordId &&
    observed.recordPath === candidate.recordPath &&
    observed.recordSha256 === candidate.recordSha256 &&
    observed.record.routingPayloadSha256 === candidate.routingPayloadSha256);
  assert.equal(matches.length, 1);
  assert.equal(Object.hasOwn(observed.record, "createdAt"), false);
  assertClosedKeys(observed.record,
    currentRequiredKeys.filter((key) => key !== "createdAt"));
  const expected = matches[0];
  assert.deepEqual(observed.indexEntry, {
    recordPath: expected.recordPath,
    recordSha256: expected.recordSha256,
    routingPayloadSha256: expected.routingPayloadSha256,
    routingRecordId: expected.routingRecordId,
  });
  if (observed.indexEntryCount === compatibility.legacyRecords.length) {
    assert.equal(observed.indexSha256, compatibility.initialIndexSha256);
  } else {
    assert.ok(observed.indexEntryCount > compatibility.legacyRecords.length);
    observed.additionalRecords.forEach(validateCurrent);
  }
}

const first = compatibility.legacyRecords[0];
const legacy = currentRecord({
  routingRecordId: first.routingRecordId,
  routingPayloadSha256: first.routingPayloadSha256,
});
delete legacy.createdAt;
const exactObservation = {
  record: legacy,
  recordPath: first.recordPath,
  recordSha256: first.recordSha256,
  routingIndexPath: compatibility.routingIndexPath,
  indexSha256: compatibility.initialIndexSha256,
  indexEntryCount: 4,
  additionalRecords: [],
  indexEntry: {
    routingRecordId: first.routingRecordId,
    recordPath: first.recordPath,
    recordSha256: first.recordSha256,
    routingPayloadSha256: first.routingPayloadSha256,
  },
};
validateLegacy(exactObservation);

assert.throws(() => validateLegacy({ ...exactObservation,
  recordSha256: sha256(Buffer.from("changed")) }));
assert.throws(() => validateLegacy({ ...exactObservation,
  record: { ...legacy, createdAt: "2026-08-22T00:00:00+09:00" } }));
const missingProvider = structuredClone(legacy);
delete missingProvider.provider;
assert.throws(() => validateLegacy({ ...exactObservation, record: missingProvider }));
assert.throws(() => validateLegacy({ ...exactObservation,
  indexSha256: "0".repeat(64) }));
assert.throws(() => validateLegacy({ ...exactObservation,
  indexEntry: { ...exactObservation.indexEntry, recordSha256: "0".repeat(64) } }));

validateLegacy({
  ...exactObservation,
  indexSha256: "2".repeat(64),
  indexEntryCount: 5,
  additionalRecords: [currentRecord()],
});
const unregisteredAdditional = currentRecord();
delete unregisteredAdditional.createdAt;
assert.throws(() => validateLegacy({
  ...exactObservation,
  indexSha256: "2".repeat(64),
  indexEntryCount: 5,
  additionalRecords: [unregisteredAdditional],
}));

for (const path of [
  "../GeneratedMediaRecordGuide.md",
  "../GeneratedMediaRequestRoutingGuide.md",
  "../../../../task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md",
]) {
  const text = readFileSync(new URL(path, import.meta.url), "utf8");
  assert.match(text, /GeneratedMediaRoutingLegacyCompatibilityRegistry\.json/);
  assert.match(text, /createdAt/);
  assert.match(text, /complete-scope|complete.scope/);
}

console.log("generated media routing legacy createdAt compatibility: PASS");
