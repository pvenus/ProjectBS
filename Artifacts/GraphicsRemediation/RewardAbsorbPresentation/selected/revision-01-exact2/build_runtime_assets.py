from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parent
PAIRS = (
    (ROOT / "source/reward.gold.coin-charm.source.png", ROOT / "runtime/reward.gold.coin-charm.128.png"),
    (ROOT / "source/reward.xp.faceted-shard.source.png", ROOT / "runtime/reward.xp.faceted-shard.128.png"),
)

for source, output in PAIRS:
    image = Image.open(source).convert("RGBA")
    image = image.resize((112, 112), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    canvas.alpha_composite(image, (8, 8))
    pixels = bytearray(canvas.tobytes())
    for index in range(0, len(pixels), 4):
        if pixels[index + 3] == 0:
            pixels[index:index + 3] = b"\x00\x00\x00"
    clean = Image.frombytes("RGBA", canvas.size, bytes(pixels))
    clean.save(output, format="PNG", optimize=False, compress_level=9)
