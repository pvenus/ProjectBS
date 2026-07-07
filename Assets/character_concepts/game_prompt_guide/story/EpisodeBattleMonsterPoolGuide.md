# Episode Battle Monster Pool Guide

## Purpose

Create a battle-focused monster pool planning file from episode planning.

This file exists before concrete monster creation.

It answers:

- What monster roles does this episode battle need?
- Which roles are primary, secondary, optional, or forbidden?
- Which existing monster planning files may be reviewed as candidates?
- Is the current candidate coverage enough for later battle binding?

It does not create:

- CharacterSO data
- stats
- skills
- final monster IDs as mandatory requirements
- spawn timing
- BattleSO JSON

## Output Path

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
```

Recommended `documentType`:

```text
episodeBattleMonsterPoolPlan
```

## Inputs

Required:

- episode planning JSON
- story context JSON
- episode composition JSON

Optional:

- existing `monster_context.{groupId}.json`
- existing `monster_composition.chapter_XX_YY.json`
- existing character planning JSON under `Assets/Doc/Character/{groupId}/npc`

Existing character refs are candidates only.

## Required Fields

Top-level:

```text
poolPlanId
documentType
actId
chapterId
commonDataRef
storyContextRef
sourceStoryRefs
sourcePlanningRefs
globalRules
episodePools
```

Each `episodePools[]` entry should include:

```text
episodeId
episodeTitle
episodePlanningRef
battleNeed
poolPurpose
storyBasis
poolDirection
desiredSlots
avoidExistingCandidatesForThisEpisode
battlePoolReadiness
notes
```

## Slot Rules

Each desired slot should describe:

```text
slotKey
slotRole
need
difficultyIntent
countWeight
storyReason
visualRequirements
existingCandidateRefs
```

Allowed `need` values:

```text
primary
secondary
optional
optional_flavor
forbidden
```

Allowed role values should match spawn role language:

```text
Melee
Ranged
Tank
Support
Elite
Boss
```

## Candidate Rules

Use this shape:

```json
{
  "candidateUse": "reference_only",
  "characterId": "character.black_cloth_raider.1",
  "planningRef": "Assets/Doc/Character/group/npc/character.black_cloth_raider.1.json",
  "fitReason": "기본 근접 압박 방향과 맞는다."
}
```

Do not write candidates as required prerequisites.

## Readiness Rules

`battlePoolReadiness` should say whether the desired directions are covered.

It is allowed to be ready even when not every optional role is covered.

Primary and secondary slots should be covered before battle generation.

## Validation

- JSON syntax is valid.
- Every `episodePlanningRef` exists.
- Every candidate planning ref exists when listed.
- Existing refs are marked as candidates only.
- No CharacterSO path is required.
- No runtime stats or skills are created.
- No spawnSequenceId or exact BattleSO field is included.

