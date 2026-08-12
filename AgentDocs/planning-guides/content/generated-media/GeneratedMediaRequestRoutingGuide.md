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
assetType: character_single_image | icon_single_image | animation
domainType: character | skill | item
provider: imagegen
```

Profile keys are exact registry values. Do not route by filename, prose,
similarity, provider availability, or legacy alias.

## Deterministic Fan-out

- character/icon input creates exactly one routing unit;
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
animation/character    -> pelvis_root_ground_axis
animation/skill        -> effect_origin
```

## Routing Record v2

```yaml
schemaVersion: generated_media_routing_v2
routingRecordId:
routerVersion: generated_media_router_v2
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId:
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
unsupported_current_route
conflicting_routing_evidence
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
