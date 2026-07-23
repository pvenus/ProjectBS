# Strategic Skill JSON Guide

## 1. Purpose

This guide defines normalized standalone JSON authoring for the skill executed by
a strategic item, under:

```text
Assets/Resources/skill/json/{skillId}.json
```

Embedded skills in existing shop item files are comparison data, not templates
for current fields.

Shared schema references:

```text
Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentBaseProfileSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillCastSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillHitSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillMoveSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/BaseVisualSO.md
Assets/character_concepts/game_prompt_guide/effect/EffectSO.md
Assets/character_concepts/game_prompt_guide/effect/EffectEntrySO.md
```

## 2. Required Inputs

```text
strategicSkillPlanningFile or explicit approved design
planningReviewStatus = review_ready
skillSlug
skillName
tacticalRoleKo
targetingKo
effectsKo
executionKo
presentationKo
mechanicsCompleteness = pass
evidenceConflictCount = 0
```

Do not infer missing balance numbers from a name or flavor text.
When a reverse-planned entry is used, do not generate standalone skill JSON from
`needs_decision` or `blocked` status. Translate the plain-language design through
this guide and the detailed SO guides; do not require SO field names inside the
planning document.

Before conversion, confirm that the approved planning text resolves every
applicable topic below:

```text
activation and use restriction
target selection and target distribution
application area
ordered effects
per-hit/per-second/total numeric basis
count and interval
duration
stacking and reapplication
termination
```

## 3. Output and Overwrite Rules

```text
Assets/Resources/skill/json/skill.strategic.{skill_slug}.json
```

- Create one standalone JSON for one authorized strategic skill.
- Do not overwrite an existing file unless `allowOverwrite=true`.
- The filename uses the complete `equipmentId`.
- Do not write the skill JSON below a shop item folder.
- `Assets/Resources/skill/json` is the canonical JSON root for every skill SO JSON,
  including skills executed by strategic items.
- JSON authoring creates no Unity asset or `.meta` file.

## 4. Top-Level Schema

```json
{
  "equipmentId": "skill.strategic.{skill_slug}",
  "skillName": "{skill_name}",
  "baseProfile": {},
  "cast": {},
  "move": {},
  "hits": [],
  "baseVisual": {}
}
```

Do not add item-owned fields, an outer `skill` wrapper, legacy `slotName`, or
`visualSet`.

## 5. Strategic Skill Profiles

### 5.1 Base Profile

```json
"baseProfile": {
  "baseProfileId": "skill.strategic.{slug}.profile",
  "skillType": "Active",
  "skillComponentType": "Projectile",
  "projectileCount": 1,
  "projectileScale": 1,
  "projectileColliderRadius": 6,
  "projectileLifetime": 0.4
}
```

`projectileColliderRadius` must match the authored area.

`projectileLifetime` must be at least `0.3` seconds even for an instant skill so
its animation and visual presentation have a guaranteed display window. This
technical lifetime does not change an instant effect's `lifetimeType` or
`duration`.

Optional multi-projectile fields:

```json
"projectile": {
  "arrangement": "Circle",
  "arrangementValue": 0,
  "spreadAngle": 0,
  "radius": 5
},
"projectileSpawn": {
  "spawnOffset": 0,
  "interval": 0.1
}
```

### 5.2 Cast

```json
"cast": {
  "castId": "skill.strategic.{slug}.cast",
  "targetingType": "Position",
  "cooldown": 0,
  "castTime": 0,
  "range": 999,
  "skipAttackAnimation": true
}
```

Do not use legacy `LowestHpAlly`.

`cooldown: 0` is the strategic default only when the approved planning states
that the linked item gauge is the sole use restriction. Do not overwrite an
explicit approved cooldown with 0. A non-zero strategic cooldown requires
verified activation-service support.

### 5.3 Move

Immediate point-centered area:

```json
"move": {
  "moveId": "skill.strategic.{slug}.move",
  "moveType": "Warp",
  "applyDirectionRotation": false,
  "rotationOffset": 0
}
```

Omit `move` only when verified runtime behavior does not require it. Do not add an
empty legacy `warp` block.

### 5.4 Hit

```json
"hits": [
  {
    "hitId": "skill.strategic.{slug}.hit",
    "maxHitCount": 999,
    "ignoreSameRoot": false,
    "useRepeatInterval": false,
    "repeatInterval": 0,
    "hitStartTime": 0,
    "deactivateAfterFirstHit": false,
    "targetLayerMask": "Enemy",
    "buffEffects": [],
    "debuffEffects": []
  }
]
```

Generated JSON should omit unused optional fields rather than write `null`.

### 5.5 Base Visual

```json
"baseVisual": {
  "visualId": "skill.strategic.{slug}.visual",
  "projectileVisualType": "Default"
}
```

Use `Rain` only for supported falling-object or bombardment behavior.

## 6. Damage Schema

```json
"damage": {
  "damageId": "skill.strategic.{slug}.damage",
  "skillId": "skill.strategic.{slug}",
  "damageType": "Normal",
  "baseDamage": 120,
  "attackPercentDamage": 0,
  "canCritical": false,
  "ignoreDefense": false
}
```

Omit damage for pure heal, buff, debuff, control, or displacement.

The planning-to-damage mapping is exact:

- "회당 N 피해" maps N to one occurrence's damage, not the total.
- "총 N 피해" must first be divided only when count and equal distribution are
  explicitly approved.
- A percentage-point stat change (`%p`) is not damage scaling.
- Do not derive attack scaling, critical behavior, or defense ignore from grade,
  gauge cost, name, or visual concept.

## 7. Effect Entry Schema

Use the current nested `config` form.

Knockback debuff:

```json
{
  "entryId": "effect.skill.strategic.{slug}.knockback.entry",
  "effect": {
    "effectId": "effect.skill.strategic.{slug}.knockback",
    "effectType": "Knockback",
    "effectName": "{name_ko} 밀쳐내기",
    "config": {
      "force": 5,
      "directionType": "PushAwayFromSource",
      "normalizeDirection": true,
      "fallbackToProjectileDirection": true
    }
  },
  "lifetimeType": "Instant",
  "categoryType": "Debuff",
  "duration": 0,
  "maxApplyCount": 1
}
```

Timed party buff:

```json
{
  "entryId": "effect.skill.strategic.{slug}.attack_up.entry",
  "effect": {
    "effectId": "effect.skill.strategic.{slug}.attack_up",
    "effectType": "StatModifier",
    "effectName": "{name_ko} 공격 증가",
    "config": {
      "statType": "Attack",
      "modifierType": "Flat",
      "value": 40
    }
  },
  "lifetimeType": "Timed",
  "categoryType": "Buff",
  "duration": 8,
  "maxApplyCount": 1
}
```

Do not use `valueType` for current `StatModifier`.
`ChanceOnHitStatModifier` is a different config and does use `valueType`; follow
the exact per-effect field table in `StrategicSkillRulesGuide.md`.

Taunt debuff:

```json
{
  "entryId": "effect.skill.strategic.{slug}.taunt.entry",
  "effect": {
    "effectId": "effect.skill.strategic.{slug}.taunt",
    "effectType": "Taunt",
    "effectName": "{name_ko} 도발",
    "config": {}
  },
  "lifetimeType": "Instant",
  "categoryType": "Debuff",
  "duration": 6,
  "maxApplyCount": 1
}
```

Taunt duration belongs on the EffectEntry, not inside `effect.config`. Taunt is
resolved before collision and bound to the character hit and the current
projectile transform immediately before application. Use `Instant` with a
positive duration; forced-target state owns the expiry and a later application
replaces the target and restarts the duration.

For strategic-skill generation, the current field table in
`StrategicSkillRulesGuide.md` is normative when a shared `EffectSO.md` example
uses a historical name. Fail with `conflicting_design_input` if the guide table
and current builder no longer agree.

Planning stacking language must be validated against the selected EffectSO and
EffectEntry runtime behavior. `maxApplyCount` must not be used as a substitute
for an unimplemented stacking policy.

## 8. Generation Procedure

1. Read the source design and extract exact numeric behavior.
2. Inventory existing standalone and legacy embedded strategic skill IDs.
3. Compare behavior with the closest existing strategic skills.
4. Reject accidental duplicates.
5. Approve the slug and derive every ID.
6. Classify intent, target, area, hit policy, damage, effects, movement, and visual.
7. Author the standalone EquipmentSkill JSON using current schemas.
8. Omit unused optional profiles and legacy fields.
9. Validate syntax, enums, IDs, ranges, polarity, and description consistency.
10. Write only `skill.strategic.{skill_slug}.json`.
11. Do not generate or modify the linked item JSON.

## 9. Validation Matrix

| Check | Pass condition |
|---|---|
| Output | Exactly `{strategicSkillRoot}/skill.strategic.{skill_slug}.json` |
| Skill ID | `skill.strategic.{skill_slug}` in `equipmentId` |
| Child IDs | Derived from the exact skill ID |
| Skill schema | Current `baseProfile`, `cast`, optional `move`, `hits`, `baseVisual` |
| Targeting | `Position` under current service |
| Hit count | No count/deactivation contradiction |
| Effects | Current enum, config, polarity, lifetime, duration |
| Evidence | No unresolved evidence conflict |
| Mechanics | Target distribution, numeric basis, repetition, stacking, use restriction, and termination resolved |
| Planning status | Exactly `review_ready` with no open question |
| Boundary | No item JSON, asset, meta, CSV, image, animation, or prefab created |

## 10. Failure Types

```text
missing_strategic_skill_design
insufficient_balance_input
invalid_skill_slug
conflicting_design_input
duplicate_skill_id
duplicate_behavior_without_variant_reason
unsupported_targeting_type
unsupported_effect_type
invalid_effect_schema
invalid_skill_schema
existing_skill_requires_approval
output_write_failed
```
