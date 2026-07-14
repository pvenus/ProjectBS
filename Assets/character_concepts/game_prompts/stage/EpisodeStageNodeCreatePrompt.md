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
- optionalExistingStageNodeJsonFile: 기존 파일을 재생성할 때 `outputStageNodeJsonFile`, 신규 생성이면 null
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
5. optionalEpisodeScriptFile이 있으면 원본 지문은 script 내용을 우선 사용하고, UI 길이 때문에 원문을 축약하거나 덮어쓰지 않는다.
6. optionalEpisodeScriptFile이 없으면 episodePlanningFile의 시놉시스 수준에서 과도하게 대사를 확장하지 않고 짧은 popup chain을 만든다.
7. 각 원본 지문에 안정적인 `sourceNarrationId`를 연결하고, 완전한 원문 스냅샷을 `sourceTextKo`에 보존한다.
8. 실제 팝업 표시본만 `textKo` 또는 `bodyKo`에 작성하고 `textLayoutProfile: stage_popup_v1`을 기록한다.
9. 표시 지문은 팝업 하나당 최대 9줄, 줄당 최대 40개의 표시 문자로 구성한다. 표시 문자는 UTF-8 byte나 UTF-16 code unit가 아니라 Unicode text element(grapheme cluster) 기준으로 센다. 조합된 하나의 표시 문자는 1자, 공백과 표시 문장부호는 각각 1자로 계산하고 개행 문자는 줄 경계로만 계산한다.
10. 줄바꿈은 문장, 절, 단어, 한국어 어절 경계를 우선한다. 정확히 40자를 채우기보다 읽기 좋은 균형을 우선하고 단어·어절·고유명사·숫자를 중간에서 자르지 않는다.
11. 의미를 유지한 표시용 문장 정리만으로 9줄 안에 들어오지 않으면 문장 또는 문단 경계에서 여러 popup node로 분리한다. 원문을 생략하거나 말줄임표로 대체하지 않는다.
12. optionalExistingStageNodeJsonFile이 있으면 기존 `nodeId`, `choiceId`, 이미지 파일 연결, `manual_override` 문구를 먼저 읽고 보존한다. `manual_override`가 stage_popup_v1을 초과하면 자동 수정·줄바꿈·분할·절단하지 않고 `manual_override_conflict`로 실패 처리하며 대상 id, 실제 줄 수, 가장 긴 줄의 표시 문자 수를 보고한다.
13. 새 popup event의 `nodeId`는 배열 인덱스나 `.001` 같은 순번이 아닌 영구적인 의미 기반 eventId로 작성한다.
14. 기존 JSON에 이미 존재하는 `.001` 형식의 레거시 nodeId는 문자열 키, 이미지, 평가 이력과의 연결을 위해 그대로 보존한다. 중간에 popup을 삽입하거나 순서를 바꿔도 기존 nodeId를 재번호화하지 않고 새 popup에만 새 의미 기반 nodeId를 발급한다.
15. stageNodeId를 RoundNodeSO.nodeId로 사용할 수 있게 root `stageNodeId`에 넣는다.
16. root `roundNodeType`은 일반 스토리면 Event, 필수 진행이면 RequiredSubEvent를 우선 사용한다.
17. root `startNodeId`는 nodes[]의 첫 popup nodeId와 정확히 일치시킨다.
18. nodes[]에는 PopupEventSO로 변환될 popup event chain을 작성한다.
19. popup event별 메인 이미지가 필요한 경우 `mainImageRequired: true`와 imageDirection을 넣고, 실제 이미지는 만들지 않는다.
20. 지문 분할로 popup이 추가된 경우 기존 이미지 파일명을 바꾸지 않는다. 추가 popup에 별도 이미지가 필요한지 판단하고 필요한 경우에만 `mainImageRequired: true`를 지정한다.
21. 전투 선택지가 필요한 경우 optionalBattleId, optionalBattleGroup, optionalBattleJsonFile, optionalBattleSOPath가 모두 채워져 있는지 확인한다.
22. 전투 선택지가 필요한데 battle 입력이 불완전하면 JSON을 만들지 않고 `incomplete_battle_ref`로 실패 처리한다.
23. 전투 선택지가 필요한 경우 battle 전체 내용을 JSON에 복사하지 말고 battleId, battleJsonRef, battleSORef 같은 id/ref만 연결한다.
24. 보상은 현재 Gold 중심으로 작성하고, rewardId나 reward payload는 PopupEventSO.md의 지원 구조에 맞춘다.
25. outputStageNodeJsonFile에 Stage Node JSON을 저장한다.

Output:
- Stage Node JSON 경로: `outputStageNodeJsonFile`
- stageNodeId
- roundNodeType
- startNodeId
- 생성한 popup event nodeId 목록
- mainImageRequired event 목록
- 원본 지문별 sourceNarrationId와 파생 popup nodeId 매핑
- `stage_popup_v1` 줄 수/줄 너비 검증 결과
- 신규/유지된 popup id와 이미지 파일명 목록
- 보존한 manual_override 목록
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
  - popup_body_line_overflow
  - popup_body_width_overflow
  - invalid_word_wrap
  - source_text_truncated
  - unstable_popup_id
  - manual_override_conflict
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
- 새로 발급한 nodes[].nodeId는 의미 기반 영구 ID여야 하며 배열 위치 또는 `.001`, `.002` 같은 순번을 사용하지 않아야 한다. 기존 출력에 이미 있던 순번형 레거시 nodeId는 변경 없이 유지해야 한다.
- 모든 choices[].choiceId는 해당 JSON 안에서 고유해야 한다.
- nextNodeId가 있으면 nodes[] 안의 실제 nodeId를 가리켜야 한다.
- 원본 지문은 `sourceTextKo`에 완전하게 보존되고 표시용 `textKo/bodyKo`와 구분되어야 한다.
- 원본에서 파생된 모든 popup node는 `sourceNarrationId`를 유지해야 한다.
- 모든 `textKo`, `bodyKo`, `choices[].resultKo`는 각각 최대 9줄이어야 한다.
- 각 표시 줄은 Unicode text element 기준으로 공백과 표시 문장부호를 포함해 최대 40자여야 한다.
- 단어, 한국어 어절, 고유명사, 숫자 중간에 강제 개행이 없어야 한다.
- 9줄을 초과하는 내용은 문장 또는 문단 경계에서 별도 popup node로 분리되고 원문이 누락되지 않아야 한다.
- 재생성 시 기존 nodeId, choiceId, 이미지 파일명, `manual_override` 문구가 유지되어야 한다.
- `manual_override`가 9줄 또는 줄당 40자를 초과하면 자동 변경하지 않고 `manual_override_conflict`로 실패해야 한다.
- battle ref는 별도 BattleSO 입력 JSON 또는 BattleSO asset을 id/path로만 참조해야 한다.
- CharacterSO, BattleSO, Sprite object reference를 JSON에 직접 넣지 않아야 한다.
- main image 파일은 이 단계에서 생성하지 않고 `{eventId}.main.png` 규칙으로 후속 생성 가능해야 한다.

주의:
- 이 프롬프트는 Stage Node JSON만 생성한다.
- RoundNodeSO asset, PopupEventSO asset은 Unity editor builder로 수동 변환한다.
- 팝업 메인 이미지는 별도 이미지 생성 프롬프트에서 `{eventId}.main.png` 규칙으로 생성한다.
- `{eventId}.main.png`의 eventId는 영구적인 의미 기반 nodeId를 사용하며 popup 삽입이나 순서 변경을 이유로 기존 이미지 파일명을 바꾸지 않는다.
- BattleSO 입력 JSON 생성은 BattleFromEpisodePlanPrompt.md에서 수행한다.
```
