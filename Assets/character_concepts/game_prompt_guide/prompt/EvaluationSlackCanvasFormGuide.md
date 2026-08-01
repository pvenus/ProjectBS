# Evaluation Slack Canvas Form Guide

## 1. Purpose

This guide defines the domain-neutral Slack Canvas record used to preserve a
completed evaluation result.

It supports images, icons, animations, documents, JSON, and future artifact
types. Domain guides may add fields and score categories, but they must not
rename, remove, reorder, or reinterpret the common fields and sections.

This workflow records an evaluation. It does not:

- perform or revise the source evaluation;
- generate, edit, or regenerate an artifact;
- copy an artifact into the project;
- change evaluation scores, findings, severity, or result;
- perform Git work.

## 2. Form Version

```text
formVersion = evaluation_canvas_form_v1
versionFolder = v1
```

The version is stable. Create a new major version when a required field or
section is removed or renamed, an enum changes meaning, or promotion semantics
change. Adding an artifact type or optional domain field does not require a new
version.

## 3. Local Draft Path

When a local draft is saved, use one artifact record per file:

```text
Assets/Doc/Evaluation/slack_canvas/v1/{evaluationDomain}/{artifactType}/{artifactId}.canvas.md
```

Rules:

- `evaluationDomain` and `artifactType` use lowercase snake_case.
- `artifactId` is stable and file-safe.
- A `v1` draft must declare `evaluation_canvas_form_v1`.
- Do not combine multiple form versions in one version folder.
- The draft path is project-root-relative even when evaluated evidence is
  preserved outside the project.
- `localDraftMode=save` writes the draft to this path.
- `localDraftMode=report_only` returns Canvas-ready Markdown without writing a
  local draft; in this mode the canonical future draft path may still be
  reported as `Not Saved`.

## 4. Staging, Evaluation, and Promotion Model

The required operating sequence is:

```text
generated or external artifact
-> local staging copy
-> read-only evaluation using preserved evidence
-> approved artifact only
-> project copy
-> copy/hash/import verification
```

The three path fields are different responsibilities:

| Field | Meaning |
|---|---|
| `stagingArtifactPath` | Exact local file that was evaluated |
| `evaluationWorkspacePath` | Folder containing candidates, report, previews, hashes, and provenance |
| `projectTargetPath` | Final project destination used only after promotion approval |

`stagingArtifactPath` and `evaluationWorkspacePath` may be absolute paths on the
current PC when the evaluation workspace intentionally lives outside the
project. Never copy an absolute path from another PC. `projectTargetPath` must be
project-root-relative.

Do not replace these fields with a generic `primaryPath`, `imagePath`, or
`iconPath` in the common form. A domain alias such as `iconPath` is permitted
only when the domain guide explicitly states that it equals
`stagingArtifactPath`.

### 4.1 Path Separation

Normally:

```text
stagingArtifactPath != projectTargetPath
```

If the two resolve to the same file, record a `process_violation` finding and
use `promotionStatus: blocked`. An artifact type may allow in-place evaluation
only when its domain guide names an explicit policy and the record cites that
policy. Convenience is not an exception.

For `SKIPPED` with `promotionStatus: not_applicable`, required path fields remain
present but may use the literal value `Not Applicable`. Validators must not apply
file-path syntax checks to those values.

## 5. Required Common Fields

Every record must contain:

```text
evaluationDomain
artifactType
artifactId
artifactName
formVersion
evaluationReportSource
stagingArtifactPath
evaluationWorkspacePath
projectTargetPath
promotionStatus
```

Definitions:

- `evaluationReportSource`: exact report file, thread reference, or other stable
  source from which the Canvas record was formatted.
- `promotionStatus`: the artifact's state in the local-to-project promotion
  workflow, not the evaluation result.

Missing required fields are not inferred. Stop with `missing_required_field`.

## 6. Result and Promotion Contract

### 6.1 Result

Allowed values:

```text
PASS
CONDITIONAL_PASS
FAIL
SKIPPED
```

### 6.2 Promotion Status

Allowed values:

```text
not_promoted
approved_for_promotion
promoted
blocked
not_applicable
```

### 6.3 State Rules

| Evaluation result | Allowed before explicit approval/copy | Allowed after explicit approval, before copy | Allowed after verified project copy |
|---|---|---|---|
| `FAIL` | `not_promoted`, `blocked` | Not allowed | Not allowed |
| `CONDITIONAL_PASS` | `not_promoted`, `blocked` | `approved_for_promotion` only with recorded explicit approval | `promoted` only after that approval and copy verification |
| `PASS` | `not_promoted`, `approved_for_promotion` | `approved_for_promotion` | `promoted` |
| `SKIPPED` | `not_applicable` | Not applicable | Not applicable |

Additional rules:

- A `FAIL` can never be `approved_for_promotion` or `promoted`.
- A `CONDITIONAL_PASS` cannot be promoted before explicit approval is recorded
  in Evidence Package and Change Log.
- A `PASS` is normally `approved_for_promotion` immediately before copy.
- Set `promoted` only after the project target exists and copy integrity is
  verified. For files, record source and target hashes when available.
- `not_applicable` is for artifacts that intentionally have no project copy,
  such as an approved `SKIPPED` image policy.
- Formatting a Canvas record does not itself authorize or perform promotion.

## 7. Required Canvas Sections

Use these section names and this exact order:

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

A domain guide may add rows inside these sections. It must not replace a section
with a domain-specific name.

### 7.1 Record Metadata

```md
## Record Metadata

| Field | Value |
|---|---|
| Form Version | `evaluation_canvas_form_v1` |
| Evaluation Domain | `{evaluationDomain}` |
| Artifact Type | `{artifactType}` |
| Artifact ID | `{artifactId}` |
| Artifact Name | `{artifactName}` |
| Evaluation Report Source | `{evaluationReportSource}` |
| Review Date | `{YYYY-MM-DD}` |
| Reviewer | `{reviewer}` |
| Canvas Update Mode | `{draft_only/append/replace_artifact_section}` |
```

### 7.2 Result Summary

```md
## Result Summary

| Field | Value |
|---|---|
| Result | `{PASS/CONDITIONAL_PASS/FAIL/SKIPPED}` |
| Overall Score | `{score_or_Not Scored}` |
| Hard Fail | `{Yes/No/Not Applicable}` |
| Highest Severity | `{Critical/Major/Minor/Suggestion/None}` |
| Pass Criteria | `{exact_domain_criteria}` |
| Next Action | `{approve/revise/regenerate/re-evaluate/skip/needs_decision}` |
```

### 7.3 Target Artifact

```md
## Target Artifact

| Field | Value |
|---|---|
| Staging Artifact Path | `{stagingArtifactPath}` |
| Evaluation Workspace Path | `{evaluationWorkspacePath}` |
| Project Target Path | `{projectTargetPath}` |
| Promotion Status | `{promotionStatus}` |
| Resource Key | `{resource_key_or_Not Applicable}` |
| Staging Hash | `{hash_or_Not Provided}` |
| Project Hash | `{hash_or_Not Copied}` |
| Copy Verification | `{Not Performed/Pass/Fail/Not Applicable}` |
| Dimensions or Duration | `{value_or_Not Applicable}` |
```

### 7.4 Evidence Package

```md
## Evidence Package

| Evidence Type | Source | Notes |
|---|---|---|
| Evaluation Report | `{evaluationReportSource}` | `{note}` |
| Evaluation Guide | `{primaryEvaluationGuide}` | `{note}` |
| Source Data | `{source_data_path}` | `{note}` |
| Staging Artifact | `{stagingArtifactPath}` | `{note}` |
| Evaluation Workspace | `{evaluationWorkspacePath}` | `{note}` |
| Promotion Approval | `{approval_source_or_Not Provided}` | `{note}` |
```

### 7.5 Score Breakdown

```md
## Score Breakdown

| Category | Score | Max | Status | Evidence |
|---|---:|---:|---|---|
| `{category}` | `{score_or_Not Evaluated}` | `{max}` | `{Pass/Needs Work/Not Evaluated}` | `{short_evidence}` |
```

Preserve category names, maximums, and scores from the domain evaluation guide
or report. Do not normalize one domain's rubric into another.

### 7.6 Findings

```md
## Findings

| Severity | Finding | Evidence | Impact | Recommendation |
|---|---|---|---|---|
| `{severity}` | `{finding}` | `{evidence}` | `{impact}` | `{recommendation}` |
```

Write `No findings` when there are none. A path collision must appear here as
`process_violation`.

### 7.7 Required Actions

```md
## Required Actions

| Priority | Action | Owner | Status | Due |
|---:|---|---|---|---|
| `1` | `{action}` | `{owner_or_TBD}` | `{Open/In Progress/Done/Blocked}` | `{date_or_TBD}` |
```

Write `None` only when the evaluation report has no required correction or
approval action.

### 7.8 Optional Improvements

```md
## Optional Improvements

- `{optional_improvement_or_None}`
```

### 7.9 Domain-Specific Notes

```md
## Domain-Specific Notes

| Field | Value |
|---|---|
| `{domain_field}` | `{value}` |
```

### 7.10 Re-evaluation Plan

```md
## Re-evaluation Plan

| Field | Value |
|---|---|
| Expected Score After Fix | `{value_or_Not Estimated}` |
| Pass Likelihood | `{value_or_Not Estimated}` |
| Remaining Risk | `{value_or_None}` |
| Re-evaluation Trigger | `{required_change_or_Not Applicable}` |
```

### 7.11 Change Log

```md
## Change Log

| Date | Change |
|---|---|
| `{YYYY-MM-DD}` | `Initial record created from {evaluationReportSource}.` |
```

Record every promotion state transition. A `CONDITIONAL_PASS` approval entry
must identify its approval evidence. A `promoted` entry must identify copy
verification.

## 8. Common Enums

Severity:

```text
Critical
Major
Minor
Suggestion
None
```

Next action:

```text
approve
revise
regenerate
re-evaluate
skip
needs_decision
```

Canvas update mode:

```text
draft_only
append
replace_artifact_section
```

Local draft mode:

```text
save
report_only
```

## 9. Slack Canvas Update Rules

- `draft_only` is the default. Save or return Markdown and do not call Slack
  tools.
- `localDraftMode=save` writes the canonical local draft.
- `localDraftMode=report_only` returns Markdown only and must not create or
  modify a local draft file.
- `append` creates a new artifact record only when the user explicitly
  authorizes a Slack write and the target Canvas is unambiguous.
- `replace_artifact_section` replaces only the record matching both
  `artifactType` and `artifactId`.
- If a Slack tool is unavailable or writing is not authorized, keep the local
  draft and report that posting was not performed.
- Never post to an arbitrary Canvas when the target is missing.

### 9.1 Self-Contained Evidence

Use `canvasEvidenceMode=self_contained` when the Canvas must be understandable
without access to the current PC or repository checkout.

For every evaluated artifact:

1. Process exactly one artifact at a time. Do not load an entire image batch or
   all evaluation reports into memory at once.
2. Upload the actual evaluated media file to Slack and obtain a workspace-
   accessible Slack file reference.
3. Share the uploaded file to the explicit evidence conversation when required
   for workspace access.
4. Embed the Slack file reference directly in the Canvas. A local filesystem
   path is provenance metadata only and is not acceptable as the primary
   evidence link.
5. Include the original planning or source content verbatim. Do not summarize,
   rewrite, or truncate this source block.
6. Include the player-facing/display content separately so readers can compare
   the source intent with the displayed text.
7. Include a concise evaluation summary that preserves the source result,
   score, Hard Fail, highest severity, confirmed findings, and required actions.
8. Keep the project-relative target path and external evaluation path in
   metadata for traceability, but do not require readers to open either path to
   understand the record.

Recommended per-artifact Canvas block:

```md
### {artifactName} ??{result}

![{artifactName}]({slack_file_reference})

**Original Planning Content**

{verbatim_source_content}

**Display Content**

{display_content}

| Evaluation Field | Value |
|---|---|
| Result | `{result}` |
| Score | `{score}` |
| Highest Severity | `{severity}` |
| Finding | `{finding_or_None}` |
| Required Action | `{required_action_or_None}` |
```

The image syntax must be a top-level standalone Canvas element. Slack file
references must point to files uploaded into the same workspace; `file://`
links and local absolute paths must never be used as Canvas image sources.

If upload authorization, a Slack upload tool, a shareable destination, or an
upload result is missing, stop the self-contained publication with
`slack_evidence_upload_not_available` or `slack_evidence_upload_failed`.
Do not silently fall back to a local link.

### 9.2 Skill Animation Evidence

For `artifactType=skill_animation`, the canonical evaluation workspace is:

```text
C:\github\design_evaluation\skill_animation\{artifactId}
```

The local record must distinguish:

- exact staged reference PNG;
- exact staged animation sheet PNG;
- row-major individual PNG frames;
- contact sheet;
- playback GIF;
- technical validation;
- preserved existing evaluation;
- Unity meta, Editor reimport, clip, and runtime binding status.

Self-contained Slack publication must upload the playback GIF plus either the
reference PNG or contact sheet as Slack-hosted media. The GIF is a motion-review
aid only. Result, fatal checks, and crop/alpha claims must come from the source
PNG and individual PNG-frame evidence.

Each animation record must preserve:

```text
source reference SHA-256
source animation SHA-256
usable frame count and row-major order
nominal FPS and encoded frame delay
loopMode
playback GIF SHA-256
reference/contact-sheet evidence
existing evaluation result and score
Unity meta/reimport/clip/binding remaining steps
```

Do not expose local absolute paths in the published Canvas body. Use Slack-hosted
media and project-relative identifiers for the reader-facing record. Local
paths remain evaluator-only provenance.

For a dedicated animation Canvas:

- upsert by `artifactType=skill_animation` plus exact `artifactId`;
- keep exactly the 11 common sections in order;
- process one artifact at a time;
- do not post one channel message per artifact;
- maintain one representative Canvas link message;
- exclude failed, blocked, and ungenerated artifacts from the initial completed
  publication scope.


### 9.3 Character Evaluation Animation GIF Evidence

For `artifactType=character_evaluation`, the canonical evaluation workspace may
be rebased from PixelLab export storage into:

```text
C:\github\design_evaluation\character\{characterName}_{grade}
```

Published Canvas records must use project-relative identifiers such as:

```text
design_evaluation/character/{characterName}_{grade}/...
```

Do not expose `C:\github`, `C:\Users`, or any other local absolute path in the
reader-facing Canvas body.

When character animation evidence is available, self-contained Slack publication
must include animation GIF evidence in addition to the static rotation/contact
preview. The preferred evidence set per character is:

1. one static rotation/contact preview image;
2. one `Idle` all-directions GIF;
3. one `Move` all-directions GIF;
4. one `Attack` all-directions GIF.

The GIFs should be prepared under the evaluation workspace:

```text
evidence/animation_gif_by_type/{characterName}_{grade}_idle_all_directions.gif
evidence/animation_gif_by_type/{characterName}_{grade}_move_all_directions.gif
evidence/animation_gif_by_type/{characterName}_{grade}_attack_all_directions.gif
character_animation_gif_by_type_manifest.json
```

Each animation GIF must show all available ProjectBS directions for that
animation. For the current character animation pipeline this means four columns:

```text
DownRight, DownLeft, UpRight, UpLeft
```

The GIF is a review and playback aid. The preserved evaluation result, scores,
folder-structure pass/fail, direction handling, file naming, and Unity-copy
integrity must still come from the saved evaluation files and PNG frame evidence:

```text
metadata.json
evaluation_result.txt
evaluation_animation_result.txt
animations/
converted/
```

Rules:

- Preserve the existing image and animation evaluation results; do not re-score.
- Generate GIFs only from preserved PNG frames in the evaluation workspace.
- Do not modify `animations/`, `converted/`, Unity project assets, or PixelLab
  source files while preparing Canvas evidence.
- Upload GIFs as Slack-hosted media and embed them in the Canvas.
- Do not post one channel message per character or per GIF.
- Maintain a single representative Canvas link message after verification.
- Upsert character records by exact `artifactType=character_evaluation` plus
  `canonicalCharacterId=character.{characterName}.{grade}`.
- For a completed batch, validate the expected media count. With 22 characters,
  the minimum media set is 22 static previews plus 66 animation GIFs.

## 10. Validation

- All required common fields exist and are non-empty.
- The form version matches the `v1` draft folder.
- The 11 required sections exist once and in order.
- Result, score, severity, and findings match the evaluation report.
- Every score is within the domain category maximum and totals match when the
  report is scored.
- Critical and Major findings are not omitted.
- Staging, evaluation workspace, and project target paths retain their distinct
  meanings.
- A staging/project path collision is blocked unless an explicit in-place policy
  is cited.
- Promotion status follows the result matrix.
- `promoted` has copy-verification evidence.
- Current-PC external paths are preserved accurately; other-PC paths are not
  copied.
- Secrets, tokens, credentials, and unrelated private links are excluded.
- Markdown tables remain structurally valid.
- In `self_contained` mode, every image artifact has a Slack-hosted embedded
  image, verbatim source content, separate display content, and preserved
  evaluation summary.
- A reader with Canvas access can understand the intent, rendered artifact,
  decision, and required follow-up without opening a local path.
- Batch publication processes artifacts sequentially and does not aggregate
  image bytes or full result documents in memory.

## 11. Failure Types

```text
missing_evaluation_report
missing_required_field
missing_form_guide
invalid_form_version
invalid_result
invalid_promotion_status
promotion_result_conflict
promotion_verification_missing
staging_target_path_collision
invalid_draft_path
invalid_local_draft_mode
invalid_canvas_target
slack_write_not_available
slack_write_not_authorized
slack_evidence_upload_not_available
slack_evidence_upload_failed
missing_self_contained_evidence
artifact_section_not_found
unsupported_canvas_update_mode
output_write_failed
```
