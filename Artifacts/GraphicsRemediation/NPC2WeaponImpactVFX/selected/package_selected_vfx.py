from __future__ import annotations

import hashlib
import json
from pathlib import Path

from PIL import Image


ROOT = Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/NPC2WeaponImpactVFX/selected')
BLACK_BOARD = ROOT / 'revision-02-A-r2/black_cloth_raider/VFX-board.png'
CHAIN_ROOT = ROOT / 'revision-03-A-chain-standalone/chain_dragger_raider/frames'
OUT = ROOT / 'revision-04-final-exact8'
CANVAS = 627
SAFE_PAD = 24


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def sanitize_alpha(image: Image.Image) -> Image.Image:
    image = image.convert('RGBA')
    pixels = image.load()
    for y in range(image.height):
        for x in range(image.width):
            r, g, b, a = pixels[x, y]
            if a == 0:
                pixels[x, y] = (0, 0, 0, 0)
    return image


def alpha_bbox(image: Image.Image) -> tuple[int, int, int, int]:
    bbox = image.getchannel('A').getbbox()
    if bbox is None:
        raise ValueError('empty alpha image')
    return bbox


def save_black() -> list[dict[str, object]]:
    board = Image.open(BLACK_BOARD).convert('RGBA')
    if board.size != (1254, 1254):
        raise ValueError(f'unexpected black board size {board.size}')
    target = OUT / 'black_cloth_raider/VFX'
    target.mkdir(parents=True, exist_ok=True)
    records = []
    for index, (col, row) in enumerate(((0, 0), (1, 0), (0, 1), (1, 1)), 1):
        frame = board.crop((col * CANVAS, row * CANVAS, (col + 1) * CANVAS, (row + 1) * CANVAS))
        frame = sanitize_alpha(frame)
        bbox = alpha_bbox(frame)
        output = target / f'frame_{index:02d}.png'
        frame.save(output, 'PNG')
        records.append({'frame': index, 'sourceCell': [col, row], 'bbox': list(bbox), 'sha256': sha256(output)})
    return records


def save_chain() -> tuple[list[dict[str, object]], float]:
    sources = [Image.open(CHAIN_ROOT / f'frame_{index:02d}.png').convert('RGBA') for index in range(1, 5)]
    boxes = [alpha_bbox(image) for image in sources]
    max_width = max(right - left for left, top, right, bottom in boxes)
    max_height = max(bottom - top for left, top, right, bottom in boxes)
    scale = min((CANVAS - SAFE_PAD * 2) / max_width, (CANVAS - SAFE_PAD * 2) / max_height)
    target = OUT / 'chain_dragger_raider/VFX'
    target.mkdir(parents=True, exist_ok=True)
    records = []
    for index, (image, bbox) in enumerate(zip(sources, boxes), 1):
        cropped = image.crop(bbox)
        resized = cropped.resize(
            (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale))),
            Image.Resampling.LANCZOS,
        )
        canvas = Image.new('RGBA', (CANVAS, CANVAS), (0, 0, 0, 0))
        offset = ((CANVAS - resized.width) // 2, (CANVAS - resized.height) // 2)
        canvas.alpha_composite(resized, offset)
        canvas = sanitize_alpha(canvas)
        output = target / f'frame_{index:02d}.png'
        canvas.save(output, 'PNG')
        records.append({
            'frame': index,
            'source': str(CHAIN_ROOT / f'frame_{index:02d}.png'),
            'sourceSha256': sha256(CHAIN_ROOT / f'frame_{index:02d}.png'),
            'sourceAlphaBbox': list(bbox),
            'outputOffset': list(offset),
            'outputBbox': list(alpha_bbox(canvas)),
            'sha256': sha256(output),
        })
    return records, scale


OUT.mkdir(parents=True, exist_ok=True)
black = save_black()
chain, chain_scale = save_chain()
provenance = {
    'status': 'SELECTED_SOURCE_EXACT8_PRE_QA',
    'canvas': [CANVAS, CANVAS],
    'pivot': [0.5, 0.5],
    'frameTimes': [0.0, 0.12, 0.24, 0.36],
    'stopTime': 0.48,
    'loop': True,
    'blackBoard': {'path': str(BLACK_BOARD), 'sha256': sha256(BLACK_BOARD), 'frames': black},
    'chain': {'commonScale': chain_scale, 'safePadding': SAFE_PAD, 'frames': chain},
    'pixelOperations': {
        'black': 'lossless 2x2 cell crop; alpha0 RGB sanitized',
        'chain': 'alpha-bbox crop; one common aspect-preserving Lanczos scale; center contain; alpha0 RGB sanitized',
        'repaint': False,
        'recolor': False,
        'backgroundRemoval': False,
    },
}
(OUT / 'normalization-provenance.json').write_text(json.dumps(provenance, ensure_ascii=False, indent=2), encoding='utf-8')
