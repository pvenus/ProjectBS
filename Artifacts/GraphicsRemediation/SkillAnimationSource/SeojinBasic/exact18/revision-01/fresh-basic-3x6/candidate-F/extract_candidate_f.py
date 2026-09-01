from __future__ import annotations

import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "seojin-basic-g1-g2-g3-3x6-F.png"
SOURCE_MANIFEST = ROOT / "seojin-basic-candidate-F-source-manifest.json"
OUT = ROOT / "selected-exact18-rgba"
CONTACT = ROOT / "contacts"


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def border_background(rgb: np.ndarray) -> np.ndarray:
    border = np.concatenate(
        [rgb[:12].reshape(-1, 3), rgb[-12:].reshape(-1, 3),
         rgb[:, :12].reshape(-1, 3), rgb[:, -12:].reshape(-1, 3)], axis=0
    )
    return np.median(border, axis=0).astype(np.float32)


def matte_extract(cell: Image.Image) -> Image.Image:
    rgb = np.asarray(cell.convert("RGB"), dtype=np.float32)
    bg = border_background(rgb)
    dist = np.sqrt(np.sum((rgb - bg) ** 2, axis=2))

    # Paper variation remains below this floor; dry-brush coverage ramps smoothly.
    alpha = np.clip((dist - 14.0) / 48.0, 0.0, 1.0)
    alpha[alpha < 0.055] = 0.0

    # Recover foreground RGB from the matte composite without repainting geometry.
    safe = np.maximum(alpha[..., None], 0.08)
    fg = (rgb - (1.0 - alpha[..., None]) * bg[None, None, :]) / safe
    fg = np.clip(fg, 0.0, 255.0)
    fg[alpha == 0.0] = 0.0
    rgba = np.dstack([fg.astype(np.uint8), np.rint(alpha * 255).astype(np.uint8)])
    extracted = Image.fromarray(rgba, "RGBA")

    # Geometry-preserving uniform fit into an equal 256x256 canvas with a
    # six-pixel transparent safety margin. This prevents review-grid/dry-brush
    # fringe from touching runtime borders without changing internal geometry.
    scale = min(244.0 / extracted.width, 244.0 / extracted.height)
    size = (max(1, round(extracted.width * scale)), max(1, round(extracted.height * scale)))
    fitted = extracted.resize(size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    xy = ((256 - size[0]) // 2, (256 - size[1]) // 2)
    canvas.alpha_composite(fitted, xy)
    return canvas


def composite(frame: Image.Image, bg: tuple[int, int, int]) -> Image.Image:
    base = Image.new("RGBA", frame.size, (*bg, 255))
    base.alpha_composite(frame)
    return base.convert("RGB")


def grayscale(image: Image.Image) -> Image.Image:
    return image.convert("L").convert("RGB")


def contact_sheet(frames: list[Image.Image], cell_px: int, bg: tuple[int, int, int], gray: bool) -> Image.Image:
    sheet = Image.new("RGB", (6 * cell_px, 3 * cell_px), bg)
    for i, frame in enumerate(frames):
        rendered = composite(frame, bg).resize((cell_px, cell_px), Image.Resampling.LANCZOS)
        if gray:
            rendered = grayscale(rendered)
        sheet.paste(rendered, ((i % 6) * cell_px, (i // 6) * cell_px))
    return sheet


def main() -> None:
    manifest = json.loads(SOURCE_MANIFEST.read_text())
    source = Image.open(SOURCE).convert("RGB")
    OUT.mkdir(parents=True, exist_ok=True)
    CONTACT.mkdir(parents=True, exist_ok=True)

    outputs = []
    frames = []
    for idx, entry in enumerate(manifest["grid"]["cells"]):
        cell = source.crop(tuple(entry["rect"]))
        rgba = matte_extract(cell)
        name = f"seojin-basic-{entry['grade'].lower()}-{entry['frame'].lower()}.png"
        path = OUT / name
        rgba.save(path, format="PNG", compress_level=9)
        a = np.asarray(rgba.getchannel("A"), dtype=np.uint8)
        rgb = np.asarray(rgba.convert("RGBA"), dtype=np.uint8)[..., :3]
        outputs.append({
            "index": idx,
            "grade": entry["grade"],
            "frame": entry["frame"],
            "timestampSeconds": [0.0, 0.08, 0.16, 0.24, 0.32, 0.40][idx % 6],
            "path": str(path),
            "sha256": sha256(path),
            "dimensions": [256, 256],
            "pivot": [0.5, 0.5],
            "alphaNonzero": int(np.count_nonzero(a)),
            "alphaPartial": int(np.count_nonzero((a > 0) & (a < 255))),
            "alphaOpaque": int(np.count_nonzero(a == 255)),
            "alphaSum": int(a.sum()),
            "corners": [int(a[0, 0]), int(a[0, -1]), int(a[-1, 0]), int(a[-1, -1])],
            "borderNonzero": {
                "top": int(np.count_nonzero(a[0])),
                "right": int(np.count_nonzero(a[:, -1])),
                "bottom": int(np.count_nonzero(a[-1])),
                "left": int(np.count_nonzero(a[:, 0]))
            },
            "alpha0RgbResidue": int(np.count_nonzero(rgb[a == 0]))
        })
        frames.append(rgba)

    contact_outputs = []
    for px in (200, 80, 32):
        for bg_name, bg in (("light", (232, 228, 218)), ("dark", (24, 28, 34))):
            for mode in ("color", "gray"):
                sheet = contact_sheet(frames, px, bg, mode == "gray")
                path = CONTACT / f"seojin-basic-candidate-F-{px}px-{bg_name}-{mode}.png"
                sheet.save(path, format="PNG", compress_level=9)
                contact_outputs.append({"path": str(path), "sha256": sha256(path), "dimensions": list(sheet.size)})

    result = {
        "schema": "projectbs.skill-animation-extracted-frames.v1",
        "status": "VISUAL_ALPHA_REVIEW_READY_NOT_CANONICAL",
        "sourcePath": str(SOURCE),
        "sourceSha256": sha256(SOURCE),
        "sourceManifestSha256": sha256(SOURCE_MANIFEST),
        "method": {
            "backgroundModel": "per-cell 12px border RGB median",
            "alpha": "euclidean RGB distance ramp floor14 span48; alpha<0.055=>0",
            "foreground": "inverse matte composite recovery; no repaint",
            "normalization": "uniform fit within 244x244, centered on 256x256 (6px minimum transparent margin), Lanczos",
            "mirrorOrSemanticRedraw": False
        },
        "timing": {
            "timestampsSeconds": [0.0, 0.08, 0.16, 0.24, 0.32, 0.40],
            "stopTimeSeconds": 0.48,
            "composedTerminalAlphaAtStopTime": 0,
            "sourceFrameAtStopTime": None
        },
        "frames": outputs,
        "contacts": contact_outputs,
        "mutationBoundary": {"canonical": False, "clip": False, "metaGuid": False, "unity": False}
    }
    out_manifest = OUT / "seojin-basic-candidate-F-exact18-rgba-manifest.json"
    out_manifest.write_text(json.dumps(result, indent=2) + "\n")
    print(out_manifest)
    print(sha256(out_manifest))


if __name__ == "__main__":
    main()
