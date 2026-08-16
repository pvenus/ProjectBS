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
6a. character attack animation의 reference prompt가 exact bold-outline v2이면 direct inheritance는 `character_style_profile_conflict`이다. motion-flow successor는 reference bytes/key/hash, exact 18/8 및 64/56/5, unchanged color anchors/closed halo와 여덟 approved motion bindings가 모두 있을 때만 선택한다. attack이 아니면 `bold_outline_motion_flow_not_attack`, binding 누락은 `missing_bold_outline_motion_flow_planning_bindings`, reference/projection 불일치는 `bold_outline_motion_successor_reference_mismatch`로 no-route 차단한다.
7. exact row의 selectedPipeline/selectedAuthoringPrompt와 field-level handoff를 기록한다.
8. GeneratedMediaRecordGuide.md의 closed routingHashPayload를 정확히 투영하고 JCS canonical JSON의 전체 SHA-256을 계산한다. ID는 비 animation `gmroute2.{assetType}.{contentId}.{hash[0:20]}`, animation `gmroute2.animation.{contentId}.{animationRequestId}.{hash[0:20]}`로 만들며 20자는 정확히 lowercase hex 20자다.
9. canonical v2 record path와 같은 directory의 `routing_index.json`을 사용한다. index는 `generated_media_routing_index_v2` closed schema이고 `entries`는 routingRecordId를 key로 하며 value는 record identity/path/full payload hash/exact file hash의 정확한 projection이어야 한다.
10. 쓰기 전에 existing record/index 전체를 검증한다. exact pair는 bytes를 바꾸지 않고 재사용하고, valid record만 남은 partial success는 record를 재작성하지 않고 index entry만 복구한다. dangling/divergent index는 `routing_index_write_failed`, divergent record bytes나 hash-prefix collision은 `routing_record_collision`로 아무것도 덮어쓰지 않는다.
11. 새 record를 same-directory atomic no-clobber로 먼저 기록하고 reread/hash한 뒤 index를 atomic replace한다. record 성공 후 index 실패만 valid orphan record를 보존하고 `safeToRetry=true`로 path/hash를 반환한다. 그 밖의 blocked 요청은 record/index/failure placeholder/downstream handoff를 생성하지 않는다.
12. supersedesRoutingRecordId는 같은 scope의 유효한 v2 record일 때만 새 payload/record/index entry에 포함한다. 기존 record/index는 수정·삭제하지 않는다.
13. v1 registry/PixelLab row를 평가하거나 v1 record/index를 수정하지 않는다.
14. authoring/provider/download/packaging/evaluation/promotion/Slack/Unity/Git을 수행하지 않는다.
15. 성공 응답은 record/index bytes와 분리된 `generated_media_routing_receipt_v1` 한 건만 반환한다. record에 이미 저장된 normalizedRequest/sourcePlanningFiles/requiredElements/prohibitedElements/typeSpecification/profile locks를 control-plane 메시지에 다시 펼치지 않는다.
16. `generated_media_authority_bundle_receipt_v1`을 routing guide의 closed projection/JCS/hash/ID 규칙으로 계산한다. authoritative main SHA, requested stage scope, exact immutable artifact anchors, contract authority anchors, profile authority anchors 중 하나라도 바뀌거나 receipt가 없으면 full validation이며, 모두 같을 때만 unchanged validation을 재사용한다.
17. `generated_media_stage_delta_envelope_v1`에는 authority bundle ID/hash, fromStage/toStage, unitIdentity, 이번 stage의 새 artifact path/hash, prior validation receipt refs, publicationState/nextStep, providerState, prior pipeline chain ref만 포함한다. Git blob에서 읽을 수 있는 bulk fields와 nested handoff body를 다시 넣지 않는다.
18. child는 final stage delta envelope 한 건만 parent에 보낸다. parent는 동일 envelope를 다음 역할에 정확히 한 번 relay하고 requester/owner/Git 역할에는 full payload를 broadcast하지 않는다. 다른 observer에는 compact terminal status receipt 한 건만 보낸다.
19. routing guide의 validation receipt reuse matrix를 적용한다. mutation/CAS, authority freshness drift, stage artifact raw hash/projection, provider approval/capability/settings/cost/attempt 경계는 항상 재검증한다. exact unchanged authority/source/profile/schema receipt만 재사용할 수 있다.
20. commentary/status는 `generated_media_compact_status_v1` closed schema로 state change 또는 terminal에서만 한 번 emit한다. 동일 status/provider state/hash 재전송은 금지한다.
21. orchestration lineage는 response-only `generated_media_pipeline_receipt_chain_v1`의 append-only value로 전달한다. mutable orchestration record/index/path를 생성하지 않는다.
22. authority bundle, stage delta, routing receipt, pipeline chain, compact status 중 하나라도 schema/hash/transition/publication/relay 규칙에 어긋나면 success handoff를 emit하지 않고 기존 routing record/index를 수정하지 않는다.

Output:
- schemaVersion: generated_media_routing_receipt_v1
- status: routed
- reuseStatus: created | reused_identical
- validatedAuthorityRevision
- routingRecordId / routingRecordPath / routingPayloadSha256 / routingRecordSha256
- indexPath / indexSha256
- authorityBundleId / authorityBundleSha256
- stageDeltaEnvelopeId / stageDeltaEnvelopeSha256
- pipelineReceiptChainId / pipelineReceiptChainSha256
- authoringHandoffPointer: /authoringHandoff
- publicationState: local_unpublished | authoritative_git_blob
- nextStep: git_publication | authoring
- providerCalled: false
- 응답에 normalizedRequest/sourcePlanningFiles/requiredElements/prohibitedElements/typeSpecification/expressionProfilePayload/style lock 배열을 포함하지 않는다. authoring은 authoritative Git publication 뒤 exact routing record와 `/authoringHandoff`를 읽는다.

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
- 같은 입력 재시도는 기존 record/index의 exact bytes를 보존해야 한다.
- payload field 하나가 바뀌면 full hash와 routingRecordId가 바뀌어야 한다.
- occupied ID의 divergent bytes와 dangling/divergent index는 overwrite 없이 차단해야 한다.
- record가 index보다 먼저 publish되어야 하며 index 실패 후 retry는 orphan record를 재사용해야 한다.
- success control-plane receipt가 closed compact schema이고 persisted record payload를 되풀이하지 않아야 한다.
- same authority anchors/scope는 same bundle/chain identity이고 anchor 하나가 바뀌면 새 identity와 full validation이어야 한다.
- invalid stage/publication pair, forbidden bulk field, duplicate relay/status를 fail-closed로 거부해야 한다.
- pipeline orchestration record/index/path가 생성되지 않아야 한다.
- blocked 요청은 record/index를 변경하지 않아야 한다.
- authoring 이후 단계를 실행하지 않아야 한다.
```
