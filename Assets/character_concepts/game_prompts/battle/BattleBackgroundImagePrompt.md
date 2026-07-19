# Battle Background Image Prompt

Use this prompt after an episode battle plan exists and includes
`backgroundImageDirection`.

## Prompt

```text
작업 폴더 = {project_root}

BattleCreateGuide.md, BattleSO.md, EpisodeBattlePlanGuide.md 기준으로
배틀 기획에서 배경 이미지를 생성해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
- Assets/character_concepts/game_prompt_guide/battle/EpisodeBattlePlanGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- chapterGroup: {chapter_group}
- episodeId: {episode_id}
- battleId: {battle_id}
- battleGroup: {battle_group}
- episodeBattlePlanFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.{chapter_group}.json
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
- episodeCompositionFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.{chapter_group}.json
- outputImagePath: Assets/Resources/battle/battle_png/{battle_id}.background.png

작업:
1. episodeBattlePlanFile에서 대상 episode와 battleId를 확인한다.
2. backgroundImageDirection을 읽는다.
3. episodePlanningFile과 storyContextFile에서 장소, 시간대, 정서, 금지 조건을 보강한다.
4. 한 장짜리 16:9 배틀 배경 이미지를 생성한다.
5. 기본 스타일은 pixel_game_background로 한다.
6. 기본 해상도는 2560x1440으로 한다.
7. 중앙 전투 영역은 캐릭터, 몬스터, 스킬 이펙트가 잘 보이도록 비워둔다.
8. 오브젝트는 너무 크게 배치하지 않는다.
9. 캐릭터, 몬스터, UI, 텍스트, 로고, 스폰 마커는 이미지에 넣지 않는다.
10. 레이어 분리는 명시 요청이 있을 때만 한다.
11. 생성 이미지를 outputImagePath에 저장한다.
12. Unity Sprite import용 meta가 필요하면 생성한다.

Output:
- 생성한 배경 PNG 경로
- backgroundSprite 값: {battle_id}.background
- 이미지 해상도
- 사용한 이미지 생성 프롬프트 요약
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_episode_battle_plan
  - missing_background_direction
  - image_generation_failed
  - invalid_image_ratio
  - output_write_failed
- 실패 원인
- 보강이 필요한 입력 문서
- 다음에 실행해야 할 프롬프트

검증:
- outputImagePath에 PNG가 존재해야 한다.
- 이미지는 16:9여야 한다.
- 기본 목표는 2560x1440이다.
- Unity에서 Sprite로 import 가능해야 한다.
- 파일명은 `{battleId}.background.png` 규칙을 따라야 한다.
- BattleSO JSON에는 `backgroundSprite: "{battleId}.background"`로 연결할 수 있어야 한다.
- 배경은 기본적으로 한 장짜리 이미지여야 한다.

주의:
- 이 프롬프트는 배경 PNG만 생성한다.
- BattleSO JSON, BattleSO asset, spawner JSON은 수정하지 않는다.
- 바닥/전경 레이어 분리는 명시 요청이 있을 때만 수행한다.
```
