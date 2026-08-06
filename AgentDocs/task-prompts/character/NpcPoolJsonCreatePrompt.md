# NPC Pool JSON Create Prompt


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

Use this prompt after NPC planning JSON files exist, or when reviewing a
partially planned NPC group.

## Prompt

```text
작업 폴더 = {project_root}

CharacterDesignCreateGuide.md, NpcPoolJsonCreateGuide.md 기준으로
NPC/몬스터 풀 인덱스 JSON을 생성 또는 갱신해줘.

참조 가이드:
- AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
- AgentDocs/planning-guides/character/NpcPoolJsonCreateGuide.md
- AgentDocs/planning-guides/story/StoryPlanningContextGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- groupId: {group_id}
- chapterRange: {chapter_range}
- monsterCompositionGroup: {monster_composition_group}
- storyReferenceFiles:
  - AgentDocs/planning-data/story/00_Background.md
  - AgentDocs/planning-data/story/Act01/01_Overall_Story.md
  - AgentDocs/planning-data/story/Act01/Act_01_Background.md
  - AgentDocs/planning-data/story/Act01/ChapterXX/Chapter_XX.md
- npcPlanningRoot: AgentDocs/planning-data/character/act-plans/{group_id}/npc

작업:
1. npcPlanningRoot 아래의 NPC 기획 JSON을 읽는다.
2. 각 NPC의 characterId, 역할, 난이도, 스토리 용도, 공개 타이밍을 정리한다.
3. monster_context.{groupId}.json을 생성 또는 갱신한다.
4. monster_composition.{monster_composition_group}.json을 생성 또는 갱신한다.
5. 에피소드 초반에 쓰면 안 되는 elite/boss/spirit/true reveal 후보는 분리한다.
6. 부족한 역할은 새로 만들어졌다고 가정하지 말고 missingRoles에 기록한다.

Output:
- monster_context JSON 경로
- monster_composition JSON 경로
- 확정 또는 참조한 NPC 풀 요약
- 역할별 분류
  - Melee
  - Ranged
  - Tank
  - Support
  - Elite
  - Boss
- 챕터/에피소드별 사용 후보
- 사용 금지 또는 지연 공개 후보
- missingRoles
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_npc_planning_root
  - no_npc_planning_json
  - invalid_npc_planning_json
  - invalid_character_id_domain
  - output_write_failed
- 실패 원인
- 보강이 필요한 NPC planning 파일
- 생성하지 않은 산출물

검증:
- JSON 문법이 유효해야 한다.
- 참조한 NPC planning 파일이 존재해야 한다.
- characterId는 character.* 도메인을 사용해야 한다.
- CharacterSO, skill, stat, image, BattleSO 데이터는 만들지 않는다.
- 인덱스 파일에는 전체 캐릭터 상세를 복사하지 않는다.

주의:
- 이 단계는 NPC/몬스터 기획 인덱스를 만드는 단계다.
- 실제 몬스터 SO가 이미 존재한다고 전제하지 않는다.
- 부족한 역할은 missingRoles에 남기고 임의 생성하지 않는다.
```
