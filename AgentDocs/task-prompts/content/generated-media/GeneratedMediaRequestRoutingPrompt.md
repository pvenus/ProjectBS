# Generated Media Request Routing Prompt

외부 Generated Media 기획 handoff 하나를 검증하고 정확히 하나의 provider
prompt-authoring pipeline으로 라우팅하는 단일 진입점 프롬프트입니다.

## Prompt

```text
현재 ProjectBS 저장소에서 외부 Generated Media 기획 handoff 하나를 검증·정규화하고 정확히 하나의 provider prompt-authoring pipeline으로 route해줘. provider prompt를 작성하거나 선택한 prompt를 실행하지 마.

참조 가이드:
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md

Input:
- planningHandoffFile: {project_relative_canonical_or_compat_generated_media_planning_handoff_path}
- supersedesRoutingRecordId: {optional_prior_routing_record_id}

canonical raw handoff 필수 내용:
- schemaVersion=generated_media_planning_handoff_v1
- requestId, assetType, domainType, contentId, contentUsage
- sourcePlanningFiles와 각 path/role/hash 및 optional revision
- planningSnapshot.snapshotHash와 approvedFacts
- requiredElements, prohibitedElements
- assetType에 맞는 GeneratedMediaPlanningHandoffGuide.md Section 4의 flattened type fields

허용 compatibility input:
- schemaVersion=generated_media_planning_handoff_compat_v1일 때만 artifactType, outputUsage, top-level planningRevision과 허용 specification container를 사용할 수 있다.
- alias/container 허용 목록과 canonical target은 GeneratedMediaPlanningHandoffGuide.md Section 3.2만 따른다.

작업:
1. 현재 workspace에서 planningHandoffFile을 읽어 변경하지 않은 rawInput으로 캡처하고 canonical 또는 허용 compatibility schemaVersion인지 확인한다.
2. compatibility schema이면 alias/container를 canonical raw target으로 먼저 해석한다. alias와 canonical 값 불일치 및 specification container와 대응 flattened canonical field의 불일치는 모두 compatibility_alias_conflict, 복수 canonical asset 후보는 ambiguous_asset_type, 미등록 legacy asset alias는 unsupported_asset_type으로 단일 판정한다. 이 판정은 compatibility normalization 단계에서 수행한다. canonicalHandoff와 별도 compatibilityEvidence를 가진 compatibilityNormalizedInput을 만들고 top-level planningRevision으로 source entry를 수정하지 않는다. 그 다음 required-field validation과 unknown-field rejection을 순서대로 수행한다. canonical raw input에 normalized field를 요구하지 않는다.
3. 모든 sourcePlanningFiles를 읽어 path, SHA-256, optional revision을 검증한다. planning producer의 canonical snapshot contract로 snapshotHash를 검증하고 GeneratedMediaRecordGuide.md 규칙으로 planning_hash를 재계산한다. 검증 가능한 snapshot contract가 없으면 중단한다.
4. canonical contentUsage를 normalized outputUsage로 매핑한다. 모든 source revision이 동일하면 normalized planningRevision으로 계산하고, missing/mixed/partial revision은 차단한다. compatibility top-level revision은 snapshot-covered이고 모든 source에 동일 적용될 때만 허용한다.
5. canonical requestId, assetType, domainType, contentId, contentUsage, requiredElements, prohibitedElements와 assetType별 flattened type fields의 존재·비모호성을 검증한 뒤 normalized specification container를 조립한다.
6. 누락된 required/prohibited 요소, character identity·외형·동작, icon 의미·상징, animation sequence·loop·frame·runtime boundary, ImageGen 장면·구도·카메라를 추론하거나 보충하지 않는다.
7. GeneratedMediaRequestRoutingGuide.md의 alias 규칙만 적용해 assetType/domainType/profile을 canonical enum/profile로 정규화한다. 파일명, 설명 유사도 또는 provider 가용성으로 route하지 않는다.
8. 이 router revision에 고정된 generated_media_authoring_profile_registry_v1의 exact asset/domain/profile ID/version 행을 모두 평가한다. 호출자가 registry version을 선택하거나 override할 수 없다. 정확히 한 행이면 계속하고, 0행은 failure priority table, 2행 이상은 conflicting_routing_evidence로 중단한다. conflicting_routing_evidence는 duplicate exact row 또는 독립적인 authoritative non-alias route tuple 충돌에만 사용하고 alias conflict에는 사용하지 않는다.
9. matched row의 selectedPipeline, selectedAuthoringPrompt, appliedProfile, registryVersion과 exact row ID를 기록하고 routingReason에는 enum/profile 근거만 작성한다.
10. rawInput, compatibilityNormalizedInput과 generated_media_authoring_request_v1 normalizedRequest를 분리해 보존한다. authoringHandoff.promptInput에는 선택된 기존 prompt가 실제로 받는 planningHandoffFile과 Character용 runType/animationRequestId만 기록하고, evidenceMap에 각 normalized field의 정확한 canonical raw JSON pointer를 기록한다.
11. character_animation은 source 순서의 animationRequestId별 authoringUnits를 만들되 모두 동일 PixelLab Character pipeline/prompt를 가리키게 한다. 요청되지 않은 Attack/Idle/Move를 추가하지 않고 authoring prompt를 실행하지 않는다.
12. routerVersion, registryVersion, selectedRegistryRowId, normalized route와 optional supersedesRoutingRecordId를 포함해 canonical routingHashPayload를 계산하고 gmroute.{assetType}.{contentId}.{routingHashPrefix12} ID를 만든다. registryVersion과 selectedRegistryRowId는 모두 ID identity의 필수 요소다.
13. 기존 동일 ID record를 먼저 확인한다. canonical request가 같으면 기존 bytes와 createdAt을 그대로 재사용하고, bytes가 다르면 routing_record_collision로 중단한다.
14. 신규인 경우에만 AgentDocs/planning-data/generated-media-routing/v1/{assetType}/{contentId}/{routingRecordId}.json에 generated_media_routing_v1 immutable record를 쓰고 routing_index.json을 deterministic sort로 갱신한다. completed record file SHA-256은 record 내부가 아니라 index와 handoff에 기록한다.
15. blocked 요청은 routing record와 index를 생성·수정하지 않는다.
16. 선택한 authoring prompt의 실제 경로와 입력 매핑을 검증한 뒤 nextStep=authoring으로 handoff만 반환한다.
17. provider prompt 작성·변경, PixelLab/ImageGen 호출, 다운로드·보존·패키징·평가·project promotion, Slack, Unity, Git과 배포를 수행하지 않는다.

Output:
- status: routed
- routingRecordId / Record Path / SHA-256
- Router Version / Profile Registry Version / Selected Registry Row ID
- Raw Input Schema / SHA-256 / Compatibility Applied
- Compatibility-Normalized Input Hash
- selectedPipeline
- selectedAuthoringPrompt
- assetType / domainType / contentId
- normalizedRequest
- appliedProfile
- sourcePlanningFiles
- planningSnapshotHash
- routingReason
- Authoring Handoff
- Idempotency Result: created | reused_identical
- nextStep: authoring

실패 시 Output:
- status: blocked
- failureType: missing_planning_handoff | invalid_planning_handoff | invalid_compatibility_envelope | compatibility_alias_conflict | planning_revision_conflict | missing_asset_type | ambiguous_asset_type | missing_required_elements | missing_prohibited_elements | missing_type_specification | unsupported_asset_type | invalid_domain_profile | conflicting_routing_evidence | unreadable_source_planning | planning_snapshot_mismatch | missing_planning_revision | missing_output_usage | unsupported_domain_type | routing_record_collision | routing_record_write_failed | routing_index_write_failed
- missingFields
- conflictingFields
- candidatePipelines
- requiredDecision
- safeToRetry
- 생성·수정하지 않은 routing record/index

검증:
- sourcePlanningFiles와 planning snapshot/hash가 모두 일치해야 한다.
- compatibility alias/container resolution이 required/unknown-field validation보다 먼저 실행되어야 한다.
- alias/canonical 불일치와 container/flattened canonical field 불일치, 복수 canonical 후보, 미등록 legacy alias는 각각 compatibility_alias_conflict, ambiguous_asset_type, unsupported_asset_type으로만 판정되어야 한다.
- rawInput, compatibilityNormalizedInput과 normalizedRequest를 혼용하지 않아야 한다.
- requiredElements와 prohibitedElements가 유효한 비어 있지 않은 계약이어야 한다.
- assetType에 맞는 type specification과 등록 profile이 완전해야 한다.
- canonical normalization 후 generated_media_authoring_profile_registry_v1에서 exact asset/domain/profile ID/version 한 행만 일치해야 한다.
- 0행 failureType은 asset, domain, profile 순서의 decision table로 결정해야 한다.
- normalizedRequest의 모든 material field가 immutable handoff 근거를 가져야 한다.
- selectedPipeline과 selectedAuthoringPrompt는 matched row와 정확히 일치해야 한다.
- routingHashPayload, routingRecord와 index가 동일한 registryVersion과 selectedRegistryRowId를 포함해야 한다.
- routingRecordId, canonical record path, record SHA-256와 index entry가 재계산 결과와 일치해야 한다.
- 동일 요청 재실행은 중복 record/index entry를 만들지 않아야 한다.
- blocked 결과는 record/index를 변경하지 않아야 한다.
- 단일 authoring handoff 외의 provider/generation/download/evaluation/promotion 작업을 실행하지 않아야 한다.
```
