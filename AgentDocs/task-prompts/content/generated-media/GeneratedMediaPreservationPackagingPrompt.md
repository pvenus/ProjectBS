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
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v2_path}
- promptRecordId: {generated_media_prompt_v3_record_id_or_omit_for_accepted_result}
- generationRecordId: {generated_media_generation_v2_record_id}
- generationRecordSha256: {canonical_generation_record_sha256}
- acceptedResultCaptureRecordId: {generated_media_accepted_result_capture_v1_record_id_or_omit}; generationRecordId와 상호 배타적
- acceptedResultCaptureRecordPath: {exact_canonical_project_relative_capture_record_path_or_omit}
- acceptedResultCaptureRecordSha256: {canonical_capture_record_sha256_or_omit}
- acceptedResultCaptureReceipt: {exact_generated_media_accepted_result_capture_receipt_v1_or_omit}
- acceptedResultCaptureReceiptSha256: {exact_receiptPayloadSha256_or_omit}
- acceptedPromptEvidence: animation={source=accepted_result_capture,providerPromptPayloadHash,promptFileSha256} | unavailable-prompt character_single_image={source=accepted_result_capture,status=unavailable_observed,claim=not_claimed}_or_omit
- correctiveSingleImageInput: {exact generated_media_corrective_single_image_input_v1_or_omit; requires accepted-result character_single_image}
- singleImageBackgroundNormalizationPlan: {exact closed v1 or source-bound generated_media_border_palette_checkerboard_alpha_plan_v2_or_omit}
- gifTimingQuantizationPlan: {exact six-frame uniform-8fps centisecond plan_or_omit}
- gifBoundaryChromaNormalizationPlan: {exact accepted-source generated_media_gif_observed_boundary_chroma_plan_v2_or_omit}

작업:
0. exact noninteractive execution policy 범위의 read/hash/inspection/schema/test, accepted-output preservation, bounded package write는 재승인 없이 수행한다. 기존 immutable/no-clobber/idempotency 검증은 유지하며 overwrite/delete/out-of-root/scope expansion은 새 권한 blocker로 중단한다.
1. repository와 현재 PC의 evaluation staging root를 내부적으로 확인한다.
2. 정확히 한 input branch를 검증한다. strict branch는 기존 그대로 planning/prompt/generation identity, canonical generationRecordSha256, generationStatus=generated와 provider refs를 요구한다. accepted-result animation branch는 기존 capture record/index/receipt, source task/tool-call, prompt/settings/reference/master/GIF/frame, historical submit=1/retry=0 closure를 그대로 요구한다. accepted-result character_single_image branch는 exact one canonical PNG와 source/target raw-byte equality, authenticated acceptance exact SHA, distinct `accepted_project_candidate` role, identity/edit-target authority=false를 요구하고 prior `visual_reference_only_not_identity_or_edit_target` role은 변경하지 않으며 historical execution/prompt/settings/count의 closed `unavailable_observed`/`not_claimed` shape를 허용한다. 두 분기 모두 preSubmitGateAttestation=`not_claimed_post_result_capture`를 요구한다. mixed/partial/unknown 또는 still/animation member 혼합은 즉시 차단한다.
2a. accepted-result에 authoritative `generated_media_prompt_v3`가 없으면 fake prompt record를 만들거나 요구하지 않는다. animation은 capture의 exact `providerPromptPayloadHash`와 recovered prompt raw file SHA를 closed `acceptedPromptEvidence`로 투영하고 실제 recovered bytes와 일치시킨다. historical prompt가 unavailable인 character_single_image는 `source=accepted_result_capture,status=unavailable_observed,claim=not_claimed`만 투영하며 hash/path/prose를 만들지 않는다. 어느 분기도 generation-v2 gate/cost PASS를 추론하지 않는다.
2b. accepted-result character_single_image의 planning은 current checkout file equality로 검증하지 않는다. capture-bound routing record raw SHA를 검증하고 그 authoringHandoff의 planningHandoffPath/planningSnapshotHash/ordered sourcePlanningFiles만 사용한다. fresh origin/main에 reachable한 Git history에서 같은 path의 exact raw SHA blob을 하나씩 resolve하고, current blob이 나중 revision이면 historical blob을 planning/에 materialize한다. closed `acceptedPlanningEvidence`에 handoff raw SHA와 ordered path/role/SHA/gitBlobOid를 기록한다. local-only/unreachable commit, 다른 path, 재구성 bytes, current 내용으로의 치환은 금지한다. lineage 불일치는 `accepted_result_planning_lineage_mismatch`, exact reachable blob 부재는 `accepted_result_historical_planning_unresolvable`, distinct blob ambiguity는 `accepted_result_historical_planning_ambiguous`로 member write 전에 중단한다. strict generation 및 accepted animation branch는 기존 current validation을 그대로 유지한다.
2c. accepted corrective character_single_image이면 기존 accepted capture/receipt와 closed corrective input을 함께 검증한다. authority main, request/content, capture/reference/base-prompt hash, corrective prompt hash, official generation task/attempt, exact output PNG hash/dimensions/mode, providerCalled=true/submitCount=1/retryCount=0이 모두 일치해야 한다. fake generation-v2나 새 prompt/capture record를 만들지 않는다. 누락은 `preservation_input_branch_incomplete`, drift는 `corrective_single_image_evidence_mismatch`로 no-write 차단한다.
3. provider=imagegen과 current v2 adapter registry에서 assetType, requestedAdapterId, expectedStructureProfile이 모두 일치하는 row 하나를 확정한다. PixelLab/v1 row는 신규 입력으로 선택하지 않는다.
4. canonical preservationHashPayload/ID를 계산한다. 동일 payload 재실행은 기존 record를 resume/reuse하고 동일 ID의 다른 payload는 중단한다.
5. `.assembling/{requestId}/{preservationRecordId}.{attemptId}` 임시 경로에서만 provisional ref를 바꾸지 않고 원본을 download/export한다.
6. character/icon/background는 각각 별도 registered adapter로 원본 단일 이미지를 보존한다. icon과 background가 같은 PNG 형태여도 adapter/profile/evaluation identity를 교환하지 않는다. 신규 animation은 provider가 직접 반환한 playable animated GIF 원본과 hash를 먼저 보존하고, 그 GIF를 닫았다 다시 열어 frame count/order/timing/loop/full-canvas disposal을 검증한다. 승인된 timeline-wide 투명/outline/chroma/anchor 정책만 일관되게 적용한 완성 GIF를 저장하고 다시 열어 실제 timeline PNG frames를 추출한다. still image, contact sheet, sprite sheet, video, 독립 frames를 GIF로 합성하지 않는다. frame별 crop/scale/recenter를 금지하고 모든 파일 SHA-256을 기록한다.
6a. corrective RGB PNG에 baked checkerboard가 있고 exact plan이 있을 때만 `border_exact_checkerboard_boundary_flood_v1`을 실행한다. border-only exact two-color unique checker pattern, coordinate-exact RGB, 4-connectivity, retain-source-RGB, background alpha=0/otherwise=255를 그대로 사용한다. threshold/tolerance/blur/morphology/erosion/feather/semantic mask/retouch는 금지한다. outer-boundary foreground contact 또는 pattern ambiguity는 각각 `checkerboard_foreground_contact_ambiguous`, `checkerboard_background_pattern_unsupported`; RGB/foreground loss, dimension/alpha/receipt mismatch는 `checkerboard_alpha_normalization_validation_failed`로 package 전에 차단한다. before/after hash와 exact algorithm/encoder parameters 및 closed receipt를 기록한다.
6b. source SHA가 정확히 `4498817999fb28323eb85f62afefcc33027341640b1a7ce990a3609b32eaeb7e`이고 v2 plan이 있을 때만 `border_frozen_palette_boundary_flood_v2`를 선택할 수 있다. 5,116 outer-boundary pixels에서 sorted unique 64 RGB palette와 palette/histogram/ordered-sequence hash, four corners, bottom/right lag 2..64 exact integer covariance signature를 재계산해 published fixture와 모두 일치시킨다. frozen palette exact RGB인 outer-boundary seed에서만 4-connect하고 exact palette match만 alpha=0으로 바꾼다. 다른 RGB/alpha=255, protected noncandidate RGB hash/bbox, dimensions, counts, mask hash와 normalized row-major RGBA hash는 published expected values와 일치해야 한다. threshold/tolerance/erosion/dilation/interior seed/manual mask/semantic repair 또는 다른 source는 금지하며 source/palette/corner drift, nonperiodic evidence, boundary foreground, noncandidate mutation을 distinct v2 token으로 no-write 차단한다. V1은 변경하지 않는다.
7. closed profile extension schema와 member order/count/relations를 검증한다. 누락/unknown field를 추론하거나 수리하지 않는다.
8. manifestPayload/hash/packageId 계산 후 `{evaluationStagingRoot}/{assetType}/{contentId}/{requestId}/{packageId}/`로 안전하게 finalize한다. 기존 동일 package는 byte/hash 검증 후 재사용하며 overwrite하지 않는다.
9. evaluator adapter까지 유효하면 evaluation_handoff_ready, 아니면 sealed blocked package와 blocker를 기록한다.
10. provider 생성/재시도, prompt 수정, 평가, 승격, Slack, Unity, Git, merge, 배포를 수행하지 않는다.
11. `generated_media_attack_coherent_master_to_gif_validation_receipt_v2`가 있으면 providerDidReturnGif=false, provider master IMAGE hash, exact six cells, completed GIF hash, six PNG hashes, GIF close/reopen과 reopened-timeline extraction을 먼저 검증한다. SAME generation role이 master→GIF-first→reopen→six PNG와 deterministic pelvis/baseline translation 및 verified neighboring-cell fragment removal까지 소유하며 preservation은 이를 재실행하거나 수리하지 않는다. completed GIF/PNGs의 shared width basis, pelvis/baseline drift=0px, uniform scale/timing/global palette, fully opaque background, no clipping/no neighboring fragments를 재확인한다. accepted evidence path/bytes/full guidance는 record/package에 복사하지 않는다. historical v1 accepted receipt는 이 mode를 승인하지 않는다.
11a. 위 coherent-master mode의 approved intent가 정확히 6 frames, uniform 8fps이면 125ms를 직접 쓰지 않는다. GIF centisecond schedule은 `[12,13,12,13,12,13]`, 즉 `[120,130,120,130,120,130]ms`만 허용하고 total=750ms/exact average=8fps/no zero delay/no loop extension을 검증한다. re-encode 후 GIF를 닫고 다시 열어 schedule을 재확인하며 before/after decoded full-canvas frame pixel hashes, 640x512 canvas, count/order/pelvis/baseline/clipping/fragment state가 동일해야 한다. closed quantization receipt를 package에 넣고 다른 mixed timing은 `gif_timeline_contract_mismatch`로 차단한다. 이 exact receipt가 있으면 기존 `timingUniform=true`는 모든 stored delay 정수의 동일성이 아니라 uniform 8/1fps intent가 이 canonical schedule로 충족됐다는 뜻이다. receipt가 없으면 기존 literal uniform-timeline 검증을 그대로 유지한다.
11b. accepted GIF SHA가 정확히 `8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621`이고 v2 plan이 있을 때만 `gif_exact_uniform_boundary_color_flood_v2` observed-boundary chroma branch를 실행한다. 각 reopened frame의 2,300/2,300 outer-boundary pixels와 four corners가 exact `(240,236,228)`인지, six boundary sequence hashes와 frame evidence JCS hash가 fixture와 일치하는지 검증한다. outer-boundary exact match에서만 4-connect하여 exact `(240,236,228)` alpha를 clear하고 모든 nonmatching RGB/geometry/order/pelvis/baseline/clipping/fragments를 보존한다. 이후 exact `[12,13,12,13,12,13]`cs, 750ms, one-shot/no-loop로 저장하고 close/reopen 및 PNG extraction을 검증한다. 이 source-bound v2에는 `#F2EFE6`을 대입하지 않으며 다른 source/color/fraction/corner/mask/timing drift는 no-write 차단한다. 기존 v1/strict/provider-native chroma 규칙은 변경하지 않는다.
12. accepted-result branch는 preservation/evaluation package만 승인한다. promotion은 수행하거나 승인하지 않으며 이후 strict evaluation PASS와 explicit project mapping이 별도로 필요하다.

Output:
- Request / Asset / Domain / Content
- Generation Record ID/SHA-256 and Preservation Record ID/Payload Hash/Record Path
- Adapter / Structure Profile / Preserved Originals / Extracted Members
- Member Hashes / Manifest Payload Hash / Package ID / Canonical Package Path
- Evaluation Readiness / Blockers / Evaluation Request / Next Task

실패 시 Output:
- status: blocked | failed
- failureType: missing_planning_handoff_v2 | missing_routing_v2 | missing_prompt_v3 | missing_generation_v2 | missing_accepted_result_capture_v1 | accepted_result_capture_hash_mismatch | accepted_result_capture_not_authorized | accepted_result_capture_receipt_mismatch | accepted_result_prompt_evidence_mismatch | preservation_input_branch_conflict | preservation_input_branch_incomplete | preservation_input_unknown_field | corrective_single_image_evidence_mismatch | checkerboard_background_pattern_unsupported | checkerboard_foreground_contact_ambiguous | checkerboard_alpha_normalization_validation_failed | border_palette_source_fixture_mismatch | border_palette_checkerboard_coherence_failed | border_palette_foreground_contact_detected | border_palette_normalization_validation_failed | accepted_result_planning_lineage_mismatch | accepted_result_historical_planning_unresolvable | accepted_result_historical_planning_ambiguous | generation_not_ready | generation_record_hash_mismatch | record_identity_mismatch | preservation_record_collision | canonical_serializer_unsupported | serializer_settings_mismatch | serializer_output_hash_mismatch | serializer_reopen_validation_failed | preservation_index_collision | preservation_index_cas_mismatch | preservation_record_index_mismatch | unsupported_provider | provider_result_ref_missing | provider_result_unavailable_requires_generation_task | unsupported_preservation_adapter | evaluation_staging_root_not_configured | staging_project_path_violation | original_download_failed | provider_export_failed | source_not_original | source_hash_mismatch | provider_animated_gif_source_mismatch | gif_timeline_contract_mismatch | extraction_failed | fixed_cell_contract_mismatch | scale_lock_violation | anchor_mapping_mismatch | vertical_motion_policy_violation | chroma_key_scope_violation | gif_observed_boundary_source_fixture_mismatch | gif_observed_boundary_color_ambiguous | gif_observed_boundary_corner_mismatch | gif_observed_boundary_normalization_validation_failed | gif_first_sequence_violation | frame_order_mismatch | member_hash_mismatch | manifest_validation_failed | package_finalize_failed | package_collision | package_seal_failed | evaluation_adapter_missing
- 완료 state / 보존된 파일과 hash / 재시도 가능 지점 / Required Next Action

검증:
- normalized accepted-result output은 `helpers/generated_media_canonical_serializers_v1.mjs`의 exact writer로만 직렬화하고 `generated_media_serialization_receipt_v1`을 기록한다. serializer settings/output/reopen hash가 다르면 `serializer_output_hash_mismatch`로 중단한다.
- current v2 record를 먼저 no-clobber write/hash한 뒤 canonical `preservation_index.json`을 CAS append하고 `generated_media_preservation_evaluation_handoff_v2`를 닫는다. 동일 bytes/entry만 `reused_identical`이며 stale/different index는 `preservation_index_cas_mismatch` 또는 `preservation_record_index_mismatch`로 중단한다.
- provider 생성 호출과 prompt 변경이 없어야 한다.
- 외부 placeholder 없이 registry가 child guide 하나를 결정해야 한다.
- adapter/profile schema와 모든 hash/package identity가 재계산되어야 한다.
- 신규 animation은 scalar animationRequestId, provider_native_animated_gif, final frame count/timing/loop, scale lock, anchor-only drift correction과 exact GIF-timeline extraction을 만족해야 한다.
- accepted coherent-master mode가 있으면 provider가 GIF가 아닌 one coherent six-cell master IMAGE를 반환했고 generation role이 completed GIF-first, close/reopen, six PNG extraction을 끝냈음을 v2 compact receipt와 members로 증명해야 한다.
- preservation record 경로와 canonical package 경로가 guide 공식과 일치해야 한다.
- staging/project target이 분리되어야 한다.
- 평가 및 이후 단계가 실행되지 않아야 한다.
```
