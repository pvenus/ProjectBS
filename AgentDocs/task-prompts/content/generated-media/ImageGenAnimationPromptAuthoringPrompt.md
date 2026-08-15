# ImageGen Single Animation Prompt Authoring Prompt

## Prompt

```text
current v2 animation routing record 하나를 검증하고 정확히 한 animationRequestId의 ImageGen prompt record만 작성해줘.

참조 가이드:
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
1. routing record의 normalized animationRequest가 객체 하나이고 ID가 입력과 동일한지 확인한다. planning handoff의 같은 ID 원본 항목과 exact 비교한다. normalized 배열/복수/병합은 차단한다.
2. reference identity/path/hash, final frame count/timing/order/loop/key poses, fixed cell, scale lock, vertical motion, background/noShadow/outline, anchor, masterFirst를 검증한다.
3. character animation이면 네 reference/profile 입력이 모두 있는지 확인한다. referencePromptRecordPath의 exact file bytes SHA-256을 다시 계산해 referencePromptRecordSha256과 비교하고, immutable generated_media_prompt_v3에서 expressionProfilePayload를 읽는다. Visual guide 규칙으로 canonicalize해 hash를 다시 계산한 뒤 record·handoff·registry의 expressionProfileKey/expressionProfilePayloadHash가 모두 정확히 같은지 비교한다. 검증된 payload를 수정·번역·재정렬·요약하지 않고 상속한다. animation-ready minimal key이면 proportionProjection/detailDensityBudget/colorValueBudget/authoringProjectionContract 네 closed member도 byte-for-byte 상속하고 누락 시 missing_expression_profile_payload로 차단한다. frame마다 선 hierarchy, 단순화 수준, 비례, sparse contour, color/value budget, 얼굴 landmark, 복식 layer, 장비·무기 구조가 재해석되지 않도록 consistency 문장을 evidence에 연결한다.
4. skill animation이면 네 character reference/profile 필드가 모두 없어야 하며 캐릭터 style lock을 적용하지 않는다. 하나라도 있으면 unexpected_character_style_reference로 차단한다.
5. 최종 승인 frame count의 coherent master 하나를 만드는 visual brief와 ImageGen prompt를 작성한다. character animation의 copy-ready prompt에는 positive/negative style lock 원문을 모두 직접 포함한다. oversampling/선택, 프레임 crop/scale/recenter를 지시하지 않는다.
6. generated_media_prompt_v3를 animationRequestId 포함 v2 path에 기록한다.
7. provider 및 packaging/evaluation을 실행하지 않는다.

Output:
- status / animationRequestId / prompt record paths and hashes
- character animation일 때 referencePromptRecordPath / referencePromptRecordSha256 / expressionProfileKey / expressionProfilePayload / expressionProfilePayloadHash / positiveStyleLock coverage / negativeStyleLock coverage
- provider=imagegen / structureProfile=animation_gif_frame_set_v2 / nextStep=generation

실패 시 Output:
- status: blocked
- failureType: GeneratedMediaImageGenOnlyContractGuide.md 8.1 및 8.3의 현재 animation authoring 적용 token 중 정확히 하나. character animation은 8.3의 일곱 character token을, skill animation은 unexpected_character_style_reference만 적용하며 alias를 만들지 않는다.
- missingFields / requiredDecision / safeToRetry

검증:
- 정확히 animationRequestId 한 건만 포함해야 한다.
- frame count는 처음부터 최종 개수여야 한다.
- character animation은 immutable reference prompt record의 file hash와 canonical expression payload hash를 재계산하고 exact 상속하며 positive/negative lock evidence coverage가 완전해야 한다.
- skill animation에는 네 character reference/profile 입력과 출력이 없어야 한다.
- generation과 extraction을 실행하지 않아야 한다.
```
