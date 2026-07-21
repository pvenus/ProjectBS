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

Before authoring, maintain an internal evidence matrix with the following
columns. The matrix is reported by the task but is not copied into the
player-facing planning JSON.

| Column | Rule |
|---|---|
| `sourcePath` | Exact project-relative file path |
| `evidenceType` | `approved_design`, `runtime`, `generated_so`, `item_description`, `legacy_json`, or `flavor` |
| `priority` | Integer 1-6 matching the order above |
| `observedFactKo` | Player-facing fact without implementation field names |
| `conflictGroup` | Shared non-empty key when two facts disagree; otherwise null |

When a conflict changes target, amount, unit, count, interval, duration, effect
order, or supported behavior, `openQuestionsKo` must state both gameplay
interpretations. A generic request to review or add a description is invalid.

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

### 5.1 Field Types and Identity

| Field | Type | Constraint |
|---|---|---|
| `schemaVersion` | string | Exactly `1.0.0` |
| `documentType` | string | Exactly `strategic_skill_design` |
| `documentId` | string | Exactly `design.{skillId}` |
| `skill` | object | Exactly one object |
| `skill.skillId` | string | `^skill\.strategic\.[a-z0-9]+(?:_[a-z0-9]+)*$` |
| `skill.nameKo` | non-empty string | Stable Korean display name |
| `skill.linkedItem.itemId` | string | `^item\.strategic\.[a-z0-9]+(?:_[a-z0-9]+)*$` |
| `skill.linkedItem.grade` | string | `Basic` or `Advanced` |
| `skill.linkedItem.gaugeCost` | integer | 0-100; 0 requires approval |
| `skill.reviewStatus` | string | `review_ready`, `needs_decision`, or `blocked` |
| `skill.tacticalRoleKo` | object | One primary and zero or one secondary |
| `conceptKo` through `presentationKo` | non-empty string | Complete Korean sentence |
| `effectsKo` | string array | At least one ordered effect sentence |
| `balanceContext` | object | Must mirror linked item grade and gauge cost |
| `openQuestionsKo` | string array | Empty only when no decision remains |
| `sourceNoteKo` | non-empty string | Origin summary; must not claim approval that does not exist |

Unknown top-level or `skill` fields are not authored under schema `1.0.0`.
Additional structured fields require a schema-version change.

### 5.2 Planning Index Schema

```json
{
  "schemaVersion": "1.0.0",
  "documentType": "strategic_skill_design_index",
  "documentId": "design.strategic_skill.index",
  "planningRoot": "Assets/Doc/StrategicSkill",
  "skillCount": 1,
  "skills": [
    {
      "skillId": "skill.strategic.example",
      "nameKo": "예시 전략술",
      "reviewStatus": "review_ready",
      "linkedItemId": "item.strategic.example",
      "planningPath": "Assets/Doc/StrategicSkill/skill.strategic.example.planning.json"
    }
  ]
}
```

- `skillCount` must equal both the number of `skills` entries and the number of
  authoritative individual planning files in scope.
- `skills` is sorted by `skillId` and contains each ID and path exactly once.
- Every index value must exactly match its individual planning file.
- The deprecated redirect is not counted as an individual planning file.

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
- Use `%` for a ratio and `%p` for a percentage-point stat change.
- A repeated effect states amount per occurrence, count, interval, and total
  amount when total amount is deterministic.
- If an area is inferred by combining spawn distribution and per-hit area, do
  not publish the sum as an approved radius unless an approved design or this
  guide explicitly defines that composition. Leave a gameplay choice instead.
- Do not decide projectile profiles, collider radii, hit budgets, effect config,
  or SO IDs here.

## 9.1 Mechanics Completeness

Review every applicable row before setting `review_ready`.

| Topic | Required planning fact |
|---|---|
| Activation | How the player initiates the strategic skill |
| Target selection | Selected point, automatic target, all allies, or other approved rule |
| Application area | Visible area or explicit global/all-party scope |
| Effect order | Ordered effects when order changes gameplay |
| Numeric basis | Per-hit, per-second, current-HP, maximum-HP, flat, ratio, or percentage-point basis |
| Repetition | Count and interval, or explicit instant single application |
| Duration | Duration for every non-instant effect |
| Stacking/reapplication | No stacking, refresh, replace, independent stacking, or unresolved choice |
| Use restriction | Cooldown when one exists; otherwise state that gauge is the only authored restriction |
| Termination | Instant completion, duration expiry, area expiry, combat end, or another approved condition |

Facts that do not apply are internally marked `not_applicable`. Missing facts
that affect behavior must become a specific open question. They must not be
silently filled with common defaults.

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

Each question must do at least one of the following:

- present two or more mutually exclusive gameplay choices;
- request approval of an exact target, amount, unit, count, interval, duration,
  stacking rule, termination condition, or display name.

Invalid:

```text
게임플레이 설명 추가
수치형 설명 승인
최종 검토 필요
```

## 11. Review Status

- `review_ready`: the reverse-planned design is complete enough for human review
  and later SO JSON conversion.
- `needs_decision`: one or more gameplay choices remain unresolved.
- `blocked`: the intended effect has no current supported runtime representation,
  its required JSON-to-SO builder path is absent, or core numeric inputs are
  absent. Distinguish these causes in the report; a builder gap must not be
  described as missing runtime behavior.

Additional gates:

- `review_ready` requires an empty `openQuestionsKo` and every applicable
  mechanics-completeness topic to be resolved.
- `needs_decision` requires at least one valid gameplay choice or exact approval
  request in `openQuestionsKo`.
- `blocked` requires an open question that identifies the unsupported behavior
  or missing core input in player-facing terms.

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

## 13. Naming, Localization, Tags, and Paths

- `nameKo` is the approved Korean display name. If item and embedded skill names
  differ, keep the selected name only after `openQuestionsKo` resolves the two
  explicit candidates.
- Schema `1.0.0` does not author localization keys. The later skill workflow uses
  `{skillId}.name` and `{skillId}.desc`; it must not assume that `conceptKo` is
  automatically the final description.
- Schema `1.0.0` does not contain skill tags. Item tags remain item-owned. A
  future planning tag taxonomy requires a schema-version change and a controlled
  lowercase vocabulary.
- Every path written in the index or task report is project-relative, uses `/`,
  and starts with `Assets/`.
- The planning task may create the Unity `.meta` paired with a new planning JSON
  when required by the project. It creates no `.meta` for files it did not
  create and never reuses an existing GUID.
