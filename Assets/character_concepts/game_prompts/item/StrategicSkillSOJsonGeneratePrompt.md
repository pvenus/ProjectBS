# Strategic Skill SO JSON Generate Prompt

전략 아이템 사용 시 실행할 스킬을 독립 JSON으로 생성할 때 사용합니다.
아이템 JSON이나 아이템 SO는 생성하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 입력과 가이드를 기준으로 전략 스킬 1개의 독립 EquipmentSkill JSON을 생성해줘. 기존 전략 item.json 안의 skill 객체는 동작 비교에만 사용하고 출력 구조로 복사하지 마.

Input:
- projectRoot: {project_root}
- strategicSkillRoot: Assets/Resources/skill/json
- legacyStrategicItemRoot: Assets/Resources/shop/strategic/so
- strategicSkillPlanningFile: {Assets/Doc/StrategicSkill/skill.strategic.{skillSlug}.planning.json | null}
- strategicSkillDesignFile: {projectRoot_기준_전략_스킬_기획_JSON_또는_문서_상대경로 | null}
- strategicSkillDesign: {인라인_전략_스킬_기획 | null}
- skillSlug: {lowercase_snake_case_slug}
- allowOverwrite: false
- outputJsonPath: Assets/Resources/skill/json/skill.strategic.{skillSlug}.json

strategicSkillPlanningFile, strategicSkillDesignFile, strategicSkillDesign 중 정확히 하나를 제공해야 한다. planning file은 하나의 skill 객체만 포함해야 하며 파일명, skill.skillId, 요청 skillId가 일치하고 skill.reviewStatus가 review_ready인지 먼저 확인한다. 기획 문서에는 평문으로 작성된 대상, 범위, 효과, 수치, 지속시간, 반복 횟수와 표현 방향이 포함되어야 한다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/strategic/StrategicSkillPlanningGuide.md
- Assets/character_concepts/game_prompt_guide/skill/strategic/StrategicSkillRulesGuide.md
- Assets/character_concepts/game_prompt_guide/skill/strategic/StrategicSkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
- Assets/character_concepts/game_prompt_guide/effect/EffectSO.md
- Assets/character_concepts/game_prompt_guide/effect/EffectEntrySO.md

작업:
1. planning file이면 단일 skill 객체의 tacticalRoleKo, conceptKo, intendedUseKo, targetingKo, effectsKo, executionKo, presentationKo, openQuestionsKo를 읽는다.
2. reviewStatus가 needs_decision 또는 blocked이면 openQuestionsKo를 임의로 해결하지 않고 중단한다.
3. 평문 기획에서 정확한 대상, 범위, 수치, 지속시간, 반복 횟수, 피해, 강화·약화·제어 효과를 추출한다.
4. 추출한 기획을 StrategicSkillJsonGuide와 세부 SO 가이드에 따라 baseProfile, cast, move, hits, damage, effect config로 변환한다. 기획 문서에 SO 필드가 없다는 이유로 기존 embedded skill을 복사하지 않는다.
5. strategicSkillRoot의 standalone 전략 스킬 JSON과 legacyStrategicItemRoot의 embedded skill을 읽어 ID 및 동작 중복을 비교한다. 레거시 데이터는 비교 전용이며 출력 구조로 복사하지 않는다.
6. `equipmentId=skill.strategic.{skillSlug}`를 확정하고 child ID를 여기서 파생한다.
7. 현재 전략 아이템 사용 서비스에 맞춰 `cast.targetingType=Position`을 사용한다.
8. Enemy 또는 Party 적용 대상은 hit의 `targetLayerMask`로 표현한다.
9. 최신 `baseProfile`, `cast`, 필요한 `move`, `hits`, 필수 `baseVisual`로 독립 EquipmentSkill JSON을 작성한다.
10. 피해는 현재 damage schema, 효과는 현재 EffectSO config와 EffectEntrySO schema를 사용한다.
11. 효과 ID는 `effect.skill.strategic.{skillSlug}.{effect_slug}`에서 파생한다.
12. 레거시 `profileId`, `visualSet`, `LowestHpAlly`, 평탄화된 `Taunt` 객체, `slotName`을 복사하지 않는다. 도발은 현재 `effectType=Taunt`, 빈 config, `categoryType=Debuff`, `lifetimeType=Instant`, 양수인 EffectEntry duration으로 재작성한다.
13. 기획에 없는 구현 수치나 동작을 임의로 추가하지 않는다. 필요한 값이 없으면 중단한다.
14. allowOverwrite=false이고 outputJsonPath가 존재하면 수정하지 않는다.
15. 유효한 스킬 JSON 하나만 outputJsonPath에 저장한다.
16. 전략 아이템 item.json, StrategicSkillItemSO, 아이콘, 애니메이션, 프리팹, localization을 생성하거나 수정하지 않는다.

Output:
- Strategic Skill ID:
- Planning Source / Status:
- Source Design:
- Compared Skills:
- Primary / Secondary Intent:
- Target / Radius / Duration:
- Damage / Effects:
- Generated Child IDs:
- Output JSON Path:
- JSON Validation: Pass / Fail
- Legacy Field Check: Pass / Fail
- Duplicate Check: Pass / Fail
- Item JSON Generation: not_performed
- Unity Asset Generation: not_performed
- Result: Pass / Fail
- Notes:

검증:
- 출력 루트는 `Assets/Resources/skill/json`이어야 한다.
- 파일명은 전체 equipmentId와 같은 `skill.strategic.{skillSlug}.json`이어야 한다.
- 최상위 `equipmentId`는 `skill.strategic.{skillSlug}`여야 한다.
- 최상위에 item ID, gaugeCost, reusable, price, item icon, tags 또는 `skill` wrapper가 없어야 한다.
- child ID와 effect ID는 정확한 equipmentId에서 파생되어야 한다.
- `baseProfileId`, `skillType`, `skillComponentType`, `baseVisual`을 사용해야 한다.
- `maxHitCount > 1 && deactivateAfterFirstHit == true` 충돌이 없어야 한다.
- Timed 또는 CombatTimed 효과의 duration은 0보다 커야 한다.
- 현재 enum과 builder schema만 사용해야 한다.
- item JSON 및 JSON 외 파일을 생성하거나 수정하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_strategic_skill_design
  - planning_skill_not_found
  - planning_needs_decision
  - planning_blocked
  - insufficient_balance_input
  - invalid_skill_slug
  - duplicate_skill_id
  - duplicate_behavior_without_variant_reason
  - unsupported_targeting_type
  - unsupported_effect_type
  - invalid_effect_schema
  - invalid_skill_schema
  - existing_skill_requires_approval
  - output_write_failed
- 실패 원인
- 부족하거나 충돌한 입력
- 비교한 기존 전략 스킬
- 생성하지 않은 파일
- 다음에 필요한 작업

주의:
- 기존 item.json의 embedded skill은 최신 스키마 원본이 아니다.
- 스킬 JSON 생성 성공과 Unity SO 생성은 서로 다른 단계다.
- 이 프롬프트는 아이템 JSON을 생성하거나 수정하지 않는다.
```
