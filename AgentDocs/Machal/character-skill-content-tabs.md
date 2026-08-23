# Character Skill Content Tabs

## Purpose

Display the Skills referenced by one `CharacterSO` as icon tabs. Selecting a tab replaces the content shown by the existing `UIContentInfoView` with that Skill's presentation data.

## Ownership and Data Flow

```text
CharacterSO.Skills
  -> CharacterSkillContentInfoPresenter
  -> SkillContentInfoTabButton instances
  -> selected EquipmentSkillSO
  -> SkillContentInfoPresenter.ShowSkill
  -> SkillPresentationResolver
  -> SkillPresentationGroupResolver.ResolveForPlayerDisplay
  -> UIContentInfoView
```

- `CharacterSkillContentInfoPresenter` owns Character Skill order, tab creation, and selection.
- `SkillContentInfoTabButton` owns only one icon, one click action, and selected-state visuals.
- `SkillContentInfoPresenter` remains the concrete `EquipmentSkillSO` presentation owner.
- `UIContentInfoView` remains content-neutral and does not depend on `CharacterSO` or `EquipmentSkillSO`.
- Skill name, description, labels, tokens, and formats continue through the existing StringManager-backed player formatter.

## Implemented Files

- `Assets/Scripts/Actor/Character/ui/CharacterSkillContentInfoPresenter.cs`
- `Assets/Scripts/Ability/Skills/UI/SkillContentInfoTabButton.cs`
- `Assets/Scripts/Ability/Skills/UI/SkillContentInfoPresenter.cs`

`SkillContentInfoPresenter` now exposes `ShowSkill(EquipmentSkillSO)` and `ClearPresentation()` so Character composition does not duplicate Skill normalization or formatting.

## Behavior Contract

- Tab order matches `CharacterSO.Skills` order.
- One tab is created for every non-null `CharacterSkillEntry.skillSo`.
- A null Skill slot is skipped and reported with its source index; no invented placeholder Skill is created.
- The configured `initialSelectedIndex` is clamped to the valid generated-tab range.
- The selected tab immediately calls the existing Skill player-display path.
- Rebuilding clears prior generated tabs before creating the new set.
- `SetCharacter(character, rebuild: true)` supports changing the Character while in Play Mode.
- A selected tab is non-interactable and can optionally activate a dedicated `selectedVisual`.

## Unity Prefab Binding Required

The agent did not operate Unity or modify prefab YAML in this work unit. The user must perform these steps in Unity:

1. Open `Assets/Prefabs/UI/Child/Slot/UISkillIconSlot.prefab`.
2. Add a `Button` component to the root GameObject named `UISkillIconSlot`.
3. Add `SkillContentInfoTabButton` to that root.
4. Confirm AutoBind assigns:
   - `button` -> the root `UISkillIconSlot` Button.
   - `skillIconImage` -> the child `Bind_SkillIconImage` Image.
5. Optionally assign a selected-frame GameObject to `selectedVisual`. If omitted, the selected state is still represented by the Button becoming non-interactable.
6. In the content panel, create or identify a `RectTransform` named `CharacterSkillTabRoot` and add the desired horizontal/grid layout components.
7. Add `CharacterSkillContentInfoPresenter` to the panel that contains the existing Skill content presenter.
8. Assign the `CharacterSO` and the `UISkillIconSlot` prefab component to `skillTabPrefab`.
9. Confirm `skillPresenter` references the existing `SkillContentInfoPresenter`. AutoBind can resolve it when its GameObject is named `SkillContentInfoPresenter`; otherwise assign it manually.
10. Enter Play Mode. `buildOnStart` builds the tabs automatically, or use the component context menu `Build Character Skill Tabs`.

## Manual Validation Checklist

- Unity compiles without new errors after refreshing the generated project files.
- Generated tab count equals the number of non-null Skill references in the selected `CharacterSO`.
- Tab order matches the Character asset.
- The configured initial tab is selected and its icon is correct.
- Clicking every other tab changes the Skill name, description, tags, groups, entries, and icon in the same `UIContentInfoView`.
- Exactly one tab is selected after every click.
- Re-running `Build Character Skill Tabs` does not leave duplicate visible tabs.
- Replacing the Character through `SetCharacter` rebuilds the list and selects a valid initial tab.
- Existing content scrolling still works after several tab changes.
- Missing localization still displays the intended key according to the existing localization contract.

## Verification State

- Source structure and current `UISkillIconSlot.prefab` YAML were inspected.
- The current prefab has the icon hierarchy but does not yet contain a `Button` or `SkillContentInfoTabButton`.
- The generated `Assembly-CSharp.csproj` now includes both new source files.
- A current `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal` attempt was still blocked before C# compilation because the sandbox could not read `C:\Users\machal89\AppData\Local\Microsoft SDKs`. This access failure is not a compile result for the new scripts.
- Unity compilation, AutoBind, prefab saving, button input, selection visuals, and Play Mode output remain user-owned and pending.

## Exclusions

- No Character, Skill, Effect, Bless, or Relic source asset was edited.
- No legacy asset was read as an implementation source or migrated.
- No alternate localization fallback was added or changed.
- No Scene binding, prefab mutation, Unity Editor operation, commit, or push was performed.

## 2026-08-13 Character List Navigation Extension

`CharacterSkillContentInfoPresenter` is now the Character-selection owner for
`Assets/Prefabs/UI/Fixed/Panel/Panel_CharacterInfo.prefab`.

- A serialized `List<CharacterSO>` defines the navigation order.
- `initialCharacterIndex` selects the first Character shown at startup.
- `ShowPreviousCharacter()` and `ShowNextCharacter()` are public actions for user-connected buttons.
- `loopCharacterSelection` controls whether navigation wraps at the two ends. When disabled, an unavailable end action is ignored. `CanShowPreviousCharacter` and `CanShowNextCharacter` expose the current availability without owning Button state.
- Null Character list slots are ignored with their source index logged. If the list is empty, the existing single `character` field remains a backward-compatible one-Character source.
- Character changes rebuild the selected Character's Skill icon tabs and call the existing `CharacterContentInfoPresenter`, keeping the Character body and Skill page synchronized.
- `initialSelectedSkillIndex` retains the old serialized `initialSelectedIndex` value through `FormerlySerializedAs`.
- `SetCharacters`, `SelectCharacter`, and the existing `SetCharacter` support runtime replacement and selection without changing CharacterSO assets.

The prefab now assigns its existing `CharacterContentInfoPresenter` to the Character-list owner and disables that presenter's independent `buildOnStart`, preventing the two previously different serialized Character fields from racing during `Start`.

Per the user-owned Unity boundary, no navigation Button object was created and no event was connected. The user will create the buttons, connect them to `ShowPreviousCharacter()` and `ShowNextCharacter()`, populate `characters`, and validate Play Mode navigation.

Static verification: `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal` completed with 0 errors and 35 existing warnings. Unity was not opened or controlled.
