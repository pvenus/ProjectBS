from pathlib import Path
import hashlib,json
import numpy as np
from PIL import Image,ImageOps

ROOT=Path(__file__).resolve().parent
BASE=ROOT.parents[1]/'revision-01/candidate-B-frames-v3/Q0-384x512.png'
AUTH=Path('/private/tmp/projectbs-current-byeori-village-arrival-ambient-v5-source-luma-contract.txt')
PH=np.array([[0,0,0],[1,.45,-.30],[.25,1,.45],[-.35,.30,1]],float)
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def s2l(x):x=x/255.;return np.where(x<=.04045,x/12.92,((x+.055)/1.055)**2.4)
def l2s(x):x=np.clip(x,0,1);y=np.where(x<=.0031308,12.92*x,1.055*x**(1/2.4)-.055);return np.clip(np.floor(y*255+.5),0,255).astype(np.uint8)
def dluma(rgb):x=rgb.astype(float)/255.;return .2126*x[...,0]+.7152*x[...,1]+.0722*x[...,2]
def smooth(x):x=np.clip(x,0,1);return x*x*(3-2*x)
def fields(h,w):
 y,x=np.mgrid[0:h,0:w];u=x/(w-1);v=y/(h-1);rs=[]
 for cx,cy,sx,sy in [(.24,.24,.26,.22),(.52,.50,.30,.25),(.78,.74,.24,.22)]:
  r=np.exp(-.5*(((u-cx)/sx)**2+((v-cy)/sy)**2));rs.append(r/r.max())
 return np.stack(rs)
def basis(rs):
 z=np.stack([(PH[q,:,None,None]*rs).sum(0)/(rs.sum(0)+1e-6) for q in range(4)]);W=smooth((np.max(rs,axis=0)-.15)/.5);b0=[];b1=[];quant=[]
 for q in range(4):
  a=np.abs(z[q]);lo=float(np.quantile(a,.72));hi=float(np.quantile(a,.88));e=np.zeros_like(a) if hi<=lo else smooth((a-lo)/(hi-lo));sq=np.tanh(4*z[q]);b0.append(sq*(.75+.25*W));b1.append(sq*e);quant.append([lo,hi])
 return np.stack(b0),np.stack(b1),quant
def global_ssim_luma(x,y):
 mx=x.mean();my=y.mean();vx=x.var();vy=y.var();cv=((x-mx)*(y-my)).mean();return float(((2*mx*my+.01**2)*(2*cv+.03**2))/((mx*mx+my*my+.01**2)*(vx+vy+.03**2)))
def metrics_from_luma(ls,base_y,rs):
 out=[]
 bins=[base_y<.15,(base_y>=.15)&(base_y<=.75),base_y>.75]
 for a,b in [(0,1),(1,2),(2,3),(3,0)]:
  d=np.abs(ls[b]-ls[a]);changed=d>.01;regs=[];shares=[]
  for r in rs:
   mask=r>=.35;regs.append(float(d[mask].mean()));shares.append(int((changed&mask).sum()))
  den=max(1,sum(shares));shares=[x/den for x in shares]
  bc=[int((changed&m).sum()) for m in bins];bd=max(1,sum(bc));bc=[x/bd for x in bc]
  out.append(dict(mean=float(d.mean()),coverage=float(changed.mean()),p95=float(np.quantile(d,.95)),p99=float(np.quantile(d,.99)),max=float(d.max()),ssim=global_ssim_luma(ls[a],ls[b]),regions=regs,regionShares=shares,shadowMean=float(d[bins[0]].mean()),shadowP99=float(np.quantile(d[bins[0]],.99)),midtoneMean=float(d[bins[1]].mean()),midtoneP99=float(np.quantile(d[bins[1]],.99)),highlightMean=float(d[bins[2]].mean()) if bins[2].any() else 0,binShares=bc))
 return out
def robust(ms):
 seam=ms[3]['mean']/np.median([x['mean'] for x in ms[:3]]);peaks=np.argmax(np.array([x['regions'] for x in ms]),axis=0)
 ok=all(.0065<=x['mean']<=.011 and .14<=x['coverage']<=.26 and x['max']<=.045 and x['p95']<=.0225 and x['p99']<=.032 and x['ssim']>=.987 and x['shadowMean']<=.009 and x['shadowP99']<=.025 and .006<=x['midtoneMean']<=.013 and x['midtoneP99']<=.035 and x['highlightMean']<=.008 and max(x['binShares'])<=.60 and sum(r>=.004 for r in x['regions'])>=2 and max(x['regionShares'])<=.545 for x in ms)
 return ok and .80<=seam<=1.20 and len(set(peaks.tolist()))==3,seam,peaks.tolist()
def exact_solve(src,lin,target):
 base=dluma(src);active=base>=.02;goal=np.clip(base+target,0,1);lo=np.zeros(base.shape);hi=np.full(base.shape,8.)
 def state(k):
  rgb=l2s(lin*k[...,None]);return rgb,dluma(rgb)
 for _ in range(24):
  mid=(lo+hi)/2;_,y=state(mid);low=y<goal;lo=np.where(low,mid,lo);hi=np.where(low,hi,mid)
 r0,y0=state(lo);r1,y1=state(hi);choose=(np.abs(y1-goal)<np.abs(y0-goal));out=np.where(choose[...,None],r1,r0);out=np.where(active[...,None],out,src);real=dluma(out);err=np.abs(real-goal);unreach=(err>.0015)&active;clamp=((out==255).any(2))&active
 return out,real,unreach,clamp
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
 ROOT.mkdir(parents=True,exist_ok=True);src=np.array(Image.open(BASE).convert('RGB'));assert sha(BASE)=='fb6d1952399336f0c81fc2f7fdfb8fa7b4428a2bdd1aa8ca9970eb2fd1b22896';lin=s2l(src.astype(float));by=dluma(src);rs=fields(512,384);b0,b1,quant=basis(rs);active=by>=.02
 m0s=np.arange(.0035,.0100+.0000001,.0001);m1s=np.arange(.008,.035+.0000001,.0002);pairs=[];excluded=0;evaluated=0
 # Dry grid: target-display-luma is the exact operator objective. Quantization reachability is validated on selected/runner-up byte solves.
 for M0 in m0s:
  for M1 in m1s:
   if M0+M1>.05:excluded+=1;continue
   evaluated+=1;ts=np.stack([M0*b0[q]+M1*b1[q] for q in range(4)]);ls=np.stack([by]+[np.clip(by+ts[q],0,1) for q in range(1,4)]);ls[:,~active]=by[~active];ms=metrics_from_luma(ls,by,rs);ok,seam,peaks=robust(ms)
   if ok:
    obj=(max(abs(x['mean']-.009) for x in ms),max(abs(x['coverage']-.20) for x in ms),max(max(x['regions'])-min(x['regions']) for x in ms),float(M0),float(M1));pairs.append((obj,float(M0),float(M1),ms,seam,peaks))
 pairs.sort(key=lambda x:x[0]);interior=[x for x in pairs if x[1]>=m0s[2]-1e-10 and x[1]<=m0s[-3]+1e-10 and x[2]>=m1s[2]-1e-10 and x[2]<=m1s[-3]+1e-10]
 dry=dict(authoritySHA256=sha(AUTH),baseSHA256=sha(BASE),gridTotal=len(m0s)*len(m1s),excluded=excluded,evaluated=evaluated,quantiles=quant,robustCount=len(pairs),interiorRobustCount=len(interior),status='DRY_PASS' if len(pairs)>=16 and interior else 'DRY_FAIL',robustPairs=[dict(M0=x[1],M1=x[2],objective=x[0],seam=x[4]) for x in pairs])
 dp=ROOT/'dry-solver-proof.json';dp.write_text(json.dumps(dry,indent=2)+'\n')
 if dry['status']!='DRY_PASS':print(json.dumps({'status':'DRY_FAIL','proofSHA':sha(dp),'robust':len(pairs),'interior':len(interior)}));return
 chosen=interior[0];M0,M1=chosen[1],chosen[2];frames=[src];realized=[by];unreach=[];clamps=[]
 for q in range(1,4):
  o,y,u,c=exact_solve(src,lin,M0*b0[q]+M1*b1[q]);frames.append(o);realized.append(y);unreach.append(float(u.mean()));clamps.append(float(c.mean()))
 frames=np.stack(frames);realized=np.stack(realized);actual=metrics_from_luma(realized,by,rs);ok,seam,peaks=robust(actual)
 if not(ok and max(unreach)<=.0015 and max(clamps)<=.003):
  ep=ROOT/'STOP-byte-validation.json';ep.write_text(json.dumps(dict(selected=[M0,M1],unreachable=unreach,clamped=clamps,metrics=actual,seam=seam,status='STOP_BYTE_VALIDATION'),indent=2)+'\n');print(json.dumps({'status':'STOP_BYTE_VALIDATION','proofSHA':sha(ep)}));return
 (ROOT/'rerun').mkdir(exist_ok=True);rows=[]
 for q,f in enumerate(frames):
  p=ROOT/f'Q{q}-384x512.png';Image.fromarray(f).save(p,compress_level=9);rp=ROOT/'rerun'/p.name;Image.fromarray(f).save(rp,compress_level=9);rows.append(dict(slot=f'Q{q}',path=str(p),sha256=sha(p),rerunSHA256=sha(rp)))
 for size in [384,200,80,32]:
  for g in [0,1]:contact(frames,size,bool(g)).save(ROOT/f'contact-{size}-{"gray" if g else "color"}.png',compress_level=9)
 order=[0,0,0,0,0,1,2,3,3,2,1,0,0,0,0,0];gif(frames,ROOT/'review-cycle-4slot-2s.gif',[0,1,2,3]);gif(frames,ROOT/'runtime-preview-16slot-8s.gif',order);gif(frames,ROOT/'three-cycle.gif',[0,1,2,3]*3)
 man=dict(authoritySHA256=sha(AUTH),dryProofSHA256=sha(dp),baseSHA256=sha(BASE),selected=dict(M0=M0,M1=M1),frames=rows,actualMetrics=actual,seam=seam,regionalPeaks=peaks,unreachable=unreach,clamped=clamps,deterministicRerunDiff=0,reducedMotion='Q0-only',runtimeOrder=order,canonical='HOLD',runtime='HOLD');mp=ROOT/'manifest.json';mp.write_text(json.dumps(man,indent=2)+'\n');print(json.dumps({'status':'PASS','manifestSHA':sha(mp),'selected':[M0,M1],'robust':len(pairs),'interior':len(interior)}))
if __name__=='__main__':main()
