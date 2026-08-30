# ProjectBS Domain Image Specifications

Status: current art specification  
Owner: 화감

## 공통 측정 규칙

- 캔버스 크기와 실제 오브젝트 크기를 별도로 기록한다.
- `alpha bbox 점유율`은 투명하지 않은 픽셀의 경계 상자 면적 / 캔버스 면적이다. 불투명 배경 이미지는 이 수치 대신 주 피사체 경계와 안전 영역을 수동 측정한다.
- 모든 자산은 원본 100%와 실제 표시 크기에서 검수한다. 원본만 보고 승인하지 않는다.
- Prefab의 `RectTransform` 값은 Canvas 단위다. 문서의 200/80/32 등은 물리 픽셀 고정값이 아니며, 런타임 검수에서는 Canvas Scaler와 화면 폭을 적용한 실제 픽셀 크기를 함께 기록한다.
- 투명 전경은 RGBA PNG, 완전한 배경은 RGB 또는 불필요 알파가 없는 PNG를 원칙으로 한다.
- 색 공간은 sRGB 전제로 검증한다. Unity 세부 임포트 값은 한결이 확인한다.

## 도메인별 현행 규격

| 도메인 | 프로젝트 출력 | 실제 표시 검증 | 점유·여백 | 배경 | 판정 메모 |
| --- | --- | --- | --- | --- | --- |
| 캐릭터 전신/portrait | 1024×1536 RGBA PNG | 112×235 HUD, 126×264 정보 패널, 64px 높이 실루엣 시험 | alpha bbox 면적 0.48~0.72 권고; 머리 8~15%, 발 4~10%, 좌우 8% 이상 | 투명 | 1023×1537 등 오차는 신규 승인 불가 |
| 캐릭터 애니메이션 | 한 세트 내 동일 캔버스·RGBA; 현행 512²/627²/768×512 혼재는 신규 세트에서 금지 | 런타임 목표 크기와 64px 높이 | 프레임별 bbox 중심 이동 5% 이내(의도 이동 제외), 바닥선 3% 이내 | 투명 | 캐릭터별 기준 전신의 얼굴·복식·색 고정 |
| 스킬 아이콘 | 제작 원본 1254² RGBA; P0 런타임 파생 max 256 | Canvas 200 정보 UI, Canvas 80 전투 HUD, 32px 파생본 스트레스 시험 | 주 상징 bbox 폭/높이 65~86%; 안전 여백 각 7% 이상 | 투명 | 동일 max 256 파생본으로 비교; 80×80 PixelLab 레거시는 신규 권위 아님 |
| 아이템 아이콘 | 제작 원본 1254² RGBA; P0 런타임 파생 max 256 권고 | Canvas 200 인벤토리, Canvas 83/69 상점, 32px 스트레스 시험 | 주 물체 bbox 62~82%; 돌출부 포함 안전 여백 7% 이상 | 투명 | 단일 물체, 효과·배경 장면 금지 |
| 스킬 VFX 애니메이션 | 신규 한 세트 내 동일 정사각 캔버스·RGBA | 전투 적용 크기, 128²/64² 축소 | 효과 핵 15% 이상, 전체 bbox 55~88%; 프레임 중심 안정 | 투명 | 현재 512/768/1254 혼재는 재사용 가능하나 신규 혼용 금지 |
| 스토리 팝업 | 960×1280, 3:4 | Prefab 마스크 573×764 | 핵심 인물/행동이 중앙 80% 안전 영역에서 읽힘 | 완전 배경 | UI 문자·말풍선·로고 금지 |
| 전투 배경 | 2560×1440 RGB, 16:9 | 기준 전투 화면 전체 + 캐릭터/VFX 합성 | 중앙 전투 영역에 강한 소품 금지; 상하 HUD 안전 영역 확보 | 완전 배경 | 장소성은 가장자리·중경, 플레이는 중앙 전경 |
| UI 프레임·패널 | Prefab 목표 Rect와 9-slice 정책에 맞춘 RGBA | 1× 및 지원 화면비 | 텍스트/아이콘 안전 영역 명시 | 투명 또는 패널 바탕 | 생성 전 실제 Rect와 상태 세트 필수 |

## Canvas 배치 크기 근거

- `Assets/Prefabs/UI/Fixed/Content/UISkillIconSlot.prefab`: 200×200, 내부 마스크 170×170
- `Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudSkillSlot.prefab`: 80×80
- `Assets/Prefabs/UI/Fixed/Panel/Panel_CharacterInfo.prefab`: Portrait 125.66×263.60
- `Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudMember.prefab`: Portrait 112.24×235.45
- `Assets/Prefabs/UI/Fixed/Content/UIInventoryItemView.prefab`: 200×200
- `Assets/Prefabs/UI/Fixed/Panel/Shop_Fixed.prefab`: 아이템 아이콘 약 83×83 및 69×69
- `Assets/Prefabs/UI/Fixed/Panel/EventPopupView_Fixed.prefab`: 이벤트 이미지 마스크 약 573×764

이 수치는 Prefab 정적 조사에서 확인한 Canvas 단위다. StageScene/BattleScene의 확인된 기준은 800×600, `matchWidthOrHeight=0`이며 Standalone 기본 1920×1080, Web 기본 960×600이다. 다른 동적 UI에는 1920×1080 또는 2560×1440, match 0.5도 있으므로 단일 물리 픽셀 계약으로 환산하지 않는다. 최종 검수는 한결이 `RectTransform.GetWorldCorners`, `Canvas.scaleFactor`, 해상도와 캡처를 한 묶음으로 제공한 뒤 확정한다.

## 크기별 판정

- Canvas 200 배치의 실제 픽셀 결과: 주요 형태, 재질, 보조 상징까지 읽혀야 한다.
- Canvas 80 배치의 실제 픽셀 결과: 주 상징과 방향, 색 계열이 1초 이내 식별되어야 한다.
- 32px 파생본 스트레스 시험: 의미의 세부가 아니라 서로 다른 형제 아이콘의 실루엣이 구분되어야 한다.
- 16px는 통과 조건이 아니라 과밀도 진단용이다.

검증 해상도는 최소 1920×1080, 960×600, 2560×1440이며 모두 동일한 플랫폼 파생본에서 비교한다.

## 애니메이션 안정성

- 동일 세트에서 캔버스, pivot, 바닥선, 방향, 팔레트, 얼굴 비례를 고정한다.
- 의도하지 않은 프레임별 확대/축소, 배경 잔여물, 알파 테두리, 장비 생성·소실은 재작업이다.
- 루프는 첫/끝 프레임의 실루엣 차와 중심 이동을 확인한다.
- 공격은 준비-접촉-회수의 힘 방향이 읽혀야 하며, 이펙트가 무기·손의 동작을 숨기지 않는다.
