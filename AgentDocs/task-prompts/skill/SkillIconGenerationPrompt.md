# Skill Icon Provider Generation Prompt

스킬 아이콘의 PixelLab provider 생성 단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 스킬 아이콘 하나의 provider generation 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
- AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md

Input:
- requestId: {optional_stable_request_id}
- equipmentId: {canonical_equipment_skill_id}
- promptRecordId: {generated_image_prompt_v1_record_id}
- contentSummary: {optional_skill_meaning}

작업:
1. artifactType=skill_icon, contentId=equipmentId로 고정한다.
2. current skill-icon adapter와 공통 generation pipeline을 적용한다.
3. prompt record의 PixelLab profile, equipmentId, contentSnapshotHash와 tool field mapping을 검증한다.
4. 검증된 concise fieldPrompts를 Create UI elements (Pro)에 그대로 제출한다.
5. 16개 variation을 포함한 provider result refs, 설정, attempt와 generation record를 보존한다.
6. 이미지 다운로드, edit/overlay/normalization, 평가와 프로젝트 복사를 수행하지 않는다.

Output:
- Equipment ID / Artifact Type
- Prompt Record ID / Hash / Provider Prompt Profile
- PixelLab Tool / Settings / Attempt Count
- Provider Result References
- Generation Record ID / Path / SHA-256
- Generation Status
- Download Handoff

실패 시 Output:
- status: blocked | failed
- failureType: missing_skill_source | prompt_record_not_found | prompt_record_stale | provider_prompt_profile_mismatch | pixellab_unavailable | provider_operation_failed | generation_record_write_failed
- Provider 호출 및 credit 사용 여부
- Required Next Action

검증:
- prompt record에 저장된 PixelLab field text를 재작성하지 않아야 한다.
- provider result와 generation record까지만 생성해야 한다.
- 다운로드·평가·후처리·Assets/ImagesGenerated 복사·Unity·Git 작업을 수행하지 않아야 한다.
```
