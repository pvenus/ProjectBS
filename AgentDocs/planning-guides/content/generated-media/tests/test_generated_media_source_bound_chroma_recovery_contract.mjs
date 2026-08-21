// Source-bound chroma recovery authority vectors. No real source is transformed
// and no provider/media/record/evaluation/promotion operation is performed.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import {
  ALGORITHM_KEY,
  ALGORITHM_VERSION,
  PROFILE_KEY,
  PROFILE_PAYLOAD_SHA256,
  deriveSourceEvidence,
  recoverRgba,
  validateFixture,
  validateMeasuredEvidence,
} from "../helpers/generated_media_source_bound_chroma_uncomposite_v1.mjs";
import { canonicalJson } from
  "../helpers/generated_media_canonical_serializers_v1.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, "..");
const repo = join(root, "..", "..", "..", "..");
const profilePath = join(root, "helpers",
  "generated_media_source_bound_chroma_recovery_profile_v1.json");
const profileBytes = readFileSync(profilePath);
const profile = JSON.parse(profileBytes.toString("utf8"));
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const jcsSha = (value) => sha(Buffer.from(canonicalJson(value), "utf8"));

assert.equal(profile.profileKey, PROFILE_KEY);
assert.equal(jcsSha(profile), PROFILE_PAYLOAD_SHA256);
assert.equal(PROFILE_KEY,
  "projectbs_character_open_ink_source_bound_green_carrier_uncomposite@1.0.0");
assert.equal(PROFILE_PAYLOAD_SHA256,
  "b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746");
assert.equal(ALGORITHM_KEY, "generated_media_border_calibrated_green_uncomposite_v1");
assert.equal(ALGORITHM_VERSION, "1.0.0");
assert.equal(profile.generationProfileBinding.expressionProfileKey,
  "projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0");
assert.equal(profile.generationProfileBinding.expressionProfilePayloadHash,
  "b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a");
assert.equal(profile.generationProfileBinding.generationContractMeaning,
  "immutable_exact_00ff00_contract_not_relaxed_or_reinterpreted");
assert.equal(profile.sourceBindings.length, 2);
assert.ok(profile.sourceBindings.every((binding) =>
  !Object.hasOwn(binding, "path") && !Object.hasOwn(binding, "sourcePath")));
assert.throws(() => validateFixture(profile, Buffer.from("unregistered-source"),
  Buffer.from("{}")), /source_chroma_binding_not_registered/);
const driftedProfile = structuredClone(profile);
driftedProfile.outputContract.alphaMin = 1;
assert.throws(() => validateFixture(driftedProfile, Buffer.from("unregistered-source"),
  Buffer.from("{}")), /source_chroma_profile_hash_mismatch/);

const [g2, g3] = profile.sourceBindings;
assert.deepEqual({ contentId: g2.contentId, source: g2.sourceSha256,
  receipt: g2.generationReceiptSha256, floor: g2.borderEvidence.sourceCalibratedFloor,
  edge: g2.topologyEvidence.edgeCarrierPixelCount,
  enclosedCount: g2.topologyEvidence.enclosedCarrierComponentCount,
  enclosedPixels: g2.topologyEvidence.enclosedCarrierPixelCount,
  ring: g2.topologyEvidence.oneRingPixelCount,
  bbox: g2.foregroundEvidence.bbox }, {
  contentId: "character.seojin.2",
  source: "1222a43bf5cc41b3e1d6d261ae8be484746fdd130f85db21a382aec907c3abf2",
  receipt: "d7e9cd9894d2989fd58caba75f0548963eebc510dd3085f9cbda03d0a0f1a74b",
  floor: 214, edge: 1145787, enclosedCount: 60, enclosedPixels: 2986,
  ring: 7276, bbox: { xMin: 49, yMin: 122, xMax: 802, yMax: 1467 },
});
assert.deepEqual({ contentId: g3.contentId, source: g3.sourceSha256,
  receipt: g3.generationReceiptSha256, floor: g3.borderEvidence.sourceCalibratedFloor,
  edge: g3.topologyEvidence.edgeCarrierPixelCount,
  enclosedCount: g3.topologyEvidence.enclosedCarrierComponentCount,
  enclosedPixels: g3.topologyEvidence.enclosedCarrierPixelCount,
  ring: g3.topologyEvidence.oneRingPixelCount,
  bbox: g3.foregroundEvidence.bbox }, {
  contentId: "character.seojin.3",
  source: "2e3333def860d13c0d1e3c955a32fa5e0e9875f55c6da101a3e39dd51f422973",
  receipt: "3a70164373fb8b45debe5767ac316080f6c3fadddc4388bfc1ec79a3d323cb1d",
  floor: 218, edge: 1055854, enclosedCount: 51, enclosedPixels: 2743,
  ring: 10471, bbox: { xMin: 31, yMin: 120, xMax: 961, yMax: 1456 },
});
assert.deepEqual(g2.foregroundEvidence.margins,
  { left: 49, top: 122, right: 221, bottom: 68 });
assert.deepEqual(g3.foregroundEvidence.margins,
  { left: 31, top: 120, right: 62, bottom: 79 });
assert.deepEqual([g2.topologyEvidence.oneRingGreenExcessMin,
  g2.topologyEvidence.oneRingGreenExcessMedian,
  g2.topologyEvidence.oneRingGreenExcessP95,
  g2.topologyEvidence.oneRingGreenExcessMax], [42, 179, 211, 213]);
assert.deepEqual([g3.topologyEvidence.oneRingGreenExcessMin,
  g3.topologyEvidence.oneRingGreenExcessMedian,
  g3.topologyEvidence.oneRingGreenExcessP95,
  g3.topologyEvidence.oneRingGreenExcessMax], [8, 177, 215, 217]);

// Synthetic source: exact edge carrier, one registered enclosed carrier pocket,
// one positive one-ring, and protected navy interior. This exercises the real
// pure transform without touching either registered source.
const width = 13;
const height = 13;
const rgb = Buffer.alloc(width * height * 3);
const set = (x, y, color) => {
  const offset = (y * width + x) * 3;
  rgb.set(color, offset);
};
for (let y = 0; y < height; y += 1)
  for (let x = 0; x < width; x += 1) set(x, y, [10, 240, 10]);
for (let y = 4; y <= 8; y += 1) {
  for (let x = 4; x <= 8; x += 1) set(x, y, [20, 30, 60]);
}
const foreground = [40, 50, 80];
const background = [10, 240, 10];
const blend = foreground.map((value, index) =>
  Math.round((128 * value + 127 * background[index]) / 255));
for (let i = 4; i <= 8; i += 1) {
  set(i, 4, blend); set(i, 8, blend); set(4, i, blend); set(8, i, blend);
}
set(6, 6, background);

const evidence = deriveSourceEvidence({ width, height, rgb });
assert.equal(evidence.floor, 230);
assert.equal(evidence.measured.borderEvidence.outerPerimeterPixelCount, 48);
assert.equal(evidence.measured.topologyEvidence.edgeCarrierPixelCount, 144);
assert.equal(evidence.measured.topologyEvidence.enclosedCarrierComponentCount, 1);
assert.equal(evidence.measured.topologyEvidence.enclosedCarrierPixelCount, 1);
assert.equal(evidence.measured.topologyEvidence.oneRingPixelCount, 16);
assert.deepEqual(evidence.measured.foregroundEvidence.bbox,
  { xMin: 4, yMin: 4, xMax: 8, yMax: 8 });
const recovered = recoverRgba({ width, height, rgb, evidence });
assert.equal(recovered.metrics.transparentPixelCount, 145);
assert.equal(recovered.metrics.partialAlphaPixelCount, 16);
assert.equal(recovered.metrics.nonTransformedRgbMismatchCount, 0);
assert.equal(recovered.metrics.greenFringeCount, 0);
assert.equal(recovered.metrics.newCyanFringeCount, 0);
assert.equal(recovered.metrics.newMagentaFringeCount, 0);
assert.ok(recovered.metrics.rawModelRecompositionErrorMax <= 1);
const protectedOffset = (6 * width + 5) * 4;
assert.deepEqual([...recovered.rgba.subarray(protectedOffset, protectedOffset + 4)],
  [20, 30, 60, 255]);
for (const index of [0, width - 1, width * (height - 1), width * height - 1])
  assert.equal(recovered.rgba[index * 4 + 3], 0);

const syntheticFixture = structuredClone(evidence.measured);
assert.equal(validateMeasuredEvidence(syntheticFixture, evidence.measured), true);
const driftedEvidence = structuredClone(evidence.measured);
driftedEvidence.topologyEvidence.oneRingPixelCount += 1;
assert.throws(() => validateMeasuredEvidence(syntheticFixture, driftedEvidence),
  /source_chroma_calibration_evidence_mismatch/);
const badPerimeter = Buffer.from(rgb);
badPerimeter.set([255, 0, 255], 0);
assert.throws(() => deriveSourceEvidence({ width, height, rgb: badPerimeter }),
  /source_chroma_outer_perimeter_not_green_dominant/);

function validateBranch(input) {
  const strict = ["generationRecordId", "generationRecordSha256"]
    .every((key) => Object.hasOwn(input, key));
  const accepted = ["acceptedResultCaptureRecordId",
    "acceptedResultCaptureRecordSha256"].every((key) => Object.hasOwn(input, key));
  const chroma = ["sourceBoundChromaRecoveryRecordId",
    "sourceBoundChromaRecoveryRecordSha256",
    "sourceBoundChromaRecoveryReceiptSha256"].every((key) => Object.hasOwn(input, key));
  if ([strict, accepted, chroma].filter(Boolean).length !== 1)
    throw new Error("preservation_input_branch_conflict");
  return strict ? "strict" : accepted ? "accepted" : "source_bound_chroma";
}
assert.equal(validateBranch({ sourceBoundChromaRecoveryRecordId: "gmchroma1.vector",
  sourceBoundChromaRecoveryRecordSha256: "a".repeat(64),
  sourceBoundChromaRecoveryReceiptSha256: "b".repeat(64) }), "source_bound_chroma");
assert.throws(() => validateBranch({ generationRecordId: "gmgen2.vector",
  generationRecordSha256: "c".repeat(64),
  sourceBoundChromaRecoveryRecordId: "gmchroma1.vector",
  sourceBoundChromaRecoveryRecordSha256: "a".repeat(64),
  sourceBoundChromaRecoveryReceiptSha256: "b".repeat(64) }),
  /preservation_input_branch_conflict/);

const surfaces = [
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaSourceBoundChromaRecoveryGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaChromaUncompositePrompt.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaCharacterExpressionEvaluationPrompt.md",
];
for (const surface of surfaces) {
  const text = readFileSync(join(repo, surface), "utf8").replaceAll("\r\n", "\n");
  assert.ok(text.includes(PROFILE_KEY), `${surface}: profile key`);
  assert.ok(text.includes(PROFILE_PAYLOAD_SHA256), `${surface}: profile hash`);
}

const oldGuide = readFileSync(join(root,
  "GeneratedMediaOpenInkWashOpaqueChromaSuccessorGuide.md"), "utf8");
const oldPayloadMatch = oldGuide.replaceAll("\r\n", "\n")
  .match(/```json\s*([\s\S]*?)\s*```/);
assert.ok(oldPayloadMatch);
assert.equal(jcsSha(JSON.parse(oldPayloadMatch[1])),
  "b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a");
assert.equal(profile.generationProfileBinding.generationReceipts,
  "immutable_consumed_nonconformant_source_evidence_not_reopened");
assert.ok(!canonicalJson(profile).includes("exact_00ff00_observed"));

console.log({ profileKey: PROFILE_KEY, profilePayloadSha256: PROFILE_PAYLOAD_SHA256,
  g2SourceSha256: g2.sourceSha256, g3SourceSha256: g3.sourceSha256,
  syntheticCarrierPixels: evidence.measured.topologyEvidence.edgeCarrierPixelCount,
  providerCalled: false, submitCount: 0 });
console.log("generated media source-bound chroma recovery contract: PASS");
