from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image

SOURCE = Path("Assets/ImagesGenerated/Item/icon/item.relic.old_war_horn.icon.png")
ROOT = Path("Artifacts/GraphicsRemediation/P1/item.relic.old_war_horn/pilot/revision-01/reframe-batch-01")
TARGET_MAX = {"A": 0.80, "B": 0.74, "C": 0.68}


def srgb_to_linear(v):
    return np.where(v <= 0.04045, v / 12.92, ((v + 0.055) / 1.055) ** 2.4)


def linear_to_srgb(v):
    return np.where(v <= 0.0031308, v * 12.92, 1.055 * np.power(v, 1 / 2.4) - 0.055)


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def bbox_metrics(im):
    a = np.asarray(im.getchannel("A"))
    ys, xs = np.where(a >= 16)
    box = [int(xs.min()), int(ys.min()), int(xs.max() + 1), int(ys.max() + 1)]
    return {
        "alpha16_bbox": box,
        "bbox_percent": [round((box[2] - box[0]) / im.width * 100, 4), round((box[3] - box[1]) / im.height * 100, 4)],
        "margins_percent": [round(box[0] / im.width * 100, 4), round(box[1] / im.height * 100, 4), round((im.width - box[2]) / im.width * 100, 4), round((im.height - box[3]) / im.height * 100, 4)],
        "corner_alpha": [int(a[0, 0]), int(a[0, -1]), int(a[-1, 0]), int(a[-1, -1])],
        "partial_alpha_pixels": int(((a > 0) & (a < 255)).sum()),
    }


def main():
    ROOT.mkdir(parents=True, exist_ok=True)
    src = Image.open(SOURCE).convert("RGBA")
    src_np = np.asarray(src).astype(np.float32) / 255.0
    alpha = src_np[..., 3:4]
    linear_pm = srgb_to_linear(src_np[..., :3]) * alpha
    packed = np.concatenate([linear_pm, alpha], axis=2)
    base = bbox_metrics(src)
    base_max = max(base["bbox_percent"]) / 100.0
    manifest = {"source": str(SOURCE), "source_sha256": sha256(SOURCE), "source_metrics": base, "algorithm": "linear-light premultiplied-alpha Lanczos; uniform scale; alpha-bbox centered; transparent RGB zero", "candidates": {}}

    for name, target in TARGET_MAX.items():
        scale = target / base_max
        size = tuple(max(1, round(v * scale)) for v in src.size)
        channels = []
        for i in range(4):
            channel = Image.fromarray(packed[..., i], mode="F").resize(size, Image.Resampling.LANCZOS)
            channels.append(np.asarray(channel, dtype=np.float32))
        resized = np.stack(channels, axis=2)
        a = np.clip(resized[..., 3:4], 0, 1)
        rgb_lin = np.divide(resized[..., :3], a, out=np.zeros_like(resized[..., :3]), where=a > 1e-7)
        rgb = np.clip(linear_to_srgb(np.clip(rgb_lin, 0, 1)), 0, 1)
        rgba = np.concatenate([rgb, a], axis=2)
        rgba[a[..., 0] <= 0] = 0
        small = Image.fromarray(np.rint(rgba * 255).astype(np.uint8), "RGBA")

        small_a = np.asarray(small.getchannel("A"))
        ys, xs = np.where(small_a >= 16)
        cx = (float(xs.min()) + float(xs.max()) + 1.0) / 2.0
        cy = (float(ys.min()) + float(ys.max()) + 1.0) / 2.0
        left = round(src.width / 2.0 - cx)
        top = round(src.height / 2.0 - cy)
        canvas = Image.new("RGBA", src.size, (0, 0, 0, 0))
        canvas.alpha_composite(small, (left, top))
        out = ROOT / f"candidate-{name}.png"
        canvas.save(out, optimize=False, compress_level=9)
        manifest["candidates"][name] = {"path": str(out), "target_max_bbox": target, "uniform_scale": round(scale, 8), "placement": [left, top], "sha256": sha256(out), "metrics": bbox_metrics(canvas)}

    (ROOT / "metrics.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
