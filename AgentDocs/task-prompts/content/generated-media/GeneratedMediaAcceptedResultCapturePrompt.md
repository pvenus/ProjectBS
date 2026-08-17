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
- sourceGenerationTaskId / exactProviderToolCallId / providerTool / observed provider result ref(optional)
- historicalSubmitCount=1 / historicalRetryCount=0
- prompt path / raw file SHA-256 / providerPromptPayloadHash
- settings path / raw file SHA-256
- exact ordered submitted reference role/path/SHA-256 array
- provider master path/SHA-256/mediaType
- completed GIF path/SHA-256/width/height/frameCount
- exact ordered frameIndex/path/SHA-256 array

작업:
1. live authority와 immutable request/routing identity를 검증하되 planning/routing/prompt bytes를 수정하지 않는다.
2. authenticated acceptance가 exact accepted artifact SHA를 명시하는지 검증한다.
3. source task와 exact provider tool-call evidence에서 historical submit=1, retry=0을 검증한다. 새 provider/capability/cost call은 절대 하지 않는다.
4. prompt, settings, 모든 submitted reference, provider master, completed GIF, 모든 frame을 raw bytes로 읽어 SHA-256과 member count/order를 검증한다. reference role을 identity/edit target으로 승격하지 않는다.
5. capabilityEvidenceStatus=unavailable_observed, costEvidenceStatus=unavailable_observed, preSubmitGateAttestation=not_claimed_post_result_capture를 그대로 기록한다. capability/cost/PASS/zero 값을 만들거나 과거 pre-submit gate가 통과했다고 주장하지 않는다.
6. GeneratedMediaRecordGuide의 closed capture payload/JCS/ID/path를 계산하고 record-first, capture_index CAS append, no-clobber로 저장한다. 동일 payload/bytes는 reused_identical이며 occupied ID의 다른 bytes는 중단한다.
7. closed terminal receipt를 반환한다. preservationAuthorized=true, evaluationAuthorized=true, promotionAuthorized=false이며 promotion은 strict evaluation PASS와 explicit project mapping 전까지 금지한다.
8. preservation/evaluation/promotion 자체, media 복사/변환, provider 재호출, planning/routing/prompt/generation record 수정, Unity, Git publication을 수행하지 않는다.

Output:
- schemaVersion: generated_media_accepted_result_capture_receipt_v1
- state / requestId / optional animationRequestId
- captureRecordId/path/raw SHA-256 / capturePayloadSha256 / captureIndexSha256
- sourceGenerationTaskId / providerMasterSha256 / completedGifSha256
- providerCalled=false / submitCount=0 / retryCount=0
- historicalSubmitCount=1 / historicalRetryCount=0
- capabilityEvidenceStatus=unavailable_observed / costEvidenceStatus=unavailable_observed
- preSubmitGateAttestation=not_claimed_post_result_capture
- preservationAuthorized / evaluationAuthorized / promotionAuthorized=false
- nextStep / receiptPayloadSha256

실패 시:
- state=blocked
- failureType: accepted_capture_acceptance_missing | accepted_capture_execution_evidence_missing | accepted_capture_identity_mismatch | accepted_capture_evidence_hash_mismatch | accepted_capture_incomplete_member_set | accepted_capture_false_attestation | accepted_capture_record_collision | accepted_capture_index_cas_failed
- providerCalled=false / submitCount=0 / retryCount=0
- preservationAuthorized=false / evaluationAuthorized=false / promotionAuthorized=false / nextStep=stop
```
