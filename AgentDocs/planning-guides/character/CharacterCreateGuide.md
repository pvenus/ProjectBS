# Character Create Guide


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

## Purpose

This document defines the full character creation pipeline.

Use this guide as the orchestration document when creating a character from planning data through runtime JSON inputs.

Pipeline:

```text
Act / Chapter story input
  -> Character planning
  -> Image generation
  -> Animation generation
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
- Planning files should separate playable characters from NPC and Boss combat pool files.
- Boss uses `characterType: "Boss"` but belongs to the enemy combat pool folder unless the task explicitly needs a separate boss folder.

---

## Story Input

Character planning should be generated from Act and Chapter context, not from an isolated monster prompt.

Input may be provided as:

```text
actId: act.01
chapterIds: [chapter.01.01, chapter.01.02]
chapterFiles:
  - AgentDocs/planning-data/story/Act01/Chapter01/Chapter_01.md
  - AgentDocs/planning-data/story/Act01/Chapter02/Chapter_02.md
```

If only a Chapter file is provided, resolve the Act using:

```text
AgentDocs/planning-guides/story/StoryStructureGuide.md
AgentDocs/planning-data/story/00_Background.md
AgentDocs/planning-data/story/Act{actNumber}/01_Overall_Story.md
AgentDocs/planning-data/story/Act{actNumber}/Act_{actNumber}_Background.md
AgentDocs/planning-data/story/Characters.md
AgentDocs/planning-data/story/Act{actNumber}/Chapter{chapterNumber}/Chapter_{chapterNumber}.md
```

Use Act context for shared world, race, faction, tone, and reuse data.

Use `AgentDocs/planning-data/story/Characters.md` as the global story character reference.

Do not treat `Characters.md` as Chapter-local output.

Use Chapter context for concrete character needs:

- Which monsters or NPCs must appear
- Which combat roles are needed
- Which monsters should be delayed or forbidden
- Which boss, elite, support, ranged, or objective-pressure roles are required
- Which player characters are present or referenced

---

## Step 1. Character Planning

### Purpose

Create the planning JSON that defines the character concept, role, stat intent, skill intent, and generation source data.

### Reference Files

```text
AgentDocs/planning-guides/character/ActCharacterPlanningStartGuide.md
AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
AgentDocs/planning-guides/character/CharacterStatGuide.md
AgentDocs/planning-guides/skill/design/SkillDegineGuide.md
AgentDocs/planning-guides/skill/design/SkillBalanceGuide.md
Assets/Doc
```

### Main Work

1. Determine the character type: `Player`, `Npc`, or `Boss`.
2. Define the character identity, role, grade, type, tag, and story use.
3. Assign planning score and stat intent.
4. Design the expected skill slots and behavior.
5. Balance skill intent using target score, cooldown, cast range, hit range, and utility rules.
6. Create as many planning characters as the Act, Chapter, and battle-role needs require.
7. Split shared group data and per-character data when useful.

### Output

Save planning JSON under:

```text
AgentDocs/planning-data/character/act-plans
```

Create one folder per planning group:

```text
AgentDocs/planning-data/character/act-plans/{groupId}
```

Single-file example:

```text
AgentDocs/planning-data/character/act-plans/sangui_spirit_npc_group.json
```

Split-file example:

```text
AgentDocs/planning-data/character/act-plans/player/sangui_spirit.player_common.json
AgentDocs/planning-data/character/act-plans/player/character.seojin.1.json
AgentDocs/planning-data/character/act-plans/sangui_spirit/sangui_spirit.common.json
AgentDocs/planning-data/character/act-plans/sangui_spirit/npc/character.mist_lingering_child.1.json
AgentDocs/planning-data/character/act-plans/sangui_spirit/npc/character.red_doll_carrier.1.json
```

Use player common data JSON for player-side shared race, faction, world, story, reuse, and source guide data.

Use monster common data JSON for enemy pool shared race, faction, world, story, reuse, and source guide data.

Use each character JSON for one character's identity, appearance, combat, planning score, stats, and skills.

Each character JSON should reference the common data JSON with `commonDataRef`.

Example:

```json
{
  "commonDataRef": "AgentDocs/planning-data/character/act-plans/player/sangui_spirit.player_common.json"
}
```

Recommended group folder shape:

```text
AgentDocs/planning-data/character/act-plans/player/
  {groupId}.player_common.json
  character.{player_name}.{grade}.json
AgentDocs/planning-data/character/act-plans/{groupId}/
  {groupId}.common.json
  monster_context.{groupId}.json
  monster_composition.chapter_XX_YY.json
  npc/
    character.{npc_name}.{grade}.json
    character.{boss_name}.{grade}.json
```

`monster_context` should contain refs and lightweight role information only.

`monster_composition` should map Act and Chapter battle needs to monster planning refs.

### Validation

- Planning data clearly identifies the target character.
- Character type is selected before stat and skill design.
- NPC rules affect composition and upgrades only.
- NPC rules do not change runtime resource domains to `npc`.
- Skill intent includes enough data for the skill JSON step.
- Shared group data is not duplicated across character JSON files.
- Character JSON files reference the common data JSON with a project-relative `commonDataRef`.
- Player planning files are stored inside `AgentDocs/planning-data/character/act-plans/player`.
- Npc and Boss combat-pool planning files are stored inside `AgentDocs/planning-data/character/act-plans/{groupId}/npc`.
- Chapter-specific monster composition is documented in a JSON or Markdown-visible section before final battle generation.
- Different planning groups are not mixed in the same folder.

---

## Step 2. Image Generation

### Purpose

Generate the character concept image in PixelLab using the planning JSON as the source.

### Reference Files

```text
AgentDocs/planning-guides/character/CharacterGenerateImage.md
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

## Step 3. Animation Generation

### Purpose

Generate Walk, Attack, and Idle animations for the selected PixelLab character before downloading animation frames.

### Reference Files

```text
AgentDocs/planning-guides/character/CharacterGenerateAnimation.md
```

### Main Work

1. Open PixelLab:

```text
https://www.pixellab.ai/create-character
```

2. Open the generated character.
3. Click **+ Add Animation**.
4. In Character Preview, select **South-East** direction.
5. Generate the required animations:
   - Walk animation from the **MOVEMENT / Walking / Walk** option.
   - Attack animation using **CUSTOM / Custom Animation V3**.
   - Idle animation using **CUSTOM / Custom Animation V3**.
6. Use `8 Frames` for Attack animation.
7. Use `6 Frames` for Idle animation.
8. Enable **Keep first frame (idle pose)** for custom animations.
9. Rename generated animations to:
   - `Walk`
   - `Attack`
   - `Idle`

### Output

The selected PixelLab character has generated animations ready for export.

### Validation

- Animation generation is performed in PixelLab only.
- Walk animation score is at least `90 / 100`.
- Attack animation score is at least `90 / 100`.
- Frame-to-frame movement score is at least `80 / 100`.
- Weapon review score is at least `80 / 100`.
- Animation names are exactly `Walk`, `Attack`, and `Idle`.
- The character appearance, equipment, and weapon remain consistent with the generated image.

---

## Step 4. Animation Extraction

### Purpose

Download the character animation export, rename animation PNGs, and copy them into the Unity resource path used by the CharacterSO generator.

### Reference Files

```text
AgentDocs/planning-guides/character/CharacterAnimationDownloadGuide.md
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
Assets/ImagesGenerated/Character/animation
```

The generator later creates animation clips under:

```text
Assets/Resources/character/animation_clip
```

### Validation

- PNG files exist in `Assets/ImagesGenerated/Character/animation`.
- File names match `character.{characterName}.{grade}.{animation_enum}.frame_000.png`.
- `animation_enum` exactly matches `CharacterAnimationClipType`.
- `characterName` and `grade` match the character JSON ID.

---

## Step 5. Skill JSON Generation

### Purpose

Create the skill JSON files used as input for EquipmentSkillSO generation.

### Reference Files

```text
AgentDocs/planning-guides/skill/design/SkillDegineGuide.md
AgentDocs/planning-guides/skill/design/SkillBalanceGuide.md
AgentDocs/planning-guides/skill/data-structures/SkillJsonGuide.md
AgentDocs/planning-guides/skill/data-structures/EquipmentSkillSO.md
AgentDocs/planning-guides/skill/data-structures/EquipmentBaseProfileSO.md
AgentDocs/planning-guides/skill/data-structures/SkillCastSO.md
AgentDocs/planning-guides/skill/data-structures/SkillHitSO.md
AgentDocs/planning-guides/skill/data-structures/SkillMoveSO.md
AgentDocs/planning-guides/skill/data-structures/BaseVisualSO.md
```

### Main Work

1. Read skill intent from the planning JSON.
2. Convert each planned skill into one EquipmentSkillSO JSON input.
3. Use required child object IDs derived from `equipmentId`.
4. Include optional profiles only when the skill actually uses them, but always include `baseVisual` for every skill JSON.
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

Canonical path:

```text
Assets/Resources/skill/json
```

File name should match the skill ID:

```text
{equipmentId}.json
```

Example:

```text
Assets/Resources/skill/json/skill.character.mist_lingering_child.1.basic_attack.cold_scratch.json
```

### Validation

- `equipmentId` starts with `skill.character`.
- Child IDs are derived from `equipmentId`.
- Required `baseProfile` and `cast` data exist.
- `cast.range` is `>= 0.4`.
- Optional profiles are omitted when unused, except `baseVisual` which is always written.
- JSON is valid before committing.

---

## Step 6. Character JSON Generation

### Purpose

Create the character JSON file used as input for CharacterSO generation.

### Reference Files

```text
AgentDocs/planning-guides/character/data-structures/CharacterSO.md
AgentDocs/planning-guides/character/CharacterStatGuide.md
AgentDocs/planning-guides/character/data-structures/StatEnum.md
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

- Planning JSON exists in `AgentDocs/planning-data/character/act-plans`.
- PixelLab image export exists under `<PixelLabExportRoot>`.
- PixelLab animations are generated and named `Walk`, `Attack`, and `Idle`.
- Animation PNGs exist in `Assets/ImagesGenerated/Character/animation`.
- Skill JSON files exist and use `skill.character`.
- Character JSON exists and uses `character`.
- `Player`, `Npc`, and `Boss` are used only as `characterType` values.
- No direct SO asset was created when the guide required JSON input.
- All generated JSON files are valid JSON.
- Git status is reviewed before commit.
