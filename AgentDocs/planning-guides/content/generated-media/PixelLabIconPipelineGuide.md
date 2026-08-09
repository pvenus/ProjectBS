# PixelLab Icon Pipeline Guide

## 1. Purpose and Scope

Guide Type: workflow/pipeline. This guide owns one domain-neutral PixelLab icon
flow for skill, item, and registered future domains. Domain differences enter
through `domainType`, `iconProfile`, and external planning facts—not separate
execution prompts.

## 2. Required References

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md
AgentDocs/planning-guides/item/ItemIconGenerationGuide.md
```

The skill/item guides are temporary profile evidence. This guide owns the new
execution pipeline; domain evaluation rubrics remain separate.

Exact supported domain/iconProfile pairs are owned only by
`generated_media_authoring_profile_registry_v1`. This guide must not accept an
unregistered pair by similarity.

Priority is ContentFolderStructureGuide for storage, PixelLabPipelineGuide for
stage ownership, the authoring profile registry for exact route pairs, planning
handoff for approved meaning, this guide for icon provider/adapter
specialization, then legacy profile evidence. Conflict blocks with
`reference_contract_conflict`.

## 3. Required Input

```text
assetType=icon
domainType
contentId
sourcePlanningFiles and immutable snapshot
iconProfile ID/version
subjectIdentity and semanticEffect
requiredElements (non-empty)
prohibitedElements (non-empty)
exactCountElements when applicable
backgroundPolicy
targetDisplayContract
```

Missing profile or design facts blocks. The pipeline does not derive skill
effects, item meaning, grade symbolism, exact counts, or background necessity.

## 4. Prompt Authoring

Use `pixellab_icon_prompt_v1`. Write short fielded PixelLab text in this order:

```text
dominant icon silhouette/subject
approved direction or composition
approved essential visual effect
approved palette/material/background policy
concise prohibited objects
```

Settings, dimensions, frame/background normalization and evaluator language do
not belong in copy-ready prompt prose unless the exact PixelLab field requires
them.

## 5. Provider Generation

- use Create UI elements (Pro) and the profile's exact supported fields;
- submit the immutable prompt record verbatim;
- record settings, attempts, cost and every variation result ref;
- select a provisional source only by the profile's deterministic provider
  operation rule, never by formal evaluation;
- write `pixellab_icon_original_png_v1` and `single_image` in the preservation
  handoff, then stop;
- do not download, hash, or package.

## 6. Preservation Adapter

The separate common preservation/package task must:

- download the selected original PNG, not a preview or browser thumbnail;
- preserve the original byte stream and SHA-256;
- do not crop, resize, frame, recolor or normalize during preservation;
- build one `single_image` evaluation package.

Provider variation selection remains `provisional_not_evaluated`. Evaluation
may fail it later.

## 7. Output and Failure

Generation outputs a generation record and provider-ref handoff. Preservation
outputs the sealed package. Their failures and retries remain independent.

```text
missing_icon_profile
unsupported_icon_profile
missing_icon_subject
missing_required_elements
missing_prohibited_elements
prompt_record_stale
provider_variations_missing
ambiguous_provider_result
selected_source_download_failed
source_not_original
source_hash_mismatch
evaluation_package_failed
```

## 8. Validation and Boundary

- no skill/item-specific execution prompt was created or selected;
- domainType/profile fully explains domain rendering differences;
- planning was translated, not supplemented;
- generation contains no downloaded path or package ID;
- preservation contains exactly one selected original PNG;
- staging and project target differ;
- no evaluation, promotion, Slack, Unity, Git, or deployment occurred.

## 9. Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabIconGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md
```
