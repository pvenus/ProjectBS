# Strategic Item SO JSON Generate Prompt

사용 시 연결된 스킬이 실행되는 전략 아이템의 정규화된 `item.json`을 생성할 때 사용합니다. 아이템이 최상위 개념이고 스킬은 아이템의 실행 데이터입니다. 이 프롬프트는 Unity SO asset을 생성하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 입력과 가이드를 기준으로 전략 아이템 1개의 item.json을 생성해줘. 아이템은 item 속성과 skillId만 가지며, 연결 스킬 JSON은 생성하거나 item.json 안에 포함하지 마.

Input:
- projectRoot: {project_root}
- strategicItemRoot: Assets/Resources/item/json
- legacyStrategicItemRoot: Assets/Resources/shop/strategic/so
- strategicItemDesignFile: {projectRoot_기준_전략_아이템_기획_JSON_또는_문서_상대경로 | null}
- strategicItemDesign: {인라인_기획_객체 | null}
- itemSlug: {lowercase_snake_case_slug}
- skillId: {skill.strategic.lowercase_snake_case_slug}
- linkedSkillJsonPath: {projectRoot_기준_별도_생성된_전략_스킬_JSON_상대경로}
- iconSpriteName: {현재_프로젝트에_존재하는_정확한_Sprite명}
- grade: {Basic | Advanced}
- gaugeCost: {0..100}
- defaultPrice: {0_이상}
- reusable: true
- tags: {lowercase_tag_array | auto_from_design}
- allowOverwrite: false
- outputJsonPath: Assets/Resources/item/json/item.strategic.{itemSlug}.json

strategicItemDesignFile과 strategicItemDesign 중 정확히 하나는 제공되어야 한다. 기획에는 아이템 이름, 설명, 등급, 게이지 비용 근거, 가격, 태그와 아이콘이 포함되어야 한다. linkedSkillJsonPath는 설명 및 ID 검증에만 사용한다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/item/strategic/StrategicSkillItemSO.md
- Assets/character_concepts/game_prompt_guide/item/strategic/StrategicItemRulesGuide.md
- Assets/character_concepts/game_prompt_guide/item/strategic/StrategicItemJsonGuide.md

작업:
1. 아이템 기획에서 이름, 설명, 등급, 게이지 비용, 가격, 태그와 아이콘을 추출한다.
2. linkedSkillJsonPath를 읽고 최상위 equipmentId가 입력 skillId와 정확히 같은지 확인한다.
3. linked skill의 대상, 범위, 피해, 개수, 지속시간과 효과를 읽어 item descriptionKo가 일치하는지 검증한다.
4. strategicItemRoot의 신규 전략 아이템 JSON과 legacyStrategicItemRoot의 레거시 item.json에서 item ID, skillId, 비용, 가격, 등급과 태그를 비교한다. 레거시 데이터는 비교 전용이며 신규 출력 구조로 복사하지 않는다.
5. `strategicSkillItemId=item.strategic.{itemSlug}`를 확정한다.
6. itemSlug를 독립적으로 검증하며 skillId에서 item ID를 재생성하지 않는다.
7. grade, gaugeCost, defaultPrice가 입력과 기획에서 충돌하면 중단한다.
8. reusable은 현재 런타임 제약에 따라 true인지 확인한다.
9. iconSpriteName이 정확한 기존 Sprite 이름인지 확인한다.
10. item.json에는 item 속성과 skillId만 작성하고 `skill`, `baseProfile`, `cast`, `move`, `hits`, `damage`, `effects`, `baseVisual`을 넣지 않는다.
11. allowOverwrite=false이고 outputJsonPath가 이미 존재하면 수정하지 않는다.
12. 유효한 item.json 하나만 저장하고 스킬 JSON 및 JSON 외 파일을 생성하거나 수정하지 않는다.

Output:
- Strategic Item ID:
- Strategic Skill ID:
- Source Design:
- Compared Existing Items:
- Gauge Cost / Price / Grade:
- Icon Sprite:
- Linked Skill JSON:
- Linked Skill ID Check: Pass / Fail
- Description Consistency: Pass / Fail
- Output JSON Path:
- JSON Validation: Pass / Fail
- Duplicate Check: Pass / Fail
- Linked Skill Resource: Ready / Blocked
- Localization Compatibility: Ready / Blocked
- Skill JSON Generation: not_performed
- Unity Asset Generation: not_performed
- Result: Pass / Fail
- Notes:

검증:
- JSON 문법이 유효해야 한다.
- outputJsonPath는 정확히 `strategicItemRoot/item.strategic.{itemSlug}.json`이어야 한다.
- itemSlug는 lowercase snake_case여야 한다.
- strategicSkillItemId는 `item.strategic.{itemSlug}`여야 한다.
- skillId는 linked skill JSON의 equipmentId와 동일해야 한다.
- nested `skill` 객체와 모든 skill-owned field가 없어야 한다.
- gaugeCost는 0..100이고 defaultPrice는 0 이상이어야 한다.
- reusable은 true여야 한다.
- descriptionKo의 모든 효과와 수치가 linked skill JSON과 일치해야 한다.
- 아이콘 Sprite가 정확히 존재해야 한다.
- JSON 외 파일을 생성하거나 수정하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_strategic_item_design
  - invalid_item_slug
  - duplicate_item_id
  - unsupported_grade
  - conflicting_design_input
  - invalid_gauge_cost
  - unsupported_reusable_false
  - missing_icon_sprite
  - missing_linked_skill_json
  - missing_linked_skill_resource
  - duplicate_linked_skill_resource
  - linked_skill_id_mismatch
  - linked_skill_description_mismatch
  - embedded_skill_object_forbidden
  - existing_item_requires_approval
  - output_write_failed
- 실패 원인
- 부족하거나 충돌한 입력
- 비교한 기존 전략 아이템
- 생성하지 않은 파일
- 다음에 필요한 작업

주의:
- 기존 전략 item.json의 embedded skill을 신규 item schema로 복사하지 않는다.
- 누락된 스킬은 이 프롬프트에서 생성하지 않는다. 전략 스킬 생성 프롬프트를 별도로 실행한다.
- item JSON 생성 성공과 linked EquipmentSkillSO 리소스 준비 상태를 구분해서 보고한다.
```
