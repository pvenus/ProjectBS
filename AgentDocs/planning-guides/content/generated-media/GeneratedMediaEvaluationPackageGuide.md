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

Current input is ImageGen-only and must form exactly one of these chains:

```text
strict branch (unchanged):
planning_handoff_v2 -> routing_v2 -> prompt_v3 -> generation_v2
-> preservation_v2 -> evaluation_package_v2

accepted-result branch:
planning_handoff_v2 -> routing_v2 -> accepted_result_capture_v1
-> preservation_v2 -> evaluation_package_v2
```

The accepted-result branch exists only for a validated preservation record that
binds an immutable accepted-result capture. It does not create or require a fake
`generated_media_prompt_v3` or `generated_media_generation_v2` record.

## Layout and Identity

Resolve `{evaluationStagingRoot}` from current-PC configuration. A foreign
absolute root blocks. Staging must differ from and not sit below projectTarget.

```text
{evaluationStagingRoot}/.assembling/{requestId}/{preservationRecordId}.{attemptId}/
{evaluationStagingRoot}/{assetType}/{contentId}/{requestId}/{packageId}/
  planning/
  prompt/ # strict or recovered-prompt accepted branch only
  generation/ # strict branch only
  accepted-capture/ # accepted-result branch only: record.json + capture-receipt.json
  preservation/
  source/
  extracted/
  manifest.json
  evaluation-request.json
```

Animation inserts `{animationRequestId}` after `{contentId}`. The temporary
directory is never an evaluation source. Every member has a lowercase SHA-256.
Exactly one of `generation/` or `accepted-capture/` exists. In the accepted
animation branch, `prompt/` contains the exact recovered provider-prompt bytes
whose raw hash equals the capture prompt evidence; it is not a prompt record.
It is absent for an unavailable-prompt `character_single_image` capture.

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
promptRecordId: strict branch only
promptRecordSha256: strict branch only
generationRecordId: strict branch only
generationRecordSha256: strict branch only
acceptedPromptEvidence: accepted-result branch only; exact closed projection below
acceptedResultCaptureRecordId: accepted-result branch only
acceptedResultCaptureRecordPath: accepted-result branch only; exact project-relative record path
acceptedResultCaptureRecordSha256: accepted-result branch only; exact raw Git-blob SHA-256
acceptedResultCaptureReceiptSha256: accepted-result branch only; exact receipt.receiptPayloadSha256
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

The accepted-result animation `acceptedPromptEvidence` object has exactly these members:

```yaml
source: accepted_result_capture
providerPromptPayloadHash: exact capture promptEvidence.providerPromptPayloadHash
promptFileSha256: exact capture promptEvidence.fileSha256
```

For `character_single_image` when the accepted capture has no historical prompt
evidence, the object instead contains exactly:

```yaml
source: accepted_result_capture
status: unavailable_observed
claim: not_claimed
```

No prompt path, hash, prompt record, or reconstructed prose is allowed. The two
closed shapes are mutually exclusive and the manifest preserves the capture's
truth without inventing prompt identity.

The four accepted-result fields and `acceptedPromptEvidence` are jointly
required and all four strict prompt/generation fields are forbidden. The strict
branch requires its existing four fields and forbids every accepted-result
field. Mixed, partial, or unknown branch fields fail before
`manifestPayloadHash` calculation.

The accepted capture record path/raw hash must match its canonical index. The
receipt must be the closed `generated_media_accepted_result_capture_receipt_v1`
for that exact record/path/hash, have a valid recomputed `receiptPayloadSha256`,
authorize preservation/evaluation, forbid promotion, and retain truthful
no-call capture action. Animation retains historical one-submit/zero-retry;
`character_single_image` may retain the exact `unavailable_observed` historical
counts from its capture.

Unknown or missing fields fail. Relative paths must remain inside the package.
In the strict branch, the copied planning snapshot, copy-ready provider prompt,
generation record, preservation record, original media and extracted members
must hash-match their authoritative records. In the accepted-result branch,
the copied planning snapshot, conditional recovered provider prompt, accepted
capture record, capture receipt, preservation record, original media and
extracted members must hash-match the accepted capture and preservation
identities. The unavailable-prompt shape requires no prompt copy.

Accepted-result animation `members` include exactly one
`accepted_provider_prompt`, one `accepted_capture_record`, one
`accepted_capture_receipt`, and the structure-profile media members.
`character_single_image` with unavailable prompt evidence omits the
`accepted_provider_prompt` member and includes exactly one captured PNG source
member with role `accepted_project_candidate_png` plus the record and receipt.
There is no `generation_record` member or
`generation/` directory. These evidence roles do not become prompt/generation
records.

## Closed Current Structure Profiles

### character_single_image_v2

Exactly one approved-view primary image. Require identityConsistencyLock,
viewpoint, pose, framing, canvas, targetDisplaySize, safeArea, final background,
generation-background removal evidence, noShadow, outline, and
`pelvis_root_ground_axis` with pelvis/root point and ground-contact axis.
Also preserve the exact prompt-record expression profile key, payload, payload
hash, provider prose, and profile/planning evidence map. For
`projectbs_character_animation_ready_minimal_ink_line@1.0.0`, package readiness
requires the closed proportion, detail-density, color/value, and authoring-
projection members; it never reconstructs them from pixels.

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

A character animation whose immutable reference prompt selected
`projectbs_character_animation_ready_minimal_ink_line@1.0.0` inherits the same
three independent evaluation fatal gates. Apply proportion, detail-density,
and color/value checks to every frame and to cross-frame consistency; any one
failing frame fails the set and cannot be hidden by average scoring. Skill
animation does not use these character-profile gates.

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

## Current Character Single-Image Evaluation Adapter

This package-mode adapter applies to
`character_single_image + character + character_single_image_v2`. It validates
the sealed package, exact prompt/profile identity, approved planning evidence,
one-view structure, and Master Concept before visual scoring.

For `projectbs_character_animation_ready_minimal_ink_line@1.0.0`, three fatal
semantic gates run independently before score acceptance:

- `character_evaluation_proportion_gate_failed`: observed full-body anatomy is
  greater than 4.25 heads, falls outside the approved 3.75-4.25-head or 24-27%
  head-height projection, or presents naturalistic seven-to-eight-head/heroic
  tall anatomy;
- `character_evaluation_detail_density_gate_failed`: observed treatment uses
  dense realistic detail, individual armor scales/rivets, dense folds,
  hatching, microtexture, modeled skin/material shading, or loses sparse
  contour/frame-reproducibility;
- `character_evaluation_color_value_gate_failed`: observed treatment uses
  gradients, cinematic/physical lighting, realistic material rendering,
  nonminimal value masses, or more than two accent hues.

Each gate is observable and independent; one failure is fatal and cannot be
averaged away. Unmeasurable proportion or missing profile/planning evidence is
`insufficient_evidence`, not a guessed visual pass.

Score 100 points only after fatal gates pass:

| criterionId | Category | Max | Minimum for PASS |
| --- | --- | ---: | ---: |
| `char.planning_identity` | Approved identity and required/prohibited fidelity | 20 | 18 |
| `char.proportion_silhouette` | Approved proportion, compact limbs, and silhouette readability | 20 | 18 |
| `char.line_detail_budget` | Sparse line vocabulary and animation-safe detail budget | 20 | 18 |
| `char.color_value_budget` | Minimal flat values and subdued accent-hue budget | 20 | 18 |
| `char.single_image_contract` | Viewpoint, pose, framing, canvas, background, outline, and anchor | 20 | 18 |

PASS requires total >= 90, every category minimum, no fatal gate, and no
Critical finding. The adapter never edits the source or relaxes a failed
profile gate.

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
character_evaluation_proportion_gate_failed
character_evaluation_detail_density_gate_failed
character_evaluation_color_value_gate_failed
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
evaluation_package_input_branch_conflict
evaluation_package_input_branch_incomplete
evaluation_package_unknown_branch_field
evaluation_package_accepted_capture_missing
evaluation_package_accepted_capture_hash_mismatch
evaluation_package_accepted_capture_receipt_mismatch
evaluation_package_accepted_prompt_evidence_mismatch
```

Validate exactly one current version-chain branch, `provider=imagegen`, one closed profile,
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
