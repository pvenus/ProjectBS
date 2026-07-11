


# EquipmentSkillSO

## Structure

```json
{
  "equipmentId": "",
  "icon": "",
  "baseProfile": {
  },
  "cast": {
  },
  "hits": [
  ],
  "move": {
  },
  "spawnSkill": {
  },
  "upgradeTable": {
  },
  "baseVisual": {
  }
}
```

## Purpose

Defines one complete skill by composing multiple smaller SO definitions.

EquipmentSkillSO is the top-level skill authoring object. It does not duplicate the schemas of its child SOs.

## Fields

| Name | Type | Required | Description |
|------|------|----------|-------------|
| equipmentId | string | Required | Unique skill/equipment identifier |
| icon | string | Optional | Icon resource key or icon asset name |
| baseProfile | EquipmentBaseProfileSO | Required | Base skill configuration |
| cast | SkillCastSO | Required | Cast configuration |
| hits | SkillHitSO[] | Optional | Hit definitions used by this skill |
| move | SkillMoveSO | Optional | Movement configuration |
| spawnSkill | SpawnSkillSO | Optional | Spawn skill configuration |
| upgradeTable | EquipmentUpgradeTableSO | Optional | Upgrade configuration |
| baseVisual | BaseVisualSO | Required | Visual configuration |

## Optional References

The following references are optional.

If omitted, the corresponding feature is disabled. `baseVisual` is the exception: write it for every skill, even when no animation clips are registered yet.

- icon
- hits
- move
- spawnSkill
- upgradeTable

## Base Profile

`baseProfile` defines shared static skill values such as skill type, component type, projectile count, projectile scale, projectile collider radius, and projectile lifetime.

See `EquipmentBaseProfileSO.md`.

## Cast

`cast` defines cast timing, cooldown, range, cast movement, and self-applied effects.

See `SkillCastSO.md`.

## Hits

`hits` defines hit behavior.

Use this when the skill requires hit detection, damage, or effects applied on hit.

See `SkillHitSO.md`.

## Move

`move` defines projectile or skill movement.

Use this when the skill requires movement behavior.

See `SkillMoveSO.md`.

## Spawn Skill

`spawnSkill` defines additional spawn behavior.

Use this only when the skill creates another skill or character.

See `SpawnSkillSO.md`.

## Upgrade Table

`upgradeTable` defines level-based upgrade modifiers.

Use this only when the skill can be upgraded.

See `EquipmentUpgradeTableSO.md`.

## Visual

`baseVisual` defines the static visual configuration and must be written for every EquipmentSkillSO JSON input.

Animation clips are resolved automatically from `visualId` inside BaseVisualSO.

See `BaseVisualSO.md`.

## Localization

`name` and `description` are not written directly in EquipmentSkillSO JSON.

They are resolved through string data using `equipmentId`.

Expected string keys:

```text
{equipmentId}.name
{equipmentId}.desc
```

## ID Derivation

`equipmentId` must follow the Skill ID format defined in `SkillJsonGuide.md`.

For character skills, use:

```text
skill.character.{character_name}.{grade}.{slot}.{skill_name}
```

Use `skill.character` for Player, Npc, and Boss character skills.

`Npc` is a `characterType`, not a skill ID domain. Do not use `skill.npc` for CharacterSO-linked skills.

Example:

```text
skill.character.military_officer.1.active_1.charge
```

Use the slot value from the source design JSON. For example, `BasicAttack` maps to `basic_attack`, `Active1` maps to `active_1`, and `Passive` maps to `passive_1` unless the design explicitly defines a more specific passive slot.

Child SO IDs should be derived from `equipmentId` to keep generated JSON consistent.

```text
baseProfileId  = {equipmentId}.profile
castId         = {equipmentId}.cast
hitId          = {equipmentId}.hit
moveId         = {equipmentId}.move
visualId       = {equipmentId}.visual
upgradeTableId = {equipmentId}.upgrade
```

Do not include SO names, folder names, generated asset type names, or builder implementation names in `equipmentId`.

## Validation

```text
equipmentId   : Required
baseProfile   : Required
cast          : Required
hits          : Optional, required when hit detection is needed
move          : Optional, required when movement behavior is needed
spawnSkill    : Optional
upgradeTable  : Optional
baseVisual    : Required
```

## References

- Base Profile : `EquipmentBaseProfileSO.md`
- Cast : `SkillCastSO.md`
- Hit : `SkillHitSO.md`
- Move : `SkillMoveSO.md`
- Spawn Skill : `SpawnSkillSO.md`
- Upgrade Table : `EquipmentUpgradeTableSO.md`
- Visual : `BaseVisualSO.md`

