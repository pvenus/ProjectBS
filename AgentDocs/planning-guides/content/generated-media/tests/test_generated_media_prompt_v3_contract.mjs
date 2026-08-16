// Executable closed-record vectors for character_single_image prompt v3.
// Fixture values stay inside JSON.stringify's RFC 8785-compatible subset.
// This is a contract vector, not a general JCS implementation or file writer.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function hashObject(value) {
  return sha256(Buffer.from(canonicalJson(value), "utf8"));
}

function jsonFileBytes(value) {
  return Buffer.from(`${canonicalJson(value)}\n`, "utf8");
}

function assertClosedKeys(value, required, optional = []) {
  const allowed = new Set([...required, ...optional]);
  for (const key of Object.keys(value)) {
    if (!allowed.has(key)) throw new Error(`unknown_record_field:${key}`);
  }
  for (const key of required) {
    if (!Object.hasOwn(value, key)) throw new Error(`missing_record_field:${key}`);
  }
}

const recordKeys = [
  "schemaVersion", "promptRecordId", "promptPayloadSha256", "requestId",
  "assetType", "domainType", "contentId", "planningHandoffPath",
  "routingRecordId", "routingRecordPath", "routingRecordSha256",
  "routingPayloadSha256", "planningSnapshotHash", "sourcePlanningFiles",
  "registryVersion", "registryRowId", "profileKey", "provider",
  "structureProfile", "visualBrief", "visualBriefSha256",
  "expressionProfileKey", "expressionProfilePayload",
  "expressionProfilePayloadHash", "scenePromptOriginal",
  "providerPromptPayloadHash", "providerSettingsIntent",
  "providerSettingsIntentSha256", "requiredElements", "prohibitedElements",
  "promptMarkdownPath", "promptMarkdownSha256", "status", "createdAt",
  "validation",
];

const payloadKeys = recordKeys.filter((key) => ![
  "promptRecordId", "promptPayloadSha256", "promptMarkdownPath", "status",
  "createdAt", "validation",
].includes(key));

const indexEntryKeys = [
  "promptRecordId", "recordSchemaVersion", "recordPath", "recordSha256",
  "promptPayloadSha256", "promptMarkdownPath", "promptMarkdownSha256",
  "requestId", "assetType", "domainType", "contentId",
  "planningSnapshotHash", "routingRecordId", "routingRecordSha256",
  "routingPayloadSha256", "registryVersion", "registryRowId", "profileKey",
  "provider", "structureProfile", "visualBriefSha256",
  "providerPromptPayloadHash", "providerSettingsIntentSha256", "status",
];

const handoffKeys = [
  "schemaVersion", "requestId", "assetType", "domainType", "contentId",
  "planningSnapshotHash", "routingRecordId", "routingRecordPath",
  "routingRecordSha256", "routingPayloadSha256", "registryVersion",
  "registryRowId", "profileKey", "provider", "structureProfile",
  "promptRecordId", "promptRecordPath", "promptRecordSha256",
  "promptPayloadSha256", "promptMarkdownPath", "promptMarkdownSha256",
  "promptIndexPath", "promptIndexSha256", "visualBriefSha256",
  "providerPromptPayloadHash", "providerSettingsIntentSha256", "status",
];

const visualBriefKeys = [
  "schemaVersion", "visualBriefId", "guideContractVersion", "requestId",
  "assetType", "domainType", "contentId", "planningSnapshotHash",
  "registryVersion", "registryRowId", "profileKey", "expressionProfileKey",
  "expressionProfilePayload", "expressionProfilePayloadHash",
  "planningOriginalRef", "primarySubjectOrSilhouette", "visualHierarchy",
  "composition", "paletteAndMaterial", "backgroundPolicy", "outlinePolicy",
  "anchorPolicy", "requiredVisualStatements", "prohibitedVisualStatements",
  "supportingElements", "likelyWrongObjects", "artifactSpecificBrief",
  "visualEvidenceMap", "providerTranslationContract", "positiveStyleLock",
  "negativeStyleLock", "status", "validation",
];

function validateVisualBrief(brief) {
  assertClosedKeys(brief, visualBriefKeys, ["animationRequestId"]);
  if (Object.hasOwn(brief, "animationRequestId")) throw new Error("unknown_record_field:animationRequestId");
  assertClosedKeys(brief.planningOriginalRef, [
    "planningHandoffPath", "routingRecordId", "routingRecordPath",
    "routingRecordSha256", "routingPayloadSha256",
  ]);
  const expressionKey = brief.expressionProfilePayload.expressionProfileKey;
  if (!["projectbs_character_restrained_ink_line@1.0.0",
    "projectbs_character_animation_ready_minimal_ink_line@1.0.0",
    "projectbs_character_sparse_ink_pastel_motion@1.0.0",
    "projectbs_character_open_ink_wash_dynamic_contour@1.0.0"].includes(expressionKey)) {
    throw new Error("expression_profile_key_mismatch");
  }
  const sparse = expressionKey === "projectbs_character_sparse_ink_pastel_motion@1.0.0";
  const openInk = expressionKey === "projectbs_character_open_ink_wash_dynamic_contour@1.0.0";
  const profileKeys = sparse
    ? ["expressionProfileKey", "contourOmissionBudget", "lineHierarchy",
      "negativeSpacePolicy", "pigmentBudget", "accentPalette", "pigmentApplication",
      "motionLinePolicy", "identityAnchors"]
    : openInk
    ? ["expressionProfileKey", "applicability", "proportionAndAgeContract",
      "contourOmissionBudget", "mokSeonContract", "pigmentApplicationContract",
      "paletteRoleContract", "negativeSpaceContract", "backgroundContract",
      "identityAnchorContract", "acceptedStyleReferenceContract",
      "authoringProjectionContract", "negativeStyleLock", "positiveStyleLock"]
    : expressionKey === "projectbs_character_animation_ready_minimal_ink_line@1.0.0"
    ? ["expressionProfileKey", "proportionProjection", "detailDensityBudget",
      "colorValueBudget", "authoringProjectionContract", "negativeStyleLock",
      "positiveStyleLock"]
    : ["expressionProfileKey", "negativeStyleLock", "positiveStyleLock"];
  assertClosedKeys(brief.expressionProfilePayload, profileKeys);
  if (sparse && (brief.negativeStyleLock.length !== 0 || brief.positiveStyleLock.length !== 0))
    throw new Error("sparse_profile_projection_mismatch");
  const payloadLocks = sparse ? [] : [...brief.expressionProfilePayload.negativeStyleLock,
    ...brief.expressionProfilePayload.positiveStyleLock];
  for (const lock of [...payloadLocks,
    ...brief.negativeStyleLock, ...brief.positiveStyleLock]) {
    assertClosedKeys(lock, ["constraintId", "statement", "authorityRef"]);
  }
  for (const list of ["requiredVisualStatements", "prohibitedVisualStatements",
    "supportingElements", "likelyWrongObjects"]) {
    for (const item of brief[list]) assertClosedKeys(item, ["constraintId", "statement"]);
  }
  assertClosedKeys(brief.artifactSpecificBrief, ["identityConsistencyLock", "singleImageSpecification"]);
  assertClosedKeys(brief.artifactSpecificBrief.identityConsistencyLock, ["identityId", "referenceFacts"]);
  const spec = brief.artifactSpecificBrief.singleImageSpecification;
  assertClosedKeys(spec, [
    "viewpoint", "pose", "framing", "canvas", "targetDisplaySize", "safeArea",
    "finalBackgroundPolicy", "generationBackground", "noShadow", "outline", "anchor",
  ]);
  assertClosedKeys(spec.canvas, ["width", "height"]);
  assertClosedKeys(spec.targetDisplaySize, ["width", "height"]);
  assertClosedKeys(spec.generationBackground, ["mode", "color"]);
  assertClosedKeys(spec.outline, ["enabled"], ["color", "exactThicknessPx", "placement"]);
  assertClosedKeys(spec.anchor, ["type", "pelvisOrRootPoint", "groundContactAxis"]);
  for (const evidence of brief.visualEvidenceMap) assertClosedKeys(evidence, [
    "constraintId", "statementPath", "sourcePath", "sourcePointer", "sourceSha256",
    "authorityRole", "transformationType",
  ]);
  assertClosedKeys(brief.providerTranslationContract, [
    "schemaVersion", "provider", "promptAssemblyOrder", "settingsSeparated",
  ]);
  assertClosedKeys(brief.validation, [
    "status", "sourceEvidence", "identityConsistency", "expressionProfile",
    "characterSingleImage", "providerTranslation",
  ]);
}

function expressionProfile() {
  const authority = "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile";
  const master = "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::10. 기본 시각 표현 경계";
  return {
    expressionProfileKey: "projectbs_character_restrained_ink_line@1.0.0",
    negativeStyleLock: [
      { constraintId: "char_ink_negative_photographic", statement: "No photographic, photorealistic, cinematic-portrait, or live-action rendering.", authorityRef: master },
      { constraintId: "char_ink_negative_skin_microtexture", statement: "No realistic skin pores or photographic skin and material microtexture.", authorityRef: authority },
      { constraintId: "char_ink_negative_lens_depth", statement: "No lens, focal-length, depth-of-field, bokeh, or camera-capture language.", authorityRef: authority },
      { constraintId: "char_ink_negative_volumetric_light", statement: "No volumetric or cinematic portrait lighting and no physically modeled lighting.", authorityRef: authority },
      { constraintId: "char_ink_negative_3d_western_realism", statement: "No painterly 3D render, PBR material render, glossy game-cinematic model, or western-fantasy realism.", authorityRef: master },
      { constraintId: "char_ink_negative_heavy_wash", statement: "No heavy ink-wash flooding or uncontrolled brush texture that hides character identity.", authorityRef: authority },
    ],
    positiveStyleLock: [
      { constraintId: "char_ink_positive_limited_line_palette", statement: "Use restrained ink and brush line drawing with a limited black and gray line vocabulary.", authorityRef: authority },
      { constraintId: "char_ink_positive_silhouette_gesture", statement: "Make the primary silhouette and gesture readable before internal detail.", authorityRef: authority },
      { constraintId: "char_ink_positive_line_hierarchy", statement: "Use clear primary contours, identity-defining face, costume, and weapon lines, and sparse subordinate folds.", authorityRef: authority },
      { constraintId: "char_ink_positive_controlled_variation", statement: "Use controlled variation in line width, density, taper, pressure, and occasional breaks rather than uniform technical outlines.", authorityRef: authority },
      { constraintId: "char_ink_positive_negative_space_value", statement: "Preserve open negative space and flat minimal value masses without dense modeled shading.", authorityRef: authority },
      { constraintId: "char_ink_positive_identity_preservation", statement: "Keep the approved face, costume layers, equipment, weapon, and palette identity-readable without ornamental expansion.", authorityRef: authority },
    ],
  };
}

function visualBriefFixture(profile) {
  const brief = {
    schemaVersion: "generated_media_visual_brief_v2",
    visualBriefId: "pending",
    guideContractVersion: "generated_media_visual_prompt_authoring_v2",
    requestId: "gmreq.character.contract_vector.1",
    assetType: "character_single_image",
    domainType: "character",
    contentId: "character.contract_vector.1",
    planningSnapshotHash: "a".repeat(64),
    registryVersion: "generated_media_authoring_profile_registry_v2",
    registryRowId: "character_single_image_v2",
    profileKey: "character_single_image@2.0.0",
    expressionProfileKey: profile.expressionProfileKey,
    expressionProfilePayload: structuredClone(profile),
    expressionProfilePayloadHash: hashObject(profile),
    planningOriginalRef: {
      planningHandoffPath: "AgentDocs/planning-data/generated-media-planning/v2/character.contract_vector.1.json",
      routingRecordId: `gmroute2.character_single_image.character.contract_vector.1.${"b".repeat(20)}`,
      routingRecordPath: `AgentDocs/planning-data/generated-media-routing/v2/character_single_image/character.contract_vector.1/gmroute2.character_single_image.character.contract_vector.1.${"b".repeat(20)}.json`,
      routingRecordSha256: "c".repeat(64),
      routingPayloadSha256: "b".repeat(64),
    },
    primarySubjectOrSilhouette: "one approved character",
    visualHierarchy: "primary silhouette before internal detail",
    composition: "one approved front viewpoint and full-body framing",
    paletteAndMaterial: "approved planning palette and materials only",
    backgroundPolicy: "transparent final over removable solid generation background",
    outlinePolicy: "no outline",
    anchorPolicy: "pelvis root aligned to the approved ground-contact axis",
    requiredVisualStatements: [{ constraintId: "required_character", statement: "Show one full character." }],
    prohibitedVisualStatements: [{ constraintId: "prohibited_text", statement: "No text." }],
    supportingElements: [],
    likelyWrongObjects: [],
    artifactSpecificBrief: {
      identityConsistencyLock: { identityId: "character.contract_vector.1", referenceFacts: ["approved identity"] },
      singleImageSpecification: {
        viewpoint: "front", pose: "neutral", framing: "full_body",
        canvas: { width: 1024, height: 1024 },
        targetDisplaySize: { width: 256, height: 256 },
        safeArea: "16px inset", finalBackgroundPolicy: "transparent",
        generationBackground: { mode: "removable_solid", color: "#00ff00" },
        noShadow: true, outline: { enabled: false },
        anchor: { type: "pelvis_root_ground_axis", pelvisOrRootPoint: "canvas_center_x", groundContactAxis: "bottom_safe_area" },
      },
    },
    visualEvidenceMap: [{
      constraintId: "required_character", statementPath: "/requiredVisualStatements/0/statement",
      sourcePath: "AgentDocs/planning-data/character/contract_vector.json",
      sourcePointer: "/appearance", sourceSha256: "d".repeat(64),
      authorityRole: "planning", transformationType: "provider_neutral_normalization",
    }],
    providerTranslationContract: {
      schemaVersion: "imagegen_character_single_image_prompt_v2", provider: "imagegen",
      promptAssemblyOrder: ["planning_facts", "negative_style_lock", "positive_style_lock"],
      settingsSeparated: true,
    },
    positiveStyleLock: structuredClone(profile.positiveStyleLock),
    negativeStyleLock: structuredClone(profile.negativeStyleLock),
    status: "normalized",
    validation: {
      status: "valid", sourceEvidence: "complete", identityConsistency: "valid",
      expressionProfile: "valid", characterSingleImage: "valid", providerTranslation: "valid",
    },
  };
  const projection = structuredClone(brief);
  delete projection.visualBriefId;
  delete projection.validation;
  projection.schemaVersion = "generated_media_visual_brief_hash_payload_v2";
  brief.visualBriefId = `gmbrief2.character_single_image.${brief.contentId}.${hashObject(projection).slice(0, 20)}`;
  return brief;
}

function markdownBytes(scenePromptOriginal) {
  if (!scenePromptOriginal || scenePromptOriginal.includes("\r") || scenePromptOriginal.endsWith("\n")) {
    throw new Error("prompt_markdown_mismatch");
  }
  return Buffer.from(`${scenePromptOriginal}\n`, "utf8");
}

function projectPromptPayload(record) {
  assertClosedKeys(record, recordKeys);
  const payload = {};
  for (const key of payloadKeys) payload[key] = structuredClone(record[key]);
  payload.schemaVersion = "generated_media_prompt_hash_payload_v3";
  return payload;
}

function promptId(payload) {
  return `gmprompt3.character_single_image.${payload.contentId}.${hashObject(payload).slice(0, 20)}`;
}

function buildArtifacts() {
  const profile = expressionProfile();
  assert.equal(hashObject(profile), "bda082ffe297c29cdc6b933a6c219ae67b11ae38bc784c198e4603c1741199cf");
  const visualBrief = visualBriefFixture(profile);
  const scenePromptOriginal = [
    "Show one full approved character. No text.",
    ...profile.negativeStyleLock.map(({ statement }) => statement),
    ...profile.positiveStyleLock.map(({ statement }) => statement),
  ].join("\n");
  const markdown = markdownBytes(scenePromptOriginal);
  const providerSettingsIntent = {
    canvas: { width: 1024, height: 1024 },
    generationBackground: { mode: "removable_solid", color: "#00ff00" },
    outputFormat: "png",
  };
  assertClosedKeys(providerSettingsIntent, ["canvas", "generationBackground", "outputFormat"]);
  assertClosedKeys(providerSettingsIntent.canvas, ["width", "height"]);
  assertClosedKeys(providerSettingsIntent.generationBackground, ["mode", "color"]);
  const record = {
    schemaVersion: "generated_media_prompt_v3", promptRecordId: "pending",
    promptPayloadSha256: "pending", requestId: "gmreq.character.contract_vector.1",
    assetType: "character_single_image", domainType: "character",
    contentId: "character.contract_vector.1",
    planningHandoffPath: "AgentDocs/planning-data/generated-media-planning/v2/character.contract_vector.1.json",
    routingRecordId: visualBrief.planningOriginalRef.routingRecordId,
    routingRecordPath: visualBrief.planningOriginalRef.routingRecordPath,
    routingRecordSha256: visualBrief.planningOriginalRef.routingRecordSha256,
    routingPayloadSha256: visualBrief.planningOriginalRef.routingPayloadSha256,
    planningSnapshotHash: "a".repeat(64),
    sourcePlanningFiles: [{ path: "AgentDocs/planning-data/character/contract_vector.json", role: "approved_character_plan", sha256: "d".repeat(64) }],
    registryVersion: "generated_media_authoring_profile_registry_v2",
    registryRowId: "character_single_image_v2", profileKey: "character_single_image@2.0.0",
    provider: "imagegen", structureProfile: "character_single_image_v2",
    visualBrief, visualBriefSha256: hashObject(visualBrief),
    expressionProfileKey: profile.expressionProfileKey,
    expressionProfilePayload: profile, expressionProfilePayloadHash: hashObject(profile),
    scenePromptOriginal,
    providerPromptPayloadHash: hashObject({ schemaVersion: "imagegen_character_single_image_prompt_v2", scenePromptOriginal }),
    providerSettingsIntent, providerSettingsIntentSha256: hashObject(providerSettingsIntent),
    requiredElements: ["one full character"], prohibitedElements: ["text"],
    promptMarkdownPath: "pending", promptMarkdownSha256: sha256(markdown),
    status: "ready_for_generation", createdAt: "2026-08-14T09:30:00+09:00",
    validation: {
      status: "valid", routingRecord: "valid", planningSnapshot: "valid",
      visualBrief: "valid", expressionProfile: "valid", providerPromptPayload: "valid",
      providerSettingsIntent: "valid", promptMarkdown: "valid", recordIdentity: "valid",
    },
  };
  for (const source of record.sourcePlanningFiles) assertClosedKeys(source, ["path", "role", "sha256"], ["revision"]);
  assertClosedKeys(record.validation, [
    "status", "routingRecord", "planningSnapshot", "visualBrief", "expressionProfile",
    "providerPromptPayload", "providerSettingsIntent", "promptMarkdown", "recordIdentity",
  ]);
  let payload = projectPromptPayload(record);
  record.promptPayloadSha256 = hashObject(payload);
  record.promptRecordId = promptId(payload);
  const base = `AgentDocs/planning-data/generated-media-prompts/v2/character_single_image/${record.contentId}`;
  const recordPath = `${base}/${record.promptRecordId}.json`;
  record.promptMarkdownPath = `${base}/${record.promptRecordId}.prompt.md`;
  payload = projectPromptPayload(record);
  assert.equal(hashObject(payload), record.promptPayloadSha256);
  const recordBytes = jsonFileBytes(record);
  const entry = {
    promptRecordId: record.promptRecordId, recordSchemaVersion: record.schemaVersion,
    recordPath, recordSha256: sha256(recordBytes), promptPayloadSha256: record.promptPayloadSha256,
    promptMarkdownPath: record.promptMarkdownPath, promptMarkdownSha256: record.promptMarkdownSha256,
    requestId: record.requestId, assetType: record.assetType, domainType: record.domainType,
    contentId: record.contentId, planningSnapshotHash: record.planningSnapshotHash,
    routingRecordId: record.routingRecordId, routingRecordSha256: record.routingRecordSha256,
    routingPayloadSha256: record.routingPayloadSha256, registryVersion: record.registryVersion,
    registryRowId: record.registryRowId, profileKey: record.profileKey, provider: record.provider,
    structureProfile: record.structureProfile, visualBriefSha256: record.visualBriefSha256,
    providerPromptPayloadHash: record.providerPromptPayloadHash,
    providerSettingsIntentSha256: record.providerSettingsIntentSha256, status: record.status,
  };
  assertClosedKeys(entry, indexEntryKeys);
  const indexPath = `${base}/prompt_index.json`;
  const index = { schemaVersion: "generated_media_prompt_index_v3", assetType: record.assetType, contentId: record.contentId, entries: { [record.promptRecordId]: entry } };
  assertClosedKeys(index, ["schemaVersion", "assetType", "contentId", "entries"]);
  const indexBytes = jsonFileBytes(index);
  const handoff = {
    schemaVersion: "generated_media_generation_handoff_v2", requestId: record.requestId,
    assetType: record.assetType, domainType: record.domainType, contentId: record.contentId,
    planningSnapshotHash: record.planningSnapshotHash, routingRecordId: record.routingRecordId,
    routingRecordPath: record.routingRecordPath, routingRecordSha256: record.routingRecordSha256,
    routingPayloadSha256: record.routingPayloadSha256, registryVersion: record.registryVersion,
    registryRowId: record.registryRowId, profileKey: record.profileKey, provider: record.provider,
    structureProfile: record.structureProfile, promptRecordId: record.promptRecordId,
    promptRecordPath: recordPath, promptRecordSha256: sha256(recordBytes),
    promptPayloadSha256: record.promptPayloadSha256, promptMarkdownPath: record.promptMarkdownPath,
    promptMarkdownSha256: sha256(markdown), promptIndexPath: indexPath,
    promptIndexSha256: sha256(indexBytes), visualBriefSha256: record.visualBriefSha256,
    providerPromptPayloadHash: record.providerPromptPayloadHash,
    providerSettingsIntentSha256: record.providerSettingsIntentSha256,
    status: "ready_for_generation",
  };
  assertClosedKeys(handoff, handoffKeys);
  return { record, payload, markdown, recordBytes, recordPath, entry, index, indexBytes, indexPath, handoff };
}

function validateArtifacts(a) {
  assertClosedKeys(a.record, recordKeys);
  validateVisualBrief(a.record.visualBrief);
  if (hashObject(projectPromptPayload(a.record)) !== a.record.promptPayloadSha256 ||
      promptId(projectPromptPayload(a.record)) !== a.record.promptRecordId) {
    throw new Error("record_identity_mismatch");
  }
  if (!a.recordBytes.equals(jsonFileBytes(a.record)) || sha256(a.recordBytes) !== a.entry.recordSha256) {
    throw new Error("record_hash_mismatch");
  }
  if (!a.markdown.equals(markdownBytes(a.record.scenePromptOriginal)) ||
      sha256(a.markdown) !== a.record.promptMarkdownSha256) {
    throw new Error("prompt_markdown_mismatch");
  }
  if (canonicalJson(a.index.entries[a.record.promptRecordId]) !== canonicalJson(a.entry)) {
    throw new Error("index_entry_invalid");
  }
  if (sha256(a.indexBytes) !== a.handoff.promptIndexSha256 ||
      sha256(a.recordBytes) !== a.handoff.promptRecordSha256 ||
      sha256(a.markdown) !== a.handoff.promptMarkdownSha256) {
    throw new Error("record_hash_mismatch");
  }
  return true;
}

function inspectExisting(expected, occupied) {
  if (occupied.entry && (!occupied.recordBytes || !occupied.markdown)) throw new Error("index_entry_invalid");
  if ((occupied.recordBytes && !occupied.markdown) || (!occupied.recordBytes && occupied.markdown)) {
    throw new Error("record_collision");
  }
  if (!occupied.recordBytes && !occupied.markdown && !occupied.entry) return { status: "new" };
  if (!occupied.recordBytes.equals(expected.recordBytes)) throw new Error("record_collision");
  if (!occupied.markdown.equals(expected.markdown)) throw new Error("prompt_markdown_mismatch");
  if (!occupied.entry) return { status: "reused_identical", recoverableOrphan: true };
  if (canonicalJson(occupied.entry) !== canonicalJson(expected.entry)) throw new Error("index_entry_invalid");
  return { status: "reused_identical", recoverableOrphan: false };
}

function simulatedPublish(expected, failAt) {
  const state = { markdown: undefined, recordBytes: undefined, indexBytes: Buffer.from("prior-index\n") };
  const priorIndex = Buffer.from(state.indexBytes);
  try {
    state.markdown = Buffer.from(expected.markdown);
    if (failAt === "markdown") throw new Error("prompt_markdown_write_failed");
    state.recordBytes = Buffer.from(expected.recordBytes);
    if (failAt === "record") throw new Error("prompt_record_write_failed");
    if (failAt === "index") throw new Error("prompt_index_write_failed");
    state.indexBytes = Buffer.from(expected.indexBytes);
    return state;
  } catch (error) {
    if (state.markdown?.equals(expected.markdown)) state.markdown = undefined;
    if (state.recordBytes?.equals(expected.recordBytes)) state.recordBytes = undefined;
    state.indexBytes = priorIndex;
    assert.equal(state.markdown, undefined);
    assert.equal(state.recordBytes, undefined);
    assert.ok(state.indexBytes.equals(priorIndex));
    return { ...state, failureType: error.message, safeToRetry: true };
  }
}

const a = buildArtifacts();
assert.equal(validateArtifacts(a), true);
assert.equal(a.record.promptPayloadSha256, hashObject(a.payload));
assert.equal(a.record.promptRecordId, promptId(a.payload));
assert.equal(a.recordBytes.at(-1), 0x0a);
assert.equal(a.recordBytes.at(-2) === 0x0d, false);
assert.equal(a.markdown.at(-1), 0x0a);
assert.equal(a.markdown.at(-2) === 0x0d, false);
assert.equal(hashObject(a.handoff), hashObject(structuredClone(a.handoff)));

const unknown = structuredClone(a.record);
unknown.extra = true;
assert.throws(() => projectPromptPayload(unknown), /unknown_record_field:extra/);
const missing = structuredClone(a.record);
delete missing.provider;
assert.throws(() => projectPromptPayload(missing), /missing_record_field:provider/);
const unknownNested = structuredClone(a);
unknownNested.record.visualBrief.providerSettings = { quality: "high" };
assert.throws(() => validateArtifacts(unknownNested), /unknown_record_field:providerSettings/);

const crlf = { ...a, markdown: Buffer.from(`${a.record.scenePromptOriginal.replaceAll("\n", "\r\n")}\r\n`, "utf8") };
assert.throws(() => validateArtifacts(crlf), /prompt_markdown_mismatch/);
const changedHash = { ...a, recordBytes: Buffer.concat([a.recordBytes, Buffer.from(" ")]) };
assert.throws(() => validateArtifacts(changedHash), /record_hash_mismatch/);

assert.deepEqual(inspectExisting(a, { recordBytes: a.recordBytes, markdown: a.markdown, entry: a.entry }),
  { status: "reused_identical", recoverableOrphan: false });
assert.deepEqual(inspectExisting(a, { recordBytes: a.recordBytes, markdown: a.markdown }),
  { status: "reused_identical", recoverableOrphan: true });
assert.throws(() => inspectExisting(a, { entry: a.entry }), /index_entry_invalid/);
assert.throws(() => inspectExisting(a, { recordBytes: Buffer.concat([a.recordBytes, Buffer.from(" ")]), markdown: a.markdown }), /record_collision/);
assert.throws(() => inspectExisting(a, { recordBytes: a.recordBytes, markdown: Buffer.concat([a.markdown, Buffer.from("\n")]) }), /prompt_markdown_mismatch/);

for (const failureType of ["markdown", "record", "index"]) {
  const failed = simulatedPublish(a, failureType);
  assert.equal(failed.safeToRetry, true);
  assert.match(failed.failureType, /^prompt_(markdown|record|index)_write_failed$/);
}

console.log({
  promptRecordId: a.record.promptRecordId,
  promptPayloadSha256: a.record.promptPayloadSha256,
  promptRecordSha256: sha256(a.recordBytes),
  promptMarkdownSha256: sha256(a.markdown),
  promptIndexSha256: sha256(a.indexBytes),
  generationHandoffSha256: hashObject(a.handoff),
});
console.log("generated media character prompt v3 contract vectors: PASS");
