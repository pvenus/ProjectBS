# PixelLab Character Prompt Authoring Prompt

PixelLab character main image or one requested character animation prompt record
only. It does not operate PixelLab.

## Prompt

```text
현재 ProjectBS 저장소에서 외부 기획 handoff 하나를 검증하고 PixelLab 캐릭터용 provider-ready prompt record만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- required handoff fields: sourcePlanningFiles, planningSnapshot, requiredElements, prohibitedElements, characterIdentity and appearanceSpecification, plus animationRequests for animation runs
- runType: {character_main_image | character_animation}
- animationRequestId: {required_only_for_character_animation}
- priorPromptRecordId: {optional_revision_record_id}
- revisionReason: {required_when_revising}

작업:
1. planningHandoffFile의 schema, assetType, domainType=character, source hashes와 planningSnapshotHash를 검증한다.
2. requiredElements와 prohibitedElements가 비어 있거나 sourcePlanningFiles/snapshot이 불완전하면 작성하지 않는다.
3. character_main_image이면 characterIdentity, appearanceSpecification과 정확한 8-way rotationContract를 요구한다.
4. character_animation이면 animationRequestId와 정확히 일치하는 animationRequests 항목, characterProviderIdentity, actionSpecification, directionOrder, frameContract와 mirroringPolicy를 요구한다.
5. 요청되지 않은 Attack/Idle/Move를 추가하지 않고 combat/skill/성격 자료로 동작을 보충하지 않는다.
6. main은 pixellab_character_prompt_v1, animation은 pixellab_character_animation_prompt_v1 provider profile로 PixelLab 실제 필드용 간결한 영어 textOriginal을 작성한다.
7. 모든 문장에 planning source fact와 constraint ID를 연결한다.
8. provider settings intent와 copy-ready prompt text를 분리하고 payload SHA-256을 계산한다.
9. generated_media_prompt_v1 immutable JSON과 copy-ready Markdown을 AgentDocs/planning-data/generated-media-prompts/v1/{assetType}/{contentId}/에 새 ID로 저장하고 index를 갱신한다.
10. PixelLab 실행, 이미지/애니메이션 생성, 다운로드, 추출, 평가, 승격, Unity, Slack, Git과 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content Identity
- Animation Request ID when applicable
- Planning Snapshot Hash
- Prompt Record ID / JSON / Markdown / SHA-256
- Provider Prompt Profile / Payload Hash / Settings Intent
- Status: ready_for_generation | blocked
- Next Task: PixelLab character generation | none

실패 시 Output:
- status: blocked | failed
- failureType: missing_planning_handoff | invalid_planning_handoff | missing_required_elements | missing_prohibited_elements | missing_character_identity | missing_appearance_specification | invalid_rotation_contract | missing_animation_requests | invalid_animation_request | animation_request_not_in_handoff | prompt_record_write_failed
- 누락·충돌 근거와 생성하지 않은 record
- Required Next Action

검증:
- 기획 handoff를 수정하거나 기획 사실을 추론하지 않아야 한다.
- animation은 요청된 한 항목만 prompt record로 만들어야 한다.
- JSON/Markdown copy-ready text와 hash가 일치해야 한다.
- provider와 후속 단계를 실행하지 않아야 한다.
```
