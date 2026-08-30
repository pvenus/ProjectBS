from pathlib import Path
import hashlib
import json
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "selected/item.relic.red_feather.icon.selected-B.png"
OUT_DIR = ROOT / "alpha-revision-01"
OUT_DIR.mkdir(parents=True, exist_ok=True)
OUTPUT = OUT_DIR / "item.relic.red_feather.icon.selected-B-alpha.png"
MASK = OUT_DIR / "mask.png"
METRICS = OUT_DIR / "metrics.json"

im = Image.open(SOURCE).convert("RGB")
rgb8 = np.asarray(im, dtype=np.uint8)
rgb = rgb8.astype(np.float32)
# Fit the smooth generated matte from the guaranteed-empty outer 12% using a
# quadratic surface per RGB channel. Foreground is the residual from that matte.
h, w = rgb.shape[:2]
yy, xx = np.mgrid[0:h, 0:w]
xn = (xx / (w - 1)) * 2.0 - 1.0
yn = (yy / (h - 1)) * 2.0 - 1.0
features = np.stack([np.ones_like(xn), xn, yn, xn*xn, yn*yn, xn*yn], axis=2)
border = (xx < int(w*.12)) | (xx >= int(w*.88)) | (yy < int(h*.12)) | (yy >= int(h*.88))
sample = border & ((xx % 4) == 0) & ((yy % 4) == 0)
X = features[sample]
bg = np.empty_like(rgb)
for c in range(3):
    coef, *_ = np.linalg.lstsq(X, rgb[:, :, c][sample], rcond=None)
    bg[:, :, c] = np.tensordot(features, coef, axes=([2], [0]))
delta = np.sqrt(np.sum((rgb - bg) ** 2, axis=2))
mx = rgb.max(axis=2)
mn = rgb.min(axis=2)
chroma = mx - mn
luma = 0.2126 * rgb[:, :, 0] + 0.7152 * rgb[:, :, 1] + 0.0722 * rgb[:, :, 2]
# Coverage follows colored safflower/hemp pixels or genuinely dark ink. Pale
# neutral matte pixels are excluded instead of being promoted by RGB distance.
a_chroma = np.clip((chroma - 16.0) * 11.0, 0.0, 255.0)
a_dark = np.clip((192.0 - luma) * 8.0, 0.0, 255.0)
alpha = np.maximum(a_chroma, a_dark).astype(np.uint8)
# Manual trimap authority: only the feather/knot/shaft envelope and the two
# approved speed-stroke envelopes can contain unknown or foreground pixels.
roi_im = Image.new("L", (w, h), 0)
draw = ImageDraw.Draw(roi_im)
draw.polygon([(52,760),(55,815),(225,855),(340,805),(420,770),(555,805),(760,780),(970,680),(1185,520),(1185,380),(1040,365),(820,410),(610,485),(430,585),(275,690)], fill=255)
draw.polygon([(115,500),(390,480),(390,655),(100,690)], fill=255)
roi = np.asarray(roi_im, dtype=np.uint8) > 0
alpha[~roi] = 0
alpha[alpha < 5] = 0

# Partial-edge-only matte decontamination against the locked raw matte family.
matte = bg
af = alpha.astype(np.float32) / 255.0
out_rgb = rgb.copy()
partial = (alpha > 0) & (alpha < 255)
safe = np.maximum(af[partial, None], 1.0 / 255.0)
fg = matte[partial] + (rgb[partial] - matte[partial]) / safe
# Evidence-based correction1: prevent the fitted light matte from producing a
# bright fringe. This changes partial-edge RGB only; opaque artwork is untouched.
out_rgb[partial] = np.minimum(np.clip(fg, 0.0, 255.0), rgb[partial] * 0.55)
out_rgb[alpha == 0] = 0

rgba = np.dstack([out_rgb.astype(np.uint8), alpha])
Image.fromarray(alpha, "L").save(MASK)
Image.fromarray(rgba, "RGBA").save(OUTPUT)

visible = alpha >= 16
ys, xs = np.where(visible)
bbox = [int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1]
metrics = {
    "assetId": "item.relic.red_feather",
    "selected": "B",
    "source": str(SOURCE),
    "output": str(OUTPUT),
    "size": [w, h],
    "cornersAlpha": [int(alpha[0,0]), int(alpha[0,-1]), int(alpha[-1,0]), int(alpha[-1,-1])],
    "alphaZeroRgbResidue": int(np.count_nonzero(rgba[:, :, :3][alpha == 0])),
    "alphaPartial": int(np.count_nonzero((alpha > 0) & (alpha < 255))),
    "bboxAlpha16": bbox,
    "bboxPercent": [round((bbox[2]-bbox[0])*100/w, 3), round((bbox[3]-bbox[1])*100/h, 3)],
    "matteModel": "quadratic RGB surface fit from outer 12%",
    "opaqueRgbMismatch": int(np.count_nonzero(out_rgb[alpha == 255].astype(np.uint8) != rgb8[alpha == 255])),
    "canonicalWrite": False,
    "metaWrite": False,
    "unity": False,
    "staging": False,
}

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

metrics["sourceSha256"] = sha(SOURCE)
metrics["outputSha256"] = sha(OUTPUT)
metrics["maskSha256"] = sha(MASK)
METRICS.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n")
metrics["metricsSha256"] = sha(METRICS)
print(json.dumps(metrics, ensure_ascii=False, indent=2))
