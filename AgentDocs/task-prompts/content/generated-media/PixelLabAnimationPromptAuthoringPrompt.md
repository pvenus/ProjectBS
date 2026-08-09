# PixelLab General Animation Prompt Authoring Prompt

캐릭터 독립형 일반 animation/VFX의 PixelLab prompt record만 작성합니다.

## Prompt

```text
현재 ProjectBS 저장소에서 외부 일반 애니메이션 기획 handoff 하나를 검증하고 PixelLab reference image와 animation action용 provider-ready prompt record만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v1_path}
- required handoff fields: domainType, animationProfile, sourcePlanningFiles, planningSnapshot, requiredElements, prohibitedElements, sequenceStages, loopMode, frameContract, runtimeBoundary, referenceImageContract
- priorPromptRecordId: {optional_revision_record_id}
- revisionReason: {required_when_revising}

작업:
1. assetType=general_animation, domainType, animationProfile, planning snapshot과 source hashes를 검증한다.
2. requiredElements, prohibitedElements, animationSubject, ordered sequenceStages, loopMode, frameContract, runtimeBoundary와 referenceImageContract를 모두 요구한다.
3. 누락된 시작·전개·종료 단계, loop, frame 또는 runtime motion을 추론하지 않는다.
4. pixellab_animation_prompt_v1의 reference_image_description과 animation_action을 서로 분리해 실제 PixelLab field용 간결한 영어 textOriginal로 작성한다.
5. runtimeOwnedMotion은 provider prompt에서 제외하고 generatedMotion만 표현한다.
6. source facts/constraints, settings intent와 payload hash를 기록한다.
7. generated_media_prompt_v1 JSON/Markdown을 AgentDocs/planning-data/generated-media-prompts/v1/general_animation/{contentId}/에 새 immutable ID로 저장하고 index를 갱신한다.
8. PixelLab 실행, media 생성·다운로드·추출·평가·승격, Unity, Slack, Git과 배포를 수행하지 않는다.

Output:
- Request / Asset / Domain / Content Identity
- Animation Profile / Sequence Summary / Planning Snapshot Hash
- Prompt Record ID / Paths / SHA-256
- Provider Prompt Profile / Field Roles / Payload Hash / Settings Intent
- Status / Next Task

실패 시 Output:
- status: blocked | failed
- failureType: invalid_planning_handoff | missing_required_elements | missing_prohibited_elements | missing_animation_profile | missing_sequence_specification | invalid_loop_mode | missing_frame_contract | missing_runtime_boundary | missing_reference_contract | prompt_record_write_failed
- 누락 근거 / 생성하지 않은 record / Required Next Action

검증:
- 캐릭터 동작이나 gameplay 효과를 기획하지 않아야 한다.
- 외부 sequence와 runtime boundary만 prompt로 변환해야 한다.
- JSON/Markdown/hash가 일치하고 provider를 실행하지 않아야 한다.
```
