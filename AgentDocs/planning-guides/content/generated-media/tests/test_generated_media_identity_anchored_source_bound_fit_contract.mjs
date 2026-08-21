// Exact identity-anchored G2 source-bound fit/chroma successor vectors.
// The helper is executed in memory only; this test creates no media artifact.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { existsSync, readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { canonicalJson } from
  "../helpers/generated_media_canonical_serializers_v1.mjs";
import { PROFILE_KEY, PROFILE_PAYLOAD_SHA256,
  executeIdentityAnchoredSourceBoundFit, validateProfile } from
  "../helpers/generated_media_identity_anchored_source_bound_fit_v1.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const generatedMedia = join(here, ".."); const helpers = join(generatedMedia, "helpers");
const repo = join(generatedMedia, "..", "..", "..", "..");
const profile = JSON.parse(readFileSync(join(helpers,
  "generated_media_identity_anchored_source_bound_fit_profile_v1.json"), "utf8"));
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");

assert.equal(profile.profileKey, PROFILE_KEY);
assert.equal(sha(Buffer.from(canonicalJson(profile), "utf8")), PROFILE_PAYLOAD_SHA256);
assert.equal(PROFILE_PAYLOAD_SHA256,
  "1a669eed96cda8a2add59445cbf3c1e174fe359b1c03bf42ed707477d3cdc138");
assert.equal(validateProfile(profile), profile);
assert.deepEqual(profile.sourceBinding, {
  byteLength: 1703705,
  generationHandoffSha256:
    "2491e0e0bb7f890f861fd8526219f26e7c7282ea55fb2a39052cb8b16dade993",
  generationReceiptByteLength: 5081,
  generationReceiptSha256:
    "1fb9dfea17e5e1e107193f988c2ec0065cd9e4cbd8c4140241187f06df760977",
  generationScopeSha256:
    "042d1a21cf545281c894254f7be1202e22fd19b48c2732284d9de417355aed12",
  rejectedPriorAlphaSha256:
    "1b68b5a50c2801a090f25d13f4b22bd7b8afc4e5c6a93b423ddf06abeaa4bfbb",
  rejectedPriorDisposition: "forbidden_input_non_authoritative_non_reusable",
  sourceSha256:
    "808594c52c823f7b5b52ffde6aef8ce0b0dd3f1fdea38ad2ea67d1ff2b784ede",
});
assert.deepEqual(profile.algorithmContract.fit, {
  canvas: { height: 1536, width: 1024 },
  placement: { x: 244, y: 200 },
  rounding: "target_height_exact_then_target_width_round_half_up",
  scaleRational: { denominator: 1309, numerator: 1136 },
  sourceForegroundBbox: { xMax: 843, xMin: 226, yMax: 1438, yMin: 130 },
  targetSize: { height: 1136, width: 536 },
});
assert.deepEqual(profile.outputContract.foregroundBbox,
  { xMax: 779, xMin: 244, yMax: 1335, yMin: 200 });
assert.equal(profile.outputContract.outputPngSha256,
  "ff512be1ac75ba0924eab316679dcab4ee171f4a0703014791f2f295e8a6d327");
assert.deepEqual(profile.outputContract.silhouetteEdgeDominanceCounts,
  { cyan: 0, green: 0, magenta: 0 });

const drift = structuredClone(profile); drift.outputContract.minimumMarginPx = 47;
assert.throws(() => validateProfile(drift), /identity_source_bound_fit_profile_hash_mismatch/);
assert.throws(() => executeIdentityAnchoredSourceBoundFit({ profile,
  sourceBytes: Buffer.from("wrong"), receiptBytes: Buffer.from("wrong") }),
  /identity_source_bound_fit_binding_not_registered/);

const fixtureRoot =
  "C:/Users/parkv/.codex/worktrees/ebe6/ProjectBS-agent/output/"+
  "generated-media-builtin-imagegen/v1/character_single_image/character.seojin.2/"+
  "gmidentity1.042d1a21cf545281c894";
const sourcePath = join(fixtureRoot, "original.png");
const receiptPath = join(fixtureRoot, "generation-receipt.json");
if (existsSync(sourcePath) && existsSync(receiptPath)) {
  const sourceBytes = readFileSync(sourcePath); const receiptBytes = readFileSync(receiptPath);
  assert.equal(sha(sourceBytes), profile.sourceBinding.sourceSha256);
  assert.equal(sha(receiptBytes), profile.sourceBinding.generationReceiptSha256);
  const first = executeIdentityAnchoredSourceBoundFit({ profile, sourceBytes, receiptBytes });
  const second = executeIdentityAnchoredSourceBoundFit({ profile, sourceBytes, receiptBytes });
  assert.deepEqual(first.outputBytes, second.outputBytes);
  assert.equal(sha(first.outputBytes), profile.outputContract.outputPngSha256);
  assert.deepEqual(first.outputValidation, {
    foregroundBbox: { xMin: 244, yMin: 200, xMax: 779, yMax: 1335 },
    foregroundPixelCount: 320975, alphaComponentCount: 1,
    transparentRgbNonzeroCount: 0,
    silhouetteEdgeDominanceCounts: { green: 0, cyan: 0, magenta: 0 },
  });
  assert.equal(first.providerCalled, false); assert.equal(first.submitCount, 0);
}

for (const surface of [
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaSourceBoundMainCompletionGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaChromaUncompositePrompt.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md",
]) {
  const text = readFileSync(join(repo, surface), "utf8").replaceAll("\r\n", "\n");
  assert.ok(text.includes(PROFILE_KEY), `${surface}: profile key`);
  assert.ok(text.includes(PROFILE_PAYLOAD_SHA256), `${surface}: profile hash`);
}

console.log({ profileKey: PROFILE_KEY, profilePayloadSha256: PROFILE_PAYLOAD_SHA256,
  expectedOutputPngSha256: profile.outputContract.outputPngSha256,
  providerCalled: false, submitCount: 0 });
console.log("generated media identity-anchored source-bound fit: PASS");
