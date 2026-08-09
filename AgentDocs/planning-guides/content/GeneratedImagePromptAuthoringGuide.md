# Generated Image Prompt Authoring Guide

> Migration status: compatibility authority for legacy `artifactType` and
> `generated_image_prompt_v1` only. New callers use
> `AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md`,
> `AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md`,
> and the routed provider child guide.

## Master Concept Reference

Before using this document, read and apply:

~~~text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
~~~

## 1. Purpose

This guide creates and saves the provider-ready prompt package required by the
general generated-image creation pipeline.

~~~text
generalized content request
-> internal canonical content resolution
-> provider and domain adapter resolution
-> generation brief
-> provider-discriminated prompt payload
-> immutable generated_image_prompt_v1 record
-> later generation execution task
~~~

This task authors prompts only. It does not open PixelLab, call ImageGen,
generate or download media, evaluate an image, copy to the project, publish to
Slack, modify Unity assets, perform Git work, or deploy.

## 2. Required References and Authority

Always read:

~~~text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
~~~

Then read exactly one domain generation adapter selected by
GeneratedImageGenerationPipelineGuide.md.

Authority:

1. Canonical planning/content data owns identity and intended meaning.
2. GeneratedImageGenerationPipelineGuide.md owns artifact/provider routing.
3. This guide owns prompt record identity, common brief,
   provider-discriminated payload schema, persistence, staleness, and handoff.
4. The routed domain generation guide owns visible subject, composition,
   background, provider-specific wording order, technical settings intent,
   exclusions, and prompt length constraints.

Do not reconcile a material conflict by inventing a compromise. Stop with
prompt_authoring_contract_conflict and identify both sources.

## 3. External Request Contract

One request authors one prompt package for one logical artifact.

### 3.1 Allowed generalized fields

~~~text
requestId: optional stable request id
artifactType: required supported generalized type
contentId: required canonical content id
contentName: optional display name
contentSummary: optional concise gameplay or narrative meaning
visualIntent: optional desired moment or emphasis
requiredElements: optional semantic must-show list
forbiddenElements: optional semantic exclusion list
contextTags: optional generalized key/value facts
priorPromptRecordId: optional stable non-path record id for revision
revisionReason: optional concise reason when revising
~~~

### 3.2 Internally resolved fields

The caller must not need to provide:

~~~text
repositoryRoot
planning or content source path
provider, tool, or URL
domain generation guide path
prompt output path
PixelLab or ImageGen settings
dimensions, variation count, frame count, or aspect ratio
background mode or style phrase
negative prompt format
download, evaluation, or project path
~~~

External facts supplement canonical sources and never silently override them.
Ignore external absolute paths and provider choices.

## 4. Canonical Source Resolution

Resolve artifactType through the generation adapter registry, then resolve
contentId:

1. AgentDocs/planning-data canonical planning;
2. Assets/Contents/{ContentDomain}/json canonical content;
3. domain-documented legacy source while migration is pending.

Record:

~~~text
canonicalContentSources
source revisions or SHA-256
planningOriginalContent
displayContent
acceptedExternalFacts
rejectedExternalFacts
contentSnapshotHash
~~~

contentSnapshotHash is calculated from ordered canonical source identities,
revisions/hashes, routed adapter ID/version, artifactType, and contentId.

Stop on ambiguous content, type mismatch, missing required planning evidence, or
a material external conflict.

## 5. Provider and Domain Routing

Use the registry in GeneratedImageGenerationPipelineGuide.md without copying it
into external input.

Prompt authoring supports every registry row whose generation adapter is ready:

- PixelLab prompt packages for skill_icon, item_icon, skill_animation,
  character_image, and character_animation;
- ImageGen prompt packages for story_popup_main_image and battle_background;
- reuse and none policies produce no provider prompt and record the policy
  handoff instead.

Do not substitute PixelLab and ImageGen. Do not author a generic fallback prompt
when a domain adapter is missing or incomplete.

## 6. Common Generation Brief

Create a concise evidence-based brief before writing provider text:

~~~text
artifactUsage
contentIdentity
gameplayOrNarrativeIntent
currentMomentOrActivation
primarySubjectOrSilhouette
directionAndComposition
requiredElements
supportingElements
forbiddenElements
likelyWrongObjects
styleAndMaterial
palette
backgroundPolicy
expectedStructureProfile
technicalExpectation
~~~

Rules:

- requiredElements are independently observable;
- supportingElements never compete with the primary subject;
- likelyWrongObjects are derived from known model failure patterns and domain
  meaning, not a generic long negative list;
- backgroundPolicy is explicit: required contextual background, constrained
  symbolic background, transparent, or domain-approved none;
- exact counts remain in prompt text only when the domain generation guide
  requires provider generation to own them;
- do not expand a concise domain contract into repeated coordinates and
  exclusions;
- preserve planningOriginalContent separately from the derived brief.

## 7. Provider-Discriminated Prompt Model

PixelLab and ImageGen share only the prompt-record envelope, evidence, brief,
and settings provenance. Their copy-ready prompt payloads use different schemas
and writing styles. Set exactly one providerPromptProfile and exactly one
matching providerPromptPayload branch.

~~~text
PixelLab -> pixellab_fielded_pixel_prompt_v1 -> providerPromptPayload.pixelLab
ImageGen -> imagegen_composed_scene_prompt_v1 -> providerPromptPayload.imageGen
~~~

Do not translate or auto-convert one provider payload into the other. A provider
change requires a new immutable prompt record and a new provider-native prompt.

### 7.1 PixelLab: fielded pixel prompt

PixelLab copy text is concise, literal, silhouette/action-first, and mapped to
the exact fields exposed by the routed tool. It is not cinematic prose.

~~~text
providerPromptPayload.pixelLab:
  fieldPrompts[]:
    fieldId
    fieldRole
    toolField
    language
    required
    order
    textOriginal
    sourceFacts
    constraintIds
~~~

Allowed fieldRole examples are primary_description, negative_description,
reference_image_description, animation_action, character_description,
movement_action, attack_action, and idle_action. A domain adapter may add a
stable role only when it names the matching PixelLab UI field.

Writing rules:

- lead with the dominant pixel silhouette, object, pose, or action;
- use short concrete sentences and observable shape, direction, palette, and
  essential effect terms;
- keep the skill or item effect simple so secondary details do not become the
  generated subject;
- use only fields actually supported by the routed PixelLab tool;
- keep size, frame count, view, direction selector, seed, and no-background
  controls in providerSettingsIntent unless the adapter proves the field itself
  requires the text;
- do not repeat settings, long coordinate lists, cinematic camera prose, or
  ImageGen-style scene paragraphs across fields;
- do not invent a reference-image field when the domain guide prohibits one.

### 7.2 ImageGen: composed scene prompt

ImageGen copy text is one cohesive visual direction that establishes a complete
scene. It must not be written as disconnected PixelLab UI phrases.

~~~text
providerPromptPayload.imageGen:
  sceneSections[]:
    sectionId
    sectionRole
    order
    textOriginal
    sourceFacts
    constraintIds
  scenePromptOriginal
  language
~~~

Build scenePromptOriginal in this order:

1. core subject, action, and exact depicted moment;
2. composition, camera, scale, and spatial relationships;
3. environment and background when required, or an intentional simple/flat
   background policy when a detailed background is unnecessary;
4. art direction, material, palette, and lighting;
5. concise exclusions and clean-image requirements.

sceneSections preserve evidence and auditability; scenePromptOriginal is the
only copy-ready submission text and must be an exact composition of those
sections under the adapter's documented separator/newline rule.

Writing rules:

- keep the main subject and current moment more prominent than atmosphere;
- use natural, cohesive scene language with enough spatial context for a full
  illustration;
- describe a background only when it supports the intended image; otherwise
  state the approved simple, flat, transparent, or absent policy clearly;
- do not use PixelLab UI field names, pixel-icon shorthand, local paths,
  filenames, evaluator instructions, or implementation details;
- include exclusions in scenePromptOriginal unless the active adapter
  explicitly declares a separate supported control.

### 7.3 Payload exclusivity and hashing

- PixelLab records set providerPromptPayload.imageGen to null.
- ImageGen records set providerPromptPayload.pixelLab to null.
- reuse_requested and skipped records may set both branches to null only when
  imagePolicy proves that no provider submission will occur.
- providerPromptPayloadHash is SHA-256 over the normalized copy-ready payload:
  ordered `{toolField, textOriginal}` pairs for PixelLab, or the exact
  scenePromptOriginal for ImageGen.
- normalize line endings to LF and preserve all other characters. Do not hash
  audit commentary or providerSettingsIntent into this value.
- profile mismatch, two populated branches, or a changed payload hash blocks
  generation.

## 8. Prompt Quality Gates

Validate before saving:

### 8.1 Identity and evidence

- artifactType, contentId, provider, adapter, and canonical source agree;
- each required visual statement has sourceFacts;
- planningOriginalContent and displayContent are preserved separately;
- no unsupported character, object, reward, location, element, or cultural
  detail is invented.

### 8.2 Visual hierarchy

- one dominant subject, action, or silhouette is explicit;
- direction/composition is unambiguous when required;
- required effects are connected to the primary subject;
- secondary/background detail remains subordinate;
- background is described only when required or intentionally constrained;
- forbidden objects are concise and artifact-specific.

### 8.3 Provider fitness

- providerPromptProfile matches the routed provider;
- every ready_for_generation record has exactly one populated
  providerPromptPayload branch; reuse/skip follows Section 7.3;
- PixelLab fieldPrompts match real routed tool fields and PixelLab writing
  style;
- ImageGen scenePromptOriginal follows the composed scene order and ImageGen
  writing style;
- wording order, language, length, dimensions/settings intent, and output
  structure match the domain guide;
- prompt contains no local path, project target, score, PASS language, Slack,
  Git, or Unity instruction;
- prompt does not ask the provider to perform deterministic post-processing
  owned by a later stage.

### 8.4 Handoff fitness

- every PixelLab fieldPrompt can be copied exactly into its intended tool field,
  or the ImageGen scenePromptOriginal can be submitted as one exact prompt;
- the provider payload hash is calculated from the copy-ready payload only;
- providerSettingsIntent is separate and complete;
- expected provider result roles are declared;
- the generation execution task can use the record without rewriting text.

## 9. Prompt Record Identity and Persistence

Create:

~~~text
promptRecordId =
imgprompt.{artifactType}.{contentId}.{UTC_YYYYMMDDTHHMMSSZ}.{content_snapshot_hash_prefix_12}
~~~

Save:

~~~text
AgentDocs/planning-data/image-prompts/v1/{artifactType}/{contentId}/{promptRecordId}.json
AgentDocs/planning-data/image-prompts/v1/{artifactType}/{contentId}/{promptRecordId}.prompt.md
AgentDocs/planning-data/image-prompts/v1/{artifactType}/{contentId}/prompt_index.json
~~~

The JSON is the machine-readable authority. The Markdown file is a human-
readable, copy-ready rendering of the same provider payload and settings intent.
They must agree exactly.

Do not pre-create artifact folders for failed requests. Never overwrite an
existing prompt record. A revision creates a new promptRecordId linked to
priorPromptRecordId.

## 10. generated_image_prompt_v1 Contract

Required fields:

~~~text
schemaVersion: generated_image_prompt_v1
promptRecordId
requestId
priorPromptRecordId
revisionReason

artifact:
  artifactType
  contentDomain
  contentId
  contentName
  artifactUsage
  expectedStructureProfile

sources:
  canonicalContentSources
  sourceHashesOrRevisions
  planningOriginalContent
  displayContent
  contentSnapshotHash

routing:
  provider
  providerTool
  providerPage
  providerPromptProfile
  domainAdapter
  adapterVersionOrRevision

externalFacts:
  accepted
  rejected

generationBrief
providerPromptPayload:
  pixelLab: fieldPrompts[] | null
  imageGen: sceneSections[], scenePromptOriginal, language | null
providerPromptPayloadHash
providerSettingsIntent
expectedProviderResultRoles
expectedDownloadRoles
imagePolicy
createdAt
author

validation:
  identityEvidence
  domainContract
  providerFitness
  visualHierarchy
  sourceCoverage
  jsonMarkdownEquality
  status
~~~

Prompt record status:

~~~text
ready_for_generation
reuse_requested
skipped
blocked
~~~

Only ready_for_generation may be submitted to a provider.

## 11. Human-Readable Prompt Document

The prompt Markdown contains:

~~~text
Prompt Record ID
Artifact Type / Content ID / Name
Provider / Tool
Domain Adapter
Content Snapshot Hash
Generation Brief
Provider Prompt Profile
Provider Settings Intent
PixelLab Field Prompts in exact field order and verbatim text
  or ImageGen Final Scene Prompt as one verbatim copy block
ImageGen Scene Section Audit outside the copy block when applicable
Provider Prompt Payload Hash
Expected Provider Result Roles
Expected Download Roles
Validation
Revision Link
~~~

Do not mix commentary inside copy-ready text. PixelLab Markdown renders one
copy block per tool field. ImageGen Markdown renders exactly one final scene
prompt copy block; its section audit stays outside that block.

## 12. Staleness and Generation Eligibility

Before generation, the execution task recalculates:

~~~text
contentSnapshotHash
provider route
adapter ID/version or source revision
prompt record JSON SHA-256
JSON/Markdown provider-payload equality
~~~

A prompt is stale when:

- canonical source content changed;
- content ID or artifact type changed;
- provider routing changed;
- providerPromptProfile or populated payload branch does not match routing;
- the domain adapter changed in a way that affects prompt or settings;
- the prompt record or Markdown was edited without a new record;
- JSON and Markdown disagree.

Stale prompts are not silently updated by the generation task. Return
prompt_record_stale and run this authoring task again.

If more than one eligible current record exists and no promptRecordId is
provided, return ambiguous_prompt_record.

## 13. Generation Handoff

Return:

~~~text
nextTask: generation
promptRecordId
promptRecordPath
promptMarkdownPath
promptRecordSha256
artifactType
contentId
provider
providerPromptProfile
providerPromptPayloadHash
domainAdapter
contentSnapshotHash
promptStatus
~~~

The generation task reads the matching providerPromptPayload and
providerSettingsIntent verbatim. It may validate current provider UI and cost,
but it does not rewrite or convert the prompt.

## 14. Failure Types

~~~text
invalid_prompt_authoring_request
unsupported_artifact_type
missing_domain_generation_adapter
incomplete_domain_generation_adapter
prompt_authoring_contract_conflict
repository_not_resolved
ambiguous_content_source
content_type_mismatch
planning_evidence_incomplete
external_content_conflict
unsupported_image_policy
provider_prompt_profile_mismatch
provider_prompt_payload_conflict
provider_prompt_payload_incomplete
provider_prompt_style_invalid
provider_prompt_contract_failed
unsupported_provider_field
prompt_record_collision
prompt_record_write_failed
prompt_markdown_write_failed
prompt_index_write_failed
json_markdown_mismatch
prior_prompt_record_not_found
~~~

Failure creates no placeholder prompt package and performs no provider call.

## 15. Validation Checklist

- [ ] External input contains generalized content facts only.
- [ ] One exact provider and ready domain adapter were resolved internally.
- [ ] Canonical content and external facts were separated.
- [ ] Every required prompt statement has source evidence.
- [ ] Visual hierarchy and background policy are explicit.
- [ ] providerPromptProfile matches the routed provider.
- [ ] A ready_for_generation record has exactly one providerPromptPayload
      branch populated; reuse/skip follows the explicit null exception.
- [ ] PixelLab field prompts match actual tool fields and use concise,
      silhouette/action-first pixel language.
- [ ] ImageGen has one cohesive scenePromptOriginal in the required scene order.
- [ ] Provider settings are not redundantly buried in prompt prose.
- [ ] No path, evaluation, promotion, Slack, Unity, Git, or deployment
      instruction appears in provider prompt text.
- [ ] JSON and Markdown copy-ready provider payloads are byte-for-byte equal
      after documented newline normalization.
- [ ] The record is immutable and revision links are preserved.
- [ ] Generation handoff contains prompt record ID, paths, hash, provider, and
      content snapshot hash.
- [ ] No image generation, download, evaluation, or project copy occurred.

## 16. Related Prompt

~~~text
AgentDocs/task-prompts/content/GeneratedImagePromptAuthoringPrompt.md
~~~
