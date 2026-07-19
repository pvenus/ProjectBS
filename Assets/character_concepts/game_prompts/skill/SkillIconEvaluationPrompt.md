# Skill Icon Evaluation Prompt

생성된 정적 스킬 아이콘을 프로젝트 스타일, 슬롯, 등급, 가독성, 중복 여부, Unity 준비 상태 기준으로 평가하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 지정한 정적 스킬 아이콘을 평가해줘. 이미지, JSON, `.meta` 파일은 생성하거나 수정하지 말고 평가만 수행해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillIconEvaluationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md

Input:
- projectRoot: {project_root}
- skillSourcePath: {스킬_JSON_절대경로}
- equipmentId: {skill.character.character_name.grade.slot.skill_name}
- iconPath: Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
- generationRecordPath: {아이콘 생성 기록 절대경로}
- lowerGradeIconPath: {auto | 하위 등급 아이콘 절대경로 | null}
- siblingIconPaths: {auto | 같은 로드아웃 아이콘 절대경로 목록 | null}
- unityMetaPath: {auto | iconPath.meta | null}
- evaluationOutputPath: {평가_결과_txt_절대경로 | report_only}

작업:
1. skillSourcePath와 iconPath가 존재하는지 확인한다.
2. source JSON 문법과 equipmentId 일치 여부를 확인한다.
3. equipmentId를 `.` 기준으로 파싱하여 domain, characterName, grade, slot, skillName을 확정한다.
4. skill JSON에서 targeting, movement, damage, buff, debuff, effect를 읽고 예상 classification을 작성한다.
5. iconPath의 PNG 디코딩, 80×80 크기, RGBA, 단일 아이콘 여부를 확인한다.
6. iconPath가 `Assets/Resources/skill/icon/skill/{equipmentId}.icon.png`와 정확히 일치하는지 확인한다.
7. PNG의 SHA-256을 계산한다.
8. 80×80 원본과 nearest-neighbor 방식의 32×32 표시를 확인한다.
9. SkillIconEvaluationGuide.md의 Fatal Failure Conditions를 점수 계산 전에 판정한다.
10. 기본 공격, 액티브, 패시브 중 대상 slot에 맞는 시각 규칙을 평가한다.
11. Grade 1~3 중 대상 등급에 맞는 효과 밀도, 강조 수준, 계승 일관성을 평가한다.
12. 기존 아이콘을 스타일 레퍼런스로 자동 선택하거나 비교 기준으로 사용하지 않는다. SkillIconGenerationGuide.md의 Prompt-First Style Contract를 유일한 스타일 기준으로 사용한다.
13. generationRecordPath에서 PixelLab URL에 `tool=create_ui_basic`이 사용됐는지, 도구가 `Create UI elements`인지, Transparent background가 Off인지, Init Image가 비어 있는지, Width와 Height가 각각 80px인지 확인한다.
14. 최종 아이콘의 외부 프레임 2px, 주 피사체 외곽선 2px, 내부선 1px, 사방 8px 내용 안전 여백을 각각 평가한다.
15. lowerGradeIconPath가 auto이면 같은 skillName의 실제 하위 등급 아이콘을 검색한다. 선택적 하위 등급 참조가 없으면 Not Evaluated로 기록한다.
16. siblingIconPaths가 auto이면 같은 characterName과 grade의 아이콘을 검색해 32×32 구분 가능성을 비교한다. 선택적 sibling reference가 없으면 Not Evaluated로 기록한다.
17. 대상 PNG와 sibling 아이콘의 SHA-256을 비교해 명시적으로 승인되지 않은 바이트 동일 재사용을 확인한다.
18. 가이드의 6개 항목을 100점 만점으로 채점한다.
19. 치명적 실패, 증거 부족, 총점을 기준으로 Pass / Conditional Pass / Fail을 판정한다.
20. 모든 감점과 실패에 대해 관찰 증거, 위반 규칙, 필요한 수정, 재생성 필요 여부를 작성한다.
21. 재생성이 필요하면 기존 PixelLab 영어 description에서 변경할 최소 문구를 제안한다.
22. unityMetaPath가 존재하면 import 준비 상태를 확인하되 Unity Editor 반영을 확인할 수 없으면 pending으로 기록한다.
23. evaluationOutputPath가 report_only가 아니면 평가 리포트 텍스트만 해당 경로에 저장하며, iconPath, skillSourcePath, unityMetaPath와 다른 프로젝트 자산은 수정하지 않는다.

Output:
- Skill Icon Evaluation
- Skill ID
- Source JSON
- Icon Path
- Unity Meta Path
- Grade
- Slot
- Expected Classification
- Canvas / Mode
- SHA-256
- Generation Record Path
- Lower Grade Icon
- Sibling Icons
- Fatal Failure Check
- Skill Intent Readability: /25
- Project Style Match: /20
- Small-Size Silhouette: /20
- Slot and Grade Distinction: /15
- Palette and Contrast: /10
- Composition and Border Quality: /10
- Total: /100
- Result: Pass / Conditional Pass / Fail
- Failure Reasons
- Required Corrections
- Regeneration Prompt Changes
- Unity Import Status
- Evaluation Output Path
- Notes

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
  - insufficient_evidence
  - evaluation_write_failed
- 평가하지 못한 항목
- 실패 원인
- 부족한 입력
- 다음에 필요한 작업

선택적 참조 처리:
- lowerGradeIconPath, siblingIconPaths를 찾을 수 없으면 관련 비교만 Not Evaluated로 기록한다.
- 필수 파일, 필수 계약, 판정에 필요한 증거가 부족한 경우에만 insufficient_evidence로 실패 처리한다.

검증:
- 평가는 SkillIconEvaluationGuide.md의 치명적 실패를 먼저 판정해야 한다.
- generationRecordPath가 없거나 UI Basic 설정을 증명하지 못하면 Pass 처리하지 않아야 한다.
- PNG는 80×80과 32×32에서 모두 확인해야 한다.
- 점수 합계는 정확히 100점 만점이어야 한다.
- 85점 이상이고 치명적 실패와 필수 증거 부족이 없을 때만 Pass여야 한다.
- 하위 등급 아이콘이 실제로 존재할 때만 계승 일관성을 Pass 또는 Fail로 판정해야 한다.
- siblingIconPaths를 확인할 수 없으면 중복 및 로드아웃 구분 항목을 Not Evaluated로 기록해야 한다.
- 선택적 lower/sibling 이미지 부재만으로 insufficient_evidence 실패 처리하지 않아야 한다.
- 명시적 재사용 승인 없이 바이트 동일 아이콘이 발견되면 Fail이어야 한다.
- 모든 감점에는 관찰 증거와 수정 방향이 있어야 한다.
- 평가 중 이미지, JSON, `.meta` 파일을 생성하거나 수정하지 않아야 한다.
- PixelLab 생성 API를 호출하지 않아야 한다.

주의:
- 이 프롬프트는 평가 전용이다.
- 실패 아이콘을 직접 수정하거나 재생성하지 않는다.
- 확인할 수 없는 항목을 추정으로 Pass 처리하지 않는다.
- 16×16 확인은 선택적 스트레스 테스트이며 Pass 필수 조건이 아니다.
- Unity Editor import를 직접 확인하지 못하면 완료로 보고하지 않는다.
- evaluationOutputPath에는 평가 리포트 텍스트 외의 파일을 생성하지 않는다.
```
