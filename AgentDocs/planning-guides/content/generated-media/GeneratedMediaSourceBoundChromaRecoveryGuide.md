# Generated Media Source-Bound Chroma Recovery Guide

## 1. Authority and compatibility boundary

This guide owns the distinct postprocess mode
`source_bound_green_carrier_uncomposite_v1`. Its registered profile is the
exact JCS object at:

```text
AgentDocs/planning-guides/content/generated-media/helpers/generated_media_source_bound_chroma_recovery_profile_v1.json
```

The profile key is
`projectbs_character_open_ink_source_bound_green_carrier_uncomposite@1.0.0`
and its RFC 8785 JCS SHA-256 is
`b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746`.
The only executable implementation is
`helpers/generated_media_source_bound_chroma_uncomposite_v1.mjs`, algorithm
`generated_media_border_calibrated_green_uncomposite_v1@1.0.0`.

This is not a new generation expression profile and does not modify, relax, or
reinterpret
`projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0` /
`b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a`.
The two generation receipts remain immutable consumed
`output_nonconformant_no_retry` evidence. Exact `#00FF00` is neither asserted
nor retroactively satisfied. No other source, receipt, content ID, profile, or
future near-green image inherits this authority.

## 2. Closed source registry

Exactly these independent bindings are registered:

| contentId | source SHA-256 | generation receipt SHA-256 | floor | edge carrier | enclosed carrier | combined one-ring |
| --- | --- | --- | ---: | ---: | ---: | ---: |
| `character.seojin.2` | `1222a43bf5cc41b3e1d6d261ae8be484746fdd130f85db21a382aec907c3abf2` | `d7e9cd9894d2989fd58caba75f0548963eebc510dd3085f9cbda03d0a0f1a74b` | 214 | 1,145,787 | 60 components / 2,986 px | 7,276 px |
| `character.seojin.3` | `2e3333def860d13c0d1e3c955a32fa5e0e9875f55c6da101a3e39dd51f422973` | `3a70164373fb8b45debe5767ac316080f6c3fadddc4388bfc1ec79a3d323cb1d` | 218 | 1,055,854 | 51 components / 2,743 px | 10,471 px |

The profile additionally binds byte length, request, handoff, consumed
idempotency key, dimensions/mode, full-perimeter statistics, row-major mask
digests, enclosed-component-size digest, one-ring statistics, foreground bbox,
and safe margins. A path is execution evidence only; the source and receipt raw
hashes are identity.

## 3. Deterministic preflight and transform

For each source independently, the postprocess owner performs this exact order:

1. Rehash the profile raw bytes/JCS payload, source PNG, and generation receipt.
   Validate the exact registered tuple, receipt request/handoff/idempotency,
   `submitCount=1`, `retryCount=0`, and immutable
   `output_nonconformant_no_retry` state. Never rewrite that receipt.
2. Decode a non-interlaced RGB8 PNG. Require 1024x1536, fully opaque source
   semantics, no crop/resize/recenter, and exact registered byte length.
3. For every pixel compute `greenExcess = G - max(R,B)`. Use the entire outer
   perimeter, not a fixed-width border. Every perimeter pixel must be positive,
   its exact min/median/max must match the binding, and its minimum is the
   source-specific floor.
4. Candidate carrier pixels are exactly `greenExcess >= floor`. Four-connect
   from every perimeter pixel and require one complete registered edge-carrier
   mask/count/hash. Discover all remaining four-connected candidate components
   in row-major seed order. Include them only when count, total, ordered-size
   digest, and complete enclosed mask digest equal the registered fixture.
5. The partial-alpha shell is exactly one four-neighbor ring outside the
   combined carrier core with `greenExcess > 0`. Its count, digest and exact
   min/median/p95/max must match. No dilation, tolerance, manual seed, inferred
   threshold, arbitrary disconnected cleanup, or second ring is allowed.
6. Carrier core becomes alpha 0 with neutral transparent RGB `(0,0,0)`. The
   one-ring alpha is round-half-up
   `255 * (floor - greenExcess) / floor`, clamped to 1..254. Select the adjacent
   core background sample with greatest greenExcess, breaking ties by smallest
   row-major index, then integer-uncomposite every channel. Apply only the
   bounded one-ring despill that prevents visible positive green excess and any
   newly introduced cyan or magenta excess. Every other source RGB byte remains
   identical with alpha 255.
7. Serialize only with `generated_media_png_rgba8_store_v1@1.0.0`. Reopen and
   require the exact final gates below. The source is never written.

No manual mask, recolor, repaint, identity/style/detail edit, crop, resize,
recenter, erosion, arbitrary component deletion, provider recall, second
provider submit, or raw-source mutation is permitted.

## 4. Output and validation gates

Before `recovered`, require all of:

- PNG RGBA8, unchanged 1024x1536 canvas, `alphaMin=0`, `alphaMax=255`, and at
  least one transparent pixel;
- four corners and the complete outer perimeter alpha 0;
- exact registered foreground bbox/margins, foreground retained, no clipping;
- every non-carrier/non-one-ring RGB byte unchanged and alpha 255;
- only registered core and one-ring pixels transformed;
- raw color-model recomposition error at most 1 per channel;
- zero visible one-ring positive-green pixels, zero newly introduced cyan
  excess, and zero newly introduced magenta excess;
- no loss of protected ink, navy, gray-brown, oxidized red, brass, pale wash,
  or any other non-green source pixel.

Any failed output gate is terminal. It does not permit threshold widening,
manual repair, recolor, another transform mode, or provider retry. Visual style
or G3 lower-body/feet findings belong only to later independent evaluation.

## 5. Closed record, receipt, paths, and idempotency

The execution receipt is
`generated_media_source_bound_chroma_uncomposite_receipt_v1`, exactly as emitted
by the helper. It contains exactly `schemaVersion`, `state`, `contentId`,
`requestId`, `sourceSha256`, `generationReceiptSha256`, `profileKey`,
`profilePayloadSha256`, `algorithmSettings`, `algorithmSettingsSha256`,
`sourceEvidence`, `sourceEvidenceSha256`, `outputSha256`, `outputByteLength`,
`width`, `height`, `colorMode`, `outputValidation`, `providerCalled`,
`submitCount`, `retryCount`, `evaluationStatus`, `projectCopyEligible`,
`nextStep`, and `receiptPayloadSha256`. Unknown or missing members reject.

The immutable record is:

```yaml
schemaVersion: generated_media_source_bound_chroma_uncomposite_record_v1
recordId: gmchroma1.character_single_image.{contentId}.{recordPayloadSha256[0:20]}
recordPayloadSha256: SHA-256(JCS(record excluding recordId and recordPayloadSha256))
authorityMainSha:
assetType: character_single_image
domainType: character
contentId:
requestId:
generationProfileKey: projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0
generationProfilePayloadHash: b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a
sourceSha256:
generationReceiptSha256:
recoveryProfileKey: projectbs_character_open_ink_source_bound_green_carrier_uncomposite@1.0.0
recoveryProfilePayloadHash: b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746
algorithmSettingsSha256:
sourceEvidenceSha256:
receiptPayloadSha256:
outputPath: exact project-relative postprocess output path
outputSha256:
outputByteLength:
width: 1024
height: 1536
colorMode: RGBA
providerCalled: false
submitCount: 0
retryCount: 0
evaluationStatus: not_evaluated
projectCopyEligible: false
nextStep: preservation_then_independent_evaluation
```

Canonical paths are:

```text
output/generated-media-postprocess/v1/character_single_image/{contentId}/{sourceSha256}/true-alpha.png
AgentDocs/planning-data/generated-media-postprocess/v1/character_single_image/{contentId}/{recordId}.json
AgentDocs/planning-data/generated-media-postprocess/v1/character_single_image/{contentId}/postprocess_index.json
```

The deterministic idempotency payload contains exactly recovery profile
key/hash, source hash, generation receipt hash, algorithm settings hash, and
content/request identity. A completed identical record/output is
`reused_identical`. Different bytes at an occupied output/record/ID are a hard
collision. Write and fsync the no-clobber output, write the immutable record,
then sorted-CAS append the closed index entry. A record is authoritative only
after output hash, record hash, and index entry all agree; partial files are
not reusable authority and require explicit transaction recovery, never
overwrite.

The closed index entry contains exactly `recordId`, `recordPath`,
`recordSha256`, `recordPayloadSha256`, `contentId`, `requestId`, `sourceSha256`,
`generationReceiptSha256`, `recoveryProfileKey`,
`recoveryProfilePayloadHash`, `outputPath`, `outputSha256`,
`receiptPayloadSha256`, `evaluationStatus`, and `projectCopyEligible`.

## 6. Downstream boundary and failures

Only a complete indexed record with byte-identical output and receipt may enter
preservation. Preservation seals the original source, immutable generation
receipt, recovery profile, source evidence, recovery receipt, and true-alpha
output. Evaluation runs independently on the sealed package. Recovery itself
does not preserve, evaluate, score, promote, copy, or make project-copy
eligible.

Closed failures are:

```text
source_chroma_profile_hash_mismatch
source_chroma_binding_not_registered
source_chroma_source_fixture_mismatch
source_chroma_generation_receipt_mismatch
source_chroma_outer_perimeter_not_green_dominant
source_chroma_outer_perimeter_carrier_disconnected
source_chroma_calibration_evidence_mismatch
source_chroma_one_ring_model_invalid
source_chroma_output_validation_failed
source_chroma_output_collision
source_chroma_record_collision
source_chroma_index_cas_mismatch
source_chroma_stage_boundary_violation
```

Every failure returns `providerCalled=false`, `submitCount=0`, `retryCount=0`,
does not change the immutable generation receipt/source, and authorizes no
fallback or threshold adjustment.
