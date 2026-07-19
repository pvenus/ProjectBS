# Spawner Create Prompt

Use this prompt when episode battle planning fails because no reusable spawner
matches the required battle shape.

This prompt creates or updates reusable typed spawner JSON. It must not bind
concrete monsters.

## Prompt

```text
작업 폴더 = {project_root}

SpawnerCreateGuide.md, SpawnerVariationCreateGuide.md, SpawnSO.md,
BattleCreateGuide.md 기준으로 재사용 가능한 스포너 JSON을 생성해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
- Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- chapterGroup: {chapter_group}
- episodeId: {episode_id}
- battleId: {battle_id}
- battleGroup: {battle_group}
- episodeBattlePlanFailureSummary: {failure_summary_or_path}
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- episodeBattleMonsterPoolFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_battle_monster_pool.{chapter_group}.json
- storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
- episodeCompositionFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.{chapter_group}.json
- spawnerType: {new_or_existing_spawner_type}
- outputSpawnerJson: Assets/Resources/battle/spawner/Jsons/sequence_presets/{spawnerType}.json

작업:
1. 실패 요약에서 기존 스포너를 사용할 수 없었던 이유를 읽는다.
2. episodePlanningFile과 storyContextFile에서 전투 목적, 공간 태그, 리듬 태그, 금지 조건을 확인한다.
3. episodeBattleMonsterPoolFile에서 필요한 역할 슬롯만 확인한다.
4. 스포너 타입의 재사용 목적을 정의한다.
5. normal 난이도를 기준 모양으로 설계한다.
6. 필요한 경우 very_easy, easy, hard, very_hard, boss 난이도 object를 추가한다.
7. 각 difficulty는 sequence step + inline content + squadPattern/group pattern 구조로 작성한다.
8. spawnUnitKey는 스폰 슬롯 의미만 표현한다.
9. spawnRole은 역할 의미만 표현한다.
10. 스포너 JSON에는 characterId, CharacterSO, 몬스터 이름을 절대 넣지 않는다.
11. targetSpawnCount, spawnWindowSec, clearWindowSec가 실제 step count와 어긋나지 않게 맞춘다.
12. outputSpawnerJson에 저장한다.

Output:
- 생성/수정한 spawner JSON 경로
- spawnerType
- difficulty 목록
- 각 difficulty의 sequenceId
- targetPartySize
- targetSpawnCount
- spawnWindowSec / clearWindowSec
- required spawnUnitKey / spawnRole 목록
- 기존 배틀 플랜에서 다시 선택 가능한지 여부
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_failure_summary
  - insufficient_battle_shape
  - invalid_spawner_type
  - invalid_spawn_enum
  - invalid_spawn_count
  - output_write_failed
- 실패 원인
- 보강이 필요한 스포너 요구사항
- 다시 실행해야 할 선행 프롬프트

검증:
- JSON 문법이 유효해야 한다.
- one file per spawnerType 구조여야 한다.
- difficulties 배열 아래에 난이도별 object가 있어야 한다.
- 각 difficulty는 sequence를 가져야 한다.
- sequence step은 inline content를 가져야 한다.
- squadPattern/group pattern 구조가 SpawnSO.md와 호환되어야 한다.
- patternKind, repeatMode, completionMode, spawnRole enum 값이 기존 코드/가이드와 맞아야 한다.
- spawnUnitKey에는 몬스터 이름이나 characterId가 들어가면 안 된다.
- targetSpawnCount는 계산 가능한 실제 스폰 수와 맞아야 한다.

주의:
- 이 프롬프트는 재사용 가능한 스포너 타입 JSON만 만든다.
- 실제 몬스터 연결은 BattleSO spawnUnitBindings에서만 처리한다.
- CharacterSO, BattleSO asset, BattleSO 입력 JSON은 생성하지 않는다.
```
