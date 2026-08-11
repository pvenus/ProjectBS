# Character Planning Data Guide

## 1. Purpose and Authority

Guide Type: schema/data-structure. This guide is the single canonical authority
for new per-character planning JSON shared by `Player`, `Npc`, and `Boss`.

Authority order:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
-> approved character planning under this schema
-> generated_media_planning_handoff_v1
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
    assetType: character_main_image
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
  required for `character_main_image` readiness;
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

### 3.4 Generated Media planning readiness

```yaml
generatedMediaPlanning:
  characterMainImage:
    readiness: ready | blocked | not_requested
    requiredElements:
      - factId: stable requirement ID
        statement: independently observable visual sentence
        evidenceFactIds: [factId]
    prohibitedElements:
      - factId: stable prohibition ID
        statement: independently observable exclusion sentence
        evidenceFactIds: [factId]
    identityConsistencyLocks:
      - exact appearance field path that may not drift across views
    rotationPolicy: generated_media_exact_8_way_v1
```

`requiredElements` and `prohibitedElements` are planning decisions. They cannot
be produced downstream from a name, personality, combat lore, skill, role tag,
or likely visual convention. An explicitly empty prohibited list is invalid
unless planning records a source-evidenced `no_prohibitions` decision.

The rotation policy means that the Generated Media handoff may expand the
technical contract to exactly:

```text
[north, north_east, east, south_east, south, south_west, west, north_west]
exactCount=8
identityConsistencyRequired=true
```

That expansion adds no character meaning. Every identity, appearance,
equipment, handedness, palette, and identifying-feature lock comes from the
approved planning file.

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
      - character_main_image_handoff
```

`planningStatus=approved` and `readiness=ready` require an empty
`missingDesignInputs`. Missing facts are blockers, not permission to generate a
plausible design.

## 4. Generated Media Handoff Production

Write a separate `generated_media_planning_handoff_v1` only after the canonical
planning file is approved and ready. Do not embed the handoff, source hash, or
snapshot hash back into the mutable planning file.

Character-planning producer storage:

```text
AgentDocs/planning-data/character/generated-media-handoffs/v1/{contentId}/{requestId}.character_main_image.json
```

`contentId` must equal `identity.characterId`. `requestId` must be stable,
project-safe, and supplied by the caller. The same request and planning snapshot
must reproduce canonically equal handoff bytes. Existing different bytes at the
same path are `character_planning_handoff_collision`; never overwrite them.

Mapping:

| Handoff field | Character planning source |
| --- | --- |
| `assetType` | constant `character_main_image` after readiness approval |
| `domainType` | constant `character` |
| `contentId` | `identity.characterId` |
| `contentName` | `identity.name` |
| `contentUsage` | `appearance.intendedDisplay.outputUsage` |
| `characterIdentity` | approved `identity` plus identity locks only |
| `appearanceSpecification` | approved structured `appearance`, including gender presentation and target display/detail-density contract |
| `requiredElements` | ordered `.statement` values |
| `prohibitedElements` | ordered `.statement` values |
| `rotationContract` | technical expansion of `rotationPolicy` |

After the planning file is complete, calculate its SHA-256 and include its
project-relative path, role, and hash in `sourcePlanningFiles`. `revision` is
optional per source: include it only when that source's owning system supplies
a stable non-empty revision string, unchanged from the authority. Never invent
one from a timestamp, Git state, file hash, or this planning file. When no
authoritative revision exists, omit the key; the SHA-256 and immutable snapshot
remain mandatory. Build `planningSnapshot` according to
GeneratedMediaPlanningHandoffGuide.md. Snapshot/hash identity belongs to the
separate immutable handoff.

If any required mapping is absent, return `character_planning_not_media_ready`
and do not write a handoff.

Other contract-level failures are `missing_character_identity`,
`invalid_character_type`, `invalid_character_planning_schema`,
`missing_design_provenance`, `planning_snapshot_hash_mismatch`, and
`character_planning_handoff_collision`. A failure never authorizes partial
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

Available facts include its existing identity/runtime domain, commonDataRef,
combat intent, planning score, stats, skills, and the limited legacy silhouette,
weapon category, and visual keywords. Its common file supplies group/story
references but does not prove per-field visual decisions.

Planner decisions still required include gender presentation, structured body, face, hair, costume,
equipment decision, exact weapon appearance, handedness, palette/material,
identifying features, pose policy, target display size/detail density and the rest of the intended display contract, observable
required/prohibited elements, and per-fact provenance.

Downstream tasks are forbidden to infer those decisions from the name `서진`,
frontline role, rescue motive, sword skill, tags, grade, or broad silhouette.
The current file is therefore `legacy_incomplete` and blocked for
`character_main_image` handoff until planning supplies approved facts. This
guide does not modify that source JSON.

## 7. Validation

- schema version, document type, identity, path, and character type agree;
- existing combat/runtime fields remain valid under their owning guides;
- every approved appearance and observable statement has provenance;
- no required appearance field, gender presentation, target display size, or
  detail-density decision is empty for an approved document;
- `missingDesignInputs` and planning/readiness states agree;
- Player/Npc/Boss use only `runtimeDomain=character`;
- the 8-way contract contains the exact ordered directions and adds no design;
- planning JSON contains no self-referential hash;
- handoff source hashes are calculated only after planning completion;
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
AgentDocs/planning-guides/content/generated-media/GeneratedMediaVisualPromptAuthoringGuide.md
```
