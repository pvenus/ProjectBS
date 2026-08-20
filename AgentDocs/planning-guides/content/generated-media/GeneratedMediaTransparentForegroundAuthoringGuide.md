# Generated Media Transparent Foreground Authoring Guide

This additive guide applies only when planning selected the closed
`generated_media_transparent_foreground_selection_v1`. It leaves the existing
visual-authoring guide blob, prompt records, indexes, handoffs, and identities
unchanged.

Authoring copies the selection byte-semantically and emits positive/negative
locks for true alpha without changing identity or style. The exact key/hash is
`generated_media_true_alpha_foreground@1.0.0` /
`2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108`.
The prompt requires alpha 0 outside intended character/equipment/pigment,
bounded artistic partial alpha only inside that silhouette, the planning safe
margin, and no matte/checkerboard/halo/vignette/floor/scene/cast shadow/fringe.

The animation branch additionally states the exact six-frame canvas, fixed
pelvis/world-root and ground baseline, rational scale, no independent recenter,
no background flicker or neighboring fragments, sword/effects inside the safe
margin, and dynamic pigment excluded from anchor movement. Main and animation
locks are mutually exclusive. These locks are prompt intent plus downstream
evidence requirements; prompt prose alone never proves alpha conformance.

## Transparent Prompt v3 Projection

For `character_single_image` only, a selected
`generated_media_transparent_foreground_selection_v1` activates the closed
transparent branch of Prompt v3. Existing records without the selection retain
the byte-identical removable-solid branch and are never required to gain a
member.

`generationBackground` is a discriminated union:

- legacy branch: exactly `{mode:"removable_solid",color:<exact planning value>}`;
- transparent branch: exactly `{mode:"transparent"}`. `color` and every unknown
  member are forbidden.

The transparent branch copies the exact selection, without `null`, aliases or
reconstruction, into all of these hash-significant locations:

1. top-level `transparentForegroundSelection` in the visual brief;
2. top-level `transparentForegroundSelection` in the prompt record and prompt
   hash payload;
3. `providerSettingsIntent.transparentForegroundSelection`, alongside
   `generationBackground={mode:"transparent"}`;
4. the exact prompt-index entry as `transparentForegroundSelection`;
5. the detached generation handoff as `transparentForegroundSelection`.

The provider-settings transparent branch contains exactly `canvas`,
`generationBackground`, `outputFormat`, and `transparentForegroundSelection`.
`outputFormat` remains `png`. The selection itself binds projection key
`generated_media_true_alpha_foreground@1.0.0`, payload hash
`2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108`,
positive safe margin, no clipping, and the exact closed main lock. Each enclosing
JCS identity therefore binds the same selection independently.

Provider prose for this branch is deterministic: LF-join, in routing source
order, every corrected `requiredElements` item, then the nine open-ink v2
negative locks, then the nine positive locks. Seven negative and eight positive
statements remain verbatim. The two background-bearing statements are replaced
only in this provider projection, by exact `constraintId`, as follows; the
published profile payload, hash, evidence and semantic ownership are unchanged:

```text
char_open_wash_v2_negative_halo_scene_shadow=No halo, vignette, radial gradient, dark backdrop, opaque or color-bearing background, matte, checkerboard, background residue, scene, environment, cast shadow, contact shadow, or shadow substitute; every pixel outside intended foreground must have alpha exactly zero.
char_open_wash_v2_positive_identity_on_ivory=Preserve approved young-adult Korean and Joseon identity and equipment with every pixel outside intended foreground alpha exactly zero, bounded artistic partial alpha only inside intended character, equipment, or pigment silhouette, and no halo, vignette, matte, checkerboard, background residue, scene, or shadow.
```

No other lock may be replaced, omitted, reordered or rewritten. This is a
versioned authoring projection selected by the separately hash-bound true-alpha
contract, not a successor profile and not a reinterpretation of legacy output.
No heading, blank line, summary, translation or authored synonym is added. The corrected required list
must already omit every statement requiring an opaque, removable-solid,
warm-ivory or color-bearing generation background and must contain the exact
true-alpha output, safe-margin and evidence requirements. Authoring never
filters or repairs an immutable planning/routing list. Thus the published v14
route that still contains `uniform removable #F2EFE6` remains blocked until one
fresh immutable planning/routing revision omits that stale item.

Closed failures are:

- `true_alpha_projection_missing` when transparent background lacks selection;
- `true_alpha_projection_mismatch` for selection drift or unknown members;
- `true_alpha_branch_conflict` for a color-bearing transparent object, a
  removable-solid object with a selection, or both branch shapes;
- `transparent_prompt_required_element_conflict` for stale opaque/removable/
  warm-ivory background requirements;
- `unsupported_record_schema` for missing/unknown enclosing branch members.

All prompt bytes and IDs derive from raw LF Git blobs. Parsing an equivalent
CRLF checkout cannot enter identity; two projections from the same raw blobs
must produce identical visual-brief, provider-settings, prompt payload/record,
Markdown, index-entry and detached-handoff bytes and hashes.
