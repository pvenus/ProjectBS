# PixelLab Icon Legacy Audit Guide

## Mandatory Authority

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```

This retained filename only audits immutable historical PixelLab icon prompt,
generation, selected-source and single-image evidence.

## Read-only Contract

Input is explicit project-relative existing evidence plus expected stored
schema/identity/hash. Verify prompt-to-generation linkage, provider refs,
selected member identity and hashes without selecting a new result or changing
bytes. Provider calls, prompt/record/index writes, downloads, retries, cost,
packaging and media modification are forbidden.

Return the authority's read-only audit output. Missing or inconsistent evidence
uses its legacy failure registry. Any execution/mutation request returns
`legacy_execution_forbidden` before side effects.
Every blocked or audited result states `providerCalled=false`,
`mutationsPerformed=false`, and `costIncurred=false`.
