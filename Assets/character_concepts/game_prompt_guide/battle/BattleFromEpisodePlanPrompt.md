# Battle From Episode Plan Prompt

Use this prompt after an episode battle plan has selected a reusable spawner.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

BattleStoryContextGuide.md, BattleCreateGuide.md, BattleSO.md 기준으로
BattleStoryContext JSON과 BattleSO 입력 JSON을 생성해줘.

Input:
- actId: {act_id}
- chapterId: {chapter_id}
- battleId: {battle_id}
- episodeBattlePlanFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_plan.chapter_XX.json
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- episodeBattleMonsterPoolFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.chapter_XX.json
- monsterContextFile: Assets/Doc/Character/{act_group_id}/monster_context.{act_group_id}.json
- monsterCompositionFile: Assets/Doc/Character/{act_group_id}/monster_composition.chapter_XX_YY.json
- selectedSpawnerJson: {selected_spawner_json_path}

작업:
1. episodeBattlePlanFile에서 selected spawner, sequenceId, requiredSlots, selectedBindings를 읽는다.
2. BattleStoryContext JSON을 생성한다.
3. BattleSO 입력 JSON을 생성한다.
4. BattleSO JSON에는 spawnSequenceId 또는 spawnSequencePath를 넣는다.
5. BattleSO JSON의 spawnUnitBindings에만 실제 characterId를 넣는다.
6. spawnerSelection은 검토용 메타데이터로 넣을 수 있지만, 런타임 필수 필드는 spawnSequenceId와 spawnUnitBindings다.
7. 기존 스포너 JSON은 수정하지 않는다.

Output:
- BattleStoryContext JSON 경로
- BattleSO 입력 JSON 경로
- spawnSequenceId 또는 spawnSequencePath
- spawnUnitBindings 목록
- BattleSO 생성 가능 여부
- 검증 결과

검증:
- JSON 문법이 유효해야 한다.
- selected SpawnSequenceSO가 존재하거나 spawnSequenceId로 해석 가능해야 한다.
- 모든 spawnUnitKey가 exact binding 또는 role fallback으로 해석 가능해야 한다.
- 모든 spawnUnitBindings.characterId가 CharacterSO로 해석 가능해야 한다.
- victoryRule 값이 유효해야 한다.
- backgroundPrefab이 존재해야 한다.
- spawner JSON에는 characterId, CharacterSO, 몬스터 이름이 직접 들어가지 않아야 한다.
```

