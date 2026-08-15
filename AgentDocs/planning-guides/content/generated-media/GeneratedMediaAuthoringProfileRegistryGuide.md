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

Character single-image selection is closed: no approved selection resolves the
legacy-compatible key, one exact approved selection of a registered nondefault
key resolves that exact profile, and any unknown, multiple, or conflicting selection blocks as
`character_style_profile_conflict`. Character animation does not independently
select a profile; it inherits the exact key, payload, and hash from its
hash-verified immutable reference prompt record. This preserves every old
prompt record byte-for-byte and prevents a registry revision from reinterpreting
its identity.

Inheritance is also scope-checked. A character-animation request may inherit
only a profile whose `appliesTo` includes `character_animation_v2`. The
bold-outline compressed-detail profile is intentionally single-image-only; an
animation reference prompt carrying it blocks as `character_style_profile_conflict`
until a separately reviewed animation-capable version exists.

The canonical authority above is the sole owner of the closed payload, lock
order, canonicalization, and hash algorithm. This registry is only a closed
key/hash projection and must not duplicate or redefine lock statements. Approved
planning continues to own gender/age presentation, face, hair, costume,
equipment, weapon, palette, materials, pose and motion. Character prompt
records must persist the exact expression profile key, payload, and payload hash.
Character animation inherits all three byte-for-byte from its immutable approved
reference prompt record. A different requested expression requires a reviewed registry/profile
version and explicit planning approval; no caller alias or silent override is
allowed.

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
