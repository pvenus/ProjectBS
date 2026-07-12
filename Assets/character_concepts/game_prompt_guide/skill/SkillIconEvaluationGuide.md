# Skill Icon Evaluation Guide

## 1. Purpose

This guide defines how to evaluate static pixel-art skill icons generated for:

```text
Assets/Resources/skill/icon/skill
```

Evaluation verifies file correctness, skill meaning, project style, slot and grade
distinction, small-size readability, reference consistency, and Unity readiness.

This guide evaluates static UI icons only. Do not use it to evaluate animated skill
VFX, character sprites, sprite sheets, or UI panels.

## 2. Required References

Read:

```text
Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/SkillJsonGuide.md
Assets/character_concepts/game_prompt_guide/skill/so_guide/EquipmentSkillSO.md
Assets/character_concepts/game_prompt_guide/skill/design/SkillDegineGuide.md
```

Use the target skill JSON as the source of truth for icon meaning.

## 3. Evaluation Inputs

Required:

```text
skillSourcePath
equipmentId
iconPath
```

Optional:

```text
styleReferencePaths
lowerGradeIconPath
siblingIconPaths
unityMetaPath
```

Definitions:

- `styleReferencePaths`: approved icons used or intended as style anchors.
- `lowerGradeIconPath`: the accepted lower-grade icon for an inherited skill.
- `siblingIconPaths`: icons displayed in the same character loadout or skill set.
- `unityMetaPath`: normally `{iconPath}.meta`.

If a required input cannot be inspected, do not infer a passing result. Report
`insufficient_evidence`.

## 4. Evaluation Boundary

Evaluation is read-only.

Do not:

- Generate, edit, resize, recolor, rename, move, or delete an icon.
- Call PixelLab generation endpoints.
- Modify skill JSON or Unity `.meta` files.
- Replace a failed icon with a placeholder.
- Mark Unity import as complete without evidence.

Corrections may be proposed as text, including revised PixelLab prompt phrases, but
must not be applied during evaluation.

## 5. Source and Identity Check

Parse `equipmentId` using:

```text
skill.{domain}.{character_name}.{grade}.{slot}.{skill_name}
```

Confirm:

- The source JSON exists and is valid.
- JSON `equipmentId` exactly matches the requested `equipmentId`.
- Grade is 1, 2, or 3.
- Slot is supported by the source design and runtime.
- `iconPath` is exactly:

```text
Assets/Resources/skill/icon/skill/{equipmentId}.icon.png
```

Extract the same semantic fields used during generation:

1. slot
2. skill type
3. targeting type
4. cast movement
5. component type
6. projectile or movement behavior
7. damage
8. buffs and debuffs
9. effect types
10. skill name

Summarize the expected:

```text
slotFamily
visualFamily
primarySymbol
secondaryEffect
composition
elementFamily
roleFamily
paletteFamily
intensity
```

## 6. File and Import Check

Required file checks:

- PNG decodes successfully.
- Width is exactly 80 pixels.
- Height is exactly 80 pixels.
- Color mode is RGBA.
- The file is a single icon, not a sprite sheet.
- The full `equipmentId` is used in the filename.
- `.png.meta` exists when Unity import completion is claimed.
- The expected resource key can resolve the icon.

Report the SHA-256 hash of the PNG. Use hashes to detect byte-identical duplicates
among sibling icons and existing skill icons.

Byte-identical reuse is not automatically a visual failure when reuse is explicitly
approved. Without explicit approval, report it as a fatal identity failure.

## 7. Inspection Sizes

Inspect the icon at:

```text
80 x 80: source-size craftsmanship and border quality
32 x 32: normal small-UI readability
16 x 16: optional stress test, not a pass requirement
```

When scaling for inspection, use nearest-neighbor scaling. Do not blur or resample
the source during evaluation.

## 8. Fatal Failure Conditions

Any fatal failure produces `Fail` regardless of score.

### 8.1 Contract Failures

- Missing or unreadable PNG.
- Wrong output path or filename.
- Canvas is not 80 x 80.
- Not a single static icon.
- Wrong `equipmentId`, grade, or slot.

### 8.2 Meaning Failures

- The icon communicates a different skill intent.
- Basic, active, or passive identity is materially wrong.
- An element, effect, weapon, or role absent from source data dominates the icon.
- The primary symbol cannot be identified at 32 x 32.
- Different skills use byte-identical icons without explicit reuse approval.

### 8.3 Style Failures

- Not recognizable as pixel art.
- Unintended smoothing, vector edges, or photorealistic rendering dominates.
- Text, letters, numbers, logo, watermark, or animation grid is present.
- Detailed scenery or unrelated objects obscure the skill symbol.
- The icon lacks the required background or usable border.
- The icon is visibly broken, cropped, or contains generation artifacts.

### 8.4 Grade Family Failures

- An inherited higher-grade skill loses the lower-grade primary symbol or direction.
- Grade enhancement changes the skill into an unrelated image.
- A lower-grade version appears substantially more powerful than its inherited
  higher-grade version without design justification.

## 9. Slot Evaluation Rules

### 9.1 Basic Attack

Expected:

- Repeated attack source is immediately readable.
- One weapon, claw, projectile, or compact impact dominates.
- Directional or diagonal composition is preferred.
- Effect density remains low.
- Full-character detail is avoided unless necessary.

### 9.2 Active Skill

Expected:

- Unique action or gameplay identity is immediately readable.
- Motion, impact, element, control, or area intent is visible.
- One dominant action and no more than two secondary effects are present.
- `active_2` and `active_3` may be stronger than `active_1`, but only when supported
  by the source design.

### 9.3 Passive Skill

Expected:

- Persistent role, trigger, defense, buff, or condition is represented.
- Emblem, armor, shield, rune, crest, ring, or aura symbolism is preferred.
- Composition is centered and approximately symmetrical.
- Excessive directional motion is avoided.

## 10. Grade Evaluation Rules

### Grade 1

- Simple silhouette.
- Minimal secondary effects.
- Restrained highlights and ornament.

### Grade 2

- Preserves inherited identity.
- Adds one controlled trail, aura, highlight, or secondary detail.
- Reads as stronger than Grade 1 without becoming crowded.

### Grade 3

- Preserves inherited identity.
- Uses the strongest controlled contrast and accent.
- Adds deliberate ornament or effect density while keeping the main symbol readable.

Grade progression is evaluated only against actual inherited versions. Do not fail a
standalone skill merely because a lower-grade icon does not exist.

## 11. Scoring

Score only after fatal failures are checked.

### 11.1 Skill Intent Readability: 25

- 22-25: Exact skill action, role, and key effect are immediately understood.
- 17-21: Correct general intent with minor ambiguity.
- 10-16: Broad category is visible but important meaning is missing.
- 0-9: Wrong or unreadable intent.

### 11.2 Project Style Match: 20

- 18-20: Pixel density, outline, shading, border, and visual tone match references.
- 14-17: Mostly consistent with small deviations.
- 8-13: Noticeably different but still usable after correction.
- 0-7: Belongs to a different visual language.

### 11.3 Small-Size Silhouette: 20

- 18-20: Primary symbol remains clear at 32 x 32.
- 14-17: Readable with minor detail loss.
- 8-13: Meaning becomes ambiguous at 32 x 32.
- 0-7: Primary symbol collapses or becomes noise.

### 11.4 Slot and Grade Distinction: 15

- 13-15: Slot and grade are correctly expressed and family identity is preserved.
- 10-12: Correct with minor intensity or composition issues.
- 6-9: Slot or grade signal is weak.
- 0-5: Slot or grade is materially wrong.

### 11.5 Palette and Contrast: 10

- 9-10: Element and role palette is accurate with strong controlled contrast.
- 7-8: Correct palette with minor contrast or color-count issues.
- 4-6: Usable but muddy, oversaturated, or weakly related to source.
- 0-3: Palette communicates the wrong identity or destroys readability.

### 11.6 Composition and Border Quality: 10

- 9-10: One dominant symbol, controlled effects, stable background and border.
- 7-8: Good composition with small spacing or border inconsistencies.
- 4-6: Crowded, underfilled, or visibly inconsistent.
- 0-3: Broken layout, crop, or unusable border.

Total:

```text
100 points
```

## 12. Result Rules

```text
Pass:
- 85-100
- no fatal failure
- no unresolved required evidence

Conditional Pass:
- 75-84
- no fatal failure
- correctable without regeneration or acceptable with explicit approval

Fail:
- below 75
- or any fatal failure
- or insufficient evidence for a required contract check
```

Do not round a score upward to reach a threshold.

## 13. Sibling and Duplicate Review

Compare against `siblingIconPaths` when provided.

Confirm:

- Loadout icons are distinguishable at 32 x 32.
- Basic, active, and passive icons do not share the same silhouette accidentally.
- Element colors do not create misleading equivalence.
- No unapproved byte-identical reuse exists.
- Similar inherited skills remain a family without becoming indistinguishable.

If sibling icons are unavailable, mark this section `not_evaluated`, not `Pass`.

## 14. Required Corrections

For every failed or deducted item, record:

```text
Observed issue
Evidence at 80 x 80 or 32 x 32
Expected rule
Required correction
Regeneration required: yes or no
```

When regeneration is required, propose changes to the English PixelLab description.
Keep proposed changes minimal and targeted.

Examples:

```text
Replace "detailed battlefield scene" with "one centered weapon symbol on a simple
opaque icon background".

Add "readable at 32 by 32 pixels, one primary symbol, no unrelated objects".

Reduce Grade 1 effect wording from "explosive radiant aura" to "one small controlled
highlight".
```

## 15. Evaluation Output

Use this structure:

```text
Skill Icon Evaluation

Skill ID:
Source JSON:
Icon Path:
Unity Meta Path:
Grade:
Slot:
Expected Classification:
Canvas / Mode:
SHA-256:
Style Reference Paths:
Lower Grade Icon:
Sibling Icons:

Fatal Failure Check:
- File and path contract: Pass / Fail / Insufficient Evidence
- Skill meaning: Pass / Fail / Insufficient Evidence
- Pixel-art project style: Pass / Fail / Insufficient Evidence
- Text, logo, or animation grid absent: Pass / Fail
- Background and border usable: Pass / Fail
- Grade family identity: Pass / Fail / Not Evaluated
- Unapproved duplicate absent: Pass / Fail / Not Evaluated

Scores:
- Skill Intent Readability: /25
- Project Style Match: /20
- Small-Size Silhouette: /20
- Slot and Grade Distinction: /15
- Palette and Contrast: /10
- Composition and Border Quality: /10
- Total: /100

Result: Pass / Conditional Pass / Fail
Failure Reasons:
Required Corrections:
Regeneration Prompt Changes:
Unity Import Status:
Notes:
```

## 16. Failure Output

If evaluation itself cannot be completed:

```text
status: failed
failureType:
- missing_skill_json
- invalid_skill_json
- equipment_id_mismatch
- missing_icon
- invalid_png
- unsupported_slot
- missing_required_reference
- insufficient_evidence
- evaluation_write_failed

failedSkillId:
failedIconPath:
failureReason:
missingInput:
nextRequiredAction:
```

## 17. Final Checklist

- [ ] Source JSON and equipment ID match.
- [ ] Icon path and filename match the contract.
- [ ] PNG is 80 x 80 RGBA.
- [ ] Fatal failures were checked before scoring.
- [ ] Icon was inspected at 80 x 80 and 32 x 32.
- [ ] Skill intent, slot, grade, palette, composition, and style were scored.
- [ ] Lower-grade family identity was checked when applicable.
- [ ] Sibling distinction and duplicate hashes were checked when evidence exists.
- [ ] Every deduction includes evidence and a correction.
- [ ] Evaluation did not modify or regenerate any asset.
