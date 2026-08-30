# B1 popup_main T0 handoff package — Events 21, 22, 23, 25

Status: `HANDOFF_PACKAGE_READY / FOUNDATION_VALIDATION_PENDING / CANONICAL_WRITE_BLOCKED`

Authority inputs:

- Art batch receipt: `/private/tmp/projectbs-current-hwagam-art-b1-events21-26-batch.txt` (`0c92b0154ea1344f58e2d946d5004c36c2d52036c8f7a1dfcce39eee80e07bdf`)
- R1 mapping receipt: `/private/tmp/projectbs-current-hangyeol-b1-r1-art-mapping.txt` (`383c819e788fa8802407dea468f4b20c1dd40c6b52684000b5a056f459901d79`)

## Exact mapping

| Event | eventId | nodeId / sourcePopupId | Exact source | Source SHA-256 | Exact canonical target | Baseline disposition |
|---|---|---|---|---|---|---|
| 21 | `event.act1.random_event.21.breath_between_water_drops` | `node.act1.random_event.21.breath_between_water_drops.intro` | `Artifacts/GraphicsRemediation/B1/Stage/popup_main/events21-26/revision-02/node.act1.random_event.21.breath_between_water_drops.intro/candidate-A.png` | `f3dfc696f296b363170a514c821c25c7dbbec7d1f040658a12fc32ef0efc743c` | `Assets/ImagesGenerated/Stage/popup_main/node.act1.random_event.21.breath_between_water_drops.intro.main.png` | PNG/meta absent; create only after gate |
| 22 | `event.act1.random_event.22.sleeping_hawk_watch` | `node.act1.random_event.22.sleeping_hawk_watch.intro` | `Artifacts/GraphicsRemediation/B1/Stage/popup_main/events21-26/revision-02/node.act1.random_event.22.sleeping_hawk_watch.intro/candidate-A.png` | `0a326616d0bc9559ca27d50a395510c15ebf8f43029e2f83dfae79b439290b76` | `Assets/ImagesGenerated/Stage/popup_main/node.act1.random_event.22.sleeping_hawk_watch.intro.main.png` | PNG/meta absent; create only after gate |
| 23 | `event.act1.random_event.23.temple_hundred_eight_steps` | `node.act1.random_event.23.temple_hundred_eight_steps.intro` | `Artifacts/GraphicsRemediation/B1/Stage/popup_main/events21-26/revision-02/node.act1.random_event.23.temple_hundred_eight_steps.intro/candidate-A.png` | `f187d68c6e902e0941b2541284b35fcaacbbbe20831ce4b8fb5d966c4cfab81e` | `Assets/ImagesGenerated/Stage/popup_main/node.act1.random_event.23.temple_hundred_eight_steps.intro.main.png` | PNG/meta absent; create only after gate |
| 25 | `event.act1.random_event.25.hot_spring_beneath_ice` | `node.act1.random_event.25.hot_spring_beneath_ice.intro` | `Artifacts/GraphicsRemediation/B1/Stage/popup_main/events21-26/revision-02/node.act1.random_event.25.hot_spring_beneath_ice.intro/candidate-A.png` | `1a6b0b2842e643f664386a3ae57d1924efdcf33ebc0fadab975c49c8bafd9553` | `Assets/ImagesGenerated/Stage/popup_main/node.act1.random_event.25.hot_spring_beneath_ice.intro.main.png` | PNG already exists with identical SHA; do not overwrite. Existing meta GUID `5558aaa53cee46978b9218930847963e` must be preserved |

## T0 image contract

- Each source decodes as PNG, 960×1280, 8-bit RGB, non-interlaced.
- Alpha is not required; runtime image is opaque popup_main art.
- Filename is exactly `<sourcePopupId>.main.png`; aliases and inferred slugs are forbidden.
- One event owns one unique canonical path and one unique GUID.

## Importer contract

- TextureImporter, Sprite, Single, Full Rect, PPU 100.
- sRGB on, mipmap off, Bilinear, Clamp, alphaIsTransparency off.
- Standalone maxSize 2048, Automatic / Normal quality 50, crunch off.
- Inactive WebGL override may remain baseline-only and must not drive acceptance.
- New Events21/22/23: Unity may create a new `.meta` only inside the separately authorized material handoff; then exact importer values and new unique GUID are recorded.
- Event25: preserve GUID `5558aaa53cee46978b9218930847963e`. Current PNG content already matches selected SHA. Current meta is not final art canonical because it serializes Tight/alpha-on; adjust importer only when the exact meta correction scope is authorized, never by replacing the GUID.

## Atomic handoff algorithm

1. Re-hash all four sources and check dimensions/decode.
2. Assert targets 21/22/23 are absent. If any exists, stop rather than overwrite.
3. Assert target25 SHA equals selected source. If equal, PNG copy is a no-op; if unequal, stop and request disposition.
4. After foundation validation and exact material authorization only, copy 21/22/23 to their exact targets atomically.
5. Generate/correct meta within the authorized exact scope; preserve Event25 GUID and assign unique new GUIDs to 21/22/23.
6. Re-hash canonical PNGs, record GUID/importer values, and only then stage exact approved files. Staging owner: 한결.

## Rollback and no-overwrite boundary

- Before material authorization: canonical Assets, meta, GUID, Unity, index and staging are read-only.
- If any source hash/dimension changes, target unexpectedly exists, GUID collides, or importer cannot meet contract, stop without partial promotion.
- Rollback for newly created 21/22/23 removes only the exact newly created PNG/meta pair before staging; Event25 existing canonical is never deleted or overwritten.
- Candidate sources and this package remain immutable provenance; runtime must never reference `Artifacts/`.

## Next remediation queue

- Next unresolved exact Art asset: `Assets/ImagesGenerated/Skill/icon/skill.strategic.soulbreaking_formation.icon.png` (`ANTI-SKILL-002`).
- Reason: RGBA container but visible alpha bbox is empty; current registry marks it `사용 금지; 재생성 필요`.
- Proposed next unit: Art A2 exact1 meaning/style brief → small candidate batch → selected alpha-safe 512 policy.
- ETA after scope authorization: 2–4 focused hours. Next gate: 이음 scope confirmation plus 벼리 must-show brief; production remains closed.
