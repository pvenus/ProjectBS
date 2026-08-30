from pathlib import Path
from PIL import Image, ImageFilter
import hashlib, json
import numpy as np

ROOT = Path(__file__).resolve().parent
SRC = ROOT.parent / "alpha-revision-02" / "item.relic.ember_necklace.icon.selected-C-alpha-r2.png"
OUT = ROOT / "item.relic.ember_necklace.icon.selected-C-alpha-r3.png"
MASK = ROOT / "manual-contour-alpha-r3.png"
CONTACT = ROOT / "neutral-dark-contact-r3.png"
METRICS = ROOT / "metrics-r3.json"

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

im = Image.open(SRC).convert("RGBA")
a = np.array(im, dtype=np.uint8)
old_alpha = a[:, :, 3]
h, w = old_alpha.shape

# The hand-authored authority is the definite visible contour (alpha >= 128).
# Only its one-pixel outward normal band may carry reconstructed AA coverage.
core = old_alpha >= 128
core_img = Image.fromarray((core * 255).astype(np.uint8), "L")
dilated = np.array(core_img.filter(ImageFilter.MaxFilter(3))) > 0
outer_band = dilated & ~core

# A valid AA contour sample must be supported by at least two neighbouring
# definite-foreground pixels. Singleton/diagonal-only support creates the gray
# beads and wedges seen in the rejected dark contact.
padded_core = np.pad(core.astype(np.uint8), 1)
support = np.zeros_like(old_alpha, dtype=np.uint8)
for dy in range(3):
    for dx in range(3):
        if dx == 1 and dy == 1: continue
        support += padded_core[dy:dy+h, dx:dx+w]
outer_band &= support >= 2

new_alpha = np.zeros_like(old_alpha)
new_alpha[core] = old_alpha[core]
new_alpha[outer_band] = np.maximum(32, np.minimum(127, old_alpha[outer_band]))

# Preserve enclosed dry-brush holes: pixels not connected to canvas exterior stay
# transparent/partial exactly as authored. Exterior disconnected veil/specks are zero.
background = ~core
seen = np.zeros_like(background, dtype=bool)
stack = []
for x in range(w):
    if background[0, x]: stack.append((0, x))
    if background[h-1, x]: stack.append((h-1, x))
for y in range(h):
    if background[y, 0]: stack.append((y, 0))
    if background[y, w-1]: stack.append((y, w-1))
while stack:
    y, x = stack.pop()
    if seen[y, x] or not background[y, x]:
        continue
    seen[y, x] = True
    if y: stack.append((y-1, x))
    if y+1 < h: stack.append((y+1, x))
    if x: stack.append((y, x-1))
    if x+1 < w: stack.append((y, x+1))
enclosed = background & ~seen
new_alpha[enclosed] = old_alpha[enclosed]

# Manual problem-zone contour cleanup: neutral matte wedges/specks embedded in the
# old edge are recognized only within two pixels of the definite silhouette edge.
# Warm rope highlights deeper inside the foreground are intentionally untouched.
eroded = np.array(core_img.filter(ImageFilter.MinFilter(5))) > 0
inner_edge_zone = core & ~eroded
chroma = a[:, :, :3].max(axis=2).astype(np.int16) - a[:, :, :3].min(axis=2).astype(np.int16)
neutral_matte_speck = inner_edge_zone & (a[:, :, :3].min(axis=2) >= 220)
# One source-matte oval remained attached to the rope's inner contour; it has no
# foreground continuity and is explicitly excluded by the manual contour.
yy_grid, xx_grid = np.ogrid[:h, :w]
neutral_matte_speck |= ((xx_grid - 451) ** 2 + (yy_grid - 573) ** 2 <= 9 ** 2) & ~eroded
new_alpha[neutral_matte_speck] = 0

# Explicit manual-contour rule: the necklace is one continuous foreground object.
# Any remaining alpha island without 8-neighbour continuity to that object is an
# unsupported speck/wedge and is removed with RGB zeroing below.
vis = new_alpha > 0
visited = np.zeros_like(vis, dtype=bool)
components = []
for sy, sx in zip(*np.where(vis & ~visited)):
    if visited[sy, sx]:
        continue
    comp = []
    stack = [(int(sy), int(sx))]
    visited[sy, sx] = True
    while stack:
        cy, cx = stack.pop(); comp.append((cy, cx))
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                if not (dx or dy): continue
                ny, nx = cy + dy, cx + dx
                if 0 <= ny < h and 0 <= nx < w and vis[ny, nx] and not visited[ny, nx]:
                    visited[ny, nx] = True; stack.append((ny, nx))
    components.append(comp)
components.sort(key=len, reverse=True)
disconnected_removed = 0
for comp in components[1:]:
    disconnected_removed += len(comp)
    yy, xx = zip(*comp)
    new_alpha[np.array(yy), np.array(xx)] = 0

# Rebuild RGB only for the one-pixel AA band from adjacent definite foreground.
# This is local contour-normal extrapolation, not repaint or broad decontamination.
rgb = a[:, :, :3].copy()
ys, xs = np.where(outer_band & (new_alpha > 0))
for y, x in zip(ys.tolist(), xs.tolist()):
    y0, y1 = max(0, y-1), min(h, y+2)
    x0, x1 = max(0, x-1), min(w, x+2)
    local_core = core[y0:y1, x0:x1]
    local_rgb = rgb[y0:y1, x0:x1]
    if local_core.any():
        rgb[y, x] = np.median(local_rgb[local_core], axis=0).astype(np.uint8)
    else:
        new_alpha[y, x] = 0

rgb[new_alpha == 0] = 0
out = np.dstack([rgb, new_alpha]).astype(np.uint8)
Image.fromarray(out, "RGBA").save(OUT, optimize=False, compress_level=9)
Image.fromarray(new_alpha, "L").save(MASK, optimize=False, compress_level=9)

def composite(bg):
    base = Image.new("RGBA", im.size, bg + (255,))
    base.alpha_composite(Image.fromarray(out, "RGBA"))
    return base.convert("RGB")
neutral = composite((216, 213, 204))
dark = composite((18, 19, 21))
contact = Image.new("RGB", (w*2, h), (0, 0, 0))
contact.paste(neutral, (0, 0)); contact.paste(dark, (w, 0))
contact.save(CONTACT, optimize=False, compress_level=9)

visible = new_alpha > 0
partial = (new_alpha > 0) & (new_alpha < 255)
ysv, xsv = np.where(new_alpha >= 16)
metrics = {
    "method": "manual-definite-contour plus one-pixel normal AA reconstruction",
    "source": str(SRC), "sourceSha256": sha(SRC),
    "outputSha256": sha(OUT), "maskSha256": sha(MASK),
    "contactSha256": sha(CONTACT), "dimensions": [w, h],
    "visiblePixels": int(visible.sum()), "partialAlphaPixels": int(partial.sum()),
    "alphaZeroRgbResidue": int(np.count_nonzero(rgb[new_alpha == 0])),
    "cornerAlpha": [int(new_alpha[0,0]), int(new_alpha[0,-1]), int(new_alpha[-1,0]), int(new_alpha[-1,-1])],
    "bboxAlpha16": [int(xsv.min()), int(ysv.min()), int(xsv.max()+1), int(ysv.max()+1)],
    "outerBandPixels": int(outer_band.sum()),
    "manualNeutralEdgeSpecksRemoved": int(neutral_matte_speck.sum()),
    "disconnectedAlphaIslandsRemoved": int(disconnected_removed),
    "removedDisconnectedOrExteriorPixels": int(((old_alpha > 0) & (new_alpha == 0)).sum()),
    "constraints": ["no crop", "no scale", "no geometry repaint", "no broad matte decontamination", "no R2 trimap reuse"]
}
METRICS.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(metrics, ensure_ascii=False))
