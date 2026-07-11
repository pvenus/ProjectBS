# Skill Image Animation Evaluation Guide

## 1. Purpose

This guide evaluates character-independent skill VFX sprite animations generated in PixelLab.

Evaluation must inspect the reference image, the sprite sheet, every individual frame, and the played animation.

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

## 3. Scored Evaluation

Total: **100 points**. Passing score: **85 points or higher**, with no fatal failure.

### 3.1 Transparency and Isolation — 15 points

- 15: Clean alpha background; no unwanted shadow, floor, scene, halo box, or residue.
- 10: Minor removable alpha noise that does not affect readability.
- 5: Noticeable residue or unintended environmental pixels.
- 0: Opaque background or fatal isolation failure.

### 3.2 Safe Margin and No Cropping — 20 points

- 20: Every frame stays fully within the canvas with at least 12.5% practical margin.
- 15: Fully contained, but one or more frames have a narrow margin.
- 8: Effect approaches an edge and is risky for runtime use.
- 0: Any pixel touches an edge or is cropped; fatal failure.

### 3.3 Frame-to-Frame Consistency — 15 points

- 15: Shape, palette, pixel scale, lighting, and detail remain coherent.
- 10: Small flicker or detail variation without identity loss.
- 5: Noticeable morphing, palette shift, or unstable pixel density.
- 0: Severe replacement, duplication, or unrelated frames.

### 3.4 Motion Readability — 15 points

- 15: Anticipation, main action, impact, and ending are immediately readable.
- 10: Main action is readable but timing or staging is weak.
- 5: Motion exists but gameplay meaning is ambiguous.
- 0: Frames do not form a meaningful action.

### 3.5 Center and Spatial Stability — 10 points

- 10: Local motion is centered and stable; runtime can position it reliably.
- 7: Minor center drift that can be corrected by pivot settings.
- 3: Significant unintended drift or jitter.
- 0: Effect travels across or exits the canvas.

### 3.6 Gameplay Silhouette — 10 points

- 10: Strong silhouette and element identity at intended gameplay size.
- 7: Readable with minor clutter.
- 3: Important details collapse or blend together.
- 0: Effect is unreadable at gameplay scale.

### 3.7 Skill Intent and Theme — 10 points

- 10: Motion, shape, palette, and intensity clearly match the skill data.
- 7: General element matches, but utility or impact is under-expressed.
- 3: Weak thematic relationship.
- 0: Contradicts the intended skill.

### 3.8 Loop or Ending Quality — 5 points

- 5: Loop joins cleanly, or one-shot ending dissipates clearly.
- 3: Small pop or timing discontinuity.
- 1: Obvious jump, premature cut, or lingering residue.
- 0: Playback mode is unusable.

## 4. Technical Checks

For every frame verify:

- Same width and height.
- Alpha channel present.
- Transparent corner pixels.
- No edge contact on top, bottom, left, or right.
- Stable pivot candidate near the canvas center.
- No unexpected color-background matte.
- No unintended character or world element.
- Consistent pixel scale and palette.

For the complete animation verify:

- Frame order is correct.
- Frame count matches the requested setting or PixelLab's documented output format.
- The primary impact frame is visually identifiable.
- The skill does not encode world-space travel unnecessarily.
- Loop or one-shot behavior matches the skill design.

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
Canvas:
Requested Frames:
Observed Frames:
Loop Mode:

Fatal Failure Check:
- Transparent background: Pass / Fail
- Character independence: Pass / Fail
- No cropping or edge contact: Pass / Fail
- Consistent canvas and alignment: Pass / Fail
- No unrelated content: Pass / Fail
- Usable frame output: Pass / Fail

Scores:
- Transparency and Isolation: /15
- Safe Margin and No Cropping: /20
- Frame-to-Frame Consistency: /15
- Motion Readability: /15
- Center and Spatial Stability: /10
- Gameplay Silhouette: /10
- Skill Intent and Theme: /10
- Loop or Ending Quality: /5
- Total: /100

Result: Pass / Conditional Pass / Fail
Failure Reasons:
Required Corrections:
Regeneration Prompt Changes:
Notes:
```
