# Basic combo54 frame contamination audit

- Exact54 inspected: 54
- Installed byte match to r1: 54/54
- Provenance-risk frames: 54/54 (99 source components cross proportional strip-cell boundaries).
- Installed PNG border-alpha touch: 0/54; this does not repair a silhouette already cut during strip extraction.
- Visually unambiguous high-risk examples: body hit0 F4/F5 (blade/foreign left fragment), VFX G2 hit2 F1/F2 (right/bottom neighboring marks), VFX G3 hit1 F2 (right/bottom neighboring mark). The TSV remains the exhaustive conservative list rather than silently clearing less obvious cases.
- GIF safe-canvas issue is separate from source cell contamination: the former clipped the final composite against 960x540, while this audit finds components crossing the original generated strip cell cuts before GIF composition.
- Hangyeol binding/importer audit SHA `419d55ff9e6181b45e7a50de0650a90d533218685dc09ec0464040557f9e8178`: clip/GUID/order/importer/proxy contamination0; source PNG/composition remains leading root.
- See `failures.tsv` for exact sides, component areas, and probable neighbor donors.
- Safe complete-set remedy: prefer fresh standalone full-canvas frames. Overlap-aware re-extraction is safe only if every complete connected subject exists inside the original strip with recoverable padding; the current boundary-crossing components make that condition unproven. No partial replacement.
- No repaint/crop/repair/install performed.
