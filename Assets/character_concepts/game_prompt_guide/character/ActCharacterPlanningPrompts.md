# Act Character Planning Prompt

복사해서 agent에게 전달하기 위한 단일 생성 프롬프트입니다.

상세 규칙은 아래 가이드를 기준으로 합니다.

```text
Assets/character_concepts/game_prompt_guide/character/ActCharacterPlanningStartGuide.md
```

## Prompt

```text
ActCharacterPlanningStartGuide.md 기준으로 Act 캐릭터/몬스터 기획 산출물을 생성해줘.

Input:
- actId: act.02
- actStoryFile: Assets/Doc/Story/Act_02_Background.md
- chapterFiles:
  - Assets/Doc/Story/Chapter_06.md
  - Assets/Doc/Story/Chapter_07.md
  - Assets/Doc/Story/Chapter_08.md

Output:
- Player planning: Assets/Doc/Character/player 아래에 생성
- NPC/Boss planning: Assets/Doc/Character/{act_group_id}/npc 아래에 생성
- Assets/Doc/Character/player/{act_group_id}.player_common.json
- Assets/Doc/Character/player/*.json
- Assets/Doc/Character/{act_group_id}/{act_group_id}.common.json
- Assets/Doc/Character/{act_group_id}/npc/*.json
- Assets/Doc/Character/{act_group_id}/monster_context.{act_group_id}.json
- Assets/Doc/Character/{act_group_id}/monster_composition.chapter_XX_YY.json
- JSON 문법 및 planningRef 검증 결과

주의:
- 생성된 Act 산출물 폴더에는 JSON과 .meta만 둬.
- README, 가이드 문서, 프로세스 문서는 Act 산출물 폴더에 만들지 마.
- Assets/Doc/Story/Characters.md는 전체 스토리 공통 인물 기준이므로 Chapter별 산출물로 복사하지 마.
- Player 기획은 Assets/Doc/Character/player 아래에 둬.
- Npc/Boss 몬스터 풀은 Assets/Doc/Character/{act_group_id}/npc 아래에 둬.
- Player/Npc/Boss는 characterType으로만 사용해.
- 런타임 도메인은 항상 character를 사용해.
- BattleStoryContext나 composition에는 전체 캐릭터 데이터를 복사하지 말고 planningRef 중심으로 연결해.
```
