# Story Planning Context Guide

## Purpose

`StoryPlanningContext` is the first planning layer created from episode prose.

It should translate story writing into game-design information before battle,
spawner, reward, NPC, or character generation begins.

This document is not a final runtime JSON schema. It defines a synopsis-level
planning surface that later generation steps should read.

For the concrete file creation process, use:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodePlanningCreateGuide.md
```

## Position In The Pipeline

```text
Episode Markdown
  -> StoryPlanningContext
  -> Formal script generation
  -> Monster / NPC planning
  -> BattleStoryContext
  -> Monster composition / Spawner selection
  -> BattleSO / CharacterSO / SpawnSO
```

`StoryPlanningContext` should be created before NPC-specific planning.

## What It Owns

`StoryPlanningContext` owns:

- episode role and player-facing purpose
- script synopsis and scene beats
- choice/result direction
- reward direction
- battle direction
- monster family, role, and difficulty direction
- reveal constraints
- references to source story files

It does not own:

- final BattleSO JSON
- final SpawnSequenceSO data
- concrete CharacterSO values
- exact NPC stats
- final monster skill data
- exact spawner type
- exact spawn timing or spawn count
- exact reward tuning beyond the current reward guide
- full dialogue rewrites

## Episode Role

Each episode should identify its main role.

Recommended values:

```text
intro
investigation
choice_branch
companion_join
travel
ambush
rescue
breakthrough
defense
boss_reveal
aftermath
transition
```

One episode may have secondary roles, but it should have one primary role.

## Script Synopsis Planning

Story text should be converted into a synopsis-level handoff for later formal
script generation.

Recommended fields:

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
        "tense",
        "dry",
        "uneasy"
      ],
      "sceneFunction": "Introduce the first visible threat without revealing the Sangui truth.",
      "mustShow": [
        "black cloth",
        "red doll charm",
        "improvised weapons"
      ],
      "mustHide": [
        "sangui_true_form",
        "spirit_boss"
      ],
      "scriptHandoffHint": "Formal script should be generated later using character persona and this synopsis."
    }
  }
}
```

Use concise planning statements. Do not copy long story prose and do not write
the final script here.

## Choice And Result Planning

Choices should become intent data, not only button labels.

Recommended fields:

```json
{
  "story": {
    "choiceDirections": [
      {
        "choiceId": "stage.node.choice.1",
        "choiceIntent": "Protect villagers from the raiders.",
        "outcomeDirection": "Seojin enters combat to hold the raiders away from civilians.",
        "opens": [
          "battle"
        ],
        "rewardDirection": [
          "gold_clear_reward"
        ]
      }
    ]
  }
}
```

The outcome direction should explain what changes after the choice.

For battle choices, the outcome direction should stop at battle entry. Battle
resolution belongs to battle/reward generation.

## Battle Direction Surface

When an episode implies combat, keep only battle direction here.

The purpose is to let later `BattleStoryContext` generation infer mode,
difficulty, rhythm, and spawner needs.

Recommended fields:

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

Detailed battle conversion should follow:

```text
Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
```

Do not include exact spawner type, spawn timing, spawn count, or BattleSO data.

## Monster Planning Direction

Monster planning direction describes what kind of monster/NPC planning should
happen later.

Do not create concrete NPCs in this layer.

Recommended fields:

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

NPC planning may later split one monster family into multiple concrete enemies.

## Reward Planning Surface

Reward direction should be present at this layer, but exact reward expansion
should follow the reward guide.

Current reward guide:

```text
Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md
```

At this stage, use only `gold` as the concrete reward type and keep the reason
or scale at planning level.

Do not invent item, equipment, skill, or difficulty reward tables until the
reward guide is expanded.

## Recommended File Location

Recommended generated planning files:

```text
Assets/Doc/StoryPlanning/{act_group_id}/{act_group_id}.story_common.json
Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.chapter_XX.json
```

`episode_battle_plan.chapter_XX.json` is conditional. Create it only when a
reusable spawner has been selected for every included battle that needs one.

For the current Chapter 01 Sangui case:

```text
Assets/Doc/StoryPlanning/cheongun_sangui_act1/cheongun_sangui_act1.story_common.json
Assets/Doc/StoryPlanning/cheongun_sangui_act1/episode.act1.chapter01.01.json
Assets/Doc/StoryPlanning/cheongun_sangui_act1/episode_battle_monster_pool.chapter_01.json
Assets/Doc/StoryPlanning/cheongun_sangui_act1/story_context.cheongun_sangui_act1.json
Assets/Doc/StoryPlanning/cheongun_sangui_act1/episode_composition.chapter_01.json
```

## Split JSON Structure

Use a split structure similar to character planning.

### Common Data JSON

The common data JSON contains shared information used by every episode in the
group.

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

### Episode Data JSON

Each episode data JSON contains only one episode's planning data.

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

Episode JSON should use large planning categories:

| Category | Owns |
|---|---|
| `common` | Episode identity, role, and source refs. |
| `story` | Script synopsis, scene beats, choice directions, tone, must-show/must-hide story direction. |
| `monster` | Monster family, role direction, difficulty direction, visual direction, reuse/creation policy. |
| `battle` | Battle need, mood, shape, difficulty direction, pace direction, pressure, and avoid rules. |
| `reward` | Reward direction, current reward type, reward reason, reward guide ref. |
| `handoff` | Later generation needs and refs. |

### Context And Composition Index JSON

Use `story_context.{act_group_id}.json` to expose the available episode planning
files to later agents.

It may contain:

- `contextId`
- `actId`
- `groupId`
- `commonDataRef`
- `episodePlanningRefs`
- `episodeCompositionRefs`
- `episodeBattleMonsterPoolRefs`
- `episodeBattlePlanRefs` only when battle plan files exist
- `sourceStoryRefs`
- `downstreamRefs`

It must not contain:

- Full episode prose
- Full per-episode planning data
- Final battle data
- Final NPC data
- Final reward tables

Use `episode_battle_monster_pool.chapter_XX.json` for battle monster pool
direction before concrete monster creation. Existing monster refs in that file
are candidates only.

Use `episode_battle_plan.chapter_XX.json` for concrete battle balance, monster
pool ratios, spawn count balance, and selected reusable spawner data.

If no existing spawner matches, do not create an episode battle plan JSON. The
generation step must fail and report that spawner creation is required,
including the missing spawner direction.

Use `episode_composition.chapter_XX.json` when Chapter sequence and downstream
needs must be preserved.

It may contain:

- `compositionId`
- `chapterId`
- `episodeCompositions`
- `episodeRole`
- `storyPurpose`
- `battleNeed`
- `rewardNeed`
- `downstreamNeeds`
- `lockedReveals`

## Minimal Shape

```json
{
  "documentId": "episode_planning.act1.chapter01.01",
  "documentType": "episodePlanning",
  "commonDataRef": "Assets/Doc/StoryPlanning/cheongun_sangui_act1/cheongun_sangui_act1.story_common.json",
  "common": {
    "episodeId": "episode.act1.chapter01.01",
    "actId": "act.01",
    "chapterId": "chapter.01.01",
    "title": "청운촌의 습격",
    "episodeRole": "intro",
    "source": {
      "sourceEpisodeFile": "Assets/Doc/Story/Act01/Chapter01/01_episode1.md"
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

## Validation Checklist

- The source episode file is preserved.
- The episode role is clear.
- `story.scriptSynopsis` is synopsis-level, not final script prose.
- Choices explain what they open or change.
- `reward` is present when a choice resolves or starts a battle.
- `battle` is present only when the episode needs battle.
- `battle` does not contain exact spawn timing, spawn count, or final spawner type.
- `monster` gives family, role, and difficulty direction only.
- `monster` does not create concrete NPC stats.
- Reveal constraints preserve story mystery.
- Later files can trace back to this planning context.
