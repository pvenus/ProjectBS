# Dash + character fade consistency rootfix

**DASH_CHARACTER_FADE_CONSISTENCY_ROOTFIX_APPLIED**. 사용자가 관찰한 “Dash만 보이거나 screen fade만 보이는 전환”의 경로를 수정했습니다. 이전 horizontal revision03의 실제 좌표/예약/P2/sorting은 유지합니다.

## 원인과 변경

기존 정상 경로는 임의 clear 위치에서 먼 고정 exit까지 이동했습니다. 캐릭터가 고정 카메라 밖으로 나간 뒤에야 마지막45% 페이드가 진행될 수 있었습니다. 또한 출발점부터 전체 raw corridor 폭을 요구하여 경계 가까이의 합법적인 clear 위치도 실패로 판정했고, 무필터 Rigidbody cast가 잔여 NPC/투사체를 장애물로 처리했습니다. fallback에서는 스킬 body 재생을 호출하지 않아 화면 페이드만 남았습니다. 정상 body 호출 자체도 CC와 facing owner 조건의 영향을 받았습니다.

- **짧은 계산 경로:** `MorpgDashExitPath`가 현재 clear 위치에서 revision03 중앙선으로 이어지는 짧은 경로를 계산합니다. 중앙에서는 약2.5wu의 오른쪽 출구 동작입니다. 경계 근처에서는 짧은 보정 이동을 포함할 수 있습니다. 경로를 실제 actor radius + actorInset으로 연속 sweep하며, 보이는 위치 점프를 하지 않습니다. 먼 고정 앵커까지 횡단하던 원인을 제거했습니다.
- **명확한 character alpha:** 정상 exit35%부터 끝까지 smoothstep fade-out, 정확한 alpha0 retained simulation frame, entry .22s fade-in을 사용합니다. 카메라 고정/투명 원자 컷/.36s entry/.16s settle/잠금 해제 후.20s wave 순서는 유지합니다.
- **Dash 표시 분리:** 선택된 원본 clip의 sprite/flip-only 곡선을 전환 LateUpdate에서 body renderer에 직접 sample합니다. 스킬 호출, cooldown 소비, facing lock 획득 없이 정상·보조 경로 모두 오른쪽 Dash 자세를 유지합니다. AnimationMono에는 읽기 전용 body renderer 참조만 추가했습니다. 기존 상태이상을 제거하지 않으며, settle에서 기존 Idle 복귀를 요청합니다.
- **구조물만 차단:** 활성 environment structural root 소유의 enabled, non-trigger collider만 path/landing의 실시간 차단물로 인정합니다. 잔여 NPC/투사체 및 다른 소유자는 제외합니다. cast 결과는 크기 제한 배열 대신 재사용 List로 받아 앞쪽의 무관한 hit가 진짜 구조물 hit를 가리는 문제도 방지합니다.
- **한 방향 모드:** NormalDash에서 필요한 경우 AssistedDash로 한 번만 이동합니다. AssistedDash는 Dash 자세와 character fade를 계속 보여 주며, alpha가0이 된 뒤에만 screen cover로 원자 위치 전환을 보조합니다. .32s fade-out 보조 구간에서 character fade .22s가 먼저 보이고, 도착 cover는.06s에 걷히므로.22s character fade-in이 보입니다. 다시 정상 모드로 돌아가거나 반복 fallback으로 시계를 재시작하지 않습니다.
- **명시적 NonDash:** 초기 ReducedMotion 또는 실제 clip/body renderer 부재에서만 선택합니다. AnimationMono 부재나 CC/기존 body owner 거부는 Dash 표시를 없애는 조건이 아닙니다. CC가 정상 lifecycle로 풀리기를 최대.50s 더 기다린 뒤에도 막혀 있으면 AssistedDash를 사용합니다.

## 진단 로그

정산 대기가 끝나 실제 전환 표시가 시작될 때 `[MORPG transition]`을 한 번 기록합니다. source/destination ordinal, mode, reason, clear 위치, exit 길이가 포함됩니다. 이후 구조물 때문에 NormalDash→AssistedDash가 필요할 때만 `[MORPG transition assistance]`를 추가로 한 번 기록합니다. 샘플은 `diagnostics.log`에 있습니다. ordinal0→1은 Z1→Z2,1→2는 Z2→Z3입니다.

## 검증

| 검사 | 결과 |
|---|---:|
| 실제 Unity C# 컴파일 | 오류0 / 기존 경고31 |
| Live route/view/environment, Unity API stub | 40/40 |
| Production transition clock | 9/9 |
| Geometry | 14/14 |
| Reward queue | 12/12 |
| Inactive spawn | 10/10 |
| NPC cast | 4/4 |
| P1 권한·fault·dedup | 27/27 |
| 총 결정적 검사 | **116/116** |

추가 핵심 근거:

- Z1/Z2의 .50wu 격자에서 actor radius.35 + inset.15로 합법적인 clear 위치 **1,762개**를 검사했습니다. 전부 경로가 존재하고 경로 sample이 구조물/경계를 침범하지 않았습니다.
- top/bottom/center/right-edge clear 위치의 live 전환이 NormalDash를 유지했고, exit 중간 alpha가.3..8 범위이며 캐릭터가 고정 카메라 안에 있음을 검사했습니다.
- 잔여 NPC/투사체 collider의 path/landing false obstruction0.
- 실제 구조물 소유 blocker는 AssistedDash를 한 번만 선택하며, Dash sample과 character alpha가 계속 존재합니다. entry 도중 보조 전환도 두 번째 CAS 없이 처리합니다.
- CC 및 body 재생 거부 상황에서도 Dash sample이 나오며 상태이상·기존 facing 값을 제거하지 않습니다. ReducedMotion/clip/renderer 부재는 명시적 NonDash입니다.
- 출구35%와 중간·끝 alpha, 진입 alpha, pause, alpha0 checkpoint, 카메라와 wave 순서, 복원 검사가 통과했습니다.
- revision03 `canonicalHashes`와 현재 environment/route SHA가 동일합니다. 선택 Dash clip도 원본 SHA 동일이며 sprite/flip 곡선 외 position/scale/event가 없습니다.
- `git diff --check`, 이번 패치 reverse-check PASS. Git index hash 변경0. 새 파일은 exit planner와 그 meta뿐입니다.

이 검사는 실제 생산 C# 코드를 사용하는 정적/결정적 시뮬레이션입니다. Unity API의 물리/렌더링은 스텁이며, Unity GUI·Play Mode·GPU 화면 검증을 수행하지 않았습니다. 사용자 기기에서의 육안 확인은 별도입니다. 씬 편집, staging, commit, push는 수행하지 않았습니다.

## 롤백과 전달

`receipt.json`, `dash-consistency-only.diff`, `before/`, `rollback-manifest.json`, 각 `*.log`를 함께 확인합니다. 현재 hash를 확인한 뒤 이번 패치만 역적용하면 수정 전 Dash 상태로 돌아가며, 다른 공유 dirty 변경과 가로형 좌표를 reset하지 않습니다. reverse-check는 실제 파일을 되돌리지 않았습니다.

한결에게 전달할 완료 자료입니다. 앞선 상세 보고 전송은 자동 승인 검토가 대상 작업 및 내부 내용의 전달 권한을 확인하지 못해 거절했습니다. 이번 자료를 우회 경로로 전송하지 않았으며, 신뢰할 수 있는 명시적 전달 승인이 필요합니다.
