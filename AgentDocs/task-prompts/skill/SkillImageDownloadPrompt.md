# Skill Animation Preservation Prompt

완료된 ImageGen animated GIF를 current preservation contract에 따라 보존하고 평가 패키지를 만든다.

## Inputs

- skillId: {skillId}
- animationRequestId: {animationRequestId}
- handoffMessage: {self_contained_generation_result_message}
- generatedMediaAttachments: {actual_gif_and_or_ordered_frames}

## Rules

1. 핸드오프 메시지만으로 `skillId`, 프레임 순서, 타이밍, 루프와 effect origin을 확인한다.
2. 첨부된 실제 GIF/프레임만 사용한다. record/package/manifest/receipt 경로를 요구하지 않는다.
3. 별도 import 승인이 없으면 `Assets`에 쓰지 않는다.
4. import 승인 시 대상은 `Assets/ImagesGenerated/Skill/animation/{skillId}/frame-{number}.png`다.
5. 단일 `{skillId}.animation.png`, `animation_reference`, `.meta` 파일은 만들거나 수정하지 않는다.

결과 이미지 자체와 프레임 수·순서의 짧은 확인 또는 채팅 blocker를 반환한다.
