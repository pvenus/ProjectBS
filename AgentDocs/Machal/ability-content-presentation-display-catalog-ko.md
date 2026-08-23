# Ability 콘텐츠 Presentation 표시 카탈로그

## 상태

- 원본 문서 날짜: 2026-08-13
- 범위: 플레이어에게 표시하는 Character, Skill, Effect, Bless, Relic의 라벨, 토큰, 포맷, 필터 정책
- 원본 계약: 원본 필드 하나당 Entry 하나, Entry 하나당 Value 하나를 유지한다.
- 로컬라이징 계약: 이름과 설명은 기존 `StringManager` 경로를 유지하고, Group, Entry, Tag, 열거값 대체 단어, 값 포맷은 `name` sub-key를 가진 표준 `presentation.*` main key를 사용한다.
- 디버그 계약: 검사 출력은 원본 Key와 필터되지 않은 작성 값을 유지한다.

## 표시 파이프라인

```text
승인된 SO / Runtime 데이터
-> 도메인 Presentation Resolver
-> 원본 의미 Group, Entry, Tag, Token Key
-> 플레이어 표시 Allowlist와 조건부 값 필터
-> PresentationDisplayCatalog 로컬라이징 Key
-> PresentationLocalizedTextResolver.ResolveRequired(...)
-> StringManager.Get(..., returnNullIfMissing: true)로 후보를 순서대로 확인
-> 모두 실패하면 첫 번째 의도 Key와 Sub-key를 StringManager.Get으로 일반 조회
-> UIContentInfoView
```

카탈로그는 게임플레이 수치를 창작하지 않고 두 원본 필드를 하나의 Entry로 합치지 않는다. Catalog 매핑이 없는 미승인 원본 JSON/C# Key는 생략한다. 필드가 승인되어 매핑된 뒤 Localization 행이 누락되면 전체 의도 `mainKey.subKey`를 표시하여 결함을 드러낸다. `StringManager` 자체가 없을 때 사용하던 기존 에셋 이름 대체 경로는 변경하지 않는다.

## 누락 Key 경계

- 승인된 Catalog 매핑 없음: 플레이어 UI에서 원본 게임플레이 Key를 생략한다.
- 승인된 매핑 Key 또는 필수 이름/설명 Key에 대응하는 StringManager 행 없음: 첫 번째 의도 `mainKey.subKey` 전체를 표시한다.
- Main Key 후보가 여러 개임: `returnNullIfMissing: true`로 순서대로 확인하고 처음 해석된 텍스트를 사용한다. 모두 실패한 경우에만 첫 번째 의도 Key를 노출한다.
- 누락 Key를 임의 생성 Pascal Case 텍스트나 게임플레이 값에서 추론한 문장으로 대체하지 않는다.

## 로컬라이징 Key 계열

| 목적 | Key 계열 | 예시 |
| --- | --- | --- |
| Group 타이틀 | `presentation.group.*` | `presentation.group.special_effect` |
| Entry 라벨 | `presentation.entry.*` | `presentation.entry.effect_distance` |
| Tag 라벨 | `presentation.tag.*` | `presentation.tag.Active` |
| Enum/Token 대체 단어 | 문맥별 계열 | `presentation.damage.Normal`, `presentation.control.Stun` |
| 값 포맷 | `presentation.format.*` | `presentation.format.control_type` |
| Stat 대체 단어 | `presentation.stat.*` | `presentation.stat.StunDuration` |

모든 행은 `Assets/Resources/string/presentation_string.csv`에서 관리한다. `Skill.Hit.1.Damage` 같은 번호 Key를 만들지 않고, 플레이어 경로에서 Pascal Case를 임의 문장으로 바꾸지 않는다.

## Skill 이름과 Tag

| 원본 | 원본 Presentation 값 | 플레이어 정책 | 최종 텍스트 원본 |
| --- | --- | --- | --- |
| `equipmentId` | Identity ID | 검사/Provenance 전용 | 없음 |
| `skillName` / Unity 에셋 이름 | 원본 Identity 대체 경로 | 기존 동작만 유지 | 기존 Skill 이름 처리 |
| `LocalizationMainKey.name` | Identity 이름 | 로컬라이징 값이 있으면 항상 | 기존 `StringManager` 경로, 전략 Skill은 확인된 Item Key 대체 경로 유지 |
| `LocalizationMainKey.desc` | 설명 | 작성된 경우 | 기존 `StringManager` 경로 |
| `baseProfile.skillType` | `Active`, `Passive` | 플레이어 Tag | `presentation.tag.<value>` |
| `baseProfile.skillComponentType` | Component Token | 검사 전용 | 없음 |
| `baseProfile.brainMeta.category` | Category Token | 승인된 값이면 플레이어 Tag | `presentation.tag.<value>` |
| `baseProfile.brainMeta.targetType` | Target Token | 승인된 값이면 플레이어 Tag | `presentation.tag.<value>` |
| `baseProfile.brainMeta.tacticalNeed` | AI 판단 Token | 검사 전용 | 없음 |

`Assets/Resources/skill/json/`의 현행 20개 파일에는 `Active`, `Projectile`, `Attack/Buff/Control/Heal` Category, `Ally/Enemy` Target, `None/AllySupport/AreaControl` Tactical Need가 있다. 플레이어 Tag에는 Skill Type, Category, Target만 포함할 수 있다.

## Skill Entry

정책 용어:

- **표시**: 원본 개념이 있고 명시된 조건을 통과하면 포함한다.
- **조건부**: `ResolveForPlayerDisplay()`에서만 기본값, 0, 비활성, 무제한 Sentinel을 제외한다.
- **검사 전용**: `Resolve()`와 Editor 검사 도구에는 유지하지만 플레이어 View에는 전달하지 않는다.

| 원본 JSON/SO 필드 | 원본 Entry Key | Skill Group | 플레이어 정책 | Label Key |
| --- | --- | --- | --- | --- |
| `cast.targetingType` | `targetingType` | Activation | `None`이 아니면 표시 | `presentation.entry.targeting` |
| `cast.cooldown` | `cooldown` | Activation | 조건부: 0보다 큼 | `presentation.entry.cooldown` |
| `cast.castTime` | `castTime` | Activation | 조건부: 0보다 큼 | `presentation.entry.cast_time` |
| `cast.range` | `range` | Activation | 조건부: 사거리를 쓰는 Targeting, 양수, `999` 미만 | `presentation.entry.range` |
| `cast.burst.count` | `burst.count` | Delivery | 조건부: 1보다 큼 | `presentation.entry.burst_count` |
| `cast.burst.interval` | `burst.interval` | Delivery | 조건부: 양수이고 Burst Count가 1보다 큼 | `presentation.entry.burst_interval` |
| Cast 이동 유형 | `castMove.moveType` | Delivery | `None`이 아니면 표시 | `presentation.entry.cast_move_type` |
| Cast 이동 거리 | `castMove.distance` | Delivery | 조건부: 양수 | `presentation.entry.cast_move_distance` |
| Cast 이동 시간 | `castMove.duration` | Delivery | 조건부: 양수 | `presentation.entry.cast_move_duration` |
| `baseProfile.projectileCount` | `projectileCount` | Delivery | 조건부: 1보다 큼 | `presentation.entry.projectile_count` |
| `baseProfile.projectileScale` | `projectileScale` | Delivery | 검사 전용 | 없음 |
| `baseProfile.projectileColliderRadius` | `projectileColliderRadius` | Delivery | 조건부: 양수이고 `999` 미만 | `presentation.entry.effect_range` |
| `baseProfile.projectileLifetime` | `projectileLifetime` | Delivery | 조건부: 양수 | `presentation.entry.duration` |
| `baseProfile.projectile.arrangement` | `projectile.arrangement` | Delivery | Arrangement 데이터가 의미 있을 때 표시 | `presentation.entry.projectile_arrangement` |
| `baseProfile.projectile.arrangementValue` | `projectile.arrangementValue` | Delivery | 검사 전용 | 없음 |
| `baseProfile.projectile.spreadAngle` | `projectile.spreadAngle` | Delivery | 조건부: 양수 | `presentation.entry.spread_angle` |
| `baseProfile.projectile.radius` | `projectile.radius` | Delivery | 조건부: 양수 | `presentation.entry.arrangement_radius` |
| `baseProfile.projectileSpawn.spawnOffset` | `projectileSpawn.spawnOffset` | Delivery | 검사 전용 | 없음 |
| `baseProfile.projectileSpawn.interval` | `projectileSpawn.interval` | Delivery | 조건부: 양수 | `presentation.entry.projectile_spawn_interval` |
| `move.moveType` | `moveType` | Delivery | 검사 전용 | 없음 |
| `move.config.speed` | `config.speed` | Delivery | 검사 전용 | 없음 |
| `move.config.turnSpeed` | `config.turnSpeed` | Delivery | 검사 전용 | 없음 |
| Orbit/Follow 이동 Config | `config.orbitRadius`, `config.orbitAngularSpeed`, `config.clockwise`, `config.followOffset.x/y` | Delivery | 검사 전용 | 없음 |
| `hits.targetLayerMask` | `targetLayerMask` | Delivery | 검사 전용 | 없음 |
| `hits.damage.damageType` | `damage.damageType` | Outcome | 의미 있는 피해가 있을 때 표시 | `presentation.entry.damage_type` |
| `hits.damage.baseDamage` | `damage.baseDamage` | Outcome | 조건부: 양수 | `presentation.entry.base_damage` |
| 첫 적중 피해 SO 필드 | `damage.firstHitBaseDamage` | Outcome | 조건부: 양수, Runtime이 전달하지 않으면 Preview 전용 | `presentation.entry.first_hit_damage` |
| `hits.damage.attackPercentDamage` | `damage.attackPercentDamage` | Outcome | 조건부: 양수 | `presentation.entry.attack_scaling` |
| `hits.damage.canCritical` | `damage.canCritical` | Outcome | 조건부: `true`만 표시 | `presentation.entry.can_critical` |
| `hits.damage.ignoreDefense` | `damage.ignoreDefense` | Outcome | 조건부: `true`만 표시 | `presentation.entry.ignore_defense` |
| `hits.maxHitCount` | `maxHitCount` | Delivery | 조건부: 양수이고 `999` 미만 | `presentation.entry.max_hit_count` |
| `hits.hitStartTime` | `hitStartTime` | Delivery | 검사 전용 | 없음 |
| `hits.repeatInterval` | `repeatInterval` | Delivery | 조건부: 양수 | `presentation.entry.repeat_interval` |
| 분할 적중 횟수 | `split.hitCount` | Delivery | 조건부: 1보다 큼 | `presentation.entry.split_hit_count` |
| 분할 적중 간격 | `split.hitInterval` | Delivery | 조건부: 양수 | `presentation.entry.split_hit_interval` |
| Spawn Character 참조 | `character` | Outcome | 표시, 값은 기존 로컬라이징 Character 이름 | `presentation.entry.summoned_character` |
| Spawn 횟수 | `spawnCount` | Outcome | 조건부: 1보다 큼 | `presentation.entry.spawn_count` |
| Spawn 간격 | `spawnInterval` | Outcome | 조건부: 양수이고 Spawn Count가 1보다 큼 | `presentation.entry.spawn_interval` |
| Spawn 지속시간 | `spawnLifeTime` | Outcome | 조건부: 양수 | `presentation.entry.spawn_lifetime` |

현행 전략 Skill JSON에는 Cast 이동, 분할 적중, Spawn 필드가 없지만 현행 SO 정의는 이를 지원한다. 이 항목들은 원본 기반 지원 기능으로만 유지하고 현행 에셋에 합성하지 않는다.

## Effect Entry

7개 Typed Outcome은 내부 정규화 모델로 유지한다. Skill 조합은 Activation을 `Activation`, 일반 결과를 `Outcome`, Control/Displacement를 `SpecialEffect`, SkillInvoke를 `LinkedSkill`로 보낸다.

| 정규화 원본 필드 | 원본 Entry Key | 플레이어 정책 | Label/Token Key |
| --- | --- | --- | --- |
| 발동 Trigger | `Activation.Trigger` | 값이 있으면 표시 | `presentation.entry.activation_trigger`, `presentation.trigger.<value>` |
| 발동 확률 Ratio | `Activation.chance` | 값이 있으면 표시 | `presentation.entry.activation_chance` |
| 발동 확률 Percent | `Activation.chancePercent` | 값이 있으면 표시 | `presentation.entry.activation_chance` |
| 발동 Target | `Activation.Target` | 값이 있으면 표시 | `presentation.entry.activation_target`, `presentation.target.<value>` |
| 치명타 요구 | `Activation.RequiresCriticalHit` | true이면 표시 | `presentation.entry.critical_condition`, `presentation.boolean.True` |
| Stat 유형 | `StatModifier.Stat` | 표시 | `presentation.entry.stat`, `presentation.stat.<value>` |
| Stat 연산 | `StatModifier.Operation` | 표시 | `presentation.entry.operation`, `presentation.operation.<value>` |
| Stat 값 | `StatModifier.value` | 표시 | `presentation.entry.modifier_value` |
| 기간형 Stat 지속시간 | `StatModifier.durationSeconds` | 값이 있으면 표시 | `presentation.entry.duration` |
| 최대 체력 회복 비율 | `Heal.maxHpPercent` | 값이 있으면 표시 | `presentation.entry.max_health_ratio` |
| 고정 회복량 | `Heal.flatHealAmount` | 값이 있으면 표시 | `presentation.entry.heal_amount` |
| 공격력 회복 계수 | `Heal.attackPercentHeal` | 값이 있으면 표시 | `presentation.entry.attack_scaling` |
| 최대 체력 Clamp | `Heal.ClampToMaximumHealth` | 검사 전용 | 없음 |
| Cooldown 변경 유형 | `CooldownChange.Kind` | 표시 | `presentation.entry.cooldown_change_type`, `presentation.cooldown_change.<value>` |
| Cooldown 비율 | `CooldownChange.reducePercent` | 값이 있으면 표시 | `presentation.entry.cooldown_reduction_ratio` |
| Cooldown 시간 | `CooldownChange.reduceSeconds` | 값이 있으면 표시 | `presentation.entry.cooldown_reduction_time` |
| 강제 이동 방향 | `Displacement.Direction` | 표시 | `presentation.entry.displacement_type`, `presentation.displacement.<value>` |
| 강제 이동 Force | `Displacement.force` | 값이 있으면 표시 | `presentation.entry.effect_magnitude` |
| 강제 이동 거리 | `Displacement.distanceMeters` | 값이 있으면 표시 | `presentation.entry.effect_distance` |
| 지속 피해 공격력 비율 | `PeriodicDamage.attackRatioPercent` | 값이 있으면 표시 | `presentation.entry.attack_scaling` |
| 틱당 지속 피해 비율 | `PeriodicDamage.attackRatioPercentPerTick` | 값이 있으면 표시 | `presentation.entry.attack_scaling_per_tick` |
| 지속 피해 Rate Unit | `PeriodicDamage.RateUnit` | 표시 | `presentation.entry.periodic_rate`, `presentation.periodic_rate.<value>` |
| Tick 간격 | `PeriodicDamage.tickIntervalSeconds` | 값이 있으면 표시 | `presentation.entry.interval` |
| 지속 피해 시간 | `PeriodicDamage.durationSeconds` | 값이 있으면 표시 | `presentation.entry.duration` |
| 호출 Skill | `SkillInvoke.Skill` | 표시, 값은 기존 로컬라이징 Skill 이름 | `presentation.entry.linked_skill` |
| 호출 범위 | `SkillInvoke.Range` | 값이 있으면 표시 | `presentation.entry.effect_range` |
| Control 유형 | `Control.Kind` | 표시 | `presentation.entry.control_type`, `presentation.control.<value>` |
| Control 지속시간/값 | `Control.value` 또는 `Control.duration` | 값이 있으면 표시 | `presentation.entry.duration` |
| EffectEntry 지속시간 | `duration` | 의미 있을 때 표시 | `presentation.entry.duration` |
| 최대 적용 횟수 | `maxApplyCount` | 의미 있을 때 표시 | `presentation.entry.max_apply_count` |
| Category, Lifetime, Status | `categoryType`, `lifetimeType`, `status` | 검사 전용 | 없음 |

`ValueOverride`, `hasValueOverride`, 현재 Runtime Resolver가 적용하지 않는 Upgrade Modifier, Runtime에서 소비되지 않는 값은 정규화된 플레이어 데이터에서 계속 제외한다.

## Damage, Control, Displacement 포맷

플레이어 View에는 원본 Enum을 직접 표시하지 않는다. Entry가 대체 Key 계열과 포맷 Key를 함께 선택한다.

| Entry | 대체 Key | 포맷 Key | 현재 한국어 예시 |
| --- | --- | --- | --- |
| `damage.damageType` | `presentation.damage.<DamageType>` | `presentation.format.damage_type` | 일반 피해, 폭발 피해, 지속 피해, 고정 피해 |
| `Control.Kind` | `presentation.control.<EffectControlKind>` | `presentation.format.control_type` | 기절, 속박, 도발 |
| `Displacement.Direction` | `presentation.displacement.<EffectDisplacementDirection>` | `presentation.format.displacement_type` | 밀쳐내기, 끌어당기기, 투사체 방향 이동, 지정 방향 이동 |

UI가 로컬라이징된 Entry 라벨을 별도로 그리므로 현재 포맷 행은 `{0}`이다. CSV 포맷을 바꾸면 코드나 원본 데이터를 변경하지 않고도 대체 텍스트를 감싸거나 꾸밀 수 있다.

## Character 표시 목록

현재 승인된 작성 원본은 `Assets/Resources/character/json/` 아래 JSON 22개다.

| JSON/SO 원본 | 플레이어 정책 | Localization/출력 |
| --- | --- | --- |
| `name` / `LocalizationMainKey` | 표시 | `character_string.csv`의 기존 `characterId` + `name` StringManager 경로 |
| `characterType` | Tag로 표시 | `presentation.tag.<CharacterType>` |
| `job` | 원본 기반 Tag 하나로 표시 | `presentation.tag.<CharacterJob>`; Family/Tier/Branch를 별도 생성한 Row로 대체하지 않는다 |
| `baseStats[].statType` + `value` | 원본 Stat마다 Entry 하나로 표시 | `presentation.stat.<StatType>` 라벨과 원본 숫자 |
| `characterId` | Identity/Provenance 전용 | 플레이어 Entry나 Tag로 렌더링하지 않음 |
| 생성된 Animation Clip 참조 | 검사 전용 | 플레이어 라벨 없음 |
| 생성된 Skill 참조와 `slotKey` | 검사 및 Skill 탭 조합 전용 | Character 정보 본문에 중복 표시하지 않음 |
| 파생 Job Family/Tier/Branch | 검사 전용 | 작성 JSON이 `job` 필드 하나를 가지므로 플레이어 라벨 없음 |

현재 Character JSON Stat은 `Attack`, `Defense`, `MaxHp`, `AttackSpeed`, `CritChance`, `CritDamage`, `MoveSpeed`다. Runtime 근거에 따라 `CritChance`와 `CritDamage`는 Percent, `MoveSpeed`는 m/s, `AttackSpeed`는 배율로 표시한다. 배율 포맷은 `presentation.format.multiplier`로 Localization하며 원본 숫자는 바꾸지 않는다.

`Assets/Editor/tools/character/CharacterPresentationPreviewWindow.cs` 비교 도구는 `Original JSON`, `SO Inspection (all)`, `Player UI (filtered)`를 나란히 보여준다. ID, Type, Job, 정렬된 Stat, 숫자 값, StringManager 기반 이름의 JSON/SO 불일치도 보고한다.

## Owned Effect 인벤토리 표시 목록

획득한 모든 일반 Bless와 보유한 모든 Relic은 즉시 적용된다. 권위 있는 기획에서 둘 다 장착 상태가 없다. 인벤토리에는 활성 Bless 기반 Faith 기능도 표시할 수 있지만 진행과 미래 해금 정보는 별도 Faith 도감이 소유한다.

| 콘텐츠/원본 | 플레이어 정책 | 로컬라이징/출력 |
| --- | --- | --- |
| Bless/Relic 이름과 설명 | 기존 동작 유지 | 기존 `LocalizationMainKey`를 `StringManager`로 처리 |
| Bless Category, Duration Type, God | 카탈로그에 있으면 플레이어 Tag | `presentation.tag.<value>` |
| 원본 Bless Authoring Tag | 카탈로그가 명시적으로 승인한 값만 | 그 외는 제외 |
| Bless 전투 지속 횟수 | 표시 | `presentation.entry.bless_duration_battles` |
| Bless Runtime Level과 남은 전투 | 값이 있으면 표시 | `presentation.entry.bless_level`, `presentation.entry.remaining_battles` |
| Bless Runtime `isEquipped` / `isSelected` | 오래됐거나 권위 없는 Runtime 필드, 검사 전용 | 장착 UI를 만들지 않음 |
| Relic Rarity | 플레이어 Tag | `presentation.tag.<value>` |
| Relic 원본 Category/Subcategory | 제품 표시 어휘가 승인될 때까지 검사 전용 | 없음 |
| Relic 보유 상태 | 보유 목록 포함 여부 결정 | 보유한 모든 Relic이 활성 상태 |
| `EquippedRelics` / Relic 장착 상태 | 현행 Runtime 불일치, 검사 전용 | 플레이어 표시를 장착 상태로 필터링하지 않음 |
| 활성 Bless 기반 Faith 기능 | 보유 효과의 신앙 축복 섹션에 포함 | Bless 플레이어 표시 경로로 해석 |
| 비활성 또는 미래 Faith 기능 | Faith 도감 Preview 전용 | Owned Effects에서 제외 |
| Exclusive Job Change | Faith 도감 전용 | 향후 명시적인 Effect 원본이 작성되지 않으면 제외 |
| Bless/Relic Effect | 동일한 정규화 Effect 카탈로그 사용 | 위 Key 사용 |

현재 화면 역할은 탭이 없는 보유 효과 인벤토리 하나다. 세로 Scroll 하나에 현재 적용 중인 보유 유물, 획득한 일반 축복, 활성 신앙 축복의 카테고리 섹션을 배치하며 이 페이지는 `Catalog`를 사용하지 않는다. 유물 도감과 일반 축복 도감은 별도 페이지로 구성하고 공통 카테고리/아이템 표시를 `Catalog` 모드로 재사용할 수 있다. 신앙 도감은 전체 신앙 진행과 향후 해금을 소유한다. 모든 보유 효과 선택은 중립 `UIContentInfoView` 하나에 연결하며 또 다른 보유 전용 축복 페이지나 중복 Faith 진행 Track을 만들지 않는다.

별도 일반 축복 도감은 아직 계획된 `Catalog` 페이지이며 `GeneralBlessCatalogPresenter`는 존재하지 않는다. 승인된 계약은 명시적인 목록 또는 `BlessPoolSO.Blessings`로 전달된 null이 아닌 고유 `BlessSO`를 모두 표시하고, 활성 `BlessRuntimeData.BlessEntry` 목록에 같은 작성 `BlessingId`가 있는 정의만 활성 표시하며, 비활성 정의도 계속 표시하고 선택할 수 있게 하는 것이다. Pool의 weight와 progressionStep은 검사/생성 데이터로 유지하고 표시하지 않는다.

`RelicCollectionView`는 획득/미획득 항목, 잠금 실루엣, 보유/전체 개수를 제공하는 향후 Relic 도감용으로 분리 유지한다. 이 도감 View를 Owned Effect 인벤토리로 재사용하거나 일반화하지 않는다.

구현 전에 현행 Runtime 소유권을 정리해야 한다. `RelicItemService.EquippedRelics`는 Relic에 장착 상태가 없다는 기획과 충돌하고, 현재 `BlessManager.AddBless`는 기존 영구 Common Bless를 교체한다. 이 표시 카탈로그는 구형 Bless/Relic 에셋을 마이그레이션하거나 수정하지 않는다.

## 유지보수 규칙

새로 표시할 원본 필드나 Enum 값이 추가되면 다음 순서를 따른다.

1. 현행 Runtime/SO 데이터에 실제 필드가 있는지 확인하고 Provenance를 기록한다.
2. 번호나 파생 값을 만들지 말고 원본 Entry/Tag Key 하나를 추가한다.
3. 플레이어 표시, 조건부, 검사 전용 정책을 명시적으로 결정한다.
4. `PresentationDisplayCatalog`에 표준 로컬라이징 매핑을 추가한다.
5. 필요한 모든 `presentation.*` 행을 `presentation_string.csv`에 추가한다.
6. 검증을 확장하고 사용자 담당 Unity 테스트를 다시 실행한다.

플레이어 View에서는 임의 생성 단어로 조용히 대체하지 않는다. Catalog 매핑 누락은 미승인 원본 필드를 생략한다. 승인된 매핑 또는 필수 이름/설명의 StringManager 행 누락은 검증 실패이며 수정될 때까지 의도한 전체 Localization Key로 표시한다.
