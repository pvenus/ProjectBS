# Skill Animation ImageGen Sample Concept Guide

## Status

이 문서는 메인 캐릭터 스킬 애니메이션의 ImageGen 샘플 제작에서 확인한 컨셉과 작업 방식을 정리한다. 현재 결과는 `concept_sample`이며 Unity production frame set이 아니다.

## Message-only workflow

```text
스킬 기획
-> 완결적인 채팅 핸드오프 메시지
-> ImageGen 스킬 애니메이션 요청
-> 컨셉 애니메이션 생성
-> GIF 또는 frame 이미지 자체를 채팅에 첨부해 전달
```

채팅 간 planning/prompt/generation record, manifest, package 또는 output path를 전달하지 않는다. 다음 채팅이 이전 파일을 열지 않아도 되도록 다음 정보를 메시지 본문에 모두 포함한다.

- 전체 skillId와 표시 이름
- gameplay category, target, range, lifetime
- 핵심 효과와 부가 효과
- 시작·전개·강조·종료/루프의 ordered phase
- frame count, timing, loop mode
- effect origin, 방향, scale, safe margin
- palette, material, Korean visual motif
- 캐릭터/문자/UI/배경 등 금지 요소
- 실제 reference 또는 generated media attachment

Unity `.meta` 파일은 어느 단계에서도 생성·수정·복사·삭제·검증하지 않는다.

## Shared visual direction

- 캐릭터 본체를 그리지 않는 독립형 VFX다.
- 메인 구조와 운동 방향은 굵기가 변하는 먹선, wet-to-dry 전환, 갈라진 붓끝과 제한된 먹 번짐으로 만든다.
- 먹선화나 크레파스 그림처럼 평면적으로 단순화하지 않는다. 내부 디테일, 깊이, 작은 반짝임과 게임 VFX의 정교함은 유지한다.
- 역동성은 파스텔 안료 가루, 짧게 끊긴 파스텔 획, 먹방울, 가늘어지는 붓꼬리와 진행 방향을 따르는 곡선 파티클로 높인다.
- 광점과 파티클은 주 실루엣보다 강하지 않으며 색은 스킬 의미 구분에 필요한 범위로 제한한다.
- 스킬별 주 실루엣 하나가 먼저 읽히고 부가 입자와 전통 요소는 종속된다.
- 프레임마다 동일한 effect origin, scale, canvas와 safe margin을 유지한다.
- 기본 스킬 애니메이션은 시작과 소멸이 없는 4프레임 지속 loop로 설계한다. 생성/등장과 종료/소멸은 런타임 또는 별도 효과가 담당한다.
- 네 frame은 동일한 effect origin, footprint, camera와 scale을 유지하며 먹선 회전 위상, 이동 광점, 내부 광량과 작은 파티클 궤도만 순환한다. frame 4에서 frame 1로 자연스럽게 이어져야 한다.
- 그릇, 침처럼 프레임마다 다시 그릴 때 흔들리는 hero object보다 고정 field/ring을 loop identity로 사용한다. hero object가 반드시 필요하면 identity·scale·baseline을 모든 frame에서 완전히 고정하고 런타임 이동과 배치를 분리한다.
- 텍스트, 문자, 숫자, UI, 워터마크와 외국 문화 상징을 넣지 않는다.
- 컨셉 보드는 흐름 확인용이다. production은 ordered 개별 프레임 또는 playable GIF로 다시 생성·검증한다.

## Sample A — Seojin

- skillId: `skill.character.seojin.3.active_2.crane_wing_formation`
- meaning: 5초 동안 유지되는 학익진 영역. 적에게 지속 피해와 이동속도 감소를 주고 아군 방어를 높인다.
- concept: 먹빛 학 날개 진형, 안쪽의 무광 청동 방어환, 바깥쪽의 절제된 주홍 감속 경계.
- six phases: 작은 지휘 인장 → 양 날개 전개 → 진형 고정 → 청동/주홍 맥동 → 먹깃 회수 → 시작 인장에 가까운 루프 연결.
- keep: 중앙 effect origin, 좌우 대칭, 영역 제어의 넓은 실루엣.
- avoid: 실제 학 캐릭터, 장군, 무기, 문자 부적, 과도한 백색 날개와 서양 천사 이미지.

## Sample B — Yujin

- skillId: `skill.character.yujin.3.active_2.hwalbin_barrage`
- meaning: 런타임이 동일한 투사체 인스턴스를 3열·4회 연사한다. 이미지 애니메이션은 재사용 가능한 화살 한 발만 표현한다.
- concept: 하나의 청록 먹선 화살에 황토색 대나무 섬유 궤적, 남색 마른 붓 잔상과 작은 광점이 맥동한다.
- six phases: 희미한 바람매듭과 단일 화살 → 먹선 응집 → 파스텔 꼬리 확장 → 제한된 최고 광점 → 붓꼬리 감쇠 → 시작 상태에 가까운 루프 연결.
- keep: 모든 프레임에 정확히 화살 한 발, 명확한 우측 방향, 동일 effect origin·baseline·길이·scale. 화살은 셀 안에서 이동하지 않고 내부 광점과 주변 먹선 파티클만 변화한다.
- runtime ownership: 3열 배치, projectile count와 네 차례 burst는 게임 로직이 담당하며 이미지에 복수 화살로 굽지 않는다.
- avoid: 캐릭터, 활 본체, 불/번개, 왕실 금장, 서양식 마법 화살.

## Sample C — Jihan

- skillId: `skill.character.jihan.3.active_1.medicine_prescription`
- meaning: 아군 한 명에게 즉시 도달해 고정량과 공격력 비례 체력을 회복한다.
- concept: 봉인된 약첩이 열리며 청자빛 약탕과 쑥·당귀색 약기가 원형 치유 파동으로 퍼진다.
- six phases: 접힌 약첩 → 봉인 해제 → 깨진 약탕기 파편의 조립 → 온전한 약탕기 1회 타격 → 용기 없는 약초/치유 폭발 → 용기 없는 부드러운 소멸.
- keep: 따뜻하고 안정적인 support 판독, 중심에서 바깥으로 퍼지는 한 번의 명확한 회복 pulse.
- avoid: 수도승 캐릭터, 독/해골, 서양 의료 십자, 읽을 수 있는 처방 문자.

## Extended active-skill samples

### Seojin — Charge

- skillId: `skill.character.seojin.3.active_1.charge`
- one reusable effect: 우측 방향의 먹빛 충격 쐐기 하나. 실제 캐릭터 dash는 런타임 소유다.
- motion: 압축 → 청동 방어광 응집 → 전방 압력 → 주홍 충돌 → 붓꼬리 반동 → 루프 복귀.

### Seojin — Turtle Ship Assault

- skillId: `skill.character.seojin.3.active_3.turtle_ship_assault`
- one reusable effect: 우측 방향 거북선 먹선 VFX 한 척. 두 인스턴스 생성은 런타임 소유다.
- motion: 장갑 선수 → 청동 장갑 점화 → 먹물 파도 확장 → 포화/충각 광점 → 연기 감쇠 → 루프 복귀.

### Yujin — Multi Shot

- skillId: `skill.character.yujin.3.active_1.multi_shot`
- one reusable effect: 가늘고 빠른 우측 화살 한 발. 10발·60도 spread는 런타임 소유다.
- motion: 조밀한 화살 → 꼬리 응집 → 은빛 중심 → 최고 광점 → 파티클 감쇠 → 루프 복귀.
- Hwalbin Barrage보다 가늘고 소용돌이가 적으며 송엽색·목탄색을 중심으로 구분한다.

### Yujin — Outlaw Appearance

- skillId: `skill.character.yujin.3.active_3.outlaw_appearance`
- one-shot effect: 캐릭터 없이 목탄 먹구름이 빠르게 펼쳐지고 솔잎색/황토색 신호광 뒤에 흩어진다.
- Japanese ninja smoke가 아닌 한국 수묵 연무와 찢긴 붓 파티클로 표현한다.

### Jihan — Ten Tonic Soup

- skillId: `skill.character.jihan.3.active_2.ten_tonic_soup`
- one-shot effect: 열 가지 약재 배열 → 약재 흡입 → 약액 방울 → 온전한 약탕기 1회 → 용기 없는 강화 오라 → 약효 인장으로 전환한다.
- 완성 약탕기·그릇은 한 phase에만 등장하며 다른 phase에서는 재료, 액체, 오라와 인장을 주 실루엣으로 사용한다.
- Medicine Prescription보다 짙은 갈색·쑥색과 넓은 party buff pulse로 구분한다.

### Jihan — Divine Acupuncture

- skillId: `skill.character.jihan.3.active_3.divine_acupuncture`
- one-shot key pose set: 경혈도 → 미완성 침 파편/기 실 → 온전한 침 1회 타격 → 침 없는 경락 파동 → 침 없는 생명기 폭발 → 적자색 기맥 봉인.
- 온전한 장침은 타격 phase에만 등장하며 다른 phase에는 반복하지 않는다.
- targeting과 ally 적용은 런타임 소유이며 이미지에는 인체도·피·복수 침을 넣지 않는다.

## Grade reuse policy

동일한 기획명을 공유하는 grade 1/2/3 스킬은 projectile count, damage, duration 같은 수치 차이를 이미지에 굽지 않는다. 하나의 단일 투사체/효과 프레임셋을 grade별 content ID에 복제할 수 있으며, 시각적으로 구분할 명시적 기획이 생긴 경우에만 별도 생성한다. 이름 없는 하위 단계 `active_1/2/3` placeholder와 독립 시각 의미가 없는 passive는 현재 샘플 생성 대상에서 제외한다.

## Sample findings

ImageGen은 3×2 six-phase 구성과 스킬별 실루엣·색상 차이를 잘 표현했다. 첫 거친 먹선 수정은 디테일과 광택을 너무 제거해 크레파스/평면 드로잉처럼 보였으므로 채택하지 않았다. 최종 샘플 방향은 `먹선이 주 질감`, `세부 묘사와 반짝임 유지`, `파스텔 안료와 먹 터치 파티클로 운동 강화`다.

`genuinely transparent background` 요청만으로는 체크무늬 또는 불투명 배경이 구워질 수 있다. 실제 파일의 alpha를 반드시 검사한다. 직접 alpha가 없으면 foreground에 사용하지 않은 균일 chroma 배경으로 ImageGen 분리본을 만든 뒤, 배경 채널 우세도만 이용해 알파 마스크를 생성한다. 최종 PNG는 RGBA, 완전 투명 픽셀, 부분 알파 가장자리를 검사한다. 숨겨진 RGB 키 색은 투명 픽셀에 남을 수 있으나 렌더링에는 사용되지 않는다. 효과 픽셀을 다시 그리거나 `.meta`를 조작해서 보정하지 않는다.

프레임 추출 전에는 다음을 확인한다.

- 정확히 여섯 phase가 있는가
- 여섯 phase의 의미와 주 실루엣이 서로 구분되는가
- 동일한 완성 hero object가 두 frame 이상 반복되지 않는가. 예외인 단일 투사체라면 identity·scale·baseline 고정과 런타임 소유가 명시됐는가
- cell마다 effect origin과 scale이 유지되는가
- 인접 cell의 빛과 입자가 섞이지 않는가
- 시작/종료가 loop 또는 one-shot 계약과 맞는가
- 배경과 전경을 안전하게 분리할 실제 alpha가 있는가

## Separate-chat handoff template

```text
Skill animation sample generation request

skillId:
skill meaning:
target / duration / direction:
ordered phases:
frame count / timing / loop:
effect origin / canvas / safe margin:
visual concept / palette / material:
required elements:
prohibited elements:
reference attachments:
delivery: attach the actual generated GIF and/or ordered frame images in chat
file artifacts: none
Unity .meta operations: forbidden
```
