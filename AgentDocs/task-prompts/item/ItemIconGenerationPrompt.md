# Item Icon Provider Generation Prompt

> Deprecated compatibility entry. Replaced by
> `AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md`
> and `AgentDocs/task-prompts/content/generated-media/PixelLabIconGenerationPrompt.md`.

아이템 아이콘의 PixelLab provider 생성 단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 아이템 아이콘 하나의 provider generation 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
- AgentDocs/planning-guides/item/ItemIconGenerationGuide.md

Input:
- requestId: {optional_stable_request_id}
- itemId: {canonical_item_id}
- promptRecordId: {generated_image_prompt_v1_record_id}
- contentSummary: {optional_item_meaning}

작업:
1. artifactType=item_icon, contentId=itemId로 고정한다.
2. current item-icon adapter와 공통 generation pipeline을 적용한다.
3. prompt record의 PixelLab profile, item identity, contentSnapshotHash와 tool field mapping을 검증한다.
4. 검증된 fieldPrompts를 PixelLab Create UI elements (Pro)의 지정 field에 그대로 제출한다.
5. 모든 variation/result reference, 설정, attempt와 generation record를 보존한다.
6. 후보를 다운로드·추출·평가·정규화하거나 프로젝트에 복사하지 않는다.

Output:
- Item ID / Artifact Type
- Prompt Record ID / Hash / Provider Prompt Profile
- PixelLab Tool / Settings / Attempt Count
- Provider Result References
- Generation Record ID / Path / SHA-256
- Generation Status
- Download Handoff

실패 시 Output:
- status: blocked | failed
- failureType: missing_item_source | prompt_record_not_found | prompt_record_stale | provider_prompt_profile_mismatch | pixellab_unavailable | provider_operation_failed | generation_record_write_failed
- Provider 호출 및 credit 사용 여부
- Required Next Action

검증:
- PixelLab provider generation만 수행해야 한다.
- prompt record 원문과 item identity를 변경하지 않아야 한다.
- 다운로드·평가·normalization·Assets/ImagesGenerated 복사·Unity·Git 작업을 수행하지 않아야 한다.
```
