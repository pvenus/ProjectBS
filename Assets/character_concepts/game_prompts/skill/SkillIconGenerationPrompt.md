# Skill Icon Generation Prompt

스킬 JSON과 기존 프로젝트 아이콘을 기준으로 PixelLab에서 정적 스킬 아이콘 하나를 생성하고 검증하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 지정한 스킬의 80×80 정적 아이콘을 PixelLab Simple Creator의 `Create UI elements` 메뉴에서 프롬프트만으로 생성하고 프로젝트 경로에 저장해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md

Input:
- projectRoot: {project_root}
- skillSourcePath: {스킬_JSON_절대경로}
- equipmentId: {skill.character.character_name.grade.slot.skill_name}
- pixelLabCreatorUrl: https://www.pixellab.ai/create?tool=create_ui_basic
- pixelLabTool: Create UI elements
- referenceMode: prompt_only
- inheritedIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- outputIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- generationMode: pixelLabSimpleCreatorUi
- finalIconSize: 80x80
- contentSafeMarginRatio: 0.10
- maxRegenerationCount: 1

작업:
1. skillSourcePath가 존재하고 유효한 JSON인지 확인한다.
2. JSON의 equipmentId가 입력 equipmentId와 정확히 일치하는지 확인한다.
3. equipmentId를 `.` 기준으로 파싱하여 domain, characterName, grade, slot, skillName을 확정한다.
4. grade가 1~3인지 확인하고, slot이 source design과 runtime에서 허용되는 값인지 확인한다.
5. skill JSON에서 skillType, targetingType, castMove, componentType, moveType, damage, buff, debuff, effect를 읽는다.
6. SkillIconGenerationGuide.md의 우선순위에 따라 slotFamily, visualFamily, primarySymbol, secondaryEffect, composition, elementFamily, roleFamily, paletteFamily, intensity를 결정한다.
7. `Assets/Resources/skill/icon/skill`의 기존 아이콘을 스타일 참조 또는 Init Image로 자동 선택하지 않는다. 이번 생성의 스타일 기준은 SkillIconGenerationGuide.md의 Prompt-First Style Contract뿐이다.
8. inheritedIconPath가 auto이고 같은 이름의 하위 등급 아이콘이 존재하면 화면으로 분석하여 primarySymbol, 방향, 기본 팔레트만 텍스트 프롬프트에 반영한다. 하위 등급 파일도 Init Image로 업로드하지 않는다. 없으면 null로 기록한다.
9. PixelLab의 Init Image는 비워 두며 gallery, clipboard, local upload에서 어떤 이미지도 첨부하지 않는다.
10. 계승 스킬이면 하위 등급 아이콘의 primarySymbol, 방향, 기본 팔레트를 유지하고 현재 등급에 맞는 효과만 강화한다.
11. 가이드의 Prompt Construction과 Prompt-First Style Contract 순서에 맞춰 `Description` 입력란에 넣을 하나의 영어 프롬프트를 작성한다. 별도의 `style_description`은 만들지 않는다.
12. `pixelLabCreatorUrl`을 열고 현재 URL이 `tool=create_ui_basic`인지, 선택된 도구명이 `Create UI elements`인지 확인한다. 다른 도구로 자동 전환되었다면 Change 메뉴에서 `Create UI elements`를 다시 선택한다.
13. `Description`에는 생성한 영어 프롬프트 하나만 입력한다.
14. `Transparent background`를 끄고, Init Image는 비워 둔다.
15. Width와 Height를 각각 정확히 80px로 설정한다. 128, 256, 320 등 다른 크기로 생성한 뒤 잘라내거나 축소하는 방식은 사용하지 않는다.
16. Generate를 한 번 실행한다. `Create UI elements`는 실행당 단일 이미지를 생성하므로 각 실행 결과를 하나의 후보로 기록한다.
17. 완료된 후보를 임시 평가 폴더에 수정 없이 다운로드하고 실제 파일 크기를 확인한다.
18. 후보가 정확히 80×80 RGBA가 아니면 자르기, 리사이즈, 캔버스 확장으로 보정하지 말고 기술 실패로 기록한다.
19. 후보에서 외부 프레임 2px, 주 피사체 외곽선 2px, 내부선 1px, 사방 8px 이상의 내용 안전 여백을 확인하고 80×80과 32×32에서 Candidate Scoring으로 평가한다.
20. 치명적 실패가 없고 85점 이상인 후보 중 최고 점수를 선택한다.
21. 합격 후보가 없으면 실패 원인에 맞게 description을 수정하고 maxRegenerationCount 범위에서만 재생성한다.
22. 합격한 80×80 후보를 outputIconPath에 `{equipmentId}.icon.png` 이름으로 저장한다.
23. 프로젝트의 기존 스킬 아이콘 import 설정과 맞는 `.png.meta`를 생성 또는 갱신한다.
24. PNG 디코딩, 80×80 크기, RGBA, 리소스 키, Unity import 설정을 검증한다.
25. 최종 아이콘이 다른 스킬 아이콘과 바이트가 동일하면 명시적 재사용 승인 없이 완료 처리하지 않는다.
26. 최종 선택 후보, generation record, candidate score 요약은 보존하고, 최종 파일과 기록을 확인한 뒤 중복 다운로드, 실패 후보, 중간 파일만 정리한다.

Output:
- Skill ID
- Source JSON
- Output Icon Path
- Grade
- Slot
- Classification
- Reference Mode
- Inherited Icon Reference
- PixelLab Creator URL
- PixelLab Tool
- PixelLab Description
- Requested Width / Height
- Actual Downloaded Width / Height
- Content Safe Margin
- Transparent Background
- Init Image
- Candidate Count
- Candidate Scores
- Selected Candidate
- Generation Record Path
- Candidate Score Summary Path
- Regeneration Performed
- PNG Validation
- Unity Meta Status
- Final Score
- Pass / Conditional Pass / Fail
- Failure Reasons
- Cleanup Status

실패 시 Output:
- status: failed
- failureType:
  - missing_skill_json
  - invalid_skill_json
  - equipment_id_mismatch
  - invalid_grade
  - unsupported_slot
  - missing_style_reference
  - pixellab_unavailable
  - pixellab_authentication_failed
  - insufficient_pixellab_credits
  - wrong_pixellab_tool
  - invalid_ui_settings
  - generation_timeout
  - no_passing_candidate
  - output_write_failed
  - unity_import_pending
- 생성하지 않은 파일
- 실패 원인
- 부족한 입력 또는 인증 상태
- 마지막 PixelLab 결과 식별 정보
- 마지막으로 사용한 description
- 다음에 필요한 작업

검증:
- 최종 PNG 경로는 `Assets/Resources/skill/icon/skill/{equipmentId}.icon.png`와 정확히 일치해야 한다.
- 최종 PNG는 80×80 RGBA 이미지여야 한다.
- 아이콘은 32×32 표시에서도 primarySymbol이 식별 가능해야 한다.
- 기본 공격, 액티브, 패시브의 슬롯 시각 규칙을 따라야 한다.
- Grade 1~3의 효과 밀도와 강조 수준이 가이드에 맞아야 한다.
- 계승 스킬은 하위 등급의 primarySymbol과 방향성을 유지해야 한다.
- 텍스트, 문자, 숫자, 로고, 실사 표현, 부드러운 벡터 표현, 애니메이션 격자가 없어야 한다.
- 배경과 테두리가 SkillIconGenerationGuide.md의 Prompt-First Style Contract와 일치해야 한다.
- 기존 아이콘 폴더의 이미지를 스타일 참조 또는 Init Image로 사용하면 안 된다.
- 최종 80×80 기준 외부 프레임은 2px, 주 피사체 외곽선은 2px, 내부선은 1px을 기본 계약으로 사용해야 한다.
- 프레임을 제외한 주 피사체와 효과는 사방 8px 안전 영역을 침범하지 않아야 한다.
- PixelLab `Create UI elements`에서 Width와 Height를 직접 80px로 설정해야 하며 결과를 잘라내거나 리사이즈해서 맞추면 안 된다.
- PixelLab 후보 평가 점수는 85점 이상이고 치명적 실패가 없어야 한다.
- 최종 아이콘과 `.png.meta`가 존재하고 예상 리소스 키로 해석 가능해야 한다.

주의:
- 생성은 반드시 PixelLab Simple Creator의 `Create UI elements` 메뉴에서만 수행한다.
- `Create from style reference (Pro)`, `Create UI elements (Pro)`, `Create M-XL image` 및 API 생성 엔드포인트로 대체하지 않는다.
- 이 프롬프트는 정적 아이콘 전용이며 스킬 VFX, 애니메이션 시트, 캐릭터 스프라이트를 생성하지 않는다.
- gameplay JSON이나 스킬 밸런스를 아이콘에 맞춰 수정하지 않는다.
- source JSON에 없는 slot, element, effect를 임의로 만들지 않는다.
- 실패 시 placeholder 아이콘을 생성하지 않는다.
- 기존 합격 아이콘을 덮어쓸 때는 명시적인 교체 승인이 있어야 한다.
```
