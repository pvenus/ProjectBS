# Character Image Provider Generation Prompt

> Deprecated compatibility entry. Replaced by
> `AgentDocs/task-prompts/content/generated-media/PixelLabCharacterPromptAuthoringPrompt.md`
> and `AgentDocs/task-prompts/content/generated-media/PixelLabCharacterGenerationPrompt.md`.

PixelLab 캐릭터 이미지의 provider 생성 단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 캐릭터 이미지 하나의 provider generation 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
- AgentDocs/planning-guides/character/CharacterGenerateImage.md

Input:
- requestId: {optional_stable_request_id}
- characterId: {canonical_character_id}
- promptRecordId: {generated_image_prompt_v1_record_id}
- contentSummary: {optional_character_meaning}
- visualIntent: {optional_pose_or_rotation_intent}

작업:
1. artifactType=character_image, contentId=characterId로 고정한다.
2. current character-image adapter와 공통 generation pipeline을 적용한다.
3. prompt record의 PixelLab profile, 캐릭터 identity, contentSnapshotHash와 tool field mapping을 검증한다.
4. 검증된 PixelLab fieldPrompts를 지정 UI field에 그대로 제출한다.
5. 모든 provider result reference, 설정, attempt와 generation record를 보존한다.
6. 다운로드·평가·이름 변경·프로젝트 복사·Unity 처리를 수행하지 않는다.

Output:
- Character ID / Artifact Type
- Prompt Record ID / Hash / Provider Prompt Profile
- PixelLab Tool / Settings / Attempt Count
- Provider Result References
- Generation Record ID / Path / SHA-256
- Generation Status
- Download Handoff

실패 시 Output:
- status: blocked | failed
- failureType: missing_character_source | prompt_record_not_found | prompt_record_stale | provider_prompt_profile_mismatch | pixellab_unavailable | provider_operation_failed | generation_record_write_failed
- Provider 호출 및 credit 사용 여부
- Required Next Action

검증:
- 지정 PixelLab tool만 사용해야 한다.
- 캐릭터 identity와 field prompt를 generation 작업에서 변경하지 않아야 한다.
- provider generation 이후 단계를 실행하지 않아야 한다.
- Assets/ImagesGenerated, Unity, Slack과 Git 상태를 변경하지 않아야 한다.
```
