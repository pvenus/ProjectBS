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
| Legacy flattened `effectType: Taunt` with effect-owned duration | Rebuild as current nested `effect.config: {}` and put the positive duration on the Debuff EffectEntry |
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

### 6.1 Balance Input and Review Budget

The cost bands are classification gates, not a value-generation formula. A new
design must provide approved values for every applicable power axis:

```text
damage or healing amount
flat, ratio, or percentage-point basis
target count or distribution rule
application radius or all-party scope
duration
repeat count and interval
control strength
stacking and reapplication
combined secondary effect
```

Review the proposed values against at least two existing skills with the same
primary intent. Record the comparison in the task report, including skill ID,
gauge cost, area, duration, and principal amount. Existing values may reject an
outlier, but they do not authorize inventing a replacement value.

If an approved value is missing, use `insufficient_balance_input`; do not choose
the midpoint of a band. `review_ready` means the supplied values are internally
consistent, not that the guide independently proves mathematical balance.

### 6.2 Grade and Growth

`Basic` and `Advanced` are item authoring metadata. They do not automatically
multiply skill power and do not define a growth curve.

- Strategic skills are non-upgradeable unless an approved design explicitly
  defines levels and every per-level modifier.
- Do not generate `upgradeTable` from grade or gauge cost.
- An upgradeable strategic skill must provide level, target value, operation,
  and modifier amount for every entry and must follow
  `EquipmentUpgradeTableSO.md` during the later JSON step.
- Missing growth data blocks only the requested upgrade behavior; it does not
  authorize a flat or percentage default.

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

For planning conversion, distinguish these layers before authoring JSON:

```text
projectile or occurrence count
successful target-hit budget
repeat hit on the same target
split damage within one successful hit
```

Do not treat one layer as another. A planning sentence must state the
player-facing occurrence count and target-distribution rule when more than one
layer could produce the same apparent total.

## 8. Effect Rules

For JSON-to-SO generation, use only values supported by both the current
`EffectType` enum and `EffectAssetBuilder`:

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
Taunt
```

- Immediate heal or cooldown reduction uses `Instant`, duration 0.
- Temporary buffs/debuffs use `Timed` or `CombatTimed`, duration greater than 0,
  except Taunt, which uses `Instant` with a positive entry duration because the
  forced-target runtime owns its expiry.
- Helpful entries belong in `buffEffects`; harmful/control entries belong in
  `debuffEffects`.
- Each entry embeds one EffectSO behavior.
- Use only actual `StatType` enum values.
- Stun/root via `StunDuration` or `RootDuration` requires verified runtime behavior.
- Taunt is a harmful control effect and belongs in `debuffEffects`. Use an empty
  `config` object; place the exact taunt duration on the EffectEntry.
- Taunt is bound to the character hit and the projectile transform at hit time,
  so a position-targeted incense projectile becomes the forced attack target.
- A legacy `TauntEffectSO` asset is evidence of intent only. Do not assume it is
  compatible with the current `EffectSO` config schema or current enum values.

Current Effect builder field names take precedence when a shared guide and code
use different historical names:

| Effect | Current config fields |
|---|---|
| `StatModifier` | `statType`, `modifierType`, `value` |
| `ChanceOnHitStatModifier` | `chancePercent`, `statType`, `valueType`, `value` |
| `ChanceOnHealStatModifier` | `chance`, `triggerTargetType`, `statType`, `modifierType`, `value` |
| `ChanceOnHitSkill` | `chance`, `requireCriticalHit`, `skillId` |
| `Taunt` | No config fields; use `{}` and author duration on the EffectEntry |

This precedence is normative. Conflicting examples in the shared `EffectSO.md`
are historical for strategic-skill JSON authoring; the field table above and
the current builder must agree before generation proceeds.

### 8.1 Stacking, Reapplication, and Termination

Planning must identify one behavior for every non-instant effect:

```text
does not stack and refreshes duration
does not stack and keeps the longer remaining duration
replaces the previous value
stacks independently up to an approved maximum
unresolved design choice
```

Do not infer stacking from `maxApplyCount`; that field limits application of an
entry and is not a complete stacking policy. Timed effects terminate when their
approved duration expires unless a verified runtime rule says otherwise.

Strategic skills use the linked item's gauge as their normal use restriction.
The later skill JSON uses cooldown 0 only when no separate cooldown was approved.
If a separate cooldown is requested, it must appear in planning and be verified
against the strategic-item activation service before JSON generation.

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
