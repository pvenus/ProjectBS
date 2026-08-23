# Stages 5-8: Skill Composition and UI Handoff

## Status

- Code implementation: complete on 2026-08-11
- Nested Skill traversal and detail expansion: deferred by user decision
- Runtime and Editor C# compilation: passed without errors
- Unity asset validation: the null-slot-corrected user rerun previously passed all 58 approved Skills; the player-display catalog change requires a current user rerun
- Prefab components: attached by the user; hierarchy AutoBind code added after the user identified the missing attributes
- Legacy Effect, Bless, and Relic assets: unchanged and excluded

## Stage 5: Skill Composition

Implemented:

- `Assets/Scripts/Ability/Skills/Data/SkillClassificationPresentationData.cs`
- `Assets/Scripts/Ability/Skills/Data/SkillPresentationData.cs`
- `Assets/Scripts/Ability/Skills/SkillPresentationResolver.cs`
- `Assets/Scripts/Ability/Skills/SkillPresentationGroupResolver.cs`
- `Assets/Scripts/Ability/Effects/EffectPresentationGroupResolver.cs`

The Skill resolver keeps identity and classification as content metadata. The seven typed Effect Outcomes remain the internal normalization contract, while the final Skill display aggregates entries into five role groups: `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, and `LinkedSkill`. This removes one-group-per-Effect fragmentation without combining source fields: every source field remains a separate entry with one value. Preview values and runtime-resolved values carry distinct provenance; multiple hits and Effects remain preserved in the typed Skill data before display aggregation.

`Control` and `Displacement` route to `SpecialEffect`. Stun and Root use `Control` with `config.Value` as the source-backed duration; Taunt uses the Effect Entry duration when supplied. Knockback and pull use `Displacement`, preserving direction and either Force or Distance according to the concrete Config. `SkillInvoke` routes to `LinkedSkill`.

The following nested behavior is deferred:

- Do not traverse `SkillHitSO.SpawnSkill`.
- Do not merge a spawned or invoked Skill's full detail into its parent.
- `SkillInvoke` may retain a referenced identity and detail content ID, but it does not resolve the referenced Skill.

Ambiguous `SkillEffectSO` data is exposed only through `SkillPresentationResolver.ResolveLegacyEffect`. It returns authored description and tags when available and never normalizes value, duration, chance, or stack fields.

## Stage 6: Approved Skill Asset Validation

Implemented user-run tools:

- Full matrix: `Tools > ProjectBS > Presentation > Run Skill Asset Validation`
- Selected asset log: `Assets > ProjectBS > Presentation > Log Selected Skill`
- Interactive inspector: `Tools > ProjectBS > Presentation > Open Skill Data Preview`

The full matrix scans only:

- `Assets/Resources/skill/character/generated/`
- `Assets/Resources/skill/json/`

It covers no-hit, no-Effect, one-Effect, multiple-Effect, unsupported Effect, preview/runtime provenance, Ratio/Percent unit separation, and deferred nested references. Stage 1 JSON/SO mismatch evidence is retained without repair or migration.

The tool compiled successfully. The first Unity execution was performed by the user; the agent did not operate Unity.

The first user run reported ten unsupported Effect records. Investigation showed that all ten were null `EffectEntrySO` slots serialized as `{fileID: 0}` across nine Cast assets, not unsupported concrete Effects. This matches the Stage 1 inventory and the gameplay `EffectResolver.ResolveEntries` behavior, which skips null entries. `SkillPresentationResolver` now also skips null slots, and the validation report counts them separately as `Ignored null EffectEntry slots`.

The null-slot-corrected user rerun previously passed:

- `Approved unique Skill paths: 58`
- `Resolved Skills: 58`
- `No hit / no Effect / one Effect / multiple Effects: 7 / 42 / 14 / 2`
- `Supported / description-only / unsupported Effects: 18 / 0 / 0`
- `Ignored null EffectEntry slots: 10`
- `Ratio / Percent values: 120 / 102`
- `Failures: 0`

This PASS predates the current player-display catalog. The current Effect self-test and Skill asset validation remain pending user rerun.

## Stage 7: Character, Bless, and Relic Adapters

Implemented:

- Character: `Assets/Scripts/Actor/Character/Data/CharacterPresentationData.cs` and `Assets/Scripts/Actor/Character/CharacterPresentationResolver.cs`
- Bless: `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/Data/BlessPresentationData.cs` and `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessPresentationResolver.cs`
- Relic: `Assets/Scripts/Collection/Relic/Data/RelicPresentationData.cs` and `Assets/Scripts/Collection/Relic/RelicPresentationResolver.cs`

All adapters support definition preview and runtime-state overloads. Bless and Relic reuse `EffectPresentationResolver` and `EffectPresentationGroupResolver`.

Validation state:

- Character/Bless/Relic source-level adapter compilation: complete
- Approved current Character asset validation: pending
- Approved current Bless asset validation: pending
- Approved current Relic asset validation: pending
- Legacy Bless/Relic paths remain excluded and were not read as authoritative current data

## Stage 8: Semantic Text and Generic View Handoff

Implemented:

- `Assets/Scripts/Presentation/PresentationValueData.cs`
- `Assets/Scripts/Presentation/PresentationTextFormatter.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoGroupView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoEntryView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoTagView.cs`
- `Assets/Scripts/Ability/Skills/UI/SkillContentInfoPresenter.cs`
- `Assets/Scripts/Actor/Character/ui/CharacterContentInfoPresenter.cs`
- `Assets/Scripts/Stage/NodeContents/Shrine/UI/BlessContentInfoPresenter.cs`
- `Assets/Scripts/Collection/Relic/UI/RelicContentInfoPresenter.cs`

The formatter converts explicit units to compact output and supports external label/token resolvers for later localization. The generic Views consume only `ContentPresentationData`; they do not interpret Skill, Effect, Bless, Relic, or Character SO fields.

### User-owned Unity binding

The user attached the four View components. The agent then corrected the scripts to follow the existing hierarchy AutoBind convention:

1. `UIContentInfoTagView`: `AutoBindPrefix("Tag")`; `text` binds to `Tag_Text`.
2. `UIContentInfoEntryView`: `AutoBindPrefix("Entry")`; hierarchy fields bind to the matching `Entry_*` objects.
3. `UIContentInfoGroupView`: `AutoBindPrefix("Group")`; hierarchy fields bind to the matching `Group_*` objects.
4. `UIContentInfoView`: `AutoBindPrefix("Info")`; hierarchy fields bind to the matching `Info_*` objects.

`tagPrefab`, `groupPrefab`, and `entryPrefab` are prefab asset references rather than child components. The current AutoBind utility does not resolve asset references, so the user must assign those three fields manually. After the script reload, open or validate and save all four prefabs, then confirm no missing hierarchy reference or Console compile error.

No Scene binding is required for the data-layer completion. Generic hierarchy AutoBind and the Skill-specific presenter entrypoint are implemented; broader Scene integration remains separate. Group/entry label data is now managed in `Assets/Resources/string/presentation_string.csv`.

### User-run Skill UI presentation

The temporary Editor preview tool was retired. `SkillContentInfoPresenter` now sends its assigned `EquipmentSkillSO` through `SkillPresentationResolver`, `SkillPresentationGroupResolver`, and the assigned `UIContentInfoView.Bind()` without creating UI or a Canvas.

This is the current ownership boundary, not a final merge decision. Keep the concrete Skill SO and `Build Presentation` action in `SkillContentInfoPresenter`; moving them into neutral `UIContentInfoView` would reverse the approved dependency direction and requires explicit user approval.

- Add `SkillContentInfoPresenter` to the user-owned content-information UI root.
- Assign its `UIContentInfoView` and `EquipmentSkillSO` fields. `UIContentInfoView` can AutoBind when a matching child is named `UIContentInfoView`.
- Enter Play Mode, open the component context menu, and run `Build Presentation`.
- Leave `Use Runtime Values` disabled for authored preview. Enable it and set levels for runtime-resolved preview.
- An active Scene `EventSystem` is required for `ScrollRect` input. The presenter reports a warning when it is absent.

The presenter does not create or save UI. Unity execution and visual evaluation remain user-owned.

### Owned Effect inventory and separate encyclopedia ownership

`BlessContentInfoPresenter` belongs to the existing Shrine UI domain, and `RelicContentInfoPresenter` belongs to the existing Relic UI domain. Each presenter sends definition Preview data through its domain resolver's `ResolveForPlayerDisplay()` path into an assigned existing `UIContentInfoView`; runtime-entry overloads use the same player-display path with `PresentationContext.Runtime`.

Every acquired General Bless and every owned Relic applies immediately. Neither content type has an equipment state in the authoritative design, so their information UI must not filter by or present equip/unequip state. `BlessContentInfoPresenter` may own a standalone General-Bless list and selection; `RelicContentInfoPresenter` may display one owned Relic at a time. The shared View owns neither gameplay inventory nor selection.

Current runtime source does not yet match that design. `RelicItemService` still exposes `EquippedRelics`, and `BlessManager.AddBless` removes an existing permanent Common Bless before adding a new one. Treat these as implementation gaps, not player-display rules. Reconcile runtime ownership before building a final collection UI.

The current Relic-page screen role is finalized as one tabless Owned Effects inventory. One vertical scroll contains categorized sections for every currently applied owned Relic, acquired General Bless, and active Faith Bless; this page is owned/active-only and cannot use `Catalog`. Relic and General Bless catalogs are separate pages that may reuse the common category/item system in `Catalog` mode. Full Faith progression and future unlocks remain in the Faith encyclopedia. Exclusive Job Change remains excluded unless a future explicit Effect source is authored. Selecting any item binds its detail to one neutral `UIContentInfoView`. Do not create another owned-only Bless page.

The active implementation uses neutral `ContentInventoryData`, `ContentInventoryItemView`, and `ContentInventoryCategoryView` together with the tabless `OwnedEffectInventoryView` and `OwnedEffectInventoryPresenter` under `Assets/Scripts/Presentation/SharedUI/Content/`. The Presenter accepts configured Preview definitions or explicit runtime lists, creates ordered `OwnedOnly` sections, and leaves automatic Manager-source collection to a later unit. `OwnedEffectInventoryData` and `OwnedEffectGridItemView` remain unwired legacy units and must not restore the former four-tab behavior.

The user explicitly authorized direct component wiring for `Panel_OwnedEffects.prefab`, `UIContentInventoryCategory.prefab`, and `UIInventoryItemView.prefab` in this work unit. All required references are serialized, only the panel-local `RelicContentInfoPresenter` was removed, and the shared detail remains active. This is a one-off authorization, not a general permission for later prefab YAML edits or Unity operation. Unity import, Inspector confirmation, source assignment, interaction, scrolling, localization, and visual validation remain user-owned.

Preserve `RelicCollectionView` for a future separate Relic encyclopedia that shows acquired and unacquired Relics. Its locked silhouettes and owned/total counts belong to that codex role; do not generalize it into the Owned Effect inventory. The user-prepared `Assets/Prefabs/UI/Fixed/Panel/Panel_RelicInfo.prefab` was not modified and still requires user Unity replacement and wiring.

### Separate Faith page ownership

Faith is a separate unlock and level-progression system. Each god owns four heterogeneous features: a Faith-scaling Basic Bless, a job-family Exclusive Job Change that is not `BlessSO`, Exclusive Bless 1 acquired at Faith lock, and Exclusive Bless 2 acquired at Faith level 8 after lock. The three Bless features may reuse `BlessPresentationResolver`; Exclusive Job Change requires Character job data and a Faith-owned adapter.

`FaithPagePresenter`, not `BlessContentInfoPresenter` or `UIContentInfoView`, owns acquired-Faith tabs, selected god, the level 1-10 roadmap, feature selection, and page composition. Locked features remain readable as Preview. General Blesses stay outside selected-god ownership.

The authoritative detailed page and prefab contract is `AgentDocs/Machal/faith-page-design.md`. It supersedes the earlier three-Bless and Bless-tab-only preparation model. Current source does not explicitly encode Exclusive Job Change or the four feature slots, so runtime and prefab binding remain deferred. Do not infer missing roles, job mappings, unlocks, or scaling from names or list order.

### Semantic display filtering and scrolling correction

`SkillPresentationGroupResolver.Resolve()` remains the complete inspection path used by the Skill Presentation Editor tool and validation. It retains source-visible values such as `0`, `999`, default count/scale, and disabled/applied flags. `ResolveForPlayerDisplay()` is the player-UI path: it omits zero time/distance/damage values, default projectile count and scale, `999` unbounded sentinels, default-disabled critical/defense labels, and empty hit groups. The Skill presenter uses only the filtered method. This does not erase data from the inspection model or unrelated Effect outcomes.

`UIContentInfoView.Bind()` now disables stale generated children before deferred destruction, forces layout rebuilding after new groups are created, stops existing scroll movement, and resets the view to the top. Scroll input still depends on an active Scene `EventSystem`. Static prefab inspection also found no raycastable `Graphic` on the Viewport, so wheel and drag must be tested over areas without an active child Graphic. Add a transparent raycast-target `Image` only if the user confirms the input gap in Unity.

## Runtime Accuracy Boundaries

- `EquipmentSkillRuntimeData` stores resolved range, burst, projectile count, spread, arrangement value, and scale. Other fields remain authored-asset values even in runtime presentation and retain authored provenance.
- Current `EquipmentStatResolver` reads the first `SkillHitSO` as the base for resolved damage and max-hit modifiers. Runtime presentation mirrors that behavior for every resolved hit instead of inventing per-hit upgrade resolution.
- `EquipmentSkillResolver` does not copy `FirstHitBaseDamage` into the runtime damage DTO. It is preview-only and omitted from runtime presentation.
- Current runtime copies `SplitHitCount` without carrying `UseSplitMultiHitDamage`; runtime presentation exposes the values the runtime receives. This gameplay discrepancy is unchanged.
- Nested Skill traversal is deferred. No cycle traversal code was added.
- Identity fallback uses the Unity asset name when `StringManager` is unavailable. Descriptions use required ordered `StringManager` lookup. Strategic `EquipmentSkillSO` IDs query their exact `skill.strategic.*.desc` key first, then the confirmed `item.strategic.*.desc` owner key; if all candidates fail, the first intended full key remains visible. Raw Group, Entry, Tag, and token keys remain inspection/provenance data; player text uses only explicit `PresentationDisplayCatalog` mappings to canonical `presentation.*` rows in `Assets/Resources/string/presentation_string.csv`.

## Verification

- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`: 0 errors
- Stage 5-8 build: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal` completed with 0 warnings and 0 errors on that run
- AutoBind correction build: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal` completed with 0 errors and 191 existing project warnings
- Stage 6 null-slot correction build: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal` completed with 0 errors and 191 existing project warnings
- Previous Stage 6 Unity validation before the latest contract corrections: PASS for 58 Skills, 18 supported Effects, 0 unsupported Effects, 10 ignored null slots, and 0 failures
- Latest semantic-regrouping Editor assembly build: 0 errors and 156 existing warnings
- Latest static content checks: 140 localization data rows with zero case-insensitive duplicate key pairs; all 20 strategic Skill JSON files parse as strict UTF-8 JSON
- Presenter/filter/scroll correction build with the new source explicitly included: `dotnet build Assembly-CSharp.csproj --no-restore` completed with 0 errors and 35 existing project warnings
- Description localization correction build: `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:minimal` completed with 0 errors and 35 existing project warnings
- Bless/Relic Presenter build: `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal` completed with 0 errors and 35 existing project warnings
- Bless list Presenter build: `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal` completed with 0 errors and 35 existing project warnings
- Strategic description-key audit: all 20 approved `skill.strategic.*` IDs matched an `item.strategic.*.desc` row
- Static approved-asset inventory: ten null `EffectEntrySO` slots across nine Cast assets; `unyielding_will` contains two slots
- Static prefab inspection: all four View script GUIDs are attached and all fourteen required prefixed hierarchy target names exist
- Current prefab YAML static check: thirteen of seventeen required references are assigned; `UIContentInfoEntry` label, value, and detail-button fields plus `UIContentInfoTag` text remain null until user validation or `OnValidate` and prefab save
- Stage 5-8 implementation `.meta` scan: each of the 22 GUIDs appears exactly once under `Assets/`
- Presenter `.meta` GUID `6c063f32913a4de298db20334f5c9b2a` is assigned; Unity import and visual behavior remain user-owned validation
- Scoped `git diff --check`: passed
- Placeholder scan: no `[PLACEHOLDER]`, `TODO`, or `FIXME` remains in the new implementation
- Unity Editor was not opened or controlled by the agent

## Supported / Pending Matrix

| Area | State |
| --- | --- |
| Current Skill preview composition | Implemented; previous 58-Skill PASS predates the player-display catalog, current user rerun pending |
| Current Skill runtime composition | Implemented; previous 58-Skill PASS predates the player-display catalog, current user rerun pending |
| Current Effect typed normalization | Implemented; seven Outcomes retained |
| Skill semantic role grouping | Implemented; five aggregate groups replace per-Effect UI groups |
| Ambiguous legacy `SkillEffectSO` | Description-only fallback |
| Nested Skill traversal/detail | Deferred |
| Character adapter | Implemented; approved asset validation pending |
| Bless adapter | Implemented; approved current asset validation pending |
| Relic adapter | Implemented; approved current asset validation pending |
| Generic compact formatter | Implemented; source-key label data added, additional rows may be added after asset validation |
| Generic View scripts | Implemented with hierarchy AutoBind and forced scroll-layout refresh; user visual and Viewport input validation pending |
| Skill content presenter | Implemented for an assigned existing UI; final ownership decision and user Unity validation pending |
| General Bless content presenter | List-owned information tabs implemented; General Blesses apply immediately and have no equipment state; runtime ownership reconciliation and user Unity validation pending |
| Relic content presenter | Definition Preview and runtime entry implemented; future collection UI must use all owned Relics without equipment filtering; runtime ownership reconciliation and user Unity validation pending |
| Owned Effects inventory | Tabless `OwnedOnly` View/Presenter and the item/category/panel prefab graph are directly wired; static solution and serialized-reference checks passed; user Unity import, interaction, scroll/detail/localization/visual validation and later Manager-source collection remain pending |
| Relic encyclopedia | Existing `RelicCollectionView` preserved for acquired/unacquired items, silhouettes, and owned/total counts; future page work pending |
| Faith page and four-feature adapter | Detailed design complete; Exclusive Job Change and explicit four-slot source model are not implemented |
| Faith prefab preparation | Detailed contract recorded in `AgentDocs/Machal/faith-page-design.md`; user prefab work and Unity validation pending |
| Scene integration | Deferred |

## 2026-08-12 Player Display Catalog Extension

- Added `Assets/Scripts/Presentation/PresentationDisplayCatalog.cs` as the explicit allowlist and localization-key mapping for player Groups, Entries, Tags, contextual enum replacements, and value formats.
- Added `Assets/Scripts/Presentation/PresentationLocalizedTextResolver.cs` for safe `StringManager` lookup while retaining the existing name fallback used when `StringManager` is unavailable.
- `PresentationTextFormatter.CreatePlayerFormatter(...)` is strict: player text has no raw-key or generated Pascal-case fallback. The default formatter remains the complete inspection/debug path.
- `SkillPresentationGroupResolver.ResolveForPlayerDisplay()` filters system-only fields and conditionally omits zero/default/unbounded values. `Resolve()` is unchanged as the raw inspection path.
- Bless and Relic player-display methods use the same catalog. Their existing name and description behavior is unchanged; unapproved raw category/runtime-state text is inspection-only.
- Added 154 canonical `presentation.*` rows to `Assets/Resources/string/presentation_string.csv`, including fixed DamageType, ControlType, and Displacement vocabulary plus explicit format keys.
- Full field inventory and policy: `AgentDocs/Machal/ability-content-presentation-display-catalog.md`.
- Verification: `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity:minimal` completed with 0 errors and 191 existing warnings. CSV has 294 data rows and zero duplicate key pairs. Unity was not run by the agent.

## 2026-08-12 Missing Localization Key Visibility Correction

- `PresentationLocalizedTextResolver.ResolveRequired` now probes ordered candidates silently and exposes the full first intended key only when every candidate is missing.
- Skill, Effect, Bless, and Relic descriptions use required lookup. Candidate ownership and order are unchanged.
- Player catalog filtering remains strict: an unapproved raw field with no display mapping is omitted, while a missing StringManager row for an approved mapping is visible as its intended key.
- The StringManager-unavailable asset-name fallback and the prohibition on generated Pascal-case player text remain unchanged.
- Static Editor assembly build completed with 0 errors and 191 existing warnings. Unity visual verification remains user-owned and pending.
