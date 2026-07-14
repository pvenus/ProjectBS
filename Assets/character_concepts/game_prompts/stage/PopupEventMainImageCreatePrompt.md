# Popup Event Main Image Create Prompt

Stage Node JSON의 특정 popup event를 기준으로 `PopupEventSO.mainImage`
매핑용 메인 이미지를 생성하는 복사용 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 참조 가이드를 기준으로 popup event 메인 이미지를 생성·재사용하거나 이미지 없는 event를 정상 skip해줘.
이 단계에서는 Stage Node JSON, PopupEventSO asset, RoundNodeSO asset을 수정하지 않고,
imagePolicy가 generate/reuse일 때만
`Assets/Resources/stage_new/popup_png/{eventId}.main.png`를 출력한다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageCreateGuide.md
- Assets/character_concepts/game_prompt_guide/stage/PopupEventSO.md
- Assets/character_concepts/game_prompt_guide/stage/EpisodeStageNodeCreateGuide.md
- Assets/character_concepts/game_prompt_guide/stage/StoryImageVisualGuide.md
- Assets/character_concepts/game_prompt_guide/stage/StoryImageElementGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- chapterId: {chapter_id}
- chapterGroup: {chapter_group}
- actGroupId: {act_group_id}
- episodeId: {episode_id}
- popupName: {planning_popup_name}
- popupId: {planning_popup_id}
- eventId: {planning_popup_id}
- stageNodeJsonFile: Assets/Resources/stage_new/{chapter_group}/episode.{episode_id}.json
- episodePlanningFile: Assets/Doc/StoryPlanning/{act_group_id}/episode.{episode_id}.json
- optionalEpisodeScriptFile: {episode_script_file_or_null}
- storyContextFile: Assets/Doc/StoryPlanning/{act_group_id}/story_context.{act_group_id}.json
- optionalCharacterReferenceFiles:
  - Assets/Doc/Character/{act_group_id}/...
- optionalLocationReferenceFiles:
  - Assets/Doc/Location/...
- optionalStyleReferenceImages:
  - Assets/Resources/stage_new/popup_png/reference/...
- outputImagePath: Assets/Resources/stage_new/popup_png/{planning_popup_id}.main.png

작업:
1. PopupEventMainImageCreateGuide.md를 먼저 읽고 output path, naming, visual rules, validation을 확인한다.
2. episodePlanningFile의 story.popupDefinitions[]에서 popupName과 popupId가 일치하는 항목을 찾고 popupType, sourceNarrationIds, imagePolicy, imageDirection을 읽는다.
3. 신규 popup은 `popupId = node.{act_key}.{chapter_key}.{episode_key}.{popupName}`인지 검증하고 eventId가 popupId와 같은지 확인한다. 이미지 단계에서 id를 새로 만들지 않는다.
4. imagePolicy가 none이면 이미지를 생성하지 않고 정상 skip으로 보고한다. reuse이면 imageSourcePopupId의 승인 PNG를 변경·리사이즈·재압축하지 않고 outputImagePath로 복사한 뒤 SHA-256 동일성을 검증한다.
5. stageNodeJsonFile을 읽고 eventId와 일치하는 popup node를 찾는다.
6. 해당 popup node의 textKo/bodyKo, locationId, speakerId, speakerNameKo, choices, battle entry intent를 정리한다.
7. optionalEpisodeScriptFile이 있으면 같은 eventId 또는 해당 장면의 정식 스크립트 문맥을 우선 반영한다.
8. episodePlanningFile과 storyContextFile로 장소, 시간대, 정서, 인물 관계, 금지 조건을 보강한다.
9. optionalCharacterReferenceFiles와 optionalLocationReferenceFiles가 있으면 외형/장소 일관성 확인용으로만 사용한다.
10. popup 전체 에피소드 요약이 아니라 해당 popup event의 현재 극적 순간 하나를 이미지 방향으로 잡는다.
11. imagePolicy가 generate이면 StoryImageVisualGuide.md와 StoryImageElementGuide.md를 읽고 pixel-game-friendly painted illustration style의 이미지 생성 프롬프트를 작성한다. reuse/none이면 새 시각 프롬프트를 작성하지 않는다.
12. 권장 비율은 3:4, 권장 해상도는 960x1280으로 한다.
13. UI 텍스트, 자막, 말풍선, 버튼, 라벨은 이미지에 넣지 않는다.
14. popup UI가 위에 올라와도 읽히도록 중요한 디테일은 가장자리와 하단 UI 영역에 몰아넣지 않는다.
15. imagePolicy가 generate일 때만 새 이미지를 생성해 outputImagePath에 저장한다. reuse는 4단계의 byte-for-byte 복사 결과를 사용하고 none은 파일 작업 없이 종료한다.
16. imagePolicy가 generate/reuse이고 Unity Sprite import용 meta가 필요하면 생성한다.

Output:
- eventId
- popupName
- popupId
- imagePolicy
- imagePath: generate/reuse이면 outputImagePath, none이면 null
- spriteName: {planning_popup_id}.main
- sourceStageNodeJsonFile
- sourcePopupSummary
- generate이면 visualPrompt와 imageResolution
- reuse이면 sourceImagePath, outputImagePath, sourceSha256, copiedSha256
- none이면 skipped: true, skipReason: image_policy_none
- validationResult

실패 시 Output:
- status: failed
- failureType:
  - missing_stage_node_json
  - popup_event_not_found
  - popup_definition_not_found
  - popup_id_mismatch
  - missing_image_source_popup_id
  - reusable_image_not_found
  - reused_image_checksum_mismatch
  - insufficient_popup_context
  - image_generation_failed
  - invalid_image_ratio
  - output_write_failed
- 실패 원인
- 생성하지 않은 산출물
- 보강이 필요한 입력
- 다음에 실행해야 할 프롬프트 또는 수동 작업

검증:
- imagePolicy가 generate/reuse이면 outputImagePath에 PNG가 존재해야 한다. none이면 PNG 없이 skipped로 보고해야 한다.
- 파일 경로는 `Assets/Resources/stage_new/popup_png/{eventId}.main.png` 규칙을 따라야 한다.
- 신규 popup의 eventId는 planning popupId와 같고 semantic suffix는 popupName과 같아야 한다.
- reuse이면 imageSourcePopupId 원본과 outputImagePath의 SHA-256이 같아야 한다.
- 파일명에는 stageNodeId를 사용하지 않아야 한다.
- Sprite name은 `{eventId}.main`으로 해석 가능해야 한다.
- generate 이미지는 popup event의 현재 장면과 맞고 3:4여야 하며 UI 텍스트, 자막, 말풍선, 버튼, 라벨이 없어야 한다.
- reuse의 현재 event 사용 적합성은 planning에서 승인된 imagePolicy와 imageSourcePopupId 선택으로 확정한다. 이미지 단계에서는 장면을 맞추기 위해 원본을 변경하지 않고 source/destination SHA-256 동일성만 검증한다.
- none은 이미지 내용·비율 검증을 수행하지 않고 정상 skip 결과만 검증한다.
- PopupEventBuilder가 Unity import 후 eventId로 이미지를 찾을 수 있어야 한다.

주의:
- 이 프롬프트는 popup main image PNG만 생성한다.
- Stage Node JSON, PopupEventSO asset, RoundNodeSO asset은 수정하지 않는다.
- eventId별 이미지가 필요하므로 여러 popup event 이미지는 eventId마다 반복 실행한다.
- BattleSO 배경 이미지는 BattleBackgroundImagePrompt.md에서 별도로 생성한다.
```
