# ImageGen Background Single-Image Prompt Authoring Prompt

## Prompt

```text
current v2 배경 단일 이미지 routing record와 planning handoff를 검증하고 ImageGen prompt record 하나만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenBackgroundPipelineGuide.md

Input:
- routingRecordFile: {generated_media_routing_v2_path}
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- required: immutable identity/snapshot, requiredElements, prohibitedElements, backgroundProfile, backgroundSpecification

작업:
1. provider=imagegen, assetType=background_single_image, domainType=stage|battle|environment와 exact background profile row 한 건을 검증한다.
2. sceneContract, composition, viewpoint, horizon, ordered depthLayers, playable/readability area, subject inclusions/exclusions, canvas/aspect, target display, safe area, final background policy, consistency lock와 scene_composition_anchor를 검증한다.
3. 승인된 planning evidence만 closed background `generated_media_visual_brief_v2`로 변환한다. scene, era, culture, weather, lighting, landmark, camera 또는 subject를 보충하지 않고 character identity/expression/reference member를 만들지 않는다. planning에 `style_contract_only` 또는 `none`이 고정되어 있으면 외부 image reference 없이 그대로 투영한다.
4. ordered requiredElements 뒤에 ordered prohibitedElements를 `Do not depict or include: {value}` 형식으로 한 번씩 붙이는 exact LF projection으로 `scenePromptOriginal`을 만든다. `imagegen_background_single_image_prompt_v2` provider payload, `{canvas,generationBackground:{mode:opaque},outputFormat:png}` settings intent와 각 hash를 계산한다.
5. GeneratedMediaRecordGuide의 background discriminated branch에 따라 `gmprompt3.background_single_image` immutable record/Markdown/closed index를 canonical v2 path에 no-clobber/CAS로 작성하고 detached generation handoff를 반환한다. 기존 byte-identical artifact만 idempotent reuse한다.
6. `test_generated_media_background_prompt_v3_contract.mjs`와 기존 character prompt-v3 fixed vector를 검증한다.
7. provider 호출, download, packaging, evaluation, promotion과 Git을 수행하지 않는다.

Output:
- status / prompt record, Markdown, index paths and hashes / detached generation handoff hash / applied profile
- provider=imagegen / structureProfile=background_single_image_v2 / nextStep=generation

실패 시 Output:
- status: blocked
- failureType: planning_snapshot_mismatch | missing_required_elements | missing_prohibited_elements | ambiguous_image_role | missing_background_scene_contract | missing_background_composition | missing_background_viewpoint | missing_background_horizon | missing_background_depth_layer_contract | missing_background_playable_area | missing_background_subject_contract | missing_background_canvas_contract | missing_background_aspect_ratio | missing_background_target_display | missing_background_safe_area | missing_background_consistency_lock | missing_background_policy | missing_anchor_contract | unsupported_background_domain | unsupported_current_route | unsupported_record_schema | provider_value_invalid | record_identity_mismatch | record_collision | prompt_markdown_mismatch | index_entry_invalid | prompt_record_write_failed | prompt_markdown_write_failed | prompt_index_write_failed | prompt_publish_rollback_failed
- missingFields / conflictingFields / requiredDecision / safeToRetry

검증:
- icon, character 또는 animation 계약을 사용하지 않아야 한다.
- exact background profile 한 행과 planning evidence만 사용해야 한다.
- RFC 8785 JCS, JSON/Markdown LF, payload/record/settings/index/handoff hash와 gmprompt3 ID/path가 fixed-vector helper와 일치해야 한다.
- prompt authoring 이후 단계를 실행하지 않아야 한다.
```
