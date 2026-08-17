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
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v2_path}
- promptRecordId: {generated_media_prompt_v3_record_id}
- generationRecordId: {generated_media_generation_v2_record_id}
- generationRecordSha256: {canonical_generation_record_sha256}

작업:
1. repository와 현재 PC의 evaluation staging root를 내부적으로 확인한다.
2. planning/prompt/generation identity, canonical generationRecordSha256, generationStatus=generated와 provider refs를 검증한다.
3. provider=imagegen과 current v2 adapter registry에서 assetType, requestedAdapterId, expectedStructureProfile이 모두 일치하는 row 하나를 확정한다. PixelLab/v1 row는 신규 입력으로 선택하지 않는다.
4. canonical preservationHashPayload/ID를 계산한다. 동일 payload 재실행은 기존 record를 resume/reuse하고 동일 ID의 다른 payload는 중단한다.
5. `.assembling/{requestId}/{preservationRecordId}.{attemptId}` 임시 경로에서만 provisional ref를 바꾸지 않고 원본을 download/export한다.
6. character/icon/background는 각각 별도 registered adapter로 원본 단일 이미지를 보존한다. icon과 background가 같은 PNG 형태여도 adapter/profile/evaluation identity를 교환하지 않는다. 신규 animation은 provider가 직접 반환한 playable animated GIF 원본과 hash를 먼저 보존하고, 그 GIF를 닫았다 다시 열어 frame count/order/timing/loop/full-canvas disposal을 검증한다. 승인된 timeline-wide 투명/outline/chroma/anchor 정책만 일관되게 적용한 완성 GIF를 저장하고 다시 열어 실제 timeline PNG frames를 추출한다. still image, contact sheet, sprite sheet, video, 독립 frames를 GIF로 합성하지 않는다. frame별 crop/scale/recenter를 금지하고 모든 파일 SHA-256을 기록한다.
7. closed profile extension schema와 member order/count/relations를 검증한다. 누락/unknown field를 추론하거나 수리하지 않는다.
8. manifestPayload/hash/packageId 계산 후 `{evaluationStagingRoot}/{assetType}/{contentId}/{requestId}/{packageId}/`로 안전하게 finalize한다. 기존 동일 package는 byte/hash 검증 후 재사용하며 overwrite하지 않는다.
9. evaluator adapter까지 유효하면 evaluation_handoff_ready, 아니면 sealed blocked package와 blocker를 기록한다.
10. provider 생성/재시도, prompt 수정, 평가, 승격, Slack, Unity, Git, merge, 배포를 수행하지 않는다.
11. `generated_media_attack_coherent_master_to_gif_validation_receipt_v2`가 있으면 providerDidReturnGif=false, provider master IMAGE hash, exact six cells, completed GIF hash, six PNG hashes, GIF close/reopen과 reopened-timeline extraction을 먼저 검증한다. SAME generation role이 master→GIF-first→reopen→six PNG와 deterministic pelvis/baseline translation 및 verified neighboring-cell fragment removal까지 소유하며 preservation은 이를 재실행하거나 수리하지 않는다. completed GIF/PNGs의 shared width basis, pelvis/baseline drift=0px, uniform scale/timing/global palette, fully opaque background, no clipping/no neighboring fragments를 재확인한다. accepted evidence path/bytes/full guidance는 record/package에 복사하지 않는다. historical v1 accepted receipt는 이 mode를 승인하지 않는다.

Output:
- Request / Asset / Domain / Content
- Generation Record ID/SHA-256 and Preservation Record ID/Payload Hash/Record Path
- Adapter / Structure Profile / Preserved Originals / Extracted Members
- Member Hashes / Manifest Payload Hash / Package ID / Canonical Package Path
- Evaluation Readiness / Blockers / Evaluation Request / Next Task

실패 시 Output:
- status: blocked | failed
- failureType: missing_planning_handoff_v2 | missing_routing_v2 | missing_prompt_v3 | missing_generation_v2 | generation_not_ready | generation_record_hash_mismatch | record_identity_mismatch | preservation_record_collision | unsupported_provider | provider_result_ref_missing | provider_result_unavailable_requires_generation_task | unsupported_preservation_adapter | evaluation_staging_root_not_configured | staging_project_path_violation | original_download_failed | provider_export_failed | source_not_original | source_hash_mismatch | provider_animated_gif_source_mismatch | gif_timeline_contract_mismatch | extraction_failed | fixed_cell_contract_mismatch | scale_lock_violation | anchor_mapping_mismatch | vertical_motion_policy_violation | chroma_key_scope_violation | gif_first_sequence_violation | frame_order_mismatch | member_hash_mismatch | manifest_validation_failed | package_finalize_failed | package_collision | package_seal_failed | evaluation_adapter_missing
- 완료 state / 보존된 파일과 hash / 재시도 가능 지점 / Required Next Action

검증:
- provider 생성 호출과 prompt 변경이 없어야 한다.
- 외부 placeholder 없이 registry가 child guide 하나를 결정해야 한다.
- adapter/profile schema와 모든 hash/package identity가 재계산되어야 한다.
- 신규 animation은 scalar animationRequestId, provider_native_animated_gif, final frame count/timing/loop, scale lock, anchor-only drift correction과 exact GIF-timeline extraction을 만족해야 한다.
- accepted coherent-master mode가 있으면 provider가 GIF가 아닌 one coherent six-cell master IMAGE를 반환했고 generation role이 completed GIF-first, close/reopen, six PNG extraction을 끝냈음을 v2 compact receipt와 members로 증명해야 한다.
- preservation record 경로와 canonical package 경로가 guide 공식과 일치해야 한다.
- staging/project target이 분리되어야 한다.
- 평가 및 이후 단계가 실행되지 않아야 한다.
```
