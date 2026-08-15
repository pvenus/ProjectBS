import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const gm = path.resolve(here, "..");
const docs = {
  visual: path.join(gm, "GeneratedMediaVisualPromptAuthoringGuide.md"),
  registry: path.join(gm, "GeneratedMediaAuthoringProfileRegistryGuide.md"),
  contract: path.join(gm, "GeneratedMediaImageGenOnlyContractGuide.md"),
  record: path.join(gm, "GeneratedMediaRecordGuide.md"),
  pipeline: path.join(gm, "ImageGenCharacterImagePipelineGuide.md"),
  planning: path.resolve(gm, "..", "..", "character", "data-structures", "CharacterPlanningDataGuide.md"),
  authoring: path.resolve(gm, "..", "..", "..", "task-prompts", "content", "generated-media", "ImageGenCharacterImagePromptAuthoringPrompt.md"),
  generation: path.resolve(gm, "..", "..", "..", "task-prompts", "content", "generated-media", "ImageGenCharacterImageGenerationPrompt.md"),
  evaluation: path.resolve(gm, "..", "..", "..", "task-prompts", "content", "generated-media", "GeneratedMediaCharacterExpressionEvaluationPrompt.md")
};

const text = Object.fromEntries(Object.entries(docs).map(([k, p]) => [k, fs.readFileSync(p, "utf8")]));
const key = "projectbs_character_bold_outline_compressed_detail@2.0.0";
const v1Key = "projectbs_character_bold_outline_compressed_detail@1.0.0";
const v1Hash = "dc5db9990f26dd1ed0ebc25c6c2b46a10b68cb4ca3248e69f7c27b28e1568b33";
const expectedV2Hash = "5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3";

function extractPayload(header, profileKey) {
  const start = text.visual.indexOf(header);
  assert.ok(start >= 0, `missing ${header}`);
  const fence = text.visual.indexOf("```json", start);
  const end = text.visual.indexOf("```", fence + 7);
  const payload = JSON.parse(text.visual.slice(fence + 7, end));
  assert.equal(payload.expressionProfileKey, profileKey);
  return payload;
}

function canonicalJson(v) {
  if (v === null || typeof v !== "object") return JSON.stringify(v);
  if (Array.isArray(v)) return `[${v.map(canonicalJson).join(",")}]`;
  return `{${Object.keys(v).sort().map(k => `${JSON.stringify(k)}:${canonicalJson(v[k])}`).join(",")}}`;
}

const sha = value => crypto.createHash("sha256").update(canonicalJson(value), "utf8").digest("hex");
const payload = extractPayload("### Bold-outline accepted-result alignment profile", key);
const payloadHash = sha(payload);
console.log(`successorProfilePayloadSha256=${payloadHash}`);
assert.equal(payloadHash, expectedV2Hash);
assert.ok(text.registry.replaceAll("\r\n", "\n").includes(`${key}\nexpressionProfilePayloadHash=${expectedV2Hash}`));

assert.deepEqual(Object.keys(payload).sort(), [
  "authoringProjectionContract", "colorSignatureContract", "compressedDetailBudget",
  "expressionProfileKey", "facialSimplificationBudget", "inkHaloContract", "inkTreatment",
  "negativeStyleLock", "outlineHierarchy", "positiveStyleLock", "proportionProjection"
].sort());
assert.equal(payload.positiveStyleLock.length, 7);
assert.equal(payload.negativeStyleLock.length, 7);
assert.equal(payload.compressedDetailBudget.maximumTotalVisibleMarks, 64);
assert.equal(payload.compressedDetailBudget.maximumInternalLineMarks, 56);
assert.equal(payload.compressedDetailBudget.maximumSecondaryFoldMarksPerGarmentRegion, 5);
assert.deepEqual(payload.colorSignatureContract.allowedSecondaryOchreAnchorSiteClasses,
  ["small_utility_pouch", "small_travel_accessory"]);
assert.deepEqual(payload.inkHaloContract.enabledBranch.maximumOpacity, { minimum: 0.08, maximum: 0.35 });
assert.deepEqual(payload.inkHaloContract.enabledBranch.maximumCanvasCoveragePercent, { minimum: 1, maximum: 45 });

// Backward compatibility: the predecessor payload and registered digest remain exact.
const v1Payload = extractPayload("### Bold-outline compressed-detail character profile", v1Key);
assert.equal(sha(v1Payload), v1Hash);
assert.ok(text.registry.replaceAll("\r\n", "\n").includes(`${v1Key}\nexpressionProfilePayloadHash=${v1Hash}`));

for (const name of ["registry", "contract", "record", "pipeline", "planning", "authoring", "generation", "evaluation"]) {
  assert.ok(text[name].includes(key), `${name} missing successor key`);
}

const requiredTokens = [
  "missing_bold_outline_v2_detail_budget_projection",
  "bold_outline_v2_detail_budget_out_of_range",
  "missing_bold_outline_v2_color_anchor_projection",
  "bold_outline_v2_color_anchor_out_of_range",
  "missing_bold_outline_v2_halo_selection",
  "bold_outline_v2_halo_projection_invalid",
  "bold_outline_v2_profile_evidence_omission",
  "provider_prompt_bold_outline_v2_projection_missing",
  "character_generation_bold_outline_v2_detail_budget_gate_failed",
  "character_generation_bold_outline_v2_color_anchor_gate_failed",
  "character_generation_bold_outline_v2_halo_gate_failed",
  "character_evaluation_bold_outline_v2_detail_budget_gate_failed",
  "character_evaluation_bold_outline_v2_color_anchor_gate_failed",
  "character_evaluation_bold_outline_v2_halo_gate_failed"
];
for (const token of requiredTokens) assert.ok(text.contract.includes(token), `missing token ${token}`);

const accepted = {
  fullBodyHeadCount: 4.5,
  externalOutlineSourcePx: 18,
  internalLineSourcePx: 8,
  facialMarkBudget: { maximumTotalMarks: 9, componentMaximums: { browsAndEyes: 4, nose: 1, mouth: 1, jawAndFaceShape: 3 } },
  detailMarkBudget: { countingUnit: payload.compressedDetailBudget.visibleMarkCountingUnit, maximumTotalVisibleMarks: 64, maximumInternalLineMarks: 56, maximumSecondaryFoldMarksPerGarmentRegion: 5 },
  primaryHue: "muted_indigo", primaryAnchorElements: ["robe_panels"],
  secondaryHue: "subdued_ochre", secondaryAnchorElements: ["utility_pouch"], secondaryAnchorSiteClasses: ["small_utility_pouch"],
  maximumCharacterCoveragePercent: 24, maximumColorMasses: 4,
  neutralOutlineColor: "dark_neutral_ink", neutralWeaponColor: "dark_neutral_steel",
  inkHalo: { enabled: true, color: "dark_neutral_ink", maximumOpacity: 0.3, maximumCanvasCoveragePercent: 36,
    centerPolicy: "character_silhouette_center", extentPolicy: "single_centered_soft_halo_behind_silhouette",
    edgeFalloff: "soft_monotonic_to_zero_alpha", edgeAlpha: 0, noScene: true,
    noOpaqueBackground: true, noShadowSubstitute: true, noDirectionalCastShadow: true },
  observed: { longLimbs: false, heroicTall: false, weakOutline: false, realisticFace: false,
    hatching: false, microtexture: false, realisticMaterials: false, denseScaleOrRivetEnumeration: false,
    arbitraryColor: false, fullGarmentFill: false, scenicBackground: false, opaqueDarkBackground: false,
    directionalShadow: false }
};

function validate(v) {
  if (v.fullBodyHeadCount < 4 || v.fullBodyHeadCount > 5 || v.observed.longLimbs || v.observed.heroicTall) return false;
  if (v.externalOutlineSourcePx < 16 || v.externalOutlineSourcePx > 22 || v.externalOutlineSourcePx / v.internalLineSourcePx < 2 || v.observed.weakOutline) return false;
  if (v.facialMarkBudget.maximumTotalMarks > 9 || v.observed.realisticFace) return false;
  const d = v.detailMarkBudget;
  if (d.maximumTotalVisibleMarks > 64 || d.maximumInternalLineMarks > 56 || d.maximumInternalLineMarks > d.maximumTotalVisibleMarks || d.maximumSecondaryFoldMarksPerGarmentRegion > 5) return false;
  if (v.observed.hatching || v.observed.microtexture || v.observed.realisticMaterials || v.observed.denseScaleOrRivetEnumeration) return false;
  if (v.maximumCharacterCoveragePercent > 35 || v.maximumColorMasses > 4 || v.observed.arbitraryColor || v.observed.fullGarmentFill) return false;
  if (v.secondaryHue && (!v.secondaryAnchorElements?.length || !v.secondaryAnchorSiteClasses?.length || v.secondaryAnchorSiteClasses.some(x => !payload.colorSignatureContract.allowedSecondaryOchreAnchorSiteClasses.includes(x)))) return false;
  const h = v.inkHalo;
  if (h.enabled === false) return Object.keys(h).length === 1 && !v.observed.opaqueDarkBackground && !v.observed.scenicBackground;
  if (h.enabled !== true || h.maximumOpacity < 0.08 || h.maximumOpacity > 0.35 || h.maximumCanvasCoveragePercent < 1 || h.maximumCanvasCoveragePercent > 45) return false;
  return h.centerPolicy === "character_silhouette_center" && h.extentPolicy === "single_centered_soft_halo_behind_silhouette" && h.edgeFalloff === "soft_monotonic_to_zero_alpha" && h.edgeAlpha === 0 && h.noScene && h.noOpaqueBackground && h.noShadowSubstitute && h.noDirectionalCastShadow && !v.observed.scenicBackground && !v.observed.opaqueDarkBackground && !v.observed.directionalShadow;
}

assert.equal(validate(accepted), true, "accepted-trait fixture must pass");
const fail = (mutate, label) => { const v = structuredClone(accepted); mutate(v); assert.equal(validate(v), false, label); };
fail(v => { v.fullBodyHeadCount = 7; }, "naturalistic tall anatomy must fail");
fail(v => { v.observed.longLimbs = true; }, "long limbs must fail");
fail(v => { v.internalLineSourcePx = 9.5; }, "weak outline ratio must fail");
fail(v => { v.observed.realisticFace = true; }, "realistic face must fail");
fail(v => { v.observed.hatching = true; }, "hatching must fail");
fail(v => { v.observed.microtexture = true; }, "microtexture must fail");
fail(v => { v.observed.realisticMaterials = true; }, "realistic material must fail");
fail(v => { v.detailMarkBudget.maximumTotalVisibleMarks = 65; }, "total marks over budget must fail");
fail(v => { v.detailMarkBudget.maximumInternalLineMarks = 57; }, "internal marks over budget must fail");
fail(v => { v.detailMarkBudget.maximumSecondaryFoldMarksPerGarmentRegion = 6; }, "fold marks over budget must fail");
fail(v => { v.secondaryAnchorSiteClasses = ["full_overcoat"]; }, "arbitrary ochre anchor must fail");
fail(v => { v.observed.fullGarmentFill = true; }, "full garment fill must fail");
fail(v => { v.inkHalo.maximumOpacity = 0.8; }, "opaque halo must fail");
fail(v => { v.observed.scenicBackground = true; }, "scenic halo must fail");
fail(v => { v.observed.directionalShadow = true; }, "directional shadow halo must fail");
fail(v => { v.inkHalo.edgeFalloff = "hard_opaque_edge"; }, "nonfading halo must fail");
const disabled = structuredClone(accepted); disabled.inkHalo = { enabled: false }; assert.equal(validate(disabled), true);
disabled.observed.opaqueDarkBackground = true; assert.equal(validate(disabled), false, "disabled halo cannot authorize dark background");

// Standing hosted-preview approval vectors remain owned by their existing test/contract.
assert.ok(text.contract.includes("generated_media_hosted_preview_auto_approval_policy_v1"));
assert.ok(text.contract.includes("hosted_preview_auto_approval_policy_mismatch"));

console.log("generated media bold-outline successor profile contract: PASS");
