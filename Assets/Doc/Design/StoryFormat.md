# 에피소드 스토리 입력 포맷

- formatId: `design.format.episode_story`
- version: `4`

## 목적

기획자가 AI에게 에피소드 스토리를 전달할 때 사용할 최소 입력 포맷이다. 현재 episode JSON에 이미 존재하는 필드 중 기획자가 신경 쓸 만한 필드만 정리한다.

## 포함 필드

- `titleKo`
- `summaryKo`
- `themeKo`
- `nodes`
- `nodeId`
- `bodyKo`
- `nextNodeId`
- `choices`
- `choiceId`
- `labelKo`
- `resultKo`
- `rewards`
- `rewardType`
- `rewardId`
- `battle`

## 필드 정의

| 필드 | 의미 | 예시 / 비고 |
| --- | --- | --- |
| `titleKo` | 에피소드 제목 | `단서 조사` |
| `summaryKo` | 에피소드 전체 요약 | `서진이 마을 사람들의 증언과 숲 입구의 흔적을 조사하며 다음 분기를 선택한다.` |
| `themeKo` | 에피소드의 주제 또는 기능 | `조사 준비, 분기 선택, 전투 진입, 동료 합류` |
| `nodes` | 플레이어에게 보여줄 지문 목록 |  |
| `nodeId` | 지문을 구분하기 위한 숫자 ID | `1` |
| `bodyKo` | 실제로 화면에 표시될 지문 | `광장에는 불안한 사람들이 모여 있었다. 누군가는 아이의 마지막 말을 이야기했고, 누군가는 숲 입구에 남은 발자국을 가리켰다.` |
| `nextNodeId` | 다음에 이어질 지문의 `nodeId`를 숫자로 적는다. 마지막 지문이면 `0`으로 둔다. | `2` |
| `choices` | 플레이어 선택지 목록 |  |
| `choiceId` | `choices` 배열 안의 각 선택지를 구분하기 위한 ID. 모든 선택지는 `choiceId`를 가진다. | `stage.node.choice.1` |
| `labelKo` | 선택지 버튼 문구 | `아이의 가족에게 직접 묻는다` |
| `resultKo` | 선택 직후의 결과 설명 | `서진은 아이가 남긴 마지막 말과 붉은 천 인형에 대해 듣는다.` |
| `rewards` | 선택으로 발생하는 결과 목록. 분기 해금, 전투 진입, 동료 후보 획득 같은 것을 적는다. |  |
| `rewardType` | 보상 또는 결과의 종류 | 아래 `rewardType` 값을 참고한다. |
| `rewardId` | 보상이 가리키는 대상. 기획 단계에서는 다음 에피소드, 분기, 캐릭터, 전투 이름을 적는다. | `episode3-1` |
| `battle` | 전투가 발생하는 선택지에서 사용하는 전투 정보. 상세 수치는 전투 포맷에서 따로 관리할 수 있으므로 스토리 포맷에서는 전투 이름과 의도만 적어도 된다. |  |

## choiceId 형식

기본 형식은 `카테고리.도메인.이름.인덱스`이다.

| 파트 | 설명 |
| --- | --- |
| 카테고리 | 큰 분류. 예: `stage` |
| 도메인 | 데이터 영역. 예: `node` |
| 이름 | 선택지 성격 또는 이름. 예: `choice` |
| 인덱스 | 같은 지문 안에서의 선택지 번호. 예: `1` |

전투가 발생하는 선택지의 경우에도 별도 전투 ID를 직접 적지 않고, 이 `choiceId`를 기준으로 `battleId`를 추론한다.

## rewardType 값

| 값 | 사용할 때 | rewardId 규칙 | 예시 |
| --- | --- | --- | --- |
| `UnlockRoute` | 선택 결과로 다음 분기나 다른 에피소드 경로가 열릴 때 사용한다. | 열릴 에피소드나 경로 이름을 적는다. | `{"rewardType":"UnlockRoute","rewardId":"episode3-1"}` |
| `SpecialBattle` | 선택 결과로 특수 전투가 바로 시작될 때 사용한다. | 작성하지 않는다. convert 단계에서 `choiceId`를 기준으로 `battleId`를 만든다. | `{"choiceId":"stage.node.choice.1","rewardType":"SpecialBattle"}` |
| `battle` | 선택 결과로 일반 전투가 발생할 때 사용한다. | 작성하지 않는다. convert 단계에서 `choiceId`를 기준으로 `battleId`를 만든다. | `{"choiceId":"stage.node.choice.2","rewardType":"battle"}` |
| `party_candidate` | 선택 결과로 동료 후보 또는 합류 인물이 열릴 때 사용한다. | 인물 이름이나 인물 참조명을 적는다. | `{"rewardType":"party_candidate","rewardId":"유진"}` |

특별한 결과가 없으면 `rewards`를 비워두거나 `rewardType`을 작성하지 않는다.

## 입력 템플릿

```json
{
  "titleKo": "",
  "summaryKo": "",
  "themeKo": "",
  "nodes": [
    {
      "nodeId": 1,
      "bodyKo": "",
      "nextNodeId": 0,
      "choices": [
        {
          "choiceId": "stage.node.choice.1",
          "labelKo": "",
          "resultKo": "",
          "nextNodeId": 0,
          "rewards": [
            {
              "rewardType": "",
              "rewardId": ""
            }
          ]
        }
      ]
    }
  ]
}
```

## 선형 지문 예시

```json
{
  "nodeId": 1,
  "bodyKo": "지한은 아이가 남긴 말을 다시 떠올렸다. 숲에서 누군가 부른다는 말은 단순한 헛소리처럼 들리지 않았다.",
  "nextNodeId": 2
}
```

## 선택지 지문 예시

```json
{
  "nodeId": 1,
  "bodyKo": "광장에는 불안한 사람들이 모여 있었다. 누군가는 아이의 마지막 말을 이야기했고, 누군가는 숲 입구에 남은 발자국을 가리켰다.",
  "choices": [
    {
      "labelKo": "아이의 가족에게 직접 묻는다",
      "choiceId": "stage.node.choice.1",
      "resultKo": "서진은 아이가 남긴 마지막 말과 붉은 천 인형에 대해 듣는다.",
      "nextNodeId": 2,
      "rewards": [
        {
          "rewardType": "UnlockRoute",
          "rewardId": "episode3-1"
        }
      ]
    },
    {
      "labelKo": "숲 입구의 흔적을 확인한다",
      "choiceId": "stage.node.choice.2",
      "resultKo": "서진은 발자국과 끌린 흔적을 따라 숲으로 향한다.",
      "nextNodeId": 3,
      "rewards": [
        {
          "rewardType": "UnlockRoute",
          "rewardId": "episode3-2"
        }
      ]
    }
  ]
}
```

## 에피소드 예시

```json
{
  "titleKo": "단서 조사",
  "summaryKo": "서진은 마을 사람들의 증언과 숲 입구의 흔적 중 어느 쪽을 먼저 조사할지 선택한다.",
  "themeKo": "조사 분기",
  "nodes": [
    {
      "nodeId": 1,
      "bodyKo": "광장에는 불안한 사람들이 모여 있었다. 누군가는 아이의 마지막 말을 이야기했고, 누군가는 숲 입구에 남은 발자국을 가리켰다.",
      "choices": [
        {
          "labelKo": "아이의 가족에게 직접 묻는다",
          "choiceId": "stage.node.choice.1",
          "resultKo": "서진은 아이가 남긴 마지막 말과 붉은 천 인형에 대해 듣는다.",
          "nextNodeId": 2,
          "rewards": [
            {
              "rewardType": "UnlockRoute",
              "rewardId": "episode3-1"
            }
          ]
        },
        {
          "labelKo": "숲 입구의 흔적을 확인한다",
          "choiceId": "stage.node.choice.2",
          "resultKo": "서진은 발자국과 끌린 흔적을 따라 숲으로 향한다.",
          "nextNodeId": 3,
          "rewards": [
            {
              "rewardType": "UnlockRoute",
              "rewardId": "episode3-2"
            }
          ]
        }
      ]
    },
    {
      "nodeId": 2,
      "bodyKo": "아이의 어머니는 붉은 천 인형을 꺼내 보였다. 아이가 마지막까지 손에 쥐고 있던 물건이라고 했다.",
      "nextNodeId": 4
    },
    {
      "nodeId": 3,
      "bodyKo": "숲 입구의 발자국은 중간에서 갑자기 끊겨 있었다. 사람의 흔적처럼 보였지만 평범한 납치라고 보기엔 이상했다.",
      "nextNodeId": 4
    },
    {
      "nodeId": 4,
      "bodyKo": "서진은 서로 다른 단서가 결국 같은 산을 가리키고 있음을 깨달았다.",
      "nextNodeId": 0
    }
  ]
}
```

## 최소 작성 규칙

1. 새 필드를 만들지 말고 이 문서에 적힌 필드만 사용한다.
2. 제목, 요약, 테마는 `titleKo`, `summaryKo`, `themeKo`에 작성한다.
3. 본문은 `nodes` 안의 `bodyKo`에 작성한다.
4. 각 지문은 숫자 `nodeId`를 가진다.
5. `nextNodeId`는 반드시 숫자로 적고, 연결할 지문의 `nodeId`를 가리킨다.
6. 마지막 지문은 `nextNodeId`를 `0`으로 둔다.
7. 선택지가 있으면 `choices`를 작성하고, 선택지 문구는 `labelKo`에 쓴다.
8. `choices` 배열 안의 모든 선택지는 `choiceId`를 가진다.
9. `choiceId`의 기본 형식은 `stage.node.choice.1`처럼 `카테고리.도메인.이름.인덱스`로 적는다.
10. 선택 결과 설명은 `resultKo`에 쓴다.
11. 분기 해금, 전투, 동료 후보 같은 결과는 `rewards`에 작성한다.
12. `rewardType`이 `SpecialBattle` 또는 `battle`이면 `rewardId`를 쓰지 않는다. 전투 ID는 `choiceId`를 기준으로 추론한다.
13. 기획자는 이 문서에 포함된 필드만 작성한다.
