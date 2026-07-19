# Skill Icon Generation Prompt

스킬 JSON을 기준으로 PixelLab에서 80×80 정적 스킬 아이콘의 주 실루엣을 생성하고, 필요한 효과와 정확한 개수 요소를 분리 처리한 뒤 기존 템플릿으로 최종 규격을 보정하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 지정한 스킬의 80×80 정적 아이콘을 생성해줘. 주 피사체 방향은 기존 실루엣 Init Image로 제어하고, 의미 효과·정확한 개수 요소·프레임 보정을 서로 다른 단계로 처리해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconDownloadGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md

Input:
- projectRoot: {project_root}
- skillSourcePath: {스킬_JSON_절대경로}
- equipmentId: {skill.character.character_name.grade.slot.skill_name}
- evaluationRoot: /Users/pvenus/Documents/PixelLab/skill/icon
- pixelLabCreatorUrl: https://www.pixellab.ai/create?tool=create_ui_basic
- primaryTool: Create UI elements
- semanticEffectTool: Edit image
- referenceMode: silhouette_init
- silhouetteFamily: {auto | horizontal_projectile | descending_projectile | diagonal_melee | centered_radial_active | centered_passive_emblem}
- silhouetteInitImagePath: {auto | 현재 PC에 이미 존재하는 80×80 구조 참조 이미지 절대경로}
- frameTemplatePath: {현재 PC에 이미 존재하는 승인된 80×80 프레임·배경 템플릿 절대경로}
- inheritedIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- outputIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- finalIconSize: 80x80
- primaryDisplaySize: 40-52px
- maxPrimaryGenerationCount: 2
- maxSemanticEditCount: 1

사전 확인:
1. 현재 PC에서 projectRoot, evaluationRoot, outputIconPath가 기존 문서·기록의 경로 체계와 일치하는지 확인한다.
2. 다른 PC에서 전달된 절대 경로를 사용하지 않는다.
3. 템플릿 또는 실루엣용 새 폴더 구조를 임의로 만들지 않는다.
4. frameTemplatePath가 존재하고 정확히 80×80 RGBA인지 확인한다. 없으면 생성하지 말고 `missing_frame_template`로 중단한다.
5. silhouetteInitImagePath가 auto이면 기존 파일 중 분류에 맞는 80×80 구조 참조를 찾는다. 없으면 임의 이미지나 기존 완성 아이콘으로 대체하지 말고 `missing_silhouette_init_image`로 중단한다.

작업:
1. skillSourcePath와 equipmentId를 검증하고 grade, slot, skillName을 확정한다.
2. 스킬 JSON에서 targeting, castMove, componentType, moveType, damage, buff, debuff, effect를 읽는다.
3. slotFamily, primarySymbol, direction, composition, mandatorySemanticEffect, exactCountElements, prohibitedObjects, gradeIntensity, palette를 분류한다.
4. 방향과 구도에 따라 silhouetteFamily를 하나만 선택한다.
5. 하위 등급 아이콘은 계승 정체성을 분석하는 용도로만 읽는다. 스타일 레퍼런스나 Init Image로 사용하지 않는다.
6. 가이드의 Five-Sentence Prompt Contract에 따라 주 피사체 생성용 영어 Description을 최대 5개 핵심 문장으로 작성한다.
7. 생성 Description에서 프레임, 카드, 패널, 배경 테두리, 픽셀 좌표, 정확한 작은 효과 개수는 제거한다.
8. 부분 물체는 의미 명칭보다 시각 형상을 먼저 기술한다. 예: `wolf jaw` 대신 `two disconnected dark-gray crescent jaw strips`.
9. `pixelLabCreatorUrl`을 열고 non-Pro `Create UI elements`가 선택됐는지 확인한다.
10. Width와 Height를 각각 80px로 설정하고 Transparent background를 켠다.
11. silhouetteInitImagePath를 Init Image로 넣고 초기 strength는 방향·덩어리 유도를 위한 300-400 범위로 설정한다. UI에 strength가 없으면 실제 노출 설정을 기록한다.
12. 주 피사체는 40-52px 크기, 의미 있는 선은 최소 4px, 요소 간 간격은 최소 4-6px가 되도록 생성한다.
13. 첫 결과가 방향, 부분 물체, 큰 실루엣을 지키는지 먼저 검사한다. 치명적으로 틀리면 같은 prompt_only 문구를 늘리지 않는다.
14. 방향 실패는 silhouette Init Image를 교체하거나 strength를 조정하고, 부분 물체 실패는 fragment mask 또는 더 직접적인 시각 형상 Init Image를 사용한다.
15. 큰 arc, field, trail처럼 의미를 전달하는 효과가 필요한 경우에만 PixelLab `Edit image`로 1회 보강한다. 편집 지시는 `add`, `remove`, `change`, `replace` 중 하나로 시작하는 짧은 문장으로 작성한다.
16. 정확한 개수가 필요한 sparks, chips, threads, chevrons는 생성 또는 Edit image에 맡기지 않는다.
17. 정확한 개수 요소는 결정적 픽셀 오버레이로 추가한다. 각 요소는 최종 80×80 기준 최소 4×4px, 상호 간격 최소 4px를 지킨다.
18. arcs와 rings는 최종 기준 3-4px 두께를 확보한다.
19. frameTemplatePath의 기존 80×80 템플릿을 기준으로 flat charcoal/deep-brown 내부 배경을 적용한다.
20. 주 피사체와 효과를 중앙 64×64 영역 안에 합성하고 safe-area 밖의 생성 콘텐츠를 제거한다.
21. 최종 rows/columns 0, 1, 78, 79는 frameTemplatePath의 값으로 덮어써 exact outer 2px frame을 보장한다.
22. 19-21은 resize/crop이 아니라 deterministic frame/background/safe-area normalization으로 기록한다.
23. 최종 80×80 이미지를 nearest-neighbor 방식으로 32×32 미리보기하여 primarySymbol과 의미 효과가 살아 있는지 확인한다.
24. 최종 PNG와 생성·편집·오버레이·정규화 기록을 evaluationRoot의 기존 equipmentId 보존 구조에 저장한다.
25. SkillIconEvaluationGuide.md로 평가하고 치명적 실패가 없으며 85점 이상인 경우에만 outputIconPath로 복사한다.
26. Unity PNG는 보존된 최종 source와 바이트 및 SHA-256이 같아야 하며 `.meta`는 기존 import 정책을 따른다.

실패 라우팅:
- direction_failure: silhouette Init Image 교체 또는 strength 조정
- partial_object_failure: fragment mask/reference 또는 시각 형상 문구 교체
- exact_count_failure: deterministic overlay로 처리
- semantic_effect_failure: Edit image의 짧은 add/remove/change/replace 지시 사용
- frame_background_failure: frameTemplatePath 기반 deterministic normalization 재실행
- safe_margin_failure: 합성 위치·크기 조정 후 safe-area 밖 콘텐츠 제거
- small_size_failure: primary 40-52px, 선 4px 이상, 간격 4-6px 이상으로 재구성

Output:
- Skill ID
- Source JSON
- Output Icon Path
- Grade / Slot / Classification
- Silhouette Family
- Silhouette Init Image Path
- Init Image Strength
- Frame Template Path
- Five-Sentence Primary Description
- Semantic Edit Instruction / Result
- Exact-Count Overlay Manifest
- Requested / Downloaded Size
- Primary Display Size
- Safe-Area Normalization
- Frame Normalization
- 32×32 Preview Result
- Generation Record Path
- Candidate Score Summary Path
- Final Score
- Pass / Conditional Pass / Fail
- Unity Meta Status
- Failure Reasons

실패 시 Output:
- status: failed
- failureType:
  - missing_skill_json
  - invalid_skill_json
  - equipment_id_mismatch
  - invalid_grade
  - unsupported_slot
  - missing_frame_template
  - invalid_frame_template
  - missing_silhouette_init_image
  - invalid_silhouette_init_image
  - pixellab_unavailable
  - pixellab_authentication_failed
  - insufficient_pixellab_credits
  - wrong_pixellab_tool
  - generation_timeout
  - no_passing_candidate
  - normalization_failed
  - output_write_failed
  - unity_import_pending
- 실패 원인
- 사용한 기존 경로
- 생성하지 않은 파일 또는 폴더
- 마지막 Description 또는 Edit instruction
- 다음에 필요한 작업

주의:
- 다른 PC의 절대 경로를 복사하지 않는다.
- 프레임·배경·안전 여백·정확한 개수 요소를 생성 모델에 맡기지 않는다.
- 기존 완성 아이콘을 스타일 레퍼런스로 사용하지 않는다.
- 프레임, 카드, 패널 문구를 생성 Description에 넣지 않는다.
- 좌표와 금지문을 반복해 긴 Attempt 2를 만들지 않는다.
- placeholder 템플릿이나 임시 실루엣을 만들어 통과 처리하지 않는다.
- gameplay JSON이나 스킬 밸런스를 수정하지 않는다.
```
