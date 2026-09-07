from pathlib import Path
import json,hashlib,re
from PIL import Image
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgPivotImportFix';B=O/'before';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
path='Assets/Resources/battle/morpg/wave-environment-v1/binding.json';old=json.loads((B/path).read_text());current=json.loads((R/path).read_text());assert current['interiorPropsEnabled'] is True
before=json.loads((O/'before-file-dependencies.json').read_text());rows=[]
for ow,nw in zip(old['waves'],current['waves']):
 for a,b in zip(ow['assets'],nw['assets']):
  a['pivot']=[.5,.5];assert a==b,'anchor/geometry changed'
  image=R/('Assets/Resources/'+b['resource']+'.png');meta=Path(str(image)+'.meta');im=Image.open(image);assert im.size==(1536,512)
  src=meta.read_text();prior=(B/meta.relative_to(R)).read_text();guid=re.search(r'^guid: (\w+)',src,re.M)[1];assert guid==b['guid']==re.search(r'^guid: (\w+)',prior,re.M)[1]
  for token in ['spriteAlignment: 0','spritePivot: {x: 0.5, y: 0.5}','spritePixelsToUnits: 100','maxTextureSize: 2048','spriteMeshType: 0']:assert token in src,(meta,token)
  assert sha(image)==b['sha256']==before[str(image.relative_to(R))]['sha256']
  for p in [image,meta]:assert p.stat().st_mtime_ns>before[str(p.relative_to(R))]['mtimeNs'],('dependency timestamp not refreshed',p)
  l,bot,r,t=b['rect'];w,h=r-l,t-bot
  for px,py in [(b['alphaRect'][0],b['alphaRect'][1]),(b['alphaRect'][2],b['alphaRect'][3])]:
   x=l+w*.5+(px-768)/100*(w/15.36);y=bot+h*.5+(512-py-256)/100*(h/5.12)
   assert abs(x-(l+w*px/1536))<1e-6 and abs(y-(t-h*py/512))<1e-6
  rows.append(dict(id=b['id'],guid=guid,metaSha256=sha(meta),sourceSha256=sha(image),spriteAlignment=0,normalizedPivot=[.5,.5],expectedImportedPivotPixels=[768,256],canvas=[1536,512],ppu=100,sourceMtimeNs=image.stat().st_mtime_ns,metaMtimeNs=meta.stat().st_mtime_ns,sourceMtimeBeforeNs=before[str(image.relative_to(R))]['mtimeNs'],metaMtimeBeforeNs=before[str(meta.relative_to(R))]['mtimeNs']))
assert old==current
for n,h in json.loads((O/'same-path-before.json').read_text()).items():assert sha(R/n)==h,n
(O/'meta-readback-and-dependencies.json').write_text(json.dumps(rows,indent=2)+'\n')
print('PASS canonical center import settings9/9; GUID+PNG unchanged9/9; source/meta mtime advanced18/18; binding rect/alpha/collider/interior geometry diff0')
print('Actual post-reimport Sprite values require next Play; no Unity runtime query was performed.')
