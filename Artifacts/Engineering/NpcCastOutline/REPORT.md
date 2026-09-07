# 한결 통합 검토용 receipt — inactive spawn + NPC cast outline

결합 실행 범위의 정적/모형 검증 완료. 실제 Unity 플레이/셰이더 GPU 컴파일 및 시각 검증은 아직 수행하지 않았다. 사용자 완료 handoff 없이 한결에게 검토 요청한다.

## 코드 변경

- `CharacterSkillCastPresentationMono.cs`: exact ID `character.black_cloth_raider.1`, `character.chain_axe_enforcer.2` AND `CharacterType.Npc`에만 default white outline MPB lease. casting 중 red outline + red cast base/accent palette. original shared material을 수정하지 않고 기존처럼 cast용 한 개 clone을 생성/해제한다. original renderer material/MPB를 저장하고, 자기 material lease인 경우 종료 시 복원한다. 새 material owner가 들어온 경우 그 material은 유지한다. cancel/complete는 두 NPC만 즉시 복원하고, death event/OnDisable/OnDestroy/pool reconfiguration에서도 복원한다. non-NPC/Player/서진 기본값과 cast palette는 기존 동작을 유지한다.
- `CharacterSkillManager.cs`: NPC palette에서만 FireSkillImmediate 호출 이후 finally 복원. 성공/실패/예외 발사 경계를 포함한다. 기존 플레이어 completion timing은 유지한다. NPC pooled 재초기화 시에만 이전 casting lease를 취소한다. 기존 ground cone 호출 및 telegraph 소스는 변경하지 않았다.
- `CharacterManager.cs`: 두 초기화 경로에서 위 NPC defaults 설정. 기존 inactive reveal rootfix를 보존한다.
- `CharacterCastProgress.shader`: default OFF인 `_NpcCastOutlineEnabled` 분기에서 기존 body outline과 같은 8-neighbor 알파 경계를 사용한다. NPC 전용 MPB에서만 활성화된다. non-NPC에는 분기 비활성.
- 앞선 `NpcSpawnService.cs` rootfix 유지: inactive data initialize → ownership prepare → parent detach/activation → runtime 검증 → sequence/living register. 예외/거절이면 registration 철회 및 폐기.

## 검사 결과

- Unity 실제 Roslyn + 프로젝트 참조: 0 errors / 기존과 같은 31 warnings.
- production cast presenter 모형: 4/4. black/chain 각각 white→red→complete/cancel/death/disable, 20회 pooled reconfigure, shared material shader 불변, custom MPB 보존, material allocation 회수, Player/other NPC 비영향, newer material ownership 보존.
- post-fire finally 순서 및 shader default-OFF 경계 검사 통과.
- inactive spawn actual-service/lifecycle 모형: 8/8. black/chain inactive 및 pooled reuse, reveal 중복0, 일반 active spawn, init/prepare/registry observer 실패 등록 철회.
- MORPG live glue 7/7, P0/P1 27/27, deterministic trace SHA 유지.
- 변경 대상 git diff --check 통과.
- Unity GUI, staging, commit, push 미사용. shared NPC material asset과 ground telegraph source 미수정.

## 검토·런타임 확인 요청

1. 두 NPC 기본 outline white 및 cast 중 red outline/red body cue를 실제 셰이더로 확인.
2. 발사 직후 white 복원, target loss/interruption/cancel/death/disable/pool reuse 잔류0 확인.
3. 서진/Player/other NPC와 ground cone 기존 표현 유지 확인.
4. inactive coroutine 오류0 및 19→5→4 진행 회귀 확인.
5. 모형의 Destroy는 즉시 해제로 검증하므로 Unity 지연 Destroy 이후 frame-end material 수는 실제 플레이에서 확인 필요.

## 재현 / rollback 자료

- `python3 AgentTools/MorpgIntegrationHarness/compile.py`
- `python3 AgentTools/MorpgIntegrationHarness/cast_outline_tests.py`
- `python3 AgentTools/MorpgIntegrationHarness/inactive_spawn_tests.py`
- `sh AgentTools/MorpgIntegrationHarness/run.sh`
- `sh Artifacts/Engineering/MorpgP1Rollback/revision-02/REPRODUCE.sh`

outline 변경만의 before snapshot: `Artifacts/Engineering/NpcCastOutline/before/` (inactive rootfix 포함).
outline 역적용 검토용 patch: `outline-only.diff`. 원복은 실행하지 않았다. 실행 전 `receipt.json`의 현재 파일 SHA 일치 여부를 확인하고, 일치하지 않으면 다른 작업자의 후속 변경을 덮어쓰지 말고 hunk 단위로 재검토한다. rootfix까지 원복해야 할 경우 outline 원복 검토 후 `Artifacts/Engineering/MorpgInactiveSpawnFix/before/`와 `fix-only.diff`를 별도로 검토한다. baseline 파일 전체 복사로 후속 수정본을 덮어쓰지 않는다.

원래 MORPG 보고서의 P2 월드 pickup 및 durable-save/reload 복구 미연결 제한은 이번 색상/rootfix 작업으로 해소된 것으로 간주하지 않는다.
