# Skill Image Animation Download and Evaluation Prompt

PixelLab에서 생성한 스킬 기준 이미지와 애니메이션을 구분해서 저장하고, Unity 복사·슬라이스 검증·평가·압축 정리까지 수행하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

아래 가이드를 먼저 읽고 PixelLab 스킬 이펙트 결과의 다운로드, 보존, Unity 복사, 슬라이스 검증, 평가, 임시 파일 정리를 진행해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillImageGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillImageDownloadGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillImageEvaluationGuide.md

Input:
- skillSourcePath: {스킬 JSON 절대경로}
- skillId: {전체 equipment skill id}
- skillSlug: {평가 폴더용 짧은 이름}
- pixelLabPage: {열린 PixelLab 결과 페이지 또는 URL}
- evaluationRoot: /Users/pvenus/Documents/PixelLab/skill
- expectedCellSize: {auto | 64x64 | 128x128 | 256x256}
- expectedFrameCount: {auto | 숫자}
- expectedLoopMode: {loop | one_shot}

작업:
1. skillSourcePath와 PixelLab 페이지가 같은 스킬인지 확인한다.
2. 기준 이미지와 최종 애니메이션을 별도 결과물로 다운로드한다.
3. 압축 파일이면 임시 폴더에 해제하고 기준 PNG와 애니메이션 PNG를 구분한다.
4. PNG 디코딩, 알파 채널, 투명 모서리, 가장자리 접촉, 잘림을 검사한다.
5. 애니메이션 시트 크기와 셀 크기로 columns, rows, usable frame count를 계산한다.
6. 프레임 수나 순서를 확정할 수 없으면 Unity 복사를 중단하고 실패 처리한다.
7. {evaluationRoot}/{skillSlug}/reference, animation, evaluation 폴더를 구성한다.
8. 기준 이미지는 reference/{skillId}.animation_ref.png로 복사한다.
9. 애니메이션은 animation/{skillId}.animation.png로 복사한다.
10. 프롬프트, PixelLab 페이지, 선택 variation, 크기, 격자, 프레임 수, loop mode, SHA-256을 generation_record.txt에 저장한다.
11. 동일 파일을 다음 Unity 경로에 복사하고 SHA-256 일치를 확인한다.
    - Assets/Resources/skill/animation_ref_png/{skillId}.animation_ref.png
    - Assets/Resources/skill/animation_png/{skillId}.animation.png
12. Unity PNG 복사 직후 같은 경로의 `.png.meta`를 반드시 생성 또는 갱신한다. 애니메이션 meta를 Sprite Multiple, Point, no mipmap, alpha transparency, no compression으로 설정하고 실제 columns × rows와 cell size에 맞춰 슬라이스한다. PNG 복사만으로 Unity 반영 완료 처리하지 않는다.
13. 프레임 이름을 {skillId}.animation.frame_00부터 숫자 순서로 작성한다.
14. 기준 이미지가 4 variation sheet일 때만 2×2로 슬라이스하고 variation_00 형식으로 이름을 작성한다.
15. 올바른 Unity Editor 버전에서 PNG를 reimport하여 실제 Sprite sub-asset 수와 이름을 확인한 뒤 SkillBaseVisualAssetBuilder를 실행한다. loop.anim 생성, 프레임 수, 12 FPS, ProjectileLoop 연결을 확인한다. 올바른 Editor를 실행할 수 없으면 `slice configured / Unity reimport pending`으로 보고하고 빌더 완료로 간주하지 않는다.
16. SkillImageEvaluationGuide.md로 보존된 기준 이미지, 시트, 개별 프레임, 재생 순서를 평가한다.
17. 평가 결과를 {evaluationRoot}/{skillSlug}/evaluation/evaluation_result.txt에 저장한다.
18. 보존 및 검증이 끝난 뒤 ZIP, 임시 압축 해제 폴더, 중복 다운로드만 삭제한다.
19. 최종 체크리스트와 누락/실패 항목을 보고한다.

주의:
- 기준 이미지와 애니메이션을 같은 파일명이나 같은 Unity 폴더에 저장하지 않는다.
- 실제 시트가 확인되지 않은 상태에서 3×3 또는 2×2를 가정하지 않는다.
- 평가용 파일과 Unity 파일은 바이트가 동일해야 한다.
- 모든 Unity 대상 PNG는 복사 직후 실제 시트 구조에 맞는 `.png.meta` 슬라이스 처리를 항상 수행한다.
- `.meta`의 sprite rect 수와 이름 수는 usable frame/variation 수와 정확히 일치해야 한다.
- 개별 프레임을 확인할 수 없으면 Pass 처리하지 않는다.
- 현재 자동 빌더는 ProjectileLoop만 생성하므로 one_shot/Hit 자동 연결 완료로 보고하지 않는다.
- ZIP 삭제는 보존 복사, Unity 복사, 체크섬, 평가 결과 저장이 모두 끝난 후에만 수행한다.

Output:
- Skill ID
- Source JSON
- PixelLab Page
- Reference Source / Preserved / Unity Path
- Animation Source / Preserved / Unity Path
- Sheet Size / Cell Size / Columns / Rows
- Requested / Observed / Usable Frames
- Reference / Animation SHA-256
- Unity Import and Slice Status
- Clip Generation and Binding Status
- Evaluation Result / Result Path
- ZIP and Temporary Cleanup Status
- Missing Items
- Pass / Fail
```
