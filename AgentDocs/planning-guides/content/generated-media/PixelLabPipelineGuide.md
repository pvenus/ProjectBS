# PixelLab Generated Media Pipeline Guide

## 1. Purpose

Guide Type: workflow/pipeline. This is the PixelLab provider-level router for
three generated-media families:

```text
character_main_image or character_animation -> PixelLab Character Pipeline
icon                                       -> PixelLab Icon Pipeline
general_animation                          -> PixelLab Animation Pipeline
```

It defines routing across three independently retryable tasks: prompt
authoring, provider generation, and preservation/packaging. Evaluation is a
fourth separate task. It does not execute a generic fallback branch.

## 2. Required References

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md
AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
```

Then read exactly one child pipeline guide. The planning handoff owns content
meaning; the child guide owns PixelLab prompt/tool rules and declares its
preservation adapter. This guide owns routing and stage boundaries.

## 3. Shared Stage Model

```text
planning_handoff_received
-> prompt_authoring
-> prompt_ready
-> provider_generation
-> provider_result_ready
-> stop and hand off provider refs
-> preservation_packaging (separate task)
-> evaluation_package_sealed
-> stop and hand off package
-> evaluation (separate task) | blocked
```

Each arrow crossing a `stop` is a distinct owner with independent retry and
failure state. Generation consumes the exact prompt and returns provider refs;
it cannot download/export/extract/package. Preservation cannot invoke provider
generation. Evaluation never runs in either task.

## 4. Routing Contract

| assetType | Child guide | Prompt profile | Structure profile |
| --- | --- | --- | --- |
| character_main_image | PixelLabCharacterPipelineGuide.md | pixellab_character_prompt_v1 | ordered_rotation_set |
| character_animation | PixelLabCharacterPipelineGuide.md | pixellab_character_animation_prompt_v1 | ordered_frame_set |
| icon | PixelLabIconPipelineGuide.md | pixellab_icon_prompt_v1 | single_image |
| general_animation | PixelLabAnimationPipelineGuide.md | pixellab_animation_prompt_v1 | paired_sheet_animation |

`domainType` never selects a different execution prompt. It selects an approved
profile or evaluation adapter inside the child contract.

## 5. Common Records

New prompt records:

```text
AgentDocs/planning-data/generated-media-prompts/v1/{assetType}/{contentId}/{promptRecordId}.json
AgentDocs/planning-data/generated-media-prompts/v1/{assetType}/{contentId}/{promptRecordId}.prompt.md
```

New authoring writes `generated_media_prompt_v2`. Existing validated
`generated_media_prompt_v1` records remain read-only compatibility inputs under
GeneratedMediaRecordGuide.md and are never upgraded in place.

Every new authoring run requires the immutable `routingRecordFile` and matching
`planningHandoffFile`, validates the selected registry row and identities, and
does not perform registry selection independently.

New generation records:

```text
AgentDocs/planning-data/generated-media-generation/v1/{assetType}/{contentId}/{generationRecordId}.json
```

The authoritative field schemas are defined by
`GeneratedMediaRecordGuide.md`. Both records preserve `domainType`, optional `legacyArtifactType`, planning
snapshot hash, provider prompt/profile/hash, settings, attempts and provenance.
Records are immutable; revisions create new IDs.

## 6. Shared Gates

Before prompt authoring:

- validate `generated_media_planning_handoff_v1`;
- require non-empty requiredElements and prohibitedElements;
- require the child type-specific planning contract;
- reject planning inference and unsupported profile/domain combinations.

Before provider generation:

- verify prompt record identity and hash against the unchanged planning
  snapshot;
- verify exact PixelLab tool/page and current cost/credit before execution;
- stop on missing authentication, credits, tool or stale prompt;
- never substitute ImageGen.

Before preservation handoff:

- generation record contains only attempts/settings/result refs and adapter
  request;
- result refs are sufficient for a later independent task;
- no local media path or package identity is present.

Before evaluation handoff, in the preservation/package task:

- preserve provider originals through the child adapter;
- extract members without changing source bytes;
- build and hash the common evaluation package;
- report readiness/blockers without scoring.

## 7. Failure Types

```text
invalid_pixellab_request
unsupported_pixellab_asset_type
missing_planning_handoff
missing_required_elements
missing_prohibited_elements
prompt_record_missing
prompt_record_stale
provider_prompt_hash_mismatch
pixellab_unavailable
pixellab_authentication_required
pixellab_credit_insufficient
pixellab_tool_not_found
provider_operation_failed
provider_result_missing
preservation_adapter_failed
evaluation_package_failed
```

Failure never triggers another provider, evaluation, project promotion, Unity,
Git, or deployment.

## 8. Compatibility and Version Migration

Legacy `artifactType` is accepted only through the explicit mapping in the
package guide. `generated_media_generation_v1` replaces provider execution and
result-reference handoff only. The legacy generation-only/later-download
contract remains authoritative at this boundary. Download/export/extraction is
replaced by `generated_media_preservation_v1`; evaluation remains unchanged and
separate. Existing domain prompts remain deprecated compatibility entry points.

## 9. Completion

PixelLab generation completes when it writes a valid generation record and
preservation handoff or a typed blocker. The preservation task completes when
it seals a package or returns its own blocker. A visual PASS is never a
completion condition of either task.

## 10. Related Child Guides

```text
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
```

There is intentionally no PixelLab parent execution prompt. A caller resolves
one assetType and uses the child authoring/generation prompt pair; a generic
parent prompt would duplicate routing and blur single-owner execution.
