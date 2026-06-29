# Skill JSON Guide

## Common Rules

- Only write fields that are required or actually used.
- Do not write optional profiles when they are not needed.
- If an optional profile is omitted, the builder keeps existing SO values or uses runtime defaults.
- Prefer the simplest JSON that fully describes the skill.

## ID Rules

### Skill IDs

Skill IDs use the following format:

```text
skill.{domain}.{character_name}.{grade}.{slot}.{skill_name}
```

Use this ID as the `equipmentId` value and as the base for child SO IDs.

`domain` identifies who owns or uses the skill.

```text
character
npc
```

For player character skills, use `character`.

For monster or NPC skills, use `npc`.

`character_name` is the design character id, such as `military_officer`.

`grade` is the character or skill grade number.

`slot` must come from the source design JSON. Do not infer it only from the skill file name.

Use the following mapping unless the design JSON explicitly defines a more specific slot:

| Design value | Slot |
|--------------|------|
| BasicAttack | basic_attack |
| Active1 | active_1 |
| Active2 | active_2 |
| Passive | passive_1 |
| Passive1 | passive_1 |
| Passive2 | passive_2 |

Known slot values:

```text
basic_attack
active_1
active_2
passive_1
passive_2
```

Examples:

```text
skill.character.military_officer.1.active_1.charge
skill.character.military_officer.1.basic_attack.frontline_slash
skill.character.military_officer.1.passive_1.unyielding_will
skill.npc.monster.1.basic_attack.melee_attack
```

Do not include SO names, folder names, generated asset type names, or builder implementation names in the skill ID.

Child SO IDs are derived from the skill ID:

```text
baseProfileId  = {equipmentId}.profile
castId         = {equipmentId}.cast
hitId          = {equipmentId}.hit
moveId         = {equipmentId}.move
visualId       = {equipmentId}.visual
upgradeTableId = {equipmentId}.upgrade
```

### Effect IDs

Effect IDs use the following format:

```text
effect.{source_id}
```

`source_id` is composed as:

```text
{so_name}.{category_name}.{grade}.{effect_name}
```
Example:
effect.common.buff.1.attack_up

- Do not use `/` in IDs.
- IDs are identifiers, not file paths.

## Optional Profiles

Optional profiles are only written when additional behavior is required.
