# Seojin G1 Basic comboIndex2 bind correction

- Status: STATIC_IMPLEMENTATION_PASS / UNITY_RUNTIME_PENDING
- Scope: `skill.character.seojin.1.basic_attack.basic_attack` only
- Requested bind duration: `0.100` gameplay seconds

## Implementation

- Reused the existing stable effect ID `skill.character.seojin.1.basic_attack.basic_attack.combo.2.effect.debuff.2`; no duplicate EffectSO or EffectEntrySO was created.
- Canonical JSON `RootDuration` Flat value changed from `0.2` to `0.1`.
- The corresponding materialized EffectSO value changed from `0.2` to `0.1`.
- Existing comboIndex2 HitSO -> EffectEntrySO -> EffectSO GUID/fileID linkage is unchanged.
- EffectEntry remains Instant, Debuff, `maxApplyCount: 1`; therefore it is resolved only for a valid hit and does not apply on miss/cancel/invalid target.
- comboIndex0/1 contain no `RootDuration`. G2/G3 retain their existing `0.2` values and are outside this correction.

## Preserved

- Hit cadence `.160/.640/.940`, completion `1.060`, cooldown policy, damage, knockback, VFX, body animation, and all existing GUID/fileID values.
- Runtime root semantics remain `RootDuration > 0`; the status tick service decrements the duration and existing death/disable lifecycle owns cleanup.

## Validation

- Added focused contract coverage for one G1 RootDuration entry, stable ID, JSON/SO `0.1` readback, Instant/max-one entry semantics, and unchanged G2/G3 values.
- `Assembly-CSharp`: 0 errors.
- `Assembly-CSharp-Editor`: 0 errors.
- `git diff --check`: PASS.
- Unity runtime/Inspector reimport was not invoked; existing-session runtime confirmation remains pending.

## Rollback

- Restore the two changed numeric values from `0.1` to `0.2` and remove the added focused test method. No asset identity or reference rollback is required.
