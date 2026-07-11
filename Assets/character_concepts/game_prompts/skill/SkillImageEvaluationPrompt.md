# Skill Image Animation Evaluation Prompt

생성된 스킬 이펙트 애니메이션을 가이드에 따라 평가하는 실행 프롬프트입니다.

## Prompt

```text
작업 폴더 = /Users/pvenus/ProjectBS

아래 평가 가이드를 기준으로 캐릭터 독립형 스킬 이펙트 애니메이션을 평가해줘.
생성 또는 수정은 하지 말고 평가만 수행해줘.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/skill/SkillImageGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillImageEvaluationGuide.md

Input:
- skillSourcePath: {스킬 정보가 포함된 JSON 또는 문서 절대경로}
- skillIdOrName: {평가할 스킬 ID 또는 이름}
- assetPathOrPixelLabPage: {로컬 PNG/GIF/스프라이트 시트 경로 또는 열린 PixelLab 결과 페이지}
- referenceAssetPath: {기준 이미지 경로 또는 null}
- animationAssetPath: {애니메이션 시트 경로 또는 null}
- unityReferencePath: {Unity animation_ref_png 경로 또는 null}
- unityAnimationPath: {Unity animation_png 경로 또는 null}
- expectedCanvasSize: {예상 캔버스 크기}
- expectedFrameCount: {예상 프레임 수}
- expectedLoopMode: {loop | one_shot}

평가 작업:
1. skillSourcePath에서 대상 스킬의 intent, 속성, 범위, 상태 효과를 확인한다.
2. assetPathOrPixelLabPage에서 기준 이미지, 스프라이트 시트, 개별 프레임, 재생 애니메이션을 확인한다.
3. 모든 프레임의 크기, 알파 채널, 네 모서리 투명도, 가장자리 접촉 여부를 확인한다.
4. 캐릭터, 손, 신체, 얼굴, 의도하지 않은 생물, 배경, 지형, UI, 텍스트가 포함됐는지 확인한다.
5. 이펙트 본체뿐 아니라 반투명 글로우, 연기, 파티클, 잔상까지 잘리지 않았는지 확인한다.
6. 프레임별 중심축, 픽셀 크기, 팔레트, 형태, 광원 일관성을 확인한다.
7. 동작의 anticipation, primary motion, impact/release, ending이 읽히는지 확인한다.
8. 월드 이동이 불필요하게 이미지 안에 포함되지 않았는지 확인한다.
9. loop 또는 one-shot 종료 품질이 expectedLoopMode와 맞는지 확인한다.
10. SkillImageEvaluationGuide.md의 Fatal Failure Conditions를 먼저 판정한다.
11. 치명적 실패가 없으면 8개 항목을 100점 만점으로 채점한다.
12. 총점과 치명적 실패 여부를 기준으로 Pass / Conditional Pass / Fail을 판정한다.
13. 실패 또는 감점 항목마다 관찰한 프레임과 구체적인 원인을 기록한다.
14. 재생성이 필요한 경우 기존 프롬프트에서 변경해야 할 영어 문구를 제안한다.
15. 로컬 평가인 경우 시트 크기, 셀 크기, columns, rows, usable frame count를 기록한다.
16. Unity 경로가 제공되면 평가 파일과 Unity 파일의 SHA-256 일치 여부 및 슬라이스/클립 프레임 수를 확인한다.

필수 채점:
- Transparency and Isolation: /15
- Safe Margin and No Cropping: /20
- Frame-to-Frame Consistency: /15
- Motion Readability: /15
- Center and Spatial Stability: /10
- Gameplay Silhouette: /10
- Skill Intent and Theme: /10
- Loop or Ending Quality: /5
- Total: /100

판정:
- Pass: 85~100, 치명적 실패 없음
- Conditional Pass: 75~84, 치명적 실패 없음, 재생성 없이 수정 가능
- Fail: 75 미만 또는 치명적 실패 존재

Output 형식:
Skill Image Animation Evaluation

Skill:
Source JSON:
Asset Path or PixelLab Page:
Reference Asset Path:
Animation Asset Path:
Unity Reference Path:
Unity Animation Path:
Canvas:
Sheet Size / Cell Size / Columns / Rows:
Requested Frames:
Observed Frames:
Usable / Unity Sliced / Clip Frames:
Loop Mode:
Checksum Match:

Fatal Failure Check:
- Transparent background: Pass / Fail
- Character independence: Pass / Fail
- No cropping or edge contact: Pass / Fail
- Consistent canvas and alignment: Pass / Fail
- No unrelated content: Pass / Fail
- Usable frame output: Pass / Fail

Scores:
- Transparency and Isolation: /15
- Safe Margin and No Cropping: /20
- Frame-to-Frame Consistency: /15
- Motion Readability: /15
- Center and Spatial Stability: /10
- Gameplay Silhouette: /10
- Skill Intent and Theme: /10
- Loop or Ending Quality: /5
- Total: /100

Result: Pass / Conditional Pass / Fail
Failure Reasons:
Required Corrections:
Regeneration Prompt Changes:
Notes:

주의:
- 평가만 수행하고 PixelLab 생성 버튼을 누르지 않는다.
- 일부 프레임을 확인할 수 없으면 추정으로 합격시키지 말고 insufficient_evidence로 기록한다.
- 배경 불투명, 캐릭터 포함, 잘림, 가장자리 접촉은 점수와 무관하게 Fail이다.
- 프레임 수가 UI 표기와 스프라이트 시트에서 다르게 보이면 관찰값과 PixelLab 출력 방식을 함께 기록한다.
```
