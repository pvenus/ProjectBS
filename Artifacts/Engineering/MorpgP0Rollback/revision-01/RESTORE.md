# MORPG P0 additive rollback

This package records a non-executed rollback boundary. All manifest paths were absent at baseline.

1. Verify every file SHA against `manifest.json`.
2. Preserve unrelated worktree and index changes.
3. Remove only the listed additive files and any additive directory that is empty afterward.
4. Do not stage, reset, restore, or checkout unrelated paths.
5. Confirm all listed paths are absent and that the pre-existing battle material remains unchanged.

No restore was executed while producing this package.
