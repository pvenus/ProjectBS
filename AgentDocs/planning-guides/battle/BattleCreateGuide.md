# Battle Create Guide

> Migration note: only this guide's background-image generation execution is
> deprecated. It is replaced by
> `AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md`;
> non-image battle planning contracts remain authoritative here.


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## Generated Image Storage Reference

Before generating, downloading, evaluating, promoting, or resolving a generated
image, read and apply:

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
```

This storage guide is mandatory. Its `Assets/ImagesGenerated` contract takes
precedence over legacy generated-image output paths under `Assets/Resources`.
Existing reference-only assets may remain in their documented legacy locations.

## Guide Contract

- **Guide type:** battle-data workflow guide with a prompt-authoring handoff for background images.
- **Responsibility:** author and validate Battle data; for a background, resolve only the approved visual brief and future asset identity.
- **Inputs/preconditions:** approved battle plan, monster pool, spawner contract, and readable required references.
- **Handoff:** Battle JSON plus a `battle_background` prompt-authoring request. Provider generation is owned by `BattleBackgroundImagePrompt.md`.
- **Mutation boundary:** this guide does not generate, download, evaluate, promote, import, or create Unity image metadata.

Battle planning controls narrative intent, the BattleSO schema controls data
shape, and the generated-image common guides control image stage boundaries. If
these authorities conflict, stop with `battle_authority_conflict` and escalate
the exact fields; do not infer a resolution.

This guide defines how to create BattleSO data from story context, monster pool
data, and reusable spawn variations.

Battle data is responsible for choosing actual monsters. Spawner data is
responsible only for timing, placement, and role slots.

## Required References

- Battle story context guide: `AgentDocs/planning-guides/battle/BattleStoryContextGuide.md`
- Battle generation prompt: `AgentDocs/task-prompts/battle/BattleGenerationPrompt.md`
- Episode battle plan guide: `AgentDocs/planning-guides/battle/EpisodeBattlePlanGuide.md`
- Battle from episode plan prompt: `AgentDocs/task-prompts/battle/BattleFromEpisodePlanPrompt.md`
- Battle background image prompt: `AgentDocs/task-prompts/battle/BattleBackgroundImagePrompt.md`
- BattleSO asset build prompt: `AgentDocs/task-prompts/battle/BattleSOAssetBuildPrompt.md`
- Spawner creation guide: `AgentDocs/planning-guides/spawner/SpawnerCreateGuide.md`
- Spawner SO schema: `AgentDocs/planning-guides/spawner/data-structures/SpawnSO.md`
- Character planning data: `AgentDocs/planning-data/character/act-plans`
- Story data: `AgentDocs/planning-data/story`

## Authoring Inputs

Battle creation should receive:

- act id
- chapter id
- episode or battle id
- battle story context
- selected monster pool
- desired battle intensity
- player progression context
- reusable spawn variation candidates

The battle generator should not invent monster identity directly from spawner
slots. It should first select monsters from the prepared monster pool, then bind
them to spawn slots.

## Battle Creation Pipeline

1. Read story context.
2. Determine battle purpose:
   - tutorial pressure
   - patrol encounter
   - ambush
   - survival wave
   - elite check
   - boss battle
   - boss reinforcement phase
3. Select monster pool entries.
4. Select a reusable spawn variation.
5. Record the selected spawner type, difficulty, source file, and selection reason in battle JSON.
6. Read the selected spawner's required `spawnUnitKey` and `spawnRole` slots.
7. Map monsters to those slots through BattleSO `spawnUnitBindings`.
8. Create or confirm the background-image brief and handoff identity from battle planning.
9. Create or update BattleSO.
10. Validate that the spawn sequence can resolve every required slot.

## BattleSO Data

BattleSO must store:

- battle identity and display metadata
- selected background sprite
- battle rules and rewards
- selected `SpawnSequenceSO`
- `spawnUnitBindings`

`spawnUnitBindings` is the bridge between abstract spawner slots and concrete
CharacterSO monsters.

Example concept:

```json
{
  "battleId": "battle.act1.chapter01.forest_ambush",
  "spawnerSelection": {
    "spawnerType": "field_ambush",
    "difficulty": "normal",
    "sourceJsonPath": "Assets/Resources/battle/spawner/Jsons/sequence_presets/field_ambush.json",
    "sequenceId": "seq.field_ambush.normal",
    "selectionReason": "Matches forest ambush with front melee pressure and rear ranged support."
  },
  "spawnSequenceId": "seq.act1.forest.ambush",
  "spawnUnitBindings": [
    {
      "unitKey": "spawn.front.pressure.melee",
      "role": "Melee",
      "characterId": "monster.act1.forest.striker"
    },
    {
      "unitKey": "spawn.rear.support.ranged",
      "role": "Ranged",
      "characterId": "monster.act1.forest.slinger"
    }
  ]
}
```

The exact runtime asset field is a CharacterSO reference. JSON authoring may use
`characterId` only as an editor/build-time lookup key.

## Background Image Prompt-Authoring Handoff

Battle background image generation is driven by battle planning data.

Use the selected battle plan's `backgroundImageDirection` together with:

- battle id
- battle purpose
- location and time of day
- space tags
- rhythm tags as mood/pacing hints
- forbidden conditions
- required gameplay readability

Future PASS-only promotion target (informational):

```text
Assets/ImagesGenerated/Battle/background/{battleId}.background.png
```

The BattleSO input JSON should store:

```json
{
  "backgroundSprite": "{battleId}.background"
}
```

If `backgroundSprite` is omitted, the editor builder attempts to find a Sprite
named `{battleId}.background`.

Image generation requirements:

- Create one composed 16:9 background sprite by default.
- Target `2560x1440`.
- Use a pixel-game background style unless the plan explicitly says otherwise.
- Keep the center readable for combat.
- Keep objects small enough that characters and enemy attacks remain visible.
- Avoid characters, monsters, UI, text, logos, and large foreground blockers.
- Do not include spawn markers or literal encounter diagrams.
- Do not split floor/background/parallax layers unless the user explicitly asks.

Return the visual brief, provider prompt request, `battleId`, and future target
identity to the generation task. Do not require a PNG, `.meta`, Sprite GUID, or
builder result at this stage. Those checks belong to later download,
immutable-evaluation, and PASS-only promotion tasks.

## Battle-Spawner Connection Metadata

Battle JSON should keep a `spawnerSelection` object so the encounter can be
reviewed without opening the generated `SpawnSequenceSO`.

This object is authoring/debug metadata. Runtime execution still depends on:

- `spawnSequenceId` or `spawnSequencePath`
- `spawnUnitBindings`

Recommended shape:

```json
{
  "spawnerSelection": {
    "spawnerType": "elimination_90s_swarm",
    "difficulty": "normal",
    "sourceJsonPath": "Assets/Resources/battle/spawner/Jsons/sequence_presets/elimination_90s_swarm.json",
    "sequenceId": "seq.elimination_90s_swarm.normal",
    "targetPartySize": 3,
    "targetSpawnCount": 80,
    "spawnWindowSec": 60,
    "clearWindowSec": 30,
    "matchedTags": {
      "intentTags": ["pressure", "attrition"],
      "spaceTags": ["surround"],
      "rhythmTags": ["escalating", "staggered"]
    },
    "selectionReason": "90-second elimination battle using normal 3-player swarm pacing.",
    "requiredSlots": [
      {
        "unitKey": "spawn.swarm.fodder.melee",
        "role": "Melee",
        "purpose": "main count filler",
        "bindingStrategy": "exact"
      },
      {
        "unitKey": "spawn.swarm.pressure.ranged",
        "role": "Ranged",
        "purpose": "force movement and target priority",
        "bindingStrategy": "exact"
      }
    ]
  }
}
```

Keep these fields consistent:

- `spawnerSelection.sequenceId` must match `spawnSequenceId` unless `spawnSequencePath` is intentionally used.
- `spawnerSelection.sourceJsonPath` must point to the typed spawner file.
- `spawnerSelection.difficulty` must match the difficulty object that owns the selected sequence.
- `spawnerSelection.requiredSlots` should be derived from the selected sequence groups.
- `spawnUnitBindings` must cover every required slot through exact binding or role fallback.

## Spawner Selection Rules

Choose a spawn variation based on battle intent, not monster names.

Good matching signals:

- approach direction: front, rear, side, surround
- tempo: burst, staggered, looped, delayed
- threat shape: melee rush, ranged line, tank anchor, elite arrival
- story moment: chase, trap, ritual, defense, boss phase
- map topology: narrow lane, open arena, choke point, multi-entry space
- player skill test: movement, dodge timing, target priority, area denial

Avoid selecting a spawner because it was authored for a specific monster name.

## Monster Binding Rules

Use exact key binding when a specific slot matters.

Example:

```json
{
  "unitKey": "spawn.rear.support.ranged",
  "role": "Ranged",
  "characterId": "monster.act1.bandit.thrower"
}
```

Use role fallback when the same monster can satisfy many equivalent slots.

Example:

```json
{
  "unitKey": "",
  "role": "Melee",
  "characterId": "monster.act1.forest.striker"
}
```

Resolution order:

1. exact `unitKey`
2. fallback `role`

## Validation Rules

Battle build should validate:

- `spawnSequenceId` resolves to a generated SpawnSequenceSO.
- Every `spawnUnitKey` in the sequence is either exactly bound or has a role fallback.
- Every `spawnRole` value is a supported enum.
- Every binding resolves to a valid CharacterSO.
- No spawner slot key contains concrete monster names.
- Boss or elite roles are intentionally bound.
- The selected monster pool can support all required spawn pressure.

Spawner build should validate:

- every step has inline `content`
- every group has `spawnUnitKey` or `spawnRole`
- every `patternKind` is valid
- pattern config is legal for its kind

## Battle JSON Responsibility

Battle JSON may reference generated spawner assets, but it should not duplicate
the full spawner layout.

Battle JSON should contain:

- `spawnerSelection`
- `spawnSequenceId`
- `spawnUnitBindings`

Spawner JSON should contain:

- sequence steps
- inline squad content
- squad/group patterns
- abstract spawn slot keys and roles

## Example Flow

Story context says:

- forest ambush
- fast melee pressure from the front
- ranged harassment from behind
- no boss

Spawner selection:

- choose a surround or rear-support spawn variation

Spawner slots:

- `spawn.front.pressure.melee`
- `spawn.rear.support.ranged`

Monster pool:

- forest striker
- forest slinger

BattleSO binding:

- melee slot -> forest striker CharacterSO
- ranged slot -> forest slinger CharacterSO

## Output Checklist

- BattleSO references the selected SpawnSequenceSO.
- BattleSO has all required `spawnUnitBindings`.
- The spawner JSON remains monster-agnostic.
- Monster selection comes from the prepared monster pool.
- The encounter purpose is readable from battle data and binding choices.
