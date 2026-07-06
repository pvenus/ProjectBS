# Character Animation Generation Prompt

Use this prompt when generating PixelLab animations for a character from a character planning folder.

```text
입력으로 받은 캐릭터 폴더 경로와 PixelLabExportRoot를 기준으로 이미지 생성 완료/평가 정보를 찾아 기존 PixelLab 캐릭터를 검색하고, CharacterCreateGuide.md 및 CharacterGenerateAnimation.md 가이드에 따라 PixelLab에서 캐릭터 애니메이션 생성만 진행해줘.

입력:
- characterFolderPath = {캐릭터_폴더_절대경로}
- PixelLabExportRoot = {PixelLab_export_저장_루트_절대경로}

작업 조건:
1. characterFolderPath 안의 캐릭터 기획 JSON을 읽는다.
2. common JSON이 있으면 함께 읽고, 캐릭터별 JSON의 commonDataRef를 기준으로 공유 설정을 반영한다.
3. 캐릭터 기획 JSON의 identity와 grade를 기준으로 이미지 생성 결과 폴더를 찾는다.
   - 기본 경로: {PixelLabExportRoot}/{CharacterName}_{Grade}
   - 폴더명이 정확히 일치하지 않으면 CharacterName, Grade, character image prompt 일부로 PixelLabExportRoot 아래를 검색한다.
4. 이미지 생성 결과 폴더에서 관련 정보를 확인한다.
   - evaluation_result.txt
   - rotations 폴더
   - 저장된 export 파일
   - 이미지 프롬프트나 캐릭터 설명이 포함된 텍스트 파일
5. evaluation_result.txt가 있으면 Pass / Fail 결과와 실패 사유를 읽는다.
6. 이미지 생성이 실패로 기록되어 있으면 애니메이션 생성을 중단하고 blocker로 보고한다.
7. 이미지 생성 완료/평가 정보와 캐릭터 기획 JSON의 identity, grade, appearance를 조합해 PixelLab 검색용 문구를 작성한다.
   - 우선순위 1: image generation result folder name
   - 우선순위 2: character name + grade
   - 우선순위 3: saved image prompt or character description 앞부분
   - 우선순위 4: character name
8. 캐릭터 기획 JSON의 appearance, combat, skills를 기준으로 Attack Action Description과 Idle Action Description을 작성한다.
9. Chrome에서 https://www.pixellab.ai/create-character 를 열고 PixelLab만 사용한다.
10. PixelLab 캐릭터 목록 검색창에서 검색용 문구로 기존 캐릭터를 찾는다.
11. 검색 결과에서 이미지 생성 완료/평가 정보와 가장 일치하는 기존 캐릭터를 연다.
12. 이미지는 다시 생성하지 않는다. 검색으로 찾은 기존 캐릭터를 사용한다.
13. Character Preview 방향은 South-East를 선택한다.
14. Move 애니메이션을 생성한다.
   - MOVEMENT / Walking / Walk 사용
   - 생성 후 애니메이션 이름을 Move로 수정
   - 생성 후 South-West 미러링 버튼으로 south-east를 south-west에 복제
15. Attack 애니메이션을 생성한다.
   - CUSTOM / Custom Animation V3 사용
   - 캐릭터 무기와 공격 방식에 맞는 Action Description 작성
   - Frame Count: 8 Frames
   - Keep first frame 체크
   - 생성 후 애니메이션 이름을 Attack으로 수정
   - 생성 후 South-West 미러링 버튼으로 south-east를 south-west에 복제
16. Idle 애니메이션을 생성한다.
   - CUSTOM / Custom Animation V3 사용
   - 캐릭터 외형과 대기 자세에 맞는 Idle Action Description 작성
   - Frame Count: 6 Frames
   - Keep first frame 체크
   - 생성 후 애니메이션 이름을 Idle로 수정
   - 생성 후 South-West 미러링 버튼으로 south-east를 south-west에 복제
17. PixelLab 상세 페이지에서 Move, Attack, Idle 이름이 정확히 존재하는지 확인한다.
18. 각 애니메이션에 south-east와 south-west 방향이 모두 존재하는지 확인한다.
19. 다운로드, Export, 압축 해제, 파일 정리, 평가 파일 저장은 수행하지 않는다.

출력 형식:
- Character:
- Source JSON:
- Image Result Folder:
- Image Evaluation Result:
- PixelLab Search Query:
- PixelLab Page:
- Move:
- Attack Prompt:
- Attack:
- Idle Prompt:
- Idle:
- South-West Mirroring:
- Direction Check:
- Pass / Fail:
- Failure Reason:
- Notes:

생성 확인 기준:
- Animation names must be exactly Move, Attack, and Idle.
- Move, Attack, and Idle must include South-East.
- Move, Attack, and Idle must include mirrored South-West.
- Attack motion must match the character weapon and combat style.
- Idle motion must match the character appearance and personality.
- Character appearance, equipment, and weapon must remain consistent with the generated image.

주의:
- 생성은 반드시 PixelLab에서만 수행한다.
- 다운로드 및 평가는 이 프롬프트의 범위가 아니다.
- PixelLabExportRoot는 기존 이미지 생성 결과를 찾기 위한 입력이며, 이 프롬프트에서는 새 파일을 저장하지 않는다.
- Player/Npc/Boss는 characterType으로만 사용한다.
- 런타임 도메인은 character 기준으로 해석한다.
- Action Description은 PixelLab 입력창에 그대로 붙여넣을 수 있게 영어 한 문단으로 작성한다.
- PixelLab을 열 수 없거나 사용할 수 없으면 다른 도구로 대체하지 말고 blocker로 보고한다.
```
