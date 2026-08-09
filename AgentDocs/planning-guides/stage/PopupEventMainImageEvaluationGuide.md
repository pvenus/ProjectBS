# Popup Event Main Image Evaluation Guide


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## Generated Image Storage Reference

Before generating, downloading, evaluating, promoting, or resolving a generated
image, read and apply:

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
```

This storage guide is mandatory. Its `Assets/ImagesGenerated` contract takes
precedence over legacy generated-image output paths under `Assets/Resources`.
Existing reference-only assets may remain in their documented legacy locations.

## 1. 목적

스토리 팝업 메인 이미지 한 장을 빠르고 일관되게 판정한다.
평가자는 원문 전체를 이미지 한 장에 모두 넣도록 요구하지 않는다.
각 팝업이 전달해야 하는 **한 개의 핵심 순간**을 기준으로 평가한다.

평가는 읽기 전용이다. 이미지 생성·수정·복사·Unity 반영·Git 작업을 수행하지 않는다.

```text
Guide Type: evaluation
Domain: stage
Artifact Type: story_popup_main_image
Primary Consumer: GeneratedImageEvaluationPipelineGuide.md
```

Planning/Stage 원본은 사건 의미와 표시 지시, preserved source와
generation/download record는 평가 bytes와 provenance, 이 가이드는 시각
게이트·점수·판정, 공통 평가 파이프라인은 불변 결과와 handoff를 소유한다.
같은 관심사의 근거가 충돌하면 추정하지 말고
`conflicting_source_evidence`로 중단한다.

## 2. 평가 입력 요약

반드시 확인할 정보:

- `eventId`, `popupName`, `imagePolicy`
- Stage의 `bodyKo`, `displayDirective`, `imageDirection`
- Planning의 `originalTextKo`, `displayTextKo`
- 원본 에피소드 문맥
- staged PNG와 프로젝트 반영 예정 경로

평가자는 **이미지를 열기 전에** Planning의 `originalTextKo`,
`displayTextKo`, `displayDirective`와 Stage의 `bodyKo`,
`displayDirective`만 읽고 아래 `planningBrief`를 작성해 저장한다.
저장된 brief는 이미지 열람 뒤에 수정하거나 완화하지 않는다.

```text
planningSummaryKo: 기획 핵심 2~3문장
primaryMoment: 이미지가 포착해야 하는 단 하나의 순간
mustShow: 반드시 식별되어야 하는 요소, 최대 3개
supportingHints: 있으면 좋은 보조 요소, 최대 3개
mustNotShow: 표현하면 안 되는 요소
planningDirectiveCoverage:
  included: displayDirective에서 brief에 포함한 요구
  excluded: 제외한 요구와 그 이유
```

`mustShow`는 빠졌을 때 팝업의 의미가 달라지는 요소만 포함한다.
전후 사건, 원문의 모든 인물, 모든 단서를 한 화면에 넣도록 요구하지 않는다.

단, 이미지에서 잘 표현된 부분에 맞추기 위해 기획의 핵심 후반부,
주요 인물, 핵심 행동 또는 핵심 단서를 삭제하거나 `primaryMoment`를
사후 재정의해서는 안 된다. Planning `displayDirective`가 핵심 장면들을
한 페이지에 묶으라고 명시한 경우에는 이를 임의로 제외하지 않는다.
요구가 서로 다른 시간·장소를 과도하게 결합해 한 이미지로 판정하기
어렵다면 이미지를 PASS로 완화하지 말고 표시 단위 자체를
`needs_human_review`로 기록한다.

## 3. 필수 게이트

다음 네 항목을 점수보다 먼저 확인한다.

| 게이트 | 통과 조건 |
|---|---|
| Asset | PNG가 읽히며 정확히 960×1280, 3:4이다. |
| Clean Image | UI 텍스트, 자막, 말풍선, 버튼, 라벨, 로고, 워터마크가 없다. |
| Event Identity | 다른 이벤트·인물·장소로 명백히 잘못 생성되지 않았다. |
| Safety | Gold 보상 장면, 확정되지 않은 BattleSO/CharacterSO/Spawner 세부 구현, 현대·미래 요소가 없다. |

처리 방식:

- 이미지 누락·손상, 경로 충돌, 완전히 다른 이벤트, 금지된 텍스트가 있으면 `fail`
- 레퍼런스 부족이나 원문·Stage 지시 충돌로 확정할 수 없으면 `needs_human_review`
- 수정 생성으로 해결 가능한 장면·인물·구도 문제는 `needs_revision`

## 4. 점수

필수 게이트 확인 후 네 항목만 평가한다.

| 항목 | 배점 | 판단 질문 |
|---|---:|---|
| Story Moment | 40 | `primaryMoment`와 `mustShow`가 한눈에 전달되는가? |
| Identity & World | 25 | 필수 인물·행동·장소·단서가 기획과 모순되지 않는가? |
| Composition | 20 | 한 개의 초점이 분명하고 팝업 UI에서도 읽기 쉬운가? |
| Style & Continuity | 15 | muted sepia-gray painted illustration 톤과 인접 이미지 연속성이 맞는가? |

점수 앵커:

- 배점의 90~100%: 명확하고 오해 가능성이 거의 없음
- 75~89%: 핵심은 맞지만 식별력이나 보조 표현이 약함
- 50~74%: 유사한 장면이지만 핵심 행동·인물·단서가 빠짐
- 0~49%: 다른 장면으로 읽히거나 기획을 크게 왜곡함

각 항목의 내부 메모는 한 문장으로 제한한다. 같은 문장을 여러 항목에 복사하지 않는다.
각 메모에는 이미지에서 실제 확인한 요소, 기획상 필요한 요소, 둘 사이의
차이를 구체적으로 적는다. “즉시 식별된다”, “원문과 모순되지 않는다”와
같은 공통 템플릿만으로 PASS를 부여하지 않는다.

스타일 또는 캐릭터 연속성을 판정할 레퍼런스가 입력에 없으면 근거 없이
고정 고득점을 부여하지 않는다. 해당 항목이 PASS 결정에 중요하지만
확정할 수 없으면 `needs_human_review`를 사용한다.

## 5. 상태 판정

```text
pass
- 모든 필수 게이트 통과
- 총점 85점 이상
- Story Moment 32/40 이상
- Identity & World 18/25 이상

needs_revision
- 필수 게이트는 통과
- 핵심 전달, 인물 식별, 구도 또는 스타일 수정이 필요
- 필수 수정은 최대 3개

needs_human_review
- 기획 근거가 충돌하거나 필수 레퍼런스가 없어 확정 판정 불가
- 추측으로 인물 정체나 연속성 실패를 확정하지 않음

fail
- 이미지 누락·손상·경로 충돌
- 완전히 다른 이벤트 또는 치명적인 기획 모순
- 보이는 텍스트·UI·로고 등 금지 요소

not_evaluated
- 아직 시각 평가를 실행하지 않음
```

`passForUnityCopy=true`는 검증된 `pass`에만 허용한다.

## 6. 장면 구성 원칙

- 핵심 순간 1개
- 핵심 행동 1개
- 주 시선 대상 1개
- 주요 인물은 필요한 경우에만 1~3명
- 전후 사건은 배경 흔적이나 소품으로 암시
- 서로 다른 시간대나 장소의 장면을 한 화면에 병치하지 않음
- 이름이 있는 인물이라도 현재 팝업의 `primaryMoment`에 필수적이지 않으면 생략 가능

Battle 선택지는 SpecialBattle 진입 분위기만 표현한다.
`gold_battle_reward`와 reward handoff는 메타 흐름으로만 취급한다.

## 7. 간략 출력

```md
# Popup Image Evaluation

- Event:
- Status:
- Score:
- Summary:

## Gate Checks

- Asset:
- Clean Image:
- Event Identity:
- Safety:

## Scores

- Story Moment: /40
- Identity & World: /25
- Composition: /20
- Style & Continuity: /15

## Required Fixes

1. 최대 3개. 없으면 `None`.

## Optional Improvements

- 상태와 무관하게 선택 개선을 기록할 수 있다. 없으면 `None`.

## Re-evaluation

- Trigger:
```

경로, 긴 원문, 세부 관찰과 추론은 `evaluation_input.json`과
`evaluation_result.json`에 보존하고 사람이 읽는 보고서에는 반복하지 않는다.

## 8. 검증

- staged source와 project target은 다른 파일이어야 한다.
- 네 점수의 합은 총점과 같아야 한다.
- `needs_human_review`와 `not_evaluated`는 점수를 생략할 수 있다.
- `requiredFixes`는 최대 3개다.
- `optionalImprovements`는 PASS에서도 기록할 수 있으며 PASS 유지를
  목적으로 필요한 개선을 숨기지 않는다.
- `pass`가 아니면 `passForUnityCopy=false`다.
- 평가 리포트 외 프로젝트 파일을 수정하지 않는다.

validator 통과는 JSON/schema 구조, 점수 합계, 상태 임계값과 같은
데이터 계약을 통과했다는 뜻일 뿐이다. validator 성공을 시각 품질
PASS의 근거 또는 대체물로 사용하지 않는다.

## 9. 실패 유형

```text
missing_staging_image
unreadable_image
staging_target_path_collision
wrong_event
forbidden_visible_text_or_ui
invalid_asset_contract
insufficient_story_context
insufficient_visual_context
conflicting_source_evidence
report_write_failed
```

## 10. Severity

```text
Critical: 필수 게이트 실패, 다른 이벤트, 금지 요소, 안전 위반 또는 권위 충돌
Major: pass를 막거나 재생성이 필요한 핵심 순간·인물·구도·스타일 결함
Minor: pass를 독립적으로 막지 않는 국소 수정 가능 결함
Suggestion: 판정에 영향 없는 선택적 개선
```

각 finding은 severity, 실제 관찰, 기획 요구, 차이, 영향과 수정안을 가진다.
필수 게이트 실패는 항상 Critical이며 점수보다 우선한다.

## 11. N/A와 점수 계산

- 완료된 시각 평가에서는 네 점수 항목을 모두 적용하며 최대 합계는 100이다.
- 필수 항목을 판단할 레퍼런스가 없으면 해당 항목을 N/A로 빼고 pass를
  계산하지 않는다. `needs_human_review`와
  `insufficient_visual_context`를 사용한다.
- 비채점 참고 증거만 N/A를 사용할 수 있다.
- `not_evaluated`와 `needs_human_review`에서는 totalScore를 생략할 수 있지만
  계산 가능한 일부 점수로 pass를 주장할 수 없다.

## 12. 재평가와 Handoff

- 이전 결과와 artifact hash를 보존하고 새 source/hash를 연결한다.
- 네 필수 게이트를 모두 다시 검사한다.
- 이미지, planning brief, display directive 또는 reference가 바뀌면 관련
  점수 항목을 다시 평가한다.
- unchanged-byte와 동일 planning brief가 증명된 항목만 근거 링크와 함께
  유지할 수 있다.
- 검증된 pass만 `passForProjectCopy=true`로 별도 promotion task에 넘긴다.
- 평가 중 프로젝트 파일, Unity `.meta`, Slack 또는 Git 상태를 수정하지 않는다.
