# Prompt Evaluation Report Prompt

프롬프트를 평가한 뒤 점수 통과 항목과 미달 항목을 분리해서 보고할 때
사용하는 복사용 평가 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 참조 가이드를 기준으로 대상 프롬프트를 평가하고, 평가 결과를
점수 통과 항목과 미달 항목으로 분리해서 정리해줘.
이 작업은 프롬프트 문서 평가만 수행하며, 대상 프롬프트의 실제 작업은 실행하지 않는다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/prompt/PromptEvaluationGuide.md
- Assets/character_concepts/game_prompt_guide/prompt/PromptAuthoringGuide.md

Input:
- projectRoot: {project_root}
- promptFile: Assets/character_concepts/game_prompts/{domain}/{PromptName}.md
- optionalReferenceGuideFiles: 필요할 때만 입력하며, 미사용 시 생략한다.
  - Assets/character_concepts/game_prompt_guide/{domain}/{GuideName}.md
- optionalRelatedPipelineGuide: 대상 promptFile이 story pipeline 평가와 관련 있을 때만 입력한다.
  - Assets/character_concepts/game_prompt_guide/story/StoryBattlePlanningPipelineGuide.md
- passScore: 90
- categoryPassScore: 90
- itemPassScore: 90
- hardFailRules:
  - Critical finding이 있으면 점수와 무관하게 미달 처리
  - promptFile 위치가 game_prompts 밖이면 미달 처리
  - copy-ready prompt가 game_prompt_guide 아래에 있으면 미달 처리

작업:
1. PromptEvaluationGuide.md를 먼저 읽고 평가 항목, 카테고리, severity, pass/fail 기준을 확인한다.
2. PromptEvaluationGuide.md 또는 PromptAuthoringGuide.md를 읽을 수 없으면 채점하지 않고 `missing_evaluation_guide`로 실패 Output만 작성한다.
3. promptFile 경로가 비어 있거나 파일을 읽을 수 없으면 채점하지 않고 `missing_prompt_file`로 실패 Output만 작성한다.
4. promptFile이 `Assets/character_concepts/game_prompts` 밖이면 채점하지 않고 `invalid_prompt_location`으로 실패 Output만 작성한다.
5. promptFile을 읽고 평가 대상의 domain, task, pipeline 위치를 식별한다.
6. promptFile에 명시된 참조 가이드만 우선 읽는다.
7. optionalReferenceGuideFiles가 있으면 부족한 참조 확인용으로만 읽고, 평가 대상 프롬프트가 직접 참조해야 할 문서인지 구분한다.
8. optionalRelatedPipelineGuide는 story pipeline 관련 promptFile일 때만 보조 확인용으로 읽는다.
9. 읽어야 하는 참조 가이드를 읽을 수 없으면 채점하지 않고 `unreadable_reference_guide`로 실패 Output만 작성한다.
10. 평가 기준의 세부 정의는 PromptEvaluationGuide.md 원본을 우선하며, 이 프롬프트에 적힌 임계값과 출력 형식은 보고서 작성 규칙으로만 사용한다.
11. 각 평가 item을 0~100점으로 채점한다.
12. category score는 해당 item 평균으로 계산한다.
13. overall score는 category score 평균으로 계산한다.
14. itemPassScore 이상인 item은 `점수 통과 항목`으로 분리한다.
15. itemPassScore 미만인 item은 `점수 미달 항목`으로 분리한다.
16. categoryPassScore 미만인 category는 `카테고리 미달`로 분리한다.
17. passScore 이상이고 hardFailRules에 걸리지 않으면 종합 통과로 판단한다.
18. 미달 항목은 수정 우선순위를 Critical, Major, Minor, Suggestion 순서로 정리한다.
19. 대상 프롬프트를 자동 수정하지 않는다. 필요한 수정 방향만 제안한다.

Output:
- Prompt
- Domain
- Task
- Overall Score
- Rating
- Overall Pass / Fail
- Hard Fail 여부

점수 통과 항목:
- Category:
  - Item:
  - Score:
  - 통과 근거:

점수 미달 항목:
- Category:
  - Item:
  - Score:
  - 미달 원인:
  - 영향:
  - 수정 제안:

카테고리별 점수:
- Scope & Dependency:
  - Task Boundary:
  - Pipeline Fit:
- Contract Completeness:
  - Input Contract:
  - Output Contract:
  - Failure Behavior:
- Reference Quality:
  - Guide References:
  - Maintainability:
- Execution Safety:
  - Validation:
  - Safety:
- User Readiness:
  - Copy Readiness:

Findings:
- [Severity] Title
  Evidence:
  Impact:
  Recommendation:

수정 우선순위:
- 1순위:
- 2순위:
- 3순위:

재평가 예상:
- 수정 후 예상 점수:
- 통과 가능 여부:
- 남는 리스크:

실패 시 Output:
- status: failed
- failureType:
  - missing_prompt_file
  - missing_evaluation_guide
  - invalid_prompt_location
  - unreadable_reference_guide
  - insufficient_evaluation_context
- 실패 원인
- 평가하지 못한 항목
- 보강이 필요한 입력
- 실패 조건에서는 Overall Score, Rating, 점수 통과 항목, 점수 미달 항목을 산출하지 않는다.

검증:
- 점수는 모든 item에 대해 0~100 범위여야 한다.
- category score는 item 평균이어야 한다.
- overall score는 category score 평균이어야 한다.
- 통과 항목과 미달 항목은 같은 item을 중복 포함하지 않아야 한다.
- 미달 항목이 없으면 `점수 미달 항목: 없음`으로 표시해야 한다.
- hardFailRules에 걸리면 overall score가 높아도 Overall Pass / Fail은 Fail이어야 한다.

주의:
- 대상 프롬프트의 실제 작업을 실행하지 않는다.
- 대상 프롬프트를 자동 수정하지 않는다.
- 점수만 제시하지 말고, 통과 근거와 미달 원인을 함께 적는다.
- 평가 기준은 PromptEvaluationGuide.md를 원본으로 따른다.
- optional 입력은 평가 맥락 보강용이며, 대상 promptFile이 직접 참조해야 할 문서로 자동 간주하지 않는다.
```
