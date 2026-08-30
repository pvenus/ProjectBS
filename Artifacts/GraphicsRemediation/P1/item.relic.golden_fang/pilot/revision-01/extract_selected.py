from pathlib import Path
import hashlib, json
import numpy as np
from PIL import Image, ImageFilter
from collections import deque

ROOT=Path(__file__).resolve().parent
SOURCE=ROOT/"selected/item.relic.golden_fang.icon.selected-C.png"
OUT=ROOT/"alpha-revision-01"
OUT.mkdir(parents=True,exist_ok=True)
OUTPUT=OUT/"item.relic.golden_fang.icon.selected-C-alpha.png"
MASK=OUT/"mask.png"
METRICS=OUT/"metrics.json"
rgb8=np.asarray(Image.open(SOURCE).convert("RGB"),dtype=np.uint8)
rgb=rgb8.astype(np.float32)
h,w=rgb.shape[:2]
mx=rgb.max(axis=2); mn=rgb.min(axis=2)
chroma=mx-mn
luma=.2126*rgb[:,:,0]+.7152*rgb[:,:,1]+.0722*rgb[:,:,2]
# Connected-matte trimap: only pale, low-chroma pixels connected to the canvas
# boundary are background. Pale ivory enclosed by the painted contour remains FG.
bg_candidate=(luma>184)&(chroma<24)
visited=np.zeros((h,w),dtype=bool)
q=deque()
for x in range(w):
    if bg_candidate[0,x]: visited[0,x]=True; q.append((0,x))
    if bg_candidate[h-1,x]: visited[h-1,x]=True; q.append((h-1,x))
for y in range(h):
    if bg_candidate[y,0] and not visited[y,0]: visited[y,0]=True; q.append((y,0))
    if bg_candidate[y,w-1] and not visited[y,w-1]: visited[y,w-1]=True; q.append((y,w-1))
while q:
    y,x=q.popleft()
    for ny,nx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
        if 0<=ny<h and 0<=nx<w and bg_candidate[ny,nx] and not visited[ny,nx]:
            visited[ny,nx]=True; q.append((ny,nx))
binary=(~visited).astype(np.uint8)*255
alpha=np.asarray(Image.fromarray(binary,"L").filter(ImageFilter.GaussianBlur(.65)),dtype=np.uint8).copy()
alpha[alpha<4]=0

# Selected source matte is a pale neutral; edge RGB is locally extrapolated from
# the raw pixel and darkened only where alpha is partial to prevent a light halo.
matte=np.array([216.,213.,204.],dtype=np.float32)
af=alpha.astype(np.float32)/255
out=rgb.copy(); partial=(alpha>0)&(alpha<255)
safe=np.maximum(af[partial,None],1/255)
fg=matte+(rgb[partial]-matte)/safe
out[partial]=np.minimum(np.clip(fg,0,255),rgb[partial]*.60)
out[alpha==0]=0
rgba=np.dstack([out.astype(np.uint8),alpha])
Image.fromarray(alpha,"L").save(MASK)
Image.fromarray(rgba,"RGBA").save(OUTPUT)
vis=alpha>=16; ys,xs=np.where(vis)
bbox=[int(xs.min()),int(ys.min()),int(xs.max())+1,int(ys.max())+1]
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
m={"assetId":"item.relic.golden_fang","selected":"C","size":[w,h],"source":str(SOURCE),"output":str(OUTPUT),"cornersAlpha":[int(alpha[0,0]),int(alpha[0,-1]),int(alpha[-1,0]),int(alpha[-1,-1])],"alphaZeroRgbResidue":int(np.count_nonzero(rgba[:,:,:3][alpha==0])),"alphaPartial":int(np.count_nonzero((alpha>0)&(alpha<255))),"bboxAlpha16":bbox,"bboxPercent":[round((bbox[2]-bbox[0])*100/w,3),round((bbox[3]-bbox[1])*100/h,3)],"opaqueRgbMismatch":int(np.count_nonzero(out[alpha==255].astype(np.uint8)!=rgb8[alpha==255])),"canonicalWrite":False,"metaWrite":False,"unity":False,"staging":False}
m.update(sourceSha256=sha(SOURCE),outputSha256=sha(OUTPUT),maskSha256=sha(MASK))
METRICS.write_text(json.dumps(m,ensure_ascii=False,indent=2)+"\n")
m["metricsSha256"]=sha(METRICS)
print(json.dumps(m,ensure_ascii=False,indent=2))
