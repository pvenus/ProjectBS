# Skill Icon Evaluation Slack Canvas Guide

## 1. Purpose

This guide extends `EvaluationSlackCanvasFormGuide.md` for a completed skill icon
evaluation.

```text
formVersion = evaluation_canvas_form_v1
evaluationDomain = skill
artifactType = skill_icon
artifactId = {equipmentId}
```

It maps an existing `SkillIconEvaluationGuide.md` report into the common Canvas
record. It does not evaluate, generate, normalize, copy, import, or fix an icon.

## 2. Required References

```text
Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationGuide.md
Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
```

## 3. Required Path Model

The domain alias `iconPath` must equal `stagingArtifactPath`.

```text
stagingArtifactPath =
{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png

evaluationWorkspacePath =
{evaluationRoot}/{equipmentId}

projectTargetPath =
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png

localCanvasDraftPath =
Assets/Doc/Evaluation/slack_canvas/v1/skill/skill_icon/{equipmentId}.canvas.md
```

The staging source is the evaluated, preserved icon. The project target is not
the evaluation input. If the report evaluated the project target directly,
record `process_violation` unless an explicit in-place policy is cited.

## 4. Required Evidence

- completed evaluation report and `evaluationReportSource`;
- `equipmentId` and skill source JSON;
- staging icon and SHA-256 when available;
- generation record;
- nearest-neighbor 32x32 preview;
- fatal failure result;
- six category scores and total;
- required corrections and regeneration method;
- project copy/hash/import evidence only when `promotionStatus=promoted`.

Record missing optional evidence as `Not Provided` or `Not Evaluated`. Do not
infer a Pass.

## 5. Score Breakdown Mapping

Copy these categories and maximums exactly from
`SkillIconEvaluationGuide.md`:

| Category | Max |
|---|---:|
| Skill Intent Readability | 25 |
| Project Style Match | 20 |
| Small-Size Silhouette | 20 |
| Slot and Grade Distinction | 15 |
| Palette and Contrast | 10 |
| Composition and Border Quality | 10 |

Pass contract:

```text
PASS: 85-100, no fatal failure, no unresolved required evidence
CONDITIONAL_PASS: 75-84, no fatal failure, explicit correction or approval needed
FAIL: below 75, fatal failure, or insufficient required evidence
```

Do not convert report labels only because capitalization differs.

## 6. Evidence Package Mapping

Add available rows inside the common `Evidence Package` section:

| Evidence Type | Source | Notes |
|---|---|---|
| Skill Source JSON | `{skillSourcePath}` | Meaning, grade, slot, targeting |
| Generation Record | `{generationRecordPath}` | PixelLab, normalization, candidate provenance |
| Preview 32 | `{preview32Path}` | Small-size readability |
| Frame Template | `{frameTemplatePath_or_Not Provided}` | Deterministic border evidence |
| Normalization Record | `{normalizationRecordPath_or_Not Provided}` | Background/frame/safe-area evidence |
| Sibling Icons | `{siblingIconPaths_or_Not Evaluated}` | Duplicate and loadout distinction |
| Lower Grade Icon | `{lowerGradeIconPath_or_Not Evaluated}` | Grade-family continuity |

## 7. Domain-Specific Notes

Add these rows inside the common `Domain-Specific Notes` section:

| Field | Value |
|---|---|
| Equipment ID | `{equipmentId}` |
| Skill Name | `{artifactName}` |
| Slot | `{slot}` |
| Grade | `{grade}` |
| Skill Source JSON | `{skillSourcePath}` |
| Resource Key | `{equipmentId}.icon` |
| Preview 32 Path | `{preview32Path}` |
| Composition Profile | `{compositionProfile_or_Not Provided}` |
| Background Mode | `{backgroundMode_or_Not Provided}` |
| Exact-Count Overlay | `{exactCountOverlayManifest_or_Not Applicable}` |
| Unity Meta Status | `{unityMetaStatus_or_Not Copied}` |

## 8. Findings and Actions

Preserve the report's fatal checks and correction routing:

```text
core_outline_rewrite
direction_sentence_replace
shape_only_rewrite
semantic_edit
exact_count_overlay
deterministic_normalization
small_size_recompose
```

Do not hide sibling duplication, wrong equipment identity, 32x32 collapse,
broken frame/background, or exact-count mismatch.

## 9. Promotion Rules

- `FAIL`: `not_promoted` or `blocked`; never copy.
- `CONDITIONAL_PASS`: remains unpromoted until explicit approval.
- `PASS`: may become `approved_for_promotion`.
- `promoted`: requires byte identity or equivalent copy verification between the
  preserved source and project target plus Unity import evidence.
- Canvas formatting never performs the copy.

## 10. Validation

- Common field names and all 11 common sections remain unchanged.
- `evaluationDomain`, `artifactType`, and `artifactId` use the fixed values.
- `iconPath == stagingArtifactPath`.
- Staging and project target are distinct.
- The six category scores total the report's overall score.
- Fatal failures and Critical/Major findings remain visible.
- Promotion status follows the common result matrix.
- Raw candidates are not represented as the evaluated final icon.
