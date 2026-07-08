# Episode Planning Create Guide

## Purpose

Generate episode planning JSON from episode prose.

This planning JSON is not the final script, final battle context, final monster
planning, or runtime story data.

It should preserve only the design direction needed for later steps:

```text
Episode Markdown
  -> Episode planning JSON
  -> Formal script generation
  -> Monster / NPC planning
  -> BattleStoryContext
  -> Runtime story, CharacterSO, BattleSO, SpawnSO
```

The episode planning JSON should be synopsis-level. It should tell later agents
what to make, not fully make it here.

## Core Principle

Episode planning owns direction, not implementation.

It should answer:

- What is this episode about?
- What should the formal script preserve?
- What monster family or enemy direction is implied?
- What monster difficulty direction is implied?
- What battle shape and battle difficulty can be inferred?
- What reward direction is implied?
- What story reveal must remain hidden?

It should not decide:

- final script prose
- full dialogue
- exact NPC stats
- concrete CharacterSO data
- exact spawner type
- exact spawn timing
- exact spawn count
- final BattleSO JSON
- final reward economy tables

## Output

Save generated episode planning JSON files to:

```text
Assets/Doc/StoryPlanning
```

Create one folder per story planning group:

```text
Assets/Doc/StoryPlanning/{act_group_id}
```

Example:

```text
Assets/Doc/StoryPlanning/cheongun_sangui_act1
```

Recommended files:

```text
Assets/Doc/StoryPlanning/{act_group_id}/{act_group_id}.story_common.json
Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.chapter_XX.json
```

`episode_battle_plan.chapter_XX.json` is conditional. Create it only after a
reusable spawner has been selected. If no spawner matches, do not create this
file and report failure instead.

The group folder should contain only planning JSON files for that group and
Unity `.meta` files.

Do not place guide documents, process README files, or authoring manuals inside
generated planning folders.

## Allowed References

Use story and planning guide documents.

Recommended references:

```text
Assets/Doc/Story
Assets/Doc/Design/EpisodeFormat.md
Assets/Doc/Design/StoryFormat.json
Assets/character_concepts/game_prompt_guide/story/StoryBattlePlanningPipelineGuide.md
Assets/character_concepts/game_prompt_guide/story/StoryStructureGuide.md
Assets/character_concepts/game_prompt_guide/story/StoryPlanningContextGuide.md
Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md
Assets/character_concepts/game_prompt_guide/stage/EpisodeStageNodeCreateGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
```

Existing character planning refs may be mentioned as downstream references, but
this layer should not inspect runtime resource folders or generate CharacterSO
data.

## Split JSON Structure

Use the same review-friendly split direction as character planning.

### Common Data JSON

Create:

```text
Assets/Doc/StoryPlanning/{act_group_id}/{act_group_id}.story_common.json
```

Recommended fields:

```text
documentId
documentType
actId
group
sourceStoryRefs
sourceGuides
worldUse
storyUse
episodeRules
rewardPolicyRef
downstreamPolicies
notes
```

Recommended `documentType`:

```text
storyPlanningCommon
```

Common JSON should contain shared Act/group direction only.

Do not put episode-specific synopsis, monster direction, battle direction, or
reward direction in common JSON.

### Episode Data JSON

Create one JSON per episode:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
```

Recommended fields:

```text
documentId
documentType
commonDataRef
common
story
monster
battle
reward
handoff
constraints
notes
```

Recommended `documentType`:

```text
episodePlanning
```

### Context And Composition Index JSON

Use `story_context.{act_group_id}.json` to expose the available episode planning
files to later agents.

Use `episode_composition.chapter_XX.json` to preserve chapter order and
episode-level downstream needs.

Use `episode_battle_monster_pool.chapter_XX.json` to preserve the monster pool
direction needed for episode battle composition before monster creation happens.

Index files should keep only refs and summaries.

Do not copy full episode planning data into index files.

## Episode Data Categories

Episode data should be grouped by large planning categories.

Use these top-level categories:

```text
common
story
monster
battle
reward
handoff
```

Category meaning:

| Category | Owns |
|---|---|
| `common` | Episode identity, role, source refs, and shared local metadata. |
| `story` | Script synopsis, scene beats, choice directions, tone, must-show/must-hide story direction. |
| `monster` | Monster family, role direction, difficulty direction, visual direction, reuse/creation policy. |
| `battle` | Battle need, mood, shape, difficulty direction, pace direction, pressure, and avoid rules. |
| `reward` | Reward direction, current reward type, reward reason, reward guide ref. |
| `handoff` | Which later generation steps should continue and which refs they should use. |

Do not spread the same concern across several categories.

For example:

- Monster difficulty direction belongs to `monster`.
- Battle difficulty direction belongs to `battle`.
- Formal script synopsis belongs to `story`.
- Reward type and reward reason belong to `reward`.
- Downstream refs belong to `handoff`.

### common

`common` contains the episode's local identity and source refs.

```json
{
  "common": {
    "episodeId": "episode.act1.chapter01.01",
    "actId": "act.01",
    "chapterId": "chapter.01.01",
    "title": "청운촌의 습격",
    "episodeRole": "intro",
    "secondaryRoles": [
      "rescue",
      "battle_entry"
    ],
    "source": {
      "sourceEpisodeFile": "Assets/Doc/Story/Act01/Chapter01/01_episode1.md",
      "sourceChapterFile": "Assets/Doc/Story/Act01/Chapter01/Chapter_01.md"
    }
  }
}
```

### story

`story` is the main bridge to formal script generation.

It should remain synopsis-level. Do not write the final script here.

Recommended shape:

```json
{
  "story": {
    "scriptSynopsis": {
      "episodeSummary": "Seojin reaches Cheongun Village and enters the first rescue battle.",
      "sceneBeats": [
        "Seojin hears rumors of missing children and heads to Cheongun Village.",
        "The drought-stricken village appears broken and anxious.",
        "Black-cloth raiders are attacking villagers.",
        "Seojin chooses to protect the villagers and enters battle."
      ],
      "emotionalTone": [
        "urgent",
        "dry_village",
        "personal_regret"
      ],
      "mustShow": [
        "drought damage",
        "black cloth",
        "red doll charm"
      ],
      "mustHide": [
        "sangui_true_form",
        "final_abduction_truth"
      ],
      "scriptHandoffHint": "Formal script should be generated later using character persona and this synopsis."
    },
    "choiceDirections": [
      {
        "choiceId": "episode.act1.chapter01.01.choice.rescue_villagers.1",
        "choiceIntent": "Protect villagers immediately.",
        "outcomeDirection": "Battle begins as Seojin steps between villagers and raiders.",
        "opens": [
          "battle"
        ],
        "rewardDirection": [
          "gold_battle_reward"
        ]
      }
    ]
  }
}
```

Use this field instead of detailed node plans unless node-level runtime
conversion is explicitly requested.

Do not decide final battle IDs or runtime node IDs here unless explicitly asked.

### monster

This is not NPC generation.

`monster` should only say which monster family, role direction, and difficulty
direction later monster/NPC planning should consider.

Recommended shape:

```json
{
  "monster": {
    "monsterFamily": "black_cloth_raiders",
    "difficultyIntent": "early_easy",
    "recommendedTier": "normal",
    "roleDirections": [
      "basic_melee",
      "light_control_melee"
    ],
    "visualDirection": [
      "black cloth",
      "red doll charm",
      "improvised weapon"
    ],
    "reusePreference": "reuse_existing_if_possible",
    "creationPolicy": "Create a new variant only if the existing pool cannot cover the implied role.",
    "forbiddenTypes": [
      "elite",
      "boss",
      "spirit"
    ],
    "revealPolicy": "Enemies should read as humans using the Sangui rumor, not the Sangui itself."
  }
}
```

Do not include:

- concrete stats
- skill data
- final character IDs unless referencing an existing planning pool is required
- CharacterSO paths

When a battle needs a concrete monster pool direction, create or update:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
```

That file is still pre-monster-creation planning. It may define desired slots
such as `basic_melee`, `light_control_melee`, or `light_front_blocker`.

If matching monsters already exist, list them only under candidate refs such as
`existingCandidateRefs`.

Do not write the document as if those monsters must already exist.

When a battle needs concrete balance and spawner selection direction, first
search existing spawner presets.

Only create or update this file after a reusable spawner has been selected:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.chapter_XX.json
```

That file is still before `BattleStoryContext` and `BattleSO`, but it is more
concrete than the episode `battle` block.

It should decide:

- battle mode intent
- target battle length
- target party size
- monster pool slot ratio
- spawn count range
- spawn window and clear window
- reusable spawner selection result
- selected spawner type, difficulty, and sequence id
- spawnUnitKey to monster pool slot mapping

If no reusable spawner matches the battle direction, monster pool, count
balance, party assumption, and forbidden pressure rules, stop the battle plan
generation step and report failure to the user or calling agent.

Do not create `episode_battle_plan.chapter_XX.json` as a failure artifact.
Do not select a near match just to continue.
Do not create the new spawner in this step.

The failure response should say:

- no reusable spawner was selected
- battle plan JSON was not created
- BattleSO cannot be created yet
- spawner creation is required
- the missing spawner direction, including mode, party size, spawn count range,
  required spawn roles, and forbidden roles

### battle

This is not `BattleStoryContext`.

`battle` should preserve enough direction for a later `BattleStoryContext` to
infer mode, difficulty, rhythm, and spawner selection.

Recommended shape:

```json
{
  "battle": {
    "battleNeed": "required",
    "battleMood": "urgent_rescue",
    "battleShape": "front_pressure_with_light_flank",
    "difficultyIntent": "easy_intro",
    "partyAssumption": "solo_story_intro",
    "paceIntent": "short_readable_swarm",
    "monsterPressure": [
      "basic_melee",
      "light_control"
    ],
    "avoid": [
      "heavy_surround",
      "elite_pressure",
      "boss_reveal",
      "spirit_reveal"
    ]
  }
}
```

Do not include:

- exact `SpawnSequenceSO`
- final spawner type
- spawn timings
- spawn count
- final victory rule unless explicitly required
- BattleSO fields

Those belong to `BattleStoryContext` and battle generation.

### reward

`reward` should remain simple and refer to the reward guide.

Current allowed reward type is gold only.

Recommended shape:

```json
{
  "reward": {
    "rewardPolicyRef": "Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md",
    "rewardType": "gold",
    "rewardReason": "Reward the player for clearing the village rescue battle.",
    "rewardScaleHint": "normal_battle_clear"
  }
}
```

Do not create item, material, skill, or difficulty-scaled rewards until
`RewardPlanningGuide.md` defines them.

### handoff

`handoff` should tell later steps where to continue.

Recommended shape:

```json
{
  "handoff": {
    "formalScriptNeedsPersona": true,
    "monsterPlanningNeeded": true,
    "battleMonsterPoolRef": "Assets/Doc/StoryPlanning/cheongun_sangui_act1/episode_battle_monster_pool.chapter_01.json",
    "battlePlanStatus": "not_created_until_spawner_selected",
    "battleStoryContextNeeded": true,
    "rewardPlanningNeeded": true,
    "monsterContextRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_context.cheongun_sangui_act1.json",
    "monsterCompositionRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_composition.chapter_01_05.json",
    "rewardPolicyRef": "Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md"
  }
}
```

Use refs only. Do not copy full downstream data.

## Minimal Episode Shape

```json
{
  "documentId": "episode_planning.act1.chapter01.01",
  "documentType": "episodePlanning",
  "commonDataRef": "Assets/Doc/StoryPlanning/cheongun_sangui_act1/cheongun_sangui_act1.story_common.json",
  "common": {
    "episodeId": "episode.act1.chapter01.01",
    "actId": "act.01",
    "chapterId": "chapter.01.01",
    "title": "",
    "episodeRole": "",
    "source": {
      "sourceEpisodeFile": ""
    }
  },
  "story": {
    "scriptSynopsis": {
      "episodeSummary": "",
      "sceneBeats": [],
      "emotionalTone": [],
      "mustShow": [],
      "mustHide": []
    },
    "choiceDirections": []
  },
  "monster": {},
  "battle": {},
  "reward": {
    "rewardPolicyRef": "Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md",
    "rewardType": "gold"
  },
  "handoff": {},
  "constraints": []
}
```

## Composition JSON Direction

`episode_composition.chapter_XX.json` should summarize sequence and downstream
needs.

Each entry should include:

```text
episodeId
episodeTitle
episodePlanningRef
episodeRole
storyPurpose
scriptNeed
monsterPlanningNeed
battleNeed
rewardNeed
lockedReveals
notes
```

It should not include full synopsis, full monster direction, or full battle
direction.

## Episode Battle Monster Pool Direction

Create a separate file when episode battle composition needs a monster pool:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
```

This file exists before concrete monster creation.

It should define:

```text
episodeId
poolPurpose
storyBasis
poolDirection
desiredSlots
avoidExistingCandidatesForThisEpisode
battlePoolReadiness
notes
```

`desiredSlots` should describe role, difficulty, story reason, visual
requirements, and count weight at planning level.

Allowed:

- desired role slots
- monster family direction
- difficulty direction
- visual requirements
- forbidden pressure
- existing candidate refs for review only

Not allowed:

- final CharacterSO binding
- stats
- skills
- exact spawn count
- assuming an existing monster must be used

Existing monster refs must be marked as candidates only.

## Validation

Before finishing:

1. Validate all generated JSON syntax.
2. Validate every `commonDataRef` exists.
3. Validate every `episodePlanningRef` exists.
4. Validate every `sourceEpisodeFile` exists.
5. Validate `scriptSynopsis` is synopsis-level and not final script prose.
6. Validate `monster` gives only family, role, and difficulty direction.
7. Validate `battle` gives only shape, mood, and difficulty direction.
8. Validate reward direction uses `RewardPlanningGuide.md`.
9. Validate current reward type is only `gold`.
10. Validate episode battle monster pool refs, when present, are pre-monster-creation planning files.
11. Validate episode battle plan refs, when present, select an existing reusable spawner.
12. If no reusable spawner matches, validate that no episode battle plan JSON was created and that the final response reports spawner creation is required.
13. Validate no final runtime asset data is embedded.

## Final Response

Keep the final response short.

Report:

- Output folder
- Common JSON
- Story context JSON
- Episode composition JSON
- Episode planning count
- Episodes needing formal script
- Episodes needing monster planning
- Episodes needing battle planning
- Validation result
