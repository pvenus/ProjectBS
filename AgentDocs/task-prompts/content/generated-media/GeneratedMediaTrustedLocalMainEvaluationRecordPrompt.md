# Generated Media Trusted-Local MAIN Evaluation Record Prompt

```text
역할: generated_media_trusted_local_main_evaluation_record_projector

필수 가이드:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaTrustedLocalMainEvaluationRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaDeterministicFastPathGuide.md

Input:
- projectionInput: exact generated_media_trusted_local_main_evaluation_projection_input_v1
- independentEvaluationEvidencePath/Sha256: immutable canonical evidence
- sourceBoundReceiptPath/Sha256: immutable canonical source-bound receipt
- indexPath/indexBeforeSha256: exact CAS preimage

작업:
1. adapter `generated_media_trusted_local_main_evaluation_record_adapter@1.0.0` / `c76b11ee51f641da78b54048c670658628e379ce9f74f8b9cb878c1c9742953e`, input schema와 output `generated_media_trusted_local_main_evaluation_record_v1`을 검증한다.
2. media path/SHA/bytes, profile/reference policy, evaluation evidence raw SHA와 source-bound receipt raw SHA를 검증한다. receipt의 registered pointer content SHA, evidence content/media SHA가 모두 동일해야 한다.
3. independent evidence의 existing PASS|FAIL|unavailable decision과 ordered facts를 그대로 사용한다. 평가·이미지 inspection·score 계산·decision 변경·chat-only 근거 사용을 하지 않는다. numeric score는 금지한다.
4. pure helper generated_media_trusted_local_main_evaluation_projector_v1.mjs로 exact record ID/path/JCS+LF bytes, record SHA와 sorted index projection을 계산한다.
5. record-first atomic no-clobber 후 reread/hash하고 exact indexBeforeSha256 CAS로 index를 append한다. identical record+entry만 reused_identical이다.
6. provider receipt를 요구·생성·합성하지 않는다. provider/media/planning/routing/postprocess/evaluation decision/preservation/promotion/copy를 실행하지 않는다.

Output:
- status: completed | blocked
- adapterKey / adapterPayloadSha256
- evaluationRecordId / evaluationRecordPath / evaluationPayloadSha256 / evaluationRecordSha256
- result: PASS | FAIL | unavailable
- indexPath / indexBeforeSha256 / indexAfterSha256
- reuseStatus: created | reused_identical
- publicationState: local_unpublished
- providerCalled=false / submitCount=0 / cost=0

실패는 guide의 exact trusted_local_evaluation_* token 하나이며 partial index write나 evidence 수정 없이 종료한다.
```
