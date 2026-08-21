# Generated Media Source-Bound Chroma Uncomposite Prompt

## Prompt

```text
exact registered opaque-chroma provider master 하나를 source-bound true-alpha로 복구해줘.

참조 가이드:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaSourceBoundChromaRecoveryGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaOpenInkWashOpaqueChromaSuccessorGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md

Input:
- authorityMainSha: {exact_authoritative_main}
- contentId: {exact_character_content_id}
- requestId: {exact_planning_request_id}
- sourcePath: {evidence_path_only}
- sourceSha256: {exact_registered_source_sha256}
- generationReceiptPath: {exact_read_only_generation_receipt_path}
- generationReceiptSha256: {exact_registered_receipt_sha256}
- recoveryProfilePath: AgentDocs/planning-guides/content/generated-media/helpers/generated_media_source_bound_chroma_recovery_profile_v1.json
- recoveryProfileKey: projectbs_character_open_ink_source_bound_green_carrier_uncomposite@1.0.0
- recoveryProfilePayloadHash: b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746

작업:
1. authority/profile/source/generation receipt raw bytes를 다시 hash하고 exact 등록 tuple을 고른다. 기존 generation receipt의 output_nonconformant_no_retry, submitCount=1, retryCount=0, idempotency scope를 수정·재개·성공으로 재해석하지 않는다.
2. sourcePath는 evidence locator일 뿐 identity가 아니다. source SHA/byteLength/PNG RGB 1024x1536과 request/handoff/receipt/idempotency가 profile fixture와 하나라도 다르면 no-write 차단한다.
3. helpers/generated_media_source_bound_chroma_uncomposite_v1.mjs 한 버전만 사용한다. 전체 outer perimeter에서 greenExcess=G-max(R,B)를 측정하고 source floor, one four-connected edge carrier, exact enclosed component mask, exact positive one-ring digest/statistics를 profile과 비교한다.
4. exact match일 때만 carrier core alpha0, one-ring deterministic uncomposite/despill, 그 밖의 RGB byte-identical alpha255를 적용한다. manual mask/recolor/repaint/crop/resize/recenter/erosion/tolerance/threshold 변경/provider recall/raw-source mutation은 금지한다.
5. canonical RGBA8 serializer로 distinct no-clobber output을 만들고 reopen하여 canvas/alpha/corners/full border/foreground bbox/nontransformed bytes/recomposition/fringe/no-clipping을 검증한다. source는 before/after hash가 같아야 한다.
6. generated_media_source_bound_chroma_uncomposite_receipt_v1을 닫고 output no-clobber 확인 후 immutable record를 작성한 다음 postprocess_index를 sorted CAS append한다. occupied-different 또는 partial collision은 overwrite하지 않는다. exact completed bytes만 reused_identical이다.
7. preservation/evaluation/promotion/project copy/provider/Unity를 실행하지 않는다. nextStep은 preservation_then_independent_evaluation만 반환한다.

Output:
- status: recovered | reused_identical | blocked | failed
- recordId / recordPath / recordSha256 / recordPayloadSha256
- sourceSha256 / generationReceiptSha256
- recoveryProfileKey / recoveryProfilePayloadHash
- algorithmSettingsSha256 / sourceEvidenceSha256 / receiptPayloadSha256
- outputPath / outputSha256 / outputByteLength / width / height / colorMode
- outputValidation compact metrics
- providerCalled=false / submitCount=0 / retryCount=0
- evaluationStatus=not_evaluated / projectCopyEligible=false
- nextStep: preservation_then_independent_evaluation | stop

실패 시 failureType은 Source-Bound Chroma Recovery Guide의 exact token 하나이며 source/output/record/index를 수리·덮어쓰기하거나 다른 threshold/provider call로 진행하지 않는다.
```
