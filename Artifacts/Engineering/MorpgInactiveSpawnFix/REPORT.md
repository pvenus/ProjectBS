# Inactive NPC spawn coroutine fix

원인: MORPG가 NPC를 inactive staging parent 아래에서 초기화하는데, CharacterManager.InitializeFromSO가 마지막에 PlaySpawnRevealNextFrame 코루틴을 즉시 시작했다. Unity는 activeInHierarchy=false인 GameObject의 코루틴 시작을 거절한다. 기존 데이터 초기화는 유효했으나 reveal 시작 시점이 잘못됐다.

## 변경

- Assets/Scripts/Actor/Character/CharacterManager.cs: InitializeFromSO와 Initialize 모두 reveal 요청만 기록한다. isActiveAndEnabled일 때만 시작하고, inactive이면 OnEnable에서 한 번 시작한다. OnDisable에서는 진행 중 reveal을 취소하고 요청을 보존한다. 재초기화는 이전 reveal/사망 코루틴, isDying, lastHitAttacker를 정리해 pooled reuse가 이전 생명 상태를 물려받지 않게 한다.
- Assets/Scripts/Battle/Spawn/Sequence/service/NpcSpawnService.cs: 초기화→소유권 준비→부모 분리/활성화→활성·runtime 검증→추적 등록 순서를 유지한다. 준비 거절 또는 예외 시 sequence/living 등록을 철회하고 객체를 비활성화·폐기한다. 일반 active spawn은 기존 호출 형태와 reveal 시점을 유지한다.
- AgentTools/MorpgIntegrationHarness/inactive_spawn_tests.py: production NpcSpawnService 전체와 CharacterManager의 실제 lifecycle 메서드를 소스에서 추출해 코루틴 활성 조건을 강제하는 모형에서 실행한다. 초기화 진입점 두 곳이 새 helper를 사용하는지도 검사한다. 실제 스탯/스킬 초기화 전체나 Unity 엔진을 대신하는 테스트는 아니다.

## 검증

- 실제 Unity Roslyn/프로젝트 참조 정적 컴파일: 오류 0, 기존 경고 31.
- inactive spawn regression: 8/8 (black·chain staging, black·chain pooled reuse, active legacy, 초기화 실패, ownership 거절, registry observer 실패).
- MORPG live adapter 모형: 7/7.
- P0/P1 focused: 27/27, 결정적 추적 SHA 유지.
- 대상 파일 git diff --check 통과.
- Unity GUI, staging, commit, push 사용 안 함.

재현: `python3 AgentTools/MorpgIntegrationHarness/inactive_spawn_tests.py`, `python3 AgentTools/MorpgIntegrationHarness/compile.py`, `sh AgentTools/MorpgIntegrationHarness/run.sh`.

## 사용자 재확인

1. black_cloth_raider와 chain_axe_enforcer 첫 스폰에서 inactive coroutine 오류가 사라지는지 확인.
2. 두 적의 스탯·스킬·등장 연출이 정상이며 inactive staging 상태에서는 적이 타깃/전투에 노출되지 않는지 확인.
3. 같은 pooled root를 비활성화→재초기화→재활성화했을 때 reveal이 한 번만 재생되고 이전 사망 상태/연출이 남지 않는지 확인.
4. MORPG 19→5→4 구역 진행과 최종 승리가 계속 정상인지 확인.
5. 일반 non-MORPG 스폰도 기존과 같이 등장하는지 확인.

이번 수정 이후 MORPG 전체 정적·모형 회귀까지 이어서 검증했다. 실제 Unity 플레이 및 기존 보고서에 명시한 P2 픽업/저장·재진입 보상 복구 범위는 여전히 별도 검증/구현 대상이다.
