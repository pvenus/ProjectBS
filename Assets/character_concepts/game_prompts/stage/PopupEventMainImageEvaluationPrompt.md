# Popup Event Main Image Evaluation Prompt

프로젝트 반영 전 로컬에 보존된 스토리 팝업 메인 이미지 하나를 읽기
전용으로 평가할 때 사용합니다.

## Prompt

```text
작업 폴더 = {project_root}

로컬 평가 source에 보존된 스토리 팝업 메인 이미지 하나를 평가해줘. 프로젝트 popup_png 목적지는 향후 Pass 승격 경로로만 기록하고 이미지 복사는 하지 마.

Input:
- projectRoot: {project_root}
- eventId: {stable_event_id}
- popupId: {stable_popup_id}
- popupName: {popup_name}
- imagePolicy: {generate | reuse | none}
- evaluationRoot: {현재_PC의_기존_팝업_이미지_평가_루트}
- stagingArtifactPath: {evaluationRoot}/{eventId}/source/{eventId}.main.png
- evaluationWorkspacePath: {evaluationRoot}/{eventId}
- projectTargetPath: Assets/Resources/stage_new/popup_png/{eventId}.main.png
- stageNodeJsonFile: {stage_node_json_path}
- episodePlanningFile: {episode_planning_path}
- optionalStoryContextFile: {path | null}
- optionalEpisodeScriptFile: {path | null}
- optionalCharacterReferencePaths: [{path}]
- optionalLocationReferencePaths: [{path}]
- optionalSiblingPopupImagePaths: [{path}]
- optionalStyleReferenceImagePaths: [{path}]
- optionalReuseSourcePath: {path | null}
- optionalReuseSourceHash: {hash | null}
- optionalStagingHash: {hash | null}
- outputEvaluationReportPath: {evaluationWorkspacePath}/evaluation/evaluation_report.md

참조 가이드:
- Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageEvaluationGuide.md
- Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageCreateGuide.md
- Assets/character_concepts/game_prompt_guide/stage/StoryImageVisualGuide.md
- Assets/character_concepts/game_prompt_guide/stage/StoryImageElementGuide.md
- Assets/character_concepts/game_prompt_guide/stage/PopupEventSO.md
- Assets/character_concepts/game_prompt_guide/stage/EpisodeStageNodeCreateGuide.md

작업:
1. PopupEventMainImageEvaluationGuide.md를 먼저 읽는다.
2. stagingArtifactPath와 projectTargetPath가 같은 파일이면 process violation으로 중단한다.
3. imagePolicy=none이면 이미지 점수를 만들지 않고 SKIPPED로 기록한다.
4. stage node와 episode planning에서 eventId, popupId, popupName, imagePolicy 및 planned moment를 확인한다.
5. stagingArtifactPath의 이미지만 읽기 전용으로 검사한다.
6. optional evidence는 존재할 때만 사용하고, 없으면 관련 항목을 Not Evaluated로 기록한다.
7. reuse이면 승인된 reuse intent와 제공된 source/staging hash 동일성을 확인한다.
8. fatal failure를 먼저 확인한 뒤 20/15/15/10/15/10/5/5/5 항목을 평가한다.
9. confirmed evidence와 inference를 분리하고 Critical/Major/Minor/Suggestion finding을 작성한다.
10. required action과 optional improvement를 분리하고 재평가 trigger를 제시한다.
11. outputEvaluationReportPath에 평가 리포트 하나만 저장한다.
12. 이미지 생성·편집·이름 변경·프로젝트 복사·Unity import·promotion·source 문서 수정·Git 작업을 수행하지 않는다.

Output:
- Event / Popup Identity:
- Staging / Workspace / Project Target Paths:
- Evidence Reviewed:
- Result / Overall Score / Hard Fail:
- Nine Category Scores:
- Findings by Severity:
- Required Actions:
- Optional Improvements:
- Re-evaluation Plan:
- Evaluation Report Path:
- Modified Files:

검증:
- staging source와 project target이 분리되어야 한다.
- category 합계는 overall score와 같아야 한다.
- PASS는 90점 이상, fatal failure 없음, Major/Critical 없음이어야 한다.
- CONDITIONAL_PASS는 80-89점, fatal failure 없음, Minor/Suggestion만 허용한다.
- FAIL은 80점 미만, fatal failure 또는 Major/Critical이 있는 경우다.
- imagePolicy=none은 SKIPPED다.
- 평가 리포트 외 파일은 수정하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType: {missing_staging_image | unreadable_image | missing_stage_node_json | missing_episode_planning_file | popup_event_not_found | popup_definition_not_found | event_id_mismatch | invalid_image_policy | missing_reuse_source | checksum_mismatch | insufficient_story_context | insufficient_visual_context | staging_target_path_collision | report_write_failed}
- failureReason:
- unevaluatedItems:
- requiredAdditionalInput:
- unchangedArtifacts:
```
