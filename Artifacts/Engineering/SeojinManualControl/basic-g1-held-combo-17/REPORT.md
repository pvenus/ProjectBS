# Seojin G1 held-input timed combo correction

- Status: STATIC_IMPLEMENTATION_PASS / UNITY_RUNTIME_PENDING
- Supersedes: the previous `held-no-auto-chain` input policy only

## Behavior

- A fresh left-button press starts one Basic step.
- While that step is busy/recovering, held input creates at most one coalesced continuation intent; frame-by-frame enqueue/spawn is impossible because `basicPressPending` is a single boolean slot.
- Recovery completion consumes the one intent and advances through the existing input-driven progression `0→1→2`.
- A released short click does not synthesize another intent. A later press within the serialized one-second window advances; after expiry it resets to step 0.
- After step 2, a pending held request is discarded while cooldown is unavailable. If the button remains held, a fresh step-0 intent is created only when the ordinary readiness gate becomes true.
- Active skill priority, Charge/Dash shortcuts, forced-state cancellation, and timed-combo reset semantics are unchanged.

## Preserved contracts

- G1 JSON/SO `inputDriven`, comboIndex0/1 `nextComboActivationTime=1`, segment mapping `0..5/6..11/12..17`, canonical body scale 1, hit3 RootDuration `.5`, VFX `0/0/1`, damage, knockback, direction, and cooldown ownership.
- G2/G3, NPCs, images, clips, materials, GUIDs, and fileIDs unchanged.

## Validation

- Manual input/core harness: 56/56 PASS.
- Explicit cases cover held continuation, no busy-frame spam, one recovery buffer, release behavior, cooldown-ready restart, active-skill priority, and forced-state cleanup.
- Assembly-CSharp errors 0; Assembly-CSharp-Editor errors 0.
- JSON parse and `git diff --check`: PASS.
- Unity was not launched or modified; existing-session feel verification remains pending.

## Rollback

- Revert the held coalescing/readiness blocks in `ManualControlCore.cs` and the corresponding harness/contract assertions. No data or asset rollback is required.
