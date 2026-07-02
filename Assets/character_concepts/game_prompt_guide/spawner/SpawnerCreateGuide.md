# Spawner Create Guide

## Purpose

This document defines the agent workflow for creating spawn JSON used by battle encounters.

Use this guide before writing `BattleSO` JSON.

Pipeline:

```text
Encounter spawn intent
  -> Monster pool analysis
  -> Spawn variation selection
  -> Monster binding
  -> Battle-specific sequence assembly
  -> All-in-one spawn JSON
  -> Spawn SO generation
```

## Global Rules

- Use all-in-one JSON unless the task explicitly asks for split files.
- Keep spawner JSON focused on spawning.
- Do not include battle rewards, battle background, relic chances, or victory rules.
- Do not include character stat definitions.
- Use exact enum values from `SpawnSO.md`.
- Create the main `SpawnSequenceSO` before authoring `BattleSO`.
- Prefer reusable spawn variations over one-off battle-specific layouts.
- Treat monster composition and spawn variation as separate authoring layers.

## Spawn Variation Architecture

### Purpose

Build diverse battle spawning by selecting from independent spawn variations and binding battle-specific monster pools into them.

Do not make every battle invent its own spawn structure from scratch.

Use this structure:

```text
Story / battle intent
  -> Monster pool
  -> Spawn variation candidates
  -> Monster binding into variation roles
  -> Generated battle sequence
```

### Authoring Layers

| Layer | Owns | Should Be Reusable |
|---|---|---:|
| Monster Pool | Which monsters can appear | Yes, across battles in the same faction/area |
| Spawn Variation | Timing, pressure direction, pattern shape, wave rhythm | Yes, across many battles |
| Monster Binding | Which monster fills each variation role | Battle-specific |
| Battle Sequence | Final `SpawnSequenceSO` selected by BattleSO | Battle-specific |

### Spawn Variation Definition

A spawn variation is an abstract encounter pattern.

It should describe:

- Pressure direction: front, flank, back, surround, random, objective
- Rhythm: single wave, escalation, ambush, loop, boss phase
- Role slots: front pressure, backline pressure, diver, support, swarm, boss
- Timing: step order, start delays, completion modes
- Difficulty budget: expected count, density, overlap, elite/boss allowance

It should not hard-code one specific monster unless the variation is intentionally monster-specific.

### Variation Examples

| Variation | Intent | Role Slots |
|---|---|---|
| `variation.front_line_basic` | Simple readable front pressure | front_basic |
| `variation.front_then_backline` | Melee front followed by ranged pressure | front_basic, backline_ranged |
| `variation.front_then_flank` | Player reads front, then reacts to side pressure | front_basic, flank_active |
| `variation.surround_swarm` | High pressure swarm moment | swarm_basic, surround_pressure |
| `variation.objective_pressure` | Enemies move around a prop/objective | objective_attacker, support_or_guard |
| `variation.elite_anchor` | One elite anchors weaker enemies | elite_anchor, filler_basic |
| `variation.boss_phase_adds` | Boss with add waves | boss_anchor, add_wave |

### Monster Binding

After selecting a variation, bind monsters from the monster pool into role slots.

Example:

```text
variation.front_then_backline
  front_basic      -> character.wolf.easy.1
  backline_ranged  -> character.archer_spirit.easy.1
```

Binding rules:

- A role slot may accept multiple candidate monsters.
- Prefer monsters whose skill slot profile matches the role.
- If no monster matches a role, choose another variation or simplify the variation.
- Do not force a monster into a variation role that contradicts its skill slots.

### Balance Expansion

To scale difficulty, prefer changing these in order:

1. Quantity
2. Spawn delay / slot interval
3. Number of repeated formations
4. Step overlap
5. Elite replacement
6. Back/flank/surround pressure
7. Boss or active_2 profile

Avoid jumping directly from simple front pressure to surround + elite + overlap in one step.

## Step 1. Read Source Intent

### Purpose

Understand the desired battle pacing and enemy composition.

### Main Work

1. Identify story and battle intent.
2. Identify available monster pool.
3. Identify target difficulty and expected duration.
4. Select candidate spawn variations.
5. Bind monster pool entries into variation role slots.
6. Decide whether the final sequence is one-shot or looping.

### Validation

- Every enemy has a `CharacterSO.characterId`.
- Every selected variation can be filled by the monster pool.
- The intended encounter can be expressed as reusable variation steps.
- Repeated or large groups are represented by formations instead of duplicating many sequence steps.

## Step 1-A. Monster Spawn Role Mapping

### Purpose

Connect each monster's combat style to a meaningful spawn side, distance, pattern, and sequence behavior.

Spawning should express how the monster fights. Do not place monsters only by count.

### Skill Slot Source

Use the character skill slot model as the primary vocabulary.

Character-owned skills use these slot keys:

```text
basic_attack
active_1
active_2
active_3
passive_1
```

Authoring guides also use these design names:

```text
BasicAttack -> basic_attack
Active1     -> active_1
Active2     -> active_2
Passive     -> passive_1
Passive1    -> passive_1
Passive2    -> passive_2, when explicitly supported by the source design
```

NPC slot availability is limited by tier and grade:

| NPC Type | Typical Slots | Spawn Reading |
|---|---|---|
| Normal grade 1 | Basic only | Simple, readable pressure. Use front or mild flank. |
| Normal grade 2 | Basic + one identity skill | Add one spatial hook from `active_1` or `passive_1`. |
| Normal grade 3 | Basic + one or two identity skills | Use mixed squads or delayed pressure if needed. |
| Elite / leader | Basic + Active1 + Passive | Can anchor a wave or support other units. |
| Boss | Basic + Passive + Active1 + Active2 | Should usually have dedicated sequence steps or phases. |

### Skill Slot Spawn Meaning

Map spawn decisions from slots before using free-form tactical labels.

| Slot | Combat Meaning | Spawn Meaning |
|---|---|---|
| `basic_attack` | Repeated baseline pressure | Defines default engagement distance and facing. |
| `active_1` | Frequent identity action | Adds a readable pattern hook such as charge, shot, leap, area poke, summon, or small CC. |
| `active_2` | Main high-impact action | Use for elites/bosses or major wave threats; give distance, delay, or staging. |
| `active_3` | Very high-impact action | Boss-like only; avoid normal group spam. |
| `passive_1` | Always-on or conditional modifier | Changes grouping, support position, durability, aura spacing, or priority target. |
| `passive_2` | Secondary passive, when explicitly used | Treat as elite/boss complexity; avoid for normal NPCs. |

### Slot-Derived Spawn Keywords

Prefer these keywords because they describe what the skill slot does in combat:

| Keyword | Usually Comes From | Meaning For Spawning |
|---|---|---|
| `basic_melee` | `basic_attack` | Front or soft flank, close/mid distance. |
| `basic_ranged` | `basic_attack` | Backline or wide flank, spaced pattern. |
| `basic_area` | `basic_attack` with area hit | Avoid dense stacking; use spread or stagger. |
| `active_charge` | `active_1` / `active_2` | Needs lane or front/flank entry. |
| `active_jump` | `active_1` / `active_2` | Can justify flank/back entry with delay. |
| `active_projectile` | `active_1` / `active_2` | Backline or diagonal flank, spaced line. |
| `active_area` | `active_1` / `active_2` | Spread enemies; avoid unfair surround overlap. |
| `active_summon` | `active_1` / `active_2` | Back/protected spawn or delayed sequence step. |
| `active_cc` | `active_1` / `active_2` | Use lower density or staged timing. |
| `passive_tank` | `passive_1` | Front anchor, protects fragile roles. |
| `passive_aura` | `passive_1` | Spawn with allies inside aura spacing, not alone. |
| `passive_enrage` | `passive_1` | Avoid excessive simultaneous pressure. |
| `passive_support` | `passive_1` | Backline or second-line placement. |

### Monster Role Axes

Use these axes after reading the skill slots:

| Axis | Values | Spawn Meaning |
|---|---|---|
| Primary slot profile | `basic_attack`, `active_1`, `active_2`, `passive_1` | Determines the monster's visible combat hook. |
| Engagement range | `Melee`, `MidRange`, `LongRange` | Determines distance from player/frontline. |
| Pressure direction | `Front`, `Back`, `Flank`, `Surround`, `Center`, `Random` | Determines where threat enters from. |
| Mobility | `Slow`, `Normal`, `Fast`, `Teleport`, `Flying` | Determines spawn distance and warning time. |
| Durability | `Fragile`, `Normal`, `Tank`, `Boss` | Determines whether spawn can be close or should be staged. |
| Attack shape | `SingleTarget`, `Line`, `Cone`, `Area`, `Projectile`, `Summon`, `Support` | Determines spacing and formation shape. |
| Target preference | `Nearest`, `Backline`, `LowestHp`, `Structure`, `Random` | Determines whether front/back/flank placement matters. |
| Derived tactical role | `Blocker`, `Diver`, `Harasser`, `Artillery`, `Swarm`, `Support`, `Spawner`, `Objective` | Secondary label derived from slots and stats. |

### Spawn Side Guidelines

| Slot-Derived Profile | Preferred Spawn Side | Pattern Guidance |
|---|---|---|
| `basic_melee` | Front or soft flank | `FixedPosition` line, arc, or small cluster. |
| `basic_ranged` | Back or wide flank | Spread pattern, enough distance from player. |
| `basic_area` | Mid/front spread | Avoid dense stacking. Use wider slots or staggered spawn. |
| `active_charge` | Front lane or flank | Leave readable travel distance. |
| `active_jump` | Flank or back edge | Small group, delayed step, avoid instant unavoidable hit. |
| `active_projectile` | Backline or diagonal flank | Wide sparse line to avoid projectile stacking. |
| `active_area` | Far back, protected center, or staged side | Sparse formation, often after blockers. |
| `active_summon` | Back, protected, or objective-adjacent | Spawn after initial pressure or with guards. |
| `active_cc` | Front/mid with lower density | Avoid simultaneous surround unless intended. |
| `passive_tank` | Front center | Wide line or gate-like formation. |
| `passive_aura` | Center of allied group | Keep allies inside intended aura spacing. |
| `passive_support` | Behind tanks or mixed into second line | Formation with front blockers and rear support. |
| `passive_enrage` | Staged front or mid | Avoid excessive simultaneous pressure. |
| Boss slot profile | Center-front or staged arena anchor | Dedicated sequence step, usually with adds. |
| Objective profile | Side closest to target prop | Pattern should align with prop position. |

### Distance Guidelines

- Close spawn: use only for slow melee, durable blockers, or clearly telegraphed ambushes.
- Mid spawn: default for normal melee and mixed squads.
- Far spawn: use for ranged, artillery, support, bosses, or enemies with charge/cast preparation.
- Back spawn: use for divers, ambushes, objective pressure, or split-attention moments.
- Surround spawn: use sparingly; it raises difficulty sharply.

### Composition Guidelines

- Pair tanks with ranged/support behind them.
- Pair swarm with one durable or disruptive enemy only when difficulty should spike.
- Avoid spawning long-range artillery with no frontline unless the intent is a fragile glass-cannon wave.
- Avoid backline divers and front blockers in the same instant step unless the battle is meant to be a high-pressure check.
- Use `slotInterval` for readability when many enemies spawn together.
- Use separate sequence steps when the player should read one threat before the next arrives.

### Sequence Timing Guidelines

| Intent | Completion Mode | Timing |
|---|---|---|
| Continuous pressure | `AfterSpawnCompleted` | Next step can begin after spawn finishes. |
| Wave-by-wave clarity | `AfterSpawnedEnemiesDefeated` | Next step waits until enemies die. |
| Ambush | `AfterSpawnCompleted` | Use start delay after front pressure begins. |
| Boss phase | `AfterSpawnedEnemiesDefeated` | Usually waits for previous phase cleanup. |
| Survival loop | `AfterSpawnCompleted` | Often used with `Infinite`. |

### Agent Decision Process

For each monster group:

1. Identify available skill slots.
2. Read `basic_attack` to determine default range and facing.
3. Read `active_1` / `active_2` to determine the monster's visible threat hook.
4. Read `passive_1` to determine grouping, aura, tank, enrage, support, or objective behavior.
5. Derive tactical role from the slot profile.
6. Choose pressure direction.
7. Choose distance.
8. Choose pattern shape.
9. Choose whether the group is a squad or formation.
10. Choose sequence order and completion mode.
11. Check fairness: player must have time and space to react.

## Step 2. Pattern Design

### Purpose

Define local positions or random spawn areas.

### Rules

- Use `FixedPosition` for readable hand-authored layouts.
- Use `RangeRandom` for scattered enemy groups.
- Keep reusable patterns generic.
- Apply `rotation` and `scale` only when the pattern variant should be stored as a distinct preset.
- Name patterns by spatial intent when the position is tactically meaningful.
- Prefer front/back/flank/surround naming when the same monster should be reused in different pressure roles.

### Spatial Pattern Types

Use these naming concepts when authoring patterns:

| Spatial Intent | Suggested Pattern Shape | Example ID |
|---|---|---|
| Front line | Horizontal line or shallow arc | `pattern.forest.front.line.3` |
| Back ambush | Small cluster behind player side | `pattern.forest.back.cluster.2` |
| Left flank | Offset vertical line or cluster | `pattern.forest.flank_left.3` |
| Right flank | Offset vertical line or cluster | `pattern.forest.flank_right.3` |
| Surround | Circle or four-point fixed pattern | `pattern.forest.surround.4` |
| Artillery backline | Wide sparse back row | `pattern.forest.backline.spread.3` |
| Random swarm | Circle or rectangle random area | `pattern.forest.random.swarm.r4` |

### Output

Write entries under:

```json
"patterns": []
```

### Validation

- Fixed patterns have `positions`.
- Random patterns have `shape` and `areaSize`.
- IDs use `pattern.{group}.{shape_or_use}`.

## Step 3. Squad Design

### Purpose

Bind NPC IDs to patterns.

### Rules

- A squad is the smallest reusable spawn content unit.
- Use one group for simple spawns.
- Use multiple groups for mixed enemy squads.
- Use `quantity` mainly with random patterns.
- Use `slotInterval` to avoid all enemies appearing on the same frame.
- Put monsters with different tactical roles in separate groups when they need different offsets, timing, or patterns.
- Use group `order` to put blockers before ranged/support enemies inside the same squad.

### Output

Write entries under:

```json
"squads": []
```

### Validation

- `contentId` is unique.
- Every `npcId` exists.
- Every referenced `patternId` exists.
- `quantity` is at least `1`.

## Step 4. Formation Design

### Purpose

Repeat a squad over a larger pattern.

### Rules

- Use formations when the same squad should appear multiple times.
- Do not create a formation if a single squad is enough.
- `squadId` must reference a squad from the same JSON or an existing generated squad.
- `patternId` controls where each squad copy appears.

### Output

Write entries under:

```json
"formations": []
```

### Validation

- `squadId` exists.
- `patternId` exists when provided.
- `quantity` matches the intended number of squad copies for random formation patterns.

## Step 5. Sequence Design

### Purpose

Define battle spawn timing.

### Rules

- Use one main sequence per battle.
- Use `order` gaps such as `0, 10, 20`.
- Use `AfterSpawnCompleted` when the next step can begin after spawn commands finish.
- Use `AfterSpawnedEnemiesDefeated` when the next step should wait until spawned enemies are killed.
- Use `Infinite` only for survival or pressure-loop encounters.
- For `Infinite`, `loopStartOrder` must match an existing step.

### Output

Write entries under:

```json
"sequences": []
```

### Validation

- The main sequence has a stable `sequenceId`.
- Every `contentId` references a squad or formation.
- `startDelay` is not negative.
- `repeatMode` is `Once` or `Infinite`.
- `completionMode` is valid.

## Step 6. JSON Save Location

Recommended path:

```text
Assets/Scripts/battle_spawn/Resource/Jsons/{encounter_id}.spawn.json
```

Use the source preset as a reference:

```text
Assets/Scripts/battle_spawn/Resource/Jsons/presets_all_in_one.json
```

## Step 7. Generate Spawn SO Assets

Open Unity menu:

```text
BS/Spawn/Spawn SO FromJson Window
```

Set or confirm:

```text
NPC Pool Asset
Content Pool Asset
Pattern Pool Asset
Base Output Folder
```

Then use:

```text
Bake All from Single JSON
```

The generator creates or updates:

```text
Patterns
SpawnContents/Squads
SpawnContents/Formations
Sequences
```

## Output Contract For Battle JSON

After generation, pass the main sequence ID to battle JSON:

```json
{
  "spawnSequenceId": "seq.forest.001.main"
}
```

Do not duplicate sequence steps in battle JSON.

## Agent Checklist

- Read `SpawnSO.md`.
- Confirm NPC IDs.
- Generate all-in-one JSON.
- Validate references in memory before writing the file.
- Ensure the main sequence ID is clear for `BattleSO`.
- Keep the JSON minimal and readable.
- Do not write legacy wave spawner data.
