# Generated Media Current Record Guide

## Purpose and Boundary

Guide Type: current v2/v3 schema authority. It owns canonical JSON, immutable
identity, paths, indexes and state handoffs for the ImageGen-only flow.
Legacy record schemas are owned by
GeneratedMediaLegacyV1CompatibilityGuide.md and do not appear as current
examples here.

## Canonical Rules

All current record, payload and index objects use RFC 8785 JSON Canonicalization
Scheme (JCS). JCS output is encoded as UTF-8 without a BOM. Object member order
is therefore the JCS lexicographic order, array order is preserved, JSON string
escaping and number rendering are the JCS forms, and non-finite numbers are
forbidden. Unknown fields and missing required fields reject before ID
calculation.

`canonicalJson(value)` means the exact JCS UTF-8 byte sequence with no trailing
newline. A JSON file's exact bytes are `canonicalJson(value)` followed by one
LF byte (`0A`). A payload hash is SHA-256 over `canonicalJson(payload)`. A file
hash is SHA-256 over the complete file bytes including the final LF. Hashes are
64 lowercase hexadecimal characters. Paths are project-relative, use `/`, and
never include another PC's root.

## Current Paths and Indexes

```text
routing:     AgentDocs/planning-data/generated-media-routing/v2/...
prompts:     AgentDocs/planning-data/generated-media-prompts/v2/...
generation:  AgentDocs/planning-data/generated-media-generation/v2/...
preservation:AgentDocs/planning-data/generated-media-preservation/v2/...
```

Animation adds `{contentId}/{animationRequestId}/` before the record filename.
Current routing indexes use the exact contract below. Other stage indexes must
state an equally closed schema before they are written. Current work never
writes a v1 index.

## Routing v2 Identity and Record

### Closed routing hash payload

Construct exactly one `routingHashPayload` by projecting the validated routing
unit into the following closed schema. Names shown with `?` are conditionally
present, not nullable. No other member is allowed.

```yaml
schemaVersion: generated_media_routing_hash_payload_v2
routerVersion: generated_media_router_v2
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId: exact selected rowId
profileKey: exact selected registry profileKey including version
requestId: exact planning handoff requestId
assetType: character_single_image | icon_single_image | background_single_image | animation
domainType: character | skill | item | stage | battle | environment
contentId: exact planning handoff contentId
animationRequestId?: required only for animation; forbidden otherwise
planningHandoffPath: exact project-relative v2 handoff path
planningSnapshotHash: exact verified planning snapshot SHA-256
sourcePlanningFiles: exact ordered planning-handoff array, including every path/role/sha256/revision member
requiredElements: exact ordered planning-handoff array
prohibitedElements: exact ordered planning-handoff array or signed no_prohibitions value
typeSpecification: exactly one complete type value selected without renaming:
  character_single_image: {identityConsistencyLock, singleImageSpecification}
  icon_single_image: {identityConsistencyLock, iconProfile, singleImageSpecification}
  background_single_image: {backgroundProfile, backgroundSpecification}
  animation: {animationRequest} where animationRequest is the one source object selected by animationRequestId
normalizedRequest: exact normalized single-unit request defined below
selectedPipeline: exact registry value
selectedAuthoringPrompt: exact project-relative registry path
selectedGenerationPrompt: exact project-relative registry path
provider: imagegen
structureProfile: exact registry value
routingReason: exact object defined below
authoringHandoff: exact field-level handoff defined below, before derived routing record references are bound
supersedesRoutingRecordId?: present only when the caller supplied and validation accepted an earlier generated_media_routing_v2 ID
```

`sourcePlanningFiles`, `requiredElements`, `prohibitedElements`, every nested
specification array, and animation key-pose/frame arrays preserve source order.
Object member order has no semantic input meaning because JCS fixes its byte
order. The payload includes every value that can change routing identity or the
field-level authoring input. Its `authoringHandoff` is the pre-binding handoff:
it must not yet contain `routingRecordId`, `routingRecordPath`,
`routingPayloadSha256`, or `indexPath`. Those derived references
are bound only in the record's `authoringHandoff` after ID/path calculation and
are removed again when re-projecting the payload. The payload also excludes
top-level `routingRecordId`, `routingPayloadSha256`, `createdAt`, and
`validation`, plus filesystem roots, file mtimes, router host/user data,
provider state, cost, and every downstream-stage result.

`normalizedRequest` is a closed object with
`requestId`, `assetType`, `domainType`, `contentId`, `contentUsage`,
`planningSnapshotHash`, `requiredElements`, `prohibitedElements`, and the same
exact `typeSpecification` object as the payload. It additionally contains the
same scalar `animationRequestId` only for animation. No source files, registry
selection, routing references, timestamps, or downstream fields occur in it.

`routingReason` is the closed object below, not free-form prose:

```yaml
code: exact_registry_row_match
registryRowId: exact selected rowId
profileKey: exact selected profileKey
matchedFields:
  assetType: exact selected row assetType
  domainType: exact selected row domainType
  profileKey: exact selected profileKey
```

The pre-binding `authoringHandoff` is a closed object containing exactly:

```yaml
planningHandoffPath:
requestId:
assetType:
domainType:
contentId:
animationRequestId?: same conditional-presence rule
planningSnapshotHash:
sourcePlanningFiles:
requiredElements:
prohibitedElements:
typeSpecification:
normalizedRequest:
registryVersion:
registryRowId:
profileKey:
selectedPipeline:
selectedAuthoringPrompt:
selectedGenerationPrompt:
provider: imagegen
structureProfile:
```

The record form binds exactly four additional members into that object:
`routingRecordId`, `routingRecordPath`, `routingPayloadSha256`, and `indexPath`.
All repeated values must be byte-semantically equal to the corresponding
payload/record values.

Calculate:

```text
routingPayloadSha256 = lowercase_hex(SHA256(canonicalJson(routingHashPayload)))
hashPrefix = first 20 hexadecimal characters of routingPayloadSha256

non-animation:
routingRecordId = gmroute2.{assetType}.{contentId}.{hashPrefix}

animation:
routingRecordId = gmroute2.animation.{contentId}.{animationRequestId}.{hashPrefix}
```

The 20-character prefix is exactly 80 bits. The full 64-character
`routingPayloadSha256`, not the prefix, is the authoritative payload identity
and must be stored and compared. `assetType`, `contentId`, and
`animationRequestId` must already be validated safe single path/ID segments;
the router does not encode or sanitize them after hash calculation.

The record path is exact:

```text
non-animation:
AgentDocs/planning-data/generated-media-routing/v2/{assetType}/{contentId}/{routingRecordId}.json

animation:
AgentDocs/planning-data/generated-media-routing/v2/animation/{contentId}/{animationRequestId}/{routingRecordId}.json
```

The closed `generated_media_routing_v2` record schema is the following member
set. `?` has the same conditional-presence meaning as above.

```yaml
schemaVersion: generated_media_routing_v2
routingRecordId:
routingPayloadSha256:
routerVersion: generated_media_router_v2
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId:
profileKey:
requestId:
assetType:
domainType:
contentId:
animationRequestId?:
planningHandoffPath:
planningSnapshotHash:
sourcePlanningFiles:
requiredElements:
prohibitedElements:
typeSpecification:
normalizedRequest:
selectedPipeline:
selectedAuthoringPrompt:
selectedGenerationPrompt:
provider: imagegen
structureProfile:
routingReason:
authoringHandoff:
supersedesRoutingRecordId?:
createdAt: exact RFC 3339 planningSnapshot.capturedAt copied from the validated handoff
validation:
  status: valid
  planningHandoff: valid
  sourceHashes: valid
  planningSnapshot: valid
  typeSpecification: valid
  registryMatchCount: 1
  recordIdentity: valid
```

Every payload member except `authoringHandoff` is copied byte-semantically into
the same-named record member. Bind the four derived routing references listed
above into the record's `authoringHandoff` without changing any pre-binding
member. Re-projecting removes the four excluded top-level members and those four
derived handoff references, then restores
`schemaVersion=generated_media_routing_hash_payload_v2`; the result must
recreate the exact `routingHashPayload`, payload hash, and ID. `createdAt` is a
stable copied planning fact, not router wall-clock time. The exact closed
`validation` object above contains no timestamp, write outcome, host data, or
new routing fact, so independently constructed records from the same input have
the same bytes.

### Closed routing index

There is one index in the same directory as its records:

```text
non-animation:
AgentDocs/planning-data/generated-media-routing/v2/{assetType}/{contentId}/routing_index.json

animation:
AgentDocs/planning-data/generated-media-routing/v2/animation/{contentId}/{animationRequestId}/routing_index.json
```

The exact closed schema is:

```yaml
schemaVersion: generated_media_routing_index_v2
assetType:
contentId:
animationRequestId?: required only when assetType=animation; forbidden otherwise
entries:
  "{routingRecordId}":
    routingRecordId: must equal the containing object key
    recordSchemaVersion: generated_media_routing_v2
    recordPath: exact canonical project-relative record path
    recordSha256: SHA-256 of exact record file bytes including final LF
    routingPayloadSha256: full 64-character payload hash
    requestId:
    assetType:
    domainType:
    contentId:
    animationRequestId?: same conditional-presence rule
    planningSnapshotHash:
    registryVersion: generated_media_authoring_profile_registry_v2
    registryRowId:
    profileKey:
    supersedesRoutingRecordId?: same presence and value as the record
```

The top-level `entries` value is an object, not an array. Its keys are exact
record IDs; each value is the projection shown above. Top-level scope fields,
entry identity fields, the referenced record, and the directory path must all
agree. JCS orders the entry keys and every nested object key. Index file bytes
are `canonicalJson(index) + LF`, and no timestamp, count, latest pointer,
status, tombstone, or downstream field is allowed.

### Idempotency, collision, supersession, and writes

The router first computes the full payload hash, ID, record bytes to be created,
record path, index path and expected index entry in memory, then validates all
existing state before writing:

1. Neither record nor index entry exists: write the record first, then publish
   the updated index.
2. The record and matching entry both exist: verify canonical file bytes,
   closed schemas, re-projected full payload hash, ID/path, record file hash and
   exact entry projection. When all match, return the existing record and index
   bytes unchanged. Do not recompute `createdAt` or `validation`. This is the
   only byte-identical idempotent reuse.
3. A valid matching record exists but the index or its entry is absent: preserve
   the record bytes and add only the exact derived entry. This is recoverable
   completion after a prior index-write failure.
4. An index exists without the addressed entry: it must first pass its closed
   schema and every existing-entry validation. A new record may then be written
   and the new entry added.
5. The addressed entry exists but its record does not: write nothing and return
   `routing_index_write_failed`; restoring or removing the dangling entry is a
   separate authorized remediation.
6. An ID/path is occupied by bytes whose re-projected full payload hash, record
   schema, canonical bytes, or expected record bytes differ: write nothing and
   return `routing_record_collision`. This includes an 80-bit prefix collision,
   same payload hash with divergent record bytes, and non-canonical alternate
   serialization.
7. An addressed index key has any value other than the exact projection of the
   valid record, or existing index bytes/schema are divergent: write nothing
   and return `routing_index_write_failed`. Never normalize or overwrite it as
   part of routing.

`supersedesRoutingRecordId` never mutates, replaces, hides, or deletes an older
record/index entry. It must identify an existing, valid
`generated_media_routing_v2` record in the same asset/content/animation scope,
must not equal the new ID, is included in the new payload identity, and is
copied to the new record and index entry. An invalid or cross-scope target
blocks before writes as `conflicting_routing_evidence`.

Use a same-directory temporary file, flush file data, and atomically publish
with no-clobber semantics for a new immutable record. After the record exists
and has been reread and hashed, construct the complete new index in memory,
write/flush a same-directory temporary file, then atomically replace the index
only if the on-disk prior index bytes still equal the bytes validated before the
write. Use a same-scope exclusive lock or equivalent compare-and-swap; a changed
preimage returns `routing_index_write_failed` and must never lose a concurrent
entry. Never publish the index before the record. Temporary files are not
workflow artifacts and must be removed after a failed publish when removal is
safe.

If record publication fails, leave the prior index untouched, publish no other
artifact, and return `routing_record_write_failed`. If record publication
succeeds but index publication fails, preserve the valid immutable record as a
recoverable orphan, leave the prior index bytes untouched, report both paths
and the record hash, return `routing_index_write_failed`, and set
`safeToRetry=true`. A retry follows rule 3 and must not rewrite the record.
Blocked validation and collision cases create no record, index, placeholder,
failure JSON, or downstream handoff.

## Prompt v3

Use the exact `generated_media_prompt_v3` schema in
GeneratedMediaImageGenOnlyContractGuide.md and embedded
`generated_media_visual_brief_v2` from
GeneratedMediaVisualPromptAuthoringGuide.md. Provider is exactly `imagegen` and
there is one scenePromptOriginal; no PixelLab branch is allowed.

```text
promptHashPayload includes immutable identity/snapshot, registry row/profile,
structureProfile, visualBrief hash, scene prompt hash and settings intent.
promptRecordId=gmprompt3.{assetType}.{contentId}.{optionalAnimationRequestId}.{hash[0:20]}
```

Ready status requires valid visual evidence, exact Markdown prompt-body equality
after LF normalization, and all type readiness gates.

## Generation v2

Use the exact `generated_media_generation_v2` schema in the current contract.
It stores prompt identity/hash, provider approval scope, settings, attempts,
costEvidence, result refs and preservation handoff only.

```text
generationHashPayload includes prompt ID/hash, immutable request/snapshot,
provider=imagegen, provider execution scopeHash, providerExecutionApprovalSha256,
and optional animation ID. The approval envelope hash binds maxAttempts,
maxCost and estimateUnavailablePolicy even though those limits do not alter the
execution scopeHash.
generationRecordId=gmgen2.{assetType}.{contentId}.{optionalAnimationRequestId}.{hash[0:20]}
```

The exact closed scope payload, approval, cost union, logical-attempt rules,
idempotency-key formula, actual-cost handling, generation-index entry, and
`approvalCostProjection` are defined only in sections 6.1-6.2 of
GeneratedMediaImageGenOnlyContractGuide.md. Recompute every hash; do not trust a
caller-provided scope or approval hash. Record/index JSON uses the canonical and
file-byte rules at the start of this guide.

Before an external call, look up the deterministic ID and active attempt:

- identical completed result -> reuse without billing;
- identical active attempt -> block `duplicate_provider_call_risk`;
- changed prompt/settings/asset identity -> new scope and fresh approval, never
  append as an equivalent retry;
- changed limits for the same scope -> new approval SHA/generation identity,
  while consumed attempt numbering remains cumulative for that scope;
- a submit-boundary crossing consumes the attempt even on provider failure or
  ambiguous outcome; no-call validation and completed reuse do not;
- attempts cannot exceed approved `maxAttempts`;
- every call or avoided call records closed `costEvidence`, including
  unavailable/no-charge, and its hash is projected byte-identically to the
  record, generation index entry, and preservation handoff.

## Preservation v2 and State Flow

Preservation schema/path/identity is owned by
GeneratedMediaPreservationPackagingGuide.md.

```text
planning_handoff_v2
-> routing_v2
-> prompt_v3
-> generation_v2
-> preservation_v2
-> evaluation_package_v2
-> separate evaluation/promotion
```

Only a validated prior state advances. Failed/blocked stages do not fabricate a
later record. Records are immutable after completion; changed planning,
prompt, profile, settings or media bytes create a new identity.

## Failure and Validation

```text
unknown_record_field
missing_record_field
record_identity_mismatch
record_hash_mismatch
record_collision
index_entry_invalid
prompt_markdown_mismatch
provider_value_invalid
unsupported_record_schema
missing_provider_execution_approval
invalid_provider_execution_approval
provider_execution_scope_mismatch
provider_cost_unit_mismatch
provider_cost_estimate_unavailable
provider_cost_limit_exceeded
provider_actual_cost_unavailable
retry_limit_exceeded
duplicate_provider_call_risk
```

Validate schema/path/version parity, exact one-animation ID rules, provider
ImageGen, four-role asset/profile/anchor mapping, prompt/settings/approval
hashes, attempt/cost provenance and stage separation. Icon and background
records remain distinct through registryRowId, structureProfile, adapter and
evaluation identity even when their original media type is the same.

## Related Guides

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
