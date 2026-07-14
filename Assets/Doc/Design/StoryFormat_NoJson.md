# 에피소드 스토리 입력 포맷 - 비 JSON 설명

- formatId: `design.format.episode_story`
- version: `5`

## 작성 대상

기획자는 원본 지문, 팝업의 영구 이름과 순서, 선택지, 결과, 이미지 정책을
작성한다. 팝업 이름은 이후 Stage JSON, 문자열, 이미지, PopupEventSO의
식별자 기준이 된다.

## 팝업 이름과 ID

- `popupName`: 에피소드 안에서 고유한 영문 소문자 snake_case 이름
- `popupNameKo`: 사람이 읽는 검토용 이름
- `popupId`: `node.{act}.{chapter}.{episode}.{popupName}`
- `nodeId`: Stage 변환이 `popupId`를 그대로 복사하는 Builder 호환 값
- `popupOrder`: 변경 가능한 순서 값이며 ID에 포함하지 않음

좋은 이름:

```text
village_arrival
black_cloth_attack
rescue_choice
```

사용하지 않는 이름:

```text
popup_1
scene_2
event_003
```

## 원본 지문

원본은 `sourceNarration` 아래 의미 단위 block으로 보존한다.

- `sourceNarrationId`: 원문 block의 영구 의미 기반 ID
- `sourceOrder`: 원문 표시 순서
- `originalTextKo`: 축약·윤문·UI 개행을 하지 않은 원문

팝업은 `sourceNarrationIds`로 원문 block을 참조한다.

## 팝업 연결

기획의 `nextPopupId`에는 다음 팝업의 `popupId`를 적는다. 마지막 팝업은
`null`로 둔다. Stage 변환은 이를 `nextNodeId`로 복사한다. 숫자 `1`, `2`와
종료값 `0`은 신규 데이터에서 사용하지 않는다.

## 선택지

- `choiceName`: 팝업 안에서 고유한 의미 기반 snake_case
- `choiceId`: `choice.{act}.{chapter}.{episode}.{popupName}.{choiceName}`
- `labelKo`: 버튼 문구
- `resultKo`: 선택 직후 결과 지문

선택지 배열 번호는 ID가 아니다.

## 이미지 정책

- `generate`: 이 팝업의 신규 이미지 생성
- `reuse`: `imageSourcePopupId`의 승인 이미지를 변경 없이 새 popupId 파일명으로 복사
- `none`: 이미지 없음

이미지 파일명은 `{popupId}.main.png`를 사용한다.

## 최소 작성 규칙

1. 신규 팝업마다 영구 `popupName`을 기획 단계에서 지정한다.
2. `popupId`는 공식 생성식으로 만들고 발급 후 변경하지 않는다.
3. 순서는 `popupOrder`로 바꾸며 ID를 재번호화하지 않는다.
4. 9줄×40자 때문에 분할되는 팝업도 각각 의미 기반 이름을 받는다.
5. 이름 없는 Stage 전용 팝업을 만들지 않는다.
6. 신규 선택지는 의미 기반 `choiceName`으로 ID를 만든다.
7. 기존 숫자/순번형 ID는 영구 레거시 ID로 유지한다.
