# Skill Icon Evaluation Prompt

정규화된 정적 스킬 아이콘을 의미·방향·부분 물체 구조·정확한 개수·32×32 생존성·Unity 준비 상태 기준으로 평가하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 최종 normalized 스킬 아이콘을 평가해줘. 평가 중 이미지, JSON, `.meta` 파일을 생성하거나 수정하지 마.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md

Input:
- projectRoot: {project_root}
- evaluationRoot: /Users/pvenus/Documents/PixelLab/skill/icon
- skillSourcePath: {스킬_JSON_절대경로}
- equipmentId: {skill.character.character_name.grade.slot.skill_name}
- iconPath: {evaluationRoot}/{equipmentId}/source/{equipmentId}.icon.png
- intendedUnityIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- generationRecordPath: {아이콘 생성 기록 절대경로}
- preview32Path: {nearest-neighbor 32×32 미리보기 절대경로}
- lowerGradeIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- siblingIconPaths: {auto | 같은 로드아웃 아이콘 절대경로 목록 | null}
- unityMetaPath: {auto | iconPath.meta | null}
- evaluationOutputPath: {평가_결과_txt_절대경로 | report_only}

작업:
1. 현재 PC의 projectRoot와 입력 경로가 기존 기록의 경로 체계를 따르는지 확인한다. 다른 PC 절대 경로가 기록에 남아 있으면 Pass 처리하지 않는다.
2. skillSourcePath, equipmentId, 보존 source iconPath와 80×80 RGBA 단일 PNG 계약을 확인한다.
3. generationRecordPath에서 Create UI elements (Pro), 4×4/16개 변형, referenceMode, compositionProfile, backgroundMode, 선택적 backgroundDescription, core outline·direction·simple effect·compact exclusion/grade 문장, frameTemplatePath, semantic edit, exact-count overlay, normalization, preview32 증거를 읽는다.
4. `create_ui_pro`, Custom size 80×80, Concept Image 비어 있음, 16개 변형 보존 또는 제외 사유를 확인한다. frameTemplatePath는 현재 PC에 실제로 존재하고 80×80이어야 한다.
5. source JSON으로 예상 direction, composition, primary fragment shape, mandatorySemanticEffect, exactCountElements를 다시 분류한다.
6. 실제 아이콘 방향이 compositionProfile 및 source와 일치하는지 평가한다. 무기나 화살이 근거 없이 좌하단→우상단으로 수렴하면 Fail 사유로 기록한다.
7. 부분 물체가 완전한 머리, 인물, 제단, 생물로 복원되지 않았는지 확인한다.
8. exactCountElements가 overlay manifest와 정확히 일치하는지 확인한다.
9. rows/columns 0, 1, 78, 79가 frameTemplatePath와 픽셀 단위로 일치하는지 확인한다.
10. `flat`은 승인된 단색 템플릿 배경인지 확인한다. `contextual`은 기록된 배경 요소 1-2개만 x=2..77, y=2..77 안에 존재하고 핵심보다 대비가 낮은지 확인한다. primary와 효과는 중앙 64×64 영역을 벗어나면 안 된다.
11. primary가 약 40-52px인지, 의미 선이 4px 이상인지, 요소 간격이 4-6px인지 검사한다.
12. sparks/chips는 각 4×4px 이상인지, arcs/rings는 3-4px 두께인지 검사한다.
13. preview32Path를 확인하여 primarySymbol, 방향, 필수 의미 효과가 모두 남는지 평가한다.
14. Concise Outline Prompt Contract의 핵심 4문장과 contextual일 때만 허용되는 배경 1문장을 확인한다. frame/card/panel/좌표/exact count를 반복 위임했거나 불필요한 장면 묘사를 누적했으면 감점한다.
15. 하위 등급과 sibling 비교, SHA-256 중복 검사를 수행한다.
16. 치명적 실패를 먼저 판정한 뒤 6개 항목을 100점 만점으로 채점한다.
17. 실패마다 다음 수정 방법 중 하나를 지정한다: core_outline_rewrite, direction_sentence_replace, shape_only_rewrite, semantic_edit, exact_count_overlay, deterministic_normalization, small_size_recompose.
18. 같은 prompt_only 문구를 길게 늘리는 수정을 제안하지 않는다.
19. 85점 이상이고 치명적 실패와 필수 증거 부족이 없을 때만 Pass 처리한다.
20. evaluationOutputPath가 report_only가 아니면 평가 리포트만 저장한다.

Output:
- Skill Icon Evaluation
- Skill ID / Source JSON / Preserved Icon Path / Intended Unity Icon Path
- Canvas / SHA-256
- Generation Record Path
- Reference Mode / Composition Profile
- Background Mode / Description / Contrast Result
- Pro Grid / 16 Variation Evidence
- Core Outline / Direction / Simple Skill Effect / Compact Exclusion·Grade Sentences
- Frame Template Path
- Semantic Edit Evidence
- Exact-Count Overlay Manifest
- Normalization Record
- Preview32 Path / Survival Result
- Expected / Actual Direction
- Fragment Structure Result
- Fatal Failure Check
- Skill Intent Readability: /25
- Project Style Match: /20
- Small-Size Silhouette: /20
- Slot and Grade Distinction: /15
- Palette and Contrast: /10
- Composition and Border Quality: /10
- Total: /100
- Result: Pass / Conditional Pass / Fail
- Required Correction Method
- Minimal Prompt or Pipeline Change
- Unity Import Status

실패 시 Output:
- status: failed
- failureType:
  - missing_skill_json
  - invalid_skill_json
  - equipment_id_mismatch
  - missing_icon
  - invalid_png
  - unsupported_slot
  - missing_generation_record
  - missing_frame_template
  - missing_normalization_record
  - missing_preview32
  - insufficient_evidence
  - evaluation_write_failed
- 평가하지 못한 항목
- 실패 원인
- 부족한 입력
- 다음에 필요한 작업

주의:
- 평가는 normalized final source를 대상으로 한다.
- raw primary 또는 edited 후보를 최종 아이콘처럼 평가하지 않는다.
- 평가 중 이미지를 보정하거나 재생성하지 않는다.
- 확인할 수 없는 항목을 추정으로 Pass 처리하지 않는다.
```
