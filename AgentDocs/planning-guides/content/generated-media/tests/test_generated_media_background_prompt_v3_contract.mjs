import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import {
  backgroundPromptContractInternals,
  buildBackgroundPromptArtifacts,
  classifyBackgroundPromptExistingState,
  validateBackgroundRoutingRecord,
} from "../helpers/generated_media_background_prompt_v3_contract_v1.mjs";
import { sha256Hex } from "../helpers/generated_media_canonical_serializers_v1.mjs";

const { hashObject } = backgroundPromptContractInternals;
const H = (digit) => digit.repeat(64);
const testDir = path.dirname(fileURLToPath(import.meta.url));
const generatedMediaRoot = path.resolve(testDir, "..");

function routeFixture() {
  const contentId = "battle.contract_vector.1";
  const planningHandoffPath = `AgentDocs/planning-data/generated-media/battle/background/contract/${contentId}.json`;
  const routingRecordId = `gmroute2.background_single_image.${contentId}.${"a".repeat(20)}`;
  const routingRecordPath = `AgentDocs/planning-data/generated-media-routing/v2/background_single_image/${contentId}/${routingRecordId}.json`;
  const backgroundSpecification = {
    anchor: {
      focalDepth: "midground",
      framingRegion: "left shrine gate beside an open central combat lane",
      type: "scene_composition_anchor",
    },
    aspectRatio: "16:9",
    canvas: { height: 1440, width: 2560 },
    composition: "weathered shrine gate offset left, broad packed-earth lane through center",
    consistencyLock: {
      contentIdentity: contentId,
      sceneFacts: [
        "Joseon mountain shrine immediately before a battle",
        "rough Korean ink, charcoal, dry brush, muted mineral pigment, and paper grain",
      ],
    },
    depthLayers: ["foreground packed earth", "midground shrine lane", "background misted ridge"],
    finalBackgroundPolicy: "single opaque environment background",
    horizon: "upper-middle subdued mountain shoulder",
    playableOrReadabilityArea: "wide unobstructed central and lower-middle combat lane",
    safeArea: "central combat lane remains quiet",
    sceneContract: "empty pre-combat shrine approach under cold mist",
    subjectExclusions: ["characters", "victory state"],
    subjectInclusions: ["weathered shrine gate", "cold mist"],
    targetDisplay: "side-view combat background",
    viewpoint: "slightly elevated side-view game camera",
  };
  const requiredElements = [
    "Create exactly one environment-only Joseon mountain shrine battle background.",
    "Keep a broad unobstructed central combat lane.",
  ];
  const prohibitedElements = ["characters_or_monsters", "ui_text_logo_readable_letters"];
  const typeSpecification = { backgroundSpecification };
  const normalizedRequest = {
    assetType: "background_single_image", contentId,
    contentUsage: "contract vector battle background", domainType: "battle",
    planningSnapshotHash: H("1"), prohibitedElements, requestId: "gmplan2.background.contract.vector",
    requiredElements, typeSpecification,
  };
  const authoringHandoff = {
    assetType: "background_single_image", contentId, domainType: "battle",
    indexPath: `AgentDocs/planning-data/generated-media-routing/v2/background_single_image/${contentId}/routing_index.json`,
    normalizedRequest, planningHandoffPath, planningSnapshotHash: H("1"),
    profileKey: "battle_background@2.0.0", prohibitedElements, provider: "imagegen",
    registryRowId: "battle_background_single_image_v2",
    registryVersion: "generated_media_authoring_profile_registry_v2",
    requestId: "gmplan2.background.contract.vector", requiredElements,
    routingPayloadSha256: H("a"), routingRecordId, routingRecordPath,
    selectedAuthoringPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundPromptAuthoringPrompt.md",
    selectedGenerationPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundGenerationPrompt.md",
    selectedPipeline: "imagegen_background_single_image",
    sourcePlanningFiles: [{ path: "Assets/Contents/Battle/json/battle.contract_vector.1.json",
      role: "canonical_battle_json", sha256: H("2") }],
    structureProfile: "background_single_image_v2", typeSpecification,
  };
  return {
    assetType: "background_single_image", authoringHandoff, contentId,
    createdAt: "2026-08-26T21:00:00+09:00", domainType: "battle", normalizedRequest,
    planningHandoffPath, planningSnapshotHash: H("1"), profileKey: "battle_background@2.0.0",
    prohibitedElements, provider: "imagegen", registryRowId: "battle_background_single_image_v2",
    registryVersion: "generated_media_authoring_profile_registry_v2",
    requestId: "gmplan2.background.contract.vector", requiredElements,
    routerVersion: "generated_media_router_v2", routingPayloadSha256: H("a"),
    routingReason: { code: "exact_registry_row_match",
      matchedFields: { assetType: "background_single_image", domainType: "battle",
        profileKey: "battle_background@2.0.0" },
      profileKey: "battle_background@2.0.0", registryRowId: "battle_background_single_image_v2" },
    routingRecordId, schemaVersion: "generated_media_routing_v2",
    selectedAuthoringPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundPromptAuthoringPrompt.md",
    selectedGenerationPrompt: "AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundGenerationPrompt.md",
    selectedPipeline: "imagegen_background_single_image",
    sourcePlanningFiles: authoringHandoff.sourcePlanningFiles,
    structureProfile: "background_single_image_v2", typeSpecification,
    validation: { planningHandoff: "valid", planningSnapshot: "valid",
      recordIdentity: "valid", registryMatchCount: 1, sourceHashes: "valid",
      status: "valid", typeSpecification: "valid" },
  };
}

const route = routeFixture();

const registryGuide = fs.readFileSync(path.join(generatedMediaRoot,
  "GeneratedMediaAuthoringProfileRegistryGuide.md"), "utf8");
const backgroundGuide = fs.readFileSync(path.join(generatedMediaRoot,
  "ImageGenBackgroundPipelineGuide.md"), "utf8");
const authoringPrompt = fs.readFileSync(path.resolve(generatedMediaRoot, "../../../task-prompts/content/generated-media/ImageGenBackgroundPromptAuthoringPrompt.md"), "utf8");
assert.match(registryGuide, /battle_background_single_image_v2[^\n]*battle_background@2\.0\.0[^\n]*imagegen_background_single_image/);
assert.match(backgroundGuide, /gmprompt3\.background_single_image/);
assert.match(backgroundGuide, /style_contract_only/);
assert.match(authoringPrompt, /imagegen_background_single_image_prompt_v2/);
assert.match(authoringPrompt, /unsupported_record_schema/);
assert.equal(validateBackgroundRoutingRecord(route), true);
const first = buildBackgroundPromptArtifacts(route, { routingRecordSha256: H("b") });
const second = buildBackgroundPromptArtifacts(structuredClone(route), { routingRecordSha256: H("b") });

assert.deepEqual(first, second);
assert.equal(first.record.assetType, "background_single_image");
assert.equal(first.record.domainType, "battle");
assert.equal(first.record.structureProfile, "background_single_image_v2");
assert.equal("expressionProfileKey" in first.record, false);
assert.equal("referenceBindings" in first.record, false);
assert.deepEqual(first.providerPayload, {
  schemaVersion: "imagegen_background_single_image_prompt_v2",
  scenePromptOriginal: first.scenePromptOriginal,
});
assert.equal(first.record.providerPromptPayloadHash, hashObject(first.providerPayload));
assert.deepEqual(first.providerSettingsIntent, {
  canvas: { height: 1440, width: 2560 },
  generationBackground: { mode: "opaque" },
  outputFormat: "png",
});
assert.equal(first.record.providerSettingsIntentSha256, hashObject(first.providerSettingsIntent));
assert.equal(first.record.promptPayloadSha256, hashObject(first.promptHashPayload));
assert.equal(first.record.promptRecordId,
  `gmprompt3.background_single_image.${route.contentId}.${first.record.promptPayloadSha256.slice(0, 20)}`);
assert.equal(first.recordPath,
  `AgentDocs/planning-data/generated-media-prompts/v2/background_single_image/${route.contentId}/${first.record.promptRecordId}.json`);
assert.equal(first.record.promptMarkdownPath, first.recordPath.replace(/\.json$/, ".prompt.md"));
assert.equal(first.indexPath,
  `AgentDocs/planning-data/generated-media-prompts/v2/background_single_image/${route.contentId}/prompt_index.json`);
assert.equal(first.recordBytes.at(-1), 0x0a);
assert.equal(first.recordBytes.includes(0x0d), false);
assert.equal(first.promptMarkdownBytes.at(-1), 0x0a);
assert.equal(first.promptMarkdownBytes.includes(0x0d), false);
assert.equal(first.generationHandoff.promptIndexSha256, sha256Hex(first.indexBytes));

assert.equal(classifyBackgroundPromptExistingState(first, {}), "new");
assert.equal(classifyBackgroundPromptExistingState(first, {
  recordBytes: first.recordBytes,
  promptMarkdownBytes: first.promptMarkdownBytes,
  index: first.index,
}), "reused_identical");
assert.equal(classifyBackgroundPromptExistingState(first, {
  recordBytes: first.recordBytes,
  promptMarkdownBytes: first.promptMarkdownBytes,
}), "recoverable_orphan");
const collision = Buffer.from(first.recordBytes);
collision[10] ^= 1;
assert.throws(() => classifyBackgroundPromptExistingState(first, {
  recordBytes: collision, promptMarkdownBytes: first.promptMarkdownBytes,
}), /record_collision/);
const dangling = structuredClone(first.index);
assert.throws(() => classifyBackgroundPromptExistingState(first, { index: dangling }),
  /index_entry_invalid/);
const badRoute = structuredClone(route);
badRoute.typeSpecification.backgroundSpecification.unplanned = "forbidden";
assert.throws(() => validateBackgroundRoutingRecord(badRoute),
  /missing_background_scene_contract/);

const vector = {
  visualBriefId: first.visualBrief.visualBriefId,
  promptRecordId: first.record.promptRecordId,
  promptPayloadSha256: first.record.promptPayloadSha256,
  promptRecordSha256: first.generationHandoff.promptRecordSha256,
  promptMarkdownSha256: first.record.promptMarkdownSha256,
  promptIndexSha256: first.generationHandoff.promptIndexSha256,
  generationHandoffSha256: first.generationHandoffSha256,
};
assert.deepEqual(vector, {
  visualBriefId: "gmbrief2.background_single_image.battle.contract_vector.1.e207d168cec83ebff3f0",
  promptRecordId: "gmprompt3.background_single_image.battle.contract_vector.1.29bed041a02422ef747c",
  promptPayloadSha256: "29bed041a02422ef747c4b10522b81188d9a3561faf18a18b18cb404d9173a9c",
  promptRecordSha256: "33effca46cac9ff119f5b7d69e6122f2897112a4067b3aea231867a83810964e",
  promptMarkdownSha256: "24b0529b2806ee082d73e1dc75c4c2b829902cb23fd074d26ca896b579fa7749",
  promptIndexSha256: "ca4c4e768f3196353d52cabfd855b169726f8669ae61d510a03ff0b0ecfa995f",
  generationHandoffSha256: "82c97d6d3517d82f86d8444cbe90ca94c2b9daf06f930d616f5826691100af69",
});

console.log(vector);
console.log("generated media background prompt v3 vectors: PASS");
