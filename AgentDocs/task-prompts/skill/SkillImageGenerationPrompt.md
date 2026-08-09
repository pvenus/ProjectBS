# Skill Animation Provider Generation Prompt

> Deprecated compatibility entry. Replaced by
> `AgentDocs/task-prompts/content/generated-media/PixelLabAnimationPromptAuthoringPrompt.md`
> and `AgentDocs/task-prompts/content/generated-media/PixelLabAnimationGenerationPrompt.md`.

캐릭터 독립형 스킬 애니메이션의 PixelLab provider 생성 단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 스킬 애니메이션 하나의 provider generation 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
- AgentDocs/planning-guides/skill/SkillImageGenerationGuide.md

Input:
- requestId: {optional_stable_request_id}
- skillId: {canonical_equipment_skill_id}
- promptRecordId: {generated_image_prompt_v1_record_id}
- contentSummary: {optional_skill_animation_meaning}

작업:
1. artifactType=skill_animation, contentId=skillId로 고정한다.
2. eligibility와 current skill-animation adapter를 검증한다.
3. prompt record의 PixelLab profile, reference description/action fields, contentSnapshotHash와 tool version을 검증한다.
4. 검증된 fieldPrompts와 provider settings로 reference image와 animation provider operation을 실행한다.
5. reference 및 animation result refs, settings, attempts와 generation record를 보존한다.
6. provider 결과를 다운로드·평가·프로젝트 복사하지 않는다.

Output:
- Skill ID / Artifact Type
- Prompt Record ID / Hash / Provider Prompt Profile
- PixelLab Tool Version / Settings / Attempt Count
- Reference and Animation Result References
- Generation Record ID / Path / SHA-256
- Generation Status
- Download Handoff

실패 시 Output:
- status: blocked | failed
- failureType: ineligible_skill_animation | prompt_record_not_found | prompt_record_stale | provider_prompt_profile_mismatch | pixellab_unavailable | provider_operation_failed | generation_record_write_failed
- Provider 호출 및 credit 사용 여부
- Required Next Action

검증:
- 지정 PixelLab animation tool과 saved field prompts만 사용해야 한다.
- provider result refs와 generation record까지만 생성해야 한다.
- 다운로드·평가·Assets/ImagesGenerated 복사·Unity·Git 작업을 수행하지 않아야 한다.
```
