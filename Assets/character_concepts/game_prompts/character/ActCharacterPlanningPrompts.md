# Act Character Planning Prompt

Act 단위 스토리를 기획 JSON으로 1차 가공하고, 이후 캐릭터/NPC/몬스터
생성 단계에서 참조할 수 있는 캐릭터 기획 산출물을 만드는 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 참조 가이드를 기준으로 Act 캐릭터/몬스터 기획 산출물을 생성해줘.
이 단계는 기획 JSON 생성 단계이며 CharacterSO, SkillSO, 이미지, 애니메이션,
전투 JSON은 생성하지 않는다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/character/ActCharacterPlanningStartGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterStatGuide.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillBalanceGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
- Assets/character_concepts/game_prompt_guide/story/StoryPlanningContextGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- actGroupId: {act_group_id}
- chapterGroup: {chapter_group}
- monsterCompositionGroup: {monster_composition_group}
- actStoryFile: Assets/Doc/Story/Act_XX_Background.md
- overallStoryFiles:
  - Assets/Doc/Story/00_Background.md
  - Assets/Doc/Story/01_Overall_Story.md
- chapterFiles:
  - Assets/Doc/Story/Chapter_XX.md
- outputPlayerRoot: Assets/Doc/Character/player
- outputActCharacterRoot: Assets/Doc/Character/{act_group_id}

작업:
1. actStoryFile, overallStoryFiles, chapterFiles를 읽고 Act의 핵심 갈등, 세력, 지역, 정서, 반복 모티프를 정리한다.
2. actGroupId를 기준으로 Act 전용 공용 기획 JSON을 생성 또는 갱신한다.
3. Player 후보가 있으면 player 공용/개별 기획 JSON으로 분리한다.
4. NPC, Boss, Monster 후보를 출력 루트 아래의 역할별 기획 JSON으로 분리한다.
5. 각 캐릭터에는 persona, storyRole, revealTiming, combatDirection, difficultyHint, planningRef를 포함한다.
6. 몬스터 후보는 아직 실제 몬스터가 존재한다고 전제하지 말고 기획 방향성으로 작성한다.
7. monster_context.{actGroupId}.json에는 몬스터 생성 전 단계에서 필요한 역할/위협/난이도 방향성을 기록한다.
8. monster_composition.{monster_composition_group}.json에는 챕터별로 사용 가능한 몬스터 역할 풀과 공개 제한을 기록한다.
9. 이후 NPC Pool, CharacterSO, SkillSO, Battle 생성 단계에서 참조 가능한 planningRef를 정리한다.

Output:
- Player common JSON 경로
- Player planning JSON 목록
- Act common JSON 경로
- NPC/Boss/Monster planning JSON 목록
- monster_context JSON 경로
- monster_composition JSON 경로
- Act 캐릭터/몬스터 기획 요약
- missingDesignInputs
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_story_file
  - invalid_act_group_id
  - insufficient_story_basis
  - invalid_json
- 실패 원인
- 보강이 필요한 입력 문서
- 다음에 실행하면 좋은 프롬프트

검증:
- 모든 JSON 문법이 유효해야 한다.
- actGroupId가 출력 경로와 planningRef에 일관되게 반영되어야 한다.
- Player 기획은 Assets/Doc/Character/player 아래에 있어야 한다.
- NPC/Boss/Monster 기획은 Assets/Doc/Character/{act_group_id} 아래에 있어야 한다.
- planningRef는 실제 생성 파일 경로를 가리켜야 한다.
- Player/Npc/Boss는 characterType으로만 사용하고 런타임 도메인은 character를 사용해야 한다.
- BattleStoryContext나 composition에는 전체 캐릭터 상세를 복사하지 않고 planningRef 중심으로 연결해야 한다.

주의:
- 생성된 Act 산출물 폴더에는 JSON과 .meta만 둔다.
- README, 가이드 문서, 프로세스 문서는 Act 산출물 폴더에 만들지 않는다.
- 이 단계에서는 실제 CharacterSO, SkillSO, BattleSO, 이미지, 애니메이션을 생성하지 않는다.
```
