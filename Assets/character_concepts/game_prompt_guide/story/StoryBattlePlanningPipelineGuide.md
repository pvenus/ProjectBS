# Story Battle Planning Pipeline Guide

## Purpose

This guide splits story-to-battle generation into independent steps.

Each step should be runnable on its own when its inputs already exist.

Do not merge these responsibilities into one large prompt unless the user
explicitly asks for end-to-end generation.

## Pipeline

```text
Episode Markdown
  -> Episode Planning JSON
  -> Episode Battle Monster Pool JSON
  -> Episode Battle Plan JSON
  -> Battle Background Image
  -> BattleStoryContext JSON
  -> BattleSO Input JSON
  -> BattleSO Asset Builder
```

Character or NPC creation can run before or after episode planning, but battle
binding must use only existing CharacterSO-compatible IDs.

## Independent Steps

### 1. Episode Planning

Guide:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodePlanningCreateGuide.md
```

Prompt:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodePlanningPrompt.md
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

### 2. Episode Battle Monster Pool

Guide:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodeBattleMonsterPoolGuide.md
```

Prompt:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodeBattleMonsterPoolPrompt.md
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
Assets/character_concepts/game_prompt_guide/battle/EpisodeBattlePlanPrompt.md
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
Assets/character_concepts/game_prompt_guide/battle/BattleFromEpisodePlanPrompt.md
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

Input:

```text
Assets/Resources/battle/{battle_group}/{battle_id}.json
```

Creates:

```text
Assets/Resources/battle/{battle_group}/{battle_id}.asset
```

This step is a Unity editor builder operation.

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
