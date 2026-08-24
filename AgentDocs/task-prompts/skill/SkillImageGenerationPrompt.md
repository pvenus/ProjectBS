# Skill Animation ImageGen Prompt

`ImageGenAnimationPromptAuthoringPrompt.md`와 `ImageGenAnimationGenerationPrompt.md`를 Skill 도메인으로 실행한다.

## Inputs

- skillId: {skillId}
- animationRequestId: {animationRequestId}
- approvedMotion: {motion_and_key_poses}
- frameCount: {frameCount}
- timingAndLoop: {timing_and_loop}
- canvasAndOrigin: {canvas_size_and_effect_origin}
- planningHandoffMessage: {self_contained_skill_planning_message}
- referenceMediaAttachments: {optional_actual_images}

## Rules

1. `artifactType=animation`, `domainType=skill`, `contentId=skillId`로 고정한다.
2. provider는 ImageGen이며 registry의 `skill_animation@2.0.0` profile을 사용한다.
3. anchor는 `effect_origin`이고 character reference/profile 필드는 넣지 않는다.
4. provider-native animated GIF 생성까지만 수행한다.
5. PixelLab, 단일 스프라이트 시트 또는 독립 프레임 생성으로 대체하지 않는다.
6. planning/routing/prompt/generation record 파일을 입력으로 요구하거나 만들지 않는다.
7. 결과 GIF와 프레임 이미지를 채팅에 직접 첨부하거나 렌더링한다. 다음 채팅에 파일 경로나 package를 핸드오프하지 않는다.
8. Unity `.meta` 파일을 생성·수정·복사·삭제하지 않는다.

생성 이미지 자체, skillId, 프레임 순서·타이밍·루프의 짧은 요약 또는 채팅 blocker를 반환한다.
