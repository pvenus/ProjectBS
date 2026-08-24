# CharacterSO Guide

## Required References

Read and apply these first:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
```

The master concept governs character identity and design. The content folder
guide governs JSON and generated SO storage.

## Purpose

Define the canonical JSON input and editor generation contract for
`CharacterSO`. JSON is authored data; CharacterSO and referenced skill SOs are
generated data and must not be hand-authored in a `json` folder.

## Canonical Storage

```text
Assets/Contents/Character/json/{characterId}.json
Assets/Contents/Character/so/{safeCharacterId}.asset
```

The generator replaces dots, slashes, and spaces with underscores in the asset
filename:

```text
Assets/Contents/Character/json/character.seojin.1.json
Assets/Contents/Character/so/character_seojin_1.asset
```

Legacy data under `Assets/Resources` is migration input. Do not create new
canonical character JSON there.

## JSON Contract

```json
{
  "characterId": "character.seojin.1",
  "name": "서진",
  "characterType": "Player",
  "job": "SoldierBase",
  "baseStats": [
    { "statType": "Attack", "value": 10 },
    { "statType": "MaxHp", "value": 100 },
    { "statType": "AttackSpeed", "value": 1 },
    { "statType": "CritChance", "value": 10 },
    { "statType": "CritDamage", "value": 50 },
    { "statType": "MoveSpeed", "value": 1 }
  ]
}
```

| Field | Required | Rule |
|---|---|---|
| `characterId` | Yes | `character.{character_name}.{grade}` |
| `name` | Optional metadata | Korean display/planning name; localization remains separately owned |
| `characterType` | Yes | `Player`, `Npc`, or `Boss` |
| `job` | Yes | Exact `CharacterJob` enum name |
| `baseStats` | Yes | Only supported `StatType` values |

Do not write these legacy or generated fields:

```text
animationClips
skills
localization
animationOverrideSet
skillOverrideSet
prefabName
```

The current CharacterSO stores animation and skill entries directly. It does
not consume legacy `AnimationOverrideSetSO` or `SkillPoolOverrideSO` data.

## ID and Enum Rules

`characterId` always uses the `character` domain. `Player`, `Npc`, and `Boss`
are only `characterType` values.

Supported jobs are exact `CharacterJob` enum values:

```text
SoldierBase, SoldierFirst, SoldierSecond, SoldierAltFirst, SoldierAltSecond
ArcherBase, ArcherFirst, ArcherSecond, ArcherAltFirst, ArcherAltSecond
ScholarBase, ScholarFirst, ScholarSecond, ScholarAltFirst, ScholarAltSecond
PhysicianBase, PhysicianFirst, PhysicianSecond, PhysicianAltFirst, PhysicianAltSecond
MonkBase, MonkFirst, MonkSecond, MonkAltFirst, MonkAltSecond
```

## Animation Generation and Mapping

Canonical source frames:

```text
Assets/ImagesGenerated/Character/animation/
  {characterId}/
    {animationFolder}/
      frame-0.png
      frame-1.png
```

`animationFolder` must contain one supported action token:

```text
idle
movement or move
attack
death
```

Only Sprite assets named `frame-{number}` or `frame_{number}` are used. Preview
GIFs and unrelated files are excluded. Frames are sorted by numeric suffix.

Generated clips are stored under:

```text
Assets/AnimationClips/Character
```

The source artwork faces right. For each action folder the builder creates:

```text
{characterId}.{animationName}.Right.anim  # flipX = false
{characterId}.{animationName}.Left.anim   # flipX = true
```

Existing clips are updated in place so their Unity GUIDs remain stable.
`CharacterJsonGenerator` searches by character ID, action, and side. If no
matching clip exists, it runs `CharacterClipBuilder` automatically and searches
again.

Current source packages do not distinguish Up and Down, so corresponding
Up/Down enum entries share the same side clip. Missing actions are omitted; the
generator must not invent missing frames.

## Skill Generation and Mapping

Canonical skill roots:

```text
Assets/Contents/Skill/json/{equipmentId}.json
Assets/Contents/Skill/so/{generatedAsset}.asset
```

For `character.seojin.1`, the generator searches skill JSON IDs beginning with:

```text
skill.character.seojin.1.
```

The complete contract is:

```text
skill.character.{character_name}.{grade}.{slot}.{skill_name}
```

Example:

```text
skill.character.seojin.1.active_1.charge
```

The slot is the first segment following the complete character ID. Each matching
JSON is passed to `EquipmentSkillJsonGenerator`; its primary and supporting SOs
are created or updated in `Assets/Contents/Skill/so`, then assigned directly to
`CharacterSO.skills`. Missing skill JSON produces no entry.

Every child ID must derive from the full `equipmentId`. Generic IDs such as
`basic_attack_cast` are prohibited because the flat SO folder would collide.

## Editor Generation Procedure

1. Validate character JSON and matching skill JSON files.
2. Select one character JSON or the canonical Character `json` folder.
3. Run the CharacterSO generation menu.
4. Generate missing character clips automatically.
5. Generate or update matching skill SOs under `Assets/Contents/Skill/so`.
6. Generate or update CharacterSO under `Assets/Contents/Character/so`.
7. Verify non-null animation and skill entries.

## Validation

- JSON exists only in the canonical `json` folder.
- Generated assets exist only in the corresponding `so` folder.
- JSON filename equals `{characterId}.json`.
- No override-set or relative `skillJson` path is present.
- Every skill ID begins with `skill.{characterId}.`.
- Every supporting skill ID derives from its complete `equipmentId`.
- Frames follow the nested folder and numeric `frame` filename contract.
- Left clips use `flipX = true`; right clips use `flipX = false`.
- Existing generated asset GUIDs are preserved on update.
