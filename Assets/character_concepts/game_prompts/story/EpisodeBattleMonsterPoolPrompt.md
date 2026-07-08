# Episode Battle Monster Pool Prompt

Use this prompt after episode planning exists and before battle plan generation.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

EpisodeBattleMonsterPoolGuide.md, EpisodePlanningCreateGuide.md,
StoryPlanningContextGuide.md, CharacterDesignCreateGuide.md 기준으로
에피소드 배틀 몬스터 풀 JSON을 생성해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/story/EpisodeBattleMonsterPoolGuide.md
- Assets/character_concepts/game_prompt_guide/story/EpisodePlanningCreateGuide.md
- Assets/character_concepts/game_prompt_guide/story/StoryPlanningContextGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
- Assets/character_concepts/game_prompt_guide/prompt/PromptAuthoringGuide.md

Input:
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- episodeId: {episode_id}
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
- episodeCompositionFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.chapter_XX.json
- optionalMonsterContextFile: Assets/Doc/Character/{act_group_id}/monster_context.{act_group_id}.json
- optionalMonsterCompositionFile: Assets/Doc/Character/{act_group_id}/monster_composition.chapter_XX_YY.json

작업:
1. episodePlanningFile의 story/monster/battle 항목을 읽는다.
2. 전투에 필요한 몬스터 방향을 역할 슬롯으로 나눈다.
3. primary, secondary, optional, optional_flavor, forbidden을 구분한다.
4. 기존 몬스터 기획 파일이 있으면 후보로만 연결한다.
5. 기존 몬스터가 있다고 전제하지 않는다.
6. episode_battle_monster_pool.chapter_XX.json을 생성 또는 갱신한다.
7. story_context, episode_composition, episode JSON에 battleMonsterPoolRef를 연결한다.

Output:
- 생성/수정한 몬스터 풀 JSON 경로
- 확정한 desiredSlots 목록
- primary/secondary/optional 구분
- 후보로 참조한 기존 monster planning 목록
- 피해야 할 후보 목록
- battlePoolReadiness
- 다음 단계 실행 가능 여부
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_episode_planning
  - missing_story_context
  - invalid_episode_planning_json
  - insufficient_battle_direction
  - invalid_json
- 실패 원인
- 보강이 필요한 episode planning 항목
- 다음 단계 실행 가능 여부

검증:
- JSON 문법이 유효해야 한다.
- primary/secondary 슬롯은 story/battle 방향과 맞아야 한다.
- existingCandidateRefs는 reference_only로 표시되어야 한다.
- CharacterSO, stat, skill, spawnSequenceId, BattleSO 필드는 만들지 않는다.
- optional 슬롯 부재는 실패가 아니다.

주의:
- 이 문서는 몬스터를 실제 생성하기 이전 단계의 배틀용 몬스터 풀 기획이다.
- 기존 몬스터 기획 파일은 reference_only 후보로만 사용한다.
- 특정 CharacterSO가 반드시 존재한다고 전제하지 않는다.
```
