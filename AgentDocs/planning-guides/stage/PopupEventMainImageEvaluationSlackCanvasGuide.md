# Popup Event Main Image Evaluation Slack Canvas Guide


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

## 1. Purpose

This guide extends `EvaluationSlackCanvasFormGuide.md` for a completed story
popup main-image evaluation.

```text
formVersion = evaluation_canvas_form_v1
evaluationDomain = stage
artifactType = story_popup_main_image
artifactId = {eventId}
```

It maps an existing `PopupEventMainImageEvaluationGuide.md` report into the
common Canvas record. It does not evaluate, generate, edit, copy, import, or fix
an image or story document.

## 2. Required References

```text
AgentDocs/planning-guides/prompt/EvaluationSlackCanvasFormGuide.md
AgentDocs/planning-guides/stage/PopupEventMainImageEvaluationGuide.md
AgentDocs/planning-guides/stage/PopupEventMainImageCreateGuide.md
AgentDocs/planning-guides/stage/StoryImageVisualGuide.md
```

## 3. Required Path Model

The domain alias `imagePath` must equal `stagingArtifactPath`.

```text
stagingArtifactPath =
{evaluationRoot}/{eventId}/source/{eventId}.main.png

evaluationWorkspacePath =
{evaluationRoot}/{eventId}

projectTargetPath =
Assets/ImagesGenerated/Stage/popup_main/{eventId}.main.png

localCanvasDraftPath =
AgentDocs/planning-data/evaluation/slack_canvas/v1/stage/story_popup_main_image/{eventId}.canvas.md
```

For `imagePolicy=none`, use:

```text
stagingArtifactPath = Not Applicable
projectTargetPath = Not Applicable
promotionStatus = not_applicable
result = SKIPPED
```

For `generate` and `reuse`, evaluating the project target directly is a process
violation under this staged workflow. `reuse` may stage a byte-identical approved
source, but the future project destination remains separate.

## 4. Required Evidence

- completed evaluation report and `evaluationReportSource`;
- `eventId`, `popupId`, `popupName`, and `imagePolicy`;
- episode planning and stage node JSON;
- staged image and hash when applicable;
- story context, popup text, character/location evidence when available;
- score breakdown, findings, and required corrections;
- source/copy hash evidence for `reuse` when available;
- project copy/hash/import evidence only when `promotionStatus=promoted`.

Missing optional story evidence is `Not Provided` or `Not Evaluated`. Do not
invent narrative facts.

## 5. Score Breakdown Mapping

For generated_image_evaluation_v1, copy category IDs, names, maximums, scores,
status, evidence, result, and pass criteria exactly from evaluation_result.json.
PopupEventMainImageEvaluationGuide.md as captured by that result is the scoring
authority. This Canvas guide must not apply a separate hardcoded rubric.

The static mapping below is legacy-only. Use it only when formatting a
completed legacy report that explicitly contains that exact rubric. Do not use
it for a new evaluation and do not combine it with a current normalized result.
A category or threshold conflict is evaluation_contract_conflict and blocks
Canvas formatting.

The following historical mapping was previously associated with
`PopupEventMainImageEvaluationGuide.md`:

| Category | Max |
|---|---:|
| Planning and Story Fidelity | 20 |
| Current Popup Moment and Situation | 15 |
| Character Expression and Identity | 15 |
| World, Setting, and Element Accuracy | 10 |
| Art Style Consistency | 15 |
| Composition and Popup Usability | 10 |
| Technical Asset Contract | 5 |
| Continuity and Reuse Consistency | 5 |
| Evidence and Report Quality | 5 |

Result contract:

```text
PASS: 90 or higher, no fatal failure, no Major finding
CONDITIONAL_PASS: 80-89, no fatal failure, only Minor/Suggestion findings
FAIL: below 80, fatal failure, or any Major/Critical finding
SKIPPED: imagePolicy=none
```

## 6. Evidence Package Mapping

Add available rows inside the common `Evidence Package` section:

| Evidence Type | Source | Notes |
|---|---|---|
| Stage Node JSON | `{stageNodeJsonFile}` | Popup event identity and resource key |
| Episode Planning | `{episodePlanningFile}` | Planned moment and image policy |
| Story Context | `{storyContextFile_or_Not Provided}` | Narrative continuity |
| Episode Script | `{episodeScriptFile_or_Not Provided}` | Popup text and immediate beat |
| Character References | `{characterReferencePaths_or_Not Evaluated}` | Identity and expression |
| Location References | `{locationReferencePaths_or_Not Evaluated}` | Setting accuracy |
| Sibling Images | `{siblingPopupImagePaths_or_Not Evaluated}` | Visual continuity |
| Reuse Source | `{reuseSourcePath_or_Not Applicable}` | Reuse identity and hash |

Use short evidence summaries. Do not paste full story documents into Canvas.

## 7. Domain-Specific Notes

Add these rows inside the common `Domain-Specific Notes` section:

| Field | Value |
|---|---|
| Event ID | `{eventId}` |
| Popup ID | `{popupId}` |
| Popup Name | `{popupName}` |
| Image Policy | `{generate/reuse/none}` |
| Resource Key | `{eventId}.main` |
| Planned Moment | `{short_summary_or_Not Provided}` |
| Referenced Popup Text | `{short_excerpt_or_Not Provided}` |
| Reuse Source Hash | `{sourceHash_or_Not Applicable}` |
| Staged Reuse Hash | `{stagedHash_or_Not Applicable}` |
| Dimensions | `{width}x{height_or_Not Provided}` |
| Unity Meta Status | `{unityMetaStatus_or_Not Copied}` |

## 8. Findings and Actions

Keep fatal story contradictions and Critical/Major findings at the top of the
common `Findings` section. Preserve whether the required action is:

- planning or popup-text clarification;
- targeted prompt revision;
- image regeneration;
- reuse-source correction;
- evidence collection;
- re-evaluation.

Do not convert a story contradiction into an optional improvement.

## 9. Promotion Rules

- `FAIL`: `not_promoted` or `blocked`; never copy.
- `CONDITIONAL_PASS`: remains unpromoted until explicit approval.
- `PASS`: may become `approved_for_promotion`.
- `promoted`: requires verified copy integrity and Unity import evidence.
- `SKIPPED`: uses `not_applicable`.
- Canvas formatting never performs the copy.

## 10. Validation

- Common field names and all 11 sections remain unchanged.
- Fixed domain, artifact type, and artifact ID are correct.
- `imagePath == stagingArtifactPath` for `generate` and `reuse`.
- Staging and project target are distinct.
- The normalized result's domain category scores and maximums match the report
  and total correctly when a numeric total is used.
- Fatal conditions and Critical/Major findings remain visible.
- `imagePolicy=none` maps to `SKIPPED/not_applicable`.
- Promotion status follows the common result matrix.
