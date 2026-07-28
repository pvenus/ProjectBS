# Item Icon Evaluation Slack Canvas Prompt

완료된 아이템 아이콘 평가 리포트를 공통 Slack Canvas 평가 기록으로
변환할 때 사용합니다. 평가와 프로젝트 복사는 수행하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

완료된 아이템 아이콘 평가 리포트를 공통 Slack Canvas 폼으로 변환해줘. 평가 폴더의 source 아이콘과 Pass 후 Unity 목적지를 반드시 분리하고, raw 2x2 시트를 최종 아이콘으로 기록하지 마.

Input:
- projectRoot: {project_root}
- formVersion: evaluation_canvas_form_v1
- evaluationDomain: item
- artifactType: item_icon
- itemId: {item.category.lowercase_snake_case_slug}
- itemCategory: {relic | future_item_category}
- artifactId: {itemId}
- artifactName: {item_display_name}
- evaluationReportSource: {완료된_평가_리포트_파일_또는_안정적인_참조}
- inlineEvaluationReport: {인라인_리포트 | null}
- itemSourcePath: {승인된_아이템_기획_또는_JSON_경로}
- optionalItemPlanningFile: {path | null}
- evaluationRoot: {현재_PC의_기존_아이템_아이콘_평가_루트}
- stagingArtifactPath: {evaluationRoot}/{itemId}/source/{itemId}.icon.png
- iconPath: {evaluationRoot}/{itemId}/source/{itemId}.icon.png
- evaluationWorkspacePath: {evaluationRoot}/{itemId}
- projectTargetPath: Assets/Resources/item/icon/{itemId}.icon.png
- generationRecordPath: {evaluationRoot}/{itemId}/evaluation/generation_record.txt
- candidateScoresPath: {evaluationRoot}/{itemId}/evaluation/candidate_scores.txt
- selectedCandidatePath: {evaluationRoot}/{itemId}/candidates/{selected_candidate_filename}
- preview64Path: {evaluationRoot}/{itemId}/candidates/{selected_preview64_filename}
- optionalRawSheetPath: {path | null}
- optionalLegacyReferencePaths: [{path}]
- optionalCompositionProfile: {value | null}
- optionalEffectCue: {value | null}
- promotionStatus: {not_promoted | approved_for_promotion | promoted | blocked}
- promotionApprovalSource: {승인_근거 | null}
- stagingHash: {source_hash | null}
- projectHash: {project_hash | null}
- copyVerification: {Not Performed | Pass | Fail}
- reviewDate: {YYYY-MM-DD}
- reviewer: {reviewer_or_agent}
- canvasUpdateMode: {draft_only | append | replace_artifact_section}
- slackCanvasTarget: {workspace_canvas_id_or_url | null}
- slackWriteAuthorized: {false | true}
- localDraftMode: {save | report_only}
- outputLocalCanvasDraftPath: {Assets/Doc/Evaluation/slack_canvas/v1/item/item_icon/{itemId}.canvas.md | null}

참조 가이드:
- Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
- Assets/character_concepts/game_prompt_guide/item/ItemIconEvaluationSlackCanvasGuide.md
- Assets/character_concepts/game_prompt_guide/item/ItemIconGenerationGuide.md

작업:
1. 공통 가이드와 아이템 확장 가이드를 순서대로 읽는다.
2. evaluationReportSource에서 결과, 총점, rejection condition, severity, category score, findings, required actions, 재평가 계획을 그대로 추출한다.
3. itemId가 item source와 일치하는지 확인한다.
4. iconPath와 stagingArtifactPath가 동일한 preserved 128x128 source인지 확인한다.
5. stagingArtifactPath, evaluationWorkspacePath, projectTargetPath를 공통 Target Artifact에 각각 기록한다.
6. 전용 ItemIconEvaluationGuide가 없으면 ItemIconGenerationGuide의 30/20/20/15/15 점수 항목을 Score Breakdown에 매핑한다. 리포트가 승인된 후속 평가 가이드를 사용했다면 그 이름과 점수표를 그대로 사용한다.
7. raw sheet, selected candidate, candidate scores, preview64, generation record, legacy reference를 Evidence Package와 Domain-Specific Notes에 추가한다.
8. rejection condition과 Critical/Major finding을 생략하지 않는다.
9. 공통 promotion 상태 규칙을 적용한다. promoted이면 source/project byte identity 및 Unity import 근거가 있어야 한다.
10. 공통 11개 섹션을 이름과 순서를 바꾸지 않고 작성한다.
11. localDraftMode=save이면 canonical local draft 하나만 저장한다. report_only이면 파일을 만들거나 수정하지 않고 Canvas-ready Markdown만 출력한다. 아이콘 평가, 생성, 후보 추출, 프로젝트 복사, Unity import, Git 작업을 수행하지 않는다.
12. Slack 쓰기는 명시 승인과 도구가 모두 있을 때만 수행한다. 기본값은 draft_only이다.

Output:
- Artifact ID / Item Source:
- Evaluation Report Source / Rubric:
- Result / Score / Rejection Condition:
- Staging / Workspace / Project Target Paths:
- Promotion Status / Validation:
- Score Categories:
- Required Findings Preserved:
- Local Canvas Draft: {saved_path | not_saved}
- Slack Canvas Update:
- Modified Files:
- Result: Pass | Fail

검증:
- evaluationDomain=item, artifactType=item_icon, artifactId=itemId여야 한다.
- staging/icon 경로는 `{evaluationRoot}/{itemId}/source/{itemId}.icon.png`여야 한다.
- projectTargetPath는 `Assets/Resources/item/icon/{itemId}.icon.png`여야 한다.
- 점수표와 합계는 cited evaluation rubric과 같아야 한다.
- raw 2x2 sheet와 legacy atlas는 최종 아이콘으로 기록하지 않아야 한다.
- staging/project target은 동일하면 안 된다.
- localDraftMode=report_only이면 local draft 파일을 쓰지 않아야 한다.
- 평가나 프로젝트 복사를 수행하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType: {missing_evaluation_report | missing_required_field | invalid_form_version | item_id_mismatch | invalid_item_icon_path | invalid_item_rubric | promotion_result_conflict | promotion_verification_missing | staging_target_path_collision | invalid_draft_path | invalid_local_draft_mode | slack_write_not_available | slack_write_not_authorized | output_write_failed}
- failureReason:
- blockedAction:
- unchangedArtifacts:
```
