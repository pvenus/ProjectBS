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

Example:

```text
characterName = seojin
grade = 1
imagePage = <image page url>
```

---

## Download

On the image page, use the `Export` button to download the character animation images.

After downloading, extract the archive.

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

Rename each PNG file using this format:

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

---

## Copy to Unity Resource Path

After renaming, copy all PNG files to this folder:

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

After copying the final PNG files, remove temporary files created during the download and extraction process.

Clean up:

- Downloaded archive files
- Extracted folders
- Intermediate temporary working folders

Only the final PNG files required by Unity should remain in `Assets/Resources/character/animation_png`.

---

## Validation Checklist

Before running the Unity character generator, check the following:

- Are the PNG files copied into `animation_png`?
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
-> Extract the archive
-> Check animations/{type}/{direction} files
-> Rename to character.{characterName}.{grade}.{animation_enum}.{frame}.png
-> Copy to Assets/Resources/character/animation_png
-> Remove archive files and temporary folders
-> Git commit
-> Merge to main
-> Deploy
```
