from collections import deque
from pathlib import Path
import hashlib, json, sys
import numpy as np
from PIL import Image, ImageFilter, ImageDraw

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()

src=Path(sys.argv[1]); outdir=Path(sys.argv[2]); erosion=int(sys.argv[3]) if len(sys.argv)>3 else 3
outdir.mkdir(parents=True, exist_ok=True)
rgb=np.asarray(Image.open(src).convert('RGB'),dtype=np.uint8)
r,g,b=[rgb[...,i] for i in range(3)]
eligible=(r>=195)&(g>=188)&(b>=178)&((rgb.max(2)-rgb.min(2))<=30)
h,w=eligible.shape; bg=np.zeros((h,w),np.uint8); q=deque()
for x in range(w):
    for y in (0,h-1):
        if eligible[y,x] and not bg[y,x]: bg[y,x]=1;q.append((y,x))
for y in range(h):
    for x in (0,w-1):
        if eligible[y,x] and not bg[y,x]: bg[y,x]=1;q.append((y,x))
while q:
    y,x=q.popleft()
    for yy,xx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
        if 0<=yy<h and 0<=xx<w and eligible[yy,xx] and not bg[yy,xx]: bg[yy,xx]=1;q.append((yy,xx))
hard=(1-bg)*255
mask=Image.fromarray(hard.astype(np.uint8),'L').filter(ImageFilter.MinFilter(erosion)).filter(ImageFilter.GaussianBlur(0.7))
if len(sys.argv)>4 and sys.argv[4] == 'restore_sword':
    # Correction1: restore only the canonical lowered blade corridor. Matte-like pixels
    # remain clear; neutral/darker metal pixels receive luminance-derived alpha.
    corridor=Image.new('L',(w,h),0)
    ImageDraw.Draw(corridor).polygon([(273,823),(311,840),(121,1375),(61,1382)],fill=255)
    cm=np.asarray(corridor,dtype=np.uint8)>0
    lum=rgb.astype(np.float32).mean(2)
    sword=np.clip((216.0-lum)/22.0*255.0,0,255).astype(np.uint8)
    sword[~cm]=0
    sword_im=Image.fromarray(sword,'L').filter(ImageFilter.GaussianBlur(0.45))
    mask=Image.fromarray(np.maximum(np.asarray(mask),np.asarray(sword_im)).astype(np.uint8),'L')
alpha=np.asarray(mask,dtype=np.uint8); a=alpha.astype(np.float32)/255
source=rgb.astype(np.float32)/255
# Approximate the local matte with a smooth resized version of the known background pixels.
# At partial edges only, use the canonical requested matte as a stable decontamination anchor.
matte=np.array([216,210,199],np.float32)/255
fg=source.copy(); partial=(a>0)&(a<1)
rec=(source-matte[None,None,:]*(1-a[...,None]))/np.maximum(a[...,None],1/255)
fg[partial]=np.clip(rec[partial],0,1)
out=np.concatenate([np.rint(fg*255).astype(np.uint8),alpha[...,None]],2); out[alpha==0,:3]=0
op=outdir/'portrait-alpha-r1.png'; mp=outdir/'mask-r1.png'
Image.fromarray(out,'RGBA').save(op,compress_level=9); mask.save(mp,compress_level=9)
ys,xs=np.where(alpha>=16)
m={"source":str(src),"source_sha256":sha(src),"output":str(op),"output_sha256":sha(op),"mask":str(mp),"mask_sha256":sha(mp),"size":[w,h],"bbox_alpha16":[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)],"corner_alpha":[int(alpha[0,0]),int(alpha[0,-1]),int(alpha[-1,0]),int(alpha[-1,-1])],"partial_alpha_pixels":int(((alpha>0)&(alpha<255)).sum()),"transparent_rgb_residue":int(np.any(out[alpha==0,:3]!=0,axis=1).sum()),"opaque_rgb_mismatch":int(np.any(out[alpha==255,:3]!=rgb[alpha==255],axis=1).sum()),"erosion":erosion,"sword_restore":len(sys.argv)>4 and sys.argv[4]=='restore_sword',"rule":"border-connected low-chroma beige matte; partial-edge-only decontamination; alpha0 RGB zero; optional canonical sword-corridor luminance restore"}
(outdir/'metrics-r1.json').write_text(json.dumps(m,ensure_ascii=False,indent=2)+'\n',encoding='utf-8'); print(json.dumps(m,ensure_ascii=False,indent=2))
