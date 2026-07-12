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
https://www.pixellab.ai/docs/tools/consistent-style
https://www.pixellab.ai/docs/options/init-image
https://www.pixellab.ai/docs/options/color
https://api.pixellab.ai/v2/docs
```

## 3. Mandatory Tool and Generation Path

Use PixelLab only.

The standard automated path is:

```text
PixelLab API v2
POST /v2/generate-with-style-v2
```

Use `Generate with style (Pro)` because the project already contains approved
skill icons that can define pixel size, outline, shading, palette behavior, and
icon composition.

Do not use these as the primary icon generator:

- `Generate UI (Pro)`: use only to design a shared icon frame or UI slot.
- `Create image Pixflux`: use only when approved style references are unavailable.
- `Image to pixel art`: use only to convert an already approved non-pixel draft.
- `Animate with text`: this creates animation assets, not static icons.

If PixelLab is unavailable, authentication fails, credits are insufficient, or
the required style references do not exist, stop and report the blocker. Do not
substitute another image generator.

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
| Border | Compact dark square border |
| Outline | Crisp dark 1-3 pixel outline |
| Palette | Limited, high-contrast palette |
| Text | Not allowed |
| Animation | Not allowed |

The icon must remain readable when shown at 32 x 32 pixels. Prefer a strong
silhouette over fine detail.

## 6. Project Style Standard

Use this common style description for every generation:

```text
compact square dark-fantasy tactical RPG skill icon, crisp handcrafted pixel art,
strong readable central silhouette, dark one-to-three-pixel outline, limited muted
palette with one bright accent color, high contrast at small UI size, subtle square
border, one primary symbol and no more than two secondary effects
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
near-duplicate colors, use PixelLab `Reduce Colors` and preserve the dominant
outline, background, base color, and accent color.

Recommended final range:

```text
12-24 colors
```

This is a project style target, not a PixelLab API requirement.

## 11. Style Reference Selection

Use one to four approved reference icons from:

```text
Assets/Resources/skill/icon/skill
```

References must have compatible pixel density, border thickness, shading, and
composition. Do not use four near-identical duplicate files as four references.

Select by slot:

### Basic Attack Reference Set

- One approved weapon icon.
- One approved projectile icon when the target skill is ranged.
- One simple action icon.
- One shared project-style anchor.

### Active Reference Set

- One action silhouette icon.
- One impact or elemental icon.
- One icon matching the target composition.
- One icon matching the target grade intensity.

### Passive Reference Set

- One centered aura icon.
- One shield, armor, rune, or emblem icon.
- One role-matching icon.
- One shared project-style anchor.

Record the exact reference file paths for reproducibility.

## 12. PixelLab Request

Standard API request:

```text
POST https://api.pixellab.ai/v2/generate-with-style-v2
Authorization: Bearer {PIXELLAB_API_TOKEN}
Content-Type: application/json
```

Request body shape:

```json
{
  "description": "{generated_english_description}",
  "image_size": {
    "width": 80,
    "height": 80
  },
  "style_images": [
    {
      "image": {
        "base64": "data:image/png;base64,..."
      },
      "width": 80,
      "height": 80
    }
  ],
  "style_description": "compact square dark-fantasy tactical RPG skill icon, crisp handcrafted pixel art, strong dark outline, limited muted palette, high contrast, subtle square border",
  "no_background": false,
  "seed": 42
}
```

Do not store or print the PixelLab API token in a prompt, report, source file, or
generation record.

PixelLab v2 accepts the request asynchronously. Record the returned
`background_job_id`, poll the background job every 5-10 seconds, and continue only
when its status is `completed`.

Handle failures explicitly:

| Status | Meaning | Required Behavior |
|---:|---|---|
| 401 | Invalid token | Stop and request authentication repair |
| 402 | Insufficient credits | Stop and report required credits |
| 422 | Invalid request | Report invalid fields and do not retry unchanged |
| 429 | Concurrency limit | Wait and retry with bounded backoff |

An 80 x 80 style generation normally returns multiple candidates. Evaluate every
returned candidate and select one; do not automatically accept the first image.

## 13. Prompt Construction

Write the English description in this order:

```text
asset type
→ slot role
→ primary symbol
→ action or effect
→ composition
→ background
→ palette
→ grade intensity
→ pixel-art style
→ exclusions
```

Template:

```text
A compact square pixel-art tactical RPG {slot_family} skill icon. {Primary symbol}
performing or representing {skill intent}. {Composition and secondary effect}.
{Background description}. {Palette description}. Grade {grade} visual intensity:
{grade rule}. Crisp handcrafted pixels, dark one-to-three-pixel outline, limited
palette, strong silhouette, high contrast and readable at 32 by 32 pixels, subtle
square border. No text, no letters, no numbers, no logo, no photorealism, no smooth
vector art, no detailed scenery, no unrelated objects, no animation sheet.
```

Example:

```text
A compact square pixel-art tactical RPG active skill icon. A black armored officer
charging forward with a lowered shoulder, strong horizontal speed lines and a small
gold impact flare. Forward diagonal composition on a parchment beige background.
Muted sand, black, dark brown and gold palette. Grade 2 visual intensity: one clear
secondary trail and stronger highlights while keeping a simple silhouette. Crisp
handcrafted pixels, dark one-to-three-pixel outline, limited palette, strong
silhouette, high contrast and readable at 32 by 32 pixels, subtle square border.
No text, no letters, no numbers, no logo, no photorealism, no smooth vector art,
no detailed scenery, no unrelated objects, no animation sheet.
```

## 14. Seed Strategy

Use deterministic seeds when generation is automated.

Recommended derivation:

```text
baseSeed = stable positive hash of characterName

slotOffset:
basic_attack = 100
active_1    = 200
active_2    = 300
active_3    = 400
passive_1   = 500
passive_2   = 600

gradeOffset:
grade_1 = 1
grade_2 = 2
grade_3 = 3

seed = baseSeed + slotOffset + gradeOffset
```

Store the final numeric seed in the generation record. A seed improves
reproducibility but does not replace lower-grade init images for inherited skills.

## 15. Generation Workflow

1. Read the target skill JSON.
2. Validate `equipmentId`, grade, slot, and output path.
3. Confirm that the slot is allowed by the source design and runtime.
4. Extract targeting, movement, damage, effects, element, and role.
5. Build the normalized classification record.
6. Search for approved style references and remove byte-identical duplicates.
7. If the skill is inherited, add the accepted lower-grade icon as a reference.
8. Build the English PixelLab description.
9. Derive and record the deterministic seed.
10. Submit `generate-with-style-v2` with an 80 x 80 canvas and
    `no_background: false`.
11. Poll the background job until completion or timeout.
12. Download every candidate to a temporary evaluation folder.
13. Validate dimensions, format, border, composition, palette, and readability.
14. Compare candidates and select the best passing result.
15. If every candidate fails, revise the prompt once and regenerate.
16. Save the selected icon as `{equipmentId}.icon.png`.
17. Configure or refresh the Unity `.png.meta` file according to the project's
    sprite import rules.
18. Record the generation inputs, references, seed, selected candidate, and
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
- The border and background match the approved project icon family.
- Pixels remain crisp with no unintended smoothing.
- There is no text, letter, number, logo, or animation grid.
- There is no detailed scenery or unrelated object.

### 16.3 Reference and Identity Validation

- Reference paths are recorded.
- Byte-identical duplicate references are not counted as separate style examples.
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
Style Reference Paths:
Inherited Icon Reference:
PixelLab Endpoint:
Description:
Style Description:
Canvas:
No Background:
Seed:
Background Job ID:
Candidate Count:
Selected Candidate:
Regeneration Performed:
Validation Score:
Result:
Failure Reasons:
Notes:
```

Never record the API token.

## 19. Failure Output

When generation cannot complete, report:

```text
status: failed
failureType:
- missing_skill_json
- invalid_equipment_id
- unsupported_slot
- missing_style_reference
- pixellab_unavailable
- pixellab_authentication_failed
- insufficient_pixellab_credits
- invalid_pixellab_request
- pixellab_concurrency_limit
- generation_timeout
- no_passing_candidate
- output_write_failed
- unity_import_pending

failedSkillId:
failedOutputPath:
failureReason:
missingInput:
lastPixelLabJobId:
nextRequiredAction:
```

Do not create a placeholder icon as a failure artifact.

## 20. Hard Boundaries

- Generate static skill icons only.
- Do not generate skill animation PNGs or sprite sheets.
- Do not replace PixelLab with another image tool.
- Do not alter skill balance or gameplay JSON to fit an image.
- Do not invent a new slot, element, or effect.
- Do not expose the PixelLab API token.
- Do not overwrite an accepted icon without preserving or explicitly approving the
  replacement workflow.
- Do not report Unity import completion unless the PNG and `.meta` are present and
  the sprite can be resolved by the expected resource key.
