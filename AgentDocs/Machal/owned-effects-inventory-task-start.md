# Owned Effects Inventory New-Chat Start Contract

- Category: `[GUIDE]`
- Path: `AgentDocs/Machal/owned-effects-inventory-task-start.md`
- Korean: `AgentDocs/Machal/owned-effects-inventory-task-start-ko.md`
- Status date: 2026-08-18

## Purpose

This is the task-specific entry contract for continuing the ProjectBS Owned Effects inventory in a new chat. It does not replace repository rules or the detailed Ability Presentation documents. It tells the next agent exactly what to read, what is currently authoritative, what has already been implemented, and which single work unit comes next.

## Copy-Paste Request for a New Chat

```text
Continue the ProjectBS tabless Owned Effects inventory task.

Before analyzing or changing files, read AGENTS.md, AgentDocs/task-start-documentation-prompt.md, AgentDocs/Machal/README.md, and AgentDocs/Machal/owned-effects-inventory-task-start.md completely. Then follow the exact Mandatory Reading Order in the task-start contract. Read AgentDocs/code-writing-rules.md before changing C#.

After reading, first report the current confirmed design, the next single implementation unit, the exact paths you intend to change, and the Unity work that remains user-owned. Preserve every unrelated modified or untracked file. Do not reset, clean, commit, push, migrate legacy data, modify prefabs through YAML, or operate Unity.

Proceed with only the next work unit recorded in the start contract. Stop and ask the user when Unity import, component attachment, AutoBind, prefab editing, Scene binding, or Play Mode verification is required. Update the English canonical AgentDocs and Korean mirrors at the end of the work unit.
```

## Mandatory Reading Order

Read every required file completely before implementation:

1. `AGENTS.md`
2. `AgentDocs/task-start-documentation-prompt.md`
3. `AgentDocs/Machal/README.md`
4. `AgentDocs/Machal/owned-effects-inventory-task-start.md`
5. `AgentDocs/Machal/basic-work-guide.md`
6. `AgentDocs/Machal/ability-content-presentation-task.md`
7. `AgentDocs/Machal/ability-content-presentation-inventory.md`
8. `AgentDocs/Machal/ability-content-presentation-contract-evaluation.md`
9. `AgentDocs/Machal/ability-content-presentation-display-catalog.md`
10. `AgentDocs/Machal/ability-content-presentation-stage4-verification.md`
11. `AgentDocs/Machal/ability-content-presentation-stage5-8-completion.md`
12. `AgentDocs/Machal/ability-content-ui-prefab-preparation.md`
13. `AgentDocs/Machal/character-skill-content-tabs.md`
14. `AgentDocs/Machal/character-content-presentation.md`
15. `AgentDocs/Machal/faith-page-design.md`
16. `AgentDocs/Machal/ability-content-presentation-log.md`
17. `AgentDocs/code-writing-rules.md` before changing scripts or code
18. Every exact source path named by the active task contract for the work unit

If any required path is missing, do not guess. Record the missing path in the task log and report it to the user.

## Confirmed Current Design

### Owned Effects Page

- The page has no tabs.
- It is one inventory-like page with one vertical `ScrollRect`.
- Its scroll content dynamically creates ordered category sections.
- It shows only currently owned or active content:
  - owned Relics,
  - acquired General Blesses,
  - active Bless-backed Faith effects.
- A category with no visible items may be omitted.
- Every category uses the same category-section View and item View system.
- Every selected item binds to one shared, content-neutral `UIContentInfoView`.
- The Owned Effects page never uses `Catalog` mode.
- Exclusive Job Change is excluded unless a future explicit Effect source is authored.

### Separate Encyclopedia Pages

- The Relic encyclopedia is a separate page and may show acquired and unacquired Relics in `Catalog` mode.
- The General Bless encyclopedia is a separate page and may show acquired and unacquired General Blesses in `Catalog` mode.
- The Faith encyclopedia remains separate and owns Faith progression, inactive features, future unlocks, and Exclusive Job Change.
- The common item/category/detail rendering may be reused, but each page keeps its own domain Presenter and source policy.

### Layout Boundary

```text
Panel_OwnedEffects
├─ OwnedEffectsPresenter
├─ OwnedEffectsPageView
│  └─ one vertical ScrollRect
│     └─ CategoryRoot
│        ├─ Relic category section        (runtime)
│        ├─ General Bless section         (runtime)
│        └─ active Faith Bless section    (runtime)
└─ one shared UIContentInfoView
```

Do not put a vertical `ScrollRect` inside each category. Category sections contain a header and item grid; the page owns the only list scroll.

## Current Repository State

### Implemented and Verified

- `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryData.cs`
  - `ContentInventoryDisplayMode`
  - `ContentAcquisitionState`
  - `ContentActivationState`
  - `ContentInventoryItemData`
  - `ContentInventoryCategoryData`
  - `ContentInventoryPageData`
- `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryItemView.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/ContentInventoryCategoryView.cs`
- Phase 2 clean static solution build passed with 0 errors and 197 existing warnings after temporary project inclusion.
- The generated project file was restored after verification.

`ContentInventoryDisplayMode.Catalog` remains valid for separate encyclopedia pages. It must not become an Owned Effects page option.

### Superseded Legacy Units

- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryData.cs`
- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectGridItemView.cs`
- the old four-tab implementations formerly stored in `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryView.cs` and `OwnedEffectInventoryPresenter.cs`

`OwnedEffectInventoryData.cs` and `OwnedEffectGridItemView.cs` remain as unwired legacy units. The two View/Presenter paths now contain the tabless category-section implementation; do not restore their earlier four-tab behavior.

### Preserved for Other Pages

- Keep `RelicCollectionView` for the future Relic encyclopedia.
- Keep standalone `RelicContentInfoPresenter` and `BlessContentInfoPresenter` behavior unless a later task explicitly changes their dedicated-page roles.
- Keep `UIContentInfoView` content-neutral. It must not interpret `RelicSO`, `BlessSO`, or ownership rules.

### Directly Wired on 2026-08-18

- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryView.cs` now creates one category prefab per non-empty category, owns cross-category selection, and binds one shared detail View.
- `Assets/Scripts/Presentation/SharedUI/Content/OwnedEffectInventoryPresenter.cs` now composes ordered owned Relic, General Bless, and active Faith Bless categories in `OwnedOnly` mode.
- `Assets/Prefabs/UI/Fixed/Panel/Panel_OwnedEffects.prefab` has both components attached and all page/detail/category references assigned.
- `Assets/Prefabs/UI/Fixed/Content/UIContentInventoryCategory.prefab` has `ContentInventoryCategoryView`, title/count/root references, and the item prefab assigned.
- `Assets/Prefabs/UI/Fixed/Content/UIInventoryItemView.prefab` has `ContentInventoryItemView` assigned to the existing child `UISelectableIconButton`.
- The standalone `RelicContentInfoPresenter` was removed only from the shared detail object inside `Panel_OwnedEffects`; its source and dedicated-page behavior remain preserved.
- The shared detail object is active by default.
- Four StringManager rows were added for the page and category titles.
- This prefab YAML work was performed only because the user explicitly requested direct component connection in this task. It does not authorize future prefab YAML edits.
- Static verification: `dotnet build ProjectBS.sln --no-restore -v:minimal` passed with 0 errors and 197 existing warnings; serialized-reference and localization uniqueness checks passed.

### Not Yet Implemented or Verified

- automatic runtime Manager collection,
- Unity import and Play Mode verification,
- separate Relic and General Bless encyclopedia pages.

The checkout contains extensive unrelated modified and untracked work. There is no task-authorized reset, clean, commit, or push. Inspect scoped paths before and after every work unit.

## Next Single Work Unit

User-owned Unity validation of the directly wired Owned Effects page:

1. Let Unity import and compile the changed scripts and prefabs; report any Console error without repairing unrelated files.
2. Open `Assets/Prefabs/UI/Fixed/Panel/Panel_OwnedEffects.prefab` and confirm the root has `OwnedEffectInventoryView` and `OwnedEffectInventoryPresenter` with non-null page references.
3. Assign test Relic, General Bless, and Faith Bless SO lists to the Presenter.
4. Enter Play Mode and invoke `Build Configured Owned Effects` if `buildOnStart` is disabled, or let `buildOnStart` populate it.
5. Confirm each non-empty category appears, item clicks update the one always-active `UIContentInfoView`, selected state moves correctly, and the outer vertical ScrollRect works.
6. Report the exact Console message or failing interaction if validation fails. Code correction begins only from that evidence.

After this Unity validation passes, the next code unit is automatic runtime source collection for the Owned Effects Presenter. Separate General Bless and Relic encyclopedia integrations remain later independent units.

## Work Method

- Work in one independently verifiable unit at a time.
- Before editing, report the exact scoped paths and confirm that overlapping existing changes will be preserved.
- Follow `AgentDocs/code-writing-rules.md`: create callable, compilable structure first and mark temporary implementation points honestly when needed.
- Do not expose raw JSON/C# keys or generated Pascal-case labels to the player View.
- Names, descriptions, category titles, labels, tags, enum replacements, and formats follow the existing StringManager/`PresentationDisplayCatalog` contract.
- Missing approved localization remains visible as the intended key; do not replace it with invented fallback text.
- Do not edit or migrate legacy Effect, Bless, or Relic assets.
- Do not change unrelated gameplay ownership behavior while building the Presentation UI.
- Do not commit or push without an explicit user request.

## Unity Work Boundary

The user owns Unity operations for this task. Stop and request the user when any of these are required:

- Unity script import or Console confirmation,
- adding or removing components,
- AutoBind execution or serialized-reference inspection,
- prefab creation, duplication, hierarchy edits, or save,
- UnityEvent/Button wiring,
- Scene binding,
- Play Mode input, scroll, selection, localization, or visual verification.

Static source inspection and static `.NET` compilation do not prove Unity, prefab, AutoBind, or Play Mode behavior.

## End-of-Unit Report

Report these separately:

1. completed behavior,
2. exact changed paths,
3. static verification evidence,
4. Unity/user validation still required,
5. next single work unit,
6. unresolved source or design blockers,
7. documentation paths updated.

Update the English canonical documents and matching Korean `-ko` mirrors in the same work unit. Append, rather than rewrite, verification history in the task log.
