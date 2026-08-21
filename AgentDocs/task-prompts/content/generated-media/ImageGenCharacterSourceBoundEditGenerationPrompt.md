# ImageGen Character Source-Bound Edit Generation Prompt

## Prompt

```text
exact registered character single-image source-bound edit route 하나를 실행해줘.

참조:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaSourceBoundMainCompletionGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md

Input:
- exact generated_media_source_bound_character_edit_route_v1 record/path/SHA
- profileKey: projectbs_character_open_ink_source_bound_single_edit@1.0.0
- profilePayloadSha256: aa65434f5fb9c22cb42db199c936ee414648b933f4b83c159065341f4e704011
- routeContractKey: projectbs_generated_media_source_bound_character_edit_route@1.0.0
- routeContractPayloadSha256: 77b4b9d4d9d5db7a2c2fb1cdb5ccb1812faffe535559fdb57400515f48e05359

실행:
1. authority/profile/route/source/generation receipt를 raw bytes로 검증한다. exact registered G3 tuple이 아니면 no-write 차단한다.
2. helpers/generated_media_source_bound_character_edit_route_v1.mjs로 route payload/ID/record SHA, exact approvalEvidence, execution scope/hash, gmedit1 idempotency, detached execution handoff/hash, index entry를 재계산한다. route의 initial state/createdAt과 index CAS가 contract와 다르면 submit하지 않는다.
3. callable prompt는 ordered six lines를 UTF-8 LF로 join하되 terminal LF가 없는 exact bytes이며 SHA-256은 3a90b405e4303362dc912a19117f0501fa4b0575ee188aaf8a6f8353fb7d255d다. JCS array hash는 line 검증 전용이다. referenced_image_paths=[exact immutable sourcePathEvidence] 하나만 함께 사용하고 source image는 수정·복사·재인코딩하지 않는다.
4. 정확히 한 번만 submit하고 retry하지 않는다. 호출 경계를 넘으면 providerCalled=true, submitCount=1, retryCount=0, costKnown=false다.
5. 반환 이미지를 reopen하여 one PNG/RGB/1024x1536/fully opaque/exact uniform #00FF00 background, bbox/occupancy, complete unclipped figure, exactly one retained brass closure, identity/equipment/orientation/mantle/sash/wind/no-mirror locks와 fringe/fragments 부재를 관찰한다.
6. 비적합 결과도 submit을 소비하고 stop_no_retry다. alpha/uncomposite/preservation/evaluation/promotion/project copy/Unity/provider recall은 실행하지 않는다.

Output은 compact terminal receipt로 route/profile/source/output hashes, state, outputConformance, providerCalled, submitCount, retryCount, costKnown=false, evaluationStatus=not_evaluated, projectCopyEligible=false, nextStep 또는 exact failureType만 반환한다.
```
