# Final settlement integration receipt

Final settlement now has an explicit `FinalSettlementPending` state. Pending presentation is a normal wait, without invoking victory CAS or logging the former `final settlement not ready` failure. After the existing full-frame defeat barrier, hostile/spawn scope work stops and player movement is locked while rewards settle. Arrivals drain the ordered exactly-once ledger; missing presentation flushes it. A 1.25-second scaled-time deadline flushes stalled visuals. Pause freezes the deadline.

Victory CAS runs only after pending count reaches zero and reconciliation verifies every one of the 28 canonical reservation bundles, credited states, and authoritative 40 Gold / 10 raw XP account deltas. Reward values are derived from the reservation definition rather than mutable CharacterSO reference identity. A real discrepancy remains a diagnostic failure. The earlier message alone cannot establish whether the observed runtime failure was pending presentation or a genuine totals mismatch; this change separates those cases explicitly.

Defeat is checked before final victory, including an arrival tick. Teardown aborts any uncommitted victory, flushes confirmed receipts once, and releases locks. HUD disappearance alone can finish settlement on the next live tick. Disposal does not request a victory or load a scene.

## Validation

- Actual installed Unity C# compiler: 0 errors, 31 pre-existing warnings.
- Production live adapter with Unity stubs: 18/18; includes six focused final tests: pending arrival + duplicate death, missing HUD, stalled presentation timeout + pause, defeat on arrival, teardown abort + idempotent receipt settlement, and real account mismatch failure.
- Production delivery queue + ledger: 12/12, including transaction failure and rollback.
- Spawn lifecycle simulations: 10/10.
- NPC presentation simulations: 4/4 plus source/shader boundaries.
- P0/P1 deterministic suite: 27/27; trace SHA 19c159bebac2905bc8afa1213e3ed9c5331154d845c1ffedfd45c51105f7f355.
- Reward assets/import/static checks and git diff --check passed.
- fix-only.diff reverse --check passed; not applied. Index SHA unchanged.

No Unity GUI/gameplay execution was performed. Existing unrelated shared edits were retained. No scene editing, staging, commit, or push was performed.

Detailed handoff remains local: earlier automatic approval review rejected sending internal task details to the requested lead task. This receipt was not routed through an alternative channel.
