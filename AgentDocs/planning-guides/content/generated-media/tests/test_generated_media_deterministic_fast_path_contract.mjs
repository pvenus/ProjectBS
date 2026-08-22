// Closed deterministic fast-path orchestration vectors. No workflow artifacts,
// provider calls, media operations, or downstream execution occur.

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
const policy = read(join(guideRoot, "GeneratedMediaDeterministicFastPathGuide.md"));
const routingGuide = read(join(guideRoot, "GeneratedMediaRequestRoutingGuide.md"));
const orchestrationPrompt = read(join(promptRoot,
  "GeneratedMediaPipelineOrchestrationPrompt.md"));
const routingPrompt = read(join(promptRoot, "GeneratedMediaRequestRoutingPrompt.md"));

function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") return `{${Object.keys(value)
    .sort().map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  return JSON.stringify(value);
}
const sha256 = (value) => createHash("sha256").update(value).digest("hex");

const checkKeys = ["liveAuthority", "profileScope", "routeStructure",
  "evaluationRecord", "lineageRecord", "trustedReference", "destinationIndexCas"];
function prerequisiteAudit(checks) {
  assert.deepEqual(Object.keys(checks).sort(), [...checkKeys].sort());
  for (const value of Object.values(checks))
    assert.ok(["pass", "fail", "not_applicable"].includes(value));
  const payload = {
    schemaVersion: "generated_media_fast_path_prerequisite_audit_hash_payload_v1",
    policyKey: "generated_media_deterministic_fast_path_v1",
    pipelineRunId: "g2-g3-animation-run",
    authorityReceiptId: "gmreceipt1.authority",
    authorityReceiptSha256: "a".repeat(64),
    checks,
  };
  const hash = sha256(Buffer.from(canonicalJson(payload), "utf8"));
  return { ...payload,
    schemaVersion: "generated_media_fast_path_prerequisite_audit_v1",
    prerequisiteAuditId: `gmpreaudit1.${hash.slice(0, 20)}`,
    prerequisiteAuditSha256: hash };
}
function mayWrite(audit) {
  return Object.values(audit.checks).every((value) => value !== "fail");
}

const passingChecks = Object.fromEntries(checkKeys.map((key) => [key, "pass"]));
const audit = prerequisiteAudit(passingChecks);
assert.equal(mayWrite(audit), true);
assert.equal(mayWrite(prerequisiteAudit({ ...passingChecks,
  trustedReference: "fail" })), false);
assert.throws(() => prerequisiteAudit({ ...passingChecks, unknown: "pass" }));
assert.throws(() => prerequisiteAudit({ ...passingChecks, routeStructure: "unknown" }));

function recoveryRoute({ identityEquipmentPass, domains, registeredHelper }) {
  const allowed = new Set(["geometry", "carrier", "fringe"]);
  if (identityEquipmentPass && domains.length > 0
      && domains.every((domain) => allowed.has(domain))) {
    return registeredHelper ? "registered_source_bound_postprocess" : "fail_closed";
  }
  return "no_automatic_recovery";
}
assert.equal(recoveryRoute({ identityEquipmentPass: true,
  domains: ["carrier", "fringe"], registeredHelper: true }),
"registered_source_bound_postprocess");
assert.equal(recoveryRoute({ identityEquipmentPass: true,
  domains: ["geometry"], registeredHelper: false }), "fail_closed");
assert.equal(recoveryRoute({ identityEquipmentPass: false,
  domains: ["carrier"], registeredHelper: true }), "no_automatic_recovery");
assert.equal(recoveryRoute({ identityEquipmentPass: true,
  domains: ["phase_timing"], registeredHelper: true }), "no_automatic_recovery");

const suiteSelection = ({ authorityMutation }) => authorityMutation
  ? "full_generated_media_suite" : "targeted_immutable_unit_regression";
assert.equal(suiteSelection({ authorityMutation: true }), "full_generated_media_suite");
assert.equal(suiteSelection({ authorityMutation: false }),
  "targeted_immutable_unit_regression");

const siblingUnits = [
  { id: "g2", output: "g2/output", index: "g2/index", key: "g2-key" },
  { id: "g3", output: "g3/output", index: "g3/index", key: "g3-key" },
];
assert.equal(new Set(siblingUnits.map((unit) => unit.output)).size, 2);
assert.equal(new Set(siblingUnits.map((unit) => unit.index)).size, 2);
assert.equal(new Set(siblingUnits.map((unit) => unit.key)).size, 2);
assert.equal(audit.prerequisiteAuditId.startsWith("gmpreaudit1."), true);

function terminalComplete(value) {
  return value.recordsMaterialized && value.receiptsMaterialized
    && value.indexCasComplete && value.handoffMaterialized
    && (!value.evaluationPass || value.evaluationRecordMaterialized);
}
assert.equal(terminalComplete({ recordsMaterialized: true,
  receiptsMaterialized: true, indexCasComplete: true, handoffMaterialized: true,
  evaluationPass: true, evaluationRecordMaterialized: true }), true);
assert.equal(terminalComplete({ recordsMaterialized: true,
  receiptsMaterialized: true, indexCasComplete: true, handoffMaterialized: true,
  evaluationPass: true, evaluationRecordMaterialized: false }), false);

const telemetryKeys = ["schemaVersion", "policyKey", "pipelineRunId",
  "authorityAndTestingElapsedMs", "orchestrationWaitElapsedMs",
  "providerElapsedMs", "efficiencyWarnings"];
const telemetry = { schemaVersion: "generated_media_fast_path_efficiency_receipt_v1",
  policyKey: "generated_media_deterministic_fast_path_v1",
  pipelineRunId: "g2-g3-animation-run", authorityAndTestingElapsedMs: 100,
  orchestrationWaitElapsedMs: 20, providerElapsedMs: "unavailable",
  efficiencyWarnings: [] };
assert.deepEqual(Object.keys(telemetry).sort(), telemetryKeys.sort());

for (const surface of [policy, routingGuide, orchestrationPrompt, routingPrompt]) {
  assert.match(surface, /generated_media_deterministic_fast_path_v1/);
  assert.match(surface, /generated_media_fast_path_prerequisite_audit_v1/);
  assert.match(surface, /identity_equipment/);
  assert.match(surface, /geometry/);
  assert.match(surface, /carrier/);
  assert.match(surface, /fringe/);
  assert.match(surface, /phase_timing/);
  assert.match(surface, /full (?:task )?history/i);
  assert.match(surface, /Base64/i);
  assert.match(surface, /completed-PASS|completed PASS/i);
  assert.match(surface, /targeted regression/i);
  assert.match(surface, /generated_media_fast_path_efficiency_receipt_v1/);
}
for (const warning of ["token_heavy_full_history_ingestion_observed",
  "token_heavy_provider_base64_ingestion_observed",
  "token_heavy_full_payload_relay_observed",
  "token_heavy_unchanged_polling_observed",
  "token_heavy_unnecessary_full_suite_observed"]) assert.match(policy, new RegExp(warning));
assert.match(policy, /submitMax1/);
assert.match(policy, /retryCountMaximum=0/);
assert.match(policy, /no-clobber/);
assert.match(policy, /CAS/);

console.log({ policyKey: "generated_media_deterministic_fast_path_v1",
  prerequisiteAuditId: audit.prerequisiteAuditId,
  fullHistoryIngestionAllowed: false, providerBase64IngestionAllowed: false,
  providerCalled: false, submitCount: 0, cost: 0 });
console.log("generated media deterministic fast path vectors: PASS");
