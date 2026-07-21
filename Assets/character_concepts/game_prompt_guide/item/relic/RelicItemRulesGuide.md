# Relic Item Rules Guide

## 1. Domain Boundary

A relic is an item with persistent or triggered effects.

The relic owns:

- identity, rarity, icon, theme color, category, and visibility flags;
- localization identity;
- the ordered list of effect entries.

The effect owns:

- stat, chance, amount, direction, healing, bleed, cooldown, or triggered-skill
  behavior;
- effect lifetime and application metadata through its Effect Entry.

Do not model a relic as a skill. A triggered skill is allowed only through the
supported `ChanceOnHitSkill` effect and remains an external skill resource.

## 2. Identity Rules

```text
Relic ID  = item.relic.{relic_slug}
Effect ID = effect.item.relic.{relic_slug}.{effect_slug}
Entry ID  = effect.item.relic.{relic_slug}.{effect_slug}.entry
File name = item.relic.{relic_slug}.json
```

- Every slug is lowercase snake case.
- `effect_slug` describes behavior, for example `attack_up`, `bleed_on_hit`, or
  `cooldown_recovery`.
- Do not use list position (`1`, `2`, `effect_01`) as the new canonical identity.
- Reordering entries must not change any ID.
- All relic, effect, and entry IDs must be globally unique in their domains.

## 3. Design Inputs

An approved relic design must define:

- Korean name and description;
- tactical role and intended build interaction;
- rarity and balance justification;
- exact effect target and trigger intent;
- every chance, value, unit, duration, and application limit;
- icon Sprite name and theme color;
- category and subcategory;
- visibility flags.

Do not infer missing values from flavor text, rarity, icon, or a similar relic.

## 4. Supported Effect Selection

Use only current `EffectType` values:

| Design intent | Effect type |
|---|---|
| Persistent stat change | `StatModifier` |
| Immediate recovery | `Heal` |
| Push or pull | `Knockback` |
| Cooldown reduction | `CooldownReduce` |
| Stat change after an attack hit | `ChanceOnHitStatModifier` |
| Stat change after healing | `ChanceOnHealStatModifier` |
| Cooldown reduction after healing | `ChanceOnHealCooldownReduce` |
| Bleed after attacking | `AttackBleed` |
| Trigger an existing skill after a hit | `ChanceOnHitSkill` |

If the design requires an unsupported event, target filter, stacking rule, or
effect type, stop with `unsupported_relic_behavior`. Do not approximate it with a
different supported effect.

For `StatModifier`, use current builder fields `statType`, `modifierType`, and
`value`. For `ChanceOnHitStatModifier`, use `statType`, `valueType`, and `value`.
Do not use the older `targetStat` alias in new relic JSON.

## 5. Lifetime Rules

- Persistent equipped effects: `Manual`, duration `0`.
- Immediate one-time effects: `Instant`, duration `0`, only when equip-time
  execution is the approved design.
- Temporary effects: `Timed` or `CombatTimed` with duration greater than `0`.
- `maxApplyCount` must be greater than `0`.
- A trigger that must remain available while equipped is normally installed as a
  `Manual` trigger effect; its produced status may have separate timing in the
  effect implementation.
- Do not write legacy `applyType`. The Effect type expresses attack/heal triggers.

## 6. Rarity and Balance

`Common`, `Rare`, `Epic`, and `Legendary` are valid rarities. Rarity is not a
formula that automatically selects effect values.

Review balance across:

- effect count and synergy;
- trigger probability;
- flat versus percent scaling;
- uptime and duration;
- stacking/application count;
- whether the effect multiplies offense, defense, sustain, or control;
- interaction with a maximum of three equipped relics.

Multiple effects are allowed only when the design explicitly describes each one.
Do not split one semantic effect into multiple entries merely to imitate a legacy
asset layout.

## 7. Existing Data Interpretation

The current relic folder contains ten legacy RelicSO assets using stat modifiers,
knockback, chance-on-hit modifiers, and bleed. Most are `Common`; category fields
are often empty. These observations are comparison evidence, not authoring
defaults.

- Empty category/subcategory values are legacy data gaps.
- Reused or placeholder-like icons do not establish an icon reuse rule.
- Numeric effect suffixes are legacy identifiers and should become semantic IDs
  when authoring new relics.
- Legacy serialized `effects` and `applyType` do not represent the current
  `RelicSO.effectEntries` contract.

## 8. Validation

- The relic is an item and contains no skill-owned fields.
- At least one valid effect entry exists.
- Each entry contains exactly one complete Effect definition.
- Every gameplay statement in the Korean description matches the effect entries.
- No unsupported trigger, target, or stacking behavior is silently substituted.
- The icon exists, enums are current, numerical units are explicit, and IDs are
  stable and unique.
