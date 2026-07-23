# Skill Icon Evaluation Slack Canvas Prompt

완료된 스킬 아이콘 평가 리포트를 공통 Slack Canvas 평가 기록으로
변환할 때 사용합니다. 평가와 프로젝트 복사는 수행하지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

완료된 스킬 아이콘 평가 리포트를 공통 Slack Canvas 폼으로 변환해줘. 평가 폴더의 source 아이콘을 staging 대상으로 기록하고, Pass 후 Unity 목적지와 섞지 마.

Input:
- projectRoot: {project_root}
- formVersion: evaluation_canvas_form_v1
- evaluationDomain: skill
- artifactType: skill_icon
- equipmentId: {skill.domain.character.grade.slot.skill_name}
- artifactId: {equipmentId}
- artifactName: {skill_display_name}
- evaluationReportSource: {완료된_평가_리포트_파일_또는_안정적인_참조}
- inlineEvaluationReport: {인라인_리포트 | null}
- skillSourcePath: {스킬_JSON_경로}
- evaluationRoot: {현재_PC의_기존_스킬_아이콘_평가_루트}
- stagingArtifactPath: {evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png
- iconPath: {evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png
- evaluationWorkspacePath: {evaluationRoot}/{equipmentId}
- projectTargetPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- generationRecordPath: {evaluationRoot}/{equipmentId}/generation_record.txt
- preview32Path: {evaluationRoot}/{equipmentId}/candidates/{selected_preview32_filename}
- promotionStatus: {not_promoted | approved_for_promotion | promoted | blocked}
- promotionApprovalSource: {승인_근거 | null}
- stagingHash: {source_hash | null}
- projectHash: {project_hash | null}
- copyVerification: {Not Performed | Pass | Fail}
- optionalSlot: {slot | null}
- optionalGrade: {grade | null}
- optionalFrameTemplatePath: {path | null}
- optionalNormalizationRecordPath: {path | null}
- optionalExactCountOverlayManifest: {value | null}
- optionalCompositionProfile: {value | null}
- optionalBackgroundMode: {value | null}
- optionalLowerGradeIconPath: {path | null}
- optionalSiblingIconPaths: [{path}]
- reviewDate: {YYYY-MM-DD}
- reviewer: {reviewer_or_agent}
- canvasUpdateMode: {draft_only | append | replace_artifact_section}
- slackCanvasTarget: {workspace_canvas_id_or_url | null}
- slackWriteAuthorized: {false | true}
- localDraftMode: {save | report_only}
- outputLocalCanvasDraftPath: {Assets/Doc/Evaluation/slack_canvas/v1/skill/skill_icon/{equipmentId}.canvas.md | null}

참조 가이드:
- Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationSlackCanvasGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationGuide.md

작업:
1. 공통 가이드와 스킬 확장 가이드를 순서대로 읽는다.
2. evaluationReportSource에서 결과, 총점, fatal failure, severity, six category score, findings, required corrections, 재평가 계획을 그대로 추출한다.
3. equipmentId와 skillSourcePath의 ID가 일치하는지 확인한다.
4. iconPath와 stagingArtifactPath가 같은 preserved source인지 확인한다.
5. stagingArtifactPath, evaluationWorkspacePath, projectTargetPath를 공통 Target Artifact에 각각 기록한다.
6. SkillIconEvaluationGuide의 25/20/20/15/10/10 점수 항목을 Score Breakdown에 그대로 매핑한다.
7. preview32, generation record, normalization, frame, sibling/lower-grade evidence를 Evidence Package와 Domain-Specific Notes에 추가한다.
8. 원본 리포트의 fatal failure와 Critical/Major finding을 생략하지 않는다.
9. 공통 promotion 상태 규칙을 적용한다. promoted이면 source/project copy 및 Unity import 검증 근거가 있어야 한다.
10. 공통 11개 섹션을 이름과 순서를 바꾸지 않고 작성한다.
11. localDraftMode=save이면 canonical local draft 하나만 저장한다. report_only이면 파일을 만들거나 수정하지 않고 Canvas-ready Markdown만 출력한다. 아이콘 평가, 생성, 편집, normalization, 프로젝트 복사, Unity import, Git 작업을 수행하지 않는다.
12. Slack 쓰기는 명시 승인과 도구가 모두 있을 때만 수행한다. 기본값은 draft_only이다.

Output:
- Artifact ID / Skill Source:
- Evaluation Report Source:
- Result / Score / Fatal Failure:
- Staging / Workspace / Project Target Paths:
- Promotion Status / Validation:
- Six Score Categories:
- Required Findings Preserved:
- Local Canvas Draft: {saved_path | not_saved}
- Slack Canvas Update:
- Modified Files:
- Result: Pass | Fail

검증:
- evaluationDomain=skill, artifactType=skill_icon, artifactId=equipmentId여야 한다.
- staging/icon 경로는 `{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png`여야 한다.
- projectTargetPath는 `Assets/Resources/skill/icon/skill/{equipmentId}.icon.png`여야 한다.
- 점수 항목과 합계는 원본 평가 리포트와 같아야 한다.
- staging/project target은 동일하면 안 된다.
- localDraftMode=report_only이면 local draft 파일을 쓰지 않아야 한다.
- 평가나 프로젝트 복사를 수행하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType: {missing_evaluation_report | missing_required_field | invalid_form_version | equipment_id_mismatch | invalid_skill_icon_path | promotion_result_conflict | promotion_verification_missing | staging_target_path_collision | invalid_draft_path | invalid_local_draft_mode | slack_write_not_available | slack_write_not_authorized | output_write_failed}
- failureReason:
- blockedAction:
- unchangedArtifacts:
```
