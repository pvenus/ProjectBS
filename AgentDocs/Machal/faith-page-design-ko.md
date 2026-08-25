# 신앙 페이지 설계

## 상태

- 설계일: 2026-08-17
- 최신 정정: 2026-08-21 — 성장 로드맵 아래를 현재 레벨/다음 레벨 신앙 효과 비교 카드 두 개로 고정
- 범위: 신앙 정보 페이지, 프리팹 준비, Presentation 소유권, 단계별 구현 계획
- 구현: 2026-08-21 메인 패널 하이어라키와 View/Presenter 호출 골격 완료
- Unity 프리팹 열기와 정적 시각 확인 완료, Play Mode 데이터 검증은 대기
- 이 설계는 이전의 신앙 페이지 축복 탭 3개 준비 계약을 대체한다.

## 제품 계약

획득한 신앙 하나는 서로 다른 네 기능 단위를 가진다.

1. 기본축복: 신앙 획득과 함께 적용되고 신앙 레벨마다 강화된다.
2. 전용전직: 캐릭터 직업군과 연결되는 신앙 전용 해금 기능이다.
3. 전용축복 1: 신앙 고정 시 획득한다.
4. 전용축복 2: 고정된 신앙이 레벨 8에 도달하면 획득한다.

페이지는 현재 효과뿐 아니라 앞으로 해금될 기능도 설명해야 한다. 단순 축복 컬렉션 페이지가 아니다.

## 권장 사용자 흐름

```text
신앙 페이지 열기
-> 획득 신앙마다 탭 하나 생성
-> 고정 신앙이 있으면 우선 선택, 없으면 가장 높은 레벨 신앙 선택
-> 선택한 신의 정보와 현재 진행 표시
-> 레벨 1-10 성장 로드맵 표시
-> 로드맵 아래에 현재 레벨 신앙 효과 카드와 다음 레벨 신앙 효과 카드 표시
-> 다음 레벨 카드에서 강화되는 효과와 새로 획득하는 기능을 원본 기준으로 구분
```

획득 신앙 탭은 정보 탐색용으로 유지한다. Gameplay 활성, 고정, 제거된 효과 상태는 탭 삭제가 아니라 별도 상태로 표현한다.

## 페이지 구조

```text
Panel_FaithInfo
|- Faith_Header
|  |- Faith_TitleText
|  `- Faith_CloseButton
|- Faith_GodTabScrollRect
|  `- Viewport
|     `- Faith_GodTabRoot
`- Faith_SelectedGodPage
   |- Faith_GodSummary
   |  |- Faith_GodIconImage
   |  |- Faith_GodNameText
   |  |- Faith_GodDescriptionText
   |  |- Faith_LevelText
   |  |- Faith_AffinityText
   |  |- Faith_LevelProgressSlider
   |  `- Faith_StateText
   |- Faith_RoadmapScrollRect
   |  `- Viewport
   |     `- Faith_LevelNodeRoot
   `- Faith_LevelEffectComparisonRoot
      |- Faith_CurrentLevelEffectCard
      `- Faith_NextLevelEffectCard
```

PC 권장 배치는 상단 신 탭과 신 정보, 중앙 성장 로드맵, 하단 좌우 비교 카드다. 두 카드는 동일한 폭을 사용하고 각 카드의 효과 목록은 자체 세로 ScrollRect를 가진다.

## 필요한 프리팹

### `Panel_FaithInfo.prefab`

향후 `FaithPagePresenter`를 부착하는 합성 소유자다. 획득 신앙 탭, 선택 신, 로드맵 갱신, 기능 선택, Runtime 이벤트 구독을 소유한다.

### `UIFaithGodTab.prefab`

```text
UIFaithGodTab
|- FaithTab_IconImage
|- FaithTab_NameText
|- FaithTab_LevelText
|- FaithTab_SelectedFrameImage
|- FaithTab_LockedMark
`- FaithTab_InactiveMark
```

탐색과 상태만 표시하고 Shrine 데이터를 직접 해석하지 않는다.

### `UIFaithLevelNode.prefab`

```text
UIFaithLevelNode
|- FaithLevel_LevelText
|- FaithLevel_CurrentMark
|- FaithLevel_AcquiredMark
|- FaithLevel_LockedMark
`- FaithLevel_MilestoneIconRoot
```

미래 기본축복 레벨도 미리보기 가능하게 둔다. 미래 노드는 활성 효과가 아니라 작성된 Preview로 표시한다.

### `UIFaithLevelEffectCard.prefab`

현재 레벨과 다음 레벨에 같은 프리팹을 한 번씩 사용하는 비교 카드다.

```text
UIFaithLevelEffectCard
|- FaithEffectCard_Header
|  |- FaithEffectCard_StateText
|  `- FaithEffectCard_LevelText
|- FaithEffectCard_ScrollRect
|  `- Viewport
|     `- FaithEffectCard_GroupRoot
`- FaithEffectCard_EmptyStateText
```

현재 카드는 현재 레벨에 실제 적용되는 전체 신앙 기능을 표시한다. 다음 카드는 다음 레벨의 전체 결과를 표시하며 각 Group 또는 Entry에 `강화`, `신규 획득`, `변경 없음` 상태를 붙일 수 있다. 잠긴 전용 기능은 정확한 해금 조건을 표시하고 활성 효과처럼 표현하지 않는다.

### `UIContentInfoView_Faith.prefab`

기존 중립 `UIContentInfoView`의 선택적 레이아웃 Variant다. 기존 Group, Entry, Tag 템플릿을 사용하며 `ShrineGodSO`, `BlessSO`, 캐릭터 직업 의미를 직접 보유하지 않는다.

`UIFaithLevelEffectCard` 내부 Group/Entry 표현에 재사용한다. 기본축복, 전용전직, 전용축복 1, 전용축복 2별 상세 프리팹이나 독립 기능 카드는 만들지 않는다.

## 소유권과 데이터 흐름

```text
ShrineConfigSO + ShrineGodSO + 명시적인 신앙 진행 정의
+ ShrineManager Runtime 상태
-> ShrineFaithPresentationResolver
-> FaithPagePresentationData
-> FaithPagePresenter
-> 신 탭 / 레벨 노드
-> 현재 레벨 / 다음 레벨 비교 Presentation
-> 신앙 효과 카드 두 개
```

- `FaithPagePresenter`가 신앙 선택과 두 비교 카드의 동적 UI 합성을 소유한다.
- `ShrineFaithPresentationResolver`가 Gameplay 의미, 해금 판정, Preview/Runtime 출처를 소유한다.
- 기본축복과 전용축복 상세에서는 `BlessPresentationResolver`를 재사용한다.
- 전용전직은 확인된 Character 직업 정의와 Localization을 사용하며 가짜 Bless로 만들지 않는다.
- 비교 카드 내부에서 재사용하는 `UIContentInfoView` Group/Entry 구조는 콘텐츠 중립성을 유지한다.
- `BlessContentInfoPresenter`는 일반 축복이나 단일 축복 정보에는 사용할 수 있지만 신앙 페이지 소유자가 아니다.

## 제안 도메인 데이터 계약

```text
Assets/Scripts/Stage/NodeContents/Shrine/Faith/
|- Data/FaithPagePresentationData.cs
`- ShrineFaithPresentationResolver.cs
```

작성 Gameplay 정의는 다음을 명시적으로 제공해야 한다.

```text
ShrineGodSO
`- ShrineFaithProgressionSO
   |- BasicBlessProgression
   |  `- 명시적인 레벨별 항목
   |- ExclusiveJobUnlock
   |  |- 해금 레벨
   |  |- 신앙 고정 필요 여부
   |  `- 직업군별 목표 직업 항목
   |- ExclusiveBless1Unlock
   |  |- 신앙 고정 필요 = true
   |  `- BlessSO
   `- ExclusiveBless2Unlock
      |- 해금 레벨 = 8
      |- 신앙 고정 필요 = true
      `- BlessSO
```

기본축복 강화는 명시적인 원본 기반 레벨 항목이나 실제 Runtime 계산 결과를 사용한다. Presentation에서 값을 계산하거나 보간하지 않는다. 레벨마다 별도 `BlessSO`를 사용한다면 각 레벨 효과 카드 안에서 기본축복 Group 하나로 묶는다.

전용전직의 해금 조건은 아직 확정되지 않았다. 신앙 고정 레벨, Character 직업 Enum 이름, 목록 순서에서 추론하지 않고 작성 데이터로 명시해야 한다.

## 페이지 Presentation 데이터

`FaithPagePresentationData`는 다음을 가진다.

- 획득 신 목록과 선택 신 ID
- 신 Identity, 현재 레벨, 친밀도, 다음 레벨 요구량, 고정 상태, 활성 상태
- 정렬된 레벨 1-10 노드
- 현재 레벨 효과 카드와 다음 레벨 효과 카드
- 각 카드 안에서 네 기능 종류로 분류된 Group/Entry
- 명시적인 Preview 또는 Runtime 출처

`FaithLevelEffectComparisonPresentationData`는 다음을 가진다.

- 현재 레벨과 다음 레벨
- 현재 레벨 카드 데이터
- 다음 레벨 카드 데이터
- 최대 레벨 여부

각 카드 안의 `FaithFeaturePresentationData`는 다음을 가진다.

- 종류: BasicBless, ExclusiveJobChange, ExclusiveBless1, ExclusiveBless2
- Localization 원본 Identity와 아이콘
- 해금 레벨과 신앙 고정 필요 여부
- 상태: Acquired, Active, Upcoming, LockedByFaith, Inactive
- 비교 상태: Current, Unchanged, Strengthened, NewlyUnlocked
- 실제 Bless 또는 Character 직업 원본으로 생성한 상세 Content

잠긴 기능의 미리보기는 Preview로 표시하고 활성 Runtime 효과처럼 표현하지 않는다.

## 현재/다음 레벨 비교 규칙

- 왼쪽 카드는 실제 현재 레벨에 적용되는 신앙 효과의 완전한 목록이다.
- 오른쪽 카드는 바로 다음 레벨에 작성된 신앙 효과의 완전한 목록이다.
- 같은 원본 기능 ID가 두 레벨에 모두 있고 작성 값이 바뀌면 `Strengthened`로 분류한다.
- 현재 레벨에 없고 다음 레벨에서 처음 등장하면 `NewlyUnlocked`로 분류한다.
- 두 레벨의 정확한 값을 `현재 값 -> 다음 값`으로 표시할 수 있지만 차이 값을 계산해 새 수치로 만들지 않는다.
- 비교는 Localization 결과 문자열이나 화면 라벨이 아니라 안정적인 원본 기능/Entry ID를 사용한다.
- 원본이 강화 관계를 식별할 수 없으면 `강화`로 추정하지 않고 다음 레벨의 전체 값만 표시한다.
- 최대 레벨에서는 오른쪽 카드 위치를 유지하고 `presentation.faith.next_level.none`에 해당하는 빈 상태를 표시한다.
- 로드맵의 미래 노드는 해금 이정표 확인용이며, 하단 두 카드는 언제나 실제 현재 레벨과 바로 다음 레벨을 기준으로 한다.

## 기능 표시 규칙

### 기본축복

- 현재 카드에는 현재 신앙 레벨의 작성 버전을 표시한다.
- 다음 카드에는 바로 다음 신앙 레벨의 작성 버전을 표시한다.
- 같은 원본 Entry의 값이 달라졌다면 정확한 두 값을 보여주고 `강화` 상태를 표시한다.
- 원본이 제공하지 않으면 레벨 간 차이 값이나 적용 횟수를 계산하지 않는다.

### 전용전직

- 정확한 대상 직업군과 목표 직업을 표시한다.
- Character 직업 Localization과 원본 Identity를 사용한다.
- `BlessContentInfoPresenter`나 Effect Group을 재사용하지 않는다.
- 해금 레벨/조건은 명시적인 사용자 결정과 원본 필드가 추가될 때까지 대기한다.
- 다음 레벨에서 처음 해금되는 경우 다음 카드의 `신규 획득` Group으로 표시한다.

### 전용축복 1

- 해금 조건: 선택된 신앙이 고정됨.
- 상세는 정규화된 Bless/Effect Presentation을 재사용한다.
- 다음 레벨에서 해금 조건이 충족되는 경우 다음 카드의 `신규 획득` Group으로 표시한다.

### 전용축복 2

- 해금 조건: 선택된 신앙이 고정되고 현재 신앙 레벨이 8 이상.
- 상세는 정규화된 Bless/Effect Presentation을 재사용한다.
- 레벨 7에서 고정된 신앙을 볼 때 다음 레벨 카드의 `신규 획득` Group으로 표시한다.
- 그 외 해금 전에는 로드맵 이정표에서 정확한 조건을 표시한다.

## Localization

- 신, Bless, Character 직업 이름/설명은 각 소유자의 StringManager 경로를 사용한다.
- 신앙 라벨, 기능 종류, 해금 조건, 로드맵 상태, 상태 단어는 명시적인 `presentation.faith.*` Key를 사용한다.
- 승인된 Localization 누락은 전체 의도 Key를 표시한다.
- 플레이어 페이지에 `Faith Lv.`나 Enum `ToString()`을 하드코딩하지 않는다.

## 현행 소스 공백

- `ShrineGodSO`에 네 기능 필드가 없다.
- `ShrineBlessingGroup`은 Base와 Enhanced만 제공한다.
- `ShrineGodSO.GetAvailableBlessings`는 Group 인자를 사용하지 않는다.
- `BlessPoolEntry`에는 `progressionStep`만 있고 기능 역할이 없다.
- 현재 `CharacterJob`에는 확인된 신앙 전용전직 매핑이 없다.
- 임계값이 `ShrineConfigSO`, `ShrineGodSO`, `FaithRuntimeData`, `ShrineFaithService`, `ShrineManager`, `ShrineGodInfoPanel`에 중복되거나 하드코딩되어 있다.
- `ShrineGodInfoPanel`은 영문을 하드코딩하며 네 기능 로드맵을 표현하지 못한다.
- 승인된 현행 Bless/Faith 에셋 경로가 없다. 제외된 레거시 `Assets/Resources/shring/`은 구현 기준이 아니다.

최종 프리팹 연결 전에 이 공백을 해결한다.

## 구현 순서

1. 전용전직 해금 규칙과 목표 직업 데이터를 확정한다.
2. 현행 신앙/Bless 작성 경로와 JSON/SO Schema를 확정한다.
3. 명시적인 신앙 진행 정의를 추가하고 임계값 원본을 하나로 통일한다.
4. Runtime 보상/적용 로직이 명시적인 기능 정의를 사용하도록 갱신한다.
5. 신앙 Presentation 데이터와 `ShrineFaithPresentationResolver`를 추가한다.
6. `FaithPagePresenter`와 작은 탭/노드/레벨 효과 카드 View를 추가한다.
7. 사용자가 페이지, 신앙 탭, 레벨 노드, 레벨 효과 카드, 선택적 ContentInfo Variant를 제작하고 연결한다.
8. StringManager Catalog Row와 정적 검증을 추가한다.
9. 사용자가 Unity 프리팹, AutoBind, Play Mode, Scroll, 선택, Localization을 검증한다.

## 검증 항목

- 획득 신앙 없음, 하나, 여러 개
- 신앙 고정 요청, 수락, 거절
- 고정 신앙 레벨 8 미만과 레벨 8 도달
- 기본축복 현재/미래 레벨 Preview
- 현재/다음 카드의 동일 Entry 값 비교와 `강화` 표시
- 다음 레벨 최초 해금 기능의 `신규 획득` 표시
- 최대 레벨에서 다음 카드 빈 상태 유지
- 작성된 모든 직업군의 전용전직
- 전용축복 1/2 잠금 및 해금 Preview
- 페이지가 열린 상태에서 신앙 레벨 변경
- 정의 또는 Localization 누락 진단
- 갱신 후 획득 탭 순서와 선택 유지
- 레거시 Bless 에셋을 읽거나 변경하지 않음

## 제외 범위

- 이 문서는 Gameplay 보상이나 전직 구현을 허가하지 않는다.
- 이번 설계 단위에서는 프리팹, Scene, SO 에셋, 레거시 데이터를 변경하지 않는다.
- 획득한 일반/Common Bless와 활성 Bless 기반 Faith 기능은 탭 없는 보유 효과 페이지의 서로 다른 카테고리 섹션에 표시한다. 일반 축복 도감은 향후 별도 페이지로 만든다. 전체 Faith 진행, 미래 해금, Exclusive Job Change는 향후 명시적인 Effect 원본이 작성되지 않는 한 이 Faith 도감에 유지한다.
- 누락된 해금, 직업, 강화를 이름이나 제외된 레거시 에셋에서 추론하지 않는다.

## 2026-08-21 메인 패널 골격 구현

- 메인 프리팹은 `Assets/Prefabs/UI/Fixed/Panel/Panel_FaithInfo.prefab`이다.
- 기존 배경/전경 시각 요소는 유지하고 구형 `FaithDetailView`, 미완성 `Content`, `FaithNodeRoot`, `Panel_Desc` 구조를 제거했다.
- 실제 하이어라키는 `Faith_Header`, `Faith_GodTabScrollRect`, `Faith_SelectedGodPage`, `Faith_GodSummary`, `Faith_RoadmapScrollRect`, `Faith_LevelEffectComparisonRoot`로 재구성했다.
- `Faith_LevelEffectComparisonRoot` 아래에 `Faith_CurrentLevelEffectCard`와 `Faith_NextLevelEffectCard`를 직접 배치했다. 현재 단계에서는 별도 카드 프리팹으로 추출하지 않고 패널 내부 동일 구조 인스턴스 두 개를 사용한다.
- 두 카드 안에는 기존 `UIContentInfoView.prefab` 인스턴스를 하나씩 넣어 Group/Entry/Scroll 구조를 재사용했다.
- `Faith_GodTabTemplate` 하나와 정적 레벨 노드 10개를 만들었다. 탭과 로드맵 Root는 수평 ScrollRect와 ContentSizeFitter를 사용한다.
- 추가된 Runtime UI 컴포넌트는 `FaithPageView`, `FaithPagePresenter`, `FaithGodTabView`, `FaithLevelNodeView`, `FaithLevelEffectCardView`다.
- `FaithPagePresenter`는 Inspector의 `configuredGods`와 `configuredFaithLevel`, `Build Configured Faith Page` ContextMenu 호출 골격을 제공한다.
- 현행 신앙 진행 원본이 네 기능과 레벨별 비교 데이터를 아직 제공하지 않으므로 현재/다음 `ContentPresentationData` 생성은 `[PLACEHOLDER]`로 남아 있다.
- `AutoBindEditorUtility`가 `[AutoBind] GameObject` 필드를 Component로 조회해 예외를 내던 문제를 수정해 GameObject와 Component를 각각 처리한다.
- `presentation.faith.*` 페이지, 현재/다음 카드, 빈 상태, 상태, 비교 라벨 Key를 추가했다.
- 정적 확인 결과 모든 신규 컴포넌트 참조가 연결됐고 레벨 노드는 정확히 10개며 구형 `FaithDetailView` 참조는 없다. 전체 Solution 빌드는 오류 0개, 기존 경고 209개로 통과했다.
- Unity Editor에서 프리팹 하이어라키와 10개 로드맵 노드의 균등 배치를 확인했다. Play Mode, configured SO 입력, 탭 생성, 카드 데이터, Scroll 입력은 아직 검증하지 않았다.
