# Generated Media Character Expression Evaluation Guide

## Purpose and Scope

This evaluation guide applies when a character main image declares
`projectbs_character_sparse_ink_pastel_motion@1.0.0` or
`projectbs_character_bold_outline_compressed_detail@1.0.0` or
`projectbs_character_bold_outline_compressed_detail@2.0.0` or
`projectbs_character_open_ink_wash_dynamic_contour@1.0.0` or
`projectbs_character_open_ink_wash_dynamic_contour@2.0.0`, and to character
animation for the sparse profile or the separately registered
`projectbs_character_bold_outline_attack_motion_flow@1.0.0` composed successor;
the two base bold profiles remain single-image-only. It evaluates preserved
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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md
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

If the prompt/evaluation lineage contains `role=style_only`, the evaluator
rehashes the exact six-member binding, durable asset, immutable review record,
and review index before inspecting the media. The reference is usable only for
the review record's allowed style/composition observations. It is never
identity truth, a person/pose/action/clothing/equipment source, or an edit
target. Missing or divergent binding evidence uses the shared
`style_reference_*` blocker from the ImageGen-only contract before scoring;
semantic transfer is the fatal reference-role failure below.

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
| `character_evaluation_bold_outline_v2_detail_budget_gate_failed` | v2 total visible marks exceed 64, internal marks exceed 56 or total, folds exceed 5 in a garment region, or hatching, microtexture, modeled/realistic materials, dense folds/scales/rivets appear |
| `character_evaluation_bold_outline_v2_color_anchor_gate_failed` | v2 ochre appears outside approved small utility-pouch/travel-accessory sites, or color coverage/masses/full-fill/hierarchy exceed the closed contract |
| `character_evaluation_bold_outline_v2_halo_gate_failed` | disabled authorizes a dark background, or enabled halo exceeds opacity 0.35/coverage 45, is opaque/scenic/noncentered/nonfading, has nonzero edge alpha, or acts as a shadow/directional cast shadow |
| `character_evaluation_bold_outline_motion_flow_gate_failed` | missing/wrong-direction faded-indigo sword/torso 3-5 brush flow, missing gray-brown shoulder/hem inertia, missing bounded dark-neutral trajectory, static repetition, generic clean-vector sheet, arbitrary speed lines, or magic VFX |
| `character_evaluation_bold_outline_motion_continuity_gate_failed` | key-pose order does not evolve continuously or fixed cell/scale/root anchor continuity breaks |
| `character_evaluation_bold_outline_motion_identity_equipment_gate_failed` | any successor identity or equipment anchor drifts across frames |
| `character_evaluation_open_ink_wash_proportion_age_gate_failed` | full body is outside 4-5 heads, does not preserve the 4.25 target, or reads as child/minor rather than young adult |
| `character_evaluation_open_ink_wash_contour_mok_seon_gate_failed` | omission is outside 35-55 percent or lacks target-45 intent; contour is sticker-clean/uniform/vector-clean; or brush start, directional drag, dry end, pressure variation, or directional weight is absent |
| `character_evaluation_open_ink_wash_pigment_negative_space_gate_failed` | broad rough watercolor/pastel, controlled bleed/misalignment, separate three-role palette, or either 70-percent achromatic/unpainted floor is absent; or cel fill/decorative small splashes appear |
| `character_evaluation_open_ink_wash_background_gate_failed` | generation/final evidence shows a non-removable non-warm-ivory generation background, halo, vignette, scene, or shadow |
| `character_evaluation_open_ink_wash_identity_equipment_gate_failed` | approved young-adult Korean/Joseon identity, costume, equipment, weapon, handedness, or identifying anchors drift or disappear |
| `character_evaluation_open_ink_wash_reference_role_gate_failed` | the accepted raster was used as canonical identity, person/pose/action/clothing/equipment source, or edit target; or provider binding occurred without an exact approved durable project-relative `style_only` review binding |
| `character_evaluation_open_ink_wash_v2_surface_detail_gate_failed` | v2 output materially diverges through a realistically modeled face; dense, individually enumerable armor plates/scales/rivets/lacing/fasteners; garment microfolds or microtexture; modeled light/material; or surface construction that conflicts with planning, identity, equipment, proportion, or silhouette. The bounded main-image readability variance below is not this fatal token. |

The evaluator uses the authored profile projection and an observable checklist.
It may use a reproducible pixel-area measurement when available, but must record
the method and evidence. If measurement is unstable or unavailable, it must not
invent computer-vision precision: clearly visible opaque/cel fill is fatal;
uncertain area is a blocker requiring reviewed evidence, not a guessed pass.
For v2, the same rule applies to visible/internal/fold counts, ochre anchor-site
class, halo opacity/coverage/center/falloff/edge alpha, and scene/shadow status.
For open ink-wash v2, the same rule applies to the full-body head-count method,
surface-detail categories, spatial background uniformity, and every result in
the prior compact conformance receipt. The receipt may focus review but cannot
replace evaluation-package/media evidence or turn a preview into an evaluation
input. Insufficient reproducible evidence returns
`character_evaluation_evidence_insufficient`, never a guessed acceptance.

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

For an open ink-wash main image, confirm 4-5 heads targeted at 4.25 and clearly
young-adult presentation; 35-55 percent open contour targeted at 45; observable
pressure-variable mok-seon with brush-start, directional-drag, and dry-end
phases; broad rough bleeding/misaligned watercolor-pastel masses; separate
faded-blue-gray-or-indigo, dusty-gray-brown, and small-muted-ochre roles; at
least 70 percent achromatic/unpainted space in both figure interior and full
canvas; removable warm-ivory generation background; and no halo, vignette,
scene, or shadow. Confirm all planning-bound Korean/Joseon identity/equipment
anchors and audit-only reference role. Unreproducible percentages or stroke
phases block as evidence insufficient rather than passing by impression.

For the v2 successor, additionally confirm that the face, armor, and garments
remain broad and sparse rather than individually modeled, and that the full
canvas is uniform `#F2EFE6` without radial gradient or edge darkening. Apply
the v2 surface-detail fatal token independently from the existing background
and proportion gates.

For `character_main_image` only, classify minor expressive surface variance
before applying that fatal token. It is PASS-compatible only when every item is
true: marks are low-contrast relative to silhouette/identity strokes;
low-density and visually subordinate; flat with no highlight, cast shadow,
volume, gradient, or material response; contained inside one existing broad
shoulder-armor mass or expressed as a limited set of bands inside the existing
leg-wrap regions; not readable as an inventory of separate plates, scales,
rivets, lacing, fasteners, or constructed layers; and they change no identity,
equipment, proportion, silhouette, pose/action, background, or clipping state.
The evaluator records `minor_expressive_surface_variance` as a bounded score
deduction/finding or optional improvement with `regenerationRequired=false`.
It does not create a required action and cannot by itself make the result FAIL.

The tolerance is unavailable when marks tile or enumerate the shoulder mass,
form repeated plate boundaries, introduce rivets/lacing/fasteners, model
realistic armor or cloth construction, create dense leg wrapping, add
microtexture/microfolds, use modeled lighting/material, or conflict with any
planning/identity/equipment/proportion/silhouette gate. Those observations keep
`character_evaluation_open_ink_wash_v2_surface_detail_gate_failed`. The rule
does not apply to animation, other expression profiles, other adapters, or any
other fatal gate. In particular it never excuses identity/equipment drift,
wrong proportions, silhouette/action/background conflict, clipping, or a broad
style-direction conflict.

### Open ink-wash v2 hard-fail and soft-quality boundary

For the exact open ink-wash v2 main-image branch, a pre-score hard fail is
limited to a material identity or project-usability defect: wrong person or
materially wrong gender presentation; materially wrong child/minor presentation
for the required adult; major species/body/face/costume/weapon/equipment or
handedness substitution/disappearance; semantically wrong action/direction;
corrupt or undecodable media; missing required member; severe clipping; broken
alpha/background; unstable canvas/anchor; extreme proportion, silhouette, or
style divergence that reads as another design; or visible text, watermark, UI,
or another project-blocking artifact. These defects override every score.

Line finish, brush roughness, contour-omission degree, low-density stylized
armor marks/scales, wrap bands, fold counts, pigment bleed/misalignment nuance,
palette balance, negative-space nuance, modest surface-detail density, polish,
impact/readability, aesthetic preference, and minor proportion deviation are
soft quality observations whenever the same planned adult character remains
recognizable and usable with stable identity/equipment/silhouette. They may
reduce score and create Major, Minor, or Suggestion findings, but never trigger
a fatal token by themselves. Thus the open-ink contour, pigment, background,
proportion, and surface-detail fatal tokens apply only when their observation
crosses the material identity/usability boundary above. Technical integrity,
identity, required-member completeness, and project usability are not softened.

An accepted corrective package may present a transparent final PNG produced by
`border_exact_checkerboard_boundary_flood_v1`. Accept that final-background
transition only after the package-level receipt proves exact RGB preservation,
boundary-only 4-connected removal, unchanged dimensions, real alpha, and no
ambiguous foreground contact. The evaluator still applies every open-ink
proportion, detail, contour, palette, negative-space, identity/equipment and
background gate to the normalized primary pixels. A receipt never excuses
foreground loss, halo/vignette residue, or surface-detail failure.

The same evaluation source may use
`generated_media_border_palette_checkerboard_alpha_receipt_v2` only for exact
source SHA `4498817999fb28323eb85f62afefcc33027341640b1a7ce990a3609b32eaeb7e`.
Before scoring, verify the published 64-color palette/boundary/periodic fixture,
exact 4-connected mask and normalized RGBA pixel hashes, equal protected
noncandidate RGB hashes/bboxes, unchanged dimensions and no boundary foreground
contact. This evidence-bound exception never authorizes thresholding, semantic
masking, ink/wash erosion, or another source. All existing expression and
background gates still apply to the normalized primary.

## Result Contract

Return `PASS`, `FAIL`, or `BLOCKED`, the exact profile key/hash, artifact and
frame identities, every fatal gate result, evidence references, findings, and
required actions. The open ink-wash v2 main-image branch retains an observable
100-point score: `PASS` requires total score >=80 and no hard fail; `FAIL`
requires total score <80 or any hard fail. It has no soft-category fatal minimum
and no `CONDITIONAL_PASS`; category evidence and deductions remain explicit.
PASS alone sets `passForProjectCopy=true`. Other profiles keep their registered
score models. No score overrides a hard fail.
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
- Open ink-wash main pass: 4.25-head young adult, 45-percent open contour,
  complete mok-seon phases, broad bleeding pigment, separate palette roles, both
  70-percent negative-space floors, warm-ivory removable background, no scenic
  treatment, stable identity/equipment, and either semantic-only audit evidence
  or an exact approved durable `style_only` binding whose prohibited semantic
  transfers remain absent.
- Open ink-wash main fail: separately exercise child coding, omission 34/56,
  uniform vector contour, missing stroke phase, clean cel fill, decorative
  splashes, collapsed palette roles, either negative-space floor below 70,
  halo/vignette/scene/shadow, identity/equipment drift, and reference-role misuse.
- Open ink-wash v2 main PASS-compatible variance: low-contrast, low-density,
  flat scale suggestions contained in one broad shoulder mass and limited flat
  leg-wrap bands, with no enumeration, construction modeling, silhouette or
  identity drift. Record only the bounded finding; do not require regeneration.
- Open ink-wash v2 main fail: additionally exercise 7-head anatomy, a realistic
  modeled face, dense separately readable armor plates/scales/rivets/lacing,
  realistic construction/material, garment microfolds, microtexture/modeled
  light, radial dark halo, edge vignette, and insufficient measurement evidence.
  Also prove that permitted minor marks cannot excuse equipment, silhouette,
  action, background, clipping, or style-direction conflict. The compact
  preview receipt never substitutes for the evaluation package.

The six-frame count belongs only to these golden fixtures. Operational
evaluation accepts any positive approved `finalFrameCount` and requires the
ordered member count to equal it exactly. Contract tests must keep golden
fixtures deterministic and must not claim to perform image recognition.
