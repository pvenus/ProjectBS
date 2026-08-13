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

Authority by concern:

1. `GeneratedMediaPlanningHandoffGuide.md` owns the immutable planning schema,
   approved facts, and type-specific specifications.
2. `GeneratedMediaAuthoringProfileRegistryGuide.md` version
   `generated_media_authoring_profile_registry_v1` owns every exact supported
   asset/domain/profile pair and route target.
3. The four provider pipeline guides own behavior behind those registered rows.
4. `GeneratedMediaRecordGuide.md` owns canonical JSON, hashing, path separators,
   unknown-field rejection, and immutable record conventions.
5. This guide owns routing normalization, registry matching, routing record,
   idempotency, and authoring handoff mapping.

If independent authoritative non-alias sources provide incompatible complete
route tuples, return `conflicting_routing_evidence`. Alias/canonical mismatch
uses `compatibility_alias_conflict` instead. Do not merge or prefer a convenient
value.

## 3. Input Contract

The router consumes one readable canonical or allowed compatibility handoff.
Capture it unchanged as `rawInput`. For canonical
`generated_media_planning_handoff_v1`, require the actual raw fields owned by
`GeneratedMediaPlanningHandoffGuide.md`:

```yaml
planningHandoffFile: exact project-relative path
requestId: stable request identity
assetType: canonical supported enum
domainType: canonical domain enum
contentId: canonical content identity
sourcePlanningFiles: non-empty exact paths, roles, hashes, revisions
planningSnapshot:
  capturedAt: UTC timestamp
  snapshotHash: immutable SHA-256
  approvedFacts: immutable approved planning facts
requiredElements: non-empty independently observable list
prohibitedElements: non-empty independently observable list, or signed/hashed no_prohibitions
contentUsage: non-empty intended generated-media use
projectTarget: optional informational-only destination
```

Do not reject canonical input for lacking normalized names. If schemaVersion is
`generated_media_planning_handoff_compat_v1`, apply only the compatibility
envelope declared by `GeneratedMediaPlanningHandoffGuide.md` before required-
field or unknown-field validation. The result is a
`compatibilityNormalizedInput` wrapper containing an unchanged-shape
`canonicalHandoff` plus separate `compatibilityEvidence`.

Routing then derives `planningRevision` and `outputUsage` only by these rules:

- canonical `contentUsage` maps to normalized `outputUsage`;
- a compatibility top-level `planningRevision` is read only from
  `compatibilityEvidence`, must be snapshot-covered, and must apply identically
  to every source without rewriting source entries;
- otherwise, every canonical `sourcePlanningFiles` entry must contain the same
  non-empty `revision`, which becomes normalized `planningRevision`;
- missing, mixed, or unhashed revision evidence blocks.

An unhashed side input cannot supply either value.

Canonical raw type fields and their normalized containers are:

| assetType | Canonical raw required fields | normalized container |
| --- | --- |
| `character_main_image` | `characterIdentity`, `appearanceSpecification`, exact `rotationContract` | `characterSpecification` |
| `character_animation` | `characterProviderIdentity`, applicable package evidence, non-empty `animationRequests` | `characterSpecification` plus `animationRequests` |
| `icon` | `iconProfile`, `subjectIdentity`, `semanticEffect`, optional exact counts, background/display contracts | `iconSpecification` plus `iconProfile` |
| `general_animation` | `animationProfile`, subject, sequence, loop, frame, runtime boundary, reference contract | `animationSpecification` |
| `imagegen_image` | `imageProfile`, depicted moment, subjects, environment, composition, camera, aspect/background | `imageSpecification` plus `imageProfile` |

Compatibility containers are resolved to the same canonical raw fields first.
Only then are normalized containers assembled. Missing raw members cannot be
derived.

## 4. Canonical Normalization

Use this mandatory stage order:

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
schemaVersion: generated_media_authoring_request_v1
planningHandoffFile:
requestId:
assetType:
domainType:
legacyArtifactType: optional
contentId:
sourcePlanningFiles:
planningRevision:
planningSnapshotHash:
requiredElements:
prohibitedElements:
outputUsage:
appliedProfile:
typeSpecification:
projectTarget: optional informational_only
```

Type-specification compatibility mapping:

| canonical container | Current handoff fields mapped without rewriting |
| --- | --- |
| `characterSpecification` for main image | `characterIdentity`, `appearanceSpecification`, `rotationContract` |
| `characterSpecification` for animation | `characterProviderIdentity`, optional `approvedCharacterPackageId`, plus exact `animationRequests` |
| `iconSpecification` | `subjectIdentity`, `semanticEffect`, optional `exactCountElements`, `backgroundPolicy`, `targetDisplayContract`; `iconProfile` remains a sibling profile key |
| `animationSpecification` | `animationProfile`, `animationSubject`, `sequenceStages`, `loopMode`, `frameContract`, `runtimeBoundary`, `referenceImageContract` |
| `imageSpecification` | `depictedMoment`, `subjects`, `environment`, `composition`, `camera`, `aspectRatio`, `backgroundPolicy`; `imageProfile` remains a sibling profile key |

If both a container and its flattened compatibility fields exist, every mapped
value must be canonically equal. This comparison occurs during compatibility
normalization, before required-field and unknown-field validation. Any mismatch
returns `compatibility_alias_conflict`.

The router does not rewrite the planning handoff. It maps fields for the next
prompt as follows:

| selected authoring prompt | Field-level mapping |
| --- | --- |
| PixelLab Character | `planningHandoffFile`; `runType=assetType`; main uses `characterIdentity`, `appearanceSpecification`, `rotationContract`; animation carries `characterProviderIdentity`, `approvedCharacterPackageId` when required, and one authoring unit per exact `animationRequestId` |
| PixelLab Icon | `planningHandoffFile`; `domainType`; `iconProfile`; `subjectIdentity`; `semanticEffect`; `exactCountElements`; `backgroundPolicy`; `targetDisplayContract` |
| PixelLab Animation | `planningHandoffFile`; `domainType`; `animationProfile`; `animationSubject`; `sequenceStages`; `loopMode`; `frameContract`; `runtimeBoundary`; `referenceImageContract` |
| ImageGen | `planningHandoffFile`; `domainType`; `imageProfile`; `depictedMoment`; `subjects`; `environment`; `composition`; `camera`; `aspectRatio`; `backgroundPolicy` |

All rows also carry common identity, verified sources/snapshot, required and
prohibited elements, and output usage. No mapped value may originate outside
the immutable handoff.

`authoringHandoff` is directly executable by the selected existing prompt and
contains no invented prompt fields:

```yaml
authoringHandoff:
  selectedAuthoringPrompt:
  promptInput:
    routingRecordFile:
    planningHandoffFile:
    runType: character_main_image | character_animation  # Character only
    animationRequestId: exact ID                          # animation unit only
  evidenceMap:
    normalized field: exact planning handoff JSON pointer(s)
```

Every authoring prompt receives both `routingRecordFile` and
`planningHandoffFile` as required inputs. Icon, general-animation, and
ImageGen resolve mapped fields from the immutable planning handoff. Character
main also receives `runType`.
Character animation creates one handoff unit with `runType` and
`animationRequestId` for each exact request. Revision inputs remain absent
unless a separate revision request supplies them.

The authoring task verifies `routingRecordId`, `registryVersion`,
`selectedRegistryRowId`, `selectedPipeline`, `selectedAuthoringPrompt`,
`appliedProfile`, `normalizedRequest`, and `planningSnapshotHash` from the
routing record. Its request/content identity and planning hash must match the
named planning handoff. It validates the router-selected row against the pinned
registry and must not run registry selection again. Missing, stale, mismatched,
or ambiguous routing evidence blocks authoring.

For `character_animation`, `normalizedRequest.authoringUnits` contains one item
for each supplied `animationRequestId` in source order. Each unit selects the
same Character pipeline and prompt but produces one downstream prompt record.
This is one routed pipeline, not multi-pipeline selection. The router does not
invoke those units.

## 9. Routing Record Identity and Storage

Use the canonical JSON rules from `GeneratedMediaRecordGuide.md`.

```yaml
routingHashPayload:
  schemaVersion: generated_media_routing_hash_payload_v1
  routerVersion: generated_media_authoring_router_v1
  registryVersion: generated_media_authoring_profile_registry_v1
  selectedRegistryRowId:
  rawInputSha256:
  compatibilityNormalizedInputHash:
  requestId:
  assetType:
  domainType:
  contentId:
  planningRevision:
  planningSnapshotHash:
  appliedProfile:
  selectedPipeline:
  selectedAuthoringPrompt:
  supersedesRoutingRecordId: optional
```

```text
routing_hash = SHA256(canonical_json(routingHashPayload))
routingRecordId = gmroute.{assetType}.{contentId}.{routing_hash_prefix_12}

AgentDocs/planning-data/generated-media-routing/v1/{assetType}/{contentId}/{routingRecordId}.json
AgentDocs/planning-data/generated-media-routing/v1/{assetType}/{contentId}/routing_index.json
```

`routingRecordId` has no timestamp and is deterministic. The record schema is
`generated_media_routing_v1`; the index schema is
`generated_media_routing_index_v1`. Index entries are sorted by routingRecordId
and contain request/asset/domain/content identity, planningSnapshotHash,
registryVersion, selectedRegistryRowId, selected pipeline/prompt, record path,
record SHA-256, and
status.

Idempotency:

- the same canonical hash payload and identical record bytes return the
  existing record without adding a duplicate index entry;
- the same `routingRecordId` with different bytes is
  `routing_record_collision`;
- the same `requestId` with changed planning hash/profile/route requires a new
  routing ID and records `supersedesRoutingRecordId` when provided;
- blocked requests do not write a routed record or mutate the index.

## 10. generated_media_routing_v1

```yaml
schemaVersion: generated_media_routing_v1
routingRecordId:
routerVersion: generated_media_router_v2
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId:
requestId:
assetType:
domainType:
contentId:
planningHandoffFile:
sourcePlanningFiles:
planningRevision:
planningSnapshotHash:
sourcePlanningFiles: []
requiredElements: []
prohibitedElements: []
normalizedRequest:
selectedPipeline:
selectedAuthoringPrompt:
selectedGenerationPrompt:
provider: imagegen
structureProfile:
routingReason:
authoringHandoff:
createdAt:
validation:
```

Unknown/missing fields are rejected. Canonical paths:

```text
non-animation:
AgentDocs/planning-data/generated-media-routing/v2/{assetType}/{contentId}/{routingRecordId}.json

animation:
AgentDocs/planning-data/generated-media-routing/v2/animation/{contentId}/{animationRequestId}/{routingRecordId}.json
```

The deterministic hash payload excludes ID/timestamps and includes immutable
identity, snapshot/source hashes, exact type specification, registry row,
provider, structure profile, and optional animationRequestId. Same payload and
bytes reuses the record. Same ID with different bytes returns
`routing_record_collision`. Blocked input writes no record/index.

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
unreadable_source_planning
planning_snapshot_mismatch
missing_planning_revision
missing_output_usage
unsupported_domain_type
routing_record_collision
routing_record_write_failed
routing_index_write_failed
```

Success returns `status=routed`, record ID/path/hash, selected row/pipeline/
prompts, provider, structure profile, normalized request, snapshot hash, reason,
authoring handoff, and `nextStep=authoring`. Failure returns `status=blocked`,
failureType, missing/conflicting fields, candidate rows, required decision and
safeToRetry.

## Validation

- no current row or output contains a PixelLab route;
- no current character contract contains eight-way/rotation output;
- every animation unit contains exactly one animationRequestId;
- version/path/schema are routing v2 and directory v2;
- authoring handoff fields exactly match selected prompt inputs;
- no downstream stage executes.
