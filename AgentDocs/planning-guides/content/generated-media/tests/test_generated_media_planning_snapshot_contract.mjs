import assert from "node:assert/strict";
import crypto from "node:crypto";

function canonicalize(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  return `{${Object.keys(value).sort().map((key) =>
    `${JSON.stringify(key)}:${canonicalize(value[key])}`).join(",")}}`;
}

function sha256(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function selectPublishedSourceBytes(authoritativeGitBlobBytes, workingTreeBytes) {
  if (!Buffer.isBuffer(authoritativeGitBlobBytes)) {
    throw new Error("unresolved_source_planning_path");
  }
  void workingTreeBytes;
  return authoritativeGitBlobBytes;
}

function resolveJsonPointer(value, pointer) {
  if (pointer === "") return value;
  if (!pointer.startsWith("/")) throw new Error("planning_snapshot_mismatch");
  return pointer.slice(1).split("/").reduce((current, token) => {
    const key = token.replace(/~1/g, "/").replace(/~0/g, "~");
    if (current === null || typeof current !== "object" || !Object.hasOwn(current, key)) {
      throw new Error("planning_snapshot_mismatch");
    }
    return current[key];
  }, value);
}

function validateSnapshotSources(sourcePlanningFiles, approvedFacts, exactSourceBytes) {
  const sourceByPath = new Map();
  for (const source of sourcePlanningFiles) {
    const bytes = exactSourceBytes.get(source.path);
    if (!bytes || sha256(bytes) !== source.sha256) {
      throw new Error("planning_snapshot_mismatch");
    }
    sourceByPath.set(source.path, JSON.parse(new TextDecoder("utf-8", {fatal: true}).decode(bytes)));
  }
  for (const fact of approvedFacts) {
    const source = sourceByPath.get(fact.sourcePath);
    if (source === undefined ||
        canonicalize(resolveJsonPointer(source, fact.sourcePointer)) !== canonicalize(fact.value)) {
      throw new Error("planning_snapshot_mismatch");
    }
  }
}

function validateOpenInkPlanningProjection(projection, requestedFidelity) {
  const expectedKeys = [
    "backgroundExclusions",
    "contourOmissionTargetPercent",
    "fullBodyHeadCount",
    "generationBackground",
    "negativeSpaceMinimumPercent",
    "paletteRoleAnchors",
    "schemaVersion",
    "styleReferenceFidelity",
  ];
  assert.deepEqual(Object.keys(projection).sort(), expectedKeys.sort());
  assert.deepEqual(Object.keys(projection.negativeSpaceMinimumPercent).sort(),
    ["figureInterior", "fullCanvas"]);
  assert.deepEqual(Object.keys(projection.paletteRoleAnchors).sort(),
    ["primaryCool", "secondaryEarth", "smallWarmAccent"]);
  assert.deepEqual(Object.keys(projection.generationBackground).sort(), ["color", "mode"]);
  assert.deepEqual(Object.keys(projection.backgroundExclusions).sort(),
    ["halo", "scene", "shadow", "vignette"]);
  const fidelityKeys = ["auditOnlySha256", "mode", "providerReferenceAuthorized"];
  if (projection.schemaVersion === "character_open_ink_wash_planning_projection_v2") {
    fidelityKeys.push("binding");
  }
  assert.deepEqual(Object.keys(projection.styleReferenceFidelity).sort(), fidelityKeys.sort());
  if (!["character_open_ink_wash_planning_projection_v1",
        "character_open_ink_wash_planning_projection_v2"].includes(projection.schemaVersion) ||
      projection.fullBodyHeadCount !== 4.25 ||
      projection.contourOmissionTargetPercent !== 45 ||
      projection.negativeSpaceMinimumPercent.figureInterior < 70 ||
      projection.negativeSpaceMinimumPercent.fullCanvas < 70 ||
      projection.generationBackground.mode !== "removable_solid" ||
      !Object.values(projection.backgroundExclusions).every((value) => value === true)) {
    throw new Error("open_ink_wash_profile_projection_mismatch");
  }
  for (const anchors of Object.values(projection.paletteRoleAnchors)) {
    if (!Array.isArray(anchors) || anchors.length === 0 ||
        new Set(anchors).size !== anchors.length) {
      throw new Error("open_ink_wash_profile_projection_mismatch");
    }
  }
  const fidelity = projection.styleReferenceFidelity;
  if (projection.schemaVersion === "character_open_ink_wash_planning_projection_v1" &&
      (fidelity.mode !== "semantic_text_projection_only" ||
      fidelity.auditOnlySha256 !==
        "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf" ||
      fidelity.providerReferenceAuthorized !== false)) {
    throw new Error("open_ink_wash_profile_projection_mismatch");
  }
  if (projection.schemaVersion === "character_open_ink_wash_planning_projection_v2") {
    const expectedBindingKeys = ["projectRelativePath", "reviewRecordId", "reviewRecordPath",
      "reviewRecordSha256", "role", "sha256"];
    if (fidelity.mode !== "durable_style_only_binding" ||
        fidelity.providerReferenceAuthorized !== true ||
        canonicalize(Object.keys(fidelity.binding).sort()) !==
          canonicalize(expectedBindingKeys.sort()) ||
        fidelity.binding.role !== "style_only") {
      throw new Error("open_ink_wash_profile_projection_mismatch");
    }
  }
  if (projection.schemaVersion === "character_open_ink_wash_planning_projection_v1" &&
      requestedFidelity === "selected_raster_match") {
    throw new Error("character_style_profile_conflict");
  }
}

function deriveCapture({ assetType, contentId, canonicalPath, canonicalPlanning,
  decisionDocuments, sourcePlanningFiles, snapshotHash, readablePaths }) {
  const decisionPrefix = "AgentDocs/planning-data/character/design-decisions/";
  const decisionPaths = canonicalPlanning.provenance.sourcePlanningRefs.filter((path) =>
    path.startsWith(decisionPrefix) && path.endsWith(".json"));
  const sourcePlanningPaths = [canonicalPath, ...new Set(decisionPaths)];
  if (sourcePlanningPaths.length < 2) throw new Error("missing_source_planning_path");
  if (sourcePlanningPaths.some((path) => !readablePaths.has(path))) {
    throw new Error("unresolved_source_planning_path");
  }
  if (sourcePlanningPaths.length !== sourcePlanningFiles.length ||
      sourcePlanningPaths.some((path, index) => path !== sourcePlanningFiles[index].path)) {
    throw new Error("planning_snapshot_mismatch");
  }
  const currentDecision = decisionDocuments.get(sourcePlanningPaths.at(-1));
  const capturedAt = currentDecision?.approval?.approvedAt;
  if (capturedAt === undefined) throw new Error("missing_capture_authority_timestamp");
  const explicitOffset = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?[+-](?:[01]\d|2[0-3]):[0-5]\d$/;
  if (!explicitOffset.test(capturedAt) || Number.isNaN(Date.parse(capturedAt))) {
    throw new Error("invalid_capture_authority_timestamp");
  }
  return {
    contentId,
    requestId: `gmplan2.${assetType}.${contentId}.${snapshotHash.slice(0, 20)}`,
    capturedAt,
    sourcePlanningPaths,
  };
}

function classifyCaptureIdentity(requestId, recordPath, authoritativeBaselinePaths) {
  if (requestId.startsWith("gmplan2.")) return "derived_current";
  if (authoritativeBaselinePaths.has(recordPath)) return "legacy_read_only";
  throw new Error("planning_snapshot_mismatch");
}

const payload = {
  schemaVersion: "generated_media_planning_snapshot_hash_payload_v2",
  sourcePlanningFiles: [{
    path: "AgentDocs/planning-data/character/act-plans/player/character.example.1.json",
    role: "canonical_character_planning",
    sha256: "a".repeat(64),
  }],
  approvedFacts: [{
    factId: "example.identity",
    sourcePath: "AgentDocs/planning-data/character/act-plans/player/character.example.1.json",
    sourcePointer: "/identity/characterId",
    value: "character.example.1",
  }],
};

const expectedCanonical = '{"approvedFacts":[{"factId":"example.identity","sourcePath":"AgentDocs/planning-data/character/act-plans/player/character.example.1.json","sourcePointer":"/identity/characterId","value":"character.example.1"}],"schemaVersion":"generated_media_planning_snapshot_hash_payload_v2","sourcePlanningFiles":[{"path":"AgentDocs/planning-data/character/act-plans/player/character.example.1.json","role":"canonical_character_planning","sha256":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"}]}';
const expectedHash = "0a528c76ba8f6575b0ed3938b0d48bf5b851eabb8e3ecbaca07b33faba33ff59";
const canonical = canonicalize(payload);

assert.equal(canonical, expectedCanonical);
assert.equal(crypto.createHash("sha256").update(Buffer.from(canonical, "utf8")).digest("hex"), expectedHash);
assert.equal(Object.hasOwn(payload, "capturedAt"), false);

const secondSource = {
  path: "AgentDocs/planning-data/character/design-decisions/v1/character.example.1.visual-design.json",
  role: "character_visual_design_decision_current",
  sha256: "b".repeat(64),
};
const orderedSources = [...payload.sourcePlanningFiles, secondSource];
const canonicalPlanning = {
  provenance: {
    sourcePlanningRefs: [
      "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md",
      secondSource.path,
      secondSource.path,
    ],
  },
};
const decisionDocuments = new Map([[secondSource.path, {
  approval: { approvedAt: "2026-08-14T09:30:00+09:00" },
}]]);
const readablePaths = new Set(orderedSources.map(({ path }) => path));

const orderedPayload = {...payload, sourcePlanningFiles: orderedSources};
const orderedPayloadCanonical = canonicalize(orderedPayload);
const orderedPayloadHash = crypto.createHash("sha256")
  .update(Buffer.from(orderedPayloadCanonical, "utf8"))
  .digest("hex");
const captureArgs = {
  assetType: "character_single_image",
  contentId: "character.example.1",
  canonicalPath: payload.sourcePlanningFiles[0].path,
  canonicalPlanning,
  decisionDocuments,
  sourcePlanningFiles: orderedSources,
  snapshotHash: orderedPayloadHash,
  readablePaths,
};
const capture = deriveCapture(captureArgs);
assert.equal(capture.requestId,
  `gmplan2.character_single_image.character.example.1.${orderedPayloadHash.slice(0, 20)}`);
assert.deepEqual(capture.sourcePlanningPaths, orderedSources.map(({ path }) => path));
assert.equal(classifyCaptureIdentity(capture.requestId, "new.json", new Set()),
  "derived_current");
assert.equal(classifyCaptureIdentity("legacy.request.1", "published.json",
  new Set(["published.json"])), "legacy_read_only");
assert.throws(
  () => classifyCaptureIdentity("legacy.request.2", "new-legacy.json", new Set()),
  /planning_snapshot_mismatch/,
);
const reversedSources = [...orderedSources].reverse();
const reversedPayload = {...payload, sourcePlanningFiles: reversedSources};
assert.notEqual(orderedPayloadCanonical, canonicalize(reversedPayload));
assert.notEqual(
  orderedPayloadHash,
  crypto.createHash("sha256").update(Buffer.from(canonicalize(reversedPayload), "utf8")).digest("hex"),
);
assert.throws(
  () => deriveCapture({...captureArgs, sourcePlanningFiles: reversedSources}),
  /planning_snapshot_mismatch/,
);

function buildHandoff(args) {
  const captureInput = deriveCapture(args);
  return {
    requestId: captureInput.requestId,
    contentId: captureInput.contentId,
    sourcePlanningFiles: args.sourcePlanningFiles,
    planningSnapshot: {
      capturedAt: captureInput.capturedAt,
      snapshotHash: orderedPayloadHash,
      approvedFacts: payload.approvedFacts,
    },
  };
}

const retryHandoff = buildHandoff(captureArgs);
const repeatedRetryHandoff = buildHandoff(structuredClone(captureArgs));
assert.deepEqual(
  Buffer.from(canonicalize(retryHandoff), "utf8"),
  Buffer.from(canonicalize(repeatedRetryHandoff), "utf8"),
);
assert.equal(retryHandoff.planningSnapshot.capturedAt,
  decisionDocuments.get(secondSource.path).approval.approvedAt);
assert.equal(retryHandoff.planningSnapshot.snapshotHash, crypto.createHash("sha256")
  .update(Buffer.from(orderedPayloadCanonical, "utf8"))
  .digest("hex"));

const canonicalSourcePath = orderedSources[0].path;
const decisionSourcePath = orderedSources[1].path;
const canonicalSourceBytes = Buffer.from(
  '{"identity":{"characterId":"character.example.1"}}\n', "utf8",
);
const decisionSourceBytes = Buffer.from(
  '{"designFacts":{"a/b":{"~key":"escaped"},"costume":{"value":{"color":"blue"}}}}\n', "utf8",
);
const exactSources = [
  {...orderedSources[0], sha256: sha256(canonicalSourceBytes)},
  {...orderedSources[1], sha256: sha256(decisionSourceBytes)},
];
const exactFacts = [
  payload.approvedFacts[0],
  {
    factId: "example.costume",
    sourcePath: decisionSourcePath,
    sourcePointer: "/designFacts/costume/value",
    value: {color: "blue"},
  },
  {
    factId: "example.escaped_pointer",
    sourcePath: decisionSourcePath,
    sourcePointer: "/designFacts/a~1b/~0key",
    value: "escaped",
  },
];
const exactSourceBytes = new Map([
  [canonicalSourcePath, canonicalSourceBytes],
  [decisionSourcePath, decisionSourceBytes],
]);

const crlfWorkingTreeBytes = Buffer.from(
  canonicalSourceBytes.toString("utf8").replace(/\n$/, "\r\n"), "utf8",
);
const selectedPublishedBytes = selectPublishedSourceBytes(
  canonicalSourceBytes, crlfWorkingTreeBytes,
);
assert.ok(selectedPublishedBytes.equals(canonicalSourceBytes));
assert.notEqual(sha256(selectedPublishedBytes), sha256(crlfWorkingTreeBytes));
const publishedSourceVector = [{...exactSources[0], sha256: sha256(selectedPublishedBytes)}];
validateSnapshotSources(publishedSourceVector, [payload.approvedFacts[0]],
  new Map([[canonicalSourcePath, selectedPublishedBytes]]));
const publishedSnapshotPayload = {
  schemaVersion: "generated_media_planning_snapshot_hash_payload_v2",
  sourcePlanningFiles: publishedSourceVector,
  approvedFacts: [payload.approvedFacts[0]],
};
const checkoutSnapshotPayload = {
  ...publishedSnapshotPayload,
  sourcePlanningFiles: [{...publishedSourceVector[0], sha256: sha256(crlfWorkingTreeBytes)}],
};
assert.notEqual(
  sha256(Buffer.from(canonicalize(publishedSnapshotPayload), "utf8")),
  sha256(Buffer.from(canonicalize(checkoutSnapshotPayload), "utf8")),
  "published snapshot identity must use authoritative Git-blob bytes, not CRLF checkout bytes",
);
assert.throws(() => selectPublishedSourceBytes(undefined, crlfWorkingTreeBytes),
  /unresolved_source_planning_path/);

validateSnapshotSources(exactSources, exactFacts, exactSourceBytes);

const openInkProfileKey = "projectbs_character_open_ink_wash_dynamic_contour@1.0.0";
const openInkProfileHash = "37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd";
const openInkDecisionPath =
  "AgentDocs/planning-data/character/design-decisions/v1/character.example.1.open-ink-wash.json";
const openInkDecisionBytes = Buffer.from(canonicalize({
  expressionProfileKey: openInkProfileKey,
  expressionProfilePayloadHash: openInkProfileHash,
}) + "\n", "utf8");
const openInkSources = [{
  path: openInkDecisionPath,
  role: "character_expression_profile_selection",
  sha256: sha256(openInkDecisionBytes),
}];
const openInkFacts = [{
  factId: "example.expression_profile_key",
  sourcePath: openInkDecisionPath,
  sourcePointer: "/expressionProfileKey",
  value: openInkProfileKey,
}, {
  factId: "example.expression_profile_hash",
  sourcePath: openInkDecisionPath,
  sourcePointer: "/expressionProfilePayloadHash",
  value: openInkProfileHash,
}];
validateSnapshotSources(openInkSources, openInkFacts,
  new Map([[openInkDecisionPath, openInkDecisionBytes]]));
assert.throws(() => validateSnapshotSources(openInkSources,
  [{...openInkFacts[0], value: "projectbs_character_sparse_ink_pastel_motion@1.0.0"},
    openInkFacts[1]], new Map([[openInkDecisionPath, openInkDecisionBytes]])),
/planning_snapshot_mismatch/);

const openInkPlanningProjection = {
  schemaVersion: "character_open_ink_wash_planning_projection_v1",
  fullBodyHeadCount: 4.25,
  contourOmissionTargetPercent: 45,
  negativeSpaceMinimumPercent: {figureInterior: 70, fullCanvas: 70},
  paletteRoleAnchors: {
    primaryCool: ["robe_collar", "waist_sash", "hwando_hilt"],
    secondaryEarth: ["travel_overcoat", "one_shoulder_armor", "travel_accessory"],
    smallWarmAccent: ["utility_pouch", "selected_repair_point"],
  },
  generationBackground: {mode: "removable_solid", color: "#F2EFE6"},
  backgroundExclusions: {halo: true, vignette: true, scene: true, shadow: true},
  styleReferenceFidelity: {
    mode: "semantic_text_projection_only",
    auditOnlySha256: "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf",
    providerReferenceAuthorized: false,
  },
};
validateOpenInkPlanningProjection(openInkPlanningProjection, "semantic_direction_only");
assert.throws(
  () => validateOpenInkPlanningProjection(openInkPlanningProjection, "selected_raster_match"),
  /character_style_profile_conflict/,
);
assert.throws(
  () => validateOpenInkPlanningProjection({
    ...openInkPlanningProjection,
    negativeSpaceMinimumPercent: {figureInterior: 70, fullCanvas: 69},
  }, "semantic_direction_only"),
  /open_ink_wash_profile_projection_mismatch/,
);

const durableStyleBinding = {
  role: "style_only",
  projectRelativePath: "AgentDocs/reference-assets/generated-media/style-only/character_single_image/open_ink_wash_dynamic_contour/b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf.png",
  sha256: "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf",
  reviewRecordId: "gmstyleref1.character_single_image.open_ink_wash_dynamic_contour.d6dae45a8f8f6591b5cb",
  reviewRecordPath: "AgentDocs/planning-data/style-reference-reviews/v1/character_single_image/open_ink_wash_dynamic_contour/gmstyleref1.character_single_image.open_ink_wash_dynamic_contour.d6dae45a8f8f6591b5cb.json",
  reviewRecordSha256: "51630e6c2c4ec80caae9bf5c995f7673e2b8fddf83870c5a28452971fa2be4c2",
};
const openInkPlanningProjectionV2 = {
  ...structuredClone(openInkPlanningProjection),
  schemaVersion: "character_open_ink_wash_planning_projection_v2",
  styleReferenceFidelity: {
    mode: "durable_style_only_binding",
    auditOnlySha256: durableStyleBinding.sha256,
    providerReferenceAuthorized: true,
    binding: durableStyleBinding,
  },
};
validateOpenInkPlanningProjection(openInkPlanningProjectionV2, "selected_raster_match");
assert.throws(() => validateOpenInkPlanningProjection({
  ...openInkPlanningProjectionV2,
  styleReferenceFidelity: { ...openInkPlanningProjectionV2.styleReferenceFidelity,
    binding: { role: "style_only", projectRelativePath: durableStyleBinding.projectRelativePath,
      sha256: durableStyleBinding.sha256 } },
}, "selected_raster_match"), /open_ink_wash_profile_projection_mismatch/);

const durableProjectionDecisionPath =
  "AgentDocs/planning-data/character/design-decisions/v1/character.example.1.open-ink-durable-projection.example.json";
const durableProjectionBytes = Buffer.from(canonicalize({
  openInkWashPlanningProjection: openInkPlanningProjectionV2,
  styleReferenceBindings: [durableStyleBinding],
}) + "\n", "utf8");
const durableProjectionSource = { path: durableProjectionDecisionPath,
  role: "character_visual_design_decision_current", sha256: sha256(durableProjectionBytes) };
const durableBindingFacts = Object.entries(durableStyleBinding).map(([key, value]) => ({
  factId: `example.open_ink.style_binding.${key}`,
  sourcePath: durableProjectionDecisionPath,
  sourcePointer: `/styleReferenceBindings/0/${key}`,
  value,
}));
validateSnapshotSources([durableProjectionSource], durableBindingFacts,
  new Map([[durableProjectionDecisionPath, durableProjectionBytes]]));
assert.throws(() => validateSnapshotSources([durableProjectionSource], durableBindingFacts.map((fact) =>
  fact.factId.endsWith("reviewRecordSha256") ? { ...fact, value: "0".repeat(64) } : fact),
new Map([[durableProjectionDecisionPath, durableProjectionBytes]])), /planning_snapshot_mismatch/);

const projectionDecisionPath =
  "AgentDocs/planning-data/character/design-decisions/v1/character.example.1.open-ink-projection.example.json";
const projectionDecisionBytes = Buffer.from(canonicalize({
  openInkWashPlanningProjection: openInkPlanningProjection,
}) + "\n", "utf8");
const projectionSource = {
  path: projectionDecisionPath,
  role: "character_visual_design_decision_current",
  sha256: sha256(projectionDecisionBytes),
};
const projectionLeafFacts = [
  ["example.open_ink.projection_schema", "/openInkWashPlanningProjection/schemaVersion", "character_open_ink_wash_planning_projection_v1"],
  ["example.open_ink.full_body_heads", "/openInkWashPlanningProjection/fullBodyHeadCount", 4.25],
  ["example.open_ink.contour_target", "/openInkWashPlanningProjection/contourOmissionTargetPercent", 45],
  ["example.open_ink.figure_negative_space", "/openInkWashPlanningProjection/negativeSpaceMinimumPercent/figureInterior", 70],
  ["example.open_ink.canvas_negative_space", "/openInkWashPlanningProjection/negativeSpaceMinimumPercent/fullCanvas", 70],
  ["example.open_ink.primary_anchors", "/openInkWashPlanningProjection/paletteRoleAnchors/primaryCool", ["robe_collar", "waist_sash", "hwando_hilt"]],
  ["example.open_ink.secondary_anchors", "/openInkWashPlanningProjection/paletteRoleAnchors/secondaryEarth", ["travel_overcoat", "one_shoulder_armor", "travel_accessory"]],
  ["example.open_ink.accent_anchors", "/openInkWashPlanningProjection/paletteRoleAnchors/smallWarmAccent", ["utility_pouch", "selected_repair_point"]],
  ["example.open_ink.background_mode", "/openInkWashPlanningProjection/generationBackground/mode", "removable_solid"],
  ["example.open_ink.background_color", "/openInkWashPlanningProjection/generationBackground/color", "#F2EFE6"],
  ["example.open_ink.no_halo", "/openInkWashPlanningProjection/backgroundExclusions/halo", true],
  ["example.open_ink.no_vignette", "/openInkWashPlanningProjection/backgroundExclusions/vignette", true],
  ["example.open_ink.no_scene", "/openInkWashPlanningProjection/backgroundExclusions/scene", true],
  ["example.open_ink.no_shadow", "/openInkWashPlanningProjection/backgroundExclusions/shadow", true],
  ["example.open_ink.reference_mode", "/openInkWashPlanningProjection/styleReferenceFidelity/mode", "semantic_text_projection_only"],
  ["example.open_ink.reference_sha", "/openInkWashPlanningProjection/styleReferenceFidelity/auditOnlySha256", "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf"],
  ["example.open_ink.reference_authorized", "/openInkWashPlanningProjection/styleReferenceFidelity/providerReferenceAuthorized", false],
].map(([factId, sourcePointer, value]) => ({
  factId,
  sourcePath: projectionDecisionPath,
  sourcePointer,
  value,
}));
validateSnapshotSources([projectionSource], projectionLeafFacts,
  new Map([[projectionDecisionPath, projectionDecisionBytes]]));
assert.throws(() => validateSnapshotSources([projectionSource], projectionLeafFacts.map((fact) =>
  fact.factId === "example.open_ink.no_halo" ? {...fact, value: false} : fact),
new Map([[projectionDecisionPath, projectionDecisionBytes]])), /planning_snapshot_mismatch/);
const openInkV2ProfileKey = "projectbs_character_open_ink_wash_dynamic_contour@2.0.0";
const openInkV2ProfileHash = "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5";
const openInkV2DecisionPath =
  "AgentDocs/planning-data/character/design-decisions/v1/character.example.1.open-ink-wash-v2.json";
const openInkV2DecisionBytes = Buffer.from(canonicalize({
  expressionProfileKey: openInkV2ProfileKey,
  expressionProfilePayloadHash: openInkV2ProfileHash,
}) + "\n", "utf8");
const openInkV2Sources = [{ path: openInkV2DecisionPath,
  role: "character_expression_profile_selection", sha256: sha256(openInkV2DecisionBytes) }];
const openInkV2Facts = [{ factId: "example.expression_profile_key",
  sourcePath: openInkV2DecisionPath, sourcePointer: "/expressionProfileKey",
  value: openInkV2ProfileKey }, { factId: "example.expression_profile_hash",
  sourcePath: openInkV2DecisionPath, sourcePointer: "/expressionProfilePayloadHash",
  value: openInkV2ProfileHash }];
validateSnapshotSources(openInkV2Sources, openInkV2Facts,
  new Map([[openInkV2DecisionPath, openInkV2DecisionBytes]]));
assert.throws(() => validateSnapshotSources(openInkV2Sources,
  [{ ...openInkV2Facts[0], value: openInkProfileKey }, openInkV2Facts[1]],
  new Map([[openInkV2DecisionPath, openInkV2DecisionBytes]])), /planning_snapshot_mismatch/,
"v1 cannot silently replace an approved v2 planning pointer");
assert.throws(
  () => validateSnapshotSources(exactSources, exactFacts, new Map([
    [canonicalSourcePath, Buffer.from(canonicalSourceBytes.toString("utf8").replace(/\n$/, "\r\n"), "utf8")],
    [decisionSourcePath, decisionSourceBytes],
  ])),
  /planning_snapshot_mismatch/,
);
assert.throws(
  () => validateSnapshotSources(exactSources, [
    {...exactFacts[0], value: "character.other.1"},
    exactFacts[1],
  ], exactSourceBytes),
  /planning_snapshot_mismatch/,
);
assert.throws(
  () => validateSnapshotSources(exactSources, [
    {...exactFacts[0], sourcePointer: "/identity/missing"},
    exactFacts[1],
  ], exactSourceBytes),
  /planning_snapshot_mismatch/,
);

const canonicalHandoffBytes = Buffer.from(`${canonicalize(retryHandoff)}\n`, "utf8");
assert.equal(canonicalHandoffBytes.at(-1), 0x0a);
assert.equal(canonicalHandoffBytes.at(-2) === 0x0d, false);
assert.notEqual(
  sha256(canonicalHandoffBytes),
  sha256(Buffer.from(`${canonicalize(retryHandoff)}\r\n`, "utf8")),
);

assert.throws(
  () => deriveCapture({...captureArgs, decisionDocuments: new Map([[secondSource.path, {}]])}),
  /missing_capture_authority_timestamp/,
);

assert.throws(
  () => deriveCapture({...captureArgs, decisionDocuments: new Map([[secondSource.path, {
    approval: {approvedAt: "2026-08-14T00:30:00Z"},
  }]])}),
  /invalid_capture_authority_timestamp/,
);
assert.deepEqual(deriveCapture(captureArgs).sourcePlanningPaths,
  [payload.sourcePlanningFiles[0].path, secondSource.path],
  "duplicate provenance refs are deterministically collapsed at first occurrence");
assert.throws(
  () => deriveCapture({...captureArgs, canonicalPlanning: {
    provenance: {sourcePlanningRefs: []},
  }, sourcePlanningFiles: [orderedSources[0]]}),
  /missing_source_planning_path/,
);
assert.throws(
  () => deriveCapture({...captureArgs, canonicalPlanning: {provenance: {
    sourcePlanningRefs: ["AgentDocs/planning-data/character/design-decisions/v1/missing.json"],
  }}}),
  /unresolved_source_planning_path/,
);
assert.equal(Object.hasOwn(capture, "callerApproval"), false);
console.log("generated media planning snapshot v2 contract vector: PASS");
