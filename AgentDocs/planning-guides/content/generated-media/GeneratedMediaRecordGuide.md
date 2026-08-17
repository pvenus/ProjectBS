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
never include another PC's root, except exact observed evidence-location `path`
members in `generated_media_accepted_result_capture_v1`; those paths are
hash-bound source locations and never record identity, project targets, or
portable asset references.

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
styleReferenceBindings?: character_single_image only; exact one-element reviewed style-only array defined below
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

`styleReferenceBindings` is conditionally present only when the validated
character planning handoff contains the reviewed durable binding. Its value is
an array of exactly one closed six-member object:

```yaml
- role: style_only
  projectRelativePath: exact reviewed asset path
  sha256: exact raw asset SHA-256
  reviewRecordId: exact gmstyleref1 ID
  reviewRecordPath: exact project-relative review-record path
  reviewRecordSha256: exact raw review-record SHA-256
```

The same array is copied byte-semantically into exactly four top-level
locations: `routingHashPayload.styleReferenceBindings`,
`routingHashPayload.normalizedRequest.styleReferenceBindings`,
`routingHashPayload.authoringHandoff.styleReferenceBindings`, and the resulting
`generated_media_routing_v2.styleReferenceBindings`. It is forbidden inside
`typeSpecification`, `identityConsistencyLock`, or
`singleImageSpecification`. When the planning handoff has no binding, all four
members are absent rather than `null` or `[]`. Missing, extra, differently
ordered, nested, or unequal projections are
`style_reference_binding_projection_mismatch`.

`normalizedRequest` is a closed object with
`requestId`, `assetType`, `domainType`, `contentId`, `contentUsage`,
`planningSnapshotHash`, `requiredElements`, `prohibitedElements`, and the same
exact `typeSpecification` object as the payload. It additionally contains the
same optional top-level `styleReferenceBindings` only for the reviewed
character case and the same scalar `animationRequestId` only for animation. No
source files, registry selection, routing references, timestamps, or downstream
fields occur in it.

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
styleReferenceBindings?: same character-only conditional presence and exact array
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
styleReferenceBindings?: same character-only conditional presence and exact array
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

This is an additive conditional projection within the existing current v2
schema. Published `generated_media_routing_v2` records whose source planning
handoff had no style binding remain valid and byte-identically reusable with
the member absent. They and their index entries are never rewritten. A new
planning handoff that contains the binding necessarily produces a different
payload hash/record ID and cannot reuse an older record that omitted it.

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

The routing index schema is intentionally unchanged. It must not contain a
`styleReferenceBindings` member. The entry's exact `routingPayloadSha256` and
`recordSha256` bind the complete conditional projection; adding the array to an
index entry is `unknown_record_field` and would invalidate existing indexes.

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

## Durable style-reference review v1

GeneratedMediaStyleReferenceBindingGuide.md exclusively owns the closed
`generated_media_style_reference_review_v1` record, `gmstyleref1` identity,
asset path, raw review-record hash, and closed review index. These are immutable
Generated Media review artifacts, not character planning decisions, prompt
records, previews, evaluation results, or promoted images. They use the same
JCS, canonical JSON plus LF, record-before-index, no-clobber, CAS, and
failure-atomicity rules as the records below.

## Prompt v3

This section is the schema and byte authority for the current
`character_single_image` producer. It closes `generated_media_prompt_v3`, its
hash payload, Markdown file, index, and generation handoff for that type. It
does not authorize another asset type, PixelLab, a variant, a rotation set, or
provider execution. A producer presented with another prompt schema or an
unclosed type returns `unsupported_record_schema` and writes nothing.

### Closed character prompt record and nested values

The closed `generated_media_prompt_v3` top-level member set is exactly the
following. No member is nullable. `referenceBindings` is the only optional
top-level member and is allowed only for the reviewed character style-only
case. `revision` in a source item is the only optional source-item member.

```yaml
schemaVersion: generated_media_prompt_v3
promptRecordId:
promptPayloadSha256:
requestId:
assetType: character_single_image
domainType: character
contentId:
planningHandoffPath:
routingRecordId:
routingRecordPath:
routingRecordSha256:
routingPayloadSha256:
planningSnapshotHash:
sourcePlanningFiles:
  - path:
    role:
    sha256:
    revision?:
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId: character_single_image_v2
profileKey: character_single_image@2.0.0
provider: imagegen
structureProfile: character_single_image_v2
visualBrief: exact closed generated_media_visual_brief_v2 value
visualBriefSha256:
expressionProfileKey: exact registered character expression profile key
expressionProfilePayload: exact closed payload from GeneratedMediaVisualPromptAuthoringGuide.md
expressionProfilePayloadHash:
scenePromptOriginal:
providerPromptPayloadHash:
providerSettingsIntent:
  canvas:
    width: positive JSON integer
    height: positive JSON integer
  generationBackground:
    mode: removable_solid
    color: exact planning value
  outputFormat: png
providerSettingsIntentSha256:
requiredElements: non-empty ordered array copied from the routing record
prohibitedElements: non-empty ordered array or exact signed no_prohibitions value copied from the routing record
referenceBindings: conditional exact style-only array from the visual brief; absent otherwise
promptMarkdownPath:
promptMarkdownSha256:
status: ready_for_generation
createdAt: exact planningSnapshot.capturedAt copied from the verified planning handoff
validation:
  status: valid
  routingRecord: valid
  planningSnapshot: valid
  visualBrief: valid
  expressionProfile: valid
  providerPromptPayload: valid
  providerSettingsIntent: valid
  promptMarkdown: valid
  recordIdentity: valid
```

Every object named above is closed at its owning schema. In particular,
`sourcePlanningFiles` items have exactly `path`, `role`, `sha256`, and the
conditionally present `revision`; `canvas` has exactly `width` and `height`;
`generationBackground` has exactly `mode` and `color`; and `validation` has
exactly the nine displayed members. `visualBrief` must first pass the closed
`generated_media_visual_brief_v2` character-single-image contract in
GeneratedMediaVisualPromptAuthoringGuide.md. The prompt record copies that
entire value byte-semantically. The expression-profile payload uses the exact
key-discriminated closed shape owned by that same guide. The legacy-compatible
profile has exactly its original three top-level members; the animation-ready
profile additionally has the exact proportion, detail-density, color/value,
and authoring-projection members. Existing records are validated against the
shape selected by their stored key and are never required to gain new members.
`projectbs_character_bold_outline_compressed_detail@1.0.0` is a separate closed lock-array
shape with proportion, outline hierarchy, facial simplification, compressed
detail, color signature, ink treatment, and authoring projection members. It is
valid only when its stored key selects that exact registered payload; no
existing record is required or allowed to gain those members.
`projectbs_character_bold_outline_compressed_detail@2.0.0` is another exact
closed lock-array shape. Its immutable successor payload and seven ordered
positive plus seven ordered negative locks add exact planning-bound
64/56/5-or-narrower mark budgets, conditional ochre anchor site classes, and
the exact disabled or enabled ink-halo union. A v1 record is never upgraded,
and a v2 record never borrows v1 locks or defaults. `scenePromptOriginal`,
`visualEvidenceMap`, and provider payload independently preserve every
successor binding and lock.

`projectbs_character_open_ink_wash_dynamic_contour@1.0.0` is a separate
single-image-only closed lock-array shape with exactly the eleven policy
members and two ordered seven-item lock arrays owned by the visual guide. A
record stores its exact key, complete payload, and
`37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd` hash.
`scenePromptOriginal`, `visualEvidenceMap`, and provider payload independently
preserve all profile members, locks, and planning-bound Korean/Joseon
identity/equipment anchors. The payload's accepted-image SHA is audit-only
metadata: without a reviewed durable project-relative style-only publication,
it creates no `referenceBindings` entry, path, identity source, or edit target.
This profile cannot be inherited by an animation record.

`projectbs_character_open_ink_wash_dynamic_contour@2.0.0` is a distinct
single-image-only successor with exact payload hash
`b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5`.
It has the exact nineteen-member closed shape and ordered 9+9 locks owned by
the visual guide. It adds closed proportion measurement, surface-detail,
uniform-background, provider-output conformance, and compact-receipt
contracts. A v1 prompt record remains v1 and is never upgraded or interpreted
through this successor.

For either open ink-wash key, an external reviewed style-only binding may be
present without changing the profile payload. `referenceBindings`, when
present, contains exactly one six-member object owned by
GeneratedMediaStyleReferenceBindingGuide.md. It is byte-identical to the
visual brief and planning/routing projection, hash-significant in prompt
identity, absent from `scenePromptOriginal`, and never identity or edit evidence.

The animation-only
`projectbs_character_bold_outline_attack_motion_flow@1.0.0` never changes this
single-image record. Its animation prompt record preserves the exact reference
record path/file hash, the base v2 key/payload hash, the separately registered
successor key/payload/hash, all eight approved motion bindings, and the complete
ordered positive/negative animation locks. These values participate in the
animation prompt payload identity. Omitting the base or successor half,
flattening them into one rehashed v2 payload, or changing the scalar
`animationRequestId` is `record_identity_mismatch`.

`providerSettingsIntent` is not a provider request and cannot contain quality,
model, seed, attempt, cost, or tool fields. Generation resolves its separately
approved closed provider settings. `scenePromptOriginal` is a non-empty Unicode
string with LF internal line endings, no CR code point, no BOM, and no terminal
LF. For any lock-array profile it contains the complete positive and negative
style-lock statements in their normative array order. For the sparse profile it
contains the complete eight-member policy projection instead.

For a new prompt selected by
`projectbs_character_open_ink_wash_dynamic_contour@2.0.0`, the complete
`visualBrief`, `visualEvidenceMap`, and `scenePromptOriginal` are first derived
by the closed raw-Git-blob projection in
GeneratedMediaVisualPromptAuthoringGuide.md::Deterministic open ink-wash v2
authoring projection. In particular, the prompt text is the exact 28-line
`requiredElements`/negative-lock/positive-lock projection; alternate wording,
summary text, headings, blank lines, translated duplicates, reordered evidence,
or checkout-derived CRLF input is not byte-semantically equivalent. A producer
must calculate every downstream hash and ID only after this projection. Two
different in-memory results for the same verified authority bundle are
`record_identity_mismatch` before any write; neither result is authority.

This deterministic producer rule is prospective. Existing published prompt
records/index entries remain valid under their stored schema and exact bytes;
they are never rewritten merely because their historical prose predates this
projection rule.

Calculate the three nested hashes before constructing the prompt hash payload:

```text
visualBriefSha256 = lowercase_hex(SHA256(canonicalJson(visualBrief)))
providerPromptPayloadHash = lowercase_hex(SHA256(canonicalJson({
  "schemaVersion":"imagegen_character_single_image_prompt_v2",
  "scenePromptOriginal": scenePromptOriginal
})))
providerSettingsIntentSha256 = lowercase_hex(SHA256(canonicalJson(providerSettingsIntent)))
```

### Closed prompt hash payload, projection, ID, and paths

Project the validated source record into exactly this member set. The values
are copied byte-semantically from the same-named record members unless stated
otherwise. Unknown or missing fields reject before projection.

```yaml
schemaVersion: generated_media_prompt_hash_payload_v3
requestId:
assetType: character_single_image
domainType: character
contentId:
planningHandoffPath:
routingRecordId:
routingRecordPath:
routingRecordSha256:
routingPayloadSha256:
planningSnapshotHash:
sourcePlanningFiles:
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId: character_single_image_v2
profileKey: character_single_image@2.0.0
provider: imagegen
structureProfile: character_single_image_v2
visualBrief:
visualBriefSha256:
expressionProfileKey:
expressionProfilePayload:
expressionProfilePayloadHash:
scenePromptOriginal:
providerPromptPayloadHash:
providerSettingsIntent:
providerSettingsIntentSha256:
requiredElements:
prohibitedElements:
referenceBindings: conditional exact style-only array; omit when absent
promptMarkdownSha256:
```

The projection excludes exactly `promptRecordId`, `promptPayloadSha256`,
`promptMarkdownPath`, `status`, `createdAt`, and `validation`. It changes only
`schemaVersion`. It excludes all file roots, mtimes, host/user data, authoring
wall-clock time, index state, generation handoff, provider execution/approval,
attempt, cost, result, packaging, evaluation, and promotion data. The Markdown
path is excluded because it is derived from the ID; its raw byte hash remains
included and binds the body without creating an ID/path cycle.
Conditional `referenceBindings` is included when present and omitted when
absent; this preserves every existing prompt identity while making a new
durable binding hash-significant.

```text
promptPayloadSha256 = lowercase_hex(SHA256(canonicalJson(promptHashPayload)))
hashPrefix = first 20 hexadecimal characters of promptPayloadSha256
promptRecordId = gmprompt3.character_single_image.{contentId}.{hashPrefix}
```

`contentId` must already be a validated safe single path/ID segment; it is not
encoded or sanitized. The exact paths are:

```text
record:   AgentDocs/planning-data/generated-media-prompts/v2/character_single_image/{contentId}/{promptRecordId}.json
Markdown: AgentDocs/planning-data/generated-media-prompts/v2/character_single_image/{contentId}/{promptRecordId}.prompt.md
index:    AgentDocs/planning-data/generated-media-prompts/v2/character_single_image/{contentId}/prompt_index.json
```

Re-projecting a record must reproduce the full payload hash and ID. The record
file bytes are `canonicalJson(record) + LF`. `promptRecordSha256`, used only by
the detached handoff/index consumers, is SHA-256 of those exact bytes.

### Closed Markdown body and raw-byte identity

The Markdown file is intentionally a copy-ready body with no generated header,
fence, front matter, record ID, or commentary. Its exact bytes are:

```text
UTF8(scenePromptOriginal) + one LF byte (0A)
```

Internal line endings must already be LF. UTF-8 is strict and has no BOM. The
file has exactly one terminal LF: empty prompt text, CRLF, a missing terminal
LF, two terminal LFs, or any Unicode/whitespace change is a different invalid
body. `promptMarkdownSha256` is the lowercase SHA-256 of these complete raw
bytes. Validation compares raw bytes; it never normalizes a file and therefore
returns `prompt_markdown_mismatch` for CRLF/LF differences.

### Closed prompt index and entry projection

The index has exactly this schema:

```yaml
schemaVersion: generated_media_prompt_index_v3
assetType: character_single_image
contentId:
entries:
  "{promptRecordId}":
    promptRecordId: must equal the containing object key
    recordSchemaVersion: generated_media_prompt_v3
    recordPath:
    recordSha256:
    promptPayloadSha256:
    promptMarkdownPath:
    promptMarkdownSha256:
    requestId:
    assetType: character_single_image
    domainType: character
    contentId:
    planningSnapshotHash:
    routingRecordId:
    routingRecordSha256:
    routingPayloadSha256:
    registryVersion: generated_media_authoring_profile_registry_v2
    registryRowId: character_single_image_v2
    profileKey: character_single_image@2.0.0
    provider: imagegen
    structureProfile: character_single_image_v2
    visualBriefSha256:
    providerPromptPayloadHash:
    providerSettingsIntentSha256:
    status: ready_for_generation
```

`entries` is an object keyed by exact record ID, not an array. Its entry is the
exact projection above from the validated record plus the two exact file hashes
and canonical paths. JCS lexicographically orders all object keys, including
entry IDs; arrays retain source order. Index bytes are `canonicalJson(index) +
LF`. A timestamp, count, latest pointer, handoff, tombstone, alias, attempt, or
downstream field is forbidden.

### Closed detached generation handoff

After record, Markdown, and index bytes have been published and reread, the
authoring stage returns one detached object. It is not a prompt-record member or
a separately persisted artifact, so it cannot create a self-hash or index-hash
cycle. Its exact schema is:

```yaml
schemaVersion: generated_media_generation_handoff_v2
requestId:
assetType: character_single_image
domainType: character
contentId:
planningSnapshotHash:
routingRecordId:
routingRecordPath:
routingRecordSha256:
routingPayloadSha256:
registryVersion: generated_media_authoring_profile_registry_v2
registryRowId: character_single_image_v2
profileKey: character_single_image@2.0.0
provider: imagegen
structureProfile: character_single_image_v2
promptRecordId:
promptRecordPath:
promptRecordSha256:
promptPayloadSha256:
promptMarkdownPath:
promptMarkdownSha256:
promptIndexPath:
promptIndexSha256:
visualBriefSha256:
providerPromptPayloadHash:
providerSettingsIntentSha256:
status: ready_for_generation
```

Every value is an exact projection from the record/index or a raw file hash.
Generation recomputes all three file hashes, validates the index entry and
record projection, and accepts no caller summary in their place. A changed
index caused by a later valid append changes only `promptIndexSha256`; it
requires a freshly projected handoff but does not change the immutable prompt
record. The authoring prompt must return the complete object and its
`lowercase_hex(SHA256(canonicalJson(generationHandoff)))` as
`generationHandoffSha256`; that hash is output metadata, not a member.

### Idempotency, collisions, CAS, and failure atomicity

Under one same-scope exclusive lock, compute the payload, ID, Markdown bytes,
record bytes, record/index paths, and expected entry fully in memory. Validate
the complete existing index and every referenced entry before any write.

1. No addressed files or entry exist: stage all bytes in same-directory
   temporary files. Publish Markdown and record with atomic no-clobber, reread
   both, then publish the complete index with compare-and-swap against the
   validated prior index bytes.
2. Record, Markdown, and exact entry all exist: validate closed schemas,
   canonical bytes, re-projected payload/ID/path, raw hashes, and exact entry.
   Return the existing bytes unchanged with `status=reused_identical`. This is
   the only normal idempotent reuse.
3. Exact valid record and Markdown exist but the addressed entry is absent:
   treat them as one recoverable crash orphan, add only the exact entry by CAS,
   and return `status=reused_identical`. Never rewrite either file.
4. Only one of record/Markdown exists, the entry exists without both files, or
   an entry/file projection differs: write nothing and return
   `index_entry_invalid`, `prompt_markdown_mismatch`, or `record_collision` as
   applicable. Remediation is a separately authorized operation.
5. An occupied ID/path with different canonical bytes, payload hash, or
   projection is `record_collision`; an 80-bit prefix collision is included.
6. A pre-existing index with unknown/missing fields, non-canonical bytes,
   invalid entries, or a changed CAS preimage is never normalized or
   overwritten.

If a new publication fails before index commit, remove only files created by
this transaction and only after their exact bytes still match the staged
bytes. A successful rollback leaves the prior index byte-identical and returns
`prompt_record_write_failed`, `prompt_markdown_write_failed`, or
`prompt_index_write_failed` with `safeToRetry=true`. If exact rollback cannot
be proved and completed, preserve evidence, write no handoff, return
`prompt_publish_rollback_failed`, and set `safeToRetry=false`. Thus a handled
failure never intentionally leaves a partial/orphan publication. Temporary
files are not workflow artifacts and are removed when their identity is known.

Unknown/missing fields, schema/version/type mismatch, input/hash mismatch,
collision, and invalid pre-existing index state create no files and are not
safe to retry unchanged. A retry is safe only after the named source/contract
decision changes, except for the three rolled-back transient write failures.

### Character prompt fixed vector

The executable vector is
`tests/test_generated_media_prompt_v3_contract.mjs`. It validates closed
top-level and nested key rejection, missing members, prompt payload projection,
RFC 8785-compatible canonical bytes, LF versus CRLF raw hashes, record/hash
mismatch, occupied-ID collision, dangling-index rejection, recoverable exact
record/Markdown orphan completion, `reused_identical`, detached handoff hash
bindings, and rollback with no partial publication. Its fixed identity is:

```text
promptRecordId=gmprompt3.character_single_image.character.contract_vector.1.807cb5e2a6fc9669478f
promptPayloadSha256=807cb5e2a6fc9669478f874d4f689cd3e3fa0d40f4bb64fff63c8779c1462703
promptRecordSha256=647a258e8c9e75a8a4f2f1d920467688cb74ecbb310ecd6e68c40be1521f6146
promptMarkdownSha256=d5701576f7dde359bcf106c9a89a5548f6bc855e607c596e1344c6b026830c9c
promptIndexSha256=7378ddcd043c623a3a885d3436ed159b02871436a1ee4571e0d97d006350abdc
generationHandoffSha256=642f900ec42f17ad192f58023d5b9f0154c419e2d40454c702cf44af1082f8cf
```

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

- require the closed non-submit capability response, bind its descriptor and
  defaults-resolved settings into scope, and preserve its immutable evidence
  reference in cost evidence;
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

### Hosted Preview v1

Hosted preview is not `generated_media_generation_v2`. Its canonical record and
index are isolated from promotable generation:

```text
AgentDocs/planning-data/generated-media-hosted-preview/v1/{assetType}/{contentId}/{workUnitId}/
  {previewRecordId}.json
  preview_index.json
output/generated-media-preview/v1/{assetType}/{contentId}/{workUnitId}/original.{ext}
```

The closed record is `generated_media_hosted_preview_record_v1`. Its exact
member set is below. `animationRequestId` is required only for an animation
work unit and forbidden otherwise. `referenceBindings` is a role-discriminated
ordered array. Existing non-style roles remain closed
`{role, projectRelativePath, sha256}` objects. `role=style_only` instead requires
exactly `{role, projectRelativePath, sha256, reviewRecordId, reviewRecordPath,
reviewRecordSha256}` under GeneratedMediaStyleReferenceBindingGuide.md. A
three-member style entry and every absolute/transient path are forbidden.

```yaml
schemaVersion: generated_media_hosted_preview_record_v1
previewRecordId:
requestId:
assetType:
domainType:
contentId:
animationRequestId?:
planningSnapshotHash:
promptRecordId:
promptRecordSha256:
promptFileSha256:
providerPromptPayloadHash:
referenceBindings:
executionMode: hosted_builtin_preview_v1
previewScopeHash:
hostedPreviewApproval: one exact manual approval or automatic approval attestation
hostedPreviewApprovalSha256:
hostedPreviewAutoApprovalPolicy?: required only for automatic attestation; forbidden for manual approval
hostedPreviewAutoApprovalPolicySha256?: required only for automatic attestation; forbidden for manual approval
settingsSeal:
settingsSealSha256:
provider: imagegen
providerTool: built-in_imagegen
toolMode: exact observed callable mode
submitCount: 1
retryCount: 0
costKnown: false
capabilityEvidenceStatus: unavailable_on_callable_surface
settingsEvidenceStatus: exposed_options_only
costEvidenceStatus: unavailable_on_callable_surface
previewOnly: true
notPromotable: true
notEvaluated: true
observableOutputPath: canonical project-relative preview path
observableOutputSha256: 64-lowercase-hex
createdAt: exact observed RFC 3339 timestamp with offset
validation:
  status: valid
  approval: valid
  scope: valid
  promptAndReferences: valid
  submitLimit: valid
  outputHash: valid
```

`hostedPreviewApproval`, optional `hostedPreviewAutoApprovalPolicy`, and
`settingsSeal` use exactly the closed schemas in
GeneratedMediaImageGenOnlyContractGuide.md section 6.1.1. Manual records omit
both policy members. Automatic records require both, recompute the policy and
authorization-source hashes, and bind the selected attestation to the final
`previewScopeHash`. The record contains
no descriptor, descriptor version, provider evidenceRef, guessed default,
price, preservation handoff, evaluation result, or promotion state.

```text
previewPayloadSha256 = SHA-256(JCS(record excluding previewRecordId, validation))
previewRecordId = gmpreview1.{assetType}.{contentId}.{workUnitId}.{hash[0:20]}
```

The closed index contains exactly `schemaVersion`
(`generated_media_hosted_preview_index_v1`), `assetType`, `contentId`,
`workUnitId`, and an `entries` object keyed by previewRecordId. Each entry
contains exactly `previewRecordId`, `recordPath`, `recordSha256`,
`previewPayloadSha256`, `previewScopeHash`, `requestId`, `assetType`,
`domainType`, `contentId`, conditional `animationRequestId`,
`observableOutputPath`, and `observableOutputSha256`.

The index is closed to this schema and exact record path/raw SHA. Identical
scope and output bytes reuse identical record bytes. Same ID with different
bytes is `record_collision`; an occupied scope with any consumed submit is
`hosted_preview_submit_limit_exceeded`, not a new record. Record-before-index,
CAS, raw UTF-8/LF and failure-atomicity rules match current records. Neither the
record nor its media path satisfies any generation-v2 or preservation-v2 input
schema.

### Hosted fast preview terminal receipt v1

`hosted_builtin_fast_preview_v1` persists no planning, routing, authoring,
prompt, preservation, evaluation-package, or promotion record. Its only
workflow evidence is the detached
`generated_media_fast_preview_terminal_receipt_v1` defined by
GeneratedMediaImageGenOnlyContractGuide.md plus the observed preview file.

When a new preview succeeds, the canonical non-promotable output path is:

```text
output/generated-media-fast-preview/v1/{assetType}/{contentId}/{idempotencyKey}/original.{observedExtension}
```

The receipt's `outputObservation.path` is that project-relative path and its
hash is over the exact raw bytes. The file and receipt are preview evidence
only: `previewOnly=true`, `notPromotable=true`, `notPreserved=true`, and
`strictEvaluationPerformed=false`. The inline `visualEvaluation` is a
non-scoring preview observation, not a generated-media evaluation record.

There is no fast-preview record index, latest pointer, prompt index mutation,
or CAS projection. Idempotency is checked against active execution state and
previous terminal receipts before submit. A matching completed receipt is
returned unchanged as `completed_reuse`; ambiguous, divergent, dangling, or
already-active evidence blocks as `fast_preview_duplicate_submit_risk`.
Neither reuse nor warning resolution rewrites immutable upstream artifacts.

Unavailable provider controls, contract/schema discrepancies, and missing
capability/cost attestations are recorded only in the ordered terminal warning
arrays. They are not provider-enforcement evidence and do not become canonical
record fields elsewhere. A user-adopted preview must enter the existing strict
workflow through a separate explicit request. The preview receipt alone cannot
be promoted or preserved; an exact accepted post-result capture v1 record is
the only additive bridge for already-existing accepted bytes.

### Accepted post-result capture v1

`generated_media_accepted_result_capture_v1` is an immutable evidence envelope,
not a generation-v2 record and not a retroactive preflight attestation. The
generation role owns its production in a distinct post-result capture task. It
performs no provider, capability, cost, preservation, evaluation, promotion,
Unity, planning, routing, or prompt mutation.

Canonical paths are:

```text
non-animation:
AgentDocs/planning-data/generated-media-accepted-result-capture/v1/{assetType}/{contentId}/{captureRecordId}.json
AgentDocs/planning-data/generated-media-accepted-result-capture/v1/{assetType}/{contentId}/capture_index.json
animation:
AgentDocs/planning-data/generated-media-accepted-result-capture/v1/animation/{contentId}/{animationRequestId}/{captureRecordId}.json
AgentDocs/planning-data/generated-media-accepted-result-capture/v1/animation/{contentId}/{animationRequestId}/capture_index.json
```

The record has exactly these members. `animationRequestId` is required only for
animation and forbidden otherwise. Every `path` is the exact observed source
path; it may be absolute because it is evidence location, never canonical
identity or a project target. All hashes bind raw file bytes.

```yaml
schemaVersion: generated_media_accepted_result_capture_v1
captureRecordId:
capturePayloadSha256:
requestId:
assetType:
domainType:
contentId:
animationRequestId?:
planningSnapshotHash:
routingRecordId:
routingRecordSha256:
sourceExecutionEvidence:
  taskId:
  toolCallId:
  provider: imagegen
  providerTool:
  providerResultRef?: exact observed value only
  historicalSubmitCount: 1
  historicalRetryCount: 0
userAcceptance:
  authorityType: authenticated_user_acceptance
  acceptanceMessageId:
  acceptedAt: RFC 3339 timestamp with offset
  acceptedArtifactSha256:
promptEvidence:
  path:
  fileSha256:
  providerPromptPayloadHash:
settingsEvidence:
  path:
  fileSha256:
referenceEvidence:
  - role:
    path:
    sha256:
resultEvidence:
  providerMaster:
    path:
    sha256:
    mediaType: image | animated_gif
  completedGif:
    path:
    sha256:
    width:
    height:
    frameCount:
  frames:
    - frameIndex:
      path:
      sha256:
capabilityEvidenceStatus: unavailable_observed
costEvidenceStatus: unavailable_observed
preSubmitGateAttestation: not_claimed_post_result_capture
captureAction:
  providerCalled: false
  submitCount: 0
  retryCount: 0
downstreamAuthorization:
  preservationAuthorized: true
  evaluationAuthorized: true
  promotionAuthorized: false
  promotionPrerequisites: strict_evaluation_pass_and_explicit_project_mapping
createdAt: exact userAcceptance.acceptedAt copied byte-semantically
validation:
  status: valid
  acceptance: valid
  executionEvidence: valid
  identities: valid
  rawHashes: valid
  memberClosure: valid
```

`referenceEvidence` is a non-empty ordered array and preserves each submitted
role/path/hash exactly; it grants no new identity or edit-target semantics.
`frames` is ordered by unique zero-based `frameIndex`, has exactly the observed
GIF timeline count, and each member is extracted from the captured completed
GIF. For coherent-master-to-GIF v2 the master mediaType is `image`, the GIF is
generation-role-built, and the six frame entries match the reopened timeline.
For existing provider-native GIF evidence the master mediaType is
`animated_gif`; that mode remains separate and unchanged.

```text
capturePayloadSha256 = SHA-256(JCS(record excluding captureRecordId, capturePayloadSha256, validation))
captureRecordId = gmaccept1.{assetType}.{contentId}.{optionalAnimationRequestId}.{capturePayloadSha256[0:20]}
```

The closed index contains exactly `schemaVersion`
(`generated_media_accepted_result_capture_index_v1`), `assetType`, `contentId`,
conditional `animationRequestId`, and `entries`. Each entry is keyed by
`captureRecordId` and contains exactly `captureRecordId`, `recordPath`,
`recordSha256`, `capturePayloadSha256`, `requestId`, conditional
`animationRequestId`, `sourceGenerationTaskId`, `providerMasterSha256`,
`completedGifSha256`, and `acceptedArtifactSha256`.

Write the record first, then CAS-append the index. An identical payload, record
bytes and index projection returns the original bytes as `reused_identical`.
An occupied ID with different bytes is `accepted_capture_record_collision`;
neither record nor index is overwritten. Raw UTF-8, RFC 8785 JCS, LF, one
terminal LF, failure atomicity, and no-clobber rules match all current records.

The detached terminal receipt has exactly these members:

```yaml
schemaVersion: generated_media_accepted_result_capture_receipt_v1
state: captured | reused_identical | blocked
requestId:
animationRequestId?:
captureRecordId?: required unless blocked
captureRecordPath?: required unless blocked
captureRecordSha256?: required unless blocked
capturePayloadSha256?: required unless blocked
captureIndexSha256?: required unless blocked
sourceGenerationTaskId:
providerMasterSha256:
completedGifSha256:
providerCalled: false
submitCount: 0
retryCount: 0
historicalSubmitCount: 1
historicalRetryCount: 0
capabilityEvidenceStatus: unavailable_observed
costEvidenceStatus: unavailable_observed
preSubmitGateAttestation: not_claimed_post_result_capture
preservationAuthorized: boolean
evaluationAuthorized: boolean
promotionAuthorized: false
failureType?: required only when blocked
nextStep: preservation | stop
receiptPayloadSha256:
```

`receiptPayloadSha256` is SHA-256 of RFC 8785 JCS for the exact receipt excluding
only itself. `captured` and `reused_identical` require both downstream booleans
true and `nextStep=preservation`; `blocked` requires both false and
`nextStep=stop`. No receipt may claim a provider call, a passed pre-submit gate,
known capability/cost, evaluation verdict, project mapping, or promotion.
Promotion remains forbidden until a strict evaluation `PASS` and explicit
project mapping are both supplied to the existing promotion contract.

### Compact profile-conformance receipt

The v2 open ink-wash post-output triage returns one response-only receipt. It
is not a planning, routing, prompt, preview, preservation, evaluation, or
promotion record and creates no file, index row, or CAS projection.

The receipt has exactly these members and no others:

```yaml
schemaVersion: generated_media_profile_conformance_receipt_v1
requestId:
planningSnapshotHash:
promptRecordId:
promptRecordSha256:
expressionProfileKey: projectbs_character_open_ink_wash_dynamic_contour@2.0.0
expressionProfilePayloadHash: b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5
observableOutputSha256:
profileConformanceStatus: preview_conformant_no_downstream | preview_profile_nonconformant | preview_profile_conformance_blocked
failureType: none | character_preview_open_ink_wash_v2_proportion_age_nonconformant | character_preview_open_ink_wash_v2_contour_mok_seon_nonconformant | character_preview_open_ink_wash_v2_surface_detail_nonconformant | character_preview_open_ink_wash_v2_pigment_negative_space_nonconformant | character_preview_open_ink_wash_v2_background_nonconformant | character_preview_open_ink_wash_v2_identity_equipment_nonconformant | character_preview_open_ink_wash_v2_reference_role_nonconformant | character_preview_open_ink_wash_v2_evidence_insufficient
gateResults:
  - gateId: proportion_age | contour_mok_seon | surface_detail | pigment_palette_negative_space | background | identity_equipment | reference_role
    result: pass | fail | evidence_insufficient
providerCalled:
submitCount:
retryCount:
nextStep: no_downstream | stop_no_retry_not_final
receiptPayloadSha256:
```

`gateResults` contains exactly seven entries in the displayed order. All pass
requires `failureType=none`, `preview_conformant_no_downstream`, and
`nextStep=no_downstream`. The first fail in order supplies the matching
nonconformant failure token. With no fail, the first insufficient result
supplies the evidence-insufficient token. Either nonpass requires
`stop_no_retry_not_final`. `providerCalled` preserves the historical submit
fact and post-output triage does not increment `submitCount` or `retryCount`.

`receiptPayloadSha256` is lowercase SHA-256 of RFC 8785 JCS canonical UTF-8
bytes for the exact receipt excluding only `receiptPayloadSha256`. After the
full receipt is returned once, subsequent status or consolidation messages use
only `schemaVersion`, `requestId`, `observableOutputSha256`,
`profileConformanceStatus`, `failureType`, and `receiptPayloadSha256`. They do
not resend profile payloads, prompt prose, source-planning arrays, routing
fields, or registry rows. The recipient may reuse this receipt for reporting,
but any later mutation boundary must still perform the authority checks owned
by that boundary.

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
prompt_record_write_failed
prompt_markdown_write_failed
prompt_index_write_failed
prompt_publish_rollback_failed
provider_value_invalid
unsupported_record_schema
missing_provider_execution_approval
invalid_provider_execution_approval
provider_execution_scope_mismatch
provider_capability_descriptor_unavailable
provider_capability_preflight_invalid
provider_capability_drift
provider_cost_unit_mismatch
provider_cost_estimate_unavailable
provider_cost_limit_exceeded
provider_actual_cost_unavailable
retry_limit_exceeded
duplicate_provider_call_risk
missing_hosted_preview_approval
invalid_hosted_preview_approval
missing_hosted_preview_auto_approval_policy
invalid_hosted_preview_auto_approval_policy
hosted_preview_auto_approval_policy_mismatch
hosted_preview_auto_approval_policy_revoked
hosted_preview_scope_mismatch
hosted_preview_unknown_setting
hosted_preview_prompt_drift
hosted_preview_reference_drift
hosted_preview_submit_limit_exceeded
hosted_preview_retry_forbidden
hosted_preview_output_missing
hosted_preview_output_hash_mismatch
hosted_preview_preservation_forbidden
hosted_preview_promotion_forbidden
fast_preview_duplicate_submit_risk
fast_preview_authority_or_safety_violation
fast_preview_callable_input_absent
fast_preview_submit_failed_no_retry
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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
