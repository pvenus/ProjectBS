from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image


ROOT = Path(__file__).parent
PROJECT = ROOT.parents[4]
SOURCES = {
    "skill.character.sangui_abandoned_child.3.active_2.why_only_me.icon": PROJECT / "Assets/ImagesGenerated/Skill/icon/skill.character.sangui_abandoned_child.3.active_2.why_only_me.icon.png",
    "skill.strategic.taeul_healing_formation.icon": PROJECT / "Assets/ImagesGenerated/Skill/icon/skill.strategic.taeul_healing_formation.icon.png",
}


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


results = []
for asset_id, source in SOURCES.items():
    source_rgba = np.array(Image.open(source).convert("RGBA"))
    output_rgba = source_rgba.copy()
    before_alpha = output_rgba[:, :, 3].copy()

    # Mask-only correction: only the four canvas boundary rows/columns are
    # cleared. Interior RGB, composition, scale and all non-boundary alpha stay
    # byte-identical. Fully transparent RGB is normalized to zero.
    output_rgba[0, :, 3] = 0
    output_rgba[-1, :, 3] = 0
    output_rgba[:, 0, 3] = 0
    output_rgba[:, -1, 3] = 0
    output_rgba[output_rgba[:, :, 3] == 0, :3] = 0

    asset_root = ROOT / asset_id
    asset_root.mkdir(parents=True, exist_ok=True)
    output = asset_root / f"{asset_id}.edge-alpha-r1.png"
    mask = asset_root / f"{asset_id}.edge-alpha-mask-r1.png"
    Image.fromarray(output_rgba, "RGBA").save(output, optimize=False, compress_level=9)
    Image.fromarray(output_rgba[:, :, 3], "L").save(mask, optimize=False, compress_level=9)

    alpha = output_rgba[:, :, 3]
    ys, xs = np.where(alpha >= 16)
    visible = alpha > 0
    rgb_mismatch_visible = int(np.count_nonzero(output_rgba[visible, :3] != source_rgba[visible, :3]))
    record = {
        "assetId": asset_id,
        "source": str(source.relative_to(PROJECT)),
        "sourceSha256": sha256(source),
        "output": str(output.relative_to(PROJECT)),
        "outputSha256": sha256(output),
        "mask": str(mask.relative_to(PROJECT)),
        "maskSha256": sha256(mask),
        "operation": "outermost 1px alpha->0; alpha0 RGB->0",
        "alphaPixelsChanged": int((before_alpha != alpha).sum()),
        "visibleRgbMismatch": rgb_mismatch_visible,
        "alphaZeroRgbResidue": int(np.count_nonzero(output_rgba[alpha == 0, :3])),
        "cornersAlpha": [int(alpha[0, 0]), int(alpha[0, -1]), int(alpha[-1, 0]), int(alpha[-1, -1])],
        "edgeAlphaNonzero": int((alpha[0] > 0).sum() + (alpha[-1] > 0).sum() + (alpha[:, 0] > 0).sum() + (alpha[:, -1] > 0).sum()),
        "alphaPartial": int(((alpha > 0) & (alpha < 255)).sum()),
        "bboxAlpha16": [int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())],
        "canonicalWrite": False,
        "unity": False,
        "staging": False,
    }
    metrics = asset_root / "metrics.json"
    metrics.write_text(json.dumps(record, ensure_ascii=False, indent=2) + "\n")
    record["metrics"] = str(metrics.relative_to(PROJECT))
    record["metricsSha256"] = sha256(metrics)
    results.append(record)

manifest = ROOT / "edge-alpha-02-handoff-manifest.json"
manifest.write_text(json.dumps({"status": "VISUAL_EDGE_ALPHA_PASS_T0_PENDING_NOT_PROMOTABLE", "assets": results}, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"manifest": str(manifest.relative_to(PROJECT)), "manifestSha256": sha256(manifest), "assets": results}, ensure_ascii=False, indent=2))
