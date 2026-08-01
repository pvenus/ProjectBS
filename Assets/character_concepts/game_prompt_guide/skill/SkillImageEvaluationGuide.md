# Skill Image Animation Evaluation Guide

## 1. Purpose

This guide evaluates character-independent skill VFX sprite animations generated in PixelLab.

Evaluation must inspect the reference image, the sprite sheet, every individual frame, and the played animation.

Use the target skill JSON and generation record as the source of truth for the
effect's activation moment, direction, range, element, damage, buff, debuff,
summon, and other gameplay intent. Reuse the design principles from
`SkillIconGenerationPrompt.md` only where they apply to character-independent VFX:
one dominant readable effect family, a visible activation moment, connected
secondary motion, one skill-linked Korean traditional motif when required, a
controlled palette, and a clear visual hierarchy.

Do not import the icon prompt's opaque full-bleed background contract into this
guide. Production skill VFX remain isolated on an alpha-transparent canvas with no
scene, floor, card, frame, or internal background painting.

## 2. Fatal Failure Conditions

Any item below immediately produces **Fail**, regardless of total score:

- Background is not alpha-transparent.
- A character, hand, body part, face, or unintended creature appears.
- Any effect, glow, smoke, particle, or afterimage is cropped.
- Any non-transparent pixel touches a canvas edge.
- Frames use inconsistent canvas dimensions or alignment.
- The animation contains severe object replacement, duplicated effects, or unrelated imagery.
- Text, watermark, UI frame, scenery, or unintended floor texture appears.
- The exported file cannot be separated into usable animation frames.
- The effect communicates a different skill, direction, range, impact type, buff,
  debuff, or summon behavior from the source data.
- A static weapon, equipment item, inventory object, badge, logo, or icon replaces
  the requested active VFX.
- The primary effect and its required impact, trail, aura, or secondary motion read
  as unrelated detached objects.
- A dominant Japanese, Chinese, Western, or generic foreign motif replaces the
  required Korean traditional style.

## 3. Scored Evaluation

Total: **100 points**. Passing score: **85 points or higher**, with no fatal failure.

### 3.1 Transparency and Isolation — 10 points

- 10: Clean alpha background; no unwanted shadow, floor, scene, halo box, or residue.
- 7: Minor removable alpha noise that does not affect readability.
- 3: Noticeable residue or unintended environmental pixels.
- 0: Opaque background or fatal isolation failure.

### 3.2 Safe Margin and No Cropping — 15 points

- 15: Every frame stays fully within the canvas with at least 12.5% practical margin.
- 11: Fully contained, but one or more frames have a narrow margin.
- 5: Effect approaches an edge and is risky for runtime use.
- 0: Any pixel touches an edge or is cropped; fatal failure.

### 3.3 Frame-to-Frame Consistency — 10 points

- 10: Shape, palette, pixel scale, lighting, and detail remain coherent.
- 7: Small flicker or detail variation without identity loss.
- 3: Noticeable morphing, palette shift, or unstable pixel density.
- 0: Severe replacement, duplication, or unrelated frames.

### 3.4 Motion Readability — 15 points

- 15: Anticipation, main action, impact, and ending are immediately readable.
- 10: Main action is readable but timing or staging is weak.
- 5: Motion exists but gameplay meaning is ambiguous.
- 0: Frames do not form a meaningful action.

### 3.5 Direction, Center, and Spatial Stability — 10 points

- 10: The source-driven axis is immediately readable and local motion stays stable
  around a reliable runtime pivot.
- 7: Direction is correct with minor center drift that can be corrected by pivot
  settings.
- 3: Direction is ambiguous, or significant unintended drift or jitter appears.
- 0: Direction contradicts the source, or the effect travels across or exits the
  canvas when runtime should control that movement.

### 3.6 Gameplay Silhouette — 10 points

- 10: Strong silhouette and element identity at intended gameplay size.
- 7: Readable with minor clutter.
- 3: Important details collapse or blend together.
- 0: Effect is unreadable at gameplay scale.

### 3.7 Skill Intent and Effect Connection — 15 points

- 15: Activation, motion, shape, range, element, intensity, and connected impact,
  trail, aura, or secondary motion clearly match the skill data.
- 11: General intent and connection are correct, but utility, impact, or range is
  under-expressed.
- 5: Broad element matches, but activation or effect connection is ambiguous.
- 0: Contradicts the intended skill.

### 3.8 Korean Traditional Style, Palette, and Hierarchy — 10 points

- 10: One skill-linked Korean traditional motif or motion language is readable,
  palette roles are controlled, and hierarchy stays `primary effect > connected
  secondary motion > traditional accent` in every important frame.
- 7: Style and palette are generally correct with minor loss of motif clarity or
  hierarchy during motion.
- 3: Generic East Asian decoration, excess colors, or competing secondary effects
  weaken the project style.
- 0: A foreign motif dominates, the palette communicates the wrong element or role,
  or the traditional accent overwhelms the skill effect.

If the source and generation record explicitly mark the traditional motif as not
applicable, evaluate palette and hierarchy only and record the motif check as
`Not Applicable`; do not grant or deduct points for an invented motif.

### 3.9 Loop or Ending Quality — 5 points

- 5: Loop joins cleanly, or one-shot ending dissipates clearly.
- 3: Small pop or timing discontinuity.
- 1: Obvious jump, premature cut, or lingering residue.
- 0: Playback mode is unusable.

## 4. Technical Checks

Before scoring, extract or reconstruct from the skill JSON and generation record:

```text
activationMoment
primaryEffectShape
directionOrCompositionAxis
connectedSecondaryMotion
koreanTraditionalMotif or not_applicable
elementAndRolePalette
likelyWrongObjects
expectedLoopOrEnding
```

If these fields cannot be supported by the source or generation evidence, mark the
affected check `Insufficient Evidence`; do not invent an element, motif, direction,
or effect merely to complete the score.

For every frame verify:

- Same width and height.
- Alpha channel present.
- Transparent corner pixels.
- No edge contact on top, bottom, left, or right.
- Stable pivot candidate near the canvas center.
- No unexpected color-background matte.
- No unintended character or world element.
- Consistent pixel scale and palette.
- One dominant effect family remains identifiable; secondary sparks, trails, rings,
  smoke, or afterimages are source-supported and visibly connected.
- The frame does not resemble a static weapon, inventory item, icon, badge, logo,
  card, or UI glyph.
- Korean traditional accents, when required, remain simplified and subordinate to
  the effect rather than becoming a separate decorative object.
- No Japanese shrine or torii, oni, kamon, Chinese coin or dragon emblem, Western
  heraldry, or other unintended foreign motif appears.

For the complete animation verify:

- Frame order is correct.
- Frame count matches the requested setting or PixelLab's documented output format.
- The primary impact frame is visually identifiable.
- The anticipation, activation point, travel or expansion axis, impact or utility
  moment, and dissipation agree with the source data and generation record.
- Direction is recorded as one explicit source-driven axis or a centered local
  composition; it is not inferred from a generic diagonal default.
- Required impact, aura, debuff pulse, summon signal, or trail begins at the primary
  effect's center, outline, contact point, or motion path instead of floating as an
  unrelated object.
- Visual hierarchy remains stable through the key frames: primary effect first,
  connected secondary motion second, Korean traditional accent third.
- Palette roles remain stable: primary effect, brighter semantic accent, restrained
  traditional accent, and transparent background.
- The skill does not encode world-space travel unnecessarily.
- Loop or one-shot behavior matches the skill design.

When evaluating downloaded production candidates, also verify:

- The reference and animation are stored as separate files.
- The preserved evaluation PNG and Unity destination PNG have matching SHA-256 values.
- Sheet width and height, frame cell size, columns, rows, and usable frame count are recorded.
- Unity slice count and generated clip frame count match the usable frame count.
- The evaluation uses the exact preserved file copied into Unity, not a preview or recompressed duplicate.

## 5. Result Classification

- **Pass**: 85–100, no fatal failure.
- **Conditional Pass**: 75–84, no fatal failure, and issues are safely correctable without regeneration.
- **Fail**: Below 75 or any fatal failure.

Assets intended for direct production use require **Pass**. Conditional Pass assets must be corrected and evaluated again.

## 6. Evaluation Output Format

```text
Skill Image Animation Evaluation

Skill:
Source JSON:
Asset Path or PixelLab Page:
Reference Asset Path:
Animation Asset Path:
Unity Reference Path:
Unity Animation Path:
Canvas:
Sheet Size:
Frame Cell Size:
Columns / Rows:
Requested Frames:
Observed Frames:
Usable / Unity Sliced / Clip Frames:
Loop Mode:
Reference SHA-256:
Animation SHA-256:
Unity Copy Checksum Match: Pass / Fail / Not Applicable

Fatal Failure Check:
- Transparent background: Pass / Fail
- Character independence: Pass / Fail
- No cropping or edge contact: Pass / Fail
- Consistent canvas and alignment: Pass / Fail
- No unrelated content: Pass / Fail
- Usable frame output: Pass / Fail
- Source-driven skill intent and direction: Pass / Fail / Insufficient Evidence
- Active VFX rather than static item or icon: Pass / Fail
- Primary and secondary effect connection: Pass / Fail / Not Applicable
- Korean traditional style and no foreign motif: Pass / Fail / Not Applicable

Design Evidence:
- Activation Moment:
- Primary Effect Shape:
- Direction or Composition Axis:
- Connected Secondary Motion:
- Korean Traditional Motif:
- Element and Role Palette:
- Likely Wrong Objects:
- Expected Loop or Ending:

Scores:
- Transparency and Isolation: /10
- Safe Margin and No Cropping: /15
- Frame-to-Frame Consistency: /10
- Motion Readability: /15
- Direction, Center, and Spatial Stability: /10
- Gameplay Silhouette: /10
- Skill Intent and Effect Connection: /15
- Korean Traditional Style, Palette, and Hierarchy: /10
- Loop or Ending Quality: /5
- Total: /100

Result: Pass / Conditional Pass / Fail
Failure Reasons:
Required Corrections:
Regeneration Prompt Changes:
Notes:
```

## 7. Existing Result Migration and Playback Evidence

An existing completed evaluation may be normalized into:

```text
C:\github\design_evaluation\skill_animation\{skillId}
```

only with `format_existing` semantics.

- Do not re-score or change the existing result, category scores, findings, or
  required actions.
- Stage exact copies of the preserved reference and animation sheets and record
  SHA-256 equality with the Unity production files.
- Derive individual PNG frames in left-to-right, top-to-bottom row-major order.
- Preserve a contact sheet and a playback GIF for motion review.
- Record usable frame count, frame order, nominal FPS, encoded GIF frame delays,
  loop mode, contact-sheet SHA-256, and GIF SHA-256.
- GIF is not a production artifact and must not be used to judge source alpha,
  edge contact, transparent corners, cropping, or pixel fidelity.
- All fatal alpha/crop checks remain based on the original PNG sheets and
  individual PNG frames.
- A playback GIF may loop for review when the legacy loop mode is unavailable,
  but the normalized record must state that the review loop is not evidence of
  the runtime loop mode.
- Keep Unity `.meta` configuration, Editor reimport, sliced sub-assets, clip
  generation, and runtime binding as separate evidence fields.
- Failed, blocked, or never-generated assets are not migrated as completed
  records.
