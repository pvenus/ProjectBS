# Popup Event Main Image Create Guide

## Purpose

Create one main illustration for each popup event that needs a visual.

The output image is not stored in `PopupEventSO` JSON. It is saved as a Sprite
asset under a deterministic path, and the editor builder maps it to
`PopupEventSO.mainImage` by event id.

## Output Path

Store popup main images here:

```text
Assets/Resources/stage_new/popup_png
```

Use this file name:

```text
{eventId}.main.png
```

Example:

```text
Assets/Resources/stage_new/popup_png/node.ch1.episode1.001.main.png
```

The imported Sprite name should be:

```text
{eventId}.main
```

## Input

Required:

```text
actId: {act_id}
chapterId: {chapter_id}
episodeId: {episode_id}
eventId: {event_id}
stageNodeJsonFile: Assets/Resources/stage_new/{chapter_group}/episode{episode_number}.json
```

Recommended:

```text
episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
episodeScriptFile: Assets/Doc/Story/{chapter_or_episode_script}.md
storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
```

Optional:

```text
characterReferenceFiles:
  - Assets/Doc/Character/{act_group_id}/...
locationReferenceFiles:
  - Assets/Doc/Location/...
styleReferenceImages:
  - Assets/Resources/stage_new/popup_png/reference/...
```

## Image Direction

Read the popup node from `stageNodeJsonFile` by `eventId`.

Use:

- popup `bodyKo` or `textKo`
- `locationId`
- `speakerId` / `speakerNameKo`
- choice outcome intent, when the event is a decision point
- battle entry intent, when the choice starts a battle

The image should show the current dramatic moment of the popup, not the whole
episode summary.

## Visual Rules

- Use a pixel-game-friendly painted illustration style.
- Keep the image readable behind popup UI overlays.
- Avoid tiny important details near the edges.
- Prefer one clear focal moment: character arrival, discovery, threat, choice,
  reward, or route reveal.
- Do not include UI text, captions, speech bubbles, buttons, or labels.
- Do not include final combat VFX unless the popup itself is a battle entry
  or battle aftermath event.
- Keep characters and props small enough to leave room for the popup layout.

Recommended aspect ratio:

```text
16:9
```

Recommended working resolution:

```text
1280x720
```

Higher resolutions are acceptable when the project import settings preserve the
Sprite cleanly.

## Output Contract

Return:

```text
eventId
imagePath
spriteName
sourceStageNodeJsonFile
sourcePopupSummary
visualPrompt
validationResult
```

The final `imagePath` must be:

```text
Assets/Resources/stage_new/popup_png/{eventId}.main.png
```

## Builder Mapping

`PopupEventBuilder` uses:

```text
eventId -> {eventId}.main -> PopupEventSO.mainImage
```

It searches under:

```text
Assets/Resources/stage_new/popup_png
```

This mapping is per popup event. Do not use `stageNodeId` for popup main image
names.

## Validation

Before finishing, verify:

- The file exists at `Assets/Resources/stage_new/popup_png/{eventId}.main.png`.
- The file name does not use `stageNodeId`.
- The image does not include UI text.
- The image matches the popup event moment.
- The Sprite import name can become `{eventId}.main`.
- `PopupEventBuilder` can find the image by event id after Unity imports it.
