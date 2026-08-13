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
routing_record_collision
routing_record_write_failed
routing_index_write_failed
```

Success returns `status=routed`, record ID/path/hash, selected row/profile/
pipeline/prompts, provider, structure profile, normalized request, snapshot hash, reason,
authoring handoff, and `nextStep=authoring`. Failure returns `status=blocked`,
failureType, missing/conflicting fields, candidate rows, required decision and
safeToRetry.

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
- no downstream stage executes.
