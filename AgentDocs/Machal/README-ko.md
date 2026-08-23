# Machal 에이전트 작업 시작 문서

## 목적

이 폴더는 ProjectBS에서 Machal과 작업하는 에이전트가 다른 채팅에서도 작업을 이어갈 수 있도록 만드는 인계 진입점이다.
공통 작업 규칙, 현재 Task 계약, 결정 사항, 검증 이력을 기록한다.

이 방식은 사용자가 직접 지정하는 작업 계약이다. 이 작업을 위해 `AGENTS.md`는 수정하지 않는다.

## 작업 전 필수 읽기 순서

Machal 관련 Task를 분석하거나 코드, 에셋, 프리팹을 수정하기 전에 다음 문서를 처음부터 끝까지 순서대로 읽는다.

1. `AgentDocs/Machal/README.md`
2. 아래에 지정된 현재 Task 전용 새 채팅 시작 계약
3. `AgentDocs/Machal/basic-work-guide.md`
4. 아래에 지정된 현재 Task 문서
5. 아래에 지정된 현재 인벤토리 문서
6. 아래에 지정된 현재 계약 평가 문서
7. 아래에 지정된 현재 표시 카탈로그
8. 아래에 지정된 현재 4단계 검증 문서
9. 아래에 지정된 현재 5-8단계 완료 문서
10. 아래에 지정된 현재 UI 프리팹 준비 문서
11. 아래에 지정된 현재 Character Skill 탭 문서
12. 아래에 지정된 현재 Character 콘텐츠 문서
13. 아래에 지정된 현재 신앙 페이지 설계 문서
14. 아래에 지정된 현재 Task 로그
15. 현재 Task 문서에 적힌 정확한 소스 및 참고 경로

필수 경로가 없으면 구현을 시작하지 않는다. 추측하지 말고 누락 경로를 Task 로그에 기록한 뒤 사용자에게 보고한다.

## 현재 Task

- Task: Ability 콘텐츠 Presentation 데이터 시스템
- 새 채팅 시작 계약: `AgentDocs/Machal/owned-effects-inventory-task-start.md`
- 한국어 새 채팅 시작 계약: `AgentDocs/Machal/owned-effects-inventory-task-start-ko.md`
- 계약 문서: `AgentDocs/Machal/ability-content-presentation-task.md`
- 인벤토리: `AgentDocs/Machal/ability-content-presentation-inventory.md`
- 3단계 계약: `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`
- 계약 평가: `AgentDocs/Machal/ability-content-presentation-contract-evaluation.md`
- 표시 카탈로그: `AgentDocs/Machal/ability-content-presentation-display-catalog.md`
- 4단계 검증: `AgentDocs/Machal/ability-content-presentation-stage4-verification.md`
- 5-8단계 완료: `AgentDocs/Machal/ability-content-presentation-stage5-8-completion.md`
- UI 프리팹 준비: `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`
- Character Skill 탭: `AgentDocs/Machal/character-skill-content-tabs.md`
- Character 콘텐츠: `AgentDocs/Machal/character-content-presentation.md`
- 신앙 페이지: `AgentDocs/Machal/faith-page-design-ko.md`
- 로그: `AgentDocs/Machal/ability-content-presentation-log.md`
- 현재 단계: 탭 없는 보유 효과 View/Presenter와 필수 프리팹 세 개의 컴포넌트 그래프를 사용자의 이번 작업 한정 승인에 따라 직접 연결했다. 정적 솔루션 빌드와 직렬화 참조 검사는 통과했다. Unity Import, Inspector 확인, 원본 목록 할당, Play Mode 클릭/스크롤/상세 표시 검증은 사용자 작업이며 Runtime Manager 자동 수집과 별도 도감 페이지는 대기한다.

## 다른 채팅에서 작업 시작하기

다음 에이전트에게 아래 문장을 전달한다.

```text
ProjectBS의 탭 없는 보유 효과 인벤토리 작업을 이어서 진행해. 분석하거나 파일을 변경하기 전에 AGENTS.md, AgentDocs/task-start-documentation-prompt.md, AgentDocs/Machal/README.md, AgentDocs/Machal/owned-effects-inventory-task-start-ko.md를 모두 읽고 작업 시작 계약의 필수 읽기 순서를 따라. C# 변경 전에는 AgentDocs/code-writing-rules.md도 읽어. 이미 연결된 Panel_OwnedEffects, 카테고리 프리팹, 아이템 프리팹을 먼저 보존하고 확인해. Unity Import와 Play Mode 검증은 사용자가 담당한다. 관련 없는 작업을 보존하고 reset, clean, commit, push, 레거시 데이터 마이그레이션, Unity 조작, 새 명시적 사용자 승인 없는 추가 프리팹 YAML 수정은 하지 마.
```

## 갱신 규칙

각 작업 단위가 끝날 때 다음을 수행한다.

1. 범위, 설계, 소스 경로, 결정 사항이 바뀌었다면 현재 Task 문서를 갱신한다.
2. 현재 Task 로그에 날짜가 포함된 항목을 추가한다.
3. 검증 완료, 대기, 차단 상태를 분리한다.
4. 프로젝트 루트 기준 정확한 경로와 구체적인 검증 근거를 기록한다.

영문 문서를 원본으로 사용한다. 같은 작업 단위에서 대응하는 `-ko` 문서도 함께 갱신한다.
