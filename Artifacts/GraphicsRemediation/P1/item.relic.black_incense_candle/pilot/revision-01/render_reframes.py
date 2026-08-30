from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image

PROJECT = Path(__file__).resolve().parents[6]
SOURCE = PROJECT / "Assets/ImagesGenerated/Item/icon/item.relic.black_incense_candle.icon.png"
ROOT = Path(__file__).parent
OUT = ROOT / "reframe-batch-01"
TARGET_MAX = {"A": 0.72, "B": 0.76, "C": 0.80}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def premultiplied_resize(image, size):
    arr = np.asarray(image.convert("RGBA")).astype(np.float32) / 255.0
    alpha = arr[:, :, 3:4]
    premul = arr[:, :, :3] * alpha
    planes = []
    for channel in range(3):
        planes.append(np.asarray(Image.fromarray(premul[:, :, channel], "F").resize(size, Image.Resampling.LANCZOS)))
    out_alpha = np.asarray(Image.fromarray(alpha[:, :, 0], "F").resize(size, Image.Resampling.LANCZOS))
    out_premul = np.stack(planes, axis=2)
    out_rgb = np.zeros_like(out_premul)
    visible = out_alpha > (1 / 255)
    out_rgb[visible] = np.clip(out_premul[visible] / out_alpha[visible, None], 0, 1)
    rgba = np.dstack([np.rint(out_rgb * 255).astype(np.uint8), np.rint(np.clip(out_alpha, 0, 1) * 255).astype(np.uint8)])
    rgba[rgba[:, :, 3] == 0, :3] = 0
    return Image.fromarray(rgba, "RGBA")


source = Image.open(SOURCE).convert("RGBA")
a = np.asarray(source)[:, :, 3]
ys, xs = np.where(a >= 16)
bbox = (int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1)
subject = source.crop(bbox)
OUT.mkdir(parents=True, exist_ok=True)
records = []
for name, target in TARGET_MAX.items():
    scale = target * source.height / max(subject.width, subject.height)
    size = (max(1, round(subject.width * scale)), max(1, round(subject.height * scale)))
    resized = premultiplied_resize(subject, size)
    canvas = Image.new("RGBA", source.size, (0, 0, 0, 0))
    offset = ((source.width - size[0]) // 2, (source.height - size[1]) // 2)
    canvas.alpha_composite(resized, offset)
    output = OUT / f"candidate-{name}.png"
    canvas.save(output, optimize=False, compress_level=9)
    out_a = np.asarray(canvas)[:, :, 3]
    oy, ox = np.where(out_a >= 16)
    records.append({
        "candidate": name,
        "targetMaxPercent": target * 100,
        "path": str(output.relative_to(PROJECT)),
        "sha256": sha(output),
        "bboxPercent": [round((ox.max() - ox.min() + 1) / source.width * 100, 3), round((oy.max() - oy.min() + 1) / source.height * 100, 3)],
        "cornersAlpha": [int(out_a[0, 0]), int(out_a[0, -1]), int(out_a[-1, 0]), int(out_a[-1, -1])],
        "alphaZeroRgbResidue": int(np.count_nonzero(np.asarray(canvas)[:, :, :3][out_a == 0])),
        "operation": {"cropToSourceAlpha16BBox": list(bbox), "uniformScale": scale, "centerOffset": list(offset)}
    })

manifest = ROOT / "reframe-batch-01-manifest.json"
manifest.write_text(json.dumps({
    "assetId": "item.relic.black_incense_candle",
    "source": str(SOURCE.relative_to(PROJECT)),
    "sourceSha256": sha(SOURCE),
    "operation": "identity-preserving deterministic crop-to-alpha-bbox, uniform scale, centered transparent canvas",
    "candidates": records,
    "canonicalWrite": False,
    "metaWrite": False,
    "unity": False,
    "staging": False
}, indent=2) + "\n")
print(json.dumps({"manifest": str(manifest), "manifestSha256": sha(manifest), "candidates": records}, indent=2))
