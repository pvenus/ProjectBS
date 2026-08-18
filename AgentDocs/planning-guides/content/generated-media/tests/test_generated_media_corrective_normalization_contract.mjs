// Closed vectors for accepted corrective PNG normalization and exact-average
// 8fps GIF centisecond quantization. No media/provider/evaluation/copy occurs.

import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const planningGuideRoot = join(guideRoot, "..", "..");
const promptRoot = join(guideRoot, "..", "..", "..", "task-prompts", "content");
const generatedPromptRoot = join(promptRoot, "generated-media");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");

const rgbKey = (rgb) => rgb.join(",");
const sameRgb = (left, right) => rgbKey(left) === rgbKey(right);
const assertClosedKeys = (value, keys) =>
  assert.deepEqual(Object.keys(value).sort(), [...keys].sort());
const expectedColor = (x, y, colors, tileSize = 1, phaseX = 0, phaseY = 0) =>
  colors[(Math.floor((x + phaseX) / tileSize)
    + Math.floor((y + phaseY) / tileSize)) % 2];

function normalizeBoundaryCheckerboard(source, plan) {
  const height = source.length;
  const width = source[0].length;
  assert.ok(source.every((row) => row.length === width));
  const border = [];
  for (let x = 0; x < width; x += 1) border.push([x, 0]);
  for (let y = 1; y < height; y += 1) border.push([width - 1, y]);
  for (let x = width - 2; x >= 0; x -= 1) border.push([x, height - 1]);
  for (let y = height - 2; y > 0; y -= 1) border.push([0, y]);
  const colors = [...new Map(border.map(([x, y]) =>
    [rgbKey(source[y][x]), source[y][x]])).values()]
    .sort((a, b) => rgbKey(a).localeCompare(rgbKey(b)));
  if (colors.length !== 2) throw new Error("checkerboard_foreground_contact_ambiguous");
  if (border.some(([x, y]) => !sameRgb(source[y][x],
    expectedColor(x, y, colors, plan.tileSizePx, plan.phaseX, plan.phaseY)))) {
    throw new Error("checkerboard_background_pattern_unsupported");
  }
  const visited = Array.from({ length: height }, () => Array(width).fill(false));
  const queue = border.filter(([x, y]) => sameRgb(source[y][x],
    expectedColor(x, y, colors, plan.tileSizePx, plan.phaseX, plan.phaseY)));
  for (const [x, y] of queue) visited[y][x] = true;
  for (let i = 0; i < queue.length; i += 1) {
    const [x, y] = queue[i];
    for (const [nx, ny] of [[x - 1, y], [x + 1, y], [x, y - 1], [x, y + 1]]) {
      if (nx < 0 || ny < 0 || nx >= width || ny >= height || visited[ny][nx]) continue;
      if (sameRgb(source[ny][nx],
        expectedColor(nx, ny, colors, plan.tileSizePx, plan.phaseX, plan.phaseY))) {
        visited[ny][nx] = true;
        queue.push([nx, ny]);
      }
    }
  }
  const output = source.map((row, y) => row.map((rgb, x) =>
    [...rgb, visited[y][x] ? 0 : 255]));
  return { colors, output, removedPixelCount: visited.flat().filter(Boolean).length };
}

const colors = [[204, 204, 204], [238, 238, 238]];
const correctiveInput = {
  schemaVersion: "generated_media_corrective_single_image_input_v1",
  authorityMain: "563b72d844a73d32a4abd1ffec2cbf519b3eb43a",
  requestId: "gmplan2.character_single_image.character.seojin.1.e5537d6487d06b88f452",
  contentId: "character.seojin.1",
  acceptedResultCaptureRecordId:
    "gmaccept1.character_single_image.character.seojin.1.ccfbcad4e1e8dd818cb8",
  acceptedResultCaptureRecordSha256:
    "06d137ee46adf0f221b73a1e769127cb1b6b0e95f8b3405e873953a6fa101145",
  acceptedReferenceSha256:
    "0fbc5702a04683e2fe483ba230d10f92d31ea88984330c7314f14590313815b0",
  basePromptRecordId:
    "gmprompt3.character_single_image.character.seojin.1.4c37ee9b6e2217168eb2",
  basePromptRecordSha256:
    "4d041c99ac1323e9de3b73613c94c9c227c2978eb2a55d282f8a719e817cb3d3",
  correctivePromptSha256:
    "937be5f941169874fff6bfe67d4b226956cb493aeac561f2efe4f469f1f935e8",
  executionAttemptId:
    "character.seojin.1.character_single_image.corrective_transparent_detail_budget.v1",
  sourceGenerationTaskId: "01a01411-2c0b-7102-868b-f270a4adcfeb",
  outputPath: "C:/exact/corrective-output.png",
  outputSha256:
    "4498817999fb28323eb85f62afefcc33027341640b1a7ce990a3609b32eaeb7e",
  width: 1024, height: 1536, colorMode: "RGB",
  providerCalled: true, submitCount: 1, retryCount: 0,
};
assertClosedKeys(correctiveInput, ["schemaVersion", "authorityMain",
  "requestId", "contentId", "acceptedResultCaptureRecordId",
  "acceptedResultCaptureRecordSha256", "acceptedReferenceSha256",
  "basePromptRecordId", "basePromptRecordSha256", "correctivePromptSha256",
  "executionAttemptId", "sourceGenerationTaskId", "outputPath",
  "outputSha256", "width", "height", "colorMode", "providerCalled",
  "submitCount", "retryCount"]);
assert.equal(correctiveInput.providerCalled, true);
assert.equal(correctiveInput.submitCount, 1);
assert.equal(correctiveInput.retryCount, 0);

const backgroundPlan = {
  schemaVersion: "generated_media_border_checkerboard_alpha_plan_v1",
  algorithmId: "border_exact_checkerboard_boundary_flood_v1",
  candidateDerivation: "outer_border_exact_two_color_unique_checkerboard",
  colorMatch: "exact_rgb", connectivity: 4,
  transparentRgbPolicy: "retain_source_rgb",
  alphaForRemovedBackground: 0, alphaForPreservedPixels: 255,
  pngEncoderName: "test-vector-encoder", pngEncoderVersion: "1.0.0",
  pngCompressionLevel: 9, pngFilter: "none", pngBitDepth: 8,
  pngColorType: "rgba", pngInterlace: false,
};
assertClosedKeys(backgroundPlan, ["schemaVersion", "algorithmId",
  "candidateDerivation", "colorMatch", "connectivity",
  "transparentRgbPolicy", "alphaForRemovedBackground",
  "alphaForPreservedPixels", "pngEncoderName", "pngEncoderVersion",
  "pngCompressionLevel", "pngFilter", "pngBitDepth", "pngColorType",
  "pngInterlace"]);
assert.equal(backgroundPlan.colorMatch, "exact_rgb");
assert.equal(backgroundPlan.connectivity, 4);
assert.equal(backgroundPlan.transparentRgbPolicy, "retain_source_rgb");

const source = Array.from({ length: 7 }, (_, y) => Array.from({ length: 7 },
  (_, x) => structuredClone(expectedColor(x, y, colors))));
const ink = [20, 30, 40];
for (const [x, y] of [[2, 2], [3, 2], [4, 2], [2, 3], [4, 3],
  [2, 4], [3, 4], [4, 4]]) source[y][x] = structuredClone(ink);
// Center keeps an exact candidate color but is enclosed by nonmatching ink.
source[3][3] = structuredClone(expectedColor(3, 3, colors));
// An off-by-one foreground pixel is nonmatching and must never be thresholded.
source[1][3] = [237, 238, 238];

const normalized = normalizeBoundaryCheckerboard(source,
  { tileSizePx: 1, phaseX: 0, phaseY: 0 });
assert.equal(normalized.output[0][0][3], 0);
assert.equal(normalized.output[3][3][3], 255);
assert.equal(normalized.output[1][3][3], 255);
for (let y = 0; y < source.length; y += 1) {
  for (let x = 0; x < source[0].length; x += 1) {
    assert.deepEqual(normalized.output[y][x].slice(0, 3), source[y][x]);
  }
}
const foregroundOnBorder = structuredClone(source);
foregroundOnBorder[0][3] = [255, 0, 0];
assert.throws(() => normalizeBoundaryCheckerboard(foregroundOnBorder,
  { tileSizePx: 1, phaseX: 0, phaseY: 0 }),
/checkerboard_foreground_contact_ambiguous/);
assert.throws(() => normalizeBoundaryCheckerboard(source,
  { tileSizePx: 2, phaseX: 0, phaseY: 0 }),
/checkerboard_background_pattern_unsupported/);

const exactSchedule = [12, 13, 12, 13, 12, 13];
const quantizationPlan = {
  schemaVersion: "generated_media_gif_8fps_centisecond_quantization_plan_v1",
  requestedFpsNumerator: 8, requestedFpsDenominator: 1, frameCount: 6,
  frameDelayCentiseconds: exactSchedule,
  frameDelayMilliseconds: [120, 130, 120, 130, 120, 130],
  totalDurationMilliseconds: 750, playbackMode: "one_shot",
  loopExtensionPresent: false,
  decodedPixelPolicy: "unchanged_full_canvas_rgba",
};
assertClosedKeys(quantizationPlan, ["schemaVersion", "requestedFpsNumerator",
  "requestedFpsDenominator", "frameCount", "frameDelayCentiseconds",
  "frameDelayMilliseconds", "totalDurationMilliseconds", "playbackMode",
  "loopExtensionPresent", "decodedPixelPolicy"]);
function validateGifQuantization(value) {
  assert.deepEqual(Object.keys(value).sort(), ["afterFramePixelSha256s",
    "afterGifSha256", "averageFpsDenominator", "averageFpsNumerator",
    "beforeFramePixelSha256s", "beforeGifSha256", "frameCount",
    "frameDelayCentiseconds", "frameDelayMilliseconds", "gifClosedAndReopened",
    "height", "loopExtensionPresent", "playbackMode",
    "requestedFpsDenominator", "requestedFpsNumerator", "schemaVersion",
    "status", "totalDurationMilliseconds", "width"].sort());
  const valid = value.schemaVersion
      === "generated_media_gif_8fps_centisecond_quantization_receipt_v1"
    && value.requestedFpsNumerator === 8 && value.requestedFpsDenominator === 1
    && value.frameCount === 6
    && JSON.stringify(value.frameDelayCentiseconds) === JSON.stringify(exactSchedule)
    && JSON.stringify(value.frameDelayMilliseconds)
      === JSON.stringify([120, 130, 120, 130, 120, 130])
    && value.frameDelayCentiseconds.every((delay) => delay > 0)
    && value.totalDurationMilliseconds === 750
    && value.averageFpsNumerator === 8 && value.averageFpsDenominator === 1
    && value.playbackMode === "one_shot" && value.loopExtensionPresent === false
    && value.gifClosedAndReopened === true
    && value.width === 640 && value.height === 512
    && value.beforeFramePixelSha256s.length === 6
    && JSON.stringify(value.beforeFramePixelSha256s)
      === JSON.stringify(value.afterFramePixelSha256s);
  assert.equal(value.status, valid ? "valid" : "blocked");
  return valid;
}

const pixelHashes = Array.from({ length: 6 }, (_, index) =>
  String(index + 1).repeat(64));
const quantizationReceipt = {
  schemaVersion: "generated_media_gif_8fps_centisecond_quantization_receipt_v1",
  requestedFpsNumerator: 8, requestedFpsDenominator: 1, frameCount: 6,
  frameDelayCentiseconds: exactSchedule,
  frameDelayMilliseconds: [120, 130, 120, 130, 120, 130],
  totalDurationMilliseconds: 750,
  averageFpsNumerator: 8, averageFpsDenominator: 1,
  playbackMode: "one_shot", loopExtensionPresent: false,
  beforeGifSha256: "8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621",
  afterGifSha256: "a".repeat(64), width: 640, height: 512,
  beforeFramePixelSha256s: pixelHashes,
  afterFramePixelSha256s: structuredClone(pixelHashes),
  gifClosedAndReopened: true, status: "valid",
};
assert.equal(validateGifQuantization(quantizationReceipt), true);
for (const invalid of [
  { frameDelayCentiseconds: [13, 12, 13, 12, 13, 12] },
  { frameDelayCentiseconds: [12, 12, 12, 12, 12, 12] },
  { frameDelayCentiseconds: [0, 13, 12, 13, 12, 13] },
  { loopExtensionPresent: true }, { playbackMode: "loop" },
  { totalDurationMilliseconds: 720 }, { width: 639 },
  { afterFramePixelSha256s: [...pixelHashes.slice(0, 5), "f".repeat(64)] },
]) assert.equal(validateGifQuantization({ ...quantizationReceipt, ...invalid,
  status: "blocked" }), false);

const preservation = read(join(guideRoot,
  "GeneratedMediaPreservationPackagingGuide.md"));
const evaluationPackage = read(join(guideRoot,
  "GeneratedMediaEvaluationPackageGuide.md"));
const imagegenContract = read(join(guideRoot,
  "GeneratedMediaImageGenOnlyContractGuide.md"));
const expressionEvaluation = read(join(guideRoot,
  "GeneratedMediaCharacterExpressionEvaluationGuide.md"));
const animationEvaluation = read(join(planningGuideRoot, "character",
  "EvaluationAnimationGuide.md"));
const preservationPrompt = read(join(generatedPromptRoot,
  "GeneratedMediaPreservationPackagingPrompt.md"));
const evaluationPrompt = read(join(promptRoot, "GeneratedImageEvaluationPrompt.md"));

for (const surface of [preservation, evaluationPackage, imagegenContract,
  preservationPrompt]) {
  assert.match(surface, /border_exact_checkerboard_boundary_flood_v1/);
}
for (const surface of [preservation, imagegenContract, preservationPrompt]) {
  assert.match(surface, /checkerboard_foreground_contact_ambiguous/);
  assert.match(surface, /checkerboard_alpha_normalization_validation_failed/);
}
assert.match(evaluationPackage, /evaluation_package_background_normalization_mismatch/);
for (const surface of [preservation, evaluationPackage, preservationPrompt,
  evaluationPrompt, animationEvaluation]) {
  assert.match(surface, /\[12,\s*13,\s*12,\s*13,\s*12,\s*13\]/);
  assert.match(surface, /750\s*ms|750ms|750 milliseconds/i);
  assert.match(surface, /no (?:NETSCAPE\/application )?loop extension|no loop extension|loopExtensionPresent: false/i);
}
assert.match(expressionEvaluation, /boundary-only 4-connected removal/);
assert.match(evaluationPrompt, /normalized primary/);
assert.match(preservation, /provider-native and other timing contracts are unchanged/);
assert.match(preservation, /timingUniform=true/);
assert.match(preservation,
  /does not require[\s\S]*stored GIF delay integers to be equal/);
assert.match(preservationPrompt, /timingUniform=true/);
assert.match(animationEvaluation, /Legacy and provider-native[\s\S]*unchanged/);

console.log({ checkerboardRemovedPixelCount: normalized.removedPixelCount,
  gifFrameDelaysCentiseconds: exactSchedule, totalDurationMilliseconds: 750,
  exactAverageFps: 8, providerCalled: false, submitCount: 0 });
console.log("generated media corrective normalization vectors: PASS");
