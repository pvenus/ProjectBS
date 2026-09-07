# 실제 runtime Hover 방향 rootfix 08 완료

## 정확한 원인

Resolver에서 설정한 orientManualPresentation이 ProjectileFactory.CreateInstanceRuntimeData 복제에서 누락됐다. 따라서 실제 생성된 projectile에서는 앞선 visual 방향 코드의 gate가 false였다. Hover 역시 runtime direction을 받지 않고 target−owner 또는 spawn−owner를 추론했으며, Charge followOffset=0에서는 owner.right로 귀결됐다. Factory spawn과 MoveController는 gameplay root에 회전을 쓰고, 앞선 ProjectileVisual 자식 회전은 MoveSO의 applyDirectionRotation/rotationOffset을 따르지 않았다. Warp instance DTO 복제도 해당 두 설정을 누락했다.

보존한 수정 전 Factory 메서드를 동일 하네스에 넣으면 manual flag 검증이 실패하고, 수정본은 통과한다(negative-control.log). 이번에 확인한 G1~G3 Charge 실제 clip은 rotation/euler 곡선이 비어 있고 root flip/sprite 곡선이 있다. 현재 문제를 clip rotation 덮어쓰기 때문이라고 단정하지 않는다. 새 계층은 향후 localRotation 곡선에도 방향 wrapper가 덮어써지지 않도록 분리한다.

## 최종 구조와 동작

실제 Resources/skill/ProjectileEntity.prefab은 ProjectileEntity → Scaler → Renderer(Animator + SpriteRenderer)다. 수동 Direction에서는 Scaler와 Renderer 사이에 __ProjectileDirectionRotation을 삽입한다. Animator clip의 local transform 쓰기는 Renderer에만 적용된다. wrapper world Z만 atan2(ProjectileRuntimeData.NormalizedDirection)+MoveSO.rotationOffset이며 applyDirectionRotation=false이면 offset도 무시하고 0이다.

Factory instance 복제가 방향 플래그를 보존한다. ProjectileMovement가 NormalizedDirection과 authoritative/visual-only 정책을 context에 전달하고 Hover는 수동 snapshot을 target/spawn 추론보다 우선한다. 수동 Direction에서 Factory spawn rotation은 identity, MoveController의 root 방향 회전은 비활성화한다. 재초기화 시 projectile root를 identity로 두고 movement controller를 재구성한다. ProjectileVisual의 이전 방향 자식 경로를 제거해 wrapper 한 곳만 각도를 소유한다. caster/collider에는 방향 회전을 적용하지 않는다.

Hover/Linear/Warp 수동 Direction은 동일 MoveSO flag/offset 정책을 사용한다. 비수동 Hover/Linear 등 기존 controller 경로는 유지하며 Warp clone은 flag/offset을 보존하도록 고쳤다. G1은 이름이 charge가 아닌 active_1.active_1.move.asset이며 실제 equipment MoveSO GUID를 따라 G1~G3 모두 Hover / applyDirectionRotation=true / offset=0임을 확인했다. SO/JSON/clip/prefab 자산은 편집하지 않았다.

wrapper는 원래 부모/sibling/local position/rotation/scale/flip과 기존 wrapper pose를 저장해 despawn/disable/reinitialize/destroy 시 복원한다. physics root 또는 physics component를 포함한 subtree는 wrapper 대상으로 거부한다. 현재 ProjectileEntity는 실제로 Destroy 기반이며 반복 reuse 계약은 대역 하네스에서 검증했다. 공용 prefab이 없는 root-bound Animator fallback은 안전하게 wrapper를 취득하지 않는다.

## 검증

기존 392 + 새로운 factory/hover/controller/wrapper assertion 229 + 수정 전 negative control 1 = 622 통과, 실패 0. 8방향 visible world forward, offset, apply=false 0, clip child rotation 쓰기, misleading target, physics/caster root 0, 반복 parent/local pose/flip 복원, legacy Hover, Warp clone 설정을 검증했다. 실제 production C# 및 추출한 Factory 메서드를 Unity primitive 대역과 실행했다. Unity 엔진 화면/Animator lifecycle 실행 검증은 아니다.

runtime compiler 오류 0 / 경고 31, editor 오류 0 / 경고 41. MORPG SHA 동일, 보호 자산 455개 유지, 이전 승인 설계 필드 15개 유지. diff 및 누적/revision 역패치 dry-run 통과. Unity GUI/headless/Play/import/staging/commit/push/실제 rollback 모두 0회.

hierarchy-audit.json: 실제 prefab/clip/MoveSO readback. check-00~16.log: 회귀/컴파일/감사 결과. correction.patch 및 rollback.patch: 이번 변경과 역패치. 상위 scoped.patch: 전체 누적 변경.
