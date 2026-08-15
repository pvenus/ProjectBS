# ImageGen Character Single-Image Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 캐릭터 단일 이미지 record 하나를 검증하고 저장된 prompt를 ImageGen에 변경 없이 제출해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md

Input:
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- generationHandoff: {exact_generated_media_generation_handoff_v2_from_authoring}
- executionMode: promotable_generation_v2 | hosted_builtin_preview_v1
- providerExecutionApproval: required only for promotable_generation_v2
- hostedPreviewApproval: optional manual exact current authenticated single-image approval for hosted_builtin_preview_v1
- hostedPreviewAutoApprovalPolicy: optional standing automatic policy for hosted_builtin_preview_v1; exactly one of manual approval or policy is required

작업:
1. closed generationHandoff의 promptRecordId와 JSON/Markdown/index path/hash를 exact
   bytes에서 다시 계산하고 closed index entry, prompt payload projection, provider=imagegen,
   assetType=character_single_image, snapshot, identity lock,
   single-image/background/outline/anchor readiness를 검증한다. CRLF/LF를 정규화하거나
   handoff의 caller summary를 신뢰하지 않는다.
2. provider capability를 읽기 전에 immutable prompt의 selected expression payload/hash, visualEvidenceMap, scenePromptOriginal을 다시 검증한다. animation-ready minimal profile이면 4.25 heads 초과 또는 7-8등신/영웅적 장신 허용, dense realistic detail·비늘·리벳·조밀한 주름·해칭·microtexture·modeled shading 허용, gradient·cinematic/physical lighting·realistic material·2개 초과 accent hue 허용 중 하나라도 있으면 각각 character_generation_proportion_gate_failed, character_generation_detail_density_gate_failed, character_generation_color_value_gate_failed로 차단하고 providerCalled=false/submitCount=0/cost=0을 반환한다. prompt를 수정하지 않는다.
   sparse-ink profile이면 35-45% omission, <=18% pigment area, 4-7 accents, exact palette, no-fill/negative-space, 3.75-4.25 heads와 identity anchor projection을 확인한다. omission 범위 위반은 character_generation_sparse_omission_budget_gate_failed, accent 범위·area·opaque/cel fill·off-palette 위반은 character_generation_sparse_pigment_budget_gate_failed, closed/fully-inked contour와 identity drift는 각각 중앙 8.4의 contour/identity token으로 차단한다.
   `projectbs_character_bold_outline_compressed_detail@1.0.0`이면 immutable payload, visualEvidenceMap, planning-bound projection과 scenePromptOriginal에서 head count 4.0-5.0, exact outside outline 16-22 source px, external/internal ratio >=2, facial total/component mark maxima, compressed-detail forbidden set, exact primary/optional-secondary hue anchors, coverage <=35%, masses <=4, neutral outline/weapon colors를 각각 재검증한다. 어느 closed field나 evidence가 없거나 output intent가 범위를 허용하면 중앙 8.4의 exact `character_generation_bold_outline_*_gate_failed` token으로 provider access 전에 차단한다. prompt를 수정·보충하지 않는다.
3. promotable_generation_v2이면 기존 contract 6.1-6.2 descriptor/settings/cost/approval 계약을 변경 없이 수행한다. descriptor가 없으면 기존 blocker로 중단한다.
4. hosted_builtin_preview_v1이면 contract 6.1.1의 manual exact-one-image approval 또는 authenticated standing automatic policy 중 정확히 하나를 검증한다. policy branch는 final settings seal과 request/content/prompt/reference hash를 먼저 확정하고 exact-scope attestation을 파생한다. 추가 사용자 메시지를 요구하지 않지만 policy 범위를 넓히거나 submitCount=1/retry=0을 변경하지 않는다. hidden default/descriptor/evidenceRef/cost를 만들지 않고 unavailable을 기록한다.
5. submit 직전에 모든 hash와 reference role을 재검증한다. preview는 built-in_imagegen으로 한 번만 제출하고 observable output을 preview 전용 상대 경로에 저장·hash하여 generated_media_hosted_preview_record_v1만 작성한다. preservation handoff를 만들지 않는다.
6. promotable mode만 deterministic idempotencyKey와 generated_media_generation_v2/costEvidence/character_single_image_v2 preservation handoff를 사용한다.
7. PixelLab fallback, download, 변환, packaging, evaluation, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status / executionMode / generationRecordId 또는 previewRecordId / submitCount / result refs / costKnown
- provider=imagegen / structureProfile=character_single_image_v2
- nextStep: preservation_packaging | preview_complete_no_downstream

실패 시 Output:
- status: blocked | failed
- failureType: contract 8.1/8.4의 기존 generation token 또는 contract 6.1.1의 exact hosted-preview token 하나
- providerCalled / submitCount / costKnown / applicable evidence status / requiredDecision / safeToRetry

검증:
- 8-way, rotation result, download/package/evaluation 결과를 만들지 않아야 한다.
- generation record에는 provider provenance와 preservation handoff만 있어야 한다.
- preview record에는 preview_only/not_promotable/not_evaluated와 unavailable evidence가 있고 preservation/evaluation/promotion 입력이 없어야 한다.
```
