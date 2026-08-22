# Generated Media Sequential Grade Identity Authority Guide

## 1. Registered authority

This additive contract enables one `character_single_image_v2` target grade to
use the immediately preceding evaluated MAIN as identity, equipment,
proportion, and orientation authority. It keeps the expression profile
`projectbs_character_open_ink_wash_opaque_chroma_master@1.0.0` /
`b8e5d07f4e3c828649880c23d32bfd945b05b0e57a2c9cc2c240a2068049fb1a`
byte- and meaning-unchanged.

The execution profile is
`projectbs_character_open_ink_opaque_chroma_sequential_grade_identity_anchored@1.0.0`.
Its canonical payload is
`helpers/generated_media_sequential_grade_identity_execution_profile_v1.json`.
RFC 8785 JCS over the complete JSON object has SHA-256
`73a48f8c8013e3a79ac04e0c161075a14ce6b1194527c48585fd33edb009ea04`.
The profile applies only to `character_single_image + character +
character_single_image_v2`, an exact next-grade relation, and the unchanged
opaque-chroma expression profile above.

The registered G2-to-G3 fixture is:

```text
authorityContentId=character.seojin.2
targetContentId=character.seojin.3
localPath=C:/Users/parkv/Documents/Codex/2026-08-21/seojin-grade3-main-targeted-retry-postprocessing/outputs/g2-official-identity-anchored-fit-v1/seojin-g2-identity-anchored-source-bound-final.png
sha256=ff512be1ac75ba0924eab316679dcab4ee171f4a0703014791f2f295e8a6d327
byteLength=6293540
```

The fixture path/hash/length makes the asset eligible for selection; it does
not fabricate or replace its completed-PASS evaluation record or source-bound
receipt. Those evidence identities are required in each selection and must be
resolved from authoritative immutable evidence before routing.

## 2. Closed selection

A fresh planning handoff selects the branch with exactly one top-level
`sequentialIdentityAuthoritySelection` object with exactly these members:

```yaml
schemaVersion: generated_media_sequential_grade_identity_authority_selection_v1
executionProfileKey: projectbs_character_open_ink_opaque_chroma_sequential_grade_identity_anchored@1.0.0
executionProfilePayloadSha256: 73a48f8c8013e3a79ac04e0c161075a14ce6b1194527c48585fd33edb009ea04
role: identity_equipment_proportion_orientation_authority_only
targetContentId:
targetGrade: positive integer
authorityContentId:
authorityGrade: positive integer exactly targetGrade minus one
localPath:
pathPolicy: registered_exact_local_absolute_path_read_only_existing_file_no_copy_no_rewrite
sha256:
byteLength: positive integer
trustedEvidencePolicyKey: generated_media_trusted_evaluated_prior_grade_main_reference@1.0.0
evaluationRecordId:
evaluationRecordPath:
evaluationRecordSha256:
sourceBoundReceiptId:
sourceBoundReceiptPath:
sourceBoundReceiptSha256:
```

Unknown, missing, nested, duplicate, null, or empty members are invalid. The
selection must match one registered fixture and the exact target planning
content. The evaluation record must reopen at its SHA and prove
`evaluationStatus=completed`, `result=PASS`, and the same authority content and
media SHA. The source-bound receipt must reopen at its SHA and prove the same
media SHA and successful immutable production/preservation lineage. A provider
receipt is neither required nor permitted as a substitute, and must never be
invented.

The path is a registered exact local read-only path. It is resolved and hashed
before authoring and immediately before submit. It is not copied, normalized,
rewritten, or converted into project identity merely by selection.

## 3. Semantic boundary and grade delta

The reference role is exactly
`identity_equipment_proportion_orientation_authority_only`. It hard-locks:

- face geometry, hairline, low topknot, and short controlled hair;
- compact body proportions;
- right-handed sword/scabbard and recognizable pouch/shoulder equipment;
- body, equipment, and facing orientation.

The only permitted differences are next-grade clothing and authority facts
explicitly approved in the target planning snapshot. Style remains owned by the
selected expression profile and target planning. The reference is never an
edit source/target and grants no style, pose, background, framing, or pixel-copy
authority.

The branch rejects aging/severe face redesign, hairline or topknot drift,
realistic/style substitution, tall/elongated or otherwise material proportion
drift, handedness/equipment/orientation drift, foreign/ronin/samurai/katana/
fantasy or other cultural substitution, and any prior rejected target media.
For the registered G2-to-G3 fixture, SHA
`1af18044008dc72c749ade2232f61838aad1d037541f8a57cf439031a1714f2e`
is explicitly non-authoritative and forbidden as reference, edit source, or
output reuse.

## 4. Consumer projection and identity

The exact selection is copied byte-semantically and hash-significantly at the
top level through:

1. `generated_media_planning_handoff_v2`;
2. routing hash payload, routing record, `normalizedRequest`, and
   `authoringHandoff`;
3. visual brief, prompt hash payload, `generated_media_prompt_v3`, prompt index,
   and detached generation handoff;
4. generation preflight, execution scope, and generation receipt.

It is forbidden inside type specifications, identity locks, style bindings,
reference arrays, provider prose, or another selection. It is omitted for all
other branches. Existing records without the member remain valid and unchanged.
Any unequal projection fails `sequential_identity_projection_mismatch` before
record/index write or provider access.

Authoring turns the registered hard locks and the exact target-grade approved
delta into provider prose. It does not copy evidence IDs, paths, hashes, source
pose/background, or pixels into prose. Missing or contradictory locks fail the
owning sequential-identity token.

## 5. Generation mode

`builtin_imagegen_authenticated_sequential_grade_identity_single_submit_v1`
uses the actual call with exactly:

```yaml
prompt: exact non-empty stored provider prose
referenced_image_paths:
  - exact registered localPath
```

No other callable member is present. The reference count is one;
`num_last_images_to_include` is prohibited. The execution-scope payload binds
the authority main, request/prompt/handoff identities, expression-profile
key/hash, selection and selection hash, execution-profile key/hash,
`submitCountMaximum=1`, and `retryCountMaximum=0`. Its idempotency key is
`gmseqidentity1.{executionScopeSha256[0:20]}`. Active, completed, or ambiguous
same-key state blocks another submit.

The returned provider master must satisfy the unchanged opaque-chroma profile.
Generation additionally observes every identity lock and only the approved
next-grade delta. A mismatch consumes the one submit and ends no-retry; it does
not authorize repair, edit, postprocess, evaluation, or promotion. A conforming
master proceeds only to the distinct `generated_media_chroma_uncomposite` role
and remains `projectCopyEligible=false`.

## 6. Failure tokens

```text
sequential_identity_profile_mismatch
sequential_identity_projection_mismatch
sequential_identity_reference_mismatch
sequential_identity_evidence_mismatch
sequential_identity_reference_count_invalid
sequential_identity_aging_or_face_drift
sequential_identity_style_drift
sequential_identity_proportion_drift
sequential_identity_equipment_or_orientation_drift
sequential_identity_cultural_drift
sequential_identity_rejected_target_media_forbidden
```

Pre-submit failures have `providerCalled=false`, `submitCount=0`, and
`retryCount=0`. Existing provider/capability/approval/idempotency and
opaque-chroma failure tokens remain unchanged.
