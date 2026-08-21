// Exact compatibility vectors for occupied routing v2 indexes.
// These fixtures reconstruct the observed canonical index bytes without copying
// or changing any routing artifact.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const here = dirname(fileURLToPath(import.meta.url));
const registryPath = join(here, "..", "GeneratedMediaRoutingLegacyCompatibilityRegistry.json");
const registryBytes = readFileSync(registryPath);
const registry = JSON.parse(registryBytes.toString("utf8"));

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") return `{${Object.keys(value).sort().map(
    (key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function entry(contentId, idSuffix, snapshotHash, recordSha256, requestSuffix) {
  const routingRecordId = `gmroute2.character_single_image.${contentId}.${idSuffix.slice(0, 20)}`;
  return {
    assetType: "character_single_image",
    contentId,
    domainType: "character",
    planningSnapshotHash: snapshotHash,
    profileKey: "character_single_image@2.0.0",
    recordPath: `AgentDocs/planning-data/generated-media-routing/v2/character_single_image/${contentId}/${routingRecordId}.json`,
    recordSchemaVersion: "generated_media_routing_v2",
    recordSha256,
    registryRowId: "character_single_image_v2",
    registryVersion: "generated_media_authoring_profile_registry_v2",
    requestId: `gmplan2.character_single_image.${contentId}.${requestSuffix}`,
    routingPayloadSha256: idSuffix,
    routingRecordId,
  };
}

function occupiedIndex(contentId, rows) {
  return {
    assetType: "character_single_image",
    contentId,
    entries: Object.fromEntries(rows.map((row) => [row.routingRecordId, row])),
    schemaVersion: "generated_media_routing_index_v2",
  };
}

const grade2Rows = [
  entry("character.seojin.2", "2fda768ebb5816f8fa917a1b26143c193ba42b31f45843d97a4a095e05ef896a", "b7476e702e9816e3c644c24e08ef22113acbe7206cb62505a6e60a200cd0f269", "5317d8560f1bcb5e1d8541ce470c62b67b71bf92ba0940d0a9e3fd0ca724ce82", "b7476e702e9816e3c644"),
  entry("character.seojin.2", "37629a9e2f6564605b579c76cc696a3b7c6a1906c67b6b2bc2ad4e054c08f564", "6ab92e9a3ab16c9a355d55e64faf71234de03043e1bfe7d9ade11c7180e0c138", "827ad85803734040e1e80e066974a06545f8eb7b860e62ceefdaa69408562764", "6ab92e9a3ab16c9a355d"),
  entry("character.seojin.2", "c851b0e07f0c2f4cd14d9e95238ddbfa0229142f2c74a72639fa3ff2042dec86", "e867da5d399e5d8e7336f78035b47b1c19c8a4bce3490870069be5e375d79363", "bd2664b46f4f742b0ea02571f21041bb493d9682b3edc7191beb545e781711ed", "e867da5d399e5d8e7336"),
];
const grade3Rows = [
  entry("character.seojin.3", "74c749bf7ab42f5057e26b5ba4721b87af063377cfa785688408c1d75830ddcc", "0c37bf906ee541dd462b5b2159259babdfcb2abf5aaf1bc179e780aa5be34f3f", "97c89bd59b51445206c22fbc5255a3a24fc54d1cc7e4bc282bdee5ffe7f7e78e", "0c37bf906ee541dd462b"),
  entry("character.seojin.3", "7c56681217bec46a124bd20de9e76421c94f9bb9ca2f7984beabfaadaa36cd82", "11d7492930d8582acce70cf87feec57ff1754e87f2ca6d985791e16490827eb1", "94a7a2778882e57a7e98e6642fc7e59cbfc43db859b03254aa40fe18a1302863", "11d7492930d8582acce7"),
  entry("character.seojin.3", "84c78ebcfc129dbf9e0068721021038369272a03ca706035042b60bed79f8d0e", "64ea99406c5cc50e6e3b9304bd4f100ef10b7e0e19dcfd47acd18778a3b2a8aa", "96777e7111cf853ae46a07ba5632d0ab19afd1d1478eb13c351647a52a2f843d", "64ea99406c5cc50e6e3b"),
  entry("character.seojin.3", "8dbaed9a2d079e7460c458e61a8fdd92b7aec61e9a6e6d2c9e4d4098d32714a4", "540f8fd2716081c723faffa629239e288e8f57205f86a517f5562f27e5ceec29", "65994f23f69305ba86910c017d0f9523f0ea8d7affd9030080491dd14c20055c", "540f8fd2716081c723fa"),
];

const grade2 = occupiedIndex("character.seojin.2", grade2Rows);
const grade3 = occupiedIndex("character.seojin.3", grade3Rows);
assert.equal(sha256(`${canonicalJson(grade2)}\n`),
  "d31eac6d5cf7be78695cf0b880c6ace453b15101bea034a12c5a557373feba66");
assert.equal(sha256(`${canonicalJson(grade3)}\n`),
  "189b518eb5ccc209f45c7b6c98c80fcaa467754f61799163dc156537d7c6cf2b");

assert.equal(registry.schemaVersion,
  "generated_media_routing_legacy_compatibility_registry_v1");
assert.equal(`${canonicalJson(registry)}\n`, registryBytes.toString("utf8"));
const grade3Authority = registry.entries[
  "character_single_image.character.seojin.3.pre_created_at.v1"];
assert.equal(grade3Authority.initialIndexSha256,
  "189b518eb5ccc209f45c7b6c98c80fcaa467754f61799163dc156537d7c6cf2b");
assert.equal(grade3Authority.compatibilityRule, "allow_exact_created_at_omission_only");
assert.equal(grade3Authority.omittedRequiredMember, "createdAt");
assert.deepEqual(grade3Authority.legacyRecords.map((record) => ({
  routingRecordId: record.routingRecordId,
  routingPayloadSha256: record.routingPayloadSha256,
  recordPath: record.recordPath,
  recordSha256: record.recordSha256,
})), grade3Rows.map((row) => ({
  routingRecordId: row.routingRecordId,
  routingPayloadSha256: row.routingPayloadSha256,
  recordPath: row.recordPath,
  recordSha256: row.recordSha256,
})));

function validateCurrentRecord(record) {
  if (record.schemaVersion !== "generated_media_routing_v2" ||
      typeof record.createdAt !== "string") throw new Error("routing_index_write_failed");
  return true;
}

function validateLegacyMetadata(scope, metadata, missingKeys) {
  const authority = Object.values(registry.entries).find((candidate) =>
    candidate.assetType === scope.assetType && candidate.contentId === scope.contentId);
  if (!authority || missingKeys.length !== 1 || missingKeys[0] !== "createdAt" ||
      !authority.legacyRecords.some((registered) => canonicalJson(registered) ===
        canonicalJson(metadata))) throw new Error("routing_index_write_failed");
  return true;
}

for (const metadata of grade3Authority.legacyRecords) {
  assert.equal(validateLegacyMetadata(grade3, metadata, ["createdAt"]), true);
  assert.throws(() => validateLegacyMetadata(grade3, metadata, ["createdAt", "validation"]),
    /routing_index_write_failed/);
  assert.throws(() => validateLegacyMetadata(grade3,
    { ...metadata, recordSha256: "0".repeat(64) }, ["createdAt"]),
  /routing_index_write_failed/);
}
assert.throws(() => validateLegacyMetadata(grade2, {
  routingRecordId: "unregistered", routingPayloadSha256: "0".repeat(64),
  recordPath: "unregistered.json", recordSha256: "0".repeat(64),
}, ["createdAt"]), /routing_index_write_failed/);

const fresh = {
  schemaVersion: "generated_media_routing_v2",
  createdAt: "2026-08-22T00:00:00Z",
};
assert.equal(validateCurrentRecord(fresh), true);
assert.throws(() => validateCurrentRecord({ schemaVersion: fresh.schemaVersion }),
  /routing_index_write_failed/);

function appendCurrent(index, newId, newEntry, newRecord) {
  validateCurrentRecord(newRecord);
  assert.equal(Object.hasOwn(index.entries, newId), false);
  const before = structuredClone(index.entries);
  const after = structuredClone(index);
  after.entries[newId] = structuredClone(newEntry);
  for (const [id, value] of Object.entries(before)) assert.deepEqual(after.entries[id], value);
  return after;
}

for (const occupied of [grade2, grade3]) {
  const newId = `gmroute2.character_single_image.${occupied.contentId}.ffffffffffffffffffff`;
  const appended = appendCurrent(occupied, newId, {
    ...Object.values(occupied.entries)[0],
    routingRecordId: newId,
    recordPath: `AgentDocs/planning-data/generated-media-routing/v2/character_single_image/${occupied.contentId}/${newId}.json`,
    recordSha256: "e".repeat(64),
    routingPayloadSha256: "f".repeat(64),
    requestId: `gmplan2.character_single_image.${occupied.contentId}.freshappend0000000000`,
    planningSnapshotHash: "d".repeat(64),
  }, fresh);
  assert.equal(Object.keys(appended.entries).length, Object.keys(occupied.entries).length + 1);
}

console.log("Generated Media legacy routing append contract tests passed.");
