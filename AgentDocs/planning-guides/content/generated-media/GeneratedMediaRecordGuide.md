# Generated Media Prompt and Generation Record Guide

## 1. Purpose

Guide Type: schema/data-structure. This guide defines immutable prompt and
provider-generation records shared by PixelLab and ImageGen. Prompt authoring,
provider execution, preservation/packaging, and evaluation are separate task
owners. A generation record contains provider result references only; it never
contains downloaded files, extracted members, or an evaluation package.

## 2. Authority and Source Priority

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
```

ContentFolderStructureGuide owns storage boundaries. The planning handoff owns
approved facts and its snapshot identity. GeneratedImagePromptAuthoringGuide
owns authoring separation; GeneratedImageGenerationPipelineGuide owns the
generation-only boundary. This guide owns new record serialization and IDs.
Provider/type guides may add supported fields but cannot change these
identities. Conflict returns `record_authority_conflict`.
GeneratedMediaVisualPromptAuthoringGuide owns visual normalization behavior.
This guide alone owns the complete persisted `visualBrief` schema, field order,
required/optional classification, serialization, and hash payload.

## 3. Canonical Serialization, Storage, and IDs

All timestamps used in IDs are UTC `YYYYMMDDTHHMMSSZ` with exactly 16 ASCII
characters, for example `20260810T031405Z`. Record `createdAt` may use ISO 8601,
but the ID timestamp cannot contain punctuation or fractional seconds.

Canonical JSON means UTF-8 without BOM; object keys sorted by Unicode code
point; arrays retained in declared semantic order; JSON primitives only; no
insignificant whitespace; `/` as every project-relative path separator; no
trailing slash for file paths; LF inside strings; and no final file newline in
the hashed byte sequence. Unknown keys are rejected before hashing. Missing
required keys block. Optional keys are omitted rather than encoded as `null`
unless their schema explicitly permits null.

Canonical ID payloads are:

```yaml
planningHashPayload:
  schemaVersion: generated_media_planning_hash_payload_v1
  requestId:
  assetType:
  domainType:
  contentId:
  contentUsage:
  planningSnapshotHash:
  sourcePlanningFiles: sorted by path, then role, then sha256 ascending

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
  providerSettings: exact submitted settings
```

`planning_hash = SHA256(canonical_json(planningHashPayload))` and
`request_hash = SHA256(canonical_json(requestHashPayload))`. Provider values are
stored and compared as canonical lowercase `pixellab` or `imagegen`; ASCII
lowercase normalization is allowed only before validation.

```text
promptRecordId = gmprompt.{assetType}.{contentId}.{UTC_YYYYMMDDTHHMMSSZ}.{planning_hash_prefix_12}
generationRecordId = gmgen.{assetType}.{contentId}.{UTC_YYYYMMDDTHHMMSSZ}.{request_hash_prefix_12}

AgentDocs/planning-data/generated-media-prompts/v1/{assetType}/{contentId}/{promptRecordId}.json
AgentDocs/planning-data/generated-media-prompts/v1/{assetType}/{contentId}/{promptRecordId}.prompt.md
AgentDocs/planning-data/generated-media-prompts/v1/{assetType}/{contentId}/prompt_index.json
AgentDocs/planning-data/generated-media-generation/v1/{assetType}/{contentId}/{generationRecordId}.json
AgentDocs/planning-data/generated-media-generation/v1/{assetType}/{contentId}/generation_index.json
```

The path segment and index family `v1` are `directoryVersion=v1`; they
identify the stable storage layout and do not identify the JSON record schema.
New files in that directory use
`recordSchemaVersion=generated_media_prompt_v2`. A v1 directory/index may
therefore point to both read-only compatibility v1 records and new v2 records.

Each index is `generated_media_prompt_index_v1` or
`generated_media_generation_index_v1` and contains a deterministic array sorted
by `createdAt`, then record ID. Each entry records request/asset/domain/content
identity, optional legacyArtifactType, status, hashes, timestamp, and exact
project-relative record file path. Prompt entries also record the `.prompt.md`
path. A v2 prompt entry additionally records `visualBriefId`,
`visualBriefSha256`, `visualPromptGuideVersion`, `registryRowId`, and exact
profile ID/version. Duplicate IDs or differing bytes at an existing path are
collisions.

## 4. generated_media_prompt_v2

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
  schemaVersion: generated_media_visual_brief_v1
  visualBriefId:
  guideContractVersion:
  requestId:
  assetType:
  domainType:
  contentId:
  contentUsage:
  planningSnapshotHash:
  registryVersion:
  registryRowId:
  profileId:
  profileVersion:
  planningOriginalRef:
  primarySubjectOrSilhouette:
  visualHierarchy:
  composition:
  paletteAndMaterial:
  backgroundPolicy:
  requiredVisualStatements: []
  prohibitedVisualStatements: []
  supportingElements: []
  likelyWrongObjects: []
  artifactSpecificBrief:
  visualEvidenceMap: []
  providerTranslationContract:
  status: normalized
  validation:
visualBriefSha256:
provider: pixellab | imagegen
providerTool:
providerPromptProfile:
providerPromptPayload:
  pixelLab:
    fieldPrompts:
      - fieldOrder:
        toolField:
        textOriginal:
        markdownFenceBodySha256:
  imageGen:
    sceneSections: []
    scenePromptOriginal:
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

The YAML declaration above is the canonical field order for
`generated_media_prompt_v2` and its embedded `visualBrief`. Every shown field
is required except fields explicitly marked `optional`. Unknown fields are
rejected. Hashing still sorts object keys under Section 3; declared field order
is used for schema parity checks and human-readable rendering.

The canonical `visualBrief` hash payload is the complete `visualBrief` object
in the exact schema above, including `contentUsage`, `status`, and
`validation`. Compute
`visualBriefSha256=SHA256(canonical_json(visualBrief))`. No other guide may
add, omit, rename, reorder, or change requiredness of embedded brief fields.

`visualBrief` is the only persisted provider-neutral authoring intermediate. It
is embedded in the immutable prompt JSON and rendered in the prompt Markdown as
a clearly labeled non-copy-ready audit section. It has no separate mutable path
or index. Recompute `visualBriefId` by
`GeneratedMediaVisualPromptAuthoringGuide.md`, then compute
`visualBriefSha256=SHA256(canonical_json(visualBrief))`. A mismatch, missing
evidence map, or non-`normalized` status blocks the prompt record.

Exactly one provider payload branch is populated. For PixelLab, sort by unique
contiguous `fieldOrder` starting at zero and hash canonical JSON of the ordered
`toolField` plus LF-normalized `textOriginal` records. The copy-ready Markdown
contains one `## PixelLab Field: {toolField}` heading immediately followed by
exactly one fenced body for every field, in fieldOrder, and no unlabeled
provider text.
For each field independently: remove only the fence delimiters, normalize CRLF
or CR to LF, preserve all other bytes including leading/trailing spaces and
whether the body ends in LF, then require byte equality with the correspondingly
LF-normalized `textOriginal`; also verify `markdownFenceBodySha256`. Missing,
duplicate, extra, reordered, or mismatched fields block.

ImageGen hashes the exact LF-normalized `scenePromptOriginal`; its single
copy-ready fenced body is byte-equal after the same delimiter removal and LF
normalization. Do not trim or reflow provider text.

## 5. generated_media_generation_v1

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
  selectionRule: optional deterministic provider-operation rule
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

The record stores exact submitted payload provenance but never rewrites prompt
text. It is immutable once written. Download, export, extraction, member
hashes, evaluation readiness, and package identity belong to a later task.

## 6. State and Ownership

```text
prompt authoring task -> generated_media_prompt_v2
provider generation task -> generated_media_generation_v1
preservation/package task -> generated_media_preservation_v1 + generated_media_evaluation_package_v1
evaluation task -> evaluation result
```

Only `ready_for_generation` prompts may be submitted. Changed planning,
profile, prompt text, or settings contract requires a new prompt record.
Provider retries may share one generation record only while the prompt contract
is unchanged. Preservation failure never authorizes provider regeneration.

## 7. Compatibility and Migration

Legacy `generated_image_prompt_v1` and `generated_image_generation_v1` remain
read-only compatibility inputs. New version `generated_media_generation_v1`
replaces only provider execution and its result-reference handoff; it does not
replace later download/evaluation tasks with generation behavior.

`generated_media_prompt_v1` also remains an immutable read-only compatibility
input. It predates the required embedded visual brief. New prompt-authoring
tasks write `generated_media_prompt_v2`; they never add visual fields to or
overwrite a v1 record. A generation task may consume an existing v1 record only
under its original validated contract and unchanged planning/profile hashes.
Re-authoring creates a new v2 record with a new promptRecordId and explicit
`priorPromptRecordId`; no in-place schema upgrade is allowed.

`directoryVersion=v1` and `recordSchemaVersion` are independent version
axes. The former changes only when the path/index layout changes; the latter
changes when record fields or semantics change. Migration from prompt v1 to
prompt v2 creates a new v2 record and index entry in the existing v1 directory,
links `priorPromptRecordId`, and leaves the v1 bytes untouched.

```text
legacyArtifactType -> explicit assetType/domainType mapping
contentSnapshotHash -> planningSnapshotHash after source re-verification
providerPromptPayload -> matching provider-native branch
provider result refs -> verified attempt provenance and preservation handoff
```

Unverifiable mapping or hash returns `legacy_record_migration_blocked`.

## 8. Failure and Validation Closure

```text
record_identity_mismatch
planning_snapshot_hash_mismatch
provider_prompt_profile_mismatch
provider_payload_branch_conflict
provider_prompt_hash_mismatch
visual_brief_identity_mismatch
visual_brief_hash_mismatch
visual_evidence_map_incomplete
visual_brief_schema_parity_failed
prompt_markdown_mismatch
prompt_record_collision
prompt_record_write_failed
generation_record_collision
generation_record_write_failed
legacy_record_migration_blocked
prompt_schema_version_unsupported
record_authority_conflict
invalid_utc_id_timestamp
canonical_payload_invalid
unknown_record_field
missing_record_field
provider_value_invalid
pixellab_field_order_invalid
pixellab_field_body_mismatch
index_entry_invalid
```

- verify IDs and paths use the same asset/content identity;
- recompute both canonical payload hashes and record IDs from stored facts;
- reject every unknown field and block on every missing required field before
  calculating an ID, writing a record, or updating an index;
- verify prompt, submitted payload, settings, attempts, and refs agree;
- verify visual guide/profile versions, visualBriefId, visualBriefSha256,
  evidence coverage, and provider translation contract agree;
- verify the embedded brief has exact field-name, declared-order,
  required/optional, and unknown-field parity with Section 4;
- verify every PixelLab field order, fenced body, textOriginal, and body hash;
- verify provider is stored as canonical lowercase;
- verify index schema, sort order, exact record path, and record hash;
- reject downloaded paths, extraction results, package IDs, and evaluation
  verdict/readiness fields in a generation record;
- verify no record path contains an external absolute workspace root.

Validation closes only when all checks succeed and `validation.status=valid`
lists the rule IDs and computed hashes. Any failure returns one typed failure,
keeps the prior index unchanged, and does not write a partial ready record.

## 9. Related Guides

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```
