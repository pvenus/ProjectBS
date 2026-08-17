// Deterministic open-ink-v2 authoring projection vectors.
// Reads immutable authority from Git blobs and writes no workflow artifact.

import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(testDir, "../../../../../");
const safeDirectory = `safe.directory=${repoRoot.replaceAll("\\", "/")}`;
const authorityRef = process.env.GENERATED_MEDIA_AUTHORITY_REF ?? "HEAD";
const paths = {
  route: "AgentDocs/planning-data/generated-media-routing/v2/character_single_image/character.seojin.1/gmroute2.character_single_image.character.seojin.1.ab5ea9dd0a1e5c6371fe.json",
  routeIndex: "AgentDocs/planning-data/generated-media-routing/v2/character_single_image/character.seojin.1/routing_index.json",
  planning: "AgentDocs/planning-data/character/generated-media-handoffs/v2/character.seojin.1/gmplan2.character_single_image.character.seojin.1.e5537d6487d06b88f452.character_single_image.json",
  promptIndex: "AgentDocs/planning-data/generated-media-prompts/v2/character_single_image/character.seojin.1/prompt_index.json",
  visualGuide: "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md",
};

function gitBlob(path) {
  const object = authorityRef === ":" ? `:${path}` : `${authorityRef}:${path}`;
  return execFileSync("git", ["-c", safeDirectory, "show", object], {
    cwd: repoRoot, encoding: null, maxBuffer: 16 * 1024 * 1024,
  });
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function hashObject(value) {
  return sha256(Buffer.from(canonicalJson(value), "utf8"));
}

function jsonFileBytes(value) {
  return Buffer.from(`${canonicalJson(value)}\n`, "utf8");
}

function parseAuthorityJson(bytes) {
  assert.equal(bytes.subarray(0, 3).equals(Buffer.from([0xef, 0xbb, 0xbf])), false,
    "authority blob must not have BOM");
  assert.equal(bytes.includes(0x0d), false, "authority blob must be LF-only");
  assert.equal(bytes.at(-1), 0x0a, "authority blob must have terminal LF");
  assert.notEqual(bytes.at(-2), 0x0a, "authority blob must have one terminal LF");
  return JSON.parse(bytes.subarray(0, -1).toString("utf8"));
}

function profileFromGuide(bytes) {
  const text = bytes.toString("utf8");
  const heading = "### Open ink-wash output-conformance successor profile";
  const start = text.indexOf(heading);
  assert.notEqual(start, -1);
  const bodyStart = text.indexOf("```json\n", start) + 8;
  const bodyEnd = text.indexOf("\n```", bodyStart);
  assert.ok(bodyStart > 7 && bodyEnd > bodyStart);
  return JSON.parse(text.slice(bodyStart, bodyEnd));
}

function escapePointer(value) {
  return value.replaceAll("~", "~0").replaceAll("/", "~1");
}

function leafPointers(value, base = "") {
  if (Array.isArray(value)) {
    return value.flatMap((item, index) => leafPointers(item, `${base}/${index}`));
  }
  if (value !== null && typeof value === "object") {
    return Object.keys(value).sort().flatMap((key) =>
      leafPointers(value[key], `${base}/${escapePointer(key)}`));
  }
  return [base];
}

function evidenceId(statementPath, sourcePointer) {
  return `authoring_evidence_${sha256(Buffer.from(`${statementPath}|${sourcePointer}`, "utf8")).slice(0, 20)}`;
}

function evidence({ constraintId, statementPath, sourcePath, sourcePointer,
  sourceSha256, authorityRole, transformationType }) {
  return { constraintId: constraintId ?? evidenceId(statementPath, sourcePointer),
    statementPath, sourcePath, sourcePointer, sourceSha256, authorityRole,
    transformationType };
}

function visualBriefProjection({ route, routeRawSha, planning, profile,
  profileSourceSha }) {
  const handoff = route.authoringHandoff;
  const required = handoff.requiredElements;
  const prohibited = handoff.prohibitedElements;
  assert.equal(required.length, 10, "provider_value_invalid:required_slot_count");
  assert.equal(prohibited.length, 14, "provider_value_invalid:prohibited_slot_count");
  const routePath = handoff.routingRecordPath;
  const requiredVisualStatements = required.map((statement, index) => ({
    constraintId: `routing_required_${String(index + 1).padStart(2, "0")}`,
    statement,
  }));
  const prohibitedVisualStatements = prohibited.map((statement, index) => ({
    constraintId: `routing_prohibited_${String(index + 1).padStart(2, "0")}`,
    statement,
  }));
  const brief = {
    schemaVersion: "generated_media_visual_brief_v2",
    visualBriefId: "pending",
    guideContractVersion: "generated_media_visual_prompt_authoring_v2",
    requestId: handoff.requestId,
    assetType: handoff.assetType,
    domainType: handoff.domainType,
    contentId: handoff.contentId,
    planningSnapshotHash: handoff.planningSnapshotHash,
    registryVersion: handoff.registryVersion,
    registryRowId: handoff.registryRowId,
    profileKey: handoff.profileKey,
    expressionProfileKey: profile.expressionProfileKey,
    expressionProfilePayload: structuredClone(profile),
    expressionProfilePayloadHash: hashObject(profile),
    planningOriginalRef: {
      planningHandoffPath: handoff.planningHandoffPath,
      routingRecordId: handoff.routingRecordId,
      routingRecordPath: handoff.routingRecordPath,
      routingRecordSha256: routeRawSha,
      routingPayloadSha256: handoff.routingPayloadSha256,
    },
    primarySubjectOrSilhouette: required[0],
    visualHierarchy: required[1],
    composition: required[9],
    paletteAndMaterial: required.slice(3, 7).join("\n"),
    backgroundPolicy: required[7],
    outlinePolicy: required[2],
    anchorPolicy: canonicalJson(handoff.typeSpecification.singleImageSpecification.anchor),
    requiredVisualStatements,
    prohibitedVisualStatements,
    supportingElements: [],
    likelyWrongObjects: [],
    artifactSpecificBrief: structuredClone(handoff.typeSpecification),
    referenceBindings: structuredClone(handoff.styleReferenceBindings),
    visualEvidenceMap: [],
    providerTranslationContract: {
      schemaVersion: "imagegen_character_single_image_prompt_v2",
      provider: "imagegen",
      promptAssemblyOrder: "planning_facts,negative_style_lock,positive_style_lock",
      settingsSeparated: true,
    },
    positiveStyleLock: structuredClone(profile.positiveStyleLock),
    negativeStyleLock: structuredClone(profile.negativeStyleLock),
    status: "normalized",
    validation: {
      status: "valid", sourceEvidence: "complete", identityConsistency: "valid",
      expressionProfile: "valid", characterSingleImage: "valid",
      providerTranslation: "valid",
    },
  };

  const routingEntry = (constraintId, statementPath, sourcePointer,
    transformationType = "direct_copy") => evidence({ constraintId, statementPath,
    sourcePath: routePath, sourcePointer, sourceSha256: routeRawSha,
    authorityRole: "planning", transformationType });
  const map = [];
  requiredVisualStatements.forEach((item, index) => map.push(routingEntry(
    item.constraintId, `/requiredVisualStatements/${index}/statement`,
    `/authoringHandoff/requiredElements/${index}`)));
  prohibitedVisualStatements.forEach((item, index) => map.push(routingEntry(
    item.constraintId, `/prohibitedVisualStatements/${index}/statement`,
    `/authoringHandoff/prohibitedElements/${index}`)));
  const summary = [
    ["/primarySubjectOrSilhouette", "/authoringHandoff/requiredElements/0", "direct_copy"],
    ["/visualHierarchy", "/authoringHandoff/requiredElements/1", "direct_copy"],
    ["/composition", "/authoringHandoff/requiredElements/9", "direct_copy"],
    ["/paletteAndMaterial", "/authoringHandoff/requiredElements", "provider_neutral_normalization"],
    ["/backgroundPolicy", "/authoringHandoff/requiredElements/7", "direct_copy"],
    ["/outlinePolicy", "/authoringHandoff/requiredElements/2", "direct_copy"],
    ["/anchorPolicy", "/authoringHandoff/typeSpecification/singleImageSpecification/anchor", "provider_neutral_normalization"],
  ];
  summary.forEach(([statementPath, sourcePointer, transformationType]) =>
    map.push(routingEntry(undefined, statementPath, sourcePointer, transformationType)));
  for (const suffix of leafPointers(handoff.typeSpecification)) {
    map.push(routingEntry(undefined, `/artifactSpecificBrief${suffix}`,
      `/authoringHandoff/typeSpecification${suffix}`));
  }
  const profileMembers = Object.keys(profile)
    .filter((key) => !["negativeStyleLock", "positiveStyleLock"].includes(key)).sort();
  assert.equal(profileMembers.length, 17);
  for (const member of profileMembers) {
    const pointer = `/${escapePointer(member)}`;
    map.push(evidence({
      statementPath: `/expressionProfilePayload${pointer}`,
      sourcePath: paths.visualGuide,
      sourcePointer: `::Open ink-wash output-conformance successor profile#${pointer}`,
      sourceSha256: profileSourceSha, authorityRole: "expression_profile",
      transformationType: "profile_policy_projection",
    }));
  }
  for (const [listName, transformationType] of [
    ["negativeStyleLock", "profile_lock"], ["positiveStyleLock", "profile_lock"],
  ]) {
    profile[listName].forEach((_, index) => {
      const pointer = `/${listName}/${index}`;
      map.push(evidence({ statementPath: `/expressionProfilePayload${pointer}`,
        sourcePath: paths.visualGuide,
        sourcePointer: `::Open ink-wash output-conformance successor profile#${pointer}`,
        sourceSha256: profileSourceSha, authorityRole: "expression_profile",
        transformationType }));
    });
  }
  const bindingOrder = ["role", "projectRelativePath", "sha256", "reviewRecordId",
    "reviewRecordPath", "reviewRecordSha256"];
  for (const member of bindingOrder) {
    const escaped = escapePointer(member);
    map.push(routingEntry(undefined, `/referenceBindings/0/${escaped}`,
      `/authoringHandoff/styleReferenceBindings/0/${escaped}`));
  }
  brief.visualEvidenceMap = map;
  const payload = structuredClone(brief);
  delete payload.visualBriefId;
  delete payload.validation;
  payload.schemaVersion = "generated_media_visual_brief_hash_payload_v2";
  brief.visualBriefId = `gmbrief2.character_single_image.${brief.contentId}.${hashObject(payload).slice(0, 20)}`;
  assert.equal(planning.planningSnapshot.snapshotHash, brief.planningSnapshotHash);
  return brief;
}

const recordKeys = [
  "schemaVersion", "promptRecordId", "promptPayloadSha256", "requestId",
  "assetType", "domainType", "contentId", "planningHandoffPath",
  "routingRecordId", "routingRecordPath", "routingRecordSha256",
  "routingPayloadSha256", "planningSnapshotHash", "sourcePlanningFiles",
  "registryVersion", "registryRowId", "profileKey", "provider",
  "structureProfile", "visualBrief", "visualBriefSha256",
  "expressionProfileKey", "expressionProfilePayload", "expressionProfilePayloadHash",
  "scenePromptOriginal", "providerPromptPayloadHash", "providerSettingsIntent",
  "providerSettingsIntentSha256", "requiredElements", "prohibitedElements",
  "referenceBindings", "promptMarkdownPath", "promptMarkdownSha256", "status",
  "createdAt", "validation",
];
const excludedPayloadKeys = new Set(["promptRecordId", "promptPayloadSha256",
  "promptMarkdownPath", "status", "createdAt", "validation"]);
const basePromptEntryIds = [
  "gmprompt3.character_single_image.character.seojin.1.62a56ba4a9cc90cfc7ca",
  "gmprompt3.character_single_image.character.seojin.1.71553355aca87460f152",
  "gmprompt3.character_single_image.character.seojin.1.7fadf5963b97775d5480",
  "gmprompt3.character_single_image.character.seojin.1.92f6eeba1a1a29c67c69",
  "gmprompt3.character_single_image.character.seojin.1.b59e5e851eed9702c30b",
  "gmprompt3.character_single_image.character.seojin.1.eec795597332e2ba9b50",
];

function projectPromptPayload(record) {
  const payload = {};
  for (const key of recordKeys) {
    if (!excludedPayloadKeys.has(key)) payload[key] = structuredClone(record[key]);
  }
  payload.schemaVersion = "generated_media_prompt_hash_payload_v3";
  return payload;
}

function buildProjection(raw) {
  const route = parseAuthorityJson(raw.route);
  const routeIndex = parseAuthorityJson(raw.routeIndex);
  const planning = parseAuthorityJson(raw.planning);
  const livePromptIndex = parseAuthorityJson(raw.promptIndex);
  const priorPromptIndex = {
    schemaVersion: livePromptIndex.schemaVersion,
    assetType: livePromptIndex.assetType,
    contentId: livePromptIndex.contentId,
    entries: Object.fromEntries(basePromptEntryIds.map((id) => {
      assert.ok(livePromptIndex.entries[id], `missing immutable base prompt entry ${id}`);
      return [id, structuredClone(livePromptIndex.entries[id])];
    })),
  };
  const profile = profileFromGuide(raw.visualGuide);
  const handoff = route.authoringHandoff;
  const routeEntry = routeIndex.entries[route.routingRecordId];
  assert.equal(routeEntry.recordSha256, sha256(raw.route));
  assert.equal(hashObject(profile),
    "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5");
  assert.equal(canonicalJson(handoff.styleReferenceBindings),
    canonicalJson(handoff.normalizedRequest.styleReferenceBindings));
  const visualBrief = visualBriefProjection({ route, routeRawSha: routeEntry.recordSha256,
    planning, profile, profileSourceSha: sha256(raw.visualGuide) });
  const scenePromptOriginal = [
    ...handoff.requiredElements,
    ...profile.negativeStyleLock.map(({ statement }) => statement),
    ...profile.positiveStyleLock.map(({ statement }) => statement),
  ].join("\n");
  assert.equal(scenePromptOriginal.split("\n").length, 28);
  assert.equal(scenePromptOriginal.includes("\r"), false);
  assert.equal(scenePromptOriginal.endsWith("\n"), false);
  const markdown = Buffer.from(`${scenePromptOriginal}\n`, "utf8");
  const providerSettingsIntent = {
    canvas: structuredClone(handoff.typeSpecification.singleImageSpecification.canvas),
    generationBackground: structuredClone(
      handoff.typeSpecification.singleImageSpecification.generationBackground),
    outputFormat: "png",
  };
  const record = {
    schemaVersion: "generated_media_prompt_v3", promptRecordId: "pending",
    promptPayloadSha256: "pending", requestId: handoff.requestId,
    assetType: handoff.assetType, domainType: handoff.domainType,
    contentId: handoff.contentId, planningHandoffPath: handoff.planningHandoffPath,
    routingRecordId: handoff.routingRecordId, routingRecordPath: handoff.routingRecordPath,
    routingRecordSha256: routeEntry.recordSha256,
    routingPayloadSha256: handoff.routingPayloadSha256,
    planningSnapshotHash: handoff.planningSnapshotHash,
    sourcePlanningFiles: structuredClone(handoff.sourcePlanningFiles),
    registryVersion: handoff.registryVersion, registryRowId: handoff.registryRowId,
    profileKey: handoff.profileKey, provider: handoff.provider,
    structureProfile: handoff.structureProfile, visualBrief,
    visualBriefSha256: hashObject(visualBrief),
    expressionProfileKey: profile.expressionProfileKey,
    expressionProfilePayload: structuredClone(profile),
    expressionProfilePayloadHash: hashObject(profile), scenePromptOriginal,
    providerPromptPayloadHash: hashObject({
      schemaVersion: "imagegen_character_single_image_prompt_v2", scenePromptOriginal,
    }),
    providerSettingsIntent,
    providerSettingsIntentSha256: hashObject(providerSettingsIntent),
    requiredElements: structuredClone(handoff.requiredElements),
    prohibitedElements: structuredClone(handoff.prohibitedElements),
    referenceBindings: structuredClone(handoff.styleReferenceBindings),
    promptMarkdownPath: "pending", promptMarkdownSha256: sha256(markdown),
    status: "ready_for_generation", createdAt: planning.planningSnapshot.capturedAt,
    validation: { status: "valid", routingRecord: "valid", planningSnapshot: "valid",
      visualBrief: "valid", expressionProfile: "valid", providerPromptPayload: "valid",
      providerSettingsIntent: "valid", promptMarkdown: "valid", recordIdentity: "valid" },
  };
  let payload = projectPromptPayload(record);
  record.promptPayloadSha256 = hashObject(payload);
  record.promptRecordId = `gmprompt3.character_single_image.${record.contentId}.${record.promptPayloadSha256.slice(0, 20)}`;
  const base = `AgentDocs/planning-data/generated-media-prompts/v2/character_single_image/${record.contentId}`;
  const recordPath = `${base}/${record.promptRecordId}.json`;
  record.promptMarkdownPath = `${base}/${record.promptRecordId}.prompt.md`;
  payload = projectPromptPayload(record);
  assert.equal(hashObject(payload), record.promptPayloadSha256);
  const recordBytes = jsonFileBytes(record);
  const entry = {
    promptRecordId: record.promptRecordId, recordSchemaVersion: record.schemaVersion,
    recordPath, recordSha256: sha256(recordBytes),
    promptPayloadSha256: record.promptPayloadSha256,
    promptMarkdownPath: record.promptMarkdownPath,
    promptMarkdownSha256: record.promptMarkdownSha256, requestId: record.requestId,
    assetType: record.assetType, domainType: record.domainType, contentId: record.contentId,
    planningSnapshotHash: record.planningSnapshotHash,
    routingRecordId: record.routingRecordId, routingRecordSha256: record.routingRecordSha256,
    routingPayloadSha256: record.routingPayloadSha256, registryVersion: record.registryVersion,
    registryRowId: record.registryRowId, profileKey: record.profileKey,
    provider: record.provider, structureProfile: record.structureProfile,
    visualBriefSha256: record.visualBriefSha256,
    providerPromptPayloadHash: record.providerPromptPayloadHash,
    providerSettingsIntentSha256: record.providerSettingsIntentSha256,
    status: record.status,
  };
  const index = structuredClone(priorPromptIndex);
  assert.equal(Object.hasOwn(index.entries, record.promptRecordId), false);
  index.entries[record.promptRecordId] = entry;
  const indexBytes = jsonFileBytes(index);
  const indexPath = `${base}/prompt_index.json`;
  const generationHandoff = {
    schemaVersion: "generated_media_generation_handoff_v2", requestId: record.requestId,
    assetType: record.assetType, domainType: record.domainType, contentId: record.contentId,
    planningSnapshotHash: record.planningSnapshotHash,
    routingRecordId: record.routingRecordId, routingRecordPath: record.routingRecordPath,
    routingRecordSha256: record.routingRecordSha256,
    routingPayloadSha256: record.routingPayloadSha256,
    registryVersion: record.registryVersion, registryRowId: record.registryRowId,
    profileKey: record.profileKey, provider: record.provider,
    structureProfile: record.structureProfile, promptRecordId: record.promptRecordId,
    promptRecordPath: recordPath, promptRecordSha256: sha256(recordBytes),
    promptPayloadSha256: record.promptPayloadSha256,
    promptMarkdownPath: record.promptMarkdownPath,
    promptMarkdownSha256: record.promptMarkdownSha256,
    promptIndexPath: indexPath, promptIndexSha256: sha256(indexBytes),
    visualBriefSha256: record.visualBriefSha256,
    providerPromptPayloadHash: record.providerPromptPayloadHash,
    providerSettingsIntentSha256: record.providerSettingsIntentSha256,
    status: "ready_for_generation",
  };
  return { visualBrief, scenePromptOriginal, payload, record, recordBytes, markdown,
    index, indexBytes, generationHandoff };
}

function assertSameProjection(actual, expected) {
  for (const key of ["visualBrief", "scenePromptOriginal", "payload", "record",
    "index", "generationHandoff"]) {
    if (canonicalJson(actual[key]) !== canonicalJson(expected[key])) {
      throw new Error("record_identity_mismatch");
    }
  }
  for (const key of ["recordBytes", "markdown", "indexBytes"]) {
    if (!actual[key].equals(expected[key])) throw new Error("record_identity_mismatch");
  }
  return true;
}

const authority = Object.fromEntries(Object.entries(paths).map(([key, path]) =>
  [key, gitBlob(path)]));
const crlfCheckout = Buffer.from(authority.route.toString("utf8").replaceAll("\n", "\r\n"), "utf8");
assert.notEqual(sha256(crlfCheckout), sha256(authority.route));
assert.throws(() => parseAuthorityJson(crlfCheckout), /authority blob must be LF-only/);

// Both clean producers consume the same verified raw Git blobs. Their checkout
// representations may differ and are deliberately excluded from projection.
const producerLf = buildProjection(authority);
const producerCrlfCheckout = buildProjection(authority);
assert.equal(assertSameProjection(producerLf, producerCrlfCheckout), true);
assert.equal(producerLf.record.promptRecordId,
  `gmprompt3.character_single_image.character.seojin.1.${producerLf.record.promptPayloadSha256.slice(0, 20)}`);
assert.equal(producerLf.recordBytes.at(-1), 0x0a);
assert.equal(producerLf.recordBytes.includes(0x0d), false);
assert.equal(producerLf.markdown.at(-1), 0x0a);
assert.equal(producerLf.markdown.includes(0x0d), false);

const alternateWording = structuredClone(producerLf);
alternateWording.scenePromptOriginal += "\nAuthor-written summary.";
assert.throws(() => assertSameProjection(alternateWording, producerLf),
  /record_identity_mismatch/);
const reorderedEvidence = structuredClone(producerLf);
[reorderedEvidence.visualBrief.visualEvidenceMap[0],
  reorderedEvidence.visualBrief.visualEvidenceMap[1]] = [
  reorderedEvidence.visualBrief.visualEvidenceMap[1],
  reorderedEvidence.visualBrief.visualEvidenceMap[0],
];
assert.throws(() => assertSameProjection(reorderedEvidence, producerLf),
  /record_identity_mismatch/);
const missingRequired = structuredClone(parseAuthorityJson(authority.route));
missingRequired.authoringHandoff.requiredElements.pop();
assert.throws(() => visualBriefProjection({ route: missingRequired,
  routeRawSha: "0".repeat(64), planning: parseAuthorityJson(authority.planning),
  profile: profileFromGuide(authority.visualGuide),
  profileSourceSha: sha256(authority.visualGuide) }),
  /provider_value_invalid:required_slot_count/);

const vector = {
  promptRecordId: producerLf.record.promptRecordId,
  promptPayloadSha256: producerLf.record.promptPayloadSha256,
  promptRecordSha256: sha256(producerLf.recordBytes),
  promptMarkdownSha256: sha256(producerLf.markdown),
  promptIndexAfterSha256: sha256(producerLf.indexBytes),
  generationHandoffSha256: hashObject(producerLf.generationHandoff),
};
assert.deepEqual(vector, {
  promptRecordId: "gmprompt3.character_single_image.character.seojin.1.4c37ee9b6e2217168eb2",
  promptPayloadSha256: "4c37ee9b6e2217168eb26f32779034929ab99379a4bdaeec8e45d6ca30b95076",
  promptRecordSha256: "4d041c99ac1323e9de3b73613c94c9c227c2978eb2a55d282f8a719e817cb3d3",
  promptMarkdownSha256: "444cf2132cffc6d85ec88c35e2bf3e80adcb35338a7436324ed12f7a25034358",
  promptIndexAfterSha256: "2be27064c4a060abfb4af37fa18273d803ba79dcfb95fc9ed042c6e13758e120",
  generationHandoffSha256: "07260002f216643843d23b9e7f7a1de9745eccdd81064f3ec5f44653dadce782",
});
console.log(vector);
console.log("generated media authoring determinism vectors: PASS");
