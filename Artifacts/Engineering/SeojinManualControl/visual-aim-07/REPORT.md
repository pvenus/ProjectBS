# visual / aim correction 07 완료

Shift Dash/Charge 몸체의 방향 회전 폴백을 제거하고 Basic 콤보를 단계별 마우스 스냅샷으로 변경했다. blocker 없음.

## 몸체와 VFX

SkillDirectionPresentationProfile은 8방향 등록 clip, 수평 반전 가능한 clip, 좌우 facing/flip 폴백 순서로 선택한다. 몸체 폴백 angle은 항상 0이다. canonicalForward로 몸체 atan2를 계산하던 경로를 제거했다. 캐릭터 root 및 원본 body renderer Transform에 회전 쓰기를 추가하지 않았고, 몸체 표시 자식의 Z도 0이다. Shift/Charge keydown WASD 및 기존 fallback/게임플레이 이동은 유지했다.

Charge VFX는 renderer 전용 자식에만 스냅샷 atan2-canonicalForward 회전을 적용한다. caster, collider, Rigidbody와 원본 renderer root의 회전은 바꾸지 않는다. Basic의 표시되는 단계 VFX도 해당 단계 Direction 데이터를 이 동일한 renderer 전용 경로에서 사용하도록 했다. 기존 Basic VFX 001 suppression은 유지한다. 반환·비활성·취소 시 rotation/flip/local transform 복원 계약은 유지했다. Self 및 비수동 projectile에는 방향 표시 flag가 없다.

## Basic 단계별 스냅샷

ManualGameplayAdapter → CharacterSkillManager → ActiveSkillService가 Func<ManualSkillAim> 공급자를 전달한다. 공급자는 각 단계의 실제 시작 시간과 generation/held gate 통과 후 한 번 호출된다. index0 A, index1 B, index2 C가 각각 local currentStepAim으로 확정된다. step hit, projectile/VFX context 및 facing lock 모두 같은 값을 사용하며 hit 시 재샘플하지 않는다.

유효한 마우스 → 현재 WASD → 직전 단계의 정확한 facing 벡터 순서로 fallback한다. 첫 단계는 현재 AnimationMono facing을 사용하며 모두 없으면 right다. 직전 단계의 수직 벡터를 대각선 enum으로 양자화하지 않는다. release 시 다음 공급자를 호출하지 않고 현재 단계만 끝낸다. 수동 Direction combo의 공통 런타임 경로로 G1/G2/G3에 적용했다. G1 cooldown은 index2 진입, G2/G3는 기존 완료 시점 그대로다.

단계별 facing lock을 사용하고 clip 샘플 뒤에 원본 및 combo 표시 renderer의 flip을 함께 보정한다. 취소 및 iterator dispose에서 소유자에 해당하는 lock만 해제해 과거 generation의 정리가 새 실행을 덮어쓰지 않는다. readiness 거부는 callback/held 상태를 바꾸지 않는다.

## 검증 결과

392개 통과, 실패 0. 기존 323개 범위에 새 검증 69개를 추가했다. 이전 body 회전 폴백 검증의 기대값은 이번 upright 요구에 맞게 Z=0으로 갱신했다.

| 범위 | 개수 |
| --- | ---: |
| 실제 input/core/adapter 대역 실행 | 51 |
| 실제 combo/guard 추출 실행 및 step fallback | 53 |
| teardown | 10 |
| aim pipeline | 15 |
| rootfix differential replay | 1 |
| MORPG / 환경 / dash | 62 / 14 / 9 |
| reward / spawn / NPC cast / P1 | 12 / 10 / 4 / 27 |
| 실제 presentation utility 및 post-sample flip 추출 실행 | 124 |

8방향 body root/renderer Z=0 및 VFX-only rotation, pool 복원, G1/G2/G3 A→B→C와 midstep 고정, release/취소 뒤 미호출, 원본·proxy flip 보정, invalid/epsilon fallback, cooldown 거부 시 provider 유지 등을 확인했다. 대역 기반 테스트이며 Unity 화면/전체 lifecycle 실행 검증은 아니다.

실제 C# compiler: runtime 오류 0 / 경고 31, editor 오류 0 / 경고 41. MORPG 소스 SHA 동일. 보호 자산 455개 동일, 앞서 승인된 설계 필드 자산 15개 유지. 누적 scoped patch 기존 44개 / new 27개. diff, revision 및 누적 patch reverse dry-run 통과.

Unity GUI/headless/Play/import, staging/commit/push, 실제 rollback 실행은 모두 0회다. Unity 화면상 최종 외관 확인은 수행하지 않았다.

## 재현 및 복구

check-00.log ~ check-14.log에 각 명령 결과를 보존했다. 콤보 추가 검증은 `python3 AgentTools/SeojinManualHarness/combo.py --step-aim`, 표현 검증은 `python3 AgentTools/SeojinManualHarness/visual_direction.py`로 실행한다.

correction.patch는 이번 변경, rollback.patch는 이번 변경의 역패치다. before-sha.json / after-sha.json으로 파일을 확인할 수 있다. 상위 scoped.patch는 전체 누적 변경이다. 실제 rollback은 수행하지 않았다.

## Close review 보강

입력 공급자는 UI/focus, pause, explicit auto, transition, disable 및 현재 수동 권한을 입력 read 이전에 확인한다. 콤보 routine도 시작과 hit 직전에 live 권한을 확인한다. Core의 suspended 진입은 Clear(true)로 epoch/generation을 취소하므로 UI가 열렸다 닫혀도 과거 실행이 되살아나지 않는다. 기존 generation guard 및 소유자별 finally 정리가 유지된다.

첫 provider 호출이 Fire와 같은 Time.frameCount이면 이미 확보한 initialAim을 재사용해 ray를 추가로 읽지 않는다. 첫 단계가 다음 프레임으로 지연되면 새 입력을 읽는다. 이후 각 단계는 현재 WASD를 우선하고 직전 단계의 정확한 facing을 fallback으로 사용한다. full-combo 최초 방향을 고정하지 않는다.

추가 10개 검증: update 이전 UI/pause/auto/transition/disable 공급자 호출의 ray read 0, 같은 프레임 initial 재샘플 0, 지연 시작 fresh sample, hit/다음 step 권한 거부 2건, UI suspension의 실제 cancel 호출. 총 input 51, combo 53, 전체 392개 통과.
