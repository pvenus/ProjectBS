# PixelLab Legacy Audit Guide

## Mandatory Authority

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```

This retained filename is a read-only audit entry for already-existing
PixelLab v1 evidence. It is not a pipeline and cannot author prompts, call a
provider, create records/index entries, download, export, package, retry,
charge credits, or modify evidence.

## Input and Work

Accept explicit project-relative immutable legacy record/index/media paths and
expected stored schema, identity and hash. Read bytes, verify their links and
report historical provider/profile/structure facts. Never use current-only
guides as v1 schema authority and never repair missing evidence.

## Output and Failure

Return `status=audited`, `mode=read_only_legacy_audit`, evidence paths, observed
schemas, identity/hash verification, findings, `mutationsPerformed=false`,
`providerCalled=false`, and `costIncurred=false`.

Use only the legacy failure registry in the mandatory authority. Any request
for execution, reproduction or mutation returns
`failureType=legacy_execution_forbidden` before side effects.
