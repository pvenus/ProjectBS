from __future__ import annotations

import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parent
STRIPS = ROOT / "grade-strips"
OUT = ROOT / "exact18-rgba"
CONTACTS = ROOT / "contacts"
CELL_W = 362
TARGET = 256
INNER = 236
TIMES = [0.0, 0.08, 0.16, 0.24, 0.32, 0.40]


def sha(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def isolate(cell: Image.Image) -> tuple[Image.Image, list[int]]:
    rgba = np.asarray(cell.convert("RGBA"), dtype=np.uint8).copy()
    a = rgba[..., 3]
    yy, xx = np.nonzero(a)
    if len(xx) == 0:
        raise RuntimeError("empty logical cell")
    bbox = [int(xx.min()), int(yy.min()), int(xx.max() + 1), int(yy.max() + 1)]
    crop = rgba[bbox[1]:bbox[3], bbox[0]:bbox[2]].copy()
    crop[crop[..., 3] == 0, :3] = 0
    island = Image.fromarray(crop, "RGBA")
    scale = min(1.0, INNER / island.width, INNER / island.height)
    if scale < 1.0:
        size = (max(1, round(island.width * scale)), max(1, round(island.height * scale)))
        island = island.resize(size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (TARGET, TARGET), (0, 0, 0, 0))
    xy = ((TARGET - island.width) // 2, (TARGET - island.height) // 2)
    canvas.alpha_composite(island, xy)
    return canvas, bbox


def composite(frame: Image.Image, bg: tuple[int, int, int], gray: bool) -> Image.Image:
    base = Image.new("RGBA", frame.size, (*bg, 255))
    base.alpha_composite(frame)
    out = base.convert("RGB")
    return out.convert("L").convert("RGB") if gray else out


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    CONTACTS.mkdir(parents=True, exist_ok=True)
    frames: list[Image.Image] = []
    rows = []
    strip_sources = []

    for grade in range(1, 4):
        strip_path = STRIPS / f"seojin-basic-G{grade}-strip.png"
        strip = Image.open(strip_path).convert("RGBA")
        strip_sources.append({"grade": f"G{grade}", "path": str(strip_path), "sha256": sha(strip_path), "dimensions": list(strip.size)})
        for frame in range(6):
            rect = [frame * CELL_W, 0, (frame + 1) * CELL_W, strip.height]
            logical = strip.crop(rect)
            output, alpha_bbox = isolate(logical)
            path = OUT / f"seojin-basic-g{grade}-f{frame}.png"
            output.save(path, format="PNG", compress_level=9)
            arr = np.asarray(output, dtype=np.uint8)
            a = arr[..., 3]
            rows.append({
                "grade": f"G{grade}", "frame": f"F{frame}", "timestampSeconds": TIMES[frame],
                "stripLogicalRect": rect, "stripAlphaBBox": alpha_bbox,
                "path": str(path), "sha256": sha(path), "dimensions": [256, 256], "pivot": [0.5, 0.5],
                "alphaNonzero": int(np.count_nonzero(a)), "alphaPartial": int(np.count_nonzero((a > 0) & (a < 255))),
                "alphaOpaque": int(np.count_nonzero(a == 255)), "alphaSum": int(a.sum()),
                "corners": [int(a[0, 0]), int(a[0, -1]), int(a[-1, 0]), int(a[-1, -1])],
                "borderNonzero": [int(np.count_nonzero(a[0])), int(np.count_nonzero(a[:, -1])), int(np.count_nonzero(a[-1])), int(np.count_nonzero(a[:, 0]))],
                "alpha0RgbResidue": int(np.count_nonzero(arr[a == 0, :3]))
            })
            frames.append(output)

    contact_rows = []
    for px in (200, 80, 32):
        for bg_name, bg in (("light", (232, 228, 218)), ("dark", (24, 28, 34))):
            for mode in ("color", "gray"):
                sheet = Image.new("RGB", (6 * px, 3 * px), bg)
                for i, frame in enumerate(frames):
                    item = composite(frame, bg, mode == "gray").resize((px, px), Image.Resampling.LANCZOS)
                    sheet.paste(item, ((i % 6) * px, (i // 6) * px))
                path = CONTACTS / f"seojin-basic-fresh-exact18-{px}px-{bg_name}-{mode}.png"
                sheet.save(path, format="PNG", compress_level=9)
                contact_rows.append({"path": str(path), "sha256": sha(path), "dimensions": list(sheet.size)})

    manifest = {
        "schema": "projectbs.skill-animation-native-rgba-exact18.v1",
        "status": "FRESH_SOURCE_REVIEW_READY_NOT_INSTALLED",
        "contentId": "skill.character.seojin.basic_attack",
        "freshBatch": {"priorCandidatePixelReuse": False, "priorFrameSplice": False, "frameDonor": False},
        "stripSources": strip_sources,
        "mapping": {"rows": ["G1", "G2", "G3"], "columns": ["F0", "F1", "F2", "F3", "F4", "F5"], "frameCount": 18},
        "timing": {"timestampsSeconds": TIMES, "stopTimeSeconds": 0.48, "composedTerminalAlpha": 0},
        "normalization": "crop each 362x724 logical cell to its native alpha bbox; alpha0 RGB zero; never upscale; downscale only if bbox exceeds236; center in256x256",
        "frames": rows,
        "contacts": contact_rows,
        "mutationBoundary": {"canonical": False, "clip": False, "metaGuid": False, "shader": False, "unity": False}
    }
    manifest_path = OUT / "seojin-basic-fresh-exact18-manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    print(manifest_path)
    print(sha(manifest_path))


if __name__ == "__main__":
    main()
