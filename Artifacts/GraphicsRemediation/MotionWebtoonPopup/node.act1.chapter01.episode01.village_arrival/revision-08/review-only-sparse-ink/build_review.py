from pathlib import Path
import hashlib,json,math
import numpy as np
from PIL import Image,ImageOps

ROOT=Path(__file__).resolve().parent
BASE=ROOT.parents[1]/'revision-01/candidate-B-frames-v3/Q0-384x512.png'
SEED=0x56494C4C41474531
CENTERS=[(.24,.25),(.52,.51),(.76,.72)];SIGMAS=[(.23,.18),(.25,.20),(.22,.17)];PHI=[0,.31,.63];AMP=[(3.2,1.4),(2.4,1.8),(3.8,1.2)];BREATH=[.16,.12,.18]
PALETTE=np.array([[78,91,98],[67,83,96],[88,91,92]],float)/255.
AUTH=Path('/private/tmp/projectbs-current-byeori-popup-overlay-review-only-authority.txt')
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def make():
 h,w=512,384;y,x=np.mgrid[0:h,0:w];out=[]
 for p in [0,.25,.5,.75]:
  raws=[]
  for i,((cx,cy),(sx,sy),(ax,ay),phi,d) in enumerate(zip(CENTERS,SIGMAS,AMP,PHI,BREATH)):
   dx=ax*np.sin(2*np.pi*(p+phi));dy=ay*((1-np.cos(2*np.pi*(p+phi)))-1)
   xx=(x-(cx*(w-1)+dx))/(sx*w);yy=(y-(cy*(h-1)+dy))/(sy*h);g=np.exp(-.5*(xx*xx+yy*yy))
   irr=.82+.10*np.sin(2*np.pi*(x/w*1.15+y/h*.43+i*.27))+.08*np.cos(2*np.pi*(x/w*.37-y/h*.81+i*.19));raw=g*irr
   density=1+d*np.sin(2*np.pi*(p+phi+.17));raws.append(raw*density)
  raws=np.stack(raws);combined=1-np.prod(1-np.clip(raws,0,.98),axis=0);q60,q92=np.quantile(combined,[.60,.92]);s=np.clip((combined-q60)/(q92-q60),0,1);s=s*s*(3-2*s);a=.18*np.power(s,.85)
  weights=raws/(raws.sum(0)+1e-12);color=(weights[...,None]*PALETTE[:,None,None,:]).sum(0);color=np.where((a>0)[...,None],color,0);prem=color*a[...,None]
  out.append(np.concatenate([np.floor(prem*255+.5).astype(np.uint8),np.floor(a[...,None]*255+.5).astype(np.uint8)],2))
 return np.stack(out)
def comp(base,o):a=o[...,3:4].astype(float)/255.;return np.clip(np.floor((o[...,:3].astype(float)/255.+base.astype(float)/255.*(1-a))*255+.5),0,255).astype(np.uint8)
def main():
 ROOT.mkdir(parents=True,exist_ok=True);(ROOT/'overlay').mkdir(exist_ok=True);(ROOT/'composite').mkdir(exist_ok=True)
 base=np.array(Image.open(BASE).convert('RGB'));assert sha(BASE)=='fb6d1952399336f0c81fc2f7fdfb8fa7b4428a2bdd1aa8ca9970eb2fd1b22896';ovs=make();comps=np.stack([comp(base,o) for o in ovs]);rows=[]
 for q in range(4):
  op=ROOT/'overlay'/f'Q{q}.png';cp=ROOT/'composite'/f'Q{q}.png';Image.fromarray(ovs[q],'RGBA').save(op,compress_level=9);Image.fromarray(comps[q]).save(cp,compress_level=9);rows.append(dict(slot=f'Q{q}',overlay=str(op),overlaySHA256=sha(op),composite=str(cp),compositeSHA256=sha(cp),nonzeroAlphaCoverage=float((ovs[q,...,3]>0).mean()),meanAlpha=float((ovs[q,...,3]/255.).mean()),maxAlpha=float((ovs[q,...,3]/255.).max())))
 # Mechanical contact sheet.
 sheet=Image.new('RGB',(384*4,512));
 for q in range(4):sheet.paste(Image.fromarray(comps[q]),(384*q,0))
 sp=ROOT/'contact-exact4.png';sheet.save(sp,compress_level=9)
 ims=[Image.fromarray(x) for x in comps];gp=ROOT/'village-arrival-sparse-ink-review-2s.gif';ims[0].save(gp,save_all=True,append_images=ims[1:],duration=500,loop=0,optimize=False,disposal=2)
 order=[0,0,0,0,0,1,2,3,3,2,1,0,0,0,0,0];rt=[ims[i] for i in order];rp=ROOT/'village-arrival-sparse-ink-runtime-preview-8s.gif';rt[0].save(rp,save_all=True,append_images=rt[1:],duration=500,loop=0,optimize=False,disposal=2)
 man=dict(status='REVIEW_ONLY_NOT_ACCEPTED',installAuthority=False,authoritySHA256=sha(AUTH),basePath=str(BASE),baseSHA256=sha(BASE),scriptSHA256=sha(__file__),seed=hex(SEED),formula='v1 fields/phases; per-frame combined raw alpha q60/q92 smoothstep; alpha=.18*pow(S,.85)',blend='premultiplied source-over',frames=rows,contact=dict(path=str(sp),sha256=sha(sp)),reviewGif=dict(path=str(gp),sha256=sha(gp),dimensions=[384,512],frames=4,delayMs=500,cycleSeconds=2,loop='infinite'),runtimeGif=dict(path=str(rp),sha256=sha(rp),dimensions=[384,512],frames=16,delayMs=500,cycleSeconds=8,loop='infinite',order=order),projectCanonicalUnityWrite=0)
 mp=ROOT/'manifest.json';mp.write_text(json.dumps(man,indent=2)+'\n');print(json.dumps({'status':man['status'],'reviewGif':str(gp),'reviewGifSHA':sha(gp),'runtimeGif':str(rp),'runtimeGifSHA':sha(rp),'manifestSHA':sha(mp),'firstOverlay':rows[0]['overlay'],'firstSHA':rows[0]['overlaySHA256'],'coverage':[r['nonzeroAlphaCoverage'] for r in rows]}))
if __name__=='__main__':main()
