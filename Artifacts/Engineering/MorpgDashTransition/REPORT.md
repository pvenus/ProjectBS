# MORPG Dash transition implementation receipt

Status: **IMPLEMENTED_WITH_CURRENT_GEOMETRY_SAFE_FALLBACK**. This is not acceptance of the pending horizontal environment revision03.

The live MORPG route now owns a Dash presentation state machine: reward settlement → right-facing anticipation → cubic-in exit → alpha-zero atomic boundary/player/camera switch → cubic-out entry → Idle settle → unlock → delayed wave receipt. Camera follow is held until the following simulation frame after that receipt. The existing gameplay transition authority, deduplication, source cleanup and reward ledger remain in use.

The accepted addendum is copied to `contract.txt`, SHA-256 `d7ed28cd4f31ae36c6b9d14ab1fb7c398af0874f61fcb0090929ab08f0724f92`.

## Implemented behavior

- Cleanup/settlement gate at .50s; .08s anticipation; exit duration clamp(distance/14, .32, .65), cubic-in motion, final45% smoothstep alpha fade. Entry is 3wu/.36s cubic-out with .22s fade, followed by .16s Idle and .20s delay after unlock before target-wave start.
- The invisible simulation sample is retained through a later frame. Old boundary is disabled, player/body and camera are moved, and destination boundary is enabled synchronously. Target-wave due-zero reservations spawn on the same tick as the wave receipt. Camera releases on the following frame.
- Animation-only reuse of the existing Seojin Dash clip; source and Resources copy are byte-identical. Sprite/flip curves only, no position/scale curves or events. No gameplay skill invocation or cooldown consumption; root motion is disabled and restored.
- Actor radius plus .15 collision clearance, authored corridor validation, dynamic Rigidbody casts and landing checks. Missing animation, reduced motion, invalid corridor or dynamic obstruction selects obscured fallback. An entry obstruction hides relocation to the accepted landing without another transition CAS. A blocked landing fails and restores the source without spawning the next wave.
- Exact captured renderer color/flip, animation/facing ownership, root motion, input/immunity and camera ownership release on success/abort. CharacterManager remains enabled so status lifecycles continue; transition locks also guard skill entry points. Existing external animation ownership is not stopped when this presentation never acquired it.
- Environment profile accepts separate optional transition anchors. The revision02 author script preserves these fields. No horizontal geometry was invented and no spawn reservation was changed by this task.

## Current authored geometry limitation

The installed revision02 closed polygons do **not** accommodate the addendum's exit/staging capsules. For example Z1 exit x=-5.70 is outside its boundary near x=-5.85, and Z2 staging x=-3.00 cannot fit the required corridor inset against its left edge x=-4.25. Accordingly the real installed profile selects `authored dash corridor blocked` and uses the safe obscured fallback. Its fallback behavior is explicitly covered by the live integration harness.

Normal Dash tests use a clearly isolated, widened in-memory test geometry. These tests do not certify the installed map for normal Dash. A new accepted horizontal revision03 contract with compatible corridors is still required before changing map coordinates or claiming normal Dash on the installed map. Lead guidance was to retain the current same-path environment base if that authority is absent.

## Verification

Unity 6000.3.10f1 actual C# compiler: **0 errors, 31 existing warnings**. No Unity GUI or scene mutation.

| Check | Result |
|---|---:|
| Live production route/environment/Dash view, Unity engine stub | 33/33 |
| Production transition clock | 8/8 |
| Production environment geometry | 12/12 |
| Reward queue and ledger | 12/12 |
| Inactive spawn lifecycle | 10/10 |
| Cast presenter | 4/4 |
| Existing P1 authority/fault suite | 27/27 |
| Total deterministic cases | 106/106 |

Reward sprite byte checks and source invariants pass. `git diff --check` and `git apply --reverse --check dash-only.diff` pass. The Git index hash is unchanged. No staging, commit, push, Unity launch or scene edit was performed.

The 28-reservation wave file remains SHA-256 `2bb146cec2b8d80fa1cfca4b100cce57b6549a8b7389407e74fb34b97763e7b7`; stable IDs, roles, due times and 19/5/4 counts are unchanged.

The harness validates simulation samples and mocked Unity APIs. It is **not** GPU/rendered-frame or Play Mode evidence. On-screen alpha, actual sprite rig playback, physics and camera ordering still need scene acceptance once scene execution is authorized.

## Artifacts and rollback

`receipt.json` records machine-readable results. `dash-only.diff`, `before/`, and `rollback-manifest.json` isolate this task from existing shared dirty work. Check current hashes before a future rollback; reverse only the task patch. The reverse check did not apply any rollback.

Test outputs are adjacent `*.log` files. Reproduce with the commands named by the harness scripts under `AgentTools/MorpgDashHarness`, `AgentTools/MorpgIntegrationHarness`, `AgentTools/MorpgEnvironmentHarness`, and the P1 `REPRODUCE.sh`.

## Delivery status

This receipt is prepared for 한결. Earlier detailed cross-task sends were rejected by automatic approval review because destination authorization came only from untrusted task text. No alternate routing was used to bypass that rejection. Detailed delivery requires explicit trusted authorization. The permitted request for a shared contract path did not transmit this implementation report.
