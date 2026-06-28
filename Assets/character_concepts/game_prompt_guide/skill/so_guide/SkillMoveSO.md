# SkillMoveSO

## Structure

```json
{
  "moveId": "projectile.linear",
  "moveType": "Linear",
  "config": {
    "speed": 8
  },
  "applyDirectionRotation": true,
  "rotationOffset": 0
}
```

`config` uses a different JSON format for each `moveType`.

For example, `Linear` uses `speed`, `Hover` uses `followOffset`, and `Orbit` uses orbit-specific fields.

## Purpose

Defines how a projectile moves after it is spawned.

SkillMoveSO stores movement type, movement config, and rotation behavior. Runtime movement data is created by `EquipmentSkillResolver`.

## Base Fields

| Name | Type | Description | Range |
|------|------|-------------|-------|
| moveId | string | Unique movement definition id | Required |
| moveType | ProjectileMoveType | Movement type | Required |
| config | object | Type-specific movement config | Depends on moveType |
## Optional Profiles

The following profile is optional.

- Config

If the selected `moveType` does not require additional settings, `config` may be omitted.
| applyDirectionRotation | bool | Rotate projectile by movement direction | true / false |
| rotationOffset | float | Rotation offset in degrees | Any |

## Type Categories

### Linear

Moves from spawn position toward the target position in a straight line.

```json
{
  "moveId": "projectile.linear",
  "moveType": "Linear",
  "config": {
    "speed": 8
  },
  "applyDirectionRotation": true,
  "rotationOffset": 0
}
```

| Name | Type | Description | Range |
|------|------|-------------|-------|
| speed | float | Movement speed | >= 0 |

### Hover

Stays attached to the target with an offset.

```json
{
  "moveId": "projectile.hover",
  "moveType": "Hover",
  "config": {
    "followOffset": {
      "x": 0,
      "y": 1
    }
  },
  "applyDirectionRotation": false,
  "rotationOffset": 0
}
```

| Name | Type | Description | Range |
|------|------|-------------|-------|
| followOffset.x | float | Horizontal follow offset | Any |
| followOffset.y | float | Vertical follow offset | Any |

### Warp

Instantly moves to the resolved target position.

```json
{
  "moveId": "projectile.warp",
  "moveType": "Warp",
  "applyDirectionRotation": false,
  "rotationOffset": 0
}
```

No config fields.

### Homing

Moves while continuously turning toward the target.

```json
{
  "moveId": "projectile.homing",
  "moveType": "Homing",
  "config": {
    "speed": 8,
    "turnSpeed": 180
  },
  "applyDirectionRotation": true,
  "rotationOffset": 0
}
```

| Name | Type | Description | Range |
|------|------|-------------|-------|
| speed | float | Movement speed | >= 0 |
| turnSpeed | float | Turning speed in degrees per second | >= 0 |

### Orbit

Moves around the target in an orbit pattern.

```json
{
  "moveId": "projectile.orbit",
  "moveType": "Orbit",
  "config": {
    "orbitRadius": 1.5,
    "orbitAngularSpeed": 180,
    "clockwise": false,
    "spawnOrder": 0,
    "maxProjectileCount": 1,
    "resetPhaseWhenLayoutChanges": true,
    "radialPulseAmplitude": 0,
    "radialPulseFrequency": 0
  },
  "applyDirectionRotation": true,
  "rotationOffset": 0
}
```

| Name | Type | Description | Range |
|------|------|-------------|-------|
| orbitRadius | float | Orbit radius | >= 0 |
| orbitAngularSpeed | float | Orbit angular speed in degrees per second | >= 0 |
| clockwise | bool | Orbit direction | true / false |
| spawnOrder | int | Projectile order in multi-projectile layout | >= 0 |
| maxProjectileCount | int | Total projectile count used for orbit spacing | >= 1 |
| resetPhaseWhenLayoutChanges | bool | Reset orbit phase when layout changes | true / false |
| radialPulseAmplitude | float | Radius pulse amplitude | >= 0 |
| radialPulseFrequency | float | Radius pulse frequency | >= 0 |

## Validation Rules

```text
moveId                              : Required
moveType                            : Required enum
config                              : Optional depending on moveType
applyDirectionRotation              : true / false
rotationOffset                      : Any
Linear.config.speed                 : >= 0
Hover.config.followOffset           : Optional vector2
Warp.config                         : Not required
Homing.config.speed                 : >= 0
Homing.config.turnSpeed             : >= 0
Orbit.config.orbitRadius            : >= 0
Orbit.config.orbitAngularSpeed      : >= 0
Orbit.config.maxProjectileCount     : >= 1
Orbit.config.radialPulseAmplitude   : >= 0
Orbit.config.radialPulseFrequency   : >= 0
```

## Notes

> `config` is optional for movement types that do not require additional settings.
>
> If omitted, `EquipmentSkillResolver` creates a default runtime config based on `moveType`.
>
> `moveId` is an identifier for authoring and lookup.
>
> `moveType` decides which config format is used.
>
> `config` must match the selected `moveType`.
>
> `applyDirectionRotation` and `rotationOffset` are applied after the runtime move DTO is created.