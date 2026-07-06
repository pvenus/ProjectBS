# Skill Balance Guide

## 1. Purpose

This document defines the main planning and balance rules used to design skills, buff effects, debuff effects, summon effects, and upgrade data.

The numbers in this document are design guidelines, not final implementation limits.

When generating a new skill, use this guide first, then convert the result into implementation JSON.

---

## 2. Unit Scale

`1` means `1 Unity unit`.

All range values in planning JSON use Unity unit scale.

---

## 3. Golden Rule

All skill balance starts from the basic attack baseline.

```text
Expected Attack = 20
Base Damage = 0
Attack Percent Damage = 50%
Total Damage = 0 + 20 x 50% = 10
Attack Speed Scale = 1.0
Cooldown = 1s
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

## 4. Core Formula

### Total Damage

```text
TotalDamage = BaseDamage + ExpectedAttack × AttackPercentDamage
```

`AttackPercentDamage` is written as percent in planning documents.

Example:

```text
ExpectedAttack = 20
BaseDamage = 0
AttackPercentDamage = 50%
TotalDamage = 0 + 20 x 50% = 10
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

## 5. Expected Stats

Use these values as the default early-game balance baseline.

| Category | Value | Notes |
|----------|------:|-------|
| Expected PC Attack | 20 | Default value for damage calculations |
| Basic Attack DPS | 10 | Equals Balance Score 100 |
| Basic Attack Cooldown | 1s at Attack Speed 1.0 | Cooldown scales from attack speed |
| Normal Monster HP | 30 - 60 | Dies in roughly 3 - 6 seconds to baseline DPS |
| Elite Monster HP | 150 - 250 | Requires sustained focus |
| Boss HP | 800+ | Depends on encounter length |
| Normal Monster Attack | 3 - 5 | Basic incoming damage |
| PC HP | 80 - 120 | Allows several hits before death |

These values should grow over time, but new low-grade skills should still be readable in one-digit or low two-digit damage ranges.

---

## 6. Skill Categories

| Category | Role | Baseline |
|----------|------|----------|
| Basic | Repeating basic attack | Score 100 |
| Active1 | Frequent low-impact active skill | Score 300 |
| Active2 | Main active skill | Score 500 |
| Active3 | High-impact active skill | Score 800 |
| Passive | Conditional or always-on support | Convert effect value into score |

Basic skills are balanced around repeated use.

### NPC Tier / Grade Slot Limits

The skill category table describes possible skill slots, not the number of skills every unit should receive.

For NPCs, choose the slot count from `stats.tierId` and `stats.grade` before selecting target scores.

| NPC tierId | Grade | Slot budget | Balance note |
|------------|------:|-------------|--------------|
| normal | 1 | Basic only | Keep score close to the Basic baseline. |
| normal | 2 | Basic + 1 identity skill | The identity skill should usually be Passive or low-impact Active1. |
| normal | 3 | Basic + 1-2 identity skills | Add a second identity skill only when the role needs it. |
| elite / leader | 2-3 | Basic + Active1 + Passive | Use Active2 only for explicitly high-impact elite patterns. |
| boss | any | Multiple active skills and passive/phase mechanics | Active2/Active3 are reserved for boss-like encounters. |

Normal NPCs should stay readable in group combat. A normal grade 2 NPC with `typeId: defense` should usually use `basic + passive`, not `basic + active1 + passive`.

Active skills are balanced around target score and target DPS.

Passive skills are balanced by estimating how much damage, defense, utility, or sustain they add over time.

---

## 7. Target Score

| Skill Slot | Target DPS | Target Score | Notes |
|------------|-----------:|-------------:|-------|
| Basic | 10 | 100 | Standard repeating attack |
| Active1 | 15 | 300 | Frequent active, can include utility |
| Active2 | 25 | 500 | Main active skill |
| Active3 | 40 | 800 | High-impact skill |

`Target DPS` is the damage-only baseline.

`Target Score` is the final total value after adding hit count, area value, projectile value, and utility value.

Active skills may include wide area, crowd control, buff, debuff, projectile behavior, or summon value.

When a skill has no utility and only hits a single target, its final score should stay close to the damage-only score.

Example:

```text
Active1 Target Score = 300
Damage Score = 180
Utility Score = 120
Final Score = 300
```

---

## 8. Damage

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
| Basic Grade1 | 0 - 5 | 25% - 50% |
| Basic Grade2 | 25 | 100% |
| Basic Grade3 | 46 | 100% |
| Active1 damage-only | 25 - 40 | 100% - 150% |
| Active2 damage-only | 50 - 80 | 150% - 250% |
| Active3 damage-only | 90+ | 250%+ |

These values assume single-target or small-area skills. Reduce damage when hit count, hit range, or utility is high.

Do not balance `BaseDamage` and `AttackPercentDamage` as if they have the same value.

The same percent value can be weak or strong depending on expected attack power.

---

## 9. Cooldown

Cooldown must scale from attack speed.

Attack speed uses `1.0` as the normal baseline. A character with attack speed `1.0` uses a basic or repeating skill every `1s`.

| Attack Speed Scale | Meaning | Basic / Repeating Cooldown |
|-------------------:|---------|---------------------------:|
| 0.50 | Very slow | 2.00s |
| 0.75 | Slow | 1.33s |
| 1.00 | Normal | 1.00s |
| 1.50 | Fast | 0.67s |
| 2.00 | Very fast | 0.50s |

Formula:

```text
RepeatingCooldown = 1 / AttackSpeedScale
```

For active skills, first choose a base pattern cooldown from the skill's intended rhythm, then scale it by attack speed:

```text
FinalActiveCooldown = BasePatternCooldown / AttackSpeedScale
```

Examples:

```text
AttackSpeedScale 1.0, BasicAttack = 1.00s
AttackSpeedScale 1.5, BasicAttack = 0.67s
AttackSpeedScale 0.75, BasicAttack = 1.33s
BasePatternCooldown 6s, AttackSpeedScale 1.5 => FinalActiveCooldown 4s
```

When cooldown becomes shorter because attack speed is higher, reduce per-hit damage if the skill must preserve the same target DPS or target score.

Do not use the older fixed `3s` basic attack baseline for new planning.

---

## 10. castRange and hitRange

`castRange` and `hitRange` are separate concepts.

- `castRange` means how far the skill can be used from the caster.
- `hitRange` means the size of the damage area.
- `hitRange` is interpreted as radius.
- The minimum `castRange` for any skill is `0.4`.
- Do not generate a skill with `castRange` below `0.4`, even for very short melee attacks.

Because `hitRange` is radius-based, a large hitRange can cover behind the caster as well.

For example, if `hitRange` is `0.5`, the damage area extends `0.5` units in every direction from its center. If `castRange` is also too large, the skill may feel like it hits farther than intended for a melee attack.

For most melee attacks:

```text
castRange <= hitRange
```

Example:

```json
{
  "castRange": 0.4,
  "hitRange": 0.5
}
```

This means the caster only reaches slightly forward, while the hit area itself has a 0.5 unit radius.

### Melee Range Guidelines

These are guidelines, not fixed rules.

The important point is the relationship between `castRange` and `hitRange`.

| Melee Type | castRange | hitRange | Notes |
|------------|----------:|---------:|-------|
| Close melee | 0.4 | 0.4 - 0.5 | Short weapon or body-close attack |
| Standard melee | 0.4 - 0.5 | 0.5 - 0.7 | Normal sword-range attack |
| Extended melee | 0.5 - 0.8 | 0.6 - 0.9 | Spear-like or slightly longer melee attack |

When the weapon should feel longer, increase `castRange` slightly while keeping `hitRange` close to the actual hit size.

Example:

```json
{
  "castRange": 0.6,
  "hitRange": 0.7
}
```

This is suitable for a spear-like melee attack.

### AreaFactor Guideline

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

## 11. HitCount

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

Use `999` only for intentionally designed piercing or full AoE skills.

---

## 12. Area Scaling

Skill balance should not be determined by DPS alone.

The effective value of a skill increases as both the number of targets and the attack area increase.

Conceptually:

```text
BalanceScore = DPS × HitCount × AreaFactor
```

Where:

- DPS: Expected sustained damage
- HitCount: Maximum number of targets
- AreaFactor: Value derived from hitRange

Example:

```text
DPS = 100
HitCount = 3
AreaFactor = 1.5
BalanceScore = 450
```

The exact `AreaFactor` formula can be adjusted later.

Designers should evaluate hit count and hit area together instead of balancing them independently.

---

## 13. Projectile

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

## 14. Buff / Debuff

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

## 15. Crowd Control

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

## 16. Heal / Shield

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

## 17. Summon

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

## 18. Upgrade (Lv1~15)

Upgrade level and skill grade are different concepts.

- Skill Grade: Grade 1 / Grade 2 / Grade 3 skill version
- Upgrade Level: Shared growth from Lv1 to Lv15

Upgrade data is shared by all skill grades unless a skill explicitly needs a special table.

### Upgrade Applicability

Upgrade data is for upgradeable player/equipment skills unless a document explicitly says an NPC skill can be upgraded.

Do not generate Lv1-Lv15 upgrade plans for normal NPC concept JSON. NPC difficulty should come from `tierId`, `grade`, stats, encounter composition, and a small number of readable skill behaviors.

### Growth Target

| Level | Target Score |
|------:|-------------:|
| 1 | 100 |
| 5 | 125 |
| 10 | 165 |
| 15 | 220 |

This means a Lv15 skill should feel roughly `2.2x` stronger than the Lv1 baseline, including damage, range, cooldown, hit count, and utility.

Because player skills also gain power through grade progression, upgrade growth should remain moderate.

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
    "Base damage +1",
    "Attack scaling +5%"
  ]
}
```

Avoid implementation-style data in planning JSON unless explicitly requested.

---

## 19. Balancing Priority

Balance skills in the following order.

```text
1. Target DPS
2. Hit Count
3. Hit Range
4. Cooldown
5. Additional Utility
```

If a skill has a higher `HitCount` or larger `hitRange`, it should generally compensate by reducing one or more of the following.

- DPS
- Cooldown efficiency
- Additional utility

---

## 20. Skill Generation Workflow

When creating a new skill, follow this order.

```text
1. Decide skill grade.
2. Decide skill category.
3. Select target score.
4. Use Expected Attack = 20 unless specified otherwise.
5. Choose BaseDamage and AttackPercentDamage.
6. Calculate TotalDamage.
7. Decide attack speed scale and cooldown from `1 / AttackSpeedScale` or active pattern cooldown scaling.
8. Decide hitCount.
9. Decide castRange and hitRange.
10. Apply AreaFactor.
11. Add projectile, summon, buff, debuff, heal, shield, or CC value.
12. Calculate final BalanceScore.
13. Compare final BalanceScore with target score.
14. Generate Lv2~Lv15 upgrade descriptions only for upgradeable player/equipment skills. Skip this for normal NPC concept JSON.
15. Convert planning JSON into implementation JSON.
```

Do not generate final implementation data before completing the balance calculation.

---

## 21. Checklist

Before accepting a new skill design, check the following.

```text
[ ] Does it use Expected Attack = 20?
[ ] Does DPS 10 equal Score 100?
[ ] Is Basic Grade1 close to Score 100?
[ ] Does the basic or repeating cooldown use `1 / AttackSpeedScale`?
[ ] Is the target score appropriate for the skill slot?
[ ] Are BaseDamage and AttackPercentDamage both reasonable?
[ ] Is cooldown calculated from target DPS?
[ ] Are castRange and hitRange treated separately?
[ ] For melee attacks, is castRange <= hitRange?
[ ] Is hitCount included in score calculation?
[ ] Is hitRange included through AreaFactor?
[ ] Are projectile behaviors converted into ProjectileFactor?
[ ] Are buffs, debuffs, heals, shields, summons, and CC converted into UtilityScore?
[ ] If this is an upgradeable player/equipment skill, does Lv1~Lv15 upgrade growth follow the target score curve?
[ ] If this is an NPC skill, did you avoid generating an upgrade plan unless explicitly required?
[ ] Are planning descriptions readable and not overly implementation-specific?
```

If the answer is unclear, revise the skill before implementation.

---

## 22. Notes

- Do not treat `castRange` as the full damage range.
- Do not treat `hitRange` as skill use distance.
- `hitRange` is the damage area radius.
- `castRange` is the skill reach from the caster.
- For melee attacks, tune both values together.
- Use `999` hitCount only when the skill is intentionally designed as unlimited penetration or full AoE.
