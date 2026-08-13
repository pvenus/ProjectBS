// Executable fixed vectors for provider execution approval v1.
// Values stay inside JSON.stringify's RFC 8785-compatible subset. This is a
// contract vector, not a general-purpose JCS implementation.

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

function sha256(value) {
  return createHash("sha256").update(value, "utf8").digest("hex");
}

function hashObject(value) {
  return sha256(canonicalJson(value));
}

function assertClosedKeys(value, required, optional = []) {
  const allowed = [...required, ...optional].sort();
  for (const key of Object.keys(value)) assert.ok(allowed.includes(key), `unknown key: ${key}`);
  for (const key of required) assert.ok(Object.hasOwn(value, key), `missing key: ${key}`);
}

const providerSettings = {
  background: "opaque",
  format: "png",
  quality: "high",
  size: "1024x1024",
};

function scopeFixture() {
  return {
    schemaVersion: "generated_media_provider_execution_scope_hash_payload_v1",
    requestId: "gmreq.character.seojin.1",
    assetType: "character_single_image",
    domainType: "character",
    contentId: "seojin",
    planningSnapshotHash: "a".repeat(64),
    registryRowId: "character_single_image_v2",
    structureProfile: "character_single_image_v2",
    promptRecordId: "gmprompt3.character_single_image.character.seojin.1.e12ee2ebe2787f10e8a5",
    promptRecordSha256: "b".repeat(64),
    promptFileSha256: "3313e882e877653bc059fa85bfea8299940f88360673b1ba39d111106c2803c9",
    providerPromptPayloadHash: "6f855d4140bc32db400af207899c1ab3a981d4b9df17d3313fe594d05698809d",
    provider: "imagegen",
    providerTool: "imagegen",
    providerInterface: "configured_imagegen_capability",
    providerSettings: structuredClone(providerSettings),
    providerSettingsSha256: "a1e5fb882b29876db5770023e913b6e62056ac1395a30765136132460be5ce4c",
  };
}

function approvalFixture(scopeHash, overrides = {}) {
  return {
    schemaVersion: "generated_media_provider_execution_approval_v1",
    approvedBy: "user:contract-reviewer",
    approvedAt: "2026-08-13T12:00:00+09:00",
    scopeHash,
    maxAttempts: 2,
    maxCost: { amount: "0.250000", currency: "USD", kind: "iso_currency" },
    estimateUnavailablePolicy: "block",
    approvalEvidence: "codex-thread:019ffabb-97f6-7af3-abaa-f70747dc125f/message:approval-1",
    ...overrides,
  };
}

const decimal = /^(0|[1-9][0-9]*)\.[0-9]{6}$/;
function millionths(amount) {
  assert.match(amount, decimal);
  return BigInt(amount.replace(".", ""));
}

function sameUnit(left, right) {
  if (left.kind !== right.kind) return false;
  if (left.kind === "iso_currency") return left.currency === right.currency;
  if (left.kind === "provider_credit") {
    return left.provider === right.provider && left.creditUnit === right.creditUnit;
  }
  if (left.kind === "provider_unit") {
    return left.provider === right.provider && left.unit === right.unit;
  }
  return left.kind === "no_charge";
}

function validateEstimate(approval, estimate) {
  if (estimate.status === "no_charge") return "approved_no_charge";
  if (estimate.status === "unavailable") {
    if (approval.estimateUnavailablePolicy !== "allow_upper_bound"
      || approval.maxCost.kind === "no_charge") {
      throw new Error("provider_cost_estimate_unavailable");
    }
    return "approved_at_max_cost_upper_bound";
  }
  if (!sameUnit(approval.maxCost, estimate.cost)) {
    throw new Error("provider_cost_unit_mismatch");
  }
  if (approval.maxCost.kind === "no_charge"
    || millionths(estimate.cost.amount) > millionths(approval.maxCost.amount)) {
    throw new Error("provider_cost_limit_exceeded");
  }
  return "approved_within_limit";
}

function validateApproval(approval) {
  assertClosedKeys(approval, [
    "schemaVersion", "approvedBy", "approvedAt", "scopeHash", "maxAttempts",
    "maxCost", "estimateUnavailablePolicy", "approvalEvidence",
  ]);
  if (!Number.isInteger(approval.maxAttempts)
    || approval.maxAttempts < 1 || approval.maxAttempts > 2147483647) {
    throw new Error("invalid_provider_execution_approval");
  }
  if (approval.maxCost.kind !== "no_charge") millionths(approval.maxCost.amount);
  return true;
}

function validateActual(approval, actual) {
  if (actual.status === "no_charge") return "actual_no_charge";
  if (actual.status === "unavailable") throw new Error("provider_actual_cost_unavailable");
  if (!sameUnit(approval.maxCost, actual.cost)) throw new Error("provider_cost_unit_mismatch");
  if (approval.maxCost.kind === "no_charge"
    || millionths(actual.cost.amount) > millionths(approval.maxCost.amount)) {
    throw new Error("provider_cost_limit_exceeded");
  }
  return "actual_within_limit";
}

function attemptsConsumed(entries) {
  return new Set(entries.filter((entry) => entry.providerCalled)
    .map((entry) => entry.attemptNumber)).size;
}

function authorizeNextAttempt(approval, entries) {
  const consumed = attemptsConsumed(entries);
  if (consumed >= approval.maxAttempts) throw new Error("retry_limit_exceeded");
  return consumed + 1;
}

const scope = scopeFixture();
assertClosedKeys(scope, [
  "schemaVersion", "requestId", "assetType", "domainType", "contentId",
  "planningSnapshotHash", "registryRowId", "structureProfile", "promptRecordId",
  "promptRecordSha256", "promptFileSha256", "providerPromptPayloadHash", "provider",
  "providerTool", "providerInterface", "providerSettings", "providerSettingsSha256",
], ["animationRequestId"]);
assert.equal(hashObject(providerSettings), scope.providerSettingsSha256);
const scopeCanonical = canonicalJson(scope);
const fixedScopeHash = hashObject(scope);
assert.equal(fixedScopeHash, "be78667b021ad8a15e3b02cb00198249304092d723f20f5a90c3b969a09d01bb");

// Same scope is stable; every bound prompt/settings/identity change changes it.
assert.equal(hashObject(scopeFixture()), fixedScopeHash);
for (const mutate of [
  (v) => { v.promptRecordSha256 = "c".repeat(64); },
  (v) => { v.promptFileSha256 = "d".repeat(64); },
  (v) => { v.providerPromptPayloadHash = "e".repeat(64); },
  (v) => { v.providerSettings.quality = "standard"; v.providerSettingsSha256 = hashObject(v.providerSettings); },
  (v) => { v.contentId = "seojin_variant"; },
]) {
  const changed = scopeFixture();
  mutate(changed);
  assert.notEqual(hashObject(changed), fixedScopeHash);
}

// Limits are envelope bindings, not execution-scope bindings.
const approval = approvalFixture(fixedScopeHash);
assertClosedKeys(approval, [
  "schemaVersion", "approvedBy", "approvedAt", "scopeHash", "maxAttempts",
  "maxCost", "estimateUnavailablePolicy", "approvalEvidence",
]);
assert.equal(validateApproval(approval), true);
assert.throws(() => validateApproval(approvalFixture(fixedScopeHash, { maxAttempts: 0 })),
  /invalid_provider_execution_approval/);
assert.throws(() => validateApproval(approvalFixture(fixedScopeHash, { maxAttempts: 1.5 })),
  /invalid_provider_execution_approval/);
assert.throws(() => millionths("0.25"));
const renewed = approvalFixture(fixedScopeHash, {
  approvedAt: "2026-08-13T13:00:00+09:00",
  maxAttempts: 3,
  maxCost: { amount: "0.500000", currency: "USD", kind: "iso_currency" },
  approvalEvidence: "codex-thread:019ffabb-97f6-7af3-abaa-f70747dc125f/message:approval-2",
});
assert.equal(approval.scopeHash, renewed.scopeHash);
assert.notEqual(hashObject(approval), hashObject(renewed));
assert.equal(hashObject(approval), "4d974e3c0abc88354f32b49d42f9c03c228c30d686f27eac6f93c1ff663f28fd");

// Free/no-charge, unknown estimate, unit mismatch and amount exceed.
assert.equal(validateEstimate(approval, { status: "no_charge" }), "approved_no_charge");
const freeApproval = approvalFixture(fixedScopeHash, { maxCost: { kind: "no_charge" } });
assert.equal(validateEstimate(freeApproval, { status: "no_charge" }), "approved_no_charge");
assert.throws(() => validateEstimate(freeApproval, {
  status: "exact", cost: { amount: "0.000001", currency: "USD", kind: "iso_currency" },
}), /provider_cost_unit_mismatch/);
assert.throws(() => validateEstimate(approvalFixture(fixedScopeHash, {
  maxCost: { kind: "no_charge" }, estimateUnavailablePolicy: "allow_upper_bound",
}), { status: "unavailable" }), /provider_cost_estimate_unavailable/);
assert.throws(() => validateEstimate(approval, { status: "unavailable" }),
  /provider_cost_estimate_unavailable/);
const unknownAllowed = approvalFixture(fixedScopeHash, { estimateUnavailablePolicy: "allow_upper_bound" });
assert.equal(validateEstimate(unknownAllowed, { status: "unavailable" }),
  "approved_at_max_cost_upper_bound");
assert.throws(() => validateEstimate(approval, {
  status: "exact", cost: { amount: "0.250001", currency: "USD", kind: "iso_currency" },
}), /provider_cost_limit_exceeded/);
assert.throws(() => validateEstimate(approval, {
  status: "exact", cost: { amount: "0.100000", currency: "KRW", kind: "iso_currency" },
}), /provider_cost_unit_mismatch/);
assert.equal(validateActual(approval, { status: "no_charge" }), "actual_no_charge");
assert.throws(() => validateActual(approval, { status: "unavailable" }),
  /provider_actual_cost_unavailable/);
assert.throws(() => validateActual(approval, {
  status: "exact", cost: { amount: "0.250001", currency: "USD", kind: "iso_currency" },
}), /provider_cost_limit_exceeded/);

// Failed/ambiguous submitted attempts are consumed; preflight/reuse are not.
const attemptEvidence = [
  { attemptNumber: 0, providerCalled: false, event: "preflight_blocked" },
  { attemptNumber: 1, providerCalled: true, event: "submitted" },
  { attemptNumber: 1, providerCalled: true, event: "terminal", result: "failed" },
  { attemptNumber: 0, providerCalled: false, event: "completed_reuse" },
  { attemptNumber: 2, providerCalled: true, event: "terminal", result: "ambiguous" },
];
assert.equal(attemptsConsumed(attemptEvidence), 2);
assert.equal(attemptsConsumed(attemptEvidence) >= approval.maxAttempts, true);
assert.throws(() => authorizeNextAttempt(approval, attemptEvidence), /retry_limit_exceeded/);
assert.equal(authorizeNextAttempt(renewed, attemptEvidence), 3);

// Record, generation-index entry, and preservation handoff use one projection.
const costEvidence = [{
  scopeHash: fixedScopeHash,
  attemptNumber: 1,
  providerCalled: true,
  event: "terminal",
  estimate: { status: "exact", cost: { amount: "0.100000", currency: "USD", kind: "iso_currency" } },
  actualCost: { status: "exact", cost: { amount: "0.100000", currency: "USD", kind: "iso_currency" } },
  approvedUpperBound: approval.maxCost,
  providerOperationRef: "imagegen-operation:vector-1",
  evidenceRef: "provider-ledger:vector-1",
  recordedAt: "2026-08-13T12:01:00+09:00",
}];
const projection = {
  scopeHash: fixedScopeHash,
  providerExecutionApprovalSha256: hashObject(approval),
  maxAttempts: approval.maxAttempts,
  maxCost: approval.maxCost,
  estimateUnavailablePolicy: approval.estimateUnavailablePolicy,
  attemptsConsumed: 1,
  costEvidenceSha256: hashObject(costEvidence),
  actualCostStatus: "exact",
  actualCostTotal: { amount: "0.100000", currency: "USD", kind: "iso_currency" },
};
const generationRecord = { approvalCostProjection: structuredClone(projection) };
const generationIndexEntry = { approvalCostProjection: structuredClone(projection) };
const preservationHandoff = { approvalCostProjection: structuredClone(projection) };
assert.equal(canonicalJson(generationRecord.approvalCostProjection), canonicalJson(projection));
assert.equal(canonicalJson(generationIndexEntry.approvalCostProjection), canonicalJson(projection));
assert.equal(canonicalJson(preservationHandoff.approvalCostProjection), canonicalJson(projection));
const divergentHandoff = structuredClone(preservationHandoff);
divergentHandoff.approvalCostProjection.attemptsConsumed = 2;
assert.notEqual(canonicalJson(divergentHandoff.approvalCostProjection), canonicalJson(projection));

// These one-line values are the concrete canonical JSON examples used by the fixed vector.
assert.equal(scopeCanonical, canonicalJson(scopeFixture()));
assert.equal(canonicalJson(JSON.parse(scopeCanonical)), scopeCanonical);
console.log({ fixedScopeHash, fixedApprovalHash: hashObject(approval), scopeCanonical });
console.log("generated media provider execution approval v1 contract vectors: PASS");
