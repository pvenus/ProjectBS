# P1 revision-02 rollback (not executed)

Run `python3 Artifacts/Engineering/MorpgP1Rollback/revision-02/verify.py` from the project root before any restore. Stop on a SHA mismatch; never overwrite later work. No Git index, staging, commit, checkout, reset or global deletion is involved.

Two exact rollback destinations are retained in manifest.json:

1. Undo this executor's slice: restore the three `sourceChanges` rows with a `baseline` from their exact baseline .txt bytes and remove only the three rows marked `baselineAbsent`. This returns the unaccepted prior coordinator draft; it is not suitable for activation.
2. Remove the whole P1 source unit while preserving P0: use `wholeP1Rollback`. Remove only the five listed P1 additive files (coordinator .cs/.meta, new runtime .cs/.meta, P1RuntimeTests.cs). Restore Program.cs from p0-harness-program.txt and the project from p0-harness-project.txt. Both reconstructed P0 bytes were verified against the pre-existing accepted P0 manifest SHA, not inferred from behavior. Retain UnityEngineStub.cs and all P0 sources/materials unchanged.

Retain this evidence directory for audit. Restore source by exact path and verified bytes only; do not remove directories recursively. Re-read restored SHA values and confirm only explicitly removed paths are absent. Confirm preserved BattleManager, BattleSpawnManager and index hashes match the manifest, then compile/run the retained P0 sources with the same Mono/Roslyn compiler (omit P1 source/test files). The P0 csproj targets net8 and its previously absent offline targeting packs remain a separate environment limitation; Mono is the proven reproduction route.

No restore has been executed. Actual BattleManager/BattleSpawnManager attachment, producer integration, Unity scenes, visual camera/warp/cleanup QA, activation and P2 remain on hold.
