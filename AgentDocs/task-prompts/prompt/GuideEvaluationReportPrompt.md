# Guide Evaluation Report Prompt

가이드 문서 자체의 품질, 정합성, 참조 관계와 실행 안전성을 읽기 전용으로
평가하고 점수 통과·미달 항목을 분리해 보고할 때 사용하는 복사용
프롬프트입니다.

## Prompt

```text
현재 작업에서 사용 중인 ProjectBS 저장소를 확인하고, 아래 가이드 문서 하나를 읽기 전용으로 평가해줘.
대상 가이드나 참조 문서를 수정하지 말고 평가 보고서만 현재 응답으로 출력해줘.

참조 가이드:
- AgentDocs/planning-guides/prompt/GuideEvaluationGuide.md
- AgentDocs/planning-guides/prompt/GuideAuthoringGuide.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/prompt/PromptEvaluationGuide.md

Input:
- guideFile: {project_relative_guide_markdown_path}
- guideTypeHint: {optional_reference | schema_data_structure | workflow_pipeline | evaluation | slack_canvas | hybrid}
- domainHint: {optional_domain}
- purposeHint: {optional_expected_purpose}
- optionalReferenceGuideFiles:
  - AgentDocs/planning-guides/{domain}/{ExactGuideName}.md
- optionalRuntimeContractFiles:
  - {optional_exact_project_relative_schema_code_or_unity_contract_path}
- knownConflictOrFailureCase: {optional_specific_case}
- passScore: 90
- categoryPassScore: 90
- itemPassScore: 90

입력 경계:
- guideFile만 필수이고 나머지 hint와 추가 참조는 생략할 수 있다.
- hint는 탐색 보조 정보이며 대상 문서와 저장소 근거를 덮어쓰지 않는다.
- 다른 PC의 절대 경로를 사용하지 않는다.
- 모호한 디렉터리 전체나 “관련 문서 전부”를 입력으로 요구하지 않는다.

작업:
1. GuideEvaluationGuide.md를 먼저 읽고 category, item, 계산식, severity, Hard Fail, 운영 실패 규칙을 확인한다.
2. GuideEvaluationGuide.md를 읽을 수 없으면 missing_evaluation_guide로, GuideAuthoringGuide.md를 읽을 수 없으면 missing_authoring_guide로 채점하지 않고 실패 Output만 작성한다.
3. guideFile이 없거나 읽을 수 없으면 채점하지 않고 missing_guide_file 실패 Output만 작성한다.
4. guideFile을 읽고 실제 Guide Type, Domain, Purpose와 적용 범위를 식별한다. hint와 다르면 실제 문서 근거를 사용하고 mismatch를 finding으로 기록한다.
5. master concept, guideFile이 정확히 명시한 상위 기준·schema·runtime·공통 가이드를 우선순위에 따라 읽는다.
6. guideFile의 ID, 경로, 파일명, 상태, handoff 또는 책임 경계를 검증하는 데 꼭 필요한 정확한 cross-guide만 추가로 읽는다. 디렉터리 전체를 읽지 않는다.
7. optionalReferenceGuideFiles와 optionalRuntimeContractFiles는 해당 주장 검증에 필요한 경우에만 읽고 References Actually Read에 기록한다.
8. 필수 권위 문서를 읽을 수 없어 핵심 계약을 검증할 수 없으면 unreadable_reference_guide로, 근거가 구조적으로 부족하면 insufficient_evaluation_context로 운영 실패 Output을 작성한다.
9. 운영 실패가 없으면 GuideEvaluationGuide.md의 모든 적용 가능한 item을 0~100점으로 채점한다. Evaluation Rule Quality는 평가 verdict를 소유한 guide에만 적용하고 정당한 N/A는 평균에서 제외한다.
10. category score는 적용 item의 산술평균, overall score는 적용 category의 산술평균으로 계산하고 중간값은 반올림하지 않는다. 표시값만 소수점 둘째 자리까지 반올림한다.
11. itemPassScore 이상 item은 점수 통과 항목, 미만 item은 점수 미달 항목으로 중복 없이 분리한다.
12. categoryPassScore 미만 category는 카테고리 미달로 표시한다.
13. Guide Location, Prompt Separation과 Responsibility Separation을 확인한다. guide가 AgentDocs/planning-guides 밖에 있거나 실행용 copy-ready task prompt가 guide에 포함되면 해당 Hard Fail을 기록한다.
14. 아래 구 경로가 현재 기본·입력·출력·참조 계약으로 남아 있는지 확인한다. 명시적 금지 예시나 migration history가 아니라 active contract이면 stale_path_contract Hard Fail로 처리한다.
    - Assets/character_concepts/game_prompt_guide
    - Assets/character_concepts/game_prompts
15. Purpose/Scope, source of truth와 우선순위, guide type에 필요한 입력·출력·경로·ID·파일명·상태·실패·검증 계약의 완전성을 평가한다.
16. GuideAuthoringGuide.md를 공통 작성 표준으로 대조한다. PromptAuthoringGuide.md, PromptEvaluationGuide.md, ContentFolderStructureGuide.md와 관련 도메인 가이드는 대상 계약과 실제 관련될 때만 대조하고, 불필요한 참조 강요는 하지 않는다.
17. 생성, 다운로드, 평가, 프로젝트 승격, Slack 기록, Unity import/build, Git과 배포 경계가 섞이거나 무제한 mutation 권한을 여는지 확인한다.
18. 대상이 evaluation guide이면 점수 체계, category 합계/평균, 관찰 가능한 기준, severity, 절대 탈락, verdict와 재평가 규칙을 모두 평가한다.
19. 모든 finding을 Critical, Major, Minor, Suggestion으로 분류하고 정확한 파일·section 근거, 영향, 최소 수정안을 기록한다.
20. overall/item/category threshold를 모두 만족하고 Critical 및 Hard Fail이 없을 때만 Overall Pass로 판정한다.
21. 대상 guide, 참조 guide, prompt, schema, 코드, Unity asset, Canvas 또는 평가 report 파일을 생성·수정하지 않는다.
22. 대상 guide가 설명하는 실제 작업을 실행하지 않고 이미지 생성·다운로드·평가·복사, Slack 게시, Unity 작업, Git commit/push/merge와 배포를 수행하지 않는다.

Output:
- Guide
- Guide Type
- Domain
- Purpose
- Overall Score
- Rating
- Overall Pass / Fail
- Hard Fail 여부
- Hard Fail Rules Triggered

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
  - Evidence:
  - 영향:
  - 수정 제안:

카테고리별 점수:
- Location & Role Separation:
  - Guide Location:
  - Prompt Separation:
  - Responsibility Separation:
- Purpose & Scope:
  - Purpose Clarity:
  - Guide Type Clarity:
  - Applicability & Exclusions:
- Source of Truth & Reference Priority:
  - Authority Declaration:
  - Priority & Conflict Rule:
  - Reference Precision:
- Contract Completeness:
  - Inputs & Preconditions:
  - Outputs & Handoffs:
  - Identity & Storage:
  - States & Failure Behavior:
  - Validation & Executability:
- Cross-guide Consistency:
  - Common Contract Alignment:
  - Domain/Pipeline Alignment:
  - Current Path Contract:
- Safety & Boundary:
  - Mutation Boundary:
  - Pipeline Stage Separation:
  - Destructive/External Safety:
- Evaluation Rule Quality: {score | N/A}
  - Score Model:
  - Criteria Observability:
  - Severity & Hard Fail:
  - Verdict & Re-evaluation:
- Maintainability:
  - Duplication Control:
  - Stable References:
  - Extension & Versioning:
- User/Agent Readiness:
  - Action Clarity:
  - Decision Closure:
  - Navigability:

카테고리 미달:
- Category:
  - Score:
  - 핵심 원인:

Findings:
- [Critical | Major | Minor | Suggestion] Title
  Evidence:
  Impact:
  Recommendation:

Cross-guide Conflicts:
- 대상 계약:
  충돌 문서/section:
  영향:
  필요한 owner 결정:

Boundary Risks:
- ...

수정 우선순위:
- 1순위:
- 2순위:
- 3순위:

재평가 예상:
- 수정 후 예상 점수:
- 통과 가능 여부:
- 남는 리스크:

References Actually Read:
- {project_relative_path}: {reason}

References Required But Unavailable:
- {project_relative_path}: {blocked_check}

실패 시 Output:
- status: failed
- failureType: missing_guide_file | missing_evaluation_guide | missing_authoring_guide | unreadable_reference_guide | insufficient_evaluation_context
- 실패 원인
- 확인한 Guide File
- References Actually Read
- References Required But Unavailable
- 평가하지 못한 category/item
- 보강이 필요한 입력 또는 Required Next Action
- 운영 실패에서는 Overall Score, Rating, pass/fail, 통과·미달 항목과 category score를 산출하지 않는다.

검증:
- 모든 적용 item 점수는 0~100 범위여야 한다.
- category score는 해당 적용 item 평균이어야 한다.
- overall score는 적용 category 평균이어야 한다.
- N/A는 허용된 조건부 item/category에만 사용하고 평균에서 제외해야 한다.
- 점수 통과와 미달 항목은 같은 item을 중복 포함하지 않아야 한다.
- 미달 item이나 finding이 없으면 해당 section을 `없음`으로 표시해야 한다.
- Critical 또는 Hard Fail이 있으면 점수와 무관하게 Overall Pass / Fail은 Fail이어야 한다.
- invalid_guide_location, copy_ready_prompt_inside_guide, stale_path_contract는 채점을 중단하는 운영 실패가 아니라 근거와 점수를 남기는 Hard Fail이어야 한다.
- 읽지 않은 문서를 읽었다고 주장하거나 근거 없는 line/section을 만들지 않아야 한다.
- 대상 및 참조 파일의 수정, 실제 workflow 실행과 외부 상태 변경이 없어야 한다.
```
