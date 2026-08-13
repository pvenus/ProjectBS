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
```

Unknown fields are rejected after schema selection. All paths are
project-relative. Snapshot identity is canonical JSON over exact source entries
and approved facts according to GeneratedMediaRecordGuide.md.

## Closed Planning Snapshot v2

This section is the sole current authority for planning-snapshot structure and
identity. Producers and domain guides reference it; they must not redefine a
different `approvedFacts` or hash payload schema.

### Closed planning capture input

The caller or planning-orchestration owner supplies exactly one capture input
for each requested handoff. The authoring agent validates and copies it; it
must not create a timestamp, choose sources, sort paths, or repair identity.

```yaml
planningCaptureInputs:
  - contentId: exact intended handoff contentId
    requestId: exact intended handoff requestId
    capturedAt: RFC 3339 timestamp with explicit numeric UTC offset (+HH:MM or -HH:MM)
    sourcePlanningPaths: non-empty ordered array of unique project-relative source paths
```

Unknown members are forbidden. `Z`, local time without an offset, filesystem
mtime, Git time, and producer current time are invalid `capturedAt` values.
`contentId` and `requestId` must be byte-equal to the handoff identity. Each
`sourcePlanningPaths` entry must be readable and must correspond exactly once,
at the same array index, to `sourcePlanningFiles[].path`. No path may be added,
omitted, deduplicated, or reordered by the producer.

The current failure meanings are closed:

- absent capture object: `missing_planning_capture_inputs`;
- missing or nonconforming explicit-offset timestamp:
  `invalid_planning_capture_timestamp`;
- missing/empty path member: `missing_source_planning_path`;
- duplicate path bytes: `duplicate_source_planning_path`;
- unreadable or non-project-relative path: `unresolved_source_planning_path`;
- contentId/requestId disagreement: `planning_capture_identity_mismatch`;
- source order or path-to-sourcePlanningFiles projection disagreement after
  validation: `planning_snapshot_mismatch`.

Any failure writes no handoff or partial capture artifact.

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
  capturedAt: exact RFC 3339 timestamp supplied by the approved planning authority
  snapshotHash: exact 64-lowercase-hex hash calculated above
  approvedFacts: exact same sorted array used by the hash payload
```

`capturedAt` is provenance only and is excluded from `snapshotHash`. It must be
copied unchanged from the approved planning capture/decision; a producer must
not use its current clock. A retry for the same source bytes and approved facts
must reuse the same `capturedAt` and produce byte-identical handoff content.

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
  and pelvis/root plus ground-contact anchor.
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
  first contract. Character entries use pelvis/root plus ground-contact axis;
  skill entries use effect origin.

The handoff may carry multiple animationRequests. The router alone fans them
out in source order. Every downstream handoff and record contains exactly one.

## Failure and Readiness

Use the typed blockers in GeneratedMediaImageGenOnlyContractGuide.md without
renaming. Readiness requires every source hash/snapshot and applicable type
field to validate. `status=ready_for_routing` is forbidden when any blocker is
present.

## Boundary and Related Guides

This guide does not route, author prompts, call a provider, package, evaluate,
promote, or perform Git work.

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRequestRoutingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
