#!/usr/bin/env python3
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parent
OUT = ROOT / "selected-border-clean"
OUT.mkdir(exist_ok=True)

for index in range(4):
    source = ROOT / f"frame-{index}.png"
    image = Image.open(source).convert("RGBA")
    pixels = image.load()
    width, height = image.size
    for x in range(width):
        for y in (0, height - 1):
            if pixels[x, y][3] <= 1:
                pixels[x, y] = (0, 0, 0, 0)
    for y in range(height):
        for x in (0, width - 1):
            if pixels[x, y][3] <= 1:
                pixels[x, y] = (0, 0, 0, 0)
    image.save(OUT / source.name, optimize=False, compress_level=9)
