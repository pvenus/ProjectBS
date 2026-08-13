# Generated Media Visual Prompt Authoring Guide

## Purpose and Authority

Guide Type: current visual-normalization and provider-translation authority.
It converts approved v2 planning facts into
`generated_media_visual_brief_v2`, then one ImageGen prompt. It does not plan,
route, call ImageGen, package, evaluate, or promote.

```text
Master Concept
-> approved immutable planning handoff v2
-> this common visual guide
-> exact registry v2 profile
-> ImageGen provider prompt
```

No lower layer may relax or invent identity, period, culture, material, color,
symbol, motion, background, camera, or prohibition. Legacy v1 PixelLab and
eight-way visual rules are isolated in
GeneratedMediaLegacyV1CompatibilityGuide.md and do not appear as current
positive rules here.

## Visual Brief v2 Schema and Identity

```yaml
schemaVersion: generated_media_visual_brief_v2
visualBriefId:
guideContractVersion: generated_media_visual_prompt_authoring_v2
requestId:
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
contentId:
animationRequestId: required only for animation
planningSnapshotHash:
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId:
profileKey:
planningOriginalRef:
primarySubjectOrSilhouette:
visualHierarchy:
composition:
paletteAndMaterial:
backgroundPolicy:
outlinePolicy:
anchorPolicy:
requiredVisualStatements: []
prohibitedVisualStatements: []
supportingElements: []
likelyWrongObjects: []
artifactSpecificBrief:
visualEvidenceMap: []
providerTranslationContract:
status: normalized
validation:
```

The deterministic ID/hash includes every field except the ID itself and
validation-computed hash. Visual evidence entries contain constraintId,
statement path, exact planning source path/JSON pointer/hash, authority role,
and transformation type. Missing evidence blocks.

Keep `planningOriginal`, normalized brief, provider prompt payload, and
provider settings as distinct immutable layers. Do not rewrite planning text to
make it fit a prompt.

## Common Visual Rules

- convert required/prohibited elements into independently observable statements;
- define one primary subject/silhouette and subordinate supporting elements;
- define hierarchy, composition, palette/material, final/generation background,
  no-shadow, outline and exact anchor only from approved facts/profile;
- keep likelyWrongObjects minimal and evidence-backed;
- prohibit unapproved text, UI, logo, watermark and cultural/period conflicts;
- use one cohesive ImageGen prompt and keep settings outside prompt prose;
- missing, ambiguous or conflicting evidence returns a typed blocker.

## Character Single Image

Require identityConsistencyLock, one viewpoint, pose, framing, canvas, target
display size, safe area, background/no-shadow, exact outline, and
pelvis/root-ground-axis anchor. The brief represents one approved image only.
Do not add directions, rotations, alternate views or camera facts.

Provider handoff: `imagegen_character_single_image_prompt_v2`, prompt v3,
structure `character_single_image_v2`.

## Icon Single Image

Require identityConsistencyLock, exact icon profile, observable symbol/effect,
framing, canvas/display/safe area, background/no-shadow, exact outline and
visual-center anchor. Supporting effects cannot compete with the symbol. Do not
invent icon meaning or create skill/item-specific prompts.

This section owns only `domainType=skill|item`. It rejects stage, battle,
environment, story-scene, landscape, horizon, depth-layer, playable-area and
camera-scene responsibilities. Transparent or approved simple background,
clear silhouette/outline, visual-center balance and target small-size
readability are icon-only profile concerns.

Provider handoff: `imagegen_icon_single_image_prompt_v2`, prompt v3, structure
`icon_single_image_v2`.

## Background Single Image

Require an exact stage/battle/environment background profile plus approved
scene contract, composition, viewpoint, horizon, ordered depth layers,
playable/readability area, subject inclusions/exclusions, canvas/aspect, target
display, safe area, final background policy, content/scene consistency lock and
`scene_composition_anchor`.

The provider-neutral brief preserves scene framing and spatial hierarchy. It
must not invent scene, era, culture, weather, lighting, landmark, camera or
subjects. It must not apply transparent-icon defaults, icon visual center,
outline/silhouette scoring or small-icon readability. Missing background facts
return the exact background typed blocker before authoring.

Provider handoff: `imagegen_background_single_image_prompt_v2`, prompt v3,
structure `background_single_image_v2`.

## Animation

The normalized unit contains exactly one animationRequestId. Require hashed
reference, final frame count/timing/order/loop/key poses, fixed cell, scale
lock, vertical-motion policy, background/no-shadow, outline and master-first.

Anchor discriminator:

```text
domainType=character -> pelvis_root_ground_axis with groundContactAxis
domainType=skill     -> effect_origin
```

The brief asks for one coherent master at the final frame count. It cannot ask
for oversampling, request merging, per-frame crop/scale/recenter, or suppression
of approved vertical motion.

Provider handoff: `imagegen_animation_prompt_v2`, prompt v3, structure
`animation_gif_frame_set_v2`.

## Failure, State, and Validation

Use the exact common/type blockers in GeneratedMediaImageGenOnlyContractGuide.md,
including missing evidence/identity/background/outline/anchor/reference/fixed-
cell/master-first/snapshot conditions. State is
`planning_verified -> brief_normalized -> provider_payload_ready`; any blocker
stops before a prompt record is written.

Validate schema parity, visualBriefId/hash, evidence coverage, registry/profile,
anchor discriminator, one provider payload, and no planning invention. Current
provider is ImageGen only.

## Related Guides

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
