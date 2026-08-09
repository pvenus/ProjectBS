# Popup Event Main Image Provider Generation Prompt

스토리 팝업 메인 이미지의 ImageGen provider 생성 단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 스토리 팝업 메인 이미지 하나의 provider generation 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
- AgentDocs/planning-guides/stage/PopupEventMainImageCreateGuide.md

Input:
- requestId: {optional_stable_request_id}
- eventId: {canonical_popup_event_id}
- promptRecordId: {generated_image_prompt_v1_record_id}
- contentSummary: {optional_popup_meaning}
- visualIntent: {optional_primary_moment}

작업:
1. artifactType=story_popup_main_image, contentId=eventId로 고정한다.
2. imagePolicy가 generate인지 확인하고 reuse/none이면 provider를 호출하지 않는다.
3. prompt record의 ImageGen profile, event identity, contentSnapshotHash와 domain adapter를 검증한다.
4. 검증된 scenePromptOriginal 하나를 수정 없이 ImageGen에 제출한다.
5. provider result refs, settings, attempts와 generation record를 보존한다.
6. 이미지를 다운로드·평가·이름 변경·프로젝트 복사하지 않는다.

Output:
- Event ID / Artifact Type / Image Policy
- Prompt Record ID / Hash / Provider Prompt Profile
- ImageGen Settings / Attempt Count
- Provider Result References
- Generation Record ID / Path / SHA-256
- Generation Status
- Download Handoff

실패 시 Output:
- status: blocked | failed
- failureType: missing_popup_source | unsupported_image_policy | prompt_record_not_found | prompt_record_stale | provider_prompt_profile_mismatch | provider_unavailable | provider_operation_failed | generation_record_write_failed
- Provider 호출 여부
- Required Next Action

검증:
- generate policy에서만 ImageGen을 호출해야 한다.
- scenePromptOriginal을 generation 작업에서 다시 작성하지 않아야 한다.
- provider generation 이후 단계를 실행하지 않아야 한다.
- Assets/ImagesGenerated 복사·평가·Unity·Slack·Git 작업을 수행하지 않아야 한다.
```
