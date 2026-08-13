# PixelLab Animation Legacy Audit Guide

## Mandatory Authority

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```

This retained filename only audits immutable historical PixelLab reference,
sheet, extracted-frame, timing, loop and hash evidence.

## Read-only Contract

Input is explicit project-relative existing evidence plus expected stored
schema/identity/hash. Verify reference/sheet/frame relationships, fixed stored
order and hashes without extraction or repair. Provider calls, prompt/record/
index creation, downloads, retries, billing, packaging and media modification
are forbidden.

Return the authority's read-only audit output. Missing or inconsistent evidence
uses its legacy failure registry. Any execution/mutation request returns
`legacy_execution_forbidden` before side effects.
Every blocked or audited result states `providerCalled=false`,
`mutationsPerformed=false`, and `costIncurred=false`.
