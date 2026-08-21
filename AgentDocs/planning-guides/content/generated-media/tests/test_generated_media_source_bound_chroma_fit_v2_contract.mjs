// G2 exact-source residual carrier uncomposite successor vectors.
// The registered real source is represented by immutable measured evidence;
// this test does not execute or write that media.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { canonicalJson } from
  "../helpers/generated_media_canonical_serializers_v1.mjs";
import { deriveSourceEvidence } from
  "../helpers/generated_media_source_bound_chroma_uncomposite_v1.mjs";
import {
  PROFILE_KEY,
  PROFILE_PAYLOAD_SHA256,
  G3_EDIT_PROFILE_KEY,
  G3_EDIT_PROFILE_PAYLOAD_SHA256,
  deriveCarrierModels,
  recoverExpandedFringe,
  removeTargetResidualCarrier,
  validateOutputContract,
  validateProfile,
} from "../helpers/generated_media_source_bound_chroma_fit_v2.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, "..");
const repo = join(root, "..", "..", "..", "..");
const profile = JSON.parse(readFileSync(join(root, "helpers",
  "generated_media_source_bound_chroma_fit_profile_v2.json"), "utf8"));
const oldProfile = JSON.parse(readFileSync(join(root, "helpers",
  "generated_media_source_bound_chroma_fit_profile_v1.json"), "utf8"));
const g3Profile = JSON.parse(readFileSync(join(root, "helpers",
  "generated_media_source_bound_chroma_fit_g3_edit_profile_v1.json"), "utf8"));
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const jcsSha = (value) => sha(Buffer.from(canonicalJson(value), "utf8"));

assert.equal(profile.profileKey, PROFILE_KEY);
assert.equal(jcsSha(profile), PROFILE_PAYLOAD_SHA256);
assert.equal(PROFILE_PAYLOAD_SHA256,
  "84db44afba6bce328a51f078f2147055846f282de71b2c56b9d7876264f9bccf");
assert.equal(validateProfile(profile), profile);
assert.equal(g3Profile.profileKey, G3_EDIT_PROFILE_KEY);
assert.equal(jcsSha(g3Profile), G3_EDIT_PROFILE_PAYLOAD_SHA256);
assert.equal(G3_EDIT_PROFILE_PAYLOAD_SHA256,
  "f1b9563f271334c5addbf780bec1bca886f540d1a804e93684f56774c516a086");
assert.equal(validateProfile(g3Profile), g3Profile);
assert.equal(jcsSha(oldProfile),
  "ca3102e7369da6513b4c4e462e68b373da618a2b1630a393d803d37136d812df");
assert.deepEqual(profile.predecessorBinding, {
  meaningUnchanged: true,
  profileKey: "projectbs_character_open_ink_source_bound_green_carrier_fit@1.0.0",
  profilePayloadSha256: "ca3102e7369da6513b4c4e462e68b373da618a2b1630a393d803d37136d812df",
});
assert.deepEqual({ source: profile.sourceBinding.sourceSha256,
  receipt: profile.sourceBinding.generationReceiptSha256,
  rejectedGreen: profile.rejectedPredecessorCandidateEvidence
    .partialAlphaPositiveGreenPixelCount,
  fringeCount: profile.algorithmContract.sourceFringe.pixelCount,
  fringeMask: profile.algorithmContract.sourceFringe.sourceFringeMaskSha256,
  residual: profile.algorithmContract.targetResidual
    .preResidualPartialAlphaPositiveGreenCount,
  solved: profile.algorithmContract.targetResidual.residualSolvedPixelCount,
  cleared: profile.algorithmContract.targetResidual.residualClearedPixelCount,
  post: profile.algorithmContract.targetResidual
    .postResidualPartialAlphaPositiveGreenCount }, {
  source: "66dc1c94be2e38e9dc4d6ff15b4b6b0699353b9830d718ec16743dc4ff92acf9",
  receipt: "4e4457df58f31eff61adb1a93d8915e8a2fb0926e28cb9eb8358f2ce8b606526",
  rejectedGreen: 6543,
  fringeCount: 19197,
  fringeMask: "c64677f64282b10fb4b9c91875b47b8c6d3443a7732a9c93cc5d3d61b9130c2f",
  residual: 457, solved: 442, cleared: 15, post: 0,
});
assert.equal(profile.algorithmContract.targetResidual.residualTargetMaskSha256,
  "af95f4dc84665463d4337cff5f785a744fcc91a0f27e25b14edc5823e1413b25");
assert.equal(profile.algorithmContract.targetResidual
  .residualClearedTargetMaskSha256,
  "11438f1108dc1437c565e80f427e80bc8c7f3e105277b0519321bebd08d778b7");
assert.equal(profile.outputContract.partialAlphaPositiveGreenExcessCount, 0);
assert.deepEqual(profile.algorithmContract.fit.targetForegroundBbox,
  { xMin: 233, yMin: 128, xMax: 790, yMax: 1376 });

// Synthetic topology: opaque carrier border, a two-layer green composite
// fringe, and protected navy center. It exercises expansion beyond v1 one-ring.
const width = 9; const height = 9;
const rgb = Buffer.alloc(width * height * 3);
const set = (x, y, color) => rgb.set(color, (y * width + x) * 3);
for (let y = 0; y < height; y += 1) for (let x = 0; x < width; x += 1)
  set(x, y, [10, 240, 10]);
for (let y = 2; y <= 6; y += 1) for (let x = 2; x <= 6; x += 1)
  set(x, y, [30, 140, 40]);
set(4, 4, [25, 35, 70]);
const evidence = deriveSourceEvidence({ width, height, rgb });
const models = deriveCarrierModels({ width, height, evidence });
assert.ok(models.measured.fringePixelCount > evidence.measured.topologyEvidence.oneRingPixelCount);
assert.ok(models.measured.maxGraphDistance >= 2);
const recovered = recoverExpandedFringe({ width, height, rgb, evidence, models });
assert.equal(recovered.protectedRgbMismatchCount, 0);
assert.deepEqual([...recovered.rgba.subarray((4 * width + 4) * 4,
  (4 * width + 4) * 4 + 4)], [25, 35, 70, 255]);
for (let offset = 0; offset < recovered.rgba.length; offset += 4)
  if (recovered.rgba[offset + 3] === 0)
    assert.deepEqual([...recovered.rgba.subarray(offset, offset + 4)], [0, 0, 0, 0]);

// Target residual model: the pure inverse-composite search must remove a
// green-dominant partial sample or clear it when no in-range solution exists.
const residualInput = Buffer.from([50, 100, 50, 128]);
const tinyRgb = Buffer.from([10, 240, 10]);
const tinyRoot = new Int32Array([0]);
const residual = removeTargetResidualCarrier({ resizedRgba: residualInput,
  rgb: tinyRgb, fullRoot: tinyRoot, sourceFringeMask: Uint8Array.of(1), sourceWidth: 1,
  sourceBbox: { xMin: 0, yMin: 0, xMax: 0, yMax: 0 },
  targetWidth: 1, targetHeight: 1 });
assert.equal(residual.measured.residualPixelCount, 1);
assert.equal(residual.measured.postResidualPartialAlphaPositiveGreenCount, 0);
assert.deepEqual([...residual.rgba], [0, 0, 0, 0]);

const outputFixture = Buffer.alloc(3 * 3 * 4);
outputFixture.set([12, 20, 31, 255], (1 * 3 + 1) * 4);
assert.deepEqual(validateOutputContract({ rgba: outputFixture, width: 3, height: 3,
  outputContract: { canvas: { width: 3, height: 3 }, alphaMin: 0, alphaMax: 255,
    foregroundBbox: { xMin: 1, yMin: 1, xMax: 1, yMax: 1 },
    partialAlphaPositiveGreenExcessCount: 0 } }), {
  alphaMin: 0, alphaMax: 255,
  foregroundBbox: { xMin: 1, yMin: 1, xMax: 1, yMax: 1 },
  partialAlphaPositiveGreenExcessCount: 0, transparentPixelRgb: [0, 0, 0],
  cornersAndFullBorderAlpha: 0, noClipping: true,
});

// The G3 edit is a distinct exact fixture. Its one rational scale is selected
// from width; height is the closed round-half-up image of that same scale.
const sourceWidth = g3Profile.algorithmContract.fit.sourceForegroundBbox.xMax
  - g3Profile.algorithmContract.fit.sourceForegroundBbox.xMin + 1;
const sourceHeight = g3Profile.algorithmContract.fit.sourceForegroundBbox.yMax
  - g3Profile.algorithmContract.fit.sourceForegroundBbox.yMin + 1;
assert.equal(sourceWidth, 716);
assert.equal(sourceHeight, 1092);
assert.deepEqual(g3Profile.algorithmContract.fit.scaleRational,
  { numerator: 176, denominator: 179 });
assert.equal(704 * 179, sourceWidth * 176);
assert.equal(Math.floor((sourceHeight * 176 * 2 + 179) / (2 * 179)), 1074);
assert.deepEqual(g3Profile.algorithmContract.fit.targetSize,
  { width: 704, height: 1074 });
assert.deepEqual(g3Profile.algorithmContract.fit.placement, { x: 160, y: 231 });
assert.deepEqual(g3Profile.outputContract.foregroundBbox,
  { xMin: 160, yMin: 231, xMax: 863, yMax: 1304 });
assert.equal(g3Profile.sourceBinding.sourceSha256,
  "7394278aac0553bd7f0967f84ec5654a61de438efde4626c439d3f64cead3e4a");
assert.equal(g3Profile.sourceBinding.editExecutionReceiptSha256,
  "df9921b80222ab4a3a59f5dd35753d48e8988d76e4ea7b81cf690a522a453cc3");
assert.equal(g3Profile.outputContract.outputPngSha256,
  "190f6e937ec61ed59dd2c04a415b937ef5d5ba6f46d1f5d476a38b54de11563a");
assert.equal(g3Profile.outputContract.partialAlphaPositiveGreenExcessCount, 0);
assert.equal(g3Profile.outputContract.modeledResidualGreenCyanMagentaFringeCount, 0);

const drift = structuredClone(profile);
drift.algorithmContract.targetResidual.residualSolvedPixelCount += 1;
assert.throws(() => validateProfile(drift), /source_chroma_fit_v2_profile_hash_mismatch/);

const surfaces = [
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaSourceBoundMainCompletionGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md",
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
  assert.ok(text.includes(G3_EDIT_PROFILE_KEY), `${surface}: G3 profile key`);
  assert.ok(text.includes(G3_EDIT_PROFILE_PAYLOAD_SHA256), `${surface}: G3 profile hash`);
}

console.log({ profileKey: PROFILE_KEY, profilePayloadSha256: PROFILE_PAYLOAD_SHA256,
  g3ProfileKey: G3_EDIT_PROFILE_KEY,
  g3ProfilePayloadSha256: G3_EDIT_PROFILE_PAYLOAD_SHA256,
  rejectedPartialAlphaPositiveGreenCount: 6543,
  expectedPostResidualPartialAlphaPositiveGreenCount: 0,
  providerCalled: false, submitCount: 0 });
console.log("generated media source-bound chroma fit v2 contract: PASS");
