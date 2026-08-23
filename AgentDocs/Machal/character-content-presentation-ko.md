# Character 콘텐츠 Presentation

## 목적

플레이어에게 필요한 Character 작성 데이터를 전용 `UIContentInfoView`에 표시하고, 원본 비교를 위한 전체 JSON 및 SO 검사 출력은 별도로 유지한다.

## 승인된 원본

- JSON: `Assets/Resources/character/json/*.json`
- 생성 SO: `Assets/Resources/character/json/*.asset`
- 현재 인벤토리: JSON 22개와 대응 CharacterSO 22개
- Generator: `Assets/Editor/tools/character/CharacterJsonGenerator.cs`

현재 JSON에는 `characterId`, `name`, `characterType`, `job`, `baseStats`만 있다. Animation Clip과 Skill 참조는 Builder가 CharacterSO에 생성하는 값이며 Character JSON 작성 필드가 아니다.

## 플레이어 표시 경계

| 원본 | 플레이어 UI | 검사 도구 |
| --- | --- | --- |
| `name` | `StringManager.Get(characterId, "name")`로 표시 | 원본 JSON 이름과 해석된 StringManager 값 |
| `characterType` | Localization된 Tag | 원본 Enum과 Localization된 Tag |
| `job` | Localization된 Tag 하나 | 원본 Job과 파생 내부 분류 |
| `baseStats` | 원본 Stat마다 Localization된 Entry 하나 | 모든 원본 Stat/값/Provenance |
| `characterId` | Row로 숨김, Identity/Provenance에 유지 | 표시 |
| Animation Clip | 숨김 | SO 전용 시스템 데이터로 개수 표시 |
| Skill 참조 / `slotKey` | Character 본문에서는 숨기고 Skill 탭에서 사용 | SO 전용 시스템 데이터로 전체 참조 표시 |
| Runtime 상태와 파생 Job 요소 | 작성 Character UI에서 숨김 | 검사/Runtime 데이터 전용 |

숫자는 결합하거나 대체하지 않는다. 현재 단위 해석은 실제 원본과 Runtime 근거를 따른다.

- `Attack`, `Defense`, `MaxHp`: Flat
- `AttackSpeed`: 원본 숫자에 Localization된 배율 포맷 적용
- `CritChance`, `CritDamage`: Runtime에서 100으로 나누므로 Percent
- `MoveSpeed`: m/s

## Runtime UI 흐름

```text
CharacterSO
  -> CharacterContentInfoPresenter
  -> CharacterPresentationResolver.ResolveForPlayerDisplay
  -> PresentationDisplayCatalog
  -> StringManager 기반 Formatter
  -> CharacterContentInfoView (UIContentInfoView)
```

`CharacterContentInfoPresenter`는 선택적으로 같은 CharacterSO를 `CharacterSkillContentInfoPresenter`에 전달할 수 있다. 이 경우 Character 본문과 Skill 탭이 하나의 선택 원본을 사용한다.

## 비교 도구

다음 중 하나로 연다.

- `Tools > ProjectBS > Presentation > Open Character Data Preview`
- CharacterSO 우클릭 후 `Assets > ProjectBS > Presentation > Preview Selected Character`

창에는 독립적으로 스크롤되는 열 세 개가 있다.

1. `Original JSON`: 승인된 경로의 TextAsset 원문.
2. `SO Inspection (all)`: 필터링하지 않은 Presentation과 SO 전용 Animation/Skill 시스템 참조.
3. `Player UI (filtered)`: Runtime View에서 사용할 필터링 데이터와 동일한 StringManager 카탈로그 출력.

상단에서 `characterId`, `characterType`, `job`, 정렬된 `baseStats`, 숫자 값, 한국어 Character 이름 Row의 불일치를 보고한다.

## 필요한 Unity 연결

이번 작업 단위에서 에이전트는 프리팹을 수정하거나 Unity를 조작하지 않았다.

1. Character 본문용 `UIContentInfoView` 인스턴스를 별도로 추가하고 GameObject 이름을 `CharacterContentInfoView`로 맞추거나 직접 연결한다.
2. 상위 Panel에 `CharacterContentInfoPresenter`를 추가한다.
3. 현재 CharacterSO를 연결한다.
4. Character 본문 View를 `contentView`에 연결한다.
5. Character 선택을 동기화하려면 기존 `CharacterSkillContentInfoPresenter`를 `skillTabs`에 선택적으로 연결한다.
6. Play Mode에 진입하고 `buildOnStart`를 껐다면 컴포넌트 Context Menu의 `Build Character Presentation`을 실행한다.

## 현재 예상 플레이어 출력

- Localization된 Character 이름
- Character Type Tag(현재 승인 JSON에서는 `Npc` 또는 `Boss`)
- Job Tag(`SoldierBase`, `ArcherBase`, `ScholarBase`, `MonkBase`)
- 현재 원본 Stat 7개가 들어 있는 `Character Stats` Group 하나
- Character ID, Animation Clip, Skill 참조, slotKey, 파생 Job 요소, Runtime 상태 Row 없음

## 검증

- 승인 JSON 22개 모두 Strict UTF-8로 파싱됐다.
- JSON 22개 모두 생성 SO의 ID, Type, Job, 정렬된 Stat Type, 숫자 값과 일치한다.
- JSON 이름 22개 모두 `character_string.csv`의 한국어 이름 Row 하나와 정확히 일치한다.
- `presentation_string.csv`는 데이터 308행이며 대소문자 무시 복합 Key 중복은 0개다.
- 신규 Presenter를 임시 포함한 Runtime Assembly 빌드: 오류 0개, 기존 경고 35개.
- 최종 비교 창을 임시 포함한 Editor Assembly 빌드: 오류 0개, 전체 경고 197개. 신규 검사 도구의 JsonUtility DTO에 대한 예상된 `CS0649` 경고를 포함한다.
- 검증 후 생성 프로젝트의 임시 Compile 항목을 제거했다. Unity가 이를 정상적으로 다시 생성한다.
- Unity 프리팹 연결 및 Play Mode 화면 검증은 사용자 담당으로 대기한다.

## 변경된 Runtime 및 도구 경로

- `Assets/Scripts/Actor/Character/Data/CharacterPresentationData.cs`
- `Assets/Scripts/Actor/Character/CharacterPresentationResolver.cs`
- `Assets/Scripts/Actor/Character/ui/CharacterContentInfoPresenter.cs`
- `Assets/Scripts/Presentation/PresentationDisplayCatalog.cs`
- `Assets/Scripts/Presentation/PresentationTextFormatter.cs`
- `Assets/Editor/tools/character/CharacterPresentationPreviewWindow.cs`
- `Assets/Resources/string/character_string.csv`
- `Assets/Resources/string/presentation_string.csv`
