# ImageGen Animation Pipeline Guide

## Purpose

This current v2 pipeline owns prompt authoring and ImageGen generation for one
character or skill animation request. Generation creates one provider-native
animated GIF; preservation verifies that GIF and extracts its frames.
Evaluation is separate.

## Authority and Scope

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPreservationPackagingGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
```

Approved planning owns motion and exact final frames. The router owns request
splitting, the registry owns the exact character/skill row, this guide owns one
request's ImageGen translation/generation, and preservation owns GIF/frame
extraction. Missing meaning or technical contracts block.

## Contract

Read `GeneratedMediaImageGenOnlyContractGuide.md`. Require exactly one scalar
`animationRequestId`, hashed reference image, approved final frame count and
timing/order/loop/key poses, fixed cell, scale lock, vertical-motion policy,
background/no-shadow/outline, anchor, and `masterFirst=true`.

For a character request also require `referencePromptRecordPath`,
`referencePromptRecordSha256`, `expressionProfileKey`, and
`expressionProfilePayloadHash`. Read the immutable prompt-record bytes,
recompute their SHA-256, parse its closed `expressionProfilePayload`, canonicalize
it by the visual-guide rule, and recompute its payload hash. The record, handoff,
registry projection, and recomputed values must match exactly. Inherit the same
payload without editing, reordering, translating, or summarizing any lock. A
skill request must omit all four fields; their presence returns
`unexpected_character_style_reference`.

Authoring writes one `generated_media_prompt_v3`. For every new animation,
authoring requires `animationSourceMode=provider_native_animated_gif` and
`extractionMode=gif_timeline_exact`. Generation produces one playable animated
GIF at the final approved frame count and writes one
`generated_media_generation_v2` with
`structureProfile=animation_gif_frame_set_v2`. A still image, contact sheet,
sprite sheet, collage, video, or independently generated frame set is a fatal
source mismatch. Generation cannot oversample, choose a subset, merge
requests, extract frames, or package output.

The profile discriminator fixes the anchor: character animation uses
`pelvis_root_ground_axis`; skill animation uses `effect_origin`. Generation
uses the registered animation provider through
`providerInterface=configured_animated_gif_capability`. The execution role
first requires a zero-submit attestation for playable GIF output, exact
dimensions, final frame count, timing, loop, full-canvas disposal, and required
reference roles. Missing support returns
`animated_provider_capability_unavailable` with
`providerCalled=false` and `submitCount=0` before upload or submit. The role computes
and presents the contract 6.1 scope hash. Generation validates its closed
approval, tagged cost, cumulative attempts, projection, and checks an
animationRequestId-bearing idempotency key before billing. Identical completed
work is reused, active duplicate work blocks, and every decision records
`costEvidence`.

Character animation inherits the approved reference/main-image identity and
the exact discriminated expression payload. The two lock-array profiles inherit
their positive/negative locks; the sparse profile inherits its exact eight
policy members and uses empty compatibility lock arrays. Across every
frame it preserves line hierarchy, simplification level, controlled stroke
variation, face landmarks, costume layers, equipment and weapon structure.
Pose may change only through approved motion. Missing data uses
`missing_reference_prompt_record`, `missing_expression_profile_payload`,
`missing_expression_profile_key`, or `missing_expression_profile_payload_hash`.
File-byte, key, or payload-hash mismatch uses its corresponding typed blocker
from the current contract. `character_animation_style_lock_mismatch` remains an
aggregate consistency failure only after those exact checks pass. Skill
animation does not inherit the character style contract.

If the inherited key is
`projectbs_character_animation_ready_minimal_ink_line@1.0.0`, the inherited
payload includes its four closed proportion/detail/color/projection members.
Authoring preserves them byte-for-byte and projects their frame-reproducibility
locks into the animated-timeline prompt. Generation repeats the three
`character_generation_*_gate_failed` semantic checks before capability access
or submit; it rejects greater-than-4.25-head/naturalistic anatomy, dense
realistic detail, or nonminimal color/value treatment and never repairs the
reference prompt.

If the inherited key is
`projectbs_character_sparse_ink_pastel_motion@1.0.0`, authoring inherits the
same exact eight-member payload/hash while keeping request and structure
identity independent. Every frame projects 35-50 percent omission, 3-6 palette
accents, registered motion-line cues, and stable identity/action anchors.
Attack frames additionally project 3-5 faded-indigo sword/torso motion marks
and gray-brown shoulder/hem inertia. Generation rejects static repetition,
missing line/pigment motion cues, closed/filled treatment, or anchor drift with
the exact sparse generation tokens before provider access.
Sparse missing/mismatch/evidence/provider-projection failures use only the four
sparse authoring tokens; no `missing_*_style_lock` token applies.

For `projectbs_character_bold_outline_attack_motion_flow@1.0.0`, the reference
record remains bold v2 and immutable. Authoring verifies its exact bytes,
5702307b... base payload hash, 18px/8px projection, 64/56/5 ceilings, exact color
anchors, and closed halo before composing the separately registered successor.
It then requires all eight approved attack-motion bindings and projects
directional faded-indigo sword/torso brush flow, gray-brown shoulder/hem
inertia, bounded dark-neutral trajectory, ordered continuity, and identity plus
equipment anchor locks. Static repetition, generic clean-vector sheets,
arbitrary speed lines, magic VFX, or drift are fatal at authoring, generation
pre-submit, and evaluation. No failure may access the provider capability.

An exact current-user approval may instead select
`hosted_builtin_preview_v1` for exactly one scalar animationRequestId. This
isolated lane permits one submit and zero retries and ends at a non-evaluated,
non-promotable preview record; it cannot enter extraction, preservation, or
promotion. Promotable animation remains on the unchanged descriptor/approval/
cost generation-v2 contract and additionally requires the animated-GIF
capability attestation above. A hosted still-image preview never satisfies the
provider-native GIF source contract.

## Input, Output, State, and Validation

One scalar animationRequestId route/handoff becomes one prompt v3 record. One
ready prompt becomes one provider-native animated-GIF generation v2 record plus
preservation handoff. State is
`routed -> authored -> generated -> preservation_pending`.
Validate the exact ID/reference, final count/timing/order/loop/key poses,
GIF-timeline/scale/anchor policies, the registered animation provider, and
`animation_gif_frame_set_v2`. Character units also validate exact reference
prompt-record bytes, expression key/payload/hash, and the selected union branch:
direct lock inclusion/evidence for lock-array profiles or complete eight-member
sparse projection/evidence with empty lock arrays. Retry cannot change the motion contract. No
extraction, evaluation, promotion, or Git work occurs.

Generation blockers additionally include
`animated_provider_capability_unavailable` and the exact contract 6.1-6.2 approval,
scope, cost, attempt, duplicate-call, and provider-operation failure tokens.
Blocked output
includes the exact animationRequestId, providerCalled=false, costEvidence,
requiredDecision and safeToRetry; success includes record ID/hash, attempts,
refs, costEvidence, idempotencyKey, preservation handoff and nextStep.

## Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md
```
