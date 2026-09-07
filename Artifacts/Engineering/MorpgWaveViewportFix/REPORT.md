# PROVENANCE ONLY — superseded false-gate evidence

**Current production acceptance: [correction-01-interiors-enabled/REPORT.md](correction-01-interiors-enabled/REPORT.md).** Production interiorPropsEnabled is TRUE. The historical text and root-level binding/live logs below belong to the earlier false-gate implementation and are not current acceptance evidence.

---

# Screenshot viewport rootfix — completed

**SCREENSHOT_VIEWPORT_ROOTFIX_APPLIED**. 구현 blocker0. 요청 스크린샷을 기준으로 내부 프랍 gate, 장벽 alpha bbox 정렬, camera/player 범위와 실제 viewport 크기의 정적 crop을 완료했습니다.

## 적용 내용

- `binding.json`의 `interiorPropsEnabled=false`가 기본값입니다. Revision03 내부 prop17개와 decoration54개의 renderer/collider 생성을 모두 건너뜁니다. 이동과 Dash도 같은 runtime gate를 사용하여 보이지 않는 가상 prop 충돌을 남기지 않습니다. 원래 profile 정의·이미지·GUID·collider 정의는 그대로입니다. 다음 battle 활성화 전에 해당 값을true로 바꾸면 원래 프랍과 충돌이 재활성화되며, 테스트로 확인했습니다.
- 각 원본 PNG의 실제 alpha bbox를 기록했습니다. 보이는 장벽 좌우 끝은 각 zone x0..32(각각+36/+72)에, 안쪽 끝은 top y4 / bottom y-4에 정확히 맞춥니다. visible bbox 높이는1wu입니다. Top은y4..5, bottom은y-5..-4입니다. 원본 PNG/pivot/GUID를 변경하지 않고 전체 canvas transform을 역산했습니다. 기존과 같이 가로·세로 scale은 독립입니다.
- renderer scale은 불투명 mesh의 `sprite.bounds`가 아닌1536×512/PPU100의 실제 canvas 크기에서 계산합니다. Import된 rect/PPU/pivot이 계약과 다르면 preflight에서 거부합니다. collider patch는 그와 같은 world transform으로 다시 계산했으며 모든 패치의 원본 pixel 전체가alpha>=192이고 기존 outer boundary 밖입니다.
- Background는36×12wu, 각 zone x-2..34 / y-5.2..6.8의 overscan입니다. 원본3:1 비율을 유지하고 전투 상단이 이미지 하단76.67%에 대응합니다. 원본 이미지/metadata는 그대로입니다.
- camera clamp는 실제 orthographic half-height와aspect로 계산합니다. 기본size4/16:9에서 local camera x범위7.1111..24.8889, y범위-1..1입니다. 좌우 끝에서는 viewport edge가 zone side edge와 일치합니다. camera target에만y=-.9 framing offset을 적용하여 하단 장벽 끝을 보이게 합니다. clamp 자체는 반복 호출해도 이동이 누적되지 않습니다. MORPG follow 목표와 Dash의 고정 도착 framing에 동일한 규칙을 적용했습니다.
- player side 제한은 camera-center 범위에 viewport half-width를 다시 더한 영역과 기존 outer bounds를 교차한 후 actor radius+.15로 제한합니다. camera-center 제한을 player 제한으로 혼동하지 않습니다. 예: radius.35의 플레이어는 각zone local x.5..31.5, y-3.5..3.5까지 접근합니다.
- 기존32×8 outer polygon, wave/spawn/reservation, P2/reward/HUD/sorting, Dash mode·fade·잠금·wave 순서는 유지합니다. Dash의 geometry 참조만 현재 gate를 반영하는 runtime zone으로 연결했습니다.

## 추가 통합: legacy Background 중복 제거

Production `BattleManager.SpawnBackground`와 test `BattleSpawnManager.SpawnBackground`가 만든 renderer를 명시적으로 같은 BattleRuntime 소유자로 등록합니다. MORPG 활성화의 모든 준비가 성공한 뒤에만 suppression lease를 획득하여 해당 renderer를 숨깁니다. 활성화 뒤 test 경로에서 늦게 생성된 renderer도 즉시 숨깁니다. 다른 전투의 배경을 이름으로 검색하거나 변경하지 않습니다.

객체나 BattleSO sprite 에셋은 삭제하지 않습니다. 정상 legacy에서는 visible1, MORPG 성공에서는 legacy visible0 + wave1 background1, dressing preflight/activation 실패에서는 legacy visible1입니다. Dispose/중복 release 시 원래 renderer.enabled 값으로 복원하며, 애초 disabled였던 renderer를 켜지 않습니다. 다른 BattleRuntime은 영향을 받지 않습니다. 기존 일반 battle/fallback 코드는 보존했습니다.

## 검증

실제 C# compile 오류0 / 기존 경고31. Live52/52, geometry14/14, Dash clock9/9, reward12/12, spawn10/10, cast4/4, P1 authority/fault27/27: **총128/128 PASS**.

새 focused 검사: 기본 gate의 renderer9개만 생성 및 interior collider0, hidden virtual collision0, 원래 프랍 재활성, 모든zone camera 극점의 background 범위 포함, viewport/side edge 일치, player radius 반영, clamp 반복 drift0, alpha bbox/world collider 정렬, 잘못된 imported pivot 거부, tight sprite bounds가 canvas scale을 바꾸지 않음. 기존 Dash 검사에는1,762개 clear 위치 sweep이 포함됩니다.

`viewport-crops.json`에 W1–W3의 center/left/right/top/bottom 및 네 corner, 총27개960×540 crop의 world 좌표를 기록했습니다. 모든 crop에서 alpha 미커버 픽셀0입니다. 실제 설치 원본과 binding을 합성한 **정적 viewport crop이며 Unity screenshot/GPU 검증은 아닙니다**. `viewport-contact.png`에서 비교하고 `wave1-viewport-bottom.png` 등 개별 파일로 확인할 수 있습니다.

원본 PNG/GUID/meta, canonical environment/예약 데이터의 보호 SHA diff0. `git diff --check`, 이번 패치 reverse-check PASS, index hash 변경0. Unity GUI/headless/Play Mode/scene 편집/staging/commit/push/destructive delete0.

## 파일과 롤백

`first-diff.txt`, `same-path-before.json`, `before/`, `viewport-only.diff`, `rollback-manifest.json`, `receipt.json`과 검사 로그를 제공합니다. 현재 SHA가 일치할 때 이번 패치만 역적용하십시오. 이전 P2/Dash/sorting 및 원본 이미지/geometry를 reset하거나 삭제하지 않습니다. 실제 rollback은 수행하지 않았습니다.

재생성은 `AgentTools/MorpgWaveDressingHarness/reframe.py`, viewport 증거는 `viewport_audit.py`입니다. 이전 installer가 새 bbox binding을 되돌리지 않도록 guard를 추가했습니다. Reframe은 기본OFF 상태를 재생성하며, gate를 수동으로true로 변경하면 다음 battle에서 원래 프랍을 복원합니다.

## 최종 보고 전달 상태

추가 수정까지 구현·검증·롤백·receipt 정리가 완료됐으며 구현 blocker는 없습니다. 앞서 한결에게 직접 보낸 보고는 자동 승인 검토가 대상 작업 소유자 및 내부 보고서/경로 전달 권한을 확인하지 못해 거절했습니다. 거절 후 같은 자료를 재전송하거나 우회하지 않았습니다. 최종 결과는 이 REPORT와 receipt.json에 모두 반영되어 있습니다.
