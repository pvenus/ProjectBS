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
  -> PopupEvent SpecialBattle/BossBattle entry action
```

Battle-clear reward intent remains owned by the battle reward pipeline. It is
not converted into a popup `Gold` payload.

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
Assets/Resources/stage_new/popup_png/node.act1.chapter01.episode01.village_arrival.main.png
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

For newly planned episodes, `episodePlanningFile` must provide:

```text
story.sourceNarration.blocks[]
story.popupDefinitions[]
```

Each new popup definition owns the permanent `popupName` and `popupId` used by
the Stage JSON. The Stage conversion step copies these identities; it does not
invent, renumber, or rename them.

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
  "startNodeId": "node.act1.chapter01.episode01.village_arrival",
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
  "popupName": "village_arrival",
  "popupId": "node.act1.chapter01.episode01.village_arrival",
  "popupOrder": 100,
  "nodeId": "node.act1.chapter01.episode01.village_arrival",
  "nodeType": "narration",
  "locationId": "location.cheongun_village_entrance",
  "speakerId": "character.seojin",
  "speakerNameKo": "서진",
  "sourceNarrationId": "narration.act1.chapter01.episode01.village_arrival",
  "sourceTextKo": "Original narration preserved from planning or the formal script.",
  "textKo": "Player-facing popup body text derived from the original narration.",
  "textLayoutProfile": "stage_popup_v1",
  "textEditState": "generated",
  "imagePolicy": "generate",
  "nextNodeId": "node.act1.chapter01.episode01.black_smoke",
  "mainImageRequired": true,
  "imageDirection": "Player-facing image direction for this semantic popup event.",
  "choices": []
}
```

Builder mapping:

- planning `story.popupDefinitions[].popupId` -> `nodes[].nodeId`
- planning `story.popupDefinitions[].popupName`, `popupId`, `popupOrder`, and
  `imagePolicy` remain authoring metadata for review and validation.
- `nodeId` -> `PopupEventSO.eventId`
- `choices[].choiceId` -> `PopupEventChoice.choiceId`
- `choices[].nextNodeId` -> `PopupEventChoice.nextEvent`
- root `startNodeId` -> `RoundNodeSO.popupEvent`

Text is not stored directly in `PopupEventSO` fields at runtime.

`PopupEventSO.Title`, `PopupEventSO.Body`, choice label, description, and result
are resolved through `StringManager` keys based on `eventId` and `choiceId`.

Even so, JSON should keep `textKo`, `labelKo`, and `resultKo` as display text for
string table generation and review. When source narration is available, also
keep `sourceNarrationId` and `sourceTextKo` as provenance metadata. Builders may
ignore these metadata fields, but authoring automation must preserve them.

## Original Narration And Popup Display Text

Do not shorten or overwrite the canonical narration in episode planning or the
formal script to satisfy the current popup UI.

Treat popup text as a non-destructive display projection:

```text
canonical planning/script narration
  -> sourceNarrationId + sourceTextKo snapshot
  -> popup display textKo/bodyKo
  -> string table and PopupEventSO
```

Rules:

- `sourceTextKo` preserves the complete input narration without UI-driven
  shortening, manual line breaks, or omitted sentences.
- `textKo` or `bodyKo` contains the popup-ready display projection.
- One source narration may produce multiple popup nodes.
- Every derived popup node must keep the same `sourceNarrationId` so the full
  narration can be reconstructed and reviewed.
- `textEditState: generated` may be regenerated from the source.
- `textEditState: reviewed` requires review before replacement.
- `textEditState: manual_override` must not be overwritten automatically.
- If `manual_override` text violates `stage_popup_v1`, do not rewrite, wrap,
  split, or truncate it automatically. Stop generation and report
  `manual_override_conflict` with the affected node or choice id, observed line
  count, and maximum observed line width.
- When the source changes, keep the existing popup ids and mark affected display
  text for review instead of silently replacing manual edits.

## Popup Text Layout Profile

Use `textLayoutProfile: stage_popup_v1` for the current popup display rules.

The profile applies independently to every player-facing popup body:

- Maximum lines: `9`.
- Maximum width per line: `40` displayed characters.
- The newline character itself is not included in the width count.
- Spaces and visible punctuation are included in the width count.
- Count Unicode text elements (grapheme clusters), not UTF-8 bytes or UTF-16
  code units. A visible composed character counts as one character; spaces and
  punctuation each count as one, and `\n` counts only as a line boundary.
- Apply the same body budget to `textKo`, `bodyKo`, and `choices[].resultKo`.
- `choices[].labelKo` is rendered in a separate button area and is not included
  in the nine-line body budget. Keep it concise and apply the same word-boundary
  wrapping principles if a line break is necessary.

Line wrapping rules:

1. Prefer natural sentence, clause, Korean eojeol, and word boundaries.
2. Prefer a slightly shorter balanced line over filling all 40 characters when
   that produces more natural reading rhythm.
3. Insert explicit `\n` only after determining balanced semantic line breaks.
4. Do not split a Korean eojeol, English word, number, proper noun, or punctuation
   group in the middle merely to reach 40 characters.
5. If a sentence cannot fit cleanly, first rewrite only the display projection
   without changing its meaning. Preserve the unmodified wording in
   `sourceTextKo`.
6. If the complete display text still exceeds 9 lines, split it into multiple
   popup nodes at a sentence or paragraph boundary. Do not truncate the text.
7. Do not use ellipses as a substitute for omitted source content unless the
   ellipsis is an intentional part of the narration.
8. After wrapping, validate every produced line and the total line count.

Avoid character-count-only hard wrapping. Automatic wrapping that cuts a word or
eojeol in the middle is a validation failure even when the result technically
fits within 40 characters.

## Stable Popup And Image Identity

New popup identity must not depend on array position or a sequential index such
as `.001`, `.002`, or `.003`.

Use a permanent semantic id:

```text
node.{act}.{chapter}.{episode}.{semantic_key}
```

For newly planned content, `{semantic_key}` is exactly the immutable planning
`popupName`, and the complete value is the planning `popupId`.

Examples:

```text
node.act1.chapter01.episode01.village_arrival
node.act1.chapter01.episode01.black_smoke
node.act1.chapter01.episode01.rescue_choice
```

Rules:

- Once issued, a `nodeId` is permanent and must not be renumbered after popup
  insertion, deletion, or reordering.
- Existing legacy ids that already end in `.001`, `.002`, or another sequential
  suffix remain permanent. Do not rename them merely to adopt this guide because
  that would break string keys, references, images, and evaluation history.
- Only newly issued ids use the semantic pattern. A new popup inserted between
  legacy nodes receives a semantic id without changing either neighboring id.
- New Stage JSON must copy `popupId` from the matched planning
  `popupDefinitions[]` entry into `nodes[].nodeId`; do not recompute a different
  id or create a local-only popup name.
- Copy planning `popupDefinitions[].nextPopupId` to the matching Stage
  `nodes[].nextNodeId`. Copy each planning choice `nextPopupId` to its Stage
  `nextNodeId`. Preserve `null` for terminal flow and never translate a popup
  reference into an array index.
- `popupOrder` controls planning/review order only. Runtime flow is still
  expressed by `startNodeId`, `nextNodeId`, and choices.
- Popup order is expressed by array order and `nextNodeId`, never by parsing the
  id suffix.
- Inserted popup nodes receive a new semantic id; existing ids stay unchanged.
- Deleted ids are not reused for different content.
- Choices use stable semantic `choiceId` values and connect through
  `nextNodeId`, never a node array index.
- Because the current image builder resolves `{eventId}.main.png`, a permanent
  semantic `nodeId` also makes the popup image filename permanent.
- When text is split only for layout, decide explicitly whether the added popup
  needs a new image. Set `mainImageRequired: true` only when a distinct image is
  required; otherwise keep the additional popup image-neutral according to the
  supported runtime presentation.
- Never rename existing approved image files merely because a new popup was
  inserted before them.
- If the 9-line/40-character profile requires another popup but planning has no
  unused named popup definition for that segment, stop with
  `missing_popup_definition`. Return to episode planning to assign `popupName`,
  `popupId`, source mapping, popup type, order, and image policy. Do not generate
  an anonymous or indexed popup in Stage conversion.

## Choice Shape

```json
{
  "choiceId": "choice.act1.chapter01.episode01.rescue_choice.accept",
  "textKo": "마을 사람들을 구한다.",
  "labelKo": "마을 사람들을 구한다.",
  "resultKo": "서진은 검은 천의 무리 사이로 뛰어들었다.",
  "nextNodeId": "node.act1.chapter01.episode01.rescue_start",
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

Current story planning may use gold as the reward content, but reward content
must not be confused with reward execution ownership. Read `rewardOwner` and
`rewardTrigger` before creating any popup reward payload.

| Planning ownership | Stage behavior |
|---|---|
| `rewardOwner: popup` | May emit a concrete `Gold` entry in `choices[].rewards[]` when the trigger is explicitly popup-executed. |
| `rewardOwner: battle` | Must not emit popup `Gold`. Hand the intent to the battle reward pipeline. The popup may emit only `SpecialBattle`/`BossBattle` to enter the battle. |

Never infer popup ownership from `rewardType: gold`. For legacy input,
`gold_battle_reward`, `normal_battle_clear`, a battle-clear reason, or a choice
opening a battle is battle-owned. If ownership evidence conflicts or remains
unclear, fail with `ambiguous_reward_owner`.

Regression example for legacy Episode 1 planning:

```text
rewardType: gold
rewardIntent: gold_battle_reward
rewardScaleHint: normal_battle_clear
choice opens: battle
```

Required interpretation:

```text
rewardOwner: battle
popup rewards: SpecialBattle/BossBattle battle-entry action only
popup Gold: forbidden
battle reward handoff: gold_battle_reward
```

The word `gold` never overrides the battle-clear ownership evidence.

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

Battle entry action (not battle-clear payout):

```json
{
  "rewardType": "SpecialBattle",
  "rewardId": "battle.act1.chapter01.01.rescue_villagers"
}
```

This `SpecialBattle` entry starts the referenced battle. It does not grant the
battle's gold reward. Battle-owned gold is omitted from popup `rewards[]` and is
handed to the battle reward pipeline. The concrete battle definition belongs in
a separate BattleSO input JSON, for example
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
- Use permanent semantic ids; changing ids breaks saved references, string keys,
  popup image filenames, and evaluation history.
- Never use popup array indexes or sequential numbering for newly issued ids.
  Preserve existing legacy ids as permanent compatibility ids.
- Preserve canonical narration separately from its popup display projection.
- Apply `stage_popup_v1`: at most 9 lines and at most 40 displayed characters
  per body line.
- Wrap at sentence, clause, word, or Korean eojeol boundaries. Do not hard-wrap
  through a word or truncate source content.
- Split overlong display text into additional semantic popup nodes while keeping
  the original narration and all existing popup ids unchanged.
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
- Every newly issued `nodeId` is semantic and does not use its popup array
  position as an id. Pre-existing sequential legacy ids are preserved unchanged.
- Every new `nodeId` exactly matches a planning `popupDefinitions[].popupId`, and
  its semantic suffix exactly matches that entry's `popupName`.
- Every `choices[].choiceId` is unique within the JSON.
- Every `nextNodeId` exists in `nodes[]` unless intentionally external.
- Every derived popup keeps its `sourceNarrationId` and source text provenance.
- Every `textKo`, `bodyKo`, and `resultKo` has at most 9 lines.
- Every body/result line has at most 40 displayed characters.
- No manual or automatic line break cuts a word or Korean eojeol in the middle.
- Text exceeding the profile is split at a semantic boundary and is never
  silently truncated.
- A layout split never creates an unnamed Stage-only popup; missing planning
  identity fails as `missing_popup_definition`.
- Existing `manual_override` display text and existing popup ids are not
  overwritten during regeneration.
- Every `conditionType` parses as `PopupEventChoiceConditionType`.
- Every `rewardType` parses as `PopupEventRewardType`.
- Gold rewards use `Gold`, not lowercase `gold`.
- Every popup Gold reward has explicit popup ownership provenance.
- Battle-owned reward intent never becomes popup Gold.
- `SpecialBattle`/`BossBattle` is a battle-entry action and references a generated or planned battle id.
- Missing or conflicting ownership fails as `ambiguous_reward_owner`.

After building SO assets:

- `RoundNodeSO.nodeId` equals `stageNodeId`.
- `RoundNodeSO.popupEvent` points to the start `PopupEventSO`.
- Every `PopupEventSO.eventId` matches a source `nodeId`.
- Choice `nextEvent` references are resolved when `nextNodeId` is local.
- Warnings from `StageNodeBuilder` and `PopupEventBuilder` are reported.
