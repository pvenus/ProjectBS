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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
```
