# Battle Create Guide

## Purpose

This document defines the battle JSON creation process for agents.

Use this guide when converting encounter intent into runtime JSON and generated SO assets.

Pipeline:

```text
Story planning source
  -> BattleStoryContext
  -> Encounter planning
  -> Monster pool selection
  -> Optional SpawnVariationProfile selection
  -> Optional Spawner JSON generation
  -> Battle JSON generation
  -> Unity SO generation
  -> Scene/prefab verification
```

## Global Rules

- Generate JSON first.
- Do not create Unity SO assets directly unless the task explicitly asks for it.
- Use `BattleSpawnManager` and `SpawnSequenceSO` for enemy spawning.
- Do not use the removed legacy wave spawner fields.
- Keep battle JSON separate from spawner JSON.
- Make IDs stable and readable.
- Use exact enum names from the guide documents.
- Select reusable spawn variations from story and monster composition only when the task includes spawner selection.
- Use `BattleStoryContext` as the battle-specific story extraction layer, not the full story planning source.
- Current battle planning may stop at monster pool selection. In that case, preserve spawn tags and role-slot hints for a later spawner mapping task.

## Step 1. Encounter Planning

### Purpose

Define what the battle is trying to do before writing implementation JSON.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/story/StoryStructureGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
```

### Main Work

1. Read the input Chapter story.
2. Read story references through `StoryStructureGuide.md`.
3. Generate `BattleStoryContext`.
4. Determine the battle theme, difficulty, and expected duration.
5. Select victory rule.
6. Define available monster pool.
7. Preserve required/forbidden spawn tags for later variation selection.
8. Select candidate spawn variations only if the task includes spawner mapping.
9. Bind monsters into selected variation role slots only if a variation was selected.
10. Decide whether battle props are needed.
11. Decide rewards and relic drop chances.

### Required Pre-Generation Inputs

Use these inputs before creating final spawner JSON:

| Input | Purpose |
|---|---|
| Chapter story | Primary story input for battle context extraction. |
| BattleStoryContext | Story-derived battle context used for generation. |
| Location / space context | Chooses front, back, flank, surround, objective, or random pressure. |
| Monster pool | Defines which character IDs can fill variation roles. |
| Skill slot profile | Determines whether a monster fits front, ranged, flank, support, elite, or boss roles. |
| Difficulty budget | Limits count, elite use, overlap, flank/back/surround pressure. |
| Encounter rhythm | Chooses one-shot, escalating, ambush, loop, objective, boss phase. |
| Forbidden rules | Prevents unfair or story-breaking combinations. |
| SpawnVariationProfile catalog | Optional. Used only when selecting an independent spawner variation. |

### Spawn Intent Notes

Before writing spawner JSON, classify each monster group by its skill slot profile first.

| Element | Examples | Why It Matters |
|---|---|---|
| Skill slot profile | `basic_attack`, `active_1`, `active_2`, `passive_1` | Primary source for visible combat behavior. |
| Basic attack form | Melee, Ranged, Area, Summon-like | Drives default distance and pattern. |
| Active hook | Charge, Jump, Projectile, Area, Summon, CC | Drives side, delay, and sequence timing. |
| Passive hook | Tank, Aura, Support, Enrage, Objective | Drives grouping and ally spacing. |
| Attack range | Melee, MidRange, LongRange | Drives front/back distance. |
| Pressure direction | Front, Back, Flank, Surround, Random | Drives pattern choice. |
| Target preference | Nearest, Backline, Structure, Random | Drives placement relative to party or props. |
| Fairness requirement | Safe, Pressure, Ambush, Spike | Drives delay, distance, and completion mode. |

Derived tactical labels such as `Blocker`, `Diver`, `Harasser`, `Artillery`, `Swarm`, or `Support` may be used after the slot profile is understood.

The result should be expressible in spawner JSON as pattern names, squad groups, formations, and sequence steps.

### Spawn Variation Selection

Select a spawn variation before writing concrete squads and formations only when the task includes spawner mapping.

Independent spawn variation authoring is defined in:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
```

Concrete SpawnSO JSON generation is defined in:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
```

Examples:

| Story / Monster Composition | Recommended Variation |
|---|---|
| Simple patrol or first encounter | `front_line_basic` |
| Melee with ranged support | `front_then_backline` |
| Fast attacker in monster pool | `front_then_flank` |
| Many weak monsters | `surround_swarm` or `front_line_basic` with higher quantity |
| Elite plus weak adds | `elite_anchor` |
| Objective or prop pressure | `objective_pressure` |
| Boss battle | `boss_phase_adds` |

If several variations match, choose the one that best expresses the story and difficulty budget.

Do not generate a new bespoke variation when an existing one can be filled by the monster pool.

If the current task is battle planning only, do not select a concrete `selectedVariationId`. Instead, preserve:

```text
requiredSpawnTags
forbiddenSpawnTags
spaceTags
rhythmTags
objectiveTags
requiredRoleSlotHints
```

These fields will let a later spawner mapping task choose a `SpawnVariationProfile`.

### Output

Planning can be written in task notes or a planning JSON under:

```text
Assets/Doc/Battle
```

### Validation

- The battle has one clear victory condition.
- Enemy pacing can be represented as spawn sequence steps.
- Required character IDs already exist or are planned for generation.
- Any prop behavior is described before authoring `propDefinitions`.
- Monster skill slots are connected to meaningful front/back/flank/surround placement.
- Back, flank, and surround spawns are intentional and fair.
- A reusable spawn variation is selected before concrete sequence authoring, when spawner generation is in scope.
- The monster pool can fill every required variation role slot, when a variation was selected.

## Step 2. Spawner JSON Generation

### Purpose

Create the spawn assets used by the battle by baking the selected variation and monster bindings into concrete JSON.

Skip this step when the current task only asks for battle planning or monster pool selection.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
Assets/Scripts/battle_spawn/Resource/Jsons/presets_all_in_one.json
```

### Main Work

1. Choose the selected variation.
2. Generate patterns required by the variation.
3. Generate squads by binding monster pool entries to variation role slots.
4. Generate formations when a variation repeats a squad as a larger layout.
5. Generate one main sequence for the battle.
6. Confirm the main sequence ID that `BattleSO` will reference.

### Output

Recommended spawner JSON path:

```text
Assets/Scripts/battle_spawn/Resource/Jsons/{battleId}.spawn.json
```

### Validation

- Every `npcId` matches an existing or planned `CharacterSO.characterId`.
- Every squad `patternId` exists in `patterns`.
- Every formation `squadId` exists in `squads`.
- Every formation `patternId` exists in `patterns`.
- Every sequence step `contentId` exists in `squads` or `formations`.
- `repeatMode` is `Once` or `Infinite`.
- `completionMode` is `AfterSpawnCompleted` or `AfterSpawnedEnemiesDefeated`.
- `selectedVariationId` and role bindings can be traced back to `BattleStoryContext` tags.

## Step 3. Battle JSON Generation

### Purpose

Create one BattleSO input JSON that references the generated main spawn sequence.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
```

### Main Work

1. Write `battleId` and `battleName`.
2. Set `victoryRule`.
3. Reference the main spawn sequence using `spawnSequenceId`.
4. Add battle props only if the battle needs them.
5. Add timed prop placements only for props that appear after battle start.
6. Set reward and relic drop values.

### Output

Recommended output path:

```text
Assets/Resources/battle/{battle_group}/{battleId}.json
```

### Validation

- `spawnSequenceId` matches the sequence generated in Step 2.
- `victoryRule` matches the intended battle flow.
- `SurviveTime` battles have `survivalTimeSeconds > 0`.
- Relic chances are in `0..100`.
- Props referenced by `timedPropPlacements` exist in `propDefinitions` or as existing `BattlePropSO` assets.

## Step 4. Unity SO Generation

### Spawner SO Generation

Open:

```text
BS/Spawn/Spawn SO FromJson Window
```

Use the all-in-one JSON option when possible:

```text
Bake All from Single JSON
```

This creates or updates:

```text
SpawnPatternSO
SpawnSquadSO
SpawnFormationSO
SpawnSequenceSO
```

### Battle SO Generation

Select the battle JSON in Unity Project view and run:

```text
Assets/Battle/Generate BattleSO From Json
```

This creates or updates:

```text
BattlePropSO
BattleSO
```

## Step 5. Scene And Prefab Verification

### Battle Scene Requirements

The battle scene must have runtime code that can initialize a `BattleSession` with a `BattleSO`.

When battle starts:

```text
BattleManager
  -> reads BattleSO.SpawnSequence
  -> ensures BattleSpawnManager
  -> BattleSpawnManager.PlaySequence(...)
```

### Required Runtime Assets

- Main `BattleSO`
- Referenced `SpawnSequenceSO`
- Referenced `SpawnContentSO`
- Referenced `SpawnPattern`
- Referenced NPC `CharacterSO`
- Referenced prop prefabs, if props are used
- Background prefab, if used

## Agent Checklist

- Read `SpawnerCreateGuide.md`.
- Read `SpawnSO.md`.
- Generate spawner JSON.
- Read `BattleSO.md`.
- Generate battle JSON.
- Confirm battle JSON has no legacy wave fields.
- Confirm main sequence ID is referenced by battle JSON.
- Confirm JSON is valid before handing it to Unity.
- Run or ask the user to run the Unity builders.
