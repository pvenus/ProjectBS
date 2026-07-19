# Character Animation Generation Prompt

PixelLab에 이미 생성된 캐릭터를 찾아 Move, Attack, Idle 애니메이션을
생성하는 프롬프트입니다. 다운로드와 평가는 별도 프롬프트에서 수행합니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 참조 가이드를 기준으로 기존 PixelLab 캐릭터를 검색하고,
Move, Attack, Idle 애니메이션 생성만 진행해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/character/CharacterCreateGuide.md
- Assets/character_concepts/game_prompt_guide/character/CharacterGenerateAnimation.md
- Assets/character_concepts/game_prompt_guide/character/CharacterGenerateImage.md

Input:
- projectRoot: {project_root}
- characterFolderPath: {캐릭터_기획_JSON_폴더_절대경로}
- PixelLabExportRoot: {PixelLab_export_저장_루트_절대경로}

작업:
1. characterFolderPath 안의 캐릭터 기획 JSON을 읽는다.
2. common JSON이 있으면 함께 읽고, 캐릭터별 JSON의 commonDataRef를 기준으로 공유 설정을 반영한다.
3. 캐릭터 기획 JSON의 identity와 grade를 기준으로 이미지 생성 결과 폴더를 찾는다.
   - 기본 경로: {PixelLabExportRoot}/{CharacterName}_{Grade}
   - 폴더명이 정확히 일치하지 않으면 CharacterName, Grade, character image prompt 일부로 PixelLabExportRoot 아래를 검색한다.
4. 이미지 생성 결과 폴더에서 evaluation_result.txt, rotations 폴더, 저장된 export 파일, 이미지 프롬프트를 확인한다.
5. evaluation_result.txt가 실패로 기록되어 있으면 애니메이션 생성을 중단하고 blocker로 보고한다.
6. 이미지 생성 완료/평가 정보와 캐릭터 기획 JSON의 identity, grade, appearance를 조합해 PixelLab 검색용 문구를 작성한다.
7. 캐릭터 기획 JSON의 appearance, combat, skills를 기준으로 Attack Action Description과 Idle Action Description을 작성한다.
8. Chrome에서 https://www.pixellab.ai/create-character 를 열고 PixelLab만 사용한다.
9. PixelLab 캐릭터 목록 검색창에서 검색용 문구로 기존 캐릭터를 찾는다.
10. 검색 결과에서 이미지 생성 완료/평가 정보와 가장 일치하는 기존 캐릭터를 연다.
11. 이미지는 다시 생성하지 않는다.
12. Character Preview 방향은 South-East를 선택한다.
13. Move 애니메이션을 생성한다.
    - MOVEMENT / Walking / Walk 사용
    - 생성 후 애니메이션 이름을 Move로 수정
    - 생성 후 South-West 미러링 버튼으로 south-east를 south-west에 복제
14. Attack 애니메이션을 생성한다.
    - CUSTOM / Custom Animation V3 사용
    - 캐릭터 무기와 공격 방식에 맞는 Action Description 작성
    - Frame Count: 8 Frames
    - Keep first frame 체크
    - 생성 후 애니메이션 이름을 Attack으로 수정
    - 생성 후 South-West 미러링 버튼으로 south-east를 south-west에 복제
15. Idle 애니메이션을 생성한다.
    - CUSTOM / Custom Animation V3 사용
    - 캐릭터 외형과 대기 자세에 맞는 Idle Action Description 작성
    - Frame Count: 6 Frames
    - Keep first frame 체크
    - 생성 후 애니메이션 이름을 Idle로 수정
    - 생성 후 South-West 미러링 버튼으로 south-east를 south-west에 복제
16. PixelLab 상세 페이지에서 Move, Attack, Idle 이름이 정확히 존재하는지 확인한다.
17. 각 애니메이션에 South-East와 South-West 방향이 모두 존재하는지 확인한다.
18. 다운로드, Export, 압축 해제, 파일 정리, 평가 파일 저장은 수행하지 않는다.

Output:
- Character
- Source JSON
- Image Result Folder
- Image Evaluation Result
- PixelLab Search Query
- PixelLab Page
- Move
- Attack Prompt
- Attack
- Idle Prompt
- Idle
- South-West Mirroring
- Direction Check
- Pass / Fail
- Failure Reason
- Notes

실패 시 Output:
- status: failed
- failureType:
  - missing_character_folder
  - missing_image_result
  - image_evaluation_failed
  - pixellab_character_not_found
  - pixellab_unavailable
  - animation_creation_failed
  - missing_required_direction
- 실패 원인
- 확인한 PixelLab 검색어
- 다음에 필요한 작업

검증:
- 애니메이션 이름은 정확히 Move, Attack, Idle이어야 한다.
- Move, Attack, Idle은 South-East 방향을 포함해야 한다.
- Move, Attack, Idle은 미러링된 South-West 방향을 포함해야 한다.
- Attack 모션은 캐릭터 무기와 전투 스타일에 맞아야 한다.
- Idle 모션은 캐릭터 외형과 성격에 맞아야 한다.
- 캐릭터 외형, 장비, 무기는 생성된 이미지와 일관되어야 한다.

주의:
- 생성은 반드시 PixelLab에서만 수행한다.
- 다운로드 및 평가는 이 프롬프트의 범위가 아니다.
- PixelLabExportRoot는 기존 이미지 생성 결과를 찾기 위한 입력이며, 이 프롬프트에서는 새 파일을 저장하지 않는다.
- Action Description은 PixelLab 입력창에 그대로 붙여넣을 수 있게 영어 한 문단으로 작성한다.
- PixelLab을 열 수 없거나 사용할 수 없으면 다른 도구로 대체하지 말고 blocker로 보고한다.
```
