# Generated Image Project Promotion Guide

## 1. Purpose

This guide owns only the final project-copy step for an image that has already
been generated, downloaded, preserved locally, and evaluated.

~~~text
generalized promotion request
-> resolve the completed local evaluation package
-> verify Pass and artifact identity
-> derive the canonical project target internally
-> copy the exact evaluated bytes
-> verify hash, Unity metadata, and consumer readiness
~~~

Generation, provider operation, download, image correction, scoring,
re-evaluation, Slack publication, Git work, and deployment are separate tasks.
This task must not perform or silently continue any of them.

## 2. Required References

Always read:

~~~text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/planning-guides/content/GeneratedImageEvaluationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
~~~

For current package mode, the generated-media guides are mandatory. Then
resolve and read the exact domain download/evaluation guides in Section 5.
The completed evaluation package remains the authority for the result. This
task verifies that package but does not score the image again.

## 3. External Request Contract

One request promotes one logical artifact or one domain-defined artifact set.
The caller supplies generalized identity and approval facts only.

### 3.1 Allowed fields

~~~text
requestId: optional stable request id
evaluationPackageId: preferred current package identity
assetType: required for package mode
domainType: required for package mode
artifactType: required only for legacy mode
contentId: required canonical content id
evaluationRecordId: optional stable non-path evaluation record id
replaceExisting: optional boolean, default false
replacementApprovalRef: required non-path approval reference when replacing
~~~

evaluationRecordId is only a lookup discriminator. It does not prove Pass.
When omitted, the task may use the single latest unambiguous completed record
for the same artifactType and contentId.

Exactly one identity mode is authoritative:

~~~text
current package mode: evaluationPackageId + assetType + domainType + contentId
legacy mode: artifactType + contentId
~~~

Mixing modes returns promotion_identity_mode_conflict. Current
`background_single_image` never aliases legacy `battle_background` or
`imagegen_image`.

### 3.2 Fields the caller must not need

~~~text
repositoryRoot
evaluationRoot or evaluationWorkspacePath
stagingArtifactPath or sourceFilePath
evaluationReportPath
projectTargetPath
filename or extension
ContentDomain or imageArtifactType
Unity importer settings or .meta path
generation provider, prompt, or tool URL
score or passing threshold
builder or consumer path
~~~

If supplied, path-like fields are untrusted hints and must not be used as the
copy source or destination. Resolve current-PC local paths and repository rules
inside the task. Never reuse another PC's absolute path.

## 4. Strict Responsibility Boundary

This task may:

- locate an existing completed local evaluation package;
- read the report, manifest, hashes, and evaluated source files;
- confirm that the recorded result is exactly Pass;
- perform non-scoring file integrity and identity checks;
- derive the canonical project target;
- copy the exact evaluated bytes after all gates pass;
- preserve or create Unity metadata according to approved domain rules;
- verify source/project hashes and report consumer readiness.

This task must not:

- create or edit an image;
- open ImageGen or PixelLab;
- download or convert provider output;
- assign or change an evaluation score or result;
- turn Conditional Pass, approval text, or caller-provided text into Pass;
- choose another candidate or repair an evaluated artifact;
- write to Slack or perform Git, merge, deployment, or content build.

When evidence is incomplete, stale, ambiguous, or not Pass, stop without
copying and return the work to the appropriate preceding task.

## 5. Internal Artifact Routing Registry

The task derives ownership, evidence conventions, project folder, filename, and
whether the artifact is a single file or set.

| artifactType | Domain | Artifact form | Canonical project contract | Required domain guides |
| --- | --- | --- | --- | --- |
| skill_icon | Skill | one PNG | Assets/ImagesGenerated/Skill/icon/{contentId}.icon.png | AgentDocs/planning-guides/skill/SkillIconDownloadGuide.md and SkillIconEvaluationGuide.md |
| item_icon | Item | one PNG | Assets/ImagesGenerated/Item/icon/{contentId}.icon.png | AgentDocs/planning-guides/item/ItemIconGenerationGuide.md |
| skill_animation | Skill | domain-defined PNG set | Skill/animation_reference and Skill/animation | AgentDocs/planning-guides/skill/SkillImageDownloadGuide.md and SkillImageEvaluationGuide.md |
| character_animation | Character | evaluated renamed frame set | Assets/ImagesGenerated/Character/animation | AgentDocs/planning-guides/character/CharacterAnimationDownloadGuide.md and EvaluationAnimationGuide.md |
| battle_background | Battle | one PNG | Assets/ImagesGenerated/Battle/background/{contentId}.background.png | AgentDocs/planning-guides/battle/BattleCreateGuide.md and the evaluation guide named by the report |
| story_popup_main_image | Stage | one PNG | Assets/ImagesGenerated/Stage/popup_main/{contentId}.main.png | AgentDocs/planning-guides/stage/PopupEventMainImageEvaluationGuide.md |
| background_single_image + domainType=battle | Battle | one PNG, current package v2 | Assets/ImagesGenerated/Battle/background/{contentId}.background.png | AgentDocs/planning-guides/battle/BattleCreateGuide.md and AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md |
| background_single_image + domainType=stage | Stage | one PNG, current package v2 | domain target resolver extension required | AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md; block until a Stage background storage guide owns a canonical target |
| background_single_image + domainType=environment | Environment | one PNG, current package v2 | domain target resolver extension required | AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md; block until an Environment background storage guide owns a canonical target |

Routing rules:

1. artifactType must match one row exactly.
2. Do not infer or add a route from a similar name.
3. contentId must match the identity recorded by the evaluation package.
4. A set route is atomic. Promote every required member or none.
5. Derive targets from this registry, ContentFolderStructureGuide.md, and the
   exact domain guide.
6. A conflicting domain path blocks promotion with
   domain_storage_contract_conflict.
7. Current background rows require the exact evaluationPackageId,
   evaluationRecordId, `background_single_image_v2`, domainType and registered
   background adapter identity through copy verification.
8. Icon/background rows are never exchanged by media type or filename.
9. Stage/environment background rows return
   background_promotion_target_contract_missing until an exact domain guide
   and ContentFolderStructureGuide contract define their canonical target. Do
   not invent a folder.
10. The legacy battle_background row remains artifactType-based compatibility;
    the current battle row is package mode and does not reuse legacy identity.

## 6. Repository and Evaluation Package Resolution

### 6.1 Repository

Resolve the current repository from the active task workspace and Git metadata.
Confirm that AgentDocs, Assets, and the selected domain guides belong to the
same repository.

### 6.2 Completed local evaluation package

Resolve the current PC's evaluation workspace in this order:

1. exact evaluationRecordId for the requested package identity and contentId,
   or for the requested legacy artifactType and contentId;
2. an existing same-artifact record referenced in the current task;
3. the single latest completed record under the established domain evaluation
   root for the current PC;
4. repository or task-local configuration already recorded for this PC.

Do not invent a local root or ask the caller to provide an absolute path.

The selected package must contain or reliably link the fields for its identity
mode. Current package mode preserves the exact evaluationPackageId,
assetType, domainType, contentId, structureProfile, evaluationRecordId, and
immutable hashes. Legacy mode preserves artifactType and contentId and must not
invent current package fields.

~~~text
current package mode: evaluationPackageId, assetType, domainType, contentId,
                      structureProfile, evaluationRecordId
legacy mode: artifactType, contentId, evaluationRecordId
common: generated_image_evaluation_v1 structured result, completed report,
        evaluated source manifest, member SHA-256, completion identity
~~~

The selected package must also contain or reliably link:

~~~text
artifactType and contentId
generated_image_evaluation_v1 structured result
completed evaluation report
final result
evaluated source file or complete file-set manifest
SHA-256 for every evaluated source member
evaluation completion identity or timestamp
required fatal-gate evidence from the domain evaluation workflow
~~~

Multiple equally valid records, missing hashes, mutable preview-only files, or
a report that cannot be tied to the exact source bytes block promotion.

## 7. Promotion Gates

All gates must pass before the project folder is modified.

### 7.1 Evaluation gate

- schemaVersion is generated_image_evaluation_v1;
- resultSummary.evaluationStatus is completed;
- resultSummary.result is exactly PASS;
- resultSummary.passForProjectCopy is true;
- targetArtifact.promotionStatus is not_promoted;
- Conditional Pass, approved wording without Pass, Fail, skipped, incomplete,
  and insufficient evidence are not promotable;
- this task does not recalculate or improve the result;
- all fatal checks recorded by the domain evaluation are clear.

### 7.2 Identity and integrity gate

- requested artifactType and contentId equal the report and manifest;
- each evaluated source exists and decodes as the expected artifact form;
- current SHA-256 equals evaluated SHA-256 for every member;
- set membership, stable names, count, frame order, and pair relations match the
  domain manifest;
- no candidate, preview, contact sheet, report image, or thumbnail is selected.

### 7.3 Target and replacement gate

- targets are derived internally and are under Assets/ImagesGenerated;
- no destination is under Assets/Resources;
- canonical filenames contain no attempt, approval, date, or provider suffix;
- when a target or .meta exists, replaceExisting=true and a non-empty
  replacementApprovalRef are both required;
- replacement preserves the existing .meta and GUID;
- a new target never reuses another asset's .meta or GUID.

Perform every preflight check before copying the first member of a set.

## 8. Copy Procedure

1. Build an immutable promotion manifest from the verified evaluation package.
2. Derive every destination and check duplicate or colliding targets.
3. Capture existing destination and .meta state without changing it.
4. Create only the required canonical project folder when promotion is ready.
5. Copy each evaluated source byte-for-byte. Do not resize, crop, recompress,
   alter pixels, or switch candidates.
6. For approved replacement, leave the existing .meta and GUID unchanged.
7. For a new asset, create/import metadata only through the approved Unity
   domain workflow and verify GUID uniqueness.
8. Apply documented Sprite/import/slicing settings without altering PNG bytes.
9. Recompute destination SHA-256 and compare every member with its source.
10. Verify the complete set, .meta, importer, and manifest before promoted.

If a multi-file copy fails partway, do not report partial success. Restore the
pre-copy state when safely possible. Otherwise report every partial path and
block Unity consumption until cleanup is explicitly authorized.

## 9. Consumer Readiness

Project promotion and consumer readiness are independent states.

- Check whether the relevant builder or serialized consumer supports the
  Assets/ImagesGenerated location.
- When code is hardcoded to Assets/Resources, report
  builder_path_migration_required.
- Never create a Resources duplicate as a workaround.
- Do not claim SO binding, animation clip construction, addressable setup, or
  runtime availability without direct evidence.

## 10. State Model

~~~text
received
resolved
pass_verified
copy_preflight_passed
copied
copy_verified
promoted
~~~

Terminal non-success states:

~~~text
blocked
not_promoted
copy_failed
~~~

No state may skip pass_verified or copy_verified.

## 11. Output Contract

~~~text
requestId
artifactType
contentId
status
evaluationRecordId
evaluationResult
evaluationCompletedAt or equivalent identity
resolvedEvaluationWorkspace
evaluatedSourcePaths and hashes
projectTargetPaths and hashes
replacementMode and replacementApprovalRef
metaAndGuidStatus
importStatus
copyVerification
promotionStatus
consumerReadiness
blockers
requiredNextActions
~~~

Resolved paths are output evidence only. They are not external input fields.

## 12. Failure Types

~~~text
invalid_promotion_request
unsupported_artifact_type
repository_not_resolved
local_evaluation_root_not_configured
evaluation_record_not_found
ambiguous_evaluation_record
evaluation_not_complete
evaluation_not_pass
evaluation_source_not_found
evaluation_hash_missing
evaluation_source_hash_mismatch
artifact_identity_mismatch
promotion_identity_mode_conflict
evaluation_package_not_found
evaluation_package_hash_mismatch
background_structure_profile_mismatch
background_promotion_adapter_mismatch
background_promotion_target_contract_missing
legacy_current_identity_conflict
artifact_set_incomplete
domain_storage_contract_conflict
existing_project_artifact_requires_approval
project_target_collision
project_copy_failed
project_hash_mismatch
unity_meta_failed
unity_import_pending
builder_path_migration_required
consumer_binding_not_verified
~~~

Any failure before copy leaves the project unchanged. This task does not invoke
generation, download, correction, or evaluation as recovery.

## 13. Validation Checklist

- [ ] Input contains generalized identity and approval facts only.
- [ ] No source, evaluation, or project path was required from the caller.
- [ ] One exact artifact route was resolved internally.
- [ ] Package mode preserved evaluationPackageId, assetType, domainType,
      evaluationRecordId and structureProfile through promotion.
- [ ] Icon/background and legacy/current identities were not exchanged.
- [ ] A completed local evaluation package was found unambiguously.
- [ ] Result is exactly Pass and tied to current source hashes.
- [ ] Artifact identity and single-file or set membership match.
- [ ] Every project target is under Assets/ImagesGenerated.
- [ ] Replacement permission and .meta preservation were checked.
- [ ] Source and destination SHA-256 values match after copy.
- [ ] Promotion and consumer readiness are reported separately.
- [ ] No generation, download, scoring, Slack, Git, or deployment occurred.

## 14. Related Prompt

~~~text
AgentDocs/task-prompts/content/GeneratedImageProjectPromotionPrompt.md
~~~
