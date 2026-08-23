# Stage 3: Effect Normalization Model

## Status

- Design preparation: complete
- Script implementation: completed on 2026-08-11
- Next stage: Stage 4 Config mapping branches
- Prerequisite: satisfied; Stage 2 shared contracts and the required code-writing guide are available

## Objective

Stage 3 introduced the smallest typed Effect normalization model and one `EffectPresentationResolver` entrypoint. It does not implement the Config mappings; those branches are Stage 4.

## Initial File Scope

Start flat and small:

```text
Assets/Scripts/Ability/Effects/
  Data/
    EffectPresentationData.cs
  EffectPresentationResolver.cs
```

Keep related Effect presentation types in `EffectPresentationData.cs` initially. Split files only after the model becomes difficult to navigate or changes independently.

Do not add:

- `IEffectConfigPresentationResolver`
- Config-specific resolver classes
- a feature-only `Resolvers/` folder
- Presentation conversion methods to `EffectConfig`

## Stage 2 Dependencies

Stage 3 expects the shared layer to provide:

- `PresentationIdentityData`
- `PresentationContext`
- `PresentationProvenanceData`
- `PresentationValueData`
- `PresentationEntryData`
- `PresentationGroupData`
- `ContentPresentationData`

All seven contracts are now implemented under `Assets/Scripts/Presentation/` and passed the Stage 2 smoke validation.

The Effect model may retain typed domain data before the later grouping layer converts it into shared groups.

## Effect Data Model

`EffectPresentationData` contains:

- Shared identity for the source `EffectSO`
- Authored description when available
- Optional `EffectActivationPresentationData`
- Exactly one `EffectOutcomePresentationData`, unless the result is description-only or unsupported
- Entry-owned application constraints
- Shared provenance
- Explicit supported, description-only, or unsupported status

`EffectActivationPresentationData` contains only activation conditions:

- Trigger: None, OnHit, OnHeal, or OnAttack
- Authored chance number and source unit when present
- Heal target condition when present
- Critical-hit requirement when present

`EffectEntryConstraintPresentationData` contains runtime-relevant entry data:

- Buff or Debuff category
- Lifetime kind
- Duration only when the runtime uses the entry duration
- Max apply count from the real `EffectEntrySO.MaxApplyCount` field

Never derive apply count from interval and duration.

## Outcome Types

Keep one typed outcome base with these meaningful payload types in the same initial file:

- `StatModifierPresentationData`
- `HealPresentationData`
- `CooldownChangePresentationData`
- `DisplacementPresentationData`
- `PeriodicDamagePresentationData`
- `SkillInvokePresentationData`
- `ControlPresentationData`

These types represent normalized meaning, not raw Config layouts and not final UI strings.

### Activation and Outcome Axes

Activation and outcome answer different questions and must not be merged:

- Activation answers when the Effect starts: `OnHit`, `OnHeal`, or `OnAttack`.
- `Heal` stores healing amounts and clamp behavior.
- `PeriodicDamage` stores damage scaling, rate unit, interval, and duration.
- Source values remain explicit typed fields and are not combined into invented numeric values.

Therefore `OnHit` and `OnHeal` remain separate triggers, while `Heal` and `PeriodicDamage` remain separate outcome types under the approved seven-outcome contract.

## Single Resolver Surface

Initial public surface:

```text
EffectPresentationData Resolve(
    EffectEntrySO entry,
    PresentationContext context)
```

Resolution flow:

```text
validate entry
-> read EffectSO and entry constraints
-> switch on EffectSO.Config
-> build optional Activation
-> build one outcome
-> attach provenance
-> return EffectPresentationData
```

Stage 3 establishes the reachable method and unsupported/fallback behavior. Stage 4 fills the thirteen Config branches.

## Validation and Fallback

- Null `EffectEntrySO`: unsupported result with no invented text
- Null `EffectSO`: unsupported result with entry provenance when possible
- Null Config: use authored Effect description only when non-empty; otherwise unsupported
- Unknown Config: same description-only fallback rule
- `EffectEntrySO.ValueOverride`: record neither as active value nor as applied provenance because runtime does not apply it
- Effect upgrade modifiers: do not apply or display as resolved values

The ambiguous legacy `SkillEffectSO` belongs to the Skill domain. Its description-only fallback will be handled during Stage 5 Skill composition instead of creating an Ability Effects-to-Skills dependency.

## Stage 4 Branch Order

Implement branches in independently verified batches:

1. Current linked-asset coverage:
   - `StatModifierEffectConfig`
   - `HealEffectConfig`
   - `CooldownReduceEffectConfig`
   - `KnockbackEffectConfig`
   - `TauntEffectConfig`
2. Triggered StatModifier and cooldown mappings:
   - `ChanceOnHitStatModifierEffectConfig`
   - `OnHitTimedStatModifierEffectConfig`
   - `ChanceOnHealStatModifierEffectConfig`
   - `ChanceOnHealCooldownReduceEffectConfig`
3. Periodic damage, distance displacement, and skill invocation:
   - `AttackBleedEffectConfig`
   - `OnHitPoisonDotEffectConfig`
   - `OnHitKnockbackDistanceEffectConfig`
   - `ChanceOnHitSkillEffectConfig`

Private helpers are grouped by normalized outcome only when construction repeats.

## Unit and Provenance Rules to Carry Into Stage 4

- Preserve the authored source number, source unit, and provenance explicitly.
- Do not convert chance fields into one canonical numeric representation. Keep Ratio and Percent data distinct; a formatter may change only the rendered text.
- Keep flat values, ratios, percentages, seconds, meters, force, and counts distinguishable.
- Taunt duration comes from `EffectEntrySO.Duration`.
- Periodic application count is never calculated from duration and interval.
- JSON-only declarations use authoring provenance and are not active runtime results.

## Stage 3 Verification Results

- A temporary placeholder entrypoint was reached and logged before final fallback construction replaced it; no production placeholder remains.
- The isolated smoke result was `STAGE3_EFFECT_PRESENTATION_SMOKE_OK`.
- Smoke coverage includes null entry, null Effect, description-only fallback, unsupported fallback, provenance, timed and instant constraints, explicit Seconds and Count units, distinct OnHit and OnHeal triggers, and separate Heal and PeriodicDamage outcomes.
- The Effect data source references only its owning Ability Effects types and the shared Presentation contracts; the shared contracts retain no concrete SO dependency.
- No Config or asset was mutated.
- Unity regenerated `Assembly-CSharp.dll` with `EffectPresentationData.cs` and `EffectPresentationResolver.cs` included.
- Asset-backed Config mapping coverage remains a separate Stage 4 responsibility.

## Exit Criteria

Stage 3 is complete when:

- The typed Effect model compiles.
- One public resolver entrypoint is reachable.
- Fallback and unsupported results are deterministic.
- Provenance and entry constraints are carried without inventing values.
- No Config-specific resolver hierarchy exists.
- Stage 4 can add one switch branch at a time without changing the public contract.

All exit criteria were satisfied on 2026-08-11.
