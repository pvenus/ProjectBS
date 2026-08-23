# Task: Ability Content Presentation Data System

## Status

- Phase: player-display catalog and visible missing-localization-key handling implemented; previous Skill validation predates the latest display contract; nested Skill traversal deferred; current user Unity rerun pending
- Implementation: shared contracts, all current Effect mappings, Skill composition, current-definition Character/Bless/Relic adapters, explicit player-display allowlist, localization catalog, compact formatting, generic View scripts, Character/Skill/Bless/Relic presenters, and user-run validation tools implemented
- Stage 4 verification: thirteen synthetic mappings and twenty approved EffectEntry assets passed before the latest corrections; the current fifteen-case self-test rerun is pending
- Display-catalog verification: Editor assembly build passed with 0 errors and 191 existing warnings; 141 statically required localization keys have 0 missing rows
- Stages 5-8 evidence: `AgentDocs/Machal/ability-content-presentation-stage5-8-completion.md`
- View and prefab work: the user attached the generic View components; hierarchy references now use the project AutoBind convention, while the Tag/Group/Entry template-prefab asset references remain manual
- Git commit and push: prohibited unless separately requested

## Goal

Build a structured presentation-data system for gameplay content so UI code does not interpret `EquipmentSkillSO`, `EffectSO`, `BlessSO`, or `RelicSO` fields directly.

The first delivery is the data-setting layer. View, Presenter, prefab, and layout work are a later phase.

## User Decisions

1. Do not modify or migrate legacy Effect, Bless, or Relic data.
2. Identify and use only approved current SO and asset paths.
3. Gameplay interpretation belongs to the Ability domain, not to UI.
4. Do not create or use a new `Assets/Scripts/Core/` path for this feature. Place neutral cross-content presentation contracts under the existing root `Assets/Scripts/Presentation/` category.
5. Design around semantic category and grouping, not direct variable-to-string formatting.
6. Complete and verify data setting before planning View integration.
7. Keep planning, design, work order, work method, work content, and progress logs under `AgentDocs/Machal/` for continuation by another agent.
8. Do not add content-domain `Presentation/` or feature-only `Resolvers/` folders. Put content presentation data in the owner's `Data/` child and distinguish behavior with explicit resolver or builder class names at the owner root.
9. Use the user-approved Effect-to-normalized-result table as the authoritative normalization contract.
10. Keep `OnHit`, `OnHeal`, and `OnAttack` as distinct activation events. Keep `Heal` and `PeriodicDamage` as separate outcome types under the latest approved seven-outcome contract.
11. Keep the existing StringManager name/description candidate paths, lookup order, and StringManager-unavailable asset-name fallback unchanged. Resolve player Group, Entry, Tag, enum replacement, and value-format text by canonical StringManager keys.
12. Never expose raw JSON/C# keys or generated Pascal-case text in the player View. Retain those keys only in inspection and provenance output.
13. Classify every available field as player-visible, conditionally visible, or inspection-only. Do not infer a player label for a new field until the display catalog explicitly approves it.
14. Keep an intended localization key visible when its mapped or required StringManager row is missing. This diagnostic key is not the same as an unapproved raw gameplay key, which remains omitted.

## Required Source Files

- `Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentSkillSO.cs`
- `Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectSO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectEntrySO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/config/`
- `Assets/Scripts/Ability/Effects/Resolvers/EffectResolver.cs`
- `Assets/Scripts/Ability/Effects/Runtime/config/`
- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessSO.cs`
- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessRuntimeData.cs`
- `Assets/Scripts/Collection/Relic/Definitions/RelicSO.cs`
- `Assets/Scripts/Collection/Relic/Runtime/RelicRuntimeData.cs`
- `Assets/Scripts/Actor/Character/so/CharacterSO.cs`

## Approved Current Asset Paths

Use these paths for the first implementation and verification:

- `Assets/Resources/skill/character/generated/`
- `Assets/Resources/skill/json/`
- `Assets/Resources/relic/json/`

The Skill paths contain assets serialized with the current `EquipmentSkillSO`, `EffectSO`, and `EffectEntrySO` definitions. `Assets/Resources/relic/json/` is the approved current Relic path: it contains the normalized Relic JSON files and, after the user runs the Unity builder, the generated current `RelicSO`, `EffectSO`, and `EffectEntrySO` assets beside those JSON files.

## Excluded Asset Paths

Do not modify, migrate, or use the following as authoritative verification data:

- `Assets/Resources/bless/`
- `Assets/Resources/shring/`
- `Assets/Resources/shop/relic/`
- Other existing Bless/Relic assets that serialize legacy `effects` data

The excluded legacy Relic paths remain unchanged. Current Relic assets are generated only under `Assets/Resources/relic/json/`; their Unity generation and asset validation remain pending user execution. Bless asset validation remains pending until an approved current Bless asset path exists.

## Architecture

### Shared presentation contract

Planned path:

```text
Assets/Scripts/Presentation/
  ContentPresentationData.cs
  PresentationIdentityData.cs
  PresentationGroupData.cs
  PresentationEntryData.cs
  PresentationValueData.cs
  PresentationContext.cs
  PresentationProvenanceData.cs
```

These shared classes must not reference `EffectSO`, `EquipmentSkillSO`, `BlessSO`, or `RelicSO`. They are data contracts, not gameplay resolvers and not View components.

### Effect presentation

Planned path:

```text
Assets/Scripts/Ability/Effects/
  Data/
    EffectPresentationData.cs
  EffectPresentationResolver.cs
```

`EffectPresentationResolver` is the only Effect presentation behavior class. It switches on the concrete current `EffectConfig`, builds the optional Activation and semantic outcome directly, and returns structured data without producing final UI sentences.

The first implementation keeps Activation, outcome, constraint, and typed outcome records together in `EffectPresentationData.cs`. Split them only after the file grows enough to justify separate ownership. The `Data/` child separates passive normalized data. Do not add a Config-specific resolver interface or one resolver class per Config while the mappings remain small. Repeated construction may use small private methods grouped by normalized outcome, such as StatModifier or CooldownChange.

### Skill presentation

Planned path:

```text
Assets/Scripts/Ability/Skills/
  Data/
    SkillPresentationData.cs
    SkillClassificationPresentationData.cs
  SkillPresentationResolver.cs
  SkillPresentationGroupResolver.cs
```

Skill presentation reuses normalized Effect presentation results and does not reinterpret Effect configs.
The exact Skill data path is `Assets/Scripts/Ability/Skills/Data/`; resolver and builder classes stay directly under `Assets/Scripts/Ability/Skills/`.

### Other content presentation

Implemented source-definition adapter paths; approved current asset validation remains pending:

```text
Assets/Scripts/Stage/NodeContents/Shrine/Blessings/
  Data/BlessPresentationData.cs
  BlessPresentationResolver.cs

Assets/Scripts/Collection/Relic/
  Data/RelicPresentationData.cs
  RelicPresentationResolver.cs

Assets/Scripts/Actor/Character/
  Data/CharacterPresentationData.cs
  CharacterPresentationResolver.cs
```

These adapters reuse the Effect normalization entrypoint in `Ability/Effects` and convert their own identity or runtime state into the shared contract in `Assets/Scripts/Presentation/`. They do not make excluded legacy assets authoritative. The user explicitly authorized source-level adapter implementation before approved current Character/Bless/Relic asset paths exist; asset-level verification remains pending.

The exact future data paths are:

- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Data/`
- `Assets/Scripts/Collection/Relic/Data/`
- `Assets/Scripts/Actor/Character/Data/`

### UI

Generic UI Views consume the neutral contracts in `Assets/Scripts/Presentation/`. They must not own gameplay interpretation.

The prefab structure and layout are complete under `Assets/Prefabs/UI/Fixed/Content/` using `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`. The user attached the generic View components. Hierarchy fields use `AutoBindPrefix` and `AutoBind`; template-prefab asset references remain manual. Domain presenters bind assigned Character, Skill, Bless, or Relic content to an existing View without creating UI. Do not move concrete SO fields or build behavior into neutral `UIContentInfoView`. Broader Scene integration remains separate. Group/entry label data is managed in `Assets/Resources/string/presentation_string.csv`. The user owns Unity import, component attachment, field assignment, component menu execution, prefab/Scene validation, and visual evaluation.

## Active Design References

- Stage 3 Effect model and resolver preparation: `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`
- Player display inventory and localization catalog: `AgentDocs/Machal/ability-content-presentation-display-catalog.md`
- Stage 4 Effect mapping and user test: `AgentDocs/Machal/ability-content-presentation-stage4-verification.md`
- User-side UI prefab preparation: `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`

### Dependency direction

```text
Presentation neutral contracts
<- owning-domain resolvers and builders
<- Views, tooltips, and editor previews consume resolved data
```

1. Shared content-neutral contracts know no concrete SO types.
2. Domain resolver and builder code knows only its owned SO/config types and outputs shared contracts.
3. Views and tools consume normalized contracts.
4. Final string formatting is later and must not be collapsed into per-config resolver strings.

## Authoritative Effect Normalization Contract

Effect data is normalized into an optional `Activation` plus one semantic outcome. The outcome is not a direct dump of source fields.

| Source Effect | Normalized result |
| --- | --- |
| `StatModifierEffect` | `StatModifier` |
| `ChanceOnHitStatModifierEffect` | `Activation(OnHit + Chance) + StatModifier` |
| `OnHitTimedStatModifierEffect` | `Activation(OnHit + Chance) + StatModifier` including duration; `StunDuration` and `RootDuration` specialize to `Control(Stun/Root + duration)` because runtime applies them as control timers |
| `ChanceOnHealStatModifierEffect` | `Activation(OnHeal + Chance + Target) + StatModifier` |
| `HealEffect` | `Heal` |
| `ChanceOnHealCooldownReduceEffect` | `Activation(OnHeal + Chance + Target) + CooldownChange` |
| `CooldownReduceEffect` | `CooldownChange` |
| `KnockbackEffect` | `Displacement` |
| `OnHitKnockbackDistanceEffect` | `Activation(OnHit + Chance) + Displacement` |
| `AttackBleedEffect` | `Activation(OnAttack + Chance) + PeriodicDamage` |
| `OnHitPoisonDotEffect` | `Activation(OnHit + Chance) + PeriodicDamage` |
| `ChanceOnHitSkillEffect` | `Activation(OnHit + Chance + Critical requirement) + SkillInvoke` |
| `TauntEffect` | `Control` |
| Semantically ambiguous legacy `SkillEffectSO` | Do not normalize fields; use only the authored description |

### Activation

`EffectActivationPresentationData` can contain:

- Trigger: `OnHit`, `OnHeal`, or `OnAttack`
- Authored chance number and source unit, or an explicit runtime-resolved value with runtime provenance
- Heal target condition
- Critical-hit requirement

Only conditions that affect activation belong here.

### Outcomes

- `StatModifier`: stat, operation, source-faithful value and unit, and duration when the modifier itself is timed
- `Heal`: flat, maximum-HP, and attack-scaling source values plus clamp behavior when applicable
- `CooldownChange`: ratio and/or flat seconds according to the runtime reduce type
- `Displacement`: direction and a magnitude whose kind distinguishes force from distance
- `PeriodicDamage`: source damage scaling, rate unit, interval, and duration when supplied
- `SkillInvoke`: referenced current skill and supported invocation conditions
- `Control`: control kind and source-backed duration from the concrete Config or Effect entry

The seven Outcome types above are the internal Effect normalization contract, not the final Skill UI group list. A standalone Effect adapter may use its Outcome kind as its single group key. Skill composition aggregates normalized data by display role into `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, and `LinkedSkill`; it must not create one UI group per Effect. Optional Activation fields remain separate entries under `Activation`, while each normalized Outcome is routed to its owning Skill group. Do not introduce additional field-bundle groups such as `Scaling`, `Persistence`, `Constraints`, `CountAndScale`, or `SizeAndLifetime`.

### Legacy `SkillEffectSO` fallback

`SkillEffectSO` exposes generic trigger, target, value, duration, chance, and stack fields without a reliable semantic operation. Do not convert those fields into any normalized outcome. When its authored `Description` is non-empty, return a description-only fallback record. When the description is empty, return an unsupported record without inventing text.

This fallback is composed by the Skill domain in Stage 5. It is not a branch of `EffectPresentationResolver`.

## Single Effect Resolver Contract

```text
EffectEntrySO
-> EffectPresentationResolver.Resolve(...)
-> switch on EffectSO.Config
-> build optional Activation
-> build one normalized outcome
-> EffectPresentationData
```

The public behavior surface is one `EffectPresentationResolver`. Config branches remain inside it. Private methods are introduced only for repeated normalized-result construction, not to mirror every Config type.

Expected internal grouping:

- `CreateActivation(...)`
- `CreateStatModifier(...)`
- `CreateHeal(...)`
- `CreateCooldownChange(...)`
- `CreateDisplacement(...)`
- `CreatePeriodicDamage(...)`
- `CreateSkillInvoke(...)`
- `CreateControl(...)`
- `CreateDescriptionFallback(...)`

The Config classes remain gameplay definitions and do not gain `ToPresentationData()` methods or references to Presentation contracts.

## Normalization and Later Display Rules

- Normalized structures are domain data. Do not name them `ViewData`, `UIData`, `ValueOverrideView`, or `StackView`.
- Exclude runtime-irrelevant or structurally unclear values from the active normalization contract.
- Preserve the approved Activation and outcome structures. In the shared UI conversion, one source field becomes one `PresentationEntryData`; do not combine unrelated source values into one label/value row.
- Preserve the source number, source unit, and provenance. A formatter may render a Ratio as a percentage without changing normalized data.
- Do not clamp authored numbers, substitute minimums, convert Ratio data into Percent data, or synthesize counts.
- Action words such as pull, push, and knockback remain in authored descriptions. Normalized numeric labels use general concepts such as effect distance or effect range.
- Later UI output uses compact label/value pairs, for example separate `Reduction ratio: 20%`, `Reduction time: 1s`, or `Effect distance: 5m` rows. Full explanatory prose remains authored content.
- Inspection and validation output must retain source-visible raw/default values. `SkillPresentationGroupResolver.Resolve()` is the complete inspection path; only `ResolveForPlayerDisplay()` may omit player-irrelevant defaults or unbounded sentinels.
- Player-facing `desc` fields use required ordered `StringManager` lookup. The resolver probes candidate main keys without exposing failed candidates; if none resolve, normal lookup of the first intended key displays the full `*.desc` key for debugging. Structured Effect values must not be converted into invented description prose.
- Strategic Skills first query the exact `skill.strategic.*.desc` key, then the confirmed localization owner `item.strategic.*.desc`.
- Group, Entry, Tag, enum replacement, and value-format text use canonical `presentation.*` main keys with sub-key `name`. The player formatter never falls back to raw JSON/C# keys or generated Pascal-case words.
- The exhaustive source-field policy and localization mapping are authoritative in `AgentDocs/Machal/ability-content-presentation-display-catalog.md`.
- Never derive application count from interval and duration. Include a count only when a real applied source field such as `ApplyCount` exists and the runtime uses it.
- Do not expose `EffectEntrySO.ValueOverride` or effect upgrade modifiers as active values while the runtime resolver does not apply them.

## Nested Skill Contract

- A parent skill contains the nested skill name and a compact summary only.
- Do not flatten the nested skill's complete targeting, damage, movement, persistence, and Effect details into the parent.
- Detailed nested-skill presentation is resolved independently and opened as a separate content page in the later UI phase.
- Nested skill composition must guard against cycles and duplicate traversal.

## Value Rules Already Confirmed

- Do not infer units from field names alone.
- `StatModifierEffectConfig.Percent` runtime math treats the value as a `0..1` ratio.
- `ChanceOnHitStatModifierEffectConfig.Percent` runtime math treats the value as a `0..100` percentage.
- Heal max-HP and attack scaling values are multiplied directly and therefore behave as ratios.
- Chance-on-hit skill chance is compared with `Random.Range(0, 100)` and behaves as `0..100`.
- `EffectResolver` currently does not apply `EffectEntrySO.ValueOverride` or passed effect upgrade modifiers. Presentation must not apply them independently.
- Preserve each Effect as a separate structured record.

## Implementation Stages

### Stage 1 — Source and Asset Inventory

- Status: completed on 2026-08-08
- Owned paths: `AgentDocs/Machal/` only
- Work:
  - Freeze the working-tree baseline and approved paths.
  - Trace current Skill and Effect definitions through runtime resolution.
  - Count current SO assets, authoring JSON, reachable Effect entries, and unsupported asset cases.
  - Separate authoring declarations from values reachable through current runtime SO references.
- Exit evidence: `AgentDocs/Machal/ability-content-presentation-inventory.md`

### Stage 2 — Shared Neutral Contracts

- Status: completed on 2026-08-09
- Owned path: `Assets/Scripts/Presentation/`
- Add the smallest content-neutral identity, context, provenance, value, entry, group, and content contracts.
- Keep these contracts free of concrete Skill, Effect, Bless, Relic, and Character SO references.
- Verify a reachable placeholder first, then compilation and contract-level tests.
- Completion evidence:
  - `AgentDocs/code-writing-rules.md` was restored and read before code work.
  - A temporary `[PLACEHOLDER]` factory was reached through the smoke harness, then removed after final construction behavior replaced it.
  - Final smoke validation created identity, entries, a group, runtime provenance, and both Preview and Runtime contexts.
  - All seven `.cs` files have paired Unity `.meta` files.

### Stage 3 — Effect Normalization Model and Entrypoint

- Status: completed on 2026-08-11
- Data path: `Assets/Scripts/Ability/Effects/Data/`
- Behavior path: `Assets/Scripts/Ability/Effects/`
- Define Activation and semantic outcome data without final display strings.
- Add one `EffectPresentationResolver` dispatch entrypoint with internal Config switching and deterministic fallback behavior.
- Keep activation events distinct and represent Heal and Periodic Damage through separate typed outcomes.
- Completion evidence:
  - A temporary reachable placeholder was verified and then removed.
  - The isolated smoke result was `STAGE3_EFFECT_PRESENTATION_SMOKE_OK`.
  - Null, unsupported, description-only, provenance, entry-constraint, unit, trigger-separation, Heal, and PeriodicDamage behavior passed.
  - Unity regenerated `Assembly-CSharp.dll` with both new scripts included.
- Design and verification contract: `AgentDocs/Machal/ability-content-presentation-stage3-preparation.md`

### Stage 4 — Effect Config Mapping Branches

- Status: completed on 2026-08-11
- Batch A, verified against current linked assets: `StatModifier`, `Heal`, `CooldownChange`, `Displacement`, and `Control`.
- Batch B, source-code verified but without approved linked assets: triggered StatModifier variants, heal-triggered cooldown change, attack bleed, poison damage over time, distance displacement, and skill invocation.
- All thirteen Config classes dispatch inside the single `EffectPresentationResolver`; no Config-specific resolver hierarchy was added.
- Completion evidence:
  - `dotnet build Assembly-CSharp.csproj --no-restore` completed with 0 errors.
  - Before the source-fidelity correction, the Unity self-test passed all thirteen synthetic Config mappings.
  - Before the source-fidelity correction, all twenty `EffectEntrySO` assets under the approved paths resolved as Supported.
  - After semantic regrouping and Stun/Root specialization, `Assembly-CSharp-Editor.csproj` compiled with 0 errors and 156 existing warnings. The self-test now covers fifteen cases across thirteen Config classes; current Unity rerun remains pending.
  - User test menu: `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`.
- Asset-level verification remains pending for the eight Config types without approved linked assets; their coverage is source-level synthetic validation.

### Stage 5 — Skill Composition

- Status: code complete on 2026-08-11; nested Skill traversal and detail expansion deferred by user decision

- Data path: `Assets/Scripts/Ability/Skills/Data/`
- Behavior path: `Assets/Scripts/Ability/Skills/`
- Keep identity and classification as content metadata. Preserve JSON field provenance, but aggregate visible Skill entries into five semantic groups: `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, and `LinkedSkill`.
- Route cast/target/trigger/chance conditions to `Activation`; projectile, movement, burst, hit cadence, and delivery geometry to `Delivery`; damage/heal/stat/cooldown/periodic/spawn results to `Outcome`; `Control` and `Displacement` to `SpecialEffect`; and `SkillInvoke` to `LinkedSkill`.
- Keep each source field as its own entry and value. Semantic grouping does not authorize combining fields, deriving values, or replacing source numbers.
- Distinguish direct-SO preview values from `EquipmentSkillRuntimeData` values with provenance.
- Preserve multiple hits and Effects. Do not traverse nested skills; retain only referenced identity and detail content ID where available.
- Handle ambiguous legacy `SkillEffectSO` only here as an authored-description fallback; do not introduce an Effects-to-Skills dependency.

### Stage 6 — Approved-Asset Validation

- Status: the null-slot-corrected user Unity run previously passed all 58 approved current Skill assets; the latest player-display catalog now requires a user rerun

- Validate current results against all reachable approved runtime SOs.
- Cover skills with no hit, no Effect, one Effect, multiple Effects, unsupported config, and differing unit scales.
- Record JSON-to-generated-SO mismatches without repairing or migrating assets in this task.

### Stage 7 — Character, Bless, and Relic Adapters

- Status: current-definition adapters complete; approved current asset validation remains pending for all three domains

- Source-level adapters are implemented by the user's explicit Stage 7 request; asset validation still requires an approved current path for that domain.
- Reuse normalized Effect results and add only domain-owned identity or runtime state.
- Keep Character, Bless, and Relic asset-level results pending while only their source definitions, not approved current assets, are confirmed.

### Stage 8 — Data-Layer Approval and UI Handoff

- Status: explicit player-display allowlist, StringManager catalog, compact formatter, generic View scripts, hierarchy AutoBind, semantic default filtering, and Character/Skill/Bless/Relic presenters complete; localized user visual validation remains pending

- The supported, fallback, pending, and unverified matrix is recorded in `AgentDocs/Machal/ability-content-presentation-stage5-8-completion.md`.
- Compact formatting and generic View scripts are implemented.
- `PresentationDisplayCatalog` owns the explicit player-visible Group, Entry, Tag, enum-replacement, and format-key mapping. The default formatter keeps raw keys for inspection. The player formatter omits unapproved raw keys with no catalog mapping, but a mapped localization key missing from StringManager remains visible as the full intended key instead of becoming generated fallback text.
- The user attached the four View components. Hierarchy fields now follow `AutoBindPrefix` and `AutoBind`; Unity must refresh, run `OnValidate`, and save the prefabs before those serialized references are confirmed.
- `tagPrefab`, `groupPrefab`, and `entryPrefab` remain manual asset assignments. The previous approved Skill validation predates the player-display catalog; the current rerun is pending.
- Character, Skill, Bless, and Relic presenters bind their assigned current definitions into assigned existing `UIContentInfoView` instances in Play Mode.
- Bless and Relic presenters also expose runtime-entry overloads so an owning gameplay UI can display actual runtime state without putting domain interpretation in the shared View.
- `SkillContentInfoPresenter` remains the Skill-owned integration boundary. Merging its SO field or build action into `UIContentInfoView` would violate the neutral Presentation dependency direction unless the user explicitly chooses a different ownership design.
- Existing Editor inspection and validation tools continue using complete `Resolve()` output; only the Skill presenter uses `ResolveForPlayerDisplay()`.
- The temporary Editor menu that created an overlay Canvas was removed. Scene integration remains separate follow-up work; group/entry label data now lives in `Assets/Resources/string/presentation_string.csv`.

## Deferred Scope

- Final ownership decision for `SkillContentInfoPresenter` versus the neutral `UIContentInfoView`
- User Unity wheel/drag verification and any necessary transparent raycast-target `Image` on the ScrollRect Viewport
- Content-specific prefab behavior beyond the implemented Skill presenter and current generic hierarchy/template boundary
- Scene integration
- Missing authored description rows and any additional label/token rows discovered by asset validation
- Legacy asset migration
- Bless/Relic asset creation or repair
- Combat behavior changes
- Large namespace or folder reorganization

## Separate Folder-Migration Boundary

The architecture reference describes the target ownership layout but does not authorize a whole-project migration in this task.

If a later task moves scripts:

- Move folders and files without changing class names, namespaces, or behavior in the same unit.
- Move every Unity `.cs.meta` with its `.cs` file to preserve GUIDs.
- Preserve existing modified and untracked work.
- Do not move or delete unrelated non-script files such as `Assets/Scripts/shring.zip`.
- Verify old-path residue, missing or duplicate files, GUID preservation, references, and Unity compilation.

## Separate SO/JSON Boundary

For a later explicitly authorized content-authoring task:

```text
Assets/Contents/<Content>/*.json
= authoring source

Assets/Contents/<Content>/Generated/*.asset
= generated runtime SO
```

`Assets/Contents` contains only source JSON and generated SO assets. Do not mix this future content-layout work with the current Presentation data task or script-folder refactor.

## Work Order

1. Stage 1 inventory and boundary verification — completed.
2. Shared neutral contracts — completed.
3. Effect normalization model and single dispatch entrypoint — completed.
4. Effect Config mapping branches and user-runnable Unity self-test — implementation complete; current fifteen-case Unity rerun pending after semantic regrouping and special-effect correction.
5. Skill composition — code complete; nested Skill traversal and detail expansion deferred by user decision.
6. Approved-asset validation tooling — complete; the previous 58-Skill PASS predates the latest player-display catalog and the current user rerun is pending.
7. Current-definition Character, Bless, and Relic adapters — code complete; approved current asset validation pending.
8. Player-display catalog, compact formatter, and generic View handoff — strict StringManager catalog lookup, Character/Skill/Bless/Relic presenters, semantic filtering, and scroll refresh complete; final Skill presenter ownership plus user-owned localized visual and scroll-input validation pending.

## Verification Matrix for the First Delivery

- Current Effect types present in approved asset paths
- Single Effect skill
- Multiple Effect skill
- Skill without Effect
- Preview data
- Runtime-resolved skill values where available
- Different percentage scales
- Unsupported config fallback
- Unclassified skill without inferred values

For every case, compare:

```text
Approved SO or runtime source
-> owning content resolver or builder
-> shared Presentation data
```

## Handoff State

The next agent must begin with `AgentDocs/Machal/README.md` and `AgentDocs/Machal/owned-effects-inventory-task-start.md`. No implementation should begin from this document alone without following the exact reading order, work guide, and task log recorded by the start contract.

## 2026-08-13 Panel Character Navigation Extension

- `CharacterSkillContentInfoPresenter` now owns an ordered CharacterSO list and previous/next selection for `Panel_CharacterInfo`.
- A selection refreshes both the Character body and the selected Character's Skill icon tabs through the existing domain presenters; no presentation-data or localization ownership moved into the prefab.
- The old single Character field remains a one-item fallback, and the old initial Skill index is serialization-compatible.
- `Panel_CharacterInfo.prefab` references its existing Character presenter and disables the latter's independent startup build so Character selection has one startup owner.
- Source compilation passed with 0 errors and 35 existing warnings.
- No Button object or event was created. The user owns Button creation, event connection, Character list population, prefab save, and Play Mode validation.

## 2026-08-14 Bless List Presenter Extension

- The Faith-page and Bless-category composition remains deferred.
- `BlessContentInfoPresenter` owns an inspector-configured `List<BlessSO>`, creates one reusable `UISelectableIconButton` per non-null Bless, and binds the selected Bless through the existing `BlessPresentationResolver` into `UIContentInfoView`.
- `UIContentInfoView` remains content-neutral and displays only the currently selected normalized result.
- The old `SetBless`, `ShowBless`, `BuildPresentation`, and `Bless` access paths remain available for callers that supply one Bless directly.
- Unity prefab wiring and Play Mode validation remain user actions.

## 2026-08-14 Deferred Faith Page Correction

- Selecting a god tab on the future Faith page changes the entire god-information page to that god's Faith feature set, following the same ownership pattern as Character selection and its Skill tabs.
- The future Faith-page Presenter owns the god list, selected god, and heterogeneous Faith feature tabs. It delegates Bless features to the Bless presentation path and the Exclusive Advancement feature to its own domain adapter; `BlessContentInfoPresenter` must not own the whole Faith page.
- Each god owns four Faith features: one Basic Bless that becomes stronger with Faith level, one job-group Exclusive Advancement, Exclusive Bless 1 acquired when Faith is locked, and Exclusive Bless 2 acquired upon reaching Faith level 8 after Faith lock.
- The three Bless features are separate Blesses. The Basic Bless's changing strength is progression state and must not be represented as multiple unrelated Bless tabs. Exclusive Advancement is not a `BlessSO`.
- Exclusive Bless 2's confirmed acquisition condition is Faith level 8 after Faith lock. This rule must be represented explicitly rather than inferred from the current `successorFaithLevel` field or source method names.
- Future Faith feature adapters must preserve authored identities and progression. They must not infer the four feature slots from names or list order.
- Current source mismatch to resolve before implementation: the current model exposes only `ShrineBlessingGroup.Base/Enhanced`; `ShrineGodSO.GetAvailableBlessings` does not use its group argument; and `BlessPoolEntry` only stores `progressionStep`. No runtime behavior is changed in the current task.

### Deferred Faith Prefab Preparation

- Prepare one composition panel, `Panel_FaithInfo`, containing a god-tab root, selected-god header/progress area, `BlessContentInfoTabRoot`, and one existing `UIContentInfoView_Bless` instance renamed `BlessContentInfoView` for AutoBind.
- Prepare one reusable god icon-tab prefab and one reusable Faith feature icon-tab prefab. Both may use `UISelectableIconButton`; do not make separate detail prefabs for the four feature types.
- The Faith feature tab prefab should include `UI_IconImage`, `UI_SelectedFrameImage`, and `UI_LockedOverlay`. The lock visual is needed for Exclusive Advancement and Exclusive Bless 1/2 even though acquisition-state binding is not implemented yet.
- All four Faith features render into one neutral selected content View. Bless features use Bless presentation data; Exclusive Advancement requires its own adapter.
- General/Common Blesses are not owned by a selected god and remain outside this deferred selected-god prefab contract.

### Confirmed Owned-Effect and Codex Page Ownership

- Authoritative gameplay rule: every owned Relic and every acquired General Bless applies immediately; neither uses an equipment state. Faith is progression state with unlock and level-up behavior.
- Keep the full Faith encyclopedia page separate because it explains god selection, Faith level, lock state, Basic Bless scaling, Exclusive Advancement, and Exclusive Bless unlock progress.
- Convert the current Relic-page screen role into one tabless `Owned Effects` page.
- The page uses one vertical scroll and dynamically creates categorized sections for every currently applied owned Relic, acquired General Bless, and active Faith Bless. It is always owned/active-only and never exposes a `Catalog` option.
- Relic and General Bless catalogs are separate pages that may reuse the common category/item system in `Catalog` mode. Full Faith progression, inactive features, and future unlocks remain in the separate Faith encyclopedia.
- The Owned Effects Faith section excludes Exclusive Advancement unless a future explicit Effect source is authored for it.
- Selecting any inventory item displays its details in the single right-side neutral `UIContentInfoView`.
- Do not create another owned-only Bless page. The Owned Effects page contains acquired General Blesses, while the separate General Bless catalog may show acquired and unacquired definitions.
- A future Relic encyclopedia is a separate page that shows both acquired and unacquired Relics. Preserve the existing lock/silhouette/count-oriented `RelicCollectionView` for that future role rather than generalizing it into the Owned Effects View.
- Current code mismatch to reconcile before implementation: `RelicItemService` still exposes owned/equipped lists, and `BlessManager.AddBless` replaces an existing permanent Common Bless. UI must follow the authoritative design after runtime ownership rules are corrected, not preserve these stale behaviors by assumption.

Current implementation status:

- `ContentInventoryData`, `ContentInventoryItemView`, and `ContentInventoryCategoryView` provide the neutral data, item, and non-scrolling category-section layer.
- `OwnedEffectInventoryView` and `OwnedEffectInventoryPresenter` now implement the tabless category page. The Presenter accepts configured Preview definitions or explicit runtime lists for owned Relics, active General Blesses, and active Faith Blesses; automatic Manager collection is not implemented.
- The user explicitly authorized direct component wiring only for `Assets/Prefabs/UI/Fixed/Panel/Panel_OwnedEffects.prefab`, `Assets/Prefabs/UI/Fixed/Content/UIContentInventoryCategory.prefab`, and `Assets/Prefabs/UI/Fixed/Content/UIInventoryItemView.prefab` in the 2026-08-18 work unit. All required serialized references were assigned, the panel-local `RelicContentInfoPresenter` was removed, and the shared detail remains active. This one-off authorization is not permission for later prefab YAML edits or Unity operation.
- Existing `RelicCollectionView` remains preserved for the separate Relic encyclopedia. Unity import, Inspector deserialization, configured source assignment, category/item interaction, detail binding, outer scrolling, localization, and visual layout remain user-owned validation.

## 2026-08-17 Detailed Faith Page Design

The authoritative prefab, data, ownership, interaction, implementation-order, and validation contract is now `AgentDocs/Machal/faith-page-design.md`.

- The page uses acquired-Faith tabs, a selected-god summary, a level 1-10 roadmap, exactly four reusable feature cards, and one neutral selected-detail View.
- `FaithPagePresenter` and `ShrineFaithPresentationResolver` replace the earlier plan for `BlessContentInfoPresenter` to own the Faith page.
- Basic and Exclusive Bless detail delegates to `BlessPresentationResolver`; Exclusive Job Change uses explicit Character job-family/target-job source data.
- Locked future features remain readable as Preview and are never labeled as active Runtime effects.
- Required prefab kinds: `Panel_FaithInfo`, `UIFaithGodTab`, `UIFaithLevelNode`, `UIFaithFeatureCard`, and an optional `UIContentInfoView_Faith` layout variant.
- Implementation remains blocked on the Exclusive Job Change unlock rule, target-job mapping, current Faith/Bless authoring path, and explicit four-feature progression definition.

## 2026-08-17 Category-Section Content Inventory Refactor

- The final inventory layout uses one vertical page `ScrollRect`. Ordered category sections and their item grids are created inside that one scroll content; categories do not own nested vertical `ScrollRect` instances.
- Relic, General Bless, and active Faith Bless keep separate domain presenters for source and ownership interpretation. A content-neutral page layer owns section creation, cross-section selection, category filtering, and the one shared `UIContentInfoView`.
- The same category and item system supports `OwnedOnly` and `Catalog` construction modes across different pages. Runtime ownership filtering remains a domain-presenter responsibility; the common View only renders the supplied acquisition state.
- The Owned Effects page has no tabs and requests owned/active-only Relic, General Bless, and Faith Bless sections. Separate Relic and General Bless catalog pages may request `Catalog`; the Faith encyclopedia owns the Faith catalog/progression role.
- Phase 1 added `ContentInventoryData.cs` beside the existing `OwnedEffect` scaffold without replacing current behavior. Its neutral contract contains page, category, and item snapshots plus `OwnedOnly`/`Catalog` and `Owned`/`Unowned`/`Locked` state.
- Category identity is a neutral string ID plus a StringManager localization key. The common contract does not define Relic- or Bless-specific category enums.
- Static solution compilation passed with 0 errors and 197 existing warnings after temporary project inclusion; the generated project file was restored. Unity import and runtime behavior were not tested in this phase.

## 2026-08-18 General Bless Encyclopedia Phase 2

- The first integrated catalog target is the separate General Bless encyclopedia, not the tabless Owned Effects page.
- The caller may provide either a complete `IReadOnlyList<BlessSO>` or one `BlessPoolSO`. Every non-null unique Bless definition from that source remains visible; Pool weight and progression step are generation metadata and are not inventory display fields.
- A separate active `BlessRuntimeData.BlessEntry` list marks matching definitions active by authored `BlessingId`. Inactive definitions remain visible and selectable.
- Added `ContentActivationState.Inactive/Active` to the neutral item contract without combining active state with selected, acquired, or locked state.
- Added `ContentInventoryItemView` and `ContentInventoryCategoryView`. The item has separate selected, locked, and active visuals. The category binds the localized title, count, and generated item Grid and intentionally owns no `ScrollRect`, concrete SO, Resolver, Manager, or detail View.
- Clean static solution compilation passed with 0 errors and 197 existing warnings after temporary project inclusion; the generated project file was restored. Unity import, AutoBind, prefab, active visual, and Play Mode behavior are not yet verified.

## 2026-08-18 Owned Effects Direct Prefab Wiring

- Replaced the earlier four-tab `OwnedEffectInventoryView` and `OwnedEffectInventoryPresenter` behavior with one vertical-scroll page that creates ordered `OwnedOnly` sections for Relic, General Bless, and active Faith Bless content.
- Attached `ContentInventoryItemView`, `ContentInventoryCategoryView`, `OwnedEffectInventoryView`, and `OwnedEffectInventoryPresenter` to the three user-prepared prefabs and assigned their required serialized references.
- Removed only the panel-local `RelicContentInfoPresenter`; its source and dedicated-page role remain unchanged. The shared `UIContentInfoView` stays active and content-neutral.
- Added four `presentation.inventory.*` StringManager rows for the page and category titles.
- Direct prefab YAML wiring was a one-off user authorization for these three prefabs. Future prefab YAML edits or Unity operation require a new explicit request.
- Reported and statically confirmed verification: `dotnet build ProjectBS.sln --no-restore -v:minimal` passed with 0 errors and 197 existing warnings; script attachment, required-reference, old-presenter removal, shared-detail active-state, and localization uniqueness checks passed.
- User Unity validation is next. Automatic runtime Manager collection and separate encyclopedia pages remain later work units.

## 2026-08-21 Faith Roadmap Current/Next Comparison Correction

- The area below the Faith growth roadmap is no longer four standalone feature cards plus one selected-detail View.
- It contains exactly two equal-role instances of `UIFaithLevelEffectCard`: the actual current-level Faith-effect card and the immediate-next-level Faith-effect card.
- The current card shows the complete currently applied Faith feature set. The next card shows the complete authored next-level result and classifies source-identical entries as `Strengthened`, source-new entries as `NewlyUnlocked`, or unchanged entries as `Unchanged`.
- Comparison uses stable authored feature/Entry IDs and exact current/next values. Presentation must not compare localized labels, calculate invented delta values, or infer a strengthening relationship when source identity is insufficient.
- At maximum level the next-card position remains present with the localized no-next-level empty state. Future roadmap nodes communicate milestones; they do not replace the actual current/next comparison pair.
- The authoritative prefab and data contract is `AgentDocs/Machal/faith-page-design.md`. No runtime source, prefab, Scene, asset, build, staging, commit, or push action was performed.

## 2026-08-21 Faith Main Panel Scaffold Implementation

- Directly rebuilt `Assets/Prefabs/UI/Fixed/Panel/Panel_FaithInfo.prefab` under the user's explicit prefab-edit authorization.
- Preserved the panel background/foreground, removed the legacy `FaithDetailView` and incomplete body/roadmap/description objects, and created the acquired-Faith tab strip, selected-god summary, ten-node horizontal roadmap, and embedded current/next level effect cards.
- Attached and assigned `FaithPageView`, `FaithPagePresenter`, one `FaithGodTabView` template, ten `FaithLevelNodeView` instances, and two `FaithLevelEffectCardView` instances. Each effect card contains an existing neutral `UIContentInfoView` prefab instance.
- Added configured-God Inspector sources and a `Build Configured Faith Page` ContextMenu call path. Current/next Faith comparison data remains an explicit `[PLACEHOLDER]` until source-backed progression data and `ShrineFaithPresentationResolver` exist.
- Corrected shared AutoBind lookup to support `[AutoBind] GameObject` fields without passing `GameObject` to `GetComponent(Type)`.
- Added thirteen unique `presentation.faith.*` localization rows.
- Static prefab validation passed: all new serialized component references are non-null, ten level nodes exist, both controlled horizontal layouts are configured, and legacy `FaithDetailView` residue is absent.
- Unity Editor static visual inspection confirmed the new hierarchy and even ten-node roadmap layout. `dotnet build ProjectBS.sln --no-restore -v:minimal` passed with 0 errors and 209 existing warnings. Play Mode data/interaction validation remains user-owned.
