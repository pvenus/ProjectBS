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

## Exact G2 fit successor

`projectbs_character_open_ink_source_bound_green_carrier_fit@1.0.0` /
`ca3102e7369da6513b4c4e462e68b373da618a2b1630a393d803d37136d812df`
이면 위 v1 profile/helper를 바꾸지 말고
`helpers/generated_media_source_bound_chroma_fit_profile_v1.json`과
`helpers/generated_media_source_bound_chroma_fit_v1.mjs`만 사용한다. Exact
G2 source/receipt/evidence를 검증한 뒤 v1 recovery, registered premultiplied
box fit, canonical serialization 순서만 실행한다. Provider 호출, manual mask,
host resampler, repaint, arbitrary crop/scale/recenter는 금지한다. Record-first,
CAS/no-clobber, reused-identical 및 preservation-then-evaluation 경계는
GeneratedMediaSourceBoundMainCompletionGuide.md를 따른다.

## Exact G2 residual-carrier v2

V1 fit candidate의 partial-alpha positive-green count가 6,543이면 그 candidate는
rejected/non-authoritative로 유지하고 재실행·수리하지 않는다. 정확히
`projectbs_character_open_ink_source_bound_green_carrier_fit@2.0.0` /
`84db44afba6bce328a51f078f2147055846f282de71b2c56b9d7876264f9bccf`
profile과 `generated_media_source_bound_chroma_fit_v2.mjs`만 사용한다.
Registered expanded fringe/root evidence, inverse-composite model, unchanged
integer fit, exact 457/442/15 target masks와 post-green=0을 모두 검증한다.
Alpha0 RGB는 `[0,0,0]`이어야 한다. Exact mask/model 밖의 recolor/manual mask/
crop/repaint/identity edit/provider call은 금지하고 record-v2를 no-clobber로
한 번만 만든다.

G3 edited source에는 별도
`projectbs_character_open_ink_source_bound_green_carrier_fit_g3_edit@1.0.0` /
`f1b9563f271334c5addbf780bec1bca886f540d1a804e93684f56774c516a086`
만 사용한다. Exact source `7394278...3e4a`와 edit receipt `df9921...3cc3`,
176/179 rational scale, round-half-up 1074 height, 704x1074, x160/y231,
bbox `[160,231,863,1304]`, exact masks/output hashes가 모두 일치해야 one
no-clobber invocation을 허용한다. G2 identity와 혼합하지 않는다.

Final correction은 G2
`projectbs_character_open_ink_source_bound_green_carrier_fit@3.0.0` /
`5188d2bd92fdf22dded70fe8e3ab60f1fee1aa79ac6072845883072d99a875c2`
또는 G3 `projectbs_character_open_ink_source_bound_green_carrier_fit_g3_edit@2.0.0` /
`40cf8dcfbdc9043d1cdadeca64ee34ef8a11566140aa1e0ac8cc0d3b5baae425`
중 exact content binding 하나만 선택한다. 원본 RGB source/receipt에서 이전
결과를 메모리 재현·검증한 뒤 G2 exact 400+1 mask 또는 G3 exact partial
silhouette-edge inverse-composite mask만 적용한다. Rejected alpha PNG 입력,
recursive edit, hue neutralization, repaint, crop, manual mask, provider call은
금지한다. Record-v3/receipt-v3는 exactly one fresh no-clobber invocation이다.
