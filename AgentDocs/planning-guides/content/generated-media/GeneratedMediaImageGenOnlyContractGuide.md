# Generated Media ImageGen-Only Contract Guide

## 1. Purpose and Version Boundary

Guide Type: schema, routing, and compatibility authority for new Generated
Media requests.

```text
currentPlanningSchema: generated_media_planning_handoff_v2
currentRegistryVersion: generated_media_authoring_profile_registry_v2
currentRouterVersion: generated_media_router_v2
currentRoutingRecordSchema: generated_media_routing_v2
currentPromptSchema: generated_media_prompt_v3
currentGenerationSchema: generated_media_generation_v2
currentPreservationSchema: generated_media_preservation_v2
currentEvaluationPackageSchema: generated_media_evaluation_package_v2
currentProvider: imagegen
```

Every request created under this contract uses ImageGen. The only current
execution roles are:

```text
character_single_image
icon_single_image
animation
```

PixelLab records and the v1 registry remain immutable legacy evidence. They are
never upgraded in place, selected for a new request, or reinterpreted by this
guide. `ordered_rotation_set`, eight-way character generation, legacy
`imagegen_image`, and provider-specific v1 routes are not current routes.

## 2. Authority and Stage Separation

```text
Master Concept
-> approved immutable planning
-> planning handoff and router
-> provider-prompt authoring
-> ImageGen generation
-> preservation and packaging
-> separate evaluation and promotion
```

The stages have separate records and owners. Authoring cannot call ImageGen.
Generation cannot download, transform, package, evaluate, or promote. Packaging
cannot regenerate or evaluate. Evaluation and promotion cannot rewrite source,
prompt, generation, or package records.

Required authorities:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
```

## 3. Current Planning Handoff

`generated_media_planning_handoff_v2` requires common immutable identity:

```yaml
schemaVersion: generated_media_planning_handoff_v2
requestId:
assetType: character_single_image | icon_single_image | animation
domainType: character | skill | item
contentId:
contentUsage:
sourcePlanningFiles:
  - path:
    role:
    sha256:
    revision: optional
planningSnapshot:
  capturedAt:
  snapshotHash:
  approvedFacts:
requiredElements: non-empty observable list
prohibitedElements: non-empty observable list or signed no_prohibitions
projectTarget: optional informational_only
```

Missing facts are never inferred. Each type adds exactly one specification.

### 3.1 character_single_image

```yaml
identityConsistencyLock:
  identityId:
  referenceFacts: non-empty
singleImageSpecification:
  viewpoint:
  pose:
  framing:
  canvas: {width:, height:}
  targetDisplaySize: {width:, height:}
  safeArea:
  finalBackgroundPolicy:
  generationBackground:
    mode: removable_solid
    color:
  noShadow: true | false
  outline:
    enabled: true | false
    color: required when enabled
    exactThicknessPx: required positive integer when enabled
    placement: outside_silhouette
  anchor:
    type: pelvis_root_ground_axis
    pelvisOrRootPoint:
    groundContactAxis:
```

One approved viewpoint produces one image. No direction list, rotation count,
eight-way expansion, or `ordered_rotation_set` is allowed.

### 3.2 icon_single_image

```yaml
identityConsistencyLock:
  identityId:
  referenceFacts: non-empty
iconProfile: {profileId:, profileVersion:}
singleImageSpecification:
  viewpoint:
  pose: static_symbol | approved_explicit_pose
  framing:
  canvas: {width:, height:}
  targetDisplaySize: {width:, height:}
  safeArea:
  finalBackgroundPolicy:
  generationBackground:
    mode: removable_solid
    color:
  noShadow: true | false
  outline:
    enabled: true | false
    color: required when enabled
    exactThicknessPx: required positive integer when enabled
    placement: outside_silhouette
  anchor:
    type: visual_center
    point:
```

Skill/item differences are registered profiles, not copied execution prompts.

### 3.3 animation

One planning handoff may contain multiple approved requests. The router splits
them deterministically. Every routing, authoring, prompt, generation,
preservation, and evaluation unit after that split contains exactly one request.

```yaml
animationRequests:
  - animationRequestId: stable exact identity, unique in handoff
    animationSubjectType: character | skill_effect
    identityConsistencyLock: required for character, optional only when profile explicitly says not_applicable
    referenceImage:
      identity:
      path:
      sha256:
    finalFrameCount: approved positive integer
    timing: ordered final timing or approved uniform FPS
    frameOrder: exact ordered indices
    loop: loop | one_shot | hold_last
    keyPoses: non-empty ordered definitions
    fixedCellCanvas: {width:, height:}
    scaleLock:
    allowedIntentionalVerticalMotion:
    finalBackgroundPolicy: transparent
    generationBackground:
      mode: removable_solid
      color:
    noShadow: true | false
    outline:
      enabled: true | false
      color: required when enabled
      exactThicknessPx: required positive integer when enabled
      placement: outside_silhouette
    anchor:
      type: pelvis_root_ground_axis | effect_origin
      point:
      groundContactAxis: required for pelvis_root_ground_axis
    masterFirst: true
    extractionMode: fixed_cell_only
```

The router emits one independent normalized `animationRequest` object and
record per source-order `animationRequestId` before authoring. Authoring and
generation reject arrays in the normalized unit, merged requests, and added
motions.

## 4. Animation Master and Extraction Contract

The approved final frame count is used from the first generation attempt.
Oversampling followed by frame selection is prohibited.

```text
one coherent master result
-> fixed-cell split only
-> apply approved transparent-background and outline policy
-> save completed GIF first
-> reopen that GIF
-> extract ordered PNG frames
```

- per-frame crop, rescale, silhouette recenter, or canvas change is forbidden;
- scale remains locked across every frame;
- drift correction may translate only the declared root/effect anchor;
- approved intentional vertical movement is preserved and is not drift;
- chroma key removes only the declared generation-background color;
- internal subject colors and luminance are byte/measurement-preserved within
  the approved conversion tolerance;
- key residue removal, transparent output, white 2px outside outline, or any
  other exact value comes from approved input/profile and is never global.

## 5. Closed Current Registry

Registry matching uses exact canonical lowercase values. Exactly one row must
match. Zero or multiple rows block.

| rowId | assetType | domainType | profile | authoring prompt | generation prompt | structureProfile |
| --- | --- | --- | --- | --- | --- | --- |
| `character_single_image_v2` | `character_single_image` | `character` | `character_single_image@2.0.0` | `ImageGenCharacterImagePromptAuthoringPrompt.md` | `ImageGenCharacterImageGenerationPrompt.md` | `character_single_image_v2` |
| `skill_icon_single_image_v2` | `icon_single_image` | `skill` | `skill_icon@2.0.0` | `ImageGenIconPromptAuthoringPrompt.md` | `ImageGenIconGenerationPrompt.md` | `icon_single_image_v2` |
| `item_icon_single_image_v2` | `icon_single_image` | `item` | `relic@2.0.0` | `ImageGenIconPromptAuthoringPrompt.md` | `ImageGenIconGenerationPrompt.md` | `icon_single_image_v2` |
| `character_animation_v2` | `animation` | `character` | `character_animation@2.0.0` | `ImageGenAnimationPromptAuthoringPrompt.md` | `ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |
| `skill_animation_v2` | `animation` | `skill` | `skill_animation@2.0.0` | `ImageGenAnimationPromptAuthoringPrompt.md` | `ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |

All prompt paths are relative to
`AgentDocs/task-prompts/content/generated-media/`. All rows use
`provider=imagegen`. Adding a domain requires a reviewed registry/profile row;
it does not create a domain execution prompt.

## 6. Record and Storage Contract

Current record paths use directory version v2 and never share the legacy v1
index:

```text
AgentDocs/planning-data/generated-media-routing/v2/{assetType}/{contentId}/
AgentDocs/planning-data/generated-media-prompts/v2/{assetType}/{contentId}/
AgentDocs/planning-data/generated-media-generation/v2/{assetType}/{contentId}/
AgentDocs/planning-data/generated-media-preservation/v2/{assetType}/{contentId}/
```

For animation, record identity also includes `animationRequestId` and its path
adds `/{animationRequestId}/` after `{contentId}`. IDs are deterministic hashes
of immutable request/content/source/snapshot identity, exact registry row,
profile version, and type specification. Same ID plus identical bytes is
idempotent reuse. Same ID plus different bytes is a collision. No v1 record or
index is edited.

`generated_media_prompt_v3` has exactly one ImageGen prompt payload.
`generated_media_generation_v2` stores the exact prompt hash, settings,
attempts, cost evidence, and provider refs. Both reject `provider=pixellab` and any PixelLab
payload branch.

```yaml
generated_media_prompt_v3:
  schemaVersion: generated_media_prompt_v3
  promptRecordId:
  requestId:
  assetType:
  domainType:
  contentId:
  animationRequestId: required only for animation
  planningSnapshotHash:
  sourcePlanningFiles: []
  registryVersion: generated_media_authoring_profile_registry_v2
  registryRowId:
  provider: imagegen
  structureProfile:
  visualBrief:
  visualBriefSha256:
  scenePromptOriginal:
  providerPromptPayloadHash:
  providerSettingsIntent:
  requiredElements: []
  prohibitedElements: []
  status: ready_for_generation | blocked
  createdAt:
  validation:

generated_media_generation_v2:
  schemaVersion: generated_media_generation_v2
  generationRecordId:
  requestId:
  assetType:
  domainType:
  contentId:
  animationRequestId: required only for animation
  planningSnapshotHash:
  promptRecordId:
  promptRecordSha256:
  provider: imagegen
  providerPromptPayloadHash:
  structureProfile:
  providerSettings:
  providerTool: imagegen
  providerInterface: configured_imagegen_capability
  providerExecutionApproval:
    approvedBy:
    approvedAt:
    scopeHash:
    maxAttempts:
    maxCost:
  idempotencyKey:
  attempts: []
  costEvidence: []
  providerResultRefs: []
  preservationHandoff:
  generationStatus: generated | blocked | failed
  createdAt:
  validation:
```

Unknown fields are rejected. Animation records without one scalar
animationRequestId, or non-animation records containing it, are invalid.

## 7. Preservation and Structure Profiles

```text
character_single_image_v2:
  one preserved original/coherent source
  one approved single-view output
  identity lock, canvas, safe area, background, outline, and anchor metadata

icon_single_image_v2:
  one preserved original/coherent source
  one approved icon output
  identity lock, canvas, safe area, background, outline, and visual-center metadata

animation_gif_frame_set_v2:
  coherent master source
  completed GIF
  PNG frames extracted by reopening the GIF
  contiguous final order and timing
  fixed cell, scale lock, anchor, intentional vertical-motion metadata
```

Packaging owns download and conversion. It must preserve original hashes and
record every derived-member hash. Evaluation receives a sealed package and
cannot alter it.

## 8. Typed Blockers and Readiness

This section is the single current failure-token authority. A stage may use
common readiness tokens plus only the tokens listed for that stage below.
Prompts/guides must not invent aliases. Legacy audit tokens, including
`legacy_execution_forbidden`, belong exclusively to
`AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md`.

### 8.1 Common and Type Readiness

```text
missing_planning_handoff
planning_snapshot_mismatch
missing_identity_consistency_lock
missing_required_elements
missing_prohibited_elements
missing_single_image_viewpoint
missing_single_image_pose
missing_framing_contract
missing_canvas_contract
missing_target_display_size
missing_safe_area
missing_background_policy
missing_generation_background
missing_no_shadow_policy
missing_outline_policy
invalid_outline_contract
missing_anchor_contract
missing_animation_request_id
multiple_animation_requests_not_allowed
missing_reference_image
reference_image_hash_mismatch
missing_final_frame_count
missing_animation_timing
missing_frame_order
missing_loop_contract
missing_key_poses
missing_fixed_cell_contract
missing_scale_lock
missing_vertical_motion_policy
missing_master_first_contract
oversampling_not_allowed
unsupported_provider
missing_provider_execution_approval
provider_cost_not_approved
retry_limit_exceeded
duplicate_provider_call_risk
prompt_record_missing
prompt_record_stale
provider_operation_failed
unsupported_current_route
legacy_record_not_current_request
record_collision
routing_record_collision
```

### 8.2 Router Extension

```text
duplicate_animation_request_id
conflicting_routing_evidence
routing_record_collision
routing_record_write_failed
routing_index_write_failed
```

### 8.3 Authoring and Record Extension

```text
unknown_record_field
missing_record_field
record_identity_mismatch
record_hash_mismatch
record_collision
index_entry_invalid
prompt_markdown_mismatch
prompt_record_missing
prompt_record_stale
provider_value_invalid
unsupported_record_schema
```

### 8.4 Generation Extension

```text
missing_provider_execution_approval
provider_cost_not_approved
retry_limit_exceeded
duplicate_provider_call_risk
provider_operation_failed
```

### 8.5 Preservation Extension

```text
missing_planning_handoff_v2
missing_routing_v2
missing_prompt_v3
missing_generation_v2
generation_not_ready
generation_record_hash_mismatch
record_identity_mismatch
preservation_record_collision
provider_result_ref_missing
provider_result_unavailable_requires_generation_task
unsupported_preservation_adapter
evaluation_staging_root_not_configured
staging_project_path_violation
original_download_failed
provider_export_failed
source_not_original
source_hash_mismatch
extraction_failed
fixed_cell_contract_mismatch
scale_lock_violation
anchor_mapping_mismatch
vertical_motion_policy_violation
chroma_key_scope_violation
gif_first_sequence_violation
frame_order_mismatch
member_hash_mismatch
manifest_validation_failed
package_finalize_failed
package_collision
package_seal_failed
evaluation_adapter_missing
```

### 8.6 Evaluation-package Extension

```text
missing_preservation_v2
invalid_current_version_chain
missing_animation_request_id
multiple_animation_requests_not_allowed
structure_profile_mismatch
animation_anchor_profile_mismatch
gif_first_evidence_missing
frame_count_mismatch
member_hash_mismatch
package_path_violation
staging_target_path_collision
package_collision
unknown_package_field
missing_package_field
```

Readiness is true only when every common and type-specific field exists, hashes
verify, exactly one current registry row matches, provider is ImageGen, and the
provider interface is `configured_imagegen_capability`. Before every external
call, approval scope must match the prompt/settings hash, estimated cost must
fit `maxCost`, attempts must remain below `maxAttempts`, and the deterministic
idempotency key must have no active duplicate. An identical completed result is
reused without billing; every attempted or avoided call records `costEvidence`.
The stage input record must also be valid. Blockers are identical across schema, router,
authoring prompt, generation prompt, and packaging adapter.

## 9. Legacy and Non-migration Boundary

- v1 PixelLab planning/routing/prompt/generation/preservation/evaluation records
  remain readable under their original documents and hashes;
- legacy PixelLab guides/prompts are deprecated read-only reproduction aids;
- no new v2 request can select a v1 row or write a v1 index;
- no v1 PixelLab prompt is converted to ImageGen automatically;
- no eight-way package is collapsed into a current character image;
- legacy `ordered_rotation_set` remains readable by legacy evaluation only;
- re-requesting legacy content requires a new approved v2 planning handoff and
  produces unrelated v2 record identities.

## 10. Validation

- current registry has no PixelLab row;
- current character path contains no eight-way or rotation-set requirement;
- one animation unit has exactly one `animationRequestId`;
- schema examples, registry rows, prompt inputs, failure types, and readiness
  use the same names;
- authoring, generation, packaging, evaluation, and promotion stay separate;
- current prompt/generation records accept only `provider=imagegen`;
- all source and reference hashes recompute;
- legacy records and Master Concept remain unmodified.

## 11. Related Current Guides

```text
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenCharacterImagePipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenAnimationPipelineGuide.md
```
