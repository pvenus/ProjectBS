# Game Prompt Guide


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

This folder contains reference documents for agents.

Use these files as guides, schemas, SO explanations, policy notes, and workflow
documentation while executing a task.

Prompt authoring and review guides:

```text
AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
AgentDocs/planning-guides/prompt/PromptEvaluationGuide.md
AgentDocs/planning-guides/prompt/GuideAuthoringGuide.md
AgentDocs/planning-guides/prompt/GuideEvaluationGuide.md
AgentDocs/planning-guides/prompt/EvaluationSlackCanvasFormGuide.md
```

Copy-ready user prompts belong in:

```text
AgentDocs/task-prompts
```

Do not put copy-ready prompts in this folder.

Content storage and folder structure:

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/planning-guides/content/GeneratedImageProjectPromotionGuide.md
```

Current generated-media request routing and provider pipelines:

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenBackgroundPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenAnimationPipelineGuide.md
```

Current `background_single_image_v2` continues through these shared downstream
entries without becoming a legacy background identity:

```text
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/planning-guides/content/GeneratedImageProjectPromotionGuide.md
```

The current v2 provider structure is ImageGen-only with exactly four roles:
character single image, icon single image, background single image, and one
character/skill animationRequestId. PixelLab
guides and v1 rows are deprecated read-only legacy audit contracts.

Current authoring uses planning handoff v2, registry/router v2,
generated_media_prompt_v3, generated_media_generation_v2, and v2 storage.
Legacy v1/v2 prompt and v1 generation records remain immutable.

Legacy generated-media migration:

| Existing guide | Status | replacedBy / new role |
| --- | --- | --- |
| AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md | compatibility envelope for generated_image_prompt_v1 | generated-media planning handoff plus provider child prompt-authoring contracts |
| AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md | legacy generation-only compatibility boundary | GeneratedMediaImageGenOnlyContractGuide.md and the four current ImageGen role guides |
| AgentDocs/planning-guides/character/CharacterGenerateImage.md | deprecated legacy evidence | ImageGenCharacterImagePipelineGuide.md for new single-view requests |
| AgentDocs/planning-guides/character/CharacterGenerateAnimation.md | deprecated legacy evidence | ImageGenAnimationPipelineGuide.md for one animationRequestId |
| AgentDocs/planning-guides/character/CharacterAnimationDownloadGuide.md | deprecated historical evidence | GeneratedMediaLegacyV1CompatibilityGuide.md read-only audit |
| AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md | deprecated legacy profile evidence | ImageGenIconPipelineGuide.md with skill profile v2 |
| AgentDocs/planning-guides/item/ItemIconGenerationGuide.md | deprecated legacy profile evidence | ImageGenIconPipelineGuide.md with item profile v2 |
| AgentDocs/planning-guides/skill/SkillImageGenerationGuide.md | deprecated legacy profile evidence | ImageGenAnimationPipelineGuide.md with one animationRequestId |
| AgentDocs/planning-guides/skill/SkillImageDownloadGuide.md | deprecated historical evidence | GeneratedMediaLegacyV1CompatibilityGuide.md read-only audit |
| AgentDocs/planning-guides/stage/PopupEventMainImageCreateGuide.md image execution | deprecated historical evidence | ImageGenBackgroundPipelineGuide.md after a new v2 stage background handoff |
| AgentDocs/planning-guides/battle/BattleCreateGuide.md background-image execution | deprecated historical evidence | ImageGenBackgroundPipelineGuide.md after a new v2 battle background handoff |

Do not delete legacy guides until their profile and evaluation contracts have
dedicated replacements and all callers have migrated.

Character planning authority:

```text
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
AgentDocs/planning-guides/character/ActCharacterPlanningStartGuide.md
AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
AgentDocs/planning-guides/character/CharacterCreateGuide.md
```

`CharacterPlanningDataGuide.md` is the single per-character schema authority
for new Player/Npc/Boss planning. Legacy character planning is read-only until
an explicit reviewed migration.

Strategic item authoring guides:

```text
AgentDocs/planning-guides/item/strategic/data-structures/StrategicSkillItemSO.md
AgentDocs/planning-guides/item/strategic/StrategicItemRulesGuide.md
AgentDocs/planning-guides/item/strategic/data-structures/StrategicItemJsonGuide.md
```

Relic item authoring guides:

```text
AgentDocs/planning-guides/item/ItemIconGenerationGuide.md
AgentDocs/planning-guides/item/relic/data-structures/RelicItemSO.md
AgentDocs/planning-guides/item/relic/RelicItemRulesGuide.md
AgentDocs/planning-guides/item/relic/data-structures/RelicItemJsonGuide.md
AgentDocs/planning-guides/item/relic/RelicItemPlanningGuide.md
```

Standalone strategic skill authoring guides:

```text
AgentDocs/planning-guides/skill/strategic/StrategicSkillRulesGuide.md
AgentDocs/planning-guides/skill/data-structures/StrategicSkillJsonGuide.md
AgentDocs/planning-guides/skill/strategic/StrategicSkillPlanningGuide.md
```

Evaluation and Slack Canvas recording guides:

```text
AgentDocs/planning-guides/stage/PopupEventMainImageEvaluationGuide.md
AgentDocs/planning-guides/stage/PopupEventMainImageEvaluationSlackCanvasGuide.md
AgentDocs/planning-guides/skill/SkillIconEvaluationSlackCanvasGuide.md
AgentDocs/planning-guides/item/ItemIconEvaluationSlackCanvasGuide.md
```
