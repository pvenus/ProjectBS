# MORPG wave background exact3 + barrier exact6 atomic install

**EXACT3_BACKGROUND_EXACT6_BARRIER_ATOMIC_INSTALL_PASS**. 한결의 직접 설치 지시에 따라 W1–W3 배경을 revision02 foreground-empty로 전환하고, revision01 상·하 장벽6종을 독립 설치했습니다. 기존 설치 이미지를 삭제하거나 덮어쓰지 않았습니다.

## 설치 경계와 자료

- 배경 source manifest SHA: `a02e23e9b812768cdbb80b1aec473d1e5cdcca8019b9d43eeb6d36e7cd18338b`.
- 장벽 source manifest SHA: `99c0118b29104e6433779d5c54786589de7643ac4a09ed01a1e9e6325cdc66aa`.
- 설치: `Assets/Resources/battle/morpg/wave-environment-v1/`. 이미지9개, 각각의 meta, 단일 `binding.json`을 새 버전 묶음으로 준비했습니다. 원본 PNG 바이트9/9 동일.
- 변경 전 `first-diff.txt`, `same-path-before.json`, `before/`를 기록했습니다. 기존 runtime은 하나의 session background를 전체 지도에 확대했으며 per-wave 슬롯은 없었습니다. 이번 binding이 배경3/장벽6의 명시적 슬롯을 제공합니다. 기존 session 배경 에셋은 보존됩니다.
- 모든 이미지와 metadata를 준비한 후 버전 디렉터리를 게시했습니다. runtime은 binding 전체와 sprite9개를 모두 검증한 후에만 scene 객체 생성을 시작합니다. 하나라도 누락되면 활성화 전 실패하므로 혼합된 일부 슬롯만 설치하지 않습니다. 이후 재실행 결과도 동일합니다.

## 배치·PPU·충돌·정렬

모든 소스는1536×512, PPU100, Bilinear, Clamp, mipmap0, 무압축, maxTextureSize2048, FullRect mesh를 사용합니다. pivot은 manifest대로 background(.5,.5), top(.5,1), bottom(.5,0)입니다.

Manifest는 world rect를 지정하지 않았으므로 현재32×8 geometry에 대한 world rect를 `binding.json`에 별도로 기록했습니다. 배경은 비율을 유지한32.5×10.8333wu이며 y=-4.5..6.3333에 배치합니다. 전투 상단 y4가 이미지 아래쪽78.46% 위치에 대응하여 하단75~80% 빈 지면 framing을 유지합니다. 카메라 최대 viewport도 배경 내부에 들어옵니다.

장벽은 각 wave에 top/bottom 하나씩 독립 renderer를 가지며 width32.5, height3.2wu로 외곽 band에 맞춥니다. top y2.5..5.7, bottom y-5.7..-2.5입니다. **장벽은 수평·수직 scale을 독립 적용**합니다. 원본 픽셀이나 manifest pivot을 변경한 것이 아닙니다. 정적 world preview에 실제 이 배치를 반영했습니다.

정렬은 background=-1000, top/back=-900, bottom foreground=50으로 고정합니다. 기존 프랍 overlap fade, body/HUD/telegraph/reward 코드와 순서는 유지합니다. Bottom 장벽을 프랍 자동 정렬 목록에 넣지 않아 back band로 내려가지 않습니다.

각 장벽에는 별도 PolygonCollider2D 하나를 둡니다. 투명 영역을 가로지르는 대형 사각형 대신, **각 패치 전체의 alpha>=192가 검증된 불투명 실루엣만** 사용합니다.6개 collider에 합계138개 작은 사각 path가 있으며, 패치는 모두 |y|>=4.35입니다. 기존 player polygon±4, 최대 camera viewport±4.25, spawn y±2.1과 겹치지 않습니다. 기존 닫힌 zone 경계와 내부 prop collider는 그대로 유지되어 벽의 gameplay 의미가 바뀌지 않습니다. 각 장벽 collider는 해당 wave structural root와 함께 활성화/비활성화합니다.

## 검증

| 검사 | 결과 |
|---|---:|
| 실제 C# compiler | 오류0 / 기존 경고31 |
| Live horizontal/Dash/sorting/new dressing | 43/43 |
| Horizontal geometry | 14/14 |
| Dash clock | 9/9 |
| Reward | 12/12 |
| Inactive spawn | 10/10 |
| NPC cast | 4/4 |
| P1 authority/fault/dedup | 27/27 |
| 총 결정적 검사 | **119/119** |

추가 확인:9개 중 어느 sprite가 빠져도 scene mutation 이전 실패, 잘못된 sorting/unknown JSON/전투 영역 침범 collider 거부, pivot/scale 계산,6개 silhouette path와 zone 수명, top/bottom 정렬 유지. 기존 Dash 검사에는1,762개 합법 clear 위치 sweep도 포함됩니다. 연결성28개 미연결0, P2 선택 보상 asset 검사 PASS.

원본 이미지9/9 byte exact, 새 GUID/meta 누락0·중복0. 기존 좌표·예약·프랍 이미지/meta·Dash/camera 관련 보호 파일50개의 same-path SHA diff0. Git index hash 변경0. `git diff --check`와 binary PNG를 포함한 한정 패치 reverse-check가 통과했습니다.

Unity GUI/headless/Play Mode 실행0, scene 편집0, staging/commit/push0. compiler는 C# compiler만 호출했습니다. 행동 검사의 Unity API는 스텁이므로 실제 GPU/물리 화면 검증으로 해석하지 않습니다.

## 결과와 롤백

`receipt.json`, `installed-assets.json`, `asset-audit.json`, `binding.json`, 각 검사 로그를 함께 확인합니다. `wave1-world-preview.png` 등은 설치 픽셀과 world rect로 합성한 정적 배치 자료이며 Unity screenshot이 아닙니다. 녹색은 기존32×8 전투 경계, 적색은 신규 실루엣 collider입니다.

`wave-dressing-only.diff`에는 신규 PNG의 binary patch도 포함합니다. `rollback-manifest.json`의 현재 SHA를 먼저 확인한 뒤 이 패치만 역적용하면 기존 배경 runtime으로 복원됩니다. 기존 이미지 삭제나 전체 repository reset은 필요하지 않습니다. 역적용 사전 검사만 수행했으며 실제 rollback은 실행하지 않았습니다.

## 보고 전달 상태

요청한 한결 작업으로 직접 완료 보고를 보냈으나 자동 승인 검토가 전송을 거절했습니다. 사유는 대상 작업의 소유자와 내부 경로·검증·롤백 자료를 공개할 구체적 권한 미확인입니다. 우회하지 않았으며 로컬 결과는 모두 준비되어 있습니다. 전달에 대한 명시적 승인이 필요합니다.
