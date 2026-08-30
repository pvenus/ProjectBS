# Stage Reward–Risk Value Framework

- Definition unit: `stage-reward-risk-definition.v1`
- Coefficient version: `stage-value-coefficients.v1`
- Row schema: `stage-reward-risk-row.v1`
- Owner: 벼리 — 게임 기획 디렉터
- Status: design authority; audit-only
- Scope snapshot: Chapter 1 event catalog, 2026-08-30

## Purpose and boundary

This framework compares the immediate utility, conditions, costs, and variance of Stage/Event choices. It does not alter gameplay numbers, content copy, event JSON/SO, or runtime reward authority. An EV band is an audit classification, not a rarity, recommendation, or UI grade.

The definition unit consists of this guide, `StageRewardRiskAudit.schema.json`, and `chapter1-events21-46.audit.json`. The three artifacts share the definition, coefficient, and schema versions above.

## Stage Value Unit (SVU)

| Component | Reference quantity | Base SVU | Rule |
|---|---:|---:|---|
| Gold | 25 | 1.0 | Grant positive, spend negative |
| Party HP | 10% MaxHP for all living members | 1.0 | Use effective heal after clamp; cost is negative |
| Party MaxHP | 10% for all members for the run | 3.0 | Persistent value; no direct grant in Events21–46 |
| Growth | PartyWide, targetCount 2 | 4.0 | Apply eligibility and optional-cap factors |
| Growth | PartyWide, targetCount 3 | 5.0 | Keep granted and applied distinct |
| Relic | Fixed/shared Common, one | 4.0 | Apply owned/capacity eligibility |
| Relic | Deterministic pool, one | 4.5 | Only for a validated pool |
| Route information | Immediate successor purposes | 1.0 | Information only; no edge mutation |
| Favorable route commit | Shortest/useful successor | 2.0 contextual | Requires graph-snapshot override evidence |
| Dangerous route commit | Longest/Battle-purpose successor | -1.0 contextual | Follow-up reward is separate |
| Run flag without consumer | 0 realized, 0.5 OV | OV is option value and is excluded from realized EV |
| Run flag with consumer | Effect-equivalent | Consumer ID/version required |
| Ordinary Battle entry | -2.0 baseline | Low confidence until win-rate, HP loss, and time data exist |
| Actionable information | 0.5–1.0 | Must arrive before the decision it changes |

Narrative and ethical meaning are retained as `NV` tags and are never erased by SVU. A flag without a verified downstream consumer cannot be valued like Gold, healing, growth, or a relic.

## Risk axes

Each axis uses 0–3.

| Axis | 0 | 1 | 2 | 3 |
|---|---|---|---|---|
| Probability | guaranteed | at least 80% | 50–79% or state-dependent | below 50% or unmeasured RNG |
| Severity | none | at most 1 SVU | over 1 through 3 SVU | over 3 SVU or run-build damage |
| Irreversibility | immediately reversible | same-node retry | run-persistent | save/long-term persistent |
| Delay/uncertainty | immediate | next node | later section | timing/effect unknown |
| Recoverability | full | easy recovery | scarce recovery | unrecoverable |

`RewardEV = Σ(probability × baseSVU × delayFactor × eligibilityFactor)`

`CostEV = deterministicCostSVU + Σ(probability × severitySVU × irreversibilityFactor × recoverabilityFactor)`

`NetEV = RewardEV - CostEV`

Delay factors are immediate 1.0, next node 0.9, later section 0.75, unknown 0.5. Eligibility is measured telemetry when available; an unmeasured state condition uses provisional 0.75. Irreversibility factors are immediate 1.0, run 1.15, long-term 1.30. Recoverability factors are easy 0.85, ordinary 1.0, difficult 1.15, none 1.30. Opportunity cost is reported as the delta from the best alternative and is not added twice.

## Bands and variance

- EV: S `>=6`, A `4–<6`, B `2.5–<4`, C `1–<2.5`, D `0–<1`, E `<0`.
- Variance: V0 deterministic or span below 0.5 SVU; V1 state/clamp span below 1.5; V2 conditional/Battle span 1.5–4; V3 RNG/route/relic span above 4 or unmeasured.
- Low-confidence Battle and contextual route rows must retain a range-like band and a warning; they cannot be promoted to a precise number without an override.

Standard warnings: `DOMINANT_DELTA_GT_2`, `NO_REALIZED_REWARD`, `FLAG_NO_CONSUMER`, `CAP_OR_OWNED_STALE`, `BATTLE_MODEL_LOW_CONFIDENCE`, `ROUTE_CONTEXT_REQUIRED`, `REPEAT_FARM`, `PARTIAL_COMMIT`, `REWARD_COPY_RUNTIME_MISMATCH`, `CHOICE_DISABLED_ALL`, `DELAYED_CLAIM`.

## Row and override contract

The machine-readable schema is authoritative for fields. Every row records stable event/node/source-popup/choice IDs, source path and receipt, intent, purpose, outcomes, cost/risk, EV band, variance, confidence, repeat/exclusion policy, terminal behavior, warnings, and audit state.

An override requires `overrideId`, scope, base coefficient version/value, replacement value or range, rationale, evidence, owner, approver, review gate, and affected rows. Valid reasons include measured Battle difficulty, relic rarity/synergy, actual route length, a verified flag consumer, and measured party/clamp behavior. Copy importance or visual rarity is not evidence.

## Version, staleness, and recalculation

- Any coefficient, threshold, multiplier, or formula change marks all active 48 event rows `STALE_COEFFICIENT`.
- Recalculation order is coefficient freeze → all event/choice rows → override review → diff report → warning/dominance regression → producer acceptance.
- An outcome change stales its row plus the same repeat/exclusion group, purpose quota, and reward-source peers.
- IDs cannot be aliased. Removal leaves a tombstone for historical comparison.
- Runtime/content disagreement is `BLOCKED_AUTHORITY_MISMATCH`; the audit must not invent a reward to reconcile it.

## Inventory snapshot and progress

Read-only inventory at the snapshot:

- `Assets/Contents/Stage/json/event/act01/*.json`: 48 definitions.
- Normalized nodes: 51 total; Events21–46 contain 27.
- General events: 46; random-growth definitions: 2.
- Serialized `nodes[].choices`: 129.
- Audited here: Events21–46, 26 events and 58 choice/result rows.
- Remaining: 22 events and 71 serialized choices, status `NOT_AUDITED`.

Portfolio registry exclusions, chain variants, and original16/Smithy mutual exclusion remain status fields; they do not silently change the physical 48/129 inventory snapshot.

Actual outcome authority was cross-checked against `/private/tmp/projectbs-current-hangyeol-stage-event-outcome-inventory.jsonl` SHA-256 `aa5d543d2262c455a70ae00de72eaacacd70fa8f7229718750cdba123d1d5c1f`. Event/node/choice IDs and counts match. Document-level `sourcePopupId` is absent in the event JSON and is therefore `null` in the audit; ordinary Battle tuples retain their nested source-popup identity. Execution operations and payloads remain owned by each `contentPath` and the normalized inventory digest, while repeat/exclusion/retry/cooldown remain runtime-contract authority.

## Acceptance rules

- Same canonical outcome under the same coefficient version produces the same audit value.
- Guaranteed and conditional rewards remain separate.
- Non-executable metadata is never counted as a reward.
- A consumerless flag remains realized EV 0 plus OV only.
- Route/Battle rows remain contextual/low-confidence until evidence exists.
- Overrides without rationale, evidence, approval, and affected rows fail validation.
- A choice delta over 2 SVU raises a warning; it does not automatically change content.
