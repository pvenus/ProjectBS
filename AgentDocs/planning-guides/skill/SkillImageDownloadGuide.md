# Skill Image Animation Download Guide

> Deprecated execution contract. Retained as sheet preservation/extraction
> evidence. Replaced by
> `AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md`
> using the adapter declared by
> `AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md`.


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
- **Responsibility:** download the PixelLab reference and animation deliverables, validate source integrity, and preserve immutable evaluation inputs.
- **Inputs:** generation record, provider result references, full `skillId`, and resolved `{evaluationRoot}`.
- **Preconditions:** the result is eligible, uniquely identified, and both deliverables are available.
- **Handoff:** preserved reference/animation files, checksums, download record, and a separate evaluation request.
- **Mutation boundary:** no scoring/verdict, project copy, slicing, Unity clip/meta work, cleanup of required evidence, or Git work.

The generation record controls provenance, this guide controls download and
preservation, and `SkillImageEvaluationGuide.md` controls the later verdict.
Conflicts stop with `download_authority_conflict`.

## 1. Purpose

This guide defines the download/preservation workflow for a PixelLab skill VFX
reference image and animation.

Use this guide after `SkillImageGenerationGuide.md`. Generation remains a two-stage process:

1. Create and select a reference image.
2. Animate the selected reference image.

The reference export and animation export are different deliverables and must never overwrite each other.

Do not run this workflow for a melee basic attack (`.basic_attack.` with `cast.range <= 1.0`), because that skill must not have a separately generated skill animation.

## 2. Required Inputs and Paths

```text
projectRoot = {current_project_root}
evaluationRoot = {current_pc_skill_animation_evaluation_root}
skillId = full equipment skill id
skillSlug = short filesystem-safe descriptive name
```

Future PASS-only promotion targets (informational):

```text
Assets/ImagesGenerated/Skill/animation_reference/{skillId}.animation_ref.png
Assets/ImagesGenerated/Skill/animation/{skillId}.animation.png
```

This is the required target contract. Before running the builder, inspect its
configured sprite folder. If `SkillBaseVisualAssetBuilder` still points to
`Assets/Resources/skill/animation_png`, stop with
`builder_path_migration_required`. Do not duplicate the passing animation into
the legacy Resources path as a workaround.

Preserved evaluation folder:

```text
{evaluationRoot}/{skillSlug}/
  reference/
    {skillId}.animation_ref.png
  animation/
    {skillId}.animation.png
  evaluation/
    evaluation_result.txt
  generation_record.txt
```

`skillSlug` is only the folder label. Unity filenames must always use the full `skillId`.

## 3. Download and Classification

1. Confirm that the opened PixelLab result belongs to the requested skill.
2. Download the selected reference image result separately from the animation result.
3. Classify files by their PixelLab source page and visible content; do not classify only by browser download name.
4. Confirm that the reference export contains the selected image or variation sheet.
5. Confirm that the animation export contains the final ordered animation sprite sheet.
6. If either deliverable is missing or ambiguous, stop before Unity copy.
7. If PixelLab returns an archive, extract it into a temporary folder outside the final evaluation folder.
8. Do not preserve wrapper folders, duplicate previews, thumbnails, or unrelated exports as production inputs.

## 4. Source Validation Before Copy

Validate both PNG files before renaming:

- PNG decoding succeeds.
- Width and height are non-zero.
- Alpha channel exists.
- Transparent background is present.
- No effect pixel or glow touches an outer edge.
- The animation sheet dimensions are exactly divisible by the intended frame cell width and height.
- The calculated rows × columns equals the observed frame count.
- Frame order is left-to-right, then top-to-bottom unless the PixelLab export explicitly documents another order.
- Empty padding cells are not counted as animation frames.

Hard fail before copy:

- Missing reference or animation PNG.
- Opaque background.
- Cropped/edge-touching content.
- Unknown frame order.
- Sheet dimensions that cannot be divided into equal frame cells.
- Requested and observed frame counts cannot be reconciled.

## 5. Rename and Preserve Evaluation Copies

Create the target evaluation structure and copy, rather than move, the validated PNG files:

```text
reference/{skillId}.animation_ref.png
animation/{skillId}.animation.png
```

Preserve the exact files used for Unity import and evaluation. Do not resize, recompress, trim transparent pixels, alter color mode, or rebuild the sheet differently between the evaluation copy and Unity copy.

Save `generation_record.txt` with:

```text
Skill ID:
Source JSON:
PixelLab Page:
Reference Prompt:
Animation Prompt:
Selected Variation:
Canvas / Frame Cell Size:
Sheet Width / Height:
Rows / Columns:
Requested / Observed Frames:
Loop Mode:
Download Date:
Reference SHA-256:
Animation SHA-256:
```

## 6. Legacy Evaluation, Promotion, and Unity Appendix (Non-executable)

This section and all following Unity, clip, evaluation, cleanup, completion, and
failure-output sections describe the former combined workflow. Current execution
stops after Section 5 and hands immutable evidence to a separate evaluation task.

### Copy to Unity (Legacy)

Do not execute this section before completing the evaluation in Section 9.
Only `Pass` with no fatal failure may be promoted to
`Assets/ImagesGenerated`. `Conditional Pass`, `Fail`, and
`insufficient_evidence` remain in the evaluation workspace and must not be
copied to the project image path.

Copy the preserved evaluation files to:

```text
reference/{skillId}.animation_ref.png
  -> Assets/ImagesGenerated/Skill/animation_reference/{skillId}.animation_ref.png

animation/{skillId}.animation.png
  -> Assets/ImagesGenerated/Skill/animation/{skillId}.animation.png
```

After copy, confirm that source and destination SHA-256 values match.

Never copy a reference sheet into `animation`, or an animation sheet into `animation_reference`.

### Unity Import and Slice Rules (Legacy)

PNG 복사만으로 Unity 반영이 완료된 것으로 간주하지 않는다. PNG를 Unity 대상 경로에 복사한 직후 반드시 해당 PNG의 `.meta`를 생성하거나 갱신하고, 실제 시트 크기와 셀 크기에 맞는 Sprite Multiple 슬라이스를 구성해야 한다.

필수 처리 순서:

1. Unity 대상 PNG 복사 및 SHA-256 일치를 확인한다.
2. 실제 PNG 크기를 셀 크기로 나누어 columns, rows, 전체 셀 수를 다시 계산한다.
3. 모든 셀이 비어 있지 않은지 확인하여 usable frame count를 확정한다.
4. 대상 PNG와 같은 경로에 `{filename}.png.meta`를 생성하거나 갱신한다.
5. `.meta`에 Sprite Mode Multiple, Point filter, mipmap disabled, alpha transparency enabled, default compression None을 설정한다.
6. Sprite rect는 Unity 좌표계를 사용하되 재생 순서는 원본 시트의 왼쪽 위에서 오른쪽 아래로 유지한다.
7. animation은 `frame_00`부터, variation sheet는 `variation_00`부터 누락 없이 이름을 기록한다.
8. 올바른 Unity Editor 버전에서 reimport한 뒤 실제 Sprite sub-asset 수와 이름을 확인한다.

올바른 Unity Editor 버전을 실행할 수 없는 환경에서는 `.meta` 구성과 정적 검증까지만 완료하고 `slice configured / Unity reimport pending`으로 보고한다. 이 상태를 `Unity slice verified` 또는 clip 생성 완료로 보고하지 않는다.

Animation sheet:

- Texture Type: Sprite (2D and UI).
- Sprite Mode: Multiple.
- Filter Mode: Point.
- Compression: None for the default platform.
- Alpha Is Transparency: enabled.
- Mip Maps: disabled.
- Pivot: center.
- Slice by exact cell size or exact columns × rows derived from the exported sheet.
- Sprite names: `{skillId}.animation.frame_00`, `_01`, and so on in playback order.

Reference sheet:

- Keep it separate from runtime animation frames.
- If it contains four generated variations, slice it as 2 columns × 2 rows.
- Sprite names: `{skillId}.animation_ref.variation_00`, `_01`, and so on.
- If PixelLab exports only the selected single reference image, import it as a single sprite; do not invent a 2×2 grid.

Do not assume a fixed 3×3 animation grid. Derive columns and rows from the actual sheet and frame cell size. For example, a 384×384 sheet with 128×128 cells is 3×3 and contains 9 cells.

### Animation Clip Verification (Legacy)

`SkillBaseVisualAssetBuilder` resolves:

```text
Assets/ImagesGenerated/Skill/animation/{skillId}.animation.png
```

and recreates:

```text
{visualId}.loop.anim
```

Verify after running the skill builder:

- The sheet has sliced Sprite sub-assets.
- Frame suffixes sort in numeric order.
- Generated clip frame count equals the usable animation frame count.
- Sample rate is 12 FPS unless the implementation or skill data explicitly overrides it.
- The clip is registered as `ProjectileLoop`.
- The generated `BaseVisualSO` references the clip.

The current builder creates a looping `ProjectileLoop` clip. A one-shot or Hit animation requires separate builder/runtime support and must not be claimed as automatically supported.

`SkillBaseVisualAssetBuilder`는 PNG 복사 직후의 슬라이스 처리보다 나중에 실행한다. Sprite sub-asset이 실제로 임포트되지 않은 상태에서는 빌더를 실행하거나 clip 생성 성공으로 보고하지 않는다.

### Evaluation (Legacy)

Evaluate the preserved files under `{evaluationRoot}/{skillSlug}` using `SkillImageEvaluationGuide.md`.

Evaluation evidence must include:

- Preserved reference PNG.
- Preserved animation PNG.
- Every sliced animation frame.
- Playback in frame order.
- Source skill JSON and generation record.

Save the result to:

```text
{evaluationRoot}/{skillSlug}/evaluation/evaluation_result.txt
```

Record the exact asset paths, requested and observed frame counts, grid, fatal checks, category scores, final result, and required corrections. Do not mark Pass when individual frames or alpha/edge checks cannot be inspected; use `insufficient_evidence`.

Quality Fail does not delete evidence. Preserve the failed files and evaluation
result until a replacement is accepted. Any result other than `Pass`, as well as
a technical hard fail, prevents project image copy.

### 9.1 Existing Evaluation Migration

When an already completed production animation is migrated into the shared
evaluation workspace, use `format_existing` semantics. Migration does not
authorize generation, re-scoring, image correction, or a production overwrite.

The migration workspace is resolved beneath the established evaluation root:

```text
{evaluationRoot}/{skillId}/
  input\evaluation_input.json
  input\evaluation_prompt.md
  source\{skillId}.animation_ref.png
  source\{skillId}.animation.png
  evaluation\evaluation_result.json
  evaluation\evaluation_report.md
  evaluation\evaluation_canvas.md
  evaluation\evidence\frame_00.png
  evaluation\evidence\contact_sheet.png
  evaluation\evidence\playback.gif
  evaluation\evidence\technical_validation.json
```

Migration rules:

- Copy the exact preserved reference and animation PNG bytes; never move the
  preserved files or the Unity production files.
- Verify preserved, staged, and Unity destination SHA-256 values before
  recording copy verification.
- Preserve the existing result, scores, findings, and required actions without
  re-scoring.
- Slice row-major PNG frames from the exact animation sheet. Record source cell
  index, frame order, usable frame count, and per-frame SHA-256.
- Keep `contact_sheet.png` as review evidence derived from the exact animation
  sheet.
- Create `playback.gif` only as review evidence. Record nominal FPS, encoded
  frame delays, loop mode, and GIF SHA-256.
- Judge alpha, edge, corner, and crop status from the original PNG sheets and
  individual PNG frames, never from GIF quantization or GIF playback.
- Record Unity `.meta`, Editor reimport, clip, and runtime binding as separate
  states. A configured `.meta` does not prove Editor reimport or runtime
  binding.
- Exclude failed, blocked, or never-generated assets from completed migration
  and initial Slack publication.

### Cleanup (Legacy)

After preservation, Unity copy, checksum verification, and evaluation result creation:

- Delete downloaded ZIP archives.
- Delete temporary extraction wrapper folders.
- Delete duplicate browser-download copies and unrelated thumbnails.
- Keep the preserved `reference`, `animation`, `evaluation`, and `generation_record.txt` files.
- Keep Unity PNG and `.meta` files.
- Do not delete a previous passing evaluation when replacing an asset; archive it or record replacement history first.

### Completion Checklist (Legacy)

- [ ] Correct PixelLab skill result identified.
- [ ] Reference and animation downloaded separately.
- [ ] Both PNGs decode and contain alpha transparency.
- [ ] No frame is cropped or touches an edge.
- [ ] Sheet size, cell size, columns, rows, and observed frame count recorded.
- [ ] Evaluation folder contains reference and animation copies.
- [ ] Unity filenames use the full `skillId`.
- [ ] Reference copied only to `animation_reference`.
- [ ] Animation copied only to `animation`.
- [ ] Unity meta uses Sprite Multiple and correct grid for the animation.
- [ ] Unity 대상 PNG마다 같은 경로에 `.png.meta`가 존재한다.
- [ ] `.meta`의 rect 수가 usable frame/variation 수와 일치한다.
- [ ] Sprite names and numeric frame order are correct.
- [ ] 올바른 Unity Editor에서 reimport 후 실제 Sprite sub-asset 수를 확인했거나, 실행 불가 시 `Unity reimport pending`으로 명시했다.
- [ ] Generated clip frame count and `ProjectileLoop` registration verified.
- [ ] Builder sprite folder uses `Assets/ImagesGenerated/Skill/animation`, or
      build execution is blocked as `builder_path_migration_required`.
- [ ] `evaluation_result.txt` saved using the evaluation guide.
- [ ] Evaluation result is `Pass` before project image copy.
- [ ] Source/destination checksums match.
- [ ] ZIP and temporary extraction files deleted.

### Failure Output (Legacy)

```text
status: failed
failureType:
  - pixellab_result_mismatch
  - missing_reference_export
  - missing_animation_export
  - invalid_png
  - missing_alpha
  - cropped_or_edge_contact
  - invalid_sheet_grid
  - frame_count_mismatch
  - unity_copy_failed
  - unity_slice_failed
  - clip_generation_failed
  - builder_path_migration_required
  - insufficient_evidence
failureReason:
preservedFiles:
cleanupStatus:
nextAction:
```
