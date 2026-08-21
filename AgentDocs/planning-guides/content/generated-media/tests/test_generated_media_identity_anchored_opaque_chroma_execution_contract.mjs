import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, "..", "..", "..", "..", "..");
const generatedMedia = join(root, "AgentDocs", "planning-guides", "content", "generated-media");
const profilePath = join(generatedMedia, "helpers",
  "generated_media_identity_anchored_opaque_chroma_execution_profile_v1.json");

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function assertClosedKeys(value, keys) {
  assert.deepEqual(Object.keys(value).sort(), [...keys].sort());
}

const rawProfile = readFileSync(profilePath);
assert.equal(rawProfile[0], 0x7b);
assert.equal(rawProfile.at(-1), 0x0a);
assert.equal(rawProfile.includes(Buffer.from("\r")), false);
const profile = JSON.parse(rawProfile.toString("utf8"));
const profileHash = sha256(Buffer.from(canonicalJson(profile), "utf8"));
assert.equal(profileHash,
  "44d3bafcc720d39ac260fb2089798c16f9ec1f50d391165eea676dbc79cdc3ad");
assert.equal(profile.executionProfileKey,
  "projectbs_character_open_ink_opaque_chroma_identity_anchored_regeneration@1.0.0");
assert.equal(profile.applicability.expressionProfilePayloadHash,
  "b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a");
assert.equal(profile.rejectedLineage.disposition,
  "non_authoritative_non_reusable_no_reference_no_edit_source");

const selection = {
  schemaVersion: "generated_media_identity_anchored_generation_selection_v1",
  executionProfileKey: profile.executionProfileKey,
  executionProfilePayloadSha256: profileHash,
  role: profile.identityReferenceBinding.role,
  authorityContentId: profile.identityReferenceBinding.authorityContentId,
  projectRelativePath: profile.identityReferenceBinding.projectRelativePath,
  sha256: profile.identityReferenceBinding.sha256,
};

const selectionKeys = ["schemaVersion", "executionProfileKey",
  "executionProfilePayloadSha256", "role", "authorityContentId",
  "projectRelativePath", "sha256"];

function validateSelection(value) {
  assertClosedKeys(value, selectionKeys);
  if (canonicalJson(value) !== canonicalJson(selection)) {
    throw new Error("identity_anchored_generation_projection_mismatch");
  }
  if (/^[A-Za-z]:[\\/]/.test(value.projectRelativePath) ||
      value.projectRelativePath.includes("..")) {
    throw new Error("identity_anchored_generation_projection_mismatch");
  }
  if (value.role !== "identity_equipment_authority") {
    throw new Error("identity_anchored_reference_role_invalid");
  }
  return true;
}

assert.equal(validateSelection(selection), true);
assert.equal(selection.projectRelativePath,
  "Assets/ImagesGenerated/Character/portrait/character.seojin.1.portrait.png");
assert.equal(selection.sha256,
  "ba2f769ba7d45909d618f7fd672a9bdad61015b9553d3c0d360bc49a13bb97cf");

function projectPipeline(authoritySelection) {
  validateSelection(authoritySelection);
  const planning = { identityAnchoredGenerationSelection: structuredClone(authoritySelection) };
  const routing = {
    identityAnchoredGenerationSelection: structuredClone(authoritySelection),
    normalizedRequest: { identityAnchoredGenerationSelection: structuredClone(authoritySelection) },
    authoringHandoff: { identityAnchoredGenerationSelection: structuredClone(authoritySelection) },
  };
  const visualBrief = { identityAnchoredGenerationSelection: structuredClone(authoritySelection) };
  const promptRecord = { identityAnchoredGenerationSelection: structuredClone(authoritySelection) };
  const promptIndexEntry = { identityAnchoredGenerationSelection: structuredClone(authoritySelection) };
  const generationHandoff = { identityAnchoredGenerationSelection: structuredClone(authoritySelection) };
  for (const projection of [planning.identityAnchoredGenerationSelection,
    routing.identityAnchoredGenerationSelection,
    routing.normalizedRequest.identityAnchoredGenerationSelection,
    routing.authoringHandoff.identityAnchoredGenerationSelection,
    visualBrief.identityAnchoredGenerationSelection,
    promptRecord.identityAnchoredGenerationSelection,
    promptIndexEntry.identityAnchoredGenerationSelection,
    generationHandoff.identityAnchoredGenerationSelection]) {
    if (canonicalJson(projection) !== canonicalJson(authoritySelection)) {
      throw new Error("identity_anchored_generation_projection_mismatch");
    }
  }
  return { planning, routing, visualBrief, promptRecord, promptIndexEntry, generationHandoff };
}

const pipeline = projectPipeline(selection);
const selectionHash = sha256(Buffer.from(canonicalJson(selection), "utf8"));
const promptSha256 = "1".repeat(64);
const callProjection = {
  promptSha256,
  referenceMode: "identity_equipment_authority",
  referencedImagePaths: [selection.projectRelativePath],
};
const callProjectionSha256 = sha256(Buffer.from(canonicalJson(callProjection), "utf8"));
const scope = {
  schemaVersion: "generated_media_builtin_imagegen_identity_anchored_execution_scope_v1",
  executionMode: "builtin_imagegen_authenticated_identity_anchored_single_submit_v1",
  authorityMainSha: "2".repeat(40),
  requestId: "gmplan2.character_single_image.character.seojin.2.contract",
  promptRecordId: "gmprompt3.character_single_image.character.seojin.2.contract",
  promptRecordSha256: "3".repeat(64),
  promptMarkdownSha256: "4".repeat(64),
  generationHandoffSha256: "5".repeat(64),
  providerPromptPayloadHash: "6".repeat(64),
  expressionProfileKey: profile.applicability.expressionProfileKey,
  expressionProfilePayloadHash: profile.applicability.expressionProfilePayloadHash,
  callableSchemaSha256: "708b75b05f820870ac165eadcf08d093568944a35d2793e0a7d117bf23646af1",
  callProjectionSha256,
  providerSettingsIntentSha256: "7".repeat(64),
  identityAnchoredGenerationSelection: structuredClone(selection),
  identityAnchoredGenerationSelectionSha256: selectionHash,
  executionProfileKey: profile.executionProfileKey,
  executionProfilePayloadSha256: profileHash,
  submitCountMaximum: 1,
  retryCountMaximum: 0,
};
const executionScopeHash = sha256(Buffer.from(canonicalJson(scope), "utf8"));
assert.equal(`gmidentity1.${executionScopeHash.slice(0, 20)}`.length, 32);

function validateCall(call, observedReferenceSha256, state = "fresh") {
  assertClosedKeys(call, ["prompt", "referenced_image_paths"]);
  if (typeof call.prompt !== "string" || call.prompt.length === 0) {
    throw new Error("identity_anchored_authoring_gate_mismatch");
  }
  if (!Array.isArray(call.referenced_image_paths) || call.referenced_image_paths.length !== 1) {
    throw new Error("identity_anchored_reference_count_invalid");
  }
  if (call.referenced_image_paths[0] !== selection.projectRelativePath) {
    throw new Error("identity_anchored_generation_projection_mismatch");
  }
  if (observedReferenceSha256 !== selection.sha256) {
    throw new Error("identity_anchored_reference_hash_mismatch");
  }
  if (["active", "completed", "ambiguous"].includes(state)) {
    throw new Error("duplicate_provider_call_risk");
  }
  return true;
}

assert.equal(validateCall({ prompt: "closed Grade 2 prompt",
  referenced_image_paths: [selection.projectRelativePath] }, selection.sha256), true);
assert.throws(() => validateCall({ prompt: "x", referenced_image_paths: [] }, selection.sha256),
  /identity_anchored_reference_count_invalid/);
assert.throws(() => validateCall({ prompt: "x", referenced_image_paths: [selection.projectRelativePath,
  selection.projectRelativePath] }, selection.sha256), /identity_anchored_reference_count_invalid/);
assert.throws(() => validateCall({ prompt: "x", referenced_image_paths: [selection.projectRelativePath],
  num_last_images_to_include: 1 }, selection.sha256), /actual.*expected|deep-equal|keys/i);
assert.throws(() => validateCall({ prompt: "x", referenced_image_paths: [selection.projectRelativePath] },
  "0".repeat(64)), /identity_anchored_reference_hash_mismatch/);
assert.throws(() => validateCall({ prompt: "x", referenced_image_paths: [selection.projectRelativePath] },
  selection.sha256, "completed"), /duplicate_provider_call_risk/);

for (const role of ["style_only", "edit_source", "edit_target"]) {
  assert.throws(() => validateSelection({ ...selection, role }),
    /identity_anchored_generation_projection_mismatch|identity_anchored_reference_role_invalid/);
}
assert.throws(() => validateSelection({ ...selection, sha256: "0".repeat(64) }),
  /identity_anchored_generation_projection_mismatch/);
assert.throws(() => validateSelection({ ...selection, providerReceiptSha256: "0".repeat(64) }),
  /deep-equal|keys/i);

const nested = structuredClone(pipeline.routing);
nested.typeSpecification = { identityAnchoredGenerationSelection: selection };
assert.equal(Object.hasOwn(nested.typeSpecification, "identityAnchoredGenerationSelection"), true);
assert.throws(() => {
  if (Object.hasOwn(nested.typeSpecification, "identityAnchoredGenerationSelection")) {
    throw new Error("identity_anchored_generation_projection_mismatch");
  }
}, /identity_anchored_generation_projection_mismatch/);

assert.equal(scope.submitCountMaximum, 1);
assert.equal(scope.retryCountMaximum, 0);
assert.equal(profile.providerMasterContractBinding.postprocessOwnerRole,
  "generated_media_chroma_uncomposite");
assert.equal(profile.stageBoundary.generationMayUncomposite, false);
assert.equal(profile.stageBoundary.projectCopyEligible, false);

for (const relative of [
  "GeneratedMediaIdentityAnchoredOpaqueChromaExecutionGuide.md",
  "GeneratedMediaImageGenOnlyContractGuide.md",
  "GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md",
  "GeneratedMediaRecordGuide.md",
  "GeneratedMediaRequestRoutingGuide.md",
  "GeneratedMediaAuthoringProfileRegistryGuide.md",
]) {
  const text = readFileSync(join(generatedMedia, relative), "utf8");
  assert.match(text, /identityAnchoredGenerationSelection|identity-anchored/i);
}
assert.match(readFileSync(join(generatedMedia,
  "GeneratedMediaIdentityAnchoredOpaqueChromaExecutionGuide.md"), "utf8"),
/44d3bafcc720d39ac260fb2089798c16f9ec1f50d391165eea676dbc79cdc3ad/);

console.log("Generated Media identity-anchored opaque-chroma execution contract tests passed.");
