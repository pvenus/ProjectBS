# Art A1 selected2 alpha family provenance

- Inputs are immutable Basic B2 SHA `a5e67388002306056148aaf85f97d11a193001f25dab1236aaaff16494a897f5` and Passive A2 SHA `019131d949ec359d111f9df6db4b1123baed10a0bc32f04a99f39f077bf2044c`.
- Background was modeled as a robust quadratic RGB surface from outer 48px samples. The model only assisted trimap and edge inverse-composite decontamination.
- Alpha used smoothstep of `max(RGB distance, 1.35*luminance distance)` from 7.5 to 30. Alpha-zero RGB was forced to zero; alpha255 internal RGB was unchanged before normalization.
- Basic used scale1.0. Passive used the authorized combined-foreground uniform scale0.96 and centered translation0; crop, rotation, nonuniform scale and repaint were zero.
- Basic production r0 had one lower-left corner sample alpha4. Evidence showed the subject bbox began at x165/y190, so correction1 reclassified only `outer4px AND 0<alpha<16` to RGBA0. Exactly one pixel was removed; must-preserve overlap was zero. Passive used no correction.
- Production and correction were each reproduced to temporary roots. Selected masters and runtime512 outputs were byte-identical between render1/render2.
- Temporary light/dark/family contacts were visual-review evidence only and are not part of this exact24 root. They showed no matte veil, bright halo or dark fringe; Basic remains one diagonal arrow and Passive remains one arrow plus two open waves.
- Initial renderer invocation failed before image processing because ESM could not resolve `sharp`; only the module-loading line was corrected. No image material existed from that failed invocation.
- Canonical PNG/meta/GUID/importer, Unity, staging, commit and push were untouched. Current family status is alpha visual pass, technical audit pending, not promotable.
