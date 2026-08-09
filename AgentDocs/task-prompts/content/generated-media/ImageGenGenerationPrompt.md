# ImageGen Generation Prompt

도메인 공통 scene prompt를 ImageGen에 제출하고 provider 결과 참조를
기록합니다. 다운로드·보존·패키징·평가는 수행하지 않습니다.

## Prompt

```text
현재 ProjectBS 저장소에서 ImageGen provider 생성 요청 하나만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- promptRecordId: {generated_media_prompt_v2_or_validated_legacy_v1_record_id}

작업:
1. prompt schema를 확인한다. 신규 v2이면 visualBriefId/hash/evidence coverage까지 검증하고, 기존 v1이면 GeneratedMediaRecordGuide.md의 read-only compatibility gate를 충족해야 한다. assetType=imagegen_image, domainType/imageProfile, planning snapshot과 scene prompt hash를 검증한다.
2. scene 누락, unsupported profile 또는 stale prompt이면 provider를 호출하지 않는다.
3. 저장된 scenePromptOriginal 하나를 바꾸지 않고 정확한 settings로 ImageGen에 제출한다.
4. settings, cost evidence, attempts와 모든 provider result refs를 generation record에 기록한다.
5. profile에 deterministic provider-operation rule이 있으면 provisional ref만 기록하고 평가하지 않는다.
6. imagegen_original_media_v1/single_image preservation handoff를 기록하고 종료한다.
7. PixelLab fallback, 다운로드, 파일 저장/변환, hash, package seal, 평가, 승격, Slack, Unity, Git, 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content / Image Profile
- Generation Record ID / Status
- ImageGen Settings / Cost / Attempts / Result Refs
- Provisional Selection Status
- Preservation Handoff / Next Task: preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: invalid_planning_handoff | missing_scene_specification | unsupported_image_profile | prompt_record_missing | prompt_schema_version_unsupported | prompt_record_stale | provider_prompt_hash_mismatch | visual_brief_identity_mismatch | visual_brief_hash_mismatch | visual_evidence_map_incomplete | provider_unavailable | provider_operation_failed | ambiguous_provider_result | generation_record_write_failed
- Provider 호출·비용 여부 / 보존된 provider refs / Required Next Action

검증:
- stage/battle 차이는 domainType/profile로만 처리해야 한다.
- generation record에는 provider refs와 handoff만 있어야 한다.
- 다운로드·패키징·평가를 실행하지 않아야 한다.
```
