from pathlib import Path
from PIL import Image, ImageOps, ImageDraw

ROOT = Path(__file__).resolve().parent
RAW_BG = ROOT / "raw/backgrounds"
RAW_WALL = ROOT / "raw/barriers"
OUT = ROOT / "exact1536x512"
EVIDENCE = ROOT / "evidence"
OUT.mkdir(exist_ok=True)
EVIDENCE.mkdir(exist_ok=True)

TARGET = (1536, 512)

def alpha_zero_rgb_zero(image: Image.Image) -> Image.Image:
    image = image.convert("RGBA")
    data = bytearray(image.tobytes())
    for i in range(0, len(data), 4):
        if data[i + 3] == 0:
            data[i:i + 3] = b"\x00\x00\x00"
    return Image.frombytes("RGBA", image.size, bytes(data))

def cover_center(image: Image.Image) -> Image.Image:
    image = image.convert("RGB")
    scale = max(TARGET[0] / image.width, TARGET[1] / image.height)
    resized = image.resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    left = (resized.width - TARGET[0]) // 2
    top = (resized.height - TARGET[1]) // 2
    return resized.crop((left, top, left + TARGET[0], top + TARGET[1]))

for wave in range(1, 4):
    bg = cover_center(Image.open(RAW_BG / f"wave{wave}-background.raw.png"))
    bg.save(OUT / f"wave{wave}-background.1536x512.png", compress_level=9)

    for role in ("top", "bottom"):
        image = alpha_zero_rgb_zero(Image.open(RAW_WALL / f"wave{wave}-{role}.raw.png"))
        box = image.getchannel("A").getbbox()
        if not box:
            raise RuntimeError(f"empty alpha: wave{wave}-{role}")
        visible = image.crop(box)
        scale = min(1504 / visible.width, 480 / visible.height)
        visible = visible.resize((round(visible.width * scale), round(visible.height * scale)), Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", TARGET, (0, 0, 0, 0))
        x = (TARGET[0] - visible.width) // 2
        y = 16 if role == "top" else TARGET[1] - 16 - visible.height
        canvas.alpha_composite(visible, (x, y))
        alpha_zero_rgb_zero(canvas).save(OUT / f"wave{wave}-{role}-barrier.1536x512.png", compress_level=9)

rows = []
for wave in range(1, 4):
    bg = Image.open(OUT / f"wave{wave}-background.1536x512.png").convert("RGBA")
    bg.alpha_composite(Image.open(OUT / f"wave{wave}-top-barrier.1536x512.png"))
    bg.alpha_composite(Image.open(OUT / f"wave{wave}-bottom-barrier.1536x512.png"))
    rows.append(bg)

sheet = Image.new("RGB", (1536, 1536), (24, 21, 18))
for index, row in enumerate(rows):
    sheet.paste(row.convert("RGB"), (0, index * 512))
ImageDraw.Draw(sheet).line((0, 512, 1536, 512), fill=(30, 27, 23), width=2)
ImageDraw.Draw(sheet).line((0, 1024, 1536, 1024), fill=(30, 27, 23), width=2)
sheet.save(EVIDENCE / "wave1-wave2-wave3-composite-contact.png", compress_level=9)
