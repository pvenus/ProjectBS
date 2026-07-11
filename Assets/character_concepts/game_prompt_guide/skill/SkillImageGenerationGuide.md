# Skill Image Animation Generation Guide

## 1. Purpose

This guide defines how to create character-independent pixel-art skill VFX animations in PixelLab.

The generated asset represents only the skill effect. Character movement, projectile travel, targeting, collision, and world positioning are handled by the game runtime.

## 2. Mandatory Tool

- Use PixelLab Creator: `https://www.pixellab.ai/create`.
- Use **Create image (Pro)** for the reference image.
- Use **Animate with text (New)** for the animation.
- Do not replace PixelLab with another image generation service.
- If PixelLab is unavailable, stop and report the blocker.

## 3. Asset Requirements

### 3.0 Eligibility

- Do not generate a separate skill VFX animation for melee basic attacks.
- A skill is treated as a melee basic attack when its ID contains `.basic_attack.` and `cast.range <= 1.0`.
- Melee basic attacks use only the character's basic attack animation.
- Ranged basic attacks may use an independent projectile VFX animation.

### 3.1 Character Independence

The asset must not contain:

- A character, body part, hand, face, creature, or caster silhouette.
- A weapon unless the weapon itself is the projectile defined by the skill.
- A scene, terrain, room, sky, decorative background, UI frame, text, or icon border.
- A baked world-space travel path that should be controlled by the game runtime.

### 3.2 Transparent Background

- Enable **Remove background** in both image and animation generation.
- The output must use an alpha-transparent background.
- All four corners must be fully transparent.
- Do not accept solid-color, checkerboard, scenery, floor-texture, or vignette backgrounds.
- A ground seal or impact mark is allowed only when it is part of the skill effect.

### 3.3 Canvas and Safe Area

- Default canvas: **128×128**.
- Default animation length: **8 frames**.
- Use a square canvas unless the skill explicitly requires another ratio.
- Keep the reference effect within **55%** of the canvas width and height.
- Keep the largest animated state within **70%** of the canvas.
- Maintain at least **12.5% transparent padding** on every side.
- No opaque, translucent, glow, particle, smoke, or afterimage pixel may touch a canvas edge.
- The complete effect must remain visible in every frame; cropping is a hard failure.

### 3.4 Runtime Responsibility Boundary

Generate only local VFX deformation and timing in PixelLab:

- Rotation.
- Pulsing.
- Charging.
- Short hovering.
- Local expansion and contraction.
- Fragment separation and convergence.
- Impact flash.
- Dissipation.

Implement these movements in the game runtime instead:

- Straight or curved travel across the battlefield.
- Homing and target tracking.
- Ballistic trajectory.
- Spawn position and rotation.
- Collision, damage area, and hit timing.
- World-space scaling.

Do not use action descriptions such as `flies across the screen`, `exits the frame`, or `fills the entire canvas`.

## 4. Recommended Workflow

### Step 1: Read Skill Data

Identify:

- Skill name and intent.
- Projectile, airborne, ground-impact, area, beam, aura, or burst type.
- Damage element and status effect.
- Cast range, hit range, hit count, and duration.
- Required color and visual identity.
- Whether the animation loops or plays once.

### Step 2: Define a Visual Sequence

Describe the animation in three to five stages:

```text
spawn/idle → anticipation → primary motion → impact/release → fade/recovery
```

Keep the sequence readable at gameplay scale. One animation should communicate one main action.

### Step 3: Generate the Reference Image

In **Create image (Pro)**:

- Output size: `128×128`.
- Remove background: enabled.
- Generate four variations.
- Describe a single isolated skill VFX sprite.
- Explicitly specify maximum canvas occupancy and transparent padding.
- Exclude characters, scenery, text, UI, and unwanted ground elements.

Choose the variation with:

- The clearest silhouette.
- The largest safe margin.
- The fewest stray particles.
- Strong readability at small size.
- A shape suitable for the intended motion.

When PixelLab displays **Pick a Frame**, select one variation and use it as a single image.

### Step 4: Animate the Reference Image

In **Animate with text (New)**:

- Reference image: selected single variation.
- Frame count: `8` by default.
- Remove background: enabled.
- Use an English one-paragraph action description.
- State that the effect is fixed at the canvas center.
- State the maximum animated diameter.
- State that the result must not travel, crop, or touch an edge.
- State whether the animation is looping or one-shot.

### Step 5: Preview and Evaluate

- Play the animation in PixelLab.
- Review every sprite-sheet frame, not only the animated preview.
- Apply `SkillImageEvaluationGuide.md`.
- Regenerate if a fatal failure is present or the total score is below the passing score.

### Step 6: Download, Preserve, Copy, and Evaluate

After a result is accepted, follow `SkillImageDownloadGuide.md` for the complete post-generation workflow.

- Download the selected reference and final animation as separate deliverables.
- Preserve them under `/Users/pvenus/Documents/PixelLab/skill` before Unity copy.
- Copy them to `animation_ref_png` and `animation_png` using the full skill ID filenames.
- Derive the slice grid from actual sheet dimensions; do not assume a fixed grid.
- Save `generation_record.txt` and `evaluation/evaluation_result.txt`.
- Delete ZIP and temporary extraction files only after preservation and verification succeed.

## 5. Prompt Construction Rules

### 5.1 Reference Image Description Order

Use this order:

```text
asset type → gameplay role → central shape → secondary elements → motion-ready silhouette
→ palette → pixel-art style → exclusions → camera/view → safe-area constraints → transparency
```

Required phrases:

```text
single character-independent 2D pixel art skill effect
isolated game VFX sprite only
entire effect centered and fully visible
generous transparent padding on every side
no pixel or glow touching canvas edges
transparent background
```

### 5.2 Animation Action Order

Use this order:

```text
initial state → anticipation → main action → impact/release → ending
→ loop mode → center lock → maximum extent → exclusions → transparency
```

Required phrases:

```text
fixed center
no movement across the canvas
maximum effect diameter 70 percent of canvas
all pixels and glow fully contained
no edge contact
no cropping
no character
transparent background
```

## 6. Recommended Presets

| Effect Type | Canvas | Frames | Reference Occupancy | Animated Maximum | Mode |
|---|---:|---:|---:|---:|---|
| Small projectile loop | 64×64 | 8–12 | 40–50% | 65% | Loop |
| Airborne projectile burst | 128×128 | 8 | 45–55% | 65% | One-shot |
| Ground impact | 128×128 | 8 | 25–40% | 70% | One-shot |
| Area seal/field | 128×128 | 8–12 | 25–35% | 70% | Loop or one-shot |
| Large boss burst | 256×256 | 4–8 | 30–45% | 70% | One-shot |

## 7. Output Record

Record the following after generation:

```text
Skill:
Source JSON:
Effect Type:
PixelLab Tool:
Reference Prompt:
Selected Variation:
Animation Prompt:
Canvas:
Frame Count:
Loop Mode:
Remove Background:
PixelLab Page:
Generation Status:
Evaluation Status:
Notes:
```
