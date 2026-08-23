# Ability Content Presentation Source Inventory

## Status

- Inventory date: 2026-08-08
- Stage: 1 complete
- Stage 4 validation update: 2026-08-11; all thirteen synthetic Config mappings and twenty approved EffectEntry assets passed
- Scope: read-only inspection of current Skill and Effect source code and approved asset paths
- Runtime code, assets, prefabs, scenes, and project settings changed: none

## Authority and Source Priority

Use sources in this order when deciding what can be presented as active gameplay data:

1. Current runtime resolver behavior
2. Current SO references reachable from an approved `EquipmentSkillSO`
3. Current generated SO fields
4. Authoring JSON, marked as authoring-only provenance
5. Authored descriptions for semantically ambiguous legacy data

Never present a JSON-only value as an active runtime value when the generated SO does not contain or reference it.

## Approved Paths

- `Assets/Resources/skill/character/generated/`
- `Assets/Resources/skill/json/`

The Skill and Effect source paths named by the active task contract also exist. All inspected Skill/Effect source and approved asset paths were clean in the Git baseline. The wider checkout contains many unrelated modified and untracked files and must remain untouched.

## Missing Required Workflow References

- `AgentDocs/code-writing-rules.md`
- `AgentDocs/task-start-documentation-prompt.md`

Stage 1 is documentation-only and did not require script edits. Stage 2 code work must not begin until the code-writing guide is restored or supplied. Documentation-manager handoff cannot use the prescribed request format until the missing prompt is available.

## Current Skill Source Graph

```text
EquipmentSkillSO
|- EquipmentBaseProfileSO
|- SkillCastSO
|  `- self EffectEntrySO[]
|- SkillHitSO[]
|  |- damage profile
|  |- buff EffectEntrySO[]
|  |- debuff EffectEntrySO[]
|  `- nested EquipmentSkillSO
|- SkillMoveSO
|- SpawnSkillSO
|- EquipmentUpgradeTableSO
`- BaseVisualSO

EquipmentSkillResolver
`- EquipmentSkillRuntimeData
   |- resolved level and range
   |- resolved burst count and interval
   |- resolved projectile count, spread, arrangement, and scale
   `- resolved upgrade and visual context
```

Primary source files:

- `Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentSkillSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentBaseProfileSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/cast/SkillCastSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/hit/SkillHitSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/move/SkillMoveSO.cs`
- `Assets/Scripts/Ability/Skills/Definitions/spawn/SpawnSkillSO.cs`
- `Assets/Scripts/Ability/Skills/Runtime/EquipmentRuntimeData.cs`
- `Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs`

Presentation provenance must distinguish direct SO preview data from resolved runtime data. Nested Skill references are supported by the source schema, but no non-null nested Skill reference was found in the approved assets.

## Current Effect Source Graph

```text
EffectEntrySO
|- EffectSO
|  `- EffectConfig serialized reference
|- lifetime and category
|- duration and max apply count
`- value override fields

EffectResolver
`- EffectRuntimeData selected by concrete EffectConfig type
```

Primary source files:

- `Assets/Scripts/Ability/Effects/Definitions/EffectSO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectEntrySO.cs`
- `Assets/Scripts/Ability/Effects/Definitions/EffectEnum.cs`
- `Assets/Scripts/Ability/Effects/Definitions/config/`
- `Assets/Scripts/Ability/Effects/Resolvers/EffectResolver.cs`
- `Assets/Scripts/Ability/Effects/Runtime/config/`

Runtime findings:

- `EffectResolver` dispatches all thirteen current `EffectConfig` classes.
- `EffectEntrySO.Duration` and `MaxApplyCount` are passed into `EffectEntryRuntime`.
- `TauntEffectConfig` has no own fields; Taunt duration comes from `EffectEntrySO.Duration`.
- `EffectEntrySO.ValueOverride` is not passed into the current resolver-created runtime objects.
- The `effectUpgradeModifiers` and `defaultCategoryType` parameters of `ResolveEntries` are currently not applied.
- Therefore Presentation must not show overrides or upgrade modifiers as active resolved values.

## Approved Asset Counts

Script GUIDs used to classify YAML assets:

- `EquipmentSkillSO`: `63226e07ba84a4a69967ef3b8995b8d7`
- `EffectSO`: `57a976b41e687441cad798047c5f5afc`
- `EffectEntrySO`: `ebac1672555d84d38bb2ad4ebe71d4ff`

| Path | JSON | EquipmentSkillSO | SkillHitSO | EffectSO | EffectEntrySO | Reachable EffectEntrySO |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `Assets/Resources/skill/character/generated/` | 44 | 38 | 31 | 2 | 2 | 0 |
| `Assets/Resources/skill/json/` | 20 | 20 | 20 | 18 | 18 | 18 |
| Total | 64 | 58 | 51 | 20 | 20 | 18 |

Additional linkage results:

- Character Skills: 31 have a `SkillHitSO`; 7 do not.
- Strategic Skills: all 20 have a `SkillHitSO`.
- Character hit assets have zero non-null EffectEntry references.
- Strategic hit assets: 16 have Effects and 4 have none.
- No approved cast asset has a non-null self Effect reference. Nine Character cast assets contain null `{fileID: 0}` placeholders and are not active Effects.
- No approved Skill or hit asset has a non-null nested Skill reference.

## Authoring JSON Versus Runtime SO

| Path | JSON files with Effect declarations | Effect declarations | Runtime-reachable Effect entries |
| --- | ---: | ---: | ---: |
| Character generated path | 21 | 27 | 0 |
| Strategic path | 16 | 18 | 18 |

Character JSON declares 23 `StatModifier` and 4 `Knockback` Effects, but the corresponding current hit SOs do not reference Effect entries. These values are authoring evidence only and must not be labeled as active gameplay values.

Six Character JSON files have no matching primary `EquipmentSkillSO` asset:

- `skill.character.military_officer.2.active_1.charge.json`
- `skill.character.military_officer.2.basic_attack.frontline_slash.json`
- `skill.character.military_officer.2.passive_1.unyielding_will.json`
- `skill.character.military_officer.3.active_1.charge.json`
- `skill.character.military_officer.3.basic_attack.frontline_slash.json`
- `skill.character.military_officer.3.passive_1.unyielding_will.json`

Two Character EffectEntry assets are not referenced by any approved asset:

- `Assets/Resources/skill/character/generated/effect.skill.character.door_shield_barricader.1.basic_attack.shield_bash.minor_knockback.entry.asset`
- `Assets/Resources/skill/character/generated/skill.military_officer.1.active_1.knockback.entry.asset`

Do not repair, delete, relink, or migrate these files in this task.

## Effect Config Coverage

`Approved EffectSO` counts serialized current configs. `Reachable` counts entries actually referenced by approved Skill assets.

| EffectConfig | Approved EffectSO | Reachable | JSON declarations | Planned normalized result |
| --- | ---: | ---: | ---: | --- |
| `StatModifierEffectConfig` | 13 | 13 | 36 | `StatModifier` |
| `ChanceOnHitStatModifierEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + StatModifier` |
| `OnHitTimedStatModifierEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + StatModifier(duration)` |
| `ChanceOnHealStatModifierEffectConfig` | 0 | 0 | 0 | `Activation(OnHeal + Chance + Target) + StatModifier` |
| `HealEffectConfig` | 1 | 1 | 1 | `Heal` |
| `ChanceOnHealCooldownReduceEffectConfig` | 0 | 0 | 0 | `Activation(OnHeal + Chance + Target) + CooldownChange` |
| `CooldownReduceEffectConfig` | 1 | 1 | 1 | `CooldownChange` |
| `KnockbackEffectConfig` | 4 | 2 | 6 | `Displacement` |
| `OnHitKnockbackDistanceEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + Displacement` |
| `AttackBleedEffectConfig` | 0 | 0 | 0 | `Activation(OnAttack + Chance) + PeriodicDamage` |
| `OnHitPoisonDotEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance) + PeriodicDamage` |
| `ChanceOnHitSkillEffectConfig` | 0 | 0 | 0 | `Activation(OnHit + Chance + Critical requirement) + SkillInvoke` |
| `TauntEffectConfig` | 1 | 1 | 1 | `Control` |

Five config types have approved current EffectSO coverage. Eight current source-supported config types have no approved asset coverage and require source-level tests until current assets exist.

Stage 4 validation result:

- All thirteen Config classes passed synthetic source-level mapping tests.
- All twenty `EffectEntrySO` assets under the approved roots resolved as `Supported`.
- The five asset-backed Config types are verified against current assets.
- The remaining eight Config types are implemented and synthetic-test verified, but their asset-level status remains pending.
- User test menu: `Tools > ProjectBS > Presentation > Run Effect Mapping Self Test`.

## Initial Validation Fixtures

- One reachable Effect: `Assets/Resources/skill/json/skill.strategic.blackwind_bomb.asset`
- Multiple reachable Effects: `Assets/Resources/skill/json/skill.strategic.blood_meridian_release.asset`
- Multiple reachable Effects: `Assets/Resources/skill/json/skill.strategic.wind_demon_pull.asset`
- No Effect: `Assets/Resources/skill/json/skill.strategic.heavenfall_thunder.asset`
- JSON/SO mismatch: `Assets/Resources/skill/character/generated/skill.character.abandoned_shrine_wraith.2.active_1.lost_child_cry.json` and its `.hit.asset`
- No nested Skill fixture exists in the approved paths.
- No approved legacy `SkillEffectSO` fixture exists; fallback verification must remain source-level or use a test-created in-memory object.

## Pending Content Domains

The current source definitions exist for:

- `Assets/Scripts/Stage/NodeContents/Shrine/Blessings/BlessSO.cs`
- `Assets/Scripts/Collection/Relic/Definitions/RelicSO.cs`
- `Assets/Scripts/Actor/Character/so/CharacterSO.cs`

No current Character, Bless, or Relic asset path has been approved for adapter verification. Their adapters remain Stage 7 pending. Legacy Bless and Relic asset paths remain excluded.

## Stage 1 Exit Decision

Stage 1 is complete. The implementation can use current runtime SO references as authoritative inputs and carry authoring-only provenance separately. Stage 2 is blocked until `AgentDocs/code-writing-rules.md` is restored or supplied; no code placeholder or runtime file was created in Stage 1.
