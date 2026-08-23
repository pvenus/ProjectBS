# Task: Ability 콘텐츠 Presentation 데이터 시스템

## 상태

- 단계: 플레이어 표시 카탈로그와 누락 Localization Key 표시 처리 구현 완료, 이전 Skill 검증은 최신 표시 계약 이전 결과, 중첩 Skill 순회 보류, 현재 사용자 Unity 재실행 대기
- 구현: 공통 계약, 모든 현행 Effect 매핑, Skill 조합, 현행 정의 Character/Bless/Relic Adapter, 명시적 플레이어 표시 Allowlist, 로컬라이징 카탈로그, Compact Formatter, 공통 View 스크립트, Character/Skill/Bless/Relic Presenter, 사용자 실행 검증 도구 구현 완료
- 4단계 검증: 최신 정정 전 Synthetic 매핑 13종 및 승인 EffectEntry 에셋 20개 통과, 현재 15개 Case 자체 테스트 재실행 대기
- 표시 카탈로그 검증: Editor Assembly 빌드 오류 0개, 기존 경고 191개로 통과, 정적으로 필요한 Localization Key 141개 중 누락 0개
- 5-8단계 근거: `AgentDocs/Machal/ability-content-presentation-stage5-8-completion-ko.md`
- View 및 프리팹 작업: 사용자가 공통 View 컴포넌트를 부착함. 계층 참조는 프로젝트 AutoBind 규칙을 사용하며 Tag/Group/Entry 템플릿 프리팹 에셋 참조는 수동 지정 유지
- Git 커밋 및 푸시: 별도 요청 없이는 금지

## 목표

UI 코드가 `EquipmentSkillSO`, `EffectSO`, `BlessSO`, `RelicSO` 필드를 직접 해석하지 않도록 게임플레이 콘텐츠의 구조화된 Presentation 데이터 시스템을 구축한다.

첫 번째 결과물은 데이터 설정 계층이다. View, Presenter, 프리팹, 레이아웃 작업은 후속 단계로 분리한다.

## 사용자 결정 사항

1. 레거시 Effect, Bless, Relic 데이터를 수정하거나 마이그레이션하지 않는다.
2. 승인된 현행 SO 및 에셋 경로만 확인하여 사용한다.
3. 게임플레이 해석은 UI가 아니라 Ability 도메인이 소유한다.
4. 이 기능을 위해 새 `Assets/Scripts/Core/` 경로를 만들거나 사용하지 않는다. 여러 콘텐츠가 공유하는 중립 Presentation 계약은 기존 루트 `Assets/Scripts/Presentation/` 카테고리에 둔다.
5. 변수를 문자열로 바로 바꾸는 방식보다 의미 카테고리와 그룹을 중심으로 설계한다.
6. 데이터 설정을 구현하고 검증한 뒤 View 연결을 계획한다.
7. 다른 에이전트가 이어갈 수 있도록 계획, 설계, 작업 순서, 작업 방식, 작업 내용, 진행 로그를 `AgentDocs/Machal/`에 유지한다.
8. 콘텐츠 도메인에 `Presentation/` 또는 이 기능만을 위한 `Resolvers/` 폴더를 추가하지 않는다. 콘텐츠 Presentation 데이터는 소유 도메인의 `Data/` 하위에 두고, 동작은 소유 경로 바로 아래에서 명확한 Resolver 또는 Builder 클래스명으로 구분한다.
9. 사용자가 승인한 Effect 원본과 정규화 결과 표를 권위 있는 정규화 계약으로 사용한다.
10. `OnHit`, `OnHeal`, `OnAttack`은 서로 다른 발동 이벤트로 유지한다. 최신 승인된 7개 Outcome 계약에 따라 `Heal`과 `PeriodicDamage`도 서로 다른 Outcome 타입으로 유지한다.
11. 기존 StringManager 이름/설명 후보 경로와 조회 순서, StringManager를 사용할 수 없을 때의 에셋 이름 Fallback은 변경하지 않는다. 플레이어 Group, Entry, Tag, Enum 대체 단어, 값 포맷 텍스트는 표준 StringManager Key로 조회한다.
12. 플레이어 View에 원본 JSON/C# Key나 임의 생성 Pascal Case 텍스트를 노출하지 않는다. 이 Key는 검사와 Provenance 출력에만 유지한다.
13. 사용 가능한 모든 필드를 플레이어 표시, 조건부 표시, 검사 전용으로 분류한다. 표시 카탈로그가 명시적으로 승인하기 전에는 새 필드의 플레이어 라벨을 추론하지 않는다.
14. 매핑되었거나 필수인 StringManager 행이 누락되면 의도한 Localization Key를 표시한다. 이 진단 Key는 계속 생략하는 미승인 원본 게임플레이 Key와 다르다.

## 필수 소스 파일

- `Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentSkillSO.cs`
- `Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectSO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectEntrySO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/config/`
- `Assets/Scripts/Ability/Effects/Resolvers/EffectResolver.cs`
- `Assets/Scripts/Ability/Effects/Runtime/config/`
- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessSO.cs`
- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessRuntimeData.cs`
- `Assets/Scripts/Collection/Relic/Definitions/RelicSO.cs`
- `Assets/Scripts/Collection/Relic/Runtime/RelicRuntimeData.cs`
- `Assets/Scripts/Actor/Character/so/CharacterSO.cs`

## 승인된 현행 에셋 경로

첫 구현과 검증에는 다음 경로만 사용한다.

- `Assets/Resources/skill/character/generated/`
- `Assets/Resources/skill/json/`
- `Assets/Resources/relic/json/`

Skill 경로에는 현행 `EquipmentSkillSO`, `EffectSO`, `EffectEntrySO` 정의로 직렬화된 에셋이 존재한다. `Assets/Resources/relic/json/`은 승인된 현행 Relic 경로이며, 정규화 Relic JSON과 사용자가 Unity Builder를 실행한 뒤 생성되는 현행 `RelicSO`, `EffectSO`, `EffectEntrySO`를 같은 폴더에 둔다.

## 제외 에셋 경로

다음 경로는 신뢰 가능한 검증 데이터로 사용하거나 수정 또는 마이그레이션하지 않는다.

- `Assets/Resources/bless/`
- `Assets/Resources/shring/`
- `Assets/Resources/shop/relic/`
- 레거시 `effects` 데이터를 직렬화하는 기타 기존 Bless/Relic 에셋

제외된 레거시 Relic 경로는 그대로 유지한다. 현행 Relic 에셋은 `Assets/Resources/relic/json/` 아래에만 생성하며 Unity 생성과 에셋 검증은 사용자 실행을 기다린다. Bless 에셋 검증은 승인된 현행 Bless 경로가 생길 때까지 대기한다.

## 아키텍처

### 공통 Presentation 계약

계획 경로:

```text
Assets/Scripts/Presentation/
  ContentPresentationData.cs
  PresentationIdentityData.cs
  PresentationGroupData.cs
  PresentationEntryData.cs
  PresentationValueData.cs
  PresentationContext.cs
  PresentationProvenanceData.cs
```

이 공통 클래스는 `EffectSO`, `EquipmentSkillSO`, `BlessSO`, `RelicSO`를 참조하지 않는다. 데이터 계약이며 게임플레이 Resolver나 View 컴포넌트가 아니다.

### Effect Presentation

계획 경로:

```text
Assets/Scripts/Ability/Effects/
  Data/
    EffectPresentationData.cs
  EffectPresentationResolver.cs
```

`EffectPresentationResolver`는 Effect Presentation 동작을 담당하는 유일한 클래스다. 구체적인 현행 `EffectConfig`를 분기하고 선택적인 Activation과 의미 결과를 직접 구성하며 최종 UI 문장 없이 구조화된 데이터를 반환한다.

첫 구현에서는 Activation, Outcome, Constraint, 타입별 Outcome 레코드를 `EffectPresentationData.cs` 하나에 함께 둔다. 파일이 커져 소유권을 분리할 이유가 생길 때만 나눈다. `Data/` 하위 폴더는 수동적인 정규화 데이터만 구분한다. 매핑이 작은 동안 Config 전용 Resolver 인터페이스나 Config마다 하나씩 Resolver 클래스를 만들지 않는다. 반복되는 생성은 StatModifier, CooldownChange 같은 정규화 결과 기준의 작은 private 메서드로만 묶을 수 있다.

### Skill Presentation

계획 경로:

```text
Assets/Scripts/Ability/Skills/
  Data/
    SkillPresentationData.cs
    SkillClassificationPresentationData.cs
  SkillPresentationResolver.cs
  SkillPresentationGroupResolver.cs
```

Skill Presentation은 정규화된 Effect Presentation 결과를 재사용하며 Effect Config를 다시 해석하지 않는다.
Skill 데이터의 정확한 경로는 `Assets/Scripts/Ability/Skills/Data/`이며 Resolver와 Builder 클래스는 `Assets/Scripts/Ability/Skills/` 바로 아래에 둔다.

### 기타 콘텐츠 Presentation

현행 소스 정의 Adapter 구현 경로이며, 승인된 현행 에셋 검증은 대기한다.

```text
Assets/Scripts/Stage/NodeContents/Shrine/Blessings/
  Data/BlessPresentationData.cs
  BlessPresentationResolver.cs

Assets/Scripts/Collection/Relic/
  Data/RelicPresentationData.cs
  RelicPresentationResolver.cs

Assets/Scripts/Actor/Character/
  Data/CharacterPresentationData.cs
  CharacterPresentationResolver.cs
```

이 Adapter들은 `Ability/Effects`의 Effect 정규화 진입점을 재사용하고 각자의 Identity 또는 Runtime 상태를 `Assets/Scripts/Presentation/`의 공통 계약으로 변환한다. 제외된 레거시 에셋을 현행 기준으로 취급하지 않는다. 사용자가 승인된 현행 Character/Bless/Relic 에셋 경로보다 먼저 소스 수준 Adapter 구현을 명시적으로 승인했으며, 에셋 수준 검증은 대기한다.

정확한 후속 데이터 경로는 다음과 같다.

- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Data/`
- `Assets/Scripts/Collection/Relic/Data/`
- `Assets/Scripts/Actor/Character/Data/`

### UI

공통 UI View는 `Assets/Scripts/Presentation/`의 중립 계약만 사용한다. 게임플레이 해석 책임을 가지지 않는다.

`AgentDocs/Machal/ability-content-ui-prefab-preparation.md`를 기준으로 `Assets/Prefabs/UI/Fixed/Content/` 아래 프리팹 구조와 레이아웃을 완료했다. 사용자가 공통 View 컴포넌트를 부착했다. 계층 필드는 `AutoBindPrefix`와 `AutoBind`를 사용하며 템플릿 프리팹 에셋 참조는 수동으로 유지한다. Domain Presenter는 UI를 생성하지 않고 지정된 Character, Skill, Bless, Relic을 기존 View에 연결한다. 구체 SO 필드와 Build 동작을 중립 `UIContentInfoView`로 옮기지 않는다. 더 넓은 Scene 통합은 별도 작업으로 유지한다. Group/Entry Label 데이터는 `Assets/Resources/string/presentation_string.csv`에서 관리한다. Unity Import, 컴포넌트 부착, 필드 지정, 컴포넌트 메뉴 실행, 프리팹/Scene 검증과 시각 평가는 사용자가 담당한다.

## 활성 설계 참고 문서

- 3단계 Effect 모델 및 Resolver 준비: `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`
- 플레이어 표시 인벤토리와 로컬라이징 카탈로그: `AgentDocs/Machal/ability-content-presentation-display-catalog.md`
- 4단계 Effect 매핑 및 사용자 테스트: `AgentDocs/Machal/ability-content-presentation-stage4-verification.md`
- 사용자 UI 프리팹 준비: `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`

### 의존 방향

```text
Presentation 중립 계약
<- 콘텐츠 소유 Resolver 및 Builder
<- View, Tooltip, Editor Preview가 정규화 결과 사용
```

1. 여러 콘텐츠가 공유하는 중립 계약은 구체적인 SO 타입을 알지 못한다.
2. 도메인 Resolver 및 Builder 코드는 자신이 소유한 SO/Config 타입만 알고 공통 계약을 출력한다.
3. View와 Tool은 정규화된 계약을 사용한다.
4. 최종 문자열 포맷은 후속 계층이며 Config별 Resolver 문자열에 합치지 않는다.

## 권위 있는 Effect 정규화 계약

Effect 데이터는 선택적인 `Activation`과 하나의 의미 결과로 정규화한다. 결과는 원본 필드를 그대로 나열한 값이 아니다.

| 원본 Effect | 정규화 결과 |
| --- | --- |
| `StatModifierEffect` | `StatModifier` |
| `ChanceOnHitStatModifierEffect` | `Activation(OnHit + Chance) + StatModifier` |
| `OnHitTimedStatModifierEffect` | `Activation(OnHit + Chance) + StatModifier`에 duration 포함. 런타임에서 제어 타이머로 적용되는 `StunDuration`, `RootDuration`은 `Control(Stun/Root + duration)`으로 특수화 |
| `ChanceOnHealStatModifierEffect` | `Activation(OnHeal + Chance + Target) + StatModifier` |
| `HealEffect` | `Heal` |
| `ChanceOnHealCooldownReduceEffect` | `Activation(OnHeal + Chance + Target) + CooldownChange` |
| `CooldownReduceEffect` | `CooldownChange` |
| `KnockbackEffect` | `Displacement` |
| `OnHitKnockbackDistanceEffect` | `Activation(OnHit + Chance) + Displacement` |
| `AttackBleedEffect` | `Activation(OnAttack + Chance) + PeriodicDamage` |
| `OnHitPoisonDotEffect` | `Activation(OnHit + Chance) + PeriodicDamage` |
| `ChanceOnHitSkillEffect` | `Activation(OnHit + Chance + Critical 조건) + SkillInvoke` |
| `TauntEffect` | `Control` |
| 의미가 불분명한 구형 `SkillEffectSO` | 필드를 정규화하지 않고 작성된 설명만 사용 |

### Activation

`EffectActivationPresentationData`는 다음을 포함할 수 있다.

- Trigger: `OnHit`, `OnHeal`, `OnAttack`
- 작성된 확률 숫자와 원본 단위 또는 Runtime Provenance를 가진 명시적인 Runtime 해석 값
- Heal 대상 조건
- 치명타 요구 조건

실제 발동에 영향을 주는 조건만 여기에 둔다.

### Outcome

- `StatModifier`: Stat, 연산, 원본에 충실한 값과 단위, Modifier 자체가 시간제인 경우 지속시간
- `Heal`: 원본의 Flat, 최대 HP, 공격력 계수 값과 필요한 Clamp 동작
- `CooldownChange`: 런타임 감소 타입에 따른 비율 또는 고정 초
- `Displacement`: 방향과 크기, Force와 Distance를 구분하는 단위 종류
- `PeriodicDamage`: 원본 피해 계수, 빈도 단위, 원본에 존재하는 Interval과 Duration
- `SkillInvoke`: 참조하는 현행 Skill과 지원되는 발동 조건
- `Control`: 제어 종류와 구체 Config 또는 Effect Entry에서 온 원본 기반 지속시간

위 7개 Outcome은 내부 Effect 정규화 계약이며 최종 Skill UI Group 목록이 아니다. 단독 Effect Adapter는 Outcome 종류를 단일 Group Key로 사용할 수 있다. Skill 조합은 정규화 데이터를 표시 역할에 따라 `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, `LinkedSkill`로 모으며 Effect마다 UI Group을 만들지 않는다. 선택적인 Activation 필드는 `Activation` 아래의 별도 Entry로 유지하고, 각 정규화 Outcome은 소유 Skill Group으로 보낸다. `Scaling`, `Persistence`, `Constraints`, `CountAndScale`, `SizeAndLifetime` 같은 추가 필드 묶음 Group을 만들지 않는다.

### 구형 `SkillEffectSO` Fallback

`SkillEffectSO`는 의미 연산을 확정할 수 없는 범용 Trigger, Target, Value, Duration, Chance, Stack 필드를 노출한다. 이 필드를 어떤 정규화 결과로도 변환하지 않는다. 작성된 `Description`이 비어 있지 않으면 설명 전용 Fallback 레코드를 반환한다. 설명도 비어 있으면 문장을 만들지 않고 미지원 레코드를 반환한다.

이 Fallback은 5단계에서 Skill 도메인이 조합한다. `EffectPresentationResolver`의 분기가 아니다.

## 단일 Effect Resolver 계약

```text
EffectEntrySO
-> EffectPresentationResolver.Resolve(...)
-> EffectSO.Config 타입 분기
-> 선택적인 Activation 구성
-> 정규화 결과 하나 구성
-> EffectPresentationData
```

공개 동작 표면은 `EffectPresentationResolver` 하나다. Config 분기는 이 클래스 내부에 유지한다. Private 메서드는 Config 타입마다 대응시키지 않고 반복되는 정규화 결과 생성에만 추가한다.

예상 내부 그룹:

- `CreateActivation(...)`
- `CreateStatModifier(...)`
- `CreateHeal(...)`
- `CreateCooldownChange(...)`
- `CreateDisplacement(...)`
- `CreatePeriodicDamage(...)`
- `CreateSkillInvoke(...)`
- `CreateControl(...)`
- `CreateDescriptionFallback(...)`

Config 클래스는 게임플레이 정의로 유지하며 `ToPresentationData()` 메서드나 Presentation 계약 참조를 추가하지 않는다.

## 정규화 및 후속 표시 규칙

- 정규화 구조는 도메인 데이터다. `ViewData`, `UIData`, `ValueOverrideView`, `StackView` 같은 이름을 사용하지 않는다.
- 런타임에서 의미가 없거나 구조가 불명확한 값은 활성 정규화 계약에서 제외한다.
- 승인된 Activation과 Outcome 구조는 유지한다. 공통 UI 변환에서는 원본 필드 하나를 `PresentationEntryData` 하나로 만들며, 서로 다른 원본 값을 한 Label/Value 행에 결합하지 않는다.
- 원본 숫자, 원본 단위, Provenance를 보존한다. Formatter는 정규화 데이터를 바꾸지 않고 Ratio를 백분율로 표시할 수 있다.
- 작성 숫자를 Clamp하거나 최솟값으로 대체하거나 Ratio 데이터를 Percent 데이터로 변환하거나 횟수를 합성하지 않는다.
- 끌어당김, 밀침, 넉백 같은 동작 단어는 작성된 설명에 유지한다. 정규화 수치 Label은 효과 거리, 효과 범위 같은 일반 개념을 사용한다.
- 후속 UI는 `감소 비율: 20%`, `감소 시간: 1초`, `효과 거리: 5m`처럼 원본 필드별 Label/Value를 사용한다. 전체 설명 문장은 작성된 콘텐츠를 사용한다.
- 검사 및 검증 출력은 원본에서 확인 가능한 값과 기본값을 유지해야 한다. `SkillPresentationGroupResolver.Resolve()`는 전체 검사 경로이며, `ResolveForPlayerDisplay()`만 플레이어에게 불필요한 기본값이나 무제한 센티널을 생략할 수 있다.
- 플레이어 표시용 `desc` 필드는 순서가 있는 필수 `StringManager` 조회를 사용한다. 후보 Main Key는 실패한 후보를 노출하지 않고 먼저 확인하며, 모두 실패하면 첫 번째 의도 Key를 일반 조회하여 전체 `*.desc` Key를 디버깅용으로 표시한다. 구조화된 Effect 수치로 설명 문장을 만들지 않는다.
- 전략 Skill은 정확한 `skill.strategic.*.desc`를 먼저 조회한 뒤 확인된 Localization 소유 경로 `item.strategic.*.desc`를 조회한다.
- Group, Entry, Tag, Enum 대체 단어, 값 포맷 텍스트는 `name` Sub-key를 가진 표준 `presentation.*` Main Key를 사용한다. 플레이어 Formatter는 원본 JSON/C# Key나 임의 생성 Pascal Case 단어로 대체하지 않는다.
- 전체 원본 필드 정책과 로컬라이징 매핑은 `AgentDocs/Machal/ability-content-presentation-display-catalog.md`를 권위 문서로 사용한다.
- Interval과 Duration으로 적용 횟수를 계산하지 않는다. 런타임이 사용하는 실제 `ApplyCount` 같은 원본 필드가 있을 때만 횟수를 포함한다.
- 런타임 Resolver가 적용하지 않는 동안 `EffectEntrySO.ValueOverride`와 Effect 업그레이드 Modifier를 활성 값으로 노출하지 않는다.

## 중첩 Skill 계약

- 부모 Skill에는 중첩 Skill 이름과 간단한 요약만 포함한다.
- 중첩 Skill의 전체 Targeting, Damage, Movement, Persistence, Effect 상세를 부모에 펼치지 않는다.
- 중첩 Skill 상세 Presentation은 독립적으로 Resolve하고 후속 UI 단계에서 별도 콘텐츠 페이지로 연다.
- 중첩 Skill 조합은 순환 참조와 중복 순회를 방지해야 한다.

## 이미 확인된 수치 규칙

- 필드명만 보고 단위를 추론하지 않는다.
- `StatModifierEffectConfig.Percent` 런타임 계산은 값을 `0..1` 비율로 취급한다.
- `ChanceOnHitStatModifierEffectConfig.Percent` 런타임 계산은 값을 `0..100` 퍼센트로 취급한다.
- Heal 최대 HP 및 공격력 계수는 값을 직접 곱하므로 비율로 동작한다.
- 적중 시 스킬 발동 확률은 `Random.Range(0, 100)`과 비교되므로 `0..100`으로 동작한다.
- 현재 `EffectResolver`는 `EffectEntrySO.ValueOverride`와 전달받은 Effect 업그레이드 Modifier를 적용하지 않는다. Presentation이 이를 독립적으로 적용하면 안 된다.
- 각 Effect는 별도의 구조화된 레코드로 유지한다.

## 단계별 구현 계획

### 1단계 — 소스 및 에셋 인벤토리

- 상태: 2026-08-08 완료
- 소유 경로: `AgentDocs/Machal/`만 사용
- 작업:
  - 작업 트리 기준선과 승인 경로를 고정한다.
  - 현행 Skill 및 Effect 정의부터 런타임 해석까지 추적한다.
  - 현행 SO 에셋, 작성 JSON, 도달 가능한 EffectEntry, 미지원 에셋 사례를 집계한다.
  - 작성 데이터 선언과 현행 런타임 SO 참조를 통해 도달 가능한 값을 분리한다.
- 완료 근거: `AgentDocs/Machal/ability-content-presentation-inventory.md`

### 2단계 — 공통 중립 계약

- 상태: 2026-08-09 완료
- 소유 경로: `Assets/Scripts/Presentation/`
- 콘텐츠에 종속되지 않는 Identity, Context, Provenance, Value, Entry, Group, Content 최소 계약을 추가한다.
- 구체적인 Skill, Effect, Bless, Relic, Character SO를 참조하지 않는다.
- 먼저 호출 가능한 Placeholder를 검증한 뒤 컴파일과 계약 수준 테스트를 수행한다.
- 완료 근거:
  - `AgentDocs/code-writing-rules.md`가 복구되어 코드 작업 전에 전부 읽었다.
  - 임시 `[PLACEHOLDER]` Factory를 Smoke Harness에서 실제 호출한 뒤 최종 생성 동작으로 교체하면서 제거했다.
  - 최종 Smoke 검증에서 Identity, 조합된 값 2개, Entry 1개, Group 1개, Runtime Provenance, Preview 및 Runtime Context를 생성했다.
  - `.cs` 파일 7개 모두 대응하는 Unity `.meta` 파일을 가진다.

### 3단계 — Effect 정규화 모델과 진입점

- 상태: 2026-08-11 완료
- 데이터 경로: `Assets/Scripts/Ability/Effects/Data/`
- 동작 경로: `Assets/Scripts/Ability/Effects/`
- 최종 표시 문자열 없이 Activation과 의미 결과 데이터를 정의한다.
- 내부에서 Config를 분기하며 결정적 Fallback을 제공하는 단일 `EffectPresentationResolver` 진입점을 추가한다.
- 발동 이벤트를 구분하고 Heal과 Periodic Damage를 서로 다른 Typed Outcome으로 표현한다.
- 완료 근거:
  - 임시 호출 가능 Placeholder를 검증한 뒤 제거했다.
  - 격리 Smoke 결과는 `STAGE3_EFFECT_PRESENTATION_SMOKE_OK`였다.
  - Null, 미지원, 설명 전용, Provenance, Entry 제약, 단위, Trigger 구분, Heal 및 PeriodicDamage 동작을 검증했다.
  - Unity가 두 신규 스크립트를 포함해 `Assembly-CSharp.dll`을 다시 생성했다.
- 설계 및 검증 계약: `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`

### 4단계 — Effect Config 매핑 분기

- 상태: 2026-08-11 완료
- A 묶음, 현행 연결 에셋 검증 가능: `StatModifier`, `Heal`, `CooldownChange`, `Displacement`, `Control`.
- B 묶음, 소스 코드는 확인했지만 승인된 연결 에셋 없음: Trigger가 있는 StatModifier 계열, 회복 Trigger 쿨다운 변경, 공격 출혈, 독 지속 피해, 거리 이동, Skill 호출.
- Config 클래스 13종 모두 단일 `EffectPresentationResolver` 내부에서 분기하며 Config별 Resolver 계층은 추가하지 않았다.
- 완료 근거:
  - `dotnet build Assembly-CSharp.csproj --no-restore`가 Error 0건으로 완료됐다.
  - 원본 충실도 수정 전 Unity 자체 테스트에서 Synthetic Config 매핑 13종이 모두 통과했다.
  - 원본 충실도 수정 전 승인 경로의 `EffectEntrySO` 에셋 20개가 모두 Supported로 해석됐다.
  - 의미 재그룹화 및 Stun/Root 특수화 후 `Assembly-CSharp-Editor.csproj`는 오류 0개, 기존 경고 156개로 컴파일됐다. 자체 테스트는 Config 클래스 13종에 대한 Case 15개를 포함하며 현재 Unity 재실행은 대기한다.
  - 사용자 테스트 메뉴: `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`.
- 승인된 연결 에셋이 없는 Config 8종은 에셋 수준 검증을 계속 대기하며 소스 수준 Synthetic 검증만 완료됐다.

### 5단계 — Skill 조합

- 상태: 코드 완료, 중첩 Skill 순회와 상세 확장은 사용자 결정으로 보류

- 데이터 경로: `Assets/Scripts/Ability/Skills/Data/`
- 동작 경로: `Assets/Scripts/Ability/Skills/`
- Identity와 Classification은 콘텐츠 Metadata로 유지한다. JSON 필드 Provenance는 보존하되, 표시할 Skill Entry는 `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, `LinkedSkill`의 다섯 의미 Group으로 모은다.
- 시전/대상/Trigger/Chance 조건은 `Activation`, 투사체/이동/Burst/Hit 주기/전달 형상은 `Delivery`, Damage/Heal/Stat/Cooldown/Periodic/Spawn 결과는 `Outcome`, `Control`과 `Displacement`는 `SpecialEffect`, `SkillInvoke`는 `LinkedSkill`로 보낸다.
- 원본 필드 하나는 Entry와 Value 하나로 유지한다. 의미 Grouping은 필드 결합, 값 유도, 원본 숫자 대체를 허용하지 않는다.
- 직접 SO Preview 값과 `EquipmentSkillRuntimeData` 값을 Provenance로 구분한다.
- 여러 Hit와 Effect를 보존한다. 중첩 Skill은 순회하지 않으며, 가능한 경우 참조 Identity와 상세 콘텐츠 ID만 유지한다.
- 의미가 불분명한 레거시 `SkillEffectSO`는 여기에서만 작성된 설명 Fallback으로 처리하며 Effects에서 Skills로 향하는 의존성을 만들지 않는다.

### 6단계 — 승인 에셋 검증

- 상태: Null 슬롯 수정 후 승인된 현행 Skill 에셋 58개에 대한 사용자 Unity 실행은 이전에 통과했으며 최신 플레이어 표시 카탈로그 적용 후 재실행이 필요함

- 도달 가능한 모든 승인 런타임 SO를 기준으로 결과를 검증한다.
- Hit 없음, Effect 없음, Effect 1개, 여러 Effect, 미지원 Config, 서로 다른 단위 범위를 포함한다.
- JSON과 생성 SO의 불일치는 기록하되 이 Task에서 에셋을 복구하거나 마이그레이션하지 않는다.

### 7단계 — Character, Bless, Relic Adapter

- 상태: 현행 정의 Adapter 완료, 세 도메인의 승인된 현행 에셋 검증은 대기

- 사용자 요청에 따라 소스 수준 Adapter를 구현했다. 에셋 검증에는 해당 도메인의 승인된 현행 경로가 필요하다.
- 정규화된 Effect 결과를 재사용하고 도메인 소유 Identity 또는 Runtime 상태만 추가한다.
- 현재는 소스 정의만 확인되었으므로 Character, Bless, Relic의 에셋 수준 결과는 대기 상태로 둔다.

### 8단계 — 데이터 계층 승인 및 UI 인계

- 상태: 명시적 플레이어 표시 Allowlist, StringManager 카탈로그, Compact Formatter, 공통 View 스크립트, 계층 AutoBind, 의미 기반 기본값 필터와 Character/Skill/Bless/Relic Presenter 완료, 사용자 Localization 시각 검증 대기

- 지원, Fallback, 대기, 미검증 매트릭스는 `AgentDocs/Machal/ability-content-presentation-stage5-8-completion-ko.md`에 기록했다.
- Compact Formatter와 공통 View 스크립트를 구현했다.
- `PresentationDisplayCatalog`가 플레이어 표시 Group, Entry, Tag, Enum 대체 단어, 포맷 Key의 명시적 매핑을 소유한다. 기본 Formatter는 검사용 원본 Key를 유지한다. 플레이어 Formatter는 Catalog 매핑이 없는 미승인 원본 Key를 생략하지만 StringManager에서 누락된 매핑 Key는 임의 대체 문구 대신 의도한 전체 Key로 표시한다.
- 사용자가 네 View 컴포넌트를 부착했다. 계층 필드는 `AutoBindPrefix`와 `AutoBind`를 따르며, Unity 갱신 후 `OnValidate` 실행과 프리팹 저장을 거쳐야 직렬화 참조가 확정된다.
- `tagPrefab`, `groupPrefab`, `entryPrefab`은 수동 에셋 지정으로 유지한다. 이전 승인 Skill 검증은 플레이어 표시 카탈로그 적용 전 결과이며 현재 재실행은 대기한다.
- Character, Skill, Bless, Relic Presenter는 Play Mode에서 지정된 현행 정의를 지정된 기존 `UIContentInfoView`에 연결한다.
- Bless와 Relic Presenter는 Domain 해석을 공통 View에 넣지 않으면서 실제 Runtime 상태를 표시할 수 있도록 Runtime Entry Overload도 제공한다.
- `SkillContentInfoPresenter`는 Skill 소유 통합 경계로 유지한다. 사용자가 다른 소유 설계를 명시적으로 선택하지 않은 상태에서 SO 필드나 Build 동작을 `UIContentInfoView`로 합치면 중립 Presentation 의존 방향을 위반한다.
- 기존 Editor 검사 및 검증 도구는 전체 `Resolve()` 결과를 계속 사용하며, Skill Presenter만 `ResolveForPlayerDisplay()`를 사용한다.
- Overlay Canvas를 만들던 임시 Editor 메뉴는 제거했다. Scene 통합은 별도 후속 작업으로 유지하며 Group/Entry Label 데이터는 `Assets/Resources/string/presentation_string.csv`에서 관리한다.

## 연기 범위

- `SkillContentInfoPresenter`와 중립 `UIContentInfoView` 사이의 최종 소유 결정
- ScrollRect Viewport의 사용자 Unity Wheel/Drag 검증 및 필요한 경우 투명 Raycast Target `Image` 추가
- 구현된 Skill Presenter와 현재 공통 계층/템플릿 경계를 넘어서는 콘텐츠별 프리팹 동작
- Scene 연결
- 누락된 작성 설명 행과 에셋 검증에서 추가로 발견되는 Label/Token 행
- 레거시 에셋 마이그레이션
- Bless/Relic 에셋 생성 또는 복구
- 전투 동작 변경
- 대규모 Namespace 또는 폴더 재구성

## 별도 폴더 마이그레이션 경계

아키텍처 참고 문서는 목표 소유 구조를 설명하지만 이 Task에서 프로젝트 전체 마이그레이션을 허용하지 않는다.

향후 별도 Task에서 스크립트를 이동한다면 다음을 지킨다.

- 같은 작업 단위에서 클래스명, Namespace, 동작을 함께 변경하지 않고 폴더와 파일만 이동한다.
- Unity GUID 보존을 위해 모든 `.cs.meta`를 `.cs`와 함께 이동한다.
- 기존 수정 및 미추적 작업을 보존한다.
- `Assets/Scripts/shring.zip` 같은 관련 없는 비스크립트 파일은 이동하거나 삭제하지 않는다.
- 이전 경로 잔여물, 누락 또는 중복 파일, GUID 보존, 참조, Unity 컴파일을 검증한다.

## 별도 SO/JSON 경계

향후 명시적으로 승인된 콘텐츠 작성 Task에서는 다음을 사용한다.

```text
Assets/Contents/<Content>/*.json
= 작성 원본

Assets/Contents/<Content>/Generated/*.asset
= 생성된 런타임 SO
```

`Assets/Contents`에는 작성 JSON과 생성 SO만 둔다. 이 후속 콘텐츠 배치 작업을 현재 Presentation 데이터 Task 또는 스크립트 폴더 개편과 섞지 않는다.

## 작업 순서

1. 1단계 인벤토리 및 경계 검증 — 완료.
2. 공통 중립 계약 — 완료.
3. Effect 정규화 모델과 단일 분기 진입점 — 완료.
4. Effect Config 매핑 분기 및 사용자 실행 가능 Unity 자체 테스트 — 구현 완료, 의미 재그룹화 및 특수 효과 정정 후 현재 15개 Case Unity 재실행 대기.
5. Skill 조합 — 코드 완료, 중첩 Skill 순회와 상세 확장은 사용자 결정으로 보류.
6. 승인 에셋 검증 도구 — 완료, 이전 58개 Skill PASS는 최신 플레이어 표시 카탈로그 전 결과이며 현재 사용자 재실행 대기.
7. 현행 정의 Character, Bless, Relic Adapter — 코드 완료, 승인된 현행 에셋 검증 대기.
8. 플레이어 표시 카탈로그, Compact Formatter 및 공통 View 인계 — 엄격한 StringManager Catalog 조회, Character/Skill/Bless/Relic Presenter, 의미 필터, 스크롤 갱신 완료, Skill Presenter 최종 소유 결정과 사용자 담당 Unity Localization 시각 및 스크롤 입력 검증 대기.

## 1차 결과물 검증 매트릭스

- 승인 경로에 실제 존재하는 현행 Effect 타입
- Effect 1개를 가진 Skill
- 여러 Effect를 가진 Skill
- Effect가 없는 Skill
- Preview 데이터
- 확인 가능한 Runtime Resolver 결과
- 서로 다른 퍼센트 범위
- 미지원 Config Fallback
- 값을 추론하지 않는 Unclassified Skill

각 사례는 다음을 비교한다.

```text
승인된 SO 또는 Runtime 원본
-> 콘텐츠 소유 Resolver 또는 Builder
-> 공통 Presentation 데이터
```

## 인계 상태

다음 에이전트는 `AgentDocs/Machal/README.md`와 `AgentDocs/Machal/owned-effects-inventory-task-start-ko.md`부터 시작해야 한다. 이 문서만 읽고 구현을 시작하지 말고 시작 계약의 정확한 읽기 순서, 기본 작업 가이드, Task 로그를 모두 따른다.

## 2026-08-13 패널 캐릭터 탐색 확장

- `CharacterSkillContentInfoPresenter`가 `Panel_CharacterInfo`의 정렬된 CharacterSO 목록과 이전/다음 선택을 소유한다.
- 선택이 바뀌면 기존 도메인 Presenter를 통해 캐릭터 본문과 선택 캐릭터의 스킬 아이콘 탭을 함께 갱신한다. Presentation 데이터나 Localization 소유권은 프리팹으로 이동하지 않았다.
- 기존 단일 Character 필드는 한 항목용 fallback으로 유지하고, 기존 초기 스킬 인덱스 직렬화 값도 호환된다.
- `Panel_CharacterInfo.prefab`은 기존 Character Presenter를 참조하며, 캐릭터 시작 선택 소유자를 하나로 만들기 위해 해당 Presenter의 독립적인 시작 빌드를 끈다.
- 소스 컴파일은 오류 0개, 기존 경고 35개로 통과했다.
- Button 오브젝트나 이벤트는 생성하지 않았다. 버튼 생성, 이벤트 연결, Character 목록 입력, 프리팹 저장, Play Mode 검증은 사용자가 담당한다.

## 2026-08-14 Bless 목록 Presenter 확장

- 신앙 페이지와 축복 종류별 화면 구성은 후순위로 유지한다.
- `BlessContentInfoPresenter`가 인스펙터의 `List<BlessSO>`를 소유하고, null이 아닌 Bless마다 범용 `UISelectableIconButton`을 하나씩 생성한 뒤 선택한 Bless를 기존 `BlessPresentationResolver`를 통해 `UIContentInfoView`에 연결한다.
- `UIContentInfoView`는 콘텐츠 중립성을 유지하며 현재 선택된 정규화 결과 하나만 표시한다.
- 단일 Bless를 직접 전달하는 기존 `SetBless`, `ShowBless`, `BuildPresentation`, `Bless` 접근 경로는 호환을 위해 유지한다.
- Unity 프리팹 연결과 Play Mode 검증은 사용자 작업으로 남긴다.

## 2026-08-14 후순위 신앙 페이지 설계 정정

- 향후 신앙 페이지에서 특정 신 탭을 선택하면 해당 신의 신앙 기능 세트 전체로 정보 페이지를 교체한다. 캐릭터 선택과 해당 캐릭터의 스킬 탭이 연결되는 구조와 같은 소유 패턴을 사용한다.
- 향후 신앙 페이지 Presenter가 신 목록, 선택된 신, 서로 다른 타입의 신앙 기능 탭을 소유한다. 축복 기능은 Bless Presentation 경로에 위임하고 전용전직은 자체 Domain Adapter에 위임하며, `BlessContentInfoPresenter`가 신앙 페이지 전체를 소유하면 안 된다.
- 각 신은 신앙 레벨과 함께 강화되는 기본축복 하나, 직업군 전용전직 하나, 신앙 고정 시 획득하는 전용축복 1, 신앙 고정 후 신앙 레벨 8 달성 시 획득하는 전용축복 2까지 네 개의 신앙 기능을 가진다.
- 축복 기능 세 개는 서로 다른 축복이다. 기본축복의 레벨별 강화 상태를 서로 무관한 여러 축복 탭으로 만들면 안 된다. 전용전직은 `BlessSO`가 아니다.
- 전용축복 2의 확정 획득 조건은 신앙 고정 후 신앙 레벨 8 달성이다. 이 규칙은 현행 `successorFaithLevel` 필드나 메서드 이름에서 추론하지 않고 명시적으로 표현해야 한다.
- 향후 신앙 기능 Adapter는 작성된 식별자와 진행 상태를 보존해야 하며, 이름이나 목록 순서로 네 기능 슬롯을 추론하면 안 된다.
- 구현 전 해결할 현행 소스 불일치: 현재 모델은 `ShrineBlessingGroup.Base/Enhanced`만 제공하고, `ShrineGodSO.GetAvailableBlessings`는 Group 인자를 사용하지 않으며, `BlessPoolEntry`에는 `progressionStep`만 저장된다. 이번 작업에서는 Runtime 동작을 변경하지 않는다.

### 후순위 신앙 프리팹 준비

- 합성 패널 `Panel_FaithInfo` 하나를 준비하고 그 아래에 신 탭 Root, 선택된 신의 헤더/신앙 진행 영역, `BlessContentInfoTabRoot`, AutoBind를 위해 인스턴스 이름을 `BlessContentInfoView`로 바꾼 기존 `UIContentInfoView_Bless` 하나를 둔다.
- 반복 사용 가능한 신 아이콘 탭 프리팹 하나와 신앙 기능 아이콘 탭 프리팹 하나를 준비한다. 둘 다 `UISelectableIconButton`을 사용할 수 있으며, 네 기능 타입별 상세 프리팹을 따로 만들지 않는다.
- 신앙 기능 탭 프리팹에는 `UI_IconImage`, `UI_SelectedFrameImage`, `UI_LockedOverlay`를 둔다. 획득 상태 연결은 아직 구현하지 않았지만 전용전직과 전용축복 1/2의 잠금 표시에 필요하다.
- 네 신앙 기능은 모두 선택된 하나의 중립 Content View에 표시한다. 축복 기능은 Bless Presentation 데이터를 사용하고 전용전직은 별도 Adapter가 필요하다.
- 일반/Common 축복은 특정 신이 소유하지 않으므로 이 후순위 선택 신 프리팹 계약의 범위 밖에 둔다.

### 확정된 보유 효과 및 도감 페이지 소유권

- 권위 있는 게임 규칙: 유물은 보유하는 순간 모두 적용되고 일반 축복도 획득하는 순간 적용되며, 둘 다 장착 상태를 사용하지 않는다. 신앙은 잠금 해제와 레벨업을 갖는 진행 상태다.
- 신앙 도감 페이지는 신 선택, 신앙 레벨, 고정 상태, 기본축복 강화, 전용전직, 전용축복 해금 진행을 설명하므로 전체 페이지를 별도로 유지한다.
- 기존 유물 페이지 화면 역할을 탭이 없는 `보유 효과` 페이지 하나로 전환한다.
- 페이지는 세로 Scroll 하나를 사용하고 현재 적용 중인 보유 유물, 획득한 일반 축복, 활성 신앙 축복을 카테고리 섹션으로 동적 생성한다. 항상 보유/활성 항목만 표시하며 `Catalog` 옵션을 노출하지 않는다.
- 유물 도감과 일반 축복 도감은 별도 페이지로 만들고 공통 카테고리/아이템 시스템을 `Catalog` 모드로 재사용할 수 있다. 전체 신앙 진행, 비활성 기능, 향후 해금은 별도 신앙 도감에 유지한다.
- 향후 명시적인 Effect 원본이 작성되지 않는 한 전용전직은 보유 효과의 신앙 축복 섹션에서 제외한다.
- 인벤토리 항목을 선택하면 우측의 중립 `UIContentInfoView` 하나에 상세 정보를 표시한다.
- 또 다른 보유 전용 축복 페이지는 만들지 않는다. 보유 효과 페이지에는 획득한 일반 축복을 표시하고, 별도 일반 축복 도감에서는 획득/미획득 정의를 표시할 수 있다.
- 향후 유물 도감은 획득/미획득 유물을 모두 보여주는 별도 페이지다. 현재 잠금/실루엣/보유 수를 처리하는 `RelicCollectionView`는 보유 효과 View로 일반화하지 않고 미래 유물 도감용으로 보존한다.
- 구현 전 해결할 현행 코드 불일치: `RelicItemService`에는 아직 보유/장착 목록이 함께 있고 `BlessManager.AddBless`는 기존 영구 Common 축복을 교체한다. UI는 이 오래된 동작을 가정하지 말고 Runtime 소유 규칙을 권위 있는 기획에 맞춘 뒤 따라야 한다.

현재 구현 상태:

- `ContentInventoryData`, `ContentInventoryItemView`, `ContentInventoryCategoryView`가 중립 데이터, 아이템, Scroll 없는 카테고리 섹션 계층을 제공한다.
- `OwnedEffectInventoryView`와 `OwnedEffectInventoryPresenter`는 현재 탭 없는 카테고리 페이지를 구현한다. Presenter는 설정된 Preview 정의 또는 보유 유물, 활성 일반 축복, 활성 신앙 축복의 명시적 Runtime 목록을 받으며 Manager 자동 수집은 구현하지 않았다.
- 사용자는 2026-08-18 작업 단위에서 `Assets/Prefabs/UI/Fixed/Panel/Panel_OwnedEffects.prefab`, `Assets/Prefabs/UI/Fixed/Content/UIContentInventoryCategory.prefab`, `Assets/Prefabs/UI/Fixed/Content/UIInventoryItemView.prefab` 세 경로의 직접 컴포넌트 연결만 명시적으로 승인했다. 필수 직렬화 참조를 모두 지정하고 패널 내부 `RelicContentInfoPresenter`만 제거했으며 공용 상세는 활성 상태로 유지했다. 이 1회 승인은 이후 프리팹 YAML 수정이나 Unity 조작 권한이 아니다.
- 기존 `RelicCollectionView`는 별도 유물 도감용으로 보존한다. Unity Import, Inspector 역직렬화, 원본 목록 할당, 카테고리/아이템 상호작용, 상세 연결, 외부 스크롤, 로컬라이징, 시각 레이아웃은 사용자 검증으로 남는다.

## 2026-08-17 신앙 페이지 상세 설계

최신 프리팹, 데이터, 소유권, 상호작용, 구현 순서, 검증 계약은 `AgentDocs/Machal/faith-page-design-ko.md`다.

- 페이지는 획득 신앙 탭, 선택 신 요약, 레벨 1-10 로드맵, 정확히 네 개의 재사용 기능 카드, 중립 선택 상세 View 하나를 사용한다.
- `FaithPagePresenter`와 `ShrineFaithPresentationResolver`가 기존 `BlessContentInfoPresenter`의 신앙 페이지 소유 계획을 대체한다.
- 기본/전용 축복 상세는 `BlessPresentationResolver`에 위임하고 전용전직은 명시적인 Character 직업군/목표 직업 원본을 사용한다.
- 잠긴 미래 기능도 Preview로 열람할 수 있지만 활성 Runtime 효과라고 표시하지 않는다.
- 필요한 프리팹 종류는 `Panel_FaithInfo`, `UIFaithGodTab`, `UIFaithLevelNode`, `UIFaithFeatureCard`, 선택적인 `UIContentInfoView_Faith` 레이아웃 Variant다.
- 전용전직 해금 규칙, 목표 직업 매핑, 현행 Faith/Bless 작성 경로, 명시적인 네 기능 진행 정의가 확정될 때까지 구현은 대기한다.

## 2026-08-17 카테고리 섹션 Content Inventory 리팩터링

- 최종 인벤토리 레이아웃은 페이지 세로 `ScrollRect` 하나를 사용한다. 정렬된 카테고리 섹션과 각 아이템 Grid를 하나의 Scroll Content 안에 생성하며, 카테고리에는 중첩된 세로 `ScrollRect`를 두지 않는다.
- Relic, 일반 Bless, 활성 Faith Bless는 원본과 보유 상태를 해석하는 도메인 Presenter를 각각 유지한다. 콘텐츠 중립 페이지 계층이 섹션 생성, 전체 섹션 선택 상태, 카테고리 필터, 공용 `UIContentInfoView` 하나를 소유한다.
- 동일한 카테고리/아이템 시스템이 서로 다른 페이지에서 `OwnedOnly`와 `Catalog` 생성 모드를 지원한다. Runtime 보유 필터링은 도메인 Presenter 책임이며 공통 View는 전달받은 획득 상태만 표현한다.
- 보유 효과 페이지에는 탭이 없고 보유/활성 유물, 일반 축복, 신앙 축복 섹션만 요청한다. 별도 유물/일반 축복 도감은 `Catalog`를 요청할 수 있고, 신앙 도감이 신앙 Catalog와 진행 역할을 소유한다.
- 1단계에서 기존 `OwnedEffect` 골격을 교체하지 않고 `ContentInventoryData.cs`를 병행 추가했다. 중립 계약은 페이지, 카테고리, 아이템 Snapshot과 `OwnedOnly`/`Catalog`, `Owned`/`Unowned`/`Locked` 상태를 포함한다.
- 카테고리 식별은 중립 문자열 ID와 StringManager Localization Key로 구성한다. 공통 계약에는 Relic/Bless 전용 카테고리 Enum을 정의하지 않는다.
- 임시 프로젝트 포함 후 Solution 정적 컴파일은 오류 0개, 기존 경고 197개로 통과했고 생성 프로젝트 파일은 복구했다. 이번 단계에서는 Unity Import와 Runtime 동작을 검증하지 않았다.

## 2026-08-18 일반 축복 도감 2단계

- 첫 통합 Catalog 대상은 탭 없는 보유 효과 페이지가 아니라 별도의 일반 축복 도감이다.
- 호출자는 전체 `IReadOnlyList<BlessSO>` 또는 `BlessPoolSO` 하나를 전달할 수 있다. 해당 원본의 null이 아닌 고유 Bless 정의를 모두 계속 표시하며 Pool의 weight와 progressionStep은 생성 메타데이터이므로 인벤토리 표시 필드로 사용하지 않는다.
- 별도로 전달받은 활성 `BlessRuntimeData.BlessEntry` 목록을 작성된 `BlessingId`로 대조하여 일치하는 정의만 활성 표시한다. 비활성 정의도 계속 표시하며 선택할 수 있다.
- 활성 상태를 선택, 획득, 잠금 상태와 결합하지 않고 중립 Item 계약에 `ContentActivationState.Inactive/Active`를 추가했다.
- `ContentInventoryItemView`와 `ContentInventoryCategoryView`를 추가했다. Item은 선택, 잠금, 활성 Visual을 각각 분리한다. Category는 Localization 제목, 개수, 동적 Item Grid를 연결하며 의도적으로 `ScrollRect`, 구체 SO, Resolver, Manager, Detail View를 소유하지 않는다.
- 임시 프로젝트 포함 후 깨끗한 Solution 정적 컴파일은 오류 0개, 기존 경고 197개로 통과했고 생성 프로젝트 파일은 복구했다. Unity Import, AutoBind, 프리팹, 활성 Visual, Play Mode 동작은 아직 검증하지 않았다.

## 2026-08-18 보유 효과 프리팹 직접 연결

- 이전 네 탭 `OwnedEffectInventoryView`와 `OwnedEffectInventoryPresenter` 동작을 유물, 일반 축복, 활성 신앙 축복의 정렬된 `OwnedOnly` 섹션을 만드는 세로 Scroll 페이지 하나로 교체했다.
- 사용자가 준비한 세 프리팹에 `ContentInventoryItemView`, `ContentInventoryCategoryView`, `OwnedEffectInventoryView`, `OwnedEffectInventoryPresenter`를 붙이고 필수 직렬화 참조를 지정했다.
- 패널 내부 `RelicContentInfoPresenter`만 제거했다. 해당 소스와 전용 페이지 역할은 바꾸지 않았으며 공용 `UIContentInfoView`는 활성 상태와 콘텐츠 중립성을 유지한다.
- 페이지 및 카테고리 제목을 위한 `presentation.inventory.*` StringManager 행 네 개를 추가했다.
- 직접 프리팹 YAML 연결은 이 세 프리팹에 대한 사용자 1회 승인이다. 이후 프리팹 YAML 수정이나 Unity 조작에는 새 명시적 요청이 필요하다.
- 보고 및 정적 확인된 검증: `dotnet build ProjectBS.sln --no-restore -v:minimal`은 오류 0개, 기존 경고 197개로 통과했고 스크립트 연결, 필수 참조, 구형 Presenter 제거, 공용 상세 활성 상태, 로컬라이징 중복 검사가 통과했다.
- 다음 단계는 사용자 Unity 검증이다. Runtime Manager 자동 수집과 별도 도감 페이지는 이후 작업 단위로 남긴다.

## 2026-08-21 신앙 로드맵 현재/다음 비교 정정

- 신앙 성장 로드맵 아래 영역은 더 이상 독립 기능 카드 네 개와 선택 상세 View 하나로 구성하지 않는다.
- 동일한 역할의 `UIFaithLevelEffectCard` 두 개만 배치한다. 하나는 실제 현재 레벨 신앙 효과 카드이고 다른 하나는 바로 다음 레벨 신앙 효과 카드다.
- 현재 카드는 현재 적용되는 전체 신앙 기능을 표시한다. 다음 카드는 작성된 다음 레벨 전체 결과를 표시하고 원본 ID가 같은 Entry는 `Strengthened`, 다음 레벨에 처음 나타나는 Entry는 `NewlyUnlocked`, 바뀌지 않은 Entry는 `Unchanged`로 분류한다.
- 비교에는 안정적인 작성 원본 기능/Entry ID와 정확한 현재/다음 값을 사용한다. Localization 라벨을 비교하거나 차이 값을 창작하거나 원본 식별이 불충분한 강화 관계를 추론하지 않는다.
- 최대 레벨에서도 다음 카드 위치를 유지하고 Localization된 다음 레벨 없음 빈 상태를 표시한다. 미래 로드맵 노드는 이정표를 알리며 실제 현재/다음 비교 쌍을 교체하지 않는다.
- 최신 프리팹과 데이터 계약은 `AgentDocs/Machal/faith-page-design-ko.md`다. Runtime 소스, 프리팹, Scene, 에셋, 빌드, Staging, Commit, Push 작업은 수행하지 않았다.

## 2026-08-21 신앙 메인 패널 골격 구현

- 사용자의 명시적인 프리팹 직접 수정 승인 범위에서 `Assets/Prefabs/UI/Fixed/Panel/Panel_FaithInfo.prefab`을 재구성했다.
- 패널 배경/전경을 유지하고 구형 `FaithDetailView`와 미완성 Body/로드맵/설명 오브젝트를 제거한 뒤 획득 신앙 탭, 선택 신 정보, 수평 레벨 노드 10개, 현재/다음 레벨 효과 카드 두 개를 만들었다.
- `FaithPageView`, `FaithPagePresenter`, `FaithGodTabView` 템플릿 하나, `FaithLevelNodeView` 10개, `FaithLevelEffectCardView` 두 개를 부착하고 참조를 연결했다. 각 효과 카드는 기존 중립 `UIContentInfoView` 프리팹 인스턴스를 포함한다.
- Inspector 설정 신 목록과 `Build Configured Faith Page` ContextMenu 호출 경로를 추가했다. 원본 기반 진행 데이터와 `ShrineFaithPresentationResolver`가 생길 때까지 현재/다음 신앙 비교 데이터는 명시적인 `[PLACEHOLDER]`다.
- 공용 AutoBind 검색이 `[AutoBind] GameObject` 필드를 `GetComponent(Type)`에 전달하지 않고 GameObject로 직접 연결하도록 수정했다.
- 고유한 `presentation.faith.*` Localization 행 13개를 추가했다.
- 정적 프리팹 검증 결과 신규 직렬화 컴포넌트 참조가 모두 연결됐고 레벨 노드는 10개이며 수평 Layout 두 개가 설정되고 구형 `FaithDetailView` 잔존이 없다.
- Unity Editor 정적 시각 확인에서 새 하이어라키와 균등한 10개 로드맵 배치를 확인했다. `dotnet build ProjectBS.sln --no-restore -v:minimal`은 오류 0개, 기존 경고 209개로 통과했다. Play Mode 데이터/상호작용 검증은 사용자 작업으로 남긴다.
