# Generated Image Project Promotion Prompt

Use this prompt only after image download and evaluation are complete. It
promotes the exact evaluated Pass artifact from the local evaluation workspace
into the Unity project. It does not generate, download, edit, or evaluate.

## Prompt

~~~text
현재 작업에서 사용 중인 ProjectBS 저장소와 현재 PC의 기존 평가 폴더 체계를 내부적으로 확인하고, 다운로드와 평가가 이미 완료된 이미지 하나 또는 도메인 정의 파일 세트를 프로젝트 폴더로 복사해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImageProjectPromotionGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md

Input:
- requestId: {required_stable_request_id_for_package_mode | optional_for_legacy}
- evaluationPackageId: {generated_media_evaluation_package_v2_id | null_for_legacy}
- assetType: {character_single_image | animation | background_single_image | null_for_legacy}
- domainType: {character | stage | battle | environment | null_for_legacy}
- artifactType: {skill_icon | item_icon | skill_animation | character_animation | battle_background | story_popup_main_image | null_for_package_mode}
- contentId: {canonical_content_id}
- evaluationRecordId: {required_stable_non_path_record_id_for_package_mode | optional_for_legacy}
- replaceExisting: {false | true}
- replacementApprovalRef: {required_non_path_approval_reference_when_replacing | null}

입력 경계:
- 외부에는 package mode의 evaluationPackageId/assetType/domainType 또는 legacy artifactType, contentId, 평가 기록 식별자, 교체 승인 정보 같은 일반화된 값만 받는다.
- repositoryRoot, evaluationRoot, source path, report path, project target, 파일명, ContentDomain, Unity import 설정을 요구하지 않는다.
- 외부 payload에 경로나 생성 도구 정보가 들어 있어도 복사 경로나 규칙으로 신뢰하지 않는다.
- 다른 PC의 절대 경로를 사용하지 않는다.

작업 범위:
1. 현재 작업 안에서 저장소, 로컬 평가 workspace, 평가 리포트, 보존 source, 프로젝트 target을 내부적으로 찾는다.
2. GeneratedImageProjectPromotionGuide.md의 routing registry로 identity mode, 도메인 가이드, structureProfile, 단일 파일/파일 세트 구조와 canonical Assets/ImagesGenerated 목적지를 결정한다. current background와 legacy battle_background를 섞지 않는다.
3. package mode는 evaluationPackageId, assetType, domainType, contentId, evaluationRecordId와 registry row의 exact structureProfile이 모두 일치하는 exact package/record만 사용한다. character_single_image+domainType=character는 character_single_image_v2, animation+domainType=character는 animation_gif_frame_set_v2, background_single_image는 등록된 background_single_image_v2 row만 허용한다. legacy mode는 artifactType/contentId 기준이며 current animation+domainType=character를 legacy character_animation으로 재해석하지 않는다. 두 mode가 섞이면 차단한다.
4. generated_image_evaluation_v1의 evaluation_result.json, 완료 평가 리포트, source 또는 file-set manifest, 각 파일 SHA-256, fatal gate 근거가 서로 같은 평가 결과를 가리키는지 확인한다.
5. evaluationStatus=completed, result=PASS, passForProjectCopy=true, promotionStatus=not_promoted인지 확인한다. Conditional Pass, Fail, insufficient evidence, incomplete, caller가 적어준 Pass 문구는 승격 근거로 인정하지 않는다.
6. 현재 source SHA-256이 평가 당시 SHA-256과 같은지 확인한다. 파일 세트이면 파일 수, 이름, 역할, 순서와 모든 hash를 함께 검증한다.
7. source가 candidate, preview, contact sheet, thumbnail 또는 평가 첨부 이미지가 아닌 실제 평가 완료 원본인지 확인한다.
8. project target을 외부 입력이 아니라 ContentFolderStructureGuide.md, routing registry와 정확한 도메인 가이드로 계산한다. current character single-image는 `Assets/ImagesGenerated/Character/portrait/{contentId}.portrait.png`, current character animation은 `Assets/ImagesGenerated/Character/animation`, battle current background는 canonical battle background target을 사용한다. stage/environment는 권위 있는 target contract가 없으면 추정하지 않고 extension blocker를 반환한다.
9. 복사 전에 모든 target, 기존 PNG와 .meta, 충돌, overwrite 승인, 폴더와 importer 규칙을 preflight한다.
10. 기존 target 또는 .meta가 있으면 replaceExisting=true와 replacementApprovalRef가 모두 있어야 한다. 승인된 교체에서는 기존 .meta와 GUID를 보존한다.
11. 모든 gate가 통과한 경우에만 평가된 source bytes를 변경 없이 Assets/ImagesGenerated의 canonical target으로 복사한다. resize, crop, 재압축, 재생성, 후보 교체를 하지 않는다.
12. 신규 asset의 .meta는 승인된 Unity import 절차로 생성하고 다른 asset의 GUID를 재사용하지 않는다. 도메인별 Sprite/import/slicing 설정을 적용한다.
13. 복사 후 source와 project SHA-256이 모든 파일에서 같은지 확인한다. 파일 세트는 전부 성공했을 때만 promoted 처리한다.
14. builder 또는 consumer가 Assets/ImagesGenerated를 지원하는지 별도로 확인한다. Resources 하드코딩이면 복제하지 말고 builder_path_migration_required로 보고한다.
15. 생성 도구 실행, 이미지 다운로드, 이미지 수정, 재평가, 점수 변경, Slack 게시, Git commit/push/merge, 배포는 수행하지 않는다.
16. 평가 근거가 없거나 Pass가 아니거나 경로가 모호하면 프로젝트를 수정하지 않고 이전 단계가 다시 처리해야 할 blocker를 반환한다.

Output:
- Request ID
- Artifact Type / Content ID
- Evaluation Package ID / Asset Type / Domain Type / Structure Profile
- Status
- Resolved Evaluation Record ID
- Evaluation Result / Completion Identity
- Resolved Local Evaluation Workspace
- Evaluated Source Paths / SHA-256
- Project Target Paths / SHA-256
- Replacement Mode / Approval Reference
- Copy Verification
- Unity .meta / GUID / Import Status
- Promotion Status
- Consumer Readiness
- Blockers
- Required Next Actions

실패 시 Output:
- status: blocked | not_promoted | copy_failed
- failureType: promotion_identity_mode_conflict | evaluation_package_not_found | evaluation_package_hash_mismatch | background_structure_profile_mismatch | background_promotion_adapter_mismatch | background_promotion_target_contract_missing | legacy_current_identity_conflict | 기존 promotion failure token
- 실패한 단계와 근거
- 프로젝트가 변경되지 않았는지 여부
- 부분 복사가 발생했다면 정확한 대상과 정리 필요 상태
- 보존된 평가 source와 report 식별 정보
- 되돌려야 할 선행 작업: download | evaluation | approval | path migration | none

검증:
- 외부 입력에 로컬 source나 project target 경로가 없어야 한다.
- package mode는 evaluationPackageId+assetType+domainType+exact structureProfile을 보존하고 legacy artifactType과 혼합하지 않아야 한다.
- character_single_image+domainType=character 및 animation+domainType=character current package row를 legacy character_animation이나 유사 경로로 대체하지 않아야 한다.
- icon/background가 동일 PNG여도 promotion row와 adapter를 교환하지 않아야 한다.
- 평가 리포트와 현재 source hash가 묶인 정확한 Pass만 허용해야 한다.
- 복사 전 모든 단일 파일/파일 세트 대상의 preflight가 끝나야 한다.
- 모든 project target은 Assets/ImagesGenerated 아래여야 한다.
- Assets/Resources로 우회 복사하지 않아야 한다.
- 교체 시 기존 .meta와 GUID가 보존되어야 한다.
- promoted이면 모든 source/project SHA-256이 같아야 한다.
- 프로젝트 복사와 consumer readiness를 별도 상태로 보고해야 한다.
- 생성, 다운로드, 평가, Slack, Git, 배포를 수행하지 않아야 한다.
~~~
