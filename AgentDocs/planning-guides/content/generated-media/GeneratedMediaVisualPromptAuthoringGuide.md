# Generated Media Visual Prompt Authoring Guide

## 1. Purpose and Contract Identity

Guide Type: reference/policy and embedded-data contract.

```text
guideContractVersion: generated_media_visual_prompt_authoring_v1
embeddedBriefSchema: generated_media_visual_brief_v1
```

This guide defines how one immutable Generated Media planning handoff becomes a
provider-neutral visual brief and then a provider-native prompt. It supplies a
single common design language plus artifact-specific translation rules for:

```text
character_main_image
character_animation
icon
general_animation
imagegen_image
```

It does not create planning, route a request, call a provider, download or
package media, evaluate output, promote an asset, operate Unity, perform Git, or
deploy. Provider prompt authoring remains owned by the four registered authoring
prompts and their pipeline guides.

## 2. Authority and Required References

Read these exact authorities before authoring:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
```

Source priority is mandatory:

```text
Master Concept
-> approved immutable planning handoff
-> this common visual authoring guide
-> exact registered artifact/domain profile
```

Authority is concern-specific within that order:

- Master Concept owns Joseon-period grounding, Korean cultural identity,
  traditional aesthetics, and cultural prohibitions. No lower source can relax
  or create an exception to it.
- The approved planning handoff owns asset identity, depicted meaning,
  required/prohibited elements, appearance, motion, scene, composition, camera,
  material, palette, and background facts that are specific to the request.
- This guide owns evidence-preserving normalization, common visual hierarchy,
  provider-neutral brief structure, and provider translation boundaries.
- The exact registry row and its versioned profile authority may add only
  registered artifact/domain rendering constraints. It cannot replace missing
  planning or weaken an upper rule.
- Runtime/code owns executable frame, sheet, direction, import, and serialization
  behavior, but it does not create visual meaning.

If two sources materially disagree, return
`material_visual_contract_conflict`. Do not merge, average, or prefer a visually
convenient interpretation. If a Master Concept-compliant interpretation cannot
be established from verified evidence, generate no brief or prompt.

## 3. Scope and Non-responsibilities

This guide may:

- translate approved facts into independently observable visual statements;
- select visual wording and hierarchy without changing meaning;
- separate subject, composition, palette/material, background, exclusions, and
  provider settings;
- apply one exact registered artifact/domain profile;
- produce evidence and constraint mappings for every prompt statement.

This guide must not:

- invent identity, period, culture, material, color, symbol, action,
  environment, background, camera, story beat, effect stage, or frame behavior;
- infer requiredElements or prohibitedElements from a name, ID, gameplay lore,
  asset resemblance, or legacy output;
- convert an absent planning decision into a profile default when that decision
  changes depicted meaning;
- duplicate skill/item/stage/battle authoring prompts;
- select a route or profile by similarity;
- perform deterministic post-processing or describe it as provider artwork;
- place evaluation, PASS, project promotion, Slack, Unity, Git, or deployment
  instructions inside provider prompt text.

## 4. Registered Profile Gate

Domain-specific rules apply only when all values exactly match one row in
`generated_media_authoring_profile_registry_v1`:

```text
assetType + domainType + profileId + profileVersion
```

The authoring task must consume `routingRecordFile` and the router's
`selectedRegistryRowId`, `appliedProfile`, selected pipeline/prompt, and
normalized request. It validates that selected row against the pinned registry;
it never independently selects, substitutes, or tie-breaks a registry row.

```text
1 exact row -> continue
0 rows -> unsupported_visual_profile
2+ rows -> visual_profile_registry_conflict
```

Unknown profile IDs are not case-folded, aliased, upgraded to latest, or mapped
by semantic similarity. A profile may add technical style, layout, readability,
or provider-field constraints only when its registered authority states them.
It cannot supply a missing character identity, symbol, action, sequence, scene,
camera, or other planning-owned fact.

## 5. Common Provider-neutral Visual Brief

### 5.1 Separation of representations

Preserve three distinct layers:

```text
planningOriginal
-> normalizedVisualBrief
-> providerPromptPayload
```

- `planningOriginal` is an immutable capture/reference to approved handoff facts.
- `normalizedVisualBrief` restates only those facts and registered constraints in
  provider-neutral, observable visual language.
- `providerPromptPayload` translates the normalized brief into PixelLab fields
  or one ImageGen scene prompt without adding design facts.

Do not replace planningOriginal with the normalized brief, and do not treat
provider wording as planning authority.

### 5.2 generated_media_visual_brief_v1

The brief is embedded only in a new `generated_media_prompt_v2`; it is not a standalone
pipeline record or separately mutable file.

`GeneratedMediaRecordGuide.md` Section 4 is the single canonical authority for
the complete `visualBrief` schema, including `contentUsage`, field order,
hash payload, required/optional fields, and unknown-field rejection. This guide
does not duplicate that field list. Authoring performs schema-field parity
validation against that section before writing a v2 record. Existing
`generated_media_prompt_v1` records are read-only compatibility inputs: never
add a visual brief to them or modify them in place.

Fields that are not applicable use an explicit registered enum/value owned by
the artifact contract; do not encode missing evidence as empty prose. A field
required by the artifact or profile but unsupported by planning blocks before
the brief is embedded.

### 5.3 Deterministic identity and storage

Calculate:

```yaml
visualContractHashPayload:
  schemaVersion: generated_media_visual_contract_hash_payload_v1
  requestId:
  assetType:
  domainType:
  contentId:
  planningSnapshotHash:
  registryVersion:
  registryRowId:
  profileId:
  profileVersion:
  guideContractVersion: generated_media_visual_prompt_authoring_v1
```

Use canonical JSON and canonical visualBrief hash rules from
`GeneratedMediaRecordGuide.md`.

```text
visual_contract_hash = SHA256(canonical_json(visualContractHashPayload))
visualBriefId = gmvisual.{assetType}.{contentId}.{visual_contract_hash_prefix_16}
```

The same inputs must reproduce the same `visualBriefId`. Different bytes for an
existing identity are `visual_brief_collision`. The brief and its evidence map
are stored only inside the corresponding immutable prompt JSON and rendered as
a non-copy-ready audit section in its `.prompt.md`. Only the provider payload
fence is copy-ready.

## 6. Common Design Normalization Rules

### 6.1 Observable required and prohibited statements

Convert each planning element into one independently observable statement.

Good statements name what a reviewer can see:

```text
one full-body figure holds the approved polearm in the right hand
the icon contains one centered broken bronze bell silhouette
no Latin letters, numerals, logos, or watermark are visible
```

Reject statements that only name an abstract goal:

```text
looks powerful
feels traditional
shows the skill correctly
has a good background
```

Do not split one fact into repeated prompt emphasis unless separate observations
are required. Do not combine unrelated mandatory facts into one untestable
sentence.

### 6.2 Constraint IDs and evidence

Assign deterministic IDs without changing source order:

```text
planning.required.{zero_padded_index}
planning.prohibited.{zero_padded_index}
master.{section_slug}.{rule_slug}
profile.{registryRowId}.{rule_slug}
runtime.{contract_id}.{rule_slug}
```

Every normalized statement and provider prompt sentence records:

```yaml
constraintId:
statement:
sourceType: master | planning | common | profile | runtime
sourcePath:
sourcePointerOrSection:
sourceSha256:
planningFactId: when available
providerTargets: []
```

One provider sentence may reference multiple constraint IDs. A required
statement with no evidence returns `missing_visual_evidence`. A sentence that
cannot be traced to at least one authority is invented and must not be emitted.

### 6.3 Primary subject and hierarchy

- declare exactly one primary subject, silhouette, action, or scene focus;
- make its defining shape and required meaning readable before supporting detail;
- supporting elements may reinforce the primary subject but must not compete in
  scale, contrast, saturation, edge sharpness, or narrative weight;
- if planning requires multiple co-primary subjects, preserve their stated
  relationship and hierarchy rather than forcing one to disappear;
- do not promote a likely wrong object, decorative motif, background prop, or
  effect particle into the primary subject.

### 6.4 Composition

Composition may express only approved spatial facts and registered technical
constraints. Preserve direction, order, framing, scale relationship, camera,
safe-area, and display-readability requirements when supplied. If composition
or camera changes depicted meaning and is absent, return
`missing_composition_specification` rather than inventing it.

### 6.5 Palette and material

- use planning-specified colors and materials exactly;
- apply Master Concept grounding and registered profile constraints without
  inventing a false historical material or symbolic color meaning;
- do not infer an elemental palette, grade color, faction color, costume
  material, lighting color, or item material from a name alone;
- when a registered profile owns only a technical palette limit, it may constrain
  the supplied colors but may not select their semantic identity;
- missing required color/material evidence returns
  `missing_palette_material_specification`.

### 6.6 Background policy

Normalize one explicit policy:

```text
detailed | constrained | symbolic | flat | transparent | none
```

Describe background content only when planning supplies it. A registered profile
may require a technical transparent, flat, or low-detail treatment for asset
readability, but it cannot invent a location or symbolic scene. Background
contrast and detail remain subordinate to the primary subject. If the profile
requires a choice and neither planning nor the registered rule owns it, return
`missing_background_policy`.

### 6.7 Exclusions and likely wrong objects

`prohibitedVisualStatements` contains every approved prohibition plus applicable
Master Concept prohibitions. `likelyWrongObjects` is a minimal provider-risk list
derived only from known artifact/profile failure patterns. It must:

- contain only objects whose accidental appearance would materially change the
  approved image;
- cite the profile evidence that establishes the failure pattern;
- never become a generic long negative list;
- never introduce an unapproved alternative design merely by naming it;
- remain separate from mandatory planning prohibitions.

### 6.8 Text, UI, logo, and watermark

Unless planning and the active registered profile explicitly require a verified
in-world textual element, generated media must exclude UI chrome, captions,
speech bubbles, labels, modern logos, signatures, and watermarks. Do not render
unverified pseudo-Hangul, foreign script, or invented historical insignia. This
rule supplements, and never weakens, Master Concept cultural prohibitions.

## 7. Artifact-specific Rules

### 7.1 character_main_image

Owns: provider-neutral organization of an approved character identity and
appearance for a main image and exact eight-way identity-consistent rotation
contract.

Does not own: character identity, gender presentation, biological sex, body,
face, costume, equipment, weapon, palette, status, pose meaning, target display
size, detail density, cultural detail, or rotation invention.

Required planning fields:

```text
characterIdentity
appearanceSpecification containing approved genderPresentation, body/face/hair, costume,
  equipment, weapon, handedness, palette/material, identifying features,
  pose policy, and intendedDisplay with targetDisplaySize and detailDensity
rotationContract.orderedDirections
rotationContract.exactCount=8
rotationContract.identityConsistencyRequired=true
requiredElements / prohibitedElements
contentUsage
exact registered character profile
```

When the upstream source is ProjectBS canonical character planning, it must
validate against
`AgentDocs/planning-guides/character/data-structures/CharacterPlanningDataGuide.md`.
`planningStatus=approved`, main-image readiness, empty `missingDesignInputs`,
and per-fact provenance are mandatory. Prompt authoring cannot repair a legacy
or blocked planning file.

Visual priority and composition:

- preserve the full approved identity through one readable body silhouette;
- keep gender presentation, face, body proportions, costume, equipment, handedness, and palette
  consistent across every direction;
- rotation directions are technical views of one identity, not eight design
  variations;
- apply transparent/background and camera treatment only from the registered
  profile or explicit planning;
- do not infer biological sex, gender presentation, a hero pose, combat pose,
  social class, faction ornament, weapon detail, target size, or detail density
  from the character name or other semantic metadata;
- use the planning-owned target display size/detail density for readability;
  provider canvas/export dimensions remain profile-owned technical settings.

Provider handoff: one `pixellab_character_prompt_v1` field payload plus exact
settings intent and an `ordered_rotation_set` expectation. The authoring task
does not generate or inspect the eight outputs.

Validation:

- all appearance statements map to character planning evidence;
- no appearance statement originates only from name, personality, combat lore,
  skill, role, grade, or tag;
- exactly eight approved ordered directions exist;
- no direction changes identity, equipment, handedness, or palette;
- provider text does not claim generation/evaluation completion.

### 7.2 character_animation

Owns: one prompt translation for one externally requested character animation.

Does not own: a fixed Attack/Idle/Move set, action design, anticipation, impact,
recovery, direction, mirroring, frame count, timing, loop, or runtime movement.

Required planning fields:

```text
characterProviderIdentity
approvedCharacterPackageId when policy requires it
one exact animationRequestId
animationType: attack | idle | move
actionSpecification
directionOrder
frameContract
mirroringPolicy
requiredElements / prohibitedElements
```

Visual priority and motion/frame rules:

- preserve the approved character identity above motion embellishment;
- express only the requested visible action and ordered direction/frame facts;
- do not create omitted animation types or fill missing motion phases;
- keep runtime-owned translation, targeting, collision, or rotation out of the
  generated frames unless planning explicitly assigns it to generated motion;
- background treatment follows the approved character/profile contract and does
  not become a new scene.

Provider handoff: one `pixellab_character_animation_prompt_v1` action payload per
requested animationRequestId with `ordered_frame_set` expectation.

Validation:

- one request produces one prompt record;
- all action words map to actionSpecification;
- direction/frame/mirroring facts are complete and unchanged;
- no unrequested Attack/Idle/Move prompt exists.

### 7.3 icon

Owns: a domain-neutral icon hierarchy for one approved central subject and
semantic effect.

Does not own: skill effect, item meaning, grade symbolism, exact count,
background necessity, palette identity, border meaning, or domain-specific
symbol choice.

Required planning fields:

```text
iconProfile ID/version
subjectIdentity
semanticEffect
requiredElements / prohibitedElements
exactCountElements when applicable
backgroundPolicy
targetDisplayContract
```

Visual priority and composition:

- make one approved silhouette/object/emblem the dominant readable subject;
- connect one or more effects only as planning requires and keep them subordinate;
- preserve approved orientation, count, palette, material, background, and
  small-display readability;
- use profile-owned outline, pixel density, safe-area, or palette-count rules as
  technical constraints, not semantic design;
- do not turn an icon into a character portrait, inventory card, badge, scenic
  illustration, text mark, or unrelated collection of objects.

Provider handoff: ordered `pixellab_icon_prompt_v1` field text with
`single_image` expectation. Deterministic frame/background normalization, when
registered, remains outside provider prompt authorship.

Validation:

- central subject and effect are separately observable;
- required exact counts remain exact and are owned by planning or deterministic
  processing, never guessed by prose;
- background policy has an authority;
- skill/item differences enter through exact registry profile, not prompt copy.

### 7.4 general_animation

Owns: provider-neutral translation of one character-independent object/VFX
sequence into reference-state and action fields.

Does not own: gameplay effect meaning, subject design, sequence stages, loop,
frame layout, timing, runtime movement, targeting, collision, or reference image.

Required planning fields:

```text
animationProfile ID/version
animationSubject
sequenceStages in exact order
loopMode
frameContract
runtimeBoundary.generatedMotion
runtimeBoundary.runtimeOwnedMotion
referenceImageContract with source hash
requiredElements / prohibitedElements
```

Visual priority and motion/frame rules:

- the approved animationSubject remains recognizable through every stage;
- the reference field describes only the approved starting state;
- the action field preserves exact sequence order and ending/loop behavior;
- do not add anticipation, impact, dissipation, secondary particles, camera
  movement, or runtime-owned motion unless explicitly planned;
- transparency/background and sheet layout are profile/runtime technical facts.

Provider handoff: separate `reference_image_description` and `animation_action`
fields under `pixellab_animation_prompt_v1`, with
`paired_sheet_animation` expectation.

Validation:

- every stage has evidence and deterministic order;
- loop, frame, runtime boundary, and reference hash are complete;
- generatedMotion and runtimeOwnedMotion do not overlap;
- the asset is not a character animation.

### 7.5 imagegen_image

Owns: evidence-preserving assembly of one approved scene into a cohesive
ImageGen prompt.

Does not own: depicted moment, subjects, relationships, location, environment,
composition, camera, aspect ratio, background, story clue, battle readability,
lighting meaning, or narrative detail.

Required planning fields:

```text
imageProfile ID/version
depictedMoment
subjects and relationships
environment
composition
camera
aspectRatio
backgroundPolicy
requiredElements / prohibitedElements
```

Visual priority and composition:

- place the approved depicted moment and subject relationship first;
- preserve planned framing, camera, scale, spatial hierarchy, environment, and
  background policy;
- apply stage or battle visual treatment only from the exact registered profile;
- keep supporting environment subordinate to the planned focus;
- do not reconstruct a scene from story prose, battle ID, content name, mood
  tag, or previous image when the required scene fields are absent.

Provider handoff: one `imagegen_composed_scene_prompt_v1` assembled in this
order: subject/moment; composition/camera/space; environment/background; approved
art direction/palette/material/lighting; concise exclusions. It produces a
`single_image` expectation.

Validation:

- every scene section maps to approved planning or exact profile evidence;
- the scene prompt contains one coherent moment, not multiple story beats;
- no PixelLab field fragments, evaluator language, project path, or invented
  camera/environment appears.

## 8. Provider Translation Boundary

The normalized brief is provider-neutral. Translation is deterministic:

```text
character_main_image -> pixellab_character_prompt_v1
character_animation -> pixellab_character_animation_prompt_v1
icon -> pixellab_icon_prompt_v1
general_animation -> pixellab_animation_prompt_v1
imagegen_image -> imagegen_composed_scene_prompt_v1
```

PixelLab uses exact UI field payloads in the order owned by the profile. Use
short, literal, silhouette/action-first language. Do not turn fields into
cinematic prose or repeat settings in each field.

ImageGen uses one cohesive `scenePromptOriginal` compiled from auditable
sections. Do not split it into PixelLab fragments. Provider translation may
reorder only as declared above; it cannot omit a required statement or add a
new visual fact.

## 9. State, Failure, and Retry

State transition:

```text
validated planning handoff
-> profile_verified
-> visual_brief_normalized
-> provider_payload_authored
-> prompt_record_ready
```

Any blocker ends before prompt record creation. Do not persist a partial ready
brief or update an index on failure.

Failure types:

```text
missing_visual_authority
missing_visual_evidence
ambiguous_visual_fact
material_visual_contract_conflict
unsupported_visual_profile
visual_profile_registry_conflict
visual_profile_version_mismatch
missing_artifact_visual_specification
missing_primary_subject
missing_composition_specification
missing_palette_material_specification
missing_background_policy
visual_invention_required
visual_evidence_map_incomplete
provider_translation_contract_failed
visual_brief_identity_mismatch
visual_brief_collision
```

`safeToRetry=true` only when the planning owner supplies corrected immutable
evidence or a reviewed registry/profile revision is activated. Rewording a
prompt, using another provider, or searching unrelated content cannot resolve a
planning or authority blocker.

## 10. Validation

Before setting a prompt record to `ready_for_generation`:

- verify Master Concept was read and every applicable prohibition is preserved;
- verify `routingRecordFile`, routing identity/hash, selected pipeline/prompt,
  exact selected registry row/profile, normalized request, and planning handoff
  identity/hash all agree;
- verify exact visualBrief schema-field parity with GeneratedMediaRecordGuide
  Section 4, including required `contentUsage` and unknown-field rejection;
- recompute visualBriefId and provider payload hash;
- verify every required/prohibited/provider statement has evidence and a stable
  constraint ID;
- verify planningOriginal, normalized brief, provider payload, and settings are
  separate;
- verify exactly one primary hierarchy and an explicit background policy;
- verify supporting elements do not compete with the primary subject;
- verify likelyWrongObjects is minimal and profile-evidenced;
- verify no planning-owned fact was inferred or repaired;
- verify the artifact-specific contract and provider handoff profile;
- verify the prompt record contains no provider result, downloaded media,
  evaluation verdict, project promotion, Unity, Slack, Git, or deployment state.

Validation succeeds only when `visualBrief.status=normalized` and the prompt
record validation lists the exact guide/profile versions, constraint coverage,
computed IDs, and hashes.

Missing, stale, hash-mismatched, or ambiguous routing evidence returns
`missing_routing_record`, `stale_routing_record`,
`routing_record_mismatch`, or `ambiguous_routing_record`; no brief or prompt
record is written.

## 11. Versioning, Maintenance, and New Domains

This guide owns common normalization and artifact contracts. Registry rows own
activated domain/profile combinations. Domain rules remain inactive until:

```text
approved planning contract
-> exact profile guide and registry version
-> prompt/guide evaluation
-> registry activation
```

Add a registry/profile row, not a copied domain execution prompt. A new domain
must define planning fields, visual profile authority, provider translation,
preservation/evaluation structure, and validation before activation.

Version rules:

- wording clarification that does not change normalized output may retain the
  guide contract version;
- changed required brief fields, constraint semantics, artifact behavior, or
  provider mapping requires a new guide contract version;
- changed domain rendering behavior requires a new profileVersion;
- added/removed route rows require a new registryVersion;
- existing immutable prompt records retain stored versions and are never
  silently reinterpreted;
- migration requires revalidation against the original planning snapshot and
  creates a new prompt record.

## 12. Related Documents

```text
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaAuthoringProfileRegistryGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md
AgentDocs/planning-guides/content/generated-media/ImageGenPipelineGuide.md
```
