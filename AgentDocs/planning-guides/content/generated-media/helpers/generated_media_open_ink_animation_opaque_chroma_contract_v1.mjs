import { createHash } from "node:crypto";
import { canonicalJson } from "./generated_media_canonical_serializers_v1.mjs";

export const EXPRESSION_PROFILE_KEY =
  "projectbs_character_open_ink_wash_animation_opaque_chroma_master@1.0.0";
export const EXECUTION_PROFILE_KEY =
  "projectbs_character_open_ink_animation_opaque_chroma_identity_anchored@1.0.0";
export const PROFILE_PAYLOAD_SHA256 =
  "da38a4c91bbe3a808f09f1c24763cd3cece02518a2d1398f7294ce3eedb3f7c8";
export const IDENTITY_SCHEMA =
  "generated_media_animation_identity_equipment_authority_selection_v1";
export const MOTION_SCHEMA = "generated_media_animation_motion_lineage_selection_v1";
export const POSTPROCESS_SCHEMA =
  "generated_media_animation_opaque_chroma_postprocess_selection_v1";

const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const sorted = (values) => [...values].sort();

export function assertClosedKeys(value, required, token) {
  if (!value || typeof value !== "object" || Array.isArray(value)
    || canonicalJson(sorted(Object.keys(value))) !== canonicalJson(sorted(required)))
    throw new Error(token);
}

export function validateProfile(profile) {
  if (profile.expressionProfileKey !== EXPRESSION_PROFILE_KEY
    || profile.executionProfileKey !== EXECUTION_PROFILE_KEY
    || sha(Buffer.from(canonicalJson(profile), "utf8")) !== PROFILE_PAYLOAD_SHA256)
    throw new Error("open_ink_animation_opaque_chroma_profile_mismatch");
  return profile;
}

export function validateIdentitySelection(profile, selection, observed) {
  const contract = profile.animationIdentityAuthorityContract;
  assertClosedKeys(selection, contract.requiredSelectionMembers,
    "animation_identity_authority_selection_missing");
  const fixture = contract.fixtureBindings.find((entry) =>
    entry.contentId === selection.contentId);
  if (!fixture || selection.schemaVersion !== contract.schemaVersion
    || selection.localPath !== fixture.localPath || selection.sha256 !== fixture.sha256
    || selection.byteLength !== fixture.byteLength
    || selection.pathPolicy !== contract.pathPolicy
    || selection.referenceRole !== contract.referenceRole
    || selection.trustedEvidencePolicyKey !== contract.trustedEvidencePolicyKey
    || selection.executionProfileKey !== EXECUTION_PROFILE_KEY
    || selection.executionProfilePayloadHash !== PROFILE_PAYLOAD_SHA256)
    throw new Error("animation_identity_authority_fixture_mismatch");
  if (!observed || observed.path !== fixture.localPath
    || observed.sha256 !== fixture.sha256 || observed.byteLength !== fixture.byteLength)
    throw new Error("animation_identity_authority_fixture_mismatch");
  if (!selection.evaluationRecordId || !/^[0-9a-f]{64}$/.test(
    selection.evaluationRecordSha256))
    throw new Error("animation_identity_authority_evaluation_mismatch");
  return selection;
}

export function validateMotionSelection(profile, selection) {
  const contract = profile.motionLineageContract;
  assertClosedKeys(selection, contract.requiredSelectionMembers,
    "animation_motion_lineage_selection_missing");
  if (selection.schemaVersion !== contract.schemaVersion
    || selection.referenceRole !== contract.referenceRole
    || selection.providerReferenceAllowed !== false
    || selection.pixelTransferAllowed !== false
    || !selection.sourceContentId || !selection.sourceAnimationRequestId
    || !selection.lineageRecordId || !selection.lineageRecordPath
    || !/^[0-9a-f]{64}$/.test(selection.lineageRecordSha256)
    || !selection.evaluationRecordId
    || !/^[0-9a-f]{64}$/.test(selection.evaluationRecordSha256))
    throw new Error("animation_motion_lineage_projection_mismatch");
  return selection;
}

export function validateProjection(profile, projections) {
  const fields = ["planningHandoff", "routingRecord", "normalizedRequest",
    "authoringHandoff", "promptRecord", "generationHandoff"];
  for (const field of fields) {
    if (!projections[field])
      throw new Error("animation_identity_motion_projection_mismatch");
    validateIdentitySelection(profile,
      projections[field].animationIdentityAuthoritySelection, projections.observedIdentity);
    validateMotionSelection(profile,
      projections[field].animationMotionLineageSelection);
  }
  const identity = canonicalJson(projections.planningHandoff
    .animationIdentityAuthoritySelection);
  const motion = canonicalJson(projections.planningHandoff
    .animationMotionLineageSelection);
  if (fields.some((field) => canonicalJson(projections[field]
      .animationIdentityAuthoritySelection) !== identity
    || canonicalJson(projections[field].animationMotionLineageSelection) !== motion))
    throw new Error("animation_identity_motion_projection_mismatch");
  return true;
}

export function buildProviderCall(prompt, identitySelection) {
  if (typeof prompt !== "string" || prompt.length === 0)
    throw new Error("animation_opaque_chroma_provider_prompt_missing");
  return { prompt, referenced_image_paths: [identitySelection.localPath] };
}

export function executionScope(profile, input) {
  assertClosedKeys(input, ["authorityMainSha", "requestId", "animationRequestId",
    "promptRecordId", "promptRecordSha256", "animationIdentityAuthoritySelection",
    "animationMotionLineageSelection", "callProjectionSha256"],
  "animation_opaque_chroma_execution_scope_mismatch");
  const payload = { schemaVersion:
    "generated_media_animation_opaque_chroma_execution_scope_v1",
  authorityMainSha: input.authorityMainSha, requestId: input.requestId,
  animationRequestId: input.animationRequestId, promptRecordId: input.promptRecordId,
  promptRecordSha256: input.promptRecordSha256,
  expressionProfileKey: profile.expressionProfileKey,
  expressionProfilePayloadHash: PROFILE_PAYLOAD_SHA256,
  executionProfileKey: profile.executionProfileKey,
  animationIdentityAuthoritySelection: input.animationIdentityAuthoritySelection,
  animationMotionLineageSelection: input.animationMotionLineageSelection,
  callProjectionSha256: input.callProjectionSha256,
  submitCountMaximum: 1, retryCountMaximum: 0 };
  const executionScopeSha256 = sha(Buffer.from(canonicalJson(payload), "utf8"));
  return { payload, executionScopeSha256,
    idempotencyKey: `gmanimidentity1.${executionScopeSha256.slice(0, 20)}` };
}

export function validatePostprocessReceipt(profile, receipt) {
  const contract = profile.recordContract;
  assertClosedKeys(receipt, contract.postprocessReceiptRequiredMembers,
    "animation_opaque_chroma_postprocess_evidence_mismatch");
  if (receipt.schemaVersion !== contract.postprocessReceiptSchemaVersion
    || receipt.state !== "postprocess_complete"
    || receipt.expressionProfileKey !== EXPRESSION_PROFILE_KEY
    || receipt.expressionProfilePayloadHash !== PROFILE_PAYLOAD_SHA256
    || canonicalJson(receipt.root) !== canonicalJson({ x: 256, y: 300 })
    || receipt.baselineY !== 448 || receipt.rootDriftMaxPx !== 0
    || receipt.baselineDriftMaxPx !== 0 || receipt.scaleFixed !== true
    || receipt.cameraFixed !== true || receipt.centroidFixed !== true
    || receipt.safeMarginPx !== 48 || receipt.transparentRgbZero !== true
    || receipt.prohibitedFringeCount !== 0 || receipt.fragmentCount !== 0
    || receipt.duplicateFrameHashCount !== 0 || receipt.repeatedMotionPhaseCount !== 0
    || receipt.wholeBodyMirrorDetected !== false
    || receipt.frame5To0ClosureValid !== true
    || receipt.gifClosedAndReopened !== true || receipt.providerCalled !== false
    || receipt.submitCount !== 0 || receipt.retryCount !== 0
    || receipt.evaluationStatus !== "not_evaluated"
    || receipt.projectCopyEligible !== false)
    throw new Error("animation_opaque_chroma_postprocess_evidence_mismatch");
  if (!Array.isArray(receipt.orderedCellSha256s)
    || receipt.orderedCellSha256s.length !== 6
    || new Set(receipt.orderedCellSha256s).size !== 6
    || receipt.orderedCellSha256s.some((value) => !/^[0-9a-f]{64}$/.test(value))
    || !Array.isArray(receipt.orderedFramePngSha256s)
    || receipt.orderedFramePngSha256s.length !== 6
    || new Set(receipt.orderedFramePngSha256s).size !== 6
    || receipt.orderedFramePngSha256s.some((value) => !/^[0-9a-f]{64}$/.test(value))
    || canonicalJson(receipt.timelineEvidence) !== canonicalJson({
      frameCount: 6, delayMs: 150, totalMs: 900, loop: "infinite" }))
    throw new Error("animation_opaque_chroma_postprocess_evidence_mismatch");
  return receipt;
}
