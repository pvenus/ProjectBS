# Skill Icon Generation Prompt

스킬 JSON을 기준으로 핵심 실루엣과 한 가지 효과만 간결하게 강조하여 80×80 정적 스킬 아이콘을 생성하고, 기존 템플릿으로 최종 규격을 보정하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 지정한 스킬의 80×80 정적 아이콘을 생성해줘. Concept Image나 스타일 레퍼런스 없이, 도구 외형만 정물처럼 단순하게 보여 주지 말고 실제 스킬 도구가 한 가지 이펙트와 함께 발동되는 순간 또는 스킬의 작용을 압축한 문양을 핵심 아웃라인으로 표현해줘. 한국 전통 다크 판타지 스타일을 유지하되 엉뚱한 부가 요소가 강조되지 않게 해줘.

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
- representationMode: {auto | activated_tool | symbolic_emblem}
- styleProfile: korean_traditional_dark_fantasy_pixel_art
- koreanTraditionalStylePolicy: one_primary_motif_low_detail
- compositionProfile: {auto | horizontal_projectile | descending_projectile | diagonal_melee | centered_radial_active | centered_passive_emblem}
- backgroundMode: {auto | contextual | flat}
- internalEffectPolicy: skill_matched_single_effect
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
3. representationMode, activationMoment, primaryOutline, direction, compositionProfile, oneSimpleEffect, internalEffectDescription, koreanTraditionalMotif, traditionalMaterial, backgroundRequirement, backgroundMode, backgroundDescription, exactCountElements, likelyWrongObjects, gradeIntensity, palette를 분류한다.
4. 하위 등급 아이콘은 계승 정체성을 분석하는 용도로만 읽고 PixelLab에 업로드하지 않는다.
5. `representationMode=auto`이면 스킬을 수행하는 도구와 발동 동작이 시각적으로 중요한 경우 `activated_tool`, 추상 효과·버프·디버프·패시브이거나 도구가 정적인 장비 아이콘으로 오인될 가능성이 높은 경우 `symbolic_emblem`으로 확정한다.
6. `activated_tool`은 도구가 휘두르기·발사·충돌·공명·연소·개방·회전·소환 중 하나의 명확한 발동 상태에 있는 순간을 그린다. 도구를 수직으로 세워 놓거나 카탈로그 제품처럼 고립시킨 정적인 외형은 금지한다.
7. `symbolic_emblem`은 스킬의 방향·범위·속성·효과를 하나의 굵은 기하 실루엣으로 합친 문양으로 만든다. 실제 장비 문장, 로고, 배지, 문자 또는 UI 버튼처럼 보이지 않게 한다.
8. 한국 전통 디자인은 스킬 의미와 직접 연결되는 주 모티프 하나만 선택한다. 허용 예시는 단청의 구름·연꽃·덩굴 곡선, 삼태극 회전, 조선 창호 격자, 귀면와의 곡선, 무속 오방색 띠, 전통 매듭의 흐름이며 80×80에서 작은 장식이 되지 않도록 굵고 단순하게 변형한다.
9. 전통 재질과 색상은 한지, 옻칠 목재, 묵선, 낡은 청동, 단청 안료 중 스킬에 맞는 1-2개만 사용한다. 오방색은 전부 나열하지 않고 grade와 element에 맞는 2-4색으로 제한한다.
10. 가이드의 Concise Outline Prompt Contract에 따라 영어 Description을 필수 5문장으로 작성한다. 순서는 `발동 형상 또는 스킬 문양`, `방향과 구도`, `연결된 내부 이펙트`, `스킬 맞춤 내부 배경`, `배제 대상과 grade/palette/한국 전통 pixel-art 스타일`이다.
11. 첫 문장은 의미 명칭보다 실제 보이는 발동 형상 또는 굵은 문양을 먼저 쓴다. `activated_tool`이면 도구와 동작을 하나의 실루엣으로, `symbolic_emblem`이면 스킬 작용과 전통 모티프를 하나의 문양으로 기술한다.
12. 둘째 문장은 방향과 구도를 하나의 축으로 단정적으로 기술한다.
13. 셋째 문장은 스킬 JSON의 element, damage, buff, debuff, effect를 근거로 slash arc, impact burst, aura ring, magic trail, elemental glow, shockwave 중 가장 잘 맞는 내부 이펙트 하나만 기술한다.
14. `activated_tool`의 이펙트는 날·끝·타격점·발사구·공명부처럼 도구의 실제 발동 지점에서 시작해야 한다. `symbolic_emblem`의 이펙트는 문양의 중심·외곽선·회전축 중 하나와 직접 연결해야 한다. 도구와 이펙트가 서로 떨어진 별도 물체처럼 보이면 실패다.
15. 내부 이펙트는 primaryOutline의 뒤쪽·둘레·진행 경로 중 의미에 맞는 한 위치에 배치하고 아이콘 프레임 안쪽에서 끝나게 한다. primaryOutline보다 명도 또는 채도는 높일 수 있지만 핵심 형상을 가리거나 별도의 물체처럼 보이게 하지 않는다.
16. 넷째 문장은 스킬의 속성·위치·영역·진행 방향·발동 원인 중 가장 중요한 맥락 하나와 한국 전통 모티프를 결합한 낮은 대비의 내부 배경 요소 1-2개를 기술한다.
17. 내부 배경은 프레임 안쪽을 채우는 단순한 환경 표면, 단청 색면, 한지·옻칠 재질, 창호 격자, 흐림, 균열, 바닥 문양 또는 저대비 에너지 흔적으로 구성한다. 핵심 아웃라인과 내부 이펙트보다 명도·채도·디테일 대비를 낮게 유지한다.
18. 다섯째 문장은 정적인 도구, 인벤토리 아이템, 문자, 로고, 서양 문장, 일본 신사·도리이·오니·가몬, 중국 동전·용 문양 등 오인 대상을 3-6개 이내로 배제하고 grade/palette/한국 전통 pixel-art 가독성을 짧게 지정한다.
19. 시각 계층은 `activatedToolOrEmblem > oneSimpleEffect > koreanTraditionalAccent > internalBackground` 순서로 고정한다. 전통 모티프는 스킬을 설명해야 하며 별도 장식 테두리처럼 주제를 압도하면 안 된다.
20. `backgroundMode=auto`는 기본적으로 `contextual`로 확정한다. 스킬 JSON에서 배경 맥락을 신뢰성 있게 도출할 수 없을 때만 `flat`을 사용하며, 이 경우에도 전통 재질과 grade/palette에 어울리는 저대비 색면을 지정한다.
21. frame, card, panel, background border, safe-area 좌표, exact-count 요소는 생성 Description에서 제거한다.
22. PixelLab `Create UI elements (Pro)`를 열고 Custom size를 80×80으로 설정한다.
23. 발동 이펙트와 내부 배경을 함께 생성하기 위해 Transparent background를 Off로 설정한다.
24. Concept image는 비워 두고 Color palette에는 primary, oneSimpleEffect, 전통 강조색, 내부 배경 색상을 역할별로 간결하게 입력한다.
25. Pro가 한 번에 반환하는 4×4, 총 16개 80×80 변형을 각각 독립 후보로 보존한다. 합쳐진 그리드를 아이콘 후보로 사용하지 않는다.
26. primary 40-52px, 의미 선 최소 4px, 요소 간격 최소 4-6px를 목표로 생성한다.
27. 16개 변형에 정적·의미 검사를 먼저 적용한다. 정적인 도구 외형, 인벤토리 아이템 구도, 이펙트와 도구의 분리, 한국 전통 모티프 부재 또는 타 문화 모티프 혼입을 검사하고 상위 3개까지만 후속 처리한다.
28. 첫 실행이 실패하면 문장을 추가하지 말고 실패한 한 문장을 더 짧고 직접적인 문장으로 교체한다. Pro 재실행은 16개 변형이 모두 핵심 구조에 실패했을 때만 요청한다.
29. 방향 실패 시 direction 문장을 `left to right`, `top to bottom`, `upper left to lower right`, `centered radial`, `centered symmetrical` 중 하나로 교체하고 새 seed로 재생성한다.
30. 부분 물체가 완전한 생물·인물로 복원되면 의미 명칭을 제거하고 시각 형상만 남긴다. 예: `wolf jaw` 대신 `two disconnected dark-gray crescent jaw strips`.
31. 도구가 정적인 아이템으로 생성되면 `activated_tool` 문장을 발동 동사와 접촉점이 보이는 문장으로 교체한다. 그래도 정물 구도가 반복되면 `symbolic_emblem`으로 전환해 스킬 작용 자체를 문양화한다.
32. oneSimpleEffect가 빠졌지만 primary는 올바를 때만 `Edit image`로 한 번 보강한다. 지시는 `add`, `remove`, `change`, `replace` 중 하나로 시작하고 도구 또는 문양의 연결 지점을 포함하는 한 문장만 사용한다.
33. exactCountElements는 생성·편집 프롬프트에 맡기지 않고 결정적 픽셀 오버레이로 추가한다. 각 요소는 최소 4×4px, 간격 최소 4px이다.
34. arcs/rings는 최종 80×80 기준 3-4px 두께로 유지한다.
35. `flat`이면 생성된 저대비 전통 색상·재질 내부 배경을 frameTemplatePath의 내부 영역에 맞춰 보존한다. `contextual`이면 생성된 스킬 연관 전통 배경을 보존하되 핵심 발동 형상보다 대비가 높아지지 않게 한다.
36. 두 모드 모두 발동 형상 또는 문양과 내부 이펙트를 중앙 64×64 영역으로 제한하고 rows/columns 0, 1, 78, 79를 템플릿 픽셀로 덮어쓴다. 내부 배경만 프레임 안쪽 전체에 존재할 수 있다.
37. 35-36은 resize/crop이 아니라 deterministic frame/background/safe-area normalization으로 기록한다.
38. 최종 80×80 이미지를 nearest-neighbor 32×32로 확인하여 발동 동작 또는 스킬 문양, direction, oneSimpleEffect, 한국 전통 모티프, internalBackground가 살아 있고 시각 계층 순서가 유지되는지 검사한다.
39. 생성·편집·오버레이·정규화 기록을 evaluationRoot의 기존 equipmentId 구조에 보존한다.
40. normalized source가 85점 이상 Pass일 때만 outputIconPath와 `.meta`를 반영한다.

실패 라우팅:
- direction_failure: direction 문장 교체 + 새 seed
- partial_object_failure: 의미 명칭 삭제 + 시각 형상 문장 교체
- inert_tool_failure: 발동 동사·발동 지점이 보이는 `activated_tool` 문장 교체, 반복 실패 시 `symbolic_emblem` 전환
- disconnected_effect_failure: 이펙트가 시작되는 도구 접촉점 또는 문양 연결축을 명시한 한 문장 Edit image
- korean_traditional_style_failure: 일반 장식을 제거하고 스킬 의미에 맞는 한국 전통 주 모티프 하나와 전통 재질 1-2개로 교체
- foreign_motif_failure: 타 문화 모티프를 제거하고 단청·삼태극·창호·귀면와·오방색 띠·전통 매듭 중 스킬에 맞는 하나로 교체
- unrelated_object_failure: 오인 대상 3-6개만 간결하게 교체
- semantic_effect_failure: 한 문장 Edit image
- exact_count_failure: deterministic overlay
- frame_background_failure: frameTemplatePath 기반 normalization
- small_size_failure: primary 확대, 의미 선 굵게, 요소 간격 확대

Output:
- Skill ID / Source JSON / Output Icon Path
- Grade / Slot / Classification
- Representation Mode / Activation Moment
- Activated Tool or Symbolic Emblem Description
- Korean Traditional Motif / Material / Palette
- Composition Profile
- Background Mode / Requirement / Description
- Core Outline Sentence
- Direction Sentence
- Internal Effect Sentence / Type / Placement
- Tool-or-Emblem / Effect Connection Check
- Internal Effect and Background Contrast Check
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
  - representation_failure
  - inert_tool_failure
  - disconnected_effect_failure
  - korean_traditional_style_failure
  - foreign_motif_failure
  - semantic_edit_failed
  - overlay_failed
  - normalization_failed
  - no_passing_candidate
  - output_write_failed
  - unity_import_pending
- 실패 원인
- 사용한 기존 경로
- 생성하지 않은 파일 또는 폴더
- 마지막 필수 5문장 또는 Edit instruction
- 다음에 필요한 작업

주의:
- Concept Image와 스타일 레퍼런스를 사용하지 않는다.
- 스킬 도구를 발동 없이 세워 둔 정적인 장비·재료·인벤토리 아이템 외형으로 만들지 않는다.
- 도구를 사용하는 스킬은 도구의 발동 동작과 이펙트 시작점이 하나의 실루엣으로 연결되어야 한다.
- 추상 스킬·버프·디버프·패시브는 장비를 억지로 넣지 말고 스킬 작용과 한국 전통 모티프를 결합한 문양으로 표현한다.
- 한국 전통 디자인은 스킬과 연결된 주 모티프 하나만 사용하고, 여러 전통 요소를 장식 목록처럼 나열하지 않는다.
- 한글·한자·부적 글자·문장·로고를 읽을 수 있는 텍스트로 생성하지 않는다.
- 한국 전통 스타일을 일반 동아시아풍으로 대체하지 않고 일본·중국·서양의 대표 상징을 혼입하지 않는다.
- 핵심 아웃라인보다 캐릭터·배경·장식이 강조되는 문장을 쓰지 않는다.
- 스킬 효과를 여러 개 나열하지 않는다.
- 실패한 프롬프트 뒤에 좌표와 금지문을 계속 추가하지 않는다.
- 프레임·안전 여백·정확한 개수는 생성 모델에 맡기지 않는다. 내부 배경은 생성하되 최종 프레임과 안전 영역은 템플릿 정규화로 확정한다.
- gameplay JSON이나 스킬 밸런스를 수정하지 않는다.
```
