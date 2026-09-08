# Seojin G1 input-driven segment body scale rootfix

- Status: STATIC_IMPLEMENTATION_PASS / UNITY_RUNTIME_PENDING
- Root cause: the new input-driven segment player passed each step's legacy `bodyPresentationCalibration`; those profiles contain uniform scales from approximately `1.844828` through `2.29064`, so the canonical continuous18 body was enlarged again.

## Correction

- Removed `bodyPresentationCalibration` from all three G1 Basic combo steps in canonical JSON.
- Set all three corresponding materialized EquipmentSkillSO object references to null (`fileID: 0`).
- The JSON generator already resolves a missing calibration ID to null, so repeated materialization preserves this result.
- Runtime `BeginComboPresentation` explicitly uses `Vector3.one` when calibration is null; effective presentation local scale is therefore canonical `1,1,1`.
- Legacy calibration assets were retained because G2/G3 still reference them. No asset or GUID was deleted.

## Preserved

- Segments `0..5 / 6..11 / 12..17`, input-driven local timing, one-second continuation windows, hit3 RootDuration `.5`, VFX `0/0/1`, damage, knockback, cooldown, and direction.

## Validation

- G1 JSON body calibration IDs: 0/3.
- G1 materialized SO body calibration references: three `fileID: 0`.
- Focused regression test now requires null calibration for all G1 steps while retaining the legacy refs for G2/G3.
- JSON parse PASS; manual control harness 56/56 PASS.
- Assembly-CSharp errors 0; Assembly-CSharp-Editor errors 0.
- `git diff --check` PASS.
- Unity was not launched or modified; existing-session visual scale confirmation remains pending.

## Rollback

- Restore the three prior G1 JSON calibration IDs and their three deterministic SO GUID references (`d200...0000`, `d200...0001`, `d200...0002`). No asset recovery is required.
