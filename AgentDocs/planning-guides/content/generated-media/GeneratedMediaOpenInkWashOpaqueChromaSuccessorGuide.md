# Generated Media Open Ink-Wash Opaque Chroma Successor Guide

## 1. Scope

This guide owns one additive `character_single_image_v2` successor. It preserves
the hash-bound open-ink v2 character style while replacing only its warm-ivory
provider-master background semantics with one exact opaque chroma carrier. It
does not modify or reinterpret the v2 payload, locks, records, or historical
outputs. Direct true-alpha handoffs are not compatible with this successor.

## 2. Canonical payload

The following JSON object is canonical and immutable.

```json
{
  "expressionProfileKey": "projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0",
  "baseProfileBinding": {
    "expressionProfileKey": "projectbs_character_open_ink_wash_dynamic_contour@2.0.0",
    "expressionProfilePayloadHash": "b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5",
    "mutationPolicy": "base_payload_hash_locks_and_historical_meaning_unchanged"
  },
  "singleImageApplicability": {
    "assetType": "character_single_image",
    "domainType": "character",
    "structureProfile": "character_single_image_v2",
    "outputCount": 1,
    "selection": "explicit_approved_planning_fact_and_complete_successor_projection_required"
  },
  "providerMasterContract": {
    "schemaVersion": "generated_media_opaque_chroma_provider_master_v1",
    "canvas": {"width": 1024, "height": 1536},
    "outputFormat": "png",
    "backgroundFullyOpaque": true,
    "generationBackground": {"mode": "removable_solid", "color": "#00FF00"},
    "fieldCoverage": "edge_to_edge_outside_intended_foreground",
    "fieldUniformity": "every_background_pixel_exact_rgb_0_255_0",
    "foregroundExactChromaRgb": "prohibited",
    "providerTransparency": "prohibited",
    "neighboringFragments": "prohibited",
    "forbiddenFieldFeatures": ["checkerboard", "gradient", "texture", "lighting_variation", "halo", "vignette", "floor", "scene", "shadow"]
  },
  "baseStyleProjectionContract": {
    "inheritance": "all_open_ink_v2_non_background_members_and_lock_meaning_required",
    "negativeSpaceCanvasPolicy": "opaque_chroma_carrier_is_not_artistic_paint_and_full_canvas_floor_is_measured_after_authorized_uncomposite",
    "providerConformanceBackgroundGate": "exact_opaque_chroma_master_contract",
    "backgroundStatementSubstitutions": [
      {"constraintId": "char_open_wash_v2_negative_halo_scene_shadow", "statement": "No provider transparency, checkerboard, gradient, texture, lighting variation, halo, vignette, floor, scene, shadow, neighboring fragments, or any background color except a perfectly uniform opaque edge-to-edge #00FF00 field outside the intended foreground."},
      {"constraintId": "char_open_wash_v2_positive_identity_on_ivory", "statement": "Preserve approved young-adult Korean and Joseon identity and equipment on a perfectly uniform opaque edge-to-edge #00FF00 removable field, with no exact #00FF00 inside the intended foreground and no halo, vignette, floor, scene, shadow, or neighboring fragments."}
    ],
    "allOtherBaseLocks": "verbatim_in_original_order"
  },
  "postprocessBoundaryContract": {
    "ownerRole": "generated_media_chroma_uncomposite",
    "timing": "after_provider_master_generation_in_a_distinct_authorized_stage",
    "inputBinding": "exact_provider_master_path_sha_dimensions_and_generation_receipt_required",
    "requiredOutcome": "final_true_alpha",
    "algorithmAuthority": "separate_closed_postprocess_contract_required_before_execution",
    "generationRoleMayUncomposite": false,
    "generationRoleMayClaimTrueAlpha": false,
    "providerRecall": "prohibited"
  },
  "authoringProjectionContract": {
    "requiredPlanningBindings": ["expressionProfileKey", "expressionProfilePayloadHash", "singleImageSpecification.canvas", "singleImageSpecification.generationBackground", "singleImageSpecification.finalBackgroundPolicy"],
    "providerProseOrder": "corrected_required_elements_then_base_negative_locks_with_exact_substitution_then_base_positive_locks_with_exact_substitution_then_successor_negative_locks_then_successor_positive_locks",
    "evidencePolicy": "base_style_chroma_master_and_stage_boundary_evidence_required",
    "conflictPolicy": "block_before_routing_or_prompt_publication"
  },
  "negativeProviderMasterLock": [
    {"constraintId": "char_open_wash_chroma_negative_transparency_or_checkerboard", "statement": "Do not ask the provider for transparency, alpha, checkerboard, cutout, or already-uncomposited pixels."},
    {"constraintId": "char_open_wash_chroma_negative_field_variation", "statement": "No gradient, texture, lighting variation, halo, vignette, floor, scene, shadow, neighboring fragments, or non-#00FF00 background pixels outside the intended foreground."},
    {"constraintId": "char_open_wash_chroma_negative_foreground_key_collision", "statement": "Do not place exact RGB #00FF00 in the character, equipment, weapon, pigment, or any intended foreground pixel."},
    {"constraintId": "char_open_wash_chroma_negative_base_style_substitution", "statement": "Do not weaken or replace any inherited open-ink v2 identity, proportion, contour, mok-seon, pigment, palette-role, surface-detail, reference-role, or output-triage meaning."},
    {"constraintId": "char_open_wash_chroma_negative_stage_collapse", "statement": "Do not perform or claim chroma uncomposite, true alpha, preservation, evaluation, or project eligibility in the provider-master generation stage."}
  ],
  "positiveProviderMasterLock": [
    {"constraintId": "char_open_wash_chroma_positive_exact_master", "statement": "Return exactly one 1024x1536 PNG provider master with a fully opaque edge-to-edge perfectly uniform RGB #00FF00 field outside the intended foreground."},
    {"constraintId": "char_open_wash_chroma_positive_base_style", "statement": "Preserve the complete hash-bound open-ink v2 character style, identity, equipment, and observable conformance requirements except for the explicitly replaced generation-background statements."},
    {"constraintId": "char_open_wash_chroma_positive_separable_foreground", "statement": "Keep every intended foreground pixel free of exact RGB #00FF00 and fully inside the canvas with no clipping or neighboring fragments."},
    {"constraintId": "char_open_wash_chroma_positive_stage_boundary", "statement": "Close the exact provider-master hash and conformance receipt, then hand off only to the distinct authorized chroma-uncomposite role; project-copy eligibility remains false."}
  ]
}
```

RFC 8785 JCS over this exact payload has SHA-256
`b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a`.
Array order and every value are normative; object member order is not.

## 3. Closed selection and stage boundary

Planning selects the exact successor key/hash and uses exactly
`canvas={width:1024,height:1536}` plus
`generationBackground={mode:"removable_solid",color:"#00FF00"}`. It omits
`transparentForegroundSelection` and every direct-alpha required element. The
required list instead closes one opaque provider master and the distinct later
postprocess requirement. Routing copies the successor payload unchanged; it
never patches open-ink v2 or converts an old direct-alpha handoff.

Authoring keeps every non-background base lock in its original order, applies
the two exact constraint-ID substitutions above, then appends the successor
locks in canonical order. The green carrier is not character pigment and is
excluded from artistic negative-space measurement; final full-canvas negative
space is measured only after an independently authorized uncomposite.

Generation may emit only a hash-bound opaque provider-master receipt. It does
not remove green, create alpha, preserve, evaluate, or claim project-copy
eligibility. `generated_media_chroma_uncomposite` is a separate later role and
cannot execute until a closed algorithm/receipt contract and exact source
authority are supplied; it may not recall the provider.

Closed failures are:

- `open_ink_chroma_successor_base_mismatch`
- `open_ink_chroma_master_contract_mismatch`
- `open_ink_chroma_direct_alpha_conflict`
- `open_ink_chroma_profile_projection_mismatch`
- `open_ink_chroma_provider_prose_mismatch`
- `open_ink_chroma_provider_master_nonopaque`
- `open_ink_chroma_provider_master_field_nonuniform`
- `open_ink_chroma_provider_master_foreground_key_collision`
- `open_ink_chroma_provider_master_forbidden_feature`
- `open_ink_chroma_stage_boundary_violation`

Every routing/authoring conflict is no-write before provider access. Every
generation failure terminates with the observed provider counters and master
hash, with no retry or downstream stage.
