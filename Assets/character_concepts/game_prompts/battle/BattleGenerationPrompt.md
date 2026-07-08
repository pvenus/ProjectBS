# Battle Generation Prompt

Legacy battle-context 기반 입력을 새 전투 생성 파이프라인에 맞게 점검하거나
마이그레이션할 때 사용하는 호환 프롬프트입니다.

신규 에피소드 기반 전투는 기본적으로 아래 순서를 사용합니다.

1. `EpisodeBattlePlanPrompt.md`
2. `BattleBackgroundImagePrompt.md`
3. `BattleFromEpisodePlanPrompt.md`
4. `BattleSOAssetBuildPrompt.md`

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

아래 참조 가이드를 기준으로 legacy battleContextFile 기반 전투 입력이
현재 BattleSO 생성 파이프라인으로 변환 가능한지 점검하고, 조건이 충족될 때만
BattleStoryContext JSON과 BattleSO 입력 JSON을 생성해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
- Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
- Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
- Assets/character_concepts/game_prompt_guide/prompt/PromptAuthoringGuide.md

Input:
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- battleId: {battle_id}
- battleGroup: {battle_group}
- battleContextFile: Assets/Doc/Battle/{battle_context_file}.json
- monsterContextFile: Assets/Doc/Character/{act_group_id}/monster_context.{act_group_id}.json
- monsterCompositionFile: Assets/Doc/Character/{act_group_id}/monster_composition.chapter_XX_YY.json
- backgroundImagePath: Assets/Resources/battle/battle_png/{battle_id}.background.png
- outputBattleStoryContextFile: Assets/Doc/Battle/{battle_group}/{battle_id}.story_context.json
- outputBattleJsonFile: Assets/Resources/battle/{battle_group}/{battle_id}.json
- spawnerSearchRoots:
  - Assets/Resources/battle/spawner/Jsons/sequence_presets
  - Assets/Doc/Spawner
  - Assets/Scripts/battle_spawn/Resource/Jsons
- storyReferenceFiles:
  - Assets/Doc/Story/00_Background.md
  - Assets/Doc/Story/01_Overall_Story.md
  - Assets/Doc/Story/Act_XX_Background.md
  - Assets/Doc/Story/Chapter_XX.md

작업:
1. battleContextFile을 읽고 전투 목적, 공간 태그, 리듬 태그, 난이도, 핵심 위협, 금지 조건을 요약한다.
2. monsterContextFile과 monsterCompositionFile에서 이 전투에 사용할 수 있는 몬스터 후보를 추린다.
3. 후보 몬스터를 역할 기준으로 분류한다.
   - Melee
   - Ranged
   - Tank
   - Support
   - Elite
   - Boss
4. spawnerSearchRoots에서 battleContextFile의 공간/리듬/난이도/승리 조건과 맞는 기존 스포너를 검색한다.
5. 재사용 가능한 스포너가 없으면 새 스포너를 만들지 말고 실패로 보고한다.
6. 재사용 가능한 스포너가 있으면 spawnUnitKey와 spawnRole 슬롯을 읽는다.
7. 슬롯별 exact binding을 우선 구성하고, exact binding이 불필요한 슬롯은 role fallback으로 해석 가능한지 확인한다.
8. 모든 binding 대상 characterId가 실제 CharacterSO로 연결 가능한지 확인한다.
9. backgroundImagePath가 없으면 BattleBackgroundImagePrompt.md를 먼저 실행해야 한다고 보고한다.
10. 조건이 모두 충족될 때만 BattleStoryContext JSON과 BattleSO 입력 JSON을 생성한다.
11. BattleSO 입력 JSON에는 backgroundPrefab을 넣지 않고 `backgroundSprite: "{battleId}.background"`를 넣는다.
12. BattleSO 입력 JSON에는 spawnSequenceId 또는 spawnSequencePath와 spawnUnitBindings를 넣는다.

Output:
- legacy 입력 요약
- 확정 몬스터 풀 요약
- 역할별 몬스터 분류
- 선택한 기존 스포너 경로
- 선택한 spawnSequenceId 또는 spawnSequencePath
- spawnUnitBindings 목록
- BattleStoryContext JSON 경로
- BattleSO 입력 JSON 경로
- backgroundSprite 값
- BattleSO 생성 가능 여부
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_required_input
  - reusable_spawner_not_found
  - missing_background_image
  - unresolved_spawn_binding
  - missing_character_so
  - invalid_json_or_enum
- 실패 원인
- 필요한 다음 프롬프트
- 새 스포너가 필요한 경우: SpawnerCreatePrompt.md 실행 필요
- 배경 이미지가 필요한 경우: BattleBackgroundImagePrompt.md 실행 필요

검증:
- JSON 문법이 유효해야 한다.
- 기존 재사용 스포너가 있을 때만 BattleSO 입력 JSON을 생성한다.
- BattleSO는 SpawnSequenceSO를 spawnSequenceId 또는 spawnSequencePath로 참조할 수 있어야 한다.
- 모든 spawnUnitKey는 exact binding 또는 role fallback으로 해석 가능해야 한다.
- 모든 spawnUnitBindings.characterId는 실제 CharacterSO로 연결 가능해야 한다.
- 스포너 JSON에는 characterId, CharacterSO, 몬스터 이름이 직접 들어가지 않아야 한다.
- patternKind, spawnRole, victoryRule 등 enum 값은 현재 코드에서 파싱 가능한 값이어야 한다.
- BattleSO 생성 후 BattleManager가 BattleSpawnManager.PlaySequence(spawnSequence, SpawnUnitBindingResolver) 구조로 실행할 수 있어야 한다.

주의:
- 이 프롬프트는 legacy 호환용이다.
- 새 스포너 생성, Unity builder 코드 수정, SO 에셋 생성은 이 프롬프트 범위가 아니다.
- 신규 에피소드 기반 전투는 EpisodeBattlePlanPrompt.md부터 진행한다.
```
