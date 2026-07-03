# Character Animation Download And Evaluation Prompt

Use this prompt when downloading PixelLab animation images, preserving the character `animations` folder, evaluating the animation images, and copying converted PNGs into Unity resources.

```text
입력으로 받은 캐릭터 정보와 PixelLabExportRoot를 기준으로 CharacterAnimationDownloadGuide.md 및 EvaluationAnimationGuide.md 가이드에 따라 PixelLab 애니메이션 이미지를 확인/다운로드하고, 캐릭터 animations 폴더 보존, 이미지 평가, 변환 복사까지 진행해줘.

입력:
- PixelLabExportRoot = {PixelLab_export_저장_루트_절대경로}
- targetCharacterFolder = {PixelLabExportRoot_아래_대상_캐릭터_폴더_절대경로}

작업 조건:
1. targetCharacterFolder가 PixelLabExportRoot 아래의 대상 캐릭터 폴더인지 확인한다.
2. targetCharacterFolder의 폴더명 또는 내부 metadata/result/evaluation 파일에서 characterName과 grade를 추출한다.
   - characterName과 grade를 입력으로 직접 받지 않는다.
   - 파일명 생성에는 추출한 characterName과 grade를 사용한다.
3. targetCharacterFolder 안에서 이미지 생성에 사용한 PixelLab prompt와 결과 metadata를 찾는다.
   - CharacterGenerateImagePrompt.md / CharacterGenerateImage.md 흐름에서 기록된 PixelLab Prompt를 우선 사용한다.
   - evaluation_result.txt가 있으면 통과한 이미지 생성 결과와 연결된 prompt를 우선 사용한다.
   - PixelLab Page URL을 입력으로 직접 받지 않는다.
4. 먼저 아래 캐릭터 animations 폴더가 존재하는지 확인한다.
   - {targetCharacterFolder}/animations
5. animations 폴더가 이미 있고 이번 작업이 새 Export/교체 요청이 아니라면 기존 animations 폴더를 사용한다.
6. animations 폴더가 없거나, 이번 작업이 특정 캐릭터의 새 Export/교체 요청이면 Chrome에서 https://www.pixellab.ai/create-character 를 연다.
7. https://www.pixellab.ai/create-character 페이지에서 이미지 생성에 사용한 PixelLab prompt, characterName, grade tag를 사용해 기존 PixelLab 캐릭터를 검색한다.
8. 검색 결과에서 prompt와 태그가 일치하는 기존 PixelLab 캐릭터 상세 페이지를 연다.
9. 해당 페이지 검색 결과에서 일치하는 PixelLab 캐릭터를 찾지 못하면 즉시 실패 처리하고 lookup failure로 보고한다.
10. PixelLab 상세 페이지에서 Walk, Attack, Idle 애니메이션이 존재하는지 확인한다.
11. 각 애니메이션에 south-east와 south-west 방향이 존재하는지 확인한다.
12. PixelLab 상세 페이지의 Export를 사용해 animation 이미지 archive를 다운로드한다.
13. 다운로드 archive는 임시 작업 폴더에만 압축 해제한다.
14. 압축 해제 결과에서 animations 폴더만 찾는다.
15. 새 Export 결과가 Required Folder Structure Hard Fail 검사를 통과하면, 기존 캐릭터 animations 폴더를 새 animations 폴더로 교체한다.
   - 이동 대상: {targetCharacterFolder}/animations
   - 전체 압축 해제 폴더나 archive를 character folder로 이동하지 않는다.
   - character folder에는 animations 폴더만 source result로 남긴다.
16. 새 Export 결과가 Required Folder Structure Hard Fail 검사를 통과하지 못하면 기존 animations 폴더를 교체하지 않고 즉시 실패 처리한다.
17. 사용할 animations 폴더 구조를 확인한다.
   - animations/idle
   - animations/move
   - animations/attack
18. 각 animation type의 방향 폴더를 확인한다.
   - south-east
   - south-west
   - north-east
   - north-west
19. Required Folder Structure Hard Fail 조건을 먼저 확인한다.
   - animations/ 폴더가 없으면 즉시 실패 처리하고 중단한다.
   - idle, move, attack 중 하나라도 없으면 즉시 실패 처리하고 중단한다.
   - 각 animation type의 south-east 또는 south-west가 없으면 즉시 실패 처리하고 중단한다.
   - south-east 또는 south-west 폴더에 PNG frame이 없으면 즉시 실패 처리하고 중단한다.
   - 같은 animation type에서 south-east와 south-west frame count가 다르면 즉시 실패 처리하고 중단한다.
   - 원본 frame이 불완전하거나 Missing Direction Rule 복제에 사용할 수 없으면 즉시 실패 처리하고 중단한다.
   - north-east 또는 north-west만 누락된 경우, south-facing 원본이 완전하면 즉시 실패가 아니며 Missing Direction Rule로 처리한다.
20. Required Folder Structure Hard Fail이 발생하면 평가, 변환, Unity 리소스 복사를 진행하지 않는다.
   - 가능한 경우 실패 사유를 {targetCharacterFolder}/evaluation_animation_result.txt에 저장한다.
   - 기존 animations 폴더가 있으면 삭제하거나 교체하지 않는다.
21. EvaluationAnimationGuide.md 기준으로 source animations 이미지들을 평가한다.
   - 평가 대상: {targetCharacterFolder}/animations
   - frame-to-frame movement score
   - weapon review score
   - walk animation score
   - attack animation score
   - pass / fail
   - failure reason, if failed
   - missing direction notes, if any
22. 평가 결과를 아래 파일로 저장한다.
   - {targetCharacterFolder}/evaluation_animation_result.txt
23. 평가 Pass / Fail과 관계없이 변환 작업은 계속 진행한다.
24. 단, 19번의 Required Folder Structure Hard Fail은 평가 실패와 다르며, 발생 시 변환 작업을 진행하지 않는다.
25. source animations 파일은 이름 변경하거나 이동하지 않는다.
26. CharacterAnimationDownloadGuide.md의 Animation Enum Mapping에 따라 direction folder를 CharacterAnimationClipType enum 이름으로 매핑한다.
27. north-east 또는 north-west 방향이 누락된 경우 Missing Direction Rule에 따라 south-facing 원본에서 복사해 converted에 생성한다.
28. source PNG를 아래 폴더에 복사하면서 ProjectBS 파일명으로 변경한다.
   - {targetCharacterFolder}/converted
   - character.{characterName}.{grade}.{animation_enum}.{original_frame_name}.png
29. converted 폴더의 PNG를 Unity 리소스 폴더로 복사한다.
   - Assets/Resources/character/animation_png
30. 임시 다운로드 archive, 임시 압축 해제 폴더, 다운로드 캐시만 정리한다.
31. 아래 파일과 폴더는 삭제하지 않는다.
   - {targetCharacterFolder}/animations
   - {targetCharacterFolder}/converted
   - {targetCharacterFolder}/evaluation_animation_result.txt

출력 형식:
- Character:
- Grade:
- Target Character Folder:
- Image Generation Prompt:
- PixelLab Lookup Result:
- PixelLab Page:
- Export Folder:
- Existing Animations Folder:
- Download Performed:
- Replacement Performed:
- Animations Folder:
- Temporary Export Cleanup:
- Folder Structure Check:
- Hard Fail:
- Direction Check:
- Missing Direction Handling:
- Evaluation File:
- Evaluation Result:
- Converted Folder:
- Unity Resource Folder:
- Final File Count:
- Missing Directions:
- Pass / Fail:
- Failure Reason:
- Cleanup:
- Notes:

주의:
- 다운로드와 Export는 반드시 PixelLab에서 수행한다.
- characterName과 grade는 입력으로 직접 받지 않고 targetCharacterFolder 또는 그 안의 metadata/result/evaluation 파일에서 추출한다.
- PixelLab Page URL은 입력으로 직접 받지 않는다. https://www.pixellab.ai/create-character 페이지에서 이미지 생성에 사용한 PixelLab Prompt와 태그로 기존 PixelLab 캐릭터를 검색해서 연다.
- PixelLabExportRoot 기준 targetCharacterFolder의 animations 폴더가 있고 새 교체 요청이 아니면 추가 다운로드하지 않는다.
- 특정 캐릭터를 대상으로 새 Export를 수행한 경우, 성공한 새 animations 폴더로 기존 animations 폴더를 교체하는 개념으로 처리한다.
- Export 후 character folder에는 animations 폴더만 이동한다. archive나 전체 압축 해제 폴더는 이동하지 않는다.
- 작업 진행에 필요한 animation folder structure가 완전하지 않으면 즉시 실패 처리하고 평가/변환/Unity 복사를 진행하지 않는다.
- 평가는 {targetCharacterFolder}/animations 기준으로 수행한다.
- rotations 평가는 이 프롬프트에서 별도로 수행하지 않는다. rotations 정보는 이미지 생성 평가 단계에 이미 존재하는 것으로 간주한다.
- 평가 실패 여부와 관계없이 converted 생성과 Unity 리소스 복사는 계속 진행한다. 단, Required Folder Structure Hard Fail은 예외로 즉시 중단한다.
- source animations 파일은 평가 근거이므로 삭제하거나 이름 변경하지 않는다.
- 이름 변경은 converted 폴더에 복사본을 만들면서 수행한다.
- CharacterAnimationClipType enum 이름은 가이드의 mapping 표와 정확히 일치해야 한다.
- 최종 Unity 리소스 파일명은 character.{characterName}.{grade}.{animation_enum}.{original_frame_name}.png 형식을 따라야 한다.
```
