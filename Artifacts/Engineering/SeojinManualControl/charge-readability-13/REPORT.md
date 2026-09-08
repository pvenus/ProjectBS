# Seojin Charge neon readability rootfix

- Status: STATIC_PASS / UNITY_IMPORT_AND_RUNTIME_VISUAL_QA_PENDING

## Root cause

All three Charge `BaseVisualSO`s referenced the shared `skillAnimationVfx.impactAttack.seojinValidation.v1` profile. Its alpha envelope was `fadeIn .07 + hold .06 + fadeOut .21 = .34s`, while the Charge clip loop contract is `.8333333s`. It also applied tint `.10`, color shift `.16`, ink density `.42`, palette remap and the generic impact material to source-authored neon frames.

## Narrow correction

- Added Charge-only profile `skillAnimationVfx.seojinChargeHoverNeonReadable.v1`.
- Added Charge-only material using `Custom/SkillAnimationVfxSourceReadable`.
- Bound only the G1/G2/G3 Charge `BaseVisualSO`s.
- Added `baseVisual.animationVfxProfile` to JSON/builder as an optional explicit binding. Omission preserves legacy serialized profiles; missing or duplicate explicit IDs fail closed.
- New alpha envelope: `.03 + .77 + .04 = .84s`, covering the `.8333333s` loop.
- Source preservation settings: tint `0`, color shift `0`, desaturate `0`, ink `0`, alpha clip `0`.
- Bounded lift: body opacity gain `.40` (monotonic source-alpha lift), emission `.06`, outline alpha `.18`.

## Preserved

- revision-02 PNG exact18 and all PNG meta/GUID/fileIDs
- clip keys `0/.07/.16/.40/.66/.75`, stop contract `.8333333`, loop1
- G1/G2/G3 source footprint and luminance lineage
- MoveSO direction rotation, renderer-only direction wrapper and WASD snapshot
- body/gameplay/collider/cooldown/distance/speed
- shared shader/material/profile and every non-Charge visual

## Pool safety

`SkillAnimationVfxControllerMono.ResetState` clears and reapplies an empty MPB. `SkillAnimationVfxFeatureObject.OnDisable/OnDestroy` calls `StopImmediate`, and `ProjectileVisual` restores its captured baseline shared material before applying the next spawn.

## Validation

- JSON3 and BaseVisualSO3 exact profile readback: PASS
- dedicated material/profile GUID and fileID linkage: PASS
- Assembly-CSharp compile: errors0
- Assembly-CSharp-Editor compile: errors0
- focused source/pool contract tests compile: PASS
- `git diff --check`: PASS
- Unity GUI/import/Play: not invoked

## Rollback

Restore the three BaseVisual profile references to GUID `c2d35d2f73b645a4ad5b18b4d7aebf52`; remove the three JSON `animationVfxProfile` values, the two dedicated assets/metas, the optional builder/editor setter, and the focused Charge assertions.
