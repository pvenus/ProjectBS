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
```

## Required Current Input

```yaml
planningHandoffFile: generated_media_planning_handoff_v2
routingRecordId: generated_media_routing_v2
promptRecordId: generated_media_prompt_v3
generationRecordId: generated_media_generation_v2; required only for strict branch
generationRecordSha256: required only for strict branch
acceptedResultCaptureRecordId: generated_media_accepted_result_capture_v1; mutually exclusive with generationRecordId
acceptedResultCaptureRecordSha256: required with acceptedResultCaptureRecordId
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
`generationRecordId`, `generationRecordSha256`, `providerResultRefs`, and
`approvalCostProjection`; the accepted-result branch uses only
`acceptedResultCaptureRecordId` and `acceptedResultCaptureRecordSha256` for its
source envelope and forbids those four strict-only fields.

Every identity/hash/provider/profile must agree. In the strict branch,
generation status must be
`generated`. The generation record, generation index entry, and
`preservationHandoff.approvalCostProjection` must be JCS-byte-identical and its
`costEvidenceSha256` must recompute from the generation record before any
provider result is accessed. `actualCostStatus=unavailable` is not preservation
ready. Missing/foreign paths, project/staging overlap, unsupported provider, or
incomplete readiness block before download.

For the accepted-result branch, verify the capture record and index raw hashes,
authenticated acceptance, source task/tool-call identity, all prompt/settings/
reference/master/GIF/frame raw hashes, historical one-submit/zero-retry facts,
and the exact literals `unavailable_observed` and
`not_claimed_post_result_capture`. This branch does not require or synthesize a
generation-v2 cost projection. It is preservation/evaluation-authorized only;
promotion remains forbidden until a later strict evaluation `PASS` and explicit
project mapping.

## Current Adapter Registry

| assetType/domain | adapterId | structureProfile | exact responsibility |
| --- | --- | --- | --- |
| character_single_image/character | imagegen_character_single_image_v2 | character_single_image_v2 | preserve original; apply approved removable background/no-shadow/outline without crop/scale; record pelvis/root and ground axis |
| icon_single_image/skill or item | imagegen_icon_single_image_v2 | icon_single_image_v2 | preserve original; apply approved background/no-shadow/outline without crop/scale; record visual center |
| background_single_image/stage, battle or environment | imagegen_background_single_image_v2 | background_single_image_v2 | preserve original scene bytes; retain scene composition, viewpoint, depth/playable-area, target/safe-area, consistency lock and scene anchor metadata without icon transforms |
| animation/character | imagegen_animation_master_gif_frames_v2 | animation_gif_frame_set_v2 | provider-native animated GIF original; pelvis/root anchor; exact timeline extraction |
| animation/skill | imagegen_animation_master_gif_frames_v2 | animation_gif_frame_set_v2 | provider-native animated GIF original; effect-origin anchor; exact timeline extraction |

Exactly one row must match provider+asset+domain+adapter+structure. No filename
or judgment fallback is allowed.

Icon and background adapters remain distinct even when both preserve one PNG.
Neither their profile identity, adapter ID, manifest extension nor evaluation
route is interchangeable.

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
promptRecordId:
generationRecordId: strict branch only
generationRecordSha256: strict branch only
acceptedResultCaptureRecordId: mutually exclusive alternative
acceptedResultCaptureRecordSha256: required with acceptedResultCaptureRecordId
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
promptRecordId:
generationRecordId: strict branch only
generationRecordSha256: strict branch only
acceptedResultCaptureRecordId: mutually exclusive alternative
acceptedResultCaptureRecordSha256: required with acceptedResultCaptureRecordId
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
generation_record_hash_mismatch
unsupported_provider
provider_result_ref_missing
source_hash_mismatch
provider_animated_gif_source_mismatch
gif_timeline_contract_mismatch
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
- no provider/evaluation/promotion/Git stage executes.
