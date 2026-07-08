# Character Image Generation Prompt

Character planning JSON을 기준으로 PixelLab 캐릭터 이미지를 생성하고,
Export 결과와 이미지 평가 결과를 정리하는 프롬프트입니다.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

아래 참조 가이드를 기준으로 PixelLab에서 캐릭터 이미지를 생성하고,
Export 폴더와 평가 결과를 정리해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterGenerateImage.md
- Assets/character_concepts/game_prompt_guide/character/EvaluationImageGuide.md
- Assets/character_concepts/game_prompt_guide/prompt/PromptAuthoringGuide.md

Input:
- characterFolderPath: {캐릭터_기획_JSON_폴더_절대경로}
- PixelLabExportRoot: {PixelLab_export_저장_루트_절대경로}

작업:
1. characterFolderPath 안의 캐릭터 기획 JSON을 읽는다.
2. common JSON이 있으면 함께 읽고, 캐릭터별 JSON의 commonDataRef를 기준으로 공유 설정을 반영한다.
3. 캐릭터 identity, grade, appearance, equipment, combatDirection을 정리한다.
4. CharacterGenerateImage.md의 Character Prompt Required Elements 규칙에 맞춰 PixelLab 입력용 영어 프롬프트를 작성한다.
5. Chrome에서 https://www.pixellab.ai/create-character 를 열고 PixelLab만 사용해 이미지를 생성한다.
6. PixelLab 설정은 CharacterGenerateImage.md의 최신 설정을 그대로 따른다.
   - Generation Mode: Pro
   - Camera View: High Top-Down
   - Detail: Highly detailed
   - Outline: Black outline
7. 생성 완료 후 캐릭터 상세 페이지에서 image size, 8 directions 여부, camera view를 확인한다.
8. PixelLab 상세 페이지에서 Add tag를 사용해 캐릭터 이름과 grade를 태그로 추가한다.
9. PixelLab 상세 페이지의 Export를 사용해 이미지 파일을 다운로드한다.
10. 다운로드한 Export 파일을 아래 폴더에 저장하고 압축 파일이면 해제한다.
    - {PixelLabExportRoot}/{CharacterName}_{Grade}
11. Export 폴더 안의 rotations 이미지를 기준으로 이미지 평가를 수행한다.
12. 평가 결과를 아래 파일로 저장한다.
    - {PixelLabExportRoot}/{CharacterName}_{Grade}/evaluation_result.txt
13. 임시 다운로드 파일이나 중간 작업 폴더가 있으면 정리하고, 최종 Export 폴더만 남긴다.

Output:
- Character
- Source JSON
- PixelLab Settings
- PixelLab Prompt
- PixelLab Page
- Export Folder
- Export Contents
- Evaluation File
- Evaluation Result
- Pass / Fail
- Failure Reason
- Cleanup

실패 시 Output:
- status: failed
- failureType:
  - missing_character_folder
  - missing_pixellab_export_root
  - invalid_character_json
  - pixellab_unavailable
  - export_not_found
  - evaluation_failed
- 실패 원인
- 생성된 중간 산출물 경로
- 재시도에 필요한 입력

검증:
- characterFolderPath가 존재해야 한다.
- PixelLabExportRoot가 존재하지 않으면 작업을 중단하고 입력 보강을 요청한다.
- Export Folder는 `{CharacterName}_{Grade}` 규칙을 따라야 한다.
- rotations 이미지가 존재해야 한다.
- Rotation Validation은 90 / 100 이상이어야 한다.
- Prompt Accuracy는 80 / 100 이상이어야 한다.
- Reference Style Compatibility는 70 / 100 이상이어야 한다.

주의:
- Player/Npc/Boss는 characterType으로만 사용한다.
- 런타임 도메인은 character 기준으로 해석한다.
- PixelLab Prompt는 PixelLab 입력창에 그대로 붙여넣을 수 있게 한 문단 영어로 작성한다.
- 생성 이미지는 반드시 PixelLab 결과만 사용한다.
- PixelLab을 열 수 없거나 사용할 수 없으면 다른 이미지 생성 도구로 대체하지 말고 blocker로 보고한다.
```
