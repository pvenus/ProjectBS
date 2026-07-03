# Character Animation Download And Evaluation Prompt

Use this prompt when downloading PixelLab animation images, preserving the original export, evaluating the animation images, and copying converted PNGs into Unity resources.

```text
입력으로 받은 캐릭터 정보와 PixelLabExportRoot를 기준으로 CharacterAnimationDownloadGuide.md 및 EvaluationAnimationGuide.md 가이드에 따라 PixelLab 애니메이션 이미지를 다운로드하고, 원본 보존, 이미지 평가, 변환 복사까지 진행해줘.

입력:
- characterName = {character_id_name}
- grade = {character_grade}
- imagePage = {애니메이션이_생성된_PixelLab_캐릭터_상세_URL}
- PixelLabExportRoot = {PixelLab_export_저장_루트_절대경로}

작업 조건:
1. Chrome에서 imagePage를 열고 기존 PixelLab 캐릭터 상세 페이지를 확인한다.
2. PixelLab 상세 페이지에서 Walk, Attack, Idle 애니메이션이 존재하는지 확인한다.
3. 각 애니메이션에 south-east와 south-west 방향이 존재하는지 확인한다.
4. PixelLab 상세 페이지의 Export를 사용해 animation 이미지 archive를 다운로드한다.
5. 다운로드 archive를 아래 폴더에 보존한다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/original
6. archive를 아래 폴더에 압축 해제한다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/original/extracted
7. 압축 해제 후 animations 폴더 구조를 확인한다.
   - animations/idle
   - animations/move
   - animations/attack
8. 각 animation type의 방향 폴더를 확인한다.
   - south-east
   - south-west
   - north-east
   - north-west
9. EvaluationAnimationGuide.md 기준으로 원본 추출 이미지들을 평가한다.
   - 평가 대상: {PixelLabExportRoot}/{CharacterName}_{Grade}/original/extracted/animations
   - frame-to-frame movement score
   - weapon review score
   - walk animation score
   - attack animation score
   - pass / fail
   - failure reason, if failed
   - missing direction notes, if any
10. 평가 결과를 아래 파일로 저장한다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/evaluation_animation_result.txt
11. 평가 Pass / Fail과 관계없이 변환 작업은 계속 진행한다.
12. 원본 추출 파일은 이름 변경하거나 이동하지 않는다.
13. CharacterAnimationDownloadGuide.md의 Animation Enum Mapping에 따라 direction folder를 CharacterAnimationClipType enum 이름으로 매핑한다.
14. north-east 또는 north-west 방향이 누락된 경우 Missing Direction Rule에 따라 south-facing 원본에서 복사해 converted에 생성한다.
15. 원본 PNG를 아래 폴더에 복사하면서 ProjectBS 파일명으로 변경한다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/converted
   - character.{characterName}.{grade}.{animation_enum}.{original_frame_name}.png
16. converted 폴더의 PNG를 Unity 리소스 폴더로 복사한다.
   - Assets/Resources/character/animation_png
17. 임시 다운로드 캐시나 중간 작업 폴더만 정리한다.
18. 아래 파일과 폴더는 삭제하지 않는다.
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/original
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/converted
   - {PixelLabExportRoot}/{CharacterName}_{Grade}/evaluation_animation_result.txt

출력 형식:
- Character:
- PixelLab Page:
- Export Folder:
- Original Archive:
- Extracted Structure:
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
- 평가는 원본 추출 이미지 기준으로 수행한다.
- 평가 실패 여부와 관계없이 converted 생성과 Unity 리소스 복사는 계속 진행한다.
- 원본 추출 파일은 평가 근거이므로 삭제하거나 이름 변경하지 않는다.
- 이름 변경은 converted 폴더에 복사본을 만들면서 수행한다.
- CharacterAnimationClipType enum 이름은 가이드의 mapping 표와 정확히 일치해야 한다.
- 최종 Unity 리소스 파일명은 character.{characterName}.{grade}.{animation_enum}.{original_frame_name}.png 형식을 따라야 한다.
```
