"""Author exact contract geometry and deterministic safe reservation corrections."""
from pathlib import Path
import json,math,hashlib,uuid,shutil,re
from PIL import Image
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgEnvironmentInstall'
path=R/'Assets/Resources/battle/morpg/environment/environment.v1.json'
wave=R/'Assets/Resources/battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward.v1.json'
b=O/'before'/wave.relative_to(R);b.parent.mkdir(parents=True,exist_ok=True)
if not b.exists():shutil.copyfile(wave,b)
d=json.loads(b.read_text());assets=json.loads((O/'asset-map.json').read_text())
walk=[[[-14.5,-7.35],[-6.1,-7.35],[-5.85,-5.8],[-5.85,5.8],[-6.1,7.35],[-14.5,7.35]],[[-4.25,-7.35],[4.25,-7.35],[4.25,7.35],[-4.25,7.35]],[[6,-7.25],[14.5,-7.25],[14.5,7.25],[6,7.25]]]
cores=[[-13.65,-5.65,-6.85,5.65],[-3.55,-5.6,3.55,5.6],[6.75,-5.5,13.75,5.5]]
zs=[]
for i,z in enumerate(d['zones']):
 zs.append(dict(zoneId=z['id'],waveId=z['waveId'],anchorId=z['entryWarpAnchorId'],entry=z['entry'],walkable=walk[i],core=cores[i],spawnInset=.65,actorInset=.15,landingRadius=1.5,egressLength=2.5,egressRadius=1.4,egressAxis=[1,0],cameraClamp=[[-10.42,-5.25,-10.08,5.25],[-.35,-5.25,.35,5.25],[10.08,-5.25,10.42,5.25]][i],boundaryId='boundary.'+z['id']+'.combat.v1',propManifestId='props.'+z['id'],colliderManifestId='colliders.'+z['id'],backgroundBindingId='battle-runtime-background.full32x18',reservationAuditRef='environment.reservations.v1',props=[],decorations=[]))
def prop(zi,id,asset,center,radius=0,box=None,width=1,height=1):
 z=zs[zi];full='prop.'+z['zoneId']+'.'+id
 z['props'].append(dict(id=full,colliderId='collider.'+full,asset=asset,center=center,radius=radius,box=box or [],visualSize=[width,height],visualOrigin=[center[0],center[1]-(radius if radius else (box[3]-box[1])/2)]))
for i,(x,y) in enumerate([(-14,-6.8),(-12,-7),(-9,-7),(-6.4,-6.8),(-14,6.8),(-12,7),(-9,7),(-6.4,6.8)]):prop(0,'sapling.%02d'%(i+1),'z1-sapling',[x,y],.28,width=.9,height=.95)
for side,y in [('top',7),('bottom',-7)]:
 for name,x in [('left',-2.75),('right',2.75)]:prop(1,'fence.'+side+'.'+name,'z2-fence-straight',[x,y],box=[x-1.25,y-.12,x+1.25,y+.12],width=2.5,height=.95)
for name,box in [('house.top.left',[6.3,6.2,8.5,7.25]),('house.top.right',[11,6.2,13.5,7.25]),('house.bottom.left',[6.3,-7.25,8.5,-6.2]),('house.bottom.right',[11,-7.25,13.5,-6.2]),('wall.right',[14.3,-3,14.5,3])]:
 prop(2,name,'z3-wall-straight' if name=='wall.right' else 'z3-house-edge-end',[(box[0]+box[2])/2,(box[1]+box[3])/2],box=box,width=box[2]-box[0],height=6 if name=='wall.right' else (1 if 'bottom' in name else 1.8))
# Nonblocking art remains in the outer dressing band; it never supplies collision geometry.
for zi,id,x,y,w,h in [(0,'z1-boundary-straight',-11,8,2,.7),(0,'z1-boundary-corner',-15.3,7.8,.8,.8),(0,'z1-boundary-end',-15.3,-8.5,.8,.8),(0,'z1-boundary-gate',-10.25,-8.8,2,.8),(1,'z2-fence-corner',-4.7,7.8,.6,.8),(1,'z2-fence-end',4.7,-8.5,.6,.8),(1,'z2-fence-gate',0,7.7,2,.9),(1,'z2-cart-straw',0,-8.7,1.4,.8),(2,'z3-wall-corner',15.3,7.8,.6,.8),(2,'z3-wall-gate',11,7.9,2,.8),(2,'z3-square-props',13.5,-8.5,1.2,.6)]:
 zs[zi]['decorations'].append(dict(id='dressing.'+id,asset=id,origin=[x,y],size=[w,h]))
def edge_clear(z,p,inset):
 for a,b in zip(z['walkable'],z['walkable'][1:]+z['walkable'][:1]):
  dx,dy=b[0]-a[0],b[1]-a[1]
  if (dx*(p[1]-a[1])-dy*(p[0]-a[0]))/math.hypot(dx,dy)<inset-1e-6:return False
 return True
def dist_prop(p,q):
 if q['radius']:return math.dist(p,q['center'])-q['radius']
 a,b,c,d=q['box'];return math.hypot(max(a-p[0],0,p[0]-c),max(b-p[1],0,p[1]-d))
def capsule_dist(p,z):
 x,y=z['entry'];return math.hypot(p[0]-max(x,min(x+z['egressLength'],p[0])),p[1]-y)
def valid(z,p,others):
 return edge_clear(z,p,.65) and all(dist_prop(p,q)>=.55-1e-6 for q in z['props']) and math.dist(p,z['entry'])>=3.25 and capsule_dist(p,z)>=1.4+.55 and all(math.dist(p,t)>=.65 for t in others)
audit=[]
prior_path=O/'reservation-audit.json'
prior={r['reservationId']:r['position'] for r in json.loads(prior_path.read_text())} if prior_path.exists() else {}
for z,rows in zip(zs,d['zones']):
 accepted=[]
 for row in rows['reservations']:
  old=row['position'];new=prior.get(row['reservationId'],old)
  if prior:assert valid(z,new,accepted),('accepted reservation invalid under new geometry',row['reservationId'])
  if not valid(z,new,accepted):
   candidates=([x/20,y/20] for x in range(-300,301) for y in range(-140,141))
   safe=(p for p in candidates if valid(z,p,accepted) and math.dist(p,z['entry'])>=rows['radius'][0])
   new=min(safe,key=lambda p:((p[0]-old[0])**2+(p[1]-old[1])**2,p[0],p[1]))
  row['position']=new;accepted.append(new)
  audit.append(dict(reservationId=row['reservationId'],zoneId=z['zoneId'],role=row['unitKey'],original=old,position=new,spawnValid=True,relocated=new!=old))
for a in assets:
 im=Image.open(R/a['path']);x,y,xx,yy=im.getchannel('A').getbbox();a['opaqueRect']=[x/100,(1024-yy)/100,(xx-x)/100,(yy-y)/100]
for z in zs:
 for a in z['props']:a['visualRotation']=90 if a['id'].endswith('wall.right') else 0
profile=dict(schemaVersion='morpg-environment.v1',contractSha256=hashlib.sha256((O/'contract.txt').read_bytes()).hexdigest(),sourceManifestSha256='d5353184175684b7affaa9a95933c7271e3dc65addabbdbfa912c85e0bfa66fb',mapBounds=[-16,-9,16,9],overscan=.25,minCorridorWidth=2.5,assets=assets,zones=zs,reservations=audit)
for z in zs:z['walkable']=[dict(x=v[0],y=v[1]) for v in z['walkable']]
path.write_text(json.dumps(profile,indent=2)+'\n');source=b.read_text()
for row in audit:
 pattern=r'("reservationId":"'+re.escape(row['reservationId'])+r'"[^\n]*?"position":)\[[^\]]+\]'
 source,n=re.subn(pattern,lambda m:m[1]+json.dumps(row['position'],separators=(',',':')),source);assert n==1
wave.write_text(source)
meta=Path(str(path)+'.meta')
if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextScriptImporter:\n  externalObjects: {}\n')
(O/'reservation-audit.json').write_text(json.dumps(audit,indent=2)+'\n')
print('profile authored; relocated',sum(x['relocated'] for x in audit),'of 28 reservations')
