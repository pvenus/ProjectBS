# Seojin Dual Direction HUD Size ×2

- Status: STATIC_PASS / UNITY_RUNTIME_QA_PENDING
- Scope: presentation-only footprint of `SeojinDirectionGround`
- Change: `FootprintScale = 2f`
- Preserved: red radius `1.15`, blue radius `1.0`, canonical rotation, center transparency, sorting, hide/input policies
- Unchanged: source PNGs, GUIDs, PPU, pivots, selection aura scale, character root/body/collider
- Validation: source contract assertions added; `git diff --check` passed
- Runtime QA: existing Unity visual confirmation pending; Unity/headless was not launched
- Rollback: remove `FootprintScale`, remove both multiplications, and remove the focused test method
