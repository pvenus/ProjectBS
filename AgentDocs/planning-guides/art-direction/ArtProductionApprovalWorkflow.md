# ProjectBS Art Production and Approval Workflow

Status: current art gate  
Owner: 화감

## 필수 흐름

`기획 근거 → 생성 전 아트 명세 → 기준 결박 → 생성/제작 → 원본 검사 → 실제 크기 비교 → 합성 검수 → 화감 판정 → 프로젝트 반영`

중간 후보·실패본·접촉 시트는 `Assets/ImagesGenerated`에 넣지 않는다.

## Gate 0 — 입력 완전성

필수 입력:

- 콘텐츠 ID와 자산 역할
- 벼리가 승인한 의미·인물·사건 사실
- 사용 화면, 레이어, 배경, 상태
- 출력 경로와 파일명
- 목표 캔버스와 실제 표시 크기
- 레지스트리 기준 ID 1개 이상, 반례 ID 1개 이상
- must show 최대 3개, must not show

하나라도 없으면 `명세 보완`이며 생성하지 않는다.

## Gate 1 — 생성 전 아트 명세

```yaml
artBriefVersion: projectbs_art_brief_v1
assetId:
contentId:
domain:
usage:
screenAndLayer:
canvas:
actualDisplaySizes: []
alphaPolicy:
objectOccupancyTarget:
safeArea:
visualHierarchy:
paletteRoles:
lineAndTexture:
mustShow: []
mustNotShow: []
referenceIds: []
antiReferenceIds: []
outputPath:
owner: 화감
planningAuthorityRef:
technicalValidationOwner: 한결
```

고정 아트 규칙은 `ProjectArtDirection.md` 링크로 결박하며 개별 프롬프트에 임의 재작성하지 않는다.

## Gate 2 — 결과 원본 검사

- 경로·이름·포맷·캔버스·모드·알파
- 잘림, 가짜 문자, 워터마크, 기형, 배경 잔여물
- bbox 점유율, 중심축, 안전 여백
- 기획 정체성과 조선 시대성

치명 오류는 점수와 관계없이 반려한다.

## Gate 3 — 비교와 실사용 검수

반드시 같은 판에 배치한다.

1. 승인 기준 이미지
2. 평가 대상
3. 같은 화면에 노출되는 형제 자산 2개 이상
4. 실제 표시 크기 미리보기
5. 실제 UI/전투 배경 합성 또는 런타임 캡처

아이콘은 Canvas 200/80 배치와 32px 파생본 스트레스 시험, 캐릭터와 팝업은 해당 Canvas Rect의 실제 픽셀 결과, 배경은 실제 게임 화면 합성을 기본 시험으로 한다. Prefab 단위를 물리 픽셀로 간주하지 않는다.

P0 런타임 증거는 G3 이후 1920×1080, 960×600, 2560×1440에서 수집한다. 각 캡처에는 `RectTransform.GetWorldCorners`, `Canvas.scaleFactor`, 실제 해상도와 사용한 플랫폼 파생본을 연결한다. 증거는 `Artifacts/GraphicsValidation/P0/<build>/<resolution>/`의 PNG, `manifest.json`, `importer.csv`, `metrics.json`, `summary.md`로 구성하며 `Assets/ImagesGenerated`에 넣지 않는다.

P0 시각 재검수 범위는 episode01~03 실제 노출분으로 제한한다.

- Stage popup_main 12장
- Battle background 3장: chapter01.01, 03_1, 03_2
- Character portrait 3장: seojin.1, jihan.1, yujin.1
- 위 세 캐릭터 1등급 animation 36 PNG 프레임
- 위 세 캐릭터 1등급 스킬 아이콘 9장

한결의 자동 검사 성공은 시각 승인이 아니다. 화감은 캡처 묶음에서 식별성, 역할 차이, 밝고 어두운 배경 대비, 알파 가장자리, 시각적 jitter만 최종 판정한다.

## Gate 4 — 점수와 판정

| 항목 | 배점 |
| --- | ---: |
| 기획·문화·시대 적합성 | 25 |
| 스타일 및 형제 자산 일관성 | 25 |
| 실제 크기 식별성과 UI 기능 | 20 |
| 구도·점유율·여백 | 15 |
| 파일·알파·프레임 기술 완전성 | 15 |

- **승인:** 85점 이상, 모든 치명 게이트 통과, 항목별 60% 이상
- **조건부 승인:** 80~84점 또는 비파괴적 수정 1~3개로 85점 도달 가능. 수정 완료 전 프로젝트 반영 금지
- **재작업:** 60~79점, 방향은 유효하나 구도·스타일·실사용 구조 수정 필요
- **반려:** 60점 미만 또는 문화/시대 오류, 다른 스타일, 빈 이미지, 잘못된 정체성, 필수 규격 실패

승인 기록에는 판정, 근거, 게임 화면 영향, 수정 지시, 다음 행동, 검사한 표시 환경, 남은 위험을 쓴다.

## 기준 변경 게이트

새 결과가 더 좋아 보여도 자동으로 기준이 되지 않는다. 기준 변경은 다음을 모두 요구한다.

- 화감의 변경 승인과 레지스트리 버전 증가
- 변경 이유와 기존 기준의 한계
- 영향 도메인 및 형제 자산 비교
- 한결의 실제 화면·임포트 검증
- 이음의 재검수/교체 범위 반영
- 게임 정체성 변경이면 벼리·가온과 재협의

## 신규 콘텐츠 제작 재개 게이트

- 이 폴더의 권위 문서가 요청에 직접 참조됨
- 해당 도메인에 승인 기준 1개와 반례 1개 이상이 있음
- 실제 표시 크기와 Prefab 사용처가 확인됨
- art brief가 완전함
- 후보 저장 위치와 프로젝트 승격 경계가 분리됨
- Canvas 배치별 실제 픽셀 미리보기 및 합성 검수 방법이 준비됨
- 한결이 기술 검증 가능한 상태이며 이음이 우선순위를 확정함
