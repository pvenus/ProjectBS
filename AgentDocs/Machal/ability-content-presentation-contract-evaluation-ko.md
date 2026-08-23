# Ability 콘텐츠 Presentation 계약 평가

## 상태

- 평가일: 2026-08-12
- 범위: 현행 Effect 정규화, Skill에서 공통 Group으로의 변환, 플레이어 표시 Localization 카탈로그
- JSON 근거: `Assets/Resources/skill/json/` 아래 20개 파일 전체
- 정적 컴파일: 플레이어 표시 카탈로그 변경 후 `Assembly-CSharp-Editor.csproj` 오류 0건, 기존 경고 191건
- Unity Editor 검증: 사용자 실행 대기

## 최신 권위 계약

`EffectPresentationData`는 Identity, 선택적인 Activation, 정확히 하나의 정규화 Outcome, 작성 설명, Entry 제약, Provenance, Resolution 상태를 가진다.

Outcome 타입은 다음 7개다.

- `StatModifierPresentationData`
- `HealPresentationData`
- `CooldownChangePresentationData`
- `DisplacementPresentationData`
- `PeriodicDamagePresentationData`
- `SkillInvokePresentationData`
- `ControlPresentationData`

이 최신 결정은 Heal과 Periodic Damage를 하나의 `HealthChangePresentationData`로 합쳤던 중간 설계를 대체한다.

## JSON 근거

전략 Skill JSON 20개에는 `tags`라는 이름의 속성이 없다. 명시적인 분류 필드는 다음과 같다.

- `baseProfile.skillType`
- `baseProfile.skillComponentType`
- `baseProfile.brainMeta.category`
- `baseProfile.brainMeta.targetType`
- `baseProfile.brainMeta.tacticalNeed`
- `hits[].targetLayerMask`
- `hits[].buffEffects[]/debuffEffects[].effect.effectType`
- `hits[].buffEffects[]/debuffEffects[].categoryType`
- `hits[].buffEffects[]/debuffEffects[].lifetimeType`

현행 인벤토리는 Skill 20개, Skill당 Hit 1개, Effect Entry 18개다. Effect 정규화는 승인된 Outcome 타입을 사용하고 Skill 분류 Tag는 임의 Prefix 없이 원본 분류값을 사용한다.

## 교정된 표시 변환

- Skill UI Group Key는 승인된 다섯 의미 역할인 `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, `LinkedSkill`을 사용한다.
- Effect의 7개 Outcome 타입은 내부 정규화 타입으로 유지하며 Effect마다 UI Group을 만드는 대신 Skill Group으로 분배한다.
- 시전/대상/Trigger/Chance는 `Activation`, 투사체/이동/Burst/Hit 주기는 `Delivery`, Damage/Heal/Stat/Cooldown/Periodic/Spawn 결과는 `Outcome`, `Control`과 `Displacement`는 `SpecialEffect`, `SkillInvoke`는 `LinkedSkill`로 보낸다.
- `PresentationGroupData.SourceContentId`는 단독 Effect Group과 상세 이동에 사용할 수 있도록 유지하며, 이를 복원하거나 임의 Skill Group Label로 사용하지 않는다.
- 원본 필드 하나는 Value 하나를 가진 `PresentationEntryData` 하나가 된다.
- Label은 원본 필드를 플레이어용 단어로 변환할 수 있지만 값은 결합, 재계산, 대체하지 않는다.
- 작성 숫자는 원본 숫자와 원본 단위를 유지한다. `Chance=0.25 Ratio`는 화면에서 `25%`로 Format할 수 있지만 정규화 데이터는 `0.25 Ratio`로 남고, `ChancePercent=25`는 `25 Percent`로 남는다.
- 작성 정규화에서는 숫자를 Clamp하거나 최솟값으로 대체하거나 Ratio 데이터를 Percent 데이터로 변환하거나 횟수를 합성하지 않는다. Runtime 해석 값은 명시적인 Runtime 원본에서 얻고 Runtime Provenance를 가진 경우에만 허용한다.
- 제거 대상인 임의 표시 구조는 `Skill.Hit.1.Damage`, `Skill.Effect.Self.1`, `Behavior`, `CountAndScale`, `SizeAndLifetime`이다.
- 원본 필드와 정규화 컴포넌트 Key는 검사/Provenance 데이터로 유지한다. 플레이어 Group, Entry, Tag, Enum 대체 단어, 값 포맷 텍스트는 명시적인 `PresentationDisplayCatalog` 매핑을 통해서만 `Assets/Resources/string/presentation_string.csv`의 표준 `presentation.*` 행을 조회한다.

## 알려진 원본 경계

생성 SO는 Effect `entryId`, Hit `damageId`, 선택적 속성의 존재 여부처럼 JSON의 모든 Identity나 Property Presence를 보존하지 않는다. Presentation은 이름 규칙으로 이 값을 복원하지 않는다. 현행 SO에 보존된 값과 ID만 사용하고, 의미 있게 존재한다고 확인할 수 없는 선택 구조는 플레이어 표시에서 제외한다.

## 남은 계획 일치도 평가

- 일치: 검사 창은 전체 `Resolve()` 경로를 사용하므로 `0`, `999` 같은 작성값과 기본값을 그대로 표시한다. 플레이어 UI만 `ResolveForPlayerDisplay()`를 사용한다.
- 일치: View 계층 참조는 `AutoBindPrefix`/`AutoBind`를 사용하고 Prefab Template 에셋 참조는 명시적으로 지정한다.
- 미병합: `Build Presentation`과 지정 `EquipmentSkillSO`는 여전히 `UIContentInfoView`가 아니라 `SkillContentInfoPresenter`에 있다. 이를 공용 View로 옮기면 루트 Presentation이 구체적인 Skill 도메인에 의존해 콘텐츠 중립 의존 규칙과 충돌한다. 조용히 합치지 말고 최종 소유 결정을 명시적으로 확정해야 한다.
- 미확인: Prefab에는 세로 `ScrollRect`, 연결된 Content/Viewport, `RectMask2D`, Vertical Layout, Content Size Fitting이 있다. 그러나 Viewport에 Raycast 가능한 `Graphic`이 없어 활성 자식 Graphic이 Pointer Raycast를 받지 않는 영역에서는 Wheel/Drag 입력이 계속 실패할 수 있다. Scene의 활성 `EventSystem`도 필요하다. Unity에서 사용자가 확인하고 필요하면 Viewport에 투명한 Raycast Target `Image`를 추가해 재검증한다.
- 결정에 따른 보류: 중첩 Skill 상세 조합은 이번 작업 범위 밖으로 유지한다.

## Unity에서 필요한 검증

1. `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`를 실행한다.
2. `Tools > ProjectBS > Presentation > Run Skill Asset Validation`을 실행한다.
3. Play Mode에서 설정된 Skill 콘텐츠 컴포넌트의 `Build Presentation`을 실행한다.
4. `skill.strategic.golden_chain_formation` 같은 전략 Skill을 확인한다.
   - `projectileColliderRadius`와 `projectileLifetime`이 서로 다른 행으로 표시된다.
   - 번호가 붙은 Skill/Effect Group Title이 없다.
   - 표시 Group은 다섯 의미 Skill Group 이름으로 제한된다.
   - Stun/Root/Taunt는 `SpecialEffect` 아래에 `Control` 종류와 원본 기반 지속시간으로 표시되고, Knockback/Pull은 원본 Config에 따라 `Displacement` 방향과 Force 또는 Distance로 표시된다.
   - Localization Label은 `presentation_string.csv`에서 조회된다.
5. 활성 Detail Button이 없는 Entry 영역에서 Wheel과 Drag 입력을 확인한다. 스크롤되지 않으면 Unity에서 Viewport에 투명한 Raycast Target `Image`를 추가하고 다시 확인한다.

에이전트는 Unity Editor를 실행하거나 조작하지 않으며 사용자가 이 검증을 담당한다.

## 2026-08-12 표시 카탈로그 재평가

- Label이 원본 Source Path를 사용한다는 이전 규칙은 내부 Entry Key에 대한 설명이지 플레이어 텍스트 규칙이 아니었다. 플레이어 텍스트는 이제 명시적인 표준 `presentation.*` Key를 `StringManager`로 조회한다.
- 기존 Skill/Bless/Relic 이름과 설명 조회 및 기존 대체 동작은 변경하지 않는다.
- 플레이어 Group, Entry, Tag, DamageType, ControlType, Displacement Direction, 값 포맷 텍스트는 `PresentationDisplayCatalog` 매핑을 사용한다. 매핑이 없으면 원본 Key나 임의 생성 Pascal Case 텍스트로 대체하지 않는다.
- Skill 5개 Group과 내부 Effect 7개 Outcome은 변경하지 않는다. 이번 작업은 정규화가 아니라 표시 어휘와 플레이어 필터를 변경한다.
- 전체 원본 필드 분류는 `AgentDocs/Machal/ability-content-presentation-display-catalog.md`에 기록했다.
- 정적 검증: Editor Assembly 빌드 오류 0개, 기존 경고 191개로 완료했으며 Localization CSV 데이터 294행, `presentation.*` Main Key 154개, 복합 Key 중복 0개, 정적으로 필요한 Catalog Key 141개 중 누락 0개를 확인했다.
- 이 표시 계약 변경 후 Unity Effect/Skill 검증과 최종 화면 확인은 사용자 담당으로 대기한다.

## 2026-08-12 누락 Localization Key 표시 정정

- 이 정정은 매핑된 Localization 텍스트가 누락되면 생략한다는 이전 활성 결론을 대체한다.
- Catalog 승인과 Localization 해석은 서로 다른 단계다. 승인 매핑이 없는 원본 게임플레이 Key는 계속 필터링하지만, 승인된 매핑 Key가 StringManager에서 누락되면 디버깅을 위해 의도한 전체 `mainKey.subKey`를 표시한다.
- `PresentationLocalizedTextResolver.ResolveRequired`는 `returnNullIfMissing: true`로 Main Key 후보를 순서대로 확인한다. 처음 해석된 텍스트를 반환하고, 모든 후보가 실패하면 첫 번째 의도 Key를 일반 조회하여 StringManager가 전체 Key를 노출하게 한다.
- Skill, Effect, Bless, Relic 설명도 같은 필수 조회를 사용한다. 기존 후보 경로와 순서는 유지하며 구조화된 값으로 대체 문장을 만들지 않는다.
- StringManager를 사용할 수 없을 때의 에셋 이름 Fallback은 변경하지 않는다.
- 정적 검증은 오류 0개, 기존 프로젝트 경고 191개로 완료했다. 임시로 존재하지 않는 매핑 Key를 사용한 사용자 담당 Unity 화면 검증은 대기하며, 확인 후 Key를 원복해야 한다.
