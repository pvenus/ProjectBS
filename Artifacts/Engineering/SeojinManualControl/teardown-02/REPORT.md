# 서진 기본 Manual 제어 완료

Player 서진만 시작 시 Manual 소유자로 바인딩한다. 기존 AI 컴포넌트는 보존하며, 명시적 Auto 선택에서만 캡처한 enabled 상태대로 동작한다. MORPG 전환, 사망, 스턴·루트·넉백·강제 타깃, 컷신은 별도 제어 사유로 처리하며 일반 AI 명령을 허용하지 않는다. 대기·타깃 없음·공격 입력 해제로 Auto가 되지 않는다.

## 변경 동작 (마우스 Dash 교정 반영)

- WASD 8방향: 반대키 상쇄, 대각선 정규화. PartyMovementMono의 기존 이동 속도·애니메이션 경로 사용.
- 좌클릭: 누르고 있으면 기존 G1 0→1→2 콤보, 해제하면 현재 원자 단계의 타격·회복을 완료하고 다음 단계 중단. 최초 press의 마우스 스냅샷은 쿨다운 대기 중에도 보존한다. 완료·쿨다운 후 물리 입력이 계속 눌렸을 때 새 마우스 스냅샷으로 다음 콤보를 반복한다.
- Shift: 좌·우 Shift keydown 시 공통 clamp SSOT를 거친 마우스 월드 지점과 caster 위치로 정규화 방향을 고정한다. 마우스는 반대 WASD보다 우선한다. 변환 실패 또는 caster와 거리 0.01 이하일 때만 WASD 정규화 → 마지막 유효 facing → 오른쪽 기본값으로 대체한다. 대기 중 마우스와 caster가 이동해도 캡처한 방향을 바꾸지 않는다.
- 1/2/3: keydown 시 공통 AimSnapshot의 마우스 월드 지점을 고정한다. 현재 MORPG walkable/맵 경계로 clamp하며 이후 마우스나 타깃을 추적하지 않는다.
- 실행 대기 슬롯은 최대 1개이며 새 keydown으로 교체된다. 쿨다운·실행 준비를 기다리고 중복 Fire/소비를 발행하지 않는다. 없는 런타임은 입력 단계에서 거부한다.
- UI 포인터·선택 포커스, 앱 포커스 상실, 일시정지, 전투 종료는 입력을 0으로 만든다. 제어권 변경은 버퍼를 비우고 복귀 프레임을 0으로 소비한 뒤 다음 프레임에 물리 입력을 다시 읽는다.
- CC·사망·전환 취소는 실행 세대를 무효화한다. 중첩 코루틴의 이전 지연 타격과 이전 콤보의 새 콤보 정리 간섭을 막는다.
- OnDisable은 캡처한 AI `pair.Value`를 복원하고 이동 입력을 0으로 반환한다. OnEnable 재구성은 반복 가능하며 Update 이전의 오래된 Auto 권한도 초기화한다.

## 구조와 범위

입력 판독은 `SeojinInputReader`, 순수 입력·소유권 상태는 `ManualControlCore`, Unity 연결 및 게임플레이 호출은 `SeojinManualControl`과 어댑터로 분리했다. 기존 CharacterSkillManager/ActiveSkillService의 쿨다운, G1 시간표, body 동기화, VFX 억제 0/0/1과 3타 진입 쿨다운을 재사용한다. 수동 point 모드는 자동 타깃 재탐색을 하지 않는다. NPC에는 수동 컴포넌트를 바인딩하지 않고 기존 일반 코루틴 경로를 유지한다. 새 패키지 의존성은 없다.

수정 기존 파일 10개와 신규 파일 15개의 정확한 목록·해시는 `before-sha.json`, `after-sha.json`, `new-files.json`에 있다. 새 Unity Control 폴더와 스크립트 메타를 포함한다. 전체 공유 작업 트리의 기존 변경을 포함하지 않는 이번 작업 전용 차이는 `scoped.patch`이다.

## 검증

| 검사 | 통과 |
| --- | ---: |
| 실제 입력·core·owner 파일 + 엔진 스텁 | 21/21 |
| 실제 콤보·실행 guard 메서드 추출 실행 | 14/14 |
| MORPG live glue | 62/62 |
| 환경 geometry | 14/14 |
| Dash transition clock | 9/9 |
| 보상 delivery | 12/12 |
| inactive spawn | 10/10 |
| cast presenter | 4/4 |
| P1 기존 회귀 | 27/27 |
| 비활성 teardown 실제 메서드 | 10/10 |
| 합계 | 183/183 |

Unity 설치의 Roslyn과 실제 프로젝트 참조를 사용한 C# 컴파일: 오류 0, 기존 경고 31. 전체 컴파일 로그는 `build.log`, 각 테스트 로그는 같은 폴더에 있다. `git diff --check` 및 `git apply --reverse --check scoped.patch` 통과. NPC 소스, 서진 스킬 자산, MORPG Resources, Git index를 포함한 보호 해시 470개 모두 작업 시작 상태와 동일하다.

Unity GUI/headless/Play는 실행하지 않았다. 입력·생명주기는 스텁 환경이며, 콤보 테스트는 실제 메서드 본문을 추출하고 타격/애니메이션 끝점을 모의한다. 따라서 실제 프레임 렌더링, VFX 표시와 물리 동작의 Play 검증 결과로 해석하지 않는다. 누락 body clip의 게임플레이 fallback 및 null VFX 전달은 모의 경계에서 확인했다.

## 교정 감사

기본 공격 경로 `FireManualAtPoint → FireSkillAtSnapshotPoint → FireComboRoutine`은 target=null과 usePoint=true로 고정한 지점을 전달한다. 각 단계의 ResolveDirection 및 ApplyAnimationDirection, UseSkillOnce까지 스냅샷을 유지하고 자동 재타깃 경로는 실행하지 않는다. G1 cast 자산의 targetingType=AutoTarget(2)와 SkillUseHelper 지점 전달도 확인했다. 기존 타깃 기반 NPC 경로와 1/2/3 슬롯 동작은 바꾸지 않았다.

추가 검사는 반대 WASD보다 마우스 우선, epsilon/변환 실패 fallback, 경계 밖 clamp, 대기 중 마우스·caster 변경에도 방향 불변, basic press 대기 스냅샷, 반복 cycle 재캡처, 실제 ResolveDirection/ApplyAnimationDirection의 3단계 좌향 전달, 슬롯 1/2/3 보존을 포함한다. 교정 전 원본과 교정 전용 역패치 dry-run은 `correction-01/`에 보존했다.

## 비활성 teardown 교정

기존 `OnDisable → SetOwnedManualInput(false, zero) → ApplyMovementControl → UpdateMovementAnimation → PlayIdle → PlayState → StartCoroutine` 경로를 제거했다. `ReleaseManualControlForTeardown`은 캡처한 movement control 상태를 복원하고 입력, 최근 입력 시간, 이동 phase/timer와 velocity만 동기로 정리한다. presentation을 호출하지 않는다. 외부 이동 소유권은 CastMoveService 등 해당 소유자가 해제한다.

Seojin OnDisable 전체를 AnimationMono의 중첩 가능한 동기 teardown 범위로 감쌌다. 활성 GameObject에서 control 컴포넌트만 끄는 경우에도 상태 Exit·cast 취소·AI 복원 콜백이 presentation 코루틴을 시작하지 못한다. finally에서 범위를 닫는다. 이미 파괴된 actor/skills/state 참조는 Unity null 검사로 건너뛴다.

AnimationMono의 PlayIdle/PlayMove/PlayState 및 clip 검증은 `isActiveAndEnabled && gameObject.activeInHierarchy`와 teardown 범위를 확인한다. 모든 presentation coroutine 시작은 단일 최종 gate를 통과한다. 비활성 요청은 기존 코루틴을 중단하고 state=None, clip/routine=null, one-shot·cast pose·facing lock과 combo presentation을 동기 정리한다. OnDisable/OnDestroy는 같은 reset을 재사용한다. 재활성화 때 이전 clip 동일성으로 재생이 누락되지 않으며 동일 Idle/Move 요청은 한 번만 시작한다.

인접 감사: MovementMono.StopAllMotion 및 PartyMovementExecutionService의 단일 StopMovement 경로는 velocity만 정리한다. CastMoveService.StopMove는 기존 코루틴을 중단하고 소유권을 반환하며 terminal 콜백은 위 teardown 범위 안에서 실행된다. AnimationMono의 공격 회복·콤보·CC·death 시작도 최종 gate에 포함했다.

검증은 활성 control disable, parent inactive, animation component disable, 직접 PlayState/인접 시작 요청, 반복 disable/destroy, 첫 활성 idle/move 각 1회, 중첩 정리 및 콜백 예외 후 범위 반환, legacy inactive movement toggle 10개다. 실제 production 메서드를 추출하고 inactive StartCoroutine 시 예외를 던지는 테스트 런타임으로 검증했다. 전체 183개는 통과했고 모든 animation coroutine call site가 gate를 사용하는 추가 소스 감사도 통과했다. Unity의 실제 객체 파괴·렌더링은 실행하지 않았다.

교정 직전 원본, 교정 전용 patch와 역패치 검사, report/receipt 사본은 `teardown-02/`에 보존했다.

## 재현과 복구

저장소 루트에서 `sh AgentTools/SeojinManualHarness/run.sh`, `python3 AgentTools/SeojinManualHarness/combo.py`, `python3 AgentTools/SeojinManualHarness/teardown.py`, `python3 AgentTools/MorpgIntegrationHarness/compile.py`로 신규 동작과 전체 컴파일을 재현한다. `python3 AgentTools/SeojinManualHarness/audit.py`는 보호 해시·현재 차이·역패치 적용 가능성을 검사한다.

각 파일의 첫 수정 직전 10개 파일 원본은 `before/`에 보존했다. 복구 대상은 `scoped.patch`와 `new-files.json`으로 한정한다. 역패치 dry-run만 수행했으며 실제 복구, 파일 삭제, stage/commit/push는 하지 않았다. 후속 공유 변경이 생기면 현재 해시와 `after-sha.json`을 비교하고 충돌을 먼저 검토해야 한다.

## 전달

자동 승인 검토가 다른 작업으로 내부 소스 경로·상세 상태를 전송하는 동작을 목적지 공개 권한 미확인 사유로 거절했다. 상세 결과는 이 로컬 보고서와 `receipt.json`에 보존했다.
