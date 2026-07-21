# Item Icon Generation Guide

## 1. Purpose

This guide defines how to generate one static item icon with PixelLab and turn
the selected result into a stable Unity Sprite.

The first supported visual profile is `relic`. Relics are items that own gameplay
effects; the icon represents the physical item and its fantasy, not an EffectSO,
skill VFX, character portrait, inventory slot, or UI card.

```text
approved item planning
-> concise item-icon brief
-> PixelLab Create UI elements (Pro)
-> split the 2x2 result into four candidates
-> evaluate and preserve one candidate
-> copy the passing 128x128 Sprite to Unity
```

Use the skill icon workflow as an operational reference, but do not copy its
80x80 canvas, opaque background, square frame, slot rules, or grade rules.

## 2. Required References

Project references:

```text
Assets/character_concepts/game_prompt_guide/skill/SkillIconGenerationGuide.md
Assets/character_concepts/game_prompt_guide/item/relic/RelicItemPlanningGuide.md
Assets/character_concepts/game_prompt_guide/item/relic/RelicItemJsonGuide.md
Assets/Resources/shop/relic
```

Official PixelLab references:

```text
https://www.pixellab.ai/docs/tools/create-ui-elements-pro
https://www.pixellab.ai/docs/options/general
https://www.pixellab.ai/create?tool=create_ui_pro
```

The PixelLab documentation states that `Create UI elements (Pro)` accepts a text
Description, optional Color Palette and Concept Image, and a No Background
option. At 86-128 pixels it returns a 2x2 grid and costs 20 generations. This
workflow uses Custom size 128x128, so one run produces four candidates in one
256x256 result sheet.

## 3. Existing Relic Icon Analysis

The legacy reference set contains nine 256x256 PNG files:

```text
Assets/Resources/shop/relic/relic-icon-01.png
...
Assets/Resources/shop/relic/relic-icon-09.png
```

Each file is a transparent 2x2 visual sheet with four item cells. Eight sheets
are imported as four fixed 128x128 Sprites. `relic-icon-08.png` is an important
legacy exception: its four Sprites were auto-sliced to different opaque bounds
instead of the cell grid. That produces inconsistent Sprite dimensions and
pivots even though the PNG has the same 256x256 layout. Existing relic assets
select individual cells through Unity `fileID` references. The sheets are useful
as visual evidence, but their generic file names, Sprite names, and slicing
exceptions are not rules for new icons.

### 3.1 Shared visual language

- One isolated physical relic per cell on a transparent background.
- A centered, immediately readable silhouette occupying roughly 70-84% of the
  cell, with breathing room on every side.
- Front or restrained three-quarter presentation; necklaces and circular relics
  are nearly symmetrical, while blades, feathers, horns, and nails use a natural
  object axis.
- Dark-fantasy antique materials: aged gold, bronze, dark iron, carved wood,
  leather, bone, crystal, wax, cloth, and enamel.
- A dark near-black colored outline separates the item from any inventory UI.
- Hand-placed pixel clusters, stepped curves, hard material highlights, and
  controlled selective detail rather than smooth vector edges.
- A dark or muted material base with one dominant saturated accent family.
  Gold/bronze trim and small cyan, blue, red, violet, or toxic-green highlights
  are common.
- Ornament follows the object's construction: engraved bands, knots, runes,
  facets, seams, tassels, studs, or carved motifs.
- A small semantic cue may surround the object, such as smoke, frost, sparks,
  electricity, or a glow. The cue remains subordinate to the physical relic.
- No text, rarity badge, border, inventory slot, card, scene, character, hand, or
  full-screen spell effect.

### 3.2 Inconsistencies to normalize

The legacy set varies in subject scale, outline thickness, detail density, accent
saturation, the amount of surrounding magic, and one sheet's auto-sliced Sprite
bounds. New icons must not reproduce those variations blindly.

Use this normalized contract:

| Property | New item icon rule |
|---|---|
| Primary subject | Exactly one physical item |
| Canvas per candidate | 128x128 RGBA |
| Background | Transparent by default |
| Visible bounds | Target 90-108 pixels on the longest axis |
| Safe margin | At least 10 pixels on every edge |
| Outer silhouette | Continuous dark outline, visually 2-4 pixels |
| Internal detail | Major construction lines 2 pixels or thicker |
| Palette | Dark base + one accent family + restrained metal highlight |
| Light | Upper-left or frontal-upper light, consistent within the item |
| Ornament | Enough to imply an artifact, never enough to damage silhouette |
| Semantic effect | Zero or one simple cue, at most about 20% of visible area |
| Text and frame | Not allowed |

The 128x128 icon must remain identifiable in a nearest-neighbor 64x64 preview.

## 4. Source of Truth and Identity

Use an approved independent planning document when available:

```text
Assets/Doc/Relic/item.relic.{relic_slug}.planning.json
```

Evidence priority:

1. Approved item planning `presentationKo`, concept, role, and gameplay effect.
2. Approved item JSON fields and player-facing name/description.
3. Verified existing runtime behavior.
4. Legacy RelicSO and icon evidence.

Do not invent a visual mechanic from an unclear legacy icon. If gameplay sources
conflict, the icon brief must use only the stable physical concept and report the
unresolved effect cue.

Canonical relic identity:

```text
itemId = item.relic.{lowercase_snake_case_slug}
spriteName = {itemId}.icon
filename = {itemId}.icon.png
outputPath = Assets/Resources/item/icon/{itemId}.icon.png
```

Do not overwrite or append cells to `Assets/Resources/shop/relic/relic-icon-*.png`.
Those sheets and their `.meta` fileIDs are legacy references. A new single-Sprite
file prevents unrelated relic references from changing when content is extended.

## 5. PixelLab Tool Contract

Use only:

```text
PixelLab URL: https://www.pixellab.ai/create?tool=create_ui_pro
Tool: Create UI elements (Pro)
Custom size: 128x128
No Background: On
Concept Image: empty
Color Palette: short material and accent palette
```

Do not upload the legacy sheets as Concept Image. Their shared traits have
already been converted into this textual contract, while their inconsistencies
must not be reinforced.

Do not substitute `Create UI elements`, style-reference generation, image-to-pixel
art, a general image generator, or an animation tool. If PixelLab, authentication,
or credits are unavailable, stop and report the blocker.

One run must describe one item. The four cells are four visual alternatives of
the same item, not four different relics.

## 6. Concise Description Contract

The PixelLab Description must contain five short English sentences or lines:

1. `Primary object`: visible physical silhouette, orientation, and main parts.
2. `Material`: construction material and one or two readable ornaments.
3. `Effect cue`: zero or one small cue derived from approved gameplay intent.
4. `Style`: dark-fantasy handcrafted pixel-art rendering and palette hierarchy.
5. `Composition/exclusions`: one isolated item, transparent background, no
   competing objects or UI.

Keep the description focused on visible form. Prefer:

```text
A squat black wax candle in a carved bronze holder, with a broad stable base and
a short bent wick.
```

Avoid lore-heavy or abstract wording such as:

```text
An ancient artifact of despair that consumes every soul in endless darkness.
```

The latter encourages characters, scenes, portals, and oversized magic instead
of a usable item icon.

### 6.1 Common style phrase

```text
handcrafted dark-fantasy pixel-art inventory icon, crisp stepped edges, continuous
dark colored outline, clustered shading, aged materials, one saturated accent,
high readability at small size
```

### 6.2 Common exclusions

```text
no text, no letters, no numbers, no logo, no rarity badge, no border, no card,
no inventory slot, no scenery, no character, no hand holding it, no photorealism,
no smooth vector art, no soft airbrush, no multiple unrelated objects
```

Do not expand the negative list after every failure. Rewrite the one sentence
that caused the mistaken subject, material, effect, or composition.

## 7. Relic Composition Profiles

Choose exactly one profile from the physical item shape:

| Profile | Suitable items | Composition |
|---|---|---|
| `centered_emblem` | gear, shield, medallion, crystal cluster | centered and near-symmetrical |
| `hanging_talisman` | necklace, pouch, charm | centered hanging shape, chain or cord contained |
| `upright_vessel` | candle, vial, urn | vertical axis, stable lower base |
| `diagonal_relic` | feather, nail, dagger | natural diagonal with both ends inside safe margin |
| `curved_relic` | horn, fang, claw | curve fills the center without touching edges |

Do not force every item into the lower-left-to-upper-right diagonal. The profile
must follow the physical object.

## 8. Effect Cue and Background Rules

The physical item is always primary. Use an effect cue only when it improves
recognition of an approved behavior or elemental identity.

Good cues:

- one short smoke curl above a candle;
- a compact frost rim on a nail;
- two or three broad electric branches around a crystal;
- a small poisonous vapor curl at a pouch opening;
- a restrained ember glow inside a necklace gem.

Do not encode exact gameplay values, chances, durations, or effect counts in the
icon. Do not show a full target, attacker, battlefield, or skill impact.

Relic icons use `backgroundMode=transparent` by default. A contextual background
does not match the current relic library and requires explicit art-direction
approval. Colored aura pixels attached to the item are an effect cue, not a
background.

## 9. Candidate Extraction and Preservation

One 128x128 Pro run yields one 256x256 2x2 sheet. Preserve the raw result, then
extract cells without scaling, filtering, redrawing, or recompression:

```text
candidate_00 = top-left     (x=0,   y=0,   128x128)
candidate_01 = top-right    (x=128, y=0,   128x128)
candidate_02 = bottom-left  (x=0,   y=128, 128x128)
candidate_03 = bottom-right (x=128, y=128, 128x128)
```

Use image-coordinate origin at the top-left for this extraction manifest. Do not
confuse it with Unity Sprite Editor's bottom-left rectangle coordinates.

Recommended preservation layout:

```text
{evaluationRoot}/{itemId}/
  source/{itemId}.icon.png
  candidates/attempt_01.sheet.png
  candidates/candidate_00.png
  candidates/candidate_01.png
  candidates/candidate_02.png
  candidates/candidate_03.png
  candidates/candidate_00.preview64.png
  evaluation/generation_record.txt
  evaluation/candidate_scores.txt
```

The evaluation folder is authoritative evidence. Copy the passing preserved
source into Unity; do not use the Unity folder as the only source archive.

## 10. Selection and Retry Rules

Reject a candidate before scoring when it has any of these failures:

- corrupt PNG, wrong cell dimensions, or non-RGBA result;
- cropped item or less than 10 pixels of edge clearance;
- opaque or scene-like background;
- multiple unrelated items, character, hand, text, border, card, or badge;
- the physical object is not the requested item category;
- spell effect is larger or more readable than the item;
- silhouette cannot be recognized in the 64x64 preview.

Score the remaining candidates on a 100-point scale:

| Criterion | Points |
|---|---:|
| Item identity and silhouette | 30 |
| Match to approved concept/effect | 20 |
| Legacy relic style consistency | 20 |
| Pixel-art craft and material readability | 15 |
| Composition, safe margin, 64x64 readability | 15 |

Pass requires at least 85 points and no rejection condition. Preserve at most the
best passing candidate.

If all four candidates fail, change only the failed description block and use a
new seed. Maximum two PixelLab runs per item unless the user approves more.

- wrong object -> rewrite `Primary object` with concrete visible geometry;
- weak silhouette or crop -> simplify parts and strengthen centered scale wording;
- wrong material -> rewrite `Material` and shorten the palette;
- effect dominates -> remove or reduce `Effect cue`;
- extra objects or scenery -> simplify `Composition/exclusions`;
- broad style failure -> rewrite `Style`; do not upload a legacy sheet.

## 11. Unity Handoff

Only after Pass:

1. Preserve the selected 128x128 RGBA candidate as
   `{evaluationRoot}/{itemId}/source/{itemId}.icon.png`.
2. Confirm the source is transparent and its SHA-256 is recorded.
3. If `Assets/Resources/item/icon` does not yet exist, create exactly that
   canonical directory after the candidate passes and let the approved Unity
   import workflow create its folder `.meta`.
4. Copy it byte-for-byte to
   `Assets/Resources/item/icon/{itemId}.icon.png`.
5. Ensure Unity imports it as one Sprite with pivot centered, mipmaps off,
   alpha transparency enabled, and no atlas slicing.
6. Ensure the Sprite name is exactly `{itemId}.icon`, matching Relic JSON `icon`.
7. Preserve an existing `.meta` when replacing an approved icon. For a new icon,
   let the approved Unity import workflow create the `.meta`; do not reuse a GUID.
8. Verify no file under `Assets/Resources/shop/relic` changed.

Do not generate or modify item JSON, RelicSO, EffectSO, localization, shop data,
pool data, reward data, or gameplay balance in this workflow.

## 12. Required Generation Record

Record:

```text
Item ID:
Item Category:
Source Planning / JSON:
PixelLab Creator URL:
Tool / Custom Size / No Background:
Concept Image: none
Composition Profile:
Primary Object Sentence:
Material Sentence:
Effect Cue Sentence: omitted | value
Style Sentence:
Composition / Exclusion Sentence:
Color Palette:
Seed: value | not_exposed
Attempt Count:
Raw Sheet Path / Size:
Candidate Extraction Manifest:
Candidate Scores:
64x64 Preview Result:
Selected Candidate / SHA-256:
Preserved Source Path:
Unity Output Path / Sprite Name:
Unity Meta Status:
Result: Pass | Fail
```

## 13. Failure Types

```text
missing_item_source
invalid_item_id
unsupported_item_category
unresolved_item_concept
missing_visual_direction
pixellab_unavailable
pixellab_authentication_failed
insufficient_pixellab_credits
wrong_pixellab_tool
generation_timeout
invalid_result_sheet
candidate_extraction_failed
no_passing_candidate
preservation_failed
output_write_failed
sprite_name_mismatch
unity_import_pending
```
