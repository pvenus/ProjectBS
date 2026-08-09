# Skill Icon Download and Preservation Prompt

완료된 PixelLab 스킬 아이콘 결과를 다운로드하고 평가용 원본으로 보존하는
단계만 실행합니다.

## Prompt

```text
현재 ProjectBS 저장소와 기존 로컬 평가 workspace를 확인하고 스킬 아이콘 provider 결과의 download/preserve 단계만 실행해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/skill/SkillIconDownloadGuide.md

Input:
- requestId: {optional_stable_request_id}
- equipmentId: {canonical_equipment_skill_id}
- generationRecordId: {generated_image_generation_v1_record_id}
- generationRecordPath: {accessible_generation_record_path_or_stable_reference}
- evaluationRoot: {current_pc_skill_icon_evaluation_root}
- selectedProviderResultRef: {provider_result_ref_or_provisional_ref}
- replacePreservedSource: {false | true_with_explicit_approval}

작업:
1. generation record의 artifactType=skill_icon, equipmentId, PixelLab result refs와 expected download role을 검증한다.
2. 현재 PC의 기존 evaluationRoot를 확인하고 새 임의 루트를 만들지 않는다.
3. 선택 result의 원본 PNG를 다운로드하고 preview/thumbnail을 source로 사용하지 않는다.
4. `{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png`에 원본 bytes를 보존한다.
5. dimensions, mode, source SHA-256, provider ref와 download record를 작성한다.
6. frame normalization, overlay, 32×32 평가, 점수와 프로젝트 복사를 수행하지 않는다.

Output:
- Equipment ID / Generation Record ID
- Provider Result Reference
- Preserved Source Path / Dimensions / SHA-256
- Download Record ID / Path / SHA-256
- Download Status
- Evaluation Handoff

실패 시 Output:
- status: blocked | failed
- failureType: generation_record_not_found | provider_result_not_found | invalid_png | existing_source_requires_approval | download_failed | checksum_failed | download_record_write_failed
- 보존한 기존 source
- Required Next Action

검증:
- source는 provider 원본 bytes여야 한다.
- 평가 결과나 PASS를 생성하지 않아야 한다.
- normalization·Assets/ImagesGenerated 복사·Unity `.meta`·Slack·Git 작업을 수행하지 않아야 한다.
```
