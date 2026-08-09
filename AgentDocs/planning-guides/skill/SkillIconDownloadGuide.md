# Skill Icon Download Guide


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

- **Guide type:** download/preservation workflow guide.
- **Responsibility:** resolve provider results, download candidates without semantic modification, preserve evidence, and emit a download record plus evaluation handoff.
- **Inputs:** generation record, provider result references, full `equipmentId`, and current-PC `{evaluationRoot}`.
- **Preconditions:** provenance is traceable to the requested skill and the preservation root is outside the project target.
- **Handoff:** immutable downloaded evidence, checksums, download record, and evaluation request.
- **Mutation boundary:** no rubric scoring/verdict, PASS decision, project copy, Unity import/meta work, regeneration, or Git work.

The generation record controls provenance, this guide controls download and
preservation, and `SkillIconEvaluationGuide.md` controls the later verdict. Stop
with `download_authority_conflict` when identities or records disagree.

## 1. Purpose

This guide defines the download/preservation stage for a static PixelLab skill icon:

```text
identify result
→ download primary candidate
→ preserve optional semantic edit
→ preserve deterministic overlay and normalized candidate
→ preserve downloaded evidence
→ write checksums and download record
→ hand off to immutable evaluation
```

Use this guide after `SkillIconGenerationGuide.md`. Use
`SkillIconEvaluationGuide.md` for the quality decision.

Generation, download, evaluation, and Unity copy are separate responsibilities.
This guide does not generate or regenerate an icon.

## 2. Required Inputs

```text
projectRoot
skillSourcePath
equipmentId
pixelLabResult
evaluationRoot
frameTemplatePath
backgroundMode
backgroundDescription
normalizationRecord
```

Resolve the established evaluation root on the current PC:

```text
evaluationRoot = {current_pc_skill_icon_evaluation_root}
```

`pixelLabResult` may be one of:

- An opened `create_ui_pro` result page containing the 16 variations or gallery items.
- A local download folder containing the completed per-attempt images and generation record.

If the result cannot be tied to `equipmentId`, stop before preservation or Unity copy.

## 3. ID and Filename Rules

Use the full equipment skill ID:

```text
skill.{domain}.{character_name}.{grade}.{slot}.{skill_name}
```

Example:

```text
skill.character.military_officer.3.active_1.charge
```

Canonical icon filename:

```text
{equipmentId}.icon.png
```

Do not shorten the filename to `skillName`, `slot`, or a display name. Do not use
`skillSlug` as the Unity filename.

## 4. Evaluation Preservation Layout

Preserve the selected source and evaluation evidence outside the Unity project:

```text
{evaluationRoot}/{equipmentId}/
  source/
    {equipmentId}.icon.png
  candidates/
    candidate_00.primary.png
    ...
    candidate_15.primary.png
    candidate_00.edited.png
    candidate_00.normalized.png
    candidate_00.preview32.png
  evaluation/
    evaluation_result.txt
  generation_record.txt
  candidate_scores.txt
```

Rules:

- `source/{equipmentId}.icon.png` is the immutable selected source.
- `candidates` contains the primary, optional edited, normalized, and 32 x 32
  preview evidence using the existing folder. Do not add a new intermediate folder.
- `evaluation_result.txt` follows `SkillIconEvaluationGuide.md`.
- `generation_record.txt` records PixelLab inputs and provenance.
- `candidate_scores.txt` records every candidate score and selection reason.

Do not use the Unity project folder as the only preservation location.

## 5. Future Project Target (Informational Only)

A separate PASS-only promotion task may later copy the approved source to:

```text
Assets/ImagesGenerated/Skill/icon/{equipmentId}.icon.png
```

Required `.meta` path:

```text
Assets/ImagesGenerated/Skill/icon/{equipmentId}.icon.png.meta
```

The preserved source and Unity destination must be byte-identical. Confirm with
SHA-256 after copy.

## 6. Result Identification and Download

1. Read `skillSourcePath` and confirm its `equipmentId`.
2. Confirm that `pixelLabResult` belongs to the same skill and generation request.
3. Confirm that the primary result was generated at
   `https://www.pixellab.ai/create?tool=create_ui_pro` with
   `Create UI elements (Pro)` and Custom size 80 x 80.
4. Record the result page or gallery items, four core Description sentences,
   optional contextual background sentence, background mode, Transparent background
   setting, empty Concept Image, Color palette, optional Seed, and attempt number.
5. Download all 16 individual 80 x 80 variations from the 4 x 4 Pro result without
   resizing or recompressing them. Do not treat a combined grid as one candidate.
6. Apply cheap technical and semantic rejection checks to all 16 variations and
   advance at most the best three candidates.
7. When a semantic edit exists, record the `Edit image` result and its one-sentence
   add/remove/change/replace instruction separately.
8. Reject thumbnails, previews, unrelated downloads, HTML placeholders, and broken
   files.
9. Do not classify candidates only by browser-generated filenames.

Do not download a gallery contact sheet, preview thumbnail, or a result created by
another PixelLab tool.

## 7. Technical Validation Before Preservation

Every downloaded primary and edited candidate must pass:

- PNG decoding succeeds.
- Width is exactly 80 pixels.
- Height is exactly 80 pixels.
- Color mode is RGBA.
- The file contains one static icon.
- The image is not an animation sheet or multi-panel grid.
- A `flat` primary uses transparency because the background and frame are applied
  during deterministic normalization. A `contextual` primary is opaque.
- There is no text, watermark, or PixelLab UI artifact.

Every normalized candidate must additionally pass:

- Existing `frameTemplatePath` is recorded and is 80 x 80 RGBA.
- `flat` uses the approved charcoal/deep-brown template interior. `contextual`
  preserves only the recorded low-contrast background inside x=2..77 and y=2..77.
- The primary and effects remain inside the central 64 x 64 foreground safe area.
- Rows and columns 0, 1, 78, and 79 exactly match the frame template.
- The exact-count overlay matches its manifest.
- The nearest-neighbor 32 x 32 preview is present.

Technical hard fail:

- No valid candidate exists.
- Candidate dimensions are not exactly 80 x 80. Do not crop or resize to reconcile
  the dimensions.
- The candidate is corrupt, incomplete, or not a PNG.
- The result belongs to another skill.

A technical hard fail prevents preservation as the selected source and prevents
Unity copy.

## 8. Candidate Preservation

1. Preserve every technically valid downloaded candidate as immutable evidence.
2. Do not assign rubric scores or select a passing source in this stage.
3. When the generation record identifies a provisional candidate, preserve that
   identity without treating it as an evaluation verdict.
4. Copy, rather than move, the provisional candidate to:

```text
{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png
```

5. Do not modify the provisional normalized file after it is preserved as source.
   Deterministic overlay and normalization must occur before source preservation.
6. Record the selected candidate index and SHA-256.

If no valid candidate can be preserved, retain available evidence and report a
download failure. Do not copy anything to the project.

## 9. Generation Record

Save:

```text
{evaluationRoot}/{equipmentId}/generation_record.txt
```

Required fields:

```text
Skill ID:
Source JSON:
PixelLab Creator URL:
Primary Tool:
PixelLab Result Page or Gallery Item:
Primary Description:
Reference Mode: none
Composition Profile:
Background Requirement:
Background Mode: flat | contextual
Background Description: omitted | value
Core Outline Sentence:
Direction Sentence:
Simple Skill Effect Sentence:
Compact Exclusion / Grade Sentence:
Optional Contextual Background Sentence:
Semantic Edit Tool / Instruction / Result:
Frame Template Path:
Exact-Count Overlay Manifest:
Transparent Background:
Seed: value or not_exposed
Requested Width / Height:
Downloaded Width / Height:
Attempt Count:
Normalization Record:
32x32 Preview Path / Result:
Selected Candidate:
Selected Source Path:
Selected SHA-256:
Download Date:
```

The record must show `tool=create_ui_pro`, `Create UI elements (Pro)`, Custom size
80 x 80, a 4 x 4 result with 16 accounted variations, empty Concept Image,
background mode and transparency choice, any semantic edit, exact-count overlay,
existing frame template, deterministic normalization, and 32 x 32 preview result.

## 10. Legacy Evaluation and Promotion Appendix (Non-executable)

Sections 10 through 16 are retained to interpret historical artifacts. Current
execution stops after Section 9 and emits an immutable evaluation handoff. It
must not score, promote, import, or clean evidence required by downstream tasks.

### Evaluation Result (Legacy)

Evaluate the preserved selected source using `SkillIconEvaluationGuide.md` and save:

```text
{evaluationRoot}/{equipmentId}/evaluation/evaluation_result.txt
```

The result must include:

- Skill ID and source JSON.
- Preserved source path.
- Intended Unity path.
- Grade and slot.
- Expected classification.
- 80 x 80 and 32 x 32 inspection results.
- SHA-256.
- Fatal failure checks.
- Category scores and total.
- Pass, Conditional Pass, or Fail.
- Required corrections and regeneration prompt changes.

Only `Pass` may proceed to Unity copy. `Conditional Pass` requires explicit approval
before Unity copy. `Fail` never proceeds to Unity copy.

Quality failure does not delete evaluation evidence.

### Copy to Unity (Legacy)

After the preserved source receives `Pass`:

1. Confirm the preservation filename is `{equipmentId}.icon.png`.
2. Confirm the destination is exactly:

```text
Assets/ImagesGenerated/Skill/icon/{equipmentId}.icon.png
```

3. If an accepted Unity icon already exists, stop unless replacement is explicitly
   authorized.
4. Copy the preserved source to the Unity destination.
5. Calculate SHA-256 for the preserved source and Unity destination.
6. If hashes differ, delete neither copy; report `checksum_mismatch` and stop before
   import completion.

Do not copy a failed candidate, a contact sheet, or `candidate_XX.png` directly into
Unity.

### Unity Import Rules (Legacy)

The static icon is one sprite, not a multi-sprite sheet.

Required import settings:

- Texture Type: Sprite (2D and UI).
- Sprite Mode: Single.
- Filter Mode: Point.
- Mip Maps: disabled.
- Alpha Is Transparency: enabled.
- Compression: None for the default platform unless the project has an explicit
  icon compression policy.
- Pivot: center.

Create or update:

```text
Assets/ImagesGenerated/Skill/icon/{equipmentId}.icon.png.meta
```

Match the existing approved skill icon `.meta` format and resource import policy.
Do not reuse another icon's GUID.

If the correct Unity Editor cannot be run, report:

```text
meta configured / Unity reimport pending
```

Do not report import completion without editor evidence.

### Resource and ID Validation (Legacy)

Confirm:

- Source JSON `equipmentId` matches the filename ID.
- The Unity filename uses the entire `equipmentId`.
- The icon resource key or EquipmentSkillSO icon reference resolves to the intended
  file according to the current builder/runtime convention.
- No legacy shortened icon name is introduced.
- No different skill points to this icon unless explicit reuse is approved.
- PNG and `.meta` exist together at the destination.

If the current runtime expects an icon asset name rather than a path, record the
exact resolved key. Do not invent a new naming convention in this workflow.

### Cleanup (Legacy)

Cleanup only after preservation, evaluation result save, Unity copy, checksum
verification, and meta configuration are complete.

Delete:

- Download archives.
- Temporary extraction folders.
- Duplicate browser downloads.
- Unrelated previews and thumbnails.

Keep:

- Preserved selected source.
- `generation_record.txt`.
- `candidate_scores.txt`.
- `evaluation/evaluation_result.txt`.
- Candidate evidence required to explain a failure or selection.
- Unity PNG and `.meta`.

Do not delete failed evaluation evidence until a replacement is accepted and the
replacement history is recorded.

### Completion Checklist (Legacy)

- [ ] PixelLab result matches `equipmentId`.
- [ ] Every candidate was downloaded and technically validated.
- [ ] Empty Concept Image and the existing frame template path were recorded.
- [ ] All 16 Pro variations were accounted for and at most three advanced.
- [ ] Background mode, description, transparency, and normalization agree.
- [ ] Exact-count overlay and normalization records were preserved.
- [ ] The normalized candidate, not the raw primary, was evaluated.
- [ ] The 32 x 32 nearest-neighbor preview was checked.
- [ ] Every valid candidate was scored.
- [ ] Selected candidate scored at least 85 with no fatal failure.
- [ ] Selected source is preserved outside Unity.
- [ ] Evaluation result is saved under the evaluation folder.
- [ ] Preserved and Unity filenames use the full `equipmentId`.
- [ ] Unity destination is under `Assets/ImagesGenerated/Skill/icon`.
- [ ] Preserved and Unity SHA-256 values match.
- [ ] Unity `.meta` uses Sprite Single and a unique GUID.
- [ ] Runtime icon key resolves correctly or pending status is explicit.
- [ ] Temporary files were cleaned without deleting evidence.

### Failure Output (Legacy)

```text
status: failed
failureType:
  - pixellab_result_mismatch
  - missing_download
  - invalid_png
  - invalid_icon_size
  - missing_frame_template
  - overlay_failed
  - normalization_failed
  - invalid_equipment_id
  - no_passing_candidate
  - evaluation_write_failed
  - existing_icon_requires_approval
  - unity_copy_failed
  - checksum_mismatch
  - unity_meta_failed
  - unity_import_pending
  - unresolved_icon_resource_key
failureReason:
preservedFiles:
evaluationResultPath:
unityPath:
cleanupStatus:
nextAction:
```
