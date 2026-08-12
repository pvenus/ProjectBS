# ImageGen Character Single-Image Prompt Authoring Prompt

## Prompt

```text
현재 ProjectBS 저장소에서 current v2 캐릭터 단일 이미지 routing record와 planning handoff를 검증하고 ImageGen provider-ready prompt record 하나만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md

Input:
- routingRecordFile: {project_relative_generated_media_routing_v2_record}
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v2}
- required: request/content/source/snapshot identity, requiredElements, prohibitedElements, identityConsistencyLock, singleImageSpecification

작업:
1. registryVersion=v2, provider=imagegen, assetType=character_single_image, exact registry row와 snapshot/hash를 검증한다.
2. viewpoint, pose, framing, canvas, targetDisplaySize, safeArea, background, generationBackground, noShadow, outline, pelvis/root와 ground-contact anchor를 모두 검증한다.
3. 승인 기획을 provider-neutral visual brief로 정규화하고 모든 문장을 evidence/constraint에 연결한다. 누락된 외형·시점·색·배경을 만들지 않는다.
4. 한 승인 시점의 cohesive ImageGen prompt 하나와 settings intent를 작성한다. 8-way, rotation, ordered_rotation_set을 넣지 않는다.
5. generated_media_prompt_v3 immutable record와 Markdown을 v2 record path에 작성하고 index를 갱신한다.
6. ImageGen 호출, download, packaging, evaluation, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status / promptRecordId / paths / hashes
- identity / registry row / visual brief evidence coverage
- provider=imagegen / structureProfile=character_single_image_v2
- nextStep: generation

실패 시 Output:
- status: blocked
- failureType: planning_snapshot_mismatch | missing_identity_consistency_lock | missing_required_elements | missing_prohibited_elements | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_target_display_size | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | unsupported_current_route | record_collision
- missingFields / requiredDecision / safeToRetry

검증:
- provider는 imagegen 하나여야 한다.
- 캐릭터 신규 계약에 8-way/rotation set이 없어야 한다.
- planning, brief, provider prompt, settings가 분리되어야 한다.
- provider 및 후속 단계를 실행하지 않아야 한다.
```
