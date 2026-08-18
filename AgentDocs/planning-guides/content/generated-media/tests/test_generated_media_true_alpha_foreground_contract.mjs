import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

const canonical = (value) => Array.isArray(value)
  ? `[${value.map(canonical).join(",")}]`
  : value && typeof value === "object"
    ? `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${canonical(value[key])}`).join(",")}}`
    : JSON.stringify(value);
const hash = (value) => createHash("sha256").update(canonical(value)).digest("hex");
const key = "generated_media_true_alpha_foreground@1.0.0";
const payloadHash = "2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108";

const payload = {
  animation: { backgroundFlicker: false, baselineDriftMaxPx: 0,
    completedAnimationFormat: "gif", dynamicPigmentExcludedFromAnchorMovement: true,
    fixedGroundBaseline: true, fixedPelvisWorldRootCoordinate: true, fixedScale: true,
    identicalCanvas: true, independentSilhouetteRecentering: false,
    neighboringFragments: false, orderedFrameCount: 6,
    orderedTrueAlphaFrameFormat: "png", pelvisDriftMaxPx: 0,
    swordAndEffectsInsideSafeMargin: true },
  appliesTo: [
    { assetType: "character_single_image", structureProfile: "character_single_image_v2" },
    { assetType: "animation", structureProfile: "animation_gif_frame_set_v2" },
  ],
  characterSingleImage: { colorMode: "RGBA", fullFigureEquipmentPigmentInBounds: true,
    primaryFormat: "png" },
  common: { boundedArtisticPartialAlpha:
      "inside_intended_character_equipment_pigment_silhouette_only",
    forbidden: ["matte", "checkerboard", "halo", "vignette", "floor", "scene",
      "cast_shadow", "residual_fringe"], noClipping: true,
    outsideIntendedForegroundAlpha: 0, safeMarginPx: "required_positive_integer" },
  compatibility: { backgroundFullyOpaque: "not_reinterpreted", existingBranches: "unchanged" },
  gates: { evaluation: "all_projection_failures_pre_score_hard_fail",
    generationAndPreservation: "alpha_mask_fringe_anchor_baseline_before_complete",
    promotion: "separate_completed_pass_and_passForProjectCopy_true_before_authenticated_replaceExisting" },
  projectionKey: key,
  schemaVersion: "generated_media_transparent_foreground_output_projection_v1",
};
assert.equal(hash(payload), payloadHash);

const commonSelection = { schemaVersion: "generated_media_transparent_foreground_selection_v1",
  projectionKey: key, projectionPayloadHash: payloadHash, safeMarginPx: 24, noClipping: true };
const mainSelection = { ...commonSelection, assetType: "character_single_image",
  mainLock: { rgbaEvidenceRequired: true, fullFigureEquipmentPigmentInBounds: true } };
const animationSelection = { ...commonSelection, assetType: "animation",
  animationLock: { frameCount: 6, canvasWidth: 640, canvasHeight: 512,
    pelvisWorldRootX: 320, pelvisWorldRootY: 400, groundBaselineY: 440,
    scaleNumerator: 1, scaleDenominator: 1, independentSilhouetteRecentering: false,
    dynamicPigmentExcludedFromAnchorMovement: true } };

function validateSelection(value) {
  const commonKeys = ["schemaVersion", "projectionKey", "projectionPayloadHash",
    "assetType", "safeMarginPx", "noClipping"];
  const branch = value.assetType === "character_single_image" ? "mainLock"
    : value.assetType === "animation" ? "animationLock" : null;
  if (!branch || Object.keys(value).sort().join() !== [...commonKeys, branch].sort().join())
    return "true_alpha_projection_mismatch";
  if (value.projectionKey !== key || value.projectionPayloadHash !== payloadHash
    || !Number.isInteger(value.safeMarginPx) || value.safeMarginPx < 1 || value.noClipping !== true)
    return "true_alpha_projection_mismatch";
  if (branch === "mainLock") return Object.keys(value.mainLock).sort().join()
    === ["rgbaEvidenceRequired", "fullFigureEquipmentPigmentInBounds"].sort().join()
    && value.mainLock.rgbaEvidenceRequired === true
    && value.mainLock.fullFigureEquipmentPigmentInBounds === true ? "valid"
      : "true_alpha_projection_mismatch";
  const lock = value.animationLock;
  const keys = ["frameCount", "canvasWidth", "canvasHeight", "pelvisWorldRootX",
    "pelvisWorldRootY", "groundBaselineY", "scaleNumerator", "scaleDenominator",
    "independentSilhouetteRecentering", "dynamicPigmentExcludedFromAnchorMovement"];
  return Object.keys(lock).sort().join() === keys.sort().join() && lock.frameCount === 6
    && lock.canvasWidth > 0 && lock.canvasHeight > 0 && lock.scaleNumerator > 0
    && lock.scaleDenominator > 0 && lock.independentSilhouetteRecentering === false
    && lock.dynamicPigmentExcludedFromAnchorMovement === true ? "valid"
      : "true_alpha_projection_mismatch";
}
assert.equal(validateSelection(mainSelection), "valid");
assert.equal(validateSelection(animationSelection), "valid");
assert.equal(validateSelection({ ...mainSelection, animationLock: animationSelection.animationLock }),
  "true_alpha_projection_mismatch");
assert.equal(validateSelection({ ...animationSelection, safeMarginPx: 0 }),
  "true_alpha_projection_mismatch");
assert.equal(validateSelection({ ...animationSelection, animationLock:
  { ...animationSelection.animationLock, independentSilhouetteRecentering: true } }),
  "true_alpha_projection_mismatch");

const commonReceipt = { schemaVersion: "generated_media_true_alpha_output_receipt_v1",
  projectionKey: key, projectionPayloadHash: payloadHash, selectionSha256: "a".repeat(64),
  safeMarginPx: 24, alphaMaskSha256: "b".repeat(64), outsideForegroundAlphaMaximum: 0,
  partialAlphaInsideIntendedSilhouette: true, matteDetected: false,
  checkerboardDetected: false, haloDetected: false, vignetteDetected: false,
  floorDetected: false, sceneDetected: false, castShadowDetected: false,
  residualFringeDetected: false, clippingDetected: false, status: "valid" };
const mainReceipt = { ...commonReceipt, assetType: "character_single_image", width: 1024,
  height: 1536, rgbaPixelSha256: "c".repeat(64),
  fullFigureEquipmentPigmentInBounds: true };
const animationReceipt = { ...commonReceipt, assetType: "animation",
  completedGifSha256: "c".repeat(64),
  trueAlphaPngFrameSha256s: Array.from({ length: 6 }, (_, i) => `${i}`.repeat(64)),
  frameAlphaMaskSha256s: Array.from({ length: 6 }, (_, i) => `${i + 1}`.repeat(64)),
  canvasWidth: 640, canvasHeight: 512, pelvisWorldRootX: 320, pelvisWorldRootY: 400,
  groundBaselineY: 440, pelvisDriftMaxPx: 0, baselineDriftMaxPx: 0,
  scaleNumerator: 1, scaleDenominator: 1, independentSilhouetteRecentering: false,
  backgroundFlickerDetected: false, neighboringFragmentsDetected: false,
  swordAndEffectsInsideSafeMargin: true, dynamicPigmentExcludedFromAnchorMovement: true };

const commonReceiptKeys = ["schemaVersion", "projectionKey", "projectionPayloadHash",
  "selectionSha256", "assetType", "safeMarginPx", "alphaMaskSha256",
  "outsideForegroundAlphaMaximum", "partialAlphaInsideIntendedSilhouette",
  "matteDetected", "checkerboardDetected", "haloDetected", "vignetteDetected",
  "floorDetected", "sceneDetected", "castShadowDetected", "residualFringeDetected",
  "clippingDetected", "status"];
const mainReceiptKeys = [...commonReceiptKeys, "width", "height", "rgbaPixelSha256",
  "fullFigureEquipmentPigmentInBounds"];
const animationReceiptKeys = [...commonReceiptKeys, "completedGifSha256",
  "trueAlphaPngFrameSha256s", "frameAlphaMaskSha256s", "canvasWidth", "canvasHeight",
  "pelvisWorldRootX", "pelvisWorldRootY", "groundBaselineY", "pelvisDriftMaxPx",
  "baselineDriftMaxPx", "scaleNumerator", "scaleDenominator",
  "independentSilhouetteRecentering", "backgroundFlickerDetected",
  "neighboringFragmentsDetected", "swordAndEffectsInsideSafeMargin",
  "dynamicPigmentExcludedFromAnchorMovement"];
const exactKeys = (value, keys) => Object.keys(value).sort().join() === keys.sort().join();

function validateReceipt(value) {
  const keys = value.assetType === "character_single_image" ? mainReceiptKeys
    : value.assetType === "animation" ? animationReceiptKeys : null;
  if (!keys || !exactKeys(value, keys)) return "true_alpha_branch_conflict";
  if (value.projectionKey !== key || value.projectionPayloadHash !== payloadHash)
    return "true_alpha_projection_mismatch";
  if (value.outsideForegroundAlphaMaximum !== 0) return "outside_foreground_alpha_nonzero";
  if (!value.partialAlphaInsideIntendedSilhouette)
    return "artistic_partial_alpha_outside_silhouette";
  if (value.residualFringeDetected) return "true_alpha_residual_fringe_detected";
  if (["matteDetected", "checkerboardDetected", "haloDetected", "vignetteDetected",
    "floorDetected", "sceneDetected", "castShadowDetected"].some((k) => value[k]))
    return "true_alpha_background_artifact_detected";
  if (value.clippingDetected) return "true_alpha_safe_margin_or_clipping_violation";
  if (value.status !== "valid") return "true_alpha_projection_mismatch";
  if (value.assetType === "character_single_image"
    && !value.fullFigureEquipmentPigmentInBounds)
    return "true_alpha_safe_margin_or_clipping_violation";
  if (value.assetType === "animation") {
    if (value.trueAlphaPngFrameSha256s.length !== 6 || value.frameAlphaMaskSha256s.length !== 6)
      return "true_alpha_projection_mismatch";
    if (value.pelvisDriftMaxPx !== 0 || value.baselineDriftMaxPx !== 0)
      return "true_alpha_animation_anchor_baseline_drift";
    if (value.independentSilhouetteRecentering)
      return "true_alpha_animation_independent_recentering_detected";
    if (value.backgroundFlickerDetected) return "true_alpha_animation_background_flicker";
    if (value.neighboringFragmentsDetected) return "true_alpha_animation_neighboring_fragment";
    if (!value.swordAndEffectsInsideSafeMargin)
      return "true_alpha_safe_margin_or_clipping_violation";
    if (!value.dynamicPigmentExcludedFromAnchorMovement)
      return "true_alpha_animation_dynamic_pigment_anchor_contamination";
  }
  return "valid";
}
assert.equal(validateReceipt(mainReceipt), "valid");
assert.equal(validateReceipt(animationReceipt), "valid");
for (const [change, token] of [
  [{ outsideForegroundAlphaMaximum: 1 }, "outside_foreground_alpha_nonzero"],
  [{ partialAlphaInsideIntendedSilhouette: false }, "artistic_partial_alpha_outside_silhouette"],
  [{ residualFringeDetected: true }, "true_alpha_residual_fringe_detected"],
  [{ matteDetected: true }, "true_alpha_background_artifact_detected"],
  [{ clippingDetected: true }, "true_alpha_safe_margin_or_clipping_violation"],
]) assert.equal(validateReceipt({ ...mainReceipt, ...change }), token);
assert.equal(validateReceipt({ ...animationReceipt, pelvisDriftMaxPx: 1 }),
  "true_alpha_animation_anchor_baseline_drift");
assert.equal(validateReceipt({ ...animationReceipt, backgroundFlickerDetected: true }),
  "true_alpha_animation_background_flicker");
assert.equal(validateReceipt({ ...mainReceipt, unexpected: true }), "true_alpha_branch_conflict");
assert.equal(validateReceipt({ ...mainReceipt, completedGifSha256: "d".repeat(64) }),
  "true_alpha_branch_conflict");
assert.equal(validateReceipt({ ...animationReceipt, trueAlphaPngFrameSha256s:
  animationReceipt.trueAlphaPngFrameSha256s.slice(0, 5) }), "true_alpha_projection_mismatch");
assert.equal(validateReceipt({ ...animationReceipt, independentSilhouetteRecentering: true }),
  "true_alpha_animation_independent_recentering_detected");
assert.equal(validateReceipt({ ...animationReceipt, neighboringFragmentsDetected: true }),
  "true_alpha_animation_neighboring_fragment");
assert.equal(validateReceipt({ ...animationReceipt, swordAndEffectsInsideSafeMargin: false }),
  "true_alpha_safe_margin_or_clipping_violation");
assert.equal(validateReceipt({ ...animationReceipt,
  dynamicPigmentExcludedFromAnchorMovement: false }),
  "true_alpha_animation_dynamic_pigment_anchor_contamination");

const root = new URL("../../../../", import.meta.url);
const paths = [
  "planning-guides/character/data-structures/CharacterPlanningDataGuide.md",
  "task-prompts/character/ActCharacterPlanningPrompts.md",
  "planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md",
  "planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md",
  "planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md",
  "planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md",
  "planning-guides/content/generated-media/GeneratedMediaTransparentForegroundAuthoringGuide.md",
  "task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md",
  "task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md",
  "task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md",
  "task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md",
  "planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md",
  "task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md",
  "planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md",
  "planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md",
  "task-prompts/content/GeneratedImageEvaluationPrompt.md",
  "task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md",
];
for (const path of paths) {
  const text = readFileSync(new URL(path, root), "utf8");
  assert.ok(text.includes(key), `${path} missing key`);
  assert.ok(text.includes(payloadHash), `${path} missing hash`);
}
const providerNativeTest = readFileSync(new URL("test_generated_media_provider_native_animated_gif_contract.mjs", import.meta.url), "utf8");
assert.ok(providerNativeTest.includes("backgroundFullyOpaque"));
console.log("generated media true-alpha foreground projection v1: PASS");
