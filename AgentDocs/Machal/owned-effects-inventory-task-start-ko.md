# 보유 효과 인벤토리 새 채팅 작업 시작 계약

- 분류: `[GUIDE]`
- 경로: `AgentDocs/Machal/owned-effects-inventory-task-start.md`
- 한국어: `AgentDocs/Machal/owned-effects-inventory-task-start-ko.md`
- 상태 기준일: 2026-08-18

## 목적

ProjectBS 보유 효과 인벤토리 작업을 새 채팅에서 이어가기 위한 작업 전용 진입 계약이다. 저장소 규칙이나 상세 Ability Presentation 문서를 대체하지 않는다. 다음 에이전트가 무엇을 읽고, 어떤 설계를 권위 있게 사용하며, 무엇이 구현되었고, 다음에 어떤 단일 작업만 진행해야 하는지 명시한다.

## 새 채팅에 붙여 넣을 작업 요청

```text
ProjectBS의 탭 없는 보유 효과 인벤토리 작업을 이어서 진행해.

파일을 분석하거나 변경하기 전에 AGENTS.md, AgentDocs/task-start-documentation-prompt.md, AgentDocs/Machal/README.md, AgentDocs/Machal/owned-effects-inventory-task-start-ko.md를 처음부터 끝까지 읽어. 그다음 작업 시작 계약의 필수 읽기 순서를 모두 따라. C#을 변경하기 전에는 AgentDocs/code-writing-rules.md도 읽어.

문서를 읽은 뒤 먼저 현재 확정 설계, 이번에 진행할 단일 구현 단계, 수정 예정인 정확한 경로, 사용자가 담당하는 Unity 작업을 요약해서 보고해. 관련 없는 수정 및 미추적 파일은 전부 보존해. reset, clean, commit, push, 레거시 데이터 마이그레이션, 프리팹 YAML 직접 수정, Unity 조작은 하지 마.

시작 계약에 기록된 다음 작업 단위 하나만 진행해. Unity Import, 컴포넌트 부착, AutoBind, 프리팹 편집, Scene 연결, Play Mode 검증이 필요해지면 작업을 멈추고 나에게 요청해. 작업 단위가 끝나면 영문 원본 AgentDocs와 한국어 Mirror를 함께 갱신해.
```

## 필수 읽기 순서

구현 전에 다음 필수 파일을 모두 처음부터 끝까지 읽는다.

1. `AGENTS.md`
2. `AgentDocs/task-start-documentation-prompt.md`
3. `AgentDocs/Machal/README.md`
4. `AgentDocs/Machal/owned-effects-inventory-task-start-ko.md`
5. `AgentDocs/Machal/basic-work-guide.md`
6. `AgentDocs/Machal/ability-content-presentation-task.md`
7. `AgentDocs/Machal/ability-content-presentation-inventory.md`
8. `AgentDocs/Machal/ability-content-presentation-contract-evaluation.md`
9. `AgentDocs/Machal/ability-content-presentation-display-catalog.md`
10. `AgentDocs/Machal/ability-content-presentation-stage4-verification.md`
11. `AgentDocs/Machal/ability-content-presentation-stage5-8-completion.md`
12. `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`
13. `AgentDocs/Machal/character-skill-content-tabs.md`
14. `AgentDocs/Machal/character-content-presentation.md`
15. `AgentDocs/Machal/faith-page-design.md`
16. `AgentDocs/Machal/ability-content-presentation-log.md`
17. Script 또는 Code 변경 전 `AgentDocs/code-writing-rules.md`
18. 해당 작업 단위에 대해 활성 Task 계약이 지정한 모든 정확한 원본 경로

필수 경로가 하나라도 없으면 추측하지 않는다. 누락 경로를 Task 로그에 기록하고 사용자에게 보고한다.

## 현재 확정 설계

### 보유 효과 페이지

- 탭이 없다.
- 세로 `ScrollRect` 하나를 가진 인벤토리 형태의 단일 페이지다.
- Scroll Content 아래에 정렬된 카테고리 섹션을 동적으로 생성한다.
- 현재 보유하거나 활성화된 콘텐츠만 표시한다.
  - 보유 유물,
  - 획득한 일반 축복,
  - 활성 상태인 Bless 기반 신앙 효과.
- 표시할 항목이 없는 카테고리는 생략할 수 있다.
- 모든 카테고리는 같은 카테고리 섹션 View와 아이템 View 시스템을 사용한다.
- 어떤 아이템을 선택해도 콘텐츠 중립적인 공용 `UIContentInfoView` 하나에 연결한다.
- 보유 효과 페이지는 `Catalog` 모드를 사용하지 않는다.
- 향후 명시적인 Effect 원본이 작성되지 않는 한 전용전직은 제외한다.

### 별도 도감 페이지

- 유물 도감은 별도 페이지이며 `Catalog` 모드에서 획득/미획득 유물을 표시할 수 있다.
- 일반 축복 도감은 별도 페이지이며 `Catalog` 모드에서 획득/미획득 일반 축복을 표시할 수 있다.
- 신앙 도감은 별도로 유지하며 신앙 진행, 비활성 기능, 향후 해금, 전용전직을 소유한다.
- 공통 아이템/카테고리/상세 표시를 재사용할 수 있지만 각 페이지는 자체 도메인 Presenter와 원본 정책을 유지한다.

### 레이아웃 경계

```text
Panel_OwnedEffects
├─ OwnedEffectsPresenter
├─ OwnedEffectsPageView
│  └─ 세로 ScrollRect 하나
│     └─ CategoryRoot
│        ├─ 유물 카테고리 섹션        (Runtime 생성)
│        ├─ 일반 축복 섹션            (Runtime 생성)
│        └─ 활성 신앙 축복 섹션       (Runtime 생성)
└─ 공용 UIContentInfoView 하나
```

각 카테고리 안에 세로 `ScrollRect`를 넣지 않는다. 카테고리 섹션은 Header와 Item Grid만 가지며 페이지가 목록 Scroll 하나를 소유한다.

## 현재 저장소 상태

### 구현 및 검증 완료

- `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryData.cs`
  - `ContentInventoryDisplayMode`
  - `ContentAcquisitionState`
  - `ContentActivationState`
  - `ContentInventoryItemData`
  - `ContentInventoryCategoryData`
  - `ContentInventoryPageData`
- `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryItemView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryCategoryView.cs`
- 임시 프로젝트 포함 후 2단계의 깨끗한 Solution 정적 빌드는 오류 0개, 기존 경고 197개로 통과했다.
- 검증 후 생성 프로젝트 파일을 복구했다.

`ContentInventoryDisplayMode.Catalog`는 별도 도감 페이지에 유효하므로 유지한다. 보유 효과 페이지 옵션으로 사용하면 안 된다.

### 대체된 레거시 단위

- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryData.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectGridItemView.cs`
- 이전에 `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryView.cs`와 `OwnedEffectInventoryPresenter.cs`에 있던 구형 네 탭 구현

`OwnedEffectInventoryData.cs`와 `OwnedEffectGridItemView.cs`는 연결되지 않은 레거시 단위로 남아 있다. View/Presenter 두 경로에는 현재 탭 없는 카테고리 섹션 구현이 들어 있으므로 이전 네 탭 동작을 복원하지 않는다.

### 다른 페이지를 위해 보존

- 향후 유물 도감을 위해 `RelicCollectionView`를 유지한다.
- 이후 Task가 전용 페이지 역할을 명시적으로 변경하기 전에는 독립 `RelicContentInfoPresenter`와 `BlessContentInfoPresenter` 동작을 유지한다.
- `UIContentInfoView`는 콘텐츠 중립성을 유지한다. `RelicSO`, `BlessSO`, 보유 규칙을 직접 해석하면 안 된다.

### 2026-08-18 직접 연결 완료

- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryView.cs`는 비어 있지 않은 카테고리마다 카테고리 프리팹을 하나씩 만들고, 카테고리 전체 선택 상태와 공용 상세 View 연결을 소유한다.
- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryPresenter.cs`는 `OwnedOnly` 모드에서 보유 유물, 일반 축복, 활성 신앙 축복 카테고리를 순서대로 구성한다.
- `Assets/Prefabs/UI/Fixed/Panel/Panel_OwnedEffects.prefab`에 두 컴포넌트와 페이지/상세/카테고리 참조를 모두 연결했다.
- `Assets/Prefabs/UI/Fixed/Content/UIContentInventoryCategory.prefab`에 `ContentInventoryCategoryView`, 제목/개수/Root 참조, 아이템 프리팹을 연결했다.
- `Assets/Prefabs/UI/Fixed/Content/UIInventoryItemView.prefab`에 `ContentInventoryItemView`를 연결하고 기존 자식 `UISelectableIconButton`을 지정했다.
- `Panel_OwnedEffects` 내부 공용 상세 오브젝트에서 독립 `RelicContentInfoPresenter`만 제거했다. 해당 소스와 전용 페이지 동작은 보존한다.
- 공용 상세 오브젝트는 기본 활성 상태다.
- 페이지 및 카테고리 제목용 StringManager 행 네 개를 추가했다.
- 이 프리팹 YAML 작업은 사용자가 이번 작업에서 직접 연결을 명시적으로 요청했기 때문에 수행했다. 이후 프리팹 YAML 수정 권한으로 확대하지 않는다.
- 정적 검증: `dotnet build ProjectBS.sln --no-restore -v:minimal`은 오류 0개, 기존 경고 197개로 통과했고 직렬화 참조 및 로컬라이징 중복 검사도 통과했다.

### 미구현 또는 미검증

- Runtime Manager 자동 수집,
- Unity Import 및 Play Mode 검증,
- 별도 유물/일반 축복 도감 페이지.

Checkout에는 관련 없는 수정 및 미추적 작업이 많이 존재한다. 이 Task는 reset, clean, commit, push를 허용하지 않는다. 각 작업 단위 전후에 범위 내 경로만 검사한다.

## 다음 단일 작업 단위

직접 연결된 보유 효과 페이지를 사용자가 Unity에서 검증한다.

1. Unity가 변경된 Script와 Prefab을 Import하고 Compile하게 한 뒤, Console 오류가 있으면 관련 없는 파일을 고치지 말고 그대로 보고한다.
2. `Assets/Prefabs/UI/Fixed/Panel/Panel_OwnedEffects.prefab`을 열어 Root에 `OwnedEffectInventoryView`와 `OwnedEffectInventoryPresenter`가 있고 페이지 참조가 모두 비어 있지 않은지 확인한다.
3. Presenter에 테스트용 Relic, 일반 Bless, Faith Bless SO 목록을 할당한다.
4. Play Mode에서 `buildOnStart`가 꺼져 있으면 `Build Configured Owned Effects`를 실행하고, 켜져 있으면 자동 구성을 확인한다.
5. 비어 있지 않은 각 카테고리, 아이템 클릭과 선택 이동, 항상 활성인 `UIContentInfoView` 상세 연결, 외부 세로 ScrollRect를 확인한다.
6. 실패하면 정확한 Console 메시지 또는 실패한 상호작용을 보고한다. 그 증거가 있어야 코드 수정을 시작한다.

이 Unity 검증을 통과한 뒤 다음 코드 단위는 보유 효과 Presenter의 Runtime 원본 자동 수집이다. 별도 일반 축복 및 유물 도감 통합은 이후 독립 작업으로 남긴다.

## 작업 방식

- 독립적으로 검증할 수 있는 한 단위씩 작업한다.
- 편집 전에 정확한 범위 경로를 보고하고 겹치는 기존 작업을 보존함을 확인한다.
- `AgentDocs/code-writing-rules.md`를 따른다. 먼저 호출 가능하고 컴파일 가능한 구조를 만들며 임시 구현 지점은 필요할 때 정직하게 표시한다.
- 플레이어 View에 원본 JSON/C# Key나 임의 생성 Pascal Case Label을 노출하지 않는다.
- 이름, 설명, 카테고리 제목, Label, Tag, Enum 대체 단어, Format은 기존 StringManager/`PresentationDisplayCatalog` 계약을 따른다.
- 승인된 Localization이 누락되면 의도 Key가 보이도록 유지하며 창작 Fallback 텍스트로 바꾸지 않는다.
- 레거시 Effect, Bless, Relic 에셋을 편집하거나 마이그레이션하지 않는다.
- Presentation UI를 만드는 과정에서 관련 없는 Gameplay 보유 동작을 변경하지 않는다.
- 사용자의 명시적 요청 없이 Commit하거나 Push하지 않는다.

## Unity 작업 경계

이 Task의 Unity 작업은 사용자가 담당한다. 다음 작업이 필요해지면 멈추고 사용자에게 요청한다.

- Unity Script Import 또는 Console 확인,
- 컴포넌트 추가/제거,
- AutoBind 실행 또는 직렬화 참조 확인,
- 프리팹 생성, 복제, 하이어라키 편집, 저장,
- UnityEvent/Button 연결,
- Scene 연결,
- Play Mode 입력, Scroll, 선택, Localization, 시각 검증.

정적 Source 검사와 `.NET` 컴파일은 Unity, Prefab, AutoBind, Play Mode 동작을 증명하지 않는다.

## 작업 단위 종료 보고

다음을 분리하여 보고한다.

1. 완료된 동작,
2. 정확한 변경 경로,
3. 정적 검증 근거,
4. 남은 Unity/사용자 검증,
5. 다음 단일 작업 단위,
6. 미해결 원본 또는 설계 Blocker,
7. 갱신한 문서 경로.

같은 작업 단위에서 영문 원본 문서와 한국어 `-ko` Mirror를 함께 갱신한다. Task Log의 검증 이력은 다시 쓰지 않고 추가한다.
