// Closed vectors for the actual built-in ImageGen authenticated single-submit
// mode. This test performs no provider call and does not create media/records.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const promptRoot = join(guideRoot, "..", "..", "..", "task-prompts", "content", "generated-media");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");
const guide = read(join(guideRoot,
  "GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md"));

function canonical(value) {
  if (Array.isArray(value)) return `[${value.map(canonical).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonical(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}
const hash = (value) => createHash("sha256").update(canonical(value)).digest("hex");
function assertClosed(value, required, optional = []) {
  const allowed = [...required, ...optional].sort();
  assert.deepEqual(Object.keys(value).sort(), allowed);
}

const callableMatch = guide.match(/```json\s*([\s\S]*?)\s*```/);
assert.ok(callableMatch, "callable schema JSON is missing");
const callableSchema = JSON.parse(callableMatch[1]);
const callableSchemaHash =
  "708b75b05f820870ac165eadcf08d093568944a35d2793e0a7d117bf23646af1";
assert.equal(hash(callableSchema), callableSchemaHash);
assert.deepEqual(callableSchema.exposedMembers, [
  { name: "prompt", required: true, type: "string" },
  { name: "referenced_image_paths", required: false, type: "array_of_paths" },
  { name: "num_last_images_to_include", required: false, type: "integer" },
]);
function validateCallable(candidate) {
  if (hash(candidate) !== callableSchemaHash) {
    throw new Error("builtin_imagegen_callable_schema_drift");
  }
  return true;
}
assert.equal(validateCallable(callableSchema), true);
assert.throws(() => validateCallable({ ...callableSchema,
  exposedMembers: [...callableSchema.exposedMembers,
    { name: "size", required: false, type: "string" }] }),
  /builtin_imagegen_callable_schema_drift/);

const profileKey = "projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0";
const profileHash = "b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a";
const fixed = {
  authorityMainSha: "a".repeat(40),
  requestId: "gmplan2.character_single_image.character.seojin.2.b7476e702e9816e3c644",
  promptRecordId: "gmprompt3.character_single_image.character.seojin.2.vector",
  promptRecordSha256: "b".repeat(64),
  promptMarkdownSha256: "c".repeat(64),
  generationHandoffSha256: "dec4d883e306fa28da7fa780179858d1b6c93ff3311bb7f4398b84c0e1dbe31b",
  providerPromptPayloadHash: "d".repeat(64),
  expressionProfileKey: profileKey,
  expressionProfilePayloadHash: profileHash,
};

function callProjection({ referenceMode = "none", paths,
  numLastImagesToInclude } = {}) {
  const projection = { promptSha256: "e".repeat(64), referenceMode };
  if (paths !== undefined) projection.referencedImagePaths = paths;
  if (numLastImagesToInclude !== undefined) {
    projection.numLastImagesToInclude = numLastImagesToInclude;
  }
  if (paths !== undefined && numLastImagesToInclude !== undefined) {
    throw new Error("builtin_imagegen_reference_projection_mismatch");
  }
  const keys = ["promptSha256", "referenceMode"];
  if (referenceMode === "none") {
    assertClosed(projection, keys);
  } else if (referenceMode === "referenced_image_paths") {
    assertClosed(projection, [...keys, "referencedImagePaths"]);
    assert.ok(Array.isArray(paths) && paths.length > 0);
  } else if (referenceMode === "num_last_images_to_include") {
    assertClosed(projection, [...keys, "numLastImagesToInclude"]);
    assert.ok(Number.isInteger(numLastImagesToInclude)
      && numLastImagesToInclude >= 1 && numLastImagesToInclude <= 5);
  } else {
    throw new Error("builtin_imagegen_reference_projection_mismatch");
  }
  return projection;
}

const settingsIntent = {
  canvas: { height: 1536, width: 1024 },
  generationBackground: { color: "#00FF00", mode: "removable_solid" },
  outputFormat: "png",
};
function derive(reference = callProjection()) {
  const preflight = {
    schemaVersion: "generated_media_builtin_imagegen_preflight_v1",
    executionMode: "builtin_imagegen_authenticated_single_submit_v1",
    ...fixed,
    callableSchema: structuredClone(callableSchema),
    callableSchemaSha256: callableSchemaHash,
    callProjection: structuredClone(reference),
    callProjectionSha256: hash(reference),
    providerSettingsIntent: structuredClone(settingsIntent),
    providerSettingsIntentSha256: hash(settingsIntent),
    controlCoverage: {
      canvas: "prompt_bound_not_callable",
      generationBackground: "prompt_bound_not_callable",
      outputFormat: "prompt_bound_not_callable",
    },
    capabilityDescriptorStatus: "unavailable_not_exposed",
    settingsDescriptorStatus: "unavailable_not_exposed",
    costEstimate: { status: "unavailable_not_exposed" },
  };
  const scope = {
    schemaVersion: "generated_media_builtin_imagegen_execution_scope_v1",
    executionMode: preflight.executionMode,
    ...fixed,
    callableSchemaSha256: preflight.callableSchemaSha256,
    callProjectionSha256: preflight.callProjectionSha256,
    providerSettingsIntentSha256: preflight.providerSettingsIntentSha256,
    submitCountMaximum: 1,
    retryCountMaximum: 0,
  };
  return { preflight, scope, executionScopeHash: hash(scope),
    idempotencyKey: `gmbuiltin1.${hash(scope).slice(0, 20)}` };
}

const first = derive();
const second = derive();
assert.equal(hash(first.preflight), hash(second.preflight));
assert.equal(first.executionScopeHash, second.executionScopeHash);
assert.equal(first.idempotencyKey, second.idempotencyKey);
assert.deepEqual(first.preflight.controlCoverage, {
  canvas: "prompt_bound_not_callable",
  generationBackground: "prompt_bound_not_callable",
  outputFormat: "prompt_bound_not_callable",
});
assert.deepEqual(first.preflight.costEstimate, { status: "unavailable_not_exposed" });
function validateEvidenceClaims(preflight) {
  if (preflight.capabilityDescriptorStatus !== "unavailable_not_exposed"
      || preflight.settingsDescriptorStatus !== "unavailable_not_exposed"
      || canonical(preflight.costEstimate)
        !== canonical({ status: "unavailable_not_exposed" })) {
    throw new Error("builtin_imagegen_preflight_projection_mismatch");
  }
  return true;
}
assert.equal(validateEvidenceClaims(first.preflight), true);
assert.throws(() => validateEvidenceClaims({ ...first.preflight,
  capabilityDescriptorStatus: "supported" }),
  /builtin_imagegen_preflight_projection_mismatch/);
assert.throws(() => validateEvidenceClaims({ ...first.preflight,
  costEstimate: { status: "exact", amount: "0" } }),
  /builtin_imagegen_preflight_projection_mismatch/);

function authorize(derived, overrides = {}) {
  const approval = {
    schemaVersion: "generated_media_builtin_imagegen_authenticated_approval_v1",
    executionMode: "builtin_imagegen_authenticated_single_submit_v1",
    approvedBy: "authenticated_current_user",
    approvedAt: "2026-08-22T10:00:00+09:00",
    approvalEvidence: "codex-thread:authenticated-exact-request",
    executionScopeHash: derived.executionScopeHash,
    submitCountMaximum: 1,
    retryCountMaximum: 0,
    unavailableEvidenceAcceptance: "capability_settings_and_cost_not_exposed",
    ...overrides,
  };
  assertClosed(approval, ["schemaVersion", "executionMode", "approvedBy",
    "approvedAt", "approvalEvidence", "executionScopeHash",
    "submitCountMaximum", "retryCountMaximum", "unavailableEvidenceAcceptance"]);
  if (approval.executionScopeHash !== derived.executionScopeHash
      || approval.submitCountMaximum !== 1 || approval.retryCountMaximum !== 0
      || approval.unavailableEvidenceAcceptance
        !== "capability_settings_and_cost_not_exposed") {
    throw new Error("builtin_imagegen_authenticated_approval_invalid");
  }
  return approval;
}
assert.ok(authorize(first));
assert.throws(() => authorize(first, { executionScopeHash: "f".repeat(64) }),
  /builtin_imagegen_authenticated_approval_invalid/);
assert.throws(() => authorize(first, { submitCountMaximum: 2 }),
  /builtin_imagegen_authenticated_approval_invalid/);
assert.throws(() => authorize(first, { retryCountMaximum: 1 }),
  /builtin_imagegen_authenticated_approval_invalid/);
assert.throws(() => callProjection({ referenceMode: "referenced_image_paths",
  paths: ["approved.png"], numLastImagesToInclude: 1 }),
  /builtin_imagegen_reference_projection_mismatch/);
assert.throws(() => callProjection({ referenceMode: "num_last_images_to_include",
  numLastImagesToInclude: 6 }), /AssertionError/);

function submissionDecision({ active = false, completed = false,
  submitCount = 0, retryCount = 0 } = {}) {
  if (active || completed || submitCount > 0) {
    throw new Error("duplicate_provider_call_risk");
  }
  if (retryCount > 0) throw new Error("builtin_imagegen_authenticated_approval_invalid");
  return "submit_once";
}
assert.equal(submissionDecision(), "submit_once");
assert.throws(() => submissionDecision({ active: true }), /duplicate_provider_call_risk/);
assert.throws(() => submissionDecision({ completed: true }), /duplicate_provider_call_risk/);
assert.throws(() => submissionDecision({ submitCount: 1 }), /duplicate_provider_call_risk/);
assert.throws(() => submissionDecision({ retryCount: 1 }),
  /builtin_imagegen_authenticated_approval_invalid/);

function outputConformance(output) {
  if (output.format !== "png" || output.width !== 1024 || output.height !== 1536
      || !output.fullyOpaque) return "open_ink_chroma_provider_master_nonopaque";
  if (!output.uniformOutsideForeground || output.outsideRgb !== "#00FF00") {
    return "open_ink_chroma_provider_master_field_nonuniform";
  }
  if (output.foregroundContainsExactKey) {
    return "open_ink_chroma_provider_master_foreground_key_collision";
  }
  if (output.forbiddenFeatures.length > 0) {
    return "open_ink_chroma_provider_master_forbidden_feature";
  }
  return "provider_master_complete";
}
const conforming = { format: "png", width: 1024, height: 1536,
  fullyOpaque: true, uniformOutsideForeground: true, outsideRgb: "#00FF00",
  foregroundContainsExactKey: false, forbiddenFeatures: [] };
assert.equal(outputConformance(conforming), "provider_master_complete");
assert.equal(outputConformance({ ...conforming, outsideRgb: "#F2EFE6" }),
  "open_ink_chroma_provider_master_field_nonuniform");
assert.equal(outputConformance({ ...conforming, fullyOpaque: false }),
  "open_ink_chroma_provider_master_nonopaque");
assert.equal(outputConformance({ ...conforming, foregroundContainsExactKey: true }),
  "open_ink_chroma_provider_master_foreground_key_collision");
assert.equal(outputConformance({ ...conforming, forbiddenFeatures: ["checkerboard"] }),
  "open_ink_chroma_provider_master_forbidden_feature");

const surfaces = [
  join(guideRoot, "GeneratedMediaImageGenOnlyContractGuide.md"),
  join(guideRoot, "GeneratedMediaRecordGuide.md"),
  join(guideRoot, "ImageGenCharacterImagePipelineGuide.md"),
  join(guideRoot, "GeneratedMediaNoninteractiveExecutionPolicyGuide.md"),
  join(promptRoot, "ImageGenCharacterImageGenerationPrompt.md"),
];
for (const surface of surfaces) {
  const text = read(surface);
  assert.ok(text.includes("builtin_imagegen_authenticated_single_submit_v1"), surface);
}
for (const surface of [surfaces[0], surfaces[2], surfaces[4]]) {
  assert.ok(read(surface).includes(profileKey), surface);
}

const strictGuide = read(join(guideRoot, "GeneratedMediaImageGenOnlyContractGuide.md"));
assert.ok(strictGuide.includes("providerInterface: configured_imagegen_capability"));
assert.ok(strictGuide.includes("hosted_builtin_preview_v1"));
assert.ok(strictGuide.includes("hosted_builtin_fast_preview_v1"));
assert.ok(guide.includes("configured_imagegen_capability` remains the only authority"));
assert.match(guide, /does not make an existing preview or\s+direct-alpha chain reusable/);

const existingGenerationHandoffs = [
  "dec4d883e306fa28da7fa780179858d1b6c93ff3311bb7f4398b84c0e1dbe31b",
  "cecafba09e19e5da61afd275f519a002ed41cf3cc037f4ae3d647ca21bc0c0d5",
];
for (const handoffSha256 of existingGenerationHandoffs) {
  assert.match(handoffSha256, /^[0-9a-f]{64}$/);
}
assert.ok(guide.includes("Existing planning, routing, prompt, profile, preview, and strict-generation\nrecords remain byte-immutable"));

console.log("Generated Media built-in ImageGen authenticated generation contract: PASS");
