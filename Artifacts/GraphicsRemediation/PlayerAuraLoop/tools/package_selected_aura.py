#!/usr/bin/env python3
import hashlib
import json
from pathlib import Path

from PIL import Image


ROOT = Path('/Users/pvenus/ProjectBS')
SOURCE = ROOT / 'Artifacts/candidates/seojin-selection-indicator-neon-revision-04-K-v20260906/K/source-board.png'
OUT = ROOT / 'Artifacts/GraphicsRemediation/PlayerAuraLoop/selected/revision-03-neon-refinement-K'
CANVAS = (1536, 1024)
INNER = 1408
OFFSET = ((CANVAS[0] - INNER) // 2, 0)
TIMES = [0.00, 0.16, 0.32, 0.48, 0.64, 0.80]


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open('rb') as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b''):
            h.update(chunk)
    return h.hexdigest()


def sanitize(im: Image.Image) -> Image.Image:
    im = im.convert('RGBA')
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a <= 3:
                px[x, y] = (0, 0, 0, 0)
    return im


def split_half(im: Image.Image, back: bool) -> Image.Image:
    result = im.copy()
    px = result.load()
    if back:
        ys = range(result.height // 2, result.height)
    else:
        ys = range(0, result.height // 2)
    for y in ys:
        for x in range(result.width):
            px[x, y] = (0, 0, 0, 0)
    return result


def main() -> None:
    source = Image.open(SOURCE).convert('RGBA')
    if source.size != (1536, 1024):
        raise SystemExit(f'unexpected source size {source.size}')
    for d in ('combined', 'back', 'front'):
        (OUT / d).mkdir(parents=True, exist_ok=True)

    records = []
    for i in range(6):
        col, row = i % 3, i // 3
        cell = source.crop((col * 512, row * 512, (col + 1) * 512, (row + 1) * 512))
        cell = sanitize(cell)
        cell = cell.resize((INNER, INNER), Image.Resampling.LANCZOS)
        cell = cell.crop((0, (INNER - CANVAS[1]) // 2, INNER, (INNER + CANVAS[1]) // 2))
        frame = Image.new('RGBA', CANVAS, (0, 0, 0, 0))
        frame.alpha_composite(cell, OFFSET)
        frame = sanitize(frame)
        outputs = {}
        for kind, image in (
            ('combined', frame),
            ('back', split_half(frame, True)),
            ('front', split_half(frame, False)),
        ):
            path = OUT / kind / f'frame_{i + 1:02d}.png'
            image.save(path, optimize=False)
            outputs[kind] = {'path': str(path), 'sha256': sha256(path)}
        records.append({'index': i, 'time': TIMES[i], 'sourceCell': [col, row], 'outputs': outputs})

    manifest = {
        'schemaVersion': 'player_aura_loop_selected_source_v1',
        'status': 'ART_SOURCE_SELECTED_INSTALL_NOT_AUTHORIZED',
        'selection': 'K',
        'source': {'path': str(SOURCE), 'sha256': sha256(SOURCE), 'layout': '3x2 row-major'},
        'processing': {
            'operation': 'deterministic fixed-cell extraction, uniform Lanczos contain, alpha<=1 sanitation, horizontal back/front split',
            'cellSize': [512, 512], 'canvas': list(CANVAS), 'uniformScaledSizeBeforeVerticalWindow': [INNER, INNER],
            'verticalWindow': [0, (INNER - CANVAS[1]) // 2, INNER, (INNER + CANVAS[1]) // 2],
            'offset': list(OFFSET), 'repaint': False, 'recolor': False, 'backgroundRemoval': False,
        },
        'runtimeConvention': {'ppu': 512, 'pivot': [0.5, 0.5], 'filter': 'bilinear', 'wrap': 'clamp'},
        'animation': {'frameCount': 6, 'timestamps': TIMES, 'stop': 0.96, 'loop': True},
        'frames': records,
        'promotionTargets': [
            'Assets/ImagesGenerated/Battle/character/player-character-aura-back.png',
            'Assets/ImagesGenerated/Battle/character/player-character-aura-front.png'
        ],
        'boundary': {'unityWrite': False, 'canonicalWrite': False, 'installAuthority': False,
                     'runtimeLoopRequiresAnimatorOrCodeAuthority': True,
                     'rollback': 'artifact package absence; canonical assets untouched'}
    }
    manifest_path = OUT / 'source-manifest.json'
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n')
    print(manifest_path)
    print(sha256(manifest_path))


if __name__ == '__main__':
    main()
