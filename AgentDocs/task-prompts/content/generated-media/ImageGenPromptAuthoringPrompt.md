# ImageGen Prompt Authoring Prompt

도메인 공통 ImageGen 장면 prompt record만 작성합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 외부 이미지 기획 handoff 하나를 검증하고 ImageGen provider-ready scene prompt record만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- required handoff fields: domainType, imageProfile, sourcePlanningFiles, planningSnapshot, requiredElements, prohibitedElements, depictedMoment, subjects, environment, composition, camera, aspectRatio, backgroundPolicy
- priorPromptRecordId: {optional_revision_record_id}
- revisionReason: {required_when_revising}

작업:
1. assetType=imagegen_image, domainType, imageProfile, content identity, source hashes와 planningSnapshotHash를 검증한다.
2. depictedMoment, subjects, environment, composition, camera, aspectRatio, backgroundPolicy, requiredElements와 prohibitedElements를 모두 요구한다.
3. story summary나 battle ID만으로 장면·인물·장소·시점을 추론하지 않는다.
4. imagegen_composed_scene_prompt_v1 순서로 auditable scene sections를 만들고 하나의 응집된 scenePromptOriginal을 작성한다.
5. stage/battle 차이는 domainType/imageProfile로만 처리하며 도메인별 실행 prompt를 만들지 않는다.
6. 모든 문장을 source fact/constraint에 연결하고 prompt/settings를 분리해 payload hash를 계산한다.
7. generated_media_prompt_v1 JSON/Markdown을 AgentDocs/planning-data/generated-media-prompts/v1/imagegen_image/{contentId}/에 새 immutable ID로 저장하고 index를 갱신한다.
8. ImageGen 실행, 다운로드, 평가, 승격, Unity, Slack, Git과 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content Identity
- Image Profile / Planning Snapshot Hash
- Prompt Record ID / Paths / SHA-256
- Scene Prompt / Payload Hash / Settings Intent
- Status / Next Task

실패 시 Output:
- status: blocked | failed
- failureType: invalid_planning_handoff | missing_required_elements | missing_prohibited_elements | missing_image_profile | unsupported_image_profile | missing_scene_specification | planning_authority_conflict | prompt_record_write_failed
- 누락 근거 / 생성하지 않은 record / Required Next Action

검증:
- 기획 장면을 보충하지 않고 제공된 사실만 조합해야 한다.
- 하나의 cohesive ImageGen prompt여야 한다.
- JSON/Markdown/hash가 일치하고 provider를 실행하지 않아야 한다.
```
