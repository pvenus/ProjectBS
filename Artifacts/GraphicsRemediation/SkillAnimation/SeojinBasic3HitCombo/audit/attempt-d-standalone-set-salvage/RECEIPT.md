# Basic combo Attempt D standalone-set salvage audit

Status: `STOP_NOT_COMPLETE54 / INSTALL_AUTHORITY0`

## Authority and scope

- Audited root: `/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/candidates/revision-04-exact-size-attempt-d`
- Read-only audit. Re-extraction, mixing, repaint, crop, repair, generation, and install: 0.
- Old bbox targets were not used as a new acceptance requirement.

## Exact physical inventory

- `manifest.json` rows: 54 (body18 + VFX36 planned records).
- Records with `pass=true`, an output `path`, and an existing PNG: 14/54.
- Materialized body frames: 8/18; missing: 10.
- Materialized VFX frames: 6/36; missing: 30.
- Total `frame-*.png` under Attempt D: 14.
- Complete standalone exact54: **false**.
- `manifest.json` SHA-256: `c8fefe62a7b8e8863d67bddd4115871eba1bcefb2420c458da985af293eb950a`.
- Existing `STOP.md` SHA-256: `a4fb5d24afff7c9f9b0c72196227169a8f2e25652ce319902be4e4025e46c978`.

The manifest's remaining 40 rows contain external generation-source references and measurements but no accepted Attempt-D output path. Those references do not constitute a complete workspace-bound D source set and cannot be mixed with the 14 materialized frames.

## Disposition

The physical complete-set gate fails before RGBA54, uniqueness54, alpha/perimeter, continuity, identity, or foreign-fragment acceptance can be issued. The 14 frames remain provenance-only and cannot be promoted or combined with another attempt.

Sole safe next remedy under the supplied instruction: generate one fresh standalone whole-set exact54 attempt, with all 54 workspace-bound outputs present before any atomic replacement. Strip/cell extraction and cross-attempt/partial installation remain forbidden.
