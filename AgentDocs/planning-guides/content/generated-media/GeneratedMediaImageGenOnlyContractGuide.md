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
| `stage_background_single_image_v2` | `background_single_image` | `stage` | `stage_background@2.0.0` | `ImageGenBackgroundPromptAuthoringPrompt.md` | `ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `battle_background_single_image_v2` | `background_single_image` | `battle` | `battle_background@2.0.0` | `ImageGenBackgroundPromptAuthoringPrompt.md` | `ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
| `environment_background_single_image_v2` | `background_single_image` | `environment` | `environment_background@2.0.0` | `ImageGenBackgroundPromptAuthoringPrompt.md` | `ImageGenBackgroundGenerationPrompt.md` | `background_single_image_v2` |
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

Unknown fields are rejected. Animation records without one scalar
animationRequestId, or non-animation records containing it, are invalid.

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
providerSettings: exact closed provider request settings object
providerSettingsSha256: SHA-256 of canonicalJson(providerSettings)
```

The request, asset/domain/content identity, optional animation identity,
snapshot, selected registry/profile, prompt record identity and exact bytes,
copy-ready prompt bytes, provider payload, provider/tool/interface, and exact
settings are all approval bindings. Changing any bound value requires a new
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
{"assetType":"character_single_image","contentId":"seojin","domainType":"character","planningSnapshotHash":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","promptFileSha256":"3313e882e877653bc059fa85bfea8299940f88360673b1ba39d111106c2803c9","promptRecordId":"gmprompt3.character_single_image.character.seojin.1.e12ee2ebe2787f10e8a5","promptRecordSha256":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb","provider":"imagegen","providerInterface":"configured_imagegen_capability","providerPromptPayloadHash":"6f855d4140bc32db400af207899c1ab3a981d4b9df17d3313fe594d05698809d","providerSettings":{"background":"opaque","format":"png","quality":"high","size":"1024x1024"},"providerSettingsSha256":"a1e5fb882b29876db5770023e913b6e62056ac1395a30765136132460be5ce4c","providerTool":"imagegen","registryRowId":"character_single_image_v2","requestId":"gmreq.character.seojin.1","schemaVersion":"generated_media_provider_execution_scope_hash_payload_v1","structureProfile":"character_single_image_v2"}
```

Its fixed `scopeHash` is
`be78667b021ad8a15e3b02cb00198249304092d723f20f5a90c3b969a09d01bb`.
The canonical approval JSON is:

```json
{"approvalEvidence":"codex-thread:019ffabb-97f6-7af3-abaa-f70747dc125f/message:approval-1","approvedAt":"2026-08-13T12:00:00+09:00","approvedBy":"user:contract-reviewer","estimateUnavailablePolicy":"block","maxAttempts":2,"maxCost":{"amount":"0.250000","currency":"USD","kind":"iso_currency"},"schemaVersion":"generated_media_provider_execution_approval_v1","scopeHash":"be78667b021ad8a15e3b02cb00198249304092d723f20f5a90c3b969a09d01bb"}
```

Its fixed approval SHA-256 is
`4d974e3c0abc88354f32b49d42f9c03c228c30d686f27eac6f93c1ff663f28fd`.
The vector proves same-scope stability; prompt record/file/payload, settings and
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
invalid_provider_execution_approval
provider_execution_scope_mismatch
provider_cost_unit_mismatch
provider_cost_estimate_unavailable
provider_cost_limit_exceeded
provider_actual_cost_unavailable
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
call, the approval object and recomputed scope must pass sections 6.1-6.2, the
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
