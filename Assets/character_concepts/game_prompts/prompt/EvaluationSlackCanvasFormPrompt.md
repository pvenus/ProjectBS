# Evaluation Slack Canvas Form Prompt

완료된 평가 리포트를 도메인 중립적인 Slack Canvas 평가 기록으로 변환할
때 사용합니다. 이 프롬프트는 평가, 수정, 승격 복사, Git 작업을 수행하지
않습니다.

이 작업의 역할은 **Slack 디자인 평가 Canvas 기록·레이아웃 관리
오케스트레이터**입니다. 단순히 평가 결과를 메시지로 전달하는 역할이
아닙니다. 원본 기획, 디자인 정의, 실제 이미지, 생성 프롬프트, 불변 평가
결과를 독자가 한 Canvas 안에서 이해할 수 있도록 구성하고 게시 후 다시
검증합니다.

`role_parent`는 정책·대상·불변 데이터·완료 상태만 통제하며 Slack을 직접
수정하지 않습니다. 실제 Canvas create/update/upload/message/cleanup은 같은
Canvas와 목표에 대해 등록된 단일 `forked_execution` 작업만 수행합니다.
재시도와 입력 보완은 그 실행 작업을 계속 재사용하며, 대체 실행 작업이나
중첩 분기를 만들지 않습니다.

기록 필드·단일 세로 섹션 배치·11개 보존 의미·reader-facing 작성 규칙은 공통 Guide만
정의합니다. 이 Prompt는 입력 확인, 상태 전이, 도구 실행 순서, 중단 조건,
완료 검증만 정의하며 별도의 표시 형식을 다시 만들지 않습니다.

## Prompt

```text
작업 폴더 = {project_root}

완료된 평가 리포트를 공통 Slack Canvas 평가 기록 폼으로 변환해줘. 원본 평가 결과와 점수는 변경하지 말고, 로컬 평가 대상 경로와 Pass 후 프로젝트 반영 경로를 반드시 분리해 기록해줘.

Input:
- projectRoot: {project_root}
- formVersion: evaluation_canvas_form_v1
- readerFacingLayoutVersion: artifact_design_sections_v3_single_column
- taskExecutionMode: {role_parent | forked_execution}
- parentThreadId: {source_thread_id | null}
- evaluationDomain: {lowercase_snake_case}
- artifactType: {lowercase_snake_case}
- artifactId: {stable_file_safe_id}
- artifactName: {display_name}
- artifactUsage: {게임_내_사용_위치_표시_크기_용도}
- planningSource: {기획_문서_ID_및_프로젝트_상대경로}
- planningOriginalContent: {관련_기획_원문_그대로}
- displayContent: {게임_표시문_또는_Same_as_Planning_Original_Content}
- planningCoreInterpretation: {주요_대상_행동_물성_공간관계_해석}
- designConcept: {분위기_형태_재질_색상_화풍}
- promptCoreGoals: [{가장_먼저_전달해야_할_목표_최대_3개}]
- requiredVisualElements: [{독립적으로_확인_가능한_필수_표현_요소_3_5개}]
- hardConstraints: [{금지_요소_및_기술적_Hard_Gate}]
- generationPromptOriginal: {생성_도구에_실제_입력한_최종_프롬프트_원문}
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
- canvasEvidenceMode: {reference_only | self_contained}
- slackEvidenceUploadAuthorized: {false | true}
- slackEvidenceConversationId: {channel_or_conversation_id | null}
- slackChannelTopTabRequired: {false | true}
- representativeMessageRequired: {false | true}
- artifactEvidenceItems: [{artifactId, artifactName, artifactUsage, mediaType, stagingArtifactPath, projectTargetPath, planningSource, planningOriginalContent, displayContent, planningCoreInterpretation, designConcept, promptCoreGoals, requiredVisualElements, hardConstraints, generationPromptOriginal, evaluationResultSource}]
- localDraftMode: {save | report_only}
- outputLocalCanvasDraftPath: {Assets/Doc/Evaluation/slack_canvas/v1/{evaluationDomain}/{artifactType}/{artifactId}.canvas.md | null}

참조 가이드:
- Assets/character_concepts/game_prompt_guide/prompt/EvaluationSlackCanvasFormGuide.md
- {domainSlackCanvasGuide | 생략 가능}

역할 계약:
- 역할명: Slack 디자인 평가 Canvas 기록·레이아웃 관리 오케스트레이터
- 부모 책임: `role_parent`는 요청 해석, 입력 계약, 대상 Canvas, 불변 평가 데이터, 포맷 버전, 실행 작업 레지스트리와 최종 완료 상태만 관리한다. Slack UI/API 쓰기를 직접 수행하지 않는다.
- 실행 책임: `forked_execution`은 등록된 단일 Slack 쓰기 소유자다. Canvas create/update/upload/message/cleanup과 UI 검증을 다른 작업과 나누거나 병행하지 않는다.
- 단일 소유권: 같은 Canvas와 같은 목표에는 활성 실행 작업이 정확히 하나여야 한다. 부모나 다른 실행 작업이 동일 artifact의 임시 레이아웃을 함께 수정하면 `mixed_writer_mode_violation`으로 즉시 중단한다.
- 작업 단위: canonical artifact 1개. batch는 이 작업 단위를 순차 반복한 집합이다.
- 메모리 안전 단위: artifact 하나도 `source 1건 읽기 -> single-column record 1건 구성 -> Canvas 1건 갱신 -> 해당 record 1건 재조회 -> 임시 데이터 해제`의 작은 체크포인트로 나눈다.
- 일괄 적재 금지: 전체 Canvas 본문, 전체 평가 보고서, 전체 원문, 전체 이미지 byte를 동시에 메모리에 올리지 않는다. manifest와 누적 count만 batch 상태로 유지한다.
- 도구 호출 크기: 가능한 경우 section/canonical ID 기반 대상 조회를 사용하고, 전체 조회가 불가피하면 필요한 항목만 추출한 뒤 원문 결과를 다음 artifact 처리에 재사용하지 않는다.
- 작업 분기 정책: 역할과 기준을 유지하는 부모 작업은 정책·대상·완료 상태를 관리하고, 실제 Canvas create/update/upload/message/cleanup은 독립된 작업 요청마다 정확히 하나의 분기 실행 작업에서만 수행한다.
- 분기 재사용: 같은 Canvas와 같은 목표의 입력 보완, blocker 해소, 재시도, 검증 후속 요청은 기존 분기 작업을 재사용한다. 기존 분기가 완료·보관되기 전에는 같은 범위의 추가 분기를 만들지 않는다.
- 분기 생성 확인: fork 호출은 한 번만 실행한다. timeout, aborted, app-server unavailable처럼 결과가 불명확하면 실패로 단정하거나 즉시 다시 fork하지 않고, 서버 지연 반영을 고려해 최근 작업 목록을 충분한 간격으로 재확인한다.
- 분기 재시도 조건: 최초 호출 뒤 같은 parentThreadId·Canvas·목적의 child가 없다는 사실을 지연 조회에서도 확인한 경우에만 한 번 재시도한다. 늦게 생성된 중복 child가 발견되면 작업을 전달하지 않고 정확한 중복 child만 보관한다.
- 분기 식별: 실행 작업 제목은 `분기 작업 — {Canvas 제목 또는 ID} {작업 목적}` 형식으로 작성하고, `parentThreadId`를 전달해 원본 작업과의 관계를 보존한다.
- 입력 책임: 식별 정보, 기획 근거, 디자인 정의, 생성 기록, 불변 평가 결과, 게시 대상을 서로 다른 입력 묶음으로 확인한다.
- 실행 책임: 입력 근거 확인, 공통 Guide의 단일 세로 섹션 흐름 적용, 실제 media 순차 업로드, canonical ID upsert, 채널 공유, 상단 탭 연결, 대표 링크 1건, 재조회 검증
- 권한 밖 작업: 이미지 생성·수정, 재평가·재채점, 평가 완화·강화, 프로젝트 승격, Unity·기획 원문·Git 변경
- 누락 정보 처리: 추측하거나 꾸미지 말고 정확한 missing input과 publication waiting 상태를 보고
- reader-facing 준수: 공통 Guide의 금지 항목과 증거 표시 규칙을 검사한다.
- 운영 상태: READY -> PUBLISHING_ONE_ARTIFACT -> VERIFYING_ONE_ARTIFACT -> READY_FOR_NEXT를 artifact마다 반복한 뒤 VERIFYING_BATCH -> COMPLETE로 전이한다.
- 중단 상태: 필수 입력이 없으면 WAITING_INPUT, 게시 권한·도구·대상이 유효하지 않거나 게시 검증이 실패하면 PUBLICATION_BLOCKED다.
- 완료 조건: create, upload, update, message 호출 성공만으로 완료하지 않는다. `connector_only`는 connector 재조회에서 canonical record 단일성·불변 평가·media reference·금지 문자열·표/다단 레이아웃 부재를 확인하고 UI 렌더는 `Not Verified`로 명시한다. `ui_only`는 autosave 안정 대기 후 UI reload에서 실제 content block과 image DOM/natural size/aspect ratio를 추가 확인해야 COMPLETE다.
- 백업 보존: 구조 migration 중 기존 canonical record는 새 구조가 reload 검증을 통과할 때까지 삭제하지 않는다. 백업 삭제와 reader-facing cloud content 삭제는 action-time 사용자 확인 후에만 수행한다.
- 실패 정리: 새 구조 검증이 실패하면 다음 artifact로 이동하지 않는다. 임시 layout/block만 rollback하고 기존 canonical record를 보존하며, rollback 결과·고아 file ID·미수정 범위를 콜백한다.
- 콜백 책임: 실행 작업은 실제 저장 상태, canonical ID, 수정 범위, reload/UI/connector 검증, failureType, rollback, unreferenced Slack file ID, untouched artifact 범위를 사실대로 보고한다.

작업:
1. taskExecutionMode=role_parent에서 Slack 변경 요청을 받으면 현재 작업에서 쓰기를 시작하지 않는다. 같은 Canvas와 목표의 기존 분기가 있으면 그 작업에 후속 요청을 전달하고, 기존 분기가 없을 때만 같은 맥락을 상속하는 새 실행 작업을 정확히 하나 분기해 제목·parentThreadId·대상 Canvas·요청 범위를 전달한다.
2. fork 응답이 timeout, aborted, app-server unavailable이면 생성 실패가 아니라 `fork_result_unknown`으로 처리한다. 새 fork 전에 최근 child 목록을 지연 재조회하고, 하나가 확인되면 그 child를 사용한다.
3. taskExecutionMode=forked_execution이면 parentThreadId와 분기 관계가 확인되어야 실제 Slack 쓰기를 시작할 수 있다.
4. 공통 가이드를 먼저 읽고 `evaluation_canvas_form_v1`, `artifact_design_sections_v3_single_column`, 필수 필드, 11개 archival semantics와 5개 reader-facing category mapping을 확인한다.
5. domainSlackCanvasGuide가 있으면 공통 계약을 제거하거나 이름을 바꾸지 않는 추가 규칙으로만 적용한다.
6. 각 artifact의 canonical ID, 실제 media, planningSource, planningOriginalContent, displayContent, designConcept, generationPromptOriginal, immutable evaluation source가 서로 같은 대상을 가리키는지 확인한다.
7. 공통 Guide가 요구하는 기획·디자인·프롬프트·평가 근거가 모두 제공되었는지 검사한다. 누락 필드를 생성하거나 추정하지 않는다.
8. evaluationReportSource에서 완료된 결과를 읽고 결과·점수·Hard Fail·severity·findings·actions를 불변 값으로 잠근다. inlineEvaluationReport는 source 내용을 전달할 수 있지만 source 식별자를 대체하지 않는다.
9. stagingArtifactPath, evaluationWorkspacePath, projectTargetPath의 역할과 promotionStatus의 결과 호환성 및 검증 근거를 확인한다. 충돌이나 모순이 있으면 게시 전에 중단한다.
10. localDraftMode=save이면 지정된 canonical draft 경로에 draft 하나만 저장한다. report_only이면 로컬 파일을 생성하거나 수정하지 않는다.
11. canvasUpdateMode=draft_only이면 Slack 도구를 호출하지 않는다. append 또는 replace_artifact_section은 쓰기 승인과 명확한 target이 있을 때만 수행한다.
12. artifact record는 공통 Guide의 `artifact_design_sections_v3_single_column` 계약을 그대로 적용한다. Markdown 표, Canvas native columns/layout, 셀 병합 모양을 사용하지 않는다.
13. 구조 migration 전 대상 Canvas에서 artifact 1건만 pilot한다. canonical heading, top-level media 1개, 5개 연속 category section을 저장하고 선택한 writer mode의 재조회 검증을 통과한 뒤에만 batch를 확장한다.
14. artifact마다 writer mode를 `connector_only` 또는 `ui_only` 중 하나로 잠근다. 같은 transient layout에 두 writer를 혼용하지 않는다. connector read-only 검증은 어느 writer mode에서도 허용한다.
15. canvasEvidenceMode=self_contained이면 artifactEvidenceItems를 한 번에 하나씩 처리한다. 실제 media를 Slack에 업로드하고 Guide가 지정한 media 위치에 삽입한 뒤 Slack file reference와 임시 표식 제거를 확인한다.
16. media 업로드 전 exact source identity, SHA, dimensions/aspect ratio와 Canvas 삽입 경로를 확인한다. unshared file이 Canvas picker에서 재사용 가능하다고 가정하지 않으며, 업로드 결과가 삽입되지 않으면 같은 artifact를 반복 업로드하지 않고 file ID를 unreferenced로 보고한다.
17. 각 artifact는 다음 체크포인트를 순서대로 수행한다: (a) manifest entry 1건 확인, (b) source/provenance/evaluation 파일을 각각 필요할 때 1개씩 읽기, (c) single-column record 1건 구성, (d) media 1건 확인 또는 승인된 업로드, (e) Canvas record 1건 upsert, (f) 선택한 writer mode의 저장 안정성 확인, (g) connector로 해당 canonical record·media ref·5개 category·표/다단 레이아웃 부재를 재검증, (h) `ui_only`일 때만 UI reload와 image-ready 검증, (i) 임시 본문·media byte·도구 출력을 해제하고 count만 남기기.
18. 한 체크포인트가 끝나기 전에 다음 artifact의 원문·이미지·평가 파일을 읽지 않는다. 병렬 media 업로드와 여러 artifact를 포함한 대형 update 요청을 금지한다.
19. 업로드 권한·도구·conversation·결과 중 하나라도 없으면 로컬 링크로 대체하지 말고 정확한 failureType으로 중단한다.
20. 동일 artifactType+artifactId가 이미 있으면 중복 append하지 않고 정확한 기존 record만 upsert한다.
21. image DOM이 없거나 naturalWidth/naturalHeight가 0이면 connector에 file reference가 있어도 self-contained media 검증 실패다. 즉시 `slack_canvas_file_reference_blank_after_reload`로 중단한다.
22. connector가 nested legacy element를 실제 삭제하지 못하고 zero-width content로 대체한 경우 cleanup 완료로 선언하지 않는다. reader-visible 영향과 잔여 section을 보고한다.
23. artifact 게시 직후 Guide 적합성, 불변 평가 보존, 실제 media, canonical ID 단일성을 확인한다. 통과한 뒤에만 READY_FOR_NEXT로 전이한다.
24. batch 검증도 전체 원문을 다시 조립하지 않는다. canonical ID, layout version, media reference, 5개 category, result, 표/다단 레이아웃 및 금지 문자열 여부의 최소 집계만 순차 확인한다.
25. 요청된 경우 Canvas를 지정 채널에 귀속·공유하고 채널 상단 탭에 추가한다. artifact별 메시지는 금지하며 대표 Canvas 링크 메시지는 정확히 1건만 게시한다.
26. batch 재조회와 요청된 채널 연결 검증까지 통과한 경우에만 COMPLETE를 보고한다.

Output:
- Role Contract Check:
- Task Execution Mode / Parent Task / Fork Check:
- Task Operating State: READY | WAITING_INPUT | PUBLISHING_ONE_ARTIFACT | VERIFYING_ONE_ARTIFACT | READY_FOR_NEXT | VERIFYING_BATCH | COMPLETE | PUBLICATION_BLOCKED
- Form Version:
- Reader-Facing Layout Version:
- Evaluation Domain / Artifact Type / Artifact ID:
- Planning Source / Original Preservation Check:
- Design Definition Check:
- Generation Prompt Original Preservation Check:
- Evaluation Report Source:
- Result / Overall Score / Highest Severity:
- Staging Artifact Path:
- Evaluation Workspace Path:
- Project Target Path:
- Promotion Status / Validation:
- Local Canvas Draft: {saved_path | not_saved}
- Slack Canvas Update: not_requested | posted | skipped
- Slack Evidence Upload: not_requested | uploaded | skipped
- Slack Channel Share / Top Tab:
- Representative Message: not_requested | posted_once | skipped
- Self-Contained Evidence Check: Pass | Fail | Not Applicable
- Writer Mode / Single Writer Check:
- Connector Reread / UI Verification Mode Check:
- Media Reference / UI Render Verification Check:
- Backup / Rollback Check:
- Unreferenced Slack File IDs:
- Media Block / Placeholder Check:
- Duplicate / Local Path / Literal BR Check:
- Single-Column Sections / Archival Semantics Mapping Check:
- Result Preservation Check:
- Path Separation Check:
- Modified Files:
- Result: Pass | Fail

검증:
- formVersion은 evaluation_canvas_form_v1이어야 한다.
- 새 self_contained visual record와 format migration은 readerFacingLayoutVersion=artifact_design_sections_v3_single_column이어야 한다.
- draft path는 Assets/Doc/Evaluation/slack_canvas/v1/{evaluationDomain}/{artifactType}/{artifactId}.canvas.md여야 한다.
- 공통 Guide의 필수 필드, `artifact_design_sections_v3_single_column`, 5개 category와 11개 archival semantics mapping 검사가 통과해야 한다.
- artifact reader-facing record에는 Markdown 표, Canvas native layout, Canvas column block이 없어야 한다.
- 원본 평가 결과, 점수, severity, findings가 유지되어야 한다.
- staging/evaluation/project target의 역할이 섞이지 않아야 한다.
- promotionStatus는 평가 결과 및 승인·복사 검증 근거와 일치해야 한다.
- FAIL은 promoted 또는 approved_for_promotion일 수 없다.
- promoted는 검증 근거 없이 기록할 수 없다.
- localDraftMode=report_only이면 local draft 파일을 쓰지 않아야 한다.
- self_contained 기록의 핵심 근거가 로컬 파일 링크에만 의존해서는 안 된다.
- artifactEvidenceItems는 메모리에 일괄 적재하지 않고 한 항목씩 순차 처리해야 한다.
- artifact 내부에서도 source, evaluation, media, Canvas update를 한 체크포인트씩 처리하고 다음 단계 전에 불필요한 대형 내용을 해제해야 한다.
- batch 검증은 전체 Canvas/전체 source를 다시 조립하지 않고 최소 검증 필드의 누적 count로 수행해야 한다.
- canonical artifact ID 중복이 없어야 하고 공통 Guide의 reader-facing 금지 항목 검사가 통과해야 한다.
- 요청된 채널 공유·상단 탭·대표 링크 1건이 재조회로 확인되어야 한다.
- 이 프롬프트는 평가, artifact 수정, 프로젝트 복사, Git 작업을 수행하지 않는다.

실패 시 Output:
- status: failed
- failureType: {invalid_task_execution_mode | thread_fork_not_available | fork_result_unknown | duplicate_execution_thread | missing_parent_thread_id | missing_evaluation_report | missing_required_field | missing_planning_source | missing_planning_original_content | missing_design_definition | missing_generation_prompt_original | missing_form_guide | invalid_form_version | invalid_result | invalid_promotion_status | promotion_result_conflict | promotion_verification_missing | staging_target_path_collision | invalid_draft_path | invalid_local_draft_mode | invalid_canvas_target | slack_write_not_available | slack_write_not_authorized | slack_evidence_upload_not_available | slack_evidence_upload_failed | missing_self_contained_evidence | invalid_reader_facing_layout | reader_facing_local_path_exposed | image_cell_placeholder_remaining | duplicate_artifact_record | artifact_section_not_found | unsupported_canvas_update_mode | slack_canvas_table_merge_not_supported | slack_canvas_native_layout_not_supported | slack_canvas_native_layout_autosave_data_loss | slack_canvas_top_level_text_block_autosave_loss | slack_canvas_file_reference_blank_after_reload | slack_canvas_unshared_file_reinsertion_not_available | computer_use_owned_file_dialog_not_targetable | slack_canvas_nested_delete_not_available | mixed_writer_mode_violation | output_write_failed}
- failureReason:
- missingInputs:
- blockedAction:
- unchangedArtifacts:
```

## Skill Animation Migration Addendum

When `artifactType=skill_animation` and the source evaluation already exists:

```text
workflowMode: format_existing
evaluationWorkspacePath:
  C:\github\design_evaluation\skill_animation\{artifactId}
canvasEvidenceMode: self_contained
```

Required inputs:

```text
stagingReferencePath
stagingAnimationPath
referenceSha256
animationSha256
usableFrameCount
frameOrder
nominalFps
encodedFrameDelayMs
loopMode
playbackGifPath
playbackGifSha256
contactSheetPath
technicalValidationPath
unityMetaStatus
unityReimportStatus
clipBindingStatus
```

Rules:

1. Preserve the existing evaluation result and score; do not re-score.
2. Apply the Guide's Skill Animation Evidence content and format contract
   without redefining it in this Prompt.
3. Process one artifact at a time: upload its required Slack-hosted media,
   upsert by exact `artifactType + artifactId`, then re-read and verify it.
4. Use a dedicated channel-shared Canvas and a single representative link
   message instead of per-artifact channel messages.
5. Do not publish failed, blocked, or ungenerated assets as completed.
6. Do not generate or modify any source or production image, Unity asset, or
   Git state.

## Character Evaluation Canvas Addendum

When `artifactType=character_evaluation` and existing character evaluation data
is being migrated to Slack Canvas, use the rebased evaluation root instead of
publishing directly from PixelLab export storage:

```text
workflowMode: format_existing
evaluationWorkspacePath:
  C:\github\design_evaluation\character\{characterName}_{grade}
canvasEvidenceMode: self_contained
canonicalCharacterId:
  character.{characterName}.{grade}
```

Required local inputs:

```text
metadata.json
evaluation_result.txt
evaluation_animation_result.txt
animations/
converted/
character_canvas_manifest.json
migration_summary.json
character_animation_gif_by_type_manifest.json
evidence/animation_gif_by_type/{characterName}_{grade}_idle_all_directions.gif
evidence/animation_gif_by_type/{characterName}_{grade}_move_all_directions.gif
evidence/animation_gif_by_type/{characterName}_{grade}_attack_all_directions.gif
```

Execution rules:

1. Preserve the existing image and animation evaluation results; do not re-score.
2. Apply the Guide's Character Evaluation Animation GIF Evidence content and
   format contract without redefining it in this Prompt.
3. Process one character at a time: upload its required media, upsert by
   `artifactType=character_evaluation` plus exact `canonicalCharacterId`, then
   re-read and verify the record before continuing.
4. Do not create one channel message per character, preview, or GIF. Post only
   one representative Canvas link after all records are verified.
5. Do not generate, regenerate, crop, resize, interpolate, semantically edit, or
   otherwise modify source/production character assets while formatting Canvas
   evidence.

Batch validation:

```text
targetCharacters: 22
expectedAnimationGifsPerCharacter: 3
expectedAnimationGifTotal: 66
minimumMediaTotal: 88  # 22 static previews + 66 animation GIFs
```

Report GIF publication separately from static preview publication in the final
summary.
