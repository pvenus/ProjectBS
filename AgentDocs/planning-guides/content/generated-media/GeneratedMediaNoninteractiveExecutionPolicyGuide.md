# Generated Media Noninteractive Execution Policy Guide

## Purpose and version

`generated_media_noninteractive_execution_policy_v1` is the closed execution-
authority policy for one exact Generated Media pipeline run. It governs
interactive platform/tool approval prompts; it does not weaken artifact
schemas, provider idempotency, immutable hashes, no-clobber rules, evaluation
gates, promotion routing, or replacement safety.

Planning, routing, prompt authoring, character/icon/background generation,
animation generation, preservation/package creation, evaluation, protected Git
artifact publication, and terminal project promotion inherit this policy. One
official role task owns each stage in a persistent serial role worktree. A role
must not fork a child merely to read, inspect, hash, validate, or evaluate.

## Closed policy

The coordinator derives exactly one policy from the authenticated user's exact
request before stage work begins:

```yaml
schemaVersion: generated_media_noninteractive_execution_policy_v1
pipelineRunId: non-empty stable run identity
authorityRequestRef: non-path authenticated request reference
declaredStages: ordered unique subset of planning | routing | prompt_authoring | character_image_generation | animation_generation | preservation_packaging | evaluation | git_publication | project_promotion
declaredWorkspaceRoots: non-empty ordered canonical workspace/project roots
providerSubmitMaximum: non-negative integer
providerRetryMaximum: non-negative integer
replaceExistingAuthorized: boolean
destructiveDeleteAuthorized: boolean
platformApprovalMode: not_required | bundled_required
policyPayloadSha256: lowercase SHA-256
```

`policyPayloadSha256` is SHA-256 of RFC 8785 JCS UTF-8 bytes of the preceding
ten members, excluding itself. Unknown, missing, duplicate-stage, reordered,
or out-of-scope values are invalid. `providerSubmitMaximum` and
`providerRetryMaximum` must equal the authenticated request; neither is a
default. `replaceExistingAuthorized` or `destructiveDeleteAuthorized` may be
true only when that exact new authority is present.

## Standing authority and zero-prompt execution

The authenticated request for the exact pipeline is standing authority. When
an action stays inside the policy and declared roots, roles execute without a
second interactive approval request:

- local/project and exact Git-blob reads, plus image/GIF inspection;
- SHA-256, JCS, schema, byte, LF/no-BOM, and test validation, including
  temporary LF-exact materialization;
- the coordinator's one live authority fetch at the mutation boundary and
  downstream reuse of its immutable authority receipt and exact detached
  commit;
- bounded prompt, record, index, package, and evaluation artifact creation
  with existing immutable/no-clobber/CAS rules;
- exactly the user-authorized provider submit count and retry limit;
- accepted-output preservation and a completed PASS evaluation record;
- project copy through an exact registered route when the canonical target is
  absent and every package/hash/PASS gate succeeds; and
- bounded protected Git publication of the exact declared artifacts when that
  publication is a normal requested pipeline step.

Standing authority supplies execution authorization, not fabricated evidence.
Capability, settings, cost, approval-scope, provider return, preservation,
evaluation, publication, and copy receipts remain truthful and hash-bound.
Unavailable evidence remains unavailable; the policy never converts it to an
attestation. Existing strict provider execution approval records may be
deterministically projected from the exact request/policy and final sealed
scope without asking the user again.

## One bundled platform approval

If the host/platform cannot perform declared work without an interactive tool
approval, the coordinator computes one closed request before any covered work:

```yaml
schemaVersion: generated_media_bundled_platform_approval_request_v1
pipelineRunId: exact policy pipelineRunId
policyPayloadSha256: exact policy hash
actions: ordered unique concrete platform action classes
commands: ordered exact commands, empty only when no shell command is needed
roots: exact ordered roots affected by actions/commands
requestedAt: RFC 3339 timestamp with offset
requestPayloadSha256: lowercase SHA-256
```

`requestPayloadSha256` is the RFC 8785 JCS UTF-8 SHA-256 of the preceding seven
members. `actions`, `commands`, and `roots` are complete for the bounded run;
wildcard scope and undeclared roots are forbidden. The coordinator requests
this bundle once. After it is granted, every covered stage reuses it and MUST
NOT ask again. A denied, missing, partial, expired, or scope-mismatched bundle
terminates once with `generated_media_bundled_platform_approval_unavailable`;
no partial stage mutation or second approval prompt is allowed.

## New-authority boundary

Interactive approval remains mandatory only for authority not already present
in the exact request/policy:

```text
existing_project_content_replace_not_authorized
destructive_delete_not_authorized
provider_submit_or_retry_limit_exceeded
credential_or_elevation_required
write_root_outside_declared_scope
material_scope_expansion_required
```

An existing target may be replaced only with the existing exact replacement
approval contract. A role does not reinterpret standing authority as overwrite,
delete, credential/elevation, extra spend/submit/retry, new root, or broader
scope permission. It returns the one exact blocker without issuing a routine
approval prompt or doing partial work.

## Coordinator, worktrees, and command boundary

The repository setup coordinator owns one setup mutex, one live authority
fetch, and one `generated_media_pipeline_authority_receipt_v1` per run.
Downstream roles validate and reuse the exact receipt/commit and do not fetch
again. Mutation, provider, publication, and copy boundaries still perform their
existing hash/CAS/idempotency/safety checks against those anchors; "fresh" does
not mean another authority fetch.

Use persistent serial worktrees for planning, routing+authoring, generation,
and preservation+evaluation. A sealed external evaluation package is evaluated
outside the source Git worktree and never fetches that repository. When shell
work is required, each stage batches its routine file/shell operations into one
deterministic helper or command boundary. Archived, dirty, unpublished, and
orphan task/worktree state remains inventory-only without exact cleanup
authority.

## Compact status projection

Every `generated_media_compact_status_v1` and its hash payload add exactly:

```yaml
approvalRequestsCount: 0 | 1
bundledApprovalUsed: boolean
```

`approvalRequestsCount=0` requires `bundledApprovalUsed=false`.
`approvalRequestsCount=1` requires `platformApprovalMode=bundled_required`.
It uses `bundledApprovalUsed=true` after grant/reuse; false is allowed only on
the terminal `generated_media_bundled_platform_approval_unavailable` blocker.
A ready/success status with count 1 and false, or any count above one, is
invalid. These fields count
interactive platform approval requests only, not immutable replacement or
provider-approval evidence. Compact status still omits full authority bundles.

## Required regression behavior

For `builtin_imagegen_authenticated_single_submit_v1`, the authenticated exact
opaque-chroma generation request is standing execution authority only after
the generation role closes the scope and projects
`generated_media_builtin_imagegen_authenticated_approval_v1`. This is a
routine in-scope one-submit/zero-retry action and adds no interactive approval
request. It does not authorize another submit, a retry, fabricated
capability/settings/cost evidence, postprocess, preservation, evaluation,
promotion, replacement, or scope expansion.

- an eligible new-output run completes with zero interactive approvals;
- a host-required run emits exactly one complete bundled approval and no
  second request after grant;
- overwrite, destructive delete, additional submit/retry/cost, credentials or
  elevation, out-of-root writes, and scope expansion remain blocked;
- provider idempotency, exact hashes, immutable records, no-clobber/CAS,
  completed PASS, registered route, and replacement approval gates remain
  unchanged.
