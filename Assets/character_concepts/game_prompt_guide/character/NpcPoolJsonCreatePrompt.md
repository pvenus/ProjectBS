# NPC Pool JSON Create Prompt

Use this prompt after NPC planning JSON files exist, or when reviewing a
partially planned NPC group.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

CharacterDesignCreateGuide.md, NpcPoolJsonCreateGuide.md 기준으로
NPC/몬스터 풀 인덱스 JSON을 생성 또는 갱신해줘.

Input:
- actId: {act_id}
- groupId: {group_id}
- chapterRange: {chapter_range}
- storyReferenceFiles:
  - Assets/Doc/Story/00_Background.md
  - Assets/Doc/Story/Act01/01_Overall_Story.md
  - Assets/Doc/Story/Act01/Act_01_Background.md
  - Assets/Doc/Story/Act01/ChapterXX/Chapter_XX.md
- npcPlanningRoot: Assets/Doc/Character/{group_id}/npc

작업:
1. npcPlanningRoot 아래의 NPC 기획 JSON을 읽는다.
2. 각 NPC의 characterId, 역할, 난이도, 스토리 용도, 공개 타이밍을 정리한다.
3. monster_context.{groupId}.json을 생성 또는 갱신한다.
4. monster_composition.chapter_XX_YY.json을 생성 또는 갱신한다.
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

검증:
- JSON 문법이 유효해야 한다.
- 참조한 NPC planning 파일이 존재해야 한다.
- characterId는 character.* 도메인을 사용해야 한다.
- CharacterSO, skill, stat, image, BattleSO 데이터는 만들지 않는다.
- 인덱스 파일에는 전체 캐릭터 상세를 복사하지 않는다.
```

