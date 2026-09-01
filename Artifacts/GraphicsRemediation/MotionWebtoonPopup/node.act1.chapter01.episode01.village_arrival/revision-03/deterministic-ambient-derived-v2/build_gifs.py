from pathlib import Path
from PIL import Image, GifImagePlugin
import hashlib,json

root=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/MotionWebtoonPopup/node.act1.chapter01.episode01.village_arrival/revision-03/deterministic-ambient-derived-v2')
frames=[Image.open(root/f'Q{i}-384x512.png').convert('RGB') for i in range(4)]
# One fixed palette derived from immutable Q0; no dithering and no resampling.
pal=frames[0].quantize(colors=256,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE)
q=[f.quantize(palette=pal,dither=Image.Dither.NONE) for f in frames]
runtime_order=[0,0,0,0,0,1,2,3,3,2,1,0,0,0,0,0]
review_order=[0,1,2,3]
def write(name,order):
    seq=[q[i].copy() for i in order];p=root/name
    # Write each slot explicitly: Pillow's high-level saver merges identical adjacent Q0 slots.
    with p.open('wb') as fp:
        header,_=GifImagePlugin.getheader(seq[0],info={'loop':0})
        for block in header: fp.write(block)
        for frame in seq:
            for block in GifImagePlugin.getdata(frame,duration=500,disposal=1): fp.write(block)
        fp.write(b';')
    im=Image.open(p);delays=[]
    for i in range(im.n_frames): im.seek(i);delays.append(im.info.get('duration'))
    return {'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'dimensions':[im.width,im.height],'frames':im.n_frames,'delaysMs':delays,'loop':im.info.get('loop'),'order':order}
m={'authorityManifestSHA256':'e3ef88a5fe270677d3808d844a4554a896405f8737834557f8ed956cad91b144','palette':'fixed Q0 MEDIANCUT256, dithering0','resampling':0,'runtimePreview':write('village-arrival-runtime-preview-16slot-8s.gif',runtime_order),'reviewCycle':write('village-arrival-review-cycle-4slot-2s.gif',review_order)}
p=root/'gif-manifest.json';p.write_text(json.dumps(m,indent=2)+'\n');print(hashlib.sha256(p.read_bytes()).hexdigest())
