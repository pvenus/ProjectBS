# Character Animation Generation Guide

## Generate Character Animation

### Mandatory Tool
- Generate the character image only in PixelLab at https://www.pixellab.ai/create-character.
- Do not use ChatGPT, Codex built-in image generation, local image generation tools, or any other image generation service for this workflow.
- If PixelLab cannot be opened or used, stop and report the blocker instead of generating the image elsewhere.

### 1. Open PixelLab
- Open https://www.pixellab.ai/create-character directly in a browser.
- Confirm that the PixelLab Create Character page is loaded before continuing.
- Open the character for which you want to create an animation.

### 2. Add Animation
- Click **+ Add Animation**.

### 3. Character Preview
- Select **South-East** for the Direction in the Character Preview tab.

### 4. Select Animation Type
#### (1) Generate Walking Animation
- If you want to create a walk animation, open the **Walking** section under the **MOVEMENT** tab and select **Walk**.
- Click **Generate in Background** to generate the walk animation.
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
- Click **Generate in Background** to generate the attack animation.

### 5. Edit animation name
- After generating the character's Walk, Attack, and Idle animations, edit the names of the generated animations in the **Animations** tab to **Walk**, **Attack**, and **Idle**, respectively.


### 6. Animation Evaluation
- Evaluation begins once the animation generation process is complete.
- The images from the generated character's 8-directional preview serve as the reference.
- The animation is reviewed based on the preview image corresponding to the **South-East** direction.

#### Evaluation Criteria
- Walk animation evaluation
  - Verify that the walk motion is moving correctly in the designated direction.
  - Check whether the hands and feet move symmetrically, regularly, and naturally.
  - Check whether the direction of your hands and feet aligns with the direction in which your body is moving.
  - Verify that the character's appearance, equipment, and weapons match the reference image.
  - Passing Score: **90 / 100** or higher.

- Attack animation evaluation
  - Check whether the character attacks naturally in a way that suits the weapon they are holding.
  - Check whether the character moves appropriately according to the prompt.
  - Check if the character's body or joints exhibit abnormal movement.
  - Passing Score: **90 / 100** or higher.

- frame to frame animation movement evaluation
  - Check whether the character's movement between frames connects naturally and smoothly.
  - Check that all character elements are displayed in every frame.
  - Check whether the character's central axis is consistent across all frames.
  - Passing Score: **80 / 100** or higher.

- Weapon Review
  - Check to ensure the shape and color of the weapon being held do not break or become distorted.
  - Check to ensure that additional weapons not held by the character do not appear or disappear.
  - Check whether the weapon's direction and angle match those of the base character's weapon.
  - Check to ensure the weapon does not move to the other hand or another location.
  - Check whether you are gripping the weapon firmly and correctly.
  - Verify that the weapon's characteristics and physical properties have been accurately implemented.
  - Passing Score: **80 / 100** or higher.

