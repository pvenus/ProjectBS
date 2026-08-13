# Character Design Create Guide


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## Purpose

Generate character planning JSON before creating images, stats, skills, and runtime data.

The generated planning JSON will be used as the source for all later generation steps.

### Project Character Visual Direction

This planning workflow owns character facts, not reusable rendering rules. It
must not invent or infer age, gender presentation, ethnicity, face, hair,
costume, equipment, weapon, handedness, palette, material, pose, attractiveness,
facial hair, fatigue, aging, gravitas, sexualization, or modernized appearance.

The sole normative expression-profile authority is:

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile
expressionProfileKey=projectbs_character_restrained_ink_line@1.0.0
```

This guide does not copy, shorten, or reinterpret that profile's positive or
negative locks. Downstream authoring resolves the exact registered key and hash.

If approved planning conflicts with the active ProjectBS style profile, return
`character_style_profile_conflict`. Require an explicit planning/style-profile
revision; do not silently restyle either authority.

The single per-character schema authority is:

```text
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
```

This guide supplies workflow and design-source rules only. It must not define a
second loose character JSON shape.

For Act-level generation from Act and Chapter story input, start with:

```text
AgentDocs/planning-guides/character/ActCharacterPlanningStartGuide.md
```

---

## Important

### Output

Save all generated JSON files to:

```text
AgentDocs/planning-data/character/act-plans
```

Create one folder per planning group.

Recommended folder:

```text
AgentDocs/planning-data/character/act-plans/player
AgentDocs/planning-data/character/act-plans/{groupId}
```

When creating an Act or character group, the planning data may be split into:

- One common data JSON for shared group data.
- One character JSON per character.

Do not force all characters into a single JSON if split files are easier to review or reuse.

There is no fixed character count.

Create, reuse, or skip characters according to story needs and battle-role coverage.

All common and character JSON files for the same planning group should be stored in the same group folder.

Recommended file names:

```text
AgentDocs/planning-data/character/act-plans/player/{groupId}.player_common.json
AgentDocs/planning-data/character/act-plans/player/{characterId}.json
AgentDocs/planning-data/character/act-plans/{groupId}/{groupId}.common.json
AgentDocs/planning-data/character/act-plans/{groupId}/npc/{characterId}.json
```

Example:

```text
AgentDocs/planning-data/character/act-plans/player/sangui_spirit.player_common.json
AgentDocs/planning-data/character/act-plans/player/character.seojin.1.json
AgentDocs/planning-data/character/act-plans/sangui_spirit/sangui_spirit.common.json
AgentDocs/planning-data/character/act-plans/sangui_spirit/npc/character.mist_lingering_child.1.json
AgentDocs/planning-data/character/act-plans/sangui_spirit/npc/character.red_doll_carrier.1.json
```

Recommended index files:

```text
AgentDocs/planning-data/character/act-plans/{groupId}/monster_context.{groupId}.json
AgentDocs/planning-data/character/act-plans/{groupId}/monster_composition.chapter_XX_YY.json
```

For creating or updating only those index files, use:

```text
AgentDocs/planning-guides/character/NpcPoolJsonCreateGuide.md
AgentDocs/task-prompts/character/NpcPoolJsonCreatePrompt.md
```

The index files should keep only refs, role slots, chapter use, and composition hints.

Do not duplicate full identity, appearance, stats, or skills in the index files.

### Required And Allowed References

Read these exact authorities first:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
AgentDocs/planning-guides/story/StoryStructureGuide.md
AgentDocs/planning-data/story/Characters.md
AgentDocs/planning-guides/character/CharacterStatGuide.md
AgentDocs/planning-guides/skill/design/SkillDegineGuide.md
AgentDocs/planning-guides/skill/design/SkillBalanceGuide.md
```

Use only the exact Act, Chapter, common planning, and story files supplied by
the task or resolved through StoryStructureGuide.md. Record every file actually
used in `sourceStoryRefs` or `sourcePlanningRefs` and map each character fact to
an exact source section/pointer.

Do not search broad folders such as `Assets/Doc`. Do not inspect runtime,
resource, image, or implementation folders to invent planning facts. If an
allowed source does not establish a required design fact, write a typed
`missingDesignInputs` blocker; do not leave an ambiguous note or infer a value.

### Player / NPC

Determine the character type before generating any data.

Player and NPC use different rules for:

- Stat generation
- Skill composition
- Skill upgrades
- Balance

Always follow the corresponding guide documents.

### Player / NPC Folder Split

Planning files must be separated by operational use:

```text
AgentDocs/planning-data/character/act-plans/player
AgentDocs/planning-data/character/act-plans/{groupId}/npc
```

Use `AgentDocs/planning-data/character/act-plans/player` for playable or party characters.

Use `AgentDocs/planning-data/character/act-plans/{groupId}/npc` for enemy combat pool entries, including:

- `characterType: "Npc"`
- `characterType: "Boss"`

Do not create an `npc` runtime domain.

The folder is only a planning organization boundary.

The runtime domain remains:

```text
character
```

### Domain Naming

Use `character` as the generation domain for all character-related runtime data.

`Player`, `Npc`, and `Boss` are character types, not ID domains.

Do not use `npc` as a runtime resource domain when generating IDs for CharacterSO, character skills, animation links, or localization keys.

Examples:

```text
character.mist_lingering_child.1
skill.character.mist_lingering_child.1.basic_attack.cold_scratch
```

---

## Workflow

### 1. Resolve Act And Chapter Input

Character planning should start from Act and Chapter context.

Recommended input shape:

```json
{
  "actId": "act.01",
  "chapterIds": ["chapter.01.01", "chapter.01.02"],
  "chapterFiles": [
    "AgentDocs/planning-data/story/Act01/Chapter01/Chapter_01.md",
    "AgentDocs/planning-data/story/Act01/Chapter02/Chapter_02.md"
  ]
}
```

If the user provides only a Chapter file, resolve the Act only through
`StoryStructureGuide.md` and exact registered story references. If more than one
Act is possible, stop with an ambiguous-source blocker rather than infer one.

Use Act context for shared data:

- Race
- Faction
- World use
- Story use
- Reuse policy
- Source guide list

Use Chapter context for concrete generation:

- Monster candidates
- Required combat roles
- Spawn or spatial hints
- Boss or elite timing
- Forbidden monster types
- Player characters present

---

### 2. Review World Setting

Use the exact approved story and planning references recorded in provenance.
Review the character's established world use, story purpose, and appearances.
Do not turn broad setting tone into unsupported per-character appearance.

---

### 3. Create Race

- Search existing races.
- Reuse an existing race whenever possible.
- Create a new race only when necessary.
- Keep the race consistent with the world setting.

---

### 4. Create Race Group

Create NPC concepts belonging to the relevant race, faction, or story threat.

There is no fixed NPC count.

The group should grow or shrink according to Act, Chapter, and battle-role needs.

Separate shared race/group information from character-specific information when useful.

Common data should describe the group once. Character files should reference the common data instead of duplicating the same shared settings.

Create a dedicated folder for the group before writing planning JSON files.

The group should contain various combat roles.

Example:

- Scout
- Warrior
- Defender
- Archer
- Assassin
- Shaman
- Elite
- Captain
- Guardian
- Boss

---

### 5. Decide Encounter Score

Assign a planning score for every character.

The planning score determines:

- Encounter timing
- Difficulty
- Stat generation
- Skill generation

Higher scores should generally appear later in progression.

---

### 6. Create Planning Score

Create planning scores before generating stats.

Recommended categories:

```text
Overall
Health
Attack
Defense
Speed
Range
Support
```

Planning scores describe combat identity only.

They are not runtime values.

---

### 7. Generate Stats

Reference:

```text
AgentDocs/planning-guides/character/CharacterStatGuide.md
```

Rules:

- Determine Player or NPC first.
- Generate stats from the planning score.
- Follow Player or NPC stat rules.
- Keep the stat distribution consistent with the combat role.

---

### 8. Generate Skills

Reference:

```text
AgentDocs/planning-guides/skill/design/SkillDegineGuide.md

AgentDocs/planning-guides/skill/design/SkillBalanceGuide.md
```

Rules:

- Determine Player or NPC first.
- Generate skills from the planning score.
- Follow the Skill Design Guide.
- Follow the Skill Balance Guide.

Player

- Grade-based skill composition
- Generate skill upgrades

NPC

- Tier-based skill composition
- Do not generate skill upgrades

---

## Canonical JSON Contract

New per-character planning must conform exactly to:

```text
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
schemaVersion=character_planning_v2
```

The canonical planning contains:

```text
schemaVersion
documentId
documentType
planningStatus
commonDataRef
identity
provenance
appearance
combat
planningScore
stats
skills
generatedMediaPlanning
missingDesignInputs
notes
```

### Split JSON Structure

Prefer this split when the group has shared race, faction, story context, or guide references.

#### Common Data JSON

The common data JSON contains shared information used by every character in the group.

The common-data contract remains a separate group-level planning artifact. The
following established fields explain split-file ownership and are not a second
per-character schema:

```text
documentId
documentType
sourceGuides
group
race
faction
worldUse
storyUse
reuse
sharedVisualStyle
sharedSkillRules
notes
```

Recommended `documentType`:

```text
characterPlanningCommon
```

Example file:

```text
AgentDocs/planning-data/character/act-plans/sangui_spirit/sangui_spirit.common.json
```

#### Character Data JSON

Each character data JSON contains only one character's planning data.

Canonical per-character fields are not optional recommendations. Use the exact
schema guide, including:

```text
documentId
documentType
schemaVersion
planningStatus
commonDataRef
identity
provenance
appearance
combat
planningScore
stats
skills
generatedMediaPlanning
missingDesignInputs
notes
```

Required `documentType`:

```text
characterPlanning
```

`commonDataRef` must point to the common data JSON using a project-relative path.

Example:

```json
{
  "commonDataRef": "AgentDocs/planning-data/character/act-plans/player/sangui_spirit.player_common.json"
}
```

Example file:

```text
AgentDocs/planning-data/character/act-plans/sangui_spirit/npc/character.mist_lingering_child.1.json
```

### Group Folder Rule

Every planning group must be managed under `AgentDocs/planning-data/character/act-plans`.

Use this structure:

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

The group folder should contain only planning JSON files for that group.

Do not mix multiple character planning groups in the same folder.

Do not place planning group JSON files directly under `AgentDocs/planning-data/character/act-plans` unless the task explicitly asks for a single legacy file.

Do not place guide documents, process README files, or authoring manuals inside generated planning group folders.

Generated planning group folders should contain data artifacts only:

- Common JSON
- Monster context JSON
- Monster composition JSON
- Character planning JSON files
- Unity `.meta` files

Authoring guides must stay under:

```text
AgentDocs/planning-guides
```

### Context And Composition Index Rule

Use `monster_context.{groupId}.json` to expose the available enemy monster pool to later agents.

It may contain:

- `commonDataRef`
- `playerPlanningRefs`
- `monsterPoolRefs`
- `bossRefs`
- `roleSlots`
- `storyUseTags`
- `monsterCompositionRef`

It must not contain:

- Full appearance descriptions
- Full stat intent
- Full skill intent
- Runtime SO data

Use `monster_composition.chapter_XX_YY.json` under `AgentDocs/planning-data/character/act-plans/{groupId}` when Act or Chapter battle needs must be preserved.

It may contain:

- `actId`
- `chapterCompositions`
- `coreBattleIntent`
- `primaryMonsters`
- `secondaryMonsters`
- `lockedOutMonsters`
- `recommendedSpawnTags`
- `forbiddenSpawnTags`

This lets the battle pipeline select from a prepared monster pool instead of inventing monsters again.

### Duplication Rule

Do not duplicate shared race, faction, world, source guide, or reuse data in every character JSON.

Keep shared information in the common data JSON and keep character-specific identity, stat, skill, combat, and appearance data in each character JSON.

If a character intentionally overrides common data, write only the override in the character JSON and leave a short note explaining why.

Existing files that do not match `character_planning_v2` are legacy read-only
inputs. Classify and migrate only through the versioned strategy in
CharacterPlanningDataGuide.md. Do not weaken the canonical schema to resemble a
legacy example.

---

## Scope Rule

Do not inspect:

- Runtime folders
- Image folders
- Resource folders
- Character implementations
- Skill implementations
- Stat implementations

Only use the exact authorities and task-supplied sources declared above.
Generated Media prompt authoring may begin only from a separate approved
handoff; it cannot fill a missing character design.
