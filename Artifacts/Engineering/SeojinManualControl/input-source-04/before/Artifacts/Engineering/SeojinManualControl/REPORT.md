# Skill aim mode SSOT 및 수동 입력 구현 완료

현재 결과로 종료한다. blocker 없음. Unity GUI/headless/Play, staging/commit/push는 실행하지 않았다.

## 계약

EquipmentSkillSO의 직렬화된 aimMode가 수동 실행의 단일 기준이다. JSON root → EquipmentSkillJson DTO → ApplySkillFields → ConfigureAimMode → 직렬화 enum 필드를 연결했다. 런타임 슬롯별 aimMode ID 하드코딩은 없다.

| Mode | 입력 및 실행 |
| --- | --- |
| Direction (1) | 원래 mouse world point − keydown/press caster 위치의 정규화 방향. bounds clamp와 분리. 기존 caster spawn offset 및 resolved range 규칙을 사용하며 지면 위치 spawn 0. |
| Target (2) | 마우스 방향 전방 ±45도 원뿔에서 기존 resolver의 hit layer, targetable, 범위, 거리/instance-ID 정렬을 재사용해 유닛을 한 번 잠금. 대기·cast 중 다른 유닛으로 재탐색하지 않으며 지면 위치 spawn 0. |
| GroundPoint (3) | 명시 point 의미를 가진 스킬만 bounds 및 runtime.resolvedRange로 clamp. 실행 직전에도 범위를 검사하며 지정점 생성 허용. |
| Self (4) | 마우스 변환 성공 여부·좌표를 무시하고 자기 중심 실행. |

Legacy (0)는 기존 SO 및 aimMode가 없는 JSON의 호환값이다. 기존 cast metadata의 None/Self→Self, AutoTarget→Target, AutoTargetDirection/Directional→Direction, Position→GroundPoint로 결정하며 기존 SnapshotTargetPointOnCast는 GroundPoint로 보존한다. 알 수 없는 문자열·직렬화 enum·누락 cast는 Invalid로 닫는다. NPC 기존 실행은 이 수동 dispatch를 사용하지 않는다.

Basic 최초 press는 실행 대기 중에도 snapshot을 보존하고, held의 새 combo cycle은 새 방향을 캡처한다. 콤보 3단계 facing과 hit context까지 Direction을 전달하며 타깃 탐색으로 덮지 않는다. Dash의 raw mouse 방향이 우선하며 변환 실패/거리 0.01 이하에서만 WASD→마지막 유효 facing 대체를 사용한다. 기존 제어권, UI 차단, teardown 및 3타 cadence/VFX/cooldown 계약을 유지한다.

## 서진 실제 슬롯

| 등급 | Basic | 1 | 2 | 3 | Dash |
| --- | --- | --- | --- | --- | --- |
| G1 | Direction | Target | 없음 | 없음 | Direction |
| G2 | Direction | Target | Self | 없음 | Direction |
| G3 | Direction | Target | Self | Direction | Direction |

CharacterSO의 기존 바인딩에서 12개 실제 슬롯을 추출했고 없는 3개 슬롯을 추가하지 않았다. 정확한 ID/JSON/SO 목록은 aim-mode-03/slot-manifest.json이다. 이 12개 JSON 및 SO YAML에는 aimMode만 추가했다. nested skill와 기존 profile/cast/hit/move/VFX 값은 보존했다.

## 검증 결과

- 런타임 C# 컴파일 오류 0, 경고 31. Editor 컴파일 오류 0, 경고 41. editor-compile.log, build.log, editor-build.log 참조.
- 입력/core/owner 29, 콤보·guard 15, teardown 10, schema/materializer/projectile helper/cone 12, MORPG live glue 62, geometry 14, dash clock 9, reward 12, spawn 10, cast presenter 4, P1 회귀 27: 합계 204/204 통과.
- 기존 183개 검사 범위를 유지하며 변경된 mouse 의미의 기대값을 교정했고 새 검사를 추가했다. 불필요한 추가 반복은 중단했다.
- materialized readback: 실제 JSON parser·ApplySkillFields·EquipmentSkillSO API를 테스트용 serialization adapter로 실행했다. 12개 JSON을 각 2회 적용해 직렬화 enum 값과 AimMode가 동일하며 현재 YAML 값과 일치함을 확인했다. unknown 재적용은 Invalid, 누락 재적용은 동일 legacy 의미로 수렴한다.
- 실제 helper 메서드에서 Direction의 explicit target point null 및 caster-offset spawn, Target의 unit lock/caster spawn, GroundPoint의 point spawn, Self의 mouse ignore를 검사했다. 기존 NPC point spawn도 그대로 통과했다.
- NPC public FireSkillAtSnapshotPoint 메서드 본문 SHA가 작업 전과 동일하다. aim-mode-03/npc-point-audit.json 참조.
- 보호 파일 458개 해시 동일. 별도 승인 범위인 SO 12개는 aimMode 한 줄만 변경했고 대응 JSON도 aimMode 외 기존 JSON 값이 동일하다. Git index는 동일하다.
- git diff --check, 전체 작업 scoped.patch 및 이번 aim-mode 전용 patch의 reverse dry-run 통과. 실제 rollback·파일 삭제 없음.

Unity engine import/AssetDatabase 저장·Play는 실행하지 않았다. 런타임 동작 검사는 스텁 및 실제 메서드 추출 방식이다. Editor 코드는 실제 Unity 참조로 컴파일했으나 materializer idempotency는 테스트 serialization adapter에서 검증한 결과이며 실제 Unity reimport 결과는 아니다.

## 범위 및 복구

이번 aim-mode 교정 원본·해시·전용 patch·report/receipt는 aim-mode-03/에 보존한다. 전체 수동 제어 작업의 누적 차이는 scoped.patch이며 공유 작업 트리의 다른 변경을 포함하지 않는다. 누적 기존 파일 38개, 신규 파일 20개. 세부 목록은 before-sha.json/after-sha.json/new-files.json이다. 복구 전 현재 SHA와 after-sha.json을 비교해야 하며 역패치 검사만 수행했다.

재현 명령은 AgentTools/SeojinManualHarness의 run.sh, combo.py, teardown.py, aim_pipeline.py, compile_editor.py 및 audit.py이다. 모든 명령은 저장소 루트에서 실행한다.

상세 결과는 로컬 report/receipt에 보존하며 완료 알림은 한결에게만 전달한다.
