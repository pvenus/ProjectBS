// Exact-source MAIN completion authority vectors. No registered source is
// transformed and no provider/media/record/evaluation/promotion action occurs.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import {
  FIT_ALGORITHM_KEY,
  FIT_ALGORITHM_VERSION,
  PROFILE_KEY as FIT_PROFILE_KEY,
  PROFILE_PAYLOAD_SHA256 as FIT_PROFILE_HASH,
  placeOnCanvas,
  resizePremultipliedBox,
  validateProfile,
} from "../helpers/generated_media_source_bound_chroma_fit_v1.mjs";
import { canonicalJson } from
  "../helpers/generated_media_canonical_serializers_v1.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, "..");
const repo = join(root, "..", "..", "..", "..");
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const jcsSha = (value) => sha(Buffer.from(canonicalJson(value), "utf8"));
const load = (name) => JSON.parse(readFileSync(join(root, "helpers", name), "utf8"));

const fit = load("generated_media_source_bound_chroma_fit_profile_v1.json");
const edit = load("generated_media_source_bound_character_edit_profile_v1.json");
assert.equal(fit.profileKey, FIT_PROFILE_KEY);
assert.equal(jcsSha(fit), FIT_PROFILE_HASH);
assert.equal(FIT_PROFILE_HASH,
  "ca3102e7369da6513b4c4e462e68b373da618a2b1630a393d803d37136d812df");
assert.equal(FIT_ALGORITHM_KEY, "generated_media_premultiplied_box_foreground_fit_v1");
assert.equal(FIT_ALGORITHM_VERSION, "1.0.0");
assert.equal(validateProfile(fit), fit);
const fitDrift = structuredClone(fit);
fitDrift.algorithmContract.fit.placement.x += 1;
assert.throws(() => validateProfile(fitDrift), /source_chroma_fit_profile_hash_mismatch/);

assert.deepEqual({ source: fit.sourceBinding.sourceSha256,
  receipt: fit.sourceBinding.generationReceiptSha256,
  bbox: fit.sourceBinding.foregroundEvidence.bbox,
  scale: fit.algorithmContract.fit.scaleRational,
  target: fit.algorithmContract.fit.targetForegroundBbox }, {
  source: "66dc1c94be2e38e9dc4d6ff15b4b6b0699353b9830d718ec16743dc4ff92acf9",
  receipt: "4e4457df58f31eff61adb1a93d8915e8a2fb0926e28cb9eb8358f2ce8b606526",
  bbox: { xMin: 226, yMin: 90, xMax: 843, yMax: 1472 },
  scale: { numerator: 1249, denominator: 1383 },
  target: { xMin: 233, yMin: 128, xMax: 790, yMax: 1376 },
});
assert.deepEqual(fit.algorithmContract.operationOrder,
  ["exact_source_validation", "border_calibrated_green_uncomposite_v1",
    "premultiplied_box_foreground_fit_v1", "canonical_png_rgba8_serialization",
    "reopen_validation"]);
assert.equal(fit.outputContract.noManualMaskOrSemanticEdit, true);
assert.equal(fit.generationProfileBinding.generationSemanticsUnchanged, true);

// Exact integer-area premultiplied reduction: opaque 2x2 -> one averaged pixel.
const sourceRgba = Buffer.from([
  0, 0, 0, 255, 100, 0, 0, 255,
  0, 100, 0, 255, 100, 100, 0, 255,
]);
const resized = resizePremultipliedBox({ rgba: sourceRgba, sourceWidth: 2,
  sourceHeight: 2, sourceBbox: { xMin: 0, yMin: 0, xMax: 1, yMax: 1 },
  targetWidth: 1, targetHeight: 1 });
assert.deepEqual([...resized], [50, 50, 0, 255]);
const canvas = placeOnCanvas({ resizedRgba: resized, targetWidth: 1,
  targetHeight: 1, canvasWidth: 3, canvasHeight: 3, x: 1, y: 1 });
assert.deepEqual([...canvas.subarray(16, 20)], [50, 50, 0, 255]);
assert.equal(canvas.filter((value, index) => index % 4 === 3 && value !== 0).length, 1);
assert.throws(() => placeOnCanvas({ resizedRgba: resized, targetWidth: 1,
  targetHeight: 1, canvasWidth: 1, canvasHeight: 1, x: 1, y: 0 }),
  /source_chroma_fit_canvas_overflow/);

const EDIT_KEY = "projectbs_character_open_ink_source_bound_single_edit@1.0.0";
const EDIT_HASH = "aa65434f5fb9c22cb42db199c936ee414648b933f4b83c159065341f4e704011";
assert.equal(edit.profileKey, EDIT_KEY);
assert.equal(jcsSha(edit), EDIT_HASH);
assert.deepEqual({ source: edit.sourceBinding.sourceSha256,
  receipt: edit.sourceBinding.generationReceiptSha256,
  observed: edit.editContract.sourceObservation.observedBrassCircularClosures,
  required: edit.outputContract.requiredBrassClosures,
  submits: edit.executionContract.submitCountMaximum,
  retries: edit.executionContract.retryCountMaximum }, {
  source: "d435d0a6e5a7de4e7c50cd4e2552145eaa1eb8310d8874b37ed1e1a5a4c82c3d",
  receipt: "64457ef0c95045452745f167dbd42e024bf6c3d97cabb25dc65286c6b5cd6db5",
  observed: 2, required: 1, submits: 1, retries: 0,
});
assert.equal(edit.callableContract.callableSchemaSha256,
  "708b75b05f820870ac165eadcf08d093568944a35d2793e0a7d117bf23646af1");
assert.equal(edit.callableContract.referencedImageCount, 1);
assert.equal(edit.editContract.providerPromptLines.length, 6);
assert.match(edit.editContract.providerPromptLines.join("\n"),
  /Remove only the extra smaller brass closure on screen-left/);
assert.match(edit.editContract.providerPromptLines.join("\n"),
  /retain exactly one brass closure/);
for (const lock of ["face", "sword", "scabbard", "whole_body_mirror", "source_mutation"])
  assert.ok(edit.editContract.forbiddenChanges.includes(lock));
assert.equal(edit.postprocessBoundary.uncompositeInEditStage, false);

const routeMembers = ["schemaVersion", "routeId", "authorityMainSha", "requestId",
  "contentId", "sourcePathEvidence", "sourceSha256", "generationReceiptPathEvidence",
  "generationReceiptSha256", "generationHandoffSha256", "profileKey",
  "profilePayloadSha256", "callableSchemaSha256", "providerPromptLines",
  "approvalEvidence", "executionScopeHash", "idempotencyKey", "submitCountMaximum",
  "retryCountMaximum", "outputContract", "state", "createdAt"];
function validateEditRoute(route) {
  if (canonicalJson(Object.keys(route).sort()) !== canonicalJson([...routeMembers].sort()))
    throw new Error("source_bound_edit_route_projection_mismatch");
  if (route.sourceSha256 !== edit.sourceBinding.sourceSha256
    || route.generationReceiptSha256 !== edit.sourceBinding.generationReceiptSha256)
    throw new Error("source_bound_edit_binding_not_registered");
  if (canonicalJson(route.providerPromptLines)
    !== canonicalJson(edit.editContract.providerPromptLines))
    throw new Error("source_bound_edit_prompt_mismatch");
  if (route.submitCountMaximum !== 1) throw new Error("source_bound_edit_submit_limit_exceeded");
  if (route.retryCountMaximum !== 0) throw new Error("source_bound_edit_retry_forbidden");
  return true;
}
const route = Object.fromEntries(routeMembers.map((key) => [key, "fixture"]));
Object.assign(route, { sourceSha256: edit.sourceBinding.sourceSha256,
  generationReceiptSha256: edit.sourceBinding.generationReceiptSha256,
  providerPromptLines: edit.editContract.providerPromptLines,
  submitCountMaximum: 1, retryCountMaximum: 0 });
assert.equal(validateEditRoute(route), true);
assert.throws(() => validateEditRoute({ ...route, unknown: true }),
  /source_bound_edit_route_projection_mismatch/);
assert.throws(() => validateEditRoute({ ...route, sourceSha256: "0".repeat(64) }),
  /source_bound_edit_binding_not_registered/);
assert.throws(() => validateEditRoute({ ...route, providerPromptLines: ["different"] }),
  /source_bound_edit_prompt_mismatch/);
assert.throws(() => validateEditRoute({ ...route, submitCountMaximum: 2 }),
  /source_bound_edit_submit_limit_exceeded/);
assert.throws(() => validateEditRoute({ ...route, retryCountMaximum: 1 }),
  /source_bound_edit_retry_forbidden/);

const surfaces = [
  ["AgentDocs/planning-guides/content/generated-media/GeneratedMediaSourceBoundMainCompletionGuide.md", true, true],
  ["AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md", true, true],
  ["AgentDocs/planning-guides/content/generated-media/GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md", false, true],
  ["AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md", true, true],
  ["AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md", true, false],
  ["AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md", true, false],
  ["AgentDocs/task-prompts/content/generated-media/GeneratedMediaChromaUncompositePrompt.md", true, false],
  ["AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md", true, false],
  ["AgentDocs/task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md", true, false],
  ["AgentDocs/task-prompts/content/generated-media/ImageGenCharacterSourceBoundEditGenerationPrompt.md", false, true],
];
for (const [surface, requiresFit, requiresEdit] of surfaces) {
  const text = readFileSync(join(repo, surface), "utf8").replaceAll("\r\n", "\n");
  if (requiresFit) {
    assert.ok(text.includes(FIT_PROFILE_KEY), `${surface}: fit key`);
    assert.ok(text.includes(FIT_PROFILE_HASH), `${surface}: fit hash`);
  }
  if (requiresEdit) {
    assert.ok(text.includes(EDIT_KEY), `${surface}: edit key`);
    assert.ok(text.includes(EDIT_HASH), `${surface}: edit hash`);
  }
}

const oldProfile = load("generated_media_source_bound_chroma_recovery_profile_v1.json");
assert.equal(jcsSha(oldProfile),
  "b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746");
console.log({ fitProfileKey: FIT_PROFILE_KEY, fitProfilePayloadSha256: FIT_PROFILE_HASH,
  editProfileKey: EDIT_KEY, editProfilePayloadSha256: EDIT_HASH,
  providerCalled: false, submitCount: 0 });
console.log("generated media source-bound MAIN completion contract: PASS");
