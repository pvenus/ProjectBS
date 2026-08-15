# ImageGen Character Single-Image Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 캐릭터 단일 이미지 record 하나를 검증하고 저장된 prompt를 ImageGen에 변경 없이 제출해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md

Input:
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- generationHandoff: {exact_generated_media_generation_handoff_v2_from_authoring}
- providerExecutionApproval: exact generated_media_provider_execution_approval_v1 from the execution-role-presented scope hash and envelope

작업:
1. closed generationHandoff의 promptRecordId와 JSON/Markdown/index path/hash를 exact
   bytes에서 다시 계산하고 closed index entry, prompt payload projection, provider=imagegen,
   assetType=character_single_image, snapshot, identity lock,
   single-image/background/outline/anchor readiness를 검증한다. CRLF/LF를 정규화하거나
   handoff의 caller summary를 신뢰하지 않는다.
2. 실행 역할이 configured_imagegen_capability의 closed non-submit preflight에서 immutable descriptor/evidence, defaults-resolved exact settings, tagged estimate를 받아 각 hash를 재계산한다. descriptor/settings를 scope에 bind해 closed scope payload/hash를 직접 계산하고 사용자에게 한도와 함께 제시한 뒤 받은 closed approval을 재검증한다. submit 직전 preflight drift 검사, actual cost, 누적 logical attempts와 projection은 contract 6.1-6.2를 그대로 따른다.
3. deterministic idempotencyKey로 완료 결과는 재사용하고 active 동일 호출은 차단한 뒤 exact prompt/settings를 한 번의 논리 실행 단위로 제출한다.
4. attempts/result refs와 호출·비호출 costEvidence를 generated_media_generation_v2에 기록하고 character_single_image_v2 preservation handoff를 반환한다.
5. PixelLab fallback, download, 변환, packaging, evaluation, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status / generationRecordId / attempts / result refs / costEvidence / idempotencyKey
- provider=imagegen / structureProfile=character_single_image_v2
- nextStep: preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: planning_snapshot_mismatch | missing_identity_consistency_lock | missing_required_elements | missing_prohibited_elements | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_target_display_size | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | unsupported_provider | prompt_record_missing | prompt_record_stale | missing_provider_execution_approval | invalid_provider_execution_approval | provider_execution_scope_mismatch | provider_capability_descriptor_unavailable | provider_capability_preflight_invalid | provider_capability_drift | provider_cost_unit_mismatch | provider_cost_estimate_unavailable | provider_cost_limit_exceeded | provider_actual_cost_unavailable | retry_limit_exceeded | duplicate_provider_call_risk | provider_operation_failed | record_collision
- providerCalled / costEvidence / requiredDecision / safeToRetry

검증:
- 8-way, rotation result, download/package/evaluation 결과를 만들지 않아야 한다.
- generation record에는 provider provenance와 preservation handoff만 있어야 한다.
```
