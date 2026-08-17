# ImageGen Single Animation Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 animation record 하나를 검증하고 정확히 한 animationRequestId의 provider-native playable animated GIF를 생성해줘.

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
   motion-flow successor이면 capability access 전에 exact bold v2 base/hash와 18/8, 64/56/5, color/halo projection을 다시 확인한다. faded-indigo sword/torso 3-5 directional flow, gray-brown shoulder/hem inertia, bounded dark-neutral trajectory가 없거나 static repetition/generic clean-vector sheet/arbitrary speed lines/magic VFX가 허용되면 `character_generation_bold_outline_motion_flow_gate_failed`; ordered frame continuity가 닫히지 않으면 `character_generation_bold_outline_motion_continuity_gate_failed`; identity/equipment anchor가 drift하면 `character_generation_bold_outline_motion_identity_equipment_gate_failed`로 providerCalled=false, submitCount=0에서 차단한다.
3. animationSourceMode=provider_native_animated_gif와 extractionMode=gif_timeline_exact를 확인한다. configured_animated_gif_capability의 zero-submit attestation에서 playable GIF MIME/output, exact canvas, finalFrameCount, ordered timing, loop, full-canvas disposal, reference-role support를 모두 확인한다. 하나라도 없으면 animated_provider_capability_unavailable로 providerCalled=false/submitCount=0에서 즉시 종료한다. prompt prose, still ImageGen, contact sheet, sprite sheet, video, 독립 frame 생성으로 대체하지 않는다.
4. promotable_generation_v2이면 기존 contract 6.1-6.2와 animated-GIF capability descriptor를 함께 scope에 포함한다. hosted_builtin_preview_v1은 provider-native animated GIF를 반환한다고 attested된 callable에서만 허용한다. still-image hosted preview는 이 animation 요청에 사용할 수 없다.
5. submit 직전 exact animationRequestId, 모든 hash/reference role, animated capability descriptor/settings hash를 재검증한다. 정확히 한 번 제출하고 retry=0으로 실행한다. 반환값이 playable animated GIF가 아니거나 frame count/order/timing/loop가 다르면 source mismatch로 종료하며 정지 이미지나 합성 GIF를 만들지 않는다.
6. promotable mode만 animationRequestId-bearing idempotencyKey, generated_media_generation_v2, costEvidence, exact original animated-GIF provider ref/hash와 animation_gif_frame_set_v2 preservation handoff를 사용한다. Generation은 provider 원본 GIF를 변환하지 않는다.
7. oversampling, 복수 요청 병합, 임의 동작 추가, fixed-cell split, frame 생성/선택/보간, transparency/outline/chroma 처리와 frame 보정을 수행하지 않는다.
8. 평가, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status / executionMode / animationRequestId / generationRecordId 또는 previewRecordId / submitCount / refs / costKnown
- animationSourceMode=provider_native_animated_gif / extractionMode=gif_timeline_exact
- provider / providerInterface=configured_animated_gif_capability / originalAnimatedGifRef / structureProfile=animation_gif_frame_set_v2
- nextStep: preservation_packaging | preview_complete_no_downstream

실패 시 Output:
- status: blocked | failed
- failureType: animated_provider_capability_unavailable | provider_animated_gif_source_mismatch | contract 8.1/8.4의 기존 generation token 또는 contract 6.1.1의 exact hosted-preview token 하나
- providerCalled / submitCount / costKnown / applicable evidence status / requiredDecision / safeToRetry

검증:
- 정확히 한 animationRequestId와 최종 frame count를 사용해야 한다.
- provider result는 최종 frame count/order/timing/loop를 가진 playable animated GIF 하나여야 한다.
- generation record에 extraction/package/evaluation 결과가 없어야 한다.
- preview record는 exact one animationRequestId, preview_only/not_promotable/not_evaluated이며 preservation/evaluation/promotion 입력이 없어야 한다.
```
