#!/usr/bin/env python3
import hashlib,json
from pathlib import Path
from PIL import Image

BASE=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected')
SRC=BASE/'revision-01'; DST=BASE/'revision-02-scale-normalized'
FACTORS={'body':1.06,'vfx':1.10}

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def norm(p,kind):
 im=Image.open(p).convert('RGBA'); box=im.getchannel('A').getbbox()
 fg=im.crop(box); f=min(FACTORS[kind],236/fg.width,236/fg.height)
 sz=(round(fg.width*f),round(fg.height*f)); fg=fg.resize(sz,Image.Resampling.LANCZOS)
 out=Image.new('RGBA',(256,256),(0,0,0,0))
 x=(256-sz[0])//2; y=244-sz[1] if kind=='body' else (256-sz[1])//2
 out.alpha_composite(fg,(x,y)); px=out.load()
 for yy in range(256):
  for xx in range(256):
   if px[xx,yy][3]==0: px[xx,yy]=(0,0,0,0)
 return out,f,[x,y,x+sz[0],y+sz[1]]

rows=[]
for kind in ('body','vfx'):
 for p in sorted((SRC/kind).glob('**/frame-*.png')):
  rel=p.relative_to(SRC); q=DST/rel; q.parent.mkdir(parents=True,exist_ok=True)
  out,f,b=norm(p,kind); out.save(q,optimize=False)
  rows.append({'registry':kind,'source':str(p),'source_sha256':sha(p),'path':str(q),'sha256':sha(q),'factor_applied':f,'bbox':b,'pivot':[.5,.5],'size':[256,256]})
manifest={'status':'COMPLETE54_SCALE_NORMALIZED_CANDIDATE','policy':{'body_global_factor':1.06,'vfx_global_factor':1.10,'max_bbox':236,'body_anchor':'bottom_y244','vfx_anchor':'canvas_center','crop':False,'warp':False},'rows':rows,
 'known_limit':'VFX source aspect remains substantially taller/narrower than accepted hit0; matching hit0 width would overflow without forbidden crop/warp.'}
DST.mkdir(parents=True,exist_ok=True)
m=DST/'manifest.json'; m.write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps({'manifest':str(m),'sha256':sha(m),'count':len(rows)},indent=2))
