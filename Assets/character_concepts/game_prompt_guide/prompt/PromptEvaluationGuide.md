# Prompt Evaluation Guide

## Purpose

Use this guide to review a prompt under:

```text
Assets/character_concepts/game_prompts
```

The review should decide whether the prompt is safe, focused, copy-ready, and
aligned with the reference guides.

This guide evaluates the prompt document. It does not execute the prompt's task.

## Evaluation Inputs

Required:

```text
promptFile: Assets/character_concepts/game_prompts/{domain}/{PromptName}.md
```

Recommended:

```text
referenceGuideFiles:
  - Assets/character_concepts/game_prompt_guide/{domain}/...
relatedPipelineGuide: Assets/character_concepts/game_prompt_guide/story/StoryBattlePlanningPipelineGuide.md
```

Optional:

```text
sampleInputValues
expectedOutputFiles
knownFailureCase
```

## Evaluation Method

1. Read the prompt file.
2. Identify the task domain and task boundary.
3. Read only the reference guides explicitly named by the prompt.
4. Check whether the named guides are sufficient and not excessive.
5. Check whether inputs, task steps, outputs, validation, and failure behavior
   form a complete contract.
6. Check whether the prompt accidentally performs another pipeline step.
7. Check whether the prompt follows current folder separation:
   - prompts under `game_prompts`
   - guides under `game_prompt_guide`
8. Report issues by severity.

## Scoring

Score each item from 0 to 100.

Category scores are the average of their item scores. The overall score is the
average of all category scores.

| Category | Item | Meaning |
|---|---|---|
| Scope & Dependency | Task Boundary | The prompt does exactly one task or one intentional task bundle. |
| Scope & Dependency | Pipeline Fit | The prompt respects dependency order and manual build boundaries. |
| Contract Completeness | Input Contract | Required inputs are explicit, consistent, and enough to execute. |
| Contract Completeness | Output Contract | Outputs are concrete paths, ids, summaries, or validation results. |
| Contract Completeness | Failure Behavior | Missing dependencies have explicit stop/fail behavior. |
| Reference Quality | Guide References | The prompt references exact, minimal guide files. |
| Reference Quality | Maintainability | The prompt avoids duplicating guide content and stale rules. |
| Execution Safety | Validation | The prompt defines checkable pass/fail criteria. |
| Execution Safety | Safety | The prompt avoids destructive, broad, or unintended work. |
| User Readiness | Copy Readiness | A user can copy the prompt block and fill placeholders easily. |

Recommended rating:

```text
90-100: Excellent
80-89: Good
70-79: Needs revision
0-69: Rewrite recommended
```

## Severity

Use these severity levels:

```text
Critical
Major
Minor
Suggestion
```

Critical:

- Prompt can generate or modify the wrong asset type.
- Prompt crosses a manual build boundary without saying so.
- Prompt embeds data that must be referenced by id.
- Prompt can silently create failure artifacts.
- Prompt omits a required dependency gate.

Major:

- Inputs are incomplete or inconsistent.
- Outputs are not concrete.
- Validation is too vague to verify.
- Guide references are missing or point to the wrong root.
- Prompt can read too many unrelated files.

Minor:

- Naming is inconsistent but understandable.
- Placeholder names differ from pipeline conventions.
- Output order is inconvenient.
- Failure wording is present but weak.

Suggestion:

- Improve wording, grouping, or readability.
- Add examples.
- Rename a task for clarity.

## Required Checks

### 1. Location

Pass when:

- prompt file is under `Assets/character_concepts/game_prompts`
- no copy-ready prompt is under `Assets/character_concepts/game_prompt_guide`

Fail when:

- guide and prompt files share the same domain folder again
- a prompt path is referenced through `game_prompt_guide/...Prompt.md`

### 2. Task Boundary

Ask:

- What task does this prompt perform?
- Is it independent?
- If bundled, are the bundled steps naturally inseparable?
- Does it accidentally include Unity asset generation, image generation, or
  runtime validation outside its scope?

### 3. Inputs

Inputs should include all files needed to execute the task.

Check for:

- stable ids
- source files
- output paths
- dependency outputs from earlier prompts
- optional files marked as optional

### 4. Guide References

Prompt should reference exact guide files.

Fail examples:

```text
Assets/character_concepts/game_prompt_guide/story
모든 관련 가이드
필요한 문서 전부
```

Pass examples:

```text
Assets/character_concepts/game_prompt_guide/story/EpisodePlanningCreateGuide.md
Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md
```

### 5. Outputs

Outputs should be file paths, ids, or named summaries.

Check:

- Does every written file have a path?
- Does every generated id have a naming rule?
- Does the output include validation result?
- Does the prompt say what was not generated when appropriate?

### 6. Validation

Validation should be executable mentally or by command.

Good:

- `JSON 문법이 유효해야 한다.`
- `BattleSO asset이 expectedBattleSOPath에 있어야 한다.`
- `outputImagePath의 PNG가 16:9여야 한다.`

Weak:

- `잘 되었는지 확인한다.`
- `품질을 확인한다.`

### 7. Failure Behavior

If the prompt depends on search or matching, it needs failure behavior.

Examples:

- no reusable spawner found
- no CharacterSO resolves
- no BattleSO exists for `rewardId`
- no PixelLab export found
- required story/planning JSON missing

### 8. Boundary Rules

Check for current ProjectBS boundaries:

- JSON-only prompt does not create `.asset`.
- Battle reward uses `rewardId` and does not embed full `battle`.
- Spawner JSON does not contain concrete monster names or `characterId`.
- BattleSO JSON owns `spawnUnitBindings`.
- Stage popup image uses `{eventId}.main.png`.
- Battle background image uses `{battleId}.background.png`.
- Manual Unity build prompts are separate from authoring prompts.

## Evaluation Output Format

Use this output shape:

```text
Prompt:
Domain:
Task:
Overall Score:
Rating:

Findings:
- [Severity] Title
  Evidence:
  Impact:
  Recommendation:

Score Breakdown:
- Scope & Dependency: /100
  - Task Boundary: /100
  - Pipeline Fit: /100
- Contract Completeness: /100
  - Input Contract: /100
  - Output Contract: /100
  - Failure Behavior: /100
- Reference Quality: /100
  - Guide References: /100
  - Maintainability: /100
- Execution Safety: /100
  - Validation: /100
  - Safety: /100
- User Readiness: /100
  - Copy Readiness: /100

Missing Inputs:
- ...

Unclear Outputs:
- ...

Boundary Risks:
- ...

Suggested Revision Summary:
- ...

Pass / Fail:
```

## Pass Criteria

A prompt passes when:

- overall score is 80 or higher
- no Critical findings exist
- no more than two Major findings exist
- the prompt location is correct
- the prompt can be copied and executed with filled inputs

If a prompt fails, do not rewrite it silently. Report the issues and propose the
minimal set of edits needed.
