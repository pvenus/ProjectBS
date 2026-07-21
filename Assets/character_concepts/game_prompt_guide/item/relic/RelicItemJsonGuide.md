# Relic Item JSON Guide

## 1. Purpose and Output

Create one normalized RelicSO input JSON at:

```text
Assets/Resources/item/json/item.relic.{relic_slug}.json
```

The JSON owns item fields and current nested Effect Entry definitions. JSON
authoring does not create Unity `.asset`, `.meta`, localization, image, pool,
shop product, or reward data.

## 2. Required Inputs

```text
relicSlug
nameKo
descriptionKo
rarity
iconSpriteName
themeColor
category
subCategory
hidden
developerOnly
approvedEffectDesigns[]
approvedImplementationMapping
allowOverwrite
```

Every approved effect design must be traceable to planning. Every implementation
mapping entry must include its semantic slug, supported Effect type, exact
config, lifetime, category, duration, and maximum application count. Planning
alone is insufficient input for SO JSON conversion.

## 3. Canonical Schema

```json
{
  "relicId": "item.relic.ember_guard",
  "nameKo": "잿불 수호패",
  "descriptionKo": "장착 중 방어력이 12% 증가한다.",
  "icon": "item.relic.ember_guard.icon",
  "themeColor": {
    "r": 0.85,
    "g": 0.32,
    "b": 0.12,
    "a": 1
  },
  "rarity": "Rare",
  "effectEntries": [
    {
      "entryId": "effect.item.relic.ember_guard.defense_up.entry",
      "effect": {
        "effectId": "effect.item.relic.ember_guard.defense_up",
        "effectType": "StatModifier",
        "effectName": "잿불 수호패 방어 증가",
        "config": {
          "statType": "Defense",
          "modifierType": "Percent",
          "value": 12
        }
      },
      "lifetimeType": "Manual",
      "categoryType": "Buff",
      "duration": 0,
      "maxApplyCount": 1,
      "hasValueOverride": false,
      "valueOverride": 0
    }
  ],
  "category": "defense",
  "subCategory": "stat",
  "hidden": false,
  "developerOnly": false
}
```

`nameKo` and `descriptionKo` are JSON authoring/localization inputs. A RelicSO
builder must use them for string generation rather than serialize them as fields
on `RelicSO`.

## 4. Forbidden Fields

```text
effects
applyType
skill
skillId
baseProfile
cast
move
hits
direct EffectSO asset path or GUID
numeric-only canonical effect identity
```

## 5. Generation Procedure

1. Read the approved item design and extract exact item and effect facts.
2. Inventory current item JSON IDs and legacy relic IDs.
3. Compare behavior with existing relics and reject accidental duplicates.
4. Confirm the relic slug and derive the relic ID and output filename.
5. Assign one semantic slug to each distinct effect behavior.
6. Map each design behavior to a supported current Effect type and config.
7. Add current Effect Entry lifetime and application metadata.
8. Verify the Korean description against every effect value and unit.
9. Verify icon, color range, rarity, category, and visibility flags.
10. Write exactly one JSON file and no other output.

If the approved implementation mapping/spec is missing, incomplete, or requests
runtime-unsupported behavior, stop with `missing_implementation_mapping` or
`unsupported_relic_behavior`. Do not infer EffectSO config from planning prose.
For Relic SO generation, `ChanceOnHitStatModifier` and `AttackBleed` are
currently runtime-unsupported because they require dynamic hit-target state,
duration, stacking, and removal semantics that the Relic owner listener does not
safely provide.

## 6. Validation Matrix

| Check | Pass condition |
|---|---|
| Output | `Assets/Resources/item/json/{relicId}.json` |
| Relic ID | `item.relic.{lowercase_snake_case}` |
| File name | Exact `{relicId}.json` |
| Effects | At least one current `effectEntries` object |
| IDs | Semantic, stable, unique, and derived from relic ID |
| Schema | Current EffectSO and EffectEntrySO fields only |
| Manual lifetime | Duration `0` |
| Timed lifetime | Duration greater than `0` |
| Apply count | Greater than `0` |
| Description | Exact agreement with effect type, target intent, value, unit, and duration |
| Icon | Exact existing Sprite name |
| Color | Every RGBA component in `0..1` |
| Boundary | No asset, meta, localization, image, pool, shop, or reward output |

## 7. Failure Types

```text
missing_relic_design
invalid_relic_slug
duplicate_relic_id
duplicate_relic_behavior_without_variant_reason
missing_effect_design
invalid_effect_slug
duplicate_effect_id
unsupported_relic_behavior
unsupported_effect_type
unsupported_targeting_rule
invalid_effect_schema
invalid_effect_lifetime
invalid_effect_value
invalid_rarity
missing_icon_sprite
invalid_theme_color
missing_implementation_mapping
description_effect_mismatch
existing_relic_requires_approval
output_write_failed
```

Existing legacy assets may be read for comparison but must never be overwritten
by this JSON-authoring workflow.
