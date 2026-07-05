# Battle Generation Prompt

복사해서 agent에게 전달하기 위한 단일 생성 프롬프트입니다.

상세 규칙은 아래 가이드를 기준으로 합니다.

```text
Assets/character_concepts/game_prompt_guide/battle/BattleStoryContextGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleCreateGuide.md
Assets/character_concepts/game_prompt_guide/battle/BattleSO.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnerCreateGuide.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnSO.md
Assets/character_concepts/game_prompt_guide/spawner/SpawnerVariationCreateGuide.md
Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
Assets/character_concepts/game_prompt_guide/character/CharacterDesignCreateGuide.md
```

## Prompt

```text
BattleStoryContextGuide.md, BattleCreateGuide.md, BattleSO.md, SpawnerCreateGuide.md, SpawnSO.md 기준으로 BattleSO 완성 작업을 진행해줘.

Input:
- actId: act.01
- chapterId: chapter.01
- battleId: battle.act1.chapter01.example
- battleContextFile: Assets/Doc/Battle/{battle_context_file}.json
- monsterContextFile: Assets/Doc/Character/{act_group_id}/monster_context.{act_group_id}.json
- monsterCompositionFile: Assets/Doc/Character/{act_group_id}/monster_composition.chapter_XX_YY.json
- storyReferenceFiles:
  - Assets/Doc/Story/00_Background.md
  - Assets/Doc/Story/01_Overall_Story.md
  - Assets/Doc/Story/Act_01_Background.md
  - Assets/Doc/Story/Chapter_01.md

작업 순서:
1. battleContextFile을 분석해서 전투 목적, 공간 태그, 리듬 태그, 난이도, 핵심 위협, 금지 조건을 정리한다.
2. monsterContextFile과 monsterCompositionFile에서 이 전투에 사용할 몬스터 풀을 확정한다.
3. 확정된 몬스터 풀을 역할 기준으로 분류한다.
   - Melee
   - Ranged
   - Tank
   - Support
   - Elite
   - Boss
4. 기존 스포너/스폰 베리에이션에서 이 전투에 적합한 것을 검색한다.
   - Assets/Doc/Spawner 아래의 variation/profile/catalog 문서
   - Assets/Scripts/battle_spawn/Resource/Jsons/sequence_presets.json
   - 이미 생성된 SpawnSequenceSO / SpawnSquadSO 참조 가능 여부
5. 적합한 스포너가 있으면 재사용한다.
6. 적합한 스포너가 없으면 새 스포너를 추가한다.
   - 스포너 JSON은 몬스터 이름을 직접 포함하지 않는다.
   - spawnUnitKey는 스폰 슬롯 의미만 표현한다.
   - spawnRole은 역할 의미만 표현한다.
   - 실제 몬스터 연결은 BattleSO spawnUnitBindings에서만 처리한다.
   - 새 스포너는 sequence step + inline content + squadPattern/group pattern 구조로 작성한다.
7. 선택 또는 추가된 스포너의 spawnUnitKey / spawnRole 슬롯을 읽고, 확정된 몬스터 풀을 BattleSO spawnUnitBindings로 연결한다.
8. BattleSO 입력 JSON을 완성한다.
9. 필요한 경우 Unity editor builder 코드가 spawnUnitBindings를 처리하는지 확인하고, 누락되어 있으면 BattleJsonGenerator/BattleSOAssetBuilder/validation을 함께 수정한다.
10. 생성/수정 결과를 검증한다.

Output:
- 확정 몬스터 풀 요약
- 선택한 기존 스포너 또는 새로 추가한 스포너 요약
- 새 스포너를 추가했다면 수정된 spawner JSON 경로
- BattleSO 입력 JSON 경로
- BattleSO에 저장될 spawnSequenceId 또는 spawnSequencePath
- BattleSO에 저장될 spawnUnitBindings 목록
- 검증 결과

검증 기준:
- BattleSO가 SpawnSequenceSO를 참조한다.
- 모든 spawnUnitKey는 exact binding 또는 role fallback으로 해석 가능하다.
- 모든 spawnUnitBindings.characterId는 실제 CharacterSO로 연결 가능하다.
- 스포너 JSON에는 characterId, CharacterSO, 몬스터 이름이 직접 들어가지 않는다.
- patternKind, spawnRole, victoryRule 등 enum 값은 parsing/build 시점에 검증된다.
- 기존에 재사용 가능한 스포너가 있으면 새 스포너를 만들지 않는다.
- 새 스포너를 만들 경우 기존 sequence_presets 구조와 호환되어야 한다.
- BattleSO 생성 후 BattleManager가 BattleSpawnManager.PlaySequence(spawnSequence, SpawnUnitBindingResolver)를 통해 실행할 수 있어야 한다.
```
