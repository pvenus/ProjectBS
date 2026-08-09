# Generated Media Authoring Profile Registry Guide

## 1. Purpose and Authority

Guide Type: schema/registry. This document is the sole closed registry for
Generated Media prompt-authoring route pairs.

```text
registryVersion: generated_media_authoring_profile_registry_v1
```

`GeneratedMediaRequestRoutingGuide.md` owns routing procedure. This registry
owns the exact supported `assetType + domainType + profileId + profileVersion`
pairs, their selected pipeline/prompt, and technical prompt profile. Provider
authoring consumes and validates the immutable router-selected row through
`routingRecordFile`; it never independently reselects a row.
pipeline guides own the behavior behind registered rows. External planning owns
all visual meaning and specifications.
`GeneratedMediaVisualPromptAuthoringGuide.md` owns common visual normalization;
this registry owns the exact profile authority that may add artifact/domain
rendering constraints to each row.

The current router is pinned to
`generated_media_authoring_profile_registry_v1`. A caller cannot select,
override, or request another registry version. A future registry becomes usable
only with a reviewed router guide/prompt revision or an explicit registry
migration contract that changes the pinned version.

## 2. Canonical Profile Key

```yaml
profile:
  profileId: lowercase_snake_case
  profileVersion: MAJOR.MINOR.PATCH

profileKey = {profileId}@{profileVersion}
```

Do not trim, alias, case-fold, or semantically translate an unknown profile
after compatibility normalization. Only exact keys in Section 3 are supported.
Character profiles are fixed technical results of their exact asset rows; other
asset types must supply the exact profile object in the planning handoff.

## 3. Closed Registry

| registryRowId | assetType | domainType | profileId | profileVersion | selectedPipeline | selectedAuthoringPrompt | providerPromptProfile |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `character_main_v1` | `character_main_image` | `character` | `character_main_image` | `1.0.0` | `pixellab_character` | `AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md` | `pixellab_character_prompt_v1` |
| `character_animation_v1` | `character_animation` | `character` | `character_animation` | `1.0.0` | `pixellab_character` | `AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md` | `pixellab_character_animation_prompt_v1` |
| `skill_icon_v1` | `icon` | `skill` | `skill_icon` | `1.0.0` | `pixellab_icon` | `AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md` | `pixellab_icon_prompt_v1` |
| `relic_icon_v1` | `icon` | `item` | `relic` | `1.0.0` | `pixellab_icon` | `AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md` | `pixellab_icon_prompt_v1` |
| `skill_animation_v1` | `general_animation` | `skill` | `skill_animation` | `1.0.0` | `pixellab_animation` | `AgentDocs/task-prompts/content/generated-media/PixelLabAnimationPromptAuthoringPrompt.md` | `pixellab_animation_prompt_v1` |
| `stage_popup_v1` | `imagegen_image` | `stage` | `story_popup_main_image` | `1.0.0` | `imagegen` | `AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md` | `imagegen_composed_scene_prompt_v1` |
| `battle_background_v1` | `imagegen_image` | `battle` | `battle_background` | `1.0.0` | `imagegen` | `AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md` | `imagegen_composed_scene_prompt_v1` |

No `environment` or `other_registered_domain` pair is supported in this
registry version. They return `invalid_domain_profile` until an explicit row
and required evaluation adapter are approved.

The item row supports the currently documented `relic` visual profile only.
The skill-icon row's `skill_icon` routing profile does not replace the external
composition profile; composition remains an immutable design fact inside
`iconSpecification`.

### 3.1 Registered Visual Profile Authorities

The mapping below documents the exact already-active visual evidence for each
registry row. It does not make legacy execution steps authoritative.

| registryRowId | artifact contract | registered visual profile authority |
| --- | --- | --- |
| `character_main_v1` | `character_main_image` | `AgentDocs/planning-guides/character/CharacterGenerateImage.md` provider/profile sections |
| `character_animation_v1` | `character_animation` | `AgentDocs/planning-guides/character/CharacterGenerateAnimation.md` provider/profile sections |
| `skill_icon_v1` | `icon` | `AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md` visual profile sections |
| `relic_icon_v1` | `icon` | `AgentDocs/planning-guides/item/ItemIconGenerationGuide.md` visual profile sections |
| `skill_animation_v1` | `general_animation` | `AgentDocs/planning-guides/skill/SkillImageGenerationGuide.md` visual profile sections |
| `stage_popup_v1` | `imagegen_image` | `AgentDocs/planning-guides/stage/StoryImageVisualGuide.md`, `AgentDocs/planning-guides/stage/StoryImageElementGuide.md`, and the visual sections of `AgentDocs/planning-guides/stage/PopupEventMainImageCreateGuide.md` |
| `battle_background_v1` | `imagegen_image` | background visual sections of `AgentDocs/planning-guides/battle/BattleCreateGuide.md` |

Apply these only after Master Concept, approved planning, and the common visual
authoring guide. A legacy profile statement that invents missing request meaning
or conflicts with an upper authority is inactive and returns
`material_visual_contract_conflict`; it is not an exception.

## 4. Exact Match Contract

Match all four key fields. Character rows assign their fixed profile before
matching only after exact asset/domain validation. Every other row requires an
exact external profile ID/version.

```text
1 exact row -> eligible for routed
0 rows -> invalid_domain_profile after enum validation
2+ rows -> conflicting_routing_evidence and registry defect
```

Completeness of required/prohibited elements and type specification is a
separate validation gate and cannot make an unsupported pair match.

## 5. Ownership, Versioning, and Extension

Registry owner: Generated Media pipeline documentation owner.

Adding a pair requires one reviewed change set that:

1. adds one non-overlapping exact row;
2. updates the owning provider pipeline guide/profile behavior;
3. defines the matching planning specification and provider settings contract;
4. defines an exact visual profile authority compatible with
   GeneratedMediaVisualPromptAuthoringGuide.md;
5. provides a preservation adapter and evaluation structure/profile adapter;
6. updates compatibility mapping only when a legacy caller exists;
7. updates this registry version and README index/reference impact;
8. passes guide and prompt evaluation before Git handoff.

Add a row/profile, not a copied domain task prompt. A changed profile contract
requires a new `profileVersion`. Any row addition/removal or route target change
requires a new `registryVersion`; prior routing records retain their stored
version and are never silently reinterpreted.

Migration rules:

- unchanged exact pair and semantics may remain under the same profile version;
- changed required fields, provider profile, or pipeline behavior increments
  profileVersion;
- changed row set increments registryVersion;
- requests using an older registry remain reproducible from their immutable
  records, but the current router always uses its pinned v1 registry;
- no automatic latest-version upgrade is allowed;
- caller-supplied registry version selection is rejected;
- unavailable prior registry evidence returns `invalid_domain_profile` or
  `conflicting_routing_evidence`, not a guessed replacement.

## 6. Validation and Failure

```text
missing_profile_id
missing_profile_version
invalid_profile_id_format
invalid_profile_version_format
invalid_domain_profile
duplicate_registry_key
overlapping_registry_row
registry_target_missing
registry_version_unsupported
registry_migration_ambiguous
```

- every key tuple is unique;
- every selected prompt and pipeline guide exists;
- every row has one preservation/evaluation adapter path through its pipeline;
- every row has one exact artifact contract and registered visual profile
  authority;
- providerPromptProfile agrees with the selected pipeline;
- unknown domains and profiles do not match;
- registry changes never modify existing immutable routing records.

## 7. Related Documents

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```
