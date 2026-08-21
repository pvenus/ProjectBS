# Generated Media Source-Bound MAIN Completion Guide

## Scope

This guide registers two disjoint, exact-source exceptions for immutable
nonconformant character MAIN provider masters. Neither exception changes the
meaning of the generating profile, generation receipt, prompt, route, or
idempotency scope. An unlisted source or receipt is ineligible.

## G2: deterministic uncomposite and fit

The registered profile is
`projectbs_character_open_ink_source_bound_green_carrier_fit@1.0.0` with JCS
payload SHA-256
`ca3102e7369da6513b4c4e462e68b373da618a2b1630a393d803d37136d812df`.
Its canonical payload is
`helpers/generated_media_source_bound_chroma_fit_profile_v1.json`; execution
uses only `helpers/generated_media_source_bound_chroma_fit_v1.mjs`.

Eligibility is the exact G2 source SHA
`66dc1c94be2e38e9dc4d6ff15b4b6b0699353b9830d718ec16743dc4ff92acf9`
plus generation-receipt SHA
`4e4457df58f31eff61adb1a93d8915e8a2fb0926e28cb9eb8358f2ce8b606526`
and the closed request/handoff/idempotency/evidence tuple in the profile.
Validation derives greenExcess evidence from the entire outer perimeter. It
first applies the unchanged v1 border-calibrated recovery, then crops only the
registered alpha foreground bbox in memory, performs the exact integer-area
premultiplied RGBA reduction `618x1383 -> 558x1249`, and places it at `(233,128)`
on a new transparent 1024x1536 canvas. This yields the exact allowed alpha bbox
`[233,128,790,1376]`. The operation is an aspect-preserving fit, not a semantic
edit: no manual mask, repaint, recolor, crop of foreground, enlargement,
independent-axis scale, host-library resampler, or identity change is allowed.

The helper owns the closed transform math, transparent RGB zero policy,
canonical PNG serializer, source-before/after hash check, exact output RGBA
hash, transform settings hash, alpha bbox, full-border alpha zero, no-clipping,
and no-new-fragment evidence. Any source/receipt/evidence/settings/geometry/hash
drift fails before write with one of
`source_chroma_fit_binding_not_registered`,
`source_chroma_fit_generation_receipt_mismatch`,
`source_chroma_fit_calibration_evidence_mismatch`,
`source_chroma_fit_geometry_invalid`, `source_chroma_fit_canvas_overflow`, or
`source_chroma_fit_output_bbox_mismatch`.

The immutable record is
`generated_media_source_bound_chroma_fit_record_v1`; its terminal receipt is
`generated_media_source_bound_chroma_fit_receipt_v1`. Record identity hashes
the exact source/receipt/profile/settings/source-evidence/recovered-RGBA/output
tuple. Write record first, then sorted CAS append; occupied-different is a hard
no-clobber failure and exact completed bytes alone are `reused_identical`.
Provider counters are always zero. A valid result authorizes only preservation
and independent evaluation; it is never directly project-copy eligible.

```text
recordId=gmchromafit1.character_single_image.character.seojin.2.{recordPayloadSha256[0:20]}
recordPath=AgentDocs/planning-data/generated-media-postprocess/v1/character_single_image/character.seojin.2/{recordId}.json
indexPath=AgentDocs/planning-data/generated-media-postprocess/v1/character_single_image/character.seojin.2/postprocess_index.json
```

## G3: one authenticated source-bound edit

The registered profile is
`projectbs_character_open_ink_source_bound_single_edit@1.0.0` with JCS payload
SHA-256
`aa65434f5fb9c22cb42db199c936ee414648b933f4b83c159065341f4e704011`.
Its canonical payload is
`helpers/generated_media_source_bound_character_edit_profile_v1.json`.

The route schema is `generated_media_source_bound_character_edit_route_v1`,
ID prefix `gmeditroute1`, with a
`generated_media_source_bound_character_edit_route_index_v1` sorted CAS index.
The closed route has exactly: schemaVersion, routeId, authorityMainSha,
requestId, contentId, sourcePathEvidence, sourceSha256,
generationReceiptPathEvidence, generationReceiptSha256,
generationHandoffSha256, profileKey, profilePayloadSha256,
callableSchemaSha256, providerPromptLines, approvalEvidence,
executionScopeHash, idempotencyKey, submitCountMaximum, retryCountMaximum,
outputContract, state, createdAt. Unknown or missing members fail closed.

```text
routePayloadSha256=SHA256(JCS(route without routeId, state, createdAt))
routeId=gmeditroute1.character_single_image.character.seojin.3.{routePayloadSha256[0:20]}
routePath=AgentDocs/planning-data/generated-media-source-edits/v1/character_single_image/character.seojin.3/{routeId}.json
indexPath=AgentDocs/planning-data/generated-media-source-edits/v1/character_single_image/character.seojin.3/source_edit_route_index.json
```

Eligibility is exact source SHA
`d435d0a6e5a7de4e7c50cd4e2552145eaa1eb8310d8874b37ed1e1a5a4c82c3d`
plus exact receipt SHA
`64457ef0c95045452745f167dbd42e024bf6c3d97cabb25dc65286c6b5cd6db5`
and the profile tuple. The callable projection is exactly non-empty `prompt`
plus `referenced_image_paths=[sourcePathEvidence]`; no other reference and no
invented settings/cost/capability members are permitted. The six prompt lines
are ordered and byte-significant. The only semantic removal is the smaller
screen-left brass closure near the shoulder; the screen-right upper closure at
the oxidized-red sash terminus remains, so the result has exactly one closure.
The complete figure may only be uniformly reduced/recentered into the registered
bbox/occupancy. Every listed identity, equipment, orientation, mantle, sash,
wind, no-mirror, and style lock remains mandatory. Output is one opaque RGB
1024x1536 PNG with exact uniform edge-to-edge `#00FF00` outside foreground.

Authenticated bounded authority permits one future provider submit and zero
retries. `gmedit1.{executionScopeHash[0:20]}` is the active/completed
idempotency key; either state blocks another submit. Returned nonconformance
consumes the submit and stops. Generation never uncomposites, preserves,
evaluates, promotes, or recalls the provider. A conformant master must first be
registered by a distinct chroma-uncomposite authority before any alpha work.

Hard failures include `source_bound_edit_binding_not_registered`,
`source_bound_edit_route_projection_mismatch`,
`source_bound_edit_reference_mismatch`, `source_bound_edit_prompt_mismatch`,
`source_bound_edit_approval_invalid`, `duplicate_provider_call_risk`,
`source_bound_edit_submit_limit_exceeded`,
`source_bound_edit_retry_forbidden`, `source_bound_edit_identity_drift`,
`source_bound_edit_closure_count_mismatch`,
`source_bound_edit_geometry_mismatch`, and
`source_bound_edit_carrier_nonconformant`.

## Compatibility boundary

The existing exact-`#00FF00` generation profile and the existing source-bound
uncomposite v1 profile/hashes remain unchanged. G2 does not authorize a provider
call. G3 does not authorize local retouching or alpha recovery. Neither branch
rewrites any existing artifact or grants preservation, evaluation, promotion,
project copy, or Unity authority ahead of its stated next-stage gate.
