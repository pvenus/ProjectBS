# Episode Stage Node Create Prompt

Episode planning 또는 정식 스크립트 데이터를 기준으로 `RoundNodeSO`와
`PopupEventSO` 변환용 Stage Node JSON을 생성하는 복사용 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 참조 가이드를 기준으로 에피소드용 Stage Node JSON을 생성해줘.
이 단계에서는 Unity SO asset을 직접 생성하지 않고, 수동 변환에 사용할 JSON만 생성한다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/stage/EpisodeStageNodeCreateGuide.md
- Assets/character_concepts/game_prompt_guide/stage/RoundNodeSO.md
- Assets/character_concepts/game_prompt_guide/stage/PopupEventSO.md
- Assets/character_concepts/game_prompt_guide/story/EpisodePlanningCreateGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- chapterGroup: {chapter_group}
- episodeId: {episode_id}
- stageNodeId: {stage_node_id}
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
- episodeCompositionFile: Assets/Doc/StoryPlanning/{act_group_id}/episode_composition.{chapter_group}.json
- optionalEpisodeScriptFile: {episode_script_file_or_null}
- optionalBattleId: {battle_id_or_null}
- optionalBattleGroup: {battle_group_or_null}
- optionalBattleJsonFile: 전투 선택지가 있으면 Assets/Resources/battle/{battle_group}/{battle_id}.json, 없으면 null
- optionalBattleSOPath: 전투 선택지가 있으면 Assets/Resources/battle/{battle_group}/{battle_id}.asset, 없으면 null
- outputStageNodeJsonFile: Assets/Resources/stage_new/{chapter_group}/episode.{episode_id}.json

작업:
1. EpisodeStageNodeCreateGuide.md를 먼저 읽고 Stage Node JSON 구조와 builder 매핑을 확인한다.
2. RoundNodeSO.md를 읽고 root 필드가 RoundNodeSO에 어떻게 매핑되는지 확인한다.
3. PopupEventSO.md를 읽고 nodes[] 항목과 choices/rewards/battle refs 매핑을 확인한다.
4. episodePlanningFile을 읽고 에피소드 목표, 시놉시스, 장소, 등장 인물, 보상 방향, handoff를 정리한다.
5. optionalEpisodeScriptFile이 있으면 플레이어에게 노출될 popup text는 script 내용을 우선 사용한다.
6. optionalEpisodeScriptFile이 없으면 episodePlanningFile의 시놉시스 수준에서 과도하게 대사를 확장하지 않고 짧은 popup chain을 만든다.
7. stageNodeId를 RoundNodeSO.nodeId로 사용할 수 있게 root `stageNodeId`에 넣는다.
8. root `roundNodeType`은 일반 스토리면 Event, 필수 진행이면 RequiredSubEvent를 우선 사용한다.
9. root `startNodeId`는 nodes[]의 첫 popup nodeId와 정확히 일치시킨다.
10. nodes[]에는 PopupEventSO로 변환될 popup event chain을 작성한다.
11. popup event별 `nodeId`는 안정적인 eventId로 작성한다.
12. popup event별 메인 이미지가 필요한 경우 `mainImageRequired: true`와 imageDirection을 넣고, 실제 이미지는 만들지 않는다.
13. 전투 선택지가 필요한 경우 optionalBattleId, optionalBattleGroup, optionalBattleJsonFile, optionalBattleSOPath가 모두 채워져 있는지 확인한다.
14. 전투 선택지가 필요한데 battle 입력이 불완전하면 JSON을 만들지 않고 `incomplete_battle_ref`로 실패 처리한다.
15. 전투 선택지가 필요한 경우 battle 전체 내용을 JSON에 복사하지 말고 battleId, battleJsonRef, battleSORef 같은 id/ref만 연결한다.
16. 보상은 현재 Gold 중심으로 작성하고, rewardId나 reward payload는 PopupEventSO.md의 지원 구조에 맞춘다.
17. outputStageNodeJsonFile에 Stage Node JSON을 저장한다.

Output:
- Stage Node JSON 경로: `outputStageNodeJsonFile`
- stageNodeId
- roundNodeType
- startNodeId
- 생성한 popup event nodeId 목록
- mainImageRequired event 목록
- battle ref 연결 목록
- reward 연결 목록
- 수동 변환에서 생성될 RoundNodeSO 예상 경로: `Assets/Resources/stage_new/nodes/{stageNodeId}.asset`
- 수동 변환에서 생성될 PopupEventSO 예상 경로 목록: `Assets/Resources/stage_new/popup_events/{nodeId}.asset`
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_episode_planning
  - missing_story_context
  - missing_episode_composition
  - missing_output_path
  - incomplete_battle_ref
  - invalid_stage_node_id
  - invalid_round_node_type
  - invalid_popup_chain
  - unresolved_battle_ref
  - invalid_json
- 실패 원인
- 생성하지 않은 산출물
- 보강이 필요한 입력
- 다음에 실행해야 할 프롬프트 또는 수동 작업

검증:
- JSON 문법이 유효해야 한다.
- root stageNodeId가 존재해야 한다.
- roundNodeType은 RoundNodeSO.md의 enum 값이어야 한다.
- startNodeId는 nodes[] 안의 첫 시작 popup nodeId와 일치해야 한다.
- 모든 nodes[].nodeId는 JSON 안에서 고유해야 한다.
- 모든 choices[].choiceId는 해당 JSON 안에서 고유해야 한다.
- nextNodeId가 있으면 nodes[] 안의 실제 nodeId를 가리켜야 한다.
- battle ref는 별도 BattleSO 입력 JSON 또는 BattleSO asset을 id/path로만 참조해야 한다.
- CharacterSO, BattleSO, Sprite object reference를 JSON에 직접 넣지 않아야 한다.
- main image 파일은 이 단계에서 생성하지 않고 `{eventId}.main.png` 규칙으로 후속 생성 가능해야 한다.

주의:
- 이 프롬프트는 Stage Node JSON만 생성한다.
- RoundNodeSO asset, PopupEventSO asset은 Unity editor builder로 수동 변환한다.
- 팝업 메인 이미지는 별도 이미지 생성 프롬프트에서 `{eventId}.main.png` 규칙으로 생성한다.
- BattleSO 입력 JSON 생성은 BattleFromEpisodePlanPrompt.md에서 수행한다.
```
