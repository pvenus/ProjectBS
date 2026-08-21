# Generated Media Authoring Profile Registry Guide

## Purpose

Guide Type: current closed registry. The current router is pinned only to:

```text
registryVersion=generated_media_authoring_profile_registry_v2
provider=imagegen
```

Legacy v1 rows are not present here and are owned by
GeneratedMediaLegacyV1CompatibilityGuide.md.

## Canonical Key and Rows

Profile IDs are lowercase snake_case and versions are exact MAJOR.MINOR.PATCH.
No aliases, latest-version selection, or caller override is allowed.

| rowId | assetType | domainType | profileKey | selectedPipeline | selectedAuthoringPrompt | selectedGenerationPrompt | structureProfile |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `character_single_image_v2` | `character_single_image` | `character` | `character_single_image@2.0.0` | `imagegen_character_single_image` | `AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImagePromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenCharacterImageGenerationPrompt.md` | `character_single_image_v2` |
| `skill_icon_single_image_v2` | `icon_single_image` | `skill` | `skill_icon@2.0.0` | `imagegen_icon_single_image` | `AgentDocs/task-prompts/content/generated-media/ImageGenIconPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenIconGenerationPrompt.md` | `icon_single_image_v2` |
| `item_icon_single_image_v2` | `icon_single_image` | `item` | `relic@2.0.0` | `imagegen_icon_single_image` | `AgentDocs/task-prompts/content/generated-media/ImageGenIconPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenIconGenerationPrompt.md` | `icon_single_image_v2` |
| `stage_background_single_image_v2` | `background_single_image` | `stage` | `stage_background@2.0.0` | `imagegen_background_single_image` | `AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `battle_background_single_image_v2` | `background_single_image` | `battle` | `battle_background@2.0.0` | `imagegen_background_single_image` | `AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `environment_background_single_image_v2` | `background_single_image` | `environment` | `environment_background@2.0.0` | `imagegen_background_single_image` | `AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `character_animation_v2` | `animation` | `character` | `character_animation@2.0.0` | `imagegen_animation` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |
| `skill_animation_v2` | `animation` | `skill` | `skill_animation@2.0.0` | `imagegen_animation` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |

Exactly one row must match. Zero returns `unsupported_current_route`; multiple
returns `conflicting_routing_evidence`. Every selected prompt must exist and
directly reference ContentFolderStructureGuide.md.

The registry exposes exactly four execution roles: character single image,
icon single image, background single image, and one animation request. Icon
rows accept only `skill|item`; background rows accept only
`stage|battle|environment`. Evidence compatible with both image roles blocks
as `ambiguous_image_role`; no similarity fallback is allowed.

### Character expression profiles

The two character rows additionally resolve one registered reusable expression
profile; it does not replace either row's routing `profileKey`. Existing
planning without an explicit expression-profile selection continues to resolve
the immutable legacy-compatible profile:

```text
expressionProfileKey=projectbs_character_restrained_ink_line@1.0.0
expressionProfilePayloadHash=bda082ffe297c29cdc6b933a6c219ae67b11ae38bc784c198e4603c1741199cf
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile
appliesTo=character_single_image_v2,character_animation_v2
```

The following new immutable profile is available only when an approved planning
fact explicitly selects its exact key for a new character single-image request:

```text
expressionProfileKey=projectbs_character_animation_ready_minimal_ink_line@1.0.0
expressionProfilePayloadHash=de3339457f05c3dfd6fb6f854c102079c5c14f54d908a474cca093943afc7e06
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Animation-ready minimal ink-line profile
appliesTo=character_single_image_v2,character_animation_v2
selection=explicit_approved_planning_fact_required
```

The following new sparse-ink/pastel-motion profile is also available only by
one exact approved planning pointer. It is shared byte-for-byte by a character
main image and its character-animation descendants:

```text
expressionProfileKey=projectbs_character_sparse_ink_pastel_motion@1.0.0
expressionProfilePayloadHash=b5ce18d11e249598ad6da13d59340cf7cede3d2896259dcc5e02dbbf98e80443
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Sparse ink pastel motion profile
appliesTo=character_single_image_v2,character_animation_v2
selection=explicit_approved_planning_fact_required
```

The following open ink-wash dynamic-contour profile is a separate additive
single-image-only selection. It neither inherits from nor changes any existing
profile:

```text
expressionProfileKey=projectbs_character_open_ink_wash_dynamic_contour@1.0.0
expressionProfilePayloadHash=37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash dynamic-contour character profile
appliesTo=character_single_image_v2
selection=explicit_approved_planning_fact_and_complete_projection_required
```

The output-conformance successor is a distinct major version. It does not
change or reinterpret v1 planning, prompt, preview, payload, hash, or locks:

```text
expressionProfileKey=projectbs_character_open_ink_wash_dynamic_contour@2.0.0
expressionProfilePayloadHash=b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Open ink-wash output-conformance successor profile
appliesTo=character_single_image_v2
predecessorExpressionProfileKey=projectbs_character_open_ink_wash_dynamic_contour@1.0.0
predecessorExpressionProfilePayloadHash=37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd
selection=explicit_approved_planning_fact_and_complete_successor_projection_required
```

The opaque-chroma provider-master successor preserves the exact open-ink v2
base key/hash and replaces only its generation-background projection. It is a
new single-image profile, not a reinterpretation of v2 or a direct-alpha lane:

```text
expressionProfileKey=projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0
expressionProfilePayloadHash=b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashOpaqueChromaSuccessorGuide.md::Canonical payload
appliesTo=character_single_image_v2
baseExpressionProfileKey=projectbs_character_open_ink_wash_dynamic_contour@2.0.0
baseExpressionProfilePayloadHash=b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5
providerMasterContract=generated_media_opaque_chroma_provider_master_v1
postprocessOwnerRole=generated_media_chroma_uncomposite
selection=explicit_approved_planning_fact_and_complete_successor_projection_required
```

The following registered execution profile composes that expression profile
without modifying it. It is a target-specific execution authority, not an
expression-profile successor and not a source-bound edit route:

```text
executionProfileKey=projectbs_character_open_ink_opaque_chroma_identity_anchored_regeneration@1.0.0
executionProfilePayloadSha256=44d3bafcc720d39ac260fb2089798c16f9ec1f50d391165eea676dbc79cdc3ad
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/helpers/generated_media_identity_anchored_opaque_chroma_execution_profile_v1.json
guideAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaIdentityAnchoredOpaqueChromaExecutionGuide.md
appliesTo=character_single_image_v2+character+character.seojin.2
expressionProfileKey=projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0
expressionProfilePayloadHash=b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a
executionMode=builtin_imagegen_authenticated_identity_anchored_single_submit_v1
selection=exact reviewed identityAnchoredGenerationSelection required
```

The animation-only open-ink attack successor composes the exact open-ink v2
reference without changing that profile or the existing sparse-motion profile:

```text
expressionProfileKey=projectbs_character_open_ink_wash_attack_motion@1.0.0
expressionProfilePayloadHash=07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashAttackMotionSuccessorGuide.md::Canonical payload
appliesTo=character_animation_v2
baseExpressionProfileKey=projectbs_character_open_ink_wash_dynamic_contour@2.0.0
baseExpressionProfilePayloadHash=b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5
requiredTransparentForegroundProjectionKey=generated_media_true_alpha_foreground@1.0.0
requiredTransparentForegroundProjectionPayloadHash=2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108
selection=exact_open_ink_v2_reference_plus_approved_attack_motion_and_true_alpha_bindings_required
```

The following bold-outline compressed-detail profile is available only for a
new character single-image request whose approved planning selects the exact
key and supplies every closed character-specific projection binding:

```text
expressionProfileKey=projectbs_character_bold_outline_compressed_detail@1.0.0
expressionProfilePayloadHash=dc5db9990f26dd1ed0ebc25c6c2b46a10b68cb4ca3248e69f7c27b28e1568b33
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline compressed-detail character profile
appliesTo=character_single_image_v2
selection=explicit_approved_planning_fact_and_projection_required
```

The accepted-result-aligned successor is a separate major version. It does not
change the preceding key, payload, hash, locks, or any record that stores them:

```text
expressionProfileKey=projectbs_character_bold_outline_compressed_detail@2.0.0
expressionProfilePayloadHash=5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline accepted-result alignment profile
appliesTo=character_single_image_v2
selection=explicit_approved_planning_fact_and_successor_projection_required
```

The animation-only motion-flow successor composes, but never rewrites, one
hash-verified bold v2 reference prompt record:

```text
expressionProfileKey=projectbs_character_bold_outline_attack_motion_flow@1.0.0
expressionProfilePayloadHash=1c828ef73b1de41453197f0d2fef80eebb069e42767d3f017ccb8dab0b947c8c
canonicalPayloadAuthority=AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Bold-outline attack motion-flow successor profile
appliesTo=character_animation_v2
baseExpressionProfileKey=projectbs_character_bold_outline_compressed_detail@2.0.0
baseExpressionProfilePayloadHash=5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3
selection=exact_v2_reference_plus_exact_approved_attack_motion_bindings_required
```

Character single-image selection is closed: no approved selection resolves the
legacy-compatible key, one exact approved selection of a registered nondefault
key resolves that exact profile, and any unknown, multiple, or conflicting selection blocks as
`character_style_profile_conflict`. Character animation normally inherits the
exact key, payload, and hash from its hash-verified immutable reference prompt
record. The only exceptions are the exact registered animation-only composed
successors, each of which stores a new payload/hash while binding its own
unchanged v2 reference key/hash. This preserves every old prompt record
byte-for-byte and prevents a registry revision from reinterpreting its identity.

Inheritance is also scope-checked. A character-animation request may inherit
only a profile whose `appliesTo` includes `character_animation_v2`. Both
bold-outline compressed-detail versions remain intentionally single-image-only;
a direct animation inheritance attempt blocks as
`character_style_profile_conflict`. The reviewed exceptions are composition of
the exact bold v2 reference into its registered motion-flow successor and the
exact open-ink v2 reference into its registered open-ink attack successor.

The canonical authority above is the sole owner of the closed payload, lock
order, canonicalization, and hash algorithm. This registry is only a closed
key/hash projection and must not duplicate or redefine lock statements. Approved
planning continues to own gender/age presentation, face, hair, costume,
equipment, weapon, palette, materials, pose and motion. Character prompt
records must persist the exact expression profile key, payload, and payload hash.
Character animation normally inherits all three byte-for-byte from its immutable
approved reference prompt record. A registered composed successor instead
preserves those three as its immutable base and adds its own exact
key/payload/hash; it never substitutes or rewrites the base. The bold successor
and open-ink successor are disjoint by exact base key/hash and their own closed
eligibility rules. A different requested expression
requires a reviewed registry/profile version and explicit planning approval; no
caller alias or silent override is allowed.

The opaque-chroma successor is also disjoint: it is eligible only for
`character_single_image_v2`, exact 1024x1536 PNG, and exact opaque removable
`#00FF00`. It forbids `transparentForegroundSelection`; an old direct-alpha or
warm-ivory handoff is never rewritten into this row.

The registry validates the closed discriminated projection without owning its
values. The legacy-compatible payload has exactly the original three members.
The animation-ready payload additionally requires the four exact members shown
below. The sparse-ink payload instead has exactly `expressionProfileKey` plus
`contourOmissionBudget`, `lineHierarchy`, `negativeSpacePolicy`,
`pigmentBudget`, `accentPalette`, `pigmentApplication`, `motionLinePolicy`, and
`identityAnchors`, using the exact closed value and hash owned by the visual
guide; it has no positive/negative lock arrays. The bold-outline payload has
exactly `expressionProfileKey`, `proportionProjection`, `outlineHierarchy`,
`facialSimplificationBudget`, `compressedDetailBudget`,
`colorSignatureContract`, `inkTreatment`, `authoringProjectionContract`,
`negativeStyleLock`, and `positiveStyleLock` with the exact canonical value and
hash owned by the visual guide.
The open ink-wash payload has exactly `expressionProfileKey`, `applicability`,
`proportionAndAgeContract`, `contourOmissionBudget`, `mokSeonContract`,
`pigmentApplicationContract`, `paletteRoleContract`, `negativeSpaceContract`,
`backgroundContract`, `identityAnchorContract`,
`acceptedStyleReferenceContract`, `authoringProjectionContract`,
`negativeStyleLock`, and `positiveStyleLock`. It is single-image-only; its
audit-only accepted SHA has no canonical path/reference binding until exact
bytes are separately reviewed and published at a durable project-relative path.
The reviewed publication now resolves only through
GeneratedMediaStyleReferenceBindingGuide.md and its exact six-member
`role=style_only` binding. This external record does not change this registry
row or either open ink-wash payload hash.
The open ink-wash v2 successor has exactly `expressionProfileKey`,
`predecessorBinding`, `applicability`, `proportionAndAgeContract`,
`proportionMeasurementContract`, `contourOmissionBudget`, `mokSeonContract`,
`pigmentApplicationContract`, `paletteRoleContract`, `negativeSpaceContract`,
`surfaceDetailContract`, `backgroundContract`, `identityAnchorContract`,
`acceptedStyleReferenceContract`, `providerOutputConformanceContract`,
`compactConformanceReceiptContract`, `authoringProjectionContract`,
`negativeStyleLock`, and `positiveStyleLock`. It is self-contained and must not
be constructed by mutating or merging the v1 payload at run time.
The animation-only motion-flow successor has exactly `expressionProfileKey`,
`baseProfileBinding`, `animationApplicability`, `motionFlowContract`,
`frameContinuityContract`, `authoringProjectionContract`,
`negativeAnimationLock`, and `positiveAnimationLock`. Its base key/hash are
fixed to bold v2; the two animation lock arrays are non-empty and order is
normative.

```yaml
expressionProfilePayload:
  expressionProfileKey: exact registered key
  negativeStyleLock:
    - constraintId: required string
      statement: required string
      authorityRef: required project-relative-path::section string
  positiveStyleLock:
    - constraintId: required string
      statement: required string
      authorityRef: required project-relative-path::section string
  proportionProjection: required only for animation-ready profile; exact closed canonical value
  detailDensityBudget: required only for animation-ready profile; exact closed canonical value
  colorValueBudget: required only for animation-ready profile; exact closed canonical value
  authoringProjectionContract: required only for animation-ready profile; exact closed canonical value
```

Every lock item has exactly the three displayed members. Arrays preserve the
canonical authority's displayed order; object members use RFC 8785 JCS ordering
for hashing. A character image record must store the selected canonical payload
and registered hash. Its character animation descendant must copy both unchanged
from the hash-verified immutable reference record; resolving the same key again
and constructing a fresh payload is not inheritance.

## Extension and Validation

Adding a domain requires approved planning schema/profile, new non-overlapping
registry version, preservation/evaluation adapters, guide/prompt evaluation,
then router activation. Add a row/profile, never a copied domain prompt.

Validate unique keys, ImageGen-only provider, existing prompt paths, exact
structure profile, type-specific anchor discriminator, exact character
expression profile/lock hash, and failure/readiness parity. Current registry
changes never reinterpret an immutable record.

## Related Guides

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```

## Exact source-bound MAIN completion rows

GeneratedMediaSourceBoundMainCompletionGuide.md registers two disjoint
post-generation rows. G2 uses
`projectbs_character_open_ink_source_bound_green_carrier_fit@1.0.0` /
`ca3102e7369da6513b4c4e462e68b373da618a2b1630a393d803d37136d812df`
for one no-provider deterministic uncomposite-and-fit tuple. G3 uses
`projectbs_character_open_ink_source_bound_single_edit@1.0.0` /
`aa65434f5fb9c22cb42db199c936ee414648b933f4b83c159065341f4e704011`
for one authenticated immutable-source edit with submit maximum one and retry
maximum zero. These are not prompt-authoring expression profiles and cannot
replace, merge with, or reinterpret an existing expression-profile row.

The G3 post-generation row's only registered route projection contract is
`projectbs_generated_media_source_bound_character_edit_route@1.0.0` /
`77b4b9d4d9d5db7a2c2fb1cdb5ccb1812faffe535559fdb57400515f48e05359`.
It closes identity and transport for that exact profile/source/receipt only;
it is not an expression profile and creates no alias for other rows.

The exact G2 residual-carrier correction is an additive postprocess successor:
`projectbs_character_open_ink_source_bound_green_carrier_fit@2.0.0` /
`84db44afba6bce328a51f078f2147055846f282de71b2c56b9d7876264f9bccf`.
It preserves the v1 fit profile/hash and geometry, selects only the same exact
source/receipt plus rejected-v1 evidence, and authorizes no other content or
general chroma relaxation.

The distinct G3 edited-source postprocess binding is
`projectbs_character_open_ink_source_bound_green_carrier_fit_g3_edit@1.0.0` /
`f1b9563f271334c5addbf780bec1bca886f540d1a804e93684f56774c516a086`.
It selects only source `7394278aac0553bd7f0967f84ec5654a61de438efde4626c439d3f64cead3e4a`
and edit receipt `df9921b80222ab4a3a59f5dd35753d48e8988d76e4ea7b81cf690a522a453cc3`;
it is not an alias or shared fixture with G2.

Final exact postprocess successors are registered independently:
`projectbs_character_open_ink_source_bound_green_carrier_fit@3.0.0` /
`5188d2bd92fdf22dded70fe8e3ab60f1fee1aa79ac6072845883072d99a875c2`
for the G2 opaque-carrier/isolation cleanup, and
`projectbs_character_open_ink_source_bound_green_carrier_fit_g3_edit@2.0.0` /
`40cf8dcfbdc9043d1cdadeca64ee34ef8a11566140aa1e0ac8cc0d3b5baae425`
for the G3 exact partial silhouette-edge inverse-composite cleanup. Each row is
source/receipt/predecessor-rejection bound and creates no cross-content alias.
