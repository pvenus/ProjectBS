# Generated Media Request Routing Guide

## Purpose and Boundary

Guide Type: current v2 workflow and record contract. It validates one approved
planning handoff and creates one or more independent ImageGen authoring units.
It never authors prompts, calls providers, packages, evaluates, promotes, or
performs Git work.

Legacy v1 routing is physically separated in
`GeneratedMediaLegacyV1CompatibilityGuide.md` and is not a current fallback.

## Authorities

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
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
`stageReceiptId`, `stageReceiptSha256`, `publicationState`, and the same closed
`providerState`. Its ID/hash use `gmstatus1.{stage}.{hash[0:20]}` and full
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

## Validation Receipt Reuse Matrix

| Boundary or check | May reuse an exact receipt | Mandatory fresh work |
| --- | --- | --- |
| authority main and anchored source/profile/contract blobs | yes, only with byte-identical bundle ID/hash and unchanged requested scope | fetch and full validation on missing/invalid receipt, main/scope/anchor drift or unavailable blob |
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
