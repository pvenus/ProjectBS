# Popup Event Main Image Evaluation Prompt


## Prompt

```text
참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/stage/PopupEventMainImageEvaluationGuide.md

스토리 팝업 메인 이미지 한 장을 읽기 전용으로 평가해줘.
이미지 생성·수정·이름 변경·프로젝트 복사·Unity import·Git 작업은 하지 마.

Input:
- eventId: {stable_event_id}
- evaluationInputPath: {evaluation_input_json_path}
- stagingArtifactPath: {evaluation_root}/{eventId}/source/{eventId}.main.png
- projectTargetPath: Assets/ImagesGenerated/Stage/popup_main/{eventId}.main.png
- outputEvaluationResultPath: {evaluation_root}/{eventId}/evaluation/evaluation_result.json
- outputEvaluationReportPath: {evaluation_root}/{eventId}/evaluation/evaluation_report.md

먼저 PopupEventMainImageEvaluationGuide.md를 읽는다.
evaluationInputPath에서 Stage, planning popupDefinition, 원본 에피소드 근거를 확인한다.

작업:
1. **staged PNG를 열기 전에** Planning `originalTextKo` /
   `displayTextKo` / `displayDirective`와 Stage `bodyKo` /
   `displayDirective`만 읽는다.
2. 아래 `planningBrief`를 먼저 결과 파일에 저장하고 고정한다.
   - `planningSummaryKo`: 기획 핵심 2~3문장
   - `primaryMoment`
   - `mustShow`: 최대 3개
   - `supportingHints`: 최대 3개
   - `mustNotShow`
   - `planningDirectiveCoverage.included/excluded`
3. planningBrief 저장 뒤 staged PNG를 처음 열고 실제 관찰과 비교한다.
4. 이미지에 잘 보이는 내용에 맞춰 primaryMoment나 mustShow를 사후
   축소·교체하지 않는다.
5. Asset / Clean Image / Event Identity / Safety 네 게이트를 확인한다.
6. 게이트 판정이 가능한 경우 네 항목만 점수화한다.
   - Story Moment: 40
   - Identity & World: 25
   - Composition: 20
   - Style & Continuity: 15
7. 결과를 pass / needs_revision / needs_human_review / fail 중 하나로 판정한다.
8. 한 줄 요약, 필수 수정 최대 3개, 선택 개선을 작성한다.
9. scoreNote마다 이미지에서 실제 보인 것, 기획상 필요한 것, 차이를
   구체적으로 한 문장에 기록한다. 공통 템플릿 문장만 사용하지 않는다.

중요 원칙:
- 원문 전체를 이미지 한 장에 모두 넣도록 요구하지 않는다.
- 서로 다른 시간·장소의 장면을 합치지 않는다.
- Planning displayDirective가 한 페이지에 묶도록 명시한 핵심 장면은
  임의로 제외하지 않는다. 요구 자체가 한 이미지로 과도하면
  `needs_human_review`로 기록한다.
- 이름 있는 인물도 현재 핵심 순간에 필수적이지 않으면 생략 가능하다.
- 캐릭터나 스타일 레퍼런스가 없으면 추측으로 불일치를 확정하지 않는다.
- 레퍼런스가 없으면 근거 없이 Style & Continuity 고정 고득점을 주지 않는다.
- Battle 선택지는 전투 진입 분위기만 본다.
- Gold 보상과 reward handoff는 이미지에 표현하지 않는다.
- validator 통과는 schema·점수 계약 검증일 뿐 시각 품질 PASS가 아니다.
- PASS라도 필요한 선택 개선은 `optionalImprovements`에 기록할 수 있다.

PASS:
- 네 게이트 통과
- 총점 85 이상
- Story Moment 32 이상
- Identity & World 18 이상

Output:
- Event
- Status
- Score
- Summary
- Gate Checks 4개
- Scores 4개
- Required Fixes 최대 3개
- Optional Improvements
- Re-evaluation Trigger

`passForUnityCopy=true`는 검증된 pass에만 설정한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_evaluation_input
  - missing_staging_artifact
  - planning_evidence_incomplete
  - evaluation_contract_conflict
  - insufficient_evidence
  - evaluation_write_failed
- 평가하지 못한 게이트 또는 점수 항목
- 실패 원인
- 생성하지 않은 평가 결과·보고서
- 다음에 필요한 작업

검증:
- planningBrief를 staged PNG를 열기 전에 고정해야 한다.
- 네 게이트 중 하나라도 실패하면 passForUnityCopy를 true로 설정하지 않아야 한다.
- 네 점수 항목의 합계가 총점과 일치해야 한다.
- pass는 총점과 Story Moment 및 Identity & World 최소 점수를 모두 충족해야 한다.
- 이미지 생성·수정·이름 변경·프로젝트 복사·Unity import·Git 작업을 수행하지 않아야 한다.
```
