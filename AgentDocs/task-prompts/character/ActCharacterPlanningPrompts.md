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
- handoffRequestIds:
  - contentId: {character_id}
    requestId: {stable_request_id}
- planningCaptureInputs:
  - contentId: {character_id}
    requestId: {stable_request_id}
    capturedAt: {planning_authority_approved_rfc3339_timestamp_with_explicit_numeric_offset}
    sourcePlanningPaths:
      - {ordered_project_relative_planning_or_decision_json_path}
- sourceRevisionInputs:
  - path: {optional_exact_source_path}
    revision: {optional_stable_revision_from_source_authority}

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
13. 준비된 대상만 별도 generated_media_planning_handoff_v2를 작성한다. caller/planning orchestration이 승인해 제공한 request별 planningCaptureInputs를 `Closed planning capture input` 계약으로 검증한다. authoring agent는 capturedAt을 만들거나 현재 시각을 사용하거나 sourcePlanningPaths를 선택·추가·제거·중복 제거·재정렬하지 않는다. capture contentId/requestId는 handoff identity와 exact equality여야 하며 ordered sourcePlanningPaths 각 항목은 같은 index의 sourcePlanningFiles.path에 정확히 한 번 대응해야 한다.
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
- Generated Media handoff planningCaptureInputs 적용값 / sourcePlanningFiles / planningSnapshotHash
- 검증 결과

실패 시 Output:
- status: blocked | failed
- failureType: missing_story_file | invalid_act_group_id | insufficient_story_basis | invalid_json | missing_character_identity | invalid_character_type | invalid_character_planning_schema | missing_design_provenance | missing_gender_presentation | missing_body_design | missing_face_design | missing_hair_design | missing_costume_design | missing_equipment_decision | missing_weapon_design | missing_handedness_decision | missing_palette_design | missing_material_design | missing_identifying_features | missing_pose_policy | missing_display_contract | missing_target_display_size | missing_detail_density | missing_required_elements | missing_prohibited_elements | missing_planning_capture_inputs | invalid_planning_capture_timestamp | missing_source_planning_path | duplicate_source_planning_path | unresolved_source_planning_path | planning_capture_identity_mismatch | missing_identity_consistency_lock | missing_single_image_viewpoint | missing_single_image_pose | missing_framing_contract | missing_canvas_contract | missing_safe_area | missing_background_policy | missing_generation_background | missing_no_shadow_policy | missing_outline_policy | invalid_outline_contract | missing_anchor_contract | character_planning_not_media_ready | legacy_record_not_current_request | legacy_character_planning_conflict | planning_snapshot_mismatch | record_collision
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
- planningCaptureInputs의 contentId/requestId/capturedAt/sourcePlanningPaths가 authority 입력과 byte-equal하고, sourcePlanningPaths와 sourcePlanningFiles.path가 같은 순서로 일대일 대응해야 한다.
- 후속 단계가 이름·성격·combat lore로 시각 디자인을 보충하도록 지시하지 않아야 한다.
- 실제 provider 실행, 평가, project promotion, Unity 또는 Git 변경이 없어야 한다.
```
