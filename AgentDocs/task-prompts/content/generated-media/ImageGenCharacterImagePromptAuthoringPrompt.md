# ImageGen Character Single-Image Prompt Authoring Prompt

## Prompt

```text
현재 ProjectBS 저장소에서 current v2 캐릭터 단일 이미지 routing record와 planning handoff를 검증하고 ImageGen provider-ready prompt record 하나만 작성해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/prompt/PromptAuthoringGuide.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
- AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaTransparentForegroundAuthoringGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md

Input:
- routingRecordFile: {project_relative_generated_media_routing_v2_record}
- planningHandoffFile: {project_relative_generated_media_planning_handoff_v2}
- required: request/content/source/snapshot identity, requiredElements, prohibitedElements, identityConsistencyLock, singleImageSpecification, conditional transparentForegroundSelection

작업:
0. exact noninteractive execution policy 범위의 read/hash/schema/test와 bounded prompt record/index write는 재승인 없이 수행한다. host-required bundled approval은 coordinator의 한 건만 재사용하고 새 권한 경계에서는 partial write 없이 차단한다.
1. registryVersion=v2, provider=imagegen, assetType=character_single_image, exact registry row와 snapshot/hash를 검증한다.
2. viewpoint, pose, framing, canvas, targetDisplaySize, safeArea, background, generationBackground, noShadow, outline, pelvis/root와 ground-contact anchor를 모두 검증한다.
3. 승인 기획을 provider-neutral visual brief로 정규화하고 모든 문장을 evidence/constraint에 연결한다. 캐릭터 사실과 ProjectBS 캐릭터 표현 profile을 분리하고 누락된 외형·시점·색·배경을 만들지 않는다.
4. GeneratedMediaVisualPromptAuthoringGuide의 canonical expressionProfilePayload를 exact 사용하고 RFC 8785 JCS canonical JSON UTF-8 bytes의 SHA-256을 다시 계산해 등록된 expressionProfilePayloadHash와 비교한다. registered lock-array profile이면 non-empty positiveStyleLock/negativeStyleLock의 배열 순서와 evidence를 보존한다. sparse-ink profile이면 두 compatibility lock array를 empty로 유지하고 정확히 여덟 policy member를 닫힌 형태로 검증해 각 budget/policy를 evidence에 연결한다. sparse에는 missing_*_style_lock을 적용하지 않는다. `stylized` 한 단어로 대체하지 않는다.
5. animation-ready minimal profile은 exact approved planning fact가 정확한 key를 선택했을 때만 사용한다. closed proportionProjection/detailDensityBudget/colorValueBudget/authoringProjectionContract를 검증하고, planning-bound 비례가 3.75-4.25 heads 및 head 24-27% 안의 exact 또는 더 좁은 범위인지, 짧고 단순화된 팔다리·animation-safe low detail·최소 flat value masses·차분한 accent hue 1-2개가 모두 독립 observable statement와 exact evidence로 투영됐는지 확인한다. 누락, 범위 초과, budget 누락, evidence 누락은 current contract의 exact typed blocker로 중단한다.
5a. `projectbs_character_bold_outline_compressed_detail@1.0.0`은 exact approved planning selection과 closed `expressionProfileProjection`이 모두 있을 때만 사용한다. 4.0-5.0 heads, 1024x1536 source의 outside outline 16-22px, exact internal-line thickness와 external/internal ratio >=2, closed facial mark budget <=9 및 component maxima, identity-first compressed detail budget, primary/optional secondary hue와 exact anchor sites, coverage 1-35%, color masses 1-4, neutral outline/weapon colors를 검증한다. secondary hue/anchors는 jointly present/absent여야 한다. 모든 값과 lock은 독립 observable statement, exact planning/profile evidence, provider prose에 직접 포함되어야 하며 prompt wording으로 누락 field를 보충하지 않는다.
5b. `projectbs_character_bold_outline_compressed_detail@2.0.0`이면 v1 공통 비례/outline/face/color 검증에 더해 exact detailMarkBudget(동일 counting unit, total <=64, internal <=56 및 <=total, garment-region fold <=5), optional ochre의 exact element와 `small_utility_pouch|small_travel_accessory` site class, closed `inkHalo` union을 검증한다. disabled는 정확히 `{enabled:false}`이며 dark background를 허용하지 않는다. enabled는 dark-neutral color, opacity 0.08-0.35, coverage 1-45, centered soft extent, monotonic zero-alpha edge와 no-scene/no-opaque/no-shadow/no-directional-shadow를 모두 exact evidence와 provider prose에 포함한다. 누락·범위 초과·lock/evidence 누락은 중앙 contract의 exact `bold_outline_v2_*` blocker로 중단하고 accepted PNG에서 값을 추론하지 않는다.
5c. `projectbs_character_open_ink_wash_dynamic_contour@1.0.0`이면 exact eleven policy members와 ordered 7+7 locks를 검증한다. 4-5 heads/target 4.25, young adult/no child, contour omission 35-55%/target 45%, pressure-variable tactile mok-seon의 brush start/directional drag/dry end/directional weight, broad rough watercolor/pastel의 controlled bleed 및 outline 밖 misalignment, 분리된 faded blue-gray-or-indigo/dusty gray-brown/small muted-ochre role, figure interior와 full canvas 각각 achromatic/unpainted >=70%, removable warm-ivory solid, no halo/vignette/scene/shadow, exact Korean/Joseon identity/equipment anchor를 모두 독립 evidence와 prose에 포함한다. sticker-clean/uniform/vector contour, clean cel fill, decorative small splash를 금지한다. accepted SHA는 audit-only이며 durable project-relative style-only publication 전에는 referenceBindings/path/identity/edit target으로 사용하지 않는다. 누락·mismatch·evidence/prose/reference-role 위반은 중앙 contract의 exact `open_ink_wash_*` authoring token으로 prompt publication 전에 차단한다.
5d. 새 planning revision이 `projectbs_character_open_ink_wash_dynamic_contour@2.0.0`을 exact 선택한 경우에만 exact nineteen members와 ordered 9+9 locks를 사용한다. v1 record를 변환하지 않는다. 기존 open-ink 제약에 더해 full-body head-count measurement, realistic modeled face와 individual armor plate/scale/rivet/lacing/fastener·garment microfold·microtexture·modeled light를 금지하는 surfaceDetailContract, seven ordered post-output gate와 compact receipt contract를 exact brief/evidence에 투영한다. removable-solid branch는 기존 spatially uniform #F2EFE6/no radial-or-edge-darkening background를 byte/meaning 변경 없이 유지한다. transparent selection branch는 profile payload를 변경하지 않고 5g의 reviewed execution-background composition을 적용한다. provider prose에는 아래 5f/6a/6c의 exact executable projection만 들어가며 receipt/review/post-output workflow 문구는 넣지 않는다. 누락·mismatch·evidence/prose/reference-role 위반은 중앙 contract의 distinct `open_ink_wash_v2_*` authoring token으로 publication 전에 차단한다.
5e. routing record의 top-level `styleReferenceBindings`가 있으면 normalizedRequest와 `/authoringHandoff`의 top-level projection이 exact byte-semantic match이고 typeSpecification에는 absent인지 먼저 검증한다. 이후 asset/review/index canonical path와 raw hash, deterministic record ID/payload, purpose=style_only, selected profile key/hash, prohibited transfer 전체를 재검증한다. exact six-member binding을 visual brief와 prompt record에만 복사하고 scenePromptOriginal에는 reference 사람/pose/action/clothing/equipment나 record/path/hash를 쓰지 않는다. 누락·drift·3-member style binding·absolute path·identity/edit role·nested/unequal projection은 중앙 `style_reference_*` token으로 prompt publication 전에 차단한다.
5f. open ink-wash v2 신규 authoring은 fresh fetched commit의 raw Git blob만 hash-significant authority로 사용하고 GeneratedMediaVisualPromptAuthoringGuide의 `Deterministic open ink-wash v2 authoring projection`을 그대로 실행한다. exact 10 required/14 prohibited slot, brief field mapping, evidence ordering, 19-member profile, 9+9 locks와 six-member binding을 닫힌 값으로 검증한다. checkout CRLF를 정규화하거나 자유 prose/요약/번역/heading을 추가하지 않는다. 동일 raw authority에서 두 projection의 visual brief, 28-line provider text, payload/record/Markdown/index-after/handoff bytes 또는 ID/hash가 하나라도 다르면 `record_identity_mismatch`, no-write, `safeToRetry=false`로 중단한다.
5g. top-level `transparentForegroundSelection`이 있으면 routing/normalizedRequest/authoringHandoff/planning handoff의 exact object와 `generationBackground={mode:transparent}`를 검증한다. color, removable-solid branch, unknown member 또는 stale opaque/removable/warm-ivory required element가 있으면 각각 `true_alpha_branch_conflict`, `true_alpha_projection_mismatch`, `transparent_prompt_required_element_conflict` 중 exact token으로 no-write 차단한다. fresh corrected route만 visualBrief/prompt record/hash payload/providerSettingsIntent/index entry/detached handoff에 selection을 byte-semantically 투영한다.
6. 한 승인 시점의 cohesive ImageGen prompt 하나에 선택 profile의 complete provider projection과 planning-bound exact 값을 직접 포함하고 settings intent를 분리한다. sparse-ink profile은 main omission 35-45%, pigment area <=18%, 4-7 accents, exact two-color palette, loose bloom/rub/dragged stroke, no-fill/negative-space gate, darkest identity anchors와 3.75-4.25 heads/short limbs를 포함한다. photographic/3D/PBR, closed coloring-book/vector/cel fill, fully inked silhouette, uniform line, dense hatching/modeled shading, off-palette hue와 7-8등신을 허용하지 않는다. 8-way, rotation, ordered_rotation_set을 넣지 않는다.
6a. open ink-wash v2는 exact 10 required statement, 9 negative-lock statement, 9 positive-lock statement만 그 순서로 LF join하여 28-line `scenePromptOriginal`을 만든다. heading, blank line, final gestalt, prohibited/audit/receipt transcript 또는 author-written 문장을 추가하지 않는다. 다른 lock-array profile은 `scenePromptOriginal`을 evidence transcript가 아닌 provider 실행문으로 조립한다. 첫 hard-output block에 planning-bound 비례, silhouette/contour, submitted generation background를 두고, identity/equipment, line/pigment/palette/negative-space 순으로 한 번씩만 설명한다. ordered negative lock과 ordered positive lock의 exact statement는 각각 normative order로 정확히 한 번만 넣고, 마지막에는 lock 문장을 복사하지 않는 짧은 measurable gestalt check 하나만 둔다. 같은 primary numeric/prohibition concept는 hard instruction 또는 exact lock과 optional final check를 합쳐 최대 두 번만 나타나야 한다.
6b. provider prose에 SHA/hash, record/routing ID, evidence path, authority/workflow/provider label, `APPROVED ... STATEMENTS`/`ORDERED ... LOCKS` heading, bilingual duplicate를 넣지 않는다. `generationBackground.mode=removable_solid`이면 provider에는 approved solid color를 edge-to-edge uniform하게 생성하고 luminance falloff/dark corner/radial gradient/halo/vignette/scene/shadow를 금지하는 현재 generation target만 쓴다. 이 legacy branch에는 `transparent final` 또는 background-removal/packaging instruction을 섞지 않는다. transparent branch는 solid color나 background-removal stage를 추가하지 않고 exact true-alpha output requirements만 사용한다. armor/equipment identity는 simplified interrupted mass로 표현하고 금지된 repeated plate/scale/rivet, dense fold, microtexture, modeled material detail을 되살리지 않는다. exact lock 중복, meta/evidence leakage, background-stage 혼합, 의미 우선순위 약화는 `provider_value_invalid`로 prompt publication 전에 중단한다.
6c. transparent branch의 `scenePromptOriginal`은 corrected requiredElements 전부를 source order로 둔 다음 ordered 9 negative locks와 ordered 9 positive locks를 LF-join한다. 단 `char_open_wash_v2_negative_halo_scene_shadow`와 `char_open_wash_v2_positive_identity_on_ivory`는 TransparentForegroundAuthoringGuide의 exact true-alpha provider 문장으로 constraintId 기반 치환하고, 나머지 16개 lock은 verbatim 유지한다. profile payload/hash는 바꾸지 않는다. heading/blank/summary/synonym을 추가하지 않으며 raw Git blob에서 두 번 독립 projection한 visual brief/settings/payload/record/Markdown/index/handoff bytes와 IDs/hashes가 모두 같아야 한다.
7. 승인 기획과 style profile이 material conflict이면 몰래 변환하지 않고 character_style_profile_conflict로 중단한다.
8. GeneratedMediaRecordGuide.md::Prompt v3의 closed
   generated_media_prompt_hash_payload_v3를 source record에서 exact projection하고
   RFC 8785 JCS UTF-8 SHA-256, deterministic ID/path, closed nested field sets를 검증한다.
9. scenePromptOriginal의 LF-only/no-BOM/exactly-one-terminal-LF raw Markdown bytes와
   hash를 먼저 계산한다. closed generated_media_prompt_v3 JSON/Markdown/closed
   generated_media_prompt_index_v3를 same-scope lock, no-clobber, CAS, rollback 규칙으로
   게시한다. unknown/missing field, hash mismatch, collision, invalid/dangling index는
   덮어쓰거나 정규화하지 않는다. exact existing triplet 또는 recoverable exact
   JSON+Markdown orphan만 검증해 reused_identical로 재사용한다.
10. 게시된 JSON/Markdown/index exact bytes를 다시 읽고 세 raw file hash를 포함하는
   closed detached generated_media_generation_handoff_v2와 그 canonical hash를 반환한다.
11. ImageGen 호출, download, packaging, evaluation, promotion, Slack, Unity, Git을 수행하지 않는다.

Output:
- status: ready_for_generation | reused_identical
- promptRecordId / promptPayloadSha256 / prompt record path/raw SHA-256
- prompt Markdown path/raw SHA-256 / prompt index path/raw SHA-256
- complete generated_media_generation_handoff_v2 / generationHandoffSha256
- identity / registry row / expressionProfileKey / expressionProfilePayload / expressionProfilePayloadHash / visual brief evidence coverage / profile projection coverage
- provider=imagegen / structureProfile=character_single_image_v2
- nextStep: generation

실패 시 Output:
- status: blocked
- failureType: GeneratedMediaImageGenOnlyContractGuide.md 8.1 및 8.3의 character single-image authoring 적용 token 중 정확히 하나. registered lock-array profile에는 lock token을 사용하고 sparse는 four sparse projection token만 사용한다. bold-outline profile은 그 section의 exact proportion/outline/face/detail/color/evidence/provider-projection token을, open ink-wash profile은 exact projection/evidence/provider/reference-role token을 사용한다. expression-profile identity에는 payload/key/hash missing 또는 mismatch token만 적용하며 reference-record/skill token과 alias를 사용하지 않는다. record publication에는 exact record/hash/index/Markdown/write/rollback token만 사용한다.
- missingFields / requiredDecision / safeToRetry

검증:
- provider는 imagegen 하나여야 한다.
- 캐릭터 신규 계약에 8-way/rotation set이 없어야 한다.
- planning, brief, provider prompt, settings가 분리되어야 한다.
- copy-ready prompt에 선택 profile의 complete projection이 있고 각 항목 evidence coverage가 완전해야 한다.
- copy-ready prompt는 exact lock statement를 각각 한 번만 포함하고, raw hash/path/record/workflow heading이 없으며, removable-solid generation instruction과 later transparent/background-removal responsibility를 혼합하지 않아야 한다.
- animation-ready minimal profile이면 numeric/detail/color/value projection과 planning/profile evidence coverage가 완전하고 dense material prose가 simplification lock을 약화하지 않아야 한다.
- bold-outline compressed-detail profile이면 head ratio, source/target outline projection, external/internal ratio, facial mark count/component budget, compressed-detail forbidden set, color hue/anchor/coverage/mass/neutral bindings과 모든 lock의 planning/profile evidence가 완전해야 한다.
- open ink-wash profile이면 eleven policy members, 7+7 locks, exact planning-bound identity/equipment, prompt prose, audit-only reference-role prohibition이 완전해야 한다.
- open ink-wash v2이면 nineteen members, 9+9 locks, surface-detail ceiling, uniform-background lock, seven-gate order와 compact receipt projection이 완전해야 한다.
- durable style-only binding이 있으면 planning/routing/brief/prompt의 six fields와 review/index/asset raw bytes가 exact하며 provider prose에는 reference subject semantics가 없어야 한다.
- JSON은 canonicalJson+LF, Markdown은 scenePromptOriginal UTF-8 bytes+LF이며 CRLF/BOM/추가 terminal LF가 없어야 한다.
- prompt record/index/handoff projection과 모든 ID/path/hash가 exact 재계산되어야 한다.
- 실패 시 partial/orphan file이나 generation handoff를 새로 남기지 않아야 한다.
- provider 및 후속 단계를 실행하지 않아야 한다.
- planning이 `generated_media_transparent_foreground_selection_v1`을 선택했으면 exact `generated_media_true_alpha_foreground@1.0.0` / `2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108` key/hash와 mainLock을 prompt record/handoff에 그대로 투영하고 outside-foreground alpha0, inside-only bounded partial alpha, safe margin/no clipping 및 matte/checkerboard/halo/vignette/floor/scene/cast-shadow/fringe 금지를 positive/negative lock에 포함한다. prompt prose만으로 conformance를 주장하지 않는다.
```
