# Machal Agent Work Start

## Purpose

This directory is the handoff entrypoint for agents working with Machal on ProjectBS.
It records the working rules, active task contract, decisions, and verification history needed to continue work in another chat.

This is a user-invoked work contract. `AGENTS.md` is intentionally not changed for this workflow.

## Required Reading Before Work

Before analyzing, editing code, changing assets, or modifying prefabs for a Machal task, read these files from beginning to end in this order:

1. `AgentDocs/Machal/README.md`
2. The active task-specific new-chat start contract listed below
3. `AgentDocs/Machal/basic-work-guide.md`
4. The active task document listed below
5. The active inventory document listed below
6. The active contract evaluation document listed below
7. The active display catalog listed below
8. The active Stage 4 verification document listed below
9. The active Stages 5-8 completion document listed below
10. The active UI prefab preparation document listed below
11. The active Character Skill tab document listed below
12. The active Character content document listed below
13. The active Faith page design listed below
14. The active task log listed below
15. Any exact source or reference paths listed by the active task document

Do not start implementation when a required path is missing. Record the missing path in the task log and report it instead of guessing.

## Active Task

- Task: Ability content presentation data system
- New-chat start contract: `AgentDocs/Machal/owned-effects-inventory-task-start.md`
- Korean new-chat start contract: `AgentDocs/Machal/owned-effects-inventory-task-start-ko.md`
- Contract: `AgentDocs/Machal/ability-content-presentation-task.md`
- Inventory: `AgentDocs/Machal/ability-content-presentation-inventory.md`
- Stage 3 contract: `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`
- Contract evaluation: `AgentDocs/Machal/ability-content-presentation-contract-evaluation.md`
- Display catalog: `AgentDocs/Machal/ability-content-presentation-display-catalog.md`
- Stage 4 verification: `AgentDocs/Machal/ability-content-presentation-stage4-verification.md`
- Stages 5-8 completion: `AgentDocs/Machal/ability-content-presentation-stage5-8-completion.md`
- UI prefab preparation: `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`
- Character Skill tabs: `AgentDocs/Machal/character-skill-content-tabs.md`
- Character content: `AgentDocs/Machal/character-content-presentation.md`
- Faith page: `AgentDocs/Machal/faith-page-design.md`
- Log: `AgentDocs/Machal/ability-content-presentation-log.md`
- Current phase: the tabless Owned Effects View/Presenter and the three required prefab component graphs are directly wired under the user's one-off authorization. Static solution build and serialized-reference checks pass. Unity import, Inspector confirmation, configured source assignment, and Play Mode click/scroll/detail validation remain user-owned; automatic runtime Manager collection and separate encyclopedia pages remain pending

## Starting This Work in Another Chat

Give the next agent this instruction:

```text
Continue the ProjectBS tabless Owned Effects inventory task. Before analyzing or changing files, read AGENTS.md, AgentDocs/task-start-documentation-prompt.md, AgentDocs/Machal/README.md, and AgentDocs/Machal/owned-effects-inventory-task-start.md completely. Then follow the exact Mandatory Reading Order in the task-start contract. Read AgentDocs/code-writing-rules.md before changing C#. First preserve and verify the already wired Panel_OwnedEffects, category prefab, and item prefab. Unity import and Play Mode verification are user-owned. Preserve unrelated work and do not reset, clean, commit, push, migrate legacy data, operate Unity, or perform further prefab YAML edits without a new explicit user authorization.
```

## Update Rule

At the end of each work unit:

1. Update the active task document if scope, design, source paths, or decisions changed.
2. Append a dated entry to the active task log.
3. Separate verified work, pending work, and blocked work.
4. Record exact project-root-relative paths and concrete verification evidence.

The English documents are canonical. Update the matching `-ko` document in the same work unit.
