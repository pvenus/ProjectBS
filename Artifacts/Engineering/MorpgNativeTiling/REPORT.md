# MORPG native tiling / parallax / wave isolation — 완료

구현, 컴파일, 회귀 검사 및 정적 프리뷰 검증을 완료했습니다. Unity GUI/headless/Play는 실행하지 않았습니다. **Editor가 C#/binding 변경을 반영한 뒤 현재 Play를 중지하고 다시 시작해야 실제 화면에 적용됩니다.**

## 구현 결과

- 원본 1536×512 / PPU 100 / center pivot을 유지했습니다. 각 장벽은 native width 15.36이며 transform scale=(1,1,1)입니다. 각 side 3개, wave당 6개, 전체 18개 타일을 12% 겹칩니다. top/bottom의 시작 위상 및 생성 순서를 결정적으로 다르게 했습니다. 끝 타일만 source rect를 crop하며 타일과 충돌 조각 모두 zone x 경계 안에 있습니다.
- top은 불투명 능선 기준 전투 상단 y=4에 배치합니다. bottom은 viewport 하단에서 0.65 위에 능선을 배치합니다. 862개 원본 불투명 영역 기반 template patch를 각 타일 좌표로 이동·clip합니다. 모든 patch의 원본 alpha>=192 및 gameplay core 불침범을 검사했습니다.
- parallaxFactor=0.20은 **world-follow 계수**입니다. `backgroundBase + (camera - zoneCameraOrigin) * 0.20`으로 배경 시각 요소만 이동합니다. 장벽·props·collider 위치는 고정입니다. 처음 제시했던 상대 화면 이동률 해석은 최종 구현에 사용하지 않습니다.
- binding layout이 width 32, gap 20, origin 계산의 단일 기준입니다. 런타임 origin은 0/52/104입니다. 기존 canonical profile/contract는 변경하지 않고 attempt 내부에서 spawn/reservation/entry/staging/exit/camera/prop 좌표를 같은 delta로 투영합니다. 저장 계약과 공개 필드는 추가하지 않았습니다. 배경의 parallax swept extent도 인접 wave와 겹치지 않습니다.
- 현재 wave의 background/barriers/props/decor/structural collider만 활성화됩니다. invisible cut에서 source를 끄고, 목적지 위치 및 카메라 배치 후 parallax 기준 갱신과 destination 활성화를 같은 호출 흐름에서 완료합니다. 두 실제 Dash 전환을 매 tick 검사했습니다.
- interiorPropsEnabled=true, 기존 props 17/decor 54/구조 collider 17, legacy 성공 visible 0 / 실패 visible 1, center-pivot 엄격 검증, 보상·스폰·Dash/fade 순서를 유지했습니다.

## 상단 가시성 보정

초기 프리뷰에서 top이 보이지 않은 원인은 기존 half-height 4 / cameraY -0.9의 화면 상단이 y=3.1이었기 때문입니다. 최종 binding은 half-height 5 / cameraY -0.5로 상단 y=4.5, 하단 y=-5.5를 보여 줍니다. 장벽을 늘리지 않았습니다. 플레이어 local x .5..31.5와 전투 구역 폭 32는 유지됩니다.

W1–W3 center/left/right 9개 960×540 프리뷰 전부 top/bottom 불투명 bbox와 visible pixel 수를 audit.json에 기록했습니다. top 20,390–26,976 pixels, bottom 28,134–40,383 pixels가 보입니다. 각 화면의 하단 장벽 누락 열 0, 미피복 픽셀 0입니다. 기준은 alpha>=192입니다.

## 검증 및 증거

- 실제 Unity Roslyn 컴파일: errors 0 / 기존 warnings 31 (`compile.log`).
- 회귀 134/134: live 58, geometry 14, clock 9, reward 12, spawn 10, cast 4, P1 27. live는 실제 런타임 클래스를 Unity stub에서 실행합니다. geometry 별도 검사는 보존된 canonical profile, live preflight는 투영된 profile도 검사합니다.
- `git diff --check` 및 이번 변경 patch의 reverse dry-run 모두 exit 0. 역적용은 실행하지 않았습니다.
- 보호 대상 동일 경로 SHA 차이 0: 원본 PNG, importer meta/GUID, canonical contract/profile, transition clip, Git index 유지.
- `runtime-frames.json`: 하니스에서 생성된 실제 renderer 위치·scale·crop 값.
- `audit.json`, `preview-audit.json`: 픽셀 가시성 및 미피복 검사. `layout-audit.json`: 겹침과 origin/배경 swept extent.
- `viewport-contact.png`: W1–W3 center/left/right. `wave1/2/3-seams-collider-overlay.png`: 접합 및 collider overlay. `transition-gap.png`, `parallax-before-after.png`: 전환과 월드 추종 계수 설명 프리뷰.

프리뷰는 원본 이미지와 하니스 transform으로 합성한 정적 검증입니다. 실제 Unity/GPU 프레임 캡처가 아니며 실제 Play 화면 검증은 남아 있습니다.

## 롤백

`before/`와 `before-sha.json`은 수정 직전 동일 경로 파일을 보존합니다. `first-diff.txt`는 시작 시 공유 작업의 tracked diff 상태입니다. `changes.patch`는 이번 변경과 새 검증 스크립트만 포함합니다. `rollback-check.log`는 역적용 사전 검사 결과입니다. 공유 작업이 추가로 변경됐다면 재검사 후 이번 patch만 역적용해야 합니다. 이번 작업에서는 롤백·파일 삭제·Unity 실행·Git staging/commit/push를 하지 않았습니다.
