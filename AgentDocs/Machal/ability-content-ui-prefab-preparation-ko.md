# Ability 콘텐츠 UI 프리팹 준비 가이드

## 목적과 경계

이 문서는 데이터 계층과 함께 준비한 공통 시각 계층, 레이아웃, 현재 바인딩 경계를 기록한다.

2026-08-10에 사용자가 프리팹 골격과 중첩 샘플 인스턴스를 만들었고, 에이전트가 View, Scene, AutoBind, 게임플레이 해석 동작 없이 레이아웃과 필수 UI 컴포넌트를 적용했다. 2026-08-11에 사용자가 네 공통 View 컴포넌트를 부착했고, 계층 필드가 프로젝트 AutoBind 규칙을 사용하도록 수정되었다.

구체적인 `EquipmentSkillSO`, `EffectSO`, `BlessSO`, `RelicSO`, `CharacterSO` 필드를 이 프리팹에 직접 연결하지 않는다.

## 기존 UI 확인 결과

- `Assets/Prefabs/UIWidget/UITooltipWidget.prefab`은 `Tooltip_ContentText` 하나와 배경만 가진다. 짧은 단일 문자열 Tooltip에는 적합하지만 구조화된 콘텐츠 그룹에는 부족하다.
- 현행 AutoBind는 `[AutoBindPrefix]`와 필드명으로 만든 정확한 자식 오브젝트 이름을 찾아 컴포넌트를 연결한다.
- 동적 콘텐츠는 고정 높이 Row보다 LayoutGroup을 사용한다. ContentSizeFitter는 ScrollRect Content Root처럼 자신의 크기를 소유하는 오브젝트에만 사용하고, 부모 Layout이 Rect를 제어하는 자식에는 중복 적용하지 않는다.

## 현재 프리팹

사용자가 만든 골격과 완성된 레이아웃 경로:

- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoView.prefab`
- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoGroup.prefab`
- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoEntry.prefab`
- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoTag.prefab`

같은 공통 콘텐츠 View로 나중에 Skill, Effect, Bless, Relic, Character Presentation 데이터를 표시할 수 있다.

## 메인 View 계층

다음 계층을 준비한다. 시각 계층을 다르게 만들더라도 AutoBind 오브젝트 이름은 정확히 유지한다.

```text
UIContentInfoView
|- Background
|  `- bg                             Image
|- Header
|  |- Info_IconImage                  Image
|  |- Info_NameText                   TMP_Text
|  `- Info_TagRoot                    RectTransform
`- Body
   |- Info_DescriptionText            TMP_Text
   |- Info_ScrollRect                 ScrollRect
   |  `- Viewport                     RectMask2D
   |     `- Info_GroupRoot            RectTransform
   `- Info_StatusText                 TMP_Text, 선택 사항이며 기본 비활성
```

구현된 View는 `[AutoBindPrefix("Info")]`와 `iconImage`, `nameText`, `tagRoot`, `descriptionText`, `scrollRect`, `groupRoot`, `statusText` 필드를 사용한다. CloseButton은 추가하지 않았으며 Popup 닫기 동작은 후속 View 결정으로 남긴다.

## Group 프리팹

```text
UIContentInfoGroup
|- Group_TitleText                    TMP_Text
|- Group_DescriptionText              TMP_Text, 선택 사항이며 비어 있으면 숨김
`- Group_EntryRoot                    RectTransform
```

Root 또는 EntryRoot에 VerticalLayout을 사용한다. Skill 콘텐츠에서 Group은 하나의 의미 표시 역할을 나타낸다. 현재 Key는 다음과 같다.

- `Activation`: 시전, 대상, Trigger, Chance, 발동 조건
- `Delivery`: 투사체, 이동, Burst, Hit 주기, 전달 형상
- `Outcome`: Damage, Heal, Stat, Cooldown, Periodic Damage, Spawn 결과
- `SpecialEffect`: 원본 기반 Duration, Force, Distance를 포함한 `Control`과 `Displacement`
- `LinkedSkill`: `SkillInvoke` 참조

Effect의 7개 정규화 Outcome은 이 Grouping 아래의 Typed 데이터로 유지한다. Skill UI는 Effect마다 Group을 생성하지 않는다. 각 원본 필드는 계속 별도 Entry가 되며 Group 통합 과정에서 값을 결합하거나 유도하지 않는다.

각 영역을 위한 고정 자식 오브젝트를 만들지 않는다. Resolve된 데이터에 존재하는 Group만 생성한다.

## Entry 프리팹

```text
UIContentInfoEntry
|- Entry_LabelText                    TMP_Text
|- Entry_ValueText                    TMP_Text
`- Entry_DetailButton                 Button, 선택 사항이며 기본 비활성
```

일반 Label/Value Row에는 HorizontalLayout을 사용한다. 두 Text 모두 줄바꿈을 허용하고 Row 높이를 고정하지 않는다.

원본 필드 하나는 Entry 하나와 Value 하나로 변환한다. `projectileColliderRadius`와 `projectileLifetime`처럼 서로 다른 원본 필드를 한 행에 결합하지 않는다. Label 문구만 효과 범위나 지속시간 같은 플레이어용 표현으로 변환할 수 있다.

`Entry_DetailButton`은 중첩 Skill처럼 별도 화면으로 열 콘텐츠를 위해 남겨둔다. 부모 패널에 중첩 Skill 상세를 모두 펼치지 않는다.

## Category 또는 Tag 프리팹

```text
UIContentInfoTag
`- Tag_Text                           TMP_Text
```

TagRoot는 HorizontalLayout으로 0개 이상의 Tag를 표시한다. 실제 분류 개수가 여러 행을 요구할 때 줄바꿈을 후속 개선한다.

## 데이터와 프리팹 연결 표

| Presentation 데이터 | 프리팹 대상 |
| --- | --- |
| Identity Icon | `Info_IconImage` |
| Identity 표시 이름 | `Info_NameText` |
| 작성된 설명 | `Info_DescriptionText` |
| Classification 항목 | `Info_TagRoot` 아래 Tag 인스턴스 |
| Presentation Group | `Info_GroupRoot` 아래 Group 인스턴스 |
| Group Label | `Group_TitleText` |
| 선택적인 Group 설명 | `Group_DescriptionText` |
| Entry Label | `Entry_LabelText` |
| 후속 단계에서 포맷한 간결한 값 | `Entry_ValueText` |
| 중첩 콘텐츠 이동 | `Entry_DetailButton` |
| 미지원 또는 설명 전용 상태 | `Info_StatusText` 또는 작성된 설명 |

일반 플레이어용 프리팹에는 Provenance 표시가 필요하지 않다. 이후 Editor Preview나 Debug Overlay에서 별도로 표시할 수 있다.

## AutoBind 경계

- `UIContentInfoView`의 일곱 계층 컴포넌트 필드는 `Info` 접두사를 사용한다.
- `UIContentInfoGroupView`의 세 계층 컴포넌트 필드는 `Group` 접두사를 사용한다.
- `UIContentInfoEntryView`의 세 계층 컴포넌트 필드는 `Entry` 접두사를 사용한다.
- `UIContentInfoTagView`의 텍스트 필드는 `Tag` 접두사를 사용한다.
- 현재 AutoBind 도구는 프리팹 에셋 참조가 아니라 프리팹 계층의 컴포넌트를 해석하므로 `tagPrefab`, `groupPrefab`, `entryPrefab`은 수동 연결을 유지한다.
- Unity에서 스크립트를 다시 컴파일하고 `OnValidate`가 실행된 뒤 사용자가 네 프리팹을 저장해야 한다.

## 구현된 레이아웃

- View는 기준 크기 700 x 1000, 높이 170의 Header, 사방 24 여백을 가진 Stretch Body를 사용한다.
- `Info_ScrollRect`는 세로 전용이며 `RectMask2D` Viewport와 `Info_GroupRoot` Content를 연결한다.
- `Info_GroupRoot`는 ScrollRect Content Root이므로 VerticalLayout과 Preferred Height ContentSizeFitter를 사용한다.
- 정적 YAML 검사에서 현재 Viewport에는 `RectTransform`과 `RectMask2D`만 있고 Raycast 가능한 `Graphic`은 없다. 활성 자식 Graphic이 없는 영역에서 Wheel과 Drag 입력을 확인하고, 사용자가 Unity에서 입력 공백을 확인한 경우에만 투명 Raycast Target `Image`를 추가한다.
- Group Root와 `Group_EntryRoot`는 VerticalLayout이 Preferred Size를 부모 Layout에 제공하며 중첩 `ContentSizeFitter`를 사용하지 않는다.
- Entry는 부모 Layout이 크기를 제어하는 가로 Label/Value Row이며 줄바꿈과 Layout 계산 Preferred Height를 지원한다.
- `Entry_DetailButton`, `Group_DescriptionText`, `Info_StatusText`는 존재하며 기본 비활성이다.
- Tag는 옅은 Image 배경, TMP Text, 가로 Padding을 사용하고 중첩 `ContentSizeFitter` 없이 부모 Layout이 Preferred Size를 적용한다.
- 최종 색상, Sprite, Localization, 콘텐츠별 시각 보정은 이번 레이아웃 범위에서 제외한다.

## 아직 준비하지 않을 항목

- Effect Config 13종을 위한 고정 필드
- Effect Config별 별도 프리팹
- 원본 `ValueOverride` 또는 Upgrade Modifier 필드
- Interval과 Duration으로 계산한 적용 횟수 필드
- JSON에만 있는 게임플레이 값
- 프리팹 Text에 하드코딩한 최종 Localization 문장
- 공통 콘텐츠 View의 구체 SO 참조
- Scene 연결과 구현된 Character, Skill, Bless, Relic Presenter의 Unity 측 부착 및 설정
- Skill 전용 `EquipmentSkillSO`와 `Build Presentation` 동작의 최종 소유 결정. 명시적으로 결정하기 전에는 `SkillContentInfoPresenter`에 유지한다.
- Viewport의 Unity Wheel/Drag 검증 및 필요한 경우 투명 Raycast Target `Image` 추가

## 완료 체크리스트

- 메인 콘텐츠 View 계층이 존재한다.
- 정확한 Binding 오브젝트 이름을 유지했다.
- 네 View 컴포넌트가 부착되었고 계층 필드가 대응하는 AutoBind 접두사를 사용한다.
- 템플릿 프리팹 에셋 필드는 명시적인 수동 지정으로 유지한다.
- Group, Entry, Tag Template을 별도 재사용 프리팹으로 만들었다.
- GroupRoot가 늘어나고 Scroll될 수 있다.
- Label/Value Row는 줄바꿈을 지원하며 Row마다 원본 값 하나만 표시한다.
- 선택 오브젝트를 숨겨도 Layout이 깨지지 않는다.
- 중첩 콘텐츠 이동용 선택 Button 자리를 남겼다.
- 런타임 SO 또는 레거시 데이터를 프리팹에 추가하지 않았다.
