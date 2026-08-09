# Battle Background Provider Generation Prompt

> Deprecated compatibility entry. Replaced by
> `AgentDocs/task-prompts/content/generated-media/ImageGenPromptAuthoringPrompt.md`
> and `AgentDocs/task-prompts/content/generated-media/ImageGenGenerationPrompt.md`.

전투 배경의 provider 생성 단계만 실행하는 copy-ready 프롬프트입니다.
프롬프트 작성, 다운로드, 평가, 프로젝트 승격과 Unity 작업은 수행하지 않습니다.

## Prompt

```text
현재 ProjectBS 저장소에서 전투 배경 하나의 provider generation 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
- AgentDocs/planning-guides/battle/BattleCreateGuide.md

Input:
- requestId: {optional_stable_request_id}
- battleId: {canonical_battle_id}
- promptRecordId: {generated_image_prompt_v1_record_id}
- contentSummary: {optional_battle_meaning}
- visualIntent: {optional_background_visual_intent}

작업:
1. artifactType=battle_background, contentId=battleId로 고정한다.
2. BattleCreateGuide.md의 current generation adapter와 GeneratedImageGenerationPipelineGuide.md를 적용한다.
3. promptRecordId의 providerPromptProfile, contentSnapshotHash, domain adapter와 ImageGen route가 현재 근거와 일치하는지 검증한다.
4. 검증된 scenePromptOriginal을 수정·분해하지 않고 ImageGen에 한 번의 provider-native prompt로 제출한다.
5. provider result reference, 실제 설정, attempt, 비용 근거와 generation record를 보존한다.
6. 결과를 다운로드하거나 평가·수정·프로젝트 복사하지 않는다.

Output:
- Artifact Type / Battle ID
- Prompt Record ID / Hash / Provider Prompt Profile
- Provider / Settings / Attempt Count
- Provider Result References
- Generation Record ID / Path / SHA-256
- Generation Status
- Download Handoff

실패 시 Output:
- status: blocked | failed
- failureType: prompt_record_not_found | prompt_record_stale | provider_prompt_profile_mismatch | provider_unavailable | provider_operation_failed | generation_record_write_failed
- Provider 호출 및 비용 발생 여부
- 생성하지 않은 generation record
- Required Next Action

검증:
- ImageGen 외 provider를 사용하지 않아야 한다.
- 검증된 prompt record 원문을 다시 작성하지 않아야 한다.
- provider result와 generation record까지만 생성해야 한다.
- 다운로드·평가·Assets/ImagesGenerated 복사·Unity·Slack·Git 작업을 수행하지 않아야 한다.
```
