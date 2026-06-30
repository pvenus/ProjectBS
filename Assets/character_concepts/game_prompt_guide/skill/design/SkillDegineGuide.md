# Skill Design Guide

## Overview
This document defines the rules for designing character skills.

The generated skill design must be consistent with the character concept, combat role, grade progression, and the game's balance philosophy.

---

# Skill Domain Naming

All character-owned skills use the `skill.character` domain, regardless of whether the owner is a Player, Npc, or Boss.

NPC skill rules below define composition, upgrade availability, and balance only. They do not change the runtime skill ID domain to `skill.npc`.

Use:

```text
skill.character.{character_name}.{grade}.{slot}.{skill_name}
```

Do not use:

```text
skill.npc.{character_name}.{grade}.{slot}.{skill_name}
```

---

# Player Skill Structure

## Grade 1
- Basic Attack
- Passive

## Grade 2
- Basic Attack
- Passive
- Active Skill 1

## Grade 3
- Basic Attack
- Passive
- Active Skill 1
- Active Skill 2

## Grade Progression Rules
- Higher grades inherit all skills from lower grades.
- Existing skills become stronger at higher grades.
- New Active Skills are unlocked when reaching a higher grade.
- Basic Attack, Passive, and Active Skill 1 should receive stronger values or improved functionality in higher grades.

---

# NPC Skill Structure

## Normal
- Basic Attack

## Rare
- Basic Attack
- Active Skill 1

## Elite
- Basic Attack
- Passive
- Active Skill 1

## Boss
- Basic Attack
- Passive
- Active Skill 1
- Active Skill 2

## NPC Rules
- NPCs do not have skill upgrades.
- Passive Skills are available only for Elite and Boss enemies.
- Active Skill 2 is reserved for Boss enemies.

---

# Player Upgrade Rules

Skill upgrades apply only to Player characters.

## Maximum Level
- Grade 1: Level 5
- Grade 2: Level 10
- Grade 3: Level 15

## Upgrade Inheritance
- Grade 2 includes every upgrade from Grade 1.
- Grade 3 includes every upgrade from Grades 1 and 2.

Players effectively use:
- Grade 1: Level 1–5
- Grade 2: Level 1–10
- Grade 3: Level 1–15

---

# Upgrade Design Rules

Upgrades should strengthen existing skills rather than introduce entirely new mechanics.

Preferred upgrade targets:
- Base Damage
- Attack Scaling
- Cooldown
- Cast Range
- Hit Range
- Hit Count
- Effect Value
- Effect Duration
- Trigger Radius

Higher upgrade levels should focus on improving the character's identity rather than providing only raw numerical increases.

---

# General Skill Design Rules

- Basic Attack should emphasize consistency and repeated damage.
- Active Skills should define the character's unique combat identity.
- Passive Skills should reinforce the character's intended role.
- Utility-heavy skills should deal less damage.
- Skills with difficult activation conditions may receive stronger effects.
- Every skill should have a clear combat purpose.
- Avoid creating redundant skills with overlapping functionality.
