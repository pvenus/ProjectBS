# 에피소드 스토리 입력 포맷

- formatId: `design.format.episode_story`
- version: `6`

## 목적

기획 단계에서 원본 지문과 팝업의 영구 정체성을 보존하고, 이후 Stage
JSON·문자열·이미지·PopupEventSO가 같은 ID를 사용하도록 정의한다.

## 핵심 원칙

- 원본 지문은 `sourceNarration`에 그대로 보존한다.
- 모든 신규 팝업은 기획 단계에서 고유한 `popupName`을 받는다.
- `popupId`는 `popupName`으로부터 한 번 생성한 뒤 변경하지 않는다.
- 표시 순서는 `popupOrder`로 관리하며 ID에 인덱스를 넣지 않는다.
- Stage 변환은 `nodes[].nodeId`에 planning `popupId`를 그대로 복사한다.
- 기존 숫자/순번형 ID는 영구 레거시 ID로 보존한다.

## ID 규칙

### popupName

```text
lowercase semantic snake_case
```

좋은 예:

```text
village_arrival
black_cloth_attack
rescue_choice
```

금지 예:

```text
popup_1
scene_2
event_003
```

### popupId

```text
node.{act_key}.{chapter_key}.{episode_key}.{popupName}
```

예:

```text
node.act1.chapter01.episode01.village_arrival
```

### choiceId

선택지도 배열 인덱스를 사용하지 않는다.

```text
choice.{act_key}.{chapter_key}.{episode_key}.{popupName}.{choiceName}
```

예:

```text
choice.act1.chapter01.episode01.black_cloth_attack.rescue_villagers
```

## 필드 정의

| 필드 | 의미 |
|---|---|
| `titleKo` | 에피소드 제목 |
| `summaryKo` | 에피소드 요약 |
| `themeKo` | 에피소드 기능 또는 주제 |
| `sourceNarration` | 원본 파일과 원문 block 목록 |
| `sourceNarrationId` | 원문 block의 영구 의미 기반 ID |
| `originalTextKo` | 변형하지 않은 원본 지문 |
| `popupDefinitions` | 기획 단계의 영구 팝업 정의 목록 |
| `nodes` | Stage 변환 단계에서 생성하는 런타임 호환 목록 |
| `popupName` | 에피소드 안에서 고유한 영구 의미 이름 |
| `popupNameKo` | 사람이 읽는 변경 가능한 검토 이름 |
| `popupId` | 공식 생성식으로 만든 영구 팝업 ID |
| `nodeId` | Builder 호환 필드. Stage 변환이 `popupId`를 그대로 복사함 |
| `popupOrder` | 변경 가능한 표시/검토 순서. ID가 아님 |
| `popupType` | narration, dialogue, choice, transition 등 분류 |
| `bodyKo` | 실제 팝업 표시 지문 |
| `nextPopupId` | 기획 단계의 다음 팝업 영구 ID. 마지막이면 `null` |
| `nextNodeId` | Stage 변환이 `nextPopupId`를 복사하는 Builder 호환 필드 |
| `imagePolicy` | `generate`, `reuse`, `none` |
| `choices` | 선택지 목록 |
| `choiceName` | 팝업 안에서 고유한 의미 기반 선택지 이름 |
| `choiceId` | 공식 생성식으로 만든 영구 선택지 ID |
| `labelKo` | 선택지 버튼 문구 |
| `resultKo` | 선택 직후 표시되는 결과 지문 |
| `rewards` | 선택으로 발생하는 결과 |
| `rewardType` | 결과 종류 |
| `rewardId` | 명시적인 안정적 대상 ID |
| `rewardOwner` | 실제 지급 시스템. `battle` 또는 `popup` |
| `rewardTrigger` | 지급 시점. `battle_clear`, `choice_confirm`, `episode_clear`, `chapter_clear` |

## 입력 예시

```json
{
  "titleKo": "청운촌의 습격",
  "summaryKo": "서진이 청운촌에 도착해 주민을 구한다.",
  "themeKo": "도입, 구조, 전투 진입",
  "sourceNarration": {
    "sourceEpisodeFile": "Assets/Doc/Story/Act01/Chapter01/01_episode1.md",
    "blocks": [
      {
        "sourceNarrationId": "narration.act1.chapter01.01.village_arrival",
        "sourceOrder": 100,
        "originalTextKo": "청운촌에 가까워질수록 공기는 무겁고 메말랐다."
      }
    ]
  },
  "popupDefinitions": [
    {
      "popupName": "village_arrival",
      "popupNameKo": "청운촌 도착",
      "popupId": "node.act1.chapter01.episode01.village_arrival",
      "popupOrder": 100,
      "popupType": "narration",
      "sourceNarrationIds": [
        "narration.act1.chapter01.01.village_arrival"
      ],
      "nextPopupId": "node.act1.chapter01.episode01.black_cloth_attack",
      "imagePolicy": "generate",
      "choices": []
    },
    {
      "popupName": "black_cloth_attack",
      "popupNameKo": "검은 천 무리의 습격",
      "popupId": "node.act1.chapter01.episode01.black_cloth_attack",
      "popupOrder": 200,
      "popupType": "choice",
      "nextPopupId": null,
      "imagePolicy": "generate",
      "choices": [
        {
          "choiceName": "rescue_villagers",
          "choiceId": "choice.act1.chapter01.episode01.black_cloth_attack.rescue_villagers",
          "labelKo": "마을 사람들을 구한다",
          "resultKo": "서진은 검은 천의 무리 사이로 뛰어들었다.",
          "nextPopupId": "node.act1.chapter01.episode01.rescue_start",
          "rewardOwner": "battle",
          "rewardTrigger": "battle_clear",
          "rewardIntent": [
            "gold_battle_reward"
          ],
          "rewards": [
            {
              "rewardType": "SpecialBattle",
              "rewardId": "battle.act1.chapter01.01.rescue_villagers"
            }
          ]
        }
      ]
    }
  ]
}
```

## 작성 규칙

1. 신규 팝업은 모두 `popupName`, `popupId`, `popupOrder`를 가진다.
2. `popupName`과 `choiceName`은 의미 기반 snake_case를 사용한다.
3. `popupId`는 공식 생성식과 일치해야 한다.
4. Stage 변환은 신규 `nodeId`에 `popupId`를 그대로 복사해야 한다.
5. `nextPopupId`는 대상 `popupId`를 사용하고 마지막이면 `null`로 둔다. Stage 변환은 이를 `nextNodeId`로 복사한다.
6. `popupOrder` 변경으로 ID를 바꾸지 않는다.
7. 9줄×40자 제한 때문에 분할할 팝업도 기획 단계에서 각각 이름을 받는다.
8. 이름 없는 자동 팝업이나 배열 인덱스 기반 ID를 만들지 않는다.
9. `reuse` 이미지 정책은 기존 `imageSourcePopupId`를 명시하고 원본 PNG를 변경 없이 새 popupId 경로로 복사한다.
10. 보상 종류와 지급 주체를 분리한다. `rewardType: gold`만으로 popup 지급을 추론하지 않는다.
11. `rewardOwner: battle` 또는 `gold_battle_reward`는 popup `Gold` payload로 만들지 않는다. 팝업의 `SpecialBattle`/`BossBattle`은 battleId를 참조하는 전투 진입 action일 뿐 전투 클리어 보상이 아니다.
12. popup `Gold`는 `rewardOwner: popup`과 명시적인 popup 실행 trigger가 있을 때만 작성한다. 소유권이 불명확하면 실패한다.
13. 기존 순번형 ID와 연결 자산은 자동 변경하지 않는다.
