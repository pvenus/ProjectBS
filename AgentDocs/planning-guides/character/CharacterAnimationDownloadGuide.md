# Character Animation Download Guide


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
- **Responsibility:** export the resolved PixelLab animation, validate its source structure, and preserve immutable source evidence plus a download record.
- **Inputs:** generation record/provider identity, `PixelLabExportRoot`, and `targetCharacterFolder`.
- **Preconditions:** the Git owner confirmed readiness and the provider character identity is uniquely resolved.
- **Handoff:** preserved `animations/`, provenance/checksums, download status, and a separate evaluation request.
- **Mutation boundary:** no rubric evaluation, converted project-ready assets, project promotion, Unity metadata/import work, or Git publication.

The generation record controls provider identity, this guide controls export and
preservation, and `EvaluationAnimationGuide.md` controls the later verdict. Stop
with `download_authority_conflict` on any identity or source conflict.

## Purpose

This document describes the standard workflow for downloading character
animation images and preserving their immutable source structure for evaluation.

Repository synchronization and Git publication are separate owner tasks. This
guide starts only after the Git owner confirms a safe, current worktree and ends
with a validated artifact handoff.

---

## Repository Readiness Gate

Before starting, require confirmation from the Git owner that the designated
working branch contains the latest approved base and that no conflict or dirty
worktree blocker prevents this scoped operation. This guide does not run pull,
checkout, merge, reset, stash, commit, push, or deployment commands.

---

## Required Inputs

The task requires the following inputs.

| Input | Description |
|-------|-------------|
| PixelLabExportRoot | Root folder where character PixelLab animation folders and evaluation results are preserved. |
| targetCharacterFolder | Absolute path to the target character folder under `PixelLabExportRoot`. Derive `characterName` and `grade` from this folder or its metadata. |

Example:

```text
PixelLabExportRoot = {current_pc_pixellab_export_root}
targetCharacterFolder = {PixelLabExportRoot}/{characterName}_{grade}
```

---

## Download

Use the character folder under `PixelLabExportRoot` as the source of truth:

```text
<targetCharacterFolder>/animations
```

If this `animations` folder already exists for the target character, use it directly for structure validation, evaluation, conversion, and Unity copy unless the task explicitly asks for a new PixelLab export for that target character.

If the `animations` folder does not exist, or if the task explicitly targets the character for a new download/export, find and open the existing PixelLab character using the prompt that was used for image generation.

PixelLab character lookup:

1. Search `targetCharacterFolder` for the image generation prompt and result metadata.
2. Prefer the exact PixelLab prompt recorded by `AgentDocs/task-prompts/character/CharacterGenerateImagePrompt.md` / `AgentDocs/planning-guides/character/CharacterGenerateImage.md`.
3. If multiple prompt records exist, use the one associated with the passing image evaluation result.
4. Open `https://www.pixellab.ai/create-character` in Chrome.
5. On that page, use the prompt text, character name, and grade tags to search the existing PixelLab character.
6. Open the matched PixelLab character detail page from the search result.
7. Do not require `imagePage` as a manual input.
8. If the matching PixelLab character cannot be found on `https://www.pixellab.ai/create-character` from the image generation prompt/result data, stop and report a lookup failure.

After opening the matched PixelLab character, use the PixelLab `Export` button to download the character animation images.

For a specific target character, a new successful export is treated as a replacement for that character's previous animation source folder. Replace only that character's `animations/` folder.

Export handling:

1. Download and extract the PixelLab export into a temporary working folder.
2. Locate the extracted `animations/` folder.
3. Validate the temporary `animations/` folder with the Required Folder Structure Hard Fail rules.
4. If validation fails, stop immediately and do not replace the existing character `animations` folder.
5. If validation passes, replace the target character's existing `animations/` folder with the extracted `animations/` folder:

```text
<targetCharacterFolder>/animations
```

Do not move the whole extracted archive folder. Only the `animations/` folder becomes the preserved PixelLab source result for the target character.

After the `animations/` folder is moved into the character folder, the temporary downloaded archive and extracted wrapper folders can be cleaned up.

Do not use the source files inside `animations/` as the renamed Unity resource files directly.

The character animation source folder should contain this structure:

```text
<targetCharacterFolder>/animations/
  idle/
  move/
  attack/
```

Each animation type folder should contain directional folders:

```text
south-east/
south-west/
north-east/
north-west/
```

Recommended preserved export structure:

```text
<targetCharacterFolder>/
  animations/
    idle/
    move/
    attack/
  converted/
    character.{characterName}.{grade}.{animation_enum}.{frame}.png
  evaluation_animation_result.txt
```

`animations/` is the preserved PixelLab source result and is used for evaluation.

`converted/` is a legacy downstream artifact and is not created by the current
download/preservation stage.

## Required Folder Structure Hard Fail

Before evaluation, renaming, conversion, or project image promotion, validate the character `animations` folder structure.

Immediately mark the work as failed and stop processing if any required structure is incomplete.

Hard fail conditions:

- `<targetCharacterFolder>/animations/` is missing after the download-or-use-existing step.
- Any required animation type folder is missing: `idle`, `move`, `attack`.
- Any required source direction folder is missing for a required animation type: `south-east`, `south-west`.
- A required source direction folder exists but contains no PNG frames.
- `south-east` and `south-west` frame counts do not match for the same animation type.
- Required source frames are incomplete, unreadable, or cannot be used for Missing Direction Rule duplication.

The `north-east` and `north-west` folders are not hard fail conditions by themselves when the matching south-facing source folders are complete. In that case, continue using the Missing Direction Rule.

When a hard fail occurs:

- Do not run animation evaluation.
- Do not create converted files.
- Do not copy files into `Assets/ImagesGenerated`.
- Preserve any existing character `animations` folder when a new export failed validation.
- Save the failure reason to `evaluation_animation_result.txt` if the character export folder is available.
- Report the failure as a folder structure failure in the final summary.

## PixelLab South-West Mirroring

After generating each animation in PixelLab, duplicate the generated `south-east` direction to `south-west` with the PixelLab south-west mirror button before exporting.

Apply this rule immediately after each animation is generated:

- Source direction: `south-east`
- Target direction: `south-west`
- Required animation types: `Walk`, `Attack`, `Idle`, and any additional generated animation
- Keep the animation name unchanged. Only add the mirrored direction frames.

Before using the `Export` button, confirm that every generated animation contains both the original `south-east` direction and the mirrored `south-west` direction in PixelLab.

---

## Legacy Evaluation and Promotion Appendix (Non-executable)

This section and all following conversion, promotion, Canvas, cleanup, and Git
handoff sections are historical reference only. Current execution stops after
source preservation and structure validation, then hands off to immutable
evaluation. No downstream mutation may be performed from this guide.

### Animation Evaluation (Legacy)

Evaluate the character animation source folder before promoting images into
`Assets/ImagesGenerated`.

Use the preserved source files:

```text
<targetCharacterFolder>/animations
```

Perform the evaluation according to:

```text
AgentDocs/planning-guides/character/EvaluationAnimationGuide.md
```

Do not add a separate rotation evaluation in this download workflow. Character image rotations should already be available from the image generation evaluation stage; this workflow only validates animation folder structure, animation quality, direction folders, and frame resources.

Evaluation must check:

- Frame-to-frame movement score
- Weapon review score
- Walk animation score
- Attack animation score
- Pass / Fail result
- Failure reason, if failed
- Missing direction notes, if any
- Additional notes, if needed

Save the evaluation result here:

```text
<targetCharacterFolder>/evaluation_animation_result.txt
```

Evaluation does not block creation of `converted/` evaluation copies.

Only `Pass` may proceed from `converted/` to
`Assets/ImagesGenerated/Character/animation`. `Fail` must preserve the source,
converted evidence, and failure report, but must not copy or overwrite any
project image. Folder-structure hard failures still stop the workflow before
evaluation and conversion.

Do not delete the source animation images used for evaluation.

---

### Animation Enum Mapping (Legacy)

Map source direction folders to the ProjectBS `CharacterAnimationClipType` enum names.

| Animation Type | Direction | Animation Enum |
|----------------|-----------|----------------|
| idle | south-east | IdleDownRight |
| idle | south-west | IdleDownLeft |
| idle | north-east | IdleUpRight |
| idle | north-west | IdleUpLeft |
| move | south-east | MoveDownRight |
| move | south-west | MoveDownLeft |
| move | north-east | MoveUpRight |
| move | north-west | MoveUpLeft |
| attack | south-east | AttackDownRight |
| attack | south-west | AttackDownLeft |
| attack | north-east | AttackUpRight |
| attack | north-west | AttackUpLeft |

If Death animations are downloaded separately, use the same direction mapping.

| Animation Type | Direction | Animation Enum |
|----------------|-----------|----------------|
| death | south-east | DeathDownRight |
| death | south-west | DeathDownLeft |
| death | north-east | DeathUpRight |
| death | north-west | DeathUpLeft |

### Missing Direction Rule

Some exports may not include the `north-east` or `north-west` animation folders.

If either folder is missing, duplicate the corresponding south-facing images before applying the file naming rules.

| Missing Direction | Use Images From |
|-------------------|-----------------|
| north-east | south-east |
| north-west | south-west |

The duplicated images should then be renamed using the appropriate `CharacterAnimationClipType` enum:

- `north-east` ??`MoveUpRight`, `IdleUpRight`, `AttackUpRight`, `DeathUpRight`
- `north-west` ??`MoveUpLeft`, `IdleUpLeft`, `AttackUpLeft`, `DeathUpLeft`

The duplicated files should be treated exactly the same as normal downloaded images.

---

### File Naming Rules (Legacy)

Copy each source PNG from the preserved `animations/` folder into the character export folder's `converted/` folder, then rename the copied file using this format:

```text
character.{characterName}.{grade}.{animation_enum}.{original_frame_name}.png
```

`original_frame_name` must be copied from the original file name without the file extension.

Examples:

```text
Original file:
animations/idle/south-east/frame_000.png

Renamed file:
character.seojin.1.IdleDownRight.frame_000.png
```

```text
Original file:
animations/attack/north-west/frame_005.png

Renamed file:
character.seojin.1.AttackUpLeft.frame_005.png
```

Important rules:

- `characterName` must match the character ID.
- `grade` must match the character grade and must appear immediately after `characterName`.
- `animation_enum` must exactly match a `CharacterAnimationClipType` enum name.
- Preserve the original frame name, such as `frame_000` or `frame_001`.
- Keep the `.png` extension.
- Do not rename or move the source PixelLab files inside `<targetCharacterFolder>/animations`.
- Missing direction duplicates are created as renamed copies in `converted/`; do not modify the source `animations/` folder.

---

### Promote to Unity Generated-Image Path (Legacy)

After the animation evaluation returns `Pass`, copy all converted PNG files to
this folder:

```text
Assets/ImagesGenerated/Character/animation
```

The Unity generator searches this path using the following pattern:

```text
character.{characterName}.{grade}.{animation_enum}*
```

Before running the generator, inspect its configured sprite folder. If
`CharacterJsonGenerator` still points to
`Assets/Resources/character/animation_png`, stop with
`builder_path_migration_required`. Do not duplicate the passing PNGs back into
`Assets/Resources` as a workaround. The builder must be migrated to
`Assets/ImagesGenerated/Character/animation` in a separate implementation task.

The generator sorts the matched sprites in ascending order and creates an AnimationClip.

Generated AnimationClips are saved here:

```text
Assets/Resources/character/animation_clip
```

Generated AnimationClip file names use this format:

```text
character.{characterName}.{grade}.{animation_enum}.clip
```

---


### Canvas Animation GIF Evidence (Legacy)

When downloaded character animations are later prepared for Slack Canvas review,
create GIF evidence from the preserved PNG frames instead of modifying source
animation files.

Evaluation workspace after rebasing PixelLab output:

```text
{DesignEvaluationRoot}/character/{characterName}_{grade}
```

Canvas-ready GIF evidence should be written under:

```text
<evaluationCharacterFolder>/evidence/animation_gif_by_type/
  {characterName}_{grade}_idle_all_directions.gif
  {characterName}_{grade}_move_all_directions.gif
  {characterName}_{grade}_attack_all_directions.gif
<evaluationRoot>/character_animation_gif_by_type_manifest.json
```

GIF construction rules:

- Create separate GIFs for `idle`, `move`, and `attack`.
- Each GIF must show all ProjectBS directions in one view:
  `DownRight`, `DownLeft`, `UpRight`, `UpLeft`.
- Use `converted/` PNG frames as the source for the Canvas GIFs so missing
  north-facing directions are represented by the same duplicated copies that
  Unity receives.
- Do not modify or rename files in `animations/` or `converted/` while creating
  GIF evidence.
- The GIFs are review/playback evidence only. Folder-structure validation,
  direction handling, naming validation, and Unity-copy validation still come
  from PNG frames and `evaluation_animation_result.txt`.
- If GIF creation fails, do not treat the animation download/conversion itself
  as failed. Report the GIF evidence failure separately for Canvas publication.

For each character, a complete Canvas evidence package should include one static
rotation/contact preview and the three animation GIFs above.

### Cleanup (Legacy)

After copying the final PNG files, remove only temporary working files that are outside the character export folder.

Clean up:

- Browser download cache copies, if separately created
- Intermediate temporary working folders outside `<targetCharacterFolder>`
- Any duplicate scratch folders created only for processing

Do not delete:

- `<targetCharacterFolder>/animations`
- `<targetCharacterFolder>/converted`
- `<targetCharacterFolder>/evaluation_animation_result.txt`

The Unity generated-image folder should contain only passing copied PNG files:

```text
Assets/ImagesGenerated/Character/animation
```

The PixelLab export folder should retain the source `animations/` folder, converted copies, and evaluation result.

---

### Validation Checklist (Legacy)

Before running the Unity character generator, check the following:

- Does each generated animation contain both `south-east` and PixelLab-mirrored `south-west` before export?
- Did the character `animations/` folder pass the Required Folder Structure Hard Fail check?
- If a new export was required, was only the extracted `animations/` folder moved into the character folder?
- Are source animation files preserved for evaluation?
- Does `evaluation_animation_result.txt` exist under the character export folder?
- Does the evaluation result include Pass / Fail and failure reason if failed?
- If evaluation passed, are the PNG files copied into `Assets/ImagesGenerated/Character/animation`?
- If evaluation failed, was project image copy correctly skipped?
- Are renamed PNG files also preserved under `<targetCharacterFolder>/converted`?
- Do file names follow `character.{characterName}.{grade}.{animation_enum}.frame_000.png`?
- Does `animation_enum` exactly match `CharacterAnimationClipType`?
- Are any frames missing for each animation direction?
- Does `characterName` match the `characterId` in CharacterSO?
- Does the character generator resolve
  `Assets/ImagesGenerated/Character/animation`, or was the build correctly
  blocked as `builder_path_migration_required`?

---

### Git Handoff (Legacy)

After validation, report the changed project-relative files, preserved evidence,
evaluation result, checksums, Unity readiness, and any blocker. A separate Git
owner decides whether and how to stage, commit, merge, push, or deploy. Do not
request Git publication before the required evaluation has passed.

---

### Summary (Legacy)

Overall workflow:

```text
Confirm repository readiness with the Git owner
-> Use existing <targetCharacterFolder>/animations if present and no new export is required
-> If animations is missing or replacement is required, Export from PixelLab into a temporary folder
-> Move only the extracted animations folder into <targetCharacterFolder>/animations
-> Check animations/{type}/{direction} files
-> Stop immediately if Required Folder Structure Hard Fail conditions are found
-> Evaluate source animations using EvaluationAnimationGuide.md
-> Save evaluation_animation_result.txt under the character export folder
-> Copy and rename files into <targetCharacterFolder>/converted
-> Copy converted files to Assets/ImagesGenerated/Character/animation
-> Remove only temporary files outside the character export folder
-> Return validation and artifact handoff to the separate Git owner
```
