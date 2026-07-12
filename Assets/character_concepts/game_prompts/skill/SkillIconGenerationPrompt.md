# Skill Icon Generation Prompt

스킬 JSON과 기존 프로젝트 아이콘을 기준으로 PixelLab에서 정적 스킬 아이콘 하나를 생성하고 검증하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 지정한 스킬의 80×80 정적 아이콘을 PixelLab에서 생성하고 프로젝트 경로에 저장해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md

Input:
- projectRoot: {project_root}
- skillSourcePath: {스킬_JSON_절대경로}
- equipmentId: {skill.character.character_name.grade.slot.skill_name}
- styleReferencePaths: {auto | 쉼표로 구분한 기존 아이콘 절대경로 목록}
- inheritedIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- outputIconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- generationMode: pixelLabApiV2
- maxRegenerationCount: 1

작업:
1. skillSourcePath가 존재하고 유효한 JSON인지 확인한다.
2. JSON의 equipmentId가 입력 equipmentId와 정확히 일치하는지 확인한다.
3. equipmentId를 `.` 기준으로 파싱하여 domain, characterName, grade, slot, skillName을 확정한다.
4. grade가 1~3인지 확인하고, slot이 source design과 runtime에서 허용되는 값인지 확인한다.
5. skill JSON에서 skillType, targetingType, castMove, componentType, moveType, damage, buff, debuff, effect를 읽는다.
6. SkillIconGenerationGuide.md의 우선순위에 따라 slotFamily, visualFamily, primarySymbol, secondaryEffect, composition, elementFamily, roleFamily, paletteFamily, intensity를 결정한다.
7. styleReferencePaths가 auto이면 Assets/Resources/skill/icon/skill에서 slot, composition, role, grade intensity가 가까운 승인 아이콘을 1~4개 선택한다.
8. 스타일 참조 후보에서 바이트가 동일한 중복 이미지를 제거하고, 최종 참조 경로를 기록한다.
9. inheritedIconPath가 auto이고 같은 이름의 하위 등급 아이콘이 존재하면 이를 계승 참조로 사용한다. 없으면 null로 기록한다.
10. 계승 스킬이면 하위 등급 아이콘의 primarySymbol, 방향, 기본 팔레트를 유지하고 현재 등급에 맞는 효과만 강화한다.
11. 가이드의 Prompt Construction 순서에 맞춰 PixelLab API v2 입력용 영어 `description`과 `style_description`을 작성한다.
12. 안정적인 characterName hash, slot offset, grade offset으로 numeric seed를 계산한다.
13. PixelLab API v2의 `POST /v2/generate-with-style-v2`를 사용한다.
14. 요청값은 `image_size` 80×80, `no_background: false`, `description`, `style_description`, `seed`와 선택한 참조별 `style_images[].image.base64`, `style_images[].width`, `style_images[].height`로 설정한다.
15. PixelLab API token은 환경 변수 또는 기존 secret 설정에서만 읽는다. 토큰 값은 Input, Output, 로그, 프롬프트, 생성 기록에 출력하지 않는다.
16. background_job_id를 기록하고 5~10초 간격으로 완료 상태를 확인한다.
17. 완료된 모든 후보를 임시 평가 폴더에 저장해 평가하고 첫 번째 후보를 자동 채택하지 않는다.
18. 각 후보를 80×80과 32×32에서 확인하고 SkillIconGenerationGuide.md의 Candidate Scoring으로 평가한다.
19. 치명적 실패가 없고 85점 이상인 후보 중 최고 점수를 선택한다.
20. 합격 후보가 없으면 실패 원인에 맞게 description을 수정하고 maxRegenerationCount 범위에서만 재생성한다.
21. 합격 후보를 outputIconPath에 `{equipmentId}.icon.png` 이름으로 저장한다.
22. 프로젝트의 기존 스킬 아이콘 import 설정과 맞는 `.png.meta`를 생성 또는 갱신한다.
23. PNG 디코딩, 80×80 크기, RGBA, 리소스 키, Unity import 설정을 검증한다.
24. 최종 아이콘이 다른 스킬 아이콘과 바이트가 동일하면 명시적 재사용 승인 없이 완료 처리하지 않는다.
25. 최종 선택 후보, generation record, candidate score 요약은 보존하고, 최종 파일과 기록을 확인한 뒤 중복 다운로드, 실패 후보, 중간 파일만 정리한다.

Output:
- Skill ID
- Source JSON
- Output Icon Path
- Grade
- Slot
- Classification
- Style Reference Paths
- Inherited Icon Reference
- PixelLab Endpoint
- PixelLab Description
- PixelLab style_description
- Canvas Size
- No Background
- Seed
- Background Job ID
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
  - invalid_pixellab_request
  - pixellab_concurrency_limit
  - generation_timeout
  - no_passing_candidate
  - output_write_failed
  - unity_import_pending
- 생성하지 않은 파일
- 실패 원인
- 부족한 입력 또는 인증 상태
- 마지막 PixelLab background_job_id
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
- 배경과 테두리가 기존 프로젝트 스킬 아이콘 스타일과 일치해야 한다.
- PixelLab 후보 평가 점수는 85점 이상이고 치명적 실패가 없어야 한다.
- 최종 아이콘과 `.png.meta`가 존재하고 예상 리소스 키로 해석 가능해야 한다.
- PixelLab API token이 어떤 산출물이나 로그에도 포함되지 않아야 한다.

주의:
- 생성은 반드시 PixelLab에서만 수행한다.
- `Generate with style (Pro)` 이외의 도구는 가이드가 허용한 보조 작업에만 사용한다.
- 이 프롬프트는 정적 아이콘 전용이며 스킬 VFX, 애니메이션 시트, 캐릭터 스프라이트를 생성하지 않는다.
- gameplay JSON이나 스킬 밸런스를 아이콘에 맞춰 수정하지 않는다.
- source JSON에 없는 slot, element, effect를 임의로 만들지 않는다.
- 실패 시 placeholder 아이콘을 생성하지 않는다.
- 기존 합격 아이콘을 덮어쓸 때는 명시적인 교체 승인이 있어야 한다.
```
