from pathlib import Path
from collections import deque
import hashlib, json
import numpy as np
from PIL import Image, ImageFilter

ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/"selected/item.relic.ember_necklace.icon.selected-C.png"
OUT=Path(__file__).resolve().parent
TRIMAP=OUT/"manual-trimap.png"
PROVENANCE=OUT/"manual-trimap-provenance.json"

rgb=np.asarray(Image.open(SOURCE).convert("RGB"),dtype=np.uint8)
f=rgb.astype(np.float32); h,w=f.shape[:2]
mx=f.max(axis=2); mn=f.min(axis=2); chroma=mx-mn
luma=.2126*f[:,:,0]+.7152*f[:,:,1]+.0722*f[:,:,2]

# Manual matte authority: pale low-chroma canvas connected to the outer canvas is
# definite background. The closed rope loop interior is explicitly seeded as BG.
bg_candidate=(luma>178)&(chroma<30)
visited=np.zeros((h,w),dtype=bool); q=deque()
def seed(y,x):
    if 0<=y<h and 0<=x<w and bg_candidate[y,x] and not visited[y,x]:
        visited[y,x]=1; q.append((y,x))
for x in range(w): seed(0,x); seed(h-1,x)
for y in range(h): seed(y,0); seed(y,w-1)
seed(500,650)  # canvas visible inside the closed rope loop
while q:
    y,x=q.popleft()
    for ny,nx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
        if 0<=ny<h and 0<=nx<w and bg_candidate[ny,nx] and not visited[ny,nx]:
            visited[ny,nx]=1; q.append((ny,nx))

fg=(~visited).astype(np.uint8)*255
fg_im=Image.fromarray(fg,"L")
def_fg=np.asarray(fg_im.filter(ImageFilter.MinFilter(5)),dtype=np.uint8)==255
possible_fg=np.asarray(fg_im.filter(ImageFilter.MaxFilter(5)),dtype=np.uint8)>0
trimap=np.full((h,w),128,dtype=np.uint8)
trimap[~possible_fg]=0
trimap[def_fg]=255
Image.fromarray(trimap,"L").save(TRIMAP)

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
record={
  "assetId":"item.relic.ember_necklace",
  "authority":"selected-C",
  "source":str(SOURCE),"sourceSha256":sha(SOURCE),
  "trimap":str(TRIMAP),"trimapSha256":sha(TRIMAP),
  "dimensions":[w,h],"format":"PNG grayscale L8",
  "values":{"definiteBackground":0,"unknown":128,"definiteForeground":255},
  "unknownBand":"2px nominal (contract range 1–3px)",
  "manualSeeds":[{"x":650,"y":500,"purpose":"closed rope-loop interior background"}],
  "geometryOrRgbChange":False,"globalDistanceMaskReuse":False,"thresholdRelaxation":False,
  "canonicalWrite":False,"metaWrite":False,"unity":False,"staging":False,
}
PROVENANCE.write_text(json.dumps(record,ensure_ascii=False,indent=2)+"\n")
record["provenanceSha256"]=sha(PROVENANCE)
print(json.dumps(record,ensure_ascii=False,indent=2))
