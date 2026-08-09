# Skill Icon Evaluation Slack Canvas Prompt


스킬 아이콘을 기존 정식 평가 계약으로 평가한 뒤, 결과를 손실 없이
Slack Canvas 형식으로 변환하는 실행 프롬프트입니다. 이미 완료된 평가를
Canvas로만 변환하는 모드도 지원합니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 순서대로 읽고 스킬 아이콘 평가와 Slack Canvas 기록을 처리해줘.
평가 기준은 SkillIconEvaluationGuide.md를 그대로 사용하고, Canvas 변환 과정에서
점수·fatal failure·finding·수정 방법·promotion 상태를 다시 해석하거나 완화하지 마.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/skill/SkillIconEvaluationGuide.md
- AgentDocs/planning-guides/skill/SkillIconEvaluationSlackCanvasGuide.md
- AgentDocs/planning-guides/prompt/EvaluationSlackCanvasFormGuide.md
- AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md
- AgentDocs/planning-guides/skill/data-structures/SkillJsonGuide.md
- AgentDocs/planning-guides/skill/data-structures/EquipmentSkillSO.md
- AgentDocs/planning-guides/skill/design/SkillDegineGuide.md

Input:
- projectRoot: {현재_PC에서_확인한_ProjectBS-agent_루트}
- workflowMode: {evaluate_and_format | format_existing}
- evaluationRoot: {current_pc_skill_icon_evaluation_root}
- equipmentId: {skill.domain.character.grade.slot.skill_name}
- artifactName: {skill_display_name}
- skillSourcePath: {현재_PC의_스킬_JSON_절대경로}
- stagingArtifactPath: {evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png
- generationRecordPath: {현재_PC의_생성_기록_절대경로}
- preview32Path: {현재_PC의_nearest-neighbor_32x32_미리보기_절대경로}
- frameTemplatePath: {현재_PC의_80x80_프레임_템플릿_절대경로 | null}
- normalizationRecordPath: {현재_PC의_normalization_기록_절대경로 | null}
- exactCountOverlayManifest: {manifest_경로_또는_값 | null}
- lowerGradeIconPath: {auto | 현재_PC의_절대경로 | null}
- siblingIconPaths: {auto | 현재_PC의_절대경로_목록 | null}
- intendedUnityIconPath: Assets/ImagesGenerated/Skill/icon/{equipmentId}.icon.png
- evaluationReportSource: {format_existing일_때_완료_리포트_경로_또는_안정적_참조 | null}
- promotionStatus: {not_promoted | approved_for_promotion | promoted | blocked}
- promotionApprovalSource: {명시적_승인_근거 | null}
- stagingHash: {sha256 | auto}
- projectHash: {sha256 | null}
- copyVerification: {Not Performed | Pass | Fail}
- reviewDate: {YYYY-MM-DD}
- reviewer: {reviewer_or_agent}
- formVersion: evaluation_canvas_form_v1
- canvasEvidenceMode: {self_contained | metadata_only}
- canvasUpdateMode: {draft_only | append | replace_artifact_section}
- slackCanvasTarget: {workspace_canvas_id_or_url | null}
- slackEvidenceConversation: {channel_id_or_conversation_id | null}
- slackWriteAuthorized: {false | true}
- localDraftMode: {save | report_only}

고정 결과 경로:
- evaluationWorkspacePath: {evaluationRoot}/{equipmentId}
- evaluationReportPath: {evaluationWorkspacePath}/evaluation/evaluation_report.md
- evaluationResultPath: {evaluationWorkspacePath}/evaluation/evaluation_result.json
- outputLocalCanvasDraftPath: {evaluationWorkspacePath}/evaluation/evaluation_canvas.md
- projectTargetPath: Assets/ImagesGenerated/Skill/icon/{equipmentId}.icon.png

작업:
1. projectRoot, evaluationRoot와 모든 절대 경로를 현재 PC에서 다시 확인한다.
   다른 PC의 사용자명·홈 디렉터리·임시 visualizations 경로를 재사용하거나 추정하지 않는다.
2. evaluationRoot는 현재 PC에서 확인한 기존 스킬 아이콘 평가 루트여야 한다.
   stagingArtifactPath는 그 아래의 정확한 preserved source여야 하며 projectTargetPath와
   같은 파일로 해석되면 즉시 blocked 처리한다.
3. `workflowMode=evaluate_and_format`이면 SkillIconEvaluationGuide.md의 전체 계약으로
   normalized final source를 읽기 전용 평가한다. 다음 항목을 반드시 유지한다.
   - 80x80 RGBA 단일 PNG, equipmentId/JSON/경로/SHA-256 검증
   - create_ui_pro, 4x4/16개 변형, generation record와 current-PC frame template 증거
   - source JSON 기반 direction, composition, primary fragment, mandatory effect,
     exact-count 요소 재분류
   - 부분 물체가 완전한 머리·인물·생물·제단으로 복원되지 않았는지 확인
   - rows/columns 0,1,78,79 frame 일치, background mode, 중앙 64x64 safe area,
     primary 크기·선 두께·간격·particle·arc 두께 확인
   - nearest-neighbor 32x32 생존성, lower-grade/sibling 비교와 duplicate hash 확인
   - fatal failure를 점수보다 먼저 판정
   - 25/20/20/15/10/10의 여섯 점수와 총점
   - 85점 이상, fatal failure 없음, 필수 증거 부족 없음일 때만 PASS
   - 각 감점/실패마다 관찰 증거, 기대 규칙, 필수 수정, regeneration 여부,
     단 하나의 correction method 기록
4. 평가 결과를 evaluation_report.md에 저장하고 동일 사실을 구조화한
   evaluation_result.json을 저장한다. 이미지, JSON source, `.meta`, Unity project,
   생성 기록과 기존 증거 파일은 수정하지 않는다.
5. `workflowMode=format_existing`이면 evaluationReportSource를 필수로 읽고,
   원본 결과를 재채점하거나 고치지 않는다. source report와 staging SHA-256이 현재
   workspace의 증거와 일치하지 않으면 변환을 중단한다.
6. 평가 결과를 EvaluationSlackCanvasFormGuide.md의 공통 11개 섹션과 정확한 순서로
   변환한다. Result Summary에는 원본 Result/Total/Fatal Failure/Highest Severity를,
   Score Breakdown에는 여섯 점수를 그대로 기록한다.
7. Findings에서 fatal failure와 모든 Critical/Major finding을 생략하지 않는다.
   Required Actions에는 원본 correction method와 minimal prompt or pipeline change,
   regeneration required를 유지한다.
8. Evidence Package에는 source JSON, preserved icon/SHA-256, generation record,
   32x32 preview, frame template, normalization, exact-count manifest,
   sibling/lower-grade 증거를 각각 구분해 기록한다. 없는 선택 증거는
   `Not Provided` 또는 `Not Evaluated`로 표시하고 Pass로 추정하지 않는다.
9. localDraftMode=save이면 evaluation_canvas.md 하나를 canonical local draft로 저장한다.
   report_only이면 파일을 생성하거나 수정하지 않고 Canvas-ready Markdown만 출력한다.
10. canvasEvidenceMode=self_contained이면 Slack 게시 전에 실제 evaluated icon을 Slack에
    업로드하고 workspace-accessible file reference를 Canvas 최상위 이미지 요소로 사용한다.
    관련 skill JSON 원문 블록과 평가 요약을 함께 남긴다. 로컬 절대 경로는 provenance
    metadata일 뿐 Canvas 이미지 링크로 사용하지 않는다.
11. Slack 쓰기는 `slackWriteAuthorized=true`, 명확한 slackCanvasTarget,
    self_contained일 때 업로드 가능한 slackEvidenceConversation과 필요한 Slack 도구가
    모두 있을 때만 수행한다. 그 외에는 draft_only로 종료한다.
12. Canvas 작성은 아이콘 생성·편집·normalization·Unity 복사/import·Git 작업을
    수행하거나 승인하지 않는다.

평가 결과 JSON 필수 필드:
- schemaVersion: "1.0"
- equipmentId
- skillSourcePath
- stagingArtifactPath
- sha256
- dimensions
- fatalFailure
- fatalFailureChecks
- scores:
  - skillIntentReadability: {score, max: 25}
  - projectStyleMatch: {score, max: 20}
  - smallSizeSilhouette: {score, max: 20}
  - slotAndGradeDistinction: {score, max: 15}
  - paletteAndContrast: {score, max: 10}
  - compositionAndBorderQuality: {score, max: 10}
- totalScore
- result: {PASS | CONDITIONAL_PASS | FAIL}
- highestSeverity
- findings
- requiredActions
- correctionMethods
- regenerationRequired
- promotionStatus
- passForUnityCopy
- evidence
- evaluatedAt
- reviewer

Output:
- Skill ID / Source JSON:
- Workflow Mode:
- Evaluation Workspace:
- Preserved Icon / SHA-256:
- Evaluation Report / Structured Result:
- Result / Score / Fatal Failure:
- Six Score Categories:
- Required Findings Preserved:
- Promotion Status / Validation:
- Local Canvas Draft: {saved_path | not_saved}
- Slack Evidence Upload:
- Slack Canvas Update:
- Modified Files:
- Unity Project Changes: None

검증:
- 원본 평가의 fatal failure, 점수, 결과, finding, correction method가
  JSON·Markdown report·Canvas draft에서 서로 같아야 한다.
- stagingArtifactPath와 projectTargetPath의 책임과 경로가 분리되어야 한다.
- PASS는 85점 이상이며 fatal failure와 필수 증거 부족이 없어야 한다.
- FAIL은 approved_for_promotion 또는 promoted가 될 수 없다.
- promoted는 source/project hash 또는 동등한 copy 검증과 Unity import 근거가 있어야 한다.
- 공통 11개 Canvas 섹션이 각각 한 번만 정확한 순서로 존재해야 한다.
- self_contained Slack Canvas는 Slack-hosted image와 source intent,
  평가 결정, required action만으로 현재 PC 없이 이해 가능해야 한다.
- 평가와 Canvas 변환은 Unity project asset과 `.meta`를 변경하지 않아야 한다.

실패 시 Output:
- status: failed
- failureType: {missing_skill_json | invalid_skill_json | equipment_id_mismatch |
  missing_icon | invalid_png | missing_generation_record | missing_frame_template |
  missing_normalization_record | missing_preview32 | insufficient_evidence |
  missing_evaluation_report | evaluation_report_hash_mismatch | invalid_form_version |
  invalid_result | promotion_result_conflict | promotion_verification_missing |
  staging_target_path_collision | invalid_evaluation_root | other_pc_path_detected |
  invalid_draft_path | output_write_failed | slack_write_not_available |
  slack_write_not_authorized | slack_evidence_upload_not_available |
  slack_evidence_upload_failed}
- failureReason:
- blockedAction:
- unchangedArtifacts:
- nextRequiredAction:
```
