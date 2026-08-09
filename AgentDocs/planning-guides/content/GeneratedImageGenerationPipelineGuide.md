# Generated Image Generation Pipeline Guide

> Migration status: compatibility router for legacy `artifactType`. New
> callers use
> `AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md`
> or `AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md`.
> Their `generated_media_generation_v1` stage replaces provider execution and
> result-reference handoff only. Download/export/extraction/package sealing is
> owned by
> `AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md`.
> The generation-only/later-download boundary in this guide remains mandatory.

## Master Concept Reference

Before using this document, read and apply:

~~~text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
~~~

## 1. Purpose

This guide defines a generalized request and execution pipeline for creating one
generated visual artifact in a dedicated generation task.

~~~text
generalized external request
-> one dedicated generation execution task
-> internal provider and domain-guide routing
-> PixelLab or ImageGen generation
-> immutable generated_image_generation_v1 record
-> later download task
~~~

The parent task coordinates one generation execution owner. The execution task
resolves repository sources, provider, tool, prompt rules, settings, attempts,
and provider result references internally.

This pipeline performs generation only. It does not download provider files,
evaluate, repair, promote to the project, write Slack, create Unity metadata,
perform Git work, or deploy.

## 2. Required References and Authority

Always read:

~~~text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
~~~

The generation execution task then reads exactly one routed domain generation
adapter from Section 6.

Authority:

1. ContentFolderStructureGuide.md owns future project storage.
2. This guide owns task separation, provider routing, common request fields,
   generation-record schema, handoff, and extension rules.
3. The routed domain guide owns content interpretation, provider-specific prompt
   contract, tool page, settings, composition, dimensions, variant count, and
   retry limit.
4. Canonical planning/content data owns identity and intended meaning.

Do not merge conflicting provider or visual rules by judgment. Stop with
generation_contract_conflict and name both sources.

For generated-media v1, this guide remains authoritative for the generation
boundary; provider child guides specialize tool inputs only. The common
preservation guide begins after a successful immutable result-reference
handoff. Neither child guide may broaden generation to download or packaging.

## 3. Task Roles and Thread Ownership

### 3.1 generation_parent

The parent:

- validates the generalized request;
- finds or creates exactly one dedicated execution task for that request;
- passes generalized facts and repository context, not local paths;
- records the generation task ID;
- waits for the immutable generation record or a blocker;
- sends retry or clarification to the same execution task;
- does not open PixelLab, call ImageGen, or write a generation record.

### 3.2 generation_execution

The execution task:

- confirms its parent request and current repository;
- resolves provider, exact domain guide, canonical content, and generation
  contract internally;
- owns every provider operation and attempt for the artifact;
- writes and returns one immutable generation record;
- does not create another task or hand generation to another task.

### 3.3 Single-owner rules

- one logical artifact has one active generation execution task;
- do not run PixelLab and ImageGen in parallel for the same request;
- retries reuse the same execution task and append attempts to the same record;
- a batch is split into one request and one record per logical artifact;
- uncertain dispatch does not authorize duplicate task creation;
- the parent must not fall back to generating in its own task when dispatch is
  blocked.

## 4. External Request Contract

External callers provide generalized content facts only.

### 4.1 Allowed request

~~~text
requestId: optional stable external request id
artifactType: required supported generalized type
contentId: required canonical content id
promptRecordId: optional stable non-path generated_image_prompt_v1 record id
contentName: optional display name
contentSummary: optional concise gameplay or narrative meaning
visualIntent: optional desired moment or visual emphasis
requiredElements: optional semantic must-show list
forbiddenElements: optional semantic exclusion list
contextTags: optional generalized key/value facts
~~~

External facts supplement repository facts. They never silently override
canonical planning or content data.

### 4.2 Internally resolved fields

The caller must not need to provide:

~~~text
repositoryRoot
planningSourcePath or contentSourcePath
provider or provider tool URL
generationGuidePath
generation prompt path or prompt text
PixelLab page, mode, size, variation count, or animation preset
ImageGen model or output settings
local export or evaluation root
download path or filename
projectTargetPath
score, pass threshold, or evaluation guide
Slack, Git, or deployment destination
~~~

Path-like and provider fields supplied externally are untrusted hints. Resolve
them inside the execution task. Never reuse another PC's absolute path.

## 5. Canonical Content Resolution

Resolve content by exact artifactType and contentId:

1. canonical planning data under AgentDocs/planning-data;
2. canonical content JSON under Assets/Contents/{ContentDomain}/json;
3. a domain guide's explicitly documented legacy source while migration is
   pending.

Use external facts only for missing, non-conflicting presentation context.
Record accepted and rejected external facts. Stop on ambiguous identity,
content-type mismatch, or material planning conflict.

Before provider use, resolve a saved generated_image_prompt_v1 package and
verify that its generation brief contains:

~~~text
artifact identity and player-facing usage
content meaning and activation/current moment
primary subject or silhouette
direction and composition
required semantic elements
forbidden elements and likely wrong objects
style, material, palette, and background policy
artifact structure and technical output expectation
~~~

The execution task does not author or rewrite this brief or its provider-native
prompt payload.
It recalculates contentSnapshotHash and verifies the saved package against
current canonical content and routing.

### 5.1 Prompt record gate

Resolve the prompt record in this order:

1. exact promptRecordId matching artifactType and contentId;
2. the single latest ready_for_generation record whose contentSnapshotHash,
   provider, and adapter revision match the current request;
3. reuse_requested or skipped record for the current approved image policy.

The record must satisfy:

~~~text
schemaVersion = generated_image_prompt_v1
prompt status is eligible for its image policy
artifactType and contentId match
provider, providerPromptProfile, and domain adapter match current routing
contentSnapshotHash matches current canonical sources
exactly one providerPromptPayload branch is populated
JSON and Markdown copy-ready provider payloads agree
providerPromptPayloadHash matches the normalized copy-ready payload
prompt record SHA-256 is recorded
~~~

Do not choose between multiple eligible records by timestamp alone. Stop with
ambiguous_prompt_record. Missing or stale prompt records return the task to
GeneratedImagePromptAuthoringPrompt.md.

## 6. Artifact Generation Adapter Registry

| artifactType | Domain | Provider | Prompt profile | Structure expectation | Primary domain generation adapter | Readiness |
| --- | --- | --- | --- | --- | --- | --- |
| skill_icon | skill | PixelLab | pixellab_fielded_pixel_prompt_v1 | single icon candidate from UI variation set | AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md | ready |
| item_icon | item | PixelLab | pixellab_fielded_pixel_prompt_v1 | single icon candidate from UI variation set | AgentDocs/planning-guides/item/ItemIconGenerationGuide.md | ready |
| skill_animation | skill | PixelLab | pixellab_fielded_pixel_prompt_v1 | reference image plus animation sheet | AgentDocs/planning-guides/skill/SkillImageGenerationGuide.md | ready |
| character_image | character | PixelLab | pixellab_fielded_pixel_prompt_v1 | character identity and configured rotations | AgentDocs/planning-guides/character/CharacterGenerateImage.md | ready for generation/download handoff only |
| character_animation | character | PixelLab | pixellab_fielded_pixel_prompt_v1 | named Move, Attack, and Idle provider animations | AgentDocs/planning-guides/character/CharacterGenerateAnimation.md | ready |
| story_popup_main_image | stage | ImageGen | imagegen_composed_scene_prompt_v1 | one 3:4 story illustration when policy=generate | AgentDocs/planning-guides/stage/PopupEventMainImageCreateGuide.md | ready |
| battle_background | battle | ImageGen | imagegen_composed_scene_prompt_v1 | one 16:9 battle background | AgentDocs/planning-guides/battle/BattleCreateGuide.md | ready |

Rules:

1. artifactType must match one row exactly.
2. Provider is internal and mandatory. Never substitute the other provider.
3. reuse and none image policies do not call a provider. Return generation
   status=reuse_requested or skipped and preserve the policy source.
4. character_image may be generated and handed to download, but its project
   promotion remains unsupported until a canonical target adapter exists.
5. A domain guide that currently bundles download, evaluation, or project copy
   is a legacy combined guide. In this pipeline, execute only its generation
   rules and defer all later operations.
6. Add a new artifact through Section 15; do not infer a provider from its name.

## 7. Provider Contracts

### 7.1 PixelLab

PixelLab routes use the exact tool and page named by the domain guide.

Official references:

~~~text
https://www.pixellab.ai/docs
https://www.pixellab.ai/docs/tools/create-ui-elements-pro
https://www.pixellab.ai/docs/tools/animate-with-text-new
https://www.pixellab.ai/docs/tools/animate-with-text-pro
https://www.pixellab.ai/docs/tools/create-character
~~~

- Skill and item icons use Create UI elements (Pro) at the domain-documented
  create_ui_pro route.
- Skill animation uses the reference-image and animation flow documented by
  SkillImageGenerationGuide.md.
- Character image and animation use the character creation workflow documented
  by the character guides.
- Confirm the signed-in page and intended tool before spending credits.
- Record tool, page, prompt, settings, seed when exposed, variation or animation
  IDs, credit/cost confirmation, and provider result references.
- Require providerPromptProfile=pixellab_fielded_pixel_prompt_v1 and a populated
  providerPromptPayload.pixelLab branch only.
- Submit each verified fieldPrompt textOriginal verbatim to its mapped PixelLab
  toolField in order. Do not concatenate it into an ImageGen-style paragraph.
- Create UI elements (Pro) output grid and cost vary by requested size. Record
  the observed grid and pre-execution cost rather than copying a cost from an
  older record.
- PixelLab animation tools have different reference-size, frame-count, pricing,
  and first-frame behavior. Use only the exact version named by the domain
  adapter and validate the current UI before generation.
- Do not use an uploaded style/concept image when the domain guide prohibits it.
- Leave successful provider results available for the later download task.
- Do not save gallery previews or browser thumbnails as local source files.

If PixelLab is unavailable, signed out, out of credits, or the required tool is
missing, stop. Do not call ImageGen.

### 7.2 ImageGen

ImageGen routes use the configured ImageGen capability.

- Validate the saved final prompt against current canonical content and the
  exact domain visual guide.
- Require providerPromptProfile=imagegen_composed_scene_prompt_v1 and a populated
  providerPromptPayload.imageGen branch only.
- Submit the exact saved scenePromptOriginal as one prompt. Do not split it into
  PixelLab-style UI field fragments.
- Use the domain-defined aspect ratio, composition, subject exclusions, and
  technical intent.
- Record the exact submitted prompt and provider result attachment/reference.
- Preserve every returned result reference required to identify the selected
  result in the generation task.
- Do not download, recompress, resize, or write the image to the project.

If ImageGen is unavailable or fails, stop. Do not open PixelLab as fallback.

## 8. Generation Versus Evaluation

Generation completion is not visual approval.

The execution task may perform only provider-operation checks needed to know
that a usable result reference exists:

~~~text
provider operation completed
expected result count or named animation exists
result reference belongs to this request
media is visible and not an obvious provider error/blank response
required provider settings were used
~~~

It must not:

- assign an evaluation score;
- output PASS, CONDITIONAL_PASS, or FAIL as a quality decision;
- perform the domain evaluation rubric;
- claim project-copy eligibility;
- correct output bytes;
- treat provisional selection as evaluated approval.

Use generationStatus values instead:

~~~text
generated
generated_with_multiple_results
reuse_requested
skipped
blocked
failed
~~~

When a PixelLab tool returns multiple variants, the domain generation guide may
identify one provisional preferred result for download. Record the selection
reason and all provider result IDs. Label it
selectionStatus=provisional_not_evaluated.

## 9. Attempt and Retry Rules

1. Use the stricter of the domain attempt limit and two provider runs unless the
   user explicitly authorizes more.
2. Every attempt remains in the generation record.
3. Retry only provider-operation or obvious generation-contract failures.
4. Do not run formal evaluation to justify another generation attempt.
5. A retry may change an exposed runtime setting only when the prompt record
   permits it. Prompt text changes require a new promptRecordId from the prompt
   authoring task.
6. Reuse the same execution task.
7. Never overwrite a previous attempt's prompt, settings, result IDs, or failure
   facts.
8. When the limit is reached, return attempt_limit_reached and hand all result
   references to the caller without claiming success.

## 10. Generation Record Identity and Storage

Create one stable record ID:

~~~text
generationRecordId =
gen.{artifactType}.{contentId}.{UTC_YYYYMMDDTHHMMSSZ}.{request_hash_prefix_12}
~~~

The request hash is calculated from normalized generalized input, canonical
content identity, routed provider, and domain adapter ID/version.

Canonical project-relative record:

~~~text
AgentDocs/planning-data/image-generation/v1/{artifactType}/{contentId}/{generationRecordId}.json
~~~

Index:

~~~text
AgentDocs/planning-data/image-generation/v1/{artifactType}/{contentId}/generation_index.json
~~~

The record contains provider references and provenance, not downloaded image
bytes. Do not create the record folder before provider execution is ready to
start. On dispatch or dependency failure before execution, return a blocker
without a placeholder record.

Record workspace visibility must be explicit:

~~~text
shared_workspace
isolated_worktree
message_only
~~~

- shared_workspace means the parent can verify the relative record path.
- isolated_worktree means the record exists only in the execution task's
  worktree until an explicitly authorized synchronization step occurs.
- message_only is allowed only when the execution environment cannot write the
  record; return the complete validated record payload and its SHA-256.
- the parent does not copy, commit, or merge an isolated record as part of this
  generation task.
- the download handoff always includes generationTaskId, recordVisibility,
  project-relative intended record path, and record SHA-256.
- a later download task must use the same accessible workspace, read the source
  generation task, or consume an explicitly synchronized record. It must not
  assume cross-task file visibility.

Never reuse a generationRecordId for different request facts or provider
results. Stop on generation_record_collision.

## 11. generated_image_generation_v1 Contract

Required fields:

~~~text
schemaVersion: generated_image_generation_v1
generationRecordId
requestId
generationTaskId
parentTaskId
recordVisibility
generationRecordPath
generationRecordSha256
promptRecordId
promptRecordPath
promptRecordSha256
providerPromptProfile
providerPromptPayloadHash

artifact:
  artifactType
  contentDomain
  contentId
  contentName
  expectedStructureProfile

request:
  normalizedExternalFacts
  acceptedExternalFacts
  rejectedExternalFacts

sources:
  canonicalContentSources
  planningOriginalContent
  contentSnapshotHash

routing:
  provider
  providerTool
  providerPage
  domainAdapter
  adapterVersion or sourceRevision

generationBrief:
  artifactUsage
  visualIntent
  primarySubjectOrSilhouette
  directionAndComposition
  requiredElements
  forbiddenElements
  styleMaterialPalette
  backgroundPolicy
  technicalExpectation

prompt:
  promptRecordId
  contentSnapshotHash
  providerPromptProfile
  providerPromptPayloadHash
  submittedProviderPayload:
    pixelLab: submittedFieldPrompts[] | null
    imageGen: submittedScenePromptOriginal | null
  submittedProviderPayloadHash
  promptLanguages

providerSettings
attempts
providerResultRefs
provisionalPreferredResultRef
selectionStatus
generationStatus
downloadHandoff
createdAt
updatedAt
validation
~~~

Each attempt records:

~~~text
attemptNumber
startedAt and completedAt
promptRecordId
providerPromptProfile
submittedProviderPayload
submittedProviderPayloadHash
providerSettings
costOrCreditEvidence when exposed
providerResultRefs
providerOperationStatus
observedContractIssue
retryChange
~~~

downloadHandoff records:

~~~text
nextTask: download
generationTaskId
generationRecordId
recordVisibility
generationRecordPath
generationRecordSha256
artifactType
contentId
provider
expectedStructureProfile
providerResultRefs
provisionalPreferredResultRef
expectedDownloadRoles
expectedDimensionsOrFrameContract
downloadWarnings
~~~

Do not include a fabricated local source path, evaluation result, project hash,
promotion status, or Unity metadata.

## 12. Parent-to-Execution Handoff

The parent sends:

~~~text
executionMode: generation_execution
parentTaskId
requestId
artifactType
contentId
promptRecordId
contentName
contentSummary
visualIntent
requiredElements
forbiddenElements
contextTags
~~~

It does not send resolved provider, paths, scores, or copied guide prose.

The execution task returns:

~~~text
generationTaskId
generationRecordId, visibility, record path, and record SHA-256
promptRecordId and prompt record SHA-256
artifactType and contentId
provider and routed domain adapter
generationStatus
attemptCount
providerResultRefs
provisionalPreferredResultRef
downloadHandoff
blockers
~~~

The parent validates the returned request/artifact identity and record existence.
It does not rewrite the execution result.

## 13. Completion Conditions

Generation is complete only when:

- the execution task identity and parent request are linked;
- exact canonical content and domain guide were resolved;
- one current generated_image_prompt_v1 record was verified;
- the routed provider and required tool were used;
- at least one valid provider result reference exists, or an approved
  reuse/skip policy was resolved;
- every attempt and exact provider prompt is recorded;
- every attempt records the matching provider profile and only its native
  submitted payload branch;
- generated_image_generation_v1 validates;
- the record and index point to the same artifact and request when files were
  written;
- workspace visibility and record SHA-256 are reported truthfully;
- downloadHandoff is complete;
- no download, evaluation, project copy, Slack, Unity, Git, or deployment
  operation occurred.

## 14. Failure Types

~~~text
invalid_generation_request
thread_dispatch_unavailable
thread_dispatch_result_unknown
duplicate_generation_task
missing_parent_task
unsupported_artifact_type
missing_domain_generation_adapter
incomplete_domain_generation_adapter
generation_contract_conflict
repository_not_resolved
ambiguous_content_source
content_type_mismatch
external_content_conflict
planning_evidence_incomplete
prompt_record_not_found
ambiguous_prompt_record
prompt_record_stale
prompt_record_identity_mismatch
prompt_record_hash_mismatch
prompt_record_json_markdown_mismatch
provider_prompt_profile_mismatch
provider_prompt_payload_conflict
provider_prompt_payload_hash_mismatch
provider_prompt_style_invalid
provider_unavailable
provider_authentication_required
provider_credit_insufficient
provider_tool_not_found
provider_operation_failed
provider_result_not_found
ambiguous_provider_result
attempt_limit_reached
generation_record_collision
generation_record_write_failed
generation_index_write_failed
download_handoff_incomplete
~~~

Failure does not invoke download, evaluation, promotion, or another provider.

## 15. Domain Adapter Extension Contract

A domain generation guide must declare:

~~~text
adapterId and version
artifactType
contentDomain
provider
providerPromptProfile
providerTool and page
canonicalContentSourceRule
imagePolicyRule
expectedStructureProfile
generationBriefFields
providerPromptPayloadContract
providerSettings
backgroundPolicy
resultCountOrNamedOutputContract
provisionalSelectionRule
attemptLimit
expectedDownloadRoles
downloadWarnings
~~~

Adapter validation:

1. artifactType is unique.
2. Provider and tool are exact and cannot be substituted.
3. Canonical content can be resolved without external absolute paths.
4. Generation prompt rules are separated from evaluation scoring.
5. Provider result references are sufficient for a later download task.
6. The adapter defines single-image or set expectations.
7. Legacy download/evaluation/copy steps are explicitly deferred.
8. No project file is written during generation.
9. providerPromptProfile matches provider and the adapter defines only the
   matching providerPromptPayload branch.

## 16. Validation Checklist

- [ ] The external request contains generalized content facts only.
- [ ] One parent and one execution owner exist for the artifact.
- [ ] The execution task did not create a nested task.
- [ ] One exact ready adapter and provider were resolved.
- [ ] Canonical content conflicts were not hidden.
- [ ] One immutable ready prompt record matches current content and routing.
- [ ] providerPromptProfile and the single populated provider payload branch
      match the routed provider.
- [ ] PixelLab fieldPrompts or ImageGen scenePromptOriginal was submitted
      verbatim from the prompt record without conversion.
- [ ] PixelLab and ImageGen were not substituted for each other.
- [ ] Exact provider-native payloads and hashes, settings, attempts, cost
      evidence, and result refs are preserved.
- [ ] Provisional selection is not labeled as evaluated approval.
- [ ] generated_image_generation_v1 and generation_index.json agree.
- [ ] Record visibility and cross-task accessibility are not inferred.
- [ ] downloadHandoff identifies every expected artifact role.
- [ ] No downloaded file path was fabricated.
- [ ] No download, evaluation, project copy, Slack, Unity, Git, or deployment
      occurred.

## 17. Related Prompt

~~~text
AgentDocs/task-prompts/content/GeneratedImageGenerationPrompt.md
~~~
