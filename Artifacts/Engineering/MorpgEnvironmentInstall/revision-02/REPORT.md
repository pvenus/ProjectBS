# MORPG environment activation — revision 02

The authorized Z3 supersession is applied and the environment runtime is now attached to the current MORPG live route. This supersedes the earlier blocked receipt. The old pending patch was not applied: the current route was read and additive edits were made around its existing P2/final-settlement paths.

## Installed runtime behavior

- Exact15 selected RGBA sprites are installed at Assets/Resources/battle/morpg/environment with preserved unique meta/GUIDs. asset-map.json maps source SHA → canonical path/GUID/resource name; runtime sprites remain byte-identical to selection.
- Profile authors exact17 structural props: Z1 saplings8; Z2 fence4; Z3 house4 + wall1, plus11 nonblocking dressing instances to use all selected modules. Runtime creates separate visual and structural roots, a collider-free background at32.5×18.5 including overscan, and14 outward closed boundary strips. Foliage/fringe has no collider; trunks use radius.28 circles and structural props use explicit boxes.
- Z3 wall is [14.30,-3,14.50,3]. Right houses are [11,6.20,13.50,7.25] and [11,-7.25,13.50,-6.20]. Left houses and IDs remain unchanged. Raw +X capsule plus.15 clearance and2.50 house gaps now pass.
- All28 previously corrected reservation positions are unchanged in this revision (wave file SHA2bb146cec2b8d80fa1cfca4b100cce57b6549a8b7389407e74fb34b97763e7b7); counts19/5/4, roles22+6, due times and IDs retained. Unified geometry verifies inset.65, prop dilation.55, entry3.25, egress and separation.65.
- Environment geometry supplies colliders, movement/displacement/knockback sweeps, warp clearance and camera clamp. Movement uses actual collider extent plus.15. Warp validates before enabling the destination boundary; placement occurs before disabling the source. Geometry readback exposes the same sweep for dash callers. Existing transition timing/presentation code was preserved.
- Camera is set to ortho3 with16:9 projection, uses the exact per-zone clamp and restores its prior size/aspect/projection mode on disposal. Alpha fades use the scaled route tick; a conservative sprite-bound overlap triggers.35 opacity.
- Invalid/missing profile/sprite is rejected before mutation. Partial activation construction failure destroys the complete owned root before returning false. BattleSpawnManager then uses BattleManager's unchanged current v3→legacy chain, as newly authorized. Failures after activation stop the attempt and clean up without replaying a fallback or leaving a mixed environment.

## Evidence

- Actual installed Unity C# compiler:0 errors,31 existing warnings.
- Production geometry10/10: corrected profile valid; old coordinates rejected; all28 spawns safe; sweep/wall/sloped boundary; landing/camera; missing references; strict unknown/missing JSON rejection.
- Live production route + actual environment runtime under Unity stubs23/23: existing18 P2/final-settlement tests plus root/collider inventory, destination-before-source ordering, missing sprite preflight, partial activation cleanup and camera restoration.
- Production reward queue/ledger12/12; inactive spawn lifecycle10/10; NPC cast4/4 plus shader/order checks; P0/P1 suite27/27. Spawn harness required a test-only adjustment because shared engine stubs gained Quaternion and activeInHierarchy; production spawn code was not changed.
- Independent.10wu grid: all28 spawns connected to entry. Exact house gap and capsule clearance are checked by production geometry. No claim of exhaustive medial-axis/pocket proof beyond these focused checks.
- Asset15/15 audit:1024×1024 RGBA,48px transparent border, zero RGB at alpha0, no GUID collisions, correct pivot/PPU/filter/wrap/compression and collider-free sprite metadata. Authored composition preview visually inspected; it is not an engine screenshot.
- git diff --check and cumulative rollback reverse --check passed; rollback not executed; index unchanged. No Unity GUI, scene edits, staging, commit or push.

No Unity gameplay/render run was performed. Physical collision response, screen-pixel occlusion thresholds and rendered transition-pop behavior remain engine-unverified. The simulator exercises the production runtime's object creation and state ordering with engine stubs; it does not substitute for that visual gameplay check. No separate dash implementation appeared in the inspected route; existing transition code and timing were retained, not replaced or newly claimed as dash-tested.

## Authority and rollback

Source manifest SHA d5353184175684b7affaa9a95933c7271e3dc65addabbdbfa912c85e0bfa66fb; original contract SHA20fd0dc578f51d8218e7e222faecf94f70fa3260c41bf27346756f7decfd75b8 plus this revision's supersession.txt. receipt.json records the effective profile hash. The earlier pending-route-integration.diff is obsolete and must not be applied.

rollback-manifest.json + cumulative-environment.diff restore only environment changes to the original P2/final-settlement baseline and remove new environment files only on exact hash match. Existing reward assets, ledger/presentation code and other shared work are retained. route-additive.diff separately shows the live hook changes against the most recent route snapshot.

The requested lead handoff remains blocked by the previously reported automatic approval rejection. No alternate routing was attempted.

Sorting follow-up: the rendering bands in this revision were superseded by Artifacts/Engineering/MorpgSortingFix/REPORT.md to fix body occlusion. Apply sorting-only rollback first if rolling back the earlier environment revision.
