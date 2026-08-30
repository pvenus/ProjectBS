# Black incense candle reframe selection and T0 handoff

- Asset ID: `item.relic.black_incense_candle`
- Canonical target: `Assets/ImagesGenerated/Item/icon/item.relic.black_incense_candle.icon.png`
- Baseline SHA-256: `f10bf7afa0c226f528f10510aaab6caef64dab8403363c67872661cf72dd6237`
- Candidate batch: deterministic reframe A/B/C; no ImageGen and no RGB repaint.
- Selected: B, target maximum alpha occupancy 76%.
- Selected SHA-256: `c2920c5fe8df7ba4a0582016f0e7cb34554c9e36f56e820d7b284a19a306975b`
- Selected alpha bbox: 34.290% × 75.997%; corners alpha0; alpha0 RGB residue0.
- Operation: crop to the source alpha>=16 bounds, uniform premultiplied-alpha Lanczos scale, centered on the original 1254×1254 transparent canvas.
- Visual decision: B preserves the candle, brazier, smoke, aged-metal identity while fixing the 96.09% vertical crowding. A is too small at 32/80; C restores avoidable vertical density.
- Next technical operation: identity-preserving deterministic 512 RGBA, FullRect/Bilinear/max512/lossless, then atomic canonical PNG/meta promotion if T0 passes.
- Canonical/meta/GUID/Unity/staging changes in this Art unit: none.
