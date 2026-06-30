# Character Design Create Guide

## Purpose

Generate character planning JSON before creating images, stats, skills, and runtime data.

The generated planning JSON will be used as the source for all later generation steps.

---

## Important

### Output

Save all generated JSON files to:

```text
Assets/Doc/Character
```

When creating an initial group of approximately 10 characters, the planning data may be split into:

- One common data JSON for shared group data.
- One character JSON per character.

Do not force all 10 characters into a single JSON if split files are easier to review or reuse.

Recommended file names:

```text
{groupId}.common.json
{characterId}.json
```

Example:

```text
sangui_spirit.common.json
mist_lingering_child.json
red_doll_carrier.json
```

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

### 1. Review World Setting

Reference:

```text
Assets/Doc
```

- Review the world setting.
- Review the story.
- Identify the character's purpose.
- Determine where the character appears.

---

### 2. Create Race

- Search existing races.
- Reuse an existing race whenever possible.
- Create a new race only when necessary.
- Keep the race consistent with the world setting.

---

### 3. Create Race Group

Create approximately **10 NPC concepts** belonging to the same race.

For the initial 10-character group, separate shared race/group information from character-specific information when useful.

Common data should describe the group once. Character files should reference the common data instead of duplicating the same shared settings.

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

### 4. Decide Encounter Score

Assign a planning score for every character.

The planning score determines:

- Encounter timing
- Difficulty
- Stat generation
- Skill generation

Higher scores should generally appear later in progression.

---

### 5. Create Planning Score

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

### 6. Generate Stats

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

### 7. Generate Skills

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

For an initial 10-character set, prefer this split when the group has shared race, faction, story context, or guide references.

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
Assets/Doc/Character/sangui_spirit.common.json
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
  "commonDataRef": "Assets/Doc/Character/sangui_spirit.common.json"
}
```

Example file:

```text
Assets/Doc/Character/mist_lingering_child.json
```

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
