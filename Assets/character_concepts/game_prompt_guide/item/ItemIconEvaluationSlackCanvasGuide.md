# Item Icon Evaluation Slack Canvas Guide

## 1. Purpose

This guide extends `EvaluationSlackCanvasFormGuide.md` for a completed item icon
evaluation.

```text
formVersion = evaluation_canvas_form_v1
evaluationDomain = item
artifactType = item_icon
artifactId = {itemId}
```

The current scoring source is `ItemIconGenerationGuide.md`. If a dedicated
`ItemIconEvaluationGuide.md` is added later, its category names, maximums, fatal
conditions, and result thresholds replace only the score mapping in this domain
guide. The common form contract remains unchanged.

This guide formats an existing report. It does not evaluate, generate, extract,
copy, import, or fix an icon.

## 2. Required References

```text
Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
Assets/character_concepts/game_prompt_guide/item/ItemIconGenerationGuide.md
```

For relics, also use the approved planning or JSON path as evidence.

## 3. Required Path Model

The domain alias `iconPath` must equal `stagingArtifactPath`.

```text
stagingArtifactPath =
{evaluationRoot}/{itemId}/source/{itemId}.icon.png

evaluationWorkspacePath =
{evaluationRoot}/{itemId}

projectTargetPath =
Assets/Resources/item/icon/{itemId}.icon.png

localCanvasDraftPath =
Assets/Doc/Evaluation/slack_canvas/v1/item/item_icon/{itemId}.canvas.md
```

The raw 256x256 PixelLab sheet and extracted candidates belong in the evaluation
workspace. They are evidence, not the project target.

## 4. Required Evidence

- completed evaluation report and `evaluationReportSource`;
- `itemId`, category, and approved item source;
- preserved staging icon and SHA-256 when available;
- generation record and candidate score file;
- selected candidate and nearest-neighbor 64x64 preview;
- rejection-condition result;
- five category scores and total;
- project copy/hash/import evidence only when `promotionStatus=promoted`.

## 5. Score Breakdown Mapping

Until a dedicated evaluation guide exists, copy these categories and maximums
from `ItemIconGenerationGuide.md`:

| Category | Max |
|---|---:|
| Item identity and silhouette | 30 |
| Match to approved concept/effect | 20 |
| Legacy relic style consistency | 20 |
| Pixel-art craft and material readability | 15 |
| Composition, safe margin, 64x64 readability | 15 |

Current Pass contract:

```text
PASS: 85 or higher and no rejection condition
FAIL: below 85 or any rejection condition
```

If the completed report uses an approved later rubric, reproduce that rubric
exactly and cite its guide. Do not mix two rubrics in one record.

## 6. Evidence Package Mapping

Add available rows inside the common `Evidence Package` section:

| Evidence Type | Source | Notes |
|---|---|---|
| Item Source | `{itemSourcePath}` | Identity, role, concept, effect |
| Item Planning | `{itemPlanningFile_or_Not Provided}` | Presentation direction |
| Generation Record | `{generationRecordPath}` | PixelLab settings and provenance |
| Raw Sheet | `{rawSheetPath_or_Not Provided}` | Four-candidate source |
| Selected Candidate | `{selectedCandidatePath}` | Candidate used as staging source |
| Candidate Scores | `{candidateScoresPath}` | Selection evidence |
| Preview 64 | `{preview64Path}` | Inventory-size readability |
| Legacy References | `{legacyReferencePaths_or_Not Evaluated}` | Style comparison only |

## 7. Domain-Specific Notes

Add these rows inside the common `Domain-Specific Notes` section:

| Field | Value |
|---|---|
| Item ID | `{itemId}` |
| Item Category | `{itemCategory}` |
| Item Name | `{artifactName}` |
| Item Source | `{itemSourcePath}` |
| Resource Key | `{itemId}.icon` |
| Composition Profile | `{compositionProfile_or_Not Provided}` |
| Effect Cue | `{effectCue_or_None}` |
| Preview 64 Path | `{preview64Path}` |
| Selected Candidate | `{selectedCandidatePath}` |
| Unity Meta Status | `{unityMetaStatus_or_Not Copied}` |

## 8. Findings and Actions

Preserve rejection conditions and report routing:

- wrong object -> rewrite the primary-object sentence;
- weak silhouette or crop -> simplify parts and scale;
- wrong material -> rewrite material and palette;
- dominant effect -> remove or reduce the effect cue;
- extra objects or scenery -> simplify composition/exclusions;
- broad style failure -> rewrite the style sentence.

Do not record a raw sheet, a different candidate, or a legacy atlas cell as the
evaluated final icon.

## 9. Promotion Rules

- `FAIL`: `not_promoted` or `blocked`; never copy.
- `PASS`: may become `approved_for_promotion`.
- `promoted`: requires verified byte identity between preserved source and
  `projectTargetPath`, plus Unity import evidence.
- If a future rubric introduces `CONDITIONAL_PASS`, apply the common approval
  rule.
- Canvas formatting never performs the copy.

## 10. Validation

- Common field names and all 11 sections remain unchanged.
- `evaluationDomain`, `artifactType`, and `artifactId` use the fixed values.
- `iconPath == stagingArtifactPath`.
- Staging and project target are distinct.
- Score category names, maximums, and total match the cited rubric.
- Rejection conditions and Critical/Major findings remain visible.
- Legacy 2x2 sheets remain reference evidence only.
- Promotion status follows the common result matrix.
