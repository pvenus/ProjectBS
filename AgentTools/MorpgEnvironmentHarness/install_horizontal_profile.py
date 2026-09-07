"""Deterministic revision03 author. Preserves identities and timings from task baseline."""
from pathlib import Path
import json,hashlib,copy,math,re
R=Path('/Users/pvenus/ProjectBS');O=R/'Artifacts/Engineering/MorpgEnvironmentInstall/revision-03/applied';B=O/'before'
profile_path=Path('Assets/Resources/battle/morpg/environment/environment.v1.json')
wave_path=Path('Assets/Resources/battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward.v1.json')
p=json.loads((B/profile_path).read_text());d=json.loads((B/wave_path).read_text())
p.update(mapBounds=[-3,-8,107,8],cameraOrthographicSize=4,contractSha256=hashlib.sha256((O/'geometry-contract.txt').read_bytes()).hexdigest())
for i,(z,w) in enumerate(zip(p['zones'],d['zones'])):
 x=36*i;entry=[x+5,0]
 z.update(entry=entry,walkable=[{'x':a,'y':b} for a,b in [(x,-4),(x+32,-4),(x+32,4),(x,4)]],core=[x+1.7,-2.5,x+30.3,2.5],cameraClamp=[x+7.2,-.25,x+24.8,.25])
 z.pop('transitionExit',None);z.pop('transitionStaging',None)
 if i<2:z['transitionExit']=[x+30,0]
 if i>0:z['transitionStaging']=[x+2,0]
 for j,q in enumerate(z['props']):
  if i==0:
   side=-1 if j<4 else 1;cx=x+[4,10,21,28][j%4];cy=side*3.65
   q.update(center=[cx,cy],radius=[.30,.36,.32,.4][j%4],box=[],visualSize=[[3.6,3.8],[2.8,3],[4.2,4.4],[2.2,2.5]][j%4])
  elif i==1:
   side=1 if 'top' in q['id'] else -1;cx=x+(9 if 'left' in q['id'] else 24);cy=side*3.65
   q.update(center=[cx,cy],radius=0,box=[cx-2.5,cy-.18,cx+2.5,cy+.18],visualSize=[5,2 if j%2==0 else 1.6])
  elif q['id'].endswith('wall.right'):
   q.update(center=[x+31.825,0],radius=0,box=[x+31.7,-1,x+31.95,1],visualSize=[.8,2]);side=0
  else:
   side=1 if 'top' in q['id'] else -1;cx=x+(7 if 'left' in q['id'] else 24);cy=side*3.625
   q.update(center=[cx,cy],radius=0,box=[cx-2,cy-.375,cx+2,cy+.375],visualSize=[5.6 if j%2 else 4.6,3.6 if j%2 else 3])
  q['visualRotation']=90 if q['id'].endswith('wall.right') else 0;q['flipX']=bool(j%2)
  if side<0:q['visualSize'][1]=round(q['visualSize'][1]*.65,2)
  contact=q['center'][1]-q['radius'] if q['radius'] else q['box'][1]
  q['visualOrigin']=[q['center'][0],contact]
 old=z['decorations'];mods=[a['id'] for a in p['assets'] if a['id'].startswith('z'+str(i+1)+'-')];decor=[]
 for side in [-1,1]:
  for cluster,center in enumerate([5,16,27]):
   for k in range(3):
    n=len(decor);prior=old[n] if n<len(old) else None
    width=[3.8,2.6,1.5][k];height=[2.8,2,1.2][k];cx=x+center+[-1.25,.65,1.9][k]
    bottom=3.35+[.1,.45,.15][k] if side>0 else -3.35-height-[.1,.45,.15][k]
    decor.append(dict(id=prior['id'] if prior else f"dressing.z{i+1}.cluster.{side}.{cluster}.{k}",asset=prior['asset'] if prior else mods[(n+i)%len(mods)],origin=[cx,bottom],size=[width,height],flipX=bool((n+i)%2),visualRotation=0))
 z['decorations']=decor
 w.update(entry=entry,bounds=[x,-4,x+32,4],spawnBounds=[x+.65,-3.35,x+31.35,3.35],radius=[3.25,24])
 for j,row in enumerate(w['reservations']):
  row['position']=[round(x+9+18*j/(len(w['reservations'])-1),2),2.1 if j%2==0 else -2.1]
lookup={r['reservationId']:r for w in d['zones'] for r in w['reservations']}
for r in p['reservations']:
 r['position']=lookup[r['reservationId']]['position'];r['relocated']=r['position']!=r['original'];r['spawnValid']=True
(R/profile_path).write_text(json.dumps(p,indent=2)+'\n')
# Retain the accepted route's textual shape as well as all noncoordinate fields.
source=(B/wave_path).read_text()
for key in ('bounds','spawnBounds','entry','radius'):
 values=iter(z[key] for z in d['zones'])
 source,n=re.subn(r'("'+key+r'":)\[[^\]]+\]',lambda m:m[1]+json.dumps(next(values),separators=(',',':')),source)
 assert n==3,(key,n)
for w in d['zones']:
 for row in w['reservations']:
  pattern=r'("reservationId":"'+re.escape(row['reservationId'])+r'"[^\n]*?"position":)\[[^\]]+\]'
  source,n=re.subn(pattern,lambda m:m[1]+json.dumps(row['position'],separators=(',',':')),source);assert n==1
assert json.loads(source)==d
(R/wave_path).write_text(source)
print('Authored3 horizontal32x8 zones,54 clustered decorations,17 independent blockers,28 stable reservations.')
