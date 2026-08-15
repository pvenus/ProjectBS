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
- providerExecutionApproval: exact generated_media_provider_execution_approval_v1 from the execution-role-presented scope hash and envelope

작업:
1. scalar animationRequestId 일치와 모든 animation readiness blocker를 검증한다.
2. 실행 역할이 closed scope payload/hash를 직접 계산해 사용자에게 한도와 함께 제시한 뒤 받은 closed approval을 재검증한다. tagged cost와 누적 logical attempts/projection은 contract 6.1-6.2를 그대로 따른다.
3. animationRequestId를 포함한 deterministic idempotencyKey로 완료 결과를 재사용하고 active 동일 호출을 차단한 뒤 최종 승인 frame count의 exact prompt/settings를 제출한다. oversampling, 복수 요청 병합, 임의 동작 추가를 금지한다.
4. generated_media_generation_v2에 attempts/result refs, 호출·비호출 costEvidence와 animation_gif_frame_set_v2 preservation handoff만 기록한다.
5. GIF 저장, fixed-cell extraction, transparency/outline/chroma 처리, frame 보정은 packaging 단계 소유이므로 수행하지 않는다.
6. 평가, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status / animationRequestId / generation record / attempts / refs / costEvidence / idempotencyKey
- provider=imagegen / structureProfile=animation_gif_frame_set_v2
- nextStep: preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: planning_snapshot_mismatch | missing_required_elements | missing_prohibited_elements | missing_identity_consistency_lock | missing_animation_request_id | multiple_animation_requests_not_allowed | missing_reference_image | reference_image_hash_mismatch | missing_final_frame_count | missing_animation_timing | missing_frame_order | missing_loop_contract | missing_key_poses | missing_fixed_cell_contract | missing_scale_lock | missing_vertical_motion_policy | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | missing_master_first_contract | oversampling_not_allowed | unsupported_provider | prompt_record_missing | prompt_record_stale | missing_provider_execution_approval | invalid_provider_execution_approval | provider_execution_scope_mismatch | provider_capability_descriptor_unavailable | provider_capability_preflight_invalid | provider_capability_drift | provider_cost_unit_mismatch | provider_cost_estimate_unavailable | provider_cost_limit_exceeded | provider_actual_cost_unavailable | retry_limit_exceeded | duplicate_provider_call_risk | provider_operation_failed | record_collision
- providerCalled / costEvidence / requiredDecision / safeToRetry

검증:
- 정확히 한 animationRequestId와 최종 frame count를 사용해야 한다.
- generation record에 extraction/package/evaluation 결과가 없어야 한다.
```
