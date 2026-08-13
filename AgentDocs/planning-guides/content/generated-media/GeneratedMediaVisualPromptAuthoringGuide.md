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
expressionProfileKey: required for character image/animation
expressionProfilePayload: required for character image/animation
expressionProfilePayloadHash: required for character image/animation
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
positiveStyleLock: []
negativeStyleLock: []
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

## Canonical Character Expression Profile

This applies only to `character_single_image` and character-subject
`animation`. Master Concept owns the project-wide non-photographic Korean
traditional-art boundary; approved planning owns character facts; this section
is the sole normative owner of the reusable provider-neutral expression
profile. Other guides and the registry may reference its key and hash but must
not reproduce or alter its lock statements.

The closed `expressionProfilePayload` schema has exactly these top-level keys:
`expressionProfileKey`, `negativeStyleLock`, and `positiveStyleLock`. Each lock
array item has exactly `constraintId`, `statement`, and `authorityRef`; extra or
missing members block. `authorityRef` uses `{project-relative path}::{exact
section heading}`. The following payload is canonical for
`projectbs_character_restrained_ink_line@1.0.0`:

```json
{
  "expressionProfileKey": "projectbs_character_restrained_ink_line@1.0.0",
  "negativeStyleLock": [
    {"constraintId": "char_ink_negative_photographic", "statement": "No photographic, photorealistic, cinematic-portrait, or live-action rendering.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::10. 기본 시각 표현 경계"},
    {"constraintId": "char_ink_negative_skin_microtexture", "statement": "No realistic skin pores or photographic skin and material microtexture.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_negative_lens_depth", "statement": "No lens, focal-length, depth-of-field, bokeh, or camera-capture language.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_negative_volumetric_light", "statement": "No volumetric or cinematic portrait lighting and no physically modeled lighting.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_negative_3d_western_realism", "statement": "No painterly 3D render, PBR material render, glossy game-cinematic model, or western-fantasy realism.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::10. 기본 시각 표현 경계"},
    {"constraintId": "char_ink_negative_heavy_wash", "statement": "No heavy ink-wash flooding or uncontrolled brush texture that hides character identity.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"}
  ],
  "positiveStyleLock": [
    {"constraintId": "char_ink_positive_limited_line_palette", "statement": "Use restrained ink and brush line drawing with a limited black and gray line vocabulary.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_positive_silhouette_gesture", "statement": "Make the primary silhouette and gesture readable before internal detail.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_positive_line_hierarchy", "statement": "Use clear primary contours, identity-defining face, costume, and weapon lines, and sparse subordinate folds.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_positive_controlled_variation", "statement": "Use controlled variation in line width, density, taper, pressure, and occasional breaks rather than uniform technical outlines.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_positive_negative_space_value", "statement": "Preserve open negative space and flat minimal value masses without dense modeled shading.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"},
    {"constraintId": "char_ink_positive_identity_preservation", "statement": "Keep the approved face, costume layers, equipment, weapon, and palette identity-readable without ornamental expansion.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile"}
  ]
}
```

Array order is normative: negative locks use the displayed order, followed by
positive locks in their displayed order when assembling provider prose. Do not
sort arrays or merge statements. Canonical JSON follows RFC 8785 JCS: sort
object member names lexicographically, preserve array order and exact Unicode
strings, emit no insignificant whitespace and encode as UTF-8 without BOM.
`expressionProfilePayloadHash` is lowercase hexadecimal
`SHA-256(canonical JSON UTF-8 bytes)` and for the payload above is exactly:

```text
bda082ffe297c29cdc6b933a6c219ae67b11ae38bc784c198e4603c1741199cf
```

Do not shorten this contract to `stylized`. The copy-ready provider prompt must
contain both locks as direct text. Every lock maps to the Master Concept or the
exact active style-profile constraint ID; character-specific statements map
separately to approved planning evidence.

Camera, lighting, and material words are allowed only when approved planning
requires them and they are phrased as flat graphic composition rather than
photographic capture or 3D rendering. A material planning/profile conflict
returns `character_style_profile_conflict`; authoring writes no prompt record
and requests an explicit planning/profile revision.

## Character Single Image

Require identityConsistencyLock, one viewpoint, pose, framing, canvas, target
display size, safe area, background/no-shadow, exact outline, and
pelvis/root-ground-axis anchor. The brief represents one approved image only.
Do not add directions, rotations, alternate views or camera facts.

Provider handoff: `imagegen_character_single_image_prompt_v2`, prompt v3,
structure `character_single_image_v2`.

The cohesive prompt contains the complete positive and negative style locks,
not merely their IDs. Evidence coverage reports both groups independently. An
approved age, face, facial-hair, fatigue, or attractiveness statement may be
rendered only from that character's evidence; no default youth,
modern/westernized beauty, minor-coded appearance, sexualization, beard,
fatigue, aging, or gravitas is added.

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

For `domainType=character`, require an immutable reference prompt record and
inherit its exact `expressionProfileKey`, `expressionProfilePayload`, and
`expressionProfilePayloadHash`. Recompute the reference-record file SHA-256 and
the canonical payload hash before comparing them with the handoff. Every
frame preserves line hierarchy, simplification level, controlled stroke
variation, face landmarks, costume layers, equipment and weapon structure.
Motion may change pose and approved cloth movement but may not reinterpret the
character or drift toward photographic/3D rendering between frames.

## Failure, State, and Validation

Use the exact common/type blockers in GeneratedMediaImageGenOnlyContractGuide.md,
including missing evidence/identity/background/outline/anchor/reference/fixed-
cell/master-first/snapshot conditions. State is
`planning_verified -> brief_normalized -> provider_payload_ready`; any blocker
stops before a prompt record is written.

Validate schema parity, visualBriefId/hash, evidence coverage, registry/profile,
anchor discriminator, one provider payload, and no planning invention. Current
provider is ImageGen only.

Character validation additionally requires non-empty `positiveStyleLock` and
`negativeStyleLock`, direct inclusion of both in `scenePromptOriginal`, separate
evidence coverage for every lock statement, and reference/main-image style-lock
equality for character animation. Skill animation must omit all four character
reference/profile fields. Typed blockers are
`missing_positive_style_lock`, `missing_negative_style_lock`,
`style_lock_evidence_incomplete`, `provider_prompt_style_lock_missing`,
`character_style_profile_conflict`, `missing_reference_prompt_record`,
`reference_prompt_record_hash_mismatch`, `missing_expression_profile_key`,
`missing_expression_profile_payload`, `missing_expression_profile_payload_hash`, `expression_profile_key_mismatch`,
`expression_profile_payload_hash_mismatch`,
`unexpected_character_style_reference`, and
`character_animation_style_lock_mismatch`.

## Related Guides

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
