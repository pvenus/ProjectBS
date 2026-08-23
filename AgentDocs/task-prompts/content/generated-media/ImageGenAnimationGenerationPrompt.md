# ImageGen Single Animation Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 animation record 하나를 검증하고 정확히 한 animationRequestId의 animation generation을 실행해줘. provider-native mode이면 playable animated GIF 하나를 provider 결과로 요구하고, accepted coherent-master attack guidance mode이면 provider는 coherent six-cell master image 하나만 반환하며 같은 generation role이 completed GIF를 만든다.

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
- acceptedResultAttackGifGuidance: optional closed accepted_result_attack_gif_guidance_v1 for future same character attack-animation role guidance only

작업:
1. scalar animationRequestId 일치와 모든 animation readiness blocker를 검증한다.
2. character animation이 animation-ready minimal profile을 상속하면 capability 접근 전에 exact payload/hash, evidence, prompt를 검사한다. 4.25 heads 초과/7-8등신·영웅적 장신 허용, dense realistic detail 허용, nonminimal color/value 허용은 각각 character_generation_proportion_gate_failed, character_generation_detail_density_gate_failed, character_generation_color_value_gate_failed로 차단하고 providerCalled=false/submitCount=0/cost=0을 반환한다. prompt를 수정하지 않는다.
   sparse-ink profile이면 same exact payload/hash와 approved finalFrameCount의 각 frame별 omission 35-50%, 3-6 accents, exact palette, line/pigment motion cues, identity/action anchor stability를 확인한다. omission 위반은 character_generation_sparse_omission_budget_gate_failed, accent 범위·filled/off-palette 위반은 character_generation_sparse_pigment_budget_gate_failed, closed contour, static repetition·motion cue 누락, anchor drift는 각각 중앙 8.4의 contour/motion/identity token으로 차단한다.
   motion-flow successor이면 capability access 전에 exact bold v2 base/hash와 18/8, 64/56/5, color/halo projection을 다시 확인한다. faded-indigo sword/torso 3-5 directional flow, gray-brown shoulder/hem inertia, bounded dark-neutral trajectory가 없거나 static repetition/generic clean-vector sheet/arbitrary speed lines/magic VFX가 허용되면 `character_generation_bold_outline_motion_flow_gate_failed`; ordered frame continuity가 닫히지 않으면 `character_generation_bold_outline_motion_continuity_gate_failed`; identity/equipment anchor가 drift하면 `character_generation_bold_outline_motion_identity_equipment_gate_failed`로 providerCalled=false, submitCount=0에서 차단한다.
3. animationSourceMode를 확인한다. `provider_native_animated_gif`이면 extractionMode=gif_timeline_exact이고 configured_animated_gif_capability의 zero-submit attestation에서 playable GIF MIME/output, exact canvas, finalFrameCount, ordered timing, loop, full-canvas disposal, reference-role support를 모두 확인한다. 하나라도 없으면 animated_provider_capability_unavailable로 providerCalled=false/submitCount=0에서 즉시 종료한다.
4. `generation_role_coherent_master_to_gif`이면 extractionMode=generation_role_reopened_gif_timeline_exact이고 provider callable은 one coherent six-cell master image output, exact canvas, reference-role support, no independent frame assembly를 attestation해야 한다. provider가 GIF를 반환한다고 요구하지 않는다. still image 하나로 의미를 축약하거나 contact sheet/sprite sheet/collage/video/독립 frame 생성으로 대체하지 않는다.
5. promotable_generation_v2이면 기존 contract 6.1-6.2와 해당 source mode descriptor를 함께 scope에 포함한다. hosted_builtin_preview_v1은 선택된 source mode를 attested한 callable에서만 허용한다. still-image hosted preview는 이 animation 요청에 사용할 수 없다.
6. submit 직전 exact animationRequestId, 모든 hash/reference role, source-mode capability descriptor/settings hash를 재검증한다. 정확히 한 번 제출하고 retry=0으로 실행한다. provider-native mode의 반환값이 playable animated GIF가 아니거나 coherent-master mode의 반환값이 coherent six-cell master image가 아니면 source mismatch로 종료한다.
7. provider-native mode는 provider 원본 GIF를 변환하지 않는다. coherent-master mode는 같은 공식 generation role이 provider master image에서 completed GIF를 먼저 만들고, 그 GIF를 닫았다 다시 열어 reopened timeline에서 six PNG frames를 추출한다. 이 mode에서 final packaging은 deterministic pelvis/baseline translation과 verified neighboring-cell edge fragment removal만 허용한다.
8. oversampling, 복수 요청 병합, 임의 동작 추가, 독립 frame 생성/선택/보간, transparency/outline/chroma 임의 처리와 임의 frame 보정을 수행하지 않는다.
9. 평가, promotion, Slack, Unity, Git을 수행하지 않는다.
10. acceptedResultAttackGifGuidance가 있으면 immutable planning/routing/prompt/record를 바꾸지 않고 generation-role-owned coherent-master-to-GIF projection으로만 사용한다. provider prompt에는 one coherent six-cell master image, fixed pelvis center/ground baseline, longest clean left/right margin을 모든 frame의 shared width basis로 사용, neighboring-cell edge fragment 제외, identical scale/timing/global palette/fully opaque background, no clipping을 직접 요구한다. generation role이 만든 completed GIF를 닫았다 다시 열어 pelvis drift=0px, baseline drift=0px, no clipping, no neighboring fragments와 exact dimensions/frameCount를 검증한다. 실패하면 기존 anchor_mapping_mismatch, scale_lock_violation 또는 gif_timeline_contract_mismatch 중 정확한 token으로 차단하고 임의 보정·재시도하지 않는다.
11. 성공 시 closed `generated_media_attack_gif_final_validation_receipt_v1` 한 건만 preservation handoff에 compact projection한다. accepted artifact SHA 8a924f...는 evidence only이며 provider return, identity reference, preservation source, media record authority로 승격하지 않는다.

Output:
- status / executionMode / animationRequestId / generationRecordId 또는 previewRecordId / submitCount / refs / costKnown
- animationSourceMode=provider_native_animated_gif | generation_role_coherent_master_to_gif
- extractionMode=gif_timeline_exact | generation_role_reopened_gif_timeline_exact
- provider / providerInterface / originalAnimatedGifRef 또는 providerMasterImageRef / completedGifRef / structureProfile=animation_gif_frame_set_v2
- nextStep: preservation_packaging | preview_complete_no_downstream
- accepted-result attack guidance 사용 시 generated_media_attack_gif_final_validation_receipt_v1

실패 시 Output:
- status: blocked | failed
- failureType: animated_provider_capability_unavailable | provider_animated_gif_source_mismatch | contract 8.1/8.4의 기존 generation token 또는 contract 6.1.1의 exact hosted-preview token 하나
- providerCalled / submitCount / costKnown / applicable evidence status / requiredDecision / safeToRetry

검증:
- 정확히 한 animationRequestId와 최종 frame count를 사용해야 한다.
- provider-native result는 최종 frame count/order/timing/loop를 가진 playable animated GIF 하나여야 한다.
- coherent-master result는 provider GIF가 아니라 coherent six-cell master image이며, generation role이 completed GIF를 먼저 만든 뒤 reopened timeline에서 frames를 추출해야 한다.
- accepted-result attack guidance receipt는 pelvisDriftMaxPx=0, baselineDriftMaxPx=0, scaleUniform/timingUniform/globalPaletteUniform/backgroundFullyOpaque=true, clippingDetected/neighboringFragmentsDetected=false여야 한다.
- generation record에 extraction/package/evaluation 결과가 없어야 한다.
- preview record는 exact one animationRequestId, preview_only/not_promotable/not_evaluated이며 preservation/evaluation/promotion 입력이 없어야 한다.
```
