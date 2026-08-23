# Ability Content Presentation Contract Evaluation

## Status

- Evaluation date: 2026-08-12
- Scope: current Effect normalization, Skill-to-shared-group conversion, and player-display localization catalog
- JSON evidence: all 20 files under `Assets/Resources/skill/json/`
- Static compile: `Assembly-CSharp-Editor.csproj` completed with 0 errors and 191 existing warnings after the player-display catalog change
- Unity Editor validation: pending user execution

## Latest Authoritative Contract

`EffectPresentationData` contains identity, optional Activation, exactly one normalized Outcome, authored description, entry constraints, provenance, and resolution status.

The seven Outcome types are:

- `StatModifierPresentationData`
- `HealPresentationData`
- `CooldownChangePresentationData`
- `DisplacementPresentationData`
- `PeriodicDamagePresentationData`
- `SkillInvokePresentationData`
- `ControlPresentationData`

This latest decision supersedes the intermediate design that merged Heal and Periodic Damage into one `HealthChangePresentationData` type.

## JSON Evidence

The 20 strategic Skill JSON files do not contain a property named `tags`. Their explicit classification fields are:

- `baseProfile.skillType`
- `baseProfile.skillComponentType`
- `baseProfile.brainMeta.category`
- `baseProfile.brainMeta.targetType`
- `baseProfile.brainMeta.tacticalNeed`
- `hits[].targetLayerMask`
- `hits[].buffEffects[]/debuffEffects[].effect.effectType`
- `hits[].buffEffects[]/debuffEffects[].categoryType`
- `hits[].buffEffects[]/debuffEffects[].lifetimeType`

The current inventory contains 20 Skills, one Hit per Skill, and 18 Effect entries. Effect normalization must use the approved Outcome types; Skill classification tags use the source categorical values without invented prefixes.

## Corrected Display Conversion

- Skill UI group keys are the five approved semantic roles: `Activation`, `Delivery`, `Outcome`, `SpecialEffect`, and `LinkedSkill`.
- The seven Effect Outcome types remain internal typed normalization. They are routed into Skill groups instead of creating one UI group per Effect.
- Cast/target/trigger/chance fields route to `Activation`; projectile/movement/burst/hit-cadence fields route to `Delivery`; damage/heal/stat/cooldown/periodic/spawn results route to `Outcome`; `Control` and `Displacement` route to `SpecialEffect`; `SkillInvoke` routes to `LinkedSkill`.
- `PresentationGroupData.SourceContentId` remains available to standalone Effect groups and detail navigation; it is not reconstructed or used as an invented Skill group label.
- One source field becomes one `PresentationEntryData` with one value.
- Labels may translate source fields into player wording, but values are not merged, recomputed, or replaced.
- Authored numeric values keep their source number and source unit. `Chance=0.25 Ratio` may format as `25%`, but the normalized data remains `0.25 Ratio`; `ChancePercent=25` remains `25 Percent`.
- Authored normalization does not clamp numbers, substitute minimums, convert Ratio data into Percent data, or synthesize counts. Runtime-resolved values are allowed only from an explicit runtime source and must carry runtime provenance.
- Removed display inventions include `Skill.Hit.1.Damage`, `Skill.Effect.Self.1`, `Behavior`, `CountAndScale`, and `SizeAndLifetime`.
- Raw source-field and normalized-component keys remain inspection/provenance data. Player Group, Entry, Tag, enum replacement, and value-format text resolve only through explicit `PresentationDisplayCatalog` mappings to canonical `presentation.*` rows in `Assets/Resources/string/presentation_string.csv`.

## Known Source Boundary

Generated SOs do not preserve every JSON identity or property-presence marker, including Effect `entryId` and Hit `damageId`. Presentation must not reconstruct these values from naming conventions. It uses only values and IDs preserved in current SOs. Optional structures that cannot be proven meaningful are omitted from player display.

## Remaining Plan-Consistency Findings

- Aligned: the inspection window uses the complete `Resolve()` path, so authored/default values such as `0` and `999` remain visible there. Player UI alone uses `ResolveForPlayerDisplay()`.
- Aligned: View hierarchy references use `AutoBindPrefix`/`AutoBind`, while prefab template asset references remain explicitly assigned.
- Not merged: `Build Presentation` and the assigned `EquipmentSkillSO` still live in `SkillContentInfoPresenter`, not directly in `UIContentInfoView`. Moving them into the shared View would make root Presentation depend on the concrete Skill domain, which conflicts with the content-neutral dependency rule. This requires an explicit final ownership decision rather than a silent merge.
- Not proven: the prefab has a vertical `ScrollRect`, connected Content/Viewport, `RectMask2D`, vertical layout, and content-size fitting. The Viewport has no raycastable `Graphic`, so pointer-wheel/drag input can still fail where no active child Graphic receives raycasts. An active Scene `EventSystem` is also required. The user must verify this in Unity and, if necessary, add a transparent raycast-target `Image` to the Viewport.
- Deferred by decision: nested Skill detail composition remains outside the current pass.

## Verification Required in Unity

1. Run `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`.
2. Run `Tools > ProjectBS > Presentation > Run Skill Asset Validation`.
3. In Play Mode, invoke `Build Presentation` on the configured Skill content component.
4. Verify a strategic Skill such as `skill.strategic.golden_chain_formation`:
   - `projectileColliderRadius` and `projectileLifetime` appear as separate rows.
   - No numbered Skill/Effect group title appears.
   - Visible groups are limited to the five semantic Skill group names.
   - Stun/Root/Taunt appear under `SpecialEffect` as `Control` kind plus source-backed duration when available; knockback/pull appear as `Displacement` direction plus force or distance according to the source Config.
   - Localized labels come from `presentation_string.csv`.
5. Verify wheel and drag input over an Entry area that has no active detail button. If it does not scroll, add a transparent raycast-target `Image` to the Viewport in Unity and retest.

The agent does not run or control Unity Editor; the user owns these checks.

## 2026-08-12 Display-Catalog Re-evaluation

- The previous rule that labels use raw source paths described the internal Entry keys, not player text. Player text now resolves an explicit canonical `presentation.*` key through `StringManager`.
- Existing Skill/Bless/Relic name and description resolution, including its established fallback behavior, remains unchanged.
- Player Group, Entry, Tag, DamageType, ControlType, Displacement direction, and value-format text use `PresentationDisplayCatalog` mappings. Missing mappings do not fall back to raw keys or generated Pascal-case text.
- The five Skill groups and seven internal Effect Outcomes remain unchanged. This work changes display vocabulary and player filtering, not normalization.
- The complete source-field classification is recorded in `AgentDocs/Machal/ability-content-presentation-display-catalog.md`.
- Static verification: Editor assembly build completed with 0 errors and 191 existing warnings; the localization CSV contains 294 data rows, 154 `presentation.*` main keys, zero duplicate key pairs, and zero missing keys among 141 statically required catalog keys.
- Unity Effect/Skill validation and final visual checks remain user-owned and pending after this display-contract change.

## 2026-08-12 Missing Localization Key Visibility Correction

- This correction supersedes the earlier active conclusion that missing mapped localization text should be omitted.
- Catalog approval and localization resolution are separate gates. A raw gameplay key without an approved mapping remains filtered; an approved mapped key missing from StringManager is rendered as its full intended `mainKey.subKey` for debugging.
- `PresentationLocalizedTextResolver.ResolveRequired` probes ordered candidate main keys with `returnNullIfMissing: true`. It returns the first resolved text; after every candidate fails, it performs normal lookup on the first intended key so StringManager exposes that full key.
- Skill, Effect, Bless, and Relic descriptions use the same required lookup. Their established candidate paths and order remain intact, and no structured values are converted into replacement prose.
- The asset-name fallback when StringManager is unavailable is unchanged.
- Static verification completed with 0 errors and 191 existing project warnings. User-owned Unity visual verification of a temporary nonexistent mapped key remains pending; the key must be restored after the check.
