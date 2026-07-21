# Relic Item SO JSON Generate Prompt

유물 기획을 아이템 및 Effect Entry가 포함된 정규화 RelicSO 입력 JSON으로
변환할 때 사용합니다. Unity asset은 생성하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 입력과 가이드를 기준으로 유물 아이템 1개의 normalized RelicSO input JSON을 생성한다. Unity asset은 생성하지 않는다. 기존 legacy RelicSO/effects/applyType 구조는 비교용으로만 읽고 복사하지 않는다.

Input:
- projectRoot: {project_root}
- relicDesignFile: {projectRoot 기준 승인된 유물 기획 문서 경로 | null}
- relicDesign: {inline 승인된 유물 기획 문서 | null}
- implementationMappingFile: {projectRoot 기준 승인된 implementation mapping/spec 경로 | null}
- implementationMapping: {inline 승인된 implementation mapping/spec | null}
- relicSlug: {lowercase_snake_case_slug}
- relicJsonRoot: Assets/Resources/item/json
- legacyRelicRoot: Assets/Resources/shop/relic
- allowOverwrite: false
- outputJsonPath: Assets/Resources/item/json/item.relic.{relicSlug}.json

Input contract:
- Provide exactly one planning input: `relicDesignFile` or `relicDesign`.
- Provide exactly one implementation mapping input: `implementationMappingFile` or `implementationMapping`.
- Planning input is pure design intent. It may contain name, description, rarity intent, role, trigger intent, target intent, effect prose, values, units, chance, duration intent, synergy, tradeoff, and open questions.
- Planning input must not be required to contain implementation construction fields such as icon sprite, theme color, category, subCategory, hidden, developerOnly, effect slug, EffectType, config fields, lifetimeType, categoryType, duration, or maxApplyCount.
- Implementation mapping/spec must provide implementation construction fields: `iconSpriteName`, `themeColor`, `category`, `subCategory`, `hidden`, `developerOnly`, one semantic effect slug per gameplay behavior, supported `effectType`, exact current config fields, `lifetimeType`, `categoryType`, `duration`, `maxApplyCount`, and traceability to the planning effect sentence or approved decision.

Reference guides:
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemSO.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemRulesGuide.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemJsonGuide.md
- Assets/character_concepts/game_prompt_guide/effect/EffectSO.md
- Assets/character_concepts/game_prompt_guide/effect/EffectEntrySO.md
- Assets/character_concepts/game_prompt_guide/character/StatEnum.md

Required Implementation Mapping Gate:
- Relic SO JSON generation requires both an approved planning document and a separate approved implementation mapping/spec. Planning prose alone is not sufficient input.
- If the mapping/spec is missing, stop with `missing_implementation_mapping`.
- If the mapping/spec is malformed, incomplete, untraceable to planning, or uses names/fields that do not match RelicItemJsonGuide, stop with `invalid_implementation_mapping`.
- If the mapping/spec requests unsupported runtime behavior, stop with `unsupported_relic_behavior`.
- Do not infer icon, theme color, category, flags, target filters, range, duration, stacking, EffectType, EffectSO config, lifetime, or maxApplyCount from planning prose alone.
- `ChanceOnHitStatModifier` and `AttackBleed` are unsupported for Relic SO generation in the current runtime because they need per-hit dynamic target state and safe removal semantics.

Procedure:
1. Read the approved planning input and extract only design facts: identity intent, nameKo, descriptionKo, rarity/design role, trigger intent, target intent, effect prose, values, units, chance, duration intent, synergy, tradeoff, and open questions.
2. Read the approved implementation mapping/spec and extract presentation/classification/visibility fields, semantic effect slugs, supported EffectType/config fields, lifetime/category/duration/maxApplyCount, and traceability links.
3. Validate that each mapping behavior traces to a planning effect sentence or approved decision without inventing missing gameplay choices.
4. Inventory current item JSON IDs and legacy relic IDs.
5. Compare behavior with existing relics and reject accidental duplicates. Legacy assets are comparison sources only.
6. Confirm `relicId=item.relic.{relicSlug}` and exact `{relicId}.json` output filename.
7. Derive Effect/Entry IDs from relicId and the mapping-provided semantic effect slugs.
8. Convert each mapped behavior to a supported current Effect type and exact config. Unsupported trigger, target, stack, duration, or effect behavior must fail rather than be approximated.
9. Add current Effect Entry lifetime and application metadata from the mapping/spec.
10. Verify descriptionKo against every mapped effect value, unit, chance, duration, target intent, and effect count.
11. Verify iconSpriteName, themeColor, rarity, category, subCategory, hidden, and developerOnly against RelicItemJsonGuide.
12. If allowOverwrite=false and outputJsonPath exists, fail without modifying it.
13. Write exactly one valid RelicSO input JSON to outputJsonPath.
14. Do not create or modify Unity asset, meta, localization, icon, RelicPool, ShopProduct, reward JSON, or skill JSON files.

`nameKo` and `descriptionKo` are JSON authoring/localization inputs. They may appear in the JSON input, but they are not direct serialized fields on `RelicSO`.

Output:
- Relic ID:
- Source Design:
- Source Implementation Mapping:
- Compared Legacy Relics:
- Rarity / Category / SubCategory:
- Effect Summary:
- Generated Effect / Entry IDs:
- Icon / Theme Color:
- Output JSON Path:
- JSON Validation: Pass / Fail
- Implementation Mapping Validation: Pass / Fail
- Description Consistency: Pass / Fail
- Duplicate Check: Pass / Fail
- Legacy Field Check: Pass / Fail
- Unity Asset Generation: not_performed
- Result: Pass / Fail
- Notes:

Validation:
- outputJsonPath must be exactly `Assets/Resources/item/json/item.relic.{relicSlug}.json`.
- relicId must be `item.relic.{relicSlug}` and file name must be `{relicId}.json`.
- rarity must be one of Common, Rare, Epic, Legendary.
- effectEntries must contain at least one current entry and each entry must contain exactly one complete effect object.
- Effect/Entry IDs must use semantic slugs from implementation mapping and derive from relicId.
- The JSON must not contain legacy `effects`, `applyType`, skill-owned fields, asset GUIDs, or direct asset paths.
- Manual/Instant duration must be 0; Timed/CombatTimed duration must be greater than 0.
- maxApplyCount must be greater than 0.
- All enum and Effect config names must match current code and guides.
- StatModifier config must use current builder field `statType`; obsolete `targetStat` must not be used.
- nameKo and descriptionKo may exist as JSON input/localization fields, but must not be treated as direct RelicSO serialized fields.
- No file other than the one JSON output may be created or modified.

Failure Output:
- status: failed
- failureType:
  - missing_relic_design
  - missing_implementation_mapping
  - invalid_implementation_mapping
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
- failure reason
- missing or conflicting input
- compared legacy relics
- files not generated
- required next decision or support path

Warnings:
- Relics are items; do not create EquipmentSkill JSON.
- Existing assets with legacy effects/applyType/numeric effect suffixes are not normalized JSON templates.
- Report unverified owner/party/enemy targeting rules instead of implementing or approximating them.
- JSON generation and Unity RelicSO/EffectSO/EffectEntrySO asset generation are separate steps.
```
