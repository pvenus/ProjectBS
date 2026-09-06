#!/usr/bin/env python3
import glob,json
from pathlib import Path
from PIL import Image
ROOT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-01')
BODY_TARGET=[(290,485),(310,475),(330,465),(453,446),(576,428),(314,476)]
VFX_TARGET={
'g1':[(113,31),(217,41),(236,64),(236,74),(222,69),(164,61)],
'g2':[(124,39),(194,142),(236,100),(236,97),(236,83),(235,89)],
'g3':[(158,44),(236,62),(236,87),(236,105),(236,100),(236,59)]}
def wh(p):
 b=Image.open(p).convert('RGBA').getchannel('A').getbbox(); return b[2]-b[0],b[3]-b[1]
def feasible(src,tgt,tol):
 sw,sh=src; tw,th=tgt
 lo=max(tw*(1-tol)/sw,th*(1-tol)/sh); hi=min(tw*(1+tol)/sw,th*(1+tol)/sh)
 f=(tw/sw+th/sh)/2
 return lo<=hi,lo,hi,f,(sw*f,sh*f)
rows=[]
for hit in ('hit0','hit1','hit2'):
 for i,p in enumerate(sorted((ROOT/'body'/hit).glob('frame-*.png'))):
  src=wh(p); ok,lo,hi,f,pred=feasible(src,BODY_TARGET[i],.05)
  rows.append({'registry':'body','unit':hit,'frame':i,'src':src,'target':BODY_TARGET[i],'uniform_feasible_5pct':ok,'factor_interval':[lo,hi],'balanced_prediction':pred})
for grade in ('g1','g2','g3'):
 for hit in ('hit1','hit2'):
  for i,p in enumerate(sorted((ROOT/'vfx'/grade/hit).glob('frame-*.png'))):
   src=wh(p); ok,lo,hi,f,pred=feasible(src,VFX_TARGET[grade][i],.08)
   rows.append({'registry':'vfx','unit':f'{grade}/{hit}','frame':i,'src':src,'target':VFX_TARGET[grade][i],'uniform_feasible_8pct':ok,'factor_interval':[lo,hi],'balanced_prediction':pred})
out=ROOT/'exact-target-feasibility.json'; out.write_text(json.dumps(rows,indent=2)+'\n')
print('body feasible',sum(r.get('uniform_feasible_5pct',False) for r in rows if r['registry']=='body'),'/18')
print('vfx feasible',sum(r.get('uniform_feasible_8pct',False) for r in rows if r['registry']=='vfx'),'/36')
print(out)
