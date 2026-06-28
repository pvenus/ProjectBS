# EffectEntrySO

## Structure

`EffectEntrySO` defines how an `EffectSO` is applied.

`EffectSO` describes **what the effect is**, while `EffectEntrySO` describes **how that effect is applied in a specific context**.

```json
{
  "effect": {
    "effectId": "effect.stat.attack_up",
    "effectType": "StatModifier"
  },
  "lifetimeType": "CombatTimed",
  "categoryType": "Buff",
  "duration": 5,
  "maxApplyCount": 1
}
```

For authoring `EffectSO` itself, see [`EffectSO.md`](./EffectSO.md).

## Purpose

This document describes how to author Effect Entry JSON data.

An Effect Entry is used when a skill, bless, relic, or other gameplay asset needs to apply an effect with specific application rules.

For example, the same `EffectSO` can be reused with different entries:

- A short 3 second buff from a skill.
- A long 30 second buff from a relic.
- A manual blessing effect that remains until removed.

## Base Fields

| Field | Type | Description | Required |
|-------|------|-------------|----------|
| effect | object | Embedded `EffectSO` definition. See [`EffectSO.md`](./EffectSO.md) for the full schema. | Required |
| lifetimeType | EffectLifetimeType | How long the effect lives. | Required |
| categoryType | EffectCategoryType | Effect category used for grouping and UI. | Required |
| duration | float | Duration value. Must be 0 or greater. | Required |
| maxApplyCount | int | Maximum number of times this entry can apply. Must be greater than 0. | Required |

`effect` is required.

## JSON Authoring

- `effect` contains a complete `EffectSO` JSON object.
- The full schema is documented in `EffectSO.md`.
- `EffectEntrySO` only adds application metadata such as lifetime, category, duration, and maxApplyCount.


The entry itself does not define effect-specific parameters such as stat type, heal amount, knockback force, or bleed ratio. Those values belong to `EffectSO`.

## Effect Reference

The `effect` field contains a complete `EffectSO` definition.

The JSON schema, supported effect types, configuration fields, examples, and validation rules are documented in [`EffectSO.md`](./EffectSO.md).

This document only describes the application metadata added by `EffectEntrySO`.

## Lifetime Types

`lifetimeType` decides how the effect is managed after it is applied.

| Value | Description |
|-------|-------------|
| Instant | Applies immediately and does not remain active. |
| Manual | Remains active until explicitly removed. |
| Timed | Remains active for `duration`. |
| CombatTimed | Remains active for `duration` during combat. |
| CombatOnly | Remains active while combat is active. |
| ConsumeOnBattleStart | Consumed when battle starts. |
| ConsumeOnBattleEnd | Consumed when battle ends. |

Use only values that exist in the project enum.

## Category Types

`categoryType` classifies the effect for application and display.

| Value | Description |
|-------|-------------|
| Buff | Positive effect. |
| Debuff | Negative effect. |

Use `Buff` for helpful effects and `Debuff` for harmful effects.

## Type Categories

### Instant Effect Entry

Used for effects that apply once and do not remain active.

```json
{
  "effect": {
    "effectId": "effect.heal.small",
    "effectType": "Heal"
  },
  "lifetimeType": "Instant",
  "categoryType": "Buff",
  "duration": 0,
  "maxApplyCount": 1
}
```

| Field | Value |
|-------|-------|
| lifetimeType | Instant |
| duration | 0 |
| maxApplyCount | Usually 1 |

### Timed Buff Entry

Used for temporary buffs.

```json
{
  "effect": {
    "effectId": "effect.stat.attack_up",
    "effectType": "StatModifier"
  },
  "lifetimeType": "CombatTimed",
  "categoryType": "Buff",
  "duration": 5,
  "maxApplyCount": 1
}
```

| Field | Value |
|-------|-------|
| lifetimeType | Timed or CombatTimed |
| categoryType | Buff |
| duration | Greater than 0 |
| maxApplyCount | Usually 1 |

### Timed Debuff Entry

Used for temporary debuffs.

```json
{
  "effect": {
    "effectId": "effect.stat.defense_down",
    "effectType": "StatModifier"
  },
  "lifetimeType": "CombatTimed",
  "categoryType": "Debuff",
  "duration": 3,
  "maxApplyCount": 1
}
```

| Field | Value |
|-------|-------|
| lifetimeType | Timed or CombatTimed |
| categoryType | Debuff |
| duration | Greater than 0 |
| maxApplyCount | Usually 1 |

### Manual Entry

Used for effects that should remain active until removed by system logic.

```json
{
  "effect": {
    "effectId": "effect.bless.attack_up",
    "effectType": "StatModifier"
  },
  "lifetimeType": "Manual",
  "categoryType": "Buff",
  "duration": 0,
  "maxApplyCount": 1
}
```

| Field | Value |
|-------|-------|
| lifetimeType | Manual |
| duration | 0 |
| maxApplyCount | Usually 1 |

Manual entries are useful for blesses, relics, equipment effects, and other persistent effects.

## Validation Rules

```text
effect                  : Required EffectSO object.
lifetimeType            : Required enum.
categoryType            : Required enum.
duration                : Must be >= 0.
maxApplyCount           : Must be > 0.
Timed duration          : Should be > 0.
Instant duration        : Usually 0.
Manual duration         : Usually 0.
```

Invalid enum values should fail generation.

If `duration` is negative, the entry is invalid.

If `maxApplyCount` is 0 or less, the entry is invalid.

Unlimited apply count is not supported. Use a sufficiently large value if repeated application is required.

## Notes

> `EffectSO` defines the effect data.
>
> `EffectEntrySO` defines application data.
>
> Do not place effect-specific config fields directly on an Effect Entry.
>
> The embedded `effect` may describe the same effect behavior across multiple entries when only duration, category, or lifetime differs.
>
> `EffectEntrySO` is converted into runtime data before application.

## Authoring Guidelines

- Keep entry JSON focused on application rules.
- Use `effect` to embed the complete `EffectSO` JSON defined in [`EffectSO.md`](./EffectSO.md).
- Prefer `Instant` for immediate effects such as heal or one-time cooldown reduction.
- Prefer `CombatTimed` for temporary combat buffs or debuffs.
- Prefer `Manual` for bless, relic, equipment, or persistent effects.
- Do not use negative duration values.
- Do not use 0 or negative `maxApplyCount` values.
- Do not mix multiple effects into one entry. One entry should apply one effect.