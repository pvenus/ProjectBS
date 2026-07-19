# Skill Icon Generation Guide

## 1. Purpose

This guide defines how to generate static pixel-art skill icons with PixelLab.

The generated icon is a UI asset that represents one skill at a small gameplay
size. It is not a skill VFX animation, character sprite, or UI panel.

Use this guide for icons stored under:

```text
Assets/Resources/skill/icon/skill
```

Do not apply the transparent-background and character-independence rules from
`SkillImageGenerationGuide.md` to these icons. That guide is for animated skill
VFX assets. Skill icons intentionally include a square background, a compact
border, and may use a simplified character silhouette when the action cannot be
communicated by a weapon or symbol alone.

## 2. Required References

Read these project guides before generation:

```text
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
```

Official PixelLab references:

```text
https://www.pixellab.ai/docs/tools/create-ui-elements-pro
https://www.pixellab.ai/docs/tools/edit-image
https://www.pixellab.ai/docs/options/general
https://www.pixellab.ai/create?tool=create_ui_pro
```

## 3. Mandatory Tool and Generation Path

Use PixelLab only.

The standard generation path is prompt-first and staged:

```text
concise core-outline prompt
→ PixelLab Create UI elements (Pro) without Concept Image: 16 primary variations
→ PixelLab Edit image: optional large semantic effect
→ deterministic exact-count pixel overlay
→ existing 80x80 frame/background template normalization
→ nearest-neighbor 32x32 preview
```

Use `Create UI elements (Pro)` with a concise Description and no Concept Image or
style reference. At 80 x 80, the Pro tool produces a 4 x 4 grid of 16 variations
and costs 25 generations per run. Use `Edit image` only when the selected primary is correct but its one simple
semantic arc, field, or trail is missing. PixelLab recommends action verbs such as
`add`, `remove`, `change`, or `replace` for image editing.

Generated artwork must come from PixelLab. Exact-count overlays and final
frame/background/safe-area normalization are deterministic pixel operations, not a
substitute image generator.

Do not substitute these tools:

- `Create UI elements`
- `Create from style reference (Pro)`
- `Create M-XL image`
- PixelLab API style-reference generation endpoints
- `Image to pixel art`
- animation tools

If PixelLab is unavailable, authentication fails, credits are insufficient, or
the required existing frame template cannot be found on the current PC,
stop and report the blocker. Do not copy another PC's absolute path, create a new
folder convention, or substitute another image generator.

## 4. Source of Truth

The skill JSON is the source of truth for icon meaning.

Use the full `equipmentId`:

```text
skill.{domain}.{character_name}.{grade}.{slot}.{skill_name}
```

Example:

```text
skill.character.military_officer.3.active_1.charge
```

Parse `equipmentId` by `.`:

| Token | Example | Meaning |
|---|---|---|
| prefix | skill | Asset domain prefix |
| domain | character | Skill ownership domain |
| characterName | military_officer | Owner identity |
| grade | 3 | Skill or character grade |
| slot | active_1 | Gameplay slot |
| skillName | charge | Stable skill name |

Use `_` only inside a token to join words. Do not infer the slot only from the
file name or display name; use the slot in the source design or `equipmentId`.

The output filename must be:

```text
{equipmentId}.icon.png
```

The output path must be:

```text
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
```

## 5. Standard Asset Specification

| Property | Required Value |
|---|---|
| Asset type | Static skill UI icon |
| Canvas | 80 x 80 pixels |
| Format | PNG |
| Color mode | RGBA |
| Background | Opaque icon background |
| Composition | One centered primary symbol |
| Outer frame | Continuous dark 2-pixel square frame |
| Subject outline | Crisp dark 2-pixel outer silhouette |
| Internal line | 1 pixel only for non-semantic detail; meaningful features are at least 4 pixels |
| Content safe margin | Primary symbol and effects stay at least 8 pixels from each canvas edge |
| Palette | Limited, high-contrast palette |
| Text | Not allowed |
| Animation | Not allowed |

The icon must remain readable when shown at 32 x 32 pixels. Prefer a strong
silhouette over fine detail.

## 6. Hybrid Style Contract

Use this common style description for primary-symbol generation:

```text
compact square dark-fantasy tactical RPG skill icon, crisp handcrafted pixel art,
one large readable primary silhouette, dark two-pixel primary silhouette outline,
limited muted palette with one bright accent color, high contrast at small UI size
```

Do not describe the outer frame, background panel, card, border coordinates, safe
area coordinates, or exact-count micro-effects in the generation prompt. The fixed
template and deterministic stages own those requirements.

Required exclusions:

```text
no text, no letters, no numbers, no logo, no photorealism, no smooth vector art,
no soft airbrush painting, no detailed scenery, no modern UI glyph, no multiple
unrelated objects, no animation sheet, no card, no badge, no inset panel
```

Final normalized asset contract:

```text
primary size: 40-52 pixels
meaningful line thickness: at least 4 pixels
element spacing: at least 4-6 pixels
spark or chip size: at least 4x4 pixels
arc or ring thickness: 3-4 pixels
safe content area: central 64x64 pixels
outer frame: template pixels on rows/columns 0, 1, 78, 79
```

Do not ask PixelLab to imitate a named living artist or a copyrighted franchise.

## 7. Slot Visual Rules

### 7.1 Basic Attack

Known slot:

```text
basic_attack
```

Visual intent:

- Show what performs the repeated attack.
- Prefer one weapon, claw, projectile, or compact impact shape.
- Derive the actual direction from source behavior. Do not default every weapon or
  projectile to lower-left-to-upper-right.
- Keep effect density low.
- Avoid a full character unless the action is impossible to read otherwise.
- Use dark red, dark brown, or weapon-material colors when no element is defined.

Default tags:

```text
slotFamily = basic_attack
composition = source_driven_direction
effectDensity = low
```

### 7.2 Active Skill

Known slots:

```text
active_1
active_2
active_3
```

Visual intent:

- Show the skill's unique action, impact, area, movement, or elemental identity.
- Use speed lines, impact light, trails, particles, or a simplified action silhouette.
- Keep one dominant action and at most two secondary effects.
- Increase visual intensity by active slot only when the source design also supports
  the higher impact.

Default intensity:

| Slot | Intensity | Typical Composition |
|---|---|---|
| active_1 | medium | One clear action |
| active_2 | high | Action plus element or impact |
| active_3 | very high | Boss-like or explicitly high-impact symbol |

`active_3` is exceptional. Do not generate it for a normal skill set unless the
source design and runtime explicitly support it.

### 7.3 Passive Skill

Known slots:

```text
passive_1
passive_2
```

Visual intent:

- Represent a persistent role, trigger, defense, buff, or condition.
- Prefer armor, shield, heart, eye, rune, crest, ring, or aura symbolism.
- Use a centered and approximately symmetrical composition.
- Minimize motion lines.
- Use steel, blue, teal, gold, or role-specific colors.

Default tags:

```text
slotFamily = passive
composition = centered_symmetry
effectDensity = medium
```

`passive_2` must not be generated unless the source design and runtime explicitly
require a second passive slot.

## 8. Grade Progression Rules

Grades 1 through 3 must feel like the same visual library.

When the same named skill exists at multiple grades, preserve its primary symbol,
composition, and base palette. Increase only effect density, contrast, ornament,
and accent strength.

| Grade | Grade Style | Visual Rule |
|---:|---|---|
| 1 | base | Simple silhouette, one main color and one support color, minimal effects |
| 2 | enhanced | Preserve Grade 1 identity, add one secondary trail, aura, or highlight |
| 3 | mastered | Preserve identity, strengthen contrast and accent, add controlled ornament |

Do not create a completely unrelated image for a higher grade version of an
inherited skill.

For inherited skills:

1. Inspect the accepted lower-grade icon for identity only.
2. Keep the same primary symbol and orientation in the classification record.
3. Convert the inherited symbol and direction into the concise outline Description;
   do not upload the completed lower-grade icon.
4. Add only the grade-appropriate enhancement.
5. Reject the result if the skill is no longer recognizable as the same family.

## 9. Semantic Classification

Do not generate from `skillName` alone. Determine visual meaning in this order:

1. `slot`
2. `baseProfile.skillType`
3. `cast.targetingType`
4. `cast.castMove.moveType`
5. `baseProfile.skillComponentType`
6. `move.moveType`
7. `hits[].damage`
8. `cast.selfEffects`
9. `hits[].buffEffects` and `hits[].debuffEffects`
10. effect type and configuration
11. `skillName`

Create one normalized classification record before writing the PixelLab prompt:

```json
{
  "assetKind": "icon",
  "equipmentId": "skill.character.military_officer.3.active_1.charge",
  "gradeTier": "grade_3",
  "slotFamily": "active",
  "visualFamily": "movement_attack",
  "coreOutline": "one bold forward-pointing armored wedge",
  "composition": "forward_diagonal",
  "compositionProfile": "diagonal_melee",
  "simpleSkillEffect": "one broad gold impact arc",
  "exactCountElements": ["three gold impact sparks"],
  "prohibitedObjects": ["full battlefield", "text banner"],
  "elementFamily": "physical",
  "roleFamily": "frontline",
  "paletteFamily": "sand_black_gold",
  "intensity": "high"
}
```

### 9.1 Visual Families

Use one primary family:

```text
weapon_strike
projectile
movement_attack
burst
area_attack
control
defense
buff
debuff
heal
summon
aura
trigger
```

### 9.2 Composition Families

Use one composition:

```text
single_diagonal_object
horizontal_projectile
descending_projectile
forward_action
centered_symmetry
radial_burst
circular_emblem
projectile_direction
ground_impact
```

### 9.3 Element Families

Use one element when the source explicitly supports it:

```text
physical
fire
water
ice
lightning
wind
earth
poison
blood
shadow
light
spirit
neutral
```

Do not invent an element from the skill name if JSON behavior and design context do
not support it.

### 9.4 Role Families

Use one dominant combat role:

```text
damage
tank
support
control
mobility
summon
survival
```

## 10. Palette Rules

Choose the palette by explicit element first, combat role second, and slot default
last.

| Meaning | Primary Palette |
|---|---|
| Physical attack | Dark red, brown, steel gray |
| Defense or survival | Blue, steel, muted cyan |
| Buff or empowerment | Gold, orange, warm white |
| Debuff or curse | Purple, dark green, black |
| Fire | Red, orange, ember yellow |
| Water or ice | Blue, cyan, cold white |
| Lightning | Yellow, pale blue, white |
| Poison | Green, yellow-green, dark brown |
| Shadow | Purple, black, desaturated blue |
| Heal or light | Teal, gold, warm white |

Keep the icon to a limited palette. If the generated result contains too many
near-duplicate colors, revise the palette clause and regenerate with `Create UI
elements`. Do not correct the result with `Reduce Colors` or another editing tool.

Recommended final range:

```text
12-24 colors
```

This is a project style target, not a PixelLab UI restriction.

## 11. No Image Reference Policy

The concise text prompt defines the primary outline, composition, direction, one
simple skill effect, and optional contextual background. Do not use a Concept
Image, gallery image, clipboard image, or style reference for primary generation.

Supported composition profiles:

```text
horizontal_projectile
descending_projectile
diagonal_melee
centered_radial_active
centered_passive_emblem
```

Rules:

- Resolve the frame template path from files already present on the current PC and
  from existing generation records.
- Never reuse an absolute path copied from another PC.
- Do not invent a new template directory when a required file is missing.
- Existing completed icons may be inspected for inherited identity, but translate
  that identity into concise shape language instead of uploading the image.
- The existing 80 x 80 frame template is used only after generation for deterministic
  background, border, and safe-area normalization.
- Stop with `missing_frame_template` when the required existing template cannot be
  found.

## 12. PixelLab Primary and Edit Stages

Primary generation:

```text
URL: https://www.pixellab.ai/create?tool=create_ui_pro
Tool: Create UI elements (Pro)
Custom size: 80 / 80
Expected output: 4 x 4 grid, 16 independent 80 x 80 variations
Transparent background: On for flat mode; Off for contextual mode
Concept Image / gallery / clipboard / style reference: Empty
Color palette: concise project palette
Description: follow Section 13
```

An 80 x 80 Pro run costs 25 generations. Start with one run, apply cheap static
and semantic rejection checks to all 16 variations, and advance at most the best
three candidates. Request another Pro run only when every variation has the same
fatal core-outline, direction, or meaning failure.

Optional semantic edit:

```text
Tool: Edit image
Input: accepted primary result
Instruction: one short add/remove/change/replace sentence
Output size: 80 x 80
```

Use image edit only for a broad arc, field, ring, or trail that contributes to skill
meaning. Do not use it to enforce exact counts or repair the fixed frame.

## 13. Concise Outline Prompt Contract

The primary Description has four short core sentences or blocks. One short
background sentence may be inserted before the final sentence only for
`backgroundMode=contextual`:

```text
1. Core outline: the large visible shape, described before its semantic name.
2. Direction and composition: one explicit axis or centered arrangement.
3. Simple skill effect: only one broad effect that explains the action.
Optional background: 1-2 low-contrast elements that explain gameplay.
4. Compact exclusions and grade/style: 3-6 likely errors, palette, and readability.
```

For `backgroundMode=flat`, omit the optional background sentence and enable Transparent
background. Deterministic normalization supplies the approved flat charcoal or
deep-brown interior. Do not prompt for empty scenery, gradients, fog, terrain, or
decoration merely to fill space.

Use `backgroundMode=contextual` only when the setting communicates a location,
area boundary, travel direction, impact surface, or source of the skill. Disable
Transparent background and describe no more than two sparse, low-contrast elements.
The background must remain visually subordinate to the core outline.

Do not include frame, card, panel, background-border, pixel-coordinate, safe-margin,
or exact-count micro-effect instructions. Do not repeat a long negative list.

Partial-object rule:

```text
Avoid: an isolated wolf jaw
Prefer: two disconnected dark-gray crescent jaw strips with four large pale fangs
```

Example:

```text
Two disconnected dark-gray crescent jaw strips with four large pale fangs form one
bold core outline. They close horizontally from left and right at the center. Add
one broad dark-crimson bite arc behind them. No full wolf, person, altar, card, or
badge; Grade 2 dark-fantasy pixel art in charcoal, bone-white, and crimson with
thick features readable at 32 by 32 pixels.
```

Contextual background example:

```text
A broad chipped iron spearhead forms one bold horizontal outline. It drives from
left to right. Add one thick crimson impact arc at its point. Behind it, show one
low-contrast broken shield edge and a narrow dust band. No person, battlefield,
banner, card, or badge; Grade 2 pixel art in iron-gray, crimson, and charcoal with
thick features readable at 32 by 32 pixels.
```

## 14. Retry Routing

Do not make Attempt 2 by appending coordinates and more prohibitions to the failed
Description.

| Failure | Required next method |
|---|---|
| Wrong direction | Replace the direction sentence with one explicit axis phrase and use a new seed |
| Whole creature/person reconstructed | Replace the semantic noun with visual-shape wording |
| Missing exact-count particles | Add deterministic overlay |
| Missing broad semantic arc/field/trail | Use one PixelLab Edit image instruction |
| Missing frame or wrong background | Re-run deterministic template normalization |
| Contextual background dominates | Shorten it to 1-2 low-contrast elements or switch to flat mode |
| 32x32 information loss | Enlarge primary, thicken lines, and increase spacing |

Exact-count overlay manifest:

```json
{
  "elements": [
    {
      "type": "spark",
      "count": 3,
      "minimumSize": "4x4",
      "color": "muted_gold",
      "anchorPixels": [[58, 24], [63, 31], [57, 38]]
    }
  ]
}
```

The manifest records post-generation pixel anchors. Do not place these coordinates
in the PixelLab Description.

Normalization record:

```text
Frame Template Path:
Frame Template SHA-256:
Background Mode: flat | contextual
Interior Background Source: frame template | generated contextual background
Foreground Safe Rect: x=8..71, y=8..71
Contextual Background Rect: x=2..77, y=2..77 | not_applicable
Removed Outside-Safe-Area Pixels:
Restored Frame Rows: 0, 1, 78, 79
Restored Frame Columns: 0, 1, 78, 79
Normalized Candidate SHA-256:
```

## 15. Staged Generation Workflow

1. Read the target skill JSON and validate `equipmentId`, grade, slot, and output
   path.
2. Resolve the current PC's existing evaluation root, Unity destination, and
   `frameTemplatePath` without creating a new path convention.
3. Build the normalized semantic classification, including `compositionProfile`,
   `coreOutline`, `simpleSkillEffect`, `backgroundRequirement`, `backgroundMode`,
   `backgroundDescription`, `exactCountElements`, and `prohibitedObjects`.
4. Write four concise core sentences. Add one background sentence only for
   `contextual` mode.
5. Open `Create UI elements (Pro)`, set Custom size to 80 x 80, leave Concept Image
   empty, and enter the project palette in Color palette.
6. Set Transparent background On for `flat` or Off for `contextual`, then perform
   one Pro run and preserve all 16 individual 80 x 80 variations.
7. Apply static and semantic rejection to all variations. Advance no more than the
   best three; do not evaluate a combined contact sheet as an icon.
8. Reject a wrong direction, reconstructed whole object, or dominant unrelated
   background before evaluating small effects.
9. If needed, apply one `Edit image` operation for a broad semantic effect.
10. Add exact-count sparks, chips, threads, or chevrons with a deterministic pixel
   overlay. Each item must be at least 4 x 4 pixels.
11. In `flat` mode, use the template interior background. In `contextual` mode,
    preserve the generated low-contrast background inside x=2..77 and y=2..77.
12. Keep the primary and effects inside the central 64 x 64 foreground safe area,
    then restore template pixels on rows and columns 0, 1, 78, and 79.
13. Record Steps 10-12 as deterministic overlay and normalization, not as resize or
    crop.
14. Produce a nearest-neighbor 32 x 32 preview. Confirm 40-52 pixel primary size,
    meaningful lines at least 4 pixels thick, 4-6 pixel element spacing, 4 x 4
    minimum particles, and 3-4 pixel arcs or rings in the 80 x 80 source.
15. Evaluate the normalized final source, not the raw PixelLab output.
16. Preserve intermediate evidence within the existing candidate layout and save
    the final source under the existing evaluation layout.
17. Copy only a passing final source to the existing Unity path and verify checksum
    and `.meta` import settings.

## 16. Validation

### 16.1 File Validation

- The output file exists at the exact required path.
- The filename is the full `equipmentId` plus `.icon.png`.
- The PNG decodes successfully.
- The image is exactly 80 x 80 pixels.
- The image uses RGBA color mode.
- The Unity `.png.meta` file exists or is created by the approved import workflow.

### 16.2 Visual Validation

- The icon is readable at 80 x 80 and 32 x 32 pixels.
- One primary symbol dominates the composition.
- No more than two secondary effects compete with the primary symbol.
- The slot is visually recognizable as basic, active, or passive.
- The grade intensity matches Grade 1, 2, or 3 without breaking family identity.
- The element and role colors match source data.
- The border matches the existing approved template. The interior follows the
  recorded `flat` or `contextual` background mode.
- The outer frame is continuously 2 pixels thick.
- The primary silhouette uses a 2-pixel outer outline. One-pixel internal details
  are allowed only when they do not carry skill meaning.
- The primary symbol and effects remain inside the central 64 x 64 safe area.
- The primary symbol is approximately 40-52 pixels across its meaningful dimension.
- Meaningful lines are at least 4 pixels thick and elements are separated by 4-6
  pixels where possible.
- Exact-count particles are at least 4 x 4 pixels and match the overlay manifest.
- Arcs and rings are 3-4 pixels thick.
- Pixels remain crisp with no unintended smoothing.
- There is no text, letter, number, logo, or animation grid.
- There is no detailed scenery or unrelated object.
- A contextual background uses at most two sparse low-contrast elements and never
  competes with the primary. Flat mode contains no generated scene.

### 16.3 Pipeline and Identity Validation

- The existing frame template path is recorded and exists on the current PC.
- The primary generation record confirms `Create UI elements (Pro)`, Custom size
  80 x 80, and an empty Concept Image input.
- All 16 Pro variations are preserved or explicitly accounted for, and no contact
  sheet is treated as a single candidate.
- The primary Description follows the four core sentence contract plus at most one
  contextual background sentence and does not delegate
  frame, exact-count, or safe-area enforcement to PixelLab.
- Semantic edits, exact-count overlays, and deterministic normalization are recorded
  as separate stages.
- The 32 x 32 nearest-neighbor preview remains readable.
- An inherited skill preserves its lower-grade primary symbol and orientation.
- Different skills do not receive byte-identical final icons unless explicit reuse
  is approved.
- The icon is distinguishable from other skills visible in the same loadout.

## 17. Candidate Scoring

Score every candidate out of 100:

| Category | Points |
|---|---:|
| Skill intent readability | 25 |
| Project style match | 20 |
| Small-size silhouette | 20 |
| Slot and grade distinction | 15 |
| Palette and contrast | 10 |
| Composition and border quality | 10 |

Result:

```text
Pass: 85-100 and no fatal failure
Conditional Pass: 75-84 and can be corrected without regeneration
Fail: below 75 or any fatal failure
```

Fatal failures:

- Wrong skill meaning or slot.
- Text, letters, numbers, or logo present.
- Not pixel art or visibly smoothed.
- Unreadable primary symbol at 32 x 32.
- Missing or broken icon background/border.
- Wrong canvas size.
- Animation sheet or multiple unrelated panels.
- Higher-grade inherited skill loses the lower-grade identity.

## 18. Generation Record

Record:

```text
Skill ID:
Source JSON:
Output Path:
Grade:
Slot:
Classification:
Reference Mode: none
Inherited Icon Reference:
Composition Profile:
Background Requirement:
Background Mode: flat | contextual
Background Description: omitted | value
Core Outline Sentence:
Direction Sentence:
Simple Skill Effect Sentence:
Compact Exclusion / Grade Sentence:
Frame Template Path:
PixelLab Creator URL:
Primary Tool:
Pro Grid / Variation Count: 4x4 / 16
Concept Image: empty
Color Palette:
Primary Description:
Semantic Effect Tool / Instruction:
Exact-Count Overlay Manifest:
Transparent Background:
Requested Width / Height:
Downloaded Width / Height:
Seed: value or not_exposed
Attempt Count:
Normalization Record:
32x32 Preview Result:
Selected Candidate:
Regeneration Performed:
Validation Score:
Result:
Failure Reasons:
Notes:
```

## 19. Failure Output

When generation cannot complete, report:

```text
status: failed
failureType:
- missing_skill_json
- invalid_equipment_id
- unsupported_slot
- missing_frame_template
- invalid_frame_template
- pixellab_unavailable
- pixellab_authentication_failed
- insufficient_pixellab_credits
- wrong_pixellab_tool
- invalid_ui_settings
- generation_timeout
- invalid_downloaded_size
- semantic_edit_failed
- overlay_failed
- normalization_failed
- no_passing_candidate
- output_write_failed
- unity_import_pending

failedSkillId:
failedOutputPath:
failureReason:
missingInput:
lastPixelLabResult:
nextRequiredAction:
```

Do not create a placeholder icon as a failure artifact.

## 20. Hard Boundaries

- Generate static skill icons only.
- Do not generate skill animation PNGs or sprite sheets.
- Do not replace PixelLab with another image tool.
- Do not alter skill balance or gameplay JSON to fit an image.
- Do not invent a new slot, element, or effect.
- Do not use a Concept Image, gallery image, clipboard image, or style reference.
- Do not resize or crop a generated result to force direction, frame, or count
  compliance.
- Deterministic frame/background/safe-area normalization and exact-count overlay are
  required post-generation operations, not prohibited edits.
- Do not create a new template folder when an existing required input cannot be found.
- Do not overwrite an accepted icon without preserving or explicitly approving the
  replacement workflow.
- Do not report Unity import completion unless the PNG and `.meta` are present and
  the sprite can be resolved by the expected resource key.
