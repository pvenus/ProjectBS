// Exact G2 opaque-carrier and G3 partial-edge fringe successor vectors.
// Real media identity/evidence is hash-bound; this test writes no media.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { canonicalJson } from
  "../helpers/generated_media_canonical_serializers_v1.mjs";
import {
  G2_FINAL_PROFILE_KEY,
  G2_FINAL_PROFILE_PAYLOAD_SHA256,
  G3_FINAL_PROFILE_KEY,
  G3_FINAL_PROFILE_PAYLOAD_SHA256,
  cleanExactOpaqueCarrierAndIsolatedAlpha,
  cleanExactPartialEdgeFringe,
  connectedComponents,
  validateFinalProfile,
} from "../helpers/generated_media_source_bound_chroma_fit_final_v3.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const generatedMedia = join(here, ".."); const helpers = join(generatedMedia, "helpers");
const repo = join(generatedMedia, "..", "..", "..", "..");
const readJson = (name) => JSON.parse(readFileSync(join(helpers, name), "utf8"));
const sha = (bytes) => createHash("sha256").update(bytes).digest("hex");
const jcsSha = (value) => sha(Buffer.from(canonicalJson(value), "utf8"));
const g2 = readJson("generated_media_source_bound_chroma_fit_g2_final_profile_v3.json");
const g3 = readJson("generated_media_source_bound_chroma_fit_g3_final_profile_v2.json");
const g2Predecessor = readJson("generated_media_source_bound_chroma_fit_profile_v2.json");
const g3Predecessor = readJson("generated_media_source_bound_chroma_fit_g3_edit_profile_v1.json");

assert.equal(g2.profileKey, G2_FINAL_PROFILE_KEY);
assert.equal(jcsSha(g2), G2_FINAL_PROFILE_PAYLOAD_SHA256);
assert.equal(G2_FINAL_PROFILE_PAYLOAD_SHA256,
  "5188d2bd92fdf22dded70fe8e3ab60f1fee1aa79ac6072845883072d99a875c2");
assert.equal(validateFinalProfile(g2), g2);
assert.equal(g3.profileKey, G3_FINAL_PROFILE_KEY);
assert.equal(jcsSha(g3), G3_FINAL_PROFILE_PAYLOAD_SHA256);
assert.equal(G3_FINAL_PROFILE_PAYLOAD_SHA256,
  "40cf8dcfbdc9043d1cdadeca64ee34ef8a11566140aa1e0ac8cc0d3b5baae425");
assert.equal(validateFinalProfile(g3), g3);
assert.equal(jcsSha(g2Predecessor),
  "84db44afba6bce328a51f078f2147055846f282de71b2c56b9d7876264f9bccf");
assert.equal(jcsSha(g3Predecessor),
  "f1b9563f271334c5addbf780bec1bca886f540d1a804e93684f56774c516a086");

assert.deepEqual({ rejected: g2.rejectedPredecessorEvidence.outputPngSha256,
  record: g2.rejectedPredecessorEvidence.recordSha256,
  receipt: g2.rejectedPredecessorEvidence.helperReceiptSha256,
  validation: g2.rejectedPredecessorEvidence.validationSha256,
  carrierPixels: g2.algorithmContract.opaqueCarrierCleanup.pixelCount,
  isolated: g2.algorithmContract.isolatedAlphaCleanup.eligibleComponentCount,
  output: g2.outputContract.outputPngSha256 }, {
  rejected: "21271703cc89dedd3b08afbc3df4c7803476a78918e3c4e8fcc49a04e635f095",
  record: "2aa3cbcecf02007bb7baba68a7bee5b00777e94055973cae180e8b0b0ab13896",
  receipt: "90650420f5523181cf9ee9e86abb775f6cb159187c611f5cd2c06ebc8c0691fe",
  validation: "3b1319607644881b14ca3a94e8bb3959fc97e9535d4158308a6f326c447ac276",
  carrierPixels: 400, isolated: 1,
  output: "1b68b5a50c2801a090f25d13f4b22bd7b8afc4e5c6a93b423ddf06abeaa4bfbb",
});
assert.deepEqual(g2.outputContract.alphaComponentAreasDescending,
  [358987, 49, 39, 6, 5, 3, 2]);
assert.deepEqual(g2.outputContract.foregroundBbox,
  { xMin: 233, yMin: 128, xMax: 790, yMax: 1376 });

assert.deepEqual({ rejected: g3.rejectedPredecessorEvidence.outputPngSha256,
  record: g3.rejectedPredecessorEvidence.recordSha256,
  receipt: g3.rejectedPredecessorEvidence.receiptSha256,
  source: g3.sourceBinding.sourceSha256,
  sourceReceipt: g3.sourceBinding.editExecutionReceiptSha256,
  candidates: g3.algorithmContract.edgeFringeCleanup.candidateCount,
  classes: g3.algorithmContract.edgeFringeCleanup.candidateCountByDominance,
  solved: g3.algorithmContract.edgeFringeCleanup.solvedCount,
  cleared: g3.algorithmContract.edgeFringeCleanup.clearedCount,
  post: g3.algorithmContract.edgeFringeCleanup.postCleanupCandidateDominanceCount,
  output: g3.outputContract.outputPngSha256 }, {
  rejected: "190f6e937ec61ed59dd2c04a415b937ef5d5ba6f46d1f5d476a38b54de11563a",
  record: "24db381c0f107dc7801c02b883881b8d5b01e4ac20afd7a218f0a2c2dce38ef5",
  receipt: "eab58cd487270005456bb53651fbf47e280c2800b9504460e57ea6ef779c8ac0",
  source: "7394278aac0553bd7f0967f84ec5654a61de438efde4626c439d3f64cead3e4a",
  sourceReceipt: "df9921b80222ab4a3a59f5dd35753d48e8988d76e4ea7b81cf690a522a453cc3",
  candidates: 5199, classes: { green: 0, cyan: 628, magenta: 4571 },
  solved: 2288, cleared: 2911, post: { green: 0, cyan: 0, magenta: 0 },
  output: "1af18044008dc72c749ade2232f61838aad1d037541f8a57cf439031a1714f2e",
});
assert.deepEqual(g3.outputContract.fitGeometry, { placement: { x: 160, y: 231 },
  scaleRational: { numerator: 176, denominator: 179 },
  targetSize: { width: 704, height: 1074 } });
assert.deepEqual(g3.outputContract.foregroundBbox,
  { xMin: 160, yMin: 231, xMax: 862, yMax: 1304 });

// Small closed component vector: only the exact strong-green component and
// exact one-pixel alpha component may be cleared.
const rgba = Buffer.alloc(4 * 3 * 4);
const put = (x, y, value) => rgba.set(value, (y * 4 + x) * 4);
put(1, 1, [10, 150, 10, 255]); put(2, 1, [12, 140, 12, 255]);
put(3, 2, [40, 50, 60, 255]);
const strong = new Uint8Array(12); strong[5] = 1; strong[6] = 1;
const isolated = new Uint8Array(12); isolated[11] = 1;
const contract = {
  opaqueCarrierCleanup: { maskSha256: sha(strong), pixelCount: 2,
    components: [{ area: 2, bbox: { xMin: 1, yMin: 1, xMax: 2, yMax: 1 } }] },
  isolatedAlphaCleanup: { eligibleAreaExact: 1, eligibleComponentCount: 1,
    eligibleMaskSha256: sha(isolated) },
};
const cleaned = cleanExactOpaqueCarrierAndIsolatedAlpha({ rgba, width: 4,
  height: 3, contract });
assert.deepEqual([...cleaned.rgba], [...Buffer.alloc(rgba.length)]);

// Small partial-edge vector: magenta dominance is corrected only where a
// one-pixel silhouette edge has positive registered fringe support.
const edgeRgba = Buffer.alloc(3 * 3 * 4);
edgeRgba.set([100, 0, 100, 128], (1 * 3 + 1) * 4);
const edgeCandidate = new Uint8Array(9); edgeCandidate[4] = 1;
const edgeContract = { candidateCount: 1,
  candidateCountByDominance: { green: 0, cyan: 0, magenta: 1 },
  candidateMaskSha256: sha(edgeCandidate), solvedCount: 0, clearedCount: 1,
  clearedMaskSha256: sha(edgeCandidate),
  postCleanupCandidateDominanceCount: { green: 0, cyan: 0, magenta: 0 } };
const edge = cleanExactPartialEdgeFringe({ rgba: edgeRgba, width: 3, height: 3,
  source: { width: 1, rgb: Buffer.from([100, 0, 100]) },
  models: { fullRoot: Int32Array.of(0), fringeMask: Uint8Array.of(1) },
  fit: { placement: { x: 1, y: 1 }, targetSize: { width: 1, height: 1 },
    sourceForegroundBbox: { xMin: 0, yMin: 0, xMax: 0, yMax: 0 } },
  contract: edgeContract });
assert.deepEqual([...edge.rgba], [...Buffer.alloc(edgeRgba.length)]);
assert.deepEqual(connectedComponents(new Uint8Array([1, 1, 0, 0]), 2, 2)
  .map(({ area }) => area), [2]);

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
  assert.ok(text.includes(G2_FINAL_PROFILE_KEY), `${surface}: G2 final key`);
  assert.ok(text.includes(G2_FINAL_PROFILE_PAYLOAD_SHA256), `${surface}: G2 final hash`);
  assert.ok(text.includes(G3_FINAL_PROFILE_KEY), `${surface}: G3 final key`);
  assert.ok(text.includes(G3_FINAL_PROFILE_PAYLOAD_SHA256), `${surface}: G3 final hash`);
}

console.log({ g2ProfileKey: G2_FINAL_PROFILE_KEY,
  g2ProfilePayloadSha256: G2_FINAL_PROFILE_PAYLOAD_SHA256,
  g3ProfileKey: G3_FINAL_PROFILE_KEY,
  g3ProfilePayloadSha256: G3_FINAL_PROFILE_PAYLOAD_SHA256,
  providerCalled: false, submitCount: 0 });
console.log("generated media source-bound chroma fit final successors: PASS");
