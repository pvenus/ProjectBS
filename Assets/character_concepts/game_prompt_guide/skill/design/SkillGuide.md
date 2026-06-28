

# Skill Planning Guide

## Unit Scale

`1` means `1 Unity unit`.

All range values in planning JSON use Unity unit scale.

---

## castRange and hitRange

`castRange` and `hitRange` are separate concepts.

- `castRange` means how far the skill can be used from the caster.
- `hitRange` means the size of the damage area.
- `hitRange` is interpreted as radius.

Because `hitRange` is radius-based, a large hitRange can cover behind the caster as well.

For example, if `hitRange` is `0.5`, the damage area extends `0.5` units in every direction from its center. If `castRange` is also too large, the skill may feel like it hits farther than intended for a melee attack.

For a natural melee feel, `castRange` should usually be smaller than `hitRange`.

Example:

```json
{
  "castRange": 0.3,
  "hitRange": 0.5
}
```

This means the caster only reaches slightly forward, while the hit area itself has a 0.5 unit radius.

---

## Melee Range Guidelines

These are guidelines, not fixed rules.

The important point is the relationship between `castRange` and `hitRange`.

| Melee Type | castRange | hitRange | Notes |
|------------|-----------|----------|-------|
| Close melee | 0.2 - 0.3 | 0.4 - 0.5 | Short weapon or body-close attack |
| Standard melee | 0.3 - 0.5 | 0.5 - 0.7 | Normal sword-range attack |
| Extended melee | 0.5 - 0.8 | 0.6 - 0.9 | Spear-like or slightly longer melee attack |

---

## Relationship Guide

Use this as the main rule of thumb.

```text
castRange <= hitRange
```

For most melee attacks, `castRange` should not exceed `hitRange`.

When `hitRange` is `0.5`, a natural melee `castRange` is around `0.3`.

When the weapon should feel longer, increase `castRange` slightly while keeping `hitRange` close to the actual hit size.

Example:

```json
{
  "castRange": 0.6,
  "hitRange": 0.7
}
```

This is suitable for a spear-like melee attack.

---

## Cooldown Guidelines

Cooldown guidelines depend on who uses the skill.

### Repeating Attacks

Repeating attacks include basic attacks or simple attacks used frequently.

| User Type | Fast | Standard | Slow | Notes |
|-----------|------|----------|------|-------|
| PC | 1s | 2s | 3s | PC repeated attacks should feel responsive |
| NPC | 1s | 3s | 5s | NPC attacks can use a wider range for pattern variety |

PC repeated attacks use `2s` as the standard cooldown.

NPC repeated attacks use `3s` as the standard cooldown.

NPC skills may use a wider `1s - 5s` cooldown range to create varied combat patterns.

---

## Active Skill Cooldown and DPS

Active skills should not use cooldown alone as the starting point.

Active skill cooldown should be decided from target DPS.

Use this rule:

```text
cooldown = totalDamage / targetDps
```

Example:

```text
targetDps = 100
totalDamage = 1000
cooldown = 1000 / 100 = 10s
```

This means a skill that deals `1000` total damage should have around `10s` cooldown when the target DPS is `100`.

---

## Damage Value Guidelines

Skill damage is usually composed of two parts.

```text
totalDamage = baseDamage + attackPower × percentDamage
```

- `baseDamage` is fixed damage.
- `percentDamage` scales with the user's attack power.

### Base Damage

`baseDamage` has stable value regardless of character stats.

It is useful when the skill should have guaranteed minimum damage.

High `baseDamage` is stronger in early stages or on low-attack characters.

### Percent Damage

`percentDamage` becomes more valuable as the user's attack power increases.

It is useful when the skill should scale with character growth.

High `percentDamage` is stronger in late stages or on high-attack characters.

### Value Relationship

The value of `percentDamage` depends on expected attack power.

```text
percentDamageValue = expectedAttackPower × percentDamage
```

Example:

```text
expectedAttackPower = 100
percentDamage = 100%
percentDamageValue = 100
```

In this case, `100% attack damage` has the same expected damage value as `baseDamage 100`.

### Design Rule

When comparing damage values, convert percent damage into expected damage first.

```text
expectedDamage = baseDamage + expectedAttackPower × percentDamage
```

Then use `expectedDamage` for DPS and balance score calculations.

### Practical Guideline

| Damage Type | Best Use |
|-------------|----------|
| High baseDamage | Early-game power, low-stat characters, reliable damage |
| High percentDamage | Scaling skills, late-game growth, attack-focused characters |
| Mixed damage | General-purpose skills |

Do not balance `baseDamage` and `percentDamage` as if they have the same value.

The same percent value can be weak or strong depending on expected attack power.

---

## Hit Count Guidelines

`hitCount` defines the maximum number of enemies that can be damaged by a single hit.

### Recommended Values

| hitCount | Description |
|----------|-------------|
| 1 | Single target |
| 2 ~ 10 | Limited penetration |
| 999 | Unlimited penetration |

Examples

```json
{
  "hitCount": 1
}
```

Single target attack.

```json
{
  "hitCount": 3
}
```

Can damage up to three enemies.

```json
{
  "hitCount": 999
}
```

Unlimited penetration.

Use `999` only for intentionally designed piercing or full AoE skills.

---

## Balance Score Guidelines

Skill balance should not be determined by DPS alone.

The effective value of a skill increases as both the number of targets and the attack area increase.

### Base Score

```text
BaseScore = DPS × HitCount
```

Examples

| DPS | HitCount | BaseScore |
|-----|----------|----------:|
|100|1|100|
|100|3|300|
|200|1|200|
|200|5|1000|

This is only the first approximation.

---

## Area Scaling

`hitRange` also increases the practical value of a skill.

A larger attack area naturally hits more enemies even when `hitCount` is unchanged.

For this reason, `hitRange` should also be considered when balancing a skill.

Conceptually,

```text
BalanceScore
    = DPS
    × HitCount
    × AreaFactor
```

Where

- DPS : Expected sustained damage
- HitCount : Maximum number of targets
- AreaFactor : Value derived from hitRange

Example

```text
DPS = 100
HitCount = 3
AreaFactor = 1.5

BalanceScore = 450
```

The exact `AreaFactor` formula is intentionally undefined.

Designers should evaluate hit count and hit area together instead of balancing them independently.

---

## Balancing Priority

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

### Active Skill Tiers

`active1`, `active2`, and `active3` should each have a target DPS.

The cooldown is then adjusted to reach that target DPS.

| Skill Slot | Design Role | Cooldown Rule |
|------------|-------------|---------------|
| active1 | Low-impact frequent active | Balance around active1 target DPS |
| active2 | Medium-impact active | Balance around active2 target DPS |
| active3 | High-impact active | Balance around active3 target DPS |

Do not assign active cooldown only by feeling.

First decide:

```text
1. targetDps
2. expected total damage
3. cooldown needed to match targetDps
```

Then adjust the final cooldown for gameplay feel.

## Notes

- Do not treat `castRange` as the full damage range.
- Do not treat `hitRange` as skill use distance.
- `hitRange` is the damage area radius.
- `castRange` is the skill reach from the caster.
- For melee attacks, tune both values together.