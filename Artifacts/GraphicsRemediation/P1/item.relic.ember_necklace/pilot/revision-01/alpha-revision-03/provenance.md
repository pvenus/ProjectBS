# Ember Necklace Alpha Revision 03

- Authority source: selected-C design, processed through the R2 RGBA only as fixed RGB/placement input.
- New method: manually defined definite contour; one-pixel antialias coverage reconstructed only along supported contour normals.
- Problem zones: rope-loop inner/outer edge and knot/tag junction.
- Unsupported neutral edge specks, wedges, and source-matte oval were explicitly excluded; alpha-zero RGB was zeroed.
- Preserved: rope/tag RGB foreground, geometry, placement, interior dry-brush holes, palette, and meaning.
- Not used: R2 trimap, global distance mask, uniform attenuation, broad matte decontamination, crop, scale, repaint, ImageGen.
- Production budget: production1, correction0.
- Visual result: PASS on neutral and dark backgrounds; no visible halo/fringe, choke, or design change.

Reproduction: run `author_contour_r3.py` with the bundled Pillow/NumPy runtime against the fixed R2 authority source.
