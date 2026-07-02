# Story Structure Guide

## Purpose

This document defines the project story document structure used by generation agents.

It explains where the core story references live and how story should be organized by:

```text
Act
  -> Chapter
  -> Episode
```

This guide is not a full story writing guide.

It is a structure and context-preservation guide so later generation steps can create:

- Character planning
- Monster pools
- Battle story context
- Encounter profiles
- Stage or event content

## Core Story Reference Files

Always read the story files in this order when generating story-derived content.

| Order | File | Purpose |
|---:|---|---|
| 1 | `Assets/Doc/Story/00_Background.md` | Full world concept and setting rules. |
| 2 | `Assets/Doc/Story/01_Overall_Story.md` | Overall act-level story summary. |
| 3 | `Assets/Doc/Story/Act_01_Background.md` | Act 1 setting, conflict, and local context. |
| 4 | `Assets/Doc/Story/Characters.md` | Global story character names, IDs, roles, and narrative positions. |
| 5 | `Assets/Doc/Story/Chapter_XX.md` | Chapter-level story summary. |

Do not read `.meta` files as story content.

`Characters.md.meta` is Unity metadata. The story content is:

```text
Assets/Doc/Story/Characters.md
```

`Characters.md` is shared across the whole story.

Do not treat it as Chapter-local content or copy it into generated Chapter or Act output folders.

## Current Story File Layout

Current story source folder:

```text
Assets/Doc/Story
```

Known files:

```text
00_Background.md
01_Overall_Story.md
Act_01_Background.md
Characters.md
Chapter_01.md
Chapter_02.md
Chapter_03.md
Chapter_04.md
Chapter_05.md
```

## Hierarchy

Story should be understood through this hierarchy:

```text
World
  -> Overall Story
    -> Act
      -> Chapter
        -> Episode
```

### World

World-level documents define rules that should not be contradicted.

Examples:

- Era and setting
- Social order
- Belief systems
- Supernatural rules
- Core moral tone

Current file:

```text
Assets/Doc/Story/00_Background.md
```

### Overall Story

Overall story defines the full narrative arc.

Examples:

- Main protagonist
- Primary conflict
- Long-term mystery
- Act-level emotional question
- Ending direction

Current file:

```text
Assets/Doc/Story/01_Overall_Story.md
```

### Act

Act-level documents define the major setting and conflict scope for a group of chapters.

Examples:

- Act location
- Act faction or threat
- Act mystery
- Act emotional theme
- Act-specific constraints

Current Act 1 file:

```text
Assets/Doc/Story/Act_01_Background.md
```

### Chapter

Chapter-level documents define a smaller story unit inside an Act.

Examples:

- Chapter title
- Immediate situation
- Main objective
- Key conflict
- Character motivation
- Battle or event hooks
- Transition to next chapter

Current chapter files:

```text
Assets/Doc/Story/Chapter_01.md
Assets/Doc/Story/Chapter_02.md
Assets/Doc/Story/Chapter_03.md
Assets/Doc/Story/Chapter_04.md
Assets/Doc/Story/Chapter_05.md
```

### Episode

Episode-level documents are the smallest planned playable or narrative unit under a Chapter.

Episodes may later become:

- Dialogue scenes
- Exploration events
- Battle encounters
- Choice events
- Reward moments
- Chapter transitions

Recommended episode file path:

```text
Assets/Doc/Story/Act_XX/Chapter_XX/Episode_XX.md
```

If the project keeps flat files for now, episode IDs must still preserve the hierarchy:

```text
act01.chapter01.episode01
```

## ID Rules

Use stable IDs for generated planning artifacts.

```text
act.{number}
chapter.{act_number}.{chapter_number}
episode.{act_number}.{chapter_number}.{episode_number}
story_context.{act_number}.{chapter_number}.{episode_number}
battle_story_context.{act_number}.{chapter_number}.{episode_number}.{battle_index}
```

Examples:

```text
act.01
chapter.01.01
episode.01.01.01
story_context.01.01.01
battle_story_context.01.01.01.battle01
```

## Chapter Document Expectations

A Chapter document should answer:

- What changed since the previous chapter?
- Where does this chapter happen?
- Which characters are involved?
- What is the immediate objective?
- What mystery, threat, or conflict appears?
- What battle, exploration, or event hooks exist?
- What emotional or plot information must be preserved?
- How does the chapter point to the next chapter?

## Generation Input Package

Agents that generate character planning, monster pools, battle context, or spawner mapping should preserve Act and Chapter information explicitly.

Recommended input shape:

```json
{
  "actId": "act.01",
  "chapterId": "chapter.01.01",
  "chapterFile": "Assets/Doc/Story/Chapter_01.md",
  "chapterRange": ["chapter.01.01", "chapter.01.05"],
  "playerPlanningRoot": "Assets/Doc/Character/player",
  "monsterContextRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_context.cheongun_sangui_act1.json",
  "monsterCompositionRef": "Assets/Doc/Character/cheongun_sangui_act1/monster_composition.chapter_01_05.json"
}
```

Use `actId` to select shared world, faction, race, and tone information.

Use `chapterId` or `chapterRange` to select immediate battle needs.

Use `playerPlanningRoot`, `monsterContextRef`, and `monsterCompositionRef` when they exist.

Do not duplicate full character planning data in story context.

Story-derived battle generation should carry these refs forward so later agents can inspect the source in Markdown and JSON files.

## Episode Document Expectations

An Episode document should answer:

- What is the playable or narrative unit?
- Which chapter does it belong to?
- What is the player-facing goal?
- What characters or factions are present?
- Is this episode dialogue, exploration, battle, choice, reward, or transition?
- If it contains battle, what story signals should become `BattleStoryContext`?
- What must not be contradicted by generated content?

Recommended episode structure:

```markdown
# Episode 01. Title

## Parent

- Act: act.01
- Chapter: chapter.01.01

## Summary

Short episode summary.

## Purpose

What this episode accomplishes in the chapter.

## Characters

Characters present or referenced.

## Story Signals

Tags or notes useful for generation.

## Battle Hooks

Battle-relevant context, if any.

## Constraints

What generated content must preserve or avoid.
```

## Context Handoff To Battle Generation

When a story segment contains a battle, do not pass the whole story prose directly to battle generation.

Instead, create:

```text
BattleStoryContext
```

The primary input for this extraction should be the relevant Chapter story document.

Reference guide:

```text
Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
```

The handoff should preserve:

- source story file
- act ID
- chapter ID
- episode ID
- short battle-relevant summary
- location tags
- faction tags
- situation tags
- intent tags
- space tags
- rhythm tags
- required or forbidden monster tags
- required or forbidden spawn tags

## Context Preservation Keys

All generated downstream planning files should keep these references when available:

```text
sourceStoryFile
actId
chapterId
episodeId
storyContextId
battleStoryContextId
```

These keys let agents trace generated battle, monster, and spawner data back to the story source.

## Separation Rules

Story structure documents should not contain:

- Final BattleSO JSON
- Final SpawnSequenceSO JSON
- Full monster stats
- Full skill implementation JSON
- Unity asset references unless required by an existing source document

Battle generation documents should not rewrite:

- World rules
- Character names
- Major chapter plot points
- Act-level mystery or ending direction

## Agent Workflow

When generating story-derived content:

1. Read `00_Background.md`.
2. Read `01_Overall_Story.md`.
3. Read the relevant `Act_XX_Background.md`.
4. Read `Characters.md`.
5. Read the relevant `Chapter_XX.md`.
6. Read or create the relevant Episode context.
7. Generate downstream context such as `BattleStoryContext`.
8. Preserve source IDs in all generated artifacts.

## Validation Checklist

- The generated content does not contradict the world background.
- The generated content fits the overall story arc.
- The generated content belongs to the correct Act.
- The generated content belongs to the correct Chapter.
- Episode IDs preserve Act and Chapter identity.
- Character names and IDs match `Characters.md`.
- Battle-related extraction uses `BattleStoryContext`, not raw story prose.
- Downstream artifacts preserve source IDs.
