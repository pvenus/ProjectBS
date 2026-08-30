from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "captain2-rev05-selected-A-alpha-r2.png"
OUT_A = ROOT / "captain2-rev05-selected-A-alpha-r2-runtime512-A.png"
OUT_B = ROOT / "captain2-rev05-selected-A-alpha-r2-runtime512-B.png"
MANIFEST = ROOT / "captain2-rev05-runtime512-r2-manifest.json"


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def srgb_to_linear(rgb):
    return np.where(rgb <= 0.04045, rgb / 12.92,
                    ((rgb + 0.055) / 1.055) ** 2.4)


def linear_to_srgb(rgb):
    return np.where(rgb <= 0.0031308, rgb * 12.92,
                    1.055 * np.power(rgb, 1.0 / 2.4) - 0.055)


def resize(source):
    rgba = np.asarray(source.convert("RGBA"), dtype=np.float32) / 255.0
    alpha = rgba[:, :, 3]
    linear = srgb_to_linear(rgba[:, :, :3])
    premul = linear * alpha[:, :, None]
    target = (round(source.width * 512 / source.height), 512)
    channels = [np.asarray(Image.fromarray(premul[:, :, i], "F").resize(
        target, Image.Resampling.LANCZOS), dtype=np.float32) for i in range(3)]
    out_alpha = np.asarray(Image.fromarray(alpha, "F").resize(
        target, Image.Resampling.LANCZOS), dtype=np.float32)
    out_premul = np.stack(channels, axis=2)
    out_linear = np.zeros_like(out_premul)
    visible = out_alpha > (1.0 / 255.0)
    out_linear[visible] = np.clip(out_premul[visible] / out_alpha[visible, None], 0, 1)
    out_rgb = np.rint(np.clip(linear_to_srgb(out_linear), 0, 1) * 255).astype(np.uint8)
    out_a = np.rint(np.clip(out_alpha, 0, 1) * 255).astype(np.uint8)
    out = np.dstack([out_rgb, out_a])
    out[out_a == 0, :3] = 0
    return Image.fromarray(out, "RGBA")


def metrics(image):
    arr = np.asarray(image.convert("RGBA"))
    alpha = arr[:, :, 3]
    ys, xs = np.where(alpha > 0)
    return {
        "dimensions": [image.width, image.height],
        "transparent": int(np.count_nonzero(alpha == 0)),
        "partial": int(np.count_nonzero((alpha > 0) & (alpha < 255))),
        "opaque": int(np.count_nonzero(alpha == 255)),
        "bbox": [int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1],
        "corners": [int(alpha[0, 0]), int(alpha[0, -1]), int(alpha[-1, 0]), int(alpha[-1, -1])],
        "alphaZeroRgbResidue": int(np.count_nonzero(arr[:, :, :3][alpha == 0])),
    }


source = Image.open(SOURCE).convert("RGBA")
render_a = resize(source)
render_b = resize(source)
render_a.save(OUT_A, optimize=False, compress_level=9)
render_b.save(OUT_B, optimize=False, compress_level=9)
if OUT_A.read_bytes() != OUT_B.read_bytes():
    raise RuntimeError("A/B byte identity failed")
MANIFEST.write_text(json.dumps({
    "source": str(SOURCE),
    "sourceSha256": sha256(SOURCE),
    "algorithm": "sRGB to linear-light, premultiplied-alpha Lanczos3, max-height512, unpremultiply, sRGB RGBA8, alpha0 RGB zero, PNG compression9",
    "runtimeA": str(OUT_A),
    "runtimeB": str(OUT_B),
    "runtimeSha256": sha256(OUT_A),
    "sourceMetrics": metrics(source),
    "runtimeMetrics": metrics(render_a),
    "decodedMismatch": int(np.count_nonzero(np.asarray(render_a) != np.asarray(render_b))),
}, indent=2) + "\n")
print(MANIFEST)
print(sha256(MANIFEST))
