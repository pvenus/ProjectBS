# Generated Media Identity-Anchored Opaque-Chroma Execution Guide

## 1. Scope and registration

This guide owns one additive, source-independent regeneration branch for
`character.seojin.2` MAIN. It does not edit, continue, or otherwise reuse the
rejected Grade 2 source/postprocess lineage. It keeps the registered expression
profile `projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0` /
`b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a`
byte- and meaning-unchanged.

The registered execution profile is
`projectbs_character_open_ink_opaque_chroma_identity_anchored_regeneration@1.0.0`.
Its canonical payload is the complete JSON object in
`helpers/generated_media_identity_anchored_opaque_chroma_execution_profile_v1.json`.
RFC 8785 JCS over that object has SHA-256
`44d3bafcc720d39ac260fb2089798c16f9ec1f50d391165eea676dbc79cdc3ad`.
Array order and all values are normative. The profile applies only to the exact
asset/domain/content/structure/expression-profile tuple in that payload.

## 2. Closed planning and downstream selection

A fresh planning handoff selects this branch with exactly one top-level
`identityAnchoredGenerationSelection` object having exactly these seven members:

```yaml
schemaVersion: generated_media_identity_anchored_generation_selection_v1
executionProfileKey: projectbs_character_open_ink_opaque_chroma_identity_anchored_regeneration@1.0.0
executionProfilePayloadSha256: 44d3bafcc720d39ac260fb2089798c16f9ec1f50d391165eea676dbc79cdc3ad
role: identity_equipment_authority
authorityContentId: character.seojin.1
projectRelativePath: Assets/ImagesGenerated/Character/portrait/character.seojin.1.portrait.png
sha256: ba2f769ba7d45909d618f7fd672a9bdad61015b9553d3c0d360bc49a13bb97cf
```

The path is canonical project-relative authority. An execution role resolves it
inside the exact checked-out project root and re-hashes the raw bytes before
submit. Absolute/transient paths, a different SHA, more than one binding, a
style-only/edit-source role, or a provider receipt requirement are invalid.
The reference grants only the five `allowedTransfer` identity/equipment anchors
in the registered payload. It grants no edit-source/edit-target, pose/framing,
background, pixel-copy, or provider-receipt semantics.

The selection is copied byte-semantically and hash-significantly through these
top-level locations and nowhere else:

1. `generated_media_planning_handoff_v2.identityAnchoredGenerationSelection`;
2. routing hash payload, routing record, `normalizedRequest`, and
   `authoringHandoff` at `.identityAnchoredGenerationSelection`;
3. visual brief, `generated_media_prompt_v3`, prompt hash payload, prompt index
   entry, and detached generation handoff at the same member name;
4. the generation preflight and execution scope described below.

The member is forbidden inside `typeSpecification`,
`identityConsistencyLock`, `singleImageSpecification`, `referenceBindings`, or
provider prose. It is omitted, not null or empty, for every existing branch.
Unknown/missing/nested/unequal projections fail
`identity_anchored_generation_projection_mismatch` before a record or provider
call. Existing prompt/routing identities without the member remain valid and
unchanged.

Authoring derives the provider prose from the authoritative Grade 2 planning
facts plus the registered execution-profile gates. It does not copy the path,
hash, or the authority portrait's pose/background into prose. The prompt must
state the required same-face/hair/body/handedness/equipment/Joseon evolution,
the prohibited foreign/ronin/samurai/katana/long-hair/tattered/wrapped/aged/
literal-historical substitutions, full-figure safe fit, and the unchanged flat
opaque `#00FF00` provider-master contract. Any omitted or contradictory item is
`identity_anchored_authoring_gate_mismatch`.

## 3. Closed generation mode

`builtin_imagegen_authenticated_identity_anchored_single_submit_v1` is a new
execution mode on the actual built-in
`image_gen.imagegen(prompt, referenced_image_paths?,
num_last_images_to_include?)` surface. Its call projection contains exactly:

```yaml
prompt: exact non-empty stored provider prose
referenced_image_paths:
  - exact resolved local path for the canonical project-relative binding
```

`num_last_images_to_include` is forbidden. The array length is exactly one and
its raw file SHA must match the selection. The reference is not an edit target
and needs no provider generation/edit receipt.

The closed preflight is `generated_media_builtin_imagegen_identity_anchored_preflight_v1`.
It contains exactly the existing built-in preflight members plus
`identityAnchoredGenerationSelection`,
`identityAnchoredGenerationSelectionSha256`, and the registered
`executionProfileKey`/`executionProfilePayloadSha256`. `callProjectionSha256`
is SHA-256 of JCS over `{promptSha256,referenceMode,referencedImagePaths}` where
`referenceMode` is exactly `identity_equipment_authority` and the path array is
the one canonical project-relative path above; the actual call resolves that
path without changing the hash projection.

The execution-scope payload schema is
`generated_media_builtin_imagegen_identity_anchored_execution_scope_v1`. It
contains exactly the existing built-in scope fields, changes `executionMode` to
the new mode, and adds the four selection/profile fields named above. Maxima are
`submitCountMaximum=1` and `retryCountMaximum=0`.
`executionScopeHash=SHA-256(JCS(scopePayload))` and
`idempotencyKey=gmidentity1.{executionScopeHash[0:20]}`. Authenticated standing
authority uses the existing approval shape with the new execution mode and exact
scope hash. Active, completed, or ambiguous same-key state blocks a new submit.

The callable surface exposes no capability/settings/cost descriptor and does
not provider-enforce canvas, background, or format controls. The receipt records
those facts as `unavailable_not_exposed` and `prompt_bound_not_callable`; it
never fabricates evidence or a zero cost. The prompt and post-return hard gates
still require exactly one fully opaque 1024x1536 PNG, full figure safely in
bounds, a perfectly uniform edge-to-edge RGB `#00FF00` field outside foreground,
clean boundary, no prohibited background feature, and all identity/equipment
gates.

The terminal receipt schema is
`generated_media_builtin_imagegen_identity_anchored_generation_receipt_v1`.
It is the existing built-in receipt shape with the new execution mode, selection
hash, execution-profile key/hash, and an `identityEquipmentConformance` object
containing the ordered required/prohibited gate results. Only all-pass plus the
opaque-chroma master gates may use `provider_master_complete` and
`nextStep=generated_media_chroma_uncomposite`. Any returned mismatch consumes
the one submit and ends `output_nonconformant_no_retry`; no provider recall,
retry, generation-stage uncomposite, preservation, evaluation, or promotion is
allowed.

## 4. Closed failures and stage boundary

- `identity_anchored_generation_profile_mismatch`
- `identity_anchored_generation_projection_mismatch`
- `identity_anchored_reference_missing`
- `identity_anchored_reference_hash_mismatch`
- `identity_anchored_reference_role_invalid`
- `identity_anchored_reference_count_invalid`
- `identity_anchored_authoring_gate_mismatch`
- `identity_anchored_generation_identity_equipment_gate_failed`
- existing built-in callable/approval/scope/duplicate/submit tokens
- existing opaque-chroma provider-master conformance tokens

Every pre-submit failure records `providerCalled=false`, `submitCount=0`, and
`retryCount=0`. Every crossed provider boundary consumes the sole submit. A
conforming provider master authorizes only the distinct registered
`generated_media_chroma_uncomposite` stage; project-copy eligibility remains
false until later preservation and independent evaluation PASS.
