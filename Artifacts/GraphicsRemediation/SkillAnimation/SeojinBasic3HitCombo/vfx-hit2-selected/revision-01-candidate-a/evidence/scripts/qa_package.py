#!/usr/bin/env python3
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parents[2]
FRAMES = ROOT / "frames"
EVIDENCE = ROOT / "evidence"
CONTACTS = EVIDENCE / "contacts"
CONTACTS.mkdir(parents=True, exist_ok=True)

TIMES = [0.0, 0.05, 0.10, 0.15, 0.20, 0.28]
STOP = 0.34
SCALES = [200, 80, 32]
BACKGROUNDS = {
    "light": (235, 237, 240, 255),
    "dark": (18, 22, 30, 255),
}


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for block in iter(lambda: f.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def alpha_bbox_inclusive(im: Image.Image):
    bbox = im.getchannel("A").getbbox()
    if bbox is None:
        return None
    x0, y0, x1, y1 = bbox
    return [x0, y0, x1 - 1, y1 - 1]


def border_alpha_nonzero(im: Image.Image) -> int:
    a = im.getchannel("A")
    w, h = im.size
    values = list(a.crop((0, 0, w, 1)).getdata())
    values += list(a.crop((0, h - 1, w, h)).getdata())
    values += list(a.crop((0, 1, 1, h - 1)).getdata())
    values += list(a.crop((w - 1, 1, w, h - 1)).getdata())
    return sum(v != 0 for v in values)


def alpha0_rgb_nonzero(im: Image.Image) -> int:
    return sum(1 for r, g, b, a in im.getdata() if a == 0 and (r or g or b))


def composite(im: Image.Image, size: int, bg):
    fg = im.resize((size, size), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (size, size), bg)
    canvas.alpha_composite(fg)
    return canvas.convert("RGB")


frame_paths = [FRAMES / f"frame-{i:02d}.png" for i in range(6)]
frames = [Image.open(p).convert("RGBA") for p in frame_paths]
frame_records = []
for i, (path, im) in enumerate(zip(frame_paths, frames)):
    bbox = alpha_bbox_inclusive(im)
    if bbox:
        x0, y0, x1, y1 = bbox
        clearance = [x0, y0, 255 - x1, 255 - y1]
    else:
        clearance = None
    frame_records.append({
        "frame": i,
        "path": str(path),
        "sha256": sha256(path),
        "mode": Image.open(path).mode,
        "dimensions": list(im.size),
        "alphaBBoxInclusive": bbox,
        "clearanceLTRB": clearance,
        "borderAlphaNonzero": border_alpha_nonzero(im),
        "alpha0RgbNonzero": alpha0_rgb_nonzero(im),
        "alphaNonzeroPixels": sum(v != 0 for v in im.getchannel("A").getdata()),
    })

contact_records = []
for size in SCALES:
    for bg_name, bg in BACKGROUNDS.items():
        color_cells = [composite(im, size, bg) for im in frames]
        color = Image.new("RGB", (size * 6, size), bg[:3])
        for i, cell in enumerate(color_cells):
            color.paste(cell, (i * size, 0))
        color_path = CONTACTS / f"contact-{size}-{bg_name}-color.png"
        color.save(color_path, optimize=False)
        gray = ImageOps.grayscale(color)
        gray_path = CONTACTS / f"contact-{size}-{bg_name}-gray.png"
        gray.save(gray_path, optimize=False)
        contact_records.extend([
            {"path": str(color_path), "sha256": sha256(color_path), "dimensions": list(color.size), "mode": color.mode},
            {"path": str(gray_path), "sha256": sha256(gray_path), "dimensions": list(gray.size), "mode": gray.mode},
        ])

gif_frames = [composite(im, 256, BACKGROUNDS["dark"]) for im in frames]
durations = [int(round((TIMES[i + 1] - TIMES[i]) * 1000)) for i in range(5)] + [int(round((STOP - TIMES[-1]) * 1000))]
gif_path = EVIDENCE / "candidate-a-non-loop-dark.gif"
gif_frames[0].save(
    gif_path,
    save_all=True,
    append_images=gif_frames[1:],
    duration=durations,
    disposal=2,
    optimize=False,
)

with Image.open(gif_path) as gif:
    gif_meta = {
        "path": str(gif_path),
        "sha256": sha256(gif_path),
        "dimensions": list(gif.size),
        "physicalFrames": getattr(gif, "n_frames", 1),
        "durationsMs": [],
        "loopMetadata": gif.info.get("loop"),
    }
    for i in range(gif.n_frames):
        gif.seek(i)
        gif_meta["durationsMs"].append(gif.info.get("duration"))

unique = len({r["sha256"] for r in frame_records})
qa = {
    "schema": "seojin_basic_combo2_vfx_selected_candidate_qa_v1",
    "status": "PHYSICAL_QA_PASS_VISUAL_SELECTION_OWNER_UNCHANGED",
    "scope": "read-only source QA plus review derivatives",
    "sourceManifest": str(ROOT / "manifest.json"),
    "sourceManifestSha256": sha256(ROOT / "manifest.json"),
    "timing": {"times": TIMES, "stop": STOP, "loop": False, "gifDurationsMs": durations},
    "checks": {
        "frameCount": len(frames),
        "uniqueFrames": unique,
        "rgba": sum(r["mode"] == "RGBA" for r in frame_records),
        "size256": sum(r["dimensions"] == [256, 256] for r in frame_records),
        "borderAlpha0": sum(r["borderAlphaNonzero"] == 0 for r in frame_records),
        "alpha0Rgb0": sum(r["alpha0RgbNonzero"] == 0 for r in frame_records),
        "safeMargin12": sum(r["clearanceLTRB"] is not None and min(r["clearanceLTRB"]) >= 12 for r in frame_records),
        "clippingByAlphaBorder": sum(r["borderAlphaNonzero"] != 0 for r in frame_records),
        "orderedNames": [p.name for p in frame_paths],
    },
    "frames": frame_records,
    "contacts": contact_records,
    "gif": gif_meta,
    "limitations": [
        "No source pixels were modified.",
        "This QA does not make visual-direction, runtime, release, or install decisions.",
        "Contact sheets and GIF are review derivatives only.",
    ],
}
manifest_path = EVIDENCE / "qa-manifest.json"
manifest_path.write_text(json.dumps(qa, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

receipt = EVIDENCE / "qa-receipt.txt"
receipt.write_text(
    "Seojin Basic combo hit2 selected Candidate A — frame QA receipt\n"
    f"source manifest SHA-256: {qa['sourceManifestSha256']}\n"
    f"QA manifest: {manifest_path}\n"
    f"QA manifest SHA-256: {sha256(manifest_path)}\n"
    f"frames: {len(frames)}/6; unique: {unique}/6; RGBA: {qa['checks']['rgba']}/6; 256x256: {qa['checks']['size256']}/6\n"
    f"border alpha0: {qa['checks']['borderAlpha0']}/6; alpha0 RGB0: {qa['checks']['alpha0Rgb0']}/6; safe margin >=12: {qa['checks']['safeMargin12']}/6\n"
    f"clipped at canvas border: {qa['checks']['clippingByAlphaBorder']}/6\n"
    f"timing: {TIMES}; stop={STOP}; loop=false; GIF delays={durations}ms; physical frames={gif_meta['physicalFrames']}; loop metadata={gif_meta['loopMetadata']}\n"
    "contacts: 200/80/32 light+dark color+gray complete\n"
    "scope: physical QA PASS; visual selection/runtime/release/install not inferred\n",
    encoding="utf-8",
)

print(json.dumps({
    "qaManifest": str(manifest_path),
    "qaManifestSha256": sha256(manifest_path),
    "receipt": str(receipt),
    "receiptSha256": sha256(receipt),
    "gif": gif_meta,
    "checks": qa["checks"],
}, indent=2))
