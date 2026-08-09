# Generated Media Preservation and Packaging Prompt

이미 생성된 provider 결과를 유형별 adapter로 다운로드/export/추출하고,
공통 evaluation package로 seal하는 provider-neutral 실행 프롬프트입니다.

## Prompt

```text
현재 ProjectBS 저장소에서 생성 완료된 media 요청 하나의 preservation/packaging task만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- promptRecordId: {generated_media_prompt_v2_or_validated_read_only_legacy_v1_record_id}
- generationRecordId: {generated_media_generation_v1_record_id}
- generationRecordSha256: {canonical_generation_record_sha256}

작업:
1. repository와 현재 PC의 evaluation staging root를 내부적으로 확인한다.
2. planning/prompt/generation identity, canonical generationRecordSha256, generationStatus=generated와 provider refs를 검증한다.
3. GeneratedMediaPreservationPackagingGuide.md의 registry에서 canonical provider, assetType, requestedAdapterId, expectedStructureProfile이 모두 일치하는 row 하나를 확정하고 그 row에 등록된 child pipeline guide를 내부적으로 읽는다. 일치 row가 0개 또는 복수이면 중단하며 임의 guide를 선택하지 않는다.
4. canonical preservationHashPayload/ID를 계산한다. 동일 payload 재실행은 기존 record를 resume/reuse하고 동일 ID의 다른 payload는 중단한다.
5. `.assembling/{requestId}/{preservationRecordId}.{attemptId}` 임시 경로에서만 provisional ref를 바꾸지 않고 원본을 download/export한다.
6. adapter가 요구하는 경우에만 lossless extraction을 실행하고 모든 파일 SHA-256을 기록한다.
7. closed profile extension schema와 member order/count/relations를 검증한다. 누락/unknown field를 추론하거나 수리하지 않는다.
8. manifestPayload/hash/packageId 계산 후 `{evaluationStagingRoot}/{assetType}/{contentId}/{requestId}/{packageId}/`로 안전하게 finalize한다. 기존 동일 package는 byte/hash 검증 후 재사용하며 overwrite하지 않는다.
9. evaluator adapter까지 유효하면 evaluation_handoff_ready, 아니면 sealed blocked package와 blocker를 기록한다.
10. provider 생성/재시도, prompt 수정, 평가, 승격, Slack, Unity, Git, merge, 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content
- Generation Record ID/SHA-256 and Preservation Record ID/Payload Hash/Record Path
- Adapter / Structure Profile / Preserved Originals / Extracted Members
- Member Hashes / Manifest Payload Hash / Package ID / Canonical Package Path
- Evaluation Readiness / Blockers / Evaluation Request / Next Task

실패 시 Output:
- status: blocked | failed
- failureType: missing_generation_record | generation_not_ready | generation_record_hash_mismatch | record_identity_mismatch | preservation_record_collision | provider_result_ref_missing | provider_result_unavailable_requires_generation_task | unsupported_preservation_adapter | evaluation_staging_root_not_configured | staging_project_path_violation | original_download_failed | provider_export_failed | source_not_original | source_hash_mismatch | extraction_failed | structure_contract_mismatch | manifest_validation_failed | package_finalize_failed | package_collision | package_seal_failed | evaluation_adapter_missing
- 완료 state / 보존된 파일과 hash / 재시도 가능 지점 / Required Next Action

검증:
- provider 생성 호출과 prompt 변경이 없어야 한다.
- 외부 placeholder 없이 registry가 child guide 하나를 결정해야 한다.
- adapter/profile schema와 모든 hash/package identity가 재계산되어야 한다.
- preservation record 경로와 canonical package 경로가 guide 공식과 일치해야 한다.
- staging/project target이 분리되어야 한다.
- 평가 및 이후 단계가 실행되지 않아야 한다.
```
