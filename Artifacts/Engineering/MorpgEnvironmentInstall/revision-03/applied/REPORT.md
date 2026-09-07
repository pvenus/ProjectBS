# Horizontal environment revision03 — applied

**HORIZONTAL_REVISION03_APPLIED_STATIC_ACCEPTANCE_PASS**. 가온의 운영 개편 지시에 따라 결합이 exact geometry를 결정하고 실제 환경·경로 데이터에 적용했습니다. 이전 PREPARATION의 좌표 계약 대기 상태와 Dash 보고서의 설치 지형 불일치 제한을 이 보고서가 대체합니다. 디자이너 검수나 Unity Play Mode 검수를 받았다는 뜻은 아닙니다.

## 실제 변경

| 항목 | Z1 | Z2 | Z3 |
|---|---|---|---|
| 전투 경계 | x0..32, y-4..4 | x36..68, y-4..4 | x72..104, y-4..4 |
| 가로:세로 | 4:1 | 4:1 | 4:1 |
| entry | (5,0) | (41,0) | (77,0) |
| Dash staging | 해당 없음 | (38,0) | (74,0) |
| Dash exit | (30,0) | (66,0) | 해당 없음 |
| 도착 고정 카메라 중심 | (7.2,0) | (43.2,0) | (79.2,0) |
| NPC 예약 | 19 | 5 | 4 |
| 구조물 / 장식 | 8 / 18 | 4 / 18 | 5 / 18 |

전체 지도는 [-3,-8,107,8], 카메라 orthographic size4 / 16:9입니다. 도착 고정 카메라는 구역 전체 중앙이 아닌 진입점 주변의 유효 clamp 위치로 결정하여, staging부터 entry까지가 화면 안에 있습니다. 전환 중 카메라 고정·투명 프레임 컷·웨이브 영수증 다음 프레임 follow 재개 규칙은 유지합니다.

상·하단 각 세 군집은 대·중·소 크기와 결정적 flipX를 사용합니다. 기존 선택15종 원본 파일과 GUID, 구조물17개의 ID·collider ID·asset 참조, 기존 장식 ID를 보존했습니다. 시각적 크기는 충돌체와 독립적이며 구조물 바닥은 footprint 하단에 맞췄습니다. 하단 구조물 높이는 .65배로 제한하고 기존 전경 겹침 알파 처리를 유지합니다.

28개 예약은 기존 순서대로 x=X+9..X+27을 진행하며 y=±2.1을 교대로 사용합니다. ID·unitKey·sourceZoneId·positionCandidateKey·localDueTime·19/5/4 counts를 포함한 **좌표 이외 경로 데이터 diff0**을 검증했습니다. 중앙 Dash 반경1.55와 NPC 여유.55를 합친2.10wu 띠에 예약이 들어오지 않습니다. P2 보상·최종 정산·NPC 표시·sorting 생산 코드는 변경하지 않았습니다.

환경 사전 검증에는 route/environment entry 일치 및 저작된 Dash capsule 검사를 추가했습니다. 잘못된 명시적 앵커는 활성화 전에 검출하며, 누락 앵커·애니메이션·ReducedMotion·실시간 장애물에 대한 기존 안전 폴백은 유지합니다. 플레이어의 실제 clear 위치가 검증된 중앙 경로 밖인 경우에도 안전 폴백이 가능합니다.

## 검증

- 실제 Unity6000.3.10f1 C# 컴파일: 오류0 / 기존 경고31.
- 설치된 프로필로 live34/34. 테스트용 polygon 확대를 제거했으며 Z1→Z2와 Z2→Z3 정상 Dash, alpha0 컷, 카메라 유지, 입력 복원, 웨이브 시작 순서를 모두 검사했습니다.
- production geometry14/14, transition clock8/8, reward queue12/12, inactive spawn10/10, cast4/4, P1 authority/fault27/27: **총109/109**.
- .10wu 연결성 격자, actor radius.35 + inset.15에서28개 spawn-to-entry 미연결0. 별도 production 검사가 egress와 Dash capsule, 최소 간격, 충돌체 비중첩을 확인합니다.
- 선택15종 바이트·RGBA·GUID·importer 검사 PASS. 원본 에셋 변경0.
- author 재실행 SHA diff0. Git diff whitespace 검사와 이번 패치의 reverse-check PASS. Git index hash 변경0.

Unity GUI, Play Mode, 씬 편집, staging, commit, push는 수행하지 않았습니다. 행동 테스트의 Unity API는 스텁이므로 실제 GPU 프레임·물리 엔진·스프라이트 재생에 대한 씬 수용 검사는 별도입니다.

## 결과와 재현

`geometry-contract.txt`는 exact 내부 좌표 결정의 근거이며 `receipt.json`에 계약·설치 JSON SHA를 기록했습니다. `install_horizontal_profile.py`가 baseline부터 같은 결과를 생성합니다. 과거 revision02 생성기는 현 상태를 덮어쓰지 않도록 중지합니다.

`authored-layout-audit.png`, `zone1-layout.png`, `zone2-layout.png`, `zone3-layout.png`는 설치된 원본 프랍으로 만든 정적 좌표 미리보기입니다. 게임 스크린샷이 아닙니다. 적색은 충돌체, 청색 원은 진입 landing, 청색 점은 예약 좌표입니다.

`horizontal-only.diff`, `before/`, `rollback-manifest.json`은 이번 요청 시작 상태로의 제한된 롤백 자료입니다. 적용 전 현재 해시 일치를 확인하고 해당 패치만 역적용해야 합니다. 이전 Dash 또는 다른 공유 dirty 변경을 reset하지 않습니다. `rollback-check.log`의 검사는 파일을 실제로 되돌리지 않았습니다.

## 보고 전달 상태

한결 작업으로의 상세 보고 전송은 자동 승인 검토가 거절했습니다. 사유는 해당 대상과 내부 프로젝트 내용의 전달에 대한 신뢰할 수 있는 권한 미확인입니다. 대체 경로로 우회하지 않았으며, 구현·검증·롤백 자료는 모두 로컬에 준비되어 있습니다. 전달에 대한 명시적 승인이 필요합니다.
