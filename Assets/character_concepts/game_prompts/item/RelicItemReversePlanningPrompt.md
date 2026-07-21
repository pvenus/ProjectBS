# Relic Item Reverse Planning Prompt

기존 RelicSO, Effect asset, 현지화 문구를 분석해 구현 구조와 분리된 개별
유물 기획 문서를 역기획할 때 사용합니다.

## Prompt

```text
작업 폴더 = {project_root}

기존 유물 데이터를 분석해 각 유물의 독립 기획 문서를 역기획해줘. 구현 asset은 근거로만 읽고 결과에는 역할, 장착 의도, 발동 조건, 대상, 효과 수치, 시너지, tradeoff와 표현 방향 같은 일반 기획 언어만 작성해. RelicSO/EffectSO JSON 필드나 runtime enum을 planning에 넣지 마.

Input:
- projectRoot: {project_root}
- legacyRelicRoot: Assets/Resources/shop/relic
- localizationFile: Assets/Resources/string/string_table.csv
- scope: {all | relicId_array}
- relicIds: {[] | [item.relic.example]}
- allowOverwrite: false
- outputPlanningRoot: Assets/Doc/Relic
- outputPlanningIndexPath: Assets/Doc/Relic/relic_item.planning.index.json
- outputPlanningPathPattern: Assets/Doc/Relic/{relicId}.planning.json

참조 가이드:
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemPlanningGuide.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemRulesGuide.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemSO.md

작업:
1. scope에 해당하는 RelicSO, 연결된 Effect asset, 아이콘 근거와 한국어 name/desc를 모두 찾는다.
2. 명시적 기획 지시, player-facing 설명, verified behavior, asset 값, visual name 순으로 근거를 비교한다.
3. 구현 정보는 역할, trigger, target, 효과, 수치, duration과 시너지의 일반 한국어 기획 문장으로 번역한다.
4. description과 asset이 충돌하면 한쪽을 임의로 최종안으로 확정하지 않고 gameplay 선택지를 openQuestionsKo에 기록한다.
5. 각 유물에 주 역할 하나와 필요한 경우에만 보조 역할 하나를 지정한다.
6. equipBehaviorKo, triggerKo, targetKo를 각각 완전한 한국어 문장으로 작성한다.
7. effectsKo에 대상, 값, 단위, 확률, 거리, 지속시간을 의미 있는 만큼 포함한다.
8. synergyKo와 tradeoffKo는 실제 효과에서 도출할 수 있는 build 의도만 작성한다.
9. 충분하고 일관된 기획은 review_ready, 결정이 필요하면 needs_decision, 핵심 근거가 없으면 blocked로 분류한다.
10. effectEntries, EffectSO type/config, stat enum, lifetime, GUID, fileID, builder 이름은 planning에 쓰지 않는다.
11. 유물마다 `{relicId}.planning.json`을 별도 생성하고 각 파일에는 relic 객체 하나만 넣는다.
12. 모든 개별 파일의 relicId, 이름, 상태, 희귀도, 경로를 index에 기록한다.
13. allowOverwrite=false이고 planning 또는 index가 존재하면 해당 파일을 수정하지 않는다.
14. 기획 JSON과 Unity 필수 meta만 저장한다. RelicSO JSON, Effect JSON, asset, localization, icon, pool, shop, reward는 생성하거나 수정하지 않는다.

Output:
- Scope:
- Source Relic Count:
- Planned Relic Count:
- Review Ready Count:
- Needs Decision Count:
- Blocked Count:
- Evidence Conflict Summary:
- Output Planning Root:
- Output Planning Index:
- Generated Planning Files:
- ID Coverage: Pass / Fail
- Design Boundary Check: Pass / Fail
- Planning JSON Validation: Pass / Fail
- Relic SO JSON Generation: not_performed
- Unity Asset Generation: not_performed
- Result: Pass / Fail
- Notes:

검증:
- scope=all이면 발견한 모든 고유 relicId마다 개별 planning 파일이 하나 있어야 한다.
- 각 파일은 schemaVersion 1.0.0, documentType relic_item_design과 relic 객체 하나를 가져야 한다.
- 파일명은 `{relicId}.planning.json`이고 relicId는 `item.relic.{slug}` 형식이어야 한다.
- 각 파일은 RelicItemPlanningGuide의 모든 필수 필드를 가져야 한다.
- effectsKo는 player-facing 기획 언어와 정확한 의미 단위를 사용해야 한다.
- 구현 필드, enum, asset reference, builder 이름이 없어야 한다.
- 충돌한 근거가 openQuestionsKo 없이 review_ready로 분류되지 않아야 한다.
- index의 relicCount와 개별 파일 수가 같아야 한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_source_root
  - missing_localization_source
  - no_relics_found
  - requested_relic_not_found
  - duplicate_relic_id
  - insufficient_behavior_evidence
  - planning_schema_invalid
  - duplicate_planning_file
  - planning_index_mismatch
  - overwrite_requires_approval
  - output_write_failed
- 실패 원인
- 누락된 relicId 또는 근거
- 생성하지 않은 파일
- 다음에 필요한 기획 결정

주의:
- 역기획 문서는 구현 구조가 아니라 플레이 경험과 build 의도를 설명한다.
- source asset의 구형 필드는 기획 문서 스키마가 아니다.
- 기획 문서 생성과 RelicSO JSON 생성은 별도 단계다.
```
