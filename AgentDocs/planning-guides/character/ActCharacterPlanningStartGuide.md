# Act Character Planning Start Guide


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## Purpose

This guide starts Act-level character and monster planning from story input.

Use this when a new Act story and Chapter stories are ready, and an agent must create the planning data needed for later battle generation.

This guide is a process guide.

Do not copy this guide into generated Act output folders.

Generated output folders under `AgentDocs/planning-data/character/act-plans/player` and `AgentDocs/planning-data/character/act-plans/{act_group_id}` should contain JSON data artifacts and Unity `.meta` files only.

`AgentDocs/planning-data/story/Characters.md` is the global story character reference.

Do not treat `Characters.md` as Chapter-local input.

Use it to resolve player and story-common character identities.

## Input

The user should provide only short Act and Chapter input.

Recommended input:

```json
{
  "actId": "act.01",
  "actStoryFile": "AgentDocs/planning-data/story/Act01/Act_01_Background.md",
  "chapterFiles": [
    "AgentDocs/planning-data/story/Act01/Chapter01/Chapter_01.md",
    "AgentDocs/planning-data/story/Act01/Chapter02/Chapter_02.md",
    "AgentDocs/planning-data/story/Act01/Chapter03/Chapter_03.md"
  ]
}
```

Optional input:

```json
{
  "actGroupId": "act2_group_id",
  "reuseMonsterContextRefs": [
    "AgentDocs/planning-data/character/act-plans/cheongun_sangui_act1/monster_context.cheongun_sangui_act1.json"
  ],
  "notes": ["Reuse existing monsters only when they fit the story and role."]
}
```

## Required Guides

Read these guides before generating output:

```text
AgentDocs/planning-guides/story/StoryStructureGuide.md
AgentDocs/planning-guides/character/CharacterCreateGuide.md
AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
AgentDocs/planning-guides/battle/BattleStoryContextGuide.md
```

Read these guides when stat or skill intent is needed:

```text
AgentDocs/planning-guides/character/CharacterStatGuide.md
AgentDocs/planning-guides/skill/design/SkillDegineGuide.md
AgentDocs/planning-guides/skill/design/SkillBalanceGuide.md
```

## Required Story References

Read the story files in the order defined by `StoryStructureGuide.md`.

Required baseline:

```text
AgentDocs/planning-data/story/00_Background.md
AgentDocs/planning-data/story/Characters.md
```

Then read the provided Act-level overall/background file and Chapter files.

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
AgentDocs/planning-data/character/act-plans/{act_group_id}
```

### 3. Create Player Common Data JSON

Create:

```text
AgentDocs/planning-data/character/act-plans/player/{act_group_id}.player_common.json
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

Use `AgentDocs/planning-data/story/Characters.md` as the global character source.

Do not put monster pool data in the player common JSON.

Do not put character-specific appearance, stats, skills, or combat behavior in common JSON.

### 4. Create NPC Common Data JSON

Create:

```text
AgentDocs/planning-data/character/act-plans/{act_group_id}/{act_group_id}.common.json
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
AgentDocs/planning-data/character/act-plans/player
AgentDocs/planning-data/character/act-plans/{act_group_id}/npc
```

Use `AgentDocs/planning-data/character/act-plans/player` for playable or party-side planning files shared across story.

Use `AgentDocs/planning-data/character/act-plans/{act_group_id}/npc` for Act-specific NPC and Boss combat pool files.

The folder name `npc` is only an authoring organization boundary.

Do not use `npc` as a runtime domain.

Runtime IDs must use:

```text
character.{name}.{grade}
skill.character.{name}.{grade}.{slot}.{skill_name}
```

### 6. Create Character And Monster Planning JSON

Every new per-character JSON must use the single canonical contract:

```text
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
schemaVersion=character_planning_v2
```

The canonical top-level contract preserves `commonDataRef`, `identity`,
`combat`, `planningScore`, `stats`, and `skills`, and adds provenance,
structured appearance, Generated Media readiness, and typed
`missingDesignInputs`. Do not substitute a local Act-specific schema.

Required identity rules:

- `identity.characterId` starts with `character.`
- `identity.characterType` is one of `Player`, `Npc`, or `Boss`
- `identity.runtimeDomain` is `character`
- `Player` planning files are placed under `AgentDocs/planning-data/character/act-plans/player`
- `Npc` and `Boss` planning files are placed under `AgentDocs/planning-data/character/act-plans/{act_group_id}/npc`

Create new characters only when Act or Chapter battle needs require a missing role.

Reuse existing character planning refs when the role, story use, and tone fit.

An existing file without `schemaVersion=character_planning_v2` is legacy
read-only input. Classify it according to CharacterPlanningDataGuide.md. Do not
silently migrate or overwrite it, and do not treat a short legacy appearance
object as sufficient for image generation.

Character visual decisions must be grounded in exact story/planning evidence.
Do not derive gender presentation or biological sex, body, face, hair, costume,
equipment, weapon detail, handedness, palette/material, identifying features,
pose policy, target display size, detail density, required elements, or
prohibited elements from a name, personality, combat role, skill, grade, or
tag. Missing evidence creates typed `missingDesignInputs` and blocks a
`character_main_image` handoff.

### 7. Create Monster Context JSON

Create:

```text
AgentDocs/planning-data/character/act-plans/{act_group_id}/monster_context.{act_group_id}.json
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

Monster refs should point to `AgentDocs/planning-data/character/act-plans/{act_group_id}/npc/*.json`.

Player refs, if needed for context, should point to `AgentDocs/planning-data/character/act-plans/player/*.json`.

Do not copy full identity, appearance, stat intent, or skill intent into this file.

### 8. Create Monster Composition JSON

Create:

```text
AgentDocs/planning-data/character/act-plans/{act_group_id}/monster_composition.chapter_XX_YY.json
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

### 9. Validate

Before finishing:

1. Validate all generated JSON syntax.
2. Validate every `commonDataRef` exists.
3. Validate every `planningRef` exists.
4. Validate Player files are under `AgentDocs/planning-data/character/act-plans/player`.
5. Validate Npc and Boss files are under `AgentDocs/planning-data/character/act-plans/{act_group_id}/npc`.
6. Validate runtime domain remains `character`.
7. Validate no guide docs or README files were created inside generated output folders.
8. Validate every new per-character file is `character_planning_v2` and has no
   unknown fields.
9. Validate structured appearance and every observable required/prohibited
   statement have exact provenance.
10. Validate `missingDesignInputs`, `planningStatus`, and Generated Media
    readiness agree.
11. Validate legacy files were not overwritten without explicit reviewed
    migration approval.
12. Validate planning JSON contains no self-referential file or snapshot hash.
13. Create a separate `generated_media_planning_handoff_v1` only for an
    approved/ready character when explicitly requested; otherwise report the
    blocker without generating provider prompt or media.

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
- Canonical schema version and legacy classifications
- Character main-image readiness and handoff paths or blockers

Do not paste full JSON in the final response unless explicitly requested.
