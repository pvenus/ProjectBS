# PixelLab Character Legacy Audit Guide

## Mandatory Authority

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```

This retained filename only audits immutable historical PixelLab character
records, stored eight-direction members, animation members and hashes. The
direction/profile names are observed evidence, not generation instructions.

## Read-only Contract

Input must identify existing project-relative evidence and expected stored
identity/hash. Verify record links, provider provenance, ordered-member counts,
directions, animationRequestId, timing and hashes without changing bytes.
Provider/tool access, new prompts or records, index writes, export/extraction,
retry, cost, packaging and original modification are forbidden.

Output uses the authority's `status=audited` schema. Missing or inconsistent
evidence uses its legacy failure registry. Any execution or mutation request
returns `legacy_execution_forbidden` with providerCalled=false,
mutationsPerformed=false and costIncurred=false.
