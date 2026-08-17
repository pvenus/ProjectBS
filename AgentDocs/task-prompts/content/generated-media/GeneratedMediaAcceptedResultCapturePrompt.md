# Generated Media Accepted Result Capture Prompt

## Prompt

```text
현재 ProjectBS의 user-accepted generated result 하나에 대해 accepted_post_result_capture_v1 증거 봉인만 수행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md

Input:
- targetRecordSchema: generated_media_accepted_result_capture_v1
- authenticatedUserAcceptanceMessageId
- requestId / planningSnapshotHash / routingRecordId / routingRecordSha256
- assetType / domainType / contentId / animationRequestId(optional only for animation)
- animation: sourceGenerationTaskId / exactProviderToolCallId / providerTool / optional observed result ref; historicalSubmitCount=1 / historicalRetryCount=0; prompt/settings/reference/master/GIF/frame evidence
- character_single_image: exact one PNG source path/SHA-256/byteLength, authenticated acceptance of that SHA, and historical execution/prompt/settings evidence status (`unavailable_observed` + `not_claimed` when unavailable)

작업:
1. live authority와 immutable request/routing identity를 검증하되 planning/routing/prompt bytes를 수정하지 않는다.
2. authenticated acceptance가 exact accepted artifact SHA를 명시하는지 검증한다.
3. animation은 기존대로 source task와 exact provider tool-call evidence에서 historical submit=1, retry=0을 검증한다. character_single_image는 historical execution/prompt/settings evidence가 없으면 정확히 `unavailable_observed`/`not_claimed`로 기록하고 provider/tool-call/count/PASS를 만들지 않는다. 새 provider/capability/cost call은 절대 하지 않는다.
4. animation은 기존 prompt/settings/reference/master/GIF/frame raw-byte closure를 그대로 검증한다. character_single_image는 exact one PNG source만 허용하고 raw SHA-256/byteLength/mediaType을 검증한다. authenticated acceptance로 그 exact bytes에 별도 `accepted_project_candidate` capture role을 부여하되 기존 `visual_reference_only_not_identity_or_edit_target` 역할을 승격·재해석하거나 identity/edit-target 권한을 부여하지 않는다.
4a. character_single_image canonical target은 `AgentDocs/planning-data/generated-media-accepted-result-capture/v1/character_single_image/{contentId}/media/{sha256}.png`이다. source bytes를 변환 없이 no-clobber copy하고 target raw SHA를 재검증한다. 동일 bytes는 재사용하고 다른/non-PNG occupant는 `accepted_capture_canonical_target_collision`로 중단한다. animation member와 still-image member의 혼합을 거부한다.
5. capabilityEvidenceStatus=unavailable_observed, costEvidenceStatus=unavailable_observed, preSubmitGateAttestation=not_claimed_post_result_capture를 그대로 기록한다. capability/cost/PASS/zero 값을 만들거나 과거 pre-submit gate가 통과했다고 주장하지 않는다.
6. GeneratedMediaRecordGuide의 closed capture payload/JCS/ID/path를 계산하고 record-first, capture_index CAS append, no-clobber로 저장한다. 동일 payload/bytes는 reused_identical이며 occupied ID의 다른 bytes는 중단한다.
7. closed terminal receipt를 반환한다. preservationAuthorized=true, evaluationAuthorized=true, promotionAuthorized=false이며 promotion은 strict evaluation PASS와 explicit project mapping 전까지 금지한다.
8. preservation/evaluation/promotion 자체, canonical target 이외 media 복사 또는 모든 media 변환, provider 재호출, planning/routing/prompt/generation record 수정, Unity를 수행하지 않는다. 계약 수정 task는 capture/Git publication을 수행하지 않으며 실제 capture task만 record/index와 unchanged-byte canonical PNG를 게시 대상으로 준비한다.

Output:
- schemaVersion: generated_media_accepted_result_capture_receipt_v1
- state / requestId / optional animationRequestId
- captureRecordId/path/raw SHA-256 / capturePayloadSha256 / captureIndexSha256
- animation: sourceGenerationTaskId / providerMasterSha256 / completedGifSha256
- character_single_image: acceptedImageSha256 / canonicalCapturePath
- providerCalled=false / submitCount=0 / retryCount=0
- animation: historicalSubmitCount=1 / historicalRetryCount=0
- character_single_image: historicalSubmitCount=unavailable_observed / historicalRetryCount=unavailable_observed when unavailable
- capabilityEvidenceStatus=unavailable_observed / costEvidenceStatus=unavailable_observed
- preSubmitGateAttestation=not_claimed_post_result_capture
- preservationAuthorized / evaluationAuthorized / promotionAuthorized=false
- nextStep / receiptPayloadSha256

실패 시:
- state=blocked
- failureType: accepted_capture_acceptance_missing | accepted_capture_execution_evidence_missing | accepted_capture_identity_mismatch | accepted_capture_evidence_hash_mismatch | accepted_capture_incomplete_member_set | accepted_capture_false_attestation | accepted_capture_record_collision | accepted_capture_index_cas_failed | accepted_capture_canonical_target_collision
- providerCalled=false / submitCount=0 / retryCount=0
- preservationAuthorized=false / evaluationAuthorized=false / promotionAuthorized=false / nextStep=stop
```
