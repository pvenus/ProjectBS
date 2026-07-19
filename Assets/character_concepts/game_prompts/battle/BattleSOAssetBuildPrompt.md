# BattleSO Asset Build Prompt

Use this prompt after the BattleSO input JSON exists.

This prompt is for running or checking the Unity editor builder step. It should
not redesign the battle.

## Prompt

```text
작업 폴더 = {project_root}

BattleSO.md, BattleCreateGuide.md 기준으로 BattleSO 입력 JSON을 Unity
BattleSO asset으로 변환해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
- Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- episodeId: {episode_id}
- battleId: {battle_id}
- battleGroup: {battle_group}
- battleJsonFile: Assets/Resources/battle/{battle_group}/{battle_id}.json
- expectedBattleSOPath: Assets/Resources/battle/{battle_group}/{battle_id}.asset
- backgroundImagePath: Assets/Resources/battle/battle_png/{battle_id}.background.png

작업:
1. battleJsonFile의 JSON 문법을 확인한다.
2. victoryRule, spawnRole, patternKind 등 enum 값이 코드/가이드와 맞는지 확인한다.
3. backgroundSprite가 있으면 Sprite로 해석 가능한지 확인한다.
4. spawnSequenceId 또는 spawnSequencePath가 SpawnSequenceSO로 해석 가능한지 확인한다.
5. spawnUnitBindings의 모든 characterId가 CharacterSO로 해석 가능한지 확인한다.
6. 모든 required spawnUnitKey가 exact binding 또는 role fallback으로 해석 가능한지 확인한다.
7. Unity editor builder 메뉴 또는 관련 builder 코드를 사용해 BattleSO asset을 생성/갱신한다.
8. 생성된 BattleSO asset이 expectedBattleSOPath에 있는지 확인한다.

Output:
- BattleSO 입력 JSON 경로
- 생성/갱신된 BattleSO asset 경로
- backgroundSprite 연결 결과
- spawnSequence 연결 결과
- spawnUnitBindings 연결 결과
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_battle_json
  - invalid_json_or_enum
  - missing_background_sprite
  - missing_spawn_sequence
  - unresolved_spawn_binding
  - missing_character_so
  - unity_builder_failed
- 실패 원인
- 막힌 참조 경로
- 재실행 전 필요한 수정

검증:
- BattleSO asset이 생성되어야 한다.
- BattleSO가 SpawnSequenceSO를 참조해야 한다.
- BattleSO가 backgroundSprite를 참조해야 한다.
- 모든 spawnUnitBindings.character가 CharacterSO로 연결되어야 한다.
- 모든 spawnUnitKey는 exact binding 또는 role fallback으로 해석 가능해야 한다.
- 스포너 JSON에는 characterId, CharacterSO, 몬스터 이름이 직접 들어가지 않아야 한다.
- 실패 시 asset을 생성한 것처럼 보고하지 말고, 어떤 참조가 막혔는지 명시한다.

주의:
- 이 프롬프트는 기존 BattleSO 입력 JSON을 에셋으로 변환하는 단계다.
- 전투 기획, 몬스터 풀, 스포너 선택, 배경 이미지 생성은 다시 설계하지 않는다.
```
