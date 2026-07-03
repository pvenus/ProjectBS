# Character Animation Download Guide

## Purpose

This document describes the standard workflow for downloading character animation images, renaming them to match ProjectBS file naming rules, and copying them into the Unity resource path.

The workflow covers the story agent branch flow: update Git state, download images, rename files, copy resources, clean temporary files, commit, merge, and deploy.

---

## Git Update

Before starting, update the project repository.

Standard flow:

1. Pull the latest `main` branch.
2. Switch to the working branch, `story`.
3. Merge `main` into `story`.
4. Resolve conflicts before starting the asset work.

Example:

```bash
cd <ProjectRoot>

git checkout main
git pull origin main

git checkout story
git merge main
```

---

## Required Inputs

The task requires the following inputs.

| Input | Description |
|-------|-------------|
| characterName | Character name. Used in file names as `character.{characterName}.{grade}`. |
| grade | Character grade. Used in file names immediately after `characterName`, and may also determine which image page or export to use. |
| imagePage | Image page URL or working page used to download the character animation images. |
| PixelLabExportRoot | Root folder where PixelLab exports and evaluation results are preserved. |

Example:

```text
characterName = seojin
grade = 1
imagePage = <image page url>
PixelLabExportRoot = /Users/pvenus/ProjectBS/PixelLabExports
```

---

## Download

On the image page, use the `Export` button to download the character animation images.

Save the downloaded archive under the character export folder:

```text
<PixelLabExportRoot>/<CharacterName>_<Grade>
```

After downloading, extract the archive inside the same character export folder.

Do not use the extracted original files as the renamed Unity resource files directly. Preserve the downloaded archive and extracted original files as the PixelLab source result.

The extracted folder should normally contain this structure:

```text
animations/
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
<PixelLabExportRoot>/<CharacterName>_<Grade>/
  original/
    <downloaded_archive>.zip
    extracted/
      animations/
        idle/
        move/
        attack/
  converted/
    character.{characterName}.{grade}.{animation_enum}.{frame}.png
  evaluation_animation_result.txt
```

`original/` is the preserved PixelLab source result and is used for evaluation.

`converted/` is the renamed copy used before copying into Unity resources.

## Required Folder Structure Hard Fail

Before evaluation, renaming, conversion, or Unity resource copy, validate the extracted animation folder structure.

Immediately mark the work as failed and stop processing if any required structure is incomplete.

Hard fail conditions:

- `animations/` is missing.
- Any required animation type folder is missing: `idle`, `move`, `attack`.
- Any required source direction folder is missing for a required animation type: `south-east`, `south-west`.
- A required source direction folder exists but contains no PNG frames.
- `south-east` and `south-west` frame counts do not match for the same animation type.
- Required source frames are incomplete, unreadable, or cannot be used for Missing Direction Rule duplication.

The `north-east` and `north-west` folders are not hard fail conditions by themselves when the matching south-facing source folders are complete. In that case, continue using the Missing Direction Rule.

When a hard fail occurs:

- Do not run animation evaluation.
- Do not create converted files.
- Do not copy files into Unity resources.
- Preserve the downloaded archive and extracted original files.
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

## Animation Evaluation

Evaluate the downloaded animation images after extraction and before renaming/copying them into Unity resources.

Use the preserved original extracted files:

```text
<PixelLabExportRoot>/<CharacterName>_<Grade>/original/extracted/animations
```

Perform the evaluation according to:

```text
Assets/character_concepts/game_prompt_guide/character/EvaluationAnimationGuide.md
```

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
<PixelLabExportRoot>/<CharacterName>_<Grade>/evaluation_animation_result.txt
```

Evaluation does not block conversion.

Regardless of Pass / Fail, continue the conversion and copy process so the generated resources can be reviewed in Unity. If evaluation fails, record the failure reason in `evaluation_animation_result.txt` and report it in the final summary.

This non-blocking rule applies only to animation quality evaluation failures. Folder structure hard failures must stop the workflow before evaluation and conversion.

Do not delete the original extracted animation images used for evaluation.

---

## Animation Enum Mapping

Map downloaded direction folders to the ProjectBS `CharacterAnimationClipType` enum names.

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

- `north-east` → `MoveUpRight`, `IdleUpRight`, `AttackUpRight`, `DeathUpRight`
- `north-west` → `MoveUpLeft`, `IdleUpLeft`, `AttackUpLeft`, `DeathUpLeft`

The duplicated files should be treated exactly the same as normal downloaded images.

---

## File Naming Rules

Copy each source PNG from the preserved original extraction into the character export folder's `converted/` folder, then rename the copied file using this format:

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
- Do not rename or move the original PixelLab files inside `original/extracted`.
- Missing direction duplicates are created as renamed copies in `converted/`; do not modify the original extracted folders.

---

## Copy to Unity Resource Path

After creating renamed files in `converted/`, copy all converted PNG files to this folder:

```text
Assets/Resources/character/animation_png
```

The Unity generator searches this path using the following pattern:

```text
character.{characterName}.{grade}.{animation_enum}*
```

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

## Cleanup

After copying the final PNG files, remove only temporary working files that are outside the character export folder.

Clean up:

- Browser download cache copies, if separately created
- Intermediate temporary working folders outside `<PixelLabExportRoot>/<CharacterName>_<Grade>`
- Any duplicate scratch folders created only for processing

Do not delete:

- `<PixelLabExportRoot>/<CharacterName>_<Grade>/original`
- `<PixelLabExportRoot>/<CharacterName>_<Grade>/converted`
- `<PixelLabExportRoot>/<CharacterName>_<Grade>/evaluation_animation_result.txt`

The Unity resource folder should contain the final copied PNG files:

```text
Assets/Resources/character/animation_png
```

The PixelLab export folder should retain the original source result, converted copies, and evaluation result.

---

## Validation Checklist

Before running the Unity character generator, check the following:

- Does each generated animation contain both `south-east` and PixelLab-mirrored `south-west` before export?
- Did the extracted folder structure pass the Required Folder Structure Hard Fail check?
- Is the downloaded archive preserved under `<PixelLabExportRoot>/<CharacterName>_<Grade>/original`?
- Are original extracted animation files preserved for evaluation?
- Does `evaluation_animation_result.txt` exist under the character export folder?
- Does the evaluation result include Pass / Fail and failure reason if failed?
- Are the PNG files copied into `animation_png`?
- Are renamed PNG files also preserved under `<PixelLabExportRoot>/<CharacterName>_<Grade>/converted`?
- Do file names follow `character.{characterName}.{grade}.{animation_enum}.frame_000.png`?
- Does `animation_enum` exactly match `CharacterAnimationClipType`?
- Are any frames missing for each animation direction?
- Does `characterName` match the `characterId` in CharacterSO?

---

## Git Commit

After completing the work, commit with a standard message.

Example:

```bash
git status
git add .
git commit -m "Add character animation resources for seojin"
```

Commit message format:

```text
Add character animation resources for {characterName}
```

For updates:

```text
Update character animation resources for {characterName}
```

---

## Merge to Main and Deploy

After validation on the working branch, merge into `main` and deploy.

Standard flow:

```bash
git checkout main
git pull origin main
git merge story
git push origin main
```

If deployment is handled by a separate script or CI pipeline, follow the project standard deployment process.

---

## Summary

Overall workflow:

```text
Update Git state
-> Download Export from the image page
-> Save archive under <PixelLabExportRoot>/<CharacterName>_<Grade>/original
-> Extract the archive under original/extracted
-> Check animations/{type}/{direction} files
-> Stop immediately if Required Folder Structure Hard Fail conditions are found
-> Evaluate original extracted images using EvaluationAnimationGuide.md
-> Save evaluation_animation_result.txt under the character export folder
-> Copy and rename files into <PixelLabExportRoot>/<CharacterName>_<Grade>/converted
-> Copy converted files to Assets/Resources/character/animation_png
-> Remove only temporary files outside the character export folder
-> Git commit
-> Merge to main
-> Deploy
```
