# PixelLab Character Pipeline Guide

## 1. Purpose and Scope

Guide Type: workflow/pipeline. This guide owns PixelLab character main-image
and requested character-animation prompt conversion and provider execution. It
also declares character-specific adapters used later by the common
preservation/package task; it does not run them during generation.

It does not design character identity or appearance, derive actions, evaluate
results, promote assets, build Unity data, perform Git, or deploy.

## 2. Required References

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/character/CharacterGenerateImage.md
AgentDocs/planning-guides/character/CharacterGenerateAnimation.md
AgentDocs/planning-guides/character/CharacterAnimationDownloadGuide.md
```

The three character guides are legacy provider behavior references only. This
guide replaces their execution ownership.

Priority is ContentFolderStructureGuide for storage, PixelLabPipelineGuide for
stage ownership, planning handoff for approved meaning, this guide for
character provider/adapter specialization, then legacy guides as evidence.
Conflict blocks with `reference_contract_conflict`; never merge behaviors.

## 3. Two Independent Runs

### 3.1 Main character run

```text
character_main_image planning handoff
-> character prompt authoring record
-> PixelLab main character generation
-> generation record and provider-ref handoff; stop
-> ordered eight-way export and extraction
-> ordered_rotation_set evaluation package
-> evaluation request handoff
-> stop
```

Exactly eight ordered directions are required. A preview grid is not an export.
Preserve the provider original and each extracted rotation with hashes.

### 3.2 Character animation run

```text
character_animation planning handoff
-> verify approved PixelLab character identity
-> one prompt record per animationRequest
-> execute only requested action
-> generation record and provider-ref handoff; stop
-> character animation export and frame extraction
-> ordered_frame_set evaluation package
-> evaluation request handoff
-> stop
```

Do not assume main-image PASS inside this task. When planning policy requires an
approved main image, `approvedCharacterPackageId` is mandatory external
evidence. The evaluator remains a separate task.

## 4. Prompt Authoring Contract

Main-image prompt profile:

```text
pixellab_character_prompt_v1
character_description field only plus tool-supported negative field when approved
```

Animation prompt profile:

```text
pixellab_character_animation_prompt_v1
one action field per animationRequest
```

Prompt text translates only `characterIdentity`, `appearanceSpecification`,
requiredElements, prohibitedElements, and the exact requested action. It must
not read combat/skill lore to invent Attack, Idle, or Move descriptions.

## 5. Provider Generation Contract

- use the exact PixelLab Create Character workflow;
- record provider character ID, tool/page, settings, seed when exposed, cost,
  attempts and result refs;
- stop after a generation record and preservation handoff;
- do not download, export, extract, hash, or package in this task.

## 6. Preservation Adapter Contract

The separate common task uses `pixellab_character_rotation_export_v1` for main
images and `pixellab_character_animation_export_v1` for animations. It must:

- main image export must resolve eight distinct ordered rotations;
- animation export must resolve the requested animation name, ordered
  directions and ordered frames;
- apply mirroring only when the external request explicitly permits it;
- preserve archives/original exports before extraction;
- reject thumbnails, browser previews and silently repaired frames;
- extracted filenames are deterministic from manifest identity, not provider
  display names.

## 7. Animation Request Rules

Allowed animationType values are `attack`, `idle`, and `move`, but they are not
a required fixed set. Execute exactly the supplied list. For each request:

- require actionSpecification, directionOrder and frameContract;
- use animationRequestId as the stable package/action identity;
- do not rename or merge two requests;
- do not reuse a prompt across changed action specs;
- one failed request does not authorize generating omitted action types.

## 8. Output and Handoff

Generation returns provider refs plus the matching adapter/profile handoff.
Later preservation returns one `ordered_rotation_set` main package or one
`ordered_frame_set` package per requested animation.

## 9. Failure Types

```text
missing_character_identity
missing_appearance_specification
invalid_rotation_contract
character_provider_identity_missing
approved_character_evidence_missing
missing_animation_requests
invalid_animation_request
unsupported_animation_type
animation_request_not_in_handoff
character_not_found_in_pixellab
rotation_export_incomplete
rotation_order_invalid
character_animation_export_failed
direction_missing
frame_count_mismatch
frame_order_invalid
character_identity_drift
evaluation_package_failed
```

Generation failures end at provider/result-record errors. Export, order,
extraction, and package failures belong only to preservation and never trigger
automatic regeneration.

## 10. Validation

- planning facts are immutable and no action/design fact was inferred;
- prompt and generation tasks are separate;
- only requested animations were called;
- main set has eight ordered unique rotations;
- animation sets match exact request/direction/frame order;
- generation records contain no downloaded files or package identity;
- preservation records validate originals and extracted hashes;
- package source and project target are separate;
- no evaluation, promotion, Slack, Unity, Git, or deployment occurred.

## 11. Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md
```
