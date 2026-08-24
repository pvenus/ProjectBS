# Skill Animation ImageGen Guide

## Purpose

스킬 애니메이션은 PixelLab 스프라이트 시트가 아니라 현재 ImageGen 애니메이션 파이프라인으로 생성한다. 생성·보존 계약은 `GeneratedMediaImageGenOnlyContractGuide.md`, `ImageGenAnimationPipelineGuide.md`, `GeneratedMediaPreservationPackagingGuide.md`를 따른다.

메인 캐릭터 샘플에서 확정한 컨셉, 관찰 결과와 별도 채팅용 핸드오프 템플릿은 `SkillAnimationImageGenSampleConceptGuide.md`를 따른다.

## Identity and generation

- `artifactType=animation`, `domainType=skill`, `contentId={skillId}`
- anchor는 `effect_origin`이며 캐릭터 reference/profile 필드는 사용하지 않는다.
- 승인된 동작, 프레임 수, 순서, 타이밍, 루프, 캔버스 크기를 입력한다.
- 스킬 VFX의 기본 정식 애니메이션은 시작·생성·타격·소멸 pose를 포함하지 않는 짧은 반복 loop다. 기본값은 동일한 effect origin과 footprint를 유지하는 4프레임이며 회전 위상, 이동 광점, 내부 광량과 작은 파티클만 순환시킨다.
- 생성/등장과 종료/소멸 연출은 loop 프레임에 굽지 않고 런타임 또는 별도 clip/effect가 소유한다. loop의 모든 frame은 언제든 반복 가능한 지속 상태여야 하며 마지막 frame은 첫 frame으로 자연스럽게 연결돼야 한다.
- loop 내부에서 형태 오차가 흔들림으로 읽히는 hero object를 프레임마다 다시 생성하지 않는다. 고정 field·ring·effect origin을 identity로 사용하고, 같은 완성 오브젝트의 재묘사보다 먹선 회전·광량 맥동·파티클 궤도 변화를 우선한다.
- 단일 투사체의 반복 판독이 gameplay상 필수인 경우만 예외로 하며, 이때 투사체 identity·scale·baseline은 고정하고 실제 이동·복수 배치·발사 횟수는 런타임이 소유한다.
- ImageGen provider-native animated GIF가 정식 원본이다. 정지 이미지, 컨택트 시트, 단일 스프라이트 시트, 독립 생성 프레임은 정식 원본이 아니다.
- 스킬 기획은 필요한 내용을 완결적인 채팅 메시지로 넘긴다. 다음 채팅은 기획 파일, routing record, prompt record 또는 generation record를 열지 않아도 그 메시지만으로 생성할 수 있어야 한다.
- 생성 결과는 GIF/프레임 이미지 자체를 채팅에 첨부하거나 렌더링해 전달한다. 채팅 사이에 record 경로, package 경로, manifest 또는 sidecar 파일을 전달하지 않는다.
- 프로젝트 저장은 별도로 명시적으로 요청된 import 단계에서만 수행한다.

## Canonical project input

```text
Assets/ImagesGenerated/Skill/animation/{skillId}/
  frame-0.png
  frame-1.png
  ...
```

`frame-{number}.png` 또는 `frame_{number}.png` Sprite만 허용한다. 번호 순으로 정렬하며 파일은 `{skillId}` 폴더 바로 아래에 둔다. 하위 폴더는 읽지 않는다. `animation_reference`와 `{skillId}.animation.png` 단일 시트는 레거시 입력이다.

## Generated clip

```text
Assets/AnimationClips/Skill/{visualId}.loop.anim
```

클립은 Skill SO 폴더가 아니라 Skill 콘텐츠 단위 클립 폴더에 생성한다. 재생성 시 기존 asset을 갱신해 GUID를 유지한다. 프레임 폴더 누락, 잘못된 이름, Sprite import 실패 또는 0프레임은 클립 생성 실패다.

Unity `.meta` 파일은 생성·수정·복사·삭제·정규화하지 않는다. 프레임 및 클립의 기존 `.meta`는 Unity 소유 정보로 그대로 둔다.
