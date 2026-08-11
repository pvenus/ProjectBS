# Generated Media Request Routing Guide

## 1. Purpose and Scope

Guide Type: workflow/pipeline and record contract. This guide defines the single
entry point that validates one immutable external generated-media planning
handoff, normalizes routing fields, and selects exactly one provider prompt-
authoring pipeline.

The router does not author provider prompts, operate PixelLab or ImageGen,
download or package media, evaluate results, promote project assets, perform
Git work, or deploy. It never supplies missing planning or chooses a route from
semantic similarity.

## 2. Required Authorities

```text
AgentDocs/planning-guides/prompt/GuideAuthoringGuide.md
AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
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
sourcePlanningFiles: non-empty exact paths, roles, hashes, optional revisions
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
- otherwise, when every canonical `sourcePlanningFiles` entry contains the same
  non-empty `revision`, it becomes normalized `planningRevision`;
- when every source entry omits `revision`, omit normalized
  `planningRevision`; SHA-256 and the immutable snapshot remain sufficient;
- partial revision coverage or unequal revisions blocks. A missing revision by
  itself is not a failure because revision is optional in the canonical handoff.

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
rawInput
-> schema detection
-> allowed compatibility alias/container resolution
-> alias/canonical conflict check
-> compatibilityNormalizedInput {canonicalHandoff, compatibilityEvidence}
-> required-field validation
-> unknown-field rejection
-> source/snapshot validation
-> routing normalizedRequest
-> registry matching
```

During routing normalization:

- trim surrounding ASCII whitespace from enum/profile tokens only;
- convert ASCII enum tokens to lowercase;
- convert `-` to `_` only for a documented alias in Section 5;
- preserve `requestId`, `contentId`, paths, planning text, and element text
  byte-for-byte except the canonical JSON LF rule;
- reject Unicode lookalikes and unknown enum aliases;
- reject unknown fields only after compatibility normalization and before
  calculating a routing record ID;
- deduplicate neither required nor prohibited elements; duplicates are a
  planning validation issue, not router-owned editing.

`rawInput.sha256` is the SHA-256 of exact handoff file bytes.
`compatibilityNormalizedInputHash` is the SHA-256 of canonical JSON of the
wrapper after allowed compatibility resolution; for canonical input the
wrapper contains the unchanged raw object as `canonicalHandoff` and empty
`compatibilityEvidence`. Record `compatibilityApplied=true|false`.

Canonical enums:

```text
assetType:
  character_main_image
  character_animation
  icon
  general_animation
  imagegen_image

domainType initially routable:
  character
  skill
  item
  stage
  battle
  environment
  other_registered_domain

provider:
  pixellab
  imagegen
```

Profile values use the exact format and values in
`GeneratedMediaAuthoringProfileRegistryGuide.md`. They are opaque registry
keys; the router must not infer one from `domainType`, content prose, filenames,
or similarity.

## 5. Compatibility Aliases

Legacy aliases are accepted only inside
`generated_media_planning_handoff_compat_v1`. Apply the exact compatibility
envelope in `GeneratedMediaPlanningHandoffGuide.md`. Its `artifactType` mapping
is:

| legacy artifactType | canonical assetType | required domain evidence |
| --- | --- | --- |
| `character_image` | `character_main_image` | `domainType=character`, fixed `character_main_image@1.0.0` |
| `character_animation` | `character_animation` | `domainType=character`, fixed `character_animation@1.0.0` |
| `skill_icon` | `icon` | `domainType=skill`, `skill_icon@1.0.0` |
| `item_icon` | `icon` | `domainType=item`, explicit relic planning evidence and `relic@1.0.0` |
| `skill_animation` | `general_animation` | `domainType=skill`, `skill_animation@1.0.0` |
| `story_popup_main_image` | `imagegen_image` | `domainType=stage`, `story_popup_main_image@1.0.0` |
| `battle_background` | `imagegen_image` | `domainType=battle`, `battle_background@1.0.0` |

If alias and canonical fields both exist, unequal canonical values return
`compatibility_alias_conflict`. A raw asset token yielding multiple canonical
candidates returns `ambiguous_asset_type`. An unregistered legacy alias returns
`unsupported_asset_type`. The normalized request records the legacy value as
`legacyArtifactType` but never emits it as the canonical type.

## 6. Closed Routing Registry

Router version: `generated_media_authoring_router_v1`.
Profile registry authority and version:

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
generated_media_authoring_profile_registry_v1
```

This router revision is pinned to that exact v1 registry. Callers cannot choose
or override `registryVersion`; a supplied version-selection field is invalid.
A later registry may be used only after a reviewed router guide/prompt revision
or an explicit registry migration changes the pinned version.

The exact decision table is Section 3 of the named registry file. Do not copy,
extend, or override its rows locally. For each row, compare this complete key:

```text
assetType + domainType + profileId + profileVersion
```

Character rows use the fixed technical profile declared by their exact
asset/domain row. Other rows use the external profile object after compatibility
normalization. Successful routing requires exactly one exact registry row.

Matching rules:

```text
exactly 1 matching row -> routed
0 matching rows -> apply Section 6.1 failure decision table
2 or more matching rows -> blocked: conflicting_routing_evidence
```

Do not break a tie with filename, content meaning, provider availability, or
similarity. A new domain extends the versioned authority registry and evaluation
adapter; it does not copy an authoring prompt or add an overlapping generic row.

### 6.1 Deterministic failure decision

Apply the first matching failure in this order:

Compatibility/schema failures are decided before this table. Then apply the
first matching routing failure:

| Priority | Condition | failureType |
| --- | --- | --- |
| 1 | asset token missing | `missing_asset_type` |
| 2 | asset token has multiple conflicting canonical candidates | `ambiguous_asset_type` |
| 3 | asset token is not a canonical enum or allowed alias | `unsupported_asset_type` |
| 4 | domain token is not a canonical enum | `unsupported_domain_type` |
| 5 | a non-character row lacks profile ID or version | `missing_type_specification` |
| 6 | exact asset/domain/profile ID/version pair has no registry row | `invalid_domain_profile` |
| 7 | exact key matches two or more registry rows, or two independent authoritative non-alias sources supply incompatible complete route tuples | `conflicting_routing_evidence` |

Other missing type-specific fields are evaluated after one exact registry row
is found and also return `missing_type_specification`. Do not replace an earlier
failure with a later, less specific one.

## 7. Source Verification and Conflict Rules

1. Read the handoff file and reject a missing or wrong schema.
2. Read every `sourcePlanningFiles.path`; verify exact SHA-256 and revision when
   declared.
3. Verify `snapshotHash` using the planning producer's declared canonical
   snapshot contract, then recalculate `planning_hash` using the canonical
   payload rules owned by `GeneratedMediaRecordGuide.md`. A handoff without a
   verifiable snapshot-hash contract is invalid.
4. Treat `planningSnapshot.approvedFacts` as immutable approved planning.
5. Use source files only to verify those facts, never to add a missing routing
   input.
6. External runtime hints, filenames, legacy records, and user prose are
   non-authoritative when they are not included in the hashed handoff.

A source that is unreadable, hash-mismatched, stale, or contradictory blocks
routing. Do not write a successful record from partially verified sources.

## 8. Normalized Request and Field-level Handoff

Common normalized request:

```yaml
schemaVersion: generated_media_authoring_request_v1
planningHandoffFile:
requestId:
assetType:
domainType:
legacyArtifactType: optional
contentId:
sourcePlanningFiles:
planningRevision: optional; omitted when every source omits revision
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
  planningRevision: optional; omitted when every source omits revision
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
routerVersion: generated_media_authoring_router_v1
registryVersion: generated_media_authoring_profile_registry_v1
status: routed
inputSchemaVersion:
rawInput:
  planningHandoffFile:
  sha256:
compatibilityNormalizedInput:
  canonicalHandoff:
  compatibilityEvidence:
compatibilityNormalizedInputHash:
requestId:
assetType:
domainType:
legacyArtifactType: optional
contentId:
planningHandoffFile:
sourcePlanningFiles:
planningRevision: optional; omitted when every source omits revision
planningSnapshotHash:
appliedProfile:
selectedRegistryRowId:
selectedPipeline:
selectedAuthoringPrompt:
routingReason:
normalizedRequest:
authoringHandoff:
supersedesRoutingRecordId: optional
nextStep: authoring
createdAt:
validation:
```

`routingReason` states exact enum/profile evidence and matched row ID; it cannot
contain inferred design reasoning. `createdAt` is fixed on first creation and
is excluded from the deterministic ID payload. `recordSha256` is calculated
from the completed record file and stored only in the index and returned
handoff; it is not embedded in the record bytes it hashes.

On rerun, calculate the deterministic ID and inspect an existing record before
assigning a new `createdAt`. Reuse its original bytes and timestamp when the
canonical request is identical.

## 11. States and Handoff

```text
received -> validating -> normalized -> matched -> routed
received | validating | normalized | matched -> blocked
```

`routed` and `blocked` are terminal for one routing attempt. A routed result
hands off, but does not execute, exactly one `selectedAuthoringPrompt`.

Successful output:

```yaml
status: routed
routingRecordId:
routingRecordFile:
routingRecordSha256:
registryVersion:
selectedRegistryRowId:
selectedPipeline:
selectedAuthoringPrompt:
assetType:
domainType:
contentId:
normalizedRequest:
appliedProfile:
sourcePlanningFiles:
planningSnapshotHash:
routingReason:
nextStep: authoring
```

Blocked output writes no record:

```yaml
status: blocked
failureType:
missingFields: []
conflictingFields: []
candidatePipelines: []
requiredDecision:
safeToRetry: true | false
```

## 12. Failure Types

```text
missing_planning_handoff
invalid_planning_handoff
invalid_compatibility_envelope
compatibility_alias_conflict
planning_revision_conflict
missing_asset_type
ambiguous_asset_type
missing_required_elements
missing_prohibited_elements
missing_type_specification
unsupported_asset_type
invalid_domain_profile
conflicting_routing_evidence
unreadable_source_planning
planning_snapshot_mismatch
missing_output_usage
unsupported_domain_type
routing_record_collision
routing_record_write_failed
routing_index_write_failed
```

`conflicting_routing_evidence` is restricted to duplicate exact registry rows
or incompatible complete route tuples from independent authoritative,
non-alias evidence. It never represents an alias/canonical value mismatch.

`safeToRetry=true` only when the same authoritative planning owner can supply or
correct the identified fields without changing the intended asset. Conflicting
authority, ambiguous type, unsupported registry entries, and collisions require
an owner/registry decision and default to false.

## 13. Validation and Completion

- verify the handoff schema and every source hash before normalization;
- verify rawInput file hash, compatibility-normalized canonical hash, and
  normalizedRequest are distinct and traceable;
- verify required/prohibited lists and the exact type specification;
- verify enums and profiles are canonical and registered;
- require exactly one registry row and record the row ID;
- verify every normalized field is traceable to the immutable handoff;
- recompute routing hash, ID, record path, record SHA-256, and index entry;
- verify existing identical records are reused idempotently;
- verify staging/source paths are not project targets when either is present;
- verify selected prompt exists at the exact project-relative path;
- verify `authoringHandoff.promptInput` uses only fields accepted by that
  selected prompt and every evidenceMap pointer resolves;
- verify no prompt text, provider setting, generated/downloaded media,
  evaluation result, promotion status, Unity/Git/deployment data was produced;
- verify blocked routing left the index unchanged.

Completion means one valid routing record and one authoring handoff, or one
typed blocker with no new record. Evaluation of the new guide and prompt is a
separate read-only task.

## 14. Related Documents

```text
AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```
