import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import {
  gif89aIndexedSettings,
  inspectCanonicalGif89a,
  inspectCanonicalPngRgba8,
  pngRgba8StoreSettings,
  serializeGif89aIndexed,
  serializePngRgba8,
  settingsSha256,
  sha256Hex,
} from "../helpers/generated_media_canonical_serializers_v1.mjs";

const pngPixels = Uint8Array.from([
  1, 2, 3, 255, 4, 5, 6, 0,
  7, 8, 9, 255, 10, 11, 12, 255,
]);
const pngA = serializePngRgba8({ width: 2, height: 2, rgba: pngPixels });
const pngB = serializePngRgba8({ width: 2, height: 2, rgba: pngPixels });
assert.deepEqual(pngA, pngB);
assert.equal(sha256Hex(pngA), "3cc431a4cb29ee83cd5eb7e15e0e1ba0acf9964700c612db24290cc1529007e0");
assert.deepEqual([...inspectCanonicalPngRgba8(pngA).rgba], [...pngPixels]);
assert.equal(
  settingsSha256(pngRgba8StoreSettings(1024, 1536)),
  "fc309dc17cb484ad1d21868cd3ddf8e824960e28675416d7f97ca4cfd64b6476",
);

const transparentRgb = [240, 236, 228];
const gifFrames = Array.from({ length: 6 }, (_, frame) => Uint8Array.from([
  ...transparentRgb, 0,
  ...transparentRgb, 255,
  10 + frame, 20, 30, 255,
  40, 50 + frame, 60, 255,
]));
const delays = [12, 13, 12, 13, 12, 13];
const gifA = serializeGif89aIndexed({
  width: 2,
  height: 2,
  framesRgba: gifFrames,
  delaysCentiseconds: delays,
  transparentRgb,
});
const gifB = serializeGif89aIndexed({
  width: 2,
  height: 2,
  framesRgba: gifFrames,
  delaysCentiseconds: delays,
  transparentRgb,
});
assert.deepEqual(gifA, gifB);
assert.equal(sha256Hex(gifA), "89555263e5f406bc89b211659b1462baad26b05657e64d6f4f8b96cfaa9439b0");
assert.equal(Buffer.from(gifA).includes(Buffer.from("NETSCAPE")), false);
const gifDecoded = inspectCanonicalGif89a(gifA);
assert.deepEqual(gifDecoded.delaysCentiseconds, delays);
assert.equal(gifDecoded.loopExtensionPresent, false);
assert.equal(gifDecoded.framesRgba.length, 6);
for (let i = 0; i < gifFrames.length; i += 1) {
  assert.deepEqual([...gifDecoded.framesRgba[i]], [...gifFrames[i]]);
}
assert.equal(
  settingsSha256(gif89aIndexedSettings(640, 512, delays, transparentRgb)),
  "cbddb830b28668fdeab81587afcf384614cd26adf690f2d299c998d62a90a4b0",
);
assert.throws(() => serializeGif89aIndexed({
  width: 1,
  height: 1,
  framesRgba: [Uint8Array.from([1, 2, 3, 127])],
  delaysCentiseconds: [12],
  transparentRgb,
}), /gif_alpha_not_binary/);

const guide = readFileSync(new URL("../GeneratedMediaPreservationPackagingGuide.md", import.meta.url), "utf8");
const recordGuide = readFileSync(new URL("../GeneratedMediaRecordGuide.md", import.meta.url), "utf8");
const packageGuide = readFileSync(new URL("../GeneratedMediaEvaluationPackageGuide.md", import.meta.url), "utf8");
const preservationPrompt = readFileSync(new URL("../../../../task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md", import.meta.url), "utf8");

for (const token of [
  "generated_media_png_rgba8_store_v1",
  "generated_media_gif89a_indexed_v1",
  "generated_media_serialization_receipt_v1",
  "generated_media_preservation_index_v2",
  "preservation_index.json",
  "serializerSettingsSha256",
  "orderedDecodedFrameRgbaSha256s",
  "reused_identical",
]) assert.ok(guide.includes(token), `guide missing ${token}`);

for (const text of [recordGuide, packageGuide, preservationPrompt]) {
  assert.ok(text.includes("generated_media_serialization_receipt_v1"));
  assert.ok(text.includes("serializer_output_hash_mismatch"));
}

assert.ok(guide.includes("fc309dc17cb484ad1d21868cd3ddf8e824960e28675416d7f97ca4cfd64b6476"));
assert.ok(guide.includes("cbddb830b28668fdeab81587afcf384614cd26adf690f2d299c998d62a90a4b0"));
assert.ok(!guide.includes("host library default"));

console.log("generated media canonical serializers v1 contract: PASS");
