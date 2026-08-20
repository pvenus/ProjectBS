import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, "../../../../../");
const read = (relative) => fs.readFileSync(path.join(repo, relative));
const readText = (relative) => read(relative).toString("utf8");
const routePath = "AgentDocs/planning-data/generated-media-routing/v2/character_single_image/character.seojin.1/gmroute2.character_single_image.character.seojin.1.73af678abe4608ca5be5.json";
const planningPath = "AgentDocs/planning-data/character/generated-media-handoffs/v2/character.seojin.1/gmplan2.character_single_image.character.seojin.1.005c0d9e417216c3857a.character_single_image.json";
const requestId = "gmplan2.character_single_image.character.seojin.1.005c0d9e417216c3857a";
const alphaKey = "generated_media_true_alpha_foreground@1.0.0";
const alphaHash = "2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108";
const stale = "자연스러운 front three-quarter 전신 자세를 crop 없이 넓은 여백에 두고 uniform removable #F2EFE6 배경에서 그림자 없이 보여야 한다.";
const transparentLockStatements = {
  char_open_wash_v2_negative_halo_scene_shadow: "No halo, vignette, radial gradient, dark backdrop, opaque or color-bearing background, matte, checkerboard, background residue, scene, environment, cast shadow, contact shadow, or shadow substitute; every pixel outside intended foreground must have alpha exactly zero.",
  char_open_wash_v2_positive_identity_on_ivory: "Preserve approved young-adult Korean and Joseon identity and equipment with every pixel outside intended foreground alpha exactly zero, bounded artistic partial alpha only inside intended character, equipment, or pigment silhouette, and no halo, vignette, matte, checkerboard, background residue, scene, or shadow.",
};

function canonical(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonical).join(",")}]`;
  return `{${Object.keys(value).sort().map((k) => `${JSON.stringify(k)}:${canonical(value[k])}`).join(",")}}`;
}
const bytes = (value) => Buffer.from(`${canonical(value)}\n`, "utf8");
const hashBytes = (value) => crypto.createHash("sha256").update(value).digest("hex");
const hashObject = (value) => hashBytes(Buffer.from(canonical(value), "utf8"));
const exactKeys = (value, keys) => assert.deepEqual(Object.keys(value).sort(), [...keys].sort());

const selectionKeys = ["schemaVersion", "projectionKey", "projectionPayloadHash",
  "assetType", "safeMarginPx", "noClipping", "mainLock"];
function validateSelection(selection) {
  try { exactKeys(selection, selectionKeys); } catch { throw new Error("true_alpha_projection_mismatch"); }
  if (selection.schemaVersion !== "generated_media_transparent_foreground_selection_v1"
      || selection.projectionKey !== alphaKey || selection.projectionPayloadHash !== alphaHash
      || selection.assetType !== "character_single_image" || selection.noClipping !== true
      || !Number.isInteger(selection.safeMarginPx) || selection.safeMarginPx < 1)
    throw new Error("true_alpha_projection_mismatch");
  try { exactKeys(selection.mainLock,
    ["rgbaEvidenceRequired", "fullFigureEquipmentPigmentInBounds"]); }
  catch { throw new Error("true_alpha_projection_mismatch"); }
  if (selection.mainLock.rgbaEvidenceRequired !== true
      || selection.mainLock.fullFigureEquipmentPigmentInBounds !== true)
    throw new Error("true_alpha_projection_mismatch");
}
function validateBackground(background, selection) {
  if (selection) {
    validateSelection(selection);
    if (background?.mode !== "transparent" || Object.keys(background).length !== 1)
      throw new Error("true_alpha_branch_conflict");
    return "transparent";
  }
  if (background?.mode !== "removable_solid" || Object.keys(background).sort().join()
      !== ["mode", "color"].sort().join() || typeof background.color !== "string")
    throw new Error(background?.mode === "transparent"
      ? "true_alpha_projection_missing" : "unsupported_record_schema");
  return "legacy";
}
const staleRequired = (items) => items.some((item) =>
  /uniform\s+removable|removable[_ -]solid|opaque\s+generation\s+background|warm[- ]ivory\s+generation\s+background/i.test(item));

const routeLf = read(routePath);
const planningLf = read(planningPath);
const route = JSON.parse(routeLf);
const planning = JSON.parse(planningLf);
assert.equal(route.requestId, requestId);
assert.equal(planning.requestId, requestId);
assert.equal(validateBackground(route.typeSpecification.singleImageSpecification.generationBackground,
  route.transparentForegroundSelection), "transparent");
assert.ok(route.requiredElements.includes(stale));
assert.equal(staleRequired(route.requiredElements), true);

function corrected(value) {
  if (Array.isArray(value)) return value.map(corrected);
  if (!value || typeof value !== "object") return value;
  return Object.fromEntries(Object.entries(value).map(([key, item]) => [key,
    key === "requiredElements" && Array.isArray(item)
      ? item.filter((entry) => entry !== stale).map(corrected) : corrected(item)]));
}

const visual = readText("AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md");
const marker = "### Open ink-wash output-conformance successor profile";
const section = visual.slice(visual.indexOf(marker)).replaceAll("\r\n", "\n");
const profile = JSON.parse(section.match(/```json\s*([\s\S]*?)\s*```/)[1]);
assert.equal(profile.negativeStyleLock.length, 9);
assert.equal(profile.positiveStyleLock.length, 9);

function project(routeValue, planningValue) {
  const selection = routeValue.transparentForegroundSelection;
  const background = routeValue.typeSpecification.singleImageSpecification.generationBackground;
  validateBackground(background, selection);
  assert.deepEqual(selection, planningValue.transparentForegroundSelection);
  assert.deepEqual(selection, routeValue.normalizedRequest.transparentForegroundSelection);
  assert.deepEqual(selection, routeValue.authoringHandoff.transparentForegroundSelection);
  if (staleRequired(routeValue.requiredElements) || staleRequired(planningValue.requiredElements))
    throw new Error("transparent_prompt_required_element_conflict");
  const visualBrief = { schemaVersion: "generated_media_visual_brief_v2", requestId,
    generationBackground: background, requiredElements: routeValue.requiredElements,
    prohibitedElements: routeValue.prohibitedElements,
    transparentForegroundSelection: selection };
  const providerSettingsIntent = { canvas: routeValue.typeSpecification.singleImageSpecification.canvas,
    generationBackground: background, outputFormat: "png",
    transparentForegroundSelection: selection };
  const projectLock = (lock) => transparentLockStatements[lock.constraintId] ?? lock.statement;
  const scenePromptOriginal = [...routeValue.requiredElements,
    ...profile.negativeStyleLock.map(projectLock),
    ...profile.positiveStyleLock.map(projectLock)].join("\n");
  assert.equal(/warm[- ]ivory|#F2EFE6|removable[_ -]solid/i.test(scenePromptOriginal), false);
  assert.equal(scenePromptOriginal.includes(transparentLockStatements.char_open_wash_v2_negative_halo_scene_shadow), true);
  assert.equal(scenePromptOriginal.includes(transparentLockStatements.char_open_wash_v2_positive_identity_on_ivory), true);
  const markdown = Buffer.from(`${scenePromptOriginal}\n`, "utf8");
  const promptHashPayload = { schemaVersion: "generated_media_prompt_hash_payload_v3",
    requestId, routingRecordId: routeValue.routingRecordId,
    planningSnapshotHash: planningValue.planningSnapshot.snapshotHash,
    visualBrief, visualBriefSha256: hashObject(visualBrief),
    scenePromptOriginal, providerPromptPayloadHash: hashBytes(Buffer.from(scenePromptOriginal, "utf8")),
    providerSettingsIntent, providerSettingsIntentSha256: hashObject(providerSettingsIntent),
    requiredElements: routeValue.requiredElements, prohibitedElements: routeValue.prohibitedElements,
    transparentForegroundSelection: selection, promptMarkdownSha256: hashBytes(markdown) };
  const promptPayloadSha256 = hashObject(promptHashPayload);
  const promptRecordId = `gmprompt3.character_single_image.character.seojin.1.${promptPayloadSha256.slice(0, 20)}`;
  const record = { ...promptHashPayload, schemaVersion: "generated_media_prompt_v3",
    promptRecordId, promptPayloadSha256 };
  const recordSha256 = hashBytes(bytes(record));
  const indexEntry = { promptRecordId, recordSha256, promptPayloadSha256,
    requestId, transparentForegroundSelection: selection };
  const generationHandoff = { schemaVersion: "generated_media_generation_handoff_v2",
    requestId, routingRecordId: routeValue.routingRecordId, promptRecordId,
    promptRecordSha256: recordSha256, promptPayloadSha256,
    providerSettingsIntentSha256: hashObject(providerSettingsIntent),
    transparentForegroundSelection: selection, status: "ready_for_generation" };
  return { promptRecordId, promptPayloadSha256, visualBriefSha256: hashObject(visualBrief),
    providerSettingsIntentSha256: hashObject(providerSettingsIntent),
    promptRecordSha256: recordSha256, promptMarkdownSha256: hashBytes(markdown),
    indexEntrySha256: hashObject(indexEntry), generationHandoffSha256: hashObject(generationHandoff) };
}

assert.throws(() => project(route, planning), /transparent_prompt_required_element_conflict/);
const correctedRoute = corrected(route);
const correctedPlanning = corrected(planning);
assert.equal(staleRequired(correctedRoute.requiredElements), false);
const fromLf = project(correctedRoute, correctedPlanning);
assert.deepEqual(project(corrected(JSON.parse(routeLf.toString("utf8").replaceAll("\n", "\r\n"))),
  corrected(JSON.parse(planningLf.toString("utf8").replaceAll("\n", "\r\n")))), fromLf);

assert.equal(validateBackground({ mode: "removable_solid", color: "#F2EFE6" }, undefined), "legacy");
assert.throws(() => validateBackground({ mode: "transparent" }, undefined), /true_alpha_projection_missing/);
assert.throws(() => validateBackground({ mode: "transparent", color: "#F2EFE6" },
  route.transparentForegroundSelection), /true_alpha_branch_conflict/);
assert.throws(() => validateBackground({ mode: "removable_solid", color: "#F2EFE6" },
  route.transparentForegroundSelection), /true_alpha_branch_conflict/);
assert.throws(() => validateBackground({ mode: "transparent" },
  { ...route.transparentForegroundSelection, unknown: true }), /true_alpha_projection_mismatch/);

const surfaces = [
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaTransparentForegroundAuthoringGuide.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md",
  "AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md",
  "AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md",
  "AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md",
];
for (const surface of surfaces) {
  const text = readText(surface);
  for (const token of ["transparentForegroundSelection", "true_alpha_branch_conflict",
    "transparent_prompt_required_element_conflict"]) assert.ok(text.includes(token), `${surface}: ${token}`);
}

console.log({ requestId, ...fromLf });
console.log("generated media transparent prompt-v3 projection: PASS");
