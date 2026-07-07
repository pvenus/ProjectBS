# Battle Story Context Guide

## Purpose

This document defines how to extract **battle-generation context** from story planning.

This is not the full story planning guide.

The full story planning guide owns narrative structure, characters, plot, dialogue, chapter flow, and emotional arcs.

`BattleStoryContext` only owns the information needed to generate:

- Encounter intent
- Monster pool direction
- Spawn variation selection
- Battle pacing
- Objective or prop pressure
- Forbidden or required battle constraints

## Naming

Use the term:

```text
BattleStoryContext
```

Do not call this data simply `StoryContext` when used in the battle pipeline.

`StoryContext` may refer to broader narrative planning.

`BattleStoryContext` means:

```text
Story-derived context prepared specifically for battle, monster, and spawner generation.
```

## Pipeline Role

`BattleStoryContext` is the first battle-specific input to battle generation.

The direct input should include Act and Chapter information.

```text
Act / Chapter story input
  + story reference guides
  + character planning context
  -> BattleStoryContext
  -> Monster Pool
  -> Monster Mapping
  -> Spawner Variation Selection
  -> Spawner Mapping
  -> Battle JSON / Spawn JSON
```

It should preserve enough story meaning for later stages without forcing final spawn decisions too early.

When the current task only asks for battle or monster planning, stop at monster pool selection and preserve spawn-related tags. A later task can use those tags to select a reusable `SpawnVariationProfile`.

Spawner variation authoring is defined here:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
```

## Input

Use one Chapter story document as the primary input, with Act information preserved.

Current chapter inputs live under:

```text
Assets/Doc/Story/ActXX/ChapterXX/Chapter_XX.md
```

Example:

```text
Assets/Doc/Story/Act01/Chapter01/Chapter_01.md
```

The agent should receive the Chapter story text or a path to the Chapter story file.

Do not require the user to manually provide all world, act, and character context every time.

Use the story structure guide and core story files to recover the surrounding context.

Recommended input package:

```json
{
  "actId": "act.01",
  "chapterId": "chapter.01.01",
  "chapterFile": "Assets/Doc/Story/Act01/Chapter01/Chapter_01.md",
  "playerPlanningRoot": "Assets/Doc/Character/player",
  "monsterContextRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_context.cheongun_sangui_act1.json",
  "monsterCompositionRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_composition.chapter_01_05.json"
}
```

If `monsterContextRef` or `monsterCompositionRef` exists, use it before inventing a new monster pool.

## Required Reference Guides

When generating `BattleStoryContext`, read:

```text
Assets/character_concepts/game_prompt_guide/story/StoryStructureGuide.md
```

Then use the story references described there:

```text
Assets/Doc/Story/00_Background.md
Assets/Doc/Story/Act01/01_Overall_Story.md
Assets/Doc/Story/Act01/Act_01_Background.md
Assets/Doc/Story/Characters.md
Assets/Doc/Story/ActXX/ChapterXX/Chapter_XX.md
```

If available, also read the character planning refs:

```text
Assets/Doc/Character/player/*.json
Assets/Doc/Character/{groupId}/monster_context.{groupId}.json
Assets/Doc/Character/{groupId}/monster_composition.chapter_XX_YY.json
Assets/Doc/Character/{groupId}/npc/*.json
```

If selecting or preparing spawner mapping, also read:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
```

For battle-specific extraction rules, continue using this file.

## Input Resolution Rules

If the user provides only a Chapter story path:

1. Read `StoryStructureGuide.md`.
2. Read the core story references in the order defined by that guide.
3. Read the provided Chapter story.
4. Infer Act and Chapter identity from the file name and story guide context.
5. Search `Assets/Doc/Character/player` and the relevant `Assets/Doc/Character/{groupId}` NPC planning when provided by the user or referenced by prior planning.
6. Generate `BattleStoryContext` from the Chapter story plus reference context.

If the user provides raw Chapter story text:

1. Use the raw text as the Chapter input.
2. Read `StoryStructureGuide.md`.
3. Read the core story references in the order defined by that guide.
4. Use provided Act ID, Chapter ID, player planning root, monster context, or monster composition refs if present.
5. Generate `BattleStoryContext`.

Do not invent world rules, character identities, or Act-level conflict when they can be read from the reference files.

Do not invent new monsters when `monsterCompositionRef` already contains a suitable monster set.

## Output

Recommended planning output path:

```text
Assets/Doc/Battle/{battle_group}/{battleId}.story_context.json
```

This file is an intermediate planning artifact.

It is not consumed directly by `BattleJsonGenerator` unless a future builder is added.

## Minimal JSON Shape

```json
{
  "battleStoryContextId": "battle_story_context.forest.wolf_pack.001",
  "sourceStoryId": "chapter.01.01",
  "sourceStoryFile": "Assets/Doc/Story/Act01/Chapter01/Chapter_01.md",
  "actId": "act.01",
  "chapterId": "chapter.01.01",
  "playerPlanningRoot": "Assets/Doc/Character/player",
  "monsterContextRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_context.cheongun_sangui_act1.json",
  "monsterCompositionRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_composition.chapter_01_05.json",
  "battleIdHint": "battle.forest.001",
  "summary": "The player is ambushed by a wolf pack while crossing a narrow forest path.",
  "locationTags": ["forest", "narrow_path"],
  "factionTags": ["wolf_pack"],
  "situationTags": ["ambush"],
  "intentTags": ["pressure", "survive_contact"],
  "spaceTags": ["front", "flank"],
  "rhythmTags": ["escalating"],
  "objectiveTags": [],
  "toneTags": ["tense", "wild"],
  "difficultyHint": "normal",
  "targetPartySize": 3,
  "battleModeHint": "elimination",
  "targetBattleLengthSec": 90,
  "spawnWindowSec": 60,
  "clearWindowSec": 30,
  "requiredMonsterTags": ["wolf"],
  "preferredMonsterTags": ["fast", "melee"],
  "forbiddenMonsterTags": ["boss"],
  "requiredSpawnTags": ["front_then_flank"],
  "forbiddenSpawnTags": ["heavy_surround", "backline_artillery"],
  "spawnSelectionHints": {
    "candidateSpawnerTypes": ["front_then_flank", "elimination_90s_swarm"],
    "preferredDifficulty": "normal",
    "targetPartySize": 3,
    "targetSpawnCountHint": 80,
    "spawnWindowSec": 60,
    "clearWindowSec": 30,
    "preferredVariationTags": ["front_then_flank", "delayed_ambush"],
    "avoidVariationTags": ["heavy_surround"],
    "requiredRoleSlots": ["front_basic", "flank_active"],
    "selectionReasonHint": "Use readable front contact with delayed flank pressure."
  },
  "notes": [
    "The encounter should feel like a natural animal ambush, not a military formation."
  ]
}
```

## Fields

| Field | Required | Purpose |
|---|---:|---|
| battleStoryContextId | Yes | Stable ID for this context artifact. |
| sourceStoryId | Yes | ID of the source Chapter story. |
| sourceStoryFile | Yes | Path to the source Chapter story file when available. |
| actId | Yes | Parent Act ID. |
| chapterId | Yes | Parent Chapter ID. |
| playerPlanningRoot | No | Player-side character planning root, when available. |
| monsterContextRef | No | Monster planning pool index, when available. |
| monsterCompositionRef | No | Chapter monster composition index, when available. |
| battleIdHint | No | Suggested battle ID. |
| summary | Yes | Short battle-relevant summary. |
| locationTags | Yes | Where the fight happens. |
| factionTags | Yes | Which enemy group or force appears. |
| situationTags | Yes | What is happening narratively. |
| intentTags | Yes | What the battle should make the player experience. |
| spaceTags | Yes | Spatial pressure implied by the story. |
| rhythmTags | Yes | Timing and pacing implied by the story. |
| objectiveTags | No | Prop/objective pressure, if present. |
| toneTags | No | Emotional flavor for selection and naming. |
| difficultyHint | Yes | `very_easy`, `easy`, `normal`, `hard`, `very_hard`, or `boss`. |
| targetPartySize | No | Expected party size for balancing. Use `1`, `2`, or `3` when known. |
| battleModeHint | No | Battle mode such as `elimination`, `survival`, `defense`, or `boss`. |
| targetBattleLengthSec | No | Desired total battle duration. Use `90` for standard elimination. |
| spawnWindowSec | No | Desired active spawn window. Use about `60` for 90-second elimination. |
| clearWindowSec | No | Desired cleanup window after final spawn. Use about `30` for 90-second elimination. |
| requiredMonsterTags | No | Monster tags that must appear. |
| preferredMonsterTags | No | Monster tags that fit the story. |
| forbiddenMonsterTags | No | Monster tags that should not appear. |
| requiredSpawnTags | No | Spawn variation tags that should appear. |
| forbiddenSpawnTags | No | Spawn variation tags that should not appear. |
| spawnSelectionHints | No | Optional hints for later `SpawnVariationProfile` selection. |
| notes | No | Short human-readable constraints. |

## Battle Mode And Pacing Hints

Story context should preserve enough pacing data for spawner selection.

Use these fields when the story or design intent implies them:

```json
{
  "battleModeHint": "elimination",
  "targetPartySize": 3,
  "targetBattleLengthSec": 90,
  "spawnWindowSec": 60,
  "clearWindowSec": 30,
  "difficultyHint": "normal"
}
```

Common battle modes:

```text
elimination
survival
defense
escort
boss
boss_adds
objective_pressure
```

For hack-and-slash elimination battles, default to:

- `targetBattleLengthSec`: `90`
- `spawnWindowSec`: `60`
- `clearWindowSec`: `30`
- `targetPartySize`: `3` for normal/hard/very hard/boss
- `targetPartySize`: `2` for easy
- `targetPartySize`: `1` for very easy

Do not force these values when the story clearly implies another mode, such as
survival, defense, or boss staging.

## Spawner Selection Surface

Spawner selection should be possible from `BattleStoryContext` without reading
the full story again.

The important selection surface is:

- `battleModeHint`
- `difficultyHint`
- `targetPartySize`
- `targetBattleLengthSec`
- `spawnWindowSec`
- `clearWindowSec`
- `locationTags`
- `situationTags`
- `intentTags`
- `spaceTags`
- `rhythmTags`
- `objectiveTags`
- `requiredSpawnTags`
- `forbiddenSpawnTags`
- `spawnSelectionHints`
- monster role availability from `monsterCompositionRef`

`spawnSelectionHints` may include:

```json
{
  "candidateSpawnerTypes": ["elimination_90s_swarm"],
  "preferredDifficulty": "normal",
  "targetSpawnCountHint": 80,
  "preferredVariationTags": ["surround", "escalating", "staggered"],
  "avoidVariationTags": ["heavy_surround"],
  "requiredRoleSlots": ["fodder_melee", "fast_melee", "pressure_ranged"],
  "selectionReasonHint": "The story needs a broad 90-second swarm rather than a short ambush."
}
```

These hints should not name a final `SpawnSequenceSO` unless the task explicitly
includes spawner selection. They are used later to choose `spawnerType`,
`difficulty`, and finally `spawnSequenceId`.

## Tag Groups

### Location Tags

Use tags that affect battle space.

Examples:

```text
forest
narrow_path
open_field
bridge
gate
altar
ruins
cave
village
boss_arena
```

Location tags help decide whether front, flank, surround, objective, or random pressure is natural.

### Situation Tags

Use tags that describe what is happening.

Examples:

```text
patrol_contact
ambush
defense
escort
chase
breakthrough
ritual_interrupt
boss_intro
reinforcement
survival
```

Situation tags are the strongest input for spawn variation selection.

### Intent Tags

Use tags that describe the intended player experience.

Examples:

```text
readable_intro
pressure
split_attention
protect_objective
burst_threat
attrition
phase_change
survive_contact
```

Intent tags should not name exact monsters.

They describe what the battle should do.

### Space Tags

Use tags that describe meaningful spawn pressure.

Examples:

```text
front
back
flank
flank_left
flank_right
surround
backline
center
objective
random
```

Use `surround` carefully. For early or normal encounters, prefer `front + flank` unless the story explicitly needs heavy surround pressure.

### Rhythm Tags

Use tags that describe timing.

Examples:

```text
single_wave
wave_clear
escalating
overlap_pressure
delayed_ambush
loop_pressure
boss_phase
reinforcement
```

These tags help choose `AfterSpawnCompleted`, `AfterSpawnedEnemiesDefeated`, and repeat mode.

### Monster Tags

Use tags that can later match monster pool entries.

Examples:

```text
melee
ranged
fast
slow
tank
swarm
support
summoner
elite
boss
flying
beast
spirit
human
undead
```

Monster tags should remain descriptive, not implementation-specific.

## Story-To-Context Rules

Convert story language into battle context using these rules:

| Story Signal | Suggested Tags |
|---|---|
| Enemies wait in hiding | `situationTags: ["ambush"]` |
| Enemies rush from the road ahead | `spaceTags: ["front"]` |
| Enemies come from both sides | `spaceTags: ["flank"]` |
| Player is surrounded | `spaceTags: ["surround"]` |
| The fight protects a gate/core/altar | `objectiveTags: ["defense"]`, `spaceTags: ["objective"]` |
| Reinforcements arrive | `rhythmTags: ["reinforcement"]` |
| Pressure grows over time | `rhythmTags: ["escalating"]` |
| Player must survive | `intentTags: ["survive_contact"]`, `rhythmTags: ["loop_pressure"]` |
| A leader appears | `requiredMonsterTags: ["elite"]` or `["boss"]` |
| Story is an early tutorial | `forbiddenSpawnTags: ["heavy_surround", "overlap_pressure"]` |
| Story calls for a short clear-all fight | `battleModeHint: "elimination"`, `targetBattleLengthSec: 90` |
| Story calls for many weak enemies | `intentTags: ["pressure", "attrition"]`, `preferredVariationTags: ["swarm"]` |
| Story calls for one-player testing | `targetPartySize: 1`, `difficultyHint: "very_easy"` |
| Story calls for two-player testing | `targetPartySize: 2`, `difficultyHint: "easy"` |
| Story calls for three-player baseline | `targetPartySize: 3`, `difficultyHint: "normal"` |

## Required Separation

Do not put these in `BattleStoryContext`:

- Exact `SpawnSequenceSO` step data
- Final spawner pattern config, squad content, or spawn slot binding data
- Concrete `selectedVariationId` unless the task explicitly performs spawner selection
- Exact reward values
- Story reward rules or reward tables
- Full monster stats
- Full skill JSON
- Full character planning JSON
- Dialogue or long story prose

Those belong to later stages or other guides.

## Character Planning Handoff

When story context needs monsters, create or select character planning files first.

Do not expand every monster and character field inside `BattleStoryContext`.

Use this handoff shape instead:

```text
Act / Chapter story
  -> Character planning common JSON
  -> Per-character planning JSON
  -> Thin character context or monster pool refs
  -> BattleStoryContext
  -> Monster Mapping
  -> Spawner Mapping
```

Recommended character planning path:

```text
Assets/Doc/Character/player/{characterId}.json
Assets/Doc/Character/{groupId}/npc/{characterId}.json
```

Use `Assets/Doc/Character/player/` for playable and party-side planning files.

Use `Assets/Doc/Character/{groupId}/npc/` for enemy combat pool planning files, including Boss entries.

`Player`, `Npc`, and `Boss` remain `characterType` values only.

All runtime IDs still use the `character` domain.

Recommended context-level references:

```json
{
  "playerPlanningRoot": "Assets/Doc/Character/player",
  "monsterContextRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_context.cheongun_sangui_act1.json",
  "monsterCompositionRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_composition.chapter_01_05.json",
  "monsterPoolRefs": [
    {
      "characterId": "character.black_cloth_raider.1",
      "planningRef": "Assets/Doc/Character/cheongun_sangui_act1/npc/character.black_cloth_raider.1.json",
      "roleSlots": ["front_basic", "flank_basic"],
      "storyUseTags": ["chapter_01", "human", "ambush"]
    }
  ]
}
```

`BattleStoryContext` should preserve only the information needed for battle selection:

- Character planning refs
- Monster pool refs
- Role slots
- Story use tags
- Required or forbidden monster tags
- Chapter-specific monster composition refs

Keep identity, appearance, stat intent, and skill intent inside the character planning files.

## Context Preservation

Carry these IDs forward through later generated artifacts:

```text
sourceStoryId
battleStoryContextId
battleIdHint
monsterPoolId
encounterProfileId
selectedVariationId
```

This lets an agent explain why a spawn variation or monster binding was chosen.

Before spawner selection happens, `BattleStoryContext` should preserve only the selection surface:

```text
requiredSpawnTags
forbiddenSpawnTags
spaceTags
rhythmTags
objectiveTags
spawnSelectionHints
```

After spawner selection happens, use a separate mapping artifact or task notes to carry:

```text
encounterProfileId
selectedVariationId
monsterGroupId
spawnUnitBindingHints
spawnSequenceId
```

Do not expand concrete spawn sequence steps inside `BattleStoryContext`.

## Validation

Before using `BattleStoryContext`, check:

- `summary` is battle-relevant and short.
- `locationTags` are spatially meaningful.
- `factionTags` can map to a monster pool.
- `situationTags` can map to one or more spawn variations.
- `spaceTags` do not contradict the story.
- `difficultyHint` is compatible with forbidden spawn tags.
- Required and forbidden tags do not conflict.
- `battleModeHint`, `targetPartySize`, and timing hints match the intended design.
- `spawnSelectionHints` are specific enough to select a spawner type and difficulty later.
- No final spawner JSON data is embedded here.

## Agent Checklist

- Read the story planning source.
- Extract only battle-relevant information.
- Generate `BattleStoryContext`.
- Use it to create or select a monster pool.
- Use it to create `EncounterProfile`.
- Use `EncounterProfile` to select spawn variation only when the task includes spawner selection.
- Preserve IDs and reasons in later artifacts.
