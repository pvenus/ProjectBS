# blunt_gear alpha correction provenance

- Selected RGB source SHA-256: `9abfae831d47b2df80a9cefa4759cce5952981d63ae79f835fe996ca0de40343`
- Production1 output: `alpha-revision-01/candidate-alpha-r1.png` (conditional fail: visible pale fringe on dark background)
- Correction1 output: `alpha-revision-02/candidate-alpha-r1.png`
- Correction1 SHA-256: `d12f072cc4845cdd4269e3e43a6c601a1bbe0fe95e4ab06ef399e58c86c388da`
- Mask SHA-256: `4254889c5ce9ec85d3aee0e47db50110283dc4f4d140a821503abe77952ab512`
- Rule: border-connected neutral checker removal, MinFilter5 edge contraction, Gaussian0.7 partial alpha, edge-only 250-gray decontamination, alpha0 RGB zero.
- RGB repaint/crop/scale/rotation/geometry change: none.
- Status: `ALPHA_VISUAL_PASS / T0_AUDIT_PENDING / NOT_PROMOTABLE`.
