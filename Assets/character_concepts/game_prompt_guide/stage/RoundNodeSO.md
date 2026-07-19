# RoundNodeSO Guide

## Purpose

`RoundNodeSO` is the design-time stage map node asset.

It defines what kind of node appears on the stage route and which payload should
run when the player selects that node.

For story/event nodes, the payload is usually a `PopupEventSO`.

Runtime flow:

```text
RoundNodeSO
  -> RoundNode runtime data
  -> Stage map button
  -> node selection
  -> popup event / battle / other node execution
```

## Source Code

```text
Assets/Scripts/stage/so/RoundNodeSO.cs
Assets/Scripts/stage/StageEnum.cs
Assets/Editor/resource_tools/stage/StageNodeBuilder.cs
```

## Generated Asset Path

Default builder output:

```text
Assets/Resources/stage_new/nodes/{stageNodeId}.asset
```

## Fields

| Field | Type | Purpose |
|---|---|---|
| `nodeId` | string | Stable node id. Also used as localization main key. |
| `nodeType` | `RoundNodeType` | Runtime node category. |
| `popupEvent` | `PopupEventSO` | Popup event payload for event/battle popup nodes. |
| `isRequired` | bool | Whether this node is required in the route/progression. |
| `hiddenByDefault` | bool | Whether the node starts hidden until revealed. |
| `tags` | `List<string>` | Selection, filtering, route, and authoring tags. |
| `appearanceConditions` | `List<RoundNodeCondition>` | Conditions that must be satisfied for random pool appearance. |

Computed/localized properties:

| Property | Meaning |
|---|---|
| `LocalizationMainKey` | Returns `nodeId`. |
| `Title` | `StringManager.Get(nodeId, "title")`. |

## RoundNodeType

Valid values:

```text
None
Start
Battle
EliteBattle
Boss
Event
RequiredSubEvent
Shop
Rest
```

Recommended usage:

- `Start`: stage start node.
- `Event`: normal story or popup event.
- `RequiredSubEvent`: mandatory story side event.
- `Battle`: normal battle node.
- `EliteBattle`: harder battle node.
- `Boss`: boss stage node.
- `Shop`: shop node.
- `Rest`: rest/recovery node.

If a story episode is represented by a popup chain, use `Event` unless it must
be distinguished as mandatory side content.

## RoundNodeCondition

`appearanceConditions` uses:

```json
{
  "conditionType": "HasTag",
  "targetId": "",
  "invert": false
}
```

Supported `RoundNodeConditionType` values:

```text
None
HasCharacter
HasEquipment
HasRelic
HasItem
HasFaith
HasBless
```

Use `invert: true` when the node should appear only when the condition is not
met.

## Stage Node JSON Mapping

`StageNodeBuilder` maps stage node JSON root fields into `RoundNodeSO`.

```json
{
  "stageNodeId": "stage.act1.chapter01.episode01",
  "roundNodeType": "Event",
  "startNodeId": "node.act1.chapter01.episode01.village_arrival",
  "isRequired": true,
  "hiddenByDefault": false,
  "tags": ["story", "act1", "chapter01"]
}
```

Mapping:

| JSON field | RoundNodeSO field |
|---|---|
| `stageNodeId` | `nodeId` |
| `roundNodeId` | fallback for `nodeId` |
| `nodeId` | fallback for `nodeId` |
| `roundNodeType` | `nodeType` |
| `nodeType` | fallback for `nodeType` |
| `isRequired` | `isRequired` |
| `hiddenByDefault` | `hiddenByDefault` |
| `tags` | `tags` |
| root `startNodeId` | resolved to `popupEvent` through `PopupEventBuilder` |

## Authoring Rules

- `nodeId` must be stable. Do not rename it casually.
- `nodeId` should be globally unique across stage nodes.
- `RoundNodeSO` should not store final story text directly.
- Story text belongs to string data keyed by `nodeId` or popup event ids.
- Use `popupEvent` for popup-driven story/event content.
- Do not put `CharacterSO`, `BattleSO`, or Sprite references directly in JSON.
- Use ids in JSON and let editor builders resolve references.
- For new episode popup chains, `startNodeId` must be the first planning
  `popupDefinitions[].popupId`, copied into the corresponding Stage `nodeId`.
- Existing indexed start ids remain valid immutable legacy references.

## Validation

Before build:

- `stageNodeId`, `roundNodeId`, or `nodeId` exists.
- `roundNodeType` parses as `RoundNodeType`.
- `startNodeId` exists when a popup event is expected.
- `tags` are review-friendly and do not encode runtime object names.

After build:

- `RoundNodeSO.nodeId` matches the selected stage node id.
- `RoundNodeSO.nodeType` matches the JSON.
- `RoundNodeSO.popupEvent` is assigned for popup event nodes.
- `RoundNodeSO.tags` are copied from JSON.
- Builder warnings are reviewed.
