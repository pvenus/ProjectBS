# PixelLab Icon Prompt Authoring Prompt

도메인 공통 PixelLab 아이콘 provider prompt record만 작성합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 외부 아이콘 기획 handoff 하나를 검증하고 PixelLab 아이콘 provider-ready prompt record만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md

Input:
- routingRecordFile: {project_relative_generated_media_routing_v1_record_path}
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- required handoff fields: domainType, iconProfile, sourcePlanningFiles, planningSnapshot, requiredElements, prohibitedElements, subjectIdentity, semanticEffect, backgroundPolicy, targetDisplayContract
- priorPromptRecordId: {optional_revision_record_id; legacy v1 is read-only and revision writes a new v2 record}
- revisionReason: {required_when_revising}

작업:
1. routingRecordFile과 planningHandoffFile을 읽고 routingRecordId, registryVersion, selectedRegistryRowId, selectedPipeline=pixellab_icon, selectedAuthoringPrompt=AgentDocs/task-prompts/content/generated-media/PixelLabIconPromptAuthoringPrompt.md, appliedProfile, normalizedRequest와 planningSnapshotHash를 exact 비교한다. routing record의 request/content identity와 planning hash가 handoff와 정확히 일치해야 한다.
2. iconProfile, subjectIdentity, semanticEffect, backgroundPolicy, targetDisplayContract, requiredElements와 prohibitedElements를 모두 요구한다.
3. exactCountElements가 선언되면 원문 개수를 보존하고 임의 개수나 상징을 추가하지 않는다.
4. routing record가 선택한 exact registry row/profile을 pinned registry에 대조 검증하고, registry를 다시 선택하지 않는다. GeneratedMediaVisualPromptAuthoringGuide.md에 따라 planningOriginal과 분리된 generated_media_visual_brief_v1, visualBriefId와 visualEvidenceMap을 작성한다. 의미·상징·색·소재·배경을 보충하지 않는다.
5. pixellab_icon_prompt_v1에 따라 normalized visual brief의 dominant silhouette, composition, essential effect, palette/material/background, concise prohibitions 순서만 PixelLab 실제 field text로 변환한다.
6. 스킬 효과·아이템 의미·등급·배경 필요성을 추론하지 않고 skill/item 전용 prompt로 분기하지 않는다.
7. settings intent와 prompt text를 분리하고 visual brief/source fact/constraint 연결 및 payload hash를 작성한다.
8. generated_media_prompt_v2 JSON/Markdown을 AgentDocs/planning-data/generated-media-prompts/v1/icon/{contentId}/에 새 immutable ID로 저장하고 index를 갱신한다. 기존 v1은 수정하지 않는다.
9. PixelLab 실행, 다운로드, 평가, 승격, Unity, Slack, Git과 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content Identity
- Icon Profile / Planning Snapshot Hash
- Visual Brief ID / SHA-256 / Guide Contract Version / Evidence Coverage
- Prompt Record ID / Paths / SHA-256
- Provider Prompt Profile / Payload Hash / Settings Intent
- Status / Next Task

실패 시 Output:
- status: blocked | failed
- failureType: missing_routing_record | stale_routing_record | routing_record_mismatch | ambiguous_routing_record | invalid_planning_handoff | missing_required_elements | missing_prohibited_elements | missing_icon_profile | unsupported_icon_profile | missing_icon_subject | planning_authority_conflict | missing_visual_evidence | material_visual_contract_conflict | unsupported_visual_profile | provider_translation_contract_failed | prompt_record_write_failed
- 누락 근거 / 생성하지 않은 record / Required Next Action

검증:
- router가 선택한 registry row를 검증·소비하고 registry를 독립 재선택하지 않아야 한다.
- missing/stale/mismatch/ambiguous routing record이면 record를 쓰지 않아야 한다.
- domainType 차이를 입력/profile로만 처리해야 한다.
- planningOriginal, normalized visual brief, provider payload와 settings가 분리되어야 한다.
- 기획 요소를 보충하거나 별도 도메인 prompt를 만들지 않아야 한다.
- JSON/Markdown/hash가 일치하고 provider를 실행하지 않아야 한다.
```
