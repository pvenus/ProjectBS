from pathlib import Path
import json,hashlib,re
from PIL import Image,ImageDraw
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgWaveDressingInstall';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
d=json.loads((R/'Assets/Resources/battle/morpg/wave-environment-v1/binding.json').read_text());p=json.loads((R/'Assets/Resources/battle/morpg/environment/environment.v1.json').read_text());assets=json.loads((O/'installed-assets.json').read_text());guids={}
for meta in (R/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: ([0-9a-f]{32})$',meta.read_text(errors='ignore'),re.M)
 if m:guids.setdefault(m[1],[]).append(str(meta))
images={};patches=0
for row in assets:
 path=R/row['path'];assert sha(path)==sha(R/row['source'])==row['sha256'];im=Image.open(path);images[path.stem]=im
 assert im.size==(1536,512) and len(guids[row['guid']])==1
 meta=Path(str(path)+'.meta').read_text()
 for token in ['spritePixelsToUnits: 100','spriteMeshType: 0','spriteMode: 1','maxTextureSize: 2048','filterMode: 1','enableMipMap: 0','wrapU: 1','textureCompression: 0','physicsShape: []']:assert token in meta,(path,token)
for wave in d['waves']:
 for a in wave['assets']:
  im=images[a['resource'].split('/')[-1]];l,b,r,t=a['rect'];sx=(r-l)/1536;sy=(t-b)/512
  meta=(R/('Assets/Resources/'+a['resource']+'.png.meta')).read_text();assert f"spritePivot: {{x: 0.5, y: {a['pivot'][1]}}}" in meta
  if a['kind']=='background':assert im.mode=='RGB' and not a['colliderRects'];continue
  assert im.mode=='RGBA';alpha=im.getchannel('A')
  for patch in a['colliderRects']:
   x0,y0,x1,y1=patch['rect'];px0=round((x0-l)/sx);px1=round((x1-l)/sx);py0=round((t-y1)/sy);py1=round((t-y0)/sy)
   assert alpha.crop((px0,py0,px1,py1)).getextrema()[0]>=192,'collider spans transparent pixels'
   assert y0>=4.35-1e-5 if a['kind']=='top' else y1<=-4.35+1e-5
   patches+=1
protected=json.loads((O/'same-path-before.json').read_text())
for n,h in protected.items():assert sha(R/n)==h,('protected same-path mutation',n)
# Static world composition from byte-exact installed assets; not a Unity screenshot.
S=45;previews=[]
propimages={a['id']:Image.open(R/('Assets/Resources/'+a['resource']+'.png')) for a in p['assets']}
for i,(wave,z) in enumerate(zip(d['waves'],p['zones'])):
 x=36*i;canvas=Image.new('RGBA',(34*S,16*S),(225,214,190,255))
 def pix(wx,wy):return(round((wx-x+1)*S),round((8-wy)*S))
 def paste(raw,rect):
  l,b,r,t=rect;tile=raw.convert('RGBA').resize((round((r-l)*S),round((t-b)*S)),Image.Resampling.LANCZOS);canvas.alpha_composite(tile,pix(l,t))
 for a in wave['assets']:
  if a['kind']!='bottom':paste(images[a['resource'].split('/')[-1]],a['rect'])
 for a in z['props']+z['decorations']:
  origin=a.get('visualOrigin',a.get('origin'));size=a.get('visualSize',a.get('size'));raw=propimages[a['asset']];raw=raw.crop(raw.getchannel('A').getbbox())
  if a.get('visualRotation')==90:raw=raw.transpose(Image.Transpose.ROTATE_90)
  if a.get('flipX'):raw=raw.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
  paste(raw,[origin[0]-size[0]/2,origin[1],origin[0]+size[0]/2,origin[1]+size[1]])
 a=wave['assets'][2];paste(images[a['resource'].split('/')[-1]],a['rect'])
 draw=ImageDraw.Draw(canvas);draw.rectangle([pix(x,4),pix(x+32,-4)],outline='#3d6157',width=2)
 for a in wave['assets']:
  for patch in a['colliderRects']:
   l,b,r,t=patch['rect'];draw.rectangle([pix(l,t),pix(r,b)],outline='#a53630',width=1)
 canvas.convert('RGB').save(O/f'wave{i+1}-world-preview.png');previews.append(canvas)
contact=Image.new('RGBA',(34*S,16*S*3));
for i,img in enumerate(previews):contact.alpha_composite(img,(0,i*16*S))
contact.convert('RGB').save(O/'all-waves-world-preview.png')
report=dict(byteExact=9,uniqueGuid=9,backgrounds=3,barriers=6,opaqueSupportedColliderPatches=patches,collisionAlphaMin=192,minimumAbsoluteColliderY=4.35,protectedSamePathDiff=0,foregroundGroundMappingAtCombatTop=(4+4.5)/(32.5/3),imageEvidence='static composition only; not Unity GPU')
(O/'asset-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
