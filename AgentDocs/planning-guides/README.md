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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```

The current provider structure is `PixelLab -> Character/Icon/Animation` and
`ImageGen`. New integrations use `assetType + domainType`; the earlier
`artifactType` remains a compatibility alias only.

Legacy generated-media migration:

| Existing guide | Status | replacedBy / new role |
| --- | --- | --- |
| AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md | compatibility envelope for generated_image_prompt_v1 | generated-media planning handoff plus provider child prompt-authoring contracts |
| AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md | authoritative generation-only boundary and compatibility router | PixelLabPipelineGuide.md or ImageGenPipelineGuide.md for provider execution, then GeneratedMediaPreservationPackagingGuide.md |
| AgentDocs/planning-guides/character/CharacterGenerateImage.md | deprecated execution contract; retained as provider/profile evidence | AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md |
| AgentDocs/planning-guides/character/CharacterGenerateAnimation.md | deprecated execution contract; retained as provider/profile evidence | AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md |
| AgentDocs/planning-guides/character/CharacterAnimationDownloadGuide.md | deprecated execution contract; retained as export evidence | AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md + PixelLabCharacterPipelineGuide.md adapter |
| AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md | deprecated execution contract; retained as skill icon profile evidence | AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md |
| AgentDocs/planning-guides/item/ItemIconGenerationGuide.md | deprecated execution contract; retained as item icon profile/evaluation evidence | AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md |
| AgentDocs/planning-guides/skill/SkillImageGenerationGuide.md | deprecated execution contract; retained as animation profile evidence | AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md |
| AgentDocs/planning-guides/skill/SkillImageDownloadGuide.md | deprecated execution contract; retained as sheet extraction evidence | AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md + PixelLabAnimationPipelineGuide.md adapter |
| AgentDocs/planning-guides/stage/PopupEventMainImageCreateGuide.md image execution | deprecated execution contract; retained as stage profile evidence | AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md |
| AgentDocs/planning-guides/battle/BattleCreateGuide.md background-image execution | deprecated execution contract; retained as battle profile evidence | AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md |

Do not delete legacy guides until their profile and evaluation contracts have
dedicated replacements and all callers have migrated.

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
