

# Character Stat Balance Guide

## Purpose

This document defines the core stat balance rules used when generating character base stats.

The system is based on a **Total Score** model. Character stats are not authored directly. Instead, a character is defined by:

- Total Score
- Growth Type (Attack / Defense / Utility multipliers)
- Grade

The actual stat values are generated from these inputs.

---

# Standard Character

A Standard character with **Score 100** represents the baseline used for balancing every other character.

## Standard Stats

| Stat | Value |
|------|------:|
| Attack | 20 |
| Defense | 20 (≈10% damage reduction reference) |
| Critical Chance | 10% |
| Critical Damage | +50% Attack Damage |
| Attack Speed | 1.0 |
| Max HP | 500 |
| HP Regen | 5 / sec |
| Move Speed | 1.0 |

These values are balancing references only.

## Speed Scale Standard

Movement speed and attack speed use `1.0` as the normal baseline.

| Scale | Meaning | Use |
|------:|---------|-----|
| 0.50 | Very slow | Heavy blockers, rooted casters, slow elites |
| 0.75 | Slow | Shield units, heavy bruisers, cautious ranged units |
| 1.00 | Normal | Standard player and standard NPC movement/attack speed |
| 1.50 | Fast | Pursuers, agile beasts, skirmishers |
| 2.00 | Very fast | Explicitly speed-focused rushers or special burst phases |

Use these values directly for `Move Speed` and `Attack Speed` intent. Avoid using older baselines where normal movement was `1.5` or normal attack speed was `2`.


---

# Long-Term Balance Philosophy

Character HP persists between battles.

Balance is designed around approximately **30 consecutive battles**, not a single encounter.

As a result:

- Max HP has higher value than traditional RPGs.
- HP Regen has significant long-term value.
- Recovery effects should be evaluated across multiple battles.
- Characters are expected to gradually lose HP over a run.

---

# NPC Hack-And-Slash Balance Override

Use this override when the game mode is a monster-heavy hack-and-slash encounter where many NPC enemies can appear at the same time.

This rule applies to `characterType: "Npc"` encounter enemies unless a later document explicitly marks an enemy as a solo elite or boss exception.

## NPC Regen Rule

NPC enemies must not own HP Regen as a generated stat.

- Do not give NPC planning data an `hp_regen` stat bias.
- Do not generate NPC base HP Regen values.
- Do not use HP Regen as a hidden compensation for low HP.
- If a special enemy needs recovery, design it as an explicit skill, phase mechanic, or encounter gimmick instead of a base stat.

## Multi-Monster Damage And Health Scale

For hack-and-slash mob encounters, start from the normal score model, then apply these NPC encounter multipliers:

| NPC Budget | Multiplier | Purpose |
|------------|-----------:|---------|
| Attack score / direct skill damage | 0.50 | Many enemies can attack together, so each enemy's individual damage must be lower. |
| Health score / Max HP budget | 0.25 | Enemies should die quickly enough to support dense combat and fast clear rhythm. |
| HP Regen | 0 | NPC enemies do not regenerate by default. |

Keep defense, range, speed, support, and utility scores as role identity values unless the encounter itself is still over budget.

When updating existing NPC planning JSON:

1. Multiply `planningScore.attack` by `0.50`.
2. Multiply `planningScore.health` by `0.25`.
3. Recalculate `planningScore.overall` and `stats.totalScore` from the adjusted planning budget.
4. Remove `hp_regen` from NPC `stats.statBias`.
5. Multiply direct NPC skill damage intent by `0.50`.
6. Keep pure utility skills readable, but do not use HP Regen as their utility.

Bosses may use separate boss-specific rules, but do not inherit NPC HP Regen by default.

---

# Score System

Every character owns a Total Score.

Example:

- Score 100
- Score 150
- Score 250
- Score 500

The score represents the overall combat budget before being distributed.

---

# Growth Axes

Character growth is divided into three independent axes.

- Attack
- Defense
- Utility

Each type applies different multipliers to these axes.

Example:

| Type | Attack | Defense | Utility |
|------|-------:|--------:|--------:|
| Standard | 1.0 | 1.0 | 1.0 |
| Offense | 1.4 | 0.8 | 0.7 |
| Defense | 0.8 | 1.4 | 0.8 |
| Mobile | 0.9 | 0.8 | 1.6 |

Final Axis Score:

AxisScore = TotalScore × AxisMultiplier

---

# Current Growth Policy

Not every stat scales automatically.

## Default Growth Stats

These stats are intended to scale automatically from Score.

- Attack
- Max HP
- HP Regen

## Special Growth Stats

The following stats are considered special mechanics.

They normally remain at their baseline values and only grow for specific characters, equipment, relics, blessings or skills.

- Defense
- Critical Chance
- Critical Damage
- Attack Speed
- Move Speed

These are intentionally excluded from the standard growth calculation.

---

# Future Formula

The final implementation should follow this flow:

1. Total Score
2. Apply Type Multipliers
3. Calculate Attack / Defense / Utility Scores
4. Convert each axis into actual stat values using growth curves
5. Apply special modifiers (equipment, relics, skills, blessings, passive effects)

This keeps every character consistent while allowing unique archetypes through axis multipliers and special stat growth.

---

# Tag System

Tags define species, tribe, faction, body type, or group identity.

A Type defines the combat role.

A Tag defines the creature flavor.

Examples:

- Wolf = higher Move Speed, pursuit identity
- Orc = higher Max HP, lower Move Speed
- Spirit = lower physical durability, higher Utility or recovery identity

Tags must not be applied as a flat bonus to every Type.

The same Tag should react differently depending on Type.

Examples:

| Combination | Result |
|-------------|--------|
| Defense + Orc | Max HP focused tank |
| Melee DPS + Orc | Bruiser with slightly higher HP |
| Ranged DPS + Orc | Slower but harder-to-kill ranged unit |
| Melee DPS + Wolf | Fast pursuit attacker |
| Defense + Wolf | Fast guard or interceptor, not a pure tank |

---

# Tag Multipliers

Each Tag has two layers.

1. Base Tag Multiples
2. Type + Tag Override Multiples

Base Tag Multiples define the general identity of a tag.

Type + Tag Override Multiples define how strongly the tag applies to a specific combat role.

This prevents every Orc from becoming the same HP-heavy unit and every Wolf from becoming the same fast unit.

---

# Current Tag Definitions

The current tag set represents species, tribe, or high-level group identity.

Use these tag ids in character design data.

| Tag ID | Identity | Main Bias |
|--------|----------|-----------|
| wolf | Mobility, pursuit, fast engage/disengage | Higher Utility and Move Speed, lower durability |
| orc | High HP, low mobility, pressure through durability | Higher Defense/Max HP, lower Utility and Move Speed |
| spirit | Low physical durability, high utility/recovery/specialness | Higher Utility and HP Regen, lower Max HP and Defense |
| human | Standard body, equipment mastery, few clear weaknesses | Slight Attack/Crit bias, mostly neutral |
| undead | High HP, low speed, attrition and curse/infection identity | Higher Max HP/Defense, lower HP Regen, speed, and crit |
| dokkaebi | High aggression, supernatural individuality | Higher Attack/Crit Damage, variant-specific defense or ranged magic |
| divinity | Superior entity with command, summon, or support power | Higher overall budget, strong Utility and sustain |

## Base Tag Multiples

Base tag multiples define the default species or group body profile before Type+Tag overrides.

| Tag ID | Attack Axis | Defense Axis | Utility Axis | Attack | Max HP | HP Regen | Defense | Crit Chance | Crit Damage | Attack Speed | Move Speed |
|--------|------------:|-------------:|-------------:|-------:|-------:|---------:|--------:|------------:|------------:|-------------:|-----------:|
| wolf | 1.03 | 0.90 | 1.18 | 1.02 | 0.90 | 0.95 | 0.90 | 1.08 | 1.00 | 1.05 | 1.18 |
| orc | 0.98 | 1.20 | 0.88 | 1.02 | 1.22 | 1.05 | 1.08 | 0.90 | 1.00 | 0.95 | 0.88 |
| spirit | 0.95 | 0.82 | 1.28 | 0.95 | 0.82 | 1.22 | 0.80 | 1.00 | 1.00 | 1.00 | 1.08 |
| human | 1.02 | 1.00 | 1.00 | 1.03 | 1.00 | 1.00 | 1.00 | 1.05 | 1.00 | 1.00 | 1.00 |
| undead | 0.98 | 1.12 | 0.95 | 0.98 | 1.15 | 0.90 | 1.08 | 0.90 | 1.00 | 0.95 | 0.88 |
| dokkaebi | 1.08 | 1.02 | 1.03 | 1.08 | 1.02 | 1.00 | 1.02 | 1.02 | 1.08 | 1.00 | 0.98 |
| divinity | 1.10 | 1.08 | 1.18 | 1.10 | 1.12 | 1.15 | 1.08 | 1.05 | 1.10 | 1.00 | 1.00 |

## Type + Tag Override Roles

Type+Tag overrides should be applied only for combinations that have a clear gameplay identity.

Do not invent a full override table for every possible Type+Tag pair.

Use the following current override roles:

| Tag ID | Type | Override Role |
|--------|------|---------------|
| wolf | melee_dps | Fast pursuit attacker that bites in and out quickly |
| wolf | defense | Fast guard/interceptor rather than a pure tank |
| wolf | ranged_dps | Ranged pursuer with strong repositioning |
| orc | defense | Max HP focused tank |
| orc | melee_dps | Bruiser with slightly higher HP |
| orc | ranged_dps | Slow but hard-to-kill thrower/shooter |
| orc | support | Rear support that survives longer than usual |
| spirit | support | Recovery/support-specialized spirit |
| spirit | ranged_dps | Fragile ranged unit with special firepower |
| human | melee_dps | Weapon-trained melee fighter |
| human | ranged_dps | Marksmanship or sniping specialist |
| human | support | Medicine or ritual-based support |
| undead | defense | Slow but hard-to-collapse guardian |
| undead | melee_dps | Attrition-based infected melee unit |
| undead | support | Curse/infection amplifier rather than pure healer |
| dokkaebi | defense | Stone/brute-force frontline defender |
| dokkaebi | ranged_dps | Will-o-wisp or trick-based chain attacker |
| divinity | ranged_dps | Superior caster with command or summon identity |
| divinity | support | Strong follower buff or recovery support |

If a character needs a tag not listed here, add the tag identity, base tag multiples, and any Type+Tag overrides to this guide before using it in generated stat data.

---

# Tag Formula

Apply stat calculation in this order:

1. Select Total Score and Grade
2. Apply Type Multipliers
3. Apply Grade growth
4. Apply Base Tag Multipliers
5. Apply Type + Tag Override Multipliers
6. Clamp special stats if needed

## Axis Formula

```text
TaggedAxisScore = TotalScore
                × TypeAxisMultiplier
                × TagAxisMultiplier
                × TypeTagAxisOverride
```

## Default Growth Stat Formula

Default Growth Stats are:

- Attack
- Max HP
- HP Regen

```text
Value = Score100Value
      × RealizedGrowthTarget(Grade)
      × TypeAxisMultiplier
      × TagAxisMultiplier
      × TypeTagAxisOverride
      × TypeStatMultiplier
      × TagStatMultiplier
      × TypeTagStatOverride
```

## Special Growth Stat Formula

Special Growth Stats are:

- Defense
- Critical Chance
- Critical Damage
- Attack Speed
- Move Speed

```text
Value = Score100Value
      × GradeStatMultiplier(Grade)
      × TypeStatMultiplier
      × TagStatMultiplier
      × TypeTagStatOverride
```

Special Growth Stats do not automatically receive the full Score scaling.

They only grow through Type, Tag, Type + Tag Override, equipment, relics, blessings, skills, or passive effects.

---

# Tag Stacking Rules

A character should normally have one primary Tag.

A character may have a second Tag only when the design strongly needs it.

Recommended limit:

| Tag Slot | Weight |
|----------|-------:|
| Primary Tag | 1.0 |
| Secondary Tag | 0.5 |

Secondary Tag formula:

```text
SecondaryTagMultiplier = 1 + (TagMultiplier - 1) × 0.5
```

This keeps tags from overpowering Type identity.

---

# Recommended Special Stat Clamps

| Stat | Min | Max |
|------|----:|----:|
| Move Speed | 0.50 | 2.00 |
| Attack Speed | 0.50 | 2.00 |
| Critical Chance | 0 | 40 |
| Defense | 0 | 80 |

Clamp values are first-pass balance guards.

They can be changed after playtesting.

---

# Tag Examples

## Orc + Defense

Purpose:

- Max HP focused tank
- Strong long-term durability
- Low mobility

```text
MaxHP = 500
      × RealizedGrowthTarget
      × DefenseTypeAxis
      × OrcTagDefenseAxis
      × OrcDefenseOverride
      × DefenseTypeMaxHpStat
      × OrcTagMaxHpStat
      × OrcDefenseMaxHpOverride
```

## Orc + Melee DPS

Purpose:

- Bruiser
- Higher HP than a normal melee DPS
- Still primarily an attacker, not a pure tank

## Wolf + Melee DPS

Purpose:

- Fast pursuit attacker
- Better chase and engage speed
- Lower durability than a stable frontline unit


