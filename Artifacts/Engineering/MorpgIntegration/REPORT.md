# MORPG same-path integration

현재 로컬 체크아웃에 실제 코드를 병합했다. 기존 수정본을 `before/`에 기록한 뒤 그 상태 위에 연결했다. 이번 작업만의 패치는 `integration-only.diff`에 있다. Git index 해시는 작업 전후 동일하다. Unity GUI, staging, commit, push는 실행하지 않았다.

P1의 authority API SHA `a63a43386771e681562d4233a40258b2dfa25f2d8d90cd7c920c9a0c68741647`은 기존 revision-02 manifest에서 확인했다. 기존 isolated 인터페이스/토큰/핸들/코디네이터 파일은 수정하지 않고 live adapter가 이를 구현한다.

## 변경 파일

- `Assets/Scripts/Battle/Core/BattleManager.cs`: exact feature flag, MORPG→v3→legacy 분기, MORPG 최종 승리 조건을 기존 CompleteBattle 경로에 연결, 카메라 smoothing 초기화.
- `Assets/Scripts/Battle/Spawn/Sequence/BattleSpawnManager.cs`: live attempt 소유·tick·HUD·teardown.
- `Assets/Scripts/Battle/Morpg/BattleMorpgLiveRoute.cs` (신규): definition/addendum/producer/account 사전 검사, 19→5→4 활성 구역 생성, 다음 프레임 clear 판정, 0.50 fade/0.68 warp/1.05 unlock/1.25 next wave, token 검증, 구역 경계와 player/camera 이동, 실패 시 legacy 재생 금지, 보상 정산 및 최종 승리 요청.
- `Assets/Scripts/Battle/Morpg/MorpgOwnedObject.cs` (신규, meta 포함): root/projectile exact ownership, idempotent disposal, 예기치 않은 적 despawn 실패 처리.
- `Assets/Scripts/Battle/Spawn/Sequence/service/NpcSpawnService.cs`: 기존 호출 호환 optional inactive staging/등록 callback. activation 전 root 등록.
- `Assets/Scripts/Ability/Skills/Services/ProjectileFactory.cs`: owned projectile activation 전 등록, interval coroutine의 source scope 폐기 후 지연 생성 차단. 기존 non-MORPG 생성 경로 유지.
- `Assets/Scripts/Battle/Map/BattleMapBoundsContext.cs`: 기존 arena 위에 active-zone actor clamp 추가.
- `Assets/Scripts/Actor/Character/CharacterManager.cs`: MORPG root에 한해 기존 gold drop 중복 억제.
- `Assets/Scripts/Actor/Character/service/CharacterDamageService.cs`: 전환 토큰 소유 player의 damage 진입 차단.
- `AgentTools/MorpgP0Harness/P1RuntimeTests.cs`: authorized adapter가 존재할 때 기존 no-live-wiring 조건 대신 manager 분기 순서와 victory gate 검사. 기존 순수 P1 경계 검사 유지.
- `AgentTools/MorpgIntegrationHarness/`: 실제 live adapter 소스를 실행하는 headless Unity 모형 테스트 및 실제 Unity 정적 컴파일 재현 스크립트.

## 검증

- Unity 6000.3.10f1의 실제 Roslyn 및 프로젝트 response-file 참조로 현재 Assembly-CSharp 정적 컴파일: **오류 0, 경고 31**.
- 작업 직전 파일을 대입한 baseline 컴파일: **오류 0, 경고 31**. 경고 메시지 목록 동일.
- P0/P1 focused harness: **27/27**, 반복 실행 trace SHA `19c159bebac2905bc8afa1213e3ed9c5331154d845c1ffedfd45c51105f7f355` 유지.
- 실제 live glue를 Unity 모형에서 실행: **7/7**. exact/disabled/missing gate, 19→5→4/warp/40G/10XP/dedup/final, pause/teardown unlock, final-frame defeat 우선, spawn failure, XP notification fault rollback, owned projectile cleanup/foreign preservation/late producer rejection.
- 대상 tracked 파일 `git diff --check` 통과. 새 코드 trailing whitespace 없음. `.git/index` 작업 전후 동일.
- 로그: `build.log`, `build-baseline.log`, `focused.log`, `live-tests.log`, `verification.json`.

재현:

```sh
python3 AgentTools/MorpgIntegrationHarness/compile.py
sh AgentTools/MorpgIntegrationHarness/run.sh
sh Artifacts/Engineering/MorpgP1Rollback/revision-02/REPRODUCE.sh
```

## P2 범위와 남은 검증

P2는 사망별 bundle dedup, black 1G/0.25XP와 chain 3G/0.75XP 즉시 정산, raw XP, 기존 gold drop 억제, 최종 XP payload 억제, 누적 보상/구역 HUD까지 연결했다. 별도 월드 pickup/orb·흡수 애니메이션은 구현하지 않았다. 기존 CurrencyManager 알림 이벤트 대신 이 HUD가 정산 누적치를 표시한다.

Unity 엔진의 실제 Awake/OnEnable/사망 애니메이션/physics/입력 연동은 아직 플레이 검증하지 않았다. headless 모형 테스트는 이를 대신하는 엔진 검증이 아니다. Projectiles의 공용 factory/interval 생성 경로를 연결했으며, 그 밖의 별도 hostile effect/delayed-hit producer를 추가할 때도 동일한 구역 registry 등록이 필요하다.

기존 저장 시스템의 nonterminal durable-save queue / reload 시 pre-battle checkpoint 복원 전체 연결은 이번 변경에 포함하지 않았다. 현재 P2 계정은 run-local 실제 계정에 즉시 반영하므로, 씬 재진입·중단 후 보상 보존/복구 정책은 출시 전 별도 검증·연결이 필요하다. 이 결과는 static integration 완료이며 전체 live/P2 acceptance 완료를 뜻하지 않는다.

## 사용자 전투 테스트 체크리스트

1. `battle.act1.chapter01.01.rescue_villagers` 진입 시 왼쪽 entry에서 시작하며 다른 구역 적이 먼저 나오지 않는지 확인한다.
2. 왼쪽 19명 전멸 후 fade→중앙 warp→입력 복구→중앙 5명 순서와, 중앙 전멸 후 오른쪽 4명 순서를 확인한다.
3. 전환 중 이동·시전·피해가 차단되고, 이전 구역 적/투사체가 남지 않으며 다른 소유 객체가 삭제되지 않는지 확인한다.
4. 일시정지 중 spawn/transition 시간이 흐르지 않고 해제 뒤 이어지는지 확인한다. 전환 중 종료 후 다시 입장해 잠금이 남지 않는지 확인한다.
5. 오른쪽 최종 전멸에서만 기존 승리/스킬 업그레이드 경로로 넘어가며 HUD 총합이 28명, +40G, +10XP인지 확인한다. 기존 gold coin이 중복 생성되지 않아야 한다.
6. 마지막 적과 player 동시 사망에서는 승리가 나오지 않아야 한다. 생성/보상 실패 시 legacy 적이 다시 생성되면 안 된다.
7. feature flag OFF, definition 누락/손상, 다른 battle ID에서 기존 v3/legacy 전투 동작을 확인한다.
8. 저장/재진입 검증은 위 P2 미연결 범위를 고려해 별도로 수행한다.
