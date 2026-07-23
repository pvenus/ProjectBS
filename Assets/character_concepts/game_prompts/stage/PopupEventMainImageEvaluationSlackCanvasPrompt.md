# Popup Event Main Image Evaluation Slack Canvas Prompt

완료된 스토리 팝업 메인 이미지 평가 리포트를 공통 Slack Canvas 평가
기록으로 변환할 때 사용합니다. 평가와 프로젝트 복사는 수행하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

완료된 스토리 팝업 메인 이미지 평가 리포트를 공통 Slack Canvas 폼으로 변환해줘. 로컬 평가 source와 Pass 후 프로젝트 popup_png 목적지를 반드시 분리하고 공통 섹션 이름을 바꾸지 마.

Input:
- projectRoot: {project_root}
- formVersion: evaluation_canvas_form_v1
- evaluationDomain: stage
- artifactType: story_popup_main_image
- eventId: {stable_event_id}
- popupId: {stable_popup_id}
- popupName: {popup_name}
- artifactId: {eventId}
- artifactName: {popupName}
- imagePolicy: {generate | reuse | none}
- evaluationReportSource: {완료된_평가_리포트_파일_또는_안정적인_참조}
- inlineEvaluationReport: {인라인_리포트 | null}
- evaluationRoot: {현재_PC의_기존_팝업_이미지_평가_루트}
- stagingArtifactPath: {evaluationRoot}/{eventId}/source/{eventId}.main.png
- imagePath: {evaluationRoot}/{eventId}/source/{eventId}.main.png
- evaluationWorkspacePath: {evaluationRoot}/{eventId}
- projectTargetPath: Assets/Resources/stage_new/popup_png/{eventId}.main.png
- stageNodeJsonFile: {stage_node_json_path}
- episodePlanningFile: {episode_planning_path}
- optionalStoryContextFile: {path | null}
- optionalEpisodeScriptFile: {path | null}
- optionalCharacterReferencePaths: [{path}]
- optionalLocationReferencePaths: [{path}]
- optionalSiblingPopupImagePaths: [{path}]
- optionalReuseSourcePath: {path | null}
- optionalReuseSourceHash: {hash | null}
- promotionStatus: {not_promoted | approved_for_promotion | promoted | blocked | not_applicable}
- promotionApprovalSource: {승인_근거 | null}
- stagingHash: {source_hash | null}
- projectHash: {project_hash | null}
- copyVerification: {Not Performed | Pass | Fail | Not Applicable}
- reviewDate: {YYYY-MM-DD}
- reviewer: {reviewer_or_agent}
- canvasUpdateMode: {draft_only | append | replace_artifact_section}
- slackCanvasTarget: {workspace_canvas_id_or_url | null}
- slackWriteAuthorized: {false | true}
- localDraftMode: {save | report_only}
- outputLocalCanvasDraftPath: {Assets/Doc/Evaluation/slack_canvas/v1/stage/story_popup_main_image/{eventId}.canvas.md | null}

참조 가이드:
- Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
- Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageEvaluationSlackCanvasGuide.md
- Assets/character_concepts/game_prompt_guide/stage/PopupEventMainImageEvaluationGuide.md

작업:
1. 공통 가이드와 스토리 팝업 확장 가이드를 순서대로 읽는다.
2. evaluationReportSource에서 결과, 총점, fatal failure, severity, nine category score, findings, required corrections, optional improvements, 재평가 계획을 그대로 추출한다.
3. eventId, popupId, popupName, imagePolicy가 stage node 및 planning evidence와 일치하는지 기록 수준에서 확인한다. 새 평가 판단은 하지 않는다.
4. generate/reuse이면 imagePath와 stagingArtifactPath가 같은 local preserved source인지 확인한다. none이면 두 artifact path를 Not Applicable로 바꾸고 SKIPPED/not_applicable만 허용한다.
5. stagingArtifactPath, evaluationWorkspacePath, projectTargetPath를 공통 Target Artifact에 각각 기록한다.
6. PopupEventMainImageEvaluationGuide의 20/15/15/10/15/10/5/5/5 점수 항목을 Score Breakdown에 그대로 매핑한다.
7. planning, stage node, story context, script, character/location, sibling/reuse evidence를 Evidence Package와 Domain-Specific Notes에 짧게 기록한다.
8. fatal story contradiction과 Critical/Major finding을 생략하거나 optional로 낮추지 않는다.
9. 공통 promotion 상태 규칙을 적용한다. promoted이면 source/project copy 및 Unity import 검증 근거가 있어야 한다.
10. 공통 11개 섹션을 이름과 순서를 바꾸지 않고 작성한다.
11. localDraftMode=save이면 canonical local draft 하나만 저장한다. report_only이면 파일을 만들거나 수정하지 않고 Canvas-ready Markdown만 출력한다. 이미지 평가, 생성, 편집, 프로젝트 복사, Unity import, story/stage 수정, Git 작업을 수행하지 않는다.
12. Slack 쓰기는 명시 승인과 도구가 모두 있을 때만 수행한다. 기본값은 draft_only이다.

Output:
- Event / Popup Identity:
- Evaluation Report Source:
- Result / Score / Fatal Failure:
- Staging / Workspace / Project Target Paths:
- Promotion Status / Validation:
- Nine Score Categories:
- Required Findings Preserved:
- Local Canvas Draft: {saved_path | not_saved}
- Slack Canvas Update:
- Modified Files:
- Result: Pass | Fail

검증:
- evaluationDomain=stage, artifactType=story_popup_main_image, artifactId=eventId여야 한다.
- generate/reuse의 staging/image 경로는 `{evaluationRoot}/{eventId}/source/{eventId}.main.png`여야 한다.
- projectTargetPath는 `Assets/Resources/stage_new/popup_png/{eventId}.main.png`여야 한다.
- 점수 항목과 합계는 원본 평가 리포트와 같아야 한다.
- none은 SKIPPED/not_applicable이어야 한다.
- staging/project target은 동일하면 안 된다.
- localDraftMode=report_only이면 local draft 파일을 쓰지 않아야 한다.
- 평가나 프로젝트 복사를 수행하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType: {missing_evaluation_report | missing_required_field | invalid_form_version | event_id_mismatch | invalid_popup_image_path | invalid_image_policy | promotion_result_conflict | promotion_verification_missing | staging_target_path_collision | invalid_draft_path | invalid_local_draft_mode | slack_write_not_available | slack_write_not_authorized | output_write_failed}
- failureReason:
- blockedAction:
- unchangedArtifacts:
```
