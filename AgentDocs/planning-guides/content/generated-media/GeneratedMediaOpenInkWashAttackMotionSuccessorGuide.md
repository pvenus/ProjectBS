# Generated Media Open Ink-Wash Attack Motion Successor Guide

## 1. Scope

This guide owns one additive animation-only composed successor. The existing
`projectbs_character_sparse_ink_pastel_motion@1.0.0` is not a valid successor
for open-ink v2: its 3.75-4.25 head range, two-role palette and sparse pigment
budget do not preserve the open-ink v2 4-5 head range, three separate palette
roles, 70 percent negative-space contract, surface-detail contract, or output
conformance contract. Neither existing profile is changed or reinterpreted.

## 2. Canonical payload

The following JSON object is canonical and immutable.

```json
{
  "expressionProfileKey": "projectbs_character_open_ink_wash_attack_motion@1.0.0",
  "baseProfileBinding": {
    "expressionProfileKey": "projectbs_character_open_ink_wash_dynamic_contour@2.0.0",
    "expressionProfilePayloadHash": "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5",
    "mutationPolicy": "base_payload_locks_and_meaning_unchanged",
    "referencePromptRecord": "exact_hash_verified_required"
  },
  "animationApplicability": {
    "structureProfiles": ["character_animation_v2"],
    "motionClass": "attack",
    "singleImageSelection": "prohibited",
    "selection": "exact_open_ink_v2_reference_plus_approved_attack_motion_and_true_alpha_bindings_required"
  },
  "motionProjectionContract": {
    "requiredPlanningBindings": ["motionDirection", "swordArc", "torsoRotation", "keyPoseOrder", "frameContinuityAnchors", "dynamicPigment"],
    "orderedFrameCount": 6,
    "singleCoherentAttack": "required",
    "secondAttack": "prohibited",
    "staticRepeatedFrames": "prohibited",
    "identityEquipmentContinuity": "required",
    "baseStyleProjection": "complete_open_ink_v2_members_and_locks_required"
  },
  "trueAlphaProjectionBinding": {
    "projectionKey": "generated_media_true_alpha_foreground@1.0.0",
    "projectionPayloadHash": "2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108",
    "selectionSchemaVersion": "generated_media_transparent_foreground_selection_v1",
    "requirements": ["identical_canvas", "fixed_pelvis_world_root", "fixed_ground_baseline", "pelvis_drift_max_px_0", "baseline_drift_max_px_0", "fixed_scale", "independent_silhouette_recentering_false", "background_flicker_false", "clipping_false", "neighboring_fragments_false", "sword_and_effects_inside_safe_margin", "dynamic_pigment_excluded_from_anchor_movement"]
  },
  "authoringProjectionContract": {
    "baseProfileProjection": "verbatim_complete",
    "requiredApprovedMotionBindings": ["motionDirection", "swordArc", "torsoRotation", "keyPoseOrder", "frameContinuityAnchors", "dynamicPigment"],
    "evidencePolicy": "base_reference_motion_and_true_alpha_evidence_required_for_every_projected_lock",
    "conflictPolicy": "block_before_routing_or_prompt_publication"
  },
  "negativeAnimationLock": [
    {"constraintId": "char_open_wash_attack_negative_base_style_substitution", "statement": "Do not replace, weaken, or reinterpret any open-ink v2 proportion, age, contour, mok-seon, pigment, palette-role, negative-space, surface-detail, background, identity, equipment, reference-role, or output-conformance lock."},
    {"constraintId": "char_open_wash_attack_negative_motion_semantic_drift", "statement": "No static repeated frames, reversed action, reordered key poses, second attack, identity redesign, equipment substitution, or weapon discontinuity."},
    {"constraintId": "char_open_wash_attack_negative_anchor_drift", "statement": "No pelvis or world-root drift, ground-baseline drift, scale pumping, per-frame crop or padding, or independent silhouette recentering."},
    {"constraintId": "char_open_wash_attack_negative_alpha_bounds_failure", "statement": "No matte, checkerboard, halo, vignette, floor, scene, cast shadow, residual fringe, background flicker, clipping, neighboring fragments, out-of-margin sword or effects, or dynamic-pigment anchor contamination."}
  ],
  "positiveAnimationLock": [
    {"constraintId": "char_open_wash_attack_positive_base_style_continuity", "statement": "Preserve the complete hash-verified open-ink v2 profile and exact character identity and equipment across all six ordered frames."},
    {"constraintId": "char_open_wash_attack_positive_coherent_attack", "statement": "Render one coherent approved attack through ordered anticipation, acceleration, contact, stop, and recovery using the exact planning motion bindings."},
    {"constraintId": "char_open_wash_attack_positive_anchor_continuity", "statement": "Keep identity, equipment, weapon, support, pelvis or world-root, ground baseline, canvas, and scale continuous while dynamic pigment remains outside anchor measurement."},
    {"constraintId": "char_open_wash_attack_positive_true_alpha", "statement": "Deliver the completed GIF and six ordered true-alpha PNG frames only after every exact transparent-foreground anchor, alpha-mask, fringe, flicker, clipping, fragment, and safe-margin gate passes."}
  ]
}
```

RFC 8785 JCS over this exact payload has registered SHA-256
`07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c`.
Array order and every displayed value are normative; object member order is not.

## 3. Selection and failure behavior

The successor is eligible only for one attack animation whose immutable
reference prompt record exactly binds the base key/hash, whose planning handoff
contains all six exact motion bindings, and whose exact true-alpha selection
matches the key/hash/schema above with frame count 6. Routing stores the new
successor key/hash while retaining the base binding unchanged. It never aliases
or selects the sparse-motion profile.

Failure tokens are:

- `character_style_profile_conflict` for sparse-motion or any other requested
  successor key paired with the open-ink v2 reference
- `open_ink_attack_successor_reference_mismatch`
- `open_ink_attack_motion_not_attack`
- `missing_open_ink_attack_motion_bindings`
- `open_ink_attack_true_alpha_binding_mismatch`
- `open_ink_attack_base_projection_mismatch`
- `open_ink_attack_evidence_omission`
- `provider_prompt_open_ink_attack_projection_missing`
- `character_generation_open_ink_attack_style_gate_failed`
- `character_generation_open_ink_attack_motion_continuity_gate_failed`
- `character_generation_open_ink_attack_true_alpha_gate_failed`
- `character_evaluation_open_ink_attack_style_gate_failed`
- `character_evaluation_open_ink_attack_motion_continuity_gate_failed`
- `character_evaluation_open_ink_attack_true_alpha_gate_failed`

All routing/authoring failures occur before provider access. Generation cannot
complete without the existing closed true-alpha output receipt. Evaluation
applies the same true-alpha failures as pre-score hard fails and scores only
after the base style, motion continuity and true-alpha gates pass.
