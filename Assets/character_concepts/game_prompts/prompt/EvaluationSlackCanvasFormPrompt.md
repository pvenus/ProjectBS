# Evaluation Slack Canvas Form Prompt

완료된 평가 리포트를 도메인 중립적인 Slack Canvas 평가 기록으로 변환할
때 사용합니다. 이 프롬프트는 평가, 수정, 승격 복사, Git 작업을 수행하지
않습니다.

## Prompt

```text
작업 폴더 = {project_root}

완료된 평가 리포트를 공통 Slack Canvas 평가 기록 폼으로 변환해줘. 원본 평가 결과와 점수는 변경하지 말고, 로컬 평가 대상 경로와 Pass 후 프로젝트 반영 경로를 반드시 분리해 기록해줘.

Input:
- projectRoot: {project_root}
- formVersion: evaluation_canvas_form_v1
- evaluationDomain: {lowercase_snake_case}
- artifactType: {lowercase_snake_case}
- artifactId: {stable_file_safe_id}
- artifactName: {display_name}
- evaluationReportSource: {완료된_평가_리포트_파일_또는_안정적인_참조}
- inlineEvaluationReport: {인라인_평가_리포트 | null}
- primaryEvaluationGuide: {평가_가이드_상대경로}
- domainSlackCanvasGuide: {도메인_확장_가이드_상대경로 | null}
- stagingArtifactPath: {실제로_평가한_로컬_복사본}
- evaluationWorkspacePath: {후보_리포트_프리뷰_hash_근거_폴더}
- projectTargetPath: {Pass_후_복사될_프로젝트_상대경로}
- promotionStatus: {not_promoted | approved_for_promotion | promoted | blocked | not_applicable}
- promotionApprovalSource: {명시_승인_근거 | null}
- stagingHash: {hash | null}
- projectHash: {hash | null}
- copyVerification: {Not Performed | Pass | Fail | Not Applicable}
- sourceDataFiles: [{path}]
- referenceEvidencePaths: [{path}]
- domainSpecificFields: {key_value_map}
- passCriteria: {도메인의_정확한_통과_기준}
- reviewDate: {YYYY-MM-DD}
- reviewer: {reviewer_or_agent}
- canvasUpdateMode: {draft_only | append | replace_artifact_section}
- slackCanvasTarget: {workspace_canvas_id_or_url | null}
- slackWriteAuthorized: {false | true}
- localDraftMode: {save | report_only}
- outputLocalCanvasDraftPath: {Assets/Doc/Evaluation/slack_canvas/v1/{evaluationDomain}/{artifactType}/{artifactId}.canvas.md | null}

참조 가이드:
- Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
- {domainSlackCanvasGuide | 생략 가능}

작업:
1. 공통 가이드를 먼저 읽고 formVersion과 11개 필수 섹션 계약을 확인한다.
2. domainSlackCanvasGuide가 있으면 공통 계약을 제거하거나 이름을 바꾸지 않는 추가 규칙으로만 적용한다.
3. evaluationReportSource에서 완료된 평가 결과, 총점, category score, Hard Fail, severity, findings, required actions, optional improvements, 재평가 계획을 추출한다. inlineEvaluationReport는 source 내용을 제공하는 용도이며 source 식별자를 대체하지 않는다.
4. 결과·점수·severity·finding을 요약 과정에서 재평가하거나 변경하지 않는다.
5. stagingArtifactPath가 실제 평가 대상, evaluationWorkspacePath가 근거 보존 폴더, projectTargetPath가 최종 프로젝트 목적지인지 확인한다.
6. stagingArtifactPath와 projectTargetPath가 같은 파일이면 명시적인 domain in-place policy가 없는 한 `staging_target_path_collision`로 중단하고 promotionStatus를 blocked로 보고한다.
7. 결과와 promotionStatus의 호환성을 검증한다.
   - FAIL: not_promoted 또는 blocked만 허용
   - CONDITIONAL_PASS: 명시 승인 전 not_promoted/blocked, 승인 후 approved_for_promotion, 검증된 복사 후 promoted
   - PASS: 복사 전 not_promoted 또는 approved_for_promotion, 검증된 복사 후 promoted
   - SKIPPED: not_applicable
8. promoted이면 projectTargetPath 존재 및 staging/project hash 또는 동등한 복사 검증 근거가 있어야 한다. 이 프롬프트가 직접 복사하거나 상태를 승격시키지는 않는다.
9. Current PC의 외부 evaluation path는 정확히 보존하되 다른 PC에서 전달된 절대 경로를 사용하지 않는다. projectTargetPath와 local draft path는 프로젝트 상대경로로 기록한다.
10. 공통 섹션을 정확히 다음 순서로 작성한다: Record Metadata, Result Summary, Target Artifact, Evidence Package, Score Breakdown, Findings, Required Actions, Optional Improvements, Domain-Specific Notes, Re-evaluation Plan, Change Log.
11. localDraftMode=save이면 outputLocalCanvasDraftPath에 Canvas Markdown draft 하나만 저장한다. report_only이면 파일을 만들거나 수정하지 않고 Canvas-ready Markdown만 출력한다. 평가 리포트, artifact, 프로젝트 리소스는 수정하지 않는다.
12. canvasUpdateMode=draft_only이면 Slack 도구를 호출하지 않는다.
13. append/replace_artifact_section은 slackWriteAuthorized=true이고 target이 명확하며 Slack Canvas 도구가 있을 때만 수행한다. 아니면 draft만 보존하고 미게시 사유를 출력한다.

Output:
- Form Version:
- Evaluation Domain / Artifact Type / Artifact ID:
- Evaluation Report Source:
- Result / Overall Score / Highest Severity:
- Staging Artifact Path:
- Evaluation Workspace Path:
- Project Target Path:
- Promotion Status / Validation:
- Local Canvas Draft: {saved_path | not_saved}
- Slack Canvas Update: not_requested | posted | skipped
- Required Section Check:
- Result Preservation Check:
- Path Separation Check:
- Modified Files:
- Result: Pass | Fail

검증:
- formVersion은 evaluation_canvas_form_v1이어야 한다.
- draft path는 Assets/Doc/Evaluation/slack_canvas/v1/{evaluationDomain}/{artifactType}/{artifactId}.canvas.md여야 한다.
- 필수 공통 필드와 11개 섹션이 모두 존재해야 한다.
- 원본 평가 결과, 점수, severity, findings가 유지되어야 한다.
- staging/evaluation/project target의 역할이 섞이지 않아야 한다.
- promotionStatus는 평가 결과 및 승인·복사 검증 근거와 일치해야 한다.
- FAIL은 promoted 또는 approved_for_promotion일 수 없다.
- promoted는 검증 근거 없이 기록할 수 없다.
- localDraftMode=report_only이면 local draft 파일을 쓰지 않아야 한다.
- 이 프롬프트는 평가, artifact 수정, 프로젝트 복사, Git 작업을 수행하지 않는다.

실패 시 Output:
- status: failed
- failureType: {missing_evaluation_report | missing_required_field | missing_form_guide | invalid_form_version | invalid_result | invalid_promotion_status | promotion_result_conflict | promotion_verification_missing | staging_target_path_collision | invalid_draft_path | invalid_local_draft_mode | invalid_canvas_target | slack_write_not_available | slack_write_not_authorized | artifact_section_not_found | unsupported_canvas_update_mode | output_write_failed}
- failureReason:
- missingInputs:
- blockedAction:
- unchangedArtifacts:
```
