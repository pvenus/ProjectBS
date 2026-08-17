// Closed contract vectors for hosted_builtin_fast_preview_v1.
// This test performs no provider call and writes no workflow artifact.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");
const contract = read(join(guideRoot, "GeneratedMediaImageGenOnlyContractGuide.md"));
const recordGuide = read(join(guideRoot, "GeneratedMediaRecordGuide.md"));
const routingGuide = read(join(guideRoot, "GeneratedMediaRequestRoutingGuide.md"));
const routingPrompt = read(join(guideRoot, "..", "..", "..", "task-prompts",
  "content", "generated-media", "GeneratedMediaRequestRoutingPrompt.md"));

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const hashObject = (value) => sha256(Buffer.from(canonicalJson(value), "utf8"));

const pointerBase = {
  schemaVersion: "generated_media_fast_preview_pointer_v1",
  authoritativeMainSha: "4da1514aaf66109b5519d0bfa9b4d236c89ef464",
  requestId: "gmplan2.character_single_image.character.seojin.1.e5537d6487d06b88f452",
  promptRecordId: "gmprompt3.character_single_image.character.seojin.1.4c37ee9b6e2217168eb2",
  promptRecordSha256: "4d041c99ac1323e9de3b73613c94c9c227c2978eb2a55d282f8a719e817cb3d3",
  referencePath: "AgentDocs/reference-assets/generated-media/style-only/character_single_image/open_ink_wash_dynamic_contour/b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf.png",
  referenceSha256: "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf",
};

function makePointer() {
  const payload = {
    schemaVersion: "generated_media_fast_preview_idempotency_payload_v1",
    authoritativeMainSha: pointerBase.authoritativeMainSha,
    requestId: pointerBase.requestId,
    promptRecordId: pointerBase.promptRecordId,
    promptRecordSha256: pointerBase.promptRecordSha256,
    referencePath: pointerBase.referencePath,
    referenceSha256: pointerBase.referenceSha256,
  };
  return { ...pointerBase,
    idempotencyKey: `gmfastpreview1.${hashObject(payload).slice(0, 20)}` };
}

const hardBlockers = new Set([
  "fast_preview_duplicate_submit_risk",
  "fast_preview_authority_or_safety_violation",
  "fast_preview_callable_input_absent",
]);

function preflight({ pointer = makePointer(), idempotencyState = "absent",
  authorityValid = true, safetyValid = true, prompt = "one executable prompt",
  referenceReadable = true, warnings = [] } = {}) {
  if (["active", "completed", "ambiguous", "dangling", "divergent"].includes(idempotencyState))
    return { state: "blocked", failureType: "fast_preview_duplicate_submit_risk",
      providerCalled: false, submitCount: 0, retryCount: 0 };
  if (!authorityValid || !safetyValid)
    return { state: "blocked", failureType: "fast_preview_authority_or_safety_violation",
      providerCalled: false, submitCount: 0, retryCount: 0 };
  if (!prompt?.trim() || !referenceReadable)
    return { state: "blocked", failureType: "fast_preview_callable_input_absent",
      providerCalled: false, submitCount: 0, retryCount: 0 };
  assert.match(pointer.idempotencyKey, /^gmfastpreview1\.[0-9a-f]{20}$/);
  return { state: "ready", pointer, providerCalled: false, submitCount: 0,
    retryCount: 0, backlogWarnings: [...new Set(warnings)],
    unavailableCallableControls: ["exact_canvas_control", "cost_descriptor"] };
}

function submitOnce(preflightReceipt) {
  if (preflightReceipt.state !== "ready") throw new Error("submit_not_ready");
  return { ...preflightReceipt, state: "submitted", providerCalled: true,
    submitCount: 1, historicalSubmitCount: 1, retryCount: 0 };
}

function terminal(submitted, output) {
  if (submitted.submitCount !== 1 || submitted.retryCount !== 0)
    throw new Error("fast_preview_duplicate_submit_risk");
  if (!output) return { state: "submit_failed_no_retry",
    failureType: "fast_preview_submit_failed_no_retry", providerCalled: true,
    submitCount: 1, historicalSubmitCount: 1, retryCount: 0 };
  return {
    schemaVersion: "generated_media_fast_preview_terminal_receipt_v1",
    state: "preview_complete",
    ...submitted.pointer,
    providerCalled: true,
    submitCount: 1,
    historicalSubmitCount: 1,
    retryCount: 0,
    costKnown: false,
    previewOnly: true,
    notPromotable: true,
    notPreserved: true,
    strictEvaluationPerformed: false,
    unavailableCallableControls: submitted.unavailableCallableControls,
    backlogWarnings: submitted.backlogWarnings,
    outputObservation: output,
    visualEvaluation: {
      scope: "preview_visual_observation_only",
      status: "observed",
      summary: "Preview returned; intent differences are warning-only.",
      intentWarnings: ["canvas_not_provider_attested"],
      adoptedByUser: false,
      strictEvaluationPerformed: false,
    },
    nextStep: "await_user_adoption",
  };
}

// New lane is explicit and its route-owned exception is present on every owner surface.
for (const text of [contract, recordGuide, routingGuide, routingPrompt])
  assert.match(text, /hosted_builtin_fast_preview_v1/);
assert.match(routingGuide, /three and only three pre-submit blocker classes/);
assert.match(routingPrompt, /child final.*parent relay/);
assert.match(contract, /schemaVersion: generated_media_fast_preview_terminal_receipt_v1/);
for (const token of hardBlockers)
  assert.equal(contract.split(token).length >= 3, true);

// Existing strict lane stays fail-closed for unavailable exact settings.
assert.match(contract, /hosted_builtin_preview_v1/);
assert.match(contract, /hosted_preview_unknown_setting/);
assert.match(contract, /does not change `hosted_builtin_preview_v1` or\n+`promotable_generation_v2`/);

// Warnings are non-blocking and one submit produces one inline visual observation.
const ready = preflight({ warnings: ["schema_projection_conflict",
  "capability_attestation_unavailable", "schema_projection_conflict"] });
assert.equal(ready.state, "ready");
assert.deepEqual(ready.backlogWarnings,
  ["schema_projection_conflict", "capability_attestation_unavailable"]);
const submitted = submitOnce(ready);
assert.equal(submitted.submitCount, 1);
assert.equal(submitted.retryCount, 0);
const receipt = terminal(submitted, {
  path: `output/generated-media-fast-preview/v1/character_single_image/character.seojin.1/${ready.pointer.idempotencyKey}/original.png`,
  sha256: "a".repeat(64), byteLength: 1234, mimeType: "image/png",
  width: 1024, height: 1536,
});
assert.equal(receipt.previewOnly, true);
assert.equal(receipt.notPromotable, true);
assert.equal(receipt.notPreserved, true);
assert.equal(receipt.strictEvaluationPerformed, false);
assert.equal(receipt.visualEvaluation.status, "observed");
assert.equal(receipt.costKnown, false);
assert.equal(Object.hasOwn(receipt, "cost"), false);

// Exactly the three hard blockers stop before submit.
for (const [input, expected] of [
  [{ idempotencyState: "active" }, "fast_preview_duplicate_submit_risk"],
  [{ idempotencyState: "completed" }, "fast_preview_duplicate_submit_risk"],
  [{ authorityValid: false }, "fast_preview_authority_or_safety_violation"],
  [{ safetyValid: false }, "fast_preview_authority_or_safety_violation"],
  [{ prompt: "" }, "fast_preview_callable_input_absent"],
  [{ referenceReadable: false }, "fast_preview_callable_input_absent"],
]) {
  const blocked = preflight(input);
  assert.equal(blocked.failureType, expected);
  assert.equal(hardBlockers.has(blocked.failureType), true);
  assert.equal(blocked.providerCalled, false);
  assert.equal(blocked.submitCount, 0);
}

// The provider boundary cannot be crossed twice and failure cannot retry.
assert.throws(() => submitOnce(submitted), /submit_not_ready/);
const failed = terminal(submitted, null);
assert.equal(failed.failureType, "fast_preview_submit_failed_no_retry");
assert.equal(failed.submitCount, 1);
assert.equal(failed.retryCount, 0);

console.log({ executionMode: "hosted_builtin_fast_preview_v1",
  idempotencyKey: ready.pointer.idempotencyKey, testsProviderCalled: false,
  testsSubmitCount: 0 });
console.log("generated media fast preview orchestration contract vectors: PASS");
