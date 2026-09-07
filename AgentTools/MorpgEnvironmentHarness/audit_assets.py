from pathlib import Path
import hashlib,json,re
from PIL import Image,ImageDraw
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgEnvironmentInstall'
rows=json.loads((O/'asset-map.json').read_text());p=json.loads((R/'Assets/Resources/battle/morpg/environment/environment.v1.json').read_text())
guids={};audit=[]
for meta in (R/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: ([0-9a-f]{32})$',meta.read_text(errors='ignore'),re.M)
 if m:guids.setdefault(m[1],[]).append(str(meta.relative_to(R)))
for row in rows:
 file=R/row['path'];assert hashlib.sha256(file.read_bytes()).hexdigest()==row['sha256']
 im=Image.open(file);assert im.size==(1024,1024) and im.mode=='RGBA'
 a=im.getchannel('A');border=max(a.crop(box).getextrema()[1] for box in [(0,0,1024,48),(0,976,1024,1024),(0,0,48,1024),(976,0,1024,1024)])
 assert border==0
 assert not any((r or g or b) and alpha==0 for r,g,b,alpha in im.getdata())
 assert len(guids[row['guid']])==1
 meta=Path(str(file)+'.meta').read_text()
 for token in ['spriteMode: 1','spriteAlignment: 9','spritePivot: {x: 0.5, y: 0.046875}','spritePixelsToUnits: 100','maxTextureSize: 1024','textureCompression: 0','enableMipMap: 0','filterMode: 1','wrapU: 1','physicsShape: []']:assert token in meta,(file,token)
 audit.append(dict(id=row['id'],rgba=True,resolution=[1024,1024],borderAlphaMax=border,alphaZeroRgbNonzero=0,guidUnique=True))
O=O/'revision-03/applied'
(O/'asset-audit.json').write_text(json.dumps(audit,indent=2)+'\n')
# Authored top-down composition preview. This is not a Unity screenshot.
S=45;map_left,map_bottom,map_right,map_top=p['mapBounds'];im=Image.new('RGBA',(int((map_right-map_left)*S),int((map_top-map_bottom)*S)),(222,211,185,255));draw=ImageDraw.Draw(im)
def pix(x,y):return (int((x-map_left)*S),int((map_top-y)*S))
images={row['id']:Image.open(R/row['path']) for row in rows}
for z in p['zones']:
 draw.polygon([pix(v['x'],v['y']) for v in z['walkable']],outline=(100,88,63))
 for a in z['props']+z['decorations']:
  origin=a.get('visualOrigin',a.get('origin'));size=a.get('visualSize',a.get('size'))
  raw=images[a['asset']];raw=raw.crop(raw.getchannel('A').getbbox())
  if a.get('visualRotation')==90:raw=raw.transpose(Image.Transpose.ROTATE_90)
  if a.get('flipX'):raw=raw.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
  tile=raw.resize((max(1,int(size[0]*S)),max(1,int(size[1]*S))),Image.Resampling.LANCZOS)
  px,py=pix(origin[0]-size[0]/2,origin[1]+size[1]);im.alpha_composite(tile,(px,py))
 draw=ImageDraw.Draw(im)
 for a in z['props']:
  if a['radius']:
   x,y=a['center'];r=a['radius'];draw.ellipse([pix(x-r,y+r),pix(x+r,y-r)],outline=(180,45,40),width=2)
  else:
   x,y,xx,yy=a['box'];draw.rectangle([pix(x,yy),pix(xx,y)],outline=(180,45,40),width=2)
 x,y=z['entry'];draw.ellipse([pix(x-1.5,y+1.5),pix(x+1.5,y-1.5)],outline=(50,120,180),width=2)
 for row in p['reservations']:
  if row['zoneId']==z['zoneId']:
   x,y=row['position'];draw.ellipse([pix(x-.09,y+.09),pix(x+.09,y-.09)],fill=(30,90,160))
im.convert('RGB').save(O/'authored-layout-audit.png')
for i,z in enumerate(p['zones']):
 x=min(v['x'] for v in z['walkable']);im.crop((pix(x-1,8)[0],0,pix(x+33,8)[0],im.height)).convert('RGB').save(O/f'zone{i+1}-layout.png')
print('PASS 15/15 byte-exact RGBA1024, 48px alpha border, zero-alpha RGB, unique GUID and importer audits')
print('Saved authored layout audit (not Unity gameplay evidence)')
