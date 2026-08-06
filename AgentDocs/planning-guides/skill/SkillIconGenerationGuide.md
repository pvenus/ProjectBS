# Skill Icon Generation Guide


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

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
VFX assets. Skill icons intentionally include an opaque square background and a
compact approved border. The primary subject must be either an activated tool or
a symbolic emblem. Use a simplified character silhouette only when source data
cannot be communicated by either representation, and never let the character,
background, or ornament dominate the skill outline.

## 2. Required References

Read these project guides before generation:

```text
AgentDocs/planning-guides/skill/data-structures/SkillJsonGuide.md
AgentDocs/planning-guides/skill/data-structures/EquipmentSkillSO.md
AgentDocs/planning-guides/skill/design/SkillDegineGuide.md
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
| Background | Opaque, skill-linked, low-contrast, illustrated full-bleed background |
| Composition | One activated tool or symbolic emblem plus one connected simple effect |
| Outer frame | Continuous dark 2-pixel square frame |
| Subject outline | Crisp dark 2-pixel outer silhouette |
| Internal line | 1 pixel only for non-semantic detail; meaningful features are at least 4 pixels |
| Content safe margin | Primary symbol and effects stay at least 8 pixels from each canvas edge |
| Palette | Limited role-based palette with a medium-dark background base and one bright effect accent |
| Text | Not allowed |
| Animation | Not allowed |

The icon must remain readable when shown at 32 x 32 pixels. Prefer a strong
silhouette over fine detail.

## 6. Hybrid Style Contract

Use this common style description for primary-symbol generation:

```text
compact square Korean traditional dark-fantasy tactical RPG skill icon, crisp
handcrafted pixel art, one large readable activated tool or symbolic emblem, one
connected simple effect, dark two-pixel primary silhouette outline, limited muted
palette with one bright accent color, low-contrast skill-reactive full-bleed
background, high contrast at small UI size
```

Do not describe the outer frame, card, border coordinates, safe-area coordinates,
or exact-count micro-effects in the generation prompt. The fixed template and
deterministic stages own those requirements. The internal background is different:
PixelLab must generate it as a continuous skill-reactive painting before frame
normalization.

Required exclusions:

```text
no text, no letters, no numbers, no logo, no photorealism, no smooth vector art,
no soft airbrush painting, no unrelated or high-contrast scenery, no modern UI
glyph, no multiple unrelated objects, no animation sheet, no card, no badge, no
inset panel, no flat color-only background, no white or transparent canvas
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
background coverage: continuous through x=2..77 and y=2..77, including all corners
near-white blank ratio: at most 2 percent before and after normalization
dominant single background RGB ratio: at most 85 percent
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
- Prefer one weapon, claw, projectile, or compact impact shape caught in its actual
  swing, launch, collision, recoil, or other activation moment.
- Reject a weapon or tool shown upright, isolated, or presented like an inventory
  item.
- Derive the actual direction from source behavior. Do not default every weapon or
  projectile to lower-left-to-upper-right.
- Keep effect density low and use exactly one connected simple effect.
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
- Keep one dominant action and exactly one connected simple effect.
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
- Prefer a symbolic emblem that combines the passive trigger or persistent effect
  with one directly relevant Korean traditional motif.
- Do not default to a standalone shield, armor piece, heart, eye, rune, crest, or
  ring when it would read as an inventory item, logo, badge, or generic UI glyph.
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
  "representationMode": "activated_tool",
  "activationMoment": "impact",
  "coreOutline": "one bold forward-pointing armored wedge",
  "composition": "forward_diagonal",
  "compositionProfile": "diagonal_melee",
  "simpleSkillEffect": "one broad gold impact arc",
  "internalEffectDescription": "the gold arc begins at the wedge impact edge",
  "koreanTraditionalMotif": "one simplified dancheong cloud curve",
  "traditionalMaterial": ["lacquered wood", "dancheong pigment"],
  "backgroundRequirement": "required",
  "backgroundMode": "contextual",
  "backgroundBaseColor": "deep charcoal lacquer",
  "backgroundSceneAction": "impact cracks spread across a shrine courtyard",
  "backgroundSceneElements": ["cracked lacquer floor", "dancheong cloud curve"],
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

Assign color by role rather than listing colors without purpose:

```text
primary activated tool or emblem: 1-2 material colors
one simple effect: 1 brighter element or role accent
Korean traditional motif: 1 restrained accent shared with the scene
internal background: 1 medium-dark base plus 1-2 low-contrast scene colors
```

The background base must be an explicit medium-dark palette color. Never leave it
implicit, default it to white, or provide only one background color in a way that
encourages a uniform fill. The background palette must support a visible surface,
atmospheric flow, impact trace, or other skill-linked scene action.

Recommended final range:

```text
12-24 colors
```

This is a project style target, not a PixelLab UI restriction.

## 11. No Image Reference Policy

The concise text prompt defines the representation mode, activation moment,
primary outline, composition, direction, one simple skill effect, Korean
traditional motif, and required internal background. Do not use a Concept
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
- The existing 80 x 80 frame template is used only after generation for
  deterministic outer-frame and safe-area normalization. It must not manufacture,
  replace, or hide a missing or flat internal background.
- Stop with `missing_frame_template` when the required existing template cannot be
  found.

## 12. PixelLab Primary and Edit Stages

Primary generation:

```text
URL: https://www.pixellab.ai/create?tool=create_ui_pro
Tool: Create UI elements (Pro)
Custom size: 80 / 80
Expected output: 4 x 4 grid, 16 independent 80 x 80 variations
Transparent background: Off
Concept Image / gallery / clipboard / style reference: Empty
Color palette: primary, simple effect, traditional accent, medium-dark background
base, and low-contrast scene colors entered by role
Description: follow Section 13
```

An 80 x 80 Pro run costs 25 generations. Start with one run, apply cheap static
and semantic rejection checks to all 16 variations, and advance at most the best
three candidates. Perform no more than two Pro runs. Request the second run only
when every variation has the same fatal core-outline, direction, representation,
meaning, blank-canvas, or solid-background failure, and replace the failed sentence
instead of appending instructions.

Optional semantic edit:

```text
Tool: Edit image
Input: accepted primary result
Instruction: one short add/remove/change/replace sentence
Output size: 80 x 80
```

Use image edit only for a broad arc, field, ring, or trail that contributes to skill
meaning and when the primary representation is already correct. The effect must
begin at a named tool contact point or emblem axis. Do not use image edit to enforce
exact counts, repair the frame, replace a white canvas, or create a missing
full-bleed background.

## 13. Concise Outline Prompt Contract

The primary Description contains exactly five concise English sentences in this
order:

```text
1. Activated tool or symbolic emblem: describe the visible form before its meaning.
2. Direction and composition: state one explicit axis or centered arrangement.
3. Connected simple effect: describe exactly one broad effect and its connection point.
4. Skill-reactive full-bleed background: visible scene action, one surface or atmosphere, and one Korean traditional motif.
5. Compact exclusions and grade/style: 3-6 likely errors, palette, Korean traditional dark-fantasy pixel art, and small-size readability.
```

Choose `representationMode` before writing Sentence 1:

- `activated_tool`: use when the tool and its swing, launch, collision, resonance,
  ignition, opening, rotation, or summoning action are essential. The tool and
  activation must read as one silhouette. A static upright or isolated tool fails.
- `symbolic_emblem`: use for abstract effects, buffs, debuffs, passives, or cases
  where a tool would resemble equipment. Combine skill direction, range, element,
  and effect with one bold Korean traditional motif. Do not create a logo, badge,
  heraldry, text, or UI button.

Sentence 3 must connect the effect to the actual blade, tip, impact point, launch
opening, resonant part, emblem center, outline, or rotation axis. A detached glow,
ring, trail, or particle cluster is a `disconnected_effect_failure`.

Sentence 4 is mandatory for every icon and uses this contract:

```text
A full-bleed {medium-dark backgroundBaseColor} {traditionalMaterial} background
depicts {visible backgroundSceneAction} through {one environmental surface or
atmosphere} and {one Korean traditional motif}, filling the entire icon canvas edge
to edge including all four corners; no solid-color-only, white, transparent, blank,
or unpainted area remains.
```

Use `backgroundMode=contextual` when source data supports a location, target,
surface, or environmental reaction. Otherwise use
`backgroundMode=symbolic_effect_scene` and visualize the skill's element,
direction, range, or impact result as an abstract traditional background scene.
`flat` mode is not allowed. Both modes require an opaque illustrated full-bleed
background and Transparent background Off.

The background must be one continuous low-contrast painted surface with a visible
skill reaction such as scratches following a slash, lacquer cracks spreading from
an impact, frost tracing paper grain, scorched dancheong pigment, poison seeping
through a floor pattern, or spirit wind bending a lattice shadow. A uniform color,
simple gradient, local patch behind the emblem, default white canvas, transparent
area, or unpainted margin is not a background.

Apply the master concept Korean traditional design language to the skill meaning.
Choose one readable primary motif and one or two verified matching materials; this
guide controls their icon-scale simplification and hierarchy, while the master
concept exclusively controls cultural eligibility and foreign-element prohibition.

Visual hierarchy is fixed:

```text
activated tool or emblem > one simple effect > Korean traditional accent > internal background
```

Do not include frame, card, panel, background-border, pixel-coordinate, safe-margin,
or exact-count micro-effect instructions. Do not repeat a long negative list. The
mandatory full-bleed and no-blank wording in Sentence 4 is not a frame instruction
and must not be removed.

Partial-object rule:

```text
Avoid: an isolated wolf jaw
Prefer: two disconnected dark-gray crescent jaw strips with four large pale fangs
```

Example:

```text
Two disconnected dark-gray crescent jaw strips with four large pale fangs form one
bold core outline. They close horizontally from left and right at the center. Add
one broad dark-crimson bite arc that begins at the inner fang edges. A full-bleed
deep charcoal inked-hanji background depicts bite pressure splitting a frozen path
through cracked earth and one faded dancheong cloud curve, filling the entire icon
canvas edge to edge including all four corners; no solid-color-only, white,
transparent, blank, or unpainted area remains. No full wolf, person, moon, card, or
badge; Grade 2 Korean dark-fantasy pixel art in charcoal, bone-white, muted cyan,
and crimson with thick features readable at 32 by 32 pixels.
```

Activated-tool example:

```text
A broad chipped iron spear is caught driving through one compact impact point,
forming one bold activated silhouette. It thrusts strictly from left to right. One
thick crimson impact arc begins directly at the spear point and continues forward.
A full-bleed deep charcoal lacquered-wood background depicts the thrust scoring a
shrine courtyard through one cracked floor path and one faded teal dancheong cloud
curve, filling the entire icon canvas edge to edge including all four corners; no
solid-color-only, white, transparent, blank, or unpainted area remains. No static
upright weapon, inventory item, person, banner, or card; Grade 2 Korean dark-fantasy
pixel art in iron-gray, crimson, faded teal, and charcoal with thick features
readable at 32 by 32 pixels.
```

## 14. Retry Routing

Do not make Attempt 2 by appending coordinates and more prohibitions to the failed
Description.

| Failure | Required next method |
|---|---|
| Wrong direction | Replace the direction sentence with one explicit axis phrase and use a new seed |
| Whole creature/person reconstructed | Replace the semantic noun with visual-shape wording |
| Static or inventory-like tool | Replace Sentence 1 with an activation verb and contact point; if repeated, switch to `symbolic_emblem` |
| Tool or emblem detached from effect | Replace Sentence 3 with one connection point; use one Edit image instruction only when the primary is already correct |
| Missing Korean traditional identity | Replace generic ornament with one skill-linked Korean motif and one or two traditional materials |
| Foreign cultural motif | Remove it and replace it with one skill-linked dancheong, samtaegeuk, lattice, gwimyeon, obangsaek, or maedeup motif |
| Missing exact-count particles | Add deterministic overlay |
| Missing broad semantic arc/field/trail | Use one PixelLab Edit image instruction |
| Blank, white, transparent, or locally patched background | Replace Sentence 4 with the mandatory full-bleed contract and use one new seed; stop with `no_passing_candidate` if repeated |
| Uniform or gradient-only background | Replace `backgroundSceneAction` with a concrete environmental reaction or atmospheric motion and use one new seed; stop if repeated |
| Missing or wrong frame on a background-gate-passing candidate | Re-run deterministic template edge normalization |
| Background dominates | Lower its contrast and detail; keep one surface or atmosphere and one Korean motif |
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
Background Mode: contextual | symbolic_effect_scene
Interior Background Source: generated full-bleed skill-reactive background
Near-White Blank Ratio Before / After:
Dominant Background RGB Ratio Before / After:
Four-Corner / Edge Continuity: Pass | Fail
Foreground Safe Rect: x=8..71, y=8..71
Internal Background Rect: x=2..77, y=2..77
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
3. Build the normalized semantic classification, including `representationMode`,
   `activationMoment`, `compositionProfile`, `coreOutline`, `direction`,
   `simpleSkillEffect`, `internalEffectDescription`, `koreanTraditionalMotif`,
   `traditionalMaterial`, `backgroundRequirement`, `backgroundMode`,
   `backgroundBaseColor`, `backgroundSceneAction`, `backgroundSceneElements`,
   `exactCountElements`, and `prohibitedObjects`.
4. Write exactly five concise sentences in the required order: activated tool or
   emblem, direction, connected simple effect, full-bleed skill-reactive background,
   and compact exclusions plus grade/palette/Korean traditional style.
5. Open `Create UI elements (Pro)`, set Custom size to 80 x 80, leave Concept Image
   empty, and enter the project palette in Color palette.
6. Set Transparent background Off, then perform one Pro run and preserve all 16
   individual 80 x 80 variations.
7. Apply the background hard gate first. Reject a candidate when near-white blank
   pixels exceed 2 percent, the illustrated background does not continue through
   all four corners and all edges, the background exists only as a local patch, one
   RGB color exceeds 85 percent of the visible background, or no skill-linked scene
   action is visible.
8. Apply representation and semantic checks only to background-gate-passing
   candidates. Reject wrong direction, static or inventory-like tools, detached
   effects, reconstructed whole objects, missing Korean traditional identity,
   foreign motifs, and unrelated objects. Advance no more than the best three; do
   not treat the contact sheet as one icon.
9. If all 16 candidates share a fatal background, direction, representation, or
   meaning failure, replace the failed sentence and perform one final Pro run with
   a new seed. Do not exceed two primary runs. Stop with `no_passing_candidate` if
   the repeated batch still has no passing candidate.
10. If the primary is correct but the one broad semantic effect is missing, apply
   at most one `Edit image` instruction that names its connection point. Do not edit
   in a missing background.
11. Add exact-count sparks, chips, threads, or chevrons with a deterministic pixel
   overlay. Each item must be at least 4 x 4 pixels.
12. Preserve the already passing generated low-contrast background inside x=2..77
    and y=2..77. Normalization must not replace a blank, white, transparent, flat,
    or semantically unrelated interior.
13. Keep the primary and effects inside the central 64 x 64 foreground safe area,
   then restore template pixels on rows and columns 0, 1, 78, and 79.
14. Record Steps 11-13 as deterministic overlay and normalization, not as resize or
   crop.
15. Produce a nearest-neighbor 32 x 32 preview. Confirm 40-52 pixel primary size,
   meaningful lines at least 4 pixels thick, 4-6 pixel element spacing, 4 x 4
   minimum particles, and 3-4 pixel arcs or rings in the 80 x 80 source. Confirm
   the activated tool or emblem, direction, connected effect, Korean motif, and
   background scene action remain readable in the fixed hierarchy.
16. Re-run the background gate after normalization and evaluate the normalized
   final source, not the raw PixelLab output.
17. Preserve intermediate evidence within the existing candidate layout and save
   the final source under the existing evaluation layout.
18. Copy only a passing final source to the existing Unity path and verify checksum
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
- One activated tool or symbolic emblem dominates the composition.
- Exactly one simple effect is visibly connected to the tool activation point or
  emblem axis and does not become a separate object.
- The slot is visually recognizable as basic, active, or passive.
- The grade intensity matches Grade 1, 2, or 3 without breaking family identity.
- The element and role colors match source data.
- The border matches the existing approved template. The interior follows the
  recorded `contextual` or `symbolic_effect_scene` background mode.
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
- One skill-linked Korean traditional motif is readable without becoming an
  ornamental frame, generic East Asian decoration, or foreign cultural symbol.
- The internal background is an opaque, illustrated, low-contrast, continuous
  scene that reaches all four corners and all edges behind the foreground.
- Near-white blank or unpainted pixels are at most 2 percent, excluding only
  documented compact effect highlights that are not part of the background.
- No single RGB color occupies more than 85 percent of the visible background.
- A contextual or symbolic-effect background uses one environmental surface or
  atmosphere and one Korean motif, visibly depicts a skill-linked reaction or
  flow, and never competes with the primary.
- There is no flat color-only fill, simple gradient-only fill, white default
  canvas, transparent gap, unpainted margin, local color patch, or unrelated object.

### 16.3 Pipeline and Identity Validation

- The existing frame template path is recorded and exists on the current PC.
- The primary generation record confirms `Create UI elements (Pro)`, Custom size
  80 x 80, and an empty Concept Image input.
- All 16 Pro variations are preserved or explicitly accounted for, and no contact
  sheet is treated as a single candidate.
- The generation record confirms `representationMode`, `activationMoment`, one
  connected simple effect, one Korean traditional motif, one or two traditional
  materials, and `backgroundMode=contextual|symbolic_effect_scene`.
- The primary Description follows the mandatory five-sentence contract and does
  not delegate frame, exact-count, or safe-area enforcement to PixelLab.
- Transparent background is Off and Color palette assigns roles for primary,
  effect, traditional accent, medium-dark background base, and scene support colors.
- The background hard gate is recorded for every candidate before semantic scoring.
- A second Pro run, when used, replaces the failed sentence, uses a new seed, and
  remains within the maximum of two primary runs.
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
| Skill intent and representation readability | 20 |
| Tool-or-emblem and effect connection | 15 |
| Korean traditional project style | 15 |
| Full-bleed background design and visual hierarchy | 15 |
| Small-size silhouette | 15 |
| Direction and composition | 10 |
| Palette, grade, and template frame quality | 10 |

Result:

```text
Pass: 85-100 and no fatal failure
Conditional Pass: 75-84 and can be corrected without regeneration
Fail: below 75 or any fatal failure
```

Fatal failures:

- Wrong skill meaning or slot.
- Static or inventory-like tool when `activated_tool` is required.
- Tool or emblem is visibly detached from its simple effect.
- Missing Korean traditional primary motif, or dominant Japanese, Chinese,
  Western, or generic foreign motif.
- Near-white blank ratio above 2 percent without a documented compact-highlight
  exception.
- Internal background does not reach every corner and edge, exists only as a local
  patch, or contains white, transparent, blank, or unpainted areas.
- Dominant background RGB ratio above 85 percent, or no identifiable skill-linked
  scene action is present.
- Flat color-only, gradient-only, default canvas, card, panel, or inset-square
  background.
- Text, letters, numbers, or logo present.
- Not pixel art or visibly smoothed.
- Unreadable primary symbol at 32 x 32.
- Missing or broken icon background or template border.
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
Representation Mode:
Activation Moment:
Activated Tool or Symbolic Emblem Description:
Composition Profile:
Background Requirement:
Background Mode: contextual | symbolic_effect_scene
Background Base Color:
Background Scene Action:
Background Scene Elements:
Background Description:
Korean Traditional Motif:
Traditional Material:
Core Outline Sentence:
Direction Sentence:
Connected Simple Skill Effect Sentence / Type / Placement:
Tool-or-Emblem / Effect Connection Check:
Full-Bleed Background Sentence:
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
Candidate Background Gate Results:
Near-White Blank Ratio:
Dominant Background RGB Ratio:
Four-Corner / Edge Continuity:
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
- representation_failure
- inert_tool_failure
- disconnected_effect_failure
- korean_traditional_style_failure
- foreign_motif_failure
- blank_canvas_failure
- solid_background_failure
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
- Do not use a flat, white, transparent, blank, gradient-only, or locally patched
  internal background. Transparent background remains Off for primary generation.
- Do not present a skill tool as a static upright object, material, equipment piece,
  or inventory item. Show its activation or switch to a symbolic emblem.
- Do not separate the one simple effect from its tool contact point or emblem axis.
- Use no more than one primary traditional motif. Apply the master concept
  foreign-element prohibition as a hard gate.
- Do not select or normalize a candidate that failed the near-white, dominant-color,
  four-corner, edge-continuity, or skill-linked-background hard gate.
- Do not resize or crop a generated result to force direction, frame, or count
  compliance.
- Deterministic frame-edge/safe-area normalization and exact-count overlay are
  required post-generation operations, not prohibited edits. They must not create,
  replace, or conceal a missing or flat internal background.
- Do not create a new template folder when an existing required input cannot be found.
- Do not overwrite an accepted icon without preserving or explicitly approving the
  replacement workflow.
- Do not report Unity import completion unless the PNG and `.meta` are present and
  the sprite can be resolved by the expected resource key.
