# Relic Item SO JSON Generate Prompt

유물 기획을 아이템 및 Effect Entry가 포함된 정규화 RelicSO 입력 JSON으로
변환할 때 사용합니다. Unity asset은 생성하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 입력과 가이드를 기준으로 유물 아이템 1개의 RelicSO 입력 JSON을 생성해줘. 유물은 아이템이며, gameplay behavior는 current effectEntries 안의 EffectSO와 EffectEntrySO로 표현해. 기존 유물 asset의 구형 effects/applyType 구조는 비교에만 사용하고 복사하지 마.

Input:
- projectRoot: {project_root}
- relicDesignFile: {projectRoot_기준_승인된_유물_기획_상대경로 | null}
- relicDesign: {인라인_승인된_유물_기획 | null}
- relicSlug: {lowercase_snake_case_slug}
- relicJsonRoot: Assets/Resources/item/json
- legacyRelicRoot: Assets/Resources/shop/relic
- allowOverwrite: false
- outputJsonPath: Assets/Resources/item/json/item.relic.{relicSlug}.json

relicDesignFile과 relicDesign 중 정확히 하나를 제공해야 한다. 기획에는 이름, 설명, 희귀도, 역할, 아이콘, 테마 색상, category/subCategory, 공개 플래그와 각 Effect의 semantic slug, trigger/target intent, exact value/unit/chance/duration/apply count가 있어야 한다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemSO.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemRulesGuide.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemJsonGuide.md
- Assets/character_concepts/game_prompt_guide/effect/EffectSO.md
- Assets/character_concepts/game_prompt_guide/effect/EffectEntrySO.md
- Assets/character_concepts/game_prompt_guide/character/StatEnum.md

작업:
1. 기획에서 item identity, presentation, rarity, classification, visibility와 Effect 동작을 분리해 추출한다.
2. relicJsonRoot의 신규 JSON과 legacyRelicRoot의 기존 RelicSO/Effect asset을 읽어 ID와 동작 중복을 비교한다. 레거시는 비교 전용이다.
3. `relicId=item.relic.{relicSlug}`와 `{relicId}.json` 파일명을 확정한다.
4. 각 Effect에 숫자 인덱스가 아닌 semantic effectSlug를 지정하고 Effect/Entry ID를 relicId에서 파생한다.
5. 각 동작을 현재 지원되는 EffectType과 정확한 config로 변환한다. 지원되지 않는 trigger, target, stack, effect는 임의의 유사 효과로 바꾸지 않고 중단한다.
6. 장착 중 지속 효과는 기본적으로 Manual/duration=0으로 작성하고, 공격 또는 회복 trigger는 legacy applyType이 아니라 지원되는 ChanceOnHit/ChanceOnHeal/AttackBleed EffectType으로 표현한다.
7. Effect Entry마다 하나의 complete Effect object, lifetimeType, categoryType, duration, maxApplyCount와 override 필드를 작성한다.
8. descriptionKo의 모든 대상, 수치, 단위, 확률, 지속시간과 효과 개수가 effectEntries와 정확히 일치하는지 확인한다.
9. icon은 기존 Sprite의 정확한 이름인지 확인하고 themeColor RGBA가 0..1인지 검증한다.
10. allowOverwrite=false이고 outputJsonPath가 존재하면 수정하지 않는다.
11. 유효한 RelicSO 입력 JSON 하나만 outputJsonPath에 저장한다.
12. Unity asset, meta, localization, 아이콘, RelicPool, ShopProduct, 보상 JSON, 스킬 JSON을 생성하거나 수정하지 않는다.

`nameKo`와 `descriptionKo`는 JSON authoring 및 localization 생성 단계의 입력이다. JSON에는 포함하지만 `RelicSO`의 serialized field로 간주하지 않는다.

Output:
- Relic ID:
- Source Design:
- Compared Legacy Relics:
- Rarity / Category / SubCategory:
- Effect Summary:
- Generated Effect / Entry IDs:
- Icon / Theme Color:
- Output JSON Path:
- JSON Validation: Pass / Fail
- Description Consistency: Pass / Fail
- Duplicate Check: Pass / Fail
- Legacy Field Check: Pass / Fail
- Unity Asset Generation: not_performed
- Result: Pass / Fail
- Notes:

검증:
- outputJsonPath는 정확히 `Assets/Resources/item/json/item.relic.{relicSlug}.json`이어야 한다.
- relicId는 `item.relic.{relicSlug}`이고 파일명은 `{relicId}.json`이어야 한다.
- rarity는 Common, Rare, Epic, Legendary 중 하나여야 한다.
- effectEntries가 하나 이상이어야 하고 각 entry는 complete effect object 하나만 가져야 한다.
- Effect/Entry ID는 semantic slug를 사용하고 relicId에서 파생되어야 한다.
- effects, applyType, skill-owned field, asset GUID/path가 없어야 한다.
- Manual/Instant duration은 0이고 Timed/CombatTimed duration은 0보다 커야 한다.
- maxApplyCount는 0보다 커야 한다.
- 모든 enum과 Effect config는 현재 코드와 가이드의 필드여야 한다.
- StatModifier config는 current builder 필드인 `statType`을 사용하고 obsolete `targetStat`을 사용하지 않아야 한다.
- nameKo와 descriptionKo는 JSON 입력에는 존재하되 RelicSO 직렬화 필드로 처리하지 않아야 한다.
- JSON 외 파일을 생성하거나 수정하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_relic_design
  - invalid_relic_slug
  - duplicate_relic_id
  - duplicate_relic_behavior_without_variant_reason
  - missing_effect_design
  - invalid_effect_slug
  - duplicate_effect_id
  - unsupported_relic_behavior
  - unsupported_effect_type
  - unsupported_targeting_rule
  - invalid_effect_schema
  - invalid_effect_lifetime
  - invalid_effect_value
  - invalid_rarity
  - missing_icon_sprite
  - invalid_theme_color
  - description_effect_mismatch
  - existing_relic_requires_approval
  - output_write_failed
- 실패 원인
- 부족하거나 충돌한 입력
- 비교한 기존 유물
- 생성하지 않은 파일
- 다음에 필요한 결정 또는 런타임 지원

주의:
- 유물은 아이템이며 EquipmentSkill JSON을 생성하지 않는다.
- 기존 asset의 effects/applyType/숫자 effect suffix는 신규 JSON 템플릿이 아니다.
- 현재 런타임에서 확인되지 않은 owner/party/enemy target 규칙을 설명만 보고 임의 구현하지 않는다.
- JSON 생성 성공과 Unity RelicSO/EffectSO/EffectEntrySO asset 생성은 별도 단계다.
```

## Required Implementation Mapping Gate

Relic SO JSON generation requires both an approved planning document and a
separate approved implementation mapping/spec. Planning prose alone is not
sufficient input.

The mapping/spec must include:

- `iconSpriteName`, `themeColor`, `category`, `subCategory`, `hidden`,
  `developerOnly`;
- one semantic effect slug per gameplay behavior;
- supported `effectType` and exact current config fields;
- `lifetimeType`, `categoryType`, `duration`, `maxApplyCount`;
- traceability to the planning effect sentence or approved decision.

If the mapping/spec is missing, stop with `missing_implementation_mapping`.
If it requests unsupported runtime behavior, stop with
`unsupported_relic_behavior`. Do not infer target filters, range, duration,
stacking, or EffectSO config from planning prose alone.
