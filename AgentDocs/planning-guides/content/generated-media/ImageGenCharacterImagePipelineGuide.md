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

Authoring also requires the exact approved ProjectBS character expression
profile as a closed discriminated union. Only the restrained and animation-
ready profiles require non-empty positive/negative lock arrays.
For either of those two legacy lock-array profiles, authoring resolves the
canonical payload from the visual guide, recomputes its RFC 8785 JCS canonical
JSON UTF-8 SHA-256, persists its exact key/payload/hash, and produces separate
non-empty positive and negative style locks. It maps every
lock to authority evidence, and includes both verbatim in the copy-ready
ImageGen prompt. The pipeline rejects photographic/photorealistic/cinematic
portrait, realistic pores, lens/DOF/bokeh, volumetric portrait light,
painterly/PBR 3D render, and western-fantasy realism. `stylized` alone is not a
valid style contract. A planning/profile conflict blocks rather than silently
restyling the approved design.

When approved planning explicitly selects
`projectbs_character_animation_ready_minimal_ink_line@1.0.0`, authoring also
validates the closed proportion projection, animation-safe detail budget,
color/value budget, and mandatory authoring projection. The planning-bound
head-count and head-height values must be exact or narrower inside 3.75-4.25
heads and 24-27 percent, limbs must be shortened/simplified, and every required
projection must have exact planning evidence. Prompt prose must include all
profile locks and bound values without allowing dense material prose to
override simplification. Missing/out-of-range/budget/evidence failures use the
exact current contract tokens and write no prompt record.

When planning selects `projectbs_character_sparse_ink_pastel_motion@1.0.0`,
authoring projects its exact eight-member payload and hash. The main image uses
35-45 percent contour/internal-boundary omission, no closed fill, no more than
18 percent pigmented area, 4-7 accents, the exact two-family faded palette,
3.75-4.25 heads, and stable identity anchors. Generation applies the sparse
contour, pigment, and identity-anchor gates before any provider access.
Its visual-brief lock arrays are empty compatibility members; authoring instead
requires complete eight-member projection and evidence coverage. Missing or
mismatched sparse projection uses only the four sparse authoring tokens and
never a `missing_*_style_lock` token.

Generation performs a separate no-submit semantic preflight over the immutable
prompt record. Any allowance for greater-than-4.25-head or naturalistic
seven-to-eight-head anatomy, dense realistic detail, or nonminimal color/value
treatment uses the three `character_generation_*_gate_failed` tokens. A failed
gate has `providerCalled=false`, `submitCount=0`, and `cost=0`; generation may
not repair or reinterpret the prompt.

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

Alternatively, an exact current-user approval or a valid authenticated standing
automatic preview policy may select the isolated
`hosted_builtin_preview_v1` lane from contract section 6.1.1. It permits one
built-in ImageGen submit and zero retries for this single image, records
unavailable descriptor/settings/cost evidence truthfully, and ends at a
non-evaluated, non-promotable preview record. It never returns a generation-v2
or preservation handoff. Any promotable run continues to require the complete
descriptor, approval, cost, generation-v2, and preservation contracts.
The automatic branch derives its exact-scope attestation only after final
prompt/reference/settings hashes pass. It cannot widen content scope, approve a
second submit, or replace app-authenticated current-user instruction evidence.

## Input, Output, State, and Validation

One valid v2 route/handoff becomes one prompt v3 record, one raw copy-ready
Markdown file, one exact index entry, and one detached generation handoff. One ready prompt
becomes one generation v2 record plus preservation handoff. State is
`routed -> authored -> generated -> preservation_pending`; a blocker writes no
ready record. Validate the exact route, snapshot, profile, evidence and hashes,
one approved viewpoint, ImageGen provider, `character_single_image_v2`, and
the selected union branch: non-empty lock-array coverage for either lock-array
profile, or exact eight-member projection/evidence coverage with empty lock
arrays for sparse. No planning, packaging, evaluation, promotion, or
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
