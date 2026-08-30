from collections import deque
from pathlib import Path
import hashlib
import json
import sys

import numpy as np
from PIL import Image, ImageFilter


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def flood_background(rgb):
    hi = rgb.min(axis=2) >= 235
    neutral = (rgb.max(axis=2) - rgb.min(axis=2)) <= 7
    eligible = hi & neutral
    h, w = eligible.shape
    seen = np.zeros((h, w), dtype=np.uint8)
    q = deque()
    for x in range(w):
        if eligible[0, x]: q.append((0, x)); seen[0, x] = 1
        if eligible[h-1, x] and not seen[h-1, x]: q.append((h-1, x)); seen[h-1, x] = 1
    for y in range(h):
        if eligible[y, 0] and not seen[y, 0]: q.append((y, 0)); seen[y, 0] = 1
        if eligible[y, w-1] and not seen[y, w-1]: q.append((y, w-1)); seen[y, w-1] = 1
    while q:
        y, x = q.popleft()
        for yy, xx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
            if 0 <= yy < h and 0 <= xx < w and eligible[yy, xx] and not seen[yy, xx]:
                seen[yy, xx] = 1; q.append((yy, xx))
    return seen.astype(bool)


def main(src_s, out_dir_s, erosion=3):
    src = Path(src_s); out_dir = Path(out_dir_s); out_dir.mkdir(parents=True, exist_ok=True)
    rgb = np.asarray(Image.open(src).convert("RGB"), dtype=np.uint8)
    bg = flood_background(rgb)
    hard = (~bg).astype(np.uint8) * 255
    # Remove the one-pixel matte-connected fringe, then restore a narrow antialiased edge.
    mask_im = Image.fromarray(hard, "L").filter(ImageFilter.MinFilter(erosion)).filter(ImageFilter.GaussianBlur(0.7))
    alpha = np.asarray(mask_im, dtype=np.uint8)
    # Fail closed if foreground touches the canvas.
    if np.any(alpha[0]) or np.any(alpha[-1]) or np.any(alpha[:,0]) or np.any(alpha[:,-1]):
        raise SystemExit("foreground touches border after extraction")
    a = alpha.astype(np.float32) / 255.0
    source = rgb.astype(np.float32) / 255.0
    matte = np.full_like(source, 250.0 / 255.0)
    # Edge-only matte decontamination. Fully opaque RGB remains byte-identical.
    fg = source.copy()
    partial = (a > 0) & (a < 1)
    if np.any(partial):
        rec = (source - matte * (1.0 - a[...,None])) / np.maximum(a[...,None], 1/255)
        fg[partial] = np.clip(rec[partial], 0, 1)
    out = np.concatenate([np.rint(fg*255).astype(np.uint8), alpha[...,None]], axis=2)
    out[alpha == 0, :3] = 0
    output = out_dir / "candidate-alpha-r1.png"
    mask_path = out_dir / "mask-r1.png"
    Image.fromarray(out, "RGBA").save(output, compress_level=9)
    mask_im.save(mask_path, compress_level=9)
    ys, xs = np.where(alpha >= 16)
    metrics = {
        "source": str(src), "source_sha256": sha(src), "output": str(output), "output_sha256": sha(output),
        "mask": str(mask_path), "mask_sha256": sha(mask_path), "size": [rgb.shape[1], rgb.shape[0]],
        "bbox_alpha16": [int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)],
        "bbox_percent": [round((xs.max()+1-xs.min())/rgb.shape[1]*100,4),round((ys.max()+1-ys.min())/rgb.shape[0]*100,4)],
        "corner_alpha": [int(alpha[0,0]),int(alpha[0,-1]),int(alpha[-1,0]),int(alpha[-1,-1])],
        "partial_alpha_pixels": int(((alpha>0)&(alpha<255)).sum()),
        "transparent_rgb_residue": int(np.any(out[alpha==0,:3] != 0, axis=1).sum()),
        "opaque_rgb_mismatch": int(np.any(out[alpha==255,:3] != rgb[alpha==255], axis=1).sum()),
        "rule": f"border-connected neutral RGB>=235 checker removal; subject MinFilter{erosion}; Gaussian0.7 partial edge; 250-gray edge-only decontamination; alpha0 RGB zero"
    }
    (out_dir / "metrics-r1.json").write_text(json.dumps(metrics, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
    print(json.dumps(metrics, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], int(sys.argv[3]) if len(sys.argv) > 3 else 3)
