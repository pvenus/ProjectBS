# Item Icon Generation Prompt

승인된 아이템 기획을 기준으로 PixelLab UI Pro에서 동일 아이템의 후보 네
개를 생성하고, 통과한 128x128 단일 아이콘만 보존 및 Unity에 반영할 때
사용합니다. 현재 지원 프로필은 `relic`입니다.

## Prompt

```text
작업 폴더 = {project_root}

아래 입력과 가이드를 기준으로 아이템 1개의 정적 픽셀 아트 아이콘을 생성해줘. 스킬 아이콘의 실행 흐름은 참고하되, 아이템은 투명 배경의 독립된 물리 오브젝트로 표현하고 스킬 아이콘의 80x80 프레임 규칙은 적용하지 마.

Input:
- projectRoot: {project_root}
- itemCategory: relic
- itemId: {item.relic.lowercase_snake_case_slug}
- itemPlanningFile: {projectRoot_기준_승인된_아이템_기획_상대경로 | null}
- itemJsonFile: {projectRoot_기준_승인된_아이템_JSON_상대경로 | null}
- evaluationRoot: {현재_PC에서_기존에_사용하는_PixelLab_아이템_평가_루트}
- pixelLabCreatorUrl: https://www.pixellab.ai/create?tool=create_ui_pro
- pixelLabTool: Create UI elements (Pro)
- customSize: 128x128
- noBackground: true
- conceptImage: none
- maxGenerationRuns: 2
- outputIconPath: Assets/Resources/item/icon/{itemId}.icon.png
- allowOverwrite: false

itemPlanningFile을 우선 사용한다. 기획 파일이 없을 때만 itemJsonFile의 승인된 이름, 설명, 효과와 presentation 정보를 사용한다. 둘 다 없거나 서로 핵심 물체·효과가 충돌하면 생성하지 않는다.

참조 가이드:
- Assets/character_concepts/game_prompt_guide/item/ItemIconGenerationGuide.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemPlanningGuide.md
- Assets/character_concepts/game_prompt_guide/item/relic/RelicItemJsonGuide.md
- Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md

작업:
1. 현재 PC에서 projectRoot와 evaluationRoot가 기존 경로 체계와 일치하는지 확인한다. 다른 PC의 절대 경로를 복사하거나 새 평가 루트 관례를 임의로 만들지 않는다.
2. itemId가 `item.relic.{lowercase_snake_case_slug}`인지 확인하고 source의 relicId와 일치시킨다.
3. 승인된 source에서 physicalObject, silhouette, material, ornament, approvedEffectCue, accentPalette를 추출한다. 수치, 확률, 지속시간은 아이콘에 표현하지 않는다.
4. `centered_emblem`, `hanging_talisman`, `upright_vessel`, `diagonal_relic`, `curved_relic` 중 하나의 compositionProfile을 물리 형상에 따라 선택한다.
5. `Assets/Resources/shop/relic/relic-icon-01.png`부터 `relic-icon-09.png`까지는 스타일 분석 근거로만 읽는다. Concept Image로 업로드하거나 신규 파일명·Sprite 이름의 근거로 복사하지 않는다.
6. ItemIconGenerationGuide의 Concise Description Contract에 따라 영어 Description을 다섯 개의 짧은 문장으로 작성한다: Primary object, Material, Effect cue, Style, Composition/exclusions.
7. effect cue가 필요하지 않으면 셋째 문장을 생략하고 나머지 문장을 늘리지 않는다. 아이템 자체보다 연기, 번개, 불꽃, 오라가 커지지 않게 한다.
8. PixelLab `Create UI elements (Pro)`를 열고 Custom size를 128x128, No Background를 On, Concept Image를 비운 상태로 설정한다. Color Palette에는 어두운 기본 재질, 한 가지 강조색 계열, 제한된 금속 하이라이트만 입력한다.
9. 한 번의 실행에서 네 칸 모두 같은 아이템의 대안이 되게 요청한다. 서로 다른 유물 네 개를 요청하지 않는다.
10. 반환된 256x256 2x2 결과 시트를 evaluationRoot의 기존 itemId 구조에 원본 그대로 보존한다.
11. 시트를 이미지 좌표 기준 top-left, top-right, bottom-left, bottom-right 순서의 네 128x128 후보로 무손실 분리한다. 확대·축소, 보간, 자동 크롭, 배경 합성, 프레임 추가를 하지 않는다.
12. 각 후보가 RGBA 128x128, 투명 배경, 한 개의 물리 아이템, 최소 10px safe margin인지 정적 검사한다. 64x64 nearest-neighbor 미리보기도 만든다.
13. 가이드의 100점 기준으로 네 후보를 모두 평가한다. Hard rejection이 없고 85점 이상인 최고점 후보 하나만 선택한다.
14. 네 후보가 모두 실패하면 실패한 Description 블록 하나만 더 구체적이고 짧게 교체하고 새 seed로 한 번만 재생성한다. 금지문을 누적하거나 Concept Image를 추가하지 않는다.
15. 통과 후보를 `{evaluationRoot}/{itemId}/source/{itemId}.icon.png`에 보존하고 SHA-256을 기록한다.
16. allowOverwrite=false이고 outputIconPath 또는 `.meta`가 이미 존재하면 Unity 파일을 수정하지 않고 `existing_item_icon_requires_approval`로 중단한다.
17. 통과 후보가 생긴 뒤에만 canonical `Assets/Resources/item/icon` 폴더가 없으면 정확히 그 경로를 생성하고, 승인된 Unity import 절차로 폴더 `.meta`를 만든다. 다른 아이템 아이콘 경로는 새로 만들지 않는다.
18. 통과한 보존 원본만 outputIconPath로 byte-for-byte 복사한다. Sprite 이름은 `{itemId}.icon`이어야 하며 단일 Sprite, 중앙 pivot, mipmaps off, alpha transparency on, atlas slicing 없음으로 가져온다.
19. 기존 아이콘 교체가 승인된 경우 `.meta` GUID를 보존한다. 신규 파일은 승인된 Unity import 절차로 새 `.meta`를 만들고 다른 GUID를 재사용하지 않는다.
20. `Assets/Resources/shop/relic`, item JSON, RelicSO, EffectSO, localization, shop/pool/reward 데이터와 gameplay balance를 수정하지 않는다.

Output:
- Item ID / Category:
- Source Planning / JSON:
- Composition Profile:
- Physical Object / Material / Ornament:
- Approved Effect Cue:
- PixelLab Description:
- Color Palette:
- PixelLab Tool / Custom Size / Settings:
- Raw Sheet Path / Size:
- Candidate Extraction Paths:
- Candidate Scores / Rejection Reasons:
- 64x64 Preview Results:
- Selected Candidate / SHA-256:
- Preserved Source Path:
- Unity Output Path / Sprite Name:
- Unity Import / Meta Status:
- Modified Files:
- Result: Pass / Fail

검증:
- PixelLab 도구는 `Create UI elements (Pro)`이고 Custom size는 128x128이어야 한다.
- 결과 시트는 256x256이며 정확히 네 개의 128x128 후보로 분리되어야 한다.
- 최종 후보는 RGBA, 투명 배경, 단일 물리 아이템, 최소 10px safe margin이어야 한다.
- 최종 후보는 64x64에서 실루엣과 물체 종류가 식별되어야 한다.
- 아이템 효과는 최대 한 가지 보조 cue이며 물리 아이템보다 강조되지 않아야 한다.
- 최종 점수는 85점 이상이고 Hard rejection이 없어야 한다.
- filename은 `{itemId}.icon.png`, Sprite 이름은 `{itemId}.icon`이어야 한다.
- 보존 원본과 Unity PNG의 SHA-256이 동일해야 한다.
- legacy `Assets/Resources/shop/relic` 파일은 변경되지 않아야 한다.

실패 시 Output:
- status: failed
- failureType:
  - missing_item_source
  - invalid_item_id
  - unsupported_item_category
  - unresolved_item_concept
  - missing_visual_direction
  - pixellab_unavailable
  - pixellab_authentication_failed
  - insufficient_pixellab_credits
  - wrong_pixellab_tool
  - generation_timeout
  - invalid_result_sheet
  - candidate_extraction_failed
  - no_passing_candidate
  - preservation_failed
  - existing_item_icon_requires_approval
  - output_write_failed
  - sprite_name_mismatch
  - unity_import_pending
- 실패 원인과 실패한 Description 블록
- 사용한 source와 기존 경로
- 생성하거나 수정하지 않은 파일
- 후보별 점수와 rejection 사유
- 다음에 필요한 승인 또는 재시도 방향

주의:
- Concept Image나 스타일 레퍼런스를 사용하지 않는다.
- 네 후보를 서로 다른 아이템으로 만들지 않는다.
- 투명 배경에 프레임, 카드, 인벤토리 슬롯, 장면을 추가하지 않는다.
- legacy 2x2 시트를 직접 수정하거나 신규 아이콘 저장소로 재사용하지 않는다.
- PixelLab 실패 시 다른 이미지 생성 도구로 대체하지 않는다.
```
