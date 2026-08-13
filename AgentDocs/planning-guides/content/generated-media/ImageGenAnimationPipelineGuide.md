# ImageGen Animation Pipeline Guide

## Purpose

This current v2 pipeline owns prompt authoring and ImageGen generation for one
character or skill animation request. Preservation creates the GIF and frames;
evaluation is separate.

## Authority and Scope

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
```

Approved planning owns motion and exact final frames. The router owns request
splitting, the registry owns the exact character/skill row, this guide owns one
request's ImageGen translation/generation, and preservation owns GIF/frame
extraction. Missing meaning or technical contracts block.

## Contract

Read `GeneratedMediaImageGenOnlyContractGuide.md`. Require exactly one scalar
`animationRequestId`, hashed reference image, approved final frame count and
timing/order/loop/key poses, fixed cell, scale lock, vertical-motion policy,
background/no-shadow/outline, anchor, and `masterFirst=true`.

Authoring writes one `generated_media_prompt_v3`. Generation produces one
coherent master result at the final approved frame count and writes one
`generated_media_generation_v2` with
`structureProfile=animation_gif_frame_set_v2`. It cannot oversample, choose a
subset, merge requests, extract frames, or package output.

The profile discriminator fixes the anchor: character animation uses
`pelvis_root_ground_axis`; skill animation uses `effect_origin`. Generation
uses only `providerTool=imagegen` through
`providerInterface=configured_imagegen_capability`. It requires scope-matching
external-call approval, enforces `maxCost` and `maxAttempts`, and checks an
animationRequestId-bearing idempotency key before billing. Identical completed
work is reused, active duplicate work blocks, and every decision records
`costEvidence`.

## Input, Output, State, and Validation

One scalar animationRequestId route/handoff becomes one prompt v3 record. One
ready prompt becomes one coherent-master generation v2 record plus preservation
handoff. State is `routed -> authored -> generated -> preservation_pending`.
Validate the exact ID/reference, final count/timing/order/loop/key poses,
fixed-cell/scale/anchor policies, ImageGen provider, and
`animation_gif_frame_set_v2`. Retry cannot change the motion contract. No
extraction, evaluation, promotion, or Git work occurs.

Generation blockers additionally include
`missing_provider_execution_approval`, `provider_cost_not_approved`,
`retry_limit_exceeded`, and `duplicate_provider_call_risk`. Blocked output
includes the exact animationRequestId, providerCalled=false, costEvidence,
requiredDecision and safeToRetry; success includes record ID/hash, attempts,
refs, costEvidence, idempotencyKey, preservation handoff and nextStep.

## Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md
```
