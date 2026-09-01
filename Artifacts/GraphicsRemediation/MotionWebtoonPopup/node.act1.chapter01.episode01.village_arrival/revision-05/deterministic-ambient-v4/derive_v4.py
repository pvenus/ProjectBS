from pathlib import Path
import hashlib,json
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parent
BASE=ROOT.parents[1]/'revision-01/candidate-B-frames-v3/Q0-384x512.png'
AUTH=Path('/private/tmp/projectbs-current-byeori-village-arrival-ambient-v4-production-contract.txt')
PH=np.array([[0,0,0],[1,.45,-.30],[.25,1,.45],[-.35,.30,1]],np.float64)
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def s2l(x):x=x/255.;return np.where(x<=.04045,x/12.92,((x+.055)/1.055)**2.4)
def l2s(x):x=np.clip(x,0,1);y=np.where(x<=.0031308,12.92*x,1.055*x**(1/2.4)-.055);return np.clip(np.floor(y*255+.5),0,255).astype(np.uint8)
def lum(x):return .2126*x[...,0]+.7152*x[...,1]+.0722*x[...,2]
def smooth(x):x=np.clip(x,0,1);return x*x*(3-2*x)
def fields(h,w):
 y,x=np.mgrid[0:h,0:w];u=x/(w-1);v=y/(h-1);rs=[]
 for cx,cy,sx,sy in [(.24,.24,.26,.22),(.52,.50,.30,.25),(.78,.74,.24,.22)]:
  r=np.exp(-.5*(((u-cx)/sx)**2+((v-cy)/sy)**2));rs.append(r/r.max())
 return np.stack(rs)
def basis(rs):
 z=np.stack([(PH[q,:,None,None]*rs).sum(0)/(rs.sum(0)+1e-6) for q in range(4)])
 W=smooth((np.max(rs,axis=0)-.15)/.50);b0=[];b1=[];quant=[]
 for q in range(4):
  a=np.abs(z[q]);lo=float(np.quantile(a,.72));hi=float(np.quantile(a,.88));t=np.zeros_like(a) if hi<=lo else smooth((a-lo)/(hi-lo));sq=np.tanh(4*z[q]);b0.append(sq*(.75+.25*W));b1.append(sq*t);quant.append([lo,hi])
 return np.stack(b0),np.stack(b1),quant
def render_luma(lin,g):return lum(s2l(l2s(lin*(1+g[...,None])).astype(np.float64)))
def main():
 ROOT.mkdir(parents=True,exist_ok=True);base=np.array(Image.open(BASE).convert('RGB'));assert sha(BASE)=='fb6d1952399336f0c81fc2f7fdfb8fa7b4428a2bdd1aa8ca9970eb2fd1b22896'
 lin=s2l(base.astype(float));rs=fields(512,384);b0,b1,quant=basis(rs);base_l=render_luma(lin,np.zeros((512,384)))
 m0s=np.arange(.0035,.0090+.0000001,.0001);m1s=np.arange(.0080,.0300+.0000001,.0002)
 scanned=excluded=primary=0;env={'meanMin':1.,'meanMax':0.,'coverageMin':1.,'coverageMax':0.,'maxPixelMax':0.};best=None
 for M0 in m0s:
  for M1 in m1s:
   scanned+=1
   if M0+M1>.05:excluded+=1;continue
   ls=[base_l]
   for q in range(1,4):ls.append(render_luma(lin,M0*b0[q]+M1*b1[q]))
   metrics=[]
   for a,b in [(0,1),(1,2),(2,3),(3,0)]:
    d=np.abs(ls[b]-ls[a]);metrics.append((float(d.mean()),float((d>.01).mean()),float(np.quantile(d,.95)),float(np.quantile(d,.99)),float(d.max())))
   means=[m[0] for m in metrics];cov=[m[1] for m in metrics]
   env['meanMin']=min(env['meanMin'],min(means));env['meanMax']=max(env['meanMax'],max(means));env['coverageMin']=min(env['coverageMin'],min(cov));env['coverageMax']=max(env['coverageMax'],max(cov));env['maxPixelMax']=max(env['maxPixelMax'],max(m[4] for m in metrics))
   score=max(abs(x-.009) for x in means)+max(abs(x-.20) for x in cov)
   if best is None or score<best[0]:best=(score,float(M0),float(M1),metrics)
   if all(.006<=m[0]<=.012 and .12<=m[1]<=.28 and m[2]<=.025 and m[3]<=.035 and m[4]<=.05 for m in metrics):primary+=1
 proof=dict(authoritySHA256=sha(AUTH),baseSHA256=sha(BASE),formula='Sq*(M0*(.75+.25W)+M1*Tq)',quantiles=quant,gridPairs=scanned,analyticallyExcluded=excluded,postQuantPrimaryGatePass=primary,observedEnvelope=env,closestPrimary=dict(M0=best[1],M1=best[2],transitions=best[3]),contractTarget=dict(meanAbsLuma=[.006,.012],coverageOver1pct=[.12,.28],localMax=.05),status='STOP_NO_FEASIBLE_PAIR' if primary==0 else 'PRIMARY_PASS_REQUIRES_FULL_GATES')
 p=ROOT/'solver-proof.json';p.write_text(json.dumps(proof,indent=2)+'\n');print(json.dumps({'status':proof['status'],'proofSHA256':sha(p),'primary':primary,'grid':scanned}))
if __name__=='__main__':main()
