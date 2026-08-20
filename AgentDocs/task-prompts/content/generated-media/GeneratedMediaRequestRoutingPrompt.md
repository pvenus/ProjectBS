# Generated Media Request Routing Prompt

## Prompt

```text
현재 ProjectBS 저장소에서 기본적으로 generated_media_planning_handoff_v2 하나를 검증하고 current ImageGen authoring role로 route해줘. 단, authenticated user authority가 `executionMode=hosted_builtin_fast_preview_v1`을 명시하면 아래 fast-preview 예외만 수행한다.

참조 가이드:
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v2_path}
- supersedesRoutingRecordId: {optional_v2_record_id}
- executionMode: route_only_v2 | hosted_builtin_fast_preview_v1 (optional; default route_only_v2)
- fastPreviewPointer: {required only for hosted_builtin_fast_preview_v1}
- pipelineAuthorityReceipt: {exact generated_media_pipeline_authority_receipt_v1 from coordinator}
- noninteractiveExecutionPolicy: {exact generated_media_noninteractive_execution_policy_v1}

작업:
0. executionMode가 `route_only_v2`이면 기존 1-22를 그대로 수행한다. `hosted_builtin_fast_preview_v1`이면 기존 routing record/authoring publication 작업 1-22를 실행하지 않고 0a-0h만 수행한다. 다른 값은 차단한다.
0a. fastPreviewPointer가 main SHA, requestId, promptRecordId/SHA, reviewed reference path/SHA, deterministic idempotency key의 closed compact projection인지 확인한다. full planning/routing/authoring/profile/prompt payload를 메시지로 요구하거나 재전송하지 않는다.
0b. submit 전 hard blocker는 같은 idempotency key의 provider/과금 중복 위험, authenticated authority 또는 safety 위반, executable prompt/reference의 완전 부재 세 종류뿐이다. 각각 `fast_preview_duplicate_submit_risk`, `fast_preview_authority_or_safety_violation`, `fast_preview_callable_input_absent`를 사용한다.
0c. authoritative planning/prompt가 이미 있으면 재작성·재게시하지 않는다. schema/doc projection conflict, pre-preview Git publication 부재, full contract suite 미실행, capability/cost attestation 부재, exact canvas/background/outputFormat/structured style_only callable control 부재는 ordered `backlogWarnings`로 남기고 preview를 막지 않는다.
0d. prompt text와 reviewed durable style reference 한 장을 in-memory callable input으로 준비한다. 노출되지 않은 provider option은 prompt prose best effort로 보존하고 `unavailableCallableControls`에 기록한다. provider enforcement/default/cost/capability evidence를 합성하지 않는다.
0e. official generation role child를 정확히 한 번 호출한다. child에는 compact pointer와 sealed prompt/reference만 전달한다. provider submitCountMaximum=1, retryCountMaximum=0이며 active/completed/ambiguous same-key 상태에서는 새 submit을 금지한다.
0f. provider return 후 output path/raw hash/byte length/MIME/dimensions와 exposed provider result ref만 관찰하고 즉시 한 번 시각 확인한다. retry/edit 없이 preview intent 차이를 concise summary와 ordered intentWarnings로 같은 terminal receipt의 `visualEvaluation`에 기록한다. 이는 strict evaluation package가 아니다.
0g. terminal receipt는 `previewOnly=true`, `notPromotable=true`, `notPreserved=true`, `strictEvaluationPerformed=false`이며 preservation/evaluation package/promotion/Unity를 호출하지 않는다. 사용자 채택은 별도 strict workflow 요청이다.
0h. child final 한 건을 받고 parent relay 한 건만 수행한다. observer에 full receipt/payload를 broadcast하지 않는다.
0i. coordinator의 pipelineAuthorityReceipt repo/originMain/fetchedAt/hash를 검증하고 exact originMain commit만 읽는다. read-only child는 fetch하지 않는다. routing record/index mutation은 기존 raw blob, no-clobber, CAS 검사를 그대로 fresh 수행하되 setup fetch/worktree mutation은 coordinator의 repository mutex 밖에서 실행하지 않는다.
0j. exact policy 범위의 read/hash/schema/test와 bounded record/index write에는 interactive approval을 다시 요청하지 않는다. host-required bundle은 coordinator의 한 건만 재사용하며, overwrite/delete/extra submit·retry·cost/elevation/out-of-root/scope expansion은 해당 new-authority token으로 partial write 없이 차단한다.
1. request/content/source/snapshot identity와 모든 source hash를 검증한다.
2. requiredElements/prohibitedElements와 assetType별 specification을 검증하고 누락값을 추정하지 않는다.
3. assetType/domainType/profile을 canonical lowercase enum으로 검증한다. 현재 assetType은 character_single_image, icon_single_image, background_single_image, animation뿐이다.
4. generated_media_authoring_profile_registry_v2에서 exact row를 찾는다. 1행만 성공하고 0행/2행 이상은 차단한다. 모든 current row의 provider가 imagegen인지 확인한다.
5. character는 character single-image 계약, icon은 skill/item icon 전용 visual-center/safe-area/small-size 계약, background는 stage/battle/environment scene/composition/viewpoint/depth/playable-area/canvas/target/safe-area/consistency-lock/scene-anchor 계약을 검증한다. icon/background 양쪽 증거가 있으면 추정하지 않고 차단한다.
6. animation은 planning handoff의 animationRequests를 source order로 읽어 animationRequestId별 독립 unit으로 분리한다. 각 normalizedRequest/routing record에는 선택된 animationRequest 객체 하나와 scalar ID 한 건만 포함한다. reference/hash, final frame count/timing/order/loop/key poses, fixed cell, scale lock, vertical motion, background/noShadow/outline/anchor/masterFirst, animationSourceMode=provider_native_animated_gif, extractionMode=gif_timeline_exact를 그대로 보존하며 병합하거나 동작을 추가하지 않는다. 신규 fixed_cell_only 또는 still/contact-sheet/sprite-sheet/frame-set source는 route하지 않는다.
6a. character attack animation의 reference prompt가 exact bold-outline v2이면 direct inheritance는 `character_style_profile_conflict`이다. motion-flow successor는 reference bytes/key/hash, exact 18/8 및 64/56/5, unchanged color anchors/closed halo와 여덟 approved motion bindings가 모두 있을 때만 선택한다. attack이 아니면 `bold_outline_motion_flow_not_attack`, binding 누락은 `missing_bold_outline_motion_flow_planning_bindings`, reference/projection 불일치는 `bold_outline_motion_successor_reference_mismatch`로 no-route 차단한다.
7. exact row의 selectedPipeline/selectedAuthoringPrompt와 field-level handoff를 기록한다.
7a. character planning handoff에 reviewed `styleReferenceBindings`가 있으면 exact one-element/six-member array를 routing payload/record, normalizedRequest, authoringHandoff의 top-level에 byte-semantically 동일하게 투영한다. typeSpecification/identityConsistencyLock/singleImageSpecification 내부에는 넣지 않는다. planning에 없으면 네 위치 모두 omit한다. 누락·추가·중첩·값/순서 불일치는 `style_reference_binding_projection_mismatch`로 no-write 차단한다.
7b. planning이 `generated_media_transparent_foreground_selection_v1`을 선택했으면 exact `generated_media_true_alpha_foreground@1.0.0` / `2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108` key/hash와 selection을 routing payload/record, normalizedRequest, authoringHandoff의 top-level `transparentForegroundSelection`에 동일 투영하고 typeSpecification 내부에는 넣지 않는다. unselected이면 모두 omit한다. main은 `generationBackground={mode:transparent}`만 허용하고 color/removable-solid 및 opaque/removable/warm-ivory required element를 각각 `true_alpha_branch_conflict` / `transparent_prompt_required_element_conflict`로 no-write 차단한다. main/animation lock 혼합, safe margin 또는 root/baseline/scale drift, 누락/unknown field는 `true_alpha_projection_mismatch`로 no-write 차단한다.
7c. exact open-ink v2 reference prompt를 가진 attack animation은 sparse-motion을 successor로 취급하지 않는다. fresh planning이 `projectbs_character_open_ink_wash_attack_motion@1.0.0` / `07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c`를 선택하고 exact base key/hash, six motion bindings와 true-alpha key/hash/selection을 모두 제공할 때만 route한다. reference, attack class, motion member, true-alpha drift는 각각 `open_ink_attack_successor_reference_mismatch`, `open_ink_attack_motion_not_attack`, `missing_open_ink_attack_motion_bindings`, `open_ink_attack_true_alpha_binding_mismatch`로 no-write 차단한다.
8. GeneratedMediaRecordGuide.md의 closed routingHashPayload를 정확히 투영하고 JCS canonical JSON의 전체 SHA-256을 계산한다. ID는 비 animation `gmroute2.{assetType}.{contentId}.{hash[0:20]}`, animation `gmroute2.animation.{contentId}.{animationRequestId}.{hash[0:20]}`로 만들며 20자는 정확히 lowercase hex 20자다.
9. canonical v2 record path와 같은 directory의 `routing_index.json`을 사용한다. index는 `generated_media_routing_index_v2` closed schema이고 `entries`는 routingRecordId를 key로 하며 value는 record identity/path/full payload hash/exact file hash의 정확한 projection이어야 한다.
10. 쓰기 전에 existing record/index 전체를 검증한다. exact pair는 bytes를 바꾸지 않고 재사용하고, valid record만 남은 partial success는 record를 재작성하지 않고 index entry만 복구한다. dangling/divergent index는 `routing_index_write_failed`, divergent record bytes나 hash-prefix collision은 `routing_record_collision`로 아무것도 덮어쓰지 않는다.
11. 새 record를 same-directory atomic no-clobber로 먼저 기록하고 reread/hash한 뒤 index를 atomic replace한다. record 성공 후 index 실패만 valid orphan record를 보존하고 `safeToRetry=true`로 path/hash를 반환한다. 그 밖의 blocked 요청은 record/index/failure placeholder/downstream handoff를 생성하지 않는다.
12. supersedesRoutingRecordId는 같은 scope의 유효한 v2 record일 때만 새 payload/record/index entry에 포함한다. 기존 record/index는 수정·삭제하지 않는다.
13. v1 registry/PixelLab row를 평가하거나 v1 record/index를 수정하지 않는다.
14. authoring/provider/download/packaging/evaluation/promotion/Slack/Unity/Git을 수행하지 않는다.
15. 성공 응답은 record/index bytes와 분리된 `generated_media_routing_receipt_v1` 한 건만 반환한다. record에 이미 저장된 normalizedRequest/sourcePlanningFiles/requiredElements/prohibitedElements/typeSpecification/profile locks를 control-plane 메시지에 다시 펼치지 않는다.
16. `generated_media_authority_bundle_receipt_v1`을 routing guide의 closed projection/JCS/hash/ID 규칙으로 계산한다. style binding이 있으면 asset/review record/review index를 separate immutable anchors로 포함한다. authoritative main SHA, requested stage scope, exact immutable artifact anchors, contract authority anchors, profile authority anchors 중 하나라도 바뀌거나 receipt가 없으면 full validation이며, 모두 같을 때만 unchanged validation을 재사용한다.
17. `generated_media_stage_delta_envelope_v1`에는 authority bundle ID/hash, fromStage/toStage, unitIdentity, 이번 stage의 새 artifact path/hash, prior validation receipt refs, publicationState/nextStep, providerState, prior pipeline chain ref만 포함한다. Git blob에서 읽을 수 있는 bulk fields와 nested handoff body를 다시 넣지 않는다.
18. child는 final stage delta envelope 한 건만 parent에 보낸다. parent는 동일 envelope를 다음 역할에 정확히 한 번 relay하고 requester/owner/Git 역할에는 full payload를 broadcast하지 않는다. 다른 observer에는 compact terminal status receipt 한 건만 보낸다.
19. routing guide의 validation receipt reuse matrix를 적용한다. mutation/CAS, authority freshness drift, stage artifact raw hash/projection, provider approval/capability/settings/cost/attempt 경계는 항상 재검증한다. exact unchanged authority/source/profile/schema receipt만 재사용할 수 있다.
20. commentary/status는 `generated_media_compact_status_v1` closed schema로 state change 또는 terminal에서만 한 번 emit한다. 동일 status/provider state/hash 재전송은 금지한다.
21. orchestration lineage는 response-only `generated_media_pipeline_receipt_chain_v1`의 append-only value로 전달한다. mutable orchestration record/index/path를 생성하지 않는다.
22. authority bundle, stage delta, routing receipt, pipeline chain, compact status 중 하나라도 schema/hash/transition/publication/relay 규칙에 어긋나면 success handoff를 emit하지 않고 기존 routing record/index를 수정하지 않는다.
23. task setup은 GeneratedMediaRequestRoutingGuide의 repository setup coordinator가 소유한다. queued client ID를 officialThreadId로 간주하거나 pending 중 replacement를 만들지 않는다. setup failure token은 `worktree_metadata_permission_denied`, `task_registry_collision`, `helper_setup_refresh_failed`, `tool_approval_required` 중 하나이며 자동 worktree 삭제를 수행하지 않는다.
24. persistent serial role worktree를 재사용하고 micro-stage별 새 worktree를 요구하지 않는다. sealed package 평가는 source Git worktree 밖에서 수행하며 evaluation role은 source repo를 fetch하지 않는다.

Output:
- fast-preview이면 `generated_media_fast_preview_terminal_receipt_v1` 한 건만 반환한다. providerCalled/submitCount/historicalSubmitCount/retryCount/costKnown을 사실대로 쓰고 unavailable cost를 0으로 쓰지 않는다. blocked pre-submit은 위 세 hard blocker 중 하나만, submit 후 실패는 `fast_preview_submit_failed_no_retry`만 사용한다.
- route_only_v2이면 아래 기존 routing receipt를 반환한다.
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
- failureType: missing_planning_handoff | planning_snapshot_mismatch | missing_identity_consistency_lock | missing_required_elements | missing_prohibited_elements | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_target_display_size | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | ambiguous_image_role | missing_background_scene_contract | missing_background_composition | missing_background_viewpoint | missing_background_horizon | missing_background_depth_layer_contract | missing_background_playable_area | missing_background_subject_contract | missing_background_canvas_contract | missing_background_aspect_ratio | missing_background_target_display | missing_background_safe_area | missing_background_consistency_lock | unsupported_icon_domain | unsupported_background_domain | missing_animation_request_id | duplicate_animation_request_id | missing_reference_image | reference_image_hash_mismatch | missing_final_frame_count | missing_animation_timing | missing_frame_order | missing_loop_contract | missing_key_poses | missing_fixed_cell_contract | missing_scale_lock | missing_vertical_motion_policy | missing_master_first_contract | unsupported_current_route | conflicting_routing_evidence | missing_style_reference_review_record | style_reference_review_record_hash_mismatch | style_reference_review_payload_mismatch | style_reference_asset_missing | style_reference_asset_hash_mismatch | style_reference_index_invalid | style_reference_binding_incomplete | style_reference_binding_scope_mismatch | style_reference_binding_projection_mismatch | style_reference_role_invalid | style_reference_semantic_transfer_forbidden | unknown_record_field | routing_record_collision | routing_record_write_failed | routing_index_write_failed
- missingFields / conflictingFields / candidatePipelines / requiredDecision / safeToRetry

검증:
- fast-preview positive vector는 warning이 있어도 executable prompt/reference와 authority/idempotency가 유효하면 exactly one submit/zero retry 및 같은 receipt의 visualEvaluation으로 끝나야 한다.
- fast-preview negative vector는 세 hard blocker만 submit 전에 멈추고 providerCalled=false/submitCount=0이어야 한다. completed/active/ambiguous key는 새 submit을 금지한다.
- strict `hosted_builtin_preview_v1`, promotable generation, normal routing v2 의미와 기존 bytes/hash는 바뀌지 않아야 한다.
- current route에 PixelLab이 없어야 한다.
- current registry의 execution role은 character/icon/background/animation 네 종류여야 한다.
- icon/background ambiguity는 fail-closed여야 한다.
- character route에 8-way/ordered_rotation_set이 없어야 한다.
- animation routing record 하나당 animationRequestId가 정확히 한 건이어야 한다.
- 같은 입력 재시도는 기존 record/index의 exact bytes를 보존해야 한다.
- payload field 하나가 바뀌면 full hash와 routingRecordId가 바뀌어야 한다.
- durable style binding은 four top-level projections에서만 exact하며 typeSpecification 내부에는 없어야 한다. 기존 no-binding record/index는 byte-identical read/reuse만 허용한다.
- occupied ID의 divergent bytes와 dangling/divergent index는 overwrite 없이 차단해야 한다.
- record가 index보다 먼저 publish되어야 하며 index 실패 후 retry는 orphan record를 재사용해야 한다.
- success control-plane receipt가 closed compact schema이고 persisted record payload를 되풀이하지 않아야 한다.
- same authority anchors/scope는 same bundle/chain identity이고 anchor 하나가 바뀌면 새 identity와 full validation이어야 한다.
- invalid stage/publication pair, forbidden bulk field, duplicate relay/status를 fail-closed로 거부해야 한다.
- pipeline orchestration record/index/path가 생성되지 않아야 한다.
- pipeline run당 coordinator authority fetch/receipt는 한 건이고 read-only child fetch는 0건이어야 한다.
- queued task replacement, 동시 setup mutation, 자동 worktree cleanup이 없어야 한다.
- blocked 요청은 record/index를 변경하지 않아야 한다.
- authoring 이후 단계를 실행하지 않아야 한다.
```
