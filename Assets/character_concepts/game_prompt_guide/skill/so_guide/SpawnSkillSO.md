# SpawnSkillSO

## Structure

```json
{
  "spawnCount": 1,
  "spawnInterval": 0,
  "spawnLifeTime": 10,
  "characterSpawn": {
    "characterId": "summon.wolf"
  }
}
```

## Purpose

Defines how characters are spawned by a skill.

SpawnSkillSO stores how many characters are spawned and how long the spawned characters remain alive.

## Base Fields

| Name | Type | Description | Range |
|------|------|-------------|-------|
| spawnCount | int | Number of spawned characters | >= 1 |
| spawnInterval | float | Interval between spawns (seconds) | >= 0 |
| spawnLifeTime | float | Lifetime of the spawned character (seconds) | >= 0 |

## Optional Profiles

The following profile is optional.

- Character Spawn

If the profile is omitted, no character is spawned.

## Character Spawn

| Name | Type | Description | Range |
|------|------|-------------|-------|
| characterId | string | Character to spawn | Required |

## Validation Rules

```text
spawnCount        : >= 1
spawnInterval     : >= 0
spawnLifeTime     : >= 0
characterSpawn    : Optional.
characterId       : Required
```

## Notes

> SpawnSkillSO is used only for character spawning.
>
> Child skill spawning is configured in SkillHitSO.
>
> Character spawning is enabled only when the Character Spawn profile is provided.