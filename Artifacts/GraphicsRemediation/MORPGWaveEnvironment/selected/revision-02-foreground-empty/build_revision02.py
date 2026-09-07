from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
RAW = ROOT / "raw"
OUT = ROOT / "exact1536x512"
EVIDENCE = ROOT / "evidence"
REV1 = ROOT.parent / "revision-01" / "exact1536x512"
OUT.mkdir(exist_ok=True)
EVIDENCE.mkdir(exist_ok=True)

for wave in range(1, 4):
    image = Image.open(RAW / f"wave{wave}-background.raw.png").convert("RGB")
    scale = 1536 / image.width
    resized = image.resize((1536, round(image.height * scale)), Image.Resampling.LANCZOS)
    if resized.height < 512:
        raise RuntimeError(f"insufficient height: wave{wave}")
    final = resized.crop((0, 0, 1536, 512))
    final.save(OUT / f"wave{wave}-background.1536x512.png", compress_level=9)

backgrounds = Image.new("RGB", (1536, 1536), (24, 21, 18))
composites = Image.new("RGB", (1536, 1536), (24, 21, 18))
for wave in range(1, 4):
    bg = Image.open(OUT / f"wave{wave}-background.1536x512.png").convert("RGBA")
    backgrounds.paste(bg.convert("RGB"), (0, (wave - 1) * 512))
    composite = bg.copy()
    composite.alpha_composite(Image.open(REV1 / f"wave{wave}-top-barrier.1536x512.png"))
    composite.alpha_composite(Image.open(REV1 / f"wave{wave}-bottom-barrier.1536x512.png"))
    composites.paste(composite.convert("RGB"), (0, (wave - 1) * 512))

for sheet in (backgrounds, composites):
    draw = ImageDraw.Draw(sheet)
    draw.line((0, 512, 1536, 512), fill=(30, 27, 23), width=2)
    draw.line((0, 1024, 1536, 1024), fill=(30, 27, 23), width=2)

backgrounds.save(EVIDENCE / "backgrounds-wave1-wave2-wave3.png", compress_level=9)
composites.save(EVIDENCE / "backgrounds-with-revision01-barriers.png", compress_level=9)
