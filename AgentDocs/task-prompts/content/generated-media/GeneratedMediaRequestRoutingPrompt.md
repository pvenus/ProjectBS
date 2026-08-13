# Generated Media Request Routing Prompt

## Prompt

```text
현재 ProjectBS 저장소에서 generated_media_planning_handoff_v2 하나를 검증하고 current ImageGen authoring role로 route해줘. provider prompt와 media를 생성하지 마.

참조 가이드:
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v2_path}
- supersedesRoutingRecordId: {optional_v2_record_id}

작업:
1. request/content/source/snapshot identity와 모든 source hash를 검증한다.
2. requiredElements/prohibitedElements와 assetType별 specification을 검증하고 누락값을 추정하지 않는다.
3. assetType/domainType/profile을 canonical lowercase enum으로 검증한다. 현재 assetType은 character_single_image, icon_single_image, background_single_image, animation뿐이다.
4. generated_media_authoring_profile_registry_v2에서 exact row를 찾는다. 1행만 성공하고 0행/2행 이상은 차단한다. 모든 current row의 provider가 imagegen인지 확인한다.
5. character는 character single-image 계약, icon은 skill/item icon 전용 visual-center/safe-area/small-size 계약, background는 stage/battle/environment scene/composition/viewpoint/depth/playable-area/canvas/target/safe-area/consistency-lock/scene-anchor 계약을 검증한다. icon/background 양쪽 증거가 있으면 추정하지 않고 차단한다.
6. animation은 planning handoff의 animationRequests를 source order로 읽어 animationRequestId별 독립 unit으로 분리한다. 각 normalizedRequest/routing record에는 선택된 animationRequest 객체 하나와 scalar ID 한 건만 포함한다. reference/hash, final frame count/timing/order/loop/key poses, fixed cell, scale lock, vertical motion, background/noShadow/outline/anchor/masterFirst를 그대로 보존하며 병합하거나 동작을 추가하지 않는다.
7. exact row의 selectedPipeline/selectedAuthoringPrompt와 field-level handoff를 기록한다.
8. generated_media_routing_v2를 v2 canonical path에 결정적 ID로 기록한다. animation path/ID에는 animationRequestId가 포함된다. 동일 payload는 재사용하고 다른 bytes는 collision이다.
9. v1 registry/PixelLab row를 평가하거나 v1 record/index를 수정하지 않는다.
10. authoring/provider/download/packaging/evaluation/promotion/Slack/Unity/Git을 수행하지 않는다.

Output:
- status: routed
- routingRecordId / path / hash
- registryVersion / selectedRegistryRowId
- selectedPipeline / selectedAuthoringPrompt
- assetType / domainType / contentId / optional animationRequestId
- normalizedRequest / planningSnapshotHash / routingReason
- nextStep: authoring

실패 시 Output:
- status: blocked
- failureType: missing_planning_handoff | planning_snapshot_mismatch | missing_identity_consistency_lock | missing_required_elements | missing_prohibited_elements | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_target_display_size | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | ambiguous_image_role | missing_background_scene_contract | missing_background_composition | missing_background_viewpoint | missing_background_horizon | missing_background_depth_layer_contract | missing_background_playable_area | missing_background_subject_contract | missing_background_canvas_contract | missing_background_aspect_ratio | missing_background_target_display | missing_background_safe_area | missing_background_consistency_lock | unsupported_icon_domain | unsupported_background_domain | missing_animation_request_id | duplicate_animation_request_id | missing_reference_image | reference_image_hash_mismatch | missing_final_frame_count | missing_animation_timing | missing_frame_order | missing_loop_contract | missing_key_poses | missing_fixed_cell_contract | missing_scale_lock | missing_vertical_motion_policy | missing_master_first_contract | unsupported_current_route | conflicting_routing_evidence | routing_record_collision | routing_record_write_failed | routing_index_write_failed
- missingFields / conflictingFields / candidatePipelines / requiredDecision / safeToRetry

검증:
- current route에 PixelLab이 없어야 한다.
- current registry의 execution role은 character/icon/background/animation 네 종류여야 한다.
- icon/background ambiguity는 fail-closed여야 한다.
- character route에 8-way/ordered_rotation_set이 없어야 한다.
- animation routing record 하나당 animationRequestId가 정확히 한 건이어야 한다.
- blocked 요청은 record/index를 변경하지 않아야 한다.
- authoring 이후 단계를 실행하지 않아야 한다.
```
