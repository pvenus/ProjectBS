# Ability Content UI Prefab Preparation

## Purpose and Boundary

This document records the generic visual hierarchy, layout, and current binding boundary prepared alongside the data layer.

On 2026-08-10, the user created the prefab skeletons and sample nested instances. The agent then applied layout and required UI components without adding View, Scene, AutoBind, or gameplay interpretation behavior. On 2026-08-11, the user attached the four generic View components, and the hierarchy fields were updated to use the project's AutoBind convention.

Do not bind concrete `EquipmentSkillSO`, `EffectSO`, `BlessSO`, `RelicSO`, or `CharacterSO` fields directly to these prefabs.

## Existing UI Findings

- `Assets/Prefabs/UIWidget/UITooltipWidget.prefab` contains only `Tooltip_ContentText` and a background. It is suitable for a compact one-string tooltip, not structured content groups.
- Current AutoBind resolves a component by the exact child object name generated from `[AutoBindPrefix]` plus the field name.
- Dynamic content should use layout groups instead of fixed-height rows. Use content-size fitting only on the object that owns its size, such as the ScrollRect content root, not on children whose parent layout controls their rect.

## Current Prefabs

The user-created skeletons and completed layouts are at:

- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoView.prefab`
- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoGroup.prefab`
- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoEntry.prefab`
- `Assets/Prefabs/UI/Fixed/Content/UIContentInfoTag.prefab`

The same generic content view can later display Skill, Effect, Bless, Relic, and Character presentation data.

## Main View Hierarchy

Prepare this hierarchy or preserve the exact AutoBind object names if the visual hierarchy differs:

```text
UIContentInfoView
|- Background
|  `- bg                             Image
|- Header
|  |- Info_IconImage                  Image
|  |- Info_NameText                   TMP_Text
|  `- Info_TagRoot                    RectTransform
`- Body
   |- Info_DescriptionText            TMP_Text
   |- Info_ScrollRect                 ScrollRect
   |  `- Viewport                     RectMask2D
   |     `- Info_GroupRoot            RectTransform
   `- Info_StatusText                 TMP_Text, optional and hidden by default
```

The implemented View uses `[AutoBindPrefix("Info")]` with fields named `iconImage`, `nameText`, `tagRoot`, `descriptionText`, `scrollRect`, `groupRoot`, and `statusText`. No close button was added; popup close behavior remains a later View decision.

## Group Prefab

```text
UIContentInfoGroup
|- Group_TitleText                    TMP_Text
|- Group_DescriptionText              TMP_Text, optional and hidden when empty
`- Group_EntryRoot                    RectTransform
```

Use a vertical layout on the root or entry root. For Skill content, a group represents one semantic display role. Current keys are:

- `Activation`: cast, targeting, trigger, chance, and activation conditions
- `Delivery`: projectile, movement, burst, hit cadence, and delivery geometry
- `Outcome`: damage, heal, stat, cooldown, periodic damage, and spawn results
- `SpecialEffect`: `Control` and `Displacement`, including source-backed duration, force, or distance
- `LinkedSkill`: `SkillInvoke` references

The seven normalized Effect Outcomes remain typed data beneath this grouping. The Skill UI does not instantiate one group per Effect. Each source field still becomes a separate entry; group aggregation must not combine or derive values.

Do not create one fixed child object for every section. Instantiate only the groups present in the resolved data.

## Entry Prefab

```text
UIContentInfoEntry
|- Entry_LabelText                    TMP_Text
|- Entry_ValueText                    TMP_Text
`- Entry_DetailButton                 Button, optional and hidden by default
```

Use a horizontal layout for the normal label/value row. Allow both texts to wrap and avoid a fixed row height.

Each source field maps to one entry and one value. `Entry_ValueText` must not combine unrelated source fields such as `projectileColliderRadius` and `projectileLifetime`; only the label text may translate a source field into player-facing wording such as Effect Range or Duration.

`Entry_DetailButton` is reserved for content that opens separately, such as a nested Skill. Do not place all nested Skill details in the parent panel.

## Category or Tag Prefab

```text
UIContentInfoTag
`- Tag_Text                           TMP_Text
```

The tag root supports zero or more tags with a horizontal layout. Wrapping remains a later improvement if real classification counts require multiple rows.

## Data-to-Prefab Mapping

| Presentation data | Prefab target |
| --- | --- |
| Identity icon | `Info_IconImage` |
| Identity display name | `Info_NameText` |
| Authored description | `Info_DescriptionText` |
| Classification entries | Instances under `Info_TagRoot` |
| Presentation groups | Group instances under `Info_GroupRoot` |
| Group label | `Group_TitleText` |
| Optional group description | `Group_DescriptionText` |
| Entry label | `Entry_LabelText` |
| Later formatted compact value | `Entry_ValueText` |
| Nested-content navigation | `Entry_DetailButton` |
| Unsupported or description-only state | `Info_StatusText` or authored description |

Provenance is not required in the normal player-facing prefab. It may be exposed later in an editor preview or debug overlay.

## AutoBind Boundary

- `UIContentInfoView` uses prefix `Info` for its seven hierarchy component fields.
- `UIContentInfoGroupView` uses prefix `Group` for its three hierarchy component fields.
- `UIContentInfoEntryView` uses prefix `Entry` for its three hierarchy component fields.
- `UIContentInfoTagView` uses prefix `Tag` for its text field.
- `tagPrefab`, `groupPrefab`, and `entryPrefab` remain manual because the current AutoBind utility resolves components in the prefab hierarchy, not prefab asset references.
- Unity must recompile the scripts and run `OnValidate`, then the user must save the four prefabs.

## Implemented Layout

- The View uses a fixed 700 by 1000 reference size, a 170-high Header, and a stretched Body with 24-pixel margins.
- `Info_ScrollRect` is vertical-only, uses a `RectMask2D` Viewport, and points to `Info_GroupRoot` as content.
- `Info_GroupRoot` uses a vertical layout and preferred-height content fitting because it is the ScrollRect content root.
- Static YAML inspection confirms the Viewport currently has `RectTransform` and `RectMask2D` only, with no raycastable `Graphic`. Wheel and drag input must be tested over areas without an active child Graphic; add a transparent raycast-target `Image` only if the user confirms the input gap in Unity.
- Group roots and `Group_EntryRoot` use vertical layouts and expose preferred sizes to their parent layouts without nested `ContentSizeFitter` components.
- Entry uses a parent-controlled horizontal label/value row with wrapping and layout-derived preferred height.
- `Entry_DetailButton`, `Group_DescriptionText`, and `Info_StatusText` are present and hidden by default.
- Tag has a subtle Image background, TMP text, horizontal padding, and parent-layout-driven preferred sizing without a nested `ContentSizeFitter`.
- Final colors, sprites, localization, and content-specific visual polish remain outside this layout unit.

## Do Not Prepare Yet

- Fixed fields for all thirteen Effect Config types
- A separate prefab per Effect Config
- Raw `ValueOverride` or upgrade-modifier fields
- A derived application-count field based on interval and duration
- JSON-only gameplay values
- Final localization strings hardcoded into prefab text
- Concrete SO references on the generic content view
- Scene binding and Unity-side attachment/configuration of the implemented Character, Skill, Bless, and Relic presenters
- Final ownership of the Skill-specific `EquipmentSkillSO` and `Build Presentation` action; keep them in `SkillContentInfoPresenter` until explicitly decided
- Unity wheel/drag verification for the Viewport and any required transparent raycast-target `Image`

## Completion Checklist

- Main content view hierarchy exists.
- Exact binding object names are preserved.
- The four View components are attached and their hierarchy fields use the matching AutoBind prefixes.
- Template-prefab asset fields remain explicit manual assignments.
- Group, entry, and tag templates are separate reusable prefabs.
- The group root can grow and scroll.
- Label/value rows support wrapping and one source value per row.
- Optional objects can be hidden without breaking layout.
- Nested-content detail navigation has a reserved optional button.
- No runtime SO or legacy data was added to the prefab.
