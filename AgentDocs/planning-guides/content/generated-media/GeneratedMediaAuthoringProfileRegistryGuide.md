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
| `character_animation_v2` | `animation` | `character` | `character_animation@2.0.0` | `imagegen_animation` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |
| `skill_animation_v2` | `animation` | `skill` | `skill_animation@2.0.0` | `imagegen_animation` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md` | `AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |

Exactly one row must match. Zero returns `unsupported_current_route`; multiple
returns `conflicting_routing_evidence`. Every selected prompt must exist and
directly reference ContentFolderStructureGuide.md.

## Extension and Validation

Adding a domain requires approved planning schema/profile, new non-overlapping
registry version, preservation/evaluation adapters, guide/prompt evaluation,
then router activation. Add a row/profile, never a copied domain prompt.

Validate unique keys, ImageGen-only provider, existing prompt paths, exact
structure profile, type-specific anchor discriminator, and failure/readiness
parity. Current registry changes never reinterpret an immutable record.

## Related Guides

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
