# Strategic Skill Planning Guide

## 1. Purpose

This guide defines a planning document for strategic skills used by strategic
items. The planning document sits between product intent and standalone
EquipmentSkill JSON.

```text
existing item JSON / generated SO / explicit design request
→ strategic skill planning document
→ standalone strategic skill JSON
→ EquipmentSkillSO
→ strategic item references it through skillId
```

Planning describes intended behavior. It must not copy legacy builder fields as
if they were design decisions.

## 2. Output Location

Authoritative per-skill planning document:

```text
Assets/Doc/StrategicSkill/skill.strategic.{skill_slug}.planning.json
```

Planning index:

```text
Assets/Doc/StrategicSkill/strategic_skill.planning.index.json
```

One file contains exactly one skill. The full `skillId` is the identity and the
filename prefix. A folder name or Korean display name is not. The legacy
`strategic_skill.reverse_planning.json` file is a deprecated redirect to the
index, not an authoritative editing source.

## 3. Evidence Priority

Use evidence in this order:

1. Explicit approved design document or user instruction.
2. Current runtime behavior verified in code.
3. Generated SO values.
4. Existing item description and numeric fields.
5. Embedded legacy skill JSON.
6. Name and flavor text.

Higher-priority evidence overrides lower-priority evidence. When evidence
conflicts, do not silently choose one. Record a plain-language question and set
the review status to `needs_decision` or `blocked`.

## 4. Planning Boundary

The planning document contains only player-facing design intent:

- what the skill is for;
- how the player selects or activates it;
- who is affected and over what visible area;
- what happens, by how much, and for how long;
- the broad visual concept;
- unresolved design questions.

Do not store SO construction data in planning. The following belong to the later
standalone skill JSON conversion step:

```text
baseProfile, cast, move, hits, damage object, EffectSO config
projectile collider, lifetime, warp/homing enum, layer mask
child SO IDs, legacy field names, builder compatibility fields
```

Implementation data may be read as reverse-planning evidence, but it must be
translated into plain design language before being written.

## 5. Planning Schema

```json
{
  "schemaVersion": "1.0.0",
  "documentType": "strategic_skill_design",
  "documentId": "design.skill.strategic.example",
  "skill": {
    "skillId": "skill.strategic.example",
    "nameKo": "예시 전략술",
    "linkedItem": {
      "itemId": "item.strategic.example",
      "grade": "Basic",
      "gaugeCost": 30
    },
    "reviewStatus": "review_ready",
    "tacticalRoleKo": {
      "primary": "범위 약화",
      "secondary": null
    },
    "conceptKo": "차가운 진법이 적의 발을 얼린다.",
    "intendedUseKo": "적이 밀집한 위치의 진입을 늦춘다.",
    "targetingKo": "플레이어가 지정한 위치의 반경 6m 안에 적용한다.",
    "effectsKo": [
      "범위 안의 적 이동속도를 60% 감소시키며 6초 동안 유지한다."
    ],
    "executionKo": "사용 즉시 한 번 적용한다.",
    "presentationKo": "효과 반경과 지속 상태가 명확해야 한다.",
    "balanceContext": {
      "itemGrade": "Basic",
      "gaugeCost": 30
    },
    "openQuestionsKo": [],
    "sourceNoteKo": "기존 전략 스킬 구현을 바탕으로 역기획한 초안"
  }
}
```

## 6. Required Fields

| Field | Rule |
|---|---|
| `skillId` | `skill.strategic.{skill_slug}` |
| `nameKo` | Stable skill display name |
| `linkedItem.itemId` | Item using this skill |
| `reviewStatus` | `review_ready`, `needs_decision`, or `blocked` |
| `tacticalRoleKo` | One primary role and at most one secondary role |
| `conceptKo` | One-sentence fantasy without implementation terms |
| `intendedUseKo` | When and why the player uses it |
| `targetingKo` | Plain-language target and visible area |
| `effectsKo` | Ordered effect sentences with values and duration |
| `executionKo` | Instant, repeated, or persistent behavior in plain language |
| `presentationKo` | Player-readable visual direction |
| `balanceContext` | Linked item grade and gauge cost for review only |
| `openQuestionsKo` | Design decisions still requiring approval |

## 7. Tactical Roles

Use one Korean primary role:

```text
광역 피해
집중 피해
연속 포격
회복
아군 강화
범위 약화
행동 제어
위치 제어
전술 보조
```

Add at most one secondary role when the effect is explicit. Examples include
damage plus knockback or pull plus root.

## 8. Targeting Language

Write one complete player-facing sentence. Examples:

```text
플레이어가 지정한 위치의 반경 6m 안에 적용한다.
사용 즉시 모든 아군에게 적용한다.
사용 시 현재 체력이 가장 낮은 아군 1명을 자동 선택한다.
```

Do not write `Position`, `Party`, `targetLayerMask`, `LowestHpAlly`, or other
runtime enum names in the planning output.

## 9. Effect Writing Rules

Write effects as short Korean design sentences:

```text
모든 아군이 6초 동안 매초 최대 체력의 0.8333%를 회복한다.
범위 안의 적 이동속도를 60% 감소시키며 6초 동안 유지한다.
0.1초 간격으로 낙뢰 20개가 떨어지며, 낙뢰 하나마다 240 피해를 준다.
범위 안의 적을 지정 위치로 끌어당긴 뒤 2초 동안 속박한다.
```

- Include target, amount, unit, duration, and count only when meaningful.
- State current HP versus maximum HP explicitly.
- State per-hit versus total damage in ordinary language.
- Do not decide projectile profiles, collider radii, hit budgets, effect config,
  or SO IDs here.

## 10. Open Questions

Questions must ask for a design choice, not an implementation fix.

Good:

```text
단발 750 피해와 0.25초 간격 반복 피해 중 하나를 확정한다.
현재 체력 10% 소모와 최대 체력 10% 소모 중 하나를 확정한다.
범위 전체 기절과 단일 대상 기절 중 하나를 확정한다.
```

Do not mention builder field names, legacy enums, serialized values, or code
changes in `openQuestionsKo`.

## 11. Review Status

- `review_ready`: the reverse-planned design is complete enough for human review
  and later SO JSON conversion.
- `needs_decision`: one or more gameplay choices remain unresolved.
- `blocked`: the intended effect has no current supported runtime representation
  or core numeric inputs are absent.

High gauge cost or an existing SO does not automatically mean `ready`.

## 12. Validation

- Every source strategic skill has exactly one individual planning file.
- Each file contains exactly one `skill` object and its filename matches
  `{skillId}.planning.json`.
- The index contains every individual planning path exactly once.
- Skill and linked item IDs are unique and traceable.
- Effects are plain Korean design sentences with meaningful values and units.
- No SO profile, cast, move, hit, effect config, child ID, enum, or builder field
  appears in an individual planning file.
- Open questions describe gameplay choices only.
- Unsupported behavior is not silently replaced or marked `review_ready`.
- Item-owned cost and grade are context only and are not emitted into skill JSON.
- The planning task creates no item JSON, skill JSON, Unity asset, image,
  localization, animation, or prefab.
