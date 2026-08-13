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
3. 승인된 planning evidence만 provider-neutral visual brief와 cohesive scene prompt로 변환한다. scene, era, culture, weather, lighting, landmark, camera 또는 subject를 보충하지 않는다.
4. generated_media_prompt_v3 immutable record/Markdown/index를 v2 canonical path에 작성한다.
5. provider 호출, download, packaging, evaluation, promotion과 Git을 수행하지 않는다.

Output:
- status / prompt record paths and hashes / applied profile
- provider=imagegen / structureProfile=background_single_image_v2 / nextStep=generation

실패 시 Output:
- status: blocked
- failureType: planning_snapshot_mismatch | missing_required_elements | missing_prohibited_elements | ambiguous_image_role | missing_background_scene_contract | missing_background_composition | missing_background_viewpoint | missing_background_horizon | missing_background_depth_layer_contract | missing_background_playable_area | missing_background_subject_contract | missing_background_canvas_contract | missing_background_aspect_ratio | missing_background_target_display | missing_background_safe_area | missing_background_consistency_lock | missing_background_policy | missing_anchor_contract | unsupported_background_domain | unsupported_current_route | record_collision
- missingFields / conflictingFields / requiredDecision / safeToRetry

검증:
- icon, character 또는 animation 계약을 사용하지 않아야 한다.
- exact background profile 한 행과 planning evidence만 사용해야 한다.
- prompt authoring 이후 단계를 실행하지 않아야 한다.
```
