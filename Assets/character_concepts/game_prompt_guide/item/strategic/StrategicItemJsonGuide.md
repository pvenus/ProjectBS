# Strategic Item JSON Guide

## 1. Purpose

This guide creates one strategic item JSON containing item data and one
`skillId`. The linked skill is authored and generated separately.

```text
Assets/Resources/item/json/item.strategic.{item_slug}.json
```

## 2. Required Inputs

```text
itemSlug
nameKo
descriptionKo
grade
gaugeCost
defaultPrice
reusable
tags
iconSpriteName
skillId
linkedSkillDesignFile or linkedSkillJsonPath
```

The linked skill input is read-only validation context. Do not copy its fields
into the item JSON.

## 3. Output Schema

```json
{
  "strategicSkillItemId": "item.strategic.{item_slug}",
  "grade": "Basic",
  "nameKo": "예시 전략 도구",
  "descriptionKo": "지정 위치에 전략 효과를 발생시킨다.",
  "icon": "item.strategic.example.icon",
  "gaugeCost": 30,
  "reusable": true,
  "skillId": "skill.strategic.{skill_slug}",
  "defaultPrice": 100,
  "tags": ["control", "area"]
}
```

The following top-level fields are forbidden:

```text
skill
baseProfile
cast
move
hits
damage
effects
baseVisual
```

## 4. Output and Overwrite Rules

- Create exactly one `item.strategic.{item_slug}.json` for one authorized item.
- `Assets/Resources/item/json` is the canonical JSON root for strategic item SO
  JSON. Do not place new JSON in the legacy shop folder.
- Do not overwrite an existing file unless `allowOverwrite=true`.
- JSON authoring creates no Unity `.asset`, `.meta`, localization CSV, image,
  animation, or prefab.

## 5. Skill Reference Validation

```text
item.skillId == linked skill JSON.equipmentId
item.skillId == generated EquipmentSkillSO.EquipmentId
```

For JSON-only planning, the linked skill JSON may be the validation source. For
item SO generation, the corresponding EquipmentSkillSO must already exist below
`Assets/Resources`.

- Missing skill JSON during planning: `missing_linked_skill_json`.
- Missing skill SO during item build: `missing_linked_skill_resource`.
- Multiple skill SOs with the ID: `duplicate_linked_skill_resource`.
- ID disagreement: `linked_skill_id_mismatch`.

Do not generate the missing skill as part of the item task.

## 6. Description Consistency

Read the separate skill design or JSON and verify that `descriptionKo` agrees
with its target, area, damage, count, duration, buffs, debuffs, and control.
Description validation never permits embedding those fields in the item JSON.

## 7. Generation Procedure

1. Read the strategic item design.
2. Read the linked strategic skill design or standalone skill JSON.
3. Check existing item IDs and item-level duplicates.
4. Validate `itemSlug`, grade, gauge, reuse, price, tags, and icon.
5. Verify that `skillId` equals the standalone skill `equipmentId`.
6. Verify the item description against linked skill behavior.
7. Write only the item-level schema to `item.json`.
8. Assert that no nested `skill` or skill-owned field exists.
9. Report JSON validity separately from linked SO resource readiness.

## 8. Validation Matrix

| Check | Pass condition |
|---|---|
| Output | Exactly `Assets/Resources/item/json/item.strategic.{item_slug}.json` |
| Item ID | `item.strategic.{item_slug}` |
| Relationship | One non-empty `skillId` and no `skill` object |
| Skill ID | Exact match with standalone skill `equipmentId` |
| Gauge | 0-100 with source justification |
| Reuse | `true` under the current runtime |
| Price | Non-negative explicit value |
| Grade | `Basic` or `Advanced` |
| Icon | Exact existing Sprite name |
| Description | Matches the separate skill behavior |
| Boundary | No skill JSON or non-JSON asset generated |

## 9. Failure Types

```text
missing_strategic_item_design
invalid_item_slug
duplicate_item_id
unsupported_grade
invalid_gauge_cost
unsupported_reusable_false
invalid_default_price
missing_icon_sprite
missing_linked_skill_design
missing_linked_skill_json
missing_linked_skill_resource
duplicate_linked_skill_resource
linked_skill_id_mismatch
linked_skill_description_mismatch
embedded_skill_object_forbidden
existing_item_requires_approval
output_write_failed
```
