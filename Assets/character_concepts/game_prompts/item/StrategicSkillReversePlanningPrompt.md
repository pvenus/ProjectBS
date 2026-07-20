# Strategic Skill Reverse Planning Prompt

기존 전략 아이템의 embedded skill과 생성된 스킬 리소스를 분석해 전략
스킬 기획 문서를 역기획할 때 사용합니다.

## Prompt

```text
작업 폴더 = {project_root}

기존 전략 스킬 구현을 분석해 전략 스킬 기획 문서를 역기획해줘. 구현 데이터는 근거로만 읽고, 결과에는 대상·범위·효과·수치·지속시간·사용 의도 같은 기획 언어만 작성해. SO JSON 필드나 런타임 enum은 기획 문서에 넣지 마.

Input:
- projectRoot: {project_root}
- legacyStrategicItemRoot: Assets/Resources/shop/strategic/so
- generatedSkillResourceRoots:
  - Assets/Resources/skill/json
  - Assets/Resources/shop/strategic/so
- scope: {all | skillId_array}
- skillIds: {[] | [skill.strategic.example]}
- allowOverwrite: false
- outputPlanningRoot: Assets/Doc/StrategicSkill
- outputPlanningIndexPath: Assets/Doc/StrategicSkill/strategic_skill.planning.index.json
- outputPlanningPathPattern: Assets/Doc/StrategicSkill/{skillId}.planning.json

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/strategic/StrategicSkillPlanningGuide.md
- Assets/character_concepts/game_prompt_guide/skill/strategic/StrategicSkillRulesGuide.md
- Assets/character_concepts/game_prompt_guide/skill/strategic/StrategicSkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/item/strategic/StrategicItemRulesGuide.md
- Assets/character_concepts/game_prompt_guide/item/strategic/StrategicItemJsonGuide.md

작업:
1. scope에 해당하는 기존 item.json과 EquipmentSkillSO 리소스를 모두 찾는다.
2. item ID, skill ID, 이름, grade, gaugeCost, 설명과 실제 동작을 내부 근거로 수집한다.
3. 명시적 설명과 실제 구현 값이 일치하는지 비교하되 구현 필드 이름을 출력에 복사하지 않는다.
4. 각 스킬에 한국어 주 역할 하나와 필요한 경우에만 보조 역할 하나를 지정한다.
5. conceptKo와 intendedUseKo를 각각 한 문장으로 작성한다.
6. targetingKo는 누가 선택되고 어느 범위에 적용되는지를 완전한 한국어 문장으로 작성한다.
7. effectsKo는 플레이어가 체감하는 효과를 실행 순서대로 작성하고 대상, 수치, 단위, 횟수, 지속시간을 필요한 만큼 포함한다.
8. executionKo는 즉시 1회, 일정 간격 반복, 일정 시간 영역 유지 중 해당하는 동작만 평문으로 작성한다.
9. presentationKo에는 전투에서 효과를 알아볼 수 있는 시각 방향만 한 문장으로 작성한다.
10. 확정이 필요한 사항은 구현 필드가 아니라 선택 가능한 게임플레이 안을 openQuestionsKo에 기록한다.
11. 지원되지 않는 기능을 임의의 현재 효과로 대체하지 않는다.
12. 충분한 기획은 review_ready, 결정이 필요하면 needs_decision, 현재 구현 불가 또는 핵심 값 누락은 blocked로 분류한다.
13. baseProfile, cast, move, hits, damage object, effect config, layer mask, collider, child ID, enum 이름은 기획 파일에 쓰지 않는다.
14. 스킬 하나마다 `{skillId}.planning.json`을 별도 생성하고 각 파일에는 해당 skill 객체 하나만 넣는다.
15. 모든 개별 파일의 skillId, 이름, 상태, 경로를 outputPlanningIndexPath에 기록한다.
16. allowOverwrite=false이고 대상 개별 파일 또는 index가 존재하면 해당 파일을 수정하지 않는다.
17. 기획 JSON과 Unity 필수 meta만 저장한다. item.json, 독립 skill JSON, SO, 이미지, localization, animation, prefab은 생성하거나 수정하지 않는다.

Output:
- Scope:
- Source Item Files:
- Source Skill Resources:
- Planned Skill Count:
- Review Ready Count:
- Needs Decision Count:
- Blocked Count:
- Issue Summary:
- Output Planning Root:
- Output Planning Index:
- Generated Planning Files:
- ID Coverage: Pass / Fail
- Design Boundary Check: Pass / Fail
- Planning JSON Validation: Pass / Fail
- Skill JSON Generation: not_performed
- Unity Asset Generation: not_performed
- Result: Pass / Fail
- Notes:

검증:
- scope=all이면 발견한 모든 고유 전략 skillId마다 개별 파일이 정확히 하나 있어야 한다.
- 각 planning 파일은 `schemaVersion: "1.0.0"`과 `documentType: "strategic_skill_design"`을 가져야 한다.
- 각 skillId는 `skill.strategic.{skill_slug}` 형식이어야 한다.
- 각 linkedItem.itemId는 실제 source item과 일치해야 한다.
- 각 파일은 conceptKo, intendedUseKo, targetingKo, effectsKo, executionKo, presentationKo를 가져야 한다.
- effectsKo의 수치는 필요한 단위와 적용 기준을 일반 기획 언어로 설명해야 한다.
- SO JSON 필드, 런타임 enum, child ID, builder 이름이 없어야 한다.
- 불확실한 사항은 openQuestionsKo에 게임플레이 선택지로 남아야 한다.
- 지원되지 않는 동작이 review_ready 상태로 분류되지 않아야 한다.
- 각 파일은 단일 skill 객체만 포함해야 한다.
- 파일명은 `{skillId}.planning.json`이어야 한다.
- index의 skillCount와 개별 파일 수가 같아야 한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_source_root
  - no_strategic_skills_found
  - requested_skill_not_found
  - duplicate_skill_id
  - invalid_source_json
  - insufficient_behavior_evidence
  - planning_schema_invalid
  - duplicate_planning_file
  - planning_index_mismatch
  - overwrite_requires_approval
  - output_write_failed
- 실패 원인
- 누락된 skillId 또는 근거
- 생성하지 않은 파일
- 다음에 필요한 작업

주의:
- 역기획 문서는 구현 구조가 아니라 플레이 경험과 효과를 설명하는 기획 문서다.
- item grade와 gaugeCost는 밸런스 맥락이며 standalone skill JSON 필드가 아니다.
- 기획 문서 생성과 전략 스킬 JSON 생성은 별도 프롬프트로 수행한다.
```
