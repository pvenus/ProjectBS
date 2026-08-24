# Skill Animation Evaluation Prompt

## Inputs

- skillId: {skillId}
- handoffMessage: {self_contained_generation_result_message}
- animatedGifAttachment: {animatedGifAttachment}
- orderedFrameAttachments: {orderedFrameAttachments}
- approvedMotionContract: {approvedMotionContract}

채팅에 첨부된 ImageGen animated GIF와 ordered frame set을 `SkillImageEvaluationGuide.md`에 따라 평가한다. record/package/path 파일을 요구하지 않는다. 스킬 의미, 프레임 순서·연속성, 루프, 캔버스, 배경/투명도, effect origin 안정성과 픽셀 가독성을 확인한다.

평가만 수행하며 생성, 이미지 수정 또는 프로젝트 복사를 하지 않는다. 승격 대상은 `Assets/ImagesGenerated/Skill/animation/{skillId}/frame-{number}.png`이다.

Result(Pass/Conditional Pass/Fail), 점수, 발견 사항과 수정 요구 또는 blocker를 채팅 메시지로 반환한다. `.meta` 파일은 열거나 변경하지 않는다.
