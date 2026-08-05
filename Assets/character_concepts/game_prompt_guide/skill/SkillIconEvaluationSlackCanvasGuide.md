# Skill Icon Evaluation Slack Canvas Guide


## Master Concept Reference

Before using this document, read and apply:

Assets/character_concepts/game_prompt_guide/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## 1. Purpose

This guide joins two strictly separated phases:

1. evaluate a preserved skill icon with `SkillIconEvaluationGuide.md`; and
2. map that immutable result into `evaluation_canvas_form_v1`.

It also supports `format_existing`, which formats a completed report without
re-evaluating it. Neither phase generates or edits an icon, normalizes pixels,
copies an artifact into Unity, changes `.meta`, or performs Git work.

```text
workflowMode = evaluate_and_format | format_existing
formVersion = evaluation_canvas_form_v1
evaluationDomain = skill
artifactType = skill_icon
artifactId = {equipmentId}
```

## 2. Source-of-Truth Order

Read and apply these references in order:

```text
Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationGuide.md
Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationSlackCanvasGuide.md
Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
```

`SkillIconEvaluationGuide.md` owns evaluation facts, fatal failures, scoring,
result thresholds, and correction routing. The common Canvas guide owns the 11
section names, order, field meanings, promotion states, and Slack publication
rules. This guide only defines the skill-icon mapping and storage model.

Never relax an evaluation rule to make a Canvas record easier to publish.

## 3. Current-PC Path Resolution

The evaluator must resolve every path on the current PC before reading evidence.
Do not reuse a recorded home directory, user name, temporary visualization
folder, or project checkout path from another agent PC.

The canonical evaluation root for this workflow is:

```text
C:\github\design_evaluation\skill_icon
```

The project root is supplied or discovered independently. A valid project root
must contain the referenced `Assets` tree. The fixed evaluation root must not be
silently relocated into the Unity project.

If a supplied absolute path belongs to another PC:

- resolve the same project-relative file under the current `projectRoot` when
  the relative identity is unambiguous;
- otherwise stop with `other_pc_path_detected`;
- never treat the stale path as evidence for PASS.

## 4. Evaluation Workspace Layout

Use one workspace per full `equipmentId`:

```text
C:\github\design_evaluation\skill_icon\
  README.md
  {equipmentId}\
    source\
      {equipmentId}.icon.png
    input\
      evaluation_input.json
    evaluation\
      evidence\
        preview32.png
      evaluation_result.json
      evaluation_report.md
      evaluation_canvas.md
```

Required resolved paths:

```text
evaluationRoot =
C:\github\design_evaluation\skill_icon

evaluationWorkspacePath =
{evaluationRoot}\{equipmentId}

stagingArtifactPath =
{evaluationWorkspacePath}\source\{equipmentId}.icon.png

evaluationResultPath =
{evaluationWorkspacePath}\evaluation\evaluation_result.json

evaluationReportPath =
{evaluationWorkspacePath}\evaluation\evaluation_report.md

localCanvasDraftPath =
{evaluationWorkspacePath}\evaluation\evaluation_canvas.md

projectTargetPath =
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
```

`iconPath` is a domain alias for `stagingArtifactPath`. The staging source is the
exact preserved artifact that is evaluated. The project target is never the
evaluation input. A path collision is a `process_violation` and blocks
promotion.

## 5. Workspace Write Boundary

The evaluation workspace may receive only evaluation records and evidence
copies explicitly prepared for evaluation. Evaluation itself is read-only with
respect to:

- the preserved source PNG;
- skill JSON and generation records;
- frame, normalization, overlay, preview, sibling, and lower-grade evidence;
- all Unity project files and `.meta` files.

Do not crop, resize, pad, recompress, recolor, rename, or replace the preserved
source. If the exact source is not already staged, stop with `missing_icon`
instead of substituting a gallery preview, raw candidate, Unity target, or
placeholder.

## 6. Evaluation Phase Contract

For `workflowMode=evaluate_and_format`, execute the entire
`SkillIconEvaluationGuide.md` contract. At minimum, preserve the following.

### 6.1 Identity and Technical Evidence

- valid source JSON and exact `equipmentId` match;
- exact preserved path and filename;
- decodable single 80 x 80 RGBA PNG;
- SHA-256;
- generation record proving the required generation/preservation pipeline;
- current-PC 80 x 80 frame template, normalization record, and nearest-neighbor
  32 x 32 preview when the contract requires them.

Missing required evidence is not a visual FAIL guess. It is
`insufficient_evidence`, and the result cannot be PASS.

### 6.2 Semantic and Structural Evidence

Reclassify from source JSON:

```text
slotFamily
visualFamily
primarySymbol
secondaryEffect
composition
elementFamily
roleFamily
paletteFamily
intensity
expectedDirection
primaryFragmentShape
mandatorySemanticEffect
exactCountElements
```

Check that:

- direction matches the source and recorded composition profile;
- a fragment has not become a complete head, character, creature, altar, or
  unrelated scene;
- exact-count elements match the overlay manifest;
- flat/contextual background rules and the central 64 x 64 safe area hold;
- primary size, meaningful line thickness, spacing, particles, arcs, and rings
  satisfy the source-size and 32 x 32 survival contract;
- lower-grade identity, sibling distinction, and duplicate hashes are checked
  when evidence exists.

### 6.3 Fatal Failures Before Scoring

Run all fatal checks in sections 8.1-8.4 of
`SkillIconEvaluationGuide.md` before assigning scores. Any fatal failure makes
the result FAIL regardless of total.

### 6.4 Exact Score Mapping

| Category | Max |
|---|---:|
| Skill Intent Readability | 25 |
| Project Style Match | 20 |
| Small-Size Silhouette | 20 |
| Slot and Grade Distinction | 15 |
| Palette and Contrast | 10 |
| Composition and Border Quality | 10 |

Result contract:

```text
PASS:
- 85-100
- no fatal failure
- no unresolved required evidence

CONDITIONAL_PASS:
- 75-84
- no fatal failure
- explicit correction or approval is still required

FAIL:
- below 75
- or any fatal failure
- or insufficient required evidence
```

Do not round up to a threshold or convert labels during Canvas formatting.

## 7. Structured Evaluation Result

`evaluate_and_format` saves both:

- `evaluation_report.md`, using the human-readable structure from
  `SkillIconEvaluationGuide.md`; and
- `evaluation_result.json`, containing the same decision in machine-readable
  form.

Required JSON facts include:

```text
schemaVersion
equipmentId
skillSourcePath
stagingArtifactPath
sha256
dimensions
fatalFailure
fatalFailureChecks
scores and category maximums
totalScore
result
highestSeverity
findings
requiredActions
correctionMethods
regenerationRequired
promotionStatus
passForUnityCopy
evidence
evaluatedAt
reviewer
```

The report and JSON must agree exactly. `passForUnityCopy` may be true only for
PASS. This field is eligibility evidence, not authorization to copy.

For `format_existing`, `evaluationReportSource` is required. Do not re-score or
rewrite it. Validate its artifact identity and staging hash against the current
workspace before formatting.

## 8. Correction Preservation

For every failed or deducted item preserve:

```text
Observed issue
Evidence at 80 x 80 or 32 x 32
Expected rule
Required correction
Regeneration required: yes | no
Correction method
```

Allowed correction methods remain:

```text
core_outline_rewrite
direction_sentence_replace
shape_only_rewrite
semantic_edit
exact_count_overlay
deterministic_normalization
small_size_recompose
```

Do not replace a precise pipeline correction with a longer `prompt_only`
negative list. Canvas formatting must not hide wrong direction, fragment
reconstruction, 32 x 32 collapse, frame/background failure, exact-count
mismatch, sibling duplication, or identity mismatch.

## 9. Canvas Mapping

Use all common sections once, in this exact order:

1. `Record Metadata`
2. `Result Summary`
3. `Target Artifact`
4. `Evidence Package`
5. `Score Breakdown`
6. `Findings`
7. `Required Actions`
8. `Optional Improvements`
9. `Domain-Specific Notes`
10. `Re-evaluation Plan`
11. `Change Log`

### 9.1 Result Summary

Copy, do not recompute:

- result and total score;
- fatal failure/Hard Fail;
- highest severity;
- the exact PASS contract;
- next action.

### 9.2 Evidence Package

Add one row for each available source:

| Evidence Type | Source | Notes |
|---|---|---|
| Evaluation Report | `{evaluationReportPath}` | Immutable source decision |
| Structured Result | `{evaluationResultPath}` | Machine-readable mirror |
| Skill Source JSON | `{skillSourcePath}` | Meaning, grade, slot, targeting |
| Preserved Icon | `{stagingArtifactPath}` | Exact evaluated bytes and SHA-256 |
| Generation Record | `{generationRecordPath}` | Candidate and pipeline provenance |
| Preview 32 | `{preview32Path}` | Small-size readability |
| Frame Template | `{frameTemplatePath_or_Not Provided}` | Border evidence |
| Normalization Record | `{normalizationRecordPath_or_Not Provided}` | Background/frame/safe-area evidence |
| Exact-Count Overlay | `{manifest_or_Not Applicable}` | Count evidence |
| Sibling Icons | `{paths_or_Not Evaluated}` | Loadout distinction and hashes |
| Lower Grade Icon | `{path_or_Not Evaluated}` | Grade-family continuity |
| Promotion Approval | `{source_or_Not Provided}` | Never inferred from PASS |

Missing optional evidence uses `Not Provided` or `Not Evaluated`. Missing
required evidence remains visible as a blocking finding.

### 9.3 Domain-Specific Notes

| Field | Value |
|---|---|
| Equipment ID | `{equipmentId}` |
| Skill Name | `{artifactName}` |
| Slot | `{slot}` |
| Grade | `{grade}` |
| Skill Source JSON | `{skillSourcePath}` |
| Resource Key | `{equipmentId}.icon` |
| Preview 32 Path | `{preview32Path}` |
| Expected / Actual Direction | `{value}` |
| Fragment Structure | `{value}` |
| Composition Profile | `{value_or_Not Provided}` |
| Background Mode | `{value_or_Not Provided}` |
| Exact-Count Overlay | `{value_or_Not Applicable}` |
| Unity Meta Status | `{value_or_Not Copied}` |

## 10. Self-Contained Slack Evidence

`canvasEvidenceMode=self_contained` is required when Canvas readers must not
depend on the evaluator PC.

Before publication:

1. process one icon at a time;
2. upload the exact evaluated PNG to Slack;
3. obtain a workspace-accessible Slack file reference;
4. use that reference as a standalone top-level Canvas image;
5. include the relevant source skill JSON block verbatim;
6. include the evaluation result, score, Hard Fail, highest severity, confirmed
   findings, and required actions;
7. keep current-PC paths and SHA-256 as provenance metadata only.

Never use `file://` or a local absolute path as the Canvas image source. A local
draft may state `Slack evidence pending`, but it must not pretend that a local
link is publishable evidence.

If upload authorization, a Slack upload capability, a shareable evidence
conversation, or an upload result is missing, do not publish. Return
`slack_evidence_upload_not_available` or
`slack_evidence_upload_failed` and keep the canonical local draft.

## 11. Promotion Rules

- FAIL: `not_promoted` or `blocked`; never copy.
- CONDITIONAL_PASS: remains unpromoted unless explicit approval is recorded.
- PASS: may be `approved_for_promotion`; PASS alone does not perform a copy.
- `promoted`: requires project target existence, copy/hash verification, and
  Unity import evidence.
- Canvas generation and publication never copy to Unity.

Record every state transition in `Change Log`. Staging and project target paths
must retain different responsibilities even after promotion.

## 12. Slack Write Rules

- `draft_only` is the default.
- `localDraftMode=save` writes only
  `{evaluationWorkspacePath}\evaluation\evaluation_canvas.md`.
- `report_only` writes no draft file.
- `append` or `replace_artifact_section` requires explicit write authorization
  and an unambiguous target Canvas.
- `replace_artifact_section` may replace only the record matching both
  `artifactType=skill_icon` and `artifactId={equipmentId}`.
- If tools or authorization are missing, report the blocked publication and do
  not select another Canvas.

## 13. Validation Checklist

- [ ] All paths were resolved on the current PC.
- [ ] `evaluationRoot` is `C:\github\design_evaluation\skill_icon`.
- [ ] `iconPath == stagingArtifactPath`.
- [ ] Staging and project target are distinct.
- [ ] Source JSON, equipment ID, preserved filename, and SHA-256 agree.
- [ ] The entire fatal-failure checklist ran before scoring.
- [ ] Six category scores respect 25/20/20/15/10/10 and sum exactly.
- [ ] PASS is 85+ with no fatal failure or required-evidence gap.
- [ ] Report, JSON result, and Canvas contain the same decision.
- [ ] Critical/Major findings and all required corrections remain visible.
- [ ] The 11 common Canvas sections exist once and in order.
- [ ] Promotion status follows the common state matrix.
- [ ] `promoted` has copy and Unity evidence.
- [ ] A self-contained published Canvas uses Slack-hosted media and verbatim
      source intent, not a local path.
- [ ] No source asset, Unity file, `.meta`, or Git state was changed.

## 14. Failure Types

```text
missing_skill_json
invalid_skill_json
equipment_id_mismatch
missing_icon
invalid_png
missing_generation_record
missing_frame_template
missing_normalization_record
missing_preview32
insufficient_evidence
missing_evaluation_report
evaluation_report_hash_mismatch
invalid_form_version
invalid_result
invalid_promotion_status
promotion_result_conflict
promotion_verification_missing
staging_target_path_collision
invalid_evaluation_root
other_pc_path_detected
invalid_draft_path
invalid_local_draft_mode
slack_write_not_available
slack_write_not_authorized
slack_evidence_upload_not_available
slack_evidence_upload_failed
output_write_failed
```
