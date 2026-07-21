# Relic Item SO Guide

## 1. Purpose

`RelicSO` is an item that remains equipped and owns one or more
`EffectEntrySO` definitions. The relic is the top-level item; an effect is its
gameplay behavior, not a separate item or skill.

Current implementation:

```text
Assets/Scripts/item/so/RelicSO.cs
Assets/Scripts/item/ItemManager.cs
Assets/Scripts/effect/so/EffectEntrySO.cs
```

## 2. Current Structure

```json
{
  "relicId": "item.relic.{relic_slug}",
  "icon": "{exact_sprite_name}",
  "themeColor": { "r": 1, "g": 1, "b": 1, "a": 1 },
  "rarity": "Common",
  "effectEntries": [],
  "category": "offense",
  "subCategory": "on_hit",
  "hidden": false,
  "developerOnly": false
}
```

| Field | Required | Rule |
|---|---|---|
| `relicId` | Yes | `item.relic.{lowercase_snake_case_slug}` |
| `icon` | Yes | Exact existing Sprite name |
| `themeColor` | Yes | RGBA values in `0..1` |
| `rarity` | Yes | `Common`, `Rare`, `Epic`, or `Legendary` |
| `effectEntries` | Yes | One or more current Effect Entry objects |
| `category` | Yes | Stable lowercase item classification |
| `subCategory` | Yes | Stable lowercase behavior classification |
| `hidden` | Yes | Hide from normal presentation when true |
| `developerOnly` | Yes | Development-only content when true |

Name and description are not stored directly on `RelicSO`. They are resolved by
`relicId` through localization keys:

```text
{relicId}.name
{relicId}.desc
```

## 3. Effect Ownership

Each element of `effectEntries` is an `EffectEntrySO` input containing one
complete `EffectSO` definition.

```json
{
  "entryId": "effect.item.relic.{relic_slug}.{effect_slug}.entry",
  "effect": {
    "effectId": "effect.item.relic.{relic_slug}.{effect_slug}",
    "effectType": "StatModifier",
    "effectName": "{effect_name_ko}",
    "config": {
      "statType": "Attack",
      "modifierType": "Percent",
      "value": 10
    }
  },
  "lifetimeType": "Manual",
  "categoryType": "Buff",
  "duration": 0,
  "maxApplyCount": 1,
  "hasValueOverride": false,
  "valueOverride": 0
}
```

`EffectSO` defines what happens. `EffectEntrySO` defines lifetime, category, and
application count. Effect-specific fields belong only inside `effect.config`.

## 4. Runtime Contract

- A maximum of three relics can be equipped under the current item service.
- All entries are applied when the relic is equipped and removed when unequipped.
- A persistent equipped stat or trigger effect normally uses `Manual` with
  `duration=0`.
- On-hit behavior is expressed only by a trigger effect type currently supported
  by the Relic runtime. `ChanceOnHitSkill`, `OnHitKnockbackDistance`,
  `OnHitTimedStatModifier`, and `OnHitPoisonDot` are owner-source filtered.
  Do not use legacy `ChanceOnHitStatModifier` or `AttackBleed` for new Relic
  generation.
- Current `RelicSO.effectEntries` has no per-entry target selector. Do not claim
  owner-only, party-only, or enemy-only targeting unless runtime support is
  verified for that effect.
- `ChanceOnHitSkill` requires its referenced skill resource to exist before the
  relic asset is built.

## 5. Legacy Boundary

Existing assets under `Assets/Resources/shop/relic` are reverse-engineering and
comparison sources. Some serialize old fields such as:

```text
effects
applyType
direct EffectSO asset references
numeric effect suffixes such as .1 and .2
```

Do not copy these fields into new JSON. New authoring uses `effectEntries`,
current nested Effect JSON, and semantic effect slugs.

## 6. References

```text
Assets/character_concepts/game_prompt_guide/item/relic/RelicItemRulesGuide.md
Assets/character_concepts/game_prompt_guide/item/relic/RelicItemJsonGuide.md
Assets/character_concepts/game_prompt_guide/effect/EffectSO.md
Assets/character_concepts/game_prompt_guide/effect/EffectEntrySO.md
Assets/character_concepts/game_prompt_guide/character/StatEnum.md
```
