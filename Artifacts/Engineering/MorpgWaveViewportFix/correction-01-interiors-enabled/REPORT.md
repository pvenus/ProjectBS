# Correction01 — production true, freshly re-executed

**PRODUCTION_TRUE_REEXECUTED_ACCEPTANCE_PASS**. 현재 production binding은 `interiorPropsEnabled=true`입니다. 복사된 `binding.json`도 동일한true 파일이며 SHA는 receipt에 기록했습니다. 이 correction revision이 상위 폴더의 false/default-off 자료를 대체합니다. 상위 REPORT/receipt는 **PROVENANCE ONLY**로 표시했으며, 예전 binding/log는 역사적 증거로 보존했습니다.

이번 revision에서 production true 상태로 실제 C# compiler, live focused suite, geometry, connectivity, Dash clock, P2 reward, spawn, sorting/cast, legacy background 회귀와 viewport 합성을 모두 다시 실행했습니다. 이전 false 결과의 문자열만 변경한 보고가 아닙니다.

## 직접 증거

`live.log`에는 다음 실행 결과가 있습니다.

- `AUDIT production true: props17/decor54, structural colliders8/4/5; all17 prop centers blocked`
- `AUDIT 1762 legal clear positions swept`
- `PASS 52/52 live glue simulations (Unity engine stub)`

Production-default fixture는 gate를true로 덮어쓰지 않고 실제 binding으로 활성화합니다. 실제 생성 객체를 세어 renderer80개=background3+barrier6+prop17+decoration54, interior collider17개=각zone8/4/5를 확인했습니다. 모든17개 prop 중심을 runtime collision query가 차단하므로 restored collider semantics를 확인합니다. 별도의 이름이 붙은 debug-OFF fixture에서만false capability를 검사합니다.

`connectivity.log`와 `connectivity.json`: actor radius.35 + inset.15, .10wu grid에서28개 reservation 모두 entry와 연결, 미연결0. `geometry.log`: 실제 retained17-prop 지형과28개 예약 및 Dash capsule14/14 PASS. Live의1,762개 legal clear 위치 검사는 원래 prop geometry를 포함합니다.

`compile.log`: 오류0 / 기존 경고31. Live52 + geometry14 + clock9 + reward12 + spawn10 + cast4 + P1 27 = **128/128 PASS**. P2 accounting, props sorting/overlap fade, normal Dash/assisted fallback, legacy normal1/MORPG success0/failure1 및 Dispose 복원도 통과했습니다.

`viewport-audit.json`, `viewport-crops.json`: 프랍17+장식54가 포함된 W1–W3 center/left/right/top/bottom 및4 corner의27개960×540 정적 crop, 미커버 픽셀0. PNG는 이 correction 폴더에 저장되어 있으며 `viewport-contact.png`로 비교합니다. 배경의 baked foreground-empty 상태와 별개로 runtime props는 존재합니다.

## 보존 범위

Binding JSON에서 이전 snapshot과 달라진 항목은 `interiorPropsEnabled` 하나뿐임을 비교했습니다. Revision03 배치/scale/flip/dense lane/예약, source PNG/GUID/meta, barrier alpha bbox와 collider 정렬, camera/player limits, owner-scoped legacy background suppression은 그대로입니다. 생산 코드 변경은 bool 기본값true뿐이며, generator도true를 유지하도록 수정했습니다. false gate는 debug/rollback 기능으로 보존합니다.

`receipt.json`에 실행 로그·binding·connectivity·viewport audit·rollback manifest SHA를 기록했습니다. `before/`, `before-sha.json`, `same-path-before.json`, `interior-restore-only.diff`, `rollback-manifest.json`, `rollback-check.log`는 이 correction revision의 근거입니다. 기존 source/image/geometry 보호 SHA diff0, index hash 변경0, diff-check 및 reverse-check PASS입니다. 실제 rollback을 적용하지 않았습니다. rollback은false로 복귀하므로 현재 사용자 의도true와 구분해야 합니다.

Unity GUI/headless/Play Mode/scene 편집/index/staging/commit/push/destructive asset delete0. 검사는 실제 생산 C# 코드와 Unity API stub이며, 미리보기는 설치 PNG와 world rect의 정적 합성입니다. GPU/실제 물리 실행 증거로 해석하지 않습니다. 구현·증거 정리 blocker0입니다.
