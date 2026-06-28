# Skill JSON Guide

## Common Rules

- Only write fields that are required or actually used.
- Do not write optional profiles when they are not needed.
- If an optional profile is omitted, the builder keeps existing SO values or uses runtime defaults.
- Prefer the simplest JSON that fully describes the skill.

- Skill IDs use the following format:

```text
skill.{source_id}
```

`source_id` is composed as:

```text
{so_name}.{character_name}.{grade}.{skill_name}
```
Example:
skill.skill_temp.common.1.frontline_slash

- Effect IDs use the following format:

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