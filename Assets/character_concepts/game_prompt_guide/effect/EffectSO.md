# EffectSO

## Structure

The `EffectSO` is defined as a ScriptableObject asset representing a single gameplay effect. Its structure is as follows:

```json
{
  "effectId": "effect.stat.attack_up",
  "effectType": "StatModifier",
  "config": {
    "statType": "Attack",
    "modifierType": "Flat",
    "value": 20
  }
}
```

> `effectType` must be one of the values defined in the `EffectType` enum and
> supported by the active `EffectAssetBuilder`. The presence of an
> `EffectConfig` or runtime class alone does not make a JSON effect buildable.

> **Note:** The format of the `config` field varies depending on the effect type. Each effect type defines its own configuration schema.

## Purpose

This document describes how to author Effect JSON assets for gameplay effects, including the structure, required fields, and authoring guidelines.

## Base Fields

| Field | Type | Description |
|-------|------|-------------|
| effectId | string | Unique identifier for the effect. Used for referencing and lookup. |
| effectType | EffectType | Type of the effect. Determines the schema of the `config` object. |
| config | EffectConfig | Serialized configuration object that defines effect-specific parameters and data. |

## EffectType

| Value | Description |
|-------|-------------|
| StatModifier | Modifies a stat using a flat or percentage value. |
| Heal | Restores HP. |
| Knockback | Pushes or pulls the target. |
| CooldownReduce | Reduces skill cooldown. |
| ChanceOnHitStatModifier | Applies a stat modifier when hitting a target. |
| ChanceOnHealStatModifier | Applies a stat modifier when receiving healing. |
| ChanceOnHealCooldownReduce | Reduces cooldown when receiving healing. |
| AttackBleed | Applies a bleed effect after attacking. |
| ChanceOnHitSkill | Triggers another skill when hitting a target. |
| Taunt | Forces the affected character to target the hit source for the entry duration. |
| OnHitKnockbackDistance | Relic-safe owner hit listener that moves the hit target by a configured final distance. |
| OnHitTimedStatModifier | Relic-safe owner hit listener that applies target-specific non-stacking timed stat state. |
| OnHitPoisonDot | Relic-safe owner hit listener that applies target-specific poison damage over time. |

`Taunt` uses an empty config object. Its duration is application metadata and
belongs on the containing EffectEntry.

## JSON Authoring

Every effect JSON contains the following common fields:

- `effectId` (required): Unique identifier for the effect.
- `effectType` (required): One of the values in the `EffectType` enum. This determines which `config` schema is valid.
- `config` (optional): Effect-specific parameters.

The contents of the `config` object depend on the selected effect type. Each effect type defines its own JSON schema for `config`. Only fields supported by the chosen effect type should be included. Unknown fields are ignored.

## Effect Types

Each effect type supports a different set of configuration fields in the `config` object. Use only the documented fields for each effect type.

### Taunt

Forces the affected character to use the hit source as its target for the
duration supplied by the EffectEntry.

```json
{
  "effectId": "effect.skill.strategic.demon_lure_incense.taunt",
  "effectType": "Taunt",
  "config": {}
}
```

Taunt has no effect-owned numeric fields. Use `categoryType: Debuff`,
`lifetimeType: Instant`, and a positive EffectEntry `duration`.

### Stat Modifier

Modifies a target's stat by a flat amount or a percentage. Commonly used for buffs and debuffs.

**Example:**

```json
{
  "effectId": "effect.attack_up",
  "effectType": "StatModifier",
  "config": {
    "statType": "Attack",
    "modifierType": "Percent",
    "value": 20
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| statType | StatType | Required | The stat to modify. See [StatEnum.md](../character/StatEnum.md). |
| modifierType | StatModifierType | Required | `Flat`, `Percent`, or `Multiply`. |
| value | number | Required | Amount to add as flat value or percentage. A value of 0 has no effect. |

**Notes:**

- Use `statType`, which is the current `EffectAssetBuilder` field.
- Do not author the obsolete `targetStat` alias; unknown fields are ignored by
  the current builder.

- Only include one stat per effect.
- Use positive values for buffs and negative values for debuffs.
- `modifierType` must be a valid `StatModifierType` value.

### Heal

Restores health to the target, either as a flat amount or scaled by attack.

**Example:**

```json
{
  "effectId": "effect.heal",
  "effectType": "Heal",
  "config": {
    "flatHealAmount": 100,
    "useAttackScaling": true,
    "attackPercentHeal": 50,
    "clampToMaxHp": true
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| flatHealAmount | number | Optional | Flat amount of HP to heal. |
| useMaxHpPercent | boolean | Optional | If true, healing uses a percentage of max HP. Defaults to true in code. |
| maxHpPercent | number | Conditional | Percentage of max HP to heal. Used when `useMaxHpPercent` is true. |
| useAttackScaling | boolean | Optional | If true, healing also scales with attack. |
| attackPercentHeal | number | Conditional | Percentage of attack added as healing. Used when `useAttackScaling` is true. |
| clampToMaxHp | boolean | Optional | If true, healing cannot exceed max HP. Defaults to true in code. |

**Notes:**

- `flatHealAmount`, `maxHpPercent`, and `attackPercentHeal` can be combined.
- If only flat healing is needed, omit percentage and scaling fields.
- At least one heal amount source should be greater than 0.
- `attackPercentHeal` is only used if `useAttackScaling` is true.

### Cooldown Reduce

Reduces skill cooldown.

**Example:**

```json
{
  "effectId": "effect.cooldown_reduce",
  "effectType": "CooldownReduce",
  "config": {
    "reduceType": "FlatSeconds",
    "reduceSeconds": 1
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| reduceType | CooldownReduceType | Required | Determines how cooldown is reduced. |
| reducePercent | number | Conditional | Percent cooldown reduction. Used by `Percent` and `PercentAndFlat`. Runtime clamps this to 0-1. |
| reduceSeconds | number | Conditional | Flat seconds to reduce cooldown. Used by `FlatSeconds` and `PercentAndFlat`. Runtime clamps this to >= 0. |

**Notes:**

- `reduceType` is required.
- Use `reducePercent` for percent-based reduction.
- Use `reduceSeconds` for flat time reduction.
- If both resolved percent and seconds are 0, the runtime does nothing.

### Knockback

Pushes or pulls the target using a direction rule and force value.

**Example:**

```json
{
  "effectId": "effect.knockback",
  "effectType": "Knockback",
  "config": {
    "force": 5,
    "directionType": "PushAwayFromSource",
    "customDirection": {
      "x": 0,
      "y": 1
    },
    "normalizeDirection": true,
    "fallbackToProjectileDirection": true
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| force | number | Required | Knockback force. Must be greater than 0 to apply movement. |
| directionType | KnockbackDirectionType | Required | Determines the knockback direction. |
| customDirection | Vector2 | Conditional | Direction used when `directionType` is `CustomDirection`. |
| normalizeDirection | boolean | Optional | If true, the direction vector is normalized before applying force. Defaults to true in code. |
| fallbackToProjectileDirection | boolean | Optional | If true, uses projectile direction when source-based direction cannot be calculated. Defaults to true in code. |

**Notes:**

- `directionType` is required and must be a valid `KnockbackDirectionType` value.
- `force` is required and must be greater than 0; 0 or lower is ignored by runtime.
- `customDirection` is only meaningful when `directionType` is `CustomDirection`.
- `normalizeDirection` should usually be true.
- `fallbackToProjectileDirection` should usually be true for projectile-based skills.

### Chance On Hit Skill

When the owner hits a target, there is a chance to trigger another skill.

**Example:**

```json
{
  "effectId": "effect.chance_on_hit_skill",
  "effectType": "ChanceOnHitSkill",
  "config": {
    "skillSo": "skill.fire_burst",
    "chance": 30,
    "requireCriticalHit": false,
    "rangeOverride": -1
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| skillSo | EquipmentSkillSO | Required | The skill to trigger. If missing, runtime does nothing. |
| chance | number | Required | Percent chance to trigger the skill on hit. Uses 0-100 scale. Runtime clamps this to 0-100. |
| requireCriticalHit | boolean | Optional | If true, the trigger only occurs on critical hit. Defaults to false in code. |
| rangeOverride | number | Optional | Override range for the triggered skill. Values <= 0 use default target/range behavior. Defaults to -1 in code. |

**Notes:**

- `chance` should be between 0 and 100.
- `chance` must be greater than 0 to trigger.
- `skillSo` must reference an existing skill asset generated before this effect is used.
- `rangeOverride` may be omitted when default target/range behavior should be used.

### Chance On Hit Stat Modifier

When the owner hits a target, there is a chance to apply a stat modifier effect.

**Example:**

```json
{
  "effectId": "effect.chance_on_hit_statmod",
  "effectType": "ChanceOnHitStatModifier",
  "config": {
    "chancePercent": 25,
    "statType": "Defense",
    "valueType": "Percent",
    "value": -10
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| chancePercent | number | Required | Percent chance to apply the modifier on hit. Uses 0-100 scale. |
| statType | StatType | Required | The stat to modify. See [StatEnum.md](../character/StatEnum.md). |
| valueType | StatModifierType | Required | `Flat`, `Percent`, or `Multiply`. |
| value | number | Required | Amount to add as flat value or percentage. A resolved value of 0 has no effect. |

**Notes:**

- Use negative `value` for debuffs.
- `chancePercent` should be between 0 and 100.
- `Percent` uses the current stat value and `value / 100`.

### Chance On Heal Stat Modifier

When the owner receives healing, there is a chance to apply a stat modifier.

**Example:**

```json
{
  "effectId": "effect.chance_on_heal_statmod",
  "effectType": "ChanceOnHealStatModifier",
  "config": {
    "chance": 1,
    "triggerTargetType": "AnyAlly",
    "statType": "Attack",
    "valueType": "Flat",
    "value": 10
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| chance | number | Optional | Chance to apply the modifier on healing. Uses 0-1 scale. Defaults to 1 in code. |
| triggerTargetType | HealTriggerTargetType | Optional | Determines which heal target can trigger the effect. Defaults to `AnyAlly` in code. |
| statType | StatType | Required | The stat to modify. See [StatEnum.md](../character/StatEnum.md). |
| valueType | StatModifierType | Required | `Flat`, `Percent`, or `Multiply`. |
| value | number | Required | Amount to add. Runtime currently applies this value directly. |

**Notes:**

- `chance` should be between 0 and 1.
- Runtime triggers only for positive healing amounts.

### Chance On Heal Cooldown Reduce

When the owner receives healing, there is a chance to reduce cooldown.

**Example:**

```json
{
  "effectId": "effect.chance_on_heal_cooldown_reduce",
  "effectType": "ChanceOnHealCooldownReduce",
  "config": {
    "chance": 1,
    "triggerTargetType": "AnyAlly",
    "reduceType": "FlatSeconds",
    "reduceSeconds": 1
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| chance | number | Optional | Chance to trigger cooldown reduction on healing. Uses 0-1 scale. Defaults to 1 in code. |
| triggerTargetType | HealTriggerTargetType | Optional | Determines which heal target can trigger the effect. Defaults to `AnyAlly` in code. |
| reduceType | CooldownReduceType | Required | Determines how cooldown is reduced. |
| reducePercent | number | Conditional | Percent cooldown reduction. Used by `Percent` and `PercentAndFlat`. Runtime clamps this to 0-1. |
| reduceSeconds | number | Conditional | Flat seconds to reduce cooldown. Used by `FlatSeconds` and `PercentAndFlat`. Runtime clamps this to >= 0. |

**Notes:**

- `reduceType` is required.
- `chance` should be between 0 and 1.
- Runtime triggers only for positive healing amounts.
- If both resolved percent and seconds are 0, the runtime does nothing.

### Attack Bleed

Applies a bleed effect to the target after an attack.

**Example:**

```json
{
  "effectId": "effect.attack_bleed",
  "effectType": "AttackBleed",
  "config": {
    "chancePercent": 10,
    "attackRatioPercent": 10
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| chancePercent | number | Required | Percent chance to apply bleed after attacking. Uses 0-100 scale. |
| attackRatioPercent | number | Required | Bleed damage ratio based on attack. Must resolve to positive damage. |

**Notes:**

- `chancePercent` should be between 0 and 100.
- `chancePercent` must be greater than 0 to trigger.
- `attackRatioPercent` must be greater than or equal to 0.

### On Hit Knockback Distance

Relic-safe owner hit listener. When the relic owner hits a target, the hit
target is moved away from the owner by an exact configured distance, clipped by
Rigidbody2D collision casts.

```json
{
  "effectId": "effect.item.relic.blunt_gear.knockback_on_hit",
  "effectType": "OnHitKnockbackDistance",
  "config": {
    "chancePercent": 100,
    "distanceMeters": 0.4,
    "directionType": "PushAwayFromSource"
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| chancePercent | number | Required | Percent chance to trigger on owner hit. Uses 0-100 scale. |
| distanceMeters | number | Required | Final displacement distance in meters before collision clipping. |
| directionType | KnockbackDirectionType | Required | Use `PushAwayFromSource` for approved Relic knockback away from owner. |

### On Hit Timed Stat Modifier

Relic-safe owner hit listener. It filters by source owner, resolves the hit
target dynamically, and applies one target-specific timed state keyed by effect,
source owner, and target. Reapplication refreshes duration without stacking.

```json
{
  "effectId": "effect.item.relic.ember_necklace.attack_down_on_hit",
  "effectType": "OnHitTimedStatModifier",
  "config": {
    "chancePercent": 10,
    "statType": "AttackPercent",
    "modifierType": "Flat",
    "value": -15,
    "durationSeconds": 3
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| chancePercent | number | Required | Percent chance to trigger on owner hit. Uses 0-100 scale. |
| statType | StatType | Required | Target stat to modify. |
| modifierType | StatModifierType | Required | `Flat`, `Percent`, or `Multiply`. |
| value | number | Required | Modifier value, or duration value for duration stats. |
| durationSeconds | number | Required | Produced target state duration. |

For `StunDuration` and `RootDuration`, the runtime refreshes the duration stat
to at least `value` and does not subtract it on removal, so the existing status
tick service remains the single countdown owner.

### On Hit Poison Dot

Relic-safe owner hit listener. It snapshots the owner's attack when poison is
applied, ticks once per configured interval, and refreshes one target-specific
poison state without stacking.

```json
{
  "effectId": "effect.item.relic.toxic_pouch.poison_on_hit",
  "effectType": "OnHitPoisonDot",
  "config": {
    "chancePercent": 15,
    "attackRatioPercentPerTick": 8,
    "tickIntervalSeconds": 1,
    "durationSeconds": 3
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| chancePercent | number | Required | Percent chance to trigger on owner hit. Uses 0-100 scale. |
| attackRatioPercentPerTick | number | Required | Percent of owner attack snapshot dealt each tick. |
| tickIntervalSeconds | number | Required | Tick interval. |
| durationSeconds | number | Required | Produced target poison duration. |

## Validation Rules

When authoring Effect JSON assets, follow these rules:

- `effectId` is required and must be unique.
- `effectType` is required and must be a valid `EffectType` enum value.
- The `config` field must conform to the schema for the selected effect type.
- Only include fields documented for the chosen effect type.
- Numeric values must be within the documented ranges for their field.
- Use only one effect type per effect asset.
- If a field is marked optional, it may be omitted if not needed.

## Authoring Guidelines

- Keep JSON minimal. Only include required and relevant fields for the chosen effect type.
- Do not include fields from other effect types in `config`.
- Use meaningful, descriptive `effectId` values that clearly indicate the effect's gameplay behavior.
- Each effect asset should describe a single, clear gameplay behavior.
- Refer to this document for valid fields and value ranges for each effect type.
