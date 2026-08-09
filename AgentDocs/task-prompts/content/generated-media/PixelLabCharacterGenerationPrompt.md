# PixelLab Character Generation Prompt

검증된 prompt record를 PixelLab에 제출하고 provider 결과 참조를 기록합니다.
다운로드·export·추출·패키징·평가는 수행하지 않습니다.

## Prompt

```text
현재 ProjectBS 저장소에서 PixelLab 캐릭터 provider 생성 요청 하나만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- promptRecordId: {generated_media_prompt_v2_or_validated_legacy_v1_record_id}
- runType: {character_main_image | character_animation}
- animationRequestId: {required_only_for_character_animation}

작업:
1. prompt schema를 확인한다. 신규 v2이면 visualBriefId/hash/evidence coverage까지 검증하고, 기존 v1이면 GeneratedMediaRecordGuide.md의 read-only compatibility gate를 충족해야 한다. planning snapshot, prompt identity/profile/payload/hash와 runType을 검증한다.
2. main은 eight-way 계약을, animation은 지정된 한 animationRequest와 character provider identity를 검증한다.
3. stale/mismatch/누락이면 provider를 호출하지 않고 blocker를 반환한다.
4. 저장된 provider field text를 바꾸지 않고 정확한 PixelLab Character tool/settings에 제출한다.
5. animationRequests에 있는 해당 요청만 실행하며 Attack/Idle/Move 고정 세트를 만들지 않는다.
6. settings, cost evidence, attempts와 모든 provider result refs를 generated_media_generation_v1에 기록한다.
7. deterministic provider-operation rule이 있으면 provisional ref만 기록하고 평가하지 않는다.
8. preservationHandoff에 adapter/profile/required refs를 기록하고 종료한다.
9. 다운로드, export, 파일 저장, frame/rotation 추출, hash, package seal, 평가, 승격, Slack, Unity, Git, 배포를 수행하지 않는다.

Output:
- Request / Run / Character / Animation Request Identity
- Generation Record ID / Status
- Provider Tool / Settings / Cost / Attempts / Result Refs
- Provisional Selection Status
- Preservation Handoff / Next Task: preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: prompt_record_missing | prompt_schema_version_unsupported | prompt_record_stale | provider_prompt_hash_mismatch | visual_brief_identity_mismatch | visual_brief_hash_mismatch | visual_evidence_map_incomplete | character_provider_identity_missing | animation_request_not_in_handoff | pixellab_unavailable | provider_operation_failed | provider_result_missing | generation_record_write_failed
- Provider 호출·비용 여부 / 보존된 provider refs / Required Next Action

검증:
- generation record에 로컬 파일, member hash, package ID, 평가 상태가 없어야 한다.
- provider result ref만으로 후속 task가 독립 재시도 가능해야 한다.
- 요청되지 않은 animation이나 후속 단계를 실행하지 않아야 한다.
```
