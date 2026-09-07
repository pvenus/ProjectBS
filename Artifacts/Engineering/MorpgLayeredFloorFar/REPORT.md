# revision03 layered floor + far 설치 — 완료

선택된 revision03 원본을 새 버전 리소스 경로에 설치하고 floor/far 독립 슬롯과 런타임을 구현했습니다. **Editor가 새 PNG/meta/shader/C#를 import한 뒤 현재 Play를 중지하고 다시 시작해야 합니다.** Unity GUI/headless/Play는 실행하지 않았습니다. C# 컴파일과 정적 합성·하니스 검증을 마쳤으며 실제 Unity shader/GPU 컴파일과 화면 확인은 남아 있습니다.

## 원본과 설치

- source: `Artifacts/GraphicsRemediation/MORPGWaveEnvironment/selected/revision-03-layered-floor-far`
- manifest SHA: `f3ef1e10d277c7359d7cf611dfcd31e365387a803827dc3cc7b1cf2e1627a425`
- 정확히 floor3 + far3, 각 1536×512, alpha 전부255, manifest output SHA 일치 확인.
- 새 경로: `Assets/Resources/battle/morpg/wave-environment-v3-layered/`; 6개 PNG와 각각 고유 GUID/meta, PPU100, center pivot(0.5,0.5), FullRect, maxTextureSize2048.
- 원본 및 revision02/DepthTiling 리소스를 덮어쓰거나 삭제하지 않았습니다. 보호된 동일 경로 SHA 차이0이며 이전 문서·롤백 자료도 보존했습니다.

## 독립 레이어

binding schema를 내부 v2로 확장해 wave당 `[far, floor, top, bottom]`을 엄격히 로드합니다. 원본 sprite는 총12개, 신규 plane은 정확히6개입니다. 하나라도 누락되거나 canvas/PPU/pivot가 잘못되거나 floor shader가 없거나 지원되지 않으면 scene mutation 전에 실패합니다. 모든 슬롯의 preflight 성공 후 활성화하며 legacy는 성공 visible0 / 실패 visible1입니다.

| 레이어 | World-follow | Screen-relative | Sorting | Collider |
|---|---:|---:|---:|---|
| far | .70 | .30 | -1100 | 0 |
| floor | 0, world fixed | 1.00 | -1000 | 0 |
| top2 | .42 | .58 | -900 | 기존 위치 고정 |
| bottom4 | -.15 | 1.15 | 50 | 기존 위치 고정 |

floor는 원본 RGB를 보존하며 world y=3.55에서 투명, y≤2.95에서 완전 불투명해지는 0.60 높이의 smoothstep 혼합을 사용합니다. 전용 floor material만 이 동작을 사용하고, far는 뒤에서 전체 viewport를 덮습니다. 이로써 불투명 floor가 far를 전부 가리는 일을 방지합니다. floor/far의 rect와 horizon 값은 binding에 있으며 전투 props·actors·telegraph보다 뒤에 그려집니다.

## top 위치와 gameplay 보존

최종 top 시각 안쪽 경계는 **y=3.55**, 전투 상단 y=4보다 0.45 낮습니다. 고정 collider의 y≥4.05 경계는 바꾸지 않았으므로 renderer만 physics 기준에서 수직 -0.50 이동합니다. 기존 수평 parallax와 native scale1, top2/bottom4 개수는 유지합니다.

core 상단 y=2.5와 시각 경계 사이 1.05, 플레이어 중심 상한 y=3.5와 사이 0.05를 확인했습니다. 플레이어 body(min sorting -800)와 telegraph(100)는 top(-900)보다 앞에 그려집니다. 캐릭터 텍스처와 경계의 시각적 겹침이 있더라도 top이 이를 가리는 정렬은 아닙니다. 이동·스폰·스킬·충돌에는 새 blocker를 넣지 않았습니다. 실제 top collider의 모든 점은 y≥4.05입니다.

width32 / gap20 / origins0,52,104, camera half-height5/y=-.5, 플레이어 local x .5..31.5, production interiors=true를 유지했습니다. props17/decor54/기존 구조 collider17도 유지했습니다. 현재 wave의 far1/floor1/top2/bottom4와 내부 환경만 활성입니다. source fixed camera→invisible cut→destination fixed camera 순서 및 parallax 기준 재설정은 유지하며 floor는 고정 위치 그대로 wave root와 함께 전환합니다.

## 검증

- 실제 Unity Roslyn C# 컴파일: errors0 / 기존 warnings31. GPU shader compiler는 실행하지 않았습니다.
- 회귀 **138/138**: live62 + geometry14 + clock9 + reward12 + spawn10 + cast4 + P1 27. live는 실제 런타임 클래스를 Unity stub에서 실행한 결과입니다.
- exact6 각각 누락 시 partial activation0/legacy 유지, exact12 source pivot/canvas 검증, shader 누락/unsupported 거부, floor 고정/far .30 relative, sorting, top 시각 하강/고정 collider, current-wave-only, 두 전환과 첫 프레임 위치 차이0 검증.
- W1–W3 center/left/right 9개 및 fast extremes6개: 미피복0, 하단 누락 열0, top/bottom 가시 픽셀·bbox 기록.
- 원본/설치 SHA, 불투명성, 메타 readback6/6. 동일 경로 보호 SHA 차이0.
- git diff --check 및 이번 text+binary patch reverse dry-run exit0. 실제 rollback/index/staging/commit/push/파일 삭제 없음.

## 증거

`source-contact-2row.png`는 선택 원본의 2행 contact입니다. `viewport-contact.png`, `depth-annotated-contact.png`, 각 wave center/left/right 이미지, `horizon-floor-contact.png`, `parallax-before-after.png`, `transition-gap.png`를 참조하십시오. 프리뷰의 floor alpha는 설치 shader의 world-space smoothstep 식을 동일하게 적용한 정적 합성입니다.

`runtime-frames.json`은 실제 하니스의 renderer transform/crop/physicsBase, `source-preflight.json`과 `installed-readback.json`은 exact6 근거, `audit.json`은 가시성·horizon·core/중심 경계, `receipt.json`은 최종 파일 및 증거 SHA를 기록합니다. 기존 DepthTiling 폴더는 직전 상태의 증거이며 이번 완료 기준은 이 폴더입니다.

## 롤백

`before/`, `before-sha.json`, `same-path-before.json`, `first-diff.txt`에 조정 직전 상태를 보존했습니다. `new-files.json`은 additive 설치 파일 목록입니다. `changes.patch`는 이번 C#/binding/harness 변경과 새 리소스의 binary patch를 포함하며 `rollback-check.log`는 역적용 사전 검사 결과입니다. 원본 source 파일은 patch에 포함하지 않았습니다. 공유 작업이 추가됐다면 재검사 후 이번 범위만 되돌려야 합니다. 이번 작업에서는 실제 역적용을 실행하지 않았습니다.
