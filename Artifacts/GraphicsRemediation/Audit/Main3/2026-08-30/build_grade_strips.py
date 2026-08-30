from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[4]
FAMILIES = {
    "Seojin — Charge": [
        "Assets/ImagesGenerated/Skill/icon/skill.character.seojin.1.active_1.active_1.icon.png",
        "Assets/ImagesGenerated/Skill/icon/skill.character.seojin.2.active_1.charge.icon.png",
        "Assets/ImagesGenerated/Skill/icon/skill.character.seojin.3.active_1.charge.icon.png",
    ],
    "Jihan — Medicine Prescription": [
        "Assets/ImagesGenerated/Skill/icon/skill.character.jihan.1.active_1.medicine_prescription.icon.png",
        "Assets/ImagesGenerated/Skill/icon/skill.character.jihan.2.active_1.medicine_prescription.icon.png",
        "Assets/ImagesGenerated/Skill/icon/skill.character.jihan.3.active_1.medicine_prescription.icon.png",
    ],
    "Yujin — Multi Shot": [
        "Assets/ImagesGenerated/Skill/icon/skill.character.yujin.1.active_1.multi_shot.icon.png",
        "Assets/ImagesGenerated/Skill/icon/skill.character.yujin.2.active_1.multi_shot.icon.png",
        "Assets/ImagesGenerated/Skill/icon/skill.character.yujin.3.active_1.multi_shot.icon.png",
    ],
}
COLOR_NOTES = {
    "Seojin — Charge": {
        "anchor": [(29,44,67),(24,27,31),(126,52,34)],
        "support": [(105,111,116),(224,217,198),(132,100,63),(92,72,56)],
        "note": "Anchor: navy/ink + recurring russet impact. Recover iron gray, paper white, muted bronze/ash-brown for material and grade depth; no hue is categorically banned."
    },
    "Jihan — Medicine Prescription": {
        "anchor": [(176,132,67),(112,124,93),(65,145,139)],
        "support": [(235,229,207),(139,69,48),(95,132,151),(48,52,51)],
        "note": "Anchor: herb ochre/green-gray + teal circulation. Recover paper, ceramic blue, medicinal red-brown and charcoal; amber/gold is valid only when meaning/value structure supports it."
    },
    "Yujin — Multi Shot": {
        "anchor": [(31,54,70),(91,125,143),(220,221,213)],
        "support": [(115,83,57),(96,101,104),(178,139,70),(132,61,45)],
        "note": "Anchor: ink-blue/blue-gray on the firing axis. Recover feather brown, iron gray, ochre or restrained russet for physical material/impact and grade contrast; monochrome is not required."
    },
}

font = ImageFont.load_default(size=22)
small = ImageFont.load_default(size=18)
outputs = {}
rows = []
for title, rels in FAMILIES.items():
    canvas = Image.new("RGB", (1664, 870), (214, 211, 202))
    d = ImageDraw.Draw(canvas)
    d.text((32, 18), title, fill=(25, 27, 30), font=font)
    for i, rel in enumerate(rels):
        src = PROJECT / rel
        icon = Image.open(src).convert("RGBA")
        icon.thumbnail((480, 480), Image.Resampling.LANCZOS)
        x = 32 + i * 544 + (480 - icon.width)//2
        y = 85 + (480 - icon.height)//2
        checker = Image.new("RGB", (480,480), (236,234,228))
        cd = ImageDraw.Draw(checker)
        for cy in range(0,480,32):
            for cx in range(0,480,32):
                if ((cx//32)+(cy//32))%2: cd.rectangle((cx,cy,cx+31,cy+31), fill=(205,207,208))
        checker.paste(icon, ((480-icon.width)//2,(480-icon.height)//2), icon)
        canvas.paste(checker, (32+i*544,85))
        d.text((32+i*544, 580), f"G{i+1}", fill=(25,27,30), font=font)
        d.text((82+i*544, 583), src.name[:42], fill=(55,58,62), font=small)
        # Actual-size readability row: 80 px and synthetic 32 px, neutral/dark.
        for size, sx in ((80, 32+i*544), (32, 130+i*544)):
            tiny = Image.open(src).convert("RGBA").resize((size,size), Image.Resampling.LANCZOS)
            for bg, bx in (((214,211,202), sx), ((18,19,21), sx+size+10)):
                tile = Image.new("RGBA", (size,size), bg+(255,)); tile.alpha_composite(tiny)
                canvas.paste(tile.convert("RGB"), (bx, 640))
        d.text((32+i*544, 720), "80px light/dark · 32px light/dark", fill=(55,58,62), font=small)
    note = COLOR_NOTES[title]
    d.text((32, 770), "Signature = recognition anchor, not color restriction", fill=(25,27,30), font=font)
    x = 32
    for color in note["anchor"]:
        d.rectangle((x,805,x+38,843), fill=color, outline=(245,245,240)); x += 46
    x += 20
    for color in note["support"]:
        d.rectangle((x,805,x+38,843), fill=color, outline=(245,245,240)); x += 46
    d.text((410, 806), note["note"], fill=(45,48,52), font=small)
    slug = title.split(" ")[0].lower()
    out = ROOT / f"main3-{slug}-g1-g2-g3-strip.png"
    canvas.save(out, optimize=False, compress_level=9)
    outputs[title] = {"path": str(out), "sha256": hashlib.sha256(out.read_bytes()).hexdigest(), "sources": rels}
    rows.append(canvas)

combined = Image.new("RGB", (1664, 2610), (214,211,202))
for i,row in enumerate(rows): combined.paste(row,(0,i*870))
combined_path = ROOT / "main3-grade-evolution-comparison.png"
combined.save(combined_path, optimize=False, compress_level=9)
outputs["combined"] = {"path": str(combined_path), "sha256": hashlib.sha256(combined_path.read_bytes()).hexdigest()}
(ROOT / "manifest.json").write_text(json.dumps(outputs, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
