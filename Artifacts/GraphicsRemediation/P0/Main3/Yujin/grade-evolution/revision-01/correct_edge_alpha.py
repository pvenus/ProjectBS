from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image


ROOT = Path(__file__).parent
TARGETS = (
    "skill.character.yujin.2.active_2.hwalbin_barrage.icon",
    "skill.character.yujin.2.passive_1.passive_1.icon",
    "skill.character.yujin.3.active_2.hwalbin_barrage.icon",
    "skill.character.yujin.3.passive_1.passive_1.icon",
)


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def correct(asset_dir: Path) -> dict:
    asset_id = asset_dir.name
    source = asset_dir / "alpha" / f"{asset_id}.selected-alpha.png"
    rgba = np.array(Image.open(source).convert("RGBA"))
    before = rgba[:, :, 3].copy()

    # Evidence-based correction1: remove only the low-alpha matte veil and the
    # border band it reaches. RGB design pixels, geometry, scale and centering
    # remain unchanged. Partial brush alpha >= 64 is preserved.
    alpha = rgba[:, :, 3]
    alpha[alpha < 64] = 0
    alpha[:48, :] = 0
    alpha[-48:, :] = 0
    alpha[:, :48] = 0
    alpha[:, -48:] = 0
    rgba[:, :, 3] = alpha
    rgba[alpha == 0, :3] = 0

    out_dir = asset_dir / "alpha-correction-01"
    out_dir.mkdir(exist_ok=True)
    output = out_dir / f"{asset_id}.selected-alpha-r1.png"
    mask = out_dir / f"{asset_id}.mask-r1.png"
    Image.fromarray(rgba, "RGBA").save(output, optimize=False, compress_level=9)
    Image.fromarray(alpha, "L").save(mask, optimize=False, compress_level=9)

    ys, xs = np.where(alpha >= 16)
    changed = before != alpha
    metrics = {
        "assetId": asset_id,
        "source": str(source.relative_to(ROOT)),
        "sourceSha256": sha256(source),
        "output": str(output.relative_to(ROOT)),
        "outputSha256": sha256(output),
        "mask": str(mask.relative_to(ROOT)),
        "maskSha256": sha256(mask),
        "rule": "alpha<64->0; outer48px alpha->0; alpha0 RGB->0",
        "rgbDesignChangedWhereVisible": int(
            np.count_nonzero(
                rgba[alpha > 0, :3]
                != np.array(Image.open(source).convert("RGBA"))[alpha > 0, :3]
            )
        ),
        "alphaPixelsChanged": int(changed.sum()),
        "alphaPartial": int(((alpha > 0) & (alpha < 255)).sum()),
        "alphaOpaque": int((alpha == 255).sum()),
        "alphaZeroRgbResidue": int(np.count_nonzero(rgba[alpha == 0, :3])),
        "cornersAlpha": [
            int(alpha[0, 0]),
            int(alpha[0, -1]),
            int(alpha[-1, 0]),
            int(alpha[-1, -1]),
        ],
        "bboxAlpha16": (
            [int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())]
            if len(xs)
            else None
        ),
    }
    metrics_path = out_dir / f"{asset_id}.metrics-r1.json"
    metrics_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n")
    metrics["metrics"] = str(metrics_path.relative_to(ROOT))
    metrics["metricsSha256"] = sha256(metrics_path)
    return metrics


assets = {p.name: p for p in ROOT.glob("Y[23]/*") if p.is_dir()}
results = [correct(assets[asset_id]) for asset_id in TARGETS]

# The stronger Y2 basic revision uses the same one-time edge cleanup after its
# visual reselection. Keep it in its own revision directory.
basic_asset = ROOT / "Y2/skill.character.yujin.2.basic_attack.basic_attack.icon"
basic_source = basic_asset / "alpha-revision-02/skill.character.yujin.2.basic_attack.basic_attack.icon.selected-alpha-r2.png"
if basic_source.exists():
    rgba = np.array(Image.open(basic_source).convert("RGBA"))
    before = rgba[:, :, 3].copy()
    alpha = rgba[:, :, 3]
    alpha[alpha < 64] = 0
    alpha[:48, :] = 0
    alpha[-48:, :] = 0
    alpha[:, :48] = 0
    alpha[:, -48:] = 0
    rgba[:, :, 3] = alpha
    rgba[alpha == 0, :3] = 0
    out_dir = basic_asset / "alpha-revision-02-correction-01"
    out_dir.mkdir(exist_ok=True)
    output = out_dir / f"{basic_asset.name}.selected-alpha-r2c1.png"
    mask = out_dir / f"{basic_asset.name}.mask-r2c1.png"
    Image.fromarray(rgba, "RGBA").save(output, optimize=False, compress_level=9)
    Image.fromarray(alpha, "L").save(mask, optimize=False, compress_level=9)
    ys, xs = np.where(alpha >= 16)
    basic_metrics = {
        "assetId": basic_asset.name,
        "source": str(basic_source.relative_to(ROOT)),
        "sourceSha256": sha256(basic_source),
        "output": str(output.relative_to(ROOT)),
        "outputSha256": sha256(output),
        "mask": str(mask.relative_to(ROOT)),
        "maskSha256": sha256(mask),
        "rule": "alpha<64->0; outer48px alpha->0; alpha0 RGB->0",
        "rgbDesignChangedWhereVisible": 0,
        "alphaPixelsChanged": int((before != alpha).sum()),
        "alphaPartial": int(((alpha > 0) & (alpha < 255)).sum()),
        "alphaOpaque": int((alpha == 255).sum()),
        "alphaZeroRgbResidue": int(np.count_nonzero(rgba[alpha == 0, :3])),
        "cornersAlpha": [int(alpha[0, 0]), int(alpha[0, -1]), int(alpha[-1, 0]), int(alpha[-1, -1])],
        "bboxAlpha16": [int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())],
    }
    metrics_path = out_dir / f"{basic_asset.name}.metrics-r2c1.json"
    metrics_path.write_text(json.dumps(basic_metrics, ensure_ascii=False, indent=2) + "\n")
    basic_metrics["metrics"] = str(metrics_path.relative_to(ROOT))
    basic_metrics["metricsSha256"] = sha256(metrics_path)
    results.append(basic_metrics)
manifest = ROOT / "alpha-correction-01-manifest.json"
manifest.write_text(json.dumps(results, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(results, ensure_ascii=False, indent=2))
