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

The displayed top-level member set is closed. For
`assetType=character_single_image`, every nested value is also closed as
follows; no field is nullable and no additional member is allowed:

```yaml
planningOriginalRef:
  planningHandoffPath:
  routingRecordId:
  routingRecordPath:
  routingRecordSha256:
  routingPayloadSha256:
expressionProfilePayload: exact closed Canonical Character Expression Profile payload below
primarySubjectOrSilhouette: non-empty provider-neutral string
visualHierarchy: non-empty provider-neutral string
composition: non-empty provider-neutral string
paletteAndMaterial: non-empty provider-neutral string
backgroundPolicy: non-empty provider-neutral string
outlinePolicy: non-empty provider-neutral string
anchorPolicy: non-empty provider-neutral string
requiredVisualStatements:
  - constraintId:
    statement:
prohibitedVisualStatements:
  - constraintId:
    statement:
supportingElements:
  - constraintId:
    statement:
likelyWrongObjects:
  - constraintId:
    statement:
artifactSpecificBrief:
  identityConsistencyLock:
    identityId:
    referenceFacts: non-empty ordered array
  singleImageSpecification:
    viewpoint:
    pose:
    framing:
    canvas: {width: positive JSON integer, height: positive JSON integer}
    targetDisplaySize: {width: positive JSON integer, height: positive JSON integer}
    safeArea:
    finalBackgroundPolicy:
    generationBackground: {mode: removable_solid, color: exact planning value}
    noShadow: boolean
    outline:
      enabled: boolean
      color?: required only when enabled; forbidden otherwise
      exactThicknessPx?: required positive JSON integer only when enabled; forbidden otherwise
      placement?: outside_silhouette only when enabled; forbidden otherwise
    anchor:
      type: pelvis_root_ground_axis
      pelvisOrRootPoint:
      groundContactAxis:
visualEvidenceMap:
  - constraintId:
    statementPath: RFC 6901 pointer into this brief
    sourcePath:
    sourcePointer: RFC 6901 pointer into the exact source, or exact authority section selector
    sourceSha256:
    authorityRole: planning | master_concept | expression_profile
    transformationType: direct_copy | provider_neutral_normalization | profile_lock
providerTranslationContract:
  schemaVersion: imagegen_character_single_image_prompt_v2
  provider: imagegen
  promptAssemblyOrder:
    - planning_facts
    - negative_style_lock
    - positive_style_lock
  settingsSeparated: true
positiveStyleLock: exact ordered array copied from expressionProfilePayload.positiveStyleLock
negativeStyleLock: exact ordered array copied from expressionProfilePayload.negativeStyleLock
validation:
  status: valid
  sourceEvidence: complete
  identityConsistency: valid
  expressionProfile: valid
  characterSingleImage: valid
  providerTranslation: valid
```

The `generated_media_visual_brief_v2` record has exactly the top-level members
shown in the first schema block. `animationRequestId` is absent for a character
single image. All statement items have exactly `constraintId` and `statement`.
Every evidence item has exactly the seven displayed members. The lock-item
shape is exactly the three-member shape below. `planningOriginalRef` binds the
verified immutable route; it is not free-form prose.

For deterministic identity, project every top-level member except
`visualBriefId` and `validation`, change only `schemaVersion` to
`generated_media_visual_brief_hash_payload_v2`, and calculate:

```text
visualBriefPayloadSha256 = lowercase_hex(SHA256(canonicalJson(projected payload)))
visualBriefId = gmbrief2.character_single_image.{contentId}.{visualBriefPayloadSha256[0:20]}
```

`visualBriefPayloadSha256` is a derived validation result and is not a record
member. The enclosing prompt calculates `visualBriefSha256` over
`canonicalJson(the complete validated visualBrief record)`, including its ID
and closed validation object. Visual evidence entries preserve source order and
contain exact planning/authority identity. Missing evidence blocks.

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

The expression profile is a closed discriminated union keyed by
`expressionProfileKey`. The legacy-compatible
`projectbs_character_restrained_ink_line@1.0.0` payload has exactly
`expressionProfileKey`, `negativeStyleLock`, and `positiveStyleLock`. The
animation-ready profile below has those three members plus exactly
`proportionProjection`, `detailDensityBudget`, `colorValueBudget`, and
`authoringProjectionContract`. A member from one shape is not optional in the
other shape. Each lock-array item has exactly `constraintId`, `statement`, and
`authorityRef`; extra or missing members block. `authorityRef` uses
`{project-relative path}::{exact section heading}`.

The following payload remains canonical and immutable for
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

### Animation-ready minimal ink-line profile

The following payload is canonical and immutable for
`projectbs_character_animation_ready_minimal_ink_line@1.0.0`:

```json
{
  "expressionProfileKey": "projectbs_character_animation_ready_minimal_ink_line@1.0.0",
  "proportionProjection": {
    "fullBodyHeadCount": {"minimum": 3.75, "maximum": 4.25},
    "headToFullHeightPercent": {"minimum": 24, "maximum": 27},
    "limbPolicy": "shortened_simplified",
    "rejectAboveHeadCount": 4.25,
    "rejectNaturalisticAdultHeadCountRange": {"minimum": 7, "maximum": 8}
  },
  "detailDensityBudget": {
    "level": "animation_safe_low_detail",
    "silhouettePriority": "first",
    "contourPolicy": "omit_or_break_non_identity_contours",
    "identityStrokeGroups": ["face", "garment", "armor", "weapon"],
    "identityEncoding": "few_high_signal_strokes_per_group",
    "flatValueMasses": "minimal",
    "frameToFrameReproducibility": "required",
    "forbidden": ["individual_armor_scales", "individual_rivets", "dense_folds", "hatching", "skin_microtexture", "material_microtexture", "modeled_skin_shading", "modeled_material_shading"]
  },
  "colorValueBudget": {
    "accentHueCount": {"minimum": 1, "maximum": 2},
    "accentHuePolicy": "subdued_only",
    "valueMassPolicy": "minimal_flat_only",
    "gradients": "prohibited",
    "modeledShading": "prohibited",
    "cinematicOrPhysicalLighting": "prohibited",
    "realisticMaterialRendering": "prohibited"
  },
  "authoringProjectionContract": {
    "planningSelection": "explicit_approved_fact_required",
    "planningValuePolicy": "exact_or_narrower_within_profile_bounds",
    "requiredProjectionIds": ["full_body_head_count", "head_to_full_height_percent", "shortened_simplified_limbs", "animation_safe_detail_density", "minimal_flat_value_masses", "subdued_accent_hue_count"],
    "evidencePolicy": "each_projection_and_lock_requires_exact_approved_fact_or_profile_authority_evidence",
    "promptInclusion": "verbatim_profile_locks_and_exact_planning_bound_values_required",
    "conflictPolicy": "block_before_prompt_publication"
  },
  "negativeStyleLock": [
    {"constraintId": "char_anim_min_negative_over_4_25_heads", "statement": "Do not exceed 4.25 heads in full-body height; reject naturalistic seven-to-eight-head adult anatomy and heroic tall proportions.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_negative_long_naturalistic_limbs", "statement": "No long naturalistic adult limbs; keep limbs shortened and simplified within the approved proportion projection.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_negative_armor_microdetail", "statement": "No individually rendered armor scales or rivets.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_negative_dense_folds_hatching", "statement": "No dense garment folds, dense contour filling, or hatching.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_negative_microtexture_shading", "statement": "No skin or material microtexture, modeled skin shading, modeled material shading, stains, or fine surface texture.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_negative_gradient_lighting", "statement": "No gradients, cinematic lighting, volumetric lighting, or physically modeled lighting.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_negative_realistic_rendering", "statement": "No photographic, photorealistic, painterly 3D, PBR, glossy game-cinematic, western-realism, or realistic material rendering.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::10. 기본 시각 표현 경계"},
    {"constraintId": "char_anim_min_negative_excess_color", "statement": "No more than two accent hues and no saturated multicolor or nonminimal value treatment.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"}
  ],
  "positiveStyleLock": [
    {"constraintId": "char_anim_min_positive_exact_proportion", "statement": "Use a full-body proportion of 3.75 to 4.25 heads, with the head occupying 24 to 27 percent of full height.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_positive_short_limbs", "statement": "Use shortened simplified limbs and compact non-heroic anatomy.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_positive_sparse_contour", "statement": "Use an animation-safe sparse line vocabulary, silhouette-first hierarchy, and broken or omitted nonessential contours.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_positive_high_signal_identity", "statement": "Represent face, garment, armor, and weapon identity with only a few high-signal strokes per group.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_positive_flat_color_value", "statement": "Use minimal flat value masses and only one or two subdued accent hues.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_positive_negative_space", "statement": "Preserve open negative space and a contour density reproducible frame to frame.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile"},
    {"constraintId": "char_anim_min_positive_east_asian_ink_line", "statement": "Use restrained East Asian ink-line character treatment consistent with the ProjectBS Korean traditional-art boundary.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::5. 전통 예술과 조형"}
  ]
}
```

Its exact RFC 8785 JCS canonical JSON UTF-8 SHA-256 is exactly:

```text
de3339457f05c3dfd6fb6f854c102079c5c14f54d908a474cca093943afc7e06
```

The registry projection and executable contract vector must match this value;
a missing or divergent value is `expression_profile_payload_hash_mismatch` and
blocks activation.

Profile constants are the numeric ranges, budgets, policies, projection IDs,
and lock text in the payload. Planning-bound values are the character-specific
approved facts that select this exact profile and satisfy every required
projection. Authoring may accept an exact range or a narrower range inside the
profile bounds, but it may not pick a value, widen a range, or synthesize an
approval. Each projected planning value must appear in an independently
observable required/prohibited statement and in `visualEvidenceMap` with its
exact planning fact path/pointer. Each profile lock separately maps to
`authorityRole=expression_profile`. Missing selection, range, budget, color
limit, or evidence blocks before prompt publication.

The style-only raster used to approve these constants is not a canonical
record member. A temporary absolute path is neither durable provenance nor an
allowed `styleReference` member in any current closed schema. Provider-side
reference use requires a separately approved durable project-relative copy,
exact SHA-256, purpose `style_only`, and forbidden semantic transfer of person,
pose, action, clothes, and identity under a future reviewed schema revision.
Until then, only the approved profile constants above enter canonical identity.

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

For `projectbs_character_animation_ready_minimal_ink_line@1.0.0`, authoring
also requires the closed proportion/detail/color/projection members, explicit
approved planning selection, compatible planning-bound values, complete
projection evidence, and verbatim inclusion of every lock plus exact bound
values in provider prose. Use `missing_character_proportion_projection`,
`character_proportion_out_of_range`,
`missing_animation_safe_detail_budget`,
`missing_character_color_value_budget`, or
`character_profile_evidence_omission` as applicable. Generation repeats the
semantic check against immutable prompt bytes before submit and uses
`character_generation_proportion_gate_failed`,
`character_generation_detail_density_gate_failed`, or
`character_generation_color_value_gate_failed`; it never repairs prompt prose.

## Related Guides

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
