# Production interior props restore — completed

**PRODUCTION_INTERIOR_PROPS_RESTORED**. 한결이 전달한 사용자 의미 정정을 적용했습니다. 제거 대상은 배경 PNG에 baked된 근접 세부 요소이며 runtime interior props가 아닙니다. 이 보고서가 이전 viewport fix의 production gate OFF 상태를 대체합니다.

Production binding과 코드 기본값은 `interiorPropsEnabled=true`입니다. 재생성 스크립트도true를 유지합니다. runtime prop17개 + decoration54개 renderer와 기존 interior collider17개가 다시 활성화됩니다. 배경3+장벽6을 포함한 environment renderer는 총80개입니다. `false` gate는 debug/rollback 용도로 유지하고 별도 fixture에서 검사합니다.

Binding의 변경은 gate false→true뿐임을 JSON 비교로 확인했습니다. Revision03 배치·scale·flip·dense lane·spawn/reservation, revision02 foreground-empty 배경3종, barrier alpha bbox와 collider 정렬, camera/player clamp, Dash/reward/HUD/sorting, legacy background의 owner-scoped suppression은 유지합니다. 원본 PNG·GUID·meta 및 canonical geometry/예약의 보호 SHA diff0입니다.

검증: 실제 C# compile 오류0 / 기존 경고31. Live52/52, geometry14/14, Dash clock9/9, reward12/12, spawn10/10, cast4/4, P1 authority/fault27/27: **총128/128 PASS**. Enabled production default의 renderer/collider 수, debug OFF/재활성, 실제 props sorting/fade, spawn/Dash/reward, legacy 정상1/MORPG 성공0/실패1 복원 검사를 포함합니다.

프랍과 장식을 포함하여 W1–W3 center/left/right/top/bottom 및4 corners의27개960×540 viewport crop을 다시 생성했습니다. 미커버 픽셀0입니다. `viewport-contact.png`, 각 `wave*-viewport-*.png`, `viewport-crops.json`, `viewport-audit.json`을 확인하십시오. 캐릭터가 없는 정적 구성이라 props의 겹침 알파는 기본1이며, 실제 overlap fade는 기존 live 테스트로 검증합니다.

`first-diff.txt`, `same-path-before.json`, `before/`, `interior-restore-only.diff`, `rollback-manifest.json`, `receipt.json`에 근거와 제한된 롤백을 기록했습니다. `git diff --check`와 reverse-check PASS, Git index hash 변경0입니다. 롤백을 실행하지 않았습니다. 역적용하면 gate가false로 돌아가므로 현재 사용자 의도는 **true**임에 유의하십시오.

Unity GUI/headless/Play Mode/scene 편집/index/staging/commit/push/에셋 삭제0. 미리보기는 실제 설치 PNG·배치값을 합성한 정적 viewport이며 Unity GPU screenshot은 아닙니다. 구현 blocker는 없습니다.

전달: 한결에게 수정·검증 완료 알림을 정상 전달했습니다. 기존 승인 검토가 상세 내부 자료 전송을 거절했으므로 경로·수치·receipt 내용은 메시지로 전송하지 않고 로컬에 보관했습니다.
