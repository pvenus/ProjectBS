from pathlib import Path
import json,os
from PIL import Image
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgInteriorRestore';P=R/'Assets/Resources/battle/morpg/wave-environment-v1/binding.json'
d=json.loads(P.read_text())
if 'layout' in d:raise SystemExit('Native tiling binding installed; use native_tiles.py to regenerate.')
e=json.loads((R/'Assets/Resources/battle/morpg/environment/environment.v1.json').read_text());d['interiorPropsEnabled']=True
for w,z in zip(d['waves'],e['zones']):
 left=min(v['x'] for v in z['walkable']);right=max(v['x'] for v in z['walkable']);bottom=min(v['y'] for v in z['walkable']);top=max(v['y'] for v in z['walkable'])
 for a in w['assets']:
  a['pivot']=[.5,.5]
  im=Image.open(R/('Assets/Resources/'+a['resource']+'.png'));bb=im.getchannel('A').getbbox() if im.mode=='RGBA' else (0,0,1536,512);a['alphaRect']=list(bb)
  if a['kind']=='background':a['rect']=[left-2,bottom-1.2,right+2,bottom-1.2+(right-left+4)/3];continue
  sx=(right-left)/(bb[2]-bb[0]);sy=1.0/(bb[3]-bb[1]);l=left-bb[0]*sx;t=top+bb[3]*sy if a['kind']=='top' else bottom+bb[1]*sy
  a['rect']=[l,t-512*sy,l+1536*sx,t];alpha=im.getchannel('A');boxes=[]
  for y in range(0,512,8):
   run=None
   for px in range(0,1568,32):
    y0=t-(y+8)*sy;y1=t-y*sy;outside=y0>=top+.05 if a['kind']=='top' else y1<=bottom-.05
    solid=px<1536 and outside and alpha.crop((px,y,px+32,y+8)).getextrema()[0]>=192
    if solid and run is None:run=px
    if not solid and run is not None:boxes.append({'rect':[round(l+run*sx,6),round(y0,6),round(l+px*sx,6),round(y1,6)]});run=None
  assert boxes;a['colliderRects']=boxes
text=json.dumps(d,indent=2)+'\n';tmp=Path(str(P)+'.tmp');tmp.write_text(text);os.replace(tmp,P);(O/'binding.json').write_text(text)
print('PASS bbox inner edges aligned to y+/-4; visible side edges to zone x bounds; interior production gate ON')
