# Generated Media Pipeline Orchestration Prompt

## Prompt

```text
현재 ProjectBS Generated Media pipeline 한 건의 repository/task setup과 compact authority relay만 조정해줘. planning/routing/authoring/generation/preservation/evaluation artifact 작업은 각 공식 role에 남겨둔다.

참조 가이드:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md

Input:
- repo: {canonical origin remote URL}
- pipelineRunId: {one bounded run identity}
- requestedRoleSequence: {planning, routing_authoring, generation, preservation_evaluation 중 필요한 순서}
- existingTaskStates: {client task/officialThreadId/status compact inventory}
- cleanupAuthorization: {absent by default; exact targets only when explicitly authorized}

작업:
1. canonical repo의 `gmsetup1.{repo SHA-256 prefix}` repository setup mutex를 획득한다. worktree add/remove/prune/fetch mutation은 mutex 안에서 직렬로 한 건씩만 수행한다.
2. pipeline run당 `fetch origin main --prune`를 정확히 한 번 실행하고 fetched origin/main을 exact 40-hex commit으로 확정한다.
3. `{schemaVersion=generated_media_pipeline_authority_receipt_v1, repo, originMain, fetchedAt}`의 RFC 8785 JCS SHA-256을 authorityReceiptSha256으로 추가해 closed response-only receipt를 만든다.
4. read-only downstream role에는 receipt와 exact detached commit만 전달하고 source repo fetch를 요청하지 않는다. record/index mutation, Git publication, provider submit 경계의 기존 fresh 검사는 생략하지 않는다.
5. planning, routing_authoring, generation, preservation_evaluation의 persistent serial worktree를 우선 재사용한다. micro-stage마다 새 worktree를 만들지 않는다.
6. queued client task는 distinct officialThreadId가 생길 때까지 기다린다. pending/queued/setup 중 replacement task를 만들지 않는다. original 상태가 abandoned 또는 failed로 확정된 뒤에만 setup retry 최대 1회를 허용하며 retry는 새 distinct officialThreadId를 받아야 한다.
6a. `client-new-thread` setup 실패가 officialThreadId를 받지 못하면 `set_thread_archived` 대상이 아니며 orphan UI card로 남을 수 있다. 새 client card를 만들어 retry하지 말고 persistent role worktree와 이미 존재하는 동일 official task를 사용한다. 기존 orphan card 정리는 Codex app/platform 책임이며 repository worktree/file 삭제로 처리하지 않는다.
7. sealed hash-bound evaluation package의 평가는 source Git worktree 밖 evaluation workspace에서 실행한다. evaluation role은 source repository를 fetch하지 않는다.
8. archived/dirty/detached/partially-configured/unpublished worktree는 inventory-only다. exact cleanupAuthorization 없이는 remove/prune/delete하지 않는다.
9. setup 실패는 worktree_metadata_permission_denied | task_registry_collision | helper_setup_refresh_failed | tool_approval_required 중 정확히 하나로 반환한다. 실패가 cleanup, 추가 fetch 또는 replacement task를 승인하지 않는다.
10. state change와 terminal에서만 compact status 한 건을 반환한다. full authority bundle, planning payload, unchanged inventory와 동일 status를 재전송하지 않는다.
11. provider 호출, media 생성/수정, planning/routing/prompt/record/index 작성, preservation/evaluation/promotion/Unity를 수행하지 않는다.
12. sealed preservation package의 평가가 끝난 뒤에만 GeneratedMediaRequestRoutingGuide의 terminal project-promotion dispatch를 판정한다. `generated_image_evaluation_v1 + evaluationStatus=completed + result=PASS + passForProjectCopy=true + promotionStatus=not_promoted`와 exact current package registry row가 모두 맞을 때만 persistent official task `01a01094-7d22-7a51-b92e-bf6154769017`에 정확히 한 번 dispatch한다.
13. relay는 requestId,evaluationPackageId,assetType,domainType,contentId,evaluationRecordId,replaceExisting,replacementApprovalRef 여덟 member만 갖는다. source/target 절대·상대 경로, full bundle/manifest, prompt/provider payload, media와 unknown/nested member를 넣지 않는다. exact relay JCS hash를 response-only active/completed key로 확인하되 key를 relay에 추가하거나 저장소 record/index/path를 만들지 않는다.
14. preview/notEvaluated/incomplete/non-PASS/Conditional Pass/Fail/missing package/false 또는 missing passForProjectCopy/not_promoted 이외 promotionStatus는 dispatch하지 않는다. promotion child final은 promoted | blocked | not_promoted | copy_failed 중 하나로 terminal 처리하고 routing/generation/preservation/evaluation로 되돌리지 않는다.

Output:
- status: ready | queued | blocked
- pipelineRunId
- officialThreadIds: role별 distinct official ID 또는 pending
- authorityReceipt: generated_media_pipeline_authority_receipt_v1 (ready에서 required)
- setupMutexKey / mutexState: released | queued
- reusedPersistentWorktrees: role 이름만 포함한 ordered list
- setupRetryCount: 0 | 1
- failureType: blocked에서만 exact setup token
- cleanupPerformed: false (이 prompt는 cleanup을 실행하지 않음)
- nextRole / compactStatusHash
- providerCalled=false / submitCount=0
- promotionTerminalStatus: promoted | blocked | not_promoted | copy_failed (final-stage를 판정한 경우)
- projectPromotionDispatchPerformed: false | true
```
