import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

function fail(message) {
  process.stderr.write(`${message}\n`);
  process.exit(1);
}

function canonicalJson(value) {
  if (value === null || typeof value !== "object") {
    return JSON.stringify(value);
  }
  if (Array.isArray(value)) {
    return `[${value.map(canonicalJson).join(",")}]`;
  }
  return `{${Object.keys(value)
    .sort()
    .map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`)
    .join(",")}}`;
}

function sha256(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

const [projectRoot, planningRelativePathInput, requestId, capturedAt, outputRelativePathInput] =
  process.argv.slice(2);

if (!projectRoot || !planningRelativePathInput || !requestId || !capturedAt) {
  fail(
    "usage: node build-character-main-handoff.mjs <projectRoot> <planningRelativePath> <requestId> <capturedAtUtc> [outputRelativePath]",
  );
}

if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$/.test(capturedAt)) {
  fail("capturedAtUtc must use YYYY-MM-DDTHH:mm:ssZ");
}

const planningRelativePath = planningRelativePathInput.replaceAll("\\", "/");
const planningPath = path.resolve(projectRoot, ...planningRelativePath.split("/"));
const planningBytes = fs.readFileSync(planningPath);
const planning = JSON.parse(planningBytes.toString("utf8").replace(/^\uFEFF/, ""));

if (planning.schemaVersion !== "character_planning_v2") {
  fail("planning must use character_planning_v2");
}
if (planning.planningStatus !== "approved") {
  fail("planningStatus must be approved");
}
if (planning.identity?.runtimeDomain !== "character") {
  fail("identity.runtimeDomain must be character");
}
if (planning.generatedMediaPlanning?.characterMainImage?.readiness !== "ready") {
  fail("characterMainImage.readiness must be ready");
}
if (!Array.isArray(planning.missingDesignInputs) || planning.missingDesignInputs.length !== 0) {
  fail("missingDesignInputs must be an empty array");
}

const characterMainImage = planning.generatedMediaPlanning.characterMainImage;
const requiredElements = characterMainImage.requiredElements.map(
  (entry) => entry.statement,
);
const prohibitedElements = characterMainImage.prohibitedElements.map(
  (entry) => entry.statement,
);
if (requiredElements.length === 0 || prohibitedElements.length === 0) {
  fail("requiredElements and prohibitedElements must be non-empty");
}

const assetType = "character_main_image";
const domainType = "character";
const contentId = planning.identity.characterId;
const contentUsage = planning.appearance.intendedDisplay.outputUsage;
const sourcePlanningFiles = [
  {
    path: planningRelativePath,
    role: "design",
    sha256: sha256(planningBytes),
  },
];
const characterIdentity = {
  identity: planning.identity,
  identityConsistencyLocks: characterMainImage.identityConsistencyLocks,
};
const appearanceSpecification = planning.appearance;
const approvedFacts = {
  characterIdentity,
  appearanceSpecification,
  requiredElements,
  prohibitedElements,
  rotationPolicy: characterMainImage.rotationPolicy,
};
const snapshotPayload = {
  schemaVersion: "generated_media_planning_snapshot_payload_v1",
  requestId,
  assetType,
  domainType,
  contentId,
  contentUsage,
  sourcePlanningFiles,
  approvedFacts,
};
const snapshotHash = sha256(
  Buffer.from(canonicalJson(snapshotPayload), "utf8"),
);

const handoff = {
  schemaVersion: "generated_media_planning_handoff_v1",
  requestId,
  assetType,
  domainType,
  contentId,
  contentName: planning.identity.name,
  contentUsage,
  sourcePlanningFiles,
  planningSnapshot: {
    capturedAt,
    snapshotHash,
    approvedFacts,
  },
  requiredElements,
  prohibitedElements,
  characterIdentity,
  appearanceSpecification,
  rotationContract: {
    orderedDirections: [
      "north",
      "north_east",
      "east",
      "south_east",
      "south",
      "south_west",
      "west",
      "north_west",
    ],
    exactCount: 8,
    identityConsistencyRequired: true,
  },
};

const outputBytes = Buffer.from(`${JSON.stringify(handoff, null, 2)}\n`, "utf8");

if (!outputRelativePathInput) {
  process.stdout.write(outputBytes);
  process.exit(0);
}

const outputRelativePath = outputRelativePathInput.replaceAll("\\", "/");
const outputPath = path.resolve(projectRoot, ...outputRelativePath.split("/"));
if (fs.existsSync(outputPath)) {
  const existingBytes = fs.readFileSync(outputPath);
  if (!existingBytes.equals(outputBytes)) {
    fail(`character_planning_handoff_collision: ${outputRelativePath}`);
  }
  process.stdout.write(`reused_identical ${outputRelativePath}\n`);
  process.exit(0);
}

fs.mkdirSync(path.dirname(outputPath), { recursive: true });
fs.writeFileSync(outputPath, outputBytes, { flag: "wx" });
process.stdout.write(`created ${outputRelativePath}\n`);
