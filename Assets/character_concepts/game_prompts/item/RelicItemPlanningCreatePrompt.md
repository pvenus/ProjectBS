# Relic Item Planning Create Prompt

신규 유물 아이디어를 역기획 문서와 동일한 상세도와 스키마의 독립 기획
문서로 작성할 때 사용합니다. SO JSON이나 Unity asset은 생성하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 입력과 가이드를 기준으로 신규 유물 아이템의 독립 기획 문서를 작성해줘. 결과는 기존 유물 역기획 문서와 동일한 schemaVersion 1.0.0 및 relic_item_design 구조와 상세도를 사용하고, EffectSO 또는 RelicSO 구현 필드는 포함하지 마.

Input:
- projectRoot: {project_root}
- relicConceptFile: {projectRoot_기준_승인된_유물_아이디어_상대경로 | null}
- relicConcept: {인라인_승인된_유물_아이디어 | null}
- relicSlug: {lowercase_snake_case_slug}
- allowOverwrite: false
- outputPlanningRoot: Assets/Doc/Relic
- outputPlanningPath: Assets/Doc/Relic/item.relic.{relicSlug}.planning.json
- outputPlanningIndexPath: Assets/Doc/Relic/relic_item.planning.index.json

relicConceptFile과 relicConcept 중 정확히 하나를 제공해야 한다. 입력에는 이름, 희귀도, 역할, 장착 의도, 발동 조건, 대상, 정확한 효과 수치와 단위, 지속시간, 시너지, 페널티와 표현 방향이 포함되어야 한다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemPlanningGuide.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemRulesGuide.md

작업:
1. 유물을 item 도메인으로 정의하고 `relicId=item.relic.{relicSlug}`를 확정한다.
2. 입력에서 fantasy, 역할, 장착 목적, trigger, target, 효과, 시너지, tradeoff, rarity와 presentation을 분리한다.
3. designRoleKo는 주 역할 하나와 필요한 경우에만 보조 역할 하나를 지정한다.
4. effectsKo는 플레이어가 읽는 한국어 문장으로 작성하고 의미 있는 대상, 수치, 단위, 확률, 거리, 지속시간을 포함한다.
5. 입력에 없는 수치, 대상, trigger, stack, 페널티를 이름이나 희귀도만 보고 추론하지 않는다.
6. 필수 기획 값이 완전하면 review_ready, 선택이 필요하면 needs_decision, 핵심 값이 없으면 blocked로 지정한다.
7. openQuestionsKo에는 코드나 구현 필드가 아닌 선택 가능한 gameplay 안을 기록한다.
8. effectEntries, effectType, config, statType, lifetimeType, categoryType, asset GUID, builder 이름을 출력하지 않는다.
9. RelicItemPlanningGuide의 schema와 같은 개별 planning JSON 하나를 outputPlanningPath에 저장한다.
10. index에 relicId, nameKo, reviewStatus, rarityKo, planningPath를 추가하거나 갱신한다. 기존 다른 entry를 제거하거나 재정렬하지 않는다.
11. allowOverwrite=false이고 같은 relic planning이 존재하면 수정하지 않는다.
12. 기획 JSON과 Unity 필수 meta 외에는 생성하거나 수정하지 않는다.

Output:
- Relic ID:
- Source Concept:
- Review Status:
- Design Role:
- Trigger / Target:
- Effects:
- Open Questions:
- Output Planning Path:
- Planning Index Path:
- Schema Validation: Pass / Fail
- Design Boundary Check: Pass / Fail
- Relic SO JSON Generation: not_performed
- Unity Asset Generation: not_performed
- Result: Pass / Fail
- Notes:

Planning Boundary:
- Do not request or emit implementation mapping fields in this planning step.
- Do not emit effectType, config, lifetimeType, categoryType, duration,
  maxApplyCount, EffectSO, EffectEntrySO, asset paths, GUIDs, or builder names.
- If the concept is not enough for SO conversion, record gameplay questions in
  openQuestionsKo and keep the output as planning only.

검증:
- schemaVersion은 1.0.0이고 documentType은 relic_item_design이어야 한다.
- 파일명과 relic.relicId는 `item.relic.{relicSlug}`를 기준으로 일치해야 한다.
- 역기획 문서와 동일한 필드 및 상세도를 가져야 한다.
- effectsKo는 일반 기획 언어로 완전한 효과와 단위를 설명해야 한다.
- SO/Effect JSON 필드, runtime enum, asset reference, builder 명칭이 없어야 한다.
- 불확실한 값이 임의로 확정되거나 review_ready로 표시되지 않아야 한다.
- index의 planningPath와 실제 파일이 일치해야 한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_relic_concept
  - invalid_relic_slug
  - insufficient_design_input
  - conflicting_design_input
  - duplicate_relic_id
  - planning_schema_invalid
  - planning_index_mismatch
  - overwrite_requires_approval
  - output_write_failed
- 실패 원인
- 보강하거나 결정해야 할 기획 입력
- 생성하지 않은 파일

주의:
- 이 프롬프트는 신규 유물 기획만 작성한다.
- RelicSO/Effect JSON과 Unity asset 생성은 승인된 planning을 입력으로 별도 수행한다.
```
