#!/usr/bin/env python3
import hashlib
import json
import shutil
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
OUT = ROOT / "selected-border-safe-v002"
NPCS = {
    "black_cloth_raider": (627, 627),
    "chain_dragger_raider": (768, 512),
}
PASS_ACTIONS = ("Idle", "BasicAttack", "VFX")
FIX_ACTIONS = ("Move", "Death", "Stun")
MARGIN = 12

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def resize_premultiplied(image, size):
    rgba = np.asarray(image, dtype=np.float32) / 255.0
    alpha = rgba[..., 3:4]
    rgb = rgba[..., :3] * alpha
    packed = np.concatenate((rgb, alpha), axis=2)
    channels = []
    for channel in range(4):
        plane = Image.fromarray(np.round(packed[..., channel] * 65535).astype(np.uint16), mode="I;16")
        plane = plane.resize(size, Image.Resampling.LANCZOS)
        channels.append(np.asarray(plane, dtype=np.float32) / 65535.0)
    premul = np.stack(channels, axis=2)
    out_alpha = np.clip(premul[..., 3:4], 0, 1)
    out_rgb = np.divide(premul[..., :3], out_alpha, out=np.zeros_like(premul[..., :3]), where=out_alpha > 0)
    out = np.concatenate((np.clip(out_rgb, 0, 1), out_alpha), axis=2)
    out = np.round(out * 255).astype(np.uint8)
    out[out[..., 3] == 0, :3] = 0
    return Image.fromarray(out, "RGBA")

manifest = {
    "method": "action-unit uniform aspect-preserving scale+translation",
    "margin": MARGIN,
    "pivot": [0.5, 0.5],
    "ppu": 100,
    "rendererScaleMultiplier": 0.72,
    "sourceDownsample": False,
    "actions": {},
}

for npc, canvas in NPCS.items():
    width, height = canvas
    manifest["actions"][npc] = {}
    for action in PASS_ACTIONS:
        dest = OUT / npc / action
        dest.mkdir(parents=True, exist_ok=True)
        entries = []
        for index in range(4):
            source = ROOT / npc / action / f"frame-{index}.png"
            target = dest / source.name
            shutil.copy2(source, target)
            entries.append({"frame": index, "sourceSha256": sha(source), "outputSha256": sha(target), "bytePreserved": True})
        manifest["actions"][npc][action] = {"status": "byte-preserved", "frames": entries}

    for action in FIX_ACTIONS:
        sources = [ROOT / npc / action / f"frame-{index}.png" for index in range(4)]
        images = [Image.open(path).convert("RGBA") for path in sources]
        bboxes = [image.getchannel("A").getbbox() for image in images]
        union = (min(b[0] for b in bboxes), min(b[1] for b in bboxes), max(b[2] for b in bboxes), max(b[3] for b in bboxes))
        union_w, union_h = union[2] - union[0], union[3] - union[1]
        scale = min((width - 2 * MARGIN) / union_w, (height - 2 * MARGIN) / union_h, 1.0)
        scaled_canvas = (max(1, round(width * scale)), max(1, round(height * scale)))
        transformed_union_w = union_w * scale
        transformed_union_h = union_h * scale
        tx = round((width - transformed_union_w) / 2 - union[0] * scale)
        ty = round((height - transformed_union_h) / 2 - union[1] * scale)
        dest = OUT / npc / action
        dest.mkdir(parents=True, exist_ok=True)
        entries = []
        for index, (source, image) in enumerate(zip(sources, images)):
            resized = resize_premultiplied(image, scaled_canvas)
            target_image = Image.new("RGBA", canvas, (0, 0, 0, 0))
            target_image.alpha_composite(resized, (tx, ty))
            array = np.asarray(target_image).copy()
            array[array[..., 3] == 0, :3] = 0
            target = dest / f"frame-{index}.png"
            Image.fromarray(array, "RGBA").save(target, optimize=False, compress_level=9)
            entries.append({"frame": index, "sourceSha256": sha(source), "outputSha256": sha(target), "bytePreserved": False})
        manifest["actions"][npc][action] = {
            "status": "corrected",
            "sourceUnionAlphaBbox": list(union),
            "uniformScale": scale,
            "translation": [tx, ty],
            "frames": entries,
        }

OUT.mkdir(parents=True, exist_ok=True)
(OUT / "selected-manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
