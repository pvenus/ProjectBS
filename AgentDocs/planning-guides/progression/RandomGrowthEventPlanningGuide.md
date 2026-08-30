# Random Growth Event Planning Guide

## 1. Authority and status

- Canonical path: `AgentDocs/planning-guides/progression/RandomGrowthEventPlanningGuide.md`
- Design owner and approver: 벼리 — 게임 기획 디렉터
- Scope approver: 이음 — 프로젝트 프로듀서
- Status: **G0 design authority approved**
- Contract version: `random-growth-event-design.v1`
- Runtime implementation remains unauthorized until the producer opens G1.

This document is the canonical game-design authority for the Chapter 1 P0
random growth event. Technical implementation, scheduling, and visual approval
remain owned by 한결, 이음, and 화감 respectively.

## 2. Player experience

The event adds one optional risk decision between episode 5 and episode 6. A
player who accepts a clearly disclosed, nonlethal party-wide HP cost receives
one PartyWide roguelike growth opportunity. The event must strengthen both the
pleasure of weighing a story choice and the pleasure of adapting the build to a
random growth offer.

The fixed Chapter 1 growth opportunities remain guaranteed. The random event
is an optional additional opportunity and never replaces them.

## 3. Non-negotiable P0 rules

| Rule | Authority value |
|---|---|
| Fixed growth applied cap | `2` |
| Random growth applied cap | `1` |
| Chapter total growth applied cap | `3` |
| Absolute appearance chance | `40%` (`4000` basis points out of `10000`) |
| Logical event count | At most `1` per run |
| Encounter count | At most `1` per run |
| Growth pool mode | `PartyWide` |
| Risk cost | Every current party member loses `ceil(MaxHP * 0.10)` HP |
| Survival requirement | Every member must have at least `1` HP after paying the full cost |
| Decline result | No HP cost and no growth opportunity |
| Persistence | Current in-memory run/session only; no file save in P0 |

The HP floor is an eligibility condition, not a clamp. A party member may pay
only when `CurrentHP - ceil(MaxHP * 0.10) >= 1`. If any member cannot pay the
full amount, the risk choice is disabled and no member loses HP.

## 4. Placement contract

The event belongs between the existing episode 5 branches and episode 6:

- `sec_ep_5_1_to_ep_6`
- `sec_ep_5_2_to_ep_6`

These are mutually exclusive route sections. One logical reservation is
mirrored into both sections at the same logical ordinal. Only the node on the
route actually entered may count as encountered. The off-route reservation is
neither an encounter nor a consumed appearance.

The absolute 40% roll is owned by the Chapter random-growth manifest. Existing
placement weights, pool weights, `oneShot`, or `cooldownRounds` are not the
authority for this probability or cap.

## 5. Stable ID allow-list

The following IDs are immutable and exhaustive for the P0 event. Aliases and
display-name-derived alternatives are forbidden.

| Kind | Allowed stable ID |
|---|---|
| Pool | `event_pool.act1.random_growth.cheongun_sangui` |
| Event | `event.act1.random_growth.01.crying_bell_smithy_trial` |
| Stage | `stage.act1.random_growth.01.crying_bell_smithy_trial` |
| Node | `node.act1.random_growth.01.crying_bell_smithy_trial.intro` |
| Risk choice | `choice.act1.random_growth.01.crying_bell_smithy_trial.take_heated_talisman` |
| Decline choice | `choice.act1.random_growth.01.crying_bell_smithy_trial.leave_forge` |
| Progression segment | `progress.segment.act1.chapter01.random_before_episode06` |
| Reservation definition | `reservation.act1.chapter01.random_growth.before_episode06` |

The canonical authoring inputs are:

- `Assets/Contents/Stage/json/stage_chapter1.json`
- `Assets/Contents/Stage/json/episode5_1.json`
- `Assets/Contents/Stage/json/episode5_2.json`
- `Assets/Contents/Stage/json/episode6.json`

The existing event
`event.act1.random_event.16.crying_bell_smithy` is reference material only. Its
JSON, SO, GUID, story result, and final runtime image must remain unchanged.

## 6. Run identity contract

### 6.1 `runId`

- Owner: `GameSession` new-run initialization boundary.
- Creation: generated exactly once when a new playable run is committed, before
  any Chapter graph or random-growth manifest is generated.
- Identity: an opaque, globally unique, immutable value. Display names, time,
  scene names, and chapter node IDs must not be used as the ID.
- Lifetime: the complete in-memory run across Stage and Battle scene changes.
- Reset: only an explicit new-run/restart-run operation or destruction of the
  P0 session creates a new value. Stage reload, scene re-entry, retry, route
  selection, or UI reopening must not reset it.
- P0 persistence: it is not restored after application restart because file
  save is outside P0.

### 6.2 `stageGenerationId`

- Owner: `StageSession` at the first committed construction of the Chapter 1
  graph for the current `runId`.
- Creation: exactly once for the `(runId, chapterId)` pair, before manifest roll
  and slot assignment.
- Relationship to a run: Chapter 1 has exactly one `stageGenerationId` within a
  P0 run. All Stage scene reloads, graph rebuild requests, and route transitions
  must reuse it and the already-built manifest.
- Regeneration: a new value is permitted only after a new `runId` is created.
  A missing or corrupt manifest inside the same run must not create a new ID or
  reroll; the random event is suppressed for that run and the error is recorded.

This prevents graph re-entry from becoming a free appearance reroll.

## 7. Generator version contract

The initial manifest generator version is:

```text
chapter1.random_growth_manifest.v1
```

The version is part of deterministic seed input and manifest evidence. It must
be bumped only when at least one of the following changes:

- canonical seed serialization or PRNG algorithm;
- absolute-roll interpretation;
- mirrored reservation or slot-selection semantics;
- manifest schema in a way that changes generated identity or placement.

Content prose, display names, art, UI layout, and balance telemetry do not bump
the generator version when generation output is unchanged.

P0 has no cross-version save compatibility or alias migration. An active
session whose stored manifest version differs from the running generator must
not regenerate or translate the event. It records an incompatible-version
failure, suppresses the random event, and allows the main Chapter route to
continue. A newly started run uses the currently approved version.

## 8. Reservation identity contract

The final reservation definition ID is accepted as:

```text
reservation.act1.chapter01.random_growth.before_episode06
```

It is globally unique among canonical reservation definitions. Its runtime
instance uniqueness key is:

```text
(runId, stageGenerationId, reservationId)
```

Exactly one runtime instance may exist for that key. The left and right section
placements are mirrored projections of this one instance, not two independent
reservations. They share appearance, encounter, result, and cap state.

## 9. Choice, result, and reward boundary

The new event's choice reward arrays must remain empty:

```text
rewards = []
```

The risk and decline choices use typed result execution after confirmation.
They do not use the current immediate Gold/reward dispatch. This progression
entitlement is not a new `rewardType` in `RewardPlanningGuide.md`.

Risk result commit must atomically record the full party HP cost, the stable
result receipt, and one Pending PartyWide growth entitlement. Duplicate cause
delivery applies no additional cost or entitlement. Failure rolls back the
whole transaction.

Decline commits only the decline result: cost `0`, growth `0`.

Applying a growth card consumes the entitlement only after the selected skill
is increased by exactly one level. Application failure keeps the fixed offer
and Pending entitlement and never reapplies the HP cost.

## 10. Narrative and visual distinction

The new event is titled **우는 쇠종의 시련**. It may inherit the Joseon forge,
ink, hanji, and fire-material language of event 16, but it must use a distinct
close composition centered on an empty forge, the crying iron bell, and a
heated safety talisman that must be extracted with tongs.

It must not reuse event 16's final main image, character grouping, composition,
reward meaning, or runtime identity. The safety talisman is a trial catalyst,
not a collectible relic.

Approved player-facing choice meanings:

- Risk: `달아오른 안전패를 꺼낸다`
  - `파티 전원: 최대 HP의 10% 피해 (올림)`
  - `모든 파티원이 비용 전액을 지불하고 HP 1 이상 남아야 합니다.`
  - `시련 성공 시: 성장 정비 1회`
- Decline: `대장간을 떠난다`
  - `피해 없음 · 성장 없음`

The result leads to the approved PartyWide fixed 3/2/1-card offer and does not
promise a particular character or skill.

## 11. G0 validation gates

G0 passes only when all of the following are evidenced without interpretation:

- this file is the single canonical design authority;
- every ID exactly matches the allow-list and no alias exists;
- the real episode files are `episode5_1.json`, `episode5_2.json`, and
  `episode6.json` under `Assets/Contents/Stage/json`;
- run, stage-generation, generator-version, and reservation identities follow
  Sections 6–8;
- the existing event 16 JSON/SO hash and GUID remain unchanged;
- the new choices use empty reward arrays and typed result execution;
- fixed, random, and total caps are `2`, `1`, and `3`;
- implementation remains blocked until the common progression ledger, cap, and
  fixed-offer interfaces are separately accepted by 한결 and opened by 이음.
