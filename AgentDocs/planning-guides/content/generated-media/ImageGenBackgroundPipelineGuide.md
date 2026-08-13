# ImageGen Background Single-Image Pipeline Guide

## Purpose and Boundary

Guide Type: current v2 background-only ImageGen workflow. It owns prompt
authoring and generation contracts for one approved stage, battle, or
environment background. It does not own icons, characters, animations,
planning, preservation, evaluation, promotion, or Git work.

## Authority

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
```

Planning owns scene meaning and every visual fact. The registry owns the exact
domain/profile row. This guide only translates an approved background contract
and executes its unchanged prompt.

## Required Contract

Require `assetType=background_single_image`, `domainType=stage|battle|environment`,
an exact registered backgroundProfile and:

```text
sceneContract
composition and viewpoint
horizon and ordered depthLayers
playableOrReadabilityArea
subjectInclusions and subjectExclusions
canvas and aspectRatio
targetDisplay and safeArea
finalBackgroundPolicy
content/scene consistencyLock
scene_composition_anchor with framingRegion and focalDepth
```

Missing scene, era, culture, weather, lighting, landmark, camera, subject or
spatial facts are never inferred. Icon transparent-background defaults,
visual-center anchors, silhouette/outline rules and small-size readability are
forbidden. Character identity locks and animation frame contracts are also
invalid.

## Authoring and Generation

One routed unit produces one `generated_media_prompt_v3` with
`structureProfile=background_single_image_v2`. Authoring writes a provider-
neutral visual brief and one cohesive ImageGen scene prompt; it cannot call the
provider.

Generation submits the stored prompt/settings unchanged through
`providerTool=imagegen` and
`providerInterface=configured_imagegen_capability`. Before any external call it
requires approval whose scope matches prompt/settings, verifies `maxCost` and
`maxAttempts`, and checks the deterministic idempotency key. Identical completed
work is reused without billing; an active duplicate blocks. Every attempted or
avoided call records `costEvidence`. Generation stops at provider refs and a
background preservation handoff.

## State, Failure and Validation

```text
routed -> authored -> generated -> preservation_pending
```

Use the central failure tokens, including:

```text
ambiguous_image_role
missing_background_scene_contract
missing_background_composition
missing_background_viewpoint
missing_background_horizon
missing_background_depth_layer_contract
missing_background_playable_area
missing_background_subject_contract
missing_background_canvas_contract
missing_background_aspect_ratio
missing_background_target_display
missing_background_safe_area
missing_background_consistency_lock
unsupported_background_domain
missing_provider_execution_approval
provider_cost_not_approved
retry_limit_exceeded
duplicate_provider_call_risk
```

Validate one exact registry row, background-only fields, planning evidence and
hashes, scene anchor, prompt/settings/approval hashes, attempts/cost evidence,
and stage separation. No PixelLab route, eight-way contract, icon adapter,
provider fallback, packaging, evaluation, promotion or Git work is allowed.

## Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundGenerationPrompt.md
```
