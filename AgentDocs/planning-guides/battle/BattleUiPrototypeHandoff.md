# Battle UI Prototype Continuation Handoff

## Purpose

This document lets an agent with no prior ProjectBS or chat context continue the
current battle UI prototype and standalone battle test work safely.

Read these files before making changes:

1. `AGENTS.md`
2. `AgentDocs/task-start-documentation-prompt.md`
3. `AgentDocs/code-writing-rules.md` for code work
4. This handoff

Korean mirror:
`AgentDocs/planning-guides/battle/BattleUiPrototypeHandoff-ko.md`

Snapshot date: 2026-08-24.

## Authority and Working Rules

- The manager task is `[Battle] manager`, task ID
  `01a02e6a-07c8-7313-890a-354e51cda301`.
- Requests are routed from the manager task to the relevant `[Battle]` part
  task. Part tasks report results back to the manager for user feedback.
- All tasks use the same local checkout: `C:/_UnityProjects/ProjectBS`.
- Do not create another worktree. Earlier worktree attempts failed, and the user
  chose the shared local checkout.
- Do not commit, push, stage broad paths, or clean unrelated changes unless the
  user explicitly asks.
- Preserve the dirty worktree. Many files in this handoff are currently
  modified or untracked.
- Use exact project-root-relative paths.
- The user performs Unity hierarchy, Inspector, Prefab Mode, and menu actions
  when they say they will handle Unity directly. Do not take over those actions
  or hand-edit scene/prefab YAML in that situation.
- A .NET build or static YAML check is not Unity import, serialization, or Play
  Mode evidence.

## Product Direction and Confirmed UI Decisions

The original reference image was used for structure and placement, not for
copying its art.

Confirmed scope:

- Party information HUD with replaceable background, portrait slot, skill icon
  slots, HP bar, status, and text.
- Four active skill slots per character; the first active slot is the basic
  attack. One passive slot is separate.
- Skill cooldown uses both a radial image fill and remaining-time text.
- The basic attack can be shown or hidden without changing the HUD structure;
  the current default is shown.
- Strategic board layout is a shared gauge on the left and four strategic skill
  slots on the right.
- The strategic gauge has real fill/update logic. Later art direction changed
  from the early octagonal concept to a horizontal bar with a brush background,
  a ten-compartment foreground, and an inner fill bar.
- Strategic slot overlays use centered state icons rather than a full-slot
  overlay image.
- Battle progress belongs at the top center. Boss status belongs at the top
  right.
- AUTO, speed controls, and pause controls were removed from this scope.
- The current prefab root is `Assets/Prefabs/UI/Fixed/Battle`, not the older
  `Assets/Prefabs/Fixed/Battle` path used in early task prompts.
- Generated battle UI images live under `Assets/ImageGenerated/Battle/UI`.

## Runtime Data Flow

Normal production flow:

```text
Stage scene
  -> GameSession and BattleSession are prepared
  -> LoadingScene
  -> BattleScene managers consume the session
```

Standalone test flow:

```text
BattleFeatureTestBootstrap Inspector data
  -> GameSession.BattleSession direct battle state
  -> BattleSession.PartyRuntimeData
  -> GameSession.StageSession.StrategicSkillItemRuntimeData
  -> ItemManager strategic skill service
  -> existing BattleManager / PartyManager / spawn / AI flow
```

Important ownership:

- `BattleFeatureTestBootstrap` is only the startup injector.
- Party runtime authority is
  `GameSession.BattleSession.PartyRuntimeData`.
- Battle authority is `GameSession.BattleSession.BattleSO/BattleRuntime`.
- Strategic inventory authority is
  `GameSession.StageSession.StrategicSkillItemRuntimeData` plus the
  `ItemManager` service.
- Strategic gauge authority is `StrategicSkillCostManager`.
- `BattleUiDataSetupTester` is display-only. It does not own or create battle
  runtime data.

The direct test path uses:

- `BattleSession.TryPrepareDirectBattle(...)` to prepare the current scene
  without loading `LoadingScene`.
- `PartyManager.IsBattleSpawnContext(...)` to accept either the production
  `BattleScene` or an active session whose `BattleSceneName` exactly matches the
  current scene.
- `[DefaultExecutionOrder(-100)]` on `StrategicSkillCostManager`, so
  `ItemManager` does not capture a null cost manager.
- `[DefaultExecutionOrder(1000)]` on `BattleFeatureTestBootstrap`, so manager
  `Awake` methods finish before test data is injected and manager `Start`
  methods consume it.

## Derived Tasks and Ownership

| Part task | Task ID | Result and current use |
| --- | --- | --- |
| `[Battle] Party HUD Prefabs` | `01a02e7e-1c96-77a0-89c2-09ac69fd7c88` | Party root/member/skill-slot prefabs and display data/View APIs exist. Runtime adapter and final scene integration remain separate. |
| `[Battle] Strategic Skill Board Prefabs` | `01a02e7e-2073-7f62-bae5-6bbee50d776c` | Board/gauge/slot prefabs and View/Binder APIs exist. A reference-repair menu is ready, but the repair has not yet been executed on the current prefab. |
| `[Battle] Battle Progress and Boss Status Prefabs` | `01a02e7e-2566-7ed1-980e-1e0a8646d747` | Independent progress and boss status prefabs/ViewData APIs exist. Runtime presenter and final scene placement remain pending. |
| `[Battle] UI Image Generation` | `01a02ed5-cb5b-7963-88be-a222e61957de` | Strategic board white transparent geometry/state icons and horizontal gauge images exist. Earlier wood/bronze experiments are retained but are not the current direction. |
| `[Battle] UI Data Injection Test` | `01a02f59-af55-7b50-a2d5-69ca6ff6bc23` | `BattleUiDataSetupTester` can display CharacterSO and StrategicSkillItemSO data in Party/Strategic Views. Its old battle-entry method is still a placeholder and is not used by the bootstrap. |
| `[Battle] Standalone Battle Test Scene` | `01a02ffc-e18b-7a22-944b-a830c922b1a4` | Direct battle session/bootstrap flow exists. The user owns scene hierarchy setup. Strategic UI scene integration is waiting for the shared prefab repair. |
| `[Battle] Test Party and Strategic Skill SOs` | `01a0302c-e205-7322-a327-78e520c481d3` | Three characters, five shared character skills, four strategic items, profiles/effects, localization, and a repeatable builder were created and Unity-validated. |

Use `codex://threads/{task-id}` to open a task. Send future changes to the part
that owns the affected paths, then require a completion report back to the
manager task.

## Implemented UI Assets

### Party HUD

Scripts:

```text
Assets/Scripts/Battle/UI/PartyHud/
```

Prefabs:

```text
Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudRoot.prefab
Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudMember.prefab
Assets/Prefabs/UI/Fixed/Battle/PartyHud/PartyHudSkillSlot.prefab
```

Implemented behavior:

- 1-4 member display data.
- Background, portrait, name, HP background/fill/current/max text, and status.
- Four active slots and one passive slot.
- First active slot is the basic attack.
- Radial cooldown fill, cooldown text, ready/locked/passive presentation.
- `SetBasicAttackVisible(bool)` without rebuilding the structure.
- Views consume display data and do not interpret concrete managers/SOs.

Current limitation:

- The HUD visible in the current standalone Play test is the existing runtime
  character HUD created by `CharacterBuilder`, not proof that the new
  `PartyHudRoot.prefab` is integrated.
- Portraits and generated test skill icons are currently null.
- A production runtime presenter/adapter and broad Play Mode coverage are still
  required.

### Strategic Board

Scripts:

```text
Assets/Scripts/Battle/UI/StrategicBoard/
```

Prefabs:

```text
Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicBoard.prefab
Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicGauge.prefab
Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicSkillSlot.prefab
```

Implemented APIs and behavior:

- Gauge current/max display and fill update.
- Charge-per-second display.
- `StrategicGaugeBinder` subscription to
  `StrategicSkillCostManager.OnGaugeChanged`.
- Four slot states, selection, resource availability, empty/locked/disabled
  presentation, drag callbacks, and `ExecutionRequested`.
- Slot payload can carry `StrategicSkillItemSO`.

Critical current disk state:

- `StrategicBoardView.slots` is currently serialized as
  `[slot 1, null, null, null]` in `StrategicBoard.prefab`.
- The standalone `StrategicGauge.prefab` has `boardView: null`, which is normal
  because it has no parent board. The nested gauge instance in
  `StrategicBoard.prefab` must receive a `boardView` override.
- `managerOverride: null` with `findManagerInScene: true` is intentional for a
  reusable prefab.
- `StrategicBoardPrefabBuilder` now contains
  `Tools > Battle > Repair Strategic Board References`.
- The user has not yet run that repair menu. Do not claim the shared prefab is
  fixed until the menu succeeds and the current prefab is rechecked.

Repair acceptance evidence:

```text
[StrategicBoardPrefabBuilder] Strategic board references repaired and verified.
```

After running the menu, verify all four slot references, unique IDs
`strategic-slot-1` through `strategic-slot-4`, nested binder `boardView`, null
`managerOverride`, and `findManagerInScene=true`.

Actual strategic skill execution is still not integrated. The next runtime
integration must subscribe to each slot's `ExecutionRequested` and route the
payload/screen position to
`ItemManager.TryUseStrategicSkillItemFromScreenPosition(...)`.

### Battle Progress and Boss Status

Scripts:

```text
Assets/Scripts/Battle/UI/BattleStatus/
```

Prefabs:

```text
Assets/Prefabs/UI/Fixed/Battle/BattleStatus/BattleProgressView.prefab
Assets/Prefabs/UI/Fixed/Battle/BattleStatus/BossStatusView.prefab
```

Implemented display fields:

- Battle name, current/total wave, remaining time, elapsed time, and remaining
  enemy count.
- Boss label/name, horizontal HP fill, and current/max HP.
- Show/hide/render APIs and safe normalization for invalid values.

Pending:

- Authoritative runtime presenter/binder.
- Boss identification and HP source.
- Current/total wave source.
- Final Canvas placement and Play Mode verification.

### Generated Strategic UI Images

Current simple white/transparent assets:

```text
Assets/ImageGenerated/Battle/UI/StrategicBoard/StrategicBoard/
Assets/ImageGenerated/Battle/UI/StrategicBoard/SharedGauge/
Assets/ImageGenerated/Battle/UI/StrategicBoard/StrategicSkillSlot/
```

Notable horizontal gauge files:

```text
shared-gauge-brush-background.png
shared-gauge-segmented-foreground.png
shared-gauge-inner-bar.png
```

The earlier `strategic-board-background.png` and
`strategic-board-frame.png` wood/bronze experiments remain for history but are
not the current minimal-white direction.

## Standalone Battle Test Runtime

Primary paths:

```text
Assets/Scenes/BattleFeatureTestScene.unity
Assets/Scripts/Battle/Test/BattleFeatureTestBootstrap.cs
Assets/Scripts/Battle/UI/Test/BattleUiDataSetupTester.cs
Assets/Scripts/Battle/Session/BattleSession.cs
Assets/Scripts/Actor/Party/PartyManager.cs
Assets/Scripts/Battle/AbilityIntegration/StrategicSkillCost/StrategicSkillCostManager.cs
```

The user manually created independent scene objects for:

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

Current serialized bootstrap state:

- BattleSO:
  `Assets/Resources/battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.asset`
- Party list currently contains Ranger and Vanguard only.
- Medic exists but is not currently assigned to the scene bootstrap.
- All four generated strategic items are assigned.
- GameSession, BattleManager, PartyManager, ItemManager, and
  StrategicSkillCostManager references are non-null.
- `prepareOnAwake=true`.
- `returnSceneName=StageScene`.
- `findMissingReferencesAutomatically=false`.
- `battleUiDataSetupTester=null`.

User-confirmed Play result:

- Characters spawn.
- Existing character and character-skill HUD is visible.
- Strategic board is not visible.

Confirmed reason for missing strategic UI:

- `BattleFeatureTestScene.unity` currently has no Canvas.
- It has no EventSystem.
- It has no `StrategicBoard.prefab` instance.
- It has no `BattleUiDataSetupTester`.
- The bootstrap tester reference is null.

Do not treat the visible character HUD as PartyHud prototype integration.

## BattleUiDataSetupTester Usage

Recommended bootstrap-driven setup:

1. Add `BattleUiDataSetupTester` to a scene object.
2. Assign `PartyHudView` or `PartyBoard Root` when testing the PartyHud
   prototype.
3. Assign the scene `StrategicBoardView`.
4. Assign the tester to
   `BattleFeatureTestBootstrap.battleUiDataSetupTester`.
5. Keep `injectOnStart=false`.
6. Enter Play Mode.

The bootstrap calls `Configure(...)`, then `ApplyConfiguredData()`. Do not
duplicate the CharacterSO/StrategicSkillItemSO lists in the tester Inspector in
this mode.

Standalone manual context menus, Play Mode only:

```text
Apply Configured Data
Apply Party Data
Apply Strategic Data
Clear Test Data
```

Do not use `Inject Configured Data At Battle Entry`; it still logs a
`[PLACEHOLDER]` and is not part of the working bootstrap flow.

## Generated Test Content

Canonical authoring and generated output:

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

Generation menus:

```text
Tools > ProjectBS > Contents > Battle Test > Build First Character + Basic Attack
Tools > ProjectBS > Contents > Battle Test > Build Full Party + Strategic Skills
Tools > ProjectBS > Contents > Battle Test > Validate Battle Test Content
```

Generation result:

- 70 `battle_test*.asset` files.
- 12 canonical JSON files.
- Three CharacterSO assets.
- Five shared character EquipmentSkillSO assets.
- Four StrategicSkillItemSO assets and four execution EquipmentSkillSO assets.
- Nine EffectSO and nine EffectEntrySO assets.
- Re-running the full builder preserved all GUIDs and created no duplicates.
- Localization uses a separate CSV with `battle_test` keys.
- All test icons are intentionally null.
- Character-specific prefabs and BaseVisualSO assets were not created.

Characters:

```text
Assets/Contents/Character/battle_test.character.vanguard.asset
Assets/Contents/Character/battle_test.character.ranger.asset
Assets/Contents/Character/battle_test.character.medic.asset
```

All three use five slots:

```text
basic_attack
active_1
active_2
active_3
passive_1
```

Shared skills:

```text
battle_test.skill.basic_attack
battle_test.skill.guard_rush
battle_test.skill.volley
battle_test.skill.field_mend
battle_test.skill.steady_training
```

All three characters reference the same 12 animation clips from
`Assets/Resources/character/Player/main/character_military_officer_1.asset`:
four idle directions, four move directions, and four attack directions. The
JSON stores project-relative animation paths, and the builder validates exact
reference equality.

Current tuning notes:

- Basic attack range is `2.2` Unity world units.
- Character MoveSpeed is authored in each canonical Character JSON. Current
  values are Vanguard `2.6`, Ranger `3.5`, and Medic `3.0`.
- Do not set MoveSpeed to zero when trying to slow a character; runtime fallback
  uses a default speed. Use a positive lower value and rebuild.
- Direct edits to generated `.asset` files are overwritten by the builder.

Strategic items:

```text
Assets/Contents/StrategicSkill/battle_test.strategic.arrow_barrage.asset   cost 20
Assets/Contents/StrategicSkill/battle_test.strategic.iron_banner.asset     cost 35
Assets/Contents/StrategicSkill/battle_test.strategic.recovery_field.asset  cost 50
Assets/Contents/StrategicSkill/battle_test.strategic.thunder_judgment.asset cost 70
```

All are reusable and have non-null execution `EquipmentSkillSO` references.

## Immediate Next Actions

Perform these in order. Do not skip the verification gates.

### 1. Repair the shared StrategicBoard prefab

The user should run:

```text
Tools > Battle > Repair Strategic Board References
```

Then verify the success log and recheck the serialized references. Do not place
the board in the test scene before this gate passes.

### 2. Add strategic UI to the standalone test scene

The user owns these Unity actions:

1. Add a Canvas and EventSystem if they are still absent.
2. Place the repaired
   `Assets/Prefabs/UI/Fixed/Battle/StrategicBoard/StrategicBoard.prefab` under
   the Canvas.
3. Add a `BattleUiDataSetupTester` component.
4. Assign its `strategicBoardView` to the scene board instance.
5. Assign that tester to the bootstrap's `battleUiDataSetupTester` field.
6. Keep `injectOnStart=false`.

Do not add scene overrides for the four slots, nested `boardView`, or
`managerOverride` after the shared prefab is repaired.

### 3. Run the strategic presentation Play test

Acceptance checks:

- Board is visible and on-screen.
- Four costs display as 20, 35, 50, and 70.
- Current/max gauge is visible.
- Resource-insufficient presentation updates when the gauge changes.
- Existing character HUD remains visible.
- Console has no new errors.
- The tester summary reports `strategicSlots=4`.

### 4. Decide the next integration slice

Recommended order:

1. Wire `StrategicSkillSlotView.ExecutionRequested` to the real ItemManager
   screen-position execution path and Play-test cost spending.
2. Integrate the new PartyHud prototype with authoritative runtime party data;
   keep the existing runtime character HUD separate until replacement is an
   explicit user decision.
3. Add BattleProgress/BossStatus presenters and scene placement.
4. Replace null icons/portraits and apply the approved simple UI images.
5. Perform Canvas resolution/anchor/safe-area testing.
6. Only after functional validation, perform final visual spacing/tint/art
   polish.

## Validation Matrix

| Area | Current evidence | Still required |
| --- | --- | --- |
| PartyHud code/prefabs | .NET compile and static prefab/reference checks passed in the part task. | Runtime presenter, final Canvas placement, resolution and Play tests. |
| Strategic board code/prefabs | View/Binder APIs exist; repair code and verifier exist. | Run repair menu, recheck current prefab, scene placement, `strategicSlots=4`, execution wiring. |
| Battle status code/prefabs | .NET compile and static prefab checks passed. | Runtime data sources, presenter, scene placement, Play tests. |
| Standalone battle bootstrap | Build passed; user confirmed characters and existing skill HUD appear. | Full Play regression, third character assignment if desired, UI integration, battle completion/return checks. |
| Test content | Unity generation and validation passed; Console error 0; GUID rerun stability passed. | Visual assets, role-specific visuals/animations if desired, gameplay balancing. |
| Strategic images | PNG alpha/dimensions and Unity sprite meta were checked during generation. | Assign sprites to prefabs, verify slicing/type/layout, final art review. |

## Safety and Common Failure Modes

- Do not modify `Assets/Scenes/BattleScene.unity` while working on the standalone
  test scene unless the user explicitly broadens scope.
- Do not treat part-task reports as current truth without rechecking disk state.
  The strategic board was once reported with four references, but current disk
  inspection found three null references.
- Do not use the old `Assets/Prefabs/Fixed/Battle` path.
- Do not recreate AUTO/speed/pause controls.
- Do not manually edit generated SO assets; edit canonical JSON and rerun the
  content builder.
- Do not use `BattleUiDataSetupTester.InjectConfiguredDataAtBattleEntry()`.
- Do not claim strategic execution works merely because a slot displays an item.
- Do not claim PartyHud integration because the existing CharacterBuilder HUD
  is visible.
- Keep `StrategicGaugeBinder.managerOverride` null in the prefab; scene lookup is
  intentional.
- Preserve user scene objects, unrelated dirty files, and existing GUIDs.

## Handoff Completion Checklist

Before reporting a future slice complete, include:

1. Completed behavior.
2. Exact changed paths.
3. Current disk/serialization evidence.
4. Unity Editor and Play Mode evidence, clearly separated from builds.
5. Known placeholders and unresolved runtime integration.
6. User-owned Unity actions still required.
7. Confirmation that unrelated work, commits, pushes, and worktrees were not
   changed.
