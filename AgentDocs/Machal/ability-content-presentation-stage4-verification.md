# Stage 4: Effect Config Mapping and Verification

## Status

- Implementation: completed on 2026-08-11
- Production mapping: all thirteen current `EffectConfig` classes dispatch through the single `EffectPresentationResolver`
- Previous Unity verification: thirteen synthetic mappings and twenty approved `EffectEntrySO` assets passed before the source-fidelity correction
- Current Unity verification: pending user rerun after semantic regrouping and special-effect correction
- Next action: user reruns the fifteen-case Effect self-test, Skill asset validation, and configured UI presentation

## Implemented Scope

- `StatModifierEffectConfig` -> `StatModifier`
- `ChanceOnHitStatModifierEffectConfig` -> `Activation(OnHit) + StatModifier`
- `OnHitTimedStatModifierEffectConfig` -> `Activation(OnHit) + StatModifier(duration)`, except `StunDuration` and `RootDuration`, which normalize as `Control(Stun/Root + duration)`
- `ChanceOnHealStatModifierEffectConfig` -> `Activation(OnHeal + Target) + StatModifier`
- `HealEffectConfig` -> `Heal`
- `ChanceOnHealCooldownReduceEffectConfig` -> `Activation(OnHeal + Target) + CooldownChange`
- `CooldownReduceEffectConfig` -> `CooldownChange`
- `KnockbackEffectConfig` -> `Displacement(Force)`
- `OnHitKnockbackDistanceEffectConfig` -> `Activation(OnHit) + Displacement(Meters)`
- `AttackBleedEffectConfig` -> `Activation(OnAttack) + PeriodicDamage(PerSecond)`
- `OnHitPoisonDotEffectConfig` -> `Activation(OnHit) + PeriodicDamage(PerTick)`
- `ChanceOnHitSkillEffectConfig` -> `Activation(OnHit + Critical condition) + SkillInvoke`
- `TauntEffectConfig` -> `Control(Taunt, duration)`

Probability values preserve the authored field's numeric representation and unit: `ChancePercent` remains a `Percent`, while ratio-based `Chance` remains a `Ratio`. Formatting may display a Ratio as a percentage, but the normalized numeric value is not clamped, multiplied, or replaced. The same rule applies to other authored numeric fields. Runtime-resolved values are allowed only when they come from a runtime source and carry runtime provenance.

## Runtime-Accuracy Boundaries

- `ChanceOnHealStatModifierEffectRuntime` currently ignores `ValueType` and passes `Value` directly to `AddStat`; Presentation therefore reports the active operation as Flat.
- `HealEffectConfig.ClampToMaxHp` is not read by `HealEffectRuntime`, while `CharacterDamageService.Heal` always clamps to maximum HP; Presentation reports the actual clamp behavior as true and does not expose the unused Config flag.
- `ChanceOnHitSkillEffectConfig.RangeOverride` is not used by the current runtime and remains excluded.
- `ChanceOnHitSkillEffectConfig.RequireCriticalHit` is preserved in Presentation because the runtime checks it. However, `EffectManager` currently passes `true` to the callback instead of the real hit result; actual critical-only behavior needs a separate gameplay fix.
- `OnHitKnockbackDistanceEffectRuntime` only distinguishes Pull from all other direction values; Presentation maps non-Pull values to PushAwayFromSource.
- `ChanceOnHitStatModifierEffectConfig` with Multiply falls back instead of showing an active value because the current runtime applies zero for that operation.
- `OnHitTimedStatModifierEffectConfig` targeting `StunDuration` or `RootDuration` follows the runtime max-set timer behavior and normalizes as `Control`. The control duration is `config.Value`; `config.DurationSeconds` is not substituted for it.
- A `ChanceOnHitSkillEffectConfig` without a Skill reference falls back instead of returning a supported empty invocation.
- `EffectEntrySO.ValueOverride`, upgrade modifiers, and derived periodic application counts remain excluded.

## User Test

Run the full mapping check in Unity:

1. Open `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`.
2. Confirm one Console log named `[EffectPresentationStage4SelfTest] PASS`.
3. The details must report `Synthetic mapping cases: 15` and `Approved EffectEntry assets: 20`. The 15 cases cover 13 Config classes, with separate Stun and Root specializations of `OnHitTimedStatModifierEffectConfig`.

Inspect one real entry:

1. Select an `EffectEntrySO` asset under `Assets/Resources/skill/character/generated/` or `Assets/Resources/skill/json/`.
2. Run `Assets > ProjectBS > Presentation > Log Selected Effect Entry`.
3. Inspect the typed Status, Activation, Outcome, and Constraints fields in the Console.

This menu tests the data layer only. Run the configured `Build Presentation` flow separately for player-facing UI verification.

## Verification Evidence

- `dotnet build Assembly-CSharp.csproj --no-restore`: 0 errors; 35 pre-existing warnings.
- Unity regenerated `Library/ScriptAssemblies/Assembly-CSharp.dll` at `2026-08-11 03:05:25`.
- Previous Unity self-test result: PASS, thirteen synthetic mappings, twenty approved entries. This result predates the source-fidelity correction and must not be treated as current verification.
- Latest static Editor assembly build after the semantic-grouping correction: 0 errors and 156 existing warnings.
- Current Unity rerun: pending user execution.
- No Config, SO asset, legacy asset, prefab, Scene, or gameplay runtime behavior was modified.
