# Generated Media Character Expression Evaluation Guide

## Purpose and Scope

This evaluation guide applies when a character main image declares
`projectbs_character_sparse_ink_pastel_motion@1.0.0` or
`projectbs_character_bold_outline_compressed_detail@1.0.0`, and to character
animation only for the sparse profile because the bold profile is single-image-only. It evaluates preserved
staging media read-only after generation; it does not generate, edit, preserve,
promote, copy to Unity, or perform Git work.

## Authority and Inputs

Read, in order:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
```

Require a valid evaluation package, immutable planning snapshot/hash, exact
expression profile key/payload/hash, prompt record and hashes, preserved media
hashes, and either `single_image` or the ordered animation set whose count
equals the package/animationRequest approved `finalFrameCount`.
Missing, stale, or mismatched evidence blocks before scoring.
Use `missing_character_expression_evaluation_package`,
`character_evaluation_profile_mismatch`,
`character_evaluation_frame_count_mismatch`, or
`character_evaluation_evidence_insufficient` for these pre-gate failures.

## Pre-score Fatal Gates

Any observed gate below is `FAIL` regardless of numeric score:

Apply only the rows owned by the exact selected profile. Sparse rows never
evaluate a bold-profile artifact, and bold rows never reinterpret sparse
budgets.

| failureType | observable condition |
| --- | --- |
| `character_evaluation_proportion_gate_failed` | 7-8-head naturalistic/adult proportion |
| `character_evaluation_sparse_contour_gate_failed` | closed coloring-book contour or fully inked silhouette |
| `character_evaluation_sparse_omission_budget_gate_failed` | main omission outside 35-45 percent or any approved animation frame outside 35-50 percent |
| `character_evaluation_sparse_pigment_budget_gate_failed` | main accents outside 4-7, animation-frame accents outside 3-6, opaque/cel fill, off-palette hue, or main pigment area above 18 percent |
| `character_evaluation_sparse_motion_gate_failed` | static repeated action frames or missing line/pigment motion cues |
| `character_evaluation_identity_anchor_gate_failed` | gaze, topknot, hand/sword grip, support foot, or action-joint identity drift |
| `character_evaluation_bold_outline_proportion_gate_failed` | full body outside 4.0-5.0 heads, or naturalistic 6.5-8 heads, long limbs, or heroic tall anatomy |
| `character_evaluation_bold_outline_hierarchy_gate_failed` | outside silhouette is not visibly bold/dark, is not materially thicker than internal lines, or pigment weakens/erases it |
| `character_evaluation_bold_outline_facial_mark_budget_gate_failed` | total facial marks exceed 9, component maxima exceed 4/1/1/3, or realistic facial modeling appears |
| `character_evaluation_bold_outline_detail_budget_gate_failed` | dense folds, more than three secondary fold marks in a garment region, individual scales/rivets, microtexture, hatching, modeled shading, or realistic material treatment appears |
| `character_evaluation_bold_outline_color_signature_gate_failed` | hue appears outside approved anchors, secondary hue/anchors disagree, coverage exceeds 35 percent, color masses exceed 4, neutral outline/weapon colors drift, full-garment fill appears, or color overrides line hierarchy |

The evaluator uses the authored profile projection and an observable checklist.
It may use a reproducible pixel-area measurement when available, but must record
the method and evidence. If measurement is unstable or unavailable, it must not
invent computer-vision precision: clearly visible opaque/cel fill is fatal;
uncertain area is a blocker requiring reviewed evidence, not a guessed pass.

## Observable Checklist

For a main image, confirm 3.75-4.25 heads, short/simple limbs, 35-45 percent
intentional contour/internal-boundary omission, no-fill negative space, no more
than 18 percent pigmented area, 4-7 pigment accents, exact faded indigo/navy and
dusty ochre/gray-brown palette, subordinate loose bloom/rub/dragged strokes,
and darkest lines restricted to identity/action anchors.

For each ordered animation frame in the approved `finalFrameCount`, confirm 35-50 percent
omission, 3-6 accents, the same palette and payload hash, stable identity
anchors, and observable motion through searching overlap, taper-break,
robe/sleeve lag, sword arc, or overshoot/smear. Attack frames use 3-5 indigo
sword/torso marks and gray-brown shoulder/hem inertia where the approved motion
calls for them. Active frames may reduce face/costume detail.

For a bold-outline main image, confirm 4.0-5.0 heads with compact shortened
limbs; compare the outside silhouette against the authored 16-22 source-pixel
binding and exact internal-line binding after accounting for the sealed media
scale; require outside/internal ratio at least 2 and a continuous readable bold
silhouette. Count facial marks with the profile rule: one continuous visible
mark between pen lifts or intentional breaks is one mark. Confirm total and
component maxima, identity-first compressed detail, primary/optional-secondary
hue anchor sites, coverage/mass limits, and neutral outline/weapon colors.
If the preserved media scale cannot support reproducible thickness comparison,
or coverage/mark evidence is ambiguous, return the evidence blocker rather than
approximating a pass.

## Result Contract

Return `PASS`, `FAIL`, or `BLOCKED`, the exact profile key/hash, artifact and
frame identities, every fatal gate result, evidence references, findings, and
required actions. A fatal failure prevents scoring. A nonfatal result may use
the common evaluation package score model, but no score overrides a fatal gate.
The evaluator never changes the media or declares it promotable. Input media
and evidence remain at their ContentFolderStructureGuide-governed staging
paths. This guide defines no new storage path and writes no project artifact;
the caller may persist the returned report only through the common evaluation
package/report contract.

## Golden Validation Fixtures

- Main pass: all main budgets, palette, no-fill, proportions, and anchors pass.
- Main fail: separately exercise 7-8 heads, closed contour, cel fill, pigment
  area over 18 percent, and an off-palette hue.
- Six-frame golden pass: ordered frames vary action, retain anchors, satisfy per-frame
  omission/accent budgets, and expose both line and pigment motion cues.
- Six-frame golden fail: separately exercise static repetition, missing motion cues,
  missing line/pigment evidence, identity-anchor drift, and per-frame budget
  violations.
- Bold-outline main pass: 4.5 heads, exact 18px outside/8px internal source
  bindings, nine-or-fewer component-valid face marks, compressed detail, and
  valid anchored color signature.
- Bold-outline main fail: separately exercise 6.5-head anatomy, outside/internal
  ratio below 2, tenth facial mark, dense scales/folds, unanchored secondary
  hue, coverage above 35 percent, and a fifth color mass.

The six-frame count belongs only to these golden fixtures. Operational
evaluation accepts any positive approved `finalFrameCount` and requires the
ordered member count to equal it exactly. Contract tests must keep golden
fixtures deterministic and must not claim to perform image recognition.
