# ImageGen Generated Media Pipeline Guide

## Purpose

Guide Type: provider-level workflow index. New Generated Media requests use
ImageGen only and route to exactly one execution role:

```text
character_single_image -> ImageGenCharacterImagePipelineGuide.md
icon_single_image      -> ImageGenIconPipelineGuide.md
animation              -> ImageGenAnimationPipelineGuide.md
```

This guide does not route by semantic similarity and does not combine stage
ownership. Routing is owned by GeneratedMediaRequestRoutingGuide.md and the v2
registry. Provider prompt authoring and generation are separate tasks;
preservation/packaging and evaluation/promotion remain later tasks.

## Required Contract

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
```

Every current prompt/generation record uses `provider=imagegen`. PixelLab
fallback is prohibited. Legacy v1 ImageGen stage/battle and PixelLab records
remain readable only under their original immutable contracts.

## Role Registry

| Current role | Authoring guide | Generation result | Preservation profile |
| --- | --- | --- | --- |
| character single image | `ImageGenCharacterImagePipelineGuide.md` | one provider-result set for one approved viewpoint | `character_single_image_v2` |
| icon single image | `ImageGenIconPipelineGuide.md` | one provider-result set for one icon | `icon_single_image_v2` |
| animation | `ImageGenAnimationPipelineGuide.md` | one coherent master for one animationRequestId | `animation_gif_frame_set_v2` |

Generation records settings, attempts, and refs only. It cannot download,
extract, save GIF/PNG, package, evaluate, or promote.

All three generation roles use `providerTool=imagegen` through
`providerInterface=configured_imagegen_capability`. Before an external call,
they require approval matching prompt/settings scope, validate `maxCost` and
`maxAttempts`, and check a deterministic idempotency key. An identical complete
result is reused without a new call; an active duplicate blocks. Attempts and
avoided calls record `costEvidence` so billing decisions are auditable.

## Failure and Validation

Use the typed blockers and readiness conditions in
GeneratedMediaImageGenOnlyContractGuide.md. A current route fails when provider
is not ImageGen, a role is not one of the three rows, or any required type
contract is missing. Animation additionally fails unless exactly one scalar
animationRequestId is present.

The shared external-call blockers are
`missing_provider_execution_approval`, `provider_cost_not_approved`,
`retry_limit_exceeded`, and `duplicate_provider_call_risk`. Failure output must
include providerCalled, costEvidence, requiredDecision and safeToRetry.

## Related Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenIconPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenIconGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md
```
