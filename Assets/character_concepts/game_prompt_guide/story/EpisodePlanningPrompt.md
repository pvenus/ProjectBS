# Episode Planning Prompt

Use this prompt when converting episode prose into synopsis-level planning JSON.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

EpisodeFormat.md, StoryStructureGuide.md, StoryPlanningContextGuide.md,
EpisodePlanningCreateGuide.md, RewardPlanningGuide.md 기준으로 에피소드
기획 JSON을 생성해줘.

Input:
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- episodeId: {episode_id}
- episodeFile: {episode_markdown_path}
- chapterFile: {chapter_markdown_path}
- storyReferenceFiles:
  - Assets/Doc/Story/00_Background.md
  - Assets/Doc/Story/Act01/01_Overall_Story.md
  - Assets/Doc/Story/Act01/Act_01_Background.md
  - Assets/Doc/Story/Characters.md

작업:
1. episodeFile 원문을 읽고 시놉시스 수준으로 요약한다.
2. 원문에 있는 플레이어/파티 기준을 확인한다. 근거가 없으면 임의로
   2인/3인 파티를 넣지 않는다.
3. common/story/monster/battle/reward/handoff 카테고리로 episode JSON을 만든다.
4. 공용 JSON, story_context JSON, episode_composition JSON을 생성 또는 갱신한다.
5. 이 단계에서는 정확한 spawner, spawn count, BattleSO, CharacterSO를 만들지 않는다.
6. 전투가 필요하면 battleMonsterPoolRef가 들어갈 위치를 만든다.

Output:
- 생성/수정한 JSON 경로
- episode battleNeed
- partyAssumption 판단 근거
- monster direction 요약
- battle direction 요약
- reward direction 요약
- 다음 단계에서 필요한 작업
- 검증 결과

검증:
- JSON 문법이 유효해야 한다.
- sourceEpisodeFile, sourceChapterFile이 존재해야 한다.
- 시놉시스가 정식 스크립트 수준의 대사/지문으로 확장되지 않아야 한다.
- partyAssumption은 원문 근거가 있어야 한다.
- battle에는 정확한 spawner type, spawnSequenceId, spawn count가 없어야 한다.
- reward는 현재 gold 기준만 사용한다.
```

