from pathlib import Path
import hashlib
import json
import shutil

from PIL import Image, ImageDraw


ROOT = Path(__file__).parent
PROJECT = ROOT.parents[6]

PAIRS = {
    "basic_attack": (
        "skill.character.yujin.3.basic_attack.basic_attack.icon",
        "skill.character.yujin.3.clone.basic_attack.basic_attack.icon",
    ),
    "passive_1": (
        "skill.character.yujin.3.passive_1.passive_1.icon",
        "skill.character.yujin.3.clone.passive_1.passive_1.icon",
    ),
    "multi_shot": (
        "skill.character.yujin.3.active_1.multi_shot.icon",
        "skill.character.yujin.3.clone.active_1.multi_shot.icon",
    ),
    "hwalbin_barrage": (
        "skill.character.yujin.3.active_2.hwalbin_barrage.icon",
        "skill.character.yujin.3.clone.active_2.hwalbin_barrage.icon",
    ),
}


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def selected(asset: Path) -> Path:
    revised = sorted((asset / "selected-revision-02").glob("*.png"))
    if revised:
        return revised[0]
    return next((asset / "selected").glob("*.png"))


def alpha(asset: Path) -> Path:
    basic_corrected = sorted((asset / "alpha-revision-02-correction-01").glob("*selected-alpha-r2c1.png"))
    if basic_corrected:
        return basic_corrected[0]
    basic_revised = sorted((asset / "alpha-revision-02").glob("*selected-alpha-r2.png"))
    if basic_revised:
        return basic_revised[0]
    corrected = sorted((asset / "alpha-correction-01").glob("*selected-alpha-r1.png"))
    if corrected:
        return corrected[0]
    return next((asset / "alpha").glob("*selected-alpha.png"))


projections = []
for family, (source_id, clone_id) in PAIRS.items():
    source_asset = ROOT / "Y3" / source_id
    clone_asset = ROOT / "Y3C" / clone_id
    out_selected = clone_asset / "selected" / "selected-from-Y3.png"
    out_alpha = clone_asset / "alpha" / f"{clone_id}.selected-alpha.png"
    out_selected.parent.mkdir(parents=True, exist_ok=True)
    out_alpha.parent.mkdir(parents=True, exist_ok=True)
    source_selected = selected(source_asset)
    source_alpha = alpha(source_asset)
    shutil.copyfile(source_selected, out_selected)
    shutil.copyfile(source_alpha, out_alpha)
    record = {
        "family": family,
        "sourceAssetId": source_id,
        "cloneAssetId": clone_id,
        "projection": "byte-identical artwork; distinct canonical identity only",
        "selectedSource": str(source_selected.relative_to(ROOT)),
        "selectedSourceSha256": sha256(source_selected),
        "selectedProjection": str(out_selected.relative_to(ROOT)),
        "selectedProjectionSha256": sha256(out_selected),
        "alphaSource": str(source_alpha.relative_to(ROOT)),
        "alphaSourceSha256": sha256(source_alpha),
        "alphaProjection": str(out_alpha.relative_to(ROOT)),
        "alphaProjectionSha256": sha256(out_alpha),
        "selectedByteIdentical": source_selected.read_bytes() == out_selected.read_bytes(),
        "alphaByteIdentical": source_alpha.read_bytes() == out_alpha.read_bytes(),
    }
    manifest = clone_asset / "projection-manifest.json"
    manifest.write_text(json.dumps(record, ensure_ascii=False, indent=2) + "\n")
    record["manifest"] = str(manifest.relative_to(ROOT))
    record["manifestSha256"] = sha256(manifest)
    projections.append(record)


def asset_alpha(grade: str, needle: str):
    matches = sorted((ROOT / grade).glob(f"*{needle}*"))
    return alpha(matches[0]) if matches else None


g1 = PROJECT / "Assets/ImagesGenerated/Skill/icon"
rows = [
    ("basic", g1 / "skill.character.yujin.1.basic_attack.basic_attack.icon.png", asset_alpha("Y2", "basic_attack"), asset_alpha("Y3", "basic_attack")),
    ("passive", g1 / "skill.character.yujin.1.passive_1.passive_1.icon.png", asset_alpha("Y2", "passive_1"), asset_alpha("Y3", "passive_1")),
    ("multi", g1 / "skill.character.yujin.1.active_1.multi_shot.icon.png", asset_alpha("Y2", "multi_shot"), asset_alpha("Y3", "multi_shot")),
    ("barrage", None, asset_alpha("Y2", "hwalbin_barrage"), asset_alpha("Y3", "hwalbin_barrage")),
    ("outlaw", None, None, asset_alpha("Y3", "outlaw_appearance")),
]


evidence = ROOT / "family-evidence"
evidence.mkdir(exist_ok=True)
contacts = []
for size in (200, 80, 32):
    pad = max(8, size // 8)
    label_h = 18
    cell_w = size + pad * 2
    cell_h = size + pad * 2 + label_h
    canvas = Image.new("RGB", (cell_w * 3, cell_h * len(rows)), "#171b20")
    draw = ImageDraw.Draw(canvas)
    for row_index, (name, *images) in enumerate(rows):
        for col_index, path in enumerate(images):
            x = col_index * cell_w + pad
            y = row_index * cell_h + pad + label_h
            draw.text((col_index * cell_w + 4, row_index * cell_h + 2), f"{name} G{col_index + 1}", fill="#d8d5cc")
            if path and path.exists():
                icon = Image.open(path).convert("RGBA")
                icon.thumbnail((size, size), Image.Resampling.LANCZOS)
                px = x + (size - icon.width) // 2
                py = y + (size - icon.height) // 2
                canvas.paste(icon, (px, py), icon)
    output = evidence / f"family-contact-{size}.png"
    canvas.save(output, optimize=False, compress_level=9)
    contacts.append({"size": size, "path": str(output.relative_to(ROOT)), "sha256": sha256(output)})


family_manifest = {
    "status": "VISUAL_ALPHA_FAMILY_READY_TECHNICAL_T0_PENDING_NOT_PROMOTABLE",
    "y3cProjection": projections,
    "contacts": contacts,
    "rules": {
        "gradeEvolution": "G1 individual skill; G2 connected tactical reach; G3 completed command structure",
        "cloneProjection": "corresponding Y3 artwork byte-identical; no clone badge/person/hue shift",
        "canonicalWrite": False,
        "unity": False,
        "staging": False,
    },
}
manifest_path = ROOT / "family-final-handoff-manifest.json"
manifest_path.write_text(json.dumps(family_manifest, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"manifest": str(manifest_path), "sha256": sha256(manifest_path), **family_manifest}, ensure_ascii=False, indent=2))
