

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
| Attack Speed | 2 |
| Max HP | 500 |
| HP Regen | 5 / sec |
| Move Speed | 1.5 |

These values are balancing references only.

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
| Move Speed | 0.75 | 2.4 |
| Attack Speed | 0.6 | 3.2 |
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

