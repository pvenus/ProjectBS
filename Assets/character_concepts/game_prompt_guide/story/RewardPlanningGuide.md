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

This is the reward content only. It must be nested under a planning wrapper that
declares the execution owner and trigger as shown below.

`rewardType` answers **what** is granted. It does not answer **where or when**
the reward is executed. Every new reward plan must also declare ownership:

```text
rewardOwner: battle | popup
rewardTrigger: battle_clear | choice_confirm | episode_clear | chapter_clear
```

Ownership is authoritative:

| rewardOwner | Meaning | Stage popup `choices[].rewards[]` |
|---|---|---|
| `battle` | Granted by the battle-clear reward pipeline after victory. | Must not contain a `Gold` payout. The popup may contain only the `SpecialBattle`/`BossBattle` action that enters the referenced battle. |
| `popup` | Granted immediately by `StagePopupEventManager` when the choice is confirmed. | May contain a concrete `Gold` payload. |

Never infer ownership from `rewardType: gold`. Both owners may ultimately grant
gold. Determine ownership from `rewardOwner` and `rewardTrigger` first.

For legacy planning without these fields, classify an intent containing
`battle_reward`, a reason saying the reward is for battle clear, or a choice
that opens a battle as `rewardOwner: battle`. Do not convert it to popup Gold.
If the evidence conflicts or remains unclear, stop with
`ambiguous_reward_owner` instead of guessing.

Recommended planning wrapper:

```json
{
  "rewardIntent": "gold_clear_reward",
  "rewardPolicyRef": "Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md",
  "rewardOwner": "popup",
  "rewardTrigger": "episode_clear",
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
  "rewardOwner": "popup",
  "rewardTrigger": "choice_confirm",
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
    "rewardOwner": "battle",
    "rewardTrigger": "battle_clear",
    "rewardIntent": "gold_battle_reward"
  }
}
```

`gold_battle_reward` is a battle-clear payout intent. It must be handed to the
battle reward pipeline and must not become a popup `Gold` entry. A popup choice
that starts this battle uses `SpecialBattle` or `BossBattle` with the stable
`battleId`; that entry is a transition action, not the gold payout itself.

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
- Every new reward has `rewardOwner` and `rewardTrigger`.
- `rewardOwner: battle` never produces a popup `Gold` payload.
- A `SpecialBattle`/`BossBattle` popup entry is not treated as battle-clear loot.
- Missing or conflicting ownership fails as `ambiguous_reward_owner`.
- Story prose does not hard-code final economy balance.
- Battle context does not own exact reward tuning.
- Future reward types are mentioned only as expansion points.
