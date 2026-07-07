# Spawner Variation Create Guide

## Purpose

This document defines how to create reusable spawn variation profiles before battle generation.

A spawn variation is an independent spawner design unit. It describes spatial pressure, rhythm, role slots, balance knobs, and selection rules.

It does not choose final monsters for a battle.

Battle generation should first select a monster pool. A later spawner mapping step will choose one of these spawn variations and bind the selected monster pool into its role slots.

## Related Files

Read these files with this guide:

```text
Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
```

Use `SpawnSO.md` only when converting a selected variation into concrete spawn JSON.

## Pipeline Position

Create spawn variations before battle-specific spawner JSON.

```text
Spawner variation authoring
  -> SpawnVariationProfile pool
  -> BattleStoryContext
  -> Monster pool selection
  -> Spawner variation selection
  -> Monster role binding
  -> Concrete typed spawner JSON
  -> BattleSO spawnSequenceId
```

Current battle generation may stop at monster pool selection. Keep enough tags and IDs in the battle context so a later step can select a matching spawn variation.

## Output Concept

Use this conceptual artifact name:

```text
SpawnVariationProfile
```

It is a planning artifact for agents, not the final Unity `SpawnSequenceSO`.

Recommended planning path:

```text
Assets/Doc/Spawner/{spawner_group}/spawn_variation_profile.{variation_id}.json
```

Recommended index path:

```text
Assets/Doc/Spawner/{spawner_group}/spawn_variation_catalog.{spawner_group}.json
```

Final runtime spawn JSON still follows `SpawnSO.md` and should be written only
after a battle has selected monsters for the variation roles.

Runtime spawner JSON path:

```text
Assets/Resources/battle/spawner/Jsons/sequence_presets/{spawnerType}.json
```

The runtime file contains one spawner type and one or more difficulty objects.

## Minimal Profile Shape

```json
{
  "variationId": "spawn_variation.front_then_flank.basic",
  "displayName": "Front Then Flank Basic",
  "spawnerGroupId": "generic_field_pressure",
  "version": 1,
  "purpose": "Readable front contact followed by delayed side pressure.",
  "selectionTags": {
    "locationTags": ["forest", "village", "open_path"],
    "situationTags": ["ambush", "tracking", "breakthrough"],
    "intentTags": ["pressure", "split_attention"],
    "spaceTags": ["front", "flank"],
    "rhythmTags": ["delayed_ambush", "escalating"],
    "difficultyTags": ["normal", "hard"]
  },
  "avoidTags": {
    "situationTags": ["boss_intro"],
    "spaceTags": ["heavy_surround"],
    "difficultyTags": ["easy_intro"]
  },
  "roleSlots": [
    {
      "slotId": "front_basic",
      "required": true,
      "acceptedMonsterTags": ["melee", "tank", "basic_melee"],
      "forbiddenMonsterTags": ["boss", "artillery"],
      "preferredRange": "melee",
      "pressureSide": "front",
      "countHint": { "min": 2, "max": 4 }
    },
    {
      "slotId": "flank_active",
      "required": true,
      "acceptedMonsterTags": ["fast", "ambush", "active_charge", "active_jump"],
      "forbiddenMonsterTags": ["slow_tank"],
      "preferredRange": "melee",
      "pressureSide": "flank",
      "countHint": { "min": 1, "max": 3 }
    }
  ],
  "sequenceShape": [
    {
      "stepKey": "front_contact",
      "roleSlotId": "front_basic",
      "space": "front",
      "rhythm": "initial",
      "completionModeHint": "AfterSpawnCompleted",
      "startDelayHint": 0
    },
    {
      "stepKey": "flank_reveal",
      "roleSlotId": "flank_active",
      "space": "flank",
      "rhythm": "delayed",
      "completionModeHint": "AfterSpawnCompleted",
      "startDelayHint": 3
    }
  ],
  "balanceKnobs": {
    "baseThreat": 3,
    "density": "medium",
    "overlap": "low",
    "eliteAllowance": "none_or_one",
    "rangedAllowance": "optional",
    "surroundAllowance": "none",
    "normalTargetSpawnCount": 80,
    "spawnWindowSec": 60,
    "clearWindowSec": 30,
    "recommendedDurationSec": { "min": 25, "max": 50 }
  },
  "fairnessRules": [
    "The flank step must be delayed after the front contact.",
    "Do not use heavy ranged pressure with this variation unless difficulty is hard."
  ],
  "handoffIds": {
    "selectedVariationId": "spawn_variation.front_then_flank.basic",
    "expectedSequenceIdPattern": "seq.{battle_group}.{battle_id}.front_then_flank"
  }
}
```

## Required Fields

| Field | Required | Purpose |
|---|---:|---|
| variationId | Yes | Stable ID used by battle/spawner mapping. |
| displayName | Yes | Human-readable name. |
| spawnerGroupId | Yes | Reuse group such as field, shrine, hideout, boss, objective. |
| purpose | Yes | Short explanation of the encounter pressure. |
| selectionTags | Yes | Tags used to match `BattleStoryContext`. |
| avoidTags | No | Tags that should reject this variation. |
| roleSlots | Yes | Abstract monster roles to fill later. |
| sequenceShape | Yes | Timing and pressure shape before concrete JSON. |
| balanceKnobs | Yes | Difficulty and density controls. |
| fairnessRules | No | Human-readable constraints. |
| handoffIds | Yes | IDs preserved for later battle/spawner mapping. |

## Difficulty Expansion Policy

Design one canonical shape per spawner type, then expand it into difficulty
objects. Do not create unrelated shapes just because the difficulty changes.

Use `normal` as the baseline:

- 3-player target
- canonical step order
- canonical role mix
- canonical total spawn count
- canonical spawn window

Difficulty expansion:

- `very_easy`: 1-player target. Reduce count below easy, increase gaps, avoid ranged/tank/elite pressure unless required.
- `easy`: 2-player target. Usually keep the normal shape but bind weaker monsters or slightly reduce role pressure.
- `normal`: 3-player baseline. Defines the intended rhythm and count.
- `hard`: 3-player target. Usually keep normal count but bind stronger monsters.
- `very_hard`: 3-player target. Add elite or higher roles, irregular flank/surround pressure, or extra count.
- `boss`: 3-player target. Design supporting spawns around boss readability and phase pressure.

The same spawner type may therefore have several balance versions. A difficulty
object may change count and roles, but it should still feel like the same type.

## 90-Second Elimination Baseline

For hack-and-slash elimination stages, use this baseline:

- total battle target: `90` seconds
- spawn activity target: about `60` seconds
- cleanup target: about `30` seconds
- normal party size: `3`
- very easy party size: `1`
- easy party size: `2`

Avoid the pattern of long rest followed by one large burst. Instead:

- keep rest beats short
- stagger squad slots with `squadPatternSlotInterval`
- stagger units inside a group with `slotInterval`
- make later waves feel denser through role mix or overlap, not only instant count

For example, a very easy 90-second swarm can spawn only 28 units, finish spawning
around 54 seconds, and leave the player enough time to clean up without feeling
like the last enemies appear artificially at the end.

## Balance Heuristics

Estimate difficulty from several axes:

- total count: total spawned units
- density: count per spawn window second
- ramp: how quickly pressure increases
- overlap: whether new steps arrive before old enemies are cleared
- role mix: ranged, tank, support, elite, boss, irregular flank/surround
- monster binding: weak or strong CharacterSO assigned to the same role slot

Same count does not mean same difficulty. A shorter spawn window, stronger role
mix, earlier elite pressure, or higher overlap makes a preset harder.

Use monster group weight ratios when selecting bindings. The spawner provides
role slots and count ratios; BattleSO bindings decide the actual CharacterSO and
therefore the final weighted difficulty.

## Selection Tags

Use the same vocabulary as `BattleStoryContextGuide.md` so the later selection step can compare tags directly.

### Location Tags

Examples:

```text
village
forest
narrow_path
training_ground
hut
mountain_hideout
altar
ruins
boss_arena
```

### Situation Tags

Examples:

```text
patrol_contact
ambush
tracking
defense
escort
breakthrough
objective_pressure
ritual_hint
reinforcement
boss_intro
phase_change
rescue_aftermath
```

### Intent Tags

Examples:

```text
readable_intro
pressure
split_attention
protect_objective
burst_threat
attrition
survive_contact
phase_change
boss_resolution
```

### Space Tags

Examples:

```text
front
back
flank
flank_left
flank_right
backline
surround
objective
center
random
elevated
```

### Rhythm Tags

Examples:

```text
single_wave
wave_clear
escalating
overlap_pressure
delayed_ambush
reinforcement
loop_pressure
boss_phase
summon_support
```

## Role Slot Rules

Role slots must describe what kind of monster can fill the slot, not a specific character.

Good:

```text
front_basic -> melee, tank, basic_melee
backline_ranged -> ranged, active_projectile
objective_attacker -> captor, fast, objective_pressure
elite_anchor -> elite, tank, leader
```

Avoid:

```text
front_basic -> character.black_cloth_raider.1
```

Concrete character IDs are added later during monster role binding.

## Core Variation Catalog

Use these base variations before inventing new ones.

| Variation ID | Best For | Required Role Slots | Selection Point |
|---|---|---|---|
| `spawn_variation.front_line_basic` | First contact, patrol, readable intro | `front_basic` | `readable_intro`, `front`, low difficulty |
| `spawn_variation.front_then_backline` | Melee blockers plus ranged/support | `front_basic`, `backline_ranged` | `backline`, `pressure`, ranged monster pool exists |
| `spawn_variation.front_then_flank` | Ambush that remains fair | `front_basic`, `flank_active` | `ambush`, `tracking`, `delayed_ambush` |
| `spawn_variation.soft_surround_swarm` | Many weak enemies without elite spike | `swarm_basic`, `surround_pressure` | `surround`, `swarm`, normal or higher |
| `spawn_variation.objective_guard` | Cage, altar, gate, hut, exit defense | `objective_guard`, `front_basic`, optional `support_backline` | `objective`, `protect_objective`, `breakthrough` |
| `spawn_variation.reinforcement_escalation` | Enemies arrive as fight progresses | `front_basic`, `reinforcement_slot` | `reinforcement`, `escalating`, `attrition` |
| `spawn_variation.elite_anchor` | One leader/elite with fillers | `elite_anchor`, `filler_basic` | `elite`, `leader`, `normal_elite` |
| `spawn_variation.elevated_backline` | Watchtower or high-ground ranged pressure | `front_basic`, `elevated_ranged` | `elevated`, `backline`, `mountain_hideout` |
| `spawn_variation.boss_phase_adds` | Boss plus phase adds | `boss_anchor`, `add_wave` | `boss_phase`, `phase_change`, `summon_support` |
| `spawn_variation.boss_objective_pressure` | Boss tied to altar/rescue/choice pressure | `boss_anchor`, `objective_pressure`, optional `add_wave` | `boss_intro`, `objective`, `rescue_aftermath` |

## Selection Point Rules

When choosing a variation later, score it in this order:

1. Required `situationTags` match the battle context.
2. Required `spaceTags` match the battle space.
3. Required `rhythmTags` match the intended pacing.
4. Required role slots can be filled by the selected monster pool.
5. Difficulty tags match the battle difficulty.
6. Avoid tags do not conflict with the context.
7. Fairness rules can be satisfied by available space and delays.

Do not select a variation only because its name sounds similar to the story. It must have fillable role slots.

## Balance Knob Rules

Define balance as adjustable ranges, not final numbers.

Use these knobs:

| Knob | Values | Meaning |
|---|---|---|
| baseThreat | `1..10` | Overall pressure before battle-specific scaling. |
| density | `low`, `medium`, `high` | How many enemies can be active together. |
| overlap | `none`, `low`, `medium`, `high` | How much next pressure overlaps current enemies. |
| eliteAllowance | `none`, `one`, `one_or_more`, `boss_only` | Whether elites can fill slots. |
| rangedAllowance | `none`, `optional`, `required`, `limited` | Whether ranged pressure is expected. |
| surroundAllowance | `none`, `soft`, `heavy` | Surround pressure budget. |
| recommendedDurationSec | range | Expected encounter length before tuning. |

Concrete counts, delays, and `max alive` values are decided during final spawn JSON generation.

## Battle Connection Contract

Every variation must preserve these handoff concepts:

```text
selectedVariationId
selectionTags
roleSlots
sequenceShape
balanceKnobs
expectedSequenceIdPattern
```

`BattleStoryContext` should later carry:

```text
battleStoryContextId
monsterContextRef
monsterCompositionRef
requiredMonsterTags
preferredMonsterTags
forbiddenMonsterTags
requiredSpawnTags
forbiddenSpawnTags
```

The spawner selection step should produce:

```text
encounterProfileId
selectedVariationId
monsterGroupId
roleBindings
spawnSequenceId
```

`BattleSO` should only reference the final `spawnSequenceId`.

## Validation

Before saving a variation profile, check:

- `variationId` is stable and unique.
- Tags use the same vocabulary as `BattleStoryContextGuide.md`.
- No concrete `characterId` is required by the profile.
- Every required `roleSlot` has accepted monster tags.
- `sequenceShape` references only defined role slots.
- Avoid tags are not the same as required selection tags.
- Balance knobs are ranges or categories, not one battle's final tuned values.
- The profile can be reused by multiple battles.
- The profile can later be converted into `SpawnSO.md` JSON.

## Agent Checklist

- Read `BattleStoryContextGuide.md` for tag vocabulary.
- Read `SpawnSO.md` for eventual runtime constraints.
- Create reusable `SpawnVariationProfile` entries first.
- Do not bind monsters while authoring the independent variation.
- Do not create `BattleSO` fields in the variation profile.
- Preserve `selectedVariationId` for future battle mapping.
- Include enough `roleSlots` and `selectionTags` for an agent to choose the variation later.
