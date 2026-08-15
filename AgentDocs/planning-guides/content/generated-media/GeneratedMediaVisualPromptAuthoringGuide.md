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
`authoringProjectionContract`. The sparse-ink profile below has exactly its
discriminator plus the eight budget/policy members displayed in its canonical
payload; it has no lock arrays. The bold-outline compressed-detail profile has
exactly its discriminator, `proportionProjection`, `outlineHierarchy`,
`facialSimplificationBudget`, `compressedDetailBudget`,
`colorSignatureContract`, `inkTreatment`, `authoringProjectionContract`, and
the two lock arrays displayed in its canonical payload. A member from one shape
is not optional in another shape. Each lock-array item in the three lock-array profiles has exactly
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

For `projectbs_character_bold_outline_compressed_detail@1.0.0`, the cohesive
prompt contains every lock plus the exact planning-bound head count, external
and internal source-pixel thicknesses, calculated thickness ratio, facial mark
budget, primary and conditional secondary hue/anchor lists, coverage/mass
limits, and neutral outline/weapon colors. Surface-detail prose cannot weaken
the compressed-detail budget. Missing planning bindings, a value outside the
closed ranges, missing evidence, or omitted provider prose blocks before prompt
publication.

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

## Related Guides

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
