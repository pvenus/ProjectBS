# Popup Event Main Image Evaluation Guide

## 1. Purpose

This guide defines read-only evaluation for one story popup main image.

The evaluation checks story fidelity, immediate situation, character intent,
world accuracy, visual style, popup usability, and technical readiness. It does
not generate, edit, rename, copy, import, promote, or delete an image.

## 2. Staged Evaluation Model

Evaluate the preserved local source before it is copied into the project:

```text
stagingArtifactPath =
{evaluationRoot}/{eventId}/source/{eventId}.main.png

evaluationWorkspacePath =
{evaluationRoot}/{eventId}

projectTargetPath =
Assets/Resources/stage_new/popup_png/{eventId}.main.png
```

`projectTargetPath` is the intended destination only. It is not the normal
evaluation input. If staging and target resolve to the same file, report
`staging_target_path_collision` and stop.

For `imagePolicy=none`, no image quality evaluation is performed and the result
is `SKIPPED`.

## 3. Required Inputs

```text
eventId
popupId
popupName
imagePolicy
stagingArtifactPath
evaluationWorkspacePath
projectTargetPath
stageNodeJsonFile
episodePlanningFile
```

Recommended:

```text
storyContextFile
episodeScriptFile
characterReferencePaths
locationReferencePaths
siblingPopupImagePaths
styleReferenceImagePaths
reuseSourcePath
reuseSourceHash
stagingHash
```

Missing recommended evidence is `Not Evaluated`. Do not invent story facts.

## 4. Required References

```text
Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageCreateGuide.md
Assets/character_concepts/game_prompt_guide/stage/StoryImageVisualGuide.md
Assets/character_concepts/game_prompt_guide/stage/StoryImageElementGuide.md
Assets/character_concepts/game_prompt_guide/stage/PopupEventSO.md
Assets/character_concepts/game_prompt_guide/stage/EpisodeStageNodeCreateGuide.md
```

## 5. Image Policy

### generate

Evaluate the staged generated image against the current popup moment, story
context, visual style, composition, and technical contract.

### reuse

Evaluate approved reuse intent and byte identity first. Do not fail an explicitly
approved reuse merely because the image was created for another popup. Still
report identity, story, or continuity contradictions.

### none

Confirm that planning intentionally omits an image. Return `SKIPPED`; do not
invent an image path or score.

## 6. Fatal Failure Conditions

Any fatal condition produces `FAIL` regardless of score:

- missing, unreadable, corrupt, blank, or unusably cropped staged image;
- staging and project target are the same file;
- wrong event, popup, character, location, or story beat;
- contradiction of an explicit planning or stage-node fact;
- wrong identity, faction, age group, or impossible character role;
- visible UI, captions, speech bubbles, watermark, logo, or prompt text;
- modern, futuristic, or unrelated setting elements;
- unusable aspect ratio or composition;
- `reuse` source and staged hashes differ when both are available.

## 7. Scoring

Score after fatal checks:

| Category | Max | Core question |
|---|---:|---|
| Planning and Story Fidelity | 20 | Does it preserve the planned story beat without invented outcomes? |
| Current Popup Moment and Situation | 15 | Does one immediate dramatic moment read clearly? |
| Character Expression and Identity | 15 | Are identity, role, pose, gaze, and emotion correct? |
| World, Setting, and Element Accuracy | 10 | Are location, props, time, weather, and material culture appropriate? |
| Art Style Consistency | 15 | Does it match the approved cinematic story-image style? |
| Composition and Popup Usability | 10 | Does the 3:4 image remain clear under popup UI? |
| Technical Asset Contract | 5 | Is the staged PNG technically usable and correctly identified? |
| Continuity and Reuse Consistency | 5 | Does it maintain sibling or approved reuse continuity? |
| Evidence and Report Quality | 5 | Are evidence, inference, severity, and actions traceable? |

### 7.1 Style Expectations

- cinematic semi-realistic anime presentation;
- soft charcoal-like ink, rough pixel texture, and painterly finish;
- muted earthy palette with selective accents;
- restrained local contrast and one clear focal moment.

Reject dominant photorealism, glossy splash-art finish, hard cel shading, bright
generic fantasy saturation, or unrelated AI illustration polish.

### 7.2 Character-Absence Rule

If planning intentionally shows no character, score Character Expression and
Identity by whether that absence is justified and the environment carries the
required narrative expression.

## 8. Result Rules

```text
PASS:
- 90-100
- no fatal failure
- no Major or Critical finding

CONDITIONAL_PASS:
- 80-89
- no fatal failure
- only Minor or Suggestion findings

FAIL:
- below 80
- or fatal failure
- or any Major/Critical finding

SKIPPED:
- imagePolicy=none
```

Do not round upward to reach a threshold.

## 9. Severity

- `Critical`: fatal or unusable story/asset contradiction.
- `Major`: misleading story, damaged identity/style, or required regeneration.
- `Minor`: localized issue that does not invalidate the image.
- `Suggestion`: optional polish.

## 10. Required Evaluation Output

```md
# Popup Event Main Image Evaluation

## Target

- Event ID:
- Popup ID:
- Popup Name:
- Image Policy:
- Staging Artifact Path:
- Evaluation Workspace Path:
- Project Target Path:
- Stage Node JSON:
- Episode Planning:

## Result

- Overall Score:
- Result:
- Hard Fail:
- Highest Severity:

## Score Breakdown

- Planning and Story Fidelity: /20
- Current Popup Moment and Situation: /15
- Character Expression and Identity: /15
- World, Setting, and Element Accuracy: /10
- Art Style Consistency: /15
- Composition and Popup Usability: /10
- Technical Asset Contract: /5
- Continuity and Reuse Consistency: /5
- Evidence and Report Quality: /5

## Findings

- [Severity] Title
  - Evidence:
  - Impact:
  - Recommendation:

## Required Actions

- 1:

## Optional Improvements

- None

## Evidence Reviewed

-

## Re-evaluation Plan

- Expected Score After Fix:
- Pass Likelihood:
- Remaining Risk:
- Re-evaluation Trigger:
```

## 11. Validation

- Evaluation is read-only.
- The staged source, not the project target, is inspected.
- Every category score is within its maximum.
- The total equals the category sum.
- Fatal failure always produces `FAIL`.
- Missing optional evidence does not become invented evidence.
- Findings distinguish confirmed facts from inference.
- Project copy and promotion are not performed.

## 12. Failure Types

```text
missing_staging_image
unreadable_image
missing_stage_node_json
missing_episode_planning_file
popup_event_not_found
popup_definition_not_found
event_id_mismatch
invalid_image_policy
missing_reuse_source
checksum_mismatch
insufficient_story_context
insufficient_visual_context
staging_target_path_collision
report_write_failed
```
