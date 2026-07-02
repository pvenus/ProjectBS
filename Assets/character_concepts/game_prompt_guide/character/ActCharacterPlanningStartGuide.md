# Act Character Planning Start Guide

## Purpose

This guide starts Act-level character and monster planning from story input.

Use this when a new Act story and Chapter stories are ready, and an agent must create the planning data needed for later battle generation.

This guide is a process guide.

Do not copy this guide into generated Act output folders.

Generated output folders under `Assets/Doc/Character/player` and `Assets/Doc/Character/{act_group_id}` should contain JSON data artifacts and Unity `.meta` files only.

`Assets/Doc/Story/Characters.md` is the global story character reference.

Do not treat `Characters.md` as Chapter-local input.

Use it to resolve player and story-common character identities.

## Input

The user should provide only short Act and Chapter input.

Recommended input:

```json
{
  "actId": "act.02",
  "actStoryFile": "Assets/Doc/Story/Act_02_Background.md",
  "chapterFiles": [
    "Assets/Doc/Story/Chapter_06.md",
    "Assets/Doc/Story/Chapter_07.md",
    "Assets/Doc/Story/Chapter_08.md"
  ]
}
```

Optional input:

```json
{
  "actGroupId": "act2_group_id",
  "reuseMonsterContextRefs": [
    "Assets/Doc/Character/cheongun_sangui_act1/monster_context.cheongun_sangui_act1.json"
  ],
  "notes": ["Reuse existing monsters only when they fit the story and role."]
}
```

## Required Guides

Read these guides before generating output:

```text
Assets/character_concepts/game_prompt_guide/story/StoryStructureGuide.md
Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
```

Read these guides when stat or skill intent is needed:

```text
Assets/character_concepts/game_prompt_guide/character/CharacterStatGuide.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillBalanceGuide.md
```

## Required Story References

Read the story files in the order defined by `StoryStructureGuide.md`.

Required baseline:

```text
Assets/Doc/Story/00_Background.md
Assets/Doc/Story/01_Overall_Story.md
Assets/Doc/Story/Characters.md
```

Then read the provided Act story file and Chapter files.

Do not read `.meta` files as story content.

## Workflow

### 1. Resolve Act And Chapter Context

Use `actId`, `actStoryFile`, and `chapterFiles` as the generation boundary.

Act context should decide:

- Shared race candidates
- Shared faction candidates
- World use
- Story use
- Reuse policy
- Source guide references

Chapter context should decide:

- Which battle roles are required
- Which monsters or NPCs appear
- Which enemies are delayed or forbidden
- Which player-side characters are present
- Which boss, elite, support, ranged, swarm, or objective roles are needed
- Which spawn or spatial pressure tags should be preserved

### 2. Choose Act Group ID

If `actGroupId` is provided, use it.

Otherwise derive a stable lowercase group ID from the Act story.

Examples:

```text
cheongun_sangui_act1
capital_shadow_act2
river_fortress_act3
```

Use the group ID as the folder name for Act-specific NPC and Boss planning:

```text
Assets/Doc/Character/{act_group_id}
```

### 3. Create Player Common Data JSON

Create:

```text
Assets/Doc/Character/player/{act_group_id}.player_common.json
```

The player common JSON should contain player-side shared data only:

```text
race
faction
worldUse
storyUse
reuse
sourceGuides
```

Use `Assets/Doc/Story/Characters.md` as the global character source.

Do not put monster pool data in the player common JSON.

Do not put character-specific appearance, stats, skills, or combat behavior in common JSON.

### 4. Create NPC Common Data JSON

Create:

```text
Assets/Doc/Character/{act_group_id}/{act_group_id}.common.json
```

The NPC common JSON should contain Act-level NPC and Boss pool shared data:

```text
race
faction
worldUse
storyUse
reuse
sourceGuides
```

Do not put player-only data in the NPC common JSON.

### 5. Create Player And Monster Folders

Create:

```text
Assets/Doc/Character/player
Assets/Doc/Character/{act_group_id}/npc
```

Use `Assets/Doc/Character/player` for playable or party-side planning files shared across story.

Use `Assets/Doc/Character/{act_group_id}/npc` for Act-specific NPC and Boss combat pool files.

The folder name `npc` is only an authoring organization boundary.

Do not use `npc` as a runtime domain.

Runtime IDs must use:

```text
character.{name}.{grade}
skill.character.{name}.{grade}.{slot}.{skill_name}
```

### 6. Create Character And Monster Planning JSON

Each character planning JSON should contain only:

```text
commonDataRef
identity
appearance
combat
planningScore
stats
skills
```

Required identity rules:

- `identity.characterId` starts with `character.`
- `identity.characterType` is one of `Player`, `Npc`, or `Boss`
- `identity.runtimeDomain` is `character`
- `Player` planning files are placed under `Assets/Doc/Character/player`
- `Npc` and `Boss` planning files are placed under `Assets/Doc/Character/{act_group_id}/npc`

Create new characters only when Act or Chapter battle needs require a missing role.

Reuse existing character planning refs when the role, story use, and tone fit.

### 7. Create Monster Context JSON

Create:

```text
Assets/Doc/Character/{act_group_id}/monster_context.{act_group_id}.json
```

This file should expose the available enemy monster pool to later agents.

Allowed fields:

```text
contextId
commonDataRef
monsterCompositionRef
sourceStoryRefs
monsterPoolRefs
bossRefs
playerPlanningRefs
```

Monster refs should point to `Assets/Doc/Character/{act_group_id}/npc/*.json`.

Player refs, if needed for context, should point to `Assets/Doc/Character/player/*.json`.

Do not copy full identity, appearance, stat intent, or skill intent into this file.

### 8. Create Monster Composition JSON

Create:

```text
Assets/Doc/Character/{act_group_id}/monster_composition.chapter_XX_YY.json
```

Use this file to preserve Chapter battle needs.

Recommended fields:

```text
compositionId
actId
commonDataRef
monsterContextRef
sourceStoryRefs
chapterCompositions
globalRules
```

Each `chapterCompositions` entry should include:

```text
chapterId
chapterTitle
coreBattleIntent
battleScale
locationTags
situationTags
recommendedSpawnTags
forbiddenSpawnTags
primaryMonsters
secondaryMonsters
lockedOutMonsters
notes
```

Use `primaryMonsters` and `secondaryMonsters` as refs to character planning files.

Do not embed full character planning data.

### 8. Validate

Before finishing:

1. Validate all generated JSON syntax.
2. Validate every `commonDataRef` exists.
3. Validate every `planningRef` exists.
4. Validate Player files are under `Assets/Doc/Character/player`.
5. Validate Npc and Boss files are under `Assets/Doc/Character/{act_group_id}/npc`.
6. Validate runtime domain remains `character`.
7. Validate no guide docs or README files were created inside generated output folders.

## Final Response

Keep the final response short.

Report:

- Output folder
- Common JSON
- Monster context JSON
- Monster composition JSON
- Player count
- NPC and Boss count
- Validation result

Do not paste full JSON in the final response unless explicitly requested.
