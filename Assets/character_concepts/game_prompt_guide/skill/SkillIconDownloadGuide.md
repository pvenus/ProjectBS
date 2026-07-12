# Skill Icon Download Guide

## 1. Purpose

This guide defines the post-generation workflow for a static PixelLab skill icon:

```text
identify result
→ download candidates
→ select and preserve source
→ evaluate preserved source
→ copy passing source to Unity
→ verify checksum and import metadata
→ clean temporary files
```

Use this guide after `SkillIconGenerationGuide.md`. Use
`SkillIconEvaluationGuide.md` for the quality decision.

Generation, download/evaluation, and Unity copy are separate responsibilities.
This guide does not generate or regenerate an icon.

## 2. Required Inputs

```text
projectRoot
skillSourcePath
equipmentId
pixelLabResult
evaluationRoot
```

Default evaluation root:

```text
/Users/pvenus/Documents/PixelLab/skill/icon
```

`pixelLabResult` may be one of:

- A completed PixelLab v2 `background_job_id` accessible through authenticated API.
- An opened PixelLab result page.
- A local download folder containing the completed candidate images and generation record.

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
    candidate_00.png
    candidate_01.png
  evaluation/
    evaluation_result.txt
  generation_record.txt
  candidate_scores.txt
```

Rules:

- `source/{equipmentId}.icon.png` is the immutable selected source.
- `candidates` contains evidence needed to explain selection or failure.
- `evaluation_result.txt` follows `SkillIconEvaluationGuide.md`.
- `generation_record.txt` records PixelLab inputs and provenance.
- `candidate_scores.txt` records every candidate score and selection reason.

Do not use the Unity project folder as the only preservation location.

## 5. Unity Destination

Copy the passing preserved source to:

```text
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
```

Required `.meta` path:

```text
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png.meta
```

The preserved source and Unity destination must be byte-identical. Confirm with
SHA-256 after copy.

## 6. Result Identification and Download

1. Read `skillSourcePath` and confirm its `equipmentId`.
2. Confirm that `pixelLabResult` belongs to the same skill and generation request.
3. Record the PixelLab endpoint, background job ID or result page, seed, description,
   `style_description`, style reference paths, canvas size, and candidate count.
4. Download every completed candidate to a temporary folder.
5. If PixelLab returns an archive or grid, extract individual candidate PNG files
   without resizing or recompressing them.
6. Reject thumbnails, previews, unrelated downloads, HTML placeholders, and broken
   files.
7. Do not classify candidates only by browser-generated filenames.

PixelLab authentication secrets must never be written to the generation record,
evaluation output, terminal output, or project files.

## 7. Technical Validation Before Preservation

Every candidate must pass:

- PNG decoding succeeds.
- Width is exactly 80 pixels.
- Height is exactly 80 pixels.
- Color mode is RGBA.
- The file contains one static icon.
- The image is not an animation sheet or multi-panel grid.
- The icon has an opaque or intentionally complete icon background.
- The border is not cropped or broken.
- There is no text, watermark, or PixelLab UI artifact.

Technical hard fail:

- No valid candidate exists.
- Candidate dimensions cannot be reconciled with 80 x 80.
- The candidate is corrupt, incomplete, or not a PNG.
- The result belongs to another skill.

A technical hard fail prevents preservation as the selected source and prevents
Unity copy.

## 8. Candidate Selection and Preservation

1. Evaluate every technically valid candidate with `SkillIconEvaluationGuide.md`.
2. Record all candidate scores in `candidate_scores.txt`.
3. Select the highest-scoring candidate that has no fatal failure and scores at
   least 85.
4. Copy, rather than move, the selected candidate to:

```text
{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png
```

5. Do not resize, crop, recolor, quantize, recompress, or otherwise modify the
   selected file during preservation.
6. Record the selected candidate index and SHA-256.

If no candidate passes, preserve the candidate evidence and evaluation result, do
not create a selected source, and do not copy anything to Unity.

## 9. Generation Record

Save:

```text
{evaluationRoot}/{equipmentId}/generation_record.txt
```

Required fields:

```text
Skill ID:
Source JSON:
PixelLab Endpoint:
PixelLab Background Job ID or Result Page:
Description:
Style Description:
Style Reference Paths:
Seed:
Canvas:
No Background:
Candidate Count:
Selected Candidate:
Selected Source Path:
Selected SHA-256:
Download Date:
```

Never record the PixelLab API token.

## 10. Evaluation Result

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

## 11. Copy to Unity

After the preserved source receives `Pass`:

1. Confirm the preservation filename is `{equipmentId}.icon.png`.
2. Confirm the destination is exactly:

```text
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
```

3. If an accepted Unity icon already exists, stop unless replacement is explicitly
   authorized.
4. Copy the preserved source to the Unity destination.
5. Calculate SHA-256 for the preserved source and Unity destination.
6. If hashes differ, delete neither copy; report `checksum_mismatch` and stop before
   import completion.

Do not copy a failed candidate, a contact sheet, or `candidate_XX.png` directly into
Unity.

## 12. Unity Import Rules

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
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png.meta
```

Match the existing approved skill icon `.meta` format and resource import policy.
Do not reuse another icon's GUID.

If the correct Unity Editor cannot be run, report:

```text
meta configured / Unity reimport pending
```

Do not report import completion without editor evidence.

## 13. Resource and ID Validation

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

## 14. Cleanup

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

## 15. Completion Checklist

- [ ] PixelLab result matches `equipmentId`.
- [ ] Every candidate was downloaded and technically validated.
- [ ] Every valid candidate was scored.
- [ ] Selected candidate scored at least 85 with no fatal failure.
- [ ] Selected source is preserved outside Unity.
- [ ] Evaluation result is saved under the evaluation folder.
- [ ] Preserved and Unity filenames use the full `equipmentId`.
- [ ] Unity destination is under `Assets/Resources/skill/icon/skill`.
- [ ] Preserved and Unity SHA-256 values match.
- [ ] Unity `.meta` uses Sprite Single and a unique GUID.
- [ ] Runtime icon key resolves correctly or pending status is explicit.
- [ ] Temporary files were cleaned without deleting evidence.

## 16. Failure Output

```text
status: failed
failureType:
  - pixellab_result_mismatch
  - missing_download
  - invalid_png
  - invalid_icon_size
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
