# Generated Image Evaluation Pipeline Guide

## Master Concept Reference

Before using this document, read and apply:

~~~text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
~~~

## 1. Purpose

This guide defines the common evaluation pipeline for generated visual
artifacts. It combines:

~~~text
common evaluation contract
+ artifact-structure profile
+ one exact content-domain evaluation adapter
-> one normalized evaluation_result.json
~~~

The normalized result contains every fact required by the common Slack Canvas
evaluation form. Slack publication is a later formatting task and must not
re-score or reinterpret the result.

This pipeline evaluates an artifact already generated, downloaded, and
preserved in the current PC's established local evaluation workspace. It does
not generate, download, edit, promote, copy to the project, write Slack, perform
Git work, or deploy.

## 2. Required References and Authority

Always read:

~~~text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/prompt/EvaluationSlackCanvasFormGuide.md
~~~

Then read exactly one routed domain evaluation adapter from Section 5.

Authority is separated:

1. ContentFolderStructureGuide.md owns project storage and path separation.
2. This guide owns evaluation lifecycle, structure profiles, common gates,
   normalized result fields, and extension rules.
3. The routed domain guide owns visual meaning, domain fatal gates, score
   categories, maximums, thresholds, category minimums, and domain evidence.
4. EvaluationSlackCanvasFormGuide.md owns archival field semantics and Canvas
   presentation. It does not own scoring.

Do not merge conflicting rules by judgment. Stop with
evaluation_contract_conflict and identify both sources. A Slack formatting
guide must never replace a domain evaluation rubric.

## 3. Strict Task Boundary

The evaluation task may:

- resolve the repository and established local evaluation workspace;
- locate the exact preserved source and completed generation/download records;
- construct and lock an evidence-based evaluation brief;
- derive non-destructive previews, decoded frames, contact sheets, and playback
  aids inside the evaluation evidence folder;
- execute common, structure, and domain evaluation checks;
- write evaluation_input.json, evaluation_result.json, evaluation_report.md,
  and evaluation evidence;
- validate that the report and structured result agree.

The evaluation task must not:

- create, regenerate, download, replace, crop, resize, pad, recolor, recompress,
  rename, normalize, or otherwise repair the preserved source;
- choose a different generation candidate;
- copy anything into Assets/ImagesGenerated or Assets/Resources;
- create or modify Unity .meta, SO, animation clip, or runtime binding;
- publish or edit Slack Canvas;
- perform Git, merge, deployment, or content build;
- convert missing evidence into a visual FAIL guess or an inferred PASS.

## 4. External Request Contract

One request evaluates one logical artifact or one domain-defined artifact set.
New generated-media callers provide a sealed evaluation package identity;
legacy callers may provide the earlier artifact identity contract.

### 4.1 Allowed input

~~~text
requestId: optional stable request id
evaluationPackageId: preferred stable generated_media_evaluation_package_v2 ID
assetType: required with package mode
domainType: required with package mode
artifactType: legacy compatibility identity
contentId: required canonical content id
sourceRecordId: optional stable non-path generation/download record id
workflowMode: evaluate_new | re_evaluate
priorEvaluationRecordId: optional stable non-path record id for re_evaluate
~~~

### 4.2 Internally resolved fields

The caller must not need to provide:

~~~text
repositoryRoot
planningSourcePath
evaluationRoot or evaluationWorkspacePath
stagingArtifactPath
evaluationReportPath or result path
projectTargetPath
filename or extension
ContentDomain
structureProfile
domainGuidePath
score categories, threshold, or fatal gates
preview, frame, contact sheet, or playback paths
Slack destination
generation provider or tool URL
~~~

Path-like fields supplied externally are untrusted hints. Resolve current-PC
paths and rules internally. Never reuse an absolute path from another PC.

## 5. Artifact Adapter Registry

| Request identity | Domain | Structure profile | Primary domain evaluation adapter | Readiness |
| --- | --- | --- | --- | --- |
| skill_icon | skill | single_image | AgentDocs/planning-guides/skill/SkillIconEvaluationGuide.md | ready |
| item_icon | item | single_image | AgentDocs/planning-guides/item/ItemIconGenerationGuide.md evaluation sections | ready, temporary adapter |
| story_popup_main_image | stage | single_image | AgentDocs/planning-guides/stage/PopupEventMainImageEvaluationGuide.md | ready |
| skill_animation | skill | paired_sheet_animation | AgentDocs/planning-guides/skill/SkillImageEvaluationGuide.md | ready |
| character_animation | character | ordered_frame_set | AgentDocs/planning-guides/character/EvaluationAnimationGuide.md | ready with all required subsection thresholds |
| character_main_image or legacy character_image | character | ordered_rotation_set | AgentDocs/planning-guides/character/CharacterGenerateImage.md evaluation criteria | ready, temporary adapter |
| icon + domainType=skill | skill | single_image | AgentDocs/planning-guides/skill/SkillIconEvaluationGuide.md | ready |
| icon + domainType=item | item | single_image | AgentDocs/planning-guides/item/ItemIconGenerationGuide.md evaluation sections | ready, temporary adapter |
| general_animation + domainType=skill | skill | paired_sheet_animation | AgentDocs/planning-guides/skill/SkillImageEvaluationGuide.md | ready |
| imagegen_image + domainType=stage | stage | single_image | AgentDocs/planning-guides/stage/PopupEventMainImageEvaluationGuide.md | ready |
| imagegen_image + domainType=battle | battle | single_image | dedicated adapter not yet defined | blocked |
| battle_background | battle | single_image | dedicated adapter not yet defined | blocked |
| background_single_image + domainType=stage | stage | background_single_image_v2 | AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md current background adapter | ready |
| background_single_image + domainType=battle | battle | background_single_image_v2 | AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md current background adapter | ready |
| background_single_image + domainType=environment | environment | background_single_image_v2 | AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md current background adapter | ready |

Rules:

1. New package mode routes by `assetType + domainType`; legacy mode routes by
   artifactType. Exactly one identity mode is authoritative.
2. Readiness=blocked returns missing_domain_evaluation_adapter.
3. Do not use a generic visual-quality score as a substitute.
4. Item icon uses the named generation guide only for its existing evaluation
   rubric. Replace this registry entry when ItemIconEvaluationGuide.md exists.
5. Character animation may use Overall Score=Not Scored. PASS requires every
   required subsection to meet its documented threshold and no common,
   structure, or domain fatal failure.
6. Popup evaluation uses PopupEventMainImageEvaluationGuide.md as scoring
   authority. A Slack adapter with different categories or thresholds is a
   formatting contract conflict and must not change the evaluation.
7. Add a content type only through the adapter declaration in Section 14.
8. The three current background rows are package mode only. They require a
   sealed v2 package and never alias legacy `imagegen_image` or
   `battle_background`.
9. Icon and background adapters are not interchangeable even when both contain
   one PNG. A structure/profile/domain mismatch returns
   background_adapter_identity_mismatch before scoring.

## 6. Workspace and Source Resolution

Resolve the repository from the active task workspace and Git metadata. Confirm
that AgentDocs and Assets belong to the same repository.

Resolve the current PC's local evaluation root in this order:

1. evaluationPackageId whose sealed manifest identity and hashes validate;
2. sourceRecordId matching the routed identity and contentId;
3. an existing same-artifact generation/download record in the current task;
4. the established domain evaluation root and the single latest unambiguous
   preserved source for the requested identity;
5. repository or task-local configuration recorded for the current PC.

For package mode, read
`AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md`.
The package manifest supplies source membership and ordering but never supplies
an evaluation verdict.

Do not invent an absolute root. If no root can be established, stop with
local_evaluation_root_not_configured.

The evaluation workspace must separate:

~~~text
source/       exact immutable artifact to evaluate
evaluation/records/{evaluationRecordId}/input/
evaluation/records/{evaluationRecordId}/evidence/
evaluation/records/{evaluationRecordId}/evaluation_result.json
evaluation/records/{evaluationRecordId}/evaluation_report.md
evaluation/evaluation_index.json
candidates/   generation candidates, when present; never evaluated by default
~~~

The staging source must be tied to contentId by its generation/download record,
manifest, stable filename, and SHA-256. Project output is never evaluation
input.

## 7. Planning and Design Evidence

Create evaluation_input.json before visual scoring. It must preserve the Canvas
form's required self-contained design evidence:

~~~text
artifactUsage
planningSource
planningOriginalContent
displayContent
planningCoreInterpretation
designConcept
promptCoreGoals
requiredVisualElements
hardConstraints
generationPromptOriginal
~~~

Rules:

- planningOriginalContent and generationPromptOriginal are verbatim source
  facts, not reconstructions.
- displayContent remains separate from original planning content.
- planningCoreInterpretation records subject, action, physical clues, time, and
  spatial relations supported by source evidence.
- requiredVisualElements contains three to five independently observable
  requirements unless the domain source supports fewer.
- hardConstraints includes common, structure, and domain fatal conditions.
- unavailable required evidence blocks evaluation; optional evidence is
  explicitly Not Provided or Not Evaluated.
- do not remove an important planning fact because the generated image omitted
  it.

For domains that require pre-visual commitment, record:

~~~text
evaluationBriefCreatedAt
evaluationBriefHash
visualInspectionStartedAt
~~~

The brief hash must be recorded before opening the evaluated media. A domain
adapter may require this gate; story_popup_main_image does.

## 8. Artifact Structure Profiles

### 8.1 Common structure manifest

Every result declares:

~~~text
profile
members[]
memberId
role
sourcePath
sha256
mediaType
width and height
~~~

Set profiles declare stable member order. Package mode uses the sealed
`manifestPayloadHash` from GeneratedMediaEvaluationPackageGuide.md; legacy mode
uses `manifestHash` calculated from canonical ordered member records. Preserve
each member hash in either mode.

### 8.2 single_image

Use for icons, backgrounds, portraits, and popup images.

Required checks:

- exactly one evaluable source image;
- decodable expected media type;
- expected dimensions, ratio, color mode, and alpha contract from the domain;
- no crop or edge violation under the domain rule;
- intended display-size preview when required;
- one primary Slack media role for later Canvas formatting.

### 8.2.1 background_single_image_v2

Use only for current package-mode `background_single_image` with
`domainType=stage|battle|environment`. In addition to single_image checks,
verify the sealed package contains the exact registered background profile and:

~~~text
scene contract
composition and viewpoint
horizon and ordered depth layers
playable/readability area
subject inclusions and exclusions
canvas and aspect ratio
target display and safe area
final background policy
content/scene consistency lock
scene_composition_anchor
~~~

Missing metadata blocks evaluation; it is not reconstructed from pixels. Icon
visual-center, transparent-icon, outline/silhouette and small-size icon rules
are invalid here. The inverse is also true: a current icon adapter must reject
background scene metadata.

### 8.3 paired_sheet_animation

Use when a reference image and animation sheet form one evaluated artifact.

Required evidence:

- reference/source PNG;
- exact animation sheet PNG;
- sheet dimensions, cell size, rows, columns, and usable frame count;
- row-major individual PNG frames;
- contact sheet;
- playback GIF or equivalent motion preview;
- frame order, expected timing, and loop/ending mode;
- hash for source members and derived evidence.

The playback aid proves motion readability only. Alpha, crop, edge, pixel
fidelity, and source integrity are judged from original PNGs and decoded PNG
frames.

### 8.4 ordered_rotation_set

Use for an exact ordered character rotation export.

Required checks:

- exactly eight members ordered north, north_east, east, south_east, south,
  south_west, west, north_west;
- one unique member per direction with no preview/thumbnail substitution;
- consistent identity, equipment, palette, canvas, scale and center;
- per-member source hash and one package `manifestPayloadHash` or legacy
  ordered `manifestHash`;
- any missing, duplicate, reordered or identity-drift member fails the set.

### 8.5 ordered_frame_set

Use for an ordered group of individual animation frames.

Required checks:

- manifest membership, exact count, stable order, and no duplicate/missing
  member;
- consistent canvas, scale, center, identity, palette, equipment, and required
  parts;
- valid action progression, direction, key pose, ending, and loop behavior;
- per-frame technical gates and cross-frame semantic gates;
- contact sheet and playback evidence.

### 8.6 Set decision rule

An image set is one atomic evaluation unit.

- one fatal member failure fails the whole set;
- never hide a bad member through average scoring;
- member findings identify memberId or frame range;
- artifact-level and playback-level findings remain distinct;
- a set result applies to the exact package `manifestPayloadHash` or legacy
  `manifestHash`;
- replacing one member invalidates the set result and requires re-evaluation.

## 9. Evaluation Execution Order

### Phase A: Resolve and lock input

1. Validate the request and route one adapter.
2. Resolve repository, workspace, source record, source bytes, and planning
   evidence.
3. Create the structure manifest and compute source hashes.
4. Create the record-scoped evaluation_input.json and lock the evaluation
   brief.
5. If required facts are missing or conflict, stop before scoring.

### Phase B: Common gates

Run before domain scoring:

~~~text
artifact identity
source provenance and hash
file integrity and decodability
staging/project path separation
required planning/design evidence completeness
forbidden visible text, logo, watermark, or UI unless domain-approved
master concept hard constraints
evidence sufficiency
~~~

Common gate status:

~~~text
PASS
FAIL
INSUFFICIENT_EVIDENCE
NOT_APPLICABLE
~~~

### Phase C: Structure gates

Execute the routed profile from Section 8. Derive evidence only inside
evaluation/evidence. Never alter source bytes.

### Phase D: Domain evaluation

1. Run every domain fatal gate before scoring.
2. Preserve the exact domain category names and maximums.
3. Record a short, artifact-specific evidence statement for each score.
4. Apply domain threshold and category minimums without rounding up.
5. Preserve the domain-native result and map it through Section 10.

### Phase E: Normalize and validate

1. Write evaluation_result.json using Section 11.
2. Write evaluation_report.md from the same in-memory facts.
3. Confirm result, scores, findings, actions, hashes, and evidence agree.
4. Validate Canvas-required fields and eleven archival semantics.
5. Leave promotionStatus=not_promoted for evaluable artifacts. Evaluation does
   not approve or perform project copy.

## 10. Status and Result Normalization

Evaluation lifecycle:

~~~text
not_started
in_progress
completed
blocked
skipped
~~~

Completed Canvas-compatible result:

~~~text
PASS
CONDITIONAL_PASS
FAIL
SKIPPED
~~~

Normalization rules:

- preserve PASS, CONDITIONAL_PASS, FAIL, and SKIPPED when explicitly defined by
  the domain adapter;
- domain pass maps to PASS;
- domain fail maps to FAIL;
- domain needs_revision maps to FAIL with nextAction=revise or regenerate,
  unless the domain adapter explicitly declares an equivalent
  CONDITIONAL_PASS contract;
- domain needs_human_review maps to evaluationStatus=blocked, result=null,
  nextAction=needs_decision;
- domain not_evaluated maps to evaluationStatus=not_started, result=null;
- an intentional no-image policy maps to skipped/SKIPPED only when the domain
  guide approves it.

Project-copy eligibility:

~~~text
PASS             -> passForProjectCopy=true
CONDITIONAL_PASS -> passForProjectCopy=false
FAIL             -> passForProjectCopy=false
SKIPPED          -> passForProjectCopy=false, promotionStatus=not_applicable
blocked/null     -> passForProjectCopy=false
~~~

An approval does not change an evaluation result. New generated-image promotion
uses pass_only_v1: only exact PASS is eligible. Legacy Canvas records that
promoted a CONDITIONAL_PASS remain historical records, not precedent.

## 11. Normalized Result Contract

Create a stable record ID before writing evaluation output:

~~~text
evaluationRecordId =
eval.{request_type_key}.{contentId}.{UTC_YYYYMMDDTHHMMSSZ}.{source_or_manifest_hash_prefix_12}
~~~

`request_type_key` is `{assetType}.{domainType}` in package mode and the exact
legacy artifactType in compatibility mode.

The record ID identifies one decision over one exact source hash or set
manifestHash. If the ID already exists with different bytes or facts, stop with
evaluation_record_collision. Never reuse an ID for a new evaluation.

schemaVersion is:

~~~text
generated_image_evaluation_v1
~~~

evaluation_result.json must contain:

~~~text
schemaVersion

recordMetadata:
  evaluationRecordId
  formVersion
  evaluationDomain
  evaluationPackageId
  assetType
  domainType
  legacyArtifactType
  artifactType
  artifactId
  artifactName
  evaluationReportSource
  reviewDate
  reviewedAt
  reviewer

resultSummary:
  evaluationStatus
  domainNativeResult
  result
  overallScore or Not Scored
  scoreMaximum or Not Scored
  hardFail
  highestSeverity
  passCriteria
  nextAction
  passForProjectCopy

targetArtifact:
  stagingArtifactPath
  evaluationWorkspacePath
  projectTargetPath
  promotionStatus
  resourceKey
  stagingHash or manifestPayloadHash or legacy manifestHash
  projectHash
  copyVerification
  dimensionsOrDuration

planningAndDesign:
  artifactUsage
  planningSource
  planningOriginalContent
  displayContent
  planningCoreInterpretation
  designConcept
  promptCoreGoals
  requiredVisualElements
  hardConstraints
  generationPromptOriginal

artifactStructure
evidencePackage
commonGates
structureGates
domainGates
scoreBreakdown
findings
requiredActions
optionalImprovements
domainSpecificNotes
reEvaluationPlan
changeLog
validation
~~~

In package mode, `artifactType` is the registered legacyArtifactType when one
exists; otherwise it is the exact assetType for Canvas compatibility. Never
derive it from a filename. `assetType` and `domainType` remain authoritative.

The following Canvas archival semantics map directly and must all be
representable:

| Canvas semantic | Result field |
| --- | --- |
| Record Metadata | recordMetadata |
| Result Summary | resultSummary |
| Target Artifact | targetArtifact |
| Evidence Package | planningAndDesign, artifactStructure, evidencePackage |
| Score Breakdown | scoreBreakdown |
| Findings | findings |
| Required Actions | requiredActions |
| Optional Improvements | optionalImprovements |
| Domain-Specific Notes | domainSpecificNotes |
| Re-evaluation Plan | reEvaluationPlan |
| Change Log | changeLog |

### 11.1 Score entries

Each entry contains:

~~~text
criterionId
category
sourceGuide
score or Not Evaluated
max
status
evidence
scope
memberIds
~~~

Do not rename or rebalance domain categories for consistency. Overall Score may
be Not Scored when the adapter uses subsection thresholds instead of one total.

### 11.2 Findings

Each finding contains:

~~~text
findingId
severity: Critical | Major | Minor | Suggestion
criterionId
scope: artifact | member | frame_range | playback | project_import
memberIds
finding
evidence
impact
recommendation
~~~

No findings is represented by an empty array plus highestSeverity=None.

### 11.3 Required actions

Each action contains:

~~~text
priority
action
owner
status: Open | In Progress | Done | Blocked
due
correctionMethod
regenerationRequired
triggerFindingIds
~~~

Required corrections never move into Optional Improvements merely to preserve a
PASS.

### 11.4 Re-evaluation plan

~~~text
required
expectedScoreAfterFix or Not Estimated
passLikelihood or Not Estimated
remainingRisk
reEvaluationTrigger
priorEvaluationRecordId
~~~

re_evaluate creates a new immutable result linked to the prior record. It does
not overwrite the prior decision or evidence.

## 12. Slack Canvas Readiness

The evaluation output prepares Canvas data but does not publish it.

Required reader-facing evidence:

- exact reviewed media through a future Slack-hosted file, never a local file
  link;
- original planning content verbatim;
- display content separately;
- planning interpretation and design concept;
- one to three prompt core goals;
- three to five observable required elements when supported;
- hard constraints;
- exact generation prompt or an explicit unavailable reason;
- result, score, Hard Fail, severity, findings, actions, and re-evaluation
  trigger;
- provenance, review identity, and promotion state.

Media presentation profiles:

- single_image: one primary reviewed image;
- paired_sheet_animation: one playback GIF plus either reference PNG or contact
  sheet, as required by the common Canvas animation evidence rule;
- ordered_frame_set: one playback GIF plus a contact sheet or representative
  source frame;
- ordered_rotation_set: one ordered eight-direction contact sheet plus one
  representative original rotation;
- diagnostic previews remain evidence and are not presented as the primary
  evaluated artifact.

The later Canvas formatter reads evaluation_result.json and may validate it. It
must not re-score, change categories, remap thresholds, rewrite source text, or
infer missing required evidence.

## 13. Output Files

Write under the internally resolved evaluation workspace:

~~~text
evaluation/records/{evaluationRecordId}/input/evaluation_input.json
evaluation/records/{evaluationRecordId}/evaluation_result.json
evaluation/records/{evaluationRecordId}/evaluation_report.md
evaluation/records/{evaluationRecordId}/evidence/{domain-defined evidence}
evaluation/evaluation_index.json
~~~

The human report and JSON must agree. The report may be concise; the JSON
preserves complete audit facts.

evaluation_index.json is a small record locator containing artifact identity,
evaluationRecordId, result, source or manifest hash, evaluatedAt, and relative
record paths. It may identify the latest record but is not itself evaluation
evidence. Canvas and promotion tasks must resolve and pin one immutable record,
not depend on a moving latest pointer.

Do not write evaluation output into Assets/Contents, Assets/ImagesGenerated, or
Assets/Resources.

## 14. Domain Adapter Extension Contract

A new or revised domain evaluation guide must declare:

~~~text
adapterId
assetType and domainType, or legacyArtifactType for compatibility
evaluationDomain
structureProfile
canonicalContentSourceRule
artifactUsageRule
planningEvidenceRule
stagingSourceRule
projectTargetRule
requiredEvidence
domainFatalGates
scoreCategories and maximums
passThreshold
categoryMinimums
domainNativeResults
resultNormalization
domainSpecificNotes fields
mediaEvidenceRule
reEvaluationRule
~~~

Adapter validation:

1. The `assetType + domainType` key or legacyArtifactType is unique in the
   registry.
2. The structure profile exists.
3. Required source facts can be resolved without external absolute paths.
4. Fatal gates are distinguishable from scored deductions.
5. Categories have stable IDs, names, and maximums.
6. Result normalization is explicit.
7. Required Canvas planning/design evidence has a source.
8. Single or set media evidence can be rendered later in Slack.
9. passForProjectCopy is true only for normalized PASS.
10. The adapter does not generate, download, repair, promote, or publish.

## 15. Failure Types

~~~text
invalid_evaluation_request
evaluation_package_not_found
evaluation_package_not_sealed
evaluation_package_hash_mismatch
evaluation_identity_mode_conflict
unsupported_artifact_type
missing_domain_evaluation_adapter
incomplete_domain_evaluation_adapter
evaluation_contract_conflict
repository_not_resolved
local_evaluation_root_not_configured
source_record_not_found
ambiguous_source_record
staging_source_not_found
artifact_identity_mismatch
source_hash_mismatch
artifact_set_incomplete
planning_source_not_found
planning_evidence_incomplete
generation_prompt_provenance_missing
evaluation_brief_not_locked
invalid_media
insufficient_evidence
evidence_derivation_failed
evaluation_write_failed
evaluation_record_collision
result_report_mismatch
score_validation_failed
canvas_required_field_missing
background_adapter_identity_mismatch
legacy_current_identity_conflict
missing_background_evaluation_contract
~~~

Failure preserves the source and existing records unchanged. Do not invoke a
preceding or following pipeline step as recovery.

## 16. Validation Checklist

- [ ] The request contains generalized IDs, not required paths.
- [ ] One ready domain adapter and one structure profile were resolved.
- [ ] The exact preserved source matches its record and hash.
- [ ] Planning and design evidence satisfies the Canvas common contract.
- [ ] The evaluation brief was locked before visual inspection when required.
- [ ] Common and structure gates ran before domain scoring.
- [ ] Domain categories, maximums, thresholds, and minimums were preserved.
- [ ] A bad set member was not hidden by average scoring.
- [ ] Result normalization is explicit and lossless through domainNativeResult.
- [ ] All eleven Canvas archival semantics are representable.
- [ ] Report and JSON agree.
- [ ] PASS alone sets passForProjectCopy=true.
- [ ] No image mutation, project copy, Slack write, Git action, or deployment
      occurred.

## 17. Related Prompt

~~~text
AgentDocs/task-prompts/content/GeneratedImageEvaluationPrompt.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
~~~
