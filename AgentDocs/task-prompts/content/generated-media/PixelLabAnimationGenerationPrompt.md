# PixelLab General Animation Generation Prompt

일반 animation/VFX의 reference와 sheet를 provider에서 생성하고 결과 참조를
기록합니다. 다운로드·추출·패키징·평가는 수행하지 않습니다.

## Prompt

```text
현재 ProjectBS 저장소에서 PixelLab 일반 애니메이션 provider 생성 요청 하나만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- promptRecordId: {generated_media_prompt_v1_record_id}

작업:
1. assetType=general_animation과 sequence/loop/frame/runtime/reference 계약 및 prompt hash를 검증한다.
2. 누락이나 stale prompt이면 provider를 호출하지 않는다.
3. 저장된 reference_image_description으로 reference 결과를 생성하고 refs/settings/attempt를 기록한다.
4. 저장된 animation_action과 지정 tool/settings로 animation sheet 결과를 생성한다.
5. 두 작업의 cost evidence, attempts와 모든 result refs를 generation record에 기록한다.
6. pixellab_general_animation_sheet_v1/paired_sheet_animation preservation handoff를 기록한다.
7. 다운로드, sheet 검사/수정, frame 추출, hash, package seal, 평가, 승격, Slack, Unity, Git, 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content / Animation Profile
- Generation Record ID / Status
- Reference/Animation Settings / Cost / Attempts / Result Refs
- Preservation Handoff / Next Task: preservation_packaging

실패 시 Output:
- status: blocked | failed
- failureType: missing_sequence_specification | missing_runtime_boundary | prompt_record_missing | prompt_record_stale | pixellab_unavailable | reference_generation_failed | animation_generation_failed | provider_result_missing | generation_record_write_failed
- Provider 호출·비용 여부 / 보존된 provider refs / Required Next Action

검증:
- 외부 sequence/runtime boundary만 provider에 제출해야 한다.
- generation record에는 원본/추출 파일이나 package 정보가 없어야 한다.
- 후속 단계가 provider refs로 독립 재시도 가능해야 한다.
```
