# Generated Media Preservation and Packaging Guide

## Purpose and Boundary

Guide Type: current v2 preservation/packaging workflow and record schema. It
starts from either a generated ImageGen v2 record or an exact accepted
post-result capture v1 record, preserves original media, performs
the registered deterministic adapter, and seals an evaluation package. It
never calls a provider, changes prompts, evaluates, promotes, writes Slack,
modifies Unity, or performs Git work.

Legacy v1 PixelLab adapters/formulas are owned only by
`AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md`.

## Authority

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaNoninteractiveExecutionPolicyGuide.md
```

## Required Current Input

```yaml
planningHandoffFile: generated_media_planning_handoff_v2
routingRecordId: generated_media_routing_v2
promptRecordId: generated_media_prompt_v3; strict branch only
generationRecordId: generated_media_generation_v2; required only for strict branch
generationRecordSha256: required only for strict branch
acceptedResultCaptureRecordId: generated_media_accepted_result_capture_v1; mutually exclusive with generationRecordId
acceptedResultCaptureRecordPath: exact canonical project-relative capture record path
acceptedResultCaptureRecordSha256: required with acceptedResultCaptureRecordId
acceptedResultCaptureReceipt: exact generated_media_accepted_result_capture_receipt_v1; accepted-result branch only
acceptedResultCaptureReceiptSha256: exact receipt.receiptPayloadSha256; accepted-result branch only
acceptedPromptEvidence: accepted-result branch only; exact projection defined below
provider: imagegen
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
contentId:
animationRequestId: required only for animation
planningSnapshotHash:
requestedAdapterId:
expectedStructureProfile:
providerResultRefs: non-empty exact generation refs; strict branch only
approvalCostProjection: exact projection from generation record and index; strict branch only
projectTarget: optional informational_only
```

Exactly one input branch is present. The strict branch uses
`promptRecordId`, `generationRecordId`, `generationRecordSha256`,
`providerResultRefs`, and `approvalCostProjection`; the accepted-result branch
uses `acceptedResultCaptureRecordId`, `acceptedResultCaptureRecordPath`,
`acceptedResultCaptureRecordSha256`, the
exact capture receipt/hash, and `acceptedPromptEvidence`. Each branch forbids
every field owned by the other branch. Mixed, partial, or unknown branch fields
fail before payload identity calculation.

When no authoritative `generated_media_prompt_v3` exists, the accepted-result
branch MUST NOT invent one. Animation projects the existing closed recovered
prompt identity below and requires byte equality with the recovered prompt
file:

```yaml
acceptedPromptEvidence:
  source: accepted_result_capture
  providerPromptPayloadHash: exact capture promptEvidence.providerPromptPayloadHash
  promptFileSha256: exact capture promptEvidence.fileSha256
```

For a `character_single_image` capture whose historical prompt is unavailable,
`acceptedPromptEvidence` instead has exactly `source=accepted_result_capture`,
`status=unavailable_observed`, and `claim=not_claimed`. It contains no hash,
path, prompt-record identity, or reconstructed prose. The two prompt-evidence
shapes are mutually exclusive.

### Accepted still historical planning resolution

An accepted-result `character_single_image` does not require a mutable current
planning source file to remain byte-identical to the snapshot that the capture
already binds. The producer derives this closed `acceptedPlanningEvidence`
object; callers do not supply or repair it:

```yaml
source: accepted_capture_handoff_lineage
planningHandoffPath: exact path from the hash-verified routing authoringHandoff
planningHandoffSha256: raw SHA-256 of the resolved immutable handoff blob
planningSnapshotHash: exact capture/routing/handoff value
resolutionMode: origin_main_reachable_git_blob_by_path_and_sha256
sourcePlanningFiles:
  - path: exact handoff sourcePlanningFiles order and path
    role: exact handoff role
    sha256: exact handoff raw-byte SHA-256
    gitBlobOid: exact Git blob object ID containing those bytes
```

Resolution is deterministic and read-only:

1. Re-hash the capture record/index/receipt, then resolve the canonical routing
   record by its capture-bound ID/path/raw SHA. It must be reachable from the
   freshly fetched `origin/main` history.
2. From that routing record only, take `planningHandoffPath`, `requestId`,
   `planningSnapshotHash`, and ordered `sourcePlanningFiles`. Resolve exactly one
   distinct canonical handoff blob at that path from commits reachable from
   fetched `origin/main`; its closed schema and snapshot projection must match.
3. For each ordered source entry, accept the current Git blob when its raw
   SHA-256 matches. Otherwise search only commits reachable from fetched
   `origin/main` at that same path and select the one distinct Git blob whose raw
   SHA-256 equals the handoff value. Repeated commits naming the same blob are
   one resolution.
4. Materialize those exact blob bytes into `planning/` and hash them again.
   Never substitute the current checkout, an unreachable/local-only commit,
   another path, a semantically similar document, or reconstructed content.

A later current-file revision is expected lineage drift, not
`planning_snapshot_mismatch`, and is excluded from preservation identity. If
the capture/routing/handoff chain disagrees, return
`accepted_result_planning_lineage_mismatch`. If no exact reachable blob exists,
return `accepted_result_historical_planning_unresolvable`; if more than one
distinct blob satisfies a supposedly single identity, return
`accepted_result_historical_planning_ambiguous`. No preservation member is
written in any failure case. Strict generation and accepted animation
resolution remain unchanged.

The capture receipt must be a valid `captured` or `reused_identical` receipt,
name the same capture record/path/raw SHA and request/conditional animation
identity, retain `providerCalled=false` and capture submit/retry zero, and
authorize preservation/evaluation but not promotion. Animation retains
historical submit one/retry zero; `character_single_image` may retain the exact
`unavailable_observed` historical counts from its capture. Its
`receiptPayloadSha256` is recomputed from the closed receipt before use.

Every identity/hash/provider/profile must agree. In the strict branch,
generation status must be
`generated`. The generation record, generation index entry, and
`preservationHandoff.approvalCostProjection` must be JCS-byte-identical and its
`costEvidenceSha256` must recompute from the generation record before any
provider result is accessed. `actualCostStatus=unavailable` is not preservation
ready. Missing/foreign paths, project/staging overlap, unsupported provider, or
incomplete readiness block before download.

For the accepted-result animation branch, verify the capture record and index
raw hashes, authenticated acceptance, source task/tool-call identity, all
prompt/settings/reference/master/GIF/frame raw hashes, historical
one-submit/zero-retry facts, and the exact literals `unavailable_observed` and
`not_claimed_post_result_capture`. For `character_single_image`, verify exactly
one PNG canonical capture member, source/target raw hash and byte identity,
authenticated acceptance of that SHA, the distinct `accepted_project_candidate`
role and explicit absence of identity/edit-target authority. The prior
`visual_reference_only_not_identity_or_edit_target` role remains unchanged.
Historical
execution/prompt/settings/count values may only be the capture's closed
`unavailable_observed`/`not_claimed` shape. This branch does not require or synthesize a
generation-v2 cost projection. It is preservation/evaluation-authorized only;
promotion remains forbidden until a later strict evaluation `PASS` and explicit
project mapping.

### Accepted corrective single-image input

An accepted-result `character_single_image` may have one additive corrective
sub-branch when an authenticated bounded pipeline used the published accepted
capture as its only edit input and the official generation role returned one
terminal corrective PNG without retry. It does not create a fake
`generated_media_generation_v2`, prompt record, or second accepted capture.

The preservation input includes the existing accepted capture record/receipt
plus one closed `generated_media_corrective_single_image_input_v1`:

```yaml
schemaVersion: generated_media_corrective_single_image_input_v1
authorityMain: exact 40-lowercase-hex pipeline authority commit
requestId:
contentId:
acceptedResultCaptureRecordId:
acceptedResultCaptureRecordSha256:
acceptedReferenceSha256:
basePromptRecordId:
basePromptRecordSha256:
correctivePromptSha256: SHA-256 of exact LF UTF-8 corrective prompt bytes
executionAttemptId:
sourceGenerationTaskId:
outputPath: exact observed local PNG path; evidence only
outputSha256: exact raw PNG SHA-256
width:
height:
colorMode: RGB | RGBA
providerCalled: true
submitCount: 1
retryCount: 0
```

All members are required and unknown members reject. The accepted capture,
reference, base prompt, request/content, corrective prompt receipt, official
task terminal receipt, attempt ID, output bytes/hash/dimensions/mode and
one-submit/zero-retry facts must agree. The path is evidence, never canonical
identity. Missing or mixed evidence remains `preservation_input_branch_incomplete`;
drift is `corrective_single_image_evidence_mismatch`. This sub-branch performs
no provider call and cannot be used by animation or any non-corrective output.

## Current Adapter Registry

| assetType/domain | adapterId | structureProfile | exact responsibility |
| --- | --- | --- | --- |
| character_single_image/character | imagegen_character_single_image_v2 | character_single_image_v2 | preserve original; conditionally apply the closed accepted-corrective boundary-connected checkerboard alpha normalization below; otherwise apply only approved removable background/no-shadow/outline without crop/scale; record pelvis/root and ground axis |
| icon_single_image/skill or item | imagegen_icon_single_image_v2 | icon_single_image_v2 | preserve original; apply approved background/no-shadow/outline without crop/scale; record visual center |
| background_single_image/stage, battle or environment | imagegen_background_single_image_v2 | background_single_image_v2 | preserve original scene bytes; retain scene composition, viewpoint, depth/playable-area, target/safe-area, consistency lock and scene anchor metadata without icon transforms |
| animation/character | imagegen_animation_master_gif_frames_v2 | animation_gif_frame_set_v2 | provider-native animated GIF original; pelvis/root anchor; exact timeline extraction |
| animation/skill | imagegen_animation_master_gif_frames_v2 | animation_gif_frame_set_v2 | provider-native animated GIF original; effect-origin anchor; exact timeline extraction |

Exactly one row must match provider+asset+domain+adapter+structure. No filename
or judgment fallback is allowed.

Icon and background adapters remain distinct even when both preserve one PNG.
Neither their profile identity, adapter ID, manifest extension nor evaluation
route is interchangeable.

## Accepted-corrective checkerboard alpha normalization

Only the accepted corrective single-image sub-branch may select
`generated_media_border_checkerboard_alpha_v1`. The source must be an RGB PNG
whose exact hash/dimensions/mode match the corrective input. The plan is a
hash-significant conditional preservation member:

```yaml
schemaVersion: generated_media_border_checkerboard_alpha_plan_v1
algorithmId: border_exact_checkerboard_boundary_flood_v1
candidateDerivation: outer_border_exact_two_color_unique_checkerboard
colorMatch: exact_rgb
connectivity: 4
transparentRgbPolicy: retain_source_rgb
alphaForRemovedBackground: 0
alphaForPreservedPixels: 255
pngEncoderName:
pngEncoderVersion:
pngCompressionLevel: integer 0..9
pngFilter: none
pngBitDepth: 8
pngColorType: rgba
pngInterlace: false
```

The algorithm has no threshold, tolerance, blur, morphology, erosion, color
distance, alpha feather, retouch, or semantic mask:

1. Read only the outermost top/right/bottom/left border, in clockwise order.
   It must contain exactly two distinct RGB triplets; sort them
   lexicographically as the candidate colors.
2. From border evidence only, enumerate checker tile size integers from 1
   through `max(width,height)` and x/y phase integers from 0 through
   `tileSizePx-1`. Keep only solutions whose exact alternating two-color
   pattern matches every border pixel. Equivalent parameters may collapse only
   when they produce the same
   full-canvas expected mask. Zero or more than one distinct mask is
   `checkerboard_background_pattern_unsupported`.
3. A noncandidate border pixel or evidence that foreground/ink/wash/effect
   reaches the outer border is `checkerboard_foreground_contact_ambiguous`.
4. Seed every boundary pixel that exactly equals its expected checker color.
   Traverse only 4-connected pixels that exactly equal the expected color at
   their coordinate. Set alpha to zero only for that visited set.
5. Preserve every unvisited pixel byte-for-byte in RGB, including enclosed
   candidate-colored pixels and every nonmatching foreground pixel. Set their
   alpha to 255. With `retain_source_rgb`, all output RGB bytes equal source RGB
   at the same coordinate.
6. Encode one RGBA PNG using the recorded exact encoder name/version,
   compression level and filter. No crop, resize, palette conversion, color
   correction, identity/style/detail edit, or cleanup is allowed.

The record and package carry the closed
`generated_media_border_checkerboard_alpha_receipt_v1`:

```yaml
schemaVersion: generated_media_border_checkerboard_alpha_receipt_v1
plan: exact plan object
beforeSha256:
afterSha256:
width:
height:
candidateColors: exactly two lexicographically ordered [r,g,b] arrays
tileSizePx:
phaseX:
phaseY:
removedPixelCount: positive integer
enclosedCandidatePixelCountPreserved: non-negative integer
nonmatchingPixelCountPreserved: positive integer
rgbChangedPixelCount: 0
foregroundBoundaryContactDetected: false
dimensionsUnchanged: true
alphaChannelPresent: true
outerBoundaryBackgroundRemaining: false
status: valid
```

Reopen the output and recompute every receipt value. The after hash is the
derived primary evaluation member; the before PNG remains immutable source
evidence. Any loss/change outside the exact visited background set, missing
alpha, dimension drift, encoder/receipt mismatch, ink/wash/effect erosion, or
arbitrary threshold fails `checkerboard_alpha_normalization_validation_failed`
and publishes no record/package.

## Animation Packaging Sequence

This sequence applies to new records with
`animationSourceMode=provider_native_animated_gif`. Historical fixed-cell
records remain read-only under their recorded contract.

```text
preserve exact provider-native animated GIF original and hash
-> close and reopen the original GIF
-> verify playable timeline, final frame count, order, timing, loop and full-canvas disposal
-> preserve scale lock and approved vertical motion across the timeline
-> correct drift only by declared profile anchor translation, when approved
-> remove only declared solid generation-background color across all frames
-> apply approved transparent output and outside-silhouette outline consistently
-> save normalized completed GIF
-> close and reopen the normalized GIF
-> extract ordered PNG frames from that reopened GIF timeline
-> hash every source/derived member
```

Per-frame crop, scale, silhouette recenter, canvas change, internal color or
luminance modification is forbidden. Exact outline/background/key-residue
values come from approved input/profile and are never global defaults.
Preservation never constructs an animation from still images, a contact sheet,
a sprite sheet, a video, or independently generated frames. If the generation
ref is not an original playable animated GIF, return
`provider_animated_gif_source_mismatch` without synthesizing a replacement.

The historical `generated_media_attack_gif_final_validation_receipt_v1` does
not authorize accepted-result packaging because it incorrectly treats the
provider result as a final GIF. Existing `provider_native_animated_gif`
preservation above remains separate and unchanged.

When the generation handoff conditionally includes
`generated_media_attack_coherent_master_to_gif_validation_receipt_v2`,
preservation verifies that the provider returned one coherent six-cell master
IMAGE, not a GIF. It verifies `providerDidReturnGif=false`, the provider master
image hash, exactly six cells, completed GIF hash, exact six PNG hashes,
dimensions/frame count, close/reopen state, and reopened-GIF extraction state
all match before copying any member. The generation role, not preservation,
owns master segmentation, GIF construction, GIF close/reopen, PNG extraction,
and any deterministic final-packaging normalization.

Preservation confirms the completed GIF and extracted PNGs retain the same
shared clean left/right margin width basis, fixed pelvis center, fixed ground
baseline, identical scale/timing/global palette, fully opaque background, no
clipping, and no neighboring-cell edge fragments. It does not translate,
remove fragments, derive a width basis, or repair the package. Anchor/baseline
disagreement is `anchor_mapping_mismatch`, scale disagreement is
`scale_lock_violation`, and timeline/palette/background/clipping/fragment or
GIF/PNG member disagreement is `gif_timeline_contract_mismatch`. Preservation
copies only the accepted capture record ID/raw SHA into its conditional input
branch; observed source paths, evidence bytes, task/tool-call envelope and full
guidance are not duplicated into preservation records or evaluation packages.

For exactly six frames with approved uniform `8/1` fps intent, GIF's
centisecond delay unit cannot encode 125 ms. The coherent-master accepted mode
therefore permits exactly one deterministic quantization and no other mixed
timing: `[12,13,12,13,12,13]` centiseconds, equivalently
`[120,130,120,130,120,130]` milliseconds. It preserves chronological order,
has no zero-delay frame, totals 750 ms, and yields exact average 8 fps. This is
the canonical GIF representation of that uniform intent, not timing drift.

The hash-significant plan has exactly:

```yaml
schemaVersion: generated_media_gif_8fps_centisecond_quantization_plan_v1
requestedFpsNumerator: 8
requestedFpsDenominator: 1
frameCount: 6
frameDelayCentiseconds: [12, 13, 12, 13, 12, 13]
frameDelayMilliseconds: [120, 130, 120, 130, 120, 130]
totalDurationMilliseconds: 750
playbackMode: one_shot
loopExtensionPresent: false
decodedPixelPolicy: unchanged_full_canvas_rgba
```

The conditional
`generated_media_gif_8fps_centisecond_quantization_receipt_v1` contains exactly:

```yaml
schemaVersion: generated_media_gif_8fps_centisecond_quantization_receipt_v1
requestedFpsNumerator: 8
requestedFpsDenominator: 1
frameCount: 6
frameDelayCentiseconds: [12, 13, 12, 13, 12, 13]
frameDelayMilliseconds: [120, 130, 120, 130, 120, 130]
totalDurationMilliseconds: 750
averageFpsNumerator: 8
averageFpsDenominator: 1
playbackMode: one_shot
loopExtensionPresent: false
beforeGifSha256:
afterGifSha256:
width:
height:
beforeFramePixelSha256s: exactly six ordered decoded full-canvas pixel hashes
afterFramePixelSha256s: exact same ordered array
gifClosedAndReopened: true
status: valid
```

When this exact receipt is present, an existing `timingUniform=true` assertion
means that the requested timeline intent is uniform `8/1` fps and is satisfied
by the canonical alternating centisecond schedule above. It does not require
all six stored GIF delay integers to be equal. Without this receipt, every
existing literal uniform-timeline validation remains unchanged.

The completed GIF has no NETSCAPE/application loop extension, is closed and
reopened, and re-reports the exact schedule. Canvas, frame count, decoded frame
pixels, global palette semantics, pelvis, baseline, clipping state, fragment
state and chronological order remain unchanged. The before/after GIF byte hash
may differ only because of timing/one-shot metadata. Any other frame count/FPS,
schedule, total, zero delay, loop representation, arbitrary mixed timing, pixel
or canvas change remains `gif_timeline_contract_mismatch`. Existing
provider-native and other timing contracts are unchanged.

## Preservation Record v2

Hash payload:

```yaml
schemaVersion: generated_media_preservation_hash_payload_v2
requestId:
assetType:
domainType:
contentId:
animationRequestId: required only for animation
planningSnapshotHash:
routingRecordId:
promptRecordId: strict branch only
acceptedPromptEvidence: accepted-result branch only; mutually exclusive with promptRecordId
acceptedPlanningEvidence: required only for accepted-result character_single_image; forbidden for strict and animation
generationRecordId: strict branch only
generationRecordSha256: strict branch only
acceptedResultCaptureRecordId: mutually exclusive alternative
acceptedResultCaptureRecordPath: accepted-result branch only
acceptedResultCaptureRecordSha256: required with acceptedResultCaptureRecordId
acceptedResultCaptureReceiptSha256: accepted-result branch only
correctiveSingleImageInput: accepted corrective single-image sub-branch only
singleImageBackgroundNormalizationPlan: same sub-branch only
gifTimingQuantizationPlan: exact six-frame 8fps coherent-master sub-branch only
provider: imagegen
adapterId:
structureProfile:
providerResultRefs: strict branch only
approvalCostProjection: strict branch only
```

```text
payloadHash=SHA256(canonical_json(hashPayload))
preservationRecordId=gmpreserve2.{assetType}.{contentId}.{optionalAnimationRequestId}.{payloadHash[0:20]}
non-animation path:
AgentDocs/planning-data/generated-media-preservation/v2/{assetType}/{contentId}/{preservationRecordId}.json
animation path:
AgentDocs/planning-data/generated-media-preservation/v2/animation/{contentId}/{animationRequestId}/{preservationRecordId}.json
```

Closed record:

```yaml
schemaVersion: generated_media_preservation_v2
preservationRecordId:
preservationPayloadHash:
requestId:
assetType:
domainType:
contentId:
animationRequestId: required only for animation
planningSnapshotHash:
routingRecordId:
promptRecordId: strict branch only
acceptedPromptEvidence: accepted-result branch only; mutually exclusive with promptRecordId
acceptedPlanningEvidence: required only for accepted-result character_single_image; forbidden for strict and animation
generationRecordId: strict branch only
generationRecordSha256: strict branch only
acceptedResultCaptureRecordId: mutually exclusive alternative
acceptedResultCaptureRecordPath: accepted-result branch only
acceptedResultCaptureRecordSha256: required with acceptedResultCaptureRecordId
acceptedResultCaptureReceiptSha256: accepted-result branch only
correctiveSingleImageInput: accepted corrective single-image sub-branch only
singleImageBackgroundNormalizationPlan: same sub-branch only
singleImageBackgroundNormalizationReceipt: same sub-branch only after transform
gifTimingQuantizationPlan: exact six-frame 8fps coherent-master sub-branch only
gifTimingQuantizationReceipt: same sub-branch only after GIF reopen validation
provider: imagegen
adapterId:
structureProfile:
providerResultRefs: strict branch only
approvalCostProjection: strict branch only
originalMembers: []
derivedMembers: []
memberHashes: []
state:
attempts: []
failureType: optional
packageId: optional after seal
createdAt:
validation:
```

Unknown fields reject. Same payload/bytes is idempotent reuse; same ID with
different bytes is collision. Record is append-only while active and immutable
after seal.

## State, Failure, Output

```text
not_started -> refs_resolved -> originals_preserved -> transformed
-> gif_saved (animation) -> gif_reopened (animation) -> members_extracted
-> manifest_ready -> package_sealed -> evaluation_handoff_ready
```

Typed failures are limited to the common/type and Preservation Extension
registries in GeneratedMediaImageGenOnlyContractGuide.md:

```text
unsupported_preservation_adapter
missing_planning_handoff_v2
missing_routing_v2
missing_prompt_v3
missing_generation_v2
missing_accepted_result_capture_v1
accepted_result_capture_hash_mismatch
accepted_result_capture_not_authorized
accepted_result_capture_receipt_mismatch
accepted_result_prompt_evidence_mismatch
preservation_input_branch_conflict
preservation_input_branch_incomplete
preservation_input_unknown_field
accepted_result_planning_lineage_mismatch
accepted_result_historical_planning_unresolvable
accepted_result_historical_planning_ambiguous
generation_record_hash_mismatch
unsupported_provider
provider_result_ref_missing
source_hash_mismatch
provider_animated_gif_source_mismatch
gif_timeline_contract_mismatch
corrective_single_image_evidence_mismatch
checkerboard_background_pattern_unsupported
checkerboard_foreground_contact_ambiguous
checkerboard_alpha_normalization_validation_failed
fixed_cell_contract_mismatch
scale_lock_violation
anchor_mapping_mismatch
vertical_motion_policy_violation
chroma_key_scope_violation
gif_first_sequence_violation
frame_order_mismatch
member_hash_mismatch
manifest_validation_failed
preservation_record_collision
package_collision
package_seal_failed
```

Success returns preservation record/path/hash, adapter/profile, original and
derived members/hashes, package ID/path/hash, readiness/blockers and a separate
evaluation request. It never returns an evaluation verdict.

## Validation

- input versions are handoff/routing v2, prompt v3, preservation v2 and exactly
  one of generation v2 or accepted-result capture v1;
- provider is ImageGen and one current adapter row matches;
- approval/cost projection equals the generation record, generation index and
  preservation handoff, and actual cost evidence is preservation-ready;
- animation unit has exactly one ID and correct profile anchor;
- provider-native GIF-first sequence, exact timeline and structure
  profile/member schema agree;
- staging source differs from project target;
- accepted-result capture input preserves unavailable capability/cost truth,
  never asserts past gate success, and cannot authorize promotion;
- accepted-result `character_single_image` preserves exactly one canonical PNG
  whose bytes equal the capture source and rejects any animation/still mixture;
- accepted-result `character_single_image` packages the exact capture-bound
  historical planning Git blobs even when later unrelated planning changed the
  current path; unreachable, missing, ambiguous, or reconstructed evidence fails;
- no provider/evaluation/promotion/Git stage executes.
