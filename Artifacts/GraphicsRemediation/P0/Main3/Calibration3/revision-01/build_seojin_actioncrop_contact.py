from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json

ROOT = Path(__file__).resolve().parent
paths = [
    ROOT / 'selected/seojin-charge-G1-selected-v3-actioncrop.png',
    ROOT / 'selected/seojin-charge-G2-selected-v4-actioncrop.png',
    ROOT / 'selected/seojin-charge-G3-selected-v5-actioncrop.png',
]
canvas = Image.new('RGB', (1664, 800), (216, 213, 204))
draw = ImageDraw.Draw(canvas)
font = ImageFont.load_default(size=22)
small = ImageFont.load_default(size=17)
draw.text((28, 18), 'Seojin Charge - close-action correction strip v3', fill=(24, 27, 30), font=font)
rows = []
for index, path in enumerate(paths):
    image = Image.open(path).convert('RGBA')
    x = 32 + index * 544
    tile = Image.new('RGB', (480, 480), (216, 213, 204))
    fitted = image.copy()
    fitted.thumbnail((460, 460), Image.Resampling.LANCZOS)
    tile.paste(fitted.convert('RGB'), ((480 - fitted.width) // 2, (480 - fitted.height) // 2))
    canvas.paste(tile, (x, 70))
    draw.text((x, 560), f'G{index + 1}', fill=(24, 27, 30), font=font)
    for size, sx in ((80, x), (32, x + 210)):
        tiny = image.resize((size, size), Image.Resampling.LANCZOS)
        for background, bx in (((216, 213, 204), sx), ((18, 19, 21), sx + size + 8)):
            preview = Image.new('RGBA', (size, size), background + (255,))
            preview.alpha_composite(tiny)
            canvas.paste(preview.convert('RGB'), (bx, 610))
    rows.append({'grade': index + 1, 'path': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()})
draw.text((32, 735), 'Same close-action axis: G1 shield+tip+impact; G2 + joined shield; G3 + two side masses and open forward corridor.', fill=(55, 58, 62), font=small)
output = ROOT / 'contact-seojin-charge-actioncrop-v3.png'
canvas.save(output, compress_level=9)
manifest = {'status': 'SEOJIN_ACTIONCROP_USER_REVIEW_READY', 'contact': str(output), 'contactSha256': hashlib.sha256(output.read_bytes()).hexdigest(), 'grades': rows}
(ROOT / 'seojin-actioncrop-v3-manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n')
