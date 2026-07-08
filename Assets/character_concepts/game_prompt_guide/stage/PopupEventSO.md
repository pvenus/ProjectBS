# PopupEventSO Guide

## Purpose

`PopupEventSO` is the runtime popup event asset used by stage nodes.

It owns:

- popup event identity
- visual references
- choice list
- choice rewards
- choice conditions
- next popup event references

It does not directly store player-facing title/body text. Text is resolved by
`StringManager` from the event and choice ids.

Runtime flow:

```text
RoundNodeSO.popupEvent
  -> PopupEventSO
  -> PopupEventPanelUI
  -> PopupEventChoice
  -> rewards / nextEvent / event completion
```

## Source Code

```text
Assets/Scripts/stage/so/PopupEventSO.cs
Assets/Scripts/stage/so/PopupEventChoice.cs
Assets/Scripts/stage/StageEnum.cs
Assets/Editor/resource_tools/stage/StagePopupEventBuilder.cs
```

## Generated Asset Path

Default builder output:

```text
Assets/Resources/stage_new/popup_events/{nodeId}.asset
```

Each popup node in stage node JSON becomes one `PopupEventSO`.

## PopupEventSO Fields

| Field | Type | Purpose |
|---|---|---|
| `eventId` | string | Stable popup event id. Also used as localization main key. |
| `mainImage` | Sprite | Main popup illustration. Optional. |
| `icon` | Sprite | Small popup/reward/category icon. Optional. |
| `choices` | `List<PopupEventChoice>` | Player choices or generated linear next choice. |

## Main Image Mapping

Each `PopupEventSO` can have a main image.

Image files should be stored under:

```text
Assets/Resources/stage_new/popup_png
```

Name each image by event id:

```text
{eventId}.main.png
```

Example:

```text
Assets/Resources/stage_new/popup_png/node.ch1.episode1.001.main.png
```

`PopupEventBuilder` looks up the Sprite by `{eventId}.main` and assigns it to
`PopupEventSO.mainImage`. The image is resolved per popup event, not per
`RoundNodeSO`.

Computed/localized properties:

| Property | Meaning |
|---|---|
| `LocalizationMainKey` | Returns `eventId`. |
| `Title` | `StringManager.Get(eventId, "title")`. |
| `Body` | `StringManager.Get(eventId, "body")`. |

## Popup Node JSON Mapping

```json
{
  "nodeId": "node.act1.chapter01.episode01.001",
  "nodeType": "narration",
  "locationId": "location.cheongun_village_entrance",
  "speakerId": "character.seojin",
  "speakerNameKo": "서진",
  "textKo": "Popup body source text.",
  "nextNodeId": "node.act1.chapter01.episode01.002",
  "choices": []
}
```

Mapping:

| JSON field | PopupEventSO / builder behavior |
|---|---|
| `nodeId` | `PopupEventSO.eventId` |
| `nodeType` | Optional compatibility field if target asset supports it. |
| `locationId` | Optional compatibility field if target asset supports it. |
| `textKo` | Source text for string generation/review, not stored in current SO. |
| `nextNodeId` | Generates a default next choice when `choices` is empty. |
| `choices` | Builds `PopupEventChoice` list. |

## PopupEventChoice Fields

| Field | Type | Purpose |
|---|---|---|
| `choiceId` | string | Stable choice id and localization key. |
| `requirements` | `List<PopupEventRequirementData>` | Cost/requirement data. |
| `visibleConditions` | `List<PopupEventChoiceConditionData>` | Conditions for displaying the choice. |
| `rewards` | `List<PopupEventRewardData>` | Rewards/effects executed after selection. |
| `completesEvent` | bool | Whether selecting this choice closes/completes the event. |
| `nextEvent` | `PopupEventSO` | Next popup event in the chain. |

Computed/localized properties:

| Property | Meaning |
|---|---|
| `Label` | `StringManager.Get(choiceId, "label")`. |
| `Description` | `StringManager.Get(choiceId, "description")`. |
| `ResultText` | `StringManager.Get(choiceId, "result", true)`. |

Choice JSON:

```json
{
  "choiceId": "choice.act1.chapter01.episode01.accept",
  "textKo": "마을 사람들을 구한다.",
  "labelKo": "마을 사람들을 구한다.",
  "resultKo": "서진은 검은 천의 무리 사이로 뛰어들었다.",
  "nextNodeId": "node.act1.chapter01.episode01.002",
  "visibleConditions": [],
  "rewards": []
}
```

Builder mapping:

| JSON field | PopupEventChoice field |
|---|---|
| `choiceId` | `choiceId` |
| `valueTag` | compatibility field if present |
| `nextNodeId` | resolved to `nextEvent` |
| `visibleConditions` | `visibleConditions` |
| `rewards` | `rewards` |

`labelKo`, `textKo`, and `resultKo` are source text for review/string table
generation. They are not directly stored in the current `PopupEventChoice`.

## Conditions

`visibleConditions[]` maps to `PopupEventChoiceConditionData`.

```json
{
  "conditionType": "HasTag",
  "targetId": "",
  "value": 0,
  "tag": "story.route.physician",
  "invert": false
}
```

Valid `PopupEventChoiceConditionType` values:

```text
None
HasCharacter
HasCharacterJob
HasCharacterJobFamily
HasCharacterJobTier
HasTag
HasRelic
HasBless
HasItem
```

## Requirements

`requirements[]` maps to `PopupEventRequirementData`.

Valid `PopupEventRequirementType` values:

```text
None
StoryItem
Relic
Bless
Tag
```

Use requirements only when the choice should consume or require something.

## Rewards

`rewards[]` maps to `PopupEventRewardData`.

```json
{
  "rewardType": "Gold",
  "rewardId": "reward.act1.chapter01.episode01.clear",
  "amount": 50
}
```

Builder mapping:

| JSON field | PopupEventRewardData field |
|---|---|
| `rewardType` | `rewardType` |
| `rewardId` | `rewardId` |
| `targetId` | `targetId` |
| `amount` | `value` when `value` is not set |
| `value` | `value` |
| `tag` | `tag` |

For `SpecialBattle` and `BossBattle`, `rewardId` is resolved as a `BattleSO`
`battleId`.

Valid `PopupEventRewardType` values:

```text
None
Gold
Hp
HpPercent
Reputation
Faith
Relic
RelicPool
StrategicSkillItem
StrategicSkillItemPool
Blessing
BlessingPool
AIFunction
FirstJobChange
SecondJobChange
SpecialBattle
BossBattle
UnlockRoute
RevealHiddenNode
NextEvent
NextBattleAttackSpeed
NextBattleMoveSpeed
NextBattleDefense
```

Current story planning default:

- Use `Gold` for ordinary rewards.
- Use `UnlockRoute` for route branching.
- Use `SpecialBattle` or `BossBattle` only when the battle branch has produced
  or planned the referenced battle.

## Battle Rewards

Battle rewards connect by stable battle id:

```json
{
  "rewardType": "SpecialBattle",
  "rewardId": "battle.act1.chapter01.01.rescue_villagers"
}
```

`PopupEventBuilder` resolves `rewardId` first, then finds a generated `BattleSO`
whose `battleId` matches that value.

The concrete battle definition must live in a separate BattleSO input JSON,
for example `Assets/Resources/battle/{battle_group}/{battle_id}.json`. Do not
duplicate full BattleSO JSON inside popup JSON.

## Authoring Rules

- `eventId` must be stable and unique.
- `choiceId` must be stable and unique enough for localization.
- Use `nextNodeId` for popup chains.
- Do not store final text in SO fields directly.
- Keep Korean source text in JSON for review and string table generation.
- Do not put raw `PopupEventSO` references in JSON.
- Use ids and let `PopupEventBuilder` resolve `nextEvent`.
- Keep reward types aligned with `PopupEventRewardType`.

## Validation

Before build:

- root `startNodeId` exists in `nodes[]`.
- every popup `nodeId` is unique.
- every choice `choiceId` is unique within the stage JSON.
- every local `nextNodeId` exists in `nodes[]`.
- every `conditionType` parses as `PopupEventChoiceConditionType`.
- every `rewardType` parses as `PopupEventRewardType`.
- gold rewards use `Gold`, not lowercase `gold`.

After build:

- every source popup node has a `PopupEventSO` asset.
- `PopupEventSO.eventId` matches source `nodeId`.
- choices are generated.
- local `nextEvent` references are resolved.
- reward values are copied into `PopupEventRewardData`.
- builder warnings are reviewed.
