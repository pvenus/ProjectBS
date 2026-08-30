# Art A0 `multi_shot` pilot provenance

- Unit: `art-A0-multishot-normalization`
- Producer: Codex built-in ImageGen for the source; deterministic Pillow/NumPy premultiplied-alpha normalization for the candidate
- Approver: 화감 — 프로젝트 아트 디렉터
- Status: `candidate_original_approved / canonical_promotion_blocked`
- Design authority receipt: `/private/tmp/projectbs-current-byeori-art-a0-multishot-brief.txt`
- Design authority SHA-256: `a174221d6fb49ee42e9553543e8efd5ccb0855a5d9a4841f47e16a4e253cf1bf`
- Production brief: `/private/tmp/projectbs-current-hwagam-art-a0-production-brief.txt`
- Production brief SHA-256: `759a858f6a00f5410eb6d82acb033862887fa2fc308b778928900180cfd285ae`

## Source

- Path: `/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-1d76db10-2d7c-45b6-b1ca-ec9c110cb958.png`
- SHA-256: `b49daa36d60cbb4a1a06b26d84ca6fbc80b312b7d45eb6b54c5bdddf02b2e09b`
- Format: 1254×1254 RGBA PNG
- Generation count: 1 initial ImageGen generation
- ImageGen correction count: 1, exhausted; its RGB checkerboard result was rejected and is not an input to this candidate

Initial source prompt required exactly three independent physical arrows sharing one launch origin and separating in a shallow simultaneous fan, with traditional feather fletching, forged iron heads, restrained ink/wood/blue-gray color, and a genuinely transparent background. It prohibited a single arrow with afterimages, sequential volley, guided or magical projectiles, neon, crosshair, card frame, text, person, bow, target, scenery, and any full-frame background.

## Authorized deterministic normalization

1. Verify the source SHA-256 exactly.
2. Premultiply RGB by alpha.
3. Resize the complete 1254×1254 RGBA canvas to 991×991 using Lanczos.
4. Unpremultiply RGB without alpha thresholding.
5. Place the full 991×991 result at `(131,131)` on a 1254×1254 `(0,0,0,0)` canvas.
6. Do not crop, repaint, reconstruct, remove stray pixels, change hue, or edit arrow pixels internally.
7. Generate 200×200, 80×80, and 32×32 contacts from the candidate with the same premultiplied-alpha Lanczos method. The 32px image is synthetic stress evidence, not an authoritative runtime consumer.

## Candidate metrics

- Candidate SHA-256: `dad98d37317e90d0b121e64cfa1e5eddc597ac0f020dea989feb4b28ee8de4f3`
- Format: 1254×1254 RGBA
- Alpha extrema: 0..255
- Alpha bbox: `(131,146)–(1111,1122)`
- Bbox occupancy: width 78.15%, height 77.83%
- Margins: left 131px, top 146px, right 143px, bottom 132px
- Corner alpha: 0 / 0 / 0 / 0
- Fully transparent pixel RGB residue: 0

## Contact evidence

- `contact-200.png` — actual consumer proxy; SHA-256 `54e9c353bdcac2a5852f8c2648c8e6679789fe3992b286285752372b9f8e5a07`
- `contact-80.png` — actual consumer proxy; SHA-256 `c1c7f5858a10f51f65eae8cb96da91b1379f8ad2749473c492b1dfd56a05a1fe`
- `contact-32.png` — synthetic stress only; SHA-256 `fb8c4553314f1e9053a0316344da559fb7f4ca0fa0b40e0576ab5e9c3ac75b27`

## Art-direction decision

- PASS: exactly three independently readable physical arrowheads and shafts.
- PASS: all arrows share one origin and separate as one simultaneous fan.
- PASS: 200px preserves origin, wood, iron, fletching, and direction.
- PASS: 80px preserves three axes and three arrowheads.
- PASS: 32px synthetic stress preserves a three-arrowhead fan silhouette.
- PASS: actual transparent cutout, no full-frame black/translucent background, no baked checkerboard, no text/card/crosshair/person.
- PASS: original candidate bbox and margins meet 65–86% and at least 7% requirements.
- Remaining risk: importer filtering and mesh may create edge darkening or merge the narrow shafts against live UI backgrounds. FullRect/Tight and Bilinear/Point comparison belongs to the next technical gate.

## Promotion boundary

This approval covers the candidate original and synthetic contacts only. Canonical PNG/meta/GUID, Registry, Unity, Prefab, code, JSON, SO, catalog, staging, commit, and push remain blocked pending separate importer, G2, G3, and promotion approvals.
