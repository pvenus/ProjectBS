# Character Create Guide

## Purpose

This document defines the full character creation pipeline.

Use this guide as the orchestration document when creating a character from planning data through runtime JSON inputs.

Pipeline:

```text
Character planning
  -> Image generation
  -> Animation extraction
  -> Skill JSON generation
  -> Character JSON generation
```

---

## Global Rules

- Follow the step order in this document.
- Use the referenced guide files for detailed rules in each step.
- Generate JSON first when a generator guide requires JSON input.
- Do not create Unity SO assets directly unless a guide explicitly says to do so.
- For Player, Npc, and Boss character-owned runtime data, use the `character` domain.
- `Player`, `Npc`, and `Boss` are character types, not resource ID domains.
- Character-owned skill IDs must use `skill.character`, not `skill.npc`.

---

## Step 1. Character Planning

### Purpose

Create the planning JSON that defines the character concept, role, stat intent, skill intent, and generation source data.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
Assets/character_concepts/game_prompt_guide/character/CharacterStatGuide.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillBalanceGuide.md
Assets/Doc
```

### Main Work

1. Determine the character type: `Player`, `Npc`, or `Boss`.
2. Define the character identity, role, grade, type, tag, and story use.
3. Assign planning score and stat intent.
4. Design the expected skill slots and behavior.
5. Balance skill intent using target score, cooldown, cast range, hit range, and utility rules.

### Output

Save planning JSON under:

```text
Assets/Doc/Character
```

Example:

```text
Assets/Doc/Character/sangui_spirit_npc_group.json
```

### Validation

- Planning data clearly identifies the target character.
- Character type is selected before stat and skill design.
- NPC rules affect composition and upgrades only.
- NPC rules do not change runtime resource domains to `npc`.
- Skill intent includes enough data for the skill JSON step.

---

## Step 2. Image Generation

### Purpose

Generate the character concept image in PixelLab using the planning JSON as the source.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/character/CharacterGenerateImage.md
```

### Main Work

1. Open PixelLab:

```text
https://www.pixellab.ai/create-character
```

2. Use **Create from text**.
3. Select the correct PixelLab character type.
4. Use the required generation settings:

```text
Generation Mode: Pro
Camera View: High Top-Down
Detail: Highly detailed
Outline: Black outline
```

5. Write the prompt from the character planning JSON.
6. Generate the character in PixelLab.
7. Add the character name and grade as tags.
8. Export the generated image files.

### Output

Save PixelLab exports under the configured PixelLab export root:

```text
<PixelLabExportRoot>/<CharacterName>_<Grade>
```

### Validation

- Image is generated in PixelLab only.
- Rotation validation score is at least `90 / 100`.
- Prompt accuracy score is at least `80 / 100`.
- Style compatibility score is at least `70 / 100`.
- Evaluation result is saved as `evaluation_result.txt`.

---

## Step 3. Animation Extraction

### Purpose

Download the character animation export, rename animation PNGs, and copy them into the Unity resource path used by the CharacterSO generator.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/character/CharacterAnimationDownloadGuide.md
```

### Main Work

1. Export animation images from the PixelLab character page.
2. Extract the downloaded archive.
3. Map PixelLab animation folders to `CharacterAnimationClipType`.
4. Duplicate missing north-facing directions if required by the guide.
5. Rename every PNG using the ProjectBS naming rule.
6. Copy the renamed PNG files into the Unity resource folder.
7. Clean temporary downloaded and extracted files.

### Naming Rule

```text
character.{characterName}.{grade}.{animation_enum}.{original_frame_name}.png
```

Example:

```text
character.mist_lingering_child.1.IdleDownRight.frame_000.png
```

### Output

Copy renamed PNG files to:

```text
Assets/Resources/character/animation_png
```

The generator later creates animation clips under:

```text
Assets/Resources/character/animation_clip
```

### Validation

- PNG files exist in `Assets/Resources/character/animation_png`.
- File names match `character.{characterName}.{grade}.{animation_enum}.frame_000.png`.
- `animation_enum` exactly matches `CharacterAnimationClipType`.
- `characterName` and `grade` match the character JSON ID.

---

## Step 4. Skill JSON Generation

### Purpose

Create the skill JSON files used as input for EquipmentSkillSO generation.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillBalanceGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentBaseProfileSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillCastSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillHitSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillMoveSO.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/BaseVisualSO.md
```

### Main Work

1. Read skill intent from the planning JSON.
2. Convert each planned skill into one EquipmentSkillSO JSON input.
3. Use required child object IDs derived from `equipmentId`.
4. Include optional profiles only when the skill actually uses them.
5. Do not generate upgrade tables for normal NPC skills unless explicitly required.

### ID Rule

For all Player, Npc, and Boss character-owned skills, use:

```text
skill.character.{character_name}.{grade}.{slot}.{skill_name}
```

Example:

```text
skill.character.mist_lingering_child.1.basic_attack.cold_scratch
```

Do not use:

```text
skill.npc.{character_name}.{grade}.{slot}.{skill_name}
```

### Range Rule

`SkillCastSO.range` and planning `castRange` must be at least:

```text
0.4
```

### Output

Save skill JSON files under the skill JSON resource path used by the generator.

Recommended path:

```text
Assets/Resources/skill/character/generated
```

File name should match the skill ID:

```text
{equipmentId}.json
```

Example:

```text
Assets/Resources/skill/character/generated/skill.character.mist_lingering_child.1.basic_attack.cold_scratch.json
```

### Validation

- `equipmentId` starts with `skill.character`.
- Child IDs are derived from `equipmentId`.
- Required `baseProfile` and `cast` data exist.
- `cast.range` is `>= 0.4`.
- Optional profiles are omitted when unused.
- JSON is valid before committing.

---

## Step 5. Character JSON Generation

### Purpose

Create the character JSON file used as input for CharacterSO generation.

### Reference Files

```text
Assets/character_concepts/game_prompt_guide/character/CharacterSO.md
Assets/character_concepts/game_prompt_guide/character/CharacterStatGuide.md
Assets/character_concepts/game_prompt_guide/character/StatEnum.md
```

### Main Work

1. Convert the selected planning character into one CharacterSO input JSON.
2. Use the `character` domain for `characterId`.
3. Set `characterType` to `Player`, `Npc`, or `Boss`.
4. Set `job` to a valid `CharacterJob` enum value.
5. Convert planning stats into `baseStats`.
6. Do not include animation clips, skills, or localization data directly.

### ID Rule

```text
character.{character_name}.{grade}
```

Example:

```text
character.mist_lingering_child.1
```

### Output

Save the character JSON file to:

```text
Assets/Resources/character/json
```

File name:

```text
{characterId}.json
```

Example:

```text
Assets/Resources/character/json/character.mist_lingering_child.1.json
```

### Validation

- `characterId` starts with `character`.
- `characterType` is one of `Player`, `Npc`, or `Boss`.
- `job` exactly matches a valid `CharacterJob` enum value.
- Every `baseStats[].statType` exists in `StatEnum`.
- Animation data is not written into the character JSON.
- Skill references are not written into the character JSON.
- Skill JSON IDs match the CharacterSO skill search pattern.

---

## Final Validation Checklist

- Planning JSON exists in `Assets/Doc/Character`.
- PixelLab image export exists under `<PixelLabExportRoot>`.
- Animation PNGs exist in `Assets/Resources/character/animation_png`.
- Skill JSON files exist and use `skill.character`.
- Character JSON exists and uses `character`.
- `Player`, `Npc`, and `Boss` are used only as `characterType` values.
- No direct SO asset was created when the guide required JSON input.
- All generated JSON files are valid JSON.
- Git status is reviewed before commit.
