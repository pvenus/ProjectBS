# Spawn SO Schema

This document describes the current spawn ScriptableObject authoring model.

Spawner assets are reusable layout and timing data. They do not directly contain
monster identity. BattleSO completes the runtime setup by binding spawn slots to
CharacterSO assets.

## Runtime Asset Model

Current spawn assets:

- `SpawnSequenceSO`
- `SpawnSquadSO`
- inline `SpawnSquadGroup`
- inline `SpawnPatternData`
- `SpawnUnitBinding`

Removed from new authoring:

- `SpawnPatternSO`
- `SpawnFormationSO`
- `SpawnNpcPoolSO`
- separate pattern preset JSON
- separate formation preset JSON
- separate squad preset JSON

## Responsibility Split

Spawner data decides:

- when a spawn step starts
- where a group appears
- how groups are repeated
- what role a spawn slot expects
- local pattern shape and spacing

Battle data decides:

- which `SpawnSequenceSO` is used
- which CharacterSO fills each spawn slot
- which monster pool entry is assigned to each role/key

## JSON Source

Current preset folder:

`Assets/Resources/battle/spawner/Jsons/sequence_presets`

Use one JSON file per spawner type:

```text
Assets/Resources/battle/spawner/Jsons/sequence_presets/{spawnerType}.json
```

The generator expects typed preset files with `difficulties[].sequence` and
inline step content. New authoring should not depend on separate
`pattern_presets`, `formation_presets`, or `squad_presets` files.

Generated assets are written under:

```text
Assets/Resources/battle/spawner/Generated/Sequences
Assets/Resources/battle/spawner/Generated/SpawnContents/Squads
```

## Sequence Schema

New spawner type files use this root object:

```json
{
  "spawnerType": "example_type",
  "displayName": "Example Type",
  "difficulties": [
    {
      "difficulty": "normal",
      "targetPartySize": 3,
      "targetSpawnCount": 0,
      "spawnWindowSec": 60,
      "clearWindowSec": 30,
      "sequence": {
        "sequenceId": "seq.example_type.normal",
        "displayName": "Example Type Normal",
        "repeatMode": "Once",
        "loopStartOrder": 0,
        "steps": []
      }
    }
  ]
}
```

Legacy importer shapes may still appear in old data:

```json
{
  "sequences": []
}
```

```json
[
  {
    "sequenceId": "seq.example",
    "displayName": "Example Sequence",
    "repeatMode": "Once",
    "loopStartOrder": 0,
    "steps": []
  }
]
```

Do not use legacy root shapes for new spawner types.

## Typed Preset Fields

Root object:

Required:

- `spawnerType`
- `difficulties`

Optional:

- `displayName`

Difficulty object:

Required:

- `difficulty`
- `sequence`

Recommended:

- `targetPartySize`
- `targetSpawnCount`
- `spawnWindowSec`
- `clearWindowSec`

Supported difficulty names:

- `very_easy`
- `easy`
- `normal`
- `hard`
- `very_hard`
- `boss`

`targetSpawnCount` must match the calculated sequence count.

## Sequence Fields

```json
{
  "sequenceId": "seq.act1.chapter01.ambush",
  "displayName": "Act1 Chapter01 Ambush",
  "repeatMode": "Once",
  "loopStartOrder": 0,
  "steps": []
}
```

Required:

- `sequenceId`
- `steps`

Optional:

- `displayName`
- `repeatMode`
- `loopStartOrder`

Supported `repeatMode` values are defined by code enum. Use existing enum names
only.

## Step Schema

```json
{
  "order": 0,
  "startDelay": 0.0,
  "completionMode": "AfterSpawnCompleted",
  "content": {}
}
```

Required:

- `order`
- `content`

Optional:

- `startDelay`
- `completionMode`

Do not use legacy step-level `contentId` references for new data. Put the content
inline in the step.

## Content Schema

Inline content becomes a `SpawnSquadSO`.

```json
{
  "contentId": "squad.act1.chapter01.ambush.opening",
  "displayName": "Ambush Opening Squad",
  "groupInterval": 0.4,
  "squadPattern": {},
  "squadPatternQuantity": 1,
  "squadPatternSlotInterval": 0.0,
  "groups": []
}
```

Required:

- `contentId`
- `groups`

Optional:

- `displayName`
- `groupInterval`
- `squadPattern`
- `squadPatternQuantity`
- `squadPatternSlotInterval`

`squadPattern` repeats the whole squad layout over multiple selected points.
This is the replacement for the old formation concept.

## Group Schema

```json
{
  "order": 0,
  "spawnUnitKey": "spawn.front.pressure.melee",
  "spawnRole": "Melee",
  "localOffset": { "x": 0.0, "y": 0.0 },
  "localRotation": 0.0,
  "quantity": 1,
  "slotInterval": 0.0,
  "pattern": {}
}
```

Required:

- `order`
- `spawnUnitKey` or `spawnRole`

Optional:

- `localOffset`
- `localRotation`
- `quantity`
- `slotInterval`
- `pattern`

`quantity` defaults to `1`.
`slotInterval` defaults to `0`.

`spawnUnitKey` must be a semantic slot key. It must not be a monster name.

## Spawn Role

Supported role values are code enum values.

Common roles:

- `Any`
- `Melee`
- `Ranged`
- `Tank`
- `Support`
- `Elite`
- `Boss`

Use role as a fallback binding category. Use key when a slot needs a specific
monster assignment.

## Pattern Schema

Patterns are inline config objects.

```json
{
  "patternKind": "Circle",
  "displayName": "Circle 6",
  "count": 6,
  "size": 4.0,
  "rotation": 0.0
}
```

Supported pattern kinds:

- `None`
- `Point`
- `Line`
- `Circle`
- `Grid`
- `Triangle`
- `Random`

Common fields:

- `count`
- `rows`
- `columns`
- `size`
- `spacing`
- `areaSize`
- `rotation`
- `scale`

Use only the fields needed by the selected kind.

## Pattern Config Guide

`None`

- no config required

`Point`

- optional `rotation`

`Line`

- `count`
- `spacing` or `size`
- optional `rotation`

`Circle`

- `count`
- `size` as radius
- optional `rotation`

`Grid`

- `rows`
- `columns`
- `spacing` or `size`
- optional `rotation`

`Triangle`

- `rows` or `count`
- `spacing` or `size`
- optional `rotation`

`Random`

- `count`
- `areaSize`
- optional `scale`

## Full Example

```json
{
  "spawnerType": "field_ambush",
  "displayName": "Field Ambush",
  "difficulties": [
    {
      "difficulty": "normal",
      "targetPartySize": 3,
      "targetSpawnCount": 12,
      "spawnWindowSec": 30,
      "clearWindowSec": 15,
      "sequence": {
        "sequenceId": "seq.field_ambush.normal",
        "displayName": "Field Ambush Normal",
        "repeatMode": "Once",
        "loopStartOrder": 0,
        "steps": [
          {
            "order": 0,
            "startDelay": 0.0,
            "completionMode": "AfterSpawnCompleted",
            "content": {
              "contentId": "squad.field_ambush.normal.opening",
              "displayName": "Field Ambush Opening",
              "groupInterval": 0.4,
              "squadPattern": {
                "patternKind": "Circle",
                "count": 4,
                "size": 4.5,
                "rotation": 25.0
              },
              "squadPatternQuantity": 1,
              "squadPatternSlotInterval": 0.2,
              "groups": [
                {
                  "order": 0,
                  "spawnUnitKey": "spawn.front.pressure.melee",
                  "spawnRole": "Melee",
                  "quantity": 1,
                  "slotInterval": 0.15,
                  "pattern": {
                    "patternKind": "Line",
                    "count": 2,
                    "spacing": 0.9
                  }
                },
                {
                  "order": 1,
                  "spawnUnitKey": "spawn.rear.support.ranged",
                  "spawnRole": "Ranged",
                  "localOffset": { "x": 0.0, "y": -1.5 },
                  "quantity": 1,
                  "pattern": {
                    "patternKind": "Point"
                  }
                }
              ]
            }
          }
        ]
      }
    }
  ]
}
```

## Battle Binding Example

Spawner JSON:

```json
{
  "spawnUnitKey": "spawn.front.pressure.melee",
  "spawnRole": "Melee"
}
```

BattleSO binding:

```json
{
  "unitKey": "spawn.front.pressure.melee",
  "role": "Melee",
  "characterId": "monster.act1.forest.striker"
}
```

At runtime the resolver checks exact `unitKey` first, then role fallback.

## Validation Checklist

- Root is one typed spawner object for new files.
- File path is `sequence_presets/{spawnerType}.json`.
- Root `spawnerType` matches the file name.
- Root contains difficulty entries.
- Every difficulty has a `sequence`.
- Every sequence has a unique `sequenceId`.
- Every step has inline `content`.
- Every content block has a unique `contentId`.
- Every group has `spawnUnitKey` or `spawnRole`.
- `spawnUnitKey` does not contain concrete monster identity.
- `patternKind` is valid.
- Pattern config is valid for its kind.
- `targetSpawnCount` matches the calculated total spawn count.
- BattleSO provides bindings for all required slots.

Spawner JSON alone is not considered executable. It must be paired with
BattleSO `spawnUnitBindings`.
