# Stat Enum Guide

## Identity

| Stat | Description |
|------|-------------|
| Reputation | General reputation value. |
| Fame | Positive reputation earned through honorable actions. |
| Notoriety | Negative reputation earned through infamous actions. |

## Survival

| Stat | Description |
|------|-------------|
| MaxHp | Increases maximum HP. |
| MaxHpPercent | Increases maximum HP by a percentage. |
| Hp | Current HP value. |
| HpRegen | Restores a fixed amount of HP over time. |
| HpRegenMaxHpPercent | Restores HP over time based on maximum HP. |
| BleedDamagePerSecond | Deals bleed damage every second. |
| Defense | Reduces incoming damage. |
| Shield | Grants a fixed shield. |
| ShieldPercent | Grants a shield based on maximum HP or effect value. |
| LifeSteal | Restores a fixed amount of HP from damage dealt. |
| LifeStealPercent | Restores HP as a percentage of damage dealt. |
| ReflectDamagePercent | Reflects a percentage of received damage. |
| StatusResistance | Reduces status effect strength. |
| StatusResistancePercent | Increases status resistance by a percentage. |
| StunDuration | Modifies stun duration. |
| RootDuration | Modifies root duration. |
| StartBattleShield | Grants a shield at battle start. |
| ResurrectionToken | Grants an additional resurrection. |

## Attack

| Stat | Description |
|------|-------------|
| Attack | Increases attack power. |
| AttackPercent | Increases attack power by a percentage. |
| AttackSpeed | Increases attack speed. |
| AttackSpeedPercent | Increases attack speed by a percentage. |
| MoveSpeed | Increases movement speed. |
| MoveSpeedPercent | Increases movement speed by a percentage. |
| CritChance | Increases critical hit chance. |
| CritDamage | Increases critical damage. |
| FinalDamageAmplify | Increases final damage dealt after all calculations. |
| BossDamagePercent | Increases damage dealt to bosses. |
| EliteDamagePercent | Increases damage dealt to elite enemies. |
| EliteApproachMoveSpeedPercent | Increases movement speed while approaching elite enemies. |
| SkillRange | Increases skill range. |
| SkillRangePercent | Increases skill range by a percentage. |

## Conditional Combat

| Stat | Description |
|------|-------------|
| MissingHpAttackPercent | Increases attack based on missing HP. |
| MissingHpFinalDamageAmplify | Increases final damage based on missing HP. |
| LowHpAttackBonus | Grants additional attack while HP is low. |
| LowHpDefenseBonus | Grants additional defense while HP is low. |
| SurroundedAttackPercent | Increases attack while surrounded by enemies. |
| SurroundedDamageReductionPercent | Reduces damage while surrounded by enemies. |

## Progression

| Stat | Description |
|------|-------------|
| Level | Current level. |
| Experience | Current experience value. |
| ExpGain | Increases experience gained. |
| KillCount | Total enemies defeated. |
| EliteKillCount | Total elite enemies defeated. |
| BossKillCount | Total bosses defeated. |
| KillStack | Current kill stack value. |
| KillStackAttackPercent | Grants attack based on kill stacks. |
| KillStackAttackPercentAmplify | Amplifies attack gained from kill stacks. |

## Economy

| Stat | Description |
|------|-------------|
| GoldGain | Increases gold gained. |
| DropGold | Increases gold dropped by enemies. |
| BonusGoldDropChance | Chance to receive bonus gold. |
| BonusGoldDropPercent | Increases bonus gold amount. |
| DropExp | Increases experience dropped by enemies. |
| EliteGoldBonus | Grants additional gold from elite enemies. |
| BattleEndBonusGold | Grants bonus gold after battle. |
| MaxOwnedGold | Increases maximum gold capacity. |
| MaxOwnedGoldAttackPercent | Grants attack based on owned gold. |
| GoldInterestPercent | Grants interest based on current gold. |
| BattleEndGoldInterestPercent | Applies gold interest at battle end. |
| RelicDropRate | Increases relic drop rate. |
| RelicDropRatePercent | Increases relic drop rate by a percentage. |

## AI & Skill

| Stat | Description |
|------|-------------|
| AiReactionSpeed | Increases AI reaction speed. |
| AiReactionSpeedPercent | Increases AI reaction speed by a percentage. |
| CooldownReduction | Reduces skill cooldown time. |

## Consumables

| Stat | Description |
|------|-------------|
| ConsumableEffectiveness | Increases consumable effectiveness. |
| ConsumableEffectivenessPercent | Increases consumable effectiveness by a percentage. |

## Faith

| Stat | Description |
|------|-------------|
| LifeFaithLevel | Life faith progression level. |
| WarFaithLevel | War faith progression level. |
| GreedFaithLevel | Greed faith progression level. |
| DarkFaithLevel | Dark faith progression level. |
| LifeAffinity | Affinity toward the Life faith. |
| WarAffinity | Affinity toward the War faith. |
| GreedAffinity | Affinity toward the Greed faith. |
| DarkAffinity | Affinity toward the Dark faith. |