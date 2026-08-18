# Character Animation Evaluation Guide

## 1. Purpose and Guide Type

```text
Guide Type: evaluation
Domain: character
Artifact Type: character_animation
Primary Consumer: generated-image evaluation task
```

This guide evaluates preserved character animation frames and sheets. It is
read-only: it does not generate, download, rename, convert, promote, import into
Unity, publish to Slack, or perform Git work.

## 2. Authority and References

Read:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/planning-guides/character/CharacterGenerateAnimation.md
```

Authority by concern:

- character source/planning owns identity, equipment, weapon, and intended action;
- preserved provider output and download manifest own evaluated bytes and frame order;
- this guide owns character-animation visual gates, score, severity, and verdict;
- the common evaluation pipeline owns immutable result schema and PASS-only
  promotion eligibility;
- Unity/runtime code owns importer, clip, direction enum, and binding behavior.

If identity, frame order, direction mapping, or runtime evidence conflicts, stop
with `evaluation_contract_conflict`; do not choose a convenient source.

### 2.1 Current package-mode adapter declaration

This declaration is additive and does not change the legacy
`artifactType=character_animation` route above.

```text
adapterId: character_animation_gif_frame_set_v2
assetType: animation
domainType: character
evaluationDomain: character
structureProfile: animation_gif_frame_set_v2
canonicalContentSourceRule: exact contentId and animationRequestId from the sealed package
artifactUsageRule: exact planned character animation action
planningEvidenceRule: exact package-bound planning/profile evidence; no pixel inference
stagingSourceRule: sealed generated_media_evaluation_package_v2 members only
projectTargetRule: informational promotion target only; never evaluation input
requiredEvidence: coherent master, completed GIF, reopened-timeline contiguous PNG frames, reference, hashes, order, timing, loop, key poses, anchor
domainFatalGates: package/identity/structure gates plus Section 4 and applicable profile fatal gates
scoreCategories: anim.frame_continuity_body_integrity=30; anim.identity_equipment_weapon=25; anim.direction_spatial_stability=20; anim.action_readability=15; anim.timing_loop_ending=10
passThreshold: totalScore >= 90 with no fatal gate and all planned action/direction evidence inspected
categoryMinimums: none beyond valid 0..maximum scores; do not invent minima
domainNativeResults: Pass | Conditional Pass | Fail | not_evaluated
resultNormalization: Pass->PASS; Conditional Pass->CONDITIONAL_PASS; Fail->FAIL; not_evaluated->not_evaluated
domainSpecificNotes fields: animationRequestId, observedAction, observedDirections, frameRanges, anchorObservations
mediaEvidenceRule: GIF for motion only; reopened-timeline PNG frames for technical and per-frame findings
reEvaluationRule: Section 10 with exact unchanged package/member-hash evidence
```

The current adapter preserves `assetType=animation`, `domainType=character`,
`structureProfile=animation_gif_frame_set_v2`, evaluationPackageId, requestId,
contentId, animationRequestId, and member hashes through the immutable result.
It MUST NOT emit or infer legacy `artifactType=character_animation`.

For the current package branch, the sealed strict-generation or accepted-result
capture/preservation chain replaces the legacy Section 3 `generationRecord` and
`downloadRecord` inputs. An accepted-result package never requires a fake
prompt, generation, or download record. The package-bound reference and planned
action/direction evidence replace `referenceRotationPath` and other legacy path
inputs; missing package evidence remains `insufficient_evidence`.

For an exact six-frame package whose approved timing intent is uniform 8 fps,
GIF centisecond quantization is valid only with the sealed
`generated_media_gif_8fps_centisecond_quantization_receipt_v1`. The ordered
delay schedule must be `[12,13,12,13,12,13]` centiseconds (120/130 ms
alternating), total 750 ms, no zero-delay frame, and no loop extension. Six
frames over 750 ms is exact average 8 fps, so this schedule satisfies the
uniform timing intent and is not penalized as arbitrary mixed timing. Reopened
before/after decoded frame pixel hashes, 640x512 canvas, frame count/order,
pelvis/baseline and clipping/fragment state must be unchanged. Every other
mixed schedule, frame count/FPS, total, loop representation, or pixel/canvas
change remains `gif_timeline_contract_mismatch`. Legacy and provider-native
timing rules are unchanged.

For accepted GIF SHA
`8a924fdee81d01d8d8f94d742ec0755f7f7856718f16e60839affc6c9ee3e621`,
the sealed package may additionally contain
`generated_media_gif_observed_boundary_chroma_receipt_v2`. Accept transparency
only when all six frames have exact `(240,236,228)` at all 2,300 outer-boundary
pixels and four corners, the receipt proves 4-connected exact-match removal
only, and all nonmatching pixels plus geometry/order/pelvis/baseline/clipping/
fragment state remain unchanged. The same receipt must retain the canonical
`[12,13,12,13,12,13]` schedule, 750 ms total and one-shot/no-loop metadata.
No `#F2EFE6` substitution, inferred color, threshold, or other source is valid.
Receipt/source/mask/frame drift is
`evaluation_package_gif_boundary_normalization_mismatch`; existing animation
and provider-native rules are unchanged.

## 3. Inputs and Preconditions

Required:

```text
characterId
characterSource
generationRecord
downloadRecord
stagingAnimationPath or orderedFrameSetPath
referenceRotationPath
expectedAnimationType: Move | Attack | Idle
expectedDirections
expectedFrameCount
```

Optional evidence:

```text
weaponDefinition
lowerGradeOrSiblingReference
unityMetaEvidence
runtimeClipEvidence
```

The staged artifact must be a preserved evaluation copy and must differ from
the project target. Missing required frames, identity, or reference rotation is
`insufficient_evidence`, not an inferred pass.

## 4. Hard Fail Gates

Any gate below is Critical and forces `Fail` regardless of score:

- unreadable, missing, empty, or structurally incomplete frame set;
- wrong character, animation type, or material direction;
- character body, face, clothing, equipment, or held weapon is replaced or
  severely corrupted between frames;
- required body parts or the primary weapon disappear without intentional
  occlusion;
- severe axis jump, frame-size mismatch, crop, or edge contact makes the
  animation unusable;
- extra character, duplicated weapon, unrelated object, text, UI, logo, or
  watermark appears;
- foreign cultural motif violates the mandatory master concept;
- claimed frame order or direction cannot be reconciled with preserved evidence.

## 5. Scoring

Score five fixed categories. Maximums total 100.

| Category | Max | Observable requirement |
| --- | ---: | --- |
| Frame Continuity & Body Integrity | 30 | Motion connects naturally; body and joints remain complete and stable. |
| Identity, Equipment & Weapon Consistency | 25 | Appearance, clothing, equipment, grip, weapon shape/color/hand remain consistent. |
| Direction & Spatial Stability | 20 | Motion faces the required direction and maintains a usable center/ground axis. |
| Animation-Type Readability | 15 | Move, Attack, or Idle action is immediately distinguishable and appropriate. |
| Timing, Loop & Ending | 10 | Anticipation, action, recovery and loop/ending behavior are coherent. |

Every category receives a score from 0 to its maximum. `totalScore` is the
arithmetic sum and must be 0–100.

## 6. N/A and Insufficient Evidence

- Scored categories are never N/A for a completed evaluation.
- Weapon-specific subchecks may be N/A only when canonical character evidence
  confirms no weapon; the 25-point category still scores identity and equipment.
- Unity meta or runtime clip checks may be N/A because they are supporting
  readiness evidence and carry no visual score.
- If a scored category cannot be observed, use `not_evaluated` with
  `insufficient_evidence`; do not calculate totalScore or a passing verdict.

## 7. Verdict

```text
Pass: totalScore >= 90, no Hard Fail, all required directions/types inspected
Conditional Pass: totalScore 80-89.99, no Hard Fail, correction requires no provider regeneration
Fail: totalScore < 80 or any Hard Fail
not_evaluated: required evidence is missing or conflicting
```

Only Pass may set `passForProjectCopy=true`. Conditional Pass requires correction
and re-evaluation, not approval-only promotion.

## 8. Severity

```text
Critical: Hard Fail, wrong identity/action/direction, unusable structure, or unsafe contract conflict
Major: issue that prevents Pass or requires regeneration/material frame correction
Minor: localized correctable defect that does not independently prevent Pass
Suggestion: optional polish with no verdict impact
```

Each finding records severity, animation type, direction, frame range, evidence,
impact, and required correction. Severity does not replace numeric scoring.

## 9. Output and Handoff

The immutable evaluation result records artifact/evidence identity and SHA-256,
observed types/directions/frame order, Hard Fail gates, five category scores,
totalScore, verdict, passForProjectCopy, findings, actions, improvements, and a
reEvaluationPlan. Return it to the common evaluation pipeline. Do not copy
project files or create Unity metadata during evaluation.

## 10. Re-evaluation

- Preserve the previous result and identify the replacement artifact/hash.
- Re-run all Hard Fail gates.
- Re-score every category affected by changed frames, timing, identity, weapon,
  direction, or structure.
- Carry forward an unaffected category only with unchanged-byte evidence and an
  explicit provenance link.
- A previous Fail or Conditional Pass never becomes Pass without a new complete
  verdict calculation.

## 11. Failure Types

```text
missing_character_source
missing_generation_record
missing_download_record
missing_reference_rotation
missing_animation_frames
invalid_frame_structure
identity_mismatch
direction_mapping_conflict
insufficient_evidence
evaluation_contract_conflict
evaluation_result_write_failed
```

## 12. Validation Checklist

- [ ] Category maximums total 100 and totalScore equals their sum.
- [ ] Hard Fail precedes numeric verdict.
- [ ] N/A is used only for allowed non-scored evidence or weapon subchecks.
- [ ] Every finding has severity and exact frame evidence.
- [ ] Only Pass enables project-copy handoff.
- [ ] No source/project/Unity/Slack/Git mutation occurred.
