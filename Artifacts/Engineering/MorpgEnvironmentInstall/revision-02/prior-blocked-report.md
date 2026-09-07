# MORPG environment installation — activation blocked

Status: exact15 assets installed; authored runtime profile, renderer and movement geometry implemented; LIVE ROUTE INTEGRATION NOT APPLIED. This is not a completion receipt for live placement.

## Installed evidence

- Source manifest SHA d5353184175684b7affaa9a95933c7271e3dc65addabbdbfa912c85e0bfa66fb.
- Corrected contract SHA 20fd0dc578f51d8218e7e222faecf94f70fa3260c41bf27346756f7decfd75b8.
- Canonical asset root: Assets/Resources/battle/morpg/environment. asset-map.json records each source path/SHA → canonical path/GUID/resource reference.
- Exact15 byte-identical 1024×1024 RGBA sprites, unique GUIDs, PPU100, custom contact pivot, Bilinear/Clamp, mipmaps off, uncompressed; alpha48px border=0 and transparent RGB nonzero=0. No source-art edits.
- environment.v1.json authors Z1 eight saplings, Z2 four fences, Z3 four house edges + one wall; 11 nonblocking dressing instances use all remaining selected modules. Opaque bounds metadata controls visible scale; the right wall rotates 90 degrees. No collider is inferred from sprite alpha.
- MorpgEnvironmentRuntime creates separate visual and structural roots; foliage/canopy/decorations collider0; trunk circles radius .28, exact fence/house/wall boxes and outward closed polygon strips use the same geometry data. The runtime renderer compiles but has NOT been activated in game.
- All28 reservations audited; 20 conflicting positions deterministically relocated to the .05 grid with closest displacement then x/y tie-break. Stable IDs, roles22+6, due times and counts19/5/4 retained. Spawn centers satisfy eroded .65 polygon, blockers dilated .55, entry3.25, egress exclusion and separation .65.
- Geometry drives movement/displacement/knockback sweep (.15 + actor radius), camera clamp, landing checks and collision generation. Runtime transition enables destination structural root before placement and disables old root after placement. Presentation fade conservatively starts for any actor sprite overlap. None of these inactive runtime facilities change the current P2 route.

## Blocking contract contradictions

Production geometry tests reproduce exactly three failures:
1. Z3 +X capsule ends at x12.75, raw radius1.40; wall starts x14.00. Required right edge is x14.15, so the wall intersects it by .15wu. Even usable radius1.25 only touches the wall and leaves no actor clearance.
2. Top house gap = 10.00 − 8.50 = 1.50wu, below required2.50.
3. Bottom house gap is likewise1.50wu.

The instruction to preserve exact prop coordinates and move NPC spawns cannot resolve these contradictions. Spawn corrections are already applied; a revised geometry contract or an explicit supersession of the conflicting corridor/gap rules is required before activation. No silent geometry relaxation or false activation success was introduced.

The requested fallback also needs reconciliation: the existing asset named episode1.rescue_villagers.solo_large_wave.v2.asset currently declares policyId large_map.three_stage.v3, and BattleLargeWaveRunner accepts v3. Applying an environment rejection gate now would send the working P2 path to that existing fallback, not prove the requested exact-v2 fallback. The full route hookup is therefore saved as pending-route-integration.diff, apply-check passed, but NOT applied. BattleMorpgLiveRoute is byte-identical to this task's start snapshot. Do not apply that patch until the contract/fallback issues are resolved.

## Verification

- Installed Unity C# compiler: 0 errors, existing31 warnings.
- Production geometry tests8/8: exact inventory, all28 corrected spawns, explicit contract rejection, original overlap rejection, no-tunneling wall sweep, sloped boundary/radius sweep, landing/camera extrema, missing GUID/audit rejection. Passing the tests includes proving that current profile activation FAILS.
- Independent .10wu grid: all28 spawns connected to entry; no claim that Z3 minimum-width rules pass.
- Live P2/final settlement simulations18/18; delivery queue/ledger12/12; P0/P1 suite27/27. No Unity GUI or engine gameplay run.
- Native authored-layout-audit.png visually inspected; it is an offline data/art composition, not a runtime screenshot. Foreground occlusion timing, gameplay readability and transition collider popping remain engine-unverified.
- git diff --check passed. installed-changes.diff reverse --check and pending-route-integration.diff apply --check passed; neither operation executed. Rollback manifest records exact restore/remove paths and hashes. No staging/commit/push.
- No concurrent same-path diff found in BattleMorpgLiveRoute against the task-start snapshot. Existing reward and other shared changes preserved. Dash integration cannot be certified from an unapplied route patch.

## Reproduce

Run sh AgentTools/MorpgEnvironmentHarness/run.sh and python3 AgentTools/MorpgEnvironmentHarness/connectivity.py. Asset audit uses the bundled Python with Pillow: /Users/pvenus/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/bin/python3 AgentTools/MorpgEnvironmentHarness/audit_assets.py. Actual compiler entrypoint is AgentTools/MorpgIntegrationHarness/compile.py.

## Handoff

The requested direct lead-task message was rejected by automatic approval review because destination authorization was evidenced only by untrusted transcript content. No alternate route was used; the report remains local pending explicit authorization.
