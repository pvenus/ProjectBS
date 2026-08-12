# ImageGen Icon Single-Image Pipeline Guide

## Purpose

This current v2 pipeline owns domain-neutral ImageGen icon prompt authoring and
generation. Skill/item differences come only from exact registry profiles.

## Authority and Scope

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
```

Approved planning owns identity and symbolism. The current contract owns
readiness, the registry owns the exact skill/item profile row, and this guide
owns only icon-specific ImageGen translation/generation. Missing meaning or
technical fields block without inference.

## Contract

Read `GeneratedMediaImageGenOnlyContractGuide.md`. Require
`assetType=icon_single_image`, identityConsistencyLock, exact icon profile,
single-image layout, background/no-shadow, outline, and visual-center anchor.
Authoring writes one `generated_media_prompt_v3`; generation writes one
`generated_media_generation_v2` with `structureProfile=icon_single_image_v2`.

Do not decide icon symbolism or copy skill/item prompts. Missing fields use the
current typed blockers. Generation stops at provider refs.

The only provider interface is `providerTool=imagegen` through
`providerInterface=configured_imagegen_capability`. Generation requires an
approval whose scope matches prompt/settings, enforces `maxCost` and
`maxAttempts`, checks the deterministic idempotency key before billing, and
records `costEvidence`. Completed identical work is reused; an active duplicate
blocks.

## Input, Output, State, and Validation

One valid v2 route/handoff becomes one prompt v3 record; one ready prompt
becomes one generation v2 record plus preservation handoff. State is
`routed -> authored -> generated -> preservation_pending`. Validate the exact
row, ImageGen provider, identity/visual-center, display/safe area,
background/outline, and `icon_single_image_v2`. No provider fallback,
packaging, evaluation, promotion, or Git work occurs.

Generation blockers additionally include
`missing_provider_execution_approval`, `provider_cost_not_approved`,
`retry_limit_exceeded`, and `duplicate_provider_call_risk`. Blocked output
includes providerCalled=false, costEvidence, requiredDecision and safeToRetry;
success includes record ID/hash, attempts, refs, costEvidence, idempotencyKey,
preservation handoff and nextStep.

## Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenIconPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenIconGenerationPrompt.md
```
