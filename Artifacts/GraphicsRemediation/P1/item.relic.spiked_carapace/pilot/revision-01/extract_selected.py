from pathlib import Path
from collections import deque
import hashlib, json
import numpy as np
from PIL import Image, ImageFilter

ROOT=Path(__file__).resolve().parent
SOURCE=ROOT/"selected/item.relic.spiked_carapace.icon.selected-B.png"
OUT=ROOT/"alpha-revision-01"; OUT.mkdir(parents=True,exist_ok=True)
OUTPUT=OUT/"item.relic.spiked_carapace.icon.selected-B-alpha.png"
MASK=OUT/"mask.png"; METRICS=OUT/"metrics.json"
rgb8=np.asarray(Image.open(SOURCE).convert("RGB"),dtype=np.uint8)
rgb=rgb8.astype(np.float32); h,w=rgb.shape[:2]
mx=rgb.max(axis=2); mn=rgb.min(axis=2); chroma=mx-mn
luma=.2126*rgb[:,:,0]+.7152*rgb[:,:,1]+.0722*rgb[:,:,2]
bg_candidate=(luma>184)&(chroma<24)
visited=np.zeros((h,w),dtype=bool); q=deque()
for x in range(w):
    if bg_candidate[0,x]: visited[0,x]=1; q.append((0,x))
    if bg_candidate[h-1,x]: visited[h-1,x]=1; q.append((h-1,x))
for y in range(h):
    if bg_candidate[y,0] and not visited[y,0]: visited[y,0]=1; q.append((y,0))
    if bg_candidate[y,w-1] and not visited[y,w-1]: visited[y,w-1]=1; q.append((y,w-1))
while q:
    y,x=q.popleft()
    for ny,nx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
        if 0<=ny<h and 0<=nx<w and bg_candidate[ny,nx] and not visited[ny,nx]:
            visited[ny,nx]=1; q.append((ny,nx))
binary=(~visited).astype(np.uint8)*255
alpha=np.asarray(Image.fromarray(binary,"L").filter(ImageFilter.GaussianBlur(.65)),dtype=np.uint8).copy()
alpha[alpha<4]=0
matte=np.array([216.,213.,204.],dtype=np.float32)
af=alpha.astype(np.float32)/255; out=rgb.copy(); partial=(alpha>0)&(alpha<255)
safe=np.maximum(af[partial,None],1/255)
fg=matte+(rgb[partial]-matte)/safe
out[partial]=np.minimum(np.clip(fg,0,255),rgb[partial]*.60)
out[alpha==0]=0
rgba=np.dstack([out.astype(np.uint8),alpha])
Image.fromarray(alpha,"L").save(MASK); Image.fromarray(rgba,"RGBA").save(OUTPUT)
vis=alpha>=16; ys,xs=np.where(vis); bbox=[int(xs.min()),int(ys.min()),int(xs.max())+1,int(ys.max())+1]
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
m={"assetId":"item.relic.spiked_carapace","selected":"B","size":[w,h],"source":str(SOURCE),"output":str(OUTPUT),"cornersAlpha":[int(alpha[0,0]),int(alpha[0,-1]),int(alpha[-1,0]),int(alpha[-1,-1])],"alphaZeroRgbResidue":int(np.count_nonzero(rgba[:,:,:3][alpha==0])),"alphaPartial":int(np.count_nonzero((alpha>0)&(alpha<255))),"bboxAlpha16":bbox,"bboxPercent":[round((bbox[2]-bbox[0])*100/w,3),round((bbox[3]-bbox[1])*100/h,3)],"opaqueRgbMismatch":int(np.count_nonzero(out[alpha==255].astype(np.uint8)!=rgb8[alpha==255])),"canonicalWrite":False,"metaWrite":False,"unity":False,"staging":False}
m.update(sourceSha256=sha(SOURCE),outputSha256=sha(OUTPUT),maskSha256=sha(MASK)); METRICS.write_text(json.dumps(m,ensure_ascii=False,indent=2)+"\n"); m["metricsSha256"]=sha(METRICS)
print(json.dumps(m,ensure_ascii=False,indent=2))
