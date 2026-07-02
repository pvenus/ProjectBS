# Character Image Generation Prompt

Use this prompt when creating a PixelLab character image prompt from a character planning folder.

```text
입력으로 받은 캐릭터 폴더 경로와 PixelLabExportRoot를 기준으로 CharacterCreateGuide.md 및 CharacterGenerateImage.md 가이드에 따라 PixelLab 캐릭터 이미지 생성용 프롬프트와 결과 저장 계획을 작성해줘.

입력:
- characterFolderPath = {캐릭터_폴더_절대경로}
- PixelLabExportRoot = {PixelLab_export_저장_루트_절대경로}

작업 조건:
1. characterFolderPath 안의 캐릭터 기획 JSON을 읽는다.
2. common JSON이 있으면 함께 읽고, 캐릭터별 JSON의 commonDataRef를 기준으로 공유 설정을 반영한다.
3. CharacterGenerateImage.md의 Character Prompt Required Elements 규칙을 따른다.
4. PixelLab에서 바로 사용할 수 있는 영어 이미지 생성 프롬프트를 작성한다.
5. PixelLab 결과 저장 폴더를 아래 형식으로 산출한다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}
6. 해당 폴더 안에 Export 결과와 평가 결과 저장 계획을 포함한다.
   - rotations/
   - evaluation_result.txt
7. PixelLabExportRoot가 없으면 작업을 중단하고 사용자에게 입력을 요청한다.

출력 형식:
- Character:
- Source JSON:
- PixelLab Settings:
- PixelLab Prompt:
- Negative / Avoid:
- Export Folder:
- Expected Export Structure:
- Evaluation File:
- Validation Notes:

주의:
- Player/Npc/Boss는 characterType으로만 사용한다.
- 런타임 도메인은 character 기준으로 해석한다.
- PixelLab Prompt는 PixelLab 입력창에 그대로 붙여넣을 수 있게 한 문단 영어로 작성한다.
- PixelLab Settings는 CharacterGenerateImage.md의 최신 설정을 그대로 따른다.
```
