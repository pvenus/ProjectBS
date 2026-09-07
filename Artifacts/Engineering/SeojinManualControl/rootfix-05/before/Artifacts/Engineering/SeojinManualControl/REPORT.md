# Charge / Basic / SwiftStep 사용자 교정 완료

blocker 없음. 구현과 필수 검증을 완료하고 현재 결과로 종료한다.

## 최종 계약

- Charge G1–G3: aimMode=Direction, aimInputSource=MouseDirection. keydown의 caster→raw mouse 정규화 방향을 고정한다. 타깃 잠금·원뿔 탐색·ground spawn은 0. 기존 DashForward 이동 거리/시간, 충돌·피해 데이터와 실행 경로를 재사용한다.
- Basic G1–G3: cast cooldown 정확히 1.000초. Direction + MouseDirection 유지. press/각 held cycle snapshot 및 기존 cadence/body/VFX 0/0/1과 cooldown 시작 시점은 유지한다. FireComboRoutine와 UseSkill 메서드 본문 SHA는 이번 교정 전과 동일하다.
- SwiftStep G1–G3: Direction + KeyboardMoveDirection. keydown normalized WASD → 마지막 유효 이동 입력 방향 → 현재 facing → right 순서. 마우스 좌표·변환 성공 여부를 무시한다. 대기 중 키보드·마우스·caster가 바뀌어도 캡처한 방향을 바꾸지 않는다.
- 나머지 Direction 입력원은 MouseDirection. aimInputSource를 슬롯/ID 하드코딩으로 분기하지 않고 EquipmentSkillSO에서 읽는다. 기존 Target/GroundPoint/Self와 NPC point-cast 경로를 보존한다.

## 스키마 및 materializer

Additive AimInputSource enum: Legacy=0, MouseDirection=1, KeyboardMoveDirection=2, Invalid=255. 기존 필드 없는 SO/JSON은 Legacy→MouseDirection으로 결정적으로 이관한다. 알 수 없는 문자열/enum은 Invalid로 닫는다. 입력 대기 중 정의가 바뀌면 snapshot의 source와 현재 source 불일치도 거부한다.

JSON root aimInputSource → EquipmentSkillJson DTO → ApplySkillFields → EquipmentSkillSO.ConfigureAimInputSource → 직렬화 aimInputSource 필드 → runtime Capture/Fire에 연결했다. 기존 aimMode API도 유지했다. JSON/SO 12개 실제 슬롯을 명시하고 없는 3개 슬롯은 추가하지 않았다.

| 등급 | Basic | Charge (1) | 2 | 3 | SwiftStep |
| --- | --- | --- | --- | --- | --- |
| G1 | Direction / Mouse / 1.000s | Direction / Mouse | 없음 | 없음 | Direction / Keyboard |
| G2 | Direction / Mouse / 1.000s | Direction / Mouse | Self | 없음 | Direction / Keyboard |
| G3 | Direction / Mouse / 1.000s | Direction / Mouse | Self | Direction / Mouse | Direction / Keyboard |

정확한 ID·JSON·SO·Basic cast SO 경로는 input-source-04/slot-manifest.json에 있다.

## 검증 및 readback

- 실제 JSON parser/materializer/EquipmentSkillSO API를 테스트 serialization adapter로 실행: 12개 슬롯을 각 2회 적용한 aimMode/aimInputSource 값이 동일하며 SO YAML의 정확한 enum 값과 일치.
- Basic JSON cooldown을 실제 SkillCastSO.ApplyEditorData 메서드에 각 2회 적용: G1/G2/G3 모두 1.000f이며 cast SO YAML도 1.000. 기존 SkillCastAssetBuilder의 json.cooldown 연결을 확인.
- 입력/core/owner 33, schema/materializer/helper/cone 15, combo/guard 15, teardown 10, MORPG live 62, geometry 14, dash clock 9, reward 12, spawn 10, NPC cast 4, P1 27 = 211/211 통과. 기존 204개 범위를 유지하며 변경된 SwiftStep 기대값을 교정하고 7개 검사를 추가했다.
- 런타임 C# 오류 0 / 경고 31. Editor 오류 0 / 경고 41.
- Charge에서 마우스가 반대 WASD보다 우선하고 target lookup 0, 대기 중 snapshot 불변 확인. SwiftStep은 반대 마우스·카메라 없음에서도 키보드/마지막 이동을 사용하며 fallback 순서와 정규화 확인. unknown source는 Fire 0.
- FireComboRoutine, UseSkill, NPC FireSkillAtSnapshotPoint 본문 SHA 동일. 직전 projectile/damage helper SHA도 동일. preservation-audit.json 참조.
- 보호 파일 455개 SHA 동일. 승인 범위인 equipment SO 12개는 aimMode/aimInputSource만, Basic cast SO 3개는 cooldown만 변경. JSON도 해당 필드 외 기존 의미가 동일. index 변경 없음.
- git diff --check 및 누적 scoped.patch/이번 input-source.patch의 reverse dry-run 통과. 실제 rollback·삭제 없음.

Unity GUI/headless/Play/import/reimport는 실행하지 않았다. materialized readback·멱등성은 실제 메서드와 테스트 serialization adapter에서 검증했고, 실제 Unity AssetDatabase 저장 결과로 주장하지 않는다. Runtime/Editor는 실제 Unity 참조로 컴파일했다. staging/commit/push 없음.

## 증빙과 복구

이번 교정 전 원본·before/after SHA·input-source.patch·REPORT/receipt는 input-source-04/에 보존한다. 누적 작업은 기존 41개 파일 및 신규 20개 파일이며 상세 목록은 상위 before-sha.json/after-sha.json/new-files.json이다. 공유 작업 트리의 다른 변경은 포함하지 않는 scoped.patch를 유지한다. 복구 전 현재 SHA를 비교하며 실제 적용은 하지 않았다.

재현 도구: AgentTools/SeojinManualHarness/run.sh, aim_pipeline.py, combo.py, teardown.py, compile_editor.py, audit.py. 각 로그는 보고서 폴더에 있다. 완료 알림은 한결에게만 전달한다.
