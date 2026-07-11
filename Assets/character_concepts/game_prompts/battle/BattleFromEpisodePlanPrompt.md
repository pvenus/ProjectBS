# Battle From Episode Plan Prompt

Use this prompt after an episode battle plan has selected a reusable spawner.

## Prompt

```text
작업 폴더 = {project_root}

BattleStoryContextGuide.md, BattleCreateGuide.md, BattleSO.md 기준으로
BattleStoryContext JSON과 BattleSO 입력 JSON을 생성해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
- Assets/character_concepts/game_prompt_guide/battle/EpisodeBattlePlanGuide.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- chapterGroup: {chapter_group}
- monsterCompositionGroup: {monster_composition_group}
- episodeId: {episode_id}
- battleId: {battle_id}
- battleGroup: {battle_group}
- episodeBattlePlanFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.{chapter_group}.json
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- episodeBattleMonsterPoolFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.{chapter_group}.json
- storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
- episodeCompositionFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.{chapter_group}.json
- monsterContextFile: Assets/Doc/Character/{act_group_id}/monster_context.{act_group_id}.json
- monsterCompositionFile: Assets/Doc/Character/{act_group_id}/monster_composition.{monster_composition_group}.json
- selectedSpawnerJson: {selected_spawner_json_path}
- backgroundImagePath: Assets/Resources/battle/battle_png/{battle_id}.background.png
- outputBattleStoryContextFile: Assets/Doc/Battle/{battle_group}/{battle_id}.story_context.json
- outputBattleJsonFile: Assets/Resources/battle/{battle_group}/{battle_id}.json

작업:
1. episodeBattlePlanFile에서 selected spawner, sequenceId, requiredSlots, selectedBindings를 읽는다.
2. BattleStoryContext JSON을 생성한다.
3. episodeBattlePlanFile의 backgroundImageDirection과 backgroundImagePath를 확인한다.
4. 배경 이미지가 없으면 BattleBackgroundImagePrompt.md를 먼저 실행해야 한다고 보고한다.
5. BattleSO 입력 JSON을 생성한다.
6. BattleSO JSON에는 `backgroundSprite: "{battleId}.background"`를 넣는다.
7. BattleSO JSON에는 spawnSequenceId 또는 spawnSequencePath를 넣는다.
8. BattleSO JSON의 spawnUnitBindings에만 실제 characterId를 넣는다.
9. spawnerSelection은 검토용 메타데이터로 넣을 수 있지만, 런타임 필수 필드는 spawnSequenceId와 spawnUnitBindings다.
10. 기존 스포너 JSON은 수정하지 않는다.

Output:
- BattleStoryContext JSON 경로
- BattleSO 입력 JSON 경로
- backgroundSprite 값
- 배경 PNG 경로
- 배경 이미지가 없을 때 필요한 다음 단계
- spawnSequenceId 또는 spawnSequencePath
- spawnUnitBindings 목록
- BattleSO 생성 가능 여부
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_episode_battle_plan
  - missing_selected_spawner
  - missing_background_image
  - unresolved_spawn_binding
  - missing_character_so
  - invalid_json_or_enum
- 실패 원인
- 생성하지 않은 산출물 목록
- 필요한 선행 프롬프트

검증:
- JSON 문법이 유효해야 한다.
- selected SpawnSequenceSO가 존재하거나 spawnSequenceId로 해석 가능해야 한다.
- 모든 spawnUnitKey가 exact binding 또는 role fallback으로 해석 가능해야 한다.
- 모든 spawnUnitBindings.characterId가 CharacterSO로 해석 가능해야 한다.
- victoryRule 값이 유효해야 한다.
- backgroundSprite가 존재해야 한다. 생략 시 `{battleId}.background` 이름으로 `Assets/Resources/battle/battle_png/` 아래 Sprite를 찾을 수 있어야 한다.
- 배경 이미지는 기본적으로 한 장짜리 16:9 Sprite여야 하며, 레이어 분리는 명시 요청이 있을 때만 한다.
- spawner JSON에는 characterId, CharacterSO, 몬스터 이름이 직접 들어가지 않아야 한다.

주의:
- 이 프롬프트는 BattleStoryContext JSON과 BattleSO 입력 JSON만 생성한다.
- 배경 이미지 생성은 BattleBackgroundImagePrompt.md에서 수행한다.
- BattleSO asset 생성은 BattleSOAssetBuildPrompt.md에서 수행한다.
- 기존 스포너 JSON은 수정하지 않는다.
```
