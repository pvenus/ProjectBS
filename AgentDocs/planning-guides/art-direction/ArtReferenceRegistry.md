# ProjectBS Art Reference Registry

Status: current reference authority
Owner: 화감
Registry version: 1.1.0 (2026-08-27)

경로 존재는 승인이 아니다. 아래 행의 `decision`과 `allowedUse`만 권위가 있다.

## 승인 기준과 제한 기준

| ID | 경로 | decision | 역할·승인 이유 | 적용 | 금지 오해 |
| --- | --- | --- | --- | --- | --- |
| CHAR-STYLE-001 | `AgentDocs/reference-assets/generated-media/style-only/character_single_image/open_ink_wash_dynamic_contour/b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf.png` | 승인 | 먹선 계층, 여백, 절제된 담채, 조선 복식의 기준 | 캐릭터 스타일 전용 | 배경·정체성·정확한 체형을 복제하지 않음 |
| CHAR-RUNTIME-001 | `Assets/ImagesGenerated/Character/portrait/character.seojin.2.portrait.png` | 조건부 승인 | 투명 전신, 실사용 가능한 실루엣과 색 구분 | 플레이어 캐릭터 점유율·출력 참고 | 표면의 반복적 디지털 얼룩을 스타일 목표로 삼지 않음 |
| CHAR-STYLE-002 | `Assets/ImagesGenerated/Character/portrait/character.red_doll_hexer.2.portrait.png` | 조건부 승인 | 마른 먹선, 제한된 적색, 소품을 통한 역할 식별 | 적대 인물의 선·색 밀도 참고 | 얼굴 생략을 모든 캐릭터에 일반화하지 않음 |
| STAGE-STYLE-001 | `Assets/ImagesGenerated/Stage/popup_main/node.act1.chapter01.episode03_1.defend_hut.main.png` | 승인 | 저채도 회갈색, 조선 생활 공간, 한 장면의 긴장과 인물 관계 | 스토리 팝업 | 복잡한 군중 구도를 모든 팝업에 복제하지 않음 |
| BATTLE-STYLE-001 | `Assets/ImagesGenerated/Battle/background/battle.act1.chapter01.01.rescue_villagers.background.png` | 승인 | 중앙 전투 여백, 낮은 지평선, 먹색 산세와 흙색 | 낮 전투 배경 | 장소 고유 요소까지 제거한 빈 운동장으로 만들지 않음 |
| BATTLE-MOOD-001 | `Assets/ImagesGenerated/Battle/background/battle.act1.event07.red_doll_shadow_hex.background.png` | 조건부 승인 | 조선 마을 실루엣과 붉은 사건 강조 | 석양·주술 전투 분위기 | 주황색 전체 팔레트를 기본 전투색으로 일반화하지 않음 |
| SKILL-STYLE-001 | `Assets/ImagesGenerated/Skill/icon/skill.character.seojin.2.active_2.crane_wing_formation.icon.png` | 조건부 승인 | 매듭 중심의 단일 상징, 좌우 대칭, 투명 배경 | 스킬 아이콘 형태·붓결 참고 | 청록 채도와 세부량을 모든 아이콘에 복제하지 않음; 80px 검증 전 최종 승인 아님 |
| ITEM-SILHOUETTE-001 | `Assets/ImagesGenerated/Item/icon/item.relic.old_war_horn.icon.png` | 조건부 승인 | 단일 물체, 대각 실루엣, 재질 구분 | 아이템 아이콘 구성 | 3D식 광택을 강화하지 않음; 역사성은 별도 근거 필요 |
| UI-LANGUAGE-001 | `Assets/Images/UI/minhwa/bar/ppu300/minhwa.bar.short.cloud_dot.frame.ppu300.png` | 조건부 승인 | 얇은 전통 문양 테두리와 넓은 정보 영역 | UI 프레임 밀도 | 황금색·기하 장식을 모든 UI에 중첩하지 않음 |
| STAGE-RANDOM-GROWTH-001 | `Assets/ImagesGenerated/Stage/popup_main/node.act1.random_growth.01.crying_bell_smithy_trial.intro.main.png` | 승인 | 빈 대장간의 쇠종과 달아오른 안전패를 단일 수직 초점으로 묶고, 저채도 먹빛·회갈색과 하단 한지 여백으로 사건 위험과 본문 가독성을 함께 확보 | `node.act1.random_growth.01.crying_bell_smithy_trial.intro` 전용 popup_main 및 동계열 위험 사건의 밀도·여백 참고 | 안전패를 유물·보상처럼 일반화하지 않음; 기존 event16의 대체 이미지가 아님; 다른 사건에 쇠종 구도를 복제하지 않음 |

## 반례

| ID | 경로 | 판정 근거 | 재사용 허용 |
| --- | --- | --- | --- |
| ANTI-CHAR-001 | `Assets/ImagesGenerated/Character/portrait/character.training_ground_captain.2.portrait.png` | 고대비 반실사 얼굴·재질과 과도한 검정 면이 기준 담묵 캐릭터군에서 이탈 | 레거시 런타임 유지 가능; 신규 스타일 참조 금지 |
| ANTI-SKILL-001 | `Assets/ImagesGenerated/Skill/icon/skill.character.yujin.3.active_2.hwalbin_barrage.icon.png` | 불투명 검정 배경, 네온 보라·청록, 서구 모바일 판타지 VFX 인상 | 신규 생성·파생 금지; 교체 백로그 |
| ANTI-SKILL-002 | `Assets/ImagesGenerated/Skill/icon/skill.strategic.soulbreaking_formation.icon.png` | RGBA이나 alpha bbox가 비어 있어 보이는 콘텐츠가 없음 | 사용 금지; 재생성 필요 |
| ANTI-ITEM-001 | `Assets/ImagesGenerated/Item/icon/item.relic.blunt_gear.icon.png` | 톱니바퀴 형태와 공업적 청록 도장이 조선 시대성 근거 없이 지배 | 기획 근거 확인 전 신규 기준 사용 금지 |

## 레지스트리 변경 규칙

새 기준은 화감의 실제 크기 검수와 승인 사유가 있어야 추가된다. 기존 기준을 대체할 경우 `supersedes`, 영향 도메인, 이전 기준으로 제작된 자산 재검수 범위를 함께 기록하고 registry version을 올린다. 반례는 실패 유형이 재발할 가능성이 있을 때 유지한다.
