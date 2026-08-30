from pathlib import Path
import hashlib, json
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "selected/item.relic.frozen_nail.icon.selected-C.png"
OUT_DIR = ROOT / "alpha-revision-01"
OUT_DIR.mkdir(parents=True, exist_ok=True)
OUTPUT = OUT_DIR / "item.relic.frozen_nail.icon.selected-C-alpha.png"
MASK = OUT_DIR / "mask.png"
METRICS = OUT_DIR / "metrics.json"

rgb8 = np.asarray(Image.open(SOURCE).convert("RGB"), dtype=np.uint8)
rgb = rgb8.astype(np.float32)
h, w = rgb.shape[:2]
yy, xx = np.mgrid[0:h, 0:w]
xn = xx/(w-1)*2-1
yn = yy/(h-1)*2-1
F = np.stack([np.ones_like(xn),xn,yn,xn*xn,yn*yn,xn*yn], axis=2)
border = (xx<int(w*.12))|(xx>=int(w*.88))|(yy<int(h*.12))|(yy>=int(h*.88))
sample = border & ((xx%4)==0) & ((yy%4)==0)
X = F[sample]
bg = np.empty_like(rgb)
for c in range(3):
    coef,*_ = np.linalg.lstsq(X, rgb[:,:,c][sample], rcond=None)
    bg[:,:,c] = np.tensordot(F, coef, axes=([2],[0]))

delta = np.sqrt(np.sum((rgb-bg)**2, axis=2))
luma = .2126*rgb[:,:,0]+.7152*rgb[:,:,1]+.0722*rgb[:,:,2]
blue = np.maximum(rgb[:,:,2]-rgb[:,:,0], rgb[:,:,2]-rgb[:,:,1])
a_dark = np.clip((195-luma)*8,0,255)
a_frost = np.clip((blue-2)*24,0,255)
chroma = rgb.max(axis=2)-rgb.min(axis=2)
a_chroma = np.clip((chroma-15)*12,0,255)
alpha = np.maximum.reduce([a_dark,a_frost,a_chroma]).astype(np.uint8)

roi_im = Image.new("L",(w,h),0)
d = ImageDraw.Draw(roi_im)
d.polygon([(280,205),(385,150),(650,105),(790,155),(855,270),(835,390),(755,455),(795,860),(770,1090),(700,1175),(600,1180),(535,1090),(505,885),(465,620),(425,470),(335,420),(285,330)],fill=255)
roi = np.asarray(roi_im)>0
alpha[~roi]=0
alpha[alpha<5]=0

af=alpha.astype(np.float32)/255
out=rgb.copy()
partial=(alpha>0)&(alpha<255)
safe=np.maximum(af[partial,None],1/255)
fg=bg[partial]+(rgb[partial]-bg[partial])/safe
out[partial]=np.minimum(np.clip(fg,0,255),rgb[partial]*.62)
out[alpha==0]=0
rgba=np.dstack([out.astype(np.uint8),alpha])
Image.fromarray(alpha,"L").save(MASK)
Image.fromarray(rgba,"RGBA").save(OUTPUT)

vis=alpha>=16
ys,xs=np.where(vis)
bbox=[int(xs.min()),int(ys.min()),int(xs.max())+1,int(ys.max())+1]
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
m={
 "assetId":"item.relic.frozen_nail","selected":"C","size":[w,h],
 "source":str(SOURCE),"output":str(OUTPUT),
 "cornersAlpha":[int(alpha[0,0]),int(alpha[0,-1]),int(alpha[-1,0]),int(alpha[-1,-1])],
 "alphaZeroRgbResidue":int(np.count_nonzero(rgba[:,:,:3][alpha==0])),
 "alphaPartial":int(np.count_nonzero((alpha>0)&(alpha<255))),
 "bboxAlpha16":bbox,"bboxPercent":[round((bbox[2]-bbox[0])*100/w,3),round((bbox[3]-bbox[1])*100/h,3)],
 "opaqueRgbMismatch":int(np.count_nonzero(out[alpha==255].astype(np.uint8)!=rgb8[alpha==255])),
 "canonicalWrite":False,"metaWrite":False,"unity":False,"staging":False,
}
m.update(sourceSha256=sha(SOURCE),outputSha256=sha(OUTPUT),maskSha256=sha(MASK))
METRICS.write_text(json.dumps(m,ensure_ascii=False,indent=2)+"\n")
m["metricsSha256"]=sha(METRICS)
print(json.dumps(m,ensure_ascii=False,indent=2))
