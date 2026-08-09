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
```

ContentFolderStructureGuide owns storage boundaries. The planning handoff owns
approved facts and its snapshot identity. GeneratedImagePromptAuthoringGuide
owns authoring separation; GeneratedImageGenerationPipelineGuide owns the
generation-only boundary. This guide owns new record serialization and IDs.
Provider/type guides may add supported fields but cannot change these
identities. Conflict returns `record_authority_conflict`.

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

Each index is `generated_media_prompt_index_v1` or
`generated_media_generation_index_v1` and contains a deterministic array sorted
by `createdAt`, then record ID. Each entry records request/asset/domain/content
identity, optional legacyArtifactType, status, hashes, timestamp, and exact
project-relative record file path. Prompt entries also record the `.prompt.md`
path. Duplicate IDs or differing bytes at an existing path are collisions.

## 4. generated_media_prompt_v1

```yaml
schemaVersion: generated_media_prompt_v1
promptRecordId:
requestId:
assetType:
domainType:
legacyArtifactType: optional
contentId:
planningHandoffPath:
planningSnapshotHash:
sourcePlanningFiles: []
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
prompt authoring task -> generated_media_prompt_v1
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
prompt_markdown_mismatch
prompt_record_collision
prompt_record_write_failed
generation_record_collision
generation_record_write_failed
legacy_record_migration_blocked
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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```
