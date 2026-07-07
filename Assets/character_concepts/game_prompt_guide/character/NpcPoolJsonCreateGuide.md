# NPC Pool JSON Create Guide

## Purpose

Create NPC/monster planning pool index JSON files from character planning data.

This step organizes already planned NPC files so later battle generation can
select a monster pool.

It is different from `episode_battle_monster_pool.chapter_XX.json`.

- NPC pool JSON describes available or planned NPCs for an Act/group.
- Episode battle monster pool JSON describes what one episode battle needs,
  even before those NPCs exist.

## Output Paths

Create or update:

```text
Assets/Doc/Character/{groupId}/monster_context.{groupId}.json
Assets/Doc/Character/{groupId}/monster_composition.chapter_XX_YY.json
```

Recommended source character files:

```text
Assets/Doc/Character/{groupId}/npc/{characterId}.json
```

## Inputs

Required:

- group id
- Act or Chapter story context
- NPC planning files, when they exist

Recommended references:

```text
Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
Assets/Doc/Story
Assets/Doc/Character/{groupId}/npc
```

## monster_context Role

`monster_context.{groupId}.json` should summarize the available NPC pool.

It should include:

- group identity
- source refs
- monster families
- character refs
- combat roles
- difficulty tiers
- story use
- forbidden or delayed reveal notes

It should not duplicate full character JSON.

## monster_composition Role

`monster_composition.chapter_XX_YY.json` should describe how the pool can be
used across chapters or episodes.

It should include:

- chapter range
- battle or episode use hints
- primary/secondary/optional candidates
- boss or elite timing
- roles that are still missing
- refs to NPC planning files

## Boundary Rules

This step may reference NPC planning files.

It must not:

- create CharacterSO JSON
- create runtime assets
- create skills
- create images
- select exact spawner slots
- create BattleSO input JSON

## Validation

- JSON syntax is valid.
- Referenced NPC planning files exist.
- Character IDs use the `character.*` domain.
- Index files contain refs and summaries only.
- Missing roles are listed instead of invented.
- Delayed reveal monsters are not marked as early battle candidates.

