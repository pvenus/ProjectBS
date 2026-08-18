# ImageGen Single Animation Prompt Authoring Prompt

## Prompt

```text
current v2 animation routing record 하나를 검증하고 정확히 한 animationRequestId의 ImageGen prompt record만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenAnimationPipelineGuide.md

Input:
- routingRecordFile: {generated_media_routing_v2_animation_request_path}
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- animationRequestId: {exact_single_id}
- referencePromptRecordPath: {required_for_character_animation; omit_for_skill_animation}
- referencePromptRecordSha256: {required_for_character_animation; omit_for_skill_animation}
- expressionProfileKey: {required_for_character_animation; omit_for_skill_animation}
- expressionProfilePayloadHash: {required_for_character_animation; omit_for_skill_animation}

작업:
0. exact noninteractive execution policy 범위의 read/hash/schema/test와 bounded prompt record/index write는 재승인 없이 수행한다. host-required bundled approval은 coordinator의 한 건만 재사용하고 새 권한 경계에서는 partial write 없이 차단한다.
1. routing record의 normalized animationRequest가 객체 하나이고 ID가 입력과 동일한지 확인한다. planning handoff의 같은 ID 원본 항목과 exact 비교한다. normalized 배열/복수/병합은 차단한다.
2. reference identity/path/hash, final frame count/timing/order/loop/key poses, fixed cell, scale lock, vertical motion, background/noShadow/outline, anchor, masterFirst를 검증한다. 신규 요청은 animationSourceMode=provider_native_animated_gif와 extractionMode=gif_timeline_exact가 모두 정확해야 한다. fixed_cell_only는 기존 immutable record의 read-only history에만 허용하고 신규 prompt를 쓰지 않는다.
3. character animation이면 네 reference/profile 입력이 모두 있는지 확인한다. referencePromptRecordPath의 exact file bytes SHA-256을 다시 계산해 referencePromptRecordSha256과 비교하고, immutable generated_media_prompt_v3에서 expressionProfilePayload를 읽는다. Visual guide 규칙으로 canonicalize해 hash를 다시 계산한 뒤 record·handoff·registry의 expressionProfileKey/expressionProfilePayloadHash가 모두 정확히 같은지 비교한다. 검증된 payload를 수정·번역·재정렬·요약하지 않고 상속한다. 두 lock-array profile은 non-empty lock arrays를, sparse-ink key는 empty compatibility lock arrays와 여덟 closed policy member를 byte-for-byte 상속한다. sparse-ink의 approved finalFrameCount 각 frame마다 omission 35-50%, 3-6 accents, exact palette, motion cues, darkest identity/action anchor와 identity-anchor stability를 evidence에 연결하며 missing_*_style_lock을 적용하지 않는다.
3a. motion-flow successor는 유일한 composed exception이다. reference prompt의 bold v2 payload/hash와 exact 18/8, 64/56/5, color anchors, halo를 byte-preserving base로 검증하고 successor payload/hash와 여덟 approved motion bindings를 별도로 검증한다. 모든 active frame에 directional faded-indigo sword/torso 3-5 marks, gray-brown shoulder/hem lag-settle, bounded dark-neutral sword/torso trajectory, ordered continuity와 identity/equipment anchor locks를 직접 투영한다. base/projection 불일치는 `bold_outline_motion_flow_base_projection_mismatch`, motion evidence 누락은 `bold_outline_motion_flow_evidence_omission`, provider prose 누락/약화는 `provider_prompt_bold_outline_motion_flow_projection_missing`으로 prompt publication 전에 차단한다.
4. skill animation이면 네 character reference/profile 필드가 모두 없어야 하며 캐릭터 style lock을 적용하지 않는다. 하나라도 있으면 unexpected_character_style_reference로 차단한다.
5. 최종 승인 frame count/order/timing/loop를 이미 포함하는 playable animated GIF 하나를 provider가 직접 생성하도록 visual brief와 provider prompt를 작성한다. 한 장의 still image, contact sheet, sprite sheet, collage, video, 독립 frame 생성 후 조립을 허용하거나 요청하지 않는다. character animation의 copy-ready prompt에는 선택 profile의 complete projection을 직접 포함한다. sparse-ink attack은 3-5 indigo sword/torso marks, gray-brown shoulder/hem inertia와 searching overlap/taper-break/robe-sleeve lag/sword arc/overshoot-smear를 요구하고 static repeated action frame을 금지한다. oversampling/선택, 프레임 crop/scale/recenter를 지시하지 않는다.
6. generated_media_prompt_v3를 animationRequestId 포함 v2 path에 기록한다.
7. provider 및 packaging/evaluation을 실행하지 않는다.

Output:
- status / animationRequestId / prompt record paths and hashes
- character animation일 때 referencePromptRecordPath / referencePromptRecordSha256 / expressionProfileKey / expressionProfilePayload / expressionProfilePayloadHash / profile projection coverage
- animationSourceMode=provider_native_animated_gif / extractionMode=gif_timeline_exact
- provider / providerInterface=configured_animated_gif_capability / structureProfile=animation_gif_frame_set_v2 / nextStep=generation

실패 시 Output:
- status: blocked
- failureType: GeneratedMediaImageGenOnlyContractGuide.md 8.1 및 8.3의 현재 animation authoring 적용 token 중 정확히 하나. 두 lock-array profile만 lock token을 사용하고 sparse는 four sparse projection token만 사용한다. character animation identity는 8.3의 일곱 character token을, skill animation은 unexpected_character_style_reference만 적용하며 alias를 만들지 않는다.
- missingFields / requiredDecision / safeToRetry

검증:
- 정확히 animationRequestId 한 건만 포함해야 한다.
- frame count는 처음부터 최종 개수여야 한다.
- provider prompt는 playable animated GIF 직접 출력만 요구해야 하며 still/contact-sheet/sprite-sheet/frame-by-frame 합성을 fallback으로 포함하면 안 된다.
- character animation은 immutable reference prompt record의 file hash와 canonical expression payload hash를 재계산하고 exact 상속하며 선택 profile의 evidence coverage가 완전해야 한다.
- skill animation에는 네 character reference/profile 입력과 출력이 없어야 한다.
- generation과 extraction을 실행하지 않아야 한다.
```
