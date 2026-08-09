# Generated Media Preservation and Packaging Guide

## 1. Purpose and Scope

Guide Type: workflow/pipeline. This guide owns the task after provider
generation: download/export, immutable preservation, profile-specific
extraction, manifest assembly, package sealing, and evaluation-request handoff.
It never invokes generation, rewrites prompts, scores media, promotes assets,
writes Slack, modifies Unity, performs Git, or deploys.

## 2. Required References and Priority

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```

ContentFolderStructureGuide owns project destinations and staging separation.
GeneratedImageGenerationPipelineGuide owns the generation-only boundary. This
guide begins only from its provider-reference handoff. The package guide owns
manifest/hashing. Type pipeline guides own adapter requirements. Contradiction
returns `reference_contract_conflict`; do not choose broader behavior.

## 3. Input Contract

```yaml
planningHandoffFile: project-relative generated_media_planning_handoff_v1
promptRecordId: generated_media_prompt_v1 identity
generationRecordId: generated_media_generation_v1 identity
generationRecordSha256: SHA-256 of canonical generated_media_generation_v1 record
provider: canonical lowercase pixellab | imagegen
assetType:
domainType:
contentId:
requestedAdapterId:
expectedStructureProfile:
providerResultRefs: non-empty refs from generation record
projectTarget: optional informational destination
```

Normalize provider comparison only by ASCII lowercase; stored/output value must
be exactly `pixellab` or `imagegen`. Any other spelling is unsupported. All
identities and hashes must agree. Resolve the current PC's evaluation root
internally. Foreign absolute paths, staging/project overlap, missing refs, or a
generation status other than `generated` block before download.

## 4. Independent State Model

```text
preservation_not_started
-> provider_result_resolved
-> downloading_or_exporting
-> originals_preserved
-> extracting
-> manifest_ready
-> package_sealed
-> evaluation_handoff_ready
```

Terminal non-success states are `blocked` and `failed`. Retry reuses immutable
generation refs and resumes only from hash-verified state. It must not retry
generation or change selection. Expired refs return
`provider_result_unavailable_requires_generation_task`.

## 5. Adapter Registry

| Provider | Asset type | Adapter ID | Structure profile | Child pipeline guide | Responsibility |
| --- | --- | --- | --- | --- | --- |
| pixellab | character_main_image | pixellab_character_rotation_export_v1 | ordered_rotation_set | AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md | preserve export/archive; extract eight rotations |
| pixellab | character_animation | pixellab_character_animation_export_v1 | ordered_frame_set | AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md | preserve export; extract animation/direction/frame order |
| pixellab | icon | pixellab_icon_original_png_v1 | single_image | AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md | preserve one selected original PNG, never preview |
| pixellab | general_animation | pixellab_general_animation_sheet_v1 | paired_sheet_animation | AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md | preserve reference and sheet; extract ordered frames |
| imagegen | imagegen_image | imagegen_original_media_v1 | single_image | AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md | preserve one original returned medium |

Resolve one row by exact canonical provider + assetType + requestedAdapterId +
expectedStructureProfile equality. Read only its registered child guide. Zero
or multiple rows, a caller-supplied alternate guide, or identity mismatch
blocks with `unsupported_preservation_adapter`; never choose by domainType,
filename, or judgment. Provider UI/export details come from that child guide.

## 6. Preservation Record Identity and Storage

Apply the canonical JSON rules in GeneratedMediaRecordGuide.md to this closed
payload:

```yaml
preservationHashPayload:
  schemaVersion: generated_media_preservation_hash_payload_v1
  requestId:
  assetType:
  domainType:
  contentId:
  planningSnapshotHash:
  promptRecordId:
  generationRecordId:
  generationRecordSha256:
  provider: pixellab | imagegen
  adapterId:
  structureProfile:
  providerResultRefs: ordered exactly as generation record
```

Exclude record ID, timestamps, attempts, local paths, downloaded/member hashes,
package ID, and mutable state.

```text
preservationPayloadHash = SHA256(canonical_json(preservationHashPayload))
preservationRecordId = gmpreserve.{assetType}.{contentId}.{preservationPayloadHash[0:20]}
AgentDocs/planning-data/generated-media-preservation/v1/{assetType}/{contentId}/{preservationRecordId}.json
AgentDocs/planning-data/generated-media-preservation/v1/{assetType}/{contentId}/preservation_index.json
```

`generated_media_preservation_index_v1` entries sort by record ID and contain
request/asset/domain/content identity, generationRecordId and SHA-256, payload
hash, state, optional packageId, and exact record path. Same ID with different
payload is `preservation_record_collision`. Re-executing the same payload
reuses the record: return verified sealed success or resume its append-only
attempts from the last verified state. Never create a second identity.

```yaml
schemaVersion: generated_media_preservation_v1
preservationRecordId:
preservationPayloadHash:
requestId:
assetType:
domainType:
contentId:
planningSnapshotHash:
promptRecordId:
generationRecordId:
generationRecordSha256:
provider:
adapterId:
structureProfile:
providerResultRefs: []
originalMembers: []
extractedMembers: []
memberHashes: []
state:
attempts: []
failureType: optional
packageId: optional after seal
createdAt:
```

The record is append-only during an attempt and immutable after seal. Changed
generation refs or generation record bytes change the payload and identity.

## 7. Failure Behavior

```text
missing_generation_record
generation_not_ready
generation_record_hash_mismatch
record_identity_mismatch
provider_result_ref_missing
provider_result_unavailable_requires_generation_task
unsupported_preservation_adapter
evaluation_staging_root_not_configured
staging_project_path_violation
original_download_failed
provider_export_failed
source_not_original
source_hash_mismatch
extraction_failed
structure_contract_mismatch
manifest_validation_failed
package_seal_failed
preservation_record_collision
package_finalize_failed
package_collision
evaluation_adapter_missing
reference_contract_conflict
```

Preserve verified bytes and state on failure. Never repair, crop, resize, pad,
recolor, recompress, fabricate members, switch providers, or evaluate.

## 8. Output and Handoff

Success returns preservation record ID and exact project-relative record path,
package ID/path,
manifestPayloadHash, structure profile, readiness/blockers, and
evaluation-request path. `evaluation_handoff_ready` only permits a separate
evaluation task. Missing evaluator adapter yields a sealed blocked package.

Media assembly and finalize paths are owned by
GeneratedMediaEvaluationPackageGuide.md. This task must use its `.assembling`
path and return only the finalized canonical `{requestId}/{packageId}/` path;
temporary paths never appear as stagingArtifactPath or evaluation source.

## 9. Validation Checklist

- [ ] No generation provider was invoked and no prompt was changed.
- [ ] Every original/extracted hash verifies.
- [ ] Profile extension has no unknown or missing fields.
- [ ] Package ID/hash recompute deterministically.
- [ ] Staging and project target are distinct.
- [ ] Evaluation, promotion, Slack, Unity, Git, and deployment did not run.
