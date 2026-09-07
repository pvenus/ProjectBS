# 한결 통합 검토용 — P2 월드 보상/흡수/HUD 구현 receipt

상태: **실제 코드·에셋 적용, 정적 컴파일·focused 검증 PASS. Unity 실플레이는 미실행.**
보고 대상은 직속 팀장 한결뿐이다. Checkpoint A 전송은 자동 승인 검토에서 수신 권한 미확인/내부 정보 전달 위험으로 거절됐다. 이 최종 receipt를 공유 작업 공간에 보존했으며, 우회 전달이나 다른 task 보고는 하지 않았다.

## Authority / Checkpoint A

- 원계약: `/private/tmp/projectbs-current-byeori-episode1-morpg-zone-drop-hud-contract.txt`, SHA `65f8d9593e5d56f4921ff90a6790a6cb081c16305e4864700ef44f45a866911c` readback 완료.
- 최신 한결/사용자 지시의 **arrival commit**이 원계약의 immediate-credit 부분을 대체한다. 사망은 reservation만 만들고, 표현 도착 또는 explicit failover에서 동일 ledger가 실제 계정을 변경한다.
- grouping .60wu, squared distance→host ID lexical tie, Ground-only host, oldest creationSequence/host ID fallback을 유지한다. 실제 동시 bundle group cap을 12로 제한한다. 모두 Flying인 cap 포화는 추가 시각 객체를 만들지 않고 순서대로 lossless flush한다.
- ordinary clock은 scaled delta 하나로 관리하며 pause 때 dwell/fly/HUD tween/ordinary commit 모두 정지한다. 명시적인 inactive/teardown failover는 일시정지 상태에서도 확인된 사망 보상을 즉시 정산한다.
- Checkpoint A의 같은 경로 fingerprint 및 before snapshot은 `checkpoint-a.json`, `checkpoint-a.md`, `before/`에 있다.
- 원 P0 addendum의 RawExperienceOnly를 유지한다. XP bar fill은 이번 encounter의 10 XP 수령 진행도이며, 존재하지 않는 레벨/다음 레벨 기준을 만들지 않았다.

## exact9 구현 증거

1. **Death position + selected Gold/XP**: route의 사망 위치 x/y를 receipt group에 보존. `MorpgRewardHudMono`가 Camera projection으로 그 월드 위치에 Gold/XP Sprite 기반 Image 쌍을 표시한다. 지면 정지 중 카메라 이동을 따라 올바른 월드 위치를 유지한다. 그룹 host 위치는 첫 사망 위치로 고정한다.
2. **Dwell .45s**: production queue의 `GroundSeconds=.45f`. 초기 .12s 등장 크기, .20s 안정화 envelope를 적용한다. 그룹 합류는 원계약대로 host의 현재 Ground timeline을 공유한다.
3. **Fly .35s**: `FlySeconds=.35f`. 각 glyph가 독립적인 실제 Gold icon RectTransform / 하단 XP icon RectTransform 위치로 얕은 quadratic arc를 이동한다.
4. **Arrival account/UI commit**: 시각 도착은 receipt Ready만 표시한다. queue가 deathSequence 순서로 기존 `BattleRewardLedger.TryCredit`를 호출한다. Gold+XP 성공 readback 이후에만 HUD receipt를 전달한다. 값/XP fill .30s ease-out, .10s delta coalesce, .12s arrival pulse를 적용한다. 최종 승리 직전에는 authoritative 값을 확정해 다음 upgrade pause에서 tween 중간값이 남지 않게 했다.
5. **Cap/grouping/pool cleanup**: group cap12, 가까운 Ground merge, cap시 oldest Ground merge. UI glyph pair 재사용 pool은 view 수명에 귀속되고 release 시 inactive, failure/teardown에서 view 전체 폐기. 그룹이 ledger/금액을 직접 변경하지 않는다.
6. **Pause/teardown/death exactly-once**: paused ordinary Tick 무변화. confirmed death만 예약하며 death 상태에서도 기존 보상을 정산한다. Close/Flush 반복, 중복 사망, 재진입 callback은 기존 canonical bundle key와 queue receipt table로 mutation0. victory/다음 구역은 PendingCount=0 이후만 가능하다.
7. **Missing/inactive failover**: null presentation, resource sprite 누락, Camera/HUD anchor 소실, inactive glyph, show/refresh 예외, HUD 렌더 예외, view/manager OnDisable, scene teardown 모두 pending을 동일 ledger로 flush한다. HUD 예외가 battle failure로 승격되지 않게 분리했다. 실제 계정 트랜잭션 실패는 기존 required Failed 정책을 유지한다.
8. **Old immediate path duplicate0**: `OnDied`에서 ledger 직접 호출을 제거하고 `delivery.TryReserve`로 교체했다. 계정 변경은 `TryCommitArrivedReward` 한 곳이다. 기존 MORPG root의 일반 GoldDrop 억제도 유지한다.
9. **Other battles0**: exact route gate 이후에만 queue/HUD가 생성된다. 기존 PartyHud prefab/source, CurrencyManager/CharacterExperienceService, 일반 ProbCoin 및 다른 battle의 보상 경로는 수정하지 않았다.

## 변경 파일

Production same-path additive merge:

- `Assets/Scripts/Battle/Morpg/BattleMorpgLiveRoute.cs`: ordered delivery owner, arrival commit callback, settlement barrier, failover/close, final HUD snapshot.
- `Assets/Scripts/Battle/Spawn/Sequence/BattleSpawnManager.cs`: exact route가 있을 때만 OnDisable pending flush.

Production 신규:

- `Assets/Scripts/Battle/Morpg/MorpgRewardDeliveryQueue.cs` (+meta): pure state/receipt/grouping/arrival/flush owner.
- `Assets/Scripts/Battle/Morpg/MorpgRewardHudMono.cs` (+meta): optional runtime uGUI, 실제 Gold/XP targets, sprite glyph pool 및 HUD receipt 표현.
- `Assets/Resources/battle/morpg/rewards/reward.gold.coin-charm.png` (+meta).
- `Assets/Resources/battle/morpg/rewards/reward.xp.faceted-shard.png` (+meta), 필요한 folder meta.

에셋은 화감 `selected/revision-01-exact2/runtime` 파일을 byte-identical 복사했다. Gold SHA `dcfbd73c3795de4df867a17621b7811303836bda11c1699d4e9e5be37b6ead3d`, XP SHA `858c7e13ff286652d9d7841d55a7a9ee2fae09ebfc133a614ddcf07ce696dfd9`. 128×128 RGBA, Single Sprite, PPU100, Bilinear/Clamp, mipmap0, uncompressed RGBA32 importer다. 이미지 재생성/의미 변경은 하지 않았다.

Harness: `AgentTools/MorpgIntegrationHarness`의 run/RuntimeStub/Program을 새 optional view 경계에 맞춰 확장했고, `reward_delivery_tests.cs/.sh`, `reward_asset_checks.py`를 추가했다.

기존 inactive-spawn suite는 현재 공유 체크아웃의 NpcSpawnService가 **ownership→activate→initialize** 순서로 바뀐 상태를 확인해 순서에 과하게 의존하던 assertion을 갱신했다. 이번 P2 작업은 해당 production service를 수정하지 않았다. 직접 inactive initialize 두 케이스를 추가하고, 결과 초기화/동시 reveal1/이전 reveal 취소/실패 living 등록0 검증을 유지했다.

## 검증 결과

- 실제 Unity Roslyn + 프로젝트 Assembly-CSharp 참조: **0 errors / 기존 31 warnings** (`build.log`).
- production delivery queue + 실제 ledger: **12/12** (`delivery-tests.log`). .45/.35 경계, 28 deaths/40G/10XP, duplicate0, tie/cap/grouping, out-of-order arrival ordered commit, pause, missing/HUD/inactive/teardown failover, transaction rollback 검증.
- production live route + Unity 모형: **12/12** (`live-tests.log`). 실제 death-position reservation, arrival account/HUD receipt, pause, disable failover, defeat/teardown pay-confirmed-only, optional HUD exception, 최종 28-receipt/40G/10XP HUD snapshot 포함.
- inactive spawn production-service/lifecycle 모형 **10/10**, cast presenter 모형 **4/4** 및 boundary 검사, P0/P1 **27/27**. 결정적 trace SHA 유지.
- selected asset SHA/import/code-reference checks PASS (`asset-checks.log`).
- tracked `git diff --check` 및 새 파일 whitespace PASS.
- **`git apply --reverse --check p2-only.diff` PASS, 실제 역적용 미실행** (`rollback-check.log`).
- Checkpoint A 이후 **.git/index SHA 동일**. Unity GUI/scene asset/index/staging/commit/push 변경0.

## 실행 재현

```sh
python3 AgentTools/MorpgIntegrationHarness/compile.py
sh AgentTools/MorpgIntegrationHarness/reward_delivery_tests.sh
sh AgentTools/MorpgIntegrationHarness/run.sh
python3 AgentTools/MorpgIntegrationHarness/reward_asset_checks.py
python3 AgentTools/MorpgIntegrationHarness/inactive_spawn_tests.py
python3 AgentTools/MorpgIntegrationHarness/cast_outline_tests.py
sh Artifacts/Engineering/MorpgP1Rollback/revision-02/REPRODUCE.sh
```

## 한결 실플레이 확인 항목

- 두 NPC 사망 위치에서 coin-charm / faceted-shard를 식별할 수 있고 .45s 정지 후 .35s 동안 서로 다른 HUD 목표로 이동하는지.
- 도착 전 Gold/XP 값 유지, 도착 시 실제 계정/표시 증가, 최종 +40G/+10XP, 다음 구역 진입 전 pending0.
- 연속 사망 그룹, cap12, pause/resume, HUD/자산/대상 비활성, 전투 중 종료/패배에서 유실·중복 없음.
- 기존 PartyHud/화면 safe area와 겹침 및 UI 배율/폰트 확인. EBS_SB SDF가 이미 로드되어 있으면 재사용하며, 없으면 TMP 기본 폰트를 사용한다.
- Unity의 실제 Sprite import, Canvas 좌표, frame-end Destroy/pool 해제는 정적 컴파일·모형 검증이 대체하지 못한다. 이 항목들은 아직 실플레이 미검증이다.

## Rollback / 제한

`receipt.json`은 현재 변경/신규 파일의 SHA와 검사 결과다. `p2-only.diff`는 이번 텍스트 변경만 포함하며 PNG는 receipt의 별도 added/hash 목록에 있다. 기존 파일은 `before/`에 보존했다. rollback 전 현재 SHA 일치를 확인하고, 후속 변경이 있으면 전체 덮어쓰기 대신 hunk 재검토한다. 실제 원복·에셋 삭제는 하지 않았다.

기존 P2 밖의 durable-save/reload checkpoint 정책 확장은 이번 작업에 포함하지 않았다. scene teardown의 **현재 attempt pending 지급**은 구현했지만, 완전히 새로운 attempt에서 동일 예약을 재생하지 않는 영속 세이브 스키마를 새로 만들었다고 주장하지 않는다.
