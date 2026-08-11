# Game Prompts


This folder contains copy-ready prompts.

Use these files as the text a user gives to an agent for a concrete task.
Prompts may reference guide documents, but they should stay short and task
oriented.

Guides, schemas, SO explanations, policies, and long-form reference material
belong in:

```text
AgentDocs/planning-guides
```

When creating or reviewing prompts, use:

```text
AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
AgentDocs/planning-guides/prompt/PromptEvaluationGuide.md
```

Prompt evaluation copy-ready prompts are under:

```text
AgentDocs/task-prompts/prompt
```

Prompt and guide evaluation reports:

```text
AgentDocs/task-prompts/prompt/PromptEvaluationReportPrompt.md
AgentDocs/task-prompts/prompt/GuideEvaluationReportPrompt.md
```

Content folder creation:

```text
AgentDocs/task-prompts/content/ContentFolderCreatePrompt.md
AgentDocs/task-prompts/content/GeneratedImagePromptAuthoringPrompt.md
AgentDocs/task-prompts/content/GeneratedImageGenerationPrompt.md
AgentDocs/task-prompts/content/GeneratedImageEvaluationPrompt.md
AgentDocs/task-prompts/content/GeneratedImageProjectPromotionPrompt.md
```

Current generated-media request routing, prompt authoring, and execution:

```text
AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabIconGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabAnimationPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabAnimationGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenGenerationPrompt.md
AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md
```

Legacy execution prompt migration:

| Existing prompt | Status | replacedBy |
| --- | --- | --- |
| AgentDocs/task-prompts/content/GeneratedImagePromptAuthoringPrompt.md | compatibility entry for legacy artifactType | provider-specific generated-media PromptAuthoringPrompt entries |
| AgentDocs/task-prompts/content/GeneratedImageGenerationPrompt.md | compatibility entry for legacy artifactType | provider-specific GenerationPrompt, then GeneratedMediaPreservationPackagingPrompt.md |
| AgentDocs/task-prompts/character/CharacterGenerateImagePrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md + AgentDocs/task-prompts/content/generated-media/PixelLabCharacterGenerationPrompt.md |
| AgentDocs/task-prompts/character/CharacterGenerateAnimationPrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md + AgentDocs/task-prompts/content/generated-media/PixelLabCharacterGenerationPrompt.md |
| AgentDocs/task-prompts/character/CharacterAnimationDownloadPrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md |
| AgentDocs/task-prompts/skill/SkillIconGenerationPrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md + AgentDocs/task-prompts/content/generated-media/PixelLabIconGenerationPrompt.md |
| AgentDocs/task-prompts/item/ItemIconGenerationPrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md + AgentDocs/task-prompts/content/generated-media/PixelLabIconGenerationPrompt.md |
| AgentDocs/task-prompts/skill/SkillImageGenerationPrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/PixelLabAnimationPromptAuthoringPrompt.md + AgentDocs/task-prompts/content/generated-media/PixelLabAnimationGenerationPrompt.md |
| AgentDocs/task-prompts/skill/SkillImageDownloadPrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md |
| AgentDocs/task-prompts/stage/PopupEventMainImageCreatePrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md + AgentDocs/task-prompts/content/generated-media/ImageGenGenerationPrompt.md |
| AgentDocs/task-prompts/battle/BattleBackgroundImagePrompt.md | deprecated compatibility entry | AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md + AgentDocs/task-prompts/content/generated-media/ImageGenGenerationPrompt.md |

Do not copy a legacy skill/item/stage/battle execution prompt for a new domain.
Use `domainType` and an approved profile with the current provider prompt.

Character planning:

```text
AgentDocs/task-prompts/character/ActCharacterPlanningPrompts.md
```

This prompt writes `character_planning_v2` for new Player/Npc/Boss planning and
may create a separate character-main-image planning handoff only when design
readiness is complete.

Evaluation and Slack Canvas recording:

```text
AgentDocs/task-prompts/prompt/EvaluationSlackCanvasFormPrompt.md
AgentDocs/task-prompts/stage/PopupEventMainImageEvaluationPrompt.md
AgentDocs/task-prompts/stage/PopupEventMainImageEvaluationSlackCanvasPrompt.md
AgentDocs/task-prompts/skill/SkillIconEvaluationSlackCanvasPrompt.md
AgentDocs/task-prompts/item/ItemIconEvaluationSlackCanvasPrompt.md
```

Stage node and popup event JSON prompts are under:

```text
AgentDocs/task-prompts/stage
```

Strategic item JSON generation:

```text
AgentDocs/task-prompts/item/StrategicItemSOJsonGeneratePrompt.md
```

Standalone strategic skill JSON generation:

```text
AgentDocs/task-prompts/item/StrategicSkillSOJsonGeneratePrompt.md
AgentDocs/task-prompts/item/StrategicSkillReversePlanningPrompt.md
```

Relic item JSON generation:

```text
AgentDocs/task-prompts/item/RelicItemSOJsonGeneratePrompt.md
AgentDocs/task-prompts/item/RelicItemPlanningCreatePrompt.md
AgentDocs/task-prompts/item/RelicItemReversePlanningPrompt.md
AgentDocs/task-prompts/item/ItemIconGenerationPrompt.md
```

Do not put reference guides in this folder.
