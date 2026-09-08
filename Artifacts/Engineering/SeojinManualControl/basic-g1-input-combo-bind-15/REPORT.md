# Seojin G1 input-driven Basic combo + hit3 bind

- Status: STATIC_IMPLEMENTATION_PASS / UNITY_RUNTIME_PENDING
- Scope: `skill.character.seojin.1.basic_attack.basic_attack` only
- Supersedes: automatic held-input 1→2→3 execution for this G1 Basic

## Result

- One fresh Basic press executes exactly one combo step.
- A second press within the recovery-complete continuation window advances 1→2 or 2→3.
- `nextComboActivationTime` is serialized as `1.000` for comboIndex0 and comboIndex1; comboIndex2 is `0` and always resets the next chain to step 1.
- Boundary policy is deterministic: `Time.time <= continuationExpiresAt` continues; a later press resets to comboIndex0.
- A press during active recovery is buffered once. Held input alone never advances. A cooldown-rejected press is discarded and cannot fire late.
- Cancel, death, disable, CC/manual teardown call the existing combo cancellation path, which now also clears timed progress.

## Local timing

The existing absolute authoring data is preserved. Input-driven playback derives local timing from each step's authored start:

- hit1: contact `.160`, recovery complete `.280`
- hit2: contact `.160`, recovery complete `.200`
- hit3: contact `.160`, recovery complete `.280`

The continuous18 body clip is sampled only within the admitted step's segment: `0..5`, `6..11`, or `12..17`. One input cannot play all 18 frames. VFX remains `0/0/1` through the existing suppression argument.

## Cooldown decision

- comboIndex0/1 use only the continuation window, so the one-second Basic cooldown cannot starve continuation.
- G1 Basic cooldown starts once when comboIndex2 is admitted. If the chain never reaches index2, no cooldown is committed. After index2 admission it is not refunded by later target loss.

## Hit3 bind

- Reused stable effect ID `skill.character.seojin.1.basic_attack.basic_attack.combo.2.effect.debuff.2`.
- Canonical JSON and materialized EffectSO `RootDuration` are `0.500` seconds.
- comboIndex0/1 have no RootDuration; miss/cancel/invalid targets do not resolve the hit EffectEntry.
- Existing Instant/Debuff/maxApply1 entry and GUID/fileID chain are unchanged.

## Serialization and compatibility

- Added additive `SkillComboProfile.inputDriven` and `SkillComboStep.nextComboActivationTime` fields.
- JSON generator materializes both fields through `SerializedObject`.
- Missing fields default to `false/0`, retaining the legacy automatic combo path for G2/G3 and existing skills.

## Validation

- Manual input/core harness: 56/56 PASS, including held-no-repeat, recovery buffer single-consume, cooldown no-late-fire, cancel/forced-state cleanup, and skill priority.
- Focused editor contract covers JSON/SO bind readback, G1 input-driven policy, exact windows, local timing, held-edge policy, and segment-only playback.
- JSON parse: PASS.
- Assembly-CSharp: 0 errors; Assembly-CSharp-Editor: 0 errors.
- `git diff --check`: PASS.
- Unity was not launched or manipulated; runtime feel/Inspector materialization remains pending in the existing user session.

## Rollback

- Restore G1 JSON/SO bind `0.5→0.1`, remove the G1 `inputDriven`/window fields, and revert the additive runtime/generator/input/segment-playback changes listed in this receipt. No PNG, clip, material, GUID, fileID, G2/G3, Charge, NPC, damage, or knockback asset requires rollback.
