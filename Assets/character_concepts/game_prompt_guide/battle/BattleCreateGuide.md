# Battle Create Guide

This guide defines how to create BattleSO data from story context, monster pool
data, and reusable spawn variations.

Battle data is responsible for choosing actual monsters. Spawner data is
responsible only for timing, placement, and role slots.

## Required References

- Battle story context guide: `Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md`
- Battle generation prompt: `Assets/character_concepts/game_prompt_guide/battle/BattleGenerationPrompt.md`
- Spawner creation guide: `Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md`
- Spawner SO schema: `Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md`
- Character planning data: `Assets/Doc/Character`
- Story data: `Assets/Doc/Story`

## Authoring Inputs

Battle creation should receive:

- act id
- chapter id
- episode or battle id
- battle story context
- selected monster pool
- desired battle intensity
- player progression context
- reusable spawn variation candidates

The battle generator should not invent monster identity directly from spawner
slots. It should first select monsters from the prepared monster pool, then bind
them to spawn slots.

## Battle Creation Pipeline

1. Read story context.
2. Determine battle purpose:
   - tutorial pressure
   - patrol encounter
   - ambush
   - survival wave
   - elite check
   - boss battle
   - boss reinforcement phase
3. Select monster pool entries.
4. Select a reusable spawn variation.
5. Read the selected spawner's required `spawnUnitKey` and `spawnRole` slots.
6. Map monsters to those slots through BattleSO `spawnUnitBindings`.
7. Create or update BattleSO.
8. Validate that the spawn sequence can resolve every required slot.

## BattleSO Data

BattleSO must store:

- battle identity and display metadata
- battle rules and rewards
- selected `SpawnSequenceSO`
- `spawnUnitBindings`

`spawnUnitBindings` is the bridge between abstract spawner slots and concrete
CharacterSO monsters.

Example concept:

```json
{
  "battleId": "battle.act1.chapter01.forest_ambush",
  "spawnSequenceId": "seq.act1.forest.ambush",
  "spawnUnitBindings": [
    {
      "unitKey": "spawn.front.pressure.melee",
      "role": "Melee",
      "characterId": "monster.act1.forest.striker"
    },
    {
      "unitKey": "spawn.rear.support.ranged",
      "role": "Ranged",
      "characterId": "monster.act1.forest.slinger"
    }
  ]
}
```

The exact runtime asset field is a CharacterSO reference. JSON authoring may use
`characterId` only as an editor/build-time lookup key.

## Spawner Selection Rules

Choose a spawn variation based on battle intent, not monster names.

Good matching signals:

- approach direction: front, rear, side, surround
- tempo: burst, staggered, looped, delayed
- threat shape: melee rush, ranged line, tank anchor, elite arrival
- story moment: chase, trap, ritual, defense, boss phase
- map topology: narrow lane, open arena, choke point, multi-entry space
- player skill test: movement, dodge timing, target priority, area denial

Avoid selecting a spawner because it was authored for a specific monster name.

## Monster Binding Rules

Use exact key binding when a specific slot matters.

Example:

```json
{
  "unitKey": "spawn.rear.support.ranged",
  "role": "Ranged",
  "characterId": "monster.act1.bandit.thrower"
}
```

Use role fallback when the same monster can satisfy many equivalent slots.

Example:

```json
{
  "unitKey": "",
  "role": "Melee",
  "characterId": "monster.act1.forest.striker"
}
```

Resolution order:

1. exact `unitKey`
2. fallback `role`

## Validation Rules

Battle build should validate:

- `spawnSequenceId` resolves to a generated SpawnSequenceSO.
- Every `spawnUnitKey` in the sequence is either exactly bound or has a role fallback.
- Every `spawnRole` value is a supported enum.
- Every binding resolves to a valid CharacterSO.
- No spawner slot key contains concrete monster names.
- Boss or elite roles are intentionally bound.
- The selected monster pool can support all required spawn pressure.

Spawner build should validate:

- every step has inline `content`
- every group has `spawnUnitKey` or `spawnRole`
- every `patternKind` is valid
- pattern config is legal for its kind

## Battle JSON Responsibility

Battle JSON may reference generated spawner assets, but it should not duplicate
the full spawner layout.

Battle JSON should contain:

- `spawnSequenceId`
- `spawnUnitBindings`

Spawner JSON should contain:

- sequence steps
- inline squad content
- squad/group patterns
- abstract spawn slot keys and roles

## Example Flow

Story context says:

- forest ambush
- fast melee pressure from the front
- ranged harassment from behind
- no boss

Spawner selection:

- choose a surround or rear-support spawn variation

Spawner slots:

- `spawn.front.pressure.melee`
- `spawn.rear.support.ranged`

Monster pool:

- forest striker
- forest slinger

BattleSO binding:

- melee slot -> forest striker CharacterSO
- ranged slot -> forest slinger CharacterSO

## Output Checklist

- BattleSO references the selected SpawnSequenceSO.
- BattleSO has all required `spawnUnitBindings`.
- The spawner JSON remains monster-agnostic.
- Monster selection comes from the prepared monster pool.
- The encounter purpose is readable from battle data and binding choices.
