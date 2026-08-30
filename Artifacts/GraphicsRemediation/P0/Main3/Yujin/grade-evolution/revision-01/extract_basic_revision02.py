from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image


ROOT = Path(__file__).parent


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def smoothstep(lo, hi, values):
    t = np.clip((values - lo) / (hi - lo), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def extract(source):
    rgb = np.asarray(Image.open(source).convert("RGB")).astype(np.float32) / 255.0
    height, width = rgb.shape[:2]
    side = max(height, width)
    border = np.concatenate(
        [rgb[:12].reshape(-1, 3), rgb[-12:].reshape(-1, 3), rgb[:, :12].reshape(-1, 3), rgb[:, -12:].reshape(-1, 3)]
    )
    matte = np.median(border, axis=0)
    canvas = np.empty((side, side, 3), np.float32)
    canvas[:] = matte
    offset_y = (side - height) // 2
    offset_x = (side - width) // 2
    canvas[offset_y : offset_y + height, offset_x : offset_x + width] = rgb
    distance = np.sqrt(np.sum((canvas - matte) ** 2, axis=2))
    vmax = canvas.max(2)
    vmin = canvas.min(2)
    saturation = (vmax - vmin) / np.maximum(vmax, 1e-6)
    luminance = canvas @ np.array([0.2126, 0.7152, 0.0722], np.float32)
    alpha = smoothstep(0.055, 0.145, distance)
    alpha = np.maximum(alpha, smoothstep(0.08, 0.22, saturation) * smoothstep(0.88, 0.55, luminance))
    alpha[alpha < 0.015] = 0
    alpha[alpha > 0.985] = 1
    output_rgb = np.zeros_like(canvas)
    opaque = alpha >= 0.999
    partial = (alpha > 0) & ~opaque
    output_rgb[opaque] = canvas[opaque]
    aa = alpha[partial, None]
    output_rgb[partial] = np.clip((canvas[partial] - (1 - aa) * matte) / np.maximum(aa, 1 / 255), 0, 1)
    rgba = np.dstack([np.rint(output_rgb * 255).astype(np.uint8), np.rint(alpha * 255).astype(np.uint8)])

    asset = source.parent.parent
    out_dir = asset / "alpha-revision-02"
    out_dir.mkdir(exist_ok=True)
    asset_id = asset.name
    output = out_dir / f"{asset_id}.selected-alpha-r2.png"
    mask = out_dir / f"{asset_id}.mask-r2.png"
    Image.fromarray(rgba, "RGBA").save(output, optimize=False, compress_level=9)
    Image.fromarray(rgba[:, :, 3], "L").save(mask, optimize=False, compress_level=9)
    ys, xs = np.where(rgba[:, :, 3] >= 16)
    metrics = {
        "assetId": asset_id,
        "source": str(source.relative_to(ROOT)),
        "sourceSha256": sha256(source),
        "output": str(output.relative_to(ROOT)),
        "outputSha256": sha256(output),
        "mask": str(mask.relative_to(ROOT)),
        "maskSha256": sha256(mask),
        "sourceSize": [width, height],
        "normalizedSize": [side, side],
        "padOffset": [offset_x, offset_y],
        "alphaZeroRgbResidue": int(np.count_nonzero(rgba[rgba[:, :, 3] == 0, :3])),
        "cornersAlpha": [int(rgba[0, 0, 3]), int(rgba[0, -1, 3]), int(rgba[-1, 0, 3]), int(rgba[-1, -1, 3])],
        "alphaPartial": int(((rgba[:, :, 3] > 0) & (rgba[:, :, 3] < 255)).sum()),
        "bboxAlpha16": [int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())],
    }
    metrics_path = out_dir / f"{asset_id}.metrics-r2.json"
    metrics_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n")
    metrics["metrics"] = str(metrics_path.relative_to(ROOT))
    metrics["metricsSha256"] = sha256(metrics_path)
    return metrics


sources = [
    ROOT / "Y2/skill.character.yujin.2.basic_attack.basic_attack.icon/selected-revision-02/selected-B.png",
    ROOT / "Y3/skill.character.yujin.3.basic_attack.basic_attack.icon/selected-revision-02/selected-A.png",
]
results = [extract(source) for source in sources]
manifest = ROOT / "basic-revision-02-manifest.json"
manifest.write_text(json.dumps(results, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(results, ensure_ascii=False, indent=2))
