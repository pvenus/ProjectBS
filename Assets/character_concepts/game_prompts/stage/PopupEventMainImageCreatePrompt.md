# Popup Event Main Image Create Prompt

Stage Node JSON의 특정 popup event를 기준으로 `PopupEventSO.mainImage`
매핑용 메인 이미지를 생성하는 복사용 프롬프트입니다.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

아래 참조 가이드를 기준으로 popup event 메인 이미지를 생성해줘.
이 단계에서는 Stage Node JSON, PopupEventSO asset, RoundNodeSO asset을 수정하지 않고,
`Assets/Resources/stage_new/popup_png/{eventId}.main.png` 이미지만 생성한다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageCreateGuide.md
- Assets/character_concepts/game_prompt_guide/stage/PopupEventSO.md
- Assets/character_concepts/game_prompt_guide/stage/EpisodeStageNodeCreateGuide.md
- Assets/character_concepts/game_prompt_guide/prompt/PromptAuthoringGuide.md

Input:
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- episodeId: {episode_id}
- eventId: {popup_event_id}
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
- outputImagePath: Assets/Resources/stage_new/popup_png/{popup_event_id}.main.png

작업:
1. PopupEventMainImageCreateGuide.md를 먼저 읽고 output path, naming, visual rules, validation을 확인한다.
2. stageNodeJsonFile을 읽고 eventId와 일치하는 popup node를 찾는다.
3. 해당 popup node의 textKo/bodyKo, locationId, speakerId, speakerNameKo, choices, battle entry intent를 정리한다.
4. optionalEpisodeScriptFile이 있으면 같은 eventId 또는 해당 장면의 정식 스크립트 문맥을 우선 반영한다.
5. episodePlanningFile과 storyContextFile로 장소, 시간대, 정서, 인물 관계, 금지 조건을 보강한다.
6. optionalCharacterReferenceFiles와 optionalLocationReferenceFiles가 있으면 외형/장소 일관성 확인용으로만 사용한다.
7. popup 전체 에피소드 요약이 아니라 해당 popup event의 현재 극적 순간 하나를 이미지 방향으로 잡는다.
8. pixel-game-friendly painted illustration style의 이미지 생성 프롬프트를 작성한다.
9. 권장 비율은 16:9, 권장 해상도는 1280x720으로 한다.
10. UI 텍스트, 자막, 말풍선, 버튼, 라벨은 이미지에 넣지 않는다.
11. popup UI가 위에 올라와도 읽히도록 중요한 디테일은 가장자리와 하단 UI 영역에 몰아넣지 않는다.
12. 생성 이미지를 outputImagePath에 저장한다.
13. Unity Sprite import용 meta가 필요하면 생성한다.

Output:
- eventId
- imagePath
- spriteName: {popup_event_id}.main
- sourceStageNodeJsonFile
- sourcePopupSummary
- visualPrompt
- imageResolution
- validationResult

실패 시 Output:
- status: failed
- failureType:
  - missing_stage_node_json
  - popup_event_not_found
  - insufficient_popup_context
  - image_generation_failed
  - invalid_image_ratio
  - output_write_failed
- 실패 원인
- 생성하지 않은 산출물
- 보강이 필요한 입력
- 다음에 실행해야 할 프롬프트 또는 수동 작업

검증:
- outputImagePath에 PNG가 존재해야 한다.
- 파일 경로는 `Assets/Resources/stage_new/popup_png/{eventId}.main.png` 규칙을 따라야 한다.
- 파일명에는 stageNodeId를 사용하지 않아야 한다.
- Sprite name은 `{eventId}.main`으로 해석 가능해야 한다.
- 이미지는 popup event의 현재 장면과 맞아야 한다.
- 이미지는 16:9여야 한다.
- UI 텍스트, 자막, 말풍선, 버튼, 라벨이 이미지에 들어가면 안 된다.
- PopupEventBuilder가 Unity import 후 eventId로 이미지를 찾을 수 있어야 한다.

주의:
- 이 프롬프트는 popup main image PNG만 생성한다.
- Stage Node JSON, PopupEventSO asset, RoundNodeSO asset은 수정하지 않는다.
- eventId별 이미지가 필요하므로 여러 popup event 이미지는 eventId마다 반복 실행한다.
- BattleSO 배경 이미지는 BattleBackgroundImagePrompt.md에서 별도로 생성한다.

---

## Embedded Image Style Guide

아래 스타일 가이드는 모든 popup event 메인 이미지 생성에 공통 적용한다.

# Image Style Guide

## Core Prompt
Cinematic anime style illustration with detailed ink linework, rough pixel texture, painterly finish, and muted earthy color palette.

Focus tightly on the key clue and core situation. Keep the surrounding environment softly out of focus.

Use expressive lighting detail inspired by dramatic anime story cuts: warm rim light, soft shadow layers, dust in the air, and natural light filtering through the scene.

Actively infer and reflect the historical period, environment, handmade materials, social class, and everyday life from the story context.

## Composition
- Close-up composition around one key clue and one core situation.
- The key clue is always the main subject.
- Keep the key clue and situation in sharp focus.
- Keep the surrounding environment in soft out-focus style.
- Use environmental storytelling through objects, traces, hands, posture, tools, clothing, and spatial context.
- Background supports the story and period context, never dominates.
- Avoid multiple story beats in one image.
- Remove specific image descriptions from the reusable guide.

## Character Rule
- Include characters only when the situation requires them.
- Do not emphasize faces or facial expressions.
- Avoid portrait-style composition.
- Show emotion through hands, posture, clothing folds, silhouettes, cropped body parts, tools, and interaction with the environment.
- Characters should support the story, never dominate it.
- Keep the key clue and situation as the visual focus.
- Human figures should be treated as environmental storytelling elements, not portrait subjects.
- Characters are supporting elements.
- The key clue is always the main subject.

## Historical Authenticity
- Actively infer and reflect the historical period, environment, social class, and everyday life from the story context.
- Use historically grounded Korean period details when appropriate.
- Use handmade objects only: wood, straw, hemp, bamboo, earthenware, aged hanji, rough cloth.
- Use weathered, imperfect, handcrafted materials and practical tools.
- Show story-relevant props only.
- Every object should support the key clue or core situation.
- Avoid modern objects, clean manufactured items, decorative fantasy props, readable text, and watermark.

## Anime Style
- Cinematic anime illustration.
- Semi-realistic anime rendering.
- Detailed ink linework.
- Rough pixel depiction.
- Painterly finish.
- Muted earthy color palette.
- Lighting detail: warm rim light, soft shadow layers, dust in the air, natural light filtering through the scene.
- Key clue and core situation in focus; surroundings softly out of focus.
- Historical Korean atmosphere grounded in period, environment, handmade materials, and everyday life.

## Automation Notes
- Apply this guide to any story node by replacing only the key clue, core situation, place, and period context.
- Do not include scene-specific examples in this reusable guide.
- Required focus: key clue and core situation in sharp focus; everything else in out-focus style.
- Required style: anime style, expressive lighting detail, active historical and environmental inference, rough pixel depiction, muted earthy color palette.
- Required character handling: face and expression detail removed; characters remain supporting environmental storytelling elements.

```