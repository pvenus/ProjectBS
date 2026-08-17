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
referenceBindings: [] # optional only for a reviewed durable character style-only binding
visualEvidenceMap: []
providerTranslationContract:
positiveStyleLock: [] # non-empty only for a registered lock-array profile; empty for sparse
negativeStyleLock: [] # non-empty only for a registered lock-array profile; empty for sparse
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
referenceBindings: # optional; exactly one when present
  - role: style_only
    projectRelativePath:
    sha256:
    reviewRecordId:
    reviewRecordPath:
    reviewRecordSha256:
visualEvidenceMap:
  - constraintId:
    statementPath: RFC 6901 pointer into this brief
    sourcePath:
    sourcePointer: RFC 6901 pointer into the exact source, or exact authority section selector
    sourceSha256:
    authorityRole: planning | master_concept | expression_profile
    transformationType: direct_copy | provider_neutral_normalization | profile_lock | profile_policy_projection
providerTranslationContract:
  schemaVersion: imagegen_character_single_image_prompt_v2
  provider: imagegen
  promptAssemblyOrder: lock-array profile -> [planning_facts, negative_style_lock, positive_style_lock]; sparse profile -> [planning_facts, sparse_profile_policy_projection]
  settingsSeparated: true
positiveStyleLock: lock-array profile -> exact non-empty ordered array; sparse profile -> exact empty array
negativeStyleLock: lock-array profile -> exact non-empty ordered array; sparse profile -> exact empty array
validation:
  status: valid
  sourceEvidence: complete
  identityConsistency: valid
  expressionProfile: valid
  characterSingleImage: valid
  providerTranslation: valid
```

The `generated_media_visual_brief_v2` record has exactly the top-level members
shown in the first schema block, with `referenceBindings` conditionally present
only for a reviewed durable character style-only binding and otherwise absent.
`animationRequestId` is absent for a character single image. All statement
items have exactly `constraintId` and `statement`.
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
`authoringProjectionContract`. The sparse-ink profile below has exactly its
discriminator plus the eight budget/policy members displayed in its canonical
payload; it has no lock arrays. The bold-outline compressed-detail profile has
exactly its discriminator, `proportionProjection`, `outlineHierarchy`,
`facialSimplificationBudget`, `compressedDetailBudget`,
`colorSignatureContract`, `inkTreatment`, `authoringProjectionContract`, and
the two lock arrays displayed in its canonical payload. A member from one shape
is not optional in another shape. Each lock-array item in the four lock-array profiles has exactly
`constraintId`, `statement`, and `authorityRef`; extra or missing members block. `authorityRef` uses
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

### Sparse ink pastel motion profile

The following payload is canonical and immutable for
`projectbs_character_sparse_ink_pastel_motion@1.0.0`. It is a new profile and
does not alter the bytes, meaning, fallback, or hash of either existing 1.0.0
profile.

```json
{
  "expressionProfileKey": "projectbs_character_sparse_ink_pastel_motion@1.0.0",
  "contourOmissionBudget": {
    "unit": "percent_of_expected_contour_and_internal_boundary_length",
    "main": {"minimum": 35, "maximum": 45},
    "animationFrame": {"minimum": 35, "maximum": 50},
    "measurementPolicy": "closed_authoring_projection_plus_observable_evaluator_checklist"
  },
  "lineHierarchy": {
    "darkestIdentityActionAnchors": ["gaze", "topknot", "hand_sword_grip", "support_foot", "action_joints"],
    "secondaryCostumeLinePolicy": "pale_gray_broken_or_omitted",
    "primaryPolicy": "line_describes_identity_and_action",
    "uniformLineWeight": "prohibited"
  },
  "negativeSpacePolicy": {
    "internalNegativeSpace": "required",
    "closedColoringBookContours": "prohibited",
    "fullyInkedSilhouette": "prohibited"
  },
  "pigmentBudget": {
    "areaUnit": "percent_of_character_area",
    "mainMaximumPigmentedArea": 18,
    "mainAccentCount": {"minimum": 4, "maximum": 7},
    "animationFrameAccentCount": {"minimum": 3, "maximum": 6},
    "animationFrameAreaPolicy": "no_numeric_claim_use_no_fill_gate"
  },
  "accentPalette": {
    "allowed": ["faded_indigo_navy", "dusty_ochre_gray_brown"],
    "offPaletteHue": "prohibited",
    "colorRole": "subordinate_to_line_identity_and_action"
  },
  "pigmentApplication": {
    "allowedMethods": ["loose_watercolor_bloom", "soft_pastel_rub", "short_dragged_brush_stroke"],
    "outsideLineExcursion": "slight_allowed",
    "internalNegativeSpace": "required",
    "solidOpaqueCelFill": "prohibited",
    "blackWhiteClosedColoringBookFill": "prohibited"
  },
  "motionLinePolicy": {
    "mainProjection": "gesture_readability_without_animation_frame_effects",
    "animationCues": ["gesture_searching_overlaps", "taper_break", "robe_sleeve_lag", "sword_arc", "overshoot_smear"],
    "attackIndigoSupport": "three_to_five_marks_support_sword_and_torso_rotation",
    "attackGrayBrownSupport": "supports_shoulder_and_hem_inertia",
    "activeFrameDetailPolicy": "face_and_costume_detail_may_reduce_without_identity_anchor_loss",
    "staticRepeatedActionFrames": "prohibited"
  },
  "identityAnchors": {
    "required": ["gaze", "topknot", "hand_sword_grip", "support_foot", "action_joints"],
    "stability": "required_across_all_animation_frames",
    "proportion": {"fullBodyHeadCount": {"minimum": 3.75, "maximum": 4.25}, "limbPolicy": "short_simple"},
    "naturalisticSevenToEightHeads": "prohibited"
  }
}
```

Canonicalization is RFC 8785 JCS over exact UTF-8 bytes. The exact payload
SHA-256 is:

```text
b5ce18d11e249598ad6da13d59340cf7cede3d2896259dcc5e02dbbf98e80443
```

The eight policy members are closed and have the exact types shown. Percent
and count ranges are inclusive JSON integers. Array order is normative.
`animationFrameAreaPolicy` deliberately forbids a fabricated numeric computer-
vision threshold: animation uses the closed no-fill gate plus observable
accent-count and palette checks. Main authoring projects the main omission,
area, accent-count, no-fill and proportion gates. Character animation inherits
the same complete payload and hash, then projects the animation-frame omission,
accent-count, motion cues and anchor stability without changing profile bytes.
The main and animation request, structureProfile and record identities remain
independent.

Provider prose must express intentional contour gaps, short/simple limbs,
limited faded indigo/navy and dusty ochre/gray-brown pigment, loose bloom/rub/
dragged strokes, internal negative space and the applicable motion cues. It
must reject closed coloring-book contours, fully inked silhouettes, uniform
lines, solid/opaque/cel fill, clean vector rendering, dense hatching, modeled
shading, realistic materials, photo/3D/PBR and seven-to-eight-head anatomy.
Temporary absolute evidence paths are audit-only. Canonical style-reference
use first requires a durable project-relative copy, exact hash and reviewed
styleReference schema; this task creates none.

### Open ink-wash dynamic-contour character profile

The following payload is canonical and immutable for
`projectbs_character_open_ink_wash_dynamic_contour@1.0.0`. It is an additive
`character_single_image_v2`-only profile. It does not amend, alias, inherit, or
reinterpret any existing sparse-ink, bold-outline, or animation-successor
profile.

```json
{
  "expressionProfileKey": "projectbs_character_open_ink_wash_dynamic_contour@1.0.0",
  "applicability": {
    "structureProfiles": ["character_single_image_v2"],
    "characterAnimationInheritance": "prohibited",
    "selection": "explicit_approved_planning_fact_and_complete_projection_required"
  },
  "proportionAndAgeContract": {
    "fullBodyHeadCount": {"minimum": 4, "maximum": 5, "target": 4.25},
    "presentation": "young_adult",
    "minorOrChildCoding": "prohibited",
    "limbPolicy": "compact_but_adult"
  },
  "contourOmissionBudget": {
    "unit": "percent_of_expected_silhouette_and_internal_boundary_length",
    "minimum": 35,
    "maximum": 55,
    "target": 45,
    "closedStickerSilhouette": "prohibited",
    "measurementPolicy": "closed_authoring_projection_plus_observable_evaluator_checklist"
  },
  "mokSeonContract": {
    "lineQuality": "pressure_variable_tactile_mok_seon",
    "requiredStrokePhases": ["brush_start", "directional_drag", "dry_end"],
    "directionalWeight": "required",
    "uniformOutlineWeight": "prohibited",
    "vectorCleanContour": "prohibited"
  },
  "pigmentApplicationContract": {
    "media": ["rough_watercolor", "rough_pastel"],
    "applicationScale": "broad_masses_not_decorative_small_splashes",
    "controlledBleedBeyondOutline": "required",
    "controlledMisalignmentBeyondOutline": "required",
    "cleanCelFill": "prohibited",
    "decorativeSmallSplashes": "prohibited"
  },
  "paletteRoleContract": {
    "roles": [
      {"role": "primary_cool", "colorFamily": "faded_blue_gray_or_indigo"},
      {"role": "secondary_earth", "colorFamily": "dusty_gray_brown"},
      {"role": "small_warm_accent", "colorFamily": "muted_ochre", "scale": "small_only"}
    ],
    "roleSeparation": "required",
    "offRoleSubstitution": "prohibited"
  },
  "negativeSpaceContract": {
    "minimumAchromaticOrUnpaintedPercent": 70,
    "scopes": ["figure_interior", "full_canvas"],
    "figureInteriorPolicy": "open_unpainted_space_required",
    "canvasPolicy": "warm_ivory_background_counts_as_achromatic_unpainted_space"
  },
  "backgroundContract": {
    "generationBackground": {"mode": "removable_solid", "colorFamily": "warm_ivory"},
    "finalBackgroundPolicy": "transparent_or_planning_approved_after_background_removal",
    "halo": "prohibited",
    "vignette": "prohibited",
    "scene": "prohibited",
    "shadow": "prohibited"
  },
  "identityAnchorContract": {
    "planningBinding": "required_exact_character_specific",
    "requiredAnchorGroups": ["young_adult_korean_identity", "joseon_hair_and_costume", "approved_equipment", "approved_weapon", "handedness", "identifying_features"],
    "styleReferenceIdentityTransfer": "prohibited",
    "identityOrEquipmentOmission": "prohibited"
  },
  "acceptedStyleReferenceContract": {
    "sha256": "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf",
    "status": "audit_only_unbound_without_durable_project_relative_copy",
    "allowedPurpose": "style_and_composition_audit_evidence_only",
    "canonicalReferenceBinding": "prohibited_until_reviewed_durable_project_relative_copy_and_closed_reference_binding",
    "forbiddenTransfer": ["person_identity", "canonical_character_identity", "pose", "action", "clothing", "equipment", "edit_target"]
  },
  "authoringProjectionContract": {
    "planningSelection": "explicit_approved_fact_required",
    "requiredPlanningBindings": ["fullBodyHeadCount", "youngAdultPresentation", "identityConsistencyLock", "singleImageSpecification", "paletteRoleAnchors", "generationBackground"],
    "requiredProfileProjectionMembers": ["proportionAndAgeContract", "contourOmissionBudget", "mokSeonContract", "pigmentApplicationContract", "paletteRoleContract", "negativeSpaceContract", "backgroundContract", "identityAnchorContract", "acceptedStyleReferenceContract"],
    "evidencePolicy": "every_binding_profile_member_and_lock_requires_exact_planning_or_profile_authority_evidence",
    "promptInclusion": "verbatim_locks_and_complete_policy_projection_required",
    "conflictPolicy": "block_before_prompt_publication"
  },
  "negativeStyleLock": [
    {"constraintId": "char_open_wash_negative_child_or_naturalistic_tall", "statement": "Do not depict a child, minor-coded figure, or naturalistic tall heroic anatomy outside the approved four-to-five-head young-adult range.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_negative_sticker_vector_contour", "statement": "No sticker-clean closed silhouette, uniform outline weight, vector-clean contour, or clean coloring-book boundary.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_negative_clean_fill_splashes", "statement": "No clean cel fill, opaque full-region fill, or decorative small splashes standing in for broad rough pigment masses.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_negative_palette_role_collapse", "statement": "Do not merge, swap, or replace the faded blue-gray or indigo, dusty gray-brown, and small muted-ochre palette roles.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_negative_painted_space_excess", "statement": "Do not reduce achromatic or unpainted negative space below seventy percent in either the figure interior or full canvas.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_negative_halo_scene_shadow", "statement": "No halo, vignette, scene, environment, cast shadow, contact shadow, or shadow-substitute treatment.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_negative_reference_identity_transfer", "statement": "Do not use the audit-only style reference as canonical identity, person, pose, action, clothing, equipment, edit target, or provider reference binding.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"}
  ],
  "positiveStyleLock": [
    {"constraintId": "char_open_wash_positive_young_adult_compact", "statement": "Use a clearly young-adult compact figure in the approved four-to-five-head range, targeted at four-and-a-quarter heads, without child coding.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_positive_open_dynamic_contour", "statement": "Omit thirty-five to fifty-five percent of expected silhouette and internal boundaries, targeting forty-five percent, while preserving readable identity anchors.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_positive_tactile_mok_seon", "statement": "Use pressure-variable tactile mok-seon with observable brush start, directional drag, dry end, and directional weight.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_positive_broad_bleeding_pigment", "statement": "Use broad rough watercolor and pastel masses with controlled bleed and controlled misalignment beyond the contour.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_positive_separate_palette_roles", "statement": "Keep faded blue-gray or indigo, dusty gray-brown, and a small muted-ochre accent in separate approved character-specific roles.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_positive_negative_space", "statement": "Keep at least seventy percent of both the figure interior and full canvas achromatic or unpainted negative space.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"},
    {"constraintId": "char_open_wash_positive_identity_on_ivory", "statement": "Preserve the approved young-adult Korean and Joseon identity, costume, equipment, weapon, and handedness anchors on a removable warm-ivory solid generation background with no halo, scene, or shadow.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile"}
  ]
}
```

Canonicalization is RFC 8785 JCS over the exact UTF-8 payload. The registered
SHA-256 is `37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd` and must match the registry
before activation. All numeric ranges are inclusive; array order is normative.
The eleven policy members, two ordered lock arrays, and key are closed.

The exact accepted-image SHA identifies style/composition evidence; it does not
identify canonical character bytes. The separately published asset and closed
review record in GeneratedMediaStyleReferenceBindingGuide.md may authorize one
six-member `role=style_only` binding after all raw hashes and the selected
profile key/hash pass. It continues to forbid person, pose, action, clothing,
equipment, identity, and edit-target transfer. Without that complete binding,
authoring keeps `referenceBindings` absent and never invents a path;
`referenceBindings` stays empty for this audit evidence. A separately reviewed
durable style-only binding is the additive branch defined below and does not
change this profile payload rule.

Planning owns the exact Seojin identity/equipment facts and binds them through
the character planning projection; the profile owns only reusable expression
semantics. A missing member, missing planning binding, wrong reference role,
attempted animation inheritance, or conflict blocks before prompt publication.
Provider prose must independently preserve every profile member and all fourteen
ordered locks; it may not collapse this contract to `ink wash` or `stylized`.

### Open ink-wash output-conformance successor profile

The following payload is canonical and immutable for
`projectbs_character_open_ink_wash_dynamic_contour@2.0.0`. It is a separate
single-image-only successor, not an amendment or alias of `@1.0.0`. It retains
the v1 visual boundary while adding executable output measurement, closed
surface-detail rejection, mandatory post-submit conformance classification, and
a compact hash-bound receipt. Existing v1 planning, prompts, previews, key,
payload, hash, and meaning remain unchanged.

```json
{
  "expressionProfileKey": "projectbs_character_open_ink_wash_dynamic_contour@2.0.0",
  "predecessorBinding": {
    "expressionProfileKey": "projectbs_character_open_ink_wash_dynamic_contour@1.0.0",
    "expressionProfilePayloadHash": "37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd",
    "mutationPolicy": "predecessor_bytes_and_meaning_unchanged"
  },
  "applicability": {
    "structureProfiles": ["character_single_image_v2"],
    "characterAnimationInheritance": "prohibited",
    "selection": "explicit_approved_planning_fact_and_complete_successor_projection_required"
  },
  "proportionAndAgeContract": {
    "fullBodyHeadCount": {"minimum": 4, "maximum": 5, "target": 4.25},
    "presentation": "young_adult",
    "minorOrChildCoding": "prohibited",
    "limbPolicy": "compact_but_adult"
  },
  "proportionMeasurementContract": {
    "headHeight": "top_of_cranial_mass_excluding_topknot_to_bottom_of_chin",
    "fullBodyHeight": "top_of_cranial_mass_excluding_topknot_to_lowest_weight_bearing_sole",
    "headCountFormula": "fullBodyHeight_divided_by_headHeight",
    "observableAcceptance": {"minimum": 4, "maximum": 5},
    "targetIntent": 4.25,
    "uncertainMeasurement": "evidence_insufficient"
  },
  "contourOmissionBudget": {
    "unit": "percent_of_expected_silhouette_and_internal_boundary_length",
    "minimum": 35,
    "maximum": 55,
    "target": 45,
    "closedStickerSilhouette": "prohibited",
    "measurementPolicy": "closed_authoring_projection_plus_observable_output_triage"
  },
  "mokSeonContract": {
    "lineQuality": "pressure_variable_tactile_mok_seon",
    "requiredStrokePhases": ["brush_start", "directional_drag", "dry_end"],
    "directionalWeight": "required",
    "uniformOutlineWeight": "prohibited",
    "vectorCleanContour": "prohibited"
  },
  "pigmentApplicationContract": {
    "media": ["rough_watercolor", "rough_pastel"],
    "applicationScale": "broad_masses_not_decorative_small_splashes",
    "controlledBleedBeyondOutline": "required",
    "controlledMisalignmentBeyondOutline": "required",
    "cleanCelFill": "prohibited",
    "decorativeSmallSplashes": "prohibited"
  },
  "paletteRoleContract": {
    "roles": [
      {"role": "primary_cool", "colorFamily": "faded_blue_gray_or_indigo"},
      {"role": "secondary_earth", "colorFamily": "dusty_gray_brown"},
      {"role": "small_warm_accent", "colorFamily": "muted_ochre", "scale": "small_only"}
    ],
    "roleSeparation": "required",
    "offRoleSubstitution": "prohibited"
  },
  "negativeSpaceContract": {
    "minimumAchromaticOrUnpaintedPercent": 70,
    "scopes": ["figure_interior", "full_canvas"],
    "figureInteriorPolicy": "open_unpainted_space_required",
    "canvasPolicy": "warm_ivory_background_counts_as_achromatic_unpainted_space"
  },
  "surfaceDetailContract": {
    "priority": "identity_silhouette_before_surface_detail",
    "face": "high_signal_mok_seon_without_realistic_modeling_skin_shading_or_microtexture",
    "armor": "one_broad_dusty_gray_brown_mass_with_broken_identity_edges",
    "individualArmorPlateEnumeration": "prohibited",
    "rivetsLacingAndFastenerEnumeration": "prohibited",
    "garmentMicrofoldEnumeration": "prohibited",
    "materialMicrotexture": "prohibited",
    "modeledLightingAndRealisticMaterialRendering": "prohibited"
  },
  "backgroundContract": {
    "generationBackground": {"mode": "removable_solid", "color": "#F2EFE6"},
    "finalBackgroundPolicy": "transparent_or_planning_approved_after_background_removal",
    "allowedVisibleField": "uniform_warm_ivory_only",
    "halo": "prohibited",
    "vignette": "prohibited",
    "radialGradient": "prohibited",
    "darkBackdrop": "prohibited",
    "scene": "prohibited",
    "shadow": "prohibited"
  },
  "identityAnchorContract": {
    "planningBinding": "required_exact_character_specific",
    "requiredAnchorGroups": ["young_adult_korean_identity", "joseon_hair_and_costume", "approved_equipment", "approved_weapon", "handedness", "identifying_features"],
    "styleReferenceIdentityTransfer": "prohibited",
    "identityOrEquipmentOmission": "prohibited"
  },
  "acceptedStyleReferenceContract": {
    "sha256": "b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf",
    "status": "audit_only_unbound_without_durable_project_relative_copy",
    "allowedPurpose": "style_and_composition_audit_evidence_only",
    "canonicalReferenceBinding": "prohibited_until_reviewed_durable_project_relative_copy_and_closed_reference_binding",
    "forbiddenTransfer": ["person_identity", "canonical_character_identity", "pose", "action", "clothing", "equipment", "edit_target"]
  },
  "providerOutputConformanceContract": {
    "timing": "after_observable_output_before_preview_complete_or_final_status",
    "gateOrder": ["proportion_age", "contour_mok_seon", "surface_detail", "pigment_palette_negative_space", "background", "identity_equipment", "reference_role"],
    "gateResultEnum": ["pass", "fail", "evidence_insufficient"],
    "allPassStatus": "preview_conformant_no_downstream",
    "anyFailStatus": "preview_profile_nonconformant",
    "insufficientStatus": "preview_profile_conformance_blocked",
    "nonPassNextStep": "stop_no_retry_not_final",
    "submitAccounting": "preserve_observed_providerCalled_submitCount_retryCount_and_output_hash",
    "automaticRetry": "prohibited",
    "scoringOrPromotion": "prohibited"
  },
  "compactConformanceReceiptContract": {
    "schemaVersion": "generated_media_profile_conformance_receipt_v1",
    "requiredIdentityFields": ["requestId", "planningSnapshotHash", "promptRecordId", "promptRecordSha256", "expressionProfileKey", "expressionProfilePayloadHash", "observableOutputSha256"],
    "requiredStateFields": ["profileConformanceStatus", "failureType", "gateResults", "providerCalled", "submitCount", "retryCount", "nextStep"],
    "gateResultShape": ["gateId", "result"],
    "hashPolicy": "RFC8785_JCS_SHA256_over_closed_receipt_hash_payload",
    "messagePolicy": "send_receipt_once_then_reference_receipt_hash_instead_of_retransmitting_authority_payload",
    "authorityPolicy": "receipt_reusable_for_status_and_handoff_but_not_a_substitute_for_fresh_Git_blob_validation_before_mutation"
  },
  "authoringProjectionContract": {
    "planningSelection": "explicit_approved_fact_required",
    "requiredPlanningBindings": ["fullBodyHeadCount", "youngAdultPresentation", "identityConsistencyLock", "singleImageSpecification", "paletteRoleAnchors", "generationBackground"],
    "requiredProfileProjectionMembers": ["proportionAndAgeContract", "proportionMeasurementContract", "contourOmissionBudget", "mokSeonContract", "pigmentApplicationContract", "paletteRoleContract", "negativeSpaceContract", "surfaceDetailContract", "backgroundContract", "identityAnchorContract", "acceptedStyleReferenceContract", "providerOutputConformanceContract", "compactConformanceReceiptContract"],
    "evidencePolicy": "every_binding_profile_member_and_lock_requires_exact_planning_or_profile_authority_evidence",
    "promptInclusion": "verbatim_locks_and_complete_policy_projection_required",
    "conflictPolicy": "block_before_prompt_publication"
  },
  "negativeStyleLock": [
    {"constraintId": "char_open_wash_v2_negative_child_or_naturalistic_tall", "statement": "No child or minor coding and no body outside the observable four-to-five-head young-adult range measured from cranial mass to chin and cranial mass to weight-bearing sole.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_sticker_vector_contour", "statement": "No sticker-clean closed silhouette, uniform outline weight, vector-clean contour, or clean coloring-book boundary.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_surface_overdetail", "statement": "No realistic face, skin shading, material microtexture, garment microfold enumeration, individual armor plates, rivets, lacing, fasteners, modeled lighting, or realistic armor rendering.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_clean_fill_splashes", "statement": "No clean cel fill, opaque full-region fill, or decorative small splashes standing in for broad rough pigment masses.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_palette_role_collapse", "statement": "Do not merge, swap, or replace the faded blue-gray or indigo, dusty gray-brown, and small muted-ochre palette roles.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_painted_space_excess", "statement": "Do not reduce achromatic or unpainted negative space below seventy percent in either the figure interior or full canvas.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_halo_scene_shadow", "statement": "No halo, vignette, radial gradient, dark backdrop, scene, environment, cast shadow, contact shadow, or shadow substitute; show only uniform warm ivory outside the figure.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_reference_identity_transfer", "statement": "Do not use the audit-only style reference as canonical identity, person, pose, action, clothing, equipment, edit target, or provider reference binding.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_negative_nonconformant_final", "statement": "Do not label a returned image conformant, complete, final, ready, or retryable when any ordered output gate fails or lacks sufficient evidence.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"}
  ],
  "positiveStyleLock": [
    {"constraintId": "char_open_wash_v2_positive_young_adult_compact", "statement": "Use a clearly young-adult compact figure accepted only within the observable four-to-five-head range and targeted at four-and-a-quarter heads.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_open_dynamic_contour", "statement": "Omit thirty-five to fifty-five percent of expected silhouette and internal boundaries, targeting forty-five percent, while preserving readable identity anchors.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_tactile_mok_seon", "statement": "Use pressure-variable tactile mok-seon with observable brush start, directional drag, dry end, and directional weight.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_simplified_surface", "statement": "Render face, garments, and one-shoulder armor as high-signal open ink and broad pigment masses; armor is one broad dusty-gray-brown mass with broken identity edges, not enumerated construction detail.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_broad_bleeding_pigment", "statement": "Use broad rough watercolor and pastel masses with controlled bleed and controlled misalignment beyond the contour.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_separate_palette_roles", "statement": "Keep faded blue-gray or indigo, dusty gray-brown, and a small muted-ochre accent in separate approved character-specific roles.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_negative_space", "statement": "Keep at least seventy percent of both the figure interior and full canvas achromatic or unpainted negative space.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_identity_on_ivory", "statement": "Preserve approved young-adult Korean and Joseon identity and equipment on uniform removable warm ivory with no halo, vignette, radial gradient, dark backdrop, scene, or shadow.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"},
    {"constraintId": "char_open_wash_v2_positive_output_triage", "statement": "After one returned image, classify every ordered output gate and emit one compact hash-bound receipt; any fail or insufficient evidence stops without retry and without final or complete labeling.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile"}
  ]
}
```

Canonicalization is RFC 8785 JCS over the exact UTF-8 payload. The registered
SHA-256 is `b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5`. Object member order is not
hash-significant; array order and all displayed values are normative. The v2
payload is self-contained and must not be reconstructed by patching v1 at run
time.

The post-output check is a bounded conformance triage, not scoring, promotion,
preservation, retry authorization, or the separate evaluation role. It uses the
already returned observable image exactly once. A failed or insufficient gate
preserves truthful consumed-submit/output evidence but changes the response from
`preview_complete_no_downstream` to the closed non-pass status and
`stop_no_retry_not_final`. The compact receipt carries hashes and seven scalar
gate results; it replaces repeated authority prose in control-plane messages but
never replaces a fresh Git-blob validation before mutation.

### Bold-outline compressed-detail character profile

The following payload is canonical and immutable for
`projectbs_character_bold_outline_compressed_detail@1.0.0`. It applies only to
`character_single_image_v2` after one exact approved planning selection. It
does not alter or reinterpret any earlier profile or prompt identity.

```json
{
  "expressionProfileKey": "projectbs_character_bold_outline_compressed_detail@1.0.0",
  "proportionProjection": {
    "fullBodyHeadCount": {"minimum": 4, "maximum": 5, "target": 4.5},
    "headToFullHeightPercent": {"minimum": 20, "maximum": 25},
    "limbPolicy": "shortened_compressed",
    "rejectNaturalisticAdultHeadCountRange": {"minimum": 6.5, "maximum": 8},
    "rejectLongLimbs": true,
    "rejectHeroicTallAnatomy": true
  },
  "outlineHierarchy": {
    "placement": "outside_silhouette",
    "externalOutlineTone": "bold_dark_neutral",
    "sourceCanvas": {"width": 1024, "height": 1536},
    "targetDisplaySize": {"width": 96, "height": 144},
    "externalOutlineSourcePx": {"minimum": 16, "maximum": 22},
    "sourceToTargetScale": "source_px_times_3_divided_by_32",
    "externalOutlineTargetPx": {"minimum": 1.5, "maximum": 2.0625},
    "minimumExternalToInternalThicknessRatio": 2,
    "internalLinePolicy": "materially_thinner_sparse_nonuniform",
    "silhouetteContinuity": "required"
  },
  "facialSimplificationBudget": {
    "countingUnit": "one_continuous_visible_mark_between_pen_lifts_or_intentional_breaks",
    "maximumTotalMarks": 9,
    "componentMaximums": {"browsAndEyes": 4, "nose": 1, "mouth": 1, "jawAndFaceShape": 3},
    "allowedComponents": ["brows", "eyes", "minimal_nose_mark", "mouth_line", "jaw_and_face_shape"],
    "forbidden": ["realistic_facial_modeling", "facial_hatching", "skin_shading", "facial_microtexture"]
  },
  "compressedDetailBudget": {
    "priority": "identity_points_before_surface_detail",
    "identityGroups": ["face", "hair_or_headwear", "garment_silhouette", "armor_silhouette", "weapon"],
    "surfaceDetailPolicy": "omit_unless_identity_critical_and_planning_approved",
    "maximumSecondaryFoldMarksPerGarmentRegion": 3,
    "forbidden": ["dense_folds", "individual_armor_scales", "individual_rivets", "microtexture", "hatching", "modeled_shading", "realistic_material_rendering"]
  },
  "colorSignatureContract": {
    "planningBinding": "required_character_specific",
    "primaryHue": "required_exact_planning_value",
    "secondaryHue": "optional_exact_planning_value",
    "primaryAnchorElements": "required_non_empty_unique_ordered_planning_list",
    "secondaryAnchorElements": "required_non_empty_unique_ordered_planning_list_only_when_secondary_hue_present",
    "maximumCharacterCoveragePercent": {"minimum": 1, "maximum": 35},
    "maximumColorMasses": {"minimum": 1, "maximum": 4},
    "neutralOutlineColor": "required_exact_planning_value",
    "neutralWeaponColor": "required_exact_planning_value",
    "fullGarmentFill": "prohibited",
    "lineHierarchyOverride": "prohibited"
  },
  "inkTreatment": {
    "styleBoundary": "restrained_east_asian_ink_animation_drawing",
    "allowedAccentMethods": ["limited_flat_mass", "limited_dry_brush", "limited_watercolor"],
    "accentPriority": "subordinate_to_bold_silhouette_and_identity_lines",
    "silhouetteErosion": "prohibited",
    "photorealistic3dPbrWesternRealism": "prohibited"
  },
  "authoringProjectionContract": {
    "planningSelection": "explicit_approved_fact_required",
    "planningValuePolicy": "exact_values_required_within_profile_bounds",
    "requiredPlanningBindings": ["fullBodyHeadCount", "externalOutlineSourcePx", "internalLineSourcePx", "facialMarkBudget", "primaryHue", "primaryAnchorElements", "maximumCharacterCoveragePercent", "maximumColorMasses", "neutralOutlineColor", "neutralWeaponColor"],
    "conditionalPlanningBindings": ["secondaryHue_requires_secondaryAnchorElements"],
    "evidencePolicy": "every_binding_budget_and_lock_requires_exact_planning_or_profile_authority_evidence",
    "promptInclusion": "verbatim_locks_and_exact_planning_bound_values_required",
    "conflictPolicy": "block_before_prompt_publication"
  },
  "negativeStyleLock": [
    {"constraintId": "char_bold_negative_naturalistic_tall_anatomy", "statement": "No naturalistic six-and-a-half-to-eight-head anatomy, long limbs, or heroic tall proportions.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_negative_dense_surface_detail", "statement": "No dense folds, individual armor scales, individual rivets, microtexture, hatching, modeled shading, or realistic material rendering.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_negative_weak_or_uniform_outline", "statement": "No weak, uniform, or internal-line-equal silhouette outline and no pigment treatment that erases the outside contour.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_negative_realistic_face", "statement": "No realistic facial modeling, facial hatching, skin shading, facial microtexture, or marks beyond the approved facial budget.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_negative_unanchored_color", "statement": "No unanchored accent color, full-garment color fill, excessive color coverage, or color mass beyond the approved character signature.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_negative_realistic_rendering", "statement": "No photographic, photorealistic, painterly 3D, PBR, glossy cinematic, or western-realism rendering.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::10. 기본 시각 표현 경계"}
  ],
  "positiveStyleLock": [
    {"constraintId": "char_bold_positive_compact_proportion", "statement": "Use a compact full-body proportion from four to five heads, centered near four-and-a-half heads, with shortened compressed limbs.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_positive_outline_hierarchy", "statement": "Use a bold dark outside-silhouette outline at the exact approved source thickness, at least twice the exact internal-line thickness; keep internal lines materially thinner, sparse, and nonuniform.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_positive_identity_first_detail", "statement": "Prioritize face, hair or headwear, garment silhouette, armor silhouette, and weapon identity before any subordinate surface detail.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_positive_simplified_face", "statement": "Represent the face only with high-signal brows and eyes, one minimal nose mark, one mouth line, and the approved jaw or face-shape marks within the closed budget.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_positive_color_signature", "statement": "Apply the approved primary and optional secondary hues only to their exact anchor elements within the approved coverage and color-mass limits; keep outline and weapon colors at their approved neutrals.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile"},
    {"constraintId": "char_bold_positive_restrained_ink_animation", "statement": "Use a restrained East Asian ink and animation-drawing treatment with limited flat, dry-brush, or watercolor accents subordinate to the bold silhouette.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::5. 전통 예술과 조형"}
  ]
}
```

Canonicalization is RFC 8785 JCS over the exact UTF-8 payload. Its registered
SHA-256 is exactly:

```text
dc5db9990f26dd1ed0ebc25c6c2b46a10b68cb4ca3248e69f7c27b28e1568b33
```

The executable profile contract test must reproduce this value before
activation.

Profile constants are all ranges, budgets, enumerated policies, counting rules,
locks, and forbidden values above. Planning must explicitly select the profile
and bind `fullBodyHeadCount`, `externalOutlineSourcePx`,
`internalLineSourcePx`, `facialMarkBudget`, `primaryHue`,
`primaryAnchorElements`, `maximumCharacterCoveragePercent`,
`maximumColorMasses`, `neutralOutlineColor`, and `neutralWeaponColor`.
`secondaryHue` and `secondaryAnchorElements` are jointly present or jointly
absent. Head count is a JSON number from 4 through 5. Outside outline is an
integer from 16 through 22 source pixels on 1024x1536; internal-line thickness
is a positive number whose ratio to the outside outline is at least 2. The
target equivalent is calculated by the exact 3/32 scale. `facialMarkBudget`
contains the exact counting unit, a positive maximum no greater than 9, and
component maxima no greater than 4/1/1/3. Coverage and mass limits are planning
integers within 1-35 percent and 1-4 masses. Anchor lists are non-empty, unique,
ordered exact element/site identifiers; secondary anchors are forbidden when
no secondary hue is present.

Every planning binding appears as an independently observable required or
prohibited statement and an exact `visualEvidenceMap` planning entry. Every
profile constant/lock has separate `authorityRole=expression_profile` evidence.
Prompt wording cannot substitute for a missing closed planning binding. A
conflict or omission blocks before prompt publication.

The three reported output images are noncanonical audit evidence unless a
reviewed future task publishes exact bytes at a durable project-relative path
under a closed style-reference schema. Mutable `output/` paths and unavailable
files never enter profile, prompt, routing, generation, or evaluation identity.

### Bold-outline accepted-result alignment profile

The following payload is canonical and immutable for
`projectbs_character_bold_outline_compressed_detail@2.0.0`. It is a successor,
not an amendment or alias of `@1.0.0`. It preserves the compact anatomy,
outline hierarchy, simplified face, bounded identity color, and Korean/Joseon
expression boundary while closing a moderately relaxed visible-mark budget and
an optional silhouette-support ink halo. It applies only to
`character_single_image_v2` after exact approved planning selection.

```json
{
  "expressionProfileKey": "projectbs_character_bold_outline_compressed_detail@2.0.0",
  "proportionProjection": {
    "fullBodyHeadCount": {"minimum": 4, "maximum": 5, "target": 4.5},
    "headToFullHeightPercent": {"minimum": 20, "maximum": 25},
    "limbPolicy": "shortened_compressed",
    "rejectNaturalisticAdultHeadCountRange": {"minimum": 6.5, "maximum": 8},
    "rejectLongLimbs": true,
    "rejectHeroicTallAnatomy": true
  },
  "outlineHierarchy": {
    "placement": "outside_silhouette",
    "externalOutlineTone": "bold_dark_neutral",
    "sourceCanvas": {"width": 1024, "height": 1536},
    "targetDisplaySize": {"width": 96, "height": 144},
    "externalOutlineSourcePx": {"minimum": 16, "maximum": 22},
    "sourceToTargetScale": "source_px_times_3_divided_by_32",
    "externalOutlineTargetPx": {"minimum": 1.5, "maximum": 2.0625},
    "minimumExternalToInternalThicknessRatio": 2,
    "internalLinePolicy": "materially_thinner_sparse_nonuniform",
    "silhouetteContinuity": "required"
  },
  "facialSimplificationBudget": {
    "countingUnit": "one_continuous_visible_mark_between_pen_lifts_or_intentional_breaks",
    "maximumTotalMarks": 9,
    "componentMaximums": {"browsAndEyes": 4, "nose": 1, "mouth": 1, "jawAndFaceShape": 3},
    "allowedComponents": ["brows", "eyes", "minimal_nose_mark", "mouth_line", "jaw_and_face_shape"],
    "forbidden": ["realistic_facial_modeling", "facial_hatching", "skin_shading", "facial_microtexture"]
  },
  "compressedDetailBudget": {
    "priority": "identity_points_before_surface_detail",
    "visibleMarkCountingUnit": "one_continuous_visible_dark_line_segment_between_pen_lifts_or_intentional_breaks",
    "totalVisibleMarkScope": "external_outline_segments_plus_internal_line_marks_excluding_color_mass_edges_and_halo",
    "internalMarkScope": "visible_line_marks_wholly_inside_external_silhouette_including_face_hair_garment_armor_equipment_weapon_and_folds",
    "maximumTotalVisibleMarks": 64,
    "maximumInternalLineMarks": 56,
    "maximumSecondaryFoldMarksPerGarmentRegion": 5,
    "internalMarksMustNotExceedTotalVisibleMarks": true,
    "identityGroups": ["face", "hair_or_headwear", "garment_silhouette", "armor_silhouette", "weapon"],
    "surfaceDetailPolicy": "omit_unless_identity_critical_and_planning_approved",
    "forbidden": ["dense_folds", "dense_armor_scale_enumeration", "dense_rivet_enumeration", "microtexture", "hatching", "modeled_shading", "realistic_material_rendering"]
  },
  "colorSignatureContract": {
    "planningBinding": "required_character_specific",
    "primaryHue": "required_exact_planning_value",
    "secondaryHue": "optional_exact_planning_value",
    "primaryAnchorElements": "required_non_empty_unique_ordered_planning_list",
    "secondaryAnchorElements": "required_non_empty_unique_ordered_planning_list_only_when_secondary_hue_present",
    "allowedSecondaryOchreAnchorSiteClasses": ["small_utility_pouch", "small_travel_accessory"],
    "secondaryOchrePolicy": "optional_only_when_exact_planning_hue_elements_and_allowed_site_classes_are_bound",
    "maximumCharacterCoveragePercent": {"minimum": 1, "maximum": 35},
    "maximumColorMasses": {"minimum": 1, "maximum": 4},
    "neutralOutlineColor": "required_exact_planning_value",
    "neutralWeaponColor": "required_exact_planning_value",
    "fullGarmentFill": "prohibited",
    "lineHierarchyOverride": "prohibited"
  },
  "inkHaloContract": {
    "planningSelection": "explicit_enabled_or_disabled_required",
    "disabledBranch": {"closedMembers": ["enabled"], "darkBackgroundAuthorization": "none"},
    "enabledBranch": {
      "treatment": "dark_neutral_translucent_silhouette_support_halo",
      "centerPolicy": "character_silhouette_center",
      "maximumOpacity": {"minimum": 0.08, "maximum": 0.35},
      "maximumCanvasCoveragePercent": {"minimum": 1, "maximum": 45},
      "extentPolicy": "single_centered_soft_halo_behind_silhouette",
      "edgeFalloff": "soft_monotonic_to_zero_alpha",
      "edgeAlpha": 0,
      "sceneDepiction": "prohibited",
      "opaqueBackground": "prohibited",
      "shadowSubstitute": "prohibited",
      "directionalCastShadow": "prohibited",
      "providerGenerationBackgroundRelation": "independent_from_removable_pale_generation_background"
    }
  },
  "inkTreatment": {
    "styleBoundary": "restrained_east_asian_ink_animation_drawing",
    "allowedAccentMethods": ["limited_flat_mass", "limited_dry_brush", "limited_watercolor"],
    "accentPriority": "subordinate_to_bold_silhouette_and_identity_lines",
    "silhouetteErosion": "prohibited",
    "photorealistic3dPbrWesternRealism": "prohibited"
  },
  "authoringProjectionContract": {
    "planningSelection": "explicit_approved_fact_required",
    "planningValuePolicy": "exact_values_required_within_profile_bounds",
    "requiredPlanningBindings": ["fullBodyHeadCount", "externalOutlineSourcePx", "internalLineSourcePx", "facialMarkBudget", "detailMarkBudget", "primaryHue", "primaryAnchorElements", "maximumCharacterCoveragePercent", "maximumColorMasses", "neutralOutlineColor", "neutralWeaponColor", "inkHalo"],
    "conditionalPlanningBindings": ["secondaryHue_requires_secondaryAnchorElements_and_secondaryAnchorSiteClasses", "inkHalo_enabled_requires_exact_closed_enabled_branch"],
    "evidencePolicy": "every_binding_budget_halo_member_and_lock_requires_exact_planning_or_profile_authority_evidence",
    "promptInclusion": "verbatim_locks_and_exact_planning_bound_values_required",
    "conflictPolicy": "block_before_prompt_publication"
  },
  "negativeStyleLock": [
    {"constraintId": "char_bold_v2_negative_naturalistic_tall_anatomy", "statement": "No naturalistic six-and-a-half-to-eight-head anatomy, long limbs, or heroic tall proportions.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_negative_uncontrolled_surface_detail", "statement": "No hatching, microtexture, modeled shading, realistic material rendering, dense folds, or dense enumeration of armor scales or rivets; do not exceed the approved visible, internal, or fold-mark budgets.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_negative_weak_or_uniform_outline", "statement": "No weak, uniform, or internal-line-equal silhouette outline and no accent or halo treatment that erases the outside contour.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_negative_realistic_face", "statement": "No realistic facial modeling, facial hatching, skin shading, facial microtexture, or marks beyond the approved facial budget.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_negative_unanchored_color", "statement": "No unanchored accent color, arbitrary or full-garment fill, excessive coverage, excessive color masses, or secondary ochre outside approved small utility or travel-accessory anchors.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_negative_scenic_opaque_halo", "statement": "No depicted scene, environment, opaque dark background, photographic or cinematic backdrop, directional cast shadow, or shadow-substitute halo.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_negative_realistic_rendering", "statement": "No photographic, photorealistic, painterly 3D, PBR, glossy cinematic, or western-realism rendering.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::10. 기본 시각 표현 경계"}
  ],
  "positiveStyleLock": [
    {"constraintId": "char_bold_v2_positive_compact_proportion", "statement": "Use a compact full-body proportion from four to five heads, centered near four-and-a-half heads, with shortened compressed limbs.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_positive_outline_hierarchy", "statement": "Use a bold dark outside-silhouette outline at the exact approved source thickness, at least twice the exact internal-line thickness; keep internal lines materially thinner, sparse, and nonuniform.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_positive_bounded_detail", "statement": "Prioritize identity points while allowing only the approved maximum total visible marks, internal line marks, and secondary fold marks per garment region.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_positive_simplified_face", "statement": "Represent the face only with high-signal brows and eyes, one minimal nose mark, one mouth line, and approved jaw or face-shape marks within the closed budget.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_positive_color_signature", "statement": "Apply approved identity hues only to their exact anchors; optional ochre may accent approved small utility pouch or travel-accessory anchors within coverage and color-mass limits.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_positive_optional_ink_halo", "statement": "When explicitly enabled, use one centered dark-neutral translucent silhouette-support halo that fades softly to zero alpha without depicting a scene or acting as a shadow.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile"},
    {"constraintId": "char_bold_v2_positive_restrained_ink_animation", "statement": "Use a restrained East Asian ink and animation-drawing treatment with limited flat, dry-brush, or watercolor accents subordinate to the bold silhouette.", "authorityRef": "AgentDocs/planning-guides/common/DisignMasterConcept_rule.md::5. 전통 예술과 조형"}
  ]
}
```

Canonicalization is RFC 8785 JCS over the exact UTF-8 payload. The registered
SHA-256 is
`5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3` and must
match the registry before activation.

Profile constants are the anatomy and outline ranges, face budget, exact
64/56/5 visible/internal/fold mark ceilings and counting scopes, allowed ochre
anchor-site classes, coverage/mass bounds, halo bounds and behavior, ink
treatment, locks, and forbidden values. Planning explicitly selects `@2.0.0`
and binds all required fields. `detailMarkBudget` repeats the exact counting
unit and planning maxima no greater than 64/56/5, with internal marks no greater
than total marks. Character color fields retain the v1 bindings; when a
secondary ochre hue is present, planning additionally binds non-empty exact
`secondaryAnchorSiteClasses` drawn only from `small_utility_pouch` and
`small_travel_accessory`.

Planning also binds a closed `inkHalo` union. Disabled is exactly
`{enabled:false}` and authorizes no dark background. Enabled includes exactly
`enabled:true`, one exact dark-neutral `color`, `maximumOpacity` from 0.08
through 0.35, `maximumCanvasCoveragePercent` from 1 through 45,
`centerPolicy=character_silhouette_center`,
`extentPolicy=single_centered_soft_halo_behind_silhouette`,
`edgeFalloff=soft_monotonic_to_zero_alpha`, `edgeAlpha=0`, and true
`noScene`, `noOpaqueBackground`, `noShadowSubstitute`, and
`noDirectionalCastShadow`. It is independent from the removable pale provider
generation background. Authoring cannot infer any enabled value from raster
evidence.

The accepted PNG at the reported mutable generation worktree path was verified
for contract review only: SHA-256
`a8b8aa6b5334cdc062380f9a7a553c99e2eb0c841d5bc2cc4416c577a84b71a2`,
1024x1536 ARGB, with compact anatomy, bold silhouette, bounded internal marks,
small ochre utility accents, and a centered dark-neutral treatment fading to
transparent edges. A deterministic four-pixel sampling pass observed zero-alpha
corners, nonzero-alpha coverage about 36.5 percent, and a nonzero-alpha bounding
box from `(52,40)` through `(840,1488)`; these are review observations, not
profile constants or inferred planning values. The absolute path and pixels are noncanonical and are not
members of this payload, a prompt, or any record identity. Canonical use still
requires a reviewed durable project-relative reference schema; otherwise only
explicit planning facts may bind the successor.

### Bold-outline attack motion-flow successor profile

The following payload is canonical and immutable for
`projectbs_character_bold_outline_attack_motion_flow@1.0.0`. It is an
animation-only composed successor of
`projectbs_character_bold_outline_compressed_detail@2.0.0`; it does not amend,
alias, rehash, or become selectable for any character single-image request.
The immutable reference prompt record continues to own the complete v2 payload
and character-specific projection. The successor becomes eligible only after
the reference bytes, v2 payload/hash, and exact 18px/8px, 64/56/5, color-anchor,
and bounded-halo projection have passed.

```json
{
  "expressionProfileKey": "projectbs_character_bold_outline_attack_motion_flow@1.0.0",
  "baseProfileBinding": {
    "expressionProfileKey": "projectbs_character_bold_outline_compressed_detail@2.0.0",
    "expressionProfilePayloadHash": "5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3",
    "inheritancePolicy": "hash_verified_reference_payload_and_projection_byte_preserving",
    "requiredExternalOutlineSourcePx": 18,
    "requiredInternalLineSourcePx": 8,
    "requiredMaximumTotalVisibleMarks": 64,
    "requiredMaximumInternalLineMarks": 56,
    "requiredMaximumSecondaryFoldMarksPerGarmentRegion": 5,
    "colorAnchorPolicy": "inherit_exact_reference_projection",
    "inkHaloPolicy": "inherit_exact_closed_reference_projection"
  },
  "animationApplicability": {
    "structureProfile": "animation_gif_frame_set_v2",
    "domainType": "character",
    "motionClass": "attack",
    "oneDownstreamUnit": "exactly_one_scalar_animationRequestId",
    "singleImageSelection": "prohibited"
  },
  "motionFlowContract": {
    "fadedIndigoSwordTorsoBrushFlow": {
      "direction": "exact_approved_motion_direction",
      "markCountPerActiveFrame": {"minimum": 3, "maximum": 5},
      "placement": ["sword_arc", "torso_rotation"],
      "opacityProgression": "directional_fade_along_approved_trajectory"
    },
    "grayBrownInertia": {
      "placement": ["shoulder", "hem"],
      "timing": "lag_then_settle_from_approved_key_pose_order",
      "role": "secondary_inertia_not_identity_replacement"
    },
    "darkNeutralInkTrajectory": {
      "placement": ["sword_path", "torso_action_axis"],
      "role": "bounded_action_trajectory_subordinate_to_identity_outline",
      "arbitrarySpeedLines": "prohibited",
      "magicVfx": "prohibited"
    }
  },
  "frameContinuityContract": {
    "poseEvolution": "ordered_key_poses_must_change_and_flow_without_static_repetition",
    "fixedCellScaleAnchor": "inherit_exact_animation_request_contract",
    "identityAnchorLocks": ["gaze", "face_shape", "topknot", "hand_sword_grip", "support_foot", "action_joints"],
    "equipmentAnchorLocks": ["costume_layers", "shoulder_armor", "travel_robe_hem", "hwando_structure", "hwando_hand_and_side"],
    "identityEquipmentDrift": "prohibited"
  },
  "authoringProjectionContract": {
    "selection": "exact_registered_successor_plus_exact_v2_reference_required",
    "requiredApprovedMotionBindings": ["motionDirection", "swordArc", "torsoRotation", "shoulderInertia", "hemInertia", "darkNeutralInkTrajectory", "keyPoseOrder", "frameContinuityAnchors"],
    "evidencePolicy": "every_motion_binding_lock_and_inherited_projection_requires_exact_planning_profile_or_reference_record_evidence",
    "promptInclusion": "exact_inherited_base_locks_plus_verbatim_motion_locks_and_bound_values_required",
    "conflictPolicy": "block_before_prompt_publication_or_provider_capability_access"
  },
  "negativeAnimationLock": [
    {"constraintId": "char_bold_motion_negative_static_pose_repetition", "statement": "No static pose repetition or duplicated action frame presented as motion.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"},
    {"constraintId": "char_bold_motion_negative_generic_clean_vector_sheet", "statement": "No generic clean-vector sprite sheet, uniform vector stroke treatment, or identity-neutral template motion.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"},
    {"constraintId": "char_bold_motion_negative_arbitrary_speed_lines_vfx", "statement": "No arbitrary speed lines, decorative motion streaks, aura, magic trail, or magic VFX beyond the approved bounded ink trajectories.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"},
    {"constraintId": "char_bold_motion_negative_identity_equipment_drift", "statement": "No drift in face, gaze, topknot, proportions, costume layers, shoulder armor, travel-robe hem, hand-to-sword grip, weapon structure, weapon side, support foot, or action joints.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"}
  ],
  "positiveAnimationLock": [
    {"constraintId": "char_bold_motion_positive_base_identity", "statement": "Preserve the hash-verified bold-outline v2 identity, compact proportion, exact 18px-to-8px hierarchy, 64/56/5 detail ceilings, color anchors, and bounded halo in every frame.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"},
    {"constraintId": "char_bold_motion_positive_indigo_flow", "statement": "Use three to five directional faded-indigo brush marks along the approved sword arc and torso rotation in each active attack frame.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"},
    {"constraintId": "char_bold_motion_positive_gray_brown_inertia", "statement": "Use subordinate gray-brown shoulder and hem lag then settlement according to the approved key-pose order.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"},
    {"constraintId": "char_bold_motion_positive_ink_trajectory_continuity", "statement": "Use bounded dark-neutral ink trajectory on the sword path and torso action axis while preserving fixed-cell continuity and every identity and equipment anchor.", "authorityRef": "AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile"}
  ]
}
```

Canonicalization is RFC 8785 JCS over the exact UTF-8 payload. The registered
SHA-256 is `1c828ef73b1de41453197f0d2fef80eebb069e42767d3f017ccb8dab0b947c8c` and must match the registry before
activation. Array order is normative and every object is closed.

The successor is selected only by the character-animation router from one
hash-verified bold v2 reference prompt record plus all eight exact approved
motion bindings. It is never selected by character-single-image authoring and
does not alter that record's key, payload, hash, provider prose, or output.
Router, authoring, generation pre-submit, and evaluation each revalidate the
base binding and motion projection independently; no stage may infer motion
direction, add speed lines or VFX, or repair identity/equipment drift.

## Character Single Image

Require identityConsistencyLock, one viewpoint, pose, framing, canvas, target
display size, safe area, background/no-shadow, exact outline, and
pelvis/root-ground-axis anchor. The brief represents one approved image only.
Do not add directions, rotations, alternate views or camera facts.

Provider handoff: `imagegen_character_single_image_prompt_v2`, prompt v3,
structure `character_single_image_v2`.

For any registered lock-array profile, the cohesive prompt contains the
complete positive and negative style locks, not merely their IDs. For the
sparse-ink profile, it instead contains the complete projection of all eight
closed policy members. Evidence coverage reports the applicable groups or
policy members independently. An
approved age, face, facial-hair, fatigue, or attractiveness statement may be
rendered only from that character's evidence; no default youth,
modern/westernized beauty, minor-coded appearance, sexualization, beard,
fatigue, aging, or gravitas is added.

For `projectbs_character_open_ink_wash_dynamic_contour@1.0.0`, the cohesive
prompt contains all eleven closed policy members, both ordered seven-item lock
arrays, and the exact planning-bound Seojin identity/equipment anchors. It
projects 4-5 heads targeted at 4.25, young-adult/no-child presentation, 35-55
percent contour omission targeted at 45, tactile pressure-variable mok-seon,
broad bleeding/misaligned watercolor-pastel masses, the three separate palette
roles, at least 70 percent achromatic/unpainted space in both scopes, and the
warm-ivory removable-solid/no-halo/no-scene/no-shadow background contract. The
accepted-reference SHA becomes a provider style reference only through the
exact reviewed durable six-member binding. Authoring verifies its record/index,
keeps it out of identity/equipment evidence and provider prose, and copies it
unchanged into `referenceBindings`; incomplete or unreviewed input creates no
binding.

The same external binding is valid for open ink-wash v2 because its review
record lists the exact v2 key/hash. This does not change either profile payload.
The provider-facing reference role is `style_only`; authoring must not describe
the reference person's face, pose, action, clothes, or equipment.

### Deterministic open ink-wash v2 authoring projection

For a new `projectbs_character_open_ink_wash_dynamic_contour@2.0.0`
`character_single_image` publication, prose choice is not an authoring degree
of freedom. The producer reads every hash-significant authority input from the
freshly fetched commit's raw Git blob, verifies strict UTF-8, no BOM, LF-only
bytes and the recorded raw SHA-256, and only then parses JSON. A CRLF checkout
copy is never normalized into authority and never participates in identity.

The validated routing `/authoringHandoff` is the sole planning projection. Its
`requiredElements` has exactly these ten source-order slots: proportion and
age; simplified identity surface and shoulder-armor mass; contour omission;
mok-seon execution; pigment execution; palette roles; negative space;
generation background; Korean/Joseon identity and equipment; then viewpoint,
pose, framing, and no-shadow composition.

Its `prohibitedElements` has exactly these fourteen source-order slots:
child/tall anatomy, clean contour, surface over-detail, clean fill/splashes,
palette-role collapse, negative-space shortfall, halo/background/scene/shadow,
style-reference semantic transfer, nonconformant-final labeling, non-Korean
identity substitution, photographic/3D rendering, multi-view output,
animation/variant output, and failed-preview/trial reuse. A missing, additional,
reordered, or semantically mismatched slot is `provider_value_invalid`; the
producer does not repair it with prose.

The visual brief projection is exact:

- `requiredVisualStatements` and `prohibitedVisualStatements` copy the two
  arrays in source order. Constraint IDs are respectively
  `routing_required_01` through `_10` and `routing_prohibited_01` through
  `_14`.
- `primarySubjectOrSilhouette`, `visualHierarchy`, `composition`,
  `backgroundPolicy`, and `outlinePolicy` copy required slots 1, 2, 10, 8,
  and 3. `paletteAndMaterial` is the LF join of required slots 4 through 7.
  `anchorPolicy` is `canonicalJson(singleImageSpecification.anchor)` with no
  surrounding whitespace.
- `supportingElements` and `likelyWrongObjects` are exact empty arrays.
  `artifactSpecificBrief` is the byte-semantic copy of routing
  `typeSpecification`; `referenceBindings`, when present, is the exact
  top-level six-member routing binding.
- `planningOriginalRef`, registry/profile identity, the complete profile,
  locks, fixed validation values, and the four-member
  `providerTranslationContract` are copied exactly as their closed schemas
  require. The latter has the exact scalar
  `promptAssemblyOrder=planning_facts,negative_style_lock,positive_style_lock`.

`visualEvidenceMap` order is identity-bearing: ten required entries, fourteen
prohibited entries, the seven derived summary fields in the order displayed
above, every leaf of `artifactSpecificBrief` by ascending RFC 6901 pointer, the
seventeen non-lock profile members by lexicographically sorted member name, nine
negative locks, nine positive locks, then the six style-binding members in
`role`, `projectRelativePath`, `sha256`, `reviewRecordId`, `reviewRecordPath`,
`reviewRecordSha256` order. Routing-derived entries cite the routing record
path/raw SHA and exact `/authoringHandoff` pointer. Profile entries cite this
guide's raw Git-blob SHA and exact successor-profile section/pointer. IDs are
the fixed slot ID where one exists; otherwise they are
`authoring_evidence_` plus the first twenty hex characters of SHA-256 over the
UTF-8 bytes of `statementPath + "|" + sourcePointer`. Arrays retain source
order; object/leaf traversal uses lexicographically sorted RFC 6901 pointers.

The provider text for this profile is exactly 28 non-empty lines joined by one
LF with no leading/trailing whitespace and no terminal LF: the ten exact
`requiredElements`, the nine exact negative-lock statements, then the nine
exact positive-lock statements. It contains no heading, blank line, summary,
gestalt sentence, prohibited/audit/receipt transcript, binding metadata, or
author-written synonym. The Markdown layer alone appends its one terminal LF.
Two producers over the same raw Git blobs therefore must produce identical
visual brief, provider text, prompt payload/record, Markdown, index-after, and
generation handoff bytes and identities. Any difference before publication is
`record_identity_mismatch`; both candidates remain non-authoritative and no
artifact is written.

This rule applies only to new records selected by the exact v2 key. Published
prompt records and indexes are legacy read-only evidence and are never
reprojected, normalized, renamed, or rewritten under this clarification.

### Provider-facing salience and deduplication gate

`scenePromptOriginal` is executable art direction, not an evidence transcript.
The visual brief and evidence map preserve complete planning/profile
provenance; the provider prompt MUST NOT repeat those records as bookkeeping
blocks. Except for the exact open ink-wash v2 projection above, for every
`character_single_image` lock-array profile, assemble the provider prompt as
one concise priority ladder while retaining the normative
negative-lock order followed by the normative positive-lock order:

1. lead with one compact hard-output block containing the planning-bound body
   proportion, silhouette/contour treatment, and generation-background result;
2. describe identity, pose, costume, equipment, and weapon anchors once, with
   identity-critical shape before material or surface language;
3. describe line, pigment, palette-role, and negative-space execution once;
4. include every negative lock statement exactly once in normative order and
   every positive lock statement exactly once in normative order;
5. end with one short measurable gestalt check. It may restate a planning-bound
   number or short prohibition once, but MUST NOT copy a lock statement or a
   prior paragraph again.

The provider prompt MUST NOT contain record IDs, routing IDs, hashes, absolute
or project-relative evidence paths, authority labels, provider/workflow labels,
or headings such as `APPROVED REQUIRED STATEMENTS`, `APPROVED PROHIBITED
STATEMENTS`, or `ORDERED ... LOCKS`. Those values remain in the closed record,
visual brief, evidence map, and validation output. Use one provider language;
retain an untranslated Korean/Joseon term only when it is an approved identity
anchor, not as a bilingual duplicate of the same instruction.

When `generationBackground.mode=removable_solid`, provider prose describes only
the submitted generation canvas: one uniform edge-to-edge approved solid color
with no luminance falloff, dark corners, radial gradient, halo, vignette,
scene, or shadow when those are prohibited. Do not mention the later
transparent-final or background-removal operation in `scenePromptOriginal`;
that belongs to preservation/packaging and remains in the brief and settings.

Equipment identity does not authorize surface topology. When planning/profile
prohibits armor scales, rivets, microtexture, dense folds, or modeled material,
describe the approved armor as a simplified interrupted identity mass and state
that repeated plates/scales/rivets are absent. Do not repeat detailed equipment
nouns in multiple sections or add realistic material adjectives merely to
preserve identity.

Before publication, normalize line endings and verify all of the following:

- each exact lock statement occurs once and only once;
- no raw hash, path, record/workflow label, or evidence heading entered provider
  prose;
- a removable-solid prompt contains no `transparent final` or background-
  removal instruction;
- each primary measurable concept occurs no more than twice: once in its hard
  instruction or exact lock, and optionally once in the final gestalt check;
- each approved identity/equipment fact is projected once outside the exact
  lock arrays and cannot weaken its applicable simplification prohibitions.

Any violation is `provider_value_invalid`. This is a provider-payload assembly
failure; it does not mutate the selected immutable expression profile or any
planning fact.
For `projectbs_character_open_ink_wash_dynamic_contour@2.0.0`, all nineteen
closed members, both ordered nine-item lock arrays, and the seven output gates
remain complete in the visual brief and evidence map. Provider prose uses only
the exact 28-line executable projection above; compact-receipt, review,
workflow, and post-output classification language never enters it. This
successor is selected only by a new exact planning pointer; v1 and all
published prompt records remain governed by their stored bytes.

For `projectbs_character_bold_outline_compressed_detail@1.0.0`, the cohesive
prompt contains every lock plus the exact planning-bound head count, external
and internal source-pixel thicknesses, calculated thickness ratio, facial mark
budget, primary and conditional secondary hue/anchor lists, coverage/mass
limits, and neutral outline/weapon colors. Surface-detail prose cannot weaken
the compressed-detail budget. Missing planning bindings, a value outside the
closed ranges, missing evidence, or omitted provider prose blocks before prompt
publication.

For `projectbs_character_bold_outline_compressed_detail@2.0.0`, the cohesive
prompt also contains the exact 64/56/5-or-narrower detail projection,
conditional ochre anchor-site classes, and every member of the selected closed
halo branch. Enabled halo prose remains a centered translucent support
treatment distinct from the removable provider background; disabled prose
does not authorize any dark background. Every successor value and all seven
positive plus seven negative locks have independent evidence coverage.

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

The sole composed exception is the bold-outline attack motion-flow successor.
It preserves the exact reference v2 key/payload/hash as its base binding and
stores the separate successor key/payload/hash; it never changes or rehashes
the reference. All eight planning-owned motion bindings are required before
the successor payload can be projected.

## Failure, State, and Validation

Use the exact common/type blockers in GeneratedMediaImageGenOnlyContractGuide.md,
including missing evidence/identity/background/outline/anchor/reference/fixed-
cell/master-first/snapshot conditions. State is
`planning_verified -> brief_normalized -> provider_payload_ready`; any blocker
stops before a prompt record is written.

Validate schema parity, visualBriefId/hash, evidence coverage, registry/profile,
anchor discriminator, one provider payload, and no planning invention. Current
provider is ImageGen only.

Character validation requires the two non-empty lock arrays for a registered
lock-array profile or all eight exact policy members for the sparse-
ink profile. It requires direct inclusion of the complete applicable
projection in `scenePromptOriginal`, separate evidence coverage, and exact
reference/main-image payload/hash equality for character animation. Skill animation must omit all four character
reference/profile fields. Typed blockers are
`missing_positive_style_lock`, `missing_negative_style_lock`,
`style_lock_evidence_incomplete`, `provider_prompt_style_lock_missing`,
`character_style_profile_conflict`, `missing_reference_prompt_record`,
`reference_prompt_record_hash_mismatch`, `missing_expression_profile_key`,
`missing_expression_profile_payload`, `missing_expression_profile_payload_hash`, `expression_profile_key_mismatch`,
`expression_profile_payload_hash_mismatch`,
`unexpected_character_style_reference`, and
`character_animation_style_lock_mismatch`.

The open ink-wash v1 profile additionally requires all eleven exact policy
members, both ordered lock arrays, exact planning-bound identity/equipment
anchors, and direct provider-prose coverage. It is single-image-only and cannot
be inherited by animation. Use only the central `open_ink_wash_*` authoring,
generation, and evaluation tokens for its profile-specific gates. A missing
durable reference path is not a blocker because the accepted image is
audit-only; any attempt to bind an absolute/transient path or use the image for
identity/editing is `open_ink_wash_reference_role_invalid`.

The v2 successor instead requires all nineteen exact members, ordered 9+9
locks, and the distinct `open_ink_wash_v2_*` authoring tokens. Generation uses
the added pre-submit surface-detail blocker and, after a returned preview, the
seven `character_preview_open_ink_wash_v2_*` conformance tokens. A compact
receipt is response-only and has no record/index/CAS projection.

For the composed motion-flow successor, equality means exact base/reference
equality plus exact successor registry equality, not equality between the two
different keys. Validate the base first, then the successor and its ordered
animation locks, using only the motion-flow router/authoring/generation/evaluation
tokens owned by the central contract.

For the sparse profile, use only `missing_sparse_profile_projection`,
`sparse_profile_projection_mismatch`, `sparse_profile_evidence_incomplete`, or
`provider_prompt_sparse_projection_missing` for projection readiness. The two
top-level lock arrays remain exact empty arrays for visual-brief schema
compatibility and MUST NOT trigger any `missing_*_style_lock` token.

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

For `projectbs_character_sparse_ink_pastel_motion@1.0.0`, generation and
evaluation apply the exact sparse contour, pigment, motion, and identity-anchor
failure tokens defined by the current ImageGen-only contract and the character
expression evaluation guide. They use observable evidence and never fabricate
computer-vision measurements.

For `projectbs_character_bold_outline_compressed_detail@1.0.0`, authoring uses
the exact profile-specific tokens in the current ImageGen-only contract for
missing/out-of-range proportion, missing/invalid outline hierarchy, missing or
exceeded facial marks, missing/conflicting compressed detail, missing/invalid
color signature, evidence omission, and prompt-projection omission. Generation
and evaluation independently apply the five corresponding no-submit/fatal
gates for proportion, outline hierarchy, facial marks, compressed detail, and
color signature. A prompt sentence never substitutes for a missing closed
planning value or evidence pointer.

For `projectbs_character_bold_outline_compressed_detail@2.0.0`, authoring adds
the exact v2 detail/color-anchor/halo blockers. Generation and evaluation reuse
the inherited proportion, hierarchy, and facial gates and independently apply
the v2 detail, color-anchor, and halo gates. Insufficient reproducible
evaluation evidence is `character_evaluation_evidence_insufficient`, not an
inferred pass.

For `projectbs_character_open_ink_wash_dynamic_contour@1.0.0`, authoring,
generation, and evaluation independently gate proportion/age, open contour and
mok-seon, pigment/palette/negative space, background, identity/equipment, and
reference role. Generation is no-submit on any failure. Evaluation uses
reproducible observable evidence and returns
`character_evaluation_evidence_insufficient` instead of guessing exact
percentages or stroke phases.

For `projectbs_character_open_ink_wash_dynamic_contour@2.0.0`, the same
pre-submit semantic checks add the closed surface-detail gate. Returned preview
pixels must then pass the seven ordered output gates before the response may be
`preview_conformant_no_downstream`; fail or insufficient evidence returns
`stop_no_retry_not_final` without retry or downstream action. Formal evaluation
remains separate and independently validates media evidence.

## Related Guides

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
