# Skill Icon Download and Evaluation Prompt

PixelLab에서 생성한 정적 스킬 아이콘 후보를 평가 폴더에 보존하고, 평가를 통과한 파일만 Unity 경로에 복사하여 ID·체크섬·import 상태를 검증하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 PixelLab 정적 스킬 아이콘 결과를 다운로드하고, 별도 평가 폴더에 보존·평가한 뒤 통과한 파일만 Unity 경로에 복사해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconDownloadGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md

Input:
- projectRoot: {project_root}
- skillSourcePath: {스킬_JSON_절대경로}
- equipmentId: {skill.character.character_name.grade.slot.skill_name}
- pixelLabResult: {background_job_id | PixelLab 결과 페이지 | 로컬 다운로드 폴더 절대경로}
- evaluationRoot: /Users/pvenus/Documents/PixelLab/skill/icon
- styleReferencePaths: {auto | 쉼표로 구분한 승인 아이콘 절대경로 목록}
- lowerGradeIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- siblingIconPaths: {auto | 같은 로드아웃 아이콘 절대경로 목록 | null}
- unityIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- allowReplaceExistingIcon: false

작업:
1. skillSourcePath가 존재하고 유효한 JSON인지 확인한다.
2. JSON의 equipmentId가 입력 equipmentId와 정확히 일치하는지 확인한다.
3. equipmentId를 `.` 기준으로 파싱하여 domain, characterName, grade, slot, skillName을 확정한다.
4. pixelLabResult가 대상 equipmentId의 완료된 PixelLab 생성 결과인지 확인한다.
5. PixelLab 인증값은 환경 변수 또는 secret 설정에서만 읽고 Input, Output, 로그, 기록 파일에 노출하지 않는다.
6. 모든 후보 PNG를 임시 폴더에 다운로드하고 archive 또는 grid이면 후보별 PNG로 분리한다.
7. 각 후보의 PNG 디코딩, 80×80 크기, RGBA, 단일 정적 아이콘 여부, 배경, 테두리, 텍스트·워터마크 부재를 확인한다.
8. 기술 검증을 통과한 모든 후보를 SkillIconEvaluationGuide.md 기준으로 평가한다.
9. 각 후보 점수와 실패 사유를 `{evaluationRoot}/{equipmentId}/candidate_scores.txt`에 저장한다.
10. 치명적 실패가 없고 85점 이상인 후보 중 최고 점수를 선택한다.
11. 합격 후보가 없으면 후보 증거와 평가 결과를 보존하고 Unity 복사 없이 실패 처리한다.
12. 선택 후보를 수정하지 않고 `{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png`에 복사한다.
13. PixelLab endpoint, job/page, description, style_description, 참조 경로, seed, 후보 수, 선택 후보, SHA-256을 `{evaluationRoot}/{equipmentId}/generation_record.txt`에 저장한다.
14. 보존된 source를 80×80과 nearest-neighbor 32×32에서 다시 확인하고 SkillIconEvaluationGuide.md 형식으로 최종 평가한다.
15. 최종 평가를 `{evaluationRoot}/{equipmentId}/evaluation/evaluation_result.txt`에 저장한다.
16. 최종 평가가 Pass가 아니면 Unity 복사를 수행하지 않는다. Conditional Pass는 명시적 승인 전까지 중단한다.
17. allowReplaceExistingIcon이 false이고 unityIconPath에 기존 파일이 있으면 덮어쓰지 않고 승인 필요 상태로 중단한다.
18. Pass이고 교체 조건이 충족되면 보존된 source를 unityIconPath에 복사한다.
19. 보존 source와 Unity PNG의 SHA-256이 정확히 일치하는지 확인한다.
20. Unity filename이 전체 `{equipmentId}.icon.png` 규칙과 정확히 일치하는지 확인한다.
21. 같은 경로에 `{equipmentId}.icon.png.meta`를 생성 또는 갱신하고 기존 승인 아이콘의 import 정책을 따른다.
22. Unity import는 Sprite (2D and UI), Sprite Mode Single, Point, no mipmap, alpha transparency, 기본 platform compression None으로 설정한다.
23. 다른 아이콘의 GUID를 재사용하지 않고, EquipmentSkillSO 또는 runtime이 예상 icon resource key를 해석할 수 있는지 확인한다.
24. 올바른 Unity Editor에서 reimport를 확인할 수 없으면 `meta configured / Unity reimport pending`으로 보고한다.
25. 평가 폴더의 source, generation_record.txt, candidate_scores.txt, evaluation_result.txt와 필요한 후보 증거는 유지한다.
26. 보존, 평가, Unity 복사, 체크섬, meta 처리가 완료된 뒤 archive, 임시 추출 폴더, 중복 다운로드, 무관한 preview만 삭제한다.

Output:
- Skill ID
- Source JSON
- PixelLab Result
- Candidate Download Paths
- Candidate Count
- Candidate Technical Validation
- Candidate Scores Path
- Selected Candidate
- Preserved Source Path
- Generation Record Path
- Evaluation Result / Result Path
- Evaluation Score
- Pass / Conditional Pass / Fail
- Preserved Source SHA-256
- Unity Icon Path
- Unity Icon SHA-256
- Checksum Match
- Unity Meta Path
- Unity Import Status
- Resolved Icon Resource Key
- Replacement Status
- Cleanup Status
- Missing Items

실패 시 Output:
- status: failed
- failureType:
  - missing_skill_json
  - invalid_skill_json
  - equipment_id_mismatch
  - pixellab_result_mismatch
  - missing_download
  - invalid_png
  - invalid_icon_size
  - no_passing_candidate
  - evaluation_write_failed
  - existing_icon_requires_approval
  - unity_copy_failed
  - checksum_mismatch
  - unity_meta_failed
  - unity_import_pending
  - unresolved_icon_resource_key
- 실패 원인
- 보존한 파일
- 평가 결과 경로
- 생성하지 않거나 복사하지 않은 Unity 파일
- cleanup 상태
- 다음에 필요한 작업

검증:
- 평가 source는 `{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png`에 존재해야 한다.
- 평가 결과는 `{evaluationRoot}/{equipmentId}/evaluation/evaluation_result.txt`에 존재해야 한다.
- Unity filename은 전체 `{equipmentId}.icon.png`를 사용해야 한다.
- Unity 경로는 `Assets/Resources/skill/icon/skill/{equipmentId}.icon.png`와 정확히 일치해야 한다.
- 최종 평가가 Pass일 때만 Unity 복사가 수행되어야 한다.
- 보존 source와 Unity PNG는 바이트가 동일하고 SHA-256이 일치해야 한다.
- 최종 PNG는 80×80 RGBA 단일 아이콘이어야 한다.
- `.png.meta`는 Sprite Single과 프로젝트 import 정책을 따라야 하며 고유 GUID를 사용해야 한다.
- 명시적 승인 없이 기존 합격 아이콘을 덮어쓰지 않아야 한다.
- PixelLab API token이 어떤 출력이나 기록에도 포함되지 않아야 한다.
- 평가 증거는 임시 파일 정리 후에도 남아 있어야 한다.

주의:
- 이 프롬프트는 다운로드, 보존, 평가, Unity 복사를 하나의 후처리 작업으로 수행한다.
- PixelLab 이미지 생성 또는 재생성은 수행하지 않는다.
- 평가 폴더를 Unity 폴더로 대체하지 않는다.
- 후보, contact sheet, archive를 Unity 경로에 직접 복사하지 않는다.
- Conditional Pass 또는 Fail 아이콘을 Unity에 복사하지 않는다.
- PNG 복사만으로 Unity 반영 완료로 보고하지 않는다.
```
