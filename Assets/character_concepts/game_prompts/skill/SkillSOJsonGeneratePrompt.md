# Skill SO JSON Generate Prompt

Use this prompt when generating character skill JSON for ProjectBS Skill SO input.

```text
작업 폴더 = {project_root}

아래 입력을 기준으로 CharacterCreateGuide.md, SkillDegineGuide.md, SkillBalanceGuide.md, SkillJsonGuide.md, EquipmentSkillSO.md 및 필요한 SO 세부 가이드를 먼저 읽고, 캐릭터 기획 JSON의 skills를 기준으로 Skill SO 입력용 JSON만 생성해줘.

Input:
- projectRoot: {project_root}
- characterPlanningJson = {캐릭터별_기획_JSON_절대경로}
- commonDataJson = {공용_데이터_JSON_절대경로_또는_null}
- outputRoot = Assets/Resources/skill/character/generated

참조 가이드:
- Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
- Assets/character_concepts/game_prompt_guide/skill/design/SkillBalanceGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentBaseProfileSO.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillCastSO.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillHitSO.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillMoveSO.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/SpawnSkillSO.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/BaseVisualSO.md
- Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentUpgradeTableSO.md

작업:
1. characterPlanningJson을 읽고 identity, combat, stats, skills 정보를 기준으로 스킬 JSON을 생성한다.
2. commonDataJson 또는 commonDataRef가 있으면 race, faction, world tone, reuse/source guide 정보를 참고하되, 출력 JSON에는 Skill SO 입력에 필요한 필드만 작성한다.
3. SO asset을 직접 만들지 말고 JSON 파일만 생성한다.
4. equipmentId는 반드시 skill.character.{character_name}.{grade}.{slot}.{skill_name} 형식을 사용한다.
5. Player, Npc, Boss 모두 CharacterSO-linked skill로 간주하고 skill.character 도메인을 사용한다. skill.npc 도메인은 사용하지 않는다.
6. slot은 source design JSON의 skills slot을 기준으로 SkillJsonGuide.md의 slot mapping에 맞춘다.
7. cast.range는 최소 0.4 이상으로 작성한다.
8. normal NPC는 upgradeTable을 생성하지 않는다. 업그레이드가 명시적으로 필요한 경우에도 SkillJsonGuide.md의 Upgrade Table Scope를 먼저 따른다.
9. 필요한 optional profile만 작성하고, 필요 없는 hits, move, spawnSkill, upgradeTable, baseVisual은 생략한다.
10. child SO id는 equipmentId를 기준으로 SkillJsonGuide.md의 ID Derivation 규칙에 따라 생성한다.
11. JSON에는 localization string, Unity SO asset, `.meta` 파일을 생성하지 않는다.
12. 결과 JSON은 outputRoot 아래에 저장한다.
13. hits에서 maxHitCount가 1보다 크면 deactivateAfterFirstHit는 반드시 false로 작성한다.
14. deactivateAfterFirstHit가 true이면 maxHitCount는 반드시 1로 작성한다.

Output:
- Character:
- Character Type:
- Grade:
- Source Planning JSON:
- Output Folder:
- Generated Skill Files:
- Skill IDs:
- Optional Profiles Used:
- Upgrade Table Generated:
- Validation:
- Pass / Fail:
- Failure Reason:
- Notes:

실패 시 Output:
- status: failed
- failureType:
  - missing_character_planning_json
  - invalid_character_planning_json
  - missing_skill_design
  - invalid_skill_slot
  - invalid_skill_schema
  - output_write_failed
- 실패 원인
- 보강이 필요한 planning skills 필드
- 생성하지 않은 산출물

검증:
- 결과 JSON 문법이 유효해야 한다.
- equipmentId는 skill.character.{character_name}.{grade}.{slot}.{skill_name} 형식을 따라야 한다.
- slot은 SkillJsonGuide.md의 mapping과 맞아야 한다.
- cast.range는 최소 0.4 이상이어야 한다.
- 모든 hit는 `maxHitCount > 1 && deactivateAfterFirstHit == true` 충돌이 없어야 한다.
- 필요한 optional profile만 생성해야 한다.
- Unity SO asset, localization, `.meta` 파일은 생성하지 않아야 한다.

주의:
- 세부 schema와 판단 기준은 프롬프트가 아니라 참조 가이드를 원본으로 따른다.
- 캐릭터 기획 JSON의 skills에 없는 스킬을 임의로 추가하지 않는다.
- normal NPC의 난이도 보정은 upgradeTable이 아니라 stats, grade, tierId, encounter composition 기준으로 해석한다.
- Skill SO JSON 생성 외의 리소스 생성, Unity asset 생성, localization 생성은 수행하지 않는다.
```
