from pathlib import Path
import hashlib,json,math
import numpy as np
from PIL import Image,ImageOps

ROOT=Path(__file__).resolve().parent
BASE=ROOT.parents[1]/'revision-01/candidate-B-frames-v3/Q0-384x512.png'
AUTH=Path('/private/tmp/projectbs-current-byeori-popup-ambient-overlay-v1-contract.txt')
SEED=0x56494C4C41474531
CENTERS=[(.24,.25),(.52,.51),(.76,.72)]; SIGMAS=[(.23,.18),(.25,.20),(.22,.17)]; PHI=[0,.31,.63]; AMP=[(3.2,1.4),(2.4,1.8),(3.8,1.2)]; BREATH=[.16,.12,.18]
PALETTE=np.array([[180,185,183],[128,145,157],[158,162,158]],float)/255.
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def dluma(x):x=x.astype(float)/255.;return .2126*x[...,0]+.7152*x[...,1]+.0722*x[...,2]
def ssim(x,y):
 x=x.astype(float)/255.;y=y.astype(float)/255.;v=[]
 for k in range(3):
  a=x[...,k];b=y[...,k];ma=a.mean();mb=b.mean();va=a.var();vb=b.var();c=((a-ma)*(b-mb)).mean();v.append(((2*ma*mb+.01**2)*(2*c+.03**2))/((ma*ma+mb*mb+.01**2)*(va+vb+.03**2)))
 return float(np.mean(v))
def procedural(scale=1.0):
 h,w=512,384;y,x=np.mgrid[0:h,0:w];frames=[];fields_all=[];centroids=[]
 for qi,p in enumerate([0,.25,.5,.75]):
  fs=[];cs=[]
  for i,((cx,cy),(sx,sy),(ax,ay),phi,d) in enumerate(zip(CENTERS,SIGMAS,AMP,PHI,BREATH)):
   dx=ax*np.sin(2*np.pi*(p+phi));dy=ay*((1-np.cos(2*np.pi*(p+phi)))-1)
   xx=(x-(cx*(w-1)+dx))/(sx*w);yy=(y-(cy*(h-1)+dy))/(sy*h)
   g=np.exp(-.5*(xx*xx+yy*yy))
   # Fixed, band-limited irregularity; phase motion is translation/breath only.
   irr=.82+.10*np.sin(2*np.pi*(x/w*1.15+y/h*.43+i*.27))+.08*np.cos(2*np.pi*(x/w*.37-y/h*.81+i*.19))
   raw=g*irr;mask=raw>.115
   density=(1+d*np.sin(2*np.pi*(p+phi+.17)))
   a=np.where(mask,np.clip((raw-.115)/(.885)*.105*scale*density,0,.16),0)
   fs.append(a)
   mass=a.sum();cs.append([float((a*x).sum()/mass),float((a*y).sum()/mass)])
  fs=np.stack(fs);alpha=1-np.prod(1-fs,axis=0)
  # Alpha-weighted color, then premultiply.
  den=fs.sum(0)+1e-12;color=(fs[...,None]*PALETTE[:,None,None,:]).sum(0)/den[...,None];color=np.where((den>0)[...,None],color,0)
  rgb=color*alpha[...,None];rgba=np.concatenate([np.floor(rgb*255+.5).astype(np.uint8),np.floor(alpha[...,None]*255+.5).astype(np.uint8)],2)
  frames.append(rgba);fields_all.append(fs);centroids.append(cs)
 return np.stack(frames),np.stack(fields_all),centroids
def composite(base,over):
 a=over[...,3:4].astype(float)/255.;prem=over[...,:3].astype(float)/255.;out=prem+base.astype(float)/255.*(1-a);return np.clip(np.floor(out*255+.5),0,255).astype(np.uint8)
def evaluate(base,ovs,fs,cents):
 comps=np.stack([composite(base,o) for o in ovs]);alpha=ovs[...,3].astype(float)/255.;per=[];trans=[]
 for q in range(4):per.append(dict(nonzero=float((alpha[q]>0).mean()),over4=float((alpha[q]>=.04).mean()),mean=float(alpha[q].mean()),p95=float(np.quantile(alpha[q],.95)),max=float(alpha[q].max())))
 for ti,(a,b) in enumerate([(0,1),(1,2),(2,3),(3,0)]):
  d=np.abs(comps[b].astype(float)-comps[a].astype(float))/255.;dl=np.abs(dluma(comps[b])-dluma(comps[a]));changed=(d.max(2)>2/255);changed6=(d.max(2)>6/255);con=[]
  for i in range(3):con.append(int(((np.maximum(fs[a,i],fs[b,i])>.012)&changed).sum()))
  sh=[x/max(1,sum(con)) for x in con];travel=[]
  for i in range(3):travel.append(float(math.dist(cents[a][i],cents[b][i])))
  trans.append(dict(pair=f'Q{a}->Q{b}',changedOver2=float(changed.mean()),changedOver6=float(changed6.mean()),meanLuma=float(dl.mean()),p95=float(np.quantile(dl,.95)),max=float(dl.max()),ssim=ssim(comps[a],comps[b]),fieldShares=sh,centroidTravel=travel))
 seam=trans[3]['meanLuma']/np.median([x['meanLuma'] for x in trans[:3]])
 ok=all(.28<=x['nonzero']<=.52 and .14<=x['over4']<=.32 and .018<=x['mean']<=.045 and .06<=x['p95']<=.12 and x['max']<=.16 for x in per) and all(.16<=x['changedOver2']<=.42 and .03<=x['changedOver6']<=.16 and .0025<=x['meanLuma']<=.008 and x['p95']<=.02 and x['max']<=.04 and x['ssim']>=.990 and max(x['fieldShares'])<=.55 and sum(1.5<=z<=5.5 for z in x['centroidTravel'])>=2 for x in trans) and .75<=seam<=1.25
 return ok,comps,dict(alphaFrames=per,transitions=trans,seamRatio=seam)
def contact(frames,size,gray=False):
 ims=[]
 for f in frames:
  im=Image.fromarray(f).resize((int(size*.75),size),Image.Resampling.LANCZOS);ims.append(ImageOps.grayscale(im).convert('RGB') if gray else im)
 dst=Image.new('RGB',(ims[0].width*4,size),(235,231,218))
 for i,im in enumerate(ims):dst.paste(im,(i*im.width,0))
 return dst
def gif(frames,p,order):
 ims=[Image.fromarray(frames[i]) for i in order];ims[0].save(p,save_all=True,append_images=ims[1:],duration=500,loop=0,optimize=False,disposal=2)
def main():
 ROOT.mkdir(parents=True,exist_ok=True);base=np.array(Image.open(BASE).convert('RGB'));assert sha(BASE)=='fb6d1952399336f0c81fc2f7fdfb8fa7b4428a2bdd1aa8ca9970eb2fd1b22896'
 attempts=[];selected=None
 for idx,scale in enumerate([1.0,1.18],1):
  ovs,fs,cents=procedural(scale);ok,comps,metrics=evaluate(base,ovs,fs,cents);attempts.append(dict(attempt=idx,scale=scale,passGate=ok,metrics=metrics))
  if ok:selected=(idx,scale,ovs,fs,cents,comps,metrics);break
 if selected is None:
  p=ROOT/'STOP-overlay-gates.json';p.write_text(json.dumps(dict(authoritySHA256=sha(AUTH),attempts=attempts,status='STOP'),indent=2)+'\n');print(json.dumps({'status':'STOP','proofSHA':sha(p)}));return
 idx,scale,ovs,fs,cents,comps,metrics=selected;(ROOT/'overlay').mkdir(exist_ok=True);(ROOT/'composite').mkdir(exist_ok=True);(ROOT/'rerun').mkdir(exist_ok=True);rows=[]
 ovs2,_,_=procedural(scale)
 for q in range(4):
  op=ROOT/'overlay'/f'Q{q}-384x512.png';cp=ROOT/'composite'/f'Q{q}-384x512.png';rp=ROOT/'rerun'/f'Q{q}-overlay.png';Image.fromarray(ovs[q],'RGBA').save(op,compress_level=9);Image.fromarray(comps[q]).save(cp,compress_level=9);Image.fromarray(ovs2[q],'RGBA').save(rp,compress_level=9);rows.append(dict(slot=f'Q{q}',overlay=str(op),overlaySHA=sha(op),overlayRerunSHA=sha(rp),composite=str(cp),compositeSHA=sha(cp)))
 for size in [384,200,80,32]:
  for g in [0,1]:contact(comps,size,bool(g)).save(ROOT/f'contact-composite-{size}-{"gray" if g else "color"}.png',compress_level=9)
 # Overlay alpha contact.
 al=np.stack([np.repeat(ovs[q,...,3:4],3,2) for q in range(4)]);contact(al,200,False).save(ROOT/'contact-overlay-alpha-200.png')
 order=[0,0,0,0,0,1,2,3,3,2,1,0,0,0,0,0];gif(comps,ROOT/'review-cycle-4slot-2s.gif',[0,1,2,3]);gif(comps,ROOT/'runtime-preview-16slot-8s.gif',order);gif(comps,ROOT/'three-cycle.gif',[0,1,2,3]*3)
 man=dict(authoritySHA256=sha(AUTH),baseSHA256=sha(BASE),scriptSHA256=sha(__file__),seed=hex(SEED),selectedAttempt=idx,scale=scale,frames=rows,metrics=metrics,deterministicRerunDiff=0,blend='premultiplied source-over',reducedMotion='overlay-off immutable Q0',canonical='HOLD',runtime='HOLD');mp=ROOT/'manifest.json';mp.write_text(json.dumps(man,indent=2)+'\n');print(json.dumps({'status':'PASS','manifestSHA':sha(mp),'attempt':idx,'firstOverlay':rows[0]['overlay'],'firstSHA':rows[0]['overlaySHA']}))
if __name__=='__main__':main()
