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
background_single_image
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

For `animationSubjectType=character`, the four flat reference/profile fields
above are mandatory. `referencePromptRecordPath` identifies an immutable
`generated_media_prompt_v3` character single-image record;
`referencePromptRecordSha256` is the lowercase SHA-256 of its exact file bytes.
The record must contain the canonical `expressionProfilePayload`, and its key
and recomputed payload hash must equal both handoff fields. For
`animationSubjectType=skill_effect`, all four fields are prohibited and absent.

Section 8.3 is the sole token authority for reference/profile authoring
failures. Character animation applies all reference and expression-profile
tokens there; character single-image authoring applies only its expression-
profile tokens; skill animation applies only
`unexpected_character_style_reference`. Do not create a 3.4-local alias.

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

One authenticated current user message authorizes exactly one work unit:

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

Before submit, re-read and hash the request, prompt JSON/Markdown, prompt
payload, settings intent, references and exact single work-unit identity. Any
drift blocks. Check that no preview record exists and no active submission is
associated with the same scope. `submitCountMaximum=1` and
`retryCountMaximum=0` are constants: failure, timeout or ambiguity does not
authorize another call. Additional output or retry requires a new exact scope
and a new current user approval.

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
| `hosted_preview_scope_mismatch` | approval hash does not equal recomputed current scope |
| `hosted_preview_unknown_setting` | execution would require an unexposed or invented setting/default |
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
missing_planning_capture_inputs
invalid_planning_capture_timestamp
missing_source_planning_path
duplicate_source_planning_path
unresolved_source_planning_path
planning_capture_identity_mismatch
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
character_style_profile_conflict
character_animation_style_lock_mismatch
missing_character_proportion_projection
character_proportion_out_of_range
missing_animation_safe_detail_budget
missing_character_color_value_budget
character_profile_evidence_omission
character_generation_proportion_gate_failed
character_generation_detail_density_gate_failed
character_generation_color_value_gate_failed
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

The lock-array tokens `missing_positive_style_lock`,
`missing_negative_style_lock`, `style_lock_evidence_incomplete`, and
`provider_prompt_style_lock_missing` apply only to the two registered
lock-array profiles. They MUST NOT be returned for the sparse profile.

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
character_generation_proportion_gate_failed
character_generation_detail_density_gate_failed
character_generation_color_value_gate_failed
character_generation_sparse_contour_gate_failed
character_generation_sparse_omission_budget_gate_failed
character_generation_sparse_pigment_budget_gate_failed
character_generation_sparse_motion_gate_failed
character_generation_identity_anchor_gate_failed
missing_hosted_preview_approval
invalid_hosted_preview_approval
hosted_preview_scope_mismatch
hosted_preview_unknown_setting
hosted_preview_prompt_drift
hosted_preview_reference_drift
hosted_preview_submit_limit_exceeded
hosted_preview_retry_forbidden
hosted_preview_output_missing
hosted_preview_output_hash_mismatch
hosted_preview_preservation_forbidden
hosted_preview_promotion_forbidden
```

After a prompt record is immutable and before any submit boundary, character
generation repeats three semantic gates over the exact profile payload,
evidence map, and `scenePromptOriginal`. It rejects any wording that allows
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
