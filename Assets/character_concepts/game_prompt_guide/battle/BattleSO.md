# BattleSO Guide

## Purpose

Generate a Battle JSON file used as the input for `BattleSO` generation.

`BattleSO` owns battle-level data:

- Battle identity
- Background prefab reference
- Main `SpawnSequenceSO` reference
- Spawn unit bindings from abstract spawner slots to CharacterSO assets
- Optional timed battle props
- Victory condition
- Reward and relic drop values

NPC spawn waves are not authored directly in `BattleSO`. Use `SpawnSequenceSO`
from the spawner guide and reference it from battle JSON.

Spawner JSON is intentionally monster-agnostic. BattleSO must provide
`spawnUnitBindings` so the selected spawn slots can resolve to actual
CharacterSO monsters at runtime.

## Output

Save battle JSON near the generated battle asset target.

Recommended output path:

```text
Assets/Resources/battle/{battle_group}/{battleId}.json
```

The `BattleJsonGenerator` creates or updates a `BattleSO` asset in the same folder as the JSON when generated from a selected JSON file.

## Required References

Read these guides before authoring battle JSON:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
```

If the battle uses props, also read the prop section in this file.

## ID Rules

Battle IDs use:

```text
battle.{area_or_group}.{index_or_name}
```

Examples:

```text
battle.forest.001
battle.tutorial.first_contact
battle.boss.black_guard
```

Spawn sequence IDs are external references and should use the spawner ID format:

```text
seq.{encounter_group}.{purpose}
```

## JSON Schema

```json
{
  "battleId": "battle.forest.001",
  "battleName": "Forest Ambush",
  "victoryRule": "ClearAllEnemies",
  "survivalTimeSeconds": 0,
  "backgroundPrefab": "ForestBattleBackground",
  "spawnSequenceId": "seq.forest.001.main",
  "spawnSequencePath": "",
  "spawnUnitBindings": [
    {
      "unitKey": "spawn.front.pressure.melee",
      "role": "Melee",
      "characterId": "monster.act1.forest.striker"
    }
  ],
  "rewardExperience": 30,
  "normalRelicDropChance": 5,
  "bossRelicDropChance": 0,
  "timedPropPlacements": [],
  "propDefinitions": []
}
```

## Fields

| Field | Type | Required | Notes |
|---|---:|---:|---|
| battleId | string | Yes | Unique BattleSO ID. |
| battleName | string | No | Display/debug name. |
| victoryRule | enum string | Yes | `KillBoss`, `ClearAllEnemies`, or `SurviveTime`. |
| survivalTimeSeconds | number | Yes | Must be `0` or greater. Used by `SurviveTime`. |
| backgroundPrefab | string | No | Prefab asset name lookup key. |
| spawnSequenceId | string | Yes if path empty | Main `SpawnSequenceSO` lookup key. |
| spawnSequencePath | string | Yes if id empty | Asset path or path relative to the battle JSON folder. |
| spawnUnitBindings | array | Yes for spawner-driven battles | Maps spawner slot keys/roles to CharacterSO lookup IDs. |
| rewardExperience | number | Yes | Must be `0` or greater. |
| normalRelicDropChance | number | Yes | `0` to `100`. |
| bossRelicDropChance | number | Yes | `0` to `100`. |
| timedPropPlacements | array | No | Props spawned by battle time. |
| propDefinitions | array | No | Inline `BattlePropSO` definitions generated before BattleSO. |

## Victory Rules

Supported `victoryRule` values:

```text
KillBoss
ClearAllEnemies
SurviveTime
```

Use `ClearAllEnemies` for normal spawn-sequence battles.

Use `KillBoss` only when the spawn sequence includes a boss enemy and runtime boss kill tracking is expected.

Use `SurviveTime` only when `survivalTimeSeconds` is greater than `0`.

## Spawn Sequence Reference

Every battle JSON must reference a `SpawnSequenceSO`.

Preferred:

```json
{
  "spawnSequenceId": "seq.forest.001.main",
  "spawnSequencePath": ""
}
```

Use `spawnSequencePath` when the sequence asset is in a known folder:

```json
{
  "spawnSequenceId": "",
  "spawnSequencePath": "Assets/Scripts/battle_spawn/Resource/Generated/Sequences/seq.forest.001.main.asset"
}
```

If both are present, the builder first tries `spawnSequencePath`, then `spawnSequenceId`.

## Spawn Unit Bindings

Every spawner-driven battle must bind abstract spawner slots to concrete
CharacterSO monsters.

Spawner JSON owns:

```json
{
  "spawnUnitKey": "spawn.front.pressure.melee",
  "spawnRole": "Melee"
}
```

Battle JSON owns:

```json
{
  "spawnUnitBindings": [
    {
      "unitKey": "spawn.front.pressure.melee",
      "role": "Melee",
      "characterId": "monster.act1.forest.striker"
    },
    {
      "unitKey": "",
      "role": "Ranged",
      "characterId": "monster.act1.forest.slinger"
    }
  ]
}
```

Fields:

| Field | Required | Notes |
|---|---:|---|
| unitKey | No | Exact spawner slot key. Use this when a specific slot matters. |
| role | No | Fallback role. Use exact `SpawnUnitRole` enum names. |
| characterId | Yes | Editor/build-time lookup key for the CharacterSO. |

At runtime the resolver checks:

1. exact `unitKey`
2. fallback `role`

The Battle JSON builder must convert each entry into `BattleSO.spawnUnitBindings`
with a CharacterSO reference. Do not put `characterId` directly in spawner JSON.

## Timed Prop Placements

Use `timedPropPlacements` when a prop should appear after battle start.

```json
{
  "spawnTimeSeconds": 10,
  "propId": "prop.forest.seal.001",
  "position": { "x": 0, "y": 1.5, "z": 0 },
  "rotationZ": 0,
  "runtimeId": "seal_01"
}
```

Fields:

| Field | Required | Notes |
|---|---:|---|
| spawnTimeSeconds | Yes | Must be `0` or greater. |
| propId | Yes | Must match an inline or existing `BattlePropSO` ID. |
| position | No | Defaults to zero if omitted. |
| rotationZ | No | Z rotation in degrees. |
| runtimeId | No | Runtime lookup/debug ID. |

## Battle Prop Definitions

Use `propDefinitions` to create or update `BattlePropSO` assets before `BattleSO`.

```json
{
  "propId": "prop.forest.seal.001",
  "role": "Seal",
  "prefab": "BattlePropSeal",
  "skills": [],
  "stateVisuals": [
    {
      "state": "Normal",
      "animationClip": "seal_idle",
      "effectPrefab": ""
    }
  ],
  "spawnOnHit": {
    "spawnHitThreshold": 10,
    "spawnPropOnHit": "prop.forest.seal.fragment",
    "destroyAfterSpawnOnHit": true
  },
  "spawnSequenceSpawner": {
    "spawnSequenceId": "seq.forest.001.seal_spawn",
    "spawnSequencePath": "",
    "playOnInitialize": false
  }
}
```

Supported `role` values:

```text
None
Grave
Altar
Seal
Gate
Core
Generator
EscortTarget
DefenseTarget
SpawnPoint
```

Supported `stateVisuals[].state` values:

```text
None
Normal
Activated
Casting
Contested
Damaged
Corrupted
Destroyed
Cleared
```

## Validation Rules

Parsing validation rejects:

- Missing `battleId`
- Missing or invalid `victoryRule`
- Missing both `spawnSequenceId` and `spawnSequencePath`
- Invalid `spawnUnitBindings[].role`
- Missing `spawnUnitBindings[].characterId`
- `spawnUnitBindings[]` entry with neither `unitKey` nor `role`
- Negative `survivalTimeSeconds`
- Negative `rewardExperience`
- Relic drop chances outside `0..100`
- Missing timed prop `propId`
- Negative timed prop `spawnTimeSeconds`
- Duplicate inline `propId`
- Invalid `BattlePropRole`
- Invalid `BattlePropState`
- `spawnOnHit.spawnHitThreshold <= 0`
- `spawnSequenceSpawner.playOnInitialize = true` without sequence id/path

Build validation also rejects:

- Referenced `SpawnSequenceSO` not found
- Referenced `CharacterSO` for a spawn unit binding not found
- Required spawn sequence slot not covered by exact key binding or role fallback

## Minimal Battle Example

```json
{
  "battleId": "battle.forest.001",
  "battleName": "Forest Ambush",
  "victoryRule": "ClearAllEnemies",
  "survivalTimeSeconds": 0,
  "backgroundPrefab": "ForestBattleBackground",
  "spawnSequenceId": "seq.forest.001.main",
  "spawnSequencePath": "",
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
  ],
  "rewardExperience": 30,
  "normalRelicDropChance": 5,
  "bossRelicDropChance": 0,
  "timedPropPlacements": [],
  "propDefinitions": []
}
```

## Agent Authoring Rules

- Generate or confirm spawner JSON first.
- Do not write old wave fields such as `waveId`, `waveSO`, `waveJsonPath`, or `waveSpawner`.
- Do not author NPC spawn waves inside battle JSON.
- Do author `spawnUnitBindings` so abstract spawner slots resolve to CharacterSO assets.
- Keep battle JSON focused on battle-level orchestration.
- Use exact enum names.
- Keep optional arrays empty when unused.
- Verify the referenced `SpawnSequenceSO` can be generated from spawner JSON.
- Verify every required spawner slot is covered by exact key or role fallback.
