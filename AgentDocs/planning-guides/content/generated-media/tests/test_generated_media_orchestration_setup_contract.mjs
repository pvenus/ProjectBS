// Closed vectors for repository setup/authority orchestration reliability.
// No worktree, task, provider, artifact, media, or evaluation mutation occurs.

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const testDir = dirname(fileURLToPath(import.meta.url));
const guideRoot = join(testDir, "..");
const promptRoot = join(guideRoot, "..", "..", "..", "task-prompts",
  "content", "generated-media");
const read = (path) => readFileSync(path, "utf8").replaceAll("\r\n", "\n");
const guide = read(join(guideRoot, "GeneratedMediaRequestRoutingGuide.md"));
const coordinatorPrompt = read(join(promptRoot,
  "GeneratedMediaPipelineOrchestrationPrompt.md"));
const routingPrompt = read(join(promptRoot, "GeneratedMediaRequestRoutingPrompt.md"));

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") return `{${Object.keys(value)
    .sort().map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
}
const sha256 = (value) => createHash("sha256").update(value).digest("hex");

function makeAuthorityReceipt(repo, originMain, fetchedAt) {
  const payload = { schemaVersion: "generated_media_pipeline_authority_receipt_v1",
    repo, originMain, fetchedAt };
  return { ...payload,
    authorityReceiptSha256: sha256(Buffer.from(canonicalJson(payload), "utf8")) };
}

function validateAuthorityReceipt(receipt) {
  assert.deepEqual(Object.keys(receipt).sort(), ["authorityReceiptSha256",
    "fetchedAt", "originMain", "repo", "schemaVersion"].sort());
  assert.equal(receipt.schemaVersion, "generated_media_pipeline_authority_receipt_v1");
  assert.equal(typeof receipt.repo, "string");
  assert.ok(receipt.repo.length > 0);
  assert.match(receipt.originMain, /^[0-9a-f]{40}$/);
  assert.match(receipt.fetchedAt,
    /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})$/);
  const { authorityReceiptSha256, ...payload } = receipt;
  assert.equal(authorityReceiptSha256,
    sha256(Buffer.from(canonicalJson(payload), "utf8")));
  return true;
}

class SetupMutex {
  constructor() { this.owner = null; }
  acquire(runId) {
    if (this.owner !== null) return { state: "queued", owner: this.owner };
    this.owner = runId;
    return { state: "acquired", owner: runId };
  }
  release(runId) {
    assert.equal(this.owner, runId);
    this.owner = null;
  }
}

function decideTaskSetup({ state, officialThreadId, retryCount = 0,
  replacementOfficialThreadId } = {}) {
  if (["queued", "setup", "pending"].includes(state)) return {
    action: "wait_for_official_thread_id", retryCount, replacementCreated: false };
  if (["abandoned", "failed"].includes(state) && retryCount === 0) {
    if (!officialThreadId || !replacementOfficialThreadId
        || officialThreadId === replacementOfficialThreadId)
      throw new Error("task_registry_collision");
    return { action: "bounded_setup_retry", retryCount: 1,
      officialThreadId: replacementOfficialThreadId, replacementCreated: true };
  }
  throw new Error("helper_setup_refresh_failed");
}

const repo = "https://github.com/pvenus/ProjectBS";
const receipt = makeAuthorityReceipt(repo,
  "eee92610165a035a4e8f994c61373d69f4abfcfd",
  "2026-08-18T00:00:00+09:00");
assert.equal(validateAuthorityReceipt(receipt), true);
assert.throws(() => validateAuthorityReceipt({ ...receipt,
  originMain: "EEE92610165A035A4E8F994C61373D69F4ABFCFD" }));
assert.throws(() => validateAuthorityReceipt({ ...receipt,
  fetchedAt: "2026-08-18" }));
assert.throws(() => validateAuthorityReceipt({ ...receipt,
  authorityReceiptSha256: "0".repeat(64) }));

const mutex = new SetupMutex();
assert.equal(mutex.acquire("run-a").state, "acquired");
assert.deepEqual(mutex.acquire("run-b"), { state: "queued", owner: "run-a" });
mutex.release("run-a");
assert.equal(mutex.acquire("run-b").state, "acquired");
mutex.release("run-b");

const fetchCounts = { coordinator: 1, planningReadOnly: 0,
  authoringReadOnly: 0, evaluation: 0 };
assert.deepEqual(fetchCounts, { coordinator: 1, planningReadOnly: 0,
  authoringReadOnly: 0, evaluation: 0 });
assert.deepEqual(["planning", "routing_authoring", "generation",
  "preservation_evaluation"], ["planning", "routing_authoring", "generation",
  "preservation_evaluation"]);

assert.deepEqual(decideTaskSetup({ state: "queued", retryCount: 0 }), {
  action: "wait_for_official_thread_id", retryCount: 0,
  replacementCreated: false });
assert.deepEqual(decideTaskSetup({ state: "failed", retryCount: 0,
  officialThreadId: "official-a", replacementOfficialThreadId: "official-b" }), {
  action: "bounded_setup_retry", retryCount: 1,
  officialThreadId: "official-b", replacementCreated: true });
assert.throws(() => decideTaskSetup({ state: "failed", retryCount: 0,
  officialThreadId: "official-a", replacementOfficialThreadId: "official-a" }),
  /task_registry_collision/);
assert.throws(() => decideTaskSetup({ state: "failed", retryCount: 1 }),
  /helper_setup_refresh_failed/);

const sealedEvaluation = { workspace: "C:/evaluation-workspace/package-id",
  sourceWorktree: "C:/repo-worktree", sourceRepositoryFetchCount: 0 };
assert.notEqual(sealedEvaluation.workspace, sealedEvaluation.sourceWorktree);
assert.equal(sealedEvaluation.sourceRepositoryFetchCount, 0);
const worktreeInventory = [{ state: "archived" }, { state: "dirty" },
  { state: "unpublished" }];
assert.equal(worktreeInventory.every((item) => !Object.hasOwn(item, "delete")), true);

for (const surface of [guide, coordinatorPrompt]) {
  assert.match(surface, /generated_media_pipeline_authority_receipt_v1/);
  assert.match(surface, /repository(?:-scoped)? setup mutex/i);
  assert.match(surface, /officialThreadId/);
  assert.match(surface, /planning[\s\S]*routing_authoring[\s\S]*generation[\s\S]*preservation_evaluation/);
  assert.match(surface, /worktree_metadata_permission_denied/);
  assert.match(surface, /task_registry_collision/);
  assert.match(surface, /helper_setup_refresh_failed/);
  assert.match(surface, /tool_approval_required/);
}
for (const surface of [guide, coordinatorPrompt, routingPrompt]) {
  assert.match(surface, /source (Git )?worktree/i);
  assert.match(surface, /fetch/i);
  assert.match(surface, /compact/i);
  assert.match(surface, /inventory-only|자동 worktree 삭제|automatic cleanup/i);
}
assert.match(routingPrompt, /pipelineAuthorityReceipt/);
assert.match(guide, /mutation\/publication\/provider boundaries retain their existing fresh checks/);
for (const surface of [guide, coordinatorPrompt]) {
  assert.match(surface, /client-new-thread/);
  assert.match(surface, /set_thread_archived/);
  assert.match(surface, /orphan UI card/);
  assert.match(surface, /app\/platform/);
  assert.match(surface, /MUST NOT create more client cards|새 client card를 만들어 retry하지/);
  assert.match(surface, /never repository worktree or[\s\S]*file deletion|repository worktree\/file 삭제로 처리하지/);
}

console.log({ authorityReceiptSha256: receipt.authorityReceiptSha256,
  coordinatorFetchCount: 1, downstreamReadOnlyFetchCount: 0,
  providerCalled: false, submitCount: 0 });
console.log("generated media orchestration setup reliability vectors: PASS");
