# ImageGen Character Single-Image Pipeline Guide

## Purpose

This current v2 pipeline owns provider-prompt authoring and ImageGen generation
for one approved character viewpoint. It does not own planning, packaging,
evaluation, or promotion.

## Authority and Scope

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
```

Master Concept and approved planning own identity/design. The current contract
owns schema/readiness, the registry owns the route, this guide owns only
character-specific provider translation/generation, and preservation owns
media bytes. A conflict returns the exact blocker; do not invent a compromise.

## Contract

Read `GeneratedMediaImageGenOnlyContractGuide.md`. Require
`assetType=character_single_image`, exact identityConsistencyLock, viewpoint,
pose, framing, canvas, target display size, safe area, background/no-shadow,
outline, and pelvis/root plus ground-contact anchor. Authoring writes one
`generated_media_prompt_v3`; generation submits it unchanged and writes one
`generated_media_generation_v2` with `structureProfile=character_single_image_v2`.
For this type, GeneratedMediaRecordGuide.md::Prompt v3 closes the prompt hash
payload, record and nested members, raw Markdown body, prompt index, atomic
publication, and detached `generated_media_generation_handoff_v2`. Generation
accepts that handoff only after recomputing its JSON/Markdown/index raw hashes
and exact projections; it never repairs or normalizes authoring artifacts.

Eight-way, rotations, direction arrays, and `ordered_rotation_set` are invalid
for a current request. Missing fields return the exact typed blocker owned by
the current contract. Generation stops at provider refs and hands off to
preservation.

Authoring also requires the active ProjectBS character ink-line style profile.
It resolves the canonical payload from the visual guide, recomputes its RFC
8785 JCS canonical JSON UTF-8 SHA-256, persists its exact key/payload/hash, and
produces separate non-empty positive and negative style locks. It maps every
lock to authority evidence, and includes both verbatim in the copy-ready
ImageGen prompt. The pipeline rejects photographic/photorealistic/cinematic
portrait, realistic pores, lens/DOF/bokeh, volumetric portrait light,
painterly/PBR 3D render, and western-fantasy realism. `stylized` alone is not a
valid style contract. A planning/profile conflict blocks rather than silently
restyling the approved design.

The only provider interface is `providerTool=imagegen` through
`providerInterface=configured_imagegen_capability`. Its contract 6.1 non-submit
preflight must expose immutable capability/settings/cost descriptor versions,
defaults-resolved exact closed settings, a tagged estimate, their canonical
hashes, and an immutable evidence reference without crossing the submit
boundary. The execution role binds the descriptor and settings into the scope,
computes and presents that scope hash, then requires its closed approval and
enforces pre-submit drift detection, tagged `maxCost`, cumulative `maxAttempts`,
and projection equality. It
checks the deterministic idempotency key before billing, reuses an identical
completed result, blocks an identical active call, and records `costEvidence`.

## Input, Output, State, and Validation

One valid v2 route/handoff becomes one prompt v3 record, one raw copy-ready
Markdown file, one exact index entry, and one detached generation handoff. One ready prompt
becomes one generation v2 record plus preservation handoff. State is
`routed -> authored -> generated -> preservation_pending`; a blocker writes no
ready record. Validate the exact route, snapshot, profile, evidence and hashes,
one approved viewpoint, ImageGen provider, `character_single_image_v2`, and
positive/negative style-lock presence, evidence coverage and exact prompt
inclusion. No planning, packaging, evaluation, promotion, or
Git work occurs.

Generation blockers additionally include the exact contract 6.1-6.2 approval,
scope, cost, attempt, duplicate-call, and provider-operation failure tokens.
A blocked output
returns status, failureType, missingFields, providerCalled=false,
costEvidence, requiredDecision, and safeToRetry. A successful output returns
the generation record ID/hash, attempts, result refs, costEvidence,
idempotencyKey, preservation handoff, and nextStep.

## Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md
```
