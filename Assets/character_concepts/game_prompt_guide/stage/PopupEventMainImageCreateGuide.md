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
Assets/Resources/stage_new/popup_png/node.act1.chapter01.episode01.village_arrival.main.png
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
chapterGroup: {chapter_group}
actGroupId: {act_group_id}
episodeId: {episode_id}
popupName: {planning_popup_name}
popupId: {planning_popup_id}
eventId: {planning_popup_id}
stageNodeJsonFile: Assets/Resources/stage_new/{chapter_group}/episode.{episode_id}.json
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

For new content, `eventId` must equal the planning `popupId`, and the semantic id
suffix must equal `popupName`. Read `imagePolicy` from the matched planning popup
definition:

- `generate`: create a distinct `{popupId}.main.png`.
- `reuse`: require `imageSourcePopupId`, then copy its approved PNG byte-for-byte
  to `{popupId}.main.png`. This preserves the current builder's per-event lookup
  without regenerating the visual.
- `none`: do not generate an image.

Do not invent an event id in the image step.

Use:

- popup `bodyKo` or `textKo`
- `locationId`
- `speakerId` / `speakerNameKo`
- choice outcome intent, when the event is a decision point
- battle entry intent, when the choice starts a battle

The image should show the current dramatic moment of the popup, not the whole
episode summary.

## Visual Rules

- Use `StoryImageVisualGuide.md` for art style, composition, camera, lighting,
  storytelling, focus, character handling, and visual avoid rules.
- Use `StoryImageElementGuide.md` for historical period, environment,
  architecture, materials, props, social class, everyday life, and element avoid
  rules.
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
3:4
```

Recommended working resolution:

```text
960x1280
```

Higher resolutions are acceptable when the project import settings preserve the
Sprite cleanly.

## Reusable Story Image Guides

Read these guides before writing the event-specific image prompt:

```text
Assets/character_concepts/game_prompt_guide/stage/StoryImageVisualGuide.md
Assets/character_concepts/game_prompt_guide/stage/StoryImageElementGuide.md
```

Append only the event-specific key clue, core situation, place, period,
lighting, and composition details to the reusable guide direction. Do not weaken
the focus, historical grounding, character handling, or no-modern-object
requirements defined in those guides.

## Output Contract

Return:

```text
eventId
popupName
popupId
imagePolicy
imagePath or null when imagePolicy is none
spriteName
sourceStageNodeJsonFile
sourcePopupSummary
validationResult
```

Policy-specific fields:

- `generate`: return `visualPrompt` and `imageResolution`.
- `reuse`: do not write a new visual prompt; return `sourceImagePath`,
  `outputImagePath`, `sourceSha256`, and `copiedSha256`.
- `none`: return `skipped: true` and `skipReason: image_policy_none`; no image
  path, prompt, or resolution is required.

For `generate` or `reuse`, the final `imagePath` must be:

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

- For `generate` or `reuse`, the file exists at
  `Assets/Resources/stage_new/popup_png/{eventId}.main.png`.
- For `reuse`, source and destination PNG SHA-256 values match.
- For `none`, no image output is required and the result is reported as skipped.
- For new content, `eventId == popupId` and `popupId` matches the planning
  `popupName` formula.
- The file name does not use `stageNodeId`.
- For `generate`, the image does not include UI text and matches the popup
  event moment.
- For `reuse`, current-event suitability is approved upstream by planning
  `imagePolicy` and `imageSourcePopupId`. Do not alter the approved source image
  to make it match; validate identity and checksum only.
- The Sprite import name can become `{eventId}.main`.
- `PopupEventBuilder` can find the image by event id after Unity imports it.
