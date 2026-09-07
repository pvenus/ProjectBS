# Charge 쿨다운 입력 후 Basic starvation rootfix 완료

blocker 없음. Unity GUI/headless/Play/import, staging/commit/push는 실행하지 않았다.

## Root cause 및 추적

기존 ManualControlCore는 Available만 확인하고 Charge keydown을 pending 슬롯에 저장했다. 이때 AttackHeld를 `physicalHeld && !hasPending`으로 false 처리하고 Basic press snapshot도 지웠다. 뒤의 Busy/Ready gate에서 실행 불가를 확인해 return하므로 pending이 남고 Basic 경로에 도달하지 못했다. 진행 중인 콤보는 generation 취소가 없어도 continuation callback=false가 되어 다음 단계를 중단할 수 있었다.

adapter.Ready → CharacterSkillManager.ManualReady는 ManualBusy(코루틴 ticket, cast, cast-move, combo, attack animation), IsTargetable/CanUseSkill 및 IsRuntimeReady(실행 실패 retry/cooldown/SkillRuntimeData)를 검사한다. 이 유효성 검사보다 앞서 Core의 priority 상태를 변경한 것이 원인이다. 별도 MP/resource cost 시스템은 추가하지 않았으며 기존 readiness의 실패를 모두 비소비 거부로 처리한다.

root-cause-replay.json은 수정 전 보존한 실제 Core와 수정 후 실제 Core를 동일한 gameplay stub 입력으로 각각 컴파일·실행한 결과다. Basic held 상태에서 쿨다운 Charge를 50 tick 반복하고 Busy를 해제했다.

| 상태 | 수정 전 | 수정 후 |
| --- | --- | --- |
| held | false | true |
| pending | true | false |
| current combo continuation | false | true |
| Basic 실행 횟수 | 1 | 2 (repress 없음) |
| cancel/generation-clear 요청 | 0 | 0 |

## 수정 정책

- 활성 스킬 대기열을 제거했다. PendingSlot은 항상 0이다.
- Busy/current atomic 또는 Available/Ready 실패 keydown은 즉시 버린다. cooldown, resource/readiness, invalid 입력은 Basic held/press aim/현재 콤보를 preempt하지 않는다.
- Fire=false도 Basic 상태를 지우지 않는다. held LMB는 같은 tick에 이어 실행할 수 있으며 실제 Busy가 있으면 다음 실행 가능 tick에 자동으로 이어진다.
- Ready이고 Fire=true인 스킬만 우선권을 얻는다. 실패한 후보 때문에 Clear/Cancel/generation 변경을 호출하지 않는다.
- adapter의 Fire 전 StopAllMotion을 제거했다. 불가/invalid 후보가 이동을 먼저 중단하지 않는다. 성공한 입력은 Core의 movement zero 및 기존 cast-move ownership 경로를 사용한다.
- Basic 자체의 press snapshot 및 cooldown 대기는 유지한다. 폐기한 active keydown을 나중에 재생하지 않는다.

## Charge 데이터 교정

G1/G2/G3 Charge는 Direction 유지, aimInputSource만 KeyboardMoveDirection으로 변경했다. keydown WASD 정규화 → 마지막 유효 이동 입력 방향 → 현재 facing → right. 마우스는 무시한다. 해당 3개 JSON/SO의 입력원 값만 바꿨다.

SwiftStep/기타 슬롯, Basic cooldown 1.000 및 G1 combo cadence/body/VFX/cooldown 시작 시점, NPC 실행은 그대로다. 이전 교정 이후의 CharacterSkillManager·ActiveSkillService 전체 SHA와 Charge 이외의 21개 변경 이력 자산 SHA가 동일함을 확인했다. preservation-audit.json 참조.

## 검증

기존 211개 검사 범위를 유지하며 대기열 제거 정책에 맞게 기존 deferred-input 기대값을 수정했다. 7개 rootfix 검사와 1개 수정 전/후 재현을 추가했다.

- input/core/owner 40, schema/materializer/helper 15, combo/guard 15, teardown 10, MORPG 62, geometry 14, Dash clock 9, reward 12, spawn 10, NPC cast 4, P1 27, differential replay 1: 총 219/219 통과.
- runtime C# 오류 0 / 경고 31, Editor 오류 0 / 경고 41.
- cooldown spam, Fire=false, invalid, readiness/resource 거부 시 pending 0·소비/취소 0·Basic 같은/다음 tick 재개·repress 0·Basic cooldown 중 중복 실행 0 확인.
- JSON parser/materializer/SO API 12개 슬롯 반복 readback 통과. Charge 3개의 Mode=1/Source=2, SwiftStep Source=2, Basic cooldown=1.000을 exact SO YAML readback으로 확인.
- 보호 파일 455개 SHA 동일. 누적 승인 범위의 equipment metadata 12개 및 Basic cooldown 3개는 승인 필드 외 동일. index 동일. 이번 교정은 Charge 3개 입력원만 바꿨다.
- git diff --check 및 scoped/rootfix 전용 역패치 dry-run 통과. 실제 rollback·삭제 없음.

검증은 실제 production 소스와 테스트용 gameplay/serialization adapter 기반이다. 실제 Unity Play, import/reimport 또는 AssetDatabase 저장 검증을 했다고 주장하지 않는다.

## 증빙 및 복구

rootfix-05/에 before 원본·before/after SHA·rootfix.patch·root-cause-replay.json·materialized-so-readback.json·REPORT/receipt를 보존한다. 누적 작업 범위는 기존 41개/신규 22개 파일이며 상위 scoped.patch는 공유 작업 트리의 다른 변경을 포함하지 않는다. 복구 전 현재 SHA를 비교해야 한다.

추가 반복 없이 현재 결과로 종료한다. 완료 알림은 한결에게만 전달한다.
