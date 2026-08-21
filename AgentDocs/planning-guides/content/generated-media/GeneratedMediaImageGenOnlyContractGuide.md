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
currentExecutionAuthorityPolicy: generated_media_noninteractive_execution_policy_v1
```

Every request created under this contract uses ImageGen. The only current
execution roles are:

```text
character_single_image
icon_single_image
background_single_image
animation
```

PixelLab records and the v1 registry remain immutable legacy evidence. They are
never upgraded in place, selected for a new request, or reinterpreted by this
guide. `ordered_rotation_set`, eight-way character generation, legacy
`imagegen_image`, and provider-specific v1 routes are not current routes.

## 2. Authority and Stage Separation

All current stages inherit
`GeneratedMediaNoninteractiveExecutionPolicyGuide.md`. This is execution-
approval policy only; the schemas and stage boundaries below remain unchanged.

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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
```

## 3. Current Planning Handoff

`generated_media_planning_handoff_v2` requires common immutable identity:

```yaml
schemaVersion: generated_media_planning_handoff_v2
requestId:
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
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
styleReferenceBindings: optional only for character_single_image under section 3.1
identityAnchoredGenerationSelection: optional only for the registered Grade 2 branch under section 3.1
```

`GeneratedMediaPlanningHandoffGuide.md::Closed Planning Snapshot v2` is the
sole authority for the closed `approvedFacts` item schema, source binding/order,
exact hash payload, RFC 8785 JCS UTF-8 bytes, `capturedAt` exclusion and stable
retry timestamp. This summary must not construct an alternate snapshot.

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
styleReferenceBindings: # optional; exactly one only with reviewed durable planning projection v2
  - role: style_only
    projectRelativePath:
    sha256:
    reviewRecordId:
    reviewRecordPath:
    reviewRecordSha256:
identityAnchoredGenerationSelection: # optional; exact seven-member object only for registered Grade 2 regeneration
```

One approved viewpoint produces one image. No direction list, rotation count,
eight-way expansion, or `ordered_rotation_set` is allowed.
The binding is governed by GeneratedMediaStyleReferenceBindingGuide.md. It is
not an identity reference, required character element, pose/equipment source,
or edit target. Three-member style bindings and absolute/transient paths are
invalid.

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

### 3.3 background_single_image

```yaml
backgroundProfile: {profileId:, profileVersion:}
backgroundSpecification:
  sceneContract:
  composition:
  viewpoint:
  horizon:
  depthLayers: non-empty ordered list
  playableOrReadabilityArea:
  subjectInclusions: explicit list or signed none
  subjectExclusions: explicit list or signed none
  canvas: {width:, height:}
  aspectRatio:
  targetDisplay:
  safeArea:
  finalBackgroundPolicy:
  consistencyLock:
    contentIdentity:
    sceneFacts: non-empty
  anchor:
    type: scene_composition_anchor
    framingRegion:
    focalDepth:
```

The profile may register `stage`, `battle`, or `environment`; it never changes
the execution role. Planning owns scene, era, culture, weather, lighting,
landmarks, camera and inclusion/exclusion decisions. Missing facts block. Icon
visual-center, transparent-icon, silhouette/outline and small-size readability
rules are invalid for this type.

### 3.4 animation

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
    referencePromptRecordPath: required for character; prohibited for skill_effect
    referencePromptRecordSha256: required for character; prohibited for skill_effect
    expressionProfileKey: required for character; prohibited for skill_effect
    expressionProfilePayloadHash: required for character; prohibited for skill_effect
    successorExpressionProfileKey: required only for registered composed animation successor
    successorExpressionProfilePayloadHash: required only with successorExpressionProfileKey
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
    animationSourceMode: provider_native_animated_gif
    extractionMode: gif_timeline_exact
```

For `provider_native_animated_gif`, `fixedCellCanvas` is the exact full canvas
of every GIF timeline frame; it is not a multi-cell grid or sheet geometry.

For `animationSubjectType=character`, the four flat reference/profile fields
above are mandatory. `referencePromptRecordPath` identifies an immutable
`generated_media_prompt_v3` character single-image record;
`referencePromptRecordSha256` is the lowercase SHA-256 of its exact file bytes.
The record must contain the canonical `expressionProfilePayload`, and its key
and recomputed payload hash must equal both handoff fields. For
`animationSubjectType=skill_effect`, all four fields are prohibited and absent.

The two successor fields are a closed jointly-present pair. They are permitted
only for the registered bold-outline attack motion-flow successor. In that
branch the four original fields remain the exact immutable bold v2 reference;
the successor pair identifies the separately hashed animation-only payload.
All other branches must omit both fields.

Section 8.3 is the sole token authority for reference/profile authoring
failures. Character animation applies all reference and expression-profile
tokens there; character single-image authoring applies only its expression-
profile tokens; skill animation applies only
`unexpected_character_style_reference`. Do not create a 3.4-local alias.

The router emits one independent normalized `animationRequest` object and
record per source-order `animationRequestId` before authoring. Authoring and
generation reject arrays in the normalized unit, merged requests, and added
motions.

## 4. Provider-Native Animated GIF Source and Extraction Contract

New animation writes use `animationSourceMode=provider_native_animated_gif`
and `extractionMode=gif_timeline_exact`. The provider must return one playable
animated GIF whose timeline already contains the approved final frame count,
order, timing, loop policy, camera, scale, anchor, and background. A still
image, contact sheet, sprite sheet, collage, video, or independently generated
frame set is not an animation source and cannot be converted into a current
animation record.

The approved final frame count is used in the single provider generation
attempt. Oversampling followed by frame selection is prohibited.

```text
one provider-native animated GIF result
-> preserve the exact original GIF bytes and hash
-> close and reopen that GIF
-> validate frame count/order/timing/loop and full-canvas disposal
-> apply only the approved whole-timeline background/outline policy
-> save the normalized completed GIF
-> close and reopen the normalized GIF
-> extract the exact ordered PNG frames from its timeline
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

Generation must use
`providerInterface=configured_animated_gif_capability` and perform a non-submit
capability check for animated GIF output,
timeline timing control, loop control, exact dimensions, and the required
reference roles. If any member is unavailable, return
`animated_provider_capability_unavailable` with `providerCalled=false` and
`submitCount=0`. Prompt prose, a still-image ImageGen call, a sprite sheet, or
post-generation frame synthesis cannot substitute for missing capability.

Existing immutable animation records using `extractionMode=fixed_cell_only`
remain readable historical v2 evidence. They are never rewritten or silently
reinterpreted as provider-native GIF sources. New animation writes with that
legacy extraction mode are forbidden.

## 5. Closed Current Registry

Registry matching uses exact canonical lowercase values. Exactly one row must
match. Zero or multiple rows block.

| rowId | assetType | domainType | profile | authoring prompt | generation prompt | structureProfile |
| --- | --- | --- | --- | --- | --- | --- |
| `character_single_image_v2` | `character_single_image` | `character` | `character_single_image@2.0.0` | `ImageGenCharacterImagePromptAuthoringPrompt.md` | `ImageGenCharacterImageGenerationPrompt.md` | `character_single_image_v2` |
| `skill_icon_single_image_v2` | `icon_single_image` | `skill` | `skill_icon@2.0.0` | `ImageGenIconPromptAuthoringPrompt.md` | `ImageGenIconGenerationPrompt.md` | `icon_single_image_v2` |
| `item_icon_single_image_v2` | `icon_single_image` | `item` | `relic@2.0.0` | `ImageGenIconPromptAuthoringPrompt.md` | `ImageGenIconGenerationPrompt.md` | `icon_single_image_v2` |
| `stage_background_single_image_v2` | `background_single_image` | `stage` | `stage_background@2.0.0` | `ImageGenBackgroundPromptAuthoringPrompt.md` | `ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `battle_background_single_image_v2` | `background_single_image` | `battle` | `battle_background@2.0.0` | `ImageGenBackgroundPromptAuthoringPrompt.md` | `ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `environment_background_single_image_v2` | `background_single_image` | `environment` | `environment_background@2.0.0` | `ImageGenBackgroundPromptAuthoringPrompt.md` | `ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `character_animation_v2` | `animation` | `character` | `character_animation@2.0.0` | `ImageGenAnimationPromptAuthoringPrompt.md` | `ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |
| `skill_animation_v2` | `animation` | `skill` | `skill_animation@2.0.0` | `ImageGenAnimationPromptAuthoringPrompt.md` | `ImageGenAnimationGenerationPrompt.md` | `animation_gif_frame_set_v2` |

All prompt paths are relative to
`AgentDocs/task-prompts/content/generated-media/`. All rows use
`provider=imagegen`. Adding a domain requires a reviewed registry/profile row;
it does not create a domain execution prompt.

Character routing `profileKey` and expression-profile selection are distinct.
For a new character single image, the registry default preserves
`projectbs_character_restrained_ink_line@1.0.0`; the new
`projectbs_character_animation_ready_minimal_ink_line@1.0.0` is selected only
by one exact approved planning fact. Unknown, absent-as-new, multiple, or
conflicting selections are never inferred. Character animation inherits the
exact selection from its immutable reference prompt record. The selected
expression payload and hash remain part of prompt identity, so no existing
record is reinterpreted.

The registered `projectbs_character_bold_outline_compressed_detail@1.0.0`
profile is selected only by one exact approved planning fact and applies only
to `character_single_image_v2`. Its closed planning projection binds compact
proportion, outside/internal outline hierarchy, facial mark budget, compressed
detail budget, and character-specific color signature. It is not a fallback
and is not animation-inheritable.

The registered `projectbs_character_bold_outline_compressed_detail@2.0.0`
successor is likewise explicit and single-image-only. It preserves v1 anatomy,
outline, face, and color bounds while closing total/internal/fold mark ceilings
of 64/56/5, optional ochre only on approved small utility-pouch or
travel-accessory sites, and an explicit disabled-or-enabled translucent ink
halo union. It does not reinterpret v1 or standing provider approval.

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
profile version, complete type specification, normalized request, selected
prompts/structure profile, authoring handoff and optional accepted supersession.
GeneratedMediaRecordGuide.md is the exact authority for the closed
`routingHashPayload`, RFC 8785/JCS bytes, full SHA-256, `gmroute2` ID with a
20-lowercase-hex prefix, canonical record path, closed
`generated_media_routing_index_v2`, byte-identical reuse, collision handling,
record-before-index atomic publication and recoverable orphan-record policy.
Same validated payload reuses the existing bytes; an occupied ID with different
bytes is a collision. Supersession appends and never mutates an older entry. No
v1 record or index is edited.

`generated_media_prompt_v3` has exactly one ImageGen prompt payload.
`generated_media_generation_v2` stores the exact prompt hash, settings,
attempts, cost evidence, and provider refs. Both reject `provider=pixellab` and any PixelLab
payload branch.

```yaml
generated_media_prompt_v3:
  schemaVersion: generated_media_prompt_v3
  promptRecordId:
  promptPayloadSha256:
  requestId:
  assetType: character_single_image
  domainType: character
  contentId:
  planningHandoffPath:
  routingRecordId:
  routingRecordPath:
  routingRecordSha256:
  routingPayloadSha256:
  planningSnapshotHash:
  sourcePlanningFiles: []
  registryVersion: generated_media_authoring_profile_registry_v2
  registryRowId: character_single_image_v2
  profileKey: character_single_image@2.0.0
  provider: imagegen
  structureProfile: character_single_image_v2
  visualBrief:
  visualBriefSha256:
  expressionProfileKey:
  expressionProfilePayload:
  expressionProfilePayloadHash:
  scenePromptOriginal:
  providerPromptPayloadHash:
  providerSettingsIntent:
  providerSettingsIntentSha256:
  requiredElements: []
  prohibitedElements: []
  promptMarkdownPath:
  promptMarkdownSha256:
  status: ready_for_generation
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
  providerExecutionScopeHashPayload:
  providerExecutionApproval:
  providerExecutionApprovalSha256:
  idempotencyKey:
  attempts: []
  costEvidence: []
  providerResultRefs: []
  approvalCostProjection:
  preservationHandoff:
  generationStatus: generated | blocked | failed
  createdAt:
  validation:
```

The prompt schema displayed here is the closed current
`character_single_image` producer projection. GeneratedMediaRecordGuide.md::Prompt
v3 is the sole authority for its exact top-level/nested member sets,
`generated_media_prompt_hash_payload_v3`, JCS/SHA-256 projection, deterministic
ID/paths, raw Markdown bytes, `generated_media_prompt_index_v3`, detached
`generated_media_generation_handoff_v2`, CAS/no-clobber publication,
idempotent `reused_identical`, collision/orphan handling, and failure
atomicity. Unknown fields are rejected. A blocked result is returned as a task
result and is never serialized as a `generated_media_prompt_v3` record.

Other asset types retain their type-specific readiness rules but cannot infer,
copy, or widen this character record projection. A producer that cannot resolve
an exact closed schema for its own type returns `unsupported_record_schema`.
Animation records without one scalar animationRequestId, or non-animation
records containing it, are invalid.

### 6.1 Closed provider execution approval contract

This subsection is the sole current authority for approving an external
ImageGen call. A natural-language user approval is evidence of intent, not the
contract object and not a request for the user to calculate a hash. The
execution role MUST validate the prompt record and files, resolve the exact
provider settings, construct the scope payload below, calculate its hash, and
show the user the canonical scope identity plus proposed attempt/cost limits.
The user approves that presented hash and envelope. The execution role then
records who approved it and when in the closed object. It MUST NOT accept a
user-supplied hash that it has not independently recomputed.

Settings resolution and estimation use a read-only operation on
`configured_imagegen_capability`. It accepts the validated
`providerSettingsIntent` and returns the defaults-resolved settings and estimate
without crossing the provider submit boundary, allocating a provider operation,
uploading prompt media, reserving capacity, or incurring cost. Its response is
requested with this closed object:

```yaml
schemaVersion: generated_media_imagegen_capability_preflight_request_v1
mode: non_submit
provider: imagegen
providerTool: imagegen
providerInterface: configured_imagegen_capability
assetType: character_single_image | icon_single_image | background_single_image | animation
providerSettingsIntent: exact verified value from the prompt record
providerSettingsIntentSha256: recomputed SHA-256 of canonicalJson(providerSettingsIntent)
```

Unknown, missing, `null`, or differently typed request members reject as
`provider_capability_preflight_invalid` before the capability is accessed. The
capability returns the following closed object;
unknown, missing, `null`, or differently typed
members reject as `provider_capability_preflight_invalid`:

```yaml
schemaVersion: generated_media_imagegen_capability_preflight_v1
mode: non_submit
submitBoundaryCrossed: false
capabilityDescriptor:
  schemaVersion: generated_media_imagegen_capability_descriptor_v1
  provider: imagegen
  providerTool: imagegen
  providerInterface: configured_imagegen_capability
  capabilityVersion: non-empty immutable deployed-capability version
  settingsDescriptorVersion: non-empty immutable defaults/schema version
  costDescriptorVersion: non-empty immutable pricing/estimation version
capabilityDescriptorSha256: SHA-256 of canonicalJson(capabilityDescriptor)
providerSettings: defaults-resolved exact closed provider request settings object
providerSettingsSha256: SHA-256 of canonicalJson(providerSettings)
estimate: one closed tagged estimate below
evidenceRef: non-empty immutable reference to the complete preflight evidence
```

The operation returns one response or no response; it never silently omits
defaults or substitutes display settings. Missing capability support returns
`provider_capability_descriptor_unavailable`. `evidenceRef` must resolve to
immutable evidence containing the complete response and descriptor versions;
a mutable log URL, prose summary, guessed price, or synthesized zero-cost value
is invalid. `providerSettings` and both hashes are computed by the capability,
then independently canonicalized and recomputed by the execution role.
The repository fixed vector validates the closed transport and hashing rules;
it does not declare production ImageGen defaults or prices. The external owner
must maintain descriptor-versioned golden mappings from every supported intent
to its exact settings and estimate response.

`providerExecutionScopeHashPayload` is a closed object with exactly these
members. `animationRequestId` is conditionally present, never `null`; it is
required only for animation and forbidden otherwise. `providerSettings` is the
exact closed JSON object submitted to the configured capability, after defaults
are resolved. No display text, user approval wording, limits, estimates,
timestamps, attempts, provider results, or filesystem absolute paths occur in
this payload.

```yaml
schemaVersion: generated_media_provider_execution_scope_hash_payload_v1
requestId:
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
contentId:
animationRequestId?: required only for animation; forbidden otherwise
planningSnapshotHash: verified 64-lowercase-hex SHA-256
registryRowId:
structureProfile:
promptRecordId:
promptRecordSha256: SHA-256 of exact prompt record file bytes, including its final LF
promptFileSha256: SHA-256 of exact copy-ready prompt file bytes, including its final LF
providerPromptPayloadHash: verified hash stored by generated_media_prompt_v3
provider: imagegen
providerTool: imagegen
providerInterface: configured_imagegen_capability
capabilityDescriptor: exact closed descriptor returned by the non-submit preflight
capabilityDescriptorSha256: SHA-256 of canonicalJson(capabilityDescriptor)
providerSettings: exact closed provider request settings object
providerSettingsSha256: SHA-256 of canonicalJson(providerSettings)
```

The request, asset/domain/content identity, optional animation identity,
snapshot, selected registry/profile, prompt record identity and exact bytes,
copy-ready prompt bytes, provider payload, provider/tool/interface,
capability/settings/cost descriptor versions, and exact settings are all
approval bindings. Changing any bound value requires a new
scope payload and approval. `promptRecordSha256` and `promptFileSha256` are file
hashes and therefore include the one required trailing LF; the payload and
settings hashes do not. Calculate exactly:

```text
providerSettingsSha256 = lowercase_hex(SHA256(canonicalJson(providerSettings)))
scopeHash = lowercase_hex(SHA256(canonicalJson(providerExecutionScopeHashPayload)))
```

`canonicalJson` is RFC 8785 JCS encoded as UTF-8 without BOM and without a
trailing LF. The JSON file containing a record is JCS bytes followed by exactly
one `0A` byte. Hash comparison is exact-byte comparison; line-ending
normalization or semantic JSON equality cannot repair a file-hash mismatch.

`providerExecutionApproval` is a closed object with exactly these eight
members; unknown, missing, `null`, or differently typed members reject:

```yaml
schemaVersion: generated_media_provider_execution_approval_v1
approvedBy: non-empty stable user/account identifier
approvedAt: RFC 3339 timestamp with explicit offset
scopeHash: recomputed 64-lowercase-hex value above
maxAttempts: JSON integer from 1 through 2147483647
maxCost: closed CostLimit value below
estimateUnavailablePolicy: block | allow_upper_bound
approvalEvidence: non-empty immutable message/thread reference, not approval prose copied into the scope
```

`providerExecutionApprovalSha256` is
`lowercase_hex(SHA256(canonicalJson(providerExecutionApproval)))`; limits are in
this approval-envelope hash but are deliberately excluded from `scopeHash`.
Renewing limits for identical execution inputs preserves `scopeHash` and creates
a different approval SHA. A generation identity includes both hashes. A changed
scope cannot reuse or renew the old approval.

`CostLimit` is one of the following exact tagged unions. Decimal amounts are
JSON strings matching `^(0|[1-9][0-9]*)\\.[0-9]{6}$`; they have exactly six
fractional digits, no sign, exponent, commas, leading zero, or alternate
normalization. Comparison converts the string to integer millionths. Values may
be compared only when the tag and all unit identity members match exactly.
`currency` is an uppercase ISO 4217 alpha code. Provider and unit identifiers
are exact lowercase registered values.

```json
{"kind":"no_charge"}
{"amount":"0.250000","currency":"USD","kind":"iso_currency"}
{"amount":"2.000000","creditUnit":"image_credit","kind":"provider_credit","provider":"imagegen"}
{"amount":"1.000000","kind":"provider_unit","provider":"imagegen","unit":"image"}
```

For every logical attempt, `configured_imagegen_capability` returns one closed
preflight estimate: `{"status":"no_charge"}`,
`{"status":"exact","cost":CostLimit}`, `{"status":"upper_bound","cost":CostLimit}`,
or `{"status":"unavailable"}`. `no_charge` is approved by any valid limit and
records no charge. `exact` and `upper_bound` require identical units and an
amount not greater than `maxCost`; `maxCost.kind=no_charge` rejects either.
`unavailable` blocks as `provider_cost_estimate_unavailable` unless policy is
`allow_upper_bound` and `maxCost` is not `no_charge`; in that case the whole
`maxCost` is the approved per-attempt upper bound. The total authorization is
therefore at most `maxAttempts` times the per-attempt limit, but multiplication
is derived and never serialized as a decimal JSON number.

Actual cost evidence is one of `no_charge`, `exact` with a comparable
`CostLimit`, or `unavailable`, and includes an immutable provider evidence
reference. An exact actual amount above the per-attempt limit records the
incurred amount and fails `provider_cost_limit_exceeded`; it is never hidden or
clamped. When actual cost is unavailable after a call, record `unavailable` and
the approved upper bound reserved for that attempt. The result cannot advance
to preservation until cost evidence is reconciled; return
`provider_actual_cost_unavailable`, `providerCalled=true`, and
`safeToRetry=false`. This rule preserves the provider result/ref and never
authorizes a duplicate call merely to obtain cost data.

The exact tagged JSON shapes are:

```json
{"status":"no_charge"}
{"cost":{"amount":"0.100000","currency":"USD","kind":"iso_currency"},"status":"exact"}
{"cost":{"amount":"0.250000","currency":"USD","kind":"iso_currency"},"status":"upper_bound"}
{"status":"unavailable"}
```

`upper_bound` is valid only for an estimate; actual cost permits only
`no_charge`, `exact`, or `unavailable`. Unknown fields reject in every tagged
value.

Immediately before submit, generation obtains a fresh non-submit preflight and
revalidates its closed schema, descriptor hash, settings hash, and estimate.
If the descriptor or settings hash differs from the approved scope, it returns
`provider_capability_drift`, `providerCalled=false`, and `safeToRetry=false`;
the changed scope must be recomputed, presented, and freshly approved. A changed
estimate with unchanged descriptor/settings is rechecked against the approved
tagged limit and either proceeds or returns the existing cost blocker. The
fresh immutable `evidenceRef` is recorded in `costEvidence`; it is evidence, not
a scope member. No drift path may fall back to guessed settings, a cached price,
`unavailable`, or `no_charge`.

`maxAttempts` is the maximum count of logical provider attempts for one
`scopeHash`, accumulated across every renewal. Attempt numbers start at 1 and
are contiguous. Crossing the submit boundary sets `providerCalled=true` and
consumes that logical attempt even when the provider rejects, fails, times out,
or returns an ambiguous outcome. Preflight validation/cost blocks, an active
duplicate block, and reuse of an identical completed result consume zero
attempts. An ambiguous submission is retried only with the same attempt number
and idempotency key until resolved; it is not a new logical attempt. A new
logical retry uses the next number only after the prior attempt is terminal.

```text
idempotencyKey = gmexec1.{scopeHash}.a{decimalAttemptNumber}
```

An approval renewal never resets consumed attempts. Raising `maxAttempts` can
authorize the next contiguous attempt; lowering it cannot invalidate evidence
already recorded but blocks new calls when consumed attempts are at or above
the new limit. Changing prompt bytes, payload, settings, provider identity, or
asset/domain/content identity changes `scopeHash`, requires fresh approval, and
cannot be treated as an idempotent retry.

Approval validation uses only these failure types and retry meanings:

| failureType | providerCalled | safeToRetry |
| --- | --- | --- |
| `missing_provider_execution_approval` | false | false until the computed scope/envelope is approved |
| `invalid_provider_execution_approval` | false | false until a closed valid approval replaces it |
| `provider_execution_scope_mismatch` | false | false; recompute/present the changed scope and obtain fresh approval |
| `provider_capability_descriptor_unavailable` | false | false until the configured capability exposes the closed non-submit descriptor response |
| `provider_capability_preflight_invalid` | false | false until the capability returns a closed hash-valid non-submit response and immutable evidence reference |
| `provider_capability_drift` | false | false; recompute/present the changed descriptor/settings scope and obtain fresh approval |
| `provider_cost_unit_mismatch` | false | false until estimate and approval use the same tagged unit |
| `provider_cost_estimate_unavailable` | false | false until estimate exists or a renewed approval allows its upper bound |
| `provider_cost_limit_exceeded` | false for preflight; true for actual overage | false until renewed approval for a future attempt; never retry the over-limit completed call |
| `provider_actual_cost_unavailable` | true | false until billing evidence is reconciled; never call again for evidence |
| `retry_limit_exceeded` | false | false until same-scope renewal raises the cumulative limit |
| `duplicate_provider_call_risk` | false | false until the active attempt resolves; then reuse or evaluate the next attempt |
| `provider_operation_failed` | true only after submit | true only when the prior attempt is terminal, no active duplicate remains, cost evidence is complete, and another attempt is approved |

`safeToRetry` is the truth at return time, not a promise that a later approval
will be granted. Validation failures create no provider call. A failure after
submit preserves its consumed-attempt, operation, result, and cost evidence.

#### 6.1.1 Hosted built-in preview execution v1

`hosted_builtin_preview_v1` is a separate, closed, preview-only execution mode
for the callable built-in ImageGen surface that exposes submit but exposes no
non-submit capability descriptor, defaults-resolved settings descriptor, cost
estimate, or immutable provider evidence reference. Absence is recorded as
absence; no descriptor/version/default/price/evidenceRef is synthesized.

This mode does not weaken `generated_media_generation_v2`. A promotable call
continues to require the complete section 6.1 descriptor, approval and cost
contract. A preview result is never a generation-v2 result and cannot enter
preservation, evaluation-package construction, Unity, or promotion.

Hosted preview authorization has two closed branches. Manual exact-scope
approval remains valid. A standing automatic policy may instead authorize the
execution role to derive one exact-scope attestation after all prompt,
reference, and settings-seal hashes are final. Neither branch authorizes a
batch, retry, promotion, preservation, or evaluation.

One authenticated current user message may manually authorize exactly one work
unit:

```text
character_single_image -> exactly one image
animation -> exactly one scalar animationRequestId
```

The message must identify the exact request/content/prompt scope and explicitly
ask for that one output. A general request, older approval, batch wording,
multiple animation IDs, or inferred consent is invalid. The closed approval is:

```yaml
schemaVersion: generated_media_hosted_preview_approval_v1
executionMode: hosted_builtin_preview_v1
approvedBy: authenticated current user identity
approvedAt: RFC 3339 timestamp with offset
approvalEvidence: immutable current message/thread reference
previewScopeHash: recomputed 64-lowercase-hex
workUnitType: exact_single_image | exact_animation_request
animationRequestId: required only for exact_animation_request; forbidden otherwise
submitCountMaximum: 1
retryCountMaximum: 0
promotionPolicy: not_promotable
```

The execution role computes `previewScopeHash` over a closed payload containing
the exact request/asset/domain/content identity, conditional animationRequestId,
planning snapshot hash, prompt record/file/payload hashes, reference roles and
hashes, provider=`imagegen`, providerTool=`built-in_imagegen`,
executionMode=`hosted_builtin_preview_v1`, and `settingsSealSha256`. The
settings seal contains exactly the authored canvas/background/output intent and
the options actually exposed on the callable tool invocation. An option not
exposed by the tool is absent, never `null`, guessed, or defaulted:

For a `role=style_only` reference, generation first rehashes the durable asset,
review record, and review index; verifies purpose/status/profile scope and every
prohibited transfer; and binds the full six-member object into preview scope.
The callable provider surface must expose an independently selectable style-
reference role. A generic image or identity reference input is not equivalent;
when role separation is unavailable, generation stops before submit with the
applicable existing capability/unknown-setting blocker. The prompt never
describes the depicted reference subject.

```yaml
schemaVersion: generated_media_hosted_preview_settings_seal_v1
providerSettingsIntent: exact verified authoring value
providerSettingsIntentSha256: recomputed canonical hash
exposedOptions: exact closed options actually exposed by the callable tool
exposedOptionsSha256: recomputed canonical hash
capabilityDescriptorStatus: unavailable_on_callable_surface
settingsDescriptorStatus: unavailable_on_callable_surface
costEstimate: {status: unavailable}
```

`exposedOptions` is a control-coverage claim, not a list of desired values.
Before approval or submit, every provider-time member of
`providerSettingsIntent` (`canvas`, `generationBackground`, and `outputFormat`
for the current character record) MUST have an exact same-value control on the
callable surface and an exact projection in `exposedOptions`. Prompt prose is
not a substitute for a missing callable control. If any provider-time member
is absent, differently shaped, or only assumed from a hosted default, return
`hosted_preview_unknown_setting`, `providerCalled=false`, and `submitCount=0`.
This makes an empty or partial `exposedOptions` object truthful but non-
submittable for an exact-settings request.

Preview owns no background removal or other downstream transformation. If the
immutable provider prompt simultaneously asks for a removable solid generation
background and a transparent final/background-removed result, or otherwise
depends on a preservation-stage transformation, return
`hosted_preview_prompt_stage_semantics_conflict` before submit. Generation does
not repair the prompt or choose one side. A prompt that merely forbids halo,
vignette, scene, or shadow still expresses intent only; the six open-ink-wash
semantic gates prove instruction conformance, not provider-output conformance.
With zero retries and no evaluation, they cannot predict or certify that the
returned pixels will follow those instructions.

##### 6.1.1.1 Standing automatic preview approval policy v1

`generated_media_hosted_preview_auto_approval_policy_v1` is an immutable,
authenticated user policy. It is not a precomputed approval for an unknown
scope. It permits the execution role to create a detached exact-scope
attestation only when the final recomputed scope satisfies every closed policy
predicate. The authenticated source is bound without requiring an unavailable
hosted-platform account ID: the current task supplies its stable thread ID and
the SHA-256 of the exact UTF-8 user instruction bytes. The app authentication
context establishes that those bytes are a current-user instruction; copied
prose, an assistant message, or a thread summary is invalid.

```yaml
schemaVersion: generated_media_hosted_preview_auto_approval_policy_v1
policyId: gmpreviewpolicy1.{policyPayloadSha256[0:20]}
policyPayloadSha256: SHA-256 of JCS({schemaVersion, authorizationSource, policyScope, lifetime})
authorizationSource:
  type: authenticated_thread_user_instruction
  threadId: stable non-empty current task ID
  instructionSha256: SHA-256 of exact UTF-8 current-user instruction bytes
policyScope:
  provider: imagegen
  executionMode: hosted_builtin_preview_v1
  assetTypes: non-empty unique ordered subset of current asset types
  domainTypes: non-empty unique ordered subset of current domain types
  contentIds: non-empty unique ordered exact content IDs
  workUnitTypes: non-empty unique ordered subset of exact_single_image | exact_animation_request
  referencePolicy: prompt_bound_only
  submitCountMaximumPerScope: 1
  retryCountMaximumPerScope: 0
  costPolicy: allow_unavailable_preview_only
  promotionPolicy: not_promotable
  preservationPolicy: not_preservable
  evaluationPolicy: not_evaluated
lifetime: until_revoked
```

Unknown, missing, `null`, differently typed, duplicate, wildcard, empty, or
out-of-registry members reject as `invalid_hosted_preview_auto_approval_policy`.
`contentIds`, asset/domain/work-unit membership are exact; `*`, regex, prefix,
and inferred expansion are forbidden. Policy issuance itself crosses no
provider boundary and creates no submit, upload, allocation, reservation, or
cost. Revocation is fail-closed: a revoked policy hash cannot attest a new
scope. Existing consumed submits remain consumed.

After the final `previewScopeHash` is independently recomputed, the execution
role may derive exactly this detached object:

```yaml
schemaVersion: generated_media_hosted_preview_auto_approval_attestation_v1
executionMode: hosted_builtin_preview_v1
policyId: exact policy ID
policyPayloadSha256: exact recomputed policy payload hash
authorizationSourceSha256: SHA-256 of JCS(policy.authorizationSource)
previewScopeHash: exact recomputed current scope hash
workUnitType: exact_single_image | exact_animation_request
animationRequestId: required only for exact_animation_request; forbidden otherwise
submitCountMaximum: 1
retryCountMaximum: 0
promotionPolicy: not_promotable
```

The attestation is valid only if the policy is not revoked; the scope asset,
domain, content and work-unit type are exact policy members; all references are
already hash-bound by the prompt and scope; and submit/retry state is still
zero. `hostedPreviewApprovalSha256` hashes the selected manual approval or this
automatic attestation. No additional user message is required after a valid
standing policy is supplied. Prompt, reference, settings-seal, request, or
identity drift invalidates the attestation and forces a fresh scope
derivation; it never reuses the old scope hash.

The first complete validation pass may derive one in-memory, task-local
`generated_media_generation_preflight_receipt_v1`. It binds the authoritative
commit, request/work-unit identity, prompt JSON/Markdown/payload hashes,
provider-settings-intent and settings-seal hashes, reference-bindings hash,
expression-profile payload hash, semantic-gate result, and observed
submit/retry state. The receipt is never persisted, handed to another task, or
used after authority/input changes.

Immediately before submit, re-read only the receipt's exact drift anchors and
the current preview/active-attempt state. Matching anchors reuse the receipt's
already completed closed-schema/profile/semantic validation instead of
re-reading guides or re-emitting full payloads. Prompt, reference, settings,
identity, authority, or attempt drift invalidates the receipt and applies the
existing exact blocker or requires one fresh complete validation pass; it does
not authorize submit. A task MUST NOT run the same full semantic validation
again merely to produce a second transcript.

Check that no preview record exists and no active submission is associated
with the same scope. `submitCountMaximum=1` and
`retryCountMaximum=0` are constants: failure, timeout or ambiguity does not
authorize another call. Additional output or retry requires a new exact scope.
The new scope requires either a new manual current-user approval or a fresh
automatic attestation from a still-valid matching standing policy.

After the one submit, save only observable returned media to:

```text
output/generated-media-preview/v1/{assetType}/{contentId}/{workUnitId}/original.{ext}
```

`workUnitId` is `single_image` or the exact animationRequestId. A provider
absolute/transient path may be audit text only and never canonical identity.
The preview record stores the relative output path/hash, tool mode,
submitCount=1, retryCount=0, `costKnown=false`, and these literal states:

```text
preview_only=true
not_promotable=true
not_evaluated=true
capabilityEvidenceStatus=unavailable_on_callable_surface
settingsEvidenceStatus=exposed_options_only
costEvidenceStatus=unavailable_on_callable_surface
```

The canonical record/path/index contract is owned by
GeneratedMediaRecordGuide.md::Hosted Preview v1. Attempting to use this record
or media as preservation, canonical generation, evaluation-package, Unity, or
promotion input fails at that entry boundary. The descriptor blocker is not a
preview precondition; it remains mandatory at the promotable
generation/preservation boundary.

Preview uses only these additional central failure tokens:

| failureType | meaning |
| --- | --- |
| `missing_hosted_preview_approval` | no current exact authenticated approval |
| `invalid_hosted_preview_approval` | approval shape, work unit, limits or evidence is invalid |
| `missing_hosted_preview_auto_approval_policy` | automatic approval was selected without an authenticated closed policy |
| `invalid_hosted_preview_auto_approval_policy` | policy shape, identity, hash, predicates or authenticated source is invalid |
| `hosted_preview_auto_approval_policy_mismatch` | final scope is outside the policy's exact asset/domain/content/work-unit predicates |
| `hosted_preview_auto_approval_policy_revoked` | policy hash is revoked and cannot attest a new scope |
| `hosted_preview_scope_mismatch` | approval hash does not equal recomputed current scope |
| `hosted_preview_unknown_setting` | execution would require an unexposed or invented setting/default |
| `hosted_preview_prompt_stage_semantics_conflict` | immutable prompt requires both provider-time solid background and preview-forbidden downstream removal/transparent-final semantics |
| `hosted_preview_prompt_drift` | prompt record/file/payload changed before submit |
| `hosted_preview_reference_drift` | reference role/path/hash changed before submit |
| `hosted_preview_submit_limit_exceeded` | one submit was already consumed |
| `hosted_preview_retry_forbidden` | retry was requested after any submit outcome |
| `hosted_preview_output_missing` | submit returned no observable media |
| `hosted_preview_output_hash_mismatch` | saved observable bytes do not match the record hash |
| `hosted_preview_preservation_forbidden` | preview entered preservation or evaluation packaging |
| `hosted_preview_promotion_forbidden` | preview entered canonical generation, Unity or promotion |

All preview blockers preserve truthful submitCount, `costKnown=false`, and the
unavailable evidence statuses.

#### 6.1.2 Hosted built-in fast preview orchestration v1

`hosted_builtin_fast_preview_v1` is an additive, non-promotable execution mode
for measuring one official preview from an already-authoritative ImageGen
single-image unit. It does not change `hosted_builtin_preview_v1` or
`promotable_generation_v2`; their publication, schema, capability, settings,
cost, preservation, evaluation-package, and promotion gates remain exact.

The routing role owns one bounded orchestration in this mode: resolve the
compact authority pointers, prepare the callable prompt/reference projection,
invoke the official generation role once, visually inspect the returned image,
and return one terminal receipt. It MUST NOT rewrite or republish planning,
routing, authoring, or prompt records when an authoritative planning request
and prompt record already exist. It MUST NOT create a replacement prompt
record merely to append provider-option prose.

Before provider submit, exactly three blocker classes are allowed:

1. `fast_preview_duplicate_submit_risk`: the deterministic idempotency key is
   active, completed, ambiguous, or otherwise cannot prove that no provider
   call or charge has occurred;
2. `fast_preview_authority_or_safety_violation`: authenticated execution
   authority is missing/invalid, the requested provider/tool/role is outside
   the approved ImageGen single-image scope, the reference attempts identity,
   edit-target, person, pose, action, clothing, or equipment transfer, or a
   safety policy forbids execution; and
3. `fast_preview_callable_input_absent`: no executable non-empty prompt text
   or no readable reviewed durable reference image is available.

All other pre-submit discrepancies are non-blocking `backlogWarnings`. This
includes contract/schema projection disagreement, missing pre-preview Git
publication, incomplete full-suite validation, unavailable capability or cost
attestation, and unavailable exact canvas/background/output-format/structured
style-only callable controls. A warning MUST NOT be relabeled as one of the
three blockers. Conversely, the three blockers MUST NOT be waived by this
mode.

The compact input pointer is closed to exactly:

```yaml
schemaVersion: generated_media_fast_preview_pointer_v1
authoritativeMainSha: 40-lowercase-hex Git commit
requestId: exact existing planning request ID
promptRecordId: exact existing prompt record ID
promptRecordSha256: exact raw Git-blob hash
referencePath: exact reviewed durable project-relative asset path
referenceSha256: exact raw asset hash
idempotencyKey: gmfastpreview1.{20-lowercase-hex}
```

The pointer contains no planning payload, routing/authoring handoff body,
prompt prose, profile payload, or media bytes. The consumer resolves available
authoritative records from these anchors. A record/schema conflict discovered
during resolution is a warning when executable prompt text and the reviewed
reference remain unambiguous; it is a hard blocker only when it causes one of
the three conditions above.

The idempotency payload contains exactly `schemaVersion`,
`authoritativeMainSha`, `requestId`, `promptRecordId`, `promptRecordSha256`,
`referencePath`, and `referenceSha256`. Its schemaVersion is
`generated_media_fast_preview_idempotency_payload_v1`, and:

```text
idempotencyPayloadSha256 = SHA-256(JCS(idempotencyPayload))
idempotencyKey = gmfastpreview1.{idempotencyPayloadSha256[0:20]}
```

Before crossing the provider boundary, the orchestrator checks active and
completed receipts for this exact key. `absent` is the only submit-eligible
state. `active`, `completed`, `ambiguous`, dangling, or divergent evidence is
`fast_preview_duplicate_submit_risk`. A completed result may be returned as a
no-call reuse receipt, but it never authorizes another submit. Provider timeout,
failure, or uncertain return consumes the single submit.

Callable input is exactly non-empty prompt text plus one reviewed reference
image. Desired canvas, removable solid background, output format, and
style-only semantics may be preserved in prompt prose. When an option is not
exposed by the callable surface, the orchestrator records its name in
`unavailableCallableControls` and later compares the output against the intent;
it does not block and does not claim provider enforcement. Capability endpoint,
cost descriptor, exact canvas control, exact background control, exact output
format control, and structured style-only parameter are never synthesized.

The provider boundary permits `submitCountMaximum=1` and
`retryCountMaximum=0`. The routing orchestrator calls the official generation
role with the exact compact pointer and sealed callable prompt/reference only.
Generation returns the observed output path, raw SHA-256, byte length, MIME,
pixel dimensions, exposed provider result reference if any, and truthful
provider/submit/retry/cost-known state. It performs no retry, edit,
preservation, evaluation package, promotion, or Unity work.

Immediately after a successful return, the routing orchestrator performs one
visual preview inspection and includes it in the same terminal receipt. This is
not the strict evaluation-package contract. The closed observation is:

```yaml
visualEvaluation:
  scope: preview_visual_observation_only
  status: observed | unavailable
  summary: non-empty concise observation, or exact reason when unavailable
  intentWarnings: ordered unique short warning tokens
  adoptedByUser: false
  strictEvaluationPerformed: false
```

An observed defect never triggers retry. Only a later explicit user adoption
may start a new strict preservation/evaluation/promotion workflow; the preview
receipt itself cannot satisfy any of those input schemas.

The terminal receipt is closed to exactly these members, with optional members
present only where stated:

```yaml
schemaVersion: generated_media_fast_preview_terminal_receipt_v1
state: preview_complete | completed_reuse | blocked | submit_failed_no_retry
authoritativeMainSha:
requestId:
promptRecordId:
promptRecordSha256:
referencePath:
referenceSha256:
idempotencyKey:
providerCalled: boolean for this orchestration
submitCount: 0 | 1 for this orchestration
historicalSubmitCount: 0 | 1
retryCount: 0
costKnown: boolean
cost?: provider-observed value only when costKnown=true
previewOnly: true
notPromotable: true
notPreserved: true
strictEvaluationPerformed: false
unavailableCallableControls: ordered unique strings
backlogWarnings: ordered unique strings
outputObservation?: required for preview_complete or completed_reuse
visualEvaluation?: required for preview_complete or completed_reuse
failureType?: one exact blocker/failure token
nextStep: terminal | await_user_adoption
```

`outputObservation` contains exactly `path`, `sha256`, `byteLength`,
`mimeType`, `width`, `height`, and optional `providerResultRef`. Numeric cost is
forbidden when `costKnown=false`; unavailable cost is not zero. A blocked
receipt has `providerCalled=false`, `submitCount=0`, and one of the three hard
blocker tokens. A submit failure has `providerCalled=true`, `submitCount=1`,
`retryCount=0`, `failureType=fast_preview_submit_failed_no_retry`, and never
re-enters preflight. A successful new preview has exactly one submit and one
visual observation. One child final and one parent relay are the complete
control-plane path; observers receive no full relay.

This mode adds only these failure tokens:

```text
fast_preview_duplicate_submit_risk
fast_preview_authority_or_safety_violation
fast_preview_callable_input_absent
fast_preview_submit_failed_no_retry
```

#### 6.1.3 Accepted post-result capture v1

`accepted_post_result_capture_v1` is an additive, no-provider recovery mode for
an exact generated or fast-preview result that the authenticated user has
explicitly accepted. It does not turn the historical execution into
`generated_media_generation_v2`, does not assert that any pre-submit gate
passed, and does not change `hosted_builtin_preview_v1`,
`hosted_builtin_fast_preview_v1`, `promotable_generation_v2`, or either
animation source mode. The capture producer only verifies and seals already
existing evidence.

For `assetType=animation`, the mode remains eligible only when all of the
following are available and mutually consistent:

- one authenticated acceptance message naming the exact accepted artifact;
- exact task and provider tool-call identity proving the historical submit and
  retry counts;
- raw-byte-verifiable prompt, settings, every submitted reference, provider
  master/result, completed GIF, and every extracted frame, each with path and
  SHA-256;
- the immutable request, animation, routing and planning snapshot identities;
- no existing record with the derived ID except byte-identical reuse.

For `assetType=character_single_image`, the additive still-image branch instead
requires one authenticated acceptance naming one exact PNG SHA-256, one
raw-byte-verifiable PNG source, and its no-clobber project-relative canonical
capture target. Historical execution, prompt, settings, capability, and cost
evidence may each be `unavailable_observed` with `claim=not_claimed`; they are
never invented and no historical submit, retry, provider, or pre-submit PASS is
inferred. The accepted bytes acquire only the distinct
`accepted_project_candidate` capture role. A prior
`visual_reference_only_not_identity_or_edit_target` role is not promoted or
reinterpreted, and the capture grants no identity or edit-target authority.
The branch has exactly one PNG result member and forbids every animation
master/GIF/frame member.

Capture performs no provider/capability/cost call. For the capture action,
`providerCalled=false`, `submitCount=0`, and `retryCount=0`; historical one-call
facts remain separately recorded. Capability and cost states are exactly
`unavailable_observed`. They mean that the capture observed no attestation and
MUST NOT be rewritten as `supported`, `passed`, `zero`, or a guessed amount.
`preSubmitGateAttestation=not_claimed_post_result_capture` is mandatory.

The closed record, index, receipt, path, JCS hash, idempotency and no-clobber
contract is owned by GeneratedMediaRecordGuide.md::Accepted post-result capture
v1. Its record schema is `generated_media_accepted_result_capture_v1`. A valid
capture authorizes only preservation and strict evaluation-package
construction. It never authorizes promotion. Project promotion continues to
require a strict evaluation `PASS` plus explicit project mapping under the
existing promotion contract.

This mode adds only these failure tokens:

```text
accepted_capture_acceptance_missing
accepted_capture_execution_evidence_missing
accepted_capture_identity_mismatch
accepted_capture_evidence_hash_mismatch
accepted_capture_incomplete_member_set
accepted_capture_false_attestation
accepted_capture_record_collision
accepted_capture_index_cas_failed
accepted_capture_promotion_forbidden
accepted_capture_canonical_target_collision
```

#### 6.1.4 Authenticated built-in opaque-chroma single-submit v1

`builtin_imagegen_authenticated_single_submit_v1` is a separate additive
generation mode for the exact registered
`projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0` profile on
`character_single_image_v2`. It is selected only when the executable surface is
the built-in `image_gen.imagegen` call exposing exactly `prompt`,
`referenced_image_paths`, and `num_last_images_to_include`, and no configured
non-submit capability/settings/cost interface exists. Its closed callable,
preflight, approval, idempotency, output-conformance, receipt, and failure-token
contract is owned by
`GeneratedMediaBuiltinImagegenAuthenticatedGenerationGuide.md`.

This mode does not change or satisfy the descriptor contract of
`promotable_generation_v2`, and does not change either hosted preview mode.
Capability/settings/cost descriptors and provider enforcement of canvas,
background, or format are recorded as unavailable, never invented. The exact
1024x1536 PNG and opaque uniform `#00FF00` field remain immutable prompt intent
and post-return hard gates. A conforming provider master proceeds only to the
distinct `generated_media_chroma_uncomposite` role; a nonconforming return
consumes the one submit and stops without retry. The execution-local preflight,
approval, and receipt are detached and add no member to any published planning,
routing, or prompt handoff.

#### 6.1.5 Authenticated identity-anchored opaque-chroma single-submit v1

`builtin_imagegen_authenticated_identity_anchored_single_submit_v1` is a
disjoint additive branch owned by
GeneratedMediaIdentityAnchoredOpaqueChromaExecutionGuide.md. It keeps the
opaque-chroma expression profile and callable schema unchanged but requires the
exact hash-significant `identityAnchoredGenerationSelection` from planning
through generation. The actual call is exactly prompt plus one
`referenced_image_paths` entry for the registered project identity/equipment
authority. That reference is neither a style-only binding nor an edit source and
requires no provider receipt. The branch has one fresh scope, submit maximum 1,
retry maximum 0, no fabricated capability/settings/cost evidence, post-return
identity/equipment and opaque-chroma hard gates, and a distinct later
`generated_media_chroma_uncomposite` boundary. Existing built-in, preview,
strict, and source-bound-edit modes are unchanged.

### 6.2 Approval and cost projection

`costEvidence` is an ordered append-only array. Each entry is a closed object
containing `scopeHash`, `attemptNumber` (or `0` for a no-call event),
`providerCalled`, `event` (`preflight_blocked`, `submitted`, `terminal`, or
`completed_reuse`), `estimate`, `actualCost`, `approvedUpperBound`,
`providerOperationRef` (required after a provider supplies one),
`evidenceRef`, and `recordedAt`. Conditional values are absent, never `null`.
Every submitted attempt has one terminal entry before preservation handoff.
`estimate` and `approvedUpperBound` are required for a submitted entry;
`actualCost`, `providerOperationRef`, and provider `evidenceRef` are required on
its terminal entry, except that a provider that created no operation omits only
`providerOperationRef`. A preflight block requires `estimate` and
`evidenceRef`, and forbids actual/provider-operation members. Completed reuse
requires the reused generation record as `evidenceRef` and forbids all cost and
operation members. `recordedAt` is an evidence timestamp and is excluded from
scope and approval identity, never synthesized into provider billing evidence.

Calculate `costEvidenceSha256` over `canonicalJson(costEvidence)`. The closed
`approvalCostProjection` contains exactly:

```yaml
scopeHash:
providerExecutionApprovalSha256:
maxAttempts:
maxCost:
estimateUnavailablePolicy:
attemptsConsumed:
costEvidenceSha256:
actualCostStatus: no_charge | exact | unavailable
actualCostTotal?: required only for exact; one CostLimit in the approval unit
```

The projection in the generation record, its generation index entry, and the
closed `preservationHandoff.approvalCostProjection` MUST be JCS-byte-identical.
The handoff also contains exactly the generation identity/hash, provider,
provider result refs, structure profile, and that projection; it contains no
media bytes or evaluation result. Preservation re-hashes the generation record
and rejects a projection mismatch before accessing provider results.

The generation index is a closed
`generated_media_generation_index_v2` object with top-level
`schemaVersion`, `assetType`, `contentId`, optional conditional
`animationRequestId`, and `entries`. Each entry key is the exact
`generationRecordId`; its closed value contains `generationRecordId`,
`recordSchemaVersion`, `recordPath`, `recordSha256`, `requestId`, `assetType`,
`domainType`, `contentId`, optional conditional `animationRequestId`,
`planningSnapshotHash`, `promptRecordId`, `promptRecordSha256`, `scopeHash`,
`providerExecutionApprovalSha256`, `generationStatus`, and
`approvalCostProjection`. Unknown members reject. Record, index, and handoff
projections are equality gates, not independently summarized values.
To enforce cumulative `maxAttempts`, the execution role validates every index
entry/record with the same `scopeHash` and counts the union of submitted
logical attempt numbers across approvals. A renewal cannot hide attempts in an
older generation identity.

### 6.3 Canonical fixed vector

The executable vector is
`tests/test_generated_media_provider_execution_approval_contract.mjs`. Its
canonical scope JSON is exactly this single UTF-8 line with no trailing LF:

```json
{"assetType":"character_single_image","capabilityDescriptor":{"capabilityVersion":"imagegen-capability@2026-08-15.1","costDescriptorVersion":"imagegen-cost@2026-08-15.1","provider":"imagegen","providerInterface":"configured_imagegen_capability","providerTool":"imagegen","schemaVersion":"generated_media_imagegen_capability_descriptor_v1","settingsDescriptorVersion":"imagegen-settings@2026-08-15.1"},"capabilityDescriptorSha256":"56feefadf3800a8adac17ba017285665d5ddaf6083a2edb3311d61dd04b136b4","contentId":"seojin","domainType":"character","planningSnapshotHash":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","promptFileSha256":"3313e882e877653bc059fa85bfea8299940f88360673b1ba39d111106c2803c9","promptRecordId":"gmprompt3.character_single_image.character.seojin.1.e12ee2ebe2787f10e8a5","promptRecordSha256":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb","provider":"imagegen","providerInterface":"configured_imagegen_capability","providerPromptPayloadHash":"6f855d4140bc32db400af207899c1ab3a981d4b9df17d3313fe594d05698809d","providerSettings":{"background":"opaque","format":"png","quality":"high","size":"1024x1024"},"providerSettingsSha256":"a1e5fb882b29876db5770023e913b6e62056ac1395a30765136132460be5ce4c","providerTool":"imagegen","registryRowId":"character_single_image_v2","requestId":"gmreq.character.seojin.1","schemaVersion":"generated_media_provider_execution_scope_hash_payload_v1","structureProfile":"character_single_image_v2"}
```

Its fixed `scopeHash` is
`b6ff09a80553191de47b5ad746bd8960f4559da78887670ab667c07da25dcf1b`.
The canonical approval JSON is:

```json
{"approvalEvidence":"codex-thread:019ffabb-97f6-7af3-abaa-f70747dc125f/message:approval-1","approvedAt":"2026-08-13T12:00:00+09:00","approvedBy":"user:contract-reviewer","estimateUnavailablePolicy":"block","maxAttempts":2,"maxCost":{"amount":"0.250000","currency":"USD","kind":"iso_currency"},"schemaVersion":"generated_media_provider_execution_approval_v1","scopeHash":"b6ff09a80553191de47b5ad746bd8960f4559da78887670ab667c07da25dcf1b"}
```

Its fixed approval SHA-256 is
`a68e67b54ca19eaa266b9ecfa7f534764885994daa331ea0263de6bc4531b339`.
The vector proves same-scope stability; prompt record/file/payload, descriptor/settings and
content-identity sensitivity; limit renewal without scope-hash change;
free/no-charge, unknown-estimate, unit/amount exceed and attempt-exceed rules;
submitted-failure consumption; and record/index/handoff projection equality.

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

background_single_image_v2:
  one preserved original scene image
  exact registered background profile and scene-consistency lock
  composition/viewpoint/horizon/depth layers/playable area/canvas/target/safe-area metadata
  scene_composition_anchor metadata; no icon visual-center or icon readability contract

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
missing_source_planning_path
unresolved_source_planning_path
missing_capture_authority_timestamp
invalid_capture_authority_timestamp
planning_snapshot_mismatch
missing_identity_consistency_lock
missing_required_elements
missing_prohibited_elements
missing_positive_style_lock
missing_negative_style_lock
style_lock_evidence_incomplete
provider_prompt_style_lock_missing
missing_sparse_profile_projection
sparse_profile_projection_mismatch
sparse_profile_evidence_incomplete
provider_prompt_sparse_projection_missing
missing_open_ink_wash_profile_projection
open_ink_wash_profile_projection_mismatch
open_ink_wash_profile_evidence_incomplete
provider_prompt_open_ink_wash_projection_missing
open_ink_wash_reference_role_invalid
missing_style_reference_review_record
style_reference_review_record_hash_mismatch
style_reference_review_payload_mismatch
style_reference_asset_missing
style_reference_asset_hash_mismatch
style_reference_record_collision
style_reference_index_invalid
style_reference_binding_incomplete
style_reference_binding_scope_mismatch
style_reference_binding_projection_mismatch
style_reference_role_invalid
style_reference_semantic_transfer_forbidden
character_style_profile_conflict
character_animation_style_lock_mismatch
missing_character_proportion_projection
character_proportion_out_of_range
missing_animation_safe_detail_budget
missing_character_color_value_budget
character_profile_evidence_omission
missing_bold_outline_proportion_projection
bold_outline_proportion_out_of_range
missing_bold_outline_hierarchy_projection
bold_outline_hierarchy_out_of_range
missing_bold_outline_facial_mark_budget
bold_outline_facial_mark_budget_exceeded
missing_bold_outline_compressed_detail_budget
bold_outline_detail_budget_conflict
missing_character_color_signature
character_color_signature_invalid
bold_outline_profile_evidence_omission
provider_prompt_bold_outline_projection_missing
missing_bold_outline_v2_detail_budget_projection
bold_outline_v2_detail_budget_out_of_range
missing_bold_outline_v2_color_anchor_projection
bold_outline_v2_color_anchor_out_of_range
missing_bold_outline_v2_halo_selection
bold_outline_v2_halo_projection_invalid
bold_outline_v2_profile_evidence_omission
provider_prompt_bold_outline_v2_projection_missing
bold_outline_motion_successor_reference_mismatch
bold_outline_motion_flow_not_attack
missing_bold_outline_motion_flow_planning_bindings
bold_outline_motion_flow_base_projection_mismatch
bold_outline_motion_flow_evidence_omission
provider_prompt_bold_outline_motion_flow_projection_missing
character_generation_proportion_gate_failed
character_generation_detail_density_gate_failed
character_generation_color_value_gate_failed
character_generation_bold_outline_motion_flow_gate_failed
character_generation_bold_outline_motion_continuity_gate_failed
character_generation_bold_outline_motion_identity_equipment_gate_failed
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
missing_animation_source_mode
invalid_animation_source_mode
missing_animation_extraction_mode
invalid_animation_extraction_mode
oversampling_not_allowed
unsupported_provider
missing_provider_execution_approval
invalid_provider_execution_approval
provider_execution_scope_mismatch
provider_capability_descriptor_unavailable
provider_capability_preflight_invalid
provider_capability_drift
provider_cost_unit_mismatch
provider_cost_estimate_unavailable
provider_cost_limit_exceeded
provider_actual_cost_unavailable
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
missing_reference_prompt_record
reference_prompt_record_hash_mismatch
missing_expression_profile_payload
missing_expression_profile_key
missing_expression_profile_payload_hash
expression_profile_key_mismatch
expression_profile_payload_hash_mismatch
unexpected_character_style_reference
missing_style_reference_review_record
style_reference_review_record_hash_mismatch
style_reference_review_payload_mismatch
style_reference_asset_missing
style_reference_asset_hash_mismatch
style_reference_index_invalid
style_reference_binding_incomplete
style_reference_binding_scope_mismatch
style_reference_binding_projection_mismatch
style_reference_role_invalid
style_reference_semantic_transfer_forbidden
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
prompt_record_write_failed
prompt_markdown_write_failed
prompt_index_write_failed
prompt_publish_rollback_failed
```

The five animation-ready authoring tokens have these exact meanings:

| token | deterministic meaning |
| --- | --- |
| `missing_character_proportion_projection` | selected animation-ready profile lacks either the approved full-body head-count range, head-to-height percentage, or shortened-limb projection |
| `character_proportion_out_of_range` | an approved or authored bound exceeds 3.75-4.25 heads, exceeds the 24-27 percent head-height interval, or permits naturalistic seven-to-eight-head or heroic tall anatomy |
| `missing_animation_safe_detail_budget` | the approved projection does not close sparse line density, contour omission, high-signal identity groups, and frame reproducibility |
| `missing_character_color_value_budget` | the approved projection does not close minimal flat values and one-to-two subdued accent hues |
| `character_profile_evidence_omission` | a required projection or profile lock lacks its exact planning/profile evidence or is omitted from provider prose |

These tokens apply only after the exact animation-ready key was selected. The
legacy-compatible profile remains governed by its immutable original shape and
locks.

The four sparse-profile authoring tokens apply only to
`projectbs_character_sparse_ink_pastel_motion@1.0.0`:

| token | deterministic meaning |
| --- | --- |
| `missing_sparse_profile_projection` | one or more of the exact eight policy members is absent from the authored or inherited projection |
| `sparse_profile_projection_mismatch` | any projected member, nested value, type, array order, payload key, or payload hash differs from the registered canonical payload |
| `sparse_profile_evidence_incomplete` | one or more of the eight policy members lacks exact profile/planning evidence coverage |
| `provider_prompt_sparse_projection_missing` | the copy-ready provider prompt omits or weakens any applicable main/animation projection from the eight policy members |

The five open ink-wash authoring tokens apply only to
`projectbs_character_open_ink_wash_dynamic_contour@1.0.0`:

| token | deterministic meaning |
| --- | --- |
| `missing_open_ink_wash_profile_projection` | one or more of the exact eleven policy members or either ordered lock array is absent |
| `open_ink_wash_profile_projection_mismatch` | any member, nested value, type, array order, key, or recomputed payload hash differs from the canonical payload |
| `open_ink_wash_profile_evidence_incomplete` | a policy member, lock, or required character-specific planning binding lacks exact authority evidence |
| `provider_prompt_open_ink_wash_projection_missing` | provider prose omits or weakens any exact profile member, planning-bound anchor, or ordered lock |
| `open_ink_wash_reference_role_invalid` | the accepted SHA is bound through anything other than the complete reviewed six-member style-only binding, or is used for identity/person/pose/action/clothing/equipment/edit-target semantics |

The v1 meanings above are immutable. The output-conformance successor
`projectbs_character_open_ink_wash_dynamic_contour@2.0.0` uses these distinct
authoring tokens:

| token | deterministic meaning |
| --- | --- |
| `missing_open_ink_wash_v2_profile_projection` | one or more of the exact nineteen successor members, either ordered 9-item lock array, or the closed gate order is absent |
| `open_ink_wash_v2_profile_projection_mismatch` | any successor member, nested value, type, array order, key, predecessor binding, or recomputed payload hash differs from the canonical v2 payload |
| `open_ink_wash_v2_profile_evidence_incomplete` | a successor policy member, lock, gate, or character-specific planning binding lacks exact authority evidence |
| `provider_prompt_open_ink_wash_v2_projection_missing` | provider prose omits or weakens any successor constraint, including the measurement, surface-detail, uniform-background, or output-conformance constraint |
| `open_ink_wash_v2_reference_role_invalid` | the accepted SHA is bound through anything other than the complete reviewed six-member style-only binding, or is used for identity/person/pose/action/clothing/equipment/edit-target semantics |

These tokens do not reinterpret v1. A new planning revision must select the v2
key and exact v2 payload hash before authoring may use them.

The durable style-reference tokens are shared, closed stage blockers:

| token | deterministic meaning |
| --- | --- |
| `missing_style_reference_review_record` | a durable style binding names no readable canonical review record |
| `style_reference_review_record_hash_mismatch` | raw canonical review-record bytes differ from the binding or index hash |
| `style_reference_review_payload_mismatch` | review JCS payload, deterministic ID, asset identity, purpose, status, transfer boundary, or selected profile binding differs |
| `style_reference_asset_missing` | canonical project-relative reviewed asset is absent |
| `style_reference_asset_hash_mismatch` | reviewed asset raw bytes, PNG signature, length, or dimensions differ from the record |
| `style_reference_record_collision` | an occupied review ID or asset identity has divergent immutable bytes |
| `style_reference_index_invalid` | review index is absent, dangling, divergent, or not the exact closed projection |
| `style_reference_binding_incomplete` | a style-only consumer entry does not have exactly the six required members |
| `style_reference_binding_scope_mismatch` | asset type, style family, or selected profile key/hash is outside the approved review record |
| `style_reference_binding_projection_mismatch` | routing does not place one byte-semantically equal binding array at all four required top-level projections, or nests it inside typeSpecification |
| `style_reference_role_invalid` | role is not exactly `style_only`, path is noncanonical/absolute/transient, or a style binding is represented as an identity/edit reference |
| `style_reference_semantic_transfer_forbidden` | any consumer derives person, identity, pose, action, clothing, equipment, or edit-target semantics from the raster |

Planning validates before handoff publication, routing before route publication,
authoring before prompt publication, and generation again before capability or
submit access. A later stage never repairs or republishes review artifacts.

The bold-outline authoring tokens apply only to
`projectbs_character_bold_outline_compressed_detail@1.0.0`:

| token | deterministic meaning |
| --- | --- |
| `missing_bold_outline_proportion_projection` | approved planning lacks exact `fullBodyHeadCount` |
| `bold_outline_proportion_out_of_range` | head count is outside 4.0-5.0, does not project a 20-25 percent head-height equivalent, or permits 6.5-8 heads, long limbs, or heroic tall anatomy |
| `missing_bold_outline_hierarchy_projection` | approved planning lacks exact outside and internal source-pixel thickness values |
| `bold_outline_hierarchy_out_of_range` | outside thickness is outside 16-22 px on 1024x1536, placement is not outside-silhouette, outside/internal ratio is below 2, or internal lines are not sparse/materially thinner |
| `missing_bold_outline_facial_mark_budget` | the exact counting unit, total maximum, or component maxima are absent |
| `bold_outline_facial_mark_budget_exceeded` | total exceeds 9, a component exceeds 4/1/1/3, component maxima exceed the total, or realistic facial modeling is permitted |
| `missing_bold_outline_compressed_detail_budget` | the identity-first groups, fold limit, surface-detail policy, or forbidden set is absent |
| `bold_outline_detail_budget_conflict` | dense folds, more than three secondary fold marks per garment region, individual scales/rivets, microtexture, hatching, modeled shading, or realistic material rendering is allowed |
| `missing_character_color_signature` | any required primary hue/anchors, coverage/mass limits, or neutral outline/weapon color is absent, or a secondary hue/anchor pair is only partially present |
| `character_color_signature_invalid` | anchors are empty/duplicate/unbound, coverage is outside 1-35 percent, masses outside 1-4, full-garment fill is allowed, or color overrides line hierarchy |
| `bold_outline_profile_evidence_omission` | a planning binding, profile constant, or lock lacks exact planning/profile evidence coverage |
| `provider_prompt_bold_outline_projection_missing` | provider prose omits or weakens any lock, exact planning-bound value, budget, anchor, neutral color, or bold-silhouette priority |

These checks are closed-field checks. Free-form prompt wording never repairs a
missing approved planning binding.

The successor-only authoring tokens apply only to
`projectbs_character_bold_outline_compressed_detail@2.0.0`:

| token | deterministic meaning |
| --- | --- |
| `missing_bold_outline_v2_detail_budget_projection` | exact counting unit or any total/internal/fold maximum is absent |
| `bold_outline_v2_detail_budget_out_of_range` | total exceeds 64, internal exceeds 56 or total, folds exceed 5 per garment region, or hatching, microtexture, modeled/realistic material rendering, dense folds, scales, or rivets are allowed |
| `missing_bold_outline_v2_color_anchor_projection` | a required hue, anchor, limit, or neutral is absent, or a secondary hue omits elements or site classes |
| `bold_outline_v2_color_anchor_out_of_range` | ochre uses a site outside `small_utility_pouch` or `small_travel_accessory`, coverage/masses exceed 35/4, or arbitrary/full-garment fill or hierarchy override is allowed |
| `missing_bold_outline_v2_halo_selection` | no exact `enabled` discriminant is approved |
| `bold_outline_v2_halo_projection_invalid` | disabled has extra members or authorizes dark background; enabled omits an exact member, exceeds opacity 0.35 or coverage 45, does not fade monotonically to edge alpha zero, or permits a scene, opaque background, shadow substitute, or directional cast shadow |
| `bold_outline_v2_profile_evidence_omission` | a successor binding, constant, budget, halo member, or lock lacks exact evidence |
| `provider_prompt_bold_outline_v2_projection_missing` | provider prose omits or weakens any exact successor budget, anchor, halo member, or ordered lock |

The composed motion-flow successor adds three router tokens and three authoring
tokens. Router uses `bold_outline_motion_successor_reference_mismatch` for any
nonexact bold v2 reference/base projection,
`bold_outline_motion_flow_not_attack` for a non-attack motion class, and
`missing_bold_outline_motion_flow_planning_bindings` when any of the eight
approved motion facts is absent. Authoring uses
`bold_outline_motion_flow_base_projection_mismatch`,
`bold_outline_motion_flow_evidence_omission`, and
`provider_prompt_bold_outline_motion_flow_projection_missing` respectively for
base 18/8 or 64/56/5/color/halo disagreement, missing exact motion/lock evidence,
and omitted or weakened provider prose. All six stop before publication or
capability access.

The lock-array tokens `missing_positive_style_lock`,
`missing_negative_style_lock`, `style_lock_evidence_incomplete`, and
`provider_prompt_style_lock_missing` apply only to the six registered
lock-array profiles, including bold-outline compressed-detail and open ink-wash.
They MUST NOT be
returned for the sparse profile.

The first eight tokens above are authoring-readiness failures with these exact
boundaries:

| token | stage and applicability | deterministic meaning |
| --- | --- | --- |
| `missing_reference_prompt_record` | character animation authoring only | required immutable character single-image prompt record path/file is absent or unreadable |
| `reference_prompt_record_hash_mismatch` | character animation authoring only | SHA-256 of exact reference prompt-record file bytes differs from `referencePromptRecordSha256` |
| `missing_expression_profile_payload` | character single-image or character animation authoring | required closed canonical payload is absent from the record being authored or inherited |
| `missing_expression_profile_key` | character single-image or character animation authoring | required `expressionProfileKey` is absent |
| `missing_expression_profile_payload_hash` | character single-image or character animation authoring | required `expressionProfilePayloadHash` is absent |
| `expression_profile_key_mismatch` | character single-image or character animation authoring | record, handoff, registry, or inherited reference key differs from the exact registered key |
| `expression_profile_payload_hash_mismatch` | character single-image or character animation authoring | recomputed canonical payload hash differs from the record, handoff, registry, or inherited reference hash |
| `unexpected_character_style_reference` | skill animation authoring only | any character reference-prompt/profile field or payload is present where all are prohibited |

These tokens stop before a ready prompt record is written. They are not router,
generation, preservation, or evaluation tokens. Character animation never uses
the skill-only token, and skill animation never uses the seven character-only
tokens.

For character prompt publication, the remaining 8.3 record tokens and retry
meanings are exact:

| failureType | write outcome | safeToRetry |
| --- | --- | --- |
| `unknown_record_field`, `missing_record_field`, `record_identity_mismatch`, `record_hash_mismatch`, `prompt_markdown_mismatch`, `provider_value_invalid`, `unsupported_record_schema` | no new workflow artifact | `false`; correct the named input/schema/bytes first |
| `record_collision`, `index_entry_invalid`, `prompt_record_missing`, `prompt_record_stale` | preserve existing evidence; no overwrite or handoff | `false`; separate remediation or fresh identity is required |
| `prompt_record_write_failed`, `prompt_markdown_write_failed`, `prompt_index_write_failed` | every file created by this attempt was exact-byte rolled back; prior index unchanged | `true` |
| `prompt_publish_rollback_failed` | no handoff; potentially partial evidence preserved for remediation | `false` |

`safeToRetry` is the truth for an unchanged immediate retry. Corrected input or
separate remediation may change that truth. `reused_identical` is a successful
authoring status, not a failure type; it performs no overwrite and returns a
fresh detached handoff over the currently verified index bytes.

### 8.4 Generation Extension

```text
missing_provider_execution_approval
invalid_provider_execution_approval
provider_execution_scope_mismatch
provider_capability_descriptor_unavailable
provider_capability_preflight_invalid
provider_capability_drift
provider_cost_unit_mismatch
provider_cost_estimate_unavailable
provider_cost_limit_exceeded
provider_actual_cost_unavailable
retry_limit_exceeded
duplicate_provider_call_risk
provider_operation_failed
animated_provider_capability_unavailable
provider_animated_gif_source_mismatch
character_generation_proportion_gate_failed
character_generation_detail_density_gate_failed
character_generation_color_value_gate_failed
character_generation_sparse_contour_gate_failed
character_generation_sparse_omission_budget_gate_failed
character_generation_sparse_pigment_budget_gate_failed
character_generation_sparse_motion_gate_failed
character_generation_identity_anchor_gate_failed
character_generation_bold_outline_proportion_gate_failed
character_generation_bold_outline_hierarchy_gate_failed
character_generation_bold_outline_facial_mark_budget_gate_failed
character_generation_bold_outline_detail_budget_gate_failed
character_generation_bold_outline_color_signature_gate_failed
character_generation_bold_outline_v2_detail_budget_gate_failed
character_generation_bold_outline_v2_color_anchor_gate_failed
character_generation_bold_outline_v2_halo_gate_failed
character_generation_open_ink_wash_proportion_age_gate_failed
character_generation_open_ink_wash_contour_mok_seon_gate_failed
character_generation_open_ink_wash_pigment_negative_space_gate_failed
character_generation_open_ink_wash_background_gate_failed
character_generation_open_ink_wash_identity_equipment_gate_failed
character_generation_open_ink_wash_reference_role_gate_failed
character_generation_open_ink_wash_v2_surface_detail_gate_failed
character_preview_open_ink_wash_v2_proportion_age_nonconformant
character_preview_open_ink_wash_v2_contour_mok_seon_nonconformant
character_preview_open_ink_wash_v2_surface_detail_nonconformant
character_preview_open_ink_wash_v2_pigment_negative_space_nonconformant
character_preview_open_ink_wash_v2_background_nonconformant
character_preview_open_ink_wash_v2_identity_equipment_nonconformant
character_preview_open_ink_wash_v2_reference_role_nonconformant
character_preview_open_ink_wash_v2_evidence_insufficient
missing_hosted_preview_approval
invalid_hosted_preview_approval
missing_hosted_preview_auto_approval_policy
invalid_hosted_preview_auto_approval_policy
hosted_preview_auto_approval_policy_mismatch
hosted_preview_auto_approval_policy_revoked
hosted_preview_scope_mismatch
hosted_preview_unknown_setting
hosted_preview_prompt_stage_semantics_conflict
hosted_preview_prompt_drift
hosted_preview_reference_drift
hosted_preview_submit_limit_exceeded
hosted_preview_retry_forbidden
hosted_preview_output_missing
hosted_preview_output_hash_mismatch
hosted_preview_preservation_forbidden
hosted_preview_promotion_forbidden
fast_preview_duplicate_submit_risk
fast_preview_authority_or_safety_violation
fast_preview_callable_input_absent
fast_preview_submit_failed_no_retry
```

After an animation-ready minimal prompt record is immutable and before any
submit boundary, character generation repeats three semantic gates over the
exact profile payload, evidence map, and `scenePromptOriginal`. It rejects any wording that allows
more than 4.25 heads or naturalistic seven-to-eight-head anatomy as
`character_generation_proportion_gate_failed`; dense realistic detail,
microtexture, modeled shading, scales/rivets, dense folds, or hatching as
`character_generation_detail_density_gate_failed`; and gradients, cinematic or
physical lighting, realistic material rendering, nonminimal value masses, or
more than two accent hues as
`character_generation_color_value_gate_failed`. These are no-call blockers;
generation does not rewrite the prompt or substitute a profile.

For `projectbs_character_sparse_ink_pastel_motion@1.0.0`, generation also
checks the exact main-versus-animation projection before capability access.
Closed coloring-book contours or a fully inked silhouette fail
`character_generation_sparse_contour_gate_failed`; opaque/cel fill,
off-palette hue, a main-image pigment area above 18 percent, a main accent count
outside 4-7, or an animation-frame accent count outside 3-6 fail
`character_generation_sparse_pigment_budget_gate_failed`. Main omission
outside 35-45 percent or per-approved-animation-frame omission outside 35-50
percent fails `character_generation_sparse_omission_budget_gate_failed`. An animation lacking the
registered line/pigment motion cues or repeating static action frames fails
`character_generation_sparse_motion_gate_failed`; and gaze, topknot,
hand/sword grip, support foot, or action-joint drift fails
`character_generation_identity_anchor_gate_failed`. These observable semantic
checks do not claim an unavailable computer-vision measurement.

The `character_generation_*` sparse tokens are owned only by generation
pre-submit validation.

For `projectbs_character_bold_outline_compressed_detail@1.0.0`, generation
performs five independent no-submit gates over the exact payload, closed
planning projection, evidence map, and provider prompt. Head count outside
4.0-5.0 or permission for 6.5-8 heads/long limbs/heroic anatomy fails
`character_generation_bold_outline_proportion_gate_failed`. Outside outline
outside 16-22 source px, non-outside placement, outside/internal ratio below 2,
or weak/equal internal hierarchy fails
`character_generation_bold_outline_hierarchy_gate_failed`. A total above 9,
component maxima above 4/1/1/3, or realistic facial modeling fails
`character_generation_bold_outline_facial_mark_budget_gate_failed`. Dense
folds, individual scales/rivets, microtexture, hatching, modeled shading, or
realistic materials fails
`character_generation_bold_outline_detail_budget_gate_failed`. Missing or
unbound primary/secondary hue anchors, coverage above 35 percent, more than four
color masses, invalid neutral outline/weapon colors, full-garment fill, or color
overriding line hierarchy fails
`character_generation_bold_outline_color_signature_gate_failed`. All five stop
before capability access and return `providerCalled=false`, `submitCount=0`,
and `cost=0`.

For `projectbs_character_bold_outline_compressed_detail@2.0.0`, generation
reuses the inherited proportion, hierarchy, and facial gates and adds three
pre-submit gates. More than 64 total visible marks, 56 internal marks, or 5
secondary folds in any garment region, or any closed forbidden detail, fails
`character_generation_bold_outline_v2_detail_budget_gate_failed`. Unapproved
ochre sites, arbitrary/full-garment color, or coverage/mass drift fails
`character_generation_bold_outline_v2_color_anchor_gate_failed`. A nonclosed
disabled branch, or an enabled halo outside opacity 0.08-0.35 or coverage 1-45,
without centered monotonic fade to edge alpha zero, or permitting scene,
opaque-background, or shadow semantics fails
`character_generation_bold_outline_v2_halo_gate_failed`. All are no-call gates.

For `projectbs_character_bold_outline_attack_motion_flow@1.0.0`, generation
first reruns every inherited bold v2 gate against the immutable base. It then
fails missing or incorrect indigo 3-5 sword/torso flow, gray-brown shoulder/hem
inertia, bounded dark-neutral trajectory, static repetition, generic clean-
vector output, arbitrary speed lines, or magic VFX as
`character_generation_bold_outline_motion_flow_gate_failed`; frame-order,
fixed-cell, scale, or root-anchor discontinuity as
`character_generation_bold_outline_motion_continuity_gate_failed`; and any
identity/equipment anchor drift as
`character_generation_bold_outline_motion_identity_equipment_gate_failed`.
These are pre-submit gates with `providerCalled=false`, `submitCount=0`, and
`cost=0`.

For `projectbs_character_open_ink_wash_dynamic_contour@1.0.0`, generation
performs six independent no-submit gates over immutable prompt bytes. A figure
outside 4-5 heads, not targeted at 4.25, or child/minor-coded fails
`character_generation_open_ink_wash_proportion_age_gate_failed`. Omission
outside 35-55 or missing target-45 intent, pressure variation, brush start,
directional drag, dry end, or directional weight—or a sticker/uniform/vector
contour—fails `character_generation_open_ink_wash_contour_mok_seon_gate_failed`.
Missing broad rough watercolor/pastel masses, controlled bleed/misalignment,
separate three-role palette, or either 70-percent negative-space floor, or any
cel fill/decorative small splashes, fails
`character_generation_open_ink_wash_pigment_negative_space_gate_failed`.
Anything other than removable warm-ivory solid generation background, or any
halo/vignette/scene/shadow, fails
`character_generation_open_ink_wash_background_gate_failed`. Korean/Joseon,
age, costume, equipment, weapon, handedness, or identifying-anchor drift fails
`character_generation_open_ink_wash_identity_equipment_gate_failed`. Treating
the audit SHA as a provider binding, identity/edit target, or path before closed
durable publication fails
`character_generation_open_ink_wash_reference_role_gate_failed`. All return
`providerCalled=false`, `submitCount=0`, and `cost=0` without repairing prose.

For `projectbs_character_open_ink_wash_dynamic_contour@2.0.0`, generation keeps
all six v1-equivalent pre-submit gates and additionally rejects provider prose
that permits a realistically modeled face, individually rendered armor plates,
scales, rivets, lacing, fasteners, garment microfolds, microtexture, modeled
light, or realistic material rendering as
`character_generation_open_ink_wash_v2_surface_detail_gate_failed`. This is a
no-call blocker. Passing every pre-submit gate is only permission to submit; it
is not evidence that returned pixels conform.

After exactly one hosted-preview submit returns observable output, the v2
branch performs the closed, non-scoring profile-conformance triage in this
order: `proportion_age`, `contour_mok_seon`, `surface_detail`,
`pigment_palette_negative_space`, `background`, `identity_equipment`, then
`reference_role`. A visible violation returns the corresponding
`character_preview_open_ink_wash_v2_*_nonconformant` token and
`profileConformanceStatus=preview_profile_nonconformant`. In particular, a
figure outside 4-5 heads or not centered on the 4.25 target fails proportion;
realistically modeled facial or equipment micro-detail fails surface detail;
and any radial gradient, dark backdrop, halo, vignette, scene, or shadow fails
background. Evidence that cannot support a gate returns
`character_preview_open_ink_wash_v2_evidence_insufficient` and
`profileConformanceStatus=preview_profile_conformance_blocked`.

Only seven explicit passes return
`profileConformanceStatus=preview_conformant_no_downstream`. Every other result
has `nextStep=stop_no_retry_not_final`. The output remains a one-submit preview:
the triage does not call a provider, retry, edit, score, evaluate, preserve, or
promote it, and it never labels a nonpass output complete or final. Generation
returns the compact `generated_media_profile_conformance_receipt_v1` defined in
GeneratedMediaRecordGuide.md; later control-plane messages reference its hash
instead of retransmitting the full authority payload. A compact receipt never
replaces fresh Git-blob verification before a mutation boundary.

#### 8.4.1 Character Expression Evaluation Extension

```text
missing_character_expression_evaluation_package
character_evaluation_profile_mismatch
character_evaluation_frame_count_mismatch
character_evaluation_evidence_insufficient
character_evaluation_proportion_gate_failed
character_evaluation_sparse_contour_gate_failed
character_evaluation_sparse_omission_budget_gate_failed
character_evaluation_sparse_pigment_budget_gate_failed
character_evaluation_sparse_motion_gate_failed
character_evaluation_identity_anchor_gate_failed
character_evaluation_bold_outline_proportion_gate_failed
character_evaluation_bold_outline_hierarchy_gate_failed
character_evaluation_bold_outline_facial_mark_budget_gate_failed
character_evaluation_bold_outline_detail_budget_gate_failed
character_evaluation_bold_outline_color_signature_gate_failed
character_evaluation_bold_outline_v2_detail_budget_gate_failed
character_evaluation_bold_outline_v2_color_anchor_gate_failed
character_evaluation_bold_outline_v2_halo_gate_failed
character_evaluation_bold_outline_motion_flow_gate_failed
character_evaluation_bold_outline_motion_continuity_gate_failed
character_evaluation_bold_outline_motion_identity_equipment_gate_failed
character_evaluation_open_ink_wash_proportion_age_gate_failed
character_evaluation_open_ink_wash_contour_mok_seon_gate_failed
character_evaluation_open_ink_wash_pigment_negative_space_gate_failed
character_evaluation_open_ink_wash_background_gate_failed
character_evaluation_open_ink_wash_identity_equipment_gate_failed
character_evaluation_open_ink_wash_reference_role_gate_failed
character_evaluation_open_ink_wash_v2_surface_detail_gate_failed
```

The first four tokens respectively mean that the required sealed package is
absent/unreadable, the exact profile key/payload/hash does not match, ordered
animation members do not equal the positive approved `finalFrameCount`, or an
observable gate lacks sufficient reproducible evidence. These tokens and the
gate tokens are owned only by read-only evaluation. The gate tokens use the same numeric boundaries:
main omission 35-45, main accents 4-7, main pigment area at most 18;
per-approved-animation-frame omission 35-50 and accents 3-6. Contour,
palette/fill, motion, and identity-anchor conditions are otherwise identical.
Neither stage may return the other stage's prefix, and six frames is only a
golden test fixture rather than an operational count.

For the bold-outline profile, evaluation independently measures or observes the
same five closed groups and uses only the five
`character_evaluation_bold_outline_*_gate_failed` tokens. A failed group is
fatal and cannot be offset by score. If reproducible evidence cannot establish
head ratio, line thickness hierarchy, facial mark count, detail density, or
color coverage/masses/anchors, evaluation returns
`character_evaluation_evidence_insufficient` instead of guessing.

For the v2 successor, evaluation reuses inherited proportion, hierarchy, and
facial tokens and independently applies the three exact
`character_evaluation_bold_outline_v2_*_gate_failed` tokens to the 64/56/5
detail ceilings, approved ochre sites and 35/4 color bounds, and closed halo
union. An opaque, scenic, noncentered, directional-shadow, or nonfading dark
treatment is fatal. If any mark, coverage, opacity, edge, or anchor measurement
is not reproducible, it returns `character_evaluation_evidence_insufficient`.

The animation-only successor first requires exact base-profile evidence, then
uses the three `character_evaluation_bold_outline_motion_*_gate_failed` tokens
for motion-flow/VFX, continuity, and identity/equipment-anchor failures. Each is
fatal and no numeric score can offset it.

For the open ink-wash profile, evaluation applies the six corresponding
`character_evaluation_open_ink_wash_*_gate_failed` tokens to the same closed
groups. A failed group is fatal. Percentages, stroke phases, palette roles,
background status, identity/equipment stability, and reference role require
reproducible observable evidence; otherwise return
`character_evaluation_evidence_insufficient`, never an inferred pass.

For the open ink-wash v2 successor, evaluation reuses those six profile gates
and independently applies
`character_evaluation_open_ink_wash_v2_surface_detail_gate_failed` to realistic
face modeling, enumerated armor/fastener/garment detail, microtexture, modeled
light, or realistic material rendering. Its compact preview receipt is not an
evaluation package and cannot supply a guessed pass.

### 8.5 Preservation Extension

The accepted-result `character_single_image` corrective sub-branch is closed by
GeneratedMediaPreservationPackagingGuide.md. It may bind the official one-
submit/zero-retry corrective terminal receipt to the existing accepted capture
without inventing generation-v2, then apply only
`border_exact_checkerboard_boundary_flood_v1`. The exact six-frame uniform-8fps
coherent-master GIF exception uses only `[12,13,12,13,12,13]` centiseconds,
750ms total and no loop extension. Neither exception changes provider-native,
strict generation, other timing, or promotion contracts.

Two source-bound v2 preservation exceptions are additionally closed by that
guide. SHA `4498817999fb28323eb85f62afefcc33027341640b1a7ce990a3609b32eaeb7e`
may use only `border_frozen_palette_boundary_flood_v2` with its exact registered
64-color boundary fixture. Accepted GIF SHA
`8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621`
may use only `gif_exact_uniform_boundary_color_flood_v2`, whose observed color
is exact `(240,236,228)` at 2,300/2,300 boundary pixels in every frame. Both
clear only boundary-connected exact matches, preserve every nonmatching pixel,
and fail on source/evidence drift. They do not alter v1, strict, provider-native
or any other source contract.

```text
missing_planning_handoff_v2
missing_routing_v2
missing_prompt_v3
missing_generation_v2
generation_not_ready
generation_record_hash_mismatch
record_identity_mismatch
preservation_record_collision
canonical_serializer_unsupported
serializer_settings_mismatch
serializer_output_hash_mismatch
serializer_reopen_validation_failed
preservation_index_collision
preservation_index_cas_mismatch
preservation_record_index_mismatch
provider_result_ref_missing
provider_result_unavailable_requires_generation_task
unsupported_preservation_adapter
evaluation_staging_root_not_configured
staging_project_path_violation
original_download_failed
provider_export_failed
source_not_original
source_hash_mismatch
provider_animated_gif_source_mismatch
gif_timeline_contract_mismatch
corrective_single_image_evidence_mismatch
checkerboard_background_pattern_unsupported
checkerboard_foreground_contact_ambiguous
checkerboard_alpha_normalization_validation_failed
border_palette_source_fixture_mismatch
border_palette_checkerboard_coherence_failed
border_palette_foreground_contact_detected
border_palette_normalization_validation_failed
extraction_failed
fixed_cell_contract_mismatch
scale_lock_violation
anchor_mapping_mismatch
vertical_motion_policy_violation
chroma_key_scope_violation
gif_observed_boundary_source_fixture_mismatch
gif_observed_boundary_color_ambiguous
gif_observed_boundary_corner_mismatch
gif_observed_boundary_normalization_validation_failed
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

### 8.7 Downstream Evaluation and Promotion Extension

```text
background_adapter_identity_mismatch
missing_background_evaluation_contract
character_evaluation_proportion_gate_failed
character_evaluation_detail_density_gate_failed
character_evaluation_color_value_gate_failed
promotion_identity_mode_conflict
evaluation_package_not_found
evaluation_package_hash_mismatch
background_structure_profile_mismatch
background_promotion_adapter_mismatch
background_promotion_target_contract_missing
legacy_current_identity_conflict
```

Readiness is true only when every common and type-specific field exists, hashes
verify, exactly one current registry row matches, provider is ImageGen, and the
provider interface is `configured_imagegen_capability`. Before every external
call, a fresh closed non-submit capability response, the approval object, and
the recomputed scope must pass sections 6.1-6.2, the
estimate must satisfy its tagged-unit comparison rule, consumed attempts must
remain below `maxAttempts`, and the deterministic idempotency key must have no
active duplicate. An identical completed result is reused without billing;
every attempted or avoided call records `costEvidence` and the three projection
copies must be JCS-byte-identical.
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
- current registry exposes exactly four execution roles and no PixelLab row;
- icon and background routes have distinct domains, prompts, adapters,
  structure profiles and evaluation identities;
- ambiguous icon/background evidence returns `ambiguous_image_role`;
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
AgentDocs/planning-guides/content/generated-media/ImageGenBackgroundPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenAnimationPipelineGuide.md
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/task-prompts/content/GeneratedImageEvaluationPrompt.md
AgentDocs/planning-guides/content/GeneratedImageProjectPromotionGuide.md
AgentDocs/task-prompts/content/GeneratedImageProjectPromotionPrompt.md
```

## 18. Transparent Foreground Output Projection v1

This additive projection is selected only by an exact planning selection. It
does not reinterpret `backgroundFullyOpaque`, provider-native GIF,
coherent-master, corrective-normalization, or any existing record/package.

```json
{"animation":{"backgroundFlicker":false,"baselineDriftMaxPx":0,"completedAnimationFormat":"gif","dynamicPigmentExcludedFromAnchorMovement":true,"fixedGroundBaseline":true,"fixedPelvisWorldRootCoordinate":true,"fixedScale":true,"identicalCanvas":true,"independentSilhouetteRecentering":false,"neighboringFragments":false,"orderedFrameCount":6,"orderedTrueAlphaFrameFormat":"png","pelvisDriftMaxPx":0,"swordAndEffectsInsideSafeMargin":true},"appliesTo":[{"assetType":"character_single_image","structureProfile":"character_single_image_v2"},{"assetType":"animation","structureProfile":"animation_gif_frame_set_v2"}],"characterSingleImage":{"colorMode":"RGBA","fullFigureEquipmentPigmentInBounds":true,"primaryFormat":"png"},"common":{"boundedArtisticPartialAlpha":"inside_intended_character_equipment_pigment_silhouette_only","forbidden":["matte","checkerboard","halo","vignette","floor","scene","cast_shadow","residual_fringe"],"noClipping":true,"outsideIntendedForegroundAlpha":0,"safeMarginPx":"required_positive_integer"},"compatibility":{"backgroundFullyOpaque":"not_reinterpreted","existingBranches":"unchanged"},"gates":{"evaluation":"all_projection_failures_pre_score_hard_fail","generationAndPreservation":"alpha_mask_fringe_anchor_baseline_before_complete","promotion":"separate_completed_pass_and_passForProjectCopy_true_before_authenticated_replaceExisting"},"projectionKey":"generated_media_true_alpha_foreground@1.0.0","schemaVersion":"generated_media_transparent_foreground_output_projection_v1"}
```

The RFC 8785 JCS SHA-256 is
`2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108`.
The closed `generated_media_transparent_foreground_selection_v1` contains
exactly `schemaVersion`, `projectionKey`, `projectionPayloadHash`, `assetType`,
`safeMarginPx`, `noClipping`, and one conditional member: `mainLock` contains
exactly `rgbaEvidenceRequired` and `fullFigureEquipmentPigmentInBounds`; or
`animationLock` contains exactly `frameCount`, `canvasWidth`, `canvasHeight`,
`pelvisWorldRootX`, `pelvisWorldRootY`, `groundBaselineY`, `scaleNumerator`,
`scaleDenominator`, `independentSilhouetteRecentering`, and
`dynamicPigmentExcludedFromAnchorMovement`. Integers are nonnegative except
safe margin, canvas, scale numerator/denominator, and frame count are positive;
frame count is exactly 6 here. The branches are mutually exclusive.

The closed `generated_media_true_alpha_output_receipt_v1` contains common exact
members `schemaVersion`, `projectionKey`, `projectionPayloadHash`,
`selectionSha256`, `assetType`, `safeMarginPx`, `alphaMaskSha256`,
`outsideForegroundAlphaMaximum` (0), `partialAlphaInsideIntendedSilhouette`,
`matteDetected`, `checkerboardDetected`, `haloDetected`, `vignetteDetected`,
`floorDetected`, `sceneDetected`, `castShadowDetected`, `residualFringeDetected`,
`clippingDetected`, and `status`. Main adds exactly `width`, `height`,
`rgbaPixelSha256`, and `fullFigureEquipmentPigmentInBounds`. Animation adds
exactly `completedGifSha256`, six ordered `trueAlphaPngFrameSha256s`, six
ordered `frameAlphaMaskSha256s`, `canvasWidth`, `canvasHeight`,
`pelvisWorldRootX`, `pelvisWorldRootY`, `groundBaselineY`,
`pelvisDriftMaxPx` (0), `baselineDriftMaxPx` (0), `scaleNumerator`,
`scaleDenominator`, `independentSilhouetteRecentering` (false),
`backgroundFlickerDetected` (false), `neighboringFragmentsDetected` (false),
`swordAndEffectsInsideSafeMargin` (true), and
`dynamicPigmentExcludedFromAnchorMovement` (true). The GIF uses one transparent
index for outside-foreground alpha zero; bounded artistic partial alpha is
preserved and measured in the ordered RGBA PNG frames, never invented in GIF.

Generation and preservation validate the exact alpha mask, fringe/background
absence, bounds, margin, and conditional anchor/baseline evidence before
completion. Evaluation treats every receipt failure as pre-score hard fail.
Typed failures are `true_alpha_projection_missing`,
`true_alpha_projection_mismatch`, `true_alpha_branch_conflict`,
`outside_foreground_alpha_nonzero`, `artistic_partial_alpha_outside_silhouette`,
`true_alpha_residual_fringe_detected`, `true_alpha_background_artifact_detected`,
`true_alpha_safe_margin_or_clipping_violation`,
`true_alpha_animation_anchor_baseline_drift`,
`true_alpha_animation_independent_recentering_detected`,
`true_alpha_animation_background_flicker`,
`true_alpha_animation_neighboring_fragment`, and
`true_alpha_animation_dynamic_pigment_anchor_contamination`.

The additive animation-only
`projectbs_character_open_ink_wash_attack_motion@1.0.0` / payload hash
`07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c`
composes the immutable open-ink v2 base with six approved attack-motion
bindings and the exact true-alpha projection. Its closed payload authority is
`GeneratedMediaOpenInkWashAttackMotionSuccessorGuide.md`. It never aliases the
sparse-motion profile. Routing/authoring failures use the closed
`open_ink_attack_*` tokens; generation/evaluation use the three closed style,
motion-continuity and true-alpha tokens owned by their respective stages. The
existing open-ink v2 and true-alpha payload hashes and all historical records
remain unchanged.

For a selected `character_single_image` true-alpha branch, Prompt v3 uses the
closed extension in GeneratedMediaTransparentForegroundAuthoringGuide.md.
`generationBackground` is exactly `{mode:"transparent"}` and the exact
`transparentForegroundSelection` is hash-significant in visual brief, prompt record/hash payload,
provider settings, index entry and detached handoff. A color-bearing or mixed
branch is `true_alpha_branch_conflict`; a stale opaque/removable/warm-ivory
required element is `transparent_prompt_required_element_conflict`. The
existing removable-solid branch and every existing prompt identity remain
unchanged.

The registered single-image successor
`projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0` / payload hash
`b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a`
is a separate removable-solid branch owned by
GeneratedMediaOpenInkWashOpaqueChromaSuccessorGuide.md. It requires exactly one
fully opaque 1024x1536 PNG with an edge-to-edge perfectly uniform `#00FF00`
field outside the intended foreground. Provider transparency, checkerboard,
variation, halo/vignette/floor/scene/shadow, neighboring fragments, or exact
`#00FF00` foreground pixels are forbidden. Generation stops at the master
receipt; chroma uncomposite/final alpha belongs only to distinct later role
`generated_media_chroma_uncomposite` and project-copy eligibility remains false.

The postprocess-only
`projectbs_character_open_ink_source_bound_green_carrier_uncomposite@1.0.0` /
`b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746`
does not weaken that generation contract. It registers only two exact source
SHA + immutable failed-generation-receipt SHA tuples and their complete
source-derived border/topology/mask evidence. Its owner, algorithm, record,
receipt, no-clobber paths and hard alpha/fringe gates are closed by
GeneratedMediaSourceBoundChromaRecoveryGuide.md. Every other nonuniform or
near-green master remains ineligible; exact `#00FF00` is never claimed.
