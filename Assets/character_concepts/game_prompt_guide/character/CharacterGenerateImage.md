# Character Image Generation Guide

## Generate Character Image

### Mandatory Tool
- Generate the character image only in PixelLab at https://www.pixellab.ai/create-character.
- Do not use ChatGPT, Codex built-in image generation, local image generation tools, or any other image generation service for this workflow.
- If PixelLab cannot be opened or used, stop and report the blocker instead of generating the image elsewhere.

### 1. Open PixelLab
- Open https://www.pixellab.ai/create-character directly in a browser.
- Confirm that the PixelLab Create Character page is loaded before continuing.

### 2. Create Character
- Click **Create**.
- In the popup panel, select the **Create from text** tab.

### 3. Character Type
- Select **Character Type** to match the character concept.
- If the character is a quadruped, select the appropriate **Quadruped** model.

### 4. Generation Settings
- **Generation Mode:** Pro
- **Camera View:** High Top-Down
- **Detail:** Highly detailed
- **Outline:** Black outline

### 5. Character Description
- Write the character description based on the design concept.

### 6. Image Size
- Adjust the image width, height, and aspect ratio to fit the target output.

### 7. Generate in PixelLab
- Click **Generate v3 Character** inside PixelLab.
- The generated result must come from PixelLab; do not substitute a generated image from another tool.
- Search for the generated character using the generated image prompt.
- Select the generated character.
- Click **Add tag**.
- Enter the character name.
- Enter the character grade.

### 8. Image Evaluation
- Click **Export** and download the generated images.
- Save the downloaded files under the configured PixelLab export root.
- Create a folder using the format `<PixelLabExportRoot>/<CharacterName>_<Grade>`.
- Store all exported files in the created folder.
- Perform the evaluation using the PNG images in the `rotations` folder.

#### Evaluation Criteria
- Rotation Validation
  - Verify that all 8 directional rotation images are correctly generated and arranged.
  - Passing Score: **90 / 100** or higher.

- Prompt Accuracy
  - Evaluate whether the generated character matches the intended prompt.
  - Passing Score: **80 / 100** or higher.

- Reference Style Compatibility
  - Reference Image Directory: `Assets/Resources/character`
  - Randomly select 5 reference images from the reference image directory.
  - Compare the generated image with each selected reference image.
  - Evaluate harmony with the game's visual style rather than pixel-level similarity.
  - Use the average score from the 5 comparison results as the final score.
  - Passing Score: **70 / 100** or higher.

#### Retry Rule
- If any evaluation criterion fails, regenerate the character once.
- Record the following information for every failed attempt:
  - Failure reason
  - Evaluation scores
  - Generated images
- Save the evaluation result as a text file (`evaluation_result.txt`) in the character folder.
- The evaluation result should include:
  - Rotation Validation score
  - Prompt Accuracy score
  - Reference Style Compatibility score
  - Average score (if applicable)
  - Pass / Fail result
  - Failure reason (if failed)
