# Character Design Create Guide

## Purpose

Generate character planning JSON before creating images, stats, skills, and runtime data.

The generated planning JSON will be used as the source for all later generation steps.

For Act-level generation from Act and Chapter story input, start with:

```text
Assets/character_concepts/game_prompt_guide/character/ActCharacterPlanningStartGuide.md
```

---

## Important

### Output

Save all generated JSON files to:

```text
Assets/Doc/Character
```

Create one folder per planning group.

Recommended folder:

```text
Assets/Doc/Character/player
Assets/Doc/Character/{groupId}
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
Assets/Doc/Character/player/{groupId}.player_common.json
Assets/Doc/Character/player/{characterId}.json
Assets/Doc/Character/{groupId}/{groupId}.common.json
Assets/Doc/Character/{groupId}/npc/{characterId}.json
```

Example:

```text
Assets/Doc/Character/player/sangui_spirit.player_common.json
Assets/Doc/Character/player/character.seojin.1.json
Assets/Doc/Character/sangui_spirit/sangui_spirit.common.json
Assets/Doc/Character/sangui_spirit/npc/character.mist_lingering_child.1.json
Assets/Doc/Character/sangui_spirit/npc/character.red_doll_carrier.1.json
```

Recommended index files:

```text
Assets/Doc/Character/{groupId}/monster_context.{groupId}.json
Assets/Doc/Character/{groupId}/monster_composition.chapter_XX_YY.json
```

For creating or updating only those index files, use:

```text
Assets/character_concepts/game_prompt_guide/character/NpcPoolJsonCreateGuide.md
Assets/character_concepts/game_prompt_guide/character/NpcPoolJsonCreatePrompt.md
```

The index files should keep only refs, role slots, chapter use, and composition hints.

Do not duplicate full identity, appearance, stats, or skills in the index files.

### Allowed References

Only the following documents may be referenced.

```text
Assets/Doc
Assets/character_concepts/game_prompt_guide/character/CharacterStatGuide.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillBalanceGuide.md
```

Do **NOT** inspect or search any other folders or files.

If required information is missing, leave a short note inside the JSON instead of searching outside the allowed scope.

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
Assets/Doc/Character/player
Assets/Doc/Character/{groupId}/npc
```

Use `Assets/Doc/Character/player` for playable or party characters.

Use `Assets/Doc/Character/{groupId}/npc` for enemy combat pool entries, including:

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
    "Assets/Doc/Story/Chapter_01.md",
    "Assets/Doc/Story/Chapter_02.md"
  ]
}
```

If the user provides only a Chapter file, infer the Act from `StoryStructureGuide.md` and the available story docs.

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

Reference:

```text
Assets/Doc
```

- Review the world setting.
- Review the story.
- Identify the character's purpose.
- Determine where the character appears.

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
Assets/character_concepts/game_prompt_guide/character/CharacterStatGuide.md
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
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md

Assets/character_concepts/game_prompt_guide/skill/design/SkillBalanceGuide.md
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

## JSON Direction

The planning JSON should generally contain:

```text
identity
appearance
combat
planningScore
storyUse
reuse
stats
skills
```

### Split JSON Structure

Prefer this split when the group has shared race, faction, story context, or guide references.

#### Common Data JSON

The common data JSON contains shared information used by every character in the group.

Recommended fields:

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
Assets/Doc/Character/sangui_spirit/sangui_spirit.common.json
```

#### Character Data JSON

Each character data JSON contains only one character's planning data.

Recommended fields:

```text
documentId
documentType
commonDataRef
identity
appearance
combat
planningScore
stats
skills
notes
```

Recommended `documentType`:

```text
characterPlanning
```

`commonDataRef` must point to the common data JSON using a project-relative path.

Example:

```json
{
  "commonDataRef": "Assets/Doc/Character/player/sangui_spirit.player_common.json"
}
```

Example file:

```text
Assets/Doc/Character/sangui_spirit/npc/character.mist_lingering_child.1.json
```

### Group Folder Rule

Every planning group must be managed under `Assets/Doc/Character`.

Use this structure:

```text
Assets/Doc/Character/player/
  {groupId}.player_common.json
  character.{player_name}.{grade}.json
Assets/Doc/Character/{groupId}/
  {groupId}.common.json
  monster_context.{groupId}.json
  monster_composition.chapter_XX_YY.json
  npc/
    character.{npc_name}.{grade}.json
    character.{boss_name}.{grade}.json
```

The group folder should contain only planning JSON files for that group.

Do not mix multiple character planning groups in the same folder.

Do not place planning group JSON files directly under `Assets/Doc/Character` unless the task explicitly asks for a single legacy file.

Do not place guide documents, process README files, or authoring manuals inside generated planning group folders.

Generated planning group folders should contain data artifacts only:

- Common JSON
- Monster context JSON
- Monster composition JSON
- Character planning JSON files
- Unity `.meta` files

Authoring guides must stay under:

```text
Assets/character_concepts/game_prompt_guide
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

Use `monster_composition.chapter_XX_YY.json` under `Assets/Doc/Character/{groupId}` when Act or Chapter battle needs must be preserved.

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

The exact schema does not need to match existing files perfectly.

The generated JSON should be:

- Easy to read
- Easy to review
- Easy to convert into runtime data
- Consistent with existing planning JSON documents

---

## Scope Rule

Do not inspect:

- Runtime folders
- Image folders
- Resource folders
- Character implementations
- Skill implementations
- Stat implementations

Only use the documents listed in **Allowed References**.
