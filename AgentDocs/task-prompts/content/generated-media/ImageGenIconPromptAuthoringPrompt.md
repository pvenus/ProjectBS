# ImageGen Icon Single-Image Prompt Authoring Prompt

## Prompt

```text
current v2 아이콘 단일 이미지 routing record와 planning handoff를 검증하고 ImageGen prompt record 하나만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenIconPipelineGuide.md

Input:
- routingRecordFile: {generated_media_routing_v2_path}
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- required: identity/snapshot, requiredElements, prohibitedElements, identityConsistencyLock, iconProfile, singleImageSpecification

작업:
1. provider=imagegen, assetType=icon_single_image와 exact skill/item profile row를 검증한다.
2. viewpoint, pose, framing, canvas, display size, safe area, background/noShadow, outline, visual-center anchor를 모두 검증한다.
3. stage/battle/environment/background/scene 입력, horizon/depth/playable-area와 scene anchor가 없음을 확인한다. 있거나 icon/background 양쪽으로 해석되면 차단한다.
4. 승인된 의미만 visual brief와 cohesive ImageGen prompt로 변환한다. 상징·효과·색을 보충하지 않는다.
5. generated_media_prompt_v3 immutable record/Markdown/index를 v2 path에 작성한다.
6. provider 및 후속 단계를 실행하지 않는다.

Output:
- status / prompt record paths and hashes / applied profile
- provider=imagegen / structureProfile=icon_single_image_v2 / nextStep=generation

실패 시 Output:
- status: blocked
- failureType: planning_snapshot_mismatch | missing_identity_consistency_lock | missing_required_elements | missing_prohibited_elements | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_target_display_size | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | unsupported_icon_domain | ambiguous_image_role | unsupported_current_route | record_collision
- missingFields / requiredDecision / safeToRetry

검증:
- skill/item 실행 prompt를 복제하지 않아야 한다.
- scene/background 책임과 입력이 없어야 한다.
- provider=imagegen과 exact profile 한 행만 사용해야 한다.
- ImageGen 및 후속 단계를 실행하지 않아야 한다.
```
