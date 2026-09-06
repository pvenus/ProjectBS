# Seojin Basic 3-hit visual candidate handoff

Status: `COMPLETE54_CANDIDATE_ONLY_ATOMIC_HANDOFF`

- New body registry: shared hit0/hit1/hit2, 6 frames each, total 18.
- New VFX registry: G1/G2/G3 × fresh hit1/hit2 × 6 frames, total 36.
- Existing accepted Basic VFX exact18 remains combo hit0 and is not duplicated here.
- All new frames are 256×256 native RGBA, pivot `(0.5, 0.5)`, loop off.
- Physical validation: 54/54 RGBA; 54/54 unique bytes; four-border alpha zero 54/54.
- Small disconnected neighbor fragments touching strip cut edges were mechanically excluded; principal connected subjects were not repainted or regenerated.
- Timing remains runtime-owned by `comboSteps`; frame semantics are anticipation/onset/build/contact/decay/recovery.
- This package does not modify canonical assets, clips, Unity, gameplay, or project bindings.

Manifest and per-frame source/output hashes are in `manifest.json`.
