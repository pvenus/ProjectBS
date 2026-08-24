# Skill Animation Preservation and Promotion Guide

## Purpose

채팅으로 전달된 ImageGen 스킬 애니메이션 이미지를, 별도 승격 요청이 있을 때 Unity 정식 프레임 폴더로 가져오는 Skill 어댑터다.

## Preservation

이전 채팅은 결과 GIF/프레임 이미지 자체와 skillId, 프레임 순서, 타이밍, 루프, effect origin을 한 메시지로 전달한다. generation record path, evaluation package path, manifest, receipt 또는 sidecar 파일은 입력으로 요구하거나 전달하지 않는다. 전달된 GIF에서 프레임 추출이 필요하면 해당 이미지 자체만 사용한다.

## Promotion target

Pass이고 패키지 무결성이 확인된 프레임셋 전체만 원자적으로 복사한다.

```text
Assets/ImagesGenerated/Skill/animation/{skillId}/frame-{number}.png
```

`frame-0.png`부터 연속 번호를 권장한다. `frame_{number}.png`도 호환되지만 한 세트에서 형식을 섞지 않는다. `{skillId}.animation.png` 시트와 `animation_reference`는 새 입력으로 만들지 않는다. 모든 PNG는 Unity Sprite로 import한다.

Skill JSON/SO 생성기는 직접 자식 Sprite를 숫자 순으로 읽어 다음 클립을 생성·갱신한다.

```text
Assets/AnimationClips/Skill/{visualId}.loop.anim
```

SO 출력 `Assets/Contents/Skill/so`와 클립 출력을 혼동하지 않는다.

`.meta` 파일은 import 대상에 포함하지 않는다. 기존 `.meta`를 읽어 복제하거나 직접 생성·수정·삭제하지 않으며 Unity의 자동 importer 처리를 그대로 둔다.
