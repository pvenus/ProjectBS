# Character Animation Download And Evaluation Prompt

Use this prompt when downloading PixelLab animation images, preserving the character `animations` folder, evaluating animation images, and copying converted PNGs into Unity resources.

```text
작업 폴더 = {project_root}

아래 입력을 기준으로 CharacterAnimationDownloadGuide.md 및 EvaluationAnimationGuide.md를 먼저 읽고, 해당 가이드의 절차와 실패 규칙을 그대로 따라 PixelLab 애니메이션 다운로드, 평가, 변환 복사를 진행해줘.

Input:
- projectRoot: {project_root}
- PixelLabExportRoot = {PixelLab_export_저장_루트_절대경로}
- targetCharacterFolder = {PixelLabExportRoot_아래_대상_캐릭터_폴더_절대경로}

참조 가이드:
- Assets/character_concepts/game_prompt_guide/character/CharacterAnimationDownloadGuide.md
- Assets/character_concepts/game_prompt_guide/character/EvaluationAnimationGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterGenerateImage.md

작업:
1. targetCharacterFolder를 기준으로 characterName, grade, 이미지 생성에 사용한 PixelLab Prompt, 이미지 생성 평가 결과를 찾는다.
2. {targetCharacterFolder}/animations가 이미 있고 새 교체 요청이 아니면 기존 animations를 사용한다.
3. animations가 없거나 새 교체가 필요한 경우, https://www.pixellab.ai/create-character 페이지에서 이미지 생성에 사용한 PixelLab Prompt와 태그로 기존 PixelLab 캐릭터를 검색해 연다.
4. PixelLab Export 후에는 압축 해제 결과에서 animations 폴더만 targetCharacterFolder로 이동/교체한다.
5. CharacterAnimationDownloadGuide.md의 Required Folder Structure Hard Fail 조건을 통과하지 못하면 즉시 실패 처리하고 평가/변환/Unity 복사를 진행하지 않는다.
6. EvaluationAnimationGuide.md 기준으로 {targetCharacterFolder}/animations를 평가하고, 평가 결과는 {targetCharacterFolder}/evaluation_animation_result.txt에 저장한다.
7. 평가 Pass / Fail과 관계없이 가이드에 따라 converted 생성과 Unity 리소스 복사를 계속한다. 단, 폴더 구조 hard fail은 예외로 즉시 중단한다.

Output:
- Character:
- Grade:
- Target Character Folder:
- Image Generation Prompt:
- PixelLab Lookup Result:
- PixelLab Page:
- Download Performed:
- Replacement Performed:
- Animations Folder:
- Folder Structure Check:
- Hard Fail:
- Direction Check:
- Missing Direction Handling:
- Evaluation File:
- Evaluation Result:
- Converted Folder:
- Unity Resource Folder:
- Final File Count:
- Pass / Fail:
- Failure Reason:
- Cleanup:
- Notes:

실패 시 Output:
- status: failed
- failureType:
  - missing_target_character_folder
  - missing_image_generation_prompt
  - pixellab_character_not_found
  - pixellab_export_failed
  - animation_folder_hard_fail
  - unity_copy_failed
- 실패 원인
- Hard Fail 여부
- 보존한 원본 폴더
- 재시도에 필요한 입력

검증:
- targetCharacterFolder가 존재해야 한다.
- 기존 animations 폴더는 새 교체 요청이 없으면 보존해야 한다.
- Required Folder Structure Hard Fail이면 평가/변환/Unity 복사를 진행하지 않아야 한다.
- evaluation_animation_result.txt가 생성되어야 한다.
- converted 폴더와 Unity Resource Folder의 최종 파일 수를 보고해야 한다.

주의:
- 세부 정책과 판단 기준은 프롬프트가 아니라 CharacterAnimationDownloadGuide.md를 원본으로 따른다.
- PixelLab Page URL은 입력으로 직접 받지 않는다.
- 다운로드와 Export는 반드시 PixelLab에서 수행한다.
- source animations 파일은 삭제하거나 이름 변경하지 않는다.
```
