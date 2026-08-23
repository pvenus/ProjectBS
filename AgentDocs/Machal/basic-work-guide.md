# Machal Basic Work Guide

## Purpose and Scope

This guide defines the default way an agent works with Machal on ProjectBS.
It applies only when the user or task prompt points to `AgentDocs/Machal/README.md`.

Task-specific decisions belong in the active task document, not in this general guide.

## Before Work

1. Confirm the exact checkout and project root.
2. Read the required documents listed by `AgentDocs/Machal/README.md`.
3. Inspect the working tree and preserve unrelated tracked, untracked, and unstaged work.
4. Confirm every supplied reference path. Report missing paths instead of substituting similar files.
5. Identify the current runtime source, authoring source, and generated output before changing data.

## Work Method

1. Freeze scope and list owned paths before editing.
2. Work in small, independently verifiable units.
3. For code work, first establish a reachable invocation point and an observable temporary `[PLACEHOLDER]` log when practical.
4. Verify the placeholder path, then replace only one implementation unit at a time.
5. Keep data interpretation outside passive Views.
6. Prefer structured data and explicit categories over strings inferred from class, asset, skill, or effect names.
7. Do not report a feature as complete when only code compilation or partial wiring was verified.
8. Keep inspection and debug output complete. Apply omission or default-value filtering only through an explicitly named player-display path.

## Architecture and Path Rules

- Classify runtime code by gameplay-domain ownership.
- Put genuinely cross-domain presentation contracts directly under the existing `Assets/Scripts/Presentation/` category. Do not create a new `Assets/Scripts/Core/` path for Machal presentation work.
- Put gameplay interpretation and mapping under the owning domain, such as `Assets/Scripts/Ability/`.
- Do not add a content-domain `Presentation/` child solely for normalized content data. Put data types in the owner's existing or planned `Data/` child and keep resolver or builder behavior at the owner root.
- Do not add a separate `Resolvers/` child solely for this feature. Distinguish behavior by explicit class names such as `EffectPresentationResolver` and `SkillPresentationResolver`.
- Keep UI components and prefab binding under the existing Presentation/UI structure.
- Do not make UI the owner of gameplay meaning.
- Do not reorganize existing content or legacy paths unless the active task explicitly owns that migration.

## Data and Asset Rules

- Use only source and asset paths approved by the active task document.
- Do not modify legacy data merely to make a new feature easier to implement.
- Do not infer behavior from names.
- Confirm numeric units against runtime code. Similar fields may use different scales such as `0..1` and `0..100`.
- Do not apply serialized overrides or modifiers that the runtime resolver does not apply.
- Preserve multiple effects as separate semantic records.
- Resolve player-facing localization-key fields through the localization owner.
- Keep these two failure cases separate: omit an unapproved raw gameplay key that has no display mapping, but show the full intended localization key when an approved mapped key or required name/description key is missing. Visible missing keys are debugging evidence; never replace them with generated wording or prose inferred from structured numeric data.

## Verification

Each completed unit needs evidence appropriate to its risk:

- Exact files changed
- Reachable call path when applicable
- Data source to structured result comparison
- Relevant compile or automated test
- Asset or prefab binding check when UI work begins
- `git diff --check`
- Remaining fallback, unsupported, manual, and unverified cases

## Documentation and Handoff

Append to the active log after every bounded work unit. Each entry must contain:

- Date and status
- Scope performed
- Files created or changed
- Decisions made
- Verification evidence
- Pending work
- Blockers or risks
- Recommended next action

Do not rewrite older log entries to make later results look cleaner. Add a correction entry when a prior conclusion changes.

## Git

- Do not commit or push unless the user explicitly requests it.
- Stage only explicitly requested paths.
- Show the staged set before commit or push.
- Keep unrelated user changes untouched.
