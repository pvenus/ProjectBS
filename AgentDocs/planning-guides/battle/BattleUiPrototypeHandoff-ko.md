# 전투 UI 프로토타입 작업 인계서

## 목적

이 문서는 ProjectBS와 기존 채팅 내용을 전혀 모르는 에이전트가 현재 전투
UI 프로토타입과 독립 전투 테스트 작업을 안전하게 이어서 진행하기 위한
인계 문서입니다.

작업 전에 다음 문서를 순서대로 읽습니다.

1. `AGENTS.md`
2. `AgentDocs/task-start-documentation-prompt.md`
3. 코드 작업이라면 `AgentDocs/code-writing-rules.md`
4. 이 인계서

영문 원본:
`AgentDocs/planning-guides/battle/BattleUiPrototypeHandoff.md`

상태 기준일: 2026-08-24.

## 권위와 작업 규칙

- 관리자 작업은 `[Battle]관리자`, 작업 ID는
  `01a02e6a-07c8-7313-890a-354e51cda301`입니다.
- 사용자의 요청은 관리자 작업에서 적절한 `[Battle]` 파트 작업으로
  전달합니다. 파트 작업은 결과를 관리자에게 다시 보고하고, 사용자는
  관리자 채팅에서 피드백합니다.
- 모든 작업은 같은 로컬 체크아웃
  `C:/_UnityProjects/ProjectBS`를 사용합니다.
- 새 worktree를 만들지 않습니다. 이전 worktree 시도가 실패했고 사용자는
  현재 로컬 체크아웃 공유 방식을 선택했습니다.
- 사용자가 명시적으로 요청하지 않으면 commit, push, 광범위 staging,
  관련 없는 변경 정리를 하지 않습니다.
- 현재 작업 트리는 많은 수정·미추적 파일이 있는 dirty 상태입니다. 기존
  변경을 보존합니다.
- 프로젝트 루트 기준의 정확한 경로를 사용합니다.
- 사용자가 Unity 계층, Inspector, Prefab Mode, 메뉴 실행을 직접 하겠다고
  한 작업은 사용자가 수행합니다. 이 경우 대신 Unity를 조작하거나 씬·프리팹
  YAML을 직접 편집하지 않습니다.
- .NET 빌드나 YAML 정적 검사는 Unity import, 직렬화, Play Mode 검증이
  아닙니다.

## 제품 방향과 확정된 UI 결정

최초 컨셉 이미지는 미술을 복제하는 용도가 아니라 구조와 배치 참고용으로만
사용했습니다.

확정 범위:

- 파티 정보 HUD는 교체 가능한 배경, 초상화 슬롯, 스킬 아이콘 슬롯, HP 바,
  상태와 텍스트를 포함합니다.
- 캐릭터마다 액티브 스킬 슬롯 4개를 사용하고 첫 슬롯은 기본 공격입니다.
  패시브 슬롯 1개는 별도입니다.
- 스킬 쿨타임은 원형 이미지 fill과 남은 시간 텍스트를 모두 표시합니다.
- 기본 공격 표시 여부는 구조 변경 없이 전환할 수 있으며 현재 기본값은
  표시입니다.
- 전략 보드는 왼쪽 공용 게이지, 오른쪽 전략 스킬 슬롯 4개 구조입니다.
- 전략 게이지는 실제 fill과 갱신 로직을 사용합니다. 이후 미술 방향은 초기
  8각형에서 붓터치 배경, 10분할 칸막이 foreground, inner fill을 사용하는
  가로형 게이지로 변경됐습니다.
- 전략 슬롯의 상태 overlay는 슬롯 전체를 덮는 이미지가 아니라 가운데 상태
  아이콘을 사용합니다.
- 전투 진행 정보는 상단 중앙, 보스 정보는 상단 우측에 배치합니다.
- AUTO, 배속, 일시정지 UI는 범위에서 삭제했습니다.
- 현재 프리팹 루트는 `Assets/Prefabs/UI/Fixed/Battle`입니다. 초기 작업
  프롬프트의 `Assets/Prefabs/Fixed/Battle` 경로는 사용하지 않습니다.
- 전투 UI 생성 이미지는 `Assets/ImageGenerated/Battle/UI` 아래에 둡니다.

## 런타임 데이터 흐름

일반 전투 흐름:

```text
Stage 씬
  -> GameSession과 BattleSession 준비
  -> LoadingScene
  -> BattleScene의 Manager가 세션 데이터 소비
```

독립 테스트 흐름:

```text
BattleFeatureTestBootstrap Inspector 데이터
  -> GameSession.BattleSession 직접 전투 상태
  -> BattleSession.PartyRuntimeData
  -> GameSession.StageSession.StrategicSkillItemRuntimeData
  -> ItemManager 전략 스킬 서비스
  -> 기존 BattleManager / PartyManager / Spawn / AI 흐름
```

데이터 소유권:

- `BattleFeatureTestBootstrap`은 시작 시 데이터를 주입하는 역할만 합니다.
- 파티 런타임 권위는 `GameSession.BattleSession.PartyRuntimeData`입니다.
- 전투 권위는 `GameSession.BattleSession.BattleSO/BattleRuntime`입니다.
- 전략 스킬 보유 권위는
  `GameSession.StageSession.StrategicSkillItemRuntimeData`와
  `ItemManager` 서비스입니다.
- 전략 게이지 권위는 `StrategicSkillCostManager`입니다.
- `BattleUiDataSetupTester`는 표시 전용입니다. 전투 런타임 데이터를
  소유하거나 생성하지 않습니다.

직접 테스트 경로의 핵심 변경:

- `BattleSession.TryPrepareDirectBattle(...)`가 `LoadingScene`을 로드하지
  않고 현재 씬의 전투 상태를 준비합니다.
- `PartyManager.IsBattleSpawnContext(...)`는 일반 `BattleScene` 또는 활성
  세션의 `BattleSceneName`과 현재 씬 이름이 정확히 일치하는 경우에만 전투
  파티를 생성합니다.
- `StrategicSkillCostManager`의 `[DefaultExecutionOrder(-100)]`으로
  `ItemManager`가 null 게이지 매니저를 캡처하지 않게 합니다.
- `BattleFeatureTestBootstrap`의 `[DefaultExecutionOrder(1000)]`으로
  Manager `Awake` 이후 데이터를 넣고 Manager `Start`가 이를 소비하게 합니다.

## 파생 작업과 소유 범위

| 파트 작업 | 작업 ID | 결과와 현재 용도 |
| --- | --- | --- |
| `[Battle] 파티 HUD 프리팹` | `01a02e7e-1c96-77a0-89c2-09ac69fd7c88` | 파티 루트/멤버/스킬 슬롯 프리팹과 표시 데이터/View API가 있습니다. 런타임 어댑터와 최종 씬 통합은 별도입니다. |
| `[Battle] 전략 스킬 보드 프리팹` | `01a02e7e-2073-7f62-bae5-6bbee50d776c` | 보드/게이지/슬롯 프리팹과 View/Binder API가 있습니다. 참조 복구 메뉴가 준비됐지만 현재 프리팹에는 아직 실행되지 않았습니다. |
| `[Battle] 전투 진행·보스 상태 프리팹` | `01a02e7e-2566-7ed1-980e-1e0a8646d747` | 독립 진행/보스 프리팹과 ViewData API가 있습니다. 런타임 Presenter와 최종 씬 배치가 남았습니다. |
| `[Battle] UI 이미지 생성` | `01a02ed5-cb5b-7963-88be-a222e61957de` | 전략 보드용 흰색 투명 기하 이미지, 상태 아이콘, 가로 게이지 이미지가 있습니다. 초기 목재/황동 시안은 보존됐지만 현재 방향이 아닙니다. |
| `[Battle] UI 데이터 주입 테스트` | `01a02f59-af55-7b50-a2d5-69ca6ff6bc23` | `BattleUiDataSetupTester`가 CharacterSO와 StrategicSkillItemSO를 파티/전략 View에 표시합니다. 이전 전투 진입 메서드는 placeholder이며 Bootstrap은 사용하지 않습니다. |
| `[Battle] 독립 전투 테스트 씬` | `01a02ffc-e18b-7a22-944b-a830c922b1a4` | 직접 전투 세션/Bootstrap 흐름이 있습니다. 씬 계층은 사용자가 소유합니다. 전략 UI 씬 통합은 공용 프리팹 복구를 기다립니다. |
| `[Battle] 테스트 파티·전략 스킬 SO` | `01a0302c-e205-7322-a327-78e520c481d3` | 캐릭터 3명, 공용 캐릭터 스킬 5개, 전략 아이템 4개, 프로필/효과, 문자열, 재생성 빌더를 만들고 Unity 검증했습니다. |

작업은 `codex://threads/{작업 ID}`로 열 수 있습니다. 앞으로 변경할 경로를
소유한 파트 작업에 요청하고, 완료 보고를 관리자 작업으로 돌려보냅니다.

## 구현된 UI 에셋

### 파티 HUD

스크립트:

```text
Assets/Scripts/Battle/UI/PartyHud/
```

프리팹:

```text
Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudRoot.prefab
Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudMember.prefab
Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudSkillSlot.prefab
```

구현 기능:

- 파티원 1~4명 표시 데이터.
- 배경, 초상화, 이름, HP 배경/fill/현재/최대 텍스트, 상태.
- 액티브 4개와 패시브 1개.
- 첫 액티브 슬롯은 기본 공격.
- 원형 쿨타임 fill, 쿨타임 텍스트, ready/locked/passive 표현.
- 구조를 다시 만들지 않는 `SetBasicAttackVisible(bool)`.
- View는 Manager나 구체 SO를 해석하지 않고 표시 데이터를 소비합니다.

현재 한계:

- 현재 독립 전투 Play에서 보이는 HUD는 `CharacterBuilder`가 생성한 기존
  캐릭터 런타임 HUD입니다. 새 `PartyHudRoot.prefab` 통합 증거가 아닙니다.
- 초상화와 생성 테스트 스킬 아이콘은 현재 null입니다.
- 실제 런타임 Presenter/어댑터와 폭넓은 Play Mode 검증이 필요합니다.

### 전략 보드

스크립트:

```text
Assets/Scripts/Battle/UI/StrategicBoard/
```

프리팹:

```text
Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicBoard.prefab
Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicGauge.prefab
Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicSkillSlot.prefab
```

구현 API와 기능:

- 게이지 현재/최대 표시와 fill 갱신.
- 초당 충전량 표시.
- `StrategicSkillCostManager.OnGaugeChanged`를 구독하는
  `StrategicGaugeBinder`.
- 슬롯 상태, 선택, 자원 충분/부족, empty/locked/disabled 표현, drag 콜백,
  `ExecutionRequested`.
- 슬롯 payload로 `StrategicSkillItemSO` 전달 가능.

현재 디스크의 중요 상태:

- `StrategicBoard.prefab`의 `StrategicBoardView.slots`는 현재
  `[첫 슬롯, null, null, null]`로 직렬화돼 있습니다.
- 독립 `StrategicGauge.prefab`의 `boardView: null`은 부모 보드가 없으므로
  정상입니다. `StrategicBoard.prefab` 안의 중첩 게이지 인스턴스에는
  `boardView` override가 필요합니다.
- `managerOverride: null`, `findManagerInScene: true`는 재사용 프리팹의
  의도된 상태입니다.
- `StrategicBoardPrefabBuilder`에
  `Tools > Battle > Repair Strategic Board References` 메뉴가 추가됐습니다.
- 사용자가 아직 이 메뉴를 실행하지 않았습니다. 성공 로그와 현재 프리팹
  재검사 전에는 공용 프리팹이 수정됐다고 보고하지 않습니다.

복구 성공 로그:

```text
[StrategicBoardPrefabBuilder] Strategic board references repaired and verified.
```

메뉴 실행 후 슬롯 참조 4개, 고유 ID `strategic-slot-1`~`4`, 중첩 Binder의
`boardView`, null `managerOverride`, `findManagerInScene=true`를 확인합니다.

전략 스킬 실제 실행은 아직 통합되지 않았습니다. 다음 런타임 통합에서 각
슬롯의 `ExecutionRequested`를 구독하고 payload와 화면 위치를
`ItemManager.TryUseStrategicSkillItemFromScreenPosition(...)`으로 전달해야
합니다.

### 전투 진행 및 보스 상태

스크립트:

```text
Assets/Scripts/Battle/UI/BattleStatus/
```

프리팹:

```text
Assets/Prefabs/UI/Fixed/Battle/BattleStatus/BattleProgressView.prefab
Assets/Prefabs/UI/Fixed/Battle/BattleStatus/BossStatusView.prefab
```

구현 표시 항목:

- 전투 이름, 현재/전체 wave, 남은 시간, 경과 시간, 남은 적 수.
- BOSS 라벨/이름, 가로 HP fill, 현재/최대 HP.
- Show/hide/render API와 비정상 값 안전 보정.

남은 작업:

- 권위 런타임 Presenter/Binder.
- 보스 식별과 HP 데이터 소스.
- 현재/전체 wave 데이터 소스.
- 최종 Canvas 배치와 Play Mode 검증.

### 생성된 전략 UI 이미지

현재 단순 흰색/투명 에셋:

```text
Assets/ImageGenerated/Battle/UI/StrategicBoard/StrategicBoard/
Assets/ImageGenerated/Battle/UI/StrategicBoard/SharedGauge/
Assets/ImageGenerated/Battle/UI/StrategicBoard/StrategicSkillSlot/
```

가로 게이지 주요 파일:

```text
shared-gauge-brush-background.png
shared-gauge-segmented-foreground.png
shared-gauge-inner-bar.png
```

초기 `strategic-board-background.png`, `strategic-board-frame.png` 목재/황동
시안은 히스토리로 남아 있지만 현재 방향은 아닙니다. 현재 방향은 단순한
흰색/투명 UI 파츠입니다.

## 독립 전투 테스트 런타임

주요 경로:

```text
Assets/Scenes/BattleFeatureTestScene.unity
Assets/Scripts/Battle/Test/BattleFeatureTestBootstrap.cs
Assets/Scripts/Battle/UI/Test/BattleUiDataSetupTester.cs
Assets/Scripts/Battle/Session/BattleSession.cs
Assets/Scripts/Actor/Party/PartyManager.cs
Assets/Scripts/Battle/AbilityIntegration/StrategicSkillCost/StrategicSkillCostManager.cs
```

사용자가 직접 만든 씬 오브젝트:

```text
Main Camera
Core/GameSession
Core/PartyManager
Core/BattleSystems
Core/BattleSystems/StrategicSkillCost
Core/BattleSystems/Item
Core/BattleSystems/Currency
Core/BattleSystems/BattleProp
Core/StringManager
BattleTestBootstrap
```

현재 직렬화된 Bootstrap 상태:

- BattleSO:
  `Assets/Resources/battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.asset`
- 현재 파티 목록에는 Ranger와 Vanguard만 있습니다.
- Medic 에셋은 존재하지만 현재 씬 Bootstrap에는 연결되지 않았습니다.
- 생성된 전략 아이템 4개는 모두 연결돼 있습니다.
- GameSession, BattleManager, PartyManager, ItemManager,
  StrategicSkillCostManager 참조는 non-null입니다.
- `prepareOnAwake=true`.
- `returnSceneName=StageScene`.
- `findMissingReferencesAutomatically=false`.
- `battleUiDataSetupTester=null`.

사용자가 확인한 Play 결과:

- 캐릭터가 생성됩니다.
- 기존 캐릭터 및 캐릭터 스킬 HUD가 보입니다.
- 전략 보드는 보이지 않습니다.

전략 UI가 보이지 않는 확정 원인:

- `BattleFeatureTestScene.unity`에 Canvas가 없습니다.
- EventSystem이 없습니다.
- `StrategicBoard.prefab` 인스턴스가 없습니다.
- `BattleUiDataSetupTester`가 없습니다.
- Bootstrap의 Tester 참조가 null입니다.

현재 보이는 캐릭터 HUD를 PartyHud 프로토타입 통합으로 판단하지 않습니다.

## BattleUiDataSetupTester 사용법

Bootstrap 연동 권장 구성:

1. 씬 오브젝트에 `BattleUiDataSetupTester`를 추가합니다.
2. PartyHud 프로토타입을 시험할 때 `PartyHudView` 또는
   `PartyBoard Root`를 연결합니다.
3. 씬의 `StrategicBoardView`를 연결합니다.
4. Tester를 `BattleFeatureTestBootstrap.battleUiDataSetupTester`에
   연결합니다.
5. `injectOnStart=false`를 유지합니다.
6. Play Mode에 진입합니다.

Bootstrap이 `Configure(...)` 후 `ApplyConfiguredData()`를 호출합니다. 이
방식에서는 Tester Inspector에 CharacterSO/StrategicSkillItemSO 목록을
중복 입력하지 않습니다.

독립 수동 Context Menu는 Play Mode에서만 사용합니다.

```text
Apply Configured Data
Apply Party Data
Apply Strategic Data
Clear Test Data
```

`Inject Configured Data At Battle Entry`는 `[PLACEHOLDER]` 로그만 남기며
정상 Bootstrap 흐름에서 사용하지 않습니다.

## 생성된 테스트 콘텐츠

원본 데이터와 생성 결과:

```text
Assets/Contents/Character/
Assets/Contents/Skill/
Assets/Contents/Skill/Effects/
Assets/Contents/Skill/Profiles/
Assets/Contents/StrategicSkill/
Assets/Contents/StrategicSkill/Effects/
Assets/Contents/StrategicSkill/Profiles/
Assets/Contents/StrategicSkill/Skills/
Assets/Editor/tools/content/BattleTestContentAssetBuilder.cs
Assets/Resources/string/battle_test_content_string.csv
```

생성 메뉴:

```text
Tools > ProjectBS > Contents > Battle Test > Build First Character + Basic Attack
Tools > ProjectBS > Contents > Battle Test > Build Full Party + Strategic Skills
Tools > ProjectBS > Contents > Battle Test > Validate Battle Test Content
```

생성 결과:

- `battle_test*.asset` 70개.
- canonical JSON 12개.
- CharacterSO 3개.
- 공용 캐릭터 EquipmentSkillSO 5개.
- StrategicSkillItemSO 4개와 실행 EquipmentSkillSO 4개.
- EffectSO 9개와 EffectEntrySO 9개.
- 전체 빌더 재실행 후 GUID 변화와 중복 생성 없음.
- 별도 CSV에 `battle_test` 문자열 키 저장.
- 모든 테스트 아이콘은 의도적으로 null.
- 캐릭터 전용 prefab과 BaseVisualSO는 생성하지 않음.

캐릭터:

```text
Assets/Contents/Character/battle_test.character.vanguard.asset
Assets/Contents/Character/battle_test.character.ranger.asset
Assets/Contents/Character/battle_test.character.medic.asset
```

세 캐릭터는 공통 슬롯 5개를 사용합니다.

```text
basic_attack
active_1
active_2
active_3
passive_1
```

공용 스킬:

```text
battle_test.skill.basic_attack
battle_test.skill.guard_rush
battle_test.skill.volley
battle_test.skill.field_mend
battle_test.skill.steady_training
```

세 캐릭터는
`Assets/Resources/character/Player/main/character_military_officer_1.asset`의
동일한 애니메이션 12개를 참조합니다. Idle 4방향, Move 4방향, Attack
4방향입니다. JSON은 프로젝트 기준 AnimationClip 경로를 저장하고 빌더는
정확한 참조 일치를 검사합니다.

현재 튜닝 메모:

- 기본 공격 사정거리는 Unity 월드 기준 `2.2`입니다.
- MoveSpeed는 각 Character canonical JSON에서 수정합니다. 현재 값은
  Vanguard `2.6`, Ranger `3.5`, Medic `3.0`입니다.
- 캐릭터를 느리게 만들 때 MoveSpeed를 0으로 설정하지 않습니다. 런타임
  fallback이 기본 속도를 사용하므로 양수의 낮은 값을 사용하고 재생성합니다.
- 생성 `.asset`을 직접 수정하면 다음 빌드에서 JSON 값으로 덮어씁니다.

전략 아이템:

```text
Assets/Contents/StrategicSkill/battle_test.strategic.arrow_barrage.asset    cost 20
Assets/Contents/StrategicSkill/battle_test.strategic.iron_banner.asset      cost 35
Assets/Contents/StrategicSkill/battle_test.strategic.recovery_field.asset   cost 50
Assets/Contents/StrategicSkill/battle_test.strategic.thunder_judgment.asset cost 70
```

모두 재사용 가능하며 실행용 `EquipmentSkillSO` 참조가 non-null입니다.

## 즉시 다음 작업

다음 순서를 지키고 각 검증 gate를 건너뛰지 않습니다.

### 1. 공용 StrategicBoard 프리팹 복구

사용자가 다음 메뉴를 실행합니다.

```text
Tools > Battle > Repair Strategic Board References
```

성공 로그와 직렬화 참조를 다시 확인합니다. 이 검증 전에는 테스트 씬에 보드를
배치하지 않습니다.

### 2. 독립 테스트 씬에 전략 UI 추가

사용자가 직접 수행할 Unity 작업:

1. 여전히 없다면 Canvas와 EventSystem을 추가합니다.
2. 복구된
   `Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicBoard.prefab`을
   Canvas 아래에 배치합니다.
3. `BattleUiDataSetupTester` 컴포넌트를 추가합니다.
4. Tester의 `strategicBoardView`를 씬 보드 인스턴스에 연결합니다.
5. Tester를 Bootstrap의 `battleUiDataSetupTester`에 연결합니다.
6. `injectOnStart=false`를 유지합니다.

공용 프리팹 복구 후에는 슬롯 4개, 중첩 `boardView`, `managerOverride`를 씬
override로 따로 설정하지 않습니다.

### 3. 전략 UI Play 테스트

인수 조건:

- 보드가 화면 안에 표시됩니다.
- 비용 20, 35, 50, 70이 표시됩니다.
- 현재/최대 게이지가 보입니다.
- 게이지 변화에 따라 자원 부족 상태가 갱신됩니다.
- 기존 캐릭터 HUD가 유지됩니다.
- Console에 새 오류가 없습니다.
- Tester 요약 로그가 `strategicSlots=4`를 표시합니다.

### 4. 다음 통합 단위 결정

권장 순서:

1. `StrategicSkillSlotView.ExecutionRequested`를 실제 ItemManager 화면 위치 실행
   경로에 연결하고 비용 소모를 Play 검증합니다.
2. 새 PartyHud 프로토타입을 권위 파티 런타임 데이터에 연결합니다. 기존
   캐릭터 HUD 교체는 사용자가 명시적으로 결정하기 전까지 별도로 유지합니다.
3. BattleProgress/BossStatus Presenter와 씬 배치를 구현합니다.
4. null 아이콘/초상화를 교체하고 승인된 단순 UI 이미지를 적용합니다.
5. Canvas 해상도, anchor, safe area를 검증합니다.
6. 기능 검증 후 최종 간격, tint, 미술 polish를 진행합니다.

## 검증 매트릭스

| 영역 | 현재 증거 | 남은 검증 |
| --- | --- | --- |
| PartyHud 코드/프리팹 | 파트 작업에서 .NET 컴파일과 정적 프리팹/참조 검사 통과. | 런타임 Presenter, 최종 Canvas 배치, 해상도와 Play 테스트. |
| 전략 보드 코드/프리팹 | View/Binder API, 복구 코드, 검증기 존재. | 복구 메뉴 실행, 현재 프리팹 재검사, 씬 배치, `strategicSlots=4`, 실행 연결. |
| 전투 상태 코드/프리팹 | .NET 컴파일과 정적 프리팹 검사 통과. | 런타임 데이터 소스, Presenter, 씬 배치, Play 테스트. |
| 독립 전투 Bootstrap | 빌드 통과, 사용자가 캐릭터와 기존 스킬 HUD 표시 확인. | 전체 Play 회귀, 필요 시 세 번째 캐릭터 연결, UI 통합, 전투 완료/귀환 검증. |
| 테스트 콘텐츠 | Unity 생성/검증, Console 오류 0, 재생성 GUID 안정성 통과. | 시각 에셋, 필요 시 역할별 시각/애니메이션, 밸런싱. |
| 전략 UI 이미지 | 생성 과정에서 PNG alpha/크기와 Unity Sprite meta 검사. | 프리팹 Sprite 할당, slicing/type/layout, 최종 미술 검토. |

## 안전 규칙과 자주 발생하는 실패

- 사용자가 범위를 넓히지 않으면 독립 테스트 씬 작업 중
  `Assets/Scenes/BattleScene.unity`를 수정하지 않습니다.
- 파트 완료 보고를 현재 상태로 그대로 믿지 말고 디스크를 재검사합니다. 전략
  보드는 과거 슬롯 4개 참조가 있다고 보고됐지만 현재 디스크에는 null 3개가
  있습니다.
- 이전 `Assets/Prefabs/Fixed/Battle` 경로를 사용하지 않습니다.
- AUTO, 배속, 일시정지 UI를 다시 만들지 않습니다.
- 생성 SO를 직접 편집하지 않고 canonical JSON을 수정한 뒤 빌더를 실행합니다.
- `BattleUiDataSetupTester.InjectConfiguredDataAtBattleEntry()`를 사용하지
  않습니다.
- 슬롯에 아이템이 표시된다는 이유로 전략 스킬 실행이 된다고 보고하지
  않습니다.
- 기존 CharacterBuilder HUD가 보인다는 이유로 PartyHud 통합 완료라고
  보고하지 않습니다.
- 프리팹의 `StrategicGaugeBinder.managerOverride`는 null로 유지하고 씬 자동
  탐색을 사용합니다.
- 사용자 씬 오브젝트, 관련 없는 dirty 파일, 기존 GUID를 보존합니다.

## 다음 완료 보고 체크리스트

앞으로 각 작업 단위를 완료할 때 다음을 포함합니다.

1. 완료 동작.
2. 정확한 변경 경로.
3. 현재 디스크/직렬화 증거.
4. 빌드와 구분된 Unity Editor/Play Mode 증거.
5. 남은 placeholder와 미완료 런타임 통합.
6. 사용자가 직접 수행해야 하는 Unity 작업.
7. 관련 없는 변경, commit, push, worktree를 건드리지 않았다는 확인.
