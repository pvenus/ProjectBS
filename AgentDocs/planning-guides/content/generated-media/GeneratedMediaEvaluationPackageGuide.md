# Generated Media Evaluation Package Guide

## 1. Purpose

Guide Type: schema/data-structure. This guide normalizes preserved PixelLab and
ImageGen outputs for a separate evaluation task. Packaging describes immutable
media; it does not generate, score, repair, approve, promote, or publish.

## 2. Authority and Boundary

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
```

ContentFolderStructureGuide owns staging/project separation. The preservation
guide owns byte acquisition and extraction. This guide owns package identity,
canonical hashing, profiles, sealing, and evaluation handoff. Evaluation guides
own scores and verdicts.

## 3. Staging and Immutable Layout

Resolve `{evaluationStagingRoot}` from current-PC configuration. Never accept a
foreign absolute root. The staging source must not equal or sit below the
project target.

Assemble on the same filesystem in a non-canonical temporary path:

```text
{evaluationStagingRoot}/.assembling/{requestId}/{preservationRecordId}.{attemptId}/
```

This path is never an evaluation source. After payload validation and package
ID calculation, the immutable final layout is:

```text
{evaluationStagingRoot}/{assetType}/{contentId}/{requestId}/{packageId}/
  planning/
  prompt/
  generation/
  preservation/
  source/
  extracted/
  manifest.json
  evaluation-request.json
```

Every copied file has SHA-256. Source bytes are immutable after preservation.
Evaluator previews belong in evaluation evidence, not this package.

## 4. Non-circular Identity and Hashing

`manifest.json` is an envelope around `manifestPayload`.

```yaml
schemaVersion: generated_media_evaluation_package_v1
packageId:
manifestPayloadHash:
manifestPayload: {}
sealedAt:
evaluationReadiness: ready | blocked
evaluationBlockers: []
```

Canonical hash input is only `manifestPayload`. Exclude `packageId`,
`manifestPayloadHash`, `sealedAt`, readiness/blockers, absolute roots, and any
signature/hash derived from this payload. Canonicalization is UTF-8 JSON with
object keys sorted lexicographically, array order preserved, no insignificant
whitespace, LF string newlines, and no BOM.

```text
manifestPayloadHash = SHA256(canonical_json(manifestPayload))
packageId = evalpkg.{assetType}.{contentId}.{requestId}.{manifestPayloadHash[0:12]}
```

Deterministic verification:

1. validate payload schema and profile extension;
2. canonicalize the payload exactly once;
3. recompute `manifestPayloadHash`;
4. recompute `packageId` from payload identity and hash prefix;
5. compare both envelope values;
6. verify member hashes; then write `sealedAt` and readiness.

Finalize only after all steps pass. On the same filesystem, flush files when
supported and atomically rename the complete temporary directory to
`{packageId}`. If direct atomic rename is unavailable, copy to a sibling
`.finalizing-{packageId}`, reverify every byte/hash, then rename it. Never expose
a partial canonical package directory.

Never overwrite an existing canonical path. If its packageId,
manifestPayloadHash, member hashes, and bytes all match, reuse it idempotently
and discard the temporary assembly. Any mismatch is `package_collision`. A
different payload produces a different packageId and final path. Temporary
data may remain as blocked evidence but is never called sealed.

Never overwrite a sealed package. Changed stable payload facts create a new
hash and package ID. Changing only readiness/blockers or seal time does not
change the payload hash; after sealing, even envelope changes require a new
seal record rather than mutation.

## 5. manifestPayload Contract

```yaml
requestId:
assetType:
domainType:
contentId:
legacyArtifactType: optional compatibility alias
planningSnapshotHash:
promptRecordId:
providerPromptOriginalHash:
generationRecordId:
preservationRecordId:
provider:
providerResultRefs: []
providerSettings:
generationAttempts: []
structureProfile:
profileExtension: {}
members:
  - memberId:
    role:
    relativePath:
    sha256:
    mediaType:
    width:
    height:
    order:
    profileData: {}
projectTarget:
  path: optional informational path
  status: informational_only
```

`provider-prompt.txt` is byte-equal to the submitted prompt after LF
normalization. All relative paths resolve within the package.
Provider comparison uses ASCII lowercase and the stored value must be canonical
`pixellab` or `imagegen`; any other value blocks sealing.

## 6. Profile Extension Schemas

Schemas are closed: unknown fields fail with `unknown_profile_field`; missing
required fields block sealing. Optional fields may be omitted. `null` is valid
only where explicitly stated. Profile member `order` is unique and contiguous
from zero.

### 6.1 ordered_rotation_set

Required `profileExtension`:

```yaml
directionOrder: [north, north_east, east, south_east, south, south_west, west, north_west]
expectedCount: 8
identityConsistencyRequired: true
```

Exactly eight members are required. Each member `profileData` requires
`direction` and `rotationIndex`; rotationIndex equals member order and direction
equals the corresponding directionOrder entry. No duplicates or extras.

### 6.2 ordered_frame_set

Required `profileExtension`:

```yaml
animationRequestId:
animationType: attack | idle | move
directionOrder: non-empty ordered directions
loopMode: loop | one_shot | hold_last
timingMode: per_frame_ms | uniform_fps
uniformFps: required positive integer only for uniform_fps
expectedFrameCountByDirection: {}
```

Each frame member requires `animationRequestId`, `animationType`, `direction`,
`frameIndex`, `frameOrder`, and `timingMs`. For `uniform_fps`, do not round one
frame duration repeatedly. For zero-based global frame order `n`, compute
`boundaryMs(n)=round_half_up(n * 1000 / uniformFps)` using exact rational
arithmetic, then `timingMs(n)=boundaryMs(n+1)-boundaryMs(n)`. This distributes
remainder milliseconds deterministically and bounds cumulative error below
one millisecond. Reject zero/negative durations, non-integer FPS, floating-point
shortcut results, or a timing array that differs from this calculation. For
`per_frame_ms`, every supplied timingMs is a positive integer. Frames sort by directionOrder then ascending
frameIndex; frameOrder is global contiguous order. Missing directions, count
mismatch, duplicate frame index, unsupported animationType, or timing mismatch
blocks. Optional `sourceExportMemberId` links to a preserved export.

### 6.3 single_image

Required `profileExtension`:

```yaml
primaryMemberId:
selectedProviderResultRef:
selectionStatus: provisional_not_evaluated
originalMediaRole: icon_original | imagegen_original
```

Exactly one primary source member is allowed and its ID/ref must match the
preservation record. Optional `providerMimeType` may be omitted, not guessed.
Preview, thumbnail, or unselected variants cannot be primary members.

### 6.4 paired_sheet_animation

Required `profileExtension`:

```yaml
referenceMemberId:
sheetMemberId:
rows:
columns:
cellWidth:
cellHeight:
usableFrameCount:
frameOrder: row_major
loopMode: loop | one_shot | hold_last
timingMs: positive integer or ordered positive-integer array
frameMemberIds: ordered list
```

Rows, columns, cell dimensions, and usable count are positive integers;
usableFrameCount cannot exceed rows times columns. Reference/sheet IDs resolve
to distinct source members. `frameMemberIds` length equals usableFrameCount and
each extracted frame requires `frameIndex`, `row`, `column`,
`sourceSheetMemberId`, and matching cell dimensions. Order is row-major; extra
cells are declared unused, not silently extracted.

## 7. evaluation-request.json

```yaml
requestId:
evaluationPackageId:
assetType:
domainType:
contentId:
legacyArtifactType: optional
structureProfile:
sourceManifestPath: manifest.json
sourceOrManifestHash: manifestPayloadHash
planningSnapshotHash:
promptHash:
evaluationAdapterId:
evaluationAdapterStatus: ready | missing | unsupported
projectTargetPath: optional informational only
nextTask: evaluation
```

Readiness is `ready` only when files, hashes, profile order, planning/prompt/
generation/preservation identities, and evaluator adapter validate. Otherwise
seal as blocked only when preserved evidence is internally consistent.

## 8. Compatibility

New records use `assetType + domainType`; legacy adapters may receive only the
explicit `legacyArtifactType` mapping:

| assetType | domainType | legacyArtifactType |
| --- | --- | --- |
| character_main_image | character | character_image |
| character_animation | character | character_animation |
| icon | skill | skill_icon |
| icon | item | item_icon |
| general_animation | skill | skill_animation |
| imagegen_image | stage | story_popup_main_image |
| imagegen_image | battle | battle_background |

Unknown mapping blocks; never infer from filenames.

## 9. Failure and Validation

```text
evaluation_staging_root_not_configured
staging_project_path_violation
record_identity_mismatch
preserved_source_missing
member_hash_mismatch
manifest_payload_hash_mismatch
package_identity_mismatch
unknown_profile_field
missing_profile_field
structure_profile_mismatch
manifest_order_invalid
evaluation_adapter_missing
unsupported_legacy_artifact_mapping
package_collision
package_finalize_failed
package_seal_failed
```

- recompute member hashes, canonical payload hash, and package identity;
- validate the exact closed profile schema;
- verify staging/project separation and all record identities;
- return package ID/path, manifestPayloadHash, readiness/blockers, and request;
- do not generate, download, evaluate, promote, write Slack, modify Unity,
  perform Git, or deploy.
