# Seojin dual direction HUD install 09 완료

선정된 두 source PNG를 versioned Resources 경로에 원본 바이트 그대로 설치하고 기존 selection aura 아래에 독립 HUD 계층을 연결했다. 기존 aura prefab/loop/animation은 변경하지 않았다.

## 설치와 표시

Source manifest SHA: 99737281257a6c67b77eb4ac418ff97689d1d2b2850db11096e282d34703a5dd. exact2 PNG SHA, RGBA 512×512, Sprite Single, pivot(.5,.5), PPU100, 무압축/alpha/고유 GUID metadata를 확인했다. 실제 Unity import는 실행하지 않았다.

설치 경로: Assets/Resources/battle/Presentation/SeojinDualDirectionCrescent/revision-01. 기존 파일 덮어쓰기 없이 두 PNG와 metadata를 새로 추가했다.

SeojinManualControl.Configure에서 이미 생성된 AuraView root에 HUD를 한 번 연결한다. SeojinDirectionGround 아래 MouseRedPivot/WasdBluePivot 및 각각의 SpriteRenderer를 만든다. 색상은 원본 그대로 white tint이며 파랑 반경 1, 빨강 반경 1.15로 같은 방향에서도 분리된다. PPU100과 실제 aura sprite rect/VisualScale로 outer-ring 크기를 계산한다. 지면 타원 비율을 역보정한 방향에 atan2를 적용해 표시 끝이 월드 입력 방향을 가리킨다. 회전 쓰기는 두 HUD pivot의 local Z뿐이다.

빨강은 live mouse ray, 파랑은 현재 normalized WASD만 사용한다. 파랑은 release/상쇄 키에서 즉시 숨고 last-facing fallback이 없다. invalid mouse는 빨강만 숨기며 유효한 WASD 파랑은 표시할 수 있다.

## 숨김·선택·재사용

Aura SetSelectionActive의 enabled 상태와 실제 arc renderer visibility를 모두 확인한다. manual 소유권, UI, pause, CC/rooted, death, transition, explicit Auto, battle end, disable 시 두 표시를 숨긴다. UI는 기존 SeojinManualControl의 global no-intent 정책을 따른다. 따라서 pointer-over UI도 빨강·파랑 모두 숨긴다. 이는 파랑이 마우스 방향을 사용하는 것이 아니라, UI 동안 전체 manual input이 정지하는 기존 계약을 반영한다.

Resources는 bind에서 캐시하고 hierarchy는 한 번 생성한다. LateUpdate에는 managed collection/GameObject 생성 및 Resources.Load가 없다. 카메라는 캐시 후 비활성/null/1초 갱신 때 확인한다. disable은 renderer/pivot을 숨김 상태로 돌리고 camera cache를 비운다. sprite 누락 시 한 번 만든 빈 renderer는 계속 숨겨지며 매 프레임 생성/재시도하지 않는다. 재bind에서 sprite가 복구되면 기존 자식 renderer에 다시 연결된다.

## 정렬

MORPG Default layer: background(-1000) < back props(-900) < HUD(-850) < body(-800..0) < foreground occluding props(50) < telegraph(100) < world health HUD(200+). 기존 전경 소품의 가림 규칙은 유지한다. 일반 전투는 background(-1000) < 새 HUD(-999) < ground telegraph(-998)로 전용 정렬 슬롯을 추가했다. 기존 일반 ground telegraph의 상대적 우선순위를 유지하면서 절대 order를 1 높였다.

## 검증과 한계

기존 622 + HUD 27 = 649 통과, 실패 0. input/HUD production 대역 실행 78개 중 기존 input 51, HUD 27이다. 8방향 visible world direction, 동시 lane, release/opposites, UI pointer/focus, selection/renderer 숨김, pause/CC/death/transition/Auto/end, missing sprite/rebind, 반복 hierarchy/resource 안정성, sorting을 검증했다. Unity GC profiler 측정은 아니다.

runtime compiler 오류 0 / 경고 31; editor 오류 0 / 경고 41. 보호 자산 455개 동일, 이전 승인 설계 필드 15개 유지. 기존 aura SHA 동일. 누적 patch 기존 50개 / new 43개. diff 및 revision/누적 binary-aware reverse dry-run 통과.

static-preview.svg/png는 원본 이미지와 실제 오라 크기를 사용한 정적 합성이다. PNG를 로컬 SVG 렌더러로 생성해 시각 확인했다. Unity 화면이나 Play 결과가 아니다. QuickLook thumbnail 실행은 sandbox 초기화 오류, browser file URL 확인은 URL 정책으로 거부되어 해당 UI 경로는 사용하지 않았다.

Unity GUI/headless/Play/import, staging/commit/push, 실제 rollback은 모두 0회. 실제 Unity runtime 외관과 import 결과 확인은 미수행이다.

## 산출물

asset-audit.json / source-manifest.json / installed-files.json: 원본·설치·metadata 근거.
check-00~18.log: 실행 결과. static-preview.png/svg: 정적 합성.
correction.patch / rollback.patch: 이번 변경과 binary PNG 포함 역패치. 상위 scoped.patch: 전체 누적 변경.

앞선 Hover08 report 전송은 자동 검토가 목적지 공개 권한 미확인을 이유로 거부했다. 이후 read_thread로 같은 ProjectBS의 [개발팀] 한결 — 팀장 task임을 확인했다. 작업 산출물은 로컬에 보존했다.
