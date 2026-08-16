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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md
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

When planning selects
`projectbs_character_bold_outline_compressed_detail@1.0.0`, authoring requires
the exact closed planning projection in addition to the canonical payload and
locks. Planning binds 4.0-5.0 heads; outside-silhouette thickness 16-22 px on
1024x1536; a positive internal thickness with outside/internal ratio at least
2; the closed facial mark total/component maxima; primary and conditional
secondary hue anchor sites; coverage at most 35 percent; at most four color
masses; and neutral outline/weapon colors. Generation independently checks
those five semantic groups before any capability or submit boundary. Missing
evidence or prompt prose cannot be repaired downstream. This profile is
single-image-only and cannot be inherited by character animation.

When planning instead selects
`projectbs_character_bold_outline_compressed_detail@2.0.0`, authoring validates
the same inherited anatomy/outline/face/color groups plus exact total/internal/
fold maxima no greater than 64/56/5, optional ochre site classes restricted to
small utility-pouch or travel-accessory anchors, and one closed `inkHalo`
branch. Disabled authorizes no dark background. Enabled requires exact
dark-neutral color, opacity 0.08-0.35, coverage 1-45 percent, centered soft
extent, monotonic fade to edge alpha zero, and no scene, opaque background, or
shadow semantics. Generation and evaluation independently gate these fields;
neither infers them from accepted imagery. The successor remains
single-image-only.

When planning selects
`projectbs_character_open_ink_wash_dynamic_contour@1.0.0`, authoring validates
the exact eleven-member policy projection and both ordered lock arrays. Planning
must bind a 4-5-head young adult targeted at 4.25, never child-coded; 35-55
percent open contour targeted at 45; pressure-variable tactile mok-seon with
brush-start/directional-drag/dry-end phases and directional weight; broad rough
watercolor/pastel with controlled bleed and misalignment outside the line;
separate faded-blue-gray-or-indigo, dusty-gray-brown, and small-muted-ochre
roles; at least 70 percent achromatic/unpainted space in both figure interior
and canvas; removable warm-ivory solid background; and no halo, vignette, scene,
or shadow. Exact Korean/Joseon identity, costume, equipment, weapon, handedness,
and identifying anchors remain planning-owned and must be preserved. The exact
accepted-image SHA is audit-only: absent a reviewed durable project-relative
style-only publication, no reference path/binding is created and the image is
never identity or edit target. This profile is single-image-only.

A new handoff may carry exactly one reviewed durable style-only binding. The
pipeline rehashes its asset, review record, and index at authoring and again at
generation; copies the exact six-member binding into visual brief, prompt
record, and execution scope; and never copies its subject semantics into prompt
prose or identity/equipment evidence. The provider must expose a distinct style
reference role. A generic/identity image input, missing role control, incomplete
binding, or hash/profile drift blocks without provider access.

When a new planning revision instead selects
`projectbs_character_open_ink_wash_dynamic_contour@2.0.0`, authoring uses the
exact nineteen-member successor payload and 9+9 locks without changing v1. It
additionally binds a closed full-body head-count measurement method, a sparse
surface-detail ceiling that forbids modeled faces and individually rendered
armor/garment micro-detail, a spatially uniform `#F2EFE6` background with no
radial or edge darkening, and the seven-gate provider-output conformance order.
Generation rejects weakened provider prose before submit. After a one-shot
preview returns, it performs only the closed non-scoring observable triage and
returns one compact conformance receipt. A failed or insufficient gate is
`stop_no_retry_not_final`; it does not authorize a retry, edit, evaluation,
preservation, promotion, or second provider call.

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
the selected union branch: non-empty lock-array coverage for any registered
lock-array profile, including the complete bold-outline projection, or exact
eight-member projection/evidence coverage with empty lock
arrays for sparse. The open ink-wash v1 lock-array branch additionally requires
all eleven exact policy members and prohibits animation inheritance or an
unreviewed/incomplete reference binding. The v2 open ink-wash branch instead requires all
nineteen members, 9+9 locks, the seven ordered post-output gates, and the
response-only compact receipt. No planning, packaging, scoring evaluation,
promotion, or Git work occurs.
When present, the reviewed style-only binding must be identical across
planning, routing, visual brief, prompt record, and generation scope, and must
remain absent from `scenePromptOriginal` subject description.

Generation blockers additionally include the exact contract 6.1-6.2 approval,
scope, cost, attempt, duplicate-call, and provider-operation failure tokens.
A blocked output
returns status, failureType, missingFields, providerCalled=false,
costEvidence, requiredDecision, and safeToRetry. A successful output returns
the generation record ID/hash, attempts, result refs, costEvidence,
idempotencyKey, preservation handoff, and nextStep. For a hosted preview
selected with open ink-wash v2, the terminal generation response instead
returns the compact `generated_media_profile_conformance_receipt_v1`. Only
seven passes may say `preview_conformant_no_downstream`; a fail or insufficient
result must say `stop_no_retry_not_final` and must not be described as completed
or final.

## Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md
```
