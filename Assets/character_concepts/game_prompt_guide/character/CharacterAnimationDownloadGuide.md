
# Character Animation Download Guide

## Purpose

캐릭터 애니메이션 이미지를 다운로드한 뒤, ProjectBS에서 사용하는 파일명 규칙에 맞게 변경하고 Unity 리소스 경로에 반영하는 절차를 정리한다.

이 문서는 story agent 작업 브랜치 기준으로 캐릭터 이미지 다운로드, 이름 변경, 복사, 정리, 커밋, 병합, 배포까지의 표준 작업 흐름을 따른다.

---

## Git 최신화

작업 시작 전 `/Users/pvenus/Documents/ProjectBS-story-agent` 저장소를 최신 상태로 맞춘다.

기준 흐름:

1. `main` 브랜치 최신 데이터 가져오기
2. 작업 브랜치인 `story` 브랜치로 이동
3. `main` 내용을 `story` 브랜치에 병합
4. 충돌이 있다면 해결 후 작업 시작

예시:

```bash
cd /Users/pvenus/Documents/ProjectBS-story-agent

git checkout main
git pull origin main

git checkout story
git merge main
```

---

## 입력 정보

작업에 필요한 입력은 다음과 같다.

| Input | Description |
|-------|-------------|
| characterName | 캐릭터 이름. 파일명에는 `character.{characterName}.{grade}` 형태로 사용한다. |
| grade | 캐릭터 등급. 파일명에는 `character.{characterName}.{grade}` 형태로 사용하며, 이미지 페이지 선택 또는 다운로드 기준으로도 사용한다. |
| imagePage | 캐릭터 애니메이션 이미지를 다운로드할 이미지 페이지 주소 또는 작업 페이지. |

예시:

```text
characterName = seojin
grade = 1
imagePage = <image page url>
```

---

## 다운로드

이미지 페이지에서 `Export` 버튼을 사용해 캐릭터 애니메이션 이미지를 다운로드한다.

다운로드 후 압축 파일을 해제한다.

압축 해제 후 기본적으로 다음 구조를 확인한다.

```text
animations/
  idle/
  run/
  attack/
```

각 animation type 폴더 안에는 방향별 폴더가 있어야 한다.

```text
south-east/
south-west/
north-east/
north-west/
```

---

## Animation Enum 매핑 규칙

다운로드된 방향 폴더는 ProjectBS의 `CharacterAnimationClipType` enum 이름으로 변환한다.

| Animation Type | Direction | Animation Enum |
|----------------|-----------|----------------|
| idle | south-east | IdleDownRight |
| idle | south-west | IdleDownLeft |
| idle | north-east | IdleUpRight |
| idle | north-west | IdleUpLeft |
| run | south-east | MoveDownRight |
| run | south-west | MoveDownLeft |
| run | north-east | MoveUpRight |
| run | north-west | MoveUpLeft |
| attack | south-east | AttackDownRight |
| attack | south-west | AttackDownLeft |
| attack | north-east | AttackUpRight |
| attack | north-west | AttackUpLeft |

Death 애니메이션을 별도로 다운로드하는 경우에는 동일한 방향 규칙을 사용한다.

| Animation Type | Direction | Animation Enum |
|----------------|-----------|----------------|
| death | south-east | DeathDownRight |
| death | south-west | DeathDownLeft |
| death | north-east | DeathUpRight |
| death | north-west | DeathUpLeft |

---

## 파일명 변경 규칙

각 PNG 파일은 다음 규칙으로 이름을 변경한다.

```text
character.{characterName}.{grade}.{animation_enum}.{original_frame_name}.png
```

`original_frame_name`은 기존 파일명에서 확장자를 제외한 값을 유지한다.

예시:

```text
기존 파일:
animations/idle/south-east/frame_000.png

변경 후:
character.seojin.1.IdleDownRight.frame_000.png
```

```text
기존 파일:
animations/attack/north-west/frame_005.png

변경 후:
character.seojin.1.AttackUpLeft.frame_005.png
```

중요 규칙:

- `characterName`은 캐릭터 ID에 사용하는 이름과 동일해야 한다.
- `grade`는 캐릭터 등급과 동일해야 하며 파일명에서 `characterName` 바로 뒤에 위치한다.
- `animation_enum`은 `CharacterAnimationClipType` enum 이름과 정확히 일치해야 한다.
- 마지막 프레임 이름(`frame_000`, `frame_001` 등)은 기존 파일명을 유지한다.
- 확장자는 `.png`를 유지한다.

---

## Unity 리소스 경로로 복사

이름 변경이 완료된 PNG 파일은 다음 폴더로 복사한다.

```text
/Users/pvenus/Documents/ProjectBS-story-agent/Assets/Resources/character/animation_png
```

Unity 생성기는 이 경로를 기준으로 다음 규칙의 파일을 찾는다.

```text
character.{characterName}.{grade}.{animation_enum}*
```

그리고 찾은 Sprite들을 오름차순으로 정렬한 뒤 AnimationClip을 생성한다.

생성된 AnimationClip은 다음 경로에 저장된다.

```text
/Users/pvenus/Documents/ProjectBS-story-agent/Assets/Resources/character/animation_clip
```

저장 파일명 규칙:

```text
character.{characterName}.{grade}.{animation_enum}.clip
```

---

## 작업 후 정리

복사가 완료되면 다운로드 작업 중 생성된 임시 파일을 정리한다.

정리 대상:

- 다운로드된 압축 파일
- 압축 해제 폴더
- 중간 작업용 임시 폴더

Unity 리소스에 필요한 최종 PNG만 `Assets/Resources/character/animation_png`에 남긴다.

---

## 확인 절차

Unity에서 캐릭터 생성기를 실행하기 전 다음을 확인한다.

- `animation_png` 폴더에 파일이 복사되었는가?
- 파일명이 `character.{characterName}.{grade}.{animation_enum}.frame_000.png` 형식인가?
- `animation_enum`이 `CharacterAnimationClipType` enum과 일치하는가?
- 각 애니메이션 방향별 frame이 누락되지 않았는가?
- `characterName`이 CharacterSO의 `characterId`와 일치하는가?

---

## Git Commit

작업 완료 후 표준 커밋 메시지로 커밋한다.

예시:

```bash
git status
git add .
git commit -m "Add character animation resources for seojin"
```

커밋 메시지 형식:

```text
Add character animation resources for {characterName}
```

또는 수정 작업인 경우:

```text
Update character animation resources for {characterName}
```

---

## Main 병합 및 배포

작업 브랜치에서 검증이 끝나면 `main`으로 병합 후 배포한다.

기준 흐름:

```bash
git checkout main
git pull origin main
git merge story
git push origin main
```

배포 절차가 별도 스크립트나 CI로 구성되어 있다면 프로젝트 표준 배포 절차를 따른다.

---

## Summary

전체 흐름은 다음과 같다.

```text
Git 최신화
→ 이미지 페이지에서 Export 다운로드
→ 압축 해제
→ animations/{type}/{direction} 파일 확인
→ character.{characterName}.{grade}.{animation_enum}.{frame}.png 로 이름 변경
→ Assets/Resources/character/animation_png 로 복사
→ 압축 파일 및 임시 폴더 정리
→ Git commit
→ main 병합
→ 배포
```