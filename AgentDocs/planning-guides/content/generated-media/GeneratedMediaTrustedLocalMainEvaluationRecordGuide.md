# Generated Media Trusted-Local MAIN Evaluation Record Guide

## 1. Adapter registration and boundary

`generated_media_trusted_local_main_evaluation_record_adapter@1.0.0` is a
deterministic no-score record projector for an immutable trusted-local
`character_single_image + character + character_single_image_v2` MAIN. Its
canonical adapter payload is
`helpers/generated_media_trusted_local_main_evaluation_adapter_v1.json`; RFC
8785 JCS SHA-256 is
`c76b11ee51f641da78b54048c670658628e379ce9f74f8b9cb878c1c9742953e`.

The producer role is
`generated_media_trusted_local_main_evaluation_record_projector`. It does not
inspect media, evaluate quality, choose a decision, score, modify evidence, or
call provider/postprocess/preservation/promotion. It only projects an already
immutable, independently produced evaluation evidence document and an exact
source-bound receipt into a current hash-bound record.

The adapter supports reference policies
`generated_media_trusted_local_evaluated_main_reference@1.0.0` and
`generated_media_trusted_evaluated_prior_grade_main_reference@1.0.0`. A
sequential identity selection may use only a record whose `result=PASS` and
whose content/media/profile/reference-policy evidence exactly matches.

## 2. Closed projector input

`generated_media_trusted_local_main_evaluation_projection_input_v1` has exactly:

```yaml
schemaVersion:
adapterKey:
adapterPayloadSha256:
referencePolicyKey:
contentId:
assetType: character_single_image
domainType: character
structureProfile: character_single_image_v2
mediaPath:
mediaSha256:
mediaByteLength:
profileKey:
profilePayloadSha256:
evaluationEvidencePath:
evaluationEvidenceSha256:
sourceBoundReceiptId:
sourceBoundReceiptPath:
sourceBoundReceiptSha256:
sourceBoundContentShaPointer: /outputSha256 | /mediaSha256
publicationState: local_unpublished
indexBeforeSha256:
```

The media path/SHA/length, source profile, reference policy, evidence file, and
receipt file are immutable inputs. Evidence and receipt bytes must be canonical
JCS UTF-8 with one LF, no BOM, and match their raw SHA. A chat message,
commentary, unhashed observation, mutable path alias, or reconstructed evidence
is invalid.

The source-bound receipt identity is deterministically
`gmsourcereceipt1.{sourceBoundReceiptSha256[0:20]}` even when the historical
receipt has no embedded ID. This identity references the immutable raw receipt;
it does not rewrite it. The selected exact JSON pointer must resolve in that
receipt to `mediaSha256`.

## 3. Independent evaluation evidence

The referenced evidence document has schema
`generated_media_independent_evaluation_evidence_v1` and exactly:

```yaml
schemaVersion:
evaluationTaskId:
contentId:
mediaSha256:
decision: PASS | FAIL | unavailable
facts:
  - factId:
    outcome: PASS | FAIL | unavailable
    evidenceRef:
```

Facts are non-empty and ordered. The adapter preserves `decision` exactly and
does not derive or change it. Numeric score members at any depth are forbidden;
the record uses `scorePolicy=not_scored`. Content/media mismatch, malformed
facts, or evidence hash drift fails before projection.

## 4. Record identity and serialization

The hash payload schema is
`generated_media_trusted_local_main_evaluation_hash_payload_v1`. It contains
the adapter key/hash, reference policy, content/type/structure, media identity,
profile identity, evaluation task/path/raw SHA/facts JCS SHA, source-bound
receipt ID/path/raw SHA/content pointer/content SHA, `evaluationStatus=completed`,
the exact categorical result, `scorePolicy=not_scored`,
`providerReceiptPolicy=not_required_not_claimed`, and
`publicationState=local_unpublished`.

```text
evaluationPayloadSha256 = SHA256(JCS(hashPayload))
evaluationRecordId = gmtrusteval1.{contentId}.{evaluationPayloadSha256[0:20]}
```

The record replaces the payload schema with
`generated_media_trusted_local_main_evaluation_record_v1` and adds only
`evaluationRecordId` and `evaluationPayloadSha256`. Record bytes are RFC 8785
JCS UTF-8 plus exactly one LF, no BOM. Canonical path:

```text
AgentDocs/planning-data/generated-media-evaluations/v1/trusted_local_main/{contentId}/{evaluationRecordId}.json
```

The adapter permits `local_unpublished` record production so Git publication
can occur later. It never claims authoritative Git publication, provider
receipt, project-copy eligibility, or a newly executed evaluation.

## 5. Index, CAS, and no-clobber

The same directory contains `evaluation_index.json` with closed schema
`generated_media_trusted_local_main_evaluation_index_v1` and exactly
`schemaVersion`, `contentId`, and `entries`. Each entry has exactly
`evaluationRecordId`, `evaluationRecordPath`, `evaluationPayloadSha256`,
`evaluationRecordSha256`, `mediaSha256`, and `result`.

Producer order is mandatory:

1. validate all immutable inputs and exact `indexBeforeSha256`;
2. project record and index bytes without writing;
3. create the record first with atomic no-clobber and reopen/hash it;
4. append the sorted entry only by CAS against `indexBeforeSha256`;
5. return `created`, or `reused_identical` only when record and entry bytes are
   identical; never overwrite or normalize an occupied record/index.

An index failure leaves a valid no-clobber record for bounded index-only retry.
The pure helper
`helpers/generated_media_trusted_local_main_evaluation_projector_v1.mjs`
performs projection only and writes no file.

## 6. G2 registered fixture

```text
contentId=character.seojin.2
mediaSha256=ff512be1ac75ba0924eab316679dcab4ee171f4a0703014791f2f295e8a6d327
mediaByteLength=6293540
sourceBoundReceiptId=gmsourcereceipt1.27031cd5d53f04233091
sourceBoundReceiptSha256=27031cd5d53f04233091b5811665eb68ff940ba7c97c0bd106a5edd79e45ee7e
sourceBoundContentShaPointer=/outputSha256
```

The adapter profile contains the exact registered media and receipt paths. It
does not contain or invent an evaluation task, decision, facts, evidence path,
or record identity. Those must come from the completed independent evaluation.

## 7. Failures

```text
trusted_local_evaluation_adapter_profile_mismatch
trusted_local_evaluation_input_mismatch
trusted_local_evaluation_evidence_unavailable
trusted_local_evaluation_evidence_hash_mismatch
trusted_local_evaluation_content_mismatch
trusted_local_evaluation_source_receipt_hash_mismatch
trusted_local_evaluation_source_receipt_content_mismatch
trusted_local_evaluation_numeric_score_forbidden
trusted_local_evaluation_record_collision
trusted_local_evaluation_index_cas_mismatch
trusted_local_evaluation_index_collision
```

Every failure is no-write and does not authorize an evaluation decision,
provider call, media operation, or replacement evidence.
