# Faith Page Design

## Status

- Design date: 2026-08-17
- Latest correction: 2026-08-21 - the area below the roadmap is fixed to two Faith-effect comparison cards for the current and next levels
- Scope: Faith information page, prefab preparation, presentation ownership, and staged implementation plan
- Implementation: main-panel hierarchy and callable View/Presenter scaffold completed on 2026-08-21
- Unity prefab opening and static visual inspection completed; Play Mode data validation remains pending
- This design supersedes the earlier three-Bless-tab Faith-page preparation contract.

## Product Contract

One acquired Faith owns four different feature units:

1. Basic Bless: acquired with the Faith and strengthened by Faith level.
2. Exclusive Job Change: a Faith-exclusive unlock associated with Character job families.
3. Exclusive Bless 1: acquired when Faith is locked.
4. Exclusive Bless 2: acquired when the locked Faith reaches level 8.

The page must explain current effects and future unlocks. It is not a Bless collection page.

## Recommended Player Flow

```text
Open Faith page
-> build one tab per acquired Faith
-> select locked Faith when present, otherwise the highest-level Faith
-> show selected god identity and current Faith progress
-> show the level 1-10 progression roadmap
-> show current-level and next-level Faith-effect cards below the roadmap
-> distinguish strengthened effects and newly acquired features from exact source data
```

Acquired-Faith tabs remain information navigation. Gameplay-active, Faith-locked, and removed-effect state must be represented separately instead of deleting historical information tabs from the page.

## Page Layout

```text
Panel_FaithInfo
|- Faith_Header
|  |- Faith_TitleText
|  `- Faith_CloseButton
|- Faith_GodTabScrollRect
|  `- Viewport
|     `- Faith_GodTabRoot
`- Faith_SelectedGodPage
   |- Faith_GodSummary
   |  |- Faith_GodIconImage
   |  |- Faith_GodNameText
   |  |- Faith_GodDescriptionText
   |  |- Faith_LevelText
   |  |- Faith_AffinityText
   |  |- Faith_LevelProgressSlider
   |  `- Faith_StateText
   |- Faith_RoadmapScrollRect
   |  `- Viewport
   |     `- Faith_LevelNodeRoot
   `- Faith_LevelEffectComparisonRoot
      |- Faith_CurrentLevelEffectCard
      `- Faith_NextLevelEffectCard
```

Recommended desktop layout: god tabs and identity at the top, the progression roadmap in the middle, and two equal-width comparison cards below it. Each card owns its own vertical effect-list ScrollRect.

## Required Prefabs

### `Panel_FaithInfo.prefab`

Composition owner. Attach the future `FaithPagePresenter` here. It owns acquired-Faith tabs, selected god, roadmap refresh, feature selection, and runtime event subscriptions.

### `UIFaithGodTab.prefab`

```text
UIFaithGodTab
|- FaithTab_IconImage
|- FaithTab_NameText
|- FaithTab_LevelText
|- FaithTab_SelectedFrameImage
|- FaithTab_LockedMark
`- FaithTab_InactiveMark
```

It displays navigation and state only. It does not resolve Shrine data.

### `UIFaithLevelNode.prefab`

```text
UIFaithLevelNode
|- FaithLevel_LevelText
|- FaithLevel_CurrentMark
|- FaithLevel_AcquiredMark
|- FaithLevel_LockedMark
`- FaithLevel_MilestoneIconRoot
```

Future Basic-Bless levels remain previewable. Selecting a future node shows authored preview data without claiming it is currently active.

### `UIFaithLevelEffectCard.prefab`

One reusable comparison card instantiated once for the current level and once for the next level.

```text
UIFaithLevelEffectCard
|- FaithEffectCard_Header
|  |- FaithEffectCard_StateText
|  `- FaithEffectCard_LevelText
|- FaithEffectCard_ScrollRect
|  `- Viewport
|     `- FaithEffectCard_GroupRoot
`- FaithEffectCard_EmptyStateText
```

The current card shows the complete Faith feature set that actually applies at the current level. The next card shows the complete next-level result and may mark each Group or Entry as `Strengthened`, `NewlyUnlocked`, or `Unchanged`. Locked exclusive features retain their exact unlock condition and must not look active.

### `UIContentInfoView_Faith.prefab`

Optional layout variant of the existing neutral `UIContentInfoView`. It uses the existing Group, Entry, and Tag templates. It must not contain `ShrineGodSO`, `BlessSO`, or Character job interpretation.

Reuse its neutral Group/Entry structure inside `UIFaithLevelEffectCard`. Do not create separate detail prefabs or independent feature cards for Basic Bless, Exclusive Job Change, Exclusive Bless 1, or Exclusive Bless 2.

## Ownership and Data Flow

```text
ShrineConfigSO + ShrineGodSO + explicit Faith progression definition
+ ShrineManager runtime state
-> ShrineFaithPresentationResolver
-> FaithPagePresentationData
-> FaithPagePresenter
-> God tabs / level nodes
-> current-level / next-level comparison presentation
-> two Faith-effect cards
```

- `FaithPagePresenter` owns Faith selection and dynamic composition of the two comparison cards.
- `ShrineFaithPresentationResolver` owns gameplay meaning, unlock evaluation, and preview/runtime provenance.
- `BlessPresentationResolver` is reused internally for Basic and Exclusive Bless detail.
- Character job display uses confirmed Character job definitions and localization; it is not converted into a fake Bless.
- The reused `UIContentInfoView` Group/Entry structure inside each comparison card remains content-neutral.
- `BlessContentInfoPresenter` remains useful for a standalone Bless list or one-Bless page, but it is not the Faith-page owner.

## Proposed Domain Data Contract

```text
Assets/Scripts/Stage/NodeContents/Shrine/Faith/
|- Data/FaithPagePresentationData.cs
`- ShrineFaithPresentationResolver.cs
```

The authored gameplay definition should explicitly provide:

```text
ShrineGodSO
`- ShrineFaithProgressionSO
   |- BasicBlessProgression
   |  `- explicit level entries
   |- ExclusiveJobUnlock
   |  |- unlock level
   |  |- requires Faith lock
   |  `- explicit job-family to target-job entries
   |- ExclusiveBless1Unlock
   |  |- requires Faith lock = true
   |  `- BlessSO
   `- ExclusiveBless2Unlock
      |- unlock level = 8
      |- requires Faith lock = true
      `- BlessSO
```

For Basic Bless scaling, use explicit source-backed level entries or an actual runtime scaling result. Do not calculate or interpolate values in Presentation. If gameplay uses one `BlessSO` per level, group the applicable definition under one Basic-Bless Group inside each level-effect card.

The Exclusive Job Change unlock condition is not yet confirmed. It must be authored explicitly rather than inferred from the Faith lock level, Character job enum name, or list order.

## Page Presentation Data

`FaithPagePresentationData` contains:

- acquired god list and selected god ID
- god identity, current Faith level, affinity, next-level requirement, lock state, and active state
- ordered level 1-10 nodes
- current-level and next-level effect-card data
- Group/Entry data classified by the four feature kinds inside each card
- explicit Preview or Runtime provenance

`FaithLevelEffectComparisonPresentationData` contains:

- current and next level
- current-level card data
- next-level card data
- whether the Faith is at maximum level

Each card's `FaithFeaturePresentationData` contains:

- kind: BasicBless, ExclusiveJobChange, ExclusiveBless1, or ExclusiveBless2
- localized identity source and icon
- unlock level and Faith-lock requirement
- state: Acquired, Active, Upcoming, LockedByFaith, or Inactive
- comparison state: Current, Unchanged, Strengthened, or NewlyUnlocked
- detail content generated from its actual Bless or Character-job source

A locked feature preview must remain marked as Preview and must not be shown as an active runtime effect.

## Current/Next-Level Comparison Rules

- The left card is the complete list of Faith effects actually applied at the current level.
- The right card is the complete authored Faith-effect result for the immediate next level.
- When the same stable source feature ID exists at both levels and authored values differ, classify it as `Strengthened`.
- When a feature is absent at the current level and first appears at the next level, classify it as `NewlyUnlocked`.
- Exact values may be shown as `current value -> next value`, but Presentation must not calculate a new delta value.
- Compare stable source feature/Entry IDs, never localized strings or display labels.
- If the source cannot identify a strengthening relationship, do not infer one; show only the complete next-level value.
- At maximum level, keep the right-hand card and show the empty state keyed by `presentation.faith.next_level.none`.
- Future roadmap nodes communicate milestones; the two cards below always remain based on the actual current level and its immediate next level.

## Feature Rules

### Basic Bless

- Show the authored current-level version in the current card.
- Show the authored immediate-next-level version in the next card.
- When the value of the same source Entry changes, show both exact values and mark it `Strengthened`.
- Do not calculate a level delta or application count when the source does not provide one.

### Exclusive Job Change

- Show the exact eligible job family and target job entries.
- Use Character job localization and source identity.
- Do not reuse `BlessContentInfoPresenter` or Effect groups.
- Unlock level and condition remain pending an explicit user decision and source field.
- If it first unlocks at the next level, place it in the next card's `NewlyUnlocked` group.

### Exclusive Bless 1

- Unlock condition: selected Faith is locked.
- Reuse normalized Bless/Effect presentation for detail.
- If its condition becomes satisfied at the next level, place it in the next card's `NewlyUnlocked` group.

### Exclusive Bless 2

- Unlock condition: selected Faith is locked and current Faith level is at least 8.
- Reuse normalized Bless/Effect presentation for detail.
- For a locked Faith at level 7, place it in the next-level card's `NewlyUnlocked` group.
- Before that point, show its exact condition on the roadmap milestone.

## Localization

- God, Bless, and Character job names/descriptions use their owning StringManager paths.
- Faith labels, feature kinds, unlock conditions, roadmap states, and status words use explicit `presentation.faith.*` keys.
- Missing approved localization remains visible as the full intended key.
- Do not hardcode `Faith Lv.` or enum `ToString()` output in the player page.

## Current Source Gaps

- `ShrineGodSO` has no fields for the four confirmed feature units.
- `ShrineBlessingGroup` exposes only Base and Enhanced.
- `ShrineGodSO.GetAvailableBlessings` accepts but ignores the group argument.
- `BlessPoolEntry` has only `progressionStep` and no explicit feature role.
- Current `CharacterJob` values have no confirmed Faith-exclusive job mapping.
- Thresholds are duplicated or hardcoded across `ShrineConfigSO`, `ShrineGodSO`, `FaithRuntimeData`, `ShrineFaithService`, `ShrineManager`, and `ShrineGodInfoPanel`.
- `ShrineGodInfoPanel` hardcodes English text and cannot represent the four-feature roadmap.
- No approved current Bless/Faith asset path exists. Excluded legacy `Assets/Resources/shring/` data must not become authoritative.

Resolve these gaps before final prefab binding.

## Implementation Order

1. Confirm the Exclusive Job Change unlock rule and target job data.
2. Define the authoritative current Faith/Bless authoring path and JSON/SO schema.
3. Add explicit Faith progression definition data and make one threshold source authoritative.
4. Update runtime reward/application logic to consume explicit feature definitions.
5. Add Faith presentation data and `ShrineFaithPresentationResolver`.
6. Add `FaithPagePresenter` and small tab/node/level-effect-card View components.
7. User creates and binds the page, Faith tab, level node, level-effect card, and optional ContentInfo variant.
8. Add StringManager catalog rows and static validation.
9. User performs Unity prefab, AutoBind, Play Mode, scrolling, selection, and localization validation.

## Validation Matrix

- no acquired Faith; one acquired Faith; multiple acquired Faiths
- Faith lock pending, accepted, and rejected
- locked Faith below level 8 and reaching level 8
- Basic Bless current and future-level preview
- matching Entry value comparison and `Strengthened` state across the two cards
- `NewlyUnlocked` state for a feature first acquired at the next level
- stable next-card empty state at maximum level
- Exclusive Job Change for every authored job family
- Exclusive Bless 1 and 2 locked/unlocked preview
- Faith level changes while the page is open
- missing definition or localization remains diagnosable
- acquired-tab order and selection persist across refresh
- no legacy Bless asset is read or modified

## Exclusions

- This document does not authorize gameplay reward or job-change implementation.
- No prefab, Scene, SO asset, or legacy data is changed in this design unit.
- Acquired General/Common Blesses and active Bless-backed Faith features appear as separate category sections on the tabless Owned Effects page. The General Bless catalog is a separate future page. Full Faith progression, future unlocks, and Exclusive Job Change remain in this Faith encyclopedia unless an explicit Effect source is authored later.
- Do not infer missing unlocks, jobs, or scaling from names or excluded legacy assets.

## 2026-08-21 Main Panel Scaffold Implementation

- The main prefab is `Assets/Prefabs/UI/Fixed/Panel/Panel_FaithInfo.prefab`.
- Preserved its background/foreground visuals and removed the legacy `FaithDetailView` plus incomplete `Content`, `FaithNodeRoot`, and `Panel_Desc` structures.
- Rebuilt the concrete hierarchy around `Faith_Header`, `Faith_GodTabScrollRect`, `Faith_SelectedGodPage`, `Faith_GodSummary`, `Faith_RoadmapScrollRect`, and `Faith_LevelEffectComparisonRoot`.
- Placed `Faith_CurrentLevelEffectCard` and `Faith_NextLevelEffectCard` directly below `Faith_LevelEffectComparisonRoot`. This phase uses two identical embedded card structures rather than extracting a separate card prefab.
- Each card contains one existing `UIContentInfoView.prefab` instance and reuses its Group, Entry, and scrolling behavior.
- Added one `Faith_GodTabTemplate` and ten static level nodes. The tab and roadmap roots use horizontal ScrollRect plus ContentSizeFitter layouts.
- Added runtime UI components `FaithPageView`, `FaithPagePresenter`, `FaithGodTabView`, `FaithLevelNodeView`, and `FaithLevelEffectCardView`.
- `FaithPagePresenter` exposes Inspector `configuredGods`, `configuredFaithLevel`, and the `Build Configured Faith Page` ContextMenu call scaffold.
- Current Faith source data does not yet encode the four features and level comparison, so current/next `ContentPresentationData` resolution remains an explicit `[PLACEHOLDER]`.
- Fixed `AutoBindEditorUtility` so `[AutoBind] GameObject` fields resolve GameObjects instead of being passed to `GetComponent(Type)`; Component fields retain existing behavior.
- Added the required `presentation.faith.*` page, card, empty-state, state, and comparison localization keys.
- Static prefab validation found every new component reference assigned, exactly ten level nodes, and no legacy `FaithDetailView` reference. The full solution build passed with 0 errors and 209 existing warnings.
- Unity Editor static inspection confirmed the hierarchy and evenly laid-out ten-node roadmap. Play Mode, configured SO input, generated tabs, card data, and scrolling input remain unverified.
