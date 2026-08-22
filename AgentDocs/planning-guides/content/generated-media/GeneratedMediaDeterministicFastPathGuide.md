# Generated Media Deterministic Fast Path Guide

## 1. Policy identity and scope

`generated_media_deterministic_fast_path_v1` is the normative orchestration
policy for an already bounded Generated Media pipeline run. It composes, and
does not replace, the current authority-receipt, stage-delta, noninteractive,
record, no-clobber, CAS, provider-idempotency, preservation, evaluation, and
promotion contracts. Existing artifact schemas and profile payload bytes are
unchanged.

The policy reduces control-plane repetition only. It never weakens a stage
gate, crosses a role boundary, creates authority, changes `submitMax1`, or
changes `retryCountMaximum=0`.

## 2. Incremental observation and compact relay

The coordinator observes a child with incremental wait/status calls from the
last delivered cursor. It must not ingest full task history, replay all prior
messages, ingest provider Base64 output, or poll an unchanged status. Provider
binary output remains at its hash-bound path and is opened only by the role
that owns the required visual or byte validation.

Cross-stage messages use registered contract/profile keys and exact compact
hash/ID/path pointers. They reuse `generated_media_pipeline_authority_receipt_v1`,
`generated_media_authority_bundle_receipt_v1`,
`generated_media_stage_delta_envelope_v1`, and
`generated_media_compact_status_v1`; they do not repeat planning facts, prompt
prose, provider payloads, schemas, profile bodies, record bodies, media bytes,
or full authority bundles. Commentary is emitted only for a new canonical
state hash, a typed blocker requiring action, or terminal completion.

## 3. Atomic prerequisite audit

Before the first artifact write for a run, the coordinator completes one
response-only `generated_media_fast_path_prerequisite_audit_v1`. Its hash
payload has exactly these members:

```yaml
schemaVersion: generated_media_fast_path_prerequisite_audit_hash_payload_v1
policyKey: generated_media_deterministic_fast_path_v1
pipelineRunId: non-empty stable run identity
authorityReceiptId: exact validated coordinator receipt identity
authorityReceiptSha256: exact receipt SHA-256
checks:
  liveAuthority: pass | fail | not_applicable
  profileScope: pass | fail | not_applicable
  routeStructure: pass | fail | not_applicable
  evaluationRecord: pass | fail | not_applicable
  lineageRecord: pass | fail | not_applicable
  trustedReference: pass | fail | not_applicable
  destinationIndexCas: pass | fail | not_applicable
```

`not_applicable` is allowed only when the requested bounded route has no such
input or destination. Calculate
`prerequisiteAuditSha256=SHA256(JCS(payload))` and
`prerequisiteAuditId=gmpreaudit1.{prerequisiteAuditSha256[0:20]}`. The receipt
replaces the schema version with
`generated_media_fast_path_prerequisite_audit_v1` and adds only those two
identity members. Unknown or nested members are invalid.

Every applicable check must be `pass` before any record, index, package,
evaluation, or promotion artifact write. Audit failure produces the existing
owning-stage token, no partial artifact, and no speculative repair. A stage
still revalidates its input projection, write target, no-clobber, CAS, provider,
or media boundary as required by its current contract.

## 4. Orthogonal failure domains and deterministic recovery

The closed `generated_media_failure_domain_v1` values are:

```text
identity_equipment
geometry
carrier
fringe
phase_timing
```

Evidence is classified without replacing the owning contract's exact failure
token. `identity_equipment` covers character identity, required equipment,
handedness, or cultural substitution. `geometry` covers fit, bounds, scale,
anchor, baseline, clipping, or canvas placement. `carrier` covers opaque
background/key topology and alpha extraction. `fringe` covers residual carrier
or chromatic edge contamination. `phase_timing` covers ordered motion phases,
duplicates, closure, frame timing, or loop semantics.

When identity/equipment is verified PASS and every failure domain is a non-empty
subset of `geometry | carrier | fringe`, the coordinator must select an exact
registered source-bound deterministic postprocess route if one matches the
source/receipt/profile hashes. It must not request fresh generation. The helper
may perform only its registered transform and must preserve the immutable
source. Missing exact registration fails closed. An `identity_equipment` or
`phase_timing` failure is never silently relabeled as a carrier/fit/fringe
failure and never implies an automatic provider retry.

## 5. Terminal evidence completeness

A stage may report terminal success only after every contract-required record,
receipt, index/CAS projection, and handoff is materialized and reopened at its
reported hash. A completed PASS evaluation additionally requires its immutable
evaluation record to exist at the reported path and SHA. Chat text, commentary,
an in-memory score, or a local observation is not a substitute. Missing terminal
evidence keeps the stage non-terminal with its existing typed failure token.

## 6. Test selection and multi-unit scheduling

A contract, schema, registry, helper, serializer, or policy mutation requires
the full current Generated Media `.mjs` suite in raw-LF authority materialization
plus the focused new vector. An immutable execution unit whose contract/helper
blobs and input hashes are unchanged runs only its owning targeted regression
and required reopen/hash/conformance checks; it must not rerun the full contract
suite merely because it moved to the next stage.

G2/G3 or other sibling units sharing the same authority/profile/route family
perform one shared prerequisite audit for identical anchors and keep separate
unit identities and input hashes. After the shared audit passes, downstream
work may run independently when output paths, record paths, indexes/CAS targets,
idempotency keys, provider limits, and role worktrees do not overlap. Any shared
mutable target serializes only the affected boundary.

## 7. Timing and efficiency telemetry

Provider elapsed time is measured separately from authority/testing and
orchestration wait time. Once, at terminal state, the coordinator may return
the response-only telemetry object below; it is not artifact authority and is
not included in artifact identities:

```yaml
schemaVersion: generated_media_fast_path_efficiency_receipt_v1
policyKey: generated_media_deterministic_fast_path_v1
pipelineRunId:
authorityAndTestingElapsedMs: non-negative integer
orchestrationWaitElapsedMs: non-negative integer
providerElapsedMs: non-negative integer | unavailable
efficiencyWarnings: ordered unique subset of closed warning tokens
```

Closed warning tokens are
`token_heavy_full_history_ingestion_observed`,
`token_heavy_provider_base64_ingestion_observed`,
`token_heavy_full_payload_relay_observed`,
`token_heavy_unchanged_polling_observed`, and
`token_heavy_unnecessary_full_suite_observed`. Compliant new runs normally use
an empty list. Do not estimate token counts or provider time. `unavailable` is
used when provider timing was not observed.

## 8. Coordinator checklist

1. Hold the repository setup mutex and reuse the single live authority receipt.
2. Complete the atomic prerequisite audit before artifact writes.
3. Relay contract keys and hash/ID deltas, never full history or Base64.
4. Preserve role ownership, no-clobber/CAS, submit maximum one, and retry zero.
5. Route identity-PASS carrier/fit/fringe failures to an exact registered helper.
6. Persist and reopen every terminal record/receipt before terminal relay.
7. Run the full suite only for authority code/schema/helper changes.
8. Share immutable sibling preflight, then parallelize only disjoint units.
9. Emit commentary only on state change or terminal completion.
10. Report provider time separately and flag observed token-heavy operations.
