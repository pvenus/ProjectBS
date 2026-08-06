# Skill Icon Generation Prompt


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

스킬 JSON을 기준으로 핵심 실루엣과 한 가지 효과만 간결하게 강조하여 80×80 정적 스킬 아이콘을 생성하고, 기존 템플릿으로 최종 규격을 보정하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 지정한 스킬의 80×80 정적 아이콘을 생성해줘. Concept Image나 스타일 레퍼런스 없이, 도구 외형만 정물처럼 단순하게 보여 주지 말고 실제 스킬 도구가 한 가지 이펙트와 함께 발동되는 순간 또는 스킬의 작용을 압축한 문양을 핵심 아웃라인으로 표현해줘. 한국 전통 다크 판타지 스타일을 유지하되 엉뚱한 부가 요소가 강조되지 않게 해줘. 내부 배경은 단색으로만 채우지 말고 스킬의 속성·발동 경로·타격 결과 중 하나가 드러나는 저대비 그림으로 구성하며, 캔버스 네 모서리와 가장자리까지 끊김 없이 이어지게 해줘.

참조 가이드:
- AgentDocs/planning-guides/skill/SkillIconGenerationGuide.md
- AgentDocs/planning-guides/skill/SkillIconDownloadGuide.md
- AgentDocs/planning-guides/skill/SkillIconEvaluationGuide.md
- AgentDocs/planning-guides/skill/data-structures/SkillJsonGuide.md
- AgentDocs/planning-guides/skill/data-structures/EquipmentSkillSO.md
- AgentDocs/planning-guides/skill/design/SkillDegineGuide.md

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
- backgroundMode: {auto | contextual | symbolic_effect_scene}
- internalEffectPolicy: skill_matched_single_effect
- internalBackgroundPolicy: skill_effect_illustrated_low_contrast_full_bleed
- backgroundIllustrationPolicy: effect_derived_scene_not_solid_fill
- backgroundBaseTonePolicy: palette_bound_medium_dark_no_default_white
- maxNearWhiteBlankRatio: 2%
- maxDominantBackgroundColorRatio: 85%
- requireFourCornerBackgroundCoverage: true
- frameTemplatePath: {현재 PC에 이미 존재하는 승인된 80×80 프레임·배경 템플릿 절대경로}
- inheritedIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- outputIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- finalIconSize: 80x80
- primaryDisplaySize: 40-52px
- maxPrimaryGenerationCount: 2
- expectedPrimaryVariationCount: 16
- maxSemanticEditCount: 1

사전 확인:
1. 현재 PC에서 projectRoot, evaluationRoot, outputIconPath가 기존 문서·기록의 경로 체계와 일치하는지 확인한다.
2. 다른 PC에서 전달된 절대 경로를 사용하지 않고 새 폴더 구조를 임의로 만들지 않는다.
3. frameTemplatePath가 존재하고 정확히 80×80 RGBA인지 확인한다. 없으면 샘플 또는 임시 결과라도 생성하지 말고 즉시 `missing_frame_template`로 중단한다.

작업:
1. skillSourcePath와 equipmentId를 검증하고 grade, slot, skillName을 확정한다.
2. 스킬 JSON에서 targeting, castMove, componentType, moveType, damage, buff, debuff, effect를 읽는다.
3. representationMode, activationMoment, primaryOutline, direction, compositionProfile, oneSimpleEffect, internalEffectDescription, koreanTraditionalMotif, traditionalMaterial, backgroundRequirement, backgroundMode, backgroundBaseColor, backgroundSceneAction, backgroundSceneElements, backgroundDescription, exactCountElements, likelyWrongObjects, gradeIntensity, palette를 분류한다. backgroundBaseColor는 palette에 포함된 중저명도 색으로 확정하며 기본 흰색을 사용하지 않는다. backgroundSceneAction은 스킬의 속성·진행 방향·범위·타격 결과 중 하나를 시각화하고, backgroundSceneElements는 이를 보여 주는 환경 표면 또는 대기 형태 1개와 한국 전통 모티프 1개로 제한한다.
4. 하위 등급 아이콘은 계승 정체성을 분석하는 용도로만 읽고 PixelLab에 업로드하지 않는다.
5. `representationMode=auto`이면 스킬을 수행하는 도구와 발동 동작이 시각적으로 중요한 경우 `activated_tool`, 추상 효과·버프·디버프·패시브이거나 도구가 정적인 장비 아이콘으로 오인될 가능성이 높은 경우 `symbolic_emblem`으로 확정한다.
6. `activated_tool`은 도구가 휘두르기·발사·충돌·공명·연소·개방·회전·소환 중 하나의 명확한 발동 상태에 있는 순간을 그린다. 도구를 수직으로 세워 놓거나 카탈로그 제품처럼 고립시킨 정적인 외형은 금지한다.
7. `symbolic_emblem`은 스킬의 방향·범위·속성·효과를 하나의 굵은 기하 실루엣으로 합친 문양으로 만든다. 실제 장비 문장, 로고, 배지, 문자 또는 UI 버튼처럼 보이지 않게 한다.
8. 마스터 컨셉에 부합하면서 스킬 의미와 직접 연결되는 한국 전통 주 모티프 하나를 선택하고, 80×80에서 읽히도록 굵고 단순하게 변형한다. 문화적 적합성과 타 문화 금지는 마스터 문서를 단일 기준으로 판정한다.
9. 마스터 컨셉에 따라 검증된 전통 재질 1-2개와 역할이 분명한 전통 색채 조합 2-4색만 선택한다.
10. 가이드의 Concise Outline Prompt Contract에 따라 영어 Description을 필수 5문장으로 작성한다. 순서는 `발동 형상 또는 스킬 문양`, `방향과 구도`, `연결된 내부 이펙트`, `스킬 맞춤 내부 배경`, `배제 대상과 grade/palette/한국 전통 pixel-art 스타일`이다.
11. 첫 문장은 의미 명칭보다 실제 보이는 발동 형상 또는 굵은 문양을 먼저 쓴다. `activated_tool`이면 도구와 동작을 하나의 실루엣으로, `symbolic_emblem`이면 스킬 작용과 전통 모티프를 하나의 문양으로 기술한다.
12. 둘째 문장은 방향과 구도를 하나의 축으로 단정적으로 기술한다.
13. 셋째 문장은 스킬 JSON의 element, damage, buff, debuff, effect를 근거로 slash arc, impact burst, aura ring, magic trail, elemental glow, shockwave 중 가장 잘 맞는 내부 이펙트 하나만 기술한다.
14. `activated_tool`의 이펙트는 날·끝·타격점·발사구·공명부처럼 도구의 실제 발동 지점에서 시작해야 한다. `symbolic_emblem`의 이펙트는 문양의 중심·외곽선·회전축 중 하나와 직접 연결해야 한다. 도구와 이펙트가 서로 떨어진 별도 물체처럼 보이면 실패다.
15. 내부 이펙트는 primaryOutline의 뒤쪽·둘레·진행 경로 중 의미에 맞는 한 위치에 배치하고 아이콘 프레임 안쪽에서 끝나게 한다. primaryOutline보다 명도 또는 채도는 높일 수 있지만 핵심 형상을 가리거나 별도의 물체처럼 보이게 하지 않는다.
16. 넷째 문장은 반드시 `A full-bleed {backgroundBaseColor} {traditionalMaterial} background depicts {backgroundSceneAction} through {환경 표면 또는 대기 형태 1개} and {한국 전통 모티프 1개}, filling the entire icon canvas edge to edge including all four corners; no solid-color-only, white, transparent, blank, or unpainted area remains.` 형식을 사용한다. `{backgroundBaseColor}`에는 `dark`, `charcoal`, `deep`, `muted`처럼 중저명도임을 나타내는 수식어를 포함하고, `depicts` 뒤에는 반드시 눈에 보이는 장면 동사를 사용한다.
17. 내부 배경은 프레임 안쪽 전체를 채우는 하나의 연속된 바탕면 위에 스킬의 작용을 보여 주는 저대비 그림으로 구성한다. 예시는 베기 진행 방향을 따르는 바닥 흠집과 단청 구름 흐름, 충격점에서 번지는 옻칠 균열과 창호 격자 그림자, 냉기 경로를 따라 서리는 한지 결, 화염 뒤로 그을리는 단청 안료, 독기가 스며드는 바닥 문양처럼 스킬 효과와 환경의 반응이 함께 보이는 장면이다. 단일 균일 색면, 단순 그라데이션만 있는 면, 문양 뒤에만 붙은 국소 색면, 흰색 기본 캔버스, 투명 영역, 미도색 여백은 내부 배경으로 인정하지 않는다. 배경 그림은 네 모서리와 상·하·좌·우 가장자리까지 이어지되 핵심 아웃라인과 내부 이펙트보다 명도·채도·디테일 대비를 낮게 유지한다.
18. 다섯째 문장은 정적인 도구, 인벤토리 아이템, 문자, 로고와 마스터 컨셉이 금지하는 타 문화 요소 등 오인 대상을 3-6개 이내로 배제하고 grade/palette/한국 전통 pixel-art 가독성을 짧게 지정한다.
19. 시각 계층은 `activatedToolOrEmblem > oneSimpleEffect > koreanTraditionalAccent > internalBackground` 순서로 고정한다. 전통 모티프는 스킬을 설명해야 하며 별도 장식 테두리처럼 주제를 압도하면 안 된다.
20. `backgroundMode=auto`는 스킬의 장소·대상·환경 반응을 신뢰성 있게 도출할 수 있으면 `contextual`, 그렇지 않으면 스킬의 속성·방향·범위·타격 결과를 추상적인 배경 장면으로 나타내는 `symbolic_effect_scene`으로 확정한다. 단색만 사용하는 `flat` 모드는 허용하지 않으며 두 허용 모드 모두 full-bleed와 스킬 연관 그림 조건의 예외가 아니다.
21. frame, card, panel, background border, safe-area 좌표, exact-count 요소는 생성 Description에서 제거한다. 단, 넷째 문장의 `full-bleed`, `depicts`, `entire icon canvas`, `edge to edge`, `all four corners`, `no solid-color-only, white, transparent, blank, or unpainted area` 조건은 삭제하거나 완화하지 않는다.
22. PixelLab `Create UI elements (Pro)`를 열고 Custom size를 80×80으로 설정한다.
23. 발동 이펙트와 내부 배경을 함께 생성하기 위해 Transparent background를 Off로 설정한다. 이 설정은 알파만 비활성화할 뿐 지정 배경의 생성이나 전체 채움을 보장하지 않으므로, 흰색 불투명 픽셀을 정상 배경으로 간주하지 않는다.
24. Concept image는 비워 두고 Color palette에는 primary, oneSimpleEffect, 전통 강조색, 내부 배경의 backgroundBaseColor와 장면 보조색을 역할별로 간결하게 입력한다. 내부 배경은 순백색 또는 UI 기본 캔버스색을 사용하지 않고 grade와 스킬 속성에 맞는 중저명도 바탕색과 구분 가능한 저대비 보조색을 사용한다. backgroundBaseColor 하나만 입력하여 균일 단색 배경을 유도하지 않는다.
25. Pro가 한 번에 반환하는 4×4, 총 16개 80×80 변형을 정확히 분할하여 각각 독립 후보로 보존한다. 합쳐진 그리드를 아이콘 후보로 사용하지 않으며, 셀 구분선 제거 시 인접한 흰색 픽셀을 복사하지 않고 frameTemplatePath의 해당 가장자리 픽셀만 사용한다.
26. primary 40-52px, 의미 선 최소 4px, 요소 간격 최소 4-6px를 목표로 생성한다.
27. 16개 변형에 배경 하드 게이트를 먼저 적용한다. 80×80 전체에서 RGB 각 채널이 모두 235 이상인 기본 흰색·미도색 추정 픽셀이 2%를 초과하거나, 내부 배경이 네 모서리 및 상·하·좌·우 가장자리까지 연속되지 않거나, 배경이 문양 뒤의 국소 패치로만 존재하면 `blank_canvas_failure`로 즉시 탈락시킨다. 배경 가시 영역에서 단일 RGB 색상이 85%를 초과하거나 스킬에 연결된 환경 반응·대기 흐름·전통 문양 중 식별 가능한 배경 그림이 없으면 `solid_background_failure`로 즉시 탈락시킨다. 밝은 효과 하이라이트는 배경과 연결되지 않은 제한된 내부 픽셀일 때만 near-white 비율에서 제외할 수 있으며, 제외 근거를 기록한다.
28. 배경 하드 게이트를 통과한 변형에만 정적·의미 검사를 적용한다. 정적인 도구 외형, 인벤토리 아이템 구도, 이펙트와 도구의 분리, 한국 전통 모티프 부재 또는 타 문화 모티프 혼입을 검사하고 상위 3개까지만 후속 처리한다. 하드 게이트를 통과하지 못한 후보를 상대적으로 가장 낫다는 이유로 선택하지 않는다.
29. 첫 실행이 실패하면 문장을 추가하지 말고 실패한 한 문장을 더 짧고 직접적인 문장으로 교체한다. 16개 변형이 모두 `blank_canvas_failure` 또는 `solid_background_failure`이면 넷째 문장을 필수 full-bleed 장면 형식으로 다시 작성하고 backgroundSceneAction을 더 구체적인 시각 동사로 교체하여 새 seed로 한 번만 재생성한다. 재생성 후보도 모두 실패하면 `no_passing_candidate`로 중단한다.
30. 방향 실패 시 direction 문장을 `left to right`, `top to bottom`, `upper left to lower right`, `centered radial`, `centered symmetrical` 중 하나로 교체하고 새 seed로 재생성한다.
31. 부분 물체가 완전한 생물·인물로 복원되면 의미 명칭을 제거하고 시각 형상만 남긴다. 예: `wolf jaw` 대신 `two disconnected dark-gray crescent jaw strips`.
32. 도구가 정적인 아이템으로 생성되면 `activated_tool` 문장을 발동 동사와 접촉점이 보이는 문장으로 교체한다. 그래도 정물 구도가 반복되면 `symbolic_emblem`으로 전환해 스킬 작용 자체를 문양화한다.
33. oneSimpleEffect가 빠졌지만 primary는 올바를 때만 `Edit image`로 한 번 보강한다. 지시는 `add`, `remove`, `change`, `replace` 중 하나로 시작하고 도구 또는 문양의 연결 지점을 포함하는 한 문장만 사용한다. `Edit image`로 흰색 캔버스 전체를 배경으로 대체하려 하지 않는다.
34. exactCountElements는 생성·편집 프롬프트에 맡기지 않고 결정적 픽셀 오버레이로 추가한다. 각 요소는 최소 4×4px, 간격 최소 4px이다.
35. arcs/rings는 최종 80×80 기준 3-4px 두께로 유지한다.
36. `contextual`이면 스킬과 환경의 반응이 보이는 전통 배경 장면을, `symbolic_effect_scene`이면 스킬의 속성·방향·범위·타격 결과를 형상화한 전통 배경 장면을 보존하되 핵심 발동 형상보다 대비가 높아지지 않게 한다. 템플릿 정규화는 이미 하드 게이트를 통과한 배경의 가장자리와 프레임만 확정하며, 누락되거나 단색인 전체 내부 배경을 사후에 만들어 합격시키는 용도로 사용하지 않는다.
37. 두 모드 모두 발동 형상 또는 문양과 내부 이펙트를 중앙 64×64 영역으로 제한하고 rows/columns 0, 1, 78, 79를 템플릿 픽셀로 덮어쓴다. 내부 배경만 프레임 안쪽 전체에 존재할 수 있다.
38. 36-37은 resize/crop이 아니라 deterministic frame/background/safe-area normalization으로 기록한다.
39. 최종 80×80 이미지를 nearest-neighbor 32×32로 확인하여 발동 동작 또는 스킬 문양, direction, oneSimpleEffect, 한국 전통 모티프, 스킬 연관 backgroundSceneAction이 살아 있고 시각 계층 순서가 유지되는지 검사한다. 정규화 후에도 near-white blank ratio 2% 이하인지, 네 모서리가 frameTemplatePath의 프레임 픽셀 또는 연속된 내부 배경으로 채워졌는지, 프레임 안쪽에 흰색·투명·미도색 영역이나 균일 단색만 남지 않았는지 다시 검사한다.
40. 생성·편집·오버레이·정규화 기록을 evaluationRoot의 기존 equipmentId 구조에 보존한다.
41. normalized source가 85점 이상이고 모든 배경 하드 게이트를 Pass했을 때만 outputIconPath와 `.meta`를 반영한다.

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
- blank_canvas_failure: 넷째 문장을 full-bleed 필수 형식으로 교체 + 새 seed 1회, 반복 실패 시 `no_passing_candidate`
- solid_background_failure: 넷째 문장의 backgroundSceneAction을 구체적인 환경 반응 또는 대기 움직임으로 교체 + 새 seed 1회, 반복 실패 시 `no_passing_candidate`
- frame_background_failure: 배경 하드 게이트 통과 후보에 한해 frameTemplatePath 기반 가장자리 normalization
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
- Full-Bleed Background Coverage Check / Near-White Blank Ratio
- Four-Corner / Edge Continuity Check
- Background Scene Action / Elements / Dominant Color Ratio
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
  - blank_canvas_failure
  - solid_background_failure
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
- 마스터 컨셉의 타 문화 혼합 금지를 하드 게이트로 적용한다.
- 핵심 아웃라인보다 캐릭터·배경·장식이 강조되는 문장을 쓰지 않는다.
- 내부 배경의 대비를 낮추라는 지시를 배경을 생략하거나 문양 뒤의 작은 패치만 그리라는 의미로 해석하지 않는다.
- 내부 배경의 바탕색만 지정하고 작업을 끝내지 않는다. 스킬 효과가 환경에 남기는 흔적이나 흐름을 한국 전통 모티프와 결합한 저대비 그림이 반드시 보여야 한다.
- Transparent background Off는 full-bleed 배경을 보장하지 않는다. 불투명한 흰색 기본 캔버스도 실패다.
- 네 모서리와 가장자리까지 이어진 내부 배경 그림이 없거나 단색만 채워진 후보는 점수가 높거나 16개 중 가장 나은 후보여도 선택·저장하지 않는다.
- 스킬 효과를 여러 개 나열하지 않는다.
- 실패한 프롬프트 뒤에 좌표와 금지문을 계속 추가하지 않는다.
- 프레임·안전 여백·정확한 개수는 생성 모델에 맡기지 않는다. 내부 배경은 생성하되 최종 프레임과 안전 영역은 템플릿 정규화로 확정한다.
- gameplay JSON이나 스킬 밸런스를 수정하지 않는다.
```
