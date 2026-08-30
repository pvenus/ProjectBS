# P1 old_war_horn reframe handoff manifest

- Asset ID: `item.relic.old_war_horn`
- Icon ID: `item.relic.old_war_horn.icon`
- Canonical target: `Assets/ImagesGenerated/Item/icon/item.relic.old_war_horn.icon.png`
- Canonical baseline SHA-256: `56fc1d50d0d49f229563c1e83ef6a8af64032db1f25d728747decd314bd70e0c`
- Canonical baseline: 1254×1254 RGBA8
- Canonical meta SHA-256: `90a2b209fb21667a8384dee33e46044abd74fc76de733116220eb323e4c18b3b`
- Canonical GUID: `0d826bafb2ccc44bbaefc0df1be4b9bf`
- Selected source: `Artifacts/GraphicsRemediation/P1/item.relic.old_war_horn/pilot/revision-01/selected/item.relic.old_war_horn.icon.selected-reframe-B.png`
- Selected SHA-256: `88325b804c9a8253c6209277e2a366b632c831d3bc609b8d4c4099d8cf2a51f6`
- Selected status: `REFRAME_SELECTED / T0_HANDOFF_READY / NOT_PROMOTED`

## Deterministic transform

The baseline RGBA canvas was uniformly scaled to `0.83675385` using linear-light, premultiplied-alpha Lanczos resampling. No crop, rotation, repaint, color correction, or component movement was performed. The alpha≥16 bounding-box center was placed at canvas center. Pixels with alpha zero have RGB zero.

Selected alpha≥16 bbox is `[163,178,1091,1076]`, or `74.0032% × 71.6108%`. Margins are left/right `12.9984%` and top/bottom `14.1946%`. All four corner alpha values are zero.

## Visual decision

Candidate B preserves the curved C-shaped animal horn, broad bell, aged horn/brass materials, and cloth knot while reducing the baseline's excessive `88.4370% × 85.5662%` occupancy. Candidate A remains visually crowded; candidate C weakens the bell/body mass at small display sizes. No new imagery was generated.

## Promotion boundary

Canonical PNG/meta/GUID, Unity, staging, commit, and push remain untouched. Promotion must be an explicitly authorized atomic replacement of the canonical PNG with the selected file. Preserve GUID and sprite identity. Existing importer values are baseline evidence, not changed by this handoff.

Rollback is the canonical baseline SHA above. Refuse promotion if the canonical baseline SHA has changed or the selected SHA does not match.
