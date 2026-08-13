# Generated Media Legacy v1 Compatibility Guide

## Purpose and Exclusive Authority

Guide Type: read-only legacy audit/verification authority. It is the only
authority used by retained PixelLab and old generic ImageGen filenames. Those
files may interpret already-existing immutable evidence, but may not reproduce
an execution or create any artifact.

Current-only handoff, registry, visual-authoring, record, preservation and
evaluation guides are not v1 schema authorities and must not be referenced to
validate a legacy record.

## Closed Legacy Evidence

```text
generated_media_planning_handoff_v1
generated_media_planning_handoff_compat_v1
generated_media_authoring_profile_registry_v1
generated_media_router_v1
generated_media_routing_v1
generated_media_prompt_v1 | generated_media_prompt_v2
generated_media_generation_v1
generated_media_preservation_v1
generated_media_evaluation_package_v1
```

Historical providers may be `pixellab` or `imagegen`. Historical profiles may
be `ordered_rotation_set`, `ordered_frame_set`, `single_image`, or
`paired_sheet_animation`. These names describe stored evidence only.

## Read-only Audit Contract

Allowed:

- read caller-supplied project-relative immutable record, index, prompt, media,
  manifest, evaluation package and stored hash;
- verify schema name, identity links, path, stored hashes, member order and
  provider provenance without repairing them;
- report what the historical bytes prove, what is missing, and whether the
  evidence chain is internally consistent.

Forbidden:

- provider/tool/page access or availability checks;
- prompt authoring, translation, regeneration, retry or reproduction execution;
- creation or modification of prompt/generation/preservation/evaluation records;
- index insertion, update, migration, backfill or current-route selection;
- download, export, extraction, conversion, packaging, media modification;
- external approval, credit lookup, cost or billing;
- mutation of immutable record, index, prompt, media or hash bytes.

Any execution or mutation request stops before side effects with
`failureType=legacy_execution_forbidden`.

## Input, Output and Failure

Input is an explicit project-relative path to already-existing legacy evidence
and its expected stored schema/identity/hash. Broad directory scans and foreign
absolute paths are invalid.

Success output:

```yaml
status: audited
mode: read_only_legacy_audit
evidencePaths: []
observedSchemas: []
identityLinks: []
hashVerification: []
findings: []
mutationsPerformed: false
providerCalled: false
costIncurred: false
```

Legacy failure registry:

```text
legacy_execution_forbidden
legacy_evidence_path_missing
legacy_evidence_unreadable
legacy_schema_unsupported
legacy_identity_mismatch
legacy_hash_mismatch
legacy_chain_incomplete
legacy_index_mismatch
legacy_foreign_absolute_path
```

These tokens are exclusive to legacy audit and are not members of the current
Generated Media failure registry.

## Retained Audit Entry Files

```text
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabIconGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabAnimationPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabAnimationGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenGenerationPrompt.md
```

The filenames are preserved only for historical links. They do not expose an
execution path. New work starts from the current ImageGen-only contract and is
never derived by mutating legacy evidence.

### Entry-to-schema section mapping

Every retained entry must apply Canonical Serialization and Equality plus the
exact sections below. It may not substitute a current guide.

| retained entry | required legacy sections |
| --- | --- |
| PixelLabPipelineGuide.md | H1, R1, RT1, P1, G1, PR1, E1 |
| PixelLabCharacterPipelineGuide.md | P1, G1, PR1, E1 ordered_rotation_set and ordered_frame_set |
| PixelLabIconPipelineGuide.md | P1, G1, PR1, E1 single_image |
| PixelLabAnimationPipelineGuide.md | P1, G1, PR1, E1 paired_sheet_animation |
| PixelLabCharacterPromptAuthoringPrompt.md | H1, R1, RT1, P1 |
| PixelLabCharacterGenerationPrompt.md | P1, G1, E1 ordered_rotation_set/ordered_frame_set |
| PixelLabIconPromptAuthoringPrompt.md | H1, R1, RT1, P1 |
| PixelLabIconGenerationPrompt.md | P1, G1, E1 single_image |
| PixelLabAnimationPromptAuthoringPrompt.md | H1, R1, RT1, P1 |
| PixelLabAnimationGenerationPrompt.md | P1, G1, E1 paired_sheet_animation |
| ImageGenPromptAuthoringPrompt.md | H1, R1, RT1, P1 |
| ImageGenGenerationPrompt.md | P1, G1, E1 single_image |

## Canonical Serialization and Equality

The following rules are preserved verbatim in meaning from Git HEAD
`9c21718f4e627c09e318e876f890168db1ef643c` and apply only to audit.

- JSON is UTF-8 without BOM; object keys sort by Unicode code point; arrays keep
  declared semantic order; there is no insignificant whitespace or final file
  newline in hashed bytes; strings use LF; paths use `/` with no file-path
  trailing slash.
- Unknown keys reject before hashing. Required keys must exist. Optional keys
  are omitted, not `null`, unless a schema explicitly permits null.
- SHA-256 is lowercase hexadecimal over exact canonical bytes. Provider is
  canonical lowercase `pixellab` or `imagegen`.
- Text bodies normalize CRLF/CR to LF only. They are not trimmed or reflowed.
  Fence delimiters are excluded; all remaining bytes and terminal LF state are
  preserved for equality/hash checks.
- An existing ID/path with identical canonical payload and record bytes is one
  identity, not a duplicate. Same ID/path with differing payload or bytes is a
  collision. An index entry must point to the exact project-relative path and
  exact record SHA-256.

## H1 — Planning Handoff v1

Closed common schema:

```yaml
schemaVersion: generated_media_planning_handoff_v1
requestId:
assetType: character_main_image | character_animation | icon | general_animation | imagegen_image
domainType: character | skill | item | stage | battle | environment | other_registered_domain
contentId:
contentName:
contentUsage:
sourcePlanningFiles:
  - path:
    role: identity | design | motion | scene | runtime
    sha256:
    revision: optional
planningSnapshot:
  capturedAt:
  snapshotHash:
  approvedFacts:
requiredElements: non-empty list
prohibitedElements: non-empty list
projectTarget:
  path: optional
  status: informational_only
```

The type fields are flattened. Required contracts are:

- `character_main_image`: characterIdentity, appearanceSpecification and
  rotationContract with the exact eight ordered directions, exactCount 8 and
  identityConsistencyRequired=true.
- `character_animation`: characterProviderIdentity,
  approvedCharacterPackageId when required, and non-empty animationRequests;
  every item has animationRequestId, attack|idle|move type, actionSpecification,
  directionOrder, frameContract and mirroringPolicy.
- `icon`: iconProfile, subjectIdentity, semanticEffect, exactCountElements when
  applicable, backgroundPolicy and targetDisplayContract.
- `general_animation`: animationProfile, animationSubject, sequenceStages,
  loopMode, frameContract, runtimeBoundary and referenceImageContract.
- `imagegen_image`: imageProfile, depictedMoment, subjects, environment,
  composition, camera when required, aspectRatio and backgroundPolicy.

Both sourcePlanningFiles and planningSnapshot are required. Revision is optional
provenance and cannot be synthesized. Snapshot verification uses the producer's
declared normalized source-entry and approved-fact contract; an unverifiable
snapshot is `legacy_hash_mismatch`.

The only compatibility envelope is
`generated_media_planning_handoff_compat_v1`: artifactType→assetType,
outputUsage→contentUsage, type specification containers→the corresponding
flattened fields, and top-level planningRevision→compatibility evidence only.
Alias and canonical values must be canonically equal. No compatibility step may
invent a planning fact.

## R1 — Registry v1

`registryVersion=generated_media_authoring_profile_registry_v1` and
`profileKey={profileId}@{profileVersion}`. Match all assetType, domainType,
profileId and profileVersion fields exactly; 1 row matches, 0 is unsupported,
2+ is conflicting evidence.

| row | asset/domain | profile | provider/pipeline | prompt profile |
| --- | --- | --- | --- | --- |
| character_main_v1 | character_main_image/character | character_main_image@1.0.0 | pixellab/pixellab_character | pixellab_character_prompt_v1 |
| character_animation_v1 | character_animation/character | character_animation@1.0.0 | pixellab/pixellab_character | pixellab_character_animation_prompt_v1 |
| skill_icon_v1 | icon/skill | skill_icon@1.0.0 | pixellab/pixellab_icon | pixellab_icon_prompt_v1 |
| relic_icon_v1 | icon/item | relic@1.0.0 | pixellab/pixellab_icon | pixellab_icon_prompt_v1 |
| skill_animation_v1 | general_animation/skill | skill_animation@1.0.0 | pixellab/pixellab_animation | pixellab_animation_prompt_v1 |
| stage_popup_v1 | imagegen_image/stage | story_popup_main_image@1.0.0 | imagegen/imagegen | imagegen_composed_scene_prompt_v1 |
| battle_background_v1 | imagegen_image/battle | battle_background@1.0.0 | imagegen/imagegen | imagegen_composed_scene_prompt_v1 |

## RT1 — Routing v1

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
routing_hash=SHA256(canonical_json(routingHashPayload))
routingRecordId=gmroute.{assetType}.{contentId}.{routing_hash[0:12]}
record=AgentDocs/planning-data/generated-media-routing/v1/{assetType}/{contentId}/{routingRecordId}.json
index=AgentDocs/planning-data/generated-media-routing/v1/{assetType}/{contentId}/routing_index.json
```

The closed record is `generated_media_routing_v1` and contains the payload
identity plus status, inputSchemaVersion, rawInput path/hash,
compatibilityNormalizedInput and hash, planning/source identity, matched row,
pipeline/prompt, normalizedRequest, authoringHandoff, nextStep, createdAt and
validation. createdAt is excluded from ID. The
`generated_media_routing_index_v1` sorts by routingRecordId and stores identity,
planningSnapshotHash, registry/row/route, record path, record SHA-256 and status.

## P1 — Prompt v1/v2 and Hashes

The older compatibility prompt schema is `generated_image_prompt_v1` (historical
references may call it the v1 generated-media prompt). Its closed fields are:

```yaml
schemaVersion: generated_image_prompt_v1
promptRecordId:
requestId:
priorPromptRecordId:
revisionReason:
artifact:
  artifactType:
  contentDomain:
  contentId:
  contentName:
  artifactUsage:
  expectedStructureProfile:
sources:
  canonicalContentSources:
  sourceHashesOrRevisions:
  planningOriginalContent:
  displayContent:
  contentSnapshotHash:
routing:
  provider:
  providerTool:
  providerPage:
  providerPromptProfile:
  domainAdapter:
  adapterVersionOrRevision:
externalFacts:
  accepted:
  rejected:
generationBrief:
providerPromptPayload:
  pixelLab: fieldPrompts[] or null
  imageGen: sceneSections, scenePromptOriginal, language or null
providerPromptPayloadHash:
providerSettingsIntent:
expectedProviderResultRoles:
expectedDownloadRoles:
imagePolicy:
createdAt:
author:
validation:
  identityEvidence:
  domainContract:
  providerFitness:
  visualHierarchy:
  sourceCoverage:
  jsonMarkdownEquality:
  status:
```

Its stored contentSnapshotHash, provider payload hash, JSON/Markdown equality,
record path/index entry and immutable bytes must verify under the stored legacy
contract; no field is mapped into v2 during audit. The later fully closed record
is `generated_media_prompt_v2`:

```yaml
schemaVersion: generated_media_prompt_v2
promptRecordId:
requestId:
assetType:
domainType:
legacyArtifactType: optional
contentId:
contentUsage:
planningHandoffPath:
routingRecordFile:
routingRecordId:
routingRecordSha256:
planningSnapshotHash:
sourcePlanningFiles: []
registryVersion:
selectedRegistryRowId:
selectedPipeline:
selectedAuthoringPrompt:
appliedProfile:
normalizedRequest:
visualPromptGuideVersion:
visualBrief:
visualBriefSha256:
provider: pixellab | imagegen
providerTool:
providerPromptProfile:
providerPromptPayload:
providerPromptPayloadHash:
providerSettingsIntent:
requiredElements: []
prohibitedElements: []
acceptedPlanningFacts: []
rejectedOrUnusedPlanningFacts: []
status: ready_for_generation | blocked
priorPromptRecordId: optional
revisionReason: optional
createdAt:
validation:
```

The embedded visualBrief is `generated_media_visual_brief_v1`; its complete
object is the hash payload. It contains visualBriefId, guideContractVersion,
request/asset/domain/content/usage/snapshot/registry/row/profile identity,
planningOriginalRef, subject, hierarchy, composition, palette/material,
background, required/prohibited statements, supporting elements,
likelyWrongObjects, artifactSpecificBrief, evidence map, translation contract,
status and validation.

PixelLab providerPromptPayload is the canonical JSON array of contiguous unique
fieldOrder plus toolField and LF-normalized textOriginal; each field also has
markdownFenceBodySha256. ImageGen providerPromptPayloadHash is SHA-256 of exact
LF-normalized scenePromptOriginal. Exactly one provider branch is populated.

```yaml
planningHashPayload:
  schemaVersion: generated_media_planning_hash_payload_v1
  requestId:
  assetType:
  domainType:
  contentId:
  contentUsage:
  planningSnapshotHash:
  sourcePlanningFiles: sorted path, role, sha256 ascending
```

`planning_hash=SHA256(canonical_json(planningHashPayload))`.

```text
promptRecordId=gmprompt.{assetType}.{contentId}.{UTC_YYYYMMDDTHHMMSSZ}.{planning_hash[0:12]}
record=AgentDocs/planning-data/generated-media-prompts/v1/{assetType}/{contentId}/{promptRecordId}.json
markdown=.../{promptRecordId}.prompt.md
index=.../prompt_index.json
```

ID timestamps are exactly `YYYYMMDDTHHMMSSZ`. The
`generated_media_prompt_index_v1` sorts by createdAt then ID and stores identity,
status, hashes, timestamp, record/Markdown paths and, for v2, visual brief and
profile identity.

## G1 — Generation v1

```yaml
schemaVersion: generated_media_generation_v1
generationRecordId:
requestId:
assetType:
domainType:
legacyArtifactType: optional
contentId:
planningSnapshotHash:
promptRecordId:
promptRecordSha256:
providerPromptProfile:
submittedProviderPayloadHash:
provider: pixellab | imagegen
providerTool:
providerSettings:
attempts:
  - attemptNumber:
    startedAt:
    completedAt:
    submittedProviderPayloadHash:
    settings:
    costEvidence:
    providerResultRefs: []
    status:
providerResultRefs: []
provisionalSelection:
  providerResultRef: optional
  selectionRule: optional
  status: not_selected | provisional_not_evaluated | ambiguous
preservationHandoff:
  nextTask: preservation_packaging
  requestedAdapterId:
  expectedStructureProfile:
  requiredProviderResultRefs: []
generationStatus: generated | blocked | failed
createdAt:
validation:
```

```yaml
requestHashPayload:
  schemaVersion: generated_media_generation_request_hash_payload_v1
  requestId:
  assetType:
  domainType:
  contentId:
  planningSnapshotHash:
  promptRecordId:
  providerPromptPayloadHash:
  provider: pixellab | imagegen
  providerTool:
  providerSettings:
```

```text
request_hash=SHA256(canonical_json(requestHashPayload))
generationRecordId=gmgen.{assetType}.{contentId}.{UTC_YYYYMMDDTHHMMSSZ}.{request_hash[0:12]}
record=AgentDocs/planning-data/generated-media-generation/v1/{assetType}/{contentId}/{generationRecordId}.json
index=.../generation_index.json
```

The generation index schema is `generated_media_generation_index_v1`, sorted by
createdAt then ID, with identity, status, hashes, timestamp and exact record
path. Prompt hash, submitted payload hash, every attempt settings/payload hash,
and final providerResultRefs must agree exactly.

## PR1 — Preservation v1

```yaml
preservationHashPayload:
  schemaVersion: generated_media_preservation_hash_payload_v1
  requestId:
  assetType:
  domainType:
  contentId:
  planningSnapshotHash:
  promptRecordId:
  generationRecordId:
  generationRecordSha256:
  provider: pixellab | imagegen
  adapterId:
  structureProfile:
  providerResultRefs: exact generation order
```

```text
payloadHash=SHA256(canonical_json(preservationHashPayload))
preservationRecordId=gmpreserve.{assetType}.{contentId}.{payloadHash[0:20]}
record=AgentDocs/planning-data/generated-media-preservation/v1/{assetType}/{contentId}/{preservationRecordId}.json
index=.../preservation_index.json
```

`generated_media_preservation_v1` contains the hash-payload identity plus
originalMembers, extractedMembers, memberHashes, state, attempts, optional
failureType/packageId and createdAt. Its index sorts by record ID and stores
request/asset/domain/content, generation ID/SHA, payload hash, state, optional
packageId and exact path.

## E1 — Evaluation Package v1 and Profiles

The envelope is `generated_media_evaluation_package_v1` with packageId,
manifestPayloadHash, manifestPayload, sealedAt, readiness and blockers. Hash only
manifestPayload; exclude envelope fields, absolute roots and derived hashes.

```text
manifestPayloadHash=SHA256(canonical_json(manifestPayload))
packageId=evalpkg.{assetType}.{contentId}.{requestId}.{manifestPayloadHash[0:12]}
```

manifestPayload requires request/asset/domain/content, optional legacy alias,
planningSnapshotHash, prompt/provider-prompt/generation/preservation identity,
provider refs/settings/attempts, structureProfile, profileExtension, ordered
members with path/hash/media/dimensions/order/profileData, and informational
projectTarget.

- `ordered_rotation_set`: exact directionOrder north, north_east, east,
  south_east, south, south_west, west, north_west; expectedCount 8; identity
  consistency true; each rotationIndex equals member order/direction.
- `ordered_frame_set`: animationRequestId/type, directionOrder, loopMode,
  timingMode, optional uniformFps, expected counts; global contiguous frameOrder.
  Uniform timing uses boundaryMs(n)=round_half_up(n*1000/fps) and adjacent
  differences; per-frame values are positive integers.
- `single_image`: primaryMemberId, selectedProviderResultRef,
  provisional_not_evaluated, role icon_original or imagegen_original; exactly
  one matching primary member.
- `paired_sheet_animation`: reference/sheet IDs, positive rows/columns/cell
  size/usable count, row_major order, loop/timing and ordered frame IDs; each
  frame row/column/source sheet/cell size must agree.

## Deterministic Audit Decision

For every supplied chain: validate closed schema and required/optional fields;
canonicalize and recompute payload hashes/IDs; hash exact record/media bytes;
verify path and index schema/sort/entry; verify cross-record identity and stored
hash equality; then verify structure profile members. Unsupported schema,
identity inequality, hash inequality, incomplete chain and index mismatch return
respectively `legacy_schema_unsupported`, `legacy_identity_mismatch`,
`legacy_hash_mismatch`, `legacy_chain_incomplete`, and
`legacy_index_mismatch`.

All sections H1–E1 are audit-only. They do not authorize provider access,
record creation, index mutation, download, extraction, migration, repair, cost,
or reproduction. Such a request always returns `legacy_execution_forbidden`.
