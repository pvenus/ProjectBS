from __future__ import annotations

import hashlib
import json
import shutil
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).parent
SOURCES = [
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-b5022ffb-00e8-4900-bd6b-50fd7674c7e8.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-48c7f7ef-3494-466a-b673-c0fe0f5168d7.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-fa28d22a-195f-4b1e-9fbb-423d6017a073.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-cd094db6-3cc7-469b-a11b-3b1d68beecb0.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-529edc00-d9df-4a42-8786-fd53370576fa.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-d090bc09-5a2a-4793-9ac6-5d46be5ab6af.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-9e458950-4c53-4082-906f-1e6a7f617236.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-33155abc-521d-48b9-88ce-1c5e75595bb0.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-3562b3b3-9ad8-4baf-b9aa-fe8afde5b1db.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-ab9a93cc-16e5-4e68-948f-7b0ac9cb9522.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-f0b72f65-4984-4927-a11a-d9c8b83f3aa3.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-3f632a54-d725-4357-968c-912866896b2d.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-95f0432c-dbbb-4933-9ee6-1f428774ecf0.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-5272304c-0fb1-4606-9a04-a96670f27744.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-6756f68a-d465-4843-826f-763525777054.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-b47fcd0c-39b7-4734-a2ff-8b1538b3ddb4.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-d3cd2722-7076-4222-9fbf-76cdf5ece3d0.png",
    "/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93/exec-f39b7a24-bac6-4742-af0f-611cf33b2a9e.png",
]
DELAYS = [80, 80, 80, 80, 120, 80, 80, 80, 80, 80, 120, 80, 80, 80, 80, 80, 120, 320]


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


raw_dir = ROOT / "raw-native-1536x1024"
frame_dir = ROOT / "frames-768x512"
raw_dir.mkdir(parents=True, exist_ok=True)
frame_dir.mkdir(parents=True, exist_ok=True)

frames = []
records = []
for index, source in enumerate(SOURCES):
    src = Path(source)
    raw = raw_dir / f"frame-{index:02d}.png"
    shutil.copyfile(src, raw)
    image = Image.open(raw)
    if image.mode != "RGBA" or image.size != (1536, 1024):
        raise RuntimeError(f"physical gate failed: {index} {image.mode} {image.size}")
    alpha = image.getchannel("A")
    if alpha.getextrema()[0] != 0:
        raise RuntimeError(f"no transparency: {index}")
    resized = image.resize((768, 512), Image.Resampling.LANCZOS)
    output = frame_dir / f"frame-{index:02d}.png"
    resized.save(output)
    frames.append(resized)
    a = resized.getchannel("A")
    records.append({
        "frame": index,
        "source_path": source,
        "source_sha256": sha(src),
        "raw_sha256": sha(raw),
        "output_sha256": sha(output),
        "alpha_extrema": list(a.getextrema()),
        "alpha_bbox": list(a.getbbox() or (0, 0, 0, 0)),
    })

gif = ROOT / "C3-like-continuous18-clean-review.gif"
frames[0].save(gif, save_all=True, append_images=frames[1:], duration=DELAYS, loop=0, disposal=2)

thumbs = []
for image in frames:
    bg = Image.new("RGBA", image.size, (36, 42, 50, 255))
    bg.alpha_composite(image)
    thumbs.append(bg.convert("RGB").resize((384, 256), Image.Resampling.LANCZOS))
contact = Image.new("RGB", (384 * 6, 256 * 3), (36, 42, 50))
for index, thumb in enumerate(thumbs):
    contact.paste(thumb, ((index % 6) * 384, (index // 6) * 256))
contact_path = ROOT / "C3-like-contact18-unmarked.png"
contact.save(contact_path)

manifest = {
    "status": "PHYSICAL_PASS_VISUAL_REVIEW_PENDING",
    "method": "built_in_imagegen_sequential_native_rgba_standalone_frames",
    "reference_c3_sha256": "3c1d9186a9aa1abac8b05659533671ea5f8a58b2ace585a8f439849e70723dbf",
    "attempt": 2,
    "attempt1": "REJECTED_FRAME01_RGB_NO_ALPHA",
    "physical_frames": 18,
    "dimensions": [768, 512],
    "duration_ms": sum(DELAYS),
    "delays_ms": DELAYS,
    "segments": {"hit1": [0, 6], "hit2": [6, 12], "hit3": [12, 18]},
    "contacts": [4, 10, 16],
    "loop": "infinite_review_only",
    "resample": "single aspect-preserving 0.5x Lanczos from native 1536x1024",
    "frames": records,
    "gif": {"path": str(gif), "sha256": sha(gif)},
    "contact": {"path": str(contact_path), "sha256": sha(contact_path)},
    "project_install": 0,
}
(ROOT / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
print(json.dumps({"gif": str(gif), "contact": str(contact_path), "manifest": str(ROOT / 'manifest.json')}, indent=2))
