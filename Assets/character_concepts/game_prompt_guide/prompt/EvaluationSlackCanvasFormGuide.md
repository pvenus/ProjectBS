# Evaluation Slack Canvas Form Guide

## 1. Purpose

This guide defines the domain-neutral Slack Canvas record used to preserve a
completed evaluation result.

This guide is the single source of truth for record fields, field meanings,
reader-facing layout, archival sections, and content validation. Task states,
tool-call order, write authorization, failure routing, and completion reporting
belong to `EvaluationSlackCanvasFormPrompt.md`.

It supports images, icons, animations, documents, JSON, and future artifact
types. Domain guides may add fields and score categories, but they must not
rename, remove, reorder, or reinterpret the common fields and sections.

## 2. Form Version

```text
formVersion = evaluation_canvas_form_v1
versionFolder = v1
readerFacingLayoutVersion = artifact_design_table_v2_compact
```

The version is stable. Create a new major version when a required field or
section is removed or renamed, an enum changes meaning, or promotion semantics
change. Adding an artifact type or optional domain field does not require a new
version.

`readerFacingLayoutVersion` is a presentation contract layered on top of the
stable archival form. Existing `artifact_design_table_v1` records remain valid
as legacy layouts. New records and format migrations use
`artifact_design_table_v2_compact` unless a domain guide explicitly defines a
newer compatible reader-facing layout.

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

## 4. Artifact and Path Semantics

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

A record with a missing required field is invalid. Required content must never
be inferred.

### 5.1 Required Planning and Design Evidence

Every new self-contained visual record must also contain the following
reader-facing evidence. These fields describe why the artifact was designed,
not how the Slack publication was executed.

| Field | Required content | Source/derived rule |
|---|---|---|
| `artifactUsage` | Player-facing use, placement, and expected display size | Source fact |
| `planningSource` | Stable planning document identifier and project-relative path | Source fact |
| `planningOriginalContent` | Relevant planning text verbatim | Source fact; never rewritten |
| `displayContent` | Actual player-facing name, description, dialogue, or body text | Source fact; conditional when display text exists |
| `planningCoreInterpretation` | Main subject, action, physical clue, and spatial relation extracted from the source | Derived; label as interpretation |
| `designConcept` | Visual identity, mood, shape, material, palette, and style | Approved design definition |
| `promptCoreGoals` | One to three priorities that the image must communicate first | Derived design intent |
| `requiredVisualElements` | Three to five independently observable must-show elements | Verification contract |
| `hardConstraints` | Prohibited content and technical hard gates | Verification contract |
| `generationPromptOriginal` | Exact final prompt submitted to the generation tool | Source fact; never summarized as the original |

Rules:

- `planningOriginalContent`, `displayContent`, and
  `generationPromptOriginal` are separate source blocks even when their text is
  identical.
- If `displayContent` is identical to the planning source, write
  `Same as Planning Original Content` and identify the display source. Do not
  silently duplicate or merge the provenance.
- `planningCoreInterpretation` must not delete an important person, action,
  physical clue, time, or spatial relationship merely because the current
  image omitted it.
- A required visual element must be observable. Avoid abstract-only entries
  such as `good mood`, `high quality`, or `matches the story`.
- A record without the required planning or design evidence is incomplete and
  must not be presented as a complete design record.

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

### 6.4 Reader-Facing Artifact Table

Each visual artifact uses one compact integrated table as its complete
reader-facing record. The target is approximately one Canvas page per artifact;
only a long verbatim planning source or generation prompt may extend it.

Conceptual visual target:

```text
[ exact media continues ] [ Planning Original Context ]
[ exact media continues ] [ Planning & Design          ]

[ Evaluation & Action   ] [ Prompt & Required Expression ]
[ Provenance & Change   ] [ Prompt content continues     ]
```

This diagram describes information hierarchy, not a promise that Slack tables
support `rowspan`, vertical merge, or arbitrary cell geometry. Never fake the
shape with empty cells or claim a merge that the active Slack UI/API cannot
create and preserve.

Supported production fallback:

| Left | Right |
|---|---|
| **Visual & Decision** — exact Slack-hosted media, result, score, one-line summary | **Planning Original Context** — one contiguous source block |
| **Planning & Design** — interpretation and approved/derived design definition | **Prompt & Required Expression** — goals, must-show elements, constraints, exact prompt or unavailable reason |
| **Evaluation & Action** — score summary, findings, required/optional action | **Provenance & Change** — source identity, completeness, review and change note |

The eleven archival semantics remain present through the category mapping in
Section 7; they are not rendered as eleven long standalone sections in this
layout.

#### 6.4.1 Layout Rules

- Preserve the media's original aspect ratio. Use the media-bearing left cell
  for approximately 40–50 percent of the table width when the Canvas UI permits
  stable resizing.
- A standard rectangular two-column table is the supported production layout.
  Native table merge/rowspan is not a required capability and must not be
  inferred from the conceptual visual target.
- Native Canvas Columns are experimental. Use them only after a one-artifact
  pilot preserves every text block and the image through autosave, reload, and
  connector reread. If a new top-level text block alone does not survive
  reload, do not attempt Columns on that Canvas.
- Preserve meaningful category boundaries. Do not collapse all categories into
  one long content cell.
- Every category cell must contain both its bold category label and its actual
  content. Never create a row or cell containing only a category title such as
  `Prompt & Required Expression` or `Provenance & Change`.
- The full-width title row is allowed only when it also contains result, score,
  and the one-line decision summary; a name-only title row is invalid.
- Use the supported fallback cells shown above when merge is unavailable. Do
  not convert the whole Canvas to a visually clever structure that has not
  passed one-artifact persistence validation.
- Target four to five visible content rows. Equal cell sizes are not required.
- Do not show literal `<br>` text to readers or paste escaped markup as content.
  Use real Canvas paragraph breaks inside each category cell. The Slack read API
  may serialize native paragraph boundaries as `<br>` markup; that serialization
  is acceptable only when the Canvas UI renders real breaks and no literal tag
  is visible to readers.
- Keep `planningOriginalContent` as one contiguous verbatim block. Never render
  it as `Planning Original 1`, `Planning Original 2`, `Planning Original 3`, or
  other numbered fragments.
- Group the three to five required visual elements into one compact list inside
  Prompt & Required Expression. Do not create one table row per element.
- Put score categories on one compact line or list unless an individual score
  requires an explanatory finding.
- Hide non-actionable hashes, absolute evaluator paths, empty optional fields,
  and repeated identifiers from the reader-facing table. Preserve audit data in
  the stable source record rather than repeating it visually.
- Reader-facing content uses project-relative identifiers only. Local absolute
  paths and hashes belong in provenance metadata, not the primary table.
- The exact prompt belongs in the Prompt & Required Expression group. If it is
  unavailable, show one explicit provenance sentence instead of an empty or
  reconstructed prompt.

#### 6.4.2 Capability and Persistence Gates

Before migrating an existing Canvas layout:

1. preserve the current canonical record as a backup;
2. choose one writer for the artifact: Canvas connector or Slack UI;
3. create the smallest new block required by the candidate layout;
4. wait for autosave, reload the Canvas, and verify the block still exists;
5. for media, verify a real rendered image object, natural dimensions, and the
   original aspect ratio; a serialized Slack file reference alone is
   insufficient;
6. reread the canonical section and verify result, score, categories, media,
   and duplicate count;
7. remove the backup only after all gates pass and deletion is authorized.

Stop the migration and retain the supported table when any gate fails. Remove
only temporary content, record any unreferenced Slack file ID, and do not move
to the next artifact.

#### 6.4.3 Information Priority

The table is intentionally layered:

1. large readable image and one-line decision summary;
2. one combined Planning & Design category with verbatim source and clearly
   labeled interpretation;
3. one combined Evaluation & Action category containing only decision-relevant
   scores, findings, and follow-up;
4. one combined Prompt & Required Expression category;
5. one compact Provenance & Change category.

Do not duplicate the same text in multiple cells or add a second archival block
below the table. The compact table is the reader-facing record.

## 7. Required Canvas Sections

The following eleven names define stable archival semantics:

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

A domain guide may add fields inside these semantics. It must not remove or
reinterpret them.

For `artifact_design_table_v2_compact`, map the eleven semantics into four
reader-facing category groups instead of rendering eleven separate headings:

| Compact category | Preserved archival semantics |
|---|---|
| Overview | Record Metadata; Result Summary; Target Artifact |
| Planning & Design | Evidence Package; Domain-Specific Notes |
| Evaluation & Action | Score Breakdown; Findings; Required Actions; Optional Improvements |
| Provenance & Change | Re-evaluation Plan; Change Log |

The detailed templates below define field meaning and audit completeness only.
They are not additional reader-facing tables required below the compact artifact
table.

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

## 9. Published Evidence Content Rules

### 9.1 Self-Contained Evidence

Use `canvasEvidenceMode=self_contained` when the Canvas must be understandable
without access to the current PC or repository checkout.

Every evaluated artifact record must:

1. show the exact reviewed media through a workspace-accessible Slack file
   reference in the integrated artifact table;
2. treat local filesystem paths as provenance metadata only, never as the
   reader's primary evidence link;
3. include the original planning or source content verbatim without summary,
   rewrite, or truncation;
4. include player-facing/display content separately so readers can compare the
   source intent with the displayed text;
5. include a concise evaluation summary that preserves the source result,
   score, Hard Fail, highest severity, confirmed findings, and required actions;
6. retain project-relative target identifiers and provenance metadata without
   requiring the reader to open a local path.

Recommended per-artifact Canvas block:

```md
| Left | Right |
|---|---|
| **Visual & Decision** — {Slack-hosted media once, result, score, one-line summary} | **Planning Original Context** — {one contiguous planning source block} |
| **Planning & Design** — {interpretation and design content} | **Prompt & Required Expression** — {goals, required elements, constraints, exact prompt or unavailable reason} |
| **Evaluation & Action** — {scores, findings, actions, re-evaluation trigger} | **Provenance & Change** — {source identity, completeness, review and change note} |
```

Each category cell is a Canvas-native multi-paragraph cell, not a string
containing escaped newline characters. A category label is always followed by
its content in the same cell.

The final Canvas must contain the Slack-hosted media in the intended supported
table cell. Transitional placeholders are invalid final content. Do not leave
both a standalone full-width image and a duplicate table image. A file ID in
connector output does not satisfy this rule when the reloaded UI shows a blank
or zero-height image object.

Slack file references must point to files uploaded into the same workspace;
`file://` links and local absolute paths must never be used as Canvas image
sources.

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

A self-contained animation record must contain the playback GIF plus either the
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

The record must preserve the existing image and animation evaluation result,
identify the canonical character as
`character.{characterName}.{grade}`, and contain the Slack-hosted static preview
plus the three required animation GIFs. With 22 characters, the minimum
published evidence set is 22 static previews plus 66 animation GIFs.

## 10. Validation

- All required common fields exist and are non-empty.
- New self-contained visual records and format migrations declare
  `readerFacingLayoutVersion=artifact_design_table_v2_compact`.
- The integrated artifact table contains the actual Slack-hosted media in its
  single media cell, target/use information, one contiguous verbatim planning
  block, separate display provenance, planning core interpretation, design
  concept, compact prompt goals/required elements/hard constraints, evaluation
  summary, and the verbatim generation prompt or explicit unavailable reason.
- Source facts and derived interpretation are visibly separated.
- Required visual elements remain individually reviewable inside one compact
  list rather than separate table rows.
- The artifact uses one table with four to five meaningful visible content rows,
  separate category cells, no title-only category cells, and no second archival
  table block.
- A structural migration passes a one-artifact persistence pilot before batch
  expansion. Native Columns are not used when top-level blocks, column text, or
  image objects disappear after reload.
- The writer mode is fixed per artifact. Connector and UI writes are not mixed
  on the same transient layout.
- Completion includes autosave wait, UI reload, real image rendering with
  natural dimensions and preserved aspect ratio, and connector reread.
- The previous canonical record remains available until the replacement passes
  every persistence gate; temporary failure is rolled back before processing
  another artifact.
- The record does not collapse all category content into one long cell.
- No numbered `Planning Original 1/2/3` fragments exist.
- No image placeholder, reader-visible literal `<br>` text, duplicate full-width
  image, or reader-facing local absolute path remains. API serialization of
  native paragraph breaks is not a reader-visible literal-tag failure.
- The form version matches the `v1` draft folder.
- All eleven archival semantics map exactly once into the four compact category
  groups; eleven standalone headings are not required for v2 compact records.
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
