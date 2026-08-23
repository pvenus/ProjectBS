# 4단계: Effect Config 매핑 및 검증

## 상태

- 구현: 2026-08-11 완료
- Production 매핑: 현행 `EffectConfig` 클래스 13종 모두 단일 `EffectPresentationResolver`에서 분기
- 이전 Unity 검증: 원본 충실도 수정 전에 Synthetic 매핑 13종 및 승인된 `EffectEntrySO` 에셋 20개 통과
- 현재 Unity 검증: 의미 재그룹화 및 특수 효과 정정 후 사용자 재실행 대기
- 다음 작업: 사용자가 15개 Case Effect 자체 테스트, Skill 에셋 검증, 설정된 UI Presentation을 재실행

## 구현 범위

- `StatModifierEffectConfig` -> `StatModifier`
- `ChanceOnHitStatModifierEffectConfig` -> `Activation(OnHit) + StatModifier`
- `OnHitTimedStatModifierEffectConfig` -> `Activation(OnHit) + StatModifier(duration)`. 단 `StunDuration`, `RootDuration`은 `Control(Stun/Root + duration)`으로 정규화
- `ChanceOnHealStatModifierEffectConfig` -> `Activation(OnHeal + Target) + StatModifier`
- `HealEffectConfig` -> `Heal`
- `ChanceOnHealCooldownReduceEffectConfig` -> `Activation(OnHeal + Target) + CooldownChange`
- `CooldownReduceEffectConfig` -> `CooldownChange`
- `KnockbackEffectConfig` -> `Displacement(Force)`
- `OnHitKnockbackDistanceEffectConfig` -> `Activation(OnHit) + Displacement(Meters)`
- `AttackBleedEffectConfig` -> `Activation(OnAttack) + PeriodicDamage(PerSecond)`
- `OnHitPoisonDotEffectConfig` -> `Activation(OnHit) + PeriodicDamage(PerTick)`
- `ChanceOnHitSkillEffectConfig` -> `Activation(OnHit + Critical 조건) + SkillInvoke`
- `TauntEffectConfig` -> `Control(Taunt, duration)`

확률 값은 작성 필드의 숫자 표현과 단위를 보존한다. `ChancePercent`는 `Percent`, 비율 기반 `Chance`는 `Ratio`로 유지한다. Formatter가 Ratio를 백분율로 표시할 수는 있지만 정규화 숫자를 Clamp, 곱셈 또는 대체하지 않는다. 다른 작성 숫자 필드에도 같은 규칙을 적용한다. Runtime 해석 값은 실제 Runtime 출처에서 얻고 Runtime Provenance를 가진 경우에만 허용한다.

## 런타임 정확성 경계

- `ChanceOnHealStatModifierEffectRuntime`은 현재 `ValueType`을 무시하고 `Value`를 `AddStat`에 직접 전달하므로 Presentation은 실제 Operation을 Flat으로 기록한다.
- `HealEffectRuntime`은 `HealEffectConfig.ClampToMaxHp`를 읽지 않지만 `CharacterDamageService.Heal`은 항상 최대 HP로 제한한다. Presentation은 실제 Clamp 동작을 true로 기록하고 사용되지 않는 Config Flag는 노출하지 않는다.
- `ChanceOnHitSkillEffectConfig.RangeOverride`는 현행 런타임에서 사용되지 않으므로 제외한다.
- `ChanceOnHitSkillEffectConfig.RequireCriticalHit`은 런타임이 검사하므로 Presentation에 보존한다. 다만 현재 `EffectManager`는 실제 Hit 결과 대신 `true`를 Callback에 전달하므로 실제 치명타 전용 동작은 별도 게임플레이 수정이 필요하다.
- `OnHitKnockbackDistanceEffectRuntime`은 Pull과 그 외 방향만 구분하므로 Presentation은 Pull이 아닌 값을 PushAwayFromSource로 매핑한다.
- `ChanceOnHitStatModifierEffectConfig`의 Multiply는 현행 런타임이 0을 적용하므로 활성 값처럼 표시하지 않고 Fallback 처리한다.
- `OnHitTimedStatModifierEffectConfig`가 `StunDuration` 또는 `RootDuration`을 대상으로 하면 런타임의 Max-Set Timer 동작에 맞춰 `Control`로 정규화한다. 제어 지속시간은 `config.Value`이며 `config.DurationSeconds`로 대체하지 않는다.
- Skill 참조가 없는 `ChanceOnHitSkillEffectConfig`는 빈 호출을 지원 상태로 반환하지 않고 Fallback 처리한다.
- `EffectEntrySO.ValueOverride`, Upgrade Modifier, 계산으로 유도한 Periodic 적용 횟수는 계속 제외한다.

## 사용자 테스트

Unity에서 전체 매핑 검사를 실행한다.

1. `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`를 실행한다.
2. Console에서 `[EffectPresentationStage4SelfTest] PASS` 로그 1개를 확인한다.
3. 상세 내용에 `Synthetic mapping cases: 15`, `Approved EffectEntry assets: 20`이 표시되어야 한다. 15개 Case는 Config Class 13종과 `OnHitTimedStatModifierEffectConfig`의 Stun/Root 특수화 Case를 포함한다.

실제 Entry 하나를 확인한다.

1. `Assets/Resources/skill/character/generated/` 또는 `Assets/Resources/skill/json/` 아래의 `EffectEntrySO` 에셋을 선택한다.
2. `Assets > ProjectBS > Presentation > Log Selected Effect Entry`를 실행한다.
3. Console에서 Typed Status, Activation, Outcome, Constraints 필드를 확인한다.

이 메뉴는 데이터 계층만 확인한다. 플레이어 UI 검증은 설정된 `Build Presentation` 흐름을 별도로 실행한다.

## 검증 근거

- `dotnet build Assembly-CSharp.csproj --no-restore`: Error 0건, 기존 Warning 35건.
- Unity가 `Library/ScriptAssemblies/Assembly-CSharp.dll`을 `2026-08-11 03:05:25`에 다시 생성했다.
- 이전 Unity 자체 테스트: PASS, Synthetic 매핑 13종, 승인 Entry 20개. 이 결과는 원본 충실도 수정 전 결과이므로 현재 검증으로 취급하지 않는다.
- 의미 Grouping 수정 후 최신 Editor Assembly 정적 빌드: 오류 0개, 기존 경고 156개.
- 현재 Unity 재실행: 사용자 실행 대기.
- Config, SO 에셋, 레거시 에셋, 프리팹, Scene, 게임플레이 런타임 동작은 변경하지 않았다.
