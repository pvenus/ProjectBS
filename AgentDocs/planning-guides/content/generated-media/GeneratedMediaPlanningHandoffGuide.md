# Generated Media Planning Handoff Guide

## Purpose and Authority

Guide Type: current schema/data-structure. This guide defines
`generated_media_planning_handoff_v2`, the only planning handoff eligible for a
new Generated Media request.

```text
Master Concept -> approved planning -> this immutable handoff -> router v2
```

Planning owns all identity, visual meaning, layout and motion decisions. This
guide owns serialization/readiness only. Missing values block; they are never
inferred. Legacy v1 is owned only by
`GeneratedMediaLegacyV1CompatibilityGuide.md`.

Planning execution inherits
`GeneratedMediaNoninteractiveExecutionPolicyGuide.md`. Routine in-scope reads,
validation, and bounded immutable handoff writes do not request interactive
approval again; new authority boundaries remain fail-closed.

## Common Closed Schema

Use the exact common and type-specific schema in
`GeneratedMediaImageGenOnlyContractGuide.md`. Required common fields are:

```text
schemaVersion=generated_media_planning_handoff_v2
requestId, assetType, domainType, contentId, contentUsage
sourcePlanningFiles with exact path/role/sha256 and optional authority revision
planningSnapshot capturedAt/snapshotHash/approvedFacts
non-empty requiredElements and prohibitedElements or signed no_prohibitions
optional informational-only projectTarget
optional character-single-image-only styleReferenceBindings under the closed
GeneratedMediaStyleReferenceBindingGuide.md contract
```

Unknown fields are rejected after schema selection. All paths are
project-relative. Snapshot identity is canonical JSON over exact source entries
and approved facts according to GeneratedMediaRecordGuide.md.

## Closed Planning Snapshot v2

This section is the sole current authority for planning-snapshot structure and
identity. Producers and domain guides reference it; they must not redefine a
different `approvedFacts` or hash payload schema.

### Deterministic producer-owned planning capture

There is no caller-owned `planningCaptureInputs` approval object. After the
planning producer has written and validated one immutable current decision, it
derives capture identity from repository facts without asking a caller to pick
an ID, timestamp, or source order.

The derivation order is exact:

1. Resolve `contentId` and `assetType` from the approved canonical planning.
2. Build the domain-owned ordered `sourcePlanningFiles` projection. For current
   character planning this is the canonical character planning path first,
   followed by each project-relative character design-decision JSON appearing
   in `provenance.sourcePlanningRefs`, preserving first-occurrence array order.
   Repeated byte-identical paths are ignored after their first occurrence.
3. The final design-decision entry is the current capture authority. Its
   `/approval/approvedAt` value MUST be an RFC 3339 timestamp with an explicit
   numeric offset. Copy it unchanged as `planningSnapshot.capturedAt`.
4. Build and validate `approvedFacts`, then calculate `snapshotHash` under the
   exact payload below.
5. Derive the stable request identity exactly as:

```text
requestId = gmplan2.{assetType}.{contentId}.{snapshotHash[0:20]}
```

The producer never uses a wall-clock value while constructing the handoff.
The decision producer may stamp `approval.approvedAt` once when it creates the
new no-clobber decision; retries reuse those immutable decision bytes. A retry
over the same sources and facts therefore derives the same timestamp,
snapshot, request ID, path, and handoff bytes.

The current failure meanings are closed:

- no eligible canonical/decision source: `missing_source_planning_path`;
- unreadable, invalid, or non-project-relative derived source:
  `unresolved_source_planning_path`;
- missing current-decision approval timestamp:
  `missing_capture_authority_timestamp`;
- nonconforming current-decision approval timestamp:
  `invalid_capture_authority_timestamp`;
- source projection, request derivation, pointer/value, or stored/recomputed
  snapshot disagreement: `planning_snapshot_mismatch`.

Any failure writes no handoff or partial capture artifact. Caller-supplied
capture IDs, timestamps, source arrays, or overrides are unknown input and are
ignored as non-authoritative prose; they never replace this derivation.

This rule applies to new handoffs produced after its authoritative publication.
Already-published immutable v2 handoffs keep their stored legacy capture
identity and remain read-only historical evidence; do not rewrite them or
retroactively require the `gmplan2.` request form. A producer MUST NOT create a
new legacy-form handoff after publication. Validators distinguish these cases
by repository existence on the selected authoritative baseline, never by a
caller-provided compatibility flag.

### Exact source and fact schema

`sourcePlanningFiles` is a non-empty ordered array. Source order is assigned by
planning authority and preserved byte-semantically in the hash payload.

```yaml
sourcePlanningFiles:
  - path: non-empty project-relative UTF-8 string
    role: non-empty stable lowercase_snake_case string
    sha256: exactly 64 lowercase hexadecimal characters over exact file bytes
    revision?: optional non-empty authority-supplied string; omit when unavailable
```

Unknown members, duplicate paths, absolute paths, and invented revisions are
invalid. Before snapshot construction, read every source as exact bytes and
verify `sha256`. A JSON source must be valid UTF-8 JSON.

When the capture authority is a published Git revision, `exact bytes` means
the raw blob bytes at the pinned authoritative revision (`{revision}:{path}`).
The producer MUST NOT hash a working-tree materialization, apply line-ending
normalization, or fall back to checkout bytes. In particular, Windows CRLF
projection cannot change a published source hash, snapshot hash, or derived
request ID. An unavailable authoritative blob is
`unresolved_source_planning_path`; disagreement with stored source identity is
`planning_snapshot_mismatch`.

`approvedFacts` is a non-empty array with this exact closed item schema:

```yaml
approvedFacts:
  - factId: non-empty stable UTF-8 string, unique within the snapshot
    sourcePath: exact byte-equal match to one sourcePlanningFiles.path
    sourcePointer: valid RFC 6901 JSON Pointer resolving in that source JSON
    value: exact resolved JSON value; any RFC 8785/JCS-compatible JSON value
```

Unknown members are forbidden. A fact is invalid when its source is not listed,
its pointer is syntactically invalid or unresolved, or its `value` is not
deeply equal to the resolved JSON value under JSON semantics. Non-finite
numbers, duplicate object keys, invalid Unicode, and non-JSON values are
forbidden.

Fact order is deterministic. Sort `approvedFacts` ascending by:

1. zero-based index of `sourcePath` in `sourcePlanningFiles`;
2. UTF-8 byte lexicographic order of `sourcePointer`;
3. UTF-8 byte lexicographic order of `factId`.

Two facts with the same sourcePath/sourcePointer/factId are duplicates and
invalid. Array values inside a fact preserve their approved source order.

### Exact hash payload and timestamp rule

The exact closed hash payload is:

```yaml
schemaVersion: generated_media_planning_snapshot_hash_payload_v2
sourcePlanningFiles: exact validated ordered array above
approvedFacts: exact validated and sorted array above
```

No other member is allowed. In particular, `capturedAt`, request/content IDs,
filesystem metadata, host/user data, Git state, and wall-clock time are excluded
from snapshot identity.

```text
snapshotCanonicalBytes = RFC8785_JCS(planningSnapshotHashPayload) encoded as UTF-8 without BOM or trailing LF
snapshotHash = lowercase_hex(SHA256(snapshotCanonicalBytes))
```

The handoff stores:

```yaml
planningSnapshot:
  capturedAt: exact RFC 3339 timestamp copied from the current immutable decision authority
  snapshotHash: exact 64-lowercase-hex hash calculated above
  approvedFacts: exact same sorted array used by the hash payload
```

`capturedAt` is provenance only and is excluded from `snapshotHash`. It is
copied unchanged from the current immutable decision's `/approval/approvedAt`;
the handoff producer never uses its current clock. A retry for the same source
bytes and approved facts reuses the same decision timestamp and produces
byte-identical handoff content.

### Deterministic vector

The exact canonical payload bytes for the normative vector are one UTF-8 line
with no trailing LF:

```json
{"approvedFacts":[{"factId":"example.identity","sourcePath":"AgentDocs/planning-data/character/act-plans/player/character.example.1.json","sourcePointer":"/identity/characterId","value":"character.example.1"}],"schemaVersion":"generated_media_planning_snapshot_hash_payload_v2","sourcePlanningFiles":[{"path":"AgentDocs/planning-data/character/act-plans/player/character.example.1.json","role":"canonical_character_planning","sha256":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"}]}
```

Its fixed `snapshotHash` is:

```text
0a528c76ba8f6575b0ed3938b0d48bf5b851eabb8e3ecbaca07b33faba33ff59
```

The vector verifies canonicalization and hashing only; its repeated `a` source
hash is illustrative and does not waive real source-byte verification.

### Failure, retry, and publication

- source byte/hash mismatch, unresolved pointer, pointer/value mismatch,
  non-canonical payload projection, or stored/recomputed snapshot mismatch
  returns the central token `planning_snapshot_mismatch`;
- a blocked snapshot writes no handoff, index, placeholder, or partial record;
- for the same output path, exact canonical handoff bytes are idempotently
  reused without changing `capturedAt` or any byte;
- an occupied path with different bytes returns the central token
  `record_collision`; it is never overwritten or normalized;
- write a new immutable handoff with same-directory atomic no-clobber, then
  reread and verify exact bytes before exposing it to routing;
- retries never replace a valid immutable handoff and never derive a new
  timestamp from retry time.

## Type Contracts

- `character_single_image`: identityConsistencyLock plus complete
  singleImageSpecification with viewpoint, pose, framing, canvas,
  targetDisplaySize, safeArea, final/generation background, noShadow, outline,
  and pelvis/root plus ground-contact anchor. It may additionally contain
  exactly one `styleReferenceBindings` entry only when an approved
  `character_open_ink_wash_planning_projection_v2` durable branch supplies the
  same six leaf facts. The entry has exactly `role=style_only`,
  `projectRelativePath`, `sha256`, `reviewRecordId`, `reviewRecordPath`, and
  `reviewRecordSha256`; it is forbidden for semantic-text-only planning and all
  other types.
- `icon_single_image`: identityConsistencyLock, exact iconProfile, and complete
  singleImageSpecification with visual-center anchor.
- `background_single_image`: exact backgroundProfile and complete
  backgroundSpecification with scene contract, composition, viewpoint,
  horizon, ordered depth layers, playable/readability area, subject
  inclusions/exclusions, canvas/aspect, target display, safe area, final
  background policy, content/scene consistency lock, and
  scene_composition_anchor. Icon-only identity/silhouette/outline rules do not
  satisfy this contract.
- `animation`: non-empty `animationRequests`. Every entry has a unique
  animationRequestId and the complete reference/final-frame/timing/order/loop/
  key-pose/fixed-cell/scale/vertical-motion/background/outline/anchor/master-
  first contract. Every new entry also fixes
  `animationSourceMode=provider_native_animated_gif` and
  `extractionMode=gif_timeline_exact`; still/contact-sheet/sprite-sheet/video
  sources and independently generated frame sets are prohibited. Character
  entries use pelvis/root plus ground-contact axis; skill entries use effect
  origin. Existing immutable `fixed_cell_only` handoffs remain historical
  read-only evidence and cannot authorize a new generation.

The handoff may carry multiple animationRequests. The router alone fans them
out in source order. Every downstream handoff and record contains exactly one.

## Failure and Readiness

Use the typed blockers in GeneratedMediaImageGenOnlyContractGuide.md without
renaming. Readiness requires every source hash/snapshot and applicable type
field to validate. A style-only binding additionally requires the exact durable
asset/review/index bytes and selected profile binding; it does not satisfy any
identity or equipment fact. `status=ready_for_routing` is forbidden when any
blocker is present.

## Boundary and Related Guides

This guide does not route, author prompts, call a provider, package, evaluate,
promote, or perform Git work.

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```

## Transparent Foreground Projection

When planning selected `generated_media_transparent_foreground_selection_v1`,
the canonical snapshot and handoff project that object byte-for-byte with key
`generated_media_true_alpha_foreground@1.0.0` and payload hash
`2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108`.
Routing and authoring may copy and hash it only; omission, unknown members,
conditional-branch mixing, safe-margin drift, or animation coordinate/scale
drift is `true_alpha_projection_mismatch`. It is forbidden when planning did
not select it and never changes `backgroundFullyOpaque` semantics.
