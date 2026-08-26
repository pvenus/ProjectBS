import { canonicalJson, sha256Hex } from "./generated_media_canonical_serializers_v1.mjs";

const ROUTE_KEYS = ["assetType", "authoringHandoff", "contentId", "createdAt",
  "domainType", "normalizedRequest", "planningHandoffPath", "planningSnapshotHash",
  "profileKey", "prohibitedElements", "provider", "registryRowId", "registryVersion",
  "requestId", "requiredElements", "routerVersion", "routingPayloadSha256",
  "routingReason", "routingRecordId", "schemaVersion", "selectedAuthoringPrompt",
  "selectedGenerationPrompt", "selectedPipeline", "sourcePlanningFiles",
  "structureProfile", "typeSpecification", "validation"];
const BACKGROUND_SPEC_KEYS = ["anchor", "aspectRatio", "canvas", "composition",
  "consistencyLock", "depthLayers", "finalBackgroundPolicy", "horizon",
  "playableOrReadabilityArea", "safeArea", "sceneContract", "subjectExclusions",
  "subjectInclusions", "targetDisplay", "viewpoint"];
const BRIEF_KEYS = ["anchorPolicy", "artifactSpecificBrief", "assetType",
  "backgroundPolicy", "composition", "contentId", "domainType",
  "guideContractVersion", "likelyWrongObjects", "negativeStyleLock", "outlinePolicy",
  "paletteAndMaterial", "planningOriginalRef", "planningSnapshotHash",
  "positiveStyleLock", "primarySubjectOrSilhouette", "profileKey",
  "prohibitedVisualStatements", "providerTranslationContract", "registryRowId",
  "registryVersion", "requestId", "requiredVisualStatements", "schemaVersion",
  "status", "supportingElements", "validation", "visualBriefId",
  "visualEvidenceMap", "visualHierarchy"];
const RECORD_KEYS = ["assetType", "contentId", "createdAt", "domainType",
  "planningHandoffPath", "planningSnapshotHash", "profileKey", "prohibitedElements",
  "promptMarkdownPath", "promptMarkdownSha256", "promptPayloadSha256",
  "promptRecordId", "provider", "providerPromptPayloadHash",
  "providerSettingsIntent", "providerSettingsIntentSha256", "registryRowId",
  "registryVersion", "requestId", "requiredElements", "routingPayloadSha256",
  "routingRecordId", "routingRecordPath", "routingRecordSha256", "scenePromptOriginal",
  "schemaVersion", "sourcePlanningFiles", "status", "structureProfile", "validation",
  "visualBrief", "visualBriefSha256"];

const hashObject = (value) => sha256Hex(Buffer.from(canonicalJson(value), "utf8"));
const jsonFileBytes = (value) => Buffer.from(`${canonicalJson(value)}\n`, "utf8");
const markdownBytes = (value) => Buffer.from(`${value}\n`, "utf8");
const pointerEscape = (value) => value.replaceAll("~", "~0").replaceAll("/", "~1");

function assertClosedKeys(value, expected, failure = "unknown_record_field") {
  if (!value || typeof value !== "object" || Array.isArray(value)
    || canonicalJson(Object.keys(value).sort()) !== canonicalJson([...expected].sort())) {
    throw new Error(failure);
  }
}

function assertNonEmptyString(value, failure = "provider_value_invalid") {
  if (typeof value !== "string" || value.length === 0 || value.includes("\r"))
    throw new Error(failure);
}

function assertStatementArray(value, failure) {
  if (!Array.isArray(value) || value.length === 0) throw new Error(failure);
  value.forEach((item) => assertNonEmptyString(item, failure));
}

export function validateBackgroundRoutingRecord(route) {
  assertClosedKeys(route, ROUTE_KEYS);
  if (route.schemaVersion !== "generated_media_routing_v2"
    || route.assetType !== "background_single_image"
    || !["stage", "battle", "environment"].includes(route.domainType)
    || route.provider !== "imagegen"
    || route.structureProfile !== "background_single_image_v2") {
    throw new Error("unsupported_record_schema");
  }
  if (route.registryVersion !== "generated_media_authoring_profile_registry_v2"
    || route.registryRowId !== `${route.domainType}_background_single_image_v2`
    || route.profileKey !== `${route.domainType}_background@2.0.0`) {
    throw new Error("unsupported_current_route");
  }
  assertStatementArray(route.requiredElements, "missing_required_elements");
  assertStatementArray(route.prohibitedElements, "missing_prohibited_elements");
  assertClosedKeys(route.typeSpecification, ["backgroundSpecification"]);
  const spec = route.typeSpecification.backgroundSpecification;
  assertClosedKeys(spec, BACKGROUND_SPEC_KEYS, "missing_background_scene_contract");
  assertClosedKeys(spec.anchor, ["focalDepth", "framingRegion", "type"],
    "missing_anchor_contract");
  assertClosedKeys(spec.canvas, ["height", "width"], "missing_background_canvas_contract");
  assertClosedKeys(spec.consistencyLock, ["contentIdentity", "sceneFacts"],
    "missing_background_consistency_lock");
  for (const key of ["aspectRatio", "composition", "finalBackgroundPolicy", "horizon",
    "playableOrReadabilityArea", "safeArea", "sceneContract", "targetDisplay", "viewpoint"])
    assertNonEmptyString(spec[key], `missing_background_${key}`);
  for (const key of ["depthLayers", "subjectInclusions", "subjectExclusions"])
    assertStatementArray(spec[key], `missing_background_${key}`);
  if (!Number.isInteger(spec.canvas.width) || !Number.isInteger(spec.canvas.height)
    || spec.canvas.width < 1 || spec.canvas.height < 1)
    throw new Error("missing_background_canvas_contract");
  if (spec.anchor.type !== "scene_composition_anchor") throw new Error("missing_anchor_contract");
  assertNonEmptyString(spec.anchor.focalDepth, "missing_anchor_contract");
  assertNonEmptyString(spec.anchor.framingRegion, "missing_anchor_contract");
  if (spec.consistencyLock.contentIdentity !== route.contentId
    || !Array.isArray(spec.consistencyLock.sceneFacts)
    || spec.consistencyLock.sceneFacts.length === 0)
    throw new Error("missing_background_consistency_lock");
  for (const fact of spec.consistencyLock.sceneFacts)
    assertNonEmptyString(fact, "missing_background_consistency_lock");
  return true;
}

const statementItems = (prefix, values) => values.map((statement, index) => ({
  constraintId: `${prefix}_${String(index + 1).padStart(2, "0")}`,
  statement,
}));

function evidenceItem(route, routingRecordSha256, constraintId, statementPath, sourcePointer,
  transformationType) {
  return {
    constraintId,
    statementPath,
    sourcePath: route.authoringHandoff.routingRecordPath,
    sourcePointer,
    sourceSha256: routingRecordSha256,
    authorityRole: "planning",
    transformationType,
  };
}

export function buildBackgroundVisualBrief(route, routingRecordSha256) {
  validateBackgroundRoutingRecord(route);
  if (!/^[0-9a-f]{64}$/.test(routingRecordSha256))
    throw new Error("routing_record_hash_mismatch");
  const spec = route.typeSpecification.backgroundSpecification;
  const requiredVisualStatements = statementItems("required", route.requiredElements);
  const prohibitedVisualStatements = statementItems("prohibited", route.prohibitedElements);
  const visualEvidenceMap = [];
  requiredVisualStatements.forEach((item, index) => visualEvidenceMap.push(evidenceItem(route,
    routingRecordSha256,
    item.constraintId, `/requiredVisualStatements/${index}/statement`,
    `/authoringHandoff/requiredElements/${index}`, "direct_copy")));
  prohibitedVisualStatements.forEach((item, index) => visualEvidenceMap.push(evidenceItem(route,
    routingRecordSha256,
    item.constraintId, `/prohibitedVisualStatements/${index}/statement`,
    `/authoringHandoff/prohibitedElements/${index}`, "direct_copy")));
  const normalized = [
    ["primary_subject", "/primarySubjectOrSilhouette", "/sceneContract"],
    ["visual_hierarchy", "/visualHierarchy", "/playableOrReadabilityArea"],
    ["composition", "/composition", "/composition"],
    ["palette_material", "/paletteAndMaterial", "/consistencyLock/sceneFacts"],
    ["background_policy", "/backgroundPolicy", "/finalBackgroundPolicy"],
    ["outline_policy", "/outlinePolicy", "/consistencyLock/sceneFacts"],
    ["anchor_policy", "/anchorPolicy", "/anchor"],
  ];
  normalized.forEach(([id, statementPath, sourceSuffix]) => visualEvidenceMap.push(evidenceItem(
    route, routingRecordSha256, id, statementPath,
    `/authoringHandoff/typeSpecification/backgroundSpecification${sourceSuffix}`,
    "provider_neutral_normalization")));
  const brief = {
    schemaVersion: "generated_media_visual_brief_v2",
    visualBriefId: "pending",
    guideContractVersion: "generated_media_visual_prompt_authoring_v2",
    requestId: route.requestId,
    assetType: route.assetType,
    domainType: route.domainType,
    contentId: route.contentId,
    planningSnapshotHash: route.planningSnapshotHash,
    registryVersion: route.registryVersion,
    registryRowId: route.registryRowId,
    profileKey: route.profileKey,
    planningOriginalRef: {
      planningHandoffPath: route.planningHandoffPath,
      routingRecordId: route.routingRecordId,
      routingRecordPath: route.authoringHandoff.routingRecordPath,
      routingRecordSha256,
      routingPayloadSha256: route.routingPayloadSha256,
    },
    primarySubjectOrSilhouette: spec.sceneContract,
    visualHierarchy: `${spec.playableOrReadabilityArea} | ${spec.depthLayers.join(" | ")}`,
    composition: `${spec.composition} | viewpoint: ${spec.viewpoint} | horizon: ${spec.horizon}`,
    paletteAndMaterial: spec.consistencyLock.sceneFacts.join(" | "),
    backgroundPolicy: spec.finalBackgroundPolicy,
    outlinePolicy: "Use only the line and rendering treatment explicitly bound in consistencyLock.sceneFacts.",
    anchorPolicy: `${spec.anchor.type}: ${spec.anchor.framingRegion}; focalDepth=${spec.anchor.focalDepth}`,
    requiredVisualStatements,
    prohibitedVisualStatements,
    supportingElements: statementItems("subject_inclusion", spec.subjectInclusions),
    likelyWrongObjects: statementItems("subject_exclusion", spec.subjectExclusions),
    artifactSpecificBrief: {
      backgroundProfile: {
        registryRowId: route.registryRowId,
        profileKey: route.profileKey,
      },
      backgroundSpecification: structuredClone(spec),
    },
    visualEvidenceMap,
    providerTranslationContract: {
      schemaVersion: "imagegen_background_single_image_prompt_v2",
      provider: "imagegen",
      promptAssemblyOrder: "planning_facts,prohibited_facts",
      settingsSeparated: true,
    },
    positiveStyleLock: [],
    negativeStyleLock: [],
    status: "normalized",
    validation: {
      status: "valid",
      sourceEvidence: "complete",
      backgroundSingleImage: "valid",
      providerTranslation: "valid",
    },
  };
  const payload = structuredClone(brief);
  delete payload.visualBriefId;
  delete payload.validation;
  payload.schemaVersion = "generated_media_visual_brief_hash_payload_v2";
  brief.visualBriefId = `gmbrief2.background_single_image.${route.contentId}.${hashObject(payload).slice(0, 20)}`;
  assertClosedKeys(brief, BRIEF_KEYS);
  return brief;
}

export function buildBackgroundScenePrompt(route) {
  validateBackgroundRoutingRecord(route);
  return [...route.requiredElements,
    ...route.prohibitedElements.map((value) => `Do not depict or include: ${value}`)]
    .join("\n");
}

export function buildBackgroundPromptArtifacts(route, { routingRecordSha256 }) {
  const visualBrief = buildBackgroundVisualBrief(route, routingRecordSha256);
  const scenePromptOriginal = buildBackgroundScenePrompt(route);
  const providerPayload = {
    schemaVersion: "imagegen_background_single_image_prompt_v2",
    scenePromptOriginal,
  };
  const providerSettingsIntent = {
    canvas: structuredClone(route.typeSpecification.backgroundSpecification.canvas),
    generationBackground: { mode: "opaque" },
    outputFormat: "png",
  };
  const base = `AgentDocs/planning-data/generated-media-prompts/v2/background_single_image/${route.contentId}`;
  const record = {
    schemaVersion: "generated_media_prompt_v3",
    promptRecordId: "pending",
    promptPayloadSha256: "pending",
    requestId: route.requestId,
    assetType: route.assetType,
    domainType: route.domainType,
    contentId: route.contentId,
    planningHandoffPath: route.planningHandoffPath,
    routingRecordId: route.routingRecordId,
    routingRecordPath: route.authoringHandoff.routingRecordPath,
    routingRecordSha256,
    routingPayloadSha256: route.routingPayloadSha256,
    planningSnapshotHash: route.planningSnapshotHash,
    sourcePlanningFiles: structuredClone(route.sourcePlanningFiles),
    registryVersion: route.registryVersion,
    registryRowId: route.registryRowId,
    profileKey: route.profileKey,
    provider: "imagegen",
    structureProfile: "background_single_image_v2",
    visualBrief,
    visualBriefSha256: hashObject(visualBrief),
    scenePromptOriginal,
    providerPromptPayloadHash: hashObject(providerPayload),
    providerSettingsIntent,
    providerSettingsIntentSha256: hashObject(providerSettingsIntent),
    requiredElements: structuredClone(route.requiredElements),
    prohibitedElements: structuredClone(route.prohibitedElements),
    promptMarkdownPath: "pending",
    promptMarkdownSha256: sha256Hex(markdownBytes(scenePromptOriginal)),
    status: "ready_for_generation",
    createdAt: route.createdAt,
    validation: {
      status: "valid",
      routingRecord: "valid",
      planningSnapshot: "valid",
      visualBrief: "valid",
      backgroundSpecification: "valid",
      providerPromptPayload: "valid",
      providerSettingsIntent: "valid",
      promptMarkdown: "valid",
      recordIdentity: "valid",
    },
  };
  const promptHashPayload = structuredClone(record);
  for (const key of ["promptRecordId", "promptPayloadSha256", "promptMarkdownPath",
    "status", "createdAt", "validation"]) delete promptHashPayload[key];
  promptHashPayload.schemaVersion = "generated_media_prompt_hash_payload_v3";
  record.promptPayloadSha256 = hashObject(promptHashPayload);
  record.promptRecordId = `gmprompt3.background_single_image.${route.contentId}.${record.promptPayloadSha256.slice(0, 20)}`;
  record.promptMarkdownPath = `${base}/${record.promptRecordId}.prompt.md`;
  const recordPath = `${base}/${record.promptRecordId}.json`;
  const indexPath = `${base}/prompt_index.json`;
  const recordBytes = jsonFileBytes(record);
  const promptMarkdownBytes = markdownBytes(scenePromptOriginal);
  assertClosedKeys(record, RECORD_KEYS);
  const entry = {
    promptRecordId: record.promptRecordId,
    recordSchemaVersion: "generated_media_prompt_v3",
    recordPath,
    recordSha256: sha256Hex(recordBytes),
    promptPayloadSha256: record.promptPayloadSha256,
    promptMarkdownPath: record.promptMarkdownPath,
    promptMarkdownSha256: record.promptMarkdownSha256,
    requestId: record.requestId,
    assetType: record.assetType,
    domainType: record.domainType,
    contentId: record.contentId,
    planningSnapshotHash: record.planningSnapshotHash,
    routingRecordId: record.routingRecordId,
    routingRecordSha256: record.routingRecordSha256,
    routingPayloadSha256: record.routingPayloadSha256,
    registryVersion: record.registryVersion,
    registryRowId: record.registryRowId,
    profileKey: record.profileKey,
    provider: record.provider,
    structureProfile: record.structureProfile,
    visualBriefSha256: record.visualBriefSha256,
    providerPromptPayloadHash: record.providerPromptPayloadHash,
    providerSettingsIntentSha256: record.providerSettingsIntentSha256,
    status: "ready_for_generation",
  };
  const index = {
    schemaVersion: "generated_media_prompt_index_v3",
    assetType: "background_single_image",
    contentId: record.contentId,
    entries: { [record.promptRecordId]: entry },
  };
  const indexBytes = jsonFileBytes(index);
  const generationHandoff = {
    schemaVersion: "generated_media_generation_handoff_v2",
    requestId: record.requestId,
    assetType: record.assetType,
    domainType: record.domainType,
    contentId: record.contentId,
    planningSnapshotHash: record.planningSnapshotHash,
    routingRecordId: record.routingRecordId,
    routingRecordPath: record.routingRecordPath,
    routingRecordSha256: record.routingRecordSha256,
    routingPayloadSha256: record.routingPayloadSha256,
    registryVersion: record.registryVersion,
    registryRowId: record.registryRowId,
    profileKey: record.profileKey,
    provider: record.provider,
    structureProfile: record.structureProfile,
    promptRecordId: record.promptRecordId,
    promptRecordPath: recordPath,
    promptRecordSha256: sha256Hex(recordBytes),
    promptPayloadSha256: record.promptPayloadSha256,
    promptMarkdownPath: record.promptMarkdownPath,
    promptMarkdownSha256: record.promptMarkdownSha256,
    promptIndexPath: indexPath,
    promptIndexSha256: sha256Hex(indexBytes),
    visualBriefSha256: record.visualBriefSha256,
    providerPromptPayloadHash: record.providerPromptPayloadHash,
    providerSettingsIntentSha256: record.providerSettingsIntentSha256,
    status: "ready_for_generation",
  };
  return { visualBrief, scenePromptOriginal, providerPayload, providerSettingsIntent,
    promptHashPayload, record, recordPath, recordBytes, promptMarkdownBytes,
    index, indexPath, indexBytes, generationHandoff,
    generationHandoffSha256: hashObject(generationHandoff) };
}

export function classifyBackgroundPromptExistingState(artifacts, existing) {
  const record = existing.recordBytes;
  const markdown = existing.promptMarkdownBytes;
  const index = existing.index;
  if (!record && !markdown && !index) return "new";
  if (record && !markdown) throw new Error("prompt_markdown_mismatch");
  if (!record && markdown) throw new Error("record_collision");
  if (record && !Buffer.from(record).equals(artifacts.recordBytes))
    throw new Error("record_collision");
  if (markdown && !Buffer.from(markdown).equals(artifacts.promptMarkdownBytes))
    throw new Error("prompt_markdown_mismatch");
  if (index) {
    if (index.schemaVersion !== "generated_media_prompt_index_v3"
      || index.assetType !== "background_single_image"
      || index.contentId !== artifacts.record.contentId || !index.entries
      || typeof index.entries !== "object" || Array.isArray(index.entries))
      throw new Error("index_entry_invalid");
    const entry = index.entries[artifacts.record.promptRecordId];
    if (entry && canonicalJson(entry)
      !== canonicalJson(artifacts.index.entries[artifacts.record.promptRecordId]))
      throw new Error("index_entry_invalid");
    if (!record && entry) throw new Error("index_entry_invalid");
    return entry ? "reused_identical" : "recoverable_orphan";
  }
  return record ? "recoverable_orphan" : "new";
}

export const backgroundPromptContractInternals = Object.freeze({
  BACKGROUND_SPEC_KEYS, BRIEF_KEYS, RECORD_KEYS, ROUTE_KEYS, hashObject,
  jsonFileBytes, markdownBytes, pointerEscape,
});
