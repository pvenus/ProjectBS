# Skill Animation Download and Preservation Prompt

완료된 PixelLab 스킬 애니메이션 결과를 다운로드하고 평가용 원본으로
보존하는 단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소와 기존 로컬 평가 workspace를 확인하고 스킬 애니메이션 provider 결과의 download/preserve 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/skill/SkillImageDownloadGuide.md

Input:
- requestId: {optional_stable_request_id}
- skillId: {canonical_equipment_skill_id}
- generationRecordId: {generated_image_generation_v1_record_id}
- generationRecordPath: {accessible_generation_record_path_or_stable_reference}
- evaluationRoot: {current_pc_skill_animation_evaluation_root}
- replacePreservedSource: {false | true_with_explicit_approval}

작업:
1. generation record의 artifactType=skill_animation, skillId, reference/animation result refs와 expected roles를 검증한다.
2. 현재 PC의 기존 evaluationRoot를 확인한다.
3. provider 원본 reference PNG와 animation sheet를 각각 다운로드한다.
4. `{evaluationRoot}/{skillId}/source/` 아래에서 서로 다른 안정 파일명으로 원본 bytes를 보존한다.
5. 두 파일의 dimensions, sheet manifest, SHA-256, provider refs와 download record를 작성한다.
6. frame 추출, GIF, 평가, Unity slice/meta/clip과 프로젝트 복사를 수행하지 않는다.

Output:
- Skill ID / Generation Record ID
- Reference and Animation Provider Result References
- Preserved Source Paths / Sheet Manifest / SHA-256
- Download Record ID / Path / SHA-256
- Download Status
- Evaluation Handoff

실패 시 Output:
- status: blocked | failed
- failureType: generation_record_not_found | provider_result_not_found | invalid_reference_image | invalid_animation_sheet | existing_source_requires_approval | download_failed | checksum_failed | download_record_write_failed
- 보존한 기존 source
- Required Next Action

검증:
- reference와 animation 원본을 서로 덮어쓰지 않아야 한다.
- preview나 GIF를 source로 사용하지 않아야 한다.
- 평가·프레임 가공·Assets/ImagesGenerated 복사·Unity·Slack·Git 작업을 수행하지 않아야 한다.
```
