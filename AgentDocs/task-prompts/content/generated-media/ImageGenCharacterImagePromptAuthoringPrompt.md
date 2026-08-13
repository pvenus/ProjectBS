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
- AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md

Input:
- routingRecordFile: {project_relative_generated_media_routing_v2_record}
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v2}
- required: request/content/source/snapshot identity, requiredElements, prohibitedElements, identityConsistencyLock, singleImageSpecification

작업:
1. registryVersion=v2, provider=imagegen, assetType=character_single_image, exact registry row와 snapshot/hash를 검증한다.
2. viewpoint, pose, framing, canvas, targetDisplaySize, safeArea, background, generationBackground, noShadow, outline, pelvis/root와 ground-contact anchor를 모두 검증한다.
3. 승인 기획을 provider-neutral visual brief로 정규화하고 모든 문장을 evidence/constraint에 연결한다. 캐릭터 사실과 ProjectBS 캐릭터 표현 profile을 분리하고 누락된 외형·시점·색·배경을 만들지 않는다.
4. GeneratedMediaVisualPromptAuthoringGuide의 canonical expressionProfilePayload를 exact 사용하고 RFC 8785 JCS canonical JSON UTF-8 bytes의 SHA-256을 다시 계산해 등록된 expressionProfilePayloadHash와 비교한다. positiveStyleLock과 negativeStyleLock의 각 constraintId/statement/authorityRef 및 배열 순서를 보존하고 모든 항목의 evidence를 기록한다. `stylized` 한 단어로 대체하지 않는다.
5. 한 승인 시점의 cohesive ImageGen prompt 하나에 positive/negative style lock 원문을 모두 직접 포함하고 settings intent를 분리한다. photographic/photorealistic/cinematic portrait, realistic skin pores, lens/DOF/bokeh, volumetric portrait light, painterly/PBR 3D render 또는 western-fantasy realism을 유도하는 표현을 넣지 않는다. 8-way, rotation, ordered_rotation_set을 넣지 않는다.
6. 승인 기획과 style profile이 material conflict이면 몰래 변환하지 않고 character_style_profile_conflict로 중단한다.
7. GeneratedMediaRecordGuide.md::Prompt v3의 closed
   generated_media_prompt_hash_payload_v3를 source record에서 exact projection하고
   RFC 8785 JCS UTF-8 SHA-256, deterministic ID/path, closed nested field sets를 검증한다.
8. scenePromptOriginal의 LF-only/no-BOM/exactly-one-terminal-LF raw Markdown bytes와
   hash를 먼저 계산한다. closed generated_media_prompt_v3 JSON/Markdown/closed
   generated_media_prompt_index_v3를 same-scope lock, no-clobber, CAS, rollback 규칙으로
   게시한다. unknown/missing field, hash mismatch, collision, invalid/dangling index는
   덮어쓰거나 정규화하지 않는다. exact existing triplet 또는 recoverable exact
   JSON+Markdown orphan만 검증해 reused_identical로 재사용한다.
9. 게시된 JSON/Markdown/index exact bytes를 다시 읽고 세 raw file hash를 포함하는
   closed detached generated_media_generation_handoff_v2와 그 canonical hash를 반환한다.
10. ImageGen 호출, download, packaging, evaluation, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status: ready_for_generation | reused_identical
- promptRecordId / promptPayloadSha256 / prompt record path/raw SHA-256
- prompt Markdown path/raw SHA-256 / prompt index path/raw SHA-256
- complete generated_media_generation_handoff_v2 / generationHandoffSha256
- identity / registry row / expressionProfileKey / expressionProfilePayload / expressionProfilePayloadHash / visual brief evidence coverage / positiveStyleLock coverage / negativeStyleLock coverage
- provider=imagegen / structureProfile=character_single_image_v2
- nextStep: generation

실패 시 Output:
- status: blocked
- failureType: GeneratedMediaImageGenOnlyContractGuide.md 8.1 및 8.3의 character single-image authoring 적용 token 중 정확히 하나. expression-profile에는 payload/key/hash missing 또는 mismatch token만 적용하며 reference-record/skill token과 alias를 사용하지 않는다. record publication에는 exact record/hash/index/Markdown/write/rollback token만 사용한다.
- missingFields / requiredDecision / safeToRetry

검증:
- provider는 imagegen 하나여야 한다.
- 캐릭터 신규 계약에 8-way/rotation set이 없어야 한다.
- planning, brief, provider prompt, settings가 분리되어야 한다.
- copy-ready prompt에 positive/negative style lock 원문이 모두 있고 각 항목 evidence coverage가 완전해야 한다.
- JSON은 canonicalJson+LF, Markdown은 scenePromptOriginal UTF-8 bytes+LF이며 CRLF/BOM/추가 terminal LF가 없어야 한다.
- prompt record/index/handoff projection과 모든 ID/path/hash가 exact 재계산되어야 한다.
- 실패 시 partial/orphan file이나 generation handoff를 새로 남기지 않아야 한다.
- provider 및 후속 단계를 실행하지 않아야 한다.
```
