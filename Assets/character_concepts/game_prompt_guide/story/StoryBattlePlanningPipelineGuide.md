# Story Battle Planning Pipeline Guide

## Purpose

This guide splits story-to-battle generation into independent steps.

Each step should be runnable on its own when its inputs already exist.

Do not merge these responsibilities into one large prompt unless the user
explicitly asks for end-to-end generation.

## Document Roots

Copy-ready prompts live under:

```text
Assets/character_concepts/game_prompts
```

Reference guides, schemas, SO notes, policies, and workflow docs live under:

```text
Assets/character_concepts/game_prompt_guide
```

When a task says to run a prompt, open the matching file under
`game_prompts`. When a prompt says to read a guide, open the referenced file
under `game_prompt_guide`.

When creating or evaluating prompts, use:

```text
Assets/character_concepts/game_prompt_guide/prompt/PromptAuthoringGuide.md
Assets/character_concepts/game_prompt_guide/prompt/PromptEvaluationGuide.md
```

## Pipeline

```text
Episode Markdown
  -> Episode Planning JSON
  -> Stage Node JSON
  -> RoundNodeSO + PopupEventSO
  -> Episode Battle Monster Pool JSON
  -> Episode Battle Plan JSON
  -> Battle Background Image
  -> BattleStoryContext JSON
  -> BattleSO Input JSON
  -> BattleSO Asset Builder
```

Character or NPC creation can run before or after episode planning, but battle
binding must use only existing CharacterSO-compatible IDs.

Stage node generation is a sibling branch of battle generation. It turns episode
planning or formal script text into runtime map nodes and popup events.

## Common Input Contract

Use the same input names across prompts.

Required identity fields:

```text
actId: {act_id}
chapterId: {chapter_id}
actGroupId: {act_group_id}
episodeId: {episode_id}
```

Battle-specific fields are required from the battle plan step onward:

```text
battleId: {battle_id}
battleGroup: {battle_group}
```

Use these values consistently:

- `actId`: game/story act id such as `act.01`.
- `chapterId`: game/story chapter id such as `chapter.01`.
- `actGroupId`: planning/resource group folder id such as `cheongun_sangui_act1`.
- `episodeId`: episode file suffix used by `episode.{episode_id}.json`.
- `battleId`: stable BattleSO id.
- `battleGroup`: output folder grouping under `Assets/Doc/Battle/` and `Assets/Resources/battle/`.

Common planning paths:

```text
episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
episodeCompositionFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.chapter_XX.json
episodeBattleMonsterPoolFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
episodeBattlePlanFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.chapter_XX.json
```

Common battle output paths:

```text
backgroundImagePath: Assets/Resources/battle/battle_png/{battle_id}.background.png
outputBattleStoryContextFile: Assets/Doc/Battle/{battle_group}/{battle_id}.story_context.json
outputBattleJsonFile: Assets/Resources/battle/{battle_group}/{battle_id}.json
expectedBattleSOPath: Assets/Resources/battle/{battle_group}/{battle_id}.asset
```

## Independent Steps

### 1. Episode Planning

Guide:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodePlanningCreateGuide.md
```

Prompt:

```text
Assets/character_concepts/game_prompts/story/EpisodePlanningPrompt.md
```

Creates:

```text
Assets/Doc/StoryPlanning/{act_group_id}/{act_group_id}.story_common.json
Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.chapter_XX.json
```

This step owns synopsis-level planning only.

It must not create:

- final script prose
- CharacterSO data
- exact spawner selection
- BattleSO input JSON

### 1A. Episode Stage Node

Guide:

```text
Assets/character_concepts/game_prompt_guide/stage/EpisodeStageNodeCreateGuide.md
Assets/character_concepts/game_prompt_guide/stage/RoundNodeSO.md
Assets/character_concepts/game_prompt_guide/stage/PopupEventSO.md
```

Creates:

```text
Assets/Resources/stage_new/{chapter_group}/episode.{episode_id}.json
Assets/Resources/stage_new/nodes/{stageNodeId}.asset
Assets/Resources/stage_new/popup_events/{nodeId}.asset
```

This step creates runtime stage/event data:

- `RoundNodeSO`
- `PopupEventSO`

It can run after episode planning if draft popup text is acceptable.

For final player-facing text, run it after formal script generation.

It should not create monster pool data, spawner data, or BattleSO input JSON.

### 2. Episode Battle Monster Pool

Guide:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodeBattleMonsterPoolGuide.md
```

Prompt:

```text
Assets/character_concepts/game_prompts/story/EpisodeBattleMonsterPoolPrompt.md
```

Creates:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
```

This step is still before concrete monster creation.

Existing monster refs may be listed only as candidates.

It must not assume monsters already exist.

### 3. Episode Battle Plan

Guide:

```text
Assets/character_concepts/game_prompt_guide/battle/EpisodeBattlePlanGuide.md
```

Prompt:

```text
Assets/character_concepts/game_prompts/battle/EpisodeBattlePlanPrompt.md
```

Creates, only when a reusable spawner is selected:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.chapter_XX.json
```

This step chooses:

- battle mode
- selected reusable spawner
- spawner difficulty
- spawn count balance
- spawn slot to monster pool mapping
- background image direction
- BattleSO readiness

If no reusable spawner matches, this step fails and does not create the battle
plan JSON.

### 4. Battle Background Image

Guide:

```text
Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
```

Prompt:

```text
Assets/character_concepts/game_prompts/battle/BattleBackgroundImagePrompt.md
```

Input:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.chapter_XX.json
```

Creates:

```text
Assets/Resources/battle/battle_png/{battleId}.background.png
```

This step creates one composed 16:9 battle background Sprite by default.

It uses `backgroundImageDirection` from the battle plan and should target
`2560x1440` pixel-game background art unless the plan says otherwise.

It must not create split floor/background/parallax layers unless explicitly
requested.

### 5. Battle Generation From Plan

Guide:

```text
Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
```

Prompt:

```text
Assets/character_concepts/game_prompts/battle/BattleFromEpisodePlanPrompt.md
```

Creates:

```text
Assets/Doc/Battle/{battle_group}/{battle_id}.story_context.json
Assets/Resources/battle/{battle_group}/{battle_id}.json
```

This step creates the BattleSO input JSON.

It does not need to run Unity editor asset generation unless explicitly asked.

### 6. BattleSO Asset Build

Guide:

```text
Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
```

Prompt:

```text
Assets/character_concepts/game_prompts/battle/BattleSOAssetBuildPrompt.md
```

Input:

```text
Assets/Resources/battle/{battle_group}/{battle_id}.json
```

Creates:

```text
Assets/Resources/battle/{battle_group}/{battle_id}.asset
```

This step is a Unity editor builder operation.

## Dependency Map

```text
EpisodePlanningPrompt
  -> EpisodeStageNode JSON / RoundNodeSO / PopupEventSO
  -> EpisodeBattleMonsterPoolPrompt
    -> EpisodeBattlePlanPrompt
      -> BattleBackgroundImagePrompt
      -> BattleFromEpisodePlanPrompt
        -> BattleSOAssetBuildPrompt
```

Conditional spawner branch:

```text
EpisodeBattlePlanPrompt fails with no reusable spawner
  -> SpawnerCreatePrompt
    -> EpisodeBattlePlanPrompt again
```

Conditional character branch:

```text
EpisodeBattleMonsterPoolPrompt creates required monster slots
  -> character / monster creation prompts when CharacterSO-compatible IDs do not exist
  -> EpisodeBattlePlanPrompt or BattleFromEpisodePlanPrompt
```

`BattleBackgroundImagePrompt` and character/monster creation can run in parallel
after `EpisodeBattlePlanPrompt` if both have enough inputs.

`BattleSOAssetBuildPrompt` must run last because it depends on:

- BattleSO input JSON
- generated background Sprite
- generated SpawnSequenceSO
- CharacterSO references

## Prompt Dependency Table

| Prompt | Requires | Creates | Can run independently when inputs exist |
|---|---|---|---|
| `EpisodePlanningPrompt.md` | episode markdown, chapter/story refs | episode planning JSON, story context, composition | Yes |
| `EpisodeStageNodeCreateGuide.md` | episode planning JSON or formal script | stage node JSON, RoundNodeSO, PopupEventSO | Yes |
| `EpisodeBattleMonsterPoolPrompt.md` | episode planning JSON, story context | episode battle monster pool JSON | Yes |
| `EpisodeBattlePlanPrompt.md` | episode planning JSON, monster pool JSON, spawner search roots | episode battle plan JSON | Yes |
| `SpawnerCreatePrompt.md` | failed battle plan reason, episode planning, monster pool | reusable typed spawner JSON | Yes, usually after battle plan failure |
| `BattleBackgroundImagePrompt.md` | episode battle plan with `backgroundImageDirection` | battle background PNG Sprite | Yes |
| `BattleFromEpisodePlanPrompt.md` | episode battle plan, monster refs, selected spawner, optional background PNG | BattleStoryContext JSON, BattleSO input JSON | Yes |
| `BattleSOAssetBuildPrompt.md` | BattleSO input JSON, Sprite, SpawnSequenceSO, CharacterSO | BattleSO asset | Yes, but requires Unity editor/builder |

## Failure Rules

Fail early when required upstream data is missing.

Examples:

- No episode planning JSON exists: do not create battle monster pool.
- No monster pool exists: do not create episode battle plan.
- No reusable spawner matches: do not create episode battle plan.
- No selected spawner exists: do not create BattleSO input JSON.
- SpawnSequenceSO cannot resolve: do not build BattleSO asset.
- CharacterSO cannot resolve: do not build BattleSO asset.

## Status Values

Recommended planning status values:

```text
not_started
not_created_missing_input
not_created_needs_spawner
not_created_spawner_candidate_available
created
ready
failed
```

Use status values in index JSON only as lightweight summary.

Do not create failure artifact JSON files just to store an error.

## Validation Summary

Before finishing any step:

- Validate JSON syntax.
- Validate referenced files exist, except future hint refs explicitly marked as hints.
- Validate enum-like values against existing guides or code.
- Report whether the next step can run.
