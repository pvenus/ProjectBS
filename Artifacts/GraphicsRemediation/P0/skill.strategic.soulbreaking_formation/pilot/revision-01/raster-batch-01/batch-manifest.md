# Art P0 — soulbreaking_formation raster batch 01

Status: `STYLE_SELECTED_D / ALPHA_TECH_AUDIT_PENDING / NOT_PROMOTABLE`

Baseline canonical:

- `Assets/ImagesGenerated/Skill/icon/skill.strategic.soulbreaking_formation.icon.png`
- SHA-256 `20d7d5f70ca5f03fa45e0b8490550565fad5abdb5e99bff3f22492046606c1c3`
- Defect: RGBA container but no visible content in the approved audit; canonical remains untouched.

## Meaning lock

- Strategic position-targeted formation named `사혼진`.
- Applies an area debuff to enemies within radius 8m: move speed −80% for 6 seconds.
- Must show tactical circular area control, multiple caught enemies, and restricted movement—not impact damage.

## Candidate results

| Candidate | Path | SHA-256 | Decision |
|---|---|---|---|
| A | `candidates/A.png` | `0e1c606a107777dbd7581b3d7024e56a210a2f73bd9c991a45b2f450bfc9efa3` | REJECT — wooden stakes and literal rope binding overpower the strategic formation read |
| B | `candidates/B.png` | `785576de9903642c7457359bd62ca947b2f81bfa730cc87c5a0c40b293ee387e` | REJECT — six figures and dense currents are too crowded at 80px |
| C | `candidates/C.png` | `1987f8bd7eda9adf4bc491aeb0ff81e48124b532bc23bb4b1fffeaebf14313f1` | REJECT — knot anchors read as decorative talisman before battlefield formation |
| D | `candidates/D.png` | `e41e2fbd3aefafc5a5a82b5695532d247dda9ea0b6e44d61e9e72a17b97074f2` | SELECT — single circular zone, four caught enemy silhouettes, inward blue-gray currents; clearest tactical containment |

Selected style source:

- `selected/selected-D.png`
- SHA-256 `e41e2fbd3aefafc5a5a82b5695532d247dda9ea0b6e44d61e9e72a17b97074f2`
- 1254×1254, 8-bit RGBA PNG.

Selection is visual/semantic only. Actual transparency, residue, corners, bbox, halo/fringe, deterministic512 and importer remain separate gates.

## Promotion boundary

- No canonical PNG/meta/GUID/importer/Unity/staging changes in this unit.
- Exactly selected D may enter alpha technical audit and deterministic512 pilot after scope confirmation.
- Rejected candidates remain provenance and must never be promoted.

