# Strategic Skill Reverse Planning Prompt

기존 전략 아이템의 embedded skill과 생성된 스킬 리소스를 분석해 역기획하거나,
승인된 신규 설계 입력을 동일한 전략 스킬 기획 형식으로 문서화할 때 사용합니다.

## Prompt

```text
작업 폴더 = {project_root}

기존 전략 스킬 구현을 분석해 전략 스킬 기획 문서를 역기획해줘. 구현 데이터는 근거로만 읽고, 결과에는 대상·범위·효과·수치·지속시간·사용 의도 같은 기획 언어만 작성해. SO JSON 필드나 런타임 enum은 기획 문서에 넣지 마.

Input:
- projectRoot: {project_root}
- planningMode: {reverse_existing | new_design}
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
- approvedDesignInput: {
    sourceDesignPath: {null | path},
    skillSlug: {null | lowercase_snake_case},
    skillNameKo: {null | string},
    linkedItemId: {null | item.strategic.*},
    itemGrade: {null | Basic | Advanced},
    gaugeCost: {null | 0..100},
    primaryIntent: {null | string},
    secondaryIntent: {null | string},
    activationKo: {null | string},
    targetingKo: {null | string},
    effectsKo: {[] | string[]},
    executionKo: {null | string},
    stackingKo: {null | string},
    cooldownKo: {null | string},
    terminationKo: {null | string},
    presentationKo: {null | string}
  }

입력 모드 규칙:
- reverse_existing은 기존 item.json, embedded skill, 생성 SO, 현재 런타임을 근거 우선순위에 따라 비교한다.
- new_design은 approvedDesignInput 또는 sourceDesignPath의 승인된 설계만 사용한다. 이름이나 콘셉트만으로 피해, 범위, 지속시간, 횟수, 비용을 추정하지 않는다.
- new_design에서 대상, 효과 수치, 실행 방식, 중첩, 종료 조건처럼 구현과 밸런스에 필요한 핵심 입력이 빠지면 review_ready로 만들지 않는다.
- generatedSkillResourceRoots 중 일부가 없더라도 하나 이상의 유효한 근거 root와 대상 item이 있으면 계속한다. 모든 근거 root가 없을 때만 missing_source_root로 실패한다.
- 존재하지 않는 선택 root는 Issue Summary에 기록하되 그 자체로 전체 실패 처리하지 않는다.

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
18. 각 스킬마다 읽은 source 파일 경로, 채택한 플레이어 체감 사실, 서로 충돌한 사실을 내부 evidence matrix로 작성한 뒤 기획 문장을 만든다.
19. evidence matrix에는 sourcePath, evidenceType, priority, observedFactKo, conflictGroup을 기록한다. 이는 검증 보고에 출력하되 개별 planning JSON에는 구현 필드명을 복사하지 않는다.
20. 구현과 설명이 충돌하면 상위 근거를 자동 채택하지 말고 양쪽 게임플레이 해석을 openQuestionsKo의 구체적인 선택지로 기록한다.
21. 아래 메커니즘 완결성 항목을 모두 점검한다: 발동 방식, 대상 선택, 적용 범위, 효과 순서, 회당/총합 수치, 횟수/간격, 지속시간, 중첩/재적용, 쿨다운 또는 게이지 외 사용 제한, 종료 조건.
22. 해당하지 않는 항목은 "해당 없음"으로 내부 검증하고, 필요한데 근거가 없으면 needs_decision 또는 blocked로 분류한다.
23. openQuestionsKo는 승인 대상 수치 또는 서로 배타적인 게임플레이 선택지를 완전한 문장으로 작성한다. "설명 추가", "수치 승인", "검토 필요"만 적는 포괄 문장은 금지한다.
24. 범위가 여러 런타임 값의 합성으로만 계산되는 경우 계산 관례가 가이드에 명시되어 있지 않으면 확정값으로 쓰지 말고 선택지로 남긴다.
25. item 이름과 embedded skill 이름이 다르면 어느 이름을 최종 표시명으로 사용할지 openQuestionsKo에 두 이름을 모두 제시한다.

Output:
- Scope:
- Source Item Files:
- Source Skill Resources:
- Planned Skill Count:
- Review Ready Count:
- Needs Decision Count:
- Blocked Count:
- Issue Summary:
- Missing Optional Source Roots:
- Evidence Matrix Summary:
- Evidence Conflict Count:
- Mechanics Completeness: Pass / Fail
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
- documentId는 `design.{skillId}`와 정확히 같아야 한다.
- linkedItem.grade와 balanceContext.itemGrade, linkedItem.gaugeCost와 balanceContext.gaugeCost는 각각 같아야 한다.
- review_ready는 openQuestionsKo가 비어 있고 메커니즘 완결성 항목에 미확정 사항이 없어야 한다.
- needs_decision은 openQuestionsKo에 최소 하나의 구체적인 수치 승인안 또는 배타적 게임플레이 선택지가 있어야 한다.
- blocked는 지원되지 않는 핵심 동작 또는 누락된 핵심 수치와 직접 연결된 openQuestionsKo를 가져야 한다.
- 효과 문장의 백분율은 `%`, 백분율 포인트 변화는 `%p`로 구분하고, 현재 체력과 최대 체력 기준을 명시해야 한다.
- 반복 효과는 회당 수치, 횟수, 간격, 총합 또는 총합 산정 가능 여부를 밝혀야 한다.
- 지속 효과는 지속시간, 중첩/재적용 방식, 종료 조건을 확정하거나 openQuestionsKo에 남겨야 한다.

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
