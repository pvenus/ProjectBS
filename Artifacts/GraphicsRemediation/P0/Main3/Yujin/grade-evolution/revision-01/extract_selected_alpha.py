from pathlib import Path
import hashlib, json
import numpy as np
from PIL import Image

ROOT = Path(__file__).parent

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def smoothstep(lo, hi, x):
    t = np.clip((x-lo)/(hi-lo), 0.0, 1.0)
    return t*t*(3.0-2.0*t)

def process(src):
    im = Image.open(src).convert('RGB')
    a = np.asarray(im).astype(np.float32)/255.0
    h,w = a.shape[:2]
    side=max(h,w)
    # Design-invariant square normalization: matte-only symmetric padding, no crop/resample.
    border=np.concatenate([a[:12].reshape(-1,3),a[-12:].reshape(-1,3),a[:, :12].reshape(-1,3),a[:, -12:].reshape(-1,3)])
    bg=np.median(border,axis=0)
    canvas=np.empty((side,side,3),np.float32); canvas[:]=bg
    oy=(side-h)//2; ox=(side-w)//2; canvas[oy:oy+h,ox:ox+w]=a
    d=np.sqrt(np.sum((canvas-bg)**2,axis=2))
    vmax=canvas.max(2); vmin=canvas.min(2); sat=(vmax-vmin)/np.maximum(vmax,1e-6)
    lum=canvas@np.array([0.2126,0.7152,0.0722],np.float32)
    # Conservative trimap: matte texture is zero; dark/chromatic brush is foreground;
    # only their immediate color-distance band receives partial alpha.
    alpha=smoothstep(0.055,0.145,d)
    alpha=np.maximum(alpha,smoothstep(0.08,0.22,sat)*smoothstep(0.88,0.55,lum))
    alpha[alpha<0.015]=0
    alpha[alpha>0.985]=1
    # Decontaminate only partial edge pixels against the measured matte.
    out=np.zeros_like(canvas)
    opaque=alpha>=0.999
    partial=(alpha>0)&(~opaque)
    out[opaque]=canvas[opaque]
    aa=alpha[partial,None]
    out[partial]=np.clip((canvas[partial]-(1-aa)*bg)/np.maximum(aa,1/255),0,1)
    rgba=np.dstack([np.rint(out*255).astype(np.uint8),np.rint(alpha*255).astype(np.uint8)])
    target=src.parent.parent/'alpha'
    target.mkdir(exist_ok=True)
    stem=src.parent.parent.name
    op=target/(stem+'.selected-alpha.png')
    mp=target/(stem+'.mask.png')
    Image.fromarray(rgba,'RGBA').save(op,optimize=False,compress_level=9)
    Image.fromarray(rgba[:,:,3],'L').save(mp,optimize=False,compress_level=9)
    ys,xs=np.where(rgba[:,:,3]>=16)
    metrics={
      'source':str(src.relative_to(ROOT)), 'sourceSha256':sha(src),
      'output':str(op.relative_to(ROOT)), 'outputSha256':sha(op),
      'sourceSize':[w,h], 'normalizedSize':[side,side], 'padOffset':[ox,oy],
      'matteRgb':[round(float(x),6) for x in bg],
      'alphaPartial':int(((rgba[:,:,3]>0)&(rgba[:,:,3]<255)).sum()),
      'alphaOpaque':int((rgba[:,:,3]==255).sum()),
      'alphaZeroRgbResidue':int(np.count_nonzero(rgba[rgba[:,:,3]==0,:3])),
      'cornersAlpha':[int(rgba[0,0,3]),int(rgba[0,-1,3]),int(rgba[-1,0,3]),int(rgba[-1,-1,3])],
      'bboxAlpha16':[int(xs.min()),int(ys.min()),int(xs.max()),int(ys.max())] if len(xs) else None
    }
    (target/(stem+'.metrics.json')).write_text(json.dumps(metrics,ensure_ascii=False,indent=2)+'\n')
    return metrics

selected=sorted(ROOT.glob('Y[23]/*/selected/*.png'))
results=[process(p) for p in selected]
(ROOT/'alpha-family-manifest.json').write_text(json.dumps(results,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(results,ensure_ascii=False,indent=2))
