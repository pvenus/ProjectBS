# Generated Media Current Evaluation Package Guide

## Purpose and Boundary

Guide Type: current v2 schema/data-structure authority. It converts a validated
`generated_media_preservation_v2` record into an immutable evaluation package.
It does not generate, transform, score, approve, promote, publish, or call a
provider. Legacy v1 packages and profiles are read only under
`AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md`.

## Authority

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
```

Current input is ImageGen-only and must form this exact chain:

```text
planning_handoff_v2 -> routing_v2 -> prompt_v3 -> generation_v2
-> preservation_v2 -> evaluation_package_v2
```

## Layout and Identity

Resolve `{evaluationStagingRoot}` from current-PC configuration. A foreign
absolute root blocks. Staging must differ from and not sit below projectTarget.

```text
{evaluationStagingRoot}/.assembling/{requestId}/{preservationRecordId}.{attemptId}/
{evaluationStagingRoot}/{assetType}/{contentId}/{requestId}/{packageId}/
  planning/
  prompt/
  generation/
  preservation/
  source/
  extracted/
  manifest.json
  evaluation-request.json
```

Animation inserts `{animationRequestId}` after `{contentId}`. The temporary
directory is never an evaluation source. Every member has a lowercase SHA-256.

```yaml
schemaVersion: generated_media_evaluation_package_v2
packageId:
manifestPayloadHash:
manifestPayload: {}
sealedAt:
evaluationReadiness: ready | blocked
evaluationBlockers: []
```

Canonical JSON is UTF-8 without BOM, lexicographically sorted object keys,
preserved array order, no insignificant whitespace and LF strings. Hash only
`manifestPayload`; exclude envelope fields, absolute roots and derived hashes.

```text
manifestPayloadHash=SHA256(canonical_json(manifestPayload))
packageId=evalpkg2.{assetType}.{contentId}.{optionalAnimationRequestId}.{manifestPayloadHash[0:16]}
```

The final directory is atomically renamed after schema and member verification.
An identical existing package is reused; differing bytes at the same identity
block `package_collision`. Sealed packages are immutable.

## manifestPayload v2

```yaml
requestId:
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
contentId:
animationRequestId: exactly one scalar for animation; absent otherwise
planningSnapshotHash:
routingRecordId:
promptRecordId:
promptRecordSha256:
generationRecordId:
generationRecordSha256:
preservationRecordId:
preservationPayloadHash:
provider: imagegen
structureProfile: character_single_image_v2 | icon_single_image_v2 | background_single_image_v2 | animation_gif_frame_set_v2
profileExtension: {}
members:
  - memberId:
    role:
    relativePath:
    sha256:
    mediaType:
    width:
    height:
    order:
    profileData: {}
projectTarget:
  path: optional informational path
  status: informational_only
```

Unknown or missing fields fail. Relative paths must remain inside the package.
The copied planning snapshot, copy-ready provider prompt, generation record,
preservation record, original media and extracted members must hash-match their
authoritative records.

## Closed Current Structure Profiles

### character_single_image_v2

Exactly one approved-view primary image. Require identityConsistencyLock,
viewpoint, pose, framing, canvas, targetDisplaySize, safeArea, final background,
generation-background removal evidence, noShadow, outline, and
`pelvis_root_ground_axis` with pelvis/root point and ground-contact axis.

### icon_single_image_v2

Exactly one primary icon. Require identityConsistencyLock, exact icon profile,
framing, canvas, targetDisplaySize, safeArea, final/generation background,
noShadow, outline, and `visual_center` point.

### background_single_image_v2

Exactly one preserved original background image. Require exact registered
background profile, scene contract, composition/viewpoint, horizon and ordered
depth layers, playable/readability area, subject inclusions/exclusions,
canvas/aspect, target display, safe area, final background policy,
content/scene consistency lock and `scene_composition_anchor`. Reject icon
visual-center, icon outline/silhouette and small-size icon-readability fields.

The evaluation request keeps background `artifactType`, `evaluationDomain`,
structureProfile and promotion target identity distinct from an icon even when
both source members are one PNG.

For this profile, set `artifactType=background_single_image`, preserve the
registered `domainType` as evaluationDomain, and carry projectTarget only as
the approved informational promotion destination. The later evaluator and
promotion task must route by this identity tuple and never by `.png` shape.

### animation_gif_frame_set_v2

Require exactly one animationRequestId, hashed reference image, approved final
frame count/timing/order/loop/key poses, fixed cell, scale lock, intentional
vertical-motion policy, background/noShadow/outline and `masterFirst=true`.
Character profile requires `pelvis_root_ground_axis`; skill profile requires
`effect_origin`.

Members must include the coherent master, the completed GIF, and contiguous PNG
frames extracted by reopening that GIF. PNG count and order equal the approved
final frame count. Per-frame crop, scale, silhouette-center, changed cell size,
or unapproved vertical-motion removal blocks readiness.

## evaluation-request.json

```yaml
schemaVersion: generated_media_evaluation_request_v2
requestId:
packageId:
assetType:
domainType:
contentId:
animationRequestId: required only for animation
structureProfile:
manifestPath:
manifestPayloadHash:
evaluationDomain:
artifactType:
stagingArtifactPath:
evaluationWorkspacePath:
projectTargetPath:
promotionStatus: not_promoted
```

`stagingArtifactPath` and `projectTargetPath` must differ. This handoff requests
a separate evaluation; it contains no score or verdict.

## Current Background Evaluation Adapter

This section is the ready package-mode evaluation adapter for
`background_single_image + stage|battle|environment`. It never applies to
legacy `imagegen_image` or `battle_background`.

Fatal gates run first: sealed package/hash identity, exact background profile,
`background_single_image_v2`, complete scene metadata, planning evidence,
scene anchor, and no icon-adapter fields. Any missing or swapped contract is
FAIL or blocked according to evidence availability.

Score 100 points:

| criterionId | Category | Max | Minimum for PASS |
| --- | --- | ---: | ---: |
| `bg.planning_fidelity` | Planning and required/prohibited element fidelity | 20 | 18 |
| `bg.composition_viewpoint` | Scene composition, viewpoint and framing | 20 | 18 |
| `bg.depth_playable_area` | Horizon, depth layers and playable/readability area | 20 | 18 |
| `bg.canvas_target_safety` | Canvas, aspect, target display and safe-area fitness | 20 | 18 |
| `bg.policy_subject_consistency` | Background policy, subject contract and scene consistency | 20 | 18 |

PASS requires total >= 90, every category minimum, no fatal gate and no
Critical finding. Stage/battle/environment use the same stable criterion IDs;
their registered profile and planning evidence provide domain facts without
creating copied execution or evaluation prompts.

## State, Failure and Validation

```text
preservation_ready -> assembling -> validated -> sealed -> evaluation_requested
```

```text
missing_preservation_v2
invalid_current_version_chain
unsupported_provider
ambiguous_image_role
missing_background_scene_contract
missing_background_composition
missing_background_viewpoint
missing_background_horizon
missing_background_depth_layer_contract
missing_background_playable_area
missing_background_subject_contract
missing_background_canvas_contract
missing_background_aspect_ratio
missing_background_target_display
missing_background_safe_area
missing_background_consistency_lock
unsupported_icon_domain
unsupported_background_domain
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

Validate current version-chain parity, `provider=imagegen`, one closed profile,
profile-specific anchor, member count/order/hash, GIF-first provenance,
non-circular identity, atomic sealing and staging/project separation. A blocked
output contains status, failureType, missingFields, invalidMembers,
requiredDecision and safeToRetry. A success contains package ID/path/hash,
ordered members, structureProfile, evaluation-request path and nextStep.

## Downstream Entries

```text
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/task-prompts/content/GeneratedImageEvaluationPrompt.md
AgentDocs/planning-guides/content/GeneratedImageProjectPromotionGuide.md
AgentDocs/task-prompts/content/GeneratedImageProjectPromotionPrompt.md
```

Evaluation consumes the exact package identity. Promotion consumes the exact
package plus immutable evaluation record only after PASS; neither stage may
reinterpret a legacy identity as current background v2.
