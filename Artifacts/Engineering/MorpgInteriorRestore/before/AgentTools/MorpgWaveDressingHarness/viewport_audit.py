from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgWaveViewportFix';D=R/'Assets/Resources/battle/morpg/wave-environment-v1/binding.json';d=json.loads(D.read_text());e=json.loads((R/'Assets/Resources/battle/morpg/environment/environment.v1.json').read_text());assert d['interiorPropsEnabled'] is False
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for n,h in json.loads((O/'same-path-before.json').read_text()).items():assert sha(R/n)==h,n
W,H=960,540;hh=e['cameraOrthographicSize'];hw=hh*16/9;scale=H/(hh*2);rows=[];images={};patches=0;previews=[]
for i,(wave,z) in enumerate(zip(d['waves'],e['zones'])):
 left=min(v['x'] for v in z['walkable']);right=max(v['x'] for v in z['walkable']);bottom=min(v['y'] for v in z['walkable']);top=max(v['y'] for v in z['walkable']);bg=wave['assets'][0]['rect']
 xmin=max(left+hw,bg[0]+hw+.1);xmax=min(right-hw,bg[2]-hw-.1);ymin=max(bottom+hh-1,bg[1]+hh+.1);ymax=min(top-hh+1,bg[3]-hh-.1)
 assert abs(xmin-hw-left)<1e-6 and abs(xmax+hw-right)<1e-6
 for a in wave['assets']:
  path=R/('Assets/Resources/'+a['resource']+'.png');im=Image.open(path).convert('RGBA');images[a['id']]=im;assert sha(path)==a['sha256']
  bb=im.getchannel('A').getbbox();assert list(bb)==a['alphaRect'];l,b,r,t=a['rect'];sx=(r-l)/1536;sy=(t-b)/512
  if a['kind']=='background':continue
  assert abs(l+bb[0]*sx-left)<1e-6 and abs(l+bb[2]*sx-right)<1e-6
  inner=t-bb[3 if a['kind']=='top' else 1]*sy;assert abs(inner-(top if a['kind']=='top' else bottom))<1e-6
  for patch in a['colliderRects']:
   x0,y0,x1,y1=patch['rect'];crop=(round((x0-l)/sx),round((t-y1)/sy),round((x1-l)/sx),round((t-y0)/sy));assert im.getchannel('A').crop(crop).getextrema()[0]>=192
   assert y0>=top+.05-1e-6 if a['kind']=='top' else y1<=bottom-.05+1e-6;patches+=1
 positions={'center':((xmin+xmax)/2,-.9),'left':(xmin,-.9),'right':(xmax,-.9),'top':((xmin+xmax)/2,ymax),'bottom':((xmin+xmax)/2,ymin),'top-left':(xmin,ymax),'top-right':(xmax,ymax),'bottom-left':(xmin,ymin),'bottom-right':(xmax,ymin)}
 for name,(cx,cy) in positions.items():
  vl,vb,vr,vt=cx-hw,cy-hh,cx+hw,cy+hh;assert vl>=bg[0] and vr<=bg[2] and vb>=bg[1] and vt<=bg[3]
  out=Image.new('RGBA',(W,H),(0,0,0,0))
  for a in sorted(wave['assets'],key=lambda a:a['sortingOrder']):
   l,b,r,t=a['rect'];tile=images[a['id']].resize((round((r-l)*scale),round((t-b)*scale)),Image.Resampling.BILINEAR)
   out.alpha_composite(tile,(round((l-vl)*scale),round((vt-t)*scale)))
  assert out.getchannel('A').getextrema()==(255,255),'uncovered gray-producing viewport'
  path=O/f'wave{i+1}-viewport-{name}.png';out.convert('RGB').save(path);previews.append((i,name,out))
  rows.append(dict(wave=i+1,view=name,camera=[cx,cy],worldCrop=[vl,vb,vr,vt],pixels=[W,H],uncoveredPixels=0,path=str(path.relative_to(R))))
contact=Image.new('RGB',(480*3,294*9),(235,231,221));draw=ImageDraw.Draw(contact)
for i,name,im in previews:
 row=list(positions).index(name);contact.paste(im.convert('RGB').resize((480,270)),(i*480,row*294+24));draw.text((i*480+8,row*294+6),f'W{i+1} / {name} — static viewport',(30,30,30))
contact.save(O/'viewport-contact.png');(O/'viewport-crops.json').write_text(json.dumps(rows,indent=2)+'\n')
report=dict(viewportCrops=27,uncoveredPixels=0,worldSideEdgeError=0,alphaInnerEdgeError=0,opaqueSupportedPatches=patches,sourcePngShaDiff=0,protectedSamePathDiff=0,interiorCreation=False,collisionDefinitionsPreserved=True,backgroundGroundAtZoneTop=(4-bg[1])/(bg[3]-bg[1]),method='Static compositing of exact installed assets, nominal16:9 orthographic viewport; no Unity/GPU execution')
(O/'viewport-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
