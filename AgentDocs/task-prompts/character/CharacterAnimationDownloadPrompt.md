# Character Animation Download and Preservation Prompt

완료된 PixelLab 캐릭터 애니메이션을 현재 PC의 기존 평가 workspace에
다운로드하고 원본을 보존하는 단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소와 기존 로컬 평가 workspace를 확인하고 캐릭터 애니메이션 provider 결과의 download/preserve 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/character/CharacterAnimationDownloadGuide.md

Input:
- requestId: {optional_stable_request_id}
- characterId: {canonical_character_id}
- generationRecordId: {generated_image_generation_v1_record_id}
- generationRecordPath: {accessible_generation_record_path_or_stable_reference}
- evaluationRoot: {current_pc_character_animation_evaluation_root}
- replacePreservedSource: {false | true_with_explicit_approval}

작업:
1. generation record의 artifactType=character_animation, characterId, provider result refs와 expected output roles를 검증한다.
2. 현재 PC에서 evaluationRoot를 확인하고 다른 PC의 기록 경로를 재사용하지 않는다.
3. 정확한 PixelLab result에서 source animation files를 다운로드한다.
4. CharacterAnimationDownloadGuide.md의 current download adapter에 따라 원본 구조와 이름을 변경하지 않고 preserved source에 저장한다.
5. source file hash, provider result ref, download time, expected/observed file manifest와 download record를 작성한다.
6. GIF evidence, converted 파일, 평가 점수, 프로젝트 복사와 Unity `.meta`를 생성하지 않는다.

Output:
- Character ID / Generation Record ID
- Provider Result References
- Preserved Source Root / File Manifest / SHA-256
- Download Record ID / Path / SHA-256
- Download Status
- Evaluation Handoff

실패 시 Output:
- status: blocked | failed
- failureType: generation_record_not_found | provider_result_not_found | invalid_animation_folder | existing_source_requires_approval | download_failed | checksum_failed | download_record_write_failed
- 보존한 기존 source
- 생성하지 않은 후속 결과
- Required Next Action

검증:
- provider result와 generation record identity가 일치해야 한다.
- preserved source는 evaluation workspace 아래에 있어야 한다.
- 평가·변환·프로젝트 승격·Unity·Slack·Git 작업을 수행하지 않아야 한다.
```
