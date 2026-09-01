# Battle Encounter & Monster Scaling Framework v1

- 상태: DESIGN_DEFINITION_ONLY
- 소유자: 벼리
- 계약: `battle-encounter-monster-scaling.v1`
- 권위 영수증: `/private/tmp/projectbs-current-byeori-battle-encounter-monster-scaling-framework-v1.txt` (SHA-256 `94d67706e6e58bacddd73a252f18bb92d3ef1661f29a4858c95efe9cc362c540`)
- 범위: 현재값 감사 + exact4 파일럿 목표. Full23 rollout, runtime/content/stat/spawn mutation은 승인되지 않았다.

## 1. 권위와 현재/목표 경계

현재 권위는 Battle JSON/SO 23개, NPC Character JSON 21개, `elimination_90s_swarm` sequence family다. 현재 metadata의 spawn count는 very_easy 28, easy/normal/hard 80, very_hard 108이지만 sequence 파생치는 각각 28, 76, 76, 76, 104다. 양쪽을 모두 보존하며 80→76, 108→104를 `COUNT_METADATA_RUNTIME_MISMATCH`로 기록한다. 어느 쪽도 이 definition 단계에서 수정하지 않는다.

`baselineCurrent`는 현행 관찰값이고 `targetV1`은 미적용 설계값이다. target을 runtime 사실처럼 소비하면 실패다. live cap, threshold trigger, 중앙 stat scaler가 구현·검증되기 전에는 `enabled=false`, `STALE_RUNTIME_GAPS`로 fail-closed한다.

## 2. 목표 감각과 예산

일반 전투 목표는 75–105초, spawn activity 45–60초, cleanup 15–30초다. 첫 적은 조작권 후 0.5–1.5초, 개별 입장 간격은 0.35–0.75초, wave 휴지는 2–5초다. 동시 생존 상한은 Easy 16, Normal 20, Hard 24이며 이 상한은 deterministic planned count와 별개의 live gate다.

Normal dense-swarm 목표는 Early 88, Mid 96, Late 104다. Easy는 69/75/81, Hard는 102/110/119다. 수량은 동시 폭발이 아니라 짧은 간격, 역할 교대, 제한된 증원으로 체감된다.

## 3. TU와 scaling

`UnitTU = RoleTU × StatThreat × AbilityThreat × SpatialExposure`

`StatThreat = .35×EHP + .40×EDPS + .10×Speed + .10×Control + .05×Range`

`EncounterTU = Σ(UnitTU) × OverlapFactor × CadenceFactor`

Difficulty→phase→약한 party-growth→role clamp→slope cap→TU/live-cap feasibility→deterministic rounding 순서를 고정한다. Party growth는 HP .95–1.06, ATK .96–1.04, Count .95–1.05 이내이며 mid-battle adaptive scaling은 금지한다.

인접 phase 상한은 HP +10%, ATK +8%, DEF +6%, Move/AttackSpeed +3%, cooldown 감소 5%, Count +15%, EncounterTU +20%다. 몬스터 역할의 강점·약점 순서를 뒤집을 수 없다.

## 4. spawn archetype

- `burst_opening`: 읽을 수 있는 빠른 개시. 첫 1.5–2.5초에 live cap의 35–45%.
- `drip_pressure`: 0.35–0.60초 간격의 지속 압력.
- `alternating_roles`: melee→ranged/support→melee/tank 역할 교대.
- `flank_reveal`: 전면 접촉 뒤 2.0–3.5초 예고된 단일 측후방.
- `reinforcement_call`: 총수의 15–25% 단일 증원. runtime 미지원 경고 유지.
- `threshold_escalation`: 잔존 수/elite HP threshold 기반. runtime 미지원 경고 유지.

Burst+flank+elite 동시 2초, support≥2와 tank>25%, flank 중 ranged>30%, cap 예약 없는 threshold+reinforcement는 금지한다.

## 5. exact4 파일럿

1. `battle.act1.chapter01.01.rescue_villagers`: Early burst→drip, elite 0.
2. `battle.act1.chapter01.03_2.training_ground_ambush`: Early 경계, front→flank.
3. `battle.act1.chapter01.05_1.training_ground_breakout`: Mid alternating+reinforcement.
4. `battle.act1.chapter01.06.bandit_fort_assault`: Late alternating/threshold set piece.

Audit의 나머지 19개는 baseline만 기록하고 `FULL23_ROLLOUT_NOT_AUTHORIZED`다.

## 6. RewardRisk·composition 의존성

Chapter1 `DirectBattle4 / Shop2 / Rest2 / Event4`와 phase floor 1/2/1을 참조한다. Event battle은 Event node 의미를 유지하며 DirectBattle floor를 대체하지 않는다. PartyHP 10%=1 SVU, ordinary battle -2 SVU는 low-confidence baseline이고 측정 전 보상 자동 변경에 쓰지 않는다.

## 7. 결정성·stale·override

Seed domain은 length-prefixed `(runId, stageGenerationId, battleId, encounterOrdinal, coefficientVersion)`다. Unity.Random, time, string hash를 금지한다. coefficient/formula/clamp/role TU 변경은 23 encounter와 21 monster 전부 stale; reward-risk 변경은 EV만 stale한다. 재계산은 coefficients→monster→encounter→TU/TTK simulation→override review의 원자 단위다.

Override는 scope, base/override, rationale, evidence, owner, approver, review gate를 가져야 한다. 스토리 중요성만으로 stat inflation을 정당화할 수 없다.

## 8. 수용 기준과 rollback

동일 seed diff 0, live-cap 위반 0, duplicate spawn 0, forbidden pressure collision 0, adjacent slope cap 위반 0을 요구한다. 100 valid runs/cell 또는 deterministic simulation+30 playtests 전 target 승격을 금지한다.

Rollback은 `enabled=false`로 기존 CharacterSO/SpawnSequence/BattleSO bytes를 사용한다. scaled snapshot은 session-local이며 CharacterSO에 쓰지 않는다. 새 version은 active old manifest를 재해석하거나 reroll하지 않는다.

## 9. 파일 관계

- 이 문서: 인간 권위.
- `schema/BattleEncounterScaling.schema.json`: 구조·행 수 gate.
- `Assets/Contents/Battle/balance/battle-encounter-scaling.v1.json`: coefficient/version/stale 단일 소스.
- `audit/chapter1-battle-encounters.v1.json`: encounter23 + monster21 baseline/pilot audit.

README/index 병합, staging, Unity 실행은 이 definition unit에 포함되지 않는다.
