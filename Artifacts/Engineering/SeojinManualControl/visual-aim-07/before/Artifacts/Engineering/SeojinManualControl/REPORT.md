# Charge / SwiftStep 방향 표현 수정

게임플레이 입력 시점의 방향 스냅샷을 몸체와 Charge VFX 표현에 전달했다. blocker 없음.

## 원인과 변경

몸체는 SetDirectionFromVector 뒤에 body clip의 m_FlipX 샘플 및 재생 옵션이 반전을 덮어썼다. AnimationMono의 방향 몸체 API는 serialized SkillDirectionPresentationProfile의 8방향 클립, 수평 반전 가능한 클립, canonicalForward 기준 회전/반전 폴백 순서로 선택한다. 기본 canonicalForward는 오른쪽이다. 원본 renderer에서 애니메이션을 계속 샘플하고 전용 자식 renderer에 표시하므로 flip 곡선이 최종 방향을 덮어쓰지 않는다.

VFX는 ProjectileRuntimeData.direction을 갖고도 표시 회전에 사용하지 않았다. 수동 Direction 비콤보 실행에만 orientManualPresentation을 전달하고, renderer 전용 자식에 스냅샷 atan2와 canonicalForward의 각도 차를 적용한다. Rigidbody, collider, 원본 Transform 또는 게임플레이 방향을 변경하지 않는다. Self/일반 projectile/Basic combo에는 적용하지 않는다.

표현 lease는 시작 시 enabled/flip과 표시 자식의 로컬 위치·회전·스케일을 저장한다. 종료·취소·비활성·풀 반환 시 이를 복원한다. CharacterSkillManager.CancelManualExecution에서 몸체 표시를 먼저 복원하므로 MORPG scripted transition sampler가 원본 renderer를 사용한다. MORPG 소스의 변경 전후 SHA는 동일하다. 시각 리소스가 없으면 표현을 취득하지 않고 기존 게임플레이 경로가 계속된다.

프로필은 SkillCastSO의 직렬화 필드로 추가했다. 이번 변경에서 기존 SO/JSON/clip 자산은 수정하지 않았다. 기본 폴백이 기존 오른쪽 기준 클립을 처리하며, 별도 방향 클립을 연결하면 우선 사용한다.

## 검증

- 실제 SkillDirectionPresentation.cs를 Unity primitive 대역과 컴파일해 104개 assertion 통과: 8방향 몸체/VFX, 방향 클립 우선순위, mirror, custom axis, 샘플 flip 덮어쓰기, 방향 snapshot, root/child renderer, 반복 풀 복원, cancellation, missing renderer.
- 기존 입력·pipeline·combo·teardown·MORPG·환경·dash·reward·spawn·NPC cast·P1·rootfix replay 219개 회귀 검증 통과. 합계 323개 assertion/test, 실패 0.
- 연결부는 source audit로 확인했다. 새로운 AnimationMono/ProjectileVisual 전체 Unity lifecycle 실행 테스트는 수행하지 않았다.
- 실제 런타임 C# 컴파일 오류 0 / 경고 31, 에디터 오류 0 / 경고 41.
- 보호 자산 455개 동일, 이전 승인 설계 필드 자산 15개 유지. 누적 patch 기존 44개/new 26개. diff 및 reverse dry-run 통과.

Unity GUI/headless/Play/import, git staging/commit/push, 실제 rollback은 실행하지 않았다. 화면상 최종 외관은 Unity 실행 확인이 남아 있다.

## 산출물

- visual-direction.patch / rollback.patch: 이번 revision 범위 및 역패치.
- regression-00.log ~ regression-11.log: 기존 회귀 실행 결과.
- visual-tests.log / compile.log / receipt.json.
- 상위 scoped.patch: 전체 누적 변경.
