# Generated Media Open-Ink Animation Opaque-Chroma Guide

## Authority and compatibility

This guide registers one additive `character_animation_v2` authority. It does
not change the existing single-image opaque-chroma, direct true-alpha,
provider-native GIF, coherent-master, sparse-motion, attack-motion, generation,
or postprocess profiles and records.

The canonical payload is
`helpers/generated_media_open_ink_animation_opaque_chroma_profile_v1.json`.
Its RFC 8785 JCS SHA-256 is
`da38a4c91bbe3a808f09f1c24763cd3cece02518a2d1398f7294ce3eedb3f7c8`.
It registers expression profile
`projectbs_character_open_ink_wash_animation_opaque_chroma_master@1.0.0`
and execution profile
`projectbs_character_open_ink_animation_opaque_chroma_identity_anchored@1.0.0`
for exactly `animation+character+character_animation_v2`.

The complete open-ink v2 base key/hash and all 19 members plus 9+9 locks remain
byte-semantically unchanged. Only the provider master/output stage is the
registered successor: one fully opaque RGB PNG, 1536x1024, exact 3x2 row-major
grid, six 512x512 cells, and one uniform connected exact `#00FF00` carrier
outside intended foreground. Provider alpha, foreground exact key color,
checkerboard, gradient, texture, lighting variation, halo, vignette, floor,
scene, shadow, and neighboring fragments are forbidden.

## Identity and motion selections

`animationIdentityAuthoritySelection` has schema
`generated_media_animation_identity_equipment_authority_selection_v1` and
exactly the twelve ordered names declared by `requiredSelectionMembers` in the
profile. The selected content ID must match the animation character. Its path,
byte length, and raw SHA must equal one registered fixture row. The evaluation
record ID/SHA must independently establish the MAIN PNG as accepted/evaluated
identity and equipment evidence. Under trusted policy
`generated_media_trusted_local_evaluated_main_reference@1.0.0`, no provider
receipt is required. The local path is read-only and is never copied,
normalized, edited, or treated as project publication authority.

Its role is only identity, equipment, and orientation authority. It is never an
edit source/target or motion, style, pose, framing, background, or pixel-copy
authority. The actual provider call contains stored prompt plus exactly one
`referenced_image_paths` entry equal to the selected registered local path.

`animationMotionLineageSelection` has schema
`generated_media_animation_motion_lineage_selection_v1` and exactly the eleven
ordered names declared by its `requiredSelectionMembers`. It may transfer only
Grade 1 planning strings, ordered phase names, motion topology, direction, and
loop-closure intent. `referenceRole=motion_topology_only`,
`providerReferenceAllowed=false`, and `pixelTransferAllowed=false` are fixed.
The lineage path/hash and evaluation record ID/hash must resolve exactly, but
no lineage raster may enter provider references, output members, or pixel
comparison authority.

Both selections are top-level siblings. Their canonical JCS values project
unchanged through planning handoff, routing hash payload/record,
`normalizedRequest`, `authoringHandoff`, prompt hash payload/record, detached
generation handoff, and the generation/postprocess boundary as stated by each
profile projection. Nesting either under `typeSpecification`, omitting a member,
adding a member, changing a role, or mixing content identities fails no-write.

## Routing, authoring, and execution

The current route remains `character_animation_v2` with
`assetType=animation`, `domainType=character`, provider `imagegen`, and the
current ImageGen animation authoring/generation prompts. This branch uses
`animationSourceMode=provider_opaque_chroma_3x2_master` and
`extractionMode=postprocess_exact_cell_chroma_root_gif_v1`; it must not be
rewritten as `provider_native_animated_gif`, direct true alpha, or an accepted
coherent-master mode.

The prompt must project the complete base style, exact master contract,
identity-only role, motion-topology-only role, ordered six distinct phases,
frame 5-to-0 closure, no whole-body mirror, and later postprocess boundary.
The call surface is exactly `image_gen.imagegen(prompt,
referenced_image_paths=[selectedMain])`. Execution mode is
`builtin_imagegen_authenticated_animation_identity_anchored_single_submit_v1`;
the idempotency key is `gmanimidentity1.{executionScopeSha256[0:20]}`. The
closed scope payload includes request/animation IDs, prompt record/hash,
profile key/hash, both complete selections and their hashes, call projection
hash, submit maximum 1, retry maximum 0, and authority main SHA. JCS SHA-256 of
that payload is `executionScopeSha256`. No unavailable capability, settings,
cost, provider enforcement, or provider receipt evidence is fabricated.

Generation validates one returned RGB PNG against every master gate and records
only `generated_media_animation_opaque_chroma_generation_record_v1`. Success
sets `nextStep=generated_media_animation_postprocess` and
`projectCopyEligible=false`. It does not split, uncomposite, normalize, encode a
GIF, preserve, evaluate, or copy. Any returned-format, grid, carrier, foreground
key collision, forbidden feature, identity/equipment, motion-phase, duplicate,
mirror, or idempotency failure consumes the sole submit and permits no retry.

## Postprocess contract

Only role `generated_media_animation_postprocess` may consume the exact master
record. It writes record first with `wx`, then its sorted index by CAS/no-clobber.
An identical rerun is `reused_identical`; a collision is terminal.

The role validates the exact 3x2 grid and seams, then splits the six fixed cell
rectangles in row-major order without neighbor transfer. It clears only exact
RGB `(0,255,0)` after proving the full-master carrier is one connected
background and that foreground has no key collision. Transparent RGB becomes
zero. It applies only measured integer translation needed to set each frame's
root to `(256,300)` and baseline to `448`; camera, scale, and root-relative
centroid stay fixed, independent silhouette recentering is false, and every
subject/equipment/effect pixel remains at least 48 px inside the 512x512 cell.

Output is six ordered true-alpha RGBA PNGs and one GIF made from those same
frames: six frames, 150 ms each, total 900 ms, infinite loop, exact order,
reopened and revalidated. All frame hashes and ordered phase names must be
unique; duplicate/repeated phases and whole-body mirroring fail. Frame 5-to-0
closure, no clipping, no fragments, root/baseline drift 0, fixed scale/camera/
centroid, alpha/fringe, and member hashes are pre-completion gates.

The record is `generated_media_animation_opaque_chroma_postprocess_record_v1`;
the receipt is `generated_media_animation_opaque_chroma_postprocess_receipt_v1`.
Their hash payload includes the exact generation record/master SHA, profile
key/hash, split coordinates, ordered source-cell hashes, alpha masks, measured
integer translations, ordered PNG hashes, GIF hash/timeline, reopen evidence,
and all validation results. Only a completed immutable receipt may enter
preservation and independent evaluation. `projectCopyEligible` remains false
until evaluation completes PASS with `passForProjectCopy=true`.

## Failure tokens

- `open_ink_animation_opaque_chroma_profile_mismatch`
- `animation_identity_authority_selection_missing`
- `animation_identity_authority_fixture_mismatch`
- `animation_identity_authority_evaluation_mismatch`
- `animation_identity_authority_role_invalid`
- `animation_motion_lineage_selection_missing`
- `animation_motion_lineage_projection_mismatch`
- `animation_motion_lineage_media_transfer_forbidden`
- `animation_opaque_chroma_branch_conflict`
- `animation_opaque_chroma_provider_master_mismatch`
- `animation_opaque_chroma_grid_or_seam_mismatch`
- `animation_opaque_chroma_carrier_mismatch`
- `animation_opaque_chroma_foreground_key_collision`
- `animation_opaque_chroma_duplicate_or_repeated_phase`
- `animation_opaque_chroma_whole_body_mirror_forbidden`
- `animation_opaque_chroma_postprocess_evidence_mismatch`
- `animation_opaque_chroma_root_baseline_drift`
- `animation_opaque_chroma_alpha_fringe_or_fragment_failure`
- `animation_opaque_chroma_record_collision`

All routing/authoring failures occur before provider access. Postprocess failures
never recall the provider or mutate the master. Existing branch failure tokens
and meanings remain unchanged.
