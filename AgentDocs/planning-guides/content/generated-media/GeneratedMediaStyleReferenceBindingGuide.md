# Generated Media Style-Only Reference Binding Guide

## Purpose and ownership

This guide owns the durable reviewed style-reference asset, review record,
index, and consumer binding contract for Generated Media. It does not own
character identity, planning decisions, prompt prose, provider execution,
evaluation, preservation, promotion, or Unity assets.

The first supported scope is exactly `character_single_image`. A durable
style-only binding can carry line, pigment, palette-distribution, negative-space,
and non-semantic composition-density evidence. It never carries the depicted
person, identity, pose, action, clothing, equipment, or edit-target semantics.

## Canonical paths and naming

The stable style family ID is lowercase snake case and matches
`^[a-z][a-z0-9_]*$`. Asset names are the lowercase raw SHA-256 plus the exact
reviewed extension. The canonical paths are:

```text
asset:
AgentDocs/reference-assets/generated-media/style-only/{assetType}/{styleReferenceId}/{assetSha256}.{ext}

review directory:
AgentDocs/planning-data/style-reference-reviews/v1/{assetType}/{styleReferenceId}/

review record:
{reviewRecordId}.json

review index:
review_index.json
```

For the approved open ink-wash reference, the exact identities are:

```text
assetType=character_single_image
styleReferenceId=open_ink_wash_dynamic_contour
assetPath=AgentDocs/reference-assets/generated-media/style-only/character_single_image/open_ink_wash_dynamic_contour/b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf.png
assetSha256=b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf
```

An absolute path, generated-images path, worktree path, preview path, failed
output, unhashed alias, `latest`, or mutable friendly filename is never a
binding. The durable asset is reference evidence under `AgentDocs`; it is not a
generated or promoted image under `Assets/ImagesGenerated`.

## Closed review record v1

`generated_media_style_reference_review_v1` contains exactly these members:

```yaml
schemaVersion: generated_media_style_reference_review_v1
reviewRecordId:
reviewPayloadSha256:
assetType: character_single_image
styleReferenceId:
purpose: style_only
asset:
  projectRelativePath:
  sha256:
  mediaType: image/png
  byteLength:
  pixelDimensions: {width: positive integer, height: positive integer}
profileBindings:
  - expressionProfileKey:
    expressionProfilePayloadHash:
allowedObservationCategories:
  - contour_openness
  - pressure_variable_mok_seon
  - broad_rough_pigment
  - palette_role_distribution
  - negative_space_balance
  - non_semantic_composition_density
prohibitedSemanticTransfers:
  - person
  - person_identity
  - canonical_character_identity
  - pose
  - action
  - clothing
  - equipment
  - edit_target
providerReferencePolicy:
  authorizedRole: style_only
  capabilitySupportRequired: true
  identityReferenceRole: prohibited
  editReferenceRole: prohibited
  promptSubjectDescriptionFromReference: prohibited
reviewAuthority:
  type: authenticated_user_selected_best_cut
  sourceThreadId:
  reviewedAssetSha256:
status: approved
validation:
  schema: valid
  assetBytes: valid
  purposeAndTransferBoundary: valid
  profileBindings: valid
```

Every object is closed. `profileBindings` is a non-empty ordered array of
closed two-member objects. It authorizes only use with the exact registered
key/hash pairs and does not modify either payload. Observation and prohibition
arrays are non-empty, unique, and in the displayed normative order.

Construct the review hash payload by excluding exactly `reviewRecordId`,
`reviewPayloadSha256`, and `validation` from the validated record:

```text
reviewPayloadSha256 = lowercase_hex(SHA256(RFC8785_JCS_UTF8(reviewHashPayload)))
reviewRecordId = gmstyleref1.{assetType}.{styleReferenceId}.{reviewPayloadSha256[0:20]}
recordPath = review directory + reviewRecordId + ".json"
recordBytes = RFC8785_JCS_UTF8(record) + LF
reviewRecordSha256 = lowercase_hex(SHA256(recordBytes))
```

The asset is copied only after its source bytes independently hash to the
declared value. The published copy is then rehashed; source and destination
must match. PNG signature, exact byte length, and IHDR dimensions must equal the
record. A collision, mismatched source/destination hash, unknown field, invalid
purpose, or transfer-policy drift stops without a record or index update.

## Closed review index v1

The sibling index contains exactly:

```yaml
schemaVersion: generated_media_style_reference_review_index_v1
assetType: character_single_image
styleReferenceId:
entries:
  {reviewRecordId}:
    reviewRecordId:
    recordPath:
    recordSha256:
    reviewPayloadSha256:
    assetPath:
    assetSha256:
    purpose: style_only
    status: approved
```

Entries is an object keyed by exact record ID. Record-before-index, no-clobber,
compare-and-swap, failure-atomicity, strict UTF-8, LF-only, no-BOM, and raw-byte
hash rules match GeneratedMediaRecordGuide.md. Reuse requires exact record,
asset, and index bytes; any same-ID or same-asset divergence is
`style_reference_record_collision`.

## Closed consumer binding

A planning snapshot, authored prompt record, generation scope, or preview may
carry a durable style-only reference only as this exact six-member object:

```yaml
role: style_only
projectRelativePath: exact reviewed asset path
sha256: exact raw asset SHA-256
reviewRecordId: exact gmstyleref1 ID
reviewRecordPath: exact project-relative review record path
reviewRecordSha256: exact raw review-record SHA-256
```

The binding is valid only when:

1. the asset, review record, and index resolve at their canonical paths;
2. all three raw hashes recompute exactly;
3. the review record has `purpose=style_only`, `status=approved`, the requested
   `assetType`, matching asset identity, and the exact selected profile key/hash;
4. every prohibited semantic transfer remains present and unchanged;
5. planning identity/equipment facts come from planning evidence, never the
   reference;
6. the callable provider surface can express a separate style-only reference
   role. If it cannot, generation returns its existing capability/unknown-
   setting blocker before submit rather than relabeling the image as identity;
7. provider prose contains no description of the reference person's face,
   pose, action, clothes, or equipment.

Legacy closed `{role, projectRelativePath, sha256}` reference entries remain
valid only for their existing non-style roles and records. A style-only binding
must use all six members; a three-member `role=style_only` entry is invalid.

## Planning and stage projection

New planning decisions use
`character_open_ink_wash_planning_projection_v2` when durable style fidelity is
required. Its `styleReferenceFidelity` branch is:

```yaml
mode: durable_style_only_binding
providerReferenceAuthorized: true
binding: exact closed six-member style-only consumer binding
```

The six binding leaves are captured as separate approved facts. The completed
planning snapshot therefore carries exact asset path/hash and review
record ID/path/hash without adding them to character identity or required
equipment. Routing copies no bytes and does not reselect the role. Authoring
revalidates the record/index/assets, copies the exact binding into the prompt
record's conditional `referenceBindings`, and keeps it out of
`scenePromptOriginal`. Generation revalidates all hashes and provider role
support before submit. Preview/generation scope hashes bind the complete object.

This external review record satisfies the existing `prohibited_until_reviewed_`
condition in open ink-wash v1/v2 accepted-reference contracts. It does not
change either expression-profile key, payload, payload hash, lock, or meaning.

## Failure types

```text
missing_style_reference_review_record
style_reference_review_record_hash_mismatch
style_reference_review_payload_mismatch
style_reference_asset_missing
style_reference_asset_hash_mismatch
style_reference_record_collision
style_reference_index_invalid
style_reference_binding_incomplete
style_reference_binding_scope_mismatch
style_reference_role_invalid
style_reference_semantic_transfer_forbidden
```

Planning, authoring, and generation return only the token owned by their current
stage boundary and do not repair, copy, re-review, or relabel a failed binding.

## Related guides

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md
```
