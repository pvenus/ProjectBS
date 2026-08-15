# Generated Image Evaluation Prompt

Use this prompt as the main evaluation workflow for one generated image or one
domain-defined image set that is already downloaded and preserved locally.
Content-specific evaluation details are resolved from the adapter registry and
extended through domain guides, not duplicated in this prompt.

## Prompt

~~~text
현재 작업에서 사용 중인 ProjectBS 저장소와 현재 PC의 기존 평가 폴더 체계를 내부적으로 확인하고, 다운로드와 보존이 완료된 생성 이미지 하나 또는 도메인 정의 이미지 세트를 평가해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
- AgentDocs/planning-guides/prompt/EvaluationSlackCanvasFormGuide.md

Input:
- requestId: {optional_stable_request_id}
- evaluationPackageId: {preferred_generated_media_evaluation_package_v2_id | null_for_legacy}
- assetType: {character_main_image | character_animation | icon | general_animation | imagegen_image | background_single_image | null_for_legacy}
- domainType: {character | skill | item | stage | battle | environment | null_for_legacy}
- artifactType: {skill_icon | item_icon | story_popup_main_image | skill_animation | character_image | character_animation | battle_background | null_for_package_mode}
- contentId: {canonical_content_id}
- sourceRecordId: {optional_stable_non_path_generation_or_download_record_id}
- workflowMode: {evaluate_new | re_evaluate}
- priorEvaluationRecordId: {required_non_path_record_id_for_re_evaluate | null}

입력 경계:
- 신규 package mode는 evaluationPackageId, assetType, domainType, contentId를 받고 legacy mode만 artifactType을 사용한다.
- 저장소, 기획 문서, source, 평가 폴더, 결과 파일, 프로젝트 target, 도메인 가이드 경로와 점수 기준을 외부 입력으로 요구하지 않는다.
- 외부 payload에 경로나 평가 기준이 있어도 현재 PC와 저장소의 canonical 규칙 대신 사용하지 않는다.
- 다른 PC의 절대 경로를 사용하지 않는다.

작업:
1. 현재 작업의 workspace와 Git 정보를 이용해 저장소를 확인하고 AgentDocs와 Assets가 같은 저장소인지 검증한다.
2. evaluationPackageId가 있으면 GeneratedMediaEvaluationPackageGuide.md를 읽고 sealed v2 package의 assetType+domainType으로 adapter를 선택한다. 없으면 legacy artifactType으로 선택한다. package mode의 background_single_image와 legacy imagegen_image/battle_background를 교환하거나 두 identity mode를 함께 사용하면 중단한다.
3. adapter가 없거나 blocked 또는 필수 계약이 불완전하면 공통 점수로 대신 평가하지 말고 중단한다.
4. package mode이면 manifest와 모든 member hash, planning/prompt/generation identity, structureProfile, readiness를 검증한다. legacy mode에서 sourceRecordId가 있으면 artifactType/contentId와 일치하는지 확인한다. source는 하나로 확정될 때만 선택한다.
5. 평가할 source가 candidate, preview, thumbnail, contact sheet 또는 프로젝트 파일이 아니라 다운로드 후 보존된 원본인지 확인하고 SHA-256을 기록한다.
6. 단일 이미지 또는 이미지 세트 manifest를 구조 프로필에 맞춰 확인한다. package mode는 sealed manifestPayloadHash를 재검증하고 legacy mode만 canonical member manifestHash를 만든다. 세트는 멤버 역할, 순서, 파일 수와 개별 hash를 기록한다.
7. canonical 기획·콘텐츠 데이터와 generation/download 기록에서 artifactUsage, planningSource, planningOriginalContent, displayContent, planningCoreInterpretation, designConcept, promptCoreGoals, requiredVisualElements, hardConstraints, generationPromptOriginal을 수집한다.
8. 원본 기획 내용과 생성 프롬프트를 재작성하지 않는다. 필수 증거가 없으면 이미지에 맞춰 추측하지 말고 insufficient_evidence로 중단한다.
9. 도메인 가이드가 사전 brief 고정을 요구하면 이미지를 열기 전에 evaluation brief를 만들고 생성 시간과 hash를 기록한 다음 visualInspectionStartedAt을 기록한다.
10. package mode는 request_type_key={assetType}.{domainType}, legacy mode만 request_type_key=artifactType으로 확정한다. request_type_key, contentId, UTC 평가 시각과 source 또는 manifest hash prefix로 evaluationRecordId를 만들고, 해당 불변 record 폴더의 input/evaluation_input.json을 저장한 뒤 공통 입력 계약과 artifact identity를 검증한다. 파일명에서 key를 추론하지 않는다.
11. 공통 게이트를 먼저 실행한다: identity, provenance/hash, 파일 무결성, staging/project 경로 분리, 기획·디자인 증거 완전성, 금지 텍스트·UI·로고·워터마크, 마스터 컨셉 hard constraint, 증거 충분성.
12. single_image이면 도메인 크기·비율·alpha·crop·display-size 규칙과 하나의 원본 이미지를 평가한다. background_single_image_v2이면 추가로 scene composition/viewpoint/horizon/depth/playable-area/subject/canvas/aspect/target/safe-area/background-policy/consistency-lock/scene-anchor metadata와 원본의 일치를 평가하고 icon adapter 규칙을 적용하지 않는다. character_single_image_v2이면 exact expression profile payload/hash와 planning/profile evidence를 먼저 검증한다. animation-ready minimal profile은 점수 전에 비례(4.25 heads 초과, 24-27% 범위 밖, 7-8등신/영웅적 장신), detail density(비늘·리벳·조밀한 주름·해칭·microtexture·modeled shading), color/value(gradient·cinematic/physical lighting·realistic material·2개 초과 accent hue) fatal gate를 서로 독립적으로 실행하고 하나라도 실패하면 exact character_evaluation_*_gate_failed token으로 acceptance를 차단한다.
13. ordered_rotation_set이면 정확한 8방향 순서와 identity 일관성을 평가한다. paired_sheet_animation 또는 ordered_frame_set이면 원본 PNG, 개별 프레임, 순서, count, 중심축, 일관성, contact sheet와 playback evidence를 평가한다. animation-ready minimal profile을 상속한 character animation은 12번의 세 semantic gate를 모든 frame과 cross-frame consistency에 적용하고 한 frame의 실패도 전체 set 실패로 처리한다. GIF는 움직임 판단에만 사용하고 alpha·crop·픽셀 품질 판정에는 사용하지 않는다.
14. 이미지 세트의 한 멤버라도 치명적 실패가 있으면 평균 점수로 가리지 말고 전체 세트를 Fail 처리한다.
15. 구조 게이트가 끝난 뒤에만 도메인 평가 가이드의 fatal gate를 실행하고, fatal failure가 없을 때만 도메인 점수를 계산한다.
16. 도메인 점수 카테고리 이름, 배점, threshold와 category minimum을 그대로 사용한다. 다른 콘텐츠 rubric으로 이름을 바꾸거나 배점을 재분배하지 않는다.
17. 각 점수와 게이트에 criterionId, sourceGuide, scope, memberIds와 실제 관찰 근거를 기록한다.
18. domain native result를 보존하고 공통 가이드의 규칙으로 result와 evaluationStatus를 정규화한다. PASS만 passForProjectCopy=true이며 평가 단계의 promotionStatus는 not_promoted다.
19. Findings에는 severity, criterionId, scope, memberIds, finding, evidence, impact, recommendation을 기록한다.
20. Required Actions에는 우선순위, 수정 내용, correctionMethod, regenerationRequired와 관련 finding ID를 기록한다. 필수 수정을 PASS 유지를 위해 Optional Improvements로 옮기지 않는다.
21. evaluation/records/{evaluationRecordId}/evaluation_result.json을 generated_image_evaluation_v1 구조로 작성한다. Slack Canvas 공통 필드와 11개 archival semantics가 모두 표현 가능한지 검증한다.
22. 같은 record 폴더에 evaluation_report.md를 작성하고 JSON의 result, score, gate, finding, action, hash와 서로 일치하는지 확인한다.
23. workflowMode=re_evaluate이면 priorEvaluationRecordId의 기존 결과와 근거를 덮어쓰지 말고 새 immutable record를 작성한 뒤 reEvaluationPlan과 changeLog로 연결한다.
24. evaluation/evaluation_index.json에 record ID, artifact identity, source 또는 manifest hash, 결과, 평가 시각과 상대 record 경로를 등록한다. 후속 작업이 moving latest가 아니라 정확한 immutable record를 선택할 수 있어야 한다.
25. Slack Canvas를 작성하거나 게시하지 않는다. 이 결과는 이후 Canvas 포맷 작업이 재채점 없이 읽을 수 있는 source of truth다.
26. 이미지를 생성·다운로드·수정하거나 다른 후보로 교체하지 않고, Assets/ImagesGenerated 또는 Assets/Resources로 복사하지 않는다.
27. Unity .meta, SO, animation clip, runtime binding, Git commit/push/merge와 배포를 수행하지 않는다.

Output:
- Request ID
- Evaluation Package ID / Asset Type / Domain Type / Legacy Artifact Type / Content ID
- Routed Structure Profile / Domain Evaluation Guide
- Evaluation Record ID
- Resolved Evaluation Workspace
- Evaluation Input Path / Brief Hash
- Evaluated Source Paths / Individual SHA-256 / Manifest Hash
- Derived Evidence Paths
- Common / Structure / Domain Gate Results
- Domain Native Result
- Normalized Evaluation Status / Result
- Score / Maximum / Pass Criteria
- Highest Severity
- Findings / Required Actions / Optional Improvements
- passForProjectCopy / Promotion Status
- Evaluation Result JSON Path
- Evaluation Report Path
- Canvas Readiness Validation
- Blockers / Required Next Actions

실패 시 Output:
- status: blocked | failed | not_evaluated
- failureType: background_adapter_identity_mismatch | legacy_current_identity_conflict | missing_background_evaluation_contract | character_evaluation_proportion_gate_failed | character_evaluation_detail_density_gate_failed | character_evaluation_color_value_gate_failed | missing_domain_evaluation_adapter | insufficient_evidence | 기존 evaluation failure token
- 실패한 단계와 근거
- 선택하지 않은 source 또는 모호한 후보
- 누락되거나 충돌한 canonical 증거
- 생성하지 않은 평가 결과 파일
- 원본 source와 기존 평가 기록이 변경되지 않았는지 여부
- 되돌려야 할 선행 작업: generation | download | evidence_preparation | domain_guide_extension | none

검증:
- 외부 입력에 로컬 source, 평가 폴더, project target 또는 점수 규칙이 없어야 한다.
- package mode의 assetType+domainType 또는 legacy artifactType 중 하나가 ready adapter와 정확히 일치해야 한다.
- background_single_image는 stage/battle/environment ready row 중 하나와 정확히 일치하고 icon adapter와 교환되지 않아야 한다.
- legacy imagegen_image/battle_background와 current background_single_image identity가 혼합되지 않아야 한다.
- package mode evaluationRecordId에 artifactType을 사용하지 않고 legacy mode에 assetType.domainType을 사용하지 않아야 한다.
- source identity와 현재 SHA-256이 generation/download 기록과 일치해야 한다.
- planningOriginalContent와 generationPromptOriginal은 원문 그대로 보존되어야 한다.
- 공통·구조·도메인 fatal gate가 점수보다 먼저 실행되어야 한다.
- 점수 카테고리와 최대값은 도메인 가이드와 정확히 같아야 한다.
- 세트 멤버의 치명적 실패가 평균 점수로 숨겨지지 않아야 한다.
- evaluation_result.json이 generated_image_evaluation_v1이며 Canvas 필수 공통 필드와 11개 의미를 모두 표현해야 한다.
- report와 JSON의 결과, 점수, findings, actions와 hash가 일치해야 한다.
- PASS만 passForProjectCopy=true여야 한다.
- stagingArtifactPath와 projectTargetPath가 같으면 process violation으로 평가를 차단해야 한다.
- 평가 중 source bytes와 프로젝트 파일을 수정하지 않아야 한다.
- 이미지 생성, 다운로드, 프로젝트 복사, Slack 게시, Unity 작업, Git과 배포를 수행하지 않아야 한다.
~~~
