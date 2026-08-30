from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json

ROOT = Path(__file__).resolve().parent
paths = [
    ROOT / 'selected/seojin-charge-G1-selected-v5-sword.png',
    ROOT / 'selected/seojin-charge-G2-selected-v8-staggered-pressure.png',
    ROOT / 'selected/seojin-charge-G3-selected-v7-sword.png',
]
canvas = Image.new('RGB', (1664, 920), (216, 213, 204))
draw = ImageDraw.Draw(canvas)
font = ImageFont.load_default(size=22)
small = ImageFont.load_default(size=17)
draw.text((28, 18), 'Seojin Charge - sword-driven v6 grayscale structure gate', fill=(24, 27, 30), font=font)
rows = []
for index, path in enumerate(paths):
    image = Image.open(path).convert('RGB')
    gray = image.convert('L').convert('RGB')
    x = 32 + index * 544
    full = gray.copy()
    full.thumbnail((480, 480), Image.Resampling.LANCZOS)
    tile = Image.new('RGB', (480, 480), (216, 213, 204))
    tile.paste(full, ((480 - full.width) // 2, (480 - full.height) // 2))
    canvas.paste(tile, (x, 70))
    draw.text((x, 560), f'G{index + 1}', fill=(24, 27, 30), font=font)
    for size, y in ((200, 600), (80, 805), (32, 840)):
        preview = gray.resize((size, size), Image.Resampling.LANCZOS)
        canvas.paste(preview, (x, y))
    rows.append({'grade': index + 1, 'path': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()})
draw.text((32, 885), 'G2: two staggered pressure trails remain separate, then merge before the shared impact. One blade/one impact only.', fill=(55, 58, 62), font=small)
output = ROOT / 'contact-seojin-charge-sword-v6-grayscale.png'
canvas.save(output, compress_level=9)
manifest = {'status': 'SEOJIN_G2_V6_GRAYSCALE_REVIEW_READY', 'contact': str(output), 'contactSha256': hashlib.sha256(output.read_bytes()).hexdigest(), 'grades': rows}
(ROOT / 'seojin-sword-v6-manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n')
