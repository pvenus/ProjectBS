# Ability Content Presentation Display Catalog

## Status

- Canonical document date: 2026-08-13
- Scope: player-facing Character, Skill, Effect, Bless, and Relic presentation labels, tokens, formats, and filtering
- Source contract: preserve one source field per Entry and one value per Entry
- Localization contract: names and descriptions keep their existing `StringManager` paths; Group, Entry, Tag, enum replacement, and value-format text use canonical `presentation.*` main keys with sub-key `name`
- Debug contract: inspection output retains raw keys and unfiltered authored values

## Display Pipeline

```text
approved SO / runtime data
-> domain Presentation resolver
-> raw semantic Group, Entry, Tag, and token keys
-> player-display allowlist and conditional-value filter
-> PresentationDisplayCatalog localization keys
-> PresentationLocalizedTextResolver.ResolveRequired(...)
-> probe ordered candidates with StringManager.Get(..., returnNullIfMissing: true)
-> on total miss, normal StringManager.Get(first intended key, sub-key)
-> UIContentInfoView
```

The catalog never invents gameplay numbers and never combines two source fields into one Entry. An unapproved raw JSON or C# key with no catalog mapping is omitted. Once a field is approved and mapped, a missing localization row renders the full intended `mainKey.subKey` so the localization defect remains visible. The existing asset-name fallback used when `StringManager` itself is unavailable is unchanged.

## Missing-Key Boundary

- No approved catalog mapping: omit the raw gameplay key from player UI.
- Approved mapped key or required name/description key, but no matching StringManager row: display the full first intended `mainKey.subKey`.
- Multiple candidate main keys: probe them in order with `returnNullIfMissing: true`; use the first resolved text, or expose only the first intended key after every candidate fails.
- Never replace a missing key with generated Pascal-case text or prose inferred from gameplay values.

## Localization Key Families

| Purpose | Key family | Example |
| --- | --- | --- |
| Group title | `presentation.group.*` | `presentation.group.special_effect` |
| Entry label | `presentation.entry.*` | `presentation.entry.effect_distance` |
| Tag label | `presentation.tag.*` | `presentation.tag.Active` |
| Enum/token replacement | contextual family | `presentation.damage.Normal`, `presentation.control.Stun` |
| Value format | `presentation.format.*` | `presentation.format.control_type` |
| Stat replacement | `presentation.stat.*` | `presentation.stat.StunDuration` |

All of these rows live in `Assets/Resources/string/presentation_string.csv`. Do not create numbered keys such as `Skill.Hit.1.Damage` and do not use a generated Pascal-case phrase in the player path.

## Skill Identity and Tags

| Source | Raw presentation value | Player policy | Final text source |
| --- | --- | --- | --- |
| `equipmentId` | identity ID | inspection/provenance only | none |
| `skillName` / Unity asset name | source identity fallback | keep existing behavior only | existing Skill name resolution |
| `LocalizationMainKey.name` | identity name | always when localized | existing `StringManager` path; strategic Skills retain the confirmed item-key fallback |
| `LocalizationMainKey.desc` | description | when authored | existing `StringManager` path |
| `baseProfile.skillType` | `Active`, `Passive` | player Tag | `presentation.tag.<value>` |
| `baseProfile.skillComponentType` | component token | inspection only | none |
| `baseProfile.brainMeta.category` | category token | player Tag when approved | `presentation.tag.<value>` |
| `baseProfile.brainMeta.targetType` | target token | player Tag when approved | `presentation.tag.<value>` |
| `baseProfile.brainMeta.tacticalNeed` | AI decision token | inspection only | none |

The 20 current files under `Assets/Resources/skill/json/` contain `Active`, `Projectile`, categories `Attack/Buff/Control/Heal`, targets `Ally/Enemy`, and tactical needs `None/AllySupport/AreaControl`. Only Skill type, category, and target are eligible for the player Tag list.

## Skill Entries

Policy terms:

- **Visible**: included when the source concept exists and passes the stated condition.
- **Conditional**: default, zero, disabled, or unbounded sentinel values are omitted only by `ResolveForPlayerDisplay()`.
- **Inspection only**: retained by `Resolve()` and the Editor inspection tool, never sent to the player View.

| Source JSON/SO field | Raw Entry key | Skill group | Player policy | Label key |
| --- | --- | --- | --- | --- |
| `cast.targetingType` | `targetingType` | Activation | visible unless `None` | `presentation.entry.targeting` |
| `cast.cooldown` | `cooldown` | Activation | conditional: greater than zero | `presentation.entry.cooldown` |
| `cast.castTime` | `castTime` | Activation | conditional: greater than zero | `presentation.entry.cast_time` |
| `cast.range` | `range` | Activation | conditional: range-using targeting, positive, below `999` | `presentation.entry.range` |
| `cast.burst.count` | `burst.count` | Delivery | conditional: greater than one | `presentation.entry.burst_count` |
| `cast.burst.interval` | `burst.interval` | Delivery | conditional: positive and burst count greater than one | `presentation.entry.burst_interval` |
| cast movement type | `castMove.moveType` | Delivery | visible unless `None` | `presentation.entry.cast_move_type` |
| cast movement distance | `castMove.distance` | Delivery | conditional: positive | `presentation.entry.cast_move_distance` |
| cast movement duration | `castMove.duration` | Delivery | conditional: positive | `presentation.entry.cast_move_duration` |
| `baseProfile.projectileCount` | `projectileCount` | Delivery | conditional: greater than one | `presentation.entry.projectile_count` |
| `baseProfile.projectileScale` | `projectileScale` | Delivery | inspection only | none |
| `baseProfile.projectileColliderRadius` | `projectileColliderRadius` | Delivery | conditional: positive and below `999` | `presentation.entry.effect_range` |
| `baseProfile.projectileLifetime` | `projectileLifetime` | Delivery | conditional: positive | `presentation.entry.duration` |
| `baseProfile.projectile.arrangement` | `projectile.arrangement` | Delivery | visible when arrangement data is meaningful | `presentation.entry.projectile_arrangement` |
| `baseProfile.projectile.arrangementValue` | `projectile.arrangementValue` | Delivery | inspection only | none |
| `baseProfile.projectile.spreadAngle` | `projectile.spreadAngle` | Delivery | conditional: positive | `presentation.entry.spread_angle` |
| `baseProfile.projectile.radius` | `projectile.radius` | Delivery | conditional: positive | `presentation.entry.arrangement_radius` |
| `baseProfile.projectileSpawn.spawnOffset` | `projectileSpawn.spawnOffset` | Delivery | inspection only | none |
| `baseProfile.projectileSpawn.interval` | `projectileSpawn.interval` | Delivery | conditional: positive | `presentation.entry.projectile_spawn_interval` |
| `move.moveType` | `moveType` | Delivery | inspection only | none |
| `move.config.speed` | `config.speed` | Delivery | inspection only | none |
| `move.config.turnSpeed` | `config.turnSpeed` | Delivery | inspection only | none |
| orbit/follow movement config | `config.orbitRadius`, `config.orbitAngularSpeed`, `config.clockwise`, `config.followOffset.x/y` | Delivery | inspection only | none |
| `hits.targetLayerMask` | `targetLayerMask` | Delivery | inspection only | none |
| `hits.damage.damageType` | `damage.damageType` | Outcome | visible when the hit has meaningful damage | `presentation.entry.damage_type` |
| `hits.damage.baseDamage` | `damage.baseDamage` | Outcome | conditional: positive | `presentation.entry.base_damage` |
| first-hit damage SO field | `damage.firstHitBaseDamage` | Outcome | conditional: positive; preview only when runtime omits it | `presentation.entry.first_hit_damage` |
| `hits.damage.attackPercentDamage` | `damage.attackPercentDamage` | Outcome | conditional: positive | `presentation.entry.attack_scaling` |
| `hits.damage.canCritical` | `damage.canCritical` | Outcome | conditional: show only `true` | `presentation.entry.can_critical` |
| `hits.damage.ignoreDefense` | `damage.ignoreDefense` | Outcome | conditional: show only `true` | `presentation.entry.ignore_defense` |
| `hits.maxHitCount` | `maxHitCount` | Delivery | conditional: positive and below `999` | `presentation.entry.max_hit_count` |
| `hits.hitStartTime` | `hitStartTime` | Delivery | inspection only | none |
| `hits.repeatInterval` | `repeatInterval` | Delivery | conditional: positive | `presentation.entry.repeat_interval` |
| split-hit count | `split.hitCount` | Delivery | conditional: greater than one | `presentation.entry.split_hit_count` |
| split-hit interval | `split.hitInterval` | Delivery | conditional: positive | `presentation.entry.split_hit_interval` |
| spawn Character reference | `character` | Outcome | visible; value is the existing localized Character name | `presentation.entry.summoned_character` |
| spawn count | `spawnCount` | Outcome | conditional: greater than one | `presentation.entry.spawn_count` |
| spawn interval | `spawnInterval` | Outcome | conditional: positive and spawn count greater than one | `presentation.entry.spawn_interval` |
| spawn lifetime | `spawnLifeTime` | Outcome | conditional: positive | `presentation.entry.spawn_lifetime` |

The current strategic JSON does not contain cast-movement, split-hit, or spawn fields, but current SO definitions support them. They remain source-backed capabilities and must not be synthesized into current assets.

## Effect Entries

The seven typed Outcomes remain the normalization model. Skill composition routes Activation to `Activation`, ordinary outcomes to `Outcome`, Control/Displacement to `SpecialEffect`, and SkillInvoke to `LinkedSkill`.

| Normalized source field | Raw Entry key | Player policy | Label/token key |
| --- | --- | --- | --- |
| activation trigger | `Activation.Trigger` | visible when present | `presentation.entry.activation_trigger`, `presentation.trigger.<value>` |
| activation chance ratio | `Activation.chance` | visible when present | `presentation.entry.activation_chance` |
| activation chance percent | `Activation.chancePercent` | visible when present | `presentation.entry.activation_chance` |
| activation target | `Activation.Target` | visible when present | `presentation.entry.activation_target`, `presentation.target.<value>` |
| critical requirement | `Activation.RequiresCriticalHit` | visible when true | `presentation.entry.critical_condition`, `presentation.boolean.True` |
| Stat type | `StatModifier.Stat` | visible | `presentation.entry.stat`, `presentation.stat.<value>` |
| Stat operation | `StatModifier.Operation` | visible | `presentation.entry.operation`, `presentation.operation.<value>` |
| Stat value | `StatModifier.value` | visible | `presentation.entry.modifier_value` |
| timed Stat duration | `StatModifier.durationSeconds` | visible when present | `presentation.entry.duration` |
| maximum-health heal ratio | `Heal.maxHpPercent` | visible when present | `presentation.entry.max_health_ratio` |
| flat heal | `Heal.flatHealAmount` | visible when present | `presentation.entry.heal_amount` |
| attack heal scaling | `Heal.attackPercentHeal` | visible when present | `presentation.entry.attack_scaling` |
| clamp-to-max-health | `Heal.ClampToMaximumHealth` | inspection only | none |
| cooldown change kind | `CooldownChange.Kind` | visible | `presentation.entry.cooldown_change_type`, `presentation.cooldown_change.<value>` |
| cooldown ratio | `CooldownChange.reducePercent` | visible when present | `presentation.entry.cooldown_reduction_ratio` |
| cooldown seconds | `CooldownChange.reduceSeconds` | visible when present | `presentation.entry.cooldown_reduction_time` |
| displacement direction | `Displacement.Direction` | visible | `presentation.entry.displacement_type`, `presentation.displacement.<value>` |
| displacement force | `Displacement.force` | visible when present | `presentation.entry.effect_magnitude` |
| displacement distance | `Displacement.distanceMeters` | visible when present | `presentation.entry.effect_distance` |
| periodic attack ratio | `PeriodicDamage.attackRatioPercent` | visible when present | `presentation.entry.attack_scaling` |
| periodic per-tick ratio | `PeriodicDamage.attackRatioPercentPerTick` | visible when present | `presentation.entry.attack_scaling_per_tick` |
| periodic rate unit | `PeriodicDamage.RateUnit` | visible | `presentation.entry.periodic_rate`, `presentation.periodic_rate.<value>` |
| tick interval | `PeriodicDamage.tickIntervalSeconds` | visible when present | `presentation.entry.interval` |
| periodic duration | `PeriodicDamage.durationSeconds` | visible when present | `presentation.entry.duration` |
| invoked Skill | `SkillInvoke.Skill` | visible; value is the existing localized Skill name | `presentation.entry.linked_skill` |
| invocation range | `SkillInvoke.Range` | visible when present | `presentation.entry.effect_range` |
| Control kind | `Control.Kind` | visible | `presentation.entry.control_type`, `presentation.control.<value>` |
| Control duration/value | `Control.value` or `Control.duration` | visible when present | `presentation.entry.duration` |
| EffectEntry duration | `duration` | visible when meaningful | `presentation.entry.duration` |
| maximum apply count | `maxApplyCount` | visible when meaningful | `presentation.entry.max_apply_count` |
| category, lifetime, status | `categoryType`, `lifetimeType`, `status` | inspection only | none |

`ValueOverride`, `hasValueOverride`, unapplied upgrade modifiers, and any value not consumed by the current runtime resolver remain excluded from normalized player data.

## Damage, Control, and Displacement Formats

The raw enum is never shown directly in the player View. The Entry selects both a replacement-key family and a format key.

| Entry | Replacement key | Format key | Current Korean examples |
| --- | --- | --- | --- |
| `damage.damageType` | `presentation.damage.<DamageType>` | `presentation.format.damage_type` | 일반 피해, 폭발 피해, 지속 피해, 고정 피해 |
| `Control.Kind` | `presentation.control.<EffectControlKind>` | `presentation.format.control_type` | 기절, 속박, 도발 |
| `Displacement.Direction` | `presentation.displacement.<EffectDisplacementDirection>` | `presentation.format.displacement_type` | 밀쳐내기, 끌어당기기, 투사체 방향 이동, 지정 방향 이동 |

The current format row is `{0}` because the UI already renders a separate localized Entry label. Changing the CSV format can wrap or decorate the replacement text without changing code or source data.

## Character Display List

Current approved authoring source: the 22 JSON files under `Assets/Resources/character/json/`.

| JSON/SO source | Player policy | Localization/output |
| --- | --- | --- |
| `name` / `LocalizationMainKey` | visible | existing `characterId` + `name` StringManager path in `character_string.csv` |
| `characterType` | visible as a Tag | `presentation.tag.<CharacterType>` |
| `job` | visible as one source-backed Tag | `presentation.tag.<CharacterJob>`; do not replace it with independently generated family/tier/branch rows |
| `baseStats[].statType` + `value` | visible as one Entry per source Stat | `presentation.stat.<StatType>` label and the source numeric value |
| `characterId` | identity/provenance only | not rendered as a player Entry or Tag |
| generated Animation Clip references | inspection only | no player label |
| generated Skill references and `slotKey` | inspection and Skill-tab composition only | not duplicated in the Character information body |
| derived Job family/tier/branch | inspection only | no player label while the authored JSON contains one `job` field |

Current Character JSON Stats are `Attack`, `Defense`, `MaxHp`, `AttackSpeed`, `CritChance`, `CritDamage`, and `MoveSpeed`. Runtime evidence defines `CritChance` and `CritDamage` as percent values, `MoveSpeed` as meters per second, and `AttackSpeed` as a multiplier. The multiplier format is localized by `presentation.format.multiplier`; the source number is not changed.

The comparison tool at `Assets/Editor/tools/character/CharacterPresentationPreviewWindow.cs` shows `Original JSON`, `SO Inspection (all)`, and `Player UI (filtered)` side by side. It also reports JSON/SO mismatch for ID, type, job, ordered Stats, numeric values, and the StringManager-backed name.

## Owned Effect Inventory Display List

Every acquired General Bless and every owned Relic applies immediately. Neither has an equipment state in the authoritative design. The inventory may also show active Bless-backed Faith features, while the separate Faith encyclopedia owns progression and future unlock information.

| Content/source | Player policy | Localization/output |
| --- | --- | --- |
| Bless/Relic name and description | keep existing behavior | existing `LocalizationMainKey` through `StringManager` |
| Bless category, duration type, god | player Tags when catalogued | `presentation.tag.<value>` |
| raw Bless authoring tags | only values explicitly approved by the catalog | otherwise skipped |
| Bless battle duration | visible | `presentation.entry.bless_duration_battles` |
| Bless runtime level and remaining battles | visible when present | `presentation.entry.bless_level`, `presentation.entry.remaining_battles` |
| Bless runtime `isEquipped` / `isSelected` | stale or non-authoritative runtime fields; inspection only | do not create equipment UI |
| Relic rarity | player Tag | `presentation.tag.<value>` |
| Relic raw category/subcategory | inspection only until a product vocabulary is approved | none |
| Relic ownership | determines inclusion in the owned list | all owned Relics are active |
| `EquippedRelics` / Relic equipped state | current runtime mismatch; inspection only | do not filter player display by equipment |
| active Bless-backed Faith feature | included in the Owned Effects Faith section | resolved through the Bless player-display path |
| inactive or future Faith feature | Faith encyclopedia Preview only | excluded from Owned Effects |
| Exclusive Job Change | Faith encyclopedia only | exclude unless a future explicit Effect source is authored |
| Bless/Relic Effects | use the same normalized Effect catalog | keys above |

The current screen role is one tabless Owned Effects inventory. One vertical scroll contains categorized sections for currently applied owned Relics, acquired General Blesses, and active Faith Blesses; the page never uses `Catalog`. Relic and General Bless catalogs are separate pages that may reuse the common category/item rendering in `Catalog` mode. The Faith encyclopedia owns full Faith progression and future unlocks. Every Owned Effects selection binds to one neutral `UIContentInfoView`; do not create another owned-only Bless page or duplicate the Faith progression track.

The separate General Bless encyclopedia remains a planned `Catalog` page; no `GeneralBlessCatalogPresenter` exists yet. Its approved contract is to display every unique non-null `BlessSO` supplied explicitly or through `BlessPoolSO.Blessings`, mark only definitions whose authored `BlessingId` occurs in the supplied active `BlessRuntimeData.BlessEntry` list as active, and keep inactive definitions visible and selectable. Pool weight and progression step remain inspection/generation data and are not displayed.

`RelicCollectionView` remains separate for a future Relic encyclopedia containing acquired and unacquired entries, locked silhouettes, and owned/total counts. Do not reuse or generalize that codex View as the Owned Effect inventory.

Current runtime ownership must be reconciled before implementation: `RelicItemService.EquippedRelics` conflicts with the no-equipment Relic design, and `BlessManager.AddBless` currently replaces an existing permanent Common Bless. No legacy Bless or Relic asset is migrated or edited by this display catalog.

## Maintenance Rule

When a new displayed source field or enum value is added:

1. Confirm the field exists in current runtime/SO data and record its provenance.
2. Add one raw Entry or Tag key without numbering or derived values.
3. Decide player-visible, conditional, or inspection-only policy explicitly.
4. Add the canonical localization mapping to `PresentationDisplayCatalog`.
5. Add every required `presentation.*` row to `presentation_string.csv`.
6. Extend validation and rerun the user-owned Unity tests.

Do not silently fall back to a generated word in the player View. A missing catalog mapping omits the unapproved raw field. A missing StringManager row for an approved mapping or required name/description is a validation failure and must remain visible as the full intended localization key until corrected.
