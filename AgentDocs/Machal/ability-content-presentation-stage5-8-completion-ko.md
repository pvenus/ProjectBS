# 5-8단계: Skill 조합과 UI 인계

## 상태

- 코드 구현: 2026-08-11 완료
- 중첩 Skill 순회와 상세 확장: 사용자 결정으로 보류
- Runtime 및 Editor C# 컴파일: 오류 없이 통과
- Unity 에셋 검증: Null 슬롯 수정 후 사용자 재실행에서 승인 Skill 58개가 이전에 PASS했지만 플레이어 표시 카탈로그 변경 후 현재 사용자 재실행 필요
- 프리팹 컴포넌트: 사용자가 부착했으며, 사용자가 누락을 확인한 뒤 계층 AutoBind 코드를 추가함
- 레거시 Effect, Bless, Relic 에셋: 변경하지 않았고 대상에서 제외

## 5단계: Skill 조합

구현 경로:

- `Assets/Scripts/Ability/Skills/Data/SkillClassificationPresentationData.cs`
- `Assets/Scripts/Ability/Skills/Data/SkillPresentationData.cs`
- `Assets/Scripts/Ability/Skills/SkillPresentationResolver.cs`
- `Assets/Scripts/Ability/Skills/SkillPresentationGroupResolver.cs`
- `Assets/Scripts/Ability/Effects/EffectPresentationGroupResolver.cs`

Skill Resolver는 Identity와 Classification을 콘텐츠 Metadata로 유지한다. Effect의 7개 Typed Outcome은 내부 정규화 계약으로 유지하고, 최종 Skill 표시는 Entry를 `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, `LinkedSkill`의 다섯 역할 Group으로 모은다. Effect마다 Group이 생기는 과분할을 없애되 원본 필드는 결합하지 않으며 원본 필드마다 Value 하나를 가진 별도 Entry를 유지한다. Preview 값과 Runtime 해석 값은 서로 다른 Provenance를 가지며 여러 Hit와 Effect는 표시 Grouping 전 Typed Skill 데이터에 보존된다.

`Control`과 `Displacement`는 `SpecialEffect`로 보낸다. Stun과 Root는 `config.Value`를 원본 기반 지속시간으로 사용하는 `Control`, Taunt는 제공된 Effect Entry 지속시간을 사용하는 `Control`이다. Knockback과 Pull은 구체 Config에 따라 방향과 Force 또는 Distance를 보존하는 `Displacement`다. `SkillInvoke`는 `LinkedSkill`로 보낸다.

다음 중첩 동작은 보류한다.

- `SkillHitSO.SpawnSkill`을 순회하지 않는다.
- 생성되거나 호출되는 Skill의 전체 상세를 부모에 합치지 않는다.
- `SkillInvoke`는 참조 Identity와 상세 콘텐츠 ID를 유지할 수 있지만 참조 Skill을 해석하지 않는다.

의미가 불분명한 `SkillEffectSO` 데이터는 `SkillPresentationResolver.ResolveLegacyEffect`를 통해서만 노출한다. 작성된 설명과 태그가 있으면 반환하며, Value, Duration, Chance, Stack 필드는 정규화하지 않는다.

## 6단계: 승인된 Skill 에셋 검증

사용자 실행 도구:

- 전체 행렬: `Tools > ProjectBS > Presentation > Run Skill Asset Validation`
- 선택 에셋 로그: `Assets > ProjectBS > Presentation > Log Selected Skill`
- 대화형 Inspector: `Tools > ProjectBS > Presentation > Open Skill Data Preview`

전체 행렬은 다음 경로만 검사한다.

- `Assets/Resources/skill/character/generated/`
- `Assets/Resources/skill/json/`

Hit 없음, Effect 없음, Effect 1개, 여러 Effect, 미지원 Effect, Preview/Runtime Provenance, Ratio/Percent 단위 구분, 보류된 중첩 참조를 포함한다. 1단계에서 확인한 JSON/SO 불일치 근거는 복구하거나 마이그레이션하지 않고 유지한다.

도구는 컴파일에 성공했다. 첫 Unity 실행은 사용자가 수행했으며 에이전트는 Unity를 조작하지 않았다.

첫 사용자 실행에서는 미지원 Effect 레코드 10개가 보고됐다. 조사 결과 모두 구체적인 미지원 Effect가 아니라 9개 Cast 에셋에 `{fileID: 0}`으로 직렬화된 Null `EffectEntrySO` 슬롯이었다. 이는 1단계 Inventory 및 Null Entry를 건너뛰는 게임플레이 `EffectResolver.ResolveEntries` 동작과 일치한다. 이제 `SkillPresentationResolver`도 Null 슬롯을 건너뛰며, 검증 보고서는 이를 `Ignored null EffectEntry slots`로 별도 집계한다.

Null 슬롯 수정 후 사용자 재실행 결과는 이전에 PASS였다.

- `Approved unique Skill paths: 58`
- `Resolved Skills: 58`
- `No hit / no Effect / one Effect / multiple Effects: 7 / 42 / 14 / 2`
- `Supported / description-only / unsupported Effects: 18 / 0 / 0`
- `Ignored null EffectEntry slots: 10`
- `Ratio / Percent values: 120 / 102`
- `Failures: 0`

이 PASS는 현재 플레이어 표시 카탈로그 전 결과다. 현재 Effect 자체 테스트와 Skill 에셋 검증은 사용자 재실행을 대기한다.

## 7단계: Character, Bless, Relic Adapter

구현 경로:

- Character: `Assets/Scripts/Actor/Character/Data/CharacterPresentationData.cs`, `Assets/Scripts/Actor/Character/CharacterPresentationResolver.cs`
- Bless: `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Data/BlessPresentationData.cs`, `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessPresentationResolver.cs`
- Relic: `Assets/Scripts/Collection/Relic/Data/RelicPresentationData.cs`, `Assets/Scripts/Collection/Relic/RelicPresentationResolver.cs`

모든 Adapter는 정의 Preview와 Runtime 상태 Overload를 지원한다. Bless와 Relic은 `EffectPresentationResolver`와 `EffectPresentationGroupResolver`를 재사용한다.

검증 상태:

- Character/Bless/Relic 소스 수준 Adapter 컴파일: 완료
- 승인된 현행 Character 에셋 검증: 대기
- 승인된 현행 Bless 에셋 검증: 대기
- 승인된 현행 Relic 에셋 검증: 대기
- 레거시 Bless/Relic 경로는 계속 제외하며 현행 데이터의 기준으로 읽지 않음

## 8단계: 의미 텍스트와 공통 View 인계

구현 경로:

- `Assets/Scripts/Presentation/PresentationValueData.cs`
- `Assets/Scripts/Presentation/PresentationTextFormatter.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoGroupView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoEntryView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoTagView.cs`
- `Assets/Scripts/Ability/Skills/UI/SkillContentInfoPresenter.cs`
- `Assets/Scripts/Actor/Character/ui/CharacterContentInfoPresenter.cs`
- `Assets/Scripts/Stage/NodeContents/Shrine/UI/BlessContentInfoPresenter.cs`
- `Assets/Scripts/Collection/Relic/UI/RelicContentInfoPresenter.cs`

Formatter는 명시적 단위를 Compact 출력으로 변환하며 후속 Localization을 위한 외부 Label/Token Resolver를 지원한다. 공통 View는 `ContentPresentationData`만 사용하며 Skill, Effect, Bless, Relic, Character SO 필드를 해석하지 않는다.

### 사용자 담당 Unity 연결

사용자가 네 View 컴포넌트를 부착했다. 이후 에이전트가 기존 계층 AutoBind 규칙을 따르도록 스크립트를 수정했다.

1. `UIContentInfoTagView`: `AutoBindPrefix("Tag")`; `text`를 `Tag_Text`에 연결한다.
2. `UIContentInfoEntryView`: `AutoBindPrefix("Entry")`; 계층 필드를 대응하는 `Entry_*` 오브젝트에 연결한다.
3. `UIContentInfoGroupView`: `AutoBindPrefix("Group")`; 계층 필드를 대응하는 `Group_*` 오브젝트에 연결한다.
4. `UIContentInfoView`: `AutoBindPrefix("Info")`; 계층 필드를 대응하는 `Info_*` 오브젝트에 연결한다.

`tagPrefab`, `groupPrefab`, `entryPrefab`은 자식 컴포넌트가 아니라 프리팹 에셋 참조다. 현재 AutoBind 도구는 에셋 참조를 해석하지 않으므로 사용자가 이 세 필드를 수동으로 지정해야 한다. 스크립트 갱신 후 네 프리팹을 열거나 검증하고 저장한 다음, 누락된 계층 참조나 Console 컴파일 오류가 없는지 확인한다.

데이터 계층 완료를 위해 Scene 연결은 필요하지 않다. 공통 계층 AutoBind와 Skill 전용 Presenter 진입점을 구현했으며, 더 넓은 Scene 통합은 별도 결정으로 유지한다. Group/Entry Label 데이터는 `Assets/Resources/string/presentation_string.csv`에서 관리한다.

### 사용자 실행 Skill UI 표시

임시 Editor Preview 도구는 폐기했다. `SkillContentInfoPresenter`가 지정된 `EquipmentSkillSO`를 `SkillPresentationResolver`, `SkillPresentationGroupResolver`, 지정된 `UIContentInfoView.Bind()` 순서로 전달하며 UI나 Canvas를 생성하지 않는다.

이는 현재 소유 경계이며 최종 병합 결정은 아니다. 구체적인 Skill SO와 `Build Presentation` 동작은 `SkillContentInfoPresenter`에 유지한다. 이를 중립 `UIContentInfoView`로 옮기면 승인된 의존 방향이 역전되므로 명시적인 사용자 승인이 필요하다.

- 사용자가 만든 콘텐츠 정보 UI 루트에 `SkillContentInfoPresenter`를 추가한다.
- `UIContentInfoView`와 `EquipmentSkillSO`를 지정한다. 이름이 `UIContentInfoView`인 자식은 AutoBind 대상이다.
- Play Mode에서 컴포넌트 우클릭 메뉴 `Build Presentation`을 실행한다.
- 작성 값은 `Use Runtime Values`를 끄고, 런타임 계산값은 이를 켠 뒤 레벨을 지정한다.
- ScrollRect 입력에는 Scene의 활성 `EventSystem`이 필요하며, 없으면 Presenter가 경고한다.

Presenter는 UI를 생성하거나 저장하지 않는다. Unity 실행과 시각 평가는 사용자가 담당한다.

### Owned Effect 인벤토리와 분리된 도감 소유권

`BlessContentInfoPresenter`는 기존 Shrine UI Domain에, `RelicContentInfoPresenter`는 기존 Relic UI Domain에 속한다. 각 Presenter는 Domain Resolver의 `ResolveForPlayerDisplay()` 경로를 통해 정의 Preview 데이터를 지정된 기존 `UIContentInfoView`로 전달하며, Runtime Entry Overload도 같은 플레이어 표시 경로와 `PresentationContext.Runtime`을 사용한다.

획득한 모든 일반 Bless와 보유한 모든 Relic은 즉시 적용된다. 권위 있는 기획에서 둘 다 장착 상태가 없으므로 정보 UI가 장착/해제 상태를 기준으로 필터링하거나 표시하면 안 된다. `BlessContentInfoPresenter`는 독립적인 일반 Bless 목록과 선택을 소유할 수 있고, `RelicContentInfoPresenter`는 보유 Relic 하나를 표시할 수 있다. 공통 View는 Gameplay Inventory나 선택을 소유하지 않는다.

현행 Runtime 소스는 아직 이 기획과 일치하지 않는다. `RelicItemService`에는 `EquippedRelics`가 남아 있고 `BlessManager.AddBless`는 새 Bless를 추가하기 전에 기존 영구 Common Bless를 제거한다. 이를 플레이어 표시 규칙이 아니라 구현 공백으로 취급한다. 최종 Collection UI를 만들기 전에 Runtime 소유권을 정리한다.

현재 Relic 페이지 화면 역할은 탭이 없는 보유 효과 인벤토리 하나로 확정됐다. 세로 Scroll 하나에 현재 적용 중인 모든 보유 Relic, 획득한 일반 Bless, 활성 Faith Bless를 카테고리 섹션으로 배치하며 이 페이지는 보유/활성 전용이고 `Catalog`를 사용하지 않는다. 유물 도감과 일반 축복 도감은 별도 페이지로 구성하고 공통 카테고리/아이템 시스템을 `Catalog` 모드로 재사용할 수 있다. 전체 Faith 진행과 향후 해금은 신앙 도감에 유지한다. Exclusive Job Change는 향후 명시적인 Effect 원본이 작성되지 않는 한 제외한다. 어떤 항목을 선택하더라도 중립 `UIContentInfoView` 하나에 상세를 연결한다. 또 다른 보유 전용 Bless 페이지는 만들지 않는다.

활성 구현은 `Assets/Scripts/Presentation/SharedUI/Content/` 아래의 중립 `ContentInventoryData`, `ContentInventoryItemView`, `ContentInventoryCategoryView`와 탭 없는 `OwnedEffectInventoryView`, `OwnedEffectInventoryPresenter`를 사용한다. Presenter는 설정된 Preview 정의 또는 명시적 Runtime 목록을 받고 정렬된 `OwnedOnly` 섹션을 만들며 Manager 자동 원본 수집은 이후 작업으로 남긴다. `OwnedEffectInventoryData`와 `OwnedEffectGridItemView`는 연결되지 않은 레거시 단위이며 이전 네 탭 동작을 복원하는 데 사용하면 안 된다.

사용자는 이번 작업 단위에서 `Panel_OwnedEffects.prefab`, `UIContentInventoryCategory.prefab`, `UIInventoryItemView.prefab`의 직접 컴포넌트 연결을 명시적으로 승인했다. 필수 참조를 모두 직렬화하고 패널 내부 `RelicContentInfoPresenter`만 제거했으며 공용 상세는 활성 상태로 유지했다. 이는 1회 승인일 뿐 이후 프리팹 YAML 수정이나 Unity 조작에 대한 일반 권한이 아니다. Unity Import, Inspector 확인, 원본 할당, 상호작용, 스크롤, 로컬라이징, 시각 검증은 사용자 작업으로 남는다.

획득/미획득 Relic을 모두 보여줄 향후 별도 Relic 도감을 위해 `RelicCollectionView`를 보존한다. 잠금 실루엣과 보유/전체 개수는 그 도감 역할에 속하므로 Owned Effect 인벤토리로 일반화하지 않는다. 사용자 준비 `Assets/Prefabs/UI/Fixed/Panel/Panel_RelicInfo.prefab`은 수정하지 않았고 사용자 Unity 교체 및 연결이 필요하다.

### 분리된 Faith 페이지 소유권

Faith는 별도의 잠금 해제 및 레벨 진행 시스템이다. 각 신은 Faith 레벨에 따라 강해지는 Basic Bless, `BlessSO`가 아닌 직업군 Exclusive Job Change, Faith Lock 시 획득하는 Exclusive Bless 1, Faith Lock 이후 레벨 8에 획득하는 Exclusive Bless 2라는 서로 다른 기능 네 개를 가진다. 세 Bless 기능은 `BlessPresentationResolver`를 재사용할 수 있지만 Exclusive Job Change에는 Character Job 데이터와 Faith 소유 Adapter가 필요하다.

`BlessContentInfoPresenter`나 `UIContentInfoView`가 아니라 `FaithPagePresenter`가 획득 Faith 탭, 선택된 신, 레벨 1-10 Roadmap, 기능 선택, 페이지 구성을 소유한다. 잠긴 기능도 Preview로 읽을 수 있게 유지한다. 일반 Bless는 선택된 신 소유 범위 밖에 둔다.

권위 있는 상세 페이지 및 프리팹 계약은 `AgentDocs/Machal/faith-page-design.md`다. 이 문서는 이전 세 Bless 및 Bless 탭 전용 준비 모델을 대체한다. 현행 소스에는 Exclusive Job Change와 네 기능 Slot이 명시적으로 인코딩되지 않아 Runtime 및 프리팹 연결은 계속 보류한다. 누락된 역할, 직업 매핑, 해금, 강화 값을 이름이나 목록 순서에서 추론하지 않는다.

### 의미 기반 표시 필터와 스크롤 수정

`SkillPresentationGroupResolver.Resolve()`는 Skill Presentation Editor 도구와 검증에서 사용하는 전체 검사 경로다. `0`, `999`, 기본 수/배율, 비활성/적용 플래그 같은 원본 확인 값을 유지한다. `ResolveForPlayerDisplay()`만 플레이어 UI 경로이며 `0s`, `0m`, 피해량 0, 기본 투사체 수 1, 기본 배율 1, 무제한 센티널 `999`, 기본 비활성 치명타/방어 표시와 빈 Hit 그룹을 생략한다. Skill Presenter만 필터 경로를 사용하므로 검사 데이터나 Effect 결과는 삭제되지 않는다.

`UIContentInfoView.Bind()`는 기존 동적 자식을 비활성화한 뒤 제거하고, 새 그룹 생성 후 레이아웃과 ScrollRect 범위를 강제로 갱신하고 맨 위로 이동한다. 실제 스크롤 입력에는 활성 `EventSystem`이 필요하다. 정적 프리팹 검사에서는 Viewport에 Raycast 가능한 `Graphic`이 없으므로 활성 자식 Graphic이 없는 영역에서 Wheel과 Drag를 확인해야 한다. 사용자가 Unity에서 입력 공백을 확인한 경우에만 투명 Raycast Target `Image`를 추가한다.

## Runtime 정확성 경계

- `EquipmentSkillRuntimeData`는 해석된 Range, Burst, Projectile Count, Spread, Arrangement Value, Scale을 저장한다. 그 외 필드는 Runtime Presentation에서도 작성 에셋 값을 유지하며 Authored Provenance를 가진다.
- 현재 `EquipmentStatResolver`는 첫 번째 `SkillHitSO`를 해석된 Damage와 Max Hit Modifier의 기준으로 읽는다. Runtime Presentation은 Hit별 Upgrade Resolution을 새로 만들지 않고, 모든 해석된 Hit에 이 동작을 그대로 반영한다.
- `EquipmentSkillResolver`는 `FirstHitBaseDamage`를 Runtime Damage DTO에 복사하지 않는다. 이 값은 Preview 전용이며 Runtime Presentation에서 제외한다.
- 현재 Runtime은 `UseSplitMultiHitDamage`를 전달하지 않고 `SplitHitCount`를 복사한다. Runtime Presentation은 Runtime이 받은 값을 노출하며 이 게임플레이 불일치는 변경하지 않는다.
- 중첩 Skill 순회는 보류했다. 순환 순회 코드를 추가하지 않았다.
- `StringManager`를 사용할 수 없으면 Identity는 Unity 에셋 이름을 Fallback으로 사용한다. 설명은 순서가 있는 필수 `StringManager` 조회를 사용한다. 전략 `EquipmentSkillSO`는 정확한 `skill.strategic.*.desc`를 먼저 조회한 뒤 확인된 소유 키 `item.strategic.*.desc`를 조회하며, 모든 후보가 실패하면 첫 번째 의도 전체 Key를 표시한다. 원본 Group, Entry, Tag, Token Key는 검사/Provenance 데이터로 유지하고 플레이어 텍스트는 명시적인 `PresentationDisplayCatalog` 매핑을 통해서만 `Assets/Resources/string/presentation_string.csv`의 표준 `presentation.*` 행을 조회한다.

## 검증

- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`: 오류 0개
- 5-8단계 빌드: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`가 해당 실행에서 경고 0개, 오류 0개로 완료
- AutoBind 수정 빌드: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`가 오류 0개, 기존 프로젝트 경고 191개로 완료
- 6단계 Null 슬롯 수정 빌드: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`가 오류 0개, 기존 프로젝트 경고 191개로 완료
- 최신 계약 정정 전 6단계 Unity 검증: Skill 58개, 지원 Effect 18개, 미지원 Effect 0개, 무시한 Null 슬롯 10개, 실패 0건으로 PASS
- 최신 의미 재그룹화 Editor Assembly 빌드: 오류 0개, 기존 경고 156개
- 최신 정적 콘텐츠 검사: Localization 데이터 140행, 대소문자 무시 복합 Key 중복 0개, 전략 Skill JSON 20개 모두 Strict UTF-8 JSON 파싱 성공
- Presenter/필터/스크롤 수정 빌드: 신규 소스를 명시적으로 포함한 `dotnet build Assembly-CSharp.csproj --no-restore`가 오류 0개, 기존 프로젝트 경고 35개로 완료
- 설명 Localization 수정 빌드: `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:minimal`이 오류 0개, 기존 프로젝트 경고 35개로 완료
- Bless/Relic Presenter 빌드: `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`이 오류 0개, 기존 프로젝트 경고 35개로 완료
- Bless 목록 Presenter 빌드: `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`이 오류 0개, 기존 프로젝트 경고 35개로 완료
- 전략 스킬 설명 키 검사: 승인 `skill.strategic.*` ID 20개 모두 `item.strategic.*.desc` 행과 대응
- 승인 에셋 정적 조사: Cast 에셋 9개에 Null `EffectEntrySO` 슬롯 10개, `unyielding_will`에는 2개 존재
- 정적 프리팹 검사: 네 View 스크립트 GUID가 모두 부착되어 있고 필요한 접두사 계층 대상 이름 14개가 모두 존재
- 현재 프리팹 YAML 정적 검사: 필수 참조 17개 중 13개가 연결되었고 `UIContentInfoEntry`의 Label, Value, Detail Button 및 `UIContentInfoTag`의 Text는 사용자 검증 또는 `OnValidate`와 프리팹 저장 전까지 Null 상태
- 5-8단계 구현 `.meta` 검사: GUID 22개가 각각 `Assets/` 아래에 정확히 한 번 존재
- Presenter `.meta` GUID `6c063f32913a4de298db20334f5c9b2a` 지정 완료, Unity Import와 시각 동작은 사용자 검증 대기
- 범위 내 `git diff --check`: 통과
- Placeholder 검사: 새 구현에 `[PLACEHOLDER]`, `TODO`, `FIXME` 없음
- 에이전트는 Unity Editor를 열거나 조작하지 않음

## 지원 및 대기 매트릭스

| 영역 | 상태 |
| --- | --- |
| 현행 Skill Preview 조합 | 구현 완료, 이전 58개 Skill PASS는 플레이어 표시 카탈로그 전 결과, 현재 사용자 재실행 대기 |
| 현행 Skill Runtime 조합 | 구현 완료, 이전 58개 Skill PASS는 플레이어 표시 카탈로그 전 결과, 현재 사용자 재실행 대기 |
| 현행 Effect Typed 정규화 | 구현 완료, 7개 Outcome 유지 |
| Skill 의미 역할 Grouping | 구현 완료, Effect별 UI Group을 다섯 통합 Group으로 대체 |
| 의미가 불분명한 레거시 `SkillEffectSO` | 설명 전용 Fallback |
| 중첩 Skill 순회 및 상세 | 보류 |
| Character Adapter | 구현 완료, 승인 에셋 검증 대기 |
| Bless Adapter | 구현 완료, 승인된 현행 에셋 검증 대기 |
| Relic Adapter | 구현 완료, 승인된 현행 에셋 검증 대기 |
| 공통 Compact Formatter | 구현 완료, 원본 Key 기반 Label 데이터 추가, 추가 행은 에셋 검증 후 보완 가능 |
| 공통 View 스크립트 | 계층 AutoBind와 강제 스크롤 레이아웃 갱신 구현 완료, 사용자 시각 및 Viewport 입력 검증 대기 |
| Skill 콘텐츠 Presenter | 지정된 기존 UI 대상 구현 완료, 최종 소유 결정과 사용자 Unity 검증 대기 |
| 일반 Bless 콘텐츠 Presenter | 목록 소유 정보 탭 구현 완료, 일반 Bless는 즉시 적용되고 장착 상태 없음, Runtime 소유권 정리와 사용자 Unity 검증 대기 |
| Relic 콘텐츠 Presenter | 정의 Preview와 Runtime Entry 구현 완료, 향후 Collection UI는 장착 필터 없이 모든 보유 Relic을 사용해야 함, Runtime 소유권 정리와 사용자 Unity 검증 대기 |
| 보유 효과 인벤토리 | 탭 없는 `OwnedOnly` View/Presenter와 아이템/카테고리/패널 프리팹 그래프 직접 연결 완료, 정적 솔루션 및 직렬화 참조 검사 통과, 사용자 Unity Import와 상호작용/스크롤/상세/로컬라이징/시각 검증 및 이후 Manager 원본 수집 대기 |
| Relic 도감 | 획득/미획득 항목, 실루엣, 보유/전체 개수용 기존 `RelicCollectionView` 보존, 향후 페이지 작업 대기 |
| Faith 페이지 및 네 기능 Adapter | 상세 설계 완료, Exclusive Job Change와 명시적인 네 Slot 원본 모델 미구현 |
| Faith 프리팹 준비 | `AgentDocs/Machal/faith-page-design.md`에 상세 계약 기록, 사용자 프리팹 작업과 Unity 검증 대기 |
| Scene 통합 | 보류 |

## 2026-08-12 플레이어 표시 카탈로그 확장

- 플레이어 Group, Entry, Tag, 문맥별 Enum 대체 단어, 값 포맷의 명시적 Allowlist와 로컬라이징 Key 매핑으로 `Assets/Scripts/Presentation/PresentationDisplayCatalog.cs`를 추가했다.
- `StringManager` 안전 조회용 `Assets/Scripts/Presentation/PresentationLocalizedTextResolver.cs`를 추가했고, `StringManager`가 없을 때 쓰던 기존 이름 대체 동작은 유지했다.
- `PresentationTextFormatter.CreatePlayerFormatter(...)`는 엄격하다. 플레이어 텍스트는 원본 Key나 임의 생성 Pascal Case 대체를 사용하지 않는다. 기본 Formatter는 전체 검사/디버그 경로로 유지한다.
- `SkillPresentationGroupResolver.ResolveForPlayerDisplay()`는 시스템 전용 필드를 제외하고 0/기본값/무제한 값을 조건부로 생략한다. `Resolve()`는 원본 검사 경로로 그대로 유지한다.
- Bless/Relic 플레이어 표시 메서드도 같은 카탈로그를 사용한다. 기존 이름/설명 동작은 변경하지 않고 승인되지 않은 원본 Category/Runtime 상태 텍스트는 검사 전용으로 둔다.
- 고정 DamageType, ControlType, Displacement 어휘와 명시적 포맷 Key를 포함한 표준 `presentation.*` 행 154개를 `Assets/Resources/string/presentation_string.csv`에 추가했다.
- 전체 필드 인벤토리와 정책: `AgentDocs/Machal/ability-content-presentation-display-catalog.md`.
- 검증: `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`이 오류 0개와 기존 경고 191개로 완료됐다. CSV 데이터는 294행이고 복합 Key 중복은 0개다. 에이전트는 Unity를 실행하지 않았다.

## 2026-08-12 누락 Localization Key 표시 정정

- `PresentationLocalizedTextResolver.ResolveRequired`는 후보를 순서대로 조용히 확인하고 모든 후보가 누락된 경우에만 첫 번째 의도 전체 Key를 노출한다.
- Skill, Effect, Bless, Relic 설명은 필수 조회를 사용한다. 후보 소유 경로와 순서는 변경하지 않는다.
- 플레이어 Catalog 필터는 계속 엄격하다. 표시 매핑이 없는 미승인 원본 필드는 생략하고, 승인된 매핑의 StringManager 행이 누락되면 의도 Key를 표시한다.
- StringManager를 사용할 수 없을 때의 에셋 이름 Fallback과 플레이어 텍스트의 임의 Pascal Case 생성 금지는 변경하지 않는다.
- 정적 Editor Assembly 빌드는 오류 0개, 기존 경고 191개로 완료했다. Unity 화면 검증은 사용자 담당으로 대기한다.
