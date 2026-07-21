# 스토리 제작 필수 요소

- formatId: `design.rule.story_required_elements`
- version: `1`

## 목적

스토리 파일을 바탕으로 Episode를 만들기 전에 최소한으로 정해야 하는 요소를 정리한다.

스토리 본문은 별도 md 파일로 제공한다고 가정한다. 이 문서는 해당 스토리 파일을 어디에서 읽을지, 그리고 Episode 제작에 필요한 구조 정보를 무엇으로 볼지 정리한다.

Episode를 만들 때 작성 규칙은 항상 `Assets/Doc/Design/StoryWritingRules.md`를 참조한다.

Chapter 스토리 경로는 입력받지 않는다. 대신 `생성할 Act 번호`와 `생성할 Chapter 번호`를 두 자리 숫자로 맞춰 다음 형식으로 자동 조합하고, Episode를 만들 때 해당 경로의 스토리 파일을 반드시 참조한다.

```text
Assets/Doc/Story/Act{Act번호}/Chapter{Chapter번호}
```

예시:

```text
생성할 Act 번호: 1
생성할 Chapter 번호: 1
자동 Chapter 스토리 경로: Assets/Doc/Story/Act01/Chapter01
```

자동 조합한 Chapter 폴더 안의 `Chapter_XX.md`를 Chapter 원본 스토리로 사용한다.

## 필수 요소

새 스토리를 만들 때는 아래 항목을 먼저 정한다.

- 생성할 Act 번호
- 생성할 Chapter 번호
- 해당 Chapter의 Episode 수
- 분기가 있는 Episode
- 분기의 수
- 분기마다 달라지는 내용
- 분기가 다시 합쳐지는 지점
- 새로 등장하는 캐릭터 이름
- 새로 등장하는 캐릭터의 직업 또는 역할

## 간단 입력 양식

```text
생성할 Act 번호:
생성할 Chapter 번호:

Episode 수:
분기 있는 Episode:
분기 수:
분기별 차이:
분기 합류 지점:

새 캐릭터 이름:
캐릭터 직업/역할:
특이사항:
```
