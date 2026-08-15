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

const capabilityDescriptor = {
  schemaVersion: "generated_media_imagegen_capability_descriptor_v1",
  provider: "imagegen",
  providerTool: "imagegen",
  providerInterface: "configured_imagegen_capability",
  capabilityVersion: "imagegen-capability@2026-08-15.1",
  settingsDescriptorVersion: "imagegen-settings@2026-08-15.1",
  costDescriptorVersion: "imagegen-cost@2026-08-15.1",
};

// Transport/hash vector only. Production intent-to-default and pricing golden
// mappings are owned by the configured capability descriptor implementation.
const providerSettingsIntent = {
  canvas: { height: 1536, width: 1024 },
  generationBackground: { color: "#F2EFE6", mode: "removable_solid" },
  outputFormat: "png",
};

function preflightRequestFixture() {
  return {
    schemaVersion: "generated_media_imagegen_capability_preflight_request_v1",
    mode: "non_submit",
    provider: "imagegen",
    providerTool: "imagegen",
    providerInterface: "configured_imagegen_capability",
    assetType: "character_single_image",
    providerSettingsIntent: structuredClone(providerSettingsIntent),
    providerSettingsIntentSha256: hashObject(providerSettingsIntent),
  };
}

function preflightFixture(overrides = {}) {
  return {
    schemaVersion: "generated_media_imagegen_capability_preflight_v1",
    mode: "non_submit",
    submitBoundaryCrossed: false,
    capabilityDescriptor: structuredClone(capabilityDescriptor),
    capabilityDescriptorSha256: hashObject(capabilityDescriptor),
    providerSettings: structuredClone(providerSettings),
    providerSettingsSha256: hashObject(providerSettings),
    estimate: {
      status: "exact",
      cost: { amount: "0.100000", currency: "USD", kind: "iso_currency" },
    },
    evidenceRef: "imagegen-capability-evidence:vector-1",
    ...overrides,
  };
}

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
    capabilityDescriptor: structuredClone(capabilityDescriptor),
    capabilityDescriptorSha256: hashObject(capabilityDescriptor),
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

function validateCapabilityPreflight(preflight) {
  assertClosedKeys(preflight, [
    "schemaVersion", "mode", "submitBoundaryCrossed", "capabilityDescriptor",
    "capabilityDescriptorSha256", "providerSettings", "providerSettingsSha256",
    "estimate", "evidenceRef",
  ]);
  assert.equal(preflight.schemaVersion, "generated_media_imagegen_capability_preflight_v1");
  assert.equal(preflight.mode, "non_submit");
  assert.equal(preflight.submitBoundaryCrossed, false);
  assertClosedKeys(preflight.capabilityDescriptor, [
    "schemaVersion", "provider", "providerTool", "providerInterface",
    "capabilityVersion", "settingsDescriptorVersion", "costDescriptorVersion",
  ]);
  assert.equal(preflight.capabilityDescriptor.schemaVersion,
    "generated_media_imagegen_capability_descriptor_v1");
  assert.equal(preflight.capabilityDescriptor.provider, "imagegen");
  assert.equal(preflight.capabilityDescriptor.providerTool, "imagegen");
  assert.equal(preflight.capabilityDescriptor.providerInterface,
    "configured_imagegen_capability");
  for (const key of ["capabilityVersion", "settingsDescriptorVersion", "costDescriptorVersion"]) {
    assert.equal(typeof preflight.capabilityDescriptor[key], "string");
    assert.ok(preflight.capabilityDescriptor[key].length > 0);
  }
  assert.equal(hashObject(preflight.capabilityDescriptor),
    preflight.capabilityDescriptorSha256);
  assert.match(preflight.capabilityDescriptorSha256, /^[0-9a-f]{64}$/);
  assert.ok(preflight.providerSettings !== null
    && typeof preflight.providerSettings === "object"
    && !Array.isArray(preflight.providerSettings));
  assert.equal(hashObject(preflight.providerSettings), preflight.providerSettingsSha256);
  assert.match(preflight.providerSettingsSha256, /^[0-9a-f]{64}$/);
  if (preflight.estimate.status === "no_charge"
    || preflight.estimate.status === "unavailable") {
    assertClosedKeys(preflight.estimate, ["status"]);
  } else {
    assert.ok(["exact", "upper_bound"].includes(preflight.estimate.status));
    assertClosedKeys(preflight.estimate, ["status", "cost"]);
    assert.ok(["iso_currency", "provider_credit", "provider_unit"]
      .includes(preflight.estimate.cost.kind));
    const requiredCostKeys = preflight.estimate.cost.kind === "iso_currency"
      ? ["amount", "currency", "kind"]
      : preflight.estimate.cost.kind === "provider_credit"
        ? ["amount", "creditUnit", "kind", "provider"]
        : preflight.estimate.cost.kind === "provider_unit"
          ? ["amount", "kind", "provider", "unit"]
          : ["kind"];
    assertClosedKeys(preflight.estimate.cost, requiredCostKeys);
    assert.match(preflight.estimate.cost.amount, decimal);
    if (preflight.estimate.cost.kind === "iso_currency") {
      assert.match(preflight.estimate.cost.currency, /^[A-Z]{3}$/);
    } else {
      assert.match(preflight.estimate.cost.provider, /^[a-z][a-z0-9._-]*$/);
    }
  }
  assert.equal(typeof preflight.evidenceRef, "string");
  assert.ok(preflight.evidenceRef.length > 0);
  return true;
}

function validateCapabilityPreflightRequest(request) {
  assertClosedKeys(request, [
    "schemaVersion", "mode", "provider", "providerTool", "providerInterface",
    "assetType", "providerSettingsIntent", "providerSettingsIntentSha256",
  ]);
  assert.equal(request.schemaVersion,
    "generated_media_imagegen_capability_preflight_request_v1");
  assert.equal(request.mode, "non_submit");
  assert.equal(request.provider, "imagegen");
  assert.equal(request.providerTool, "imagegen");
  assert.equal(request.providerInterface, "configured_imagegen_capability");
  assert.ok(["character_single_image", "icon_single_image",
    "background_single_image", "animation"].includes(request.assetType));
  assert.equal(hashObject(request.providerSettingsIntent),
    request.providerSettingsIntentSha256);
  return true;
}

function requireCapabilityPreflight(preflight) {
  if (preflight === undefined) throw new Error("provider_capability_descriptor_unavailable");
  try {
    validateCapabilityPreflight(preflight);
  } catch {
    throw new Error("provider_capability_preflight_invalid");
  }
  return preflight;
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
  "providerTool", "providerInterface", "capabilityDescriptor",
  "capabilityDescriptorSha256", "providerSettings", "providerSettingsSha256",
], ["animationRequestId"]);
const preflight = preflightFixture();
const preflightRequest = preflightRequestFixture();
assert.equal(validateCapabilityPreflightRequest(preflightRequest), true);
const unknownPreflightRequest = preflightRequestFixture();
unknownPreflightRequest.unknown = true;
assert.throws(() => validateCapabilityPreflightRequest(unknownPreflightRequest), /unknown key/);
assert.throws(() => validateCapabilityPreflightRequest({
  ...preflightRequestFixture(), providerSettingsIntentSha256: "0".repeat(64),
}));
assert.equal(requireCapabilityPreflight(preflight), preflight);
assert.equal(preflight.capabilityDescriptorSha256, scope.capabilityDescriptorSha256);
assert.equal(preflight.providerSettingsSha256, scope.providerSettingsSha256);
assert.throws(() => requireCapabilityPreflight(undefined),
  /provider_capability_descriptor_unavailable/);
assert.throws(() => requireCapabilityPreflight(preflightFixture({ mode: "submit" })),
  /provider_capability_preflight_invalid/);
assert.throws(() => requireCapabilityPreflight(preflightFixture({ submitBoundaryCrossed: true })),
  /provider_capability_preflight_invalid/);
assert.throws(() => requireCapabilityPreflight(preflightFixture({ evidenceRef: "" })),
  /provider_capability_preflight_invalid/);
assert.throws(() => requireCapabilityPreflight(preflightFixture({
  capabilityDescriptorSha256: "0".repeat(64),
})), /provider_capability_preflight_invalid/);
const unknownPreflight = preflightFixture();
unknownPreflight.unknown = true;
assert.throws(() => requireCapabilityPreflight(unknownPreflight),
  /provider_capability_preflight_invalid/);
assert.equal(hashObject(providerSettings), scope.providerSettingsSha256);
const scopeCanonical = canonicalJson(scope);
const fixedScopeHash = hashObject(scope);
assert.equal(fixedScopeHash, "b6ff09a80553191de47b5ad746bd8960f4559da78887670ab667c07da25dcf1b");

// Same scope is stable; every bound prompt/settings/identity change changes it.
assert.equal(hashObject(scopeFixture()), fixedScopeHash);
for (const mutate of [
  (v) => { v.promptRecordSha256 = "c".repeat(64); },
  (v) => { v.promptFileSha256 = "d".repeat(64); },
  (v) => { v.providerPromptPayloadHash = "e".repeat(64); },
  (v) => { v.providerSettings.quality = "standard"; v.providerSettingsSha256 = hashObject(v.providerSettings); },
  (v) => { v.capabilityDescriptor.costDescriptorVersion = "imagegen-cost@2026-08-15.2";
    v.capabilityDescriptorSha256 = hashObject(v.capabilityDescriptor); },
  (v) => { v.contentId = "seojin_variant"; },
]) {
  const changed = scopeFixture();
  mutate(changed);
  assert.notEqual(hashObject(changed), fixedScopeHash);
}

// Any descriptor/settings drift between approved preflight and submit blocks.
function assertNoCapabilityDrift(approvedScope, currentPreflight) {
  requireCapabilityPreflight(currentPreflight);
  if (approvedScope.capabilityDescriptorSha256 !== currentPreflight.capabilityDescriptorSha256
    || approvedScope.providerSettingsSha256 !== currentPreflight.providerSettingsSha256) {
    throw new Error("provider_capability_drift");
  }
}
assert.doesNotThrow(() => assertNoCapabilityDrift(scope, preflightFixture()));
const driftedDescriptor = structuredClone(capabilityDescriptor);
driftedDescriptor.settingsDescriptorVersion = "imagegen-settings@2026-08-15.2";
assert.throws(() => assertNoCapabilityDrift(scope, preflightFixture({
  capabilityDescriptor: driftedDescriptor,
  capabilityDescriptorSha256: hashObject(driftedDescriptor),
})), /provider_capability_drift/);
const driftedSettings = { ...providerSettings, quality: "standard" };
assert.throws(() => assertNoCapabilityDrift(scope, preflightFixture({
  providerSettings: driftedSettings,
  providerSettingsSha256: hashObject(driftedSettings),
})), /provider_capability_drift/);

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
assert.equal(hashObject(approval), "a68e67b54ca19eaa266b9ecfa7f534764885994daa331ea0263de6bc4531b339");

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
