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
backgroundImageDirection
spawnerSelection
monsterPoolSelection
battleSOReadiness
```

## Background Image Direction

The episode battle plan should include image-generation direction for the
BattleSO background, but it should not create the image file.

`backgroundImageDirection` should describe:

```text
status
assetMode
targetFileName
targetResourcePath
style
aspectRatio
targetResolution
camera
environment
combatReadability
mood
mustInclude
mustAvoid
promptSeed
```

Recommended defaults:

- `assetMode`: `single_sprite`
- `targetFileName`: `{battleId}.background.png`
- `targetResourcePath`: `Assets/Resources/battle/battle_png/{battleId}.background.png`
- `style`: `pixel_game_background`
- `aspectRatio`: `16:9`
- `targetResolution`: `2560x1440`

The direction should be derived from battle planning data:

- story location and time from episode planning
- battle purpose and emotional tone from `battleDirection`
- space tags and gameplay constraints from battle story context
- spawner rhythm only as visual pacing hints, not as literal spawn markers
- forbidden conditions as visual avoid rules

The background is a gameplay surface, not an illustration cutscene.

It should preserve a readable central combat area, avoid large foreground
blockers, and avoid characters, monsters, UI, text, logos, or story spoilers.

Do not split background layers by default.

Use one composed background sprite unless the user explicitly requests separate
background/floor/parallax layers.

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
