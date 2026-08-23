# Ability 콘텐츠 Presentation Task 로그

새 항목을 이어서 추가한다. 사실 오타를 고치는 경우 외에는 이전 항목을 다시 쓰지 않으며, 결론이 바뀌면 정정 항목을 추가한다.

## 2026-08-08 — 문서 인계 구조 생성

- 상태: 문서 작업 단위 검증 완료, 구현은 시작하지 않음
- 수행 범위:
  - `AgentDocs/Machal/` 인계 진입점을 생성했다.
  - 기본 작업 방식과 필수 읽기 순서를 기록했다.
  - 현재 Presentation 데이터 계획, 아키텍처, 승인 에셋 경로, 제외 범위, 작업 순서, 검증 매트릭스를 기록했다.
- 반영한 사용자 결정:
  - 레거시 데이터를 건드리지 않는다.
  - 승인된 현행 Skill/Effect 에셋만 사용한다.
  - 중립적이고 재사용 가능한 Presentation 계약은 Core에 둔다.
  - 게임플레이 분류와 매핑은 Ability에 둔다.
  - 변수의 문자열 변환보다 의미 카테고리와 그룹을 우선한다.
  - View 작업보다 데이터 계층을 먼저 완성한다.
- 현재 소스 조사 결과:
  - 현행 Skill/Effect 에셋은 `Assets/Resources/skill/character/generated/`와 `Assets/Resources/skill/json/`에 존재한다.
  - 현행 직렬화 필드 `effectEntries`를 가진 Bless 또는 Relic 에셋은 발견하지 못했다.
  - 기존 Bless/Relic 경로는 제외 상태로 유지하고 수정하지 않는다.
- 생성 파일:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/basic-work-guide.md`
  - `AgentDocs/Machal/basic-work-guide-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- 기존 파일 수정: 없음
- 완료한 검증:
  - 인계 문서 8개가 모두 존재하며 UTF-8로 읽히는 것을 확인했다.
  - 필수 Task 문서와 승인된 소스/에셋 경로가 존재하는 것을 확인했다.
  - 영문 원본마다 대응하는 한국어 `-ko` 문서가 존재한다.
  - 후행 공백과 탭 문자가 없음을 확인했다.
  - 파일 8개가 Git에 무시되지 않고 새 미추적 파일로 표시되는 것을 확인했다.
- 수행하지 않은 작업:
  - 스테이징, 커밋, 푸시
- 권장 다음 작업:
  - 필수 문서를 모두 읽고 작업 트리 기준선을 기록한 뒤, 런타임 코드를 작성하기 전에 현행 에셋 인벤토리를 만든다.

## 2026-08-08 — 아키텍처 및 Effect 정규화 계약 정정

- 상태: 설계 문서 갱신, 구현은 시작하지 않음
- 이전 항목에 대한 정정:
  - `Assets/Scripts/Core/Presentation/`을 만들거나 사용하지 않는다.
  - 중립적인 공통 계약은 `Assets/Scripts/Presentation/Content/Data/`에 둔다.
  - 콘텐츠별 코드는 각 콘텐츠 소유 경로 안의 `Presentation/` 하위 폴더에 둔다.
- 최종 계획 콘텐츠 경로:
  - `Assets/Scripts/Ability/Effects/Presentation/`
  - `Assets/Scripts/Ability/Skills/Presentation/`
  - `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Presentation/`
  - `Assets/Scripts/Collection/Relic/Presentation/`
  - `Assets/Scripts/Actor/Character/Presentation/`
- Effect 정규화 결정:
  - 지원되는 각 현행 Effect를 선택적인 `Activation`과 승인된 하나의 의미 결과인 `StatModifier`, `Heal`, `CooldownChange`, `Displacement`, `PeriodicDamage`, `SkillInvoke`, `Control` 조합으로 정규화한다.
  - 정확한 원본과 결과 조합표를 현재 Task 문서의 권위 있는 계약으로 사용한다.
  - 의미가 불분명한 구형 `SkillEffectSO` 필드는 정규화하지 않고 작성된 설명이 있을 때만 사용한다.
- View 작업은 계속 연기한다.
- 수정 파일:
  - `AgentDocs/Machal/basic-work-guide.md`
  - `AgentDocs/Machal/basic-work-guide-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- 남은 검증:
  - 영문/한글 계약 대응 확인
  - 활성 `Core/Presentation` 경로 참조 제거 확인
  - 공백 및 경로 확인

## 2026-08-08 — 외부 아키텍처 참고 계약 반영

- 상태: 참고 계약 반영, 구현은 시작하지 않음
- 원본 Task: `019fde12-0db0-7023-a89f-c29f73b31413`
- 추가 결정:
  - 정규화 구조는 도메인 Presentation 데이터이며 `ViewData`나 `UIData`로 만들지 않는다.
  - 정규화와 최종 문자열 포맷을 분리한다.
  - 수치에는 효과 거리와 효과 범위 같은 일반 개념을 사용하고 동작 표현은 작성된 설명에 유지한다.
  - Interval과 Duration으로 적용 횟수를 계산하지 않는다.
  - 부모 Skill에는 중첩 Skill 이름과 요약만 표시하며 상세는 독립적으로 Resolve한다.
  - 아키텍처 참고 계약은 프로젝트 전체 폴더 마이그레이션을 허용하지 않는다.
  - 향후 스크립트 이동은 `.cs.meta` GUID와 관련 없는 기존 작업을 보존해야 한다.
  - 향후 `Assets/Contents` JSON/생성 SO 배치는 별도 Task로 유지한다.
- 승인된 모든 Effect 정규화 조합에 대한 계획 Resolver 클래스 표를 추가했다.
- 런타임 코드, 에셋, 프리팹, Scene, 기존 스크립트 폴더는 수정하지 않았다.
- 남은 검증:
  - 문서 수정 후 계약 대응 및 경로 확인
  - 공백 확인

## 2026-08-08 — 수정 설계 문서 검증 완료

- 상태: 문서 작업 단위 검증 완료, 구현은 시작하지 않음
- 완료한 검증:
  - 활성 가이드와 Task 계약에 거부된 `Assets/Scripts/Core/Presentation/` 또는 광범위한 `Assets/Scripts/Ability/Presentation/` 경로 참조가 없다.
  - 계획된 모든 소유 경로의 부모 폴더가 현재 체크아웃에 존재한다.
  - 원본과 정규화 결과 조합 14개가 영문 및 한국어 Task 계약에 모두 존재한다.
  - Resolver 클래스 조합, 간결한 표시 경계, 적용 횟수 규칙, 중첩 Skill 계약, 마이그레이션 경계, SO/JSON 경계가 양쪽 언어 문서에 존재한다.
  - `AgentDocs/Machal/`에서 후행 공백과 탭 문자가 발견되지 않았다.
- 수행하지 않은 작업:
  - 런타임 구현, `Assets/Scripts` 아래 폴더 생성, 에셋 변경, 스테이징, 커밋, 푸시
- 권장 다음 작업:
  - 현행 소스 인벤토리를 만든 뒤 사용자가 코드 작업 시작을 승인하면 `Assets/Scripts/Presentation/Content/Data/`에 가장 작은 공통 계약부터 구현한다.

## 2026-08-08 — Presentation 폴더 구조 단순화

- 상태: 계획 정정 및 검증 완료, 구현은 시작하지 않음
- 이전의 모든 계획 경로 항목에 대한 정정:
  - 공통 중립 계약은 `Assets/Scripts/Presentation/` 바로 아래에 둔다. 이 공통 계약을 위해 루트 Presentation 아래에 `Content/` 또는 `Data/`를 추가하지 않는다.
  - 이 기능을 위해 Ability Effects, Ability Skills, Blessings, Relic, Character에 `Presentation/` 하위 폴더를 추가하지 않는다.
  - 콘텐츠가 소유하는 수동적 데이터 타입은 각 소유 경로의 `Data/` 하위 폴더로 구분한다.
  - Resolver와 Builder 클래스는 콘텐츠 소유 경로 바로 아래에 둔다. 이 기능만을 위한 `Resolvers/` 하위 폴더를 추가하지 않는다.
  - `EffectPresentationResolver`, `<EffectType>PresentationResolver`, `SkillPresentationResolver`처럼 명확한 이름으로 동작을 그룹화한다.
- 수정된 1차 구현 경로:
  - 공통 계약: `Assets/Scripts/Presentation/`
  - Effect 데이터: `Assets/Scripts/Ability/Effects/Data/`
  - Effect Resolver: `Assets/Scripts/Ability/Effects/`
  - Skill 데이터: `Assets/Scripts/Ability/Skills/Data/`
  - Skill Resolver/Builder: `Assets/Scripts/Ability/Skills/`
- 승인된 현행 데이터가 생길 때까지 대기하는 후속 Adapter 구조:
  - Bless 데이터와 Resolver: `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Data/` 및 소유 경로 바로 아래
  - Relic 데이터와 Resolver: `Assets/Scripts/Collection/Relic/Data/` 및 소유 경로 바로 아래
  - Character 데이터와 Resolver: `Assets/Scripts/Actor/Character/Data/` 및 소유 경로 바로 아래
- 유지되는 결정:
  - 레거시 데이터는 계속 제외하며 수정하지 않는다.
  - Effect 정규화 조합은 계속 권위 있는 계약으로 사용한다.
  - 데이터 설정과 실제 에셋 검증을 모든 View 작업보다 먼저 수행한다.
- 수정 파일:
  - `AgentDocs/Machal/basic-work-guide.md`
  - `AgentDocs/Machal/basic-work-guide-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- 완료한 검증:
  - 활성 가이드와 Task 계약에 계획 경로로 남은 `Presentation/Content`, 콘텐츠 도메인 `Presentation/`, 기능 전용 `Resolvers/` 경로가 없다.
  - 양쪽 Task 계약에 공통, Effect, Skill, Bless, Relic, Character 데이터의 정확한 경로가 명시되어 있다.
  - 권위 있는 Effect 정규화 14개 사례가 양쪽 언어 문서에 모두 유지되어 있다.
  - 모든 소유 상위 경로가 현재 체크아웃에 존재하며 계획된 스크립트 폴더는 생성하지 않았다.
  - `AgentDocs/Machal/`에서 후행 공백과 탭 문자를 발견하지 않았다.
- 수행하지 않은 작업:
  - 런타임 구현, 스크립트 폴더 생성, 에셋 변경, 스테이징, 커밋, 푸시
- 권장 다음 작업:
  - 승인된 현행 Skill 및 Effect 소스를 인벤토리화한 뒤 코드 작업 승인을 받으면 `Assets/Scripts/Presentation/` 바로 아래의 가장 작은 공통 계약부터 구현한다.

## 2026-08-08 — 1단계 소스 및 에셋 인벤토리 완료

- 상태: 1단계 완료, 데이터 계층 코드는 시작하지 않음
- 수행 범위:
  - 활성 Task 계약에 지정된 모든 소스 및 승인 에셋 경로를 확인했다.
  - 현행 `EquipmentSkillSO` 소스 구조와 `EquipmentSkillResolver` 런타임 출력을 추적했다.
  - `EffectSO`, `EffectEntrySO`, 현행 `EffectConfig` 클래스 13개, `EffectResolver`, Runtime Config 사용 지점을 추적했다.
  - 승인 JSON, Skill SO, Hit SO, Effect SO, EffectEntry SO, 도달 가능한 Effect 참조를 집계했다.
  - 작성 JSON 선언과 런타임에서 도달 가능한 SO 데이터를 구분했다.
  - 8단계 구현 계획을 추가하고 인벤토리를 필수 인계 문서로 지정했다.
- 주요 확인 결과:
  - 승인 경로에는 `EquipmentSkillSO` 58개, `EffectSO` 20개, `EffectEntrySO` 20개가 있다.
  - 도달 가능한 EffectEntry는 18개이며 모두 Strategic Skill Hit 에셋을 통해 참조된다.
  - Character JSON은 Effect 27개를 선언하지만 승인된 Character Hit SO에는 Null이 아닌 EffectEntry 참조가 0개다.
  - Character JSON 6개는 대응하는 주 Skill 에셋이 없다.
  - Character EffectEntry 에셋 2개는 참조되지 않는다.
  - 승인 에셋은 현행 Config 13종 중 5종만 포함하며 8종은 소스 수준 검증만 가능하다.
  - 승인 경로에는 Null이 아닌 중첩 Skill 참조가 없다.
- 결정:
  - 런타임 Resolver 동작과 도달 가능한 현행 SO 참조가 작성 JSON보다 우선한다.
  - JSON에만 있는 값은 작성 전용 출처로 보존하며 활성 게임플레이 값으로 표시하지 않는다.
  - 이 Task는 JSON/SO 불일치를 기록하지만 복구하거나 마이그레이션하지 않는다.
- 생성 파일:
  - `AgentDocs/Machal/ability-content-presentation-inventory.md`
  - `AgentDocs/Machal/ability-content-presentation-inventory-ko.md`
- 수정 파일:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- 검증 근거:
  - 모든 필수 Task 소스 및 승인 에셋 경로가 존재한다.
  - 승인된 Skill/Effect 소스와 에셋 경로에 Git 변경이 없다.
  - YAML Script GUID 분류와 참조 도달 수를 독립적으로 확인했다.
  - 영문 및 한글 인벤토리에 같은 집계와 Config 13개 행이 존재한다.
  - `AgentDocs/Machal/`에서 후행 공백과 탭 문자를 발견하지 않았다.
- 차단 요인:
  - `AgentDocs/code-writing-rules.md`가 누락되어 있다. 복구되거나 제공되기 전에는 2단계 스크립트 작업을 시작할 수 없다.
  - `AgentDocs/task-start-documentation-prompt.md`도 누락되어 지정된 문서 인계 형식을 사용할 수 없다.
- 수행하지 않은 작업:
  - 스크립트 구현, 에셋 복구, 마이그레이션, 프리팹 또는 Scene 변경, 스테이징, 커밋, 푸시
- 권장 다음 작업:
  - `AgentDocs/code-writing-rules.md`를 복구하거나 제공한 뒤 `Assets/Scripts/Presentation/` 아래의 가장 작은 호출 가능 공통 계약 Placeholder로 2단계를 시작한다.

## 2026-08-09 — 단일 Effect Resolver 설계 채택, 2단계 차단

- 상태: 설계 정정 완료, 2단계 코드는 시작하지 않음
- 사용자 결정:
  - Effect Config 매핑 규모가 작으므로 Config 전용 Resolver 클래스와 인터페이스는 불필요하다.
  - 공개 동작은 `EffectPresentationResolver` 하나로 두고 내부에서 Config를 분기한다.
  - Private 메서드는 Config 타입을 그대로 따라 만들지 않고 반복되는 정규화 결과 생성에만 추가한다.
  - Config 클래스는 Presentation 계약을 알지 않게 유지하며 `ToPresentationData()` 메서드를 추가하지 않는다.
- 활성 계획에서 제거:
  - `IEffectConfigPresentationResolver`
  - `<EffectType>PresentationResolver` 클래스
  - Config별 Resolver 클래스 표
- 남은 구현 순서:
  - 2단계 공통 중립 계약
  - 3단계 Effect 정규화 데이터와 단일 Resolver 진입점
  - 4단계 단일 Resolver 내부 Config 매핑 분기
  - 5단계 Skill 조합
  - 6단계 승인 에셋 검증
  - 7단계 현행 데이터가 확인된 콘텐츠 Adapter
  - 8단계 데이터 계층 승인 및 후속 UI 인계
- 확인된 차단 요인:
  - `AgentDocs/code-writing-rules.md`가 계속 누락되어 있다.
  - `AgentDocs/task-start-documentation-prompt.md`도 계속 누락되어 있다.
  - Machal 시작 계약은 필수 경로가 누락된 동안 구현을 금지한다.
- 수정 파일:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- 수행하지 않은 작업:
  - 스크립트 생성, Placeholder 삽입, 컴파일, 에셋 변경, 스테이징, 커밋, 푸시
- 권장 다음 작업:
  - 누락된 코드 작성 가이드를 복구하거나 명시적으로 대체하고 처음부터 끝까지 읽은 뒤 2단계를 진행한다.

## 2026-08-09 — UI 프리팹 준비 및 3단계 설계 완료

- 상태: 사용자 UI 프리팹 준비 계약 완료, 3단계 설계 완료, 스크립트 구현은 시작하지 않음
- 사용자가 진행할 수 있는 작업:
  - 범용 콘텐츠 정보 View와 재사용 가능한 Group, Entry, Tag 프리팹을 준비한다.
  - UI 준비 문서에 적힌 향후 바인딩 오브젝트 이름을 정확히 유지한다.
  - View 스크립트, 구체적인 SO 참조, Scene 바인딩, 게임플레이 값 해석을 추가하지 않고 유연한 레이아웃과 스타일을 만든다.
- UI 확인 결과:
  - `Assets/Prefabs/UIWidget/UITooltipWidget.prefab`은 짧은 단일 문자열 Tooltip이므로 의미 그룹 및 Label/Value 행을 표현하기에는 단독으로 충분하지 않다.
  - 기존 AutoBind 동작은 Prefix와 Field 이름을 조합한 정확한 자식 이름을 요구한다.
  - 준비된 구조는 Identity, 작성된 설명, 동적 Group, 조합된 짧은 값, Fallback 상태, 별도 중첩 콘텐츠 탐색을 지원한다.
- 3단계 준비:
  - 첫 Effect 데이터 범위를 `Assets/Scripts/Ability/Effects/Data/EffectPresentationData.cs`와 `Assets/Scripts/Ability/Effects/EffectPresentationResolver.cs`로 축소했다.
  - 독립 소유권으로 분리할 이유가 생길 때까지 초기 Activation, Constraint, 타입별 Outcome 레코드를 데이터 파일 하나에 둔다.
  - 단일 Resolver 표면, Null 및 미지원 Fallback 동작, Provenance 경계, 4단계 분기 순서, 검증 완료 조건을 정의했다.
  - 의미가 불분명한 레거시 `SkillEffectSO` Fallback 소유권을 4단계 Effect 매핑에서 5단계 Skill 조합으로 수정했다.
- 생성 파일:
  - `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`
  - `AgentDocs/Machal/ability-content-ui-prefab-preparation-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`
  - `AgentDocs/Machal/ability-content-presentation-stage3-preparation-ko.md`
- 수정 파일:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- 차단 상태 유지:
  - `AgentDocs/code-writing-rules.md`가 계속 누락되어 현재 작업 계약에서는 2단계 및 3단계 스크립트 작업을 시작할 수 없다.
  - `AgentDocs/task-start-documentation-prompt.md`가 계속 누락되어 지정된 문서 인계 형식을 사용할 수 없다.
- 수행하지 않은 작업:
  - 스크립트, 프리팹, Scene, 에셋, AutoBind 변경, 컴파일, 스테이징, 커밋, 푸시
- 권장 병행 다음 작업:
  - 사용자: `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`를 기준으로 바인딩되지 않은 범용 프리팹을 준비한다.
  - 에이전트: 누락 가이드가 복구된 뒤 2단계를 구현하고 검증한 다음 준비된 3단계 계약을 실행한다.

## 2026-08-09 — 루트 작업 가이드 복구

- 상태: 누락 가이드 차단 해소, 2단계 구현은 시작하지 않음
- 복구 및 검증 경로:
  - `AgentDocs/code-writing-rules.md`
  - `AgentDocs/code-writing-rules-ko.md`
  - `AgentDocs/task-start-documentation-prompt.md`
  - `AgentDocs/task-start-documentation-prompt-ko.md`
- 현재 상태 정정:
  - 복구된 Placeholder 우선 규칙에 따라 2단계를 시작할 수 있도록 README와 Task 상태를 갱신했다.
  - 이전 로그의 차단 기록은 당시 상태의 이력으로 보존했다.
- 재사용 작업 흐름 갱신:
  - 문서화 인계에서 사용자 준비 또는 외부 변경 경로를 보고하는 에이전트의 변경 경로와 분리한다.
- 검증:
  - 루트 가이드 네 경로가 모두 존재하며 엄격한 UTF-8로 읽힌다.
  - 영문과 한국어 가이드 쌍의 구조가 일치한다.
  - 갱신 파일에서 후행 공백, 탭 또는 대체 문자를 발견하지 않았다.
- 수행하지 않은 작업:
  - 런타임 코드, 프리팹, Scene, 에셋, AutoBind 변경, 컴파일, 스테이징, 커밋, 푸시
- 권장 다음 작업:
  - 복구된 코드 작성 가이드를 읽은 뒤 가장 작은 호출 가능 공통 계약을 Placeholder와 함께 구현하여 2단계를 시작한다.

## 2026-08-09 — 2단계 공통 계약 완료

- 상태: 2단계 완료, 3단계 설계 준비 완료, 3단계 스크립트는 시작하지 않음
- `Assets/Scripts/Presentation/` 아래 구현 내용:
  - 콘텐츠 ID, 표시 이름, 선택적 Icon을 가지는 `PresentationIdentityData`
  - Preview 또는 Runtime 해석 모드를 구분하는 `PresentationContext`
  - 작성 에셋, 런타임 해석, 작성 원본, 설명 Fallback 출처를 구분하는 `PresentationProvenanceData`
  - 명시적 단위와 선택적 값 단위 출처를 가지는 숫자 또는 의미 Token 값 `PresentationValueData`
  - 하나의 의미 Key, 하나 이상의 짧은 값, 선택적 상세 콘텐츠 탐색을 가지는 `PresentationEntryData`
  - 동적 의미 Section을 위한 `PresentationGroupData`
  - Identity, 작성된 설명, 분류, Group, 출처, 지원 상태를 가지는 `ContentPresentationData`
- Placeholder 우선 검증 근거:
  - 임시 `ContentPresentationData.CreatePlaceholder()`를 추가하고 실제 호출했다.
  - Smoke Harness에서 `[PLACEHOLDER] ContentPresentationData.CreatePlaceholder called; Stage 2 contract construction pending.`와 `PLACEHOLDER_REACHED`를 확인했다.
  - 최종 Constructor 기반 계약으로 교체한 뒤 Placeholder 메서드와 로그를 제거했다.
- 최종 검증:
  - 격리된 C# Smoke Build가 성공했다.
  - 최종 출력은 `STAGE2_CONTRACT_SMOKE_OK`였다.
  - 하나의 Entry에 값 2개(`Percent`, `Seconds`), Group 1개, Runtime Provenance, Preview 및 Runtime Context를 생성했다.
  - 공통 계약은 구체적인 Skill, Effect, Bless, Relic, Character SO 타입을 참조하지 않는다.
  - 새 `.cs` 파일마다 대응하는 `.cs.meta` 파일이 존재한다.
- 범위 경계:
  - Effect 정규화, Config 매핑, View, 프리팹, Scene, 에셋, AutoBind 구현은 추가하지 않았다.
  - 스테이징, 커밋, 푸시는 수행하지 않았다.
- 다음 작업:
  - 다음의 작은 작업 단위에서 준비된 3단계 Effect 모델과 단일 `EffectPresentationResolver` 진입점을 구현한다.

## 2026-08-10 — 사용자 프리팹 골격 레이아웃 완료

- 상태: 사용자가 준비한 바인딩되지 않은 콘텐츠 정보 프리팹 골격 4개에 재사용 레이아웃과 필수 UI 컴포넌트를 적용했으며 런타임 바인딩은 계속 연기한다.
- 사용자 준비 또는 외부 경로:
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoView.prefab` 및 `.meta`
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoGroup.prefab` 및 `.meta`
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoEntry.prefab` 및 `.meta`
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoTag.prefab` 및 `.meta`
  - 골격에는 `Info_*` 바인딩 이름과 중첩 Tag 및 Group 샘플 인스턴스가 포함되어 있었다.
- 에이전트가 적용한 프리팹 변경:
  - View: 고정 루트 크기, Header/Body 레이아웃, Icon/Name/Tag 영역, Masked Viewport가 있는 세로 ScrollRect, 동적 `Info_GroupRoot`, 기본 비활성 Fallback 상태.
  - Group: 세로 Title/Description/Entry 레이아웃, 선택적인 Description 기본 비활성, 재사용 가능한 Entry 컨테이너.
  - Entry: 가로 Label/Value/Action 레이아웃, 재사용 가능한 Text 크기, Image와 Button 컴포넌트를 가진 Detail Action 기본 비활성.
  - Tag: 배경 Image, 가로 콘텐츠 레이아웃, 최소 크기 LayoutElement가 있는 TMP Text.
  - Entry, Group, Tag, View의 Prefab Mode 레이아웃 재계산값을 저장했다.
- 검증:
  - 임시 일회성 Editor Builder가 `PrefabUtility`를 통해 프리팹 4개를 모두 로드하고 저장했다.
  - 완료 후 Builder와 `.meta` 파일을 제거했으며 임시 구현은 남지 않았다.
  - Unity Prefab Mode 직접 확인으로 각 Hierarchy, Component 구성, 기본 비활성 상태, View ScrollRect의 Content/Viewport 연결을 검증했다.
  - Unity가 부모 Layout 충돌을 표시한 뒤 Entry, Group, `Group_EntryRoot`, Tag에서 중복 중첩 `ContentSizeFitter`를 제거했으며 ScrollRect Content 소유자인 `Info_GroupRoot`만 Preferred Height Fitting을 유지한다.
  - Unity에서 다시 확인해 중첩 Layout 경고가 사라진 것을 검증했다.
  - Unity Console 최종 상태는 Error 0건, Warning 0건이다.
  - 프리팹 GUID는 유지됐다: Entry `47914a9dc9448b441824302050309731`, Group `369f8a836fa3e8e4b901db8988516493`, Tag `0a6b2b3bbf91f3f47832f7c2efd1f313`, View `e23588acb90e20c4a891fd2f25b2c4bd`.
  - 이전 Domain Reload에서 이 프리팹 범위 밖의 `GameObject` 필드에 대한 기존 `AutoBindEditorUtility.FindComponent` 오류가 노출됐지만 Refresh 후 현재 Console은 깨끗하다.
- 범위 경계:
  - View 스크립트, 구체적인 SO 참조, Scene 바인딩, AutoBind 컴포넌트, 게임플레이 값 포맷팅, 최종 Localization, 최종 Visual Styling은 추가하지 않았다.
  - 스테이징, 커밋, 푸시는 수행하지 않았다.
- 다음 작업:
  - 준비된 3단계 데이터 작업을 계속하고 데이터 계층 검증 및 승인 뒤에만 이 프리팹을 바인딩하고 스타일링한다.

## 2026-08-11 — 3단계 Effect 모델 및 Fallback 진입점 완료

- 상태: 3단계 완료, 4단계 Config 매핑 분기는 대기.
- 사용자 승인 설계 수정:
  - `OnHit`, `OnHeal`, `OnAttack`은 Effect가 언제 시작되는지를 나타내므로 별도 Activation Event로 유지한다.
  - Damage와 Heal은 동일한 체력값 축의 방향을 나타내므로 `EffectOutcomeKind.HealthChange`를 공유한다.
  - `HealthChangeKind`로 Damage와 Heal을, `HealthChangeApplicationKind`로 Instant와 Periodic을 구분한다.
  - 이 결정은 `Heal`과 `PeriodicDamage`를 별도 Outcome으로 나열한 이전 계획 문구를 대체한다.
- 생성 파일:
  - `Assets/Scripts/Ability/Effects/Data.meta`, GUID `ae91860cc1f542ae83a4898b96315a20`
  - `Assets/Scripts/Ability/Effects/Data/EffectPresentationData.cs` 및 `.meta`, 스크립트 GUID `7e049d48d2a54edeaff2ad8dbf22924c`
  - `Assets/Scripts/Ability/Effects/EffectPresentationResolver.cs` 및 `.meta`, 스크립트 GUID `ad09cd55d09340a39b28a6cb9a0b8995`
- 구현:
  - Typed Activation, Entry 제약, Outcome Kind, HealthChange Amount/Basis/Rate 데이터와 나머지 의미 Outcome Payload를 추가했다.
  - 내부 Config Switch를 가지며 Config별 Resolver 계층은 만들지 않은 단일 공개 `EffectPresentationResolver.Resolve(EffectEntrySO, PresentationContext)` 진입점을 추가했다.
  - 3단계는 의도적으로 Null, 미지원, 작성 설명 Fallback 동작만 구현했다. Config 분기는 4단계 작업으로 남겼다.
  - Entry Duration은 Timed 및 CombatTimed Lifetime에만 노출하고, MaxApplyCount는 실제 필드만 사용한다. `ValueOverride`와 Upgrade Modifier는 노출하지 않는다.
- Placeholder 우선 근거:
  - 임시 Harness에서 `[PLACEHOLDER] EffectPresentationResolver.Resolve called; Stage 3 fallback behavior pending.`와 `PLACEHOLDER_CALL_COMPLETED`를 확인했다.
  - 결정적 Fallback 동작으로 교체한 뒤 Placeholder를 제거했다.
- 최종 검증:
  - 격리 Smoke 결과는 `STAGE3_EFFECT_PRESENTATION_SMOKE_OK`였다.
  - Null Entry, Null Effect, Fallback/Provenance, Timed 및 Instant 제약, Seconds 및 Count 단위, OnHit/OnHeal Trigger 구분, 하나의 HealthChange Outcome 안에서 Damage/Heal 방향 구분을 검증했다.
  - Unity가 두 신규 소스를 `Assembly-CSharp.csproj`에 포함하고 `Library/ScriptAssemblies/Assembly-CSharp.dll`을 `2026-08-11 02:15:43`, `1,255,424`바이트로 다시 생성했다.
  - Domain Reload에서 범위 밖의 기존 `AutoBindEditorUtility` `GameObject` 필드 오류가 다시 노출됐지만 신규 스크립트 컴파일 오류는 없었고, 검증 뒤 Console을 Error 0건, Warning 0건으로 정리했다.
- 범위 경계:
  - 이 단위에서는 Config, SO 에셋, 레거시 에셋, View, 프리팹, Scene, AutoBind 동작을 변경하지 않았다.
  - 스테이징, 커밋, 푸시는 수행하지 않았다.
- 다음 작업:
  - 승인된 연결 에셋 묶음부터 4단계 Config 매핑을 한 분기씩 구현하고 공개 계약을 바꾸지 않은 채 각각 검증한다.

## 2026-08-11 — 4단계 Effect Config 매핑 및 사용자 자체 테스트 완료

- 상태: 4단계 완료, 다음 작업은 5단계 Skill 조합.
- Production 구현:
  - 단일 `Assets/Scripts/Ability/Effects/EffectPresentationResolver.cs`에 현행 Config 분기 13종을 모두 추가했다.
  - 3단계 공개 계약을 유지하고 Outcome 기준으로 묶은 Private 생성 Helper만 추가했다.
  - 모든 Activation Chance를 `0..100 Percent`로 정규화하고 직접 Scaling Ratio와 Percentage Point 값은 구분했다.
  - Heal과 Periodic Damage를 공통 `HealthChange` Outcome으로 매핑했다.
  - 호출 Skill Null, ChanceOnHit Multiply 미지원, OnHitTimed Duration Stat Max-Set 사례에 결정적 Fallback을 추가했다.
- 런타임 정확성 결정:
  - 런타임이 `ChanceOnHealStatModifier.ValueType`을 무시하므로 활성 Presentation Operation은 Flat이다.
  - Heal은 `CharacterDamageService`에서 항상 Clamp되므로 사용되지 않는 `ClampToMaxHp` Config Flag를 표시하지 않는다.
  - 사용되지 않는 `ChanceOnHitSkill.RangeOverride`를 제외했다.
  - Critical 요구 조건은 보존하지만 현재 `EffectManager`가 실제 Hit Critical 결과 대신 `true`를 전달하는 게임플레이 연결 문제는 해결하지 않고 기록만 했다.
  - Distance Displacement는 현행 런타임과 같게 Pull이 아닌 방향을 Push로 합쳤다.
- 사용자 테스트 도구 생성:
  - `Assets/Editor/tools/effect/EffectPresentationStage4SelfTest.cs`
  - `Assets/Editor/tools/effect/EffectPresentationStage4SelfTest.cs.meta`, GUID `32174dd94bfa44ff9f2b77e939a7644e`
  - 전체 테스트 메뉴: `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`
  - 선택 Entry 로그 메뉴: `Assets > ProjectBS > Presentation > Log Selected Effect Entry`
- 검증:
  - `dotnet build Assembly-CSharp.csproj --no-restore`가 Error 0건, 기존 Warning 35건으로 완료됐다.
  - Unity가 `Library/ScriptAssemblies/Assembly-CSharp.dll`을 `2026-08-11 03:05:25`, `1,260,032`바이트로 다시 생성했다.
  - 최종 Unity 자체 테스트 출력: `[EffectPresentationStage4SelfTest] PASS`, `Synthetic config mappings: 13`, `Approved EffectEntry assets: 20`.
  - 최종 Unity Console: PASS Log 1건, Warning 0건, Error 0건.
- 문서:
  - 정확한 테스트 절차와 런타임 Gap을 담은 `AgentDocs/Machal/ability-content-presentation-stage4-verification.md` 및 한국어 Mirror를 추가했다.
- 범위 경계:
  - Config, 게임플레이 Runtime 클래스, SO 에셋, 레거시 에셋, 프리팹, Scene, AutoBind 동작은 변경하지 않았다.
  - 스테이징, 커밋, 푸시는 수행하지 않았다.
- 다음 작업:
  - 5단계 Skill 조합을 시작하고 정규화 Effect 결과를 재사용하며, 조합 데이터 계약 승인 전까지 플레이어 UI 프리팹 바인딩을 연기한다.

## 2026-08-11 — 5-8단계 코드 완료, Unity 인계 대기

- 상태: 요청된 코드 작업 완료, 중첩 Skill 순회 보류, 사용자 담당 Unity 작업 대기.
- 완료 범위:
  - Skill 분류, Typed 조합, Preview/Runtime Provenance, 의미 그룹화, 설명 전용 레거시 `SkillEffectSO` Fallback을 추가했다.
  - 승인 경로 Skill 검증 행렬, 선택 Skill 로그, 대화형 Editor Preview Window를 추가했다.
  - 제외된 레거시 에셋을 읽거나 변경하지 않고 현행 정의 Character, Bless, Relic Adapter를 추가했다.
  - Compact Text Formatter와 공통 Content View, Group, Entry, Tag 스크립트를 추가했다.
- 사용자 결정:
  - 이번 작업 단위에서 중첩 Skill 순회나 확장 상세를 구현하지 않는다.
  - 에이전트가 Unity Editor 기능을 사용하지 않는다. 필요한 프리팹 및 Unity 인계 지점에서 멈추고 사용자가 검증을 실행한다.
- 검증:
  - `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`: 오류 0개.
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 최종 실행에서 경고 0개, 오류 0개, `Assembly-CSharp`와 `Assembly-CSharp-Editor` 모두 생성.
  - 새 `.meta` GUID 22개가 각각 `Assets/` 아래에서 정확히 한 번 발견됐다.
  - 범위 내 `git diff --check`가 통과했고 임시 Placeholder가 남아 있지 않다.
  - Unity Editor를 열거나 조작하지 않았다.
- 대기 작업:
  - 사용자가 준비된 `UIContentInfo*` 프리팹에 공통 View 스크립트 4개와 직렬화 참조를 연결한다.
  - 사용자가 이후 실제 승인 Skill 에셋에 Skill 검증 메뉴를 실행한다.
  - Character/Bless/Relic의 승인된 현행 에셋 경로와 에셋 수준 검증은 대기한다.
  - Scene 통합, AutoBind, 최종 Localization, 콘텐츠별 Presenter는 별도 후속 작업으로 유지한다.
- 완료 참고 문서:
  - `AgentDocs/Machal/ability-content-presentation-stage5-8-completion-ko.md`

## 2026-08-11 — UIContentInfo 계층 AutoBind 수정

- 상태: 소스 수정 완료, 사용자 담당 Unity 갱신 및 프리팹 저장 대기.
- 사용자 수정 요청:
  - 네 View 컴포넌트는 프리팹에 부착되었지만 직렬화된 계층 필드에 프로젝트 AutoBind 시스템이 적용되지 않았다.
- 변경 스크립트:
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoView.cs`
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoGroupView.cs`
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoEntryView.cs`
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoTagView.cs`
- 구현:
  - 메인 View는 `UIView`, 재사용 자식 View 세 개는 `UIComponent`를 상속하도록 변경했다.
  - 기존 `Info_*`, `Group_*`, `Entry_*`, `Tag_*` 계층 이름에 대응하는 `AutoBindPrefix`를 추가했다.
  - 계층 컴포넌트 필드에만 `AutoBind`를 추가했다.
  - 현재 AutoBind 도구는 프리팹 에셋 참조가 아니라 자식 컴포넌트를 해석하므로 `tagPrefab`, `groupPrefab`, `entryPrefab`은 수동 연결로 유지했다.
- 검증:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`가 오류 0개로 완료됐으며 기존 프로젝트 경고 191개가 출력됐다.
  - 정적 프리팹 검사에서 네 스크립트 GUID가 부착되어 있고 필요한 AutoBind 대상 오브젝트 이름이 모두 존재함을 확인했다.
  - Unity 갱신 전 프리팹 YAML의 계층 참조와 템플릿 프리팹 참조는 여전히 Null이었다.
  - 에이전트는 Unity Editor를 열거나 조작하지 않았다.
- 사용자 다음 작업:
  - 스크립트 갱신을 기다린 뒤 각 프리팹을 열거나 검증하고, 템플릿 프리팹 에셋 필드 세 개를 수동 지정해 네 프리팹을 저장한 다음 Console 오류 또는 남은 `None` 계층 필드를 보고한다.

## 2026-08-11 — 6단계 Null Effect 슬롯 검증 수정

- 상태: 소스 수정 완료, 사용자 Unity 재실행 대기.
- 수정 전 사용자 결과:
  - 승인 Skill 58개를 해석했다.
  - 검증에서 지원 Effect 18개, 미지원 Effect 10개와 실패 10건이 보고됐다.
- 원인:
  - 보고된 레코드 10개는 모두 현행 Cast 에셋 9개에 `{fileID: 0}`으로 직렬화된 Null `EffectEntrySO` 요소였다.
  - `skill.character.military_officer.1.passive_1.unyielding_will`에는 Null 슬롯이 2개라 실패 경로가 중복됐다.
  - 이 실패 묶음에서 구체적인 미지원 Effect 타입은 발견되지 않았다.
  - 1단계에서 이미 이 Placeholder들을 비활성 Effect로 분류했으며 게임플레이 `EffectResolver.ResolveEntries`도 Null Entry를 건너뛴다.
- 변경 경로:
  - `Assets/Scripts/Ability/Skills/SkillPresentationResolver.cs`
  - `Assets/Editor/tools/skill/SkillPresentationStage6Validation.cs`
- 수정:
  - Skill Presentation이 `EffectPresentationResolver` 호출 전에 Null Effect 슬롯을 건너뛴다.
  - 검증 행렬은 Null 슬롯을 미지원 Effect로 분류하지 않고 별도로 집계한다.
- 검증:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 오류 0개, 기존 프로젝트 경고 191개.
  - Skill, Cast, Effect, JSON, 레거시 에셋은 변경하지 않았다.
  - 에이전트는 Unity Editor를 열거나 조작하지 않았다.
- 예상 사용자 재실행 결과:
  - `Supported / description-only / unsupported Effects: 18 / 0 / 0`
  - `Ignored null EffectEntry slots: 10`
  - `Failures: 0`

## 2026-08-11 — 6단계 사용자 PASS 및 Skill UI Preview 도구

- 상태: 승인 Skill 데이터 검증 완료, 사용자 시각 UI Preview 대기.
- 사용자 Unity 결과:
  - `[SkillPresentationStage6Validation] PASS`
  - 승인 고유 Skill 58개, 해석된 Skill 58개
  - Hit 없음 / Effect 없음 / Effect 1개 / 여러 Effect: `7 / 42 / 14 / 2`
  - 지원 / 설명 전용 / 미지원 Effect: `18 / 0 / 0`
  - 무시한 Null EffectEntry 슬롯: `10`
  - Ratio / Percent 값: `120 / 102`
  - 실패: `0`
- 사용자 실행 Editor 도구 추가:
  - `Assets/Editor/tools/skill/SkillPresentationUIPreviewTool.cs` 및 `.meta`
  - View, Group, Entry, Tag 프리팹의 직렬화 참조 17개를 검사한다.
  - Play Mode에서 임시 `DontSave` Overlay Canvas를 만들고 선택한 승인 Skill을 `UIContentInfoView`에 Bind한다.
  - 작성 Preview 값과 Runtime 레벨 1 값을 지원한다.
  - Scene을 저장하거나 프리팹을 변경하지 않는다.
- 검증:
  - 컴파일 검사를 위해 신규 소스를 `Assembly-CSharp-Editor.csproj`에 명시적으로 포함한 뒤 생성 프로젝트 파일을 원복했다.
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 오류 0개, 기존 프로젝트 경고 156개.
  - 신규 도구 Script GUID는 `7df02ec2672b45d0ad485ba3956266ff`다.
  - 에이전트는 Unity 시각 Preview를 실행하지 않았다.
- 사용자 다음 작업:
  - `Tools > ProjectBS > Presentation > Validate Content UI Prefab Bindings`를 실행한다.
  - 승인 Skill을 선택하고 Play Mode에 들어간 뒤 Asset 메뉴에서 작성 값 또는 Runtime UI Preview를 실행한다.

## 2026-08-11 — 기존 UI Presenter, 의미 필터링, 스크롤 수정

- 상태: 소스 구현 및 비-Unity 컴파일 완료, 사용자 Unity 검증 대기.
- 사용자 수정 요청:
  - Asset/Tools 메뉴가 임시 Canvas와 UI를 만들지 않도록 한다.
  - 컴포넌트에서 현재 Skill SO와 기존 `UIContentInfoView`를 지정하고 `Build Presentation` 메뉴로 표시한다.
  - `0s`, `0m`, `999m` 및 기본 비활성 표현을 게임플레이 의미에 따라 걸러낸다.
  - `Info_GroupRoot`의 ScrollRect 갱신과 입력 조건을 바로잡는다.
- 변경:
  - `Assets/Scripts/Ability/Skills/UI/SkillContentInfoPresenter.cs` 및 `.meta` 추가.
  - `Assets/Scripts/Ability/Skills/SkillPresentationGroupResolver.cs` 수정.
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoView.cs` 수정.
  - `Assets/Editor/tools/skill/SkillPresentationUIPreviewTool.cs` 및 `.meta` 제거.
- 구현:
  - Play Mode에서 `Build Presentation`을 실행하면 작성값 또는 런타임 계산값을 지정된 기존 View에 Bind한다.
  - Presenter는 Canvas, 프리팹 인스턴스, Scene 오브젝트를 만들지 않는다.
  - ScrollRect 입력에 필요한 활성 `EventSystem`이 없으면 경고한다.
  - Skill 그룹의 0 기본값, 기본 수/배율, `999` 센티널, 기본 비활성 플래그와 빈 그룹을 생략한다.
  - Bind 후 기존 자식을 비활성화하고 레이아웃과 ScrollRect 범위를 강제로 갱신한다.
- 검증:
  - `dotnet build Assembly-CSharp.csproj --no-restore`: 오류 0개, 기존 경고 35개.
  - Unity Editor는 에이전트가 열거나 조작하지 않았다.
- 사용자 다음 작업:
  - Unity가 새 컴포넌트를 Import하도록 한 뒤 기존 UI에 붙이고 View와 Skill을 지정한다.
  - Play Mode에서 `Build Presentation`을 실행하여 스크롤 휠/드래그와 기본값 제거 결과를 확인한다.

## 2026-08-11 — Skill 원본 검사 도구와 플레이어 UI 필터 분리

- 상태: 소스 정정 및 비-Unity 컴파일 완료, 사용자 Unity 확인 대기.
- 사용자 정정:
  - Skill Presentation 검사 도구는 `0`, `999` 같은 원본/기본 확인 값도 계속 표시한다.
  - 기본값 필터는 플레이어에게 보이는 콘텐츠 정보 UI에만 적용한다.
- 정정 내용:
  - `SkillPresentationGroupResolver.Resolve()`는 기존 Editor 도구와 검증이 사용하는 전체 비필터 그룹 경로로 복구했다.
  - `ResolveForPlayerDisplay()`를 명시적인 필터 경로로 추가했다.
  - `SkillContentInfoPresenter`만 `ResolveForPlayerDisplay()`를 호출한다.
- 검증:
  - `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:minimal`: 오류 0개, 기존 프로젝트 경고 35개.
  - Unity Editor는 에이전트가 열거나 조작하지 않았다.

## 2026-08-11 — 계약 재평가 및 원본 충실 Group 교정

- 상태: 소스 교정 및 정적 컴파일 완료, 사용자 Unity 검증 대기.
- 사용자 정정:
  - 승인된 Effect Outcome 7개인 StatModifier, Heal, CooldownChange, Displacement, PeriodicDamage, SkillInvoke, Control을 복구한다.
  - 선택적인 Activation을 정규화 Outcome과 연결된 상태로 유지한다.
  - `Assets/Resources/skill/json/`의 실제 필드를 기준으로 Skill 표시 데이터를 만들고 번호나 임의 Group Key를 만들지 않는다.
  - 원본 필드 하나를 Entry 하나로 변환한다. Label은 의미를 번역할 수 있지만 서로 다른 원본 값을 결합하거나 대체하지 않는다.
- 평가:
  - 전략 Skill JSON 20개를 UTF-8로 전수 파싱했다.
  - 파일에는 `tags`라는 이름의 속성이 없다. 분류는 `skillType`, `skillComponentType`, `brainMeta.category`, `brainMeta.targetType`, `brainMeta.tacticalNeed`, `targetLayerMask`, `effectType`, `categoryType`, `lifetimeType`에 있다.
  - 기존 표시 Key `Skill.Hit.<번호>.Damage`, `Skill.Effect.*.<번호>`, `Behavior`, `CountAndScale`, `SizeAndLifetime`은 원본 JSON이나 승인 정규화 계약에 속하지 않았다.
  - 중간 단계의 공통 `HealthChange` Outcome은 사용자가 다시 확정한 최신 7개 Outcome 계약과 충돌했다.
- 교정:
  - Typed `HealPresentationData`와 `PeriodicDamagePresentationData`를 복구했다.
  - Skill Group은 적용 가능한 경우 원본 객체 Key `cast`, `baseProfile`, `move`, `hits`, `spawnSkill`을 사용한다.
  - Effect Group은 정규화 Outcome 종류를 사용하고 실제 Effect ID를 `PresentationGroupData.SourceContentId`에 보존한다.
  - 교정된 모든 Skill/Effect Entry는 Value를 최대 하나만 가진다.
  - 원본 Key 및 정규화 컴포넌트 기반 Label/Token Localization을 위해 `Assets/Resources/string/presentation_string.csv`를 추가했다.
  - `SkillPresentationStage6Validation`에 원본 충실 Grouping 검사를 추가했다.
  - `AgentDocs/Machal/ability-content-presentation-contract-evaluation.md`와 한국어 문서를 추가했다.
- 검증:
  - `presentation_string.csv`: 데이터 132행, 중복 Key 0개.
  - 신규 CSV meta GUID는 정확히 한 번 존재한다.
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 오류 0개, 기존 프로젝트 경고 191개.
  - Unity Editor는 에이전트가 열거나 조작하지 않았다.
- 사용자 다음 작업:
  - Unity에서 Effect 자체 테스트와 Skill 에셋 검증을 실행한다.
  - `Build Presentation`을 실행하여 효과 범위와 지속시간이 분리된 행, 정규화 Outcome Group Title, 원본 분류 Tag, 번호 없는 Group Title을 확인한다.

## 2026-08-11 — 선택적 설명 Localization과 전략 스킬 키 소유 경로

- 상태: 소스 수정과 정적 검증 완료, 사용자 Unity Localization 확인 대기.
- 원인:
  - Presentation은 이미 `StringManager`를 호출했지만 누락 키가 키 문자열 자체로 반환되어 `.desc`로 끝나는 값이 작성된 UI 문장처럼 노출됐다.
  - 현행 전략 Skill ID는 `skill.strategic.*`이지만 실제 번역 설명은 `Assets/Resources/string/string_table.csv`의 `item.strategic.*`가 소유한다.
  - 현행 Effect ID에는 대응하는 `desc` 행이 없으므로 구조화된 Effect 그룹만 표시하고 누락 설명 키는 표시하지 않아야 한다.
- 수정:
  - Skill 설명은 정확한 키를 선택 조회한 뒤 전략 Skill에만 `item.strategic.*.desc` Fallback을 적용한다.
  - Effect, Relic, Bless Presentation 설명은 `StringManager` 선택 조회를 사용하고 누락 행은 원시 키 대신 빈 설명으로 처리한다.
  - Localization CSV, Skill, Effect, Bless, Relic, Legacy 에셋은 변경하지 않았다.
- 검증:
  - 승인 전략 Skill ID 20개 모두 `item.strategic.*.desc` 행과 대응한다.
  - `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:minimal`: 오류 0개, 기존 프로젝트 경고 35개.
  - Unity Editor는 에이전트가 열거나 조작하지 않았다.

## 2026-08-11 — 문서 관리자 계약 일관성 검토

- 상태: 현재 영문/한국어 문서를 권위 있는 7개 Outcome 및 원본 충실 Grouping 계약에 맞게 정리했으며 사용자 Unity 검증은 계속 대기한다.
- 정정:
  - `Scaling`, `Persistence`, `Constraints` 같은 임의 Effect 표시 하위 Group을 허용하던 이전 문구를 제거했다. Activation과 Entry 제약은 정규화 Outcome Group 안에서 각각 별도 Entry로 유지한다.
  - 이전 의미 기반 Skill Group 요약을 보존된 JSON 객체 Key `cast`, `baseProfile`, `move`, `hits`, `spawnSkill`로 교체했다.
  - UI 가이드의 결합 값 허용 문구를 Row마다 원본 값 하나만 표시하는 규칙으로 교체했다.
  - 4단계 Console 검사 항목이 UI Group Key가 아니라 Typed 필드라는 점을 명확히 했다.
- 검증:
  - 지정된 영문 원본과 한국어 Mirror 문서는 모두 행 수와 Heading 수가 일치한다.
  - 현재 계약 문서에는 계약 평가의 명시적인 대체 및 제거 설명을 제외하고 이전 `HealthChange`, 결합 값, 번호형 Group 규칙이 남아 있지 않다.
  - 전략 Skill JSON 20개가 Strict UTF-8로 파싱됐고 현재 `presentation_string.csv`는 데이터 132행, `main_key`/`sub_key` 복합 Key 중복 0개다.
  - 문서는 Strict UTF-8 및 후행 공백/Tab 검사를 통과했다.
  - 과거 로그 항목은 수정하지 않았으며 후속 계약 재평가 항목이 이를 대체한다.
  - 이번 검토에서는 Runtime 소스, 에셋, 프리팹, Scene, 빌드, Unity Editor, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-11 — 작성 숫자 원본 충실도 정정

- 상태: 구현 및 정적 검증 완료, 사용자 Unity 재실행 대기.
- 정정:
  - 작성 Presentation 값은 Presentation 계층에서 Clamp, 최솟값 대체, Ratio-to-Percent 숫자 변환을 하지 않고 원본 숫자와 원본 단위를 보존한다.
  - `Chance`는 Ratio, `ChancePercent`는 Percent로 유지한다. Formatter는 정규화 데이터를 바꾸지 않고 Ratio를 백분율로 표시할 수 있다.
  - Skill Preview는 작성된 이동 속도, 피해, 공격 배율, 최대 적중 횟수를 보존한다. Runtime Presentation은 Runtime Stat Resolver가 만든 값을 계속 사용하고 Runtime Provenance로 표시한다.
  - Effect 자체 테스트는 Ratio 기반 Activation과 작성된 Poison Interval을 직접 검증하도록 수정했다.
- 검증:
  - Editor Assembly 정적 빌드가 오류 0개, 기존 경고 191개로 완료됐다.
  - Unity Editor 검증은 에이전트가 실행하지 않았고 이전 Unity PASS는 이번 정정 전 결과다.

## 2026-08-11 — 문서 관리자 숫자 충실도 및 UI 소유 검토

- 상태: 현재 영문/한국어 가이드를 정리했으며 Unity 재실행과 UI 결정 두 건은 계속 대기한다.
- 정정:
  - 값을 표준 표현으로 바꾼다는 문구를 작성 숫자, 단위, Provenance 보존 규칙으로 교체했다. Ratio의 백분율 변환은 Formatter 표시에서만 수행하며 정규화 데이터를 바꾸지 않는다.
  - 이전 Effect 및 Skill 58개 Unity PASS를 현재 검증이 아니라 수정 전 이력으로 표시했다.
  - 구체적인 `EquipmentSkillSO`와 `Build Presentation` 소유는 `SkillContentInfoPresenter`에 유지했다. 이를 중립 `UIContentInfoView`로 옮기려면 명시적인 소유 결정이 필요하다.
  - Viewport에 Raycast 가능한 `Graphic`이 없음을 기록했다. 투명 Raycast Target `Image` 추가는 사용자 Unity Wheel/Drag 결과에 따른다.
- 검증:
  - 정적 Prefab YAML에서 Viewport는 `RectTransform`과 `RectMask2D`를 가지지만 `Graphic` 컴포넌트는 가지지 않음을 확인했다.
  - 이전에 보고된 Editor Assembly 빌드는 오류 0개, 기존 경고 191개 상태로 유지하며 문서 관리자가 빌드나 Unity Editor 실행을 반복하지 않았다.
  - 이번 검토에서는 Runtime 소스, 에셋, 프리팹, Scene, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-11 — Skill 의미 재그룹화와 특수 효과 정정

- 상태: 코드와 현재 영문/한국어 계약 문서를 수정했으며 정적 빌드와 사용자 Unity 재실행 결과는 별도로 기록한다.
- 결정:
  - Effect의 7개 Typed Outcome을 내부 정규화 모델로 유지한다.
  - Effect마다 Group을 만들던 Skill 표시를 `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, `LinkedSkill`의 다섯 통합 역할 Group으로 교체한다.
  - 원본 필드 하나당 Entry 하나와 Entry 하나당 Value 하나를 유지한다. Group 통합은 원본 데이터를 결합, 유도, Clamp, 대체하지 않는다.
- 특수 효과 매핑:
  - `StunDuration` 또는 `RootDuration`을 대상으로 하는 `OnHitTimedStatModifierEffectConfig`는 런타임이 `config.Value`를 Max-Set 제어 타이머로 적용하므로 `Activation(OnHit + Chance) + Control(Stun/Root + duration)`으로 정규화한다.
  - Taunt는 `Control`, Knockback과 Pull은 구체 Config에 따른 방향과 Force 또는 Distance를 보존하는 `Displacement`로 유지한다.
  - `Control`과 `Displacement`는 `SpecialEffect`, `SkillInvoke`는 `LinkedSkill`로 보낸다.
- 검증 변경:
  - Skill 에셋 검증은 다섯 의미 Skill Group Key만 허용하고 중복 Group을 거부하며 정규화 Entry의 Group 분배를 검사한다.
  - Effect Synthetic 검증에 Stun과 Root Case를 추가했다. 예상 출력은 Config Class 13종에 대한 `Synthetic mapping cases: 15`다.
  - 다섯 Skill Group과 Stun/Root를 포함한 Group 및 Token Label은 `Assets/Resources/string/presentation_string.csv`에서 관리한다.
- 정적 검증:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 오류 0개, 기존 프로젝트 경고 156개.
  - `presentation_string.csv`: 데이터 140행, 대소문자 무시 복합 Key 중복 0개, 신규 Group/Control Key마다 정확히 1행.
  - 전략 Skill JSON 20개 모두 Strict UTF-8 JSON으로 파싱됐다.
  - Unity Editor는 에이전트가 열거나 조작하지 않았다. Grouping 계약이 변경되었으므로 Effect 및 Skill 검증은 사용자가 다시 실행해야 한다.
- 대체 관계:
  - 이 항목은 JSON 객체 이름으로 Skill Row를 Grouping하거나 Effect Outcome마다 표시 Group을 만들던 이전 활성 규칙을 대체한다. JSON 필드 Provenance와 Typed Effect Outcome은 새 Skill-level 통합 아래에 계속 보존한다.

## 2026-08-11 — 문서 관리자 의미 재그룹화 정합성 반영

- 상태: 현재 영문/한국어 가이드를 Skill 5개 Group 표시 계약에 맞췄으며 사용자 Unity 재실행은 계속 대기한다.
- 정합성 반영:
  - Effect의 7개 Typed Outcome은 내부 정규화로 유지하고 `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, `LinkedSkill`만 승인된 Skill UI Group으로 기록했다.
  - Group 통합 과정에서도 원본 필드 하나당 Entry 하나, Entry 하나당 Value 하나를 유지했다.
  - Stun과 Root는 원본 기반 `Control` 특수 효과, Taunt는 `Control`, Knockback/Pull은 원본 기반 `Displacement`로 기록했다.
  - 현재 상태와 완료 근거를 오류 0개, 기존 경고 156개의 최신 정적 빌드에 맞췄고 이전 Unity PASS는 과거 이력으로 유지했다.
- 검증:
  - 현재 가이드에는 JSON 객체 Key로 Skill 표시 Group을 만들거나 Effect마다 UI Group을 만드는 활성 규칙이 남아 있지 않다.
  - `presentation_string.csv`는 데이터 140행, 대소문자 무시 복합 Key 중복 0개이며 다섯 Group Key와 Stun, Root마다 행 하나가 있다.
  - 전략 Skill JSON 20개 모두 Strict UTF-8 JSON으로 파싱됐다.
  - 문서 관리자는 Runtime 소스, Localization 에셋, 프리팹, Scene, 빌드, Unity Editor, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-12 — StringManager 기반 플레이어 표시 카탈로그

- 상태: 구현과 정적 검증 완료, 사용자 Unity 검증 대기.
- 원본 조사:
  - `Assets/Resources/skill/json/`의 현행 파일 20개에서 모든 JSON Property Path를 다시 추출했다.
  - Skill Type, Component, Category, Target, Tactical Need, Targeting, Arrangement, Move, DamageType, Effect Type, Stat Type의 현행 분류값을 확인했다.
- 구현:
  - Group, Entry, Tag Key의 명시적 플레이어 표시 Allowlist를 추가했다.
  - Group/Entry/Tag 라벨, 문맥별 Enum 대체 단어, Stat 단어, Damage/Control/Displacement 값 포맷의 표준 StringManager Key를 추가했다.
  - 기존 Skill/Bless/Relic 이름과 설명 경로 및 기존 대체 동작은 변경하지 않았다.
  - 검사 출력은 원본/기본값과 원본 Key를 유지하고 플레이어 UI만 엄격한 카탈로그 조회와 조건부 필터를 사용한다.
  - 구형 에셋을 건드리지 않고 Bless/Relic Effect 조합에 플레이어 표시 필터를 추가했다.
- 문서:
  - 전체 표시/내부 필드 목록과 유지보수 계약을 담은 `AgentDocs/Machal/ability-content-presentation-display-catalog.md` 및 한국어 Mirror를 추가했다.
  - README 라우팅, 현재 Task, 계약 평가, 5-8단계 완료 문서를 갱신했다.
- 검증:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 오류 0개, 기존 프로젝트 경고 191개.
  - `Assets/Resources/string/presentation_string.csv`: 데이터 294행, `presentation.*` Main Key 154개, 복합 Key 중복 0개.
  - 에이전트는 Unity Editor를 열거나 조작하지 않았다.

## 2026-08-12 — 문서 관리자 표시 카탈로그 정합성 반영

- 상태: 현재 영문/한국어 가이드를 엄격한 플레이어 표시 카탈로그에 맞췄으며 사용자 Unity 검증은 계속 대기한다.
- 정합성 반영:
  - 원본 Source/정규화 경로를 플레이어 Label에 사용하던 남은 문구를 `PresentationDisplayCatalog`에서 표준 `presentation.*` Key를 조회하는 규칙으로 교체했다.
  - 원본 Key와 임의 대체 동작은 검사/디버그 Formatter에만 유지했다. 플레이어 Formatter는 Catalog 또는 Localization 텍스트가 없으면 생략한다.
  - 기존 Skill/Bless/Relic 이름과 설명 조회 및 Fallback 경로는 변경하지 않았다.
  - 현재 검증과 재실행 상태를 이전 의미 재그룹화 빌드가 아니라 플레이어 표시 카탈로그 빌드에 맞췄다.
- 검증:
  - 현재 근거는 빌드 오류 0개, 기존 경고 191개, Localization 데이터 294행, `presentation.*` Main Key 154개, 복합 Key 중복 0개, 정적으로 필요한 Catalog Key 141개 중 누락 0개를 기록한다.
  - 표시 카탈로그 영문/한국어 문서는 Heading 구조가 일치한다.
  - 문서 관리자는 Runtime 소스, Localization 에셋, 프리팹, Scene, 빌드, Unity Editor, Staging, Commit, Push 작업을 수행하지 않았다.
  - 프리팹, Scene, 구형 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-12 — 누락 Localization Key 표시 정정

- 상태: 재사용 Localization 가이드 정정 완료, 사용자 Unity 화면 검증 대기.
- 정정:
  - 매핑된 Localization 텍스트 또는 설명이 누락되면 플레이어 텍스트를 비운다는 이전 규칙을 대체했다.
  - 표시 Catalog 매핑이 없는 미승인 원본 JSON/C# Key는 계속 필터링한다.
  - 승인된 매핑 Key와 필수 이름/설명 Key에 StringManager 행이 없으면 의도한 전체 `mainKey.subKey`를 표시하도록 기록했다.
  - 순서가 있는 후보 조회를 기록했다. `returnNullIfMissing: true`로 후보를 확인하여 처음 해석된 값을 사용하고, 모두 실패하면 첫 번째 의도 Key를 일반 조회한다.
  - 기존 후보 Key 경로와 순서, StringManager를 사용할 수 없을 때의 에셋 이름 Fallback, 설명 문장 창작 금지는 유지했다.
- 원본 Task 검증 근거:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 오류 0개, 기존 프로젝트 경고 191개.
- 대기:
  - 사용자가 Unity에서 승인된 매핑 Key를 임시로 존재하지 않는 값으로 바꿔 전체 Key 표시를 확인한 뒤 원복한다.
  - 문서 관리자는 Runtime 소스, Localization 에셋, 프리팹, Scene, 빌드, Unity Editor, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-13 — CharacterSO Skill 아이콘 탭

- 상태: 소스 구현 완료, 사용자 Unity 연결 및 검증 대기.
- 구현:
  - `CharacterSO.Skills`의 null이 아닌 Skill 참조마다 아이콘 탭 하나를 만들고, 작성 순서를 보존하며, 하나의 선택 상태와 최초 선택 탭을 관리하는 `CharacterSkillContentInfoPresenter`를 추가했다.
  - 아이콘 연결, 클릭 전달, 선택 화면 옵션을 담당하는 `SkillContentInfoTabButton`을 추가했다.
  - 기존 `SkillContentInfoPresenter`에 `ShowSkill`과 `ClearPresentation`을 추가했다. Character 조합은 기존 Skill 정규화, 플레이어 Grouping, StringManager 포맷, `UIContentInfoView` 연결 경로를 재사용한다.
  - `UIContentInfoView`는 콘텐츠 중립 상태로 유지했다.
- 프리팹 확인:
  - `Assets/Prefabs/UI/Child/Slot/UISkillIconSlot.prefab`에 필요한 루트와 `Bind_SkillIconImage` 하이어라키가 있지만 현재 `Button`과 `SkillContentInfoTabButton`은 없다.
  - Unity 작업은 사용자 담당이므로 프리팹과 Scene을 수정하지 않았다.
- 검증:
  - 소스와 프리팹 YAML 확인을 완료했다.
  - `dotnet build ProjectBS.sln --no-restore`는 샌드박스가 `C:\Users\machal89\AppData\Local\Microsoft SDKs`에 접근하지 못해 컴파일 전에 중단됐다.
  - 신규 소스 파일은 Unity 프로젝트 파일 갱신, 컴파일, AutoBind, 버튼, 선택, 탭 재빌드, 스크롤 검증 대기 상태다.
- 문서:
  - 소유권, 연결 절차, 수동 검증 체크리스트를 담은 `AgentDocs/Machal/character-skill-content-tabs.md`와 한국어 Mirror를 추가했다.

## 2026-08-13 — Character JSON 비교와 플레이어 정보 UI

- 상태: 소스 구현과 정적 검증 완료, 사용자 Unity 연결 및 화면 검증 대기.
- 원본 인벤토리:
  - `Assets/Resources/character/json/`에서 현행 JSON 22개와 생성 CharacterSO 22개를 확인했다.
  - 현재 JSON Key는 `characterId`, `name`, `characterType`, `job`, `baseStats`이며 생성된 Animation 및 Skill 참조는 SO 전용 시스템 데이터다.
- 플레이어 표시 계약:
  - StringManager 기반 이름, 원본 `characterType`, 원본 `job`, 현재 원본 Stat 7개를 표시한다.
  - ID, Animation 참조, Skill 참조, slotKey, 파생 Job 요소, Runtime 상태는 작성 Character 본문에서 숨긴다.
  - 원본 Stat마다 Entry 하나와 값 하나를 보존한다. Crit 값은 Percent, MoveSpeed는 m/s, AttackSpeed는 원본 숫자를 바꾸지 않는 Localization 배율 포맷을 사용한다.
- 구현:
  - Character 전용 `UIContentInfoView`와 선택적인 Skill 탭 동기화를 위한 `CharacterContentInfoPresenter`를 추가했다.
  - 필터링된 `CharacterPresentationResolver.ResolveForPlayerDisplay` 경로와 Character 표시 Catalog 항목을 추가했다.
  - Original JSON, 전체 SO 검사, 필터링된 플레이어 출력을 나란히 보여주고 JSON/SO/이름 불일치를 보고하는 `CharacterPresentationPreviewWindow`를 추가했다.
  - 현행 Character 이름 22개를 기존 `character_string.csv` 경로에 추가하고 Character 라벨/포맷을 `presentation_string.csv`에 추가했다.
- 검증:
  - JSON/SO/이름 비교: 22개, 불일치 0개.
  - `presentation_string.csv`: 데이터 308행, 복합 Key 중복 0개, 필요한 Character Key마다 Row 하나.
  - 신규 Presenter 포함 Runtime Assembly: 오류 0개, 기존 경고 35개.
  - 최종 비교 도구 포함 Editor Assembly: 오류 0개, 전체 경고 197개. 신규 도구의 JsonUtility DTO에 대한 예상된 `CS0649` 경고를 포함한다.
  - Unity Editor는 조작하지 않았다. 프리팹 연결, AutoBind, 스크롤, Play Mode 화면 출력은 대기한다.

## 2026-08-13 — 문서 관리자 Character Skill 탭 정합성 확인

- 상태: 영문/한국어 가이드와 README/로그 연결 확인 완료, 사용자 Unity 연결 및 검증 대기.
- 정합성 확인:
  - Character 소유 탭 조합이 선택 Skill을 `SkillContentInfoPresenter`로 전달하고 `UIContentInfoView`를 콘텐츠 중립 상태로 유지하는 것을 확인했다.
  - 현재 `UISkillIconSlot.prefab`에 `UISkillIconSlot`과 `Bind_SkillIconImage`는 있지만 `Button`과 `SkillContentInfoTabButton`은 여전히 없음을 확인했다.
  - 현재 `Assembly-CSharp.csproj`에 두 신규 소스 파일이 모두 포함된 상태에 맞춰 검증 문구를 정정했다.
- 검증:
  - 현재 `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal` 시도도 `C:\Users\machal89\AppData\Local\Microsoft SDKs` 접근 거부로 C# 컴파일 전에 중단됐다.
  - 영문/한국어 가이드 구조, UTF-8, 공백 검사를 통과했다.
  - 문서 관리자는 Runtime 소스, 프리팹, Scene, Unity Editor, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-13 — 문서 관리자 Character Presentation 정합성 확인

- 상태: Character 원본/표시 경계와 영문/한국어 연결 확인 완료, 사용자 Unity 연결 및 Play Mode 검증 대기.
- 정합성 확인:
  - Character 가이드가 작성 JSON 필드 5개, 정렬된 원본 Stat 7개, 전체 SO 검사, 필터링된 플레이어 출력, 선택적인 Skill 탭 동기화를 보존하는지 확인했다.
  - 표시 Catalog 상단 범위에 기존 Character 절을 포함하도록 교정했다.
  - 정정된 빌드 표현을 유지했다. Runtime은 오류 0개와 기존 경고 35개, Editor는 오류 0개와 예상된 JsonUtility DTO `CS0649`를 포함한 전체 경고 197개다.
- 검증:
  - Strict UTF-8 JSON 22개 모두 정확히 `characterId`, `name`, `characterType`, `job`, `baseStats`를 가지며 정렬된 Stat Type 7개가 같다.
  - 폴더에 생성 CharacterSO 에셋 22개가 있고 Character Localization은 JSON마다 대응하는 한국어 이름 Row 하나를 가진다. `presentation_string.csv`는 데이터 308행, 복합 Key 중복 0개다.
  - 영문/한국어 Character 가이드와 Catalog의 Heading 구조가 일치하고 UTF-8 및 공백 검사를 통과했다.
  - 문서 관리자는 Runtime 소스, Localization CSV, 프리팹, Scene, Unity Editor, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-13 — Bless와 Relic 콘텐츠 Presenter

- 상태: 소스 구현과 정적 컴파일 완료, 사용자 Unity 연결 및 Play Mode 검증 대기.
- 구현:
  - 기존 Shrine UI Domain에 `BlessContentInfoPresenter`, 기존 Relic UI Domain에 `RelicContentInfoPresenter`를 추가했다.
  - 두 Presenter 모두 각 Domain Resolver의 `ResolveForPlayerDisplay` 경로를 사용해 정의 Preview 데이터를 지정된 기존 `UIContentInfoView`에 연결한다.
  - 두 Presenter 모두 Runtime Entry Overload, `Set`, `Show`, `Clear`, 선택적 Start 자동 빌드, 컴포넌트 Context Menu 빌드, EventSystem 누락 경고를 제공한다.
  - 공통 `UIContentInfoView`는 콘텐츠 중립 상태를 유지하며 Canvas, 프리팹 Instance, Scene Object, SO 에셋, 레거시 데이터는 생성하거나 수정하지 않았다.
- 변경 소스 경로:
  - `Assets/Scripts/Stage/NodeContents/Shrine/UI/BlessContentInfoPresenter.cs` 및 `.meta`
  - `Assets/Scripts/Collection/Relic/UI/RelicContentInfoPresenter.cs` 및 `.meta`
- 검증:
  - `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 오류 0개, 기존 프로젝트 경고 35개.
  - 두 소스 파일 모두 컴파일된 Runtime Assembly 프로젝트에 포함되었다.
  - 범위 한정 `git diff --check`와 Placeholder 검사를 통과했다.
  - Agent는 Unity Editor를 열거나 조작하지 않았다.
- 사용자 대기 작업:
  - 각 Presenter를 부착하고 `BlessContentInfoView` / `RelicContentInfoView`를 지정 또는 AutoBind한 뒤 현행 SO를 지정하고 Play Mode에서 대응 컴포넌트 Context Menu를 실행한다.

## 2026-08-13 — 문서 관리자 Bless/Relic Presenter 정합성 확인

- 상태: 영문/한국어 소유권 및 인계 지침 정합성 확인 완료, 사용자 Unity Import, 연결, Play Mode 검증 대기.
- 정합성 확인:
  - 8단계 Task 상태와 작업 순서를 Skill Presenter 전용 표현에서 구현된 Character/Skill/Bless/Relic Presenter 전체로 교정했다.
  - 별도 가이드를 추가하지 않고 5-8단계 완료 문서에 Bless/Relic 정의 Preview와 Runtime Entry 연결 흐름을 기록했다.
  - 콘텐츠 중립 `UIContentInfoView` 소유권과 사용자 담당 Unity 조작 경계를 유지했다.
- 검증:
  - 두 Presenter 소스가 문서화한 `Set`, `Show`, `Clear`, 선택적 Start 자동 빌드, Context Menu, Runtime Entry, EventSystem 경고 동작을 제공함을 확인했다.
  - 보고된 정적 빌드 결과인 오류 0개와 기존 경고 35개를 보존했다. 문서 관리자는 빌드를 다시 실행하거나 Unity를 조작하지 않았다.
  - 영문/한국어 동등성, Strict UTF-8, 공백, 범위 내 Diff, 두 고유 `.meta` GUID를 검사했다.
  - 문서 관리자는 Runtime 소스, 프리팹, Scene, 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-13 패널 캐릭터 이전/다음 탐색

- 상태: 소스와 프리팹 소유권 준비 완료, 사용자 Unity 버튼 작업과 Play Mode 검증 대기.
- 사용자 요청:
  - `Panel_CharacterInfo`에서 `CharacterSkillContentInfoPresenter`가 CharacterSO 목록을 소유하고 이전/다음 동작으로 표시 캐릭터를 바꾼다.
  - Button 오브젝트 생성과 이벤트 연결은 사용자가 수행하며, 에이전트는 기능만 구현한다.
- 구현:
  - 직렬화 Character 목록, 초기 Character 인덱스, 선택적 순환 탐색, 현재 인덱스/개수 상태, 런타임 목록 교체, 직접 선택, 공개 이전/다음 메서드를 추가했다.
  - 캐릭터 변경 시 스킬 탭을 다시 만들고 기존 캐릭터 본문 Presenter를 동기화한다.
  - 기존 단일 Character를 한 항목 fallback으로 유지하고 `FormerlySerializedAs`로 기존 초기 Skill 인덱스를 보존했다.
  - Button 상태를 소유하지 않고 null 목록 항목 처리와 `CanShowPreviousCharacter`, `CanShowNextCharacter` 이동 가능 속성을 추가했다.
  - `CharacterContentInfoPresenter.ClearPresentation()`을 추가하고 두 Presenter가 이미 같은 Character를 가리킬 때 중복 Skill 탭 동기화를 막았다.
- 프리팹 준비:
  - 기존 Character Presenter를 `CharacterSkillContentInfoPresenter`에 할당했다.
  - Character 목록 소유자가 시작을 제어하도록 이 프리팹의 독립적인 `CharacterContentInfoPresenter.buildOnStart`를 껐다.
  - 이전/다음 Button 참조는 추가하지 않았고, 신규 Character 목록은 사용자 입력을 위해 비워 뒀다. 기존 단일 Character가 fallback으로 남는다.
- 변경 경로:
  - `Assets/Scripts/Actor/Character/ui/CharacterSkillContentInfoPresenter.cs`
  - `Assets/Scripts/Actor/Character/ui/CharacterContentInfoPresenter.cs`
  - `Assets/Prefabs/UI/Fixed/Panel/Panel_CharacterInfo.prefab`
  - `AgentDocs/Machal/character-skill-content-tabs.md`와 한국어 문서
  - 영문/한국어 Active Task와 Task Log
- 검증:
  - `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 오류 0개, 기존 경고 35개.
  - Unity Editor를 열거나 조작하지 않았다.
- 사용자 후속 작업:
  - `characters`를 채우고 이전/다음 버튼을 만든 뒤 `ShowPreviousCharacter()`와 `ShowNextCharacter()`에 연결한다. 프리팹 저장 후 Play Mode에서 캐릭터 본문/스킬 탭 동기화를 검증한다.

## 2026-08-14 — 현행 Relic JSON 및 생성 에셋 경로

- 상태: 원본 JSON 경로와 Unity Builder 준비 완료, SO 에셋 생성 및 검증은 사용자 Unity 실행 대기.
- 사용자 결정:
  - 현행 Relic JSON과 생성되는 `RelicSO` 에셋을 `Assets/Resources/relic/json/` 아래에 저장한다.
  - 제외된 `Assets/Resources/shop/relic/` 레거시 에셋은 변경하지 않는다.
- 구현:
  - 기존 `Assets/Resources/item/json/` 파일은 수정하지 않고 정규화 Relic JSON 10개를 승인된 현행 Relic 경로에 복사했다.
  - 운영 에셋 마이그레이션 메뉴를 `Tools > ProjectBS > Items > Build Current Relics From JSON`으로 교체했다.
  - Builder는 JSON과 같은 폴더에 `RelicSO` 10개, `EffectSO` 12개, `EffectEntrySO` 12개를 생성 또는 갱신하고 모든 참조가 승인 경로 안에 있는지 검증한다.
  - 별도 사용자 검증용 `Tools > ProjectBS > Items > Validate Current Relics` 메뉴를 추가했다.
- 변경 경로:
  - `Assets/Editor/tools/item/RelicItemAssetBuilder.cs`
  - `Assets/Resources/relic/`
  - 영문/한국어 Active Task와 Task Log.
- 검증:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q`: 오류 0개, 기존 경고 162개.
  - 신규 JSON 10개가 UTF-8 JSON으로 파싱되고 줄바꿈 정규화 후 원본 JSON 10개와 일치한다.
  - 신규 `.meta` GUID가 모두 고유하고 `git diff --check`를 통과했으며 `Assets/Resources/shop/relic/`에는 범위 내 변경이 없다.
- 대기 검증:
  - 사용자 Unity Builder 실행, 에셋 Import, 검증 메뉴 및 Relic UI 출력 확인.
- 수행하지 않은 작업:
  - Unity Editor 조작, 레거시 에셋 수정, 프리팹 변경, Staging, Commit, Push를 수행하지 않았다.

## 2026-08-14 — 문서 관리자 현행 Relic 경로 정합성 확인

- 상태: Machal 및 범용 Relic Workflow 가이드의 현행 Relic 작성/생성 에셋 경로 정합성 확인 완료, 사용자 Unity 생성 및 검증 대기.
- 정합성 확인:
  - Relic JSON 생성 프롬프트와 JSON 가이드가 현행 JSON을 `Assets/Resources/relic/json/` 아래에만 작성하도록 교정했다.
  - RelicSO 가이드를 실제 현행 코드 경로로 갱신하고 Builder 및 검증 메뉴 흐름을 기록했다.
  - `Assets/Resources/item/json/`은 원본 근거를 보존하고 `Assets/Resources/shop/relic/`은 변경하지 않는 레거시 비교 데이터로 유지함을 명확히 했다.
  - 현행 JSON과 Builder가 생성하는 `RelicSO`, `EffectSO`, `EffectEntrySO`가 승인 루트를 공유하지만 JSON 작성과 Unity 에셋 생성은 별도 작업임을 명확히 했다.
- 검증:
  - 승인 루트의 현행 JSON 10개와 같은 이름을 가진 보존 원본 JSON 10개를 확인했다.
  - 소스에서 Builder 메뉴명, 승인 루트 검사, 예상 출력 RelicSO 10개 / EffectSO 12개 / EffectEntrySO 12개, 레거시 제외를 확인했다.
  - Strict UTF-8, 공백, 오래된 현행 출력 경로, Heading 순서, 범위 내 Diff 검사를 통과했다.
  - 문서 관리자는 Builder 실행, Unity 조작, Runtime 소스, JSON, 생성 에셋, 레거시 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-14 — BlessSO 목록 및 선택 Presenter

- 상태: Runtime 소스 구현과 정적 컴파일 완료, 사용자 Unity 연결 및 Play Mode 검증 대기.
- `BlessContentInfoPresenter`를 단일 직렬화 Bless에서 인스펙터의 `List<BlessSO>`와 초기 선택 인덱스를 소유하는 구조로 확장했다.
- Presenter는 null 목록 항목을 건너뛰고 범용 `UISelectableIconButton` 탭을 생성하며, 각 Bless 아이콘과 선택 상태를 갱신하고 선택된 Bless를 기존 플레이어 표시 Resolver와 Content View에 연결한다.
- `SetBlesses`, 인덱스 기반 `SelectBless`, 탭 정리, 선택/개수 속성을 추가하고 기존 단일 Bless API는 유지했다.
- Bless 전용 버튼 컴포넌트를 추가하지 않고 기존 범용 아이콘 버튼을 재사용했다.
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 오류 0개, 기존 경고 35개.
- 프리팹, Scene, SO 에셋, 레거시 데이터, Unity Editor 상태, Staging, Commit, Push는 변경하지 않았다.

## 2026-08-14 — 신앙 페이지 및 세 축복 구조 정정

- 선택된 신 하나가 해당 신의 전체 축복 정보 페이지를 소유하며, 선택된 캐릭터 하나가 자신의 스킬 탭과 상세 페이지를 소유하는 것과 같은 구조라는 사용자 결정을 기록했다.
- 기존 Enforce 단계 해석을 폐기했다. 각 신은 신앙 레벨에 따라 강화되는 기본축복 하나, 신앙 고정 시 지급되는 전용축복 1, 신앙 고정 후 신앙 레벨 8 달성 시 지급되는 전용축복 2라는 서로 다른 세 축복 단위를 가진다.
- 기본축복의 신앙 레벨별 강화 상태는 서로 무관한 여러 축복 탭으로 만들지 않는다. 전용축복 2의 획득 조건은 신앙 고정 후 신앙 레벨 8 달성으로 확정했다.
- 신앙 페이지는 계속 후순위로 유지하며 Runtime이나 UI 구현을 변경하지 않았다.
- 소스 확인 결과 현행 `Base/Enhanced` 그룹 Enum과 `progressionStep` 필드는 정정된 세 축복 계약을 명시적으로 표현하지 못하며, `ShrineGodSO.GetAvailableBlessings`도 Group 인자를 사용하지 않는다. 향후 신앙 페이지 Adapter 구현 전에 이 불일치를 해결해야 한다.
- 후순위 프리팹 준비 계약을 기록했다. 신앙 합성 패널 하나, 재사용 신 탭 하나, 선택/잠금 표시가 있는 재사용 축복 탭 하나, 기존 공용 축복 Content View 하나를 사용하며 축복 종류별 상세 프리팹은 만들지 않는다.

## 2026-08-14 — 문서 관리자 Bless 목록 Presenter 정합성 확인

- 상태: 영문/한국어 Ability Presentation 지침 정합성 확인 완료, 사용자 Unity 연결 및 Play Mode 검증 대기.
- 정합성 확인:
  - 기존 5-8단계 완료 가이드에 Bless Presenter의 목록, 범용 탭, 선택 소유권 계약을 추가했다.
  - 콘텐츠 중립 `UIContentInfoView`, 기존 단일 Bless API 호환, 보류된 Faith/Bless 페이지 아키텍처를 유지했다.
  - 별도 가이드를 만들지 않고 정확한 사용자 연결 체크리스트를 기록했다.
- 검증:
  - 소스가 null 항목을 건너뛰고 `UISelectableIconButton`을 생성하며 선택 상태를 소유하고 `ResolveForPlayerDisplay`를 사용하면서 `SetBless`, `ShowBless`, `BuildPresentation`, `Bless`를 유지함을 확인했다.
  - 보고된 빌드 결과인 오류 0개와 기존 경고 35개를 보존했다. 문서 관리자는 빌드를 다시 실행하거나 Unity를 조작하지 않았다.
  - 영문/한국어 Heading 동등성, Strict UTF-8, 공백, 범위 내 Diff 검사를 통과했다.
  - 문서 관리자는 Runtime 소스, 프리팹, Scene, 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-14 — 문서 관리자 세 Bless 모델 정합성 확인

- 상태: 보류된 Faith 페이지 소유권과 정정된 세 Bless 계약 정합성 확인 완료, 권위 있는 원본 인코딩이 없어 구현은 계속 차단 상태.
- 정합성 확인:
  - Faith 페이지 문서나 구현을 만들지 않고 결정된 신 페이지 소유 흐름을 기존 5-8단계 완료 가이드에 추가했다.
  - Faith 레벨에 따라 강해지는 Basic Bless 하나, Faith Lock 시 획득하는 Exclusive Bless 1, Faith Lock 이후 Faith 레벨 8에 획득하는 Exclusive Bless 2를 서로 다른 Bless 단위 세 개로 기록했다.
  - Basic Bless 강도 레벨별 탭 생성과 확정된 Exclusive Bless 2 규칙을 `successorFaithLevel` 또는 소스 이름에서 추론하는 것을 명시적으로 금지했다.
  - 선택된 신의 Bless 탭/상세는 `BlessContentInfoPresenter`가 소유하고 공통 View는 콘텐츠 중립으로 유지한다.
- 검증:
  - 현행 소스에는 `ShrineBlessingGroup.Base/Enhanced`와 `BlessPoolEntry.ProgressionStep`만 노출됨을 확인했다.
  - `ShrineGodSO.GetAvailableBlessings`가 Group 인자를 받지만 사용하지 않으며 권위 있는 세 단위 원본 인코딩만 미해결임을 확인했다.
  - 영문/한국어 Heading 동등성, Strict UTF-8, 공백, 폐기된 해석 잔여물, 범위 내 Diff 검사를 통과했다.
  - 문서 관리자는 코드, 빌드, Unity, 프리팹, Scene, 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-14 - 문서 관리자 보류된 Faith 프리팹 정합성 확인

- 상태: 최소 사용자 준비 계약을 기존 문서에 반영했으며 Faith Runtime 통합은 계속 보류한다.
- 정합성 반영:
  - 별도 Faith 가이드를 만들지 않고 기존 5-8단계 완료 가이드에 준비 계약을 추가했다.
  - `Panel_FaithInfo` 하나, 재사용 신 아이콘 탭 하나, 재사용 Bless 아이콘 탭 하나, 기존 `Assets/Prefabs/UI/Fixed/Content/UIContentInfoView_Bless.prefab` 재사용을 기록했다.
  - 현재의 정확한 AutoBind 이름 `BlessContentInfoTabRoot`와 `BlessContentInfoView`를 유지했다. 향후 신 Header, Faith 진행, 신 탭 Root 이름은 미정으로 남긴다.
  - Basic, Exclusive Bless 1, Exclusive Bless 2가 아이콘, 선택 Frame, 잠금 Overlay를 갖춘 같은 Bless 탭/상세 구조를 사용하도록 기록했다. Common Bless는 선택된 신 소유 범위 밖에 둔다.
- 검증:
  - `BlessContentInfoPresenter`가 `BlessContentInfoTabRoot`와 `BlessContentInfoView`를 연결하고 `UISelectableIconButton`을 재사용하며 현재 모든 탭에 `SetLocked(false)`를 호출함을 확인했다.
  - `UISelectableIconButton`에 아이콘, 선택 Frame, 잠금 Overlay AutoBind 필드가 있음을 확인했다.
  - 문서 관리자는 코드, 빌드, Unity, 프리팹, Scene, 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-17 — 유물, 일반 축복, 신앙 표시 소유권 정정

- 사용자는 모든 보유 유물과 획득한 일반 축복이 장착 상태 없이 즉시 적용되고, 신앙만 잠금 해제/레벨 진행 시스템임을 확정했다.
- 신앙 모델을 축복 세 개에서 서로 다른 네 기능으로 정정했다. 네 기능은 신앙 레벨에 따라 강화되는 기본축복, 직업군 전용전직, 전용축복 1, 전용축복 2다.
- 별도 최상위 보유 축복 페이지 대신 `전체`, `유물`, `일반 축복` 보기를 가진 `보유 효과` 페이지를 권장했다. 전체 신앙 페이지는 별도로 유지하고 `전체`에는 현재 적용되는 신앙 결과 요약과 이동 링크만 둘 수 있다.
- 전용전직은 `BlessSO` 데이터가 아니므로 `BlessContentInfoPresenter`에 강제로 넣지 않고 신앙 소유 Adapter가 필요함을 기록했다.
- 구현 전 정리할 오래된 Runtime 개념으로 `RelicItemService.EquippedRelics`와 `BlessManager.AddBless`의 영구 Common 축복 교체 동작을 확인했다.
- Runtime 소스, 프리팹, Scene, 에셋, 빌드, Staging, Commit, Push 작업은 수행하지 않았다.

## 2026-08-17 — 보유 효과 인벤토리 골격

- 사용자는 현재 화면을 `전체`, `유물`, `일반 축복`, `신앙 축복` 탭과 우측 상세 패널 하나를 가진 효과 인벤토리로 확정했다.
- 신앙 페이지는 진행 기능을 보여주는 별도 도감으로 유지하고, 향후 유물 도감도 획득/미획득 유물을 보여주는 별도 페이지로 구성한다.
- 잠금 실루엣과 보유/전체 개수를 이미 소유하는 `RelicCollectionView`는 미래 유물 도감용으로 변경하지 않고 보존했다.
- `Assets/Scripts/Presentation/SharedUI/Content/` 아래에 `OwnedEffectInventoryData`, `OwnedEffectGridItemView`, `OwnedEffectInventoryView`, `OwnedEffectInventoryPresenter`를 추가했다.
- 새 Presenter는 인스펙터 Preview 원본과 보유 유물, 활성 일반 축복 Entry, 활성 신앙 축복 Entry를 명시적으로 받는 Runtime API를 지원한다. View는 네 탭 Button용 공개 메서드를 제공하고 선택 항목을 항상 활성 상태인 중립 Content View 하나에 연결한다.
- 정적 검증에만 임시 Compile 항목을 추가한 뒤 원상 복구했다. `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 오류 0개, 기존 경고 35개.
- 기존 유물 소스, 프리팹, Scene, Unity 오브젝트, 에셋, Staging, Commit, Push는 변경하지 않았다. Unity Import, 컴포넌트 교체, Grid 항목 프리팹 설정, Button 연결, Play Mode 검증은 사용자 작업으로 남긴다.

## 2026-08-17 - 신앙 페이지 상세 재설계

- 신앙 페이지를 축복 전용 상세 페이지가 아니라 선택한 신앙의 현재 기능과 향후 해금을 함께 설명하는 성장 페이지로 다시 정의했다.
- 획득한 신앙마다 탭 하나를 유지하고, 선택된 신앙 페이지 안에 신 요약, 레벨 1-10 로드맵, 재사용 가능한 기능 카드 4개, 콘텐츠 중립 상세 View 하나를 두도록 설계했다.
- 서로 다른 네 신앙 기능을 신앙 레벨에 따라 강화되는 기본축복, 직업군별 전용전직, 전용축복 1, 전용축복 2로 정의했다.
- 기본축복 강화는 명시적인 레벨별 작성 데이터 또는 실제 Runtime 결과만 사용하며 Presentation에서 값을 보간, 결합, 창작하지 않도록 기록했다.
- 잠긴 기능도 정보를 미리 볼 수 있도록 선택 가능하게 유지하되, 잠금 상태와 작성된 정확한 해금 조건을 분명하게 표시하도록 정했다.
- 페이지 소유자는 제안된 `FaithPagePresenter`, 의미와 해금 판정 소유자는 `ShrineFaithPresentationResolver`로 두었다. `BlessContentInfoPresenter`는 독립적인 축복 목록/상세 소유자로 유지한다.
- 필요한 프리팹을 페이지 패널, 재사용 신앙 탭, 재사용 레벨 노드, 재사용 기능 카드, 선택 사항인 중립 `UIContentInfoView` 레이아웃 Variant의 다섯 종류로 기록했다.
- 전용전직의 해금 조건과 직업군별 정확한 목표 직업 매핑은 권위 있는 작성 데이터가 필요하므로 추론하지 않고 미확정 상태로 유지했다.
- 최신 설계 계약으로 `AgentDocs/Machal/faith-page-design-ko.md`와 영문 Mirror를 추가했다. Runtime 소스, 프리팹, Scene, 에셋, 빌드, Staging, Commit, Push 작업은 수행하지 않았다.

## 2026-08-17 - 문서 관리자 소유권 및 Faith 재설계 정합성 확인

- 상태: 활성 영문/한국어 Ability Presentation 지침을 Relic/일반 Bless 무장착 규칙과 Faith 네 기능 페이지 설계에 맞췄다. Runtime 및 Unity 작업은 계속 대기한다.
- 정합성 반영:
  - 활성 5-8단계 문서의 세 Bless 및 Bless 탭 전용 Faith 표현을 선택된 신 성장 페이지와 `FaithPagePresenter` / `ShrineFaithPresentationResolver` 소유권으로 교체했다.
  - Basic Bless, 직업군 Exclusive Job Change, Exclusive Bless 1, Exclusive Bless 2를 서로 다른 Faith 기능 네 개로 기록했다. Exclusive Job Change는 `BlessSO`가 아니다.
  - 획득한 모든 일반 Bless와 보유한 모든 Relic은 즉시 적용되며 플레이어 UI에서 장착 상태를 표시하거나 장착 상태로 필터링하면 안 된다고 기록했다.
  - 전체 Faith 페이지를 분리하고 진행 상세를 중복하지 않으면서 `All`, `Relic`, `General Bless` View를 가진 `Owned Effects` Navigation 권장안을 추가했다.
  - 현행 `EquippedRelics` 및 Bless 장착형 필드를 플레이어 표시 규칙으로 만들지 않고 소스 불일치로 진단 가능하도록 표시 카탈로그를 갱신했다.
  - 이전 날짜의 세 Bless 항목은 2026-08-17 정정으로 대체된 역사 기록으로 보존했다.
- 검증:
  - `RelicItemService`에 `OwnedRelics`와 `EquippedRelics`가 함께 남아 있고 `BlessManager.AddBless`가 새 Bless 추가 전 기존 영구 Common Bless를 제거함을 확인했다.
  - 영문/한국어 Faith 설계가 상세 프리팹, 소유권, 상호작용, 구현 순서, 검증의 권위 문서로 유지됨을 확인했다.
  - 문서 관리자는 코드, 빌드, Unity, 프리팹, Scene, 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-17 - 문서 관리자 Owned Effect 인벤토리 정합성 확인

- 상태: 확정된 4탭 인벤토리와 분리된 Faith/Relic 도감 역할을 활성 영문/한국어 지침에 반영했다. 사용자 Unity 연결과 Runtime 원본 통합은 대기한다.
- 정합성 반영:
  - 이전 `Owned Effects` 3탭 권장안을 정확히 `All`, `Relic`, `General Bless`, `Faith Bless` 네 탭으로 대체했다.
  - `All`은 현재 적용 중인 모든 항목을 포함하고 `Faith Bless`는 활성 Bless 기반 Faith 기능만 포함한다고 기록했다. 전체 Faith 진행과 Exclusive Job Change는 향후 명시적인 Effect 원본이 작성되지 않는 한 Faith 도감에 유지한다.
  - 모든 인벤토리 선택의 상세 대상은 우측 중립 `UIContentInfoView` 하나이며 별도 Owned Bless 페이지를 만들지 않는 결정을 유지했다.
  - 구현된 `OwnedEffectInventoryData`, `OwnedEffectGridItemView`, `OwnedEffectInventoryView`, `OwnedEffectInventoryPresenter` 스캐폴드와 설정 Preview / 명시적 Runtime 목록 경계를 완료 지침에 추가했다.
  - `RelicCollectionView`는 획득/미획득 항목, 잠금 실루엣, 보유/전체 개수를 가진 향후 Relic 도감 전용으로 보존하고 Owned Effect 인벤토리로 일반화하지 않도록 했다.
  - 역사 항목을 다시 쓰지 않고 Faith 가이드와 표시 카탈로그를 확정된 페이지 경계에 맞췄다.
- 검증:
  - View가 네 탭 메서드를 모두 공개하고 선택 항목을 중립 상세 View 하나에 연결함을 확인했다.
  - Presenter가 설정된 Relic/일반 Bless/Faith Bless 정의와 명시적인 Runtime 목록을 받으며 Manager 자동 수집은 구현되지 않았음을 확인했다.
  - 보고된 정적 빌드 결과인 오류 0개와 기존 경고 35개를 보존했다. 문서 관리자는 빌드를 다시 실행하거나 Unity를 조작하지 않았다.
  - 문서 관리자는 Runtime 소스, 프리팹, Scene, 에셋, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-17 - Content Inventory 리팩터링 1단계

- 사용자의 참고 레이아웃에 따라 프리팹 우선 진행을 카테고리 섹션 리팩터링으로 교체했다. 세로 페이지 Scroll 하나 안에 카테고리 Header와 Item Grid를 동적으로 생성한다.
- 기존 `OwnedEffect` 골격을 수정하거나 제거하지 않고 `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryData.cs`와 `.meta`를 추가했다.
- 중립 페이지/카테고리/아이템 Snapshot 계약, `OwnedOnly`/`Catalog` 표시 모드, `Owned`/`Unowned`/`Locked` 획득 상태를 추가했다.
- 공통 계약에서 구체적인 Relic, 일반 Bless, Faith Bless 카테고리 지식을 제외했다. 후속 단계에서 도메인 Presenter가 내부 카테고리 ID와 StringManager 타이틀 Key를 제공한다.
- 정적 검증: 임시 Compile 포함 후 `dotnet build ProjectBS.sln --no-restore -v:minimal`은 오류 0개, 기존 경고 197개로 통과했다. 이후 `Assembly-CSharp.csproj`를 복구했다.
- 기존 View, Presenter, 프리팹, Scene, Gameplay 원본, 에셋, Staging, Commit, Push는 변경하지 않았다. 2단계 공통 Item/Category View와 Unity 검증은 대기한다.

## 2026-08-17 - Content Inventory 탭 의미 정정

- 예정된 `전체` 탭을 `보유`로 교체하여 탭을 정확히 `보유`, `유물`, `일반 축복`, `신앙 축복`으로 확정했다.
- `보유`는 콘텐츠 카테고리가 아니라 집계 페이지 모드다. 하나의 세로 Scroll에 사용 가능한 모든 보유 카테고리 섹션을 생성하며 항상 `OwnedOnly`이므로 `Catalog`를 선택할 수 없다.
- `유물`과 `일반 축복`은 단일 카테고리 탭이며 각각 독립적으로 `OwnedOnly` 또는 `Catalog`를 사용할 수 있다.
- `신앙 축복`은 활성 보유 항목 탭으로 유지한다. 전체 Faith 진행, 비활성 기능, 향후 해금은 별도 신앙 도감에 유지한다.
- 이번 정정은 후속 Page/View 조정 계약만 변경한다. C# 원본, 프리팹, Scene, 에셋, 빌드, Staging, Commit, Push 작업은 수행하지 않았다.

## 2026-08-18 - 탭 없는 보유 효과 페이지 정정

- 보유 효과 페이지에서 모든 탭을 제거했다. 이제 보유 유물, 획득한 일반 축복, 활성 신앙 축복만 카테고리 섹션으로 구성하는 하나의 세로 Scroll 인벤토리다.
- 보유 효과 페이지에서 `Catalog` 선택을 제거했다. 공통 `Catalog` 데이터 모드는 별도 유물/일반 축복 도감 페이지에서 계속 재사용할 수 있다.
- 전체 Faith 진행, 비활성 기능, 향후 해금은 별도 신앙 도감에 유지한다.
- 기존 네 탭 `OwnedEffectInventoryView` 골격은 명시적으로 대체 예정이며 공통 Item/Category View 구현 후 단계적으로 교체한다.
- 이번 정정 단위에서는 C# 원본, 프리팹, Scene, 에셋, 빌드, Staging, Commit, Push 작업을 수행하지 않았다.

## 2026-08-18 - 보유 효과 새 채팅 작업 시작 계약

- 탭 없는 보유 효과 인벤토리를 새 채팅에서 이어가기 위한 작업 전용 영문 원본/한국어 Mirror 시작 계약을 추가했다.
- 시작 계약에는 복사해서 사용할 요청문, 정확한 필수 읽기 순서, 확정 설계, 구현/대체 예정/미구현 경로, 다음 단일 2단계 작업, 작업 규칙, Unity 중단 경계, 작업 단위 종료 보고 형식을 포함한다.
- 일반적인 한 줄 Handoff에 의존하지 않도록 `AgentDocs/Machal/README.md`에서 작업 전용 계약을 필수 활성 진입점으로 연결했다.
- 활성 Task 인계 상태도 구현 전에 새 시작 계약을 필수로 읽도록 갱신했다.
- C# 원본, 프리팹, Scene, Gameplay 에셋, 빌드, Staging, Commit, Push 작업은 수행하지 않았다.

## 2026-08-18 - 일반 축복 도감 및 Content Inventory 2단계

- 사용자는 별도 일반 축복 도감을 첫 Catalog 통합 대상으로 정했다. 전체 `BlessSO` 목록 또는 `BlessPoolSO`를 표시하고 Runtime 활성 정의만 활성 표시하며 탭 없는 보유 효과 페이지와는 별개다.
- 현행 `BlessPoolSO.BlessPoolEntry`가 `Blessing`, 생성용 `Weight`, `ProgressionStep`을 노출하며 인벤토리 콘텐츠는 Bless 정의뿐임을 확인했다. `BlessManager.Blessings`가 작성된 `BlessingId` 대조에 사용할 수 있는 활성 `BlessRuntimeData.BlessEntry.source` 정의를 노출함을 확인했다.
- 활성 상태를 선택, 획득, 잠금 상태와 결합하지 않고 `ContentInventoryItemData`에 `ContentActivationState.Inactive/Active`를 추가했다.
- Unity `.meta`와 함께 `ContentInventoryItemView`, `ContentInventoryCategoryView`를 추가했다. Item은 별도 `UI_ActiveIndicatorImage`를 노출하고 Category는 Localization 제목/개수와 동적 Grid를 소유하지만 중첩 `ScrollRect`는 사용하지 않는다.
- 임시 Compile 포함 후 깨끗한 정적 검증: `dotnet build ProjectBS.sln --no-restore -v:minimal`은 오류 0개, 기존 경고 197개로 통과했다. 첫 검증의 임시 중복 소스 경고 하나는 중복을 제거한 뒤 재실행했으며 `Assembly-CSharp.csproj`를 복구했다.
- 기존 Presenter, 프리팹, Scene, Gameplay 원본, 에셋, Staging, Commit, Push는 변경하지 않았다. 일반 축복 Presenter/Detail 통합과 Unity 검증은 대기한다.
## 2026-08-18 - 보유 효과 프리팹 직접 연결

- 상태: 탭 없는 보유 효과 코드와 프리팹 컴포넌트 그래프의 정적 작업은 완료했으며, Unity Import와 Play Mode 검증은 사용자가 수행한다.
- 사용자 승인: 사용자가 필요한 컴포넌트 연결을 에이전트가 직접 수행하도록 명시적으로 요청했다. 이 1회 요청은 이번 작업의 세 프리팹에만 기존 중단 경계를 대체하며, 이후 Unity 조작이나 무관한 프리팹 YAML 수정 권한으로 확대하지 않는다.
- `OwnedEffectInventoryView.cs`와 `OwnedEffectInventoryPresenter.cs`의 구형 4탭 구현을 하나의 세로 스크롤과 동적 카테고리 방식으로 교체했다.
- Presenter는 설정 목록 또는 명시적 런타임 목록으로 보유 유물, 일반 축복, 활성 신앙 축복의 `OwnedOnly` 카테고리를 순서대로 만든다. Manager 자동 수집은 아직 구현하지 않았다.
- `ContentInventoryCategoryView`의 AutoBind 이름을 기존 프리팹 하이어라키와 일치시켰고, 런타임 생성 전에 모든 디자인 샘플 자식을 비활성화 후 제거하도록 했다.
- `UIInventoryItemView.prefab`에 `ContentInventoryItemView`, `UIContentInventoryCategory.prefab`에 `ContentInventoryCategoryView`를 붙이고 필수 참조를 연결했다.
- `Panel_OwnedEffects.prefab` 루트에 보유 효과 View/Presenter를 붙여 제목, 스크롤, 카테고리 루트/프리팹, 공통 상세 View를 연결했다. 패널 내부의 잘못된 전용 `RelicContentInfoPresenter`만 제거하고 공통 상세 오브젝트는 활성화했다.
- `presentation_string.csv`에 보유 효과 페이지와 유물/일반 축복/신앙 축복 카테고리 제목 키를 추가했다.
- 검증: `dotnet build ProjectBS.sln --no-restore -v:minimal` 결과 오류 0개, 기존 경고 197개. 네 스크립트 연결, 필수 참조, 구형 Presenter 제거, 상세 View 활성화, 로컬라이징 키 중복 없음 검사를 통과했다.
- 미검증: Unity Import/Console, Inspector 역직렬화, SO 목록 할당, 카테고리 생성, 아이콘 선택, 상세 표시, 외부 ScrollRect 입력, 로컬라이징, 시각 레이아웃.
- 레거시 Effect/Bless/Relic 에셋, 게임플레이 보유 동작, Scene, Git stage/commit/push/reset/clean은 변경하지 않았다.

## 2026-08-18 - 문서 관리자 직접 연결 정합성 확인

- 과거 네 탭 항목은 대체된 날짜 이력으로 보존하면서 활성 영문/한국어 가이드를 완료된 탭 없는 보유 효과 구현에 맞췄다.
- 교체 대기 및 다음 일반 축복 Presenter 표현을 현재 직접 연결된 아이템/카테고리/페이지 그래프와 사용자 Unity 검증 단계로 교체했다.
- 직접 YAML 연결은 `Panel_OwnedEffects.prefab`, `UIContentInventoryCategory.prefab`, `UIInventoryItemView.prefab` 세 경로에 대한 1회 승인임을 기록했다. 이후 프리팹 수정이나 Unity 조작에 대한 상시 권한이 아니다.
- 현재 소스와 프리팹 YAML에서 네 스크립트 연결, 비어 있지 않은 필수 참조, 패널 내부 `RelicContentInfoPresenter` 제거, 공용 상세 활성 상태, 고유한 `presentation.inventory.*` 행 네 개를 확인했다.
- `GeneralBlessCatalogPresenter`가 존재하지 않음을 확인했다. 별도 일반 축복 도감은 완료된 Catalog 경로가 아니라 향후 독립 페이지 계획으로 유지한다.
- 문서 관리자는 Runtime 소스, 프리팹, 로컬라이징 에셋, Scene, Staging, Commit, Push를 변경하지 않았고 보고된 Solution 빌드를 재실행하지 않았다.

## 2026-08-21 - 신앙 현재/다음 효과 카드 설계 정정

- 신앙 로드맵 아래의 독립 기능 카드 네 개와 선택 상세 영역 계획을 실제 현재 레벨과 바로 다음 레벨 신앙 효과를 표시하는 `UIFaithLevelEffectCard` 두 개로 교체했다.
- Localization 라벨을 비교하거나 새로운 차이 수치를 계산하지 않고, 원본 ID를 기준으로 `Strengthened`, `NewlyUnlocked`, `Unchanged` 비교 상태를 정의했다.
- 최대 레벨에서도 다음 카드 위치를 유지하고 Localization된 다음 레벨 없음 빈 상태를 표시하도록 했다.
- 미래 로드맵 노드는 이정표 정보로 유지하며 하단 비교 기준을 실제 현재/다음 레벨에서 바꾸지 않는다.
- 영문/한국어 신앙 설계와 활성 작업 계약을 갱신했다. 코드, 프리팹, Scene, 에셋, 빌드, Staging, Commit, Push 작업은 수행하지 않았다.

## 2026-08-21 - 신앙 메인 패널 직접 골격 구현

- 사용자의 명시적인 직접 수정 승인에 따라 `Assets/Prefabs/UI/Fixed/Panel/Panel_FaithInfo.prefab`의 배경과 전경 시각 요소를 유지하면서 하이어라키를 재구성했다.
- 구형 `FaithDetailView`와 미완성 레이아웃 오브젝트를 제거하고 신앙 탭, 선택 신 요약, 수평 레벨 노드 10개, 현재/다음 효과 카드 두 개를 만들었다.
- Runtime UI 컴포넌트 타입 다섯 개를 추가하고 부착했다. AutoBind 참조, 레벨 노드 참조 10개, Inspector 설정 신 목록, 중립 `UIContentInfoView` 인스턴스 두 개를 패널에 직렬화했다.
- 호출 가능한 `Build Configured Faith Page` ContextMenu 골격과 원본 기반 현재/다음 비교 Resolver 대기 지점의 명시적인 `[PLACEHOLDER]` 로그를 추가했다.
- `presentation.faith.*` StringManager 행 13개를 추가하고, 수정 전 정적 확인에서 나타난 예외 두 건을 계기로 공용 `AutoBindEditorUtility`의 GameObject 필드 처리를 수정했다.
- 컴포넌트 개수, 비어 있지 않은 참조, 레벨 노드 개수, 수평 Layout 설정, Localization 고유성, 구형 컴포넌트 제거 정적 검증을 통과했다.
- Unity Editor 정적 확인에서 재구성된 하이어라키와 균등한 레벨 노드 10개 배치를 확인했다. 전체 Solution 빌드는 오류 0개, 기존 경고 209개로 통과했다.
- 프리팹 저장 후 자동 실행되는 일회성 Editor Builder는 제거했다. Play Mode, 설정 SO 데이터, 생성 탭, 카드 내용, 클릭, Scroll 입력은 아직 검증하지 않았다.
