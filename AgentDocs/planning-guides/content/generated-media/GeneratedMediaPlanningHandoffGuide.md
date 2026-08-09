# Generated Media Planning Handoff Guide

## 1. Purpose

Guide Type: schema/data-structure. This guide defines the immutable external
planning handoff consumed by PixelLab and ImageGen prompt-authoring tasks.
Generation tasks translate approved facts into provider prompts; they never
invent identity, visual requirements, motion, sequence, or scene design.

## 2. Authority and Scope

Required authorities:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/prompt/GuideAuthoringGuide.md
AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
```

The master concept owns applicable period, culture, aesthetics and prohibitions.
The external planning owner owns meaning and design. This guide owns the common
handoff shape. Provider pipeline guides own rendering-only additions. Runtime
code/schema owns executable frame and serialization constraints.

This contract covers image and animation generation inputs only. It does not
create planning, evaluate media, promote project assets, build Unity assets, or
perform Git work.

## 3. generated_media_planning_handoff_v1

Required common fields:

```yaml
schemaVersion: generated_media_planning_handoff_v1
requestId: stable request identity
assetType: character_main_image | character_animation | icon | general_animation | imagegen_image
domainType: character | skill | item | stage | battle | environment | other_registered_domain
contentId: canonical content identity
contentName: display name
contentUsage: player-facing use of the generated media
sourcePlanningFiles:
  - path: exact project-relative source path
    role: identity | design | motion | scene | runtime
    sha256: exact source hash
    revision: optional source revision
planningSnapshot:
  capturedAt: UTC timestamp
  snapshotHash: SHA-256 of normalized source entries and approved facts
  approvedFacts: immutable planning facts used by generation
requiredElements: non-empty independently observable requirements
prohibitedElements: non-empty independently observable exclusions
projectTarget:
  path: optional informational project destination
  status: informational_only
```

`sourcePlanningFiles` and `planningSnapshot` are both required. A mutable source
path without a snapshot/hash is insufficient.

### 3.1 Canonical raw input

The fields above plus the flattened type-specific fields in Section 4 are the
authoritative raw `generated_media_planning_handoff_v1` shape. In particular:

```text
assetType is canonical
contentUsage is canonical
sourcePlanningFiles[].revision is optional per source
type-specific fields are flattened as written in Section 4
```

Do not require normalized routing names such as `outputUsage`,
`planningRevision`, or specification containers from canonical raw input.

### 3.2 Allowed compatibility envelope

Legacy callers may use `generated_media_planning_handoff_compat_v1` with only
these aliases in addition to unchanged common identity/source/snapshot fields:

| Compatibility field | Canonical raw target |
| --- | --- |
| `artifactType` | `assetType`, using the exact mapping owned by GeneratedMediaRequestRoutingGuide.md |
| `outputUsage` | `contentUsage` |
| top-level `planningRevision` | routing-only compatibility evidence; it never rewrites source entries |
| `characterSpecification` | the exact character fields in Sections 4.1 or 4.2 |
| `iconSpecification` | icon fields in Section 4.3; `iconProfile` remains explicit |
| `animationSpecification` | general-animation fields in Section 4.4 |
| `imageSpecification` | image fields in Section 4.5; `imageProfile` remains explicit |

Compatibility processing order is mandatory:

```text
rawInput capture
-> detect canonical or compatibility schemaVersion
-> resolve allowed aliases/containers
-> reject alias/canonical conflicts
-> compatibilityNormalizedInput containing canonicalHandoff + compatibilityEvidence
-> required-field validation
-> unknown-field rejection
-> source/snapshot validation
-> routing normalization
```

If alias and canonical fields coexist, their canonical JSON values must be
identical; otherwise return `compatibility_alias_conflict`. If one raw asset
token produces more than one canonical asset candidate, return
`ambiguous_asset_type`. An unregistered legacy asset alias returns
`unsupported_asset_type`. Top-level `planningRevision` is removed from `canonicalHandoff` and
preserved under `compatibilityEvidence`; it never populates or rewrites
`sourcePlanningFiles`. It is usable only when covered by the immutable snapshot
and declared to apply to every source. Multiple source revisions cannot be
collapsed; mixed, partial, or conflicting revision evidence blocks with
`planning_revision_conflict`.

No other alias or container is permitted. Compatibility normalization does not
create planning facts and does not change the canonical raw schema.
For clarity, a compatibility specification container and its corresponding
flattened canonical fields are an alias/canonical pair: unequal mapped values
return `compatibility_alias_conflict` during this normalization stage.

Compatibility asset failure selection is deterministic:

| Priority | Condition | failureType |
| --- | --- | --- |
| 1 | alias and canonical asset fields normalize to unequal values | `compatibility_alias_conflict` |
| 2 | one raw asset token yields multiple canonical asset candidates | `ambiguous_asset_type` |
| 3 | a legacy asset alias is not in the allowed mapping | `unsupported_asset_type` |

Use the first matching row. `conflicting_routing_evidence` is not an alias
normalization failure; it is reserved by the router for incompatible complete
route tuples from independent authoritative non-alias evidence or duplicate
exact registry rows.

## 4. Type-specific Contracts

### 4.1 character_main_image

```yaml
characterIdentity: approved identity and provider lookup keys
appearanceSpecification: body, costume, equipment, palette, required features
rotationContract:
  orderedDirections: [north, north_east, east, south_east, south, south_west, west, north_west]
  exactCount: 8
  identityConsistencyRequired: true
```

### 4.2 character_animation

```yaml
characterProviderIdentity: exact approved PixelLab character reference
approvedCharacterPackageId: evaluated main-character package identity when required
animationRequests:
  - animationRequestId: stable per-action identity
    animationType: attack | idle | move
    actionSpecification: externally approved visible action
    directionOrder: non-empty ordered directions
    frameContract: count, timing, key pose, ending/loop behavior
    mirroringPolicy: explicit allowed/forbidden mapping
```

Only entries present in `animationRequests` are eligible. Do not synthesize a
fixed Attack/Idle/Move list or derive action descriptions from combat lore.

### 4.3 icon

```yaml
iconProfile:
  profileId: exact registered ID
  profileVersion: exact registered MAJOR.MINOR.PATCH
subjectIdentity: approved central symbol/object
semanticEffect: approved visible meaning
exactCountElements: explicit counts when applicable
backgroundPolicy: detailed | symbolic | flat | transparent | none
targetDisplayContract: intended display size and readability constraints
```

`domainType` carries skill/item differences. New skill- or item-specific
execution prompts are prohibited.

### 4.4 general_animation

```yaml
animationProfile:
  profileId: exact registered ID
  profileVersion: exact registered MAJOR.MINOR.PATCH
animationSubject: approved character-independent VFX/object subject
sequenceStages: non-empty ordered visual stages
loopMode: loop | one_shot | hold_last
frameContract: count, timing, sheet layout, extraction order
runtimeBoundary:
  generatedMotion: motion inside frames
  runtimeOwnedMotion: translation, rotation, targeting, collision, or other runtime work
referenceImageContract: required source/reference identity and hash
```

### 4.5 imagegen_image

```yaml
imageProfile:
  profileId: exact registered ID
  profileVersion: exact registered MAJOR.MINOR.PATCH
depictedMoment: exact planned moment
subjects: approved visible subjects and relationships
environment: approved location and environmental facts
composition: approved framing and spatial priority
camera: approved viewpoint when required
aspectRatio: approved ratio
backgroundPolicy: detailed | constrained | flat | transparent | none
```

## 5. Validation and Failure

Block before prompt authoring when any required field is missing, empty,
ambiguous, stale, or unsupported.

```text
missing_planning_handoff
invalid_planning_handoff
planning_snapshot_hash_mismatch
missing_source_planning_file
missing_required_elements
missing_prohibited_elements
missing_character_identity
missing_appearance_specification
missing_animation_requests
invalid_animation_request
missing_sequence_specification
missing_animation_profile
missing_runtime_boundary
missing_icon_profile
missing_scene_specification
unsupported_asset_type
unsupported_domain_type
unsupported_profile
planning_authority_conflict
invalid_compatibility_envelope
compatibility_alias_conflict
planning_revision_conflict
missing_asset_type
ambiguous_asset_type
conflicting_routing_evidence
```

An explicitly empty `prohibitedElements` list is not valid unless the owning
planning schema supplies a signed/hashed `no_prohibitions` decision. Optional
`projectTarget.path` must never equal an evaluation staging source path.

## 6. Handoff

The prompt-authoring task receives the immutable handoff and returns either one
provider-ready prompt record or a blocker. It must preserve every accepted fact
with source evidence and list rejected/unconsumed facts. It must not modify the
planning handoff.

## 7. Completion Checklist

- [ ] Identity, source paths, snapshot, required and prohibited elements exist.
- [ ] The type-specific contract is complete.
- [ ] No provider task is asked to decide design or motion meaning.
- [ ] Project target is informational and separate from staging.
- [ ] Every path is project-relative; local roots are resolved internally.

## 8. Related Guides

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```
