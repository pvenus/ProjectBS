# StrategicSkillItemSO

## 1. Purpose

`StrategicSkillItemSO` is fundamentally an inventory and shop item. When the
player uses the item, it invokes one linked `EquipmentSkillSO`. The item owns
identity, icon, gauge cost, reuse policy, shop price, and tags; the linked skill
and its child SOs define the resulting combat behavior.

```text
StrategicSkillItemSO
└── skillId: string
    └── Resources lookup by EquipmentSkillSO.EquipmentId
        └── EquipmentSkillSO and its child SOs
```

Use this guide with `StrategicItemRulesGuide.md` and
`StrategicItemJsonGuide.md`.

## 2. JSON Authoring Structure

```json
{
  "strategicSkillItemId": "item.strategic.example",
  "grade": "Basic",
  "nameKo": "예시 전략 도구",
  "descriptionKo": "지정 위치에 전략 효과를 발생시킨다.",
  "icon": "item.strategic.example.icon",
  "gaugeCost": 30,
  "reusable": true,
  "skillId": "skill.strategic.example",
  "defaultPrice": 100,
  "tags": ["control", "area"]
}
```

The item JSON never embeds an EquipmentSkill object. `skillId` points to a
separately generated `EquipmentSkillSO` under Unity `Resources`.

## 3. Item Fields

| Field | Type | Required | Rule |
|---|---|---:|---|
| `strategicSkillItemId` | string | Yes | `item.strategic.{slug}` |
| `grade` | string | Yes | Source-defined authoring classification |
| `nameKo` | string | Yes | Korean display name source |
| `descriptionKo` | string | Yes | Exact gameplay description and values |
| `icon` | string | Yes | Exact existing Sprite name |
| `gaugeCost` | int | Yes | 0-100; normally greater than 0 |
| `reusable` | bool | Yes | Must currently be `true` for a usable item |
| `skillId` | string | Yes | Exact `EquipmentSkillSO.EquipmentId` resource key |
| `defaultPrice` | int | Yes | Non-negative and supplied by economy input |
| `tags` | string[] | Yes | Stable lowercase tags; empty is allowed |
| `skill` | object | No | Forbidden; generate the skill separately |

## 4. Runtime Fields

The current `StrategicSkillItemSO` stores:

```text
strategicSkillItemId
icon
gaugeCost
reusable
skillId
defaultPrice
tags
```

`skillId` is serialized into `StrategicSkillItemSO`. The item does not serialize
an `EquipmentSkillSO` object reference. `grade` remains authoring metadata until
the runtime item model explicitly adds it.

## 5. ID and Reference Rules

```text
Item ID       = item.strategic.{slug}
Skill ID      = skill.strategic.{slug}
```

- Use lowercase snake case for slugs.
- `skillId` must be byte-identical to the separately generated
  `EquipmentSkillSO.EquipmentId`.
- Folder names are organizational and are not part of IDs.
- Do not derive a new slug from the Korean name after IDs are approved.
- The item generator must run after the skill asset has been generated under
  `Assets/Resources`.

## 6. Gauge and Reuse Contract

The strategic gauge maximum is currently 100. The item service spends
`gaugeCost` before executing the linked skill.

```text
0 <= gaugeCost <= 100
```

`reusable=false` is not a supported one-use flow. `StrategicSkillItemRuntimeData`
returns `reusable` directly from `CanUseInBattle()`, while the current direct UI
service path does not consume the item or consistently enforce that flag. Thus
`false` is not a reliable one-use item and behaves differently by call path.
Until one-use state is implemented and integrated, require:

```text
reusable = true
```

## 7. Skill Execution Contract

`StrategicSkillItemService` executes with:

```text
Target = null
UsePoint = true
TargetPoint = selected world position
```

New strategic skills should use `targetingType: Position` unless the service is
changed and verified for another mode. Party-wide effects may use the selected
point with an explicitly authored large Party hit area.

## 8. Icon Contract

`icon` is resolved by exact Sprite name.

- The Sprite must exist before SO build.
- Do not use a download filename unless it is the exact Sprite name.
- A missing icon is a dependency failure, not permission to reuse the common
  placeholder found in old strategic items.
- `EquipmentSkillSO` separately searches for `{skillId}.icon`; record
  whether item and skill icons intentionally use the same asset.

## 9. Localization Contract

The intended keys are:

```text
{strategicSkillItemId}.name
{strategicSkillItemId}.desc
```

Current compatibility warning:

- `StrategicSkillItemSO.DescriptionSubKey` requests `desc`.
- `ItemStringBuilder` currently writes `description`.

Always provide `descriptionKo`, but report `localization_subkey_mismatch` until
code or string data is aligned. Do not claim runtime localization is complete
based only on CSV generation.

## 10. Resource Resolution Contract

The item generator and runtime resolve the linked skill by ID from all
`EquipmentSkillSO` assets under Unity `Resources`.

- Generate the strategic skill JSON and its SO before generating the item SO.
- A missing ID is `missing_linked_skill_resource`.
- More than one resource with the same `EquipmentId` is
  `duplicate_linked_skill_resource`.
- Do not fall back to an arbitrary skill, an embedded JSON object, or a direct SO
  reference.

## 11. Validation

- Item ID is unique and follows its strategic format.
- `skillId` resolves to exactly one separately generated EquipmentSkillSO.
- The item JSON has no `skill` object.
- `gaugeCost` is within 0-100.
- `reusable` is `true` under the current runtime.
- `defaultPrice` is non-negative.
- Name and description are non-empty.
- `icon` resolves to an exact existing Sprite name.
- Builder and localization compatibility warnings are reported.

## 12. Expected Resources After Separate Builds

The item build reads the canonical item JSON and produces the item asset beside
it:

```text
Assets/Resources/item/json/
item.strategic.{slug}.asset
```

The separate skill build produces its graph under the strategic skill resource
root:

```text
Assets/Resources/skill/json/
skill.strategic.{slug}.asset
skill.strategic.{slug}.profile.asset
skill.strategic.{slug}.cast.asset
skill.strategic.{slug}.move.asset
skill.strategic.{slug}.hit.asset
skill.strategic.{slug}.visual.asset
effect.skill.strategic.{slug}.{effect_slug}.asset
effect.skill.strategic.{slug}.{effect_slug}.entry.asset
```

Only behavior that exists in JSON should produce a child asset. A pure damage
skill does not need effect assets; a skill with no movement requirement should not
receive a fabricated move asset. Unity asset generation is a separate manual build
step and is not performed by the JSON authoring prompt.
