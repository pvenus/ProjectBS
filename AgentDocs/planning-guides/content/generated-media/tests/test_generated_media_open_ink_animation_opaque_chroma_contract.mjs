// Additive character_animation_v2 opaque-chroma identity/motion/postprocess vectors.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { canonicalJson } from
  "../helpers/generated_media_canonical_serializers_v1.mjs";
import { EXPRESSION_PROFILE_KEY, EXECUTION_PROFILE_KEY, PROFILE_PAYLOAD_SHA256,
  IDENTITY_SCHEMA, MOTION_SCHEMA, POSTPROCESS_SCHEMA, buildProviderCall,
  executionScope, validateIdentitySelection, validateMotionSelection,
  validatePostprocessReceipt, validateProfile, validateProjection } from
  "../helpers/generated_media_open_ink_animation_opaque_chroma_contract_v1.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const generatedMedia = join(here, ".."); const helpers = join(generatedMedia, "helpers");
const repo = join(generatedMedia, "..", "..", "..", "..");
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const profile = JSON.parse(readFileSync(join(helpers,
  "generated_media_open_ink_animation_opaque_chroma_profile_v1.json"), "utf8"));

assert.equal(validateProfile(profile), profile);
assert.equal(profile.expressionProfileKey, EXPRESSION_PROFILE_KEY);
assert.equal(profile.executionProfileKey, EXECUTION_PROFILE_KEY);
assert.equal(sha(Buffer.from(canonicalJson(profile), "utf8")), PROFILE_PAYLOAD_SHA256);
assert.equal(PROFILE_PAYLOAD_SHA256,
  "da38a4c91bbe3a808f09f1c24763cd3cece02518a2d1398f7294ce3eedb3f7c8");
assert.equal(profile.baseStyleBinding.expressionProfilePayloadHash,
  "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5");
assert.deepEqual(profile.registryApplicability, { assetType: "animation",
  domainType: "character", projectCopyEligibleBeforeEvaluationPass: false,
  structureProfile: "character_animation_v2" });
assert.deepEqual(profile.animationMasterContract.canvas, { height: 1024, width: 1536 });
assert.deepEqual(profile.animationMasterContract.cell, { height: 512, width: 512 });
assert.deepEqual(profile.animationMasterContract.background.rgb, [0, 255, 0]);
assert.equal(profile.postprocessContract.schemaVersion, POSTPROCESS_SCHEMA);
assert.deepEqual(profile.postprocessContract.normalization.root, { x: 256, y: 300 });
assert.equal(profile.postprocessContract.normalization.baselineY, 448);
assert.deepEqual(profile.postprocessContract.gif, { durationTotalMs: 900,
  frameCount: 6, frameDelayMs: 150, frameOrder: [0, 1, 2, 3, 4, 5],
  loop: "infinite", reopenValidation: true });

const fixture = profile.animationIdentityAuthorityContract.fixtureBindings[0];
const identity = { schemaVersion: IDENTITY_SCHEMA, contentId: fixture.contentId,
  localPath: fixture.localPath,
  pathPolicy: profile.animationIdentityAuthorityContract.pathPolicy,
  sha256: fixture.sha256, byteLength: fixture.byteLength,
  referenceRole: "identity_equipment_orientation_authority_only",
  trustedEvidencePolicyKey: "generated_media_trusted_local_evaluated_main_reference@1.0.0",
  evaluationRecordId: "eval.character.main.g2", evaluationRecordSha256: "a".repeat(64),
  executionProfileKey: EXECUTION_PROFILE_KEY,
  executionProfilePayloadHash: PROFILE_PAYLOAD_SHA256 };
const observed = { path: fixture.localPath, sha256: fixture.sha256,
  byteLength: fixture.byteLength };
assert.equal(validateIdentitySelection(profile, identity, observed), identity);

const motion = { schemaVersion: MOTION_SCHEMA,
  sourceContentId: "character.seojin.1",
  sourceAnimationRequestId: "character.seojin.1.movement.running.loop.v1",
  lineageRecordId: "lineage.grade1.running", lineageRecordPath: "trusted/grade1.json",
  lineageRecordSha256: "b".repeat(64), evaluationRecordId: "eval.grade1.running",
  evaluationRecordSha256: "c".repeat(64), referenceRole: "motion_topology_only",
  providerReferenceAllowed: false, pixelTransferAllowed: false };
assert.equal(validateMotionSelection(profile, motion), motion);
const projection = { animationIdentityAuthoritySelection: identity,
  animationMotionLineageSelection: motion };
assert.equal(validateProjection(profile, { planningHandoff: projection,
  routingRecord: structuredClone(projection), normalizedRequest: structuredClone(projection),
  authoringHandoff: structuredClone(projection), promptRecord: structuredClone(projection),
  generationHandoff: structuredClone(projection), observedIdentity: observed }), true);

assert.deepEqual(buildProviderCall("closed prompt", identity),
  { prompt: "closed prompt", referenced_image_paths: [fixture.localPath] });
const scopeInput = { authorityMainSha: "d".repeat(40), requestId: "gmplan2.animation.g2",
  animationRequestId: "character.seojin.2.movement.sample.v1",
  promptRecordId: "gmprompt3.animation.g2", promptRecordSha256: "e".repeat(64),
  animationIdentityAuthoritySelection: identity,
  animationMotionLineageSelection: motion, callProjectionSha256: "f".repeat(64) };
const first = executionScope(profile, scopeInput); const second = executionScope(profile, scopeInput);
assert.deepEqual(first, second);
assert.match(first.idempotencyKey, /^gmanimidentity1\.[0-9a-f]{20}$/);

const receipt = { schemaVersion:
  "generated_media_animation_opaque_chroma_postprocess_receipt_v1",
  state: "postprocess_complete", animationRequestId: scopeInput.animationRequestId,
  generationRecordSha256: "1".repeat(64), providerMasterSha256: "2".repeat(64),
  expressionProfileKey: EXPRESSION_PROFILE_KEY,
  expressionProfilePayloadHash: PROFILE_PAYLOAD_SHA256,
  splitEvidence: {}, orderedCellSha256s: Array.from({ length: 6 },
    (_, i) => `${i + 1}`.repeat(64)),
  orderedFramePngSha256s: Array.from({ length: 6 }, (_, i) => `${i}`.repeat(64)),
  completedGifSha256: "9".repeat(64), root: { x: 256, y: 300 }, baselineY: 448,
  rootDriftMaxPx: 0, baselineDriftMaxPx: 0, scaleFixed: true, cameraFixed: true,
  centroidFixed: true, safeMarginPx: 48, transparentRgbZero: true,
  prohibitedFringeCount: 0, fragmentCount: 0, duplicateFrameHashCount: 0,
  repeatedMotionPhaseCount: 0, wholeBodyMirrorDetected: false,
  frame5To0ClosureValid: true, gifClosedAndReopened: true,
  timelineEvidence: { frameCount: 6, delayMs: 150, totalMs: 900, loop: "infinite" },
  providerCalled: false, submitCount: 0, retryCount: 0,
  evaluationStatus: "not_evaluated", projectCopyEligible: false };
assert.equal(validatePostprocessReceipt(profile, receipt), receipt);

const profileDrift = structuredClone(profile); profileDrift.animationMasterContract.rows = 3;
assert.throws(() => validateProfile(profileDrift),
  /open_ink_animation_opaque_chroma_profile_mismatch/);
assert.throws(() => validateIdentitySelection(profile,
  { ...identity, referenceRole: "edit_source" }, observed),
  /animation_identity_authority_fixture_mismatch/);
assert.throws(() => validateIdentitySelection(profile, identity,
  { ...observed, sha256: "0".repeat(64) }),
  /animation_identity_authority_fixture_mismatch/);
assert.throws(() => validateMotionSelection(profile,
  { ...motion, providerReferenceAllowed: true }),
  /animation_motion_lineage_projection_mismatch/);
const mixed = structuredClone(projection);
mixed.animationIdentityAuthoritySelection.contentId = "character.seojin.3";
assert.throws(() => validateProjection(profile, { planningHandoff: projection,
  routingRecord: mixed, normalizedRequest: projection, authoringHandoff: projection,
  promptRecord: projection, generationHandoff: projection, observedIdentity: observed }),
  /animation_identity_authority_fixture_mismatch|animation_identity_motion_projection_mismatch/);
assert.throws(() => validatePostprocessReceipt(profile,
  { ...receipt, duplicateFrameHashCount: 1 }),
  /animation_opaque_chroma_postprocess_evidence_mismatch/);
assert.throws(() => validatePostprocessReceipt(profile,
  { ...receipt, wholeBodyMirrorDetected: true }),
  /animation_opaque_chroma_postprocess_evidence_mismatch/);
assert.throws(() => validatePostprocessReceipt(profile,
  { ...receipt, rootDriftMaxPx: 1 }),
  /animation_opaque_chroma_postprocess_evidence_mismatch/);

for (const surface of [
  "AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md",
  "AgentDocs/task-prompts/character/ActCharacterPlanningPrompts.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/ImageGenAnimationPipelineGuide.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaChromaUncompositePrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md",
]) {
  const text = readFileSync(join(repo, surface), "utf8").replaceAll("\r\n", "\n");
  assert.ok(text.includes(EXPRESSION_PROFILE_KEY), `${surface}: expression profile`);
  assert.ok(text.includes(PROFILE_PAYLOAD_SHA256), `${surface}: payload hash`);
}

assert.ok(readFileSync(join(repo,
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashOpaqueChromaSuccessorGuide.md"),
"utf8").includes("b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a"));
assert.ok(readFileSync(join(repo,
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashAttackMotionSuccessorGuide.md"),
"utf8").includes("07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c"));

console.log({ expressionProfileKey: EXPRESSION_PROFILE_KEY,
  executionProfileKey: EXECUTION_PROFILE_KEY,
  profilePayloadSha256: PROFILE_PAYLOAD_SHA256, identitySchema: IDENTITY_SCHEMA,
  motionSchema: MOTION_SCHEMA, postprocessSchema: POSTPROCESS_SCHEMA,
  providerCalled: false, submitCount: 0, cost: 0 });
console.log("generated media open-ink animation opaque-chroma contract: PASS");
