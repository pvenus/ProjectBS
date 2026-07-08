# Episode Stage Node Create Guide

## Purpose

Create runtime stage authoring JSON from episode planning or finalized episode
script data, then build:

- `RoundNodeSO`
- `PopupEventSO`

This is the stage/event runtime branch of the story pipeline.

It is separate from battle planning. Battle planning creates `BattleSO`; this
guide creates the stage node and popup event chain that can launch story,
choice, route, reward, or battle outcomes.

## Pipeline Position

```text
Episode Markdown
  -> Episode Planning JSON
  -> Formal script / stage event draft
  -> Stage Node JSON
  -> RoundNodeSO + PopupEventSO
```

The stage node branch can run after episode planning when synopsis-level text is
enough, but final player-facing popup text should ideally come from the formal
script step.

Battle-related branches can run in parallel:

```text
Episode Battle Plan JSON
  -> BattleSO Input JSON
  -> BattleSO Asset
  -> PopupEvent reward payload or battle reward reference
```

## Existing Builder

Use the current editor builder shape:

```text
Assets/character_concepts/game_prompt_guide/stage/RoundNodeSO.md
Assets/character_concepts/game_prompt_guide/stage/PopupEventSO.md
Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageCreateGuide.md
Assets/Editor/resource_tools/stage/StageNodeBuilder.cs
Assets/Editor/resource_tools/stage/StagePopupEventBuilder.cs
```

`StageNodeBuilder.BuildFromJsonPath(...)` reads one stage node JSON file and:

1. builds the `PopupEventSO` chain from `nodes[]`;
2. builds one `RoundNodeSO`;
3. connects `RoundNodeSO.popupEvent` to the start `PopupEventSO`;
4. assigns each popup main image by matching `{eventId}.main`.

Default output folders:

```text
RoundNodeSO: Assets/Resources/stage_new/nodes
PopupEventSO: Assets/Resources/stage_new/popup_events
Popup main images: Assets/Resources/stage_new/popup_png
```

Popup main image naming:

```text
Assets/Resources/stage_new/popup_png/{eventId}.main.png
```

For example:

```text
Assets/Resources/stage_new/popup_png/node.ch1.episode1.001.main.png
```

`PopupEventBuilder` resolves this image by `PopupEventSO.eventId`, not by
`RoundNodeSO.nodeId`.

## Input Dependencies

Required identity fields:

```text
actId
chapterId
actGroupId
episodeId
stageNodeId
```

Required planning/script inputs:

```text
episodePlanningFile
storyContextFile
episodeCompositionFile
```

Recommended script input:

```text
episodeScriptFile
```

Optional battle inputs:

```text
battleJsonFile
battleSOPath
```

Use battle inputs only when a popup choice should start a battle.

## Output JSON Path

Recommended stage node JSON output:

```text
Assets/Resources/stage_new/{chapter_group}/episode.{episode_id}.json
```

Existing examples use:

```text
Assets/Resources/stage_new/chapter1/episode1.json
```

Use a stable `stageNodeId` that can become the `RoundNodeSO.nodeId`.

Recommended id pattern:

```text
stage.{act_short}.{chapter_short}.{episode_id}
```

Example:

```text
stage.act1.chapter01.episode01
```

## Stage Node JSON Shape

The builder supports this root shape:

```json
{
  "stageNodeId": "stage.act1.chapter01.episode01",
  "roundNodeType": "Event",
  "actId": "act.01",
  "chapterId": "chapter.01",
  "episodeId": "01",
  "episodeNumber": 1,
  "titleKo": "청운촌의 습격",
  "summary": "Runtime-facing summary for review.",
  "startNodeId": "node.act1.chapter01.episode01.001",
  "isRequired": true,
  "hiddenByDefault": false,
  "tags": ["story", "act1", "chapter01"],
  "nodes": []
}
```

Supported root fields:

| Field | Purpose |
|---|---|
| `stageNodeId` | Preferred source for `RoundNodeSO.nodeId`. |
| `roundNodeId` / `nodeId` | Fallback ids for `RoundNodeSO.nodeId`. |
| `roundNodeType` / `nodeType` | Parsed as `RoundNodeType`. |
| `startNodeId` | First `PopupEventSO.eventId`. |
| `isRequired` | Sets `RoundNodeSO.isRequired`. |
| `hiddenByDefault` | Sets `RoundNodeSO.hiddenByDefault`. |
| `tags` | Sets `RoundNodeSO.tags`. |
| `nodes` | Popup event node chain. |

Valid `RoundNodeType` values:

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

For story popup episodes, prefer:

```text
Event
RequiredSubEvent
```

## Popup Event Node Shape

Each `nodes[]` entry becomes one `PopupEventSO`.

```json
{
  "nodeId": "node.act1.chapter01.episode01.001",
  "nodeType": "narration",
  "locationId": "location.cheongun_village_entrance",
  "speakerId": "character.seojin",
  "speakerNameKo": "서진",
  "textKo": "Player-facing popup body text.",
  "nextNodeId": "node.act1.chapter01.episode01.002",
  "choices": []
}
```

Builder mapping:

- `nodeId` -> `PopupEventSO.eventId`
- `choices[].choiceId` -> `PopupEventChoice.choiceId`
- `choices[].nextNodeId` -> `PopupEventChoice.nextEvent`
- root `startNodeId` -> `RoundNodeSO.popupEvent`

Text is not stored directly in `PopupEventSO` fields at runtime.

`PopupEventSO.Title`, `PopupEventSO.Body`, choice label, description, and result
are resolved through `StringManager` keys based on `eventId` and `choiceId`.

Even so, JSON should keep `textKo`, `labelKo`, and `resultKo` as source text for
string table generation and review.

## Choice Shape

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

If a node has no `choices` but has `nextNodeId`, the builder creates an
automatic single next choice:

```text
{nodeId}.next
```

Use explicit choices when the player should see a selectable option.

Use `nextNodeId` without choices only for linear narration.

## Visible Conditions

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

Valid `conditionType` values:

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

## Rewards

`rewards[]` maps to `PopupEventRewardData`.

Current story planning should use gold only unless a specific downstream system
already requires another reward.

Simple gold reward:

```json
{
  "rewardType": "Gold",
  "rewardId": "reward.act1.chapter01.episode01.clear",
  "amount": 50
}
```

Route unlock reward:

```json
{
  "rewardType": "UnlockRoute",
  "rewardId": "stage.act1.chapter01.episode03_a"
}
```

Battle reward:

```json
{
  "rewardType": "SpecialBattle",
  "rewardId": "battle.act1.chapter01.01.rescue_villagers"
}
```

Battle rewards must reference a generated BattleSO by id. The concrete battle
definition belongs in a separate BattleSO input JSON, for example
`Assets/Resources/battle/{battle_group}/{battle_id}.json`. Stage node JSON must
not duplicate full battle payloads.

Valid `PopupEventRewardType` values include:

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

## Authoring Rules

- One stage node JSON should represent one map node / episode node.
- One root `stageNodeId` should build one `RoundNodeSO`.
- `nodes[]` may contain multiple popup pages connected by `nextNodeId`.
- Use stable ids; changing ids breaks saved references and string keys.
- Keep popup text player-facing only when formal script text is available.
- If only synopsis-level planning exists, write concise draft popup text and mark
  it as needing script review in planning status.
- Do not invent final battle data inside the stage node JSON.
- Do not put CharacterSO, BattleSO, or Sprite object references directly in JSON.
- Use ids and let builders resolve SO references.

## Validation

Before building SO assets:

- JSON syntax is valid.
- `stageNodeId` is present.
- `roundNodeType` parses as `RoundNodeType`.
- `startNodeId` exists in `nodes[]`.
- Every `nodeId` is unique.
- Every `choices[].choiceId` is unique within the JSON.
- Every `nextNodeId` exists in `nodes[]` unless intentionally external.
- Every `conditionType` parses as `PopupEventChoiceConditionType`.
- Every `rewardType` parses as `PopupEventRewardType`.
- Gold rewards use `Gold`, not lowercase `gold`.
- Battle rewards reference a generated or planned battle id.

After building SO assets:

- `RoundNodeSO.nodeId` equals `stageNodeId`.
- `RoundNodeSO.popupEvent` points to the start `PopupEventSO`.
- Every `PopupEventSO.eventId` matches a source `nodeId`.
- Choice `nextEvent` references are resolved when `nextNodeId` is local.
- Warnings from `StageNodeBuilder` and `PopupEventBuilder` are reported.
