# Episode Planning Prompt

Use this prompt when converting episode prose into synopsis-level planning JSON.

## Prompt

```text
작업 폴더 = {project_root}

EpisodeFormat.md, StoryStructureGuide.md, StoryPlanningContextGuide.md,
EpisodePlanningCreateGuide.md, RewardPlanningGuide.md 기준으로 에피소드
기획 JSON을 생성해줘.

참조 가이드:
- Assets/Doc/Design/EpisodeFormat.md
- Assets/character_concepts/game_prompt_guide/story/StoryStructureGuide.md
- Assets/character_concepts/game_prompt_guide/story/StoryPlanningContextGuide.md
- Assets/character_concepts/game_prompt_guide/story/EpisodePlanningCreateGuide.md
- Assets/character_concepts/game_prompt_guide/story/RewardPlanningGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- chapterId: {chapter_id}
- actGroupId: {act_group_id}
- episodeId: {episode_id}
- episodeFile: {episode_markdown_path}
- chapterFile: {chapter_markdown_path}
- storyReferenceFiles:
  - Assets/Doc/Story/00_Background.md
  - Assets/Doc/Story/Act01/01_Overall_Story.md
  - Assets/Doc/Story/Act01/Act_01_Background.md
  - Assets/Doc/Story/Characters.md

작업:
1. episodeFile 원문을 읽고 시놉시스 수준으로 요약한다. 동시에 원문 지문을 의미 단위 block으로 나누되 `originalTextKo`는 축약·윤문·UI 개행 없이 그대로 보존하고 각 block에 영구 `sourceNarrationId`를 부여한다.
2. 원문에 있는 플레이어/파티 기준을 확인한다. 근거가 없으면 임의로
   2인/3인 파티를 넣지 않는다.
3. common/story/monster/battle/reward/handoff 카테고리로 episode JSON을 만들고 `story.sourceNarration`과 `story.popupDefinitions`를 포함한다.
4. 공용 JSON, story_context JSON, episode_composition JSON을 생성 또는 갱신한다.
5. 이 단계에서는 정확한 spawner, spawn count, BattleSO, CharacterSO를 만들지 않는다.
6. 전투가 필요하면 battleMonsterPoolRef가 들어갈 위치를 만든다.
7. 플레이어에게 표시될 각 popup에 에피소드 안에서 고유한 의미 기반 `popupName`을 부여한다. `popup_1`, `scene_2`, `event_003`처럼 배열 위치나 순번만 나타내는 이름은 금지한다.
8. `popupId`는 `node.{act_key}.{chapter_key}.{episode_key}.{popupName}`으로 생성하고 `popupOrder`는 별도 순서 값으로 둔다.
9. 선형 연결과 분기 연결은 planning `popupId`를 사용하는 `nextPopupId`로 작성하고 마지막 popup은 null로 둔다. 배열 위치나 숫자 종료값 0을 사용하지 않는다.
10. 각 popupDefinition에 popupType, sourceNarrationIds, imagePolicy와 필요한 imageDirection을 기록한다. imagePolicy는 generate/reuse/none 중 하나다. reuse이면 기존 영구 popupId를 `imageSourcePopupId`로 명시한다.
11. 선택지는 의미 기반 `choiceName`을 부여하고 `choiceId`를 `choice.{act_key}.{chapter_key}.{episode_key}.{popupName}.{choiceName}`으로 생성한다.
12. 기존 planning JSON을 갱신할 때 기존 popupName, popupId, choiceId를 보존한다. 기존 순번형 ID는 레거시 영구 ID로 유지하고 신규 데이터에만 의미 기반 규칙을 적용한다.
13. Stage 표시본이 9줄×40자를 넘을 것으로 예상되어 popup 분할이 필요하면 이 단계에서 각 분할 popup에 별도 의미 기반 이름과 이미지 정책을 지정한다.

Output:
- 생성/수정한 JSON 경로
- episode battleNeed
- partyAssumption 판단 근거
- monster direction 요약
- battle direction 요약
- reward direction 요약
- 다음 단계에서 필요한 작업
- sourceNarrationId 목록
- popupName / popupId / popupOrder 목록
- popupType / imagePolicy 목록
- choiceName / choiceId 목록
- 검증 결과

실패 시 Output:
- status: failed
- failureType:
  - missing_episode_file
  - missing_chapter_file
  - invalid_episode_format
  - invalid_popup_name
  - duplicate_popup_name
  - invalid_popup_id
  - source_narration_mismatch
  - invalid_next_popup_id
  - invalid_choice_name
  - duplicate_choice_id
  - legacy_id_renumbered
  - missing_image_source_popup_id
  - insufficient_story_basis
  - invalid_json
- 실패 원인
- 보강이 필요한 원문/참조 문서
- 다음 단계 실행 가능 여부

검증:
- JSON 문법이 유효해야 한다.
- sourceEpisodeFile, sourceChapterFile이 존재해야 한다.
- 시놉시스가 정식 스크립트 수준의 대사/지문으로 확장되지 않아야 한다.
- sourceNarration.blocks[].originalTextKo는 episodeFile 원문과 동일해야 한다.
- 신규 popupName은 에피소드 안에서 고유한 의미 기반 snake_case여야 한다.
- 신규 popupId는 popupName 기반 공식 생성식과 정확히 일치해야 한다.
- popupOrder 변경이 popupName 또는 popupId를 변경하지 않아야 한다.
- 신규 nextPopupId는 같은 planning 문서의 실제 popupId 또는 의도된 외부 영구 popupId를 가리키고 terminal이면 null이어야 한다.
- 신규 choiceId는 popupName과 의미 기반 choiceName으로 생성되어야 한다.
- 기존 레거시 popup/choice ID를 재번호화하지 않아야 한다.
- partyAssumption은 원문 근거가 있어야 한다.
- battle에는 정확한 spawner type, spawnSequenceId, spawn count가 없어야 한다.
- reward는 현재 gold 기준만 사용한다.

주의:
- 이 단계는 시놉시스 수준의 1차 기획 JSON 생성 단계다.
- 정식 스크립트, PopupEventSO JSON, RoundNodeSO JSON, BattleSO JSON은 생성하지 않는다.
- 몬스터가 이미 존재한다고 전제하지 말고 몬스터 생성 전 방향성만 기록한다.
```
