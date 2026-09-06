from __future__ import annotations

import hashlib
import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image


SOURCE = Path(
    "/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/"
    "SeojinBasic3HitCombo/motion-review/candidate-C-family-C1-C5-revision-01/"
    "gifs/C3-continuous18-768x512.gif"
)


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def smoothstep(edge0: float, edge1: float, x: np.ndarray) -> np.ndarray:
    t = np.clip((x - edge0) / (edge1 - edge0), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def decode() -> tuple[list[np.ndarray], list[int]]:
    gif = Image.open(SOURCE)
    frames: list[np.ndarray] = []
    delays: list[int] = []
    for index in range(gif.n_frames):
        gif.seek(index)
        frames.append(np.asarray(gif.convert("RGB"), dtype=np.uint8))
        delays.append(int(gif.info.get("duration", 0)))
    return frames, delays


def cleanup(output: Path) -> dict:
    output.mkdir(parents=True, exist_ok=True)
    frames, delays = decode()
    stack = np.stack(frames).astype(np.float32)

    # The review background, frame number, and panel guides are fixed in screen
    # space. A bright temporal percentile recovers that common presentation
    # layer while moving character, weapon, cloth, and ink remain deltas.
    background = np.percentile(stack, 95.0, axis=0)
    Image.fromarray(np.clip(background, 0, 255).astype(np.uint8), "RGB").save(
        output / "temporal-background-model.png"
    )

    records = []
    rgba_frames: list[Image.Image] = []
    for index, rgb_u8 in enumerate(frames):
        rgb = rgb_u8.astype(np.float32)
        dark_delta = np.max(background - rgb, axis=2)
        color_delta = np.max(np.abs(background - rgb), axis=2)
        score = np.maximum(dark_delta, color_delta * 0.70)

        # Low quantization noise remains background. The soft interval retains
        # pale robe and dry-brush edges without synthesizing foreground color.
        alpha = np.rint(255.0 * smoothstep(2.0, 24.0, score)).astype(np.uint8)

        # Fixed presentation markings have near-zero temporal delta. Explicitly
        # clear the narrow outer perimeter; the selected motion never occupies it.
        alpha[:2, :] = 0
        alpha[-2:, :] = 0
        alpha[:, :2] = 0
        alpha[:, -2:] = 0

        rgba = np.dstack([rgb_u8, alpha])
        rgba[alpha == 0, :3] = 0
        image = Image.fromarray(rgba, "RGBA")
        path = output / f"frame-{index:02d}.png"
        image.save(path)
        rgba_frames.append(image)

        bbox = image.getchannel("A").getbbox()
        records.append(
            {
                "frame": index,
                "sha256": sha256(path),
                "alpha_bbox": list(bbox) if bbox else None,
                "nonzero_alpha": int(np.count_nonzero(alpha)),
                "opaque_alpha": int(np.count_nonzero(alpha == 255)),
                "border_nonzero_alpha": int(
                    np.count_nonzero(
                        np.concatenate(
                            [alpha[0], alpha[-1], alpha[:, 0], alpha[:, -1]]
                        )
                    )
                ),
                "alpha0_rgb_residue": int(
                    np.count_nonzero(
                        np.any(rgba[:, :, :3] != 0, axis=2) & (alpha == 0)
                    )
                ),
            }
        )

    manifest = {
        "method": "deterministic_temporal_background_segmentation_v1",
        "source": str(SOURCE),
        "source_sha256": sha256(SOURCE),
        "dimensions": [768, 512],
        "physical_frames": len(frames),
        "delays_ms": delays,
        "duration_ms": sum(delays),
        "loop": "infinite",
        "segments": {"hit1": [0, 6], "hit2": [6, 12], "hit3": [12, 18]},
        "contacts": [4, 10, 16],
        "parameters": {
            "temporal_background_percentile": 95.0,
            "color_delta_weight": 0.70,
            "alpha_smoothstep": [2.0, 24.0],
            "cleared_perimeter_px": 2,
        },
        "frames": records,
    }
    (output / "manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n", encoding="utf-8"
    )
    return manifest


if __name__ == "__main__":
    if len(sys.argv) != 2:
        raise SystemExit("usage: cleanup_c3.py OUTPUT_DIR")
    cleanup(Path(sys.argv[1]))
