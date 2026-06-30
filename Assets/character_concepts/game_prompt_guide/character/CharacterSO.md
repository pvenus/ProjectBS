

# CharacterSO Guide

## Purpose

`CharacterSO` defines all static data required for a character.

A CharacterSO is generated from a character JSON file and automatically links animations, skills, and localized strings.

---

# Input

The generator requires a character JSON file.

Example:

```json
{
  "characterId": "character.military_officer.1",
  "name": "서진 (전위병)",
  "characterType": "Player",
  "job": "SoldierBase",
  "baseStats": [
    {
      "statType": "Attack",
      "value": 10
    },
    {
      "statType": "MaxHp",
      "value": 100
    },
    {
      "statType": "AttackSpeed",
      "value": 1
    },
    {
      "statType": "CritChance",
      "value": 10
    },
    {
      "statType": "CritDamage",
      "value": 50
    },
    {
      "statType": "MoveSpeed",
      "value": 1
    }
  ]
}
```

# CharacterType

Supported values:

```text
Player
Npc
Boss
```

---

# CharacterJob

The `job` field must match one of the `CharacterJob` enum values.

Base jobs:

```text
SoldierBase
ArcherBase
ScholarBase
PhysicianBase
MonkBase
```

Promotion jobs:

```text
SoldierFirst
SoldierSecond
SoldierAltFirst
SoldierAltSecond

ArcherFirst
ArcherSecond
ArcherAltFirst
ArcherAltSecond

ScholarFirst
ScholarSecond
ScholarAltFirst
ScholarAltSecond

PhysicianFirst
PhysicianSecond
PhysicianAltFirst
PhysicianAltSecond

MonkFirst
MonkSecond
MonkAltFirst
MonkAltSecond
```

The value is parsed directly using `Enum.Parse`, so it must exactly match the enum name.

---

---

# Generated Data

The generator populates the following fields.

| Field | Source |
|------|--------|
| characterId | JSON |
| characterType | JSON |
| job | JSON |
| baseStats | JSON |
| animationClips | Auto Generated |
| skills | Auto Generated |
| localization | Auto Generated |

---

# Animation Mapping

Animation clips are generated automatically.

The generator searches for sprites using the following naming convention:

```text
character.{characterId}.{AnimationClipType}.*.png
```

Example:

```text
character.seojin.IdleDownRight.frame_000.png
character.seojin.IdleDownRight.frame_001.png
```

Each `CharacterAnimationClipType` is converted into one `AnimationClip` and stored in the CharacterSO.

No animation information is required in the JSON.

---

# Skill Mapping

Skills are generated automatically.

The generator searches for skill JSON files matching:

```text
skill.character.{characterId}
```

Each matching JSON is converted into an `EquipmentSkillSO`.

The resulting skills are automatically assigned to the CharacterSO.

The slot key is extracted from the equipment id.

Example:

```text
skill.character.seojin.basic_attack.normal
```

Extracted slot:

```text
basic_attack
```

No skill information is required in the character JSON.

---

# Localization

Character names are automatically exported to the string table.

Main Key:

```text
character.{characterId}
```

Sub Key:

```text
name
```

---

# Output

The generator creates or updates:

```text
CharacterSO
EquipmentSkillSO
AnimationClip
Character String
```

---

# Summary

Generation flow:

```text
Character JSON
    ↓
Parse Character
    ↓
Generate AnimationClips
    ↓
Generate EquipmentSkillSO
    ↓
Generate Localization
    ↓
Update CharacterSO
```