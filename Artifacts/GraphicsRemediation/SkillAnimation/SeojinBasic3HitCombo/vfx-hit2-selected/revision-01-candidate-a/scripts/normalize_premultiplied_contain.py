#!/usr/bin/env python3
from pathlib import Path
from PIL import Image

SOURCE_ROOT = Path("/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/vfx-hit2-review/revision-01-neon-abc/A")
OUTPUT_ROOT = Path("/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/vfx-hit2-selected/revision-01-candidate-a/frames")
CANVAS = (256, 256)
SAFE_MARGIN = 12
FILTER = Image.Resampling.LANCZOS


def normalize(source: Path, output: Path) -> None:
    image = Image.open(source).convert("RGBA")
    alpha = image.getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        raise ValueError(f"empty alpha: {source}")

    subject = image.crop(bbox)
    sw, sh = subject.size
    safe_w = CANVAS[0] - SAFE_MARGIN * 2
    safe_h = CANVAS[1] - SAFE_MARGIN * 2
    scale = min(safe_w / sw, safe_h / sh)
    dw = max(1, round(sw * scale))
    dh = max(1, round(sh * scale))

    rgba = list(subject.getdata())
    premultiplied = Image.new("RGBA", subject.size)
    premultiplied.putdata([
        ((r * a + 127) // 255, (g * a + 127) // 255, (b * a + 127) // 255, a)
        for r, g, b, a in rgba
    ])
    resized = premultiplied.resize((dw, dh), FILTER)

    clean = []
    for pr, pg, pb, a in resized.getdata():
        if a == 0:
            clean.append((0, 0, 0, 0))
        else:
            clean.append((min(255, (pr * 255 + a // 2) // a),
                          min(255, (pg * 255 + a // 2) // a),
                          min(255, (pb * 255 + a // 2) // a), a))
    resized.putdata(clean)

    canvas = Image.new("RGBA", CANVAS, (0, 0, 0, 0))
    x = (CANVAS[0] - dw) // 2
    y = (CANVAS[1] - dh) // 2
    canvas.alpha_composite(resized, (x, y))
    pixels = [(0, 0, 0, 0) if a == 0 else (r, g, b, a)
              for r, g, b, a in canvas.getdata()]
    canvas.putdata(pixels)
    output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output, format="PNG", optimize=False, compress_level=9)


def main() -> None:
    for index in range(6):
        normalize(SOURCE_ROOT / f"frame-{index:02d}.png",
                  OUTPUT_ROOT / f"frame-{index:02d}.png")


if __name__ == "__main__":
    main()
