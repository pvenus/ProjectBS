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
5. 가이드의 Concise Outline Prompt Contract에 따라 영어 Description을 4개 짧은 핵심 문장으로 작성한다. `backgroundMode=contextual`일 때만 짧은 배경 문장 하나를 추가할 수 있다.
6. 첫 문장은 가장 중요한 시각 형상과 굵은 아웃라인만 기술한다. 의미 명칭보다 실제 보이는 형상을 먼저 쓴다.
7. 둘째 문장은 방향과 구도를 하나의 축으로 단정적으로 기술한다.
8. 셋째 문장은 스킬을 식별하는 크고 단순한 효과 하나만 기술한다. 작은 입자나 정확한 개수는 쓰지 않는다.
9. `backgroundMode=contextual`은 배경이 스킬의 위치·영역·진행 방향·발동 원인을 설명해야만 선택한다. 이때 낮은 대비의 배경 요소 1-2개만 한 문장으로 묘사한다.
10. 배경이 스킬 식별에 필요하지 않으면 `backgroundMode=flat`으로 지정하고 장면 묘사는 생략한다. 최종 단색 배경은 템플릿 정규화로 처리한다.
11. 마지막 핵심 문장은 가장 가능성이 높은 오인 대상 3-6개와 grade/palette/pixel-art 가독성만 짧게 쓴다.
12. frame, card, panel, background border, safe-area 좌표, exact-count 요소는 생성 Description에서 제거한다.
13. PixelLab `Create UI elements (Pro)`를 열고 Custom size를 80×80으로 설정한다.
14. `backgroundMode=contextual`이면 Transparent background를 Off, `flat`이면 On으로 설정한다.
15. Concept image는 비워 두고 Color palette에는 분류된 핵심 색상만 간결하게 입력한다.
16. Pro가 한 번에 반환하는 4×4, 총 16개 80×80 변형을 각각 독립 후보로 보존한다. 합쳐진 그리드를 아이콘 후보로 사용하지 않는다.
17. primary 40-52px, 의미 선 최소 4px, 요소 간격 최소 4-6px를 목표로 생성한다.
18. 16개 변형에 정적·의미 검사를 먼저 적용하고 상위 3개까지만 편집·오버레이·정규화 대상으로 진행한다.
19. 첫 실행이 실패하면 문장을 추가하지 말고 실패한 한 문장을 더 짧고 직접적인 문장으로 교체한다. Pro 재실행은 16개 변형이 모두 핵심 구조에 실패했을 때만 요청한다.
20. 방향 실패 시 direction 문장을 `left to right`, `top to bottom`, `upper left to lower right`, `centered radial`, `centered symmetrical` 중 하나로 교체하고 새 seed로 재생성한다.
21. 부분 물체가 완전한 생물·인물로 복원되면 의미 명칭을 제거하고 시각 형상만 남긴다. 예: `wolf jaw` 대신 `two disconnected dark-gray crescent jaw strips`.
22. oneSimpleEffect가 빠졌지만 primary는 올바를 때만 `Edit image`로 한 번 보강한다. 지시는 `add`, `remove`, `change`, `replace` 중 하나로 시작하는 한 문장만 사용한다.
23. exactCountElements는 생성·편집 프롬프트에 맡기지 않고 결정적 픽셀 오버레이로 추가한다. 각 요소는 최소 4×4px, 간격 최소 4px이다.
24. arcs/rings는 최종 80×80 기준 3-4px 두께로 유지한다.
25. `flat`이면 frameTemplatePath의 flat charcoal/deep-brown 내부 배경을 적용한다. `contextual`이면 생성된 저대비 배경을 내부 영역에 보존하되 핵심 아웃라인보다 대비가 높아지지 않게 한다.
26. 두 모드 모두 primary와 효과를 중앙 64×64 영역으로 제한하고 rows/columns 0, 1, 78, 79를 템플릿 픽셀로 덮어쓴다. contextual 배경만 프레임 안쪽 전체에 존재할 수 있다.
27. 25-26은 resize/crop이 아니라 deterministic frame/background/safe-area normalization으로 기록한다.
28. 최종 80×80 이미지를 nearest-neighbor 32×32로 확인하여 primaryOutline, direction, oneSimpleEffect가 살아 있는지 검사한다.
29. 생성·편집·오버레이·정규화 기록을 evaluationRoot의 기존 equipmentId 구조에 보존한다.
30. normalized source가 85점 이상 Pass일 때만 outputIconPath와 `.meta`를 반영한다.

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
