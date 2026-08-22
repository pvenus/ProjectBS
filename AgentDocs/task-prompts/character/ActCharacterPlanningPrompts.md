# Act Character Planning Prompt

Act·Chapter 근거에서 Player/Npc/Boss 공통 canonical character planning을
작성하고, 준비가 완료된 캐릭터에 한해서 별도 Generated Media planning
handoff를 작성하는 복사용 프롬프트입니다.

## Prompt

```text
작업 폴더 = {project_root}

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
- AgentDocs/planning-guides/character/ActCharacterPlanningStartGuide.md
- AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
- AgentDocs/planning-guides/character/CharacterStatGuide.md
- AgentDocs/planning-guides/skill/design/SkillDegineGuide.md
- AgentDocs/planning-guides/skill/design/SkillBalanceGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md

Input:
- projectRoot: {project_root}
- actId: {act_id}
- actGroupId: {act_group_id}
- actStoryFile: {project_relative_act_story_file}
- overallStoryFiles:
  - {project_relative_overall_story_file}
- chapterFiles:
  - {project_relative_chapter_file}
- globalCharacterFile: AgentDocs/planning-data/story/Characters.md
- existingPlanningFiles:
  - {optional_project_relative_character_planning_file}
- createCharacterSingleImageHandoffs: {true | false}
- sourceRevisionInputs:
  - path: {optional_exact_source_path}
    revision: {optional_stable_revision_from_source_authority}
- characterExpressionProfileSelections:
  - contentId: {character_id}
    expressionProfileKey: {optional_exact_registered_key}
    expressionProfilePayloadHash: {optional_exact_registered_hash}

작업:
1. 모든 입력 파일과 직접 참조 가이드를 읽고 sourceStoryRefs와 sourcePlanningRefs를 확정한다.
2. 기존 파일은 character_planning_v2 또는 legacy로 분류하고 legacy 파일을 자동 덮어쓰지 않는다.
3. 새 Player/Npc/Boss 개별 planning은 CharacterPlanningDataGuide.md의 character_planning_v2로 작성한다. characterType과 무관하게 runtimeDomain=character를 유지한다.
4. 기존 commonDataRef, identity, combat, planningScore, stats, skills의 의미와 runtime domain 규칙을 보존한다.
5. appearance의 genderPresentation, body/face/hair, costume, equipment, weapon, handedness, palette/material, identifyingFeatures, posePolicy, intendedDisplay.targetDisplaySize/detailDensity를 story/planning 근거로만 확정한다. genderPresentation은 관찰 가능한 시각 표현이며 biological sex를 뜻하지 않는다.
6. 각 appearance fact에 factId와 exact source path/section 또는 JSON pointer를 연결한다.
7. requiredElements와 prohibitedElements를 독립 관찰 가능한 문장으로 planning에서 확정하고 evidenceFactIds를 연결한다.
8. 이름, 성격, combat lore, skill, role, grade, tag에서 성별 표현·biological sex·색·소재·얼굴·복식·무기 세부·포즈·표시 크기·세부 밀도·금지 요소를 추론하지 않는다.
9. 근거가 부족한 모든 필드는 missingDesignInputs에 typed failureType, requiredDecision, checked source, blocked handoff를 기록하고 planningStatus/readiness를 blocked로 둔다.
10. current character single-image 요청이면 generatedMediaPlanning.characterSingleImage에 identityConsistencyLock과 완전한 singleImageSpecification을 planning 근거로 확정한다. assetType=character_single_image, domainType=character이며 한 viewpoint만 허용한다. 8-way/rotation/directions/ordered_rotation_set, animation/variant, PixelLab/legacy identity를 current 계약에 넣지 않는다.
11. singleImageSpecification은 viewpoint, pose, framing, canvas, targetDisplaySize, safeArea, finalBackgroundPolicy, generationBackground{mode=removable_solid,color}, noShadow, outline, anchor를 모두 포함한다. outline.enabled=false이면 color와 exactThicknessPx를 생략한다. enabled=true이면 둘을 필수로 쓰고 placement=outside_silhouette로 고정한다. anchor는 type=pelvis_root_ground_axis와 pelvisOrRootPoint/groundContactAxis를 모두 요구한다.
12. createCharacterSingleImageHandoffs=true인 각 대상에 대해 planningStatus=approved, characterSingleImage.readiness=ready, missingDesignInputs=[]인지 검증한다. 기존 characterMainImage.rotationPolicy=generated_media_exact_8_way_v1만 있는 대상은 legacy_record_not_current_request로 차단하고 current 필드를 자동 생성·변환하지 않는다. 구 입력명 createCharacterMainImageHandoffs는 current prompt에서 지원하지 않는 read-only legacy 이름이다.
12a. expression profile을 선택하면 registry의 exact key/hash를 승인된 planning pointer로만 기록한다. `projectbs_character_open_ink_wash_dynamic_contour@1.0.0` 선택은 4-5 heads/target 4.25, young adult/no child, 35-55 omission/target 45, pressure-variable mok-seon phases, broad watercolor/pastel bleed와 outline 밖 controlled misalignment, 분리된 세 palette role, figure interior와 canvas 각각 achromatic/unpainted >=70%, removable warm-ivory solid, no halo/vignette/scene/shadow, exact Korean/Joseon identity/equipment anchor facts가 모두 있을 때만 허용한다. audit-only style-reference SHA를 identity, edit target 또는 임시/절대 path binding으로 기록하지 않는다. 하나라도 충돌하면 `character_style_profile_conflict`로 차단하고 profile 의미를 planning에 맞춰 변형하지 않는다.
12a-1. 위 open ink-wash key로 새 decision을 만드는 경우 CharacterPlanningDataGuide의 closed `openInkWashPlanningProjection`을 작성한다. fullBodyHeadCount, contourOmissionTargetPercent, figureInterior/fullCanvas negative-space floor, 세 palette anchor array, generation background, 네 background exclusion, style-reference fidelity scalar를 각각 leaf JSON pointer의 별도 approved fact로 capture한다. 복합 자연어 문장이나 projection 전체 object 한 개로 이를 대체하지 않는다. 현재 audit-only SHA만 있는 상태는 `styleReferenceFidelity.mode=semantic_text_projection_only`, `providerReferenceAuthorized=false`이며 선택 raster와의 시각·구도 유사도를 보장하지 않는다. 사용자가 선택 raster를 match/closely follow하는 fidelity를 요구하면 `character_style_profile_conflict`로 handoff를 차단하고, 별도 durable project-relative style-only reference contract publication을 요구한다. 기존 authoritative baseline의 immutable handoff에는 이 신규 producer rule을 소급 적용하거나 재작성하지 않는다.
12a-2. reviewed durable binding을 사용하는 새 decision은 `character_open_ink_wash_planning_projection_v2`와 `styleReferenceFidelity.mode=durable_style_only_binding`을 exact 선택한다. GeneratedMediaStyleReferenceBindingGuide의 asset/review/index를 raw bytes로 검증하고, role/path/asset hash/review record ID/path/raw hash 여섯 binding leaf를 각각 approved fact로 capture한 뒤 동일한 closed object를 handoff의 단일 `styleReferenceBindings` entry로 복사한다. 이를 identityConsistencyLock, person/pose/action/clothing/equipment/edit target evidence로 사용하지 않는다. 세-member binding, absolute/temp path, review/hash mismatch 또는 semantic transfer는 current 중앙 blocker로 no-write 차단한다.
12a-1. `projectbs_character_open_ink_wash_dynamic_contour@2.0.0`은 새 planning revision이 exact key/hash를 명시적으로 선택할 때만 사용한다. v1 값에 더해 cranial mass-to-chin/head-to-sole measurement, one broad broken-edge armor mass와 no plate/rivet/lacing/microtexture enumeration, uniform #F2EFE6/no radial gradient·dark backdrop, mandatory ordered post-output gate와 compact receipt를 승인 pointer로 보존한다. 기존 v1 planning/handoff/prompt/preview를 v2로 자동 변환하지 않는다.
12b. 새 승인 revision은 기존 visual-design 파일의 가장 큰 numeric `.vN` 다음 번호를 자동 선택한다. decisionId/path를 CharacterPlanningDataGuide의 공식으로 만들고 decision을 atomic no-clobber로 먼저 쓴다. `approval.approvedAt`은 decision 생성 시 한 번만 explicit-offset RFC3339 현재 시각으로 고정하며 retry에서 갱신하지 않는다. 동시 collision이면 canonical planning을 쓰기 전에 목록을 다시 읽고 다음 번호를 선택한다.
13. 준비된 대상만 별도 generated_media_planning_handoff_v2를 작성한다. `Deterministic producer-owned planning capture` 계약에 따라 canonical planning과 `provenance.sourcePlanningRefs`의 character design-decision JSON에서 source 순서를 자동 구성하고, current decision의 `/approval/approvedAt`을 capturedAt으로 복사하며, completed snapshot hash에서 `gmplan2.{assetType}.{contentId}.{snapshotHash[0:20]}` requestId를 계산한다. caller-owned capture 승인이나 manual ID/source-order 입력을 요청하지 않는다. authoritative baseline에 이미 존재하는 legacy request identity의 immutable v2는 read-only history로만 보존하며 신규 legacy-form handoff는 만들지 않는다.
14. canonical planning과 사용한 design decision 파일 전체의 exact project-relative path/role/UTF-8 byte SHA-256를 capture 순서 그대로 sourcePlanningFiles에 넣고, planningSnapshot은 GeneratedMediaPlanningHandoffGuide.md의 `Closed Planning Snapshot v2`를 그대로 적용한다. approvedFacts schema/hash payload를 이 prompt에서 재정의하지 않는다. publication 직전에 모든 source bytes/hash를 다시 검증한다. revision은 source authority의 안정적 값을 그대로 받았을 때만 포함한다.
15. handoff에는 identityConsistencyLock, singleImageSpecification, ordered requiredElements/prohibitedElements만 exact field-level 매핑한다. 누락 capture/provenance, invalid capturedAt, 해석 불가·중복 source path/JSON pointer, identity/source order/hash/snapshot 불일치, incomplete technical specification은 fail closed하며 부분 handoff를 쓰지 않는다.
16. 준비되지 않은 대상은 handoff를 만들지 않고 character_planning_not_media_ready, current contract의 정확한 failureType 및 missingDesignInputs를 반환한다.
17. Player planning은 player root, Npc/Boss는 {actGroupId}/npc root에 저장하고 common/index refs를 검증한다.
18. 이 작업에서 provider prompt, 이미지·애니메이션, 라우팅 record, 평가, Unity asset, Git 작업을 생성하거나 실행하지 않는다. 기존 immutable handoff/record와 canonical planning/design-decision 파일은 별도 승인된 migration 역할 없이 수정하지 않는다.

Output:
- canonicalSchemaVersion: character_planning_v2
- 생성한 common planning JSON 경로
- 생성한 character planning JSON 경로 및 characterType
- 보존한 legacy planning JSON 경로와 legacy classification
- planningStatus / characterSingleImage readiness
- missingDesignInputs
- 확정한 genderPresentation 및 intendedDisplay.targetDisplaySize/detailDensity
- 생성한 character_single_image planning handoff v2 경로 또는 생성하지 않은 이유
- sourceStoryRefs / sourcePlanningRefs
- Generated Media handoff 자동 capture derivation / sourcePlanningFiles / planningSnapshotHash / derived requestId
- selected expressionProfileKey / expressionProfilePayloadHash 또는 selection 없음
- 검증 결과

실패 시 Output:
- status: blocked | failed
- failureType: missing_story_file | invalid_act_group_id | insufficient_story_basis | invalid_json | missing_character_identity | invalid_character_type | invalid_character_planning_schema | missing_design_provenance | missing_gender_presentation | missing_body_design | missing_face_design | missing_hair_design | missing_costume_design | missing_equipment_decision | missing_weapon_design | missing_handedness_decision | missing_palette_design | missing_material_design | missing_identifying_features | missing_pose_policy | missing_display_contract | missing_target_display_size | missing_detail_density | missing_required_elements | missing_prohibited_elements | missing_source_planning_path | unresolved_source_planning_path | missing_capture_authority_timestamp | invalid_capture_authority_timestamp | missing_identity_consistency_lock | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | character_style_profile_conflict | character_planning_not_media_ready | legacy_record_not_current_request | legacy_character_planning_conflict | planning_snapshot_mismatch | record_collision
- affectedCharacterIds
- missingDesignInputs
- filesNotCreatedOrModified
- requiredPlanningDecision
- safeToRetry

검증:
- 모든 신규 개별 planning은 schemaVersion=character_planning_v2이고 unknown field가 없어야 한다.
- Player/Npc/Boss 모두 runtimeDomain=character를 사용해야 한다.
- commonDataRef, identity, combat, planningScore, stats, skills가 기존 권위와 일치해야 한다.
- approved planning의 필수 appearance field, genderPresentation provenance, targetDisplaySize/detailDensity, required/prohibited statements가 완전해야 한다.
- 모든 observable statement는 evidenceFactIds로 exact source에 추적되어야 한다.
- missingDesignInputs가 있으면 planningStatus=approved 또는 readiness=ready가 될 수 없다.
- legacy 파일은 명시적 migration 승인 없이 덮어쓰지 않아야 한다.
- planning JSON에 자신의 hash나 자신을 포함하는 snapshot hash가 없어야 한다.
- sourcePlanningFiles.revision은 authority 입력이 있을 때만 동일한 값으로 존재하고, 없으면 생략되어야 한다.
- handoff는 approved/ready 대상에만 별도 파일로 존재하고 planning 파일 완료 후 계산한 hash를 사용해야 한다.
- current handoff는 assetType=character_single_image/domainType=character이고 identityConsistencyLock과 complete singleImageSpecification만 가져야 한다.
- current handoff에 rotationPolicy, directions, ordered_rotation_set, animation/variant 또는 PixelLab/legacy 필드가 없어야 한다.
- outline.enabled=false이면 color/exactThicknessPx가 없어야 하며 enabled=true이면 둘 다 유효해야 한다.
- 모든 sourcePlanningFiles hash, approvedFact pointer, planningSnapshotHash가 exact source bytes와 일치해야 한다.
- decision revision/path, capturedAt, sourcePlanningFiles 순서, snapshotHash, derived requestId가 deterministic producer-owned capture 공식과 정확히 일치해야 한다.
- expression profile selection은 registry exact key/hash이며 open ink-wash 선택은 모든 closed planning binding과 충돌하지 않아야 한다.
- 신규 open ink-wash decision은 closed planning projection의 leaf facts가 snapshot에 각각 존재하고 값이 exact source pointer와 일치해야 하며, audit-only reference로 raster fidelity를 약속하지 않아야 한다.
- open ink-wash v2 선택은 predecessor를 수정하지 않는 별도 exact successor pointer이며 output-conformance/receipt contract를 생략할 수 없다.
- durable style-only fidelity는 exact six-member binding과 review/index/asset raw hash가 모두 일치해야 하며 character identity/equipment approved fact를 대체할 수 없다.
- 후속 단계가 이름·성격·combat lore로 시각 디자인을 보충하도록 지시하지 않아야 한다.
- 실제 provider 실행, 평가, project promotion, Unity 또는 Git 변경이 없어야 한다.
- fresh replacement가 true alpha를 요구하면 `generated_media_transparent_foreground_selection_v1`을 정확히 한 번 선택하고 key/hash `generated_media_true_alpha_foreground@1.0.0` / `2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108`, positive safeMarginPx, noClipping=true를 기록한다. mainLock 또는 six-frame animationLock만 선택하며 animation은 exact canvas/pelvis-world-root/ground baseline/rational scale/no independent recenter/dynamic pigment anchor exclusion을 planning이 닫는다. downstream이 값을 추론하게 하지 않는다.
- open-ink v2 reference 기반 attack animation은 sparse-motion을 선택하지 않고 exact successor `projectbs_character_open_ink_wash_attack_motion@1.0.0` / `07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c`, unchanged base key/hash, six exact motion bindings와 exact true-alpha selection을 새 immutable handoff에 기록한다. 기존 blocked handoff는 수정하지 않는다.
- Grade 2/3 같은 새 opaque-chroma MAIN은 exact successor `projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0` / `b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a`를 명시하고 unchanged open-ink v2 base, 1024x1536, `generationBackground={mode:removable_solid,color:#00FF00}`, one opaque PNG master, no exact #00FF00 foreground, distinct later `generated_media_chroma_uncomposite`를 닫는다. `transparentForegroundSelection`이나 direct-alpha required element를 넣지 않으며 기존 handoff를 변환/덮어쓰지 않는다.
- Grade 2 identity-anchored regeneration은 별도 execution profile `projectbs_character_open_ink_opaque_chroma_identity_anchored_regeneration@1.0.0` / `44d3bafcc720d39ac260fb2089798c16f9ec1f50d391165eea676dbc79cdc3ad`와 guide의 exact seven-member `identityAnchoredGenerationSelection`을 새 revision/handoff top-level에 한 번만 기록한다. project-relative portrait `Assets/ImagesGenerated/Character/portrait/character.seojin.1.portrait.png` / `ba2f769b...97cf`는 identity/equipment authority일 뿐 edit source, style-only, pose/background 또는 provider receipt evidence가 아니다. 동일한 young Korean male face, low topknot/short controlled hair, compact body, right-handed sword/scabbard, pouch/shoulder equipment와 restrained command-ready Joseon naval-officer evolution을 approved facts로 닫고, ronin/samurai/katana/foreign/fantasy, long loose hair, tattered mantle, wrapped forearms/boots, aging/severe redesign, literal Yi Sun-sin reproduction을 금지한다. 기존 rejected G2 lineage를 재사용하지 않고 새 request/snapshot을 no-clobber로 만든다.
- Grade 2/3 `character_animation_v2` opaque-chroma movement는 exact expression/execution successor `projectbs_character_open_ink_wash_animation_opaque_chroma_master@1.0.0` / `projectbs_character_open_ink_animation_opaque_chroma_identity_anchored@1.0.0` / payload `da38a4c91bbe3a808f09f1c24763cd3cece02518a2d1398f7294ce3eedb3f7c8`과 guide의 two closed top-level selections을 새 immutable handoff에 기록한다. MAIN은 registered evaluated identity/equipment/orientation reference 하나이고, Grade1 lineage는 planning strings/ordered phases/motion topology/direction/closure intent만 전달한다. Grade1 raster는 provider reference/output이 아니며 MAIN은 edit/motion/style/pose/framing/background/pixel-copy authority가 아니다. `animationSourceMode=provider_opaque_chroma_3x2_master`, `extractionMode=postprocess_exact_cell_chroma_root_gif_v1`, one RGB 1536x1024 exact 3x2 master, six 512 cells, #00FF00 carrier, later root(256,300)/baseline448/safeMargin48/150msx6 infinite GIF를 닫고 provider-native GIF/direct-alpha/coherent-master branch와 혼합하지 않는다.
- next-grade `character_single_image_v2`가 evaluated prior-grade MAIN identity를 상속하면 `projectbs_character_open_ink_opaque_chroma_sequential_grade_identity_anchored@1.0.0` / `73a48f8c8013e3a79ac04e0c161075a14ce6b1194527c48585fd33edb009ea04`와 exact `generated_media_sequential_grade_identity_authority_selection_v1`을 top-level에 기록한다. completed-PASS evaluation/source-bound receipt ID·path·SHA와 exact MAIN path/SHA/bytes를 결속하고 face geometry/hairline/topknot/compact proportions/handedness/equipment/orientation을 고정한다. target planning의 next-grade clothing/authority facts만 delta이며 edit/style/pose/background/pixel transfer, provider receipt 합성, rejected target SHA 재사용은 금지한다.
```
