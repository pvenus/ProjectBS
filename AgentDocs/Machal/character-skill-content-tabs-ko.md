# Character Skill 콘텐츠 탭

## 목적

하나의 `CharacterSO`가 참조하는 Skill을 아이콘 탭으로 표시한다. 탭을 선택하면 기존 `UIContentInfoView`에 표시되는 콘텐츠가 해당 Skill의 Presentation 데이터로 교체된다.

## 소유권과 데이터 흐름

```text
CharacterSO.Skills
  -> CharacterSkillContentInfoPresenter
  -> SkillContentInfoTabButton 인스턴스
  -> 선택된 EquipmentSkillSO
  -> SkillContentInfoPresenter.ShowSkill
  -> SkillPresentationResolver
  -> SkillPresentationGroupResolver.ResolveForPlayerDisplay
  -> UIContentInfoView
```

- `CharacterSkillContentInfoPresenter`가 Character Skill 순서, 탭 생성, 선택을 소유한다.
- `SkillContentInfoTabButton`은 아이콘 하나, 클릭 동작 하나, 선택 상태 화면만 소유한다.
- `SkillContentInfoPresenter`는 계속 구체적인 `EquipmentSkillSO` Presentation의 소유자다.
- `UIContentInfoView`는 콘텐츠 중립 상태를 유지하며 `CharacterSO`나 `EquipmentSkillSO`에 의존하지 않는다.
- Skill 이름, 설명, 라벨, 토큰, 포맷은 기존 StringManager 기반 플레이어 Formatter 경로를 그대로 사용한다.

## 구현 파일

- `Assets/Scripts/Actor/Character/ui/CharacterSkillContentInfoPresenter.cs`
- `Assets/Scripts/Ability/Skills/UI/SkillContentInfoTabButton.cs`
- `Assets/Scripts/Ability/Skills/UI/SkillContentInfoPresenter.cs`

Character 조합 계층이 Skill 정규화나 포맷을 중복하지 않도록 `SkillContentInfoPresenter`에 `ShowSkill(EquipmentSkillSO)`와 `ClearPresentation()`을 공개했다.

## 동작 계약

- 탭 순서는 `CharacterSO.Skills` 순서와 같다.
- null이 아닌 `CharacterSkillEntry.skillSo`마다 탭 하나를 만든다.
- null Skill 슬롯은 원본 Index와 함께 경고하고 건너뛴다. 창작한 임시 Skill은 만들지 않는다.
- 설정된 `initialSelectedIndex`는 실제 생성된 탭 범위로 제한한다.
- 선택한 탭은 즉시 기존 Skill 플레이어 표시 경로를 호출한다.
- 다시 빌드할 때 이전 동적 탭을 먼저 제거한 뒤 새 목록을 만든다.
- Play Mode에서 `SetCharacter(character, rebuild: true)`로 Character를 교체할 수 있다.
- 선택된 탭은 클릭 불가 상태가 되고, 필요하면 전용 `selectedVisual`을 활성화할 수 있다.

## 필요한 Unity 프리팹 연결

이번 작업 단위에서 에이전트는 Unity를 조작하거나 프리팹 YAML을 수정하지 않았다. 사용자가 Unity에서 다음 작업을 수행해야 한다.

1. `Assets/Prefabs/UI/Child/Slot/UISkillIconSlot.prefab`을 연다.
2. `UISkillIconSlot`이라는 루트 GameObject에 `Button` 컴포넌트를 추가한다.
3. 같은 루트에 `SkillContentInfoTabButton`을 추가한다.
4. AutoBind가 다음을 연결했는지 확인한다.
   - `button` -> 루트 `UISkillIconSlot` Button.
   - `skillIconImage` -> 자식 `Bind_SkillIconImage` Image.
5. 필요하면 선택 프레임 GameObject를 `selectedVisual`에 연결한다. 연결하지 않아도 선택된 Button이 클릭 불가 상태가 되어 선택을 표현한다.
6. 콘텐츠 Panel에 `CharacterSkillTabRoot`라는 `RectTransform`을 만들거나 기존 오브젝트 이름을 맞추고 원하는 Horizontal/Grid Layout 컴포넌트를 추가한다.
7. 기존 Skill 콘텐츠 Presenter를 포함한 Panel에 `CharacterSkillContentInfoPresenter`를 추가한다.
8. `CharacterSO`와 `UISkillIconSlot` 프리팹의 컴포넌트를 `skillTabPrefab`에 연결한다.
9. `skillPresenter`가 기존 `SkillContentInfoPresenter`를 참조하는지 확인한다. 해당 GameObject 이름이 `SkillContentInfoPresenter`이면 AutoBind할 수 있고, 아니면 직접 연결한다.
10. Play Mode에 진입한다. `buildOnStart`가 자동으로 탭을 만들며, 컴포넌트 Context Menu의 `Build Character Skill Tabs`를 사용할 수도 있다.

## 수동 검증 체크리스트

- 생성 프로젝트 파일 갱신 후 Unity가 신규 오류 없이 컴파일된다.
- 생성된 탭 수가 선택한 `CharacterSO`의 null이 아닌 Skill 참조 수와 같다.
- 탭 순서가 Character 에셋 순서와 같다.
- 설정한 최초 탭이 선택되고 아이콘이 올바르다.
- 다른 탭을 각각 클릭하면 같은 `UIContentInfoView`의 Skill 이름, 설명, Tag, Group, Entry, 아이콘이 바뀐다.
- 클릭 후에도 정확히 하나의 탭만 선택 상태다.
- `Build Character Skill Tabs`를 다시 실행해도 중복된 보이는 탭이 남지 않는다.
- `SetCharacter`로 Character를 교체하면 목록을 다시 만들고 유효한 최초 탭을 선택한다.
- 여러 번 탭을 바꾼 뒤에도 기존 콘텐츠 스크롤이 동작한다.
- Localization이 누락되면 기존 계약대로 의도한 Key가 표시된다.

## 검증 상태

- 소스 구조와 현재 `UISkillIconSlot.prefab` YAML을 확인했다.
- 현재 프리팹에는 아이콘 하이어라키가 있지만 `Button`과 `SkillContentInfoTabButton`은 아직 없다.
- 생성된 `Assembly-CSharp.csproj`에는 현재 두 신규 소스 파일이 모두 포함되어 있다.
- 현재 `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal` 시도도 샌드박스가 `C:\Users\machal89\AppData\Local\Microsoft SDKs`를 읽지 못해 C# 컴파일 전에 중단됐다. 이 접근 실패는 신규 스크립트의 컴파일 결과가 아니다.
- Unity 컴파일, AutoBind, 프리팹 저장, 버튼 입력, 선택 화면, Play Mode 출력은 사용자 담당으로 대기한다.

## 제외 범위

- Character, Skill, Effect, Bless, Relic 원본 에셋을 수정하지 않았다.
- 구형 에셋을 구현 원본으로 읽거나 마이그레이션하지 않았다.
- Localization 대체 경로를 추가하거나 변경하지 않았다.
- Scene 연결, 프리팹 변경, Unity Editor 조작, Commit, Push를 수행하지 않았다.

## 2026-08-13 캐릭터 목록 탐색 확장

`Assets/Prefabs/UI/Fixed/Panel/Panel_CharacterInfo.prefab`에서는 이제
`CharacterSkillContentInfoPresenter`가 캐릭터 선택을 소유한다.

- 직렬화된 `List<CharacterSO>`가 캐릭터 탐색 순서를 정의한다.
- `initialCharacterIndex`가 시작 시 처음 표시할 캐릭터를 선택한다.
- `ShowPreviousCharacter()`와 `ShowNextCharacter()`는 사용자가 연결할 버튼용 공개 동작이다.
- `loopCharacterSelection`으로 목록 양 끝에서 순환할지 결정한다. 순환하지 않으면 더 이동할 수 없는 동작을 무시한다. `CanShowPreviousCharacter`와 `CanShowNextCharacter`는 Button 상태를 소유하지 않고 현재 이동 가능 여부만 제공한다.
- 목록의 null 항목은 원본 인덱스를 로그로 남기고 제외한다. 목록이 비어 있으면 기존 단일 `character` 필드를 한 캐릭터용 호환 입력으로 유지한다.
- 캐릭터가 바뀌면 해당 캐릭터의 스킬 아이콘 탭을 다시 만들고 기존 `CharacterContentInfoPresenter`를 호출하여 캐릭터 본문과 스킬 페이지를 동기화한다.
- `initialSelectedSkillIndex`는 `FormerlySerializedAs`를 통해 기존 `initialSelectedIndex` 값을 유지한다.
- `SetCharacters`, `SelectCharacter`, 기존 `SetCharacter`로 CharacterSO 에셋을 변경하지 않고 런타임 목록과 선택을 교체할 수 있다.

프리팹은 기존 `CharacterContentInfoPresenter`를 캐릭터 목록 소유자에 연결하고, 해당 Presenter의 독립적인 `buildOnStart`를 껐다. 따라서 이전에 서로 달랐던 두 직렬화 Character가 `Start` 순서에 따라 경쟁하지 않는다.

사용자 소유 Unity 작업 경계에 따라 탐색 Button 오브젝트를 생성하거나 이벤트를 연결하지 않았다. 사용자가 버튼을 만들고 `ShowPreviousCharacter()`와 `ShowNextCharacter()`에 이벤트를 연결한 뒤, `characters` 목록을 채우고 Play Mode 탐색을 검증한다.

정적 검증: `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`이 오류 0개, 기존 경고 35개로 완료되었다. Unity는 열거나 조작하지 않았다.
