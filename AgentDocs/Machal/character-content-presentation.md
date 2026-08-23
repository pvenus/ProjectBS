# Character Content Presentation

## Purpose

Display player-relevant authored Character data in a dedicated `UIContentInfoView`, while retaining complete JSON and SO inspection output for source comparison.

## Approved Source

- JSON: `Assets/Resources/character/json/*.json`
- Generated SO: `Assets/Resources/character/json/*.asset`
- Current inventory: 22 JSON files and 22 matching CharacterSO assets
- Generator: `Assets/Editor/tools/character/CharacterJsonGenerator.cs`

The current JSON contains only `characterId`, `name`, `characterType`, `job`, and `baseStats`. Animation Clip and Skill references are generated into CharacterSO by the builder and are not authored Character JSON fields.

## Player Display Boundary

| Source | Player UI | Inspection tool |
| --- | --- | --- |
| `name` | visible through `StringManager.Get(characterId, "name")` | raw JSON name and resolved StringManager value |
| `characterType` | localized Tag | raw enum and localized Tag |
| `job` | one localized Tag | raw Job plus derived internal classification |
| `baseStats` | one localized Entry per source Stat | every source Stat/value/provenance |
| `characterId` | hidden as a row; retained in identity/provenance | visible |
| Animation Clips | hidden | count visible as SO-only system data |
| Skill references / `slotKey` | hidden from Character body; consumed by Skill tabs | full references visible as SO-only system data |
| runtime state and derived Job parts | hidden in authored Character UI | inspection/runtime data only |

No numeric value is combined or replaced. Current unit interpretation is source-backed:

- `Attack`, `Defense`, `MaxHp`: flat
- `AttackSpeed`: source number with localized multiplier format
- `CritChance`, `CritDamage`: percent because runtime divides these values by 100
- `MoveSpeed`: meters per second

## Runtime UI Flow

```text
CharacterSO
  -> CharacterContentInfoPresenter
  -> CharacterPresentationResolver.ResolveForPlayerDisplay
  -> PresentationDisplayCatalog
  -> StringManager-backed formatter
  -> CharacterContentInfoView (UIContentInfoView)
```

`CharacterContentInfoPresenter` can optionally synchronize the same CharacterSO into `CharacterSkillContentInfoPresenter`, so the Character body and Skill tabs use one selected source.

## Comparison Tool

Open either:

- `Tools > ProjectBS > Presentation > Open Character Data Preview`
- Right-click a CharacterSO and choose `Assets > ProjectBS > Presentation > Preview Selected Character`

The window contains three independently scrollable columns:

1. `Original JSON`: exact TextAsset content from the approved root.
2. `SO Inspection (all)`: unfiltered Presentation plus SO-only Animation/Skill system references.
3. `Player UI (filtered)`: the same filtered data and StringManager catalog intended for the runtime View.

The header reports mismatches for `characterId`, `characterType`, `job`, ordered `baseStats`, numeric values, and the Korean Character name row.

## Unity Binding Required

The agent did not edit prefabs or operate Unity in this work unit.

1. Add a separate `UIContentInfoView` instance for the Character body and name its GameObject `CharacterContentInfoView`, or assign it manually.
2. Add `CharacterContentInfoPresenter` to the parent panel.
3. Assign the current CharacterSO.
4. Assign the Character body View to `contentView`.
5. Optionally assign the existing `CharacterSkillContentInfoPresenter` to `skillTabs` to synchronize Character selection.
6. Enter Play Mode and use `Build Character Presentation` from the component context menu if `buildOnStart` is disabled.

## Expected Current Player Output

- Localized Character name
- Character type Tag (`Npc` or `Boss` in the current approved JSON)
- Job Tag (`SoldierBase`, `ArcherBase`, `ScholarBase`, or `MonkBase`)
- One `Character Stats` group with the seven current source Stats
- No Character ID, Animation Clip, Skill reference, slotKey, derived Job component, or runtime-state row

## Verification

- All 22 approved JSON files parse as strict UTF-8.
- All 22 JSON files match their generated SO ID, type, job, ordered Stat types, and numeric values.
- All 22 JSON names match exactly one `character_string.csv` Korean name row.
- `presentation_string.csv` has 308 data rows and zero case-insensitive duplicate key pairs.
- Runtime assembly build with the new Presenter temporarily included: 0 errors, 35 existing warnings.
- Editor assembly build with the final comparison window temporarily included: 0 errors, 197 aggregate warnings, including expected JsonUtility DTO `CS0649` warnings from the new inspection tool.
- Temporary generated-project compile entries were removed after verification; Unity will regenerate them normally.
- Unity prefab binding and Play Mode visual validation remain pending and user-owned.

## Changed Runtime and Tool Paths

- `Assets/Scripts/Actor/Character/Data/CharacterPresentationData.cs`
- `Assets/Scripts/Actor/Character/CharacterPresentationResolver.cs`
- `Assets/Scripts/Actor/Character/ui/CharacterContentInfoPresenter.cs`
- `Assets/Scripts/Presentation/PresentationDisplayCatalog.cs`
- `Assets/Scripts/Presentation/PresentationTextFormatter.cs`
- `Assets/Editor/tools/character/CharacterPresentationPreviewWindow.cs`
- `Assets/Resources/string/character_string.csv`
- `Assets/Resources/string/presentation_string.csv`
