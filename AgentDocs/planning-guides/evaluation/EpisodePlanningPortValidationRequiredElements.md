# 에피소드 기획 데이터 포팅 평가 실행 필수 요소

- formatId: `design.rule.episode_planning_port_validation_required_elements`
- version: `1`

## 목적

입력받은 Act의 원본 Episode MD와 Episode Planning JSON을 찾아 `AgentDocs/planning-guides/evaluation/EpisodePlanningPortValidationRules.md`에 따라 실제 평가를 수행하기 위한 입력 형식과 실행 절차를 정의한다.

평가 기준의 설계와 점수 판정은 다음 문서를 함께 적용한다.

```text
AgentDocs/planning-guides/evaluation/EvaluationRuleAuthoringRules.md
AgentDocs/planning-guides/evaluation/EpisodePlanningPortValidationRules.md
```

문서가 충돌하면 `EvaluationRuleAuthoringRules.md`의 공통 점수 및 최종 판정 규칙을 우선하고, 포팅 세부 판정은 `EpisodePlanningPortValidationRules.md`를 적용한다.

## 입력 항목

평가 실행 시 다음 두 항목만 입력받는다.

```text
평가할 Act:
특별항목:
```

- `평가할 Act`는 필수다.
- `특별항목`은 선택 사항이며 없으면 비워 둔다.
- 이 두 항목 외의 입력은 원칙적으로 추가 요구하지 않는다.
- 파일 탐색으로 확인할 수 있는 경로, Chapter 수, Episode 수, StoryPlanning 폴더명은 사용자에게 묻지 않는다.

## Act 입력 해석

`평가할 Act`는 숫자 또는 `ActXX` 형식으로 받을 수 있다.

입력값을 두 자리 숫자로 정규화한다.

```text
1 → Act01
01 → Act01
Act01 → Act01
12 → Act12
```

숫자로 해석할 수 없거나 Act 번호가 비어 있으면 평가를 시작하지 않고 올바른 입력을 요청한다.

## 자동 평가 경로

정규화한 Act 번호로 원본 경로를 자동 구성한다.

```text
AgentDocs/planning-data/story/Act{두 자리 Act 번호}
```

예:

```text
평가할 Act: 1
원본 경로: AgentDocs/planning-data/story/Act01
```

StoryPlanning 경로는 폴더명을 입력받지 않고 JSON 내부 참조로 찾는다.

다음 조건 중 하나 이상을 만족하는 Episode Planning JSON을 평가 후보로 수집한다.

- `common.actId`가 입력 Act와 일치한다.
- `common.source.sourceEpisodeFile`이 입력 Act의 원본 경로 아래 파일을 가리킨다.
- `story.sourceNarration.sourceEpisodeFile`이 입력 Act의 원본 경로 아래 파일을 가리킨다.

기본 탐색 범위:

```text
AgentDocs/planning-data/story-planning/**/*.json
```

`.meta` 파일은 평가하지 않는다.

## 평가 대상 확정

### 원본 Episode

입력 Act 아래의 다음 파일을 원본 Episode 후보로 수집한다.

```text
AgentDocs/planning-data/story/ActXX/Chapter*/**/*episode*.md
```

다음 파일은 Episode 본문 평가 대상에서 제외한다.

- `Chapter_XX.md`
- Act 전체 요약 문서
- 배경 문서
- `.meta`

### Planning JSON

수집한 JSON 중 `documentType`이 `episodePlanning`인 파일을 Episode Planning 평가 대상으로 사용한다.

`documentType`이 없더라도 `common.source.sourceEpisodeFile`과 `story.sourceNarration`을 모두 가진 경우에는 평가 후보로 포함하고 누락된 문서 타입을 별도 오류로 기록한다.

### 일대일 연결

원본과 JSON은 `common.source.sourceEpisodeFile`을 기준으로 연결한다.

- 원본 하나에 JSON 하나가 대응해야 한다.
- 원본에 대응 JSON이 없으면 포팅 누락이다.
- JSON이 같은 원본을 중복으로 가리키면 중복 포팅이다.
- JSON이 존재하지 않는 원본을 가리키면 잘못된 참조다.
- 입력 Act 밖의 원본을 가리키는 JSON은 평가 대상에서 제외하지 말고 잘못된 Act 참조 여부를 확인한다.

## 특별항목 적용 규칙

`특별항목`은 기본 평가에 추가할 검사 조건, 집중 확인 대상 또는 보고 방식에 사용한다.

사용 가능한 예:

```text
특별항목: 선택지 문구와 위치를 우선적으로 자세히 보고할 것
특별항목: battleJsonRef가 실제 파일로 존재하는지도 검사할 것
특별항목: Chapter 03은 평가에서 제외할 것
특별항목: 결과를 Chapter별 표로 정리할 것
```

특별항목은 다음 우선순위로 적용한다.

1. 평가 범위의 명시적 추가 또는 제외
2. 추가 절대 탈락 기준
3. 추가 감점 항목
4. 기존 항목의 집중 검사
5. 결과 보고 형식

### 허용되는 특별항목

- 특정 Chapter 또는 Episode만 평가
- 특정 Chapter 또는 Episode 제외
- 추가 파일 또는 참조 존재 여부 검사
- 특정 필드의 정합성 검사
- 새로운 절대 탈락 조건 추가
- 새로운 감점 항목 추가
- 오류 근거를 더 자세히 보고
- 결과 정렬 또는 표 형식 지정

### 허용되지 않는 특별항목 해석

특별항목이 명확하게 요구하지 않는 한 다음처럼 해석하지 않는다.

- 기본 점수 기준을 임의로 완화
- 기존 절대 탈락 기준을 해제
- 원본에 없는 선택지나 전투를 허용
- 평가 결과물을 자동 수정
- 평가 대상 파일을 덮어쓰기

특별항목이 기존 룰과 충돌하면 충돌 내용을 먼저 보고한다. 사용자가 기존 룰의 변경을 명시적으로 요구한 경우에만 해당 평가 실행에 한정하여 변경하고, 적용한 예외를 결과에 기록한다.

특별항목이 비어 있으면 기본 평가 룰만 적용한다.

## 실행 전 확인

평가 시작 전에 다음 항목을 확인한다.

1. 입력 Act 원본 폴더가 존재하는가?
2. 원본 Episode MD가 하나 이상 존재하는가?
3. 대응하는 StoryPlanning JSON을 탐색할 수 있는가?
4. 각 JSON이 가리키는 원본 파일이 존재하는가?
5. 평가 룰 문서 두 개를 읽을 수 있는가?
6. 특별항목이 있다면 객관적으로 검사 가능한가?

원본 폴더 또는 평가 룰을 읽을 수 없으면 절대 탈락이 아니라 평가 실행 불가로 처리한다.

```text
Result: NOT_EVALUATED
Reason: 평가 기준 또는 평가 대상을 읽을 수 없음
```

일부 Episode만 연결되지 않는 경우에는 평가를 계속하고 해당 Episode에 포팅 누락 또는 참조 오류를 적용한다.

## 평가 실행 절차

1. `평가할 Act`를 `ActXX`로 정규화한다.
2. 원본 Story 경로에서 모든 Episode MD를 수집한다.
3. StoryPlanning 전체에서 해당 Act의 Episode Planning JSON을 수집한다.
4. 원본과 JSON을 `sourceEpisodeFile` 기준으로 연결한다.
5. 연결 누락, 중복, 잘못된 참조를 먼저 검사한다.
6. 특별항목의 추가 범위와 검사 조건을 적용한다.
7. 각 Episode를 `EpisodePlanningPortValidationRules.md`에 따라 평가한다.
8. 절대 탈락 기준을 점수 계산보다 먼저 확인한다.
9. 지문, 선택지, 전투, 전개 및 파생 데이터 점수를 계산한다.
10. Episode별 점수와 판정을 기록한다.
11. Chapter별 점수를 Episode 평균으로 계산한다.
12. Act 전체 점수를 평가한 모든 Episode 점수의 평균으로 계산한다.
13. Act 전체 절대 탈락 여부를 확인한다.
14. 오류 위치, 감점, 필수 수정 사항을 포함한 결과를 보고한다.

평가는 읽기 전용으로 수행한다. 사용자가 별도로 수정까지 요청하지 않은 경우 원본 MD, Planning JSON, battle 데이터 또는 기타 프로젝트 파일을 변경하지 않는다.

## 점수 계산

### Episode 점수

```text
Episode 점수 = 100 - Episode에 적용된 감점
```

영역별 배점:

| 영역 | 배점 |
|---|---:|
| 지문 및 의미 태그 보존 | 30점 |
| 선택지 원문 및 대응 보존 | 30점 |
| 전투 표식 및 battle 데이터 대응 | 30점 |
| 전개 순서 및 파생 데이터 정합성 | 10점 |
| 합계 | 100점 |

### Chapter 점수

```text
Chapter 점수 = Chapter에서 평가한 Episode 점수 합계 / Episode 수
```

### Act 점수

```text
Act 점수 = Act에서 평가한 Episode 점수 합계 / Episode 수
```

- Chapter와 Act 점수는 소수점 첫째 자리까지 표시한다.
- Episode가 없는 Chapter는 평균에서 제외하고 별도로 보고한다.
- 특별항목으로 평가에서 제외한 Episode는 평균에서 제외한다.
- 절대 탈락 Episode도 계산된 점수는 평균에 포함한다.

## 최종 판정

Episode, Chapter, Act 모두 다음 판정을 사용한다.

| 판정 | 조건 |
|---|---|
| `PASS` | 절대 탈락 없음 + 81~100점 |
| `FAIL_SCORE` | 절대 탈락 없음 + 0~80점 |
| `FAIL_ABSOLUTE` | 절대 탈락 기준에 해당 |
| `NOT_EVALUATED` | 평가 대상 또는 기준을 읽을 수 없어 평가 불가 |

상위 단위 판정:

- Chapter 안에 `FAIL_ABSOLUTE` Episode가 하나라도 있으면 Chapter는 `FAIL_ABSOLUTE`다.
- Act 안에 `FAIL_ABSOLUTE` Episode가 하나라도 있으면 Act는 `FAIL_ABSOLUTE`다.
- 절대 탈락이 없으면 Chapter와 Act의 평균 점수로 `PASS` 또는 `FAIL_SCORE`를 결정한다.

## 절대 탈락 확인

기본 절대 탈락 기준은 `EpisodePlanningPortValidationRules.md`를 따른다.

특별항목으로 절대 탈락 기준이 추가된 경우 다음 정보를 결과에 반드시 기록한다.

- 특별항목 원문
- 적용한 절대 탈락 기준 ID
- 검사 대상
- 실제 발견 내용
- 절대 탈락으로 판단한 이유

## 결과 보고 형식

```text
# Episode Planning Port Validation

Evaluated Act:
Story source path:
Planning search path:
Special requirements:

Act score: 0~100/100
Act result: PASS | FAIL_SCORE | FAIL_ABSOLUTE | NOT_EVALUATED

## Coverage

- Source Episodes:
- Planning JSONs:
- Matched:
- Missing ports:
- Duplicate ports:
- Invalid references:
- Excluded by special requirements:

## Chapter Summary

| Chapter | Episodes | Score | Result |
|---|---:|---:|---|

## Episode Results

| Episode | Source | Planning JSON | Score | Result | Absolute failure |
|---|---|---|---:|---|---|

## Episode Details

### {Episode ID}

Score: 0~100/100
Result: PASS | FAIL_SCORE | FAIL_ABSOLUTE

Score breakdown:
- Narration and semantic tags: 0~30/30
- Choices: 0~30/30
- Battles: 0~30/30
- Story flow and derived data: 0~10/10

Absolute failures:
- none

Errors:
- Code:
  Deduction:
  Source location:
  JSON location:
  Expected:
  Actual:
  Reason:

Allowed differences:
- 줄바꿈 또는 popup 분할

## Required Fixes

1. 절대 탈락 원인
2. 점수 탈락의 중대 오류
3. 나머지 감점 오류
```

오류가 없는 Episode도 결과 표에서 생략하지 않는다.

## 간단 입력 양식

```text
평가할 Act:
특별항목:
```

예:

```text
평가할 Act: 1
특별항목: 선택지 추가 여부와 전투 표식 없는 battle을 우선적으로 자세히 보고할 것
```

## 실행 체크리스트

- Act 번호를 두 자리로 정규화했는가?
- 입력 Act의 모든 원본 Episode를 수집했는가?
- StoryPlanning 폴더명을 추측하지 않고 JSON 참조로 찾았는가?
- 원본과 JSON의 연결 누락 및 중복을 확인했는가?
- 특별항목을 기본 룰에 추가하여 적용했는가?
- 특별항목이 없을 때 임의 조건을 만들지 않았는가?
- 평가 전에 절대 탈락 조건을 확인했는가?
- Episode별 점수가 100점 만점인가?
- 80점 이하는 `FAIL_SCORE`인가?
- 81점 이상이고 절대 탈락이 없을 때만 `PASS`인가?
- 절대 탈락이 점수 판정보다 우선하는가?
- Chapter와 Act 평균을 소수점 첫째 자리까지 계산했는가?
- 같은 원인을 중복 감점하지 않았는가?
- 줄바꿈과 popup 분할을 감점하지 않았는가?
- 평가만 요청받았을 때 프로젝트 파일을 수정하지 않았는가?
