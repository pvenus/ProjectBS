# Spawn SO Guide

## Purpose

Generate JSON used as input for battle spawn SO generation.

The spawn system is composed of four main asset types:

```text
SpawnPatternSO
  -> SpawnSquadSO
  -> SpawnFormationSO
  -> SpawnSequenceSO
```

`SpawnSequenceSO` is the asset referenced by `BattleSO`.

This document describes the concrete runtime JSON shape.

For independent reusable spawn variation authoring before monster binding, read:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
```

For converting a selected variation and selected monster pool into this JSON, read:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
```

## Output

Recommended all-in-one JSON path:

```text
Assets/Scripts/battle_spawn/Resource/Jsons/{encounter_id}.spawn.json
```

Reference example:

```text
Assets/Scripts/battle_spawn/Resource/Jsons/presets_all_in_one.json
```

Generated SO output defaults to:

```text
Assets/Scripts/battle_spawn/Resource/Generated
```

## All-In-One JSON Schema

```json
{
  "patterns": [],
  "squads": [],
  "formations": [],
  "sequences": []
}
```

Use all-in-one JSON for agent generation unless a task explicitly asks for separate files.

## Variation-Based Authoring

Spawn SO JSON is the baked output of a selected spawn variation.

Do not use concrete Spawn SO JSON as the only source of reusable spawner design. Keep independent selection rules, role slots, and balance knobs in `SpawnVariationProfile` planning data first.

The current runtime JSON does not require a separate `variation` field. Instead, encode the selected variation through:

- Pattern IDs
- Squad IDs
- Formation IDs
- Sequence IDs
- Sequence step order
- Sequence display name

Conceptual flow:

```text
SpawnVariationProfile + monster pool + role bindings
  -> bound squads/formations
  -> sequence steps
  -> SpawnSequenceSO
```

Preserve the selected variation ID in planning notes or the intermediate spawner mapping artifact. The final `SpawnSequenceSO` only needs the generated sequence ID that `BattleSO` references.

### Variation Naming

Use variation names in IDs when useful:

```text
front_line_basic
front_then_backline
front_then_flank
surround_swarm
objective_pressure
elite_anchor
boss_phase_adds
```

Examples:

```text
pattern.forest.front_then_flank.front.line.3
squad.forest.front_then_flank.front_basic
squad.forest.front_then_flank.flank_active
formation.forest.elite_anchor.adds
seq.forest.001.front_then_flank
```

### Variation Role Slots

A variation role slot is a placeholder filled by a monster from the battle monster pool.

Common role slots:

| Role Slot | Expected Monster Profile |
|---|---|
| `front_basic` | `basic_melee`, `passive_tank`, or simple `basic_attack` |
| `backline_ranged` | `basic_ranged`, `active_projectile`, support ranged |
| `flank_active` | `active_charge`, `active_jump`, fast melee |
| `swarm_basic` | low durability `basic_melee` or random-area group |
| `support_backline` | `passive_support`, `passive_aura` |
| `elite_anchor` | elite with `active_1` + `passive_1` |
| `boss_anchor` | boss with `active_2` or phase behavior |
| `objective_attacker` | monster whose target preference or passive supports objective pressure |

When generating final JSON, role slots become concrete `squads` or `formations`.

Example:

```text
front_basic -> squad.forest.front_then_flank.wolf_front_basic
flank_active -> squad.forest.front_then_flank.wolf_flank_active
```

### Monster Pool Binding Rules

Given a monster pool, bind monsters into variation role slots by this priority:

1. Skill slot profile match
2. Attack range match
3. Mobility match
4. Durability / tier match
5. Story or faction fit
6. Difficulty budget fit

If no monster fits a required role slot, choose another variation or remove that role from the generated sequence.

Do not distort a monster's spawn role only to satisfy a variation.

## ID Rules

Use stable IDs. IDs are not file paths.

```text
pattern.{group}.{shape_or_use}
squad.{group}.{composition}
formation.{group}.{layout}
seq.{group}.{purpose}
```

Examples:

```text
pattern.forest.line.3
squad.forest.wolf.3
formation.forest.wolf.circle
seq.forest.001.main
```

## Spatial Authoring Concepts

Spawner JSON should connect monster behavior to spatial intent.

Use character skill slots first, then spatial concepts.

Primary skill slot keys:

```text
basic_attack
active_1
active_2
active_3
passive_1
```

Design documents may mention `Passive2`, but normal character spawn authoring should only use a second passive when the source design explicitly supports it.

Use these spatial concepts in pattern IDs, squad IDs, and sequence display names when the position is meaningful:

```text
front
back
flank_left
flank_right
surround
backline
center
random
ambush
objective
```

Examples:

```text
pattern.forest.front.line.3
pattern.forest.backline.spread.3
pattern.forest.flank_left.cluster.2
pattern.forest.surround.4
squad.forest.blocker.front.3
squad.forest.archer.backline.2
squad.forest.diver.back.2
seq.forest.001.front_then_flank
```

## Skill-Slot Spawn Vocabulary

Use these keywords to connect a monster's skill slots to spawn choices.

These are authoring concepts, not JSON fields.

| Spawn Keyword | Slot Source | Use When | Spawn Guidance |
|---|---|---|---|
| `basic_melee` | `basic_attack` | Repeated close attack | Front or soft flank. |
| `basic_ranged` | `basic_attack` | Repeated projectile or ranged attack | Backline or wide flank. |
| `basic_area` | `basic_attack` | Repeated cone/area hit | Spread enemies; avoid dense stacking. |
| `active_charge` | `active_1` / `active_2` | Dash, rush, lane attack | Front lane or flank entry with readable distance. |
| `active_jump` | `active_1` / `active_2` | Leap, blink, dive | Flank/back entry with start delay. |
| `active_projectile` | `active_1` / `active_2` | Strong shot or projectile pattern | Backline spread or diagonal flank. |
| `active_area` | `active_1` / `active_2` | AoE, cone, ground zone | Lower density, wider spacing. |
| `active_summon` | `active_1` / `active_2` | Summons additional units | Protected backline or delayed sequence step. |
| `active_cc` | `active_1` / `active_2` | Stun, slow, pull, knockback | Avoid simultaneous surround pressure unless intended. |
| `passive_tank` | `passive_1` | Defense, shield, taunt, damage reduction | Front anchor or gate formation. |
| `passive_aura` | `passive_1` | Buff/debuff radius | Spawn near allies with intentional spacing. |
| `passive_support` | `passive_1` | Healing, buffing, protection | Behind frontline or second-line group. |
| `passive_enrage` | `passive_1` | Gets stronger over time/on damage/on death | Use staged timing; avoid overwhelming overlap. |
| `passive_objective` | `passive_1` | Interacts with props or structures | Spawn near objective side. |

ID examples using slot-derived vocabulary:

```text
squad.forest.wolf.basic_melee.front.3
squad.forest.archer.basic_ranged.backline.2
squad.forest.guard.passive_tank.front.2
squad.forest.shaman.passive_aura.support.1
seq.forest.001.basic_front_then_active_jump_flank
```

## Monster-To-Spawn Mapping

Use monster combat style to choose spawn side and pattern.

| Monster Style | Spawn Side | Pattern / Content Guidance |
|---|---|---|
| `basic_melee` | Front | Fixed line or arc. Give enough width to block movement. |
| `passive_tank` | Front center | Wide fixed formation. Usually first in sequence. |
| `active_jump` / `active_charge` | Back or flank | Small cluster, delayed sequence step. |
| `basic_ranged` / `active_projectile` | Backline or wide flank | Spread positions to avoid stacking projectiles. |
| `active_area` | Far back or protected center | Sparse pattern, often behind blockers. |
| `passive_support` / `passive_aura` | Behind frontline | Mixed squad with later group order. |
| swarm-style `basic_attack` | Front-wide, surround, or random | Random area with `quantity` and readable `slotInterval`. |
| flying movement profile | Wide random or flank | Avoid narrow blocker-only patterns. |
| `active_summon` | Backline or objective side | Guard with blockers or spawn after pressure begins. |
| boss slot profile | Center or staged arena anchor | Dedicated sequence step and clear timing. |

Use fixed patterns for authored tactical placement.

Use random patterns for pressure volume, swarms, and replay variation.

## Pattern JSON

Patterns define local spawn positions or random areas.

### Fixed Position Pattern

```json
{
  "patternId": "pattern.forest.line.3",
  "patternType": "FixedPosition",
  "displayName": "Forest Line 3",
  "positions": [
    {
      "localPosition": { "x": -1, "y": 0 },
      "rotation": 0
    },
    {
      "localPosition": { "x": 0, "y": 0 },
      "rotation": 0
    },
    {
      "localPosition": { "x": 1, "y": 0 },
      "rotation": 0
    }
  ],
  "rotation": 0,
  "scale": 1
}
```

### Random Pattern

```json
{
  "patternId": "pattern.forest.random.circle.r3",
  "patternType": "RangeRandom",
  "displayName": "Forest Random Circle R3",
  "shape": "Circle",
  "areaSize": { "x": 3, "y": 0 },
  "rotation": 0,
  "scale": 1
}
```

Pattern fields:

| Field | Required | Notes |
|---|---:|---|
| patternId | Yes | Unique pattern ID. |
| patternType | Yes | `FixedPosition` or `RangeRandom`. Empty is treated as `FixedPosition`. |
| displayName | No | Editor display/debug name. |
| positions | Fixed only | Array of local slots. |
| positions[].localPosition | Fixed only | Unity `Vector2`. |
| positions[].rotation | Fixed only | Slot rotation in degrees. |
| shape | Random only | `Circle` or `Rectangle`. |
| areaSize | Random only | Circle uses `x` as radius. Rectangle uses `x` width and `y` height. |
| rotation | No | Applied to fixed slots. |
| scale | No | Values `<= 0` are treated as `1`. |

### Pattern Placement Notes

`localPosition` is relative to the content anchor used at runtime.

Author patterns as reusable local layouts:

- Front patterns should usually be wider on `x` than `y`.
- Backline patterns should be sparse enough for ranged enemies.
- Flank patterns should be offset from the center line.
- Surround patterns should avoid placing too many enemies directly on top of the player.
- Random circle patterns use `areaSize.x` as radius.
- Random rectangle patterns use `areaSize.x` as width and `areaSize.y` as height.

## Squad JSON

Squads bind NPC character IDs to patterns.

```json
{
  "contentId": "squad.forest.wolf.3",
  "groupInterval": 2,
  "groups": [
    {
      "order": 0,
      "npcId": "character.wolf.easy.1",
      "patternId": "pattern.forest.line.3",
      "localOffset": { "x": 0, "y": 0 },
      "localRotation": 0,
      "slotInterval": 0.2,
      "quantity": 1
    }
  ]
}
```

Squad fields:

| Field | Required | Notes |
|---|---:|---|
| contentId | Yes | Unique `SpawnContentSO` ID. |
| groupInterval | No | Delay between groups. |
| groups | Yes | One or more group entries. |
| groups[].order | Yes | Execution order inside squad. |
| groups[].npcId | Yes | Existing `CharacterSO.characterId`. |
| groups[].patternId | No | Pattern used by this group. |
| groups[].localOffset | No | Offset from content anchor. |
| groups[].localRotation | No | Rotation offset in degrees. |
| groups[].slotInterval | No | Delay between slots. |
| groups[].quantity | No | Used by random patterns. Defaults to `1` when `<= 0`. |

### Squad Composition Notes

Use squad groups to express tactical layering:

```text
order 0  -> basic_melee / passive_tank front
order 10 -> basic_ranged / passive_support second line
order 20 -> active_jump / active_charge delayed pressure
```

Guidelines:

- Put front blockers and backline ranged enemies in separate groups.
- Use different `patternId` values for different roles inside the same squad.
- Use `localOffset` to move support behind blockers.
- Use `slotInterval` to make large spawns readable.
- Use `quantity` with `RangeRandom` patterns for swarm counts.

## Formation JSON

Formations repeat a squad through a formation pattern.

```json
{
  "contentId": "formation.forest.wolf.circle",
  "patternId": "pattern.forest.random.circle.r3",
  "squadId": "squad.forest.wolf.3",
  "slotInterval": 0.5,
  "quantity": 3
}
```

Formation fields:

| Field | Required | Notes |
|---|---:|---|
| contentId | Yes | Unique `SpawnContentSO` ID. |
| patternId | No | Pattern used to place squad copies. |
| squadId | Yes | Existing squad content ID. |
| slotInterval | No | Delay between repeated squad slots. |
| quantity | No | Used by random formation patterns. Defaults to `1` when `<= 0`. |

### Formation Placement Notes

Use formations when the same squad should appear in multiple locations.

Examples:

- Repeat a melee squad across a front line.
- Place the same small ranged squad at multiple backline positions.
- Scatter the same swarm squad in a random area.
- Surround the player with several copies of a light ambush squad.

Do not use a formation when one squad already describes the encounter clearly.

## Sequence JSON

Sequences orchestrate spawn content over time.

```json
{
  "sequenceId": "seq.forest.001.main",
  "displayName": "Forest Battle 001 Main",
  "repeatMode": "Once",
  "loopStartOrder": 0,
  "steps": [
    {
      "order": 0,
      "startDelay": 0,
      "contentId": "squad.forest.wolf.3",
      "completionMode": "AfterSpawnCompleted"
    },
    {
      "order": 10,
      "startDelay": 3,
      "contentId": "formation.forest.wolf.circle",
      "completionMode": "AfterSpawnedEnemiesDefeated"
    }
  ]
}
```

Sequence fields:

| Field | Required | Notes |
|---|---:|---|
| sequenceId | Yes | Unique sequence ID. Referenced by BattleSO. |
| displayName | No | Editor display/debug name. |
| repeatMode | Yes | `Once` or `Infinite`. |
| loopStartOrder | Infinite only | Step order to loop back to. |
| steps | Yes | One or more sequence steps. |
| steps[].order | Yes | Execution order. Use gaps like `0, 10, 20` for easier insertion. |
| steps[].startDelay | Yes | Must be `0` or greater. |
| steps[].contentId | Yes | Squad or formation content ID. |
| steps[].completionMode | Yes | `AfterSpawnCompleted` or `AfterSpawnedEnemiesDefeated`. |

### Sequence Pressure Notes

Use sequence steps to control when spatial pressure changes.

Examples:

```text
front blockers -> backline ranged
front swarm -> flank divers
single patrol -> surround ambush
phase 1 adds -> boss
```

Guidelines:

- Put readable front pressure before back or flank pressure unless the battle is an ambush.
- Use `AfterSpawnedEnemiesDefeated` for clean waves.
- Use `AfterSpawnCompleted` for overlapping pressure.
- Give back/flank divers a start delay when they spawn after another threat.
- Avoid simultaneous surround plus artillery unless the battle is intentionally difficult.

## Supported Enum Values

`patternType`:

```text
FixedPosition
RangeRandom
```

`shape`:

```text
Circle
Rectangle
```

`repeatMode`:

```text
Once
Infinite
```

`completionMode`:

```text
AfterSpawnCompleted
AfterSpawnedEnemiesDefeated
```

## Validation Rules

Before handing JSON to Unity, check:

- Every `patternId` is unique.
- Every `contentId` is unique across squads and formations.
- Every `sequenceId` is unique.
- Fixed patterns have at least one position.
- Random patterns have valid `shape` and positive `areaSize`.
- Every squad group has `npcId`.
- Every referenced `patternId` exists.
- Every formation has a valid `squadId`.
- Every sequence has at least one step.
- Every sequence step references an existing squad or formation `contentId`.
- `startDelay`, `groupInterval`, `slotInterval` are not negative.
- `quantity` should be `1` or greater unless relying on builder default.
- Infinite sequences have a `loopStartOrder` matching an existing step order.
- Spatial intent is consistent with monster role.
- Back, flank, and surround spawns have enough delay or distance to be fair.

## Minimal All-In-One Example

```json
{
  "patterns": [
    {
      "patternId": "pattern.forest.line.3",
      "patternType": "FixedPosition",
      "displayName": "Forest Line 3",
      "positions": [
        { "localPosition": { "x": -1, "y": 0 }, "rotation": 0 },
        { "localPosition": { "x": 0, "y": 0 }, "rotation": 0 },
        { "localPosition": { "x": 1, "y": 0 }, "rotation": 0 }
      ],
      "rotation": 0,
      "scale": 1
    }
  ],
  "squads": [
    {
      "contentId": "squad.forest.wolf.3",
      "groupInterval": 2,
      "groups": [
        {
          "order": 0,
          "npcId": "character.wolf.easy.1",
          "patternId": "pattern.forest.line.3",
          "localOffset": { "x": 0, "y": 0 },
          "localRotation": 0,
          "slotInterval": 0.2,
          "quantity": 1
        }
      ]
    }
  ],
  "formations": [],
  "sequences": [
    {
      "sequenceId": "seq.forest.001.main",
      "displayName": "Forest Battle 001 Main",
      "repeatMode": "Once",
      "loopStartOrder": 0,
      "steps": [
        {
          "order": 0,
          "startDelay": 0,
          "contentId": "squad.forest.wolf.3",
          "completionMode": "AfterSpawnCompleted"
        }
      ]
    }
  ]
}
```

## Agent Authoring Rules

- Prefer all-in-one JSON.
- Use `SpawnerVariationCreateGuide.md` for reusable variation profiles before writing concrete JSON.
- Define patterns before squads.
- Define squads before formations.
- Define formations before sequences.
- Use sequence IDs that battle JSON can reference directly.
- Do not put battle reward, background, or victory data in spawner JSON.
- Do not put NPC stat data in spawner JSON. NPC stats belong to `CharacterSO`.
