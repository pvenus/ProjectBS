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

### Accepted-result coherent-master-to-GIF guidance

The published `accepted_result_attack_gif_guidance_v1` is read-only historical
contract evidence and MUST NOT authorize an accepted-result execution: it
incorrectly described the provider output as a final GIF. The existing
`provider_native_animated_gif` mode above remains separate and unchanged.

Future use of the accepted workflow requires the distinct optional
`accepted_result_attack_coherent_master_to_gif_guidance_v2`. It is guidance for
the same character attack-animation role only, not a planning fact, routing
decision, prompt-record amendment, generation record, or preservation/
evaluation authority. The accepted completed GIF remains evidence-only:
SHA-256 `8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621`,
`640x512`, six frames. Its path is not canonical and is never transferred.

This accepted mode has
`animationSourceMode=generation_role_coherent_six_cell_master_to_gif` and
`extractionMode=generation_completed_gif_timeline_exact`. The provider returns
one coherent six-cell master IMAGE and did not return a GIF. The same official
generation role owns this exact order:

```text
receive and hash one coherent six-cell master image
-> validate exactly six ordered cells and one coherent action
-> construct the completed GIF first in approved order/timing/palette/background
-> close the completed GIF
-> reopen that completed GIF
-> extract exactly six ordered PNG frames from the reopened GIF timeline
-> apply only approved deterministic final-packaging normalization
-> validate GIF and six PNG members as one package
```

Transient cell decoding needed to encode the GIF is not a published frame set.
No PNG frame is a final member until extracted from the reopened completed GIF.
If final packaging translates pixels or removes a verified fragment, it applies
the same deterministic change to the GIF timeline, closes/reopens the changed
GIF, and re-extracts all six PNGs so the GIF remains first and authoritative.

Final packaging may translate frames only by the measured integer delta needed
to hold the approved pelvis center and ground baseline. It may remove only a
connected edge component proven to originate in a neighboring master cell; it
must not erase the subject, weapon, motion effect, or legitimate clipping.
The longest clean left/right margin across legitimate action defines one shared
width basis. Final validation requires pelvis drift `0px`, baseline drift
`0px`, identical scale/timing/global palette/fully opaque background, no
clipping, and no neighboring-cell edge fragments.

The detached guidance contains exactly `schemaVersion`, `animationRequestId`,
`acceptedResultEvidenceSha256`, `acceptedResultWidth`,
`acceptedResultHeight`, `acceptedResultFrameCount`, `animationSourceMode`,
`extractionMode`, `providerMasterLayout`, `pelvisCenter`, `groundBaselineY`,
`sharedWidthBasisPolicy`, `scalePolicy`, `timingPolicy`, `palettePolicy`, and
`backgroundPolicy`. Unknown, missing, inferred, or provider-native-GIF values
are invalid. `acceptedResultEvidenceSha256` is guidance provenance only.

Generation returns one compact
`generated_media_attack_coherent_master_to_gif_validation_receipt_v2` with
exactly `schemaVersion`, `animationRequestId`, `providerDidReturnGif`,
`providerMasterImageSha256`, `providerMasterCellCount`, `completedGifSha256`,
`width`, `height`, `frameCount`, `extractedPngFrameSha256s`,
`sharedWidthBasis`, `pelvisDriftMaxPx`, `baselineDriftMaxPx`, `scaleUniform`,
`timingUniform`, `globalPaletteUniform`, `backgroundFullyOpaque`,
`clippingDetected`, `neighboringFragmentsDetected`, `gifClosedAndReopened`,
`pngsExtractedFromReopenedGif`, and `status`. `status=valid` requires
`providerDidReturnGif=false`, six master cells, six PNG hashes, both workflow
booleans true, both drift values zero, all four uniform/opaque booleans true,
both detected booleans false, and exact evidence dimensions/frame count. A GIF
returned by the provider in this accepted mode is `invalid_animation_source_mode`.
The receipt is not a media evaluation verdict and is not persisted upstream.

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

### Registered open-ink opaque-chroma 3x2 branch

The additive profile
`projectbs_character_open_ink_wash_animation_opaque_chroma_master@1.0.0` /
`da38a4c91bbe3a808f09f1c24763cd3cece02518a2d1398f7294ce3eedb3f7c8`
uses execution profile
`projectbs_character_open_ink_animation_opaque_chroma_identity_anchored@1.0.0`.
It is the sole current exception to the provider-native GIF requirement for a
new promotable `character_animation_v2` request. Its exact source mode is
`provider_opaque_chroma_3x2_master`; extraction mode is
`postprocess_exact_cell_chroma_root_gif_v1`. All existing modes stay unchanged.

Authoring projects the closed identity/equipment MAIN selection and
topology-only Grade1 motion selection, then asks for one RGB opaque 1536x1024
PNG containing six distinct ordered 512x512 cells in a 3x2 grid on one
connected uniform #00FF00 carrier. The call contains stored prompt plus one
MAIN `referenced_image_paths` entry; motion-lineage media is never referenced.
Generation uses one submit, retry 0, observes every master gate, and stops at
`generated_media_animation_postprocess`.

That distinct role performs exact seam-aware row-major split, exact-key chroma
uncomposite, root `(256,300)` and baseline `448` integer translation, fixed
scale/camera/centroid, safe margin 48, six true-alpha PNGs and one reopened GIF
at 150 ms x6, infinite loop, total 900 ms. Duplicate/repeated phases,
whole-body mirror, frame 5-to-0 closure failure, drift, clipping, fringe, and
fragments block. Generation performs none of those operations; project copy
eligibility remains false until independent evaluation PASS.

## Task Prompts

```text
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationPromptAuthoringPrompt.md
AgentDocs/task-prompts/content/generated-media/ImageGenAnimationGenerationPrompt.md
```
