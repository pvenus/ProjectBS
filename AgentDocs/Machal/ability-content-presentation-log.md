# Ability Content Presentation Task Log

Append new entries. Do not rewrite prior entries except to fix a factual typo; use a correction entry when a conclusion changes.

## 2026-08-08 — Documentation Handoff Created

- Status: verified documentation unit; implementation not started
- Scope:
  - Created the `AgentDocs/Machal/` handoff entrypoint.
  - Recorded the basic working method and required reading order.
  - Recorded the current presentation-data plan, architecture, approved asset paths, exclusions, work order, and verification matrix.
- User decisions captured:
  - Do not touch legacy data.
  - Use only approved current Skill/Effect assets.
  - Put neutral reusable presentation contracts under Core.
  - Keep gameplay classification and mapping under Ability.
  - Prioritize semantic categories and groups over variable-to-string formatting.
  - Complete the data layer before View work.
- Current source findings:
  - Current Skill/Effect assets are available under `Assets/Resources/skill/character/generated/` and `Assets/Resources/skill/json/`.
  - No Bless or Relic asset with the current serialized `effectEntries` field was found.
  - Existing Bless/Relic paths remain excluded and unchanged.
- Files created:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/basic-work-guide.md`
  - `AgentDocs/Machal/basic-work-guide-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- Existing files changed: none
- Verification completed:
  - All eight handoff documents exist and are readable as UTF-8.
  - The required task documents and approved source/asset paths exist.
  - English canonical documents have matching Korean `-ko` documents.
  - No trailing whitespace or tab characters were found.
  - All eight files are visible to Git as new untracked files and are not ignored.
- Not performed:
  - Staging, commit, and push
- Recommended next action:
  - Read all required documents, record the working-tree baseline, and create the current asset inventory before writing runtime code.

## 2026-08-08 — Architecture and Effect Normalization Contract Corrected

- Status: design documentation updated; implementation not started
- Correction to the previous entry:
  - Do not create or use `Assets/Scripts/Core/Presentation/`.
  - Shared neutral contracts now belong under `Assets/Scripts/Presentation/Content/Data/`.
  - Content-specific code belongs in a `Presentation/` child of each owning content path.
- Final planned content paths:
  - `Assets/Scripts/Ability/Effects/Presentation/`
  - `Assets/Scripts/Ability/Skills/Presentation/`
  - `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Presentation/`
  - `Assets/Scripts/Collection/Relic/Presentation/`
  - `Assets/Scripts/Actor/Character/Presentation/`
- Effect normalization decision:
  - Normalize each supported current Effect into an optional `Activation` plus one approved semantic outcome: `StatModifier`, `Heal`, `CooldownChange`, `Displacement`, `PeriodicDamage`, `SkillInvoke`, or `Control`.
  - The exact source-to-result table is now authoritative in the active task document.
  - Do not normalize ambiguous legacy `SkillEffectSO` fields; use only its authored description when present.
- View work remains deferred.
- Files changed:
  - `AgentDocs/Machal/basic-work-guide.md`
  - `AgentDocs/Machal/basic-work-guide-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- Verification pending:
  - English/Korean contract parity
  - Removal of active `Core/Presentation` path references
  - Whitespace and path checks

## 2026-08-08 — External Architecture Reference Incorporated

- Status: reference contract incorporated; implementation not started
- Source thread: `019fde12-0db0-7023-a89f-c29f73b31413`
- Added decisions:
  - Normalized structures are domain Presentation data, never `ViewData` or `UIData`.
  - Keep normalization separate from final string formatting.
  - Use general numeric concepts such as effect distance and effect range; keep action wording in authored descriptions.
  - Do not derive application count from interval and duration.
  - Parent skills show only nested-skill name and summary; nested details resolve independently.
  - The architecture reference does not authorize whole-project folder migration.
  - Future script moves must preserve `.cs.meta` GUIDs and unrelated work.
  - Future `Assets/Contents` JSON/generated-SO layout remains a separate task.
- Added the planned resolver-class table for all approved Effect normalization combinations.
- No runtime code, assets, prefabs, scenes, or existing script folders were changed.
- Verification pending:
  - Contract parity and path review after document edits
  - Whitespace check

## 2026-08-08 — Revised Design Documentation Verified

- Status: verified documentation unit; implementation not started
- Verification completed:
  - No active guide or task contract references the rejected `Assets/Scripts/Core/Presentation/` or broad `Assets/Scripts/Ability/Presentation/` paths.
  - Every planned parent ownership path exists in the current checkout.
  - All fourteen source-to-normalized-result rows are present in both English and Korean task contracts.
  - Resolver class combinations, compact display boundary, application-count rule, nested-skill contract, migration boundary, and SO/JSON boundary are present in both language versions.
  - No trailing whitespace or tab characters were found in `AgentDocs/Machal/`.
- Not performed:
  - Runtime implementation, folder creation under `Assets/Scripts`, asset changes, staging, commit, or push
- Recommended next action:
  - Create the current source inventory, then implement the smallest shared contract under `Assets/Scripts/Presentation/Content/Data/` only after user approval to begin code work.

## 2026-08-08 — Presentation Folder Layout Simplified

- Status: planning correction verified; implementation not started
- Correction to all earlier planned-path entries:
  - Shared neutral contracts belong directly under `Assets/Scripts/Presentation/`; do not add `Content/` or `Data/` below the root Presentation category for these contracts.
  - Do not add a `Presentation/` child to Ability Effects, Ability Skills, Blessings, Relic, or Character for this feature.
  - Separate passive content-owned types in each owner's `Data/` child.
  - Keep resolver and builder classes directly under the owning content path; do not add a feature-only `Resolvers/` child.
  - Use explicit names such as `EffectPresentationResolver`, `<EffectType>PresentationResolver`, and `SkillPresentationResolver` to group behavior.
- Revised first implementation paths:
  - Shared contracts: `Assets/Scripts/Presentation/`
  - Effect data: `Assets/Scripts/Ability/Effects/Data/`
  - Effect resolvers: `Assets/Scripts/Ability/Effects/`
  - Skill data: `Assets/Scripts/Ability/Skills/Data/`
  - Skill resolvers/builders: `Assets/Scripts/Ability/Skills/`
- Future adapter layout, still pending approved current data:
  - Bless data and resolver: `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Data/` and its owner root
  - Relic data and resolver: `Assets/Scripts/Collection/Relic/Data/` and its owner root
  - Character data and resolver: `Assets/Scripts/Actor/Character/Data/` and its owner root
- Unchanged decisions:
  - Legacy data remains excluded and untouched.
  - Effect normalization combinations remain authoritative.
  - Data setting and real-asset verification precede all View work.
- Files changed:
  - `AgentDocs/Machal/basic-work-guide.md`
  - `AgentDocs/Machal/basic-work-guide-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- Verification completed:
  - Active guides and task contracts contain no planned `Presentation/Content`, content-domain `Presentation/`, or feature-only `Resolvers/` path.
  - Both task contracts name the exact shared, Effect, Skill, Bless, Relic, and Character data paths.
  - All fourteen authoritative Effect normalization cases remain present in both languages.
  - Every owner parent path exists in the checkout; no planned script folder was created.
  - No trailing whitespace or tab characters were found in `AgentDocs/Machal/`.
- Not performed:
  - Runtime implementation, script-folder creation, asset changes, staging, commit, or push
- Recommended next action:
  - Inventory the approved current Skill and Effect sources, then implement the smallest shared contracts directly under `Assets/Scripts/Presentation/` after code-work approval.

## 2026-08-08 — Stage 1 Source and Asset Inventory Completed

- Status: Stage 1 complete; data-layer code not started
- Scope performed:
  - Confirmed every source and approved asset path named by the active task contract.
  - Traced the current `EquipmentSkillSO` source graph and `EquipmentSkillResolver` runtime output.
  - Traced `EffectSO`, `EffectEntrySO`, all thirteen current `EffectConfig` classes, `EffectResolver`, and runtime config use sites.
  - Counted approved JSON, Skill SOs, hit SOs, Effect SOs, EffectEntry SOs, and reachable Effect references.
  - Separated authoring JSON declarations from runtime-reachable SO data.
  - Added the eight-stage implementation plan and made the inventory a required handoff document.
- Material findings:
  - Approved paths contain 58 `EquipmentSkillSO`, 20 `EffectSO`, and 20 `EffectEntrySO` assets.
  - Eighteen Effect entries are reachable, all through Strategic Skill hit assets.
  - Character JSON declares 27 Effects, but approved Character hit SOs contain zero non-null EffectEntry references.
  - Six Character JSON files have no matching primary Skill asset.
  - Two Character EffectEntry assets are unreferenced.
  - Approved assets cover five of the thirteen current config types; eight types remain source-level only.
  - No non-null nested Skill reference exists in the approved paths.
- Decisions:
  - Runtime resolver behavior and reachable current SO references have authority over authoring JSON.
  - JSON-only values retain authoring provenance and must not be displayed as active gameplay values.
  - This task will record JSON/SO mismatches but will not repair or migrate them.
- Files created:
  - `AgentDocs/Machal/ability-content-presentation-inventory.md`
  - `AgentDocs/Machal/ability-content-presentation-inventory-ko.md`
- Files changed:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- Verification evidence:
  - All required task source and approved asset paths exist.
  - Approved Skill/Effect source and asset paths have no Git changes.
  - YAML script GUID classification and reference reachability counts were checked independently.
  - English and Korean inventory documents contain matching counts and thirteen config rows.
  - No trailing whitespace or tab characters were found in `AgentDocs/Machal/`.
- Blocker:
  - `AgentDocs/code-writing-rules.md` is missing. Stage 2 script work cannot begin until it is restored or supplied.
  - `AgentDocs/task-start-documentation-prompt.md` is also missing, so the prescribed documentation handoff format is unavailable.
- Not performed:
  - Script implementation, asset repair, migration, prefab or Scene changes, staging, commit, or push
- Recommended next action:
  - Restore or supply `AgentDocs/code-writing-rules.md`, then start Stage 2 with the smallest reachable shared-contract placeholder under `Assets/Scripts/Presentation/`.

## 2026-08-09 — Single Effect Resolver Design Adopted; Stage 2 Blocked

- Status: design corrected; Stage 2 code not started
- User decision:
  - Effect Config mappings are small enough that Config-specific resolver classes and an interface are unnecessary.
  - Use one public `EffectPresentationResolver` with internal Config switching.
  - Add private methods only for repeated normalized-result construction, not one method or class hierarchy solely to mirror each Config type.
  - Keep Config classes independent of Presentation contracts; do not add `ToPresentationData()` methods to them.
- Removed from the active plan:
  - `IEffectConfigPresentationResolver`
  - `<EffectType>PresentationResolver` classes
  - The per-Config resolver-class table
- Remaining next implementation sequence:
  - Stage 2 shared neutral contracts
  - Stage 3 Effect normalized data and single resolver entrypoint
  - Stage 4 Config mapping branches inside the single resolver
  - Stage 5 Skill composition
  - Stage 6 approved-asset validation
  - Stage 7 confirmed-current content adapters
  - Stage 8 data-layer approval and later UI handoff
- Blocker verified:
  - `AgentDocs/code-writing-rules.md` remains missing.
  - `AgentDocs/task-start-documentation-prompt.md` remains missing.
  - The Machal start contract prohibits implementation while a required path is missing.
- Files changed:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- Not performed:
  - Script creation, placeholder insertion, compilation, asset changes, staging, commit, or push
- Recommended next action:
  - Restore or explicitly replace the missing code-writing guide, read it fully, then proceed with Stage 2.

## 2026-08-09 — UI Prefab Preparation and Stage 3 Design Prepared

- Status: user-side UI prefab preparation contract complete; Stage 3 design complete; script implementation not started
- User work enabled:
  - Prepare one generic content-information view and reusable group, entry, and tag prefabs.
  - Preserve the exact future binding object names documented in the UI preparation guide.
  - Build flexible layout and styling without adding View scripts, concrete SO references, Scene bindings, or gameplay-value interpretation.
- UI findings:
  - `Assets/Prefabs/UIWidget/UITooltipWidget.prefab` is a compact single-string tooltip and is not sufficient by itself for semantic groups and label/value rows.
  - Existing AutoBind behavior requires exact prefix-plus-field child names.
  - The prepared hierarchy supports identity, authored description, dynamic groups, compact combined values, fallback status, and separate nested-content navigation.
- Stage 3 preparation:
  - Reduced the initial Effect data scope to `Assets/Scripts/Ability/Effects/Data/EffectPresentationData.cs` and `Assets/Scripts/Ability/Effects/EffectPresentationResolver.cs`.
  - Kept the initial Activation, constraint, and typed outcome records in one data file until independent ownership justifies splitting.
  - Defined the single resolver surface, null and unsupported fallback behavior, provenance boundaries, Stage 4 branch order, and verification exit criteria.
  - Corrected ambiguous legacy `SkillEffectSO` fallback ownership from Stage 4 Effect mapping to Stage 5 Skill composition.
- Files created:
  - `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`
  - `AgentDocs/Machal/ability-content-ui-prefab-preparation-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`
  - `AgentDocs/Machal/ability-content-presentation-stage3-preparation-ko.md`
- Files changed:
  - `AgentDocs/Machal/README.md`
  - `AgentDocs/Machal/README-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-task.md`
  - `AgentDocs/Machal/ability-content-presentation-task-ko.md`
  - `AgentDocs/Machal/ability-content-presentation-log.md`
  - `AgentDocs/Machal/ability-content-presentation-log-ko.md`
- Blocker unchanged:
  - `AgentDocs/code-writing-rules.md` remains missing, so Stage 2 and Stage 3 script work cannot start under the current work contract.
  - `AgentDocs/task-start-documentation-prompt.md` remains missing, so the prescribed documentation handoff format is unavailable.
- Not performed:
  - Script, prefab, Scene, asset, or AutoBind changes; compilation; staging; commit; push
- Recommended parallel next actions:
  - User: prepare the unbound generic prefabs from `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`.
  - Agent: after the missing guide is restored, implement and verify Stage 2, then execute the prepared Stage 3 contract.

## 2026-08-09 — Root Workflow Guides Restored

- Status: missing-guide blocker cleared; Stage 2 implementation not started
- Restored and verified paths:
  - `AgentDocs/code-writing-rules.md`
  - `AgentDocs/code-writing-rules-ko.md`
  - `AgentDocs/task-start-documentation-prompt.md`
  - `AgentDocs/task-start-documentation-prompt-ko.md`
- Active-state corrections:
  - Updated the README and task status to show that Stage 2 may start under the restored placeholder-first rules.
  - Preserved earlier log entries as historical evidence of the blocker at those times.
- Reusable workflow update:
  - Documentation handoffs now separate user-prepared or externally changed paths from paths changed by the reporting agent.
- Verification:
  - All four root guide paths exist and read as strict UTF-8.
  - English and Korean guide pairs have matching structure.
  - No trailing whitespace, tabs, or replacement characters were found in the updated files.
- Not performed:
  - Runtime code, prefab, Scene, asset, or AutoBind changes; compilation; staging; commit; push
- Recommended next action:
  - Read the restored code-writing guide, then begin Stage 2 with the smallest reachable placeholder-backed shared contract.

## 2026-08-09 — Stage 2 Shared Contracts Completed

- Status: Stage 2 complete; Stage 3 design prepared; Stage 3 scripts not started
- Implemented under `Assets/Scripts/Presentation/`:
  - `PresentationIdentityData` for content ID, display name, and optional icon
  - `PresentationContext` for Preview or Runtime resolution mode
  - `PresentationProvenanceData` for authored asset, runtime-resolved, authoring-source, and description-fallback origins
  - `PresentationValueData` for numeric or semantic-token values with explicit units and optional value-level provenance
  - `PresentationEntryData` for one semantic key with one or more compact values and optional detail-content navigation
  - `PresentationGroupData` for dynamic semantic sections
  - `ContentPresentationData` for identity, authored description, classifications, groups, provenance, and supported state
- Placeholder-first evidence:
  - Added and invoked temporary `ContentPresentationData.CreatePlaceholder()`.
  - Observed `[PLACEHOLDER] ContentPresentationData.CreatePlaceholder called; Stage 2 contract construction pending.` and `PLACEHOLDER_REACHED` in the smoke harness.
  - Removed the placeholder method and log after replacing it with the final constructor-based contract.
- Final verification:
  - The isolated C# smoke build completed successfully.
  - Final output was `STAGE2_CONTRACT_SMOKE_OK`.
  - The test constructed two values in one entry (`Percent` and `Seconds`), one group, runtime provenance, and both Preview and Runtime contexts.
  - Shared contracts reference no concrete Skill, Effect, Bless, Relic, or Character SO types.
  - Every new `.cs` file has a paired `.cs.meta` file.
- Scope boundary:
  - No Effect normalization, Config mapping, View, prefab, Scene, asset, or AutoBind implementation was added.
  - No staging, commit, or push was performed.
- Next action:
  - Implement the prepared Stage 3 Effect model and single `EffectPresentationResolver` entrypoint in a new bounded work unit.

## 2026-08-10 — User Prefab Skeleton Layout Completed

- Status: the four user-prepared, unbound content-information prefab skeletons now have their reusable layout and required UI components; runtime binding remains deferred
- User-prepared or external paths:
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoView.prefab` and `.meta`
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoGroup.prefab` and `.meta`
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoEntry.prefab` and `.meta`
  - `Assets/Prefabs/UI/Fixed/Content/UIContentInfoTag.prefab` and `.meta`
  - The skeletons included the `Info_*` binding names and sample nested Tag and Group instances.
- Agent-applied prefab changes:
  - View: fixed root size, header/body layout, icon/name/tag regions, vertical ScrollRect with masked viewport, dynamic `Info_GroupRoot`, and inactive fallback status.
  - Group: vertical title/description/entry layout, inactive optional description, and reusable entry container.
  - Entry: horizontal label/value/action layout, reusable text sizing, and an inactive detail action with Image and Button components.
  - Tag: background Image, horizontal content layout, and TMP text with a minimum-size LayoutElement.
  - Prefab Mode layout recalculation values were saved for Entry, Group, Tag, and View.
- Verification:
  - Unity loaded and saved all four prefabs through `PrefabUtility` using a temporary one-shot Editor builder.
  - The builder and its `.meta` file were removed after completion; no temporary implementation remains.
  - Direct Unity Prefab Mode inspection confirmed each hierarchy, component set, default inactive state, and the View ScrollRect content/viewport wiring.
  - Removed redundant nested `ContentSizeFitter` components from Entry, Group, `Group_EntryRoot`, and Tag after Unity exposed the parent-layout conflict; only the ScrollRect content owner `Info_GroupRoot` retains preferred-height fitting.
  - A second Unity inspection confirmed that the nested layout warning is gone.
  - Unity Console ended with 0 errors and 0 warnings.
  - Prefab GUIDs remain unchanged: Entry `47914a9dc9448b441824302050309731`, Group `369f8a836fa3e8e4b901db8988516493`, Tag `0a6b2b3bbf91f3f47832f7c2efd1f313`, View `e23588acb90e20c4a891fd2f25b2c4bd`.
  - An earlier domain reload surfaced existing `AutoBindEditorUtility.FindComponent` errors for `GameObject` fields outside this prefab scope; after refresh, the current Console is clean.
- Scope boundary:
  - No View scripts, concrete SO references, Scene bindings, AutoBind components, gameplay-value formatting, final localization, or final visual styling were added.
  - No staging, commit, or push was performed.
- Next action:
  - Continue with the prepared Stage 3 data work; bind and style these prefabs only after the data layer is verified and approved.

## 2026-08-11 — Stage 3 Effect Model and Fallback Entrypoint Completed

- Status: Stage 3 complete; Stage 4 Config mapping branches remain pending.
- Design correction approved by the user:
  - `OnHit`, `OnHeal`, and `OnAttack` remain separate activation events because they describe when an Effect starts.
  - Damage and Heal now share `EffectOutcomeKind.HealthChange` because they describe direction on the same health-value axis.
  - `HealthChangeKind` preserves Damage versus Heal, and `HealthChangeApplicationKind` preserves Instant versus Periodic behavior.
  - This supersedes earlier planning text that listed separate `Heal` and `PeriodicDamage` outcomes.
- Created files:
  - `Assets/Scripts/Ability/Effects/Data.meta`, GUID `ae91860cc1f542ae83a4898b96315a20`
  - `Assets/Scripts/Ability/Effects/Data/EffectPresentationData.cs` and `.meta`, script GUID `7e049d48d2a54edeaff2ad8dbf22924c`
  - `Assets/Scripts/Ability/Effects/EffectPresentationResolver.cs` and `.meta`, script GUID `ad09cd55d09340a39b28a6cb9a0b8995`
- Implementation:
  - Added typed Activation, entry constraints, outcome kinds, HealthChange amount/basis/rate data, and the remaining semantic outcome payloads.
  - Added one public `EffectPresentationResolver.Resolve(EffectEntrySO, PresentationContext)` entrypoint with an internal Config switch and no Config-specific resolver hierarchy.
  - Stage 3 deliberately implements only null, unsupported, and authored-description fallback behavior. Config branches remain Stage 4 work.
  - Entry Duration is exposed only for Timed and CombatTimed lifetimes; MaxApplyCount comes only from the real field. `ValueOverride` and upgrade modifiers are not exposed.
- Placeholder-first evidence:
  - The temporary harness reached `[PLACEHOLDER] EffectPresentationResolver.Resolve called; Stage 3 fallback behavior pending.` and then `PLACEHOLDER_CALL_COMPLETED`.
  - The placeholder was removed after deterministic fallback behavior replaced it.
- Final verification:
  - The isolated smoke result was `STAGE3_EFFECT_PRESENTATION_SMOKE_OK`.
  - Covered null entry, null Effect, fallback/provenance, timed and instant constraints, Seconds and Count units, separate OnHit/OnHeal triggers, and one HealthChange outcome with distinct Damage/Heal directions.
  - Unity regenerated `Library/ScriptAssemblies/Assembly-CSharp.dll` at `2026-08-11 02:15:43` with size `1,255,424` bytes and included both new source files in `Assembly-CSharp.csproj`.
  - Domain reload again exposed the existing out-of-scope `AutoBindEditorUtility` errors for `GameObject` fields; the new scripts had no compile errors, and the Console was cleared to 0 errors and 0 warnings after verification.
- Scope boundary:
  - No Config, SO asset, legacy asset, View, prefab, Scene, or AutoBind behavior was changed in this unit.
  - No staging, commit, or push was performed.
- Next action:
  - Implement Stage 4 Config mappings one branch at a time, beginning with the approved linked-asset batch, and verify each mapping without changing the public contract.

## 2026-08-11 — Stage 4 Effect Config Mappings and User Self-Test Completed

- Status: Stage 4 complete; Stage 5 Skill composition is next.
- Production implementation:
  - Added all thirteen current Config branches to the single `Assets/Scripts/Ability/Effects/EffectPresentationResolver.cs`.
  - Preserved the Stage 3 public contract and added only outcome-grouped private construction helpers.
  - Normalized all activation chances to `0..100 Percent`, while keeping direct scaling ratios and percentage-point values distinct.
  - Mapped Heal and periodic Damage through the shared `HealthChange` outcome.
  - Added deterministic fallback for null invoked Skill, unsupported ChanceOnHit Multiply, and OnHitTimed duration-stat max-set cases.
- Runtime-accuracy decisions:
  - `ChanceOnHealStatModifier.ValueType` is ignored by the runtime, so the active Presentation operation is Flat.
  - Heal is always clamped by `CharacterDamageService`; the unused `ClampToMaxHp` Config flag is not presented.
  - Unused `ChanceOnHitSkill.RangeOverride` is excluded.
  - The critical requirement is preserved, but the current `EffectManager` passes `true` instead of the actual hit-critical result; this gameplay wiring remains unresolved and unchanged.
  - Distance displacement collapses non-Pull directions to Push to match the current runtime.
- User test tool created:
  - `Assets/Editor/tools/effect/EffectPresentationStage4SelfTest.cs`
  - `Assets/Editor/tools/effect/EffectPresentationStage4SelfTest.cs.meta`, GUID `32174dd94bfa44ff9f2b77e939a7644e`
  - Full test menu: `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`
  - Selected-entry log menu: `Assets > ProjectBS > Presentation > Log Selected Effect Entry`
- Verification:
  - `dotnet build Assembly-CSharp.csproj --no-restore` completed with 0 errors and 35 pre-existing warnings.
  - Unity regenerated `Library/ScriptAssemblies/Assembly-CSharp.dll` at `2026-08-11 03:05:25`, size `1,260,032` bytes.
  - Final Unity self-test output: `[EffectPresentationStage4SelfTest] PASS`, `Synthetic config mappings: 13`, `Approved EffectEntry assets: 20`.
  - Final Unity Console: one PASS log, zero warnings, zero errors.
- Documentation:
  - Added `AgentDocs/Machal/ability-content-presentation-stage4-verification.md` and its Korean mirror with the exact test steps and runtime gaps.
- Scope boundary:
  - No Config, runtime gameplay class, SO asset, legacy asset, prefab, Scene, or AutoBind behavior was changed.
  - No staging, commit, or push was performed.
- Next action:
  - Begin Stage 5 Skill composition, reuse normalized Effect results, and keep the player-facing prefab binding deferred until the composed data contract is approved.

## 2026-08-11 — Stages 5-8 Code Completed; Unity Handoff Pending

- Status: requested code work complete; nested Skill traversal deferred; user-owned Unity work pending.
- Scope completed:
  - Added Skill classification, typed composition, Preview/Runtime provenance, semantic grouping, and description-only legacy `SkillEffectSO` fallback.
  - Added an approved-path Skill validation matrix, selected-Skill log, and interactive Editor preview window.
  - Added current-definition Character, Bless, and Relic adapters without reading or changing excluded legacy assets.
  - Added the compact text formatter and generic content View, Group, Entry, and Tag scripts.
- User decision:
  - Do not implement nested Skill traversal or expanded nested detail in this unit.
  - Do not let the agent use Unity Editor features. Stop at the required prefab/Unity handoff and let the user run validation.
- Verification:
  - `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`: 0 errors.
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 0 warnings and 0 errors on the final run; produced both `Assembly-CSharp` and `Assembly-CSharp-Editor`.
  - All 22 new `.meta` GUIDs appear exactly once under `Assets/`.
  - Scoped `git diff --check` passed and no temporary placeholder remains.
  - Unity Editor was not opened or controlled.
- Pending:
  - User attaches the four generic View scripts and serialized references to the prepared `UIContentInfo*` prefabs.
  - User later runs the Skill validation menus against real approved Skill assets.
  - Character/Bless/Relic approved current asset paths and asset-level verification remain pending.
  - Scene integration, AutoBind, final localization, and a content-specific presenter remain separate follow-up work.
- Completion reference:
  - `AgentDocs/Machal/ability-content-presentation-stage5-8-completion.md`

## 2026-08-11 — UIContentInfo Hierarchy AutoBind Correction

- Status: source correction complete; user-owned Unity refresh and prefab save pending.
- User correction:
  - The four View components had been attached to the prefabs, but their serialized hierarchy fields did not use the project AutoBind system.
- Changed scripts:
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoView.cs`
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoGroupView.cs`
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoEntryView.cs`
  - `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoTagView.cs`
- Implementation:
  - Changed the main View to inherit `UIView` and the three reusable child Views to inherit `UIComponent`.
  - Added `AutoBindPrefix` values matching the existing `Info_*`, `Group_*`, `Entry_*`, and `Tag_*` hierarchy names.
  - Added `AutoBind` to hierarchy component fields only.
  - Kept `tagPrefab`, `groupPrefab`, and `entryPrefab` manual because the current AutoBind utility resolves child components, not prefab asset references.
- Verification:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal` completed with 0 errors; 191 existing project warnings were reported.
  - Static prefab inspection confirmed the four script GUIDs are attached and every required AutoBind target object name exists.
  - Before Unity refresh, serialized hierarchy and template-prefab references were still null in the prefab YAML.
  - Unity Editor was not opened or controlled by the agent.
- User next action:
  - Allow the script reload, open or validate each prefab, assign the three template-prefab asset fields manually, save all four prefabs, and report any Console error or remaining `None` hierarchy field.

## 2026-08-11 — Stage 6 Null Effect Slot Validation Correction

- Status: source correction complete; user Unity rerun pending.
- User result before correction:
  - 58 approved Skills resolved.
  - The validation reported 18 supported and 10 unsupported Effect records with 10 failures.
- Root cause:
  - All ten reported records came from null `EffectEntrySO` elements serialized as `{fileID: 0}` in nine current Cast assets.
  - `skill.character.military_officer.1.passive_1.unyielding_will` contains two null slots, which explains the duplicated failure path.
  - No unsupported concrete Effect type was found in this failure set.
  - Stage 1 had already classified these placeholders as inactive Effects, and gameplay `EffectResolver.ResolveEntries` skips null entries.
- Changed paths:
  - `Assets/Scripts/Ability/Skills/SkillPresentationResolver.cs`
  - `Assets/Editor/tools/skill/SkillPresentationStage6Validation.cs`
- Correction:
  - Skill Presentation now skips null Effect slots before invoking `EffectPresentationResolver`.
  - The validation matrix records ignored null slots separately instead of classifying them as unsupported Effects.
- Verification:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 0 errors and 191 existing project warnings.
  - No Skill, Cast, Effect, JSON, or legacy asset was modified.
  - Unity Editor was not opened or controlled by the agent.
- Expected user rerun:
  - `Supported / description-only / unsupported Effects: 18 / 0 / 0`
  - `Ignored null EffectEntry slots: 10`
  - `Failures: 0`

## 2026-08-11 — Stage 6 User PASS and Skill UI Preview Tool

- Status: approved Skill data validation complete; user visual UI preview pending.
- User Unity result:
  - `[SkillPresentationStage6Validation] PASS`
  - 58 approved unique Skills and 58 resolved Skills
  - No hit / no Effect / one Effect / multiple Effects: `7 / 42 / 14 / 2`
  - Supported / description-only / unsupported Effects: `18 / 0 / 0`
  - Ignored null EffectEntry slots: `10`
  - Ratio / Percent values: `120 / 102`
  - Failures: `0`
- Added user-run Editor tool:
  - `Assets/Editor/tools/skill/SkillPresentationUIPreviewTool.cs` and `.meta`
  - Validates seventeen serialized references across the View, Group, Entry, and Tag prefabs.
  - In Play Mode, creates a temporary `DontSave` overlay Canvas and binds the selected approved Skill to `UIContentInfoView`.
  - Supports authored Preview values and Runtime level-1 values.
  - Does not save a Scene or change a prefab.
- Verification:
  - The new source was explicitly included in `Assembly-CSharp-Editor.csproj` for the compile check, then the generated project file was restored.
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 0 errors and 156 existing project warnings.
  - Script GUID `7df02ec2672b45d0ad485ba3956266ff` was assigned to the new tool.
  - Unity visual preview was not run by the agent.
- User next action:
  - Run `Tools > ProjectBS > Presentation > Validate Content UI Prefab Bindings`.
  - Select an approved Skill, enter Play Mode, and run either authored or Runtime UI preview from the Asset menu.

## 2026-08-11 — Existing-UI Presenter, Semantic Filtering, and Scroll Correction

- Status: source implementation and non-Unity compilation complete; user Unity validation pending.
- User correction:
  - Do not create a temporary Canvas or UI from an Asset/Tools menu.
  - Bind a selected `EquipmentSkillSO` into the user's existing `UIContentInfoView` through a component context menu.
  - Remove meaningless serialized defaults and unbounded sentinels from the visible groups.
  - Correct the non-responsive `Info_GroupRoot` ScrollRect.
- Changed paths:
  - Added `Assets/Scripts/Ability/Skills/UI/SkillContentInfoPresenter.cs` and `.meta`.
  - Changed `Assets/Scripts/Ability/Skills/SkillPresentationGroupResolver.cs`.
  - Changed `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoView.cs`.
  - Removed `Assets/Editor/tools/skill/SkillPresentationUIPreviewTool.cs` and `.meta`.
- Implementation:
  - `Build Presentation` is available on `SkillContentInfoPresenter` in Play Mode and binds authored or runtime-resolved values into the assigned existing View.
  - The presenter creates no Canvas, prefab instance, or Scene object. It warns when the active Scene lacks an `EventSystem` required for ScrollRect input.
  - Skill grouping omits zero time/distance/damage fields, count/scale defaults, `999` unbounded sentinels, default-disabled combat flags, and empty damage/behavior groups.
  - `UIContentInfoView` deactivates old generated children and forces the content/layout/ScrollRect rebuild before resetting to the top.
- Verification:
  - The new presenter was explicitly included in the generated main project file only for the compile check; both generated project files were restored afterward.
  - `dotnet build Assembly-CSharp.csproj --no-restore`: 0 errors and 35 existing project warnings.
  - Unity Editor was not opened or controlled by the agent.
- User next action:
  - Let Unity import the new component, attach it to the existing UI, assign the Skill and View, enter Play Mode, and run the component menu `Build Presentation`.
  - Confirm scroll-wheel/drag input and confirm that zero/default/`999` rows are absent.

## 2026-08-11 — Raw Skill Tool and Filtered Player UI Split

- Status: source correction and non-Unity compilation complete; user Unity confirmation pending.
- User correction:
  - The Skill Presentation inspection tool must continue showing raw/default source-visible values such as `0` and `999`.
  - Default-value filtering applies only to the player-facing content information UI.
- Correction:
  - `SkillPresentationGroupResolver.Resolve()` now uses the complete, unfiltered grouping behavior used by existing Editor tools and validation.
  - Added `ResolveForPlayerDisplay()` as the explicit filtered path.
  - `SkillContentInfoPresenter` now calls only `ResolveForPlayerDisplay()`.
- Verification:
  - `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:minimal`: 0 errors and 35 existing project warnings.
  - Unity Editor was not opened or controlled by the agent.

## 2026-08-11 — Contract Re-evaluation and Source-Faithful Group Correction

- Status: source correction and static compile complete; user Unity validation pending.
- User correction:
  - Restore the approved seven Effect Outcomes: StatModifier, Heal, CooldownChange, Displacement, PeriodicDamage, SkillInvoke, and Control.
  - Keep optional Activation attached to the normalized Outcome.
  - Build Skill display data from actual `Assets/Resources/skill/json/` fields without numbered or invented group keys.
  - Map one source field to one Entry; labels may translate meaning, but different source values must not be combined or replaced.
- Evaluation:
  - All 20 strategic Skill JSON files were parsed as UTF-8.
  - The files contain no literal `tags` property. Classification is carried by `skillType`, `skillComponentType`, `brainMeta.category`, `brainMeta.targetType`, `brainMeta.tacticalNeed`, `targetLayerMask`, `effectType`, `categoryType`, and `lifetimeType`.
  - Prior display keys `Skill.Hit.<number>.Damage`, `Skill.Effect.*.<number>`, `Behavior`, `CountAndScale`, and `SizeAndLifetime` did not belong to the source JSON or approved normalization contract.
  - The intermediate shared `HealthChange` Outcome conflicted with the latest user-restated seven-outcome contract.
- Correction:
  - Restored typed `HealPresentationData` and `PeriodicDamagePresentationData`.
  - Skill groups use source object keys `cast`, `baseProfile`, `move`, `hits`, and `spawnSkill` when applicable.
  - Effect groups use the normalized Outcome kind and preserve the real Effect ID in `PresentationGroupData.SourceContentId`.
  - Every corrected Skill/Effect entry contains at most one value.
  - Added `Assets/Resources/string/presentation_string.csv` for source-key and normalized-component label/token localization.
  - Added source-faithful grouping assertions to `SkillPresentationStage6Validation`.
  - Added `AgentDocs/Machal/ability-content-presentation-contract-evaluation.md` and Korean mirror.
- Verification:
  - `presentation_string.csv`: 132 data rows, duplicate key count 0.
  - New CSV meta GUID occurs exactly once.
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 0 errors and 191 existing project warnings.
  - Unity Editor was not opened or controlled by the agent.
- User next action:
  - Run the Effect self-test and Skill asset validation in Unity.
  - Run `Build Presentation` and confirm separate Effect Range and Duration rows, normalized Outcome group titles, source classification tags, and no numbered group titles.

## 2026-08-11 — Optional Description Localization and Strategic Key Ownership

- Status: source correction and static verification complete; user Unity localization check pending.
- Root cause:
  - Presentation already reached `StringManager`, but a missing key returned the key itself, so strings ending in `.desc` appeared as authored UI text.
  - Current strategic Skill IDs are `skill.strategic.*`, while the actual localized descriptions are owned by `item.strategic.*` in `Assets/Resources/string/string_table.csv`.
  - Current Effect IDs have no matching `desc` rows; structured Effect groups remain valid, but their missing description keys must not be displayed.
- Correction:
  - Skill descriptions use optional exact-key lookup and then the `item.strategic.*.desc` fallback only for strategic Skills.
  - Effect, Relic, and Bless Presentation descriptions use optional `StringManager` lookup; missing rows yield an empty description instead of a raw key.
  - No localization CSV, Skill, Effect, Bless, Relic, or legacy asset was changed.
- Verification:
  - All 20 approved strategic Skill IDs have a matching `item.strategic.*.desc` row.
  - `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:minimal`: 0 errors and 35 existing project warnings.
  - Unity Editor was not opened or controlled by the agent.

## 2026-08-11 — Documentation Manager Contract Consistency Review

- Status: active bilingual documents reconciled with the authoritative seven-outcome and source-faithful grouping contract; user Unity validation remains pending.
- Corrections:
  - Removed the obsolete allowance for invented Effect display subgroups such as `Scaling`, `Persistence`, and `Constraints`; Activation and entry constraints remain separate entries in the normalized Outcome group.
  - Replaced the stale semantic Skill-group summary with the preserved JSON object keys `cast`, `baseProfile`, `move`, `hits`, and `spawnSkill`.
  - Replaced the UI guide's combined-value allowance with one source value per row.
  - Clarified that Stage 4 Console inspection names typed fields, not UI group keys.
- Verification:
  - All listed English canonical and Korean mirror documents retain matching line and heading counts.
  - Active contract documents contain no stale `HealthChange` or combined/numbered group rule except explicit supersession and removal notes in the contract evaluation.
  - All 20 strategic Skill JSON files parse with strict UTF-8; current `presentation_string.csv` contains 132 data rows and zero duplicate `main_key`/`sub_key` pairs.
  - Documentation files pass strict UTF-8 and trailing-whitespace/tab checks.
  - Historical log entries remain unchanged and are superseded by the later contract re-evaluation entry.
  - No runtime source, asset, prefab, Scene, build, Unity Editor, staging, commit, or push action was performed in this review.

## 2026-08-11 — Authored Numeric Fidelity Correction

- Status: implementation and static verification complete; user Unity rerun pending.
- Correction:
  - Authored Presentation values now preserve their source number and source unit instead of applying Presentation-side Clamp, minimum substitution, or Ratio-to-Percent conversion.
  - `Chance` remains a Ratio and `ChancePercent` remains a Percent. The Formatter may render a Ratio as a percentage without changing the normalized data.
  - Skill Preview preserves authored movement speed, damage, attack scaling, and max-hit values. Runtime presentation continues using values produced by the runtime stat resolver and marks them with runtime provenance.
  - The Effect self-test now verifies ratio-based activation and the authored poison interval directly.
- Verification:
  - Static Editor assembly build completed with 0 errors and 191 existing warnings.
  - Unity Editor validation was not run by the agent and the previous Unity PASS predates this correction.

## 2026-08-11 — Documentation Manager Numeric-Fidelity and UI-Ownership Review

- Status: active bilingual guidance reconciled; current Unity reruns and two UI decisions remain pending.
- Corrections:
  - Replaced canonicalized-value wording with preservation of the authored number, unit, and provenance. Ratio-to-percent conversion is formatter-only and does not mutate normalized data.
  - Marked the earlier Effect and 58-Skill Unity PASS results as pre-correction history rather than current verification.
  - Kept concrete `EquipmentSkillSO` and `Build Presentation` ownership in `SkillContentInfoPresenter`; moving them into neutral `UIContentInfoView` requires an explicit ownership decision.
  - Recorded that the Viewport has no raycastable `Graphic`; a transparent raycast-target `Image` is conditional on the user's Unity wheel/drag result.
- Verification:
  - Static prefab YAML confirms the Viewport contains `RectTransform` and `RectMask2D` but no `Graphic` component.
  - The previously reported Editor assembly build remains 0 errors and 191 existing warnings; no build or Unity Editor execution was repeated by the documentation manager.
  - No runtime source, asset, prefab, Scene, staging, commit, or push action was performed in this review.

## 2026-08-11 — Skill Semantic Regrouping and Special-Effect Correction

- Status: code and active bilingual contract documents updated; static build and user Unity rerun recorded separately.
- Decision:
  - Retain the seven typed Effect Outcomes as the internal normalization model.
  - Replace one-group-per-Effect Skill display with five aggregate role groups: `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, and `LinkedSkill`.
  - Preserve one source field per Entry and one Value per Entry. Group aggregation does not merge, derive, clamp, or replace source data.
- Special-effect mapping:
  - `OnHitTimedStatModifierEffectConfig` targeting `StunDuration` or `RootDuration` now normalizes as `Activation(OnHit + Chance) + Control(Stun/Root + duration)` because runtime applies `config.Value` as the max-set control timer.
  - Taunt remains `Control`; knockback and pull remain `Displacement`, preserving direction and Force or Distance according to the concrete Config.
  - `Control` and `Displacement` route to `SpecialEffect`; `SkillInvoke` routes to `LinkedSkill`.
- Validation changes:
  - Skill asset validation allows only the five semantic Skill group keys, rejects duplicates, and verifies normalized Entry routing.
  - Effect synthetic validation adds Stun and Root cases. Expected output is `Synthetic mapping cases: 15` across 13 Config classes.
  - Group and token labels are managed by `Assets/Resources/string/presentation_string.csv`, including the five Skill groups plus Stun and Root.
- Static verification:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 0 errors and 156 existing project warnings.
  - `presentation_string.csv`: 140 data rows, zero case-insensitive duplicate key pairs, and exactly one row for each new group/control key.
  - All 20 strategic Skill JSON files parse as strict UTF-8 JSON.
  - Unity Editor was not opened or controlled by the agent; Effect and Skill validation must be rerun by the user because the grouping contract changed.
- Supersession:
  - This entry supersedes earlier active rules that grouped Skill rows by JSON object names or created a visible group for every Effect Outcome. JSON field provenance and the typed Effect Outcome remain preserved below the new Skill-level aggregation.

## 2026-08-11 — Documentation Manager Semantic-Regrouping Reconciliation

- Status: active bilingual guides reconciled with the five-group Skill display contract; user Unity reruns remain pending.
- Reconciliation:
  - Preserved the seven typed Effect Outcomes as internal normalization while making `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, and `LinkedSkill` the only approved Skill UI groups.
  - Preserved one source field per Entry and one Value per Entry across aggregation.
  - Recorded Stun and Root as source-backed `Control` special effects, Taunt as `Control`, and knockback/pull as source-backed `Displacement`.
  - Updated active status and completion evidence to the latest static build with 0 errors and 156 existing warnings; the earlier Unity PASS remains historical.
- Verification:
  - Active guidance contains no current rule that groups Skill display by JSON object key or creates one UI group per Effect.
  - `presentation_string.csv` contains 140 data rows, zero case-insensitive duplicate key pairs, and one row for each five group keys plus Stun and Root.
  - All 20 strategic Skill JSON files parse as strict UTF-8 JSON.
  - No runtime source, localization asset, prefab, Scene, build, Unity Editor, staging, commit, or push action was performed by the documentation manager.

## 2026-08-12 — StringManager-backed Player Display Catalog

- Status: implementation and static verification complete; user Unity validation pending.
- Source review:
  - Re-extracted all JSON property paths from the 20 current files under `Assets/Resources/skill/json/`.
  - Confirmed current categorical values for Skill type, component, category, target, tactical need, targeting, arrangement, move, DamageType, Effect type, and Stat type.
- Implementation:
  - Added an explicit player display allowlist for Group, Entry, and Tag keys.
  - Added canonical StringManager keys for Group/Entry/Tag labels, contextual enum replacement words, Stat words, and Damage/Control/Displacement value formats.
  - Kept existing Skill/Bless/Relic name and description paths and their established fallback behavior unchanged.
  - Kept raw/default values and raw keys in inspection output; player UI alone uses strict catalog lookup and conditional filtering.
  - Added player-display filtering for Bless/Relic Effect composition without touching legacy assets.
- Documentation:
  - Added `AgentDocs/Machal/ability-content-presentation-display-catalog.md` and Korean mirror with the complete displayed/internal field list and maintenance contract.
  - Updated README routing, active task, contract evaluation, and Stages 5-8 completion documents.
- Verification:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 0 errors and 191 existing project warnings.
  - `Assets/Resources/string/presentation_string.csv`: 294 data rows, 154 `presentation.*` main keys, zero duplicate key pairs.
  - Unity Editor was not opened or controlled by the agent.

## 2026-08-12 — Documentation Manager Display-Catalog Reconciliation

- Status: active bilingual guidance reconciled with the strict player-display catalog; user Unity validation remains pending.
- Reconciliation:
  - Replaced the remaining player-label wording based on raw source/normalized paths with explicit `PresentationDisplayCatalog` to canonical `presentation.*` key lookup.
  - Preserved raw keys and generated fallback behavior only for inspection/debug formatting. The player formatter omits missing catalog or localization text.
  - Preserved existing Skill/Bless/Relic name and description lookup and fallback paths unchanged.
  - Updated current verification and rerun status to the player-display catalog build rather than the earlier semantic-regrouping build.
- Verification:
  - Current evidence records 0 build errors and 191 existing warnings, 294 localization data rows, 154 `presentation.*` main keys, zero duplicate key pairs, and zero missing keys among 141 statically required catalog keys.
  - The display-catalog English and Korean documents retain matching heading structure.
  - No runtime source, localization asset, prefab, Scene, build, Unity Editor, staging, commit, or push action was performed by the documentation manager.
  - No prefab, Scene, legacy asset, staging, commit, or push action was performed.

## 2026-08-12 — Missing Localization Key Visibility Correction

- Status: reusable localization guidance corrected; user Unity visual verification pending.
- Correction:
  - Superseded the earlier rule that missing mapped localization text or missing descriptions should become empty player text.
  - Kept unapproved raw JSON/C# keys filtered when no display-catalog mapping exists.
  - Required approved mapped keys and required name/description keys to remain visible as the full intended `mainKey.subKey` when StringManager has no matching row.
  - Recorded ordered candidate lookup: probe with `returnNullIfMissing: true`, use the first resolved candidate, or perform normal lookup of the first intended key after every candidate fails.
  - Preserved the existing candidate key paths and order, the StringManager-unavailable asset-name fallback, and the prohibition on invented description prose.
- Verification evidence from the source task:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal`: 0 errors and 191 existing project warnings.
- Pending:
  - The user will temporarily use a nonexistent approved mapped localization key in Unity, confirm that the full key is rendered, then restore the key.
  - No runtime source, localization asset, prefab, Scene, build, Unity Editor, staging, commit, or push action was performed by the documentation manager.

## 2026-08-13 — CharacterSO Skill Icon Tabs

- Status: source implementation complete; user Unity binding and validation pending.
- Implementation:
  - Added `CharacterSkillContentInfoPresenter` to create one icon tab for each non-null Skill reference in `CharacterSO.Skills`, preserve authored order, maintain one selected tab, and select an initial tab.
  - Added `SkillContentInfoTabButton` for icon binding, click dispatch, and optional selected visuals.
  - Added `ShowSkill` and `ClearPresentation` to the existing `SkillContentInfoPresenter`; Character composition reuses the existing Skill normalization, player grouping, StringManager formatting, and `UIContentInfoView` binding path.
  - Kept `UIContentInfoView` content-neutral.
- Prefab inspection:
  - `Assets/Prefabs/UI/Child/Slot/UISkillIconSlot.prefab` has the expected root and `Bind_SkillIconImage` hierarchy but currently has no `Button` or `SkillContentInfoTabButton`.
  - No prefab or Scene was modified because Unity work remains user-owned.
- Verification:
  - Source and prefab YAML inspection completed.
  - `dotnet build ProjectBS.sln --no-restore` was blocked before compilation by denied sandbox access to `C:\Users\machal89\AppData\Local\Microsoft SDKs`.
  - New source files are pending Unity project-file refresh, compilation, AutoBind, button, selection, tab rebuild, and scrolling validation.
- Documentation:
  - Added `AgentDocs/Machal/character-skill-content-tabs.md` and the Korean mirror with ownership, binding steps, and the manual validation checklist.

## 2026-08-13 — Character JSON Comparison and Player Information UI

- Status: source implementation and static validation complete; user Unity binding and visual validation pending.
- Source inventory:
  - Confirmed 22 current JSON files and 22 generated CharacterSO assets under `Assets/Resources/character/json/`.
  - Current JSON keys are `characterId`, `name`, `characterType`, `job`, and `baseStats`; generated Animation and Skill references are SO-only system data.
- Player-display contract:
  - Display the StringManager-backed name, source `characterType`, source `job`, and seven current source Stats.
  - Hide ID, Animation references, Skill references, slotKey, derived Job parts, and runtime state from the authored Character body.
  - Preserve each source Stat as one Entry and one value. Crit values are Percent, MoveSpeed is m/s, and AttackSpeed uses a localized multiplier format without changing the source number.
- Implementation:
  - Added `CharacterContentInfoPresenter` for a dedicated Character `UIContentInfoView` and optional Skill-tab synchronization.
  - Added a filtered `CharacterPresentationResolver.ResolveForPlayerDisplay` path and Character display-catalog entries.
  - Added `CharacterPresentationPreviewWindow` with side-by-side Original JSON, full SO inspection, and filtered player output plus JSON/SO/name mismatch reporting.
  - Added the 22 current authored Character names to the existing `character_string.csv` path and Character labels/formats to `presentation_string.csv`.
- Verification:
  - JSON/SO/name comparison: 22 files, 0 mismatches.
  - `presentation_string.csv`: 308 data rows, zero duplicate key pairs, one row per required Character key.
  - Runtime assembly including the new Presenter: 0 errors, 35 existing warnings.
  - Editor assembly including the final comparison tool: 0 errors, 197 aggregate warnings, including expected JsonUtility DTO `CS0649` warnings from the new tool.
  - Unity Editor was not operated; prefab binding, AutoBind, scrolling, and Play Mode visual output remain pending.

## 2026-08-13 — Documentation Manager Character Skill Tab Reconciliation

- Status: bilingual guide and README/log routing verified; user Unity binding and validation remain pending.
- Reconciliation:
  - Confirmed the Character-owned tab composition delegates selected Skills through `SkillContentInfoPresenter` and keeps `UIContentInfoView` content-neutral.
  - Confirmed the current `UISkillIconSlot.prefab` still has `UISkillIconSlot` and `Bind_SkillIconImage`, but no `Button` or `SkillContentInfoTabButton`.
  - Corrected the current verification note because `Assembly-CSharp.csproj` now includes both new source files.
- Verification:
  - A current `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal` attempt still stopped before C# compilation because access to `C:\Users\machal89\AppData\Local\Microsoft SDKs` was denied.
  - English/Korean guide structure, UTF-8, and whitespace checks passed.
  - No runtime source, prefab, Scene, Unity Editor, staging, commit, or push action was performed by the documentation manager.

## 2026-08-13 — Documentation Manager Character Presentation Reconciliation

- Status: Character source/display boundary and bilingual routing verified; user Unity binding and Play Mode validation remain pending.
- Reconciliation:
  - Confirmed the Character guide preserves the five authored JSON fields, seven ordered source Stats, complete SO inspection, filtered player output, and optional Skill-tab synchronization.
  - Extended the display-catalog header scope to include its existing Character section.
  - Preserved the corrected build wording: Runtime 0 errors with 35 existing warnings; Editor 0 errors with 197 aggregate warnings including expected JsonUtility DTO `CS0649` warnings.
- Verification:
  - All 22 strict-UTF8 JSON files have exactly `characterId`, `name`, `characterType`, `job`, and `baseStats`, with the same seven ordered Stat types.
  - The folder contains 22 generated CharacterSO assets; Character localization has one matching Korean name row per JSON; `presentation_string.csv` has 308 data rows and zero duplicate key pairs.
  - English/Korean Character guide and catalog heading structures match; UTF-8 and whitespace checks passed.
  - No runtime source, localization CSV, prefab, Scene, Unity Editor, staging, commit, or push action was performed by the documentation manager.

## 2026-08-13 — Bless and Relic Content Presenters

- Status: source implementation and static compilation complete; user Unity binding and Play Mode validation pending.
- Implementation:
  - Added `BlessContentInfoPresenter` under the existing Shrine UI domain and `RelicContentInfoPresenter` under the existing Relic UI domain.
  - Both presenters bind definition Preview data through their domain resolver's `ResolveForPlayerDisplay` path into an assigned existing `UIContentInfoView`.
  - Both presenters expose runtime-entry overloads, `Set`, `Show`, `Clear`, optional build-on-start behavior, component context-menu builds, and missing-EventSystem warnings.
  - The shared `UIContentInfoView` remains content-neutral and no Canvas, prefab instance, Scene object, SO asset, or legacy data was created or modified.
- Changed source paths:
  - `Assets/Scripts/Stage/NodeContents/Shrine/UI/BlessContentInfoPresenter.cs` and `.meta`
  - `Assets/Scripts/Collection/Relic/UI/RelicContentInfoPresenter.cs` and `.meta`
- Verification:
  - `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 0 errors and 35 existing project warnings.
  - Both source files were included in the compiled runtime assembly project.
  - Scoped `git diff --check` and placeholder scan passed.
  - Unity Editor was not opened or controlled by the agent.
- Pending user action:
  - Attach each Presenter, assign or AutoBind `BlessContentInfoView` / `RelicContentInfoView`, assign a current SO, enter Play Mode, and run the matching component context menu.

## 2026-08-13 — Documentation Manager Bless/Relic Presenter Reconciliation

- Status: bilingual ownership and handoff guidance reconciled; user Unity import, binding, and Play Mode validation remain pending.
- Reconciliation:
  - Updated the Stage 8 task status and work order from Skill-only presenter wording to the implemented Character/Skill/Bless/Relic presenter set.
  - Recorded the Bless/Relic definition Preview and runtime-entry binding flow in the Stages 5-8 completion guide without adding a separate guide.
  - Preserved content-neutral `UIContentInfoView` ownership and the user-owned Unity operation boundary.
- Verification:
  - Confirmed both Presenter sources expose the documented `Set`, `Show`, `Clear`, optional build-on-start, context-menu, runtime-entry, and EventSystem-warning behavior.
  - Preserved the reported static build result: 0 errors and 35 existing warnings; the documentation manager did not rerun the build or operate Unity.
  - English/Korean parity, strict UTF-8, whitespace, scoped diff, and the two unique `.meta` GUIDs were checked.
  - No runtime source, prefab, Scene, asset, staging, commit, or push action was performed by the documentation manager.

## 2026-08-13 Panel Character Previous/Next Navigation

- Status: source and prefab ownership preparation complete; user Unity button work and Play Mode validation pending.
- User request:
  - Make `CharacterSkillContentInfoPresenter` own a CharacterSO list and change the displayed Character through previous/next actions in `Panel_CharacterInfo`.
  - The user will create the Button objects and connect their events; the agent must implement only the functionality.
- Implementation:
  - Added serialized Character list, initial Character index, optional loop navigation, current index/count state, runtime list replacement, direct selection, and public previous/next methods.
  - Character changes rebuild Skill tabs and synchronize the existing Character body presenter.
  - Preserved the old single Character as a one-item fallback and preserved the old initial Skill index with `FormerlySerializedAs`.
  - Added null-list-slot handling plus `CanShowPreviousCharacter` and `CanShowNextCharacter` availability properties without owning Button state.
  - Added `CharacterContentInfoPresenter.ClearPresentation()` and prevented redundant Skill-tab synchronization when both presenters already point to the same Character.
- Prefab preparation:
  - Assigned the existing Character presenter to `CharacterSkillContentInfoPresenter`.
  - Disabled independent `CharacterContentInfoPresenter.buildOnStart` in this prefab so the Character-list owner controls startup.
  - Added no previous/next Button reference and left the new Character list empty for user assignment; the existing single Character remains the fallback.
- Changed paths:
  - `Assets/Scripts/Actor/Character/ui/CharacterSkillContentInfoPresenter.cs`
  - `Assets/Scripts/Actor/Character/ui/CharacterContentInfoPresenter.cs`
  - `Assets/Prefabs/UI/Fixed/Panel/Panel_CharacterInfo.prefab`
  - `AgentDocs/Machal/character-skill-content-tabs.md` and Korean mirror
  - Active task and task log, English and Korean
- Verification:
  - `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 0 errors and 35 existing warnings.
  - Unity Editor was not opened or controlled.
- Pending user action:
  - Populate `characters`, create the previous/next buttons, connect them to `ShowPreviousCharacter()` and `ShowNextCharacter()`, save the prefab, and validate Character body/Skill-tab synchronization in Play Mode.

## 2026-08-14 — Current Relic JSON and Generated-Asset Path

- Status: source JSON path and Unity builder prepared; generated SO asset creation and validation pending user Unity execution.
- User decision:
  - Store the current Relic JSON and generated `RelicSO` assets under `Assets/Resources/relic/json/`.
  - Keep the excluded legacy assets under `Assets/Resources/shop/relic/` unchanged.
- Implementation:
  - Copied the ten normalized Relic JSON files into the approved current Relic path without modifying the existing `Assets/Resources/item/json/` files.
  - Replaced the production-asset migration menu with `Tools > ProjectBS > Items > Build Current Relics From JSON`.
  - The builder creates or updates ten `RelicSO`, twelve `EffectSO`, and twelve `EffectEntrySO` assets beside the JSON files and validates that every reference stays inside the approved path.
  - Added `Tools > ProjectBS > Items > Validate Current Relics` for a separate user-run validation.
- Changed paths:
  - `Assets/Editor/tools/item/RelicItemAssetBuilder.cs`
  - `Assets/Resources/relic/`
  - Active task and task log, English and Korean.
- Verification:
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q`: 0 errors and 162 existing warnings.
  - Ten new JSON files parse as UTF-8 JSON and match the ten source JSON files after line-ending normalization.
  - New `.meta` GUIDs are unique, `git diff --check` passes, and `Assets/Resources/shop/relic/` has no scoped change.
- Pending verification:
  - User Unity builder execution, asset import, validation menu, and Relic UI output.
- Not performed:
  - No Unity Editor operation, legacy asset mutation, prefab change, staging, commit, or push.

## 2026-08-14 — Documentation Manager Current Relic Path Reconciliation

- Status: current Relic authoring/generated-asset path reconciled across Machal and canonical Relic workflow guides; user Unity generation and validation remain pending.
- Reconciliation:
  - Updated the Relic JSON generation prompt and JSON guide to write current JSON only under `Assets/Resources/relic/json/`.
  - Updated the RelicSO guide to the actual current code paths and recorded the builder and validation menu flow.
  - Clarified that `Assets/Resources/item/json/` retains source evidence and `Assets/Resources/shop/relic/` remains untouched legacy comparison data.
  - Clarified that current JSON and builder-generated `RelicSO`, `EffectSO`, and `EffectEntrySO` assets share the approved root, while JSON authoring and Unity asset generation remain separate operations.
- Verification:
  - Confirmed ten current JSON files under the approved root and ten retained source JSON files with matching names.
  - Confirmed the builder menu names, approved-root checks, expected ten RelicSO / twelve EffectSO / twelve EffectEntrySO outputs, and legacy exclusion in source.
  - Strict UTF-8, whitespace, stale current-output-path, heading-order, and scoped diff checks passed.
  - No builder execution, Unity operation, runtime source, JSON, generated asset, legacy asset, staging, commit, or push action was performed by the documentation manager.

## 2026-08-14 — BlessSO List and Selection Presenter

- Status: runtime source implementation and static compilation complete; user Unity wiring and Play Mode validation pending.
- Extended `BlessContentInfoPresenter` from one serialized Bless to an inspector-configured `List<BlessSO>` with an initial selection index.
- The Presenter skips null list entries, creates reusable `UISelectableIconButton` tabs, applies each Bless icon, owns selected-state updates, and displays the selected Bless through the existing player-display resolver and content View.
- Added `SetBlesses`, indexed `SelectBless`, tab cleanup, selection/count properties, and retained the previous single-Bless API surface.
- Reused the existing generic icon button instead of adding a Bless-only button component.
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 0 errors and 35 existing warnings.
- No prefab, Scene, SO asset, legacy data, Unity Editor state, staging, commit, or push was changed.

## 2026-08-14 — Faith Page and Three-Bless Design Correction

- Recorded the user decision that one selected god owns one complete Bless information page, analogous to one selected Character owning its Skill tabs and detail page.
- Superseded the former Enforce-level interpretation. Each god has three distinct Bless units: one Faith-scaling Basic Bless, Exclusive Bless 1 granted on Faith lock, and Exclusive Bless 2 granted upon reaching Faith level 8 after Faith lock.
- The Basic Bless scales with Faith level without becoming separate unrelated Bless tabs. Exclusive Bless 2's acquisition condition is confirmed as Faith level 8 after Faith lock.
- Kept the Faith page deferred and made no runtime or UI implementation change.
- Source inspection found that the current `Base/Enhanced` group enum and `progressionStep` field do not explicitly encode the corrected three-unit contract, and `ShrineGodSO.GetAvailableBlessings` ignores its group argument. This must be resolved before the future Faith-page adapter is implemented.
- Recorded the deferred prefab preparation contract: one Faith composition panel, one reusable god tab, one reusable Bless tab with selected/locked visuals, and one shared existing Bless content View. No category-specific detail prefab is required.

## 2026-08-14 — Documentation Manager Bless List Presenter Reconciliation

- Status: bilingual Ability Presentation guidance reconciled; user Unity wiring and Play Mode validation remain pending.
- Reconciliation:
  - Extended the existing Stages 5-8 completion guide with the Bless Presenter list, generic-tab, and selection ownership contract.
  - Preserved the content-neutral `UIContentInfoView`, existing single-Bless API compatibility, and deferred Faith/Bless-page architecture.
  - Recorded the exact user wiring checklist without creating a separate guide.
- Verification:
  - Confirmed the source skips null entries, instantiates `UISelectableIconButton`, owns selection state, uses `ResolveForPlayerDisplay`, and retains `SetBless`, `ShowBless`, `BuildPresentation`, and `Bless`.
  - Preserved the reported build result: 0 errors and 35 existing warnings; the documentation manager did not rerun the build or operate Unity.
  - English/Korean heading parity, strict UTF-8, whitespace, and scoped diff checks passed.
  - No runtime source, prefab, Scene, asset, staging, commit, or push action was performed by the documentation manager.

## 2026-08-14 — Documentation Manager Three-Bless Model Reconciliation

- Status: deferred Faith-page ownership and the corrected three-Bless contract reconciled; implementation remains blocked on authoritative source encoding.
- Reconciliation:
  - Added the decided god-page ownership flow to the existing Stages 5-8 completion guide without creating a Faith-page document or implementation.
  - Recorded one Faith-scaling Basic Bless, Exclusive Bless 1 acquired on Faith lock, and Exclusive Bless 2 acquired at Faith level 8 after Faith lock as three distinct Bless units.
  - Explicitly prohibited separate tabs for Basic Bless strength levels and inference of the confirmed Exclusive Bless 2 rule from `successorFaithLevel` or source names.
  - Preserved `BlessContentInfoPresenter` ownership of the selected god's Bless tabs/details and the content-neutral shared View boundary.
- Verification:
  - Confirmed current source exposes only `ShrineBlessingGroup.Base/Enhanced` and `BlessPoolEntry.ProgressionStep`.
  - Confirmed `ShrineGodSO.GetAvailableBlessings` accepts but does not use the group argument; only the authoritative three-unit source encoding remains unresolved.
  - English/Korean heading parity, strict UTF-8, whitespace, rejected-interpretation residue, and scoped diff checks passed.
  - No code, build, Unity, prefab, Scene, asset, staging, commit, or push action was performed by the documentation manager.

## 2026-08-14 - Documentation Manager Deferred Faith Prefab Reconciliation

- Status: the minimal user-preparation contract is reconciled; Faith runtime integration remains deferred.
- Reconciliation:
  - Added the preparation contract to the existing Stages 5-8 completion guide without creating a standalone Faith guide.
  - Recorded one `Panel_FaithInfo`, one reusable god icon tab, one reusable Bless icon tab, and reuse of `Assets/Prefabs/UI/Fixed/Content/UIContentInfoView_Bless.prefab`.
  - Preserved the exact current AutoBind names `BlessContentInfoTabRoot` and `BlessContentInfoView`; future god-header, Faith-progress, and god-tab-root names remain undecided.
  - Recorded one shared Bless tab/detail structure for Basic, Exclusive Bless 1, and Exclusive Bless 2, with icon, selected-frame, and locked-overlay elements. Common Blesses remain outside selected-god ownership.
- Verification:
  - Confirmed `BlessContentInfoPresenter` binds `BlessContentInfoTabRoot` and `BlessContentInfoView`, reuses `UISelectableIconButton`, and currently calls `SetLocked(false)` for every tab.
  - Confirmed `UISelectableIconButton` exposes the icon, selected-frame, and locked-overlay AutoBind fields.
  - No code, build, Unity, prefab, Scene, asset, staging, commit, or push action was performed by the documentation manager.

## 2026-08-17 — Relic, General Bless, and Faith Display Ownership Correction

- User confirmed that all owned Relics and acquired General Blesses apply immediately without equipment state; Faith alone is an unlock/level progression system.
- Corrected the Faith model from three Bless units to four heterogeneous features: Faith-scaling Basic Bless, job-group Exclusive Advancement, Exclusive Bless 1, and Exclusive Bless 2.
- Recommended one `Owned Effects` page with `All`, `Relic`, and `General Bless` views instead of a separate top-level Owned Bless page. The full Faith page stays separate; `All` may show only concise currently applied Faith results and a navigation link.
- Recorded that Exclusive Advancement is not `BlessSO` data and requires a Faith-owned adapter rather than being forced through `BlessContentInfoPresenter`.
- Source inspection found stale runtime concepts that must be reconciled before implementation: `RelicItemService.EquippedRelics` and permanent Common-Bless replacement in `BlessManager.AddBless`.
- No runtime source, prefab, Scene, asset, build, staging, commit, or push action was performed.

## 2026-08-17 — Owned Effect Inventory Scaffold

- User finalized the current screen as an effect inventory with `All`, `Relic`, `General Bless`, and `Faith Bless` tabs plus one right-side detail panel.
- The Faith page remains a separate encyclopedia for its progression features. A future Relic encyclopedia will separately show acquired and unacquired Relics.
- Preserved `RelicCollectionView` unchanged for the future Relic encyclopedia because it already owns locked silhouettes and owned/total counts.
- Added `OwnedEffectInventoryData`, `OwnedEffectGridItemView`, `OwnedEffectInventoryView`, and `OwnedEffectInventoryPresenter` under `Assets/Scripts/Presentation/SharedUI/Content/`.
- The new Presenter supports configured preview sources and an explicit runtime API receiving owned Relics, active General Bless entries, and active Faith Bless entries. The View exposes public methods for all four tab buttons and binds the selected item to one always-active neutral content View.
- A temporary compile-project inclusion was used only for static verification and then reverted. `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`: 0 errors and 35 existing warnings.
- No existing Relic source, prefab, Scene, Unity object, asset, staging, commit, or push was changed. Unity import, component replacement, grid-item prefab setup, button wiring, and Play Mode validation remain pending user work.

## 2026-08-17 - Detailed Faith Page Redesign

- Reframed the Faith page from a Bless-only detail page into a selected-god progression page that explains current power and future unlocks together.
- Kept one tab per acquired Faith and defined one selected-god page containing god summary, levels 1-10 roadmap, four reusable feature cards, and one content-neutral detail view.
- Defined the four heterogeneous Faith features as the level-scaling Basic Bless, job-family Exclusive Job Change, Exclusive Bless 1, and Exclusive Bless 2.
- Recorded that Basic Bless progression must use explicit authored level data or actual runtime results; Presentation must not interpolate, combine, or invent values.
- Recorded that locked features remain selectable for preview, while their lock state and exact authored unlock condition stay visually explicit.
- Moved page ownership to the proposed `FaithPagePresenter` and meaning/unlock evaluation to `ShrineFaithPresentationResolver`; `BlessContentInfoPresenter` remains a standalone Bless-list/detail owner.
- Recorded five required prefabs: the page panel, reusable Faith tab, reusable level node, reusable feature card, and an optional neutral `UIContentInfoView` layout variant.
- Preserved unresolved data decisions instead of inferring them: the Exclusive Job Change unlock condition and exact job-family-to-target-job mapping still require authoritative authoring data.
- Added `AgentDocs/Machal/faith-page-design.md` and its Korean mirror as the authoritative design contract. No runtime source, prefab, Scene, asset, build, staging, commit, or push action was performed.

## 2026-08-17 - Documentation Manager Ownership and Faith Redesign Reconciliation

- Status: active bilingual Ability Presentation guidance now follows the no-equipment Relic/General-Bless rule and the four-feature Faith-page design; runtime and Unity work remain pending.
- Reconciliation:
  - Replaced the active Stages 5-8 three-Bless and Bless-tab-only Faith wording with the authoritative selected-god progression page and `FaithPagePresenter` / `ShrineFaithPresentationResolver` ownership.
  - Recorded Basic Bless, job-family Exclusive Job Change, Exclusive Bless 1, and Exclusive Bless 2 as four heterogeneous Faith features; Exclusive Job Change is not `BlessSO`.
  - Recorded that every acquired General Bless and owned Relic applies immediately and that player UI must not expose or filter by equipment state.
  - Added the recommended `Owned Effects` navigation with `All`, `Relic`, and `General Bless` views while keeping the full Faith page separate and avoiding duplicated progression details.
  - Updated the display catalog so current `EquippedRelics` and Bless equipment-style fields remain diagnosable as source mismatches without becoming player-display rules.
  - Preserved earlier dated three-Bless entries as historical records superseded by the 2026-08-17 corrections.
- Verification:
  - Confirmed `RelicItemService` still exposes `OwnedRelics` and `EquippedRelics`; confirmed `BlessManager.AddBless` removes an existing permanent Common Bless before adding a new one.
  - Confirmed the bilingual Faith design remains the authoritative detailed prefab, ownership, interaction, implementation-order, and validation contract.
  - No code, build, Unity, prefab, Scene, asset, staging, commit, or push action was performed by the documentation manager.

## 2026-08-17 - Documentation Manager Owned Effect Inventory Reconciliation

- Status: the finalized four-tab inventory and separate Faith/Relic encyclopedia roles are reconciled across active bilingual guidance; user Unity wiring and runtime-source integration remain pending.
- Reconciliation:
  - Superseded the earlier three-tab `Owned Effects` recommendation with exactly `All`, `Relic`, `General Bless`, and `Faith Bless`.
  - Recorded that `All` contains all currently applied entries and `Faith Bless` contains only active Bless-backed Faith features. Full Faith progression and Exclusive Job Change remain in the Faith encyclopedia unless an explicit Effect source is authored later.
  - Recorded one right-side neutral `UIContentInfoView` as the detail target for every inventory selection and preserved the no-separate-Owned-Bless-page decision.
  - Added the implemented `OwnedEffectInventoryData`, `OwnedEffectGridItemView`, `OwnedEffectInventoryView`, and `OwnedEffectInventoryPresenter` scaffold and its configured-Preview / explicit-runtime-list boundary to the completion guidance.
  - Preserved `RelicCollectionView` exclusively for the future Relic encyclopedia with acquired/unacquired entries, locked silhouettes, and owned/total counts; it must not be generalized into the Owned Effect inventory.
  - Updated the Faith guide and display catalog to match the finalized page boundaries without rewriting historical entries.
- Verification:
  - Confirmed the View exposes all four tab methods and binds a selected item to one neutral detail View.
  - Confirmed the Presenter accepts configured Relic/General-Bless/Faith-Bless definitions and explicit runtime lists; automatic Manager collection is not implemented.
  - Preserved the reported static build result of 0 errors and 35 existing warnings; the documentation manager did not rerun the build or operate Unity.
  - No runtime source, prefab, Scene, asset, staging, commit, or push action was performed by the documentation manager.

## 2026-08-17 - Content Inventory Refactor Phase 1

- Replaced the pending prefab-first direction with an incremental category-section refactor based on the user's reference layout: one vertical page scroll containing dynamically created category headers and item grids.
- Added `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryData.cs` and its `.meta` without modifying or removing the existing `OwnedEffect` scaffold.
- Added neutral page/category/item snapshot contracts, `OwnedOnly`/`Catalog` display modes, and `Owned`/`Unowned`/`Locked` acquisition states.
- Kept concrete Relic, General Bless, and Faith Bless category knowledge out of the shared contract. Domain presenters will provide the internal category ID and StringManager title key in a later phase.
- Static verification: `dotnet build ProjectBS.sln --no-restore -v:minimal` passed with 0 errors and 197 existing warnings after temporary compile inclusion. `Assembly-CSharp.csproj` was restored afterward.
- No existing View, Presenter, prefab, Scene, gameplay source, asset, staging, commit, or push was changed. Phase 2 common item/category Views and Unity validation remain pending.

## 2026-08-17 - Content Inventory Tab Semantics Correction

- Replaced the pending `All` tab with `Owned`; tabs are now exactly `Owned`, `Relic`, `General Bless`, and `Faith Bless`.
- `Owned` is an aggregate page mode, not a content category. It creates all available owned category sections in one vertical scroll and is always `OwnedOnly`, so `Catalog` cannot be selected there.
- Relic and General Bless are single-category tabs that may independently use `OwnedOnly` or `Catalog`.
- Faith Bless remains an active-owned tab. Full Faith progression, inactive features, and future unlocks remain in the separate Faith encyclopedia.
- This correction changes the pending page/view coordination contract only. No C# source, prefab, Scene, asset, build, staging, commit, or push action was performed.

## 2026-08-18 - Tabless Owned Effects Page Correction

- Removed all tabs from the Owned Effects page. It is now one vertical-scroll inventory containing categorized sections for owned Relics, acquired General Blesses, and active Faith Blesses only.
- Removed `Catalog` selection from the Owned Effects page. The shared `Catalog` data mode remains available for separate Relic and General Bless encyclopedia pages.
- Full Faith progression, inactive features, and future unlocks remain in the separate Faith encyclopedia.
- The existing four-tab `OwnedEffectInventoryView` scaffold is explicitly superseded and will be replaced incrementally after the common item/category Views are implemented.
- No C# source, prefab, Scene, asset, build, staging, commit, or push action was performed in this correction unit.

## 2026-08-18 - Owned Effects New-Chat Start Contract

- Added a task-specific English canonical and Korean mirror start contract for continuing the tabless Owned Effects inventory in a new chat.
- The contract contains a copy-paste request, exact mandatory reading order, confirmed design, implemented/superseded/pending paths, the next single Phase 2 unit, work rules, Unity stop boundary, and end-of-unit report format.
- Updated `AgentDocs/Machal/README.md` so the task-specific contract is a required active entrypoint rather than relying on a generic one-line handoff.
- Updated the active task handoff state to require the new contract before implementation.
- No C# source, prefab, Scene, gameplay asset, build, staging, commit, or push action was performed.

## 2026-08-18 - General Bless Encyclopedia and Content Inventory Phase 2

- The user selected the separate General Bless encyclopedia as the first catalog integration target. It will display the complete supplied `BlessSO` list or `BlessPoolSO` and mark only runtime-active definitions active; it is not the tabless Owned Effects page.
- Confirmed `BlessPoolSO.BlessPoolEntry` exposes `Blessing`, generation `Weight`, and `ProgressionStep`; only the Bless definition is inventory content. Confirmed `BlessManager.Blessings` exposes active `BlessRuntimeData.BlessEntry.source` definitions suitable for authored `BlessingId` matching.
- Added `ContentActivationState.Inactive/Active` to `ContentInventoryItemData` without conflating active state with selected, acquired, or locked state.
- Added `ContentInventoryItemView` and `ContentInventoryCategoryView` with paired Unity `.meta` files. The item exposes a separate `UI_ActiveIndicatorImage`; the category owns localized title/count and a generated Grid but no nested `ScrollRect`.
- Clean static verification after temporary compile inclusion: `dotnet build ProjectBS.sln --no-restore -v:minimal` passed with 0 errors and 197 existing warnings. The first verification had one temporary duplicate-source warning and was rerun clean; `Assembly-CSharp.csproj` was restored.
- No existing Presenter, prefab, Scene, gameplay source, asset, staging, commit, or push was changed. General Bless Presenter/detail integration and Unity validation remain pending.

## 2026-08-18 - Owned Effects Direct Prefab Wiring

- Status: tabless Owned Effects code and prefab component graph are statically complete; Unity import and Play Mode validation remain user-owned.
- User authorization: the user explicitly requested that the agent directly perform the required component connections. This one-off request superseded the prefab-edit stop boundary only for the three named prefabs in this work unit; it does not authorize later Unity operation or unrelated prefab YAML edits.
- Replaced the earlier four-tab implementations at `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryView.cs` and `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryPresenter.cs` with one vertical-scroll, dynamically categorized Owned Effects page.
- The Presenter now emits ordered `OwnedOnly` categories for configured/explicit owned Relics, General Blesses, and active Faith Blesses. Automatic Manager collection remains pending.
- Updated `ContentInventoryCategoryView` AutoBind names to the existing prefab hierarchy and made runtime clearing remove every design-placeholder child before generated item creation.
- Attached and assigned `ContentInventoryItemView` on `Assets/Prefabs/UI/Fixed/Content/UIInventoryItemView.prefab` and `ContentInventoryCategoryView` on `Assets/Prefabs/UI/Fixed/Content/UIContentInventoryCategory.prefab`.
- Attached `OwnedEffectInventoryView` and `OwnedEffectInventoryPresenter` to `Assets/Prefabs/UI/Fixed/Panel/Panel_OwnedEffects.prefab`, assigned title/scroll/category/template/detail references, removed the panel-local standalone `RelicContentInfoPresenter`, and kept the shared detail object active.
- Added StringManager rows for the Owned Effects page and Relic, General Bless, and Faith Bless category titles in `Assets/Resources/string/presentation_string.csv`.
- Verification: `dotnet build ProjectBS.sln --no-restore -v:minimal` passed with 0 errors and 197 existing warnings. Static prefab checks passed for all four new script attachments, all required non-null references, removal of the old Relic Presenter, active shared detail, and four unique localization rows.
- Pending user verification: Unity import/Console, Inspector deserialization, configured SO assignment, category generation, icon click/selection, shared detail binding, outer ScrollRect input, localization, and visual layout.
- No legacy Effect/Bless/Relic asset, gameplay ownership behavior, Scene, Git staging, commit, push, reset, or clean operation was performed.

## 2026-08-18 - Documentation Manager Direct-Wiring Reconciliation

- Reconciled active English/Korean guidance to the completed tabless Owned Effects implementation while preserving earlier four-tab entries as superseded dated history.
- Replaced stale pending-replacement and next-General-Bless-Presenter wording with the current directly wired item/category/page graph and user-owned Unity validation step.
- Recorded that direct YAML wiring was authorized once for `Panel_OwnedEffects.prefab`, `UIContentInventoryCategory.prefab`, and `UIInventoryItemView.prefab`; it is not standing authorization for later prefab edits or Unity operation.
- Confirmed from current source and prefab YAML that the four scripts are attached, required references are non-null, the panel-local `RelicContentInfoPresenter` is absent, the shared detail is active, and four unique `presentation.inventory.*` rows exist.
- Confirmed that `GeneralBlessCatalogPresenter` does not exist; the separate General Bless encyclopedia remains a planned independent page rather than a completed Catalog path.
- The documentation manager did not change runtime source, prefabs, localization assets, Scenes, staging, commits, or pushes and did not rerun the reported solution build.

## 2026-08-21 - Faith Current/Next Effect-Card Design Correction

- Replaced the planned four standalone feature cards and selected-detail area below the Faith roadmap with two `UIFaithLevelEffectCard` instances for actual current-level and immediate-next-level Faith effects.
- Defined source-ID-based `Strengthened`, `NewlyUnlocked`, and `Unchanged` comparison states without deriving new numeric delta values or comparing localized labels.
- Kept a stable two-card layout at maximum level by rendering a localized no-next-level empty state in the next card.
- Kept future roadmap nodes as milestone information; they do not change the bottom comparison away from actual current and next levels.
- Updated the English/Korean Faith design and active task contracts. No code, prefab, Scene, asset, build, staging, commit, or push action was performed.

## 2026-08-21 - Faith Main Panel Direct Scaffold Implementation

- Under explicit user authorization, directly rebuilt `Assets/Prefabs/UI/Fixed/Panel/Panel_FaithInfo.prefab` while preserving its background and foreground visuals.
- Removed the legacy `FaithDetailView` and incomplete layout objects, then created the Faith tab strip, selected-god summary, horizontal ten-level roadmap, and two embedded current/next effect cards.
- Added and attached five runtime UI component types. AutoBind references, ten level-node references, the configured-God Inspector list, and two nested neutral `UIContentInfoView` instances were serialized into the panel.
- Added a callable `Build Configured Faith Page` ContextMenu scaffold and explicit `[PLACEHOLDER]` logging for the pending source-backed current/next comparison resolver.
- Added thirteen `presentation.faith.*` StringManager rows and corrected `AutoBindEditorUtility` GameObject-field handling after two pre-fix validation exceptions exposed the shared bug.
- Static validation passed for component counts, non-null references, level-node count, horizontal layout configuration, localization uniqueness, and legacy component removal.
- Unity Editor static inspection confirmed the rebuilt hierarchy and even ten-node roadmap layout. The full solution build passed with 0 errors and 209 existing warnings.
- Removed the one-off auto-running Editor builder after it saved the prefab. Play Mode, configured SO data, generated tabs, card content, clicks, and scroll input remain unverified.
