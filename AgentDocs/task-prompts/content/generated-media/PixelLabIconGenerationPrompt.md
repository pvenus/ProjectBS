# PixelLab Icon Generation Prompt

도메인 공통 아이콘 prompt를 PixelLab에 제출하고 provider 결과 참조를
기록합니다. 다운로드·보존·패키징·평가는 수행하지 않습니다.

## Prompt

```text
현재 ProjectBS 저장소에서 PixelLab 아이콘 provider 생성 요청 하나만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- promptRecordId: {generated_media_prompt_v2_or_validated_legacy_v1_record_id}

작업:
1. prompt schema를 확인한다. 신규 v2이면 visualBriefId/hash/evidence coverage까지 검증하고, 기존 v1이면 GeneratedMediaRecordGuide.md의 read-only compatibility gate를 충족해야 한다. assetType=icon, domainType, iconProfile, planning snapshot과 prompt payload/hash를 검증한다.
2. requiredElements/prohibitedElements 누락이나 stale prompt이면 provider를 호출하지 않는다.
3. 저장된 field text/settings를 Create UI elements (Pro)에 그대로 제출한다.
4. cost evidence, attempts와 모든 variation result refs를 generation record에 기록한다.
5. profile에 deterministic provider-operation rule이 있으면 provisional ref만 기록하고 점수를 부여하지 않는다.
6. pixellab_icon_original_png_v1/single_image preservation handoff를 기록하고 종료한다.
7. 다운로드, 파일 저장/변환, hash, package seal, 평가, 승격, Slack, Unity, Git, 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content / Icon Profile
- Generation Record ID / Status
- PixelLab Settings / Cost / Attempts / Variation Refs
- Provisional Selection Status
- Preservation Handoff / Next Task: preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: invalid_planning_handoff | prompt_record_missing | prompt_schema_version_unsupported | prompt_record_stale | provider_prompt_hash_mismatch | visual_brief_identity_mismatch | visual_brief_hash_mismatch | visual_evidence_map_incomplete | pixellab_unavailable | provider_operation_failed | provider_variations_missing | ambiguous_provider_result | generation_record_write_failed
- Provider 호출·비용 여부 / 보존된 provider refs / Required Next Action

검증:
- skill/item 전용 실행 프롬프트로 분기하지 않아야 한다.
- generation record에는 provider refs와 handoff만 있어야 한다.
- 다운로드·패키징·평가를 실행하지 않아야 한다.
```
