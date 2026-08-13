# ImageGen Icon Single-Image Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 아이콘 record 하나를 검증하고 저장된 prompt를 ImageGen에 변경 없이 제출해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenIconPipelineGuide.md

Input:
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- promptRecordId: {generated_media_prompt_v3_id}
- providerExecutionApproval: exact generated_media_provider_execution_approval_v1 from the execution-role-presented scope hash and envelope

작업:
1. provider, route, profile, snapshot, identity lock, icon-only single-image/background/outline/visual-center/small-size readiness와 hash를 검증하고 scene/background role 입력을 거부한다.
2. 실행 역할이 closed scope payload/hash를 직접 계산해 사용자에게 한도와 함께 제시한 뒤 받은 closed approval을 재검증한다. tagged cost와 누적 logical attempts/projection은 contract 6.1-6.2를 그대로 따른다.
3. deterministic idempotencyKey로 완료 결과를 재사용하고 active 동일 호출을 차단한 뒤 exact prompt/settings를 제출한다.
4. attempts/result refs와 호출·비호출 costEvidence를 generated_media_generation_v2에 기록하고 icon_single_image_v2 preservation handoff를 반환한다.
5. download, packaging, evaluation, promotion, Git을 수행하지 않는다.

Output:
- status / generation record / attempts / refs / costEvidence / idempotencyKey
- provider=imagegen / structureProfile=icon_single_image_v2 / nextStep=preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: planning_snapshot_mismatch | missing_identity_consistency_lock | missing_required_elements | missing_prohibited_elements | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_target_display_size | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | unsupported_icon_domain | ambiguous_image_role | unsupported_provider | prompt_record_missing | prompt_record_stale | missing_provider_execution_approval | invalid_provider_execution_approval | provider_execution_scope_mismatch | provider_cost_unit_mismatch | provider_cost_estimate_unavailable | provider_cost_limit_exceeded | provider_actual_cost_unavailable | retry_limit_exceeded | duplicate_provider_call_risk | provider_operation_failed | record_collision
- providerCalled / costEvidence / requiredDecision / safeToRetry

검증:
- generation 밖의 결과를 기록하지 않아야 한다.
```
