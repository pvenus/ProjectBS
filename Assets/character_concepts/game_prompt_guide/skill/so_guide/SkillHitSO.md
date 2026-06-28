# SkillHitSO

## Structure

Example of a complete SkillHitSO JSON definition:

```json
{
  "hitId": "",
  "maxHitCount": 3,
  "ignoreSameRoot": true,
  "useRepeatInterval": true,
  "repeatInterval": 0.5,
  "hitStartTime": 0.2,
  "deactivateAfterFirstHit": false,
  "targetLayerMask": "Enemy",
  "damage": {
  },
  "buffEffects": [],
  "debuffEffects": [],
  "split": {
    "splitHitCount": 2,
    "splitHitInterval": 0.1
  }
}
```

## Purpose

Defines hit behavior for a skill.

SkillHitSO owns hit policy, target filtering, optional damage, optional effect entries, and optional split-hit behavior.

Movement, casting, visual data, and targeting are configured by other SOs.

## Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| hitId | string | Required | Unique hit identifier |
| maxHitCount | int | Required | Maximum number of times this hit can be applied |
| ignoreSameRoot | bool | Required | Prevents hitting targets with the same root |
| useRepeatInterval | bool | Required | Enables interval-based repeated hits |
| repeatInterval | float | Required | Time between repeated hits when repeat interval is enabled |
| hitStartTime | float | Required | Delay before the hit becomes active |
| deactivateAfterFirstHit | bool | Required | Deactivates the hit after the first successful hit |
| targetLayerMask | string | Required | Target layer mask name |
| damage | SkillHitDamageProfile | Optional | Damage profile applied on hit |
| buffEffects | EffectEntrySO[] | Optional | Buff effects applied on hit |
| debuffEffects | EffectEntrySO[] | Optional | Debuff effects applied on hit |
| split | SkillHitSplitProfile | Optional | Split-hit configuration |

## Optional References

The following profiles are optional.

- Damage
- Effects
- Split Hit

If an optional profile is omitted, the corresponding behavior is not used.

## Damage

`damage` defines the damage profile applied when the hit succeeds.

This profile is optional. If omitted, the hit does not deal damage.

## Effects

`buffEffects` and `debuffEffects` are optional arrays of `EffectEntrySO` definitions.

Each entry describes an effect applied when the hit succeeds.

Use `buffEffects` for positive effects and `debuffEffects` for harmful or control effects.

See [`EffectEntrySO.md`](../../effect/EffectEntrySO.md).

## Split Hit

Split hit allows a single hit event to be divided into multiple sub-hits, each applied in sequence with a delay. This can be used for multi-tick or chaining hit behaviors.

Example:

```json
"split": {
  "splitHitCount": 3,
  "splitHitInterval": 0.15
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| splitHitCount | int | Required | Number of sub-hits to apply |
| splitHitInterval | float | Required | Interval between each sub-hit |

## Validation

```text
hitId                    : Required
maxHitCount              : >= 1
repeatInterval           : >= 0
hitStartTime             : >= 0
split.splitHitCount      : >= 1 when split exists
split.splitHitInterval   : >= 0 when split exists
```

## References

- Effect : [`EffectEntrySO.md`](../../effect/EffectEntrySO.md)
- Cast : `SkillCastSO.md`
- Move : `SkillMoveSO.md`
- Visual : `BaseVisualSO.md`
