# Guide Authoring Guide

## 1. Purpose

This document is the authoring standard for reference documents under:

```text
AgentDocs/planning-guides
```

It defines how to create or revise reference, schema/data-structure, workflow,
evaluation, Slack Canvas, and explicitly justified hybrid guides so that a
task prompt can use them without inventing material rules.

This document does not define how to write a copy-ready task prompt. Use
`AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md` for documents under
`AgentDocs/task-prompts`.

```text
Guide Type: reference/policy
Domain: prompt and guide documentation
Primary Consumers: guide-authoring tasks and guide reviewers
Successful Handoff: one review-ready planning guide or a typed authoring blocker
```

## 2. Authoring Boundary

Guide authoring may:

- analyze the requested policy, current repository structure, exact related
  guides, schemas, code, Unity/runtime contracts, and representative data;
- create or revise planning-guide Markdown;
- update a guide index when needed;
- document unresolved conflicts and block unsafe assumptions.

Guide authoring does not, unless separately requested:

- execute the workflow described by the guide;
- create content JSON, SO assets, images, evaluation reports, or Slack Canvas
  records;
- operate an image provider, download or promote an artifact, import into
  Unity, perform Git work, or deploy;
- place a user-ready executable task prompt inside a planning guide.

Short examples, schemas, field maps, and pseudo-steps are allowed when clearly
labeled as non-executable reference material.

## 3. Location and Role Separation

Use these current roots:

```text
reference guides, schemas, policies, workflows:
  AgentDocs/planning-guides/{domain}/...

copy-ready task prompts:
  AgentDocs/task-prompts/{domain}/...

planning data and generated records:
  AgentDocs/planning-data/...
```

Do not use these legacy roots as an active default, input, output, or reference
contract:

```text
Assets/character_concepts/game_prompt_guide
Assets/character_concepts/game_prompts
```

They may appear only in a clearly labeled migration note, historical statement,
or prohibited-path example.

File naming:

- use a stable PascalCase responsibility name;
- end general policy documents with `Guide.md` when it improves distinction;
- keep schema/data-structure guides in the existing domain `data-structures`
  folder when that domain uses one;
- do not name a planning guide `{TaskName}Prompt.md`;
- do not create a new folder taxonomy when an established domain folder owns
  the concept.

## 4. Required Authoring Inputs

Resolve these before writing:

```text
requested purpose and expected consumer
guide type
content domain and target artifact
target project-relative guide path
canonical source of truth
applicable common/master policy
exact related guides
exact schema, code, or Unity/runtime contract when applicable
existing IDs, paths, filenames, states, and failure conventions
known conflict, failure case, or migration constraint
```

User-provided descriptions are requirements, not proof of repository state.
Verify repository claims from current files. Never reuse another PC's absolute
path as a document contract.

If the canonical authority or guide ownership cannot be resolved, do not fill
the gap with a plausible convention. Record the blocker and required owner
decision.

## 5. Guide Type

Declare one primary type near the beginning of the document or make it
unmistakable from Purpose and Scope.

| Type | Responsibility |
| --- | --- |
| reference | Concepts, terminology, invariants, design intent, and policy. |
| schema/data-structure | Fields, types, identity, serialization, constraints, and runtime mapping. |
| workflow/pipeline | Roles, ordered work, states, handoffs, failures, retries, and boundaries. |
| evaluation | Evidence, criteria, scoring, severity, verdict, and re-evaluation. |
| Slack Canvas | Mapping an authoritative evaluation result into a recording form without re-evaluating it. |
| hybrid | Two or more inseparable responsibilities with an explicit justification and internal boundary. |

Do not use `hybrid` merely to avoid separating unrelated work. If two parts can
change, execute, or be evaluated independently, author separate guides and link
them through exact references.

## 6. Authority and Reference Rules

### 6.1 Ownership by concern

Authority is determined by responsibility, not by one unconditional global
order:

```text
runtime/code and serialized schema -> executable fields, types, loading, runtime behavior
mandatory master concept -> design period, culture, aesthetics, and prohibitions
common policy -> repository-wide paths, roles, and shared process contracts
domain source of truth -> domain identity, meaning, and domain-specific rules
workflow/evaluation/Slack extension -> orchestration, verdict, and recording extension
task prompt -> execution request constrained by applicable authorities above
example or legacy document -> non-authoritative evidence
```

State which source owns every material contract introduced by the guide. If two
authorities claim the same concern inconsistently, define a stop/escalation
rule and identify the owner decision required. Do not silently merge them.

### 6.2 Reference precision

- reference exact project-relative files, not directories;
- explain why each required reference is authoritative or necessary;
- do not use “all related guides,” “the relevant documents,” or equivalent
  broad discovery instructions;
- do not list the target guide itself as a required input;
- do not cite a task prompt as policy authority;
- a prompt may be listed as a related consumer, but its output must not define
  the guide's truth;
- distinguish required authority, optional supporting evidence, examples, and
  legacy material;
- when referencing an external specification, record the exact product/tool
  scope and avoid copying volatile details without a verification rule.

### 6.3 Master concept applicability

Read and apply the exact master/common policy required by the domain. Design,
art, story, character, item, skill, stage, and other visual or cultural guides
must identify the applicable master concept. Administrative guides must not
invent visual requirements merely to mention it.

## 7. Common Required Sections

Every guide must contain the following information. Headings may be combined
when the meaning remains explicit.

### 7.1 Title

- unique, stable, and responsibility-oriented;
- not easily confused with a copy-ready prompt or generated data file.

### 7.2 Purpose

State:

```text
what the guide governs
why it exists
who or which task consumes it
what successful use produces or decides
```

### 7.3 Scope and exclusions

Identify:

```text
guide type
domain and target artifact
applicable cases
prerequisites
non-goals and forbidden responsibility expansion
```

### 7.4 Authority and references

Name the canonical source, applicable common/master rule, related owning guide,
and runtime/schema authority. Include priority by concern and conflict behavior.

### 7.5 Contract

Define only the fields appropriate to the guide type, but do not omit a
material execution requirement:

```text
inputs and preconditions
outputs, decisions, or explicit no-output behavior
paths, IDs, filenames, ownership, and versions when applicable
states and transitions when applicable
failure, blocker, partial-result, and retry behavior when applicable
consumer or next handoff
```

### 7.6 Validation

Provide observable checks. Replace vague instructions such as “verify quality”
or “process appropriately” with conditions that an agent, validator, reviewer,
or runtime contract can check.

### 7.7 Safety and responsibility boundary

State what the consumer may and must not mutate. Separate authoring, generation,
download, evaluation, project promotion, Slack recording, Unity build/import,
Git, and deployment whenever they have different owners or approval gates.

### 7.8 Failure and conflict behavior

Define whether the consumer stops, returns a typed failure, requests an owner
decision, retries, or preserves a partial result. Never authorize invention as
the default response to missing evidence.

### 7.9 Related documents

List exact upstream authorities, downstream consumers, or extension guides.
Do not duplicate their full content.

## 8. Type-specific Required Contracts

### 8.1 Reference guide

Required when relevant:

```text
canonical terminology
invariants and prohibited interpretations
applicability and exceptions
source evidence and examples
extension ownership
```

A reference guide does not need artificial workflow states or output files when
it only owns policy. It must still explain how consumers apply the policy and
what happens when evidence conflicts.

### 8.2 Schema or data-structure guide

Required:

```text
schema or contract version
identity and ID rules
field names, types, required/optional/nullability, defaults
allowed values, ranges, units, and cross-field invariants
serialization shape and canonical storage path
runtime/Unity mapping and lookup ownership when applicable
unknown-field and backward-compatibility behavior
valid and invalid non-executable examples
validation and failure behavior
```

Do not infer serialized fields solely from example JSON when code or a canonical
schema exists.

### 8.3 Workflow or pipeline guide

Required:

```text
actors and single-owner rules
input and precondition contract
ordered stages
state transitions
outputs and immutable/mutable ownership
handoff payload
failure types, retry and idempotency rules
manual approval and external-cost gates
completion conditions
```

Do not combine independently executable stages merely to call them one
pipeline. A pipeline guide may coordinate stages while keeping each stage's
mutation authority separate.

### 8.4 Evaluation guide

Required:

```text
evaluation target and evidence package
observable categories and items
item scale and category/overall calculation
weights or explicit arithmetic-average rule
thresholds and rounding
N/A eligibility and average behavior
severity definitions
Critical and absolute Hard Fail rules
verdict precedence and conditional outcomes
required actions and optional improvements
report format
re-evaluation scope and completion rule
```

If using 100 points, category weights must total 100. If using arithmetic
averages, explicitly state which items/categories are included and how N/A is
excluded. A Critical finding or Hard Fail must override a numeric passing score.

### 8.5 Slack Canvas guide

Required:

```text
authoritative evaluation-report source
form version and identity
common field and section mapping
staging/evaluation/project-target path separation when artifacts are promoted
verdict and promotion-status mapping
evidence links or hashes
update, re-evaluation, and change-log behavior
explicit rule that Canvas recording does not recalculate the verdict
```

Do not make a recording form a second evaluation authority.

### 8.6 Hybrid guide

Include all applicable type-specific contracts and an explicit section that
explains:

```text
why responsibilities are inseparable
which subsection owns each decision
how consumers avoid executing the wrong responsibility
how future separation or extension is handled
```

## 9. Contract Writing Rules

### 9.1 Deterministic identity and storage

When the guide creates files or records, define:

```text
canonical project-relative directory
filename and ID grammar
case convention
extension
version placement
collision and overwrite behavior
index/update ownership
```

Use placeholders only for variable components. Do not place an absolute local
workspace path in a reusable contract.

### 9.2 States and failures

- use stable machine-readable status and failure names when downstream work
  branches on them;
- define eligible transitions and terminal states;
- distinguish an operational failure from an evaluated failure;
- say whether a failed operation creates no output, preserves a partial result,
  or writes a failure record;
- define retry ownership and prevent duplicate work when relevant.

### 9.3 Examples

- label examples as valid, invalid, illustrative, or non-executable;
- do not let an example introduce a field or behavior absent from the contract;
- prefer one representative example over multiple copied variants;
- never disguise a copy-ready task prompt as an “example” inside a guide.

### 9.4 Language and terminology

- use one stable term for each actor, artifact, state, and ID;
- define unavoidable abbreviations;
- separate normative rules (`must`, `must not`) from recommendations;
- avoid subjective words without observable criteria;
- preserve the canonical capitalization and path case.

## 10. Cross-guide Consistency

Before completion, compare the target only with exact guides that share its
contract. At minimum check, when applicable:

```text
PromptAuthoringGuide -> prompt/guide role separation and prompt consumption
GuideEvaluationGuide -> required quality and readiness criteria
ContentFolderStructureGuide -> Assets/Contents and Assets/ImagesGenerated paths
domain schema/runtime guide -> fields, IDs, lookup, and serialization
workflow neighbors -> stage ownership, states, and handoff payload
evaluation/Slack guides -> verdict authority and recording-only boundary
```

Do not copy a common rule into every domain guide. Reference the owner and add
only the domain delta. If a repeated rule must be locally visible for safety,
name the authority and state that the local text is a non-authoritative summary.

## 11. Maintainability and Versioning

- assign one owning guide per shared contract;
- prefer exact references over duplicated policy;
- define an extension point instead of adding undocumented special cases;
- version schemas, persisted records, forms, and external handoff contracts;
- keep a guide unversioned only when changes do not alter a persisted or
  machine-consumed contract;
- state compatibility, migration, and stale-record behavior when a versioned
  contract changes;
- update indexes and direct consumers when a path or contract is renamed;
- do not bulk-rewrite unrelated guides as part of a local authoring request.

## 12. Authoring Failure Types

Use these or a domain-specific stable extension when the guide cannot be safely
completed:

```text
missing_authoring_requirement
invalid_guide_location
ambiguous_guide_type
missing_source_of_truth
unreadable_required_reference
authority_contract_conflict
runtime_contract_conflict
stale_path_contract
insufficient_domain_evidence
guide_role_boundary_conflict
```

On failure, report the target path, unresolved contract, inspected authorities,
files not created or modified, and the required owner decision or next action.
Do not create a placeholder guide that appears authoritative.

## 13. Completion and Self-review

Before handing a guide to evaluation, verify every applicable item below.

### Location & Role Separation

- [ ] The guide is under `AgentDocs/planning-guides` in the owning domain.
- [ ] No copy-ready task prompt is embedded in the guide.
- [ ] Unrelated workflow or implementation responsibilities are separated.

### Purpose & Scope

- [ ] Purpose, consumer, guide type, domain, applicability, prerequisites, and
      exclusions are explicit.

### Source of Truth & Reference Priority

- [ ] Authorities are assigned by concern using exact current paths.
- [ ] Material conflict has a stop/escalation rule.

### Contract Completeness

- [ ] Applicable inputs, outputs, identity, paths, states, failures, validation,
      and handoffs are complete for the declared guide type.
- [ ] A consuming task prompt does not need to invent a material rule.

### Cross-guide Consistency

- [ ] Shared terms, IDs, filenames, paths, states, and ownership agree with
      exact common and domain guides.
- [ ] Legacy prompt/guide roots are not active contracts.

### Safety & Boundary

- [ ] Mutation, manual approval, external cost, Unity, Git, Slack, and
      deployment boundaries are explicit when applicable.
- [ ] Independently owned pipeline stages are not silently bundled.

### Evaluation Rule Quality

- [ ] An evaluation guide has a complete score model, observable criteria,
      severity, Hard Fail, verdict, and re-evaluation contract.

### Maintainability

- [ ] No stale path, broad directory reference, unexplained optional source, or
      unnecessary duplicated policy remains.
- [ ] Extension and version behavior is documented when needed.

### User/Agent Readiness

- [ ] The consumer knows what to do, validate, return, and not do.
- [ ] Material ambiguity ends in a defined blocker or owner decision.

The completed guide should then be evaluated read-only with:

```text
AgentDocs/planning-guides/prompt/GuideEvaluationGuide.md
AgentDocs/task-prompts/prompt/GuideEvaluationReportPrompt.md
```

Do not silently revise the guide during its evaluation task. Apply evaluation
feedback in a separate authoring revision.

## 14. Related Documents

```text
AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
AgentDocs/planning-guides/prompt/PromptEvaluationGuide.md
AgentDocs/planning-guides/prompt/GuideEvaluationGuide.md
AgentDocs/task-prompts/prompt/GuideEvaluationReportPrompt.md
```
