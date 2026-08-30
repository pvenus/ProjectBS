from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image, ImageDraw


ROOT = Path(__file__).parent
PROJECT = ROOT.parents[6]


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def smoothstep(lo, hi, values):
    t = np.clip((values - lo) / (hi - lo), 0.0, 1.0)
    return t * t * (3 - 2 * t)


def extract(source):
    rgb = np.asarray(Image.open(source).convert("RGB")).astype(np.float32) / 255
    h, w = rgb.shape[:2]
    border = np.concatenate([rgb[:12].reshape(-1, 3), rgb[-12:].reshape(-1, 3), rgb[:, :12].reshape(-1, 3), rgb[:, -12:].reshape(-1, 3)])
    matte = np.median(border, axis=0)
    distance = np.sqrt(np.sum((rgb - matte) ** 2, axis=2))
    vmax, vmin = rgb.max(2), rgb.min(2)
    saturation = (vmax - vmin) / np.maximum(vmax, 1e-6)
    luminance = rgb @ np.array([0.2126, 0.7152, 0.0722], np.float32)
    alpha = smoothstep(0.05, 0.14, distance)
    alpha = np.maximum(alpha, smoothstep(0.07, 0.20, saturation) * smoothstep(0.90, 0.58, luminance))
    alpha[alpha < 0.015] = 0
    alpha[alpha > 0.985] = 1
    out_rgb = np.zeros_like(rgb)
    opaque = alpha >= 0.999
    partial = (alpha > 0) & ~opaque
    out_rgb[opaque] = rgb[opaque]
    aa = alpha[partial, None]
    out_rgb[partial] = np.clip((rgb[partial] - (1 - aa) * matte) / np.maximum(aa, 1 / 255), 0, 1)
    rgba = np.dstack([np.rint(out_rgb * 255).astype(np.uint8), np.rint(alpha * 255).astype(np.uint8)])
    asset = source.parent.parent
    out_dir = asset / "alpha"
    out_dir.mkdir(exist_ok=True)
    output = out_dir / f"{asset.name}.selected-alpha.png"
    mask = out_dir / f"{asset.name}.mask.png"
    Image.fromarray(rgba, "RGBA").save(output, optimize=False, compress_level=9)
    Image.fromarray(rgba[:, :, 3], "L").save(mask, optimize=False, compress_level=9)
    ys, xs = np.where(rgba[:, :, 3] >= 16)
    metrics = {
        "assetId": asset.name,
        "source": str(source.relative_to(ROOT)),
        "sourceSha256": sha256(source),
        "output": str(output.relative_to(ROOT)),
        "outputSha256": sha256(output),
        "mask": str(mask.relative_to(ROOT)),
        "maskSha256": sha256(mask),
        "size": [w, h],
        "matteRgb": [round(float(value), 6) for value in matte],
        "cornersAlpha": [int(rgba[0, 0, 3]), int(rgba[0, -1, 3]), int(rgba[-1, 0, 3]), int(rgba[-1, -1, 3])],
        "alphaZeroRgbResidue": int(np.count_nonzero(rgba[rgba[:, :, 3] == 0, :3])),
        "alphaPartial": int(((rgba[:, :, 3] > 0) & (rgba[:, :, 3] < 255)).sum()),
        "bboxAlpha16": [int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())],
    }
    metrics_path = out_dir / f"{asset.name}.metrics.json"
    metrics_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n")
    metrics["metrics"] = str(metrics_path.relative_to(ROOT))
    metrics["metricsSha256"] = sha256(metrics_path)
    return output, metrics


selected = [
    ROOT / "Y2/skill.character.jihan.2.active_1.medicine_prescription.icon/selected/selected-B.png",
    ROOT / "Y3/skill.character.jihan.3.active_1.medicine_prescription.icon/selected/selected-C.png",
]
processed = [extract(path) for path in selected]
g1 = PROJECT / "Assets/ImagesGenerated/Skill/icon/skill.character.jihan.1.active_1.medicine_prescription.icon.png"

evidence = ROOT / "pilot-evidence"
evidence.mkdir(exist_ok=True)
contacts = []
for size in (200, 80, 32):
    pad = max(8, size // 8)
    label_h = 18
    cell_w = size + pad * 2
    cell_h = size + pad * 2 + label_h
    canvas = Image.new("RGB", (cell_w * 3, cell_h), "#171b20")
    draw = ImageDraw.Draw(canvas)
    for index, path in enumerate([g1, processed[0][0], processed[1][0]]):
        draw.text((index * cell_w + 4, 2), f"medicine G{index + 1}", fill="#d8d5cc")
        icon = Image.open(path).convert("RGBA")
        icon.thumbnail((size, size), Image.Resampling.LANCZOS)
        x = index * cell_w + pad + (size - icon.width) // 2
        y = pad + label_h + (size - icon.height) // 2
        canvas.paste(icon, (x, y), icon)
    output = evidence / f"medicine-family-contact-{size}.png"
    canvas.save(output, optimize=False, compress_level=9)
    contacts.append({"size": size, "path": str(output.relative_to(ROOT)), "sha256": sha256(output)})

manifest = ROOT / "medicine-pilot-manifest.json"
payload = {
    "status": "MEDICINE_STRIP_VISUAL_ALPHA_READY",
    "selection": {"G2": "candidate-B", "G3": "candidate-C"},
    "visualDecision": "one bowl/core retained; G2 adds paired ingredient cue and open circulation; G3 connects prescription to one outer support flow",
    "assets": [record for _, record in processed],
    "contacts": contacts,
    "canonicalWrite": False,
    "unity": False,
    "staging": False,
}
manifest.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"manifest": str(manifest), "manifestSha256": sha256(manifest), **payload}, ensure_ascii=False, indent=2))
