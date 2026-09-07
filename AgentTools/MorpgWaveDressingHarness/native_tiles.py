"""Bake native-size templates from unchanged source pixels; layout is binding-only SSOT."""
from pathlib import Path
import json
from PIL import Image
R=Path(__file__).resolve().parents[2];P=R/'Assets/Resources/battle/morpg/wave-environment-v1/binding.json'
d=json.loads(P.read_text());d['interiorPropsEnabled']=True
d.setdefault('layout',dict(zoneWidth=32,zoneGap=20,overlap=.12,parallaxFactor=.70,cameraY=-.5,cameraHalfHeight=5,bottomInset=.65,topCount=2,bottomCount=4,topParallaxFactor=.42,bottomParallaxFactor=-.15,collisionBandInset=.05))
for w in d['waves']:
 for a in w['assets']:
  a['pivot']=[.5,.5];a['anchorRow']=0;a['bandOffset']=0
  if a['kind'] in ['far','floor']:a['rect']=[-2,d['layout']['cameraY']-d['layout']['cameraHalfHeight']-.2,d['layout']['zoneWidth']+2,d['layout']['cameraY']-d['layout']['cameraHalfHeight']-.2+(d['layout']['zoneWidth']+4)/3];continue
  alpha=Image.open(R/('Assets/Resources/'+a['resource']+'.png')).getchannel('A')
  rows=[y for y in range(512) if sum(v>=192 for v in alpha.crop((16,y,1520,y+1)).getdata())>752]
  row=max(rows) if a['kind']=='top' else min(rows);a['anchorRow']=row
  edge=4 if a['kind']=='top' else d['layout']['cameraY']-d['layout']['cameraHalfHeight']+d['layout']['bottomInset']
  t=edge+row/100
  solid=alpha.point(lambda v:255 if v>=192 else 0).getbbox();inset=d['layout']['collisionBandInset']
  offset=max(0,4+inset-(t-solid[3]/100)) if a['kind']=='top' else min(0,-4-inset-(t-solid[1]/100))
  a['bandOffset']=offset;t+=offset;a['rect']=[0,t-5.12,15.36,t];boxes=[]
  for y in range(0,512,4):
   run=None
   for x in range(0,1544,8):
    solid=x<1536 and alpha.crop((x,y,x+8,y+4)).getextrema()[0]>=192
    if solid and run is None:run=x
    if not solid and run is not None:
     boxes.append({'rect':[run/100,round(t-(y+4)/100,6),x/100,round(t-y/100,6)]});run=None
  a['colliderRects']=boxes
P.write_text(json.dumps(d,indent=2)+'\n')
print('native templates baked; source images/importers unchanged')
