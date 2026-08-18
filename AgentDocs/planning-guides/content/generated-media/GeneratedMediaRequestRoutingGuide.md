# Generated Media Request Routing Guide

## Purpose and Boundary

Guide Type: current v2 workflow and record contract. It validates one approved
planning handoff and creates one or more independent ImageGen authoring units.
It never authors prompts, calls providers, packages, evaluates, promotes, or
performs Git work.

The only additive exception is explicit
`executionMode=hosted_builtin_fast_preview_v1`, defined below. That mode owns a
bounded prompt/reference projection, one official generation-role call, and one
post-output visual preview observation without changing normal routing v2.

Legacy v1 routing is physically separated in
`GeneratedMediaLegacyV1CompatibilityGuide.md` and is not a current fallback.

## Authorities

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md
```

Planning owns facts, registry v2 owns exact rows, this guide owns fan-out and
routing identity, and the record guide owns canonical JSON/hash conventions.
Material conflicts block without precedence guessing.

## Input and Normalization

Require a readable `generated_media_planning_handoff_v2` and verify exact
source files, hashes, snapshot, request/content identity, required/prohibited
elements, and type specification. Canonical enum values are:

```text
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
provider: imagegen
```

Profile keys are exact registry values. Do not route by filename, prose,
similarity, provider availability, or legacy alias.

Role selection is fail-closed. An icon requires `assetType=icon_single_image`,
`domainType=skill|item`, iconProfile and icon single-image specification. A
background requires `assetType=background_single_image`,
`domainType=stage|battle|environment`, backgroundProfile and the complete
backgroundSpecification. If supplied evidence declares, conflicts with, or can
match both contracts, return `ambiguous_image_role`; do not choose by filename,
content prose or visual similarity.

## Deterministic Fan-out

- character/icon/background input creates exactly one routing unit;
- animation input reads `animationRequests` in source order;
- every unique animationRequestId creates a separate unit;
- each unit contains one scalar animationRequestId and one normalized
  `animationRequest` object;
- every new animation unit preserves
  `animationSourceMode=provider_native_animated_gif` and
  `extractionMode=gif_timeline_exact`; legacy fixed-cell sources are read-only
  and cannot create a new route;
- duplicate IDs, merged objects, arrays downstream, or added actions block;
- all units retain the same immutable common request/content/source/snapshot
  identity and record their source JSON pointer.

## Exact Match and Field Mapping

Evaluate every v2 registry row. Exactly one row per unit is required. Copy the
matched row's pipeline, authoring/generation prompt, structureProfile and
profile key. The authoring handoff includes the routing record path, planning
handoff path, exact type specification, evidence pointers, and for animation
the single animationRequestId/object.

For character single-image, the router validates an optional durable
`styleReferenceBindings` entry against the review asset/record/index and copies
the exact one-element array as a top-level member of the routing payload/record,
`normalizedRequest`, and `authoringHandoff`. It is never nested inside
`typeSpecification`, `identityConsistencyLock`, or `singleImageSpecification`.
The router does not copy media bytes, change `role=style_only`, or project
reference-subject semantics into required/prohibited elements.

When planning selected `generated_media_transparent_foreground_selection_v1`,
the router validates key/hash
`generated_media_true_alpha_foreground@1.0.0` /
`2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108`
and copies the exact object into routing payload/record, `normalizedRequest`,
and `authoringHandoff` as top-level `transparentForegroundSelection`. It is
forbidden inside `typeSpecification` and absent when unselected. Mixed locks or
field drift is `true_alpha_projection_mismatch`.

For a character attack-animation whose immutable reference prompt carries
`projectbs_character_bold_outline_compressed_detail@2.0.0`, direct inheritance
is illegal. The router may instead select
`projectbs_character_bold_outline_attack_motion_flow@1.0.0` only when the
reference bytes/hash and v2 payload/hash pass, its projection is exactly
18px/8px and no greater than 64/56/5 with unchanged color anchors and closed
halo, and approved planning supplies all eight successor motion bindings. A
non-attack unit returns `bold_outline_motion_flow_not_attack`; a missing motion
binding returns `missing_bold_outline_motion_flow_planning_bindings`; any base
key/hash/projection disagreement returns
`bold_outline_motion_successor_reference_mismatch`. These are no-route gates:
they publish no routing record and do not create or merge an animation unit.

Anchor mapping is deterministic:

```text
character_single_image -> pelvis_root_ground_axis
icon_single_image      -> visual_center
background_single_image -> scene_composition_anchor
animation/character    -> pelvis_root_ground_axis
animation/skill        -> effect_origin
```

Background field mapping copies sceneContract, composition, viewpoint,
horizon, ordered depthLayers, playableOrReadabilityArea,
subjectInclusions/subjectExclusions, canvas/aspectRatio, targetDisplay,
safeArea, finalBackgroundPolicy, consistencyLock and anchor without defaults.
Icon mapping never receives those scene fields.

## Hosted Fast Preview Orchestration

When authenticated user authority explicitly selects
`hosted_builtin_fast_preview_v1`, the routing role does not publish or rewrite a
routing record. It accepts only the compact pointer defined in the ImageGen-only
contract and resolves the existing authoritative planning/prompt/reference
anchors when available. It prepares executable prompt text and one reviewed
reference image in memory, calls the official generation role once, performs
one immediate visual preview observation, and returns one terminal receipt.

The three and only three pre-submit blocker classes are duplicate provider or
charge risk for the deterministic idempotency key, authority/safety violation,
and complete absence of executable prompt or reviewed reference input. Schema
or guide conflicts, lack of pre-preview Git publication, incomplete full-suite
validation, missing capability/cost attestation, and unavailable exact provider
options become `backlogWarnings`; they never authorize invented values and do
not block this mode.

The orchestration uses `submitCountMaximum=1`, `retryCountMaximum=0`, and one
child final followed by one parent relay. It sends only main SHA, request ID,
prompt record ID/hash, reference path/hash, idempotency key, and sealed callable
inputs to the generation child. It never retransmits full authority, planning,
routing, authoring, profile, or prompt payloads. The generation result is
observed and visually summarized in the same closed terminal receipt with
`previewOnly=true`, `notPromotable=true`, `notPreserved=true`, and
`strictEvaluationPerformed=false`. No observer receives the full relay.

This exception performs no strict evaluation package, preservation, promotion,
edit, regeneration, or Unity work. An explicit later user adoption is a new
strict-workflow request and cannot reuse the fast-preview receipt as a strict
input artifact.

## Repository Setup and Authority Orchestration v1

Repository setup is owned by one control-plane coordinator, not by planning,
routing, authoring, generation, preservation, or evaluation roles. This section
creates no planning/routing/prompt/generation/preservation/evaluation record and
does not change any provider or artifact contract.

### Repository-scoped setup mutex

For one canonical repository identity, at most one setup mutation may be active.
The mutex covers `worktree add`, `worktree remove`, `worktree prune`, and every
fetch that mutates remote-tracking/FETCH_HEAD state. Its deterministic key is:

```text
repositorySetupMutexKey=gmsetup1.{SHA256(UTF8(canonical origin remote URL))[0:20]}
```

Acquire the mutex before the first covered command and release it after success
or terminal failure. Read-only Git blob/object/status operations do not require
the mutex. Another setup request queues; it never runs a competing helper or
deletes a worktree to obtain the lock.

### One authority fetch and closed receipt

The coordinator performs exactly one successful `fetch origin main --prune`
per pipeline run while holding the mutex. It then resolves the fetched commit
and emits this response-only closed receipt:

```yaml
schemaVersion: generated_media_pipeline_authority_receipt_v1
repo: canonical origin remote URL
originMain: exact 40-lowercase-hex fetched commit
fetchedAt: exact observed RFC 3339 timestamp with offset
authorityReceiptSha256: lowercase SHA-256
```

`authorityReceiptSha256` is SHA-256 of RFC 8785 JCS UTF-8 bytes for the first
four members, excluding only itself. A pipeline run has exactly one receipt.
Downstream read-only roles validate its hash/repo/commit, detach or read the
exact `originMain` commit, and MUST NOT fetch again. A missing/invalid receipt,
repo mismatch, or unavailable commit blocks read-only reuse; it does not cause
each child to fetch independently.

Receipt reuse never weakens existing boundaries. A role that will mutate a
record/index or repository, publish Git state, or cross a provider boundary
must still perform every fresh check required by that boundary and acquire the
setup mutex for any covered repository command. The authority receipt is not a
provider capability, approval, cost, idempotency, publication, or CAS receipt.
Those fresh checks reuse the coordinator's exact authority anchor and do not
perform a second authority fetch. Interactive platform approval follows
`generated_media_noninteractive_execution_policy_v1`: zero routine prompts, or
one precomputed bundled prompt when the host requires it.

### Persistent serial worktrees and task lifecycle

Prefer four persistent, serial role worktrees per repository:

```text
planning
routing_authoring
generation
preservation_evaluation
```

Reuse the matching clean role worktree at the exact detached authority commit;
do not create a worktree per micro-stage. A queued client task ID is not an
official task. The coordinator waits until it obtains one distinct
`officialThreadId` and does not create a replacement while the queued/setup
state is pending. One bounded setup retry is permitted only after the original
state is positively confirmed `abandoned` or `failed`; the retry must receive a
new distinct officialThreadId before execution.

A failed `client-new-thread` setup that never receives an `officialThreadId` is
not addressable by `set_thread_archived` and may remain as an orphan UI card.
The coordinator MUST NOT create more client cards to retry it. Continue through
the persistent role worktree and the same official task when one exists; orphan
card cleanup is Codex app/platform responsibility, never repository worktree or
file deletion.

Evaluation of a sealed, hash-bound package runs in the configured evaluation
workspace outside the source Git worktree. It verifies package bytes/hashes and
MUST NOT fetch the source repository. The `preservation_evaluation` worktree may
prepare or validate the immutable package handoff, but visual evaluation reads
the sealed external package only.

No setup failure authorizes automatic cleanup. Archived, dirty, detached,
partially configured, or unpublished worktrees are inventory-only until exact
cleanup targets receive explicit user authorization. Setup reports one compact
status on state change and one terminal status; it never retransmits authority
bundles, planning payloads, or unchanged inventory.

Setup failures use exactly these tokens:

```text
worktree_metadata_permission_denied
task_registry_collision
helper_setup_refresh_failed
tool_approval_required
```

Permission/metadata denial uses the first token; duplicate client/official task
mapping uses the second; a helper that cannot refresh setup after the single
eligible retry uses the third; a blocked command requiring explicit tool/user
approval uses the fourth. None implies safe deletion, a second fetch, or a
replacement task.

## Routing Record v2

```yaml
schemaVersion: generated_media_routing_v2
routingRecordId:
routingPayloadSha256:
routerVersion: generated_media_router_v2
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId:
profileKey:
requestId:
assetType:
domainType:
contentId:
animationRequestId: required only for animation
planningHandoffPath:
planningSnapshotHash:
sourcePlanningFiles: []
requiredElements: []
prohibitedElements: []
typeSpecification:
styleReferenceBindings: conditionally present only for reviewed character style-only binding
transparentForegroundSelection: conditionally present only for exact selected true-alpha replacement projection
normalizedRequest:
selectedPipeline:
selectedAuthoringPrompt:
selectedGenerationPrompt:
provider: imagegen
structureProfile:
routingReason:
authoringHandoff:
supersedesRoutingRecordId: conditionally present
createdAt:
validation:
```

`animationRequestId` and `supersedesRoutingRecordId` are conditionally present,
never `null`; all other listed members are required. Unknown/missing fields are
rejected. Canonical paths:

```text
non-animation:
AgentDocs/planning-data/generated-media-routing/v2/{assetType}/{contentId}/{routingRecordId}.json

animation:
AgentDocs/planning-data/generated-media-routing/v2/animation/{contentId}/{animationRequestId}/{routingRecordId}.json
```

GeneratedMediaRecordGuide.md exclusively defines the closed
`routingHashPayload`, JCS bytes, full SHA-256, exact `gmroute2` formulas and
20-hex-character prefix, record/index paths, and the closed
`generated_media_routing_index_v2` key/value schema. The router must implement
that contract without adding or dropping a field.

Record identity includes immutable request/content/source/snapshot identity,
the complete selected type specification, normalized one-unit request, exact
registry row/pipeline/prompts, provider, structure profile, authoring handoff,
routing reason, optional animationRequestId, and optional accepted
supersedesRoutingRecordId. It excludes only derived ID/hash fields,
the copied planning timestamp and deterministic validation observations.

The durable style-only array in the four exact top-level projections is
hash-significant. Nesting it inside the type specification, omitting one
projection, or changing array/member order or value is
`style_reference_binding_projection_mismatch`. Missing review bytes, a
three-member style entry, profile mismatch, absolute path, or
person/identity/pose/action/clothing/equipment/edit-target transfer blocks
before routing publication with the exact central style-reference token.

Before writing, validate the entire existing record/index pair. Byte-identical
existing state is reused without changing timestamps or bytes. A valid orphan
record may receive its missing exact index entry. Divergent record bytes or an
80-bit prefix collision returns `routing_record_collision`; a divergent or
dangling index returns `routing_index_write_failed`. Supersession appends a new
immutable record and entry and never changes the old pair.

Publish the immutable record first with same-directory atomic no-clobber, reread
and hash it, then atomically replace the complete index. If record publication
fails, the index stays unchanged. If index publication fails after record
success, preserve the valid record as the only recoverable partial artifact,
return its path/hash with `routing_index_write_failed` and `safeToRetry=true`,
and attach the entry on retry without rewriting the record. All other blocked
inputs write no record, index, placeholder, failure artifact, or downstream
handoff.

## Detached Compact Routing Receipt

After the record and index have been reread and verified, return one detached
`generated_media_routing_receipt_v1`. It is control-plane metadata, is never a
record or index member, is not persisted, and does not change routing v2
identity. Its closed schema is:

```yaml
schemaVersion: generated_media_routing_receipt_v1
status: routed
reuseStatus: created | reused_identical
validatedAuthorityRevision: exact Git revision whose blobs were validated
routingRecordId:
routingRecordPath:
routingPayloadSha256:
routingRecordSha256:
indexPath:
indexSha256:
authorityBundleId:
authorityBundleSha256:
stageDeltaEnvelopeId:
stageDeltaEnvelopeSha256:
pipelineReceiptChainId:
pipelineReceiptChainSha256:
authoringHandoffPointer: /authoringHandoff
publicationState: local_unpublished | authoritative_git_blob
nextStep: git_publication | authoring
providerCalled: false
```

`local_unpublished` always has `nextStep=git_publication`; it must not trigger
authoring. `authoritative_git_blob` is allowed only after the exact record and
index hashes resolve at the reported authority revision and has
`nextStep=authoring`. The receipt never contains `normalizedRequest`,
`sourcePlanningFiles`, `requiredElements`, `prohibitedElements`,
`typeSpecification`, expression-profile payloads, or style-lock arrays. Those
values remain complete in the immutable routing record and its
`/authoringHandoff`; the consumer reads that exact Git blob after publication.

This receipt reduces repeated task-message payload but is not a validation
waiver. It cannot authorize a consumer to trust mutable checkout bytes, skip
the exact routing-record/index hash check, or reinterpret an old v2 record.

## Cross-stage Authority Bundle Receipt

Control-plane consumers exchange one response-only, non-persisted
`generated_media_authority_bundle_receipt_v1`. Construct its hash payload with
exactly these members:

```yaml
schemaVersion: generated_media_authority_bundle_hash_payload_v1
authoritativeMainSha: exact fetched origin/main Git object ID
requestedStageScope: non-empty ordered subset of planning | routing | authoring | generation | preservation | evaluation_package | preview_terminal
immutableArtifactAnchors:
  - role: stable lowercase_snake_case
    path: exact project-relative path
    sha256: exact Git-blob or immutable raw-file SHA-256
contractAuthorityAnchors: same closed item schema
profileAuthorityAnchors: same closed item schema
```

Within each anchor array, sort by UTF-8 path bytes and then role bytes. Duplicate
path/role pairs, unknown members, absolute paths, checkout-only roots and
mutable aliases are invalid. A contract or profile array may be empty only when
the requested stage scope requires no authority of that class; emptiness is
hash-significant and the validator never invents an anchor. Calculate:

```text
authorityBundleSha256 = SHA256(JCS(authorityBundleHashPayload))
authorityBundleId = gmauthbundle1.{authorityBundleSha256[0:20]}
```

The closed receipt contains the same five payload members with
`schemaVersion=generated_media_authority_bundle_receipt_v1`, plus exactly
`authorityBundleId` and `authorityBundleSha256`. It contains no timestamp,
thread ID, host path, status prose or mutable checkout metadata. Identical main
SHA, requested scope and anchor sets therefore produce byte-identical receipts.
Any missing receipt, main drift, scope change, path/hash/role change, added or
removed anchor, invalid receipt hash, or unavailable anchored blob requires a
full validation pass and a new bundle identity. A bundle never waives current
record/index or provider-boundary checks.

When `styleReferenceBindings` is present, `immutableArtifactAnchors` additionally
contains the exact durable style asset, review record, and review index as
separate `style_reference_asset`, `style_reference_review_record`, and
`style_reference_review_index` anchors. Their paths/hashes come from the
validated binding and review index, are sorted by the existing rule, and remain
in the authority bundle only. The compact routing receipt links them through
`authorityBundleId`/`authorityBundleSha256` without repeating the anchors.

## Cross-stage Delta Envelope

After one stage has a valid bundle and newly verified artifacts, construct one
response-only `generated_media_stage_delta_envelope_v1`. Its exact hash payload
is:

```yaml
schemaVersion: generated_media_stage_delta_hash_payload_v1
authorityBundleId:
authorityBundleSha256:
fromStage: planning | routing | authoring | generation | preservation | evaluation_package
toStage: routing | authoring | generation | preservation | evaluation_package | preview_terminal | terminal
unitIdentity:
  requestId:
  assetType:
  domainType:
  contentId:
  animationRequestId?: required only for animation
newArtifacts:
  - role:
    path:
    sha256:
priorValidationReceiptRefs:
  - stage:
    receiptId:
    receiptSha256:
priorPipelineReceiptChain?: absent only for the first envelope
  pipelineReceiptChainId:
  pipelineReceiptChainSha256:
publicationState: local_unpublished | authoritative_git_blob
nextStep: git_publication | routing | authoring | generation | preservation | evaluation_package | preview_terminal | terminal
providerState:
  state: not_called | called | completed | failed
  providerCalled: boolean
  submitCount: non-negative integer
relayPolicy: child_final_once_parent_next_role_once
observerPolicy: compact_terminal_receipt_only
```

Sort `newArtifacts` by role then path; keep validation receipts in pipeline
order. Hash and identify it exactly as:

```text
stageDeltaEnvelopeSha256 = SHA256(JCS(stageDeltaHashPayload))
stageDeltaEnvelopeId = gmdelta1.{fromStage}.{toStage}.{stageDeltaEnvelopeSha256[0:20]}
```

The closed envelope replaces the payload schemaVersion with
`generated_media_stage_delta_envelope_v1` and adds only the ID and full hash.
The permitted transitions are planning->routing, routing->authoring,
authoring->generation, generation->preservation,
generation->preview_terminal, preservation->evaluation_package and
evaluation_package->terminal. `local_unpublished` always requires
`nextStep=git_publication`; `authoritative_git_blob` requires `nextStep` to
equal `toStage`. A publication change creates a new envelope identity.
`providerState=not_called` requires `providerCalled=false` and `submitCount=0`;
the other states require `providerCalled=true` and `submitCount>=1`.

The envelope recursively forbids `normalizedRequest`, `sourcePlanningFiles`,
`requiredElements`, `prohibitedElements`, `typeSpecification`,
`planningSnapshot.approvedFacts`, expression-profile payloads, style-lock
arrays, scene/provider prompt bodies, nested authoring/generation/preservation
handoffs, media bytes, and full record/index objects. Consumers read these from
the anchored Git blobs. A forbidden field, invalid transition, invalid
publication pair, bad prior receipt or hash mismatch emits no success envelope
and changes no immutable artifact.

## Relay, Status, and Pipeline Receipt Chain

One child sends exactly one final delta envelope to its parent. After validating
it, the parent relays that exact envelope exactly once to the next execution
role. The parent does not broadcast the full envelope to requester, planning
owner, Git owner or observers. Each observer receives at most one terminal
`generated_media_compact_status_v1` for the stage.

The compact status hash payload contains exactly
`schemaVersion=generated_media_compact_status_hash_payload_v1`,
`pipelineReceiptChainId`, `pipelineReceiptChainSha256`, `stage`, `state`,
`stageReceiptId`, `stageReceiptSha256`, `publicationState`, the same closed
`providerState`, `approvalRequestsCount`, and `bundledApprovalUsed`. The last
two members obey GeneratedMediaNoninteractiveExecutionPolicyGuide.md and count
only interactive platform approval requests. Its ID/hash use
`gmstatus1.{stage}.{hash[0:20]}` and full
SHA-256 over JCS. The receipt replaces schemaVersion with
`generated_media_compact_status_v1` and adds the ID/full hash. Emit only on a
new canonical state hash or once for terminal state. An unchanged status,
including repeated `providerCalled=false`, is rejected as a duplicate and is
not relayed or commented again. Commentary outside this object is limited to a
short blocking action required from the user.

Pipeline lineage is the response-only deterministic
`generated_media_pipeline_receipt_chain_v1`, never a mutable state record. Its
hash payload contains exactly:

```yaml
schemaVersion: generated_media_pipeline_receipt_chain_hash_payload_v1
authorityBundleId:
authorityBundleSha256:
unitIdentity: exact closed unit object above
stageEnvelopeRefs:
  - stageDeltaEnvelopeId:
    stageDeltaEnvelopeSha256:
```

Envelope refs preserve pipeline order and IDs are unique. Calculate full JCS
SHA-256 and `pipelineReceiptChainId=gmpipechain1.{hash[0:20]}`; the receipt
replaces schemaVersion and adds ID/full hash. Each stage returns a new value
with one ref appended; it never updates an older value. No orchestration path,
file, index, lock, latest pointer or CAS operation exists. Lineage is therefore
hash-linked without creating another mutable publication surface, collision
target or index race. Immutable stage records/indexes remain the only persisted
workflow evidence.

## Terminal Evaluation-to-Project-Promotion Dispatch v1

This response-only orchestration rule does not change routing v2 records,
indexes, stage-delta schemas, evaluation artifacts, or promotion records. After
one sealed preservation package has been evaluated, the coordinator may dispatch
the existing persistent project-promotion role exactly once:

```text
officialThreadId: 01a01094-7d22-7a51-b92e-bf6154769017
title: [Generated Media] 프로젝트 승격 및 Unity 복사
prompt: AgentDocs/task-prompts/content/GeneratedImageProjectPromotionPrompt.md
guide: AgentDocs/planning-guides/content/GeneratedImageProjectPromotionGuide.md
```

Dispatch is eligible only when every predicate is exact and current:

```text
preservation package: present, sealed, hash-bound generated_media_evaluation_package_v2
evaluation schema: generated_image_evaluation_v1
evaluationStatus: completed
result: PASS
passForProjectCopy: true
promotionStatus: not_promoted
package route: one exact current package-mode promotion registry row
```

A preview, `notEvaluated`, incomplete evaluation, non-`PASS`, `Conditional
Pass`, `Fail`, false/missing `passForProjectCopy`, missing preservation package,
or any promotion status other than `not_promoted` MUST NOT dispatch. There is no
promotion before evaluation completion.

The relay is a closed object with exactly these eight members and no others:

```yaml
requestId:
evaluationPackageId:
assetType:
domainType:
contentId:
evaluationRecordId:
replaceExisting: false | true
replacementApprovalRef: null | non-path approval reference
```

All six identity strings are non-empty and must match the sealed package and
completed evaluation result. `replacementApprovalRef` is `null` when
`replaceExisting=false` and a non-empty, non-path reference when true. Absolute
or relative source/target paths, full authority bundles, manifests, prompt or
provider payloads, media bytes, and unknown/nested fields are forbidden.

For exactly-once behavior, compute an internal response-only dispatch key as
`SHA256(JCS(the exact eight-member relay))` and inspect the persistent official
task history before calling it. An identical active or completed relay reuses
that terminal result and MUST NOT call the role again. The key is not added to
the relay and creates no repository record, index, or path.

The promotion child returns one final result and the coordinator relays it once.
Its terminal status is exactly `promoted`, `blocked`, `not_promoted`, or
`copy_failed`. Every result is terminal: no route returns to routing, generation,
preservation, or evaluation. Missing/invalid upstream evidence terminates as
`blocked` or `not_promoted` without dispatch; only the promotion role can return
`promoted` or `copy_failed` after an eligible dispatch.

## Validation Receipt Reuse Matrix

| Boundary or check | May reuse an exact receipt | Mandatory fresh work |
| --- | --- | --- |
| authority main and anchored source/profile/contract blobs | read-only roles reuse the exact pipeline authority receipt, detached commit and byte-identical bundle ID/hash | coordinator fetches once per pipeline run; mutation/publication/provider boundaries retain their existing fresh checks; invalid receipt blocks rather than child refetch |
| planning RFC 6901 facts and JCS snapshot | yes when the exact planning handoff/blob and bundle are unchanged | full resolution when planning handoff/hash or planning contract anchor changes |
| registry row and expression-profile schema/hash | yes when exact registry/profile authority anchors and selected artifact are unchanged | full match/projection on any key, hash, scope or authority change |
| stage input record/handoff raw hash and exact projection | no waiver | always verify at every consumer boundary |
| new record/index identity, no-clobber and CAS preimage | no | always verify for every mutation/publication attempt |
| authoritative publication state | no | always resolve reported record/index hashes at the reported Git revision before next role |
| provider approval, capability, settings, cost, attempt limit and idempotency | no | always rerun immediately before every provider submit boundary |
| new media bytes, preservation and evaluation evidence | no | always verify for each new output or downstream mutation |

Receipt reuse suppresses only unchanged exact-source/profile/schema work. It
never suppresses a mutation, freshness, consumer artifact, provider or media
boundary. Drift or a missing receipt always fails closed into the full
validation path.

## State, Failure, and Output

```text
received -> validated -> fanned_out -> matched -> routed
received/validated/fanned_out/matched -> blocked
```

Use the common/type and Router Extension registries in
GeneratedMediaImageGenOnlyContractGuide.md. The router extension is:

```text
duplicate_animation_request_id
ambiguous_image_role
unsupported_icon_domain
unsupported_background_domain
unsupported_current_route
conflicting_routing_evidence
style_reference_binding_projection_mismatch
routing_record_collision
routing_record_write_failed
routing_index_write_failed
```

Success returns only the detached compact receipt above. Complete selected
row/profile/pipeline/prompt, normalized request, snapshot, reason and authoring
handoff remain in the exact record; they are not echoed into the task message.
The routing receipt binds the exact authority bundle, stage delta and pipeline
chain IDs/hashes rather than competing with them.
Failure returns `status=blocked`, failureType, missing/conflicting fields,
candidate rows, required decision and safeToRetry.

## Validation

- no current row or output contains a PixelLab route;
- no current character contract contains eight-way/rotation output;
- every animation unit contains exactly one animationRequestId;
- version/path/schema are routing v2 and directory v2;
- payload hash/ID use the full SHA-256 and exact 20-character prefix contract;
- routing index is `generated_media_routing_index_v2` and every entry exactly
  projects a present canonical record;
- identical retries preserve exact record/index bytes, while any divergent
  occupied identity fails closed;
- authoring handoff fields exactly match selected prompt inputs;
- optional character styleReferenceBindings exactly match the planning
  projection and GeneratedMediaStyleReferenceBindingGuide review bytes in all
  four top-level routing projections and are absent from typeSpecification;
- routing index entries contain no style binding body and bind it only through
  exact routingPayloadSha256/recordSha256;
- the detached success receipt is closed, hash-bound, and contains none of the
  persisted bulk authoring fields or style-lock arrays;
- identical authority anchors/scope reproduce one bundle and receipt, while
  one anchor change forces a new hash and full validation;
- stage transitions/publication pairs are closed and duplicate relay/status
  hashes are rejected;
- the pipeline chain has no persisted orchestration record/index/path;
- no downstream stage executes.
