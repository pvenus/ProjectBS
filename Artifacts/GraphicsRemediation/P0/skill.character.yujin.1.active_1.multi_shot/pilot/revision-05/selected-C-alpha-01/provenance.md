# Candidate C Alpha Production R1 / Mask Correction1

- Producer/visual approver: 화감
- Input: `Artifacts/GraphicsRemediation/P0/skill.character.yujin.1.active_1.multi_shot/pilot/revision-04/raster-batch-01/candidate-C/raw-candidate.png`
- Input SHA-256: `9ba740f2ca5d8b99e865848d02d25c4be3ea3ace4966f1cb1f93864fc8ea0a76`
- Nominal matte: `#D8D5CC` (not assumed pixel-uniform)
- Method: deterministic border-fitted matte model, grayscale alpha extraction, alpha-transition-only decontamination, alpha0 RGB zeroing.
- Production attempt: 1. Automatic correction: none.
- Authorized correction1: only weak left-boundary mask samples where x<88 and alpha<128 were changed to alpha0. RGB and right/top/bottom mask were unchanged.
- RGB repaint, geometry change, crop, scale, rotate, ImageGen: none.
- Synthetic stress size: 32px; actual consumers: 200px and 80px.
