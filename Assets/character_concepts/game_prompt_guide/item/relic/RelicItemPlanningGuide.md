# Relic Item Planning Guide

## 1. Purpose

This guide defines the independent game-design document for one relic item.
Planning describes the intended player experience and balance behavior before any
RelicSO or Effect JSON is authored.

```text
approved concept or legacy relic evidence
→ relic item planning document
→ RelicSO input JSON
→ RelicSO / EffectEntrySO / EffectSO assets
```

The planning document is authoritative for design intent. It is not a dump of
Unity serialization or Effect builder fields.

## 2. Output Location

```text
Assets/Doc/Relic/item.relic.{relic_slug}.planning.json
Assets/Doc/Relic/relic_item.planning.index.json
```

One planning file contains exactly one relic. The complete `relicId` is used as
the filename prefix. Each relic remains independently editable as content grows.

## 3. Evidence Priority for Reverse Planning

1. Explicit approved design instruction.
2. Player-facing Korean name and description.
3. Verified runtime behavior.
4. RelicSO and linked Effect assets.
5. Icon and visual naming.

When sources disagree, do not silently choose one. Preserve the coherent
player-facing design, add a gameplay decision to `openQuestionsKo`, and use
`needs_decision`. Use `blocked` when the core behavior or number cannot be
recovered at all.

## 4. Planning Boundary

Planning may contain:

- the relic fantasy and tactical role;
- why a player equips it and which build it supports;
- the player-readable trigger, affected target, values, units, chance, and
  duration;
- intended synergies, tradeoffs, rarity context, and visual direction;
- unresolved gameplay decisions.

Planning must not contain implementation construction fields:

```text
effectEntries, EffectSO, EffectEntrySO, effectType, config
statType, modifierType, valueType, lifetimeType, categoryType
duration field, maxApplyCount, hasValueOverride, valueOverride
asset GUID, fileID, serialized property, builder class, enum number
```

Numbers such as “공격속도 8% 증가” or “3초 동안 유지” are design facts and are
allowed. A JSON field such as `statType: AttackSpeedPercent` is not.

## 5. Planning Schema

```json
{
  "schemaVersion": "1.0.0",
  "documentType": "relic_item_design",
  "documentId": "design.item.relic.example",
  "relic": {
    "relicId": "item.relic.example",
    "nameKo": "예시 유물",
    "reviewStatus": "review_ready",
    "rarityKo": "일반",
    "designRoleKo": {
      "primary": "공격 강화",
      "secondary": null
    },
    "conceptKo": "상처가 깊어질수록 전투 의지가 강해지는 부적이다.",
    "intendedUseKo": "체력을 낮게 유지하는 위험 보상형 공격 빌드에서 사용한다.",
    "equipBehaviorKo": "장착되어 있는 동안 효과가 유지된다.",
    "triggerKo": "장착한 캐릭터의 잃은 체력 비율이 변할 때 효과량도 함께 변한다.",
    "targetKo": "이 유물을 장착한 캐릭터에게 적용한다.",
    "effectsKo": [
      "잃은 체력 1%마다 공격력이 0.2% 증가한다."
    ],
    "synergyKo": [
      "낮은 체력에서 이득을 얻는 공격형 효과와 조합한다."
    ],
    "tradeoffKo": "효과를 크게 얻으려면 낮은 체력으로 전투하는 위험을 감수해야 한다.",
    "presentationKo": "체력이 낮아질수록 유물과 캐릭터 주변의 기운이 강해져야 한다.",
    "balanceContext": {
      "rarityTierKo": "일반",
      "equipSlotContextKo": "동시에 최대 3개의 유물을 장착하는 환경에서 비교한다."
    },
    "openQuestionsKo": [],
    "sourceNoteKo": "승인된 신규 기획"
  }
}
```

## 6. Required Fields

| Field | Rule |
|---|---|
| `relicId` | `item.relic.{lowercase_snake_case_slug}` |
| `nameKo` | Stable Korean display name |
| `reviewStatus` | `review_ready`, `needs_decision`, or `blocked` |
| `rarityKo` | Player-facing rarity tier in Korean |
| `designRoleKo` | One primary and at most one secondary role |
| `conceptKo` | One-sentence relic fantasy |
| `intendedUseKo` | Build and tactical reason to equip it |
| `equipBehaviorKo` | Persistent or conditional equip behavior in plain language |
| `triggerKo` | Player-readable activation condition |
| `targetKo` | Who receives or suffers the effect |
| `effectsKo` | Ordered, exact gameplay-effect sentences |
| `synergyKo` | Intended build interactions; may be empty |
| `tradeoffKo` | Cost, risk, limitation, or “별도 페널티 없음” |
| `presentationKo` | Recognizable visual direction |
| `balanceContext` | Rarity and three-slot comparison context only |
| `openQuestionsKo` | Unresolved gameplay choices |

## 7. Design Roles

Choose one primary role and at most one secondary role:

```text
공격 강화
방어 강화
기동 강화
상태 이상
위치 제어
자원 획득
위험 보상
반격
회복 지원
재사용 대기시간 지원
```

## 8. Effect Writing Rules

Write complete Korean sentences from the player's perspective:

```text
공격이 적중할 때 12% 확률로 대상에게 냉기 상태를 부여한다.
냉기 상태인 대상은 이동속도가 15% 감소하며 2초 동안 유지된다.
적을 처치할 때 6% 확률로 기본 획득량의 100%에 해당하는 추가 골드를 얻는다.
공격을 받으면 받은 피해의 15%를 공격자에게 반사한다.
```

- Distinguish percentage points, percent scaling, seconds, meters, and damage per
  second.
- Separate trigger, target, and resulting status when one sentence would be
  ambiguous.
- Do not invent hidden effects from an icon or rarity.
- Do not translate an implementation mismatch into an approved design silently.

## 9. Review Status

- `review_ready`: design facts are internally consistent and complete enough for
  review and later SO JSON conversion.
- `needs_decision`: sources disagree or a gameplay choice must be approved.
- `blocked`: the core effect or essential numeric value cannot be established.

Questions must be about gameplay choices, not code or serialized fields.

Good:

```text
붉은 깃털의 최종 효과를 공격속도 8% 증가와 이동속도 8% 증가 중 하나로 확정한다.
검은 향초에 최대 체력 10% 증가 효과를 포함할지 확정한다.
```

## 10. Validation

- Every relic has exactly one individual planning file and one index entry.
- Schema version is `1.0.0` and document type is `relic_item_design`.
- All effect statements use ordinary design language with exact meaningful
  values and units.
- No Effect/SO construction field, runtime enum, asset reference, or builder name
  appears in an individual planning file.
- Conflicting evidence produces an open gameplay question and does not remain
  `review_ready`.
- Planning generation creates no RelicSO JSON, Effect JSON, Unity asset,
  localization, icon, pool, shop product, or reward data.

## 11. Implementation Mapping Boundary

Planning remains pure design input. It must not require or contain the EffectSO
construction fields used by the normalized RelicSO JSON builder.

The conversion step requires a separate approved implementation mapping/spec
that references the planning file without copying builder fields back into it.
That spec must provide:

- `iconSpriteName`, `themeColor`, `category`, `subCategory`, `hidden`, and
  `developerOnly`;
- one semantic effect slug per gameplay behavior;
- the supported current `EffectType` and exact config fields;
- `lifetimeType`, `categoryType`, `duration`, and `maxApplyCount`;
- traceability back to the exact `effectsKo` sentence or open gameplay decision.

If no approved mapping/spec exists, or if the mapping asks for runtime behavior
that current Effect types cannot reproduce, SO JSON conversion must stop with a
clear failure instead of approximating the behavior.
