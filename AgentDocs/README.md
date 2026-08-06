# AgentDocs

`AgentDocs` contains agent-facing prompts, planning guides, planning source data, and reference assets that do not need to be imported by Unity.

## Directory roles

- `workflows/`: shared agent workflow rules. No workflow files were available to migrate in this change.
- `task-prompts/`: reusable prompts that directly request a bounded task.
- `planning-guides/`: authoring, evaluation, production, and data-structure guidance.
- `planning-data/`: ProjectBS world, story, character, battle, event, shop, and item planning sources.
- `reference-assets/`: non-runtime source images and map files used by planning documents.

## Migration sources

- `Assets/Doc` was moved into the role-based directories above.
- `Assets/character_concepts` was moved into the role-based directories above.
- Unity `.meta` files were intentionally excluded.

Use project-root-relative paths when linking between documents. Do not place a file back under `Assets` unless Unity must import it as a runtime or editor asset.
