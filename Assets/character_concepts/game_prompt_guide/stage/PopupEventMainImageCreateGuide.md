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

- Use the common visual style and reusable prompt core defined below.
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

## Common Visual Style

Apply this style to every popup event main image:

- Cinematic, semi-realistic anime illustration.
- Detailed ink linework with a rough pixel texture and painterly finish.
- Muted earthy color palette.
- Expressive story-cut lighting such as warm rim light, layered soft shadows,
  dust in the air, and natural light filtering through the scene.
- The key clue and core situation remain sharply readable while the surrounding
  environment stays softly out of focus.

### Composition

- Use a close-up composition around one key clue and one core situation.
- Treat the key clue as the main subject.
- Use objects, traces, hands, posture, tools, clothing, and spatial context for
  environmental storytelling.
- Let the background establish story and period context without dominating.
- Do not combine multiple story beats in one image.
- Keep scene-specific descriptions in the generated prompt, not in this reusable
  guide.

### Character Handling

- Include characters only when the current situation requires them.
- Avoid portrait composition and do not emphasize faces or facial expressions.
- Express emotion through hands, posture, clothing folds, silhouettes, cropped
  body framing, tools, and interaction with the environment.
- Treat human figures as supporting environmental storytelling elements.
- Characters must not replace the key clue or core situation as the focal point.

### Historical Authenticity

- Infer the historical period, environment, social class, and everyday life from
  the supplied story context.
- Use historically grounded Korean period details when appropriate.
- Prefer weathered, imperfect, handmade materials such as wood, straw, hemp,
  bamboo, earthenware, aged hanji, and rough cloth.
- Include only practical and story-relevant objects.
- Avoid modern objects, clean manufactured items, decorative fantasy props,
  readable text, and watermarks.

## Reusable Prompt Core

Use this common style core, then append only the event-specific key clue, core
situation, place, period, lighting, and composition details:

```text
Cinematic semi-realistic anime illustration with detailed ink linework, rough
pixel texture, painterly finish, and a muted earthy color palette. Focus tightly
on the key clue and core situation, keeping them sharp while the surrounding
environment remains softly out of focus. Use expressive dramatic story-cut
lighting with warm rim light, layered soft shadows, airborne dust, and natural
light filtering through the scene. Infer and reflect the historical period,
environment, handmade materials, social class, and everyday life from the story
context. Characters, when required, remain supporting environmental storytelling
elements without portrait emphasis or detailed facial expression.
```

For automation, replace only the event-specific key clue, core situation, place,
and period context. Do not weaken the focus, historical grounding, character
handling, or no-modern-object requirements.

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
