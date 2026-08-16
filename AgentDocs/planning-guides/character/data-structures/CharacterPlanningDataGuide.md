# Character Planning Data Guide

## 1. Purpose and Authority

Guide Type: schema/data-structure. This guide is the single canonical authority
for new per-character planning JSON shared by `Player`, `Npc`, and `Boss`.

Authority order:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
-> approved character planning under this schema
-> generated_media_planning_handoff_v2
-> GeneratedMediaVisualPromptAuthoringGuide.md
-> provider generation
```

Master Concept owns period, culture, aesthetics, materials, and prohibitions.
Story and approved planning sources own character facts. This guide owns the
planning JSON shape and readiness decision. Generated Media may translate
approved facts and add registered technical structure; it may not design the
character.

This guide does not define CharacterSO serialization, stats, skill balance,
provider prompts, image generation, evaluation, or project promotion.

Contract version:

```text
schemaVersion: character_planning_v2
documentType: characterPlanning
```

## 2. Canonical Storage and Identity

```text
Player:
  AgentDocs/planning-data/character/act-plans/player/{characterId}.json

Npc or Boss:
  AgentDocs/planning-data/character/act-plans/{actGroupId}/npc/{characterId}.json
```

`characterId` uses `character.{name}.{grade}`. All three character types use
`identity.runtimeDomain=character`. The `npc` folder is an authoring boundary,
not a runtime domain.

`documentId` is `character_planning.{characterId}`. A different payload at an
existing canonical identity is a conflict; never overwrite it silently.

## 3. character_planning_v2

New writes reject unknown fields. Required top-level fields:

```yaml
schemaVersion: character_planning_v2
documentId: character_planning.{characterId}
documentType: characterPlanning
planningStatus: draft | blocked | approved
commonDataRef: exact project-relative path
identity: {}
provenance: {}
appearance: {}
combat: {}
planningScore: {}
stats: {}
skills: []
generatedMediaPlanning: {}
missingDesignInputs: []
notes: []
```

The existing `identity`, `combat`, `planningScore`, `stats`, `skills`, and
`commonDataRef` semantics remain owned by their current domain guides and
runtime contracts. Migration must preserve their values unless their own
authority approves a change.

State transitions are closed: `draft -> blocked | approved` and
`blocked -> draft` after the planning owner supplies the recorded decisions.
An approved document is not edited silently; an intentional revision returns
to `draft`, records the revised provenance, passes validation again, and then
becomes `approved`. No downstream Generated Media stage may change planning
state or resolve a blocker.

### 3.1 Identity

Required:

```yaml
identity:
  characterId: character.{name}.{grade}
  characterType: Player | Npc | Boss
  runtimeDomain: character
  name: non-empty display name
  grade: integer >= 1
  tierId: current planning tier
  role: current planning role
  tags: []
```

Character type never changes the runtime domain or character-owned skill ID
domain.

### 3.2 Provenance

Required:

```yaml
provenance:
  sourceStoryRefs:
    - exact project-relative source path
  sourcePlanningRefs:
    - exact project-relative source path
  factEvidence:
    - factId: stable per-character fact identity
      fieldPath: exact JSON pointer owned by this planning file
      sourceRefs:
        - path: exact project-relative source path
          sourcePointerOrSection: exact JSON pointer or Markdown section
```

Every appearance fact and observable requirement cites at least one evidence
entry. A source path alone does not authorize unstated detail.

Do not store this mutable planning file's own SHA-256, a snapshot hash that
includes itself, or a self-referential revision inside the planning JSON. The
later handoff producer hashes the completed approved file and stores it under
`sourcePlanningFiles`, then creates an immutable external planning snapshot.

### 3.3 Structured appearance

All keys are required. A draft/blocked document uses `null` only for an
unresolved scalar/object and records the same path in `missingDesignInputs`.
An approved document contains no null or empty required appearance value.

```yaml
appearance:
  genderPresentation:
    description: non-empty observable visual gender presentation
  body:
    description: observable body shape, proportions, and age presentation
  face:
    description: observable face shape and approved distinguishing details
  hair:
    description: observable style, length, shape, and approved color
  costume:
    description: observable garments, layers, and condition
  equipment:
    - description: observable non-weapon equipment
  weapon:
    present: true | false
    description: required when present=true
  handedness: left | right | ambidextrous | none
  palette:
    - colorDescription: approved visible color
      appliesTo: exact body/costume/equipment/weapon target
  materials:
    - materialDescription: approved visible material
      appliesTo: exact costume/equipment/weapon target
  identifyingFeatures:
    - observable feature that must remain identity-consistent
  posePolicy:
    mode: exact | neutral_provider_pose | profile_owned_technical_pose
    description: required for exact; omitted otherwise
  intendedDisplay:
    assetType: character_single_image
    outputUsage: non-empty player-facing use
    targetDisplaySize:
      unit: px
      minimumPixelSize: {width: integer >= 1, height: integer >= 1}
      targetPixelSize: {width: integer >= 1, height: integer >= 1}
      consumerViewportOrReference: exact runtime viewport, UI slot, or approved display reference
    detailDensity:
      level: low | medium | high | exact
      description: required planning-owned visible-detail limit
    providerCanvasPolicy: profile_owned_technical_canvas
    framingPolicy: approved framing or profile_owned_technical_framing
    backgroundPolicy: detailed | constrained | flat | transparent | none | profile_owned_technical_background
```

Requiredness rules:

- `genderPresentation`, `body`, `face`, `hair`, `costume`, `weapon`, `handedness`, `palette`,
  `materials`, `identifyingFeatures`, `posePolicy`, and `intendedDisplay` are
  required for `character_single_image` readiness;
- `equipment` may be empty only when planning explicitly establishes that the
  character has no non-weapon equipment;
- `weapon.present=false` requires `handedness=none` unless another approved
  held object establishes handedness;
- palette/material facts may be constrained by Master Concept but must not be
  invented from name, personality, role, combat, tier, or faction;
- `genderPresentation.description` is a visible presentation decision, not a
  biological-sex assertion. It must have `factEvidence` and must never be
  inferred from name, personality, role, voice, costume stereotype, or combat;
- `targetDisplaySize` describes the minimum and normal in-game consumer size,
  not the provider canvas/export resolution. The target must be at least the
  minimum in both dimensions. `detailDensity` is owned by planning so authoring
  cannot independently choose how much identity detail must survive that size;
- a profile-owned technical policy may choose only presentation mechanics. It
  cannot choose costume, identity, semantic pose, location, or cultural detail.

### 3.3.1 Character fact and expression-profile boundary

This schema owns character facts only. The sole normative ProjectBS character
expression-profile authority is
`GeneratedMediaVisualPromptAuthoringGuide.md::Canonical Character Expression Profile`;
`CharacterDesignCreateGuide.md` only supplies the planning workflow. Do not copy reusable style
sentences into appearance fields as if they were character identity.

- approved planning owns gender/age presentation, face, hair, costume,
  equipment, weapon, palette, material, pose and identifying features;
- authoring may convert those approved facts into observable sentences but may
  not add youth, attractiveness, modern/westernized beauty, minor-coded or
  sexualized presentation, facial hair, fatigue, age, or gravitas;
- the reusable expression layer must supply separate positive and negative
  style locks or the exact closed profile-specific policy projection with
  profile ID/version and evidence coverage;
- planning may select only an exact registered `expressionProfileKey` and its
  reviewed payload hash as an approved pointer. The current sparse-ink option is
  `projectbs_character_sparse_ink_pastel_motion@1.0.0` with hash
  `b5ce18d11e249598ad6da13d59340cf7cede3d2896259dcc5e02dbbf98e80443`;
  the single-image-only bold-outline option is
  `projectbs_character_bold_outline_compressed_detail@1.0.0` with hash
  `dc5db9990f26dd1ed0ebc25c6c2b46a10b68cb4ca3248e69f7c27b28e1568b33`;
  its accepted-result-aligned successor is
  `projectbs_character_bold_outline_compressed_detail@2.0.0` with hash
  `5702307bebf466b8e6190b5d881bd57f38373746f02084fdcf5e348e7fc88db3`; the successor is a distinct explicit selection and never
  reinterprets v1;
  the additive single-image-only open ink-wash option is
  `projectbs_character_open_ink_wash_dynamic_contour@1.0.0` with hash
  `37ba4df4af5f8fa4b45708bd18bebbec537ad58a74ab8d00f722c7c4744817dd`.
  Planning may select it only with exact 4-5-head/4.25-target young-adult facts,
  35-55/45-target open contour, tactile mok-seon, broad bleeding pigment,
  separate blue-gray-or-indigo/gray-brown/small-muted-ochre roles, at least 70
  percent achromatic or unpainted figure-interior and canvas space, warm-ivory
  removable background, no halo/scene/shadow, and complete character-specific
  identity/equipment bindings. The accepted style-reference SHA is audit-only
  and is never a planning identity fact or edit target;
  its separate output-conformance successor is
  `projectbs_character_open_ink_wash_dynamic_contour@2.0.0` with hash
  `b0510a47827ba4b4d53f19220091799b6870b259ed23ef850dafde6444aeb6f5`.
  Planning selects v2 only as a new exact pointer; it does not upgrade v1
  records. V2 additionally requires the exact cranial-mass/chin/sole proportion
  measurement definition, simplified surface-detail boundary, uniform warm-
  ivory/no-radial-background rule, and mandatory post-output conformance receipt;
  actual planning/handoff revision remains the planning owner's work;
- a conflict between an approved character fact and the active expression
  profile returns `character_style_profile_conflict` and requires an explicit
  planning/profile revision. Downstream stages never silently restyle planning.

#### Open ink-wash planning-owned projection

For a new decision created after this projection contract is published, an
explicit selection of
`projectbs_character_open_ink_wash_dynamic_contour@1.0.0` also requires this
closed member under `generatedMediaPlanning.characterSingleImage`:

```yaml
openInkWashPlanningProjection:
  schemaVersion: character_open_ink_wash_planning_projection_v1
  fullBodyHeadCount: exact JSON number 4.25
  contourOmissionTargetPercent: exact JSON number 45
  negativeSpaceMinimumPercent:
    figureInterior: exact JSON number from 70 through 100
    fullCanvas: exact JSON number from 70 through 100
  paletteRoleAnchors:
    primaryCool: non-empty unique ordered exact character element/site labels
    secondaryEarth: non-empty unique ordered exact character element/site labels
    smallWarmAccent: non-empty unique ordered exact character element/site labels
  generationBackground:
    mode: removable_solid
    color: exact approved warm-ivory color
  backgroundExclusions:
    halo: true
    vignette: true
    scene: true
    shadow: true
  styleReferenceFidelity:
    mode: semantic_text_projection_only
    auditOnlySha256: b02550dd37f152346be7f9aa33884ae3cc790a5f956d496f420c23ecbdfd93cf
    providerReferenceAuthorized: false
```

Unknown members are rejected. The projection stores planning-owned exact
targets and character-specific anchors; the immutable expression-profile
payload continues to own the reusable ranges, mok-seon phases, pigment policy,
and style locks. The selected full-body target is `4.25`, the selected contour
target is `45`, and both negative-space floors are `70` for the current
approved direction. The generation-background value must equal
`singleImageSpecification.generationBackground`, and every exclusion must
agree with `prohibitedElements` and `singleImageSpecification.noShadow`.

Every scalar and each complete anchor array in this object is captured as its
own `approvedFacts` entry using a leaf JSON pointer. A whole prose description,
the whole projection object, or a combined required/prohibited sentence does
not replace these leaf facts. This makes the numeric and background gates
machine-comparable without asking downstream roles to parse Korean or English
prose.

`semantic_text_projection_only` records a material fidelity limitation: the
accepted raster may inform planning review, but its bytes are not available to
the provider and no visual/composition match to that raster is promised. If
the user requires the generated image to match or closely follow the selected
raster rather than only its reviewed textual semantics, the planning is
`blocked` with `character_style_profile_conflict` until a separate reviewed
durable project-relative style-only reference contract is published. Do not
mark the request ready, invent a path, or weaken the requested fidelity. This
rule does not retroactively rewrite or invalidate an immutable handoff already
published on the selected authoritative baseline.

For a new decision that uses the published reviewed raster, use the additive
projection schema below. The v1 projection above remains valid and unchanged
for semantic-text-only decisions.

```yaml
openInkWashPlanningProjection:
  schemaVersion: character_open_ink_wash_planning_projection_v2
  fullBodyHeadCount: exact JSON number 4.25
  contourOmissionTargetPercent: exact JSON number 45
  negativeSpaceMinimumPercent: {figureInterior: 70..100, fullCanvas: 70..100}
  paletteRoleAnchors:
    primaryCool: non-empty unique ordered exact labels
    secondaryEarth: non-empty unique ordered exact labels
    smallWarmAccent: non-empty unique ordered exact labels
  generationBackground: {mode: removable_solid, color: exact warm-ivory value}
  backgroundExclusions: {halo: true, vignette: true, scene: true, shadow: true}
  styleReferenceFidelity:
    mode: durable_style_only_binding
    providerReferenceAuthorized: true
    binding:
      role: style_only
      projectRelativePath: exact durable asset path
      sha256: exact raw asset SHA-256
      reviewRecordId: exact gmstyleref1 ID
      reviewRecordPath: exact durable review-record path
      reviewRecordSha256: exact raw review-record SHA-256
```

The v2 projection is allowed only for exact open ink-wash v1 or v2 profile
selection after every asset/review/index check in
GeneratedMediaStyleReferenceBindingGuide.md passes. The binding is style
evidence, never part of `identityConsistencyLock`, required character elements,
or equipment facts. Capture its six leaves separately in `approvedFacts`, then
copy the same closed object as the handoff's single `styleReferenceBindings`
entry. Any difference is `planning_snapshot_mismatch`; any identity, pose,
action, clothing, equipment, or edit-target role is
`style_reference_semantic_transfer_forbidden`.

### 3.4 Generated Media planning readiness

An attack-animation may approve the composed
`projectbs_character_bold_outline_attack_motion_flow@1.0.0` successor only as
eight planning-owned facts: `motionDirection`, `swordArc`, `torsoRotation`,
`shoulderInertia`, `hemInertia`, `darkNeutralInkTrajectory`, `keyPoseOrder`, and
`frameContinuityAnchors`. They belong to one exact `animationRequestId` and do
not alter `generatedMediaPlanning.characterSingleImage`, its v8 meaning, or any
immutable prompt record. Missing or conflicting facts are not inferred by the
router or authoring stage.

```yaml
generatedMediaPlanning:
  characterSingleImage:
    readiness: ready | blocked | not_requested
    expressionProfileKey: optional exact registered character expression-profile key
    openInkWashPlanningProjection: # required only for an exact open-ink-wash v1/v2 selection after the applicable projection publication; forbidden otherwise
      schemaVersion: character_open_ink_wash_planning_projection_v1 | character_open_ink_wash_planning_projection_v2
      fullBodyHeadCount: exact JSON number 4.25
      contourOmissionTargetPercent: exact JSON number 45
      negativeSpaceMinimumPercent: {figureInterior: JSON number 70..100, fullCanvas: JSON number 70..100}
      paletteRoleAnchors: {primaryCool: [unique labels], secondaryEarth: [unique labels], smallWarmAccent: [unique labels]}
      generationBackground: {mode: removable_solid, color: approved warm-ivory color}
      backgroundExclusions: {halo: true, vignette: true, scene: true, shadow: true}
      styleReferenceFidelity: v1 semantic_text_projection_only branch above | v2 durable_style_only_binding branch above
    styleReferenceBindings: # optional only with projection v2; exactly one entry; forbidden otherwise
      - {role: style_only, projectRelativePath:, sha256:, reviewRecordId:, reviewRecordPath:, reviewRecordSha256:}
    expressionProfileProjection: # required only for projectbs_character_bold_outline_compressed_detail@1.0.0; v2 uses the successor extension below; forbidden otherwise
      fullBodyHeadCount: JSON number from 4 through 5
      externalOutlineSourcePx: JSON integer from 16 through 22 for the approved 1024x1536 source canvas
      internalLineSourcePx: positive JSON number; externalOutlineSourcePx / internalLineSourcePx >= 2
      facialMarkBudget:
        countingUnit: one_continuous_visible_mark_between_pen_lifts_or_intentional_breaks
        maximumTotalMarks: JSON integer from 1 through 9
        componentMaximums: {browsAndEyes: integer 0..4, nose: integer 0..1, mouth: integer 0..1, jawAndFaceShape: integer 0..3}
      primaryHue: non-empty exact character-specific hue
      secondaryHue: optional non-empty exact character-specific hue
      primaryAnchorElements: non-empty unique ordered exact element/site IDs
      secondaryAnchorElements: required non-empty unique ordered exact element/site IDs only when secondaryHue is present; forbidden otherwise
      secondaryAnchorSiteClasses: # v2 only; required with secondaryHue and forbidden without it
        - small_utility_pouch | small_travel_accessory
      maximumCharacterCoveragePercent: JSON integer from 1 through 35
      maximumColorMasses: JSON integer from 1 through 4
      neutralOutlineColor: non-empty exact neutral color
      neutralWeaponColor: non-empty exact neutral color
      detailMarkBudget: # v2 only; required exact closed object
        countingUnit: one_continuous_visible_dark_line_segment_between_pen_lifts_or_intentional_breaks
        maximumTotalVisibleMarks: JSON integer from 1 through 64
        maximumInternalLineMarks: JSON integer from 0 through 56 and no greater than maximumTotalVisibleMarks
        maximumSecondaryFoldMarksPerGarmentRegion: JSON integer from 0 through 5
      inkHalo: # v2 only; exact discriminated union; disabled is exactly {enabled:false}
        enabled: false | true
        color: enabled-only non-empty exact dark-neutral color
        maximumOpacity: enabled-only JSON number from 0.08 through 0.35
        maximumCanvasCoveragePercent: enabled-only JSON number from 1 through 45
        centerPolicy: enabled-only character_silhouette_center
        extentPolicy: enabled-only single_centered_soft_halo_behind_silhouette
        edgeFalloff: enabled-only soft_monotonic_to_zero_alpha
        edgeAlpha: enabled-only exact JSON number 0
        noScene: enabled-only true
        noOpaqueBackground: enabled-only true
        noShadowSubstitute: enabled-only true
        noDirectionalCastShadow: enabled-only true
    requiredElements:
      - factId: stable requirement ID
        statement: independently observable visual sentence
        evidenceFactIds: [factId]
    prohibitedElements:
      - factId: stable prohibition ID
        statement: independently observable exclusion sentence
        evidenceFactIds: [factId]
    identityConsistencyLock:
      identityId: exact identity.characterId
      referenceFacts:
        - sourcePointer: exact resolvable JSON pointer in this planning file
          evidenceFactIds: non-empty ordered fact IDs
    singleImageSpecification:
      viewpoint: one approved viewpoint
      pose: one approved pose
      framing: approved framing
      canvas: {width: integer >= 1, height: integer >= 1}
      targetDisplaySize: {width: integer >= 1, height: integer >= 1}
      safeArea: complete approved safe-area contract
      finalBackgroundPolicy: complete approved final policy
      generationBackground: {mode: removable_solid, color: approved color}
      noShadow: true | false
      outline:
        enabled: true | false
        color: required only when enabled=true; forbidden when enabled=false
        exactThicknessPx: required positive integer only when enabled=true; forbidden when enabled=false
        placement: outside_silhouette
      anchor:
        type: pelvis_root_ground_axis
        pelvisOrRootPoint: approved point
        groundContactAxis: approved ground-contact axis
```

`requiredElements` and `prohibitedElements` are planning decisions. They cannot
be produced downstream from a name, personality, combat lore, skill, role tag,
or likely visual convention. An explicitly empty prohibited list is invalid
unless planning records a source-evidenced `no_prohibitions` decision.

`expressionProfileKey` is optional for backward compatibility. When absent,
the registry applies its immutable legacy-compatible character profile. When
present, it is an explicit approved planning fact: the value must be one exact
registered key, must have `factEvidence` to an approved planning decision, and
must be projected unchanged into the handoff snapshot `approvedFacts`. Unknown
or conflicting selection returns `character_style_profile_conflict`; no
downstream stage may add, alias, or repair this value.

For the open ink-wash key, readiness additionally requires the exact
`openInkWashPlanningProjection` above and leaf-level approved-fact capture.
Missing or mismatched projection returns
`missing_open_ink_wash_profile_projection` or
`open_ink_wash_profile_projection_mismatch`; missing leaf evidence returns
`open_ink_wash_profile_evidence_incomplete`. A selected-raster fidelity request
combined with `semantic_text_projection_only` returns
`character_style_profile_conflict`. These are no-handoff outcomes and do not
authorize a provider reference or a new visual decision.

For `projectbs_character_bold_outline_compressed_detail@1.0.0`, the displayed
`expressionProfileProjection` member set is closed and every scalar/list/object
must be captured as exact approved facts. The target outline equivalent is
derived only as `externalOutlineSourcePx * 3 / 32`; planning does not store a
second independently rounded target thickness. Component facial maxima sum to
no more than `maximumTotalMarks`. Primary and secondary anchors must identify
where their corresponding hue appears; a hue without anchors, anchors without
a hue, duplicate anchors, full-garment fill, or a coverage/mass value outside
the profile bounds blocks before handoff publication. The profile constants are
not character identity facts and are referenced separately by profile key/hash.

For `projectbs_character_bold_outline_compressed_detail@2.0.0`, the v1 members
remain required and the member set additionally closes `detailMarkBudget`,
conditional `secondaryAnchorSiteClasses`, and `inkHalo`. Planning binds exact
mark maxima no greater than 64 total, 56 internal, and 5 secondary folds per
garment region. Optional ochre is legal only when its exact elements and site
classes are bound to `small_utility_pouch` or `small_travel_accessory`.
`inkHalo` is either exactly `{enabled:false}` or the enabled closed branch with
dark-neutral color, opacity 0.08-0.35, canvas coverage 1-45 percent,
`character_silhouette_center`, one centered soft extent, monotonic fade to
alpha zero, and true no-scene, no-opaque-background, no-shadow-substitute, and
no-directional-cast-shadow assertions. None of these values may be inferred
from raster evidence or prompt prose.

`characterSingleImage` is the only current planning source for
`assetType=character_single_image`, `domainType=character`. It describes one
approved image and must not contain `rotationPolicy`, `directions`, a direction
array, a rotation count, animation/variant requests, PixelLab identity, or
`ordered_rotation_set`.

Existing `generatedMediaPlanning.characterMainImage` data with
`rotationPolicy=generated_media_exact_8_way_v1` is a legacy v1 contract. It
remains immutable/read-only evidence and is owned only by the legacy profile
with this exact direction order:

```text
[north, north_east, east, south_east, south, south_west, west, north_west]
exactCount=8
identityConsistencyRequired=true
```

That legacy expansion adds no character meaning. It is not eligible for a new
v2 handoff and must never be collapsed, upgraded, or copied into
`characterSingleImage`. A current request requires an independently approved
current contract. Legacy rotation fields in the current contract return
`legacy_record_not_current_request` and produce no handoff.

### 3.5 Missing design inputs

```yaml
missingDesignInputs:
  - fieldPath: exact unresolved JSON pointer
    failureType: missing_gender_presentation | missing_body_design | missing_face_design | missing_hair_design |
      missing_costume_design | missing_equipment_decision |
      missing_weapon_design | missing_handedness_decision |
      missing_palette_design | missing_material_design |
      missing_identifying_features | missing_pose_policy |
      missing_display_contract | missing_target_display_size |
      missing_detail_density | missing_required_elements |
      missing_prohibited_elements | missing_design_provenance
    requiredDecision: decision the planning owner must supply
    sourceRefsChecked: []
    blocks:
      - character_single_image_handoff
```

`planningStatus=approved` and `readiness=ready` require an empty
`missingDesignInputs`. Missing facts are blockers, not permission to generate a
plausible design.

## 4. Generated Media Handoff Production

Write a separate `generated_media_planning_handoff_v2` only after the canonical
planning file is approved and ready. Do not embed the handoff, source hash, or
snapshot hash back into the mutable planning file.

Character-planning producer storage:

```text
AgentDocs/planning-data/character/generated-media-handoffs/v2/{contentId}/{requestId}.character_single_image.json
```

`contentId` must equal `identity.characterId`. The planning producer derives
`requestId`, `capturedAt`, and ordered sources under
`GeneratedMediaPlanningHandoffGuide.md::Deterministic producer-owned planning
capture`; no caller approval object exists. The same immutable current decision,
source bytes, and approved facts reproduce canonically equal handoff bytes.
Existing different bytes at the same path return central `record_collision`;
never overwrite them.

Existing immutable v2 handoffs on the selected authoritative baseline retain
their historical request identity and are not reprojected. Only newly produced
handoffs use the `gmplan2.` derivation; a new legacy-form record is forbidden.

For an intentional approved revision, the character-planning producer also
owns collision-free decision identity. Scan the existing character visual-
decision filenames, treat the unversioned historical file as revision 1, and
select exactly one greater than the greatest existing numeric `.vN` suffix.
The new identity and path are:

```text
decisionId=character_visual_design_decision.{characterId}.v{N}
AgentDocs/planning-data/character/design-decisions/v1/{characterId}.visual-design.v{N}.json
```

Publish the decision first with atomic no-clobber. Stamp
`approval.approvedAt` once as the decision producer's current RFC 3339 time with
an explicit numeric offset. If another writer occupies the proposed revision,
re-read the directory and select the next numeric revision before any canonical
planning write. A retry reuses an identical existing decision and never
refreshes its timestamp. The canonical planning then appends that exact path to
`provenance.sourcePlanningRefs`; handoff capture derives source order from that
array and derives `requestId` from the completed snapshot hash.

Mapping:

| Handoff field | Character planning source |
| --- | --- |
| `schemaVersion` | constant `generated_media_planning_handoff_v2` |
| `requestId` | exact `gmplan2.{assetType}.{contentId}.{snapshotHash[0:20]}` derivation |
| `assetType` | constant `character_single_image` after readiness approval |
| `domainType` | constant `character` |
| `contentId` | `identity.characterId` |
| `contentUsage` | `appearance.intendedDisplay.outputUsage` |
| `identityConsistencyLock` | exact current lock; each sourcePointer and evidenceFactId must resolve |
| `singleImageSpecification` | exact complete current specification, with no defaults or downstream completion |
| `requiredElements` | ordered `.statement` values |
| `prohibitedElements` | ordered `.statement` values |

After every source is complete, hash its exact UTF-8 bytes and include its
project-relative path, role, and lowercase SHA-256 in ordered
`sourcePlanningFiles`. Include the canonical planning file and every separate
decision file whose facts or technical values are projected. `revision` is
optional per source: include it only when that source's owning system supplies
a stable non-empty revision string, unchanged from the authority. Never invent
one from a timestamp, Git state, file hash, or this planning file. When no
authoritative revision exists, omit the key; the SHA-256 and immutable snapshot
remain mandatory. Every approved fact must carry a resolvable source path plus
an RFC 6901 JSON pointer. Build the immutable snapshot/hash using only
`GeneratedMediaPlanningHandoffGuide.md::Closed Planning Snapshot v2`; do not
redefine its approvedFacts or hash payload. Apply GeneratedMediaRecordGuide.md
for shared JCS/file-byte rules. Re-read
and re-hash sources immediately before publication. Missing provenance,
unresolved pointers, changed bytes, hash/snapshot mismatch, or incomplete type
specification writes no partial handoff.

If any required mapping is absent, return `character_planning_not_media_ready`
and the applicable current failure token from
GeneratedMediaImageGenOnlyContractGuide.md section 8.1; do not write a handoff.

Other contract-level failures are `missing_character_identity`,
`invalid_character_type`, `invalid_character_planning_schema`,
`missing_design_provenance`, `planning_snapshot_mismatch`, and the central
`record_collision`. A failure never authorizes partial
handoff output or replacement of an existing different artifact.

## 5. Legacy Read and Migration

Files without `schemaVersion=character_planning_v2` are legacy read-only
inputs. Preserve recognized `commonDataRef`, `identity`, `combat`,
`planningScore`, `stats`, `skills`, and notes. Do not treat a short legacy
`appearance` object as a complete visual design.

Classification:

```text
legacy_complete: every v2 fact can be mapped with exact provenance
legacy_incomplete: one or more v2 design/provenance fields are absent
legacy_conflict: legacy facts contradict an authority or each other
```

Only `legacy_complete` may enter an explicitly reviewed migration. Never bulk
overwrite legacy files. A migration creates a review candidate, preserves the
source bytes/hash, lists every mapping, and requires owner approval before the
canonical path is replaced. New characters write v2 directly.

`legacy_incomplete` returns `missingDesignInputs`; `legacy_conflict` returns
`legacy_character_planning_conflict`. Neither is eligible for a Generated Media
handoff.

## 6. Seojin Legacy Case Classification

The file
`AgentDocs/planning-data/character/act-plans/player/character.seojin.1.json`
is an analysis example, not a schema template.

Its current bytes contain approved appearance/provenance facts and a legacy
`generatedMediaPlanning.characterMainImage.rotationPolicy` contract. Those
bytes are evidence, not a current schema example. They do not contain the
independently approved `characterSingleImage.identityConsistencyLock` and
complete `singleImageSpecification` required by the v2 handoff.

Downstream tasks are forbidden to derive those missing technical/planning
decisions from the name `서진`, its appearance facts, a previous provider
result, or the legacy direction contract. The current file is therefore blocked
for a new `character_single_image` handoff until the separate character-planning
migration owner approves and writes the current fields. This guide and the
handoff producer do not modify that planning JSON or its design-decision JSON.

## 7. Validation

- schema version, document type, identity, path, and character type agree;
- existing combat/runtime fields remain valid under their owning guides;
- every approved appearance and observable statement has provenance;
- no required appearance field, gender presentation, target display size, or
  detail-density decision is empty for an approved document;
- `missingDesignInputs` and planning/readiness states agree;
- Player/Npc/Boss use only `runtimeDomain=character`;
- current `characterSingleImage` contains one viewpoint and no legacy rotation,
  direction, animation, variant, PixelLab, or ordered-rotation-set field;
- legacy exact 8-way data remains isolated and cannot produce a current handoff;
- outline conditional presence is exact: disabled forbids color/thickness;
- every identity reference pointer and evidence fact resolves;
- planning JSON contains no self-referential hash;
- handoff source hashes and the immutable JCS snapshot are recalculated and
  verified only after all sources are complete;
- legacy files are not silently overwritten or marked media-ready;
- no downstream prompt/provider step is authorized to fill missing design.

## 8. Related Documents

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
AgentDocs/planning-guides/character/ActCharacterPlanningStartGuide.md
AgentDocs/planning-guides/character/CharacterDesignCreateGuide.md
AgentDocs/planning-guides/character/CharacterCreateGuide.md
AgentDocs/planning-guides/character/CharacterStatGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaPlanningHandoffGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaImageGenOnlyContractGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
```
