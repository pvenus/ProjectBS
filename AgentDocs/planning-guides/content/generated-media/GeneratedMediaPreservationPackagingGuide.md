# Generated Media Preservation and Packaging Guide

## Purpose and Boundary

Guide Type: current v2 preservation/packaging workflow and record schema. It
starts from a generated ImageGen v2 record, preserves original media, performs
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
generationRecordId: generated_media_generation_v2
generationRecordSha256:
provider: imagegen
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
contentId:
animationRequestId: required only for animation
planningSnapshotHash:
requestedAdapterId:
expectedStructureProfile:
providerResultRefs: non-empty exact generation refs
approvalCostProjection: exact projection from generation record and index
projectTarget: optional informational_only
```

Every identity/hash/provider/profile must agree. Generation status must be
`generated`. The generation record, generation index entry, and
`preservationHandoff.approvalCostProjection` must be JCS-byte-identical and its
`costEvidenceSha256` must recompute from the generation record before any
provider result is accessed. `actualCostStatus=unavailable` is not preservation
ready. Missing/foreign paths, project/staging overlap, unsupported provider, or
incomplete readiness block before download.

## Current Adapter Registry

| assetType/domain | adapterId | structureProfile | exact responsibility |
| --- | --- | --- | --- |
| character_single_image/character | imagegen_character_single_image_v2 | character_single_image_v2 | preserve original; apply approved removable background/no-shadow/outline without crop/scale; record pelvis/root and ground axis |
| icon_single_image/skill or item | imagegen_icon_single_image_v2 | icon_single_image_v2 | preserve original; apply approved background/no-shadow/outline without crop/scale; record visual center |
| background_single_image/stage, battle or environment | imagegen_background_single_image_v2 | background_single_image_v2 | preserve original scene bytes; retain scene composition, viewpoint, depth/playable-area, target/safe-area, consistency lock and scene anchor metadata without icon transforms |
| animation/character | imagegen_animation_master_gif_frames_v2 | animation_gif_frame_set_v2 | provider-native animated GIF or generation-owned completed GIF original; pelvis/root anchor; exact reopened timeline extraction |
| animation/skill | imagegen_animation_master_gif_frames_v2 | animation_gif_frame_set_v2 | provider-native animated GIF or generation-owned completed GIF original; effect-origin anchor; exact reopened timeline extraction |

Exactly one row must match provider+asset+domain+adapter+structure. No filename
or judgment fallback is allowed.

Icon and background adapters remain distinct even when both preserve one PNG.
Neither their profile identity, adapter ID, manifest extension nor evaluation
route is interchangeable.

## Animation Packaging Sequence

This sequence applies to new records with
`animationSourceMode=provider_native_animated_gif` or
`animationSourceMode=generation_role_coherent_master_to_gif`. Historical
fixed-cell records remain read-only under their recorded contract.

```text
preserve exact provider-native animated GIF original and hash, or preserve the
generation-owned completed GIF original and hash made from one coherent
six-cell master image for coherent-master mode
-> close and reopen that original/completed GIF
-> verify playable timeline, final frame count, order, timing, loop and full-canvas disposal
-> preserve scale lock and approved vertical motion across the timeline
-> correct drift only by declared profile anchor translation, when approved
-> remove only verified neighboring-cell edge fragments when declared by the
   generation compact receipt
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
a sprite sheet, a video, or independently generated frames. Coherent-master
mode arrives only after the official generation role has already consumed one
coherent six-cell master image, constructed the completed GIF, and reopened its
timeline. If the generation ref is not a playable GIF valid for its declared
source mode, return `provider_animated_gif_source_mismatch` without
synthesizing a replacement.

When the generation handoff conditionally includes
`generated_media_attack_gif_final_validation_receipt_v1`, preservation verifies
its closed member set, declared source mode, completed GIF hash/dimensions/frame
count, and optional evidence-only accepted GIF hash before opening media. It
then confirms the reopened timeline preserves the generation role's shared clean left/right margin width basis,
fixed pelvis center, fixed ground baseline, identical scale/timing/global palette,
fully opaque background, and no clipping or neighboring-cell edge fragments.
Required
`pelvisDriftMaxPx` and `baselineDriftMaxPx` are both exactly zero.

Preservation does not derive a new width basis, crop/recenter frames, remove
neighboring fragments, repair palette/background, or convert a failed receipt
to valid. Anchor/baseline disagreement is `anchor_mapping_mismatch`, scale
disagreement is `scale_lock_violation`, and timing/palette/background/clipping/
fragment disagreement is `gif_timeline_contract_mismatch`. The accepted-result
guidance provenance remains detached future-role guidance and is not copied
into preservation records or evaluation packages.

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
generationRecordId:
generationRecordSha256:
provider: imagegen
adapterId:
structureProfile:
providerResultRefs: []
approvalCostProjection:
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
generationRecordId:
generationRecordSha256:
provider: imagegen
adapterId:
structureProfile:
providerResultRefs: []
approvalCostProjection:
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

- input versions are handoff/routing v2, prompt v3, generation/preservation v2;
- provider is ImageGen and one current adapter row matches;
- approval/cost projection equals the generation record, generation index and
  preservation handoff, and actual cost evidence is preservation-ready;
- animation unit has exactly one ID and correct profile anchor;
- provider-native GIF or generation-owned coherent-master completed-GIF sequence,
  exact reopened timeline and structure
  profile/member schema agree;
- staging source differs from project target;
- no provider/evaluation/promotion/Git stage executes.
