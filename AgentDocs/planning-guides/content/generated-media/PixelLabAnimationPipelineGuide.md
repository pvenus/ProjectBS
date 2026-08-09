# PixelLab General Animation Pipeline Guide

## 1. Purpose and Scope

Guide Type: workflow/pipeline. This guide owns character-independent general
animation/VFX prompt conversion and PixelLab reference-image/animation-sheet
generation. Download/extraction/package sealing is a separate task.

It does not own character animations, gameplay effect planning, damage/state
logic, targeting, collision, or runtime movement.

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
AgentDocs/planning-guides/skill/SkillImageGenerationGuide.md
AgentDocs/planning-guides/skill/SkillImageDownloadGuide.md
```

Legacy skill guides are provider/profile references only. `domainType` and an
approved animation profile replace skill-specific execution prompts.

Exact supported domain/animationProfile pairs are owned only by
`generated_media_authoring_profile_registry_v1`. This guide must not accept an
unregistered pair by similarity.

Priority is ContentFolderStructureGuide for storage, PixelLabPipelineGuide for
stage ownership, the authoring profile registry for exact route pairs, planning
handoff for approved sequence/runtime meaning, this guide for provider/adapter
specialization, then legacy evidence. Conflict blocks with
`reference_contract_conflict`.

## 3. Required Input

```text
assetType=general_animation
domainType and animationProfile
sourcePlanningFiles and snapshot
animationSubject
requiredElements and prohibitedElements (both non-empty)
sequenceStages (non-empty ordered list)
loopMode
frameContract including sheet/extraction order
runtimeBoundary.generatedMotion
runtimeBoundary.runtimeOwnedMotion
referenceImageContract
```

Missing sequence, loop, frame, runtime-boundary or reference contract blocks
before prompt authoring. The pipeline does not invent anticipation, impact,
dissipation, loop behavior or runtime motion.

## 4. Prompt Authoring

Use `pixellab_animation_prompt_v1` with separate exact provider fields:

```text
reference_image_description
animation_action
```

The reference field describes the approved starting visual state. The action
field expresses only the supplied ordered sequence and loop/ending behavior.
Runtime-owned translation, rotation or targeting must not be rendered into the
sheet unless the handoff explicitly assigns it to generatedMotion.

## 5. Provider Generation

- use only the animation tool/version named by the approved profile;
- generate and record the reference result before the animation result;
- submit saved field prompts without rewriting;
- record provider refs/settings/attempts and stop with
  `pixellab_general_animation_sheet_v1` preservation handoff;
- do not download, inspect sheet bytes, extract, hash, or package.

## 6. Preservation Adapter

The separate common preservation/package task must:

- preserve the original reference PNG and original animation sheet;
- validate sheet dimensions, rows, columns, cell size and usable frame count;
- extract lossless frames in row-major order without altering source bytes;
- create one `paired_sheet_animation` package containing reference, sheet and
  ordered frames.

## 7. Failure Types

```text
missing_animation_profile
unsupported_animation_profile
missing_sequence_specification
invalid_loop_mode
missing_frame_contract
missing_runtime_boundary
missing_reference_contract
reference_generation_failed
animation_generation_failed
reference_download_failed
sheet_download_failed
sheet_contract_mismatch
frame_extraction_failed
frame_count_mismatch
frame_order_invalid
evaluation_package_failed
```

Generation failures stop at provider/result-record errors. Download, sheet,
extraction, and package errors belong to preservation and do not trigger
automatic provider generation.

## 8. Validation and Boundary

- the subject is character-independent;
- every visual stage maps to external evidence;
- prompt authoring and generation were separate;
- generation has no downloaded paths or package identity;
- preservation validates reference/sheet hashes and frame order;
- runtime-owned behavior was not generated;
- no evaluation, promotion, Slack, Unity, Git, or deployment occurred.

## 9. Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/PixelLabAnimationPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabAnimationGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md
```
