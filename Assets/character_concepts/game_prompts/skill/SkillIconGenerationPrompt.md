# Skill Icon Generation Prompt

스킬 JSON을 기준으로 핵심 실루엣과 한 가지 효과만 간결하게 강조하여 80×80 정적 스킬 아이콘을 생성하고, 기존 템플릿으로 최종 규격을 보정하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 지정한 스킬의 80×80 정적 아이콘을 생성해줘. Concept Image나 스타일 레퍼런스 없이, 아이콘의 핵심 아웃라인과 한 가지 스킬 효과만 짧고 강하게 서술해 엉뚱한 부가 요소가 강조되지 않게 해줘.

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
- pixelLabCreatorUrl: https://www.pixellab.ai/create?tool=create_ui_pro
- primaryTool: Create UI elements (Pro)
- semanticEffectTool: Edit image
- generationMode: concise_outline_prompt
- referenceMode: none
- compositionProfile: {auto | horizontal_projectile | descending_projectile | diagonal_melee | centered_radial_active | centered_passive_emblem}
- backgroundMode: {auto | contextual | flat}
- internalBackgroundPolicy: skill_matched_low_contrast
- frameTemplatePath: {현재 PC에 이미 존재하는 승인된 80×80 프레임·배경 템플릿 절대경로}
- inheritedIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- outputIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- finalIconSize: 80x80
- primaryDisplaySize: 40-52px
- maxPrimaryGenerationCount: 1
- expectedPrimaryVariationCount: 16
- maxSemanticEditCount: 1

사전 확인:
1. 현재 PC에서 projectRoot, evaluationRoot, outputIconPath가 기존 문서·기록의 경로 체계와 일치하는지 확인한다.
2. 다른 PC에서 전달된 절대 경로를 사용하지 않고 새 폴더 구조를 임의로 만들지 않는다.
3. frameTemplatePath가 존재하고 정확히 80×80 RGBA인지 확인한다. 없으면 생성하지 말고 `missing_frame_template`로 중단한다.

작업:
1. skillSourcePath와 equipmentId를 검증하고 grade, slot, skillName을 확정한다.
2. 스킬 JSON에서 targeting, castMove, componentType, moveType, damage, buff, debuff, effect를 읽는다.
3. primaryOutline, direction, compositionProfile, oneSimpleEffect, backgroundRequirement, backgroundMode, backgroundDescription, exactCountElements, likelyWrongObjects, gradeIntensity, palette를 분류한다.
4. 하위 등급 아이콘은 계승 정체성을 분석하는 용도로만 읽고 PixelLab에 업로드하지 않는다.
5. 가이드의 Concise Outline Prompt Contract에 따라 영어 Description을 4개 짧은 핵심 문장으로 작성하고, 스킬과 어울리는 내부 배경 문장 하나를 추가한다.
6. 첫 문장은 가장 중요한 시각 형상과 굵은 아웃라인만 기술한다. 의미 명칭보다 실제 보이는 형상을 먼저 쓴다.
7. 둘째 문장은 방향과 구도를 하나의 축으로 단정적으로 기술한다.
8. 셋째 문장은 스킬을 식별하는 크고 단순한 효과 하나만 기술한다. 작은 입자나 정확한 개수는 쓰지 않는다.
9. 스킬 JSON의 element, targeting, castMove, moveType, damage, buff, debuff, effect를 근거로 스킬의 속성·위치·영역·진행 방향·발동 원인 중 가장 중요한 맥락 하나를 선택하고, 그 맥락과 어울리는 낮은 대비의 내부 배경 요소 1-2개를 한 문장으로 묘사한다.
10. 내부 배경은 프레임 안쪽을 채우는 단순한 환경 표면, 색면, 흐림, 균열, 바닥 문양 또는 저대비 에너지 흔적으로 구성한다. 핵심 아웃라인과 oneSimpleEffect보다 명도·채도·디테일 대비를 낮게 유지하고, 캐릭터·생물·무기·건물·문자·추가 스킬 효과를 배경에 넣지 않는다.
11. `backgroundMode=auto`는 기본적으로 `contextual`로 확정한다. 스킬 JSON에서 배경 맥락을 신뢰성 있게 도출할 수 없을 때만 `flat`을 사용하며, 이 경우에도 grade와 palette에 어울리는 저대비 색상 그라데이션 또는 미세한 재질감의 내부 배경을 지정한다.
12. 마지막 핵심 문장은 가장 가능성이 높은 오인 대상 3-6개와 grade/palette/pixel-art 가독성만 짧게 쓴다.
13. frame, card, panel, background border, safe-area 좌표, exact-count 요소는 생성 Description에서 제거한다.
14. PixelLab `Create UI elements (Pro)`를 열고 Custom size를 80×80으로 설정한다.
15. 내부 배경을 생성하기 위해 Transparent background를 Off로 설정한다.
16. Concept image는 비워 두고 Color palette에는 primary, oneSimpleEffect, 내부 배경에 사용할 핵심 색상을 역할별로 간결하게 입력한다.
17. Pro가 한 번에 반환하는 4×4, 총 16개 80×80 변형을 각각 독립 후보로 보존한다. 합쳐진 그리드를 아이콘 후보로 사용하지 않는다.
18. primary 40-52px, 의미 선 최소 4px, 요소 간격 최소 4-6px를 목표로 생성한다.
19. 16개 변형에 정적·의미 검사를 먼저 적용하고 상위 3개까지만 편집·오버레이·정규화 대상으로 진행한다.
20. 첫 실행이 실패하면 문장을 추가하지 말고 실패한 한 문장을 더 짧고 직접적인 문장으로 교체한다. Pro 재실행은 16개 변형이 모두 핵심 구조에 실패했을 때만 요청한다.
21. 방향 실패 시 direction 문장을 `left to right`, `top to bottom`, `upper left to lower right`, `centered radial`, `centered symmetrical` 중 하나로 교체하고 새 seed로 재생성한다.
22. 부분 물체가 완전한 생물·인물로 복원되면 의미 명칭을 제거하고 시각 형상만 남긴다. 예: `wolf jaw` 대신 `two disconnected dark-gray crescent jaw strips`.
23. oneSimpleEffect가 빠졌지만 primary는 올바를 때만 `Edit image`로 한 번 보강한다. 지시는 `add`, `remove`, `change`, `replace` 중 하나로 시작하는 한 문장만 사용한다.
24. exactCountElements는 생성·편집 프롬프트에 맡기지 않고 결정적 픽셀 오버레이로 추가한다. 각 요소는 최소 4×4px, 간격 최소 4px이다.
25. arcs/rings는 최종 80×80 기준 3-4px 두께로 유지한다.
26. `flat`이면 생성된 저대비 색상·재질 내부 배경을 frameTemplatePath의 내부 영역에 맞춰 보존한다. `contextual`이면 생성된 스킬 연관 저대비 배경을 내부 영역에 보존하되 핵심 아웃라인보다 대비가 높아지지 않게 한다.
27. 두 모드 모두 primary와 효과를 중앙 64×64 영역으로 제한하고 rows/columns 0, 1, 78, 79를 템플릿 픽셀로 덮어쓴다. 내부 배경만 프레임 안쪽 전체에 존재할 수 있다.
28. 26-27은 resize/crop이 아니라 deterministic frame/background/safe-area normalization으로 기록한다.
29. 최종 80×80 이미지를 nearest-neighbor 32×32로 확인하여 primaryOutline, direction, oneSimpleEffect가 살아 있는지 검사한다.
30. 생성·편집·오버레이·정규화 기록을 evaluationRoot의 기존 equipmentId 구조에 보존한다.
31. normalized source가 85점 이상 Pass일 때만 outputIconPath와 `.meta`를 반영한다.

실패 라우팅:
- direction_failure: direction 문장 교체 + 새 seed
- partial_object_failure: 의미 명칭 삭제 + 시각 형상 문장 교체
- unrelated_object_failure: 오인 대상 3-6개만 간결하게 교체
- semantic_effect_failure: 한 문장 Edit image
- exact_count_failure: deterministic overlay
- frame_background_failure: frameTemplatePath 기반 normalization
- small_size_failure: primary 확대, 의미 선 굵게, 요소 간격 확대

Output:
- Skill ID / Source JSON / Output Icon Path
- Grade / Slot / Classification
- Composition Profile
- Background Mode / Requirement / Description
- Core Outline Sentence
- Direction Sentence
- Simple Effect Sentence
- Compact Exclusion and Grade Sentence
- Semantic Edit Instruction / Result
- Frame Template Path
- Exact-Count Overlay Manifest
- Requested / Downloaded Size
- Safe-Area / Frame Normalization
- 32×32 Preview Result
- Generation Record / Candidate Scores Paths
- Final Score / Result
- Unity Meta Status

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
  - pixellab_unavailable
  - pixellab_authentication_failed
  - insufficient_pixellab_credits
  - wrong_pixellab_tool
  - generation_timeout
  - semantic_edit_failed
  - overlay_failed
  - normalization_failed
  - no_passing_candidate
  - output_write_failed
  - unity_import_pending
- 실패 원인
- 사용한 기존 경로
- 생성하지 않은 파일 또는 폴더
- 마지막 핵심 4문장과 선택적 배경 문장 또는 Edit instruction
- 다음에 필요한 작업

주의:
- Concept Image와 스타일 레퍼런스를 사용하지 않는다.
- 핵심 아웃라인보다 캐릭터·배경·장식이 강조되는 문장을 쓰지 않는다.
- 스킬 효과를 여러 개 나열하지 않는다.
- 실패한 프롬프트 뒤에 좌표와 금지문을 계속 추가하지 않는다.
- 프레임·배경·안전 여백·정확한 개수는 생성 모델에 맡기지 않는다.
- gameplay JSON이나 스킬 밸런스를 수정하지 않는다.
```
