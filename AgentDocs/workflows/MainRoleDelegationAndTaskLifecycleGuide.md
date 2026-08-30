# Main Role Delegation and Task Lifecycle Guide

## 1. Purpose

This guide defines how ProjectBS's persistent main-role tasks direct work,
delegate independent production units, preserve context through complete
handoffs, review results, and archive temporary tasks after their assets are
accepted.

The persistent main roles are:

- 가온 — 프로젝트 비서
- 벼리 — 게임 기획 디렉터
- 이음 — 프로젝트 프로듀서
- 화감 — 프로젝트 아트 디렉터
- 한결 — 프로젝트 개발 리드

Main-role tasks are durable authorities. They must not become monolithic worker
tasks that research, produce, evaluate, integrate, and report every artifact by
themselves.

## 2. Main-role operating model

A main role owns:

- direction and non-negotiable constraints;
- decomposition and dependency order;
- selection of the right existing or temporary task;
- complete handoff contracts;
- domain quality gates and final acceptance;
- decision records, unresolved risks, and the next handoff.

A main role does not normally own:

- every independent production unit;
- repetitive asset generation;
- large batches of mechanical edits;
- independent audits that can run in parallel;
- implementation and evaluation at the same time when separation improves
  evidence quality;
- temporary evidence after its accepted result has been promoted.

The main role may execute a small unit directly when delegation overhead is
greater than the work, the unit cannot be separated safely, or the main role's
own judgment is the deliverable. The reason must be stated in the start plan.

## 3. Mandatory start plan

Before substantive work, the main role states:

1. its role and decision responsibility;
2. the project goal and quality bar applied to the request;
3. work units and dependencies;
4. what it will decide directly;
5. what will be delegated to existing persistent tasks;
6. what requires new temporary tasks;
7. which units may run in parallel;
8. acceptance evidence for every unit;
9. integration and final review order;
10. archive conditions for temporary tasks.

Planning must lead directly to execution when the work is authorized and safe.

## 4. Reuse, continue, or create

Choose the execution target in this order.

### 4.1 Continue an existing persistent task

Use an existing main-role task when the work belongs to its durable authority
and the accumulated context improves judgment. Send a complete handoff message
that actively starts the task; a document link alone is not a handoff.

### 4.2 Continue an existing specialized task

Reuse an existing specialized task only when all of the following are true:

- its authority and asset scope still match;
- its prior result is relevant and not a closed legacy execution;
- continuing it will not mix unrelated work;
- it can receive a new complete handoff without ambiguous ownership.

### 4.3 Create a temporary task

Create a new task when the unit is independent, bounded, and benefits from
parallel execution or isolated context. A temporary task must have one primary
deliverable, one owner, explicit write boundaries, acceptance evidence, and an
archive condition.

Do not create one task per prompt by default. Create a task for a coherent
production responsibility. Prompts and guides are inputs, not task identities.

## 5. Parallelization rules

Parallelize only units whose inputs are stable and whose writes do not overlap.

Good parallel units include:

- independent asset batches after one approved specification;
- read-only audits of different domains;
- implementation and test-scenario preparation with separate file ownership;
- static validation and visual reference preparation;
- two content units with different canonical IDs and output paths.

Keep work sequential when:

- a downstream unit depends on an unapproved design or art specification;
- two tasks would edit the same JSON, SO, Prefab, scene, registry, or index;
- generation must use an approved prior image;
- evaluation must inspect final integrated runtime output;
- one decision changes the contract for every downstream unit.

Before parallel dispatch, record the dependency gate, exact file ownership, and
the result expected from each task. Parallel speed never overrides authority,
data safety, provenance, or final quality.

## 6. Complete execution handoff

Every dispatched unit must receive a self-contained message containing:

1. sender, receiver, task identity, and primary owner;
2. purpose and why the handoff occurs now;
3. approved decisions and non-negotiable constraints;
4. necessary project and player context;
5. exact input paths and the role of each input;
6. exact execution scope;
7. exclusions, forbidden changes, and write boundaries;
8. expected outputs and canonical destinations;
9. acceptance and verification evidence;
10. dependencies, risks, and unresolved facts;
11. autonomous decision range and re-consultation conditions;
12. required completion receipt;
13. next handoff and archive condition.

The receiver must be able to start without hidden conversation context. If the
contract is incomplete, it reports the exact missing fields instead of guessing.

## 7. Production and review separation

When practical, production and final evaluation are separate responsibilities.

- The producer returns artifacts, provenance, changed paths, validation, and
  known limitations.
- The domain main role evaluates the result against the approved contract.
- The producer must not self-declare domain approval unless the handoff makes it
  the explicit evaluator.
- Failed or conditional results return through a new bounded correction
  handoff; they do not silently expand the original task.

The main role integrates receipts rather than repeating all production work.

## 8. Temporary task lifecycle

Every temporary task uses these states:

```text
created
-> ready
-> executing
-> delivered
-> under_review
-> accepted | correction_required | blocked
-> promoted
-> archived
```

Archive a temporary task only when:

- its expected artifact or analysis was delivered;
- the responsible main role accepted it;
- canonical files were promoted or the final decision was recorded;
- provenance and verification evidence are recoverable;
- no correction or downstream question remains assigned to that task;
- the completion receipt was handed back to the coordinator or next owner.

After these conditions are met, the coordinator archives the temporary task
using the app's task archival action. Archival is part of completion, not an
optional cleanup step.

Do not archive:

- persistent main-role tasks;
- a task awaiting user approval;
- a task with unreviewed output;
- a task whose artifact has not reached its canonical location;
- a task that owns an unresolved correction or failure investigation.

If a task is replaced, record the successor task before archiving the old one.

## 9. Quality and speed policy

Use speed to reduce waiting, not to remove gates.

The preferred pattern is:

```text
main-role decision
-> stable specification
-> parallel bounded production
-> independent evidence collection
-> domain review
-> integration
-> completion receipt
-> temporary-task archive
```

Batch size must remain reviewable. Run a representative pilot before a large
batch when style, schema, runtime behavior, or output quality is not already
proven. Expand only after the pilot passes.

Quality is measured by contract compliance, runtime fitness, reproducibility,
and integration evidence, not by the number of generated artifacts.

## 10. Role-specific directing duties

### 가온

Routes work, prevents duplicate tasks, tracks receipts, detects missing
handoffs, coordinates cross-role conflicts, and archives accepted temporary
tasks. 가온 does not replace domain approval.

### 벼리

Defines game intent and design contracts, delegates bounded planning analysis,
and accepts design results. It does not absorb schedule, art approval, or code
implementation into its documents.

### 이음

Decomposes milestones, assigns owners, identifies parallel lanes, controls
scope, operates gates, and verifies that accepted units advance release goals.

### 화감

Defines visual specifications and reference authority, delegates bounded asset
production and audits, and performs or commissions independent final visual
review before promotion.

### 한결

Defines technical contracts and file ownership, delegates independent
implementation, tests, and read-only audits when safe, then integrates only
verified changes. Shared-checkout conflicts take precedence over parallelism.

## 11. Completion receipt

Every temporary task returns:

- task identity and final status;
- produced or changed paths;
- provenance and source references;
- tests, visual checks, or evaluation evidence;
- deviations from the handoff contract;
- unresolved risks;
- recommended next owner and action;
- whether its archive conditions are satisfied.

The receiving main role records acceptance, correction, or rejection. Only an
accepted and promoted result qualifies for automatic archival.

