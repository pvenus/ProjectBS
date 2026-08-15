# ImageGen Single Animation Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 animation record 하나를 검증하고 정확히 한 animationRequestId의 coherent master를 ImageGen으로 생성해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenAnimationPipelineGuide.md

Input:
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- promptRecordId: {generated_media_prompt_v3_id}
- animationRequestId: {exact_single_id}
- executionMode: promotable_generation_v2 | hosted_builtin_preview_v1
- providerExecutionApproval: required only for promotable_generation_v2
- hostedPreviewApproval: required only for hosted_builtin_preview_v1; exact current authenticated approval for this animationRequestId

작업:
1. scalar animationRequestId 일치와 모든 animation readiness blocker를 검증한다.
2. character animation이 animation-ready minimal profile을 상속하면 capability 접근 전에 exact payload/hash, evidence, prompt를 검사한다. 4.25 heads 초과/7-8등신·영웅적 장신 허용, dense realistic detail 허용, nonminimal color/value 허용은 각각 character_generation_proportion_gate_failed, character_generation_detail_density_gate_failed, character_generation_color_value_gate_failed로 차단하고 providerCalled=false/submitCount=0/cost=0을 반환한다. prompt를 수정하지 않는다.
   sparse-ink profile이면 same exact payload/hash와 approved finalFrameCount의 각 frame별 omission 35-50%, 3-6 accents, exact palette, line/pigment motion cues, identity/action anchor stability를 확인한다. omission 위반은 character_generation_sparse_omission_budget_gate_failed, accent 범위·filled/off-palette 위반은 character_generation_sparse_pigment_budget_gate_failed, closed contour, static repetition·motion cue 누락, anchor drift는 각각 중앙 8.4의 contour/motion/identity token으로 차단한다.
3. promotable_generation_v2이면 기존 contract 6.1-6.2를 변경 없이 수행한다. hosted_builtin_preview_v1이면 contract 6.1.1의 exact current animationRequestId approval, settings seal, prompt/reference drift, submitCount=1/retry=0을 검증하고 unavailable descriptor/settings/cost evidence를 그대로 기록한다.
4. submit 직전 exact animationRequestId와 모든 hash/reference role을 재검증한다. preview는 built-in_imagegen으로 한 번만 제출하여 preview 전용 output/hash와 hosted preview record만 작성하고 preservation handoff를 만들지 않는다.
5. promotable mode만 animationRequestId-bearing idempotencyKey, generated_media_generation_v2, costEvidence와 animation_gif_frame_set_v2 preservation handoff를 사용한다.
6. oversampling, 복수 요청 병합, 임의 동작 추가, GIF 저장, fixed-cell extraction, transparency/outline/chroma 처리와 frame 보정을 수행하지 않는다.
7. 평가, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status / executionMode / animationRequestId / generationRecordId 또는 previewRecordId / submitCount / refs / costKnown
- provider=imagegen / structureProfile=animation_gif_frame_set_v2
- nextStep: preservation_packaging | preview_complete_no_downstream

실패 시 Output:
- status: blocked | failed
- failureType: contract 8.1/8.4의 기존 generation token 또는 contract 6.1.1의 exact hosted-preview token 하나
- providerCalled / submitCount / costKnown / applicable evidence status / requiredDecision / safeToRetry

검증:
- 정확히 한 animationRequestId와 최종 frame count를 사용해야 한다.
- generation record에 extraction/package/evaluation 결과가 없어야 한다.
- preview record는 exact one animationRequestId, preview_only/not_promotable/not_evaluated이며 preservation/evaluation/promotion 입력이 없어야 한다.
```
