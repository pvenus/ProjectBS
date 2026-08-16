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
