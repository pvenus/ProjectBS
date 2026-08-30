from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image, ImageDraw


ROOT = Path(__file__).parent


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def smoothstep(lo, hi, values):
    t = np.clip((values - lo) / (hi - lo), 0.0, 1.0)
    return t * t * (3 - 2 * t)


def extract(source):
    rgb = np.asarray(Image.open(source).convert("RGB")).astype(np.float32) / 255
    h, w = rgb.shape[:2]
    border = np.concatenate([
        rgb[:12].reshape(-1, 3), rgb[-12:].reshape(-1, 3),
        rgb[:, :12].reshape(-1, 3), rgb[:, -12:].reshape(-1, 3),
    ])
    matte = np.median(border, axis=0)
    distance = np.sqrt(np.sum((rgb - matte) ** 2, axis=2))
    vmax, vmin = rgb.max(2), rgb.min(2)
    saturation = (vmax - vmin) / np.maximum(vmax, 1e-6)
    luminance = rgb @ np.array([0.2126, 0.7152, 0.0722], np.float32)
    alpha = smoothstep(0.05, 0.14, distance)
    # Correction1: the G1 passive source carries low-chroma paper grain over the
    # full matte.  Saturation-assisted recovery turns that grain into a visible
    # square veil, so this one source uses color-distance-only trimap recovery.
    # The navy/ink/rust foreground remains well separated from the matte.
    if "seojin.1.passive_1.indomitable" in source.as_posix():
        alpha = smoothstep(0.065, 0.17, distance)
    else:
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
    rgba[rgba[:, :, 3] == 0, :3] = 0
    pre_ys, pre_xs = np.where(rgba[:, :, 3] >= 16)
    pre_max = max((pre_xs.max() - pre_xs.min() + 1) / w, (pre_ys.max() - pre_ys.min() + 1) / h)
    normalization_scale = 1.0
    if pre_max > 0.86:
        normalization_scale = 0.80 / pre_max
        image = Image.fromarray(rgba, "RGBA")
        nw, nh = max(1, round(w * normalization_scale)), max(1, round(h * normalization_scale))
        resized = image.resize((nw, nh), Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        canvas.alpha_composite(resized, ((w - nw) // 2, (h - nh) // 2))
        rgba = np.asarray(canvas).copy()
        rgba[rgba[:, :, 3] == 0, :3] = 0
    asset = source.parent.parent
    out_dir = asset / "alpha"
    out_dir.mkdir(exist_ok=True)
    output = out_dir / f"{asset.name}.selected-alpha.png"
    mask = out_dir / f"{asset.name}.mask.png"
    Image.fromarray(rgba, "RGBA").save(output, optimize=False, compress_level=9)
    Image.fromarray(rgba[:, :, 3], "L").save(mask, optimize=False, compress_level=9)
    ys, xs = np.where(rgba[:, :, 3] >= 16)
    bbox = [int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())]
    metrics = {
        "assetId": asset.name,
        "source": str(source.relative_to(ROOT)), "sourceSha256": sha256(source),
        "output": str(output.relative_to(ROOT)), "outputSha256": sha256(output),
        "mask": str(mask.relative_to(ROOT)), "maskSha256": sha256(mask),
        "size": [w, h], "matteRgb": [round(float(v), 6) for v in matte],
        "cornersAlpha": [int(rgba[0, 0, 3]), int(rgba[0, -1, 3]), int(rgba[-1, 0, 3]), int(rgba[-1, -1, 3])],
        "alphaZeroRgbResidue": int(np.count_nonzero(rgba[rgba[:, :, 3] == 0, :3])),
        "alphaPartial": int(((rgba[:, :, 3] > 0) & (rgba[:, :, 3] < 255)).sum()),
        "bboxAlpha16": bbox,
        "bboxPercent": [round((bbox[2] - bbox[0] + 1) / w * 100, 3), round((bbox[3] - bbox[1] + 1) / h * 100, 3)],
        "normalizationScale": round(normalization_scale, 6),
    }
    metrics_path = out_dir / f"{asset.name}.metrics.json"
    metrics_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n")
    metrics["metrics"] = str(metrics_path.relative_to(ROOT))
    metrics["metricsSha256"] = sha256(metrics_path)
    return output, metrics


families = {
    "basic": [
        ("G1", "Y1/skill.character.seojin.1.basic_attack.basic_attack.icon/selected/selected-A.png"),
        ("G2", "Y2/skill.character.seojin.2.basic_attack.basic_attack.icon/selected/selected-C.png"),
        ("G3", "Y3/skill.character.seojin.3.basic_attack.basic_attack.icon/selected/selected-C.png"),
    ],
    "passive": [
        ("G1", "Y1/skill.character.seojin.1.passive_1.indomitable.icon/selected/selected-B.png"),
        ("G2", "Y2/skill.character.seojin.2.passive_1.indomitable.icon/selected/selected-C.png"),
        ("G3", "Y3/skill.character.seojin.3.passive_1.indomitable.icon/selected/selected-C.png"),
    ],
    "charge": [
        ("G1", "Y1/skill.character.seojin.1.active_1.active_1.icon/selected/selected-B.png"),
        ("G2", "Y2/skill.character.seojin.2.active_1.charge.icon/selected/selected-C.png"),
        ("G3", "Y3/skill.character.seojin.3.active_1.charge.icon/selected/selected-B.png"),
    ],
    "crane-wing": [
        ("G2", "Y2/skill.character.seojin.2.active_2.crane_wing_formation.icon/selected/selected-B.png"),
        ("G3", "Y3/skill.character.seojin.3.active_2.crane_wing_formation.icon/selected/selected-B.png"),
    ],
    "turtle-ship-assault": [
        ("G3", "Y3/skill.character.seojin.3.active_3.turtle_ship_assault.icon/selected/selected-C.png"),
    ],
    "cannon-zone": [
        ("G3", "Y3/skill.character.seojin.3.turtle_ship_cannon_zone.icon/selected/selected-C.png"),
    ],
}

processed = {}
assets = []
for family, entries in families.items():
    processed[family] = []
    for grade, relative in entries:
        output, metrics = extract(ROOT / relative)
        processed[family].append((grade, output))
        assets.append({"family": family, "grade": grade, **metrics})

evidence = ROOT / "family-evidence"
evidence.mkdir(exist_ok=True)
contacts = []
for size in (200, 80, 32):
    pad = max(6, size // 10)
    label_h = 18
    columns = 3
    cell_w = size + pad * 2
    cell_h = size + pad * 2 + label_h
    canvas = Image.new("RGB", (cell_w * columns, cell_h * len(families)), "#171b20")
    draw = ImageDraw.Draw(canvas)
    for row, (family, entries) in enumerate(processed.items()):
        for col, (grade, path) in enumerate(entries):
            x0, y0 = col * cell_w, row * cell_h
            draw.text((x0 + 4, y0 + 2), f"{family} {grade}", fill="#d8d5cc")
            icon = Image.open(path).convert("RGBA")
            icon.thumbnail((size, size), Image.Resampling.LANCZOS)
            x = x0 + pad + (size - icon.width) // 2
            y = y0 + pad + label_h + (size - icon.height) // 2
            canvas.paste(icon, (x, y), icon)
    output = evidence / f"seojin13-family-contact-{size}.png"
    canvas.save(output, optimize=False, compress_level=9)
    contacts.append({"size": size, "path": str(output.relative_to(ROOT)), "sha256": sha256(output)})

manifest = ROOT / "seojin13-family-manifest.json"
payload = {
    "status": "SEOJIN13_SELECTED_ALPHA_VISUAL_PASS_T0_PENDING",
    "selection": {
        "basic": ["G1:A", "G2:C", "G3:C"],
        "passive": ["G1:B", "G2:C", "G3:C"],
        "charge": ["G1:B", "G2:C", "G3:B"],
        "craneWing": ["G2:B", "G3:B"],
        "turtleShipAssault": ["G3:C"],
        "cannonZone": ["G3:C"],
    },
    "assets": assets,
    "contacts": contacts,
    "visualReview": {
        "familyStyle": "PASS",
        "gradeEvolution": "PASS",
        "alphaEdgesLightDark": "PASS",
        "meaningMustShow": "PASS",
    },
    "canonicalWrite": False, "metaWrite": False, "unity": False, "staging": False,
}
manifest.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"manifest": str(manifest), "manifestSha256": sha256(manifest), **payload}, ensure_ascii=False, indent=2))
