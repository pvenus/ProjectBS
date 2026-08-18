// Source-bound v2 normalization vectors. This test performs no media/provider,
// preservation, evaluation, promotion, or project-copy operation.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const planningGuideRoot = join(guideRoot, "..", "..");
const promptRoot = join(guideRoot, "..", "..", "..", "task-prompts", "content");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");
const canonical = (value) => {
  if (Array.isArray(value)) return `[${value.map(canonical).join(",")}]`;
  if (value && typeof value === "object") return `{${Object.keys(value).sort()
    .map((key) => `${JSON.stringify(key)}:${canonical(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
};
const sha = (value) => createHash("sha256").update(value).digest("hex");
const hashObject = (value) => sha(canonical(value));
const keys = (value, expected) => assert.deepEqual(Object.keys(value).sort(),
  [...expected].sort());

const pngPalette = [[246,245,245],[246,246,245],[246,246,246],[246,246,247],
  [246,247,246],[246,247,247],[246,248,246],[246,248,247],[247,246,246],
  [247,246,247],[247,247,246],[247,247,247],[247,247,248],[247,248,246],
  [247,248,247],[247,248,248],[248,247,246],[248,247,247],[248,247,248],
  [248,248,246],[248,248,247],[248,248,248],[249,248,248],[249,249,248],
  [249,249,249],[249,249,250],[249,250,249],[250,249,249],[250,250,250],
  [251,250,250],[251,250,251],[251,251,250],[251,251,251],[252,251,252],
  [252,252,251],[252,252,252],[252,252,253],[252,253,252],[252,253,253],
  [253,252,252],[253,252,253],[253,253,252],[253,253,253],[253,253,254],
  [253,253,255],[253,254,253],[253,254,254],[253,254,255],[253,255,253],
  [253,255,255],[254,253,253],[254,253,254],[254,253,255],[254,254,253],
  [254,254,254],[254,254,255],[254,255,254],[254,255,255],[255,253,254],
  [255,253,255],[255,254,254],[255,254,255],[255,255,254],[255,255,255]];
assert.equal(pngPalette.length, 64);
assert.equal(sha(JSON.stringify(pngPalette)),
  "e1774764cecac66896a991a45d3722f8e495a1e25a02eefdf8868820a3e0e37f");

const pngEvidence = {
  schemaVersion: "generated_media_border_palette_checkerboard_alpha_receipt_v2",
  sourceSha256: "4498817999fb28323eb85f62afefcc33027341640b1a7ce990a3609b32eaeb7e",
  width: 1024, height: 1536, outerBoundaryPixelCount: 5116,
  candidatePalette: pngPalette, candidatePaletteCount: 64,
  candidatePaletteSha256:
    "e1774764cecac66896a991a45d3722f8e495a1e25a02eefdf8868820a3e0e37f",
  outerBoundaryHistogramSha256:
    "385c8ad653886c902c3710934d43fd40caba1ebfb89294fd00a585395f0193bc",
  outerBoundarySequenceSha256:
    "1c5592f50cba7a68799cb8de2a14c1694e1478970f85af6efd71f7ed444af46c",
  cornerRgbs: [[253,253,253],[254,253,253],[253,253,253],[251,251,251]],
  periodicSignatureSha256:
    "2388f74d059d4b5d019a011839912b64425bff8169bb2a77f8367e0fce4e9e92",
  foregroundBoundaryContactDetected: false,
  removedPixelCount: 1178688, candidatePixelCount: 1180163,
  candidatePreservedPixelCount: 1475, noncandidatePixelCount: 392701,
  noncandidateRgbSha256Before:
    "6e54f9cbaf56be626bae822ae1865fcfc7c8bf8c11ef61403fc1560662fe99d6",
  noncandidateRgbSha256After:
    "6e54f9cbaf56be626bae822ae1865fcfc7c8bf8c11ef61403fc1560662fe99d6",
  protectedNoncandidateBBoxBefore: [1,2,1023,1535],
  protectedNoncandidateBBoxAfter: [1,2,1023,1535],
  alphaMaskSha256:
    "f72eca08c2210554ed0db80959ca70b8e793a0b2a88aa3378fb928948441aebb",
  normalizedRgbaPixelSha256:
    "75a4a09ff279776eed3fe582a7f5cd22ebec2a0df636f0106fdf172255ace3ea",
};

const pngKeys = ["schemaVersion", "sourceSha256", "width", "height",
  "outerBoundaryPixelCount", "candidatePalette", "candidatePaletteCount",
  "candidatePaletteSha256", "outerBoundaryHistogramSha256",
  "outerBoundarySequenceSha256", "cornerRgbs", "periodicSignatureSha256",
  "foregroundBoundaryContactDetected", "removedPixelCount",
  "candidatePixelCount", "candidatePreservedPixelCount",
  "noncandidatePixelCount", "noncandidateRgbSha256Before",
  "noncandidateRgbSha256After", "protectedNoncandidateBBoxBefore",
  "protectedNoncandidateBBoxAfter", "alphaMaskSha256",
  "normalizedRgbaPixelSha256"];

function validatePngEvidence(value) {
  keys(value, pngKeys);
  if (value.sourceSha256 !== pngEvidence.sourceSha256
    || value.width !== 1024 || value.height !== 1536
    || value.outerBoundaryPixelCount !== 5116
    || value.candidatePaletteCount !== 64
    || hashObject(value.candidatePalette) !== pngEvidence.candidatePaletteSha256
    || value.candidatePaletteSha256 !== pngEvidence.candidatePaletteSha256
    || value.outerBoundaryHistogramSha256
      !== pngEvidence.outerBoundaryHistogramSha256
    || value.outerBoundarySequenceSha256
      !== pngEvidence.outerBoundarySequenceSha256
    || canonical(value.cornerRgbs) !== canonical(pngEvidence.cornerRgbs))
    return "border_palette_source_fixture_mismatch";
  if (value.periodicSignatureSha256 !== pngEvidence.periodicSignatureSha256)
    return "border_palette_checkerboard_coherence_failed";
  if (value.foregroundBoundaryContactDetected)
    return "border_palette_foreground_contact_detected";
  if (value.removedPixelCount !== 1178688
    || value.candidatePixelCount !== 1180163
    || value.candidatePreservedPixelCount !== 1475
    || value.noncandidatePixelCount !== 392701
    || value.noncandidateRgbSha256Before !== value.noncandidateRgbSha256After
    || value.noncandidateRgbSha256Before
      !== pngEvidence.noncandidateRgbSha256Before
    || canonical(value.protectedNoncandidateBBoxBefore)
      !== canonical(value.protectedNoncandidateBBoxAfter)
    || canonical(value.protectedNoncandidateBBoxBefore)
      !== canonical([1,2,1023,1535])
    || value.alphaMaskSha256 !== pngEvidence.alphaMaskSha256
    || value.normalizedRgbaPixelSha256
      !== pngEvidence.normalizedRgbaPixelSha256)
    return "border_palette_normalization_validation_failed";
  return "valid";
}

assert.equal(validatePngEvidence(pngEvidence), "valid");
assert.equal(validatePngEvidence({ ...pngEvidence, sourceSha256: "0".repeat(64) }),
  "border_palette_source_fixture_mismatch");
assert.equal(validatePngEvidence({ ...pngEvidence,
  candidatePalette: [...pngPalette.slice(0, 63), [1,2,3]] }),
"border_palette_source_fixture_mismatch");
assert.equal(validatePngEvidence({ ...pngEvidence,
  periodicSignatureSha256: "0".repeat(64) }),
"border_palette_checkerboard_coherence_failed");
assert.equal(validatePngEvidence({ ...pngEvidence,
  cornerRgbs: [[1,2,3], ...pngEvidence.cornerRgbs.slice(1)] }),
"border_palette_source_fixture_mismatch");
assert.equal(validatePngEvidence({ ...pngEvidence,
  foregroundBoundaryContactDetected: true }),
"border_palette_foreground_contact_detected");
assert.equal(validatePngEvidence({ ...pngEvidence,
  noncandidateRgbSha256After: "f".repeat(64) }),
"border_palette_normalization_validation_failed");

const gifFrameEvidence = [
  [0,180,195291,[121,43,504,426],"dbb425cf5c7b71d13dbf7d5547ba7f8ae61e85bf6bbb34e2763fd67eb04e90c7","72fb3cf93c16407b1ce6c3c06f0b1678840771f48251bedc446329f7f81d6ee8","b2d48d5d9feac3a072ae6f73d6919594f2d114cbe9fa2ea2aac8f66919a0a8b4"],
  [1,100,195564,[152,42,535,425],"e94d0ea0ab04fb33c87cad0b2685aa49503f3d924e46771b6d662b554bf45f6c","33e030159abd11b2763f5dc2796b8e70c388f4d91fdf8f129b83f813a4e6a5d5","da3b037e840ada64d1b3ef1d57a4d77573f406ab0d2a880f2e7e5eabded02928"],
  [2,70,197317,[166,40,549,423],"df6e8b69c51fab4c37fef25afae0ca7ce60b9c6c0cc63f5472a0f1da07b1b0e2","6b58c5ea098d91f01e7ef1438c5079fa9046dc6c7628d6a4ca112356559a9c72","f5d9838dd91ea4fe1b1995982f35ee1bc00825b3b3925ad46e2ef45544e47264"],
  [3,50,204801,[153,119,536,502],"54a7cf1afabd9b6aa38381bcfc8f119638fb3dd3802c416b6af186526d22a54e","cc380dba6083510328bc5f4ece68048f3ed0527c0128ecdd08aecae616e06792","7b05dc767e17dd70b0390892ba0aeb0d20d8f831a149013871f7bbaf360fa15d"],
  [4,70,203778,[230,117,614,500],"ffc9b5d440e892a1097c8ca47624955d78774dfa6f239a118110dc518e396f74","c746cfd6d2556f6d9ef5dea88d1f95e804f133cb0aa727be267ccc40db0dbfd6","559f9b720805569c5e9df4c4a43c4429f185c254152c9f24736bfbd89ba00c74"],
  [5,170,206915,[185,115,569,498],"0912b9df2b8973f76760497bb4d61f9690fde617a4cd8380d5a3be3b4a7006d3","339c1c40c78572caeffd587ae6bebd907515b00599a03d8762adbc59ff707635","153debc5c9b4606ae7f22d286e7f8e3511af64b9571c5895e3a86a7da130f949"],
].map(([frameIndex, sourceDelayMs, removedPixelCount, foregroundEvidenceBBox,
  sourceRgbSha256, alphaMaskSha256, normalizedRgbaSha256]) => ({ frameIndex,
  sourceDelayMs, removedPixelCount, foregroundEvidenceBBox, sourceRgbSha256,
  alphaMaskSha256, normalizedRgbaSha256 }));
assert.equal(hashObject(gifFrameEvidence),
  "529a84f985437991a112834bee35c8c19c44a271629974b93a7a8bdcf6a7ac49");

const boundarySha =
  "952ce89392b26e09831f8c16c59094ae3a34278567dc12cf1944eb22d734e45a";
const gifEvidence = {
  schemaVersion: "generated_media_gif_observed_boundary_chroma_receipt_v2",
  sourceSha256: "8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621",
  width: 640, height: 512, frameCount: 6,
  outerBoundaryPixelCountPerFrame: 2300,
  observedRemovableRgb: [240,236,228],
  boundaryMatchNumerators: Array(6).fill(2300),
  boundaryMatchDenominators: Array(6).fill(2300),
  cornerMatchPerFrame: Array(6).fill(true),
  boundarySequenceSha256s: Array(6).fill(boundarySha),
  frameEvidence: gifFrameEvidence,
  frameEvidenceSha256:
    "529a84f985437991a112834bee35c8c19c44a271629974b93a7a8bdcf6a7ac49",
  targetDelayCentiseconds: [12,13,12,13,12,13],
  targetTotalDurationMilliseconds: 750,
  playbackMode: "one_shot", loopExtensionPresent: false,
  nonmatchingPixelsUnchanged: true, geometryUnchanged: true,
};

function validateGifEvidence(value) {
  if (value.sourceSha256 !== gifEvidence.sourceSha256
    || value.width !== 640 || value.height !== 512 || value.frameCount !== 6
    || value.outerBoundaryPixelCountPerFrame !== 2300
    || canonical(value.boundarySequenceSha256s)
      !== canonical(gifEvidence.boundarySequenceSha256s))
    return "gif_observed_boundary_source_fixture_mismatch";
  if (canonical(value.observedRemovableRgb) !== canonical([240,236,228])
    || value.boundaryMatchNumerators.some((count) => count !== 2300)
    || value.boundaryMatchDenominators.some((count) => count !== 2300))
    return "gif_observed_boundary_color_ambiguous";
  if (value.cornerMatchPerFrame.some((match) => !match))
    return "gif_observed_boundary_corner_mismatch";
  if (hashObject(value.frameEvidence) !== gifEvidence.frameEvidenceSha256
    || value.frameEvidenceSha256 !== gifEvidence.frameEvidenceSha256
    || canonical(value.targetDelayCentiseconds) !== canonical([12,13,12,13,12,13])
    || value.targetTotalDurationMilliseconds !== 750
    || value.playbackMode !== "one_shot" || value.loopExtensionPresent
    || !value.nonmatchingPixelsUnchanged || !value.geometryUnchanged)
    return "gif_observed_boundary_normalization_validation_failed";
  return "valid";
}

assert.equal(validateGifEvidence(gifEvidence), "valid");
assert.equal(validateGifEvidence({ ...gifEvidence, sourceSha256: "0".repeat(64) }),
  "gif_observed_boundary_source_fixture_mismatch");
assert.equal(validateGifEvidence({ ...gifEvidence,
  observedRemovableRgb: [242,239,230] }),
"gif_observed_boundary_color_ambiguous");
assert.equal(validateGifEvidence({ ...gifEvidence,
  boundaryMatchNumerators: [2299,2300,2300,2300,2300,2300] }),
"gif_observed_boundary_color_ambiguous");
assert.equal(validateGifEvidence({ ...gifEvidence,
  cornerMatchPerFrame: [true,true,false,true,true,true] }),
"gif_observed_boundary_corner_mismatch");
const mutatedFrameEvidence = structuredClone(gifFrameEvidence);
mutatedFrameEvidence[0].normalizedRgbaSha256 = "f".repeat(64);
assert.equal(validateGifEvidence({ ...gifEvidence,
  frameEvidence: mutatedFrameEvidence }),
"gif_observed_boundary_normalization_validation_failed");

const preservation = read(join(guideRoot,
  "GeneratedMediaPreservationPackagingGuide.md"));
const evaluationPackage = read(join(guideRoot,
  "GeneratedMediaEvaluationPackageGuide.md"));
const imagegen = read(join(guideRoot,
  "GeneratedMediaImageGenOnlyContractGuide.md"));
const expression = read(join(guideRoot,
  "GeneratedMediaCharacterExpressionEvaluationGuide.md"));
const animation = read(join(planningGuideRoot, "character",
  "EvaluationAnimationGuide.md"));
const preservationPrompt = read(join(promptRoot, "generated-media",
  "GeneratedMediaPreservationPackagingPrompt.md"));
const evaluationPrompt = read(join(promptRoot, "GeneratedImageEvaluationPrompt.md"));

for (const text of [preservation, evaluationPackage, imagegen,
  preservationPrompt]) {
  assert.match(text, /border_frozen_palette_boundary_flood_v2/);
  assert.match(text, /gif_exact_uniform_boundary_color_flood_v2/);
  assert.match(text, /4498817999fb28323eb85f62afefcc33027341640b1a7ce990a3609b32eaeb7e/);
  assert.match(text, /8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621/);
}
for (const text of [preservation, evaluationPackage, animation,
  preservationPrompt, evaluationPrompt]) {
  assert.match(text, /240,236,228/);
  assert.match(text, /12,13,12,13,12,13/);
  assert.match(text, /750\s*ms|750ms/i);
}
assert.match(expression, /generated_media_border_palette_checkerboard_alpha_receipt_v2/);
assert.match(animation, /generated_media_gif_observed_boundary_chroma_receipt_v2/);
assert.match(preservation, /The v1 two-color algorithm remains unchanged/);
assert.match(preservation, /The v1 declared-color rule and provider-native modes remain unchanged/);

console.log({ pngSourceSha256: pngEvidence.sourceSha256,
  pngPaletteCount: pngPalette.length,
  pngAlphaMaskSha256: pngEvidence.alphaMaskSha256,
  gifSourceSha256: gifEvidence.sourceSha256,
  gifObservedRgb: gifEvidence.observedRemovableRgb,
  gifBoundaryFraction: "2300/2300", providerCalled: false, submitCount: 0 });
console.log("generated media evidence-bound normalization v2 vectors: PASS");
