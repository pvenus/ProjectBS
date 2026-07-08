# Episode Battle Plan Prompt

Use this prompt after episode battle monster pool JSON exists.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

EpisodeBattlePlanGuide.md, BattleCreateGuide.md, BattleSO.md,
SpawnerCreateGuide.md, SpawnSO.md 기준으로 에피소드 배틀 플랜 JSON을
생성해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/battle/EpisodeBattlePlanGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
- Assets/character_concepts/game_prompt_guide/prompt/PromptAuthoringGuide.md

Input:
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- episodeId: {episode_id}
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- episodeBattleMonsterPoolFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
- storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
- episodeCompositionFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.chapter_XX.json
- spawnerSearchRoots:
  - Assets/Resources/battle/spawner/Jsons/sequence_presets
  - Assets/Doc/Spawner
  - Assets/Scripts/battle_spawn/Resource/Jsons

작업:
1. episodePlanningFile에서 전투 목적, partyAssumption, 금지 조건을 확인한다.
2. partyAssumption이 원문 근거와 맞는지 확인한다. 근거가 없으면 임의로 변경하지 않는다.
3. episodeBattleMonsterPoolFile에서 primary/secondary/optional 슬롯을 읽는다.
4. 기존 스포너 후보를 검색한다.
5. 각 후보의 difficulty, targetPartySize, targetSpawnCount, spawnRole, spawnUnitKey를 비교한다.
6. forbidden role/pressure와 충돌하는 후보를 제외한다.
7. 필수 슬롯을 몬스터 풀로 바인딩할 수 있는 후보를 선택한다.
8. battleDirection과 전투 공간 정보를 기준으로 backgroundImageDirection을 작성한다.
9. 후보가 있으면 episode_battle_plan.chapter_XX.json을 생성한다.
10. 후보가 없으면 JSON을 만들지 말고 실패 메시지를 출력한다.
11. story_context, episode_composition, episode JSON에 battlePlanRef 또는 battlePlanStatus를 갱신한다.

Output:
- 선택된 스포너 요약 또는 실패 사유
- episode battle plan JSON 경로
- battleId
- battleStoryContextRef
- battleJsonRef
- backgroundImageDirection
- spawnSequenceId
- requiredSlots
- selectedBindings
- excluded optional slots
- BattleSO 생성 가능 여부
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_episode_planning
  - missing_monster_pool
  - reusable_spawner_not_found
  - unresolved_required_slot
  - invalid_spawner_enum
  - invalid_json
- battle plan JSON을 만들지 않았다고 명시
- 거절한 스포너 후보와 이유
- 필요한 새 스포너 방향
- 다음에 실행해야 할 독립 단계

검증:
- 스포너 JSON에는 characterId나 몬스터 이름이 들어가지 않아야 한다.
- 모든 required spawnUnitKey는 exact binding 또는 role fallback으로 해석 가능해야 한다.
- 선택된 characterId는 실제 CharacterSO 후보로 해석 가능해야 한다.
- patternKind, spawnRole, victoryRule enum은 기존 코드/가이드와 맞아야 한다.
- 후보가 없을 때 실패 JSON 파일을 만들지 않는다.

주의:
- 이 단계에서 새 스포너를 생성하지 않는다.
- 재사용 가능한 스포너가 없으면 배틀 생성 가능 상태가 아니므로 실패로 보고한다.
- BattleSO 입력 JSON 생성은 BattleFromEpisodePlanPrompt.md에서 수행한다.
- 배경 이미지 생성은 BattleBackgroundImagePrompt.md에서 수행한다.
```
