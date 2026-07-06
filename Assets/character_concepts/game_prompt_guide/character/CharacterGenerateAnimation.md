# Character Animation Generation Guide

## Generate Character Animation

### Mandatory Tool
- Generate the character animation only in PixelLab at https://www.pixellab.ai/create-character.
- Do not use ChatGPT, Codex built-in image generation, local image generation tools, or any other image generation service for this workflow.
- If PixelLab cannot be opened or used, stop and report the blocker instead of generating the animation elsewhere.

### 1. Open PixelLab
- Open https://www.pixellab.ai/create-character directly in a browser.
- Confirm that the PixelLab Create Character page is loaded before continuing.
- Search the configured PixelLab export root for the completed image generation result folder.
- Use the image generation result data as the primary source for the PixelLab search query:
  - `<PixelLabExportRoot>/<CharacterName>_<Grade>`
  - `evaluation_result.txt`
  - exported `rotations` images, if present
  - saved image prompt or character description, if present
- Search for the generated character using the image generation result data, character name, and grade.
- Open the existing PixelLab character page for which you want to create an animation.
- Do not generate a new character image in this step.

#### Image Export Result Lookup

Before searching PixelLab, locate the image generation result folder:

```text
<PixelLabExportRoot>/<CharacterName>_<Grade>
```

Use the result folder to confirm:

- Image generation was completed.
- `evaluation_result.txt` exists, if the image generation prompt saved it.
- The generated image passed or has a documented failure reason.
- The `rotations` folder exists, if the image export was downloaded.

If multiple PixelLab characters match the search query, prefer the character whose title, prompt, size, direction count, and visual preview best match the image generation result folder.

### 2. Add Animation
- Click **+ Add Animation**.

### 3. Character Preview
- Select **South-East** for the Direction in the Character Preview tab.

### 4. Select Animation Type
#### (1) Generate Walking Animation
- If you want to create a walk animation, open the **Walking** section under the **MOVEMENT** tab and select **Walk**.
- Click **Generate in Background** to generate the walk animation.
- After generation, this animation must be renamed to **Move** in the **Animations** tab.
#### (2) Generate Attack Animation
- Select **Custom Animation V3** in the **CUSTOM** tab.
- In the **Action Description**, generate and enter an attack motion prompt that matches the hand holding the weapon and the weapon itself.
- Select **8 Frames** for the **Frame Count**.
- Check the **Keep first frame (idle pose)** checkbox.
- Click **Generate in Background** to generate the attack animation.
#### (3) Generate Idle Animation
- Select **Custom Animation V3** in the **CUSTOM** tab.
- In the **Action Description**, generate and enter an "Idle" state prompt that matches the character's appearance.
- Select **6 Frames** for the **Frame Count**.
- Check the **Keep first frame (idle pose)** checkbox.
- Click **Generate in Background** to generate the idle animation.

### 5. Edit animation name
- After generating the character's movement, attack, and idle animations, edit the names of the generated animations in the **Animations** tab to **Move**, **Attack**, and **Idle**, respectively.
- The movement animation may be generated from PixelLab's **MOVEMENT / Walking / Walk** preset, but the final animation name must be **Move**, not **Walk**.

### 6. Download

On the image page, use the `Export` button to download the character animation images.

After downloading, extract the archive.

The extracted folder should normally contain this structure:

```text
animations/
  idle/
  move/
  attack/
```

Each animation type folder should contain directional folders:

```text
south-east/
south-west/
north-east/
north-west/
```

- Create a folder using the format `<PixelLabExportRoot>/<CharacterName>_<Grade>`.
- Store all exported files in the created folder.
- Perform the evaluation using the PNG images in the `animations` folder.

### 7. Animation Evaluation

- Evaluation begins once the animation generation process is complete.
- The images from the generated character's 8-directional preview serve as the reference.
- The animation is reviewed based on the preview image corresponding to the **South-East** direction.
- Perform the evaluation according to the criteria defined in `EvaluationAnimationGuide.md`.
- The evaluation result must include every evaluation item defined in `EvaluationAnimationGuide.md`.
- Save the evaluation result as `evaluation_animation_result.txt` in the character folder.
- The evaluation result should include:
  - Evaluation item
  - Evaluation score
  - Pass / Fail result
  - Failure reason (if applicable)
  - Additional notes (if applicable)
