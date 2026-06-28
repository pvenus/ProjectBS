# EquipmentUpgradeTableSO

## Structure

```json
{
  "upgradeTableId": "upgrade.sword.basic",
  "entries": [
    {
      "level": 1,
      "statModifiers": [
        {
          "modifierType": "BaseDamage",
          "operationType": "Flat",
          "value": 10
        }
      ],
      "effectModifiers": [
        {
          "targetEffectId": "effect.character.swordsman.basic.bleed",
          "fieldType": "Value",
          "operationType": "Flat",
          "value": 20
        }
      ]
    }
  ]
}
```

## Purpose

Defines upgrade data for each equipment level.
Each level contains stat modifiers and effect modifiers that are applied when the equipment reaches that level.

## Base Fields

| Field           | Type    | Description                                         |
|-----------------|---------|-----------------------------------------------------|
| upgradeTableId  | string  | Unique identifier for this upgrade table            |
| entries         | array   | List of upgrade level entries                       |

## Entry

Each element of `entries` represents a single equipment upgrade level.

| Field           | Type    | Description                                         |
|-----------------|---------|-----------------------------------------------------|
| level           | int     | Upgrade level (must be >= 1)                        |
| statModifiers   | array   | Array of stat modifier definitions for this level.  |
| effectModifiers | array   | Array of effect modifier definitions for this level. |

## Stat Modifiers

`statModifiers` modifies skill or projectile values at a specific upgrade level.

```json
{
  "modifierType": "BaseDamage",
  "operationType": "Flat",
  "value": 10
}
```

| Field | Type | Description |
|-------|------|-------------|
| modifierType | SkillStatModifierType | Target value to modify. |
| operationType | SkillStatModifierOperationType | How the value is modified. |
| value | float | Modifier value. |

### SkillStatModifierType

| Value | Description |
|-------|-------------|
| BaseDamage | Modifies base damage. |
| AttackPercentDamage | Modifies attack scaling damage percent. |
| Cooldown | Modifies skill cooldown. |
| Range | Modifies skill range. |
| SplitHitCount | Modifies split hit count. |
| MaxHitCount | Modifies the maximum number of targets a hit can affect. |
| ProjectileCount | Modifies projectile count. |
| ProjectileSpreadAngle | Modifies projectile spread angle. |
| ProjectileScale | Modifies projectile scale. |
| Lifetime | Modifies projectile lifetime. |
| ProjectileSpawnInterval | Modifies projectile spawn interval. |
| ProjectileSpawnRadius | Modifies projectile spawn radius. |
| ProjectileColliderRadius | Modifies projectile collider radius. |

### SkillStatModifierOperationType

| Value | Description |
|-------|-------------|
| Flat | Adds `value` to the current value. |
| Percent | Multiplies the current value by `1 + value`. |
| Override | Replaces the current value with `value`. |

## Effect Modifiers

`effectModifiers` modifies generated effect runtime data at a specific upgrade level.

```json
{
  "targetEffectId": "effect.character.swordsman.basic.bleed",
  "fieldType": "Value",
  "operationType": "Flat",
  "value": 20
}
```

| Field | Type | Description |
|-------|------|-------------|
| targetEffectId | string | Target effect to modify. |
| fieldType | EffectModifierFieldType | Target effect field to modify. |
| operationType | SkillStatModifierOperationType | How the value is modified. |
| value | float | Modifier value. |

### targetEffectId

`targetEffectId` specifies which generated effect is modified.

It must match the `effectId` defined by the target `EffectSO`.

When multiple effects exist in a single skill, this field determines which effect receives the upgrade.

### EffectModifierFieldType

| Value | Description |
|-------|-------------|
| Value | Modifies the effect value. |
| Duration | Modifies the effect duration. |
| Chance | Modifies the effect activation chance. |
| Cooldown | Modifies the effect cooldown. |
| MaxApplyCount | Modifies the maximum apply count. |
| TickInterval | Modifies the effect tick interval. |
| Radius | Modifies the effect radius. |

## Optional Profiles

There are no optional profiles.
Every entry requires a `level`.
Modifier arrays (`statModifiers`, `effectModifiers`) may be empty.

## Validation Rules
```
upgradeTableId      : Required
entries             : Required
level               : >= 1
statModifiers       : Optional array
stat.modifierType   : Required enum when stat modifier exists
stat.operationType  : Required enum when stat modifier exists
stat.value          : Any float
effectModifiers     : Optional array
effect.targetEffectId : Required when effect modifier exists
effect.fieldType    : Required enum when effect modifier exists
effect.operationType: Required enum when effect modifier exists
effect.value        : Any float
Duplicate levels    : Not allowed
```

## Notes
- Entries should be sorted by level.
- Each level should appear only once.
- Empty modifier arrays are allowed.
- `statModifiers` and `effectModifiers` are value objects written directly inside this JSON.
- Separate modifier MD files are not required.
- `effectModifiers` must specify `targetEffectId` when modifying a generated effect.