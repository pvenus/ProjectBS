# 3단계: Effect 정규화 모델

## 상태

- 설계 준비: 완료
- 스크립트 구현: 2026-08-11 완료
- 다음 단계: 4단계 Config 매핑 분기
- 선행 조건: 충족, 2단계 공통 계약 및 필수 코드 작성 가이드 사용 가능

## 목표

3단계에서 가장 작은 Typed Effect 정규화 모델과 단일 `EffectPresentationResolver` 진입점을 추가했다. Config 매핑 구현은 4단계 범위다.

## 최초 파일 범위

작고 평평하게 시작한다.

```text
Assets/Scripts/Ability/Effects/
  Data/
    EffectPresentationData.cs
  EffectPresentationResolver.cs
```

관련 Effect Presentation 타입은 처음에는 `EffectPresentationData.cs` 하나에 둔다. 탐색이 어려워지거나 독립적으로 변경될 때만 파일을 나눈다.

추가하지 않을 항목:

- `IEffectConfigPresentationResolver`
- Config별 Resolver 클래스
- 이 기능만을 위한 `Resolvers/` 폴더
- `EffectConfig`의 Presentation 변환 메서드

## 2단계 의존 항목

3단계는 공통 계층의 다음 타입을 사용한다.

- `PresentationIdentityData`
- `PresentationContext`
- `PresentationProvenanceData`
- `PresentationValueData`
- `PresentationEntryData`
- `PresentationGroupData`
- `ContentPresentationData`

공통 계약 7개는 모두 `Assets/Scripts/Presentation/` 아래에 구현되었고 2단계 Smoke 검증을 통과했다.

Effect 모델은 후속 Grouping 계층이 공통 Group으로 변환하기 전까지 Typed 도메인 데이터를 유지할 수 있다.

## Effect 데이터 모델

`EffectPresentationData` 구성:

- 원본 `EffectSO`의 공통 Identity
- 존재하는 경우 작성된 설명
- 선택적인 `EffectActivationPresentationData`
- 설명 전용 또는 미지원 상태가 아니라면 정확히 하나의 `EffectOutcomePresentationData`
- Entry가 소유하는 적용 제약
- 공통 Provenance
- 지원, 설명 전용, 미지원 상태의 명시적 구분

`EffectActivationPresentationData`에는 발동 조건만 둔다.

- Trigger: None, OnHit, OnHeal, OnAttack
- 존재하는 경우 작성된 Chance 숫자와 원본 단위
- 존재하는 경우 Heal 대상 조건
- 존재하는 경우 치명타 요구 조건

`EffectEntryConstraintPresentationData`에는 런타임과 관련 있는 Entry 데이터를 둔다.

- Buff 또는 Debuff Category
- Lifetime 종류
- 런타임이 Entry Duration을 사용하는 경우의 Duration
- 실제 `EffectEntrySO.MaxApplyCount` 필드의 최대 적용 횟수

Interval과 Duration으로 적용 횟수를 계산하지 않는다.

## Outcome 타입

초기 파일 하나 안에서 Typed Outcome Base와 다음 의미 Payload를 유지한다.

- `StatModifierPresentationData`
- `HealPresentationData`
- `CooldownChangePresentationData`
- `DisplacementPresentationData`
- `PeriodicDamagePresentationData`
- `SkillInvokePresentationData`
- `ControlPresentationData`

이 타입은 원본 Config 구조나 최종 UI 문자열이 아니라 정규화된 의미를 나타낸다.

### Activation 축과 Outcome 축

Activation과 Outcome은 서로 다른 질문에 답하므로 합치지 않는다.

- Activation은 Effect가 언제 시작되는지 나타낸다: `OnHit`, `OnHeal`, `OnAttack`.
- `Heal`은 회복량과 Clamp 동작을 보존한다.
- `PeriodicDamage`는 피해 계수, 빈도 단위, Interval, Duration을 보존한다.
- 원본 값은 명시적인 Typed 필드로 유지하고 새 숫자로 결합하지 않는다.

따라서 `OnHit`와 `OnHeal`은 서로 다른 Trigger로 유지하고, `Heal`과 `PeriodicDamage`는 승인된 7개 Outcome 계약에 따라 서로 다른 Outcome 타입으로 유지한다.

## 단일 Resolver 표면

최초 공개 표면:

```text
EffectPresentationData Resolve(
    EffectEntrySO entry,
    PresentationContext context)
```

Resolve 흐름:

```text
Entry 검증
-> EffectSO와 Entry 제약 읽기
-> EffectSO.Config 타입 분기
-> 선택적인 Activation 구성
-> Outcome 하나 구성
-> Provenance 연결
-> EffectPresentationData 반환
```

3단계에서는 호출 가능한 메서드와 미지원/Fallback 동작을 확립한다. Config 13개 분기는 4단계에서 채운다.

## 검증 및 Fallback

- Null `EffectEntrySO`: 문장을 만들지 않은 미지원 결과
- Null `EffectSO`: 가능한 경우 Entry Provenance를 유지한 미지원 결과
- Null Config: 작성된 Effect 설명이 비어 있지 않을 때만 설명 전용, 아니면 미지원
- 알 수 없는 Config: 같은 설명 전용 Fallback 규칙 사용
- `EffectEntrySO.ValueOverride`: 런타임이 적용하지 않으므로 활성 값이나 적용된 Provenance로 기록하지 않음
- Effect Upgrade Modifier: Resolve 값으로 적용하거나 표시하지 않음

의미가 불분명한 레거시 `SkillEffectSO`는 Skill 도메인이 소유한다. Ability Effects가 Skills에 의존하지 않도록 설명 전용 Fallback은 5단계 Skill 조합에서 처리한다.

## 4단계 분기 구현 순서

독립적으로 검증 가능한 묶음으로 구현한다.

1. 현행 연결 에셋으로 검증 가능:
   - `StatModifierEffectConfig`
   - `HealEffectConfig`
   - `CooldownReduceEffectConfig`
   - `KnockbackEffectConfig`
   - `TauntEffectConfig`
2. Triggered StatModifier 및 Cooldown 매핑:
   - `ChanceOnHitStatModifierEffectConfig`
   - `OnHitTimedStatModifierEffectConfig`
   - `ChanceOnHealStatModifierEffectConfig`
   - `ChanceOnHealCooldownReduceEffectConfig`
3. Periodic Damage, 거리 이동, Skill 호출:
   - `AttackBleedEffectConfig`
   - `OnHitPoisonDotEffectConfig`
   - `OnHitKnockbackDistanceEffectConfig`
   - `ChanceOnHitSkillEffectConfig`

Private Helper는 정규화 결과 생성이 반복될 때만 Outcome 기준으로 묶는다.

## 4단계로 이어갈 단위 및 출처 규칙

- 작성된 원본 숫자, 원본 단위, Provenance를 명시적으로 보존한다.
- Chance 필드를 하나의 표준 숫자 표현으로 변환하지 않는다. Ratio와 Percent 데이터를 구분하고 Formatter는 표시 문자열만 바꿀 수 있다.
- Flat, Ratio, Percentage, Seconds, Meters, Force, Count를 구분한다.
- Taunt Duration은 `EffectEntrySO.Duration`에서 온다.
- Duration과 Interval로 Periodic 적용 횟수를 계산하지 않는다.
- JSON에만 있는 선언은 작성 전용 Provenance를 사용하며 활성 Runtime 결과가 아니다.

## 3단계 검증 결과

- 임시 Placeholder 진입점을 실제 호출하고 로그를 확인한 뒤 최종 Fallback 생성 동작으로 교체했다. Production Placeholder는 남아 있지 않다.
- 격리 Smoke 결과는 `STAGE3_EFFECT_PRESENTATION_SMOKE_OK`였다.
- Null Entry, Null Effect, 설명 전용 Fallback, 미지원 Fallback, Provenance, Timed 및 Instant 제약, 명시적인 Seconds 및 Count 단위, OnHit와 OnHeal Trigger 구분, 분리된 Heal 및 PeriodicDamage Outcome을 검증했다.
- Effect 데이터 소스는 소유 Ability Effects 타입과 공통 Presentation 계약만 참조하며, 공통 계약에는 구체 SO 의존성이 없다.
- Config나 에셋을 변경하지 않았다.
- Unity가 `EffectPresentationData.cs`와 `EffectPresentationResolver.cs`를 포함해 `Assembly-CSharp.dll`을 다시 생성했다.
- 에셋 기반 Config 매핑 검증은 별도 4단계 책임으로 남겨 둔다.

## 종료 조건

다음을 만족하면 3단계 완료다.

- Typed Effect 모델이 컴파일된다.
- 공개 Resolver 진입점 하나가 호출 가능하다.
- Fallback과 미지원 결과가 결정적이다.
- 값을 만들지 않고 Provenance와 Entry 제약을 전달한다.
- Config별 Resolver 계층이 없다.
- 공개 계약을 변경하지 않고 4단계 Switch 분기를 하나씩 추가할 수 있다.

모든 종료 조건은 2026-08-11 충족했다.
