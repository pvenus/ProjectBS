# MORPG pivot import rootfix

현재 Play를 중지하고, Editor가 변경된 PNG/meta를 재임포트한 뒤 Play를 재시작해야 합니다. 현재 fallback 경로는 파일 변경만으로 다시 활성화되지 않습니다.

## 변경 및 근거

Editor.log에서 `wave1.top` imported canvas/pivot mismatch로 preactivation legacy fallback이 두 번 발생한 것을 확인했습니다. 당시 오류에는 실제 Sprite rect/PPU/pivot 숫자가 없으므로 캐시의 실제 pivot 수치를 확정하지 않았습니다. 기존 meta는 top/bottom에 custom edge pivot을 사용했습니다.

정확히 9개 importer를 Center (`spriteAlignment: 0`, normalized pivot 0.5/0.5)로 통일했습니다. binding pivot도 center로 맞추고 런타임 배치 기준을 full-canvas rect 중심으로 명시했습니다. 1536×512 / PPU 100 / pixel pivot (768,256)의 엄격한 검증은 유지합니다. 실패 로그에는 실제 수치와 기대 수치, refresh/restart 안내가 포함됩니다.

원본 PNG 바이트와 9개 GUID를 유지했습니다. PNG와 meta 18개 의존 파일의 mtime을 전진시켰으며 전후 값과 SHA를 기록했습니다. Library 삭제나 Unity 실행/조작은 하지 않았습니다. binding rect, alphaRect, colliderRects 및 interiorPropsEnabled=true는 유지했습니다. 기존 prop·viewport·camera·barrier 수정도 유지했습니다.

## 검증

- Unity Roslyn 컴파일: 오류 0, 기존 경고 31.
- 하니스 회귀 130/130: live 54, geometry 14, clock 9, reward 12, spawn 10, cast 4, P1 27.
- 9개 슬롯별 잘못된 rect/PPU/pivot 거부 및 수치 진단 확인.
- 의도적 preflight 실패: legacy visible 1. 정상 경로: wave1 background 1 + barriers 2, legacy visible 0. 이는 Unity stub 기반 실제 런타임 클래스 하니스 결과입니다.
- meta readback 9/9, PNG/GUID 유지 9/9, 의존 timestamp 전진 18/18, rect/alpha/collider/interior 차이 0.
- git diff --check 및 changes.patch 역적용 사전 검사 통과.

실제 Unity 재임포트 후 Sprite 수치와 화면 결과는 아직 확인하지 않았습니다. 다음 Play에서 center pivot 검증 9/9 로그와 MORPG 표시를 확인해야 합니다. 이전 false-gate viewport 증거는 현재 상태의 승인 근거가 아닙니다.

## 증거 및 롤백

`receipt.json`, `meta-readback-and-dependencies.json`, 각 검증 log, `editor-evidence-before.txt`를 참조하십시오. `before/`는 이번 수정 직전 파일 15개를 보관하며 `changes.patch`는 이번 변경과 새 audit 스크립트만 포함합니다. 역적용은 검사만 했고 실제 실행하지 않았습니다. 이후 공유 작업이 바뀌었다면 다시 역적용 검사를 해야 합니다. 필요 시 변경 patch만 역적용하고, timestamp 원복은 `before-file-dependencies.json`의 atimeNs/mtimeNs를 사용합니다. 원본 이미지나 기존 공유 변경을 삭제하지 마십시오.
