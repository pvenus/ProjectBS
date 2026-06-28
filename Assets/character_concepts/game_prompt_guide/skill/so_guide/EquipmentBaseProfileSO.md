# EquipmentBaseProfileSO

## Structure

```json
{
  "baseProfileId": "skill.xxx.profile",
  "skillType": "",
  "skillComponentType": "",
  "projectileCount": 1,
  "projectileScale": 1,
  "projectileColliderRadius": 0.5,
  "projectileLifetime": 3,

  "projectile": {
    "arrangement": "",
    "arrangementValue": 0,
    "spreadAngle": 0,
    "radius": 0
  },

  "projectileSpawn": {
    "spawnOffset": 0,
    "interval": 0
  }
}
```

## Purpose

Defines the static configuration of a skill.

EquipmentBaseProfileSO owns the base projectile configuration, projectile arrangement, projectile spawn behavior, and other non-runtime skill properties.

## Base Fields

| Name | Type | Description | Range |
|------|------|-------------|-------|
| baseProfileId | string | Unique identifier of the EquipmentBaseProfileSO asset | Required |
| skillType | SkillType | Skill category | Required |
| skillComponentType | SkillComponentType | Skill component type | Required |
| projectileCount | int | Default projectile count | >= 1 |
| projectileScale | float | Projectile scale multiplier | > 0 |
| projectileColliderRadius | float | Projectile collider radius | > 0 |
| projectileLifetime | float | Projectile lifetime seconds | > 0 |

## Optional Profiles

The following profiles are optional.

- Projectile Arrangement Profile
- Projectile Spawn Profile
- Brain Meta Profile (Spec Out. Do not author in skill JSON.)

If an optional profile is omitted, the corresponding feature is not used.

## Projectile Arrangement Profile

Controls how multiple projectiles are arranged when spawned.

Use this profile only when projectileCount is greater than 1 or a special projectile pattern is required.

| Name | Type | Description | Range |
|------|------|-------------|-------|
| arrangement | ProjectileArrangementType | Projectile arrangement type | Required when projectile exists |
| arrangementValue | float | Arrangement helper value | >= 0 |
| spreadAngle | float | Projectile spread angle | >= 0 |
| radius | float | Arrangement radius | >= 0 |

## Projectile Spawn Profile

Controls where and how projectiles are spawned.

Use this profile when projectiles need a spawn offset or spawn delay.

| Name | Type | Description | Range |
|------|------|-------------|-------|
| spawnOffset | float | Spawn offset from caster | Any |
| interval | float | Spawn interval seconds | >= 0 |

## Brain Meta Profile (Spec Out)

Reserved for future AI skill-selection behavior.

Currently not implemented.

Do not include `brainMeta` in generated skill JSON until this profile is implemented.

| Name | Type | Description |
|------|------|-------------|
| category | BattleSkillCategory | AI skill category |
| targetType | BattleSkillTargetType | AI target type |
| tacticalNeed | BattleSkillTacticalNeed | AI tactical need |
| basePriority | float | AI base priority |

## Type Categories

### SkillType

- Active : Manually or automatically cast active skill
- Passive : Always-on or condition-triggered passive skill

### SkillComponentType

- Projectile : Projectile-based skill
- Spawn : Spawn-based skill

### ProjectileArrangementType

- Spread : Spread projectiles by angle
- Line : Arrange projectiles in a line
- Circle : Arrange projectiles in a circle

## Validation Rules

```text
baseProfileId                    : Required
skillType                         : Required enum
skillComponentType                : Required enum
projectileCount                   : >= 1
projectileScale                   : > 0
projectileColliderRadius          : > 0
projectileLifetime                : > 0
projectile.arrangement            : Required enum when projectile exists
projectile.arrangementValue       : >= 0
projectile.spreadAngle            : >= 0
projectile.radius                 : >= 0
projectileSpawn.interval          : >= 0
```

## Notes

> cooldown, cast time, range, cast move, and self effects are configured in SkillCastSO.
>
> damage, hit area, target layer, buffs, debuffs, and crowd-control effects are configured in HitSO / EffectSO.
>
> visual effects and icons are configured in VisualSetSO.
