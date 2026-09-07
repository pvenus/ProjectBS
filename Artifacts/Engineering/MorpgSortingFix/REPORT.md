# MORPG body visibility / sorting regression fix

The environment background was assigned Default layer order-90, and all props order-50. Character AnimationMono uses Util.SortingOrderMono, whose legacy order is offset − round(worldY×100). At Y5 a body has order-500, below the opaque full-map environment background; health bars at200 remain visible. This reproduces the reported body-hidden/HUD-visible ordering condition. The environment alpha loop already targeted only its own prop renderers, so it was not setting character alpha to zero.

## Explicit shared policy

All these world renderers use the existing Default sorting layer (ID0). No new layer is introduced.

| Element | MORPG order |
| --- | --- |
| Existing and environment backgrounds | -1000 |
| Normal/back props | -900 |
| Player/NPC body, existing Y order mapped into bounded band | -800 through0 |
| Overlapping foreground props | 50 |
| World ground telegraph | 100 |
| Existing health bars | 200/201/202 |
| Reward presentation | ScreenSpaceOverlay Canvas250, above world renderers |

The body mapping lives in BattlePresentationSortingPolicy and is called from the existing SortingOrderMono calculation. It retains Y ordering with a bounded range and applies only to actors registered with the currently active exact MORPG environment. Foreign actors and other battles retain the old formula. AnimationMono's presentation copies inherit the source renderer's sorting layer/order. The selected NPC prefab has layerID0/order0 initially; the runtime body sorter then applies the band. CharacterBuilder does not install a separate sorting layer/group override. No SortingGroup was found in the inspected battle prefabs/scripts. Environment root is detached from scene ancestors so an unexpected parent SortingGroup cannot collapse its background/foreground ordering. Environment renderers use world Z0; ordering no longer relies on Z ties.

Overlap promotes only the owned prop to foreground50 and fades that prop to alpha.35 in.12 scaled seconds. Separation restores the prop to back order-900 and alpha1 in.18 seconds. Character color/alpha, material and MPB are never written by the environment. Normal props and the full-map background cannot cover the body band. Existing health-bar renderer policy stays unchanged and is excluded by the body sorter's existing HUD filter.

On sorter disable (including pooling), a previously applied MORPG order restores the legacy Y result. Route disposal clears environment ownership first, then refreshes actor sorters back to legacy. Reload creates new prop renderers with default alpha1. No actor material or alpha restoration is needed because neither was modified.

## Verification and limits

- Installed Unity C# compiler0 errors/31 existing warnings.
- Live route + production environment + production SortingOrderMono/policy26/26 under Unity stubs. Three new focused tests cover player and NPC at Y−7.35/−5/0/5/7.35, background/back/body/foreground/telegraph/HUD inequalities, Default layer agreement and ancestor-group isolation; prop-only overlap fade/restore with body alpha1; pooling/reenable/dispose sorting restoration and a foreign actor retaining legacy order.
- Production geometry10/10; reward queue/ledger12/12; inactive spawn10/10; cast presenter4/4 plus shader/order checks all pass.
- Source audit confirms the environment contains no material/sharedMaterial or SetPropertyBlock writes. Its color write is confined to the list populated from environment-owned Show() renderers.
- git diff --check and sorting-only.diff reverse --check pass; reverse patch not executed, index unchanged. No Unity GUI, scene changes, staging, commit or push.

No screenshot was directly supplied to this task and no Unity gameplay/render run was performed. The reported symptom is explained and reproduced as a production-code sorting-order regression, not claimed as a pixel-identical screenshot replay. GPU/material visual confirmation remains an engine check.

Detailed lead handoff remains local due to the prior automatic approval rejection of that destination. No alternative delivery channel was used.
