#!/usr/bin/env python3
import glob,os,json,hashlib
from pathlib import Path
from PIL import Image
GEN=Path('/Users/pvenus/.codex/generated_images/01a0421a-c5ea-7623-88db-2ac71c27bd93')
files=sorted(GEN.glob('exec-*.png'),key=os.path.getmtime)[-18:]
TARGET=[(290,485),(310,475),(330,465),(453,446),(576,428),(314,476)]
OUT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/candidates/revision-03/body-attempt-c')
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
rows=[]
for n,p in enumerate(files):
 hit=n//6; i=n%6; tw,th=TARGET[i]
 im=Image.open(p).convert('RGBA'); b=im.getchannel('A').getbbox(); fg=im.crop(b); sw,sh=fg.size
 f=min(tw/sw,th/sh); rw,rh=round(sw*f),round(sh*f)
 ok=abs(rw-tw)/tw<=.05 and abs(rh-th)/th<=.05
 rows.append({'hit':hit,'frame':i,'source':str(p),'source_sha':sha(p),'source_bbox':[sw,sh],'target':[tw,th],'uniform_result':[rw,rh],'pass':ok})
 if ok:
  fg=fg.resize((rw,rh),Image.Resampling.LANCZOS); canvas=Image.new('RGBA',(768,512),(0,0,0,0)); x=(768-rw)//2; y=500-rh; canvas.alpha_composite(fg,(x,y))
  q=OUT/f'hit{hit}'/f'frame-{i}.png'; q.parent.mkdir(parents=True,exist_ok=True); canvas.save(q); rows[-1].update({'path':str(q),'sha':sha(q),'bbox':[x,y,x+rw,y+rh]})
OUT.mkdir(parents=True,exist_ok=True); (OUT/'manifest.json').write_text(json.dumps(rows,indent=2)+'\n')
print('pass',sum(r['pass'] for r in rows),'/18'); print(*[(r['hit'],r['frame'],r['source_bbox'],r['uniform_result'],r['target']) for r in rows if not r['pass']],sep='\n')
