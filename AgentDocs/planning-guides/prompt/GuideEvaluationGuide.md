# Guide Evaluation Guide

## Master Concept Reference

Before using this document, read and apply:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
```

The applicable master concept, runtime/schema authority, common policy, and
explicitly named domain authority each own their documented concern. They take
precedence over a lower-level guide only within that concern. This document
evaluates guide quality; it does not replace a domain contract.

## 1. Purpose

Use this guide to evaluate a reference document under:

```text
AgentDocs/planning-guides
```

Supported targets include:

```text
reference guide
schema or data-structure guide
workflow or pipeline guide
evaluation guide
Slack Canvas guide
explicitly declared hybrid guide
```

This is not a prompt evaluation guide. Use
`AgentDocs/planning-guides/prompt/PromptEvaluationGuide.md` for copy-ready task
prompts under `AgentDocs/task-prompts`.

Evaluation is read-only. Do not edit the target, repair references, execute a
documented workflow, create assets, copy files, import into Unity, perform Git
work, deploy, or publish an evaluation result to Slack.

## 2. Evaluation Input Contract

Required:

```text
guideFile: required project-relative Markdown path
expectedLocation: AgentDocs/planning-guides/{domain}/{GuideName}.md
```

The expected location is normative, but the evaluator may receive a readable
candidate elsewhere in the repository so it can diagnose `invalid_guide_location`
rather than refusing to inspect the document.

Optional:

```text
guideTypeHint
domainHint
purposeHint
referenceGuideFiles[]
runtimeContractFiles[]
knownConflictOrFailureCase
passScore: 90
categoryPassScore: 90
itemPassScore: 90
```

Hints help locate context but do not override file contents. Derive guide type,
domain, and purpose from the target and report a mismatch with supplied hints.

## 3. Evaluation Context and Authority Order

Read in this order:

1. this evaluation guide;
2. the mandatory master concept;
3. the target guide;
4. exact higher-authority files named by the target;
5. exact schema, code, Unity/runtime contract, or common folder policy needed
   to verify claims made by the target;
6. exact cross-guides that share an ID, path, state, or workflow boundary;
7. optional input references used only to close a documented context gap.

Default authority ownership by concern:

```text
runtime/code and serialized schema -> executable fields, types, loading, runtime behavior
mandatory master concept -> design period, culture, aesthetics, and prohibitions
common policy -> repository-wide paths, roles, and shared process contracts
domain source of truth -> domain identity, meaning, and domain-specific rules
workflow/evaluation/Slack extension -> orchestration, verdict, and recording extension
task prompt -> execution request constrained by every applicable authority above
examples and legacy documents -> non-authoritative evidence only
```

An explicitly documented repository authority rule may refine this ownership.
When two authorities claim the same concern inconsistently, do not resolve the
conflict by silently choosing whichever text is more convenient. Record the
conflict, affected contract, and required owner decision.

Do not read an entire directory because a guide says “all related documents.”
Treat that wording as a maintainability defect and resolve only the minimum
exact files needed to evaluate the claim.

## 4. Guide Type Identification

Classify the target before scoring:

| Guide type | Primary responsibility |
| --- | --- |
| reference | Defines concepts, rules, terminology, or design intent. |
| schema/data-structure | Defines fields, types, constraints, identity, serialization, or runtime mapping. |
| workflow/pipeline | Defines ordered roles, inputs, outputs, states, handoffs, boundaries, and failure behavior. |
| evaluation | Defines observable criteria, scoring, severity, hard fails, verdict, and re-evaluation. |
| Slack Canvas | Defines a recording form and mapping from an authoritative evaluation report; it does not re-evaluate. |
| hybrid | Owns more than one responsibility and explicitly explains why they cannot be separated. |

If the type cannot be identified, score Guide Type Clarity below the item pass
threshold. If unrelated responsibilities are combined without justification,
also report a boundary finding.

## 5. Scoring Method

Score every applicable item from 0 to 100 using repository evidence.

```text
categoryScore = arithmetic mean of applicable item scores
overallScore = arithmetic mean of applicable category scores
```

Round only displayed results to two decimal places. Do not round intermediate
values. Mark a conditional item `N/A` only when this guide explicitly permits
it; exclude N/A items from category averages. Never use N/A to hide missing
required content.

Default thresholds:

```text
passScore: 90
categoryPassScore: 90
itemPassScore: 90
```

A guide passes only when:

- overallScore is at least passScore;
- every applicable category reaches categoryPassScore;
- every applicable item reaches itemPassScore;
- no Critical finding or Hard Fail exists.

Rating:

```text
95-100: Excellent
90-94.99: Ready
80-89.99: Needs revision
70-79.99: Major revision
0-69.99: Rewrite recommended
```

## 6. Evaluation Categories and Items

### 6.1 Location & Role Separation

| Item | Pass expectation |
| --- | --- |
| Guide Location | Target is under `AgentDocs/planning-guides` at a domain-appropriate path. |
| Prompt Separation | No copy-ready task prompt is embedded or stored as the guide itself. Examples are clearly non-executable and minimal. |
| Responsibility Separation | Guide policy is not mixed with task-prompt orchestration, unrelated pipeline stages, or implementation ownership. |

### 6.2 Purpose & Scope

| Item | Pass expectation |
| --- | --- |
| Purpose Clarity | The document states what it governs and why it exists. |
| Guide Type Clarity | Its reference, schema, workflow, evaluation, Slack, or justified hybrid role is identifiable. |
| Applicability & Exclusions | Target artifacts, consumers, prerequisites, non-goals, and out-of-scope work are explicit. |

### 6.3 Source of Truth & Reference Priority

| Item | Pass expectation |
| --- | --- |
| Authority Declaration | Canonical source, common/master rules, schema/runtime contract, and owning guide are named when applicable. |
| Priority & Conflict Rule | Precedence and stop/escalation behavior for conflicts are explicit. |
| Reference Precision | Required references use exact current paths and each reference has a clear reason. |

### 6.4 Contract Completeness

Judge fields only when appropriate to the guide type; a reference guide need
not imitate a workflow schema.

| Item | Pass expectation |
| --- | --- |
| Inputs & Preconditions | Required inputs, dependencies, allowed optional context, and readiness gates are sufficient. |
| Outputs & Handoffs | Outputs, consumers, next state, or explicit no-output behavior are concrete. |
| Identity & Storage | Relevant path, ID, filename, version, ownership, and storage rules are deterministic. |
| States & Failure Behavior | Relevant statuses, blockers, failure types, partial-result rules, and retry behavior are explicit. |
| Validation & Executability | A task prompt can reference the guide and execute it without inventing material rules. |

### 6.5 Cross-guide Consistency

| Item | Pass expectation |
| --- | --- |
| Common Contract Alignment | Terms and paths agree with GuideAuthoringGuide and with PromptAuthoringGuide, PromptEvaluationGuide, ContentFolderStructureGuide, and other applicable common guides when relevant. |
| Domain/Pipeline Alignment | IDs, filenames, states, roles, handoffs, and runtime meaning agree with exact related domain guides. |
| Current Path Contract | Normative paths use `AgentDocs/planning-guides`, `AgentDocs/task-prompts`, `AgentDocs/planning-data`, and current project storage contracts as applicable. |

The legacy roots below must not be used as current normative guide or prompt
locations:

```text
Assets/character_concepts/game_prompt_guide
Assets/character_concepts/game_prompts
```

A clearly labeled migration-history statement or prohibited-path example is
allowed. An active reference, output, default, or instruction using a legacy
root is a stale path contract and a Hard Fail.

### 6.6 Safety & Boundary

| Item | Pass expectation |
| --- | --- |
| Mutation Boundary | Read/write authority and manual approval boundaries are explicit and no unrelated mutation is implied. |
| Pipeline Stage Separation | Authoring, generation, download, evaluation, promotion, Slack recording, Unity work, Git, and deployment remain correctly separated. |
| Destructive/External Safety | Overwrite, broad copy, provider cost, external publication, and destructive actions have appropriate gates. |

### 6.7 Evaluation Rule Quality (conditional)

Score this category only for an evaluation guide or a hybrid guide that owns an
evaluation verdict. A Slack Canvas form that merely records an existing verdict
is N/A unless it introduces evaluation logic.

| Item | Pass expectation |
| --- | --- |
| Score Model | Total scale, category/item calculation, thresholds, rounding, and N/A behavior are complete. |
| Criteria Observability | Items can be judged from identified evidence rather than taste alone. |
| Severity & Hard Fail | Critical/Major/Minor/Suggestion meanings and absolute-fail rules are explicit. |
| Verdict & Re-evaluation | Pass/fail precedence, conditional outcomes if any, required actions, and re-evaluation behavior are deterministic. |

### 6.8 Maintainability

| Item | Pass expectation |
| --- | --- |
| Duplication Control | Shared contracts are referenced instead of copied into long divergent variants. |
| Stable References | No stale path, broad directory instruction, ambiguous “all related guides,” or unexplained optional dependency exists. |
| Extension & Versioning | Change ownership, extension points, compatibility/version behavior, or revision expectations are clear when needed. |

### 6.9 User/Agent Readiness

| Item | Pass expectation |
| --- | --- |
| Action Clarity | An agent understands what it must do, must validate, and must not do. |
| Decision Closure | Material ambiguity leads to a defined stop, failure, escalation, or owner decision rather than invention. |
| Navigability | Structure, terminology, examples, and references make the contract efficiently usable. |

## 7. Severity and Findings

Use:

```text
Critical
Major
Minor
Suggestion
```

- Critical: can produce the wrong artifact/state, violate an authority or
  manual boundary, create unsafe mutation, or make the guide fundamentally
  non-executable.
- Major: a required contract, source priority, failure rule, or cross-guide
  relationship is missing or contradictory.
- Minor: localized ambiguity, inconsistent naming, weak validation, or a
  maintainability defect with limited execution risk.
- Suggestion: non-blocking clarity, organization, example, or wording
  improvement.

Every finding includes exact file/section evidence, impact, and the minimum
recommended correction. Do not invent line evidence or claim a referenced file
was read when it was not.

## 8. Hard Fail Rules

The following force Overall Pass / Fail to `Fail` regardless of score:

```text
Critical finding
invalid_guide_location
copy_ready_prompt_inside_guide
stale_path_contract
material authority/runtime contradiction
unsafe or unbounded mutation authority
evaluation guide with an internally inconsistent score total or verdict rule
```

`copy_ready_prompt_inside_guide` applies when a guide contains a user-ready task
block intended to be executed, not when it shows a short, clearly labeled
non-executable example or schema fragment.

Hard Fail is an evaluated result: continue scoring and report the evidence.

## 9. Operational Failure Types

Operational failures prevent trustworthy scoring:

```text
missing_guide_file
missing_evaluation_guide
missing_authoring_guide
unreadable_reference_guide
insufficient_evaluation_context
```

On operational failure, do not output Overall Score, Rating, category scores,
or pass/miss item lists. Report what was read, what was unavailable, and the
minimum required next action. Do not classify a readable structural defect as
an operational failure merely to avoid scoring it.

## 10. Required Evaluation Report

```text
Guide
Guide Type
Domain
Purpose
Overall Score
Rating
Overall Pass / Fail
Hard Fail 여부 / Hard Fail Rules Triggered

점수 통과 항목
점수 미달 항목
카테고리별 점수와 item 점수
Findings: Critical / Major / Minor / Suggestion
Cross-guide Conflicts
Boundary Risks
수정 우선순위
재평가 예상
References Actually Read
References Required But Unavailable
```

Evidence must support every below-threshold item and every finding. When no
entry exists for a section, write `없음` rather than omitting the section.
The report is returned in the current response only. Do not persist it unless a
separate task explicitly authorizes a report destination and write operation.

## 11. Read-only Completion Checklist

- [ ] Target guide was not modified.
- [ ] No referenced workflow was executed.
- [ ] No asset, report file, Canvas, Unity file, Git state, or deployment state
      was created or changed.
- [ ] Guide type and domain were derived and hint mismatches were reported.
- [ ] Every applicable item was scored and calculations were verified.
- [ ] Hard Fail precedence was applied after scoring.
- [ ] Actual and unavailable references were distinguished.

## 12. Related Documents

```text
AgentDocs/planning-guides/prompt/GuideAuthoringGuide.md
AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
AgentDocs/planning-guides/prompt/PromptEvaluationGuide.md
AgentDocs/task-prompts/prompt/GuideEvaluationReportPrompt.md
```
