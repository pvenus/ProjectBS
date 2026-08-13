# Generated Media Current Record Guide

## Purpose and Boundary

Guide Type: current v2/v3 schema authority. It owns canonical JSON, immutable
identity, paths, indexes and state handoffs for the ImageGen-only flow.
Legacy record schemas are owned by
GeneratedMediaLegacyV1CompatibilityGuide.md and do not appear as current
examples here.

## Canonical Rules

UTF-8, no BOM, LF newlines, lexicographically sorted object keys, preserved
array order, no insignificant whitespace. Unknown fields and missing required
fields reject before ID calculation. Hashes are lowercase SHA-256 hex. Paths
are project-relative and never include another PC's root.

## Current Paths and Indexes

```text
routing:     AgentDocs/planning-data/generated-media-routing/v2/...
prompts:     AgentDocs/planning-data/generated-media-prompts/v2/...
generation:  AgentDocs/planning-data/generated-media-generation/v2/...
preservation:AgentDocs/planning-data/generated-media-preservation/v2/...
```

Animation adds `{contentId}/{animationRequestId}/` before the record filename.
Indexes are v2, deterministically sorted by record ID, and store exact identity,
record schema, path and file hash. Current work never writes a v1 index.

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
provider=imagegen, settings hash, approvalScopeHash, and optional animation ID.
generationRecordId=gmgen2.{assetType}.{contentId}.{optionalAnimationRequestId}.{hash[0:20]}
```

Before an external call, look up the deterministic ID and active attempt:

- identical completed result -> reuse without billing;
- identical active attempt -> block `duplicate_provider_call_risk`;
- changed prompt/settings/approval -> new identity or collision, never append as
  an equivalent retry;
- attempts cannot exceed approved `maxAttempts`;
- every attempt records costEvidence, including unavailable/not_charged.

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
provider_cost_not_approved
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
