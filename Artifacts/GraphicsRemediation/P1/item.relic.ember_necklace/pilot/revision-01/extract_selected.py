from pathlib import Path
import hashlib, json
import numpy as np
from PIL import Image

ROOT = Path(__file__).parent
SOURCE = ROOT / "selected/item.relic.ember_necklace.icon.selected-C.png"
OUT = ROOT / "alpha-revision-01"

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def smoothstep(lo, hi, v):
    t = np.clip((v-lo)/(hi-lo), 0, 1)
    return t*t*(3-2*t)

rgb = np.asarray(Image.open(SOURCE).convert("RGB")).astype(np.float32)/255
h,w = rgb.shape[:2]
border = np.concatenate([rgb[:12].reshape(-1,3),rgb[-12:].reshape(-1,3),rgb[:,:12].reshape(-1,3),rgb[:,-12:].reshape(-1,3)])
matte = np.median(border, axis=0)
distance = np.sqrt(np.sum((rgb-matte)**2, axis=2))
alpha = smoothstep(0.055, 0.16, distance)
alpha[alpha < 0.015] = 0; alpha[alpha > 0.985] = 1
out_rgb = np.zeros_like(rgb)
opaque = alpha >= 0.999; partial = (alpha > 0) & ~opaque
out_rgb[opaque] = rgb[opaque]
aa = alpha[partial,None]
out_rgb[partial] = np.clip((rgb[partial]-(1-aa)*matte)/np.maximum(aa,1/255),0,1)
# Correction1: suppress the residual light matte only on partial-alpha edge
# pixels. Opaque artwork RGB and geometry are untouched.
out_rgb[partial] = np.minimum(out_rgb[partial], rgb[partial] * 0.72)
rgba=np.dstack([np.rint(out_rgb*255).astype(np.uint8),np.rint(alpha*255).astype(np.uint8)])
rgba[rgba[:,:,3]==0,:3]=0
OUT.mkdir(parents=True,exist_ok=True)
output=OUT/"item.relic.ember_necklace.icon.selected-C-alpha.png"
mask=OUT/"mask.png"
Image.fromarray(rgba,"RGBA").save(output,optimize=False,compress_level=9)
Image.fromarray(rgba[:,:,3],"L").save(mask,optimize=False,compress_level=9)
ys,xs=np.where(rgba[:,:,3]>=16); bbox=[int(xs.min()),int(ys.min()),int(xs.max()),int(ys.max())]
metrics={
  "assetId":"item.relic.ember_necklace","source":str(SOURCE),"sourceSha256":sha(SOURCE),
  "output":str(output),"outputSha256":sha(output),"mask":str(mask),"maskSha256":sha(mask),
  "size":[w,h],"matteRgb":[round(float(v),6) for v in matte],
  "cornersAlpha":[int(rgba[0,0,3]),int(rgba[0,-1,3]),int(rgba[-1,0,3]),int(rgba[-1,-1,3])],
  "alphaZeroRgbResidue":int(np.count_nonzero(rgba[rgba[:,:,3]==0,:3])),
  "alphaPartial":int(((rgba[:,:,3]>0)&(rgba[:,:,3]<255)).sum()),
  "bboxAlpha16":bbox,"bboxPercent":[round((bbox[2]-bbox[0]+1)/w*100,3),round((bbox[3]-bbox[1]+1)/h*100,3)],
  "canonicalWrite":False,"metaWrite":False,"unity":False,"staging":False
}
mp=OUT/"metrics.json"; mp.write_text(json.dumps(metrics,indent=2)+"\n")
print(json.dumps({**metrics,"metricsSha256":sha(mp)},indent=2))
