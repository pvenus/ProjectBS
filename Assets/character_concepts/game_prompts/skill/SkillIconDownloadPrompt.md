# Skill Icon Download and Evaluation Prompt

PixelLab 주 피사체와 선택적 편집 결과를 기존 평가 폴더에 보존하고, 정확한 개수 오버레이와 프레임 정규화를 거친 최종 파일만 평가·Unity 복사하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

현재 PC의 기존 경로 체계를 확인한 뒤 아래 가이드에 따라 아이콘 생성 단계별 증거를 보존하고, 정규화된 최종 source가 평가를 통과할 때만 Unity 경로에 복사해줘.

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
- pixelLabPrimaryResult: {create_ui_pro 결과 페이지 또는 16개 개별 변형 로컬 파일}
- pixelLabSemanticEditResult: {Edit image 결과 페이지 또는 로컬 파일 | null}
- evaluationRoot: /Users/pvenus/Documents/PixelLab/skill/icon
- frameTemplatePath: {현재 PC에 존재하는 승인된 80×80 프레임·배경 템플릿 절대경로}
- backgroundMode: {flat | contextual}
- backgroundDescription: {omitted | 낮은 대비 배경 요소 1-2개}
- exactCountOverlayManifest: {요소 종류·개수·크기·색상·위치 기록 | null}
- normalizationRecord: {safe-area·background·frame 적용 기록}
- lowerGradeIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- siblingIconPaths: {auto | 같은 로드아웃 아이콘 절대경로 목록 | null}
- unityIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- allowReplaceExistingIcon: false

작업:
1. 현재 PC에서 projectRoot, evaluationRoot, unityIconPath가 기존 문서·파일의 경로 체계와 일치하는지 확인한다.
2. 다른 PC에서 전달된 절대 경로를 사용하거나 새 보존 폴더 규칙을 만들지 않는다.
3. skillSourcePath와 equipmentId를 검증한다.
4. frameTemplatePath가 실제로 존재하며 80×80인지 확인한다. 없으면 placeholder를 만들지 않고 중단한다.
5. primary 결과가 `Create UI elements (Pro)`, Custom size 80×80, Concept Image 없이 생성됐는지 확인한다. `flat`은 Transparent background On, `contextual`은 Off여야 한다.
6. 한 번의 Pro 실행에서 나온 4×4, 총 16개 변형을 각각 80×80 개별 파일로 내려받아 기존 `{evaluationRoot}/{equipmentId}/candidates` 폴더에 `candidate_XX.primary.png`로 보존한다. 합쳐진 그리드는 후보로 사용하지 않는다.
7. 16개 변형을 저비용 검사한 뒤 상위 3개까지만 후속 처리한다.
8. semantic edit가 있으면 `Edit image`와 한 문장 add/remove/change/replace 지시를 확인하고 `candidate_XX.edited.png`로 보존한다.
9. exactCountOverlayManifest에 따라 sparks, chips, threads, chevrons를 결정적으로 추가한다. 각 요소는 최소 4×4px, 간격은 최소 4px이다.
10. frameTemplatePath를 적용한다. `flat`은 템플릿 단색 내부 배경을 사용하고, `contextual`은 생성 배경을 x=2..77, y=2..77 안에 보존한다. primary와 효과는 중앙 64×64 영역으로 제한한다.
11. rows/columns 0, 1, 78, 79를 템플릿 픽셀로 복원해 exact outer 2px frame을 만든다.
12. 9-11의 결과를 `candidate_XX.normalized.png`로 보존한다. 이 단계는 resize/crop이 아니라 deterministic overlay/normalization으로 기록한다.
13. normalized 결과를 nearest-neighbor 32×32로 표시한 증거를 `candidate_XX.preview32.png`로 기존 candidates 폴더에 보존한다.
14. primary 40-52px, 의미 선 4px 이상, 요소 간격 4-6px, particles 4×4px 이상, arcs/rings 3-4px를 검사한다.
15. raw primary가 아니라 normalized 후보를 SkillIconEvaluationGuide.md 기준으로 평가한다.
16. 모든 단계의 SHA-256, 4개 핵심 문장과 선택적 배경 문장, backgroundMode, `referenceMode=none`, compositionProfile, 16개 변형 목록, edit instruction, overlay manifest, normalization 기록과 점수를 generation_record.txt와 candidate_scores.txt에 저장한다.
17. 치명적 실패가 없고 85점 이상인 normalized 후보만 `{evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png`에 보존한다.
18. source 보존 이후에는 리사이즈, 크롭, 색상 변경, 재압축을 수행하지 않는다.
19. 최종 평가를 기존 `{evaluationRoot}/{equipmentId}/evaluation/evaluation_result.txt`에 저장한다.
20. Pass이고 교체 조건이 충족된 경우에만 source를 unityIconPath로 복사한다.
21. 보존 source와 Unity PNG의 SHA-256이 정확히 같은지 확인하고 `.meta`를 기존 import 정책으로 처리한다.
22. Unity Editor reimport를 확인할 수 없으면 `meta configured / Unity reimport pending`으로 보고한다.
23. 기존 candidates, source, evaluation, generation_record.txt, candidate_scores.txt 구조를 유지하고 새 중간 폴더를 만들지 않는다.

Output:
- Skill ID / Source JSON
- Primary / Edited / Normalized / Preview32 Paths
- Reference Mode: none
- Composition Profile
- Background Mode / Description
- Pro Grid / Variation Count
- Core Outline / Direction / Simple Skill Effect / Compact Exclusion·Grade Sentences
- Frame Template Path
- Exact-Count Overlay Manifest
- Normalization Record
- Candidate Technical Validation
- Candidate Scores Path
- Preserved Source Path / SHA-256
- Evaluation Result / Score
- Pass / Conditional Pass / Fail
- Unity Icon / Meta Path
- Checksum Match
- Unity Import Status
- Cleanup Status

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
  - missing_frame_template
  - semantic_edit_failed
  - overlay_failed
  - normalization_failed
  - no_passing_candidate
  - evaluation_write_failed
  - existing_icon_requires_approval
  - unity_copy_failed
  - checksum_mismatch
  - unity_meta_failed
  - unity_import_pending
- 실패 원인
- 보존한 증거
- 생성하지 않은 파일 또는 폴더
- 다음에 필요한 작업

주의:
- PixelLab 생성·재생성은 이 프롬프트에서 수행하지 않는다.
- raw primary나 edited 후보를 Unity에 직접 복사하지 않는다.
- 다른 PC의 경로나 임의의 새 템플릿 폴더를 사용하지 않는다.
- 실패한 아이콘을 정규화만으로 Pass 처리하지 않는다.
```
