# Spawner Create Guide

This guide defines how to create reusable spawn variations as independent data.
Spawner data must describe where, when, and by which role a unit appears. It must
not decide which concrete monster or CharacterSO is used.

## Goal

Create spawn variations that can be reused across battles:

- Battle story and monster pool decide the encounter need.
- Spawner JSON provides reusable timing, placement, and role slots.
- BattleSO binds those spawn slots to actual CharacterSO assets.

The spawner is therefore a layout/timing asset, not a monster roster.

## Required References

- Spawner SO schema: `Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md`
- Battle creation guide: `Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md`
- Battle story context guide: `Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md`
- Source JSON folder: `Assets/Resources/battle/spawner/Jsons/sequence_presets`
- Generated sequence folder: `Assets/Resources/battle/spawner/Generated/Sequences`
- Generated squad folder: `Assets/Resources/battle/spawner/Generated/SpawnContents/Squads`

## Current Data Shape

Use one JSON file per spawner type.

Do not create separate pattern, squad, or formation preset JSON files. The current
source of truth is a typed sequence preset where each difficulty owns one inline
sequence and each step owns its inline content.

Path pattern:

```text
Assets/Resources/battle/spawner/Jsons/sequence_presets/{spawnerType}.json
```

Example:

```text
Assets/Resources/battle/spawner/Jsons/sequence_presets/elimination_90s_swarm.json
```

Allowed:

- one file per `spawnerType`
- difficulty entries under `difficulties`
- `SpawnSequenceSO`
- inline `SpawnSquadSO` content in each sequence step
- inline squad-level pattern
- inline group-level pattern
- `spawnUnitKey`
- `spawnRole`

Removed from new authoring:

- top-level pattern preset files
- formation preset files
- squad preset files
- `SpawnPatternSO`
- `SpawnFormationSO`
- monster names inside `spawnUnitKey`
- direct `CharacterSO` or monster binding inside spawner JSON
- one global `sequence_presets.json` array for new spawner types

## Typed Preset Shape

Each spawner type file is an object with difficulty entries.

```json
{
  "spawnerType": "elimination_90s_swarm",
  "displayName": "Elimination 90s Swarm",
  "difficulties": [
    {
      "difficulty": "normal",
      "targetPartySize": 3,
      "targetSpawnCount": 80,
      "spawnWindowSec": 60,
      "clearWindowSec": 30,
      "sequence": {
        "sequenceId": "seq.elimination_90s_swarm.normal",
        "displayName": "Elimination 90s Swarm Normal",
        "repeatMode": "Once",
        "loopStartOrder": 0,
        "steps": []
      }
    }
  ]
}
```

`sequenceId` and `contentId` should include the spawner type and difficulty.

Good:

```text
seq.elimination_90s_swarm.normal
squad.elimination_90s_swarm.normal.opening
```

Avoid IDs that only make sense for one battle unless the spawner is truly
battle-specific.

## Authoring Pipeline

1. Read battle/story context.
2. Identify the desired spawn pressure:
   - frontal wave
   - rear ambush
   - surround
   - staggered approach
   - ranged support line
   - elite arrival
   - boss reinforcement
3. Select a reusable spawn variation type.
4. Define sequence timing.
5. Define inline content per sequence step.
6. Define squad pattern and group patterns with enum-based configs.
7. Assign semantic `spawnUnitKey` and `spawnRole` placeholders.
8. Generate SpawnSequenceSO and SpawnSquadSO assets.
9. Let BattleSO bind those placeholders to actual monsters.

## Difficulty Model

Use `normal` as the baseline shape. Other difficulties should preserve the same
spawner type identity while changing count, monster strength, role mix, timing,
and variation pressure.

Supported planning tiers:

- `very_easy`: 1-player friendly. Fewer spawns than easy, longer gaps, simple roles.
- `easy`: 2-player friendly. Can reuse normal count with weaker bound monsters.
- `normal`: 3-player baseline. Defines the canonical rhythm and total count.
- `hard`: 3-player. Can reuse normal count with stronger bound monsters.
- `very_hard`: 3-player. Adds elite or higher roles and more irregular pressure.
- `boss`: 3-player. Uses adds and timing to frame a boss stage, not raw swarm count.

For the current 90-second elimination style, use this battle pacing:

- spawn window: about `60` seconds
- clear window: about `30` seconds
- final battle length target: about `90` seconds

Spawns should not arrive as one large burst after long rests. Prefer short rest
beats plus staggered slot intervals so the rest feels brief while individual
units still enter with readable timing.

## Difficulty Scoring Heuristics

Use these as authoring checks, not strict formulas:

- Total spawned count is the primary total difficulty driver.
- Spawn density is `targetSpawnCount / spawnWindowSec`.
- Ramp pressure is how quickly count and role threat rise over time.
- Overlap pressure increases when later steps start before earlier enemies are
  likely cleared.
- Role pressure increases when ranged, tank, support, elite, boss, or irregular
  slots are introduced.
- Monster binding can change perceived difficulty without changing the spawner.

When two presets have the same total count, the one with shorter spawn windows,
more overlap, stronger role mix, or earlier elites is harder.

## Count Calculation

Approximate one content count as:

```text
squadPattern slot count * sum(group pattern slot count * group quantity)
```

Step count is the content count. Sequence count is the sum of all step counts.

Examples:

- `Circle count 2` squad pattern + one `Line count 2` group = `4`
- `Circle count 2` squad pattern + `Line count 3` and `Point count 1` groups = `8`
- `Point` squad pattern + one `Line count 2` group = `2`

Always keep `targetSpawnCount` in sync with the calculated count.

## Spawn Slot Naming

`spawnUnitKey` must describe a spawn slot, not a monster.

Good:

```json
"spawnUnitKey": "spawn.step.0.group.0.melee"
```

```json
"spawnUnitKey": "spawn.rear.ambush.ranged"
```

```json
"spawnUnitKey": "spawn.boss.adds.elite"
```

Bad:

```json
"spawnUnitKey": "monster.forest.striker"
```

```json
"spawnUnitKey": "character.some_monster"
```

```json
"spawnUnitKey": "named_archer_unit"
```

Monster identity belongs to BattleSO `spawnUnitBindings`, not the spawner.

## Role Rules

Each group should provide at least one of these:

- `spawnUnitKey`
- `spawnRole`

Recommended: provide both.

`spawnRole` is used as a semantic fallback when no exact key binding exists.

Common roles:

- `Melee`
- `Ranged`
- `Tank`
- `Support`
- `Elite`
- `Boss`
- `Any`

## Pattern Rules

Patterns are inline data, not separate SO assets.

Use `patternKind` enum and a small config payload.

Supported pattern kinds:

- `None`
- `Point`
- `Line`
- `Circle`
- `Grid`
- `Triangle`
- `Random`

Common config fields:

- `count`
- `rows`
- `columns`
- `size`
- `spacing`
- `areaSize`
- `rotation`
- `scale`

Use only fields that matter for the selected `patternKind`.

Example:

```json
{
  "patternKind": "Triangle",
  "displayName": "Triangle 3 Slot",
  "rows": 2,
  "spacing": 1.2,
  "rotation": 0.0
}
```

## Squad Pattern vs Group Pattern

There are two pattern levels.

`content.squadPattern` defines where repeated squad instances appear.

Example: place the whole squad around the player in a circle.

`content.groups[].pattern` defines how units inside one selected squad point are
arranged.

Example: after one squad point is selected, place three melee units in a triangle.

This replaces the old formation preset concept.

## Timing Rules

Sequence controls macro timing.

Use:

- `order`
- `startDelay`
- `completionMode`
- `repeatMode`
- `loopStartOrder`

Content controls local spawn spacing.

Use:

- `groupInterval`
- group `slotInterval`
- group `quantity`
- `squadPatternSlotInterval`
- `squadPatternQuantity`

Default group values:

- `quantity`: `1`
- `slotInterval`: `0`

## JSON Example

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
              "groupInterval": 0.4,
              "squadPattern": {
                "patternKind": "Circle",
                "count": 4,
                "size": 4.5,
                "rotation": 25.0
              },
              "squadPatternQuantity": 1,
              "squadPatternSlotInterval": 0.4,
              "groups": [
                {
                  "order": 0,
                  "spawnUnitKey": "spawn.front.pressure.melee",
                  "spawnRole": "Melee",
                  "quantity": 1,
                  "slotInterval": 0.2,
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

## Validation Checklist

- Sequence step has inline `content`.
- `content.contentId` is unique and meaningful.
- No monster name appears in `spawnUnitKey`.
- No `character.*` key is used in spawner JSON.
- Every group has `spawnUnitKey` or `spawnRole`.
- `patternKind` is a supported enum value.
- Pattern config matches the selected `patternKind`.
- No separate pattern, squad, or formation preset JSON is required.
- BattleSO can bind every required exact key or role fallback.

## Output Contract

Spawner generation produces reusable spawn assets.

Battle generation must then connect:

- generated `SpawnSequenceSO`
- BattleSO `spawnUnitBindings`
- selected CharacterSO assets from the battle monster pool

Spawner JSON alone is intentionally incomplete. It becomes executable only after
BattleSO provides monster bindings.
