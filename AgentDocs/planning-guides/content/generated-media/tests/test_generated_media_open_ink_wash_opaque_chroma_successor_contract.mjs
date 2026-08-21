import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, "../../../../../");
const read = (relative) => fs.readFileSync(path.join(repo, relative), "utf8");
const guide = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashOpaqueChromaSuccessorGuide.md");
const visual = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md");
const registry = read("AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md");

const key = "projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0";
const payloadHash = "b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a";
const baseKey = "projectbs_character_open_ink_wash_dynamic_contour@2.0.0";
const baseHash = "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5";
const baseV1Hash = "37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd";

function canonical(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonical).join(",")}]`;
  return `{${Object.keys(value).sort().map((k) => `${JSON.stringify(k)}:${canonical(value[k])}`).join(",")}}`;
}
const sha = (value) => crypto.createHash("sha256").update(canonical(value)).digest("hex");
function parseFirstJson(source) {
  const match = source.replaceAll("\r\n", "\n").match(/```json\s*([\s\S]*?)\s*```/);
  assert.ok(match);
  return JSON.parse(match[1]);
}
function parseSectionJson(source, marker) {
  const normalized = source.replaceAll("\r\n", "\n");
  return parseFirstJson(normalized.slice(normalized.indexOf(marker)));
}

const profile = parseFirstJson(guide);
assert.deepEqual(Object.keys(profile), ["expressionProfileKey", "baseProfileBinding",
  "singleImageApplicability", "providerMasterContract", "baseStyleProjectionContract",
  "postprocessBoundaryContract", "authoringProjectionContract",
  "negativeProviderMasterLock", "positiveProviderMasterLock"]);
assert.equal(profile.expressionProfileKey, key);
assert.equal(sha(profile), payloadHash);
assert.equal(profile.baseProfileBinding.expressionProfileKey, baseKey);
assert.equal(profile.baseProfileBinding.expressionProfilePayloadHash, baseHash);
assert.deepEqual(profile.singleImageApplicability, {
  assetType: "character_single_image", domainType: "character",
  structureProfile: "character_single_image_v2", outputCount: 1,
  selection: "explicit_approved_planning_fact_and_complete_successor_projection_required",
});
assert.deepEqual(profile.providerMasterContract.canvas, { width: 1024, height: 1536 });
assert.deepEqual(profile.providerMasterContract.generationBackground,
  { mode: "removable_solid", color: "#00FF00" });
assert.equal(profile.providerMasterContract.backgroundFullyOpaque, true);
assert.equal(profile.providerMasterContract.providerTransparency, "prohibited");
assert.equal(profile.providerMasterContract.foregroundExactChromaRgb, "prohibited");
assert.equal(profile.providerMasterContract.neighboringFragments, "prohibited");
assert.deepEqual(profile.providerMasterContract.forbiddenFieldFeatures,
  ["checkerboard", "gradient", "texture", "lighting_variation", "halo",
    "vignette", "floor", "scene", "shadow"]);
assert.deepEqual(profile.baseStyleProjectionContract.backgroundStatementSubstitutions.map((x) => x.constraintId),
  ["char_open_wash_v2_negative_halo_scene_shadow",
    "char_open_wash_v2_positive_identity_on_ivory"]);
assert.equal(profile.postprocessBoundaryContract.ownerRole, "generated_media_chroma_uncomposite");
assert.equal(profile.postprocessBoundaryContract.generationRoleMayUncomposite, false);
assert.equal(profile.postprocessBoundaryContract.generationRoleMayClaimTrueAlpha, false);
assert.equal(profile.postprocessBoundaryContract.providerRecall, "prohibited");
assert.equal(profile.negativeProviderMasterLock.length, 5);
assert.equal(profile.positiveProviderMasterLock.length, 4);

const openV1 = parseSectionJson(visual, "### Open ink-wash dynamic-contour character profile");
const openV2 = parseSectionJson(visual, "### Open ink-wash output-conformance successor profile");
assert.equal(sha(openV1), baseV1Hash);
assert.equal(sha(openV2), baseHash);
assert.deepEqual(openV2.backgroundContract.generationBackground,
  { mode: "removable_solid", color: "#F2EFE6" });
assert.equal(openV2.backgroundContract.allowedVisibleField, "uniform_warm_ivory_only");
assert.equal(openV2.negativeStyleLock.length, 9);
assert.equal(openV2.positiveStyleLock.length, 9);

for (const text of [guide, registry]) {
  assert.ok(text.includes(key));
  assert.ok(text.includes(payloadHash));
  assert.ok(text.includes(baseKey));
  assert.ok(text.includes(baseHash));
}
assert.ok(registry.includes(`appliesTo=character_single_image_v2`));
assert.ok(registry.includes(`postprocessOwnerRole=generated_media_chroma_uncomposite`));

const surfaces = [
  "AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md",
  "AgentDocs/task-prompts/character/ActCharacterPlanningPrompts.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md",
];
for (const surface of surfaces) {
  const text = read(surface);
  assert.ok(text.includes(key), `${surface}: key`);
  assert.ok(text.includes("generated_media_chroma_uncomposite"), `${surface}: postprocess role`);
}

function route({ requestedKey = key, requestedHash = payloadHash, requestedBaseKey = baseKey,
  requestedBaseHash = baseHash, canvas = { width: 1024, height: 1536 },
  background = { mode: "removable_solid", color: "#00FF00" }, outputFormat = "png",
  outputCount = 1, transparentForegroundSelection, directAlphaRequired = false } = {}) {
  if (requestedKey !== key || requestedHash !== payloadHash)
    throw new Error("character_style_profile_conflict");
  if (requestedBaseKey !== baseKey || requestedBaseHash !== baseHash)
    throw new Error("open_ink_chroma_successor_base_mismatch");
  if (transparentForegroundSelection !== undefined || directAlphaRequired)
    throw new Error("open_ink_chroma_direct_alpha_conflict");
  if (canonical(canvas) !== canonical({ width: 1024, height: 1536 })
      || canonical(background) !== canonical({ mode: "removable_solid", color: "#00FF00" })
      || outputFormat !== "png" || outputCount !== 1)
    throw new Error("open_ink_chroma_master_contract_mismatch");
  return { expressionProfileKey: key, expressionProfilePayloadHash: payloadHash,
    generationBackground: background, postprocessOwnerRole: "generated_media_chroma_uncomposite" };
}
assert.deepEqual(route(), { expressionProfileKey: key, expressionProfilePayloadHash: payloadHash,
  generationBackground: { mode: "removable_solid", color: "#00FF00" },
  postprocessOwnerRole: "generated_media_chroma_uncomposite" });
assert.throws(() => route({ requestedHash: "0".repeat(64) }), /character_style_profile_conflict/);
assert.throws(() => route({ requestedBaseHash: baseV1Hash }), /open_ink_chroma_successor_base_mismatch/);
assert.throws(() => route({ canvas: { width: 1024, height: 1024 } }), /open_ink_chroma_master_contract_mismatch/);
assert.throws(() => route({ background: { mode: "removable_solid", color: "#F2EFE6" } }), /open_ink_chroma_master_contract_mismatch/);
assert.throws(() => route({ background: { mode: "removable_solid", color: "#00fe00" } }), /open_ink_chroma_master_contract_mismatch/);
assert.throws(() => route({ background: { mode: "transparent" } }), /open_ink_chroma_master_contract_mismatch/);
assert.throws(() => route({ transparentForegroundSelection: {} }), /open_ink_chroma_direct_alpha_conflict/);
assert.throws(() => route({ directAlphaRequired: true }), /open_ink_chroma_direct_alpha_conflict/);
assert.throws(() => route({ outputCount: 2 }), /open_ink_chroma_master_contract_mismatch/);

const generationPrompt = read("AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md");
for (const token of ["open_ink_chroma_provider_master_nonopaque",
  "open_ink_chroma_provider_master_field_nonuniform",
  "open_ink_chroma_provider_master_foreground_key_collision",
  "open_ink_chroma_provider_master_forbidden_feature",
  "open_ink_chroma_stage_boundary_violation"]) assert.ok(generationPrompt.includes(token));

console.log({ expressionProfileKey: key, expressionProfilePayloadHash: payloadHash,
  baseKey, baseHash, providerMaster: profile.providerMasterContract,
  postprocessOwnerRole: profile.postprocessBoundaryContract.ownerRole });
console.log("generated media open-ink opaque-chroma successor contract: PASS");
