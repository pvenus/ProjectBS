# Popup Event Main Image Create Guide


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## Generated Image Storage Reference

Before generating, downloading, evaluating, promoting, or resolving a generated
image, read and apply:

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
```

This storage guide is mandatory. Its `Assets/ImagesGenerated` contract takes
precedence over legacy generated-image output paths under `Assets/Resources`.
Existing reference-only assets may remain in their documented legacy locations.

## Current Executable Contract

- **Guide type:** provider-generation workflow guide.
- **Responsibility:** resolve a popup-specific visual direction, generate the image at the selected provider, and return provider references plus a generation record.
- **Inputs:** resolved popup identity, planning context, `imagePolicy`, and approved provider prompt.
- **Preconditions:** `eventId == popupId` for new content and all cited source files are readable.
- **Handoff:** provider result references, generation parameters, future target identity, and a separate download/preservation request.
- **Mutation boundary:** do not download, evaluate, copy or save to the project target, import to Unity, create metadata, run a builder, or perform Git work.

Planning data controls popup identity and story meaning, the story image guides
control visual composition, and this guide controls provider generation only.
Any conflict stops execution with `generation_authority_conflict`.

## Purpose

Create one main illustration for each popup event that needs a visual.

The provider result is not stored in `PopupEventSO` JSON. Project storage and
builder mapping occur only after separate download, immutable evaluation, and
PASS-only promotion stages.

## Future Project Target Identity (Informational Only)

After PASS-only promotion, a separate promotion task stores popup main images here:

```text
Assets/ImagesGenerated/Stage/popup_main
```

Use this file name:

```text
{eventId}.main.png
```

Example:

```text
Assets/ImagesGenerated/Stage/popup_main/node.act1.chapter01.episode01.village_arrival.main.png
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
episodePlanningFile: AgentDocs/planning-data/story-planning/{act_group_id}/episode.{episode_id}.json
episodeScriptFile: AgentDocs/planning-data/story/{chapter_or_episode_script}.md
storyContextFile: AgentDocs/planning-data/story-planning/{act_group_id}/story_context.{act_group_id}.json
```

Optional:

```text
characterReferenceFiles:
  - AgentDocs/planning-data/character/act-plans/{act_group_id}/...
locationReferenceFiles:
  - AgentDocs/planning-data/location/...
styleReferenceImages:
  - AgentDocs/reference-assets/stage/popup_main/...
```

## Image Direction

Read the popup node from `stageNodeJsonFile` by `eventId`.

For new content, `eventId` must equal the planning `popupId`, and the semantic id
suffix must equal `popupName`. Read `imagePolicy` from the matched planning popup
definition:

- `generate`: create a distinct `{popupId}.main.png`.
- `reuse`: require `imageSourcePopupId`, return `reuse_requested`, and hand off
  the approved source identity. Do not copy a file in this generation stage.
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
AgentDocs/planning-guides/stage/StoryImageVisualGuide.md
AgentDocs/planning-guides/stage/StoryImageElementGuide.md
```

Append only the event-specific key clue, core situation, place, period,
lighting, and composition details to the reusable guide direction. Do not weaken
the focus, historical grounding, character handling, or no-modern-object
requirements defined in those guides.

## Generation Handoff Contract

Return:

```text
eventId
popupName
popupId
imagePolicy
futureProjectTargetPath or null when imagePolicy is none
futureSpriteName
sourceStageNodeJsonFile
sourcePopupSummary
providerResultRefs
generationRecord
downloadHandoff
```

Policy-specific fields:

- `generate`: return `visualPrompt` and `imageResolution`.
- `reuse`: do not write a new visual prompt or copy a file; return
  `imageSourcePopupId` and `reuse_requested: true`.
- `none`: return `skipped: true` and `skipReason: image_policy_none`; no image
  path, prompt, or resolution is required.

For `generate` or `reuse`, the future project target identity is:

```text
Assets/ImagesGenerated/Stage/popup_main/{eventId}.main.png
```

## Legacy Builder Mapping Appendix (Non-executable)

This section and Validation below describe downstream consumers only. They must
not be executed by the generation task. Builder checks belong after immutable
evaluation and PASS-only promotion.

`PopupEventBuilder` uses:

```text
eventId -> {eventId}.main -> PopupEventSO.mainImage
```

The required target search root is:

```text
Assets/ImagesGenerated/Stage/popup_main
```

Before running the builder, inspect `StagePopupEventBuilder`'s configured main
image folder. If it still points to `Assets/Resources/stage_new/popup_png`, stop
with `builder_path_migration_required`. Do not duplicate the generated image into
the legacy Resources path as a workaround.

This mapping is per popup event. Do not use `stageNodeId` for popup main image
names.

## Validation

Before finishing, verify:

- For `generate` or `reuse`, the file exists at
  `Assets/ImagesGenerated/Stage/popup_main/{eventId}.main.png`.
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
- The builder target root is `Assets/ImagesGenerated/Stage/popup_main`; otherwise
  the builder step is blocked as `builder_path_migration_required`.
