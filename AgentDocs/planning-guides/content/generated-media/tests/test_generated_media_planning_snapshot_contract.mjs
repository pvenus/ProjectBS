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

function validateCapture(capture, expectedIdentity, sourcePlanningFiles, readablePaths) {
  if (!capture) throw new Error("missing_planning_capture_inputs");
  if (capture.contentId !== expectedIdentity.contentId ||
      capture.requestId !== expectedIdentity.requestId) {
    throw new Error("planning_capture_identity_mismatch");
  }
  const explicitOffset = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?[+-](?:[01]\d|2[0-3]):[0-5]\d$/;
  if (!explicitOffset.test(capture.capturedAt) || Number.isNaN(Date.parse(capture.capturedAt))) {
    throw new Error("invalid_planning_capture_timestamp");
  }
  if (!Array.isArray(capture.sourcePlanningPaths) || capture.sourcePlanningPaths.length === 0 ||
      capture.sourcePlanningPaths.some((path) => typeof path !== "string" || path.length === 0)) {
    throw new Error("missing_source_planning_path");
  }
  if (new Set(capture.sourcePlanningPaths).size !== capture.sourcePlanningPaths.length) {
    throw new Error("duplicate_source_planning_path");
  }
  if (capture.sourcePlanningPaths.some((path) => !readablePaths.has(path))) {
    throw new Error("unresolved_source_planning_path");
  }
  if (capture.sourcePlanningPaths.length !== sourcePlanningFiles.length ||
      capture.sourcePlanningPaths.some((path, index) => path !== sourcePlanningFiles[index].path)) {
    throw new Error("planning_snapshot_mismatch");
  }
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
  role: "character_visual_design_decision",
  sha256: "b".repeat(64),
};
const orderedSources = [...payload.sourcePlanningFiles, secondSource];
const capture = {
  contentId: "character.example.1",
  requestId: "gmreq.character.example.1",
  capturedAt: "2026-08-14T09:30:00+09:00",
  sourcePlanningPaths: orderedSources.map(({ path }) => path),
};
const readablePaths = new Set(capture.sourcePlanningPaths);

validateCapture(capture, {
  contentId: "character.example.1",
  requestId: "gmreq.character.example.1",
}, orderedSources, readablePaths);

const orderedPayload = {...payload, sourcePlanningFiles: orderedSources};
const orderedPayloadCanonical = canonicalize(orderedPayload);
const orderedPayloadHash = crypto.createHash("sha256")
  .update(Buffer.from(orderedPayloadCanonical, "utf8"))
  .digest("hex");
const reversedSources = [...orderedSources].reverse();
const reversedPayload = {...payload, sourcePlanningFiles: reversedSources};
assert.notEqual(orderedPayloadCanonical, canonicalize(reversedPayload));
assert.notEqual(
  orderedPayloadHash,
  crypto.createHash("sha256").update(Buffer.from(canonicalize(reversedPayload), "utf8")).digest("hex"),
);
assert.throws(
  () => validateCapture(capture, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, reversedSources, readablePaths),
  /planning_snapshot_mismatch/,
);

function buildHandoff(captureInput, sources) {
  validateCapture(captureInput, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, sources, readablePaths);
  return {
    requestId: captureInput.requestId,
    contentId: captureInput.contentId,
    sourcePlanningFiles: sources,
    planningSnapshot: {
      capturedAt: captureInput.capturedAt,
      snapshotHash: orderedPayloadHash,
      approvedFacts: payload.approvedFacts,
    },
  };
}

const retryHandoff = buildHandoff(capture, orderedSources);
const repeatedRetryHandoff = buildHandoff(structuredClone(capture), structuredClone(orderedSources));
assert.deepEqual(
  Buffer.from(canonicalize(retryHandoff), "utf8"),
  Buffer.from(canonicalize(repeatedRetryHandoff), "utf8"),
);
assert.equal(retryHandoff.planningSnapshot.capturedAt, capture.capturedAt);
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
  () => validateCapture(null, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, orderedSources, readablePaths),
  /missing_planning_capture_inputs/,
);

assert.throws(
  () => validateCapture({...capture, capturedAt: "2026-08-14T00:30:00Z"}, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, orderedSources, readablePaths),
  /invalid_planning_capture_timestamp/,
);
assert.throws(
  () => validateCapture({...capture, sourcePlanningPaths: [orderedSources[0].path, orderedSources[0].path]}, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, orderedSources, readablePaths),
  /duplicate_source_planning_path/,
);
assert.throws(
  () => validateCapture({...capture, contentId: "character.other.1"}, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, orderedSources, readablePaths),
  /planning_capture_identity_mismatch/,
);
assert.throws(
  () => validateCapture({...capture, sourcePlanningPaths: []}, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, orderedSources, readablePaths),
  /missing_source_planning_path/,
);
assert.throws(
  () => validateCapture({...capture, sourcePlanningPaths: [orderedSources[0].path, "AgentDocs/planning-data/missing.json"]}, {
    contentId: "character.example.1",
    requestId: "gmreq.character.example.1",
  }, orderedSources, readablePaths),
  /unresolved_source_planning_path/,
);
console.log("generated media planning snapshot v2 contract vector: PASS");
