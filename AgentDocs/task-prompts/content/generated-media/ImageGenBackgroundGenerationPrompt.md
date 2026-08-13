# ImageGen Background Single-Image Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 배경 record 하나를 검증하고 저장된 prompt를 ImageGen에 변경 없이 제출해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenBackgroundPipelineGuide.md

Input:
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- promptRecordId: {generated_media_prompt_v3_id}
- providerExecutionApproval: exact generated_media_provider_execution_approval_v1 from the execution-role-presented scope hash and envelope

작업:
1. provider/route/profile/snapshot과 background scene/composition/viewpoint/depth/playable-area/canvas/target/safe-area/consistency-lock/anchor readiness 및 hash를 검증한다.
2. 실행 역할이 closed scope payload/hash를 직접 계산해 사용자에게 한도와 함께 제시한 뒤 받은 closed approval을 재검증한다. tagged cost와 누적 logical attempts/projection은 contract 6.1-6.2를 그대로 따른다.
3. deterministic idempotencyKey로 동일 완료 결과를 무과금 재사용하고 active 동일 호출을 차단한 뒤 exact prompt/settings만 제출한다.
4. attempts, provider refs와 호출·비호출 costEvidence를 generated_media_generation_v2에 기록하고 background_single_image_v2 preservation handoff를 반환한다.
5. download, packaging, evaluation, promotion과 Git을 수행하지 않는다.

Output:
- status / generation record / attempts / refs / costEvidence / idempotencyKey
- provider=imagegen / structureProfile=background_single_image_v2 / nextStep=preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: planning_snapshot_mismatch | ambiguous_image_role | missing_background_scene_contract | missing_background_composition | missing_background_viewpoint | missing_background_horizon | missing_background_depth_layer_contract | missing_background_playable_area | missing_background_subject_contract | missing_background_canvas_contract | missing_background_aspect_ratio | missing_background_target_display | missing_background_safe_area | missing_background_consistency_lock | missing_background_policy | missing_anchor_contract | unsupported_background_domain | unsupported_provider | prompt_record_missing | prompt_record_stale | missing_provider_execution_approval | invalid_provider_execution_approval | provider_execution_scope_mismatch | provider_cost_unit_mismatch | provider_cost_estimate_unavailable | provider_cost_limit_exceeded | provider_actual_cost_unavailable | retry_limit_exceeded | duplicate_provider_call_risk | provider_operation_failed | record_collision
- providerCalled / costEvidence / requiredDecision / safeToRetry

검증:
- icon/character/animation 계약이나 generation 이후 결과를 기록하지 않아야 한다.
- approval, cost, attempt, idempotency와 중복 과금 방지 증거가 완전해야 한다.
```
