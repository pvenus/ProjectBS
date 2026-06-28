# Skill Balance Guide

1. Purpose

2. Golden Rule
   - Attack = 20
   - DPS 10 = Score 100
   - Basic Attack Baseline

3. Core Formula
   - Total Damage
   - DPS
   - Balance Score

4. Expected Stats
   - Attack
   - HP
   - Enemy HP
   - Enemy Attack

5. Skill Categories
   - Basic
   - Active1
   - Active2
   - Active3
   - Passive

6. Target Score
   - Slot별 목표 Score

7. Damage
   - BaseDamage
   - AttackPercentDamage

8. Cooldown

9. HitCount

10. HitRange

11. Projectile

12. Buff / Debuff

13. Crowd Control

14. Heal / Shield

15. Summon

16. Upgrade (Lv1~15)

17. Skill Generation Workflow

18. Checklist
# Skill Balance Guide

## 1. Purpose

This document defines the baseline rules used to design new skills, buff effects, debuff effects, summon effects, and upgrade data.

The numbers in this document are design guidelines, not final implementation limits.

When generating a new skill, use this guide first, then convert the result into implementation JSON.

---

## 2. Golden Rule

All skill balance starts from the basic attack baseline.

```text
Expected Attack = 20
Base Damage = 10
Attack Percent Damage = 100%
Total Damage = 10 + 20 = 30
Cooldown = 3s
DPS = 10
Balance Score = 100
```

Therefore:

```text
DPS 10 = Balance Score 100
DPS 1 = Balance Score 10
```

This keeps early-game UI numbers small while still allowing later growth into 10s, 100s, and 1000s.

---

## 3. Core Formula

### Total Damage

```text
TotalDamage = BaseDamage + ExpectedAttack × AttackPercentDamage
```

`AttackPercentDamage` is written as percent in planning documents.

Example:

```text
ExpectedAttack = 20
BaseDamage = 10
AttackPercentDamage = 100%
TotalDamage = 10 + 20 = 30
```

### DPS

```text
DPS = TotalDamage / Cooldown
```

### Balance Score

```text
DamageScore = DPS × 10
```

For a simple single-target skill:

```text
BalanceScore = DamageScore
```

For a full skill:

```text
BalanceScore = DamageScore × HitCountFactor × AreaFactor × ProjectileFactor + UtilityScore
```

Use this score as an internal design value. It is not shown to players.

---

## 4. Expected Stats

Use these values as the default early-game balance baseline.

| Category | Value | Notes |
|----------|------:|-------|
| Expected PC Attack | 20 | Default value for damage calculations |
| Basic Attack DPS | 10 | Equals Balance Score 100 |
| Basic Attack Cooldown | 3s | Slow but readable early-game attack pace |
| Normal Monster HP | 30 - 60 | Dies in roughly 3 - 6 seconds to baseline DPS |
| Elite Monster HP | 150 - 250 | Requires sustained focus |
| Boss HP | 800+ | Depends on encounter length |
| Normal Monster Attack | 3 - 5 | Basic incoming damage |
| PC HP | 80 - 120 | Allows several hits before death |

These values should grow over time, but new low-grade skills should still be readable in one-digit or low two-digit damage ranges.

---

## 5. Skill Categories

| Category | Role | Baseline |
|----------|------|----------|
| Basic | Repeating basic attack | Score 100 |
| Active1 | Frequent low-impact active skill | Score 300 |
| Active2 | Main active skill | Score 500 |
| Active3 | High-impact active skill | Score 800 |
| Passive | Conditional or always-on support | Convert effect value into score |

Basic skills are balanced around repeated use.

Active skills are balanced around target score and target DPS.

Passive skills are balanced by estimating how much damage, defense, utility, or sustain they add over time.

---

## 6. Target Score

| Skill Slot | Target DPS | Target Score | Notes |
|------------|-----------:|-------------:|-------|
| Basic | 10 | 100 | Standard repeating attack |
| Active1 | 15 | 300 | Frequent active, can include utility |
| Active2 | 25 | 500 | Main active skill |
| Active3 | 40 | 800 | High-impact skill |

`Target DPS` alone does not always equal `Target Score`.

Active skills may include wide area, crowd control, buff, debuff, projectile behavior, or summon value.

Example:

```text
Active1 Target Score = 300
Damage Score = 180
Utility Score = 120
Final Score = 300
```

---

## 7. Damage

Skill damage is composed of fixed damage and attack-scaling damage.

```text
TotalDamage = BaseDamage + ExpectedAttack × AttackPercentDamage
```

### BaseDamage

`BaseDamage` is stable damage.

It is stronger in early stages and on low-attack characters.

Use it when a skill needs reliable minimum damage.

### AttackPercentDamage

`AttackPercentDamage` scales with the user's Attack stat.

With the default baseline:

```text
ExpectedAttack = 20
100% AttackPercentDamage = 20 damage
50% AttackPercentDamage = 10 damage
150% AttackPercentDamage = 30 damage
```

Use higher percent damage for skills that should scale well with equipment or late-game growth.

### Damage Guideline

| Skill Type | BaseDamage | AttackPercentDamage |
|------------|-----------:|--------------------:|
| Basic Grade1 | 10 | 100% |
| Basic Grade2 | 25 | 100% |
| Basic Grade3 | 46 | 100% |
| Active1 damage-only | 25 - 40 | 100% - 150% |
| Active2 damage-only | 50 - 80 | 150% - 250% |
| Active3 damage-only | 90+ | 250%+ |

These values assume single-target or small-area skills. Reduce damage when hit count, hit range, or utility is high.

---

## 8. Cooldown

Cooldown depends on skill type and user type.

### Repeating Skills

| User Type | Fast | Standard | Slow |
|-----------|-----:|---------:|-----:|
| PC | 1s | 2s | 3s |
| NPC | 1s | 3s | 5s |

PC repeated attacks should feel responsive.

NPC repeated attacks can use a wider cooldown range to create readable patterns.

### Active Skills

Active skill cooldown should be calculated from target DPS.

```text
Cooldown = TotalDamage / TargetDPS
```

Example:

```text
TargetDPS = 15
TotalDamage = 90
Cooldown = 6s
```

After calculating cooldown, adjust slightly for gameplay feel.

---

## 9. HitCount

`hitCount` means the maximum number of enemies that can be damaged by one hit.

| HitCount | Meaning |
|----------|---------|
| 1 | Single target |
| 2 - 10 | Limited target count |
| 999 | Unlimited penetration |

Do not multiply balance score by `999`.

For unlimited penetration, use expected target count.

| Skill Type | Expected Target Count |
|------------|----------------------:|
| Narrow line | 3 |
| Wide line | 4 |
| Circle AoE | 5 |
| Large AoE | 6 |

Base relationship:

```text
HitCountScore = DPS × HitCount × 10
```

Example:

```text
DPS 10 × HitCount 3 = Score 300
```

---

## 10. HitRange

`hitRange` means damage area size.

`hitRange` is radius-based.

A large `hitRange` can include targets behind the caster.

`castRange` and `hitRange` must be tuned together.

For melee attacks:

```text
castRange <= hitRange
```

Recommended melee guideline:

| Melee Type | castRange | hitRange | Notes |
|------------|----------:|---------:|-------|
| Close melee | 0.2 - 0.3 | 0.4 - 0.5 | Very short attack |
| Standard melee | 0.3 - 0.5 | 0.5 - 0.7 | Normal sword range |
| Extended melee | 0.5 - 0.8 | 0.6 - 0.9 | Spear-like melee |

AreaFactor guideline:

| hitRange | AreaFactor | Notes |
|---------:|-----------:|-------|
| 0.3 | 0.8 | Very small hit |
| 0.5 | 1.0 | Standard melee hit |
| 0.7 | 1.15 | Wide melee hit |
| 1.0 | 1.3 | Small AoE |
| 1.5 | 1.6 | Medium AoE |
| 2.0 | 2.0 | Large AoE |
| 3.0 | 2.8 | Very large AoE |

Large hit ranges should usually reduce damage, cooldown efficiency, or utility.

---

## 11. Projectile

Projectile behavior changes skill value.

| Projectile Behavior | Factor | Notes |
|---------------------|-------:|-------|
| Normal | 1.0 | No special behavior |
| Pierce | 1.2 | Can hit multiple enemies in a line |
| Bounce | 1.3 | Can reach additional targets |
| Split | 1.4 | Adds coverage |
| Homing | 1.5 | Higher hit reliability |
| Explosion | 1.3 | Adds area damage |
| Returning | 1.25 | Can hit on return path |
| Orbiting | 1.4 | Sustained area threat |

Projectile count is not fully linear because not every projectile hits.

| Projectile Count | Factor |
|------------------|-------:|
| 1 | 1.0 |
| 2 | 1.8 |
| 3 | 2.5 |
| 4 | 3.0 |
| 5 | 3.5 |
| 6+ | Custom estimate |

Use a higher factor only when hit reliability is high.

---

## 12. Buff / Debuff

Buffs and debuffs are converted into UtilityScore.

### Offensive Buffs

| Buff | Duration | Score |
|------|----------|------:|
| Attack +10% | 5s | 40 |
| Attack +20% | 5s | 90 |
| Attack +30% | 5s | 150 |
| Attack Speed +10% | 5s | 40 |
| Crit Chance +10% | 5s | 35 |
| Final Damage +10% | 5s | 70 |

### Defensive Buffs

| Buff | Duration | Score |
|------|----------|------:|
| Defense +10% | 5s | 35 |
| Defense +20% | 5s | 80 |
| Damage Reduction 10% | 5s | 45 |
| Damage Reduction 20% | 5s | 100 |

### Debuffs

| Debuff | Duration | Score |
|--------|----------|------:|
| Defense -10% | 5s | 40 |
| Defense -20% | 5s | 90 |
| Damage Taken +10% | 5s | 70 |
| Attack -10% | 5s | 35 |
| Move Speed -30% | 2s | 30 |

For party-wide buffs, multiply by expected affected ally count.

Default party count is `3`.

---

## 13. Crowd Control

Crowd control value is based on duration and severity.

| CC Type | Score Per 1s | Notes |
|---------|-------------:|-------|
| Slow 30% | 15 | Soft control |
| Slow 50% | 30 | Strong slow |
| Root | 60 | Stops movement only |
| Stun | 100 | Stops action and movement |
| Taunt | 40 | Depends on enemy behavior |
| Silence | 70 | Strong against skill users |
| Freeze | 100 | Treat as stun unless special rules exist |

For AoE CC:

```text
CCScore = ScorePerSecond × Duration × ExpectedTargetCount × AreaFactor
```

Example:

```text
Stun 1s × 3 targets = 300 score
```

---

## 14. Heal / Shield

Healing and shielding are converted into score using the same scale as damage.

Baseline:

```text
Heal 30 = Score 100
Shield 30 = Score 100
```

This matches the basic attack baseline:

```text
Damage 30 over 3s = DPS 10 = Score 100
```

| Effect | Value | Score |
|--------|------:|------:|
| Heal | 15 | 50 |
| Heal | 30 | 100 |
| Heal | 60 | 200 |
| Shield | 15 | 50 |
| Shield | 30 | 100 |
| Shield | 60 | 200 |

For healing over time or shielding over time, apply duration value reduction.

| Duration | Value Multiplier |
|----------|-----------------:|
| Instant | 1.0 |
| 3s | 0.9 |
| 5s | 0.8 |
| 10s | 0.7 |
| 15s+ | 0.6 |

---

## 15. Summon

Summon value is calculated from expected damage and utility.

```text
SummonScore = SummonDPS × Lifetime × 10 + UtilityScore
```

Then convert it into cooldown value:

```text
SummonDPSValue = SummonScore / Cooldown
```

Example:

```text
SummonDPS = 5
Lifetime = 10s
Cooldown = 20s
SummonScore = 5 × 10 × 10 = 500
SummonDPSValue = 500 / 20 = 25 score per second
```

Summons with taunt, blocking, heal, or debuff effects should add UtilityScore.

---

## 16. Upgrade (Lv1~15)

Upgrade level and skill grade are different concepts.

- Skill Grade: Grade 1 / Grade 2 / Grade 3 skill version
- Upgrade Level: Shared growth from Lv1 to Lv15

Upgrade data is shared by all skill grades unless a skill explicitly needs a special table.

### Growth Target

| Level | Target Score |
|------:|-------------:|
| 1 | 100 |
| 5 | 140 |
| 10 | 215 |
| 15 | 315 |

This means a Lv15 skill should feel roughly `3.15x` stronger than the Lv1 baseline, including damage, range, cooldown, hit count, and utility.

### Upgrade Budget Ratio

| Category | Budget Ratio |
|----------|-------------:|
| Damage | 45% |
| Cooldown | 15% |
| Hit Range | 10% |
| Hit Count | 10% |
| Projectile Behavior | 10% |
| Buff / Debuff / Utility | 10% |

### Upgrade Description Style

Planning JSON should use readable descriptions.

Good:

```json
{
  "level": 2,
  "description": [
    "기본 데미지 +1",
    "공격력 계수 +5%"
  ]
}
```

Avoid implementation-style data in planning JSON unless explicitly requested.

---

## 17. Skill Generation Workflow

When creating a new skill, follow this order.

```text
1. Decide skill grade.
2. Decide skill category.
3. Select target score.
4. Use Expected Attack = 20 unless specified otherwise.
5. Choose BaseDamage and AttackPercentDamage.
6. Calculate TotalDamage.
7. Decide cooldown from target DPS.
8. Decide hitCount.
9. Decide castRange and hitRange.
10. Apply AreaFactor.
11. Add projectile, summon, buff, debuff, heal, shield, or CC value.
12. Calculate final BalanceScore.
13. Compare final BalanceScore with target score.
14. Generate Lv2~Lv15 upgrade descriptions.
15. Convert planning JSON into implementation JSON.
```

Do not generate final implementation data before completing the balance calculation.

---

## 18. Checklist

Before accepting a new skill design, check the following.

```text
[ ] Does it use Expected Attack = 20?
[ ] Does DPS 10 equal Score 100?
[ ] Is Basic Grade1 close to Score 100?
[ ] Is the target score appropriate for the skill slot?
[ ] Are BaseDamage and AttackPercentDamage both reasonable?
[ ] Is cooldown calculated from target DPS?
[ ] Is hitCount included in score calculation?
[ ] Is hitRange included through AreaFactor?
[ ] Are projectile behaviors converted into ProjectileFactor?
[ ] Are buffs, debuffs, heals, shields, summons, and CC converted into UtilityScore?
[ ] Does Lv1~Lv15 upgrade growth follow the target score curve?
[ ] Are planning descriptions readable and not overly implementation-specific?
```

If the answer is unclear, revise the skill before implementation.