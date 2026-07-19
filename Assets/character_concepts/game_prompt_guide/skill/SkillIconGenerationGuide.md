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
https://www.pixellab.ai/docs/tools/create-ui-elements
https://www.pixellab.ai/docs/options/general
https://www.pixellab.ai/create?tool=create_ui_basic
```

## 3. Mandatory Tool and Generation Path

Use PixelLab only.

The standard generation path is the PixelLab Simple Creator UI:

```text
https://www.pixellab.ai/create?tool=create_ui_basic
Tool: Create UI elements
```

This tool generates one pixel-art UI component per run from a text description and
supports explicit width, height, background transparency, and optional Init Image.
For skill icons, use the description and size controls only. Keep Init Image empty.

Do not substitute these tools:

- `Create UI elements (Pro)`
- `Create from style reference (Pro)`
- `Create M-XL image`
- PixelLab API style-reference generation endpoints
- `Image to pixel art`
- animation tools

If PixelLab is unavailable, authentication fails, credits are insufficient, or
the Simple Creator UI cannot select `Create UI elements`, stop and report the
blocker. Do not substitute another image generator or PixelLab tool.

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
| Internal line | 1 pixel |
| Content safe margin | Primary symbol and effects stay at least 8 pixels from each canvas edge |
| Palette | Limited, high-contrast palette |
| Text | Not allowed |
| Animation | Not allowed |

The icon must remain readable when shown at 32 x 32 pixels. Prefer a strong
silhouette over fine detail.

## 6. Prompt-First Style Contract

Use this common style description for every generation:

```text
compact square dark-fantasy tactical RPG skill icon, crisp handcrafted pixel art,
strong readable central silhouette, continuous dark two-pixel square frame, dark
two-pixel primary silhouette outline, one-pixel internal details, limited muted
palette with one bright accent color, opaque charcoal and deep-brown background,
at least eight pixels of clear content margin on every side, high contrast at small
UI size, one primary symbol and no more than two secondary effects
```

Required exclusions:

```text
no text, no letters, no numbers, no logo, no photorealism, no smooth vector art,
no soft airbrush painting, no detailed scenery, no modern UI glyph, no multiple
unrelated objects, no animation sheet
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
- Use a diagonal or directional composition.
- Keep effect density low.
- Avoid a full character unless the action is impossible to read otherwise.
- Use dark red, dark brown, or weapon-material colors when no element is defined.

Default tags:

```text
slotFamily = basic_attack
composition = single_diagonal_object
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

1. Use the accepted lower-grade icon as an init or style reference.
2. Keep the same primary symbol and orientation.
3. Add only the grade-appropriate enhancement.
4. Reject the result if the skill is no longer recognizable as the same family.

Recommended init image strength for a grade variant:

```text
400-600
```

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
  "primarySymbol": "charging_armored_warrior_silhouette",
  "secondaryEffect": "speed_lines_and_gold_impact_sparks",
  "composition": "forward_diagonal",
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

## 11. Prompt-First Reference Policy

The text prompt is the only project-style authority.

- Do not search `Assets/Resources/skill/icon/skill` for style references.
- Do not upload an existing icon through Init Image, gallery, clipboard, or file.
- Do not use `Create from style reference (Pro)`.
- Existing icons may be inspected only to identify a lower-grade inherited skill's
  primary symbol, orientation, and base palette.
- Translate inherited identity into text; do not transmit the image to PixelLab.
- Judge project style against Sections 5, 6, 7, 8, and 10 of this guide, not
  against whichever existing icon happens to look similar.

This prevents inconsistent legacy borders, padding, rendering density, and outline
width from propagating into new icons.

## 12. PixelLab Simple Creator UI

Open:

```text
https://www.pixellab.ai/create?tool=create_ui_basic
```

The site may initially switch to another recently used tool. Confirm both:

```text
URL query: tool=create_ui_basic
Selected tool: Create UI elements
```

If either is wrong, use `Change` and select the non-Pro `Create UI elements` entry.

Required settings:

| UI Field | Required Value |
|---|---|
| Description | One English prompt built by Section 13 |
| Transparent background | Off |
| Init Image | Empty |
| Width | 80 px |
| Height | 80 px |

PixelLab documentation states that this tool creates one image per run. Treat every
run as one candidate. Do not request a multi-icon grid in one description.

The requested and downloaded dimensions must both be 80 x 80. If the downloaded
file has another size, reject it. Do not crop, resize, pad, or resample it into
compliance.

## 13. Prompt Construction

Write the English description in this order:

```text
asset type
→ slot role
→ primary symbol
→ action or effect
→ composition
→ safe area and scale
→ background
→ palette
→ grade intensity
→ frame, outline, and pixel-art style
→ exclusions
```

Template:

```text
A compact square pixel-art tactical RPG {slot_family} skill icon. {Primary symbol}
performing or representing {skill intent}. {Composition and secondary effect}.
{Background description}. {Palette description}. Grade {grade} visual intensity:
{grade rule}. Keep the primary symbol and all effects inside the central 64 by 64
pixel safe area, leaving at least 8 pixels of clear spacing from every canvas edge.
Crisp handcrafted pixels, continuous dark two-pixel square frame, dark two-pixel
primary silhouette outline, one-pixel internal details, limited palette, strong
silhouette, high contrast and readable at 32 by 32 pixels. Opaque background. No
transparent background, no cropped object, no edge-touching glow, no text, no
letters, no numbers, no logo, no photorealism, no smooth vector art, no detailed
scenery, no unrelated objects, no animation sheet.
```

Example:

```text
A compact square pixel-art tactical RPG active skill icon. A black armored officer
charging forward with a lowered shoulder, strong horizontal speed lines and a small
gold impact flare. Forward diagonal composition on a parchment beige background.
Muted sand, black, dark brown and gold palette. Grade 2 visual intensity: one clear
secondary trail and stronger highlights while keeping a simple silhouette. Keep
the officer and all effects inside the central 64 by 64 pixel safe area with at
least 8 pixels of clear spacing from every edge. Crisp handcrafted pixels,
continuous dark two-pixel square frame, dark two-pixel primary silhouette outline,
one-pixel internal details, limited palette, strong silhouette, high contrast and
readable at 32 by 32 pixels. Opaque background. No transparent background, no
cropped object, no edge-touching glow, no text, no letters, no numbers, no logo,
no photorealism, no smooth vector art, no detailed scenery, no unrelated objects,
no animation sheet.
```

## 14. Candidate Attempt Strategy

The Simple Creator UI may not expose every advanced option in every account or UI
version. Do not switch tools to obtain a seed control.

- Record the attempt number and exact Description for every run.
- If Seed is visible, record its value; otherwise record `not_exposed`.
- Revise only the failed prompt clause on regeneration.
- Generate one icon per run and never ask for a grid or multiple alternatives in a
  single image.

## 15. Generation Workflow

1. Read the target skill JSON.
2. Validate `equipmentId`, grade, slot, and output path.
3. Confirm that the slot is allowed by the source design and runtime.
4. Extract targeting, movement, damage, effects, element, and role.
5. Build the normalized classification record.
6. Inspect a lower-grade inherited icon only when identity continuity is required;
   translate the result into text and do not upload it.
7. Build one English Description from the prompt-first contract.
8. Open `https://www.pixellab.ai/create?tool=create_ui_basic`.
9. Confirm the selected tool is non-Pro `Create UI elements`.
10. Enter the Description, turn Transparent background off, and leave Init Image
    empty.
11. Set Width and Height to exactly 80 px and generate once.
12. Download the single result to a temporary evaluation folder without editing.
13. Validate 80 x 80 RGBA, border, safe margin, outline, composition, palette, and
    readability.
14. If the candidate fails, revise only the relevant prompt clause and regenerate
    within the allowed attempt count.
15. Compare all attempted candidates and select the best passing result.
16. Save the selected icon as `{equipmentId}.icon.png`.
17. Configure or refresh the Unity `.png.meta` file according to the project's
    sprite import rules.
18. Record the generation inputs, UI settings, attempts, selected candidate, and
    validation result.
19. Delete temporary candidates only after the final icon and record are verified.

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
- The border and background match the prompt-first project style contract.
- The outer frame is continuously 2 pixels thick.
- The primary silhouette uses a 2-pixel outer outline and 1-pixel internal details.
- The primary symbol and effects remain inside the central 64 x 64 safe area.
- Pixels remain crisp with no unintended smoothing.
- There is no text, letter, number, logo, or animation grid.
- There is no detailed scenery or unrelated object.

### 16.3 Prompt Contract and Identity Validation

- No style reference or Init Image was used.
- The recorded Description contains the common frame, outline, safe-area, palette,
  and exclusion clauses.
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
Reference Mode:
Inherited Icon Reference:
PixelLab Creator URL:
PixelLab Tool:
Description:
Transparent Background:
Init Image:
Requested Width / Height:
Downloaded Width / Height:
Seed: value or not_exposed
Attempt Count:
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
- pixellab_unavailable
- pixellab_authentication_failed
- insufficient_pixellab_credits
- wrong_pixellab_tool
- invalid_ui_settings
- generation_timeout
- invalid_downloaded_size
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
- Do not upload an existing project icon as Init Image or a style reference.
- Do not crop or resize a generated result to force 80 x 80 compliance.
- Do not overwrite an accepted icon without preserving or explicitly approving the
  replacement workflow.
- Do not report Unity import completion unless the PNG and `.meta` are present and
  the sprite can be resolved by the expected resource key.
