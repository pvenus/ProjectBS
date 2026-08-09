# ImageGen Prompt Authoring Prompt

도메인 공통 ImageGen 장면 prompt record만 작성합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 외부 이미지 기획 handoff 하나를 검증하고 ImageGen provider-ready scene prompt record만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md

Input:
- routingRecordFile: {project_relative_generated_media_routing_v1_record_path}
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- required handoff fields: domainType, imageProfile, sourcePlanningFiles, planningSnapshot, requiredElements, prohibitedElements, depictedMoment, subjects, environment, composition, camera, aspectRatio, backgroundPolicy
- priorPromptRecordId: {optional_revision_record_id; legacy v1 is read-only and revision writes a new v2 record}
- revisionReason: {required_when_revising}

작업:
1. routingRecordFile과 planningHandoffFile을 읽고 routingRecordId, registryVersion, selectedRegistryRowId, selectedPipeline=imagegen, selectedAuthoringPrompt=AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md, appliedProfile, normalizedRequest와 planningSnapshotHash를 exact 비교한다. routing record의 request/content identity와 planning hash가 handoff와 정확히 일치해야 한다.
2. depictedMoment, subjects, environment, composition, camera, aspectRatio, backgroundPolicy, requiredElements와 prohibitedElements를 모두 요구한다.
3. story summary나 battle ID만으로 장면·인물·장소·시점을 추론하지 않는다.
4. routing record가 선택한 exact registry row/profile을 pinned registry에 대조 검증하고, registry를 다시 선택하지 않는다. GeneratedMediaVisualPromptAuthoringGuide.md에 따라 planningOriginal과 분리된 generated_media_visual_brief_v1, visualBriefId와 visualEvidenceMap을 작성한다. 장면·구도·카메라·배경·색·소재를 보충하지 않는다.
5. normalized visual brief를 imagegen_composed_scene_prompt_v1 순서의 auditable scene sections와 하나의 응집된 scenePromptOriginal로 변환한다.
6. stage/battle 차이는 domainType/imageProfile로만 처리하며 도메인별 실행 prompt를 만들지 않는다.
7. 모든 문장을 source fact/constraint에 연결하고 prompt/settings를 분리해 visual brief와 payload hash를 계산한다.
8. generated_media_prompt_v2 JSON/Markdown을 AgentDocs/planning-data/generated-media-prompts/v1/imagegen_image/{contentId}/에 새 immutable ID로 저장하고 index를 갱신한다. 기존 v1은 수정하지 않는다.
9. ImageGen 실행, 다운로드, 평가, 승격, Unity, Slack, Git과 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content Identity
- Image Profile / Planning Snapshot Hash
- Visual Brief ID / SHA-256 / Guide Contract Version / Evidence Coverage
- Prompt Record ID / Paths / SHA-256
- Scene Prompt / Payload Hash / Settings Intent
- Status / Next Task

실패 시 Output:
- status: blocked | failed
- failureType: missing_routing_record | stale_routing_record | routing_record_mismatch | ambiguous_routing_record | invalid_planning_handoff | missing_required_elements | missing_prohibited_elements | missing_image_profile | unsupported_image_profile | missing_scene_specification | planning_authority_conflict | missing_visual_evidence | material_visual_contract_conflict | unsupported_visual_profile | provider_translation_contract_failed | prompt_record_write_failed
- 누락 근거 / 생성하지 않은 record / Required Next Action

검증:
- router가 선택한 registry row를 검증·소비하고 registry를 독립 재선택하지 않아야 한다.
- missing/stale/mismatch/ambiguous routing record이면 record를 쓰지 않아야 한다.
- 기획 장면을 보충하지 않고 제공된 사실만 조합해야 한다.
- planningOriginal, normalized visual brief, provider payload와 settings가 분리되어야 한다.
- 하나의 cohesive ImageGen prompt여야 한다.
- JSON/Markdown/hash가 일치하고 provider를 실행하지 않아야 한다.
```
