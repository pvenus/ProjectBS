# Strategic Skill Rules Guide

## 1. Purpose

This guide defines the separately generated EquipmentSkill used by a strategic
item. The item owns price, gauge, icon, and `skillId`; this guide owns the combat
behavior resolved from that ID. It separates observed embedded legacy data from
rules for new standalone skill JSON.

## 2. Existing Data Analysis

The current shop folder contains 20 legacy strategic `item.json` files with
embedded skills. Observed patterns:

- Item IDs use `item.strategic.{slug}`.
- Linked skill IDs use `skill.strategic.{slug}`.
- Every existing item has `reusable: true`.
- Gauge costs range from 10 to 100.
- Most prices are 100; one observed price is 150.
- Observed grades are `Basic` and `Advanced`.
- Two existing items have empty `descriptionKo`, and 19 of 20 have no tags. These
  are data gaps, not optional-field precedents for new items.
- Intent families include damage, heal, party buff, area debuff, crowd control,
  displacement, and bombardment.
- All existing item icons use the same placeholder-like Sprite name. This is not
  an approved uniqueness rule for new content.

Existing files are useful for gameplay intent and relative cost comparison, but
they are not the schema authority.

## 3. Legacy Fields That Must Not Be Copied

| Legacy or unsupported pattern | Current rule |
|---|---|
| `profileId` | `baseProfileId` |
| `attackArchetype`, root `category`, root `targetType` | Use current base fields and hit target layer |
| `visualSet`, `impactVisual`, `trailVisual` | Use required `baseVisual` |
| `castType`, `autoCast`, `canMoveWhileCasting` | Omit unless a current guide field exists |
| `targetingType: LowestHpAlly` | Unsupported; use `Position` for current strategic service |
| `effectType: Taunt` | Unsupported by current `EffectType` |
| `StatModifier.valueType` | Use `config.modifierType` |
| Flattened old effect copied verbatim | Prefer current nested `effect.config` |
| Skill `slotName: strategic` | Not a current EquipmentSkill JSON field; omit |

Do not migrate an old embedded object by renaming only one field. Rebuild a
standalone normalized skill JSON from its intended behavior.

## 4. Strategic Skill Identity

Choose exactly one primary intent:

```text
damage
heal
party_buff
area_debuff
hard_control
displacement
bombardment
utility
```

At most one secondary intent may be added when the source design explicitly
requires it, such as damage plus knockback or pull plus root. Do not combine
unrelated effects merely to justify a higher gauge cost.

## 5. Targeting and Area Rules

Current strategic execution is point-based:

```json
"targetingType": "Position"
```

Application target belongs to `SkillHitSO`:

```text
Enemy effect → targetLayerMask: Enemy
Party effect → targetLayerMask: Party
```

Area size must be represented by the implemented hit/projectile data, including
`projectileColliderRadius`. Do not mention a radius only in prose while leaving
JSON at an unrelated value.

Use a global-sized Party area only when the source explicitly says all allies.
Do not silently copy the legacy value `999` into a local effect.

## 6. Linked Item Gauge Cost Rules

Gauge cost belongs to the linked item JSON, not this skill JSON. The skill author
must still compare its tactical weight with that cost. The current gauge maximum
is 100. Default observed gains are 1 for a normal monster, 8 for an elite, and 20
for a boss.

Use these bands as review aids, not automatic formulas:

| Gauge cost | Expected tactical weight |
|---:|---|
| 10-25 | Small emergency action or narrow utility |
| 30-55 | Reliable area control, support, or moderate damage |
| 60-80 | Strong combination effect or large-area pressure |
| 85-100 | Encounter-shaping ultimate effect |

- Power, radius, duration, count, cost, and price must come from a design source
  or explicit input.
- Existing items support relative comparison but do not authorize copying values.
- Missing required values cause `insufficient_balance_input`.
- `gaugeCost=0` needs explicit design approval.
- Do not exceed 100 without a runtime/config change.

## 7. Hit Policy

Single application:

```text
maxHitCount = 1
deactivateAfterFirstHit = true
useRepeatInterval = false
```

Multi-target or piercing area:

```text
maxHitCount > 1
deactivateAfterFirstHit = false
```

`maxHitCount` is the total successful-hit budget, not a per-target budget. Use a
large value only for intentionally broad application.

Repeated ticks require `useRepeatInterval=true` and a positive
`repeatInterval`. Do not provide an interval while disabling it.

## 8. Effect Rules

Use only current `EffectType` values:

```text
StatModifier
Heal
Knockback
CooldownReduce
ChanceOnHitStatModifier
ChanceOnHealStatModifier
ChanceOnHealCooldownReduce
AttackBleed
ChanceOnHitSkill
```

- Immediate heal or cooldown reduction uses `Instant`, duration 0.
- Temporary buffs/debuffs use `Timed` or `CombatTimed`, duration greater than 0.
- Helpful entries belong in `buffEffects`; harmful/control entries belong in
  `debuffEffects`.
- Each entry embeds one EffectSO behavior.
- Use only actual `StatType` enum values.
- Stun/root via `StunDuration` or `RootDuration` requires verified runtime behavior.
- Do not generate `Taunt`; it appears in legacy data but not the current enum.

Current Effect builder field names take precedence when a shared guide and code
use different historical names:

| Effect | Current config fields |
|---|---|
| `StatModifier` | `statType`, `modifierType`, `value` |
| `ChanceOnHitStatModifier` | `chancePercent`, `statType`, `valueType`, `value` |
| `ChanceOnHealStatModifier` | `chance`, `triggerTargetType`, `statType`, `modifierType`, `value` |
| `ChanceOnHitSkill` | `chance`, `requireCriticalHit`, `skillId` |

For `ChanceOnHitSkill`, `skillId` must resolve to an existing generated skill.
Do not invent a target skill or use a non-builder `skillSo` string field.

## 9. Damage and Multi-Projectile Rules

- Damage belongs under `hits[].damage`.
- Use `baseDamage` and/or `attackPercentDamage` only when the source defines the
  scaling model.
- Set `canCritical` and `ignoreDefense` explicitly.
- Projectile count belongs in `baseProfile.projectileCount`.
- Arrangement belongs in optional `baseProfile.projectile`.
- Spawn timing belongs in optional `baseProfile.projectileSpawn`.
- Use `baseVisual.projectileVisualType: Rain` only for real falling-object or
  bombardment visuals.

Do not multiply the same behavior through projectile count, burst count, split
hits, and repeat hits unless the design separately defines every layer.

## 10. Visual and Icon Boundary

JSON authoring references existing resources; it does not generate images.

- Every new strategic skill includes `baseVisual`.
- Use `Default` unless a supported special visual is required.
- Skill visual resources must resolve before Unity build is complete. The item
  icon is validated by the separate item workflow.
- Missing animation or icon resources are reported, not replaced with invented
  names or the old shared placeholder.

## 11. Duplicate and Family Rules

Before creation:

1. Inventory every existing strategic skill ID and linked item ID.
2. Compare primary intent, target layer, radius, duration, effects, and cost.
3. Reject an exact duplicate unless a variant is explicitly authorized.
4. Record a variant's gameplay distinction in source design and tags.

Folder labels with spaces are not identity. Use IDs and behavior for duplicates.

## 12. Required Validation

- One standalone JSON defines exactly one strategic skill.
- The skill JSON contains no item price, gauge, reusable, item icon, or item ID.
- A linked item references it only through `skillId`.
- IDs follow strategic derivation rules.
- Current schemas replace legacy fields.
- Primary and optional secondary intent match the description.
- Targeting supports point-based strategic execution.
- Layer, radius, duration, hit policy, damage, and effects agree.
- When a linked item is part of the task, its 0-100 gauge cost is consistent with
  the skill's tactical weight; the cost is not written into the skill JSON.
- All enums and effect types are supported.
- A JSON-only task creates no `.asset`, `.meta`, localization CSV, icon,
  animation, or prefab.
