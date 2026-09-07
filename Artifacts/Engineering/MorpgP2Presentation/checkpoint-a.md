# Checkpoint A — 한결 검토용

- 원계약 readback: /private/tmp/projectbs-current-byeori-episode1-morpg-zone-drop-hud-contract.txt (SHA 65f8d9593e5d56f4921ff90a6790a6cb081c16305e4864700ef44f45a866911c).
- selected revision-01-exact2 manifest 및 Gold/XP 128px RGBA 파일 SHA 실파일 일치 확인. runtime canonical asset은 아직 설치되지 않음.
- 원계약은 immediate credit / visuals non-authoritative. 이번 한결의 명시 지시인 reservation→presentation→arrival commit을 우선 적용하되, failover/flush는 동일 ledger 경유 즉시 commit. 별도 economy owner는 만들지 않음.
- 원계약 grouping: Ground만 .60wu 최근접 거리→host bundleId 사전순, 없고 cap12이면 oldest creationSequence→hostId. pause 시 reward visual/HUD tween/transaction 모두 정지. scaled delta를 단일 clock으로 사용. unscaled 자율 진행 없음.
- 현재 BattleMorpgLiveRoute.OnDied가 ledger.TryCredit 및 계정/HUD 수치를 즉시 변경. BattleSpawnManager가 route tick/dispose 소유. 기존 DrawHud는 OnGUI 텍스트 박스이며 currency-specific fly anchor와 bottom XP bar가 없음. PartyHud는 party member/skill 전용; Gold/XP anchor가 없음.
- 계획: exact route-owned ordered pending receipt queue + bounded visual groups, optional runtime uGUI Gold/XP HUD 및 selected image refs. 도착은 queue Ready 표시만 하며 단일 queue가 순서대로 ledger commit. presentation missing/inactive/teardown은 pending receipt flush. source-zone settlement 완료 전 fade/next wave/final victory 차단.
- 같은 경로 fingerprint/기존 diff 보존 snapshot은 checkpoint-a.json 및 before/. 초기 canonical 변경0, scene/index 변경0.
