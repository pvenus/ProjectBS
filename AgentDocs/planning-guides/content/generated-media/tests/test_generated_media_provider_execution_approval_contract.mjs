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

// Hosted built-in preview v1 is deliberately isolated from promotable v1
// approval. It records unavailable evidence rather than fabricating it.
function hostedPreviewFixture(workUnitType = "exact_single_image") {
  const animation = workUnitType === "exact_animation_request";
  const settingsSeal = {
    schemaVersion: "generated_media_hosted_preview_settings_seal_v1",
    providerSettingsIntent: structuredClone(providerSettingsIntent),
    providerSettingsIntentSha256: hashObject(providerSettingsIntent),
    exposedOptions: structuredClone(providerSettingsIntent),
    exposedOptionsSha256: hashObject(providerSettingsIntent),
    capabilityDescriptorStatus: "unavailable_on_callable_surface",
    settingsDescriptorStatus: "unavailable_on_callable_surface",
    costEstimate: { status: "unavailable" },
  };
  const scopePayload = {
    schemaVersion: "generated_media_hosted_preview_scope_hash_payload_v1",
    requestId: "gmreq.character.seojin.preview.1",
    assetType: animation ? "animation" : "character_single_image",
    domainType: "character",
    contentId: "seojin",
    ...(animation ? { animationRequestId: "attack_01" } : {}),
    planningSnapshotHash: "a".repeat(64),
    promptRecordId: "gmprompt3.preview",
    promptRecordSha256: "b".repeat(64),
    promptFileSha256: "c".repeat(64),
    providerPromptPayloadHash: "d".repeat(64),
    referenceBindings: [{ role: "character_reference", projectRelativePath:
      "output/generated-media-preview/references/seojin.png", sha256: "e".repeat(64) }],
    provider: "imagegen",
    providerTool: "built-in_imagegen",
    executionMode: "hosted_builtin_preview_v1",
    settingsSealSha256: hashObject(settingsSeal),
  };
  const previewScopeHash = hashObject(scopePayload);
  const approval = {
    schemaVersion: "generated_media_hosted_preview_approval_v1",
    executionMode: "hosted_builtin_preview_v1",
    approvedBy: "user:contract-reviewer",
    approvedAt: "2026-08-15T10:00:00+09:00",
    approvalEvidence: "codex-thread:current/message:exact-preview-approval",
    previewScopeHash,
    workUnitType,
    ...(animation ? { animationRequestId: "attack_01" } : {}),
    submitCountMaximum: 1,
    retryCountMaximum: 0,
    promotionPolicy: "not_promotable",
  };
  const scenePromptOriginal = "Generate one image on the exact removable solid warm-ivory background. No halo, vignette, scene, or shadow.";
  return { settingsSeal, scopePayload, previewScopeHash, approval, scenePromptOriginal };
}

function validateHostedPreviewCallableSurface(settingsSeal) {
  for (const key of Object.keys(settingsSeal.providerSettingsIntent)) {
    if (!Object.hasOwn(settingsSeal.exposedOptions, key)
      || canonicalJson(settingsSeal.exposedOptions[key])
        !== canonicalJson(settingsSeal.providerSettingsIntent[key])) {
      throw new Error("hosted_preview_unknown_setting");
    }
  }
  return true;
}

function validateHostedStyleReference(settingsSeal, referenceBindings) {
  for (const binding of referenceBindings ?? []) {
    if (binding.role !== "style_only") continue;
    const required = ["role", "projectRelativePath", "sha256", "reviewRecordId",
      "reviewRecordPath", "reviewRecordSha256"];
    if (Object.keys(binding).length !== required.length ||
        required.some((key) => !Object.hasOwn(binding, key))) {
      throw new Error("style_reference_binding_incomplete");
    }
    if (settingsSeal.styleReferenceRoleStatus !== "supported_distinct_style_only") {
      throw new Error("hosted_preview_unknown_setting");
    }
  }
  return true;
}

function validateHostedPreviewPromptStageSemantics(settingsSeal, scenePromptOriginal) {
  const removableSolid = settingsSeal.providerSettingsIntent.generationBackground?.mode
    === "removable_solid";
  const requestsDownstreamRemoval = /transparent final|background[- ]remov/i
    .test(scenePromptOriginal);
  if (removableSolid && requestsDownstreamRemoval) {
    throw new Error("hosted_preview_prompt_stage_semantics_conflict");
  }
  return true;
}

function validateHostedPreview({ settingsSeal, scopePayload, previewScopeHash, approval,
  scenePromptOriginal },
  state = { submitCount: 0, retryCount: 0 }) {
  assertClosedKeys(settingsSeal, ["schemaVersion", "providerSettingsIntent",
    "providerSettingsIntentSha256", "exposedOptions", "exposedOptionsSha256",
    "capabilityDescriptorStatus", "settingsDescriptorStatus", "costEstimate"],
    ["styleReferenceRoleStatus"]);
  assert.equal(settingsSeal.costEstimate.status, "unavailable");
  assert.equal(settingsSeal.capabilityDescriptorStatus, "unavailable_on_callable_surface");
  assert.equal(settingsSeal.settingsDescriptorStatus, "unavailable_on_callable_surface");
  assert.equal(hashObject(settingsSeal.providerSettingsIntent),
    settingsSeal.providerSettingsIntentSha256);
  assert.equal(hashObject(settingsSeal.exposedOptions), settingsSeal.exposedOptionsSha256);
  for (const forbidden of ["capabilityDescriptor", "capabilityVersion", "evidenceRef", "cost"])
    assert.equal(Object.hasOwn(settingsSeal, forbidden), false);
  validateHostedPreviewCallableSurface(settingsSeal);
  validateHostedStyleReference(settingsSeal, scopePayload.referenceBindings);
  validateHostedPreviewPromptStageSemantics(settingsSeal, scenePromptOriginal);
  if (hashObject(scopePayload) !== previewScopeHash
    || approval.previewScopeHash !== previewScopeHash) throw new Error("hosted_preview_scope_mismatch");
  if (approval.submitCountMaximum !== 1 || approval.retryCountMaximum !== 0
    || approval.promotionPolicy !== "not_promotable") throw new Error("invalid_hosted_preview_approval");
  if (state.submitCount >= 1) throw new Error("hosted_preview_submit_limit_exceeded");
  if (state.retryCount > 0) throw new Error("hosted_preview_retry_forbidden");
  return true;
}

function hostedPreviewAutoPolicyFixture() {
  const authorizationSource = {
    type: "authenticated_thread_user_instruction",
    threadId: "019ffbc8-31b1-7652-8f1f-6d7958c2e15d",
    instructionSha256: "1".repeat(64),
  };
  const policyScope = {
    provider: "imagegen",
    executionMode: "hosted_builtin_preview_v1",
    assetTypes: ["character_single_image"],
    domainTypes: ["character"],
    contentIds: ["seojin"],
    workUnitTypes: ["exact_single_image"],
    referencePolicy: "prompt_bound_only",
    submitCountMaximumPerScope: 1,
    retryCountMaximumPerScope: 0,
    costPolicy: "allow_unavailable_preview_only",
    promotionPolicy: "not_promotable",
    preservationPolicy: "not_preservable",
    evaluationPolicy: "not_evaluated",
  };
  const payload = {
    schemaVersion: "generated_media_hosted_preview_auto_approval_policy_v1",
    authorizationSource,
    policyScope,
    lifetime: "until_revoked",
  };
  const policyPayloadSha256 = hashObject(payload);
  return {
    policyId: `gmpreviewpolicy1.${policyPayloadSha256.slice(0, 20)}`,
    policyPayloadSha256,
    ...payload,
  };
}

function validateHostedPreviewAutoPolicy(policy) {
  assertClosedKeys(policy, ["schemaVersion", "policyId", "policyPayloadSha256",
    "authorizationSource", "policyScope", "lifetime"]);
  assertClosedKeys(policy.authorizationSource, ["type", "threadId", "instructionSha256"]);
  assertClosedKeys(policy.policyScope, ["provider", "executionMode", "assetTypes",
    "domainTypes", "contentIds", "workUnitTypes", "referencePolicy",
    "submitCountMaximumPerScope", "retryCountMaximumPerScope", "costPolicy",
    "promotionPolicy", "preservationPolicy", "evaluationPolicy"]);
  const payload = { schemaVersion: policy.schemaVersion,
    authorizationSource: policy.authorizationSource, policyScope: policy.policyScope,
    lifetime: policy.lifetime };
  assert.equal(hashObject(payload), policy.policyPayloadSha256);
  assert.equal(policy.policyId, `gmpreviewpolicy1.${policy.policyPayloadSha256.slice(0, 20)}`);
  assert.equal(policy.authorizationSource.type, "authenticated_thread_user_instruction");
  assert.ok(policy.authorizationSource.threadId.length > 0);
  assert.match(policy.authorizationSource.instructionSha256, /^[0-9a-f]{64}$/);
  assert.equal(policy.policyScope.provider, "imagegen");
  assert.equal(policy.policyScope.executionMode, "hosted_builtin_preview_v1");
  for (const key of ["assetTypes", "domainTypes", "contentIds", "workUnitTypes"]) {
    assert.ok(Array.isArray(policy.policyScope[key]) && policy.policyScope[key].length > 0);
    assert.equal(new Set(policy.policyScope[key]).size, policy.policyScope[key].length);
    assert.equal(policy.policyScope[key].some((value) => value.includes("*")), false);
  }
  assert.equal(policy.policyScope.assetTypes.every((value) => ["character_single_image",
    "icon_single_image", "background_single_image", "animation"].includes(value)), true);
  assert.equal(policy.policyScope.domainTypes.every((value) => ["character", "skill", "item",
    "stage", "battle", "environment"].includes(value)), true);
  assert.equal(policy.policyScope.workUnitTypes.every((value) => ["exact_single_image",
    "exact_animation_request"].includes(value)), true);
  assert.equal(policy.policyScope.submitCountMaximumPerScope, 1);
  assert.equal(policy.policyScope.retryCountMaximumPerScope, 0);
  assert.equal(policy.policyScope.referencePolicy, "prompt_bound_only");
  assert.equal(policy.policyScope.costPolicy, "allow_unavailable_preview_only");
  assert.equal(policy.policyScope.promotionPolicy, "not_promotable");
  assert.equal(policy.policyScope.preservationPolicy, "not_preservable");
  assert.equal(policy.policyScope.evaluationPolicy, "not_evaluated");
  assert.equal(policy.lifetime, "until_revoked");
  return true;
}

function attestHostedPreview(scope, workUnitType, policy, revoked = false) {
  validateHostedPreviewAutoPolicy(policy);
  if (revoked) throw new Error("hosted_preview_auto_approval_policy_revoked");
  const p = policy.policyScope;
  if (!p.assetTypes.includes(scope.scopePayload.assetType)
    || !p.domainTypes.includes(scope.scopePayload.domainType)
    || !p.contentIds.includes(scope.scopePayload.contentId)
    || !p.workUnitTypes.includes(workUnitType)) {
    throw new Error("hosted_preview_auto_approval_policy_mismatch");
  }
  return {
    schemaVersion: "generated_media_hosted_preview_auto_approval_attestation_v1",
    executionMode: "hosted_builtin_preview_v1",
    policyId: policy.policyId,
    policyPayloadSha256: policy.policyPayloadSha256,
    authorizationSourceSha256: hashObject(policy.authorizationSource),
    previewScopeHash: scope.previewScopeHash,
    workUnitType,
    submitCountMaximum: 1,
    retryCountMaximum: 0,
    promotionPolicy: "not_promotable",
  };
}

const singlePreview = hostedPreviewFixture();
const animationPreview = hostedPreviewFixture("exact_animation_request");
assert.equal(validateHostedPreview(singlePreview), true);
assert.equal(validateHostedPreview(animationPreview), true);
assert.equal(animationPreview.approval.animationRequestId, "attack_01");
assert.throws(() => validateHostedPreview({ ...singlePreview,
  previewScopeHash: "0".repeat(64) }), /hosted_preview_scope_mismatch/);
const promptDrift = structuredClone(singlePreview);
promptDrift.scopePayload.promptFileSha256 = "f".repeat(64);
assert.throws(() => validateHostedPreview(promptDrift), /hosted_preview_scope_mismatch/);
const referenceDrift = structuredClone(singlePreview);
referenceDrift.scopePayload.referenceBindings[0].sha256 = "f".repeat(64);
assert.throws(() => validateHostedPreview(referenceDrift), /hosted_preview_scope_mismatch/);
const durableStylePreview = structuredClone(singlePreview);
durableStylePreview.scopePayload.referenceBindings = [{
  role: "style_only",
  projectRelativePath: "AgentDocs/reference-assets/generated-media/style-only/character_single_image/open_ink_wash_dynamic_contour/b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf.png",
  sha256: "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf",
  reviewRecordId: "gmstyleref1.character_single_image.open_ink_wash_dynamic_contour.d6dae45a8f8f6591b5cb",
  reviewRecordPath: "AgentDocs/planning-data/style-reference-reviews/v1/character_single_image/open_ink_wash_dynamic_contour/gmstyleref1.character_single_image.open_ink_wash_dynamic_contour.d6dae45a8f8f6591b5cb.json",
  reviewRecordSha256: "51630e6c2c4ec80caae9bf5c995f7673e2b8fddf83870c5a28452971fa2be4c2",
}];
durableStylePreview.settingsSeal.styleReferenceRoleStatus = "supported_distinct_style_only";
durableStylePreview.scopePayload.settingsSealSha256 = hashObject(durableStylePreview.settingsSeal);
durableStylePreview.previewScopeHash = hashObject(durableStylePreview.scopePayload);
durableStylePreview.approval.previewScopeHash = durableStylePreview.previewScopeHash;
assert.equal(validateHostedPreview(durableStylePreview), true);
const incompleteStylePreview = structuredClone(durableStylePreview);
delete incompleteStylePreview.scopePayload.referenceBindings[0].reviewRecordSha256;
incompleteStylePreview.previewScopeHash = hashObject(incompleteStylePreview.scopePayload);
incompleteStylePreview.approval.previewScopeHash = incompleteStylePreview.previewScopeHash;
assert.throws(() => validateHostedPreview(incompleteStylePreview),
  /style_reference_binding_incomplete/);
const unsupportedStylePreview = structuredClone(durableStylePreview);
delete unsupportedStylePreview.settingsSeal.styleReferenceRoleStatus;
unsupportedStylePreview.scopePayload.settingsSealSha256 = hashObject(unsupportedStylePreview.settingsSeal);
unsupportedStylePreview.previewScopeHash = hashObject(unsupportedStylePreview.scopePayload);
unsupportedStylePreview.approval.previewScopeHash = unsupportedStylePreview.previewScopeHash;
assert.throws(() => validateHostedPreview(unsupportedStylePreview), /hosted_preview_unknown_setting/);
assert.throws(() => validateHostedPreview(singlePreview, { submitCount: 1, retryCount: 0 }),
  /hosted_preview_submit_limit_exceeded/);
assert.throws(() => validateHostedPreview(singlePreview, { submitCount: 0, retryCount: 1 }),
  /hosted_preview_retry_forbidden/);
const invalidPreview = structuredClone(singlePreview);
invalidPreview.approval.submitCountMaximum = 2;
assert.throws(() => validateHostedPreview(invalidPreview), /invalid_hosted_preview_approval/);
const partialCallableSurface = structuredClone(singlePreview);
partialCallableSurface.settingsSeal.exposedOptions = { outputFormat: "png" };
partialCallableSurface.settingsSeal.exposedOptionsSha256 =
  hashObject(partialCallableSurface.settingsSeal.exposedOptions);
partialCallableSurface.scopePayload.settingsSealSha256 =
  hashObject(partialCallableSurface.settingsSeal);
partialCallableSurface.previewScopeHash = hashObject(partialCallableSurface.scopePayload);
partialCallableSurface.approval.previewScopeHash = partialCallableSurface.previewScopeHash;
assert.throws(() => validateHostedPreview(partialCallableSurface),
  /hosted_preview_unknown_setting/);
const stageConflict = structuredClone(singlePreview);
stageConflict.scenePromptOriginal = "Generate against a removable solid warm-ivory background for a transparent final background after background removal.";
assert.throws(() => validateHostedPreview(stageConflict),
  /hosted_preview_prompt_stage_semantics_conflict/);

function hostedPreviewPreflightReceiptFixture(scope = singlePreview) {
  return {
    schemaVersion: "generated_media_generation_preflight_receipt_v1",
    authorityCommit: "a".repeat(40),
    requestId: scope.scopePayload.requestId,
    promptRecordSha256: scope.scopePayload.promptRecordSha256,
    promptFileSha256: scope.scopePayload.promptFileSha256,
    providerPromptPayloadHash: scope.scopePayload.providerPromptPayloadHash,
    settingsSealSha256: scope.scopePayload.settingsSealSha256,
    referenceBindingsSha256: hashObject(scope.scopePayload.referenceBindings),
    expressionProfilePayloadHash: "e".repeat(64),
    semanticGateStatus: "valid",
    submitCount: 0,
    retryCount: 0,
  };
}

function submitAdjacentReceiptCheck(receipt, current) {
  assertClosedKeys(receipt, ["schemaVersion", "authorityCommit", "requestId",
    "promptRecordSha256", "promptFileSha256", "providerPromptPayloadHash",
    "settingsSealSha256", "referenceBindingsSha256", "expressionProfilePayloadHash",
    "semanticGateStatus", "submitCount", "retryCount"]);
  if (receipt.authorityCommit !== current.authorityCommit
    || receipt.requestId !== current.requestId
    || receipt.settingsSealSha256 !== current.settingsSealSha256
    || receipt.expressionProfilePayloadHash !== current.expressionProfilePayloadHash) {
    return "fresh_full_validation_required";
  }
  if (receipt.promptRecordSha256 !== current.promptRecordSha256
    || receipt.promptFileSha256 !== current.promptFileSha256
    || receipt.providerPromptPayloadHash !== current.providerPromptPayloadHash) {
    throw new Error("hosted_preview_prompt_drift");
  }
  if (receipt.referenceBindingsSha256 !== current.referenceBindingsSha256) {
    throw new Error("hosted_preview_reference_drift");
  }
  if (current.submitCount >= 1) throw new Error("hosted_preview_submit_limit_exceeded");
  if (current.retryCount > 0) throw new Error("hosted_preview_retry_forbidden");
  return "reused_preflight_receipt";
}

const receipt = hostedPreviewPreflightReceiptFixture();
assert.equal(submitAdjacentReceiptCheck(receipt, structuredClone(receipt)),
  "reused_preflight_receipt");
assert.equal(submitAdjacentReceiptCheck(receipt,
  { ...structuredClone(receipt), authorityCommit: "b".repeat(40) }),
  "fresh_full_validation_required");
assert.throws(() => submitAdjacentReceiptCheck(receipt,
  { ...structuredClone(receipt), promptFileSha256: "f".repeat(64) }),
  /hosted_preview_prompt_drift/);
assert.throws(() => submitAdjacentReceiptCheck(receipt,
  { ...structuredClone(receipt), referenceBindingsSha256: "f".repeat(64) }),
  /hosted_preview_reference_drift/);

const autoPolicy = hostedPreviewAutoPolicyFixture();
assert.equal(validateHostedPreviewAutoPolicy(autoPolicy), true);
const autoAttestation = attestHostedPreview(singlePreview, "exact_single_image", autoPolicy);
assert.equal(autoAttestation.previewScopeHash, singlePreview.previewScopeHash);
assert.equal(autoAttestation.authorizationSourceSha256, hashObject(autoPolicy.authorizationSource));
assert.equal(autoAttestation.submitCountMaximum, 1);
assert.equal(autoAttestation.retryCountMaximum, 0);
assert.equal(validateHostedPreview({ ...singlePreview, approval: autoAttestation }), true);
const unknownAutoPolicy = structuredClone(autoPolicy);
unknownAutoPolicy.unknown = true;
assert.throws(() => validateHostedPreviewAutoPolicy(unknownAutoPolicy), /unknown key/);
const wildcardAutoPolicy = hostedPreviewAutoPolicyFixture();
wildcardAutoPolicy.policyScope.contentIds = ["*"];
wildcardAutoPolicy.policyPayloadSha256 = hashObject({
  schemaVersion: wildcardAutoPolicy.schemaVersion,
  authorizationSource: wildcardAutoPolicy.authorizationSource,
  policyScope: wildcardAutoPolicy.policyScope,
  lifetime: wildcardAutoPolicy.lifetime,
});
wildcardAutoPolicy.policyId = `gmpreviewpolicy1.${wildcardAutoPolicy.policyPayloadSha256.slice(0, 20)}`;
assert.throws(() => validateHostedPreviewAutoPolicy(wildcardAutoPolicy));
assert.throws(() => attestHostedPreview(animationPreview, "exact_animation_request", autoPolicy),
  /hosted_preview_auto_approval_policy_mismatch/);
assert.throws(() => attestHostedPreview(singlePreview, "exact_single_image", autoPolicy, true),
  /hosted_preview_auto_approval_policy_revoked/);

console.log({ fixedScopeHash, fixedApprovalHash: hashObject(approval), scopeCanonical });
console.log("generated media provider execution approval v1 contract vectors: PASS");
