# Battle MORPG Zone Reward v1

- `schemaVersion`: `battle-morpg-zone-reward.v1`
- Owner: Episode1 `rescue_villagers` battle runtime only.
- Consumer: strict `BattleMorpgDefinitionCodec` and route/reward coordinators.
- Missing definition or missing `schemaVersion`: legacy mode; no zones, warps, drops, or suppression are fabricated.
- Unknown version or invalid required field: fail closed before player control and retain the v2 fallback sequence.
- Stable clear key: `(BattleAttemptKey, zoneId, waveId)`.
- Stable transition key: `(BattleAttemptKey, fromZoneId, toZoneId, ordinal)` with Requested/Prepared/Committed/Restored/Failed states.
- Outcome key: `BattleAttemptKey(runId, episodeId, battleId, attemptId)`.
- `bundleId` is the enemy spawn/death receipt equivalent for this exact route. A reservation creates exactly one enemy root; child colliders and callback retries normalize to that root before the bundle is created.
- Save policy: no disk/public/StageSession schema expansion. Every nonterminal durable-save request is queued until the existing final outcome handoff. A scene reload or process loss abandons the run-local attempt, restores the durable pre-battle checkpoint, and requires a new attempt ID; only in-scene pause resumes local clocks.
- Reward precision: XP is recorded as integer quarter-units (`black=1`, `chain=3`, total `40`) and converted once at account application (`/4` = `10 XP`).
- Canonical material IDs: `zone.left.01`, `zone.center.02`, `zone.right.03` waves and matching `entry_warp` anchors are primary. The earlier `zone1|zone2|zone3` spellings are rejected diagnostics only.
- Lifecycle owners: clear `None→Detected→Committed`, wave start `None→Started→Completed`, transition `Requested→Prepared→Committed` (or legal restore/fail), and outcome `None→Requested→terminal` reject illegal edges and duplicate mutation.
- Reward account overflow outside the exact preflight headroom fails closed. Broader cap/level semantics are not introduced by v1.
- Migration is additive: absent definition remains legacy. Re-decoding or revalidating a v1 definition is semantic diff0 and never mutates project assets.
