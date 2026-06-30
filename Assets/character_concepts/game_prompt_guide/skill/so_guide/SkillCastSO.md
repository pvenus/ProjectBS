# SkillCastSO

## Structure

```json
{
  "castId": "",
  "targetingType": "AutoTarget",
  "cooldown": 1,
  "castTime": 0,
  "range": 5,
  "skipAttackAnimation": false,

  "burst": {
    "count": 1,
    "interval": 0
  },

  "castMove": {
    "moveType": "None",
    "distance": 0,
    "duration": 0
  },

  "selfEffects": []
}
```

## Purpose

Defines when and how a skill is cast.

SkillCastSO owns cast timing, target selection, cast-time movement, repeat casting, self effects applied at cast time, and attack-animation skip behavior.

## Base Fields

| Name | Type | Description | Range |
|------|------|-------------|-------|
| castId | string | Unique cast id | Required |
| targetingType | TargetingType | Target selection method | Required |
| cooldown | float | Cooldown seconds | >= 0 |
| castTime | float | Cast time seconds | >= 0 |
| range | float | Cast range | >= 0.4 |
| skipAttackAnimation | bool | Skip attack animation | true / false |

## Optional Profiles

The following profiles are optional.

Include them only when the skill requires the corresponding behavior.

### Burst Profile

Optional profile.

Used when the skill casts repeatedly from a single skill use.

| Name | Type | Description | Range |
|------|------|-------------|-------|
| count | int | Repeat count | >= 1 |
| interval | float | Repeat interval seconds | >= 0 |

### CastMove Profile

Optional profile.

Used when the caster moves during the cast.

| Name | Type | Description | Range |
|------|------|-------------|-------|
| moveType | CastMoveType | Cast movement type | Required when castMove exists |
| distance | float | Movement distance | >= 0 |
| duration | float | Movement duration seconds | >= 0 |

### Self Effects

Optional profile.

Used for `EffectEntrySO` objects applied to the caster at cast time.

See [`EffectEntrySO.md`](../../effect/EffectEntrySO.md) for the complete JSON schema and authoring guide.

| Name | Type | Description | Range |
|------|------|-------------|-------|
| selfEffects | array | Array of `EffectEntrySO` definitions applied to the caster. | 0+ |

## Type Categories

### TargetingType

- None : No targeting behavior.
- Self : Cast on the caster.
- AutoTarget : Use the selected target as the destination.
- AutoTargetDirection : Use the selected target only to determine the direction. The actual destination is the end of the skill range in that direction.
- Directional : Use the caster's current facing direction or input direction.
- Position : Use a specified world position as the destination.

| Type | Destination |
|------|-------------|
| AutoTarget | Actual target position |
| AutoTargetDirection | End of skill range in the target's direction |

### CastMoveType

- None : No movement
- DashForward : Dash forward from the caster's current facing or resolved cast direction
- DashBackward : Dash backward from the caster's current facing or resolved cast direction
- MoveToTarget : Move toward the resolved target
- MoveAwayFromTarget : Move away from the resolved target

## Validation Rules

```text
castId                : Required
targetingType         : Required enum
cooldown              : >= 0
castTime              : >= 0
range                 : >= 0.4
burst                 : Optional.
castMove              : Optional.
selfEffects           : Optional.
burst.count           : >= 1
burst.interval        : >= 0
castMove.moveType     : Required enum when castMove exists
castMove.distance     : >= 0
castMove.duration     : >= 0
```

## Notes

> `selfEffects` uses the `EffectEntrySO` JSON format.
>
> Damage, hit area, target layer, buffs, debuffs, and crowd-control effects are configured in HitSO / EffectSO.
>
> Projectile count, projectile arrangement, projectile spawn, and projectile lifetime are configured in EquipmentBaseProfileSO.
>
> Projectile movement behavior is configured in SkillMoveSO.
>
> Visual effects and icons are configured in VisualSetSO.
