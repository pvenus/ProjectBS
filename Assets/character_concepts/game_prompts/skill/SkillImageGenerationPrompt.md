# Skill Image Animation Generation Prompt

PixelLab에서 캐릭터와 독립된 스킬 이펙트 기준 이미지와 애니메이션을 생성하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 가이드를 기준으로 캐릭터와 독립된 스킬 이펙트 애니메이션 하나를 PixelLab에서 생성해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillImageGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillImageEvaluationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillImageDownloadGuide.md

Input:
- projectRoot: {project_root}
- skillSourcePath: {스킬 정보가 포함된 JSON 또는 문서 절대경로}
- skillIdOrName: {생성할 스킬 ID 또는 이름}
- effectType: {auto | projectile_loop | airborne_burst | ground_impact | area_field | beam | aura | burst}
- loopMode: {auto | loop | one_shot}
- canvasSize: {auto | 64x64 | 128x128 | 256x256}
- frameCount: {auto | 4 | 6 | 8 | 10 | 12 | 14 | 16}

작업:
1. skillSourcePath를 읽고 skillIdOrName에 해당하는 스킬의 이름, intent, 피해 속성, 상태 효과, 범위, 타격 수, 지속 시간을 추출한다.
2. `.basic_attack.` 스킬이면서 cast.range가 1.0 이하인 근전 기본 공격이거나 캐릭터 전신 동작이 핵심인 스킬이면 중단하고 부적합 사유를 보고한다. 원거리 기본 공격의 독립 투사체 이펙트는 생성할 수 있다.
3. effectType이 auto이면 스킬 의도에 맞춰 가장 적합한 이펙트 유형을 선택한다.
4. loopMode, canvasSize, frameCount가 auto이면 가이드의 Recommended Presets를 기준으로 정한다.
5. 캐릭터, 손, 신체, 얼굴, 배경, 지형, UI, 텍스트가 없는 독립 이펙트로 시각 콘셉트를 정의한다.
6. spawn/idle → anticipation → primary motion → impact/release → fade/recovery 순서로 3~5단계 동작을 설계한다.
7. 게임 런타임이 처리할 직선 이동, 포물선 이동, 추적, 충돌 위치 이동은 PixelLab 애니메이션에서 제외한다.
8. PixelLab Creator(https://www.pixellab.ai/create)를 연다.
9. Create image (Pro)를 선택한다.
10. 가이드의 Prompt Construction Rules에 따라 영어 Reference Image Description을 작성한다.
11. Output size를 결정한 canvasSize로 설정한다.
12. Remove background를 활성화한다.
13. 기준 이미지 4개 변형을 생성한다.
14. 네 변형을 비교하여 실루엣, 안전 여백, 작은 크기 가독성이 가장 좋은 하나를 선택한다.
15. Pick a Frame에서 해당 변형을 선택하고 single image로 사용한다.
16. Animate with text (New)를 선택한다.
17. 가이드에 따라 영어 Animation Action을 작성한다.
18. frameCount를 설정하고 Remove background를 활성화한다.
19. 애니메이션을 생성한다.
20. 재생 미리보기와 모든 스프라이트 시트 프레임을 확인한다.
21. SkillImageEvaluationGuide.md로 평가한다.
22. 치명적 실패가 있거나 점수가 85점 미만이면 문제를 수정한 프롬프트로 최대 1회 재생성한다.
23. 최종 PixelLab 결과 페이지를 열린 상태로 유지한다.
24. 생성 완료 후 저장·Unity 복사·슬라이스·평가 파일 보존은 SkillImageDownloadPrompt.md를 사용해야 한다고 보고한다.

Reference Image Description 필수 조건:
- single character-independent 2D pixel art skill effect
- isolated game VFX sprite only
- no character, no body part, no hand, no face
- no text, no UI frame, no scenery
- entire effect centered and fully visible
- reference effect maximum 55 percent of canvas
- generous transparent padding on every side
- no pixel or glow touching canvas edges
- transparent background

Animation Action 필수 조건:
- fixed center
- no movement across the canvas
- maximum animated effect diameter 70 percent of canvas
- all pixels and glow fully contained
- no edge contact
- no cropping
- no character
- transparent background
- loop 또는 one-shot 명시

Output:
- Skill
- Source
- Extracted Intent
- Effect Type
- Visual Concept
- Motion Stages
- Runtime-Controlled Motion
- PixelLab Tool
- Reference Image Description
- Canvas Size
- Selected Variation
- Selection Reason
- Animation Action
- Frame Count
- Loop Mode
- Remove Background
- PixelLab Page
- Evaluation Score
- Pass / Conditional Pass / Fail
- Failure Reasons
- Regeneration Performed
- Notes

실패 시 Output:
- status: failed
- failureType:
  - missing_skill_source
  - skill_not_found
  - unsuitable_character_animation
  - pixellab_unavailable
  - image_generation_failed
  - animation_generation_failed
  - fatal_visual_failure
  - evaluation_failed
- 실패 원인
- 마지막으로 사용한 프롬프트
- 다음에 필요한 작업

주의:
- 생성은 반드시 PixelLab에서만 수행한다.
- 캐릭터 애니메이션을 생성하지 않는다.
- 근전 기본 공격(cast.range <= 1.0)은 별도 스킬 이펙트 애니메이션을 생성하지 않고 캐릭터 기본 공격 동작만 사용한다.
- 배경이 있거나 프레임 밖으로 잘린 결과는 채택하지 않는다.
- 외부 이동은 게임 런타임이 처리하므로 이펙트는 캔버스 중심에서 국소적으로만 움직인다.
- 생성 버튼에 표시된 generation 비용은 실행 전에 확인한다.
- 이 프롬프트에서는 다운로드 파일을 임의 이름으로 정리하지 않는다. 후속 저장 작업은 SkillImageDownloadGuide.md의 전체 skillId 파일명 규칙을 따른다.
```
