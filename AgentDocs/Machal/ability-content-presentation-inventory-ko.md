# Ability 콘텐츠 Presentation 소스 인벤토리

## 상태

- 조사일: 2026-08-08
- 단계: 1단계 완료
- 4단계 검증 갱신: 2026-08-11, Synthetic Config 매핑 13종 및 승인 EffectEntry 에셋 20개 통과
- 범위: 현행 Skill 및 Effect 소스 코드와 승인 에셋 경로의 읽기 전용 조사
- 변경한 런타임 코드, 에셋, 프리팹, Scene, ProjectSettings: 없음

## 권위와 소스 우선순위

활성 게임플레이 데이터로 표시할 수 있는지 판단할 때 다음 순서를 사용한다.

1. 현행 런타임 Resolver 동작
2. 승인된 `EquipmentSkillSO`에서 참조로 도달 가능한 현행 SO
3. 현행 생성 SO 필드
4. 작성 전용 출처로 표시한 JSON
5. 의미가 불분명한 레거시 데이터의 작성된 설명

생성 SO에 값이나 참조가 없으면 JSON에만 있는 값을 활성 런타임 값으로 표시하지 않는다.

## 승인 경로

- `Assets/Resources/skill/character/generated/`
- `Assets/Resources/skill/json/`

활성 Task 계약에 적힌 Skill 및 Effect 소스 경로도 모두 존재한다. 조사한 Skill/Effect 소스와 승인 에셋 경로는 Git 기준선에서 깨끗했다. 전체 체크아웃에는 관련 없는 수정 및 미추적 파일이 많이 존재하므로 건드리지 않는다.

## 누락된 필수 작업 문서

- `AgentDocs/code-writing-rules.md`
- `AgentDocs/task-start-documentation-prompt.md`

1단계는 문서 전용이라 스크립트 수정이 필요하지 않았다. 2단계 코드 작업은 코드 작성 가이드가 복구되거나 제공되기 전에는 시작하지 않는다. 누락된 Prompt가 생기기 전에는 문서 관리 인계에 지정된 요청 형식을 사용할 수 없다.

## 현행 Skill 소스 구조

```text
EquipmentSkillSO
|- EquipmentBaseProfileSO
|- SkillCastSO
|  `- self EffectEntrySO[]
|- SkillHitSO[]
|  |- damage profile
|  |- buff EffectEntrySO[]
|  |- debuff EffectEntrySO[]
|  `- nested EquipmentSkillSO
|- SkillMoveSO
|- SpawnSkillSO
|- EquipmentUpgradeTableSO
`- BaseVisualSO

EquipmentSkillResolver
`- EquipmentSkillRuntimeData
   |- resolved level and range
   |- resolved burst count and interval
   |- resolved projectile count, spread, arrangement, and scale
   `- resolved upgrade and visual context
```

주요 소스 파일:

- `Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentSkillSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentBaseProfileSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/cast/SkillCastSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/hit/SkillHitSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/move/SkillMoveSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/spawn/SpawnSkillSO.cs`
- `Assets/Scripts/Ability/Skills/Runtime/EquipmentRuntimeData.cs`
- `Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs`

Presentation 출처는 직접 SO Preview 데이터와 Resolve된 Runtime 데이터를 구분해야 한다. 소스 구조는 중첩 Skill 참조를 지원하지만 승인 에셋에서는 Null이 아닌 중첩 Skill 참조를 찾지 못했다.

## 현행 Effect 소스 구조

```text
EffectEntrySO
|- EffectSO
|  `- EffectConfig serialized reference
|- lifetime and category
|- duration and max apply count
`- value override fields

EffectResolver
`- EffectRuntimeData selected by concrete EffectConfig type
```

주요 소스 파일:

- `Assets/Scripts/Ability/Effects/Definitions/EffectSO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectEntrySO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectEnum.cs`
- `Assets/Scripts/Ability/Effects/Definitions/config/`
- `Assets/Scripts/Ability/Effects/Resolvers/EffectResolver.cs`
- `Assets/Scripts/Ability/Effects/Runtime/config/`

런타임 확인 결과:

- `EffectResolver`는 현행 `EffectConfig` 클래스 13개를 모두 분기 처리한다.
- `EffectEntrySO.Duration`과 `MaxApplyCount`는 `EffectEntryRuntime`에 전달된다.
- `TauntEffectConfig` 자체에는 필드가 없고 Taunt 지속시간은 `EffectEntrySO.Duration`에서 온다.
- `EffectEntrySO.ValueOverride`는 현행 Resolver가 생성하는 Runtime 객체에 전달되지 않는다.
- `ResolveEntries`의 `effectUpgradeModifiers`, `defaultCategoryType` 매개변수는 현재 적용되지 않는다.
- 따라서 Presentation은 Override나 Upgrade Modifier를 활성 Resolve 값처럼 표시하면 안 된다.

## 승인 에셋 수

YAML 에셋 분류에 사용한 Script GUID:

- `EquipmentSkillSO`: `63226e07ba84a4a69967ef3b8995b8d7`
- `EffectSO`: `57a976b41e687441cad798047c5f5afc`
- `EffectEntrySO`: `ebac1672555d84d38bb2ad4ebe71d4ff`

| 경로 | JSON | EquipmentSkillSO | SkillHitSO | EffectSO | EffectEntrySO | 도달 가능한 EffectEntrySO |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `Assets/Resources/skill/character/generated/` | 44 | 38 | 31 | 2 | 2 | 0 |
| `Assets/Resources/skill/json/` | 20 | 20 | 20 | 18 | 18 | 18 |
| 합계 | 64 | 58 | 51 | 20 | 20 | 18 |

추가 참조 결과:

- Character Skill 38개 중 31개는 `SkillHitSO`가 있고 7개는 없다.
- Strategic Skill 20개는 모두 `SkillHitSO`가 있다.
- Character Hit 에셋에는 Null이 아닌 EffectEntry 참조가 하나도 없다.
- Strategic Hit 에셋은 16개에 Effect가 있고 4개에는 없다.
- 승인된 Cast 에셋에는 Null이 아닌 Self Effect 참조가 없다. Character Cast 에셋 9개에 Null `{fileID: 0}` 자리만 있으며 활성 Effect가 아니다.
- 승인된 Skill 또는 Hit 에셋에는 Null이 아닌 중첩 Skill 참조가 없다.

## 작성 JSON과 런타임 SO 비교

| 경로 | Effect 선언이 있는 JSON | Effect 선언 수 | 런타임에서 도달 가능한 EffectEntry |
| --- | ---: | ---: | ---: |
| Character generated 경로 | 21 | 27 | 0 |
| Strategic 경로 | 16 | 18 | 18 |

Character JSON에는 `StatModifier` 23개와 `Knockback` 4개가 선언되어 있지만 대응하는 현행 Hit SO는 EffectEntry를 참조하지 않는다. 이 값은 작성 데이터 근거일 뿐 활성 게임플레이 값으로 표시하면 안 된다.

Character JSON 6개는 대응하는 주 `EquipmentSkillSO` 에셋이 없다.

- `skill.character.military_officer.2.active_1.charge.json`
- `skill.character.military_officer.2.basic_attack.frontline_slash.json`
- `skill.character.military_officer.2.passive_1.unyielding_will.json`
- `skill.character.military_officer.3.active_1.charge.json`
- `skill.character.military_officer.3.basic_attack.frontline_slash.json`
- `skill.character.military_officer.3.passive_1.unyielding_will.json`

Character EffectEntry 에셋 2개는 어떤 승인 에셋에서도 참조하지 않는다.

- `Assets/Resources/skill/character/generated/effect.skill.character.door_shield_barricader.1.basic_attack.shield_bash.minor_knockback.entry.asset`
- `Assets/Resources/skill/character/generated/skill.military_officer.1.active_1.knockback.entry.asset`

이 Task에서 해당 파일을 복구, 삭제, 재연결, 마이그레이션하지 않는다.

## Effect Config 적용 범위

`승인 EffectSO`는 현행 Config가 직렬화된 수이고, `도달 가능`은 승인 Skill 에셋이 실제 참조하는 Entry 수다.

| EffectConfig | 승인 EffectSO | 도달 가능 | JSON 선언 | 계획된 정규화 결과 |
| --- | ---: | ---: | ---: | --- |
| `StatModifierEffectConfig` | 13 | 13 | 36 | `StatModifier` |
| `ChanceOnHitStatModifierEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + StatModifier` |
| `OnHitTimedStatModifierEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + StatModifier(duration)` |
| `ChanceOnHealStatModifierEffectConfig` | 0 | 0 | 0 | `Activation(OnHeal + Chance + Target) + StatModifier` |
| `HealEffectConfig` | 1 | 1 | 1 | `Heal` |
| `ChanceOnHealCooldownReduceEffectConfig` | 0 | 0 | 0 | `Activation(OnHeal + Chance + Target) + CooldownChange` |
| `CooldownReduceEffectConfig` | 1 | 1 | 1 | `CooldownChange` |
| `KnockbackEffectConfig` | 4 | 2 | 6 | `Displacement` |
| `OnHitKnockbackDistanceEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + Displacement` |
| `AttackBleedEffectConfig` | 0 | 0 | 0 | `Activation(OnAttack + Chance) + PeriodicDamage` |
| `OnHitPoisonDotEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + PeriodicDamage` |
| `ChanceOnHitSkillEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance + Critical requirement) + SkillInvoke` |
| `TauntEffectConfig` | 1 | 1 | 1 | `Control` |

Config 5종은 승인된 현행 EffectSO로 검증할 수 있다. 현행 소스가 지원하는 나머지 8종은 승인 에셋이 없으므로 현행 에셋이 생길 때까지 소스 수준 테스트만 가능하다.

4단계 검증 결과:

- Config 클래스 13종 모두 Synthetic 소스 수준 매핑 테스트를 통과했다.
- 승인 루트 아래 `EffectEntrySO` 에셋 20개가 모두 `Supported`로 해석됐다.
- 에셋이 있는 Config 5종은 현행 에셋으로 검증했다.
- 나머지 Config 8종은 구현 및 Synthetic 테스트를 완료했지만 에셋 수준 상태는 계속 대기다.
- 사용자 테스트 메뉴: `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`.

## 최초 검증 표본

- 도달 가능한 Effect 1개: `Assets/Resources/skill/json/skill.strategic.blackwind_bomb.asset`
- 도달 가능한 Effect 여러 개: `Assets/Resources/skill/json/skill.strategic.blood_meridian_release.asset`
- 도달 가능한 Effect 여러 개: `Assets/Resources/skill/json/skill.strategic.wind_demon_pull.asset`
- Effect 없음: `Assets/Resources/skill/json/skill.strategic.heavenfall_thunder.asset`
- JSON/SO 불일치: `Assets/Resources/skill/character/generated/skill.character.abandoned_shrine_wraith.2.active_1.lost_child_cry.json`과 해당 `.hit.asset`
- 승인 경로에는 중첩 Skill 검증 표본이 없다.
- 승인된 레거시 `SkillEffectSO` 표본이 없다. Fallback은 소스 수준 또는 테스트에서 메모리에 만든 객체로 검증해야 한다.

## 대기 콘텐츠 도메인

다음 현행 소스 정의는 존재한다.

- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessSO.cs`
- `Assets/Scripts/Collection/Relic/Definitions/RelicSO.cs`
- `Assets/Scripts/Actor/Character/so/CharacterSO.cs`

Adapter 검증에 승인된 현행 Character, Bless, Relic 에셋 경로는 아직 없다. 해당 Adapter는 7단계 대기 상태다. 레거시 Bless/Relic 에셋 경로는 계속 제외한다.

## 1단계 종료 결정

1단계는 완료됐다. 구현은 현행 런타임 SO 참조를 권위 있는 입력으로 사용하고 작성 전용 출처를 별도로 보존할 수 있다. `AgentDocs/code-writing-rules.md`가 복구되거나 제공될 때까지 2단계는 차단된다. 1단계에서는 코드 Placeholder나 런타임 파일을 만들지 않았다.
