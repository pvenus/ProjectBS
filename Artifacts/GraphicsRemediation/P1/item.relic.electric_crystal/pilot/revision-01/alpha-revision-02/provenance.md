# electric_crystal alpha correction provenance

- Selected RGB source SHA-256: `9ac5f554d03ea0979aa12372638278595d6b158964499444891402f9e7348d35`
- Production1 output: `alpha-revision-01/candidate-alpha-r1.png` (conditional fail: visible pale fringe on dark background)
- Correction1 output: `alpha-revision-02/candidate-alpha-r1.png`
- Correction1 SHA-256: `536bab0cd09a52e18139eddfdcc04f3c8c813c96615a78263a58472a84b102a3`
- Mask SHA-256: `55ffe4bc5abd780772b597ba147d6d93319d39140010642ddf687b2ff06ca068`
- Rule: border-connected neutral checker removal, MinFilter5 edge contraction, Gaussian0.7 partial alpha, edge-only 250-gray decontamination, alpha0 RGB zero.
- RGB repaint/crop/scale/rotation/geometry change: none.
- Status: `ALPHA_VISUAL_PASS / T0_AUDIT_PENDING / NOT_PROMOTABLE`.
