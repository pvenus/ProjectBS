// Closed vectors for noninteractive Generated Media execution authority.
// This test performs no provider, media, evaluation, publication, or copy work.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const contentGuideRoot = join(guideRoot, "..");
const promptRoot = join(guideRoot, "..", "..", "..", "task-prompts", "content");
const generatedPromptRoot = join(promptRoot, "generated-media");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}
const sha256 = (value) => createHash("sha256").update(value).digest("hex");

const stages = ["planning", "routing", "prompt_authoring",
  "character_image_generation", "animation_generation",
  "preservation_packaging", "evaluation", "git_publication",
  "project_promotion"];

function policyFixture(platformApprovalMode = "not_required") {
  const payload = {
    schemaVersion: "generated_media_noninteractive_execution_policy_v1",
    pipelineRunId: "gmpipeline1.seojin.current",
    authorityRequestRef: "codex-task:authenticated-request-1",
    declaredStages: structuredClone(stages),
    declaredWorkspaceRoots: ["C:/github/ProjectBS-agent",
      "C:/github/design_evaluation"],
    providerSubmitMaximum: 1,
    providerRetryMaximum: 0,
    replaceExistingAuthorized: false,
    destructiveDeleteAuthorized: false,
    platformApprovalMode,
  };
  return { ...payload,
    policyPayloadSha256: sha256(Buffer.from(canonicalJson(payload), "utf8")) };
}

function validatePolicy(policy) {
  assert.deepEqual(Object.keys(policy), ["schemaVersion", "pipelineRunId",
    "authorityRequestRef", "declaredStages", "declaredWorkspaceRoots",
    "providerSubmitMaximum", "providerRetryMaximum",
    "replaceExistingAuthorized", "destructiveDeleteAuthorized",
    "platformApprovalMode", "policyPayloadSha256"]);
  const { policyPayloadSha256, ...payload } = policy;
  assert.equal(policy.schemaVersion,
    "generated_media_noninteractive_execution_policy_v1");
  assert.equal(new Set(policy.declaredStages).size, policy.declaredStages.length);
  assert.ok(policy.declaredStages.every((stage) => stages.includes(stage)));
  assert.ok(Number.isInteger(policy.providerSubmitMaximum));
  assert.ok(Number.isInteger(policy.providerRetryMaximum));
  assert.ok(["not_required", "bundled_required"].includes(
    policy.platformApprovalMode));
  assert.equal(policyPayloadSha256,
    sha256(Buffer.from(canonicalJson(payload), "utf8")));
  return true;
}

function approvalDecision({ policy, hostRequiresApproval = false,
  bundleGranted = false, previousApprovalRequests = 0, action = "routine",
  targetExists = false } = {}) {
  validatePolicy(policy);
  const blockers = {
    overwrite: "existing_project_content_replace_not_authorized",
    destructive_delete: "destructive_delete_not_authorized",
    extra_submit: "provider_submit_or_retry_limit_exceeded",
    retry: "provider_submit_or_retry_limit_exceeded",
    credentials: "credential_or_elevation_required",
    elevation: "credential_or_elevation_required",
    outside_root: "write_root_outside_declared_scope",
    scope_expansion: "material_scope_expansion_required",
  };
  if (targetExists && !policy.replaceExistingAuthorized) {
    return { state: "blocked", failureType: blockers.overwrite,
      approvalRequestsCount: previousApprovalRequests,
      bundledApprovalUsed: previousApprovalRequests === 1 };
  }
  if (Object.hasOwn(blockers, action)) {
    return { state: "blocked", failureType: blockers[action],
      approvalRequestsCount: previousApprovalRequests,
      bundledApprovalUsed: previousApprovalRequests === 1 };
  }
  if (!hostRequiresApproval) return { state: "execute",
    approvalRequestsCount: 0, bundledApprovalUsed: false };
  if (previousApprovalRequests > 0 && !bundleGranted) return { state: "blocked",
    failureType: "generated_media_bundled_platform_approval_unavailable",
    approvalRequestsCount: 1, bundledApprovalUsed: false };
  if (!bundleGranted) return { state: "request_bundle",
    approvalRequestsCount: 1, bundledApprovalUsed: false };
  assert.equal(previousApprovalRequests, 1);
  return { state: "execute", approvalRequestsCount: 1,
    bundledApprovalUsed: true };
}

const noApprovalPolicy = policyFixture();
assert.equal(validatePolicy(noApprovalPolicy), true);
assert.deepEqual(approvalDecision({ policy: noApprovalPolicy }), {
  state: "execute", approvalRequestsCount: 0, bundledApprovalUsed: false });

const bundledPolicy = policyFixture("bundled_required");
assert.deepEqual(approvalDecision({ policy: bundledPolicy,
  hostRequiresApproval: true }), { state: "request_bundle",
  approvalRequestsCount: 1, bundledApprovalUsed: false });
assert.deepEqual(approvalDecision({ policy: bundledPolicy,
  hostRequiresApproval: true, bundleGranted: true,
  previousApprovalRequests: 1 }), { state: "execute",
  approvalRequestsCount: 1, bundledApprovalUsed: true });
assert.deepEqual(approvalDecision({ policy: bundledPolicy,
  hostRequiresApproval: true, previousApprovalRequests: 1 }), {
  state: "blocked",
  failureType: "generated_media_bundled_platform_approval_unavailable",
  approvalRequestsCount: 1, bundledApprovalUsed: false });

for (const [action, failureType] of Object.entries({
  overwrite: "existing_project_content_replace_not_authorized",
  destructive_delete: "destructive_delete_not_authorized",
  extra_submit: "provider_submit_or_retry_limit_exceeded",
  retry: "provider_submit_or_retry_limit_exceeded",
  credentials: "credential_or_elevation_required",
  elevation: "credential_or_elevation_required",
  outside_root: "write_root_outside_declared_scope",
  scope_expansion: "material_scope_expansion_required",
})) {
  assert.equal(approvalDecision({ policy: noApprovalPolicy, action }).failureType,
    failureType);
}
assert.equal(approvalDecision({ policy: noApprovalPolicy,
  targetExists: true }).failureType,
"existing_project_content_replace_not_authorized");

const policyGuide = read(join(guideRoot,
  "GeneratedMediaNoninteractiveExecutionPolicyGuide.md"));
const inheritedSurfaces = [
  join(guideRoot, "GeneratedMediaPlanningHandoffGuide.md"),
  join(guideRoot, "GeneratedMediaRequestRoutingGuide.md"),
  join(guideRoot, "GeneratedMediaImageGenOnlyContractGuide.md"),
  join(guideRoot, "GeneratedMediaPreservationPackagingGuide.md"),
  join(guideRoot, "GeneratedMediaEvaluationPackageGuide.md"),
  join(contentGuideRoot, "GeneratedImageProjectPromotionGuide.md"),
  join(generatedPromptRoot, "GeneratedMediaPipelineOrchestrationPrompt.md"),
  join(generatedPromptRoot, "GeneratedMediaRequestRoutingPrompt.md"),
  join(generatedPromptRoot, "ImageGenCharacterImagePromptAuthoringPrompt.md"),
  join(generatedPromptRoot, "ImageGenCharacterImageGenerationPrompt.md"),
  join(generatedPromptRoot, "ImageGenAnimationPromptAuthoringPrompt.md"),
  join(generatedPromptRoot, "ImageGenAnimationGenerationPrompt.md"),
  join(generatedPromptRoot, "GeneratedMediaPreservationPackagingPrompt.md"),
  join(promptRoot, "GeneratedImageEvaluationPrompt.md"),
  join(promptRoot, "GeneratedImageProjectPromotionPrompt.md"),
].map(read);

assert.match(policyGuide, /generated_media_noninteractive_execution_policy_v1/);
assert.match(policyGuide, /generated_media_bundled_platform_approval_request_v1/);
assert.match(policyGuide, /exactly one policy|exactly one complete bundled approval/i);
assert.match(policyGuide, /MUST[\s\S]*NOT ask again/);
assert.match(policyGuide, /accepted-output preservation/);
assert.match(policyGuide, /completed PASS/);
assert.match(policyGuide, /canonical target is[\s\S]*absent/);
assert.match(policyGuide, /protected Git publication/);
assert.match(policyGuide, /approvalRequestsCount: 0 \| 1/);
assert.match(policyGuide, /bundledApprovalUsed: boolean/);
for (const token of ["existing_project_content_replace_not_authorized",
  "destructive_delete_not_authorized",
  "provider_submit_or_retry_limit_exceeded",
  "credential_or_elevation_required", "write_root_outside_declared_scope",
  "material_scope_expansion_required"]) assert.match(policyGuide, new RegExp(token));
for (const surface of inheritedSurfaces) {
  assert.match(surface, /GeneratedMediaNoninteractiveExecutionPolicyGuide/);
}

const routingGuide = inheritedSurfaces[1];
assert.match(routingGuide, /approvalRequestsCount/);
assert.match(routingGuide, /bundledApprovalUsed/);
const orchestrationPrompt = inheritedSurfaces[6];
assert.match(orchestrationPrompt,
  /generated_media_bundled_platform_approval_unavailable/);
assert.match(orchestrationPrompt, /두 번째 prompt를 금지/);

console.log({ policyPayloadSha256: noApprovalPolicy.policyPayloadSha256,
  happyPathApprovalRequestsCount: 0, bundledApprovalRequestsCount: 1,
  secondApprovalRequestsAllowed: false, providerCalled: false, submitCount: 0 });
console.log("generated media noninteractive execution policy vectors: PASS");
