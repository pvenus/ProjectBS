# Character Image Generation Guide

> Deprecated execution contract. Retained as PixelLab character profile
> evidence. Replaced by
> `AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md`.


## Master Concept Reference

Before using this document, read and apply:

AgentDocs/planning-guides/common/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## Generated Image Storage Reference

Before generating, downloading, evaluating, promoting, or resolving a generated
image, read and apply:

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
```

This storage guide is mandatory. Its `Assets/ImagesGenerated` contract takes
precedence over legacy generated-image output paths under `Assets/Resources`.
Existing reference-only assets may remain in their documented legacy locations.

## Current Executable Contract

- **Guide type:** provider-generation workflow guide.
- **Responsibility:** create a PixelLab character result and return provider-side result references plus a generation record.
- **Inputs:** approved character identity, design concept, grade, size, and the resolved generation prompt.
- **Preconditions:** the master concept and prompt-authoring contract are readable; the target character is unambiguous.
- **Handoff:** `providerResultRefs`, `generationRecord`, and a request for the separate download/preservation stage.
- **Mutation boundary:** this guide must not download files, evaluate candidates, write project assets, create Unity metadata, or perform Git work.

Authority is concern-specific: the master concept controls visual prohibitions,
the approved character plan controls identity, and this guide controls only the
PixelLab generation operation. If those authorities conflict, stop and report
`generation_authority_conflict`; do not choose one silently.

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
- If the character is a quadruped, set **Generation Mode** to **Pro** and select the appropriate **Quadruped** model.

### 4. Generation Settings

- **Generation Mode:** Pro
- **Camera View:** High Top-Down
- **Detail:** Highly detailed
- **Outline:** Black outline


### 5. Character Description
- Write a character description based on a design concept rooted in traditional Korean design standards.
- The final prompt must be written as one English paragraph that can be pasted directly into PixelLab.

#### Character Prompt Required Elements

The PixelLab image prompt must include the following elements:

- character name
- grade
- characterType
- race, faction, and world tone
- body shape and readable silhouette
- clothing, equipment, and weapon
- combat role and pose
- dominant colors
- no extra characters
- no gore
- High top-down game character sprite
- highly detailed
- black outline
- full body visible
- transparent background
- clean readable silhouette

### 6. Image Size
- Adjust the image width, height, and aspect ratio to fit the target output.
- Based on the recorded information, select small (48px), medium (64px), large (128px), or extra-large (256px).
- The size of the reference playable character is 64px.

### 7. Generate in PixelLab
- Click **Generate Pro Character** inside PixelLab.
- The generated result must come from PixelLab; do not substitute a generated image from another tool.
- Search for the generated character using the generated image prompt.
- Select the generated character.
- Click **Add tag**.
- Enter the character name.
- Enter the character grade.

### 8. Legacy Download and Evaluation Appendix (Non-executable)

The following historical procedure is retained only to explain old artifacts.
It is not part of this guide's executable workflow. A current task must stop
after Step 7 and hand off its provider references and generation record to a
separate download/preservation task, followed by immutable evaluation and
PASS-only promotion.
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
- Save the evaluation result as a text file (`evaluation_result.txt`) in the character folder.
- The evaluation result should include:
  - Rotation Validation score
  - Prompt Accuracy score
  - Reference Style Compatibility score
  - Average score (if applicable)
  - Pass / Fail result
  - Failure reason (if failed)
