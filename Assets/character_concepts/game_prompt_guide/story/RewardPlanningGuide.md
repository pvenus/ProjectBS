# Reward Planning Guide

## Purpose

This guide defines the reward reference point for story-derived planning.

For now, rewards should only grant gold. The structure should leave room for
future difficulty-based and reward-type expansion, but generation should not
invent those rewards yet.

## Current Scope

Allowed reward type:

```text
gold
```

Not allowed yet:

```text
equipment
skill
character
material
random_drop
difficulty_scaled_bundle
```

Those can be added later after reward balance rules exist.

## Pipeline Position

```text
Episode Markdown
  -> StoryPlanningContext reward intent
  -> Reward planning
  -> Battle / episode clear result
  -> Runtime reward grant
```

Story and battle documents may reference reward intent, but exact reward rules
should be checked here.

## Current Gold Reward Model

Use a simple fixed gold reward entry.

```json
{
  "rewardType": "gold",
  "amount": 100
}
```

Recommended planning wrapper:

```json
{
  "rewardIntent": "gold_clear_reward",
  "rewardPolicyRef": "Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md",
  "rewards": [
    {
      "rewardType": "gold",
      "amount": 100
    }
  ]
}
```

## Story Choice Usage

When an episode choice gives a reward, write the reward intent in the planning
layer first.

```json
{
  "choiceName": "rescue_villagers",
  "choiceId": "choice.act1.chapter01.episode01.black_cloth_attack.rescue_villagers",
  "rewardIntent": [
    "gold_clear_reward"
  ]
}
```

New `choiceId` values must use the permanent planning `popupName` and semantic
`choiceName`; do not derive reward identity from a choice array index. Preserve
existing indexed ids as immutable legacy ids.

The story text should not decide final economy balance. It should only indicate
that a gold reward is appropriate.

## Battle Usage

For battle rewards, the battle or story planning file may carry a reward intent.

```json
{
  "battleRewardHint": {
    "rewardPolicyRef": "Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md",
    "rewardIntent": "gold_battle_reward"
  }
}
```

Do not put exact reward values inside `BattleStoryContext` unless a later task
explicitly changes the battle reward pipeline.

## Gold Amount Defaults

Until a full economy table exists, use conservative placeholder amounts.

```text
minor story choice: 25 gold
normal episode clear: 50 gold
normal battle clear: 100 gold
important chapter clear: 200 gold
```

These are placeholders for generation consistency, not final economy balance.

## Future Expansion Points

Later, this guide can be expanded with:

- difficulty-based gold scaling
- party-size-based reward scaling
- battle-mode reward multipliers
- first-clear and repeat-clear rewards
- item and material rewards
- skill upgrade rewards
- chapter and act completion bundles

Do not implement those until the guide explicitly defines them.

## Validation Checklist

- Reward entries use only `gold`.
- Gold amount is non-negative.
- Reward intent is traceable to a choice, episode clear, or battle clear.
- Story prose does not hard-code final economy balance.
- Battle context does not own exact reward tuning.
- Future reward types are mentioned only as expansion points.
