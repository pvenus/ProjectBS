from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "source"
RUNTIME = ROOT / "runtime"
RUNTIME.mkdir(exist_ok=True)

for source in sorted(SOURCE.glob("*.png")):
    image = Image.open(source).convert("RGBA")
    width, height = image.size
    scale = min(928 / width, 928 / height)
    target = (max(1, round(width * scale)), max(1, round(height * scale)))
    image = image.resize(target, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    x = (1024 - target[0]) // 2
    y = 976 - target[1]
    canvas.alpha_composite(image, (x, y))
    pixels = bytearray(canvas.tobytes())
    for index in range(0, len(pixels), 4):
        if pixels[index + 3] == 0:
            pixels[index:index + 3] = b"\x00\x00\x00"
    clean = Image.frombytes("RGBA", canvas.size, bytes(pixels))
    clean.save(RUNTIME / source.name, format="PNG", optimize=False, compress_level=9)
