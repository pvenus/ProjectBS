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

Current pipeline:

```text
Act / Chapter story input
  -> canonical character_planning_v2
  -> generated_media_planning_handoff_v1 when planning is ready
  -> Generated Media router
  -> PixelLab character prompt authoring
  -> PixelLab character generation
  -> preservation / packaging
  -> separate evaluation
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
- `CharacterPlanningDataGuide.md` is the single per-character planning schema
  authority. Legacy planning is read-only until an explicit reviewed migration.
- Generated Media stages never infer missing character appearance or action
  design from combat/story shorthand.

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
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
AgentDocs/planning-guides/character/CharacterStatGuide.md
AgentDocs/planning-guides/skill/design/SkillDegineGuide.md
AgentDocs/planning-guides/skill/design/SkillBalanceGuide.md
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

## Step 2. Character Main-image Planning Handoff

### Purpose

Create an immutable Generated Media planning handoff only when canonical
character planning is approved and visually complete.

### Reference Files

```text
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
```

### Main Work

1. Verify `schemaVersion=character_planning_v2`, `planningStatus=approved`,
   `generatedMediaPlanning.characterMainImage.readiness=ready`, and an empty
   `missingDesignInputs`.
2. Hash the completed planning source after writing it. Do not write a
   self-referential hash into the planning file.
3. Map approved identity, structured appearance, observable requirements,
   prohibitions, and identity locks into a separate
   `generated_media_planning_handoff_v1`.
4. Expand `generated_media_exact_8_way_v1` into the exact technical direction
   order without adding character meaning or appearance.
5. If any planning fact or provenance is missing, return
   `character_planning_not_media_ready`; do not create the handoff.

### Output

One separate immutable character-main-image planning handoff, or a typed
blocker with `missingDesignInputs`.

### Validation

- Every handoff visual fact maps to approved character planning evidence.
- Required/prohibited statements are independently observable.
- The exact eight-way contract adds only technical view structure.
- No provider prompt, provider result, evaluation, or project asset is created.

---

## Step 3. Generated Media Router, Authoring, and Generation

### Purpose

Run the current Generated Media tasks as separate owners after a valid handoff
exists. This orchestration guide does not execute or merge their work.

### Reference Files

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/task-prompts/content/generated-media/GeneratedMediaRequestRoutingPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/PixelLabCharacterGenerationPrompt.md
```

### Main Work

1. Route the immutable planning handoff through the closed registry.
2. Author a provider prompt from the routing record and exact planning handoff.
3. Run provider generation in the separate generation task.
4. Do not infer missing design, repair planning, or create a fixed
   Attack/Idle/Move list.
5. Character animation is a separate handoff and includes only externally
   approved `animationRequests`.

### Output

Immutable routing, prompt, and generation records owned by their current
Generated Media guides.

### Validation

- Router selects exactly one registered row.
- Prompt authoring preserves visual evidence and adds no planning facts.
- Generation consumes the immutable prompt without rewriting it.
- Evaluation is not performed by routing, authoring, or generation.

---

## Step 4. Preservation, Packaging, and Evaluation Handoff

### Purpose

Preserve provider originals and extract ordered rotations/frames in the
separate packaging task, then hand a normalized package to evaluation.

### Reference Files

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/task-prompts/content/generated-media/GeneratedMediaPreservationPackagingPrompt.md
```

### Main Work

1. Preserve the original provider media and hashes.
2. Extract main-image rotations into `ordered_rotation_set` or requested
   animation frames into `ordered_frame_set` according to the registered
   adapter.
3. Package planning, prompt, generation, originals, extracted members,
   manifests, and hashes using the common evaluation package contract.
4. Send the package to a separate evaluation task.
5. Project promotion and Unity import remain later, independently authorized
   tasks.

### Output

A normalized Generated Media evaluation package and evaluation request handoff.

### Validation

- Main-image packages have eight unique ordered rotations.
- Animation packages contain only requested animation IDs and ordered frames.
- Staging source and informational project target are distinct.
- No evaluation verdict or project promotion is claimed by packaging.

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

- New planning JSON validates as `character_planning_v2`; legacy planning was
  not silently overwritten.
- Missing appearance/provenance produces `missingDesignInputs` and no Generated
  Media handoff.
- An approved main-image request has a separate immutable planning handoff with
  verified source hash/snapshot and exact eight-way technical contract.
- Router, prompt authoring, generation, preservation/packaging, and evaluation
  remain separate owners.
- Character animation contains only externally approved animation requests; no
  fixed Walk/Attack/Idle set is invented.
- Skill JSON files exist and use `skill.character`.
- Character JSON exists and uses `character`.
- `Player`, `Npc`, and `Boss` are used only as `characterType` values.
- No direct SO asset was created when the guide required JSON input.
- All generated JSON files are valid JSON.
- Project promotion and Git remain separately authorized work.
