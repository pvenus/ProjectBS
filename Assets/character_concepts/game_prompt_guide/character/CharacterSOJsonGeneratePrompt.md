# Character SO JSON Generate Prompt

Use this prompt when generating CharacterSO input JSON from a character planning JSON.

```text
아래 입력을 기준으로 CharacterCreateGuide.md, CharacterSO.md, CharacterStatGuide.md, StatEnum.md를 먼저 읽고, 캐릭터 기획 JSON을 CharacterSO 입력용 JSON으로 변환해줘.

입력:
- characterPlanningJson = {캐릭터별_기획_JSON_절대경로}
- commonDataJson = {공용_데이터_JSON_절대경로_또는_null}
- outputRoot = Assets/Resources/character/json

참조 가이드:
- Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterSO.md
- Assets/character_concepts/game_prompt_guide/character/CharacterStatGuide.md
- Assets/character_concepts/game_prompt_guide/character/StatEnum.md

핵심 실행 지시:
1. characterPlanningJson을 읽고 identity, combat, planningScore, stats 정보를 기준으로 CharacterSO 입력 JSON을 생성한다.
2. commonDataJson 또는 commonDataRef가 있으면 race, faction, world tone, reuse/source guide 정보를 참고하되, 출력 JSON에는 CharacterSO 입력에 필요한 필드만 작성한다.
3. characterId는 반드시 character.{character_name}.{grade} 형식을 사용한다.
4. characterType은 Player, Npc, Boss 중 하나만 사용한다.
5. job은 CharacterSO.md의 CharacterJob enum 값 중 하나를 정확히 사용한다.
6. baseStats는 StatEnum.md에 존재하는 statType만 사용한다.
7. baseStats는 CharacterStatGuide.md의 밸런스 기준과 캐릭터 기획 JSON의 planningScore/stats 성향을 함께 반영한다.
8. animationClips, skills, localization은 JSON에 직접 넣지 않는다.
9. CharacterSO asset, AnimationClip asset, Skill SO asset, localization string, `.meta` 파일은 생성하지 않는다.
10. 결과 JSON은 outputRoot 아래에 {characterId}.json 파일명으로 저장한다.

출력 형식:
- Character:
- Character ID:
- Character Type:
- Job:
- Source Planning JSON:
- Output File:
- Base Stats:
- Excluded Auto Fields:
- Validation:
- Pass / Fail:
- Failure Reason:
- Notes:

주의:
- 세부 schema와 판단 기준은 프롬프트가 아니라 참조 가이드를 원본으로 따른다.
- Player/Npc/Boss는 characterType으로만 사용하고 characterId 도메인은 항상 character로 유지한다.
- StatEnum.md에 없는 statType은 임의로 만들지 않는다.
- CharacterSO 입력 JSON 생성 외의 리소스 생성은 수행하지 않는다.
```
