# ImageGen Character Single-Image Generation Prompt

## Prompt

```text
current generated_media_prompt_v3 캐릭터 단일 이미지 record 하나를 검증하고 저장된 prompt를 ImageGen에 변경 없이 제출해줘.

참조 가이드:
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md

Input:
- planningHandoffFile: {generated_media_planning_handoff_v2_path}
- generationHandoff: {exact_generated_media_generation_handoff_v2_from_authoring}
- executionMode: promotable_generation_v2 | hosted_builtin_preview_v1
- providerExecutionApproval: required only for promotable_generation_v2
- hostedPreviewApproval: optional manual exact current authenticated single-image approval for hosted_builtin_preview_v1
- hostedPreviewAutoApprovalPolicy: optional standing automatic policy for hosted_builtin_preview_v1; exactly one of manual approval or policy is required

작업:
1. authority commit을 pin하고 closed generationHandoff의 promptRecordId와 JSON/Markdown/index path/hash를 exact
   bytes에서 다시 계산하고 closed index entry, prompt payload projection, provider=imagegen,
   assetType=character_single_image, snapshot, identity lock,
   single-image/background/outline/anchor readiness를 검증한다. CRLF/LF를 정규화하거나
   handoff의 caller summary를 신뢰하지 않는다. 같은 authority/input에 대한 full guide/payload
   reread는 한 번만 수행하고, path/hash/decision만 보고하는 task-local preflight receipt를 만든다.
   immutable JSON/Markdown/guide 본문을 commentary나 handoff에 다시 붙여 넣지 않는다.
2. provider capability를 읽기 전에 immutable prompt의 selected expression payload/hash, visualEvidenceMap, scenePromptOriginal을 다시 검증한다. animation-ready minimal profile이면 4.25 heads 초과 또는 7-8등신/영웅적 장신 허용, dense realistic detail·비늘·리벳·조밀한 주름·해칭·microtexture·modeled shading 허용, gradient·cinematic/physical lighting·realistic material·2개 초과 accent hue 허용 중 하나라도 있으면 각각 character_generation_proportion_gate_failed, character_generation_detail_density_gate_failed, character_generation_color_value_gate_failed로 차단하고 providerCalled=false/submitCount=0/cost=0을 반환한다. prompt를 수정하지 않는다.
   sparse-ink profile이면 35-45% omission, <=18% pigment area, 4-7 accents, exact palette, no-fill/negative-space, 3.75-4.25 heads와 identity anchor projection을 확인한다. omission 범위 위반은 character_generation_sparse_omission_budget_gate_failed, accent 범위·area·opaque/cel fill·off-palette 위반은 character_generation_sparse_pigment_budget_gate_failed, closed/fully-inked contour와 identity drift는 각각 중앙 8.4의 contour/identity token으로 차단한다.
   `projectbs_character_bold_outline_compressed_detail@1.0.0`이면 immutable payload, visualEvidenceMap, planning-bound projection과 scenePromptOriginal에서 head count 4.0-5.0, exact outside outline 16-22 source px, external/internal ratio >=2, facial total/component mark maxima, compressed-detail forbidden set, exact primary/optional-secondary hue anchors, coverage <=35%, masses <=4, neutral outline/weapon colors를 각각 재검증한다. 어느 closed field나 evidence가 없거나 output intent가 범위를 허용하면 중앙 8.4의 exact `character_generation_bold_outline_*_gate_failed` token으로 provider access 전에 차단한다. prompt를 수정·보충하지 않는다.
   `projectbs_character_bold_outline_compressed_detail@2.0.0`이면 inherited proportion/hierarchy/face gates와 함께 total/internal/fold <=64/56/5, optional ochre approved site classes, exact disabled-or-enabled halo를 재검증한다. enabled halo는 opacity 0.08-0.35, coverage 1-45, centered soft monotonic fade to edge alpha zero, no scene/opaque background/shadow/directional shadow여야 한다. detail/color/halo 위반은 각각 exact `character_generation_bold_outline_v2_*_gate_failed` token으로 capability/provider access 전에 차단한다.
   `projectbs_character_open_ink_wash_dynamic_contour@1.0.0`이면 4-5 heads/4.25 target 및 young-adult/no-child, 35-55 omission/45 target과 tactile mok-seon phases/directional weight, broad bleeding/misaligned watercolor-pastel 및 separate three-role palette와 두 70% negative-space floor, removable warm-ivory/no-halo/no-vignette/no-scene/no-shadow, exact Korean/Joseon identity/equipment anchors, accepted-reference semantic-transfer prohibition을 immutable payload/evidence/prose에서 각각 재검증한다. 위반은 중앙 8.4의 해당 `character_generation_open_ink_wash_*_gate_failed`로 capability/provider access 전에 차단하고 providerCalled=false/submitCount=0/cost=0을 반환한다.
   `projectbs_character_open_ink_wash_dynamic_contour@2.0.0`이면 위 여섯 pre-submit gate에 surfaceDetailContract를 추가한다. provider prose가 modeled realistic face, individual armor plates/scales/rivets/lacing/fasteners, garment microfold, microtexture, modeled light 또는 realistic material을 허용하면 `character_generation_open_ink_wash_v2_surface_detail_gate_failed`로 no-call 차단한다. v2 planning selection/key/hash가 없으면 v1을 v2로 해석하지 않는다.
2a. durable role=style_only binding이 있으면 exact six fields, asset/review/index raw hashes, purpose/status/profile scope와 prohibited transfer를 재검증하고 preview scope에 전체 object를 bind한다. callable surface가 distinct style-reference role을 제공하지 않거나 generic/identity image input만 제공하면 capability/unknown-setting blocker로 provider access 전에 중단한다. reference subject의 person/identity/pose/action/clothing/equipment를 prompt에 보충하지 않는다.
3. promotable_generation_v2이면 기존 contract 6.1-6.2 descriptor/settings/cost/approval 계약을 변경 없이 수행한다. descriptor가 없으면 기존 blocker로 중단한다.
4. hosted_builtin_preview_v1이면 contract 6.1.1의 manual exact-one-image approval 또는 authenticated standing automatic policy 중 정확히 하나를 검증한다. policy branch는 final settings seal과 request/content/prompt/reference hash를 먼저 확정하고 exact-scope attestation을 파생한다. 추가 사용자 메시지를 요구하지 않지만 policy 범위를 넓히거나 submitCount=1/retry=0을 변경하지 않는다. hidden default/descriptor/evidenceRef/cost를 만들지 않고 unavailable을 기록한다. `canvas`, `generationBackground`, `outputFormat` 각각이 callable surface의 exact same-value control로 노출되지 않으면 prompt text나 hosted default로 대신하지 않고 `hosted_preview_unknown_setting`으로 차단한다. removable solid generation background와 transparent-final/background-removal semantics를 동시에 요구하면 preview가 downstream 변환을 소유하지 않으므로 `hosted_preview_prompt_stage_semantics_conflict`로 차단한다.
5. submit 직전에는 task-local preflight receipt의 authority/request/work-unit/prompt JSON·Markdown·payload/settings/reference hash와 current submit/retry state만 다시 읽는다. drift가 없으면 앞선 closed-schema/profile/six-gate 결과를 재사용하고 full guide·record·prompt를 다시 출력하거나 full semantic pass를 반복하지 않는다. drift가 있으면 receipt를 폐기하고 exact blocker를 반환하거나 fresh full pass 하나를 수행한다. preview는 built-in_imagegen으로 한 번만 제출하고 observable output을 preview 전용 상대 경로에 저장·hash하여 generated_media_hosted_preview_record_v1만 작성한다. preservation handoff를 만들지 않는다. open-ink v2 preview이면 저장된 observable output에 대해 proportion_age, contour_mok_seon, surface_detail, pigment_palette_negative_space, background, identity_equipment, reference_role 순서의 closed non-scoring triage를 수행하고 `generated_media_profile_conformance_receipt_v1`을 response로 한 번 반환한다. visible fail은 matching `character_preview_open_ink_wash_v2_*_nonconformant`, evidence 부족은 `character_preview_open_ink_wash_v2_evidence_insufficient`이다. pass가 아니면 status를 complete/final로 쓰지 않고 nextStep=stop_no_retry_not_final로 끝낸다. submitCount/retryCount를 늘리거나 retry/edit/evaluation/preservation/promotion을 실행하지 않는다.
6. promotable mode만 deterministic idempotencyKey와 generated_media_generation_v2/costEvidence/character_single_image_v2 preservation handoff를 사용한다.
7. PixelLab fallback, download, 변환, packaging, evaluation, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status / executionMode / generationRecordId 또는 previewRecordId / submitCount / result refs / costKnown
- provider=imagegen / structureProfile=character_single_image_v2
- authority SHA와 receipt reuse 여부, compact hash/decision summary; immutable full payload 재전송 금지
- nextStep: preservation_packaging | preview_complete_no_downstream | no_downstream | stop_no_retry_not_final

실패 시 Output:
- status: blocked | failed
- failureType: contract 8.1/8.4의 기존 generation token 또는 contract 6.1.1의 exact hosted-preview token 하나
- providerCalled / submitCount / costKnown / applicable evidence status / requiredDecision / safeToRetry

검증:
- 8-way, rotation result, download/package/evaluation 결과를 만들지 않아야 한다.
- generation record에는 provider provenance와 preservation handoff만 있어야 한다.
- preview record에는 preview_only/not_promotable/not_evaluated와 unavailable evidence가 있고 preservation/evaluation/promotion 입력이 없어야 한다.
- open-ink v2 preview는 seven-gate compact receipt가 있어야 하고 seven pass만 preview_conformant_no_downstream이며 fail/insufficient 결과는 complete/final wording, retry 또는 downstream을 허용하지 않아야 한다.
```
