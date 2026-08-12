# ImageGen Single Animation Prompt Authoring Prompt

## Prompt

```text
current v2 animation routing record 하나를 검증하고 정확히 한 animationRequestId의 ImageGen prompt record만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenAnimationPipelineGuide.md

Input:
- routingRecordFile: {generated_media_routing_v2_animation_request_path}
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- animationRequestId: {exact_single_id}

작업:
1. routing record의 normalized animationRequest가 객체 하나이고 ID가 입력과 동일한지 확인한다. planning handoff의 같은 ID 원본 항목과 exact 비교한다. normalized 배열/복수/병합은 차단한다.
2. reference identity/path/hash, final frame count/timing/order/loop/key poses, fixed cell, scale lock, vertical motion, background/noShadow/outline, anchor, masterFirst를 검증한다.
3. 최종 승인 frame count의 coherent master 하나를 만드는 visual brief와 ImageGen prompt를 작성한다. oversampling/선택, 프레임 crop/scale/recenter를 지시하지 않는다.
4. generated_media_prompt_v3를 animationRequestId 포함 v2 path에 기록한다.
5. provider 및 packaging/evaluation을 실행하지 않는다.

Output:
- status / animationRequestId / prompt record paths and hashes
- provider=imagegen / structureProfile=animation_gif_frame_set_v2 / nextStep=generation

실패 시 Output:
- status: blocked
- failureType: planning_snapshot_mismatch | missing_required_elements | missing_prohibited_elements | missing_identity_consistency_lock | missing_animation_request_id | multiple_animation_requests_not_allowed | missing_reference_image | reference_image_hash_mismatch | missing_final_frame_count | missing_animation_timing | missing_frame_order | missing_loop_contract | missing_key_poses | missing_fixed_cell_contract | missing_scale_lock | missing_vertical_motion_policy | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | missing_master_first_contract | oversampling_not_allowed | unsupported_current_route | record_collision
- missingFields / requiredDecision / safeToRetry

검증:
- 정확히 animationRequestId 한 건만 포함해야 한다.
- frame count는 처음부터 최종 개수여야 한다.
- generation과 extraction을 실행하지 않아야 한다.
```
