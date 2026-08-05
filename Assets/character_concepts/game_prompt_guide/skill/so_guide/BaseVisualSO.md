# BaseVisual


## Master Concept Reference

Before using this document, read and apply:

Assets/character_concepts/game_prompt_guide/DisignMasterConcept_rule.md

This master concept is mandatory and takes precedence over this document, task
inputs, story context, legacy assets, and external references. This document may
add domain-specific constraints, but it must not relax, override, or create an
exception to the master concept period, cultural, aesthetic, or prohibition rules.

## Structure

```json
{
  "visualId": "",
  "projectileVisualType": ""
}
```

## Purpose

Defines the static visual configuration of a skill.

BaseVisualSO owns visual-related data only.

Gameplay logic such as damage, hit detection, effects, cooldown, and casting is configured by other SOs.

---

## Fields

| Name | Type | Required | Description |
|------|------|----------|-------------|
| visualId | string | Required | Visual 고유 ID |
| projectileVisualType | ProjectileVisualType | Required | 투사체 표현 방식 |

## References

- Animation clips are resolved automatically from `visualId`.
- Supported animation names:
  - `{visualId}.idle`
  - `{visualId}.cast`
  - `{visualId}.attack`
  - `{visualId}.loop`
  - `{visualId}.hit`
- Register only the animation clips that are actually used.
- Missing animation clips are treated as not existing.
- Runtime reads AnimationClip directly from BaseVisualSO.