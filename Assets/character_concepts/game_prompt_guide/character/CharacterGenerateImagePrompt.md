# Character Image Generation Prompt

Use this prompt when generating a character image in PixelLab from a character planning folder.

```text
입력으로 받은 캐릭터 폴더 경로와 PixelLabExportRoot를 기준으로 CharacterCreateGuide.md 및 CharacterGenerateImage.md 가이드에 따라 PixelLab에서 캐릭터 이미지를 직접 생성하고, Export 결과와 평가 결과를 정리해줘.

입력:
- characterFolderPath = {캐릭터_폴더_절대경로}
- PixelLabExportRoot = {PixelLab_export_저장_루트_절대경로}

작업 조건:
1. characterFolderPath 안의 캐릭터 기획 JSON을 읽는다.
2. common JSON이 있으면 함께 읽고, 캐릭터별 JSON의 commonDataRef를 기준으로 공유 설정을 반영한다.
3. CharacterGenerateImage.md의 Character Prompt Required Elements 규칙에 맞춰 PixelLab 입력용 영어 프롬프트를 작성한다.
4. Chrome에서 https://www.pixellab.ai/create-character 를 열고 PixelLab만 사용해 이미지를 생성한다.
5. PixelLab 설정은 CharacterGenerateImage.md의 최신 설정을 그대로 따른다.
   - Generation Mode: Pro
   - Camera View: High Top-Down
   - Detail: Highly detailed
   - Outline: Black outline
6. 생성 완료 후 캐릭터 상세 페이지에서 메타 정보를 확인한다.
   - image size
   - 8 directions 여부
   - camera view
7. PixelLab 상세 페이지에서 Add tag를 사용해 캐릭터 이름과 grade를 태그로 추가한다.
8. PixelLab 상세 페이지의 Export를 사용해 이미지 파일을 다운로드한다.
9. 다운로드한 Export 파일을 아래 폴더에 저장하고 압축 파일이면 해제한다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}
10. Export 폴더 안의 rotations 이미지를 기준으로 이미지 평가를 수행한다.
11. 평가 결과를 아래 파일로 저장한다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/evaluation_result.txt
12. 임시 다운로드 파일이나 중간 작업 폴더가 있으면 정리하고, 최종 Export 폴더만 남긴다.
13. PixelLabExportRoot가 없으면 작업을 중단하고 사용자에게 입력을 요청한다.

출력 형식:
- Character:
- Source JSON:
- PixelLab Settings:
- PixelLab Prompt:
- PixelLab Page:
- Export Folder:
- Export Contents:
- Evaluation File:
- Evaluation Result:
- Pass / Fail:
- Failure Reason:
- Cleanup:

평가 기준:
- Rotation Validation: 90 / 100 이상
- Prompt Accuracy: 80 / 100 이상
- Reference Style Compatibility: 70 / 100 이상

주의:
- Player/Npc/Boss는 characterType으로만 사용한다.
- 런타임 도메인은 character 기준으로 해석한다.
- PixelLab Prompt는 PixelLab 입력창에 그대로 붙여넣을 수 있게 한 문단 영어로 작성한다.
- 생성 이미지는 반드시 PixelLab 결과만 사용한다.
- PixelLab을 열 수 없거나 사용할 수 없으면 다른 이미지 생성 도구로 대체하지 말고 blocker로 보고한다.
```
