from __future__ import annotations

import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parent
SOURCE_ROOT = ROOT / "exact18-rgba"
SOURCE_MANIFEST = SOURCE_ROOT / "seojin-basic-fresh-exact18-manifest.json"
OUT = ROOT / "exact18-rgba-dark-readable-r1"
CONTACTS = ROOT / "contacts-dark-readable-r1"
TARGET_NAVY = np.array([48.0, 80.0, 126.0], dtype=np.float32)


def sha(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def correct(frame: Image.Image) -> tuple[Image.Image, dict]:
    arr = np.asarray(frame.convert("RGBA"), dtype=np.uint8).copy()
    before = arr.copy()
    a = arr[..., 3].astype(np.float32)
    nonzero = a > 0
    existing = a[nonzero]
    core_threshold = max(20.0, float(np.percentile(existing, 72.0)))
    core = nonzero & (a >= core_threshold)
    shoulder = nonzero & ~core

    # Geometry invariant: only redistribute coverage where alpha already exists.
    new_a = a.copy()
    new_a[core] = np.maximum(205.0, np.clip(a[core] + (255.0 - a[core]) * 0.72, 0, 255))
    new_a[shoulder] = np.clip(a[shoulder] * 1.24, 0, 128)

    rgb = arr[..., :3].astype(np.float32)
    # Firm mid-navy core, never near-white; retain local source variation.
    rgb[core] = rgb[core] * 0.45 + TARGET_NAVY * 0.55
    rgb[shoulder] = rgb[shoulder] * 0.80 + TARGET_NAVY * 0.20
    rgb = np.clip(rgb, 0, 180)
    rgb[~nonzero] = 0
    arr[..., :3] = np.rint(rgb).astype(np.uint8)
    arr[..., 3] = np.rint(new_a).astype(np.uint8)

    after_nonzero = arr[..., 3] > 0
    metrics = {
        "nonzeroMaskIdentity": bool(np.array_equal(nonzero, after_nonzero)),
        "changedPixelCount": int(np.count_nonzero(np.any(arr != before, axis=2))),
        "alphaOpaqueBefore": int(np.count_nonzero(before[..., 3] == 255)),
        "alphaOpaqueAfter": int(np.count_nonzero(arr[..., 3] == 255)),
        "alphaSumBefore": int(before[..., 3].sum()),
        "alphaSumAfter": int(arr[..., 3].sum()),
        "maxRgbAfter": int(arr[..., :3].max())
        ,"coreThreshold": core_threshold
    }
    return Image.fromarray(arr, "RGBA"), metrics


def composite(frame: Image.Image, bg: tuple[int, int, int], gray: bool) -> Image.Image:
    base = Image.new("RGBA", frame.size, (*bg, 255))
    base.alpha_composite(frame)
    out = base.convert("RGB")
    return out.convert("L").convert("RGB") if gray else out


def main() -> None:
    source = json.loads(SOURCE_MANIFEST.read_text())
    OUT.mkdir(parents=True, exist_ok=True)
    CONTACTS.mkdir(parents=True, exist_ok=True)
    frames = []
    rows = []
    for entry in source["frames"]:
        src = Image.open(entry["path"]).convert("RGBA")
        target = entry["grade"] in {"G1", "G2"} and entry["frame"] in {"F0", "F1", "F2"}
        if target:
            out, correction = correct(src)
        else:
            out = src.copy()
            correction = {"bytePreserved": True}
        path = OUT / Path(entry["path"]).name
        out.save(path, format="PNG", compress_level=9)
        arr = np.asarray(out, dtype=np.uint8)
        a = arr[..., 3]
        rows.append({
            **{k: entry[k] for k in ("grade", "frame", "timestampSeconds")},
            "path": str(path), "sha256": sha(path), "dimensions": [256, 256], "pivot": [0.5, 0.5],
            "sourceSha256": entry["sha256"], "correctionApplied": target, "correctionMetrics": correction,
            "alphaNonzero": int(np.count_nonzero(a)), "alphaPartial": int(np.count_nonzero((a > 0) & (a < 255))),
            "alphaOpaque": int(np.count_nonzero(a == 255)), "alphaSum": int(a.sum()),
            "corners": [int(a[0, 0]), int(a[0, -1]), int(a[-1, 0]), int(a[-1, -1])],
            "borderNonzero": [int(np.count_nonzero(a[0])), int(np.count_nonzero(a[:, -1])), int(np.count_nonzero(a[-1])), int(np.count_nonzero(a[:, 0]))],
            "alpha0RgbResidue": int(np.count_nonzero(arr[a == 0, :3]))
        })
        frames.append(out)

    contact_rows = []
    for px in (200, 80, 32):
        for bg_name, bg in (("light", (232, 228, 218)), ("dark", (24, 28, 34))):
            for mode in ("color", "gray"):
                sheet = Image.new("RGB", (6 * px, 3 * px), bg)
                for i, frame in enumerate(frames):
                    item = composite(frame, bg, mode == "gray").resize((px, px), Image.Resampling.LANCZOS)
                    sheet.paste(item, ((i % 6) * px, (i // 6) * px))
                path = CONTACTS / f"seojin-basic-fresh-r1-{px}px-{bg_name}-{mode}.png"
                sheet.save(path, format="PNG", compress_level=9)
                contact_rows.append({"path": str(path), "sha256": sha(path), "dimensions": list(sheet.size)})

    manifest = {
        "schema": "projectbs.skill-animation-native-rgba-exact18.correction-r1",
        "status": "DARK_READABILITY_REVIEW_READY_NOT_INSTALLED",
        "sourceManifestSha256": sha(SOURCE_MANIFEST),
        "correctionScope": ["G1F0", "G1F1", "G1F2", "G2F0", "G2F1", "G2F2"],
        "method": "existing-alpha-only coverage redistribution and bounded mid-navy core convergence; nonzero mask invariant",
        "forbiddenOperations": {"newGeometry": False, "glow": False, "outline": False, "blur": False, "scale": False, "nearWhite": False},
        "frames": rows, "contacts": contact_rows,
        "timing": source["timing"], "mapping": source["mapping"],
        "mutationBoundary": {"canonical": False, "clip": False, "metaGuid": False, "shader": False, "unity": False}
    }
    path = OUT / "seojin-basic-fresh-exact18-dark-readable-r1-manifest.json"
    path.write_text(json.dumps(manifest, indent=2) + "\n")
    print(path)
    print(sha(path))


if __name__ == "__main__":
    main()
