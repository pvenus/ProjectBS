# Generated Media Built-in ImageGen Authenticated Generation Guide

## 1. Resolution and scope

This guide owns option B: one truthful official generation mode for the actual
built-in callable surface. Live tool metadata exposes only
`image_gen.imagegen(prompt, referenced_image_paths?,
num_last_images_to_include?)`. No enabled non-submit interface supplies a
capability descriptor, resolved settings, tagged cost estimate, immutable
provider evidence reference, or provider-created approval envelope. Therefore
`configured_imagegen_capability` remains the only authority for
`promotable_generation_v2`; it is not fabricated or aliased here.

`builtin_imagegen_authenticated_single_submit_v1` is additive and is eligible
only for an authoritative `character_single_image_v2` handoff and prompt whose
selected profile is
`projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0` / payload hash
`b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a`.
Existing planning, routing, prompt, profile, preview, and strict-generation
records remain byte-immutable. This mode does not make an existing preview or
direct-alpha chain reusable.

## 2. Actual callable schema

The following payload is canonical and closed:

```json
{
  "schemaVersion": "generated_media_builtin_imagegen_callable_schema_v1",
  "provider": "imagegen",
  "providerTool": "image_gen.imagegen",
  "exposedMembers": [
    {"name": "prompt", "required": true, "type": "string"},
    {"name": "referenced_image_paths", "required": false, "type": "array_of_paths"},
    {"name": "num_last_images_to_include", "required": false, "type": "integer"}
  ],
  "referenceSelectorPolicy": "referenced_image_paths_and_num_last_images_to_include_mutually_exclusive"
}
```

Its RFC 8785 JCS SHA-256 is
`708b75b05f820870ac165eadcf08d093568944a35d2793e0a7d117bf23646af1`.
Any added, removed, renamed, retyped, or differently required member is
`builtin_imagegen_callable_schema_drift` and blocks before submit.

The closed `callProjection` contains exactly `promptSha256`, `referenceMode`,
and one conditional selector. `referenceMode=none` forbids both selectors;
`referenceMode=referenced_image_paths` requires one non-empty ordered array of
exact authority-approved paths and forbids `numLastImagesToInclude`;
`referenceMode=num_last_images_to_include` requires integer 1 through 5 and
forbids paths. An identity/style/edit reference is callable only when the
authoritative generation handoff explicitly approves that exact role and exact
path/hash. Generic or inferred reference use is
`builtin_imagegen_reference_projection_mismatch`.

## 3. Deterministic preflight and approval

The execution role derives one in-memory
`generated_media_builtin_imagegen_preflight_v1` with exactly these members:

```yaml
schemaVersion: generated_media_builtin_imagegen_preflight_v1
executionMode: builtin_imagegen_authenticated_single_submit_v1
authorityMainSha: exact fetched 40-lowercase-hex commit
requestId:
promptRecordId:
promptRecordSha256:
promptMarkdownSha256:
generationHandoffSha256:
providerPromptPayloadHash:
expressionProfileKey: projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0
expressionProfilePayloadHash: b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a
callableSchema: exact canonical payload above
callableSchemaSha256: 708b75b05f820870ac165eadcf08d093568944a35d2793e0a7d117bf23646af1
callProjection: exact closed projection
callProjectionSha256: SHA-256(JCS(callProjection))
providerSettingsIntent: exact verified prompt-record value
providerSettingsIntentSha256: exact recomputed hash
controlCoverage:
  canvas: prompt_bound_not_callable
  generationBackground: prompt_bound_not_callable
  outputFormat: prompt_bound_not_callable
capabilityDescriptorStatus: unavailable_not_exposed
settingsDescriptorStatus: unavailable_not_exposed
costEstimate: {status: unavailable_not_exposed}
```

The three provider settings remain mandatory semantic intent and post-output
hard gates. They are not described as provider-enforced controls, resolved
defaults, or capability evidence. Their absence from the callable schema is
non-blocking only in this exact mode; the returned master must still be exactly
one opaque 1024x1536 PNG with a perfectly uniform edge-to-edge `#00FF00` field
outside foreground. A mismatch consumes the sole submit and terminates without
retry or downstream handoff.

The execution scope hash payload contains exactly `schemaVersion`,
`executionMode`, `authorityMainSha`, `requestId`, `promptRecordId`,
`promptRecordSha256`, `promptMarkdownSha256`, `generationHandoffSha256`,
`providerPromptPayloadHash`, `expressionProfileKey`,
`expressionProfilePayloadHash`, `callableSchemaSha256`, `callProjectionSha256`,
`providerSettingsIntentSha256`, `submitCountMaximum`, and
`retryCountMaximum`. Its schemaVersion is
`generated_media_builtin_imagegen_execution_scope_v1`; maxima are exactly 1
and 0. `executionScopeHash` is SHA-256 of its JCS bytes.

Authenticated standing authority is projected into the closed approval:

```yaml
schemaVersion: generated_media_builtin_imagegen_authenticated_approval_v1
executionMode: builtin_imagegen_authenticated_single_submit_v1
approvedBy: authenticated current user identity
approvedAt: RFC 3339 timestamp with explicit offset
approvalEvidence: immutable exact request message/thread reference
executionScopeHash: exact recomputed hash
submitCountMaximum: 1
retryCountMaximum: 0
unavailableEvidenceAcceptance: capability_settings_and_cost_not_exposed
```

The execution role, not the caller, recomputes the scope. Missing/extra members,
wrong maxima, non-current approval evidence, or scope drift is
`builtin_imagegen_authenticated_approval_invalid`. No cost amount, zero-cost
claim, provider setting, descriptor version, or evidence reference is inferred.

```text
idempotencyKey = gmbuiltin1.{executionScopeHash[0:20]}
```

An active or completed identical key blocks a second submit as
`duplicate_provider_call_risk`; a completed result is reused byte-identically.
There is no retry path in this mode.

## 4. Submit and terminal receipt

Immediately before submit, revalidate only the preflight hashes, approval hash,
idempotency state, and submit/retry counters. The actual call contains exactly
the non-empty prompt plus the one selected reference projection. The call may
not add desired settings as invented tool members. Crossing the tool boundary
sets `providerCalled=true`, `submitCount=1`, `retryCount=0`, and
`costKnown=false` regardless of result.

The terminal receipt is closed:

```yaml
schemaVersion: generated_media_builtin_imagegen_generation_receipt_v1
state: provider_master_complete | completed_reuse | blocked | submit_failed_no_retry | output_nonconformant_no_retry
executionMode: builtin_imagegen_authenticated_single_submit_v1
authorityMainSha:
requestId:
promptRecordId:
promptRecordSha256:
generationHandoffSha256:
executionScopeHash:
approvalSha256:
idempotencyKey:
callableSchemaSha256:
callProjectionSha256:
providerCalled: true | false
submitCount: 0 | 1
retryCount: 0
costKnown: false
costEvidenceStatus: unavailable_not_exposed
capabilityDescriptorStatus: unavailable_not_exposed
settingsDescriptorStatus: unavailable_not_exposed
providerOutputRef?: required after a returned output
providerOutputSha256?: required after a returned output
outputConformance?: required after a returned output; exact opaque-chroma gate results
postprocessOwnerRole: generated_media_chroma_uncomposite
projectCopyEligible: false
failureType?: required for blocked/failed/nonconformant states
nextStep: generated_media_chroma_uncomposite | reuse_completed | stop_no_retry
```

`provider_master_complete` requires every opaque-chroma gate to pass and uses
`nextStep=generated_media_chroma_uncomposite`. The later role independently
rehashes the master and requires its own closed algorithm authority. Generation
does not uncomposite, create alpha, preserve, evaluate, promote, or recall the
provider. Nonconformance uses the existing `open_ink_chroma_provider_master_*`
tokens and `stop_no_retry`.

## 5. Failure boundary

Pre-submit failures are `builtin_imagegen_callable_schema_drift`,
`builtin_imagegen_preflight_projection_mismatch`,
`builtin_imagegen_reference_projection_mismatch`,
`builtin_imagegen_authenticated_approval_invalid`,
`provider_execution_scope_mismatch`, or `duplicate_provider_call_risk` and
record `providerCalled=false`, `submitCount=0`, `retryCount=0`,
`costKnown=false`. Post-submit operation failure is
`builtin_imagegen_submit_failed_no_retry`; output mismatch uses the existing
opaque-chroma failure. Neither authorizes a new submit.

An exact `output_nonconformant_no_retry` master remains terminal for generation.
It may be consumed later only when its source SHA and this immutable generation
receipt SHA are both explicitly registered by
`projectbs_character_open_ink_source_bound_green_carrier_uncomposite@1.0.0` /
`b336aa015146b793cd6cb2a1adf4dde0c6fdcc178dfa51c516f77994d3ff4746`.
That distinct postprocess exception does not change this receipt state, satisfy
the exact `#00FF00` generation gate, reopen idempotency, or authorize provider
recall/retry. Every unregistered nonconformant master remains stopped.
