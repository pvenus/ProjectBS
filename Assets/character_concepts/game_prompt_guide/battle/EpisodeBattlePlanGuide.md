# Episode Battle Plan Guide

## Purpose

Create a concrete battle plan from episode planning, monster pool planning, and
existing reusable spawners.

This is the first step that may select a specific spawner.

It is still an authoring planning document, not the final BattleSO JSON.

## Output Path

Create only when a reusable spawner is selected:

```text
Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.chapter_XX.json
```

Recommended `documentType`:

```text
episodeBattlePlan
```

If no reusable spawner matches, do not create this file.

Report failure and say spawner creation is required.

## Inputs

Required:

- episode planning JSON
- episode battle monster pool JSON
- story context JSON
- existing spawner JSON files

Recommended spawner inputs:

```text
Assets/Resources/battle/spawner/Jsons/sequence_presets
Assets/Doc/Spawner
Assets/Scripts/battle_spawn/Resource/Jsons
```

## Selection Order

1. Read episode battle direction.
2. Confirm `partyAssumption` from source story or episode planning.
3. Read monster pool primary and secondary slots.
4. Search existing spawner candidates.
5. Reject candidates that violate forbidden roles or forbidden pressure.
6. Accept candidates whose required slots can be bound from primary/secondary
   monster pool slots.
7. Optional pool slots may be omitted if the selected spawner does not need them.
8. Create episode battle plan only after a candidate is selected.

## Required Battle Plan Content

Each plan entry should include:

```text
episodeId
episodeTitle
episodePlanningRef
battleMonsterPoolRef
battleId
battleStoryContextRef
battleJsonRef
planningStatus
battleDirection
spawnerSelection
monsterPoolSelection
battleSOReadiness
```

## Spawner Selection

`spawnerSelection` should include:

```text
status
spawnerType
difficulty
sourceJsonPath
sequenceId
sequenceAssetPath
targetPartySize
targetSpawnCount
spawnWindowSec
clearWindowSec
selectionReason
requiredSlots
excludedOptionalSlots
```

`requiredSlots` must be derived from actual selected spawner groups.

Do not invent slot keys.

## Monster Pool Binding

`monsterPoolSelection.selectedBindings[]` should map:

```text
poolSlotKey
spawnUnitKey
spawnRole
characterId
bindingReason
```

Use exact `spawnUnitKey` binding when the selected spawner has specific slot
keys.

Use role fallback only when the spawner has interchangeable role slots.

## Failure Conditions

Fail without creating battle plan JSON when:

- episode planning is missing
- monster pool is missing
- no reusable spawner exists
- all spawner candidates violate required story/battle constraints
- required spawner slots cannot be bound from the monster pool
- selected character candidates cannot resolve to CharacterSO when final binding
  is required

Failure response should include:

- why no battle plan was created
- rejected spawner candidates and reasons
- required new spawner direction
- whether monster pool or spawner creation should run next

## Validation

- Battle plan JSON syntax is valid.
- `planningStatus` is `ready`.
- Selected spawner JSON exists.
- Selected sequence asset exists or can be resolved by `spawnSequenceId`.
- Every required slot has exact binding or role fallback.
- No forbidden role is used.
- `battleSOReadiness.canCreateBattleSO` is true only when all bindings resolve.

