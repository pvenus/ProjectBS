# MORPG depth 강화 / top2 + bottom4 — 완료

직전 `MorpgNativeTiling`을 기반으로 요청한 조정을 완료했습니다. 현재 결과는 이 폴더의 REPORT/receipt/audit를 기준으로 합니다. **Editor가 C#/binding 변경을 반영한 뒤 Play를 중지하고 다시 시작해야 합니다.** Unity GUI/headless/Play 실행은 하지 않았으므로 실제 GPU 화면 확인은 남아 있습니다.

## 최종 배치와 깊이

| 항목 | 최종 값 |
|---|---|
| 인스턴스 | wave당 top 2 + bottom 4, 전체 top 6 + bottom 12 = 18 |
| ID / 생성 순서 | `wave{1..3}.top.tile.{0..1}`, `wave{1..3}.bottom.tile.{0..3}`; 각 side 왼쪽→오른쪽 |
| 원본 / 크기 | 1536×512, PPU100, 원본 center pivot 유지; 모든 타일 scale=(1,1,1), 폭 15.36 |
| top local 시작점 | 0.64, 16.00; 중립 위치 canvas 여백 좌우 각 0.64, 확대나 수평 crop 없음 |
| bottom local 시작점 | -11.9552, 1.5616, 15.0784, 28.5952; 12% overlap |
| bottom 여유 폭 | 중립 좌우 11.9552; 카메라 극단에서도 최소 10.8885 |
| world-follow | background +0.70 / top +0.42 / bottom -0.15 |
| screen-relative | background 0.30 / top 0.58 / bottom 1.15 |
| 폭 / 간격 / origin | 32 / 20 / 0, 52, 104 — 직전 binding SSOT 유지 |

계수는 범위 내 추가 조정 없이 요청 기본값을 사용합니다. 카메라 local 이동 범위는 중심 대비 ±7.111111입니다. 극단의 background 이동은 ±4.977778, top의 고정 실루엣 collider 대비 수평 이격은 최대 2.986667, bottom은 최대 1.066667 유닛입니다. 세 그룹의 renderer만 이동하며 collider, outer bounds, spawn, actors, interior props는 고정입니다.

## 충돌과 범위

움직이는 장벽 시각 요소를 전투 밖 band(top y≥4.05, bottom y≤-4.05)로 제한했습니다. 원본 불투명 능선을 해당 band 밖으로 위치 조정하고 안쪽의 투명 잔상만 세로 crop합니다. 원본 파일, native 가로 폭, PPU와 scale은 유지합니다. 수평 end crop은 필요하지 않아 사용하지 않았습니다. top의 자연 여백에는 배경이 보이며 outer boundary collider가 gameplay 경계를 계속 소유합니다.

실루엣 collider는 중립 타일 원본의 alpha≥192 patch와 정합하며, zone x 범위로 clip한 고정 물리 형상입니다. 각 top은 실제 두 타일에만 collider를 만듭니다. 862개 template patch의 원본 alpha와 bounds를 검사했습니다. 움직임에 따른 시각/충돌 이격은 위 band 밖으로 나오지 않습니다. bottom의 최외곽 돌출은 최대 13.021867로 gap20보다 작으며 인접 zone까지 최소 6.978133 유닛이 남습니다. 다른 wave의 visual/collider root는 비활성입니다.

## 전환 및 보존

source fixed camera → invisible cut → destination fixed camera 경로를 유지합니다. 목적지 camera 배치 후 배경과 top/bottom의 canonical origin을 함께 재설정하고 root를 활성화합니다. 하니스에서 재설정 직후와 반복 Late 갱신의 renderer 위치 차이는 0입니다. 양쪽 실제 Dash 전환 및 빠른 좌↔우 이동도 검사했습니다.

production interiors=true, 기존 props17/decor54/구조 collider17, legacy 성공 visible0/실패 visible1, 엄격한 center-pivot 검증, 플레이어 local x .5..31.5, 보상·스폰·Dash/fade/gameplay 계약을 유지합니다. canonical profile/contract, 원본 PNG, importer meta/GUID, transition clip, Git index의 동일 경로 SHA 차이는 0입니다.

## 검증 결과

- Unity Roslyn 컴파일 errors 0 / 기존 warnings 31.
- 회귀 134/134: live58 + geometry14 + clock9 + reward12 + spawn10 + cast4 + P1 27. 실제 런타임 클래스를 Unity stub에서 실행한 결과이며 Unity 엔진 실행 결과는 아닙니다.
- W1–W3 center/left/right 9개 및 fast-left/fast-right 6개, 총 15개 정적 viewport: top/bottom visible bbox·불투명 pixel 수 기록, 미피복 픽셀 0, 하단 장벽 누락 열 0.
- renderer instance count/ID, native scale·폭, layer별 coefficient, 고정 physics/props, combat 밖 band, wave isolation, 잘못된 count/계수 거부, atomic first-frame 위치를 검증했습니다.
- diff-check 및 scoped reverse dry-run exit0. 실제 롤백은 하지 않았습니다.

## 프리뷰와 증거

`depth-annotated-contact.png`와 각 `waveN-{left,center,right}-annotated.png`에 ID·접합선·world-follow 이동 화살표를 표시했습니다. `waveN-fast-{left,right}-annotated.png`는 빠른 극단 전환입니다. `waveN-seams-collider-overlay.png`의 빨간 형상은 고정 collider, 노란 선은 이동한 시각 타일 접합선입니다. `runtime-frames.json`은 renderer 위치/scale/source crop와 고정 physicsBase, `audit.json`은 시각적 가시성과 이격 허용치, `layout-audit.json`은 ID·여백·overlap·인접 zone 여유를 기록합니다. 정적 합성 프리뷰이며 실제 Unity/GPU 캡처는 아닙니다.

## 롤백

`before/`, `before-sha.json`, `same-path-before.json`, `first-diff.txt`에 이번 조정 직전 상태를 보존했습니다. `changes.patch`는 이번 변경만 포함하며 `rollback-check.log`에 역적용 사전 검사 결과가 있습니다. 이후 공유 작업이 바뀌었다면 다시 검사한 뒤 이 patch만 역적용해야 합니다. 이번 작업에서는 파일 삭제, Unity 실행, index 수정, staging, commit, push를 하지 않았습니다.
