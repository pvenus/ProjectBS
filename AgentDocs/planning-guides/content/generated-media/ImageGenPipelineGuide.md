# ImageGen Generated Image Pipeline Guide

## 1. Purpose and Scope

Guide Type: workflow/pipeline. This guide owns domain-neutral ImageGen scene
prompt conversion and provider generation. Original preservation/package
sealing is a separate task. Stage and battle differences are profiles
selected by `domainType`; they do not get new execution prompts.

## 2. Required References

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
AgentDocs/planning-guides/stage/PopupEventMainImageCreateGuide.md
AgentDocs/planning-guides/battle/BattleCreateGuide.md
```

Stage/battle guides supply temporary profile rules. External planning owns the
scene, subjects, location and depicted moment.

Priority is ContentFolderStructureGuide for storage, the planning handoff for
approved scene meaning, GeneratedImageGenerationPipelineGuide for the
generation-only boundary, this guide for ImageGen specialization, then legacy
stage/battle evidence. Conflict blocks with `reference_contract_conflict`.

## 3. Required Input

```text
assetType=imagegen_image
domainType and imageProfile
contentId
sourcePlanningFiles and immutable snapshot
depictedMoment
subjects and relationships
environment and backgroundPolicy
composition and camera
aspectRatio
requiredElements and prohibitedElements (both non-empty)
```

Do not reconstruct a scene from a generic story summary. Missing scene facts or
unsupported profile blocks before prompt authoring.

## 4. Prompt Authoring

Use `imagegen_composed_scene_prompt_v1`. Build one cohesive copy-ready prompt
from auditable sections in this order:

1. exact subject, action and depicted moment;
2. composition, camera, scale and spatial relationships;
3. approved environment and background policy;
4. approved art direction, material, palette and lighting;
5. concise prohibited elements and clean-image requirements.

Do not use PixelLab field fragments, local paths, evaluator language, score,
project target, or invented narrative detail.

## 5. Provider Generation

- verify the prompt record and unchanged planning snapshot;
- submit exactly one saved `scenePromptOriginal` to ImageGen;
- record settings, attempts and every provider result reference;
- record `imagegen_original_media_v1` and `single_image` in the preservation
  handoff, then stop;
- do not download, hash, or package.

## 6. Preservation Adapter

The separate common preservation/package task must:

- preserve the selected original returned media without resize, crop,
  recompression, retouching or filename-based inference;
- record selection as provisional and not evaluated;
- create one `single_image` common evaluation package.

ImageGen failure never falls back to PixelLab.

## 7. Output and Failure

Generation outputs provider refs and preservation handoff. Preservation outputs
the sealed single-image package. Each task has independent retry and failure.

```text
missing_image_profile
unsupported_image_profile
missing_scene_specification
missing_required_elements
missing_prohibited_elements
prompt_record_stale
provider_unavailable
provider_operation_failed
ambiguous_provider_result
original_media_download_failed
source_hash_mismatch
evaluation_adapter_missing
evaluation_package_failed
```

## 8. Migration

```text
domainType=stage  + approved story popup profile -> legacy story_popup_main_image
domainType=battle + approved battle background profile -> legacy battle_background
```

Other domains require a registered imageProfile and evaluation adapter. Do not
copy the execution prompt.

## 9. Validation and Boundary

- every prompt statement maps to the immutable planning handoff;
- prompt and generation tasks are separate;
- generation has no downloaded file or package identity;
- preservation has exactly one original primary member;
- staging and project target differ;
- no evaluation, promotion, Slack, Unity, Git, or deployment occurred.

## 10. Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md
```
